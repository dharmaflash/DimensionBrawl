import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const AUDITOR_ID = "P1B-HI3-STAGE-HELPER-PROVENANCE-V1-PACKAGE-AUDITOR-01";
const CONTRACT_ID = "P1B-HI3-STAGE-HELPER-PROVENANCE-01";
const ARTIFACT_SET_ID = "HI3-STAGE-HELPERS-MIXED-20260615-V1";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const arkRoot = "C:/Ark/SubcultureGameData";
const auditPath = join(here, "P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT.json");

const packageFiles = [
  {
    role: "inventory-generator",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_INPUT_INVENTORY_V1_GENERATOR.mjs",
    sizeBytes: 13998,
    sha256: "8ac184f12fc3f7d5635628875745fa1a4986d5b3c51f13bee9c0c76833e1c5e3",
  },
  {
    role: "materialized-inventory",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_INPUT_INVENTORY_V1.csv",
    sizeBytes: 278176,
    sha256: "9eb12152642ff031d758415645b9ab95b6e312aab1a080e8775ea9dba653dc5c",
  },
  {
    role: "isolated-replay-wrapper",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py",
    sizeBytes: 6449,
    sha256: "4c4ff6ff726dfb0e24138a8d57dcd38efd83f70f279791ac62c924d398afe2ec",
  },
  {
    role: "isolated-replay-result",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json",
    sizeBytes: 1155,
    sha256: "fc6fef25cb5eccff9e77170ee377c48bcda8819a86458e2d336d9ca2ea69f6f8",
  },
  {
    role: "provenance-generator",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_GENERATOR.mjs",
    sizeBytes: 22493,
    sha256: "1091e0f312223c770a84b309fb429f3b1aefaf5bff3cbc6bc675f44de139ae9a",
  },
  {
    role: "source-record",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_SOURCE_RECORD.json",
    sizeBytes: 11522,
    sha256: "30d124125363ca55648f1956b89b574dee07ad3be6ac376042104a93734fce6d",
  },
  {
    role: "producer-manifest",
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PRODUCER_MANIFEST.json",
    sizeBytes: 10809,
    sha256: "fa08d733ff9ead59468d4ac8f9317c196a034e84da000b974754bf1ec24c7296",
  },
];

const workspaceDependencyRoles = new Set([
  "bounded-replay-wrapper",
  "bounded-replay-result",
  "materialized-inventory-generator",
  "materialized-inventory",
]);

function fail(message) {
  throw new Error(`${AUDITOR_ID}: ${message}`);
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

function pathFrom(root, relativePath) {
  return join(root, ...relativePath.split("/"));
}

function readPinned(root, file) {
  const path = pathFrom(root, file.path);
  const bytes = readFileSync(path);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === file.sizeBytes, `${file.role ?? file.path} size changed: ${bytes.length}`);
  assert(actualSha256 === file.sha256, `${file.role ?? file.path} hash changed: ${actualSha256}`);
  return bytes;
}

function parseCsv(text, newline) {
  assert(newline === "\n" || newline === "\r\n", "unsupported CSV newline contract");
  assert(text.endsWith(newline) && !text.endsWith(newline + newline), "CSV final-newline contract changed");
  if (newline === "\r\n") {
    assert(!text.replaceAll("\r\n", "").includes("\n"), "CSV contains bare LF outside CRLF contract");
    assert(!text.replaceAll("\r\n", "").includes("\r"), "CSV contains bare CR outside CRLF contract");
  } else {
    assert(!text.includes("\r"), "CSV contains CR outside LF contract");
  }

  const records = [];
  let record = [];
  let field = "";
  let inQuotes = false;
  for (let index = 0; index < text.length; index += 1) {
    const char = text[index];
    if (inQuotes) {
      if (char === '"' && text[index + 1] === '"') {
        field += '"';
        index += 1;
      } else if (char === '"') {
        inQuotes = false;
      } else {
        field += char;
      }
      continue;
    }

    if (char === '"') {
      inQuotes = true;
    } else if (char === ",") {
      record.push(field);
      field = "";
    } else if (newline === "\r\n" && char === "\r" && text[index + 1] === "\n") {
      record.push(field);
      records.push(record);
      record = [];
      field = "";
      index += 1;
    } else if (newline === "\n" && char === "\n") {
      record.push(field);
      records.push(record);
      record = [];
      field = "";
    } else {
      field += char;
    }
  }
  assert(!inQuotes && record.length === 0 && field === "", "CSV parser ended with an incomplete record");
  return records;
}

const packageBytes = new Map(packageFiles.map((file) => [file.role, readPinned(workspaceRoot, file)]));
const sourceRecord = JSON.parse(packageBytes.get("source-record").toString("utf8"));
const producerManifest = JSON.parse(packageBytes.get("producer-manifest").toString("utf8"));
const replay = JSON.parse(packageBytes.get("isolated-replay-result").toString("utf8"));

const { canonicalSourceRecordDigest, ...sourceRecordPayload } = sourceRecord;
const { canonicalProducerManifestDigest, ...producerManifestPayload } = producerManifest;
assert(canonicalDigest(sourceRecordPayload) === canonicalSourceRecordDigest, "source-record canonical digest failed");
assert(canonicalDigest(producerManifestPayload) === canonicalProducerManifestDigest, "producer-manifest canonical digest failed");

assert(sourceRecord.contractId === CONTRACT_ID && producerManifest.contractId === CONTRACT_ID, "contract identity changed");
assert(sourceRecord.artifactSetId === ARTIFACT_SET_ID && producerManifest.artifactSetId === ARTIFACT_SET_ID, "artifact-set identity changed");
assert(sourceRecord.status === "formal-provenance-candidate-verified-admission-open", "source-record status changed");
assert(producerManifest.status === "verified-provenance-candidate-not-admitted", "producer-manifest status changed");
assert(sourceRecord.historicalIdentityBoundary.identityDisposition === "existing historical output bytes retained under existing source IDs; no successor identity created", "historical identity boundary changed");
assert(sourceRecord.historicalIdentityBoundary.partialAdmissionAllowed === false, "partial helper admission became allowed");
assert(sourceRecord.historicalIdentityBoundary.targetSourceIds.join("|") === "hi3-stage-summary-csv|hi3-stage-samples-csv", "historical helper IDs changed");

assert(producerManifest.orderedProvenanceDependencies.length === 15, "provenance dependency count changed");
const dependencyBytes = new Map();
for (const dependency of producerManifest.orderedProvenanceDependencies) {
  const root = workspaceDependencyRoles.has(dependency.role) ? workspaceRoot : arkRoot;
  dependencyBytes.set(dependency.role, readPinned(root, dependency));
}
assert(dependencyBytes.get("retained-producer").toString("utf8").includes("def make_stage_helpers("), "retained producer function is missing");

assert(sourceRecord.upstreamSnapshots.length === 3, "upstream snapshot count changed");
assert(sourceRecord.upstreamSnapshots.map((snapshot) => snapshot.sourceShort).join("|") === "devilpromt|nairieberry|msktmi", "upstream order changed");
assert(sourceRecord.upstreamSnapshots.map((snapshot) => snapshot.selectedInputCount).join("|") === "371|1138|0", "upstream contribution counts changed");
assert(sourceRecord.upstreamSnapshots.map((snapshot) => snapshot.selectedInputBytes).join("|") === "155164987|301292992|0", "upstream contribution bytes changed");
assert(sourceRecord.upstreamSnapshots.map((snapshot) => snapshot.licenseDisposition).join("|") === "none-detected-review-needed|none-detected-review-needed|agpl-3.0-review-needed", "license review boundary changed");

const inventoryBytes = packageBytes.get("materialized-inventory");
assert(!(inventoryBytes[0] === 0xef && inventoryBytes[1] === 0xbb && inventoryBytes[2] === 0xbf), "inventory gained a UTF-8 BOM");
const inventoryRecords = parseCsv(inventoryBytes.toString("utf8"), "\n");
const inventoryHeader = ["schema_version", "artifact_set_id", "input_ordinal", "source", "relative_path", "size_bytes", "sha256_uppercase"];
assert(JSON.stringify(inventoryRecords[0]) === JSON.stringify(inventoryHeader), "inventory header changed");
assert(inventoryRecords.length === 1510, "inventory row count changed");
const inventoryRows = inventoryRecords.slice(1).map((record, index) => {
  assert(record.length === inventoryHeader.length, `inventory row ${index} column count changed`);
  assert(record[0] === "1" && record[1] === ARTIFACT_SET_ID && Number(record[2]) === index, `inventory row ${index} identity changed`);
  assert(/^[0-9A-F]{64}$/.test(record[6]), `inventory row ${index} SHA format changed`);
  return { source: record[3], relativePath: record[4], sizeBytes: Number(record[5]), sha256Uppercase: record[6] };
});
const inventoryPayload = inventoryRows.map((row) => `${row.source}\t${row.relativePath}\t${row.sizeBytes}\t${row.sha256Uppercase}\n`).join("");
const inventoryDigest = sha256(Buffer.from(inventoryPayload, "utf8"));
assert(inventoryDigest === "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662", "inventory canonical digest changed");
assert(inventoryRows.reduce((sum, row) => sum + row.sizeBytes, 0) === 456457979, "inventory byte total changed");
for (const [source, expectedCount, expectedBytes] of [["devilpromt", 371, 155164987], ["nairieberry", 1138, 301292992], ["msktmi", 0, 0]]) {
  const rows = inventoryRows.filter((row) => row.source === source);
  assert(rows.length === expectedCount, `${source} inventory count changed`);
  assert(rows.reduce((sum, row) => sum + row.sizeBytes, 0) === expectedBytes, `${source} inventory bytes changed`);
}

assert(replay.status === "PASS", "isolated replay status changed");
assert(replay.producerSha256 === sourceRecord.retainedProducer.sha256, "replay producer binding changed");
assert(replay.inputCount === 1509 && replay.inputBytes === 456457979 && replay.inputInventorySha256 === inventoryDigest, "replay inventory binding changed");
assert(replay.outputs.length === 2 && replay.producerCounts.stage_table_rows === 1509 && replay.producerCounts.stage_sample_rows === 14855 && replay.producerCounts.monster_rows === 3788, "replay output counts changed");

assert(sourceRecord.targetOutputs.length === 2 && producerManifest.orderedTargetOutputs.length === 2, "target output cardinality changed");
assert(canonicalize(sourceRecord.targetOutputs) === canonicalize(producerManifest.orderedTargetOutputs), "source/manifest target bindings differ");
const targetBytesById = new Map();
for (const output of sourceRecord.targetOutputs) {
  const bytes = readPinned(arkRoot, output);
  targetBytesById.set(output.sourceId, bytes);
  assert(output.identityDisposition === "existing-historical-output-retained-not-successor", `${output.sourceId} became a successor identity`);
  assert(output.admissionState === "formal-provenance-candidate-not-admitted", `${output.sourceId} admission state changed`);
  assert(!(bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf), `${output.sourceId} gained a UTF-8 BOM`);
  const records = parseCsv(bytes.toString("utf8"), "\r\n");
  assert(records.length - 1 === output.dataRows, `${output.sourceId} data-row count changed`);
  assert(JSON.stringify(records[0]) === JSON.stringify(output.header), `${output.sourceId} header changed`);
  const replayOutput = replay.outputs.find((candidate) => candidate.name === output.path.split("/").at(-1));
  assert(replayOutput && replayOutput.sizeBytes === output.sizeBytes && replayOutput.sha256 === output.sha256 && replayOutput.dataRows === output.dataRows, `${output.sourceId} replay binding changed`);
}

assert(sourceRecord.scratchByproduct.sourceId === "hi3-monster-summary-csv", "scratch output identity changed");
assert(canonicalize([sourceRecord.scratchByproduct]) === canonicalize(producerManifest.excludedScratchByproducts), "scratch exclusion binding differs");
readPinned(arkRoot, sourceRecord.scratchByproduct);
assert(sourceRecord.scratchByproduct.countedAsTargetOutput === false && sourceRecord.scratchByproduct.countedInSupportingNine === false, "scratch output escaped exclusion boundary");
assert(producerManifest.outputSetInvariant.targetOutputCount === 2 && producerManifest.outputSetInvariant.successorIdentityCount === 0 && producerManifest.outputSetInvariant.excludedScratchByproductCount === 1 && producerManifest.outputSetInvariant.admittedCount === 0, "output-set invariant changed");

const sampleRecords = parseCsv(targetBytesById.get("hi3-stage-samples-csv").toString("utf8"), "\r\n");
const stageMainSamples = sampleRecords.slice(1).filter((record) => record[0] === "nairieberry" && record[2] === "Global/ExcelOutputAsset/Decrypted/StageData_Main.json");
assert(stageMainSamples.length === 12, "Global StageData_Main bounded sample count changed");
assert(stageMainSamples.map((record) => record[4]).join("|") === "0|1|2|3|4|5|6|7|8|9|10|11", "Global StageData_Main sample indices changed");
assert(stageMainSamples.every((record) => record[5] !== "10101" && !/\"levelId\"\s*:\s*10101/i.test(record[9])), "sample helper began independently identifying levelId 10101");

assert(sourceRecord.evidenceBoundary.helperFormalProvenanceCandidatesVerified === 2, "verified helper candidate count changed");
assert(sourceRecord.evidenceBoundary.helperSourcesAdmitted === 0 && sourceRecord.evidenceBoundary.admittedSupportingSourceCount === 0, "helper/supporting admission must remain zero");
assert(sourceRecord.evidenceBoundary.liveForeignRowCount === 0 && sourceRecord.evidenceBoundary.liveCrosswalkCellCount === 0, "live evidence must remain zero");
assert(sourceRecord.evidenceBoundary.elevenSourceAdmissionEffect === "none" && sourceRecord.evidenceBoundary.productAdoptionEffect === "none", "atomic/product admission effect changed");
assert(sourceRecord.admissionBlockedReasons.length === 3, "admission blocker ledger changed");

const packageDigestRows = packageFiles.map((file) => `${file.role}|${file.path}|${file.sizeBytes}|${file.sha256}`);
const packageDigest = sha256(Buffer.from(`${packageDigestRows.join("\n")}\n`, "utf8"));
const auditWithoutDigest = {
  schemaVersion: 1,
  auditId: "P1B-HI3-STAGE-HELPER-PROVENANCE-V1-PACKAGE-AUDIT-01",
  contractId: CONTRACT_ID,
  status: "pass-formal-provenance-candidates-not-admitted",
  auditedAt: "2026-07-16T04:05:00+09:00",
  packageFiles,
  packageDigestEncoding: "ordered role|path|sizeBytes|lowercaseSha256 rows; LF; final LF; UTF-8; SHA-256 lowercase",
  packageDigest,
  canonicalDigests: {
    sourceRecord: canonicalSourceRecordDigest,
    producerManifest: canonicalProducerManifestDigest,
    inputInventory: inventoryDigest,
  },
  verifiedCounts: {
    packageFiles: packageFiles.length,
    provenanceDependencies: producerManifest.orderedProvenanceDependencies.length,
    upstreamSnapshots: sourceRecord.upstreamSnapshots.length,
    selectedInputs: inventoryRows.length,
    selectedInputBytes: inventoryRows.reduce((sum, row) => sum + row.sizeBytes, 0),
    targetOutputs: sourceRecord.targetOutputs.length,
    targetDataRows: sourceRecord.targetOutputs.reduce((sum, output) => sum + output.dataRows, 0),
    excludedScratchByproducts: producerManifest.excludedScratchByproducts.length,
    excludedScratchRows: sourceRecord.scratchByproduct.dataRows,
    boundedGlobalStageDataSamples: stageMainSamples.length,
  },
  admissionState: {
    helperCandidatesVerified: 2,
    helperSourcesAdmitted: 0,
    helperSourcesRequired: 2,
    supportingSourcesAdmitted: 0,
    supportingSourcesRequired: 9,
    liveRows: 0,
    liveCrosswalkCells: 0,
    atomicElevenSourceEffect: "none",
    productAdoptionEffect: "none",
  },
  blockersToAdmission: sourceRecord.admissionBlockedReasons,
  negativeBoundary: "This audit authenticates only two existing historical HI3 helper bytes as formal provenance candidates. It creates no successor source identity, admits neither helper, does not count the monster scratch output, does not identify the exact levelId 10101 authority row, does not resolve reuse licensing, does not populate a claim or crosswalk cell, and proves no runtime or current-product behavior.",
};
const audit = { ...auditWithoutDigest, canonicalAuditDigest: canonicalDigest(auditWithoutDigest) };
const outputText = `${JSON.stringify(audit, null, 2)}\n`;
assert(!outputText.includes("\r") && outputText.endsWith("\n") && !outputText.endsWith("\n\n"), "audit normalization failed");

if (process.argv.includes("--verify")) {
  assert(existsSync(auditPath), "audit output is missing");
  assert(readFileSync(auditPath, "utf8") === outputText, "audit output bytes differ from reconstruction");
  console.log(`PASS ${CONTRACT_ID} package audit`);
} else {
  writeFileSync(auditPath, outputText, "utf8");
  console.log(`WROTE ${CONTRACT_ID} package audit`);
}
console.log(`packageDigest=${packageDigest}`);
console.log(`canonicalAuditDigest=${audit.canonicalAuditDigest}`);
console.log(`auditSizeBytes=${Buffer.byteLength(outputText, "utf8")}`);
console.log(`auditSha256=${sha256(Buffer.from(outputText, "utf8"))}`);
console.log("helperCandidatesVerified=2 helperSourcesAdmitted=0 supportingSourcesAdmitted=0 liveRows=0 liveCells=0 admissionEffect=none");
