# HI3 StageData stage spine - nairieberry 01d7afb v1

Status: **replacement candidate / exact static / not admitted**

## Contract

- Replacement contract: `P1B-HI3-STAGE-SPINE-REPLACEMENT-01`
- Producer contract: `HI3-STAGEDATA-STAGE-SPINE-PRODUCER-01`
- Artifact set: `HI3-STAGEDATA-STAGE-SPINE-NAIRIEBERRY-01D7AFB-V1`
- Source snapshot: `hi3-nairieberry-01d7afb-global-stagedata-spine-v1`
- This is a new versioned semantic successor. It does not recreate or overwrite the three missing historical source identities.

## Authority boundary

- Upstream: `nairieberry/HonkaiImpactData`
- Revision: `01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1`
- Upstream commit time: `2021-04-10T22:17:18Z`
- Retained snapshot: `2026-06-15`
- Locale: Global
- License disposition: `none-detected-review-needed`
- Selected root: exactly one numeric `levelId=10101`, source ordinal 2
- Authored payload values copied into this package: **0**; the selected levelId is whitelisted identity metadata only.

This is exact retained-mirror static evidence, not a claim about official current shipped behavior or a newer HI3 data state.

## Exact structural projection

- StageData_Main rows: 9642
- Duplicate levelId values: 0
- Target top-level fields: 67
- Canonical target-row digest: `93eb25ca807d6a7f5230cd1ca52e66d68c9f956db3eab25d8013d338699c968f`
- Top-level key-set digest: `bf6bba4b74ba32cfc80828ba569dc3fc96ae578406c43ac160b4b2ad6a226eec`
- Field-shape digest: `19833743758af7f5987d0fb591c82d9e275eb82e57d8c2d2c5ff806306abbb91`

- `array-empty`: 5
- `array-present`: 11
- `number-nonzero`: 25
- `number-zero`: 16
- `object-present`: 5
- `string-empty`: 2
- `string-present`: 3

No field value is stored in the field-shape ledger. Nested catalog objects are checked only for the single `Hash` key.

## Semantic reading links

| Ordinal | Slot | State | Classification | Field paths | Disposition |
|---:|---|---|---|---:|---|
| 0 | `logicalStageId` | present | proven-static | 8 | `identity-and-static-hierarchy-shape-only` |
| 1 | `physicalSceneOrScript` | present | proven-static | 1 | `static-script-reference-shape-only` |
| 2 | `briefingAndCatalog` | present | proven-static | 5 | `hashed-text-and-asset-reference-shape-only` |
| 3 | `recommendedPowerOrLevel` | present | proven-static | 4 | `static-level-and-difficulty-shape-only` |
| 4 | `loadout` | unresolved | unknown | 4 | `formation-shape-without-loadout-identity` |
| 5 | `restrictions` | present | proven-static | 13 | `static-restriction-field-family-only` |
| 6 | `entryCost` | present | proven-static | 4 | `static-entry-cost-field-family-only` |
| 7 | `recordOrTargetTime` | present | proven-static | 3 | `static-time-and-record-shape-only` |
| 8 | `prerequisite` | present | proven-static | 8 | `static-predecessor-and-unlock-shape-only` |
| 9 | `recommendedNext` | unresolved | unknown | 5 | `no-direct-next-stage-identity` |
| 10 | `storyEntry` | unresolved | unknown | 3 | `entry-shaped-fields-without-consumer` |
| 11 | `storyExit` | unresolved | unknown | 2 | `no-exit-specific-field-or-consumer` |
| 12 | `challengeReference` | present | proven-static | 1 | `static-challenge-reference-shape-only` |
| 13 | `resultReference` | present | proven-static | 7 | `static-result-facing-reference-shape-only` |

The CSV has exactly 14 rows in this order. It stores field paths and shape states only; source values, localized content, script paths, image paths, list contents, identifiers, tuning, time, costs, and rewards are withheld.

## Generated artifacts

- `hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-readfirst-md` - markdown; a new versioned successor for `hi3-readfirst-md`
- `hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-summary-json` - json; a new versioned successor for `hi3-readfirst-summary-json`
- `hi3-stagedata-stage-spine-nairieberry-01d7afb-global-v1-reading-links-csv` - csv; a new versioned successor for `hi3-readfirst-csv`

## Sibling helper boundary

`hi3-stage-summary-csv` and `hi3-stage-samples-csv` are not producer inputs. They remain byte-exact replay-authenticated sibling evidence with formal provenance/admission still open. A truncated sample cannot independently identify `levelId=10101`.

## DimensionBrawl comparison

DimensionBrawl already has typed playable-stage identity, an immutable route snapshot, truthful briefing joins, typed terminal actions, and a durable result receipt. The next product order remains result/progression joins, Station count-one Add authoring, and then explicit P1-C/P1-D owners.

HI3's restriction-, cost-, time-, challenge-, loadout-, and result-shaped fields are later comparison candidates only. They do not authorize importing foreign numbers, lists, rewards, economy, story, images, scripts, or balancing.

## Negative boundary

A static `luaFile` field is not execution. Reward/drop fields are not grant or persistence. Hashed catalog fields are not localized presentation. This package proves neither runtime consumers nor official shipped behavior and has zero effect on the eleven-source atomic gate.

## Acceptance effect

None. These three artifacts remain outside `inScopeSourceIds`; admitted supporting sources remain 0/9, live rows 0/5, and live crosswalk cells 0/70.

Canonical report digest: `d20113431ca54b1da5bc1f6c477b32de0fa9eb205f67d3e33cdaaafe4f6f7101`
