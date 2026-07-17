import { createHash } from "node:crypto";
import { existsSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-PGR-HI3-SUPPORTING-COHORT-CANDIDATE-V1-MANIFEST-GENERATOR-01";
const MANIFEST_ID = "P1B-PGR-HI3-SUPPORTING-COHORT-CANDIDATE-V1-MANIFEST-01";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "../..");
const outputPath = resolve(here, "P1B_PGR_HI3_SUPPORTING_COHORT_CANDIDATE_V1_MANIFEST.json");

const packageSpecs = [
  {
    ordinal: 0,
    family: "pgr-replacement",
    auditPath: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json",
    auditSizeBytes: 3822,
    auditSha256: "54b4cf14c6d72cf14415b301fa8b2bb79d801e329c28773d65c01b7b6f08ebd2",
    auditId: "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-PACKAGE-AUDIT-01",
    canonicalAuditDigest: "59d21e7da9c6b3e7d70201294830133a925ca56d4dbb067df5a846d8a99253f8",
    packageDigest: "09ca47fa01a1c457f4270e3cd696d0652849fad32f4adf19e9d705b06d74e800",
    generatorPath: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT_GENERATOR.mjs",
    generatorSizeBytes: 11298,
    generatorSha256: "c02d100ebdab0f65a44219a6932a1c76e47ec2b687d7f5426b7bd9b4232b8c91",
    sourceRecordPath: "_Game/DesignDocs/P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json",
    expectedCandidateCount: 4,
  },
  {
    ordinal: 1,
    family: "hi3-replacement",
    auditPath: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json",
    auditSizeBytes: 3670,
    auditSha256: "cdbb662179c10b035ab889b583341134ab5daef442dcd760e65b3807d3fd0d06",
    auditId: "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-PACKAGE-AUDIT-01",
    canonicalAuditDigest: "7f9558bcbcfb41c65cc7abbbd9471704dfba5143c6ff1f6cb20222eb3da7a867",
    packageDigest: "d8819674053194e69fb5b39393f58e6ead0d82d6620cf9673a012abf1c60dc44",
    generatorPath: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT_GENERATOR.mjs",
    generatorSizeBytes: 11335,
    generatorSha256: "ba1286afc284e37cbef40a889ca93624d1e6757b7f617922a9dfd973d1a4e7ef",
    sourceRecordPath: "_Game/DesignDocs/P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json",
    expectedCandidateCount: 3,
  },
  {
    ordinal: 2,
    family: "hi3-helper-provenance",
    auditPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT.json",
    auditSizeBytes: 4082,
    auditSha256: "383ba46684bcb51734c24073d65a95af31769633cf7d638cc2790a16198a706d",
    auditId: "P1B-HI3-STAGE-HELPER-PROVENANCE-V1-PACKAGE-AUDIT-01",
    canonicalAuditDigest: "b16247c0877cbfd0609241dbc85a9672955aee302190da64ff6e6f075757bda6",
    packageDigest: "9de3bafab2b6695263f9c7e1e4d40ffa946a0656a3337de7b43c2cd19d3fea9a",
    generatorPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT_GENERATOR.mjs",
    generatorSizeBytes: 17709,
    generatorSha256: "b3c1f90421fc60770388ee8e2eec8f0525d38c67da2e7637eae3ebb077e2377f",
    sourceRecordPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_SOURCE_RECORD.json",
    expectedCandidateCount: 2,
  },
];

const licenseSpec = {
  auditPath: "_Game/DesignDocs/P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT.json",
  auditSizeBytes: 9166,
  auditSha256: "bc418a4d6bc6809b89832dec73efa6e3dca18bb3d0c1e21157adecd65f9145e6",
  auditId: "P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-01",
  canonicalAuditDigest: "ec3e3a1e6500ddb96a8b6fb907d8e49e22627db8f1633b4cd45febebeec008e2",
  generatorPath: "_Game/DesignDocs/P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT_GENERATOR.mjs",
  generatorSizeBytes: 18085,
  generatorSha256: "3fb62d98d68f1ee99dc8ee940e71c624c4810d8aba9b0ff73d03ae0988dc0bb7",
};

const expectedCandidateSourceIds = [
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-readfirst-md",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-summary-json",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-label-context-csv",
  "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-reading-links-csv",
  "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-readfirst-md",
  "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-summary-json",
  "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-reading-links-csv",
  "hi3-stage-summary-csv",
  "hi3-stage-samples-csv",
];

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

function workspacePath(relativePath) {
  return resolve(workspaceRoot, ...relativePath.split("/"));
}

function readPinned(relativePath, expectedSizeBytes, expectedSha256) {
  const path = workspacePath(relativePath);
  const stat = statSync(path);
  assert(stat.size === expectedSizeBytes, `${relativePath} size changed: ${stat.size}`);
  const bytes = readFileSync(path);
  const actualSha256 = sha256(bytes);
  assert(actualSha256 === expectedSha256, `${relativePath} SHA-256 changed: ${actualSha256}`);
  return bytes;
}

function verifyCanonicalDocument(document, digestKey, expectedDigest, label) {
  const { [digestKey]: storedDigest, ...withoutDigest } = document;
  assert(storedDigest === expectedDigest, `${label} stored ${digestKey} changed`);
  assert(canonicalDigest(withoutDigest) === expectedDigest, `${label} ${digestKey} reconstruction failed`);
}

function verifyPackageAudit(spec) {
  readPinned(spec.generatorPath, spec.generatorSizeBytes, spec.generatorSha256);
  const auditBytes = readPinned(spec.auditPath, spec.auditSizeBytes, spec.auditSha256);
  const audit = JSON.parse(auditBytes.toString("utf8"));
  assert(audit.auditId === spec.auditId, `${spec.family} audit ID changed`);
  assert(audit.packageDigest === spec.packageDigest, `${spec.family} package digest changed`);
  verifyCanonicalDocument(audit, "canonicalAuditDigest", spec.canonicalAuditDigest, `${spec.family} audit`);

  const packageRows = [];
  for (const file of audit.packageFiles) {
    readPinned(file.path, file.sizeBytes, file.sha256);
    packageRows.push(`${file.role}|${file.path}|${file.sizeBytes}|${file.sha256}`);
  }
  const reconstructedPackageDigest = sha256(Buffer.from(`${packageRows.join("\n")}\n`, "utf8"));
  assert(reconstructedPackageDigest === spec.packageDigest, `${spec.family} package digest reconstruction failed`);

  const sourceRecord = JSON.parse(readFileSync(workspacePath(spec.sourceRecordPath), "utf8"));
  const candidates = spec.family === "hi3-helper-provenance"
    ? sourceRecord.targetOutputs
    : sourceRecord.outputs;
  assert(candidates.length === spec.expectedCandidateCount, `${spec.family} candidate count changed`);
  return {
    ordinal: spec.ordinal,
    family: spec.family,
    auditId: spec.auditId,
    auditPath: spec.auditPath,
    auditSizeBytes: spec.auditSizeBytes,
    auditSha256: spec.auditSha256,
    auditGeneratorPath: spec.generatorPath,
    auditGeneratorSizeBytes: spec.generatorSizeBytes,
    auditGeneratorSha256: spec.generatorSha256,
    canonicalAuditDigest: spec.canonicalAuditDigest,
    packageDigest: spec.packageDigest,
    packageFileCount: audit.packageFiles.length,
    candidateSourceIds: candidates.map((candidate) => candidate.sourceId),
  };
}

const packageAudits = packageSpecs.map(verifyPackageAudit);
const licenseGeneratorBytes = readPinned(licenseSpec.generatorPath, licenseSpec.generatorSizeBytes, licenseSpec.generatorSha256);
const licenseAuditBytes = readPinned(licenseSpec.auditPath, licenseSpec.auditSizeBytes, licenseSpec.auditSha256);
const licenseAudit = JSON.parse(licenseAuditBytes.toString("utf8"));
assert(licenseAudit.auditId === licenseSpec.auditId, "license audit ID changed");
verifyCanonicalDocument(licenseAudit, "canonicalAuditDigest", licenseSpec.canonicalAuditDigest, "license audit");
assert(licenseAudit.admissionState.verifiedSupportingCandidates === 9, "license audit candidate count changed");
assert(licenseAudit.admissionState.replacementCandidatesOutsidePacketInScope === 7, "license audit outside replacement count changed");
assert(licenseAudit.admissionState.historicalHelperSourcesInsidePacketInScope === 2, "license audit in-scope helper count changed");
assert(licenseAudit.admissionState.formalHelperAdmissions === 0, "license audit helper admission changed");
assert(licenseAudit.admissionState.admittedSupportingSources === 0, "license audit supporting admission changed");

const actualCandidateSourceIds = packageAudits.flatMap((entry) => entry.candidateSourceIds);
assert(JSON.stringify(actualCandidateSourceIds) === JSON.stringify(expectedCandidateSourceIds), "candidate source ID order or identity changed");
assert(new Set(actualCandidateSourceIds).size === 9, "candidate source IDs are not unique");

const cohortFiles = [
  ...packageAudits.flatMap((entry) => [
    {
      role: `${entry.family}-package-audit-generator`,
      path: entry.auditGeneratorPath,
      sizeBytes: entry.auditGeneratorSizeBytes,
      sha256: entry.auditGeneratorSha256,
    },
    {
      role: `${entry.family}-package-audit`,
      path: entry.auditPath,
      sizeBytes: entry.auditSizeBytes,
      sha256: entry.auditSha256,
    },
  ]),
  {
    role: "license-signal-audit-generator",
    path: licenseSpec.generatorPath,
    sizeBytes: licenseGeneratorBytes.length,
    sha256: licenseSpec.generatorSha256,
  },
  {
    role: "license-signal-audit",
    path: licenseSpec.auditPath,
    sizeBytes: licenseAuditBytes.length,
    sha256: licenseSpec.auditSha256,
  },
];
const cohortDigestRows = cohortFiles.map((file) => `${file.role}|${file.path}|${file.sizeBytes}|${file.sha256}`);
const cohortPackageDigest = sha256(Buffer.from(`${cohortDigestRows.join("\n")}\n`, "utf8"));

const candidateSources = actualCandidateSourceIds.map((sourceId, ordinal) => ({
  ordinal,
  sourceId,
  family: ordinal < 4 ? "pgr-replacement" : ordinal < 7 ? "hi3-replacement" : "hi3-helper-provenance",
  packetInScopeDisposition: ordinal < 7
    ? "versioned-replacement-candidate-outside-packet-in-scope"
    : "historical-source-id-inside-packet-in-scope-formal-admission-open",
  verificationState: "verified-candidate",
  admissionState: "not-admitted",
  admissionEffect: "none",
}));

const manifestWithoutDigest = {
  schemaVersion: 1,
  manifestId: MANIFEST_ID,
  status: "pass-nine-supporting-candidates-verified-zero-admitted",
  recordedAt: "2026-07-16T11:39:00+09:00",
  targetPacketId: "P1B-PGR-HI3-STAGE-SPINE-01",
  scope: "Cumulative candidate-only cutoff for the four PGR replacements, three HI3 replacements, two HI3 helper provenance candidates, and their factual repository-license signal audit.",
  packageAudits,
  licenseSignalAudit: {
    auditId: licenseSpec.auditId,
    auditPath: licenseSpec.auditPath,
    auditSizeBytes: licenseSpec.auditSizeBytes,
    auditSha256: licenseSpec.auditSha256,
    auditGeneratorPath: licenseSpec.generatorPath,
    auditGeneratorSizeBytes: licenseSpec.generatorSizeBytes,
    auditGeneratorSha256: licenseSpec.generatorSha256,
    canonicalAuditDigest: licenseSpec.canonicalAuditDigest,
    repositories: 4,
    noExplicitRepositoryLicenseSignal: 3,
    explicitRepositoryLicenseSignal: 1,
    rightsDisposition: "policy-or-rights-review-required-before-any-admission",
  },
  cohortFiles,
  cohortPackageDigestEncoding: "ordered role|path|sizeBytes|lowercaseSha256 rows; LF; final LF; UTF-8; SHA-256 lowercase",
  cohortPackageDigest,
  candidateSources,
  verifiedCounts: {
    supportingCandidates: 9,
    pgrReplacementCandidates: 4,
    hi3ReplacementCandidates: 3,
    hi3HelperProvenanceCandidates: 2,
    replacementCandidatesOutsidePacketInScope: 7,
    historicalHelperSourcesInsidePacketInScope: 2,
    formalHelperAdmissions: 0,
    admittedSupportingSources: 0,
    liveForeignRows: 0,
    liveCrosswalkCells: 0,
  },
  registryIntegrationBoundary: {
    requiredOperation: "id-based-compare-and-swap-merge-followed-by-dedicated-registry-validator",
    sourceInsertions: 3,
    historicalHelperSourceUpdates: 2,
    claimMutationAllowed: false,
    inScopeSourceIdMutationAllowedBeforeAtomicAdmission: false,
    crosswalkMutationAllowedBeforeAtomicAdmission: false,
    liveAcceptanceMutationAllowedBeforeAtomicAdmission: false,
  },
  admissionState: {
    candidateCohortVerified: true,
    candidateCohortAdmitted: false,
    supportingSourcesAdmitted: 0,
    requiredSupportingSources: 9,
    atomicElevenSourceAdmissionPassed: false,
    productAdoptionEffect: "none",
  },
  currentBlockers: [
    "The exact retained PGR, nairieberry, and DevilProMT snapshots expose no explicit repository reuse grant; an explicit policy/rights disposition or admissible replacement lineage is required.",
    "The MskTmi AGPL-3.0 repository contributes zero selected helper inputs and does not establish rights to third-party game data or media.",
    "The nine supporting candidates and the two raw authority sources must pass one atomic eleven-source provenance, row, drift, and registry admission cutoff before any live claim or crosswalk promotion.",
  ],
  negativeBoundary: "This manifest is a cumulative candidate-cohort integrity cutoff, not an atomic admission manifest, legal clearance, runtime trace, product-parity claim, or authorization to copy foreign authored data. It changes no source admission, claim mapping, crosswalk row, product owner, priority, or implementation gate.",
};
const manifest = { ...manifestWithoutDigest, canonicalManifestDigest: canonicalDigest(manifestWithoutDigest) };
const outputText = `${JSON.stringify(manifest, null, 2)}\n`;
assert(!outputText.includes("\r") && outputText.endsWith("\n") && !outputText.endsWith("\n\n"), "output normalization changed");

if (process.argv.includes("--verify")) {
  assert(existsSync(outputPath), "manifest output is missing");
  assert(readFileSync(outputPath, "utf8") === outputText, "manifest output bytes differ from reconstruction");
  console.log(`PASS ${MANIFEST_ID}`);
} else {
  writeFileSync(outputPath, outputText, "utf8");
  console.log(`WROTE ${MANIFEST_ID}`);
}
console.log(`supportingCandidates=9 pgr=4 hi3Replacement=3 hi3Helpers=2 admitted=0`);
console.log(`packetInScopeDisposition=replacementsOutside7 helpersInside2FormalOpen admitted0`);
console.log(`cohortPackageDigest=${cohortPackageDigest}`);
console.log(`canonicalManifestDigest=${manifest.canonicalManifestDigest}`);
console.log(`manifestSizeBytes=${Buffer.byteLength(outputText, "utf8")}`);
console.log(`manifestSha256=${sha256(Buffer.from(outputText, "utf8"))}`);
