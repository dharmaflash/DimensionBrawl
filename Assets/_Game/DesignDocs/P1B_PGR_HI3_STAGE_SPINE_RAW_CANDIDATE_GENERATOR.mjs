import { createHash } from "node:crypto";
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const GENERATOR_ID = "P1B-PGR-HI3-STAGE-SPINE-RAW-CANDIDATE-GENERATOR-01";
const REPORT_ID = "P1B-PGR-HI3-STAGE-SPINE-RAW-CANDIDATE-01";
const FOREIGN_EVIDENCE_REF = "EV-EVID-P1B-RAW-FIVE-ROW-CANDIDATE-20260715";
const here = dirname(fileURLToPath(import.meta.url));
const reportPath = join(here, "P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE.json");
const arkRoot = process.env.ARK_SUBCULTURE_ROOT || "C:\\Ark\\SubcultureGameData";

const pgrRoot = join(
  arkRoot,
  "games",
  "punishing-gray-raven",
  "raw",
  "alt3ri-pgr-data",
  "2026-06-14",
);
const hi3Root = join(
  arkRoot,
  "games",
  "honkai-impact-3rd",
  "raw",
  "nairieberry-honkaiimpactdata",
  "2026-06-15",
);

const inputs = {
  pgrSourceRecord: join(pgrRoot, "source-record.md"),
  pgrProducerManifest: join(pgrRoot, "manifest.yml"),
  pgrFileManifest: join(pgrRoot, "file-manifest.csv"),
  pgrEn: join(
    pgrRoot,
    "files",
    "extracted_repo",
    "PGR_Data-master",
    "EN",
    "bytes",
    "share",
    "guide",
    "GuideFight.json",
  ),
  pgrZh: join(
    pgrRoot,
    "files",
    "extracted_repo",
    "PGR_Data-master",
    "ZH",
    "bytes",
    "share",
    "guide",
    "GuideFight.json",
  ),
  hi3SourceRecord: join(hi3Root, "source-record.md"),
  hi3ProducerManifest: join(hi3Root, "manifest.yml"),
  hi3FileManifest: join(hi3Root, "files", "hi3-nairieberry-file-manifest.csv"),
  hi3Global: join(
    hi3Root,
    "files",
    "extracted_repo",
    "HonkaiImpactData-master",
    "Global",
    "ExcelOutputAsset",
    "Decrypted",
    "StageData_Main.json",
  ),
  pgrControl: join(here, "P1B_PGR_2020_GUIDEFIGHT_CONTROL.json"),
  hi3Control: join(here, "P1B_HI3_2021_STAGEDATA_10101_CONTROL.json"),
};

const expectedHashes = {
  pgrSourceRecord: "23cecc493fe4e69f59f73520e7da694c22ac76fc2283deb88070d165c37725ee",
  pgrProducerManifest: "00f535d4bb159a0f9a43a824bda3e9fad721ae3074717b01d0b68c4f1e86400d",
  pgrFileManifest: "f3909c0d8b24b9e2770cead82f86418a52536e6d2ab1602f43eae862bdc55115",
  pgrEn: "62c184d70d88a3daf377ce6a2558c6e2cb192c4dbefbae3f02b4f549b6b46e7d",
  pgrZh: "62c184d70d88a3daf377ce6a2558c6e2cb192c4dbefbae3f02b4f549b6b46e7d",
  hi3SourceRecord: "eaf1785b6fb0cddcecca59047d7de229a4130f5f67512c3e6839848c8286352e",
  hi3ProducerManifest: "11e0be6d8e9f431d6f16da48213001edc033673e0709358798e5efe52d000faa",
  hi3FileManifest: "c0c63cbf79f26d3e7f11e651c4fab6047b814b4aea3f17f9c8b9fafdb3c94cb8",
  hi3Global: "6ab32c175b399d89d035e9736d150760725dd4f85cc5bd9870c64093c51a7431",
};

const semanticSlotOrder = [
  "logicalStageId",
  "physicalSceneOrScript",
  "briefingAndCatalog",
  "recommendedPowerOrLevel",
  "loadout",
  "restrictions",
  "entryCost",
  "recordOrTargetTime",
  "prerequisite",
  "recommendedNext",
  "storyEntry",
  "storyExit",
  "challengeReference",
  "resultReference",
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

function readBytes(path) {
  return readFileSync(path);
}

function readJson(path) {
  return JSON.parse(readFileSync(path, "utf8"));
}

function canonicalize(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  return `{${Object.keys(value)
    .sort()
    .map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`)
    .join(",")}}`;
}

function projectHash(value) {
  const bytes = Buffer.from(canonicalize(value), "utf8");
  return { sizeBytes: bytes.length, sha256: sha256(bytes) };
}

function normalizedArkRelative(path) {
  const root = resolve(arkRoot).replaceAll("\\", "/").replace(/\/$/, "");
  const normalized = resolve(path).replaceAll("\\", "/");
  assert(normalized.startsWith(`${root}/`), `path escapes Ark root: ${path}`);
  return normalized.slice(root.length + 1);
}

const inputIntegrity = {};
for (const [name, expected] of Object.entries(expectedHashes)) {
  const bytes = readBytes(inputs[name]);
  const actual = sha256(bytes);
  assert(actual === expected, `${name} SHA-256 mismatch: ${actual}`);
  inputIntegrity[name] = {
    arkRelativePath: normalizedArkRelative(inputs[name]),
    sizeBytes: bytes.length,
    sha256: actual,
  };
}

const pgrEnBytes = readBytes(inputs.pgrEn);
const pgrZhBytes = readBytes(inputs.pgrZh);
assert(pgrEnBytes.equals(pgrZhBytes), "PGR EN/ZH comparison sibling is not byte-identical");

const pgrRows = JSON.parse(pgrEnBytes.toString("utf8"));
assert(Array.isArray(pgrRows) && pgrRows.length === 4, "PGR GuideFight must contain exactly four rows");
assert(new Set(pgrRows.map((row) => String(row.Id))).size === 4, "PGR GuideFight IDs must be unique");
const exactPgr = [
  { Id: 100001, StageId: 10010001, NpcId: null, Weapon: null, DefaultRecordTime: null },
  { Id: 100002, StageId: 10010002, NpcId: "[19990]", Weapon: "[2022001]", DefaultRecordTime: 400 },
  { Id: 100003, StageId: 10010003, NpcId: null, Weapon: null, DefaultRecordTime: null },
  { Id: 100004, StageId: 10010005, NpcId: null, Weapon: null, DefaultRecordTime: null },
];
assert(canonicalize(pgrRows) === canonicalize(exactPgr), "PGR exact four-row projection changed");
const pgrSelectionProjection = projectHash(pgrRows);
assert(pgrSelectionProjection.sizeBytes === 352, "PGR canonical selection size changed");
assert(
  pgrSelectionProjection.sha256 === "8b9342eb32e67527d29a05253a3f36bcbf9998b380cefb9b286916fe90414346",
  "PGR canonical selection digest changed",
);
const pgrKeySetProjection = projectHash(Object.keys(pgrRows[0]).sort());
assert(
  pgrKeySetProjection.sha256 === "bf016b2c2e9d7042d01d20e368ab839a10d2e159dd2a37044b1fb2615ecdf4e3",
  "PGR key-set digest changed",
);
const expectedPgrRowHashes = [
  "11d8e073473a021e1d49a5374e9a7d523b377cd7ffa4da9cc1abfe09aa67d11e",
  "4b01ac60f053b72cd548346253eb7ed2ffc0d0d4850680c9ecb1fa85f8d8a7c3",
  "8b3770a8d5a720ff2b321d05ae514a522b09925bcc788f3db6f00fb201ab6a21",
  "35f00689aa6b35af74555e21371b8cd2982626516fe2223f18165c94a19828cc",
];
const pgrRowProjections = pgrRows.map((row, index) => {
  const projection = projectHash(row);
  assert(projection.sha256 === expectedPgrRowHashes[index], `PGR row ${index + 1} digest changed`);
  return { sourceOrdinal: index + 1, foreignRowOrKey: `Id=${row.Id}`, ...projection };
});

const pgrControl = readJson(inputs.pgrControl);
assert(pgrControl.source.sha256.toLowerCase() === "d846ab057e526ed4cd9dabac534ba561d2a14a87fa605363b0730ef45a1ba590", "PGR control source digest changed");
assert(pgrControl.boundedExactRows.length === 3, "PGR control must retain three rows");
const currentById = new Map(pgrRows.map((row) => [String(row.Id), row]));
const historicalById = new Map(pgrControl.boundedExactRows.map((row) => [String(row.id), row]));
const added = pgrRows.filter((row) => !historicalById.has(String(row.Id)));
const removed = pgrControl.boundedExactRows.filter((row) => !currentById.has(String(row.id)));
assert(added.length === 1 && added[0].Id === 100004, "PGR added-row disposition changed");
assert(removed.length === 0, "PGR control row was removed");
for (const historical of pgrControl.boundedExactRows) {
  const current = currentById.get(String(historical.id));
  assert(String(current.StageId) === String(historical.stageId), `PGR StageId drift at ${historical.id}`);
}

const hi3Rows = JSON.parse(readBytes(inputs.hi3Global).toString("utf8"));
assert(Array.isArray(hi3Rows) && hi3Rows.length === 9642, "HI3 StageData_Main row count changed");
assert(new Set(hi3Rows.map((row) => String(row.levelId))).size === hi3Rows.length, "HI3 levelId values are not unique");
const hi3Matches = hi3Rows
  .map((row, index) => ({ row, index }))
  .filter(({ row }) => typeof row.levelId === "number" && row.levelId === 10101);
assert(hi3Matches.length === 1, "HI3 numeric levelId=10101 must match exactly once");
const hi3Match = hi3Matches[0];
assert(hi3Match.index === 1, "HI3 levelId=10101 ordinal changed");
assert(Object.keys(hi3Match.row).length === 67, "HI3 levelId=10101 key count changed");
const hi3RowProjection = projectHash(hi3Match.row);
assert(hi3RowProjection.sizeBytes === 1665, "HI3 row canonical size changed");
assert(
  hi3RowProjection.sha256 === "93eb25ca807d6a7f5230cd1ca52e66d68c9f956db3eab25d8013d338699c968f",
  "HI3 row canonical digest changed",
);
const hi3KeySetProjection = projectHash(Object.keys(hi3Match.row).sort());
assert(
  hi3KeySetProjection.sha256 === "bf6bba4b74ba32cfc80828ba569dc3fc96ae578406c43ac160b4b2ad6a226eec",
  "HI3 key-set digest changed",
);
const hi3Control = readJson(inputs.hi3Control);
assert(hi3Control.source.revision === "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1", "HI3 control revision changed");
assert(hi3Control.source.sha256.toLowerCase() === expectedHashes.hi3Global, "HI3 control source hash no longer matches Ark source");
assert(hi3Control.reproduction.targetCanonicalProjection.sha256.toLowerCase() === hi3RowProjection.sha256, "HI3 control row hash no longer reconciles");
assert(hi3Control.reproduction.topLevelKeySetProjection.sha256.toLowerCase() === hi3KeySetProjection.sha256, "HI3 control key-set hash no longer reconciles");

const localAxes = {
  logicalStageId: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "StageRunRouteSnapshot",
    dimensionBrawlField: "Identity.PlayableStageId/RouteRevision/CanonicalRouteDigest",
    dimensionBrawlClassification: "proven-runtime",
    dimensionBrawlEvidenceRef: "EV-P1A-EXIT79-VALIDATOR",
    dimensionBrawlCutoffRef: "SNAP-P1A-CURRENT-SCHEMA-EXIT-CLOSED-79",
    dimensionBrawlOwnerBoundary: "The canonical OLYMPUS-INVASION-01 route identity is owned locally; a foreign identifier is comparison evidence and is never copied into that authority.",
  },
  physicalSceneOrScript: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "PlayableStageDefinition",
    dimensionBrawlField: "Segments.StageDefinition.SceneAsset",
    dimensionBrawlClassification: "proven-runtime",
    dimensionBrawlEvidenceRef: "EV-P1A-EXIT79-FULL-ROUTE",
    dimensionBrawlCutoffRef: "SNAP-P1A-CURRENT-SCHEMA-EXIT-CLOSED-79",
    dimensionBrawlOwnerBoundary: "The local route owns Corridor and Station scene resolution; foreign scene/script fields do not authorize local loading or cleanup.",
  },
  briefingAndCatalog: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "UIStageRouteProjection",
    dimensionBrawlField: "CatalogEntryId/PlayableStageId/CanonicalProjectionDigest",
    dimensionBrawlClassification: "proven-static",
    dimensionBrawlEvidenceRef: "EV-P1B-CATALOG-REMEDIATION-VALIDATOR",
    dimensionBrawlCutoffRef: "SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05",
    dimensionBrawlOwnerBoundary: "Candidate-05 proves only the catalog-to-route projection. The separately frozen truthful briefing contract is not counted as implemented by this candidate packet.",
  },
  recommendedPowerOrLevel: unresolvedLocal("No accepted truthful current-route power recommendation is joined; historical tier authoring and rev2A contract freeze are not product acceptance."),
  loadout: unresolvedLocal("No accepted canonical loadout identity owner is established by the current bounded local evidence."),
  restrictions: unresolvedLocal("No accepted restriction-set owner is joined to the current route; future P2-A authoring is outside this packet."),
  entryCost: unresolvedLocal("No accepted entry-cost transaction owner is joined to the current route; peer economy shape cannot authorize one."),
  recordOrTargetTime: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "StageRunResultSummary",
    dimensionBrawlField: "TotalActiveMilliseconds/CombatActiveMilliseconds",
    dimensionBrawlClassification: "proven-runtime",
    dimensionBrawlEvidenceRef: "EV-P1A-EXIT79-AGGREGATE",
    dimensionBrawlCutoffRef: "SNAP-P1A-CURRENT-SCHEMA-EXIT-CLOSED-79",
    dimensionBrawlOwnerBoundary: "The local owner commits elapsed clocks only; it is not a target-time, ranking, or reward promise.",
  },
  prerequisite: unresolvedLocal("Physical predecessor metadata is not an accepted logical unlock or durable progression prerequisite join."),
  recommendedNext: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "StageRunResultSummary",
    dimensionBrawlField: "OfferedTerminalActions",
    dimensionBrawlClassification: "proven-runtime",
    dimensionBrawlEvidenceRef: "EV-P1A-EXIT79-UI-15",
    dimensionBrawlCutoffRef: "SNAP-P1A-CURRENT-SCHEMA-EXIT-CLOSED-79",
    dimensionBrawlOwnerBoundary: "Replay, Retry, and Lobby are terminal actions, not a recommended-next progression node.",
  },
  storyEntry: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "StagePresentationHandoffRef",
    dimensionBrawlField: "DirectProfile/ExpectedPortId/TriggerConditionId/CompletionConditionId",
    dimensionBrawlClassification: "proven-runtime",
    dimensionBrawlEvidenceRef: "EV-P1B-DIRECT-PRESENTATION-ACTUAL-PATH",
    dimensionBrawlCutoffRef: "SNAP-P1B-ANCHOR-PROFILE-HYGIENE-03",
    dimensionBrawlOwnerBoundary: "The Corridor entry handoff is direct and accepted; foreign entry-shaped fields do not prove equivalent runtime lifecycle ownership.",
  },
  storyExit: {
    dimensionBrawlOwnerState: "absent",
    dimensionBrawlOwner: null,
    dimensionBrawlField: null,
    dimensionBrawlClassification: "proven-static",
    dimensionBrawlEvidenceRef: "EV-P1B-DIRECT-PRESENTATION-VALIDATOR",
    dimensionBrawlCutoffRef: "SNAP-P1B-DIRECT-PRESENTATION-JOIN-01",
    dimensionBrawlOwnerBoundary: "All non-Corridor-entry revision-1 presentation arms are explicitly absent; this does not prove permanent or gamewide story-exit absence.",
  },
  challengeReference: unresolvedLocal("No accepted P1-D challenge/objective reference is joined; candidate types or future evaluators remain outside this packet."),
  resultReference: {
    dimensionBrawlOwnerState: "present",
    dimensionBrawlOwner: "StageRunResultSummary",
    dimensionBrawlField: "ResultSummaryDigest/Outcome/OfferedTerminalActions",
    dimensionBrawlClassification: "proven-runtime",
    dimensionBrawlEvidenceRef: "EV-P1A-EXIT79-AGGREGATE",
    dimensionBrawlCutoffRef: "SNAP-P1A-CURRENT-SCHEMA-EXIT-CLOSED-79",
    dimensionBrawlOwnerBoundary: "P1-A owns committed result truth, but the playable-stage spine still has no accepted direct StageResultDefinition join.",
  },
};

function unresolvedLocal(boundary) {
  return {
    dimensionBrawlOwnerState: "unresolved",
    dimensionBrawlOwner: null,
    dimensionBrawlField: null,
    dimensionBrawlClassification: "unknown",
    dimensionBrawlEvidenceRef: "EV-P1B-LOCAL-STAGE-SPINE-PREFLIGHT",
    dimensionBrawlCutoffRef: "SNAP-P1B-LOCAL-PREFLIGHT-01",
    dimensionBrawlOwnerBoundary: boundary,
  };
}

function createCell({
  sourceId,
  snapshotId,
  ordinal,
  rowKey,
  claimId,
  slot,
  valueState,
  fieldPaths,
  supportedStatement,
  foreignClassification,
  negativeBoundary,
}) {
  const local = localAxes[slot];
  assert(local, `missing local-axis mapping for ${slot}`);
  return {
    foreignSourceId: sourceId,
    foreignSourceSnapshotId: snapshotId,
    foreignSourceOrdinal: ordinal,
    foreignRowOrKey: rowKey,
    semanticSlotId: slot,
    valueState,
    foreignFieldPaths: fieldPaths,
    claimId,
    sourceMappingRef: `${sourceId}::${rowKey}::${slot}`,
    supportedStatement,
    foreignClassification,
    foreignEvidenceRef: FOREIGN_EVIDENCE_REF,
    dimensionBrawlOwnerState: local.dimensionBrawlOwnerState,
    dimensionBrawlOwner: local.dimensionBrawlOwner,
    dimensionBrawlField: local.dimensionBrawlField,
    dimensionBrawlClassification: local.dimensionBrawlClassification,
    dimensionBrawlEvidenceRef: local.dimensionBrawlEvidenceRef,
    dimensionBrawlCutoffRef: local.dimensionBrawlCutoffRef,
    dimensionBrawlOwnerBoundary: local.dimensionBrawlOwnerBoundary,
    negativeBoundary,
    sourceValueCopied: false,
  };
}

const pgrSourceId = "pgr-guidefight-alt3ri-856a0e45-en-json";
const pgrSnapshotId = "pgr-alt3ri-856a0e45-en-guidefight";
const hi3SourceId = "hi3-stagedata-main-nairieberry-01d7afb-global-json";
const hi3SnapshotId = "hi3-nairieberry-01d7afb-global-stagedata-main";
const crosswalkRows = [];

for (let index = 0; index < pgrRows.length; index += 1) {
  const row = pgrRows[index];
  const rowKey = `Id=${row.Id}`;
  for (const slot of semanticSlotOrder) {
    let valueState = "unresolved";
    let fieldPaths = [];
    let supportedStatement = `No ${slot} field is identified in this exact bounded GuideFight row.`;
    let foreignClassification = "unknown";
    let negativeBoundary = "The bounded row does not prove absence from linked PGR tables, consumers, runtime behavior, another locale, later snapshots, or the gamewide schema.";
    if (slot === "logicalStageId") {
      valueState = "present";
      fieldPaths = ["Id", "StageId"];
      supportedStatement = "The exact row contains one static guide-fight identity-to-stage identity pair.";
      foreignClassification = "proven-static";
      negativeBoundary = "The pair does not prove runtime admission, physical routing, progression identity, or parity with DimensionBrawl IDs.";
    } else if (slot === "loadout") {
      if (row.NpcId !== null && row.Weapon !== null) {
        valueState = "present";
        fieldPaths = ["NpcId", "Weapon"];
        supportedStatement = "The exact row contains non-null NPC and weapon reference fields.";
        foreignClassification = "proven-static";
        negativeBoundary = "Static reference presence does not prove entity meaning, equipment semantics, selection rules, or runtime consumption.";
      } else {
        valueState = "absent";
        supportedStatement = "The exact row encodes both bounded loadout reference fields as null.";
        foreignClassification = "proven-static";
        negativeBoundary = "Exact-row null is not table-wide, linked-table, runtime, later-snapshot, or gamewide absence.";
      }
    } else if (slot === "recordOrTargetTime") {
      if (row.DefaultRecordTime !== null) {
        valueState = "present";
        fieldPaths = ["DefaultRecordTime"];
        supportedStatement = "The exact row contains one non-null default record-time field.";
        foreignClassification = "proven-static";
        negativeBoundary = "Field presence does not establish units, comparison direction, qualification, ranking, reward, or persistence semantics.";
      } else {
        valueState = "absent";
        supportedStatement = "The exact row encodes the bounded default record-time field as null.";
        foreignClassification = "proven-static";
        negativeBoundary = "Exact-row null is not table-wide, linked-table, runtime, later-snapshot, or gamewide absence.";
      }
    }
    crosswalkRows.push(
      createCell({
        sourceId: pgrSourceId,
        snapshotId: pgrSnapshotId,
        ordinal: index + 1,
        rowKey,
        claimId: "PGR-STAGE-SPINE-01",
        slot,
        valueState,
        fieldPaths,
        supportedStatement,
        foreignClassification,
        negativeBoundary,
      }),
    );
  }
}

assert(hi3Control.crosswalkContract.cells.length === 14, "HI3 control crosswalk must contain fourteen cells");
assert(
  hi3Control.crosswalkContract.cells.map((cell) => cell.semanticSlotId).join("|") === semanticSlotOrder.join("|"),
  "HI3 control semantic slot order changed",
);
for (const controlCell of hi3Control.crosswalkContract.cells) {
  crosswalkRows.push(
    createCell({
      sourceId: hi3SourceId,
      snapshotId: hi3SnapshotId,
      ordinal: 2,
      rowKey: "levelId=10101",
      claimId: "HI3-STAGE-SPINE-01",
      slot: controlCell.semanticSlotId,
      valueState: controlCell.valueState,
      fieldPaths: [...controlCell.foreignFieldPaths],
      supportedStatement: controlCell.supportedStatement,
      foreignClassification: controlCell.classification,
      negativeBoundary: controlCell.negativeBoundary,
    }),
  );
}

assert(crosswalkRows.length === 70, `expected 70 cells, got ${crosswalkRows.length}`);
assert(
  new Set(crosswalkRows.map((cell) => [cell.foreignSourceId, cell.foreignSourceSnapshotId, cell.foreignRowOrKey, cell.semanticSlotId].join("\u001f"))).size === 70,
  "crosswalk cell identities are not unique",
);

function countBy(items, selector) {
  const result = {};
  for (const item of items) {
    const key = selector(item);
    result[key] = (result[key] || 0) + 1;
  }
  return result;
}

const pgrCells = crosswalkRows.filter((cell) => cell.foreignSourceId === pgrSourceId);
const hi3Cells = crosswalkRows.filter((cell) => cell.foreignSourceId === hi3SourceId);
assert(canonicalize(countBy(pgrCells, (cell) => cell.valueState)) === canonicalize({ present: 6, absent: 6, unresolved: 44 }), "PGR cell counts changed");
assert(canonicalize(countBy(hi3Cells, (cell) => cell.valueState)) === canonicalize({ present: 10, unresolved: 4 }), "HI3 cell counts changed");

const reportWithoutDigest = {
  schemaVersion: 1,
  reportId: REPORT_ID,
  generatorId: GENERATOR_ID,
  status: "exact-raw-five-row-candidate-blocked-by-nine-supporting-citations",
  observedAt: "2026-07-15T21:45:00+09:00",
  purpose: "A deterministic bounded candidate over one current PGR GuideFight EN snapshot and one exact HI3 Global StageData_Main row. It proves source bytes, row selection, seventy explicit comparison cells, PGR drift, and HI3 reconciliation without admitting the eleven-source live packet or copying foreign values into DimensionBrawl authority.",
  acceptanceEffect: "none; candidate evidence only. The nine contracted supporting report identities are missing at their registered paths, so packet inScopeSourceIds, claim sourceMappings, liveRawSourceAdmission, generatedReportPath, packet crosswalkRows, and all three live acceptances remain unchanged/open.",
  normalization: "UTF-8 without BOM, LF, no trailing newline; JSON property and array order authored by this generator; canonicalPacketDigest is recursive sorted-key compact JSON over the report with canonicalPacketDigest omitted.",
  sourceAdmissionBoundary: {
    targetPacketId: "P1B-PGR-HI3-STAGE-SPINE-01",
    rawCandidateSourceIds: [pgrSourceId, hi3SourceId],
    missingSupportingSourceIds: [
      "pgr-readfirst-md",
      "pgr-readfirst-summary-json",
      "pgr-guidefight-label-csv",
      "pgr-guidefight-links-csv",
      "hi3-readfirst-md",
      "hi3-readfirst-summary-json",
      "hi3-readfirst-csv",
      "hi3-stage-summary-csv",
      "hi3-stage-samples-csv",
    ],
    requiredRegisteredSourceCount: 11,
    currentlyReproducibleRawCandidateSourceCount: 2,
    activationRule: "Do not place either raw candidate source in packet.inScopeSourceIds or populate packet cells until all eleven identities are present, hashed, and accepted atomically by LiveAcceptance.",
  },
  inputIntegrity,
  sources: [
    {
      sourceId: pgrSourceId,
      sourceSnapshotId: pgrSnapshotId,
      game: "Punishing: Gray Raven",
      upstream: "https://github.com/alt3ri/PGR_Data",
      branch: "master",
      revision: "856a0e4534d0854fa440040e961b74a97ba732e2",
      committedAt: "2026-05-29T23:28:20Z",
      snapshotDate: "2026-06-14",
      locale: "EN",
      comparisonLocale: "ZH byte-identical only; never unioned",
      licenseStatus: "unknown-review-needed",
      sourceRecordPath: inputIntegrity.pgrSourceRecord.arkRelativePath,
      producerManifestPath: inputIntegrity.pgrProducerManifest.arkRelativePath,
      relativePath: "files/extracted_repo/PGR_Data-master/EN/bytes/share/guide/GuideFight.json",
      sizeBytes: inputIntegrity.pgrEn.sizeBytes,
      sha256: inputIntegrity.pgrEn.sha256,
    },
    {
      sourceId: hi3SourceId,
      sourceSnapshotId: hi3SnapshotId,
      game: "Honkai Impact 3rd",
      upstream: "https://github.com/nairieberry/HonkaiImpactData",
      branch: "master",
      revision: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
      committedAt: "2021-04-10T22:17:18Z",
      snapshotDate: "2026-06-15",
      locale: "Global",
      licenseStatus: "none-detected-review-needed",
      sourceRecordPath: inputIntegrity.hi3SourceRecord.arkRelativePath,
      producerManifestPath: inputIntegrity.hi3ProducerManifest.arkRelativePath,
      relativePath: "files/extracted_repo/HonkaiImpactData-master/Global/ExcelOutputAsset/Decrypted/StageData_Main.json",
      sizeBytes: inputIntegrity.hi3Global.sizeBytes,
      sha256: inputIntegrity.hi3Global.sha256,
    },
  ],
  selection: {
    pgr: {
      selector: "all four rows from EN source order; ZH used only for byte-identity comparison",
      sourceRowCount: 4,
      duplicateIdCount: 0,
      exactRows: pgrRows.map((row, index) => ({
        sourceOrdinal: index + 1,
        foreignRowOrKey: `Id=${row.Id}`,
        stageIdentity: `StageId=${row.StageId}`,
        loadoutState: row.NpcId !== null && row.Weapon !== null ? "present" : "explicit-null-in-exact-row",
        recordOrTargetTimeState: row.DefaultRecordTime !== null ? "present" : "explicit-null-in-exact-row",
        canonicalRowSizeBytes: pgrRowProjections[index].sizeBytes,
        canonicalRowSha256: pgrRowProjections[index].sha256,
      })),
      canonicalSelectionProjection: pgrSelectionProjection,
      topLevelKeySetProjection: pgrKeySetProjection,
      sourceValuesCopiedIntoCells: false,
    },
    hi3: {
      selector: "typeof row.levelId === number && row.levelId === 10101",
      sourceRowCount: 9642,
      duplicateLevelIdCount: 0,
      matchCount: 1,
      zeroBasedIndex: 1,
      oneBasedOrdinal: 2,
      topLevelKeyCount: 67,
      canonicalRowProjection: hi3RowProjection,
      topLevelKeySetProjection: hi3KeySetProjection,
      sourceValuesCopiedIntoCells: false,
    },
  },
  crosswalkContract: {
    schemaVersion: 2,
    cellIdentity: "foreignSourceId + foreignSourceSnapshotId + foreignRowOrKey + semanticSlotId",
    semanticSlotOrder,
    sourceRowCount: 5,
    semanticSlotCountPerRow: 14,
    totalCellCount: 70,
    combinedValueStateCounts: countBy(crosswalkRows, (cell) => cell.valueState),
    combinedForeignClassificationCounts: countBy(crosswalkRows, (cell) => cell.foreignClassification),
    pgrValueStateCounts: countBy(pgrCells, (cell) => cell.valueState),
    hi3ValueStateCounts: countBy(hi3Cells, (cell) => cell.valueState),
    sourceValueCopiedCount: crosswalkRows.filter((cell) => cell.sourceValueCopied).length,
    cells: crosswalkRows,
  },
  pgrDriftAgainst2020Control: {
    controlReportId: pgrControl.reportId,
    controlSourceSha256: pgrControl.source.sha256.toLowerCase(),
    currentSourceSha256: inputIntegrity.pgrEn.sha256,
    historicalRowCount: 3,
    currentRowCount: 4,
    added: ["Id=100004/StageId=10010005"],
    removed: [],
    sharedIdentityStagePairChanges: [],
    schemaRepresentationChanges: [
      "NpcId[1..3] and Weapon[1..3] tab columns became NpcId and Weapon JSON-string fields",
      "DefaultRecordTime is a new bounded JSON field",
    ],
    exactRowStateChanges: [
      "Id=100001 loadout present-to-explicit-null",
      "Id=100002 loadout semantic identifiers retained across representation change and DefaultRecordTime became present",
      "Id=100003 loadout present-to-explicit-null",
      "Id=100004 is newly added with explicit-null loadout and record-time fields",
    ],
    negativeBoundary: "The 2020 control is a drift detector only. It is not unioned with the current source, cannot fill null cells, and proves no runtime or gamewide semantics.",
  },
  hi3ReconciliationAgainst2021Control: {
    controlReportId: hi3Control.reportId,
    sameUpstreamRevision: true,
    sameRawFileSha256: true,
    sameCanonicalTargetRowSha256: true,
    sameTopLevelKeySetSha256: true,
    classificationResult: "unchanged: ten present/proven-static cells and four unresolved/unknown cells",
    negativeBoundary: "The Ark snapshot stores the same upstream file bytes as the historical control. This is exact reconciliation, not evidence of a newer HI3 data state, runtime behavior, or locale union.",
  },
};

const canonicalPacketDigest = sha256(Buffer.from(canonicalize(reportWithoutDigest), "utf8"));
const report = { ...reportWithoutDigest, canonicalPacketDigest };
const output = JSON.stringify(report, null, 2).replaceAll("\r\n", "\n");
assert(!output.endsWith("\n"), "serialized report unexpectedly has a trailing newline");

if (process.argv.includes("--verify")) {
  const existing = readFileSync(reportPath, "utf8");
  assert(existing === output, `report bytes differ: ${reportPath}`);
  process.stdout.write(
    `${GENERATOR_ID}: VERIFY PASS reportSha256=${sha256(Buffer.from(existing, "utf8"))} canonicalPacketDigest=${canonicalPacketDigest} cells=${crosswalkRows.length}`,
  );
} else {
  writeFileSync(reportPath, output, "utf8");
  process.stdout.write(
    `${GENERATOR_ID}: WRITE PASS reportSha256=${sha256(Buffer.from(output, "utf8"))} canonicalPacketDigest=${canonicalPacketDigest} cells=${crosswalkRows.length}`,
  );
}
