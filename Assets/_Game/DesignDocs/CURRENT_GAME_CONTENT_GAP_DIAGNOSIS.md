# Current Game Content Gap Diagnosis

## 2026-07-21 milestone update

Phase 0, A0, A1, A2, B0-1, B0-2, and B0-3 are now implemented and functionally verified.
The bounded route layer now admits either one or two ordered logical segments, derives
entry/terminal roles from topology, and admits an independent one-row entry/final route
as terminal-active without fabricating handoff evidence. That route now also seals
truthful segment-zero facts and commits Clear/Fail with an empty tutorial digest, one
durable exact-run result, and one presentation. Neutral scene adapters now bind the exact
bootstrap, encounter, fact, result, same-process recovery, overlay acknowledgement, and
Replay/Retry/Lobby boundaries without copying Olympus scene components.
The route is continuous in one physical scene; dynamic Adds participate in both
directions; the Station executor consumes the exact ordered `add-left` and `add-right`
rows with direct archetype ownership, transactional inactive staging, ticket-local death,
whole-plan cleanup, and typed receipts. The left row remains HeavyWindup Melee, while the
right row now uses the dedicated RifleCrossfire physical-projectile Ranged loadout. Current
evidence is recorded in `A1_ORDERED_ADD_ENCOUNTER_EXECUTOR.md`,
`A2_RIFLE_CROSSFIRE_RANGED_LOADOUT.md`,
`B0_1_ROUTE_TOPOLOGY_ACTIVE_ROLE_SEAM.md`,
`B0_2_TRUTHFUL_ONE_ROW_FACTS_RESULT.md`, and
`B0_3_NEUTRAL_ONE_ROW_SCENE_ADAPTERS.md`; the dated diagnosis below remains useful as
the reasoning record but its scalar/missing-A1/A2/B0-1/B0-2/B0-3 observations are superseded.

The A2 loadout is functionally verified but still awaits human mobile-landscape visual
review. The next bounded product gate is B0-4 multi-entry catalog/build plumbing. After
B0, build one compact
second playable stage and introduce only the required-defeat composition that stage
actually needs. CF-01 remains review-only and is not a runtime shortcut.

Date: 2026-07-16
Status: read-only diagnosis and lightweight living backlog; not an implementation contract

## Executive decision

At the 2026-07-16 diagnosis cutoff, the first problem was a copied-scene break in the
intended Olympus play space, followed by scalar Add execution and missing target
participation. Phase 0, A0, A1, and A2 have since closed those problems. B0-1 through B0-3
have closed the route topology, active-role, truthful one-row facts/result, and neutral
scene-adapter seams. The current first problem is the remaining multi-entry catalog/build
seam for a second playable stage. Begin with B0-4 and do not add another
broad contract layer or live-service system.

## Current playable truth

| Surface | Current product truth |
|---|---|
| Selectable content | One catalog entry: `story_v1_training_route` |
| Playable route | One route: `OLYMPUS-INVASION-01` |
| Physical combat space | The admitted route now keeps tutorial and lower combat in `OlympusCorridorInvasionStage`; the legacy Station scene remains in the build list but is not the continuous product transition |
| Build scenes | Six total: four UI scenes and two stage scenes |
| Tutorial | Intro presentation plus melee, move, ranged swap, fire, dodge, and target-clear sequence |
| Station encounter | Entry guide, one boss terminal owner, and two independently owned runtime Adds after guide release: `add-left` HeavyWindup Melee plus `add-right` RifleCrossfire Ranged; bidirectional targeting, exact physical projectile damage, terminal cleanup, unload, and Retry are functionally verified; human visual review remains pending |
| Result loop | Shared Clear/Fail result shell with Replay/Retry/Lobby and durable run-result handling |
| Progression/reward | Definitions and join data exist, but no player-facing unlock/reward application loop is admitted |

The Station Adds require the exact active canonical Station run and activate after guide
release. Each ticket owns one Enemy `CombatHealth`, injects the exact terminal player into
its agent/sensor, registers independently with the player's target selector without
replacing the authored boss, and becomes synchronously inert before deferred destruction.
Current integration tests prove real acquire/attack/damage, independent death, plan
completion, boss/player terminal, driver/participation faults, disable, unload, and Retry.
They are proven active threats but remain independent participants: only the authored boss
and player own Clear/Fail. A2 presentation readability still awaits human visual review.

## Inventory breadth is not playable breadth

The repository contains six stage templates, eight segment profiles, nine enemy archetypes, twenty role/role-deck assets, and twelve role-candidate assets. The canonical product still consumes one playable route and one selectable catalog row.

- The five `S1_*` templates are not referenced by a runtime product asset; they are referenced by a local preflight document.
- The eight legacy segment profiles feed those non-canonical templates and static supplements, not the active Olympus route.
- All twelve enemy-role candidate assets have zero downstream product references.
- `DB_Archetype_SciFiSoldier_Melee` and the dedicated
  `DB_Archetype_SciFiSoldier_Ranged` loadout are directly consumed by the live Station Add
  path. Other role/archetype graphs remain candidate or prototype inventory.

This distinction must remain explicit: authored candidates are useful raw material, but they are not content until the playable route consumes them.

## Why content work became slow

### 1. The Add executor is now reusable, but the route shell is still product-specific

The current `StageCountOneEncounterExecutor` enumerates every authored `SpawnKind.Add` row
in source order, resolves each row's direct archetype, stages ticket-local objects
transactionally, injects bidirectional participation, activates relative to one guide
epoch, records independent death, and closes the whole plan on terminal/fault/unload. It
still permits one plan executor lease per loaded scene and remains coupled to the exact
Olympus Station active-run/guide/terminal subjects.

A1 proved that a second count-one Add could flow through the same executor and scene
binding without a second runtime implementation. A2 then changed the right row's direct
archetype/loadout while retaining the same ticket lifecycle. The remaining cost problem is
not scalar Add execution; it is that a second playable route still cannot reuse a neutral
entry/final lifecycle, fact/result collector, terminal adapter, catalog, and build path.

Target cost for an encounter made from an existing enemy is:

- one logical spawn authoring operation in the stage definition, with its paired `SpawnRef` and `AnchorRef` kept consistent by tooling or validation;
- one scene anchor and binding;
- zero runtime code changes.

A0 now supplies the stage-owned combatant registration path: every admitted runtime Add
receives the exact player target, appears in the player's runtime candidates, and
unregisters from both directions on death, cancellation, unload, or Retry. Static authored
arrays remain the boss baseline and are never replaced.

### 2. The run lifecycle is route-specific

`StageRunRuntime` now admits the corrected same-scene Olympus transition, but its active
roles, terminal/fact construction, bootstrap, and validators still contain Corridor and
Station identities. A new one-scene stage cannot reuse that lifecycle as a neutral
composition until B0 separates entry/final roles from those product names.

### 3. Validation and documentation outgrew the content

Observed current sizes:

- StageRun runtime: 8 files, 7,161 lines;
- result-presentation runtime: 4 files, 2,126 lines;
- route definition runtime: 708 lines;
- route validator: 2,184 lines;
- Corridor tutorial director: 2,085 lines;
- two main route/UI PlayMode test files: 5,317 lines;
- DesignDocs: 157 files, including 90 `P1B_*` helper artifacts;
- current gap roadmap: about 1,500 lines.

These systems protect valuable behavior, but their volume is disproportionate to one playable route. Future work should use proportional tests and one milestone note, not repeated multi-document certification.

### 4. A second, disconnected PVE architecture remains

`PveStageData` and `PveEncounterDirector` already describe three trigger-based encounter groups with enemies, structures, projectile emitters, delays, and clear-to-advance behavior. No current scene, prefab, or product asset references their scripts. They are a legacy prototype, not the canonical stage executor.

Do not merge both systems or revive the prototype implicitly. Preserve it only as a reference until the canonical executor covers the useful composition semantics, then remove or move it to an explicit Prototype boundary.

### 5. The working tree has no small stable delta

At this diagnosis point the repository reported 197 untracked files, 38 modified files, and 4 deleted files. This includes much of the route/result implementation. Before the next implementation slice, create an explicit user-approved checkpoint so a content experiment can be measured and reverted without mixing it with the full optimization history. This is repository hygiene, not a new acceptance gate.

## Ark comparison: common structure worth adopting

The bounded comparison used curated PGR, Honkai Impact 3rd, Aether Gazer, Wuthering Waves, and ZZZ read-first/rollup material. The reusable consensus is structural:

1. A stage is a composition record over map, ordered encounters/waves/tasks, goals, restrictions, result/progression references, and presentation cues.
2. An encounter is a reusable payload: enemy identities/roles, placements, order, activation, completion policy, and modifiers.
3. Difficulty, practice, challenge, cost, reward, and character restrictions are wrappers over the same combat core, not duplicated scenes.
4. Camera, HUD, VFX, audio, and input-lock changes are event-bound cues with explicit restoration.
5. Tutorial and practice are content variants over the same action observations.
6. Daily/event/live-operation surfaces come after the offline stage-combat-result loop is worth repeating.

Only these separations are applicable. Foreign values, assets, identifiers, code, and large registry processes are not product inputs.

## Capability gap matrix

The labels below are product decisions, not certification phases. `Have` means the current playable route already proves the capability; `Partial` means a working example exists but adding another one still needs route-specific code or scene work; `Missing` means no player-facing product path exists yet.

| Capability | DB status | Local evidence | Ark structural comparison | Decision | Effort / dependency / main risk |
|---|---|---|---|---|---|
| Combat feel, terminal ownership, result recovery, Replay/Retry/Lobby | **Have** | The Corridor-to-Station route, boss terminal owner, durable result, and shared result shell are all exercised by the canonical route | HI3 and PGR stage surfaces also separate the combat outcome from score/reward/navigation presentation | **Preserve**. Reuse this shell for every new stage | S per consumer / avoid changing the stable core while generalizing content |
| Canonical spatial continuity | **Have** | Tutorial and lower combat now remain in `OlympusCorridorInvasionStage` through an in-place logical segment transition with the same player/camera/HUD ownership | Compared stage games commonly separate content phases from map identity; no foreign design is needed to justify preserving one local physical space | **Preserve** the corrected route and accepted identities | S regression per route change / accidental scene reload or duplicate owners |
| Ordered encounter and wave composition | **Have for ordered count-one Adds; Wave owner missing** | Station consumes two source-ordered direct-archetype Add rows with independent tickets, whole-plan rollback/cleanup, and typed receipt | PGR separates stage config from wave/spawn runtime; Aether Gazer stores ordered wave lists; ZZZ separates floors, groups, members, and placement | **Preserve A1**. Add a Wave/required-defeat owner only when a real second-stage design needs it | M when required / objective and terminal boundary / promoting CF-01 wholesale |
| Dynamic combatant registration and target participation | **Have for admitted runtime Adds** | Both Station Adds receive the exact player through agent/sensor injection, join the player's runtime candidates without replacing the boss, attack, and unregister synchronously on every stop path | Foreign enemy/stage data supports separating spawned instance ownership from enemy configuration, but does not supply a local target-registry design | **Preserve A0** and reuse the exact participation contract per new ticket | S per loadout / stale candidates, late attacks, boss target loss |
| A second encounter authored without code | **Have for a second count-one Add** | A1 admitted `add-right` through the existing collection executor and bound anchor; A2 later changed only its reviewed payload/loadout contract | The compared games repeatedly reuse enemy payloads and placements under different stage records | **Preserve the zero-new-executor path**; do not confuse a second Add with a second playable stage | S / promoted archetype and anchor / unreviewed loadout assumptions |
| Neutral playable-stage lifecycle and second catalog entry | **Partial; B0-1 through B0-3 complete** | A distinct one-row route admits terminal-active, seals truthful segment-zero Clear/Fail facts, recovers/presents the exact result through neutral adapters, and resolves Replay/Retry/Lobby; multi-entry catalog, Stage Select, validator, and build plumbing remain | PGR stage rows combine control/map/story/time/reward references; HI3 and Aether reuse stage shells with different restrictions and goals | **B0-4 next** before scene authoring | M across remaining B0 / preserving Olympus bytes and historical receipts / mutating frozen single-entry assumptions |
| Reusable map or lean arena authoring | **Partial inventory, missing product path** | The two stage scenes are about 13.7 MiB/5,102 GameObjects and 8.9 MiB/4,344 GameObjects; `scenePrefabSource` is empty. The project does have promoted modular Olympus Temple and Spring Isles environment prefabs | PGR stage rows reference stage controls/maps; Aether wave-stage rows reference `map_id`; ZZZ separates stage-room and map families | **Next**, with the second stage. Build a lean arena from promoted modular prefabs; do not duplicate either full canonical scene | M / lighting, collision, NavMesh, camera bounds / scene-copy maintenance or a visually empty proof |
| Objective and completion policies | **Partial** | Current product proves tutorial closure and boss terminal; the Add is independent and does not block clear | Aether exposes task/star conditions; HI3 exposes score/time/reward conditions; PGR exposes time limits and stage controls | **Next**. Start with `independent`, `required defeat`, `survive`, and `timed`; no general quest language | M / generic encounter lifecycle / combinatorial rules and unclear ownership |
| First-clear unlock and persistent local progression | **Partial model, missing execution** | One progression node exists, with no prerequisite, recommended-next, reward, or applied unlock | Aether uses unlock-by-stage links; HI3 uses pre-mission/unlocked-link relations | **Next**, with the second stage. First clear unlocks one next entry and survives restart | M / second catalog entry / save migration and accidental relock |
| Enemy role, stat, or behavior variants | **Have one reviewed variant; broader inventory unadmitted** | The live Station consumes `SciFiSoldier.Melee` and the dedicated `SciFiSoldier.Ranged` RifleCrossfire loadout; other role/candidate assets remain inventory | HI3 separates monster AI/stat data; Aether and ZZZ reuse enemy identities inside different stage compositions | **Preserve A2; do not promote the role layer wholesale**. Close mixed-deck/driver admission before any future Elite promotion | S-M per reviewed loadout / visual readability and candidate-deck compatibility |
| Difficulty, affix, and restriction wrappers | **Missing** | Current route explicitly leaves rule set, modifier, enemy variant, course, and reward cohorts unadmitted | HI3 exposes stage condition/buff/difficulty; Aether exposes difficulty, team/hero restrictions, revive and affix-like surfaces | **Later**. Add one modifier to an existing stage, not a duplicate scene | M / objective and variant model / balance matrix growth |
| Behavior-observed tutorial and practice flow | **Partial, strong example** | The Corridor observes melee, movement, ranged swap, fire, dodge, and target clear, but order, copy, locks, and observations are coupled to a 2,085-line director | PGR separates guide stages, groups, steps, overlays, teaching activities, and practice skill details | **Later**, after the second stage. Extract lesson order/copy/audio while preserving current observation adapters and feel | M / neutral lesson profile / tutorial regression and premature framework design |
| Intro cinematic and gameplay handoff | **Have, route-specific** | `StageCutscenePort` plus the Corridor flow own PlayableDirector stop/skip, camera restoration, HUD enable, and gameplay handoff | PGR camera recipe cards and Aether transition data separate shot/cue/transition lifecycle | **Preserve**. Only prove the same port can be reused; do not replace it | S-M / second presentation consumer / camera, listener, input, or time-state restoration |
| Event-bound camera, telegraph, VFX, audio, and HUD cues | **Partial, many hooks** | Action camera/cinematic cue profiles, telegraphs, screen cues, audio pooling, and battle HUD exist; actual scene usage is inconsistent, including ranged impact enabled in Station but disabled in Corridor | ZZZ binds camera actions to abilities; PGR and Wuthering bind camera/shake/audio/UI to action events; Aether stores transition lifecycles | **Now/Next tuning**, not a new framework. First normalize damage impact and add spawn/terminal cues | S-M / reviewed cue profile and device check / double playback, clutter, motion sickness, fill-rate |
| Result presentation and localization | **Have** | The result profile renders Clear/Fail, total/combat time, qualified proofs, actions, and ko-KR/en-US text | HI3 and PGR extend this surface with score and reward, but the separation itself already exists locally | **Preserve**. Add fields only when progression really produces them | S / new stage profile or reuse / placeholder rewards becoming false product claims |
| Accessibility controls | **Missing or unowned** | Intro skip exists, but no canonical owner was found for camera-shake/flash scale, reduced motion, color support, or subtitle/voice preferences | The compared presentation datasets consistently expose camera, HUD, post-process, timing, and dialog as separable surfaces | **Later, before content breadth becomes large**. Start with shake/flash/reduced-motion scales | M / cue ownership / inconsistent application across old scenes |
| Daily/event/stamina/shop/gacha/PvP/server operations | **Deferred** | No offline core need justifies these systems today | Ark contains many such surfaces, but they sit above a repeatable stage-result-progression loop | **Defer explicitly** until the offline game is worth replaying | L-XL / economy, backend, operations / identity drift and permanent maintenance cost |

The common pattern is not that DimensionBrawl needs more managers. It already has unusually rich combat and presentation hooks for its content volume. The gap is the **data-to-playable-content path** between those hooks and a second, third, and fourth stage.

### Evidence confidence and limits

- PGR, HI3, Aether Gazer, Wuthering Waves, and ZZZ material is curated read-first or rollup evidence, not an authoritative reconstruction of shipped runtime.
- PGR gives the clearest separation of guide, stage, wave/spawn, camera, and UI-flow concepts.
- HI3 gives the clearest evidence for stage condition/buff, enemy AI/stat, score, time, reward, and prerequisite surfaces.
- Aether Gazer gives useful ordered-wave, task, restriction, and topology structure, but not authoritative exact spawn placement.
- ZZZ supports floor/group/member/placement and action-camera separation, but some field meanings are obfuscated.
- These sources justify separation of responsibilities only. They do not justify copying content counts, balance values, identifiers, assets, or code.

## Phase A/B composition decision

This section turns the comparison into the smallest useful implementation backlog. It is a decision aid, not a frozen schema.

### Historical A0/A1 diagnosis baseline

At the 2026-07-16 diagnosis cutoff, `SpawnRef` already carried `spawnId`, kind, position ID,
anchor ID, payload ID, count, delay, and a note, while `AnchorRef` separately carried static
pose identity. The missing capability at that cutoff was a runner that could consume more
than one authored row safely.

The pre-A1 executor was scalar in every important place:

- one serialized `spawnId`;
- one resolved spawn, anchor, prefab, root, and health component;
- one activation delay;
- one scene-local payload mapping list;
- exactly one `Add` with `Count == 1`;
- Station-specific active-run validation and one executor lease per scene.

A0/A1 superseded the participation, scalar-resolution, payload-mapping, and collection
gaps in this list. The one scene-plan lease and Station-specific active-run/guide/terminal
dependency remain current and feed B0's route-neutralization priority.

The separate `AnchorRef` means “one spawn row plus one anchor” is not literally one backing row today. The practical product target is therefore **one logical authoring action in the definition asset, one scene anchor/binding, and zero runtime code**. An editor helper may maintain the paired definition rows later, but it is not required for the first runtime proof.

### Long-term common structure

Ark evidence supports four ownership layers:

```text
Stage / map / goal
  -> ordered encounter
    -> ordered wave
      -> spawn payload + placement
```

PGR separates stage config, wave/spawn runtime, and enemy runtime. Aether Gazer directly shows `level -> ordered wave IDs -> wave-stage row`, while explicitly not proving exact enemy placement. HI3 separates stage conditions from enemy AI/stat data. ZZZ and Wuthering support separating stage-room/map, placement, and trigger/action lifecycles, with weaker or obfuscated semantics.

This is the correct destination, but not the correct first implementation size.

### Phase A0 — historical acceptance plan (implemented)

This subsection preserves the pre-implementation A0 acceptance plan. The current verified
contract is `DYNAMIC_MELEE_ADD_COMBATANT_PARTICIPATION.md`.

Before multiplying spawn rows, close the missing player↔enemy participation loop for `add-left`.

- Resolve the exact current-run player health and player target selector from one scene-owned combatant registry or equivalent explicit binding; do not use a scene-wide hostile search.
- On Add activation, give its `CombatTargetSensor` the player and add the Add health to the player's candidate set without dropping the authored boss.
- On Add death, terminal cancellation, executor fault, disable, unload, and retry, remove both registrations and reject late callbacks.
- Keep target registration separate from terminal ownership: the Add may attack and be targeted while remaining independent from boss clear.
- Prove actual motion/telegraph/damage and actual player aim/contact, not only `CombatHealth` creation and programmatic lethal damage.

This is a small missing runtime capability, not a reason to introduce a global combat manager. The same registry must then be reused by the collection executor.

#### A0 local ownership decision

The smallest safe current-product design needs no new scene component, prefab, or asset:

1. `CombatEncounterController` remains the exact two-subject terminal owner and exposes read-only player/boss health. The Add never becomes a terminal subject.
2. `PlayerCombatTargetSelector` retains its authored boss array and owns a separate idempotent, scene-local runtime-candidate list. Registering one Add must not replace the authored array or reset another Add.
3. While the new root is still inactive, the executor validates exactly one Enemy health, one `ICombatAiAgent`, and its coherent `CombatTargetSensor`; it injects the exact terminal player into both the agent fallback and sensor. It activates the root only after that preparation, then registers the Add with the player selector.
4. Death, boss terminal, fault, disable, unload, and retry use one synchronous stop step: unregister the Add, clear agent/sensor targets, make the owned root inactive, and only then defer destruction. `CombatHealth.BecameInactive` is a defensive idempotent purge signal, not implicit encounter admission.
5. Player summon proxies keep their existing special registry and bonus. An ordinary enemy must not become a `SummonFrontlineProxy`, and `CombatHealth.ActiveInstances` must not make every same-scene hostile an implicitly admitted target.

Two tempting shortcuts are rejected. Calling `ConfigureTargetCandidates` for each spawn would replace the entire player array and can drop the boss. Generalizing the global summon registry would mix summon/frontline scoring with ordinary enemies and create cross-scene lifetime risk.

This slice is expected to touch seven existing files and no scene/prefab/data asset: the executor, player selector, terminal controller read-only surface, `CombatHealth` collider-cache purge, the existing validator, and two existing PlayMode test files. Runtime work is **S**; behavior/readability and adversarial verification make the bounded slice **S-M**. The cache purge belongs here because lock/aim resolution populates a static collider binding cache that currently is not removed on health disable/destroy, while Retry is already a supported product loop.

#### A0 bounded evidence

| Direct row | Required observation |
|---|---|
| Guide release | The exact Add sensor acquires the exact terminal player; the authored boss remains eligible; a narrow lock/aim query can select the Add; distance decreases; windup/telegraph occurs; `CombatHealth.Damaged` records the Add as the exact source of real player health loss |
| Add death, then same-frame boss terminal | One `Completed` receipt, zero cancellation, immediate target removal/root inactivity, boss terminal cannot rewrite the executor result |
| Boss terminal during Add windup | One `Cancelled` receipt, immediate target/telegraph/hierarchy stop, and zero later same-frame Add damage; subsequent disable/death is idempotent |
| Executor disable while living | Immediate cancellation, unregister, inactive hierarchy, released lease, and no respawn after re-enable |
| Continuous scene unload while living | One run abort owner, no old Add in active health or selector membership, no owned hierarchy, and no stale collider binding |
| Actual Fail-to-Retry | Cleanup completes before result presentation; fresh Corridor contains no old registration; the later fresh lower-segment entry registers exactly one new Add without cumulative cache growth |

The Melee A0 proof does not need a projectile assertion. Exact projectile fire, hit, expiry, and pooled cleanup belong to the first Ranged loadout smoke in A2.

### Phase A1 — historical acceptance plan (implemented)

This subsection preserves the pre-implementation A1 acceptance plan. The current verified
contract is `A1_ORDERED_ADD_ENCOUNTER_EXECUTOR.md`.

Generalize the current executor only far enough to consume all eligible `Add` rows from the bound stage definition while preserving their source ordinal.

- Keep one executor and one scene lease.
- Keep `Count == 1` per row. Multiple enemies require multiple rows and anchors; do not stack instances at one transform or invent formation offsets.
- Keep one current activation boundary, `CombatEntryGuideReleased`, and interpret each row's `delaySeconds` relative to that release. Require Add-row delays to be nondecreasing in serialized order; equal-delay rows activate in source order in the same frame.
- Keep every row `Independent` from boss clear in this phase. Executor completion is bookkeeping, not stage-terminal authority.
- Validate the entire plan before visible activation. Stage every runtime instance under an inactive ticket root; a later row failure leaves no active or registered enemy and rolls back every staged object.
- Replace scalar runtime ownership with per-`spawnId` tickets/collections while retaining exact enemy-health cardinality.
- Register every live ticket with the Phase A0 combatant set and unregister it independently. Removing one enemy must preserve the boss and every other live Add as valid targets.
- Preserve exact active-run/scene ownership, inactive staging before activation, death observation, same-frame terminal safety, disable/unload cleanup, and fresh retry behavior.
- Do not touch result, reward, progression, navigation, boss terminal ownership, or the Station guide bridge.

#### A1 exact plan and ticket boundary

The minimum runtime shape is one scene-local plan plus one ticket per authored Add row, not a wave manager.

1. Enumerate every `SpawnKind.Add` row in `StageDefinitionProfile.GetSpawn(index)` order and retain that source ordinal. Non-Add rows are outside this executor; a malformed Add row faults the whole plan rather than being skipped.
2. Build immutable ticket plans containing source ordinal, spawn/position/anchor/payload IDs, delay, the exact definition/live anchor, archetype, and gameplay prefab. `spawnId`, `anchorId`, and positive `positionId` must each be unique; reusing the same reviewed payload across distinct rows is allowed.
3. Resolve exactly one matching definition anchor, live `StageAnchorPoint`, and payload mapping per ticket. Require binding-root-local pose equality, `CombatSpawn/Add`, `Count == 1`, finite nonnegative delay, a promoted gameplay prefab, exactly one Enemy health owner, and the A0 agent/sensor contract.
4. After the exact run, player, selector, guide, terminal owner, and full plan validate, acquire one scene lease and stage every instance under an inactive per-ticket root. Only a completely staged plan may begin visible activation or target registration.
5. Capture one activation epoch when the exact run-owned guide first reaches `Released`. A ticket deadline is `epoch + delaySeconds`; delays never depend on another ticket's activation or death. A decrease in authored delay is rejected because it would make serialized order and release-relative timing disagree.
6. Activate a due ticket by injecting the exact player while inactive, subscribing its own death handler, enabling its root, and adding only that ticket to the A0 dynamic candidate set. A failure at any point faults the plan, synchronously unregisters/deactivates every pending or active ticket, and rejects late callbacks.

The conceptual ticket states stay deliberately small:

| Ticket state | Meaning |
|---|---|
| `Pending` | Fully staged and validated, inactive, waiting for its release-relative deadline |
| `Active` | Root active, exact player target injected, and dynamic candidate registration live |
| `Completed` | Its own health died; only its own registration and hierarchy were removed |
| `Cancelled` | It was unfinished when boss terminal, run loss, disable, unload, or retry stopped the plan |
| `Faulted` | Its authoring, instance integrity, or unexpected external lifetime violated the plan |

The executor is `Active` while any ticket is pending or active, becomes `Completed` exactly once only when every ticket completed naturally, and becomes `Cancelled` when an external stop closes an unfinished plan. A completed ticket remains historical if another ticket is later cancelled. Ticket completion never mutates the boss result, facts, result UI, or navigation.

`Count == 1` already rejects a negative count after the current getter clamps it to zero. `DelaySeconds` also clamps negatives to zero, so the editor validator should inspect the serialized backing value and reject negative YAML instead of silently treating it as immediate activation. This hardening does not justify a new runtime schema.

#### A1 Ark guardrail

The foreign data supports separation and explicit order, but it does not supply the local ticket semantics:

- The strongest PGR row is the bounded PBR family: one `PBRStage` sample carries `MonsterWaves=[101,102,103]`, and `PBRMonsterWave 101` separately carries four spawn groups with spawn times `[0,6,12,18]`. It directly supports stage → wave → timed spawn-group separation, but it is a PBR minigame cohort rather than a universal stage runtime contract.
- HI3 `EditorMonsterCardStage` samples expose `Wave1/2/3` and explicit per-entry slots. This supports explicit sequence identity and is a reason not to hide a future multi-wave contract in inspector order alone.
- Aether Gazer level rows expose ordered `wave_list` IDs while wave rows separately carry map, task, monster-level, and AI-level metadata. This supports the later Phase B wave sidecar, not adding it early to A1.
- Wuthering's reviewed rows prove placement identity and initial sleep/hidden state but not wave order, delay, or completion. ZZZ group/member placement remains supporting-only because the reviewed public-code candidate is not authoritative runtime evidence.

No reviewed source proves the unit or clock origin of delay, the meaning of PGR's completion code, or the runtime join from wave completion to stage result. Therefore A1's guide-release epoch, scaled local gameplay clock, one-health death observation, independent boss relationship, and terminal/unload cleanup are explicit local product decisions. Source ordinal is a transitional deterministic convention for this count-one collection only; the first real multi-wave or conditional encounter must introduce explicit wave/sequence identity rather than extend this convention by accident.

#### A1 exact two-file proof

The first proof deliberately reuses `SciFiSoldier.Melee`. Station already contains an unbound `Add_RightLaneAnchor` under the same parent as the live left anchor, with the mirrored binding-local position `(8.9, 0, 1.25)` versus `(8.9, 0, -1.25)`. Repository history also preserves the local identity tuple `add-right / Add_RightLaneAnchor / positionId 2102`, so no new ID is required.

| Product file | Exact proof delta |
|---|---|
| `DB_Stage_OlympusStationCombat.asset` | Append `AnchorRef(Add_RightLaneAnchor, CombatSpawnAnchors, (8.9,0,1.25), zero rotation)` and `SpawnRef(add-right, Add, 2102, Add_RightLaneAnchor, SciFiSoldier.Melee, direct Melee archetype, count 1, delay 0)` after the left rows |
| `OlympusStationCombatStage.unity` | Add one `StageAnchorPoint` to the existing right Transform and append that exact component to the binding after the left anchor; do not move, reparent, or duplicate the GameObject |

The old combined-definition pose `(13.35, 0, 1.875)` and placeholder payload `OlympusAdd.Right` are historical residue and must not return. The direct Melee archetype already migrated onto the left row is reused; route, template, pocket, guide, boss/result owner, and navigation remain unchanged.

Before this proof, the A1 implementation slice must replace the validator's `AnchorCount == 1`, `SpawnCount == 1`, scalar `spawnId`, and one-mapping assumptions with generic per-row checks while retaining the canonical left row as a regression baseline. Its PlayMode test must derive expected tickets from the definition. It should also make `SpawnRef` own the exact archetype reference as described in A2 below, rather than generalize the scene-local mapping and immediately replace it later. Then adding the right row exercises the same validator and runtime test without another runtime, validator, or test-code edit; the proof's product delta is genuinely the definition and scene only.

Assuming A0 is already complete, the generalized capability plus payload-owner correction should touch five existing code/test files: `StageDefinitionProfile`, the executor, validator, route PlayMode tests, and canonical UI/authoring tests. Migrating the current left row and removing the scene mapping touches the Station definition and scene. The subsequent Pincer proof edits those same two product-authoring files only. No new script, catalog, asset, route row, or digest cohort is expected.

This “Melee Pincer” fixture has moderate player novelty but the strongest workflow evidence. It may remain only if two melee enemies do not make the boss space unreadable.

### Phase A2 — implemented: one reviewed Ranged loadout

Post-A2 implementation status, 2026-07-21 KST: this phase is complete at the functional
gate.
The canonical `add-right` now resolves through a dedicated Ranged prefab, one-entry
RifleCrossfire deck/profile, hostile-orange projectile, fixed ticket-owned projectile root,
bounded three-instance reuse, profile-exact hit policy, elevation-aware warned-line aim,
pressure-screen priority, and synchronous terminal/unload/Retry cleanup. The final relevant
test classes passed 201/201, and route/policy/result identities remained unchanged. Human
mobile-landscape visual review is still pending. See
`A2_RIFLE_CROSSFIRE_RANGED_LOADOUT.md`.

The inventory and proposed proof below are the pre-implementation reasoning record. They
explain why A2 used a dedicated narrow loadout instead of approving the shared General
deck or the candidate role layer.

The static prefab inventory was promising, but the pre-A2 `SciFiSoldier.Ranged` was not yet
a truthful RifleCrossfire product enemy.

| Surface | Exact pre-A2 truth at audit cutoff | Consequence |
|---|---|---|
| Archetype/prefab | The promoted archetype directly references game-owned `PF_Enemy_SciFiSoldier_GeneralDeck`, with dedicated promotion false | Reuse the visual, health, movement, sensor, and cue wiring; do not rebuild an enemy |
| Actual projectile pattern | Of six General-deck rows, only `ClosePunish` is `ProjectileLine`; its selection range is 0–1.9 m | The current laser fires only at close range, not as backline rifle pressure |
| Other named ranged patterns | `RetreatShot`, `LinePressure`, and `RetreatBlink` are `ForwardLine`; `FanPressure` is `ForwardFan` | These are direct hit-shape checks in `BasicSoldierEnemy`, not projectile flight despite their names |
| Projectile lifetime | The driver instantiates `PF_SummonSlot2Projectile_LaserBolt` below the enemy's moving `CombatVfxPool`; hit/timeout only deactivates it | Shooter motion can affect the child projectile's world path, and inactive projectile objects accumulate until the enemy root dies |
| Target participation | The prefab sensor has an empty candidate array | A0 must inject the exact player before any behavior conclusion is valid |
| Presentation | Telegraph and pattern VFX mappings exist; attack audio is empty and the action-camera driver has a null controller | Visual behavior can be smoked now; rifle audio and pattern camera are not currently proven product features |
| Product evidence | Canonical scene/stage uses and integrated Ranged prefab/deck/driver tests are both zero | Asset presence is not admission evidence |

The first A2 action is therefore a **circuit smoke**, not content promotion: force the existing close-range `ClosePunish`, prove one real hostile projectile from windup through hit/timeout and cleanup, and expose the two lifetime defects above. Passing that smoke proves the reusable projectile circuit only; it does not approve the six-row General deck or the name Rifle Crossfire.

For durable content, prefer one new reviewed mid-range `ProjectileLine` profile and a narrow product Ranged deck, then bind that deck to the existing promoted Ranged prefab. Do not modify the shared General deck: three Corridor soldiers already consume it. The existing candidate-only `BacklineShooter` deck is not a shortcut because none of its rows is `ProjectileLine`, and enabling the role-candidate layer would introduce a second, unproven payload path.

If direct play shows the broad General deck is fun as a close/mid-range hybrid, it may instead be admitted under an honest hybrid identity. It must not be called a rifle backliner. If a truthful mid-range projectile still requires a global role framework, a new camera manager, or broad AI rewrite, stop and keep Melee Pincer as the current expansion proof.

#### A1/A2 payload owner decision

Move payload ownership from the scene executor into the spawn row while A1 is already rewriting resolution:

- Add a direct `CombatEnemyArchetypeProfile` reference to `SpawnRef` and retain `payloadId` as the stable human-readable/migration identity.
- Require `payloadId == payloadArchetype.ArchetypeId`, promotion false, a game-owned gameplay prefab, exact Enemy-health cardinality/team, and the A0 agent/sensor contract.
- Resolve and retain the exact archetype/prefab in the immutable ticket plan at admission. Do not reread a scene registry at each activation.
- Remove `StageCountOneEncounterExecutor.payloadMappings`; the Station scene must not be a hidden enemy catalog.
- Keep the direct reference optional for non-Add rows until their runtime owners are real. The current collection executor requires it for every Add.

This changes no canonical route identity: `ComputeCanonicalRouteDigest()` consumes the segment's stage ID and scene path, not SpawnRef contents. A separate stage-owned archetype catalog would add ID lookup, duplicate/unused-row validation, and another asset/API without reducing today's two- or three-row authoring cost. Reconsider it only if external/non-Unity authoring, addressable preloading, runtime patching, or many stages genuinely need one shared lookup boundary.

Ark data often uses `spawn payload ID -> central enemy definition -> model/stat/AI` joins. PGR provides the strongest example, while Wuthering supports separate entity placement and AI/blueprint configuration; HI3 and Aether do not expose a complete stage-to-enemy join in the reviewed evidence, and ZZZ remains supporting-only. In this Unity project, a GUID-backed direct reference to the central archetype profile plus an equal stable ID preserves the same responsibility split without copying the foreign catalog machinery. Stage rows still do not embed prefab, stats, AI, or role logic.

#### A2 bounded behavior evidence

| Direct row | Required observation |
|---|---|
| Exact admission | The A1 ticket consumes the exact Ranged archetype/prefab, injects the exact player, and registers the Ranged health without dropping the boss or Melee Add |
| One projectile circuit | Forced `ClosePunish` enters Windup then AttackActive, emits exactly one projectile with the Ranged health as source and Enemy team, and applies its local profile damage at most once |
| Miss/timeout | A clean miss stops at the configured lifetime and leaves no active collision, audio, trail, or damage callback |
| World-path independence | Moving/turning the shooter after firing does not translate or rotate the launched projectile's world trajectory |
| Bounded repetition | Repeated fire does not grow inactive projectile children without bound; every owned projectile is synchronously deactivated on ticket stop and eventually destroyed or reused |
| Death and terminal | Ranged death completes only its ticket; boss terminal during windup or flight cancels the ticket, makes its hierarchy/projectiles inactive before deferred destruction, and causes zero later damage |
| Disable, unload, retry | All projectile, sensor, target, VFX, subscription, and ownership state is gone before a fresh run; the new ticket fires from a clean count |
| Readability review | Melee + Ranged + boss remains targetable and visually legible; the reused summon laser visual is explicitly accepted as hostile or replaced, while absent rifle audio/camera are recorded rather than implied |

Behavior admission is **M** rather than the earlier S-M estimate: the circuit smoke is small, but current evidence predicts an owner-local projectile lifetime/world-parent repair plus one reviewed projectile profile/deck and prefab update. Audio can remain a separate S presentation slice for the smoke, but a durable player-facing rifle should not remain silently indistinguishable from existing attacks.

The likely bounded change surface is the existing projectile driver, Ranged prefab, one existing PlayMode test, and validator/setup code, plus two new logical data assets: a reviewed rifle pattern and its narrow deck. The Station definition changes only when the loadout is actually admitted as content. No new enemy controller, role framework, route owner, or projectile manager is expected.

### Enemy roster readiness — pre-A2 audit snapshot

The current product roster is Melee plus the dedicated RifleCrossfire Ranged loadout. The
table below preserves the pre-A2 inventory audit that drove the narrow-loadout decision;
its Melee-only and Ranged-zero-use rows are historical, not current product claims.

The nine archetype assets were not nine playable enemies:

| Archetype cohort | Truth at pre-A2 audit cutoff | Decision | Incremental effort after A0/A1 |
|---|---|---|---:|
| `SciFiSoldier.Melee` | Only product-consumed archetype. Static Corridor instance works; dynamic Station instance proves lifecycle but not target/attack participation | Finish A0, then use it for the zero-code Pincer authoring proof | S-M |
| `SciFiSoldier.Ranged` | Promoted gameplay prefab with health, sensor, cues, and projectile driver, but only close-range `ClosePunish` launches a projectile; projectiles inherit a moving parent and inactive instances accumulate; zero scene use | Smoke only the current projectile circuit, repair ownership, then admit one reviewed mid-range deck or rename it honestly as a hybrid | M |
| `SciFiSoldier.Elite` | Promoted prefab with one Elite deck and all five elite profiles attached; zero scene use. Aura/summon references are empty, and a selectable `ProjectileLine` pattern has no projectile driver, so that attack can deal zero damage | Later. Fix/exclude the invalid pattern, then decide one explicit role or intentionally approve a blended elite before product use | M-L |
| three `HumanoidBoss.*Elite` archetypes | Candidate role prefabs built on 90-HP `BasicSoldierEnemy`; all still require dedicated promotion and are not terminal bosses | Keep as boss prototyping inventory, not stage payloads | L |
| `Forge3D.LineTurret` | Presentation prefab only; no gameplay prefab or runtime script | Later fixed-threat slice after mobile enemies; needs health, targeting, hit shape, attack and cleanup | M-L |
| `Forge3D.MissileTurret` | Data-only; no gameplay or visual prefab | Defer | L |
| `DragonBoss.Future` | Empty inventory marker | Defer outside the current humanoid/offline core | L-XL |

The generic spawn admission must reject an archetype with `RequiresDedicatedPrefabPromotion == true`, a missing gameplay prefab, wrong health cardinality/team, or an incompatible reviewed spawn kind. Today only the Station-specific validator protects the Melee fixture; the executor itself would otherwise accept any mapped prefab with one Enemy health.

Role data should not be promoted wholesale. The repository already has twelve role profiles, eight role decks, and twelve role-candidate assets, but the runtime archetype path does not consume them. When a second behavior is needed, admit one reviewed **loadout**—archetype/prefab plus starting pattern/deck and optional elite package—rather than make every compatible role silently live.

Ark evidence supports that boundary at a structural level:

- HI3 is the strongest example: one monster config family carries identity/type plus swappable `AIName` and attack/defense/HP ratios. This supports composition of behavior and tuning without cloning a stage or visual prefab.
- Wuthering Waves physically separates monster identity, `BaseProperty` stats, `AiBase` controller/behavior-tree references, and level-entity placement. The reviewed data does not prove every exact join, so only the separation is adopted.
- Aether Gazer separates monster identity from stage/wave `monster_level`, `ai_level`, map and attribute factors, while explicitly lacking an exact enemy spawn list in the reviewed wave rows.
- PGR separates monster identity/model, mode-specific placement, affix and difficulty families; its reviewed base-stat/AI join is not strong enough to copy.
- ZZZ remains supporting-only because its enemy/stage field semantics and public-code placement candidates are not authoritative.

The safe destination is `Enemy archetype -> reviewed stat/behavior loadout -> stage wave/spawn placement -> optional difficulty modifier`. Stage data owns count, position, timing and completion; it does not own AI implementation or base stats. No foreign stat value, AI tier, role name, or difficulty formula is a product input.

### Canonical continuity correction — historical Phase 0 acceptance plan (implemented)

Phase 0 is complete. The following problem statement and acceptance list preserve the
pre-implementation plan that produced the current same-scene route; present-tense and
future-imperative wording below belongs to that historical plan.

This precedes both encounter generalization and a second selectable stage. The current two-scene boundary is not a content requirement; it is a product-path regression.

- `OlympusCorridorStageMap` contains 3,777 transforms and the Station copy contains 3,775. All 3,775 common hierarchy paths have the same local position, rotation, and scale. Both contain the same 44 modular stair pieces.
- The environment is serialized twice rather than shared through a prefab. Corridor adds only two map decorations, while the Station copy changes the whole map-root transform: Corridor is at identity with uniform scale `1.5`; Station is translated, rotated 90 degrees, and non-uniformly scaled `1.8/1.8/2.3`.
- `HandleTutorialCompleted()` currently seals the tutorial and immediately performs a Station `SingleLoad`. This bypasses the already-authored `BeginWaitingForStairEntry -> stair trigger -> BeginCorridorCombat` path.
- Single-load also replaces the player, camera, and HUD instances. Static run state survives, but physical player state and spatial continuity do not; even authored player max health differs between the two scene copies.

The product direction is therefore **one physical scene with two logical segments**:

1. Keep the Corridor scene and its map as the physical source of truth.
2. Keep tutorial-only gates, targets, traversal support, and presentation under an explicit tutorial activation group.
3. On tutorial completion, seal the tutorial fact, disable the tutorial-only objects, and release the existing stair blocker. Do not load another scene.
4. Let the player descend the authored stairs. At the lower trigger, enter logical segment two exactly once in the same scene and activate the lower combat group.
5. Move only the Station-owned runtime responsibilities into that lower group: guide, coordinated boss encounter, fact collector, result presenter, count-one Add executor, and their exact bindings. Reposition them against Corridor's real lower-area anchors rather than preserving Station's transformed duplicate-map coordinates.
6. Keep the shared additive result UI and Replay/Retry/Lobby behavior. After parity is proven, remove the copied Station map and retire the Station scene from the product route/build; retain it only as a temporary migration reference until then.

Do not disguise this as `SingleLoad`, and do not load the current full Station scene additively. Additive loading would duplicate the map, player, camera, HUD, listeners, colliders, and lighting. A shared environment prefab would reduce authoring drift but would still reload the player space, so it is only a migration aid, not the final flow.

The bounded runtime change is a route revision with an explicit in-scene segment advance. Revision 1 remains historical evidence; revision 2 owns the new condition semantics and digest. Existing two-segment tutorial/Station facts can remain, while the handoff receipt records the same scene handle and no loader generation. This is not a request for a general scene-streaming or route-graph framework.

Minimum acceptance:

1. Tutorial completion preserves the exact active scene, run ID, player, camera, HUD, health, energy, and combat mode instances; it only closes tutorial ownership and opens the stairs.
2. Before the lower trigger, segment zero remains active and the Station encounter, guide, Add, and result adapters remain inactive.
3. Physical entry into the lower trigger creates one in-scene segment-entry receipt and starts the lower combat path exactly once; duplicate trigger contact is inert.
4. Guide release, count-one Add, boss terminal ownership, facts, durable result, Replay/Retry/Lobby, abort, unload, and cleanup keep their existing behavior without the legacy corridor clear overlay also firing.
5. At runtime there is exactly one map, player, gameplay camera/listener, HUD, combat encounter, fact collector, result presenter, and Add executor.
6. Retry starts a fresh Corridor run. No revision-1 handoff or durable receipt is reinterpreted as revision 2.

Only after this correction is playable and stable should the roadmap resume encounter generalization and second-stage work.

### Encounter/wave ownership — introduce only when required

The second playable stage should be a small “Olympus Courtyard Drill” style arena built from promoted `_Game/Art/Environment/OlympusTemple` modular floor, fence, column, arch, and stair prefabs. It should not copy either multi-thousand-object canonical scene.

Its first durable combat candidate is Rifle Crossfire **after** the reviewed Ranged loadout above exists, with a simple required-defeat goal. That requirement, not foreign feature parity, is the trigger to add explicit encounter/wave ownership. The smallest later sidecars are:

| Owner | Minimum fields | Explicitly not owned |
|---|---|---|
| Encounter plan | `encounterId`, sequence, activation kind/condition, `Independent` or `Required`, ordered wave IDs | map, payload, placement, result, reward |
| Wave plan | `waveId`, sequence, activation kind/condition, delay, completion policy, ordered spawn IDs | enemy stats, camera, UI, stage terminal |
| Spawn row | existing payload ID/profile, kind, anchor/position, one instance | wave order, objective, progression |
| Anchor row/binding | ID, group, binding-local pose | payload or completion |

The first supported wave policies should be `OnEncounterStart`, `AfterPreviousWave`, `AllSpawnedEnemiesDefeated`, and `ExternalCondition`. No branching graph, score system, recommended team, stamina, reward table, AI scaling, or live-operation fields belong in this slice.

### Candidate order and stop conditions

| Candidate | Purpose | Expected product delta after Phase A1 | Keep/stop rule |
|---|---|---|---|
| Melee Pincer | Prove two-file, zero-code encounter authoring | Combined lower-combat definition + continuous Corridor scene | Keep only if targeting/camera/boss readability remains acceptable; otherwise retain as a test fixture |
| Rifle Crossfire | First clearly different durable encounter | Direct archetype row plus a reviewed mid-range projectile profile/deck and bounded projectile ownership | Do not use the current General deck under this name; stop if a narrow Ranged loadout requires the global role-candidate framework or broad AI rewrite |
| Olympus Courtyard Drill | First small selectable stage and required-defeat owner | New lean scene, definition/route/catalog/result-progression data, only proven lifecycle neutralization | Stop if implementation starts cloning the full Olympus route or either full scene instead of proving a neutral one-scene path |
| Elite Assault | Later behavior/pressure expansion | Elite archetype/prefab plus tuning | Defer until the shared EliteDeck's many behaviors are narrowed and directly played; do not promote candidate-only role prefabs implicitly |
| Turret or S1 BreakGate | Future content research | New gameplay prefab/controller or objective evaluator | Defer: these require new product behavior, not only composition |

### Minimal evidence for the Phase A1 milestone

Only bounded product evidence is needed:

1. Direct continuous-scene load still spawns zero dynamic Adds and does not enter the lower combat segment without the canonical run.
2. A malformed later Add row fails the complete plan before visible activation, leaves zero target registrations or active roots, and releases all staged ownership.
3. Guide release creates equal-delay rows exactly once at their exact anchors in source order; a delayed row uses `release epoch + delay`, and an earlier enemy's death does not move that deadline.
4. Every Add acquires the exact player, transitions through movement/telegraph/active attack, and can cause real player damage; the player can select each Add without losing the authored boss or first colliding with it.
5. Killing one spawned enemy completes and unregisters only its ticket; every other Add and the boss remain targetable and the plan remains active.
6. Killing every spawned enemy closes the executor's internal plan exactly once while the boss encounter remains nonterminal.
7. Boss terminal with a mixture of completed, active, and pending tickets preserves completed history, cancels only unfinished tickets, synchronously stops their attacks/hierarchies, and cannot rewrite the stage outcome.
8. Run loss, executor disable, continuous-scene unload, and actual Retry remove every pending/live hierarchy, telegraph/VFX owner, target registration, health subscription, collider cache entry, and scene lease; the fresh lower-segment entry activates exactly the authored ticket count once.
9. After the generalized validator and data-driven tests are already in place, the Melee Pincer proof changes only the combined stage definition and continuous scene and is automatically exercised with zero runtime/validator/test code delta.

That is sufficient. A digest registry, multi-document acceptance packet, full foreign-data promotion, or general stage graph is not part of this milestone.

## Later second selectable stage: the real minimum package

This package remains useful, but it is no longer the next product milestone. It begins only after the canonical Olympus route is continuous in one physical scene; otherwise a new stage would be built on top of a known scene-ownership mistake.

The first second stage is not an asset-only task. Re-audit these locks after the continuity correction because the in-scene transition should remove some route-specific assumptions. At the current cutoff, the product path still contains five single-route locks:

1. Every public `UIStageCatalog` projection path rejects unless `StageCount == 1`, even though its internal projection builder is per entry.
2. `PlayableStageDefinitionValidator` requires exactly one catalog row, one result definition, one progression node, one graph, and a one-node graph. Its build-readiness companion also appends `OlympusStationCombatStage` as the only combat continuation instead of walking every selected route segment.
3. **B0-1 through B0-3 closed route ownership through the neutral scene-runtime boundary:** `StageRunRuntime` derives entry and terminal eligibility from topology, facts/result derive the terminal row and typed tutorial requirement, and exact neutral adapters own bootstrap, fact, recovery, presentation, loss abort, and action routing. Catalog and build semantics remain single-entry.
4. **B0-2/B0-3 closed the false tutorial/Station fact and scene-adapter locks:** a one-row combat route binds segment-zero facts, commits Clear/Fail without guide or handoff evidence, and reaches the shared result UI without copying Olympus components. The dynamic fixture proves the scene shell; it does not claim a second product scene.
5. The admitted result/progression snapshot pins the exact result catalog, full localization table, result definition, node, and graph. Adding rows to those accepted Olympus sources would change the existing join identity, so the second stage cannot be introduced by silently expanding the first stage's frozen result sources.

The existing Stage Select art contains several stage-card controls, but the product prefab has one selected ID, one focus entry, and no serialized card-to-`SelectStage` calls. This is reusable layout inventory, not yet a multi-stage selector.

### Minimum new content package

A compact one-scene Olympus arena without a new cinematic needs at least ten new logical assets. Unity metadata doubles the file count, but metadata is not a separate product responsibility.

| New logical asset | Why it cannot be the current asset | Expected cost |
|---|---|---:|
| Lean gameplay scene | Own map, binding, anchors, encounter, player/camera/HUD references | M |
| `StageDefinitionProfile` | Exact scene path, anchors, spawns, runtime/objective states | S |
| `PlayableStageDefinition` | New playable-stage identity, route, terminal actions/policy, template and result/progression sidecars | M |
| `LinearStageTemplateProfile` | Exact segment/pocket mapping and truthful title/objective/lesson | S |
| `StageResultPresentationProfile` | The current profile is bound to `OLYMPUS-INVASION-01`; a new stage needs its own code/name/proof surface | S |
| `StageResultLocalizationTable` | A separate small table preserves the accepted Olympus localization and join digests | S |
| `StageResultPresentationCatalog` | The route-owned result definition requires an exact catalog/profile/localization identity | S |
| `StageResultDefinition` | Exact new stage identity and action decoration | S |
| `StageProgressionNode` | Exact new route identity; Phase B leaves cross-stage unlock absent | S |
| `StageProgressionGraph` | A stage-local one-node graph avoids mutating the accepted Olympus join | S |

No separate briefing asset is required; the briefing is derived from the route/reference/template. A stage with typed-absent entry presentation needs no cinematic profile or Timeline. The extra localization/catalog/graph assets are deliberate isolation, not duplicated runtime ownership: the result UI consumes the run-owned deep copy, while the accepted first-stage result join remains byte-identical.

### Shared assets that must change once

- `DB_UIStageCatalog.asset`: add a unique entry and bump/recompute its catalog projection generation/digest.
- `PF_UI_StageSelectScreen.prefab`: bind two stage cards, two focus rows, selection calls, and truthful lock visuals. Hide or disable unused cards.
- `EditorBuildSettings.asset`: add the new gameplay scene. The route table does not need one route row per stage because the selected projection already carries its exact entry scene.

The catalog generation is a presentation-selection cohort, so both catalog rows receive new projection digests. It must not change either playable route digest, the accepted Olympus result/progression join, or any durable result bytes.

The current result UI scene, Replay/Retry/Lobby executor, localization **shape**, durable run-result store, scene binding/anchor model, player/HUD, environment modules, enemy archetypes, loading card, and UI route shell remain reusable. The accepted Olympus result catalog, localization asset, node, and graph remain unchanged.

### Runtime neutralization required before those assets are useful

Do not duplicate `OlympusCorridorCombatFlowController`, `OlympusStationRunFactCollector`, or `OlympusStationCombatResultPresenter` into the new scene. Their semantics, not only their names, require the tutorial, guide, boss terminal, and two-segment route.

The bounded foundation supports only the accepted two-segment route and one new one-segment combat route. It is not a general stage graph:

1. Remove the catalog's exactly-one guard while retaining unique ID, exact projection, generation, digest, and stale-source rejection per entry.
2. **B0-1 complete.** Route-snapshot structural validation adds no serialized field: segment count 1 or 2, contiguous sequence, unique IDs, exact first-entry semantics, a typed in-scene advance or `SingleLoad` only on a non-final row, and exactly one final `ReturnToOwner` row. A non-Olympus route must own its terminal condition ID and cannot reuse `station.encounter.terminal`.
3. **B0-1 complete.** Active and terminal eligibility derive from `current segment + final ReturnToOwner`, not the words Corridor or Station. Existing lifecycle enum numbers remain unchanged so accepted Olympus and historical receipt bytes are not reinterpreted.
4. **B0-1 complete.** An admitted first-and-final scene binds segment zero terminal-active without a handoff token, entry receipt, or handoff receipt. Same-scene route/join replay is idempotent; the existing Olympus pending/in-scene handoff paths remain unchanged.
5. **B0-2 complete.** Tutorial and guide facts are conditional. Olympus keeps the exact nonempty tutorial digest and guide-release gate; a one-scene route uses the existing empty tutorial-digest value, binds collection to segment zero, and seals one segment plus truthful combat/outcome facts without fabricating tutorial or handoff evidence.
6. **B0-3 complete.** The small common entry bootstrap and thin fact/result/recovery adapters now let a lean scene avoid the Corridor director, Station guide, and copied Station presenter/recovery logic.
7. Make the validator and build-readiness reporter enumerate every catalog entry and every route segment, while keeping the exact Olympus route as a named regression fixture. Remove the hard-coded Station continuation from build readiness.

The exact implementation order is:

| Ticket | Player/authoring outcome | Cost | Depends on |
|---|---|---:|---|
| B0-1 route topology and active-role seam | **Complete 2026-07-21:** a distinct one-row entry/final route admits terminal-active; malformed topology fails before a run exists | M | none |
| B0-2 truthful one-row facts/result | **Complete 2026-07-21:** one combat segment commits Clear/Fail without fake tutorial/guide/handoff facts; exact coordinator/run receipt ownership and the fixed Olympus result digest remain verified | L | B0-1 |
| B0-3 neutral bootstrap and terminal adapter | **Complete 2026-07-21:** a lean scene joins admission, exact facts/coordinator, commit recovery, acknowledged result UI, Replay/Retry/Lobby, and adapter-loss abort without Olympus component copies | M | B0-1, B0-2 |
| B0-4 multi-entry catalog/build plumbing | Two unique cards project exact entry scenes; build readiness walks route data instead of appending Station | M | B0-1 |
| B0-5 compatibility proof | The corrected continuous Olympus route, full route, endpoints, and new route/result identities remain stable; historical revision-1 receipts are preserved but never reinterpreted | M | B0-1 through B0-4 |
| B1-1 isolated stage content pack | One lean arena and its ten logical assets are authored without mutating accepted Olympus result sources | M | B0 complete, reviewed ranged loadout |
| B1-2 two-card presentation | Both cards select/focus/start their own immutable projections; invalid or duplicate IDs produce zero route request | S | B0-4, B1-1 |
| B1-3 end-to-end second route | Clear/Fail, Replay/Retry/Lobby, unload, and fresh re-entry work from the second scene | M | B1-1, B1-2 |

B0 is **L** because fact/result neutralization is the dominant risk; B1 is **M** once B0 exists. A later stage of the same one-scene family should be **S–M** and must not add another route-specific controller.

The likely B0 code surface is the four core route/fact/result files, `UIStageCatalog`, build readiness, the validator, two small bootstrap/adapter files, and focused route/UI tests. `StageRunAbort` or finalization types are touched only if neutral aliases are required; existing enum numbers and canonical rows must not move.

Minimum B0 acceptance is deliberately small but adversarial. Items 1-3 are complete in
B0-1, item 4 is complete in B0-2, and items 5-6 are complete in B0-3. B0-4 still owns
multi-entry projection, validator enumeration, and build-route walking:

1. **B0-1 complete.** A one-row run-entry plus final `ReturnToOwner` route passes; a missing successor on `SingleLoad`, a successor on the final row, a duplicate ID, or a sequence gap fails before context creation.
2. **B0-1 complete.** Direct one-scene admission makes segment zero terminal-active with no pending token or handoff receipts; same-scene/same-route-and-join admission is idempotent and foreign or stale admission fails.
3. **B0-1 complete.** The corrected route remains exactly `CorridorActive -> same-scene segment entry -> StationActive`, including its revision-2 receipt and digest values; the historical revision-1 route digest and `SingleLoad` shape remain readable but are not a product path.
4. **B0-2 complete.** One-scene terminal commit produces one completed segment, truthful combat/outcome facts, an absent tutorial fact, one durable exact-run summary/receipt, and one result presentation. Terminal before collector readiness, stale/foreign coordinator input, forged resolution, value-identical foreign replay, or misplaced decision lookup fails closed.
5. **B0-3 complete.** Disable, unload, and unexpected exit cancel the coordinator and close exactly one diagnostic abort; fresh admission receives a new run ID.
6. **B0-3 complete.** Actual Clear-to-Replay, Fail-to-Retry, and Lobby actions resolve the exact selected route entry. Existing Olympus full-route and result-store fault tests stay green.

The Ark comparison supports only the data boundary here. PGR's reviewed PBR sample separates one stage from ordered waves and timed spawn groups, HI3 exposes explicit Wave1/2/3 slots, and Aether Gazer separates a level from its ordered `wave_list`. None of those sources proves that DimensionBrawl needs a foreign scene graph or the same lifecycle states. B0 therefore neutralizes local entry/final-segment assumptions; encounter/wave composition remains a separate stage-owned layer.

The cheaper alternative—copying the two Olympus scenes or treating a normal enemy as the existing boss terminal subject—would create a second playable row faster but would not reduce future content cost. It is rejected as a product milestone.

Required-defeat ownership is not hidden inside B0. The current terminal vocabulary is explicitly Player/Boss, so a group of ordinary enemies cannot truthfully masquerade as the boss subject. The first one-scene proof may reuse an actual boss terminal plus the reviewed ranged encounter; a roster objective gets its own later ticket only if the stage design actually requires it.

## Minimal offline first-clear unlock

The current progression assets already express `Cleared` and `MasteryObjectiveAchieved` prerequisites, exact node revisions, graph membership, and route binding. They do not evaluate, save, or apply progression. The durable result store is keyed by run ID and preserves result evidence; it is not a player profile and contains no cleared-node set.

The smallest truthful two-stage state is:

- node A: the current Olympus route, no prerequisite, available on a fresh install;
- node B: the second route, one exact prerequisite `(A, revision, Cleared, empty objectiveId)`;
- local state: a monotonic set of cleared **progression node IDs**, with bound playable-stage/result provenance for diagnosis;
- derived state: `available = every authored prerequisite is satisfied`; do not persist a second redundant `unlocked` flag;
- first-clear reward, currency, stars, mastery, stamina, inventory, and server sync: absent.

This relation must not be retrofitted by adding node B to the accepted Olympus run-owned graph: that graph digest is part of the admitted result/progression join. Phase C should introduce a versioned **availability graph/registry** owned by the player-profile progression slice, using the same stable node IDs but outside historical run-result identity. The one-node graphs inside each Phase B route remain evidence for that route, not the mutable global unlock authority.

Use the stable progression node ID as the entitlement key, not the catalog entry ID or scene path. Presentation/binding revisions must not accidentally relock an already cleared node; a semantic replacement that must not inherit progress receives a new node ID.

### Authority flow

```text
run-owned Clear candidate
  -> pending first-clear intent (node + run/result provenance)
    -> unchanged durable result commit/receipt
      -> idempotent cleared-node apply
        -> graph prerequisite evaluation
          -> catalog lock/select projection
            -> fresh admission recheck
```

The pending intent is the narrow crash bridge. For a Clear that can unlock content, durable intent preparation is a precondition for advancing the result commit; a transient write fault remains recovery-pending instead of silently losing the unlock. If the application exits after the result receipt is durable but before progress is applied, startup can match the intent to the exact receipt and finish the monotonic apply. This avoids changing the existing result decision/summary/receipt byte format. A receipt-less intent stays pending and locked until bounded recovery can classify it; corrupt or mismatched data is quarantined and never rewrites a valid result.

The first store needs only a schema/checksum/generation, ordered cleared-node records, and ordered pending intents. It does not need wall-clock timestamps, clear counts, balances, rewards, inventory, or achievement state.

Stage Select is only a preview. It may show B as locked and suppress its route request, event, and SFX, but `TryAdmitFirstSegment` must independently re-evaluate the fresh route-owned node/graph snapshot so direct loading cannot bypass the lock.

### Why progression and reward stay separate

The Ark comparison supports this separation without supplying product values:

| Dataset evidence | High-confidence structural use | Explicit limit |
|---|---|---|
| PGR stage rows expose `PreStageId`/`NextStageId` beside separate first reward, finish drop, and first action-point fields | predecessor/successor and first-clear economics are distinct concerns | raw-derived table shape, not shipped server/runtime proof |
| HI3 stage tables expose `PreMissionList`, `preLevelID`, and `UnlockedLink*`; elite compensation stores `FirstDropRewardID` separately | availability edges do not require reward ownership | public/helper evidence only |
| Aether Gazer rows expose `next_unlock_id_list` beside separate drop-library references | a directed next-stage relation can be authored independently | reviewed examples are activity/archive families; no first-clear runtime claim |
| Wuthering Waves dungeon rows expose entry conditions beside first, normal, and repeat reward IDs | entry eligibility and reward policy are parallel fields | table shape, not grant execution proof |
| ZZZ | no positive progression claim | field meanings remain too obfuscated |

The reusable consensus is only `clear state + authored prerequisite -> availability`. It does not justify importing foreign reward amounts, costs, currencies, stamina, or live-operation rules.

### Minimum player-facing acceptance

1. Fresh install: A is selectable, B is visibly locked; B produces zero route request and zero active run, including direct admission.
2. A committed Clear prepares and applies A exactly once; Fail or abort changes nothing.
3. Lobby return refreshes the catalog, and application restart retains B availability.
4. Replay/Retry and duplicate processing do not duplicate, relock, or change `firstClearRunId`.
5. Prepared/no-result remains locked; result-committed/pre-apply recovers after restart; corrupt or wrong receipt remains locked without changing durable result bytes.
6. The existing full Olympus route, Clear/Fail result presentation, Replay/Retry/Lobby, and result-store fault behavior remain unchanged.

This progression slice depends on a real second route and neutral admission path. It should ship with that route, not as an abstract framework beforehand.

## Keep, quarantine, and defer

### Keep as the current product core

- the existing combat feel and intended tutorial-to-stairs-to-lower-combat sequence, but not the current duplicate-map `SingleLoad` seam;
- the current boss terminal authority and shared result/navigation shell;
- `StageDefinitionProfile`, scene binding, anchors, archetype-to-prefab mapping, and Add cleanup behavior;
- the current tutorial observations until their presentation/order is extracted without changing feel.

### Quarantine before deletion

- `OlympusStationCombatStage` after its guide, terminal, fact, result, and Add responsibilities have reached parity in the continuous Corridor scene;
- `PveStageData` / `PveEncounterDirector` and their runtime prototype stage;
- five non-canonical `S1_*` templates and eight legacy segment profiles;
- twelve unused role-candidate profiles and their candidate-only prefab layer;
- manual review/evidence helper artifacts that are not used by runtime or a current bounded test.

Do not delete shared visual prefabs, `FrontlineWaveStageProfile`, or combat components solely because their higher-level candidate profiles are unused; check direct scene/runtime references first.

### Defer

- servers, PvP, stamina, shops, events, seasons, and broad live operations;
- multiple currencies or a general inventory economy;
- broad mastery/course frameworks before a second content slice proves demand.

## Next five bounded tasks

1. **Complete B0-4 multi-entry catalog and build plumbing.** A
   second route must not copy Olympus-specific controllers or hard-code Station as a
   continuation.
2. **Close B0-5 compatibility after B0-4.** Preserve the corrected continuous Olympus
   route, immutable identities, full route, historical revision-1 receipts, and the
   neutral one-row result path under the multi-entry implementation.
3. **Author B1-1's compact second playable stage after B0 is green.** Reuse promoted modular
   environment pieces; do not copy either large Olympus scene.
4. **Connect B1-2/B1-3 two-card presentation and end-to-end re-entry.** Both cards must
   project and start their own immutable route, then Clear/Fail, Replay/Retry/Lobby,
   unload, and fresh re-entry must remain exact.
5. **Add only the breadth loop the stage proves.** Introduce a minimal required-defeat
   owner if needed, then one persistent first-clear unlock;
   CF-01, rewards, inventory, shops, stamina, and servers remain out.

Immediate follow-up: start with B0-4 catalog/build plumbing, not broad scene authoring. After B0-4 makes
the neutral route/result foundation selectable and build-walked, build one small second
playable stage and connect minimal offline
first-clear progression so the first route unlocks the second and survives restart. After
that: extract tutorial lesson order/copy into data, admit another reviewed enemy variant,
attach event-bound spawn/terminal presentation cues, then delete or archive proven-unused
prototype layers.

## Phased living roadmap

This roadmap is deliberately milestone-shaped rather than `P1` through `P3`. A phase advances only when it produces a player-visible or authoring-cost result; it does not trigger automatic implementation.

### Phase 0 — make the current stage spatially honest

- Preserve one Corridor map and one set of player/camera/HUD runtime objects.
- End the tutorial by opening the existing stairs instead of loading the Station copy.
- Enter the lower combat segment at the authored trigger in the same scene.
- Migrate the Station guide, coordinator, facts, result, and Add ownership without duplicating terminal or clear paths.

**Exit signal:** the player completes the tutorial, walks down the stairs, fights in the lower area, sees the normal result, and can Retry/Lobby without any map or gameplay-instance swap at the segment boundary.

### Phase A — make encounter authoring cheap

- Preserve a clean checkpoint of the current canonical route and working Station Add runtime fixture.
- Turn the runtime Add fixture into a bidirectionally registered active combatant while preserving boss terminal ownership.
- Replace the scalar count-one executor with ordered count-one `SpawnRef` tickets and direct archetype ownership.
- Keep one instance per row, the existing guide-release boundary, row-relative delay, and independent completion.
- Demonstrate a second existing-enemy encounter using one logical definition entry and one scene anchor with zero runtime changes.

**Exit signal:** every spawned Add can acquire and damage the player, can be selected by the player, stops synchronously at terminal, unregisters cleanly without retry-time cache growth, and an additional existing-enemy encounter can then be added without editing executor code.

### Phase B — prove the game can grow sideways

- Create one compact, one-scene playable stage with one catalog row.
- Reuse the current result shell and presentation hooks.
- Generalize only the lifecycle and validator assumptions that this stage actually breaks.
- Use the reviewed ranged loadout and add the minimum required-defeat objective and encounter/wave ownership the stage needs.

**Exit signal:** the title screen exposes two genuinely different playable choices, and the second route is not named or hardcoded into the runtime.

### Phase C — close a small offline repeat loop

- Persist one first-clear record locally.
- Unlock the second catalog entry from the first route.
- Preserve replay and retry independently from unlock state.
- Keep reward payload and inventory optional until there is a real use for them.

**Exit signal:** a new player can clear, unlock, restart the application, and continue without a server or general economy.

### Phase D — add breadth through reuse

- Add one enemy behavior/stat variant.
- Add one stage difficulty or restriction modifier without duplicating a scene.
- Normalize high-value combat feedback, beginning with damage impact consistency and Add spawn/terminal cues.
- Extract tutorial lesson order, localized copy, and audio references while retaining the proven behavior-observation adapters.
- Add the first accessibility controls for shake, flash, and reduced motion.

**Exit signal:** at least two visible content variations come from data and tuning rather than new route-specific controllers.

### Phase E — simplify, then reconsider expansion

- Audit and quarantine or remove the disconnected PVE prototype, unused `S1_*` templates, legacy segment profiles, and unused candidate layer after their useful semantics have replacements.
- Re-measure content addition cost and player-visible breadth.
- Reconsider broader progression or operations only from observed play needs.
- Keep PvP, server authority, stamina, shops, seasons, and gacha last.

**Exit signal:** the repository has one understandable content path, obsolete alternatives no longer confuse authoring, and the next expansion decision is based on play evidence rather than feature parity.

## Measurement rule

For every new content slice, record only:

- player-visible outcome;
- production files changed;
- new runtime code lines/classes;
- reused assets/systems;
- authoring time and test time;
- regressions or cleanup failures.

The direction is correct when the second encounter and stage require fewer product-specific code changes than the first. Test count, digest count, and document count are not product progress by themselves.
