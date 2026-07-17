import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-PROVENANCE-GENERATOR-01";
const CONTRACT_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-01";
const PRODUCER_CONTRACT_ID = "PGR-GUIDEFIGHT-STAGE-SPINE-PRODUCER-01";
const ARTIFACT_SET_ID = "PGR-GUIDEFIGHT-STAGE-SPINE-ALT3RI-856A0E45-V1";
const SOURCE_SNAPSHOT_ID = "pgr-alt3ri-856a0e45-en-guidefight-stage-v1";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";
const snapshotRelativeRoot = "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14";
const snapshotRoot = join(arkRoot, ...snapshotRelativeRoot.split("/"));

const mainGenerator = {
  path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs",
  sizeBytes: 37178,
  sha256: "3284524cec2eb68ccc430aabeec4b08fc9bb70fedb03715b178814d12bc92f87",
  generatorId: "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-GENERATOR-01",
  runtime: "Node.js v24.14.0 built-ins only",
};

const outputs = [
  {
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-readfirst-md",
    predecessorSourceId: "pgr-readfirst-md",
    kind: "report",
    artifactId: "P1B-PGR-STAGE-SPINE-READFIRST-V1-MD",
    path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_READFIRST_V1.md",
    sizeBytes: 5526,
    sha256: "f606f014a3e12101bae89918c16ac68c6184eeca0290b82800b5e50dae2c7caf",
    schemaVersion: 1,
    dataRowCount: 1,
  },
  {
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-summary-json",
    predecessorSourceId: "pgr-readfirst-summary-json",
    kind: "derived",
    artifactId: "P1B-PGR-STAGE-SPINE-READFIRST-V1-SUMMARY-JSON",
    path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_READFIRST_V1_SUMMARY.json",
    sizeBytes: 19861,
    sha256: "f400b09476b33c338c8b5f670d263d0152a0201c2c92078963bf3ae30bc276bb",
    schemaVersion: 1,
    dataRowCount: 1,
    canonicalReportDigest: "39c8136d6e0813f83a78c11e1a7ada648506d204fd59548a7e509bbdfb6eedd0",
  },
  {
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-label-context-csv",
    predecessorSourceId: "pgr-guidefight-label-csv",
    kind: "derived",
    artifactId: "P1B-PGR-GUIDEFIGHT-STAGE-LABEL-CONTEXT-V1-CSV",
    path: "_Game/DesignDocs/P1B_PGR_GUIDEFIGHT_STAGE_LABEL_CONTEXT_V1.csv",
    sizeBytes: 8694,
    sha256: "0159d965438aa23b60b1e6b66cf7c1bff8b4edc21f8e0a50531d92266357d153",
    schemaVersion: 1,
    dataRowCount: 20,
  },
  {
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-reading-links-csv",
    predecessorSourceId: "pgr-guidefight-links-csv",
    kind: "derived",
    artifactId: "P1B-PGR-GUIDEFIGHT-STAGE-READING-LINKS-V1-CSV",
    path: "_Game/DesignDocs/P1B_PGR_GUIDEFIGHT_STAGE_READING_LINKS_V1.csv",
    sizeBytes: 18714,
    sha256: "312811436a34b38888582289a93c6a60e6f2ccd44b6d22eef88c6c5bc4e6170d",
    schemaVersion: 1,
    dataRowCount: 56,
  },
];

const sourceRecordPath = join(here, "P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json");
const producerManifestPath = join(here, "P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json");

function fail(message) {
  throw new Error(`${GENERATOR_ID}: ${message}`);
}

function assert(condition, message) {
  if (!condition) fail(message);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function canonicalize(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`).join(",")}}`;
}

function canonicalDigest(value) {
  return sha256(Buffer.from(canonicalize(value), "utf8"));
}

function readVerified(path, expectedSizeBytes, expectedSha256) {
  const bytes = readFileSync(path);
  assert(bytes.length === expectedSizeBytes, `size mismatch for ${path}: ${bytes.length}`);
  assert(sha256(bytes) === expectedSha256, `SHA-256 mismatch for ${path}: ${sha256(bytes)}`);
  return bytes;
}

const mainGeneratorBytes = readVerified(join(workspaceRoot, ...mainGenerator.path.split("/")), mainGenerator.sizeBytes, mainGenerator.sha256);
assert(mainGeneratorBytes.length === mainGenerator.sizeBytes, "main generator did not bind");

for (const output of outputs) {
  const bytes = readVerified(join(workspaceRoot, ...output.path.split("/")), output.sizeBytes, output.sha256);
  const text = bytes.toString("utf8");
  assert(!text.includes("\r"), `${output.sourceId} contains CR`);
  assert(text.endsWith("\n") && !text.endsWith("\n\n"), `${output.sourceId} must end with exactly one LF`);
  if (output.kind === "derived" && output.path.endsWith(".csv")) {
    assert(text.split("\n").length - 2 === output.dataRowCount, `${output.sourceId} data-row count changed`);
  }
}

const summary = JSON.parse(readFileSync(join(workspaceRoot, ...outputs[1].path.split("/")), "utf8"));
const { canonicalReportDigest, ...summaryWithoutDigest } = summary;
assert(canonicalReportDigest === outputs[1].canonicalReportDigest, "summary canonical digest field changed");
assert(canonicalDigest(summaryWithoutDigest) === canonicalReportDigest, "summary canonical digest reconstruction failed");
assert(summary.labelContextContract.rowCount === 20, "summary label row count changed");
assert(summary.readingLinksContract.rowCount === 56, "summary link row count changed");
assert(summary.readingLinksContract.stateCounts.present === 32, "summary present count changed");
assert(summary.readingLinksContract.stateCounts.absent === 20, "summary absent count changed");
assert(summary.readingLinksContract.stateCounts.unresolved === 4, "summary unresolved count changed");
assert(summary.sourceValuePolicy.sourceValueCopiedCount === 0, "summary copied payload count must be zero");

const inputInventory = summary.inputIntegrity.map((input, ordinal) => {
  const absolutePath = join(snapshotRoot, ...input.relativePath.split("/"));
  assert(resolve(absolutePath).startsWith(`${resolve(snapshotRoot)}\\`) || resolve(absolutePath) === resolve(snapshotRoot), `input escapes snapshot root: ${input.inputId}`);
  readVerified(absolutePath, input.sizeBytes, input.sha256);
  let sourceId = null;
  let authorityDisposition = "structural-observation-only";
  if (input.inputId === "pgr-en-guide-fight") {
    sourceId = "pgr-guidefight-alt3ri-856a0e45-en-json";
    authorityDisposition = "authoritative-row-selection-input";
  } else if (input.inputId === "pgr-en-stage") {
    sourceId = "pgr-stage-alt3ri-856a0e45-en-json";
    authorityDisposition = "authoritative-stage-join-input-outside-eleven-source-cohort";
  } else if (input.locale === "ZH") {
    authorityDisposition = "compare-only-never-unioned-never-fills-en-absence";
  } else if (input.locale === "provenance") {
    authorityDisposition = "snapshot-provenance-binding";
  }
  return {
    ordinal,
    inputId: input.inputId,
    sourceId,
    locale: input.locale,
    role: input.role,
    authorityDisposition,
    path: `${snapshotRelativeRoot}/${input.relativePath}`,
    sizeBytes: input.sizeBytes,
    sha256: input.sha256,
  };
});

const sourceRecordWithoutDigest = {
  schemaVersion: 1,
  sourceRecordId: "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-SOURCE-RECORD-01",
  contractId: CONTRACT_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  artifactSetId: ARTIFACT_SET_ID,
  sourceSnapshotId: SOURCE_SNAPSHOT_ID,
  status: "exact-static-replacement-candidate-not-admitted",
  recordedAt: "2026-07-16T00:50:00+09:00",
  game: "Punishing: Gray Raven",
  upstream: {
    name: "alt3ri/PGR_Data",
    url: "https://github.com/alt3ri/PGR_Data",
    branch: "master",
    commit: "856a0e4534d0854fa440040e961b74a97ba732e2",
    commitDate: "2026-05-29T23:28:20Z",
    snapshotDate: "2026-06-14",
    capturedAt: "2026-06-14T09:13:26.492069+00:00",
    licenseStatus: "unknown-review-needed",
  },
  sourceRoot: {
    path: "C:/Ark/SubcultureGameData",
    scope: "local-retained-mirror-bounded-candidate",
    snapshotRelativeRoot,
  },
  authorityBoundary: {
    authorityLocale: "EN",
    comparisonLocale: "ZH",
    snapshotsUnioned: false,
    comparisonValuesCopied: false,
    exactIdentityMetadataAllowed: ["GuideFight.Id", "GuideFight.StageId", "Stage.StageId"],
    authoredPayloadValueCopiedCount: 0,
  },
  mainGenerator,
  execution: {
    workingDirectory: "C:/Git/DimensionBrawl/Assets",
    arkSubcultureRoot: "default C:/Ark/SubcultureGameData",
    generateCommand: "& 'C:\\Users\\dharm\\.cache\\codex-runtimes\\codex-primary-runtime\\dependencies\\node\\bin\\node.exe' '_Game\\DesignDocs\\P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs'",
    verifyCommand: "& 'C:\\Users\\dharm\\.cache\\codex-runtimes\\codex-primary-runtime\\dependencies\\node\\bin\\node.exe' '_Game\\DesignDocs\\P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs' --verify",
    generateExitCode: 0,
    verifyExitCode: 0,
  },
  inputs: inputInventory,
  outputs,
  replacementBoundary: {
    relation: "new-versioned-semantic-successor-not-historical-byte-reconstruction",
    historicalSourceIdsPreserved: outputs.map((output) => output.predecessorSourceId),
    historicalPathsReused: false,
    partialAdmissionAllowed: false,
  },
  evidenceBoundary: {
    evidenceGrade: "exact-static-derived-structural",
    runtimeTraceEvidenceRefs: [],
    productAdoptionEffect: "none",
    elevenSourceAdmissionEffect: "none",
    admittedSupportingSourceCount: 0,
    requiredSupportingSourceCount: 9,
    liveForeignRowCount: 0,
    liveCrosswalkCellCount: 0,
  },
  admissionBlockedReasons: [
    "PGR upstream license disposition remains unknown-review-needed.",
    "HI3 replacement outputs and formal helper provenance are not yet complete.",
    "The eleven-source cohort must be admitted atomically; this four-artifact candidate cannot be admitted alone.",
  ],
  sourceRecordPath: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json",
  producerManifestPath: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json",
};
const sourceRecord = { ...sourceRecordWithoutDigest, canonicalSourceRecordDigest: canonicalDigest(sourceRecordWithoutDigest) };

const producerManifestWithoutDigest = {
  schemaVersion: 1,
  producerManifestId: "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-PRODUCER-MANIFEST-01",
  contractId: CONTRACT_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  artifactSetId: ARTIFACT_SET_ID,
  sourceSnapshotId: SOURCE_SNAPSHOT_ID,
  status: "verified-candidate-not-admitted",
  generator: mainGenerator,
  orderedInputs: inputInventory,
  orderedOutputs: outputs,
  selectionInvariant: {
    sort: "GuideFight.Id numeric ascending",
    exactGuideFightToStagePairs: [
      "100001->10010001",
      "100002->10010002",
      "100003->10010003",
      "100004->10010005",
    ],
    guideFightRows: 4,
    enStageJoinRows: 4,
    zhCompareStageJoinRows: 4,
    missing: 0,
    duplicates: 0,
    multiMatches: 0,
  },
  labelContextInvariant: {
    schemaVersion: 1,
    dataRows: 20,
    sourceRows: 4,
    fieldsPerSourceRow: 5,
    fieldOrder: ["Stage.Name", "Stage.Description", "Stage.RecommandLevel", "Stage.RequireLevel", "Stage.StarDesc"],
    enPresentNonempty: 20,
    zhPresentNonempty: 20,
    enTypes: { string: 12, number: 8 },
    zhTypes: { string: 12, number: 8 },
    enZhEqual: 8,
    enZhDifferent: 12,
    lowEntropyFieldValueHashesEmitted: false,
  },
  readingLinksInvariant: {
    schemaVersion: 1,
    dataRows: 56,
    sourceRows: 4,
    semanticSlotsPerSourceRow: 14,
    stateCounts: { present: 32, absent: 20, unresolved: 4 },
    classificationCounts: { provenStatic: 52, unknown: 4 },
    physicalConsumerRows: { state: "unresolved", count: 4 },
  },
  normalization: {
    encoding: "UTF-8",
    bom: false,
    newline: "LF",
    exactlyOneFinalLf: true,
    jsonIndentSpaces: 2,
    canonicalDigest: "recursive ordinal-sorted object keys; compact JSON; arrays preserve authored order; SHA-256 lowercase",
    csvDelimiter: ",",
    csvEveryCellDoubleQuoted: true,
    csvEscaping: "RFC4180 double-quote doubling",
    localeDependentFormatting: false,
  },
  negativeBoundaries: [
    "ZH is compare-only and never unions with EN or fills an EN null.",
    "Identity metadata is not source payload adoption.",
    "Labels, descriptions, NPC/weapon/robot IDs, time, level, cost, reward, story, and tuning payload values are withheld.",
    "Static field families do not prove runtime consumers, evaluators, persistence, transactions, cleanup, rewards, or product parity.",
    "No PGR signal-orb, three-ping, QTE, loadout, or target-time system is imported.",
  ],
  verificationResult: {
    mainGenerate: "PASS exit 0",
    mainVerify: "PASS exit 0",
    sourceValueCopied: 0,
    admissionEffect: "none",
  },
};
const producerManifest = { ...producerManifestWithoutDigest, canonicalProducerManifestDigest: canonicalDigest(producerManifestWithoutDigest) };

const generated = {
  [sourceRecordPath]: `${JSON.stringify(sourceRecord, null, 2)}\n`,
  [producerManifestPath]: `${JSON.stringify(producerManifest, null, 2)}\n`,
};

for (const [path, text] of Object.entries(generated)) {
  assert(!text.includes("\r"), `${path} contains CR`);
  assert(text.endsWith("\n") && !text.endsWith("\n\n"), `${path} must end with exactly one LF`);
  if (process.argv.includes("--verify")) {
    assert(existsSync(path), `missing generated provenance artifact ${path}`);
    assert(readFileSync(path, "utf8") === text, `${path} bytes differ from reconstruction`);
  } else {
    writeFileSync(path, text, "utf8");
  }
}

console.log(`${process.argv.includes("--verify") ? "PASS" : "WROTE"} ${CONTRACT_ID} provenance`);
for (const [path, text] of Object.entries(generated)) {
  const bytes = Buffer.from(text, "utf8");
  console.log(`${path.split(/[\\/]/).at(-1)} sizeBytes=${bytes.length} sha256=${sha256(bytes)}`);
}
console.log(`canonicalSourceRecordDigest=${sourceRecord.canonicalSourceRecordDigest}`);
console.log(`canonicalProducerManifestDigest=${producerManifest.canonicalProducerManifestDigest}`);
console.log("admissionEffect=none");
