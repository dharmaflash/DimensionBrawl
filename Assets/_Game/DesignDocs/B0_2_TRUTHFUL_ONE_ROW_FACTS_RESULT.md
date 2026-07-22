# B0-2 Truthful One-Row Facts and Result

Status: `IMPLEMENTED / VERIFIED`

Date: 2026-07-21 KST

## Outcome

B0-2 proves that the run owner can seal and persist a truthful result for either accepted
bounded route shape:

- a one-row route whose entry is also its terminal combat segment; or
- the existing two-row Olympus tutorial-to-combat route.

The independent in-memory route `B0-ONE-ROW-TEST-01` now admits through the production
route/reference/briefing/result/progression join path, binds fact collection to segment
zero, seals one completed segment, commits Clear and Fail results, recovers a transient
durable-read failure against an already written decision, and prepares/presents the exact
result once. This is still a
foundation proof, not a second product scene or catalog entry.

## Typed tutorial requirement

`StageRunRouteSnapshot` derives `StageRunTutorialFactRequirement` from immutable route
semantics without adding a serialized field or changing route digest order:

| Route shape | Requirement | Result tutorial digest |
|---|---|---|
| one row, entry plus final | `None` | empty string |
| current typed tutorial boundary plus final | `LegacyCorridorCompletion` | existing nonempty fact digest |

A route with `None` rejects tutorial completion sealing. It cannot manufacture the
Olympus plan ID, coverage rows, completion state, or guide fact. `StageRunResultSummary`
keeps the existing canonical key `result.tutorialRouteSummaryFactDigest`; the value is
empty only when the admitted route requires no tutorial. The field order is unchanged.

The Olympus plan semantic digest remains:

`b1b00dd84e27fe8d06c6736d85b16ff6bfe141b7ccb70b01ea851144dd8182f2`

## Terminal fact contract

`StageRunFactAccumulator` now derives the final segment index from the route snapshot.
The terminal collection window requires:

1. the current segment is the entered terminal row;
2. the exact current-run terminal collector is bound;
3. every pre-terminal segment is entered and completed; and
4. only for `LegacyCorridorCompletion`, the tutorial summary exists and the explicit
   terminal guide state is `Released`.

Damage, player-down, perfect-dodge, summon-use, semantic-proof, active-time, segment
completion, and outcome-segment identity all bind to that current terminal row. A
one-row result therefore owns exactly one entered/completed segment and no handoff token,
segment-entry receipt, or handoff-terminal receipt.

The old Station-named collector methods remain wrappers for the accepted Olympus scene.
Neutral scene components are intentionally deferred to B0-3.

## Result ownership and exactly-once boundary

Before the first terminal authority mutation, the public commit path now requires the
encounter's coordinator to be the exact coordinator registered for the active run ID and
scene handle, still live and `Idle` when first registered. The supplied terminal tuple must
also equal the encounter, coordinator, and epoch-evidence resolution field for field. The
first valid commit seals the stable encounter object as context authority. A foreign
scene, stale pre-resolved registration, forged tuple, or second same-scene coordinator is
rejected while the context remains terminal-active with zero terminal receipts.

After a result is committed, an exact duplicate request is reconciled against the sealed
encounter object, terminal record, and epoch closure. It does not require the live
coordinator generation or registration to remain after presentation cleanup, and it
returns the same summary and receipt without creating a second durable decision. A
value-identical terminal tuple and epoch evidence from a different encounter object cannot
replay the result.

Durable receipt lookup additionally checks that the receipt's sealed run ID equals the
requested run ID. Copying a valid decision under another safe run-ID filename now fails
closed instead of returning the misplaced receipt.

The one-row owner coverage uses the new
`StageTerminalFinalizationContext.NonCourseStageTerminal = 3`. The accepted Olympus value
`NonCourseStationTerminal = 1`, owner-row order, receipt schema, lifecycle numbers, and
canonical fields remain unchanged.

If terminal resolution arrives before collector readiness, finalization closes as one
typed `TerminalFinalizationFailed` abort. It creates no summary, commit receipt, decision
file, presentation snapshot, or presentation audit. The one-row route truthfully records
handoff coverage as `NotIssued`.

## Acceptance evidence

The focused one-row matrix proves:

- Clear: empty tutorial digest, one completed segment, segment-zero combat/outcome facts,
  an exact 250 ms active/combat/forward-risk clock, an actual 4.5 damage event matching
  player health and the terminal snapshot, transient durable-read recovery, exact duplicate
  commit, cache-cleared exact-run receipt read, and idempotent presentation prepare/mark;
- Fail: the actual lethal damage fact, one player down, `PlayerDefeated`, no survival
  proof, and only Retry/Lobby offered;
- collector missing: one typed abort and no product result;
- wrong collector scene, foreign encounter scene, stale registration, forged resolution,
  wrong registered same-scene coordinator, value-identical foreign replay, and misplaced
  durable decision: rejection before unauthorized authority or receipt publication; and
- Olympus: nonempty fixed tutorial digest and explicit guide-release gate remain required.

Verification ledger:

- `C:\tmp\DimensionBrawl-B0-2-StageRunRoute-Final-Verified.xml`: `40/40`;
- `C:\tmp\DimensionBrawl-B0-2-CoreRegression-Final2.xml`: `238/238`, covering route,
  terminal coordinator, durable result/receipt, canonical UI, Olympus full flow, and
  summon/energy regressions;
- `dotnet build DimensionBrawl.PlayModeTests.csproj`: zero warnings and zero errors; and
- `C:\tmp\DimensionBrawl-B0-2-PlayableStageValidator-Final.log`: static validator `PASS`.

The fixed Olympus identities remain:

- terminal policy digest:
  `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`;
- route digest:
  `878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0`;
- result/progression join digest:
  `d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71`;
- historical revision-1 route digest:
  `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9`.
- fixed callback-order-invariant Olympus result-summary digest:
  `46d2754e32f77deba5d55cecae99da0c45673c0d163754f7556c58721635f8a2`.

## Explicit deferrals

B0-2 does not make the Olympus scene adapters reusable. B0-3 must add a small neutral
bootstrap, fact adapter, result/recovery presenter, adapter-loss abort ownership, and
Replay/Retry/Lobby scene wiring that consume an already admitted one-row context without
calling `TryEnterPendingSegment` or fabricating handoff evidence.

B0-4 still owns multi-entry catalog projection, Stage Select card binding, validator
enumeration, and build-readiness route walking. B1 still owns the first actual compact
second scene and its isolated content assets.

## Post-B0-3 update

The B0-3 deferral above is retained as this milestone's historical boundary. It is now
closed by `B0_3_NEUTRAL_ONE_ROW_SCENE_ADAPTERS.md`: a lean active scene can bind the exact
bootstrap, fact, result, and same-process recovery owners; fail closed on adapter loss;
present through the neutral overlay interface; and route Replay/Retry/Lobby without
copying Olympus scene components. No second product scene or catalog row is claimed.

## Decision

B0-2 closes the truthful facts/result seam, and B0-3 has since closed its neutral scene
adapter deferral. The next bounded product gate is B0-4 catalog/build plumbing. Do not copy
the Olympus Corridor flow, Station collector, or Station result presenter into a new scene.
