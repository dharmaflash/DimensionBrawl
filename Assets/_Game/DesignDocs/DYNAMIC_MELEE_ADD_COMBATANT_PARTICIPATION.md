# Dynamic Melee Add Combatant Participation (A0)

Status: `IMPLEMENTED / VERIFIED`

Date: 2026-07-18

Canonical product scope: the existing Station `add-left` only

Verification state: the bounded A0 compile, prefab/route validators, behavior, lifecycle,
unload, Retry, canonical UI, and mobile hot-path rows recorded below passed. This record
does not claim the entire project test suite is clean.

## Executive decision

A0 closes the missing participation loop for the current runtime-spawned Melee Add before
the executor is generalized to multiple rows. The Add must be able to acquire and damage
the exact current-run player, and the player must be able to aim at the Add, without
changing who owns the stage result.

The ownership split is exact:

- `CombatEncounterController` remains the two-subject terminal owner for the exact player
  and authored boss;
- `StageCountOneEncounterExecutor` owns the independent lifetime of the runtime Add;
- the Add is a combat participant, never a terminal-stage subject;
- the authored boss remains in the player's authored target set for the entire Add
  lifetime;
- no result, reward, progression, navigation, catalog, scene, or stage-definition policy
  is added by A0.

The existing `SciFiSoldier.Melee` archetype now resolves to the dedicated game-owned
`PF_Enemy_SciFiSoldier_Melee_HeavyWindup` prefab. It reuses the existing reviewed
`DB_BasicSoldier_HeavyWindup` profile with `MeleeArc`, a null pattern deck, no projectile
driver, and attack range covered by the sensor. The shared ClosePunish projectile prefab
remains unchanged as the role-candidate source.

A0 is a local runtime capability, not a global combat manager and not the collection,
Encounter, or Wave executor reviewed by CF-01.

## Exact ownership contract

| Concern | Exact owner | A0 rule |
|---|---|---|
| Player and boss terminal outcome | `CombatEncounterController` | Exposes the exact player and boss health as read-only subjects; the Add is never appended |
| Add instance and lifecycle | `StageCountOneEncounterExecutor` | Resolves, prepares, activates, observes, stops, and disposes one exact owned Add |
| Enemy -> player participation | Add `ICombatAiAgent` plus its coherent `CombatTargetSensor` | Receives only the exact terminal player while the Add root is inactive |
| Player -> enemy participation | `PlayerCombatTargetSelector` | Stores the Add in a separate idempotent scene-local runtime candidate set |
| Authored boss targeting | Serialized authored candidate array | Remains intact; runtime registration never replaces or rewrites it |
| Inactive-object defense | `CombatHealth.BecameInactive` consumers | Purges runtime membership idempotently; it does not admit an encounter |
| Collider lookup cache | `CombatHealth` | Removes disabled, inactive, destroyed, and owner-matching bindings so Retry cannot retain stale targets |

`CombatHealth.ActiveInstances` is not an admission registry. Scene-wide hostile discovery
and the summon-frontline proxy registry are explicitly outside this contract.

## Activation transaction

The Add does not become active until both participation directions are prepared from exact
scene-owned subjects.

1. Resolve the exact terminal `Player` health, authored `Enemy` boss health, and the one
   player selector attached to the terminal player.
2. Require that the selector owns that exact player health and retains the authored boss
   candidate.
3. Instantiate the Add under an inactive owned root.
4. Require exactly one enabled, active-self `Enemy` health, exactly one
   `ICombatAiAgent`, one coherent `CombatTargetSensor`, and zero
   `SummonFrontlineProxy` components. The agent and sensor reference the same Add health,
   and the prefab sensor begins without authored target candidates.
5. While the root is inactive, inject the exact terminal player into the agent fallback
   and sensor candidate set.
6. Activate the root and require the agent and sensor to be active. Initial sensor refresh
   may remain empty while the player is outside the 12 m search radius.
7. Register the Add health in the player's runtime candidate set without touching the
   authored array. Only then may the executor report the Add as active. The sensor must
   acquire the exact terminal player before the agent may enter Windup or AttackActive.

Any failed step faults the transaction and enters the same cleanup path as every other
stop cause. A partially prepared Add must never remain visible, targetable, or attacking.

## Bidirectional participation invariants

- Enemy -> player: the owned sensor has the exact terminal player as its sole candidate.
  Its current target may be null outside search radius, but must be that exact player
  before Windup or AttackActive.
- Player -> Add: the selector can select the Add through normal target, melee aim, ranged
  aim-assist, and lock-target queries.
- Player -> boss: authored boss eligibility is preserved before, during, and after Add
  participation.
- Runtime candidates are additive and independently removable. Removing one candidate
  must not remove another runtime candidate or reset the authored target array.
- Candidates must be living, active, enabled, hostile, and in the same scene as the
  selector's self health.
- A disabled or inactive health is purged defensively and cannot be returned by collider
  resolution.

## Synchronous stop and cleanup

The same idempotent stop transaction applies to Add death, authored boss terminal,
executor fault, executor disable, continuous-scene unload, and Fail-to-Retry.

1. Reject late callbacks and snapshot the currently owned subjects.
2. Unsubscribe Add death and inactivity observation.
3. Clear and disable the Add sensor, then clear and disable the agent fallback target.
4. Make the owned Add root inactive synchronously.
5. Remove the Add only from the player's runtime candidate set, idempotently, even when
   inactivity observation already attempted the same removal.
6. Purge collider-to-health bindings that are null, disabled, inactive, destroyed, or
   owned by the stopped health.
7. Defer object destruction only after the hierarchy is already inert.

The immediate inactive step is the safety boundary. After boss terminal, fault, disable,
unload, or Retry begins, the Add must not finish a windup, apply later damage, retain a
telegraph, answer a lock query, or survive in an active hierarchy while deferred
destruction waits for the end of the frame.

Add death completes only the executor's independent receipt. Boss death or player death
still belongs exclusively to the terminal controller. If Add death and boss terminal occur
in the same frame, the Add's completed receipt is not rewritten as cancelled and the Add
still does not become a stage-clear owner.

## Validator gates

Static prefab validation must reject an A0 payload unless it has:

- exactly one enabled, active-self `CombatHealth` with team `Enemy`;
- exactly one `ICombatAiAgent` whose self health is that exact component;
- one coherent `CombatTargetSensor` whose self health is that exact component;
- an empty authored sensor candidate set before runtime injection;
- the exact existing `DB_BasicSoldier_HeavyWindup` profile with `MeleeArc` and a null
  pattern deck;
- attack range less than or equal to sensor search radius;
- zero `BasicSoldierProjectileAttackDriver` and zero `SummonFrontlineProxy` components.

Canonical scene validation must reject the route unless it has:

- exactly one terminal encounter owner;
- an exact `Player`/`Enemy` terminal pair in the same scene;
- exactly one player selector on the exact terminal player;
- selector self health equal to the terminal player;
- the exact terminal boss retained in the selector's authored candidate set.

Runtime guards must additionally preserve the active canonical run, scene lease, guide
release, exact instance cardinality, and one-owned-Add assumptions already held by the
count-one executor.

## Acceptance and verification ledger

Rows below report only inspected, bounded runs. `Passed` does not imply an unlisted full
suite or device-performance certification.

| Gate | Required direct observation | State |
|---|---|---|
| Editor compile and static validation | Unity compile exit 0; prefab reapply/validator exit 0; canonical `PlayableStageDefinitionValidator.ValidateOrThrow` exit 0 | `Passed` |
| Selector unit behavior | `DimensionBrawl-A0-SelectorCache-Final.xml`: 3/3, including authored/runtime coexistence, four query paths, inactive purge, and disable callback re-entry rejection | `Passed` |
| Collider cache lifecycle | The same focused run proves disabled/inactive/destroyed collider bindings purge and replacement binding refresh | `Passed` |
| Guide-release actual play | `DimensionBrawl-A0-StageCombat-CoreClean2.xml`: Add approaches, exact sensor acquires, HeavyWindup occurs, and real damage names Add health as source | `Passed` |
| Add death then same-frame boss terminal | Core run: one completion, zero cancellation, immediate registration removal/root inactivity, boss-owned result | `Passed` |
| Boss terminal during windup | Core run: one cancellation and zero delayed Add damage after the terminal call | `Passed` |
| Executor disable while Add lives | Core run: immediate cancellation, unregister, inactive hierarchy, released lease, and no respawn | `Passed` |
| Sensor lease loss while Add lives | `DimensionBrawl-A0-SensorLeaseLoss-Isolated.xml`: 1/1; faulted state performs the same inert cleanup without respawn | `Passed` |
| Continuous-scene unload while Add lives | `DimensionBrawl-A0-UnloadCleanup-Rerun.xml`: 1/1; shutdown probe observes cancelled ownership/participation/scene-lease cleanup before disable returns | `Passed` |
| Actual Fail-to-Retry | `DimensionBrawl-A0-RetryFreshRun-Rerun.xml`: 1/1; the terminal Add is retired in the result call, then Retry removes the old scene/run and admits a fresh WaitingForRun executor with zero ownership | `Passed` |
| Canonical/mobile regression | Canonical UI/Replay/Lobby/fixture `5/5`; mobile hot-path class `7/7` | `Passed` |

The test-only canonical Station shortcut waits for tutorial invulnerability to expire and
then normalizes `Time.timeScale` to 1 because reflection skips the physical stair walk and
its slow-motion restoration. Production timing and invulnerability values are unchanged.

Melee A0 does not require a projectile proof. Projectile fire, hit, expiry, and pooled
cleanup remain the first reviewed Ranged-loadout gate, not an implicit part of this slice.

## Ark structural evidence boundary

`CURRENT_GAME_CONTENT_GAP_DIAGNOSIS.md` records the bounded structural comparison used to
justify this ordering: spawned-instance ownership should remain separate from enemy
configuration and stage composition, and static scene arrays cannot own dynamic combatant
lifetime. That evidence supports closing bidirectional registration and cleanup before
multiplying spawn rows. It does not supply DimensionBrawl's exact selector, sensor,
terminal, Retry, or same-frame semantics; those are local product decisions documented
above.

Material under `\\DESKTOP-69817L3\ArkData` remains:

`PRIVATE REFERENCE / REVIEW NEEDED`

It may be used only for bounded structural review after provenance, rights, and product
fitness are confirmed. A0 copies no external code, assets, text, identifiers, timings,
balance values, layouts, media, or implementation details. No external generation-service
output is a runtime dependency or acceptance substitute. Future use must preserve the same
source-boundary record and independent DimensionBrawl naming and implementation.

## Explicit non-goals

- no second Add, collection executor, Encounter/Wave runtime, or CF-01 admission;
- one dedicated game-owned HeavyWindup prefab is added; no new scene component,
  pattern/design-data profile, or stage catalog row;
- no change to boss clear, player failure, result, reward, progression, save, or service
  ownership;
- no scene-wide target discovery or global enemy registry;
- no summon-proxy scoring or ordinary-enemy promotion into summon infrastructure;
- no ranged-projectile acceptance claim;
- no copied third-party presentation or combat content.

## Queued review-only presentation slices

The next presentation work is recorded as a queue, not admitted product authority.

### ST-01 — VN -> tutorial handoff

Review an explicit story-presentation completion/skip boundary that restores the existing
gameplay camera, HUD, input, listener, and time state before the current tutorial begins.
ST-01 may evaluate handoff clarity and restoration behavior only. Until a route-owned
product authority is approved, it must not mutate the canonical scene flow, tutorial
facts, result ownership, catalog, save, or progression.

A prerequisite terminal-lifecycle hardening lab is implemented and verified in
`NARRATIVE_TUTORIAL_REVIEW_TERMINAL_LAB.md`. It fixes false-success, stale-generation, and
owned-work cleanup behavior from the existing review tutorial into its review briefing.
It is not the ST-01 story-to-tutorial receipt or acceptance fixture. ST-01 remains open,
including gameplay camera, HUD, input, listener, time, and tutorial-start proof.

### VN-02 — reusable multi-character narrative presenter

Review a reusable presenter with a DimensionBrawl-owned speaker presentation catalog,
persistent left/center/right portrait state, expression selection, and inspectable
typewriter, auto, choice, log, and skip states. It must correct the current review
limitation where portrait presentation is cleared or hard-coded per line, but remains an
isolated review surface until an explicit story/route owner admits it. No external story
text, character art, voices, layouts, identifiers, or timing values are admitted by this
queue entry.

Neither queued slice authorizes generation-tool spending, browser automation, external
publication, canonical scene edits, or runtime admission by itself.
