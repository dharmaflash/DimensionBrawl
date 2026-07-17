import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-HI3-STAGE-HELPER-PROVENANCE-V1-GENERATOR-01";
const CONTRACT_ID = "P1B-HI3-STAGE-HELPER-PROVENANCE-01";
const PRODUCER_CONTRACT_ID = "HI3-STAGE-HELPERS-REPLAY-PRODUCER-01";
const ARTIFACT_SET_ID = "HI3-STAGE-HELPERS-MIXED-20260615-V1";
const here = dirname(fileURLToPath(import.meta.url));
const workspaceRoot = resolve(here, "..", "..");
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";
const gameRoot = join(arkRoot, "games", "honkai-impact-3rd");

const inventoryGenerator = {
  generatorId: "P1B-HI3-STAGE-HELPER-INPUT-INVENTORY-V1-GENERATOR-01",
  path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_INPUT_INVENTORY_V1_GENERATOR.mjs",
  sizeBytes: 13998,
  sha256: "8ac184f12fc3f7d5635628875745fa1a4986d5b3c51f13bee9c0c76833e1c5e3",
  runtime: "Node.js v24.14.0 built-ins only",
};
const inventoryArtifact = {
  artifactId: "P1B-HI3-STAGE-HELPER-INPUT-INVENTORY-V1-CSV",
  path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_INPUT_INVENTORY_V1.csv",
  sizeBytes: 278176,
  sha256: "9eb12152642ff031d758415645b9ab95b6e312aab1a080e8775ea9dba653dc5c",
  dataRows: 1509,
  inputBytes: 456457979,
  canonicalInventoryDigest: "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662",
};
const retainedProducer = {
  path: "_tools/analyze_honkai_impact_3rd_full_repos.py",
  sizeBytes: 54701,
  sha256: "39daaf45913281619c054eabf71de2fde00e435f7efd0d5c3823f23a816953ea",
  function: "make_stage_helpers",
  runtime: "CPython 3.12.13 on Windows",
};
const replayWrapper = {
  path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py",
  sizeBytes: 6449,
  sha256: "4c4ff6ff726dfb0e24138a8d57dcd38efd83f70f279791ac62c924d398afe2ec",
};
const replayResult = {
  path: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json",
  sizeBytes: 1155,
  sha256: "fc6fef25cb5eccff9e77170ee377c48bcda8819a86458e2d336d9ca2ea69f6f8",
};
const combinedManifest = {
  path: "games/honkai-impact-3rd/raw/_full-repo-analysis/2026-06-15/manifest.yml",
  sizeBytes: 1192,
  sha256: "254f661c290ad29cd05c141cd9e4df131251adf58391b5f56a4e762082c33e33",
};

const upstreamSnapshots = [
  {
    ordinal: 0,
    sourceShort: "devilpromt",
    snapshotId: "hi3-devilpromt-e92b3bd-full-repo",
    upstream: "DevilProMT/BH3-Data",
    url: "https://github.com/DevilProMT/BH3-Data",
    branch: "main",
    revision: "e92b3bdb413e74241f6f4a417a786c2704055997",
    committedAt: "2025-01-25T04:48:13Z",
    snapshotDate: "2026-06-15",
    snapshotRelativeRoot: "games/honkai-impact-3rd/raw/devilpromt-bh3-data/2026-06-15",
    sourceRecord: { path: "source-record.md", sizeBytes: 899, sha256: "ccdd783e53d93fb078db806be964958da47ceb8a7cd88fda483e2b3d0e2d9d36" },
    manifest: { path: "manifest.yml", sizeBytes: 1939, sha256: "f0d99eb5a0d6ce8a8b5716b04248129e7220e4bdd5b989041b1f8d5412d2135c" },
    zip: { path: "files/DevilProMT-BH3-Data-main.zip", sizeBytes: 30555318, sha256: "6c9ee52e068805b1a8d4a0e7cfb0de7d75959c7a18cc5449475e740141267ea3" },
    licenseDisposition: "none-detected-review-needed",
    selectedInputCount: 371,
    selectedInputBytes: 155164987,
  },
  {
    ordinal: 1,
    sourceShort: "nairieberry",
    snapshotId: "hi3-nairieberry-01d7afb-full-repo",
    upstream: "nairieberry/HonkaiImpactData",
    url: "https://github.com/nairieberry/HonkaiImpactData",
    branch: "master",
    revision: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
    committedAt: "2021-04-10T22:17:18Z",
    snapshotDate: "2026-06-15",
    snapshotRelativeRoot: "games/honkai-impact-3rd/raw/nairieberry-honkaiimpactdata/2026-06-15",
    sourceRecord: { path: "source-record.md", sizeBytes: 938, sha256: "eaf1785b6fb0cddcecca59047d7de229a4130f5f67512c3e6839848c8286352e" },
    manifest: { path: "manifest.yml", sizeBytes: 2004, sha256: "11e0be6d8e9f431d6f16da48213001edc033673e0709358798e5efe52d000faa" },
    zip: { path: "files/nairieberry-HonkaiImpactData-master.zip", sizeBytes: 121793389, sha256: "4184868dfcb9ebf2a07060e8f5c599df31c5e01ca1c20fbdffe952d6d6cafd6d" },
    licenseDisposition: "none-detected-review-needed",
    selectedInputCount: 1138,
    selectedInputBytes: 301292992,
  },
  {
    ordinal: 2,
    sourceShort: "msktmi",
    snapshotId: "hi3-msktmi-1debfbd-full-repo",
    upstream: "MskTmi/ElysianRealm-Data",
    url: "https://github.com/MskTmi/ElysianRealm-Data",
    branch: "master",
    revision: "1debfbd44dc823b1864bc8a88f84c64c9a61499c",
    committedAt: "2026-06-05T12:59:23Z",
    snapshotDate: "2026-06-15",
    snapshotRelativeRoot: "games/honkai-impact-3rd/raw/msktmi-elysianrealm-data/2026-06-15",
    sourceRecord: { path: "source-record.md", sizeBytes: 909, sha256: "97d7c4a414232254b9f2f86f171e71e6a2cc616fae492129cd30e2884bfa586e" },
    manifest: { path: "manifest.yml", sizeBytes: 1970, sha256: "204584b565ad899a1bb3e3433c6a3cfb9e75a30442261662c0103888bce20437" },
    zip: { path: "files/MskTmi-ElysianRealm-Data-master.zip", sizeBytes: 49267918, sha256: "5c7a6a67c1e07803d8865cea1254416bb0d27558f2a83ecb268facc745dbe5ab" },
    licenseDisposition: "agpl-3.0-review-needed",
    selectedInputCount: 0,
    selectedInputBytes: 0,
  },
];

const targetOutputs = [
  {
    ordinal: 0,
    sourceId: "hi3-stage-summary-csv",
    identityDisposition: "existing-historical-output-retained-not-successor",
    path: "games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv",
    sizeBytes: 295098,
    sha256: "d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7",
    dataRows: 1509,
    header: ["source", "region", "source_path", "table", "row_count", "bytes", "domains", "sample_keys"],
    encoding: "UTF-8 no BOM; csv.DictWriter Excel dialect; CRLF; exactly one final CRLF",
    evidenceGrade: "exact-static-derived-helper-provenance-candidate",
    claimBoundary: "Table identity, source path, source row count, bytes, domains, and sample-key shape only; no exact 10101 row or runtime meaning.",
    admissionState: "formal-provenance-candidate-not-admitted",
  },
  {
    ordinal: 1,
    sourceId: "hi3-stage-samples-csv",
    identityDisposition: "existing-historical-output-retained-not-successor",
    path: "games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv",
    sizeBytes: 4459588,
    sha256: "5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92",
    dataRows: 14855,
    header: ["source", "region", "source_path", "table", "row_index", "id", "name", "level", "cost", "sample_json"],
    encoding: "UTF-8 no BOM; csv.DictWriter Excel dialect; CRLF; exactly one final CRLF",
    evidenceGrade: "exact-static-derived-helper-provenance-candidate",
    claimBoundary: "Truncated first-twelve-row previews only; the Global StageData_Main sample does not include or independently identify levelId 10101.",
    admissionState: "formal-provenance-candidate-not-admitted",
  },
];

const scratchByproduct = {
  sourceId: "hi3-monster-summary-csv",
  outputSetDisposition: "verified-scratch-byproduct-excluded-from-supporting-nine",
  path: "games/honkai-impact-3rd/enemies-stages/hi3-monster-summary.csv",
  sizeBytes: 732650,
  sha256: "cffb5d4785fd2bf42eefed7a514fa5c45feaf14e54038d6e558e99513c47889b",
  dataRows: 3788,
  countedAsTargetOutput: false,
  countedInSupportingNine: false,
};

const sourceRecordPath = join(here, "P1B_HI3_STAGE_HELPER_PROVENANCE_V1_SOURCE_RECORD.json");
const producerManifestPath = join(here, "P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PRODUCER_MANIFEST.json");

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

function parseQuotedCsv(text) {
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
    } else if (char === '"') {
      inQuotes = true;
    } else if (char === ",") {
      record.push(field);
      field = "";
    } else if (char === "\n") {
      record.push(field);
      records.push(record);
      record = [];
      field = "";
    } else {
      field += char;
    }
  }
  assert(!inQuotes && record.length === 0 && field === "", "inventory CSV normalization changed");
  return records;
}

readVerified(join(workspaceRoot, ...inventoryGenerator.path.split("/")), inventoryGenerator.sizeBytes, inventoryGenerator.sha256);
const inventoryBytes = readVerified(join(workspaceRoot, ...inventoryArtifact.path.split("/")), inventoryArtifact.sizeBytes, inventoryArtifact.sha256);
readVerified(join(arkRoot, ...retainedProducer.path.split("/")), retainedProducer.sizeBytes, retainedProducer.sha256);
readVerified(join(workspaceRoot, ...replayWrapper.path.split("/")), replayWrapper.sizeBytes, replayWrapper.sha256);
const replayBytes = readVerified(join(workspaceRoot, ...replayResult.path.split("/")), replayResult.sizeBytes, replayResult.sha256);
readVerified(join(arkRoot, ...combinedManifest.path.split("/")), combinedManifest.sizeBytes, combinedManifest.sha256);

for (const snapshot of upstreamSnapshots) {
  const root = join(arkRoot, ...snapshot.snapshotRelativeRoot.split("/"));
  readVerified(join(root, ...snapshot.sourceRecord.path.split("/")), snapshot.sourceRecord.sizeBytes, snapshot.sourceRecord.sha256);
  readVerified(join(root, ...snapshot.manifest.path.split("/")), snapshot.manifest.sizeBytes, snapshot.manifest.sha256);
  readVerified(join(root, ...snapshot.zip.path.split("/")), snapshot.zip.sizeBytes, snapshot.zip.sha256);
}
for (const output of [...targetOutputs, scratchByproduct]) readVerified(join(arkRoot, ...output.path.split("/")), output.sizeBytes, output.sha256);

const replay = JSON.parse(replayBytes.toString("utf8"));
assert(replay.status === "PASS", "replay status changed");
assert(replay.inputCount === inventoryArtifact.dataRows && replay.inputBytes === inventoryArtifact.inputBytes, "replay inventory totals changed");
assert(replay.inputInventorySha256 === inventoryArtifact.canonicalInventoryDigest, "replay inventory digest changed");
assert(replay.outputs.length === 2, "replay target output count changed");
for (const output of targetOutputs) {
  const replayOutput = replay.outputs.find((candidate) => candidate.name === output.path.split("/").at(-1));
  assert(replayOutput && replayOutput.sizeBytes === output.sizeBytes && replayOutput.sha256 === output.sha256 && replayOutput.dataRows === output.dataRows, `replay binding changed for ${output.sourceId}`);
}

const inventoryRecords = parseQuotedCsv(inventoryBytes.toString("utf8"));
const expectedHeader = ["schema_version", "artifact_set_id", "input_ordinal", "source", "relative_path", "size_bytes", "sha256_uppercase"];
assert(JSON.stringify(inventoryRecords[0]) === JSON.stringify(expectedHeader), "inventory header changed");
assert(inventoryRecords.length - 1 === inventoryArtifact.dataRows, "inventory data-row count changed");
const materializedRows = inventoryRecords.slice(1).map((record, index) => {
  assert(record.length === expectedHeader.length, `inventory row ${index} column count changed`);
  assert(record[0] === "1" && record[1] === ARTIFACT_SET_ID && Number(record[2]) === index, `inventory row ${index} identity changed`);
  return { source: record[3], relativePath: record[4], sizeBytes: Number(record[5]), sha256Uppercase: record[6] };
});
const canonicalInventoryPayload = materializedRows.map((row) => `${row.source}\t${row.relativePath}\t${row.sizeBytes}\t${row.sha256Uppercase}\n`).join("");
assert(sha256(Buffer.from(canonicalInventoryPayload, "utf8")) === inventoryArtifact.canonicalInventoryDigest, "materialized inventory digest reconstruction failed");
assert(materializedRows.reduce((sum, row) => sum + row.sizeBytes, 0) === inventoryArtifact.inputBytes, "materialized inventory bytes changed");
for (const snapshot of upstreamSnapshots) {
  const selected = materializedRows.filter((row) => row.source === snapshot.sourceShort);
  assert(selected.length === snapshot.selectedInputCount, `${snapshot.sourceShort} materialized count changed`);
  assert(selected.reduce((sum, row) => sum + row.sizeBytes, 0) === snapshot.selectedInputBytes, `${snapshot.sourceShort} materialized bytes changed`);
}

const execution = {
  workingDirectory: "C:/Git/DimensionBrawl/Assets",
  platformBoundary: "Windows filesystem ordering plus CPython 3.12.13 is frozen for byte-exact historical-output replay",
  arkRoot: "C:/Ark/SubcultureGameData",
  scratchOutputRoot: "C:/tmp/DimensionBrawl-HI3-StageHelper-ContractReplay-20260715",
  command: "& 'C:/Users/dharm/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' 'C:/Git/DimensionBrawl/Assets/_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py' --ark-root 'C:/Ark/SubcultureGameData' --output-root 'C:/tmp/DimensionBrawl-HI3-StageHelper-ContractReplay-20260715' --result-path 'C:/Git/DimensionBrawl/Assets/_Game/DesignDocs/P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json'",
  runtime: "CPython 3.12.13",
  exitCode: 0,
  sourceMutation: "none; wrapper rejects output paths inside Ark",
  result: "PASS",
};

const sourceRecordWithoutDigest = {
  schemaVersion: 1,
  sourceRecordId: "P1B-HI3-STAGE-HELPER-PROVENANCE-V1-SOURCE-RECORD-01",
  contractId: CONTRACT_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  artifactSetId: ARTIFACT_SET_ID,
  status: "formal-provenance-candidate-verified-admission-open",
  recordedAt: "2026-07-16T03:35:00+09:00",
  game: "Honkai Impact 3rd",
  sourceRoot: {
    path: "C:/Ark/SubcultureGameData",
    scope: "local-retained-mirror-mixed-snapshot-helper-provenance-candidate",
  },
  historicalIdentityBoundary: {
    targetSourceIds: targetOutputs.map((output) => output.sourceId),
    identityDisposition: "existing historical output bytes retained under existing source IDs; no successor identity created",
    partialAdmissionAllowed: false,
  },
  upstreamSnapshots,
  combinedManifest,
  retainedProducer,
  inventoryGenerator,
  inventoryArtifact,
  replayWrapper,
  replayResult,
  execution,
  targetOutputs,
  scratchByproduct,
  selectionBoundary: {
    sourceOrder: ["devilpromt", "nairieberry", "msktmi"],
    fileDiscovery: "recursive *.json under each extracted root",
    matchRule: "case-insensitive Stage|Monster|Level|MapSite substring in filename stem or relative path",
    parseRule: "include only JSON parse success",
    producerIterationBoundary: "historical byte replay retains Windows Path ordering; materialized inventory canonicalizes source then relative path by ordinal string order",
    inputCount: inventoryArtifact.dataRows,
    inputBytes: inventoryArtifact.inputBytes,
    canonicalInventoryDigest: inventoryArtifact.canonicalInventoryDigest,
  },
  evidenceBoundary: {
    evidenceGrade: "exact-static-derived-helper-provenance-candidate",
    claimMapping: [
      "hi3-stage-summary-csv supports table/source/path/count/size/domain/sample-key shape only",
      "hi3-stage-samples-csv supports bounded truncated sample presence only and cannot independently identify levelId 10101",
    ],
    runtimeTraceEvidenceRefs: [],
    productAdoptionEffect: "none",
    elevenSourceAdmissionEffect: "none",
    helperFormalProvenanceCandidatesVerified: 2,
    helperSourcesAdmitted: 0,
    helperSourcesRequired: 2,
    admittedSupportingSourceCount: 0,
    requiredSupportingSourceCount: 9,
    liveForeignRowCount: 0,
    liveCrosswalkCellCount: 0,
  },
  admissionBlockedReasons: [
    "The DevilProMT and nairieberry snapshots remain none-detected-review-needed, and the zero-contribution MskTmi snapshot remains AGPL-3.0-review-needed.",
    "The two helpers cannot identify or substitute the exact HI3 levelId 10101 authority row.",
    "The complete eleven-source cohort must be admitted atomically; helper provenance verification does not admit either source alone.",
  ],
  sourceRecordPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_SOURCE_RECORD.json",
  producerManifestPath: "_Game/DesignDocs/P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PRODUCER_MANIFEST.json",
};
const sourceRecord = { ...sourceRecordWithoutDigest, canonicalSourceRecordDigest: canonicalDigest(sourceRecordWithoutDigest) };

const producerManifestWithoutDigest = {
  schemaVersion: 1,
  producerManifestId: "P1B-HI3-STAGE-HELPER-PROVENANCE-V1-PRODUCER-MANIFEST-01",
  contractId: CONTRACT_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  artifactSetId: ARTIFACT_SET_ID,
  status: "verified-provenance-candidate-not-admitted",
  orderedProvenanceDependencies: [
    ...upstreamSnapshots.flatMap((snapshot) => [
      { role: `${snapshot.sourceShort}-source-record`, path: `${snapshot.snapshotRelativeRoot}/${snapshot.sourceRecord.path}`, sizeBytes: snapshot.sourceRecord.sizeBytes, sha256: snapshot.sourceRecord.sha256 },
      { role: `${snapshot.sourceShort}-manifest`, path: `${snapshot.snapshotRelativeRoot}/${snapshot.manifest.path}`, sizeBytes: snapshot.manifest.sizeBytes, sha256: snapshot.manifest.sha256 },
      { role: `${snapshot.sourceShort}-zip`, path: `${snapshot.snapshotRelativeRoot}/${snapshot.zip.path}`, sizeBytes: snapshot.zip.sizeBytes, sha256: snapshot.zip.sha256 },
    ]),
    { role: "combined-manifest", ...combinedManifest },
    { role: "retained-producer", ...retainedProducer },
    { role: "bounded-replay-wrapper", ...replayWrapper },
    { role: "bounded-replay-result", ...replayResult },
    { role: "materialized-inventory-generator", ...inventoryGenerator },
    { role: "materialized-inventory", ...inventoryArtifact },
  ],
  selectedInputInventory: {
    schemaVersion: 1,
    materializedArtifact: inventoryArtifact,
    canonicalEncoding: "source<TAB>relative-path<TAB>size<TAB>uppercase-sha256<LF>; source then relative path ordinal sort",
    inputCount: inventoryArtifact.dataRows,
    inputBytes: inventoryArtifact.inputBytes,
    canonicalInventoryDigest: inventoryArtifact.canonicalInventoryDigest,
    contributionCounts: Object.fromEntries(upstreamSnapshots.map((snapshot) => [snapshot.sourceShort, snapshot.selectedInputCount])),
    contributionBytes: Object.fromEntries(upstreamSnapshots.map((snapshot) => [snapshot.sourceShort, snapshot.selectedInputBytes])),
  },
  execution,
  orderedTargetOutputs: targetOutputs,
  excludedScratchByproducts: [scratchByproduct],
  outputSetInvariant: {
    targetOutputCount: 2,
    existingHistoricalIdentityCount: 2,
    successorIdentityCount: 0,
    excludedScratchByproductCount: 1,
    supportingNineContributionCount: 2,
    admittedCount: 0,
  },
  normalization: {
    historicalTargetEncoding: "UTF-8 no BOM",
    historicalTargetCsvDialect: "Python csv.DictWriter default Excel dialect",
    historicalTargetNewline: "CRLF with exactly one final CRLF",
    inventoryEncoding: "UTF-8 no BOM; LF; exactly one final LF; every cell double-quoted RFC4180",
    jsonEncoding: "UTF-8 no BOM; LF; exactly one final LF; two-space indent",
    canonicalDigest: "recursive ordinal-sorted object keys; compact JSON; arrays preserve authored order; SHA-256 lowercase",
  },
  negativeBoundaries: [
    "Byte-exact replay proves derivation reproducibility, not runtime behavior, current shipped behavior, or packet admission.",
    "The sample helper contains truncated previews and cannot independently identify levelId 10101.",
    "MskTmi contributes zero selected inputs but remains pinned because the retained producer iterates the full three-source set; its license is not inherited by target outputs without review.",
    "The monster summary is verified as an excluded scratch byproduct and does not become a tenth supporting source.",
    "No helper row proves Lua execution, challenge evaluation, result handling, reward grant, persistence, retry, or cleanup.",
  ],
  verificationResult: {
    inventoryGenerate: "PASS exit 0",
    inventoryVerify: "PASS exit 0",
    isolatedReplay: "PASS exit 0",
    targetOutputsVerified: 2,
    scratchByproductsVerified: 1,
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
    assert(readFileSync(path, "utf8") === text, `${path} differs from reconstruction`);
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
console.log("helperCandidatesVerified=2 helperAdmitted=0 supportingAdmitted=0 admissionEffect=none");
