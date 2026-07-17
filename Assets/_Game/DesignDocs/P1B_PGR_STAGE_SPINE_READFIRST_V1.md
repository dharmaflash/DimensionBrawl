# PGR GuideFight Stage Spine — alt3ri 856a0e45 v1

Status: **replacement candidate / exact static / not admitted**

## Contract

- Replacement contract: `P1B-PGR-STAGE-SPINE-REPLACEMENT-01`
- Producer contract: `PGR-GUIDEFIGHT-STAGE-SPINE-PRODUCER-01`
- Artifact set: `PGR-GUIDEFIGHT-STAGE-SPINE-ALT3RI-856A0E45-V1`
- Source snapshot: `pgr-alt3ri-856a0e45-en-guidefight-stage-v1`
- This is a new versioned semantic successor. It does not recreate or overwrite the four missing historical source identities.

## Authority and comparison boundary

- Upstream: `alt3ri/PGR_Data`
- Revision: `856a0e4534d0854fa440040e961b74a97ba732e2`
- Snapshot: `2026-06-14`
- Authority locale: EN
- Comparison-only locale: ZH; never unioned with EN
- License disposition: `unknown-review-needed`
- Authored payload values copied into this package: **0**; exact Id/StageId values are whitelisted identity metadata only.

## Bounded structural input shapes

| Input | Role | Rows | Union keys | Distinct row key sets | Union-key digest |
|---|---|---:|---:|---:|---|
| `pgr-en-course-stage` | course-stage-shape | 30 | 5 | 1 | `3ef364a3170ff6d2ae196093bdee0333497d1b289724187116ce28aa745b4e97` |
| `pgr-en-course-chapter` | course-chapter-shape | 10 | 10 | 1 | `d1e8ba97cfdb42c20aa27a3744b87fb029012a5c6152eadf18d10cb3bb4354b0` |
| `pgr-en-course-stage-show-type` | course-presentation-shape | 2 | 6 | 1 | `1cbec6e09f6531bd38f4e50f865394b498fbe7b950be8ab960360f4f8a9ed4c9` |
| `pgr-en-practice-chapter` | practice-chapter-shape | 8 | 7 | 1 | `d9ce2788229a23bd6773765dbe26444987488f57ae46a75cfc7d770b444fa375` |
| `pgr-en-practice-group` | practice-group-shape | 88 | 4 | 1 | `83f49ee8738b8b70efde68e3409ca8bce83f6048173c555989b566a169bbe9b5` |
| `pgr-en-practice-skill-details` | practice-skill-presentation-shape | 85 | 5 | 1 | `77274f81e1d19adeff84fd3ff666bc6eb88a9482f716fea33ef414329f20fb6d` |
| `pgr-en-teaching-activity` | teaching-activity-shape | 48 | 29 | 1 | `e7a6df5768b5a185f833e003489b54b0b56c1072fd0cd6a70419268984caad85` |
| `pgr-en-teaching-robot` | teaching-loadout-shape | 139 | 5 | 1 | `d5e5209cc30aa886416438b499b72e196bc8e48d97dfff7d8aa897b99afb5437` |
| `pgr-en-guide-fight` | authoritative-four-row-guide-selection | 4 | 5 | 1 | `bf016b2c2e9d7042d01d20e368ab839a10d2e159dd2a37044b1fb2615ecdf4e3` |
| `pgr-en-stage` | authoritative-stage-join-and-label-shape | 10916 | 83 | 1 | `840b284d9ce57c7053165f1be5c563504fc1a1ba6554dc83415f3ba4d8955ec6` |

## Exact selection

| Ordinal | Guide row | Stage identity | EN matches | ZH compare matches | Non-empty label fields | Loadout state | Record-time state |
|---:|---|---|---:|---:|---:|---|---|
| 1 | `Id=100001` | `StageId=10010001` | 1 | 1 | 3/3 | exact-row-null | exact-row-null |
| 2 | `Id=100002` | `StageId=10010002` | 1 | 1 | 3/3 | present-withheld | present-withheld |
| 3 | `Id=100003` | `StageId=10010003` | 1 | 1 | 3/3 | exact-row-null | exact-row-null |
| 4 | `Id=100004` | `StageId=10010005` | 1 | 1 | 3/3 | exact-row-null | exact-row-null |

Label strings, descriptions, loadout identifiers, and record-time values are withheld. The label-context CSV has 20 fixed rows (four selections by five structural fields), bound to full Stage-row hashes rather than low-entropy field hashes. The reading-links CSV has 56 fixed rows (four selections by fourteen semantic slots), with explicit present/absent/unresolved and proven-static/unknown states.

## Generated artifacts

- `pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-readfirst-md` — markdown; replaces `pgr-readfirst-md` only by new versioned semantic identity
- `pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-summary-json` — json; replaces `pgr-readfirst-summary-json` only by new versioned semantic identity
- `pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-label-context-csv` — csv; replaces `pgr-guidefight-label-csv` only by new versioned semantic identity
- `pgr-guidefight-stage-spine-alt3ri-856a0e45-v1-reading-links-csv` — csv; replaces `pgr-guidefight-links-csv` only by new versioned semantic identity

## Structural observations

The separate course, practice, and teaching inputs are used only for row/key-shape observations. They do not supply current product requirements, authored text, loadout data, tuning values, or runtime claims.

## DimensionBrawl comparison

DimensionBrawl already has typed playable-stage identity, an immutable route snapshot, truthful briefing joins, terminal actions, and a durable result receipt. The immediate product order remains the frozen result/progression join, Station count-one Add authoring, foreign-evidence disposition, and P1-B full-exit audit.

The separate PGR course, practice, and teaching table shapes are later authoring candidates, not current requirements. Target-time/mastery belongs behind P1-D, and loadout truth requires a separately accepted owner. No PGR signal-orb, three-ping, QTE, loadout, or record-time system is imported by this evidence.

## Negative boundary

Static rows and joins do not prove runtime admission, stage execution, evaluator semantics, terminal cleanup, persistence, reward settlement, or shipped product behavior. ZH is compare-only, exact-row null is not table-wide absence, and this package has zero effect on the eleven-source atomic gate.

## Acceptance effect

None. These four artifacts remain outside `inScopeSourceIds`; admitted supporting sources remain 0/9, live rows 0/5, and live crosswalk cells 0/70.

Canonical report digest: `39c8136d6e0813f83a78c11e1a7ada648506d204fd59548a7e509bbdfb6eedd0`
