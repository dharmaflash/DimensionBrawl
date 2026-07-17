import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, isAbsolute, join, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const VALIDATOR_ID = "P1B-PGR-HI3-SUPPORTING-COHORT-CANDIDATE-V1-REGISTRY-VALIDATOR-01";
const currentModulePath = resolve(fileURLToPath(import.meta.url));
const here = dirname(currentModulePath);
const workspaceRoot = resolve(here, "..", "..");
const evidenceIndexPath = join(here, "SUBCULTURE_DATASET_EVIDENCE_INDEX.json");
const backlogPath = join(here, "SUBCULTURE_GAP_BACKLOG.json");
const roadmapPath = join(here, "SUBCULTURE_DATASET_GAP_ROADMAP.md");
const SELF_RELATIVE_PATH = "_Game/DesignDocs/P1B_PGR_HI3_SUPPORTING_COHORT_CANDIDATE_V1_REGISTRY_VALIDATOR.mjs";
const COHORT_MANIFEST_IDENTITY = Object.freeze({
  path: "_Game/DesignDocs/P1B_PGR_HI3_SUPPORTING_COHORT_CANDIDATE_V1_MANIFEST.json",
  sizeBytes: 12158,
  sha256: "2f6cf9f5b3e319239fe780a2dd605dedb4405f69b48a8183349610ccfd8efc9d",
  canonicalManifestDigest: "e9fc1d979b3fc44b17b161bb72511402c2ccc771853172ecee83a5517c861ac7",
  cohortPackageDigest: "d8f318474ca364cdd3791e60adb90b6897c411c92a6922618f3433dc2f58c5fe",
  generatorPath: "_Game/DesignDocs/P1B_PGR_HI3_SUPPORTING_COHORT_CANDIDATE_V1_MANIFEST_GENERATOR.mjs",
  generatorSizeBytes: 14936,
  generatorSha256: "4135ed8fea055d91bfb3e75c816717cf0f15dc5218a938cadab77693cd2d6829",
});

const pgrCandidateSourceIds = [
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-readfirst-md",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-summary-json",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-label-context-csv",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-reading-links-csv",
];
const pgrPredecessorSourceIds = [
  "pgr-readfirst-md",
  "pgr-readfirst-summary-json",
  "pgr-guidefight-label-csv",
  "pgr-guidefight-links-csv",
];
const hi3CandidateSourceIds = [
  "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-readfirst-md",
  "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-summary-json",
  "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-reading-links-csv",
];
const hi3PredecessorSourceIds = [
  "hi3-readfirst-md",
  "hi3-readfirst-summary-json",
  "hi3-readfirst-csv",
];
const helperSourceIds = ["hi3-stage-summary-csv", "hi3-stage-samples-csv"];
const allCandidateSourceIds = [...pgrCandidateSourceIds, ...hi3CandidateSourceIds, ...helperSourceIds];
const requiredOpenAcceptanceIds = [
  "ACC-EVID-P1B-LIVE-PROVENANCE",
  "ACC-EVID-P1B-EXACT-ROWS",
  "ACC-EVID-P1B-DRIFT-CLASSIFICATION",
];
const candidateAcceptanceBindings = new Map([
  ["ACC-EVID-P1B-PGR-REPLACEMENT-CANDIDATE", "EV-EVID-P1B-PGR-REPLACEMENT-V1-20260716"],
  ["ACC-EVID-P1B-HI3-REPLACEMENT-CANDIDATE", "EV-EVID-P1B-HI3-REPLACEMENT-V1-20260716"],
  ["ACC-EVID-P1B-HI3-HELPER-PROVENANCE-CANDIDATE", "EV-EVID-P1B-HI3-HELPER-PROVENANCE-V1-20260716"],
  ["ACC-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT", "EV-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-20260716"],
]);
const verifierPaths = [
  {
    path: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT_GENERATOR.mjs",
    sentinel: "PASS P1B-PGR-STAGE-SPINE-REPLACEMENT-01 package audit",
  },
  {
    path: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT_GENERATOR.mjs",
    sentinel: "PASS P1B-HI3-STAGE-SPINE-REPLACEMENT-01 package audit",
  },
  {
    path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT_GENERATOR.mjs",
    sentinel: "PASS P1B-HI3-STAGE-HELPER-PROVENANCE-01 package audit",
  },
  {
    path: "_Game/DesignDocs/P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT_GENERATOR.mjs",
    sentinel: "PASS P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-01",
  },
  {
    path: "_Game/DesignDocs/P1B_PGR_HI3_SUPPORTING_COHORT_CANDIDATE_V1_MANIFEST_GENERATOR.mjs",
    sentinel: "PASS P1B-PGR-HI3-SUPPORTING-COHORT-CANDIDATE-V1-MANIFEST-01",
  },
];
const expectedItemEvidenceRefIds = [
  "EV-P1B-LOCAL-STAGE-SPINE-PREFLIGHT",
  "EV-P1B-LOCAL-STAGE-SPINE-STATIC-SUPPLEMENT",
  "EV-P1B-PGR-2020-GUIDEFIGHT-CONTROL",
  "EV-P1B-HI3-2021-STAGEDATA-10101-CONTROL",
  "EV-EVID-P1B-RAW-FIVE-ROW-CANDIDATE-20260715",
  "EV-EVID-P1B-SUPPORTING-CITATION-RECOVERY-AUDIT-20260715",
  "EV-EVID-P1B-SUPPORTING-PROVENANCE-DISPOSITION-AUDIT-20260715",
  "EV-EVID-P1B-PGR-REPLACEMENT-V1-20260716",
  "EV-EVID-P1B-HI3-REPLACEMENT-V1-20260716",
  "EV-EVID-P1B-HI3-HELPER-PROVENANCE-V1-20260716",
  "EV-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-20260716",
];
const expectedP1bFailureProofRefIds = [
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-BUNDLE",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-MANIFESTS",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-CONTRACT-VERIFIERS",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-VALIDATOR",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-FOCUSED",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-CANONICAL-UI",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-FULL-ROUTE",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-AGGREGATE",
  "EV-P1B-RESULT-PROGRESSION-CANDIDATE-CODE-AUDIT",
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

function stableStringify(value) {
  if (Array.isArray(value)) return `[${value.map(stableStringify).join(",")}]`;
  if (value && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${stableStringify(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function stableDigest(value) {
  return sha256(Buffer.from(stableStringify(value), "utf8"));
}

function getUnique(rows, predicate, label) {
  const matches = rows.filter(predicate);
  assert(matches.length === 1, `${label} cardinality must be one, got ${matches.length}`);
  return matches[0];
}

function assertUnique(rows, selector, label) {
  const values = rows.map(selector);
  assert(values.length === new Set(values).size, `${label} contains duplicate identities`);
}

function workspacePath(relativePath) {
  assert(typeof relativePath === "string" && relativePath.length > 0, "artifact path must be non-empty");
  return isAbsolute(relativePath) ? relativePath : join(workspaceRoot, ...relativePath.replaceAll("\\", "/").split("/"));
}

function sourcePath(source) {
  const root = source.rawSnapshotRoot ?? workspaceRoot;
  return join(root, ...source.relativePath.replaceAll("\\", "/").split("/"));
}

function assertFile(path, sizeBytes, expectedSha256, label) {
  const bytes = readFileSync(workspacePath(path));
  assert(bytes.length === sizeBytes, `${label} size mismatch: expected ${sizeBytes}, got ${bytes.length}`);
  assert(sha256(bytes) === expectedSha256.toLowerCase(), `${label} SHA-256 mismatch`);
  return bytes;
}

function assertSourceFile(source, label) {
  const bytes = readFileSync(sourcePath(source));
  assert(bytes.length === source.sizeBytes, `${label} source size mismatch`);
  assert(sha256(bytes) === source.sha256.toLowerCase(), `${label} source SHA-256 mismatch`);
}

function verifyPackageAudit(relativePath, expected) {
  const bytes = assertFile(relativePath, expected.sizeBytes, expected.sha256, expected.label);
  const audit = JSON.parse(bytes.toString("utf8"));
  assert(audit.canonicalAuditDigest === expected.canonicalAuditDigest, `${expected.label} canonical digest mismatch`);
  assert(audit.packageDigest === expected.packageDigest, `${expected.label} package digest mismatch`);
  assert(audit.packageFiles.length === expected.packageFileCount, `${expected.label} package-file count mismatch`);
  for (const file of audit.packageFiles) {
    assertFile(file.path, file.sizeBytes, file.sha256, `${expected.label}:${file.path}`);
  }
  return audit;
}

function runVerifier({ path: relativePath, sentinel }) {
  const result = spawnSync(process.execPath, [workspacePath(relativePath), "--verify"], {
    cwd: workspaceRoot,
    encoding: "utf8",
    maxBuffer: 16 * 1024 * 1024,
    windowsHide: true,
  });
  const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
  assert(result.status === 0, `${relativePath} --verify failed with exit ${result.status}: ${output.slice(-1500)}`);
  const outputLines = output.split(/\r?\n/).map((line) => line.trim()).filter(Boolean);
  assert(outputLines.includes(sentinel), `${relativePath} --verify did not report exact sentinel: ${sentinel}`);
}

const index = JSON.parse(readFileSync(evidenceIndexPath, "utf8"));
const backlog = JSON.parse(readFileSync(backlogPath, "utf8"));
const roadmap = readFileSync(roadmapPath, "utf8");

assert(index.schemaVersion === 2, `evidence-index schema must be 2, got ${index.schemaVersion}`);
assert(backlog.schemaVersion === 1, `backlog schema must be 1, got ${backlog.schemaVersion}`);
assert(index.updatedAt === "2026-07-16T11:42:00+09:00", "evidence-index updatedAt mismatch");
assert(backlog.updatedAt === index.updatedAt, "index/backlog updatedAt mismatch");
assert(index.sources.length === 29, `source count must be 29, got ${index.sources.length}`);
assert(index.claims.length === 10, `claim count must be 10, got ${index.claims.length}`);
assert(backlog.snapshotRefs.length === 28, `snapshot count must be 28, got ${backlog.snapshotRefs.length}`);
assert(backlog.evidenceRefs.length === 126, `evidence count must be 126, got ${backlog.evidenceRefs.length}`);
assert(backlog.items.length === 30, `backlog item count must be 30, got ${backlog.items.length}`);
assertUnique(index.sources, (row) => row.sourceId, "source registry");
assertUnique(index.claims, (row) => row.claimId, "claim registry");
assertUnique(backlog.snapshotRefs, (row) => row.snapshotRefId, "snapshot registry");
assertUnique(backlog.evidenceRefs, (row) => row.evidenceRefId, "evidence registry");
assertUnique(backlog.items, (row) => row.itemId, "backlog items");

const declaredHashStatuses = new Set(index.statusVocabulary.hashStatus);
assert([...declaredHashStatuses].join("|") === "pending-live-recheck|verified|verified-candidate", "hash-status vocabulary mismatch");
for (const source of index.sources) {
  assert(declaredHashStatuses.has(source.hashStatus), `${source.sourceId} uses undeclared hashStatus ${source.hashStatus}`);
}
assert(index.recordSemantics.sourceRootScope["local-retained-mirror-mixed-snapshot-helper-provenance-candidate"], "helper source-root scope vocabulary missing");
assert(index.liveAccess.status === "retained-mirror-raw-pair-reproduced-supporting-nine-verified-zero-admitted-rights-and-atomic-gate-open", "liveAccess status is stale");
assert(index.liveAccess.meaning.includes("two present derived CSV identities now have exact mixed-snapshot replay provenance"), "liveAccess meaning does not acknowledge verified helper provenance");
assert(index.liveAccess.meaning.includes("all nine supporting candidates remain unadmitted"), "liveAccess meaning lost zero-admission boundary");

const packet = getUnique(index.boundedPackets, (row) => row.packetId === "P1B-PGR-HI3-STAGE-SPINE-01", "P1-B packet");
assert(packet.inScopeSourceIds.length === 9 && new Set(packet.inScopeSourceIds).size === 9, "packet must retain nine unique historical in-scope IDs");
assert(stableDigest(packet.inScopeSourceIds) === "91ae5fe77646774811725d6f37cba3566406fc3a8ca52569363cb010961449f4", "historical inScopeSourceIds changed");
assert(stableDigest(packet.supportingReplacementCandidates.pgr) === "cabf95e6c15da52e396e7a36c5f83ced8f74371070547ba28527dad052e4b887", "accepted PGR candidate subtree changed");
assert(stableDigest(index.claims) === "0ae6dcea653b343793cff7ec1e881d8a50e3a6d34108cdf00fe66ace1b4ceeda", "claims changed during candidate merge");
assert(stableDigest(packet.crosswalkRows) === "4f53cda18c2baa0c0354bb5f9a3ecbe5ed12ab4d8e11ba873c2f11161202b945", "crosswalk rows changed");
assert(packet.generatedReportPath === null && packet.generatedReportSha256 === null, "active report promoted early");
assert(packet.crosswalkRows.length === 0, "crosswalk populated early");
assert(packet.liveRawSourceAdmission.pgr.sourceId === null && packet.liveRawSourceAdmission.hi3.sourceId === null, "raw source admitted early");

const support = packet.supportingReplacementCandidates;
assert(support.status === "nine-supporting-candidates-verified-none-admitted-license-review-and-atomic-admission-open", "supporting-candidate status mismatch");
assert(support.pgr.candidateSourceIds.join("|") === pgrCandidateSourceIds.join("|"), "PGR candidate order changed");
assert(support.hi3.replacementCandidateSourceIds.join("|") === hi3CandidateSourceIds.join("|"), "HI3 candidate order changed");
assert(support.hi3.helperCandidateSourceIds.join("|") === helperSourceIds.join("|"), "helper candidate order changed");
assert(support.atomicGate.candidateSupportingSources === 9, "candidate supporting count must be nine");
assert(support.atomicGate.admittedSupportingSources === 0, "supporting source admitted early");
assert(support.atomicGate.liveRows === 0 && support.atomicGate.liveCrosswalkCells === 0, "live evidence promoted early");
assert(new Set(allCandidateSourceIds).size === 9, "candidate identity ledger is not unique");

for (let ordinal = 0; ordinal < pgrCandidateSourceIds.length; ordinal += 1) {
  const candidateId = pgrCandidateSourceIds[ordinal];
  const predecessorId = pgrPredecessorSourceIds[ordinal];
  const candidate = getUnique(index.sources, (row) => row.sourceId === candidateId, candidateId);
  const predecessor = getUnique(index.sources, (row) => row.sourceId === predecessorId, predecessorId);
  assert(candidate.predecessorSourceId === predecessorId && predecessor.replacementCandidateSourceId === candidateId, `${candidateId} predecessor binding mismatch`);
  assert(candidate.admissionState === "candidate-not-admitted" && candidate.admissionEffect === "none", `${candidateId} gained admission effect`);
  assert(candidate.replacementRelation === "new-versioned-semantic-successor-not-historical-byte-reconstruction", `${candidateId} replacement semantics changed`);
  assert(!packet.inScopeSourceIds.includes(candidateId), `${candidateId} entered packet scope early`);
  assertSourceFile(candidate, candidateId);
}

for (let ordinal = 0; ordinal < hi3CandidateSourceIds.length; ordinal += 1) {
  const candidateId = hi3CandidateSourceIds[ordinal];
  const predecessorId = hi3PredecessorSourceIds[ordinal];
  const candidate = getUnique(index.sources, (row) => row.sourceId === candidateId, candidateId);
  const predecessor = getUnique(index.sources, (row) => row.sourceId === predecessorId, predecessorId);
  assert(candidate.predecessorSourceId === predecessorId && predecessor.replacementCandidateSourceId === candidateId, `${candidateId} predecessor binding mismatch`);
  assert(candidate.admissionState === "candidate-not-admitted" && candidate.admissionEffect === "none", `${candidateId} gained admission effect`);
  assert(candidate.replacementRelation === "new-versioned-semantic-successor-not-historical-byte-reconstruction", `${candidateId} replacement semantics changed`);
  assert(candidate.inputSourceIds.join("|") === "hi3-stagedata-main-nairieberry-01d7afb-global-json", `${candidateId} input authority changed`);
  assert(!packet.inScopeSourceIds.includes(candidateId), `${candidateId} entered packet scope early`);
  assertSourceFile(candidate, candidateId);
  assertFile(candidate.sourceRecordPath, candidate.sourceRecordSizeBytes, candidate.sourceRecordSha256, `${candidateId}:source record`);
  assertFile(candidate.producerManifestPath, candidate.producerManifestSizeBytes, candidate.producerManifestSha256, `${candidateId}:producer manifest`);
}

const hi3Summary = getUnique(index.sources, (row) => row.sourceId === hi3CandidateSourceIds[1], "HI3 replacement summary");
assert(hi3Summary.normalizedProjectionSizeBytes === 19496, "HI3 normalized projection size changed");
assert(hi3Summary.normalizedProjectionSha256 === "D20113431CA54B1DA5BC1F6C477B32DE0FA9EB205F67D3E33CDAAAFE4F6F7101", "HI3 normalized projection digest changed");
assert(hi3Summary.fieldShapeProjectionSha256 === "198337432E357936402400551AB2F51F52084A57C4D267DAA34DDEB833C6BB91", "HI3 field-shape digest changed");
assert(hi3Summary.topLevelKeySetSha256 === "BF6BBA4B0E4F2900A610C63D6081DC33265E3C26A1836557636B55C0DE196EEC", "HI3 key-set digest changed");

const expectedHelperFiles = new Map([
  ["hi3-stage-summary-csv", { sizeBytes: 295098, sha256: "d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7", rows: 1509 }],
  ["hi3-stage-samples-csv", { sizeBytes: 4459588, sha256: "5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92", rows: 14855 }],
]);
for (const helperId of helperSourceIds) {
  const helper = getUnique(index.sources, (row) => row.sourceId === helperId, helperId);
  const expected = expectedHelperFiles.get(helperId);
  assert(packet.inScopeSourceIds.includes(helperId), `${helperId} historical in-scope identity moved`);
  assert(helper.formalAdmissionState === "open" && helper.admissionState === "formal-provenance-candidate-not-admitted", `${helperId} formal admission changed`);
  assert(helper.hashStatus === "verified" && helper.admissionEffect === "none", `${helperId} hash/admission state mismatch`);
  assert(helper.sizeBytes === expected.sizeBytes && helper.sha256.toLowerCase() === expected.sha256 && helper.schemaDataRows === expected.rows, `${helperId} output identity changed`);
  assert(helper.observedInputInventorySha256 === "3B00DE9A3CC41D63C7576A1958C0D01FE098E412A2C98E43ABA0B1E6D544E662", `${helperId} inventory digest changed`);
  assert(helper.upstreamSnapshotRevisions.map((row) => row.selectedInputCount).join("|") === "371|1138|0", `${helperId} selected-input distribution changed`);
  assertSourceFile(helper, helperId);
  assertFile(helper.sourceRecordPath, helper.sourceRecordSizeBytes, helper.sourceRecordSha256, `${helperId}:source record`);
  assertFile(helper.producerManifestPath, helper.producerManifestSizeBytes, helper.producerManifestSha256, `${helperId}:producer manifest`);
}
assert(support.hi3.helperVerifiedInputs.files === 1509 && support.hi3.helperVerifiedInputs.bytes === 456457979, "helper input totals changed");
assert(support.hi3.helperVerifiedInputs.canonicalInventoryDigest === "3B00DE9A3CC41D63C7576A1958C0D01FE098E412A2C98E43ABA0B1E6D544E662", "helper inventory digest changed");

for (const claimId of ["PGR-STAGE-SPINE-01", "HI3-STAGE-SPINE-01"]) {
  const claim = getUnique(index.claims, (row) => row.claimId === claimId, claimId);
  assert(claim.mappingStatus === "section-only" && claim.sourceMappings.length === 0, `${claimId} promoted early`);
  assert([...pgrCandidateSourceIds, ...hi3CandidateSourceIds].every((id) => !claim.sourceIds.includes(id)), `${claimId} references replacement candidate early`);
}

const pgrAudit = verifyPackageAudit("_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json", {
  label: "PGR package audit",
  sizeBytes: 3822,
  sha256: "54b4cf14c6d72cf14415b301fa8b2bb79d801e329c28773d65c01b7b6f08ebd2",
  canonicalAuditDigest: "59d21e7da9c6b3e7d70201294830133a925ca56d4dbb067df5a846d8a99253f8",
  packageDigest: "09ca47fa01a1c457f4270e3cd696d0652849fad32f4adf19e9d705b06d74e800",
  packageFileCount: 8,
});
const hi3Audit = verifyPackageAudit("_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json", {
  label: "HI3 replacement package audit",
  sizeBytes: 3670,
  sha256: "cdbb662179c10b035ab889b583341134ab5daef442dcd760e65b3807d3fd0d06",
  canonicalAuditDigest: "7f9558bcbcfb41c65cc7abbbd9471704dfba5143c6ff1f6cb20222eb3da7a867",
  packageDigest: "d8819674053194e69fb5b39393f58e6ead0d82d6620cf9673a012abf1c60dc44",
  packageFileCount: 7,
});
const helperAudit = verifyPackageAudit("_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT.json", {
  label: "HI3 helper package audit",
  sizeBytes: 4082,
  sha256: "383ba46684bcb51734c24073d65a95af31769633cf7d638cc2790a16198a706d",
  canonicalAuditDigest: "b16247c0877cbfd0609241dbc85a9672955aee302190da64ff6e6f075757bda6",
  packageDigest: "9de3bafab2b6695263f9c7e1e4d40ffa946a0656a3337de7b43c2cd19d3fea9a",
  packageFileCount: 7,
});
assert(pgrAudit.verifiedCounts.outputs === 4 && pgrAudit.verifiedCounts.sourceValueCopied === 0, "PGR package findings changed");
assert(hi3Audit.verifiedCounts.outputs === 3 && hi3Audit.verifiedCounts.exactRows === 1 && hi3Audit.verifiedCounts.fieldShapeRows === 67 && hi3Audit.verifiedCounts.linkRows === 14 && hi3Audit.verifiedCounts.sourceValueCopied === 0, "HI3 package findings changed");
assert(helperAudit.verifiedCounts.selectedInputs === 1509 && helperAudit.verifiedCounts.selectedInputBytes === 456457979 && helperAudit.verifiedCounts.targetDataRows === 16364, "helper package findings changed");

const licenseBytes = assertFile("_Game/DesignDocs/P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT.json", 9166, "bc418a4d6bc6809b89832dec73efa6e3dca18bb3d0c1e21157adecd65f9145e6", "license-signal audit");
const licenseAudit = JSON.parse(licenseBytes.toString("utf8"));
assert(licenseAudit.canonicalAuditDigest === "ec3e3a1e6500ddb96a8b6fb907d8e49e22627db8f1633b4cd45febebeec008e2", "license canonical digest changed");
assert(licenseAudit.verifiedCounts.repositories === 4 && licenseAudit.verifiedCounts.archivesWithNoLicenseLikeEntry === 3 && licenseAudit.verifiedCounts.archivesWithExplicitLicenseLikeEntry === 1, "license observation counts changed");
assert(licenseAudit.observations.slice(0, 3).every((row) => row.licenseLikeEntries.length === 0 && row.rootReadme.licenseSignalCount === 0), "contributing repository signal boundary changed");
const mskTmi = getUnique(licenseAudit.observations, (row) => row.repositoryId === "msktmi-hi3-data-1debfbd", "MskTmi license observation");
assert(mskTmi.licenseLikeEntries.length === 1 && mskTmi.licenseLikeEntries[0].sha256 === "6da1054eef20b8949622f2acc5a89c3243ff3b3d7aa8c2bb8fa5c04d15113c00", "MskTmi license signal changed");
assert(mskTmi.selectedContribution.helperInputFiles === 0 && mskTmi.selectedContribution.helperInputBytes === 0, "MskTmi entered selected helper inputs");

const cohortBytes = assertFile(COHORT_MANIFEST_IDENTITY.path, COHORT_MANIFEST_IDENTITY.sizeBytes, COHORT_MANIFEST_IDENTITY.sha256, "cohort manifest");
const cohort = JSON.parse(cohortBytes.toString("utf8"));
assert(cohort.canonicalManifestDigest === COHORT_MANIFEST_IDENTITY.canonicalManifestDigest, "cohort canonical digest changed");
assert(cohort.cohortPackageDigest === COHORT_MANIFEST_IDENTITY.cohortPackageDigest, "cohort package digest changed");
assert(cohort.candidateSources.map((row) => row.sourceId).join("|") === allCandidateSourceIds.join("|"), "cohort source order changed");
assert(cohort.verifiedCounts.supportingCandidates === 9 && cohort.verifiedCounts.admittedSupportingSources === 0 && cohort.verifiedCounts.liveForeignRows === 0 && cohort.verifiedCounts.liveCrosswalkCells === 0, "cohort counts changed");
for (const file of cohort.cohortFiles) assertFile(file.path, file.sizeBytes, file.sha256, `cohort:${file.path}`);

const expectedSnapshotChain = [
  ["SNAP-EVID-P1B-PGR-REPLACEMENT-V1-20260716", null],
  ["SNAP-EVID-P1B-HI3-REPLACEMENT-V1-20260716", "SNAP-EVID-P1B-PGR-REPLACEMENT-V1-20260716"],
  ["SNAP-EVID-P1B-HI3-HELPER-PROVENANCE-V1-20260716", "SNAP-EVID-P1B-HI3-REPLACEMENT-V1-20260716"],
  ["SNAP-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-20260716", "SNAP-EVID-P1B-HI3-HELPER-PROVENANCE-V1-20260716"],
];
for (const [snapshotId, expectedBase] of expectedSnapshotChain) {
  const snapshot = getUnique(backlog.snapshotRefs, (row) => row.snapshotRefId === snapshotId, snapshotId);
  if (expectedBase !== null) assert(snapshot.baseSnapshotRefId === expectedBase, `${snapshotId} base snapshot changed`);
}

const evidenceArtifactExpectations = new Map([
  ["EV-EVID-P1B-HI3-REPLACEMENT-V1-20260716", ["_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json", 3670, "cdbb662179c10b035ab889b583341134ab5daef442dcd760e65b3807d3fd0d06"]],
  ["EV-EVID-P1B-HI3-HELPER-PROVENANCE-V1-20260716", ["_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT.json", 4082, "383ba46684bcb51734c24073d65a95af31769633cf7d638cc2790a16198a706d"]],
  ["EV-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-20260716", ["_Game/DesignDocs/P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT.json", 9166, "bc418a4d6bc6809b89832dec73efa6e3dca18bb3d0c1e21157adecd65f9145e6"]],
]);
for (const [evidenceId, [path, sizeBytes, digest]] of evidenceArtifactExpectations) {
  const evidence = getUnique(backlog.evidenceRefs, (row) => row.evidenceRefId === evidenceId, evidenceId);
  const snapshot = getUnique(backlog.snapshotRefs, (row) => row.snapshotRefId === evidence.snapshotRefId, `${evidenceId}:snapshot`);
  assert(evidence.path === path && evidence.sizeBytes === sizeBytes && evidence.sha256.toLowerCase() === digest, `${evidenceId} artifact binding changed`);
  assert(evidence.canonicalAuditDigest === snapshot.canonicalAuditDigest, `${evidenceId} canonical snapshot binding changed`);
  assertFile(evidence.path, evidence.sizeBytes, evidence.sha256, evidenceId);
}

const item = getUnique(backlog.items, (row) => row.itemId === "EVID-P1B-STAGE-SPINE", "EVID-P1B-STAGE-SPINE");
assert(item.lifecycleStatus === "partial", "evidence backlog item promoted early");
assert(item.evidenceRefIds.length === 11 && item.acceptance.length === 11, "evidence backlog item count mismatch");
assert(item.evidenceRefIds.join("|") === expectedItemEvidenceRefIds.join("|"), "evidence backlog item ID set/order changed");
for (const evidenceId of item.evidenceRefIds) {
  getUnique(backlog.evidenceRefs, (row) => row.evidenceRefId === evidenceId, `item evidence ${evidenceId}`);
}
assertUnique(item.acceptance, (row) => row.acceptanceId, "EVID-P1B-STAGE-SPINE acceptances");
for (const [acceptanceId, evidenceId] of candidateAcceptanceBindings) {
  const acceptance = getUnique(item.acceptance, (row) => row.acceptanceId === acceptanceId, acceptanceId);
  assert(acceptance.required === false && acceptance.result === "pass" && acceptance.proofRefIds.join("|") === evidenceId, `${acceptanceId} candidate proof binding changed`);
}
for (const acceptanceId of requiredOpenAcceptanceIds) {
  const acceptance = getUnique(item.acceptance, (row) => row.acceptanceId === acceptanceId, acceptanceId);
  assert(acceptance.required === true && acceptance.result === "open" && acceptance.proofRefIds.length === 0, `${acceptanceId} must remain OPEN without proof`);
}
const tier = item.evidenceTierSummary.rawCandidateSupportingCitations;
assert(tier.current === 0 && tier.required === 9 && tier.verifiedCandidateCount === 9 && tier.admittedCount === 0, "backlog candidate/admission counts changed");
assert(tier.verifiedPgrReplacementCandidateSourceIds.join("|") === pgrCandidateSourceIds.join("|"), "backlog PGR candidate order changed");
assert(tier.verifiedHi3ReplacementCandidateSourceIds.join("|") === hi3CandidateSourceIds.join("|"), "backlog HI3 candidate order changed");
assert(tier.verifiedHelperProvenanceCandidateSourceIds.join("|") === helperSourceIds.join("|"), "backlog helper candidate order changed");
assert(item.evidenceTierSummary.liveForeignRows.current === 0 && item.evidenceTierSummary.liveCrosswalkCells.current === 0, "backlog live evidence promoted early");

const selfRecord = support.candidateCohortManifest;
assert(selfRecord.manifestPath === COHORT_MANIFEST_IDENTITY.path && selfRecord.manifestSizeBytes === COHORT_MANIFEST_IDENTITY.sizeBytes && selfRecord.manifestSha256.toLowerCase() === COHORT_MANIFEST_IDENTITY.sha256, "cohort manifest self-record identity mismatch");
assert(selfRecord.canonicalManifestDigest.toLowerCase() === COHORT_MANIFEST_IDENTITY.canonicalManifestDigest && selfRecord.cohortPackageDigest.toLowerCase() === COHORT_MANIFEST_IDENTITY.cohortPackageDigest, "cohort manifest self-record digest mismatch");
assert(selfRecord.generatorPath === COHORT_MANIFEST_IDENTITY.generatorPath && selfRecord.generatorSizeBytes === COHORT_MANIFEST_IDENTITY.generatorSizeBytes && selfRecord.generatorSha256.toLowerCase() === COHORT_MANIFEST_IDENTITY.generatorSha256, "cohort generator self-record identity mismatch");
assert(selfRecord.generatorPath === verifierPaths.at(-1).path, "cohort generator is not the executed verifier");
assertFile(selfRecord.generatorPath, selfRecord.generatorSizeBytes, selfRecord.generatorSha256, "cohort manifest generator");
assert(selfRecord.registryValidatorPath === SELF_RELATIVE_PATH, "registry-validator relative path changed");
assert(resolve(workspacePath(selfRecord.registryValidatorPath)).toLowerCase() === currentModulePath.toLowerCase(), "registry-validator record does not identify the executing module");
assertFile(selfRecord.registryValidatorPath, selfRecord.registryValidatorSizeBytes, selfRecord.registryValidatorSha256, "cumulative registry validator");
const licenseSnapshot = getUnique(backlog.snapshotRefs, (row) => row.snapshotRefId === "SNAP-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-20260716", "license snapshot");
const licenseEvidence = getUnique(backlog.evidenceRefs, (row) => row.evidenceRefId === "EV-EVID-P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-20260716", "license evidence");
for (const record of [licenseSnapshot.artifactManifest, licenseEvidence.artifactManifest]) {
  assert(record.path === COHORT_MANIFEST_IDENTITY.path && record.sizeBytes === COHORT_MANIFEST_IDENTITY.sizeBytes && record.sha256.toLowerCase() === COHORT_MANIFEST_IDENTITY.sha256, "cohort-manifest evidence file binding mismatch");
  assert(record.canonicalManifestDigest.toLowerCase() === COHORT_MANIFEST_IDENTITY.canonicalManifestDigest && record.cohortPackageDigest.toLowerCase() === COHORT_MANIFEST_IDENTITY.cohortPackageDigest, "cohort-manifest evidence digest binding mismatch");
  assert(record.generatorPath === selfRecord.generatorPath && record.generatorSizeBytes === selfRecord.generatorSizeBytes && record.generatorSha256 === selfRecord.generatorSha256, "cohort-generator evidence binding mismatch");
  assert(record.registryValidatorPath === selfRecord.registryValidatorPath && record.registryValidatorSizeBytes === selfRecord.registryValidatorSizeBytes && record.registryValidatorSha256 === selfRecord.registryValidatorSha256, "registry-validator evidence binding mismatch");
}

const historicalPgrValidator = readFileSync(join(here, "P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_REGISTRY_VALIDATOR.mjs"));
assert(historicalPgrValidator.length === 9293 && sha256(historicalPgrValidator) === "ab605d7982c33bc4e7ffd26a4165e8e90634e969cd03c3819877fa0faf2ee3be", "historical 26-source PGR validator changed");

const p1bProductGate = index.localProductGateStatus.p1b;
assert(p1bProductGate.status === "verified-partial-predecessor-subgates-accepted-result-progression-rev3b-candidate-source-contract-failed-full-exit-open", "P1-B product-gate status changed");
assert(p1bProductGate.resultProgressionLatestFailedCandidateRef === "SUBCULTURE_GAP_BACKLOG.json::SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-IMPLEMENTATION-CANDIDATE-06", "P1-B latest failed candidate ref changed");
const failedProductAudit = p1bProductGate.resultProgressionFailedImplementationAudit;
assert(failedProductAudit.snapshotRef === p1bProductGate.resultProgressionLatestFailedCandidateRef, "P1-B failed-audit snapshot binding changed");
assert(failedProductAudit.blockingFindings.length === 3, "P1-B product-gate blocker count must remain three");
assert(stableDigest(failedProductAudit.blockingFindings) === "2de8a0290300bb2deda6924b758ba083619f65d6d7832fd64808217b014ba716", "P1-B product-gate blocker ledger changed");
assert(failedProductAudit.acceptanceResult === "ACC-P1B-RESULT-PROGRESSION-JOINS = fail for Candidate-06. The Rev3B joint freeze and accepted predecessor cutoffs remain unchanged.", "P1-B failed-audit result changed");
assert(failedProductAudit.negativeBoundary.includes("no Station Add") && p1bProductGate.openProductGates.includes("Station Add authoring readiness") && p1bProductGate.openProductGates.includes("full P1-B exit audit"), "P1-B held downstream gates changed");

const p1bItem = getUnique(backlog.items, (row) => row.itemId === "P1B-STAGE-SPINE", "P1B-STAGE-SPINE");
const resultProgressionAcceptance = getUnique(p1bItem.acceptance, (row) => row.acceptanceId === "ACC-P1B-RESULT-PROGRESSION-JOINS", "ACC-P1B-RESULT-PROGRESSION-JOINS");
assert(resultProgressionAcceptance.required === true && resultProgressionAcceptance.result === "fail", "P1-B result/progression acceptance changed");
assert(resultProgressionAcceptance.latestFailedCandidateSnapshotRefId === "SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-IMPLEMENTATION-CANDIDATE-06", "P1-B acceptance failed-candidate binding changed");
assert(Array.isArray(resultProgressionAcceptance.proofRefIds) && Array.isArray(resultProgressionAcceptance.blockingFindings), "P1-B failed acceptance evidence/blocker ledgers must be arrays");
assert(resultProgressionAcceptance.proofRefIds.length === 9 && resultProgressionAcceptance.blockingFindings.length === 3, "P1-B failed acceptance evidence/blocker count changed");
assert(resultProgressionAcceptance.proofRefIds.join("|") === expectedP1bFailureProofRefIds.join("|"), "P1-B failed acceptance proof ledger changed");
assert(stableDigest(resultProgressionAcceptance.proofRefIds) === "16ed8208a68352ec5094dee41bfcb8250c550c590e9080224d3c404089df2f5e", "P1-B failed acceptance proof digest changed");
assert(stableDigest(resultProgressionAcceptance.blockingFindings) === "f684a94a0f729a53434c67d13f3fa229dd69deaf0ce89eac4abee3248386ecc4", "P1-B acceptance blocker ledger changed");
assert(resultProgressionAcceptance.contractState === "joint-frozen-first-implementation-candidate-failed-remediation-open", "P1-B failed acceptance contract state changed");
for (const evidenceId of resultProgressionAcceptance.proofRefIds) {
  getUnique(backlog.evidenceRefs, (row) => row.evidenceRefId === evidenceId, `P1-B failed proof ${evidenceId}`);
}
const stationAddAcceptance = getUnique(p1bItem.acceptance, (row) => row.acceptanceId === "ACC-P1B-STATION-ADD-AUTHORING", "ACC-P1B-STATION-ADD-AUTHORING");
const fullExitAcceptance = getUnique(p1bItem.acceptance, (row) => row.acceptanceId === "ACC-P1B-FULL-EXIT-AUDIT", "ACC-P1B-FULL-EXIT-AUDIT");
assert(stationAddAcceptance.required === true && stationAddAcceptance.result === "pending" && Array.isArray(stationAddAcceptance.proofRefIds) && stationAddAcceptance.proofRefIds.length === 0, "P1-B Station Add hold changed");
assert(fullExitAcceptance.required === true && fullExitAcceptance.result === "pending" && Array.isArray(fullExitAcceptance.proofRefIds) && fullExitAcceptance.proofRefIds.length === 0, "P1-B full-exit hold changed");
const failedCandidateSnapshot = getUnique(backlog.snapshotRefs, (row) => row.snapshotRefId === "SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-IMPLEMENTATION-CANDIDATE-06", "P1-B Candidate-06 snapshot");
const failedCandidateBaseSnapshot = getUnique(backlog.snapshotRefs, (row) => row.snapshotRefId === "SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-JOINT-FREEZE-05", "P1-B Candidate-06 base snapshot");
assert(failedCandidateSnapshot.snapshotState === "artifact-and-source-hash-verified-source-contract-failed-remediation-required", "P1-B Candidate-06 snapshot state changed");
assert(failedCandidateSnapshot.baseSnapshotRefId === failedCandidateBaseSnapshot.snapshotRefId, "P1-B Candidate-06 base snapshot changed");
assert(failedCandidateSnapshot.contractId === "P1B-RESULT-PROGRESSION-JOINS-01" && failedCandidateSnapshot.acceptanceId === "ACC-P1B-RESULT-PROGRESSION-JOINS", "P1-B Candidate-06 contract identity changed");
assert(failedCandidateSnapshot.verdict === "FAIL-SOURCE-CONTRACT-FAILED-CANDIDATE" && failedCandidateSnapshot.acceptanceResult === "fail", "P1-B Candidate-06 verdict changed");
assert(Array.isArray(failedCandidateSnapshot.blockingFindings) && failedCandidateSnapshot.blockingFindings.length === 3 && failedCandidateSnapshot.bundle.sha256 === "35B1B1A5523BC457AD1936190D1D41143DD1BC8A3489624CDB600631C3A6DAA1", "P1-B Candidate-06 blocker/bundle identity changed");
assert(stableDigest(failedCandidateSnapshot.blockingFindings) === "4366df574f2b88841ea48cc5d07107fcbbb39d77ddd0fef78bc0942819097df7", "P1-B Candidate-06 blocker ledger changed");
assert(failedCandidateSnapshot.acceptanceEffect === "ACC-P1B-RESULT-PROGRESSION-JOINS is FAIL for this candidate. The Rev3B joint freeze and every accepted predecessor cutoff remain immutable; remediation requires a new candidate snapshot and cannot retroactively amend this one.", "P1-B Candidate-06 acceptance effect changed");
assert(failedCandidateSnapshot.negativeBoundary.includes("No Station Add") && failedCandidateSnapshot.negativeBoundary.includes("P1-B full exit"), "P1-B Candidate-06 negative boundary changed");

assert(roadmap.includes("It contains 29 source records and ten claims"), "roadmap source count is stale");
assert(roadmap.includes("9 verified / 0 admitted / 0 live rows / 0 live cells"), "roadmap cohort state missing");
assert(roadmap.includes("policy/rights disposition") && roadmap.includes("atomic eleven-source"), "roadmap remaining gate missing");
assert(roadmap.includes("P1-B result/progression Rev3B implementation candidate audit"), "P1-B Rev3B audit boundary missing");
assert(roadmap.includes("ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL"), "P1-B Rev3B FAIL boundary changed");

for (const verifierPath of verifierPaths) runVerifier(verifierPath);

console.log(`PASS ${VALIDATOR_ID}`);
console.log("sources=29 claims=10 snapshots=28 evidence=126 items=30");
console.log("candidates=9 pgr=4 hi3Replacement=3 hi3Helpers=2 admitted=0 liveRows=0 liveCells=0");
console.log("requiredLiveAcceptancesOpen=3 policyRightsDisposition=open atomicElevenSourceAdmission=open");
