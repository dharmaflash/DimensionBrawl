using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Materializes the physical, range-bound evidence consumed by the 60-second Gate.
    /// It is intentionally not a manifest composer and never creates a human approval.
    /// </summary>
    internal static class AuditionPvSixtySecondEvidenceProducer
    {
        internal const string ReceiptSchema =
            "dimension-brawl.audition-pv.sixty-second-evidence-bundle.v1";
        internal const string ReviewSkeletonSchema =
            "dimension-brawl.audition-pv.take-review-skeleton.v1";
        internal const string FilmstripSkeletonSchema =
            "dimension-brawl.audition-pv.temporal-filmstrip-skeleton.v1";
        internal const string ToolVersion = "2";
        internal const string AutomatedTestSuite = "AuditionPvSixtySecondEvidence";
        internal const long MaxJsonBytes = 32L * 1024L * 1024L;
        internal const long MaxPngBytes = 32L * 1024L * 1024L;
        internal const int MaxPreviewCells = 32;
        private const string Rec709TransformId = "srgb8-to-bt709-oetf-rgba8-v1";
        private const string RendererAlgorithm =
            "unity-runtime-renderer-material-inventory-v2";
        private const string HudAlgorithm = "capture-runtime-hud-workload-v3";
        private const string MagentaAlgorithm =
            "full-frame-error-magenta-rgb255-0-255-v1";

        internal static AuditionPvSixtySecondEvidenceBundle Produce(
            AuditionPvSixtySecondEvidenceRequest request)
        {
            ValidatedRequest value = ValidateRequest(request);
            DateTime startedAtUtc = DateTime.UtcNow;
            string createdAtUtc = startedAtUtc.ToString("O", CultureInfo.InvariantCulture);
            string evidenceDirectory = value.evidenceDirectory;
            string reviewDirectory = value.reviewDirectory;
            Directory.CreateDirectory(evidenceDirectory);
            Directory.CreateDirectory(reviewDirectory);

            string sourceLedgerPath = Path.Combine(
                value.captureDirectory,
                "evidence",
                "sixty_second",
                SafeComponent(value.shot.id),
                "canonical_source_frame_hashes.sha256");
            List<PhysicalFrame> physicalFrames = ProcessPhysicalFrames(
                value,
                sourceLedgerPath);
            string sourceLedgerSha256 = AuditionPvSha256.FileHash(sourceLedgerPath);
            AuditionPvPinnedArtifact sourceLedgerPin = Pin(sourceLedgerPath);

            int[] previewFrames = SampledFrames(
                value.request.selectStartFrame,
                value.request.selectEndFrame);
            AuditionPvMeasuredFrame[] previewMeasured = MeasuredFrames(
                physicalFrames,
                previewFrames);
            string contactSheetPath = Path.Combine(evidenceDirectory, "contact_sheet_q25.png");
            CreateContactSheet(value, physicalFrames, previewFrames, contactSheetPath);

            RuntimeFacts runtime = ReadAndValidateRuntimeSpool(value, physicalFrames);
            var tests = new List<AuditionPvTestResult>();
            var checks = new List<AuditionPvAutomatedCheckEvidence>();
            var checkPins = new List<AuditionPvNamedPinnedArtifact>();
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);

            AddSimpleChecks(
                value,
                physicalFrames,
                previewMeasured,
                contactSheetPath,
                sourceLedgerSha256,
                createdAtUtc,
                duration,
                checks,
                checkPins,
                tests);
            AddMagentaCheck(
                value,
                physicalFrames,
                previewMeasured,
                sourceLedgerSha256,
                createdAtUtc,
                duration,
                checks,
                checkPins,
                tests);
            Rec709EvidencePins rec709 = AddRec709Check(
                value,
                physicalFrames,
                sourceLedgerSha256,
                createdAtUtc,
                duration,
                checks,
                checkPins,
                tests);
            RuntimeEvidencePins renderer = AddRuntimeCheck(
                value,
                runtime,
                physicalFrames,
                previewMeasured,
                sourceLedgerSha256,
                createdAtUtc,
                duration,
                "renderer-material-scan",
                checks,
                checkPins,
                tests);
            RuntimeEvidencePins hud = null;
            if (value.request.cleanPlate)
            {
                hud = AddRuntimeCheck(
                    value,
                    runtime,
                    physicalFrames,
                    previewMeasured,
                    sourceLedgerSha256,
                    createdAtUtc,
                    duration,
                    "hud-layer-absent",
                    checks,
                    checkPins,
                    tests);
            }

            string automatedProofPath = Path.Combine(evidenceDirectory, "automated_proof.json");
            var automatedProof = BindRange(
                new AuditionPvTakeAutomatedProofArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .AutomatedProofSchema,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    checks = checks.ToArray()
                },
                value.request);
            WriteJsonNew(automatedProofPath, automatedProof, MaxJsonBytes);
            AuditionPvPinnedArtifact automatedProofPin = Pin(automatedProofPath);

            AuditionPvPinnedArtifact filmstripPin = WriteFilmstripSkeleton(
                value,
                previewMeasured,
                Pin(contactSheetPath),
                createdAtUtc);
            AuditionPvPinnedArtifact reviewSkeletonPin = WriteReviewSkeleton(
                value,
                physicalFrames,
                Pin(contactSheetPath),
                filmstripPin,
                createdAtUtc);

            AuditionPvTestResult[] resultTests = tests.ToArray();
            ValidateUniquePassedTests(resultTests);
            var receipt = BindRange(
                new AuditionPvSixtySecondEvidenceBundleReceipt
                {
                    schemaVersion = ReceiptSchema,
                    status = "physical-evidence-complete-human-review-required",
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    sourceFrameLedger = sourceLedgerPin,
                    automatedProof = automatedProofPin,
                    checkResults = checkPins.ToArray(),
                    contactSheet = Pin(contactSheetPath),
                    filmstripSkeleton = filmstripPin,
                    humanReviewSkeleton = reviewSkeletonPin,
                    rec709Config = rec709.config,
                    rec709OutputLedger = rec709.ledger,
                    rendererRuntimeWorkload = renderer.workload,
                    hudRuntimeWorkload = hud?.workload ?? new AuditionPvPinnedArtifact(),
                    generatedTestResults = resultTests,
                    maxSimultaneousDecodedSourcePngs =
                        AuditionPvEvidenceMemoryContract.MaxSimultaneousDecodedSourcePngs,
                    maxTransientWorkingSetBytes =
                        AuditionPvEvidenceMemoryContract.MaxTransientWorkingSetBytes,
                    producer = nameof(AuditionPvSixtySecondEvidenceProducer),
                    producerVersion = ToolVersion,
                    createdAtUtc = createdAtUtc
                },
                value.request);
            string receiptPath = Path.Combine(evidenceDirectory, "evidence_bundle_receipt.json");
            WriteJsonNew(receiptPath, receipt, MaxJsonBytes);

            return new AuditionPvSixtySecondEvidenceBundle
            {
                receipt = Pin(receiptPath),
                sourceFrameLedger = sourceLedgerPin,
                automatedProof = automatedProofPin,
                contactSheet = Pin(contactSheetPath),
                filmstripSkeleton = filmstripPin,
                humanReviewSkeleton = reviewSkeletonPin,
                rec709Config = rec709.config,
                rec709OutputLedger = rec709.ledger,
                rendererRuntimeWorkload = renderer.workload,
                hudRuntimeWorkload = hud?.workload ?? new AuditionPvPinnedArtifact(),
                testResults = resultTests
            };
        }

        /// <summary>
        /// Capture producers call this before writing their immutable capture manifest.
        /// Exact duplicate artifacts are rejected; the Gate intentionally permits the same
        /// typed check name for distinct atomic ranges of one source shot.
        /// </summary>
        internal static AuditionPvTestResult[] MergeCaptureTestResults(
            IEnumerable<AuditionPvTestResult> existing,
            AuditionPvSixtySecondEvidenceBundle bundle)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            AuditionPvTestResult[] merged = (existing ?? Array.Empty<AuditionPvTestResult>())
                .Concat(bundle.testResults ?? Array.Empty<AuditionPvTestResult>())
                .ToArray();
            var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvTestResult result in merged)
            {
                if (result == null || string.IsNullOrWhiteSpace(result.suite) ||
                    string.IsNullOrWhiteSpace(result.name) ||
                    string.IsNullOrWhiteSpace(result.artifactPath) ||
                    !identities.Add(result.suite + "\0" + result.name + "\0" +
                                    Full(result.artifactPath)))
                    throw new InvalidDataException(
                        "Capture test results contain a null or exact-duplicate artifact identity.");
            }
            ValidateUniquePassedTests(bundle.testResults);
            return merged;
        }

        private static ValidatedRequest ValidateRequest(
            AuditionPvSixtySecondEvidenceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            AuditionPvCaptureManifest capture = request.captureCoreManifest ??
                throw new ArgumentException("Capture-core manifest is required.");
            if (!request.approvedSourceRange ||
                request.cleanPlate && !request.linkedCleanPlateConfirmed)
                throw new InvalidOperationException(
                    "High-cost evidence is restricted to an explicitly approved take range " +
                    "or a confirmed linked clean plate.");
            if (capture.width != AuditionPvSixtySecondGateManifestValidator.Width ||
                capture.height != AuditionPvSixtySecondGateManifestValidator.Height ||
                capture.fps != AuditionPvSixtySecondGateManifestValidator.Fps ||
                capture.sourceFormat != AuditionPvCaptureContract.SourceFormat)
                throw new InvalidDataException("Capture source contract is not QHD60 lossless PNG.");
            AuditionPvShotManifestEntry shot = (capture.shots ??
                Array.Empty<AuditionPvShotManifestEntry>()).SingleOrDefault(value =>
                value != null && string.Equals(value.id, request.sourceShotId,
                    StringComparison.Ordinal));
            if (shot == null) throw new InvalidDataException("Source shot is absent from capture core.");
            long rangeCount = (long)request.sourceRangeEndFrame -
                request.sourceRangeStartFrame + 1L;
            if (request.sourceRangeStartFrame < shot.startFrame ||
                request.sourceRangeEndFrame > shot.endFrame || rangeCount <= 0 ||
                rangeCount > AuditionPvRuntimeWorkloadCaptureSession.MaxRangeFrames ||
                request.selectStartFrame < request.sourceRangeStartFrame ||
                request.selectEndFrame > request.sourceRangeEndFrame ||
                request.selectEndFrame < request.selectStartFrame)
                throw new InvalidDataException("Evidence select/source range is invalid.");
            string captureCoreSha256 = AuditionPvSixtySecondGateManifestValidator
                .CaptureCoreSha256(capture);
            if (!AuditionPvSha256.IsSha256(captureCoreSha256) ||
                !string.IsNullOrWhiteSpace(request.expectedCaptureCoreSha256) &&
                request.expectedCaptureCoreSha256 != captureCoreSha256)
                throw new InvalidDataException("Capture-core SHA-256 identity drifted.");
            string captureDirectory = Full(capture.outputDirectory);
            string graphicsRoot = Full(request.graphicsRootDirectory);
            string reviewRoot = Full(request.reviewRootDirectory);
            if (!Directory.Exists(captureDirectory))
                throw new DirectoryNotFoundException(captureDirectory);
            RejectReparseChain(captureDirectory);
            RejectReparseChain(graphicsRoot);
            RejectReparseChain(reviewRoot);
            string rangeKey = RangeKey(request);
            string evidenceDirectory = Path.Combine(
                captureDirectory,
                "evidence",
                "sixty_second",
                SafeComponent(request.sourceShotId),
                rangeKey);
            string reviewDirectory = Path.Combine(
                reviewRoot,
                "evidence",
                SafeComponent(capture.captureId),
                SafeComponent(request.sourceShotId),
                rangeKey);
            RequireUnder(evidenceDirectory, captureDirectory, "capture evidence");
            RequireUnder(reviewDirectory, reviewRoot, "review evidence");
            if (Directory.Exists(evidenceDirectory) &&
                Directory.EnumerateFileSystemEntries(evidenceDirectory).Any())
                throw new IOException("Evidence bundle directory is immutable and already populated: " +
                                      evidenceDirectory);
            var validated = new ValidatedRequest
            {
                request = request,
                capture = capture,
                shot = shot,
                captureCoreSha256 = captureCoreSha256,
                captureDirectory = captureDirectory,
                graphicsRoot = graphicsRoot,
                reviewRoot = reviewRoot,
                evidenceDirectory = evidenceDirectory,
                reviewDirectory = reviewDirectory
            };
            ValidateRuntimeSealEnvelope(validated);
            return validated;
        }

        private static string RangeKey(AuditionPvSixtySecondEvidenceRequest request) =>
            "source_" + request.sourceRangeStartFrame.ToString("D4", CultureInfo.InvariantCulture) +
            "_" + request.sourceRangeEndFrame.ToString("D4", CultureInfo.InvariantCulture) +
            "__select_" + request.selectStartFrame.ToString("D4", CultureInfo.InvariantCulture) +
            "_" + request.selectEndFrame.ToString("D4", CultureInfo.InvariantCulture);

        private static List<PhysicalFrame> ProcessPhysicalFrames(
            ValidatedRequest value,
            string sourceLedgerPath)
        {
            int count = checked(value.request.sourceRangeEndFrame -
                                value.request.sourceRangeStartFrame + 1);
            var frames = new List<PhysicalFrame>(count);
            string rec709Directory = Path.Combine(
                value.graphicsRoot,
                "rec709",
                SafeComponent(value.capture.captureId),
                SafeComponent(value.shot.id));
            RequireUnder(rec709Directory, value.graphicsRoot, "Rec.709 output");
            Directory.CreateDirectory(rec709Directory);
            // Establish the shared whole-shot identity before any QHD decode/transform work.
            // Extra ledger frames are hash-streamed only; high-cost evidence remains range-bound.
            WriteOrVerifyWholeShotFrameLedger(
                value,
                sourceLedgerPath,
                Array.Empty<PhysicalFrame>());

            for (int sourceFrame = value.request.sourceRangeStartFrame;
                 sourceFrame <= value.request.sourceRangeEndFrame;
                 sourceFrame++)
            {
                string relative = CanonicalSourceRelative(value.shot.id, sourceFrame);
                string sourcePath = Path.Combine(
                    value.captureDirectory,
                    relative.Replace('/', Path.DirectorySeparatorChar));
                RequireUnder(sourcePath, value.captureDirectory, "source frame");
                RejectReparseChain(sourcePath);
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException(
                        "A selected source/handle frame is missing.", sourcePath);
                if (!AuditionPvSixtySecondGateManifestValidator.TryPngPreflight(
                        sourcePath,
                        MaxPngBytes,
                        out int width,
                        out int height) ||
                    width != AuditionPvSixtySecondGateManifestValidator.Width ||
                    height != AuditionPvSixtySecondGateManifestValidator.Height)
                    throw new InvalidDataException(
                        "Source frame is not a bounded decoded QHD PNG: " + sourcePath);
                string sourceSha256 = AuditionPvSha256.FileHash(sourcePath);
                string outputPath = Path.Combine(
                    rec709Directory,
                    $"frame_{sourceFrame:0000}.png");
                RequireUnder(outputPath, value.graphicsRoot, "Rec.709 output");

                long magenta;
                string expectedRawDigest;
                using (LoadedPng source = LoadedPng.OpenQhd(sourcePath))
                {
                    magenta = AuditionPvSixtySecondGateManifestValidator
                        .CountErrorMagentaPixels(source.pixels);
                    for (int index = 0; index < source.pixels.Length; index++)
                    {
                        Color32 pixel = source.pixels[index];
                        source.pixels[index] = new Color32(
                            AuditionPvSixtySecondGateManifestValidator
                                .TransformSrgb8ToRec709(pixel.r),
                            AuditionPvSixtySecondGateManifestValidator
                                .TransformSrgb8ToRec709(pixel.g),
                            AuditionPvSixtySecondGateManifestValidator
                                .TransformSrgb8ToRec709(pixel.b),
                            pixel.a);
                    }
                    expectedRawDigest = RawRgbaSha256(source.pixels);
                    source.texture.SetPixels32(source.pixels);
                    source.texture.Apply(false, false);
                    if (!File.Exists(outputPath))
                    {
                        byte[] encoded = ImageConversion.EncodeToPNG(source.texture);
                        if (encoded == null || encoded.LongLength <= 0 ||
                            encoded.LongLength > MaxPngBytes)
                            throw new InvalidDataException(
                                "Rec.709 PNG encoding exceeded its fixed byte budget.");
                        long peak = AuditionPvEvidenceMemoryContract.ConservativePeakBytes(
                            new FileInfo(sourcePath).Length,
                            encoded.LongLength);
                        if (peak > AuditionPvEvidenceMemoryContract.MaxTransientWorkingSetBytes)
                            throw new InvalidDataException(
                                "Rec.709 frame would exceed the transient memory contract.");
                        WriteBytesNew(outputPath, encoded);
                    }
                }

                RejectReparseChain(outputPath);
                if (!File.Exists(outputPath) ||
                    !AuditionPvSixtySecondGateManifestValidator.TryPngPreflight(
                        outputPath,
                        MaxPngBytes,
                        out int outputWidth,
                        out int outputHeight) ||
                    outputWidth != AuditionPvSixtySecondGateManifestValidator.Width ||
                    outputHeight != AuditionPvSixtySecondGateManifestValidator.Height)
                    throw new InvalidDataException("Rec.709 output is not a bounded QHD PNG.");
                using (LoadedPng output = LoadedPng.OpenQhd(outputPath))
                {
                    if (RawRgbaSha256(output.pixels) != expectedRawDigest)
                        throw new InvalidDataException(
                            "Rec.709 output pixels do not match the canonical LUT transform: " +
                            outputPath);
                }

                frames.Add(new PhysicalFrame
                {
                    sourceFrame = sourceFrame,
                    sourceRelativePath = relative,
                    sourcePath = Full(sourcePath).Replace('\\', '/'),
                    sourceSha256 = sourceSha256,
                    outputPath = Full(outputPath).Replace('\\', '/'),
                    outputSha256 = AuditionPvSha256.FileHash(outputPath),
                    errorMagentaPixelCount = magenta
                });
            }

            return frames;
        }

        private static void WriteOrVerifyWholeShotFrameLedger(
            ValidatedRequest value,
            string path,
            IEnumerable<PhysicalFrame> rangeFrames)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? value.captureDirectory);
            var known = (rangeFrames ?? Array.Empty<PhysicalFrame>())
                .ToDictionary(frame => frame.sourceFrame);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                // The hash/replace phase must run after FileShare.None has been released.
                {
                using var stream = new FileStream(
                    temporary,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    64 * 1024,
                    FileOptions.SequentialScan);
                using var writer = new StreamWriter(
                    stream,
                    new UTF8Encoding(false),
                    64 * 1024,
                    false)
                {
                    NewLine = "\n"
                };
                for (int sourceFrame = value.shot.startFrame;
                     sourceFrame <= value.shot.endFrame;
                     sourceFrame++)
                {
                    string relative = CanonicalSourceRelative(value.shot.id, sourceFrame);
                    string sourcePath = Path.Combine(value.captureDirectory,
                        relative.Replace('/', Path.DirectorySeparatorChar));
                    RequireUnder(sourcePath, value.captureDirectory, "source-shot ledger frame");
                    RejectReparseChain(sourcePath);
                    if (!File.Exists(sourcePath))
                        throw new FileNotFoundException(
                            "The canonical source-shot ledger cannot omit a frame.", sourcePath);
                    string sha256 = known.TryGetValue(sourceFrame, out PhysicalFrame existing)
                        ? existing.sourceSha256
                        : AuditionPvSha256.FileHash(sourcePath);
                    writer.WriteLine(sha256 + "  " + relative);
                }
                writer.Flush();
                }
                if (File.Exists(path))
                {
                    if (AuditionPvSha256.FileHash(path) != AuditionPvSha256.FileHash(temporary))
                        throw new InvalidDataException(
                            "Existing source-shot ledger bytes do not match current physical frames.");
                    File.Delete(temporary);
                }
                else File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static void CreateContactSheet(
            ValidatedRequest value,
            IReadOnlyList<PhysicalFrame> frames,
            int[] previewFrames,
            string outputPath)
        {
            previewFrames ??= Array.Empty<int>();
            if (previewFrames.Length == 0 || previewFrames.Length > MaxPreviewCells)
                throw new InvalidDataException("Contact-sheet cell count is outside the Gate limit.");
            int columns = Math.Min(4, previewFrames.Length);
            int rows = (previewFrames.Length + columns - 1) / columns;
            const int cellWidth = 2560 / 4;
            const int cellHeight = 1440 / 4;
            int sheetWidth = checked(columns * cellWidth);
            int sheetHeight = checked(rows * cellHeight);
            var sheetPixels = new Color32[checked(sheetWidth * sheetHeight)];
            long persistentBytes = checked((long)sheetPixels.Length * 4L);
            if (AuditionPvEvidenceMemoryContract.ConservativePeakBytes(
                    MaxPngBytes,
                    0,
                    persistentBytes) >
                AuditionPvEvidenceMemoryContract.MaxTransientWorkingSetBytes)
                throw new InvalidDataException("Contact sheet exceeds the memory contract.");

            for (int cell = 0; cell < previewFrames.Length; cell++)
            {
                PhysicalFrame frame = frames.Single(value =>
                    value.sourceFrame == previewFrames[cell]);
                using LoadedPng source = LoadedPng.OpenQhd(frame.sourcePath);
                int cellX = cell % columns;
                int cellY = cell / columns;
                for (int y = 0; y < cellHeight; y++)
                {
                    int sourceRow = y * 4 * AuditionPvSixtySecondGateManifestValidator.Width;
                    int targetRow = (cellY * cellHeight + y) * sheetWidth +
                                    cellX * cellWidth;
                    for (int x = 0; x < cellWidth; x++)
                        sheetPixels[targetRow + x] = source.pixels[sourceRow + x * 4];
                }
            }

            Texture2D sheet = null;
            try
            {
                sheet = new Texture2D(
                    sheetWidth,
                    sheetHeight,
                    TextureFormat.RGBA32,
                    false,
                    true);
                sheet.SetPixels32(sheetPixels);
                sheet.Apply(false, false);
                byte[] bytes = ImageConversion.EncodeToPNG(sheet);
                if (bytes == null || bytes.LongLength <= 0 || bytes.LongLength > MaxPngBytes)
                    throw new InvalidDataException("Contact-sheet PNG exceeds its byte limit.");
                WriteBytesNew(outputPath, bytes);
            }
            finally
            {
                if (sheet != null) UnityEngine.Object.DestroyImmediate(sheet);
            }

            string[] sourcePaths = previewFrames.Select(frame =>
                frames.Single(value => value.sourceFrame == frame).sourcePath).ToArray();
            if (!AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                    outputPath,
                    sourcePaths,
                    columns,
                    rows))
                throw new InvalidDataException(
                    "Generated contact sheet failed its physical quarter-scale comparison.");
        }

        private static string RawRgbaSha256(Color32[] pixels)
        {
            if (pixels == null || pixels.Length == 0)
                throw new ArgumentException("Decoded pixel buffer is empty.");
            using SHA256 sha = SHA256.Create();
            byte[] chunk = new byte[AuditionPvEvidenceMemoryContract.HashChunkBytes];
            int used = 0;
            foreach (Color32 pixel in pixels)
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
            sha.TransformFinalBlock(chunk, 0, used);
            return LowerHex(sha.Hash);
        }

        private sealed class LoadedPng : IDisposable
        {
            private readonly IDisposable lease;
            internal Texture2D texture;
            internal Color32[] pixels;

            private LoadedPng(IDisposable lease, Texture2D texture, Color32[] pixels)
            {
                this.lease = lease;
                this.texture = texture;
                this.pixels = pixels;
            }

            internal static LoadedPng OpenQhd(string path)
            {
                IDisposable lease = AuditionPvEvidenceMemoryContract.AcquireDecodedSourcePng();
                Texture2D texture = null;
                try
                {
                    var file = new FileInfo(path);
                    if (!file.Exists || file.Length <= 0 || file.Length > MaxPngBytes)
                        throw new InvalidDataException("PNG encoded bytes exceed the fixed limit.");
                    byte[] encoded = File.ReadAllBytes(path);
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                    if (!ImageConversion.LoadImage(texture, encoded, false) ||
                        texture.width != AuditionPvSixtySecondGateManifestValidator.Width ||
                        texture.height != AuditionPvSixtySecondGateManifestValidator.Height)
                        throw new InvalidDataException("PNG did not decode as QHD RGBA pixels.");
                    Color32[] pixels = texture.GetPixels32();
                    if (pixels.LongLength != AuditionPvEvidenceMemoryContract.QhdRgbaBytes / 4L)
                        throw new InvalidDataException("PNG decoded pixel cardinality drifted.");
                    return new LoadedPng(lease, texture, pixels);
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
                lease.Dispose();
            }
        }

        private static RuntimeFacts ReadAndValidateRuntimeSpool(
            ValidatedRequest value,
            IReadOnlyList<PhysicalFrame> physicalFrames)
        {
            string sealPath = Full(value.request.runtimeWorkloadSealPath);
            RequireUnder(sealPath, value.captureDirectory, "runtime workload seal");
            RejectReparseChain(sealPath);
            AuditionPvRuntimeWorkloadCaptureSeal seal = ReadJsonCapped<
                AuditionPvRuntimeWorkloadCaptureSeal>(sealPath, 1024 * 1024);
            int expectedCount = checked(value.request.sourceRangeEndFrame -
                                        value.request.sourceRangeStartFrame + 1);
            if (seal == null ||
                seal.schemaVersion != AuditionPvRuntimeWorkloadCaptureSession.SealSchema ||
                seal.captureId != value.capture.captureId ||
                seal.sourceShotId != value.shot.id ||
                !RuntimeSealCoversRange(seal, value.shot, value.request) ||
                seal.framesUtf8Bytes <= 0 ||
                seal.framesUtf8Bytes >
                AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes ||
                !RuntimeSealCompressionMetricsValid(seal) ||
                string.IsNullOrWhiteSpace(seal.tool) ||
                string.IsNullOrWhiteSpace(seal.toolVersion) ||
                !Utc(seal.completedAtUtc))
                throw new InvalidDataException("Runtime workload capture seal identity is invalid.");
            string framesPath = Full(seal.framesPath);
            RequireUnder(framesPath, value.captureDirectory, "runtime workload frames");
            RejectReparseChain(framesPath);
            var framesFile = new FileInfo(framesPath);
            if (!framesFile.Exists || framesFile.Length != seal.framesUtf8Bytes)
                throw new InvalidDataException("Runtime workload spool bytes drifted after capture.");
            if (value.request.cleanPlate)
            {
                if (seal.hudEvidenceMode !=
                        AuditionPvRuntimeWorkloadCaptureSession.HudAuthoredAndExcluded &&
                    seal.hudEvidenceMode !=
                        AuditionPvRuntimeWorkloadCaptureSession.SceneContractNoHud)
                    throw new InvalidDataException("Clean-plate HUD capture mode is invalid.");
                if (seal.hudEvidenceMode ==
                        AuditionPvRuntimeWorkloadCaptureSession.SceneContractNoHud &&
                    (seal.inspectedObjectCount <= 0 || seal.authoredHudComponentCount != 0))
                    throw new InvalidDataException("Scene no-HUD capture contract is incomplete.");
            }
            else if (!string.IsNullOrEmpty(seal.hudEvidenceMode))
                throw new InvalidDataException(
                    "A non-clean take cannot reuse clean-plate HUD workload identity.");

            var rendererEntries = new List<AuditionPvSelectedFrameScanEntry>(expectedCount);
            var hudEntries = value.request.cleanPlate
                ? new List<AuditionPvSelectedFrameScanEntry>(expectedCount)
                : null;
            long nulls = 0;
            long errors = 0;
            using var stream = OpenVerifiedRuntimeSpool(framesPath, seal);
            using var reader = new StreamReader(
                stream,
                new UTF8Encoding(false, true),
                false,
                64 * 1024,
                false);
            var runtimeValidationState =
                new AuditionPvSixtySecondGateManifestValidator
                    .RuntimeWorkloadValidationState();
            int sealedIndex = 0;
            int selectedIndex = 0;
            long observedMaxFrameLineUtf8Bytes = 0;
            int observedSnapshotFrameCount = 0;
            int observedDeltaFrameCount = 0;
            while (true)
            {
                string line = ReadRuntimeWorkloadLineCapped(reader);
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line))
                    throw new InvalidDataException("Runtime workload spool contains a blank row.");
                long lineBytes = Encoding.UTF8.GetByteCount(line) + 1L;
                if (lineBytes >
                    AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes)
                    throw new InvalidDataException("Runtime workload row exceeds its byte limit.");
                observedMaxFrameLineUtf8Bytes = Math.Max(
                    observedMaxFrameLineUtf8Bytes,
                    lineBytes);
                if (sealedIndex >= seal.frameCount)
                    throw new InvalidDataException("Runtime workload spool has excess rows.");
                AuditionPvRuntimeFrameWorkload frame =
                    JsonUtility.FromJson<AuditionPvRuntimeFrameWorkload>(line);
                int expectedSourceFrame = checked(
                    seal.sourceRangeStartFrame + sealedIndex);
                if (frame == null || frame.sourceFrame != expectedSourceFrame)
                    throw new InvalidDataException("Runtime workload source-frame order drifted.");
                if (RuntimeFrameHasFullInventorySnapshot(frame))
                    observedSnapshotFrameCount++;
                if (RuntimeFrameHasInventoryDelta(frame))
                    observedDeltaFrameCount++;

                var validationEntry = new AuditionPvSelectedFrameScanEntry
                {
                    sourceFrame = frame.sourceFrame,
                    inspectedRendererCount = frame.inspectedRendererCount,
                    inspectedMaterialSlotCount = frame.inspectedMaterialSlotCount,
                    rendererInventorySha256 = frame.rendererInventorySha256,
                    materialInventorySha256 = frame.materialInventorySha256,
                    nullMaterialCount = frame.nullMaterialCount,
                    errorMaterialCount = frame.errorMaterialCount,
                    inspectedCanvasCount = frame.inspectedCanvasCount,
                    inspectedHudRendererCount = frame.inspectedHudRendererCount,
                    inspectedDrawCommandCount = frame.inspectedDrawCommandCount,
                    visibleUiElementCount = frame.visibleUiElementCount,
                    canvasInventorySha256 = frame.canvasInventorySha256,
                    hudInventorySha256 = frame.hudInventorySha256,
                    rendererHudLayerExcluded = true
                };
                if (!AuditionPvSixtySecondGateManifestValidator
                        .RuntimeWorkloadFrameMatches(
                            "renderer-material-scan",
                            frame,
                            validationEntry,
                            string.Empty,
                            runtimeValidationState) ||
                    value.request.cleanPlate &&
                    !AuditionPvSixtySecondGateManifestValidator
                        .RuntimeWorkloadFrameMatches(
                            "hud-layer-absent",
                            frame,
                            validationEntry,
                            seal.hudEvidenceMode,
                            runtimeValidationState))
                    throw new InvalidDataException(
                        $"Runtime workload carry-forward inventory is invalid at " +
                        $"f{frame.sourceFrame}.");

                sealedIndex++;
                if (frame.nullMaterialCount != 0 || frame.errorMaterialCount != 0 ||
                    value.request.cleanPlate && frame.visibleUiElementCount != 0)
                    throw new InvalidDataException(
                        "The sealed capture-time workload contains a failing frame.");
                if (frame.sourceFrame < value.request.sourceRangeStartFrame ||
                    frame.sourceFrame > value.request.sourceRangeEndFrame)
                    continue;

                if (selectedIndex >= physicalFrames.Count)
                    throw new InvalidDataException(
                        "Runtime workload selected range has excess rows.");
                PhysicalFrame physical = physicalFrames[selectedIndex];
                if (frame.sourceFrame != physical.sourceFrame)
                    throw new InvalidDataException(
                        "Runtime workload selected range drifted from physical source frames.");

                var rendererEntry = new AuditionPvSelectedFrameScanEntry
                {
                    sourceFrame = frame.sourceFrame,
                    frameSha256 = physical.sourceSha256,
                    width = AuditionPvSixtySecondGateManifestValidator.Width,
                    height = AuditionPvSixtySecondGateManifestValidator.Height,
                    inspectedRendererCount = frame.inspectedRendererCount,
                    inspectedMaterialSlotCount = frame.inspectedMaterialSlotCount,
                    rendererInventorySha256 = frame.rendererInventorySha256,
                    materialInventorySha256 = frame.materialInventorySha256,
                    nullMaterialCount = frame.nullMaterialCount,
                    errorMaterialCount = frame.errorMaterialCount
                };
                rendererEntries.Add(rendererEntry);
                nulls = checked(nulls + frame.nullMaterialCount);
                errors = checked(errors + frame.errorMaterialCount);

                if (value.request.cleanPlate)
                {
                    var hudEntry = new AuditionPvSelectedFrameScanEntry
                    {
                        sourceFrame = frame.sourceFrame,
                        frameSha256 = physical.sourceSha256,
                        width = AuditionPvSixtySecondGateManifestValidator.Width,
                        height = AuditionPvSixtySecondGateManifestValidator.Height,
                        inspectedCanvasCount = frame.inspectedCanvasCount,
                        inspectedHudRendererCount = frame.inspectedHudRendererCount,
                        inspectedDrawCommandCount = frame.inspectedDrawCommandCount,
                        visibleUiElementCount = frame.visibleUiElementCount,
                        canvasInventorySha256 = frame.canvasInventorySha256,
                        hudInventorySha256 = frame.hudInventorySha256,
                        rendererHudLayerExcluded = true
                    };
                    hudEntries.Add(hudEntry);
                }
                selectedIndex++;
            }
            if (sealedIndex != seal.frameCount || selectedIndex != expectedCount ||
                selectedIndex != physicalFrames.Count || nulls != 0 || errors != 0)
                throw new InvalidDataException(
                    "Runtime renderer/material range is incomplete or contains error material slots.");
            if (observedMaxFrameLineUtf8Bytes != seal.maxFrameLineUtf8Bytes ||
                observedSnapshotFrameCount != seal.inventorySnapshotFrameCount ||
                observedDeltaFrameCount != seal.inventoryDeltaFrameCount)
                throw new InvalidDataException(
                    "Runtime workload compression metrics drifted from their capture-time seal.");
            reader.DiscardBufferedData();
            VerifyRuntimeSpoolHandle(stream, seal);

            return new RuntimeFacts
            {
                seal = seal,
                framesPath = framesPath,
                rendererEntries = rendererEntries.ToArray(),
                hudEntries = hudEntries?.ToArray() ??
                    Array.Empty<AuditionPvSelectedFrameScanEntry>(),
                nullMaterialCount = nulls,
                errorMaterialCount = errors
            };
        }

        private static void ValidateRuntimeSealEnvelope(ValidatedRequest value)
        {
            string sealPath = Full(value.request.runtimeWorkloadSealPath);
            RequireUnder(sealPath, value.captureDirectory, "runtime workload seal");
            RejectReparseChain(sealPath);
            AuditionPvRuntimeWorkloadCaptureSeal seal = ReadJsonCapped<
                AuditionPvRuntimeWorkloadCaptureSeal>(sealPath, 1024 * 1024);
            if (seal == null ||
                seal.schemaVersion != AuditionPvRuntimeWorkloadCaptureSession.SealSchema ||
                seal.captureId != value.capture.captureId ||
                seal.sourceShotId != value.shot.id ||
                !RuntimeSealCoversRange(seal, value.shot, value.request) ||
                !AuditionPvSha256.IsSha256(seal.framesSha256) ||
                seal.framesUtf8Bytes <= 0 ||
                seal.framesUtf8Bytes >
                AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes ||
                !RuntimeSealCompressionMetricsValid(seal))
                throw new InvalidDataException(
                    "Runtime workload capture seal is absent, partial, or range-mismatched.");
            string framesPath = Full(seal.framesPath);
            RequireUnder(framesPath, value.captureDirectory, "runtime workload frames");
            RejectReparseChain(framesPath);
            var file = new FileInfo(framesPath);
            if (!file.Exists || file.Length != seal.framesUtf8Bytes)
                throw new InvalidDataException(
                    "Runtime workload spool is not the exact capture-time sealed byte stream.");
            using FileStream verified = OpenVerifiedRuntimeSpool(framesPath, seal);
        }

        internal static bool RuntimeSealCoversRange(
            AuditionPvRuntimeWorkloadCaptureSeal seal,
            AuditionPvShotManifestEntry shot,
            AuditionPvSixtySecondEvidenceRequest request)
        {
            if (seal == null || shot == null || request == null) return false;
            long sealedCount = (long)seal.sourceRangeEndFrame -
                seal.sourceRangeStartFrame + 1L;
            return sealedCount > 0 &&
                sealedCount <= AuditionPvRuntimeWorkloadCaptureSession.MaxRangeFrames &&
                seal.sourceRangeStartFrame == shot.startFrame &&
                seal.sourceRangeEndFrame == shot.endFrame &&
                seal.frameCount == sealedCount &&
                request.sourceRangeStartFrame >= seal.sourceRangeStartFrame &&
                request.sourceRangeEndFrame <= seal.sourceRangeEndFrame;
        }

        private static bool RuntimeSealCompressionMetricsValid(
            AuditionPvRuntimeWorkloadCaptureSeal seal) =>
            seal != null && seal.maxFrameLineUtf8Bytes > 0 &&
            seal.maxFrameLineUtf8Bytes <=
            AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes &&
            seal.maxFrameLineUtf8Bytes <= seal.framesUtf8Bytes &&
            seal.inventorySnapshotFrameCount > 0 &&
            seal.inventorySnapshotFrameCount <= seal.frameCount &&
            seal.inventoryDeltaFrameCount >= 0 &&
            seal.inventoryDeltaFrameCount <= seal.frameCount;

        private static bool RuntimeFrameHasFullInventorySnapshot(
            AuditionPvRuntimeFrameWorkload frame) => frame != null &&
            ((frame.rendererStableIds?.Length ?? 0) > 0 ||
             (frame.materialSlotStableIds?.Length ?? 0) > 0 ||
             (frame.canvasStableIds?.Length ?? 0) > 0 ||
             (frame.hudRendererStableIds?.Length ?? 0) > 0);

        private static bool RuntimeFrameHasInventoryDelta(
            AuditionPvRuntimeFrameWorkload frame) => frame != null &&
            ((frame.rendererAddedStableIds?.Length ?? 0) > 0 ||
             (frame.rendererRemovedStableIds?.Length ?? 0) > 0 ||
             (frame.materialSlotAddedStableIds?.Length ?? 0) > 0 ||
             (frame.materialSlotRemovedStableIds?.Length ?? 0) > 0 ||
             (frame.canvasAddedStableIds?.Length ?? 0) > 0 ||
             (frame.canvasRemovedStableIds?.Length ?? 0) > 0 ||
             (frame.hudRendererAddedStableIds?.Length ?? 0) > 0 ||
             (frame.hudRendererRemovedStableIds?.Length ?? 0) > 0);

        internal static string ReadRuntimeWorkloadLineCapped(
            StreamReader reader,
            int maxLineUtf8Bytes =
                AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (maxLineUtf8Bytes <= 0 || maxLineUtf8Bytes >
                AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes)
                throw new ArgumentOutOfRangeException(nameof(maxLineUtf8Bytes));
            if (reader.Peek() < 0) return null;
            var builder = new StringBuilder(Math.Min(64 * 1024, maxLineUtf8Bytes));
            while (true)
            {
                int value = reader.Read();
                if (value < 0) break;
                char character = (char)value;
                if (character == '\n') break;
                if (character == '\r')
                {
                    if (reader.Peek() == '\n') reader.Read();
                    break;
                }
                builder.Append(character);
                if (builder.Length > maxLineUtf8Bytes)
                    throw new InvalidDataException(
                        "Runtime workload row exceeded its character allocation limit.");
            }
            string line = builder.ToString();
            if (Encoding.UTF8.GetByteCount(line) + 1 > maxLineUtf8Bytes)
                throw new InvalidDataException(
                    "Runtime workload row exceeded its UTF-8 byte limit.");
            return line;
        }

        private static FileStream OpenVerifiedRuntimeSpool(
            string path,
            AuditionPvRuntimeWorkloadCaptureSeal seal)
        {
            if (seal == null) throw new ArgumentNullException(nameof(seal));
            var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.SequentialScan);
            try
            {
                VerifyRuntimeSpoolHandle(stream, seal);
                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        private static void VerifyRuntimeSpoolHandle(
            FileStream stream,
            AuditionPvRuntimeWorkloadCaptureSeal seal)
        {
            if (stream == null || seal == null || !stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException("Runtime workload spool handle is invalid.");
            stream.Position = 0;
            if (stream.Length != seal.framesUtf8Bytes)
                throw new InvalidDataException(
                    "Runtime workload spool length drifted from its capture-time seal.");
            string sha256;
            using (SHA256 algorithm = SHA256.Create())
                sha256 = LowerHex(algorithm.ComputeHash(stream));
            if (!string.Equals(sha256, seal.framesSha256, StringComparison.Ordinal))
                throw new InvalidDataException(
                    "Runtime workload spool hash drifted from its capture-time seal.");
            stream.Position = 0;
        }

        private static void AddSimpleChecks(
            ValidatedRequest value,
            IReadOnlyList<PhysicalFrame> frames,
            AuditionPvMeasuredFrame[] previewMeasured,
            string contactSheetPath,
            string sourceLedgerSha256,
            string createdAtUtc,
            long duration,
            ICollection<AuditionPvAutomatedCheckEvidence> checks,
            ICollection<AuditionPvNamedPinnedArtifact> checkPins,
            ICollection<AuditionPvTestResult> tests)
        {
            int expected = checked(value.request.sourceRangeEndFrame -
                                   value.request.sourceRangeStartFrame + 1);
            int columns = Math.Min(4, previewMeasured.Length);
            int rows = (previewMeasured.Length + columns - 1) / columns;
            var contact = ResultBase("contact-sheet", value, sourceLedgerSha256, createdAtUtc);
            contact.sampledFrames = previewMeasured;
            contact.filmstripInputSha256 = previewMeasured.Select(frame => frame.frameSha256)
                .ToArray();
            contact.mediaArtifact = Pin(contactSheetPath);
            contact.mediaPurpose = "quarter-scale-contact-preview-only";
            contact.mediaColumns = columns;
            contact.mediaRows = rows;
            contact.measuredWidth = columns * (AuditionPvSixtySecondGateManifestValidator.Width / 4);
            contact.measuredHeight = rows * (AuditionPvSixtySecondGateManifestValidator.Height / 4);
            WriteCheckResult(value, contact, duration, checks, checkPins, tests);

            var missing = ResultBase("missing-frame", value, sourceLedgerSha256, createdAtUtc);
            missing.expectedFrameCount = expected;
            missing.observedFrameCount = frames.Count;
            WriteCheckResult(value, missing, duration, checks, checkPins, tests);

            var resolution = ResultBase("resolution", value, sourceLedgerSha256, createdAtUtc);
            resolution.inspectedFrameCount = frames.Count;
            resolution.measuredWidth = AuditionPvSixtySecondGateManifestValidator.Width;
            resolution.measuredHeight = AuditionPvSixtySecondGateManifestValidator.Height;
            WriteCheckResult(value, resolution, duration, checks, checkPins, tests);
        }

        private static void AddMagentaCheck(
            ValidatedRequest value,
            IReadOnlyList<PhysicalFrame> frames,
            AuditionPvMeasuredFrame[] previewMeasured,
            string sourceLedgerSha256,
            string createdAtUtc,
            long duration,
            ICollection<AuditionPvAutomatedCheckEvidence> checks,
            ICollection<AuditionPvNamedPinnedArtifact> checkPins,
            ICollection<AuditionPvTestResult> tests)
        {
            string checkId = "error-magenta";
            AuditionPvSelectedFrameScanConfigArtifact config = ScanConfig(
                checkId,
                MagentaAlgorithm,
                1,
                value,
                sourceLedgerSha256,
                createdAtUtc);
            string configPath = Path.Combine(value.evidenceDirectory,
                checkId + "_scan_config.json");
            WriteJsonNew(configPath, config, MaxJsonBytes);
            AuditionPvPinnedArtifact configPin = Pin(configPath);
            AuditionPvSelectedFrameScanEntry[] entries = frames.Select(frame =>
                new AuditionPvSelectedFrameScanEntry
                {
                    sourceFrame = frame.sourceFrame,
                    frameSha256 = frame.sourceSha256,
                    width = AuditionPvSixtySecondGateManifestValidator.Width,
                    height = AuditionPvSixtySecondGateManifestValidator.Height,
                    sampledPixelCount = AuditionPvEvidenceMemoryContract.QhdRgbaBytes / 4L,
                    errorMagentaPixelCount = frame.errorMagentaPixelCount
                }).ToArray();
            var ledger = BindRange(
                new AuditionPvSelectedFrameScanLedgerArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .FrameScanLedgerSchema,
                    checkId = checkId,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    sourceFrameLedgerSha256 = sourceLedgerSha256,
                    configSha256 = configPin.sha256,
                    frames = entries
                },
                value.request);
            string ledgerPath = Path.Combine(value.evidenceDirectory,
                checkId + "_scan_ledger.json");
            WriteJsonNew(ledgerPath, ledger, MaxJsonBytes);
            var result = ResultBase(checkId, value, sourceLedgerSha256, createdAtUtc);
            result.inspectedFrameCount = entries.LongLength;
            result.sampledPixelCount = entries.Sum(frame => frame.sampledPixelCount);
            result.detectedPixelCount = entries.Sum(frame => frame.errorMagentaPixelCount);
            result.sampledFrames = previewMeasured;
            result.scanConfig = configPin;
            result.scanLedger = Pin(ledgerPath);
            if (result.detectedPixelCount != 0 ||
                !AuditionPvSixtySecondGateManifestValidator.FullRangeScanTopologyValid(
                    checkId,
                    config,
                    ledger,
                    TakeRange(value.request)) ||
                !AuditionPvSixtySecondGateManifestValidator.FullScanAggregatesMatch(
                    checkId,
                    result,
                    ledger))
                throw new InvalidDataException(
                    "Full-range error-magenta physical scan did not pass.");
            WriteCheckResult(value, result, duration, checks, checkPins, tests);
        }

        private static Rec709EvidencePins AddRec709Check(
            ValidatedRequest value,
            IReadOnlyList<PhysicalFrame> frames,
            string sourceLedgerSha256,
            string createdAtUtc,
            long duration,
            ICollection<AuditionPvAutomatedCheckEvidence> checks,
            ICollection<AuditionPvNamedPinnedArtifact> checkPins,
            ICollection<AuditionPvTestResult> tests)
        {
            string configPath = Path.Combine(value.reviewDirectory, "rec709_transform.json");
            var config = BindRange(
                new AuditionPvRec709TransformArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .Rec709TransformSchema,
                    transformId = Rec709TransformId,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    sourceFrameLedgerSha256 = sourceLedgerSha256,
                    colorPrimaries = "bt709",
                    transferCharacteristics = "bt709",
                    matrixCoefficients = "identity-rgb",
                    signalRange = "full",
                    inputProfile = "iec-61966-2-1-srgb8",
                    outputProfile = "itu-r-bt709-oetf-rgba8",
                    roundingMode = "nearest-away-from-zero-u8",
                    alphaMode = "copy-exact",
                    editorialSourceRole = "canonical-approved-edit-original",
                    parserName = "unity-imageconversion-png-rgba8",
                    parserVersion = Application.unityVersion,
                    tool = nameof(AuditionPvSixtySecondEvidenceProducer),
                    toolVersion = ToolVersion,
                    createdAtUtc = createdAtUtc
                },
                value.request);
            WriteJsonNew(configPath, config, MaxJsonBytes);
            AuditionPvPinnedArtifact configPin = Pin(configPath);
            AuditionPvRec709OutputFrame[] outputFrames = frames.Select(frame =>
                new AuditionPvRec709OutputFrame
                {
                    sourceFrame = frame.sourceFrame,
                    sourceFrameSha256 = frame.sourceSha256,
                    outputPath = frame.outputPath,
                    outputSha256 = frame.outputSha256,
                    width = AuditionPvSixtySecondGateManifestValidator.Width,
                    height = AuditionPvSixtySecondGateManifestValidator.Height,
                    colorPrimaries = config.colorPrimaries,
                    transferCharacteristics = config.transferCharacteristics,
                    matrixCoefficients = config.matrixCoefficients,
                    signalRange = config.signalRange
                }).ToArray();
            var ledger = BindRange(
                new AuditionPvRec709OutputLedgerArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .Rec709OutputLedgerSchema,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    sourceFrameLedgerSha256 = sourceLedgerSha256,
                    configSha256 = configPin.sha256,
                    frames = outputFrames
                },
                value.request);
            string ledgerPath = Path.Combine(value.reviewDirectory, "rec709_output_ledger.json");
            WriteJsonNew(ledgerPath, ledger, MaxJsonBytes);
            if (!AuditionPvSixtySecondGateManifestValidator.Rec709OutputLedgerTopologyValid(
                    ledger,
                    TakeRange(value.request),
                    configPin.sha256,
                    config))
                throw new InvalidDataException("Rec.709 output ledger topology is invalid.");

            var result = ResultBase("rec709", value, sourceLedgerSha256, createdAtUtc);
            result.measuredWidth = AuditionPvSixtySecondGateManifestValidator.Width;
            result.measuredHeight = AuditionPvSixtySecondGateManifestValidator.Height;
            result.inspectedFrameCount = frames.Count;
            result.colorPrimaries = config.colorPrimaries;
            result.transferCharacteristics = config.transferCharacteristics;
            result.matrixCoefficients = config.matrixCoefficients;
            result.signalRange = config.signalRange;
            result.transformId = config.transformId;
            result.parserName = config.parserName;
            result.parserVersion = config.parserVersion;
            result.rec709Config = configPin;
            result.rec709OutputLedger = Pin(ledgerPath);
            if (!AuditionPvSixtySecondGateManifestValidator.Rec709EvidenceShapeValid(result))
                throw new InvalidDataException("Rec.709 result shape is invalid.");
            WriteCheckResult(value, result, duration, checks, checkPins, tests);
            return new Rec709EvidencePins
            {
                config = configPin,
                ledger = Pin(ledgerPath)
            };
        }

        private static RuntimeEvidencePins AddRuntimeCheck(
            ValidatedRequest value,
            RuntimeFacts runtime,
            IReadOnlyList<PhysicalFrame> physicalFrames,
            AuditionPvMeasuredFrame[] previewMeasured,
            string sourceLedgerSha256,
            string createdAtUtc,
            long duration,
            string checkId,
            ICollection<AuditionPvAutomatedCheckEvidence> checks,
            ICollection<AuditionPvNamedPinnedArtifact> checkPins,
            ICollection<AuditionPvTestResult> tests)
        {
            bool renderer = checkId == "renderer-material-scan";
            bool hud = checkId == "hud-layer-absent";
            if (!renderer && !hud) throw new ArgumentOutOfRangeException(nameof(checkId));
            string algorithm = renderer ? RendererAlgorithm : HudAlgorithm;
            AuditionPvSelectedFrameScanConfigArtifact config = ScanConfig(
                checkId,
                algorithm,
                0,
                value,
                sourceLedgerSha256,
                createdAtUtc);
            string configPath = Path.Combine(value.evidenceDirectory,
                checkId + "_scan_config.json");
            WriteJsonNew(configPath, config, MaxJsonBytes);
            AuditionPvPinnedArtifact configPin = Pin(configPath);
            AuditionPvSelectedFrameScanEntry[] entries = renderer
                ? runtime.rendererEntries
                : runtime.hudEntries;
            var ledger = BindRange(
                new AuditionPvSelectedFrameScanLedgerArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .FrameScanLedgerSchema,
                    checkId = checkId,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    sourceFrameLedgerSha256 = sourceLedgerSha256,
                    configSha256 = configPin.sha256,
                    frames = entries
                },
                value.request);
            string ledgerPath = Path.Combine(value.evidenceDirectory,
                checkId + "_scan_ledger.json");
            WriteJsonNew(ledgerPath, ledger, MaxJsonBytes);

            AuditionPvPinnedArtifact sceneNoHudPin = new();
            if (hud && runtime.seal.hudEvidenceMode ==
                AuditionPvRuntimeWorkloadCaptureSession.SceneContractNoHud)
            {
                string noHudPath = Path.Combine(value.evidenceDirectory,
                    "scene_no_hud_contract.json");
                var noHud = new AuditionPvSceneNoHudContractArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .SceneNoHudContractSchema,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    captureId = value.capture.captureId,
                    sourceShotId = value.shot.id,
                    noHudAuthored = true,
                    inspectedObjectCount = runtime.seal.inspectedObjectCount,
                    authoredHudComponentCount = runtime.seal.authoredHudComponentCount,
                    tool = runtime.seal.tool,
                    toolVersion = runtime.seal.toolVersion,
                    createdAtUtc = runtime.seal.completedAtUtc
                };
                WriteJsonNew(noHudPath, noHud, MaxJsonBytes);
                sceneNoHudPin = Pin(noHudPath);
                tests.Add(Passed(
                    "hud-layer-absent/scene-contract-no-hud",
                    duration,
                    sceneNoHudPin,
                    value,
                    "capture-time-scene-object-inventory=true",
                    noHudPath));
            }

            string workloadPath = Path.Combine(value.evidenceDirectory,
                checkId + "_runtime_workload.json");
            WriteRuntimeWorkloadArtifact(
                workloadPath,
                value,
                runtime,
                sourceLedgerSha256,
                configPin.sha256,
                checkId,
                sceneNoHudPin,
                createdAtUtc);
            AuditionPvPinnedArtifact workloadPin = Pin(workloadPath);
            tests.Add(Passed(
                checkId + "/runtime-workload",
                duration,
                workloadPin,
                value,
                "capture-time-frame-workload=true",
                workloadPath));

            var result = ResultBase(checkId, value, sourceLedgerSha256, createdAtUtc);
            result.inspectedFrameCount = entries.LongLength;
            result.sampledFrames = previewMeasured;
            result.scanConfig = configPin;
            result.scanLedger = Pin(ledgerPath);
            result.runtimeWorkload = workloadPin;
            if (renderer)
            {
                result.nullMaterialCount = entries.Sum(frame => frame.nullMaterialCount);
                result.errorMaterialCount = entries.Sum(frame => frame.errorMaterialCount);
            }
            else result.rendererHudLayerExcluded = true;
            if (!AuditionPvSixtySecondGateManifestValidator.FullRangeScanTopologyValid(
                    checkId,
                    config,
                    ledger,
                    TakeRange(value.request)) ||
                !AuditionPvSixtySecondGateManifestValidator.FullScanAggregatesMatch(
                    checkId,
                    result,
                    ledger))
                throw new InvalidDataException(checkId + " full-range workload topology is invalid.");
            WriteCheckResult(value, result, duration, checks, checkPins, tests);
            return new RuntimeEvidencePins
            {
                config = configPin,
                ledger = Pin(ledgerPath),
                workload = workloadPin
            };
        }

        private static void WriteRuntimeWorkloadArtifact(
            string outputPath,
            ValidatedRequest value,
            RuntimeFacts runtime,
            string sourceLedgerSha256,
            string scanConfigSha256,
            string checkId,
            AuditionPvPinnedArtifact sceneNoHudPin,
            string createdAtUtc)
        {
            var artifact = BindRange(
                new AuditionPvRuntimeWorkloadArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .RuntimeWorkloadSchema,
                    checkId = checkId,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    sourceFrameLedgerSha256 = sourceLedgerSha256,
                    scanConfigSha256 = scanConfigSha256,
                    tool = runtime.seal.tool,
                    toolVersion = runtime.seal.toolVersion,
                    inventoryIdentityContract = checkId == "renderer-material-scan"
                        ? "renderer-global-id/material-guid-local-id-sorted-v1"
                        : "canvas-global-id/hud-renderer-global-id-sorted-v1",
                    hudEvidenceMode = checkId == "hud-layer-absent"
                        ? runtime.seal.hudEvidenceMode
                        : string.Empty,
                    sceneNoHudContractProof = checkId == "hud-layer-absent"
                        ? sceneNoHudPin
                        : new AuditionPvPinnedArtifact(),
                    createdAtUtc = createdAtUtc,
                    frames = Array.Empty<AuditionPvRuntimeFrameWorkload>()
                },
                value.request);
            string shell = JsonUtility.ToJson(artifact, false);
            const string marker = "\"frames\":[]";
            int markerIndex = shell.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
                throw new InvalidDataException("Runtime workload JSON shell is not deterministic.");
            string prefix = shell.Substring(0, markerIndex) + "\"frames\":[";
            string suffix = shell.Substring(markerIndex + marker.Length);
            string temporary = outputPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                // Keep the using declarations inside a nested scope so FileShare.None is
                // released before the immutable artifact is atomically moved into place.
                {
                long written = 0;
                using var targetStream = new FileStream(
                    temporary,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    64 * 1024,
                    FileOptions.SequentialScan);
                using var writer = new StreamWriter(
                    targetStream,
                    new UTF8Encoding(false),
                    64 * 1024,
                    false)
                {
                    NewLine = "\n"
                };
                writer.Write(prefix);
                written += Encoding.UTF8.GetByteCount(prefix);
                using var sourceStream = OpenVerifiedRuntimeSpool(
                    runtime.framesPath,
                    runtime.seal);
                using var reader = new StreamReader(
                    sourceStream,
                    new UTF8Encoding(false, true),
                    false,
                    64 * 1024,
                    false);
                bool rendererArtifact = checkId == "renderer-material-scan";
                bool hudArtifact = checkId == "hud-layer-absent";
                var validationState =
                    new AuditionPvSixtySecondGateManifestValidator
                        .RuntimeWorkloadValidationState();
                int sealedIndex = 0;
                int writtenFrameCount = 0;
                while (true)
                {
                    string line = ReadRuntimeWorkloadLineCapped(reader);
                    if (line == null) break;
                    if (string.IsNullOrWhiteSpace(line))
                        throw new InvalidDataException(
                            "Runtime workload spool row is invalid while deriving a range artifact.");
                    AuditionPvRuntimeFrameWorkload frame =
                        JsonUtility.FromJson<AuditionPvRuntimeFrameWorkload>(line);
                    int expectedSourceFrame = checked(
                        runtime.seal.sourceRangeStartFrame + sealedIndex);
                    if (frame == null || frame.sourceFrame != expectedSourceFrame)
                        throw new InvalidDataException(
                            "Runtime workload spool order drifted while deriving a range artifact.");
                    var validationEntry = new AuditionPvSelectedFrameScanEntry
                    {
                        sourceFrame = frame.sourceFrame,
                        inspectedRendererCount = frame.inspectedRendererCount,
                        inspectedMaterialSlotCount = frame.inspectedMaterialSlotCount,
                        rendererInventorySha256 = frame.rendererInventorySha256,
                        materialInventorySha256 = frame.materialInventorySha256,
                        nullMaterialCount = frame.nullMaterialCount,
                        errorMaterialCount = frame.errorMaterialCount,
                        inspectedCanvasCount = frame.inspectedCanvasCount,
                        inspectedHudRendererCount = frame.inspectedHudRendererCount,
                        inspectedDrawCommandCount = frame.inspectedDrawCommandCount,
                        visibleUiElementCount = frame.visibleUiElementCount,
                        canvasInventorySha256 = frame.canvasInventorySha256,
                        hudInventorySha256 = frame.hudInventorySha256,
                        rendererHudLayerExcluded = true
                    };
                    if (!AuditionPvSixtySecondGateManifestValidator
                            .RuntimeWorkloadFrameMatches(
                                checkId,
                                frame,
                                validationEntry,
                                hudArtifact ? runtime.seal.hudEvidenceMode : string.Empty,
                                validationState))
                        throw new InvalidDataException(
                            "Runtime workload carry-forward row is invalid while deriving " +
                            "a range artifact.");
                    sealedIndex++;
                    if (frame.sourceFrame < value.request.sourceRangeStartFrame ||
                        frame.sourceFrame > value.request.sourceRangeEndFrame)
                        continue;
                    bool firstWrittenFrame = writtenFrameCount == 0;
                    if (rendererArtifact)
                    {
                        if (firstWrittenFrame)
                        {
                            frame.rendererStableIds = validationState.renderers.stableIds;
                            frame.materialSlotStableIds =
                                validationState.materialSlots.stableIds;
                            frame.rendererAddedStableIds = Array.Empty<string>();
                            frame.rendererRemovedStableIds = Array.Empty<string>();
                            frame.materialSlotAddedStableIds = Array.Empty<string>();
                            frame.materialSlotRemovedStableIds = Array.Empty<string>();
                        }
                        frame.canvasStableIds = Array.Empty<string>();
                        frame.hudRendererStableIds = Array.Empty<string>();
                        frame.canvasAddedStableIds = Array.Empty<string>();
                        frame.canvasRemovedStableIds = Array.Empty<string>();
                        frame.hudRendererAddedStableIds = Array.Empty<string>();
                        frame.hudRendererRemovedStableIds = Array.Empty<string>();
                    }
                    else
                    {
                        if (firstWrittenFrame)
                        {
                            frame.canvasStableIds = validationState.canvases.stableIds;
                            frame.hudRendererStableIds =
                                validationState.hudRenderers.stableIds;
                            frame.canvasAddedStableIds = Array.Empty<string>();
                            frame.canvasRemovedStableIds = Array.Empty<string>();
                            frame.hudRendererAddedStableIds = Array.Empty<string>();
                            frame.hudRendererRemovedStableIds = Array.Empty<string>();
                        }
                        frame.rendererStableIds = Array.Empty<string>();
                        frame.materialSlotStableIds = Array.Empty<string>();
                        frame.rendererAddedStableIds = Array.Empty<string>();
                        frame.rendererRemovedStableIds = Array.Empty<string>();
                        frame.materialSlotAddedStableIds = Array.Empty<string>();
                        frame.materialSlotRemovedStableIds = Array.Empty<string>();
                    }
                    string outputLine = JsonUtility.ToJson(frame, false);
                    int outputLineBytes = Encoding.UTF8.GetByteCount(outputLine);
                    if (outputLineBytes + 1 >
                        AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes)
                        throw new InvalidDataException(
                            "Runtime workload output row exceeds its byte limit.");
                    if (writtenFrameCount++ > 0)
                    {
                        writer.Write(',');
                        written++;
                    }
                    writer.Write(outputLine);
                    written += outputLineBytes;
                    if (written > MaxJsonBytes)
                        throw new InvalidDataException(
                            "Runtime workload artifact exceeds the Gate JSON byte limit.");
                }
                int expectedFrameCount = checked(
                    value.request.sourceRangeEndFrame -
                    value.request.sourceRangeStartFrame + 1);
                if (sealedIndex != runtime.seal.frameCount ||
                    writtenFrameCount != expectedFrameCount)
                    throw new InvalidDataException(
                        "Runtime workload range derivation was incomplete.");
                reader.DiscardBufferedData();
                VerifyRuntimeSpoolHandle(sourceStream, runtime.seal);
                writer.Write(']');
                writer.Write(suffix);
                writer.Write('\n');
                writer.Flush();
                if (targetStream.Length > MaxJsonBytes)
                    throw new InvalidDataException(
                        "Runtime workload artifact exceeds the Gate JSON byte limit.");
                }
                File.Move(temporary, outputPath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static AuditionPvSelectedFrameScanConfigArtifact ScanConfig(
            string checkId,
            string algorithm,
            int pixelStride,
            ValidatedRequest value,
            string sourceLedgerSha256,
            string createdAtUtc) => BindRange(
            new AuditionPvSelectedFrameScanConfigArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator
                    .FrameScanConfigSchema,
                checkId = checkId,
                captureId = value.capture.captureId,
                sourceCaptureCoreSha256 = value.captureCoreSha256,
                sourceShotId = value.shot.id,
                sourceFrameLedgerSha256 = sourceLedgerSha256,
                tool = nameof(AuditionPvSixtySecondEvidenceProducer),
                toolVersion = ToolVersion,
                algorithm = algorithm,
                algorithmSha256 = AuditionPvSha256.TextHash(algorithm),
                frameStride = 1,
                temporalPairStride = 0,
                pixelStride = pixelStride,
                createdAtUtc = createdAtUtc
            },
            value.request);

        private static AuditionPvAutomatedCheckResultArtifact ResultBase(
            string id,
            ValidatedRequest value,
            string sourceLedgerSha256,
            string createdAtUtc) => BindRange(
            new AuditionPvAutomatedCheckResultArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator
                    .AutomatedCheckResultSchema,
                id = id,
                captureId = value.capture.captureId,
                sourceCaptureCoreSha256 = value.captureCoreSha256,
                sourceShotId = value.shot.id,
                sourceFrameLedgerSha256 = sourceLedgerSha256,
                measurementTool = nameof(AuditionPvSixtySecondEvidenceProducer),
                measurementToolVersion = ToolVersion,
                measuredAtUtc = createdAtUtc
            },
            value.request);

        private static void WriteCheckResult(
            ValidatedRequest value,
            AuditionPvAutomatedCheckResultArtifact result,
            long duration,
            ICollection<AuditionPvAutomatedCheckEvidence> checks,
            ICollection<AuditionPvNamedPinnedArtifact> checkPins,
            ICollection<AuditionPvTestResult> tests)
        {
            string resultPath = Path.Combine(value.evidenceDirectory,
                result.id + "_result.json");
            WriteJsonNew(resultPath, result, MaxJsonBytes);
            AuditionPvPinnedArtifact pin = Pin(resultPath);
            checks.Add(new AuditionPvAutomatedCheckEvidence
            {
                id = result.id,
                status = "passed",
                supportingTestSuite = AutomatedTestSuite,
                supportingTestName = result.id,
                artifact = pin
            });
            checkPins.Add(new AuditionPvNamedPinnedArtifact
                { id = result.id, artifact = pin });
            tests.Add(Passed(
                result.id,
                duration,
                pin,
                value,
                "physical-measurement=true",
                resultPath));
        }

        private static AuditionPvTestResult Passed(
            string name,
            long duration,
            AuditionPvPinnedArtifact artifact,
            ValidatedRequest value,
            string facts,
            string artifactPath)
        {
            if (artifact == null || !AuditionPvSha256.IsSha256(artifact.sha256) ||
                !File.Exists(artifactPath) ||
                AuditionPvSha256.FileHash(artifactPath) != artifact.sha256)
                throw new InvalidDataException(
                    "A capture test cannot pass without exact physical artifact bytes.");
            return new AuditionPvTestResult
            {
                suite = AutomatedTestSuite,
                name = name,
                status = "passed",
                durationMilliseconds = duration,
                details = "artifact-sha256=" + artifact.sha256 +
                          "; capture-core-sha256=" + value.captureCoreSha256 +
                          "; source-shot=" + value.shot.id +
                          "; source-range=" + value.request.sourceRangeStartFrame + "-" +
                          value.request.sourceRangeEndFrame + "; " + facts,
                artifactPath = Full(artifactPath).Replace('\\', '/')
            };
        }

        private static AuditionPvPinnedArtifact WriteFilmstripSkeleton(
            ValidatedRequest value,
            AuditionPvMeasuredFrame[] previewMeasured,
            AuditionPvPinnedArtifact contactSheet,
            string createdAtUtc)
        {
            string path = Path.Combine(value.evidenceDirectory,
                "temporal_filmstrip_skeleton.json");
            var artifact = BindRange(
                new AuditionPvTemporalFilmstripSkeletonArtifact
                {
                    schemaVersion = FilmstripSkeletonSchema,
                    status = "preview-only-not-a-gate-scan",
                    previewOnly = true,
                    acceptedAsFullRangeScan = false,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    orderedFrames = previewMeasured,
                    contactSheet = contactSheet,
                    createdAtUtc = createdAtUtc
                },
                value.request);
            WriteJsonNew(path, artifact, MaxJsonBytes);
            return Pin(path);
        }

        private static AuditionPvPinnedArtifact WriteReviewSkeleton(
            ValidatedRequest value,
            IReadOnlyList<PhysicalFrame> physicalFrames,
            AuditionPvPinnedArtifact contactSheet,
            AuditionPvPinnedArtifact filmstrip,
            string createdAtUtc)
        {
            int[] proposedFrames = SampledFrames(
                value.request.sourceRangeStartFrame,
                value.request.sourceRangeEndFrame);
            string path = Path.Combine(value.reviewDirectory, "human_review_skeleton.json");
            var artifact = BindRange(
                new AuditionPvTakeReviewSkeletonArtifact
                {
                    schemaVersion = ReviewSkeletonSchema,
                    status = "human-review-required",
                    approved = false,
                    captureId = value.capture.captureId,
                    sourceCaptureCoreSha256 = value.captureCoreSha256,
                    sourceShotId = value.shot.id,
                    reviewedBy = string.Empty,
                    reviewedAtUtc = string.Empty,
                    requiredDecisions = new[]
                    {
                        "full-motion-range-reviewed",
                        "no-black-mesh",
                        "no-broken-trail",
                        "face-readability",
                        "attack-direction-readability",
                        "boss-silhouette-readability"
                    },
                    proposedFullRangeFrames = MeasuredFrames(
                        physicalFrames,
                        proposedFrames),
                    contactSheet = contactSheet,
                    filmstripSkeleton = filmstrip,
                    createdAtUtc = createdAtUtc
                },
                value.request);
            WriteJsonNew(path, artifact, MaxJsonBytes);
            return Pin(path);
        }

        private static AuditionPvMeasuredFrame[] MeasuredFrames(
            IReadOnlyList<PhysicalFrame> frames,
            IEnumerable<int> selected) => (selected ?? Array.Empty<int>())
            .Select(frame =>
            {
                PhysicalFrame physical = frames.Single(value => value.sourceFrame == frame);
                return new AuditionPvMeasuredFrame
                {
                    sourceFrame = frame,
                    frameSha256 = physical.sourceSha256
                };
            })
            .ToArray();

        private static int[] SampledFrames(int start, int end)
        {
            if (start < 0 || end < start) return Array.Empty<int>();
            var result = new List<int>();
            for (long frame = start;
                 frame <= end;
                 frame += AuditionPvSixtySecondGateManifestValidator.Fps)
                result.Add(checked((int)frame));
            if (result.Count == 0 || result[result.Count - 1] != end) result.Add(end);
            if (result.Count > MaxPreviewCells)
                throw new InvalidDataException(
                    "Selected range requires more contact cells than the Gate permits.");
            return result.ToArray();
        }

        private static AuditionPvSixtySecondTakeCandidate TakeRange(
            AuditionPvSixtySecondEvidenceRequest request) => new()
            {
                sourceRangeStartFrame = request.sourceRangeStartFrame,
                sourceRangeEndFrame = request.sourceRangeEndFrame,
                selectStartFrame = request.selectStartFrame,
                selectEndFrame = request.selectEndFrame
            };

        private static T BindRange<T>(
            T artifact,
            AuditionPvSixtySecondEvidenceRequest request)
            where T : AuditionPvRangeBoundArtifact
        {
            artifact.sourceRangeStartFrame = request.sourceRangeStartFrame;
            artifact.sourceRangeEndFrame = request.sourceRangeEndFrame;
            artifact.selectStartFrame = request.selectStartFrame;
            artifact.selectEndFrame = request.selectEndFrame;
            return artifact;
        }

        private static string CanonicalSourceRelative(string shotId, int frame) =>
            (shotId == AuditionPvStationTransitionGoldenCapture.ShotId
                ? AuditionPvStationTransitionGoldenCapture.FramesFolderName
                : "frames/" + SafeComponent(shotId)) + $"/frame_{frame:0000}.png";

        private static AuditionPvPinnedArtifact Pin(string path)
        {
            string full = Full(path).Replace('\\', '/');
            return new AuditionPvPinnedArtifact
            {
                path = full,
                sha256 = AuditionPvSha256.FileHash(full)
            };
        }

        private static void ValidateUniquePassedTests(
            IEnumerable<AuditionPvTestResult> tests)
        {
            var exact = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvTestResult test in tests ?? Array.Empty<AuditionPvTestResult>())
            {
                if (test == null || test.suite != AutomatedTestSuite ||
                    test.status != "passed" || string.IsNullOrWhiteSpace(test.name) ||
                    string.IsNullOrWhiteSpace(test.artifactPath) ||
                    !exact.Add(test.suite + "\0" + test.name + "\0" +
                               Full(test.artifactPath)))
                    throw new InvalidDataException(
                        "Evidence bundle contains an invalid or exact-duplicate passed test.");
                string hash = AuditionPvSha256.FileHash(test.artifactPath);
                if (!test.details.Contains("artifact-sha256=" + hash,
                        StringComparison.Ordinal))
                    throw new InvalidDataException(
                        "Passed capture test does not pin its physical artifact hash.");
            }
        }

        private static T ReadJsonCapped<T>(string path, long byteLimit)
        {
            var file = new FileInfo(path);
            if (!file.Exists || file.Length <= 0 || file.Length > byteLimit)
                throw new InvalidDataException("Evidence JSON exceeds its byte limit: " + path);
            byte[] bytes = File.ReadAllBytes(path);
            string json = new UTF8Encoding(false, true).GetString(bytes);
            if (json.Length > 0 && json[0] == '\ufeff') json = json.Substring(1);
            return JsonUtility.FromJson<T>(json);
        }

        private static void WriteJsonNew<T>(string path, T value, long byteLimit)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                      throw new InvalidDataException("Evidence path has no parent."));
            if (File.Exists(path)) throw new IOException("Evidence file already exists: " + path);
            string json = JsonUtility.ToJson(value, true) + "\n";
            int bytes = Encoding.UTF8.GetByteCount(json);
            if (bytes <= 0 || bytes > byteLimit)
                throw new InvalidDataException("Evidence JSON exceeds its byte limit: " + path);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(temporary, json, new UTF8Encoding(false));
                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static void WriteBytesNew(string path, byte[] bytes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                      throw new InvalidDataException("Evidence path has no parent."));
            if (File.Exists(path)) throw new IOException("Evidence file already exists: " + path);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(
                           temporary,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None,
                           64 * 1024,
                           FileOptions.SequentialScan))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static string LowerHex(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static bool Utc(string value) => DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTime parsed) && parsed.Kind == DateTimeKind.Utc;

        private static string SafeComponent(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "." || value == ".." ||
                value.IndexOfAny(new[] { '/', '\\', ':' }) >= 0)
                throw new InvalidDataException("Unsafe evidence path component.");
            return value;
        }

        private static string Full(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidDataException("Evidence path is empty.");
            return Path.GetFullPath(path);
        }

        private static void RequireUnder(string value, string root, string label)
        {
            string full = Full(value).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string parent = Full(root).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            if (string.Equals(full, parent, StringComparison.OrdinalIgnoreCase) ||
                full.StartsWith(parent + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase) ||
                full.StartsWith(parent + Path.AltDirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase)) return;
            throw new InvalidDataException(label + " escaped its explicit root: " + full);
        }

        private static void RejectReparseChain(string value)
        {
            string current = File.Exists(value) || Directory.Exists(value)
                ? value
                : Path.GetDirectoryName(value);
            while (!string.IsNullOrWhiteSpace(current))
            {
                if ((File.Exists(current) || Directory.Exists(current)) &&
                    (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException(
                        "Evidence paths cannot traverse reparse points: " + current);
                current = Path.GetDirectoryName(current);
            }
        }

        private sealed class ValidatedRequest
        {
            public AuditionPvSixtySecondEvidenceRequest request;
            public AuditionPvCaptureManifest capture;
            public AuditionPvShotManifestEntry shot;
            public string captureCoreSha256;
            public string captureDirectory;
            public string graphicsRoot;
            public string reviewRoot;
            public string evidenceDirectory;
            public string reviewDirectory;
        }

        private sealed class PhysicalFrame
        {
            public int sourceFrame;
            public string sourceRelativePath;
            public string sourcePath;
            public string sourceSha256;
            public string outputPath;
            public string outputSha256;
            public long errorMagentaPixelCount;
        }

        private sealed class RuntimeFacts
        {
            public AuditionPvRuntimeWorkloadCaptureSeal seal;
            public string framesPath;
            public AuditionPvSelectedFrameScanEntry[] rendererEntries;
            public AuditionPvSelectedFrameScanEntry[] hudEntries;
            public long nullMaterialCount;
            public long errorMaterialCount;
        }

        private sealed class Rec709EvidencePins
        {
            public AuditionPvPinnedArtifact config = new();
            public AuditionPvPinnedArtifact ledger = new();
        }

        private sealed class RuntimeEvidencePins
        {
            public AuditionPvPinnedArtifact config = new();
            public AuditionPvPinnedArtifact ledger = new();
            public AuditionPvPinnedArtifact workload = new();
        }
    }

    internal sealed class AuditionPvSixtySecondEvidenceRequest
    {
        public AuditionPvCaptureManifest captureCoreManifest;
        public string expectedCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceRangeStartFrame;
        public int sourceRangeEndFrame;
        public int selectStartFrame;
        public int selectEndFrame;
        public string runtimeWorkloadSealPath = string.Empty;
        public string graphicsRootDirectory = string.Empty;
        public string reviewRootDirectory = string.Empty;
        public bool cleanPlate;
        public bool approvedSourceRange;
        public bool linkedCleanPlateConfirmed;
    }

    internal sealed class AuditionPvSixtySecondEvidenceBundle
    {
        public AuditionPvPinnedArtifact receipt = new();
        public AuditionPvPinnedArtifact sourceFrameLedger = new();
        public AuditionPvPinnedArtifact automatedProof = new();
        public AuditionPvPinnedArtifact contactSheet = new();
        public AuditionPvPinnedArtifact filmstripSkeleton = new();
        public AuditionPvPinnedArtifact humanReviewSkeleton = new();
        public AuditionPvPinnedArtifact rec709Config = new();
        public AuditionPvPinnedArtifact rec709OutputLedger = new();
        public AuditionPvPinnedArtifact rendererRuntimeWorkload = new();
        public AuditionPvPinnedArtifact hudRuntimeWorkload = new();
        public AuditionPvTestResult[] testResults = Array.Empty<AuditionPvTestResult>();
    }

    internal static class AuditionPvEvidenceMemoryContract
    {
        internal const int MaxSimultaneousDecodedSourcePngs = 1;
        internal const long MaxTransientWorkingSetBytes = 128L * 1024L * 1024L;
        internal const long QhdRgbaBytes = 2560L * 1440L * 4L;
        internal const int HashChunkBytes = 64 * 1024;
        private static readonly object Sync = new();
        private static int activeSourcePngs;

        internal static IDisposable AcquireDecodedSourcePng()
        {
            lock (Sync)
            {
                if (activeSourcePngs >= MaxSimultaneousDecodedSourcePngs)
                    throw new InvalidOperationException(
                        "Evidence memory contract permits only one decoded source PNG at a time.");
                activeSourcePngs++;
            }
            return new SourcePngLease();
        }

        internal static long ConservativePeakBytes(long encodedInputBytes,
            long encodedOutputBytes, long persistentPreviewBytes = 0)
        {
            if (encodedInputBytes < 0 || encodedOutputBytes < 0 ||
                persistentPreviewBytes < 0)
                throw new ArgumentOutOfRangeException();
            return checked(encodedInputBytes + encodedOutputBytes +
                           QhdRgbaBytes * 2L + persistentPreviewBytes +
                           HashChunkBytes);
        }

        private sealed class SourcePngLease : IDisposable
        {
            private bool disposed;
            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                lock (Sync) activeSourcePngs--;
            }
        }
    }

    [Serializable]
    internal sealed class AuditionPvNamedPinnedArtifact
    {
        public string id = string.Empty;
        public AuditionPvPinnedArtifact artifact = new();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondEvidenceBundleReceipt :
        AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty;
        public string status = string.Empty;
        public string captureId = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty;
        public AuditionPvPinnedArtifact sourceFrameLedger = new();
        public AuditionPvPinnedArtifact automatedProof = new();
        public AuditionPvNamedPinnedArtifact[] checkResults =
            Array.Empty<AuditionPvNamedPinnedArtifact>();
        public AuditionPvPinnedArtifact contactSheet = new();
        public AuditionPvPinnedArtifact filmstripSkeleton = new();
        public AuditionPvPinnedArtifact humanReviewSkeleton = new();
        public AuditionPvPinnedArtifact rec709Config = new();
        public AuditionPvPinnedArtifact rec709OutputLedger = new();
        public AuditionPvPinnedArtifact rendererRuntimeWorkload = new();
        public AuditionPvPinnedArtifact hudRuntimeWorkload = new();
        public AuditionPvTestResult[] generatedTestResults =
            Array.Empty<AuditionPvTestResult>();
        public int maxSimultaneousDecodedSourcePngs;
        public long maxTransientWorkingSetBytes;
        public string producer = string.Empty;
        public string producerVersion = string.Empty;
        public string createdAtUtc = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTemporalFilmstripSkeletonArtifact :
        AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty;
        public string status = string.Empty;
        public bool previewOnly;
        public bool acceptedAsFullRangeScan;
        public string captureId = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty;
        public AuditionPvMeasuredFrame[] orderedFrames = Array.Empty<AuditionPvMeasuredFrame>();
        public AuditionPvPinnedArtifact contactSheet = new();
        public string createdAtUtc = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvTakeReviewSkeletonArtifact :
        AuditionPvRangeBoundArtifact
    {
        public string schemaVersion = string.Empty;
        public string status = string.Empty;
        public bool approved;
        public string captureId = string.Empty;
        public string sourceCaptureCoreSha256 = string.Empty;
        public string sourceShotId = string.Empty;
        public string reviewedBy = string.Empty;
        public string reviewedAtUtc = string.Empty;
        public string[] requiredDecisions = Array.Empty<string>();
        public AuditionPvMeasuredFrame[] proposedFullRangeFrames =
            Array.Empty<AuditionPvMeasuredFrame>();
        public AuditionPvPinnedArtifact contactSheet = new();
        public AuditionPvPinnedArtifact filmstripSkeleton = new();
        public string createdAtUtc = string.Empty;
    }
}
