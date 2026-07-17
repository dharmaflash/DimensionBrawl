import { createHash } from "node:crypto";
import { readFileSync, writeFileSync, existsSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-V1-GENERATOR-01";
const CONTRACT_ID = "P1B-PGR-STAGE-SPINE-REPLACEMENT-01";
const PRODUCER_CONTRACT_ID = "PGR-GUIDEFIGHT-STAGE-SPINE-PRODUCER-01";
const ARTIFACT_SET_ID = "PGR-GUIDEFIGHT-STAGE-SPINE-ALT3RI-856A0E45-V1";
const SOURCE_SNAPSHOT_ID = "pgr-alt3ri-856a0e45-en-guidefight-stage-v1";
const here = dirname(fileURLToPath(import.meta.url));
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";
const snapshotRoot = join(
  arkRoot,
  "games",
  "punishing-gray-raven",
  "raw",
  "alt3ri-pgr-data",
  "2026-06-14",
);
const localeRoot = join(snapshotRoot, "files", "extracted_repo", "PGR_Data-master");

const outputPaths = {
  readFirstMarkdown: join(here, "P1B_PGR_STAGE_SPINE_READFIRST_V1.md"),
  readFirstSummary: join(here, "P1B_PGR_STAGE_SPINE_READFIRST_V1_SUMMARY.json"),
  labelContext: join(here, "P1B_PGR_GUIDEFIGHT_STAGE_LABEL_CONTEXT_V1.csv"),
  readingLinks: join(here, "P1B_PGR_GUIDEFIGHT_STAGE_READING_LINKS_V1.csv"),
};

const artifacts = [
  {
    artifactKey: "readFirstMarkdown",
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-readfirst-md",
    artifactId: "P1B-PGR-STAGE-SPINE-READFIRST-V1-MD",
    replacesHistoricalSourceId: "pgr-readfirst-md",
    format: "markdown",
  },
  {
    artifactKey: "readFirstSummary",
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-summary-json",
    artifactId: "P1B-PGR-STAGE-SPINE-READFIRST-V1-SUMMARY-JSON",
    replacesHistoricalSourceId: "pgr-readfirst-summary-json",
    format: "json",
  },
  {
    artifactKey: "labelContext",
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-label-context-csv",
    artifactId: "P1B-PGR-GUIDEFIGHT-STAGE-LABEL-CONTEXT-V1-CSV",
    replacesHistoricalSourceId: "pgr-guidefight-label-csv",
    format: "csv",
  },
  {
    artifactKey: "readingLinks",
    sourceId: "pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-reading-links-csv",
    artifactId: "P1B-PGR-GUIDEFIGHT-STAGE-READING-LINKS-V1-CSV",
    replacesHistoricalSourceId: "pgr-guidefight-links-csv",
    format: "csv",
  },
];

const inputSpecs = [
  {
    inputId: "pgr-source-record",
    locale: "provenance",
    role: "source-record",
    relativePath: "source-record.md",
    sizeBytes: 1118,
    sha256: "23cecc493fe4e69f59f73520e7da694c22ac76fc2283deb88070d165c37725ee",
  },
  {
    inputId: "pgr-snapshot-manifest",
    locale: "provenance",
    role: "snapshot-manifest",
    relativePath: "manifest.yml",
    sizeBytes: 1645,
    sha256: "00f535d4bb159a0f9a43a824bda3e9fad721ae3074717b01d0b68c4f1e86400d",
  },
  {
    inputId: "pgr-file-manifest",
    locale: "provenance",
    role: "file-manifest",
    relativePath: "file-manifest.csv",
    sizeBytes: 19913116,
    sha256: "f3909c0d8b24b9e2770cead82f86418a52536e6d2ab1602f43eae862bdc55115",
  },
  {
    inputId: "pgr-en-course-stage",
    locale: "EN",
    role: "course-stage-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/course/CourseStage.json",
    sizeBytes: 4952,
    sha256: "c0919188664f4582d78aa9dd4eaa00c7039650c2b619f58aff5aa8d487532bc3",
  },
  {
    inputId: "pgr-en-course-chapter",
    locale: "EN",
    role: "course-chapter-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/course/CourseChapter.json",
    sizeBytes: 3643,
    sha256: "bfd83777a16b1e14ce3b392523013b599bdd309e1ac4466c2a3db9723f735c35",
  },
  {
    inputId: "pgr-en-course-stage-show-type",
    locale: "EN",
    role: "course-presentation-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/client/fuben/course/CourseStageShowType.json",
    sizeBytes: 706,
    sha256: "5f2066cfb2bb756a5f6b599e98bfcbcea8770791997e7d5a409064c0116ce9d4",
  },
  {
    inputId: "pgr-en-practice-chapter",
    locale: "EN",
    role: "practice-chapter-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/practice/PracticeChapter.json",
    sizeBytes: 2415,
    sha256: "e213fcac88ca56ba073195b45ca1d8bf9f3bb215e401929991f94f099376aa4c",
  },
  {
    inputId: "pgr-en-practice-group",
    locale: "EN",
    role: "practice-group-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/practice/PracticeGroup.json",
    sizeBytes: 12767,
    sha256: "d38089295be9700ee8565a05e062a541eacb1bfcd6feee31722972ce8ac3d887",
  },
  {
    inputId: "pgr-en-practice-skill-details",
    locale: "EN",
    role: "practice-skill-presentation-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/client/fuben/practice/PracticeSkillDetails.json",
    sizeBytes: 46259,
    sha256: "63efabc9ac56ebe69bde10567390174e166d9dfa70cb7045c9f792c084e8b7c4",
  },
  {
    inputId: "pgr-en-teaching-activity",
    locale: "EN",
    role: "teaching-activity-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/teaching/TeachingActivity.json",
    sizeBytes: 72445,
    sha256: "9e7f2b476035b6c417443924f50238fec5d54d84e621dfe73bc7130bdbe8de4a",
  },
  {
    inputId: "pgr-en-teaching-robot",
    locale: "EN",
    role: "teaching-loadout-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/teaching/TeachingRobot.json",
    sizeBytes: 46467,
    sha256: "6d43c29629c6573ca1e95eb53e97eec327527c9b32f50769e6dacae062289c3b",
  },
  {
    inputId: "pgr-en-guide-fight",
    locale: "EN",
    role: "authoritative-four-row-guide-selection",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/guide/GuideFight.json",
    sizeBytes: 595,
    sha256: "62c184d70d88a3daf377ce6a2558c6e2cb192c4dbefbae3f02b4f549b6b46e7d",
  },
  {
    inputId: "pgr-en-stage",
    locale: "EN",
    role: "authoritative-stage-join-and-label-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/Stage.json",
    sizeBytes: 29637115,
    sha256: "7d553ada4ac1cd40e77054be70263260f7b2b2dd15948dc120e7ca806b26f940",
  },
  {
    inputId: "pgr-zh-guide-fight-compare-only",
    locale: "ZH",
    role: "compare-only-guide-byte-identity",
    relativePath: "files/extracted_repo/PGR_Data-master/ZH/bytes/share/guide/GuideFight.json",
    sizeBytes: 595,
    sha256: "62c184d70d88a3daf377ce6a2558c6e2cb192c4dbefbae3f02b4f549b6b46e7d",
  },
  {
    inputId: "pgr-zh-stage-compare-only",
    locale: "ZH",
    role: "compare-only-stage-join-and-label-shape",
    relativePath: "files/extracted_repo/PGR_Data-master/ZH/bytes/share/fuben/Stage.json",
    sizeBytes: 30511512,
    sha256: "ca3ad74480538148e7bc9a1a129569437e45bc83c670c0397d8062133dc6ee3a",
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

function valueType(value) {
  if (value === null) return "null";
  if (Array.isArray(value)) return "array";
  return typeof value;
}

function csvEscape(value) {
  const text = String(value ?? "");
  return `"${text.replaceAll('"', '""')}"`;
}

function encodeCsv(headers, rows) {
  return `${headers.map(csvEscape).join(",")}\n${rows.map((row) => headers.map((header) => csvEscape(row[header])).join(",")).join("\n")}\n`;
}

function observedState(value) {
  if (value === null) return "explicit-null";
  if (typeof value === "string" && value.length === 0) return "present-empty";
  if (Array.isArray(value) && value.length === 0) return "present-empty";
  if (typeof value === "object" && value !== null && !Array.isArray(value) && Object.keys(value).length === 0) return "present-empty";
  return "present-nonempty";
}

function fieldFamilyState(row, paths, exactIdentity = false) {
  if (paths.length === 0) return "not-mapped";
  for (const path of paths) assert(Object.hasOwn(row, path), `missing pinned field ${path}`);
  if (exactIdentity) return "exact-identity-present";
  return paths.every((path) => row[path] === null) ? "all-explicit-null" : "one-or-more-present";
}

const inputIntegrity = [];
const parsedInputs = new Map();
for (const spec of inputSpecs) {
  const absolutePath = join(snapshotRoot, ...spec.relativePath.split("/"));
  assert(resolve(absolutePath).startsWith(resolve(snapshotRoot)), `input escapes snapshot root: ${spec.inputId}`);
  const bytes = readFileSync(absolutePath);
  const actualSha256 = sha256(bytes);
  assert(bytes.length === spec.sizeBytes, `${spec.inputId} size changed: ${bytes.length}`);
  assert(actualSha256 === spec.sha256, `${spec.inputId} SHA-256 changed: ${actualSha256}`);
  inputIntegrity.push({ ...spec });
  if (spec.relativePath.endsWith(".json")) {
    const parsed = JSON.parse(bytes.toString("utf8"));
    assert(Array.isArray(parsed), `${spec.inputId} root must be an array`);
    parsedInputs.set(spec.inputId, parsed);
  }
}

const pgrEnGuide = parsedInputs.get("pgr-en-guide-fight");
const pgrZhGuide = parsedInputs.get("pgr-zh-guide-fight-compare-only");
const pgrEnStage = parsedInputs.get("pgr-en-stage");
const pgrZhStage = parsedInputs.get("pgr-zh-stage-compare-only");
assert(pgrEnGuide.length === 4, "EN GuideFight row count must remain four");
assert(canonicalize(pgrEnGuide) === canonicalize(pgrZhGuide), "ZH GuideFight compare sibling is not byte-equivalent in meaning");
assert(new Set(pgrEnGuide.map((row) => String(row.Id))).size === 4, "GuideFight IDs must be unique");
assert(new Set(pgrEnStage.map((row) => String(row.StageId))).size === pgrEnStage.length, "EN StageId values must be unique");
assert(new Set(pgrZhStage.map((row) => String(row.StageId))).size === pgrZhStage.length, "ZH StageId values must be unique");

const expectedPairs = [
  [100001, 10010001],
  [100002, 10010002],
  [100003, 10010003],
  [100004, 10010005],
];
for (let index = 0; index < expectedPairs.length; index += 1) {
  const [expectedId, expectedStageId] = expectedPairs[index];
  assert(pgrEnGuide[index].Id === expectedId, `GuideFight Id drift at ordinal ${index + 1}`);
  assert(pgrEnGuide[index].StageId === expectedStageId, `GuideFight StageId drift at ordinal ${index + 1}`);
}

const enStageById = new Map(pgrEnStage.map((row, index) => [String(row.StageId), { row, index }]));
const zhStageById = new Map(pgrZhStage.map((row, index) => [String(row.StageId), { row, index }]));
const labelFields = ["Name", "Description", "StarDesc"];
const selectedRows = pgrEnGuide.map((guideRow, index) => {
  const enMatch = enStageById.get(String(guideRow.StageId));
  const zhMatch = zhStageById.get(String(guideRow.StageId));
  assert(enMatch, `missing EN Stage join for ${guideRow.StageId}`);
  assert(zhMatch, `missing ZH compare Stage join for ${guideRow.StageId}`);
  const enLabelProjection = Object.fromEntries(labelFields.map((field) => [field, enMatch.row[field]]));
  const zhLabelProjection = Object.fromEntries(labelFields.map((field) => [field, zhMatch.row[field]]));
  for (const field of labelFields) {
    assert(typeof enMatch.row[field] === "string", `${field} must be a string in EN Stage ${guideRow.StageId}`);
    assert(typeof zhMatch.row[field] === "string", `${field} must be a string in ZH Stage ${guideRow.StageId}`);
  }
  const guideRowCanonical = canonicalHash(guideRow);
  const enStageRowCanonical = canonicalHash(enMatch.row);
  const zhStageRowCanonical = canonicalHash(zhMatch.row);
  const enLabelHash = canonicalHash(enLabelProjection);
  const zhLabelHash = canonicalHash(zhLabelProjection);
  return {
    sourceOrdinal: index + 1,
    guideFightRowKey: `Id=${guideRow.Id}`,
    stageIdentityKey: `StageId=${guideRow.StageId}`,
    enStageOrdinal: enMatch.index + 1,
    zhCompareStageOrdinal: zhMatch.index + 1,
    enJoinMultiplicity: 1,
    zhCompareJoinMultiplicity: 1,
    guideRowCanonicalSizeBytes: guideRowCanonical.sizeBytes,
    guideRowCanonicalSha256: guideRowCanonical.sha256,
    enStageRowCanonicalSizeBytes: enStageRowCanonical.sizeBytes,
    enStageRowCanonicalSha256: enStageRowCanonical.sha256,
    zhStageRowCanonicalSizeBytes: zhStageRowCanonical.sizeBytes,
    zhStageRowCanonicalSha256: zhStageRowCanonical.sha256,
    enStageKeyCount: Object.keys(enMatch.row).length,
    enStageKeySetSha256: canonicalHash(Object.keys(enMatch.row).sort()).sha256,
    zhStageKeyCount: Object.keys(zhMatch.row).length,
    zhStageKeySetSha256: canonicalHash(Object.keys(zhMatch.row).sort()).sha256,
    labelFieldSet: labelFields,
    labelFieldTypes: Object.fromEntries(labelFields.map((field) => [field, valueType(enMatch.row[field])])),
    enNonEmptyLabelFieldCount: labelFields.filter((field) => enMatch.row[field].length > 0).length,
    zhNonEmptyLabelFieldCount: labelFields.filter((field) => zhMatch.row[field].length > 0).length,
    enLabelProjectionSha256: enLabelHash.sha256,
    zhLabelProjectionSha256: zhLabelHash.sha256,
    enZhLabelProjectionEqual: enLabelHash.sha256 === zhLabelHash.sha256,
    loadoutState: guideRow.NpcId === null && guideRow.Weapon === null ? "exact-row-null" : "present-withheld",
    recordTimeState: guideRow.DefaultRecordTime === null ? "exact-row-null" : "present-withheld",
    predecessorState: enMatch.row.PreStageId === null ? "exact-row-null" : "present-withheld",
    successorState: enMatch.row.NextStageId === null ? "exact-row-null" : "present-withheld",
    consumerTraceState: "not-traced-static-join-only",
    evidenceClass: "exact-static",
    sourceValueCopied: false,
    guideRow,
    zhGuideRow: pgrZhGuide[index],
    enStageRow: enMatch.row,
    zhStageRow: zhMatch.row,
  };
});

const tableShapeInputIds = inputSpecs
  .filter((spec) => spec.locale === "EN" && spec.relativePath.endsWith(".json"))
  .map((spec) => spec.inputId);
const tableShapes = tableShapeInputIds.map((inputId) => {
  const spec = inputSpecs.find((candidate) => candidate.inputId === inputId);
  const rows = parsedInputs.get(inputId);
  const unionKeys = [...new Set(rows.flatMap((row) => Object.keys(row)))].sort();
  const rowKeySetDigests = new Set(rows.map((row) => canonicalHash(Object.keys(row).sort()).sha256));
  return {
    inputId,
    role: spec.role,
    relativePath: spec.relativePath,
    rowCount: rows.length,
    unionKeyCount: unionKeys.length,
    unionKeySetSha256: canonicalHash(unionKeys).sha256,
    distinctRowKeySetCount: rowKeySetDigests.size,
  };
});

const labelFieldLedger = [
  { ordinal: 0, fieldPath: "Stage.Name", field: "Name", semanticRole: "stage-label" },
  { ordinal: 1, fieldPath: "Stage.Description", field: "Description", semanticRole: "stage-description" },
  { ordinal: 2, fieldPath: "Stage.RecommandLevel", field: "RecommandLevel", semanticRole: "recommended-level-shape" },
  { ordinal: 3, fieldPath: "Stage.RequireLevel", field: "RequireLevel", semanticRole: "required-level-shape" },
  { ordinal: 4, fieldPath: "Stage.StarDesc", field: "StarDesc", semanticRole: "challenge-copy-shape" },
];
const labelCsvHeaders = [
  "schema_version",
  "artifact_set_id",
  "source_snapshot_id",
  "guide_fight_source_ordinal",
  "guide_fight_row_key",
  "stage_source_ordinal",
  "stage_row_key",
  "context_field_ordinal",
  "field_path",
  "semantic_role",
  "en_observed_state",
  "en_json_type",
  "zh_observed_state",
  "zh_json_type",
  "en_stage_row_sha256",
  "zh_compare_stage_row_sha256",
  "en_zh_field_value_equal",
  "source_value_copied",
  "negative_boundary_code",
];
const labelCsvRows = selectedRows.flatMap((row) => labelFieldLedger.map((field) => {
  assert(Object.hasOwn(row.enStageRow, field.field), `missing EN label-context field ${field.field}`);
  assert(Object.hasOwn(row.zhStageRow, field.field), `missing ZH label-context field ${field.field}`);
  const enValueHash = canonicalHash(row.enStageRow[field.field]).sha256;
  const zhValueHash = canonicalHash(row.zhStageRow[field.field]).sha256;
  return {
    schema_version: 1,
    artifact_set_id: ARTIFACT_SET_ID,
    source_snapshot_id: SOURCE_SNAPSHOT_ID,
    guide_fight_source_ordinal: row.sourceOrdinal,
    guide_fight_row_key: row.guideFightRowKey,
    stage_source_ordinal: row.enStageOrdinal,
    stage_row_key: row.stageIdentityKey,
    context_field_ordinal: field.ordinal,
    field_path: field.fieldPath,
    semantic_role: field.semanticRole,
    en_observed_state: observedState(row.enStageRow[field.field]),
    en_json_type: valueType(row.enStageRow[field.field]),
    zh_observed_state: observedState(row.zhStageRow[field.field]),
    zh_json_type: valueType(row.zhStageRow[field.field]),
    en_stage_row_sha256: row.enStageRowCanonicalSha256,
    zh_compare_stage_row_sha256: row.zhStageRowCanonicalSha256,
    en_zh_field_value_equal: enValueHash === zhValueHash ? 1 : 0,
    source_value_copied: 0,
    negative_boundary_code: "STATIC-SHAPE-NO-PAYLOAD-NO-RUNTIME",
  };
}));

const semanticSlotLedger = [
  { ordinal: 0, id: "logicalStageId", guideFields: ["Id", "StageId"], stageFields: ["StageId"], mappingDisposition: "identity-only" },
  { ordinal: 1, id: "physicalSceneOrScript", guideFields: [], stageFields: [], mappingDisposition: "consumer-unresolved" },
  { ordinal: 2, id: "briefingAndCatalog", guideFields: [], stageFields: ["Name", "Description"], mappingDisposition: "static-field-family-only" },
  { ordinal: 3, id: "recommendedPowerOrLevel", guideFields: [], stageFields: ["RecommandLevel", "RequireLevel"], mappingDisposition: "static-field-family-only" },
  { ordinal: 4, id: "loadout", guideFields: ["NpcId", "Weapon"], stageFields: ["RobotId"], mappingDisposition: "static-field-family-only" },
  { ordinal: 5, id: "restrictions", guideFields: [], stageFields: ["CharacterLimitType", "CareerSuggestType", "AISuggestType", "LimitBuffId", "NeedJobType", "DisableJoystick", "HideAction"], mappingDisposition: "static-field-family-only" },
  { ordinal: 6, id: "entryCost", guideFields: [], stageFields: ["RequireActionPoint", "FirstRequireActionPoint", "FinishRequireActionPoint"], mappingDisposition: "static-field-family-only" },
  { ordinal: 7, id: "recordOrTargetTime", guideFields: ["DefaultRecordTime"], stageFields: ["StandardUseTimeSec", "PassTimeLimit"], mappingDisposition: "static-field-family-only" },
  { ordinal: 8, id: "prerequisite", guideFields: [], stageFields: ["PreStageId", "RequireLevel", "BeginConditions"], mappingDisposition: "static-field-family-only" },
  { ordinal: 9, id: "recommendedNext", guideFields: [], stageFields: ["NextStageId"], mappingDisposition: "static-field-family-only" },
  { ordinal: 10, id: "storyEntry", guideFields: [], stageFields: ["BeginStoryIds", "BeginConditions", "KeepPlayingStory"], mappingDisposition: "static-field-family-only" },
  { ordinal: 11, id: "storyExit", guideFields: [], stageFields: ["EndStoryIds", "EndConditions"], mappingDisposition: "static-field-family-only" },
  { ordinal: 12, id: "challengeReference", guideFields: [], stageFields: ["StarDesc", "StarRewardId", "SuggestedConditionId", "ForceConditionId"], mappingDisposition: "static-field-family-only" },
  { ordinal: 13, id: "resultReference", guideFields: [], stageFields: ["FinishRewardShow", "FirstRewardShow", "FinishDropId", "FirstRewardId", "StarRewardId"], mappingDisposition: "static-field-family-only" },
];
const linkCsvHeaders = [
  "schema_version",
  "artifact_set_id",
  "source_snapshot_id",
  "guide_fight_source_ordinal",
  "guide_fight_row_key",
  "stage_source_ordinal",
  "stage_row_key",
  "semantic_slot_ordinal",
  "semantic_slot_id",
  "guide_fight_field_paths",
  "stage_field_paths",
  "guide_fight_observed_state",
  "stage_observed_state",
  "combined_value_state",
  "foreign_classification",
  "mapping_disposition",
  "negative_boundary_code",
  "source_value_copied",
];
const linkCsvRows = selectedRows.flatMap((row) => semanticSlotLedger.map((slot) => {
  const exactIdentity = slot.id === "logicalStageId";
  const guideState = fieldFamilyState(row.guideRow, slot.guideFields, exactIdentity);
  const stageState = fieldFamilyState(row.enStageRow, slot.stageFields, exactIdentity);
  const isUnresolved = slot.id === "physicalSceneOrScript";
  const hasPresent = [guideState, stageState].some((state) => state === "one-or-more-present" || state === "exact-identity-present");
  return {
    schema_version: 1,
    artifact_set_id: ARTIFACT_SET_ID,
    source_snapshot_id: SOURCE_SNAPSHOT_ID,
    guide_fight_source_ordinal: row.sourceOrdinal,
    guide_fight_row_key: row.guideFightRowKey,
    stage_source_ordinal: row.enStageOrdinal,
    stage_row_key: row.stageIdentityKey,
    semantic_slot_ordinal: slot.ordinal,
    semantic_slot_id: slot.id,
    guide_fight_field_paths: slot.guideFields.join("|"),
    stage_field_paths: slot.stageFields.join("|"),
    guide_fight_observed_state: guideState,
    stage_observed_state: stageState,
    combined_value_state: isUnresolved ? "unresolved" : (hasPresent ? "present" : "absent"),
    foreign_classification: isUnresolved ? "unknown" : "proven-static",
    mapping_disposition: slot.mappingDisposition,
    negative_boundary_code: isUnresolved ? "NO-PHYSICAL-CONSUMER-IN-BOUNDARY" : "STATIC-FIELD-FAMILY-NO-RUNTIME",
    source_value_copied: 0,
  };
}));

assert(labelCsvRows.length === 20, `label-context row count must be 20, got ${labelCsvRows.length}`);
assert(labelCsvRows.filter((row) => row.en_observed_state === "present-nonempty").length === 20, "all EN label-context cells must remain present-nonempty");
assert(labelCsvRows.filter((row) => row.zh_observed_state === "present-nonempty").length === 20, "all ZH label-context cells must remain present-nonempty");
assert(labelCsvRows.filter((row) => row.en_json_type === "string").length === 12 && labelCsvRows.filter((row) => row.en_json_type === "number").length === 8, "EN label-context type counts changed");
assert(labelCsvRows.filter((row) => row.zh_json_type === "string").length === 12 && labelCsvRows.filter((row) => row.zh_json_type === "number").length === 8, "ZH label-context type counts changed");
assert(labelCsvRows.filter((row) => row.en_zh_field_value_equal === 1).length === 8 && labelCsvRows.filter((row) => row.en_zh_field_value_equal === 0).length === 12, "EN/ZH label-context equality counts changed");
assert(linkCsvRows.length === 56, `reading-links row count must be 56, got ${linkCsvRows.length}`);
const linkStateCounts = Object.fromEntries(["present", "absent", "unresolved"].map((state) => [state, linkCsvRows.filter((row) => row.combined_value_state === state).length]));
assert(linkStateCounts.present === 32 && linkStateCounts.absent === 20 && linkStateCounts.unresolved === 4, `unexpected reading-link state counts: ${JSON.stringify(linkStateCounts)}`);
assert(linkCsvRows.filter((row) => row.foreign_classification === "proven-static").length === 52, "proven-static link count must be 52");
assert(linkCsvRows.filter((row) => row.foreign_classification === "unknown").length === 4, "unknown link count must be 4");
const optionalGuideTupleStates = selectedRows.map((row) => [row.guideRow.NpcId, row.guideRow.Weapon, row.guideRow.DefaultRecordTime].map((value) => value === null));
assert(optionalGuideTupleStates.filter((states) => states.every(Boolean)).length === 3, "GuideFight optional tuple all-null count must be three");
assert(optionalGuideTupleStates.filter((states) => states.every((state) => !state)).length === 1, "GuideFight optional tuple all-present count must be one");
assert(optionalGuideTupleStates.filter((states) => states.some(Boolean) && !states.every(Boolean)).length === 0, "GuideFight optional tuple must not be partial");

const summaryWithoutDigest = {
  schemaVersion: 1,
  reportId: "P1B-PGR-STAGE-SPINE-READFIRST-V1-SUMMARY-JSON",
  artifactSetId: ARTIFACT_SET_ID,
  producerContractId: PRODUCER_CONTRACT_ID,
  replacementContractId: CONTRACT_ID,
  generatorId: GENERATOR_ID,
  status: "exact-static-replacement-candidate-not-admitted",
  generatedAt: "2026-07-16T00:25:00+09:00",
  replacesHistoricalSourceIds: artifacts.map((artifact) => artifact.replacesHistoricalSourceId),
  authority: {
    sourceSnapshotId: SOURCE_SNAPSHOT_ID,
    upstream: "alt3ri/PGR_Data",
    revision: "856a0e4534d0854fa440040e961b74a97ba732e2",
    snapshotDate: "2026-06-14",
    locale: "EN",
    licenseDisposition: "unknown-review-needed",
    guideFightInputId: "pgr-en-guide-fight",
    stageInputId: "pgr-en-stage",
  },
  comparison: {
    locale: "ZH",
    guideFightInputId: "pgr-zh-guide-fight-compare-only",
    stageInputId: "pgr-zh-stage-compare-only",
    guideFightByteIdentical: inputSpecs.find((spec) => spec.inputId === "pgr-en-guide-fight").sha256 === inputSpecs.find((spec) => spec.inputId === "pgr-zh-guide-fight-compare-only").sha256,
    stagePayloadByteIdentical: inputSpecs.find((spec) => spec.inputId === "pgr-en-stage").sha256 === inputSpecs.find((spec) => spec.inputId === "pgr-zh-stage-compare-only").sha256,
    selectedIdentityAndOrdinalParity: selectedRows.every((row) => row.enStageOrdinal === row.zhCompareStageOrdinal),
    snapshotsUnioned: false,
    zhValuesCopied: false,
  },
  selection: {
    requiredGuideFightIds: expectedPairs.map(([id]) => id),
    requiredPairs: expectedPairs.map(([id, stageId]) => ({ guideFightId: id, stageId })),
    sort: "GuideFight.Id numeric ascending",
    guideFightRowCount: selectedRows.length,
    stageJoinCount: selectedRows.length,
    missingCount: 0,
    duplicateCount: 0,
    multiMatchCount: 0,
    enSelectionCanonicalSizeBytes: canonicalHash(pgrEnGuide).sizeBytes,
    enSelectionCanonicalDigest: canonicalHash(pgrEnGuide).sha256,
  },
  exactRows: selectedRows.map((row) => ({
    guideFightSourceOrdinal: row.sourceOrdinal,
    guideFightRowKey: row.guideFightRowKey,
    guideFightCanonicalRowSizeBytes: row.guideRowCanonicalSizeBytes,
    guideFightCanonicalRowSha256: row.guideRowCanonicalSha256,
    stageSourceOrdinal: row.enStageOrdinal,
    stageRowKey: row.stageIdentityKey,
    stageCanonicalRowSizeBytes: row.enStageRowCanonicalSizeBytes,
    stageCanonicalRowSha256: row.enStageRowCanonicalSha256,
    zhCompareStageSourceOrdinal: row.zhCompareStageOrdinal,
    zhCompareStageCanonicalRowSizeBytes: row.zhStageRowCanonicalSizeBytes,
    zhCompareStageCanonicalRowSha256: row.zhStageRowCanonicalSha256,
    joinCardinality: 1,
    sourceValueCopied: false,
  })),
  inputIntegrity,
  tableShapes,
  labelContextContract: {
    schemaVersion: 1,
    rowCount: labelCsvRows.length,
    sourceRowCount: selectedRows.length,
    fieldsPerSourceRow: labelFieldLedger.length,
    fieldOrder: labelFieldLedger.map((field) => field.fieldPath),
    headerOrder: labelCsvHeaders,
    payloadValuesWithheld: true,
    sourceValueCopiedCount: 0,
  },
  readingLinksContract: {
    schemaVersion: 1,
    rowCount: linkCsvRows.length,
    sourceRowCount: selectedRows.length,
    semanticSlotCountPerSourceRow: semanticSlotLedger.length,
    semanticSlotOrder: semanticSlotLedger.map((slot) => slot.id),
    headerOrder: linkCsvHeaders,
    stateCounts: linkStateCounts,
    classificationCounts: { provenStatic: 52, unknown: 4 },
    sourceValueCopiedCount: 0,
  },
  outputArtifacts: artifacts.map((artifact) => ({
    sourceId: artifact.sourceId,
    artifactId: artifact.artifactId,
    replacesHistoricalSourceId: artifact.replacesHistoricalSourceId,
    format: artifact.format,
    expectedDataRowCount: artifact.artifactKey === "labelContext" ? 20 : artifact.artifactKey === "readingLinks" ? 56 : 1,
    admissionState: "candidate-not-admitted",
  })),
  dimensionBrawlComparisonBoundary: {
    alreadyPresent: [
      "typed playable-stage and route identity",
      "immutable route snapshot and truthful briefing join",
      "typed terminal actions and durable result receipt",
    ],
    nextProductGates: [
      "implement the jointly frozen result/progression join",
      "close the separate Station count-one Add authoring gate",
      "retain P1-C execution and P1-D mastery as later explicit owners",
    ],
    laterCandidates: [
      "course/practice/teaching catalog separation after the P1-B spine is complete",
      "typed target-time/mastery evaluation only under P1-D",
      "loadout truth only after a distinct accepted loadout authority exists",
    ],
    rejectedInference: [
      "Static PGR table separation does not prove runtime execution, persistence, cleanup, reward settlement, or product parity.",
      "A present loadout or time field does not authorize importing PGR loadout, record-time, signal-orb, three-ping, or QTE systems.",
      "ZH is compare-only and cannot be unioned with EN or used to fill an EN absence.",
    ],
  },
  sourceValuePolicy: {
    foreignIdentityMetadataAllowed: ["GuideFight.Id", "GuideFight.StageId", "Stage.StageId"],
    copiedPayloadValues: [],
    sourceValueCopiedCount: 0,
    withheldFamilies: ["labels-and-descriptions", "npc-weapon-robot-identifiers", "time-level-cost-reward-and-tuning-values"],
  },
  acceptanceEffect: "none; these four new versioned replacement candidates do not reuse historical source IDs, enter inScopeSourceIds, populate claim mappings/crosswalkRows, or pass any LiveAcceptance gate.",
  negativeBoundaries: [
    "The package proves exact static shapes and four EN GuideFight-to-Stage joins only.",
    "It copies no authored label, description, loadout, record-time, course, practice, or teaching payload value.",
    "Identity metadata does not prove a runtime consumer, evaluator, unlock transaction, persistence, reward settlement, or DimensionBrawl product requirement.",
    "ZH is compare-only and never fills an EN null or absence.",
  ],
  normalization: "UTF-8 without BOM; LF; exactly one final LF; JSON property/array order authored by generator; canonicalReportDigest is recursive sorted-key compact JSON with itself omitted; CSV has fixed headers, every cell double-quoted, comma delimiter, and RFC4180 escaping.",
};
const canonicalSummaryDigest = sha256(Buffer.from(canonicalize(summaryWithoutDigest), "utf8"));
const summary = { ...summaryWithoutDigest, canonicalReportDigest: canonicalSummaryDigest };
const summaryOutput = `${JSON.stringify(summary, null, 2)}\n`;

const labelOutput = encodeCsv(labelCsvHeaders, labelCsvRows);
const linkOutput = encodeCsv(linkCsvHeaders, linkCsvRows);

const tableRowsMarkdown = tableShapes.map((row) => `| \`${row.inputId}\` | ${row.role} | ${row.rowCount} | ${row.unionKeyCount} | ${row.distinctRowKeySetCount} | \`${row.unionKeySetSha256}\` |`).join("\n");
const joinRowsMarkdown = selectedRows.map((row) => `| ${row.sourceOrdinal} | \`${row.guideFightRowKey}\` | \`${row.stageIdentityKey}\` | ${row.enJoinMultiplicity} | ${row.zhCompareJoinMultiplicity} | ${row.enNonEmptyLabelFieldCount}/3 | ${row.loadoutState} | ${row.recordTimeState} |`).join("\n");
const markdownOutput = `# PGR GuideFight Stage Spine — alt3ri 856a0e45 v1

Status: **replacement candidate / exact static / not admitted**

## Contract

- Replacement contract: \`${CONTRACT_ID}\`
- Producer contract: \`${PRODUCER_CONTRACT_ID}\`
- Artifact set: \`${ARTIFACT_SET_ID}\`
- Source snapshot: \`${SOURCE_SNAPSHOT_ID}\`
- This is a new versioned semantic successor. It does not recreate or overwrite the four missing historical source identities.

## Authority and comparison boundary

- Upstream: \`alt3ri/PGR_Data\`
- Revision: \`856a0e4534d0854fa440040e961b74a97ba732e2\`
- Snapshot: \`2026-06-14\`
- Authority locale: EN
- Comparison-only locale: ZH; never unioned with EN
- License disposition: \`unknown-review-needed\`
- Authored payload values copied into this package: **0**; exact Id/StageId values are whitelisted identity metadata only.

## Bounded structural input shapes

| Input | Role | Rows | Union keys | Distinct row key sets | Union-key digest |
|---|---|---:|---:|---:|---|
${tableRowsMarkdown}

## Exact selection

| Ordinal | Guide row | Stage identity | EN matches | ZH compare matches | Non-empty label fields | Loadout state | Record-time state |
|---:|---|---|---:|---:|---:|---|---|
${joinRowsMarkdown}

Label strings, descriptions, loadout identifiers, and record-time values are withheld. The label-context CSV has 20 fixed rows (four selections by five structural fields), bound to full Stage-row hashes rather than low-entropy field hashes. The reading-links CSV has 56 fixed rows (four selections by fourteen semantic slots), with explicit present/absent/unresolved and proven-static/unknown states.

## Generated artifacts

${artifacts.map((artifact) => `- \`${artifact.sourceId}\` — ${artifact.format}; replaces \`${artifact.replacesHistoricalSourceId}\` only by new versioned semantic identity`).join("\n")}

## Structural observations

The separate course, practice, and teaching inputs are used only for row/key-shape observations. They do not supply current product requirements, authored text, loadout data, tuning values, or runtime claims.

## DimensionBrawl comparison

DimensionBrawl already has typed playable-stage identity, an immutable route snapshot, truthful briefing joins, terminal actions, and a durable result receipt. The immediate product order remains the frozen result/progression join, Station count-one Add authoring, foreign-evidence disposition, and P1-B full-exit audit.

The separate PGR course, practice, and teaching table shapes are later authoring candidates, not current requirements. Target-time/mastery belongs behind P1-D, and loadout truth requires a separately accepted owner. No PGR signal-orb, three-ping, QTE, loadout, or record-time system is imported by this evidence.

## Negative boundary

Static rows and joins do not prove runtime admission, stage execution, evaluator semantics, terminal cleanup, persistence, reward settlement, or shipped product behavior. ZH is compare-only, exact-row null is not table-wide absence, and this package has zero effect on the eleven-source atomic gate.

## Acceptance effect

None. These four artifacts remain outside \`inScopeSourceIds\`; admitted supporting sources remain 0/9, live rows 0/5, and live crosswalk cells 0/70.

Canonical report digest: \`${canonicalSummaryDigest}\`
`;

const outputs = {
  readFirstMarkdown: markdownOutput.replaceAll("\r\n", "\n"),
  readFirstSummary: summaryOutput.replaceAll("\r\n", "\n"),
  labelContext: labelOutput.replaceAll("\r\n", "\n"),
  readingLinks: linkOutput.replaceAll("\r\n", "\n"),
};

for (const output of Object.values(outputs)) {
  assert(output.endsWith("\n"), "every output must end with one LF");
  assert(!output.endsWith("\n\n"), "output has more than one trailing LF");
  assert(!output.includes("\r"), "output contains CR");
}

const selectedLabelValues = selectedRows.flatMap((row, index) => {
  const stageRow = enStageById.get(String(pgrEnGuide[index].StageId)).row;
  const zhStageRow = zhStageById.get(String(pgrEnGuide[index].StageId)).row;
  return [...labelFields.map((field) => stageRow[field]), ...labelFields.map((field) => zhStageRow[field])];
}).filter((value) => typeof value === "string" && value.length >= 3);
const prohibitedIdentifierPayloadValues = selectedRows.flatMap((row) => [
  row.guideRow.NpcId,
  row.guideRow.Weapon,
  row.enStageRow.RobotId,
  row.zhStageRow.RobotId,
]).filter((value) => typeof value === "string" && value.length >= 4);
const allowedIdentityMetadata = new Set(expectedPairs.flatMap(([id, stageId]) => [String(id), String(stageId)]));
for (const [outputKey, output] of Object.entries(outputs)) {
  for (const sourceValue of selectedLabelValues) {
    assert(!output.includes(sourceValue), `${outputKey} copied a selected label source value`);
  }
  for (const sourceValue of prohibitedIdentifierPayloadValues) {
    if (!allowedIdentityMetadata.has(sourceValue)) assert(!output.includes(sourceValue), `${outputKey} copied a selected loadout identifier payload value`);
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
console.log("sourceValueCopied=0");
console.log("admissionEffect=none");
