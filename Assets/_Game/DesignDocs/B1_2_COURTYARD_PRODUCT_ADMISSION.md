# B1-2 Courtyard Product Admission

Status: `WITHDRAWN / PRODUCT ADMISSION REVERSED 2026-07-22`

Admission checkpoint: 2026-07-22 KST

Withdrawal note: this document records a retired historical admission. Courtyard is no
longer present in the product catalog, Stage Select bindings, or Build Settings. The
admission writer and admitted-product visual-QA tool were removed to prevent accidental
republication. The isolated authored pack remains available only for direct review.

## Purpose

B1-2 promotes the already-authored and independently validated Olympus Courtyard Drill
from the B1-1 quarantine into the stage-based mobile product path. Catalog projection,
the second Stage Select card, and Unity Build Settings move together as one exact product
admission. This checkpoint does not change the Courtyard encounter design or invent a
parallel runtime route owner.

The admitted product cohort is catalog schema `1`, projection generation `3`, with exactly
two ordered entries:

| Order | Catalog ID | Playable-stage ID | Canonical projection digest |
|---:|---|---|---|
| 0 | `story_v1_training_route` | `OLYMPUS-INVASION-01` | `7bf7637516466673a3362b6caf761454632c6b1c7404d83d9c5e5ed2a6d59562` |
| 1 | `story_v1_courtyard_drill_route` | `OLYMPUS-COURTYARD-DRILL-01` | `588473db6022e05ccac3c8ebfe8c9cd5a5cf1ea50d1e02b5b6f4bce2e6594e34` |

Both entries reuse the existing `stage_to_combat_mood_bridge` loading-card identity.
The Courtyard projection presents the exact authored briefing title `Olympus Courtyard
Drill` and objective `Defeat the Courtyard terminal boss under Rifle Crossfire pressure.`

## Truthful Stage Select projection

The two real catalog rows bind exactly to Stage Select shells `01-1` and `01-2`. Bound
cards show their exact shell number and catalog title. Placeholder progression and
availability decoration is normalized away:

- `LockIcon` is absent or inactive on each bound real card;
- `StagePercentText` and `Star1` through `Star3` are inactive;
- the catalog reward-preview values remain empty and the detail reward row remains hidden;
- unused shells `01-3` and `01-4` remain inactive and non-interactable.

These states mean only that the two entries are selectable product projections. They are
not evidence of a durable unlock, completion percentage, rating, reward, or first-clear
system.

### Exact route-interaction gate

The pre-fix Stage Select prefab still serialized the old ten-control interaction-gate
inventory: Back, Start, the selected `EP 01` chapter shell, three unadmitted chapter
placeholders, and stage shells `01-1` through `01-4`. `UIRouteInteractableGate` restores
every serialized selectable to `interactable = true` when it enters the idle state. As a
result, Play Mode could re-enable the intentionally inert `EP 01` chapter-shell Button
and expose a dead interaction even though the shell itself was presentation-only.

The admission setup now regenerates that inventory from the admitted product route
instead of retaining legacy shell references. The exact and only gate members are:

1. `BackButton`
2. `StartButton`
3. `01-1_StageCard`
4. `01-2_StageCard`

The selected chapter shell, `EP 02` through `EP 04`, and `01-3` through `01-4` are not
route controls and are therefore absent from the gate. Setup validation, the playable
stage validator, the runtime visual-capture gate, and focused PlayMode coverage all fail
closed on a missing, duplicate, or extra member.

## Product build manifest

The canonical product manifest now records exactly `2` catalog entries, `3` logical route
segments, and `6` deduplicated physical scenes. Its canonical digest is
`38ed64a5266b6d3e6c46755f5f138d54cddb3a684896eef0776ef4c4c3c966a5`.

The exact enabled Build Settings order is:

1. `Assets/_Game/Scenes/UI/UI_Login.unity`
2. `Assets/_Game/Scenes/UI/UI_Lobby.unity`
3. `Assets/_Game/Scenes/UI/UI_StageSelect.unity`
4. `Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity`
5. `Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity`
6. `Assets/_Game/Scenes/UI/UI_StageClear.unity`

The accepted Olympus route contributes its ordered Corridor and Station logical segments;
the Station host is not an additional physical Build Settings row. Courtyard contributes
one independent logical segment and one physical scene.

## Atomicity and repeatability

`OlympusCourtyardDrillB12ProductAdmissionSetup` admits only either the exact historical
B1-1 baseline (generation `2`, one accepted row) or the exact already-admitted B1-2 state
(generation `3`, the two rows above). Unknown rows, changed order, stale projections, or
partial product state fail closed rather than being silently truncated.

Before mutation, the operation snapshots the catalog asset, Stage Select prefab, and
Build Settings scene list. Any setup or validation failure restores the exact captured
files and scene rows and verifies that rollback. Reapplying the operation to the exact
B1-2 state is idempotent and must retain the same two projection digests, card bindings,
scene order, and manifest digest.

## Verification checkpoint

| Gate | Current B1-2 result | Evidence |
|---|---|---|
| Product-admission setup and validator | `PASS` | `C:\tmp\DimensionBrawl-B1-2-ProductAdmissionSetup-ExactRouteGate.log` (`BATCH_PRODUCT_ADMISSION_SETUP_PASS`) |
| B1-2 Stage Select runtime visual QA | `PASS` at `1600x900` | `C:\tmp\DimensionBrawl-B1-2-StageSelectVisualQa-ExactRouteGate.log`; `C:\tmp\DimensionBrawl-B1-2-StageSelect.png` |
| `StageSelectExactBindingPlayModeTests` | `3/3 CLEAN` | `C:\tmp\DimensionBrawl-B1-2-StageSelectExact-ExactRouteGate-Retry.xml` |
| `CanonicalUiRoutePlayModeTests` | `38/38 CLEAN` | `C:\tmp\DimensionBrawl-B1-2-CanonicalUiRoute-Final.xml` |
| `OlympusChapterHubReviewControllerPlayModeTests` | `8/8 CLEAN` | `C:\tmp\DimensionBrawl-B1-2-ChapterHub-Final.xml` |
| Full project PlayMode regression | `477/477 CLEAN` | `C:\tmp\DimensionBrawl-B1-2-FullPlayMode-Final.xml` (`failed=0`, `skipped=0`, `duration=497.4762889s`) |

The visual capture entered the real Stage Select at `1600x900`, began on catalog row 0,
selected `01-2` through its bound Button, retained the Courtyard backing selection, and
captured the resulting truthful detail state without clicking Start. It also verified
that the exact six-row Build Settings manifest remained unchanged.

For historical context only, the pre-admission B1-1 baseline completed `476/476 CLEAN`
in `C:\tmp\DimensionBrawl-B1-1-FullPlayModeRegression-Final.xml`. The post-admission
B1-2 state now has its own independent `477/477 CLEAN` result above; the older result is
retained only as evidence for the preserved B1-1 checkpoint.

The historical `OlympusCourtyardDrillB11QuarantineGate` is intentionally B1-1-only. It
must not be weakened or treated as a current B1-2 gate: its expected one-row catalog and
Courtyard-absent Build Settings describe the preserved pre-admission checkpoint.

## Explicit product boundary

B1-2 admits a selectable second stage projection and its scene reachability only. It does
not claim or implement reward grants, first-clear rewards, completion persistence,
account-backed unlock state, prerequisite edges, availability scheduling, balance
acceptance, or live-operations services. Empty reward and progression decoration stays
truthful until those systems have separate owners and evidence.

## ArkData structural boundary

The ArkData review informed only structural separation: catalog projection versus runtime
route identity, ordered logical segments versus physical scenes, and product admission as
an explicit build-manifest boundary. No third-party code, IDs, field names, copy, art,
audio, UI layout, stage composition, encounter values, text, or implementation details
were copied. All B1-2 IDs, presentation decisions, digests, validation rules, and authored
content are local DimensionBrawl decisions.
