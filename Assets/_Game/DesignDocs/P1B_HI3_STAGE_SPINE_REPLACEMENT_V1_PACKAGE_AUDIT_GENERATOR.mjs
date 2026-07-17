import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const AUDITOR_ID = "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-PACKAGE-AUDITOR-01";
const CONTRACT_ID = "P1B-HI3-STAGE-SPINE-REPLACEMENT-01";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const auditPath = join(here, "P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json");

const packageFiles = [
  { role: "main-generator", path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs", sizeBytes: 30987, sha256: "b35c7eedf641af69fcf4969bb8042171dfe959223c2c44c2a06b0af433cf94e3" },
  { role: "read-first-report", path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_READFIRST_V1.md", sizeBytes: 5319, sha256: "6a54ed0b916f22d21e0b2edeee8481cb4e75425b5f994f186adb93b59b0e6db4" },
  { role: "summary", path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_READFIRST_V1_SUMMARY.json", sizeBytes: 25723, sha256: "379bc89fd6bd80854acc90c074dc4ce8e13491e08f4e5360edb061bd97592e87" },
  { role: "reading-links", path: "_Game/DesignDocs/P1B_HI3_STAGEDATA_STAGE_READING_LINKS_V1.csv", sizeBytes: 7735, sha256: "2657ab04fa749d5e1f945037e3e49efc6fd62a8ef2c9fec9a5eeb1273a06e639" },
  { role: "provenance-generator", path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PROVENANCE_GENERATOR.mjs", sizeBytes: 14206, sha256: "e479714740c5dcc58becef6d3a7338d63ff3ba7c9cc706e798f9421a21846f8e" },
  { role: "source-record", path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json", sizeBytes: 7377, sha256: "6cbd00c4376976435869d336333188ccfeaea89ff72ae9ac50647c0c938d5cd5" },
  { role: "producer-manifest", path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json", sizeBytes: 6773, sha256: "a79380decc3be34e409b4bdf3c1fb085f11cc77514a2cb88fcd63919be99e6bd" },
];

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

function readPinned(file) {
  const bytes = readFileSync(join(workspaceRoot, ...file.path.split("/")));
  const actualSha256 = sha256(bytes);
  assert(bytes.length === file.sizeBytes, `${file.role} size changed: ${bytes.length}`);
  assert(actualSha256 === file.sha256, `${file.role} hash changed: ${actualSha256}`);
  return bytes;
}

const fileBytes = new Map(packageFiles.map((file) => [file.role, readPinned(file)]));
const summary = JSON.parse(fileBytes.get("summary").toString("utf8"));
const sourceRecord = JSON.parse(fileBytes.get("source-record").toString("utf8"));
const producerManifest = JSON.parse(fileBytes.get("producer-manifest").toString("utf8"));

const { canonicalReportDigest, ...summaryPayload } = summary;
const { canonicalSourceRecordDigest, ...sourceRecordPayload } = sourceRecord;
const { canonicalProducerManifestDigest, ...producerManifestPayload } = producerManifest;
assert(canonicalDigest(summaryPayload) === canonicalReportDigest, "summary canonical digest failed");
assert(canonicalDigest(sourceRecordPayload) === canonicalSourceRecordDigest, "source record canonical digest failed");
assert(canonicalDigest(producerManifestPayload) === canonicalProducerManifestDigest, "producer manifest canonical digest failed");

assert(summary.replacementContractId === CONTRACT_ID, "summary contract mismatch");
assert(sourceRecord.contractId === CONTRACT_ID && producerManifest.contractId === CONTRACT_ID, "provenance contract mismatch");
assert(sourceRecord.status === "exact-static-replacement-candidate-not-admitted", "source record status changed");
assert(producerManifest.status === "verified-candidate-not-admitted", "manifest status changed");
assert(sourceRecord.evidenceBoundary.admittedSupportingSourceCount === 0, "supporting source admission must remain zero");
assert(sourceRecord.evidenceBoundary.liveForeignRowCount === 0, "live rows must remain zero");
assert(sourceRecord.evidenceBoundary.liveCrosswalkCellCount === 0, "live cells must remain zero");
assert(sourceRecord.evidenceBoundary.elevenSourceAdmissionEffect === "none", "atomic gate effect must remain none");
assert(sourceRecord.authorityBoundary.crossLocaleUnionAllowed === false, "cross-locale union must remain forbidden");
assert(sourceRecord.authorityBoundary.authoredPayloadValueCopiedCount === 0, "payload copy count changed");
assert(sourceRecord.authorityBoundary.officialCurrentShippedBehaviorClaimed === false, "official-current claim escaped boundary");
assert(sourceRecord.authorityBoundary.newerDataStateClaimed === false, "newer-data claim escaped boundary");

const outputBySourceId = new Map(sourceRecord.outputs.map((output) => [output.sourceId, output]));
assert(outputBySourceId.size === 3 && producerManifest.orderedOutputs.length === 3, "output cardinality must remain three");
for (const manifestOutput of producerManifest.orderedOutputs) {
  const recordOutput = outputBySourceId.get(manifestOutput.sourceId);
  assert(recordOutput, `manifest output not found in source record: ${manifestOutput.sourceId}`);
  assert(canonicalize(recordOutput) === canonicalize(manifestOutput), `output binding differs: ${manifestOutput.sourceId}`);
  const packageFile = packageFiles.find((file) => file.path === manifestOutput.path);
  assert(packageFile && packageFile.sizeBytes === manifestOutput.sizeBytes && packageFile.sha256 === manifestOutput.sha256, `output is not pinned by package inventory: ${manifestOutput.sourceId}`);
}

assert(sourceRecord.inputs.length === 4 && producerManifest.orderedInputs.length === 4, "input inventory must contain four entries");
assert(canonicalize(sourceRecord.inputs) === canonicalize(producerManifest.orderedInputs), "source/manifest input inventory differs");
assert(sourceRecord.inputs.filter((input) => input.authorityDisposition === "authoritative-exact-row-selection-input").length === 1, "authority input count must be one");
assert(sourceRecord.inputs.filter((input) => input.sourceId !== null).map((input) => input.sourceId).join("|") === "hi3-stagedata-main-nairieberry-01d7afb-global-json", "registered input-source set changed");

assert(summary.selection.sourceRowCount === 9642, "source row count changed");
assert(summary.selection.matchCount === 1 && summary.selection.oneBasedOrdinal === 2, "target selection changed");
assert(summary.fieldShapeContract.fieldCount === 67, "field-shape count changed");
assert(summary.fieldShapeContract.canonicalSha256 === "19833743758af7f5987d0fb591c82d9e275eb82e57d8c2d2c5ff806306abbb91", "field-shape digest changed");
assert(summary.readingLinksContract.rowCount === 14 && producerManifest.readingLinksInvariant.dataRows === 14, "reading-link row count changed");
assert(canonicalize(summary.readingLinksContract.valueStateCounts) === canonicalize({ present: 10, unresolved: 4 }), "reading-link states changed");
assert(summary.sourceValuePolicy.sourceValueCopiedCount === 0 && producerManifest.verificationResult.sourceValueCopied === 0, "payload copy count changed");
assert(sourceRecord.siblingHelperBoundary.producerInputCount === 0, "helper source became a producer input");
assert(sourceRecord.siblingHelperBoundary.provenanceDisposition === "byte-exact-replay-authenticated-formal-admission-open", "helper provenance boundary changed");

const predecessorSet = new Set(sourceRecord.outputs.map((output) => output.predecessorSourceId));
assert(predecessorSet.size === 3, "predecessor relation must be one-to-one");
assert(sourceRecord.replacementBoundary.historicalPathsReused === false, "historical path reuse is forbidden");
assert(sourceRecord.replacementBoundary.partialAdmissionAllowed === false, "partial admission is forbidden");
assert(sourceRecord.upstream.licenseStatus === "none-detected-review-needed", "license state must remain explicit until resolved");

const packageDigestRows = packageFiles.map((file) => `${file.role}|${file.path}|${file.sizeBytes}|${file.sha256}`);
const packageDigest = sha256(Buffer.from(`${packageDigestRows.join("\n")}\n`, "utf8"));
const auditWithoutDigest = {
  schemaVersion: 1,
  auditId: "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-PACKAGE-AUDIT-01",
  contractId: CONTRACT_ID,
  status: "pass-exact-static-candidate-not-admitted",
  auditedAt: "2026-07-16T03:00:00+09:00",
  packageFiles,
  packageDigestEncoding: "ordered role|path|sizeBytes|lowercaseSha256 rows; LF; final LF; UTF-8; SHA-256 lowercase",
  packageDigest,
  canonicalDigests: {
    report: canonicalReportDigest,
    sourceRecord: canonicalSourceRecordDigest,
    producerManifest: canonicalProducerManifestDigest,
  },
  verifiedCounts: {
    packageFiles: packageFiles.length,
    inputs: sourceRecord.inputs.length,
    outputs: sourceRecord.outputs.length,
    exactRows: 1,
    fieldShapeRows: summary.fieldShapeContract.fieldCount,
    linkRows: summary.readingLinksContract.rowCount,
    linkPresent: summary.readingLinksContract.valueStateCounts.present,
    linkUnresolved: summary.readingLinksContract.valueStateCounts.unresolved,
    sourceValueCopied: summary.sourceValuePolicy.sourceValueCopiedCount,
    helperInputs: sourceRecord.siblingHelperBoundary.producerInputCount,
  },
  admissionState: {
    candidateOutputsVerified: 3,
    supportingSourcesAdmitted: 0,
    supportingSourcesRequired: 9,
    liveRows: 0,
    liveRowsRequired: 5,
    liveCrosswalkCells: 0,
    liveCrosswalkCellsRequired: 70,
    acceptanceEffect: "none",
  },
  blockersToAdmission: sourceRecord.admissionBlockedReasons,
  negativeBoundary: "This audit authenticates only the deterministic HI3 candidate package. It does not reconstruct historical bytes, admit a source, authorize license reuse, promote either helper, populate packet claims/cells, or prove runtime/current-product behavior.",
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
console.log("candidateOutputsVerified=3 supportingSourcesAdmitted=0 liveRows=0 liveCells=0 admissionEffect=none");
