import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const VALIDATOR_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-REGISTRY-VALIDATOR-01";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const evidenceIndexPath = join(here, "SUBCULTURE_DATASET_EVIDENCE_INDEX.json");
const backlogPath = join(here, "SUBCULTURE_GAP_BACKLOG.json");
const roadmapPath = join(here, "SUBCULTURE_DATASET_GAP_ROADMAP.md");

const candidateSourceIds = [
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-readfirst-md",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-summary-json",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-label-context-csv",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-reading-links-csv",
];
const predecessorSourceIds = [
  "pgr-readfirst-md",
  "pgr-readfirst-summary-json",
  "pgr-guidefight-label-csv",
  "pgr-guidefight-links-csv",
];
const requiredOpenAcceptanceIds = [
  "ACC-EVID-P1B-LIVE-PROVENANCE",
  "ACC-EVID-P1B-EXACT-ROWS",
  "ACC-EVID-P1B-DRIFT-CLASSIFICATION",
];

function fail(message) {
  throw new Error(`${VALIDATOR_ID}: ${message}`);
}

function assert(condition, message) {
  if (!condition) fail(message);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function getUnique(rows, predicate, label) {
  const matches = rows.filter(predicate);
  assert(matches.length === 1, `${label} cardinality must be one, got ${matches.length}`);
  return matches[0];
}

const index = JSON.parse(readFileSync(evidenceIndexPath, "utf8"));
const backlog = JSON.parse(readFileSync(backlogPath, "utf8"));
const roadmap = readFileSync(roadmapPath, "utf8");
assert(index.updatedAt === "2026-07-16T01:05:00+09:00", "evidence index updatedAt mismatch");
assert(backlog.updatedAt === "2026-07-16T01:05:00+09:00", "backlog updatedAt mismatch");

const sourceIds = index.sources.map((source) => source.sourceId);
assert(sourceIds.length === 26 && new Set(sourceIds).size === 26, `source registry must contain 26 unique IDs, got ${sourceIds.length}/${new Set(sourceIds).size}`);
const stageInput = getUnique(index.sources, (source) => source.sourceId === "pgr-stage-alt3ri-856a0e45-en-json", "PGR Stage input");
assert(stageInput.admissionState === "producer-input-candidate-not-packet-source", "Stage input admission state changed");
assert(stageInput.packetAdmissionEffect.startsWith("none"), "Stage input gained packet effect");
assert(stageInput.sizeBytes === 29637115 && stageInput.sha256.toLowerCase() === "7d553ada4ac1cd40e77054be70263260f7b2b2dd15948dc120e7ca806b26f940", "Stage input bytes changed");

for (let indexOrdinal = 0; indexOrdinal < candidateSourceIds.length; indexOrdinal += 1) {
  const candidateId = candidateSourceIds[indexOrdinal];
  const predecessorId = predecessorSourceIds[indexOrdinal];
  const candidate = getUnique(index.sources, (source) => source.sourceId === candidateId, candidateId);
  const predecessor = getUnique(index.sources, (source) => source.sourceId === predecessorId, predecessorId);
  assert(candidate.admissionState === "candidate-not-admitted" && candidate.admissionEffect === "none", `${candidateId} gained admission effect`);
  assert(candidate.predecessorSourceId === predecessorId && predecessor.replacementCandidateSourceId === candidateId, `${candidateId} predecessor relation changed`);
  assert(candidate.replacementRelation === "new-versioned-semantic-successor-not-historical-byte-reconstruction", `${candidateId} replacement relation changed`);
  assert(candidate.inputSourceIds.join("|") === "pgr-guidefight-alt3ri-856a0e45-en-json|pgr-stage-alt3ri-856a0e45-en-json", `${candidateId} input set changed`);
  assert(candidate.hashStatus === "verified" && candidate.evidenceGrade === "exact-static", `${candidateId} evidence state changed`);
  const bytes = readFileSync(join(workspaceRoot, ...candidate.relativePath.split("/")));
  assert(bytes.length === candidate.sizeBytes && sha256(bytes) === candidate.sha256.toLowerCase(), `${candidateId} actual bytes differ`);
  assert(predecessor.pathPresenceStatus === "absent-at-retained-mirror", `${predecessorId} historical path state changed`);
  assert(predecessor.sha256 === null && predecessor.hashStatus === "pending-live-recheck", `${predecessorId} historical identity was rewritten`);
}

const packet = getUnique(index.boundedPackets, (candidate) => candidate.packetId === "P1B-PGR-HI3-STAGE-SPINE-01", "P1-B packet");
assert(packet.inScopeSourceIds.length === 9 && new Set(packet.inScopeSourceIds).size === 9, "packet in-scope supporting set must remain nine unique historical IDs");
assert(candidateSourceIds.every((id) => !packet.inScopeSourceIds.includes(id)), "PGR candidate entered packet scope early");
assert(!packet.inScopeSourceIds.includes(stageInput.sourceId), "Stage producer dependency entered packet scope");
assert(packet.liveRawSourceAdmission.pgr.sourceId === null && packet.liveRawSourceAdmission.hi3.sourceId === null, "live raw source admitted early");
assert(packet.generatedReportPath === null && packet.generatedReportSha256 === null, "active report promoted early");
assert(packet.crosswalkRows.length === 0, "live crosswalk populated early");
assert(packet.supportingReplacementCandidates.pgr.candidateCount === 4 && packet.supportingReplacementCandidates.pgr.admittedCount === 0, "packet PGR candidate counts changed");
assert(packet.supportingReplacementCandidates.atomicGate.admittedSupportingSources === 0, "supporting source admitted early");
assert(packet.supportingReplacementCandidates.atomicGate.liveRows === 0 && packet.supportingReplacementCandidates.atomicGate.liveCrosswalkCells === 0, "live evidence promoted early");

for (const claimId of ["PGR-STAGE-SPINE-01", "HI3-STAGE-SPINE-01"]) {
  const claim = getUnique(index.claims, (candidate) => candidate.claimId === claimId, claimId);
  assert(claim.mappingStatus === "section-only" && claim.sourceMappings.length === 0, `${claimId} promoted early`);
  assert(candidateSourceIds.every((id) => !claim.sourceIds.includes(id)), `${claimId} references candidate source early`);
}

const snapshotId = "SNAP-EVID-P1B-PGR-REPLACEMENT-V1-20260716";
const evidenceRefId = "EV-EVID-P1B-PGR-REPLACEMENT-V1-20260716";
const snapshot = getUnique(backlog.snapshotRefs, (candidate) => candidate.snapshotRefId === snapshotId, snapshotId);
const evidenceRef = getUnique(backlog.evidenceRefs, (candidate) => candidate.evidenceRefId === evidenceRefId, evidenceRefId);
assert(snapshot.atomicGateState.admittedSupportingSources === 0 && snapshot.atomicGateState.liveRows === 0 && snapshot.atomicGateState.liveCrosswalkCells === 0, "snapshot gained admission effect");
assert(evidenceRef.snapshotRefId === snapshotId && evidenceRef.canonicalAuditDigest === snapshot.canonicalAuditDigest, "snapshot/evidence binding changed");
const auditBytes = readFileSync(join(workspaceRoot, ...evidenceRef.path.split("/")));
assert(auditBytes.length === evidenceRef.sizeBytes && sha256(auditBytes) === evidenceRef.sha256.toLowerCase(), "backlog package audit bytes changed");

const backlogItem = getUnique(backlog.items, (item) => item.itemId === "EVID-P1B-STAGE-SPINE", "EVID-P1B-STAGE-SPINE");
assert(backlogItem.lifecycleStatus === "partial" && backlogItem.evidenceRefIds.includes(evidenceRefId), "backlog candidate evidence not linked or item promoted");
const candidateAcceptance = getUnique(backlogItem.acceptance, (acceptance) => acceptance.acceptanceId === "ACC-EVID-P1B-PGR-REPLACEMENT-CANDIDATE", "PGR candidate acceptance");
assert(candidateAcceptance.required === false && candidateAcceptance.result === "pass" && candidateAcceptance.proofRefIds.join("|") === evidenceRefId, "candidate acceptance changed");
for (const acceptanceId of requiredOpenAcceptanceIds) {
  const acceptance = getUnique(backlogItem.acceptance, (candidate) => candidate.acceptanceId === acceptanceId, acceptanceId);
  assert(acceptance.required === true && acceptance.result === "open" && acceptance.proofRefIds.length === 0, `${acceptanceId} must remain open without proof`);
}
assert(backlogItem.evidenceTierSummary.liveForeignRows.current === 0 && backlogItem.evidenceTierSummary.liveCrosswalkCells.current === 0, "backlog live counts changed");
assert(backlogItem.evidenceTierSummary.rawCandidateSupportingCitations.current === 0 && backlogItem.evidenceTierSummary.rawCandidateSupportingCitations.pgrCandidateCount === 4 && backlogItem.evidenceTierSummary.rawCandidateSupportingCitations.pgrAdmittedCount === 0, "backlog supporting counts changed");

assert(roadmap.includes("It contains 26 source records and ten claims"), "roadmap source count not updated");
assert(roadmap.includes("PGR replacement update, 2026-07-16 01:05 KST"), "roadmap package cutoff missing");
assert(roadmap.includes("supporting cohort stays `0/9`"), "roadmap atomic-gate boundary missing");
assert(roadmap.includes("three HI3 replacement outputs"), "roadmap next HI3 boundary missing");

console.log(`PASS ${VALIDATOR_ID}`);
console.log("sources=26 candidates=4 admitted=0 packetScope=9 liveRows=0 liveCells=0 requiredOpen=3");
console.log("next=HI3-three-replacements-plus-two-helper-provenance-and-PGR-license-then-atomic-admission");
