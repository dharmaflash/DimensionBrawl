import { createHash } from "node:crypto";
import { existsSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { dirname, extname, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-HI3-STAGE-HELPER-INPUT-INVENTORY-V1-GENERATOR-01";
const CONTRACT_ID = "P1B-HI3-STAGE-HELPER-PROVENANCE-01";
const PRODUCER_CONTRACT_ID = "HI3-STAGE-HELPERS-REPLAY-PRODUCER-01";
const ARTIFACT_SET_ID = "HI3-STAGE-HELPERS-MIXED-20260615-V1";
const here = dirname(fileURLToPath(import.meta.url));
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";
const outputPath = join(here, "P1B_HI3_STAGE_HELPER_INPUT_INVENTORY_V1.csv");
const gameRoot = join(arkRoot, "games", "honkai-impact-3rd");
const patterns = ["stage", "monster", "level", "mapsite"];

const sourceSpecs = [
  {
    ordinal: 0,
    short: "devilpromt",
    slug: "devilpromt-bh3-data",
    upstream: "DevilProMT/BH3-Data",
    branch: "main",
    revision: "e92b3bdb413e74241f6f4a417a786c2704055997",
    committedAt: "2025-01-25T04:48:13Z",
    rootName: "BH3-Data-main",
    zipName: "DevilProMT-BH3-Data-main.zip",
    sourceRecord: { sizeBytes: 899, sha256: "ccdd783e53d93fb078db806be964958da47ceb8a7cd88fda483e2b3d0e2d9d36" },
    manifest: { sizeBytes: 1939, sha256: "f0d99eb5a0d6ce8a8b5716b04248129e7220e4bdd5b989041b1f8d5412d2135c" },
    zip: { sizeBytes: 30555318, sha256: "6c9ee52e068805b1a8d4a0e7cfb0de7d75959c7a18cc5449475e740141267ea3" },
    licenseDisposition: "none-detected-review-needed",
    expectedSelectedCount: 371,
    expectedSelectedBytes: 155164987,
  },
  {
    ordinal: 1,
    short: "nairieberry",
    slug: "nairieberry-honkaiimpactdata",
    upstream: "nairieberry/HonkaiImpactData",
    branch: "master",
    revision: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
    committedAt: "2021-04-10T22:17:18Z",
    rootName: "HonkaiImpactData-master",
    zipName: "nairieberry-HonkaiImpactData-master.zip",
    sourceRecord: { sizeBytes: 938, sha256: "eaf1785b6fb0cddcecca59047d7de229a4130f5f67512c3e6839848c8286352e" },
    manifest: { sizeBytes: 2004, sha256: "11e0be6d8e9f431d6f16da48213001edc033673e0709358798e5efe52d000faa" },
    zip: { sizeBytes: 121793389, sha256: "4184868dfcb9ebf2a07060e8f5c599df31c5e01ca1c20fbdffe952d6d6cafd6d" },
    licenseDisposition: "none-detected-review-needed",
    expectedSelectedCount: 1138,
    expectedSelectedBytes: 301292992,
  },
  {
    ordinal: 2,
    short: "msktmi",
    slug: "msktmi-elysianrealm-data",
    upstream: "MskTmi/ElysianRealm-Data",
    branch: "master",
    revision: "1debfbd44dc823b1864bc8a88f84c64c9a61499c",
    committedAt: "2026-06-05T12:59:23Z",
    rootName: "ElysianRealm-Data-master",
    zipName: "MskTmi-ElysianRealm-Data-master.zip",
    sourceRecord: { sizeBytes: 909, sha256: "97d7c4a414232254b9f2f86f171e71e6a2cc616fae492129cd30e2884bfa586e" },
    manifest: { sizeBytes: 1970, sha256: "204584b565ad899a1bb3e3433c6a3cfb9e75a30442261662c0103888bce20437" },
    zip: { sizeBytes: 49267918, sha256: "5c7a6a67c1e07803d8865cea1254416bb0d27558f2a83ecb268facc745dbe5ab" },
    licenseDisposition: "agpl-3.0-review-needed",
    expectedSelectedCount: 0,
    expectedSelectedBytes: 0,
  },
];

const fixedFiles = {
  producer: {
    path: join(arkRoot, "_tools", "analyze_honkai_impact_3rd_full_repos.py"),
    sizeBytes: 54701,
    sha256: "39daaf45913281619c054eabf71de2fde00e435f7efd0d5c3823f23a816953ea",
  },
  combinedManifest: {
    path: join(gameRoot, "raw", "_full-repo-analysis", "2026-06-15", "manifest.yml"),
    sizeBytes: 1192,
    sha256: "254f661c290ad29cd05c141cd9e4df131251adf58391b5f56a4e762082c33e33",
  },
  wrapper: {
    path: join(here, "P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY.py"),
    sizeBytes: 6449,
    sha256: "4c4ff6ff726dfb0e24138a8d57dcd38efd83f70f279791ac62c924d398afe2ec",
  },
  replayResult: {
    path: join(here, "P1B_HI3_STAGE_HELPER_ISOLATED_REPLAY_RESULT.json"),
    sizeBytes: 1155,
    sha256: "fc6fef25cb5eccff9e77170ee377c48bcda8819a86458e2d336d9ca2ea69f6f8",
  },
};

const existingOutputs = [
  {
    sourceId: "hi3-stage-summary-csv",
    role: "target-existing-historical-output",
    path: join(gameRoot, "enemies-stages", "hi3-stage-table-summary.csv"),
    relativePath: "games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv",
    sizeBytes: 295098,
    sha256: "d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7",
    dataRows: 1509,
    header: ["source", "region", "source_path", "table", "row_count", "bytes", "domains", "sample_keys"],
  },
  {
    sourceId: "hi3-stage-samples-csv",
    role: "target-existing-historical-output",
    path: join(gameRoot, "enemies-stages", "hi3-stage-row-samples.csv"),
    relativePath: "games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv",
    sizeBytes: 4459588,
    sha256: "5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92",
    dataRows: 14855,
    header: ["source", "region", "source_path", "table", "row_index", "id", "name", "level", "cost", "sample_json"],
  },
  {
    sourceId: "hi3-monster-summary-csv",
    role: "scratch-byproduct-excluded-from-supporting-nine",
    path: join(gameRoot, "enemies-stages", "hi3-monster-summary.csv"),
    relativePath: "games/honkai-impact-3rd/enemies-stages/hi3-monster-summary.csv",
    sizeBytes: 732650,
    sha256: "cffb5d4785fd2bf42eefed7a514fa5c45feaf14e54038d6e558e99513c47889b",
    dataRows: 3788,
    header: ["source", "region", "source_path", "row_index", "monsterName", "typeName", "subTypeName", "categoryName", "EliteType", "nature", "attack", "defense", "HP", "AIName", "configFile", "configType", "DisplayTitle"],
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

function readVerified(path, sizeBytes, expectedSha256) {
  const bytes = readFileSync(path);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === sizeBytes, `size mismatch for ${path}: ${bytes.length}`);
  assert(actualSha256 === expectedSha256, `SHA-256 mismatch for ${path}: ${actualSha256}`);
  return bytes;
}

function walkJson(root) {
  const files = [];
  const stack = [root];
  while (stack.length > 0) {
    const current = stack.pop();
    const entries = readdirSync(current, { withFileTypes: true });
    for (const entry of entries) {
      const path = join(current, entry.name);
      if (entry.isDirectory()) stack.push(path);
      else if (entry.isFile() && extname(entry.name).toLowerCase() === ".json") files.push(path);
    }
  }
  return files;
}

function isSelected(relativePath) {
  const normalized = relativePath.replaceAll("\\", "/").toLowerCase();
  const stem = normalized.slice(normalized.lastIndexOf("/") + 1, -5);
  return patterns.some((pattern) => stem.includes(pattern) || normalized.includes(pattern));
}

function csvEscape(value) {
  const text = String(value ?? "");
  return `"${text.replaceAll('"', '""')}"`;
}

function encodeCsv(headers, rows) {
  return `${headers.map(csvEscape).join(",")}\n${rows.map((row) => headers.map((header) => csvEscape(row[header])).join(",")).join("\n")}\n`;
}

function parseFirstCsvRecord(text) {
  let inQuotes = false;
  let field = "";
  const fields = [];
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
      fields.push(field);
      field = "";
    } else if (char === "\r" && text[index + 1] === "\n") {
      fields.push(field);
      return fields;
    } else {
      field += char;
    }
  }
  fail("CSV first record is not CRLF terminated");
}

function countCsvRecords(text) {
  let inQuotes = false;
  let records = 0;
  for (let index = 0; index < text.length; index += 1) {
    const char = text[index];
    if (char === '"') {
      if (inQuotes && text[index + 1] === '"') index += 1;
      else inQuotes = !inQuotes;
    } else if (!inQuotes && char === "\r" && text[index + 1] === "\n") {
      records += 1;
      index += 1;
    }
  }
  assert(!inQuotes, "CSV ended inside a quoted field");
  return records;
}

for (const file of Object.values(fixedFiles)) readVerified(file.path, file.sizeBytes, file.sha256);
const replayResult = JSON.parse(readFileSync(fixedFiles.replayResult.path, "utf8"));
assert(replayResult.status === "PASS", "isolated replay result is not PASS");
assert(replayResult.inputCount === 1509 && replayResult.inputBytes === 456457979, "isolated replay input totals changed");
assert(replayResult.inputInventorySha256 === "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662", "isolated replay inventory digest changed");

const inventoryRows = [];
for (const source of sourceSpecs) {
  const snapshotRoot = join(gameRoot, "raw", source.slug, "2026-06-15");
  const filesRoot = join(snapshotRoot, "files");
  const extractedRoot = join(filesRoot, "extracted_repo", source.rootName);
  readVerified(join(snapshotRoot, "source-record.md"), source.sourceRecord.sizeBytes, source.sourceRecord.sha256);
  readVerified(join(snapshotRoot, "manifest.yml"), source.manifest.sizeBytes, source.manifest.sha256);
  readVerified(join(filesRoot, source.zipName), source.zip.sizeBytes, source.zip.sha256);
  assert(resolve(extractedRoot).startsWith(resolve(snapshotRoot)), `extracted root escapes snapshot: ${source.short}`);
  const selectedFiles = walkJson(extractedRoot)
    .map((path) => ({ path, relativePath: relative(extractedRoot, path).replaceAll("\\", "/") }))
    .filter((file) => isSelected(file.relativePath));
  let contributionBytes = 0;
  for (const file of selectedFiles) {
    const bytes = readFileSync(file.path);
    try {
      JSON.parse(bytes.toString("utf8"));
    } catch {
      continue;
    }
    contributionBytes += bytes.length;
    inventoryRows.push({
      source: source.short,
      relativePath: file.relativePath,
      sizeBytes: bytes.length,
      sha256Uppercase: sha256(bytes).toUpperCase(),
    });
  }
  const contributionCount = inventoryRows.filter((row) => row.source === source.short).length;
  assert(contributionCount === source.expectedSelectedCount, `${source.short} selected count changed: ${contributionCount}`);
  assert(contributionBytes === source.expectedSelectedBytes, `${source.short} selected bytes changed: ${contributionBytes}`);
}

inventoryRows.sort((left, right) => {
  if (left.source < right.source) return -1;
  if (left.source > right.source) return 1;
  if (left.relativePath < right.relativePath) return -1;
  if (left.relativePath > right.relativePath) return 1;
  return 0;
});
assert(inventoryRows.length === 1509, `inventory count changed: ${inventoryRows.length}`);
assert(inventoryRows.reduce((sum, row) => sum + row.sizeBytes, 0) === 456457979, "inventory byte total changed");
const canonicalInventoryPayload = inventoryRows
  .map((row) => `${row.source}\t${row.relativePath}\t${row.sizeBytes}\t${row.sha256Uppercase}\n`)
  .join("");
const canonicalInventoryDigest = sha256(Buffer.from(canonicalInventoryPayload, "utf8"));
assert(canonicalInventoryDigest === "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662", `canonical inventory digest changed: ${canonicalInventoryDigest}`);

for (const output of existingOutputs) {
  const bytes = readVerified(output.path, output.sizeBytes, output.sha256);
  assert(!bytes.subarray(0, 3).equals(Buffer.from([0xef, 0xbb, 0xbf])), `${output.sourceId} contains UTF-8 BOM`);
  const text = bytes.toString("utf8");
  assert(!text.replaceAll("\r\n", "").includes("\r") && !text.replaceAll("\r\n", "").includes("\n"), `${output.sourceId} contains non-CRLF newline`);
  assert(text.endsWith("\r\n") && !text.endsWith("\r\n\r\n"), `${output.sourceId} must end with exactly one CRLF`);
  assert(countCsvRecords(text) - 1 === output.dataRows, `${output.sourceId} data-row count changed`);
  assert(JSON.stringify(parseFirstCsvRecord(text)) === JSON.stringify(output.header), `${output.sourceId} header changed`);
}

const inventoryHeaders = [
  "schema_version",
  "artifact_set_id",
  "input_ordinal",
  "source",
  "relative_path",
  "size_bytes",
  "sha256_uppercase",
];
const csvRows = inventoryRows.map((row, index) => ({
  schema_version: 1,
  artifact_set_id: ARTIFACT_SET_ID,
  input_ordinal: index,
  source: row.source,
  relative_path: row.relativePath,
  size_bytes: row.sizeBytes,
  sha256_uppercase: row.sha256Uppercase,
}));
const outputText = encodeCsv(inventoryHeaders, csvRows);
assert(!outputText.includes("\r"), "inventory output contains CR");
assert(outputText.endsWith("\n") && !outputText.endsWith("\n\n"), "inventory output must end with one LF");

if (process.argv.includes("--verify")) {
  assert(existsSync(outputPath), "inventory output is missing");
  assert(readFileSync(outputPath, "utf8") === outputText, "inventory output differs from reconstruction");
  console.log(`PASS ${CONTRACT_ID} input inventory`);
} else {
  writeFileSync(outputPath, outputText, "utf8");
  console.log(`WROTE ${CONTRACT_ID} input inventory`);
}

const outputBytes = Buffer.from(outputText, "utf8");
console.log(`producerContractId=${PRODUCER_CONTRACT_ID}`);
console.log(`artifactSetId=${ARTIFACT_SET_ID}`);
console.log(`inventoryRows=${inventoryRows.length}`);
console.log(`inventoryInputBytes=${inventoryRows.reduce((sum, row) => sum + row.sizeBytes, 0)}`);
console.log(`canonicalInventoryDigest=${canonicalInventoryDigest}`);
console.log(`inventoryArtifactSizeBytes=${outputBytes.length}`);
console.log(`inventoryArtifactSha256=${sha256(outputBytes)}`);
console.log("targetOutputsVerified=2 scratchByproductVerified=1 admissionEffect=none");
