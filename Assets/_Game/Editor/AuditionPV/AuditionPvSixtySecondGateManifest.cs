using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    // Evidence validator only. It does not assemble, trim, encode, or edit a movie.
    internal static class AuditionPvSixtySecondGateManifestValidator
    {
        internal const string ManifestSchema =
            "dimension-brawl.audition-pv.preedit-60s-shot-gate.v2";
        internal const string ReportSchema =
            "dimension-brawl.audition-pv.preedit-60s-shot-gate-validation.v4";
        internal const string RightsRecordSchema =
            "dimension-brawl.audition-pv.rights-record.v2";
        internal const string SemanticProofSchema =
            "dimension-brawl.audition-pv.take-semantic-proof.v2";
        internal const string CleanPlateProofSchema =
            "dimension-brawl.audition-pv.clean-plate-companion-proof.v1";
        internal const string AutomatedProofSchema =
            "dimension-brawl.audition-pv.take-automated-proof.v2";
        internal const string AutomatedCheckResultSchema =
            "dimension-brawl.audition-pv.take-automated-check-result.v3";
        internal const string CaptureCoreDigestDomain =
            "dimension-brawl.audition-pv.capture-core.v1";
        internal const string Rec709TransformSchema =
            "dimension-brawl.audition-pv.rec709-transform-config.v3";
        internal const string Rec709OutputLedgerSchema =
            "dimension-brawl.audition-pv.rec709-output-ledger.v2";
        internal const string FrameScanConfigSchema =
            "dimension-brawl.audition-pv.selected-frame-scan-config.v2";
        internal const string FrameScanLedgerSchema =
            "dimension-brawl.audition-pv.selected-frame-scan-ledger.v2";
        internal const string RuntimeWorkloadSchema =
            "dimension-brawl.audition-pv.capture-runtime-workload.v3";
        internal const string SceneNoHudContractSchema =
            "dimension-brawl.audition-pv.scene-no-hud-contract.v1";
        internal const string ShotAuthorshipSchema =
            "dimension-brawl.audition-pv.shot-authorship.v1";
        internal const string TakeReviewSchema =
            "dimension-brawl.audition-pv.take-human-review.v1";
        internal const string VisualReviewSchema =
            "dimension-brawl.audition-pv.visual-review-25pct.v1";
        internal const string TwelveSecondApprovalSchema =
            "dimension-brawl.audition-pv.preedit-12s-approval.v1";
        internal const string AudioGenerationSchema =
            "dimension-brawl.audition-pv.audio-generation.v2";
        internal const string AudioDerivationRecipeSchema =
            "dimension-brawl.audition-pv.audio-derivation-recipe.v1";
        internal const string AudioListeningSchema =
            "dimension-brawl.audition-pv.audio-listening-review.v1";
        internal const string RightsCoverageReviewSchema =
            "dimension-brawl.audition-pv.rights-coverage-review.v1";
        internal const int Width = 2560;
        internal const int Height = 1440;
        internal const int Fps = 60;
        internal const int TotalFrames = 3600;
        internal const int MinimumHandleFrames = 180;
        internal const int MaximumHandleFrames = 300;
        internal const string ColorManagement = "Rec.709";
        internal const string ProductionManifestRoot =
            "D:/DimensionBrawl_PV/02_selects/PREEDIT_60S";
        internal const string ProductionAudioRoot = "D:/DimensionBrawl_PV/01_capture_audio";
        internal const string ProductionLicensesRoot = "D:/DimensionBrawl_PV/99_licenses";
        internal const string ProductionGraphicsRoot =
            "D:/DimensionBrawl_PV/02_selects/PREEDIT_60S/graphics";
        internal const string ProductionReviewRoot =
            "D:/DimensionBrawl_PV/02_selects/PREEDIT_60S/reviews";
        internal const string ProductionManifestFileName = "preedit_60s_shot_gate_manifest.json";
        private const long MaxQhdPngBytes = 32L * 1024L * 1024L;
        private const long MaxSheetPngBytes = 64L * 1024L * 1024L;
        private const long MaxManifestJsonBytes = 16L * 1024L * 1024L;
        private const long MaxEvidenceJsonBytes = 32L * 1024L * 1024L;
        private const long MaxFrameLedgerBytes = 64L * 1024L * 1024L;
        private const long MaxWaveBytes = 512L * 1024L * 1024L;
        private const int MaxFrameLedgerEntries = 100000;
        private const int MaxLedgerLineCharacters = 4096;
        private const long MaxDecodedPixels = 32L * (2560 / 4) * (1440 / 4);
        private const int MaxPreviewCells = 32;
        private const string Rec709TransformId = "srgb8-to-bt709-oetf-rgba8-v1";

        private static readonly string[] AudioCategories =
            { "music", "sfx", "vo", "ambience" };
        private static readonly string[] RequiredAudioCues =
        {
            "music-bed", "city-ambience", "olympus-ambience",
            "gun-mechanical", "gun-fire", "gun-tail",
            "dodge", "summon", "hit", "boss-charge", "boss-fire", "boss-death",
            "wing-deploy", "eye-open", "announcement-vo", "inori-vo", "boss-vo"
        };
        private static readonly string[] RightsScopes =
            { "asset", "font", "audio", "ai" };
        private static readonly string[] AutomatedChecks =
        {
            "contact-sheet", "missing-frame", "error-magenta", "resolution", "rec709",
            "renderer-material-scan"
        };
        private const string HudAbsentCheck = "hud-layer-absent";
        private static readonly HashSet<string> FullRangeScanChecks = new(StringComparer.Ordinal)
            { "error-magenta", "renderer-material-scan", HudAbsentCheck };
        private static readonly byte[] Srgb8ToRec709Lut = BuildSrgb8ToRec709Lut();
        private const string AutomatedTestSuite = "AuditionPvSixtySecondEvidence";
        private static readonly HashSet<string> EditorialHudModes = new(StringComparer.Ordinal)
            { "hud-on", "hud-off", "clean-plate", "mixed", "end-card" };
        private static readonly HashSet<string> CaptureHudModes = new(StringComparer.Ordinal)
            { "hud-on", "hud-off", "clean-plate", "hud-on-to-result", "end-card" };
        private static readonly HashSet<string> SourceKinds = new(StringComparer.Ordinal)
            { "gameplay", "cinematic", "end-card" };
        private static readonly HashSet<string> CoreBeatIds = new(StringComparer.Ordinal)
        {
            "city-hud-gameplay", "perfect-dodge", "summon-chain", "summon-defense",
            "c33-wing-deployment", "c34-eye-open",
            "boss-pattern-1", "boss-pattern-2", "boss-pattern-3",
            "olympus-hud-gameplay", "player-tier3-ultimate", "boss-finisher",
            "boss-collapse", "aftermath"
        };

        internal static readonly AuditionPvSixtySecondRequiredBucket[] RequiredBuckets =
        {
            Bucket("PV_S010", 0, 239, "city-alert-skyline-dimensional-anomaly",
                "City alert, skyline, and dimensional anomaly", "world-and-threat-hook", "city",
                "city-alert", "city-skyline", "dimensional-anomaly"),
            Bucket("PV_S020", 240, 599, "city-hud-gameplay",
                "HUD-on City movement and fire", "prove-real-gameplay", "city",
                "city-movement", "city-fire", "city-hud-gameplay"),
            Bucket("PV_S030", 600, 959, "hit-dodge-summon-chain",
                "Hit, dodge, and summon chain", "prove-core-systems", "city",
                "player-hit", "perfect-dodge", "summon-chain"),
            Bucket("PV_S040", 960, 1199, "dimension-rift-to-olympus",
                "Dimension-rift transition", "spatial-transition", "city",
                "dimension-rift-transition"),
            Bucket("PV_S050", 1200, 1439, "boss-low-angle-silhouette",
                "Boss low angle and silhouette", "boss-pressure", "olympus",
                "boss-low-angle", "boss-silhouette"),
            Bucket("PV_S060", 1440, 1679, "c33-wing-to-c34-eye",
                "C33 wing deployment to C34 eye open", "memory-anchor", "olympus",
                "c33-wing-deployment", "c34-eye-open"),
            Bucket("PV_S070", 1680, 2399, "phase2-three-patterns",
                "Three Phase 2 representative patterns", "boss-density", "olympus",
                "boss-pattern-1", "boss-pattern-2", "boss-pattern-3", "olympus-hud-gameplay"),
            Bucket("PV_S080", 2400, 2999, "dodge-summon-defense-ultimate",
                "Perfect dodge, summon defense, and ultimate", "climax", "olympus",
                "perfect-dodge", "summon-defense", "player-tier3-ultimate"),
            Bucket("PV_S090", 3000, 3299, "boss-finisher-collapse-aftermath",
                "Boss finisher, collapse, and aftermath", "release", "olympus",
                "boss-finisher", "boss-collapse", "aftermath"),
            Bucket("PV_S100", 3300, 3599, "logo-slogan-audition-end-card",
                "Logo, slogan, and audition end card", "close", "end-card",
                "logo", "slogan", "audition-end-card")
        };

        internal static AuditionPvSixtySecondShotGateManifest CreateEmptyPlan()
        {
            return new AuditionPvSixtySecondShotGateManifest
            {
                manifestId = "dimension-brawl-audition-pv-60s-preedit",
                declaredStatus = "gate-open-evidence-missing",
                buckets = RequiredBuckets.Select(value => new AuditionPvSixtySecondSequenceBucket
                {
                    bucketId = value.bucketId,
                    timelineStartFrame = value.referenceStartFrame,
                    timelineEndFrame = value.referenceEndFrame,
                    role = value.role,
                    content = value.content,
                    purpose = value.purpose,
                    requiredBeatIds = value.requiredBeatIds.ToArray()
                }).ToArray()
            };
        }

        // This API validates authorship structure only. It can never open the editing Gate.
        internal static AuditionPvSixtySecondGateValidationReport ValidateStructure(
            AuditionPvSixtySecondShotGateManifest manifest)
        {
            var report = new ReportBuilder();
            ValidateStructureCore(manifest, report);
            int structureErrors = report.ErrorCount;
            report.Warning("STRUCTURE_ONLY_NOT_GATE", "validationMode",
                "Structure-only validation never proves real media or opens the editing Gate.");
            return report.Build(manifest, "structure-only", structureErrors);
        }

        // Only this API can return passed=true. All production proof readers are fail-closed.
        internal static AuditionPvSixtySecondGateValidationReport ValidateProduction(
            AuditionPvSixtySecondShotGateManifest manifest,
            AuditionPvSixtySecondValidationContext context)
        {
            return ValidateProductionCore(manifest, context, authoritativeFile: false,
                inputManifestPath: string.Empty, inputManifestSha256: string.Empty);
        }

        private static AuditionPvSixtySecondGateValidationReport ValidateProductionCore(
            AuditionPvSixtySecondShotGateManifest manifest,
            AuditionPvSixtySecondValidationContext context, bool authoritativeFile,
            string inputManifestPath, string inputManifestSha256)
        {
            var report = new ReportBuilder();
            ValidateStructureCore(manifest, report);
            int structureErrors = report.ErrorCount;
            context ??= new AuditionPvSixtySecondValidationContext();
            ValidateProductionContext(context, report);
            if (manifest != null)
            {
                ValidateProductionHeader(manifest, context, report);
                Dictionary<string, AuditionPvSixtySecondRightsEvidence> rights =
                    IndexRights(manifest.rights);
                Dictionary<string, AuditionPvSixtySecondUsedItem> items =
                    IndexItems(manifest.usedItems);
                Dictionary<string, AuditionPvSixtySecondAudioEvidence> audio =
                    IndexAudio(manifest.audio);
                ValidateRightsProduction(manifest, rights, items, context, report);
                ValidateUsedItemsProduction(items, context, report);
                ValidateAudioProduction(audio, items, context, report);
                ValidateBucketsProduction(manifest, audio, items, context, report);
                ValidateGateEvidenceProduction(manifest, context, report);
            }
            if (authoritativeFile)
                ValidateAuthoritativeFinalSnapshot(context, report);
            if (!authoritativeFile)
                report.Warning(string.IsNullOrWhiteSpace(inputManifestPath)
                        ? "IN_MEMORY_MANIFEST_NOT_AUTHORITATIVE"
                        : "CALLER_CONTEXT_NOT_AUTHORITATIVE",
                    string.IsNullOrWhiteSpace(inputManifestPath) ? "manifest" : "context",
                    "Only the installed-project ValidateProductionFile entry can produce a Gate PASS.");
            return report.Build(manifest, "production", structureErrors, authoritativeFile,
                inputManifestPath, inputManifestSha256);
        }

        // Authoritative entry: roots and product HEAD come from the installed project, never a fixture.
        internal static AuditionPvSixtySecondGateValidationReport ValidateProductionFile(string path) =>
            ValidateProductionFileCore(path, CreateInstalledProductionContext(), authoritativeFile: true);

        // Test/evaluator seam. Caller-supplied roots can inspect production evidence but can never PASS.
        internal static AuditionPvSixtySecondGateValidationReport ValidateProductionFile(
            string path, AuditionPvSixtySecondValidationContext context) =>
            ValidateProductionFileCore(path, context, authoritativeFile: false);

        // Hermetic evidence-reader seam: exercises the acyclic core/result and physical PNG
        // relationship without pretending a miniature fixture is the installed 60-second Gate.
        // It is intentionally incapable of passed=true.
        internal static AuditionPvSixtySecondGateValidationReport
            ValidateHermeticAcyclicEvidenceReaderFixture(AuditionPvCaptureManifest capture,
                string resultPath, string sourcePath, string outputPath)
        {
            var report = new ReportBuilder();
            string resultSha256 = string.Empty;
            try
            {
                if (capture == null || string.IsNullOrWhiteSpace(capture.outputDirectory))
                    throw new InvalidDataException("Capture/output identity is missing.");
                resultPath = Path.GetFullPath(resultPath);
                sourcePath = Path.GetFullPath(sourcePath);
                outputPath = Path.GetFullPath(outputPath);
                RequireUnder(resultPath, new[] { capture.outputDirectory }, "hermetic result");
                RequireUnder(sourcePath, new[] { capture.outputDirectory }, "hermetic source");
                RequireUnder(outputPath, new[] { capture.outputDirectory }, "hermetic output");
                RejectReparseChain(resultPath);
                RejectReparseChain(sourcePath);
                RejectReparseChain(outputPath);
                byte[] resultBytes = ReadAllBytesCapped(resultPath, MaxEvidenceJsonBytes,
                    "Hermetic result JSON");
                resultSha256 = ByteSha256(resultBytes);
                var result = JsonUtility.FromJson<AuditionPvAutomatedCheckResultArtifact>(
                    new UTF8Encoding(false, true).GetString(resultBytes));
                string core = CaptureCoreSha256(capture);
                if (result == null || result.schemaVersion != AutomatedCheckResultSchema ||
                    result.id != "rec709" || result.captureId != capture.captureId ||
                    result.sourceCaptureCoreSha256 != core ||
                    !CaptureTestArtifactMatches(capture, AutomatedTestSuite, "rec709",
                        resultPath, resultSha256) || result.measuredWidth <= 0 ||
                    result.measuredHeight <= 0 || result.sourceMediaArtifact == null ||
                    result.outputMediaArtifact == null ||
                    !PathsEqual(result.sourceMediaArtifact.path, sourcePath) ||
                    !PathsEqual(result.outputMediaArtifact.path, outputPath) ||
                    result.sourceMediaArtifact.sha256 != AuditionPvSha256.FileHash(sourcePath) ||
                    result.outputMediaArtifact.sha256 != AuditionPvSha256.FileHash(outputPath) ||
                    !DecodedMagentaCountMatches(sourcePath, result.measuredWidth,
                        result.measuredHeight, result.detectedPixelCount) ||
                    !DecodedRec709TransformMatches(sourcePath, outputPath,
                        result.measuredWidth, result.measuredHeight))
                    report.Error("HERMETIC_ACYCLIC_EVIDENCE_INVALID", "fixture",
                        "Core/result pins or physical decoded media relation failed.");
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error("HERMETIC_ACYCLIC_EVIDENCE_INVALID", "fixture", exception.Message);
            }
            report.Warning("CALLER_CONTEXT_NOT_AUTHORITATIVE", "context",
                "A hermetic reader fixture can verify evidence mechanics but can never open the Gate.");
            return report.Build(null, "production", 0, false, resultPath ?? string.Empty,
                resultSha256);
        }

        private static AuditionPvSixtySecondGateValidationReport ValidateProductionFileCore(
            string path, AuditionPvSixtySecondValidationContext context, bool authoritativeFile)
        {
            try
            {
                string resolved = ResolveEvidencePath(path, context);
                if (authoritativeFile && !PathsEqual(resolved,
                        Path.Combine(ProductionManifestRoot, ProductionManifestFileName)))
                {
                    var nonCanonical = new ReportBuilder();
                    nonCanonical.Error("MANIFEST_CANONICAL_PATH_INVALID", "manifest",
                        "Authoritative Gate input must be the direct canonical PREEDIT_60S manifest.");
                    return nonCanonical.Build(null, "production", 1, false, resolved, string.Empty);
                }
                if (!File.Exists(resolved))
                {
                    var missing = new ReportBuilder();
                    missing.Error("MANIFEST_FILE_MISSING", "manifest", resolved);
                    return missing.Build(null, "production", 1, false, resolved, string.Empty);
                }
                byte[] manifestBytes = ReadAllBytesCapped(resolved, MaxManifestJsonBytes,
                    "60-second manifest JSON");
                string manifestSha256 = ByteSha256(manifestBytes);
                RememberFinalFile(context, resolved, manifestSha256, manifestBytes.LongLength,
                    null, "manifest");
                string manifestJson = new UTF8Encoding(false, true).GetString(manifestBytes);
                if (manifestJson.Length > 0 && manifestJson[0] == '\ufeff')
                    manifestJson = manifestJson.Substring(1);
                AuditionPvSixtySecondShotGateManifest manifest =
                    JsonUtility.FromJson<AuditionPvSixtySecondShotGateManifest>(
                        manifestJson);
                return ValidateProductionCore(manifest, context, authoritativeFile,
                    inputManifestPath: resolved, inputManifestSha256: manifestSha256);
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                var failed = new ReportBuilder();
                failed.Error("MANIFEST_READ_FAILED", "manifest", exception.Message);
                return failed.Build(null, "production", 1, false, path ?? string.Empty, string.Empty);
            }
        }

        private static AuditionPvSixtySecondValidationContext CreateInstalledProductionContext()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            return new AuditionPvSixtySecondValidationContext
            {
                projectRoot = projectRoot,
                currentGitCommitSha = git.probeSucceeded ? git.commitSha : string.Empty,
                allowedEvidenceRoots = new[]
                {
                    projectRoot, AuditionPvCaptureContract.OutputRoot,
                    AuditionPvTwelveSecondGoldAssembler.OutputRoot, ProductionManifestRoot,
                    ProductionAudioRoot, ProductionLicensesRoot, ProductionGraphicsRoot,
                    ProductionReviewRoot
                },
                allowedCaptureRoots = new[] { AuditionPvCaptureContract.OutputRoot },
                allowedSelectRoots = new[] { AuditionPvTwelveSecondGoldAssembler.OutputRoot },
                allowedAudioRoots = new[] { ProductionAudioRoot },
                allowedLicenseRoots = new[] { ProductionLicensesRoot },
                allowedGraphicsRoots = new[] { ProductionGraphicsRoot },
                allowedReviewRoots = new[] { ProductionReviewRoot },
                currentGitClean = git.probeSucceeded && !git.isDirty
            };
        }

        private static void ValidateStructureCore(
            AuditionPvSixtySecondShotGateManifest manifest, ReportBuilder report)
        {
            if (manifest == null)
            {
                report.Error("MANIFEST_NULL", "manifest", "Manifest is null.");
                return;
            }
            ValidateHeaderStructure(manifest, report);
            Dictionary<string, AuditionPvSixtySecondRightsEvidence> rights =
                ValidateRightsStructure(manifest.rights, report);
            Dictionary<string, AuditionPvSixtySecondUsedItem> items =
                ValidateUsedItemsStructure(manifest.usedItems, rights, report);
            var referencedItems = new HashSet<string>(StringComparer.Ordinal);
            bool generatedAi;
            Dictionary<string, AuditionPvSixtySecondAudioEvidence> audio =
                ValidateAudioStructure(manifest.audio, items, referencedItems, report, out generatedAi);
            ValidateBucketsStructure(manifest.buckets, audio, items, referencedItems, report);
            foreach (string id in items.Keys.Where(id => !referencedItems.Contains(id)))
                report.Error("USED_ITEM_ORPHANED", "usedItems", id);
            foreach (string scope in new[] { "asset", "font", "audio" })
                if (!items.Values.Any(value => value != null && value.scope == scope))
                    report.Error("USED_ITEM_SCOPE_MISSING", "usedItems", scope);
            if (generatedAi && !items.Values.Any(value => value != null && value.scope == "ai"))
                report.Error("USED_ITEM_SCOPE_MISSING", "usedItems", "ai");
            ValidateGateEvidenceStructure(manifest.gateEvidence, report);
        }

        private static void ValidateHeaderStructure(
            AuditionPvSixtySecondShotGateManifest manifest, ReportBuilder report)
        {
            if (manifest.schemaVersion != ManifestSchema)
                report.Error("MANIFEST_SCHEMA_INVALID", "schemaVersion", "Unsupported schema.");
            if (string.IsNullOrWhiteSpace(manifest.manifestId))
                report.Error("MANIFEST_ID_MISSING", "manifestId", "Stable manifest ID required.");
            if (manifest.width != Width || manifest.height != Height || manifest.fps != Fps ||
                manifest.totalFrames != TotalFrames || manifest.colorManagement != ColorManagement)
                report.Error("MANIFEST_FORMAT_INVALID", "format",
                    "Required contract is 2560x1440, 60fps, 3600 frames, Rec.709.");
            if (!string.IsNullOrWhiteSpace(manifest.productCheckpointGitSha) &&
                !IsFullGitSha(manifest.productCheckpointGitSha))
                report.Error("PRODUCT_CHECKPOINT_GIT_INVALID", "productCheckpointGitSha",
                    "A product checkpoint must be a full lowercase Git SHA.");
        }

        private static void ValidateProductionContext(
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            if (string.IsNullOrWhiteSpace(context.projectRoot) ||
                !Directory.Exists(context.projectRoot))
                report.Error("CONTEXT_PROJECT_ROOT_INVALID", "context.projectRoot",
                    "An existing current Unity project root is required.");
            if (!IsFullGitSha(context.currentGitCommitSha))
                report.Error("CONTEXT_CURRENT_GIT_INVALID", "context.currentGitCommitSha",
                    "A full current Git SHA is required.");
            if (!context.currentGitClean)
                report.Error("CONTEXT_CURRENT_GIT_DIRTY", "context.currentGitClean",
                    "Authoritative production validation requires a successfully probed clean worktree.");
            if ((context.allowedCaptureRoots ?? Array.Empty<string>()).Length == 0)
                report.Error("CONTEXT_CAPTURE_ROOTS_MISSING", "context.allowedCaptureRoots",
                    "At least one explicit capture evidence root is required.");
            if ((context.allowedSelectRoots ?? Array.Empty<string>()).Length == 0)
                report.Error("CONTEXT_SELECT_ROOTS_MISSING", "context.allowedSelectRoots",
                    "At least one explicit 12-second select root is required.");
            if ((context.allowedAudioRoots ?? Array.Empty<string>()).Length == 0 ||
                (context.allowedLicenseRoots ?? Array.Empty<string>()).Length == 0 ||
                (context.allowedGraphicsRoots ?? Array.Empty<string>()).Length == 0 ||
                (context.allowedReviewRoots ?? Array.Empty<string>()).Length == 0)
                report.Error("CONTEXT_CATEGORY_ROOTS_MISSING", "context",
                    "Explicit audio, license, graphics, and review roots are required.");
        }

        private static void ValidateProductionHeader(
            AuditionPvSixtySecondShotGateManifest manifest,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            if (manifest.declaredStatus != "ready-for-editing")
                report.Error("MANIFEST_STATUS_NOT_READY", "declaredStatus",
                    "Production Gate validation requires ready-for-editing.");
            if (!IsFullGitSha(manifest.productCheckpointGitSha))
                report.Error("PRODUCT_CHECKPOINT_GIT_REQUIRED", "productCheckpointGitSha",
                    "Production Gate validation requires a full product checkpoint SHA.");
            else if (IsFullGitSha(context.currentGitCommitSha) &&
                     manifest.productCheckpointGitSha != context.currentGitCommitSha)
                report.Error("PRODUCT_CHECKPOINT_NOT_CURRENT", "productCheckpointGitSha",
                    "The manifest checkpoint is not the current product HEAD.");
        }

        private static Dictionary<string, AuditionPvSixtySecondRightsEvidence>
            ValidateRightsStructure(AuditionPvSixtySecondRightsEvidence[] values, ReportBuilder report)
        {
            if ((values?.Length ?? 0) > 512)
                report.Error("RIGHTS_CARDINALITY_EXCEEDED", "rights", "At most 512 rights rows are accepted.");
            var result = new Dictionary<string, AuditionPvSixtySecondRightsEvidence>(StringComparer.Ordinal);
            foreach ((AuditionPvSixtySecondRightsEvidence value, int index) in
                     (values ?? Array.Empty<AuditionPvSixtySecondRightsEvidence>())
                     .Select((value, index) => (value, index)))
            {
                string at = $"rights[{index}]";
                if (value == null || string.IsNullOrWhiteSpace(value.id) || result.ContainsKey(value.id))
                {
                    report.Error("RIGHTS_ID_INVALID", at, "Rights IDs must be non-empty and unique.");
                    continue;
                }
                if (!RightsScopes.Contains(value.scope, StringComparer.Ordinal))
                    report.Error("RIGHTS_SCOPE_INVALID", at, value.scope ?? "<null>");
                PinShape(value.record, "RIGHTS_RECORD", at, report);
                result.Add(value.id, value);
            }
            return result;
        }

        private static Dictionary<string, AuditionPvSixtySecondUsedItem>
            ValidateUsedItemsStructure(AuditionPvSixtySecondUsedItem[] values,
                IReadOnlyDictionary<string, AuditionPvSixtySecondRightsEvidence> rights,
                ReportBuilder report)
        {
            if ((values?.Length ?? 0) > 512)
                report.Error("USED_ITEM_CARDINALITY_EXCEEDED", "usedItems",
                    "At most 512 used-item rows are accepted.");
            var result = new Dictionary<string, AuditionPvSixtySecondUsedItem>(StringComparer.Ordinal);
            foreach ((AuditionPvSixtySecondUsedItem value, int index) in
                     (values ?? Array.Empty<AuditionPvSixtySecondUsedItem>())
                     .Select((value, index) => (value, index)))
            {
                string at = $"usedItems[{index}]";
                if (value == null || string.IsNullOrWhiteSpace(value.id) || result.ContainsKey(value.id))
                {
                    report.Error("USED_ITEM_ID_INVALID", at, "Used-item IDs must be non-empty and unique.");
                    continue;
                }
                if (!RightsScopes.Contains(value.scope, StringComparer.Ordinal))
                    report.Error("USED_ITEM_SCOPE_INVALID", at, value.scope ?? "<null>");
                if (string.IsNullOrWhiteSpace(value.sourceLocator))
                    report.Error("USED_ITEM_SOURCE_MISSING", at, value.id);
                if (value.dependencyBinding != "unity-dependency" &&
                    value.dependencyBinding != "external-artifact")
                    report.Error("USED_ITEM_DEPENDENCY_BINDING_INVALID", at,
                        value.dependencyBinding ?? "<null>");
                if (value.dependencyBinding == "unity-dependency" &&
                    Normalize(value.artifact?.path) != Normalize(value.sourceLocator))
                    report.Error("USED_ITEM_DEPENDENCY_ARTIFACT_PATH_MISMATCH", at,
                        "A Unity-bound used item must pin its exact dependency path.");
                if (!rights.TryGetValue(value.rightsRecordId ?? string.Empty, out var right) ||
                    right == null || right.scope != value.scope)
                    report.Error("USED_ITEM_RIGHTS_REF_INVALID", at, value.rightsRecordId ?? "<null>");
                PinShape(value.artifact, "USED_ITEM_ARTIFACT", at, report);
                result.Add(value.id, value);
            }
            foreach (string rightId in rights.Keys.Where(id =>
                         !result.Values.Any(item => item != null && item.rightsRecordId == id)))
                report.Error("RIGHTS_RECORD_UNUSED", "rights", rightId);
            return result;
        }

        private static Dictionary<string, AuditionPvSixtySecondAudioEvidence>
            ValidateAudioStructure(AuditionPvSixtySecondAudioEvidence[] values,
                IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
                ISet<string> referencedItems, ReportBuilder report, out bool generatedAi)
        {
            if ((values?.Length ?? 0) > 128)
                report.Error("AUDIO_CARDINALITY_EXCEEDED", "audio", "At most 128 audio rows are accepted.");
            generatedAi = false;
            var result = new Dictionary<string, AuditionPvSixtySecondAudioEvidence>(StringComparer.Ordinal);
            var categories = new HashSet<string>(StringComparer.Ordinal);
            var cues = new HashSet<string>(StringComparer.Ordinal);
            foreach ((AuditionPvSixtySecondAudioEvidence value, int index) in
                     (values ?? Array.Empty<AuditionPvSixtySecondAudioEvidence>())
                     .Select((value, index) => (value, index)))
            {
                string at = $"audio[{index}]";
                if (value == null || string.IsNullOrWhiteSpace(value.id) || result.ContainsKey(value.id))
                {
                    report.Error("AUDIO_ID_INVALID", at, "Audio IDs must be non-empty and unique.");
                    continue;
                }
                if (!AudioCategories.Contains(value.category, StringComparer.Ordinal))
                    report.Error("AUDIO_CATEGORY_INVALID", at, value.category ?? "<null>");
                else categories.Add(value.category);
                string[] audioCues = value.cueIds ?? Array.Empty<string>();
                if (audioCues.Length == 0 ||
                    audioCues.Distinct(StringComparer.Ordinal).Count() != audioCues.Length)
                    report.Error("AUDIO_CUE_SET_INVALID", at, "Each stem needs unique required cue IDs.");
                foreach (string cue in audioCues)
                {
                    if (!RequiredAudioCues.Contains(cue, StringComparer.Ordinal) ||
                        CueCategory(cue) != value.category || !cues.Add(cue ?? string.Empty))
                        report.Error("AUDIO_CUE_INVALID_OR_DUPLICATE", at, cue ?? "<null>");
                }
                AuditionPvAudioCueRegion[] cueRegions = value.cueRegions ??
                    Array.Empty<AuditionPvAudioCueRegion>();
                if (cueRegions.Length > 256)
                    report.Error("AUDIO_CUE_REGION_CARDINALITY_EXCEEDED", at,
                        "At most 256 cue regions are accepted per stem.");
                string[] regionIds = cueRegions.Where(region => region != null)
                    .Select(region => region.cueId).OrderBy(id => id, StringComparer.Ordinal).ToArray();
                if (cueRegions.Any(region => !CueRegionShapeValid(region)) ||
                    !regionIds.SequenceEqual(audioCues.OrderBy(id => id, StringComparer.Ordinal),
                        StringComparer.Ordinal))
                    report.Error("AUDIO_CUE_REGION_INVALID", at,
                        "Each declared cue needs one positive, in-bounds WAV marker region; overlaps are allowed.");
                PinShape(value.file, "AUDIO_FILE", at, report);
                if (value.sampleRate != 48000 || value.channels < 1 || value.channels > 2)
                    report.Error("AUDIO_FORMAT_INVALID", at, "Audio must declare 48kHz mono/stereo.");
                if (!TryItem(value.usedItemId, "audio", items, referencedItems))
                    report.Error("AUDIO_USED_ITEM_INVALID", at, value.usedItemId ?? "<null>");
                if (value.generatedByAi)
                {
                    generatedAi = true;
                    if (!TryItem(value.aiUsedItemId, "ai", items, referencedItems))
                        report.Error("AUDIO_AI_USED_ITEM_INVALID", at, value.aiUsedItemId ?? "<null>");
                    PinShape(value.generationManifest, "AUDIO_GENERATION", at, report);
                }
                if (!new[] { "pending", "approved", "rejected" }.Contains(
                        value.humanListeningStatus, StringComparer.Ordinal))
                    report.Error("AUDIO_LISTENING_STATUS_INVALID", at,
                        "Listening approval is tracked separately from the raw-material Gate.");
                if (value.humanListeningStatus == "approved" || value.humanListeningStatus == "rejected")
                    PinShape(value.listeningReport, "AUDIO_LISTENING_REPORT", at, report);
                result.Add(value.id, value);
            }
            foreach (string category in AudioCategories.Where(category => !categories.Contains(category)))
                report.Error("AUDIO_CATEGORY_EVIDENCE_MISSING", "audio", category);
            foreach (string cue in RequiredAudioCues.Where(cue => !cues.Contains(cue)))
                report.Error("AUDIO_REQUIRED_CUE_MISSING", "audio", cue);
            return result;
        }

        private static void ValidateBucketsStructure(
            AuditionPvSixtySecondSequenceBucket[] buckets,
            IReadOnlyDictionary<string, AuditionPvSixtySecondAudioEvidence> audio,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            ISet<string> referencedItems, ReportBuilder report)
        {
            buckets ??= Array.Empty<AuditionPvSixtySecondSequenceBucket>();
            if (buckets.Length != RequiredBuckets.Length)
                report.Error("BUCKET_COUNT_INVALID", "buckets", "Exactly ten ordered sequence buckets are required.");
            var shotIds = new HashSet<string>(StringComparer.Ordinal);
            var takeIds = new HashSet<string>(StringComparer.Ordinal);
            bool hasHudOn = false, hasHudOff = false, hasCleanPlate = false;
            bool cityContinuous = false, olympusContinuous = false;
            int count = Math.Min(buckets.Length, RequiredBuckets.Length);
            for (int index = 0; index < count; index++)
            {
                AuditionPvSixtySecondSequenceBucket bucket = buckets[index];
                AuditionPvSixtySecondRequiredBucket required = RequiredBuckets[index];
                string at = $"buckets[{index}]";
                if (bucket == null)
                {
                    report.Error("BUCKET_NULL", at, required.bucketId);
                    continue;
                }
                if (bucket.bucketId != required.bucketId || bucket.role != required.role ||
                    !(bucket.requiredBeatIds ?? Array.Empty<string>()).SequenceEqual(
                        required.requiredBeatIds, StringComparer.Ordinal))
                    report.Error("BUCKET_CONTRACT_MISMATCH", at, required.bucketId);
                if (string.IsNullOrWhiteSpace(bucket.content) || string.IsNullOrWhiteSpace(bucket.purpose))
                    report.Error("BUCKET_EDITORIAL_DESCRIPTION_MISSING", at,
                        "Content and purpose remain authored, non-empty editorial descriptions.");
                bool bucketTimeline = bucket.timelineStartFrame >= 0 &&
                    bucket.timelineEndFrame >= bucket.timelineStartFrame &&
                    (index != 0 || bucket.timelineStartFrame == 0) &&
                    (index == 0 || buckets[index - 1] != null &&
                        bucket.timelineStartFrame == buckets[index - 1].timelineEndFrame + 1) &&
                    (index != RequiredBuckets.Length - 1 || bucket.timelineEndFrame == TotalFrames - 1);
                if (!bucketTimeline)
                    report.Error("BUCKET_TIMELINE_NOT_CONTIGUOUS_60S", at,
                        "Authored bucket boundaries must cover f0..f3599 without gaps or overlap.");
                AuditionPvSixtySecondAtomicShot[] shots = bucket.shots ??
                    Array.Empty<AuditionPvSixtySecondAtomicShot>();
                if (shots.Length > 32)
                    report.Error("BUCKET_SHOT_CARDINALITY_EXCEEDED", at,
                        "At most 32 atomic shots are accepted per bucket.");
                if (shots.Length == 0)
                    report.Error("BUCKET_SHOTS_MISSING", at, "A sequence bucket must contain actual shots.");
                var covered = new HashSet<string>(StringComparer.Ordinal);
                for (int shotIndex = 0; shotIndex < shots.Length; shotIndex++)
                {
                    AuditionPvSixtySecondAtomicShot shot = shots[shotIndex];
                    string shotAt = $"{at}.shots[{shotIndex}]";
                    if (shot == null)
                    {
                        report.Error("SHOT_NULL", shotAt, "null");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(shot.shotId) || !shotIds.Add(shot.shotId))
                        report.Error("SHOT_ID_INVALID", shotAt, "Shot IDs must be globally unique.");
                    bool timeline = shot.timelineStartFrame >= bucket.timelineStartFrame &&
                        shot.timelineEndFrame >= shot.timelineStartFrame &&
                        (shotIndex != 0 || shot.timelineStartFrame == bucket.timelineStartFrame) &&
                        (shotIndex == 0 || shots[shotIndex - 1] != null &&
                            shot.timelineStartFrame == shots[shotIndex - 1].timelineEndFrame + 1) &&
                        (shotIndex != shots.Length - 1 || shot.timelineEndFrame == bucket.timelineEndFrame);
                    if (!timeline)
                        report.Error("SHOT_TIMELINE_NOT_CONTIGUOUS_IN_BUCKET", shotAt,
                            "Atomic shots must exactly cover their sequence bucket.");
                    bool endCard = shot.sourceKind == "end-card";
                    if (!endCard && (string.IsNullOrWhiteSpace(shot.scenePath) ||
                        string.IsNullOrWhiteSpace(shot.cameraId) ||
                        string.IsNullOrWhiteSpace(shot.gameplayState) ||
                        string.IsNullOrWhiteSpace(shot.timelineId) || shot.deterministicSeed < 0 ||
                        !EditorialHudModes.Contains(shot.editorialHudMode) ||
                        !SourceKinds.Contains(shot.sourceKind)))
                        report.Error("SHOT_DIRECTOR_METADATA_MISSING", shotAt,
                            "Scene/state/seed/camera/Timeline/HUD/source-kind metadata is required.");
                    if (endCard && (shot.editorialHudMode != "end-card" ||
                        shot.graphicSourceId != "layout-placeholder" ||
                        shot.graphicProductionStatus != "layout-placeholder-approved" ||
                        !new[] { "pending-approval", "approved" }.Contains(
                            shot.sloganApprovalStatus, StringComparer.Ordinal) ||
                        !new[] { "pending-approval", "approved" }.Contains(
                            shot.auditionNoticeApprovalStatus, StringComparer.Ordinal)))
                        report.Error("SHOT_END_CARD_GRAPHIC_ID_INVALID", shotAt,
                            "End card needs an exact rights-pinned layout-placeholder plan and explicit wording approval states.");
                    if (endCard) PinShape(shot.graphicArtifact, "SHOT_END_CARD_GRAPHIC", shotAt, report);
                    if (required.sceneGroup == "city" &&
                        shot.scenePath != AuditionPvCityHeroPocketCapture.CityScenePath)
                        report.Error("SHOT_CITY_SCENE_INVALID", shotAt, shot.scenePath ?? "<null>");
                    if (required.sceneGroup == "olympus" &&
                        shot.scenePath != AuditionPvStationPhase2PatternRelayCapture.StationScenePath)
                        report.Error("SHOT_OLYMPUS_SCENE_INVALID", shotAt, shot.scenePath ?? "<null>");
                    if (required.sceneGroup == "end-card" && shot.sourceKind != "end-card")
                        report.Error("SHOT_END_CARD_KIND_INVALID", shotAt, shot.sourceKind ?? "<null>");
                    string[] beats = shot.beatIds ?? Array.Empty<string>();
                    if (beats.Length == 0 || beats.Distinct(StringComparer.Ordinal).Count() != beats.Length ||
                        beats.Any(beat => !required.requiredBeatIds.Contains(beat, StringComparer.Ordinal)))
                        report.Error("SHOT_BEAT_MAPPING_INVALID", shotAt, "Shot beats must be unique bucket-required beats.");
                    foreach (string beat in beats)
                    {
                        if (!covered.Add(beat))
                            report.Error("BUCKET_BEAT_MAPPED_MORE_THAN_ONCE", shotAt, beat);
                    }
                    if (beats.Any(CoreBeatIds.Contains) && !shot.coreShot)
                        report.Error("SHOT_CORE_CLASSIFICATION_INVALID", shotAt,
                            "A mandatory anchor beat cannot be downgraded to non-core.");
                    Refs(shot.audioRefIds, audio.Keys, "SHOT_AUDIO_REF", shotAt, report);
                    Refs(shot.usedItemIds, items.Keys, "SHOT_USED_ITEM_REF", shotAt, report);
                    bool endCardGraphicItem = (shot.usedItemIds ?? Array.Empty<string>()).Any(id =>
                        items.TryGetValue(id ?? string.Empty, out var item) && item != null &&
                        item.scope == "asset" && item.artifact?.path == shot.graphicArtifact?.path &&
                        item.artifact?.sha256 == shot.graphicArtifact?.sha256);
                    bool endCardFontItem = (shot.usedItemIds ?? Array.Empty<string>()).Any(id =>
                        items.TryGetValue(id ?? string.Empty, out var item) && item?.scope == "font");
                    if (endCard && (!endCardGraphicItem || !endCardFontItem))
                        report.Error("SHOT_END_CARD_GRAPHIC_RIGHTS_ITEM_MISSING", shotAt,
                            "The exact logo/source placeholder and planned font must be rights-covered used items.");
                    foreach (string item in shot.usedItemIds ?? Array.Empty<string>())
                        if (items.ContainsKey(item ?? string.Empty)) referencedItems.Add(item);
                    if (!endCard) ValidateCandidatesStructure(shot, takeIds, report, shotAt);
                    else if ((shot.candidateTakes ?? Array.Empty<AuditionPvSixtySecondTakeCandidate>()).Length != 0 ||
                             !string.IsNullOrWhiteSpace(shot.approvedTakeId) ||
                             !string.IsNullOrWhiteSpace(shot.cleanPlateTakeId))
                        report.Error("SHOT_END_CARD_CAPTURE_FIELDS_FORBIDDEN", shotAt,
                            "End-card graphics do not use capture candidates or source handles.");
                    AuditionPvSixtySecondTakeCandidate[] candidates = shot.candidateTakes ??
                        Array.Empty<AuditionPvSixtySecondTakeCandidate>();
                    hasHudOn |= candidates.Any(take => take != null && take.declaredHudMode == "hud-on");
                    hasHudOff |= candidates.Any(take => take != null && take.declaredHudMode == "hud-off");
                    bool usableClean = bucket.bucketId != "PV_S100" &&
                        (shot.sourceKind == "gameplay" || shot.sourceKind == "cinematic") &&
                        !string.IsNullOrWhiteSpace(shot.cleanPlateTakeId) &&
                        candidates.Count(take => take != null && take.takeId == shot.cleanPlateTakeId &&
                            take.declaredHudMode == "clean-plate") == 1;
                    hasCleanPlate |= usableClean;
                    int frames = shot.timelineEndFrame - shot.timelineStartFrame + 1;
                    cityContinuous |= required.sceneGroup == "city" &&
                        shot.sourceKind == "gameplay" &&
                        beats.Contains("city-hud-gameplay", StringComparer.Ordinal) &&
                        ApprovedHudOnCandidate(shot) && frames >= 300;
                    olympusContinuous |= required.sceneGroup == "olympus" &&
                        shot.sourceKind == "gameplay" &&
                        beats.Contains("olympus-hud-gameplay", StringComparer.Ordinal) &&
                        ApprovedHudOnCandidate(shot) && frames >= 300;
                }
                foreach (string beat in required.requiredBeatIds.Where(beat => !covered.Contains(beat)))
                    report.Error("BUCKET_REQUIRED_BEAT_SHOT_MISSING", at, beat);
            }
            if (!hasHudOn) report.Error("CAPTURE_HUD_MODE_EVIDENCE_MISSING", "buckets", "hud-on");
            if (!hasHudOff) report.Error("CAPTURE_HUD_MODE_EVIDENCE_MISSING", "buckets", "hud-off");
            if (!hasCleanPlate) report.Error("GAMEPLAY_CINEMATIC_CLEAN_PLATE_MISSING", "buckets",
                "An end card cannot satisfy the clean-plate requirement.");
            if (!cityContinuous) report.Error("CITY_CONTINUOUS_HUD_GAMEPLAY_MISSING", "buckets", "At least 5 seconds");
            if (!olympusContinuous) report.Error("OLYMPUS_CONTINUOUS_HUD_GAMEPLAY_MISSING", "buckets", "At least 5 seconds");
        }

        private static bool ApprovedHudOnCandidate(AuditionPvSixtySecondAtomicShot shot) =>
            !string.IsNullOrWhiteSpace(shot?.approvedTakeId) &&
            (shot.candidateTakes ?? Array.Empty<AuditionPvSixtySecondTakeCandidate>())
            .Count(take => take != null && take.takeId == shot.approvedTakeId &&
                take.declaredHudMode == "hud-on") == 1;

        private static void ValidateCandidatesStructure(AuditionPvSixtySecondAtomicShot shot,
            ISet<string> allTakeIds, ReportBuilder report, string shotAt)
        {
            AuditionPvSixtySecondTakeCandidate[] takes = shot.candidateTakes ??
                Array.Empty<AuditionPvSixtySecondTakeCandidate>();
            if (takes.Length > 8)
                report.Error("SHOT_TAKE_CARDINALITY_EXCEEDED", shotAt,
                    "At most eight candidates are accepted per atomic shot.");
            int requiredCount = shot.coreShot ? 3 : 1;
            var editorialIdentities = new HashSet<string>(StringComparer.Ordinal);
            int approvedMatches = 0;
            int cleanPlateMatches = 0;
            foreach ((AuditionPvSixtySecondTakeCandidate take, int index) in
                     takes.Select((value, index) => (value, index)))
            {
                string at = $"{shotAt}.candidateTakes[{index}]";
                if (take == null)
                {
                    report.Error("TAKE_NULL", at, "null");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(take.takeId) || !allTakeIds.Add(take.takeId))
                    report.Error("TAKE_ID_INVALID", at, "Take IDs must be globally unique.");
                if (take.takeId == shot.approvedTakeId && take.declaredHudMode != "clean-plate")
                    approvedMatches++;
                if (take.takeId == shot.cleanPlateTakeId && take.declaredHudMode == "clean-plate")
                    cleanPlateMatches++;
                if (string.IsNullOrWhiteSpace(take.sourceCaptureId) ||
                    string.IsNullOrWhiteSpace(take.sourceShotId) || !IsFullGitSha(take.gitCommitSha))
                    report.Error("TAKE_SOURCE_IDENTITY_MISSING", at,
                        "Capture ID, source shot ID, and full Git SHA are required.");
                if (!CaptureHudModes.Contains(take.declaredHudMode ?? string.Empty))
                    report.Error("TAKE_DECLARED_HUD_INVALID", at, take.declaredHudMode ?? "<null>");
                if (take.takeId == shot.approvedTakeId && shot.editorialHudMode != "mixed" &&
                    shot.editorialHudMode != "end-card" &&
                    take.declaredHudMode != shot.editorialHudMode)
                    report.Error("TAKE_APPROVED_HUD_MISMATCH", at, take.declaredHudMode ?? "<null>");
                PinShape(take.sourceManifest, "TAKE_SOURCE_MANIFEST", at, report);
                if (!AuditionPvSha256.IsSha256(take.sourceCaptureCoreSha256))
                    report.Error("TAKE_CAPTURE_CORE_IDENTITY_MISSING", at,
                        "The immutable, test-result-free capture-core SHA-256 is required.");
                else if (take.declaredHudMode != "clean-plate" &&
                         !editorialIdentities.Add(string.Join("\0",
                             take.sourceCaptureCoreSha256, take.sourceCaptureId ?? string.Empty)))
                    report.Error("SHOT_CAPTURE_CANDIDATES_NOT_DISTINCT", at,
                        "Source-shot/range/full-manifest aliases of one capture invocation count once.");
                if (!AuditionPvSha256.IsSha256(take.sourceDependencyIdentitySha256))
                    report.Error("TAKE_DEPENDENCY_IDENTITY_MISSING", at, take.sourceCaptureId ?? "<null>");
                PinShape(take.sourceFrameLedger, "TAKE_FRAME_LEDGER", at, report);
                PinShape(take.shotAuthorship, "TAKE_SHOT_AUTHORSHIP", at, report);
                if (take.cameraId != shot.cameraId || take.gameplayState != shot.gameplayState ||
                    take.deterministicSeed != shot.deterministicSeed || take.timelineId != shot.timelineId)
                    report.Error("TAKE_AUTHORSHIP_METADATA_MISMATCH", at,
                        "Every candidate must repeat the atomic shot camera/state/seed/Timeline contract.");
                if (take.declaredHudMode == "clean-plate")
                    PinShape(take.cleanPlateProof, "TAKE_CLEAN_PLATE_PROOF", at, report);
                else
                    PinShape(take.semanticProof, "TAKE_SEMANTIC_PROOF", at, report);
                if (take.takeId == shot.approvedTakeId || take.takeId == shot.cleanPlateTakeId)
                    PinShape(take.automatedProof, "TAKE_AUTOMATED_PROOF", at, report);
                if (take.takeId == shot.approvedTakeId || take.takeId == shot.cleanPlateTakeId)
                    PinShape(take.humanReview, "TAKE_HUMAN_REVIEW", at, report);
                bool ranges = take.sourceRangeStartFrame >= 0 &&
                    take.sourceRangeEndFrame >= take.sourceRangeStartFrame &&
                    take.selectStartFrame >= take.sourceRangeStartFrame &&
                    take.selectEndFrame >= take.selectStartFrame &&
                    take.selectEndFrame <= take.sourceRangeEndFrame;
                if (!ranges)
                    report.Error("TAKE_FRAME_RANGE_INVALID", at, "Invalid inclusive source/select ranges.");
                else
                {
                    int before = take.selectStartFrame - take.sourceRangeStartFrame;
                    int after = take.sourceRangeEndFrame - take.selectEndFrame;
                    if (before != take.handleBeforeFrames || after != take.handleAfterFrames)
                        report.Error("TAKE_HANDLE_ARITHMETIC_INVALID", at, "Handle/range mismatch.");
                    if (before < MinimumHandleFrames || before > MaximumHandleFrames ||
                        after < MinimumHandleFrames || after > MaximumHandleFrames)
                        report.Error("TAKE_HANDLE_DURATION_INVALID", at,
                            "Each source handle must be 180..300 frames (3..5 seconds).");
                    int selected = take.selectEndFrame - take.selectStartFrame + 1;
                    int timeline = shot.timelineEndFrame - shot.timelineStartFrame + 1;
                    if (selected != timeline)
                        report.Error("TAKE_SELECT_TIMELINE_LENGTH_MISMATCH", at,
                            "The candidate select length must equal its atomic-shot timeline length.");
                }
            }
            if (editorialIdentities.Count < requiredCount)
                report.Error("SHOT_CAPTURE_CANDIDATE_COUNT_INSUFFICIENT", shotAt,
                    $"{(shot.coreShot ? "Core" : "Non-core")} shot requires at least {requiredCount} distinct editorial action candidate(s); clean plates are companions.");
            if (string.IsNullOrWhiteSpace(shot.approvedTakeId) || approvedMatches != 1)
                report.Error("SHOT_APPROVED_TAKE_INVALID", shotAt,
                    "approvedTakeId must select exactly one listed candidate.");
            if (!string.IsNullOrWhiteSpace(shot.cleanPlateTakeId) && cleanPlateMatches != 1)
                report.Error("SHOT_CLEAN_PLATE_TAKE_INVALID", shotAt,
                    "cleanPlateTakeId must select one clean-plate candidate.");
        }

        private static void ValidateGateEvidenceStructure(
            AuditionPvSixtySecondGateEvidence evidence, ReportBuilder report)
        {
            if (evidence == null)
            {
                report.Error("GATE_EVIDENCE_MISSING", "gateEvidence", "12-second and 25% review evidence is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(evidence.twelveSecondPackageDirectory))
                report.Error("TWELVE_SECOND_PACKAGE_PATH_MISSING", "gateEvidence", "Package directory is required.");
            if (!AuditionPvSha256.IsSha256(evidence.twelveSecondManifestSha256) ||
                !AuditionPvSha256.IsSha256(evidence.twelveSecondValidationSha256))
                report.Error("TWELVE_SECOND_PACKAGE_PINS_MISSING", "gateEvidence",
                    "Canonical manifest and validation-report hashes are required.");
            PinShape(evidence.twelveSecondApproval, "TWELVE_SECOND_APPROVAL", "gateEvidence", report);
            PinShape(evidence.visualReview, "VISUAL_REVIEW", "gateEvidence", report);
            PinShape(evidence.rightsCoverageReview, "RIGHTS_COVERAGE_REVIEW", "gateEvidence", report);
            AuditionPvTwelveSecondSourceFrameLedgerBinding[] bindings =
                evidence.twelveSecondSourceFrameLedgers ??
                Array.Empty<AuditionPvTwelveSecondSourceFrameLedgerBinding>();
            int required = AuditionPvTwelveSecondGoldAssembler.RequiredRoles.Length;
            if (bindings.Length != required)
                report.Error("TWELVE_SECOND_SOURCE_LEDGER_BINDING_COUNT_INVALID", "gateEvidence",
                    $"Exactly {required} per-segment source-ledger bindings are required.");
            var orders = new HashSet<int>();
            var ledgerByShot = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach ((AuditionPvTwelveSecondSourceFrameLedgerBinding binding, int index) in
                     bindings.Select((value, index) => (value, index)))
            {
                string at = $"gateEvidence.twelveSecondSourceFrameLedgers[{index}]";
                if (binding == null || binding.segmentOrder < 0 || binding.segmentOrder >= required ||
                    !orders.Add(binding.segmentOrder) || string.IsNullOrWhiteSpace(binding.sourceCaptureId) ||
                    string.IsNullOrWhiteSpace(binding.sourceShotId) ||
                    !AuditionPvSha256.IsSha256(binding.sourceManifestSha256) ||
                    !AuditionPvSha256.IsSha256(binding.sourceDependencyIdentitySha256))
                {
                    report.Error("TWELVE_SECOND_SOURCE_LEDGER_BINDING_INVALID", at,
                        "Each segment needs one exact capture/manifest/dependency/shot identity.");
                    continue;
                }
                PinShape(binding.frameLedger, "TWELVE_SECOND_SOURCE_FRAME_LEDGER", at, report);
                string shotKey = string.Join("\0", binding.sourceManifestSha256,
                    binding.sourceCaptureId, binding.sourceShotId);
                string ledgerIdentity = Normalize(binding.frameLedger?.path) + "\0" +
                    (binding.frameLedger?.sha256 ?? string.Empty);
                if (ledgerByShot.TryGetValue(shotKey, out string prior) && prior != ledgerIdentity)
                    report.Error("TWELVE_SECOND_SOURCE_LEDGER_CONFLICT", at,
                        "One canonical source shot cannot use conflicting ledgers.");
                else ledgerByShot[shotKey] = ledgerIdentity;
            }
        }

        private static void ValidateRightsProduction(AuditionPvSixtySecondShotGateManifest manifest,
            IReadOnlyDictionary<string, AuditionPvSixtySecondRightsEvidence> rights,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            foreach (var pair in rights)
            {
                string at = "rights[" + pair.Key + "]";
                if (!ReadPinnedJson(pair.Value.record, context, "RIGHTS_RECORD", at, report,
                        out AuditionPvRightsRecordArtifact artifact, context.allowedLicenseRoots)) continue;
                string[] expectedItems = items.Values
                    .Where(item => item != null && item.rightsRecordId == pair.Key)
                    .Select(item => item.id).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] coveredItems = (artifact.coveredItemIds ?? Array.Empty<string>())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] relatedAudioIds = (manifest?.audio ??
                        Array.Empty<AuditionPvSixtySecondAudioEvidence>())
                    .Where(audio => audio != null &&
                        (expectedItems.Contains(audio.usedItemId, StringComparer.Ordinal) ||
                         expectedItems.Contains(audio.aiUsedItemId, StringComparer.Ordinal)))
                    .Select(audio => audio.id).ToArray();
                string[] expectedShots = (manifest?.buckets ??
                        Array.Empty<AuditionPvSixtySecondSequenceBucket>())
                    .Where(bucket => bucket != null)
                    .SelectMany(bucket => bucket.shots ?? Array.Empty<AuditionPvSixtySecondAtomicShot>())
                    .Where(shot => shot != null &&
                        ((shot.usedItemIds ?? Array.Empty<string>())
                            .Any(id => expectedItems.Contains(id, StringComparer.Ordinal)) ||
                         (shot.audioRefIds ?? Array.Empty<string>())
                            .Any(id => relatedAudioIds.Contains(id, StringComparer.Ordinal))))
                    .Select(shot => shot.shotId).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] coveredShots = (artifact.coveredShotIds ?? Array.Empty<string>())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                if (artifact.schemaVersion != RightsRecordSchema || artifact.rightsRecordId != pair.Key ||
                    artifact.scope != pair.Value.scope || !RightsRecordMetadataValid(artifact) ||
                    artifact.disposition == "tool-only/not-distributed" && expectedItems.Length != 0 ||
                    artifact.disposition == "project-authored" &&
                        (!coveredShots.SequenceEqual(expectedShots, StringComparer.Ordinal) ||
                         expectedShots.Length == 0) ||
                    !coveredItems.SequenceEqual(expectedItems, StringComparer.Ordinal))
                    report.Error("RIGHTS_RECORD_INVALID_OR_OPEN", at,
                        "Used items require exact disposition-appropriate verified rights evidence.");
                if (artifact.disposition == "open-license" || artifact.disposition == "purchased" ||
                    artifact.disposition == "ai-generated")
                    Pinned(artifact.termsSnapshot, context, "RIGHTS_TERMS_SNAPSHOT", at, report,
                        out _, context.allowedLicenseRoots);
                if (artifact.disposition == "purchased")
                    Pinned(artifact.entitlementEvidence, context, "RIGHTS_ENTITLEMENT", at, report,
                        out _, context.allowedLicenseRoots);
                if (artifact.attributionRequired || PinShapeValid(artifact.attributionArtifact))
                    Pinned(artifact.attributionArtifact, context, "RIGHTS_ATTRIBUTION", at, report,
                        out _, context.allowedLicenseRoots);
                if (artifact.disposition == "ai-generated")
                    Pinned(artifact.generationEvidence, context, "RIGHTS_AI_GENERATION", at, report,
                        out _, context.allowedLicenseRoots);
            }
        }

        internal static bool RightsRecordMetadataValid(AuditionPvRightsRecordArtifact value)
        {
            if (value == null || !value.verified || string.IsNullOrWhiteSpace(value.verifiedBy) ||
                !Utc(value.verifiedAtUtc) || string.IsNullOrWhiteSpace(value.useBoundary)) return false;
            return value.disposition switch
            {
                "project-authored" => !string.IsNullOrWhiteSpace(value.owner) &&
                    !string.IsNullOrWhiteSpace(value.sourceDescription) &&
                    string.IsNullOrWhiteSpace(value.provider) &&
                    string.IsNullOrWhiteSpace(value.licenseId) &&
                    string.IsNullOrWhiteSpace(value.accountEntitlementId),
                "open-license" => !string.IsNullOrWhiteSpace(value.provider) &&
                    !string.IsNullOrWhiteSpace(value.licenseId) &&
                    !string.IsNullOrWhiteSpace(value.licenseVersion) &&
                    string.IsNullOrWhiteSpace(value.accountEntitlementId) &&
                    PinShapeValid(value.termsSnapshot) &&
                    (!value.attributionRequired || PinShapeValid(value.attributionArtifact)),
                "purchased" => !string.IsNullOrWhiteSpace(value.provider) &&
                    !string.IsNullOrWhiteSpace(value.licenseId) &&
                    !string.IsNullOrWhiteSpace(value.licenseVersion) &&
                    !string.IsNullOrWhiteSpace(value.accountEntitlementId) &&
                    PinShapeValid(value.termsSnapshot) && PinShapeValid(value.entitlementEvidence),
                "ai-generated" => !string.IsNullOrWhiteSpace(value.provider) &&
                    !string.IsNullOrWhiteSpace(value.accountPlan) &&
                    PinShapeValid(value.termsSnapshot) && PinShapeValid(value.generationEvidence),
                "tool-only/not-distributed" => !string.IsNullOrWhiteSpace(value.exclusionReason),
                _ => false
            };
        }

        private static void ValidateUsedItemsProduction(
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            foreach (var pair in items)
            {
                AuditionPvSixtySecondUsedItem item = pair.Value;
                string at = "usedItems[" + pair.Key + "]";
                if (item == null)
                {
                    report.Error("USED_ITEM_NULL", at, "Indexed used-item evidence is null.");
                    continue;
                }
                if (item.artifact == null)
                {
                    report.Error("USED_ITEM_ARTIFACT_NULL", at,
                        "Every used item needs an explicit pinned artifact object.");
                    continue;
                }
                if (item.dependencyBinding != "unity-dependency")
                {
                    Pinned(item.artifact, context, "USED_ITEM_ARTIFACT", at, report, out _,
                        RootsForItem(item, context));
                    continue;
                }
                try
                {
                    string path = ResolveDependency(item.sourceLocator, context);
                    RejectReparseChain(path);
                    CurrentFile current = CurrentFile.Read(path);
                    if (!current.exists)
                        report.Error("USED_ITEM_UNITY_DEPENDENCY_MISSING", at, item.sourceLocator);
                    else if (item.artifact == null || current.sha256 != item.artifact.sha256)
                        report.Error("USED_ITEM_UNITY_DEPENDENCY_DRIFT", at,
                            "The current Assets/ or Packages/ dependency bytes do not match the used item pin.");
                    else RememberFinalFile(context, path, current.sha256, current.length, report, at);
                }
                catch (Exception exception) when (IsPathOrIo(exception))
                {
                    report.Error("USED_ITEM_UNITY_DEPENDENCY_PATH_INVALID", at, exception.Message);
                }
            }
        }

        private static void ValidateAudioProduction(
            IReadOnlyDictionary<string, AuditionPvSixtySecondAudioEvidence> audio,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            foreach (var pair in audio)
            {
                AuditionPvSixtySecondAudioEvidence value = pair.Value;
                string at = "audio[" + pair.Key + "]";
                if (value == null)
                {
                    report.Error("AUDIO_EVIDENCE_NULL", at, "Indexed audio evidence is null.");
                    continue;
                }
                if (value.file == null)
                {
                    report.Error("AUDIO_FILE_NULL", at,
                        "Every audio row needs an explicit pinned WAV object.");
                    continue;
                }
                if (Pinned(value.file, context, "AUDIO_FILE", at, report, out string path,
                        context.allowedAudioRoots))
                {
                    if (!TryReadWave(path, out WaveInfo wave) || wave.sampleRate != value.sampleRate ||
                        wave.channels != value.channels || wave.dataBytes <= 0 ||
                        wave.durationMilliseconds < MinimumAudioDurationMilliseconds(value.category) ||
                        wave.nonSilentSamples < Math.Max(1, wave.sampleRate * wave.channels / 100))
                        report.Error("AUDIO_WAV_MEASUREMENT_FAILED", at,
                            "A full readable, non-silent WAV with measured 48kHz mono/stereo data and minimum duration is required.");
                    foreach (AuditionPvAudioCueRegion region in value.cueRegions ??
                                 Array.Empty<AuditionPvAudioCueRegion>())
                    {
                        if (!CueRegionShapeValid(region) ||
                            (long)region.endMilliseconds > wave.durationMilliseconds)
                            report.Error("AUDIO_CUE_REGION_OUTSIDE_WAV", at, region?.cueId ?? "<null>");
                        else if (!WaveRegionHasSignal(wave, region.startMilliseconds,
                                     region.endMilliseconds))
                            report.Error("AUDIO_CUE_REGION_SILENT", at,
                                region.cueId ?? "<null>");
                    }
                    if (items.TryGetValue(value.usedItemId ?? string.Empty, out var item) &&
                        item != null)
                    {
                        if (item.artifact == null)
                            report.Error("AUDIO_USED_ITEM_ARTIFACT_NULL", at, value.usedItemId);
                        else if (item.artifact.path != value.file.path ||
                                 item.artifact.sha256 != value.file.sha256)
                            report.Error("AUDIO_USED_ITEM_FILE_MISMATCH", at, value.usedItemId);
                    }
                }
                if (value.generatedByAi && ReadPinnedJson(value.generationManifest, context,
                        "AUDIO_GENERATION", at, report, out AuditionPvAudioGenerationArtifact generation,
                        context.allowedAudioRoots))
                {
                    items.TryGetValue(value.aiUsedItemId ?? string.Empty, out var aiItem);
                    if (!AudioGenerationIdentityValid(generation, value, aiItem))
                        report.Error("AUDIO_GENERATION_PROVENANCE_INVALID", at,
                            "AI audio requires exact tool/model/account/rights and identity-consent provenance.");
                    bool pins = Pinned(generation.promptArtifact, context, "AUDIO_AI_PROMPT", at,
                            report, out string promptPath, context.allowedAudioRoots) &
                        Pinned(generation.originalGeneratedWav, context, "AUDIO_AI_ORIGINAL_WAV", at,
                            report, out string originalPath, context.allowedAudioRoots) &
                        Pinned(generation.editedWav, context, "AUDIO_AI_EDITED_WAV", at,
                            report, out string editedPath, context.allowedAudioRoots) &
                        Pinned(generation.derivationRecipe, context, "AUDIO_AI_DERIVATION_RECIPE", at,
                            report, out string recipePath, context.allowedAudioRoots);
                    if (generation.voiceIdentityDisposition == "consent-documented")
                        pins &= Pinned(generation.consentArtifact, context, "AUDIO_AI_CONSENT", at,
                            report, out _, context.allowedLicenseRoots);
                    bool recipeValid = ReadPinnedJson(generation.derivationRecipe, context,
                        "AUDIO_AI_DERIVATION_RECIPE", at, report,
                        out AuditionPvAudioDerivationRecipeArtifact recipe,
                        context.allowedAudioRoots) && recipe.schemaVersion == AudioDerivationRecipeSchema &&
                        recipe.audioId == value.id && recipe.promptSha256 == generation.promptArtifact?.sha256 &&
                        recipe.originalWavSha256 == generation.originalGeneratedWav?.sha256 &&
                        recipe.editedWavSha256 == generation.editedWav?.sha256 &&
                        recipe.tool == generation.tool && recipe.toolVersion == generation.toolVersion &&
                        (recipe.steps ?? Array.Empty<string>()).Length > 0 &&
                        recipe.steps.All(step => !string.IsNullOrWhiteSpace(step)) && Utc(recipe.createdAtUtc);
                    bool originalReadable = TryReadWave(originalPath, out WaveInfo originalWave) &&
                        originalWave.durationMilliseconds >= 100 && originalWave.nonSilentSamples >=
                        Math.Max(1, originalWave.sampleRate * originalWave.channels / 100);
                    if (!pins || !recipeValid || string.IsNullOrWhiteSpace(promptPath) ||
                        string.IsNullOrWhiteSpace(recipePath) || new FileInfo(promptPath).Length == 0 ||
                        new FileInfo(recipePath).Length == 0 ||
                        generation.editedWav?.sha256 != value.file?.sha256 ||
                        string.IsNullOrWhiteSpace(editedPath) || !PathsEqual(editedPath, path) ||
                        !originalReadable || !TryReadWave(editedPath, out _))
                        report.Error("AUDIO_GENERATION_PHYSICAL_DERIVATION_INVALID", at,
                            "Prompt, original WAV, edited WAV, recipe, and final file must be physical exact pins.");
                }
                ValidateListeningProduction(value, context, report, at);
            }
        }

        private static void ValidateListeningProduction(AuditionPvSixtySecondAudioEvidence value,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report, string at)
        {
            if (value == null)
            {
                report.Error("AUDIO_EVIDENCE_NULL", at, "Listening evidence owner is null.");
                return;
            }
            if (value.file == null)
            {
                report.Error("AUDIO_FILE_NULL", at,
                    "Listening evidence cannot bind a null WAV pin.");
                return;
            }
            if (value.humanListeningStatus == "pending")
            {
                report.Warning("AUDIO_HUMAN_LISTENING_PENDING", at,
                    "Raw material passed mechanical Gate checks, but edit/publication remains on human-listening hold.");
                return;
            }
            if (value.humanListeningStatus == "rejected")
                report.Error("AUDIO_HUMAN_LISTENING_REJECTED", at,
                    "Rejected raw material cannot enter the edit layout.");
            if (!ReadPinnedJson(value.listeningReport, context, "AUDIO_LISTENING_REPORT", at,
                    report, out AuditionPvAudioListeningArtifact artifact,
                    context.allowedReviewRoots)) return;
            if (artifact.schemaVersion != AudioListeningSchema || artifact.audioId != value.id ||
                artifact.fileSha256 != value.file.sha256 ||
                artifact.status != value.humanListeningStatus ||
                string.IsNullOrWhiteSpace(artifact.reviewedBy) || !Utc(artifact.reviewedAtUtc))
                report.Error("AUDIO_LISTENING_REPORT_INVALID", at,
                    "Listening status exists but its typed report is invalid.");
        }

        internal static bool AudioGenerationIdentityValid(AuditionPvAudioGenerationArtifact generation,
            AuditionPvSixtySecondAudioEvidence audio, AuditionPvSixtySecondUsedItem aiItem) =>
            generation != null && audio != null && aiItem != null &&
            generation.schemaVersion == AudioGenerationSchema && generation.audioId == audio.id &&
            generation.aiUsedItemId == audio.aiUsedItemId && aiItem.scope == "ai" &&
            aiItem.artifact?.path == audio.generationManifest?.path &&
            aiItem.artifact?.sha256 == audio.generationManifest?.sha256 &&
            generation.rightsRecordId == aiItem.rightsRecordId &&
            !string.IsNullOrWhiteSpace(generation.provider) &&
            !string.IsNullOrWhiteSpace(generation.tool) &&
            !string.IsNullOrWhiteSpace(generation.toolVersion) &&
            !string.IsNullOrWhiteSpace(generation.model) &&
            !string.IsNullOrWhiteSpace(generation.accountPlan) && Utc(generation.generatedAtUtc) &&
            new[] { "non-real-person-imitation", "consent-documented" }.Contains(
                generation.voiceIdentityDisposition, StringComparer.Ordinal) &&
            PinShapeValid(generation.promptArtifact) &&
            PinShapeValid(generation.originalGeneratedWav) &&
            PinShapeValid(generation.editedWav) &&
            PinShapeValid(generation.derivationRecipe) &&
            generation.editedWav.path == audio.file?.path &&
            generation.editedWav.sha256 == audio.file?.sha256 &&
            (generation.voiceIdentityDisposition != "consent-documented" ||
             PinShapeValid(generation.consentArtifact));

        private static bool PinShapeValid(AuditionPvPinnedArtifact value) => value != null &&
            !string.IsNullOrWhiteSpace(value.path) && AuditionPvSha256.IsSha256(value.sha256);

        private static void ValidateBucketsProduction(AuditionPvSixtySecondShotGateManifest manifest,
            IReadOnlyDictionary<string, AuditionPvSixtySecondAudioEvidence> audio,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            var captures = new Dictionary<string, LoadedCapture>(StringComparer.OrdinalIgnoreCase);
            var dependencies = new Dictionary<string, CurrentFile>(StringComparer.OrdinalIgnoreCase);
            var validatedFrames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var shotLedgers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvSixtySecondSequenceBucket bucket in manifest.buckets ??
                     Array.Empty<AuditionPvSixtySecondSequenceBucket>())
            {
                if (bucket == null) continue;
                foreach (AuditionPvSixtySecondAtomicShot shot in bucket.shots ??
                         Array.Empty<AuditionPvSixtySecondAtomicShot>())
                {
                    if (shot == null) continue;
                    if (shot.sourceKind == "end-card")
                    {
                        string graphicAt = $"buckets[{bucket.bucketId}].shots[{shot.shotId}]";
                        if (Pinned(shot.graphicArtifact, context, "SHOT_END_CARD_GRAPHIC",
                                graphicAt, report, out string graphicPath,
                                context.allowedGraphicsRoots))
                        {
                            if (!IsDecodedPngDimensions(graphicPath, Width, Height))
                                report.Error("SHOT_END_CARD_LAYOUT_PLACEHOLDER_INVALID", graphicAt,
                                    "The planned S100 slot needs one physical decoded QHD layout placeholder.");
                            else
                                report.Warning("SHOT_END_CARD_FINAL_GRAPHIC_PENDING", graphicAt,
                                    "The QHD layout placeholder is approved for planning; official wording/mark and final AE graphic remain deferred.");
                        }
                        if (shot.sloganApprovalStatus != "approved" ||
                            shot.auditionNoticeApprovalStatus != "approved")
                            report.Warning("SHOT_END_CARD_WORDING_APPROVAL_PENDING", graphicAt,
                                "Slogan/audition wording remains an explicit picture-lock hold.");
                        continue;
                    }
                    foreach (AuditionPvSixtySecondTakeCandidate take in shot.candidateTakes ??
                             Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                    {
                        if (take == null) continue;
                        string at = $"buckets[{bucket.bucketId}].shots[{shot.shotId}].takes[{take.takeId}]";
                        ValidateTakeProduction(bucket, shot, take, take.takeId == shot.approvedTakeId,
                            take.takeId == shot.cleanPlateTakeId,
                            items, context, captures, dependencies, validatedFrames, shotLedgers,
                            report, at);
                    }
                }
            }
        }

        private static void ValidateTakeProduction(AuditionPvSixtySecondSequenceBucket bucket,
            AuditionPvSixtySecondAtomicShot shot, AuditionPvSixtySecondTakeCandidate take,
            bool approved, bool cleanPlateCompanion,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            AuditionPvSixtySecondValidationContext context,
            IDictionary<string, LoadedCapture> captures, IDictionary<string, CurrentFile> dependencies,
            ISet<string> validatedFrames, IDictionary<string, string> shotLedgers,
            ReportBuilder report, string at)
        {
            LoadedCapture loaded = LoadCapture(take.sourceManifest, take.sourceDependencyIdentitySha256,
                context, captures, dependencies, report, at);
            if (!loaded.valid) return;
            AuditionPvCaptureManifest capture = loaded.manifest;
            ValidateShotItemsAgainstCapture(shot, items, capture, report, at);
            if (capture.captureId != take.sourceCaptureId || capture.gitCommitSha != take.gitCommitSha)
                report.Error("TAKE_CAPTURE_IDENTITY_MISMATCH", at,
                    "Take and canonical capture manifest identity disagree.");
            if (take.sourceCaptureCoreSha256 != loaded.captureCoreSha256 ||
                !CaptureCoreIdentityMatches(capture, take.sourceCaptureCoreSha256))
                report.Error("TAKE_CAPTURE_CORE_IDENTITY_MISMATCH", at,
                    "Take must bind the canonical test-result-free capture core.");
            AuditionPvShotManifestEntry sourceShot = (capture.shots ??
                Array.Empty<AuditionPvShotManifestEntry>()).SingleOrDefault(value =>
                value != null && value.id == take.sourceShotId);
            if (sourceShot == null)
            {
                report.Error("TAKE_SOURCE_SHOT_MISSING", at, take.sourceShotId ?? "<null>");
                return;
            }
            ValidateShotAuthorship(shot, take, capture, loaded.captureCoreSha256,
                context, report, at);
            if (sourceShot.scenePath != shot.scenePath)
                report.Error("TAKE_SCENE_MISMATCH", at, sourceShot.scenePath ?? "<null>");
            if (!CaptureHudModes.Contains(sourceShot.hudMode ?? string.Empty))
                report.Error("TAKE_HUD_INVALID", at, sourceShot.hudMode ?? "<null>");
            else if (sourceShot.hudMode != take.declaredHudMode)
                report.Error("TAKE_DECLARED_HUD_MISMATCH", at, sourceShot.hudMode);
            if (approved && shot.editorialHudMode != "mixed" && shot.editorialHudMode != "end-card" &&
                sourceShot.hudMode != shot.editorialHudMode)
                report.Error("TAKE_APPROVED_HUD_MISMATCH", at, sourceShot.hudMode);
            if (cleanPlateCompanion && sourceShot.hudMode != "clean-plate")
                report.Error("TAKE_CLEAN_PLATE_HUD_MISMATCH", at, sourceShot.hudMode);
            if (take.sourceRangeStartFrame < sourceShot.startFrame ||
                take.sourceRangeEndFrame > sourceShot.endFrame)
                report.Error("TAKE_SOURCE_RANGE_OUTSIDE_SOURCE_SHOT", at,
                    "Select and handles must remain inside the chosen source shot.");
            if (Pinned(take.sourceFrameLedger, context, "TAKE_FRAME_LEDGER", at, report,
                    out string ledgerPath, new[] { capture.outputDirectory }))
            {
                string ledgerKey = take.sourceManifestSha256 + "\0" + capture.captureId + "\0" + sourceShot.id;
                if (shotLedgers.TryGetValue(ledgerKey, out string priorLedger) &&
                    priorLedger != take.sourceFrameLedger.sha256)
                    report.Error("TAKE_SHOT_LEDGER_IDENTITY_CONFLICT", at,
                        "One canonical capture shot cannot be vouched for by different ledgers.");
                else
                {
                    shotLedgers[ledgerKey] = take.sourceFrameLedger.sha256;
                    ValidateFrameLedgerAndPngs(capture, sourceShot, take, ledgerPath,
                        context, validatedFrames, report, at);
                }
            }
            if (take.declaredHudMode == "clean-plate")
                ValidateCleanPlateProof(bucket, shot, take, capture, context, report, at);
            else
                ValidateSemanticProof(bucket, shot, take, capture, context, report, at);
            // Expensive full-range/color proof belongs only to editorial inputs: the
            // approved take and its linked clean-plate companion. Alternate candidates
            // remain fully manifest/ledger/dependency/QHD validated selection coverage.
            if (approved || cleanPlateCompanion)
                ValidateAutomatedProof(take, sourceShot, capture, context, report, at);
            if (approved || cleanPlateCompanion)
                ValidateTakeReview(bucket, shot, take, capture, context, report, at);
        }

        private static void ValidateShotAuthorship(AuditionPvSixtySecondAtomicShot shot,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            string captureCoreSha256, AuditionPvSixtySecondValidationContext context,
            ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(take.shotAuthorship, context, "TAKE_SHOT_AUTHORSHIP", at,
                    report, out AuditionPvShotAuthorshipArtifact authorship,
                    new[] { capture.outputDirectory })) return;
            string authorshipPath;
            try
            {
                authorshipPath = ResolveEvidencePath(take.shotAuthorship.path, context,
                    new[] { capture.outputDirectory });
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error("TAKE_SHOT_AUTHORSHIP_PATH_INVALID", at, exception.Message);
                return;
            }
            bool valid = ShotAuthorshipIdentityValid(authorship, shot, take, captureCoreSha256) &&
                CaptureTestArtifactMatches(capture, AutomatedTestSuite,
                    "shot-authorship/" + take.sourceShotId, authorshipPath,
                    take.shotAuthorship.sha256);
            if (Pinned(authorship?.runtimeProof, context, "TAKE_SHOT_AUTHORSHIP_RUNTIME", at,
                    report, out string runtimePath, new[] { capture.outputDirectory }))
                valid &= CaptureTestArtifactMatches(capture, AutomatedTestSuite,
                    "shot-authorship-runtime/" + take.sourceShotId, runtimePath,
                    authorship.runtimeProof.sha256);
            else valid = false;
            if (!valid)
                report.Error("TAKE_SHOT_AUTHORSHIP_INVALID", at,
                    "Detached authorship must be core/capture/shot-bound and signed by exact passed capture tests.");
        }

        internal static bool ShotAuthorshipIdentityValid(AuditionPvShotAuthorshipArtifact value,
            AuditionPvSixtySecondAtomicShot shot, AuditionPvSixtySecondTakeCandidate take,
            string captureCoreSha256) => value != null && shot != null && take != null &&
            value.schemaVersion == ShotAuthorshipSchema &&
            AuditionPvSha256.IsSha256(captureCoreSha256) &&
            value.sourceCaptureCoreSha256 == captureCoreSha256 &&
            value.sourceCaptureCoreSha256 == take.sourceCaptureCoreSha256 &&
            value.captureId == take.sourceCaptureId && value.sourceShotId == take.sourceShotId &&
            value.cameraId == take.cameraId && value.cameraId == shot.cameraId &&
            value.gameplayState == take.gameplayState && value.gameplayState == shot.gameplayState &&
            value.deterministicSeed == take.deterministicSeed &&
            value.deterministicSeed == shot.deterministicSeed &&
            value.timelineId == take.timelineId && value.timelineId == shot.timelineId &&
            !string.IsNullOrWhiteSpace(value.tool) && !string.IsNullOrWhiteSpace(value.toolVersion) &&
            Utc(value.createdAtUtc) && value.runtimeProof != null &&
            !string.IsNullOrWhiteSpace(value.runtimeProof.path) &&
            AuditionPvSha256.IsSha256(value.runtimeProof.sha256);

        internal static bool ShotAuthorshipFileIdentityValidForTest(string path,
            AuditionPvPinnedArtifact pin, AuditionPvSixtySecondAtomicShot shot,
            AuditionPvSixtySecondTakeCandidate take, string captureCoreSha256)
        {
            try
            {
                if (pin == null || !PathsEqual(path, pin.path)) return false;
                byte[] bytes = ReadAllBytesCapped(path, MaxEvidenceJsonBytes,
                    "Shot-authorship JSON");
                if (ByteSha256(bytes) != pin.sha256) return false;
                var value = JsonUtility.FromJson<AuditionPvShotAuthorshipArtifact>(
                    new UTF8Encoding(false, true).GetString(bytes).TrimStart('\ufeff'));
                return ShotAuthorshipIdentityValid(value, shot, take, captureCoreSha256);
            }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static void ValidateShotItemsAgainstCapture(AuditionPvSixtySecondAtomicShot shot,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            AuditionPvCaptureManifest capture, ReportBuilder report, string at)
        {
            foreach (string id in shot.usedItemIds ?? Array.Empty<string>())
            {
                if (!items.TryGetValue(id ?? string.Empty, out var item) || item == null ||
                    item.scope != "asset" && item.scope != "font") continue;
                if (item.dependencyBinding != "unity-dependency")
                {
                    report.Error("SHOT_USED_ITEM_NOT_CAPTURE_DEPENDENCY", at, id);
                    continue;
                }
                if (item.artifact == null)
                {
                    report.Error("SHOT_USED_ITEM_ARTIFACT_NULL", at, id);
                    continue;
                }
                string locator = Normalize(item.sourceLocator);
                AuditionPvDependencyHash dependency = (capture.dependencyHashes ??
                    Array.Empty<AuditionPvDependencyHash>()).SingleOrDefault(value =>
                    value != null && Normalize(value.path) == locator);
                if (dependency == null || !dependency.exists ||
                    dependency.sha256 != item.artifact.sha256)
                    report.Error("SHOT_USED_ITEM_DEPENDENCY_MISMATCH", at, id);
            }
        }

        private static LoadedCapture LoadCapture(AuditionPvPinnedArtifact sourceManifest,
            string dependencyIdentityPin, AuditionPvSixtySecondValidationContext context,
            IDictionary<string, LoadedCapture> cache, IDictionary<string, CurrentFile> dependencyCache,
            ReportBuilder report, string at)
        {
            string key = (sourceManifest?.path ?? string.Empty) + "\0" +
                (sourceManifest?.sha256 ?? string.Empty) + "\0" +
                (dependencyIdentityPin ?? string.Empty);
            if (cache.TryGetValue(key, out LoadedCapture cached)) return cached;
            int startErrors = report.ErrorCount;
            if (!Pinned(sourceManifest, context, "TAKE_SOURCE_MANIFEST", at, report,
                    out string path, context.allowedCaptureRoots))
                return cache[key] = new LoadedCapture();
            AuditionPvCaptureManifest capture = null;
            string captureCoreSha256 = string.Empty;
            try
            {
                byte[] bytes = ReadAllBytesCapped(path, MaxManifestJsonBytes,
                    "Capture manifest JSON");
                if (ByteSha256(bytes) != sourceManifest.sha256)
                    throw new InvalidDataException("Source manifest changed while it was being validated.");
                capture = JsonUtility.FromJson<AuditionPvCaptureManifest>(
                    new UTF8Encoding(false, true).GetString(bytes).TrimStart('\ufeff'));
                if (capture == null || (capture.shots ?? Array.Empty<AuditionPvShotManifestEntry>()).Length > 512 ||
                    (capture.baselines ?? Array.Empty<AuditionPvBaselineManifestEntry>()).Length > 2048 ||
                    (capture.dependencyHashes ?? Array.Empty<AuditionPvDependencyHash>()).Length > 4096 ||
                    (capture.testResults ?? Array.Empty<AuditionPvTestResult>()).Length > 4096)
                    throw new InvalidDataException("Capture manifest cardinality limit exceeded.");
                AuditionPvCaptureManifestWriter.Validate(capture);
                string expected = Path.GetFullPath(Path.Combine(capture.outputDirectory,
                    AuditionPvCaptureContract.ManifestFileName));
                if (!PathsEqual(path, expected))
                    throw new InvalidDataException(
                        "Source manifest is not the canonical direct file declared by its capture output.");
                RequireUnder(path, context.allowedCaptureRoots, "capture manifest");
                captureCoreSha256 = CaptureCoreSha256(capture);
                if (!AuditionPvSha256.IsSha256(captureCoreSha256))
                    throw new InvalidDataException("Capture core digest could not be computed.");
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error("TAKE_SOURCE_MANIFEST_INVALID", at, exception.Message);
            }
            if (capture != null)
            {
                if (capture.gitWorktreeDirty)
                    report.Error("TAKE_SOURCE_CAPTURE_DIRTY", at, capture.captureId);
                if (capture.width != Width || capture.height != Height || capture.fps != Fps ||
                    capture.sourceFormat != AuditionPvCaptureContract.SourceFormat)
                    report.Error("TAKE_SOURCE_FORMAT_INVALID", at, capture.captureId);
                if ((capture.testResults ?? Array.Empty<AuditionPvTestResult>()).Length == 0 ||
                    capture.testResults.Any(value => value == null || value.status != "passed"))
                    report.Error("TAKE_SOURCE_TEST_FAILURE", at, capture.captureId);
                string actualIdentity = DependencyIdentity(capture, report, at);
                if (!AuditionPvSha256.IsSha256(dependencyIdentityPin) ||
                    actualIdentity != dependencyIdentityPin)
                    report.Error("TAKE_DEPENDENCY_IDENTITY_MISMATCH", at, capture.captureId);
                foreach (AuditionPvDependencyHash dependency in capture.dependencyHashes ??
                         Array.Empty<AuditionPvDependencyHash>())
                {
                    if (dependency == null) continue;
                    string dependencyPath;
                    try { dependencyPath = ResolveDependency(dependency.path, context); }
                    catch (Exception exception) when (IsPathOrIo(exception))
                    {
                        report.Error("TAKE_PRODUCT_DEPENDENCY_PATH_INVALID", at, exception.Message);
                        continue;
                    }
                    if (!dependencyCache.TryGetValue(dependencyPath, out CurrentFile current))
                        dependencyCache[dependencyPath] = current = CurrentFile.Read(dependencyPath);
                    if (!current.exists)
                        report.Error("TAKE_PRODUCT_DEPENDENCY_MISSING", at, dependency.path);
                    else if (current.length != dependency.byteLength || current.sha256 != dependency.sha256)
                        report.Error("TAKE_PRODUCT_DEPENDENCY_DRIFT", at, dependency.path);
                    else RememberFinalFile(context, dependencyPath, current.sha256, current.length,
                        report, at);
                }
                // The clean SHA recorded at capture time is immutable provenance. It need not
                // equal the validator's current HEAD: current dependency bytes are re-hashed
                // above, while authoritative validation separately requires a stable clean HEAD.
            }
            return cache[key] = new LoadedCapture
            {
                manifest = capture,
                manifestPath = path,
                manifestSha256 = sourceManifest?.sha256 ?? string.Empty,
                captureCoreSha256 = captureCoreSha256,
                valid = capture != null && report.ErrorCount == startErrors
            };
        }

        private static void ValidateFrameLedgerAndPngs(AuditionPvCaptureManifest capture,
            AuditionPvShotManifestEntry sourceShot, AuditionPvSixtySecondTakeCandidate take,
            string ledgerPath, AuditionPvSixtySecondValidationContext context,
            ISet<string> validatedFrames, ReportBuilder report, string at)
        {
            Dictionary<string, string> entries = ParseFrameLedger(ledgerPath, report, at);
            string frameDirectory = Path.GetDirectoryName(CanonicalSourceFramePath(capture,
                sourceShot.id, sourceShot.startFrame));
            try
            {
                RequireUnder(frameDirectory, new[] { capture.outputDirectory }, "source frame directory");
                RejectReparseChain(frameDirectory);
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error("TAKE_FRAME_DIRECTORY_INVALID", at, exception.Message);
                return;
            }
            long sourceFrameCount = (long)take.sourceRangeEndFrame -
                take.sourceRangeStartFrame + 1L;
            if (sourceFrameCount <= 0 || sourceFrameCount > 100000L)
            {
                report.Error("TAKE_SOURCE_SHOT_RANGE_INVALID", at, sourceShot.id ?? "<null>");
                return;
            }
            foreach (int frame in CandidateSourceFrames(take))
            {
                string fileName = $"frame_{frame:0000}.png";
                string nested = CanonicalSourceFrameRelative(sourceShot.id, frame);
                string relative = entries.ContainsKey(nested) ? nested :
                    entries.ContainsKey(fileName) ? fileName : string.Empty;
                if (string.IsNullOrEmpty(relative))
                {
                    report.Error("TAKE_FRAME_LEDGER_ENTRY_MISSING", at, nested);
                    continue;
                }
                string framePath = relative == fileName
                    ? Path.Combine(frameDirectory, fileName)
                    : Path.Combine(capture.outputDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
                try { RequireUnder(framePath, new[] { capture.outputDirectory }, "source frame"); }
                catch (Exception exception) when (IsPathOrIo(exception))
                {
                    report.Error("TAKE_FRAME_PATH_INVALID", at, exception.Message);
                    continue;
                }
                try { RejectReparseChain(framePath); }
                catch (Exception exception) when (IsPathOrIo(exception))
                {
                    report.Error("TAKE_FRAME_PATH_INVALID", at, exception.Message);
                    continue;
                }
                string cacheKey = take.sourceManifestSha256 + "\0" + capture.captureId + "\0" +
                    sourceShot.id + "\0" + frame;
                if (!validatedFrames.Add(cacheKey)) continue;
                if (!File.Exists(framePath))
                {
                    report.Error("TAKE_SOURCE_FRAME_MISSING", at, framePath);
                    continue;
                }
                string actualSha = AuditionPvSha256.FileHash(framePath);
                if (actualSha != entries[relative])
                    report.Error("TAKE_SOURCE_FRAME_HASH_DRIFT", at, framePath);
                else RememberFinalFile(context, framePath, actualSha,
                    new FileInfo(framePath).Length, report, at);
                if (!IsDecodedPngDimensions(framePath, Width, Height))
                    report.Error("TAKE_SOURCE_FRAME_NOT_QHD_PNG", at, framePath);
            }
            // Frames outside the candidate's declared select+handle range are not Gate inputs.
            // They may legitimately coexist in the immutable source shot/ledger.
        }

        private static IEnumerable<int> CandidateSourceFrames(
            AuditionPvSixtySecondTakeCandidate take)
        {
            if (take == null) yield break;
            for (long frame = take.sourceRangeStartFrame;
                 frame <= take.sourceRangeEndFrame; frame++)
                yield return checked((int)frame);
        }

        internal static int[] CandidateSourceFramesForTest(
            AuditionPvSixtySecondTakeCandidate take) => CandidateSourceFrames(take).ToArray();

        private static Dictionary<string, string> ParseFrameLedger(
            string path, ReportBuilder report, string at)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                var file = new FileInfo(path);
                if (!file.Exists || file.Length > MaxFrameLedgerBytes)
                    throw new InvalidDataException("Frame ledger exceeds the accepted byte limit.");
                using var stream = File.OpenRead(path);
                using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true,
                    4096, false);
                int index = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    index++;
                    if (index > MaxFrameLedgerEntries)
                        throw new InvalidDataException("Frame ledger entry limit exceeded.");
                    if (line != null && line.Length > MaxLedgerLineCharacters)
                        throw new InvalidDataException("Frame ledger line is too long.");
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.Length < 67 || line[64] != ' ' || line[65] != ' ' ||
                        !AuditionPvSha256.IsSha256(line.Substring(0, 64)))
                    {
                        report.Error("TAKE_FRAME_LEDGER_INVALID", at, $"line {index}");
                        continue;
                    }
                    string relative = Normalize(line.Substring(66));
                    if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) ||
                        relative.Contains("..", StringComparison.Ordinal) || relative.Contains(':') ||
                        !result.TryAdd(relative, line.Substring(0, 64)))
                        report.Error("TAKE_FRAME_LEDGER_INVALID", at,
                            $"unsafe/duplicate line {index}");
                }
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            { report.Error("TAKE_FRAME_LEDGER_INVALID", at, exception.Message); }
            if (result.Count == 0)
                report.Error("TAKE_FRAME_LEDGER_EMPTY", at, path);
            return result;
        }

        private static void ValidateSemanticProof(AuditionPvSixtySecondSequenceBucket bucket,
            AuditionPvSixtySecondAtomicShot shot, AuditionPvSixtySecondTakeCandidate take,
            AuditionPvCaptureManifest capture, AuditionPvSixtySecondValidationContext context,
            ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(take.semanticProof, context, "TAKE_SEMANTIC_PROOF", at,
                    report, out AuditionPvTakeSemanticProofArtifact proof,
                    new[] { capture.outputDirectory })) return;
            if (proof.schemaVersion != SemanticProofSchema || proof.captureId != take.sourceCaptureId ||
                proof.sourceManifestSha256 != take.sourceManifestSha256 ||
                proof.sourceShotId != take.sourceShotId || proof.bucketId != bucket.bucketId ||
                proof.atomicShotId != shot.shotId || proof.scenePath != shot.scenePath ||
                proof.cameraId != shot.cameraId || proof.gameplayState != shot.gameplayState ||
                proof.timelineId != shot.timelineId || proof.deterministicSeed != shot.deterministicSeed ||
                !SameRange(proof, take) ||
                !(proof.beatIds ?? Array.Empty<string>()).OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual((shot.beatIds ?? Array.Empty<string>())
                        .OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal) ||
                !SemanticBeatProofSetValid(proof.beatProofs, shot.beatIds))
                report.Error("TAKE_SEMANTIC_PROOF_INVALID", at,
                    "Every semantic beat needs its own exact reviewer/test/runtime-artifact identity.");
            foreach (AuditionPvSemanticBeatProof beat in proof.beatProofs ??
                     Array.Empty<AuditionPvSemanticBeatProof>())
            {
                if (beat == null) continue;
                if (!Pinned(beat.runtimeProof, context, "TAKE_RUNTIME_BEAT_PROOF", at, report,
                        out string runtimePath, new[] { capture.outputDirectory })) continue;
                if (!CaptureSemanticBeatArtifactMatches(capture, beat,
                        runtimePath))
                    report.Error("TAKE_RUNTIME_BEAT_PROOF_NOT_FROM_EXACT_PASSED_TEST", at,
                        beat.beatId ?? "<null>");
            }
        }

        internal static bool SemanticBeatProofSetValid(AuditionPvSemanticBeatProof[] values,
            string[] expectedBeatIds)
        {
            values ??= Array.Empty<AuditionPvSemanticBeatProof>();
            expectedBeatIds ??= Array.Empty<string>();
            if (values.Length != expectedBeatIds.Length || values.Any(value => value == null))
                return false;
            string[] declared = values.Select(value => value.beatId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] expected = expectedBeatIds.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return declared.Distinct(StringComparer.Ordinal).Count() == declared.Length &&
                declared.SequenceEqual(expected, StringComparer.Ordinal) && values.All(value =>
                    value.supportingTestSuite == AutomatedTestSuite &&
                    value.supportingTestName == "semantic-beat/" + value.beatId &&
                    value.runtimeFactKey == value.beatId &&
                    string.IsNullOrWhiteSpace(value.verifiedBy) == false && Utc(value.verifiedAtUtc) &&
                    !string.IsNullOrWhiteSpace(value.runtimeProof?.path) &&
                    AuditionPvSha256.IsSha256(value.runtimeProof?.sha256));
        }

        private static void ValidateCleanPlateProof(AuditionPvSixtySecondSequenceBucket bucket,
            AuditionPvSixtySecondAtomicShot shot, AuditionPvSixtySecondTakeCandidate take,
            AuditionPvCaptureManifest capture, AuditionPvSixtySecondValidationContext context,
            ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(take.cleanPlateProof, context, "TAKE_CLEAN_PLATE_PROOF", at,
                    report, out AuditionPvCleanPlateCompanionProofArtifact proof,
                    new[] { capture.outputDirectory })) return;
            AuditionPvSixtySecondTakeCandidate reference = (shot.candidateTakes ??
                    Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                .SingleOrDefault(value => value != null && value.takeId == shot.approvedTakeId &&
                    value.declaredHudMode != "clean-plate");
            if (proof.schemaVersion != CleanPlateProofSchema ||
                proof.captureId != take.sourceCaptureId ||
                proof.sourceManifestSha256 != take.sourceManifestSha256 ||
                proof.sourceShotId != take.sourceShotId || proof.bucketId != bucket.bucketId ||
                proof.atomicShotId != shot.shotId || proof.referenceTakeId != shot.approvedTakeId ||
                !CleanPlateReferenceMatches(proof, reference) ||
                proof.scenePath != shot.scenePath || proof.cameraId != shot.cameraId ||
                proof.gameplayState != shot.gameplayState || proof.timelineId != shot.timelineId ||
                proof.deterministicSeed != shot.deterministicSeed || !SameRange(proof, take) ||
                string.IsNullOrWhiteSpace(proof.proofTool) || !Utc(proof.createdAtUtc))
                report.Error("TAKE_CLEAN_PLATE_PROOF_INVALID", at,
                    "Clean plate must be a metadata-matched companion, not action-beat proof.");
        }

        internal static bool CleanPlateReferenceMatches(
            AuditionPvCleanPlateCompanionProofArtifact proof,
            AuditionPvSixtySecondTakeCandidate reference) => proof != null && reference != null &&
            proof.referenceTakeId == reference.takeId &&
            proof.referenceCaptureId == reference.sourceCaptureId &&
            proof.referenceSourceManifestSha256 == reference.sourceManifestSha256 &&
            proof.referenceSourceShotId == reference.sourceShotId &&
            proof.referenceFrameLedgerSha256 == reference.sourceFrameLedger?.sha256 &&
            proof.referenceSourceRangeStartFrame == reference.sourceRangeStartFrame &&
            proof.referenceSourceRangeEndFrame == reference.sourceRangeEndFrame &&
            proof.referenceSelectStartFrame == reference.selectStartFrame &&
            proof.referenceSelectEndFrame == reference.selectEndFrame;

        private static void ValidateAutomatedProof(AuditionPvSixtySecondTakeCandidate take,
            AuditionPvShotManifestEntry sourceShot, AuditionPvCaptureManifest capture,
            AuditionPvSixtySecondValidationContext context,
            ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(take.automatedProof, context, "TAKE_AUTOMATED_PROOF", at,
                    report, out AuditionPvTakeAutomatedProofArtifact proof,
                    new[] { capture.outputDirectory })) return;
            string captureCoreSha256 = CaptureCoreSha256(capture);
            if (proof.schemaVersion != AutomatedProofSchema || proof.captureId != take.sourceCaptureId ||
                proof.sourceCaptureCoreSha256 != captureCoreSha256 ||
                proof.sourceCaptureCoreSha256 != take.sourceCaptureCoreSha256 ||
                proof.sourceShotId != take.sourceShotId || !SameRange(proof, take))
                report.Error("TAKE_AUTOMATED_PROOF_IDENTITY_INVALID", at,
                    "Automated checks must bind to the exact immutable capture core, shot, and range.");
            string[] requiredChecks = take.declaredHudMode == "clean-plate"
                ? AutomatedChecks.Concat(new[] { HudAbsentCheck }).ToArray()
                : AutomatedChecks;
            if ((proof.checks ?? Array.Empty<AuditionPvAutomatedCheckEvidence>()).Length > 16)
            {
                report.Error("TAKE_AUTOMATED_CHECK_CARDINALITY_EXCEEDED", at,
                    "At most sixteen typed automated checks are accepted per selected take.");
                return;
            }
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var artifacts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvAutomatedCheckEvidence check in proof.checks ??
                     Array.Empty<AuditionPvAutomatedCheckEvidence>())
            {
                if (check == null || !requiredChecks.Contains(check.id, StringComparer.Ordinal) ||
                    !seen.Add(check.id ?? string.Empty) || check.status != "passed" ||
                    check.supportingTestSuite != AutomatedTestSuite ||
                    check.supportingTestName != check.id)
                {
                    report.Error("TAKE_AUTOMATED_CHECK_INVALID", at, check?.id ?? "<null>");
                    continue;
                }
                if (!ReadPinnedJson(check.artifact, context, "TAKE_AUTOMATED_CHECK_ARTIFACT", at,
                        report, out AuditionPvAutomatedCheckResultArtifact result,
                        new[] { capture.outputDirectory })) continue;
                string checkPath = ResolveEvidencePath(check.artifact.path, context,
                    new[] { capture.outputDirectory });
                string artifactIdentity = Normalize(checkPath) + "\0" + check.artifact.sha256;
                if (!artifacts.Add(artifactIdentity))
                    report.Error("TAKE_AUTOMATED_CHECK_ARTIFACT_REUSED", at, check.id);
                if (!CaptureTestArtifactMatches(capture, check.supportingTestSuite,
                        check.supportingTestName, checkPath, check.artifact.sha256))
                    report.Error("TAKE_AUTOMATED_CHECK_NOT_FROM_PASSED_CAPTURE_TEST", at, check.id);
                ValidateAutomatedCheckResult(check.id, result, take, sourceShot, capture,
                    context, report, at);
            }
            foreach (string id in requiredChecks.Where(id => !seen.Contains(id)))
                report.Error("TAKE_AUTOMATED_CHECK_MISSING", at, id);
        }

        private static void ValidateAutomatedCheckResult(string id,
            AuditionPvAutomatedCheckResultArtifact result, AuditionPvSixtySecondTakeCandidate take,
            AuditionPvShotManifestEntry sourceShot, AuditionPvCaptureManifest capture,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report, string at)
        {
            if (result == null || result.schemaVersion != AutomatedCheckResultSchema ||
                result.id != id || result.captureId != take.sourceCaptureId ||
                result.sourceCaptureCoreSha256 != CaptureCoreSha256(capture) ||
                result.sourceCaptureCoreSha256 != take.sourceCaptureCoreSha256 ||
                result.sourceShotId != take.sourceShotId || !SameRange(result, take) ||
                result.sourceFrameLedgerSha256 != (take.sourceFrameLedger?.sha256 ?? string.Empty) ||
                string.IsNullOrWhiteSpace(result.measurementTool) ||
                string.IsNullOrWhiteSpace(result.measurementToolVersion) || !Utc(result.measuredAtUtc))
            {
                report.Error("TAKE_AUTOMATED_CHECK_RESULT_IDENTITY_INVALID", at, id);
                return;
            }
            if (FullRangeScanChecks.Contains(id) &&
                !ValidateFullSourceRangeFrameScan(id, result, take, capture, context, report, at))
                report.Error("TAKE_FULL_RANGE_SCAN_LEDGER_INVALID", at,
                    "Every source-range frame, including both handles, must be physically measured; temporal filmstrips are preview-only.");
            switch (id)
            {
                case "contact-sheet":
                    bool contactSamples = ValidateMeasuredFrames(result, take, capture, report, at,
                        out string[] contactHashes);
                    if (!Pinned(result.mediaArtifact, context, "TAKE_CONTACT_SHEET", at, report,
                            out string mediaPath, new[] { capture.outputDirectory }) ||
                        !TryDecodedPngDimensions(mediaPath, out int mediaWidth, out int mediaHeight) ||
                        result.measuredWidth != mediaWidth || result.measuredHeight != mediaHeight ||
                        result.mediaPurpose != "quarter-scale-contact-preview-only" ||
                        contactHashes.Length == 0 ||
                        result.mediaColumns != Math.Min(4, contactHashes.Length) ||
                        result.mediaRows != (contactHashes.Length + result.mediaColumns - 1) /
                            result.mediaColumns || contactHashes.Length > MaxPreviewCells ||
                        mediaWidth != result.mediaColumns * (Width / 4) ||
                        mediaHeight != result.mediaRows * (Height / 4) || !contactSamples ||
                        !(result.filmstripInputSha256 ?? Array.Empty<string>())
                            .SequenceEqual(contactHashes, StringComparer.Ordinal) ||
                        !ContactSheetMatchesQuarterScale(mediaPath,
                            SampledSourcePaths(take, capture), result.mediaColumns, result.mediaRows))
                        report.Error("TAKE_CONTACT_SHEET_RESULT_INVALID", at,
                            "The test-bound 25% sheet must bind deterministic selected-range source hashes.");
                    break;
                case "missing-frame":
                    int requiredSourceFrames = take.sourceRangeEndFrame -
                        take.sourceRangeStartFrame + 1;
                    if (result.expectedFrameCount != requiredSourceFrames ||
                        result.observedFrameCount != requiredSourceFrames)
                        report.Error("TAKE_MISSING_FRAME_RESULT_INVALID", at,
                            "The selected range plus declared handles must have zero missing frames.");
                    break;
                case "error-magenta":
                    if (result.sampledPixelCount <= 0 || result.detectedPixelCount != 0 ||
                        result.inspectedFrameCount != take.sourceRangeEndFrame -
                            take.sourceRangeStartFrame + 1 ||
                        !ValidateMeasuredFrames(result, take, capture, report, at, out _))
                        report.Error("TAKE_MAGENTA_RESULT_INVALID", at,
                            "The full select-and-handle pixel scan must report zero error-magenta pixels.");
                    break;
                case "resolution":
                    if (result.measuredWidth != Width || result.measuredHeight != Height)
                        report.Error("TAKE_RESOLUTION_RESULT_INVALID", at, $"{Width}x{Height} required.");
                    break;
                case "rec709":
                    if (!ValidateRec709Evidence(result, take, capture, context, report, at))
                        report.Error("TAKE_REC709_RESULT_INVALID", at,
                            "Every source-range frame, including handles, needs an exact canonical Rec.709 edit-original output, ledger, and pinned transform config.");
                    break;
                case "renderer-material-scan":
                    if (result.inspectedFrameCount != take.sourceRangeEndFrame -
                            take.sourceRangeStartFrame + 1 ||
                        result.nullMaterialCount != 0 || result.errorMaterialCount != 0 ||
                        !ValidateMeasuredFrames(result, take, capture, report, at, out _))
                        report.Error("TAKE_RENDERER_MATERIAL_SCAN_INVALID", at,
                            "The full select-and-handle range needs positive renderer/material inventory with zero null/error material slots.");
                    break;
                case HudAbsentCheck:
                    if (!result.rendererHudLayerExcluded ||
                        result.inspectedFrameCount != take.sourceRangeEndFrame -
                            take.sourceRangeStartFrame + 1 ||
                        !ValidateMeasuredFrames(result, take, capture, report, at, out _))
                        report.Error("TAKE_CLEAN_PLATE_HUD_RESULT_INVALID", at,
                            "Clean plate requires a capture-time typed HUD workload and zero visible UI.");
                    break;
            }
        }

        private static bool ValidateFullSourceRangeFrameScan(string id,
            AuditionPvAutomatedCheckResultArtifact result,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(result.scanConfig, context, "TAKE_SCAN_CONFIG", at, report,
                    out AuditionPvSelectedFrameScanConfigArtifact config,
                    new[] { capture.outputDirectory }) ||
                !ReadPinnedJson(result.scanLedger, context, "TAKE_SCAN_LEDGER", at, report,
                    out AuditionPvSelectedFrameScanLedgerArtifact ledger,
                    new[] { capture.outputDirectory })) return false;
            string captureCoreSha256 = CaptureCoreSha256(capture);
            bool valid = config != null && config.schemaVersion == FrameScanConfigSchema &&
                config.checkId == id && config.captureId == take.sourceCaptureId &&
                config.sourceCaptureCoreSha256 == captureCoreSha256 &&
                config.sourceCaptureCoreSha256 == take.sourceCaptureCoreSha256 &&
                config.sourceShotId == take.sourceShotId &&
                config.sourceFrameLedgerSha256 == take.sourceFrameLedger?.sha256 &&
                SameRange(config, take) && config.frameStride == 1 &&
                config.temporalPairStride == 0 &&
                (id != "error-magenta" || config.pixelStride == 1) &&
                !string.IsNullOrWhiteSpace(config.tool) &&
                !string.IsNullOrWhiteSpace(config.toolVersion) &&
                config.algorithm == ExpectedScanAlgorithm(id) &&
                config.algorithmSha256 == ExpectedScanAlgorithmSha256(id) &&
                Utc(config.createdAtUtc) &&
                ledger != null && ledger.schemaVersion == FrameScanLedgerSchema &&
                ledger.checkId == id && ledger.captureId == take.sourceCaptureId &&
                ledger.sourceCaptureCoreSha256 == captureCoreSha256 &&
                ledger.sourceCaptureCoreSha256 == take.sourceCaptureCoreSha256 &&
                ledger.sourceShotId == take.sourceShotId &&
                ledger.sourceFrameLedgerSha256 == take.sourceFrameLedger?.sha256 &&
                ledger.configSha256 == result.scanConfig?.sha256 && SameRange(ledger, take);
            valid &= FullRangeScanTopologyValid(id, config, ledger, take);
            long expectedCount = (long)take.sourceRangeEndFrame -
                take.sourceRangeStartFrame + 1L;
            AuditionPvSelectedFrameScanEntry[] frames = ledger?.frames ??
                Array.Empty<AuditionPvSelectedFrameScanEntry>();
            if (expectedCount <= 0 || expectedCount > 100000L || frames.LongLength != expectedCount)
                return false;
            long pixels = 0L, magenta = 0L, nullMaterials = 0L, errorMaterials = 0L;
            try
            {
                for (int index = 0; index < frames.Length; index++)
                {
                    AuditionPvSelectedFrameScanEntry frame = frames[index];
                    int expectedFrame = checked(take.sourceRangeStartFrame + index);
                    if (frame == null || frame.sourceFrame != expectedFrame ||
                        !AuditionPvSha256.IsSha256(frame.frameSha256) || frame.width != Width ||
                        frame.height != Height || frame.sampledPixelCount < 0 ||
                        frame.errorMagentaPixelCount < 0 || frame.nullMaterialCount < 0 ||
                        frame.errorMaterialCount < 0)
                    { valid = false; continue; }
                    if (id == "error-magenta" &&
                        frame.sampledPixelCount != (long)Width * Height) valid = false;
                    string path = CanonicalSourceFramePath(capture, take.sourceShotId, expectedFrame);
                    try
                    {
                        RequireUnder(path, new[] { capture.outputDirectory }, "scan source frame");
                        RejectReparseChain(path);
                        if (!File.Exists(path) || AuditionPvSha256.FileHash(path) != frame.frameSha256 ||
                            !IsDecodedPngDimensions(path, Width, Height)) valid = false;
                        else
                        {
                            RememberFinalFile(context, path, frame.frameSha256,
                                new FileInfo(path).Length, report, at);
                            if (id == "error-magenta" &&
                                (!TryCountErrorMagentaPixels(path, out long measuredMagenta) ||
                                 measuredMagenta != frame.errorMagentaPixelCount)) valid = false;
                        }
                    }
                    catch (Exception exception) when (IsPathOrIo(exception)) { valid = false; }
                    pixels = checked(pixels + frame.sampledPixelCount);
                    magenta = checked(magenta + frame.errorMagentaPixelCount);
                    nullMaterials = checked(nullMaterials + frame.nullMaterialCount);
                    errorMaterials = checked(errorMaterials + frame.errorMaterialCount);
                    if (id == HudAbsentCheck && !frame.rendererHudLayerExcluded) valid = false;
                }
            }
            catch (OverflowException) { valid = false; }
            valid &= result.inspectedFrameCount == expectedCount;
            switch (id)
            {
                case "error-magenta":
                    valid &= pixels > 0 && result.sampledPixelCount == pixels &&
                        result.detectedPixelCount == magenta && magenta == 0;
                    break;
                case "renderer-material-scan":
                    valid &= result.nullMaterialCount == nullMaterials &&
                        result.errorMaterialCount == errorMaterials &&
                        nullMaterials == 0 && errorMaterials == 0;
                    break;
                case HudAbsentCheck:
                    valid &= result.rendererHudLayerExcluded;
                    break;
            }
            if (id == "renderer-material-scan" || id == HudAbsentCheck)
                valid &= ValidateRuntimeWorkload(id, result, ledger, take, capture,
                    captureCoreSha256, context, report, at);
            valid &= FullScanAggregatesMatch(id, result, ledger);
            return valid;
        }

        internal static bool FullScanAggregatesMatch(string id,
            AuditionPvAutomatedCheckResultArtifact result,
            AuditionPvSelectedFrameScanLedgerArtifact ledger)
        {
            if (result == null || ledger == null) return false;
            try
            {
                long pixels = 0, magenta = 0, nulls = 0, errors = 0;
                foreach (AuditionPvSelectedFrameScanEntry frame in ledger.frames ??
                             Array.Empty<AuditionPvSelectedFrameScanEntry>())
                {
                    if (frame == null) return false;
                    pixels = checked(pixels + frame.sampledPixelCount);
                    magenta = checked(magenta + frame.errorMagentaPixelCount);
                    nulls = checked(nulls + frame.nullMaterialCount);
                    errors = checked(errors + frame.errorMaterialCount);
                }
                return result.inspectedFrameCount == (ledger.frames?.LongLength ?? 0L) && id switch
                {
                    "error-magenta" => result.sampledPixelCount == pixels &&
                        result.detectedPixelCount == magenta,
                    "renderer-material-scan" => result.nullMaterialCount == nulls &&
                        result.errorMaterialCount == errors,
                    HudAbsentCheck => result.rendererHudLayerExcluded,
                    _ => false
                };
            }
            catch (OverflowException) { return false; }
        }

        internal static bool FullRangeScanTopologyValid(string id,
            AuditionPvSelectedFrameScanConfigArtifact config,
            AuditionPvSelectedFrameScanLedgerArtifact ledger,
            AuditionPvSixtySecondTakeCandidate take)
        {
            if (config == null || ledger == null || take == null || config.checkId != id ||
                ledger.checkId != id || config.frameStride != 1 || config.temporalPairStride != 0 ||
                (id == "error-magenta" && config.pixelStride != 1) ||
                config.algorithm != ExpectedScanAlgorithm(id) ||
                config.algorithmSha256 != ExpectedScanAlgorithmSha256(id) ||
                !SameRange(config, take) || !SameRange(ledger, take)) return false;
            long expected = (long)take.sourceRangeEndFrame -
                take.sourceRangeStartFrame + 1L;
            AuditionPvSelectedFrameScanEntry[] frames = ledger.frames ??
                Array.Empty<AuditionPvSelectedFrameScanEntry>();
            if (expected <= 0 || expected > 100000L || frames.LongLength != expected) return false;
            for (int index = 0; index < frames.Length; index++)
            {
                AuditionPvSelectedFrameScanEntry frame = frames[index];
                if (frame == null || frame.sourceFrame != take.sourceRangeStartFrame + index ||
                    !AuditionPvSha256.IsSha256(frame.frameSha256) || frame.width != Width ||
                    frame.height != Height || (id == "error-magenta" &&
                        frame.sampledPixelCount != (long)Width * Height)) return false;
            }
            return true;
        }

        private static string ExpectedScanAlgorithm(string id) => id switch
        {
            "error-magenta" => "full-frame-error-magenta-rgb255-0-255-v1",
            "renderer-material-scan" => "unity-runtime-renderer-material-inventory-v2",
            HudAbsentCheck => "capture-runtime-hud-workload-v3",
            _ => string.Empty
        };

        private static string ExpectedScanAlgorithmSha256(string id) =>
            ByteSha256(new UTF8Encoding(false, true).GetBytes(ExpectedScanAlgorithm(id)));

        private static bool ValidateRuntimeWorkload(string id,
            AuditionPvAutomatedCheckResultArtifact result,
            AuditionPvSelectedFrameScanLedgerArtifact ledger,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            string captureCoreSha256, AuditionPvSixtySecondValidationContext context,
            ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(result.runtimeWorkload, context, "TAKE_RUNTIME_WORKLOAD", at,
                    report, out AuditionPvRuntimeWorkloadArtifact workload,
                    new[] { capture.outputDirectory })) return false;
            string path;
            try { path = ResolveEvidencePath(result.runtimeWorkload.path, context,
                new[] { capture.outputDirectory }); }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
            if (!CaptureTestArtifactMatches(capture, AutomatedTestSuite,
                    id + "/runtime-workload", path, result.runtimeWorkload.sha256) ||
                workload == null || workload.schemaVersion != RuntimeWorkloadSchema ||
                workload.checkId != id || workload.captureId != take.sourceCaptureId ||
                workload.sourceCaptureCoreSha256 != captureCoreSha256 ||
                workload.sourceCaptureCoreSha256 != take.sourceCaptureCoreSha256 ||
                workload.sourceShotId != take.sourceShotId ||
                workload.sourceFrameLedgerSha256 != take.sourceFrameLedger?.sha256 ||
                workload.scanConfigSha256 != result.scanConfig?.sha256 ||
                workload.inventoryIdentityContract != (id == "renderer-material-scan"
                    ? "renderer-global-id/material-guid-local-id-sorted-v1"
                    : "canvas-global-id/hud-renderer-global-id-sorted-v1") ||
                !SameRange(workload, take) || string.IsNullOrWhiteSpace(workload.tool) ||
                string.IsNullOrWhiteSpace(workload.toolVersion) || !Utc(workload.createdAtUtc))
                return false;
            string hudMode = workload.hudEvidenceMode ?? string.Empty;
            if (id == HudAbsentCheck)
            {
                if (hudMode == "hud-authored-and-excluded")
                {
                    if (!PinIsEmpty(workload.sceneNoHudContractProof)) return false;
                }
                else if (hudMode == "scene-contract-no-hud")
                {
                    if (!ReadPinnedJson(workload.sceneNoHudContractProof, context,
                            "TAKE_SCENE_NO_HUD_CONTRACT", at, report,
                            out AuditionPvSceneNoHudContractArtifact noHud,
                            new[] { capture.outputDirectory })) return false;
                    string noHudPath;
                    try { noHudPath = ResolveEvidencePath(workload.sceneNoHudContractProof.path,
                        context, new[] { capture.outputDirectory }); }
                    catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
                    if (noHud == null || noHud.schemaVersion != SceneNoHudContractSchema ||
                        noHud.sourceCaptureCoreSha256 != captureCoreSha256 ||
                        noHud.sourceCaptureCoreSha256 != take.sourceCaptureCoreSha256 ||
                        noHud.captureId != take.sourceCaptureId ||
                        noHud.sourceShotId != take.sourceShotId || !noHud.noHudAuthored ||
                        noHud.inspectedObjectCount <= 0 || noHud.authoredHudComponentCount != 0 ||
                        string.IsNullOrWhiteSpace(noHud.tool) ||
                        string.IsNullOrWhiteSpace(noHud.toolVersion) || !Utc(noHud.createdAtUtc) ||
                        !CaptureTestArtifactMatches(capture, AutomatedTestSuite,
                            "hud-layer-absent/scene-contract-no-hud", noHudPath,
                            workload.sceneNoHudContractProof.sha256)) return false;
                }
                else return false;
            }
            else if (!string.IsNullOrEmpty(hudMode) ||
                     !PinIsEmpty(workload.sceneNoHudContractProof)) return false;
            AuditionPvRuntimeFrameWorkload[] values = workload.frames ??
                Array.Empty<AuditionPvRuntimeFrameWorkload>();
            AuditionPvSelectedFrameScanEntry[] frames = ledger.frames ??
                Array.Empty<AuditionPvSelectedFrameScanEntry>();
            return RuntimeWorkloadFramesMatch(id, values, frames, hudMode);
        }

        internal static bool RuntimeWorkloadFramesMatch(string id,
            AuditionPvRuntimeFrameWorkload[] values, AuditionPvSelectedFrameScanEntry[] frames) =>
            RuntimeWorkloadFramesMatch(id, values, frames, "hud-authored-and-excluded");

        internal static bool RuntimeWorkloadFramesMatch(string id,
            AuditionPvRuntimeFrameWorkload[] values, AuditionPvSelectedFrameScanEntry[] frames,
            string hudEvidenceMode)
        {
            values ??= Array.Empty<AuditionPvRuntimeFrameWorkload>();
            frames ??= Array.Empty<AuditionPvSelectedFrameScanEntry>();
            if (values.Length != frames.Length || values.Length == 0) return false;
            var state = new RuntimeWorkloadValidationState();
            for (int index = 0; index < values.Length; index++)
            {
                if (!RuntimeWorkloadFrameMatches(
                        id,
                        values[index],
                        frames[index],
                        hudEvidenceMode,
                        state)) return false;
            }
            return true;
        }

        internal static bool RuntimeWorkloadFrameMatches(
            string id,
            AuditionPvRuntimeFrameWorkload value,
            AuditionPvSelectedFrameScanEntry frame,
            string hudEvidenceMode,
            RuntimeWorkloadValidationState state)
        {
            if (value == null || frame == null || state == null ||
                value.sourceFrame != frame.sourceFrame) return false;
            if (id == "renderer-material-scan")
            {
                return StableInventoryCarryForwardValid(
                           "renderers",
                           value.rendererStableIds,
                           value.rendererAddedStableIds,
                           value.rendererRemovedStableIds,
                           value.inspectedRendererCount,
                           value.rendererInventorySha256,
                           false,
                           state.renderers) &&
                       StableInventoryCarryForwardValid(
                           "material-slots",
                           value.materialSlotStableIds,
                           value.materialSlotAddedStableIds,
                           value.materialSlotRemovedStableIds,
                           value.inspectedMaterialSlotCount,
                           value.materialInventorySha256,
                           false,
                           state.materialSlots) &&
                       value.nullMaterialCount == 0 && value.errorMaterialCount == 0 &&
                       frame.inspectedRendererCount == value.inspectedRendererCount &&
                       frame.inspectedMaterialSlotCount == value.inspectedMaterialSlotCount &&
                       frame.rendererInventorySha256 == value.rendererInventorySha256 &&
                       frame.materialInventorySha256 == value.materialInventorySha256 &&
                       frame.nullMaterialCount == value.nullMaterialCount &&
                       frame.errorMaterialCount == value.errorMaterialCount;
            }

            bool authored = hudEvidenceMode == "hud-authored-and-excluded";
            bool sceneNoHud = hudEvidenceMode == "scene-contract-no-hud";
            return (authored || sceneNoHud) && value.inspectedDrawCommandCount > 0 &&
                   value.visibleUiElementCount == 0 &&
                   StableInventoryCarryForwardValid(
                       "canvases",
                       value.canvasStableIds,
                       value.canvasAddedStableIds,
                       value.canvasRemovedStableIds,
                       value.inspectedCanvasCount,
                       value.canvasInventorySha256,
                       sceneNoHud,
                       state.canvases) &&
                   StableInventoryCarryForwardValid(
                       "hud-renderers",
                       value.hudRendererStableIds,
                       value.hudRendererAddedStableIds,
                       value.hudRendererRemovedStableIds,
                       value.inspectedHudRendererCount,
                       value.hudInventorySha256,
                       sceneNoHud,
                       state.hudRenderers) &&
                   (!authored || value.inspectedCanvasCount > 0 &&
                    value.inspectedHudRendererCount > 0) &&
                   (!sceneNoHud || value.inspectedCanvasCount == 0 &&
                    value.inspectedHudRendererCount == 0) &&
                   frame.inspectedCanvasCount == value.inspectedCanvasCount &&
                   frame.inspectedHudRendererCount == value.inspectedHudRendererCount &&
                   frame.inspectedDrawCommandCount == value.inspectedDrawCommandCount &&
                   frame.visibleUiElementCount == value.visibleUiElementCount &&
                   frame.canvasInventorySha256 == value.canvasInventorySha256 &&
                   frame.hudInventorySha256 == value.hudInventorySha256 &&
                   frame.rendererHudLayerExcluded;
        }

        internal sealed class RuntimeWorkloadValidationState
        {
            internal readonly RuntimeInventoryIdentity renderers = new();
            internal readonly RuntimeInventoryIdentity materialSlots = new();
            internal readonly RuntimeInventoryIdentity canvases = new();
            internal readonly RuntimeInventoryIdentity hudRenderers = new();
        }

        internal sealed class RuntimeInventoryIdentity
        {
            internal bool hasFullSnapshot;
            internal long count;
            internal string sha256 = string.Empty;
            internal string[] stableIds = Array.Empty<string>();
        }

        private static bool StableInventoryCarryForwardValid(
            string domain,
            string[] ids,
            string[] addedIds,
            string[] removedIds,
            long declaredCount,
            string declaredSha256,
            bool allowEmpty,
            RuntimeInventoryIdentity state)
        {
            ids ??= Array.Empty<string>();
            addedIds ??= Array.Empty<string>();
            removedIds ??= Array.Empty<string>();
            bool hasDelta = addedIds.Length > 0 || removedIds.Length > 0;
            if (hasDelta)
            {
                if (ids.Length != 0 || !state.hasFullSnapshot ||
                    !SortedUniqueInventoryDelta(addedIds) ||
                    !SortedUniqueInventoryDelta(removedIds) ||
                    addedIds.Length > 4096 || removedIds.Length > 4096 ||
                    addedIds.Intersect(removedIds, StringComparer.Ordinal).Any())
                    return false;
                string[] resolved = ApplyInventoryDelta(
                    state.stableIds,
                    addedIds,
                    removedIds);
                if (resolved == null || !StableInventoryValid(
                        domain,
                        resolved,
                        declaredCount,
                        declaredSha256,
                        allowEmpty)) return false;
                state.hasFullSnapshot = true;
                state.count = declaredCount;
                state.sha256 = declaredSha256;
                state.stableIds = resolved;
                return true;
            }

            bool emptyIsFullSnapshot = allowEmpty && declaredCount == 0;
            if (ids.Length == 0 && !emptyIsFullSnapshot)
            {
                return state.hasFullSnapshot && state.count == declaredCount &&
                       string.Equals(
                           state.sha256,
                           declaredSha256,
                           StringComparison.Ordinal);
            }

            if (!StableInventoryValid(
                    domain,
                    ids,
                    declaredCount,
                    declaredSha256,
                    allowEmpty)) return false;
            state.hasFullSnapshot = true;
            state.count = declaredCount;
            state.sha256 = declaredSha256;
            state.stableIds = ids;
            return true;
        }

        private static bool SortedUniqueInventoryDelta(string[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(values[index]) ||
                    index > 0 && string.CompareOrdinal(values[index - 1], values[index]) >= 0)
                    return false;
            }
            return true;
        }

        private static string[] ApplyInventoryDelta(
            string[] previous,
            string[] added,
            string[] removed)
        {
            previous ??= Array.Empty<string>();
            int survivorCount = previous.Length - removed.Length;
            int resolvedCount = survivorCount + added.Length;
            if (survivorCount < 0 || resolvedCount > 4096) return null;
            var survivors = new string[survivorCount];
            int previousIndex = 0;
            int removedIndex = 0;
            int survivorIndex = 0;
            while (previousIndex < previous.Length)
            {
                if (removedIndex < removed.Length)
                {
                    int comparison = string.CompareOrdinal(
                        previous[previousIndex],
                        removed[removedIndex]);
                    if (comparison > 0) return null;
                    if (comparison == 0)
                    {
                        previousIndex++;
                        removedIndex++;
                        continue;
                    }
                }
                if (survivorIndex >= survivors.Length) return null;
                survivors[survivorIndex++] = previous[previousIndex++];
            }
            if (removedIndex != removed.Length || survivorIndex != survivors.Length)
                return null;

            var resolved = new string[resolvedCount];
            int addedIndex = 0;
            survivorIndex = 0;
            int resolvedIndex = 0;
            while (survivorIndex < survivors.Length || addedIndex < added.Length)
            {
                if (survivorIndex >= survivors.Length)
                {
                    resolved[resolvedIndex++] = added[addedIndex++];
                    continue;
                }
                if (addedIndex >= added.Length)
                {
                    resolved[resolvedIndex++] = survivors[survivorIndex++];
                    continue;
                }
                int comparison = string.CompareOrdinal(
                    survivors[survivorIndex],
                    added[addedIndex]);
                if (comparison == 0) return null;
                resolved[resolvedIndex++] = comparison < 0
                    ? survivors[survivorIndex++]
                    : added[addedIndex++];
            }
            return resolvedIndex == resolved.Length ? resolved : null;
        }

        private static bool StableInventoryValid(string domain, string[] ids, long declaredCount,
            string declaredSha256, bool allowEmpty)
        {
            ids ??= Array.Empty<string>();
            if (ids.Length > 4096 || declaredCount != ids.LongLength ||
                !allowEmpty && ids.Length == 0 || ids.Any(string.IsNullOrWhiteSpace) ||
                ids.Distinct(StringComparer.Ordinal).Count() != ids.Length ||
                !ids.SequenceEqual(ids.OrderBy(value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal)) return false;
            try { return declaredSha256 == StableInventorySha256(domain, ids); }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        internal static string StableInventorySha256(string domain, string[] ids)
        {
            using var stream = new MemoryStream();
            WriteCoreString(stream, "dimension-brawl.audition-pv.stable-inventory.v1");
            WriteCoreString(stream, domain);
            WriteCoreInt(stream, (ids ?? Array.Empty<string>()).Length);
            foreach (string id in ids ?? Array.Empty<string>()) WriteCoreString(stream, id);
            return ByteSha256(stream.ToArray());
        }

        private static bool PinIsEmpty(AuditionPvPinnedArtifact value) => value == null ||
            string.IsNullOrWhiteSpace(value.path) && string.IsNullOrWhiteSpace(value.sha256);

        private static bool TryCountErrorMagentaPixels(string path, out long count)
        {
            count = 0;
            if (!TryLoadPngPixels(path, MaxQhdPngBytes, Width, Height,
                    out Color32[] pixels)) return false;
            count = CountErrorMagentaPixels(pixels);
            return true;
        }

        internal static long CountErrorMagentaPixels(Color32[] pixels)
        {
            long count = 0;
            foreach (Color32 pixel in pixels ?? Array.Empty<Color32>())
                if (pixel.r == 255 && pixel.g == 0 && pixel.b == 255) count++;
            return count;
        }

        internal static bool DecodedMagentaCountMatches(string path, int width, int height,
            long expected)
        {
            return width > 0 && height > 0 && expected >= 0 &&
                TryLoadPngPixels(path, MaxQhdPngBytes, width, height, out Color32[] pixels) &&
                CountErrorMagentaPixels(pixels) == expected;
        }

        internal static bool Rec709EvidenceShapeValid(AuditionPvAutomatedCheckResultArtifact result) =>
            result != null && result.colorPrimaries == "bt709" &&
            result.transferCharacteristics == "bt709" &&
            result.matrixCoefficients == "identity-rgb" && result.signalRange == "full" &&
            result.transformId == Rec709TransformId &&
            !string.IsNullOrWhiteSpace(result.parserName) &&
            !string.IsNullOrWhiteSpace(result.parserVersion) &&
            !string.IsNullOrWhiteSpace(result.rec709Config?.path) &&
            AuditionPvSha256.IsSha256(result.rec709Config?.sha256) &&
            !string.IsNullOrWhiteSpace(result.rec709OutputLedger?.path) &&
            AuditionPvSha256.IsSha256(result.rec709OutputLedger?.sha256);

        private static bool ValidateRec709Evidence(AuditionPvAutomatedCheckResultArtifact result,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report, string at)
        {
            if (!Rec709EvidenceShapeValid(result) ||
                !ReadPinnedJson(result.rec709Config, context, "TAKE_REC709_CONFIG", at,
                    report, out AuditionPvRec709TransformArtifact transform,
                    context.allowedReviewRoots) ||
                !ReadPinnedJson(result.rec709OutputLedger, context, "TAKE_REC709_OUTPUT_LEDGER", at,
                    report, out AuditionPvRec709OutputLedgerArtifact ledger,
                    context.allowedReviewRoots)) return false;
            bool valid = transform.schemaVersion == Rec709TransformSchema &&
                capture.sourceFormat == AuditionPvCaptureContract.SourceFormat &&
                transform.transformId == Rec709TransformId &&
                transform.transformId == result.transformId &&
                transform.captureId == take.sourceCaptureId &&
                transform.sourceCaptureCoreSha256 == CaptureCoreSha256(capture) &&
                transform.sourceCaptureCoreSha256 == take.sourceCaptureCoreSha256 &&
                transform.sourceShotId == take.sourceShotId &&
                transform.sourceFrameLedgerSha256 == take.sourceFrameLedger?.sha256 &&
                SameRange(transform, take) && transform.colorPrimaries == result.colorPrimaries &&
                transform.transferCharacteristics == result.transferCharacteristics &&
                transform.matrixCoefficients == result.matrixCoefficients &&
                transform.signalRange == result.signalRange &&
                transform.inputProfile == "iec-61966-2-1-srgb8" &&
                transform.outputProfile == "itu-r-bt709-oetf-rgba8" &&
                transform.roundingMode == "nearest-away-from-zero-u8" &&
                transform.alphaMode == "copy-exact" &&
                transform.editorialSourceRole == "canonical-approved-edit-original" &&
                transform.parserName == result.parserName &&
                transform.parserVersion == result.parserVersion &&
                !string.IsNullOrWhiteSpace(transform.tool) &&
                !string.IsNullOrWhiteSpace(transform.toolVersion) && Utc(transform.createdAtUtc) &&
                ledger != null && ledger.schemaVersion == Rec709OutputLedgerSchema &&
                ledger.captureId == take.sourceCaptureId &&
                ledger.sourceCaptureCoreSha256 == CaptureCoreSha256(capture) &&
                ledger.sourceCaptureCoreSha256 == take.sourceCaptureCoreSha256 &&
                ledger.sourceShotId == take.sourceShotId &&
                ledger.sourceFrameLedgerSha256 == take.sourceFrameLedger?.sha256 &&
                ledger.configSha256 == result.rec709Config.sha256 && SameRange(ledger, take);
            valid &= Rec709OutputLedgerTopologyValid(ledger, take,
                result.rec709Config.sha256, transform);
            long expectedCount = (long)take.sourceRangeEndFrame -
                take.sourceRangeStartFrame + 1L;
            AuditionPvRec709OutputFrame[] frames = ledger?.frames ??
                Array.Empty<AuditionPvRec709OutputFrame>();
            if (expectedCount <= 0 || expectedCount > 100000L || frames.LongLength != expectedCount)
                return false;
            var outputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < frames.Length; index++)
            {
                AuditionPvRec709OutputFrame frame = frames[index];
                int expectedFrame;
                try { expectedFrame = checked(take.sourceRangeStartFrame + index); }
                catch (OverflowException) { valid = false; break; }
                if (frame == null || frame.sourceFrame != expectedFrame ||
                    !AuditionPvSha256.IsSha256(frame.sourceFrameSha256) ||
                    !AuditionPvSha256.IsSha256(frame.outputSha256) || frame.width != Width ||
                    frame.height != Height || frame.colorPrimaries != transform.colorPrimaries ||
                    frame.transferCharacteristics != transform.transferCharacteristics ||
                    frame.matrixCoefficients != transform.matrixCoefficients ||
                    frame.signalRange != transform.signalRange)
                { valid = false; continue; }
                string sourcePath = CanonicalSourceFramePath(capture, take.sourceShotId, expectedFrame);
                string outputPath = string.Empty;
                try
                {
                    RequireUnder(sourcePath, new[] { capture.outputDirectory }, "Rec.709 source frame");
                    RejectReparseChain(sourcePath);
                    outputPath = ResolveEvidencePath(frame.outputPath, context,
                        context.allowedGraphicsRoots);
                    if (!CanonicalRec709OutputPath(outputPath, context.allowedGraphicsRoots,
                            take.sourceCaptureId, take.sourceShotId, expectedFrame) ||
                        !outputs.Add(Normalize(outputPath)) || PathsEqual(sourcePath, outputPath) ||
                        !File.Exists(sourcePath) || !File.Exists(outputPath) ||
                        AuditionPvSha256.FileHash(sourcePath) != frame.sourceFrameSha256 ||
                        AuditionPvSha256.FileHash(outputPath) != frame.outputSha256 ||
                        !Rec709PhysicalTransformMatches(sourcePath, outputPath)) valid = false;
                    else
                    {
                        RememberFinalFile(context, sourcePath, frame.sourceFrameSha256,
                            new FileInfo(sourcePath).Length, report, at);
                        RememberFinalFile(context, outputPath, frame.outputSha256,
                            new FileInfo(outputPath).Length, report, at);
                    }
                }
                catch (Exception exception) when (IsPathOrIo(exception)) { valid = false; }
            }
            return valid;
        }

        internal static bool Rec709OutputLedgerTopologyValid(
            AuditionPvRec709OutputLedgerArtifact ledger,
            AuditionPvSixtySecondTakeCandidate take, string configSha256,
            AuditionPvRec709TransformArtifact config)
        {
            if (ledger == null || take == null || config == null ||
                ledger.configSha256 != configSha256 || !SameRange(ledger, take)) return false;
            long expected = (long)take.sourceRangeEndFrame -
                take.sourceRangeStartFrame + 1L;
            AuditionPvRec709OutputFrame[] frames = ledger.frames ??
                Array.Empty<AuditionPvRec709OutputFrame>();
            if (expected <= 0 || expected > 100000L || frames.LongLength != expected) return false;
            var outputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < frames.Length; index++)
            {
                AuditionPvRec709OutputFrame frame = frames[index];
                if (frame == null || frame.sourceFrame != take.sourceRangeStartFrame + index ||
                    !AuditionPvSha256.IsSha256(frame.sourceFrameSha256) ||
                    !AuditionPvSha256.IsSha256(frame.outputSha256) ||
                    string.IsNullOrWhiteSpace(frame.outputPath) ||
                    !outputs.Add(Normalize(frame.outputPath)) || frame.width != Width ||
                    frame.height != Height || frame.colorPrimaries != config.colorPrimaries ||
                    frame.transferCharacteristics != config.transferCharacteristics ||
                    frame.matrixCoefficients != config.matrixCoefficients ||
                    frame.signalRange != config.signalRange) return false;
            }
            return true;
        }

        private static bool Rec709PhysicalTransformMatches(string sourcePath, string outputPath)
            => DecodedRec709TransformMatches(sourcePath, outputPath, Width, Height);

        internal static bool DecodedRec709TransformMatches(string sourcePath, string outputPath,
            int width, int height)
        {
            if (width <= 0 || height <= 0 ||
                !TryExpectedRec709RawDigest(sourcePath, width, height,
                    out string expected) ||
                !TryRawPngDigest(outputPath, width, height, out string actual)) return false;
            return expected == actual;
        }

        // Keep source and output decoding in separate call frames so QHD verification never
        // retains two decoded PNG arrays at once.
        private static bool TryExpectedRec709RawDigest(string path, int width, int height,
            out string digest)
        {
            digest = string.Empty;
            if (!TryLoadPngPixels(path, MaxQhdPngBytes, width, height,
                    out Color32[] pixels)) return false;
            digest = RawPixelDigest(pixels, applyRec709Transform: true);
            return true;
        }

        private static bool TryRawPngDigest(string path, int width, int height,
            out string digest)
        {
            digest = string.Empty;
            if (!TryLoadPngPixels(path, MaxQhdPngBytes, width, height,
                    out Color32[] pixels)) return false;
            digest = RawPixelDigest(pixels, applyRec709Transform: false);
            return true;
        }

        private static string RawPixelDigest(Color32[] pixels, bool applyRec709Transform)
        {
            const int ChunkBytes = 64 * 1024;
            using var sha = System.Security.Cryptography.SHA256.Create();
            var chunk = new byte[ChunkBytes];
            int used = 0;
            foreach (Color32 value in pixels ?? Array.Empty<Color32>())
            {
                if (used + 4 > chunk.Length)
                {
                    sha.TransformBlock(chunk, 0, used, null, 0);
                    used = 0;
                }
                chunk[used++] = applyRec709Transform ? Srgb8ToRec709Lut[value.r] : value.r;
                chunk[used++] = applyRec709Transform ? Srgb8ToRec709Lut[value.g] : value.g;
                chunk[used++] = applyRec709Transform ? Srgb8ToRec709Lut[value.b] : value.b;
                chunk[used++] = value.a;
            }
            sha.TransformFinalBlock(chunk, 0, used);
            return string.Concat((sha.Hash ?? Array.Empty<byte>()).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        internal static bool Rec709PixelTransformMatches(Color32[] source, Color32[] output)
        {
            if (source == null || output == null || source.Length == 0 ||
                source.Length != output.Length) return false;
            for (int index = 0; index < source.Length; index++)
            {
                Color32 left = source[index], right = output[index];
                if (right.r != Srgb8ToRec709Lut[left.r] ||
                    right.g != Srgb8ToRec709Lut[left.g] ||
                    right.b != Srgb8ToRec709Lut[left.b] || right.a != left.a) return false;
            }
            return true;
        }

        internal static byte TransformSrgb8ToRec709(byte value) => Srgb8ToRec709Lut[value];

        private static byte[] BuildSrgb8ToRec709Lut()
        {
            var result = new byte[256];
            for (int code = 0; code < result.Length; code++)
            {
                double encoded = code / 255d;
                double linear = encoded <= 0.04045d
                    ? encoded / 12.92d
                    : Math.Pow((encoded + 0.055d) / 1.055d, 2.4d);
                double rec709 = linear < 0.018d
                    ? 4.5d * linear
                    : 1.099d * Math.Pow(linear, 0.45d) - 0.099d;
                double clamped = Math.Max(0d, Math.Min(1d, rec709));
                result[code] = (byte)Math.Floor(clamped * 255d + 0.5d);
            }
            return result;
        }

        private static bool CanonicalRec709OutputPath(string path, IEnumerable<string> roots,
            string captureId, string sourceShotId, int frame)
        {
            if (!SafePathComponent(captureId) || !SafePathComponent(sourceShotId)) return false;
            foreach (string root in roots ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(root)) continue;
                string expected = Path.Combine(root, "rec709", captureId, sourceShotId,
                    $"frame_{frame:0000}.png");
                if (PathsEqual(path, expected)) return true;
            }
            return false;
        }

        internal static bool CanonicalRec709OutputPathForTest(string path,
            IEnumerable<string> roots, string captureId, string sourceShotId, int frame)
        {
            try { return CanonicalRec709OutputPath(path, roots, captureId, sourceShotId, frame); }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static bool SafePathComponent(string value) =>
            !string.IsNullOrWhiteSpace(value) && value != "." && value != ".." &&
            value.IndexOfAny(new[] { '/', '\\', ':' }) < 0;

        private static bool ValidateMeasuredFrames(AuditionPvAutomatedCheckResultArtifact result,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            ReportBuilder report, string at, out string[] hashes)
            => ValidatePhysicalMeasuredFrames(result.sampledFrames, take, capture, report, at,
                "TAKE_AUTOMATED_SAMPLED_FRAMES_INVALID", out hashes);

        private static bool ValidatePhysicalMeasuredFrames(AuditionPvMeasuredFrame[] declared,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            ReportBuilder report, string at, string errorCode, out string[] hashes,
            bool fullSourceRange = false)
        {
            int[] expected = RequiredMeasuredFrames(take, fullSourceRange);
            declared ??= Array.Empty<AuditionPvMeasuredFrame>();
            var values = new List<string>();
            bool valid = declared.Length == expected.Length;
            for (int index = 0; index < declared.Length; index++)
            {
                AuditionPvMeasuredFrame value = declared[index];
                if (value == null || index >= expected.Length || value.sourceFrame != expected[index] ||
                    !AuditionPvSha256.IsSha256(value.frameSha256))
                { valid = false; continue; }
                string path = CanonicalSourceFramePath(capture, take.sourceShotId,
                    value.sourceFrame);
                try
                {
                    RequireUnder(path, new[] { capture.outputDirectory }, "measured source frame");
                    RejectReparseChain(path);
                    if (!File.Exists(path) || AuditionPvSha256.FileHash(path) != value.frameSha256 ||
                        !IsDecodedPngDimensions(path, Width, Height)) valid = false;
                }
                catch (Exception exception) when (IsPathOrIo(exception)) { valid = false; }
                values.Add(value.frameSha256);
            }
            hashes = values.ToArray();
            if (!valid) report.Error(errorCode, at,
                "Deterministic first/last and one-per-second source frame hashes must match physical PNGs for the required range.");
            return valid;
        }

        private static int[] RequiredMeasuredFrames(AuditionPvSixtySecondTakeCandidate take,
            bool fullSourceRange) => take == null ? Array.Empty<int>() : fullSourceRange
            ? SampledFrames(take.sourceRangeStartFrame, take.sourceRangeEndFrame)
            : SampledFrames(take.selectStartFrame, take.selectEndFrame);

        internal static int[] RequiredHumanReviewFramesForTest(
            AuditionPvSixtySecondTakeCandidate take) => RequiredMeasuredFrames(take, true);

        private static bool PinnedExactSourceFrame(AuditionPvPinnedArtifact pin, int sourceFrame,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvCaptureManifest capture,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report, string at)
        {
            if (sourceFrame < take.selectStartFrame || sourceFrame > take.selectEndFrame ||
                !Pinned(pin, context, "TAKE_MEASURED_SOURCE_FRAME", at, report, out string path,
                    new[] { capture.outputDirectory })) return false;
            string expected = CanonicalSourceFramePath(capture, take.sourceShotId, sourceFrame);
            return PathsEqual(path, expected) && IsDecodedPngDimensions(path, Width, Height);
        }

        private static int[] SampledFrames(int start, int end)
        {
            var result = new List<int>();
            if (start < 0 || end < start || (long)end - start + 1L > 100000L)
                return Array.Empty<int>();
            for (long frame = start; frame <= end; frame += Fps)
                result.Add(checked((int)frame));
            if (result.Count == 0 || result[result.Count - 1] != end) result.Add(end);
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

        private static string[] SampledSourcePaths(AuditionPvSixtySecondTakeCandidate take,
            AuditionPvCaptureManifest capture) => SampledFrames(take.selectStartFrame,
                take.selectEndFrame).Select(frame => CanonicalSourceFramePath(capture,
                    take.sourceShotId, frame)).ToArray();

        private static void ValidateTakeReview(AuditionPvSixtySecondSequenceBucket bucket,
            AuditionPvSixtySecondAtomicShot shot, AuditionPvSixtySecondTakeCandidate take,
            AuditionPvCaptureManifest capture, AuditionPvSixtySecondValidationContext context,
            ReportBuilder report, string at)
        {
            if (!ReadPinnedJson(take.humanReview, context, "TAKE_HUMAN_REVIEW", at,
                    report, out AuditionPvTakeHumanReviewArtifact review,
                    context.allowedReviewRoots)) return;
            if (review.schemaVersion != TakeReviewSchema || !review.approved ||
                !review.fullMotionRangeReviewed || !review.noBlackMesh || !review.noBrokenTrail ||
                review.takeId != take.takeId || review.captureId != take.sourceCaptureId ||
                review.sourceManifestSha256 != take.sourceManifestSha256 ||
                review.sourceShotId != take.sourceShotId || review.bucketId != bucket.bucketId ||
                review.atomicShotId != shot.shotId || !SameRange(review, take) ||
                !(review.beatIds ?? Array.Empty<string>()).OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual((shot.beatIds ?? Array.Empty<string>())
                        .OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal) ||
                string.IsNullOrWhiteSpace(review.reviewedBy) || !Utc(review.reviewedAtUtc))
                report.Error("TAKE_HUMAN_REVIEW_INVALID", at,
                    "The approved take requires a pinned identity- and beat-bound human review.");
            if ((review.reviewedFrames ?? Array.Empty<AuditionPvMeasuredFrame>()).Length > 128)
                report.Error("TAKE_HUMAN_REVIEW_CARDINALITY_EXCEEDED", at,
                    "At most 128 deterministic review frames are accepted per take.");
            ValidatePhysicalMeasuredFrames(review.reviewedFrames, take, capture, report, at,
                "TAKE_HUMAN_REVIEW_FRAMES_INVALID", out _, true);
        }

        private static bool CaptureTestArtifactMatches(AuditionPvCaptureManifest capture,
            string suite, string name, string expectedPath, string expectedSha256)
        {
            if (string.IsNullOrWhiteSpace(suite) || string.IsNullOrWhiteSpace(name) ||
                !AuditionPvSha256.IsSha256(expectedSha256)) return false;
            foreach (AuditionPvTestResult test in capture.testResults ?? Array.Empty<AuditionPvTestResult>())
            {
                if (test == null || test.status != "passed" || test.suite != suite || test.name != name ||
                    string.IsNullOrWhiteSpace(test.artifactPath) ||
                    !TestDetailsPinArtifact(test.details, expectedSha256)) continue;
                try
                {
                    string path = Path.IsPathRooted(test.artifactPath)
                        ? Path.GetFullPath(test.artifactPath)
                        : Path.GetFullPath(Path.Combine(capture.outputDirectory, test.artifactPath));
                    RequireUnder(path, new[] { capture.outputDirectory }, "capture test artifact");
                    if (PathsEqual(path, expectedPath)) return true;
                }
                catch (Exception exception) when (IsPathOrIo(exception)) { }
            }
            return false;
        }

        internal static bool CaptureTestArtifactMatchesForTest(AuditionPvCaptureManifest capture,
            string suite, string name, string expectedPath, string expectedSha256) =>
            CaptureTestArtifactMatches(capture, suite, name, expectedPath, expectedSha256);

        private static bool CaptureSemanticBeatArtifactMatches(AuditionPvCaptureManifest capture,
            AuditionPvSemanticBeatProof beat, string expectedPath)
        {
            if (beat == null || !AuditionPvSha256.IsSha256(beat.runtimeProof?.sha256)) return false;
            foreach (AuditionPvTestResult test in capture?.testResults ??
                     Array.Empty<AuditionPvTestResult>())
            {
                if (test == null || test.status != "passed" ||
                    test.suite != beat.supportingTestSuite || test.name != beat.supportingTestName ||
                    !TestDetailsPinArtifact(test.details, beat.runtimeProof.sha256) ||
                    !TestDetailsPinToken(test.details, "semantic-fact=" + beat.runtimeFactKey)) continue;
                try
                {
                    string path = Path.IsPathRooted(test.artifactPath)
                        ? Path.GetFullPath(test.artifactPath)
                        : Path.GetFullPath(Path.Combine(capture.outputDirectory, test.artifactPath));
                    RequireUnder(path, new[] { capture.outputDirectory }, "semantic beat artifact");
                    RejectReparseChain(path);
                    if (PathsEqual(path, expectedPath)) return true;
                }
                catch (Exception exception) when (IsPathOrIo(exception)) { }
            }
            return false;
        }

        private static bool TestDetailsPinArtifact(string details, string sha256) =>
            (details ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n', ';', ',' },
                StringSplitOptions.RemoveEmptyEntries)
            .Contains("artifact-sha256=" + sha256, StringComparer.Ordinal);

        private static bool TestDetailsPinToken(string details, string token) =>
            (details ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n', ';', ',' },
                StringSplitOptions.RemoveEmptyEntries).Contains(token, StringComparer.Ordinal);

        private static void ValidateGateEvidenceProduction(
            AuditionPvSixtySecondShotGateManifest manifest,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            AuditionPvSixtySecondGateEvidence evidence = manifest.gateEvidence;
            if (evidence == null) return;
            string packageDirectory;
            try
            {
                packageDirectory = ResolveEvidenceDirectory(evidence.twelveSecondPackageDirectory,
                    context, context.allowedSelectRoots);
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error("TWELVE_SECOND_PACKAGE_PATH_INVALID", "gateEvidence", exception.Message);
                packageDirectory = string.Empty;
            }
            if (!string.IsNullOrEmpty(packageDirectory))
            {
                string manifestPath = Path.Combine(packageDirectory,
                    AuditionPvTwelveSecondGoldAssembler.ManifestFileName);
                string validationPath = Path.Combine(packageDirectory,
                    AuditionPvTwelveSecondGoldAssembler.ValidationReportFileName);
                bool packagePins = PinnedExact(manifestPath, evidence.twelveSecondManifestSha256,
                        "TWELVE_SECOND_MANIFEST", "gateEvidence", context, packageDirectory, report) &
                    PinnedExact(validationPath, evidence.twelveSecondValidationSha256,
                        "TWELVE_SECOND_VALIDATION", "gateEvidence", context, packageDirectory, report);
                if (packagePins && (new FileInfo(manifestPath).Length > MaxManifestJsonBytes ||
                                    new FileInfo(validationPath).Length > MaxEvidenceJsonBytes))
                {
                    report.Error("TWELVE_SECOND_PACKAGE_JSON_TOO_LARGE", "gateEvidence",
                        "Installed 12-second manifest/report exceed accepted JSON byte limits.");
                    packagePins = false;
                }
                if (packagePins)
                {
                    try
                    {
                        AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(packageDirectory);
                        ValidateTwelveSecondSources(manifest, packageDirectory, context, report);
                    }
                    catch (Exception exception)
                    {
                        report.Error("TWELVE_SECOND_PACKAGE_INVALID", "gateEvidence", exception.Message);
                    }
                }
            }
            if (ReadPinnedJson(evidence.twelveSecondApproval, context, "TWELVE_SECOND_APPROVAL",
                    "gateEvidence", report, out AuditionPvTwelveSecondApprovalArtifact approval,
                    context.allowedReviewRoots))
            {
                if (approval.schemaVersion != TwelveSecondApprovalSchema || !approval.approved ||
                    approval.manifestId != manifest.manifestId ||
                    approval.twelveSecondManifestSha256 != evidence.twelveSecondManifestSha256 ||
                    string.IsNullOrWhiteSpace(approval.approvedBy) || !Utc(approval.approvedAtUtc))
                    report.Error("TWELVE_SECOND_APPROVAL_INVALID", "gateEvidence",
                        "12-second gold requires a typed pinned approval.");
            }
            if (ReadPinnedJson(evidence.visualReview, context, "VISUAL_REVIEW", "gateEvidence",
                    report, out AuditionPvVisualReviewArtifact visual,
                    context.allowedReviewRoots))
            {
                if ((visual.criterionRefs ?? Array.Empty<AuditionPvVisualCriterionRef>()).Length > 16 ||
                    (visual.reviewedFrameSha256 ?? Array.Empty<string>()).Length > MaxPreviewCells ||
                    (visual.contactSheetInputSha256 ?? Array.Empty<string>()).Length > MaxPreviewCells)
                {
                    report.Error("VISUAL_REVIEW_CARDINALITY_EXCEEDED", "gateEvidence",
                        "Visual review rows must fit the canonical 32-cell preview contract.");
                    return;
                }
                AuditionPvSixtySecondAtomicShot[] approvedShots = (manifest.buckets ??
                        Array.Empty<AuditionPvSixtySecondSequenceBucket>())
                    .Where(bucket => bucket != null)
                    .SelectMany(bucket => bucket.shots ?? Array.Empty<AuditionPvSixtySecondAtomicShot>())
                    .Where(shot => shot != null && shot.sourceKind != "end-card").ToArray();
                AuditionPvSixtySecondTakeCandidate[] approvedTakes = approvedShots
                    .Select(shot => (shot.candidateTakes ??
                            Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                        .FirstOrDefault(take => take != null && take.takeId == shot.approvedTakeId))
                    .Where(take => take != null).ToArray();
                string[] expectedReviews = approvedTakes
                    .Select(take => take.humanReview?.sha256 ?? string.Empty)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] reviewed = (visual.approvedTakeReviewSha256 ?? Array.Empty<string>())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                // Final S100 artwork/readability belongs to AE/picture-lock. The edit-start
                // visual Gate reviews selected moving-image inputs only; S100 carries a
                // separately rights-pinned placeholder/status contract.
                string[] orderedGraphics = Array.Empty<string>();
                string[] expectedGraphics = Array.Empty<string>();
                string[] reviewedGraphics = (visual.approvedEndCardGraphicSha256 ?? Array.Empty<string>())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                var orderedFrameHashes = new List<string>();
                var orderedSourcePaths = new List<string>();
                var orderedFrameKeys = new List<string>();
                var approvedShotByTake = approvedShots
                    .Where(shot => !string.IsNullOrWhiteSpace(shot.approvedTakeId))
                    .GroupBy(shot => shot.approvedTakeId, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
                var reviewedFramesByTake = new Dictionary<string, AuditionPvMeasuredFrame[]>(
                    StringComparer.Ordinal);
                foreach (AuditionPvSixtySecondTakeCandidate take in approvedTakes)
                    if (ReadPinnedJson(take.humanReview, context, "VISUAL_SOURCE_TAKE_REVIEW",
                            "gateEvidence", report, out AuditionPvTakeHumanReviewArtifact takeReview,
                            context.allowedReviewRoots) &&
                        ReadPinnedJson(take.sourceManifest, context, "VISUAL_SOURCE_MANIFEST",
                            "gateEvidence", report, out AuditionPvCaptureManifest sourceCapture,
                            context.allowedCaptureRoots) && sourceCapture != null &&
                        !string.IsNullOrWhiteSpace(sourceCapture.outputDirectory))
                    {
                        reviewedFramesByTake[take.takeId] = takeReview.reviewedFrames ??
                            Array.Empty<AuditionPvMeasuredFrame>();
                        foreach (AuditionPvMeasuredFrame frame in takeReview.reviewedFrames ??
                                     Array.Empty<AuditionPvMeasuredFrame>())
                            if (frame != null && AuditionPvSha256.IsSha256(frame.frameSha256))
                            {
                                orderedFrameHashes.Add(frame.frameSha256);
                                orderedSourcePaths.Add(CanonicalSourceFramePath(sourceCapture,
                                    take.sourceShotId, frame.sourceFrame));
                                orderedFrameKeys.Add(ContactCellKey(take.takeId,
                                    frame.sourceFrame, frame.frameSha256));
                            }
                    }
                var orderedGraphicPaths = new List<string>();
                int[] previewIndexes = DeterministicPreviewIndexes(orderedFrameHashes.Count,
                    MaxPreviewCells);
                string[] previewFrameHashes = previewIndexes
                    .Select(index => orderedFrameHashes[index]).ToArray();
                string[] previewSourcePaths = previewIndexes
                    .Select(index => orderedSourcePaths[index]).ToArray();
                var previewCellKeys = new HashSet<string>(previewIndexes
                    .Select(index => orderedFrameKeys[index]), StringComparer.Ordinal);
                string[] expectedFrames = previewFrameHashes
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] reviewedFrames = (visual.reviewedFrameSha256 ?? Array.Empty<string>())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                int expectedCellCount = previewFrameHashes.Length;
                bool contactSheetValid = Pinned(visual.contactSheet, context, "VISUAL_CONTACT_SHEET",
                    "gateEvidence", report, out string contactSheetPath,
                    context.allowedReviewRoots) &&
                    TryDecodedPngDimensions(contactSheetPath, out int sheetWidth, out int sheetHeight) &&
                    expectedCellCount > 0 && expectedCellCount <= MaxPreviewCells &&
                    visual.contactSheetColumns == Math.Min(4, expectedCellCount) &&
                    visual.contactSheetRows == (expectedCellCount + visual.contactSheetColumns - 1) /
                        visual.contactSheetColumns && visual.contactSheetRows <= 8 &&
                    visual.contactSheetCellCount == expectedCellCount &&
                    sheetWidth == visual.contactSheetColumns * (Width / 4) &&
                    sheetHeight == visual.contactSheetRows * (Height / 4) &&
                    visual.contactSheetGenerator == "AuditionPvQuarterScaleContactSheet" &&
                    visual.contactSheetGeneratorVersion == "nearest-rgba32-bottom-left-v1" &&
                    ContactSheetMatchesQuarterScale(contactSheetPath,
                        previewSourcePaths.Concat(orderedGraphicPaths).ToArray(),
                        visual.contactSheetColumns, visual.contactSheetRows);
                string[] expectedInputs = previewFrameHashes.Concat(orderedGraphics).ToArray();
                string[] declaredInputs = visual.contactSheetInputSha256 ?? Array.Empty<string>();
                if (visual.schemaVersion != VisualReviewSchema || !visual.approved ||
                    visual.manifestId != manifest.manifestId ||
                    visual.productCheckpointGitSha != manifest.productCheckpointGitSha ||
                    string.IsNullOrWhiteSpace(visual.reviewedBy) || !Utc(visual.reviewedAtUtc) ||
                    visual.downscalePercent != 25 || !visual.faceReadable || !visual.bossReadable ||
                    !visual.attackDirectionReadable || !visual.impactPointReadable ||
                    !visual.noPinkShader || !visual.noErrorMagenta || !visual.noNullMaterial ||
                    !visual.noBlackMesh || !visual.noBrokenTrail || !contactSheetValid ||
                    !reviewed.SequenceEqual(expectedReviews, StringComparer.Ordinal) ||
                    !reviewedGraphics.SequenceEqual(expectedGraphics, StringComparer.Ordinal) ||
                    !reviewedFrames.SequenceEqual(expectedFrames, StringComparer.Ordinal) ||
                    !declaredInputs.SequenceEqual(expectedInputs, StringComparer.Ordinal) ||
                    !VisualCriterionRefsValid(visual.criterionRefs, approvedTakes,
                        approvedShotByTake, reviewedFramesByTake, previewCellKeys))
                    report.Error("VISUAL_GATE_UNAPPROVED_OR_FAILED", "gateEvidence",
                        "Typed 25% contact-sheet review must bind exact reviewed frames/end-card bytes and criterion refs.");
            }
            if (ReadPinnedJson(evidence.rightsCoverageReview, context, "RIGHTS_COVERAGE_REVIEW",
                    "gateEvidence", report, out AuditionPvRightsCoverageReviewArtifact coverage,
                    context.allowedReviewRoots))
            {
                if ((coverage.dependencies ?? Array.Empty<AuditionPvRightsDependencyClassification>()).Length > 4096 ||
                    (coverage.usedItemIds ?? Array.Empty<string>()).Length > 512 ||
                    (coverage.reviewedCaptures ?? Array.Empty<AuditionPvRightsReviewedCaptureIdentity>()).Length > 512)
                {
                    report.Error("RIGHTS_COVERAGE_CARDINALITY_EXCEEDED", "gateEvidence",
                        "Rights closure exceeds accepted production evidence limits.");
                    return;
                }
                AuditionPvSixtySecondTakeCandidate[] selectedTakes = SelectedSourceTakes(manifest);
                string[] expected = (manifest.usedItems ?? Array.Empty<AuditionPvSixtySecondUsedItem>())
                    .Where(item => item != null).Select(item => item.id)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] declared = (coverage.usedItemIds ?? Array.Empty<string>())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] expectedCaptures = selectedTakes
                    .Select(take => ReviewedCaptureIdentity(take.sourceCaptureId,
                        take.sourceManifestSha256, take.sourceDependencyIdentitySha256))
                    .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] declaredCaptures = (coverage.reviewedCaptures ??
                        Array.Empty<AuditionPvRightsReviewedCaptureIdentity>())
                    .Select(value => value == null ? string.Empty : ReviewedCaptureIdentity(
                        value.captureId, value.sourceManifestSha256,
                        value.sourceDependencyIdentitySha256))
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                AuditionPvRightsDependencyClassification[] expectedDependencies =
                    ExpectedRightsDependencyClosure(selectedTakes, context, report);
                bool dependencyClosure = RightsDependencyClosureMatches(expectedDependencies,
                    coverage.dependencies, IndexItems(manifest.usedItems), report);
                if (coverage.schemaVersion != RightsCoverageReviewSchema || !coverage.complete ||
                    coverage.manifestId != manifest.manifestId ||
                    coverage.productCheckpointGitSha != manifest.productCheckpointGitSha ||
                    string.IsNullOrWhiteSpace(coverage.reviewedBy) || !Utc(coverage.reviewedAtUtc) ||
                    !declared.SequenceEqual(expected, StringComparer.Ordinal) ||
                    !declaredCaptures.SequenceEqual(expectedCaptures, StringComparer.Ordinal) ||
                    !dependencyClosure)
                    report.Error("RIGHTS_COVERAGE_REVIEW_INVALID", "gateEvidence",
                        "Review must reverse-cover every exact selected capture dependency with no omissions/extras.");
            }
        }

        private static AuditionPvSixtySecondTakeCandidate[] SelectedSourceTakes(
            AuditionPvSixtySecondShotGateManifest manifest) => (manifest?.buckets ??
                Array.Empty<AuditionPvSixtySecondSequenceBucket>())
            .Where(bucket => bucket != null)
            .SelectMany(bucket => bucket.shots ?? Array.Empty<AuditionPvSixtySecondAtomicShot>())
            .Where(shot => shot != null && shot.sourceKind != "end-card")
            .SelectMany(shot => (shot.candidateTakes ?? Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                .Where(take => take != null &&
                    (take.takeId == shot.approvedTakeId || take.takeId == shot.cleanPlateTakeId)))
            .ToArray();

        private static AuditionPvRightsDependencyClassification[] ExpectedRightsDependencyClosure(
            IEnumerable<AuditionPvSixtySecondTakeCandidate> takes,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            var result = new Dictionary<string, AuditionPvRightsDependencyClassification>(StringComparer.Ordinal);
            foreach (AuditionPvSixtySecondTakeCandidate take in takes ??
                         Array.Empty<AuditionPvSixtySecondTakeCandidate>())
            {
                if (!ReadPinnedJson(take.sourceManifest, context, "RIGHTS_SOURCE_MANIFEST",
                        "gateEvidence", report, out AuditionPvCaptureManifest capture,
                        context.allowedCaptureRoots)) continue;
                foreach (AuditionPvDependencyHash dependency in capture.dependencyHashes ??
                             Array.Empty<AuditionPvDependencyHash>())
                {
                    if (dependency == null) continue;
                    var value = new AuditionPvRightsDependencyClassification
                    {
                        captureId = capture.captureId,
                        sourceManifestSha256 = take.sourceManifestSha256,
                        path = Normalize(dependency.path),
                        byteLength = dependency.byteLength,
                        sha256 = dependency.sha256
                    };
                    result.TryAdd(RightsDependencyIdentity(value), value);
                }
            }
            return result.Values.OrderBy(RightsDependencyIdentity, StringComparer.Ordinal).ToArray();
        }

        private static bool RightsDependencyClosureMatches(
            AuditionPvRightsDependencyClassification[] expected,
            AuditionPvRightsDependencyClassification[] declared,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items, ReportBuilder report)
        {
            expected ??= Array.Empty<AuditionPvRightsDependencyClassification>();
            declared ??= Array.Empty<AuditionPvRightsDependencyClassification>();
            string[] expectedIds = expected.Select(RightsDependencyIdentity)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] declaredIds = declared.Select(RightsDependencyIdentity)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            bool valid = declared.All(value => value != null) &&
                declaredIds.Distinct(StringComparer.Ordinal).Count() == declaredIds.Length &&
                declaredIds.SequenceEqual(expectedIds, StringComparer.Ordinal);
            foreach (AuditionPvRightsDependencyClassification value in declared.Where(value => value != null))
            {
                if (!RightsDependencyClassificationShapeValid(value, items))
                {
                    valid = false;
                    report.Error("RIGHTS_DEPENDENCY_CLASSIFICATION_INVALID", "gateEvidence",
                        value.path ?? "<null>");
                }
            }
            if (!valid)
                report.Error("RIGHTS_DEPENDENCY_CLOSURE_MISMATCH", "gateEvidence",
                    "Every selected capture dependency needs one exact reviewed classification.");
            return valid;
        }

        internal static bool RightsDependencyClassificationShapeValid(
            AuditionPvRightsDependencyClassification value,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.reason)) return false;
            if ((value.disposition == "project-authored" ||
                 value.disposition == "tool-only/not-distributed") &&
                string.IsNullOrWhiteSpace(value.usedItemId)) return true;
            return value.disposition == "licensed-item" && items != null &&
                items.TryGetValue(value.usedItemId ?? string.Empty, out var item) && item != null &&
                item.dependencyBinding == "unity-dependency" &&
                Normalize(item.sourceLocator) == Normalize(value.path) &&
                item.artifact?.sha256 == value.sha256;
        }

        private static string RightsDependencyIdentity(AuditionPvRightsDependencyClassification value) =>
            value == null ? string.Empty : string.Join("\0", value.captureId ?? string.Empty,
                value.sourceManifestSha256 ?? string.Empty, Normalize(value.path),
                value.byteLength.ToString(CultureInfo.InvariantCulture), value.sha256 ?? string.Empty);

        private static void ValidateTwelveSecondSources(
            AuditionPvSixtySecondShotGateManifest gate, string packageDirectory,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            AuditionPvTwelveSecondSelectManifest twelve =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(packageDirectory);
            ValidateAndRememberTwelveSecondArtifact(packageDirectory,
                twelve?.frameHashLedgerFile, twelve?.frameHashLedgerSha256,
                "TWELVE_SECOND_FRAME_LEDGER", context, report);
            ValidateAndRememberTwelveSecondArtifact(packageDirectory,
                twelve?.contactSheet?.file, twelve?.contactSheet?.sha256,
                "TWELVE_SECOND_CONTACT_SHEET", context, report);
            ValidateAndRememberTwelveSecondArtifact(packageDirectory,
                twelve?.proxy?.proxyFile, twelve?.proxy?.proxySha256,
                "TWELVE_SECOND_PROXY", context, report);
            ValidateAndRememberTwelveSecondArtifact(packageDirectory,
                twelve?.proxy?.probeFile, twelve?.proxy?.probeSha256,
                "TWELVE_SECOND_PROXY_PROBE", context, report);
            var captures = new Dictionary<string, LoadedCapture>(StringComparer.OrdinalIgnoreCase);
            var dependencies = new Dictionary<string, CurrentFile>(StringComparer.OrdinalIgnoreCase);
            var loadedByCapture = new Dictionary<string, LoadedCapture>(StringComparer.Ordinal);
            var sourceByCapture = new Dictionary<string, AuditionPvTwelveSecondSourceManifestIdentity>(
                StringComparer.Ordinal);
            AuditionPvTwelveSecondSourceManifestIdentity[] sources = twelve?.sourceManifests ??
                Array.Empty<AuditionPvTwelveSecondSourceManifestIdentity>();
            if (sources.Length == 0)
                report.Error("TWELVE_SECOND_SOURCE_EVIDENCE_MISSING", "gateEvidence", "No source manifests.");
            foreach ((AuditionPvTwelveSecondSourceManifestIdentity source, int index) in
                     sources.Select((value, index) => (value, index)))
            {
                if (source == null)
                {
                    report.Error("TWELVE_SECOND_SOURCE_INVALID", "gateEvidence", $"source {index} is null");
                    continue;
                }
                var pin = new AuditionPvPinnedArtifact
                    { path = source.manifestPath, sha256 = source.manifestSha256 };
                LoadedCapture loaded = LoadCapture(pin, source.dependencyIdentitySha256,
                    context, captures, dependencies, report, "gateEvidence.twelveSecondSources");
                if (loaded.valid && (loaded.manifest.captureId != source.captureId ||
                    loaded.manifest.gitCommitSha != source.gitCommitSha))
                    report.Error("TWELVE_SECOND_SOURCE_IDENTITY_MISMATCH",
                        "gateEvidence.twelveSecondSources", source.captureId ?? "<null>");
                if (!string.IsNullOrWhiteSpace(source.captureId) &&
                    (!sourceByCapture.TryAdd(source.captureId, source) ||
                     loaded.valid && !loadedByCapture.TryAdd(source.captureId, loaded)))
                    report.Error("TWELVE_SECOND_SOURCE_CAPTURE_DUPLICATE",
                        "gateEvidence.twelveSecondSources", source.captureId);
            }
            AuditionPvTwelveSecondSourceFrameLedgerBinding[] bindings =
                gate?.gateEvidence?.twelveSecondSourceFrameLedgers ??
                Array.Empty<AuditionPvTwelveSecondSourceFrameLedgerBinding>();
            var bindingByOrder = bindings.Where(value => value != null)
                .GroupBy(value => value.segmentOrder).ToDictionary(group => group.Key,
                    group => group.First());
            var ledgerCache = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase);
            var ledgerByOrder = new Dictionary<int, Dictionary<string, string>>();
            var segmentByOrder = new Dictionary<int, AuditionPvTwelveSecondSelectSegment>();
            foreach (AuditionPvTwelveSecondSelectSegment segment in twelve?.segments ??
                     Array.Empty<AuditionPvTwelveSecondSelectSegment>())
            {
                if (segment == null || !segmentByOrder.TryAdd(segment.order, segment) ||
                    !bindingByOrder.TryGetValue(segment?.order ?? -1, out var binding) ||
                    !sourceByCapture.TryGetValue(segment?.sourceCaptureId ?? string.Empty,
                        out var source) ||
                    !loadedByCapture.TryGetValue(segment?.sourceCaptureId ?? string.Empty,
                        out var loaded))
                {
                    report.Error("TWELVE_SECOND_SOURCE_BINDING_MISSING", "gateEvidence",
                        segment?.order.ToString(CultureInfo.InvariantCulture) ?? "<null>");
                    continue;
                }
                if (!TwelveSecondSourceBindingMatches(binding, segment, source))
                    report.Error("TWELVE_SECOND_SOURCE_BINDING_IDENTITY_MISMATCH", "gateEvidence",
                        segment.order.ToString(CultureInfo.InvariantCulture));
                AuditionPvShotManifestEntry shot = (loaded.manifest.shots ??
                        Array.Empty<AuditionPvShotManifestEntry>())
                    .SingleOrDefault(value => value != null && value.id == segment.sourceShotId);
                long sourceCount = (long)segment.sourceEndFrame - segment.sourceStartFrame + 1L;
                long selectCount = (long)segment.selectEndFrame - segment.selectStartFrame + 1L;
                if (shot == null)
                    report.Error("TWELVE_SECOND_SOURCE_SHOT_MISSING", "gateEvidence",
                        segment.sourceShotId ?? "<null>");
                else
                {
                    if (shot.hudMode != segment.hudMode)
                        report.Error("TWELVE_SECOND_SOURCE_HUD_MISMATCH", "gateEvidence",
                            segment.sourceShotId ?? "<null>");
                    if (segment.sourceStartFrame < shot.startFrame ||
                        segment.sourceEndFrame > shot.endFrame || sourceCount <= 0 ||
                        sourceCount != selectCount || sourceCount != segment.frameCount)
                        report.Error("TWELVE_SECOND_SOURCE_RANGE_MISMATCH", "gateEvidence",
                            segment.sourceShotId ?? "<null>");
                }
                if (Pinned(binding.frameLedger, context, "TWELVE_SECOND_SOURCE_FRAME_LEDGER",
                        "gateEvidence", report, out string ledgerPath,
                        new[] { loaded.manifest.outputDirectory }))
                {
                    string cacheKey = Normalize(ledgerPath) + "\0" + binding.frameLedger.sha256;
                    if (!ledgerCache.ContainsKey(cacheKey))
                        ledgerCache[cacheKey] = ParseFrameLedger(ledgerPath, report, "gateEvidence");
                    ledgerByOrder[segment.order] = ledgerCache[cacheKey];
                }
            }
            var fileCache = new Dictionary<string, CurrentFile>(StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvTwelveSecondFrameMapping mapping in twelve?.frames ??
                     Array.Empty<AuditionPvTwelveSecondFrameMapping>())
            {
                if (mapping == null || !segmentByOrder.TryGetValue(mapping?.segmentOrder ?? -1,
                        out var segment) || !bindingByOrder.TryGetValue(mapping?.segmentOrder ?? -1,
                        out var binding) || !sourceByCapture.TryGetValue(
                        mapping?.sourceCaptureId ?? string.Empty, out var source) ||
                    !loadedByCapture.TryGetValue(mapping?.sourceCaptureId ?? string.Empty,
                        out var loaded))
                {
                    report.Error("TWELVE_SECOND_SOURCE_FRAME_BINDING_MISSING", "gateEvidence",
                        mapping?.selectFrame.ToString(CultureInfo.InvariantCulture) ?? "<null>");
                    continue;
                }
                int expectedSource;
                try
                {
                    expectedSource = checked(segment.sourceStartFrame +
                        mapping.selectFrame - segment.selectStartFrame);
                }
                catch (OverflowException)
                {
                    report.Error("TWELVE_SECOND_SOURCE_FRAME_RANGE_OVERFLOW", "gateEvidence",
                        mapping.selectFrame.ToString(CultureInfo.InvariantCulture));
                    continue;
                }
                string expectedRelative = CanonicalSourceFrameRelative(
                    segment.sourceShotId, expectedSource);
                if (!TwelveSecondSourceMappingIdentityValid(mapping, segment, source,
                        expectedSource, expectedRelative))
                    report.Error("TWELVE_SECOND_SOURCE_FRAME_IDENTITY_MISMATCH", "gateEvidence",
                        mapping.selectFrame.ToString(CultureInfo.InvariantCulture));
                string framePath = Path.Combine(loaded.manifest.outputDirectory,
                    expectedRelative.Replace('/', Path.DirectorySeparatorChar));
                bool physical = false;
                try
                {
                    RequireUnder(framePath, new[] { loaded.manifest.outputDirectory },
                        "12-second current source frame");
                    RejectReparseChain(framePath);
                    if (!fileCache.TryGetValue(framePath, out CurrentFile current))
                        fileCache[framePath] = current = CurrentFile.Read(framePath);
                    physical = current.exists && current.sha256 == mapping.sha256 &&
                        IsDecodedPngDimensions(framePath, Width, Height);
                    if (physical) RememberFinalFile(context, framePath, current.sha256,
                        current.length, report, "gateEvidence");
                }
                catch (Exception exception) when (IsPathOrIo(exception)) { physical = false; }
                bool inLedger = ledgerByOrder.TryGetValue(mapping.segmentOrder, out var ledger) &&
                    ((ledger.TryGetValue(expectedRelative, out string ledgerSha) ||
                      ledger.TryGetValue(Path.GetFileName(expectedRelative), out ledgerSha)) &&
                     ledgerSha == mapping.sha256);
                bool selectPhysical = false;
                try
                {
                    string selectPath = Path.GetFullPath(Path.Combine(packageDirectory,
                        (mapping.selectRelativePath ?? string.Empty)
                        .Replace('/', Path.DirectorySeparatorChar)));
                    RequireUnder(selectPath, new[] { packageDirectory }, "12-second select frame");
                    RejectReparseChain(selectPath);
                    CurrentFile current = CurrentFile.Read(selectPath);
                    selectPhysical = current.exists && current.sha256 == mapping.sha256 &&
                        IsDecodedPngDimensions(selectPath, Width, Height);
                    if (selectPhysical) RememberFinalFile(context, selectPath, current.sha256,
                        current.length, report, "gateEvidence");
                }
                catch (Exception exception) when (IsPathOrIo(exception)) { selectPhysical = false; }
                if (!physical || !inLedger || !selectPhysical)
                    report.Error("TWELVE_SECOND_SOURCE_FRAME_OR_LEDGER_INVALID", "gateEvidence",
                        mapping.selectFrame.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static string CanonicalSourceFrameRelative(string shotId, int frame) =>
            Normalize((shotId == AuditionPvStationTransitionGoldenCapture.ShotId
                    ? AuditionPvStationTransitionGoldenCapture.FramesFolderName
                    : "frames/" + shotId) + $"/frame_{frame:0000}.png");

        private static string CanonicalSourceFramePath(AuditionPvCaptureManifest capture,
            string shotId, int frame) => Path.Combine(capture.outputDirectory,
                CanonicalSourceFrameRelative(shotId, frame).Replace('/', Path.DirectorySeparatorChar));

        internal static bool TwelveSecondSourceBindingMatches(
            AuditionPvTwelveSecondSourceFrameLedgerBinding binding,
            AuditionPvTwelveSecondSelectSegment segment,
            AuditionPvTwelveSecondSourceManifestIdentity source) =>
            binding != null && segment != null && source != null &&
            binding.segmentOrder == segment.order && binding.sourceCaptureId == source.captureId &&
            binding.sourceManifestSha256 == source.manifestSha256 &&
            binding.sourceDependencyIdentitySha256 == source.dependencyIdentitySha256 &&
            binding.sourceShotId == segment.sourceShotId;

        internal static bool TwelveSecondSourceMappingIdentityValid(
            AuditionPvTwelveSecondFrameMapping mapping,
            AuditionPvTwelveSecondSelectSegment segment,
            AuditionPvTwelveSecondSourceManifestIdentity source, int expectedSourceFrame,
            string expectedRelativePath) => mapping != null && segment != null && source != null &&
            mapping.role == segment.role && mapping.segmentOrder == segment.order &&
            mapping.sourceCaptureId == segment.sourceCaptureId &&
            mapping.sourceManifestSha256 == source.manifestSha256 &&
            mapping.sourceDependencyIdentitySha256 == source.dependencyIdentitySha256 &&
            mapping.sourceShotId == segment.sourceShotId &&
            mapping.sourceFrame == expectedSourceFrame &&
            Normalize(mapping.sourceRelativePath) == Normalize(expectedRelativePath);

        private static void ValidateAndRememberTwelveSecondArtifact(string packageDirectory,
            string relativePath, string sha256, string prefix,
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || !AuditionPvSha256.IsSha256(sha256))
            { report.Error(prefix + "_PIN_INVALID", "gateEvidence", relativePath ?? "<null>"); return; }
            try
            {
                string path = Path.GetFullPath(Path.Combine(packageDirectory,
                    relativePath.Replace('/', Path.DirectorySeparatorChar)));
                RequireUnder(path, new[] { packageDirectory }, "installed 12-second artifact");
                RejectReparseChain(path);
                CurrentFile current = CurrentFile.Read(path);
                if (!current.exists || current.sha256 != sha256)
                    report.Error(prefix + "_DRIFT", "gateEvidence", path);
                else RememberFinalFile(context, path, current.sha256, current.length,
                    report, "gateEvidence");
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            { report.Error(prefix + "_PATH_INVALID", "gateEvidence", exception.Message); }
        }

        private static string DependencyIdentity(AuditionPvCaptureManifest manifest,
            ReportBuilder report, string at)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var text = new StringBuilder();
            foreach (AuditionPvDependencyHash value in (manifest.dependencyHashes ??
                         Array.Empty<AuditionPvDependencyHash>())
                     .OrderBy(value => value?.path, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(value => value?.path, StringComparer.Ordinal))
            {
                string normalized = Normalize(value?.path);
                if (value == null || string.IsNullOrWhiteSpace(normalized) || !seen.Add(normalized) ||
                    !value.exists || value.byteLength < 0 || !AuditionPvSha256.IsSha256(value.sha256))
                {
                    report.Error("TAKE_DEPENDENCY_IDENTITY_INVALID", at, "Invalid dependency entry.");
                    return string.Empty;
                }
                text.Append(normalized).Append('\0').Append('1').Append('\0')
                    .Append(value.byteLength.ToString(CultureInfo.InvariantCulture)).Append('\0')
                    .Append(value.sha256).Append('\0');
            }
            if (seen.Count == 0)
                report.Error("TAKE_DEPENDENCY_IDENTITY_INVALID", at, "Empty dependencies.");
            return seen.Count == 0 ? string.Empty : AuditionPvSha256.TextHash(text.ToString());
        }

        private static bool ReadPinnedJson<T>(AuditionPvPinnedArtifact pin,
            AuditionPvSixtySecondValidationContext context, string prefix, string at,
            ReportBuilder report, out T value, IEnumerable<string> requiredRoots = null,
            bool warningOnly = false) where T : class
        {
            value = null;
            if (!Pinned(pin, context, prefix, at, report, out string path, requiredRoots, warningOnly))
                return false;
            try
            {
                byte[] bytes = ReadAllBytesCapped(path, MaxEvidenceJsonBytes,
                    prefix + " JSON");
                if (ByteSha256(bytes) != pin.sha256)
                    throw new InvalidDataException("Pinned JSON changed while it was being read.");
                value = JsonUtility.FromJson<T>(
                    new UTF8Encoding(false, true).GetString(bytes).TrimStart('\ufeff'));
                if (value != null) return true;
                throw new InvalidDataException("JSON root is null.");
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Issue(warningOnly, prefix + "_JSON_INVALID", at, exception.Message);
                return false;
            }
        }

        private static bool Pinned(AuditionPvPinnedArtifact pin,
            AuditionPvSixtySecondValidationContext context, string prefix, string at,
            ReportBuilder report, out string path, IEnumerable<string> requiredRoots = null,
            bool warningOnly = false)
        {
            context ??= new AuditionPvSixtySecondValidationContext();
            path = string.Empty;
            if (pin == null || string.IsNullOrWhiteSpace(pin.path) ||
                !AuditionPvSha256.IsSha256(pin.sha256))
            {
                report.Issue(warningOnly, prefix + "_PIN_MISSING", at, "Path and SHA-256 are required.");
                return false;
            }
            try
            {
                path = ResolveEvidencePath(pin.path, context, requiredRoots);
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Issue(warningOnly, prefix + "_PATH_INVALID", at, exception.Message);
                return false;
            }
            try
            {
                if (!File.Exists(path))
                {
                    report.Issue(warningOnly, prefix + "_MISSING", at, path);
                    return false;
                }
                string actualSha = AuditionPvSha256.FileHash(path);
                if (actualSha != pin.sha256)
                {
                    report.Issue(warningOnly, prefix + "_DRIFT", at, path);
                    return false;
                }
                RememberFinalFile(context, path, actualSha, new FileInfo(path).Length, report, at);
                return true;
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Issue(warningOnly, prefix + "_READ_INVALID", at, exception.Message);
                return false;
            }
        }

        private static bool PinnedExact(string path, string hash, string prefix,
            string at, AuditionPvSixtySecondValidationContext context, string requiredRoot,
            ReportBuilder report)
        {
            if (!AuditionPvSha256.IsSha256(hash))
            {
                report.Error(prefix + "_PIN_MISSING", at, path);
                return false;
            }
            try
            {
                path = ResolveEvidencePath(path, context, new[] { requiredRoot });
                if (!PathsEqual(Path.GetDirectoryName(path), requiredRoot))
                    throw new InvalidDataException("Canonical 12-second file must be a direct package child.");
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error(prefix + "_PATH_INVALID", at, exception.Message);
                return false;
            }
            try
            {
                if (!File.Exists(path))
                {
                    report.Error(prefix + "_MISSING", at, path);
                    return false;
                }
                if (AuditionPvSha256.FileHash(path) != hash)
                {
                    report.Error(prefix + "_DRIFT", at, path);
                    return false;
                }
                RememberFinalFile(context, path, hash, new FileInfo(path).Length, report, at);
                return true;
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            {
                report.Error(prefix + "_READ_INVALID", at, exception.Message);
                return false;
            }
        }

        private static void RememberFinalFile(AuditionPvSixtySecondValidationContext context,
            string path, string sha256, long length, ReportBuilder report, string at)
        {
            if (context == null || string.IsNullOrWhiteSpace(path) ||
                !AuditionPvSha256.IsSha256(sha256) || length < 0) return;
            string full;
            try { full = Path.GetFullPath(path); }
            catch (Exception exception) when (IsPathOrIo(exception)) { return; }
            if (context.finalFileSnapshots.TryGetValue(full, out var prior) &&
                (prior.sha256 != sha256 || prior.length != length))
                report?.Error("FINAL_SNAPSHOT_EXPECTATION_CONFLICT", at,
                    "One evidence path was validated against conflicting bytes.");
            else context.finalFileSnapshots[full] = new AuditionPvValidationFileSnapshot
                { path = full, sha256 = sha256, length = length };
        }

        private static void ValidateAuthoritativeFinalSnapshot(
            AuditionPvSixtySecondValidationContext context, ReportBuilder report)
        {
            foreach (AuditionPvValidationFileSnapshot value in
                     (IEnumerable<AuditionPvValidationFileSnapshot>)context?.finalFileSnapshots?.Values ??
                     Array.Empty<AuditionPvValidationFileSnapshot>())
            {
                try
                {
                    if (!FinalSnapshotFileMatches(value))
                        report.Error("AUTHORITATIVE_FINAL_FILE_DRIFT", "finalSnapshot", value.path);
                }
                catch (Exception exception) when (IsPathOrIo(exception))
                { report.Error("AUTHORITATIVE_FINAL_FILE_INVALID", "finalSnapshot", exception.Message); }
            }
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded || git.isDirty ||
                git.commitSha != context?.currentGitCommitSha)
                report.Error("AUTHORITATIVE_FINAL_GIT_CHANGED", "finalSnapshot",
                    "Git must remain on the same clean HEAD for the full validation transaction.");
        }

        internal static bool FinalSnapshotFileMatches(AuditionPvValidationFileSnapshot value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.path) || value.length < 0 ||
                !AuditionPvSha256.IsSha256(value.sha256)) return false;
            try
            {
                RejectReparseChain(value.path);
                CurrentFile current = CurrentFile.Read(value.path);
                return current.exists && current.length == value.length &&
                    current.sha256 == value.sha256;
            }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static void PinShape(AuditionPvPinnedArtifact pin, string prefix,
            string at, ReportBuilder report, bool warningOnly = false)
        {
            if (pin == null || string.IsNullOrWhiteSpace(pin.path) ||
                !AuditionPvSha256.IsSha256(pin.sha256))
                report.Issue(warningOnly, prefix + "_PIN_MISSING", at, "Path and SHA-256 are required.");
        }

        private static string ResolveEvidencePath(string value,
            AuditionPvSixtySecondValidationContext context,
            IEnumerable<string> requiredRoots = null)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Path is empty.");
            context ??= new AuditionPvSixtySecondValidationContext();
            string projectRoot = string.IsNullOrWhiteSpace(context.projectRoot)
                ? string.Empty : Path.GetFullPath(context.projectRoot);
            bool rooted = Path.IsPathRooted(value);
            if (!rooted && string.IsNullOrEmpty(projectRoot))
                throw new InvalidOperationException("Relative evidence path requires projectRoot.");
            string full = Path.GetFullPath(rooted ? value : Path.Combine(projectRoot, value));
            var roots = new List<string>();
            if (requiredRoots != null)
                roots.AddRange(requiredRoots.Where(root => !string.IsNullOrWhiteSpace(root)));
            else
            {
                if (!string.IsNullOrEmpty(projectRoot)) roots.Add(projectRoot);
                roots.AddRange(context.allowedEvidenceRoots ?? Array.Empty<string>());
            }
            RequireUnder(full, roots, "evidence path");
            RejectReparseChain(full);
            return full;
        }

        private static string ResolveEvidenceDirectory(string value,
            AuditionPvSixtySecondValidationContext context, IEnumerable<string> requiredRoots)
        {
            string full = ResolveEvidencePath(value, context, requiredRoots);
            RequireUnder(full, requiredRoots, "evidence directory");
            if (!Directory.Exists(full)) throw new DirectoryNotFoundException(full);
            return full;
        }

        private static string ResolveDependency(string value,
            AuditionPvSixtySecondValidationContext context)
        {
            string path = Normalize(value);
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Contains("..") ||
                !(path.StartsWith("Assets/", StringComparison.Ordinal) ||
                  path.StartsWith("Packages/", StringComparison.Ordinal) ||
                  path.StartsWith("ProjectSettings/", StringComparison.Ordinal)))
                throw new InvalidDataException("Dependency path must be a confined Unity project/package path.");
            if (path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                PackageInfo package = PackageInfo.FindForAssetPath(path);
                if (package != null && !string.IsNullOrWhiteSpace(package.resolvedPath))
                {
                    string prefix = "Packages/" + package.name;
                    string suffix = path.Substring(Math.Min(prefix.Length, path.Length)).TrimStart('/');
                    string resolved = string.IsNullOrEmpty(suffix)
                        ? package.resolvedPath : Path.Combine(package.resolvedPath, suffix);
                    RejectReparseChain(resolved);
                    return Path.GetFullPath(resolved);
                }
            }
            if (context == null || string.IsNullOrWhiteSpace(context.projectRoot))
                throw new InvalidOperationException("Dependency resolution requires projectRoot.");
            string full = Path.GetFullPath(Path.Combine(context.projectRoot,
                path.Replace('/', Path.DirectorySeparatorChar)));
            RequireUnder(full, new[] { context.projectRoot }, "project dependency");
            RejectReparseChain(full);
            return full;
        }

        internal static bool CaptureRecordedCleanIdentityValidForTest(
            AuditionPvCaptureManifest capture, AuditionPvSixtySecondTakeCandidate take) =>
            capture != null && take != null && !capture.gitWorktreeDirty &&
            IsFullGitSha(capture.gitCommitSha) && capture.gitCommitSha == take.gitCommitSha &&
            capture.captureId == take.sourceCaptureId;

        internal static bool DependencyBytesMatchForTest(string locator,
            AuditionPvSixtySecondValidationContext context, long expectedLength,
            string expectedSha256)
        {
            try
            {
                string path = ResolveDependency(locator, context);
                CurrentFile current = CurrentFile.Read(path);
                return current.exists && current.length == expectedLength &&
                    current.sha256 == expectedSha256;
            }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static void RequireUnder(string path, IEnumerable<string> roots, string label)
        {
            string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            foreach (string value in roots ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(value)) continue;
                string root = Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
                if (PathsEqual(full, root) || full.StartsWith(root + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase) ||
                    full.StartsWith(root + Path.AltDirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase)) return;
            }
            throw new InvalidDataException(label + " is outside its explicit allowed roots: " + full);
        }

        private static void RejectReparseChain(string value)
        {
            string current = File.Exists(value) ? value :
                Directory.Exists(value) ? value : Path.GetDirectoryName(value);
            while (!string.IsNullOrWhiteSpace(current))
            {
                if ((File.Exists(current) || Directory.Exists(current)) &&
                    (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Reparse points are not accepted in evidence paths: " + current);
                current = Path.GetDirectoryName(current);
            }
        }

        internal static bool PathHasNoReparseChainForTest(string value)
        {
            try { RejectReparseChain(value); return true; }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static bool IsDecodedPngDimensions(string path, int width, int height)
        {
            return TryDecodedPngDimensions(path, out int actualWidth, out int actualHeight) &&
                actualWidth == width && actualHeight == height;
        }

        internal static bool ContactSheetMatchesQuarterScale(string sheetPath, string[] sourcePaths,
            int columns, int rows)
        {
            sourcePaths ??= Array.Empty<string>();
            int expectedColumns = Math.Min(4, sourcePaths.Length);
            int expectedRows = expectedColumns == 0 ? 0 :
                (sourcePaths.Length + expectedColumns - 1) / expectedColumns;
            if (sourcePaths.Length == 0 || sourcePaths.Length > MaxPreviewCells ||
                columns != expectedColumns || rows != expectedRows || columns > 4 || rows > 8 ||
                checked((long)columns * rows) > MaxPreviewCells) return false;
            try
            {
                RejectReparseChain(sheetPath);
                int expectedWidth = checked(columns * (Width / 4));
                int expectedHeight = checked(rows * (Height / 4));
                if (!TryContactSheetCellDigests(
                        sheetPath,
                        expectedWidth,
                        expectedHeight,
                        columns,
                        rows,
                        out string[] sheetCellDigests))
                    return false;
                for (int cell = 0; cell < sourcePaths.Length; cell++)
                {
                    RejectReparseChain(sourcePaths[cell]);
                    if (!TryQuarterScaleSourceDigest(
                            sourcePaths[cell],
                            out string sourceDigest) ||
                        sheetCellDigests[cell] != sourceDigest) return false;
                }
                string clearDigest = RepeatedPixelDigest(
                    new Color32(0, 0, 0, 0),
                    checked((Width / 4) * (Height / 4)));
                for (int cell = sourcePaths.Length; cell < columns * rows; cell++)
                    if (sheetCellDigests[cell] != clearDigest) return false;
                return true;
            }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static bool TryContactSheetCellDigests(string path,
            int expectedWidth, int expectedHeight, int columns, int rows,
            out string[] digests)
        {
            digests = Array.Empty<string>();
            if (!TryLoadPngPixels(path, MaxSheetPngBytes, expectedWidth, expectedHeight,
                    out Color32[] pixels)) return false;
            int cellWidth = Width / 4;
            int cellHeight = Height / 4;
            var result = new string[checked(columns * rows)];
            for (int cell = 0; cell < result.Length; cell++)
            {
                result[cell] = PixelRegionDigest(
                    pixels,
                    expectedWidth,
                    cell % columns * cellWidth,
                    cell / columns * cellHeight,
                    cellWidth,
                    cellHeight,
                    1);
            }
            digests = result;
            return true;
        }

        private static bool TryQuarterScaleSourceDigest(string path, out string digest)
        {
            digest = string.Empty;
            if (!TryLoadPngPixels(path, MaxQhdPngBytes, Width, Height,
                    out Color32[] pixels)) return false;
            digest = PixelRegionDigest(
                pixels,
                Width,
                0,
                0,
                Width / 4,
                Height / 4,
                4);
            return true;
        }

        private static string PixelRegionDigest(Color32[] pixels, int rowWidth,
            int startX, int startY, int width, int height, int stride)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var chunk = new byte[64 * 1024];
            int used = 0;
            for (int y = 0; y < height; y++)
            {
                int row = checked((startY + y * stride) * rowWidth + startX);
                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = pixels[checked(row + x * stride)];
                    AppendPixelToHash(sha, chunk, ref used, pixel);
                }
            }
            sha.TransformFinalBlock(chunk, 0, used);
            return string.Concat((sha.Hash ?? Array.Empty<byte>()).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static string RepeatedPixelDigest(Color32 pixel, int count)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var chunk = new byte[64 * 1024];
            int used = 0;
            for (int index = 0; index < count; index++)
                AppendPixelToHash(sha, chunk, ref used, pixel);
            sha.TransformFinalBlock(chunk, 0, used);
            return string.Concat((sha.Hash ?? Array.Empty<byte>()).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static void AppendPixelToHash(
            System.Security.Cryptography.HashAlgorithm sha,
            byte[] chunk,
            ref int used,
            Color32 pixel)
        {
            if (used + 4 > chunk.Length)
            {
                sha.TransformBlock(chunk, 0, used, null, 0);
                used = 0;
            }
            chunk[used++] = pixel.r;
            chunk[used++] = pixel.g;
            chunk[used++] = pixel.b;
            chunk[used++] = pixel.a;
        }

        private static bool TryDecodedPngDimensions(string path, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (!TryPngPreflight(path, MaxSheetPngBytes, out int expectedWidth,
                    out int expectedHeight)) return false;
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path), markNonReadable: true))
                    return false;
                width = texture.width;
                height = texture.height;
                return width == expectedWidth && height == expectedHeight;
            }
            catch (Exception exception) when (IsPathOrIo(exception))
            { return false; }
            finally
            {
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        internal static bool TryPngPreflight(string path, long encodedByteLimit,
            out int width, out int height)
        {
            width = 0;
            height = 0;
            try
            {
                var file = new FileInfo(path);
                if (!file.Exists || file.Length < 29 || file.Length > encodedByteLimit) return false;
                byte[] header = new byte[29];
                using (FileStream stream = File.OpenRead(path))
                {
                    int read = 0;
                    while (read < header.Length)
                    {
                        int count = stream.Read(header, read, header.Length - read);
                        if (count <= 0) return false;
                        read += count;
                    }
                }
                byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
                for (int index = 0; index < signature.Length; index++)
                    if (header[index] != signature[index]) return false;
                if (ReadBigEndianUInt32(header, 8) != 13 || header[12] != (byte)'I' ||
                    header[13] != (byte)'H' || header[14] != (byte)'D' || header[15] != (byte)'R')
                    return false;
                uint rawWidth = ReadBigEndianUInt32(header, 16);
                uint rawHeight = ReadBigEndianUInt32(header, 20);
                if (rawWidth == 0 || rawHeight == 0 || rawWidth > int.MaxValue ||
                    rawHeight > int.MaxValue || checked((long)rawWidth * rawHeight) > MaxDecodedPixels ||
                    header[24] != 8 || header[25] != 2 && header[25] != 6 ||
                    header[26] != 0 || header[27] != 0 || header[28] != 0) return false;
                width = (int)rawWidth;
                height = (int)rawHeight;
                return true;
            }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }

        private static uint ReadBigEndianUInt32(byte[] value, int offset) =>
            (uint)value[offset] << 24 | (uint)value[offset + 1] << 16 |
            (uint)value[offset + 2] << 8 | value[offset + 3];

        private static bool TryLoadPngPixels(string path, long encodedByteLimit,
            int expectedWidth, int expectedHeight, out Color32[] pixels)
        {
            pixels = Array.Empty<Color32>();
            if (!TryPngPreflight(path, encodedByteLimit, out int width, out int height) ||
                width != expectedWidth || height != expectedHeight) return false;
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path), false) ||
                    texture.width != width || texture.height != height) return false;
                Color32[] decoded = texture.GetPixels32();
                if (decoded.LongLength != checked((long)width * height)) return false;
                pixels = decoded;
                return true;
            }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
            finally { if (texture != null) UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static bool TryReadWave(string path, out WaveInfo result)
        {
            result = default;
            try
            {
                var file = new FileInfo(path);
                if (!file.Exists || file.Length < 12 || file.Length > MaxWaveBytes)
                    return false;
                using var reader = new BinaryReader(File.OpenRead(path), Encoding.ASCII, false);
                if (new string(reader.ReadChars(4)) != "RIFF") return false;
                reader.ReadUInt32();
                if (new string(reader.ReadChars(4)) != "WAVE") return false;
                bool format = false, data = false;
                ushort encoding = 0, bitsPerSample = 0, blockAlign = 0;
                int byteRate = 0;
                byte[] sampleBytes = null;
                while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
                {
                    string id = new string(reader.ReadChars(4));
                    uint length = reader.ReadUInt32();
                    long next = reader.BaseStream.Position + length + (length & 1);
                    if (next > reader.BaseStream.Length) return false;
                    if (id == "fmt " && length >= 16)
                    {
                        if (format) return false;
                        encoding = reader.ReadUInt16();
                        result.channels = reader.ReadUInt16();
                        result.sampleRate = checked((int)reader.ReadUInt32());
                        byteRate = checked((int)reader.ReadUInt32());
                        blockAlign = reader.ReadUInt16();
                        bitsPerSample = reader.ReadUInt16();
                        format = encoding == 1 && new ushort[] { 16, 24, 32 }.Contains(bitsPerSample) ||
                            encoding == 3 && new ushort[] { 32, 64 }.Contains(bitsPerSample);
                    }
                    else if (id == "data")
                    {
                        if (data || length > int.MaxValue || length > MaxWaveBytes) return false;
                        result.dataBytes = checked((int)length);
                        sampleBytes = reader.ReadBytes(result.dataBytes);
                        data = result.dataBytes > 0 && sampleBytes.Length == result.dataBytes;
                    }
                    reader.BaseStream.Position = next;
                }
                int bytesPerSample = bitsPerSample / 8;
                if (!format || !data || result.channels < 1 || result.channels > 2 ||
                    result.sampleRate <= 0 || byteRate <= 0 ||
                    blockAlign != result.channels * bytesPerSample ||
                    byteRate != (long)result.sampleRate * blockAlign || result.dataBytes % blockAlign != 0)
                    return false;
                result.durationMilliseconds = checked((int)((long)result.dataBytes * 1000L / byteRate));
                result.encoding = encoding;
                result.bitsPerSample = bitsPerSample;
                result.bytesPerSample = bytesPerSample;
                result.samples = sampleBytes;
                for (int offset = 0; offset < sampleBytes.Length; offset += bytesPerSample)
                {
                    if (WaveSampleNonSilent(result, offset)) result.nonSilentSamples++;
                }
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is EndOfStreamException ||
                                               exception is OverflowException) { return false; }
        }

        private static int MinimumAudioDurationMilliseconds(string category) => category switch
        {
            "music" => 1000,
            "ambience" => 1000,
            "vo" => 250,
            _ => 100
        };

        internal static bool CueRegionShapeValid(AuditionPvAudioCueRegion value) =>
            value != null && !string.IsNullOrWhiteSpace(value.cueId) &&
            value.startMilliseconds >= 0 && value.endMilliseconds >= 0 &&
            (long)value.endMilliseconds > value.startMilliseconds;

        private static bool WaveRegionHasSignal(WaveInfo wave, int startMilliseconds,
            int endMilliseconds)
        {
            if (wave.samples == null || startMilliseconds < 0 || endMilliseconds < 0 ||
                (long)endMilliseconds <= startMilliseconds ||
                endMilliseconds > wave.durationMilliseconds || wave.sampleRate <= 0 ||
                wave.channels <= 0 || wave.bytesPerSample <= 0) return false;
            long first = (long)startMilliseconds * wave.sampleRate * wave.channels / 1000L;
            long lastExclusive = (long)endMilliseconds * wave.sampleRate * wave.channels / 1000L;
            long available = wave.samples.LongLength / wave.bytesPerSample;
            if (first < 0 || lastExclusive <= first || lastExclusive > available) return false;
            long nonSilent = 0;
            double sumSquares = 0d, peak = 0d;
            for (long sample = first; sample < lastExclusive; sample++)
            {
                double magnitude = WaveSampleMagnitude(wave,
                    checked((int)(sample * wave.bytesPerSample)));
                if (magnitude > 0.00001d) nonSilent++;
                if (magnitude > peak) peak = magnitude;
                sumSquares += magnitude * magnitude;
            }
            long total = lastExclusive - first;
            long minimum = Math.Max(1L, (total + 99L) / 100L);
            double rms = Math.Sqrt(sumSquares / Math.Max(1L, total));
            return nonSilent >= minimum && peak >= 0.001d && rms >= 0.0001d;
        }

        internal static bool WaveCueRegionsHaveSignal(string path,
            AuditionPvAudioCueRegion[] regions) => TryReadWave(path, out WaveInfo wave) &&
            (regions ?? Array.Empty<AuditionPvAudioCueRegion>()).Length > 0 &&
            (regions ?? Array.Empty<AuditionPvAudioCueRegion>()).All(region =>
                CueRegionShapeValid(region) && WaveRegionHasSignal(wave,
                    region.startMilliseconds, region.endMilliseconds));

        private static bool WaveSampleNonSilent(WaveInfo wave, int offset) =>
            WaveSampleMagnitude(wave, offset) > 0.000001d;

        private static double WaveSampleMagnitude(WaveInfo wave, int offset)
        {
            if (wave.encoding == 1)
            {
                long signed = wave.bitsPerSample switch
                {
                    16 => BitConverter.ToInt16(wave.samples, offset),
                    24 => ReadPcm24(wave.samples, offset),
                    32 => BitConverter.ToInt32(wave.samples, offset),
                    _ => 0L
                };
                double scale = wave.bitsPerSample switch
                { 16 => 32768d, 24 => 8388608d, 32 => 2147483648d, _ => double.MaxValue };
                return Math.Min(1d, Math.Abs(signed / scale));
            }
            if (wave.bitsPerSample == 32)
            {
                float value = BitConverter.ToSingle(wave.samples, offset);
                return float.IsNaN(value) || float.IsInfinity(value) ? 0d : Math.Abs(value);
            }
            double doubleValue = BitConverter.ToDouble(wave.samples, offset);
            return double.IsNaN(doubleValue) || double.IsInfinity(doubleValue)
                ? 0d : Math.Abs(doubleValue);
        }

        private static int ReadPcm24(byte[] bytes, int offset)
        {
            int value = bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16;
            return (value & 0x800000) == 0 ? value : value | unchecked((int)0xff000000);
        }

        private static IEnumerable<string> RootsForItem(AuditionPvSixtySecondUsedItem item,
            AuditionPvSixtySecondValidationContext context)
        {
            if (item?.dependencyBinding == "unity-dependency") return new[] { context.projectRoot };
            return item?.scope switch
            {
                "audio" => context.allowedAudioRoots,
                "ai" => context.allowedAudioRoots,
                "asset" => context.allowedGraphicsRoots,
                "font" => context.allowedGraphicsRoots,
                _ => Array.Empty<string>()
            };
        }

        private static bool SameRange(AuditionPvRangeBoundArtifact proof,
            AuditionPvSixtySecondTakeCandidate take) =>
            proof.sourceRangeStartFrame == take.sourceRangeStartFrame &&
            proof.sourceRangeEndFrame == take.sourceRangeEndFrame &&
            proof.selectStartFrame == take.selectStartFrame &&
            proof.selectEndFrame == take.selectEndFrame;

        private static bool TryItem(string id, string scope,
            IReadOnlyDictionary<string, AuditionPvSixtySecondUsedItem> items,
            ISet<string> referenced)
        {
            if (!items.TryGetValue(id ?? string.Empty, out var item) || item == null || item.scope != scope)
                return false;
            referenced.Add(id);
            return true;
        }

        private static string CueCategory(string cue)
        {
            if (cue == "music-bed") return "music";
            if (cue == "city-ambience" || cue == "olympus-ambience") return "ambience";
            if (cue == "announcement-vo" || cue == "inori-vo" || cue == "boss-vo") return "vo";
            return "sfx";
        }

        private static string ReviewedCaptureIdentity(string captureId, string manifestSha,
            string dependencyIdentity) => string.Join("\0", captureId ?? string.Empty,
            manifestSha ?? string.Empty, dependencyIdentity ?? string.Empty);

        private static bool VisualCriterionRefsValid(AuditionPvVisualCriterionRef[] values,
            AuditionPvSixtySecondTakeCandidate[] takes,
            IReadOnlyDictionary<string, AuditionPvSixtySecondAtomicShot> shots,
            IReadOnlyDictionary<string, AuditionPvMeasuredFrame[]> reviewedFrames,
            ISet<string> contactCellKeys)
        {
            values ??= Array.Empty<AuditionPvVisualCriterionRef>();
            var required = new HashSet<string>(new[]
                { "face", "boss", "attack-direction", "impact-point" }, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var takeById = (takes ?? Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                .Where(take => take != null && !string.IsNullOrWhiteSpace(take.takeId))
                .GroupBy(take => take.takeId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            foreach (AuditionPvVisualCriterionRef value in values)
            {
                if (value == null || !required.Contains(value.criterion) ||
                    !seen.Add(value.criterion) ||
                    !takeById.TryGetValue(value.takeId ?? string.Empty, out var take) ||
                    !shots.TryGetValue(value.takeId ?? string.Empty, out var shot) ||
                    !reviewedFrames.TryGetValue(value.takeId ?? string.Empty, out var frames) ||
                    !VisualCriterionRefMatches(value, take, shot, frames) ||
                    contactCellKeys == null || !contactCellKeys.Contains(ContactCellKey(
                        value.takeId, value.sourceFrame, value.frameSha256)))
                    return false;
            }
            return seen.SetEquals(required);
        }

        private static string ContactCellKey(string takeId, int sourceFrame, string frameSha256) =>
            string.Join("\0", takeId ?? string.Empty,
                sourceFrame.ToString(CultureInfo.InvariantCulture), frameSha256 ?? string.Empty);

        internal static bool VisualCriterionRefIsContactCell(AuditionPvVisualCriterionRef value,
            IEnumerable<string> cellKeys) => value != null && cellKeys != null &&
            new HashSet<string>(cellKeys, StringComparer.Ordinal).Contains(ContactCellKey(
                value.takeId, value.sourceFrame, value.frameSha256));

        internal static bool VisualCriterionRefMatches(AuditionPvVisualCriterionRef value,
            AuditionPvSixtySecondTakeCandidate take, AuditionPvSixtySecondAtomicShot shot,
            AuditionPvMeasuredFrame[] reviewedFrames) => value != null && take != null && shot != null &&
            value.takeId == take.takeId && value.atomicShotId == shot.shotId &&
            value.sourceFrame >= take.selectStartFrame && value.sourceFrame <= take.selectEndFrame &&
            AuditionPvSha256.IsSha256(value.frameSha256) && !string.IsNullOrWhiteSpace(value.note) &&
            (reviewedFrames ?? Array.Empty<AuditionPvMeasuredFrame>()).Any(frame => frame != null &&
                frame.sourceFrame == value.sourceFrame && frame.frameSha256 == value.frameSha256) &&
            CriterionRelevantToShot(value.criterion, shot.beatIds);

        private static bool CriterionRelevantToShot(string criterion, string[] beats)
        {
            beats ??= Array.Empty<string>();
            string[] relevant = criterion switch
            {
                "face" => new[] { "c33-wing-deployment", "c34-eye-open" },
                "boss" => new[]
                {
                    "boss-low-angle", "boss-silhouette", "boss-pattern-1", "boss-pattern-2",
                    "boss-pattern-3", "boss-finisher", "boss-collapse", "aftermath"
                },
                "attack-direction" => new[]
                {
                    "city-fire", "player-hit", "perfect-dodge", "summon-chain", "summon-defense",
                    "boss-pattern-1", "boss-pattern-2", "boss-pattern-3", "player-tier3-ultimate",
                    "boss-finisher"
                },
                "impact-point" => new[]
                {
                    "city-fire", "player-hit", "summon-chain", "boss-pattern-1", "boss-pattern-2",
                    "boss-pattern-3", "player-tier3-ultimate", "boss-finisher", "boss-collapse"
                },
                _ => Array.Empty<string>()
            };
            return beats.Any(beat => relevant.Contains(beat, StringComparer.Ordinal));
        }

        private static void Refs(string[] ids, IEnumerable<string> validIds,
            string prefix, string at, ReportBuilder report)
        {
            ids ??= Array.Empty<string>();
            var valid = new HashSet<string>(validIds, StringComparer.Ordinal);
            if (ids.Length == 0) report.Error(prefix + "_MISSING", at, "At least one reference is required.");
            if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
                report.Error(prefix + "_DUPLICATE", at, "Duplicate reference.");
            foreach (string id in ids.Where(id => !valid.Contains(id ?? string.Empty)))
                report.Error(prefix + "_UNKNOWN", at, id ?? "<null>");
        }

        private static Dictionary<string, AuditionPvSixtySecondRightsEvidence> IndexRights(
            AuditionPvSixtySecondRightsEvidence[] values) =>
            (values ?? Array.Empty<AuditionPvSixtySecondRightsEvidence>())
            .Where(value => value != null && !string.IsNullOrWhiteSpace(value.id))
            .GroupBy(value => value.id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        private static Dictionary<string, AuditionPvSixtySecondUsedItem> IndexItems(
            AuditionPvSixtySecondUsedItem[] values) =>
            (values ?? Array.Empty<AuditionPvSixtySecondUsedItem>())
            .Where(value => value != null && !string.IsNullOrWhiteSpace(value.id))
            .GroupBy(value => value.id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        private static Dictionary<string, AuditionPvSixtySecondAudioEvidence> IndexAudio(
            AuditionPvSixtySecondAudioEvidence[] values) =>
            (values ?? Array.Empty<AuditionPvSixtySecondAudioEvidence>())
            .Where(value => value != null && !string.IsNullOrWhiteSpace(value.id))
            .GroupBy(value => value.id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        private static AuditionPvSixtySecondRequiredBucket Bucket(string id, int start, int end,
            string role, string content, string purpose, string sceneGroup, params string[] beats) =>
            new AuditionPvSixtySecondRequiredBucket
            {
                bucketId = id, referenceStartFrame = start, referenceEndFrame = end,
                role = role, content = content, purpose = purpose, sceneGroup = sceneGroup,
                requiredBeatIds = beats
            };

        private static bool IsFullGitSha(string value) => value != null && value.Length == 40 &&
            value.All(character => character >= '0' && character <= '9' ||
                                   character >= 'a' && character <= 'f');
        private static bool Utc(string value) => DateTime.TryParse(value,
            CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed) &&
            parsed.Kind == DateTimeKind.Utc;

        // Canonical capture-core v1: UTF-8 strings use a signed big-endian byte-length
        // prefix (-1 means null); integers are signed big-endian; booleans are one byte;
        // arrays keep manifest order and use a signed count (-1 means null). testResults
        // are deliberately excluded so capture-time result artifacts cannot form C<->R hashes.
        internal static string CaptureCoreSha256(AuditionPvCaptureManifest capture)
        {
            if (capture == null) return string.Empty;
            using var stream = new MemoryStream();
            WriteCoreString(stream, CaptureCoreDigestDomain);
            WriteCoreString(stream, capture.schemaVersion);
            WriteCoreString(stream, capture.captureId);
            WriteCoreString(stream, capture.createdAtUtc);
            WriteCoreString(stream, capture.outputRoot);
            WriteCoreString(stream, capture.outputDirectory);
            WriteCoreString(stream, capture.sourceFormat);
            WriteCoreInt(stream, capture.width);
            WriteCoreInt(stream, capture.height);
            WriteCoreInt(stream, capture.fps);
            WriteCoreString(stream, capture.gitCommitSha);
            WriteCoreString(stream, capture.gitBranch);
            stream.WriteByte(capture.gitWorktreeDirty ? (byte)1 : (byte)0);
            WriteCoreString(stream, capture.worktreeDirtyHashSha256);
            WriteCoreString(stream, capture.worktreeDirtyHashAlgorithm);
            WriteCoreString(stream, capture.unityVersion);
            WriteCoreString(stream, capture.unityVersionWithRevision);
            WriteCoreString(stream, capture.recorderPackageVersion);
            WriteCoreString(stream, capture.urpPackageVersion);
            WriteCoreString(stream, capture.activeRenderPipelineAssetPath);

            AuditionPvShotManifestEntry[] shots = capture.shots;
            WriteCoreInt(stream, shots?.Length ?? -1);
            if (shots != null)
                foreach (AuditionPvShotManifestEntry shot in shots)
                {
                    stream.WriteByte(shot == null ? (byte)0 : (byte)1);
                    if (shot == null) continue;
                    WriteCoreString(stream, shot.id);
                    WriteCoreString(stream, shot.scenePath);
                    WriteCoreInt(stream, shot.startFrame);
                    WriteCoreInt(stream, shot.endFrame);
                    WriteCoreInt(stream, shot.expectedFrameCount);
                    WriteCoreString(stream, shot.hudMode);
                    WriteCoreString(stream, shot.notes);
                }

            AuditionPvBaselineManifestEntry[] baselines = capture.baselines;
            WriteCoreInt(stream, baselines?.Length ?? -1);
            if (baselines != null)
                foreach (AuditionPvBaselineManifestEntry baseline in baselines)
                {
                    stream.WriteByte(baseline == null ? (byte)0 : (byte)1);
                    if (baseline == null) continue;
                    WriteCoreString(stream, baseline.id);
                    WriteCoreString(stream, baseline.shotId);
                    WriteCoreInt(stream, baseline.sourceFrame);
                    WriteCoreString(stream, baseline.fileName);
                    WriteCoreString(stream, baseline.hudMode);
                    WriteCoreString(stream, baseline.status);
                }

            AuditionPvDependencyHash[] dependencies = capture.dependencyHashes;
            WriteCoreInt(stream, dependencies?.Length ?? -1);
            if (dependencies != null)
                foreach (AuditionPvDependencyHash dependency in dependencies)
                {
                    stream.WriteByte(dependency == null ? (byte)0 : (byte)1);
                    if (dependency == null) continue;
                    WriteCoreString(stream, dependency.path);
                    stream.WriteByte(dependency.exists ? (byte)1 : (byte)0);
                    WriteCoreLong(stream, dependency.byteLength);
                    WriteCoreString(stream, dependency.sha256);
                }
            return ByteSha256(stream.ToArray());
        }

        internal static bool CaptureCoreIdentityMatches(AuditionPvCaptureManifest capture,
            string declaredSha256) => AuditionPvSha256.IsSha256(declaredSha256) &&
            declaredSha256 == CaptureCoreSha256(capture);

        private static void WriteCoreString(Stream stream, string value)
        {
            if (value == null) { WriteCoreInt(stream, -1); return; }
            byte[] bytes = new UTF8Encoding(false, true).GetBytes(value);
            WriteCoreInt(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteCoreInt(Stream stream, int value)
        {
            uint bits = unchecked((uint)value);
            stream.WriteByte((byte)(bits >> 24));
            stream.WriteByte((byte)(bits >> 16));
            stream.WriteByte((byte)(bits >> 8));
            stream.WriteByte((byte)bits);
        }

        private static void WriteCoreLong(Stream stream, long value)
        {
            ulong bits = unchecked((ulong)value);
            for (int shift = 56; shift >= 0; shift -= 8)
                stream.WriteByte((byte)(bits >> shift));
        }

        private static string ByteSha256(byte[] value)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha.ComputeHash(value ?? Array.Empty<byte>());
            return string.Concat(hash.Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static byte[] ReadAllBytesCapped(string path, long maximumBytes, string label)
        {
            using FileStream stream = File.OpenRead(path);
            long length = stream.Length;
            if (length < 0 || length > maximumBytes || length > int.MaxValue)
                throw new InvalidDataException(label + " exceeds the accepted byte limit.");
            byte[] bytes = new byte[(int)length];
            int offset = 0;
            while (offset < bytes.Length)
            {
                int read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read <= 0) throw new EndOfStreamException(label + " ended early.");
                offset += read;
            }
            if (stream.ReadByte() != -1 || stream.Length != length)
                throw new InvalidDataException(label + " changed while it was being read.");
            return bytes;
        }

        internal static bool EvidenceFileWithinLimitForTest(string path, long maximumBytes)
        {
            try { _ = ReadAllBytesCapped(path, maximumBytes, "Test evidence"); return true; }
            catch (Exception exception) when (IsPathOrIo(exception)) { return false; }
        }
        private static string Normalize(string value) => (value ?? string.Empty).Replace('\\', '/');
        private static bool PathsEqual(string left, string right) => string.Equals(
            Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);
        private static bool IsPathOrIo(Exception value) => value is IOException ||
            value is InvalidDataException ||
            value is ArgumentException || value is NotSupportedException ||
            value is InvalidOperationException || value is FormatException || value is OverflowException ||
            value is OutOfMemoryException ||
            value is PathTooLongException ||
            value is UnauthorizedAccessException || value is UnityException;

        private sealed class LoadedCapture
        {
            public bool valid;
            public string manifestPath = string.Empty, manifestSha256 = string.Empty;
            public string captureCoreSha256 = string.Empty;
            public AuditionPvCaptureManifest manifest;
        }

        private readonly struct CurrentFile
        {
            public CurrentFile(bool exists, long length, string sha256)
            { this.exists = exists; this.length = length; this.sha256 = sha256; }
            public static CurrentFile Read(string path) => !File.Exists(path)
                ? new CurrentFile(false, 0, string.Empty)
                : new CurrentFile(true, new FileInfo(path).Length, AuditionPvSha256.FileHash(path));
            public readonly bool exists; public readonly long length; public readonly string sha256;
        }

        private struct WaveInfo
        {
            public int sampleRate, channels, dataBytes, durationMilliseconds, nonSilentSamples;
            public ushort encoding, bitsPerSample;
            public int bytesPerSample;
            public byte[] samples;
        }

        private sealed class ReportBuilder
        {
            private readonly List<AuditionPvSixtySecondGateIssue> issues = new();
            public int ErrorCount => issues.Count(value => value.severity == "error");
            public void Error(string code, string at, string message) =>
                issues.Add(new AuditionPvSixtySecondGateIssue
                    { severity = "error", code = code, location = at, message = message });
            public void Warning(string code, string at, string message) =>
                issues.Add(new AuditionPvSixtySecondGateIssue
                    { severity = "warning", code = code, location = at, message = message });
            public void Issue(bool warning, string code, string at, string message)
            { if (warning) Warning(code, at, message); else Error(code, at, message); }

            public AuditionPvSixtySecondGateValidationReport Build(
                AuditionPvSixtySecondShotGateManifest manifest, string mode, int structureErrors,
                bool authoritativeFile = false, string inputManifestPath = "",
                string inputManifestSha256 = "")
            {
                AuditionPvSixtySecondSequenceBucket[] buckets = manifest?.buckets ??
                    Array.Empty<AuditionPvSixtySecondSequenceBucket>();
                AuditionPvSixtySecondAtomicShot[] shots = buckets.Where(value => value != null)
                    .SelectMany(value => value.shots ?? Array.Empty<AuditionPvSixtySecondAtomicShot>())
                    .Where(value => value != null).ToArray();
                return new AuditionPvSixtySecondGateValidationReport
                {
                    schemaVersion = ReportSchema,
                    validationMode = mode,
                    validatedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                    inputManifestPath = inputManifestPath ?? string.Empty,
                    inputManifestSha256 = inputManifestSha256 ?? string.Empty,
                    passed = mode == "production" && authoritativeFile && ErrorCount == 0,
                    productionEvidenceVerified = mode == "production" && authoritativeFile && ErrorCount == 0,
                    structureValid = structureErrors == 0,
                    manifestId = manifest?.manifestId ?? string.Empty,
                    errorCount = ErrorCount,
                    warningCount = issues.Count(value => value.severity == "warning"),
                    bucketCount = buckets.Length,
                    shotCount = shots.Length,
                    declaredTakeSlotCount = shots.Sum(value =>
                        (value.candidateTakes ?? Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                        .Count(take => take != null && !string.IsNullOrWhiteSpace(take.takeId))),
                    declaredApprovedTakeCount = shots.Count(value => value.sourceKind != "end-card" &&
                        !string.IsNullOrWhiteSpace(value.approvedTakeId) &&
                        (value.candidateTakes ?? Array.Empty<AuditionPvSixtySecondTakeCandidate>())
                        .Count(take => take != null && take.takeId == value.approvedTakeId) == 1),
                    declaredGraphicPlaceholderCount = shots.Count(value => value.sourceKind == "end-card" &&
                        value.graphicSourceId == "layout-placeholder" &&
                        value.graphicProductionStatus == "layout-placeholder-approved" &&
                        !string.IsNullOrWhiteSpace(value.graphicArtifact?.path) &&
                        AuditionPvSha256.IsSha256(value.graphicArtifact?.sha256)),
                    issues = issues.ToArray()
                };
            }
        }
    }

    [Serializable] internal sealed class AuditionPvSixtySecondShotGateManifest
    {
        public string schemaVersion = AuditionPvSixtySecondGateManifestValidator.ManifestSchema;
        public string manifestId = string.Empty, declaredStatus = string.Empty;
        public string colorManagement = AuditionPvSixtySecondGateManifestValidator.ColorManagement;
        public string productCheckpointGitSha = string.Empty;
        public int width = 2560, height = 1440, fps = 60, totalFrames = 3600;
        public AuditionPvSixtySecondSequenceBucket[] buckets = Array.Empty<AuditionPvSixtySecondSequenceBucket>();
        public AuditionPvSixtySecondAudioEvidence[] audio = Array.Empty<AuditionPvSixtySecondAudioEvidence>();
        public AuditionPvSixtySecondRightsEvidence[] rights = Array.Empty<AuditionPvSixtySecondRightsEvidence>();
        public AuditionPvSixtySecondUsedItem[] usedItems = Array.Empty<AuditionPvSixtySecondUsedItem>();
        public AuditionPvSixtySecondGateEvidence gateEvidence = new();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondSequenceBucket
    {
        public string bucketId = string.Empty, role = string.Empty, content = string.Empty, purpose = string.Empty;
        public int timelineStartFrame, timelineEndFrame;
        public string[] requiredBeatIds = Array.Empty<string>();
        public AuditionPvSixtySecondAtomicShot[] shots = Array.Empty<AuditionPvSixtySecondAtomicShot>();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondAtomicShot
    {
        public string shotId = string.Empty, sourceKind = string.Empty;
        public int timelineStartFrame, timelineEndFrame, deterministicSeed = -1;
        public bool coreShot;
        public string scenePath = string.Empty, cameraId = string.Empty, gameplayState = string.Empty;
        public string timelineId = string.Empty, editorialHudMode = string.Empty, approvedTakeId = string.Empty;
        public string cleanPlateTakeId = string.Empty;
        public string graphicSourceId = string.Empty;
        public string graphicProductionStatus = string.Empty;
        public string sloganApprovalStatus = string.Empty, auditionNoticeApprovalStatus = string.Empty;
        public AuditionPvPinnedArtifact graphicArtifact = new();
        public string[] beatIds = Array.Empty<string>(), audioRefIds = Array.Empty<string>();
        public string[] usedItemIds = Array.Empty<string>();
        public AuditionPvSixtySecondTakeCandidate[] candidateTakes =
            Array.Empty<AuditionPvSixtySecondTakeCandidate>();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondTakeCandidate
    {
        public string takeId = string.Empty, sourceCaptureId = string.Empty, sourceShotId = string.Empty;
        public string gitCommitSha = string.Empty, declaredHudMode = string.Empty;
        public string cameraId = string.Empty, gameplayState = string.Empty, timelineId = string.Empty;
        public int deterministicSeed = -1;
        public string sourceDependencyIdentitySha256 = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty;
        public AuditionPvPinnedArtifact sourceManifest = new(), sourceFrameLedger = new(), shotAuthorship = new();
        public AuditionPvPinnedArtifact semanticProof = new(), cleanPlateProof = new();
        public AuditionPvPinnedArtifact automatedProof = new(), humanReview = new();
        public int sourceRangeStartFrame, sourceRangeEndFrame, selectStartFrame, selectEndFrame;
        public int handleBeforeFrames, handleAfterFrames;
        public string sourceManifestSha256 => sourceManifest?.sha256 ?? string.Empty;
    }

    [Serializable] internal sealed class AuditionPvShotAuthorshipArtifact
    {
        public string schemaVersion = string.Empty, sourceCaptureCoreSha256 = string.Empty;
        public string captureId = string.Empty, sourceShotId = string.Empty;
        public string cameraId = string.Empty, gameplayState = string.Empty, timelineId = string.Empty;
        public int deterministicSeed = -1;
        public AuditionPvPinnedArtifact runtimeProof = new();
        public string tool = string.Empty, toolVersion = string.Empty, createdAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvSixtySecondAudioEvidence
    {
        public string id = string.Empty, category = string.Empty, usedItemId = string.Empty;
        public string[] cueIds = Array.Empty<string>();
        public AuditionPvAudioCueRegion[] cueRegions = Array.Empty<AuditionPvAudioCueRegion>();
        public AuditionPvPinnedArtifact file = new();
        public int sampleRate, channels;
        public bool generatedByAi;
        public string aiUsedItemId = string.Empty, humanListeningStatus = "pending";
        public AuditionPvPinnedArtifact generationManifest = new(), listeningReport = new();
    }

    [Serializable] internal sealed class AuditionPvAudioCueRegion
    { public string cueId = string.Empty; public int startMilliseconds, endMilliseconds; }

    [Serializable] internal sealed class AuditionPvSixtySecondRightsEvidence
    {
        public string id = string.Empty, scope = string.Empty;
        public AuditionPvPinnedArtifact record = new();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondUsedItem
    {
        public string id = string.Empty, scope = string.Empty, rightsRecordId = string.Empty;
        public string sourceLocator = string.Empty, dependencyBinding = string.Empty;
        public AuditionPvPinnedArtifact artifact = new();
    }

    [Serializable] internal sealed class AuditionPvPinnedArtifact
    { public string path = string.Empty, sha256 = string.Empty; }

    [Serializable] internal sealed class AuditionPvSixtySecondGateEvidence
    {
        public string twelveSecondPackageDirectory = string.Empty;
        public string twelveSecondManifestSha256 = string.Empty, twelveSecondValidationSha256 = string.Empty;
        public AuditionPvPinnedArtifact twelveSecondApproval = new(), visualReview = new();
        public AuditionPvPinnedArtifact rightsCoverageReview = new();
        public AuditionPvTwelveSecondSourceFrameLedgerBinding[] twelveSecondSourceFrameLedgers =
            Array.Empty<AuditionPvTwelveSecondSourceFrameLedgerBinding>();
    }

    [Serializable] internal sealed class AuditionPvTwelveSecondSourceFrameLedgerBinding
    {
        public int segmentOrder = -1;
        public string sourceCaptureId = string.Empty, sourceManifestSha256 = string.Empty;
        public string sourceDependencyIdentitySha256 = string.Empty, sourceShotId = string.Empty;
        public AuditionPvPinnedArtifact frameLedger = new();
    }

    internal sealed class AuditionPvSixtySecondRequiredBucket
    {
        public string bucketId = string.Empty, role = string.Empty, content = string.Empty;
        public string purpose = string.Empty, sceneGroup = string.Empty;
        public int referenceStartFrame, referenceEndFrame;
        public string[] requiredBeatIds = Array.Empty<string>();
    }

    internal sealed class AuditionPvSixtySecondValidationContext
    {
        public string projectRoot = string.Empty, currentGitCommitSha = string.Empty;
        public string[] allowedEvidenceRoots = Array.Empty<string>();
        public string[] allowedCaptureRoots = Array.Empty<string>();
        public string[] allowedSelectRoots = Array.Empty<string>();
        public string[] allowedAudioRoots = Array.Empty<string>();
        public string[] allowedLicenseRoots = Array.Empty<string>();
        public string[] allowedGraphicsRoots = Array.Empty<string>();
        public string[] allowedReviewRoots = Array.Empty<string>();
        public bool currentGitClean;
        internal readonly Dictionary<string, AuditionPvValidationFileSnapshot> finalFileSnapshots =
            new(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class AuditionPvValidationFileSnapshot
    { public string path = string.Empty, sha256 = string.Empty; public long length; }

    [Serializable] internal class AuditionPvRangeBoundArtifact
    {
        public int sourceRangeStartFrame, sourceRangeEndFrame, selectStartFrame, selectEndFrame;
    }

    [Serializable] internal sealed class AuditionPvTakeSemanticProofArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, captureId = string.Empty;
        public string sourceManifestSha256 = string.Empty, sourceShotId = string.Empty;
        public string bucketId = string.Empty, atomicShotId = string.Empty, scenePath = string.Empty;
        public string cameraId = string.Empty, gameplayState = string.Empty, timelineId = string.Empty;
        public int deterministicSeed = -1;
        public string[] beatIds = Array.Empty<string>();
        public AuditionPvSemanticBeatProof[] beatProofs = Array.Empty<AuditionPvSemanticBeatProof>();
    }

    [Serializable] internal sealed class AuditionPvSemanticBeatProof
    {
        public string beatId = string.Empty, verifiedBy = string.Empty, verifiedAtUtc = string.Empty;
        public string supportingTestSuite = string.Empty, supportingTestName = string.Empty;
        public string runtimeFactKey = string.Empty;
        public AuditionPvPinnedArtifact runtimeProof = new();
    }

    [Serializable] internal sealed class AuditionPvTakeAutomatedProofArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, captureId = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty, sourceShotId = string.Empty;
        public AuditionPvAutomatedCheckEvidence[] checks = Array.Empty<AuditionPvAutomatedCheckEvidence>();
    }

    [Serializable] internal sealed class AuditionPvCleanPlateCompanionProofArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, captureId = string.Empty;
        public string sourceManifestSha256 = string.Empty, sourceShotId = string.Empty;
        public string bucketId = string.Empty, atomicShotId = string.Empty, referenceTakeId = string.Empty;
        public string referenceCaptureId = string.Empty, referenceSourceManifestSha256 = string.Empty;
        public string referenceSourceShotId = string.Empty, referenceFrameLedgerSha256 = string.Empty;
        public int referenceSourceRangeStartFrame, referenceSourceRangeEndFrame;
        public int referenceSelectStartFrame, referenceSelectEndFrame;
        public string scenePath = string.Empty, cameraId = string.Empty, gameplayState = string.Empty;
        public string timelineId = string.Empty, proofTool = string.Empty, createdAtUtc = string.Empty;
        public int deterministicSeed = -1;
    }

    [Serializable] internal sealed class AuditionPvAutomatedCheckEvidence
    {
        public string id = string.Empty, status = string.Empty;
        public string supportingTestSuite = string.Empty, supportingTestName = string.Empty;
        public AuditionPvPinnedArtifact artifact = new();
    }

    [Serializable] internal sealed class AuditionPvAutomatedCheckResultArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, id = string.Empty, captureId = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty, sourceShotId = string.Empty;
        public string sourceFrameLedgerSha256 = string.Empty;
        public string measurementTool = string.Empty, measurementToolVersion = string.Empty;
        public string measuredAtUtc = string.Empty;
        public long expectedFrameCount = -1, observedFrameCount = -1;
        public long sampledPixelCount = -1, detectedPixelCount = -1;
        public int measuredWidth = -1, measuredHeight = -1;
        public int measuredSourceFrame = -1;
        public long inspectedFrameCount = -1, nullMaterialCount = -1, errorMaterialCount = -1;
        public int mediaColumns, mediaRows;
        public AuditionPvMeasuredFrame[] sampledFrames = Array.Empty<AuditionPvMeasuredFrame>();
        public string[] filmstripInputSha256 = Array.Empty<string>();
        public string colorPrimaries = string.Empty, transferCharacteristics = string.Empty;
        public string matrixCoefficients = string.Empty, signalRange = string.Empty;
        public string transformId = string.Empty, parserName = string.Empty, parserVersion = string.Empty;
        public string sourceFrameSha256 = string.Empty, outputMediaSha256 = string.Empty;
        public bool rendererHudLayerExcluded;
        public AuditionPvPinnedArtifact mediaArtifact = new(), transformArtifact = new();
        public AuditionPvPinnedArtifact sourceMediaArtifact = new(), outputMediaArtifact = new();
        public AuditionPvPinnedArtifact rec709Config = new(), rec709OutputLedger = new();
        public AuditionPvPinnedArtifact scanConfig = new(), scanLedger = new();
        public AuditionPvPinnedArtifact runtimeWorkload = new();
        public string mediaPurpose = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvMeasuredFrame
    { public int sourceFrame = -1; public string frameSha256 = string.Empty; }

    [Serializable] internal sealed class AuditionPvRec709TransformArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, transformId = string.Empty;
        public string captureId = string.Empty, sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty, sourceFrameLedgerSha256 = string.Empty;
        public string colorPrimaries = string.Empty, transferCharacteristics = string.Empty;
        public string matrixCoefficients = string.Empty, signalRange = string.Empty;
        public string inputProfile = string.Empty, outputProfile = string.Empty;
        public string roundingMode = string.Empty, alphaMode = string.Empty;
        public string editorialSourceRole = string.Empty;
        public string parserName = string.Empty, parserVersion = string.Empty;
        public string tool = string.Empty, toolVersion = string.Empty, createdAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvRec709OutputLedgerArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, captureId = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty, sourceShotId = string.Empty;
        public string sourceFrameLedgerSha256 = string.Empty, configSha256 = string.Empty;
        public AuditionPvRec709OutputFrame[] frames = Array.Empty<AuditionPvRec709OutputFrame>();
    }

    [Serializable] internal sealed class AuditionPvRec709OutputFrame
    {
        public int sourceFrame = -1;
        public string sourceFrameSha256 = string.Empty, outputPath = string.Empty;
        public string outputSha256 = string.Empty;
        public int width, height;
        public string colorPrimaries = string.Empty, transferCharacteristics = string.Empty;
        public string matrixCoefficients = string.Empty, signalRange = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvSelectedFrameScanConfigArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, checkId = string.Empty;
        public string captureId = string.Empty, sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty, sourceFrameLedgerSha256 = string.Empty;
        public string tool = string.Empty, toolVersion = string.Empty, algorithm = string.Empty;
        public string algorithmSha256 = string.Empty;
        public int frameStride = -1, temporalPairStride = -1, pixelStride = -1;
        public string createdAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvSelectedFrameScanLedgerArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, checkId = string.Empty;
        public string captureId = string.Empty, sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty, sourceFrameLedgerSha256 = string.Empty;
        public string configSha256 = string.Empty;
        public AuditionPvSelectedFrameScanEntry[] frames =
            Array.Empty<AuditionPvSelectedFrameScanEntry>();
    }

    [Serializable] internal sealed class AuditionPvSelectedFrameScanEntry
    {
        public int sourceFrame = -1;
        public string frameSha256 = string.Empty;
        public int width, height;
        public long sampledPixelCount, errorMagentaPixelCount;
        public long nullMaterialCount, errorMaterialCount;
        public long inspectedCanvasCount, inspectedHudRendererCount, inspectedDrawCommandCount;
        public long visibleUiElementCount, inspectedRendererCount, inspectedMaterialSlotCount;
        public string canvasInventorySha256 = string.Empty, hudInventorySha256 = string.Empty;
        public string rendererInventorySha256 = string.Empty;
        public string materialInventorySha256 = string.Empty;
        public bool rendererHudLayerExcluded;
    }

    [Serializable] internal sealed class AuditionPvRuntimeWorkloadArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, checkId = string.Empty;
        public string captureId = string.Empty, sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty, sourceFrameLedgerSha256 = string.Empty;
        public string scanConfigSha256 = string.Empty, tool = string.Empty, toolVersion = string.Empty;
        public string inventoryIdentityContract = string.Empty;
        public string hudEvidenceMode = string.Empty;
        public AuditionPvPinnedArtifact sceneNoHudContractProof = new();
        public string createdAtUtc = string.Empty;
        public AuditionPvRuntimeFrameWorkload[] frames = Array.Empty<AuditionPvRuntimeFrameWorkload>();
    }

    [Serializable] internal sealed class AuditionPvRuntimeFrameWorkload
    {
        public int sourceFrame = -1;
        public long inspectedCanvasCount, inspectedHudRendererCount, inspectedDrawCommandCount;
        public long visibleUiElementCount;
        public long inspectedRendererCount, inspectedMaterialSlotCount;
        public long nullMaterialCount, errorMaterialCount;
        public string[] canvasStableIds = Array.Empty<string>(), hudRendererStableIds = Array.Empty<string>();
        public string[] rendererStableIds = Array.Empty<string>(), materialSlotStableIds = Array.Empty<string>();
        public string[] canvasAddedStableIds = Array.Empty<string>(), canvasRemovedStableIds = Array.Empty<string>();
        public string[] hudRendererAddedStableIds = Array.Empty<string>(), hudRendererRemovedStableIds = Array.Empty<string>();
        public string[] rendererAddedStableIds = Array.Empty<string>(), rendererRemovedStableIds = Array.Empty<string>();
        public string[] materialSlotAddedStableIds = Array.Empty<string>(), materialSlotRemovedStableIds = Array.Empty<string>();
        public string canvasInventorySha256 = string.Empty, hudInventorySha256 = string.Empty;
        public string rendererInventorySha256 = string.Empty;
        public string materialInventorySha256 = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvSceneNoHudContractArtifact
    {
        public string schemaVersion = string.Empty, sourceCaptureCoreSha256 = string.Empty;
        public string captureId = string.Empty, sourceShotId = string.Empty;
        public bool noHudAuthored;
        public long inspectedObjectCount, authoredHudComponentCount;
        public string tool = string.Empty, toolVersion = string.Empty, createdAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvTakeHumanReviewArtifact : AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty, takeId = string.Empty, captureId = string.Empty;
        public string sourceManifestSha256 = string.Empty, sourceShotId = string.Empty;
        public string bucketId = string.Empty, atomicShotId = string.Empty;
        public string[] beatIds = Array.Empty<string>();
        public bool approved, fullMotionRangeReviewed, noBlackMesh, noBrokenTrail;
        public string reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public AuditionPvMeasuredFrame[] reviewedFrames = Array.Empty<AuditionPvMeasuredFrame>();
    }

    [Serializable] internal sealed class AuditionPvRightsRecordArtifact
    {
        public string schemaVersion = string.Empty, rightsRecordId = string.Empty, scope = string.Empty;
        public string disposition = string.Empty, verifiedBy = string.Empty, verifiedAtUtc = string.Empty;
        public string provider = string.Empty, licenseId = string.Empty, licenseVersion = string.Empty;
        public string accountEntitlementId = string.Empty, useBoundary = string.Empty;
        public string owner = string.Empty, sourceDescription = string.Empty;
        public string accountPlan = string.Empty, exclusionReason = string.Empty;
        public bool attributionRequired;
        public AuditionPvPinnedArtifact termsSnapshot = new(), entitlementEvidence = new();
        public AuditionPvPinnedArtifact attributionArtifact = new();
        public AuditionPvPinnedArtifact generationEvidence = new();
        public bool verified;
        public string[] coveredItemIds = Array.Empty<string>();
        public string[] coveredShotIds = Array.Empty<string>();
    }

    [Serializable] internal sealed class AuditionPvAudioGenerationArtifact
    {
        public string schemaVersion = string.Empty, audioId = string.Empty;
        public string aiUsedItemId = string.Empty, provider = string.Empty, model = string.Empty;
        public string rightsRecordId = string.Empty, accountPlan = string.Empty;
        public string tool = string.Empty, toolVersion = string.Empty, generatedAtUtc = string.Empty;
        public string voiceIdentityDisposition = string.Empty;
        public AuditionPvPinnedArtifact promptArtifact = new(), originalGeneratedWav = new();
        public AuditionPvPinnedArtifact editedWav = new(), derivationRecipe = new();
        public AuditionPvPinnedArtifact consentArtifact = new();
    }

    [Serializable] internal sealed class AuditionPvAudioListeningArtifact
    {
        public string schemaVersion = string.Empty, audioId = string.Empty, fileSha256 = string.Empty;
        public string status = string.Empty, reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvAudioDerivationRecipeArtifact
    {
        public string schemaVersion = string.Empty, audioId = string.Empty;
        public string promptSha256 = string.Empty, originalWavSha256 = string.Empty;
        public string editedWavSha256 = string.Empty, tool = string.Empty, toolVersion = string.Empty;
        public string createdAtUtc = string.Empty;
        public string[] steps = Array.Empty<string>();
    }

    [Serializable] internal sealed class AuditionPvTwelveSecondApprovalArtifact
    {
        public string schemaVersion = string.Empty, manifestId = string.Empty;
        public string twelveSecondManifestSha256 = string.Empty;
        public bool approved; public string approvedBy = string.Empty, approvedAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvVisualReviewArtifact
    {
        public string schemaVersion = string.Empty, manifestId = string.Empty;
        public string productCheckpointGitSha = string.Empty, reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public bool approved; public int downscalePercent;
        public bool faceReadable, bossReadable, attackDirectionReadable, impactPointReadable;
        public bool noPinkShader, noErrorMagenta, noNullMaterial, noBlackMesh, noBrokenTrail;
        public bool endCardLogoReadable, endCardSloganReadable, endCardAuditionNoticeReadable;
        public AuditionPvPinnedArtifact contactSheet = new();
        public int contactSheetColumns, contactSheetRows, contactSheetCellCount;
        public string contactSheetGenerator = string.Empty, contactSheetGeneratorVersion = string.Empty;
        public string[] contactSheetInputSha256 = Array.Empty<string>();
        public string[] approvedTakeReviewSha256 = Array.Empty<string>();
        public string[] approvedEndCardGraphicSha256 = Array.Empty<string>();
        public string[] reviewedFrameSha256 = Array.Empty<string>();
        public AuditionPvVisualCriterionRef[] criterionRefs = Array.Empty<AuditionPvVisualCriterionRef>();
    }

    [Serializable] internal sealed class AuditionPvVisualCriterionRef
    {
        public string criterion = string.Empty, takeId = string.Empty, atomicShotId = string.Empty;
        public int sourceFrame = -1;
        public string frameSha256 = string.Empty, note = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvRightsCoverageReviewArtifact
    {
        public string schemaVersion = string.Empty, manifestId = string.Empty;
        public string productCheckpointGitSha = string.Empty, reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public bool complete; public string[] usedItemIds = Array.Empty<string>();
        public AuditionPvRightsReviewedCaptureIdentity[] reviewedCaptures =
            Array.Empty<AuditionPvRightsReviewedCaptureIdentity>();
        public AuditionPvRightsDependencyClassification[] dependencies =
            Array.Empty<AuditionPvRightsDependencyClassification>();
    }

    [Serializable] internal sealed class AuditionPvRightsReviewedCaptureIdentity
    {
        public string captureId = string.Empty, sourceManifestSha256 = string.Empty;
        public string sourceDependencyIdentitySha256 = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvRightsDependencyClassification
    {
        public string captureId = string.Empty, sourceManifestSha256 = string.Empty;
        public string path = string.Empty, sha256 = string.Empty;
        public long byteLength;
        public string disposition = string.Empty, usedItemId = string.Empty, reason = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvSixtySecondGateIssue
    { public string severity = string.Empty, code = string.Empty, location = string.Empty, message = string.Empty; }

    [Serializable] internal sealed class AuditionPvSixtySecondGateValidationReport
    {
        public string schemaVersion = string.Empty, validationMode = string.Empty, manifestId = string.Empty;
        public string validatedAtUtc = string.Empty, inputManifestPath = string.Empty;
        public string inputManifestSha256 = string.Empty;
        public bool passed, structureValid, productionEvidenceVerified;
        public int errorCount, warningCount, bucketCount, shotCount;
        public int declaredTakeSlotCount, declaredApprovedTakeCount;
        public int declaredGraphicPlaceholderCount;
        public AuditionPvSixtySecondGateIssue[] issues = Array.Empty<AuditionPvSixtySecondGateIssue>();
    }
}
