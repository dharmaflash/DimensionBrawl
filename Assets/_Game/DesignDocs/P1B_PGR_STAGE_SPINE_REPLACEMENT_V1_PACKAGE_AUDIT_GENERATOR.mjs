import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const AUDITOR_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-PACKAGE-AUDITOR-01";
const CONTRACT_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-01";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const auditPath = join(here, "P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json");

const packageFiles = [
  { role: "main-generator", path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_GENERATOR.mjs", sizeBytes: 37178, sha256: "3284524cec2eb68ccc430aabeec4b08fc9bb70fedb03715b178814d12bc92f87" },
  { role: "read-first-report", path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_READFIRST_V1.md", sizeBytes: 5526, sha256: "f606f014a3e12101bae89918c16ac68c6184eeca0290b82800b5e50dae2c7caf" },
  { role: "summary", path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_READFIRST_V1_SUMMARY.json", sizeBytes: 19861, sha256: "f400b09476b33c338c8b5f670d263d0152a0201c2c92078963bf3ae30bc276bb" },
  { role: "label-context", path: "_Game/DesignDocs/P1B_PGR_GUIDEFIGHT_STAGE_LABEL_CONTEXT_V1.csv", sizeBytes: 8694, sha256: "0159d965438aa23b60b1e6b66cf7c1bff8b4edc21f8e0a50531d92266357d153" },
  { role: "reading-links", path: "_Game/DesignDocs/P1B_PGR_GUIDEFIGHT_STAGE_READING_LINKS_V1.csv", sizeBytes: 18714, sha256: "312811436a34b38888582289a93c6a60e6f2ccd44b6d22eef88c6c5bc4e6170d" },
  { role: "provenance-generator", path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PROVENANCE_GENERATOR.mjs", sizeBytes: 14296, sha256: "ff06cdb7b07351caf5dad28db8a893e24c7dd8d95b73c43c0fbc563ebe71fb6f" },
  { role: "source-record", path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json", sizeBytes: 13036, sha256: "1972dd751ce99e4d1d6bd44727413077975693198136e3894b1dae5626ebf51c" },
  { role: "producer-manifest", path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json", sizeBytes: 12684, sha256: "0a1f29d657d8360d2b1f6b22e59adfba9e496ca3728c182de6ab6bc51d4f4a6e" },
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
  assert(bytes.length === file.sizeBytes, `${file.role} size changed: ${bytes.length}`);
  assert(sha256(bytes) === file.sha256, `${file.role} hash changed: ${sha256(bytes)}`);
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

assert(summary.contractId === undefined, "obsolete contractId alias unexpectedly present");
assert(summary.replacementContractId === CONTRACT_ID, "summary contract mismatch");
assert(sourceRecord.contractId === CONTRACT_ID && producerManifest.contractId === CONTRACT_ID, "provenance contract mismatch");
assert(sourceRecord.status === "exact-static-replacement-candidate-not-admitted", "source record status changed");
assert(producerManifest.status === "verified-candidate-not-admitted", "manifest status changed");
assert(sourceRecord.evidenceBoundary.admittedSupportingSourceCount === 0, "supporting source admission must remain zero");
assert(sourceRecord.evidenceBoundary.liveForeignRowCount === 0, "live rows must remain zero");
assert(sourceRecord.evidenceBoundary.liveCrosswalkCellCount === 0, "live cells must remain zero");
assert(sourceRecord.evidenceBoundary.elevenSourceAdmissionEffect === "none", "atomic gate effect must remain none");
assert(sourceRecord.authorityBoundary.snapshotsUnioned === false && sourceRecord.authorityBoundary.comparisonValuesCopied === false, "EN/ZH boundary changed");
assert(sourceRecord.authorityBoundary.authoredPayloadValueCopiedCount === 0, "payload copy count changed");

const outputBySourceId = new Map(sourceRecord.outputs.map((output) => [output.sourceId, output]));
assert(outputBySourceId.size === 4 && producerManifest.orderedOutputs.length === 4, "output cardinality must remain four");
for (const manifestOutput of producerManifest.orderedOutputs) {
  const recordOutput = outputBySourceId.get(manifestOutput.sourceId);
  assert(recordOutput, `manifest output not found in source record: ${manifestOutput.sourceId}`);
  assert(canonicalize(recordOutput) === canonicalize(manifestOutput), `output binding differs: ${manifestOutput.sourceId}`);
  const packageFile = packageFiles.find((file) => file.path === manifestOutput.path);
  assert(packageFile && packageFile.sizeBytes === manifestOutput.sizeBytes && packageFile.sha256 === manifestOutput.sha256, `output is not pinned by package inventory: ${manifestOutput.sourceId}`);
}

assert(sourceRecord.inputs.length === 15 && producerManifest.orderedInputs.length === 15, "input inventory must contain 15 entries");
assert(canonicalize(sourceRecord.inputs) === canonicalize(producerManifest.orderedInputs), "source/manifest input inventory differs");
assert(sourceRecord.inputs.filter((input) => input.authorityDisposition === "authoritative-row-selection-input").length === 1, "GuideFight authority count must be one");
assert(sourceRecord.inputs.filter((input) => input.authorityDisposition === "authoritative-stage-join-input-outside-eleven-source-cohort").length === 1, "Stage join authority count must be one");
assert(sourceRecord.inputs.filter((input) => input.locale === "ZH").every((input) => input.authorityDisposition === "compare-only-never-unioned-never-fills-en-absence"), "ZH input escaped compare-only disposition");
assert(sourceRecord.inputs.filter((input) => input.sourceId !== null).map((input) => input.sourceId).sort().join("|") === "pgr-guidefight-alt3ri-856a0e45-en-json|pgr-stage-alt3ri-856a0e45-en-json", "registered input-source set changed");

assert(summary.labelContextContract.rowCount === 20, "label row count changed");
assert(summary.readingLinksContract.rowCount === 56, "link row count changed");
assert(producerManifest.labelContextInvariant.dataRows === 20, "manifest label row count changed");
assert(producerManifest.readingLinksInvariant.dataRows === 56, "manifest link row count changed");
assert(canonicalize(summary.readingLinksContract.stateCounts) === canonicalize({ present: 32, absent: 20, unresolved: 4 }), "link states changed");
assert(summary.sourceValuePolicy.sourceValueCopiedCount === 0 && producerManifest.verificationResult.sourceValueCopied === 0, "payload copy count changed");

const predecessorSet = new Set(sourceRecord.outputs.map((output) => output.predecessorSourceId));
assert(predecessorSet.size === 4, "predecessor relation must be one-to-one");
assert(sourceRecord.replacementBoundary.historicalPathsReused === false, "historical path reuse is forbidden");
assert(sourceRecord.replacementBoundary.partialAdmissionAllowed === false, "partial admission is forbidden");
assert(sourceRecord.upstream.licenseStatus === "unknown-review-needed", "license state must remain explicit until resolved");

const packageDigestRows = packageFiles.map((file) => `${file.role}|${file.path}|${file.sizeBytes}|${file.sha256}`);
const packageDigest = sha256(Buffer.from(`${packageDigestRows.join("\n")}\n`, "utf8"));
const auditWithoutDigest = {
  schemaVersion: 1,
  auditId: "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-PACKAGE-AUDIT-01",
  contractId: CONTRACT_ID,
  status: "pass-exact-static-candidate-not-admitted",
  auditedAt: "2026-07-16T01:05:00+09:00",
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
    exactRows: summary.exactRows.length,
    labelRows: summary.labelContextContract.rowCount,
    linkRows: summary.readingLinksContract.rowCount,
    linkPresent: summary.readingLinksContract.stateCounts.present,
    linkAbsent: summary.readingLinksContract.stateCounts.absent,
    linkUnresolved: summary.readingLinksContract.stateCounts.unresolved,
    sourceValueCopied: summary.sourceValuePolicy.sourceValueCopiedCount,
  },
  admissionState: {
    candidateOutputsVerified: 4,
    supportingSourcesAdmitted: 0,
    supportingSourcesRequired: 9,
    liveRows: 0,
    liveRowsRequired: 5,
    liveCrosswalkCells: 0,
    liveCrosswalkCellsRequired: 70,
    acceptanceEffect: "none",
  },
  blockersToAdmission: sourceRecord.admissionBlockedReasons,
  negativeBoundary: "This audit authenticates only the deterministic candidate package. It does not reconstruct historical bytes, admit a source, authorize license reuse, populate packet claims/cells, or prove runtime/product behavior.",
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
console.log("candidateOutputsVerified=4 supportingSourcesAdmitted=0 liveRows=0 liveCells=0 admissionEffect=none");
