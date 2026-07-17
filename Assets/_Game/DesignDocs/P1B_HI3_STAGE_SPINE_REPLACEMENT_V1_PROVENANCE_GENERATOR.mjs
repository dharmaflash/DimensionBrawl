import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-PROVENANCE-GENERATOR-01";
const CONTRACT_ID = "P1B-HI3-STAGE-SPINE-REPLACEMENT-01";
const PRODUCER_CONTRACT_ID = "HI3-STAGEDATA-STAGE-SPINE-PRODUCER-01";
const ARTIFACT_SET_ID = "HI3-STAGEDATA-STAGE-SPINE-NAIRIEBERRY-01D7AFB-V1";
const SOURCE_SNAPSHOT_ID = "hi3-nairieberry-01d7afb-global-stagedata-spine-v1";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";
const snapshotRelativeRoot = "games/honkai-impact-3rd/raw/nairieberry-honkaiimpactdata/2026-06-15";
const snapshotRoot = join(arkRoot, ...snapshotRelativeRoot.split("/"));

const mainGenerator = {
  path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs",
  sizeBytes: 30987,
  sha256: "b35c7eedf641af69fcf4969bb8042171dfe959223c2c44c2a06b0af433cf94e3",
  generatorId: "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-GENERATOR-01",
  runtime: "Node.js v24.14.0 built-ins only",
};

const outputs = [
  {
    sourceId: "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-readfirst-md",
    predecessorSourceId: "hi3-readfirst-md",
    kind: "report",
    artifactId: "P1B-HI3-STAGE-SPINE-READFIRST-V1-MD",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_READFIRST_V1.md",
    sizeBytes: 5319,
    sha256: "6a54ed0b916f22d21e0b2edeee8481cb4e75425b5f994f186adb93b59b0e6db4",
    schemaVersion: 1,
    dataRowCount: 1,
  },
  {
    sourceId: "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-summary-json",
    predecessorSourceId: "hi3-readfirst-summary-json",
    kind: "derived",
    artifactId: "P1B-HI3-STAGE-SPINE-READFIRST-V1-SUMMARY-JSON",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_READFIRST_V1_SUMMARY.json",
    sizeBytes: 25723,
    sha256: "379bc89fd6bd80854acc90c074dc4ce8e13491e08f4e5360edb061bd97592e87",
    schemaVersion: 1,
    dataRowCount: 1,
    canonicalReportDigest: "d20113431ca54b1da5bc1f6c477b32de0fa9eb205f67d3e33cdaaafe4f6f7101",
  },
  {
    sourceId: "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-reading-links-csv",
    predecessorSourceId: "hi3-readfirst-csv",
    kind: "derived",
    artifactId: "P1B-HI3-STAGEDATA-STAGE-READING-LINKS-V1-CSV",
    path: "_Game/DesignDocs/P1B_HI3_STAGEDATA_STAGE_READING_LINKS_V1.csv",
    sizeBytes: 7735,
    sha256: "2657ab04fa749d5e1f945037e3e49efc6fd62a8ef2c9fec9a5eeb1273a06e639",
    schemaVersion: 1,
    dataRowCount: 14,
  },
];

const sourceRecordPath = join(here, "P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json");
const producerManifestPath = join(here, "P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json");

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
  const actualSha256 = sha256(bytes);
  assert(bytes.length === expectedSizeBytes, `size mismatch for ${path}: ${bytes.length}`);
  assert(actualSha256 === expectedSha256, `SHA-256 mismatch for ${path}: ${actualSha256}`);
  return bytes;
}

readVerified(join(workspaceRoot, ...mainGenerator.path.split("/")), mainGenerator.sizeBytes, mainGenerator.sha256);
for (const output of outputs) {
  const bytes = readVerified(join(workspaceRoot, ...output.path.split("/")), output.sizeBytes, output.sha256);
  const text = bytes.toString("utf8");
  assert(!text.includes("\r"), `${output.sourceId} contains CR`);
  assert(text.endsWith("\n") && !text.endsWith("\n\n"), `${output.sourceId} must end with exactly one LF`);
  if (output.path.endsWith(".csv")) assert(text.split("\n").length - 2 === output.dataRowCount, `${output.sourceId} data-row count changed`);
}

const summary = JSON.parse(readFileSync(join(workspaceRoot, ...outputs[1].path.split("/")), "utf8"));
const { canonicalReportDigest, ...summaryWithoutDigest } = summary;
assert(canonicalReportDigest === outputs[1].canonicalReportDigest, "summary canonical digest field changed");
assert(canonicalDigest(summaryWithoutDigest) === canonicalReportDigest, "summary canonical digest reconstruction failed");
assert(summary.selection.sourceRowCount === 9642, "summary source row count changed");
assert(summary.selection.matchCount === 1 && summary.selection.oneBasedOrdinal === 2, "summary target selection changed");
assert(summary.selection.rowKey === "levelId=10101", "summary target identity changed");
assert(summary.fieldShapeContract.fieldCount === 67, "summary field-shape count changed");
assert(summary.fieldShapeContract.canonicalSha256 === "19833743758af7f5987d0fb591c82d9e275eb82e57d8c2d2c5ff806306abbb91", "summary field-shape digest changed");
assert(summary.readingLinksContract.rowCount === 14, "summary reading-link row count changed");
assert(summary.readingLinksContract.valueStateCounts.present === 10, "summary present count changed");
assert(summary.readingLinksContract.valueStateCounts.unresolved === 4, "summary unresolved count changed");
assert(summary.readingLinksContract.sourceValueCopiedCount === 0, "summary copied payload count must be zero");
assert(summary.siblingHelperBoundary.usedAsProducerInputs === false, "helper outputs must remain outside producer inputs");

const inputInventory = summary.inputIntegrity.map((input, ordinal) => {
  const absolutePath = join(snapshotRoot, ...input.relativePath.split("/"));
  assert(resolve(absolutePath).startsWith(`${resolve(snapshotRoot)}\\`) || resolve(absolutePath) === resolve(snapshotRoot), `input escapes snapshot root: ${input.inputId}`);
  readVerified(absolutePath, input.sizeBytes, input.sha256);
  return {
    ordinal,
    inputId: input.inputId,
    sourceId: input.inputId === "hi3-global-stage-data-main" ? "hi3-stagedata-main-nairieberry-01d7afb-global-json" : null,
    locale: "Global",
    role: input.role,
    authorityDisposition: input.inputId === "hi3-global-stage-data-main" ? "authoritative-exact-row-selection-input" : "snapshot-provenance-binding",
    path: `${snapshotRelativeRoot}/${input.relativePath}`,
    sizeBytes: input.sizeBytes,
    sha256: input.sha256,
  };
});

const sourceRecordWithoutDigest = {
  schemaVersion: 1,
  sourceRecordId: "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-SOURCE-RECORD-01",
  contractId: CONTRACT_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  artifactSetId: ARTIFACT_SET_ID,
  sourceSnapshotId: SOURCE_SNAPSHOT_ID,
  status: "exact-static-replacement-candidate-not-admitted",
  recordedAt: "2026-07-16T02:45:00+09:00",
  game: "Honkai Impact 3rd",
  upstream: {
    name: "nairieberry/HonkaiImpactData",
    url: "https://github.com/nairieberry/HonkaiImpactData",
    branch: "master",
    commit: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
    commitDate: "2021-04-10T22:17:18Z",
    snapshotDate: "2026-06-15",
    licenseStatus: "none-detected-review-needed",
  },
  sourceRoot: {
    path: "C:/Ark/SubcultureGameData",
    scope: "local-retained-mirror-bounded-candidate",
    snapshotRelativeRoot,
  },
  authorityBoundary: {
    authorityLocale: "Global",
    crossLocaleUnionAllowed: false,
    exactIdentityMetadataAllowed: ["StageData_Main.levelId=10101"],
    authoredPayloadValueCopiedCount: 0,
    officialCurrentShippedBehaviorClaimed: false,
    newerDataStateClaimed: false,
  },
  mainGenerator,
  execution: {
    workingDirectory: "C:/Git/DimensionBrawl/Assets",
    arkSubcultureRoot: "default C:/Ark/SubcultureGameData",
    generateCommand: "& 'C:\\Users\\dharm\\AppData\\Local\\OpenAI\\Codex\\bin\\node.exe' '_Game\\DesignDocs\\P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs'",
    verifyCommand: "& 'C:\\Users\\dharm\\AppData\\Local\\OpenAI\\Codex\\bin\\node.exe' '_Game\\DesignDocs\\P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs' --verify",
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
  siblingHelperBoundary: {
    sourceIds: ["hi3-stage-summary-csv", "hi3-stage-samples-csv"],
    producerInputCount: 0,
    provenanceDisposition: "byte-exact-replay-authenticated-formal-admission-open",
    samplesNegativeBoundary: "The truncated sample helper does not independently identify levelId 10101.",
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
    "HI3 upstream license disposition remains none-detected-review-needed.",
    "The two HI3 replay-authenticated helper outputs still require formal provenance records and admission metadata.",
    "The eleven-source cohort must be admitted atomically; this three-artifact candidate cannot be admitted alone.",
  ],
  sourceRecordPath: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json",
  producerManifestPath: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json",
};
const sourceRecord = { ...sourceRecordWithoutDigest, canonicalSourceRecordDigest: canonicalDigest(sourceRecordWithoutDigest) };

const producerManifestWithoutDigest = {
  schemaVersion: 1,
  producerManifestId: "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-PRODUCER-MANIFEST-01",
  contractId: CONTRACT_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  artifactSetId: ARTIFACT_SET_ID,
  sourceSnapshotId: SOURCE_SNAPSHOT_ID,
  status: "verified-candidate-not-admitted",
  generator: mainGenerator,
  orderedInputs: inputInventory,
  orderedOutputs: outputs,
  selectionInvariant: {
    selector: "typeof row.levelId === number && row.levelId === 10101",
    sourceRows: 9642,
    duplicateLevelIds: 0,
    matches: 1,
    sourceOrdinal: 2,
    targetKey: "levelId=10101",
    topLevelFields: 67,
    canonicalRowSizeBytes: 1665,
    canonicalRowSha256: "93eb25ca807d6a7f5230cd1ca52e66d68c9f956db3eab25d8013d338699c968f",
    keySetSha256: "bf6bba4b74ba32cfc80828ba569dc3fc96ae578406c43ac160b4b2ad6a226eec",
  },
  fieldShapeInvariant: {
    schemaVersion: 1,
    fieldRows: 67,
    canonicalSizeBytes: 3907,
    canonicalSha256: "19833743758af7f5987d0fb591c82d9e275eb82e57d8c2d2c5ff806306abbb91",
    stateCounts: {
      arrayEmpty: 5,
      arrayPresent: 11,
      numberNonzero: 25,
      numberZero: 16,
      objectPresent: 5,
      stringEmpty: 2,
      stringPresent: 3,
    },
    sourceValuesStored: false,
  },
  readingLinksInvariant: {
    schemaVersion: 1,
    dataRows: 14,
    sourceRows: 1,
    semanticSlotsPerSourceRow: 14,
    stateCounts: { present: 10, unresolved: 4 },
    classificationCounts: { provenStatic: 10, unknown: 4 },
    sourceValueCopied: 0,
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
    "The selected upstream commit is from 2021 and proves no newer or official current shipped HI3 state.",
    "The exact levelId is identity metadata, not source payload adoption.",
    "Localized hashes/text, script/image paths, list contents, linked identifiers, time, level, cost, reward, story, and tuning values are withheld.",
    "Static field families do not prove runtime consumers, evaluators, persistence, transactions, cleanup, rewards, or product parity.",
    "The two replay-authenticated helper outputs are sibling evidence, not this producer's inputs.",
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
