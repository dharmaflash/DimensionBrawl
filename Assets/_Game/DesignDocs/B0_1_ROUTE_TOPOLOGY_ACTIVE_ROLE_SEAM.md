# B0-1 Route Topology and Active-Role Seam

Status: `IMPLEMENTED / VERIFIED`

Date: 2026-07-21 KST

## Outcome

B0-1 proves that the existing run owner can admit a second route **shape** without
pretending that every stage is the Olympus Corridor-to-Station tutorial. It does not add
a second product scene, catalog row, or result path.

The bounded route contract now supports only these two shapes:

- one ordered segment that is both the exact run entry and the final `ReturnToOwner`;
- two ordered segments using the accepted `SingleLoad` or `InSceneAdvance` boundary,
  followed by one final `ReturnToOwner`.

This is intentionally not a general stage graph.

## Admission contract

`StageRunRouteSnapshot.TryCreate` now rejects a route before context creation unless all
of the following are true:

1. schema version is `1`, route identity is nonempty, revision is positive, and segment
   count is exactly `1` or `2`;
2. sequence indices are contiguous and segment/action IDs are unique;
3. the first condition is exactly `run.entry.admitted` with
   `RunEntrySnapshotValidatedAndFirstSegmentActivated`;
4. every adjacent exit/entry condition matches by ID and kind;
5. `SingleLoad` and `InSceneAdvance` occur only on a non-final row with their exact typed
   transition fields; `InSceneAdvance` shares the successor scene;
6. the final condition uses the existing queue/finalization semantic kind and the final
   row carries the exact `ReturnToOwner` typed-absence/owner/receipt shape; and
7. a non-Olympus route owns a distinct terminal condition ID and cannot reuse
   `station.encounter.terminal`.

The new validation adds no serialized role field and does not change canonical digest
field order.

## Active-role seam

`StageRunSegmentRole` is derived from the admitted immutable snapshot:

| Topology position | Derived role |
|---|---|
| first row | `Entry` |
| final `ReturnToOwner` row | `Terminal` |
| one-row entry/final | `Entry | Terminal` |

The lifecycle enum numbers remain unchanged. A non-final first row still activates as
`CorridorActive = 1`; a final current row activates as `StationActive = 3`. The old names
remain compatibility values, while terminal eligibility now comes from the current
segment plus final `ReturnToOwner` rather than a hard-coded Station position.

A directly admitted one-row route therefore starts segment zero terminal-active with:

- no pending handoff token;
- no segment-entry receipt; and
- no handoff-terminal receipt.

Same-scene replay is idempotent only when the first segment is active and both the route
snapshot digest and result/progression join digest match. Route and join preflight occurs
before idempotent reuse. A stale digest, stale join, malformed route, or foreign scene
leaves the existing context and run ID unchanged.

## Compatibility proof

The accepted Olympus revision-2 product route remains:

`CorridorActive -> same-scene segment entry -> StationActive`

Its in-scene transition still produces the accepted entry and handoff-terminal receipts;
B0-1 removes none of that evidence. The fixed identities remain:

- terminal policy digest:
  `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`;
- route digest:
  `878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0`;
- result/progression join digest:
  `d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71`.

The historical revision-1 route is also reconstructed in test with the separate Station
scene, `SingleLoad`, loader generation, and digest
`2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9`.
It remains readable historical evidence and is not reinterpreted as the current product
path.

## Verification

Final evidence:

- `C:\tmp\DimensionBrawl-B0-1-StageRunRoute-Identity2.xml`: `36/36`;
- `C:\tmp\DimensionBrawl-B0-1-RelatedRegression.xml`: `168/168`;
- combined related PlayMode total: `204/204`;
- `C:\tmp\DimensionBrawl-B0-1-PlayableStageValidator.log`: static product validator
  `PASS` with all three accepted Olympus digests above unchanged; and
- `git diff --check`: clean for the B0-1 code/test/document surface.

The positive admission fixture is not an Olympus alias. It builds the independent
in-memory identity `B0-ONE-ROW-TEST-01`, with matching route, result profile/catalog,
result definition, progression node/graph, reference, briefing, and join digests before
calling the production admission path.

The malformed matrix covers zero and three rows, duplicate segment/action IDs, sequence
gap, first-entry mismatch, adjacent-boundary mismatch, empty/wrong terminal condition,
successor on the final row, missing successor on `SingleLoad`, stale route/join digests,
foreign scene, and reserved Olympus terminal-ID reuse.

## Explicit deferrals

B0-1 does **not** prove that the one-row route can commit a truthful result. B0-2 must:

- represent tutorial absence with the existing tutorial-digest field empty rather than
  fabricating a completed tutorial;
- bind the combat collector to segment zero;
- seal one completed segment plus truthful combat/outcome facts;
- preserve the Olympus nonempty tutorial digest and guide-release gate;
- produce exactly one durable summary, receipt, and result presentation; and
- fail closed for collector, scene, or coordinator mismatch while preserving existing
  Olympus result bytes.

Neutral scene bootstrap/result adapters remain B0-3. Multi-entry catalog and build
enumeration remain B0-4. The product should not author B1's second scene until those gates
are green.

## Post-B0-2 update

The B0-1 deferral above is retained as the historical boundary of this milestone. B0-2
has since closed it: the one-row route now seals truthful segment-zero combat/outcome
facts, represents tutorial absence with the existing empty digest value, commits Clear or
Fail exactly once, and preserves the accepted Olympus path and fixed result-summary
digest. See `B0_2_TRUTHFUL_ONE_ROW_FACTS_RESULT.md`.

## Post-B0-3 update

B0-3 has since connected that truthful one-row context to exact neutral bootstrap, fact,
result, recovery, presentation, and action adapters. See
`B0_3_NEUTRAL_ONE_ROW_SCENE_ADAPTERS.md`. B0-4 multi-entry catalog, Stage Select, validator,
and build plumbing is now the next gate; the first compact second scene remains B1.

## Decision

B0-1 proves reusable admission topology and a neutral active-role seam. It does not claim
a second playable stage, truthful one-row result commit, multi-card selection, progression,
reward, or live-service breadth.
