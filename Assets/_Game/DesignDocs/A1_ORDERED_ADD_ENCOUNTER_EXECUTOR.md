# A1 Ordered Add Encounter Executor

Status: `IMPLEMENTED / VERIFIED`

Date: 2026-07-20

Canonical product scope: the continuous Olympus Station segment in
`OlympusCorridorInvasionStage.unity`

Post-A2 status, 2026-07-20: A1 remains the authoritative ordered-ticket and lifecycle
contract. The canonical `add-left` row remains `SciFiSoldier.Melee`; `add-right` now uses
the reviewed `SciFiSoldier.Ranged` RifleCrossfire loadout. See
`A2_RIFLE_CROSSFIRE_RANGED_LOADOUT.md` for the current roster, projectile ownership, and
latest verification. The equal-Melee table below is the historical A1 acceptance fixture.

## Outcome

A1 replaces the Station's scalar one-Add runtime assumption with one scene-local ordered
plan and one independently observable ticket for every authored `SpawnKind.Add` row.
Its initial acceptance proof contained exactly two equal-delay Melee rows:

| Source ordinal | Spawn | Position | Anchor | Binding-local pose | Payload | Count | Delay |
|---:|---|---:|---|---|---|---:|---:|
| 0 | `add-left` | 2101 | `Add_LeftLaneAnchor` | `(8.9, 0, -1.25)` | `SciFiSoldier.Melee` | 1 | 0 |
| 1 | `add-right` | 2102 | `Add_RightLaneAnchor` | `(8.9, 0, 1.25)` | `SciFiSoldier.Melee` | 1 | 0 |

At the A1 acceptance cutoff, both rows directly referenced the reviewed
`DB_Archetype_SciFiSoldier_Melee` asset. The archetype retains the game-owned
`PF_Enemy_SciFiSoldier_Melee_HeavyWindup` prefab, exact HeavyWindup pattern, null deck,
`MeleeArc`, one Enemy health, one coherent agent/sensor pair, no projectile driver, and no
summon proxy.

The current canonical product keeps the same order, anchors, count, delay, and ticket
semantics but promotes only `add-right` to `SciFiSoldier.Ranged`. That post-A1 content
change exercises the same collection executor; it does not revise the A1 ownership model.

The class name `StageCountOneEncounterExecutor` remains for serialized scene identity and
compatibility. Its runtime semantics are now collection-based. Legacy scalar observation
properties return the first relevant ticket only and are not authoritative acceptance
surfaces.

## Exact ownership boundary

| Concern | Owner | A1 rule |
|---|---|---|
| Ordered Add rows and payload identity | `StageDefinitionProfile.SpawnRef` | Source order is authoritative; every Add owns a direct archetype reference plus stable payload ID |
| Static pose | `StageDefinitionProfile.AnchorRef` | One exact anchor per Add row |
| Live pose | `StageDefinitionSceneBinding` + `StageAnchorPoint` | One exact scene-local match, validated in binding-root coordinates |
| Plan/ticket lifetime | `StageCountOneEncounterExecutor` | One scene lease, inactive transactional staging, activation, ticket-local death, cancellation/fault cleanup, typed receipt |
| Player targeting of Adds | `PlayerCombatTargetSelector` runtime candidates | Each live Add is registered and removed independently; authored boss membership is preserved |
| Add targeting of player | Ticket agent and sensor | Exact current-run terminal player only |
| Boss/player terminal outcome | `CombatEncounterController` | Unchanged exact player/boss authority; Adds never own Clear or Fail |
| Result, navigation, progression, reward | Existing route/result owners | No A1 mutation or authority |

## Authoring and admission rules

The executor enumerates `StageDefinitionProfile.GetSpawn(index)` in serialized order and
selects every `SpawnKind.Add`. A malformed Add faults the complete plan; it is never
silently skipped.

Every admitted Add row requires:

- nonempty unique spawn and anchor IDs;
- a positive unique position ID;
- raw authored `count == 1` before any clamped convenience getter;
- finite nonnegative raw delay, nondecreasing without tolerance;
- exactly one matching static anchor and one matching live `StageAnchorPoint`;
- finite binding-root-local pose matching the static anchor;
- `CombatSpawn` usage and `Add` spawn semantics;
- a direct archetype whose stable ID equals `payloadId`;
- a promoted game-owned gameplay prefab satisfying the A0 participation contract.

The deterministic editor setup authors the exact two-row canonical fixture. The runtime
executor itself is collection-based, but changing canonical cardinality intentionally
requires an editor-validator/test fixture revision. This prevents an unnoticed third row
from silently changing shipping combat composition.

## Lifecycle

1. Resolve the exact current Station run, scene binding, guide, terminal player/boss,
   selector, all Add rows, anchors, archetypes, and prefab contracts.
2. Acquire one loaded-scene executor lease.
3. Instantiate every ticket under its own inactive root. Prepare its health, agent,
   sensor, player target, death callback, and spawn pose while no ticket is visible or
   registered with the player selector.
4. Arm one activation epoch when `CombatEntryGuideReleased` is observed. Interpret each
   row delay relative to that epoch. Equal due times activate in source order.
5. Activate and register each due ticket. An exception or failure during any ticket
   activation faults the plan after the activation guard exits, cancels staged/active
   peers, and synchronously returns the plan to quiescence.
6. A ticket death completes and cleans only that ticket. Other Adds and the authored boss
   remain alive and targetable. The plan completes exactly once only after every ticket
   completes.
7. Boss terminal, player terminal, active-run loss, executor disable, scene unload, Retry,
   or a participation fault synchronously cancels/faults unfinished tickets, clears
   sensors/agents, deactivates roots, unregisters candidates, and rejects late callbacks.

Completed ticket history is retained when a later terminal boundary cancels unfinished
tickets. A completed plan receipt is not rewritten when the boss subsequently reaches its
own terminal outcome.

## Observation and receipt contract

Authoritative runtime observation uses:

- `TicketCount` and pending/active/completed/cancelled/faulted counts;
- `ActivatedTicketCount`, `ActiveParticipationCount`, and `OwnedObjectCount`;
- `GetTicketSnapshot(index)` for source identity, state, activation/terminal sequence,
  owned subjects, participation, and spawn pose;
- `LastReceipt` and `StageAddEncounterPlanReceipt.TryValidateIntegrity`;
- `IsQuiescent` for verified zero owned hierarchy and zero participation.

Receipt integrity rejects:

- nonterminal plan states, invalid ticket enum values, duplicate identities, decreasing
  delays, or non-source-ordered records;
- a completed ticket without activation;
- activation/terminal/close sequence inversions;
- active or pending terminal records;
- any record whose target registration was not released or hierarchy was not made inert;
- a Completed plan containing a non-completed ticket;
- a Cancelled plan with no cancelled ticket or any faulted ticket;
- a Faulted plan with authored tickets but no faulted ticket.

Cleanup does not erase ownership merely because an operation was attempted. If hierarchy
deactivation or candidate removal cannot be verified, references and participation state
remain observable, `IsQuiescent` remains false, and the receipt cannot claim clean closure.

## Verification ledger

All rows below were run against the canonical project on 2026-07-20.

| Gate | Evidence | Result |
|---|---|---|
| Deterministic setup and compile | `DimensionBrawl-A1-Setup-2.log` | `BATCH_SETUP_PASS` |
| Playable definition/scene/route validator | `DimensionBrawl-A1-DefinitionValidator.log` | `PASS` |
| Shared-map and anchor spatial audit | `DimensionBrawl-A1-SpatialAudit.log` | `SPATIAL_AUDIT_PASS` |
| Inactive two-ticket staging before guide | `DimensionBrawl-A1-Staging.xml` | 1/1 passed |
| Later-ticket activation failure rollback | `DimensionBrawl-A1-Rollback.xml` | 1/1 passed |
| Real AI participation and independent two-ticket completion | `DimensionBrawl-A1-Completion.xml` | 1/1 passed |
| Full stage-run route class | `DimensionBrawl-A1-StageRunRoute.xml` | 31/31 passed |
| Full canonical UI route class, including exact authoring and Retry | `DimensionBrawl-A1-CanonicalUi.xml` | 34/34 passed |
| Mobile runtime hot path | `DimensionBrawl-A1-MobileHotPath.xml` | 7/7 passed |

The three focused tests are contained in the 31-test route class. The non-duplicated broad
regression total is therefore 72/72.

Direct lifecycle coverage includes inactive staging, equal-delay source order, first
death preserving the second Add and boss, exact-once plan completion, boss terminal,
executor disable/re-enable, explicit run loss, later-ticket sensor fault, scene unload,
legacy standalone rejection, and Fail-to-Retry replacement with no stale ownership.

## ArkData structural evidence and copy boundary

The following private review paths informed only the general separation and ordering
decision:

- PGR supplemental stage/runtime notes and candidate tables under
  `SubcultureGameData/games/punishing-gray-raven`, plus reviewed lifecycle functions in
  `XDlcCSharpFuncs.lua`, `XFightBase.lua`, `XPlayerNpcContainer.lua`,
  `XFightResultJudge.lua`, and `XRelinkMonsterBase.lua`, support separating configured
  spawn identity from load/activation/death lifecycle.
- Aether Gazer's reviewed `aether-gazer-stage-topology-wave-context` tables and
  `ActivityReforgeLevelCfg.lua`, `ActivityReforgeWaveCfg.lua`, and
  `ActivityStrongholdCfg.lua` expose an ordered `wave_list` relationship. This supports
  preserving order, not importing a Wave runtime into A1.
- HI3's reviewed direct stage read-first data and `StageData_Main.json` support keeping
  retry/stage policy separate from enemy-instance lifetime.
- Snowbreak's reviewed combat task/runtime pack supplies supporting FSM separation only;
  it is not authoritative for A1 lifecycle semantics.

All material under `\\DESKTOP-69817L3\ArkData` remains
`PRIVATE REFERENCE / REVIEW NEEDED`. A1 copies no external code, IDs, values, stage
composition, timings, UI, art, audio, text, layouts, or implementation details. Source
ordinal, guide-release epoch, ticket states, cleanup order, typed receipts, boss
separation, and all exact fixture values are independent DimensionBrawl product decisions
proven by local code and tests.

## Explicit non-goals

- no Encounter/Wave schema, batch scheduler, quiet window, retry policy, or condition DSL;
- no admission of the review-only CF-01 `StageEncounterPlanProfile` or review session;
- no multi-instance `count > 1` placement policy;
- at the A1 cutoff, no Ranged archetype claim, projectile acceptance, enemy-role
  promotion, or balance work; A2 later admitted one narrow Ranged loadout without changing
  this executor contract;
- no change to boss terminal, result, catalog, reward, progression, save, or service UI;
- no external generation-service dependency and no copied third-party content.

## Next bounded decision

A1 removed the runtime collection blocker, and A2 has now admitted one truthful Ranged
loadout with projectile fire, hit, expiry, repeated-fire, terminal, scene-exit, and Retry
cleanup evidence. B0-1 has since closed the bounded route topology and active-role seam;
B0-2 has also closed truthful one-row facts/result commit without fake tutorial or handoff
evidence, and B0-3 has connected that result core to neutral bootstrap, fact, recovery,
presentation, and action adapters. The next product gate is B0-4 catalog/build plumbing.
Only then should
the compact second playable stage choose whether it needs a minimal required-defeat owner.
CF-01 remains a useful authoring review, not an implementation shortcut.
