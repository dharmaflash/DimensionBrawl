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
    /// Builds the audio/rights fragment consumed by the 60-second production
    /// composer. Decisions are always supplied by an operator spec: this type
    /// inventories candidates, verifies bytes, and records decisions, but never
    /// chooses a take, approves a listening pass, or invents a rights grant.
    /// </summary>
    internal static class AuditionPvSixtySecondAudioRightsAssembler
    {
        internal const string SelectionSchema =
            "dimension-brawl.audition-pv.audio-rights-selection.v1";
        internal const string FragmentSchema =
            "dimension-brawl.audition-pv.audio-rights-fragment.v1";
        internal const string InventorySchema =
            "dimension-brawl.audition-pv.audio-rights-inventory.v1";
        internal const string CoverageInputSchema =
            "dimension-brawl.audition-pv.rights-coverage-input.v1";
        internal const string DefaultManifestId =
            "dimension-brawl-audition-pv-60s-preedit";
        internal const string BatchSelectionArgument = "-pv60AudioRightsSelection=";
        internal const string HumanOperatorOrigin = "human-operator";
        private const long MaximumWaveBytes = 512L * 1024L * 1024L;
        private const long MaximumJsonBytes = 16L * 1024L * 1024L;
        private const long MaximumInventoryHashBytes = 64L * 1024L * 1024L;
        private const int MaximumInventoryEntries = 4096;
        private const int MaximumAudioRows = 128;
        private const int MaximumCueRegionsPerRow = 256;
        private const int MaximumRightsOrItems = 512;
        private const int MaximumCaptureShots = 512;
        private const int MaximumCaptureBaselines = 2048;
        private const int MaximumCaptureDependencies = 4096;
        private const int MaximumCaptureTestResults = 4096;
        private static readonly UTF8Encoding StrictUtf8 = new(false, true);

        private static readonly string[] Categories =
            { "music", "sfx", "vo", "ambience" };

        private static readonly string[] RequiredCues =
        {
            "music-bed", "city-ambience", "olympus-ambience",
            "gun-mechanical", "gun-fire", "gun-tail",
            "dodge", "summon", "hit", "boss-charge", "boss-fire", "boss-death",
            "wing-deploy", "eye-open", "announcement-vo", "inori-vo", "boss-vo"
        };

        private static readonly string[] AtomicShotIds =
        {
            "pv-s010-city-alert-skyline", "pv-s010-dimensional-anomaly",
            "pv-s020-city-gameplay", "pv-s030-hit-dodge-summon",
            "pv-s040-dimension-rift", "pv-s050-boss-low-angle",
            "pv-s060-wing-deployment", "pv-s060-eye-open",
            "pv-s070-pattern-one", "pv-s070-patterns-two-three",
            "pv-s080-dodge-summon-defense", "pv-s080-tier3-ultimate",
            "pv-s090-finisher-aftermath", "pv-s100-end-card"
        };

        private static readonly IReadOnlyDictionary<string, string[]> CueShotMap =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["music-bed"] = AtomicShotIds.ToArray(),
                ["city-ambience"] = new[]
                {
                    "pv-s010-city-alert-skyline", "pv-s010-dimensional-anomaly",
                    "pv-s020-city-gameplay", "pv-s030-hit-dodge-summon",
                    "pv-s040-dimension-rift"
                },
                ["olympus-ambience"] = new[]
                {
                    "pv-s050-boss-low-angle", "pv-s060-wing-deployment",
                    "pv-s060-eye-open", "pv-s070-pattern-one",
                    "pv-s070-patterns-two-three", "pv-s080-dodge-summon-defense",
                    "pv-s080-tier3-ultimate", "pv-s090-finisher-aftermath"
                },
                ["gun-mechanical"] = new[] { "pv-s020-city-gameplay" },
                ["gun-fire"] = new[] { "pv-s020-city-gameplay" },
                ["gun-tail"] = new[] { "pv-s020-city-gameplay" },
                ["dodge"] = new[]
                    { "pv-s030-hit-dodge-summon", "pv-s080-dodge-summon-defense" },
                ["summon"] = new[]
                    { "pv-s030-hit-dodge-summon", "pv-s080-dodge-summon-defense" },
                ["hit"] = new[]
                    { "pv-s030-hit-dodge-summon", "pv-s090-finisher-aftermath" },
                ["boss-charge"] = new[]
                    { "pv-s070-pattern-one", "pv-s070-patterns-two-three" },
                ["boss-fire"] = new[]
                    { "pv-s070-pattern-one", "pv-s070-patterns-two-three" },
                ["boss-death"] = new[] { "pv-s090-finisher-aftermath" },
                ["wing-deploy"] = new[] { "pv-s060-wing-deployment" },
                ["eye-open"] = new[] { "pv-s060-eye-open" },
                ["announcement-vo"] = new[]
                    { "pv-s010-city-alert-skyline", "pv-s040-dimension-rift" },
                ["inori-vo"] = new[]
                    { "pv-s030-hit-dodge-summon", "pv-s080-tier3-ultimate" },
                ["boss-vo"] = new[]
                    { "pv-s050-boss-low-angle", "pv-s070-patterns-two-three" }
            };

        internal static AuditionPvSixtySecondAudioRightsContext CreateProductionContext()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            return new AuditionPvSixtySecondAudioRightsContext
            {
                projectRoot = Normalize(projectRoot),
                audioRoot = AuditionPvSixtySecondGateManifestValidator.ProductionAudioRoot,
                licenseRoot = AuditionPvSixtySecondGateManifestValidator.ProductionLicensesRoot,
                reviewRoot = AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot,
                captureRoots = new[] { AuditionPvCaptureContract.OutputRoot }
            };
        }

        internal static AuditionPvSixtySecondAudioRightsInventory Inventory(
            AuditionPvSixtySecondAudioRightsContext context)
        {
            var issues = new List<AuditionPvAudioRightsAssemblyIssue>();
            var entries = new List<AuditionPvAudioRightsInventoryEntry>();
            if (!ContextShapeValid(context, issues))
                return InventoryResult(entries, issues);

            string assets = Path.Combine(context.projectRoot, "Assets");
            int traversedNodes = 0;
            AddInventoryTree(entries, issues, Path.Combine(assets, "_Game", "Art", "Audio"),
                "project-audio", ref traversedNodes);
            AddInventoryTree(entries, issues, context.audioRoot, "pv-audio", ref traversedNodes);
            AddInventoryTree(entries, issues, context.licenseRoot, "license-evidence",
                ref traversedNodes);
            AddInventoryTree(entries, issues,
                Path.Combine(assets, "_Game", "Art", "Fonts", "Pretendard"), "pretendard",
                ref traversedNodes);
            AddInventoryTree(entries, issues, Path.Combine(assets, "_Game", "Art", "Environment",
                "CityHeroPocket", "TokyoStreet"), "tokyo-street", ref traversedNodes);
            return InventoryResult(entries, issues);
        }

        internal static AuditionPvSixtySecondAudioRightsAssembly AssemblePreview(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            AuditionPvSixtySecondAudioRightsContext context) =>
            Assemble(spec, CanonicalSelectionBytes(spec), context, false);

        internal static AuditionPvSixtySecondAudioRightsSelectionSpec ReadSelectionFile(string path)
        {
            return ReadSelectionSnapshot(path).value;
        }

        public static void RunBatchAssemble()
        {
            try
            {
                string argument = Environment.GetCommandLineArgs().FirstOrDefault(value =>
                    value.StartsWith(BatchSelectionArgument, StringComparison.Ordinal));
                if (string.IsNullOrWhiteSpace(argument))
                    throw new ArgumentException(BatchSelectionArgument + "<absolute-json> is required.");
                string path = argument.Substring(BatchSelectionArgument.Length);
                PinnedSelection selection = ReadSelectionSnapshot(path);
                AuditionPvSixtySecondAudioRightsAssembly result =
                    Assemble(selection.value, selection.bytes, CreateProductionContext(), true,
                        selection.identity);
                if (!result.readyForComposer)
                {
                    Debug.LogError("[AuditionPV] Audio/rights fragment remains on HOLD: " +
                        string.Join(",", result.issues.Select(value => value.code)));
                    EditorApplication.Exit(2);
                    return;
                }
                Debug.Log("[AuditionPV] Audio/rights fragment ready: " + result.fragmentPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        internal static AuditionPvSixtySecondAudioRightsAssembly AssembleAndWriteForTests(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            AuditionPvSixtySecondAudioRightsContext context) =>
            Assemble(spec, CanonicalSelectionBytes(spec), context, true);

        private static AuditionPvSixtySecondAudioRightsAssembly Assemble(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            byte[] exactSelectionBytes,
            AuditionPvSixtySecondAudioRightsContext context, bool write,
            FileIdentity exactSelectionSource = null)
        {
            var issues = new List<AuditionPvAudioRightsAssemblyIssue>();
            var audio = new List<AuditionPvSixtySecondAudioEvidence>();
            var rights = new List<AuditionPvSixtySecondRightsEvidence>();
            var items = new List<AuditionPvSixtySecondUsedItem>();
            var generationLedgers = new List<AuditionPvAudioRightsGenerationLedgerBinding>();
            var rightArtifacts = new List<PendingArtifact>();
            var otherArtifacts = new List<PendingArtifact>();
            var consumedPins = new ConsumedPinRegistry();
            if (exactSelectionSource != null)
                consumedPins.Record(exactSelectionSource, "operator selection spec");
            var shotAudio = AtomicShotIds.ToDictionary(id => id,
                _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
            var shotItems = AtomicShotIds.ToDictionary(id => id,
                _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);

            if (!ContextShapeValid(context, issues) || spec == null)
            {
                if (spec == null) Add(issues, "error", "SELECTION_SPEC_NULL", "spec",
                    "An explicit operator selection spec is required.");
                return Finish(spec, context, audio, rights, items, shotAudio, shotItems,
                    generationLedgers,
                    new AuditionPvRightsCoverageReviewInput(), new AuditionPvPinnedArtifact(),
                    new AuditionPvPinnedArtifact(), issues, string.Empty, false);
            }
            if (spec.schemaVersion != SelectionSchema)
                Add(issues, "error", "SELECTION_SCHEMA_INVALID", "spec.schemaVersion",
                    spec.schemaVersion ?? "<null>");
            if (!SafeId(spec.assemblyId))
                Add(issues, "error", "ASSEMBLY_ID_INVALID", "spec.assemblyId",
                    "Use lowercase ASCII letters, digits, and hyphens.");
            if (string.IsNullOrWhiteSpace(spec.manifestId)) spec.manifestId = DefaultManifestId;
            if (spec.judgementOrigin != HumanOperatorOrigin)
                Add(issues, "error", "SELECTION_JUDGEMENT_ORIGIN_INVALID",
                    "spec.judgementOrigin",
                    "Only an explicit human-operator selection is accepted.");
            if (!IsFullGitSha(spec.productCheckpointGitSha))
                Add(issues, "error", "PRODUCT_CHECKPOINT_INVALID", "spec.productCheckpointGitSha",
                    "A full lowercase git SHA is required.");

            string audioOut = Path.Combine(context.audioRoot, "GATE_60S", spec.assemblyId ?? "invalid");
            string licenseOut = Path.Combine(context.licenseRoot, "GATE_60S", spec.assemblyId ?? "invalid");
            string reviewOut = Path.Combine(context.reviewRoot, "GATE_60S", spec.assemblyId ?? "invalid");

            AuditionPvPinnedArtifact selectionSpecPin = Pending(reviewOut,
                "operator_selection_spec.json", exactSelectionBytes ?? Array.Empty<byte>(),
                otherArtifacts);

            AuditionPvAudioRightsAudioSelection[] selectedAudio = spec.audio ??
                Array.Empty<AuditionPvAudioRightsAudioSelection>();
            AuditionPvAudioRightsItemSelection[] selectedItems = spec.items ??
                Array.Empty<AuditionPvAudioRightsItemSelection>();
            bool cardinalityExceeded = false;
            if (selectedAudio.Length > MaximumAudioRows)
            {
                Add(issues, "error", "AUDIO_CARDINALITY_EXCEEDED", "audio",
                    "At most 128 audio rows are accepted.");
                cardinalityExceeded = true;
            }
            if (selectedItems.Length > MaximumRightsOrItems)
            {
                Add(issues, "error", "ITEM_CARDINALITY_EXCEEDED", "items",
                    "At most 512 non-audio item rows are accepted.");
                cardinalityExceeded = true;
            }
            int projectedRightsAndItems = selectedItems.Length + selectedAudio.Length +
                selectedAudio.Count(value => value != null && value.generatedByAi);
            if (projectedRightsAndItems > MaximumRightsOrItems)
            {
                Add(issues, "error", "OUTPUT_CARDINALITY_EXCEEDED", "fragment",
                    "The declared rows would exceed the Gate's 512-row rights/used-item bounds.");
                cardinalityExceeded = true;
            }
            AuditionPvAudioRightsCoverageSelection declaredCoverage = spec.coverage ?? new();
            if ((declaredCoverage.selectedCaptures ??
                    Array.Empty<AuditionPvAudioRightsSelectedCapture>()).Length >
                    MaximumRightsOrItems ||
                (declaredCoverage.dependencies ??
                    Array.Empty<AuditionPvRightsDependencyClassification>()).Length >
                    MaximumCaptureDependencies)
            {
                Add(issues, "error", "RIGHTS_COVERAGE_CARDINALITY_EXCEEDED", "coverage",
                    "Coverage accepts at most 512 selected captures and 4096 dependency rows.");
                cardinalityExceeded = true;
            }
            if (cardinalityExceeded)
                return Finish(spec, context, audio, rights, items, shotAudio, shotItems,
                    generationLedgers, new AuditionPvRightsCoverageReviewInput(),
                    new AuditionPvPinnedArtifact(), selectionSpecPin, issues, string.Empty, false);

            int totalCueRegions = 0;
            foreach ((AuditionPvAudioRightsAudioSelection selection, int index) in
                     selectedAudio.Select((value, index) => (value, index)))
            {
                int regions = selection?.cueRegions?.Length ?? 0;
                int alternates = selection?.generation?.alternateGeneratedWavs?.Length ?? 0;
                if (regions > MaximumCueRegionsPerRow ||
                    totalCueRegions > RequiredCues.Length - regions)
                {
                    Add(issues, "error", "AUDIO_CUE_REGION_CARDINALITY_EXCEEDED",
                        "audio[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                        "No valid selection can exceed 256 regions per row or 17 total cue roles.");
                    cardinalityExceeded = true;
                }
                else totalCueRegions += regions;
                if (alternates > MaximumAudioRows)
                {
                    Add(issues, "error", "AUDIO_AI_ALTERNATE_CARDINALITY_EXCEEDED",
                        "audio[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                        "At most 128 alternate generations are accepted per audio row.");
                    cardinalityExceeded = true;
                }
            }
            if (cardinalityExceeded)
                return Finish(spec, context, audio, rights, items, shotAudio, shotItems,
                    generationLedgers, new AuditionPvRightsCoverageReviewInput(),
                    new AuditionPvPinnedArtifact(), selectionSpecPin, issues, string.Empty, false);

            var seenAudio = new HashSet<string>(StringComparer.Ordinal);
            var seenCues = new HashSet<string>(StringComparer.Ordinal);
            foreach ((AuditionPvAudioRightsAudioSelection selection, int index) in
                     selectedAudio
                     .Select((value, index) => (value, index)))
            {
                string at = "audio[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                if (selection == null || !SafeId(selection.id) || !seenAudio.Add(selection.id))
                {
                    Add(issues, "error", "AUDIO_ID_INVALID", at,
                        "Audio IDs must be safe and unique.");
                    continue;
                }
                string[] declaredCues = (selection.cueRegions ??
                        Array.Empty<AuditionPvAudioCueRegion>())
                    .Where(value => value != null).Select(value => value.cueId).ToArray();
                if (declaredCues.Any(value => !seenCues.Add(value ?? string.Empty)))
                {
                    Add(issues, "error", "AUDIO_CUE_GLOBAL_DUPLICATE", at,
                        "Every required cue role must be supplied by exactly one audio row.");
                    continue;
                }
                BuildAudio(selection, at, context, audioOut, licenseOut, reviewOut,
                    audio, rights, items, rightArtifacts, otherArtifacts, shotAudio,
                    generationLedgers, consumedPins, issues);
            }

            var seenItems = new HashSet<string>(items.Select(value => value.id), StringComparer.Ordinal);
            var seenRights = new HashSet<string>(rights.Select(value => value.id), StringComparer.Ordinal);
            foreach ((AuditionPvAudioRightsItemSelection selection, int index) in
                     selectedItems
                     .Select((value, index) => (value, index)))
            {
                BuildItem(selection, "items[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                    context, licenseOut, items, rights, rightArtifacts, shotItems,
                    seenItems, seenRights, consumedPins, issues);
            }

            AddCoverageHolds(audio, issues);
            AuditionPvRightsCoverageReviewInput coverageInput = BuildCoverageInput(
                spec, context, items, consumedPins, issues);
            AuditionPvPinnedArtifact coveragePin = BuildCoverageReview(spec, context,
                coverageInput, items, reviewOut, otherArtifacts, issues);

            if (rights.Count > MaximumRightsOrItems || items.Count > MaximumRightsOrItems)
                Add(issues, "error", "OUTPUT_CARDINALITY_EXCEEDED", "fragment",
                    "Gate output exceeds the 512-row rights/used-item bounds.");

            // Hashes and output paths are fully determined before any writes.
            // Thus preview and write modes produce byte-identical fragments.
            var fragment = Finish(spec, context, audio, rights, items, shotAudio, shotItems,
                generationLedgers,
                coverageInput, coveragePin, selectionSpecPin, issues, string.Empty, false);
            string fragmentPath = Normalize(Path.Combine(reviewOut, "audio_rights_fragment.json"));
            fragment.fragmentPath = fragmentPath;
            fragment.fragmentSha256 = string.Empty;
            byte[] fragmentBytes = JsonBytes(fragment);
            fragment.fragmentSha256 = ByteHash(fragmentBytes);

            if (write && !issues.Any(value => value.severity == "error"))
            {
                // The stored fragment intentionally leaves its own digest field
                // empty; the returned result carries the hash of those exact
                // bytes, avoiding a circular self-hash claim.
                string returnedHash = fragment.fragmentSha256;
                fragment.fragmentSha256 = string.Empty;
                fragmentBytes = JsonBytes(fragment);
                var fragmentArtifact = new PendingArtifact
                {
                    path = fragmentPath, bytes = fragmentBytes,
                    sha256 = ByteHash(fragmentBytes)
                };
                CommitExact(rightArtifacts.Concat(otherArtifacts).Append(fragmentArtifact),
                    consumedPins, context.afterEvidenceInstallForTests);
                fragment.fragmentSha256 = returnedHash;
            }
            return fragment;
        }

        private static void BuildAudio(AuditionPvAudioRightsAudioSelection selection, string at,
            AuditionPvSixtySecondAudioRightsContext context, string audioOut, string licenseOut,
            string reviewOut, ICollection<AuditionPvSixtySecondAudioEvidence> audio,
            ICollection<AuditionPvSixtySecondRightsEvidence> rights,
            ICollection<AuditionPvSixtySecondUsedItem> items,
            ICollection<PendingArtifact> rightArtifacts, ICollection<PendingArtifact> artifacts,
            IReadOnlyDictionary<string, HashSet<string>> shotAudio,
            ICollection<AuditionPvAudioRightsGenerationLedgerBinding> generationLedgers,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (!Categories.Contains(selection.category, StringComparer.Ordinal))
            {
                Add(issues, "error", "AUDIO_CATEGORY_INVALID", at, selection.category ?? "<null>");
                return;
            }
            AuditionPvAudioCueRegion[] regions = selection.cueRegions ??
                Array.Empty<AuditionPvAudioCueRegion>();
            string[] cues = regions.Where(value => value != null).Select(value => value.cueId)
                .ToArray();
            if (regions.Length == 0 || regions.Length > MaximumCueRegionsPerRow ||
                cues.Distinct(StringComparer.Ordinal).Count() != cues.Length ||
                regions.Any(value => !AuditionPvSixtySecondGateManifestValidator.CueRegionShapeValid(value)) ||
                cues.Any(cue => !RequiredCues.Contains(cue, StringComparer.Ordinal) ||
                    CueCategory(cue) != selection.category))
            {
                Add(issues, "error", "AUDIO_CUE_SET_INVALID", at,
                    "Every row needs unique, category-correct required cue regions.");
                return;
            }
            string filePath = ResolveExternal(selection.file?.path, context.audioRoot,
                at + ".file", issues);
            string failure = string.Empty;
            if (!TryProbePinnedWave(filePath, selection.file?.sha256, regions,
                    at + ".file", consumedPins, issues, out WaveProbe probe, out failure))
            {
                if (!string.IsNullOrWhiteSpace(failure))
                    Add(issues, "error", "AUDIO_WAV_INVALID", at, failure);
                return;
            }
            if (probe.sampleRate != 48000 || probe.channels < 1 || probe.channels > 2 ||
                probe.durationMilliseconds < MinimumDuration(selection.category) ||
                probe.nonSilentSamples < Math.Max(1L,
                    (long)probe.sampleRate * probe.channels / 100L) ||
                probe.regions.Any(value => !value.hasSignal))
            {
                Add(issues, "error", "AUDIO_WAV_MEASUREMENT_FAILED", at,
                    "WAV must be 48 kHz mono/stereo, long enough, non-silent, and audible in every cue region.");
                return;
            }

            string[] coveredShots = cues.SelectMany(cue => CueShotMap[cue])
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            foreach (string shot in coveredShots) shotAudio[shot].Add(selection.id);

            AuditionPvAudioRightsRecordSelection normalizedAudioRights = NormalizeRightsSelection(
                selection.rights, context, licenseOut, artifacts, at + ".rights", false,
                consumedPins, issues);
            if (normalizedAudioRights == null) return;

            string audioItemId = "item-audio-" + selection.id;
            string audioRightId = "rights-audio-" + selection.id;
            AuditionPvPinnedArtifact finalPin = new()
                { path = Normalize(filePath), sha256 = selection.file.sha256 };
            items.Add(new AuditionPvSixtySecondUsedItem
            {
                id = audioItemId, scope = "audio", rightsRecordId = audioRightId,
                sourceLocator = finalPin.path, dependencyBinding = "external-artifact",
                artifact = Clone(finalPin)
            });

            AuditionPvPinnedArtifact listeningPin = new();
            string listeningStatus = selection.listening?.status ?? "pending";
            if (selection.listening?.judgementOrigin != HumanOperatorOrigin)
                Add(issues, "error", "AUDIO_LISTENING_ORIGIN_INVALID", at,
                    "Listening decisions must declare judgementOrigin=human-operator.");
            if (!new[] { "pending", "approved", "rejected" }.Contains(
                    listeningStatus, StringComparer.Ordinal))
                Add(issues, "error", "AUDIO_LISTENING_STATUS_INVALID", at, listeningStatus);
            else if (listeningStatus == "pending")
                Add(issues, "hold", "AUDIO_LISTENING_PENDING", at, selection.id);
            else
            {
                if (string.IsNullOrWhiteSpace(selection.listening.reviewedBy) ||
                    !Utc(selection.listening.reviewedAtUtc))
                    Add(issues, "error", "AUDIO_LISTENING_REVIEW_INVALID", at,
                        "A reviewed status requires reviewer and UTC timestamp.");
                var report = new AuditionPvAudioListeningArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator.AudioListeningSchema,
                    audioId = selection.id, fileSha256 = finalPin.sha256,
                    status = listeningStatus, reviewedBy = selection.listening.reviewedBy ?? string.Empty,
                    reviewedAtUtc = selection.listening.reviewedAtUtc ?? string.Empty
                };
                listeningPin = PendingJson(reviewOut, selection.id + "_listening.json", report, artifacts);
                if (listeningStatus == "rejected")
                    Add(issues, "error", "AUDIO_LISTENING_REJECTED", at, selection.id);
            }

            AuditionPvPinnedArtifact generationPin = new();
            string aiItemId = string.Empty;
            string aiRightId = string.Empty;
            if (selection.generatedByAi)
            {
                if (!BuildAiGeneration(selection, finalPin, at, context, audioOut, artifacts,
                        consumedPins, issues,
                        out generationPin, out AuditionPvPinnedArtifact sourceLedgerPin,
                        out AuditionPvRightsRecordArtifact aiRight)) return;
                generationLedgers.Add(new AuditionPvAudioRightsGenerationLedgerBinding
                    { audioId = selection.id, ledger = sourceLedgerPin });
                aiItemId = "item-ai-" + selection.id;
                aiRightId = "rights-ai-" + selection.id;
                aiRight.rightsRecordId = aiRightId;
                aiRight.scope = "ai";
                aiRight.coveredItemIds = new[] { aiItemId };
                aiRight.coveredShotIds = coveredShots;
                AuditionPvPinnedArtifact aiRightPin = PendingJson(licenseOut,
                    aiRightId + ".json", aiRight, rightArtifacts);
                rights.Add(new AuditionPvSixtySecondRightsEvidence
                    { id = aiRightId, scope = "ai", record = aiRightPin });
                items.Add(new AuditionPvSixtySecondUsedItem
                {
                    id = aiItemId, scope = "ai", rightsRecordId = aiRightId,
                    sourceLocator = generationPin.path, dependencyBinding = "external-artifact",
                    artifact = Clone(generationPin)
                });
            }

            AuditionPvRightsRecordArtifact audioRight = CreateRightsRecord(
                audioRightId, "audio", normalizedAudioRights, new[] { audioItemId }, coveredShots,
                at + ".rights", issues);
            if (audioRight == null) return;
            AuditionPvPinnedArtifact audioRightPin = PendingJson(licenseOut,
                audioRightId + ".json", audioRight, rightArtifacts);
            rights.Add(new AuditionPvSixtySecondRightsEvidence
                { id = audioRightId, scope = "audio", record = audioRightPin });
            audio.Add(new AuditionPvSixtySecondAudioEvidence
            {
                id = selection.id, category = selection.category, usedItemId = audioItemId,
                cueIds = cues, cueRegions = regions.Select(Clone).ToArray(), file = finalPin,
                sampleRate = probe.sampleRate, channels = probe.channels,
                generatedByAi = selection.generatedByAi, aiUsedItemId = aiItemId,
                humanListeningStatus = listeningStatus, generationManifest = generationPin,
                listeningReport = listeningPin
            });
        }

        private static bool BuildAiGeneration(AuditionPvAudioRightsAudioSelection selection,
            AuditionPvPinnedArtifact resolvedFinalPin, string at,
            AuditionPvSixtySecondAudioRightsContext context, string audioOut,
            ICollection<PendingArtifact> artifacts,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues,
            out AuditionPvPinnedArtifact generationPin,
            out AuditionPvPinnedArtifact sourceLedgerPin,
            out AuditionPvRightsRecordArtifact aiRight)
        {
            generationPin = new AuditionPvPinnedArtifact();
            sourceLedgerPin = new AuditionPvPinnedArtifact();
            aiRight = null;
            AuditionPvAudioRightsGenerationSelection value = selection.generation;
            if (value == null || string.IsNullOrWhiteSpace(value.provider) ||
                string.IsNullOrWhiteSpace(value.model) || string.IsNullOrWhiteSpace(value.accountPlan) ||
                string.IsNullOrWhiteSpace(value.tool) || string.IsNullOrWhiteSpace(value.toolVersion) ||
                !Utc(value.generatedAtUtc) || string.IsNullOrWhiteSpace(value.promptText) ||
                (value.recipeSteps ?? Array.Empty<string>()).Length == 0 ||
                value.recipeSteps.Any(string.IsNullOrWhiteSpace) ||
                !new[] { "non-real-person-imitation", "consent-documented" }.Contains(
                    value.voiceIdentityDisposition, StringComparer.Ordinal))
            {
                Add(issues, "error", "AUDIO_AI_GENERATION_INPUT_INVALID", at,
                    "AI rows require explicit provider/model/plan/tool/time/prompt/recipe/identity data.");
                return false;
            }
            string original = ResolveExternal(value.originalGeneratedWav?.path,
                context.audioRoot, at + ".generation.originalGeneratedWav", issues);
            string sourceManifestPath = ResolveExternal(value.sourceManifest?.path,
                context.audioRoot, at + ".generation.sourceManifest", issues);
            string termsPath = ResolveExternal(value.termsSnapshot?.path, context.licenseRoot,
                at + ".generation.termsSnapshot", issues);
            string evidencePath = ResolveExternal(value.generationEvidence?.path, context.licenseRoot,
                at + ".generation.generationEvidence", issues);
            string consentPath = string.Empty;
            if (!ReadPinnedText(sourceManifestPath, value.sourceManifest?.sha256,
                    MaximumJsonBytes, at + ".generation.sourceManifest", consumedPins, issues,
                    out string sourceManifestText) ||
                !PinMatches(termsPath, value.termsSnapshot?.sha256,
                    at + ".generation.termsSnapshot", consumedPins, issues) ||
                !PinMatches(evidencePath, value.generationEvidence?.sha256,
                    at + ".generation.generationEvidence", consumedPins, issues))
                return false;
            if (!TryProbePinnedWave(original, value.originalGeneratedWav?.sha256,
                    Array.Empty<AuditionPvAudioCueRegion>(),
                    at + ".generation.originalGeneratedWav", consumedPins, issues,
                    out WaveProbe originalProbe, out string originalFailure) ||
                originalProbe.sampleRate != 48000 || originalProbe.channels < 1 ||
                originalProbe.channels > 2 || originalProbe.durationMilliseconds < 100 ||
                originalProbe.nonSilentSamples < Math.Max(1L,
                    (long)originalProbe.sampleRate * originalProbe.channels / 100L))
            {
                Add(issues, "error", "AUDIO_AI_ORIGINAL_INVALID", at,
                    originalFailure ?? string.Empty);
                return false;
            }
            var alternates = new List<AuditionPvPinnedArtifact>();
            if ((value.alternateGeneratedWavs ?? Array.Empty<AuditionPvPinnedArtifact>()).Length >
                MaximumAudioRows)
            {
                Add(issues, "error", "AUDIO_AI_ALTERNATE_CARDINALITY_EXCEEDED", at,
                    "At most 128 alternate generations are accepted per audio row.");
                return false;
            }
            foreach ((AuditionPvPinnedArtifact alternate, int index) in
                     (value.alternateGeneratedWavs ?? Array.Empty<AuditionPvPinnedArtifact>())
                     .Select((item, index) => (item, index)))
            {
                string alternateAt = at + ".generation.alternateGeneratedWavs[" +
                    index.ToString(CultureInfo.InvariantCulture) + "]";
                string alternatePath = ResolveExternal(alternate?.path, context.audioRoot,
                    alternateAt, issues);
                string alternateFailure = string.Empty;
                if (!TryProbePinnedWave(alternatePath, alternate?.sha256,
                        Array.Empty<AuditionPvAudioCueRegion>(), alternateAt, consumedPins, issues,
                        out WaveProbe alternateProbe, out alternateFailure) ||
                    alternateProbe.sampleRate != 48000 || alternateProbe.channels < 1 ||
                    alternateProbe.channels > 2 || alternateProbe.durationMilliseconds < 100 ||
                    alternateProbe.nonSilentSamples < Math.Max(1L,
                        (long)alternateProbe.sampleRate * alternateProbe.channels / 100L))
                {
                    Add(issues, "error", "AUDIO_AI_ALTERNATE_INVALID", alternateAt,
                        alternateFailure ?? string.Empty);
                    return false;
                }
                alternates.Add(new AuditionPvPinnedArtifact
                    { path = Normalize(alternatePath), sha256 = alternate.sha256 });
            }
            if (alternates.Select(item => item.sha256).Distinct(StringComparer.Ordinal).Count() !=
                    alternates.Count || alternates.Any(item => item.sha256 == selection.file.sha256))
            {
                Add(issues, "error", "AUDIO_AI_ALTERNATE_SET_INVALID", at,
                    "Alternate WAVs must be unique and must not repeat the selected final WAV.");
                return false;
            }
            if (!TextContainsPins(sourceManifestText,
                    new[] { selection.file.sha256, value.originalGeneratedWav.sha256 }
                    .Concat(alternates.Select(item => item.sha256))))
            {
                Add(issues, "error", "AUDIO_AI_SOURCE_MANIFEST_MISMATCH", at,
                    "The pinned source manifest must contain selected, original, and alternate WAV hashes.");
                return false;
            }
            AuditionPvPinnedArtifact sourceRecipePin = new();
            bool derivative = selection.file.sha256 != value.originalGeneratedWav.sha256;
            if (derivative)
            {
                string sourceRecipePath = ResolveExternal(value.sourceDerivationRecipe?.path,
                    context.audioRoot, at + ".generation.sourceDerivationRecipe", issues);
                if (!ReadPinnedText(sourceRecipePath, value.sourceDerivationRecipe?.sha256,
                        MaximumJsonBytes, at + ".generation.sourceDerivationRecipe",
                        consumedPins, issues,
                        out string sourceRecipeText) ||
                    !TextContainsPins(sourceRecipeText,
                        new[] { value.originalGeneratedWav.sha256, selection.file.sha256 }) ||
                    (value.sourceDerivationRecipe.sha256 != value.sourceManifest.sha256 &&
                     !TextContainsPins(sourceManifestText,
                         new[] { value.sourceDerivationRecipe.sha256 })))
                {
                    Add(issues, "error", "AUDIO_AI_SOURCE_RECIPE_MISMATCH", at,
                        "A derivative must pin a recipe that binds its original and edited WAV hashes.");
                    return false;
                }
                sourceRecipePin = new AuditionPvPinnedArtifact
                    { path = Normalize(sourceRecipePath), sha256 = value.sourceDerivationRecipe.sha256 };
            }
            if (value.voiceIdentityDisposition == "consent-documented")
                consentPath = ResolveExternal(value.consentArtifact?.path, context.licenseRoot,
                    at + ".generation.consentArtifact", issues);
            if (value.voiceIdentityDisposition == "consent-documented" &&
                !PinMatches(consentPath,
                    value.consentArtifact?.sha256, at + ".generation.consentArtifact",
                    consumedPins, issues))
                return false;

            AuditionPvPinnedArtifact promptPin = PendingText(audioOut,
                selection.id + "_prompt.txt", value.promptText, artifacts);
            var recipe = new AuditionPvAudioDerivationRecipeArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.AudioDerivationRecipeSchema,
                audioId = selection.id, promptSha256 = promptPin.sha256,
                originalWavSha256 = value.originalGeneratedWav.sha256,
                editedWavSha256 = selection.file.sha256, tool = value.tool,
                toolVersion = value.toolVersion, createdAtUtc = value.generatedAtUtc,
                steps = value.recipeSteps.ToArray()
            };
            AuditionPvPinnedArtifact recipePin = PendingJson(audioOut,
                selection.id + "_derivation.json", recipe, artifacts);
            string aiRightId = "rights-ai-" + selection.id;
            var generation = new AuditionPvAudioGenerationArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.AudioGenerationSchema,
                audioId = selection.id, aiUsedItemId = "item-ai-" + selection.id,
                provider = value.provider, model = value.model, rightsRecordId = aiRightId,
                accountPlan = value.accountPlan, tool = value.tool, toolVersion = value.toolVersion,
                generatedAtUtc = value.generatedAtUtc,
                voiceIdentityDisposition = value.voiceIdentityDisposition,
                promptArtifact = promptPin,
                originalGeneratedWav = new AuditionPvPinnedArtifact
                    { path = Normalize(original), sha256 = value.originalGeneratedWav.sha256 },
                editedWav = Clone(resolvedFinalPin),
                derivationRecipe = recipePin,
                consentArtifact = value.voiceIdentityDisposition == "consent-documented"
                    ? new AuditionPvPinnedArtifact
                        { path = Normalize(consentPath), sha256 = value.consentArtifact.sha256 }
                    : new AuditionPvPinnedArtifact()
            };
            generationPin = PendingJson(audioOut, selection.id + "_generation.json",
                generation, artifacts);
            sourceLedgerPin = PendingJson(audioOut, selection.id + "_source_ledger.json",
                new AuditionPvAudioGenerationSourceLedgerArtifact
                {
                    schemaVersion = "dimension-brawl.audition-pv.audio-generation-source-ledger.v1",
                    audioId = selection.id, provider = value.provider, model = value.model,
                    generatedAtUtc = value.generatedAtUtc,
                    sourceManifest = new AuditionPvPinnedArtifact
                        { path = Normalize(sourceManifestPath), sha256 = value.sourceManifest.sha256 },
                    sourceDerivationRecipe = sourceRecipePin,
                    selectedEditedWav = Clone(resolvedFinalPin),
                    originalGeneratedWav = new AuditionPvPinnedArtifact
                        { path = Normalize(original), sha256 = value.originalGeneratedWav.sha256 },
                    alternateGeneratedWavs = alternates.ToArray()
                }, artifacts);
            aiRight = CreateRightsRecord(aiRightId, "ai", new AuditionPvAudioRightsRecordSelection
            {
                disposition = "ai-generated", verified = selection.rights?.verified ?? false,
                judgementOrigin = selection.rights?.judgementOrigin ?? string.Empty,
                verifiedBy = selection.rights?.verifiedBy ?? string.Empty,
                verifiedAtUtc = selection.rights?.verifiedAtUtc ?? string.Empty,
                useBoundary = selection.rights?.useBoundary ?? string.Empty,
                provider = value.provider, accountPlan = value.accountPlan,
                termsSnapshot = new AuditionPvPinnedArtifact
                    { path = Normalize(termsPath), sha256 = value.termsSnapshot.sha256 },
                generationEvidence = new AuditionPvPinnedArtifact
                    { path = Normalize(evidencePath), sha256 = value.generationEvidence.sha256 },
                attributionRequired = selection.rights?.attributionRequired ?? false,
                attributionArtifact = Clone(selection.rights?.attributionArtifact)
            }, Array.Empty<string>(), Array.Empty<string>(), at + ".generation.aiRights", issues);
            return aiRight != null;
        }

        private static void BuildItem(AuditionPvAudioRightsItemSelection selection, string at,
            AuditionPvSixtySecondAudioRightsContext context, string licenseOut,
            ICollection<AuditionPvSixtySecondUsedItem> items,
            ICollection<AuditionPvSixtySecondRightsEvidence> rights,
            ICollection<PendingArtifact> artifacts,
            IReadOnlyDictionary<string, HashSet<string>> shotItems,
            ISet<string> seenItems, ISet<string> seenRights,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (selection == null || !SafeId(selection.id) || !seenItems.Add(selection.id) ||
                !new[] { "asset", "font" }.Contains(selection.scope, StringComparer.Ordinal))
            {
                Add(issues, "error", "ITEM_SELECTION_INVALID", at,
                    "Non-audio item IDs must be unique and use asset/font scope.");
                return;
            }
            string[] shots = (selection.atomicShotIds ?? Array.Empty<string>())
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (shots.Length == 0 || shots.Any(value => !AtomicShotIds.Contains(value,
                    StringComparer.Ordinal)))
            {
                Add(issues, "error", "ITEM_SHOT_BINDING_INVALID", at,
                    "Every selected item needs explicit valid atomic shot IDs.");
                return;
            }
            bool unity = selection.dependencyBinding == "unity-dependency";
            bool external = selection.dependencyBinding == "external-artifact";
            if (!unity && !external)
            {
                Add(issues, "error", "ITEM_DEPENDENCY_BINDING_INVALID", at,
                    selection.dependencyBinding ?? "<null>");
                return;
            }
            string physical = unity
                ? ResolveUnity(selection.sourceLocator, context.projectRoot, at, issues)
                : ResolveExternal(selection.sourceLocator, context.reviewRoot, at, issues);
            if (!PinMatches(physical, selection.expectedSha256, at + ".artifact",
                    consumedPins, issues)) return;

            if (!ValidateAdmissionProfile(selection, physical, context, at, consumedPins, issues))
                return;
            AuditionPvAudioRightsRecordSelection normalizedRights = NormalizeRightsSelection(
                selection.rights, context, licenseOut, artifacts, at + ".rights",
                selection.admissionProfile == "pretendard-ofl-1.1", consumedPins, issues);
            if (normalizedRights == null) return;
            string rightId = "rights-" + selection.id;
            if (!seenRights.Add(rightId))
            {
                Add(issues, "error", "RIGHTS_ID_DUPLICATE", at, rightId);
                return;
            }
            AuditionPvRightsRecordArtifact record = CreateRightsRecord(rightId, selection.scope,
                normalizedRights, new[] { selection.id }, shots, at + ".rights", issues);
            if (record == null) return;
            AuditionPvPinnedArtifact recordPin = PendingJson(licenseOut, rightId + ".json",
                record, artifacts);
            string locator = unity ? Normalize(selection.sourceLocator) : Normalize(physical);
            items.Add(new AuditionPvSixtySecondUsedItem
            {
                id = selection.id, scope = selection.scope, rightsRecordId = rightId,
                sourceLocator = locator, dependencyBinding = selection.dependencyBinding,
                artifact = new AuditionPvPinnedArtifact
                    { path = locator, sha256 = selection.expectedSha256 }
            });
            rights.Add(new AuditionPvSixtySecondRightsEvidence
                { id = rightId, scope = selection.scope, record = recordPin });
            foreach (string shot in shots) shotItems[shot].Add(selection.id);
        }

        private static bool ValidateAdmissionProfile(AuditionPvAudioRightsItemSelection value,
            string physical, AuditionPvSixtySecondAudioRightsContext context, string at,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            string profile = value.admissionProfile ?? string.Empty;
            if (profile == "project-authored")
            {
                bool valid = value.rights?.disposition == "project-authored";
                if (!valid)
                    Add(issues, "error", "PROJECT_AUTHORED_ADMISSION_INVALID", at,
                        "The project-authored profile requires disposition=project-authored.");
                return valid;
            }
            if (profile == "pretendard-ofl-1.1")
            {
                bool path = value.scope == "font" && Normalize(value.sourceLocator)
                    .StartsWith("Assets/_Game/Art/Fonts/Pretendard/", StringComparison.Ordinal);
                string terms = ResolveExternal(value.rights?.termsSnapshot?.path,
                    context.projectRoot, at + ".rights.termsSnapshot", issues, allowProjectRoot: true);
                bool pin = ReadPinnedText(terms, value.rights?.termsSnapshot?.sha256,
                    2L * 1024L * 1024L, at + ".rights.termsSnapshot", consumedPins, issues,
                    out string licenseText);
                bool text = pin && licenseText.Contains("SIL OPEN FONT LICENSE Version 1.1");
                if (!path || value.rights?.disposition != "open-license" || !text)
                    Add(issues, "error", "PRETENDARD_ADMISSION_INVALID", at,
                        "Pretendard needs its exact project font and OFL 1.1 license pin.");
                return path && value.rights?.disposition == "open-license" && text;
            }
            if (profile == "tokyo-street-single-entity")
            {
                bool path = value.scope == "asset" && value.dependencyBinding == "unity-dependency" &&
                    Normalize(value.sourceLocator).StartsWith(
                        "Assets/_Game/Art/Environment/CityHeroPocket/TokyoStreet/",
                        StringComparison.Ordinal);
                string admission = ResolveExternal(value.rights?.termsSnapshot?.path,
                    context.licenseRoot, at + ".rights.termsSnapshot", issues);
                string entitlement = ResolveExternal(value.rights?.entitlementEvidence?.path,
                    context.licenseRoot, at + ".rights.entitlementEvidence", issues);
                bool pins = ReadPinnedText(admission, value.rights?.termsSnapshot?.sha256,
                                4L * 1024L * 1024L, at + ".rights.termsSnapshot",
                                consumedPins, issues,
                                out string admissionText) &
                    PinMatches(entitlement, value.rights?.entitlementEvidence?.sha256,
                        at + ".rights.entitlementEvidence", consumedPins, issues);
                bool admitted = pins && admissionText.Contains("Tokyo Street") &&
                    admissionText.Contains("ADMITTED_FOR_ISOLATED_STAGING");
                if (!path || value.rights?.disposition != "purchased" || !admitted)
                    Add(issues, "error", "TOKYO_STREET_ADMISSION_INVALID", at,
                        "Tokyo Street needs the exact curated dependency, admission record, and entitlement pin.");
                return path && value.rights?.disposition == "purchased" && admitted;
            }
            Add(issues, "error", "ADMISSION_PROFILE_INVALID", at, profile);
            return false;
        }

        private static AuditionPvAudioRightsRecordSelection NormalizeRightsSelection(
            AuditionPvAudioRightsRecordSelection source,
            AuditionPvSixtySecondAudioRightsContext context, string licenseOut,
            ICollection<PendingArtifact> artifacts, string at, bool copyTermsFromProject,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (source == null)
            {
                Add(issues, "error", "RIGHTS_SELECTION_MISSING", at,
                    "Every used item requires an explicit human rights decision.");
                return null;
            }
            var result = new AuditionPvAudioRightsRecordSelection
            {
                disposition = source.disposition ?? string.Empty,
                judgementOrigin = source.judgementOrigin ?? string.Empty,
                verified = source.verified, verifiedBy = source.verifiedBy ?? string.Empty,
                verifiedAtUtc = source.verifiedAtUtc ?? string.Empty,
                useBoundary = source.useBoundary ?? string.Empty,
                provider = source.provider ?? string.Empty, licenseId = source.licenseId ?? string.Empty,
                licenseVersion = source.licenseVersion ?? string.Empty,
                accountEntitlementId = source.accountEntitlementId ?? string.Empty,
                owner = source.owner ?? string.Empty,
                sourceDescription = source.sourceDescription ?? string.Empty,
                accountPlan = source.accountPlan ?? string.Empty,
                exclusionReason = source.exclusionReason ?? string.Empty,
                attributionRequired = source.attributionRequired
            };
            bool needsTerms = source.disposition == "open-license" ||
                source.disposition == "purchased" || source.disposition == "ai-generated";
            if (needsTerms)
            {
                string root = copyTermsFromProject ? context.projectRoot : context.licenseRoot;
                string path = ResolveExternal(source.termsSnapshot?.path, root,
                    at + ".termsSnapshot", issues, copyTermsFromProject);
                if (!PinMatches(path, source.termsSnapshot?.sha256,
                        at + ".termsSnapshot", consumedPins, issues)) return null;
                if (copyTermsFromProject)
                {
                    if (!ReadPinnedBytes(path, source.termsSnapshot?.sha256, MaximumJsonBytes,
                            at + ".termsSnapshot.copy", consumedPins, issues,
                            out byte[] exactTerms)) return null;
                    result.termsSnapshot = Pending(licenseOut,
                        "source_" + source.termsSnapshot.sha256.Substring(0, 12) + "_" +
                        Path.GetFileName(path), exactTerms, artifacts);
                }
                else result.termsSnapshot = new AuditionPvPinnedArtifact
                    { path = Normalize(path), sha256 = source.termsSnapshot.sha256 };
            }
            if (source.disposition == "purchased")
            {
                string path = ResolveExternal(source.entitlementEvidence?.path, context.licenseRoot,
                    at + ".entitlementEvidence", issues);
                if (!PinMatches(path, source.entitlementEvidence?.sha256,
                        at + ".entitlementEvidence", consumedPins, issues)) return null;
                result.entitlementEvidence = new AuditionPvPinnedArtifact
                    { path = Normalize(path), sha256 = source.entitlementEvidence.sha256 };
            }
            if (source.disposition == "ai-generated")
            {
                string path = ResolveExternal(source.generationEvidence?.path, context.licenseRoot,
                    at + ".generationEvidence", issues);
                if (!PinMatches(path, source.generationEvidence?.sha256,
                        at + ".generationEvidence", consumedPins, issues)) return null;
                result.generationEvidence = new AuditionPvPinnedArtifact
                    { path = Normalize(path), sha256 = source.generationEvidence.sha256 };
            }
            if (source.attributionRequired || !string.IsNullOrWhiteSpace(
                    source.attributionArtifact?.path))
            {
                string path = ResolveExternal(source.attributionArtifact?.path, context.licenseRoot,
                    at + ".attributionArtifact", issues);
                if (!PinMatches(path, source.attributionArtifact?.sha256,
                        at + ".attributionArtifact", consumedPins, issues)) return null;
                result.attributionArtifact = new AuditionPvPinnedArtifact
                    { path = Normalize(path), sha256 = source.attributionArtifact.sha256 };
            }
            return result;
        }

        private static AuditionPvRightsRecordArtifact CreateRightsRecord(string id, string scope,
            AuditionPvAudioRightsRecordSelection source, string[] itemIds, string[] shotIds,
            string at, ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (source == null || source.judgementOrigin != HumanOperatorOrigin ||
                !source.verified || string.IsNullOrWhiteSpace(source.verifiedBy) ||
                !Utc(source.verifiedAtUtc) || string.IsNullOrWhiteSpace(source.useBoundary))
            {
                Add(issues, "error", "RIGHTS_VERIFICATION_MISSING", at,
                    "Rights rows require judgementOrigin=human-operator, an explicit verified operator, UTC time, and use boundary.");
                return null;
            }
            var result = new AuditionPvRightsRecordArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.RightsRecordSchema,
                rightsRecordId = id, scope = scope, disposition = source.disposition ?? string.Empty,
                verified = source.verified, verifiedBy = source.verifiedBy,
                verifiedAtUtc = source.verifiedAtUtc, useBoundary = source.useBoundary,
                provider = source.provider ?? string.Empty, licenseId = source.licenseId ?? string.Empty,
                licenseVersion = source.licenseVersion ?? string.Empty,
                accountEntitlementId = source.accountEntitlementId ?? string.Empty,
                owner = source.owner ?? string.Empty, sourceDescription = source.sourceDescription ?? string.Empty,
                accountPlan = source.accountPlan ?? string.Empty,
                exclusionReason = source.exclusionReason ?? string.Empty,
                attributionRequired = source.attributionRequired,
                termsSnapshot = Clone(source.termsSnapshot),
                entitlementEvidence = Clone(source.entitlementEvidence),
                attributionArtifact = Clone(source.attributionArtifact),
                generationEvidence = Clone(source.generationEvidence),
                coveredItemIds = itemIds.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                coveredShotIds = shotIds.OrderBy(value => value, StringComparer.Ordinal).ToArray()
            };
            if (!AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(result))
            {
                Add(issues, "error", "RIGHTS_METADATA_INVALID", at,
                    source.disposition ?? "<null>");
                return null;
            }
            return result;
        }

        private static void AddCoverageHolds(
            IEnumerable<AuditionPvSixtySecondAudioEvidence> values,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            AuditionPvSixtySecondAudioEvidence[] audio = values.ToArray();
            var categories = new HashSet<string>(audio.Select(value => value.category),
                StringComparer.Ordinal);
            var cues = new HashSet<string>(audio.SelectMany(value => value.cueIds ??
                Array.Empty<string>()), StringComparer.Ordinal);
            foreach (string category in Categories.Where(value => !categories.Contains(value)))
                Add(issues, "hold", "AUDIO_CATEGORY_MISSING", "audio", category);
            foreach (string cue in RequiredCues.Where(value => !cues.Contains(value)))
                Add(issues, "hold", "AUDIO_REQUIRED_CUE_MISSING", "audio", cue);
        }

        private static AuditionPvRightsCoverageReviewInput BuildCoverageInput(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            AuditionPvSixtySecondAudioRightsContext context,
            IReadOnlyCollection<AuditionPvSixtySecondUsedItem> items,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            AuditionPvAudioRightsCoverageSelection selection = spec.coverage ?? new();
            var input = new AuditionPvRightsCoverageReviewInput
            {
                schemaVersion = CoverageInputSchema, manifestId = spec.manifestId,
                productCheckpointGitSha = spec.productCheckpointGitSha,
                judgementOrigin = selection.judgementOrigin ?? string.Empty,
                approvalRequested = selection.approveComplete,
                reviewedBy = selection.reviewedBy ?? string.Empty,
                reviewedAtUtc = selection.reviewedAtUtc ?? string.Empty,
                usedItemIds = items.Select(value => value.id)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                selectedCaptures = (selection.selectedCaptures ??
                    Array.Empty<AuditionPvAudioRightsSelectedCapture>()).ToArray(),
                dependencies = (selection.dependencies ??
                    Array.Empty<AuditionPvRightsDependencyClassification>()).ToArray(),
                approvedComposeInput = Clone(selection.approvedComposeInput)
            };
            if (input.selectedCaptures.Length > MaximumRightsOrItems ||
                input.dependencies.Length > MaximumCaptureDependencies)
            {
                input.status = "hold-coverage-cardinality-exceeded";
                Add(issues, "error", "RIGHTS_COVERAGE_CARDINALITY_EXCEEDED", "coverage",
                    "Coverage accepts at most 512 selected captures and 4096 dependency rows.");
                return input;
            }
            if (selection.judgementOrigin != HumanOperatorOrigin)
            {
                input.status = "hold-human-operator-origin-missing";
                Add(issues, "error", "RIGHTS_COVERAGE_ORIGIN_INVALID", "coverage",
                    "Coverage decisions must declare judgementOrigin=human-operator.");
                return input;
            }
            if (!selection.approveComplete)
            {
                input.status = "hold-selected-capture-review-not-approved";
                Add(issues, "hold", "RIGHTS_COVERAGE_REVIEW_PENDING", "coverage",
                    "Selected-capture dependency closure has not been approved.");
                return input;
            }
            if (string.IsNullOrWhiteSpace(selection.reviewedBy) || !Utc(selection.reviewedAtUtc) ||
                input.selectedCaptures.Length == 0 || input.dependencies.Length == 0 ||
                !AuditionPvSha256.IsSha256(selection.approvedComposeInput?.sha256) ||
                string.IsNullOrWhiteSpace(selection.approvedComposeInput?.path))
            {
                input.status = "hold-selected-capture-input-incomplete";
                Add(issues, "hold", "RIGHTS_COVERAGE_INPUT_INCOMPLETE", "coverage",
                    "Completion requires reviewer/time, selected capture pins, and classifications.");
                return input;
            }

            var expected = new Dictionary<string, AuditionPvRightsDependencyClassification>(
                StringComparer.Ordinal);
            var captureIds = new HashSet<string>(StringComparer.Ordinal);
            if (!ApprovedSelectionSourceMatches(spec, context, input.selectedCaptures,
                    selection.approvedComposeInput, consumedPins, issues))
            {
                input.status = "hold-approved-take-binding-invalid";
                return input;
            }
            foreach (AuditionPvAudioRightsSelectedCapture capture in input.selectedCaptures)
            {
                if (!LoadSelectedCapture(capture, spec.productCheckpointGitSha, context,
                        expected, captureIds, consumedPins, issues))
                {
                    input.status = "hold-selected-capture-invalid";
                    return input;
                }
            }
            var itemIndex = items.ToDictionary(value => value.id, StringComparer.Ordinal);
            string[] expectedIds = expected.Values.Select(DependencyIdentity)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] actualIds = input.dependencies.Select(DependencyIdentity)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            bool unique = actualIds.Distinct(StringComparer.Ordinal).Count() == actualIds.Length;
            bool shapes = input.dependencies.All(value =>
                AuditionPvSixtySecondGateManifestValidator
                    .RightsDependencyClassificationShapeValid(value, itemIndex));
            if (!unique || !shapes || !actualIds.SequenceEqual(expectedIds, StringComparer.Ordinal))
            {
                input.status = "hold-dependency-closure-mismatch";
                Add(issues, "hold", "RIGHTS_DEPENDENCY_CLOSURE_MISMATCH", "coverage",
                    "Every exact dependency from every selected capture needs one truthful classification.");
                return input;
            }
            input.status = "exact-closure-reviewed";
            input.exactClosure = true;
            return input;
        }

        private static AuditionPvPinnedArtifact BuildCoverageReview(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            AuditionPvSixtySecondAudioRightsContext context,
            AuditionPvRightsCoverageReviewInput input,
            IReadOnlyCollection<AuditionPvSixtySecondUsedItem> items, string reviewOut,
            ICollection<PendingArtifact> artifacts,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (input == null || !input.exactClosure) return new AuditionPvPinnedArtifact();
            AuditionPvRightsReviewedCaptureIdentity[] captures = input.selectedCaptures
                .Select(value => new AuditionPvRightsReviewedCaptureIdentity
                {
                    captureId = value.captureId,
                    sourceManifestSha256 = value.sourceManifest?.sha256 ?? string.Empty,
                    sourceDependencyIdentitySha256 = value.sourceDependencyIdentitySha256
                })
                .GroupBy(value => string.Join("\0", value.captureId,
                    value.sourceManifestSha256, value.sourceDependencyIdentitySha256),
                    StringComparer.Ordinal).Select(value => value.First())
                .OrderBy(value => string.Join("\0", value.captureId,
                    value.sourceManifestSha256, value.sourceDependencyIdentitySha256),
                    StringComparer.Ordinal).ToArray();
            var review = new AuditionPvRightsCoverageReviewArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.RightsCoverageReviewSchema,
                manifestId = spec.manifestId,
                productCheckpointGitSha = spec.productCheckpointGitSha,
                reviewedBy = input.reviewedBy, reviewedAtUtc = input.reviewedAtUtc,
                complete = true,
                usedItemIds = items.Select(value => value.id)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                reviewedCaptures = captures,
                dependencies = input.dependencies.OrderBy(DependencyIdentity,
                    StringComparer.Ordinal).ToArray()
            };
            return PendingJson(reviewOut, "rights_coverage_review.json", review, artifacts);
        }

        private static bool ApprovedSelectionSourceMatches(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            AuditionPvSixtySecondAudioRightsContext context,
            AuditionPvAudioRightsSelectedCapture[] selectedCaptures,
            AuditionPvPinnedArtifact approvalPin,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            string path = ResolveExternal(approvalPin?.path, context.reviewRoot,
                "coverage.approvedComposeInput", issues);
            if (!ReadPinnedText(path, approvalPin?.sha256, MaximumJsonBytes,
                    "coverage.approvedComposeInput", consumedPins, issues, out string json))
                return false;
            AuditionPvSixtySecondProductionComposeInput input;
            try { input = JsonUtility.FromJson<AuditionPvSixtySecondProductionComposeInput>(json); }
            catch (Exception exception) when (exception is ArgumentException ||
                                              exception is InvalidOperationException)
            {
                Add(issues, "hold", "RIGHTS_APPROVAL_INPUT_INVALID", "coverage",
                    exception.Message);
                return false;
            }
            if (input == null || input.schemaVersion !=
                    AuditionPvSixtySecondProductionComposer.InputSchema ||
                input.productCheckpointGitSha != spec.productCheckpointGitSha)
            {
                Add(issues, "hold", "RIGHTS_APPROVAL_INPUT_IDENTITY_MISMATCH", "coverage",
                    "The pinned approval compose input must match schema and product checkpoint.");
                return false;
            }

            if ((input.captureManifestPaths ?? Array.Empty<string>()).Length >
                    MaximumRightsOrItems ||
                (input.takeEvidence ??
                    Array.Empty<AuditionPvSixtySecondTakeEvidenceBinding>()).Length >
                    MaximumRightsOrItems)
            {
                Add(issues, "hold", "RIGHTS_APPROVAL_INPUT_CARDINALITY_EXCEEDED", "coverage",
                    "Approval input exceeds the bounded capture/take evidence limits.");
                return false;
            }
            AuditionPvSixtySecondTakeEvidenceBinding[] bindings = input.takeEvidence ??
                Array.Empty<AuditionPvSixtySecondTakeEvidenceBinding>();
            string[] movingShots = AtomicShotIds.Where(value => value != "pv-s100-end-card").ToArray();
            bool normalShape = movingShots.All(shot => bindings.Count(value => value != null &&
                value.atomicShotId == shot && value.approved && !value.cleanPlate) == 1);
            AuditionPvSixtySecondTakeEvidenceBinding[] selectedBindings = bindings.Where(value =>
                value != null && (value.approved || value.cleanPlate)).ToArray();
            bool selectedShape = selectedBindings.Length == movingShots.Length + 1 &&
                selectedBindings.All(value =>
                    movingShots.Contains(value.atomicShotId, StringComparer.Ordinal) &&
                    !string.IsNullOrWhiteSpace(value.sourceCaptureId) &&
                    ((value.approved && !value.cleanPlate) ||
                     (!value.approved && value.cleanPlate &&
                      value.atomicShotId == "pv-s060-eye-open"))) &&
                selectedBindings.Count(value => value.cleanPlate) == 1;
            if (!normalShape || !selectedShape)
            {
                Add(issues, "hold", "RIGHTS_APPROVAL_SELECTED_TAKES_INVALID", "coverage",
                    "Approval input must bind one approved take for 13 moving shots and the exact eye-open clean plate.");
                return false;
            }

            string[] expectedIds = selectedBindings.Select(value => value.sourceCaptureId)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] suppliedIds = (selectedCaptures ??
                    Array.Empty<AuditionPvAudioRightsSelectedCapture>())
                .Where(value => value != null).Select(value => value.captureId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (suppliedIds.Distinct(StringComparer.Ordinal).Count() != suppliedIds.Length ||
                !suppliedIds.SequenceEqual(expectedIds, StringComparer.Ordinal))
            {
                Add(issues, "hold", "RIGHTS_SELECTED_CAPTURE_SET_MISMATCH", "coverage",
                    "Selected capture pins must exactly match the approval input's chosen takes and clean plate.");
                return false;
            }

            var declaredPaths = new HashSet<string>((input.captureManifestPaths ??
                    Array.Empty<string>()).Select(value => FullOrEmpty(value))
                .Where(value => !string.IsNullOrEmpty(value)), StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvAudioRightsSelectedCapture selected in selectedCaptures)
            {
                string selectedPath = ResolveAnyRoot(selected.sourceManifest?.path,
                    context.captureRoots, "coverage.sourceManifest", issues);
                if (string.IsNullOrEmpty(selectedPath) || !declaredPaths.Contains(selectedPath))
                {
                    Add(issues, "hold", "RIGHTS_SELECTED_CAPTURE_NOT_IN_APPROVAL", "coverage",
                        selected?.captureId ?? "<null>");
                    return false;
                }
            }
            return true;
        }

        private static bool LoadSelectedCapture(AuditionPvAudioRightsSelectedCapture selected,
            string productCheckpointGitSha,
            AuditionPvSixtySecondAudioRightsContext context,
            IDictionary<string, AuditionPvRightsDependencyClassification> expected,
            ISet<string> captureIds,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (selected == null || string.IsNullOrWhiteSpace(selected.captureId) ||
                !captureIds.Add(selected.captureId) ||
                !AuditionPvSha256.IsSha256(selected.sourceDependencyIdentitySha256))
            {
                Add(issues, "hold", "RIGHTS_SELECTED_CAPTURE_INVALID", "coverage", "shape");
                return false;
            }
            string path = ResolveAnyRoot(selected.sourceManifest?.path, context.captureRoots,
                "coverage.sourceManifest", issues);
            AuditionPvCaptureManifest manifest;
            if (!ReadPinnedText(path, selected.sourceManifest?.sha256, MaximumJsonBytes,
                    "coverage.sourceManifest", consumedPins, issues, out string json)) return false;
            try
            {
                manifest = JsonUtility.FromJson<AuditionPvCaptureManifest>(json);
                if (manifest == null ||
                    (manifest.shots ?? Array.Empty<AuditionPvShotManifestEntry>()).Length >
                        MaximumCaptureShots ||
                    (manifest.baselines ?? Array.Empty<AuditionPvBaselineManifestEntry>()).Length >
                        MaximumCaptureBaselines ||
                    (manifest.dependencyHashes ?? Array.Empty<AuditionPvDependencyHash>()).Length >
                        MaximumCaptureDependencies ||
                    (manifest.testResults ?? Array.Empty<AuditionPvTestResult>()).Length >
                        MaximumCaptureTestResults)
                {
                    Add(issues, "hold", "RIGHTS_SELECTED_CAPTURE_UNREADABLE", "coverage",
                        "Capture manifest cardinality limit exceeded.");
                    return false;
                }
                AuditionPvCaptureManifestWriter.Validate(manifest);
            }
            catch (Exception exception) when (IsIo(exception) ||
                                              exception is InvalidOperationException)
            {
                Add(issues, "hold", "RIGHTS_SELECTED_CAPTURE_UNREADABLE", "coverage",
                    exception.Message);
                return false;
            }
            string canonical = manifest == null ? string.Empty : FullOrEmpty(Path.Combine(
                manifest.outputDirectory ?? string.Empty, AuditionPvCaptureContract.ManifestFileName));
            if (manifest == null || manifest.captureId != selected.captureId ||
                manifest.gitWorktreeDirty || manifest.gitCommitSha != productCheckpointGitSha ||
                !IsFullGitSha(manifest.gitCommitSha) ||
                !string.Equals(path, canonical, StringComparison.OrdinalIgnoreCase) ||
                (manifest.testResults ?? Array.Empty<AuditionPvTestResult>())
                    .Any(value => value == null || value.status != "passed") ||
                (manifest.dependencyHashes ?? Array.Empty<AuditionPvDependencyHash>()).Length == 0 ||
                DependencyDigest(manifest.dependencyHashes) != selected.sourceDependencyIdentitySha256)
            {
                Add(issues, "hold", "RIGHTS_SELECTED_CAPTURE_IDENTITY_MISMATCH", "coverage",
                    selected.captureId);
                return false;
            }
            foreach (AuditionPvDependencyHash dependency in manifest.dependencyHashes ??
                         Array.Empty<AuditionPvDependencyHash>())
            {
                if (dependency == null || !dependency.exists || dependency.byteLength < 0 ||
                    !AuditionPvSha256.IsSha256(dependency.sha256) ||
                    string.IsNullOrWhiteSpace(dependency.path))
                {
                    Add(issues, "hold", "RIGHTS_CAPTURE_DEPENDENCY_INVALID", "coverage",
                        selected.captureId);
                    return false;
                }
                var row = new AuditionPvRightsDependencyClassification
                {
                    captureId = manifest.captureId,
                    sourceManifestSha256 = selected.sourceManifest.sha256,
                    path = Normalize(dependency.path), byteLength = dependency.byteLength,
                    sha256 = dependency.sha256
                };
                string identity = DependencyIdentity(row);
                if (!expected.ContainsKey(identity) &&
                    expected.Count >= MaximumCaptureDependencies)
                {
                    Add(issues, "hold", "RIGHTS_DEPENDENCY_TOTAL_CARDINALITY_EXCEEDED",
                        "coverage", "Selected captures contain more than 4096 exact dependencies.");
                    return false;
                }
                expected.TryAdd(identity, row);
            }
            return true;
        }

        private static string DependencyDigest(AuditionPvDependencyHash[] values)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var material = new StringBuilder();
            foreach (AuditionPvDependencyHash value in (values ?? Array.Empty<AuditionPvDependencyHash>())
                         .OrderBy(item => item?.path, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(item => item?.path, StringComparer.Ordinal))
            {
                string path = Normalize(value?.path);
                if (value == null || string.IsNullOrWhiteSpace(path) || !seen.Add(path) ||
                    !value.exists || value.byteLength < 0 || !AuditionPvSha256.IsSha256(value.sha256))
                    return string.Empty;
                material.Append(path).Append('\0').Append('1').Append('\0')
                    .Append(value.byteLength.ToString(CultureInfo.InvariantCulture)).Append('\0')
                    .Append(value.sha256).Append('\0');
            }
            return seen.Count == 0 ? string.Empty : AuditionPvSha256.TextHash(material.ToString());
        }

        private static AuditionPvSixtySecondAudioRightsAssembly Finish(
            AuditionPvSixtySecondAudioRightsSelectionSpec spec,
            AuditionPvSixtySecondAudioRightsContext context,
            IEnumerable<AuditionPvSixtySecondAudioEvidence> audio,
            IEnumerable<AuditionPvSixtySecondRightsEvidence> rights,
            IEnumerable<AuditionPvSixtySecondUsedItem> items,
            IReadOnlyDictionary<string, HashSet<string>> shotAudio,
            IReadOnlyDictionary<string, HashSet<string>> shotItems,
            IEnumerable<AuditionPvAudioRightsGenerationLedgerBinding> generationLedgers,
            AuditionPvRightsCoverageReviewInput coverage,
            AuditionPvPinnedArtifact coveragePin,
            AuditionPvPinnedArtifact selectionSpecPin,
            IEnumerable<AuditionPvAudioRightsAssemblyIssue> issues,
            string fragmentPath, bool ignored)
        {
            AuditionPvAudioRightsAssemblyIssue[] issueArray = issues
                .OrderBy(value => SeverityOrder(value.severity))
                .ThenBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.location, StringComparer.Ordinal).ToArray();
            return new AuditionPvSixtySecondAudioRightsAssembly
            {
                schemaVersion = FragmentSchema,
                assemblyId = spec?.assemblyId ?? string.Empty,
                manifestId = spec?.manifestId ?? DefaultManifestId,
                productCheckpointGitSha = spec?.productCheckpointGitSha ?? string.Empty,
                readyForComposer = issueArray.Length == 0 && coveragePin != null &&
                    AuditionPvSha256.IsSha256(coveragePin.sha256),
                audio = audio.OrderBy(value => value.id, StringComparer.Ordinal).ToArray(),
                rights = rights.OrderBy(value => value.id, StringComparer.Ordinal).ToArray(),
                usedItems = items.OrderBy(value => value.id, StringComparer.Ordinal).ToArray(),
                generationLedgers = generationLedgers.OrderBy(value => value.audioId,
                    StringComparer.Ordinal).ToArray(),
                shotReferences = AtomicShotIds.Select(id => new AuditionPvSixtySecondShotReferenceBinding
                {
                    atomicShotId = id,
                    audioRefIds = shotAudio[id].OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                    usedItemIds = shotItems[id].OrderBy(value => value, StringComparer.Ordinal).ToArray()
                }).ToArray(),
                coverageInput = coverage ?? new AuditionPvRightsCoverageReviewInput(),
                rightsCoverageReview = coveragePin ?? new AuditionPvPinnedArtifact(),
                operatorSelectionSpec = selectionSpecPin ?? new AuditionPvPinnedArtifact(),
                issues = issueArray, fragmentPath = fragmentPath
            };
        }

        private static bool TryProbeWave(string path, AuditionPvAudioCueRegion[] regions,
            out WaveProbe result, out string failure)
        {
            result = new WaveProbe();
            failure = string.Empty;
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists || info.Length < 44 || info.Length > MaximumWaveBytes)
                { failure = "WAV size is invalid."; return false; }
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
                using var reader = new BinaryReader(stream, Encoding.ASCII, true);
                if (ReadFour(reader) != "RIFF") { failure = "RIFF header missing."; return false; }
                reader.ReadUInt32();
                if (ReadFour(reader) != "WAVE") { failure = "WAVE header missing."; return false; }
                ushort encoding = 0, bits = 0, blockAlign = 0;
                int byteRate = 0;
                long dataOffset = -1, dataBytes = 0;
                bool formatSeen = false, dataSeen = false;
                while (stream.Position + 8 <= stream.Length)
                {
                    string id = ReadFour(reader);
                    uint rawLength = reader.ReadUInt32();
                    long next = checked(stream.Position + rawLength + (rawLength & 1));
                    if (next > stream.Length) { failure = "Truncated WAV chunk."; return false; }
                    if (id == "fmt ")
                    {
                        if (formatSeen || rawLength < 16) return false;
                        encoding = reader.ReadUInt16();
                        result.channels = reader.ReadUInt16();
                        result.sampleRate = checked((int)reader.ReadUInt32());
                        byteRate = checked((int)reader.ReadUInt32());
                        blockAlign = reader.ReadUInt16();
                        bits = reader.ReadUInt16();
                        formatSeen = true;
                    }
                    else if (id == "data")
                    {
                        if (dataSeen) return false;
                        dataOffset = stream.Position;
                        dataBytes = rawLength;
                        dataSeen = true;
                    }
                    stream.Position = next;
                }
                int bytesPerSample = bits / 8;
                bool encodingValid = encoding == 1 && new ushort[] { 16, 24, 32 }.Contains(bits) ||
                    encoding == 3 && new ushort[] { 32, 64 }.Contains(bits);
                if (!formatSeen || !dataSeen || !encodingValid || result.channels < 1 ||
                    result.channels > 2 || result.sampleRate <= 0 || bytesPerSample <= 0 ||
                    blockAlign != result.channels * bytesPerSample ||
                    byteRate != (long)result.sampleRate * blockAlign || dataBytes <= 0 ||
                    dataBytes % blockAlign != 0)
                { failure = "Unsupported or inconsistent WAV format."; return false; }
                result.durationMilliseconds = checked((int)(dataBytes * 1000L / byteRate));
                int measuredSampleRate = result.sampleRate;
                int measuredChannels = result.channels;
                result.regions = regions.Select(value => new WaveRegionProbe
                {
                    cueId = value.cueId,
                    firstSample = (long)value.startMilliseconds * measuredSampleRate *
                        measuredChannels / 1000L,
                    lastSampleExclusive = (long)value.endMilliseconds * measuredSampleRate *
                        measuredChannels / 1000L
                }).ToArray();
                long availableSamples = dataBytes / bytesPerSample;
                if (result.regions.Any(value => value.firstSample < 0 ||
                    value.lastSampleExclusive <= value.firstSample ||
                    value.lastSampleExclusive > availableSamples))
                { failure = "Cue region lies outside WAV data."; return false; }

                stream.Position = dataOffset;
                byte[] buffer = new byte[64 * 1024 - (64 * 1024 % bytesPerSample)];
                long remaining = dataBytes, sampleIndex = 0;
                while (remaining > 0)
                {
                    int wanted = (int)Math.Min(buffer.Length, remaining);
                    int read = stream.Read(buffer, 0, wanted);
                    if (read <= 0 || read % bytesPerSample != 0)
                    { failure = "Truncated WAV sample data."; return false; }
                    for (int offset = 0; offset < read; offset += bytesPerSample, sampleIndex++)
                    {
                        double magnitude = SampleMagnitude(buffer, offset, encoding, bits);
                        if (magnitude > 0.000001d) result.nonSilentSamples++;
                        foreach (WaveRegionProbe region in result.regions)
                        {
                            if (sampleIndex < region.firstSample ||
                                sampleIndex >= region.lastSampleExclusive) continue;
                            region.totalSamples++;
                            if (magnitude > 0.00001d) region.nonSilentSamples++;
                            region.sumSquares += magnitude * magnitude;
                            if (magnitude > region.peak) region.peak = magnitude;
                        }
                    }
                    remaining -= read;
                }
                foreach (WaveRegionProbe region in result.regions)
                {
                    long minimum = Math.Max(1L, (region.totalSamples + 99L) / 100L);
                    double rms = Math.Sqrt(region.sumSquares / Math.Max(1L, region.totalSamples));
                    region.hasSignal = region.nonSilentSamples >= minimum &&
                        region.peak >= 0.001d && rms >= 0.0001d;
                }
                return true;
            }
            catch (Exception exception) when (IsIo(exception) || exception is OverflowException)
            { failure = exception.Message; return false; }
        }

        private static double SampleMagnitude(byte[] bytes, int offset, ushort encoding, ushort bits)
        {
            if (encoding == 1)
            {
                long signed = bits switch
                {
                    16 => BitConverter.ToInt16(bytes, offset),
                    24 => Pcm24(bytes, offset),
                    32 => BitConverter.ToInt32(bytes, offset),
                    _ => 0
                };
                double scale = bits switch
                    { 16 => 32768d, 24 => 8388608d, 32 => 2147483648d, _ => double.MaxValue };
                return Math.Min(1d, Math.Abs(signed / scale));
            }
            double value = bits == 32
                ? BitConverter.ToSingle(bytes, offset)
                : BitConverter.ToDouble(bytes, offset);
            return double.IsNaN(value) || double.IsInfinity(value) ? 0d : Math.Abs(value);
        }

        private static int Pcm24(byte[] bytes, int offset)
        {
            int value = bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16;
            return (value & 0x800000) == 0 ? value : value | unchecked((int)0xff000000);
        }

        private static string ReadFour(BinaryReader reader) =>
            new string(reader.ReadChars(4));

        private static int MinimumDuration(string category) => category switch
            { "music" => 1000, "ambience" => 1000, "vo" => 250, _ => 100 };

        private static string CueCategory(string cue)
        {
            if (cue == "music-bed") return "music";
            if (cue == "city-ambience" || cue == "olympus-ambience") return "ambience";
            if (cue == "announcement-vo" || cue == "inori-vo" || cue == "boss-vo") return "vo";
            return "sfx";
        }

        private static void AddInventoryTree(ICollection<AuditionPvAudioRightsInventoryEntry> entries,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues, string root, string kind,
            ref int traversedNodes)
        {
            string normalized;
            try { normalized = Path.GetFullPath(root); }
            catch (Exception exception) when (IsIo(exception))
            { Add(issues, "warning", "INVENTORY_ROOT_INVALID", kind, exception.Message); return; }
            if (!Directory.Exists(normalized))
            { Add(issues, "warning", "INVENTORY_ROOT_MISSING", kind, Normalize(normalized)); return; }
            try
            {
                RejectReparseChain(normalized);
                var pendingDirectories = new Stack<string>();
                pendingDirectories.Push(normalized);
                while (pendingDirectories.Count != 0)
                {
                    if (traversedNodes >= MaximumInventoryEntries ||
                        entries.Count >= MaximumInventoryEntries)
                    {
                        Add(issues, "warning", "INVENTORY_LIMIT_REACHED", kind,
                            MaximumInventoryEntries.ToString(CultureInfo.InvariantCulture));
                        return;
                    }
                    string directory = pendingDirectories.Pop();
                    RejectReparseChain(directory);
                    traversedNodes++;
                    int remaining = MaximumInventoryEntries - traversedNodes;
                    var children = new List<string>(Math.Min(remaining + 1, 256));
                    foreach (string child in Directory.EnumerateFileSystemEntries(directory, "*",
                                 SearchOption.TopDirectoryOnly))
                    {
                        children.Add(Path.GetFullPath(child));
                        if (children.Count > remaining)
                        {
                            Add(issues, "warning", "INVENTORY_LIMIT_REACHED", kind,
                                MaximumInventoryEntries.ToString(CultureInfo.InvariantCulture));
                            return;
                        }
                    }
                    children.Sort((left, right) => StringComparer.Ordinal.Compare(
                        Normalize(left), Normalize(right)));
                    var childDirectories = new List<string>();
                    foreach (string path in children)
                    {
                        traversedNodes++;
                        FileAttributes attributes = File.GetAttributes(path);
                        if ((attributes & FileAttributes.ReparsePoint) != 0)
                        {
                            Add(issues, "warning", "INVENTORY_REPARSE_REJECTED", kind,
                                Normalize(path));
                            continue;
                        }
                        if ((attributes & FileAttributes.Directory) != 0)
                        {
                            childDirectories.Add(path);
                            continue;
                        }
                        var file = new FileInfo(path);
                        string extension = file.Extension.ToLowerInvariant();
                        string status = InventoryStatus(kind, path, extension);
                        if (string.IsNullOrEmpty(status)) continue;
                        entries.Add(new AuditionPvAudioRightsInventoryEntry
                        {
                            kind = kind, path = Normalize(path), byteLength = file.Length,
                            extension = extension, status = status,
                            sha256 = file.Length <= MaximumInventoryHashBytes
                                ? AuditionPvSha256.FileHash(path) : string.Empty
                        });
                    }
                    for (int index = childDirectories.Count - 1; index >= 0; index--)
                        pendingDirectories.Push(childDirectories[index]);
                }
            }
            catch (Exception exception) when (IsIo(exception))
            { Add(issues, "warning", "INVENTORY_SCAN_FAILED", kind, exception.Message); }
        }

        private static string InventoryStatus(string kind, string path, string extension)
        {
            if (kind == "project-audio")
                return extension == ".wav" ? "project-wav-source" :
                    new[] { ".mp3", ".ogg", ".aif", ".aiff", ".flac" }.Contains(extension)
                        ? "requires-gate-wav-derivative" : string.Empty;
            if (kind == "pv-audio")
                return extension == ".wav" ? "gate-wav-candidate" :
                    new[] { ".json", ".md", ".txt", ".sha256" }.Contains(extension)
                        ? "audio-provenance-source" : string.Empty;
            if (kind == "pretendard")
                return new[] { ".otf", ".ttf", ".txt" }.Contains(extension)
                    ? "pretendard-open-license-source" : string.Empty;
            if (kind == "tokyo-street") return "tokyo-street-project-dependency";
            if (kind == "license-evidence")
                return Path.GetFileName(path).IndexOf("TokyoStreet", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "tokyo-street-admission-source"
                    : Path.GetFileName(path).IndexOf("ELEVENLABS", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "elevenlabs-rights-source" : "license-source";
            return string.Empty;
        }

        private static AuditionPvSixtySecondAudioRightsInventory InventoryResult(
            IEnumerable<AuditionPvAudioRightsInventoryEntry> entries,
            IEnumerable<AuditionPvAudioRightsAssemblyIssue> issues) => new()
        {
            schemaVersion = InventorySchema,
            entries = entries.OrderBy(value => value.path, StringComparer.Ordinal).ToArray(),
            issues = issues.OrderBy(value => value.code, StringComparer.Ordinal).ToArray()
        };

        private static bool ContextShapeValid(AuditionPvSixtySecondAudioRightsContext context,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (context == null) { Add(issues, "error", "CONTEXT_NULL", "context", "null"); return false; }
            bool valid = Absolute(context.projectRoot) && Absolute(context.audioRoot) &&
                Absolute(context.licenseRoot) && Absolute(context.reviewRoot) &&
                (context.captureRoots ?? Array.Empty<string>()).All(Absolute);
            if (!valid) Add(issues, "error", "CONTEXT_ROOT_INVALID", "context",
                "All roots must be absolute.");
            return valid;
        }

        private static string ResolveUnity(string locator, string projectRoot, string at,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(locator) || Path.IsPathRooted(locator) ||
                !Normalize(locator).StartsWith("Assets/", StringComparison.Ordinal))
            { Add(issues, "error", "UNITY_LOCATOR_INVALID", at, locator ?? "<null>"); return string.Empty; }
            return ResolveUnder(Path.Combine(projectRoot, locator.Replace('/',
                Path.DirectorySeparatorChar)), projectRoot, at, issues);
        }

        private static string ResolveExternal(string path, string root, string at,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues, bool allowProjectRoot = false) =>
            ResolveUnder(path, root, at, issues);

        private static string ResolveAnyRoot(string path, IEnumerable<string> roots, string at,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            foreach (string root in roots ?? Array.Empty<string>())
            {
                string candidate = ResolveUnder(path, root, at, null);
                if (!string.IsNullOrEmpty(candidate)) return candidate;
            }
            Add(issues, "hold", "PATH_OUTSIDE_ALLOWED_ROOT", at, path ?? "<null>");
            return string.Empty;
        }

        private static string ResolveUnder(string path, string root, string at,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) ||
                    string.IsNullOrWhiteSpace(root) || !Path.IsPathRooted(root)) throw new ArgumentException();
                string full = Path.GetFullPath(path);
                string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
                string prefix = fullRoot + Path.DirectorySeparatorChar;
                if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(full, fullRoot, StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException();
                RejectReparseChainForExistingParents(fullRoot);
                RejectReparseChainForExistingParents(full);
                return full;
            }
            catch (Exception exception) when (IsIo(exception))
            {
                if (issues != null) Add(issues, "error", "PATH_OUTSIDE_ALLOWED_ROOT", at,
                    path ?? "<null>");
                return string.Empty;
            }
        }

        private static bool PinMatches(string path, string sha256, string at,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(path) || !AuditionPvSha256.IsSha256(sha256))
            { Add(issues, "error", "PIN_SHAPE_INVALID", at, path ?? "<null>"); return false; }
            try
            {
                FileIdentity identity = ReadStableIdentity(path, sha256, long.MaxValue, at);
                consumedPins.Record(identity, at);
                return true;
            }
            catch (Exception exception) when (IsIo(exception) ||
                                              exception is InvalidDataException)
            { Add(issues, "error", "PIN_MISSING_OR_DRIFT", at, exception.Message); return false; }
        }

        private static bool ReadPinnedBytes(string path, string sha256, long maxBytes, string at,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();
            try
            {
                FileSnapshot snapshot = ReadStableSnapshot(path, sha256, maxBytes, at);
                consumedPins.Record(snapshot, at);
                bytes = snapshot.bytes;
                return true;
            }
            catch (Exception exception) when (IsIo(exception) ||
                                              exception is InvalidDataException)
            {
                Add(issues, "error", "PIN_SNAPSHOT_FAILED", at, exception.Message);
                return false;
            }
        }

        private static bool ReadPinnedText(string path, string sha256, long maxBytes, string at,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues, out string text)
        {
            text = string.Empty;
            if (!ReadPinnedBytes(path, sha256, maxBytes, at, consumedPins, issues,
                    out byte[] bytes)) return false;
            try
            {
                text = StrictUtf8.GetString(bytes);
                return true;
            }
            catch (DecoderFallbackException exception)
            {
                Add(issues, "error", "PIN_TEXT_NOT_UTF8", at, exception.Message);
                return false;
            }
        }

        private static bool TextContainsPins(string text, IEnumerable<string> hashes) =>
            (hashes ?? Array.Empty<string>()).Where(AuditionPvSha256.IsSha256)
            .Distinct(StringComparer.Ordinal).All(hash =>
                (text ?? string.Empty).IndexOf(hash, StringComparison.OrdinalIgnoreCase) >= 0);

        private static bool TryProbePinnedWave(string path, string sha256,
            AuditionPvAudioCueRegion[] regions, string at,
            ConsumedPinRegistry consumedPins,
            ICollection<AuditionPvAudioRightsAssemblyIssue> issues,
            out WaveProbe probe, out string failure)
        {
            probe = new WaveProbe();
            failure = string.Empty;
            try
            {
                FileIdentity before = ReadStableIdentity(path, sha256, MaximumWaveBytes,
                    at + " before probe");
                if (!TryProbeWave(path, regions, out probe, out failure)) return false;
                FileIdentity after = ReadStableIdentity(path, sha256, MaximumWaveBytes,
                    at + " after probe");
                if (before.length != after.length || before.sha256 != after.sha256 ||
                    !string.Equals(before.path, after.path, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Pinned WAV changed while it was probed.");
                consumedPins.Record(after, at);
                return true;
            }
            catch (Exception exception) when (IsIo(exception) ||
                                              exception is InvalidDataException)
            {
                Add(issues, "error", "PIN_WAV_NOT_STABLE", at, exception.Message);
                failure = exception.Message;
                return false;
            }
        }

        private static AuditionPvPinnedArtifact PendingText(string root, string name, string text,
            ICollection<PendingArtifact> artifacts)
        {
            byte[] bytes = new UTF8Encoding(false, true).GetBytes((text ?? string.Empty)
                .Replace("\r\n", "\n").TrimEnd('\r', '\n') + "\n");
            return Pending(root, name, bytes, artifacts);
        }

        private static AuditionPvPinnedArtifact PendingJson<T>(string root, string name, T value,
            ICollection<PendingArtifact> artifacts) => Pending(root, name, JsonBytes(value), artifacts);

        private static AuditionPvPinnedArtifact Pending(string root, string name, byte[] bytes,
            ICollection<PendingArtifact> artifacts)
        {
            if (bytes == null || bytes.Length == 0)
                throw new InvalidDataException("A planned evidence artifact cannot be empty.");
            string path = Normalize(Path.Combine(root, name));
            string sha256 = ByteHash(bytes);
            artifacts.Add(new PendingArtifact { path = path, bytes = bytes, sha256 = sha256 });
            return new AuditionPvPinnedArtifact { path = path, sha256 = sha256 };
        }

        private static byte[] JsonBytes<T>(T value)
        {
            string json = JsonUtility.ToJson(value, true).Replace("\r\n", "\n").TrimEnd() + "\n";
            return new UTF8Encoding(false, true).GetBytes(json);
        }

        private static void CommitExact(IEnumerable<PendingArtifact> values,
            ConsumedPinRegistry consumedPins, Action afterInstallForTests)
        {
            PendingArtifact[] files = (values ?? Array.Empty<PendingArtifact>())
                .OrderBy(value => value?.path, StringComparer.Ordinal).ToArray();
            if (files.Any(value => value == null || string.IsNullOrWhiteSpace(value.path) ||
                    value.bytes == null || value.bytes.Length == 0 ||
                    !AuditionPvSha256.IsSha256(value.sha256) ||
                    ByteHash(value.bytes) != value.sha256) ||
                files.Select(value => FullOrEmpty(value.path)).Distinct(
                    StringComparer.OrdinalIgnoreCase).Count() != files.Length)
                throw new InvalidDataException("Planned evidence files are invalid or duplicated.");
            var installed = new List<string>();
            var temporaries = new List<string>();
            try
            {
                consumedPins.VerifyAll("pre-commit external pin verification");
                foreach (PendingArtifact file in files)
                {
                    string path = Path.GetFullPath(file.path);
                    string parent = Path.GetDirectoryName(path) ??
                        throw new InvalidDataException("Planned evidence file has no parent.");
                    RejectReparseChainForExistingParents(path);
                    Directory.CreateDirectory(parent);
                    RejectReparseChain(parent);
                    if (File.Exists(path))
                    {
                        ReadStableIdentity(path, file.sha256, file.bytes.LongLength,
                            "existing immutable evidence");
                        continue;
                    }
                    string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
                    using (var stream = new FileStream(temporary, FileMode.CreateNew,
                               FileAccess.Write, FileShare.None, 64 * 1024,
                               FileOptions.WriteThrough))
                    {
                        stream.Write(file.bytes, 0, file.bytes.Length);
                        stream.Flush(true);
                    }
                    temporaries.Add(temporary);
                    ReadStableIdentity(temporary, file.sha256, file.bytes.LongLength,
                        "temporary evidence");
                    File.Move(temporary, path);
                    temporaries.Remove(temporary);
                    installed.Add(path);
                }
                foreach (PendingArtifact file in files)
                    ReadStableIdentity(file.path, file.sha256, file.bytes.LongLength,
                        "installed evidence post-verification");
                afterInstallForTests?.Invoke();
                consumedPins.VerifyAll("post-commit external pin verification");
            }
            catch (Exception failure)
            {
                var cleanupFailures = new List<Exception>();
                foreach (string temporary in temporaries.AsEnumerable().Reverse())
                    try { if (File.Exists(temporary)) File.Delete(temporary); }
                    catch (Exception exception) when (IsIo(exception))
                    { cleanupFailures.Add(exception); }
                foreach (string path in installed.AsEnumerable().Reverse())
                    try { if (File.Exists(path)) File.Delete(path); }
                    catch (Exception exception) when (IsIo(exception))
                    { cleanupFailures.Add(exception); }
                if (cleanupFailures.Count != 0)
                    throw new AggregateException("Evidence rollback was incomplete.",
                        new[] { failure }.Concat(cleanupFailures));
                throw;
            }
        }

        private static string ByteHash(byte[] bytes)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(bytes).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static byte[] CanonicalSelectionBytes(
            AuditionPvSixtySecondAudioRightsSelectionSpec value) =>
            value == null ? Array.Empty<byte>() : JsonBytes(value);

        private static PinnedSelection ReadSelectionSnapshot(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
                throw new ArgumentException("Selection spec path must be absolute.", nameof(path));
            FileSnapshot snapshot = ReadStableSnapshot(path, null, MaximumJsonBytes,
                "operator selection spec");
            string json;
            try { json = StrictUtf8.GetString(snapshot.bytes); }
            catch (DecoderFallbackException exception)
            { throw new InvalidDataException("Selection spec is not strict UTF-8.", exception); }
            AuditionPvSixtySecondAudioRightsSelectionSpec value;
            try { value = JsonUtility.FromJson<AuditionPvSixtySecondAudioRightsSelectionSpec>(json); }
            catch (Exception exception) when (exception is ArgumentException ||
                                              exception is InvalidOperationException)
            { throw new InvalidDataException("Selection spec JSON is invalid.", exception); }
            if (value == null) throw new InvalidDataException("Selection spec JSON decoded to null.");
            return new PinnedSelection
            {
                value = value, bytes = snapshot.bytes,
                source = new AuditionPvPinnedArtifact
                    { path = Normalize(snapshot.path), sha256 = snapshot.sha256 },
                identity = new FileIdentity
                    { path = snapshot.path, sha256 = snapshot.sha256, length = snapshot.length }
            };
        }

        private static FileSnapshot ReadStableSnapshot(string path, string expectedSha256,
            long maximumBytes, string role)
        {
            path = Path.GetFullPath(path ?? throw new ArgumentNullException(nameof(path)));
            RejectReparseChain(path);
            var file = new FileInfo(path);
            if (!file.Exists || file.Length <= 0 || file.Length > maximumBytes ||
                file.Length > int.MaxValue)
                throw new InvalidDataException(role + " is missing or outside its byte limit.");
            byte[] bytes;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                       FileShare.Read, 64 * 1024, FileOptions.SequentialScan))
            {
                if (stream.Length != file.Length || stream.Length <= 0 ||
                    stream.Length > maximumBytes || stream.Length > int.MaxValue)
                    throw new InvalidDataException(role + " changed before its bounded read.");
                bytes = new byte[checked((int)stream.Length)];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    int read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read <= 0) throw new EndOfStreamException(role + " was truncated.");
                    offset += read;
                }
                if (stream.ReadByte() != -1)
                    throw new InvalidDataException(role + " grew during its bounded read.");
            }
            string sha256 = ByteHash(bytes);
            if (!string.IsNullOrWhiteSpace(expectedSha256) &&
                (!AuditionPvSha256.IsSha256(expectedSha256) || sha256 != expectedSha256))
                throw new InvalidDataException(role + " hash drifted.");
            var after = new FileInfo(path);
            if (!after.Exists || after.Length != bytes.LongLength ||
                AuditionPvSha256.FileHash(path) != sha256)
                throw new InvalidDataException(role + " changed after its bounded byte snapshot.");
            return new FileSnapshot
                { path = path, length = bytes.LongLength, sha256 = sha256, bytes = bytes };
        }

        private static FileIdentity ReadStableIdentity(string path, string expectedSha256,
            long maximumBytes, string role)
        {
            path = Path.GetFullPath(path ?? throw new ArgumentNullException(nameof(path)));
            RejectReparseChain(path);
            var before = new FileInfo(path);
            if (!before.Exists || before.Length <= 0 || before.Length > maximumBytes)
                throw new InvalidDataException(role + " is missing or outside its byte limit.");
            long length = before.Length;
            string sha256 = AuditionPvSha256.FileHash(path);
            var after = new FileInfo(path);
            if (!after.Exists || after.Length != length ||
                !AuditionPvSha256.IsSha256(expectedSha256) || sha256 != expectedSha256 ||
                AuditionPvSha256.FileHash(path) != sha256)
                throw new InvalidDataException(role + " changed during identity verification.");
            return new FileIdentity { path = path, length = length, sha256 = sha256 };
        }

        private static void RejectReparseChain(string path)
        {
            path = Path.GetFullPath(path);
            string current = File.Exists(path) || Directory.Exists(path)
                ? path : Path.GetDirectoryName(path);
            while (!string.IsNullOrWhiteSpace(current))
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Reparse points are forbidden: " + current);
                current = Path.GetDirectoryName(current);
            }
        }

        private static void RejectReparseChainForExistingParents(string path)
        {
            string current = Path.GetFullPath(path);
            while (!string.IsNullOrWhiteSpace(current) && !File.Exists(current) &&
                   !Directory.Exists(current))
                current = Path.GetDirectoryName(current);
            if (!string.IsNullOrWhiteSpace(current)) RejectReparseChain(current);
        }

        private static string FullOrEmpty(string value)
        {
            try { return string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value); }
            catch (Exception exception) when (IsIo(exception)) { return string.Empty; }
        }

        private static string DependencyIdentity(AuditionPvRightsDependencyClassification value) =>
            value == null ? string.Empty : string.Join("\0", value.captureId ?? string.Empty,
                value.sourceManifestSha256 ?? string.Empty, Normalize(value.path),
                value.byteLength.ToString(CultureInfo.InvariantCulture), value.sha256 ?? string.Empty);

        private static AuditionPvAudioCueRegion Clone(AuditionPvAudioCueRegion value) => new()
        { cueId = value?.cueId ?? string.Empty, startMilliseconds = value?.startMilliseconds ?? 0,
            endMilliseconds = value?.endMilliseconds ?? 0 };

        private static AuditionPvPinnedArtifact Clone(AuditionPvPinnedArtifact value) => new()
            { path = value?.path ?? string.Empty, sha256 = value?.sha256 ?? string.Empty };

        private static bool SafeId(string value) => !string.IsNullOrWhiteSpace(value) &&
            value.Length <= 80 && value[0] >= 'a' && value[0] <= 'z' &&
            value.All(character => character >= 'a' && character <= 'z' ||
                character >= '0' && character <= '9' || character == '-');

        private static bool IsFullGitSha(string value) => value != null && value.Length == 40 &&
            value.All(character => character >= '0' && character <= '9' ||
                character >= 'a' && character <= 'f');

        private static bool Utc(string value) => DateTime.TryParse(value,
            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal |
            DateTimeStyles.AssumeUniversal, out DateTime parsed) && parsed.Kind == DateTimeKind.Utc &&
            (value.EndsWith("Z", StringComparison.Ordinal) ||
             value.EndsWith("+00:00", StringComparison.Ordinal));

        private static bool Absolute(string value)
        {
            try { return !string.IsNullOrWhiteSpace(value) && Path.IsPathRooted(Path.GetFullPath(value)); }
            catch (Exception exception) when (IsIo(exception)) { return false; }
        }

        private static string Normalize(string value) => (value ?? string.Empty).Replace('\\', '/');

        private static bool IsIo(Exception value) => value is IOException ||
            value is UnauthorizedAccessException || value is ArgumentException ||
            value is NotSupportedException || value is System.Security.SecurityException;

        private static void Add(ICollection<AuditionPvAudioRightsAssemblyIssue> values,
            string severity, string code, string location, string message) => values?.Add(
            new AuditionPvAudioRightsAssemblyIssue { severity = severity, code = code,
                location = location, message = message });

        private static int SeverityOrder(string value) => value switch
            { "error" => 0, "hold" => 1, _ => 2 };

        private sealed class PendingArtifact
        {
            public string path = string.Empty, sha256 = string.Empty;
            public byte[] bytes = Array.Empty<byte>();
        }

        private class FileIdentity
        { public string path = string.Empty, sha256 = string.Empty; public long length; }

        private sealed class FileSnapshot : FileIdentity
        { public byte[] bytes = Array.Empty<byte>(); }

        private sealed class ConsumedPinRegistry
        {
            private readonly Dictionary<string, FileIdentity> values = new(
                StringComparer.OrdinalIgnoreCase);

            public void Record(FileIdentity value, string role)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.path) ||
                    value.length <= 0 || !AuditionPvSha256.IsSha256(value.sha256))
                    throw new InvalidDataException("Consumed pin identity is invalid: " + role);
                string path = Path.GetFullPath(value.path);
                if (values.TryGetValue(path, out FileIdentity prior))
                {
                    if (prior.length != value.length || prior.sha256 != value.sha256)
                        throw new InvalidDataException("Consumed pin changed between reads: " + path);
                    return;
                }
                values.Add(path, new FileIdentity
                    { path = path, length = value.length, sha256 = value.sha256 });
            }

            public void VerifyAll(string phase)
            {
                foreach (FileIdentity expected in values.Values.OrderBy(value => value.path,
                             StringComparer.OrdinalIgnoreCase))
                {
                    FileIdentity actual = ReadStableIdentity(expected.path, expected.sha256,
                        expected.length, phase + ": " + expected.path);
                    if (actual.length != expected.length || actual.sha256 != expected.sha256 ||
                        !string.Equals(actual.path, expected.path,
                            StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Consumed pin drifted during " + phase +
                            ": " + expected.path);
                }
            }
        }

        private sealed class PinnedSelection
        {
            public AuditionPvSixtySecondAudioRightsSelectionSpec value;
            public byte[] bytes = Array.Empty<byte>();
            public AuditionPvPinnedArtifact source = new();
            public FileIdentity identity;
        }

        private sealed class WaveProbe
        {
            public int sampleRate, channels, durationMilliseconds;
            public long nonSilentSamples;
            public WaveRegionProbe[] regions = Array.Empty<WaveRegionProbe>();
        }

        private sealed class WaveRegionProbe
        {
            public string cueId = string.Empty;
            public long firstSample, lastSampleExclusive, totalSamples, nonSilentSamples;
            public double sumSquares, peak;
            public bool hasSignal;
        }
    }

    [Serializable] internal sealed class AuditionPvSixtySecondAudioRightsContext
    {
        public string projectRoot = string.Empty, audioRoot = string.Empty;
        public string licenseRoot = string.Empty, reviewRoot = string.Empty;
        public string[] captureRoots = Array.Empty<string>();
        [NonSerialized] internal Action afterEvidenceInstallForTests;
    }

    [Serializable] internal sealed class AuditionPvSixtySecondAudioRightsSelectionSpec
    {
        public string schemaVersion = AuditionPvSixtySecondAudioRightsAssembler.SelectionSchema;
        public string judgementOrigin = string.Empty;
        public string assemblyId = string.Empty;
        public string manifestId = AuditionPvSixtySecondAudioRightsAssembler.DefaultManifestId;
        public string productCheckpointGitSha = string.Empty;
        public AuditionPvAudioRightsAudioSelection[] audio =
            Array.Empty<AuditionPvAudioRightsAudioSelection>();
        public AuditionPvAudioRightsItemSelection[] items =
            Array.Empty<AuditionPvAudioRightsItemSelection>();
        public AuditionPvAudioRightsCoverageSelection coverage = new();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsAudioSelection
    {
        public string id = string.Empty, category = string.Empty;
        public AuditionPvPinnedArtifact file = new();
        public AuditionPvAudioCueRegion[] cueRegions = Array.Empty<AuditionPvAudioCueRegion>();
        public bool generatedByAi;
        public AuditionPvAudioRightsGenerationSelection generation = new();
        public AuditionPvAudioRightsRecordSelection rights = new();
        public AuditionPvAudioRightsListeningSelection listening = new();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsGenerationSelection
    {
        public string provider = string.Empty, model = string.Empty, accountPlan = string.Empty;
        public string tool = string.Empty, toolVersion = string.Empty, generatedAtUtc = string.Empty;
        public string voiceIdentityDisposition = "non-real-person-imitation";
        public string promptText = string.Empty;
        public string[] recipeSteps = Array.Empty<string>();
        public AuditionPvPinnedArtifact sourceManifest = new(), sourceDerivationRecipe = new();
        public AuditionPvPinnedArtifact originalGeneratedWav = new();
        public AuditionPvPinnedArtifact[] alternateGeneratedWavs =
            Array.Empty<AuditionPvPinnedArtifact>();
        public AuditionPvPinnedArtifact termsSnapshot = new(), generationEvidence = new();
        public AuditionPvPinnedArtifact consentArtifact = new();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsListeningSelection
    {
        public string judgementOrigin = string.Empty;
        public string status = "pending", reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvAudioRightsItemSelection
    {
        public string id = string.Empty, scope = string.Empty;
        public string sourceLocator = string.Empty, expectedSha256 = string.Empty;
        public string dependencyBinding = "unity-dependency";
        public string admissionProfile = string.Empty;
        public string[] atomicShotIds = Array.Empty<string>();
        public AuditionPvAudioRightsRecordSelection rights = new();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsRecordSelection
    {
        public string judgementOrigin = string.Empty;
        public string disposition = string.Empty, verifiedBy = string.Empty, verifiedAtUtc = string.Empty;
        public string provider = string.Empty, licenseId = string.Empty, licenseVersion = string.Empty;
        public string accountEntitlementId = string.Empty, useBoundary = string.Empty;
        public string owner = string.Empty, sourceDescription = string.Empty;
        public string accountPlan = string.Empty, exclusionReason = string.Empty;
        public bool attributionRequired, verified;
        public AuditionPvPinnedArtifact termsSnapshot = new(), entitlementEvidence = new();
        public AuditionPvPinnedArtifact attributionArtifact = new(), generationEvidence = new();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsCoverageSelection
    {
        public string judgementOrigin = string.Empty;
        public bool approveComplete;
        public string reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public AuditionPvPinnedArtifact approvedComposeInput = new();
        public AuditionPvAudioRightsSelectedCapture[] selectedCaptures =
            Array.Empty<AuditionPvAudioRightsSelectedCapture>();
        public AuditionPvRightsDependencyClassification[] dependencies =
            Array.Empty<AuditionPvRightsDependencyClassification>();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsSelectedCapture
    {
        public string captureId = string.Empty;
        public AuditionPvPinnedArtifact sourceManifest = new();
        public string sourceDependencyIdentitySha256 = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvRightsCoverageReviewInput
    {
        public string schemaVersion = AuditionPvSixtySecondAudioRightsAssembler.CoverageInputSchema;
        public string status = "hold-selected-capture-review-not-approved";
        public string judgementOrigin = string.Empty;
        public string manifestId = string.Empty, productCheckpointGitSha = string.Empty;
        public bool approvalRequested, exactClosure;
        public string reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public string[] usedItemIds = Array.Empty<string>();
        public AuditionPvAudioRightsSelectedCapture[] selectedCaptures =
            Array.Empty<AuditionPvAudioRightsSelectedCapture>();
        public AuditionPvRightsDependencyClassification[] dependencies =
            Array.Empty<AuditionPvRightsDependencyClassification>();
        public AuditionPvPinnedArtifact approvedComposeInput = new();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondAudioRightsAssembly
    {
        public string schemaVersion = AuditionPvSixtySecondAudioRightsAssembler.FragmentSchema;
        public string assemblyId = string.Empty, manifestId = string.Empty;
        public string productCheckpointGitSha = string.Empty;
        public bool readyForComposer;
        public AuditionPvSixtySecondAudioEvidence[] audio =
            Array.Empty<AuditionPvSixtySecondAudioEvidence>();
        public AuditionPvSixtySecondRightsEvidence[] rights =
            Array.Empty<AuditionPvSixtySecondRightsEvidence>();
        public AuditionPvSixtySecondUsedItem[] usedItems =
            Array.Empty<AuditionPvSixtySecondUsedItem>();
        public AuditionPvAudioRightsGenerationLedgerBinding[] generationLedgers =
            Array.Empty<AuditionPvAudioRightsGenerationLedgerBinding>();
        public AuditionPvSixtySecondShotReferenceBinding[] shotReferences =
            Array.Empty<AuditionPvSixtySecondShotReferenceBinding>();
        public AuditionPvRightsCoverageReviewInput coverageInput = new();
        public AuditionPvPinnedArtifact rightsCoverageReview = new();
        public AuditionPvPinnedArtifact operatorSelectionSpec = new();
        public AuditionPvAudioRightsAssemblyIssue[] issues =
            Array.Empty<AuditionPvAudioRightsAssemblyIssue>();
        public string fragmentPath = string.Empty, fragmentSha256 = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvAudioRightsGenerationLedgerBinding
    {
        public string audioId = string.Empty;
        public AuditionPvPinnedArtifact ledger = new();
    }

    [Serializable] internal sealed class AuditionPvAudioGenerationSourceLedgerArtifact
    {
        public string schemaVersion = string.Empty, audioId = string.Empty;
        public string provider = string.Empty, model = string.Empty, generatedAtUtc = string.Empty;
        public AuditionPvPinnedArtifact sourceManifest = new();
        public AuditionPvPinnedArtifact sourceDerivationRecipe = new();
        public AuditionPvPinnedArtifact selectedEditedWav = new(), originalGeneratedWav = new();
        public AuditionPvPinnedArtifact[] alternateGeneratedWavs =
            Array.Empty<AuditionPvPinnedArtifact>();
    }

    [Serializable] internal sealed class AuditionPvSixtySecondAudioRightsInventory
    {
        public string schemaVersion = AuditionPvSixtySecondAudioRightsAssembler.InventorySchema;
        public AuditionPvAudioRightsInventoryEntry[] entries =
            Array.Empty<AuditionPvAudioRightsInventoryEntry>();
        public AuditionPvAudioRightsAssemblyIssue[] issues =
            Array.Empty<AuditionPvAudioRightsAssemblyIssue>();
    }

    [Serializable] internal sealed class AuditionPvAudioRightsInventoryEntry
    {
        public string kind = string.Empty, path = string.Empty, extension = string.Empty;
        public long byteLength;
        public string sha256 = string.Empty, status = string.Empty;
    }

    [Serializable] internal sealed class AuditionPvAudioRightsAssemblyIssue
    {
        public string severity = string.Empty, code = string.Empty;
        public string location = string.Empty, message = string.Empty;
    }
}
