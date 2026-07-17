import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-HI3-STAGE-SPINE-REPLACEMENT-V1-GENERATOR-01";
const CONTRACT_ID = "P1B-HI3-STAGE-SPINE-REPLACEMENT-01";
const PRODUCER_CONTRACT_ID = "HI3-STAGEDATA-STAGE-SPINE-PRODUCER-01";
const ARTIFACT_SET_ID = "HI3-STAGEDATA-STAGE-SPINE-NAIRIEBERRY-01D7AFB-V1";
const SOURCE_SNAPSHOT_ID = "hi3-nairieberry-01d7afb-global-stagedata-spine-v1";
const TARGET_LEVEL_ID = 10101;
const here = dirname(fileURLToPath(import.meta.url));
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";
const snapshotRoot = join(
  arkRoot,
  "games",
  "honkai-impact-3rd",
  "raw",
  "nairieberry-honkaiimpactdata",
  "2026-06-15",
);

const outputPaths = {
  readFirstMarkdown: join(here, "P1B_HI3_STAGE_SPINE_READFIRST_V1.md"),
  readFirstSummary: join(here, "P1B_HI3_STAGE_SPINE_READFIRST_V1_SUMMARY.json"),
  readingLinks: join(here, "P1B_HI3_STAGEDATA_STAGE_READING_LINKS_V1.csv"),
};

const artifacts = [
  {
    artifactKey: "readFirstMarkdown",
    sourceId: "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-readfirst-md",
    artifactId: "P1B-HI3-STAGE-SPINE-READFIRST-V1-MD",
    replacesHistoricalSourceId: "hi3-readfirst-md",
    format: "markdown",
  },
  {
    artifactKey: "readFirstSummary",
    sourceId: "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-summary-json",
    artifactId: "P1B-HI3-STAGE-SPINE-READFIRST-V1-SUMMARY-JSON",
    replacesHistoricalSourceId: "hi3-readfirst-summary-json",
    format: "json",
  },
  {
    artifactKey: "readingLinks",
    sourceId: "hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-reading-links-csv",
    artifactId: "P1B-HI3-STAGEDATA-STAGE-READING-LINKS-V1-CSV",
    replacesHistoricalSourceId: "hi3-readfirst-csv",
    format: "csv",
  },
];

const inputSpecs = [
  {
    inputId: "hi3-source-record",
    role: "source-record",
    relativePath: "source-record.md",
    sizeBytes: 938,
    sha256: "eaf1785b6fb0cddcecca59047d7de229a4130f5f67512c3e6839848c8286352e",
  },
  {
    inputId: "hi3-snapshot-manifest",
    role: "snapshot-manifest",
    relativePath: "manifest.yml",
    sizeBytes: 2004,
    sha256: "11e0be6d8e9f431d6f16da48213001edc033673e0709358798e5efe52d000faa",
  },
  {
    inputId: "hi3-file-manifest",
    role: "file-manifest",
    relativePath: "files/hi3-nairieberry-file-manifest.csv",
    sizeBytes: 646441,
    sha256: "c0c63cbf79f26d3e7f11e651c4fab6047b814b4aea3f17f9c8b9fafdb3c94cb8",
  },
  {
    inputId: "hi3-global-stage-data-main",
    role: "authoritative-level-10101-static-stage-row",
    relativePath: "files/extracted_repo/HonkaiImpactData-master/Global/ExcelOutputAsset/Decrypted/StageData_Main.json",
    sizeBytes: 30600482,
    sha256: "6ab32c175b399d89d035e9736d150760725dd4f85cc5bd9870c64093c51a7431",
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

function canonicalize(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`).join(",")}}`;
}

function canonicalHash(value) {
  const bytes = Buffer.from(canonicalize(value), "utf8");
  return { sizeBytes: bytes.length, sha256: sha256(bytes) };
}

function csvEscape(value) {
  const text = String(value ?? "");
  return `"${text.replaceAll('"', '""')}"`;
}

function encodeCsv(headers, rows) {
  return `${headers.map(csvEscape).join(",")}\n${rows.map((row) => headers.map((header) => csvEscape(row[header])).join(",")).join("\n")}\n`;
}

function shapeState(value) {
  if (value === null) return "null";
  if (Array.isArray(value)) return value.length === 0 ? "array-empty" : "array-present";
  if (typeof value === "string") return value.length === 0 ? "string-empty" : "string-present";
  if (typeof value === "object") return Object.keys(value).length === 0 ? "object-empty" : "object-present";
  if (typeof value === "boolean") return "boolean";
  if (typeof value === "number") return value === 0 ? "number-zero" : "number-nonzero";
  return "other";
}

function resolveField(root, path) {
  let current = root;
  for (const segment of path.split(".")) {
    if (current === null || typeof current !== "object" || !Object.hasOwn(current, segment)) {
      return { exists: false, value: undefined };
    }
    current = current[segment];
  }
  return { exists: true, value: current };
}

function countBy(items, selector) {
  const counts = {};
  for (const item of items) {
    const key = selector(item);
    counts[key] = (counts[key] || 0) + 1;
  }
  return counts;
}

const inputIntegrity = [];
let stageRows = null;
for (const spec of inputSpecs) {
  const absolutePath = join(snapshotRoot, ...spec.relativePath.split("/"));
  assert(resolve(absolutePath).startsWith(resolve(snapshotRoot)), `input escapes snapshot root: ${spec.inputId}`);
  const bytes = readFileSync(absolutePath);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === spec.sizeBytes, `${spec.inputId} size changed: ${bytes.length}`);
  assert(actualSha256 === spec.sha256, `${spec.inputId} SHA-256 changed: ${actualSha256}`);
  inputIntegrity.push({ ...spec });
  if (spec.inputId === "hi3-global-stage-data-main") stageRows = JSON.parse(bytes.toString("utf8"));
}

assert(Array.isArray(stageRows), "StageData_Main root must be an array");
assert(stageRows.length === 9642, `StageData_Main row count changed: ${stageRows.length}`);
assert(new Set(stageRows.map((row) => String(row.levelId))).size === stageRows.length, "StageData_Main levelId values must remain unique");
const targetMatches = stageRows
  .map((row, index) => ({ row, index }))
  .filter(({ row }) => typeof row.levelId === "number" && row.levelId === TARGET_LEVEL_ID);
assert(targetMatches.length === 1, `numeric levelId=${TARGET_LEVEL_ID} must resolve exactly once`);
const target = targetMatches[0];
assert(target.index === 1, `levelId=${TARGET_LEVEL_ID} source ordinal changed`);
assert(Object.keys(target.row).length === 67, `levelId=${TARGET_LEVEL_ID} top-level key count changed`);

const targetRowProjection = canonicalHash(target.row);
assert(targetRowProjection.sizeBytes === 1665, "target canonical row size changed");
assert(targetRowProjection.sha256 === "93eb25ca807d6a7f5230cd1ca52e66d68c9f956db3eab25d8013d338699c968f", "target canonical row digest changed");
const topLevelKeySet = Object.keys(target.row).sort();
const topLevelKeySetProjection = canonicalHash(topLevelKeySet);
assert(topLevelKeySetProjection.sizeBytes === 1037, "target key-set size changed");
assert(topLevelKeySetProjection.sha256 === "bf6bba4b74ba32cfc80828ba569dc3fc96ae578406c43ac160b4b2ad6a226eec", "target key-set digest changed");

const fieldShapeRows = topLevelKeySet.map((fieldPath) => ({
  fieldPath,
  shapeState: shapeState(target.row[fieldPath]),
}));
const fieldShapeProjection = canonicalHash(fieldShapeRows);
assert(fieldShapeProjection.sizeBytes === 3907, `field-shape size changed: ${fieldShapeProjection.sizeBytes}`);
assert(fieldShapeProjection.sha256 === "19833743758af7f5987d0fb591c82d9e275eb82e57d8c2d2c5ff806306abbb91", "field-shape digest changed");
const fieldShapeCounts = countBy(fieldShapeRows, (row) => row.shapeState);
assert(canonicalize(fieldShapeCounts) === canonicalize({
  "array-empty": 5,
  "array-present": 11,
  "number-nonzero": 25,
  "number-zero": 16,
  "object-present": 5,
  "string-empty": 2,
  "string-present": 3,
}), `field-shape counts changed: ${JSON.stringify(fieldShapeCounts)}`);

const nestedObjectFields = ["name", "displayTitle", "displayDetail", "LockedText", "UnlockedText"];
const nestedObjectKeySets = nestedObjectFields.map((fieldPath) => {
  const value = target.row[fieldPath];
  assert(value && typeof value === "object" && !Array.isArray(value), `${fieldPath} must remain an object`);
  const keys = Object.keys(value).sort();
  assert(keys.length === 1 && keys[0] === "Hash", `${fieldPath} nested key set changed`);
  return { fieldPath, keyCount: keys.length, keySetSha256: canonicalHash(keys).sha256 };
});

const semanticSlots = [
  {
    ordinal: 0,
    id: "logicalStageId",
    valueState: "present",
    classification: "proven-static",
    fields: ["levelId", "chapterId", "actId", "sectionId", "difficulty", "type", "tag", "battleType"],
    mappingDisposition: "identity-and-static-hierarchy-shape-only",
    supportedStatement: "The exact row contains a logical stage identity and static hierarchy/type field family.",
    negativeBoundary: "Field presence does not prove the runtime admission key, routing semantics, or parity with DimensionBrawl identifiers.",
  },
  {
    ordinal: 1,
    id: "physicalSceneOrScript",
    valueState: "present",
    classification: "proven-static",
    fields: ["luaFile"],
    mappingDisposition: "static-script-reference-shape-only",
    supportedStatement: "The exact row contains one non-empty script-reference field.",
    negativeBoundary: "The script bytes and consumers were not inspected, so scene loading, wave execution, and cleanup ownership remain unproven.",
  },
  {
    ordinal: 2,
    id: "briefingAndCatalog",
    valueState: "present",
    classification: "proven-static",
    fields: ["name.Hash", "displayTitle.Hash", "displayDetail.Hash", "briefPicPath", "detailPicPath"],
    mappingDisposition: "hashed-text-and-asset-reference-shape-only",
    supportedStatement: "The exact row contains hashed catalog text references and briefing/detail image-path fields.",
    negativeBoundary: "No localized text or image payload is copied, and field presence does not prove presentation runtime ownership.",
  },
  {
    ordinal: 3,
    id: "recommendedPowerOrLevel",
    valueState: "present",
    classification: "proven-static",
    fields: ["recommendPlayerLevel", "unlockPlayerLevel", "hardLevel", "hardLevelGroup"],
    mappingDisposition: "static-level-and-difficulty-shape-only",
    supportedStatement: "The exact row contains recommended, unlock, difficulty, and hard-level authoring fields.",
    negativeBoundary: "Numeric meanings, formulas, balancing policy, and runtime enforcement remain unverified.",
  },
  {
    ordinal: 4,
    id: "loadout",
    valueState: "unresolved",
    classification: "unknown",
    fields: ["teamNum", "maxNumList", "isEnterWithElf", "restrictList"],
    mappingDisposition: "formation-shape-without-loadout-identity",
    supportedStatement: "Formation-count and restriction-shaped fields exist, but no direct avatar, weapon, equipment, or named loadout identity is established.",
    negativeBoundary: "Do not reinterpret formation constraints as loadout references or infer array contents; linked tables and runtime owners remain outside scope.",
  },
  {
    ordinal: 5,
    id: "restrictions",
    valueState: "present",
    classification: "proven-static",
    fields: ["restrictList", "teamNum", "maxNumList", "isEnterWithElf", "reviveTimes", "reviveCostType", "ReviveUseTypeList", "enterTimes", "enterTimesType", "MonsterAttrShow", "HardCoeff", "UseDynamicHardLv", "BalanceModeType"],
    mappingDisposition: "static-restriction-field-family-only",
    supportedStatement: "The exact row exposes static restriction, formation, revive, entry-count, monster-attribute, and balance-mode field families.",
    negativeBoundary: "Zero and empty states remain authored shape only; list meanings and runtime enforcement are not decoded.",
  },
  {
    ordinal: 6,
    id: "entryCost",
    valueState: "present",
    classification: "proven-static",
    fields: ["staminaCost", "costMaterialId", "costMaterialNum", "firstCostMaterialNum"],
    mappingDisposition: "static-entry-cost-field-family-only",
    supportedStatement: "The exact row contains stamina and material entry-cost authoring fields.",
    negativeBoundary: "Actual deduction, shortage handling, refund, reset-cost relation, and persistence ownership are unproven.",
  },
  {
    ordinal: 7,
    id: "recordOrTargetTime",
    valueState: "present",
    classification: "proven-static",
    fields: ["fastBonusTime", "sonicBonusTime", "RecordLevelType"],
    mappingDisposition: "static-time-and-record-shape-only",
    supportedStatement: "The exact row contains time and record-level-type authoring fields.",
    negativeBoundary: "Units, comparison direction, scoring, qualification, and result persistence remain unverified.",
  },
  {
    ordinal: 8,
    id: "prerequisite",
    valueState: "present",
    classification: "proven-static",
    fields: ["preLevelID", "unlockPlayerLevel", "unlockStarNum", "PreMissionList", "PreMissionLink", "PreMissionLinkParams", "PreMissionLinkParamStr", "LockedText.Hash"],
    mappingDisposition: "static-predecessor-and-unlock-shape-only",
    supportedStatement: "The exact row contains predecessor, unlock, mission-link, and locked-text field families.",
    negativeBoundary: "Consumer semantics and unlock transaction ownership remain unverified; empty and zero states are not missing data.",
  },
  {
    ordinal: 9,
    id: "recommendedNext",
    valueState: "unresolved",
    classification: "unknown",
    fields: ["UnlockedLink", "UnlockedLinkParams", "UnlockedLinkParamStr", "UnlockedText.Hash", "preLevelID"],
    mappingDisposition: "no-direct-next-stage-identity",
    supportedStatement: "No direct next-stage identity field was established, while generic unlock-link and predecessor-shaped fields remain structurally present.",
    negativeBoundary: "Do not reverse prerequisites into recommendations or interpret generic links as stage identities.",
  },
  {
    ordinal: 10,
    id: "storyEntry",
    valueState: "unresolved",
    classification: "unknown",
    fields: ["StageEntryNameList", "luaFile", "displayDetail.Hash"],
    mappingDisposition: "entry-shaped-fields-without-consumer",
    supportedStatement: "Entry-name, script-reference, and display-detail fields are structurally present, but no consumer proves a story-entry contract.",
    negativeBoundary: "Names and scripts are not decoded or executed; this row cannot establish narrative entry ownership or ordering.",
  },
  {
    ordinal: 11,
    id: "storyExit",
    valueState: "unresolved",
    classification: "unknown",
    fields: ["luaFile", "loseDescList"],
    mappingDisposition: "no-exit-specific-field-or-consumer",
    supportedStatement: "No exit-specific field was established, while script and loss-description paths leave story-exit semantics unresolved.",
    negativeBoundary: "Do not reinterpret loss copy as story exit or claim absence inside Lua, linked tables, cutscenes, or wider-game behavior.",
  },
  {
    ordinal: 12,
    id: "challengeReference",
    valueState: "present",
    classification: "proven-static",
    fields: ["challengeList"],
    mappingDisposition: "static-challenge-reference-shape-only",
    supportedStatement: "The exact row contains a non-empty challenge-reference list.",
    negativeBoundary: "Referenced challenge rows, condition meanings, evaluators, and mastery/progress consumers were not inspected.",
  },
  {
    ordinal: 13,
    id: "resultReference",
    valueState: "present",
    classification: "proven-static",
    fields: ["highlightDisplayDropIdList", "firstVRDropList", "dropList", "avatarExpReward", "scoinReward", "maxScoinReward", "loseDescList"],
    mappingDisposition: "static-result-facing-reference-shape-only",
    supportedStatement: "The exact row contains result-facing reward, drop, progress-cap, and loss-description field families.",
    negativeBoundary: "Static references do not prove result UI, clear/fail, retry, reward grant, progress persistence, or exactly-once ownership.",
  },
];

assert(semanticSlots.length === 14, "semantic slot count must remain fourteen");
assert(semanticSlots.every((slot, index) => slot.ordinal === index), "semantic slot ordinals must be contiguous");
assert(new Set(semanticSlots.map((slot) => slot.id)).size === semanticSlots.length, "semantic slot IDs must be unique");

const readingRows = semanticSlots.map((slot) => {
  const fieldStates = slot.fields.map((fieldPath) => {
    const resolved = resolveField(target.row, fieldPath);
    assert(resolved.exists, `missing pinned field ${fieldPath}`);
    return { fieldPath, shapeState: shapeState(resolved.value) };
  });
  return {
    schema_version: 1,
    artifact_set_id: ARTIFACT_SET_ID,
    source_snapshot_id: SOURCE_SNAPSHOT_ID,
    source_ordinal: target.index + 1,
    row_key: `levelId=${TARGET_LEVEL_ID}`,
    source_row_sha256: targetRowProjection.sha256,
    semantic_slot_ordinal: slot.ordinal,
    semantic_slot_id: slot.id,
    foreign_field_paths: slot.fields.join("|"),
    field_shape_states: fieldStates.map((field) => `${field.fieldPath}=${field.shapeState}`).join("|"),
    field_path_count: fieldStates.length,
    combined_value_state: slot.valueState,
    foreign_classification: slot.classification,
    mapping_disposition: slot.mappingDisposition,
    source_value_copied: 0,
    negative_boundary_code: slot.classification === "unknown" ? "STATIC-SHAPE-CONSUMER-UNRESOLVED" : "STATIC-FIELD-FAMILY-NO-RUNTIME",
  };
});

const readingHeaders = [
  "schema_version",
  "artifact_set_id",
  "source_snapshot_id",
  "source_ordinal",
  "row_key",
  "source_row_sha256",
  "semantic_slot_ordinal",
  "semantic_slot_id",
  "foreign_field_paths",
  "field_shape_states",
  "field_path_count",
  "combined_value_state",
  "foreign_classification",
  "mapping_disposition",
  "source_value_copied",
  "negative_boundary_code",
];

const valueStateCounts = countBy(readingRows, (row) => row.combined_value_state);
const classificationCounts = countBy(readingRows, (row) => row.foreign_classification);
assert(canonicalize(valueStateCounts) === canonicalize({ present: 10, unresolved: 4 }), `semantic value-state counts changed: ${JSON.stringify(valueStateCounts)}`);
assert(canonicalize(classificationCounts) === canonicalize({ "proven-static": 10, unknown: 4 }), `semantic classification counts changed: ${JSON.stringify(classificationCounts)}`);
assert(readingRows.every((row) => row.source_value_copied === 0), "reading-link row copied a source value");

const summaryWithoutDigest = {
  schemaVersion: 1,
  reportId: "P1B-HI3-STAGE-SPINE-READFIRST-V1-SUMMARY-JSON",
  artifactSetId: ARTIFACT_SET_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  replacementContractId: CONTRACT_ID,
  generatorId: GENERATOR_ID,
  status: "exact-static-replacement-candidate-not-admitted",
  generatedAt: "2026-07-16T02:30:00+09:00",
  replacesHistoricalSourceIds: artifacts.map((artifact) => artifact.replacesHistoricalSourceId),
  authority: {
    sourceSnapshotId: SOURCE_SNAPSHOT_ID,
    upstream: "nairieberry/HonkaiImpactData",
    branch: "master",
    revision: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
    committedAt: "2021-04-10T22:17:18Z",
    snapshotDate: "2026-06-15",
    locale: "Global",
    licenseDisposition: "none-detected-review-needed",
    authorityInputId: "hi3-global-stage-data-main",
    officialCurrentShippedBehaviorClaimed: false,
  },
  selection: {
    selector: `typeof row.levelId === number && row.levelId === ${TARGET_LEVEL_ID}`,
    sourceRowCount: stageRows.length,
    duplicateLevelIdCount: 0,
    matchCount: targetMatches.length,
    zeroBasedIndex: target.index,
    oneBasedOrdinal: target.index + 1,
    rowKey: `levelId=${TARGET_LEVEL_ID}`,
    topLevelKeyCount: topLevelKeySet.length,
    canonicalRowSizeBytes: targetRowProjection.sizeBytes,
    canonicalRowSha256: targetRowProjection.sha256,
    topLevelKeySetSizeBytes: topLevelKeySetProjection.sizeBytes,
    topLevelKeySetSha256: topLevelKeySetProjection.sha256,
  },
  inputIntegrity,
  fieldShapeContract: {
    schemaVersion: 1,
    fieldCount: fieldShapeRows.length,
    canonicalSizeBytes: fieldShapeProjection.sizeBytes,
    canonicalSha256: fieldShapeProjection.sha256,
    stateCounts: fieldShapeCounts,
    fieldOrder: "top-level field path by ECMAScript default UTF-16 code-unit order",
    rows: fieldShapeRows,
    sourceValuesStored: false,
  },
  nestedObjectKeySets,
  readingLinksContract: {
    schemaVersion: 1,
    rowCount: readingRows.length,
    sourceRowCount: 1,
    semanticSlotCountPerSourceRow: semanticSlots.length,
    semanticSlotOrder: semanticSlots.map((slot) => slot.id),
    headerOrder: readingHeaders,
    valueStateCounts,
    classificationCounts,
    sourceValueCopiedCount: 0,
    rows: semanticSlots.map((slot) => ({
      ordinal: slot.ordinal,
      semanticSlotId: slot.id,
      foreignFieldPaths: slot.fields,
      valueState: slot.valueState,
      foreignClassification: slot.classification,
      mappingDisposition: slot.mappingDisposition,
      supportedStatement: slot.supportedStatement,
      negativeBoundary: slot.negativeBoundary,
      sourceValueCopied: false,
    })),
  },
  siblingHelperBoundary: {
    helperSourceIds: ["hi3-stage-summary-csv", "hi3-stage-samples-csv"],
    usedAsProducerInputs: false,
    formalProvenanceAdmissionState: "open",
    reason: "The two replay-authenticated helpers are sibling evidence. Keeping them outside this producer avoids circular provenance and preserves their separate formal-admission audit.",
    samplesNegativeBoundary: "A truncated helper sample does not independently identify levelId 10101 and cannot replace the exact StageData_Main row.",
  },
  outputArtifacts: artifacts.map((artifact) => ({
    sourceId: artifact.sourceId,
    artifactId: artifact.artifactId,
    replacesHistoricalSourceId: artifact.replacesHistoricalSourceId,
    format: artifact.format,
    expectedDataRowCount: artifact.artifactKey === "readingLinks" ? 14 : 1,
    admissionState: "candidate-not-admitted",
  })),
  dimensionBrawlComparisonBoundary: {
    alreadyPresent: [
      "typed playable-stage and route identity",
      "immutable route snapshot and truthful briefing join",
      "typed terminal actions and durable result receipt",
    ],
    immediateProductOrder: [
      "complete the separate result/progression join contract and implementation",
      "close Station count-one Add authoring readiness",
      "retain P1-C execution and P1-D mastery as later explicit owners",
    ],
    laterCandidates: [
      "typed entry restriction and cost authoring only after an accepted product owner exists",
      "target-time and challenge evaluation only behind P1-D",
      "loadout and reward truth only after separate authoritative joins exist",
    ],
    rejectedInference: [
      "A static luaFile reference is not stage execution, wave ownership, or cleanup.",
      "Reward- and drop-shaped fields do not prove grant, persistence, or exactly-once settlement.",
      "Numeric values, identifiers, list contents, story text, images, tuning, and economy values are not imported.",
    ],
  },
  sourceValuePolicy: {
    foreignIdentityMetadataAllowed: [`levelId=${TARGET_LEVEL_ID}`],
    copiedPayloadValues: [],
    sourceValueCopiedCount: 0,
    withheldFamilies: [
      "localized-hashes-and-text",
      "script-and-image-path-values",
      "list-and-reference-identifiers",
      "level-time-cost-reward-and-tuning-values",
    ],
  },
  acceptanceEffect: "none; these three new versioned replacement candidates do not reuse historical source IDs, enter inScopeSourceIds, populate claim mappings/crosswalkRows, or pass any LiveAcceptance gate.",
  negativeBoundaries: [
    "The package proves one exact retained StageData_Main row, its field shapes, and fourteen static semantic-slot dispositions only.",
    "It does not claim a newer HI3 data state, official current shipped behavior, runtime execution, evaluator semantics, cleanup, persistence, or product parity.",
    "It copies no authored string, path, identifier list, tuning, reward, economy, or narrative payload value.",
    "The two existing helper CSVs remain separate unadmitted siblings and cannot fill or reinterpret this row.",
  ],
  normalization: "UTF-8 without BOM; LF; exactly one final LF; JSON property/array order authored by generator; canonicalReportDigest is recursive sorted-key compact JSON with itself omitted; CSV has fixed headers, every cell double-quoted, comma delimiter, and RFC4180 escaping.",
};

const canonicalSummaryDigest = sha256(Buffer.from(canonicalize(summaryWithoutDigest), "utf8"));
const summary = { ...summaryWithoutDigest, canonicalReportDigest: canonicalSummaryDigest };
const summaryOutput = `${JSON.stringify(summary, null, 2)}\n`;
const readingOutput = encodeCsv(readingHeaders, readingRows);

const semanticRowsMarkdown = semanticSlots
  .map((slot) => `| ${slot.ordinal} | \`${slot.id}\` | ${slot.valueState} | ${slot.classification} | ${slot.fields.length} | \`${slot.mappingDisposition}\` |`)
  .join("\n");
const shapeCountsMarkdown = Object.keys(fieldShapeCounts)
  .sort()
  .map((state) => `- \`${state}\`: ${fieldShapeCounts[state]}`)
  .join("\n");
const markdownOutput = `# HI3 StageData stage spine - nairieberry 01d7afb v1

Status: **replacement candidate / exact static / not admitted**

## Contract

- Replacement contract: \`${CONTRACT_ID}\`
- Producer contract: \`${PRODUCER_CONTRACT_ID}\`
- Artifact set: \`${ARTIFACT_SET_ID}\`
- Source snapshot: \`${SOURCE_SNAPSHOT_ID}\`
- This is a new versioned semantic successor. It does not recreate or overwrite the three missing historical source identities.

## Authority boundary

- Upstream: \`nairieberry/HonkaiImpactData\`
- Revision: \`01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1\`
- Upstream commit time: \`2021-04-10T22:17:18Z\`
- Retained snapshot: \`2026-06-15\`
- Locale: Global
- License disposition: \`none-detected-review-needed\`
- Selected root: exactly one numeric \`levelId=${TARGET_LEVEL_ID}\`, source ordinal ${target.index + 1}
- Authored payload values copied into this package: **0**; the selected levelId is whitelisted identity metadata only.

This is exact retained-mirror static evidence, not a claim about official current shipped behavior or a newer HI3 data state.

## Exact structural projection

- StageData_Main rows: ${stageRows.length}
- Duplicate levelId values: 0
- Target top-level fields: ${fieldShapeRows.length}
- Canonical target-row digest: \`${targetRowProjection.sha256}\`
- Top-level key-set digest: \`${topLevelKeySetProjection.sha256}\`
- Field-shape digest: \`${fieldShapeProjection.sha256}\`

${shapeCountsMarkdown}

No field value is stored in the field-shape ledger. Nested catalog objects are checked only for the single \`Hash\` key.

## Semantic reading links

| Ordinal | Slot | State | Classification | Field paths | Disposition |
|---:|---|---|---|---:|---|
${semanticRowsMarkdown}

The CSV has exactly 14 rows in this order. It stores field paths and shape states only; source values, localized content, script paths, image paths, list contents, identifiers, tuning, time, costs, and rewards are withheld.

## Generated artifacts

${artifacts.map((artifact) => `- \`${artifact.sourceId}\` - ${artifact.format}; a new versioned successor for \`${artifact.replacesHistoricalSourceId}\``).join("\n")}

## Sibling helper boundary

\`hi3-stage-summary-csv\` and \`hi3-stage-samples-csv\` are not producer inputs. They remain byte-exact replay-authenticated sibling evidence with formal provenance/admission still open. A truncated sample cannot independently identify \`levelId=${TARGET_LEVEL_ID}\`.

## DimensionBrawl comparison

DimensionBrawl already has typed playable-stage identity, an immutable route snapshot, truthful briefing joins, typed terminal actions, and a durable result receipt. The next product order remains result/progression joins, Station count-one Add authoring, and then explicit P1-C/P1-D owners.

HI3's restriction-, cost-, time-, challenge-, loadout-, and result-shaped fields are later comparison candidates only. They do not authorize importing foreign numbers, lists, rewards, economy, story, images, scripts, or balancing.

## Negative boundary

A static \`luaFile\` field is not execution. Reward/drop fields are not grant or persistence. Hashed catalog fields are not localized presentation. This package proves neither runtime consumers nor official shipped behavior and has zero effect on the eleven-source atomic gate.

## Acceptance effect

None. These three artifacts remain outside \`inScopeSourceIds\`; admitted supporting sources remain 0/9, live rows 0/5, and live crosswalk cells 0/70.

Canonical report digest: \`${canonicalSummaryDigest}\`
`;

const outputs = {
  readFirstMarkdown: markdownOutput.replaceAll("\r\n", "\n"),
  readFirstSummary: summaryOutput.replaceAll("\r\n", "\n"),
  readingLinks: readingOutput.replaceAll("\r\n", "\n"),
};

for (const [outputKey, output] of Object.entries(outputs)) {
  assert(output.endsWith("\n"), `${outputKey} must end with one LF`);
  assert(!output.endsWith("\n\n"), `${outputKey} has more than one trailing LF`);
  assert(!output.includes("\r"), `${outputKey} contains CR`);
}

function collectStrings(value, results = []) {
  if (typeof value === "string") {
    if (value.length >= 6) results.push(value);
  } else if (Array.isArray(value)) {
    for (const item of value) collectStrings(item, results);
  } else if (value && typeof value === "object") {
    for (const item of Object.values(value)) collectStrings(item, results);
  }
  return results;
}

const prohibitedPayloadStrings = [...new Set(collectStrings(target.row))];
assert(prohibitedPayloadStrings.length > 0, "target row contains no strings for leakage guard");
for (const [outputKey, output] of Object.entries(outputs)) {
  for (const sourceValue of prohibitedPayloadStrings) {
    assert(!output.includes(sourceValue), `${outputKey} copied a selected authored string payload`);
  }
}

if (process.argv.includes("--verify")) {
  for (const artifact of artifacts) {
    const path = outputPaths[artifact.artifactKey];
    assert(existsSync(path), `missing output: ${path}`);
    const actual = readFileSync(path, "utf8");
    assert(actual === outputs[artifact.artifactKey], `${artifact.artifactKey} bytes differ from reconstruction`);
  }
  console.log(`PASS ${CONTRACT_ID}`);
} else {
  for (const artifact of artifacts) {
    writeFileSync(outputPaths[artifact.artifactKey], outputs[artifact.artifactKey], "utf8");
  }
  console.log(`WROTE ${CONTRACT_ID}`);
}

for (const artifact of artifacts) {
  const encoded = Buffer.from(outputs[artifact.artifactKey], "utf8");
  console.log(`${artifact.sourceId} sizeBytes=${encoded.length} sha256=${sha256(encoded)}`);
}
console.log(`canonicalSummaryDigest=${canonicalSummaryDigest}`);
console.log(`fieldShapeDigest=${fieldShapeProjection.sha256}`);
console.log("sourceValueCopied=0");
console.log("admissionEffect=none");
