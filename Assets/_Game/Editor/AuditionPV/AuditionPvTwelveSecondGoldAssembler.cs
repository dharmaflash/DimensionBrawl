using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    internal static class AuditionPvTwelveSecondGoldAssembler
    {
        internal const string SpecificationSchema =
            "dimension-brawl.audition-pv.preedit-12s-segment-spec.v1";
        internal const string ManifestSchema =
            "dimension-brawl.audition-pv.preedit-12s-source-select.v1";
        internal const string ValidationSchema =
            "dimension-brawl.audition-pv.preedit-12s-validation.v1";
        internal const string OutputRoot =
            "D:/DimensionBrawl_PV/02_selects/PREEDIT_12S";
        internal const string DefaultSpecificationPath =
            "D:/DimensionBrawl_PV/02_selects/PREEDIT_12S/preedit_12s_segments.json";
        internal const string ManifestFileName = "preedit_12s_manifest.json";
        internal const string ValidationReportFileName =
            "preedit_12s_validation_report.json";
        internal const string FrameHashFileName = "frame_hashes.sha256";
        internal const string FramesFolderName = "frames";
        internal const string ProxyFileName =
            "preedit_12s_silent_qhd60_h264.mp4";
        internal const string ProxyProbeFileName =
            "preedit_12s_silent_qhd60_h264.ffprobe.json";
        internal const string ContactSheetFileName =
            "preedit_12s_contact_sheet_25pct.png";
        internal const string ContactSheetDownsamplePolicy =
            "rgba8-box-4x4-unpremultiplied-round-half-up-linear-storage-v1";
        internal const string CounterShotId = "g06";
        internal const string G06RuntimeProofFileName =
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProofFileName;
        internal const string G06EvidenceFolderName =
            AuditionPvStationPhase2SummonCounterGoldenRunner.EvidenceFolderName;
        internal const string G06WarmupEvidenceFileName =
            AuditionPvStationPhase2SummonCounterGoldenRunner.WarmupEvidenceFileName;
        internal const int ExpectedFrameCount = 720;
        internal const int ContactSheetCellWidth = 640;
        internal const int ContactSheetCellHeight = 360;
        internal const int ContactSheetColumns = 4;
        internal const int ContactSheetRows = 3;
        internal const int ContactSheetWidth =
            ContactSheetCellWidth * ContactSheetColumns;
        internal const int ContactSheetHeight =
            ContactSheetCellHeight * ContactSheetRows;
        internal const string RequiredProxyToolVersion = "8.1.2";
        internal const string DefaultFfmpegPath =
            "C:/Users/dharm/Documents/Codex/2026-07-16/new-chat/work/ae_pv_proof/ffmpeg/ffmpeg-8.1.2-essentials_build/bin/ffmpeg.exe";
        internal const string DefaultFfmpegSha256 =
            "1326dde4c84ff1f96fe6b8916c5bed29e163e9b5dccf995f6f3db069d143ec5e";
        internal const string DefaultFfprobePath =
            "C:/Users/dharm/Documents/Codex/2026-07-16/new-chat/work/ae_pv_proof/ffmpeg/ffmpeg-8.1.2-essentials_build/bin/ffprobe.exe";
        internal const string DefaultFfprobeSha256 =
            "b49ccc7c6547b141ad5a2f6ec69cc04323d7133d7704d70b331b904c63eecb07";

        private const string SpecificationArgument = "-auditionPv12sSpec";
        private const string MenuPath =
            "DimensionBrawl/Audition PV/Assemble 12-Second Gold Source Select";
        private const int CopyBufferBytes = 1024 * 1024;

        internal static readonly string[] RequiredRoles =
        {
            "city-wide",
            "city-gameplay",
            "dimension-transition",
            "olympus-c33-c34",
            "perfect-dodge-counter"
        };

        internal static readonly int[] ContactSheetOutputFrames =
        {
            30, 90, 150, 210, 270, 330,
            390, 450, 510, 570, 630, 690
        };

        private static readonly string[] RequiredHudModes =
        {
            "hud-off",
            "hud-on",
            "hud-off",
            "hud-off",
            "hud-on"
        };

        // JsonUtility can move one emitted binary64 metric by one ULP during
        // FromJson -> ToJson. Only these declared double-valued leaf tokens may
        // differ, and only by <= 1 ULP; the parser preserves every other lexeme
        // exactly and the typed gameplay/render predicates still validate the
        // source-parsed DTO below.
        private static readonly HashSet<string> RelaxedG06DoubleMetricPaths =
            new(StringComparer.Ordinal)
            {
                "runtime.visualMetrics.blackRatio",
                "runtime.visualMetrics.magentaRatio",
                "runtime.visualMetrics.maximumFrameMagentaRatio",
                "runtime.screenDelta.meanAbsoluteRgb",
                "runtime.screenDelta.changedSampleRatio",
                "runtime.counterDelta.meanAbsoluteRgb",
                "runtime.counterDelta.changedSampleRatio"
            };

        private static readonly string[] RequiredValidationChecks =
        {
            "required-semantic-roles-exact-once-and-order",
            "g06-perfect-dodge-counter-source",
            "g06-runtime-proof-sha256-content-and-failure-absence",
            "current-head-clean-and-source-git-identical",
            "source-manifest-and-dependency-identities-pinned",
            "source-frame-sha256-pins-preflight-and-copy-verified",
            "source-ranges-hud-and-baselines-valid",
            "qhd60-png-headers-valid",
            "contiguous-frame-0000-through-0719",
            "per-frame-source-map-and-sha256-valid",
            "contact-sheet-25pct-cells-and-input-sha256-valid",
            "silent-h264-qhd60-cfr60-720f-ffprobe-valid",
            "create-new-staging-ready-for-atomic-install"
        };

        private static readonly byte[] PngSignature =
        {
            0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a
        };

        [MenuItem(MenuPath)]
        public static void AssembleMenu()
        {
            AuditionPvTwelveSecondAssemblyResult result =
                AssembleFromSpecificationFile(ResolveSpecificationPath());
            Debug.Log(
                "[AuditionPvTwelveSecondGoldAssembler] PASS: "
                + result.outputDirectory);
            EditorUtility.RevealInFinder(result.outputDirectory);
        }

        public static void RunBatchAssembly()
        {
            try
            {
                AuditionPvTwelveSecondAssemblyResult result =
                    AssembleFromSpecificationFile(ResolveSpecificationPath());
                Debug.Log(
                    "[AuditionPvTwelveSecondGoldAssembler] PASS: "
                    + result.outputDirectory);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                    return;
                }

                throw;
            }
        }

        internal static AuditionPvTwelveSecondAssemblyResult
            AssembleFromSpecificationFile(string specificationPath)
        {
            AuditionPvTwelveSecondSegmentManifest specification =
                ReadSpecification(specificationPath);
            AuditionPvGitSnapshot git =
                AuditionPvEnvironmentProbe.ReadGitSnapshot();
            return Assemble(
                specification,
                AuditionPvCaptureContract.OutputRoot,
                OutputRoot,
                git,
                DateTime.UtcNow,
                outputIdOverride: null,
                finalGitProbe: AuditionPvEnvironmentProbe.ReadGitSnapshot);
        }

        internal static AuditionPvTwelveSecondAssemblyResult Assemble(
            AuditionPvTwelveSecondSegmentManifest specification,
            string sourceRoot,
            string outputRoot,
            AuditionPvGitSnapshot currentGit,
            DateTime createdAtUtc,
            string outputIdOverride,
            Func<AuditionPvGitSnapshot> finalGitProbe,
            IAuditionPvTwelveSecondProxyEncoder proxyEncoder = null)
        {
            // Product semantics are checked before output-root creation, destination
            // selection, staging reservation, or frame copying. In particular, G05
            // cannot stand in for the required real perfect-dodge-plus-counter G06.
            ValidateSegmentContract(specification);
            if (finalGitProbe == null)
            {
                throw new ArgumentNullException(
                    nameof(finalGitProbe),
                    "A fresh final Git probe is required for atomic installation.");
            }

            ValidateCleanGit(currentGit, "current worktree");

            string normalizedSourceRoot = NormalizeAbsoluteRoot(sourceRoot);
            string normalizedOutputRoot = NormalizeAbsoluteRoot(outputRoot);
            RejectExistingReparseChain(
                normalizedSourceRoot,
                "golden-source root");
            RejectExistingReparseChain(
                normalizedOutputRoot,
                "select output root");
            if (IsSameOrDescendant(
                    normalizedOutputRoot,
                    normalizedSourceRoot) ||
                IsSameOrDescendant(
                    normalizedSourceRoot,
                    normalizedOutputRoot))
            {
                throw new InvalidDataException(
                    "Golden-source and select output roots must be disjoint.");
            }
            AuditionPvTwelveSecondProxyToolSpec proxyTools =
                ValidateProxyTools(specification.proxyTools);
            ValidatedPlan plan = BuildValidatedPlan(
                specification,
                normalizedSourceRoot,
                currentGit);

            DateTime normalizedCreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Utc
                ? createdAtUtc
                : createdAtUtc.ToUniversalTime();
            string outputId = ResolveNewOutputId(
                normalizedOutputRoot,
                currentGit,
                normalizedCreatedAtUtc,
                outputIdOverride);
            string finalDirectory = ResolveDirectChild(
                normalizedOutputRoot,
                outputId,
                requireSimpleName: true);
            if (Directory.Exists(finalDirectory) || File.Exists(finalDirectory))
            {
                throw new IOException(
                    "12-second select destination already exists and will not be "
                    + "overwritten: " + finalDirectory);
            }

            Directory.CreateDirectory(normalizedOutputRoot);
            string stagingDirectory = CreateStagingDirectory(
                normalizedOutputRoot,
                outputId);
            bool installed = false;
            try
            {
                AuditionPvTwelveSecondSelectManifest manifest = MaterializeStaging(
                    plan,
                    stagingDirectory,
                    finalDirectory,
                    normalizedSourceRoot,
                    normalizedOutputRoot,
                    outputId,
                    currentGit,
                    normalizedCreatedAtUtc,
                    proxyTools,
                    proxyEncoder ?? new ExternalProxyEncoder());

                ValidateMaterializedPackage(
                    stagingDirectory,
                    finalDirectory,
                    manifest);
                string manifestPath = Path.Combine(
                    stagingDirectory,
                    ManifestFileName);
                string manifestSha256 = FileSha256(manifestPath);
                WriteValidationReport(
                    stagingDirectory,
                    finalDirectory,
                    manifest,
                    manifestSha256,
                    normalizedCreatedAtUtc);
                ValidateValidationReport(
                    stagingDirectory,
                    finalDirectory,
                    manifest,
                    manifestSha256);

                ValidateProxyToolPins(proxyTools);
                AuditionPvGitSnapshot gitAtInstall = finalGitProbe();
                ValidateStableGit(currentGit, gitAtInstall);
                ValidateSourceIdentityPins(plan);
                if (Directory.Exists(finalDirectory) || File.Exists(finalDirectory))
                {
                    throw new IOException(
                        "12-second select destination appeared before atomic install: "
                        + finalDirectory);
                }

                Directory.Move(stagingDirectory, finalDirectory);
                installed = true;
                return new AuditionPvTwelveSecondAssemblyResult
                {
                    outputId = outputId,
                    outputDirectory = NormalizePath(finalDirectory),
                    manifestPath = NormalizePath(
                        Path.Combine(finalDirectory, ManifestFileName)),
                    validationReportPath = NormalizePath(
                        Path.Combine(finalDirectory, ValidationReportFileName)),
                    frameHashPath = NormalizePath(
                        Path.Combine(finalDirectory, FrameHashFileName)),
                    contactSheetPath = NormalizePath(
                        Path.Combine(finalDirectory, ContactSheetFileName)),
                    proxyPath = NormalizePath(
                        Path.Combine(finalDirectory, ProxyFileName)),
                    frameCount = ExpectedFrameCount
                };
            }
            finally
            {
                if (!installed && Directory.Exists(stagingDirectory))
                {
                    Directory.Delete(stagingDirectory, recursive: true);
                }
            }
        }

        internal static void ValidateSegmentContract(
            AuditionPvTwelveSecondSegmentManifest specification)
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            if (!string.Equals(
                    specification.schemaVersion,
                    SpecificationSchema,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Unsupported 12-second segment specification schema.");
            }

            AuditionPvTwelveSecondSegmentSpec[] segments =
                specification.segments
                ?? Array.Empty<AuditionPvTwelveSecondSegmentSpec>();
            ValidateProxyToolSpecification(specification.proxyTools);
            if (segments.Length != RequiredRoles.Length)
            {
                throw new InvalidDataException(
                    "The product Gate requires exactly five semantic segments.");
            }

            int total = 0;
            for (int index = 0; index < segments.Length; index++)
            {
                AuditionPvTwelveSecondSegmentSpec segment = segments[index]
                    ?? throw new InvalidDataException(
                        "The segment specification contains a null entry.");
                if (segment.order != index)
                {
                    throw new InvalidDataException(
                        $"Segment order must be contiguous 0..4; index {index} "
                        + $"declared order {segment.order}.");
                }

                if (!string.Equals(
                        segment.role,
                        RequiredRoles[index],
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Segment {index} must have semantic role "
                        + $"'{RequiredRoles[index]}'.");
                }

                ValidateSimpleId(segment.shotId, "source shot ID");
                if (segment.startFrame < 0 ||
                    segment.endFrame < segment.startFrame)
                {
                    throw new InvalidDataException(
                        $"Segment '{segment.role}' has an invalid inclusive range.");
                }

                if (string.IsNullOrWhiteSpace(segment.sourceManifestPath) ||
                    ContainsTraversalSegment(segment.sourceManifestPath))
                {
                    throw new InvalidDataException(
                        $"Segment '{segment.role}' has an unsafe source manifest path.");
                }

                if (!AuditionPvSha256.IsSha256(
                        segment.sourceManifestSha256) ||
                    !AuditionPvSha256.IsSha256(
                        segment.sourceDependencyIdentitySha256))
                {
                    throw new InvalidDataException(
                        $"Segment '{segment.role}' must pin both source manifest and "
                        + "dependency-identity SHA-256 values.");
                }

                bool requiresRuntimeProof = index == segments.Length - 1;
                if (requiresRuntimeProof != AuditionPvSha256.IsSha256(
                        segment.sourceRuntimeProofSha256) ||
                    !requiresRuntimeProof &&
                    !string.IsNullOrEmpty(segment.sourceRuntimeProofSha256))
                {
                    throw new InvalidDataException(
                        "Only the final G06 perfect-dodge-counter segment must pin "
                        + "one canonical runtime-proof SHA-256 value.");
                }

                checked
                {
                    total += segment.endFrame - segment.startFrame + 1;
                }
            }

            AuditionPvTwelveSecondSegmentSpec counter = segments[^1];
            if (!string.Equals(
                    counter.shotId,
                    CounterShotId,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The final perfect-dodge-counter role requires a real G06 "
                    + "counter source; G05 perfect-dodge-only footage is provisional "
                    + "and cannot pass the product Gate.");
            }

            if (total != ExpectedFrameCount)
            {
                throw new InvalidDataException(
                    $"The 12-second QHD60 select must contain exactly "
                    + $"{ExpectedFrameCount} frames; specification contains {total}.");
            }
        }

        internal static string ComputeDependencyIdentityForTests(
            AuditionPvCaptureManifest manifest)
        {
            return ComputeDependencyIdentity(manifest);
        }

        internal static AuditionPvTwelveSecondSelectManifest
            ReadInstalledManifest(string outputDirectory)
        {
            string normalized = Path.GetFullPath(outputDirectory);
            string path = Path.Combine(normalized, ManifestFileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Installed 12-second select manifest is missing.",
                    path);
            }

            RejectReparsePoint(path, "installed select manifest");
            return JsonUtility.FromJson<AuditionPvTwelveSecondSelectManifest>(
                File.ReadAllText(path, Encoding.UTF8));
        }

        internal static void ValidateInstalledPackage(string outputDirectory)
        {
            string normalized = Path.GetFullPath(outputDirectory);
            RejectExistingReparseChain(
                normalized,
                "installed select package root");
            AuditionPvTwelveSecondSelectManifest manifest =
                ReadInstalledManifest(normalized);
            ValidateMaterializedPackage(
                normalized,
                normalized,
                manifest);
            ValidateValidationReport(
                normalized,
                normalized,
                manifest,
                FileSha256(Path.Combine(normalized, ManifestFileName)));
        }

        private static ValidatedPlan BuildValidatedPlan(
            AuditionPvTwelveSecondSegmentManifest specification,
            string sourceRoot,
            AuditionPvGitSnapshot currentGit)
        {
            var sourcesByPath = new Dictionary<string, LoadedSource>(
                StringComparer.OrdinalIgnoreCase);
            var segments = new List<ValidatedSegment>(
                specification.segments.Length);
            int nextOutputFrame = 0;

            foreach (AuditionPvTwelveSecondSegmentSpec segment in
                     specification.segments.OrderBy(value => value.order))
            {
                string manifestPath = ValidateAndResolveManifestPath(
                    sourceRoot,
                    segment.sourceManifestPath);
                if (!sourcesByPath.TryGetValue(
                        manifestPath,
                        out LoadedSource source))
                {
                    source = LoadAndValidateSource(
                        sourceRoot,
                        manifestPath,
                        segment.sourceManifestSha256,
                        segment.sourceDependencyIdentitySha256,
                        currentGit);
                    sourcesByPath.Add(manifestPath, source);
                }
                else
                {
                    RequirePinnedIdentity(segment, source);
                }

                AuditionPvShotManifestEntry shot = source.manifest.shots
                    .SingleOrDefault(value => string.Equals(
                        value.id,
                        segment.shotId,
                        StringComparison.Ordinal));
                if (shot == null)
                {
                    throw new InvalidDataException(
                        $"Source capture '{source.manifest.captureId}' does not "
                        + $"contain shot '{segment.shotId}'.");
                }

                if (segment.startFrame < shot.startFrame ||
                    segment.endFrame > shot.endFrame)
                {
                    throw new InvalidDataException(
                        $"Segment '{segment.role}' range "
                        + $"{segment.startFrame}..{segment.endFrame} is outside "
                        + $"source shot '{shot.id}' range "
                        + $"{shot.startFrame}..{shot.endFrame}.");
                }

                ValidateSourceFramePinDefinition(segment);

                string requiredHud = RequiredHudModes[segment.order];
                if (!string.Equals(
                        shot.hudMode,
                        requiredHud,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Segment '{segment.role}' requires {requiredHud}, but "
                        + $"source shot '{shot.id}' declares '{shot.hudMode}'.");
                }

                int count = segment.endFrame - segment.startFrame + 1;
                segments.Add(new ValidatedSegment
                {
                    specification = segment,
                    source = source,
                    shot = shot,
                    outputStartFrame = nextOutputFrame,
                    outputEndFrame = nextOutputFrame + count - 1
                });
                nextOutputFrame += count;
            }

            ValidateCounterSemanticBaselines(segments[^1]);
            ValidateSharedDependencyEntries(sourcesByPath.Values);
            foreach (IGrouping<string, ValidatedSegment> referencedShot in
                     segments.GroupBy(
                         value => value.source.manifestPath + "\0" + value.shot.id,
                         StringComparer.OrdinalIgnoreCase))
            {
                ValidatedSegment first = referencedShot.First();
                ValidateCompleteSourceShot(first.source, first.shot);
            }

            ValidateSelectedSourceFramePins(segments);

            foreach (LoadedSource source in sourcesByPath.Values)
            {
                ValidateSourceBaselines(
                    source,
                    segments.Where(value => ReferenceEquals(value.source, source)));
            }

            return new ValidatedPlan
            {
                segments = segments.ToArray(),
                sources = sourcesByPath.Values
                    .OrderBy(value => value.manifestPath, StringComparer.Ordinal)
                    .ToArray()
            };
        }

        private static void ValidateSourceFramePinDefinition(
            AuditionPvTwelveSecondSegmentSpec segment)
        {
            string[] pins = segment.sourceFrameSha256
                            ?? Array.Empty<string>();
            int expectedCount = segment.endFrame - segment.startFrame + 1;
            if (pins.Length != expectedCount)
            {
                throw new InvalidDataException(
                    $"Segment '{segment.role}' must provide exactly "
                    + $"{expectedCount} source-frame SHA-256 pins in inclusive "
                    + "source-frame order.");
            }

            if (pins.Any(value => !AuditionPvSha256.IsSha256(value)))
            {
                throw new InvalidDataException(
                    $"Segment '{segment.role}' contains an invalid source-frame "
                    + "SHA-256 pin.");
            }
        }

        private static void ValidateSelectedSourceFramePins(
            IEnumerable<ValidatedSegment> segments)
        {
            foreach (ValidatedSegment segment in segments)
            {
                for (int sourceFrame = segment.specification.startFrame;
                     sourceFrame <= segment.specification.endFrame;
                     sourceFrame++)
                {
                    string path = ResolveSourceFramePath(
                        segment.source.captureDirectory,
                        segment.shot.id,
                        sourceFrame);
                    string expected = segment.specification.sourceFrameSha256[
                        sourceFrame - segment.specification.startFrame];
                    if (!string.Equals(
                            FileSha256(path),
                            expected,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            $"Pinned source-frame SHA-256 mismatch for segment "
                            + $"'{segment.specification.role}', source frame "
                            + $"{sourceFrame}: {path}");
                    }
                }
            }
        }

        private static void ValidateCounterSemanticBaselines(
            ValidatedSegment counterSegment)
        {
            var evidenceFrames = new Dictionary<string, int>(
                StringComparer.Ordinal);
            foreach (string baselineId in new[] { "bl06", "bl07" })
            {
                AuditionPvBaselineManifestEntry baseline =
                    counterSegment.source.manifest.baselines.SingleOrDefault(
                        value => value != null &&
                                 string.Equals(
                                     value.id,
                                     baselineId,
                                     StringComparison.Ordinal) &&
                                 string.Equals(
                                     value.shotId,
                                     CounterShotId,
                                     StringComparison.Ordinal));
                if (baseline == null ||
                    baseline.sourceFrame <
                    counterSegment.specification.startFrame ||
                    baseline.sourceFrame >
                    counterSegment.specification.endFrame ||
                    !string.Equals(
                        baseline.hudMode,
                        "hud-on",
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        baseline.status,
                        "captured",
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "The G06 perfect-dodge-counter segment must include both "
                        + "captured HUD-on BL06 perfect-dodge and BL07 real summon-"
                        + "counter baseline keys inside its selected range.");
                }

                evidenceFrames.Add(baselineId, baseline.sourceFrame);
            }

            if (evidenceFrames["bl06"] >= evidenceFrames["bl07"])
            {
                throw new InvalidDataException(
                    "G06 BL06 perfect-dodge evidence must precede the distinct "
                    + "BL07 real summon-counter evidence frame.");
            }

            LoadAndValidateG06RuntimeProof(counterSegment);

            string captureDirectory = counterSegment.source.captureDirectory;
            string proofPath = counterSegment.source.runtimeProofPath;
            string warmupPath = ResolveG06EvidencePath(
                captureDirectory,
                G06WarmupEvidenceFileName);
            string framesPath = ResolveDirectChildPath(
                ResolveDirectChildPath(captureDirectory, "frames"),
                CounterShotId);
            string baselineDirectory = ResolveDirectChildPath(
                captureDirectory,
                "baselines");
            var requiredTests = new[]
            {
                new
                {
                    suite = "recorder",
                    name = "raw-warmup-and-logical-frame-mapping",
                    artifactPath = warmupPath
                },
                new
                {
                    suite = "product-state",
                    name = "real-station-phase2-perfect-dodge-slot1-counter",
                    artifactPath = proofPath
                },
                new
                {
                    suite = "render",
                    name = "png-hud-and-visual-sanity",
                    artifactPath = framesPath
                },
                new
                {
                    suite = "render",
                    name = "perfect-dodge-screen-domain-f189",
                    artifactPath = ResolveDirectChildPath(
                        baselineDirectory,
                        counterSegment.source.manifest.baselines.Single(value =>
                            string.Equals(
                                value.id,
                                "bl06",
                                StringComparison.Ordinal)).fileName)
                },
                new
                {
                    suite = "render",
                    name = "slot1-screen-intercept-counter-f251",
                    artifactPath = ResolveDirectChildPath(
                        baselineDirectory,
                        counterSegment.source.manifest.baselines.Single(value =>
                            string.Equals(
                                value.id,
                                "bl07",
                                StringComparison.Ordinal)).fileName)
                },
                new
                {
                    suite = "provenance",
                    name = "git-dependencies-and-station-scene-stable",
                    artifactPath = proofPath
                },
                new
                {
                    suite = "lifecycle",
                    name = "state-restored-and-product-scene-reopened",
                    artifactPath = proofPath
                }
            };
            if (counterSegment.source.manifest.testResults.Length !=
                requiredTests.Length)
            {
                throw new InvalidDataException(
                    "The G06 source must contain the exact seven-result golden-runner "
                    + "semantic evidence test set without substituted or extra results.");
            }

            foreach (var required in requiredTests)
            {
                int count = counterSegment.source.manifest.testResults.Count(value =>
                    value != null &&
                    string.Equals(
                        value.suite,
                        required.suite,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        value.name,
                        required.name,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        value.status,
                        "passed",
                        StringComparison.Ordinal) &&
                    PathsEqual(value.artifactPath, required.artifactPath));
                if (count != 1)
                {
                    throw new InvalidDataException(
                        "The G06 source must contain exactly one passed, canonically "
                        + "linked golden-runner evidence test "
                        + $"'{required.suite}/{required.name}'.");
                }
            }
        }

        private static void LoadAndValidateG06RuntimeProof(
            ValidatedSegment counterSegment)
        {
            LoadedSource source = counterSegment.source;
            ValidateNoG06FailureArtifacts(source.captureDirectory);
            string proofPath = ResolveG06EvidencePath(
                source.captureDirectory,
                G06RuntimeProofFileName);
            if (!File.Exists(proofPath))
            {
                throw new FileNotFoundException(
                    "The canonical G06 runtime proof is missing.",
                    proofPath);
            }

            RejectExistingReparseChain(proofPath, "G06 runtime proof");
            byte[] bytes = File.ReadAllBytes(proofPath);
            string sha256 = BytesSha256(bytes);
            if (!string.Equals(
                    sha256,
                    counterSegment.specification.sourceRuntimeProofSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The canonical G06 runtime-proof SHA-256 does not match the "
                    + "segment specification pin.");
            }

            ValidateG06RuntimeProofDocument(
                source,
                DecodeStrictUtf8Json(bytes, "G06 runtime proof"));
            source.runtimeProofPath = NormalizePath(proofPath);
            source.runtimeProofSha256 = sha256;
        }

        private static void ValidateG06RuntimeProofDocument(
            LoadedSource source,
            string json)
        {
            G06JsonLexicalDocument sourceLexical =
                ParseG06JsonLexicalDocument(json, "source G06 runtime proof");
            AuditionPvG06RuntimeProofArtifact artifact =
                JsonUtility.FromJson<AuditionPvG06RuntimeProofArtifact>(json);
            if (artifact == null ||
                !string.Equals(
                    artifact.schema,
                    AuditionPvG06RuntimeProofArtifact.Schema,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    artifact.captureId,
                    source.manifest.captureId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    artifact.mapping,
                    AuditionPvG06RuntimeProofArtifact.Mapping,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    artifact.productScreenProfile,
                    AuditionPvG06RuntimeProofArtifact.ProductScreenProfile,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    artifact.summonCounterContract,
                    AuditionPvG06RuntimeProofArtifact.SummonCounterContract,
                    StringComparison.Ordinal) ||
                artifact.runtime == null)
            {
                throw new InvalidDataException(
                    "The G06 runtime-proof wrapper, capture identity, or exact "
                    + "authored contract text is invalid.");
            }

            string canonicalJson = JsonUtility.ToJson(artifact, true)
                                   + Environment.NewLine;
            G06JsonLexicalDocument canonicalLexical =
                ParseG06JsonLexicalDocument(
                    canonicalJson,
                    "canonical G06 runtime proof");
            ValidateG06JsonLexicalEquivalence(
                sourceLexical,
                canonicalLexical);

            ValidateFiniteG06RuntimeProofNumbers(artifact.runtime);

            try
            {
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(artifact.runtime);
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateVisualSequence(artifact.runtime.visualMetrics);
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateScreenDelta(artifact.runtime.screenDelta);
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateCounterDelta(artifact.runtime.counterDelta);
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidDataException(
                    "The G06 runtime proof failed the golden runner's exact "
                    + "gameplay, render, Recorder, or restoration predicates.",
                    exception);
            }

            float captureDelta =
                artifact.runtime.recorderCaptureDeltaTimeAtLogicalFrameZero;
            float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
            if (captureDelta <= minimumDelta ||
                captureDelta >= minimumDelta + 0.001f ||
                Math.Abs(artifact.runtime.hudEnergyMaxMana - 300f) > 0.001f)
            {
                throw new InvalidDataException(
                    "The G06 runtime proof does not preserve the exact Recorder "
                    + "padding or authored full-energy 300->100 contract.");
            }

            string warmupPath = ResolveG06EvidencePath(
                source.captureDirectory,
                G06WarmupEvidenceFileName);
            if (!PathsEqual(
                    artifact.runtime.warmupEvidencePath,
                    warmupPath) ||
                !File.Exists(warmupPath))
            {
                throw new InvalidDataException(
                    "The G06 runtime proof is not linked to the canonical warm-up "
                    + "evidence PNG.");
            }

            RejectExistingReparseChain(warmupPath, "G06 warm-up evidence");
            ValidatePngHeader(
                warmupPath,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            if (!AuditionPvSha256.IsSha256(
                    artifact.runtime.warmupEvidenceSha256) ||
                !string.Equals(
                    FileSha256(warmupPath),
                    artifact.runtime.warmupEvidenceSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The G06 warm-up evidence SHA-256 does not match the runtime "
                    + "proof.");
            }
        }

        internal static void ValidateG06JsonLexicalEquivalenceForTests(
            string sourceJson,
            string canonicalJson)
        {
            ValidateG06JsonLexicalEquivalence(
                ParseG06JsonLexicalDocument(sourceJson, "test source JSON"),
                ParseG06JsonLexicalDocument(
                    canonicalJson,
                    "test canonical JSON"));
        }

        private static string DecodeStrictUtf8Json(byte[] bytes, string label)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (bytes.Length >= 3 &&
                bytes[0] == 0xef &&
                bytes[1] == 0xbb &&
                bytes[2] == 0xbf)
            {
                throw new InvalidDataException(
                    label + " must be BOM-free UTF-8 JSON.");
            }

            try
            {
                return new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false,
                    throwOnInvalidBytes: true).GetString(bytes);
            }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException(
                    label + " contains invalid UTF-8.",
                    exception);
            }
        }

        private static G06JsonLexicalDocument ParseG06JsonLexicalDocument(
            string json,
            string label)
        {
            return new G06JsonLexicalParser(json, label).Parse();
        }

        private static void ValidateG06JsonLexicalEquivalence(
            G06JsonLexicalDocument source,
            G06JsonLexicalDocument canonical)
        {
            int mismatch = FirstOrdinalMismatch(
                source.skeleton,
                canonical.skeleton);
            if (mismatch >= 0)
            {
                throw new InvalidDataException(
                    "The G06 runtime proof differs from the canonical v1 JSON "
                    + "lexical structure at skeleton index "
                    + mismatch.ToString(CultureInfo.InvariantCulture)
                    + " (source "
                    + DescribeCodeUnit(source.skeleton, mismatch)
                    + ", canonical "
                    + DescribeCodeUnit(canonical.skeleton, mismatch)
                    + "). Keys, order, duplicate state, strings, trivia, integer/"
                    + "float numbers, and EOF must be exact.");
            }

            if (source.relaxedNumbers.Length !=
                canonical.relaxedNumbers.Length)
            {
                throw new InvalidDataException(
                    "The G06 runtime proof has a different canonical double-"
                    + "metric token count.");
            }

            for (int index = 0;
                 index < source.relaxedNumbers.Length;
                 index++)
            {
                G06RelaxedJsonNumber sourceNumber =
                    source.relaxedNumbers[index];
                G06RelaxedJsonNumber canonicalNumber =
                    canonical.relaxedNumbers[index];
                if (!string.Equals(
                        sourceNumber.path,
                        canonicalNumber.path,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "The G06 runtime proof changed a canonical double-metric "
                        + "property path.");
                }

                if (string.Equals(
                        sourceNumber.raw,
                        canonicalNumber.raw,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                ulong ulpDistance = DoubleUlpDistance(
                    sourceNumber.value,
                    canonicalNumber.value);
                if (ulpDistance > 1UL)
                {
                    throw new InvalidDataException(
                        "The G06 runtime proof double metric '"
                        + sourceNumber.path
                        + "' differs from the canonical JsonUtility round-trip "
                        + "by "
                        + ulpDistance.ToString(CultureInfo.InvariantCulture)
                        + " ULPs; at most one ULP is permitted.");
                }
            }
        }

        private static int FirstOrdinalMismatch(string left, string right)
        {
            int sharedLength = Math.Min(left.Length, right.Length);
            for (int index = 0; index < sharedLength; index++)
            {
                if (left[index] != right[index])
                {
                    return index;
                }
            }

            return left.Length == right.Length ? -1 : sharedLength;
        }

        private static string DescribeCodeUnit(string value, int index)
        {
            return index >= value.Length
                ? "<EOF>"
                : "U+" + ((int)value[index]).ToString(
                    "X4",
                    CultureInfo.InvariantCulture);
        }

        private static ulong DoubleUlpDistance(double left, double right)
        {
            ulong leftOrdered = OrderedDoubleBits(left);
            ulong rightOrdered = OrderedDoubleBits(right);
            return leftOrdered >= rightOrdered
                ? leftOrdered - rightOrdered
                : rightOrdered - leftOrdered;
        }

        private static ulong OrderedDoubleBits(double value)
        {
            ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
            const ulong SignBit = 0x8000000000000000UL;
            return (bits & SignBit) == 0UL
                ? bits | SignBit
                : ~bits;
        }

        private sealed class G06JsonLexicalParser
        {
            private const char RelaxedNumberStart = '\u0001';
            private const char RelaxedNumberEnd = '\u0002';

            private readonly string json;
            private readonly string label;
            private readonly StringBuilder skeleton = new();
            private readonly List<G06RelaxedJsonNumber> relaxedNumbers = new();
            private int index;

            public G06JsonLexicalParser(string json, string label)
            {
                this.json = json ?? throw new InvalidDataException(
                    label + " is null.");
                this.label = label;
            }

            public G06JsonLexicalDocument Parse()
            {
                ParseTrivia();
                ParseValue(string.Empty);
                ParseTrivia();
                if (index != json.Length)
                {
                    Fail("contains trailing non-JSON content");
                }

                return new G06JsonLexicalDocument(
                    skeleton.ToString(),
                    relaxedNumbers.ToArray());
            }

            private void ParseValue(string path)
            {
                if (index >= json.Length)
                {
                    Fail("ends before a JSON value");
                }

                char value = json[index];
                switch (value)
                {
                    case '{':
                        ParseObject(path);
                        return;
                    case '[':
                        ParseArray(path);
                        return;
                    case '"':
                        ParseString();
                        return;
                    case 't':
                        ParseLiteral("true");
                        return;
                    case 'f':
                        ParseLiteral("false");
                        return;
                    case 'n':
                        ParseLiteral("null");
                        return;
                    default:
                        if (value == '-' || IsDigit(value))
                        {
                            ParseNumber(path);
                            return;
                        }

                        Fail("contains an invalid JSON value token");
                        return;
                }
            }

            private void ParseObject(string path)
            {
                AppendExpected('{');
                ParseTrivia();
                if (TryAppend('}'))
                {
                    return;
                }

                var propertyNames = new HashSet<string>(StringComparer.Ordinal);
                while (true)
                {
                    if (index >= json.Length || json[index] != '"')
                    {
                        Fail("contains an object member without a string key");
                    }

                    string propertyName = ParseString();
                    if (!propertyNames.Add(propertyName))
                    {
                        Fail(
                            "contains duplicate object key '"
                            + propertyName
                            + "'");
                    }

                    ParseTrivia();
                    AppendExpected(':');
                    ParseTrivia();
                    string propertyPath = string.IsNullOrEmpty(path)
                        ? propertyName
                        : path + "." + propertyName;
                    ParseValue(propertyPath);
                    ParseTrivia();
                    if (TryAppend('}'))
                    {
                        return;
                    }

                    AppendExpected(',');
                    ParseTrivia();
                }
            }

            private void ParseArray(string path)
            {
                AppendExpected('[');
                ParseTrivia();
                if (TryAppend(']'))
                {
                    return;
                }

                int elementIndex = 0;
                while (true)
                {
                    ParseValue(
                        path
                        + "["
                        + elementIndex.ToString(CultureInfo.InvariantCulture)
                        + "]");
                    elementIndex++;
                    ParseTrivia();
                    if (TryAppend(']'))
                    {
                        return;
                    }

                    AppendExpected(',');
                    ParseTrivia();
                }
            }

            private string ParseString()
            {
                int start = index;
                index++;
                var decoded = new StringBuilder();
                while (index < json.Length)
                {
                    char character = json[index++];
                    if (character == '"')
                    {
                        ValidateDecodedString(decoded, start);
                        skeleton.Append(json, start, index - start);
                        return decoded.ToString();
                    }

                    if (character < 0x20)
                    {
                        Fail("contains an unescaped control character in a string");
                    }

                    if (character != '\\')
                    {
                        decoded.Append(character);
                        continue;
                    }

                    if (index >= json.Length)
                    {
                        Fail("ends inside a JSON string escape");
                    }

                    char escape = json[index++];
                    switch (escape)
                    {
                        case '"':
                            decoded.Append('"');
                            break;
                        case '\\':
                            decoded.Append('\\');
                            break;
                        case '/':
                            decoded.Append('/');
                            break;
                        case 'b':
                            decoded.Append('\b');
                            break;
                        case 'f':
                            decoded.Append('\f');
                            break;
                        case 'n':
                            decoded.Append('\n');
                            break;
                        case 'r':
                            decoded.Append('\r');
                            break;
                        case 't':
                            decoded.Append('\t');
                            break;
                        case 'u':
                            if (index + 4 > json.Length)
                            {
                                Fail("ends inside a JSON Unicode escape");
                            }

                            int codeUnit = 0;
                            for (int digit = 0; digit < 4; digit++)
                            {
                                int value = HexDigitValue(json[index + digit]);
                                if (value < 0)
                                {
                                    Fail("contains an invalid JSON Unicode escape");
                                }

                                codeUnit = codeUnit * 16 + value;
                            }

                            index += 4;
                            decoded.Append((char)codeUnit);
                            break;
                        default:
                            Fail("contains an invalid JSON string escape");
                            break;
                    }
                }

                Fail("ends inside an unterminated JSON string");
                return string.Empty;
            }

            private void ValidateDecodedString(StringBuilder decoded, int start)
            {
                for (int character = 0; character < decoded.Length; character++)
                {
                    char value = decoded[character];
                    if (char.IsHighSurrogate(value))
                    {
                        if (character + 1 >= decoded.Length ||
                            !char.IsLowSurrogate(decoded[character + 1]))
                        {
                            FailAt(
                                start,
                                "contains an unpaired high surrogate in a string");
                        }

                        character++;
                    }
                    else if (char.IsLowSurrogate(value))
                    {
                        FailAt(
                            start,
                            "contains an unpaired low surrogate in a string");
                    }
                }
            }

            private void ParseNumber(string path)
            {
                int start = index;
                if (json[index] == '-')
                {
                    index++;
                    if (index >= json.Length)
                    {
                        Fail("ends after a JSON number sign");
                    }
                }

                if (json[index] == '0')
                {
                    index++;
                    if (index < json.Length && IsDigit(json[index]))
                    {
                        Fail("contains a JSON number with a leading zero");
                    }
                }
                else if (json[index] >= '1' && json[index] <= '9')
                {
                    while (index < json.Length && IsDigit(json[index]))
                    {
                        index++;
                    }
                }
                else
                {
                    Fail("contains a JSON number without an integer part");
                }

                if (index < json.Length && json[index] == '.')
                {
                    index++;
                    if (index >= json.Length || !IsDigit(json[index]))
                    {
                        Fail("contains a JSON number without fractional digits");
                    }

                    while (index < json.Length && IsDigit(json[index]))
                    {
                        index++;
                    }
                }

                if (index < json.Length &&
                    (json[index] == 'e' || json[index] == 'E'))
                {
                    index++;
                    if (index < json.Length &&
                        (json[index] == '+' || json[index] == '-'))
                    {
                        index++;
                    }

                    if (index >= json.Length || !IsDigit(json[index]))
                    {
                        Fail("contains a JSON number without exponent digits");
                    }

                    while (index < json.Length && IsDigit(json[index]))
                    {
                        index++;
                    }
                }

                string raw = json.Substring(start, index - start);
                if (!RelaxedG06DoubleMetricPaths.Contains(path))
                {
                    skeleton.Append(raw);
                    return;
                }

                if (!double.TryParse(
                        raw,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value) ||
                    double.IsNaN(value) ||
                    double.IsInfinity(value))
                {
                    FailAt(
                        start,
                        "contains a non-finite or unparseable double metric");
                }

                skeleton.Append(RelaxedNumberStart)
                    .Append(path)
                    .Append(RelaxedNumberEnd);
                relaxedNumbers.Add(new G06RelaxedJsonNumber(path, raw, value));
            }

            private void ParseLiteral(string literal)
            {
                if (index + literal.Length > json.Length ||
                    !string.Equals(
                        json.Substring(index, literal.Length),
                        literal,
                        StringComparison.Ordinal))
                {
                    Fail("contains an invalid JSON literal");
                }

                skeleton.Append(literal);
                index += literal.Length;
            }

            private void ParseTrivia()
            {
                int start = index;
                while (index < json.Length)
                {
                    char value = json[index];
                    if (value != ' ' &&
                        value != '\t' &&
                        value != '\r' &&
                        value != '\n')
                    {
                        break;
                    }

                    index++;
                }

                skeleton.Append(json, start, index - start);
            }

            private bool TryAppend(char expected)
            {
                if (index >= json.Length || json[index] != expected)
                {
                    return false;
                }

                skeleton.Append(expected);
                index++;
                return true;
            }

            private void AppendExpected(char expected)
            {
                if (!TryAppend(expected))
                {
                    Fail("expected JSON token '" + expected + "'");
                }
            }

            private void Fail(string detail)
            {
                FailAt(index, detail);
            }

            private void FailAt(int position, string detail)
            {
                throw new InvalidDataException(
                    label
                    + " "
                    + detail
                    + " at UTF-16 index "
                    + position.ToString(CultureInfo.InvariantCulture)
                    + ".");
            }

            private static bool IsDigit(char value)
            {
                return value >= '0' && value <= '9';
            }

            private static int HexDigitValue(char value)
            {
                if (value >= '0' && value <= '9')
                {
                    return value - '0';
                }

                if (value >= 'a' && value <= 'f')
                {
                    return value - 'a' + 10;
                }

                if (value >= 'A' && value <= 'F')
                {
                    return value - 'A' + 10;
                }

                return -1;
            }
        }

        private sealed class G06JsonLexicalDocument
        {
            public G06JsonLexicalDocument(
                string skeleton,
                G06RelaxedJsonNumber[] relaxedNumbers)
            {
                this.skeleton = skeleton;
                this.relaxedNumbers = relaxedNumbers;
            }

            public readonly string skeleton;
            public readonly G06RelaxedJsonNumber[] relaxedNumbers;
        }

        private sealed class G06RelaxedJsonNumber
        {
            public G06RelaxedJsonNumber(
                string path,
                string raw,
                double value)
            {
                this.path = path;
                this.raw = raw;
                this.value = value;
            }

            public readonly string path;
            public readonly string raw;
            public readonly double value;
        }

        private static void ValidateFiniteG06RuntimeProofNumbers(
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof runtime)
        {
            bool Invalid(float value) =>
                float.IsNaN(value) || float.IsInfinity(value);
            bool InvalidDouble(double value) =>
                double.IsNaN(value) || double.IsInfinity(value);
            if (runtime == null ||
                runtime.visualMetrics == null ||
                runtime.screenDelta == null ||
                runtime.counterDelta == null ||
                Invalid(runtime.bossRiskAtFirstFrame) ||
                runtime.bossRiskAtFirstFrame > 1f ||
                Invalid(runtime.bossRiskAtFireFrame) ||
                runtime.bossRiskAtFireFrame > 1f ||
                Invalid(runtime.bossRiskAtImpactFrame) ||
                runtime.bossRiskAtImpactFrame > 1f ||
                Invalid(runtime.hudEnergyMana) ||
                Invalid(runtime.hudEnergyMaxMana) ||
                Invalid(runtime.summonEnergyBeforeUse) ||
                Invalid(runtime.summonEnergyAfterUse) ||
                Invalid(runtime.authoredCounterDamage) ||
                Invalid(runtime.bossCounterDamageAmount) ||
                Invalid(runtime.bossCounterHealthDelta) ||
                Invalid(runtime.recorderCaptureDeltaTimeAtLogicalFrameZero) ||
                InvalidDouble(runtime.visualMetrics.blackRatio) ||
                InvalidDouble(runtime.visualMetrics.magentaRatio) ||
                InvalidDouble(runtime.visualMetrics.maximumFrameMagentaRatio) ||
                InvalidDouble(runtime.screenDelta.meanAbsoluteRgb) ||
                InvalidDouble(runtime.screenDelta.changedSampleRatio) ||
                InvalidDouble(runtime.counterDelta.meanAbsoluteRgb) ||
                InvalidDouble(runtime.counterDelta.changedSampleRatio))
            {
                throw new InvalidDataException(
                    "The G06 runtime proof contains a missing metric or a non-finite "
                    + "gameplay/render number.");
            }

            AuditionPvStationPhase2SummonCounterGoldenRunner.SequenceVisualMetrics
                visual = runtime.visualMetrics;
            const long exactVisualSampleCount = 1296000L;
            const long exactDeltaSampleCount = 115200L;
            if (visual.sampleCount != exactVisualSampleCount ||
                visual.blackSampleCount < 0 ||
                visual.blackSampleCount > visual.sampleCount ||
                visual.magentaSampleCount < 0 ||
                visual.magentaSampleCount > visual.sampleCount ||
                Math.Abs(visual.blackRatio -
                         visual.blackSampleCount / (double)visual.sampleCount) >
                0.000000000001d ||
                Math.Abs(visual.magentaRatio -
                         visual.magentaSampleCount / (double)visual.sampleCount) >
                0.000000000001d ||
                visual.maximumFrameMagentaRatio < 0d ||
                visual.healthyFrameCount < 0 ||
                visual.healthyFrameCount > 360 ||
                visual.magentaAffectedFrameCount < 0 ||
                visual.magentaAffectedFrameCount > 360 ||
                visual.minimumSampledLuma < 0 ||
                visual.minimumSampledLuma > 255 ||
                visual.maximumSampledLuma < 0 ||
                visual.maximumSampledLuma > 255 ||
                visual.frameZeroHudAccentSamples < 0 ||
                visual.frameZeroHudAccentSamples > 3600 ||
                runtime.screenDelta.sampleCount != exactDeltaSampleCount ||
                runtime.counterDelta.sampleCount != exactDeltaSampleCount ||
                !HasConsistentG06Delta(runtime.screenDelta) ||
                !HasConsistentG06Delta(runtime.counterDelta))
            {
                throw new InvalidDataException(
                    "The G06 runtime proof contains internally inconsistent "
                    + "visual or pixel-delta counts and ratios.");
            }
        }

        private static bool HasConsistentG06Delta(
            AuditionPvStationPhase2SummonCounterGoldenRunner.ScreenDeltaMetrics
                metrics)
        {
            return metrics.sampleCount > 0 &&
                   metrics.changedSampleCount >= 0 &&
                   metrics.changedSampleCount <= metrics.sampleCount &&
                   metrics.meanAbsoluteRgb >= 0d &&
                   metrics.meanAbsoluteRgb <= 255d &&
                   metrics.changedSampleRatio >= 0d &&
                   metrics.changedSampleRatio <= 1d &&
                   Math.Abs(metrics.changedSampleRatio -
                            metrics.changedSampleCount /
                            (double)metrics.sampleCount) <= 0.000000000001d;
        }

        private static string ResolveG06EvidencePath(
            string captureDirectory,
            string fileName)
        {
            return ResolveDirectChildPath(
                ResolveDirectChildPath(
                    captureDirectory,
                    G06EvidenceFolderName),
                fileName);
        }

        private static bool IsCanonicalG06RuntimeProofIdentity(
            AuditionPvTwelveSecondSourceManifestIdentity source)
        {
            try
            {
                if (!AuditionPvSha256.IsSha256(source.runtimeProofSha256) ||
                    string.IsNullOrWhiteSpace(source.runtimeProofPath) ||
                    !Path.IsPathRooted(source.runtimeProofPath) ||
                    string.IsNullOrWhiteSpace(source.manifestPath) ||
                    !Path.IsPathRooted(source.manifestPath) ||
                    !string.Equals(
                        Path.GetFileName(source.manifestPath),
                        AuditionPvCaptureContract.ManifestFileName,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                string captureDirectory = Path.GetDirectoryName(
                    source.manifestPath) ?? string.Empty;
                return string.Equals(
                           Path.GetFileName(captureDirectory),
                           source.captureId,
                           StringComparison.Ordinal) &&
                       PathsEqual(
                           source.runtimeProofPath,
                           ResolveG06EvidencePath(
                               captureDirectory,
                               G06RuntimeProofFileName));
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is NotSupportedException ||
                exception is PathTooLongException)
            {
                return false;
            }
        }

        private static string ResolveDirectChildPath(string parent, string name)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                ContainsTraversalSegment(name) ||
                !string.Equals(
                    Path.GetFileName(name),
                    name,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "A canonical G06 evidence path contains an unsafe file or "
                    + "directory name.");
            }

            string normalizedParent = Path.GetFullPath(parent);
            string path = Path.GetFullPath(Path.Combine(normalizedParent, name));
            if (!PathsEqual(
                    Path.GetDirectoryName(path) ?? string.Empty,
                    normalizedParent))
            {
                throw new InvalidDataException(
                    "A canonical G06 evidence path escaped its direct parent.");
            }

            return NormalizePath(path);
        }

        private static void ValidateNoG06FailureArtifacts(string captureDirectory)
        {
            string[] failures = Directory.GetFileSystemEntries(
                    captureDirectory,
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Where(path =>
                {
                    string name = Path.GetFileName(path);
                    return string.Equals(
                               name,
                               AuditionPvStationPhase2SummonCounterGoldenRunner
                                   .FailureFileName,
                               StringComparison.OrdinalIgnoreCase) ||
                           name.StartsWith(
                               "g06_capture_failure_",
                               StringComparison.OrdinalIgnoreCase) &&
                           name.EndsWith(
                               ".json",
                               StringComparison.OrdinalIgnoreCase);
                })
                .ToArray();
            if (failures.Length != 0)
            {
                throw new InvalidDataException(
                    "The G06 source contains a capture-failure artifact and cannot "
                    + "be used for the product Gate: "
                    + string.Join(", ", failures.Select(Path.GetFileName)));
            }
        }

        private static LoadedSource LoadAndValidateSource(
            string sourceRoot,
            string manifestPath,
            string expectedManifestSha256,
            string expectedDependencyIdentitySha256,
            AuditionPvGitSnapshot currentGit)
        {
            byte[] bytes = File.ReadAllBytes(manifestPath);
            string manifestSha256 = BytesSha256(bytes);
            if (!string.Equals(
                    manifestSha256,
                    expectedManifestSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Source capture manifest SHA-256 does not match its pinned "
                    + "segment specification: " + manifestPath);
            }

            AuditionPvCaptureManifest manifest =
                JsonUtility.FromJson<AuditionPvCaptureManifest>(
                    Encoding.UTF8.GetString(bytes));
            AuditionPvCaptureManifestWriter.Validate(manifest);

            string captureDirectory = Path.GetDirectoryName(manifestPath)
                ?? throw new InvalidDataException(
                    "Source capture manifest has no parent directory.");
            string expectedDirectory = ResolveDirectChild(
                sourceRoot,
                manifest.captureId,
                requireSimpleName: true);
            if (!PathsEqual(captureDirectory, expectedDirectory) ||
                !PathsEqual(manifest.outputRoot, sourceRoot) ||
                !PathsEqual(manifest.outputDirectory, captureDirectory))
            {
                throw new InvalidDataException(
                    "Source manifest path, capture ID, output root, and output "
                    + "directory do not describe the same direct-child capture.");
            }

            if (manifest.gitWorktreeDirty ||
                !string.Equals(
                    manifest.gitCommitSha,
                    currentGit.commitSha,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    manifest.gitBranch,
                    currentGit.branch,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    manifest.worktreeDirtyHashSha256,
                    currentGit.dirtyStateHashSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    manifest.worktreeDirtyHashAlgorithm,
                    AuditionPvGitSnapshot.DirtyHashAlgorithm,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Every source capture must be clean and exactly match the "
                    + "current HEAD, branch, and clean-state identity.");
            }

            if (manifest.testResults.Any(value =>
                    value == null ||
                    !string.Equals(
                        value.status,
                        "passed",
                        StringComparison.Ordinal)))
            {
                throw new InvalidDataException(
                    "Source capture manifest contains a non-passing test result.");
            }

            string dependencyIdentity = ComputeDependencyIdentity(manifest);
            if (!string.Equals(
                    dependencyIdentity,
                    expectedDependencyIdentitySha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Source dependency identity SHA-256 does not match its pinned "
                    + "segment specification: " + manifestPath);
            }

            return new LoadedSource
            {
                manifestPath = NormalizePath(manifestPath),
                captureDirectory = NormalizePath(captureDirectory),
                manifestSha256 = manifestSha256,
                dependencyIdentitySha256 = dependencyIdentity,
                manifest = manifest
            };
        }

        private static void RequirePinnedIdentity(
            AuditionPvTwelveSecondSegmentSpec segment,
            LoadedSource source)
        {
            if (!string.Equals(
                    segment.sourceManifestSha256,
                    source.manifestSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    segment.sourceDependencyIdentitySha256,
                    source.dependencyIdentitySha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Segments referencing the same source manifest must use the "
                    + "same pinned manifest and dependency identities.");
            }
        }

        private static string ComputeDependencyIdentity(
            AuditionPvCaptureManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            AuditionPvDependencyHash[] dependencies = manifest.dependencyHashes
                ?? Array.Empty<AuditionPvDependencyHash>();
            if (dependencies.Length == 0)
            {
                throw new InvalidDataException(
                    "Source manifest dependency identity cannot be empty.");
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var material = new StringBuilder();
            foreach (AuditionPvDependencyHash dependency in dependencies
                         .OrderBy(value => value?.path, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(value => value?.path, StringComparer.Ordinal))
            {
                if (dependency == null ||
                    string.IsNullOrWhiteSpace(dependency.path) ||
                    !seen.Add(NormalizePath(dependency.path)) ||
                    !dependency.exists ||
                    dependency.byteLength < 0 ||
                    !AuditionPvSha256.IsSha256(dependency.sha256))
                {
                    throw new InvalidDataException(
                        "Source manifest contains an invalid, missing, or duplicate "
                        + "dependency identity entry.");
                }

                material.Append(NormalizePath(dependency.path))
                    .Append('\0')
                    .Append('1')
                    .Append('\0')
                    .Append(dependency.byteLength.ToString(
                        CultureInfo.InvariantCulture))
                    .Append('\0')
                    .Append(dependency.sha256)
                    .Append('\0');
            }

            return AuditionPvSha256.TextHash(material.ToString());
        }

        private static void ValidateSharedDependencyEntries(
            IEnumerable<LoadedSource> sources)
        {
            var identities = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            LoadedSource[] sourceArray = sources.ToArray();
            if (sourceArray.Length == 0)
            {
                throw new InvalidDataException(
                    "The 12-second select has no source captures.");
            }

            LoadedSource first = sourceArray[0];
            foreach (LoadedSource source in sourceArray)
            {
                AuditionPvCaptureManifest manifest = source.manifest;
                if (!string.Equals(
                        manifest.gitCommitSha,
                        first.manifest.gitCommitSha,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        manifest.gitBranch,
                        first.manifest.gitBranch,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        manifest.worktreeDirtyHashSha256,
                        first.manifest.worktreeDirtyHashSha256,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        manifest.unityVersionWithRevision,
                        first.manifest.unityVersionWithRevision,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        manifest.recorderPackageVersion,
                        first.manifest.recorderPackageVersion,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        manifest.urpPackageVersion,
                        first.manifest.urpPackageVersion,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        manifest.activeRenderPipelineAssetPath,
                        first.manifest.activeRenderPipelineAssetPath,
                        StringComparison.Ordinal) ||
                    manifest.width != first.manifest.width ||
                    manifest.height != first.manifest.height ||
                    manifest.fps != first.manifest.fps ||
                    !string.Equals(
                        manifest.sourceFormat,
                        first.manifest.sourceFormat,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Source captures do not share one clean Git/engine/QHD60 "
                        + "capture identity.");
                }

                foreach (AuditionPvDependencyHash dependency in
                         manifest.dependencyHashes)
                {
                    string path = NormalizePath(dependency.path);
                    string identity = dependency.byteLength.ToString(
                                          CultureInfo.InvariantCulture)
                                      + "\0" + dependency.sha256;
                    if (identities.TryGetValue(path, out string existing) &&
                        !string.Equals(existing, identity, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            "Source manifests disagree on shared dependency '"
                            + path + "'.");
                    }

                    identities[path] = identity;
                }
            }
        }

        private static void ValidateCompleteSourceShot(
            LoadedSource source,
            AuditionPvShotManifestEntry shot)
        {
            string frameDirectory = ResolveSourceFrameDirectory(
                source.captureDirectory,
                shot.id);
            if (!Directory.Exists(frameDirectory))
            {
                throw new DirectoryNotFoundException(
                    "Source shot frame directory is missing: " + frameDirectory);
            }

            RejectReparsePoint(frameDirectory, "source shot frame directory");

            string[] actual = Directory.GetFiles(
                    frameDirectory,
                    "*.png",
                    SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] expected = Enumerable.Range(
                    shot.startFrame,
                    shot.expectedFrameCount)
                .Select(FrameFileName)
                .ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    $"Source shot '{shot.id}' does not contain its exact contiguous "
                    + $"{shot.expectedFrameCount}-frame PNG sequence.");
            }

            for (int frame = shot.startFrame; frame <= shot.endFrame; frame++)
            {
                ValidatePngHeader(
                    Path.Combine(frameDirectory, FrameFileName(frame)),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }
        }

        private static void ValidateSourceBaselines(
            LoadedSource source,
            IEnumerable<ValidatedSegment> sourceSegments)
        {
            var referencedShots = new HashSet<string>(
                sourceSegments.Select(value => value.shot.id),
                StringComparer.Ordinal);
            foreach (AuditionPvBaselineManifestEntry baseline in
                     source.manifest.baselines.Where(value =>
                         referencedShots.Contains(value.shotId)))
            {
                if (baseline == null ||
                    !string.Equals(
                        baseline.status,
                        "captured",
                        StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(baseline.fileName) ||
                    !string.Equals(
                        Path.GetFileName(baseline.fileName),
                        baseline.fileName,
                        StringComparison.Ordinal) ||
                    ContainsTraversalSegment(baseline.fileName))
                {
                    throw new InvalidDataException(
                        "Source baseline entry is not a safe captured baseline.");
                }

                AuditionPvShotManifestEntry shot = source.manifest.shots.Single(
                    value => string.Equals(
                        value.id,
                        baseline.shotId,
                        StringComparison.Ordinal));
                if (!string.Equals(
                        baseline.hudMode,
                        shot.hudMode,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Source baseline '{baseline.id}' HUD mode differs from "
                        + $"shot '{shot.id}'.");
                }

                string baselinePath = Path.Combine(
                    source.captureDirectory,
                    AuditionPvStationTransitionGoldenCapture.BaselinesFolderName,
                    baseline.fileName);
                string sourceFramePath = ResolveSourceFramePath(
                    source.captureDirectory,
                    shot.id,
                    baseline.sourceFrame);
                ValidatePngHeader(
                    baselinePath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                if (!string.Equals(
                        FileSha256(baselinePath),
                        FileSha256(sourceFramePath),
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Source baseline '{baseline.id}' is not a byte-exact copy "
                        + "of its declared source frame.");
                }
            }
        }

        private static AuditionPvTwelveSecondSelectManifest MaterializeStaging(
            ValidatedPlan plan,
            string stagingDirectory,
            string finalDirectory,
            string sourceRoot,
            string outputRoot,
            string outputId,
            AuditionPvGitSnapshot git,
            DateTime createdAtUtc,
            AuditionPvTwelveSecondProxyToolSpec proxyTools,
            IAuditionPvTwelveSecondProxyEncoder proxyEncoder)
        {
            string framesDirectory = Path.Combine(
                stagingDirectory,
                FramesFolderName);
            Directory.CreateDirectory(framesDirectory);
            var mappings = new List<AuditionPvTwelveSecondFrameMapping>(
                ExpectedFrameCount);
            var segmentEntries = new List<AuditionPvTwelveSecondSelectSegment>(
                plan.segments.Length);

            foreach (ValidatedSegment segment in plan.segments)
            {
                segmentEntries.Add(new AuditionPvTwelveSecondSelectSegment
                {
                    role = segment.specification.role,
                    order = segment.specification.order,
                    hudMode = RequiredHudModes[segment.specification.order],
                    sourceCaptureId = segment.source.manifest.captureId,
                    sourceShotId = segment.shot.id,
                    sourceStartFrame = segment.specification.startFrame,
                    sourceEndFrame = segment.specification.endFrame,
                    selectStartFrame = segment.outputStartFrame,
                    selectEndFrame = segment.outputEndFrame,
                    frameCount = segment.outputEndFrame
                                 - segment.outputStartFrame + 1,
                    sourceRuntimeProofSha256 =
                        segment.specification.sourceRuntimeProofSha256
                });

                for (int sourceFrame = segment.specification.startFrame;
                    sourceFrame <= segment.specification.endFrame;
                    sourceFrame++)
                {
                    int selectFrame = segment.outputStartFrame
                                      + sourceFrame
                                      - segment.specification.startFrame;
                    string sourcePath = ResolveSourceFramePath(
                        segment.source.captureDirectory,
                        segment.shot.id,
                        sourceFrame);
                    string selectRelativePath = NormalizePath(Path.Combine(
                        FramesFolderName,
                        FrameFileName(selectFrame)));
                    string destinationPath = Path.Combine(
                        stagingDirectory,
                        selectRelativePath);
                    string hash = CopyPngNewAndHash(
                        sourcePath,
                        destinationPath,
                        AuditionPvCaptureContract.Width,
                        AuditionPvCaptureContract.Height);
                    string pinnedSourceHash =
                        segment.specification.sourceFrameSha256[
                            sourceFrame - segment.specification.startFrame];
                    if (!string.Equals(
                            hash,
                            pinnedSourceHash,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            $"Source frame changed after preflight SHA-256 "
                            + $"validation for segment "
                            + $"'{segment.specification.role}', source frame "
                            + $"{sourceFrame}.");
                    }
                    mappings.Add(new AuditionPvTwelveSecondFrameMapping
                    {
                        selectFrame = selectFrame,
                        role = segment.specification.role,
                        segmentOrder = segment.specification.order,
                        sourceCaptureId = segment.source.manifest.captureId,
                        sourceManifestSha256 =
                            segment.source.manifestSha256,
                        sourceDependencyIdentitySha256 =
                            segment.source.dependencyIdentitySha256,
                        sourceShotId = segment.shot.id,
                        sourceFrame = sourceFrame,
                        sourceRelativePath = NormalizePath(Path.GetRelativePath(
                            segment.source.captureDirectory,
                            sourcePath)),
                        selectRelativePath = selectRelativePath,
                        sha256 = hash
                    });
                }
            }

            if (mappings.Count != ExpectedFrameCount ||
                mappings.Where((value, index) => value.selectFrame != index).Any())
            {
                throw new InvalidDataException(
                    "Materialized select frame mapping is not contiguous 0..719.");
            }

            AuditionPvTwelveSecondContactSheetArtifact contactSheet =
                CreateContactSheet(stagingDirectory, mappings);

            string ledgerPath = Path.Combine(
                stagingDirectory,
                FrameHashFileName);
            WriteCanonicalFrameHashLedger(ledgerPath, mappings);
            string ledgerSha256 = FileSha256(ledgerPath);
            AuditionPvTwelveSecondProxyArtifact proxy = proxyEncoder.Encode(
                stagingDirectory,
                proxyTools);
            ValidateProxyProvenance(proxy, proxyTools);
            ValidateProxyArtifact(stagingDirectory, proxy);
            AuditionPvTwelveSecondBaselineReference[] baselines =
                CreateBaselineReferences(plan, mappings);
            var manifest = new AuditionPvTwelveSecondSelectManifest
            {
                schemaVersion = ManifestSchema,
                outputId = outputId,
                createdAtUtc = createdAtUtc.ToString("O"),
                sourceRoot = NormalizePath(sourceRoot),
                outputRoot = NormalizePath(outputRoot),
                outputDirectory = NormalizePath(finalDirectory),
                sourceFormat = AuditionPvCaptureContract.SourceFormat,
                width = AuditionPvCaptureContract.Width,
                height = AuditionPvCaptureContract.Height,
                fps = AuditionPvCaptureContract.Fps,
                totalFrames = ExpectedFrameCount,
                durationSeconds = ExpectedFrameCount
                                  / (double)AuditionPvCaptureContract.Fps,
                gitCommitSha = git.commitSha,
                gitBranch = git.branch,
                worktreeDirtyHashSha256 = git.dirtyStateHashSha256,
                frameHashLedgerFile = FrameHashFileName,
                frameHashLedgerSha256 = ledgerSha256,
                contactSheet = contactSheet,
                proxy = proxy,
                sourceManifests = plan.sources.Select(source =>
                    new AuditionPvTwelveSecondSourceManifestIdentity
                    {
                        captureId = source.manifest.captureId,
                        manifestPath = source.manifestPath,
                        manifestSha256 = source.manifestSha256,
                        dependencyIdentitySha256 =
                            source.dependencyIdentitySha256,
                        dependencyCount =
                            source.manifest.dependencyHashes.Length,
                        gitCommitSha = source.manifest.gitCommitSha,
                        gitBranch = source.manifest.gitBranch,
                        worktreeDirtyHashSha256 =
                            source.manifest.worktreeDirtyHashSha256,
                        unityVersionWithRevision =
                            source.manifest.unityVersionWithRevision,
                        recorderPackageVersion =
                            source.manifest.recorderPackageVersion,
                        urpPackageVersion = source.manifest.urpPackageVersion,
                        activeRenderPipelineAssetPath =
                            source.manifest.activeRenderPipelineAssetPath,
                        runtimeProofPath = source.runtimeProofPath,
                        runtimeProofSha256 = source.runtimeProofSha256
                    }).ToArray(),
                segments = segmentEntries.ToArray(),
                frames = mappings.ToArray(),
                baselineReferences = baselines
            };

            WriteJsonNew(
                Path.Combine(stagingDirectory, ManifestFileName),
                manifest);
            return manifest;
        }

        private static AuditionPvTwelveSecondContactSheetArtifact
            CreateContactSheet(
                string stagingDirectory,
                IReadOnlyList<AuditionPvTwelveSecondFrameMapping> mappings)
        {
            if (mappings == null || mappings.Count != ExpectedFrameCount)
            {
                throw new InvalidDataException(
                    "Contact-sheet input mapping must contain exactly 720 frames.");
            }

            var outputPixels = new Color32[ContactSheetWidth * ContactSheetHeight];
            var cells = new List<AuditionPvTwelveSecondContactSheetCell>(
                ContactSheetOutputFrames.Length);
            for (int cellIndex = 0;
                 cellIndex < ContactSheetOutputFrames.Length;
                 cellIndex++)
            {
                int outputFrame = ContactSheetOutputFrames[cellIndex];
                AuditionPvTwelveSecondFrameMapping mapping = mappings[outputFrame]
                    ?? throw new InvalidDataException(
                        "Contact-sheet input mapping is null at output frame "
                        + outputFrame + ".");
                string inputPath = Path.Combine(
                    stagingDirectory,
                    mapping.selectRelativePath);
                BoxDownsampleContactCell(
                    inputPath,
                    cellIndex,
                    mapping.sha256,
                    outputPixels);
                cells.Add(new AuditionPvTwelveSecondContactSheetCell
                {
                    cellIndex = cellIndex,
                    row = cellIndex / ContactSheetColumns,
                    column = cellIndex % ContactSheetColumns,
                    outputFrame = outputFrame,
                    segmentOrder = mapping.segmentOrder,
                    role = mapping.role,
                    sourceCaptureId = mapping.sourceCaptureId,
                    sourceShotId = mapping.sourceShotId,
                    sourceFrame = mapping.sourceFrame,
                    sourceSha256 = mapping.sha256
                });
            }

            byte[] pngBytes;
            var contactTexture = new Texture2D(
                ContactSheetWidth,
                ContactSheetHeight,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: true);
            try
            {
                contactTexture.SetPixels32(outputPixels);
                contactTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                pngBytes = contactTexture.EncodeToPNG();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(contactTexture);
            }

            if (pngBytes == null || pngBytes.Length == 0)
            {
                throw new InvalidDataException(
                    "Unity failed to encode the deterministic contact-sheet PNG.");
            }

            string path = Path.Combine(stagingDirectory, ContactSheetFileName);
            WriteBytesNew(path, pngBytes);
            ValidatePngHeader(path, ContactSheetWidth, ContactSheetHeight);
            return new AuditionPvTwelveSecondContactSheetArtifact
            {
                file = ContactSheetFileName,
                sha256 = FileSha256(path),
                byteLength = new FileInfo(path).Length,
                width = ContactSheetWidth,
                height = ContactSheetHeight,
                cellWidth = ContactSheetCellWidth,
                cellHeight = ContactSheetCellHeight,
                columns = ContactSheetColumns,
                rows = ContactSheetRows,
                downsamplePolicy = ContactSheetDownsamplePolicy,
                cells = cells.ToArray()
            };
        }

        private static void BoxDownsampleContactCell(
            string inputPath,
            int cellIndex,
            string expectedInputSha256,
            Color32[] outputPixels)
        {
            RejectReparsePoint(inputPath, "contact-sheet input frame");
            byte[] encoded = File.ReadAllBytes(inputPath);
            if (!string.Equals(
                    BytesSha256(encoded),
                    expectedInputSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Contact-sheet input bytes differ from their sealed source "
                    + "mapping SHA-256: " + inputPath);
            }

            using (var headerStream = new MemoryStream(
                       encoded,
                       writable: false))
            {
                ValidatePngHeader(
                    headerStream,
                    inputPath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }

            var sourceTexture = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: true);
            try
            {
                if (!sourceTexture.LoadImage(encoded, markNonReadable: false) ||
                    sourceTexture.width != AuditionPvCaptureContract.Width ||
                    sourceTexture.height != AuditionPvCaptureContract.Height)
                {
                    throw new InvalidDataException(
                        "Contact-sheet source is not a decodable exact QHD PNG: "
                        + inputPath);
                }

                Color32[] sourcePixels = sourceTexture.GetPixels32();
                if (sourcePixels.Length !=
                    AuditionPvCaptureContract.Width
                    * AuditionPvCaptureContract.Height)
                {
                    throw new InvalidDataException(
                        "Decoded contact-sheet source has an unexpected RGBA8 "
                        + "pixel count: " + inputPath);
                }

                int column = cellIndex % ContactSheetColumns;
                int rowFromTop = cellIndex / ContactSheetColumns;
                int destinationBottom =
                    (ContactSheetRows - 1 - rowFromTop)
                    * ContactSheetCellHeight;
                for (int y = 0; y < ContactSheetCellHeight; y++)
                {
                    int sourceY = y * 4;
                    int destinationY = destinationBottom + y;
                    int destinationRowOffset =
                        destinationY * ContactSheetWidth
                        + column * ContactSheetCellWidth;
                    for (int x = 0; x < ContactSheetCellWidth; x++)
                    {
                        int sourceX = x * 4;
                        int red = 0;
                        int green = 0;
                        int blue = 0;
                        int alpha = 0;
                        for (int boxY = 0; boxY < 4; boxY++)
                        {
                            int sourceOffset =
                                (sourceY + boxY)
                                * AuditionPvCaptureContract.Width
                                + sourceX;
                            for (int boxX = 0; boxX < 4; boxX++)
                            {
                                Color32 pixel = sourcePixels[sourceOffset + boxX];
                                red += pixel.r;
                                green += pixel.g;
                                blue += pixel.b;
                                alpha += pixel.a;
                            }
                        }

                        outputPixels[destinationRowOffset + x] = new Color32(
                            (byte)((red + 8) / 16),
                            (byte)((green + 8) / 16),
                            (byte)((blue + 8) / 16),
                            (byte)((alpha + 8) / 16));
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sourceTexture);
            }
        }

        private static AuditionPvTwelveSecondBaselineReference[]
            CreateBaselineReferences(
                ValidatedPlan plan,
                IReadOnlyList<AuditionPvTwelveSecondFrameMapping> mappings)
        {
            var references = new List<AuditionPvTwelveSecondBaselineReference>();
            foreach (LoadedSource source in plan.sources)
            {
                ValidatedSegment[] sourceSegments = plan.segments
                    .Where(value => ReferenceEquals(value.source, source))
                    .ToArray();
                var referencedShots = new HashSet<string>(
                    sourceSegments.Select(value => value.shot.id),
                    StringComparer.Ordinal);
                foreach (AuditionPvBaselineManifestEntry baseline in
                         source.manifest.baselines
                             .Where(value => referencedShots.Contains(value.shotId))
                             .OrderBy(value => value.id, StringComparer.Ordinal))
                {
                    AuditionPvTwelveSecondFrameMapping[] matches = mappings
                        .Where(value =>
                            string.Equals(
                                value.sourceCaptureId,
                                source.manifest.captureId,
                                StringComparison.Ordinal) &&
                            string.Equals(
                                value.sourceShotId,
                                baseline.shotId,
                                StringComparison.Ordinal) &&
                            value.sourceFrame == baseline.sourceFrame)
                        .ToArray();
                    if (matches.Length > 1)
                    {
                        throw new InvalidDataException(
                            $"Baseline '{baseline.id}' maps to more than one select "
                            + "frame; overlapping source segments are not allowed "
                            + "for baseline keys.");
                    }

                    AuditionPvTwelveSecondFrameMapping match =
                        matches.SingleOrDefault();
                    references.Add(new AuditionPvTwelveSecondBaselineReference
                    {
                        sourceCaptureId = source.manifest.captureId,
                        sourceBaselineId = baseline.id,
                        sourceShotId = baseline.shotId,
                        sourceFrame = baseline.sourceFrame,
                        sourceBaselineFileName = baseline.fileName,
                        hudMode = baseline.hudMode,
                        includedInSelect = match != null,
                        selectFrame = match?.selectFrame ?? -1,
                        selectRelativePath = match?.selectRelativePath
                                             ?? string.Empty,
                        sha256 = match?.sha256 ?? string.Empty
                    });
                }
            }

            return references.ToArray();
        }

        private static void ValidateMaterializedPackage(
            string physicalDirectory,
            string logicalFinalDirectory,
            AuditionPvTwelveSecondSelectManifest manifest)
        {
            if (manifest == null ||
                !string.Equals(
                    manifest.schemaVersion,
                    ManifestSchema,
                    StringComparison.Ordinal) ||
                !PathsEqual(manifest.outputDirectory, logicalFinalDirectory) ||
                manifest.width != AuditionPvCaptureContract.Width ||
                manifest.height != AuditionPvCaptureContract.Height ||
                manifest.fps != AuditionPvCaptureContract.Fps ||
                manifest.totalFrames != ExpectedFrameCount ||
                Math.Abs(manifest.durationSeconds - 12d) > 0.000001d ||
                !string.Equals(
                    manifest.sourceFormat,
                    AuditionPvCaptureContract.SourceFormat,
                    StringComparison.Ordinal) ||
                manifest.segments == null ||
                manifest.frames == null ||
                manifest.sourceManifests == null ||
                manifest.baselineReferences == null ||
                manifest.contactSheet == null ||
                manifest.proxy == null ||
                !string.Equals(
                    manifest.frameHashLedgerFile,
                    FrameHashFileName,
                    StringComparison.Ordinal) ||
                !AuditionPvSha256.IsSha256(
                    manifest.frameHashLedgerSha256))
            {
                throw new InvalidDataException(
                    "12-second select manifest header or required arrays are invalid.");
            }

            if (manifest.segments.Length != RequiredRoles.Length ||
                manifest.frames.Length != ExpectedFrameCount)
            {
                throw new InvalidDataException(
                    "12-second select manifest segment or frame count is invalid.");
            }

            int expectedStart = 0;
            for (int index = 0; index < manifest.segments.Length; index++)
            {
                AuditionPvTwelveSecondSelectSegment segment =
                    manifest.segments[index];
                if (segment != null)
                {
                    ValidateSimpleId(
                        segment.sourceCaptureId,
                        "select segment source capture ID");
                    ValidateSimpleId(
                        segment.sourceShotId,
                        "select segment source shot ID");
                }

                if (segment == null ||
                    segment.order != index ||
                    !string.Equals(
                        segment.role,
                        RequiredRoles[index],
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        segment.hudMode,
                        RequiredHudModes[index],
                        StringComparison.Ordinal) ||
                    segment.frameCount <= 0 ||
                    segment.sourceStartFrame < 0 ||
                    segment.sourceEndFrame - segment.sourceStartFrame + 1
                    != segment.frameCount ||
                    segment.selectStartFrame != expectedStart ||
                    segment.selectEndFrame - segment.selectStartFrame + 1
                    != segment.frameCount ||
                    (index == manifest.segments.Length - 1) !=
                    AuditionPvSha256.IsSha256(
                        segment.sourceRuntimeProofSha256) ||
                    index != manifest.segments.Length - 1 &&
                    !string.IsNullOrEmpty(
                        segment.sourceRuntimeProofSha256))
                {
                    throw new InvalidDataException(
                        "12-second select segment topology is invalid at index "
                        + index + ".");
                }

                expectedStart = segment.selectEndFrame + 1;
            }

            if (expectedStart != ExpectedFrameCount ||
                !string.Equals(
                    manifest.segments[^1].sourceShotId,
                    CounterShotId,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "12-second select does not terminate at frame 719 with G06.");
            }

            var sourceIdentities = new Dictionary<
                string,
                AuditionPvTwelveSecondSourceManifestIdentity>(
                StringComparer.Ordinal);
            int runtimeProofIdentityCount = 0;
            foreach (AuditionPvTwelveSecondSourceManifestIdentity source in
                     manifest.sourceManifests)
            {
                if (source == null)
                {
                    throw new InvalidDataException(
                        "Select source-manifest identity table contains a null entry.");
                }

                bool hasRuntimeProof =
                    !string.IsNullOrEmpty(source.runtimeProofPath) ||
                    !string.IsNullOrEmpty(source.runtimeProofSha256);
                if (string.IsNullOrWhiteSpace(source.captureId) ||
                    !sourceIdentities.TryAdd(source.captureId, source) ||
                    !AuditionPvSha256.IsSha256(source.manifestSha256) ||
                    !AuditionPvSha256.IsSha256(
                        source.dependencyIdentitySha256) ||
                    !string.Equals(
                        source.gitCommitSha,
                        manifest.gitCommitSha,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        source.gitBranch,
                        manifest.gitBranch,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        source.worktreeDirtyHashSha256,
                        manifest.worktreeDirtyHashSha256,
                        StringComparison.Ordinal) ||
                    hasRuntimeProof &&
                    !IsCanonicalG06RuntimeProofIdentity(source))
                {
                    throw new InvalidDataException(
                        "Select source-manifest identity table is invalid.");
                }

                if (hasRuntimeProof)
                {
                    runtimeProofIdentityCount++;
                }
            }

            AuditionPvTwelveSecondSelectSegment counterSegment =
                manifest.segments[^1];
            if (runtimeProofIdentityCount != 1 ||
                !sourceIdentities.TryGetValue(
                    counterSegment.sourceCaptureId,
                    out AuditionPvTwelveSecondSourceManifestIdentity counterSource) ||
                !string.Equals(
                    counterSegment.sourceRuntimeProofSha256,
                    counterSource.runtimeProofSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The installed select must bind its final G06 segment to exactly "
                    + "one canonical source runtime-proof identity.");
            }

            string framesDirectory = Path.Combine(
                physicalDirectory,
                FramesFolderName);
            if (!Directory.Exists(framesDirectory))
            {
                throw new DirectoryNotFoundException(
                    "Materialized select frames directory is missing: "
                    + framesDirectory);
            }

            RejectReparsePoint(
                framesDirectory,
                "materialized select frames directory");
            string[] actualNames = Directory.GetFiles(
                    framesDirectory,
                    "*.png",
                    SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] expectedNames = Enumerable.Range(0, ExpectedFrameCount)
                .Select(FrameFileName)
                .ToArray();
            if (!actualNames.SequenceEqual(expectedNames, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "Materialized select is not an exact frame_0000..0719 sequence.");
            }

            for (int index = 0; index < manifest.frames.Length; index++)
            {
                AuditionPvTwelveSecondFrameMapping mapping =
                    manifest.frames[index];
                AuditionPvTwelveSecondSelectSegment segment = manifest.segments
                    .Single(value =>
                        index >= value.selectStartFrame &&
                        index <= value.selectEndFrame);
                string expectedRelative = NormalizePath(Path.Combine(
                    FramesFolderName,
                    FrameFileName(index)));
                string expectedSourceRelative = NormalizePath(Path.Combine(
                    string.Equals(
                        segment.sourceShotId,
                        AuditionPvStationTransitionGoldenCapture.ShotId,
                        StringComparison.Ordinal)
                        ? AuditionPvStationTransitionGoldenCapture.FramesFolderName
                        : NormalizePath(Path.Combine(
                            FramesFolderName,
                            segment.sourceShotId)),
                    FrameFileName(
                        segment.sourceStartFrame
                        + index
                        - segment.selectStartFrame)));
                if (mapping == null ||
                    mapping.selectFrame != index ||
                    !string.Equals(
                        mapping.selectRelativePath,
                        expectedRelative,
                        StringComparison.Ordinal) ||
                    !AuditionPvSha256.IsSha256(mapping.sha256) ||
                    !AuditionPvSha256.IsSha256(
                        mapping.sourceManifestSha256) ||
                    !AuditionPvSha256.IsSha256(
                        mapping.sourceDependencyIdentitySha256) ||
                    mapping.segmentOrder != segment.order ||
                    !string.Equals(
                        mapping.role,
                        segment.role,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        mapping.sourceCaptureId,
                        segment.sourceCaptureId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        mapping.sourceShotId,
                        segment.sourceShotId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        mapping.sourceRelativePath,
                        expectedSourceRelative,
                        StringComparison.Ordinal) ||
                    mapping.sourceFrame != segment.sourceStartFrame
                                           + index
                                           - segment.selectStartFrame ||
                    !sourceIdentities.TryGetValue(
                        mapping.sourceCaptureId,
                        out AuditionPvTwelveSecondSourceManifestIdentity source) ||
                    !string.Equals(
                        mapping.sourceManifestSha256,
                        source.manifestSha256,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        mapping.sourceDependencyIdentitySha256,
                        source.dependencyIdentitySha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Select frame mapping is invalid at frame " + index + ".");
                }

                string framePath = Path.Combine(
                    physicalDirectory,
                    mapping.selectRelativePath);
                ValidatePngHeader(
                    framePath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                string actualHash = FileSha256(framePath);
                if (!string.Equals(
                        actualHash,
                        mapping.sha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Select frame SHA-256 mismatch at frame " + index + ".");
                }
            }

            ValidateContactSheetArtifact(
                physicalDirectory,
                manifest.contactSheet,
                manifest.frames);

            string ledgerPath = Path.Combine(
                physicalDirectory,
                manifest.frameHashLedgerFile);
            ValidateCanonicalFrameHashLedger(ledgerPath, manifest.frames);
            if (!string.Equals(
                    FileSha256(ledgerPath),
                    manifest.frameHashLedgerSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Select frame hash ledger SHA-256 does not match the manifest.");
            }

            ValidateProxyArtifact(physicalDirectory, manifest.proxy);

            foreach (AuditionPvTwelveSecondBaselineReference baseline in
                     manifest.baselineReferences.Where(value =>
                         value.includedInSelect))
            {
                AuditionPvTwelveSecondFrameMapping mapping = manifest.frames
                    .SingleOrDefault(value =>
                        value.selectFrame == baseline.selectFrame);
                if (mapping == null ||
                    !string.Equals(
                        mapping.selectRelativePath,
                        baseline.selectRelativePath,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        mapping.sha256,
                        baseline.sha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Included baseline key does not reference its exact select "
                        + "frame and SHA-256.");
                }
            }
        }

        private static void ValidateContactSheetArtifact(
            string physicalDirectory,
            AuditionPvTwelveSecondContactSheetArtifact contactSheet,
            IReadOnlyList<AuditionPvTwelveSecondFrameMapping> mappings)
        {
            if (contactSheet == null ||
                !string.Equals(
                    contactSheet.file,
                    ContactSheetFileName,
                    StringComparison.Ordinal) ||
                !AuditionPvSha256.IsSha256(contactSheet.sha256) ||
                contactSheet.byteLength <= 0 ||
                contactSheet.width != ContactSheetWidth ||
                contactSheet.height != ContactSheetHeight ||
                contactSheet.cellWidth != ContactSheetCellWidth ||
                contactSheet.cellHeight != ContactSheetCellHeight ||
                contactSheet.columns != ContactSheetColumns ||
                contactSheet.rows != ContactSheetRows ||
                !string.Equals(
                    contactSheet.downsamplePolicy,
                    ContactSheetDownsamplePolicy,
                    StringComparison.Ordinal) ||
                contactSheet.cells == null ||
                contactSheet.cells.Length != ContactSheetOutputFrames.Length ||
                mappings == null ||
                mappings.Count != ExpectedFrameCount)
            {
                throw new InvalidDataException(
                    "25%-scale contact-sheet artifact contract is invalid.");
            }

            string path = Path.Combine(physicalDirectory, contactSheet.file);
            if (!File.Exists(path) || Directory.Exists(path))
            {
                throw new FileNotFoundException(
                    "25%-scale contact-sheet PNG is missing.",
                    path);
            }

            ValidatePngHeader(path, ContactSheetWidth, ContactSheetHeight);
            if (new FileInfo(path).Length != contactSheet.byteLength ||
                !string.Equals(
                    FileSha256(path),
                    contactSheet.sha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "25%-scale contact-sheet byte length or SHA-256 mismatch.");
            }

            for (int index = 0;
                 index < ContactSheetOutputFrames.Length;
                 index++)
            {
                int outputFrame = ContactSheetOutputFrames[index];
                AuditionPvTwelveSecondContactSheetCell cell =
                    contactSheet.cells[index];
                AuditionPvTwelveSecondFrameMapping mapping =
                    mappings[outputFrame];
                if (cell == null || mapping == null ||
                    cell.cellIndex != index ||
                    cell.row != index / ContactSheetColumns ||
                    cell.column != index % ContactSheetColumns ||
                    cell.outputFrame != outputFrame ||
                    cell.segmentOrder != mapping.segmentOrder ||
                    !string.Equals(
                        cell.role,
                        mapping.role,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        cell.sourceCaptureId,
                        mapping.sourceCaptureId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        cell.sourceShotId,
                        mapping.sourceShotId,
                        StringComparison.Ordinal) ||
                    cell.sourceFrame != mapping.sourceFrame ||
                    !string.Equals(
                        cell.sourceSha256,
                        mapping.sha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "25%-scale contact-sheet cell mapping is invalid at cell "
                        + index + ".");
                }
            }
        }

        private static void WriteValidationReport(
            string stagingDirectory,
            string finalDirectory,
            AuditionPvTwelveSecondSelectManifest manifest,
            string manifestSha256,
            DateTime validatedAtUtc)
        {
            AuditionPvTwelveSecondSelectSegment counterSegment =
                manifest.segments[^1];
            AuditionPvTwelveSecondSourceManifestIdentity counterSource =
                manifest.sourceManifests.Single(source => string.Equals(
                    source.captureId,
                    counterSegment.sourceCaptureId,
                    StringComparison.Ordinal));
            var report = new AuditionPvTwelveSecondValidationReport
            {
                schemaVersion = ValidationSchema,
                validatedAtUtc = validatedAtUtc.ToString("O"),
                passed = true,
                outputId = manifest.outputId,
                outputDirectory = NormalizePath(finalDirectory),
                manifestFile = ManifestFileName,
                manifestSha256 = manifestSha256,
                frameHashLedgerFile = FrameHashFileName,
                frameHashLedgerSha256 = manifest.frameHashLedgerSha256,
                contactSheetFile = manifest.contactSheet.file,
                contactSheetSha256 = manifest.contactSheet.sha256,
                contactSheetByteLength = manifest.contactSheet.byteLength,
                proxyFile = manifest.proxy.proxyFile,
                proxySha256 = manifest.proxy.proxySha256,
                proxyProbeFile = manifest.proxy.probeFile,
                proxyProbeSha256 = manifest.proxy.probeSha256,
                g06SourceCaptureId = counterSource.captureId,
                g06RuntimeProofPath = counterSource.runtimeProofPath,
                g06RuntimeProofSha256 = counterSource.runtimeProofSha256,
                sourceManifestCount = manifest.sourceManifests.Length,
                segmentCount = manifest.segments.Length,
                frameCount = manifest.frames.Length,
                checks = RequiredValidationChecks.ToArray()
            };
            WriteJsonNew(
                Path.Combine(stagingDirectory, ValidationReportFileName),
                report);
        }

        private static void ValidateValidationReport(
            string physicalDirectory,
            string logicalFinalDirectory,
            AuditionPvTwelveSecondSelectManifest manifest,
            string manifestSha256)
        {
            AuditionPvTwelveSecondSelectSegment counterSegment =
                manifest != null && manifest.segments != null &&
                manifest.segments.Length > 0
                    ? manifest.segments[manifest.segments.Length - 1]
                    : null;
            AuditionPvTwelveSecondSourceManifestIdentity counterSource =
                manifest?.sourceManifests?.SingleOrDefault(source =>
                    source != null && counterSegment != null && string.Equals(
                        source.captureId,
                        counterSegment.sourceCaptureId,
                        StringComparison.Ordinal));
            string path = Path.Combine(
                physicalDirectory,
                ValidationReportFileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "12-second select validation report is missing.",
                    path);
            }

            RejectReparsePoint(path, "select validation report");
            AuditionPvTwelveSecondValidationReport report =
                JsonUtility.FromJson<AuditionPvTwelveSecondValidationReport>(
                    File.ReadAllText(path, Encoding.UTF8));
            if (report == null ||
                !string.Equals(
                    report.schemaVersion,
                    ValidationSchema,
                    StringComparison.Ordinal) ||
                !report.passed ||
                !PathsEqual(report.outputDirectory, logicalFinalDirectory) ||
                !string.Equals(
                    report.manifestFile,
                    ManifestFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.manifestSha256,
                    manifestSha256,
                    StringComparison.Ordinal) ||
                manifest == null ||
                !string.Equals(
                    report.outputId,
                    manifest.outputId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.frameHashLedgerFile,
                    manifest.frameHashLedgerFile,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.frameHashLedgerSha256,
                    manifest.frameHashLedgerSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.contactSheetFile,
                    manifest.contactSheet?.file,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.contactSheetSha256,
                    manifest.contactSheet?.sha256,
                    StringComparison.Ordinal) ||
                report.contactSheetByteLength !=
                manifest.contactSheet?.byteLength ||
                !string.Equals(
                    report.proxyFile,
                    manifest.proxy?.proxyFile,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.proxySha256,
                    manifest.proxy?.proxySha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.proxyProbeFile,
                    manifest.proxy?.probeFile,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.proxyProbeSha256,
                    manifest.proxy?.probeSha256,
                    StringComparison.Ordinal) ||
                counterSource == null ||
                !string.Equals(
                    report.g06SourceCaptureId,
                    counterSource.captureId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.g06RuntimeProofPath,
                    counterSource.runtimeProofPath,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    report.g06RuntimeProofSha256,
                    counterSource.runtimeProofSha256,
                    StringComparison.Ordinal) ||
                report.sourceManifestCount != manifest.sourceManifests.Length ||
                report.segmentCount != manifest.segments.Length ||
                report.frameCount != manifest.frames.Length ||
                report.checks == null ||
                !report.checks.SequenceEqual(
                    RequiredValidationChecks,
                    StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "12-second select validation report failed round-trip validation.");
            }
        }

        private static void ValidateSourceIdentityPins(ValidatedPlan plan)
        {
            foreach (LoadedSource source in plan.sources)
            {
                if (!File.Exists(source.manifestPath) ||
                    !string.Equals(
                        FileSha256(source.manifestPath),
                        source.manifestSha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "A source capture manifest changed during assembly: "
                        + source.manifestPath);
                }

                if (!string.IsNullOrEmpty(source.runtimeProofSha256))
                {
                    ValidateNoG06FailureArtifacts(source.captureDirectory);
                    if (!File.Exists(source.runtimeProofPath))
                    {
                        throw new InvalidDataException(
                            "The pinned G06 runtime proof disappeared during "
                            + "assembly: " + source.runtimeProofPath);
                    }

                    RejectExistingReparseChain(
                        source.runtimeProofPath,
                        "G06 runtime proof");
                    byte[] proofBytes = File.ReadAllBytes(source.runtimeProofPath);
                    if (!string.Equals(
                            BytesSha256(proofBytes),
                            source.runtimeProofSha256,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            "The pinned G06 runtime proof changed during assembly: "
                            + source.runtimeProofPath);
                    }

                    ValidateG06RuntimeProofDocument(
                        source,
                        DecodeStrictUtf8Json(
                            proofBytes,
                            "G06 runtime proof at atomic install"));
                }
            }
        }

        private static void ValidateProxyToolSpecification(
            AuditionPvTwelveSecondProxyToolSpec tools)
        {
            if (tools == null ||
                string.IsNullOrWhiteSpace(tools.ffmpegPath) ||
                string.IsNullOrWhiteSpace(tools.ffprobePath) ||
                ContainsTraversalSegment(tools.ffmpegPath) ||
                ContainsTraversalSegment(tools.ffprobePath) ||
                !Path.IsPathRooted(tools.ffmpegPath) ||
                !Path.IsPathRooted(tools.ffprobePath) ||
                !AuditionPvSha256.IsSha256(tools.ffmpegSha256) ||
                !AuditionPvSha256.IsSha256(tools.ffprobeSha256) ||
                PathsEqual(tools.ffmpegPath, tools.ffprobePath))
            {
                throw new InvalidDataException(
                    "The segment specification must pin distinct absolute ffmpeg "
                    + "and ffprobe paths with verified SHA-256 values.");
            }
        }

        private static AuditionPvTwelveSecondProxyToolSpec ValidateProxyTools(
            AuditionPvTwelveSecondProxyToolSpec tools)
        {
            ValidateProxyToolSpecification(tools);
            var normalized = new AuditionPvTwelveSecondProxyToolSpec
            {
                ffmpegPath = NormalizePath(Path.GetFullPath(tools.ffmpegPath)),
                ffmpegSha256 = tools.ffmpegSha256,
                ffprobePath = NormalizePath(Path.GetFullPath(tools.ffprobePath)),
                ffprobeSha256 = tools.ffprobeSha256
            };
            ValidateProxyToolPins(normalized);
            return normalized;
        }

        private static void ValidateProxyToolPins(
            AuditionPvTwelveSecondProxyToolSpec tools)
        {
            if (!File.Exists(tools.ffmpegPath) ||
                !File.Exists(tools.ffprobePath))
            {
                throw new FileNotFoundException(
                    "Pinned ffmpeg/ffprobe tools are required before proxy "
                    + "materialization.");
            }

            RejectReparsePoint(tools.ffmpegPath, "ffmpeg binary");
            RejectReparsePoint(tools.ffprobePath, "ffprobe binary");

            if (!string.Equals(
                    FileSha256(tools.ffmpegPath),
                    tools.ffmpegSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    FileSha256(tools.ffprobePath),
                    tools.ffprobeSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Pinned ffmpeg or ffprobe binary SHA-256 does not match the "
                    + "segment specification.");
            }
        }

        private static void ValidateProxyArtifact(
            string physicalDirectory,
            AuditionPvTwelveSecondProxyArtifact proxy)
        {
            if (proxy == null ||
                !string.Equals(
                    proxy.proxyFile,
                    ProxyFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    proxy.probeFile,
                    ProxyProbeFileName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    Path.GetFileName(proxy.proxyFile),
                    proxy.proxyFile,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    Path.GetFileName(proxy.probeFile),
                    proxy.probeFile,
                    StringComparison.Ordinal) ||
                !AuditionPvSha256.IsSha256(proxy.proxySha256) ||
                !AuditionPvSha256.IsSha256(proxy.probeSha256) ||
                !AuditionPvSha256.IsSha256(proxy.ffmpegSha256) ||
                !AuditionPvSha256.IsSha256(proxy.ffprobeSha256) ||
                proxy.proxyByteLength <= 0 ||
                proxy.width != AuditionPvCaptureContract.Width ||
                proxy.height != AuditionPvCaptureContract.Height ||
                proxy.frameCount != ExpectedFrameCount ||
                Math.Abs(proxy.durationSeconds - 12d) > 0.000001d ||
                !string.Equals(proxy.rFrameRate, "60/1", StringComparison.Ordinal) ||
                !string.Equals(proxy.avgFrameRate, "60/1", StringComparison.Ordinal) ||
                !string.Equals(proxy.codecName, "h264", StringComparison.Ordinal) ||
                !string.Equals(proxy.pixelFormat, "yuv420p", StringComparison.Ordinal) ||
                proxy.videoStreamCount != 1 ||
                proxy.audioStreamCount != 0 ||
                !proxy.silent ||
                !proxy.ffmpegVersionLine.StartsWith(
                    "ffmpeg version " + RequiredProxyToolVersion,
                    StringComparison.Ordinal) ||
                !proxy.ffprobeVersionLine.StartsWith(
                    "ffprobe version " + RequiredProxyToolVersion,
                    StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(proxy.ffmpegPath) ||
                string.IsNullOrWhiteSpace(proxy.ffprobePath) ||
                !Path.IsPathRooted(proxy.ffmpegPath) ||
                !Path.IsPathRooted(proxy.ffprobePath))
            {
                throw new InvalidDataException(
                    "Silent H.264 proxy artifact metadata does not satisfy the "
                    + "QHD60/CFR60/720-frame contract.");
            }

            string proxyPath = Path.Combine(physicalDirectory, proxy.proxyFile);
            string probePath = Path.Combine(physicalDirectory, proxy.probeFile);
            if (!File.Exists(proxyPath) || !File.Exists(probePath))
            {
                throw new FileNotFoundException(
                    "Silent proxy or ffprobe evidence file is missing.");
            }

            RejectReparsePoint(proxyPath, "silent proxy artifact");
            RejectReparsePoint(probePath, "ffprobe evidence artifact");
            if (new FileInfo(proxyPath).Length != proxy.proxyByteLength ||
                !string.Equals(
                    FileSha256(proxyPath),
                    proxy.proxySha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    FileSha256(probePath),
                    proxy.probeSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Silent proxy or ffprobe evidence file SHA-256 is invalid.");
            }

            FfprobeOutput probe = ParseAndValidateProbe(
                File.ReadAllText(probePath, Encoding.UTF8));
            if (probe.videoStream.width != proxy.width ||
                probe.videoStream.height != proxy.height ||
                probe.frameCount != proxy.frameCount ||
                Math.Abs(probe.durationSeconds - proxy.durationSeconds) >
                0.000001d ||
                !string.Equals(
                    probe.videoStream.r_frame_rate,
                    proxy.rFrameRate,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    probe.videoStream.avg_frame_rate,
                    proxy.avgFrameRate,
                    StringComparison.Ordinal) ||
                probe.videoStreamCount != proxy.videoStreamCount ||
                probe.audioStreamCount != proxy.audioStreamCount)
            {
                throw new InvalidDataException(
                    "Proxy manifest metadata and sealed ffprobe evidence disagree.");
            }
        }

        private static void ValidateProxyProvenance(
            AuditionPvTwelveSecondProxyArtifact proxy,
            AuditionPvTwelveSecondProxyToolSpec tools)
        {
            if (proxy == null ||
                !PathsEqual(proxy.ffmpegPath, tools.ffmpegPath) ||
                !PathsEqual(proxy.ffprobePath, tools.ffprobePath) ||
                !string.Equals(
                    proxy.ffmpegSha256,
                    tools.ffmpegSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    proxy.ffprobeSha256,
                    tools.ffprobeSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Proxy artifact does not preserve the explicitly verified "
                    + "ffmpeg/ffprobe paths and SHA-256 pins.");
            }
        }

        private static FfprobeOutput ParseAndValidateProbe(string json)
        {
            FfprobeJson parsed = JsonUtility.FromJson<FfprobeJson>(json);
            FfprobeStream[] streams = parsed?.streams
                                      ?? Array.Empty<FfprobeStream>();
            FfprobeStream[] videos = streams.Where(value => string.Equals(
                    value.codec_type,
                    "video",
                    StringComparison.Ordinal))
                .ToArray();
            int audioCount = streams.Count(value => string.Equals(
                value.codec_type,
                "audio",
                StringComparison.Ordinal));
            if (streams.Length != 1 ||
                videos.Length != 1 ||
                audioCount != 0 ||
                parsed.format == null)
            {
                throw new InvalidDataException(
                    "ffprobe must report exactly one video stream and no audio "
                    + "streams.");
            }

            FfprobeStream video = videos[0];
            if (!int.TryParse(
                    video.nb_frames,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int frameCount) ||
                !double.TryParse(
                    video.duration,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double streamDuration) ||
                !double.TryParse(
                    parsed.format.duration,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double formatDuration) ||
                video.width != AuditionPvCaptureContract.Width ||
                video.height != AuditionPvCaptureContract.Height ||
                frameCount != ExpectedFrameCount ||
                Math.Abs(streamDuration - 12d) > 0.000001d ||
                Math.Abs(formatDuration - 12d) > 0.000001d ||
                !string.Equals(
                    video.codec_name,
                    "h264",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    video.pix_fmt,
                    "yuv420p",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    video.r_frame_rate,
                    "60/1",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    video.avg_frame_rate,
                    "60/1",
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "ffprobe does not prove silent H.264 QHD60 CFR, 720 frames, "
                    + "and exactly 12 seconds.");
            }

            return new FfprobeOutput
            {
                videoStream = video,
                videoStreamCount = videos.Length,
                audioStreamCount = audioCount,
                frameCount = frameCount,
                durationSeconds = streamDuration
            };
        }

        private static void ValidateCleanGit(
            AuditionPvGitSnapshot git,
            string label)
        {
            if (git == null ||
                !git.probeSucceeded ||
                git.isDirty ||
                string.IsNullOrWhiteSpace(git.commitSha) ||
                string.IsNullOrWhiteSpace(git.branch) ||
                !AuditionPvSha256.IsSha256(git.dirtyStateHashSha256))
            {
                throw new InvalidOperationException(
                    "The " + label + " must have a successful clean Git snapshot "
                    + "before a 12-second source select can be assembled.");
            }
        }

        private static void ValidateStableGit(
            AuditionPvGitSnapshot before,
            AuditionPvGitSnapshot after)
        {
            ValidateCleanGit(after, "worktree at atomic install");
            if (!string.Equals(
                    before.commitSha,
                    after.commitSha,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    before.branch,
                    after.branch,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    before.dirtyStateHashSha256,
                    after.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git HEAD, branch, or clean-state identity changed during "
                    + "12-second select assembly.");
            }
        }

        private static AuditionPvTwelveSecondSegmentManifest ReadSpecification(
            string specificationPath)
        {
            if (string.IsNullOrWhiteSpace(specificationPath))
            {
                throw new ArgumentException(
                    "12-second segment specification path is empty.",
                    nameof(specificationPath));
            }

            string normalized = Path.GetFullPath(specificationPath);
            if (!File.Exists(normalized))
            {
                throw new FileNotFoundException(
                    "12-second segment specification is missing. A pinned G06 "
                    + "perfect-dodge-counter source is required before assembly.",
                    normalized);
            }

            AuditionPvTwelveSecondSegmentManifest specification =
                JsonUtility.FromJson<AuditionPvTwelveSecondSegmentManifest>(
                    File.ReadAllText(normalized, Encoding.UTF8));
            ValidateSegmentContract(specification);
            return specification;
        }

        private static string ResolveSpecificationPath()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(
                        arguments[index],
                        SpecificationArgument,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (index + 1 >= arguments.Length ||
                    string.IsNullOrWhiteSpace(arguments[index + 1]))
                {
                    throw new ArgumentException(
                        SpecificationArgument + " requires a JSON file path.");
                }

                return arguments[index + 1];
            }

            return DefaultSpecificationPath;
        }

        private static string ValidateAndResolveManifestPath(
            string sourceRoot,
            string specifiedPath)
        {
            if (ContainsTraversalSegment(specifiedPath))
            {
                throw new InvalidDataException(
                    "Source manifest path contains a traversal segment.");
            }

            string path = Path.GetFullPath(specifiedPath);
            if (!string.Equals(
                    Path.GetFileName(path),
                    AuditionPvCaptureContract.ManifestFileName,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Every source manifest path must end in capture_manifest.json.");
            }

            string captureDirectory = Path.GetDirectoryName(path)
                ?? throw new InvalidDataException(
                    "Source manifest path has no capture directory.");
            string parent = Path.GetDirectoryName(captureDirectory)
                ?? string.Empty;
            if (!PathsEqual(parent, sourceRoot) || !File.Exists(path))
            {
                throw new InvalidDataException(
                    "Source manifest must be an existing direct child capture of "
                    + "the configured golden-source root.");
            }

            RejectReparsePoint(captureDirectory, "source capture directory");
            RejectReparsePoint(path, "source capture manifest");

            return NormalizePath(path);
        }

        private static string ResolveNewOutputId(
            string outputRoot,
            AuditionPvGitSnapshot git,
            DateTime createdAtUtc,
            string outputIdOverride)
        {
            if (!string.IsNullOrWhiteSpace(outputIdOverride))
            {
                AuditionPvOutputPaths.ValidateOutputId(outputIdOverride);
                return outputIdOverride;
            }

            string baseId = AuditionPvOutputPaths.CreateOutputId(
                "preedit-12s-source-select",
                createdAtUtc,
                git.commitSha,
                isDirty: false,
                git.dirtyStateHashSha256);
            for (int revision = 1; revision <= 999; revision++)
            {
                string candidate = revision == 1
                    ? baseId
                    : baseId + "_r" + revision.ToString(
                        "000",
                        CultureInfo.InvariantCulture);
                string path = ResolveDirectChild(
                    outputRoot,
                    candidate,
                    requireSimpleName: true);
                if (!Directory.Exists(path) && !File.Exists(path))
                {
                    return candidate;
                }
            }

            throw new IOException(
                "Could not choose a create-new 12-second select output ID.");
        }

        private static string CreateStagingDirectory(
            string outputRoot,
            string outputId)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                string name = "." + outputId + ".staging-"
                              + Guid.NewGuid().ToString("N");
                string path = ResolveDirectChild(
                    outputRoot,
                    name,
                    requireSimpleName: false);
                if (Directory.Exists(path) || File.Exists(path))
                {
                    continue;
                }

                Directory.CreateDirectory(path);
                return path;
            }

            throw new IOException(
                "Could not reserve a unique sibling staging directory.");
        }

        private static string ResolveDirectChild(
            string root,
            string name,
            bool requireSimpleName)
        {
            if (requireSimpleName)
            {
                AuditionPvOutputPaths.ValidateOutputId(name);
            }
            else if (string.IsNullOrWhiteSpace(name) ||
                     ContainsTraversalSegment(name) ||
                     !string.Equals(
                         Path.GetFileName(name),
                         name,
                         StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Staging directory name is unsafe.");
            }

            string normalizedRoot = NormalizeAbsoluteRoot(root);
            string candidate = Path.GetFullPath(Path.Combine(
                normalizedRoot,
                name));
            string parent = Path.GetDirectoryName(candidate) ?? string.Empty;
            if (!PathsEqual(parent, normalizedRoot))
            {
                throw new InvalidDataException(
                    "Output path is not a direct child of its configured root.");
            }

            return NormalizePath(candidate);
        }

        private static string ResolveSourceFrameDirectory(
            string captureDirectory,
            string shotId)
        {
            if (string.Equals(
                    shotId,
                    AuditionPvStationTransitionGoldenCapture.ShotId,
                    StringComparison.Ordinal))
            {
                return NormalizePath(Path.Combine(
                    captureDirectory,
                    AuditionPvStationTransitionGoldenCapture.FramesFolderName));
            }

            return NormalizePath(Path.Combine(
                captureDirectory,
                FramesFolderName,
                shotId));
        }

        private static string ResolveSourceFramePath(
            string captureDirectory,
            string shotId,
            int frame)
        {
            return NormalizePath(Path.Combine(
                ResolveSourceFrameDirectory(captureDirectory, shotId),
                FrameFileName(frame)));
        }

        private static string FrameFileName(int frame)
        {
            if (frame < 0 || frame > 9999)
            {
                throw new ArgumentOutOfRangeException(nameof(frame));
            }

            return "frame_" + frame.ToString(
                       "0000",
                       CultureInfo.InvariantCulture) + ".png";
        }

        private static string CopyPngNewAndHash(
            string sourcePath,
            string destinationPath,
            int expectedWidth,
            int expectedHeight)
        {
            string destinationDirectory = Path.GetDirectoryName(destinationPath)
                ?? throw new InvalidDataException(
                    "Select frame destination has no parent directory.");
            Directory.CreateDirectory(destinationDirectory);
            if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
            {
                throw new IOException(
                    "Select frame destination already exists and will not be "
                    + "overwritten: " + destinationPath);
            }

            using var source = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                CopyBufferBytes,
                FileOptions.SequentialScan);
            ValidatePngHeader(
                source,
                sourcePath,
                expectedWidth,
                expectedHeight);
            source.Position = 0;
            using var destination = new FileStream(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                CopyBufferBytes,
                FileOptions.SequentialScan);
            using SHA256 sha256 = SHA256.Create();
            var buffer = new byte[CopyBufferBytes];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, read);
                sha256.TransformBlock(buffer, 0, read, buffer, 0);
            }

            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            destination.Flush(flushToDisk: true);
            return LowerHex(sha256.Hash);
        }

        private static void ValidatePngHeader(
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            RejectReparsePoint(path, "PNG source/artifact");
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            ValidatePngHeader(stream, path, expectedWidth, expectedHeight);
        }

        private static void ValidatePngHeader(
            Stream stream,
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            var header = new byte[29];
            int offset = 0;
            while (offset < header.Length)
            {
                int read = stream.Read(header, offset, header.Length - offset);
                if (read <= 0)
                {
                    throw new InvalidDataException(
                        "PNG is truncated before its IHDR fields: " + path);
                }

                offset += read;
            }

            if (!header.Take(PngSignature.Length).SequenceEqual(PngSignature) ||
                ReadBigEndianInt(header, 8) != 13 ||
                header[12] != (byte)'I' ||
                header[13] != (byte)'H' ||
                header[14] != (byte)'D' ||
                header[15] != (byte)'R' ||
                ReadBigEndianInt(header, 16) != expectedWidth ||
                ReadBigEndianInt(header, 20) != expectedHeight ||
                header[24] != 8 ||
                header[25] != 6 ||
                header[26] != 0 ||
                header[27] != 0 ||
                header[28] != 0)
            {
                throw new InvalidDataException(
                    $"PNG header is not exact opaque Recorder RGBA8 "
                    + $"{expectedWidth}x{expectedHeight}: {path}");
            }
        }

        private static int ReadBigEndianInt(byte[] bytes, int offset)
        {
            return bytes[offset] << 24 |
                   bytes[offset + 1] << 16 |
                   bytes[offset + 2] << 8 |
                   bytes[offset + 3];
        }

        private static void WriteCanonicalFrameHashLedger(
            string path,
            IEnumerable<AuditionPvTwelveSecondFrameMapping> mappings)
        {
            string text = string.Join(
                "\n",
                mappings.OrderBy(value => value.selectFrame).Select(value =>
                    value.sha256 + "  "
                    + value.selectRelativePath.Replace('\\', '/'))) + "\n";
            WriteTextNew(path, text);
        }

        private static void ValidateCanonicalFrameHashLedger(
            string path,
            IReadOnlyList<AuditionPvTwelveSecondFrameMapping> mappings)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Select frame hash ledger is missing.",
                    path);
            }

            RejectReparsePoint(path, "select frame hash ledger");
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length != mappings.Count)
            {
                throw new InvalidDataException(
                    "Select frame hash ledger line count is invalid.");
            }

            for (int index = 0; index < mappings.Count; index++)
            {
                string expected = mappings[index].sha256 + "  "
                                  + mappings[index].selectRelativePath
                                      .Replace('\\', '/');
                if (!string.Equals(
                        lines[index],
                        expected,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Select frame hash ledger is not canonical at line "
                        + (index + 1) + ".");
                }
            }
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            WriteTextNew(
                path,
                JsonUtility.ToJson(value, prettyPrint: true)
                + Environment.NewLine);
        }

        private static void WriteTextNew(string path, string value)
        {
            string directory = Path.GetDirectoryName(path)
                ?? throw new InvalidDataException(
                    "Artifact path has no parent directory.");
            Directory.CreateDirectory(directory);
            using var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            using var writer = new StreamWriter(
                stream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(value);
        }

        private static void WriteBytesNew(string path, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            string directory = Path.GetDirectoryName(path)
                ?? throw new InvalidDataException(
                    "Binary artifact path has no parent directory.");
            Directory.CreateDirectory(directory);
            using var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(flushToDisk: true);
        }

        private static string FileSha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha256 = SHA256.Create();
            return LowerHex(sha256.ComputeHash(stream));
        }

        private static string BytesSha256(byte[] bytes)
        {
            using SHA256 sha256 = SHA256.Create();
            return LowerHex(sha256.ComputeHash(bytes));
        }

        private static string LowerHex(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new InvalidDataException("SHA-256 computation returned null.");
            }

            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
            {
                builder.Append(value.ToString(
                    "x2",
                    CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static string NormalizeAbsoluteRoot(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentException("Path root must not be empty.");
            }

            string fullPath = Path.GetFullPath(root);
            string pathRoot = Path.GetPathRoot(fullPath) ?? string.Empty;
            string normalized = fullPath.Length > pathRoot.Length
                ? fullPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)
                : fullPath;
            if (!Path.IsPathRooted(normalized))
            {
                throw new ArgumentException("Path root must be absolute.");
            }

            return NormalizePath(normalized);
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private static bool PathsEqual(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) ||
                string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            string normalizedLeft = Path.GetFullPath(left)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            string normalizedRight = Path.GetFullPath(right)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            return string.Equals(
                normalizedLeft,
                normalizedRight,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSameOrDescendant(string path, string root)
        {
            string normalizedPath = Path.GetFullPath(path)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            string normalizedRoot = Path.GetFullPath(root)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            return string.Equals(
                       normalizedPath,
                       normalizedRoot,
                       StringComparison.OrdinalIgnoreCase) ||
                   normalizedPath.StartsWith(
                       normalizedRoot + Path.DirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase) ||
                   normalizedPath.StartsWith(
                       normalizedRoot + Path.AltDirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsTraversalSegment(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return true;
            }

            return path.Replace('\\', '/')
                .Split('/')
                .Any(value => value == "." || value == "..");
        }

        private static void RejectReparsePoint(string path, string label)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException(
                    label + " must not be a symbolic link, junction, or other "
                    + "reparse point: " + path);
            }
        }

        private static void RejectExistingReparseChain(
            string path,
            string label)
        {
            string current = Path.GetFullPath(path);
            while (!string.IsNullOrWhiteSpace(current))
            {
                if ((File.Exists(current) || Directory.Exists(current)) &&
                    (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidDataException(
                        label + " must not traverse a symbolic link, junction, "
                        + "or other reparse point: " + current);
                }

                string parent = Path.GetDirectoryName(current);
                if (string.IsNullOrWhiteSpace(parent) ||
                    PathsEqual(parent, current))
                {
                    break;
                }

                current = parent;
            }
        }

        private static void ValidateSimpleId(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.Length > 64 ||
                value.Any(character =>
                    !(character >= 'a' && character <= 'z' ||
                      character >= '0' && character <= '9' ||
                      character == '-' || character == '_')))
            {
                throw new InvalidDataException(
                    label + " must be a safe lowercase ASCII identifier.");
            }
        }

        private sealed class ExternalProxyEncoder :
            IAuditionPvTwelveSecondProxyEncoder
        {
            private const int VersionTimeoutMilliseconds = 30000;
            private const int EncodeTimeoutMilliseconds = 15 * 60 * 1000;

            public AuditionPvTwelveSecondProxyArtifact Encode(
                string stagingDirectory,
                AuditionPvTwelveSecondProxyToolSpec tools)
            {
                ProcessResult ffmpegVersion = RunProcess(
                    tools.ffmpegPath,
                    "-version",
                    VersionTimeoutMilliseconds);
                ProcessResult ffprobeVersion = RunProcess(
                    tools.ffprobePath,
                    "-version",
                    VersionTimeoutMilliseconds);
                string ffmpegVersionLine = FirstLine(ffmpegVersion.standardOutput);
                string ffprobeVersionLine = FirstLine(ffprobeVersion.standardOutput);
                string expectedFfmpegPrefix = "ffmpeg version "
                                              + RequiredProxyToolVersion
                                              + "-essentials_build";
                string expectedFfprobePrefix = "ffprobe version "
                                               + RequiredProxyToolVersion
                                               + "-essentials_build";
                if (!ffmpegVersionLine.StartsWith(
                        expectedFfmpegPrefix,
                        StringComparison.Ordinal) ||
                    !ffprobeVersionLine.StartsWith(
                        expectedFfprobePrefix,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Pinned proxy tools must report the verified FFmpeg "
                        + RequiredProxyToolVersion + " essentials_build version.");
                }

                string inputPattern = Path.Combine(
                    stagingDirectory,
                    FramesFolderName,
                    "frame_%04d.png");
                string proxyPath = Path.Combine(
                    stagingDirectory,
                    ProxyFileName);
                string probePath = Path.Combine(
                    stagingDirectory,
                    ProxyProbeFileName);
                if (File.Exists(proxyPath) || Directory.Exists(proxyPath) ||
                    File.Exists(probePath) || Directory.Exists(probePath))
                {
                    throw new IOException(
                        "Proxy or ffprobe destination already exists and will not "
                        + "be overwritten.");
                }

                string ffmpegArguments = string.Join(" ", new[]
                {
                    "-hide_banner",
                    "-loglevel", "error",
                    "-nostdin",
                    "-n",
                    "-framerate", AuditionPvCaptureContract.Fps.ToString(
                        CultureInfo.InvariantCulture),
                    "-start_number", "0",
                    "-i", QuoteArgument(inputPattern),
                    "-frames:v", ExpectedFrameCount.ToString(
                        CultureInfo.InvariantCulture),
                    "-an",
                    "-c:v", "libx264",
                    "-preset", "medium",
                    "-crf", "18",
                    "-pix_fmt", "yuv420p",
                    "-fps_mode", "cfr",
                    "-movflags", "+faststart",
                    QuoteArgument(proxyPath)
                });
                RunProcess(
                    tools.ffmpegPath,
                    ffmpegArguments,
                    EncodeTimeoutMilliseconds);
                if (!File.Exists(proxyPath) ||
                    new FileInfo(proxyPath).Length <= 0)
                {
                    throw new InvalidDataException(
                        "FFmpeg completed without a non-empty silent proxy.");
                }

                ProcessResult probeResult = RunProcess(
                    tools.ffprobePath,
                    "-v error -print_format json -show_streams -show_format "
                    + QuoteArgument(proxyPath),
                    VersionTimeoutMilliseconds);
                FfprobeOutput probe = ParseAndValidateProbe(
                    probeResult.standardOutput);
                WriteTextNew(
                    probePath,
                    probeResult.standardOutput.TrimEnd() + Environment.NewLine);

                return new AuditionPvTwelveSecondProxyArtifact
                {
                    proxyFile = ProxyFileName,
                    proxySha256 = FileSha256(proxyPath),
                    proxyByteLength = new FileInfo(proxyPath).Length,
                    probeFile = ProxyProbeFileName,
                    probeSha256 = FileSha256(probePath),
                    ffmpegPath = tools.ffmpegPath,
                    ffmpegSha256 = tools.ffmpegSha256,
                    ffmpegVersionLine = ffmpegVersionLine,
                    ffprobePath = tools.ffprobePath,
                    ffprobeSha256 = tools.ffprobeSha256,
                    ffprobeVersionLine = ffprobeVersionLine,
                    codecName = probe.videoStream.codec_name,
                    pixelFormat = probe.videoStream.pix_fmt,
                    width = probe.videoStream.width,
                    height = probe.videoStream.height,
                    rFrameRate = probe.videoStream.r_frame_rate,
                    avgFrameRate = probe.videoStream.avg_frame_rate,
                    frameCount = probe.frameCount,
                    durationSeconds = probe.durationSeconds,
                    videoStreamCount = probe.videoStreamCount,
                    audioStreamCount = probe.audioStreamCount,
                    silent = probe.audioStreamCount == 0
                };
            }

            private static ProcessResult RunProcess(
                string executable,
                string arguments,
                int timeoutMilliseconds)
            {
                using var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = executable,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = new UTF8Encoding(false),
                        StandardErrorEncoding = new UTF8Encoding(false)
                    }
                };
                if (!process.Start())
                {
                    throw new InvalidOperationException(
                        "Could not start proxy tool: " + executable);
                }

                Task<string> outputTask =
                    process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask =
                    process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(timeoutMilliseconds))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // The timeout remains the primary failure.
                    }

                    try
                    {
                        process.WaitForExit(30000);
                        Task.WaitAll(
                            new Task[] { outputTask, errorTask },
                            30000);
                    }
                    catch
                    {
                        // Best-effort handle/pipe drain after the primary timeout.
                    }

                    throw new TimeoutException(
                        "Proxy tool timed out: " + executable);
                }

                Task.WaitAll(new Task[] { outputTask, errorTask });
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        "Proxy tool failed (" + process.ExitCode + "): "
                        + executable + Environment.NewLine
                        + errorTask.Result.Trim());
                }

                return new ProcessResult
                {
                    standardOutput = outputTask.Result,
                    standardError = errorTask.Result
                };
            }

            private static string FirstLine(string value)
            {
                using var reader = new StringReader(value ?? string.Empty);
                return reader.ReadLine() ?? string.Empty;
            }

            private static string QuoteArgument(string value)
            {
                if (value == null || value.Contains('"'))
                {
                    throw new InvalidDataException(
                        "Proxy tool argument contains an unsupported quote.");
                }

                return "\"" + value + "\"";
            }
        }

        private sealed class ProcessResult
        {
            public string standardOutput = string.Empty;
            public string standardError = string.Empty;
        }

        [Serializable]
        private sealed class FfprobeJson
        {
            public FfprobeStream[] streams = Array.Empty<FfprobeStream>();
            public FfprobeFormat format;
        }

        [Serializable]
        private sealed class FfprobeStream
        {
            public string codec_name = string.Empty;
            public string codec_type = string.Empty;
            public int width;
            public int height;
            public string pix_fmt = string.Empty;
            public string r_frame_rate = string.Empty;
            public string avg_frame_rate = string.Empty;
            public string duration = string.Empty;
            public string nb_frames = string.Empty;
        }

        [Serializable]
        private sealed class FfprobeFormat
        {
            public string duration = string.Empty;
        }

        private sealed class FfprobeOutput
        {
            public FfprobeStream videoStream;
            public int videoStreamCount;
            public int audioStreamCount;
            public int frameCount;
            public double durationSeconds;
        }

        private sealed class ValidatedPlan
        {
            public LoadedSource[] sources = Array.Empty<LoadedSource>();
            public ValidatedSegment[] segments = Array.Empty<ValidatedSegment>();
        }

        private sealed class LoadedSource
        {
            public string manifestPath = string.Empty;
            public string captureDirectory = string.Empty;
            public string manifestSha256 = string.Empty;
            public string dependencyIdentitySha256 = string.Empty;
            public string runtimeProofPath = string.Empty;
            public string runtimeProofSha256 = string.Empty;
            public AuditionPvCaptureManifest manifest;
        }

        private sealed class ValidatedSegment
        {
            public AuditionPvTwelveSecondSegmentSpec specification;
            public LoadedSource source;
            public AuditionPvShotManifestEntry shot;
            public int outputStartFrame;
            public int outputEndFrame;
        }
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondSegmentManifest
    {
        public string schemaVersion =
            AuditionPvTwelveSecondGoldAssembler.SpecificationSchema;
        public AuditionPvTwelveSecondProxyToolSpec proxyTools =
            new AuditionPvTwelveSecondProxyToolSpec();
        public AuditionPvTwelveSecondSegmentSpec[] segments =
            Array.Empty<AuditionPvTwelveSecondSegmentSpec>();
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondProxyToolSpec
    {
        public string ffmpegPath =
            AuditionPvTwelveSecondGoldAssembler.DefaultFfmpegPath;
        public string ffmpegSha256 =
            AuditionPvTwelveSecondGoldAssembler.DefaultFfmpegSha256;
        public string ffprobePath =
            AuditionPvTwelveSecondGoldAssembler.DefaultFfprobePath;
        public string ffprobeSha256 =
            AuditionPvTwelveSecondGoldAssembler.DefaultFfprobeSha256;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondSegmentSpec
    {
        public string role = string.Empty;
        public int order;
        public string sourceManifestPath = string.Empty;
        public string sourceManifestSha256 = string.Empty;
        public string sourceDependencyIdentitySha256 = string.Empty;
        public string sourceRuntimeProofSha256 = string.Empty;
        public string shotId = string.Empty;
        public int startFrame;
        public int endFrame;
        public string[] sourceFrameSha256 = Array.Empty<string>();
    }

    [Serializable]
    internal sealed class AuditionPvG06RuntimeProofArtifact
    {
        internal const string Schema =
            "dimension-brawl.audition-pv.g06-runtime-proof.v1";
        internal const string Mapping =
            "Recorder raw0 is preserved warm-up evidence; raw1..raw360 map to logical f0..f359.";
        internal const string ProductScreenProfile =
            "authored product profile used unchanged: enabled=true, domain=.14, "
            + "invert=.015, edge=.18, glitch=.03, duration=.42s.";
        internal const string SummonCounterContract =
            "authored Slot1 cost=200, full EN 300->100, tier=2, "
            + "screen intercept=1, automatic counter damage=29.44.";

        public string schema = string.Empty;
        public string captureId = string.Empty;
        public string mapping = string.Empty;
        public string productScreenProfile = string.Empty;
        public string summonCounterContract = string.Empty;
        public AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof runtime;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondSelectManifest
    {
        public string schemaVersion = string.Empty;
        public string outputId = string.Empty;
        public string createdAtUtc = string.Empty;
        public string sourceRoot = string.Empty;
        public string outputRoot = string.Empty;
        public string outputDirectory = string.Empty;
        public string sourceFormat = string.Empty;
        public int width;
        public int height;
        public int fps;
        public int totalFrames;
        public double durationSeconds;
        public string gitCommitSha = string.Empty;
        public string gitBranch = string.Empty;
        public string worktreeDirtyHashSha256 = string.Empty;
        public string frameHashLedgerFile = string.Empty;
        public string frameHashLedgerSha256 = string.Empty;
        public AuditionPvTwelveSecondContactSheetArtifact contactSheet;
        public AuditionPvTwelveSecondProxyArtifact proxy;
        public AuditionPvTwelveSecondSourceManifestIdentity[] sourceManifests =
            Array.Empty<AuditionPvTwelveSecondSourceManifestIdentity>();
        public AuditionPvTwelveSecondSelectSegment[] segments =
            Array.Empty<AuditionPvTwelveSecondSelectSegment>();
        public AuditionPvTwelveSecondFrameMapping[] frames =
            Array.Empty<AuditionPvTwelveSecondFrameMapping>();
        public AuditionPvTwelveSecondBaselineReference[] baselineReferences =
            Array.Empty<AuditionPvTwelveSecondBaselineReference>();
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondSourceManifestIdentity
    {
        public string captureId = string.Empty;
        public string manifestPath = string.Empty;
        public string manifestSha256 = string.Empty;
        public string dependencyIdentitySha256 = string.Empty;
        public int dependencyCount;
        public string gitCommitSha = string.Empty;
        public string gitBranch = string.Empty;
        public string worktreeDirtyHashSha256 = string.Empty;
        public string unityVersionWithRevision = string.Empty;
        public string recorderPackageVersion = string.Empty;
        public string urpPackageVersion = string.Empty;
        public string activeRenderPipelineAssetPath = string.Empty;
        public string runtimeProofPath = string.Empty;
        public string runtimeProofSha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondSelectSegment
    {
        public string role = string.Empty;
        public int order;
        public string hudMode = string.Empty;
        public string sourceCaptureId = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceStartFrame;
        public int sourceEndFrame;
        public int selectStartFrame;
        public int selectEndFrame;
        public int frameCount;
        public string sourceRuntimeProofSha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondFrameMapping
    {
        public int selectFrame;
        public string role = string.Empty;
        public int segmentOrder;
        public string sourceCaptureId = string.Empty;
        public string sourceManifestSha256 = string.Empty;
        public string sourceDependencyIdentitySha256 = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceFrame;
        public string sourceRelativePath = string.Empty;
        public string selectRelativePath = string.Empty;
        public string sha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondProxyArtifact
    {
        public string proxyFile = string.Empty;
        public string proxySha256 = string.Empty;
        public long proxyByteLength;
        public string probeFile = string.Empty;
        public string probeSha256 = string.Empty;
        public string ffmpegPath = string.Empty;
        public string ffmpegSha256 = string.Empty;
        public string ffmpegVersionLine = string.Empty;
        public string ffprobePath = string.Empty;
        public string ffprobeSha256 = string.Empty;
        public string ffprobeVersionLine = string.Empty;
        public string codecName = string.Empty;
        public string pixelFormat = string.Empty;
        public int width;
        public int height;
        public string rFrameRate = string.Empty;
        public string avgFrameRate = string.Empty;
        public int frameCount;
        public double durationSeconds;
        public int videoStreamCount;
        public int audioStreamCount;
        public bool silent;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondContactSheetArtifact
    {
        public string file = string.Empty;
        public string sha256 = string.Empty;
        public long byteLength;
        public int width;
        public int height;
        public int cellWidth;
        public int cellHeight;
        public int columns;
        public int rows;
        public string downsamplePolicy = string.Empty;
        public AuditionPvTwelveSecondContactSheetCell[] cells =
            Array.Empty<AuditionPvTwelveSecondContactSheetCell>();
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondContactSheetCell
    {
        public int cellIndex;
        public int row;
        public int column;
        public int outputFrame;
        public int segmentOrder;
        public string role = string.Empty;
        public string sourceCaptureId = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceFrame;
        public string sourceSha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondBaselineReference
    {
        public string sourceCaptureId = string.Empty;
        public string sourceBaselineId = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceFrame;
        public string sourceBaselineFileName = string.Empty;
        public string hudMode = string.Empty;
        public bool includedInSelect;
        public int selectFrame = -1;
        public string selectRelativePath = string.Empty;
        public string sha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondValidationReport
    {
        public string schemaVersion = string.Empty;
        public string validatedAtUtc = string.Empty;
        public bool passed;
        public string outputId = string.Empty;
        public string outputDirectory = string.Empty;
        public string manifestFile = string.Empty;
        public string manifestSha256 = string.Empty;
        public string frameHashLedgerFile = string.Empty;
        public string frameHashLedgerSha256 = string.Empty;
        public string contactSheetFile = string.Empty;
        public string contactSheetSha256 = string.Empty;
        public long contactSheetByteLength;
        public string proxyFile = string.Empty;
        public string proxySha256 = string.Empty;
        public string proxyProbeFile = string.Empty;
        public string proxyProbeSha256 = string.Empty;
        public string g06SourceCaptureId = string.Empty;
        public string g06RuntimeProofPath = string.Empty;
        public string g06RuntimeProofSha256 = string.Empty;
        public int sourceManifestCount;
        public int segmentCount;
        public int frameCount;
        public string[] checks = Array.Empty<string>();
    }

    internal sealed class AuditionPvTwelveSecondAssemblyResult
    {
        public string outputId = string.Empty;
        public string outputDirectory = string.Empty;
        public string manifestPath = string.Empty;
        public string validationReportPath = string.Empty;
        public string frameHashPath = string.Empty;
        public string contactSheetPath = string.Empty;
        public string proxyPath = string.Empty;
        public int frameCount;
    }

    internal interface IAuditionPvTwelveSecondProxyEncoder
    {
        AuditionPvTwelveSecondProxyArtifact Encode(
            string stagingDirectory,
            AuditionPvTwelveSecondProxyToolSpec tools);
    }
}
