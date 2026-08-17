using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Deterministic, fail-closed bridge from immutable golden captures to the
    /// 60-second Gate manifest. Inventory generation is deliberately separate
    /// from final-manifest admission: incomplete work can only produce an
    /// inventory, never a plausible-looking Gate input.
    /// </summary>
    internal static class AuditionPvSixtySecondProductionComposer
    {
        internal const string InventorySchema =
            "dimension-brawl.audition-pv.preedit-60s-production-inventory.v1";
        internal const string InputSchema =
            "dimension-brawl.audition-pv.preedit-60s-production-compose-input.v1";
        internal const string InventoryFileName =
            "preedit_60s_production_inventory.json";
        internal const string BatchInputArgument = "-pv60AssemblyInput=";
        internal const int ExpectedCaptureRunCount = 19;
        private const long MaximumJsonBytes = 16L * 1024L * 1024L;
        private const string GateSuite = "AuditionPvSixtySecondEvidence";

        private static readonly FamilySpec[] Families =
        {
            new FamilySpec("city-g01-g03", 3, "g01", "g02", "g03"),
            new FamilySpec("city-s030", 3, "s030"),
            new FamilySpec("station-s050", 1, "s050"),
            new FamilySpec("station-g04", 3, "g04", "g04-clean"),
            new FamilySpec("station-g06", 3, "g06"),
            new FamilySpec("station-g07", 3, "g07"),
            new FamilySpec("station-g08", 3, "g08")
        };

        // Inclusive source coordinates are intentionally explicit. Every row
        // retains 180 real frames before and after its selected range.
        private static readonly EdlSpec[] DefaultEdl =
        {
            // G01 proves the authored skyline/alert composition. The anomaly is
            // the first second of G03, before the rift transition proper. Keep
            // the two physical sources explicit instead of attributing a portal
            // beat to G01 that its runtime proof never observed.
            Edl("PV_S010", "pv-s010-city-alert-skyline", 0, 179,
                "city-g01-g03", "g01", 180, 359, "cinematic", "hud-off", false,
                "city-alert", "city-skyline"),
            Edl("PV_S010", "pv-s010-dimensional-anomaly", 180, 239,
                "city-g01-g03", "g03", 180, 239, "cinematic", "hud-off", false,
                "dimensional-anomaly"),
            Edl("PV_S020", "pv-s020-city-gameplay", 240, 599,
                "city-g01-g03", "g02", 240, 599, "gameplay", "hud-on", true,
                "city-movement", "city-fire", "city-hud-gameplay"),
            Edl("PV_S030", "pv-s030-hit-dodge-summon", 600, 959,
                "city-s030", "s030", 180, 539, "gameplay", "hud-on", true,
                "player-hit", "perfect-dodge", "summon-chain"),
            Edl("PV_S040", "pv-s040-dimension-rift", 960, 1199,
                "city-g01-g03", "g03", 240, 479, "cinematic", "hud-off", false,
                "dimension-rift-transition"),
            Edl("PV_S050", "pv-s050-boss-low-angle", 1200, 1439,
                "station-s050", "s050", 180, 419, "cinematic", "hud-off", false,
                "boss-low-angle", "boss-silhouette"),

            // G04 has 238 authored logical frames, while S060 is 240 frames.
            // Two semantic atomic shots cover 120 frames each and overlap the
            // authored source boundary by exactly two frames. No freeze-frame,
            // retime, or fabricated source duration is represented in the Gate.
            Edl("PV_S060", "pv-s060-wing-deployment", 1440, 1559,
                "station-g04", "g04", 180, 299, "cinematic", "hud-off", true,
                "c33-wing-deployment"),
            EdlWithCleanPlate("PV_S060", "pv-s060-eye-open", 1560, 1679,
                "station-g04", "g04", 298, 417, "cinematic", "hud-off", true,
                "g04-clean", "c34-eye-open"),

            Edl("PV_S070", "pv-s070-pattern-one", 1680, 1979,
                "station-g06", "g06", 180, 479, "gameplay", "hud-on", true,
                "boss-pattern-1", "olympus-hud-gameplay"),
            Edl("PV_S070", "pv-s070-patterns-two-three", 1980, 2399,
                "station-g07", "g07", 180, 599, "gameplay", "hud-on", true,
                "boss-pattern-2", "boss-pattern-3"),

            // G06 is the only captured product source proving all S080 beats.
            // Its two 300-frame atomic selects overlap by 240 source frames;
            // the overlap is explicit rather than hidden as a 600-frame take.
            Edl("PV_S080", "pv-s080-dodge-summon-defense", 2400, 2699,
                "station-g06", "g06", 180, 479, "gameplay", "hud-on", true,
                "perfect-dodge", "summon-defense"),
            Edl("PV_S080", "pv-s080-tier3-ultimate", 2700, 2999,
                "station-g06", "g06", 240, 539, "gameplay", "hud-on", true,
                "player-tier3-ultimate"),
            Edl("PV_S090", "pv-s090-finisher-aftermath", 3000, 3299,
                "station-g08", "g08", 240, 539, "gameplay", "mixed", true,
                "boss-finisher", "boss-collapse", "aftermath"),
            EndCardEdl("PV_S100", "pv-s100-end-card", 3300, 3599,
                "logo", "slogan", "audition-end-card")
        };

        internal static AuditionPvSixtySecondProductionComposition BuildPartialInventory(
            AuditionPvSixtySecondProductionComposeInput input)
        {
            return Build(input, AuditionPvCaptureContract.OutputRoot,
                requestManifest: false, hermeticTestSeam: false);
        }

        internal static AuditionPvSixtySecondProductionComposition ComposeProduction(
            AuditionPvSixtySecondProductionComposeInput input)
        {
            return Build(input, AuditionPvCaptureContract.OutputRoot,
                requestManifest: true, hermeticTestSeam: false);
        }

        // This seam can exercise deterministic assembly with temporary files,
        // but it skips production evidence reads and can never write or validate
        // the authoritative canonical manifest.
        internal static AuditionPvSixtySecondProductionComposition ComposeForTests(
            AuditionPvSixtySecondProductionComposeInput input,
            string captureRoot)
        {
            return Build(input, captureRoot, requestManifest: true,
                hermeticTestSeam: true);
        }

        internal static AuditionPvSixtySecondProductionComposition BuildInventoryForTests(
            AuditionPvSixtySecondProductionComposeInput input,
            string captureRoot)
        {
            return Build(input, captureRoot, requestManifest: false,
                hermeticTestSeam: true);
        }

        internal static AuditionPvSixtySecondProductionEdlRow[] CreateDefaultEdlForTests()
        {
            return DefaultEdl.Select(ToInventoryRow).ToArray();
        }

        /// <summary>
        /// Writes the refreshable inventory every time. The Gate manifest is
        /// installed only after the full production validator reports zero
        /// errors with installed roots and a clean current checkpoint.
        /// </summary>
        internal static AuditionPvSixtySecondProductionWriteResult ComposeAndWriteProduction(
            AuditionPvSixtySecondProductionComposeInput input)
        {
            AuditionPvSixtySecondProductionComposition composition =
                ComposeProduction(input);
            string inventoryPath = Path.Combine(
                AuditionPvSixtySecondGateManifestValidator.ProductionManifestRoot,
                InventoryFileName);
            WriteReplace(inventoryPath, composition.inventory);

            var result = new AuditionPvSixtySecondProductionWriteResult
            {
                inventoryPath = NormalizePath(inventoryPath),
                finalManifestReady = composition.finalManifestReady,
                missingRequirements = composition.inventory?.missingRequirements ??
                    Array.Empty<string>()
            };
            if (!composition.finalManifestReady || composition.manifest == null)
                return result;

            string manifestPath = CanonicalManifestPath();
            byte[] desired = JsonBytes(composition.manifest);
            string wanted = BytesHash(desired);
            bool installedByThisCall = false;
            if (File.Exists(manifestPath))
            {
                string existing = AuditionPvSha256.FileHash(manifestPath);
                if (!string.Equals(existing, wanted, StringComparison.Ordinal))
                    throw new IOException(
                        "The canonical 60-second Gate manifest already exists with different bytes.");
            }
            else
            {
                WriteNew(manifestPath, desired);
                installedByThisCall = true;
            }

            try
            {
                result.manifestPath = NormalizePath(manifestPath);
                result.validation = AuditionPvSixtySecondGateManifestValidator
                    .ValidateProductionFile(manifestPath);
                result.authoritativePassed = result.validation.passed;
                if (!result.authoritativePassed && installedByThisCall)
                {
                    DeleteFailedInstallIfUnchanged(manifestPath, wanted);
                    result.manifestPath = string.Empty;
                }
                return result;
            }
            catch
            {
                if (installedByThisCall)
                    DeleteFailedInstallIfUnchanged(manifestPath, wanted);
                throw;
            }
        }

        public static void RunBatchCompose()
        {
            try
            {
                string inputPath = ReadArgument(BatchInputArgument);
                if (string.IsNullOrWhiteSpace(inputPath))
                    throw new ArgumentException(
                        "RunBatchCompose requires -pv60AssemblyInput=<absolute-json-path>.");
                AuditionPvSixtySecondProductionComposeInput input =
                    ReadJson<AuditionPvSixtySecondProductionComposeInput>(inputPath);
                AuditionPvSixtySecondProductionWriteResult result =
                    ComposeAndWriteProduction(input);
                if (!result.authoritativePassed)
                {
                    Debug.LogError(
                        "[AuditionPV] 60-second Gate remains closed. Inventory: "
                        + result.inventoryPath + " | missing="
                        + string.Join(", ", result.missingRequirements ??
                            Array.Empty<string>()));
                    EditorApplication.Exit(2);
                    return;
                }
                Debug.Log("[AuditionPV] Authoritative 60-second Gate PASS: "
                    + result.manifestPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        internal static void AssertAuthoritativeDestinationForTests(string path)
        {
            if (!PathsEqual(path, CanonicalManifestPath()))
                throw new InvalidOperationException(
                    "Authoritative output is restricted to the canonical PREEDIT_60S manifest.");
        }

        private static AuditionPvSixtySecondProductionComposition Build(
            AuditionPvSixtySecondProductionComposeInput input,
            string captureRoot,
            bool requestManifest,
            bool hermeticTestSeam)
        {
            input ??= new AuditionPvSixtySecondProductionComposeInput();
            captureRoot = Path.GetFullPath(captureRoot ?? string.Empty);
            var missing = new SortedSet<string>(StringComparer.Ordinal);
            if (input.schemaVersion != InputSchema)
                missing.Add("COMPOSE_INPUT_SCHEMA_INVALID");

            List<LoadedSource> loaded = LoadSources(
                input.captureManifestPaths, captureRoot, missing);
            Dictionary<string, List<LoadedSource>> byFamily = Families.ToDictionary(
                family => family.id,
                family => loaded.Where(source => source.valid && source.familyId == family.id)
                    .OrderBy(source => source.capture.captureId, StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);

            foreach (FamilySpec family in Families)
            {
                int actual = byFamily[family.id].Count;
                if (actual != family.requiredRuns)
                    missing.Add("CAPTURE_FAMILY_COUNT:" + family.id
                        + ":expected=" + family.requiredRuns.ToString(CultureInfo.InvariantCulture)
                        + ":actual=" + actual.ToString(CultureInfo.InvariantCulture));
            }
            if (loaded.Count(source => source.valid) != ExpectedCaptureRunCount)
                missing.Add("CAPTURE_RUN_COUNT:expected=19:actual="
                    + loaded.Count(source => source.valid).ToString(CultureInfo.InvariantCulture));

            Dictionary<string, AuditionPvSixtySecondTakeEvidenceBinding> bindings =
                IndexTakeBindings(input.takeEvidence, missing);
            Dictionary<string, AuditionPvSixtySecondShotReferenceBinding> references =
                IndexShotReferences(input.shotReferences, missing);

            // Build a non-exported draft even for inventory-only calls so the
            // inventory reports approval, clean-plate, audio, rights, review,
            // and reference gaps instead of merely counting capture folders.
            AuditionPvSixtySecondShotGateManifest manifest =
                BuildManifest(input, byFamily, bindings, references, missing);
            if (manifest != null)
            {
                AuditionPvSixtySecondGateValidationReport structure =
                    AuditionPvSixtySecondGateManifestValidator.ValidateStructure(manifest);
                foreach (AuditionPvSixtySecondGateIssue issue in structure.issues ??
                             Array.Empty<AuditionPvSixtySecondGateIssue>())
                {
                    if (issue != null && issue.severity == "error")
                        missing.Add("STRUCTURE:" + issue.code + "@" + issue.location);
                }
            }

            if (requestManifest && manifest != null && missing.Count == 0 && !hermeticTestSeam)
            {
                AuditionPvSixtySecondGateValidationReport production =
                    AuditionPvSixtySecondGateManifestValidator.ValidateProduction(
                        manifest, CreateInstalledContext());
                foreach (AuditionPvSixtySecondGateIssue issue in production.issues ??
                             Array.Empty<AuditionPvSixtySecondGateIssue>())
                {
                    if (issue != null && issue.severity == "error")
                        missing.Add("PRODUCTION:" + issue.code + "@" + issue.location);
                }
            }

            bool structureComplete = requestManifest && manifest != null && missing.Count == 0;
            bool ready = structureComplete && !hermeticTestSeam;
            // Hermetic assembly may return its structure for assertions, but it
            // must never advertise final-manifest readiness or authoritative
            // eligibility. Incomplete production assembly returns no manifest.
            if (!structureComplete) manifest = null;
            AuditionPvSixtySecondProductionInventory inventory = CreateInventory(
                loaded, byFamily, missing, ready, structureComplete, hermeticTestSeam);
            return new AuditionPvSixtySecondProductionComposition
            {
                inventory = inventory,
                manifest = manifest,
                finalManifestReady = ready,
                hermeticTestSeam = hermeticTestSeam
            };
        }

        private static AuditionPvSixtySecondShotGateManifest BuildManifest(
            AuditionPvSixtySecondProductionComposeInput input,
            IReadOnlyDictionary<string, List<LoadedSource>> byFamily,
            IReadOnlyDictionary<string, AuditionPvSixtySecondTakeEvidenceBinding> bindings,
            IReadOnlyDictionary<string, AuditionPvSixtySecondShotReferenceBinding> references,
            ISet<string> missing)
        {
            if (!IsFullGitSha(input.productCheckpointGitSha))
                missing.Add("PRODUCT_CHECKPOINT_GIT_SHA_INVALID");
            RequireNonEmpty(input.audio, "AUDIO_ROWS_MISSING", missing);
            RequireNonEmpty(input.rights, "RIGHTS_ROWS_MISSING", missing);
            RequireNonEmpty(input.usedItems, "USED_ITEM_ROWS_MISSING", missing);
            RequireGateEvidence(input.gateEvidence, missing);
            RequirePin(input.endCardGraphic, "END_CARD_GRAPHIC", missing);

            AuditionPvSixtySecondShotGateManifest manifest =
                AuditionPvSixtySecondGateManifestValidator.CreateEmptyPlan();
            manifest.declaredStatus = "ready-for-editing";
            manifest.productCheckpointGitSha = input.productCheckpointGitSha ?? string.Empty;
            manifest.audio = input.audio ?? Array.Empty<AuditionPvSixtySecondAudioEvidence>();
            manifest.rights = input.rights ?? Array.Empty<AuditionPvSixtySecondRightsEvidence>();
            manifest.usedItems = input.usedItems ?? Array.Empty<AuditionPvSixtySecondUsedItem>();
            manifest.gateEvidence = input.gateEvidence ?? new AuditionPvSixtySecondGateEvidence();

            foreach (AuditionPvSixtySecondSequenceBucket bucket in manifest.buckets)
            {
                EdlSpec[] rows = DefaultEdl.Where(value => value.bucketId == bucket.bucketId)
                    .ToArray();
                var shots = new List<AuditionPvSixtySecondAtomicShot>(rows.Length);
                foreach (EdlSpec row in rows)
                {
                    if (!references.TryGetValue(row.atomicShotId,
                            out AuditionPvSixtySecondShotReferenceBinding shotRefs))
                    {
                        missing.Add("SHOT_REFERENCE_MISSING:" + row.atomicShotId);
                        shotRefs = new AuditionPvSixtySecondShotReferenceBinding
                            { atomicShotId = row.atomicShotId };
                    }
                    shots.Add(row.endCard
                        ? BuildEndCardShot(row, input.endCardGraphic, shotRefs)
                        : BuildSourceShot(row, byFamily, bindings, shotRefs, missing));
                }
                bucket.shots = shots.ToArray();
            }
            return manifest;
        }

        private static AuditionPvSixtySecondAtomicShot BuildEndCardShot(
            EdlSpec row,
            AuditionPvPinnedArtifact graphic,
            AuditionPvSixtySecondShotReferenceBinding references)
        {
            return new AuditionPvSixtySecondAtomicShot
            {
                shotId = row.atomicShotId,
                sourceKind = "end-card",
                timelineStartFrame = row.timelineStartFrame,
                timelineEndFrame = row.timelineEndFrame,
                coreShot = false,
                deterministicSeed = -1,
                editorialHudMode = "end-card",
                graphicSourceId = "layout-placeholder",
                graphicProductionStatus = "layout-placeholder-approved",
                sloganApprovalStatus = "pending-approval",
                auditionNoticeApprovalStatus = "pending-approval",
                graphicArtifact = ClonePin(graphic),
                beatIds = row.beatIds.ToArray(),
                audioRefIds = Clone(references.audioRefIds),
                usedItemIds = Clone(references.usedItemIds),
                candidateTakes = Array.Empty<AuditionPvSixtySecondTakeCandidate>()
            };
        }

        private static AuditionPvSixtySecondAtomicShot BuildSourceShot(
            EdlSpec row,
            IReadOnlyDictionary<string, List<LoadedSource>> byFamily,
            IReadOnlyDictionary<string, AuditionPvSixtySecondTakeEvidenceBinding> bindings,
            AuditionPvSixtySecondShotReferenceBinding references,
            ISet<string> missing)
        {
            List<LoadedSource> sources = byFamily.TryGetValue(row.familyId, out var family)
                ? family : new List<LoadedSource>();
            LoadedAuthorship canonical = sources.Select(source =>
                    source.authorship.TryGetValue(row.sourceShotId, out LoadedAuthorship value)
                        ? value : null)
                .FirstOrDefault(value => value != null);
            AuditionPvShotManifestEntry canonicalShot = sources.Select(source =>
                    FindShot(source.capture, row.sourceShotId))
                .FirstOrDefault(value => value != null);
            if (canonical == null || canonicalShot == null)
                missing.Add("SHOT_SOURCE_METADATA_MISSING:" + row.atomicShotId);

            var shot = new AuditionPvSixtySecondAtomicShot
            {
                shotId = row.atomicShotId,
                sourceKind = row.sourceKind,
                timelineStartFrame = row.timelineStartFrame,
                timelineEndFrame = row.timelineEndFrame,
                coreShot = row.coreShot,
                scenePath = canonicalShot?.scenePath ?? string.Empty,
                cameraId = canonical?.artifact.cameraId ?? string.Empty,
                gameplayState = canonical?.artifact.gameplayState ?? string.Empty,
                deterministicSeed = canonical?.artifact.deterministicSeed ?? -1,
                timelineId = canonical?.artifact.timelineId ?? string.Empty,
                editorialHudMode = row.editorialHudMode,
                beatIds = row.beatIds.ToArray(),
                audioRefIds = Clone(references.audioRefIds),
                usedItemIds = Clone(references.usedItemIds)
            };

            var candidates = new List<AuditionPvSixtySecondTakeCandidate>();
            var approved = new List<string>();
            foreach (LoadedSource source in sources)
            {
                string key = BindingKey(row.atomicShotId, source.capture.captureId,
                    row.sourceShotId, false);
                bindings.TryGetValue(key,
                    out AuditionPvSixtySecondTakeEvidenceBinding evidence);
                if (evidence == null)
                    missing.Add("TAKE_EVIDENCE_MISSING:" + key);
                AuditionPvSixtySecondTakeCandidate take = CreateTake(
                    row, source, row.sourceShotId, evidence, cleanPlate: false, missing);
                candidates.Add(take);
                if (evidence?.approved == true)
                    approved.Add(take.takeId);
                if (canonical != null && source.authorship.TryGetValue(row.sourceShotId,
                        out LoadedAuthorship actual) && !SameDirection(canonical.artifact,
                        actual.artifact))
                    missing.Add("SHOT_AUTHORSHIP_NOT_IDENTICAL:" + row.atomicShotId + ":"
                        + source.capture.captureId);
            }
            if (approved.Count != 1)
                missing.Add("APPROVED_TAKE_COUNT:" + row.atomicShotId + ":actual="
                    + approved.Count.ToString(CultureInfo.InvariantCulture));
            shot.approvedTakeId = approved.Count == 1 ? approved[0] : string.Empty;

            if (!string.IsNullOrWhiteSpace(row.cleanPlateSourceShotId))
            {
                var cleanBindings = bindings.Values.Where(value => value != null &&
                        value.atomicShotId == row.atomicShotId && value.cleanPlate)
                    .OrderBy(value => value.sourceCaptureId, StringComparer.Ordinal).ToArray();
                if (cleanBindings.Length != 1)
                    missing.Add("CLEAN_PLATE_COUNT:" + row.atomicShotId + ":actual="
                        + cleanBindings.Length.ToString(CultureInfo.InvariantCulture));
                if (cleanBindings.Length == 1)
                {
                    AuditionPvSixtySecondTakeEvidenceBinding cleanBinding = cleanBindings[0];
                    LoadedSource source = sources.SingleOrDefault(value =>
                        value.capture.captureId == cleanBinding.sourceCaptureId);
                    if (source == null || cleanBinding.sourceShotId != row.cleanPlateSourceShotId)
                    {
                        missing.Add("CLEAN_PLATE_SOURCE_INVALID:" + row.atomicShotId);
                    }
                    else
                    {
                        AuditionPvSixtySecondTakeCandidate clean = CreateTake(row, source,
                            row.cleanPlateSourceShotId, cleanBinding, cleanPlate: true, missing);
                        candidates.Add(clean);
                        shot.cleanPlateTakeId = clean.takeId;
                    }
                }
            }
            else if (bindings.Values.Any(value => value != null &&
                         value.atomicShotId == row.atomicShotId && value.cleanPlate))
            {
                missing.Add("CLEAN_PLATE_NOT_ALLOWED:" + row.atomicShotId);
            }
            shot.candidateTakes = candidates.ToArray();
            return shot;
        }

        private static AuditionPvSixtySecondTakeCandidate CreateTake(
            EdlSpec row,
            LoadedSource source,
            string sourceShotId,
            AuditionPvSixtySecondTakeEvidenceBinding evidence,
            bool cleanPlate,
            ISet<string> missing)
        {
            AuditionPvShotManifestEntry sourceShot = FindShot(source.capture, sourceShotId);
            source.authorship.TryGetValue(sourceShotId, out LoadedAuthorship authorship);
            if (sourceShot == null || authorship == null)
                missing.Add("TAKE_SOURCE_ARTIFACT_MISSING:" + row.atomicShotId + ":"
                    + source.capture.captureId + ":" + sourceShotId);
            if (sourceShot != null && (row.sourceRangeStartFrame < sourceShot.startFrame ||
                row.sourceRangeEndFrame > sourceShot.endFrame))
                missing.Add("TAKE_RANGE_OUTSIDE_SOURCE:" + row.atomicShotId + ":"
                    + source.capture.captureId);

            AuditionPvPinnedArtifact ledger = PinReady(evidence?.sourceFrameLedger)
                ? ClonePin(evidence.sourceFrameLedger)
                : ClonePin(source.frameLedger);
            RequirePin(ledger, "SOURCE_FRAME_LEDGER:" + row.atomicShotId + ":"
                + source.capture.captureId + ":" + sourceShotId, missing);
            if (cleanPlate)
            {
                RequirePin(evidence?.cleanPlateProof, "CLEAN_PLATE_PROOF:" + row.atomicShotId,
                    missing);
            }
            else
            {
                RequirePin(evidence?.semanticProof, "SEMANTIC_PROOF:" + row.atomicShotId + ":"
                    + source.capture.captureId, missing);
            }
            if (evidence?.approved == true || cleanPlate)
            {
                RequirePin(evidence?.automatedProof, "AUTOMATED_PROOF:" + row.atomicShotId + ":"
                    + source.capture.captureId, missing);
                RequirePin(evidence?.humanReview, "HUMAN_REVIEW:" + row.atomicShotId + ":"
                    + source.capture.captureId, missing);
            }

            string prefix = cleanPlate ? "clean" : "take";
            return new AuditionPvSixtySecondTakeCandidate
            {
                takeId = row.atomicShotId + "-" + prefix + "-" + source.capture.captureId,
                sourceCaptureId = source.capture.captureId,
                sourceShotId = sourceShotId,
                gitCommitSha = source.capture.gitCommitSha,
                declaredHudMode = sourceShot?.hudMode ?? string.Empty,
                cameraId = authorship?.artifact.cameraId ?? string.Empty,
                gameplayState = authorship?.artifact.gameplayState ?? string.Empty,
                timelineId = authorship?.artifact.timelineId ?? string.Empty,
                deterministicSeed = authorship?.artifact.deterministicSeed ?? -1,
                sourceDependencyIdentitySha256 = source.dependencyIdentitySha256,
                sourceCaptureCoreSha256 = source.captureCoreSha256,
                sourceManifest = ClonePin(source.manifest),
                sourceFrameLedger = ledger,
                shotAuthorship = ClonePin(authorship?.pin),
                semanticProof = cleanPlate ? new AuditionPvPinnedArtifact()
                    : ClonePin(evidence?.semanticProof),
                cleanPlateProof = cleanPlate ? ClonePin(evidence?.cleanPlateProof)
                    : new AuditionPvPinnedArtifact(),
                automatedProof = ClonePin(evidence?.automatedProof),
                humanReview = ClonePin(evidence?.humanReview),
                sourceRangeStartFrame = row.sourceRangeStartFrame,
                sourceRangeEndFrame = row.sourceRangeEndFrame,
                selectStartFrame = row.selectStartFrame,
                selectEndFrame = row.selectEndFrame,
                handleBeforeFrames = row.selectStartFrame - row.sourceRangeStartFrame,
                handleAfterFrames = row.sourceRangeEndFrame - row.selectEndFrame
            };
        }

        private static List<LoadedSource> LoadSources(
            IEnumerable<string> manifestPaths,
            string captureRoot,
            ISet<string> missing)
        {
            var result = new List<LoadedSource>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string declared in (manifestPaths ?? Array.Empty<string>())
                         .Where(value => !string.IsNullOrWhiteSpace(value))
                         .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(value => value, StringComparer.Ordinal))
            {
                var source = new LoadedSource();
                try
                {
                    string path = Path.GetFullPath(declared);
                    if (!seenPaths.Add(path))
                        throw new InvalidDataException("Duplicate capture manifest path.");
                    RequireUnder(path, captureRoot);
                    AuditionPvCaptureManifest capture = ReadJson<AuditionPvCaptureManifest>(path);
                    AuditionPvCaptureManifestWriter.Validate(capture);
                    string canonical = Path.Combine(capture.outputDirectory,
                        AuditionPvCaptureContract.ManifestFileName);
                    if (!PathsEqual(path, canonical))
                        throw new InvalidDataException(
                            "Capture manifest is not its declared canonical direct file.");
                    if (!seenIds.Add(capture.captureId))
                        throw new InvalidDataException("Duplicate capture ID.");
                    if (capture.gitWorktreeDirty)
                        throw new InvalidDataException("Dirty capture is not eligible.");
                    if ((capture.testResults ?? Array.Empty<AuditionPvTestResult>())
                        .Any(value => value == null || value.status != "passed"))
                        throw new InvalidDataException("Capture contains a non-passing test result.");
                    string familyId = Classify(capture);
                    if (string.IsNullOrEmpty(familyId))
                        throw new InvalidDataException("Capture shot set does not match a production family.");

                    source.capture = capture;
                    source.familyId = familyId;
                    source.manifest = Pin(path);
                    source.captureCoreSha256 = AuditionPvSixtySecondGateManifestValidator
                        .CaptureCoreSha256(capture);
                    source.dependencyIdentitySha256 = DependencyIdentity(capture);
                    source.frameLedger = FindHashLedger(capture);
                    source.authorship = LoadAuthorship(capture, source.captureCoreSha256);
                    foreach (string shotId in Families.Single(value => value.id == familyId).shotIds)
                    {
                        if (!source.authorship.ContainsKey(shotId))
                            throw new InvalidDataException(
                                "Capture is missing pinned shot-authorship for " + shotId + ".");
                    }
                    source.valid = true;
                }
                catch (Exception exception) when (exception is IOException ||
                    exception is UnauthorizedAccessException ||
                    exception is ArgumentException ||
                    exception is InvalidOperationException)
                {
                    source.issue = exception.Message;
                    missing.Add("CAPTURE_INVALID:" + NormalizePath(declared) + ":"
                        + exception.GetType().Name);
                }
                result.Add(source);
            }
            return result;
        }

        private static Dictionary<string, LoadedAuthorship> LoadAuthorship(
            AuditionPvCaptureManifest capture,
            string captureCoreSha256)
        {
            var result = new Dictionary<string, LoadedAuthorship>(StringComparer.Ordinal);
            foreach (AuditionPvShotManifestEntry shot in capture.shots ??
                         Array.Empty<AuditionPvShotManifestEntry>())
            {
                string expectedName = "shot-authorship/" + shot.id;
                AuditionPvTestResult[] rows = (capture.testResults ??
                        Array.Empty<AuditionPvTestResult>())
                    .Where(value => value != null && value.suite == GateSuite &&
                        value.name == expectedName && value.status == "passed")
                    .ToArray();
                if (rows.Length != 1 || string.IsNullOrWhiteSpace(rows[0].artifactPath))
                    continue;
                AuditionPvPinnedArtifact pin = Pin(rows[0].artifactPath);
                if (!(rows[0].details ?? string.Empty).Contains(
                        "artifact-sha256=" + pin.sha256, StringComparison.Ordinal))
                    continue;
                AuditionPvShotAuthorshipArtifact artifact =
                    ReadJson<AuditionPvShotAuthorshipArtifact>(pin.path);
                if (artifact.schemaVersion !=
                        AuditionPvSixtySecondGateManifestValidator.ShotAuthorshipSchema ||
                    artifact.captureId != capture.captureId || artifact.sourceShotId != shot.id ||
                    artifact.sourceCaptureCoreSha256 != captureCoreSha256)
                    continue;
                result.Add(shot.id, new LoadedAuthorship { pin = pin, artifact = artifact });
            }
            return result;
        }

        private static AuditionPvPinnedArtifact FindHashLedger(AuditionPvCaptureManifest capture)
        {
            foreach (AuditionPvTestResult row in (capture.testResults ??
                         Array.Empty<AuditionPvTestResult>())
                     .Where(value => value != null && value.status == "passed" &&
                         !string.IsNullOrWhiteSpace(value.artifactPath))
                     .OrderBy(value => value.artifactPath, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(value => value.artifactPath, StringComparer.Ordinal))
            {
                if (!File.Exists(row.artifactPath) || !LooksLikeHashLedger(row.artifactPath))
                    continue;
                return Pin(row.artifactPath);
            }
            return new AuditionPvPinnedArtifact();
        }

        private static bool LooksLikeHashLedger(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true,
                4096, false);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                return line.Length >= 67 && line[64] == ' ' && line[65] == ' ' &&
                    AuditionPvSha256.IsSha256(line.Substring(0, 64));
            }
            return false;
        }

        private static Dictionary<string, AuditionPvSixtySecondTakeEvidenceBinding>
            IndexTakeBindings(IEnumerable<AuditionPvSixtySecondTakeEvidenceBinding> values,
                ISet<string> missing)
        {
            var result = new Dictionary<string, AuditionPvSixtySecondTakeEvidenceBinding>(
                StringComparer.Ordinal);
            foreach (AuditionPvSixtySecondTakeEvidenceBinding value in values ??
                         Array.Empty<AuditionPvSixtySecondTakeEvidenceBinding>())
            {
                if (value == null || string.IsNullOrWhiteSpace(value.atomicShotId) ||
                    string.IsNullOrWhiteSpace(value.sourceCaptureId) ||
                    string.IsNullOrWhiteSpace(value.sourceShotId) ||
                    value.approved && value.cleanPlate)
                {
                    missing.Add("TAKE_EVIDENCE_BINDING_INVALID");
                    continue;
                }
                string key = BindingKey(value.atomicShotId, value.sourceCaptureId,
                    value.sourceShotId, value.cleanPlate);
                if (!result.TryAdd(key, value))
                    missing.Add("TAKE_EVIDENCE_BINDING_DUPLICATE:" + key);
            }
            return result;
        }

        private static Dictionary<string, AuditionPvSixtySecondShotReferenceBinding>
            IndexShotReferences(IEnumerable<AuditionPvSixtySecondShotReferenceBinding> values,
                ISet<string> missing)
        {
            var result = new Dictionary<string, AuditionPvSixtySecondShotReferenceBinding>(
                StringComparer.Ordinal);
            foreach (AuditionPvSixtySecondShotReferenceBinding value in values ??
                         Array.Empty<AuditionPvSixtySecondShotReferenceBinding>())
            {
                if (value == null || string.IsNullOrWhiteSpace(value.atomicShotId) ||
                    !result.TryAdd(value.atomicShotId, value))
                    missing.Add("SHOT_REFERENCE_BINDING_INVALID_OR_DUPLICATE");
            }
            string[] expected = DefaultEdl.Select(value => value.atomicShotId).ToArray();
            foreach (string extra in result.Keys.Where(value =>
                         !expected.Contains(value, StringComparer.Ordinal)))
                missing.Add("SHOT_REFERENCE_UNKNOWN:" + extra);
            return result;
        }

        private static AuditionPvSixtySecondProductionInventory CreateInventory(
            IEnumerable<LoadedSource> loaded,
            IReadOnlyDictionary<string, List<LoadedSource>> byFamily,
            IEnumerable<string> missing,
            bool ready,
            bool structureComplete,
            bool hermeticTestSeam)
        {
            string[] missingRows = missing.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return new AuditionPvSixtySecondProductionInventory
            {
                status = ready ? "ready-for-authoritative-manifest" :
                    structureComplete && hermeticTestSeam ? "hermetic-structure-only" :
                    "partial-evidence-missing",
                authoritativeEligible = ready,
                hermeticTestSeam = hermeticTestSeam,
                expectedCaptureRunCount = ExpectedCaptureRunCount,
                observedEligibleCaptureRunCount = loaded.Count(value => value.valid),
                familyCounts = Families.Select(family => new
                    AuditionPvSixtySecondProductionFamilyCount
                    {
                        familyId = family.id,
                        expectedRuns = family.requiredRuns,
                        observedEligibleRuns = byFamily[family.id].Count
                    }).ToArray(),
                captures = loaded.Select(value => new AuditionPvSixtySecondProductionCaptureRow
                    {
                        captureId = value.capture?.captureId ?? string.Empty,
                        familyId = value.familyId ?? string.Empty,
                        eligible = value.valid,
                        manifest = ClonePin(value.manifest),
                        captureCoreSha256 = value.captureCoreSha256 ?? string.Empty,
                        dependencyIdentitySha256 = value.dependencyIdentitySha256 ?? string.Empty,
                        issue = value.issue ?? string.Empty
                    }).OrderBy(value => value.captureId, StringComparer.Ordinal).ToArray(),
                edl = DefaultEdl.Select(row =>
                {
                    AuditionPvSixtySecondProductionEdlRow value = ToInventoryRow(row);
                    value.candidateCaptureIds = row.endCard
                        ? Array.Empty<string>()
                        : byFamily[row.familyId].Select(source => source.capture.captureId).ToArray();
                    return value;
                }).ToArray(),
                missingRequirements = missingRows
            };
        }

        private static AuditionPvSixtySecondProductionEdlRow ToInventoryRow(EdlSpec row)
        {
            return new AuditionPvSixtySecondProductionEdlRow
            {
                bucketId = row.bucketId,
                atomicShotId = row.atomicShotId,
                timelineStartFrame = row.timelineStartFrame,
                timelineEndFrame = row.timelineEndFrame,
                familyId = row.familyId,
                sourceShotId = row.sourceShotId,
                sourceRangeStartFrame = row.sourceRangeStartFrame,
                sourceRangeEndFrame = row.sourceRangeEndFrame,
                selectStartFrame = row.selectStartFrame,
                selectEndFrame = row.selectEndFrame,
                handleBeforeFrames = row.endCard ? 0 : 180,
                handleAfterFrames = row.endCard ? 0 : 180,
                beatIds = row.beatIds.ToArray(),
                candidateCaptureIds = Array.Empty<string>()
            };
        }

        private static string Classify(AuditionPvCaptureManifest capture)
        {
            string[] actual = (capture.shots ?? Array.Empty<AuditionPvShotManifestEntry>())
                .Where(value => value != null).Select(value => value.id)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return Families.FirstOrDefault(value => actual.SequenceEqual(
                value.shotIds.OrderBy(id => id, StringComparer.Ordinal), StringComparer.Ordinal))?.id
                ?? string.Empty;
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
                string path = NormalizePath(dependency?.path);
                if (dependency == null || string.IsNullOrWhiteSpace(path) ||
                    !seen.Add(path) || !dependency.exists || dependency.byteLength < 0 ||
                    !AuditionPvSha256.IsSha256(dependency.sha256))
                    throw new InvalidDataException("Invalid capture dependency identity row.");
                material.Append(path).Append('\0').Append('1').Append('\0')
                    .Append(dependency.byteLength.ToString(CultureInfo.InvariantCulture))
                    .Append('\0').Append(dependency.sha256).Append('\0');
            }
            if (seen.Count == 0)
                throw new InvalidDataException("Capture dependency identity is empty.");
            return AuditionPvSha256.TextHash(material.ToString());
        }

        private static AuditionPvSixtySecondValidationContext CreateInstalledContext()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            return new AuditionPvSixtySecondValidationContext
            {
                projectRoot = projectRoot,
                currentGitCommitSha = git.probeSucceeded ? git.commitSha : string.Empty,
                currentGitClean = git.probeSucceeded && !git.isDirty,
                allowedEvidenceRoots = new[]
                {
                    projectRoot, AuditionPvCaptureContract.OutputRoot,
                    AuditionPvTwelveSecondGoldAssembler.OutputRoot,
                    AuditionPvSixtySecondGateManifestValidator.ProductionManifestRoot,
                    AuditionPvSixtySecondGateManifestValidator.ProductionAudioRoot,
                    AuditionPvSixtySecondGateManifestValidator.ProductionLicensesRoot,
                    AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot,
                    AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot
                },
                allowedCaptureRoots = new[] { AuditionPvCaptureContract.OutputRoot },
                allowedSelectRoots = new[] { AuditionPvTwelveSecondGoldAssembler.OutputRoot },
                allowedAudioRoots = new[]
                    { AuditionPvSixtySecondGateManifestValidator.ProductionAudioRoot },
                allowedLicenseRoots = new[]
                    { AuditionPvSixtySecondGateManifestValidator.ProductionLicensesRoot },
                allowedGraphicsRoots = new[]
                    { AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot },
                allowedReviewRoots = new[]
                    { AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot }
            };
        }

        private static void RequireGateEvidence(AuditionPvSixtySecondGateEvidence evidence,
            ISet<string> missing)
        {
            if (evidence == null)
            {
                missing.Add("GATE_EVIDENCE_MISSING");
                return;
            }
            if (string.IsNullOrWhiteSpace(evidence.twelveSecondPackageDirectory) ||
                !AuditionPvSha256.IsSha256(evidence.twelveSecondManifestSha256) ||
                !AuditionPvSha256.IsSha256(evidence.twelveSecondValidationSha256))
                missing.Add("TWELVE_SECOND_PACKAGE_PINS_MISSING");
            RequirePin(evidence.twelveSecondApproval, "TWELVE_SECOND_APPROVAL", missing);
            RequirePin(evidence.visualReview, "VISUAL_REVIEW", missing);
            RequirePin(evidence.rightsCoverageReview, "RIGHTS_COVERAGE_REVIEW", missing);
        }

        private static void RequirePin(AuditionPvPinnedArtifact pin, string id,
            ISet<string> missing)
        {
            if (!PinReady(pin))
            {
                missing.Add("PIN_MISSING_OR_DRIFT:" + id);
                return;
            }
            try
            {
                if (!File.Exists(pin.path) ||
                    AuditionPvSha256.FileHash(pin.path) != pin.sha256)
                    missing.Add("PIN_MISSING_OR_DRIFT:" + id);
            }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                missing.Add("PIN_MISSING_OR_DRIFT:" + id);
            }
        }

        private static bool PinReady(AuditionPvPinnedArtifact pin) =>
            pin != null && !string.IsNullOrWhiteSpace(pin.path) &&
            AuditionPvSha256.IsSha256(pin.sha256);

        private static void RequireNonEmpty<T>(T[] values, string id, ISet<string> missing)
        {
            if (values == null || values.Length == 0) missing.Add(id);
        }

        private static bool SameDirection(AuditionPvShotAuthorshipArtifact left,
            AuditionPvShotAuthorshipArtifact right) =>
            left != null && right != null && left.cameraId == right.cameraId &&
            left.gameplayState == right.gameplayState &&
            left.deterministicSeed == right.deterministicSeed &&
            left.timelineId == right.timelineId;

        private static AuditionPvShotManifestEntry FindShot(AuditionPvCaptureManifest manifest,
            string id) => (manifest?.shots ?? Array.Empty<AuditionPvShotManifestEntry>())
            .SingleOrDefault(value => value != null && value.id == id);

        private static string BindingKey(string atomicShotId, string captureId,
            string sourceShotId, bool cleanPlate) =>
            atomicShotId + "|" + captureId + "|" + sourceShotId + "|"
            + (cleanPlate ? "clean" : "editorial");

        private static AuditionPvPinnedArtifact Pin(string path)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path)) throw new FileNotFoundException("Pinned file is missing.", path);
            return new AuditionPvPinnedArtifact
            {
                path = NormalizePath(path),
                sha256 = AuditionPvSha256.FileHash(path)
            };
        }

        private static AuditionPvPinnedArtifact ClonePin(AuditionPvPinnedArtifact value) =>
            value == null ? new AuditionPvPinnedArtifact() : new AuditionPvPinnedArtifact
            {
                path = value.path ?? string.Empty,
                sha256 = value.sha256 ?? string.Empty
            };

        private static string[] Clone(string[] values) =>
            (values ?? Array.Empty<string>()).ToArray();

        private static T ReadJson<T>(string path) where T : class
        {
            string full = Path.GetFullPath(path);
            var file = new FileInfo(full);
            if (!file.Exists || file.Length > MaximumJsonBytes)
                throw new InvalidDataException("JSON input is missing or exceeds 16 MiB: " + full);
            byte[] bytes = File.ReadAllBytes(full);
            string json = new UTF8Encoding(false, true).GetString(bytes).TrimStart('\ufeff');
            T value = JsonUtility.FromJson<T>(json);
            return value ?? throw new InvalidDataException("JSON root is null: " + full);
        }

        private static void RequireUnder(string path, string root)
        {
            string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string parent = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(parent, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Path is outside the capture root: " + full);
        }

        private static string CanonicalManifestPath() => Path.GetFullPath(Path.Combine(
            AuditionPvSixtySecondGateManifestValidator.ProductionManifestRoot,
            AuditionPvSixtySecondGateManifestValidator.ProductionManifestFileName));

        private static bool PathsEqual(string left, string right) =>
            string.Equals(Path.GetFullPath(left).TrimEnd('\\', '/'),
                Path.GetFullPath(right).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase);

        private static string NormalizePath(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace('\\', '/');

        private static bool IsFullGitSha(string value) => value != null && value.Length == 40 &&
            value.All(character => character >= '0' && character <= '9' ||
                character >= 'a' && character <= 'f');

        private static byte[] JsonBytes(object value) => new UTF8Encoding(false).GetBytes(
            JsonUtility.ToJson(value, true) + Environment.NewLine);

        private static string BytesHash(byte[] value)
        {
            string temporary = Path.Combine(Path.GetTempPath(),
                "pv60-hash-" + Guid.NewGuid().ToString("N"));
            try
            {
                File.WriteAllBytes(temporary, value);
                return AuditionPvSha256.FileHash(temporary);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static void WriteReplace(string path, object value)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            byte[] bytes = JsonBytes(value);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllBytes(temporary, bytes);
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static void WriteNew(string path, byte[] bytes)
        {
            AssertAuthoritativeDestinationForTests(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllBytes(temporary, bytes);
                if (File.Exists(path))
                    throw new IOException("Canonical Gate manifest appeared during installation.");
                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static void DeleteFailedInstallIfUnchanged(string path, string expectedSha256)
        {
            if (!File.Exists(path)) return;
            string actual = AuditionPvSha256.FileHash(path);
            if (!string.Equals(actual, expectedSha256, StringComparison.Ordinal))
                throw new IOException(
                    "The failed canonical manifest changed before transactional cleanup.");
            File.Delete(path);
        }

        private static string ReadArgument(string prefix)
        {
            foreach (string argument in Environment.GetCommandLineArgs())
            {
                if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return argument.Substring(prefix.Length);
            }
            return string.Empty;
        }

        private static EdlSpec Edl(string bucketId, string atomicShotId,
            int timelineStart, int timelineEnd, string familyId, string sourceShotId,
            int selectStart, int selectEnd, string sourceKind, string editorialHudMode,
            bool coreShot, string beatId, params string[] additionalBeatIds) =>
            Edl(bucketId, atomicShotId, timelineStart, timelineEnd, familyId, sourceShotId,
                selectStart, selectEnd, sourceKind, editorialHudMode, coreShot,
                new[] { beatId }.Concat(additionalBeatIds ?? Array.Empty<string>()).ToArray(),
                string.Empty);

        private static EdlSpec EdlWithCleanPlate(string bucketId, string atomicShotId,
            int timelineStart, int timelineEnd, string familyId, string sourceShotId,
            int selectStart, int selectEnd, string sourceKind, string editorialHudMode,
            bool coreShot, string cleanPlateSourceShotId, params string[] beatIds) =>
            Edl(bucketId, atomicShotId, timelineStart, timelineEnd, familyId, sourceShotId,
                selectStart, selectEnd, sourceKind, editorialHudMode, coreShot,
                beatIds ?? Array.Empty<string>(), cleanPlateSourceShotId);

        private static EdlSpec Edl(string bucketId, string atomicShotId,
            int timelineStart, int timelineEnd, string familyId, string sourceShotId,
            int selectStart, int selectEnd, string sourceKind, string editorialHudMode,
            bool coreShot, string[] beatIds, string cleanPlateSourceShotId)
        {
            int timelineLength = timelineEnd - timelineStart + 1;
            int selectLength = selectEnd - selectStart + 1;
            if (timelineLength != selectLength)
                throw new InvalidOperationException("Default 60-second EDL length mismatch.");
            return new EdlSpec
            {
                bucketId = bucketId,
                atomicShotId = atomicShotId,
                timelineStartFrame = timelineStart,
                timelineEndFrame = timelineEnd,
                familyId = familyId,
                sourceShotId = sourceShotId,
                sourceRangeStartFrame = selectStart - 180,
                sourceRangeEndFrame = selectEnd + 180,
                selectStartFrame = selectStart,
                selectEndFrame = selectEnd,
                sourceKind = sourceKind,
                editorialHudMode = editorialHudMode,
                coreShot = coreShot,
                beatIds = beatIds,
                cleanPlateSourceShotId = cleanPlateSourceShotId
            };
        }

        private static EdlSpec EndCardEdl(string bucketId, string atomicShotId,
            int timelineStart, int timelineEnd, params string[] beatIds) => new()
        {
            bucketId = bucketId,
            atomicShotId = atomicShotId,
            timelineStartFrame = timelineStart,
            timelineEndFrame = timelineEnd,
            sourceKind = "end-card",
            editorialHudMode = "end-card",
            beatIds = beatIds ?? Array.Empty<string>(),
            endCard = true
        };

        private sealed class FamilySpec
        {
            public FamilySpec(string id, int requiredRuns, params string[] shotIds)
            {
                this.id = id;
                this.requiredRuns = requiredRuns;
                this.shotIds = shotIds;
            }
            public readonly string id;
            public readonly int requiredRuns;
            public readonly string[] shotIds;
        }

        private sealed class EdlSpec
        {
            public string bucketId = string.Empty, atomicShotId = string.Empty;
            public int timelineStartFrame, timelineEndFrame;
            public string familyId = string.Empty, sourceShotId = string.Empty;
            public int sourceRangeStartFrame, sourceRangeEndFrame;
            public int selectStartFrame, selectEndFrame;
            public string sourceKind = string.Empty, editorialHudMode = string.Empty;
            public bool coreShot, endCard;
            public string[] beatIds = Array.Empty<string>();
            public string cleanPlateSourceShotId = string.Empty;
        }

        private sealed class LoadedSource
        {
            public bool valid;
            public string familyId = string.Empty, issue = string.Empty;
            public AuditionPvCaptureManifest capture;
            public AuditionPvPinnedArtifact manifest = new(), frameLedger = new();
            public string captureCoreSha256 = string.Empty;
            public string dependencyIdentitySha256 = string.Empty;
            public Dictionary<string, LoadedAuthorship> authorship =
                new(StringComparer.Ordinal);
        }

        private sealed class LoadedAuthorship
        {
            public AuditionPvPinnedArtifact pin = new();
            public AuditionPvShotAuthorshipArtifact artifact;
        }
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionComposeInput
    {
        public string schemaVersion = AuditionPvSixtySecondProductionComposer.InputSchema;
        public string productCheckpointGitSha = string.Empty;
        public string[] captureManifestPaths = Array.Empty<string>();
        public AuditionPvSixtySecondTakeEvidenceBinding[] takeEvidence =
            Array.Empty<AuditionPvSixtySecondTakeEvidenceBinding>();
        public AuditionPvSixtySecondShotReferenceBinding[] shotReferences =
            Array.Empty<AuditionPvSixtySecondShotReferenceBinding>();
        public AuditionPvSixtySecondAudioEvidence[] audio =
            Array.Empty<AuditionPvSixtySecondAudioEvidence>();
        public AuditionPvSixtySecondRightsEvidence[] rights =
            Array.Empty<AuditionPvSixtySecondRightsEvidence>();
        public AuditionPvSixtySecondUsedItem[] usedItems =
            Array.Empty<AuditionPvSixtySecondUsedItem>();
        public AuditionPvSixtySecondGateEvidence gateEvidence = new();
        public AuditionPvPinnedArtifact endCardGraphic = new();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondTakeEvidenceBinding
    {
        public string atomicShotId = string.Empty;
        public string sourceCaptureId = string.Empty, sourceShotId = string.Empty;
        public bool approved, cleanPlate;
        public AuditionPvPinnedArtifact sourceFrameLedger = new();
        public AuditionPvPinnedArtifact semanticProof = new(), cleanPlateProof = new();
        public AuditionPvPinnedArtifact automatedProof = new(), humanReview = new();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondShotReferenceBinding
    {
        public string atomicShotId = string.Empty;
        public string[] audioRefIds = Array.Empty<string>();
        public string[] usedItemIds = Array.Empty<string>();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionComposition
    {
        public AuditionPvSixtySecondProductionInventory inventory = new();
        public AuditionPvSixtySecondShotGateManifest manifest;
        public bool finalManifestReady, hermeticTestSeam;
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionInventory
    {
        public string schemaVersion = AuditionPvSixtySecondProductionComposer.InventorySchema;
        public string status = "partial-evidence-missing";
        public bool authoritativeEligible, hermeticTestSeam;
        public int expectedCaptureRunCount, observedEligibleCaptureRunCount;
        public AuditionPvSixtySecondProductionFamilyCount[] familyCounts =
            Array.Empty<AuditionPvSixtySecondProductionFamilyCount>();
        public AuditionPvSixtySecondProductionCaptureRow[] captures =
            Array.Empty<AuditionPvSixtySecondProductionCaptureRow>();
        public AuditionPvSixtySecondProductionEdlRow[] edl =
            Array.Empty<AuditionPvSixtySecondProductionEdlRow>();
        public string[] missingRequirements = Array.Empty<string>();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionFamilyCount
    {
        public string familyId = string.Empty;
        public int expectedRuns, observedEligibleRuns;
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionCaptureRow
    {
        public string captureId = string.Empty, familyId = string.Empty;
        public bool eligible;
        public AuditionPvPinnedArtifact manifest = new();
        public string captureCoreSha256 = string.Empty;
        public string dependencyIdentitySha256 = string.Empty;
        public string issue = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionEdlRow
    {
        public string bucketId = string.Empty, atomicShotId = string.Empty;
        public int timelineStartFrame, timelineEndFrame;
        public string familyId = string.Empty, sourceShotId = string.Empty;
        public int sourceRangeStartFrame, sourceRangeEndFrame;
        public int selectStartFrame, selectEndFrame;
        public int handleBeforeFrames, handleAfterFrames;
        public string[] beatIds = Array.Empty<string>();
        public string[] candidateCaptureIds = Array.Empty<string>();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondProductionWriteResult
    {
        public string inventoryPath = string.Empty, manifestPath = string.Empty;
        public bool finalManifestReady, authoritativePassed;
        public string[] missingRequirements = Array.Empty<string>();
        public AuditionPvSixtySecondGateValidationReport validation;
    }
}
