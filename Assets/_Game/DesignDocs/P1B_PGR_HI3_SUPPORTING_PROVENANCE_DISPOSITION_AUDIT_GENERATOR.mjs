import { createHash } from "node:crypto";
import { existsSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-PGR-HI3-SUPPORTING-PROVENANCE-DISPOSITION-AUDIT-GENERATOR-01";
const REPORT_ID = "P1B-PGR-HI3-SUPPORTING-PROVENANCE-DISPOSITION-AUDIT-01";
const here = dirname(fileURLToPath(import.meta.url));
const reportPath = join(here, "P1B_PGR_HI3_SUPPORTING_PROVENANCE_DISPOSITION_AUDIT.json");
const predecessorPath = join(here, "P1B_PGR_HI3_SUPPORTING_CITATION_RECOVERY_AUDIT.json");
const replayPath = join(here, "P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py");
const replayResultPath = join(here, "P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json");
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";

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

function verifyFile(relativePath, expectedSizeBytes, expectedSha256) {
  const absolutePath = join(arkRoot, ...relativePath.split("/"));
  assert(existsSync(absolutePath), `missing Ark artifact: ${relativePath}`);
  const bytes = readFileSync(absolutePath);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === expectedSizeBytes, `${relativePath} size changed: ${bytes.length}`);
  assert(actualSha256 === expectedSha256, `${relativePath} SHA-256 changed: ${actualSha256}`);
  return {
    relativePath,
    sizeBytes: bytes.length,
    sha256: actualSha256,
  };
}

function verifyWorkspaceFile(path, expectedSizeBytes, expectedSha256) {
  const bytes = readFileSync(path);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === expectedSizeBytes, `${path} size changed: ${bytes.length}`);
  assert(actualSha256 === expectedSha256, `${path} SHA-256 changed: ${actualSha256}`);
  return { sizeBytes: bytes.length, sha256: actualSha256 };
}

const predecessor = verifyWorkspaceFile(
  predecessorPath,
  10888,
  "5240701338c92f3395ec3bc4716dd1f953637038382a4b557cf6f7d16fbebdda",
);
const predecessorJson = JSON.parse(readFileSync(predecessorPath, "utf8"));
assert(
  predecessorJson.canonicalAuditDigest === "27398ca5a9d0dfae6f3fdd01fff9d42099d8cb546386dc8ddf6eaa243ee0c991",
  "predecessor canonical audit digest changed",
);
assert(predecessorJson.supportingCitationSummary?.contractedSupportingSourceCount === 9, "predecessor source count changed");
assert(predecessorJson.supportingCitationSummary?.admittedSupportingSourceCount === 0, "predecessor admission state changed");

const replay = verifyWorkspaceFile(
  replayPath,
  6449,
  "4c4ff6ff726dfb0e24138a8d57dcd38efd83f70f279791ac62c924d398afe2ec",
);
const replayResult = verifyWorkspaceFile(
  replayResultPath,
  1155,
  "fc6fef25cb5eccff9e77170ee377c48bcda8819a86458e2d336d9ca2ea69f6f8",
);
const replayResultJson = JSON.parse(readFileSync(replayResultPath, "utf8"));
assert(replayResultJson.status === "PASS", "isolated replay result is not PASS");
assert(replayResultJson.inputCount === 1509, "isolated replay input count changed");
assert(replayResultJson.inputBytes === 456457979, "isolated replay input bytes changed");
assert(
  replayResultJson.inputInventorySha256 === "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662",
  "isolated replay input inventory digest changed",
);
assert(Array.isArray(replayResultJson.outputs) && replayResultJson.outputs.length === 2, "isolated replay output count changed");

const pgr = {
  sourceRecord: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/source-record.md",
    1118,
    "23cecc493fe4e69f59f73520e7da694c22ac76fc2283deb88070d165c37725ee",
  ),
  manifest: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/manifest.yml",
    1645,
    "00f535d4bb159a0f9a43a824bda3e9fad721ae3074717b01d0b68c4f1e86400d",
  ),
  fileManifest: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/file-manifest.csv",
    19913116,
    "f3909c0d8b24b9e2770cead82f86418a52536e6d2ab1602f43eae862bdc55115",
  ),
  enGuideFight: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/files/extracted_repo/PGR_Data-master/EN/bytes/share/guide/GuideFight.json",
    595,
    "62c184d70d88a3daf377ce6a2558c6e2cb192c4dbefbae3f02b4f549b6b46e7d",
  ),
  enStage: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/Stage.json",
    29637115,
    "7d553ada4ac1cd40e77054be70263260f7b2b2dd15948dc120e7ca806b26f940",
  ),
  zhGuideFight: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/files/extracted_repo/PGR_Data-master/ZH/bytes/share/guide/GuideFight.json",
    595,
    "62c184d70d88a3daf377ce6a2558c6e2cb192c4dbefbae3f02b4f549b6b46e7d",
  ),
  zhStage: verifyFile(
    "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/files/extracted_repo/PGR_Data-master/ZH/bytes/share/fuben/Stage.json",
    30511512,
    "ca3ad74480538148e7bc9a1a129569437e45bc83c670c0397d8062133dc6ee3a",
  ),
};

const hi3 = {
  producer: verifyFile(
    "_tools/analyze_honkai_impact_3rd_full_repos.py",
    54701,
    "39daaf45913281619c054eabf71de2fde00e435f7efd0d5c3823f23a816953ea",
  ),
  combinedManifest: verifyFile(
    "games/honkai-impact-3rd/raw/_full-repo-analysis/2026-06-15/manifest.yml",
    1192,
    "254f661c290ad29cd05c141cd9e4df131251adf58391b5f56a4e762082c33e33",
  ),
  stageSummary: verifyFile(
    "games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv",
    295098,
    "d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7",
  ),
  stageSamples: verifyFile(
    "games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv",
    4459588,
    "5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92",
  ),
  snapshots: [
    {
      snapshotId: "hi3-devilpromt-e92b3bd-full-repo",
      upstream: "DevilProMT/BH3-Data",
      revision: "e92b3bdb413e74241f6f4a417a786c2704055997",
      licenseDisposition: "none-detected-review-needed",
      selectedStageInputCount: 371,
      selectedStageInputBytes: 155164987,
      sourceRecord: verifyFile(
        "games/honkai-impact-3rd/raw/devilpromt-bh3-data/2026-06-15/source-record.md",
        899,
        "ccdd783e53d93fb078db806be964958da47ceb8a7cd88fda483e2b3d0e2d9d36",
      ),
      manifest: verifyFile(
        "games/honkai-impact-3rd/raw/devilpromt-bh3-data/2026-06-15/manifest.yml",
        1939,
        "f0d99eb5a0d6ce8a8b5716b04248129e7220e4bdd5b989041b1f8d5412d2135c",
      ),
    },
    {
      snapshotId: "hi3-nairieberry-01d7afb-full-repo",
      upstream: "nairieberry/HonkaiImpactData",
      revision: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
      licenseDisposition: "none-detected-review-needed",
      selectedStageInputCount: 1138,
      selectedStageInputBytes: 301292992,
      sourceRecord: verifyFile(
        "games/honkai-impact-3rd/raw/nairieberry-honkaiimpactdata/2026-06-15/source-record.md",
        938,
        "eaf1785b6fb0cddcecca59047d7de229a4130f5f67512c3e6839848c8286352e",
      ),
      manifest: verifyFile(
        "games/honkai-impact-3rd/raw/nairieberry-honkaiimpactdata/2026-06-15/manifest.yml",
        2004,
        "11e0be6d8e9f431d6f16da48213001edc033673e0709358798e5efe52d000faa",
      ),
    },
    {
      snapshotId: "hi3-msktmi-1debfbd-full-repo",
      upstream: "MskTmi/ElysianRealm-Data",
      revision: "1debfbd44dc823b1864bc8a88f84c64c9a61499c",
      licenseDisposition: "agpl-3.0-review-needed",
      selectedStageInputCount: 0,
      selectedStageInputBytes: 0,
      sourceRecord: verifyFile(
        "games/honkai-impact-3rd/raw/msktmi-elysianrealm-data/2026-06-15/source-record.md",
        909,
        "97d7c4a414232254b9f2f86f171e71e6a2cc616fae492129cd30e2884bfa586e",
      ),
      manifest: verifyFile(
        "games/honkai-impact-3rd/raw/msktmi-elysianrealm-data/2026-06-15/manifest.yml",
        1970,
        "204584b565ad899a1bb3e3433c6a3cfb9e75a30442261662c0103888bce20437",
      ),
    },
  ],
};

const missingDispositions = [
  ["pgr-readfirst-md", "games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices.md"],
  ["pgr-readfirst-summary-json", "games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices-summary.json"],
  ["pgr-guidefight-label-csv", "games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-label-context.csv"],
  ["pgr-guidefight-links-csv", "games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-reading-links.csv"],
  ["hi3-readfirst-md", "games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.md"],
  ["hi3-readfirst-summary-json", "games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst-summary.json"],
  ["hi3-readfirst-csv", "games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.csv"],
].map(([sourceId, relativePath]) => {
  const absolutePath = join(arkRoot, ...relativePath.split("/"));
  assert(!existsSync(absolutePath), `${sourceId} unexpectedly appeared at its historical path`);
  return {
    sourceId,
    relativePath,
    disposition: "historical-output-unrecoverable-replacement-contract-required",
    admissionState: "unadmitted",
    replacementRule: "Use a new versioned source/artifact identity and producer contract; never synthesize bytes under the historical identity.",
  };
});

const reportWithoutDigest = {
  schemaVersion: 1,
  reportId: REPORT_ID,
  generatorId: GENERATOR_ID,
  observedAt: "2026-07-15T23:58:00+09:00",
  status: "seven-historical-outputs-require-replacement-contract-two-hi3-helpers-byte-exact-replay-authenticated-formal-admission-open",
  predecessorAudit: {
    path: "_Game/DesignDocs/P1B_PGR_HI3_SUPPORTING_CITATION_RECOVERY_AUDIT.json",
    sizeBytes: predecessor.sizeBytes,
    sha256: predecessor.sha256,
    canonicalAuditDigest: predecessorJson.canonicalAuditDigest,
    preservationRule: "This audit refines provenance disposition without rewriting the earlier 7-absent/2-present path observation.",
  },
  summary: {
    supportingSourceCount: 9,
    historicalOutputReplacementContractRequiredCount: 7,
    byteExactReplayAuthenticatedFormalAdmissionOpenCount: 2,
    admittedCount: 0,
    atomicPacketGateState: "open-eleven-source-cohort-unchanged",
  },
  replacementContractRequired: missingDispositions,
  pgrReplacementAuthorityBoundary: {
    upstream: "alt3ri/PGR_Data",
    revision: "856a0e4534d0854fa440040e961b74a97ba732e2",
    snapshotDate: "2026-06-14",
    licenseDisposition: "unknown-review-needed",
    sourceRecord: pgr.sourceRecord,
    manifest: pgr.manifest,
    fileManifest: pgr.fileManifest,
    enAuthorityInputs: [pgr.enGuideFight, pgr.enStage],
    zhCompareOnlyInputs: [pgr.zhGuideFight, pgr.zhStage],
    exactEnJoinRows: [
      { guideFightId: 100001, stageId: 10010001 },
      { guideFightId: 100002, stageId: 10010002 },
      { guideFightId: 100003, stageId: 10010003 },
      { guideFightId: 100004, stageId: 10010005 },
    ],
    negativeBoundary: "These raw inputs make a new deterministic replacement possible, but they do not reconstruct the missing historical schemas, commands, ordering, normalization, bytes, or producer identity. ZH is compare-only and may not be unioned with EN.",
  },
  hi3StageHelperReplay: {
    disposition: "ex-post-byte-authenticated-reproducible-formal-provenance-record-and-admission-still-required",
    producer: hi3.producer,
    producerFunction: "make_stage_helpers; source lines 645-700 fix selector, fields, and output paths",
    retainedCombinedManifest: hi3.combinedManifest,
    retainedCombinedManifestBoundary: "The retained helper manifest records mixed sources and counts but omits a combined source record, producer hash, exact runtime/command, input inventory, and output hashes.",
    rawSnapshots: hi3.snapshots,
    selectedInputInventory: {
      canonicalEncoding: "source<TAB>relative-path<TAB>size<TAB>uppercase-sha256<LF>, rows sorted by source then ordinal relative path",
      inputCount: 1509,
      inputBytes: 456457979,
      sha256: "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662",
      contributionCounts: { devilpromt: 371, nairieberry: 1138, msktmi: 0 },
    },
    isolatedReplay: {
      wrapperPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py",
      wrapperSizeBytes: replay.sizeBytes,
      wrapperSha256: replay.sha256,
      resultPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json",
      resultSizeBytes: replayResult.sizeBytes,
      resultSha256: replayResult.sha256,
      runtime: "CPython 3.12.13",
      command: "& 'C:/Users/dharm/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' 'C:/Git/DimensionBrawl/Assets/_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py' --ark-root 'C:/Ark/SubcultureGameData' --output-root 'C:/tmp/DimensionBrawl-HI3-StageHelper-ContractReplay-20260715' --result-path 'C:/Git/DimensionBrawl/Assets/_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json'",
      sourceMutation: "none; output root guard rejects any path inside Ark",
      result: "PASS",
    },
    outputs: [
      { sourceId: "hi3-stage-summary-csv", ...hi3.stageSummary, dataRows: 1509 },
      { sourceId: "hi3-stage-samples-csv", ...hi3.stageSamples, dataRows: 14855 },
    ],
    formalAdmissionRemaining: [
      "write a combined producer source record or equivalent bounded provenance record",
      "retain the exact wrapper runtime and successful command/result artifact",
      "pin the 1509-input inventory as a versioned producer-manifest field",
      "resolve or explicitly preserve each upstream license disposition",
      "assign evidence grade and claim-to-row mapping",
      "retain the samples helper negative boundary: its truncated preview does not independently identify levelId 10101",
      "admit only with the full eleven-source atomic cohort",
    ],
  },
  replacementContractMinimum: [
    "new versioned artifact/source IDs and explicit replaced historical IDs",
    "exact raw snapshot, upstream revision/date, input paths, sizes, and SHA-256",
    "generator path/hash/revision, runtime, working directory, environment, and exact generate/verify commands",
    "output schema and field order, row selection/sort, UTF-8/BOM/newline/quoting/canonicalization rules",
    "output path, size, SHA-256, row/content invariants, source record, and producer manifest",
    "evidence grade, license disposition, claim-to-field mapping, and static-versus-runtime negative boundary",
  ],
  nextPriority: [
    "Create one new PGR replacement producer contract from the pinned EN authority inputs with ZH compare-only.",
    "Create one new HI3 read-first replacement producer contract from explicitly selected raw tables rather than guessing the missing historical schema.",
    "Promote neither replacement nor the two replay-authenticated helpers until their formal metadata and the complete eleven-source atomic gate pass.",
  ],
  acceptanceEffect: "none; inScopeSourceIds, claim mappings, generated report path/hash, crosswalkRows, live admissions, and all three LiveAcceptance results remain unchanged/open.",
  negativeBoundary: "Byte-exact replay proves derivation reproducibility, not runtime behavior or packet admission. Recoverable meaning does not recover missing historical bytes, and replacement artifacts must use new versioned identities.",
};

const canonicalAuditDigest = sha256(Buffer.from(canonicalize(reportWithoutDigest), "utf8"));
const report = { ...reportWithoutDigest, canonicalAuditDigest };
const encoded = `${JSON.stringify(report, null, 2)}\n`;

if (process.argv.includes("--verify")) {
  assert(existsSync(reportPath), `report is missing: ${reportPath}`);
  const actual = readFileSync(reportPath, "utf8");
  assert(actual === encoded, "report bytes differ from deterministic reconstruction");
  console.log(`PASS ${REPORT_ID}`);
  console.log(`reportSha256=${sha256(Buffer.from(actual, "utf8"))}`);
  console.log(`canonicalAuditDigest=${canonicalAuditDigest}`);
  console.log("replacementContractRequired=7");
  console.log("byteExactReplayAuthenticatedFormalAdmissionOpen=2");
  console.log("admitted=0");
} else {
  writeFileSync(reportPath, encoded, "utf8");
  const sizeBytes = statSync(reportPath).size;
  console.log(`WROTE ${reportPath}`);
  console.log(`sizeBytes=${sizeBytes}`);
  console.log(`reportSha256=${sha256(Buffer.from(encoded, "utf8"))}`);
  console.log(`canonicalAuditDigest=${canonicalAuditDigest}`);
}
