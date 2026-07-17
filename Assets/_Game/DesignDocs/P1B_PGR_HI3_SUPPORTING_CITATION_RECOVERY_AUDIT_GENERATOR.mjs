import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-PGR-HI3-SUPPORTING-CITATION-RECOVERY-AUDIT-GENERATOR-01";
const REPORT_ID = "P1B-PGR-HI3-SUPPORTING-CITATION-RECOVERY-AUDIT-01";
const here = dirname(fileURLToPath(import.meta.url));
const reportPath = join(here, "P1B_PGR_HI3_SUPPORTING_CITATION_RECOVERY_AUDIT.json");
const candidatePath = join(here, "P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE.json");
const candidateGeneratorPath = join(here, "P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE_GENERATOR.mjs");
const pgrControlPath = join(here, "P1B_PGR_2020_GUIDEFIGHT_CONTROL.json");
const hi3ControlPath = join(here, "P1B_HI3_2021_STAGEDATA_10101_CONTROL.json");
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";

const expectedArtifacts = {
  candidate: {
    path: candidatePath,
    sizeBytes: 115910,
    sha256: "04ebf0a5be6db2535730088b3b7bcd7b6a50c48844292a43e1f9070418efed3d",
  },
  candidateGenerator: {
    path: candidateGeneratorPath,
    sizeBytes: 29212,
    sha256: "bb7f905d30ed38fe4121e62f44dfac009a55f7d3d74f60caf818f6644700883a",
  },
  pgrControl: {
    path: pgrControlPath,
    sizeBytes: 27682,
    sha256: "652e18da6fb09321550529191cc677df27f977d9ec98472c63bcabdba4db45da",
  },
  hi3Control: {
    path: hi3ControlPath,
    sizeBytes: 19673,
    sha256: "46607e2bbf3fb9bdc62ce47b5ae06d46636238b5573813f95a6689e7f583747c",
  },
};

const supportingCitationContract = [
  {
    sourceId: "pgr-readfirst-md",
    relativePath: "games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices.md",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "pgr-readfirst-summary-json",
    relativePath: "games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices-summary.json",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "pgr-guidefight-label-csv",
    relativePath: "games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-label-context.csv",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "pgr-guidefight-links-csv",
    relativePath: "games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-reading-links.csv",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "hi3-readfirst-md",
    relativePath: "games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.md",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "hi3-readfirst-summary-json",
    relativePath: "games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst-summary.json",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "hi3-readfirst-csv",
    relativePath: "games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.csv",
    expectedPathState: "absent-at-registered-path",
  },
  {
    sourceId: "hi3-stage-summary-csv",
    relativePath: "games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv",
    expectedPathState: "present-exact-path-unadmitted",
    expectedSizeBytes: 295098,
    expectedSha256: "d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7",
    contentCheck: "nairieberry-global-stagedata-main-summary-row",
  },
  {
    sourceId: "hi3-stage-samples-csv",
    relativePath: "games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv",
    expectedPathState: "present-exact-path-unadmitted",
    expectedSizeBytes: 4459588,
    expectedSha256: "5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92",
    contentCheck: "nairieberry-global-stagedata-main-first-truncated-sample-row",
  },
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

function readPinnedArtifact(name) {
  const expected = expectedArtifacts[name];
  const bytes = readFileSync(expected.path);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === expected.sizeBytes, `${name} size changed: ${bytes.length}`);
  assert(actualSha256 === expected.sha256, `${name} SHA-256 changed: ${actualSha256}`);
  return { bytes, sizeBytes: bytes.length, sha256: actualSha256 };
}

function canonicalize(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  return `{${Object.keys(value)
    .sort()
    .map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`)
    .join(",")}}`;
}

const candidateArtifact = readPinnedArtifact("candidate");
const candidateGeneratorArtifact = readPinnedArtifact("candidateGenerator");
const pgrControlArtifact = readPinnedArtifact("pgrControl");
const hi3ControlArtifact = readPinnedArtifact("hi3Control");
const candidate = JSON.parse(candidateArtifact.bytes.toString("utf8"));

assert(
  candidate.reportId === "P1B-PGR-HI3-STAGE-SPINE-RAW-CANDIDATE-01",
  "bound candidate report ID changed",
);
assert(
  candidate.canonicalPacketDigest === "f305cc6fdde04fa8b7a2e755b3995e62b297fa9bc08eac73550c00c3056d9b2d",
  "bound candidate canonical digest changed",
);
assert(candidate.crosswalkContract?.sourceRowCount === 5, "candidate source-row count changed");
assert(candidate.crosswalkContract?.totalCellCount === 70, "candidate cell count changed");
assert(candidate.crosswalkContract?.sourceValueCopiedCount === 0, "candidate copied a source value");

const historicalBriefingCells = candidate.crosswalkContract.cells.filter(
  (cell) => cell.semanticSlotId === "briefingAndCatalog",
);
assert(historicalBriefingCells.length === 5, "candidate briefing/catalog cell count changed");
assert(
  historicalBriefingCells.every(
    (cell) => cell.dimensionBrawlCutoffRef === "SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05",
  ),
  "candidate no longer has the audited historical Candidate-05 briefing/catalog axis",
);

const supportingCitations = supportingCitationContract.map((contract) => {
  const absolutePath = join(arkRoot, ...contract.relativePath.split("/"));
  const exists = existsSync(absolutePath);
  const actualPathState = exists
    ? "present-exact-path-unadmitted"
    : "absent-at-registered-path";
  assert(
    actualPathState === contract.expectedPathState,
    `${contract.sourceId} path state changed: ${actualPathState}`,
  );

  if (!exists) {
    return {
      sourceId: contract.sourceId,
      relativePath: contract.relativePath,
      pathState: actualPathState,
      sizeBytes: null,
      sha256: null,
      boundedContentCheck: null,
      admissionState: "unadmitted-path-absent",
      remainingBlockers: [
        "exact registered path absent",
        "source record absent",
        "producer manifest absent",
        "bounded regeneration command absent",
        "evidence grade and license disposition absent",
      ],
    };
  }

  const bytes = readFileSync(absolutePath);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === contract.expectedSizeBytes, `${contract.sourceId} size changed`);
  assert(actualSha256 === contract.expectedSha256, `${contract.sourceId} SHA-256 changed`);
  const text = bytes.toString("utf8");
  let boundedContentCheck = null;
  if (contract.contentCheck === "nairieberry-global-stagedata-main-summary-row") {
    boundedContentCheck = text
      .split(/\r?\n/)
      .some((line) => line.startsWith(
        "nairieberry,Global,Global/ExcelOutputAsset/Decrypted/StageData_Main.json,StageData_Main,9642,30600482,",
      ));
  } else if (contract.contentCheck === "nairieberry-global-stagedata-main-first-truncated-sample-row") {
    boundedContentCheck = text
      .split(/\r?\n/)
      .some((line) => line.startsWith(
        "nairieberry,Global,Global/ExcelOutputAsset/Decrypted/StageData_Main.json,StageData_Main,1,",
      ) && line.includes('""Hash"":846738401') && line.endsWith('""enterTim..."'));
  }
  assert(boundedContentCheck === true, `${contract.sourceId} bounded target content check failed`);

  return {
    sourceId: contract.sourceId,
    relativePath: contract.relativePath,
    pathState: actualPathState,
    sizeBytes: bytes.length,
    sha256: actualSha256,
    boundedContentCheck: contract.contentCheck,
    admissionState: "unadmitted-provenance-incomplete",
    remainingBlockers: [
      "registry source record path null",
      "registry producer manifest path null",
      "bounded regeneration command absent",
      "upstream revision and license disposition unverified",
      ...(contract.sourceId === "hi3-stage-samples-csv"
        ? ["the helper sample is truncated before levelId and cannot independently prove the 10101 target row"]
        : []),
      "computed size and SHA-256 not promoted into the atomic packet cohort",
    ],
  };
});

const exactPathPresent = supportingCitations.filter(
  (row) => row.pathState === "present-exact-path-unadmitted",
);
const absentAtRegisteredPath = supportingCitations.filter(
  (row) => row.pathState === "absent-at-registered-path",
);
assert(exactPathPresent.length === 2, "supporting exact-path present count changed");
assert(absentAtRegisteredPath.length === 7, "supporting absent count changed");

const reportWithoutDigest = {
  schemaVersion: 1,
  reportId: REPORT_ID,
  generatorId: GENERATOR_ID,
  observedAt: "2026-07-15T22:59:07+09:00",
  status: "historical-raw-candidate-reproducible-seven-supporting-paths-absent-two-present-unadmitted",
  purpose: "Correct the supporting-citation path inventory without rewriting the immutable five-row/seventy-cell raw candidate or admitting any foreign source into the active packet.",
  candidateBinding: {
    reportPath: "_Game/DesignDocs/P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE.json",
    reportId: candidate.reportId,
    reportSizeBytes: candidateArtifact.sizeBytes,
    reportSha256: candidateArtifact.sha256,
    canonicalPacketDigest: candidate.canonicalPacketDigest,
    generatorPath: "_Game/DesignDocs/P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE_GENERATOR.mjs",
    generatorSizeBytes: candidateGeneratorArtifact.sizeBytes,
    generatorSha256: candidateGeneratorArtifact.sha256,
    rawForeignSelectionState: "reproducible-four-pgr-plus-one-hi3-seventy-cells",
    packetAdmissionEffect: "none",
  },
  controlIntegrityAudit: {
    pgrControlPath: "_Game/DesignDocs/P1B_PGR_2020_GUIDEFIGHT_CONTROL.json",
    pgrControlSizeBytes: pgrControlArtifact.sizeBytes,
    pgrControlReportSha256: pgrControlArtifact.sha256,
    hi3ControlPath: "_Game/DesignDocs/P1B_HI3_2021_STAGEDATA_10101_CONTROL.json",
    hi3ControlSizeBytes: hi3ControlArtifact.sizeBytes,
    hi3ControlReportSha256: hi3ControlArtifact.sha256,
    currentCandidateGeneratorGap: "The historical candidate generator validates selected control fields but does not pin either complete control-report artifact or every PGR loadout drift input. Its next revision must do both before promotion.",
  },
  supportingCitationSummary: {
    contractedSupportingSourceCount: 9,
    exactPathPresentCount: exactPathPresent.length,
    absentAtRegisteredPathCount: absentAtRegisteredPath.length,
    admittedSupportingSourceCount: 0,
    reproducibleRawCandidateSourceCount: 2,
    requiredAtomicCohortSourceCount: 11,
    blockingSupportingSourceIds: supportingCitations.map((row) => row.sourceId),
    presentButUnadmittedSourceIds: exactPathPresent.map((row) => row.sourceId),
    absentSourceIds: absentAtRegisteredPath.map((row) => row.sourceId),
  },
  supportingCitations,
  currentDimensionBrawlComparisonBoundary: {
    historicalBriefingAndCatalogCellCount: historicalBriefingCells.length,
    historicalLocalCutoffRef: "SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05",
    currentAcceptedLocalCutoffRef: "SNAP-P1B-TRUTHFUL-JOINS-IMPLEMENTATION-04",
    regenerationRequired: true,
    reason: "The raw candidate foreign rows remain reproducible, but its DimensionBrawl briefing/catalog axis predates the accepted truthful-joins implementation and cannot be presented as a current-state local comparison.",
    effectOnForeignRowSelectionAndDrift: "none",
  },
  nextBoundedOperations: [
    "Preserve the immutable historical five-row/seventy-cell candidate and this correction separately.",
    "Revise the candidate generator to pin both complete control reports, assert PGR control loadout inputs, record its exact runtime/command, and regenerate the local comparison axes from the accepted truthful-joins cutoff.",
    "Recover or deterministically regenerate the seven absent supporting identities with exact source records, producer manifests, commands, evidence grades, and license dispositions.",
    "Authenticate the two present HI3 helper CSVs against their producer provenance; their hashes and bounded row checks alone do not admit them.",
    "Normalize both raw candidate records to the final validator-required snapshot, revision, command, and provenance fields.",
    "Run the existing eleven-source atomic LiveAcceptance only after every supporting and raw record is complete; do not early-admit the two raw sources or the two present helpers.",
  ],
  acceptanceEffect: "none; active packet inScopeSourceIds, raw admissions, generated report path/hash, crosswalkRows, claim mappings, and all three live acceptance results remain unchanged and open.",
  negativeBoundary: "Exact path presence is not provenance, a helper report is not a raw authority, the combined HI3 helpers cannot identify one producer by themselves, and this audit does not copy payload values, promote a claim, choose a product requirement, or authorize implementation.",
};

const canonicalAuditDigest = sha256(Buffer.from(canonicalize(reportWithoutDigest), "utf8"));
const report = { ...reportWithoutDigest, canonicalAuditDigest };
const output = JSON.stringify(report, null, 2).replaceAll("\r\n", "\n");
assert(!output.endsWith("\n"), "serialized report unexpectedly has a trailing newline");

if (process.argv.includes("--verify")) {
  const existing = readFileSync(reportPath, "utf8");
  assert(existing === output, `report bytes differ: ${reportPath}`);
  process.stdout.write(
    `${GENERATOR_ID}: VERIFY PASS reportSha256=${sha256(Buffer.from(existing, "utf8"))} canonicalAuditDigest=${canonicalAuditDigest} supporting=9 present=2 absent=7 admitted=0`,
  );
} else {
  writeFileSync(reportPath, output, "utf8");
  process.stdout.write(
    `${GENERATOR_ID}: WRITE PASS reportSha256=${sha256(Buffer.from(output, "utf8"))} canonicalAuditDigest=${canonicalAuditDigest} supporting=9 present=2 absent=7 admitted=0`,
  );
}
