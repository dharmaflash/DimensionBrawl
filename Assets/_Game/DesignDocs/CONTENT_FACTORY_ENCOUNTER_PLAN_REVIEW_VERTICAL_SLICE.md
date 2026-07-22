# Content Factory Encounter Plan Review Vertical Slice (CF-01)

Status: `IMPLEMENTED REVIEW SLICE / TEMP_DO_NOT_SHIP`

Date: 2026-07-18

Verification state: deterministic setup, independent scene verification, focused
PlayMode tests, 21-capture automated QA, inspection of all 21 captures, canonical UI
regression, and the Olympus actual-play path passed. A wider StageRun bundle exposed a
pre-existing scene-unload/VFX-prewarm race, recorded separately below.

Canonical product state changed: no

## 2026-07-21 product update

A1 is implemented and verified, and A2 is functionally verified with human visual review
still pending, in the canonical continuous Station segment.
The live executor consumes two ordered count-one `SpawnRef` rows with direct archetype ownership,
inactive transactional staging, independent combat participation/death, whole-plan
cleanup, and a typed receipt. The current left row is Melee and the right row is the
physical-projectile RifleCrossfire Ranged loadout. See
`A1_ORDERED_ADD_ENCOUNTER_EXECUTOR.md` and
`A2_RIFLE_CROSSFIRE_RANGED_LOADOUT.md`.
B0-1 is also implemented and verified: a distinct one-row route identity can now admit
terminal-active, while malformed topology fails before a run exists and the accepted
Olympus identities remain unchanged. B0-2 is now implemented and verified as well: that
route can commit truthful Clear/Fail facts and a durable result without fake tutorial or
handoff evidence. B0-3 is now implemented and verified as well: neutral scene adapters bind
admission, exact facts, commit recovery, result acknowledgement, and Replay/Retry/Lobby.
B0-4 multi-entry catalog/build plumbing is the next gate.

This does **not** admit CF-01 into gameplay. `StageEncounterPlanProfile`, its Wave grammar,
review session, controller, and review scene remain `TEMP_DO_NOT_SHIP`. A1/A2 deliberately
stop below Encounter/Wave schema. The table and rationale below are a dated 2026-07-18
review snapshot where scalar Add execution was still the product truth.

## Executive decision

At the time of review, CF-01 was the next higher-value review slice than another tutorial,
cinematic, chapter, preparation, or operations UI mock. The product already had a real
tutorial-to-combat-to-result path and several isolated mobile UI review samples. A1 has
since removed the scalar Add runtime blocker, and A2 has admitted one reviewed Ranged
loadout, B0-1 has closed the bounded route topology/active-role seam, and B0-2 has closed
truthful one-row facts/result commit. B0-3 has now closed the neutral bootstrap/terminal
adapter seam. The next product gate is B0-4 catalog/build plumbing before a second playable stage; an
admitted Encounter/Wave owner remains future work only if real stage design
requires it.

CF-01 therefore reviews the smallest content-authoring grammar that can later make stages repeatable:

```text
Stage
  -> ordered Encounter
    -> ordered Wave
      -> ordered Spawn
        -> DefeatAll completion
```

That review grammar is now implemented as an isolated deterministic profile, disposable
session, controller, generated mobile-landscape scene, setup/validator, focused tests, and
an asynchronous 21-capture runner. It remains review-only: none of those types admits the
plan into gameplay. Runtime spawning, combat outcome ownership, result presentation,
navigation, rewards, persistence, and server behavior remain separate future gates.

## Why this outranks another tutorial or UI mock

The repository already proves or reviews the major player-facing grammar around one stage:

- the canonical product route covers Login, Lobby, Stage Select, continuous Olympus tutorial/cinematic/combat, and Clear/Fail return actions;
- OLY-NAR reviews visual-novel, tutorial-cutscene, and briefing presentation;
- CHUB-01 reviews chapter, stage-map, stage-detail, and confirmation hierarchy;
- PREP-01 reviews stage intelligence and a disposable preparation presentation;
- OPS-01 reviews notice, mail, mission, and event entry surfaces.

Those samples are useful, but another isolated UI surface would not reduce the cost of
producing the second, third, or fourth playable stage. At the 2026-07-18 CF-01 review
cutoff, product breadth was one catalog entry and one route while
`StageCountOneEncounterExecutor` resolved one serialized `spawnId`, one `Add`, one payload
mapping, one runtime root, and one health owner. A1/A2 later superseded those scalar and
single-loadout facts; neutral route/catalog breadth remains the current leverage point.

CF-01 is still a UI review slice, but its UI exists to make the content hierarchy inspectable and falsifiable. Its success criterion is a clearer and cheaper authoring contract, not another shipping-screen claim.

## 2026-07-18 review-cutoff local evidence

| Local source | Truth at 2026-07-18 cutoff | Gap exposed by CF-01 |
|---|---|---|
| `Assets/_Game/Scripts/LevelDesign/StageDefinitionProfile.cs` | `SpawnRef` already carries spawn kind, position, anchor, payload ID, count, delay, and note; `AnchorRef` separately carries identity and expected pose | Stage data has a useful spawn/placement base, but no admitted Encounter or Wave owner |
| `Assets/_Game/Scripts/LevelDesign/StageCountOneEncounterExecutor.cs` | Resolves one serialized `spawnId`, requires one `Add` with `Count == 1`, and owns one active runtime object | A second row still breaks scalar assumptions instead of flowing through a collection contract |
| `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset` | Supplies the canonical lower-combat stage definition and current Add row | Existing data can ground review terminology without being mutated by CF-01 |
| `Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_SciFiSoldier_Melee.asset` | Is the only enemy archetype directly consumed by the live Station Add path | CF-01 deliberately does not reference it; direct archetype ownership remains a later runtime gate |
| `Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity` | Owns the current continuous tutorial and lower-combat space | CF-01 must not add anchors, runtime components, or review UI to this scene |
| `Assets/_Game/Scripts/Combat/CombatEncounterController.cs` | Owns the current exact player/boss terminal relationship | It must not become the CF-01 review-session or `DefeatAll` owner |
| `Assets/_Game/UI/StageSelect/UIStageCatalog.cs` | Projects the one admitted stage route and still protects a single-entry product contract | CF-01 must not add a catalog entry or claim a second selectable stage |
| `Assets/_Game/Scripts/LevelDesign/StageEncounterPlanProfile.cs` | Implements the review-only plan, one Encounter, ordered Wave definitions, Spawn definitions, validation, and canonical digest | It is not consumed by `StageCountOneEncounterExecutor` or `StageRunRuntime` |
| `Assets/_Game/UI/ContentFactoryReview/StageEncounterPlanReviewSession.cs` | Implements deterministic local `DefeatAll` accounting and exact-once completion/interruption | Its counters are disposable review state, not combat facts |
| `Assets/_Game/UI/ContentFactoryReview/ContentFactoryEncounterPlanReviewController.cs` | Renders one exact three-Wave profile and exposes five local review actions | It owns no route, health, result, reward, save, or service call |

The existing source is sufficient to review responsibility boundaries. It is not sufficient to claim runtime Stage-to-Wave execution.

## CF-01 outcome

The implemented isolated review slice proves only these four outcomes:

1. a reviewer can inspect one DimensionBrawl-owned plan as Stage -> Encounter -> Wave -> Spawn;
2. the board makes order, identity, parent ownership, payload reference, placement reference, and `DefeatAll` scope visible without opening gameplay scenes;
3. a deterministic in-memory session can step through the authored plan and simulate completion without instantiating an enemy;
4. validation and a stable digest expose malformed or reordered plans before any future runtime is connected.

It does not prove that an enemy spawns, targets the player, attacks, dies, advances a wave, clears a stage, produces a result, grants a reward, or survives application restart.

The persistent review boundaries are:

`CF-01 / REVIEW ONLY / RUNTIME NOT ADMITTED`

`SIMULATION ONLY / NO PREFABS / NO HEALTH / NO RESULT / NO REWARD`

## Authoring ownership contract

### Stage

Implemented review fields:

- positive schema version and revision;
- stable review-local `planId` and `stageId`;
- one canonical plan digest;
- explicit `ReviewOnlyNotAdmitted` admission disposition;
- explicit external outcome and reward owners;
- one embedded Encounter definition.

The profile owns only the review composition. It has no map/arena reference, route,
player-facing stage reward, progression node, gameplay result, or product-stage admission.

### Encounter

Implemented review fields:

- stable review-local `encounterId`;
- one ordered Wave array.

CF-01 implements exactly one Encounter. The `.required` suffix is a review-fixture identity,
not a shipping objective or terminal policy. Encounter activation variants, multiple
encounters, independent/required runtime semantics, map ownership, result, reward, and
navigation are not implemented.

### Wave

Implemented review fields:

- stable review-local `waveId`;
- contiguous zero-based `waveIndex`;
- activation fixed to `EncounterStart` for Wave 0 and `PreviousWaveDefeated` thereafter;
- objective fixed to `DefeatAll`;
- one ordered Spawn-definition array.

Wave owns review ordering and local simulated completion. CF-01 does not define a gameplay
clock, branching graph, survival timer, score rule, external trigger, or wave-to-result
dispatch.

### Spawn

Implemented review fields:

- stable review-local `spawnId`;
- one review-only `payloadId`;
- one review-only `anchorId`;
- positive simulated combatant `count`;
- finite nonnegative authored delay displayed as data only.

Spawn owns only a review payload/placement label and a local remaining count. The current
profile contains no prefab, `CombatEnemyArchetypeProfile`, AI, stat, scene anchor, or
`CombatHealth` reference. The payload and anchor strings are DimensionBrawl-owned review
identities, not product bindings.

### `DefeatAll`

`DefeatAll` is a Wave-local review policy. In CF-01 it means only:

> the session's remaining simulated combatant count for every Spawn row in the active Wave
> has reached zero through explicit `ResolveCurrentCombatant` actions.

When that condition is met, the session marks the Wave `Cleared`. A non-final Wave enters
`WaveTransition`; the final Wave enters `Completed`. It must not call
`CombatEncounterController`, mutate a `CombatHealth`, commit a `StageRun`, open
`UI_StageClear`, or assert that the product stage is clear.

The one Encounter becomes review-complete when all three Waves are cleared. This is a
disposable inspection state, not a Stage, combat, result, or progression fact.

## Bounded review fixture

The generated profile contains this exact DimensionBrawl-owned review fixture:

```text
cf01.review.stage
  -> cf01.review.encounter.required
    -> cf01.review.wave.01-entry [EncounterStart / DefeatAll]
      -> cf01.review.spawn.01-entry-left [count 2]
    -> cf01.review.wave.02-crossfire [PreviousWaveDefeated / DefeatAll]
      -> cf01.review.spawn.02-crossfire-left [count 1]
      -> cf01.review.spawn.02-crossfire-right [count 2]
    -> cf01.review.wave.03-final [PreviousWaveDefeated / DefeatAll]
      -> cf01.review.spawn.03-final-center [count 1]
      -> cf01.review.spawn.03-final-rear [count 1]
```

The three Wave combatant totals are exactly `2 / 3 / 2`, for seven simulated combatants
across five Spawn rows. The exact canonical profile digest is:

`4d9c363cf83a7bf6aa42a606b4d2699d70a2eb7f00c930c441343ed20d8414d5`

The payload labels `dimensionbrawl.enemy.melee-probe`,
`dimensionbrawl.enemy.ranged-probe`, and `dimensionbrawl.enemy.guard-probe` are review-only
strings. They do not reference or claim readiness for an existing enemy archetype or
prefab. The fixture evaluates composition grammar and inspection clarity, not enemy
variety, balance, or admission. No row is appended to the canonical Station definition,
written into the continuous scene, or consumed by the current executor.

## Validation contract

The review plan fails closed before a session begins when any of these invariants is false:

- schema version and revision are positive;
- plan, stage, encounter, Wave, Spawn, payload, and anchor IDs are nonempty stable IDs;
- admission is exactly `ReviewOnlyNotAdmitted`, while outcome and reward remain explicitly external;
- Wave IDs and Spawn IDs are unique;
- Wave indices are contiguous from zero;
- Wave 0 activates at `EncounterStart` and later Waves at `PreviousWaveDefeated`;
- every Wave uses `DefeatAll` and contains at least one Spawn row;
- every Spawn count is positive and every delay is finite and nonnegative;
- each Wave's positive Spawn-count sum fits the session's signed 32-bit remaining-count field;
- the serialized canonical digest equals a digest recomputed from the complete plan snapshot.

The deterministic setup adds a stricter fixture/scene boundary: exact schema/revision/IDs,
exact three Waves, exact reviewed plan digest, seven simulated combatants, one camera,
canvas, safe-area root, responsive root, event system and controller, complete controller
bindings, three entries in each Wave-card array, exactly five distinct action bindings mapped
to the five expected named scene Buttons, 48 px minimum button rectangles, no persistent
button events, no missing MonoBehaviour scripts, no enumerated combat/AI/spawn/result/route
component, and no Build Settings entry. The actual dynamic TMP font assets must exist with
metadata and bind exactly to their intended text roles, while immutable protected inputs must
exist with metadata and remain digest-identical. The physical Editor Build Settings asset must
exist before exclusion is accepted. A malformed child is rejected rather than skipped. Setup
does not call the global `AssetDatabase.SaveAssets`; it saves only the generated profile asset
and review scene. The required dynamic TMP fonts are bound and validated but deliberately
excluded from the immutable protected-asset digest because their atlases are editor-mutable.
Before writing, setup verifies the existing profile IDs and scene controller/root/flow
ownership, snapshots the profile, scene, and both `.meta` files byte-for-byte, and restores
those exact files after any normal setup failure.

## Deterministic review session

The session is a pure in-memory state machine created from one validated immutable plan snapshot.

### States

```text
Ready
  -> WaveActive
    -> WaveTransition
      -> WaveActive (next authored Wave)
        -> Completed

WaveActive or WaveTransition
  -> Interrupted

Any non-Ready terminal/intermediate state
  -> Ready (Reset)
```

The implemented session state enum is exactly `Ready`, `WaveActive`, `WaveTransition`,
`Completed`, and `Interrupted`. The controller actions are `BeginEncounter`,
`ResolveCurrentCombatant`, `AdvanceWave`, `InterruptReview`, and `ResetReview`.
Reset clears the active Wave and per-attempt progress while incrementing the local attempt
generation. Completion and interruption are exact-once counters for review assertions only.

### Step rule

`ResolveCurrentCombatant` asks the session for the first unresolved Spawn in the active
Wave's serialized order, then decrements exactly one simulated combatant. No wall clock,
random number, animation callback, physics query, health event, or frame-rate-dependent
ordering participates. When the Wave remaining count reaches zero, `DefeatAll` closes that
Wave in the same action. `AdvanceWave` activates the next Wave explicitly.

Invalid transition, unknown Spawn, duplicate resolution, duplicate completion, and duplicate
interruption attempts fail closed. The current board has no arbitrary hierarchy selection,
drag ordering, editable content, or acknowledgement state.

### Digests

The implementation exposes one lowercase SHA-256 canonical plan digest. It covers schema
version, revision, plan/stage/encounter identities, ownership dispositions, ordered Wave
fields, and ordered Spawn fields. Deep-copy tests prove that projections cannot mutate the
source unnoticed and that authored tampering changes validation/digest truth.

There is deliberately no session digest. Session determinism is inspected through exact
state, current Wave, per-Wave status array, remaining count, cleared count, attempt
generation, completion count, and interruption count. The editor setup separately computes
a protected-asset digest before/after generation and the visual runner compares it
before/after capture. Neither digest is a canonical route, StageRun, result, progression,
save, or server identity.

## Mobile inspection board

The board is a touch-first, landscape review surface, not an in-game content editor.

### Layout

- actual responsive catalog entry: `AndroidLandscape`, reference `2400 x 1080`,
  CanvasScaler match `0.5`, safe-area mode `InsetsOnly`, and `32 px` inset;
- mandatory review resolutions: `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`;
- all critical content remains under `UISafeAreaRoot`;
- the top rail displays the DimensionBrawl content-factory breadcrumb and persistent
  `REVIEW ONLY / RUNTIME NOT ADMITTED` boundary;
- the left identity panel shows plan/stage/encounter/revision, objective, session state,
  progress, current Spawn, and external ownership boundary;
- the main timeline renders exactly three Wave cards with order, status, activation,
  objective, Spawn-row count, and simulated combatant count;
- the bottom action bar contains exactly `BEGIN REVIEW`, `RESOLVE TARGET`, `NEXT WAVE`,
  `INTERRUPT`, and `RESET`;
- the footer repeats `NO PREFABS / NO HEALTH / NO RESULT / NO REWARD`.

The implemented board is one fixed inspection surface. It has no right-side detail drawer,
blocking modal, editable hierarchy, product route, or combat launch control.

### Interaction and accessibility

- every actionable target has a minimum 48 px dimension at the reference resolution;
- pending, active, cleared, interrupted, and unavailable Wave states use explicit text as
  well as accent colors;
- button interactability follows the exact session transition that is currently legal;
- no drag gesture, free-text entry, or arbitrary ordering control exists;
- text wraps without shrinking below the review typography floor;
- the background reuses the existing Chapter Hub review art; no new or imported background
  art is introduced, and enemy portraits, reward icons, currencies, stamina, lock badges,
  and account identity are absent.

### Capture matrix — sealed

`ContentFactoryEncounterPlanReviewVisualQaCapture` is implemented to record these seven
capture states at each mandatory resolution, for exactly 21 PNGs:

1. `Ready`;
2. `Wave1Active`;
3. `Wave1Partial`;
4. `Wave1Transition`;
5. `Wave2Active`;
6. `Interrupted`;
7. `Completed`.

Each plan starts at `ResetReview`, reconstructs the exact profile-backed session, and uses
only public controller actions plus the session's read-only next-Spawn query. The runner
renders through the review Camera into an exact-size RenderTexture, verifies the exact
profile and three Wave arrays, deterministic session counts/statuses, absence of an active
StageRun, five 48 px action targets, zero persistent button events, nonblank pixels, exact
PNG count/resolution, setup postflight, and protected-digest equality. It enforces the exact
`BeginButton -> beginButton`, `ResolveButton -> resolveButton`,
`AdvanceButton -> advanceButton`, `InterruptButton -> interruptButton`, and
`ResetButton -> resetButton` scene-name/controller-field mapping. Every capture must resolve
the actual `AndroidLandscape` catalog entry with reference `2400 x 1080`, match `0.5`,
`InsetsOnly`, and `32 px`. Edit Mode postflight reads the manifest and all 21 PNGs back,
validates file bytes and decoded dimensions, then reads back and validates the final manifest
again. It writes
`capture-manifest.json` and `capture-report.md` under
`C:/tmp/DimensionBrawl-ContentFactoryEncounterPlanReview-QA`.

The final runner produced exactly `21/21` PNGs and passed every automated assertion. Its
protected-asset digest remained
`8138c58545a66be6fc1ba63828eadddfb36edab5b88a6a20a651e951baf91ab0`
before and after capture. Every capture retained all five action buttons inside the
rendered Canvas, with the smallest action target measuring `251.35 x 51.88` pixels. The
latest visual runner completed with top-level exit code 0; its log is
`C:/tmp/DimensionBrawl-CF01-VisualQA-Atomic-FontRoles-Final.log`.

Human review is recorded separately from the manifest's intentional
`HumanReviewed: false` machine field. All 21 PNGs were inspected on 2026-07-18. The first
capture attempt was rejected because the headless compact responsive writer remained
active while only the RenderTexture changed size, inflating authored geometry by exactly
1.5x at 1920 x 1080. After the runner was hardened with explicit capture references and a
deterministic virtual safe area, a second human pass rejected title ellipsis and mixed
partial-stretch offsets that overlapped Wave state/detail text. Those layout defects were
corrected and the scene regenerated. An independent audit then rejected the runner's manual
`1920 x 1080` / `24 px` approximation because it did not match the actual responsive catalog's
`2400 x 1080` / `0.5` / `32 px` contract. The runner was rehardened against the catalog itself,
and all latest 21 PNGs were re-inspected. They pass safe-area fit, clipping, overlap,
hierarchy, contrast, button-state legibility, and text readability at all three resolutions.

## Implemented inventory

- Plan schema and digest: `Assets/_Game/Scripts/LevelDesign/StageEncounterPlanProfile.cs`.
- Disposable review session: `Assets/_Game/UI/ContentFactoryReview/StageEncounterPlanReviewSession.cs`.
- Board controller: `Assets/_Game/UI/ContentFactoryReview/ContentFactoryEncounterPlanReviewController.cs`.
- Exact fixture: `Assets/_Game/DesignData/UI/Review/DB_ContentFactoryEncounterPlan_CF01.asset`.
- Independent scene: `Assets/_Game/Scenes/Review/UI_ContentFactoryEncounterPlanReview.unity`.
- Deterministic setup and validator: `Assets/_Game/Editor/ContentFactoryReview/ContentFactoryEncounterPlanReviewSetup.cs`.
- Visual capture runner: `Assets/_Game/Editor/ContentFactoryReview/ContentFactoryEncounterPlanReviewVisualQaCapture.cs`.
- Focused tests: `Assets/_Game/Tests/PlayMode/StageEncounterPlanReviewSessionPlayModeTests.cs` and `Assets/_Game/Tests/PlayMode/ContentFactoryEncounterPlanReviewControllerPlayModeTests.cs`.

The scene must remain outside enabled Build Settings and must contain no `UISceneFlowRouter`, `UISceneRouteLoader`, `StageRunRuntime`, `CombatEncounterController`, combat AI, `CombatHealth`, result presenter, progression/reward owner, persistence store, or service client.

## Recorded automated verification — 2026-07-18

The implemented core currently has these sealed results:

- deterministic setup after the final hardening: PASS with top-level exit code 0;
  `C:/tmp/DimensionBrawl-CF01-Setup-Atomic-FontRoles-Final.log`;
- independent generated-scene verification after the final hardening: PASS with top-level
  exit code 0; `C:/tmp/DimensionBrawl-CF01-Verify-Atomic-FontRoles-Final.log`;
- final focused PlayMode run: `10/10` passed, `0` failed, skipped, or inconclusive,
  with top-level Unity exit code 0; the split remains
  `StageEncounterPlanReviewSessionPlayModeTests` `6/6` and
  `ContentFactoryEncounterPlanReviewControllerPlayModeTests` `4/4`;
  `C:/tmp/DimensionBrawl-CF01-FocusedTests-Hardened-Final.xml`;
- independent read-only review found one Wave-total `int` overflow edge case; validation
  now accumulates with `long`, rejects sums above `int.MaxValue`, the session uses a checked
  conversion, and the added regression test covers both fail-closed validation and projection;
- automated visual QA: `21/21` exact PNGs passed across `1920 x 1080`,
  `2400 x 1080`, and `2520 x 1080`; final report at
  `C:/tmp/DimensionBrawl-ContentFactoryEncounterPlanReview-QA/capture-report.md`; top-level
  exit code 0 log at `C:/tmp/DimensionBrawl-CF01-VisualQA-Atomic-FontRoles-Final.log`;
- human visual review: PASS after re-inspecting all 21 final PNGs against the actual
  `AndroidLandscape` responsive catalog contract;
- final combined canonical/Olympus PlayMode regression: `36/36` passed, `0` failed or
  skipped, with top-level Unity exit code 0; `CanonicalUiRoutePlayModeTests` contributed
  `34/34` and `OlympusCorridorActualPlayPathTests` contributed `2/2`;
  `C:/tmp/DimensionBrawl-CF01-Canonical-Olympus-Hardened-Final.xml` and
  `C:/tmp/DimensionBrawl-CF01-Canonical-Olympus-Hardened-Final.log`;
- generated fixture: exactly three ordered Waves, five Spawn rows, combatant totals
  `2 / 3 / 2 = 7`, and canonical plan digest
  `4d9c363cf83a7bf6aa42a606b4d2699d70a2eb7f00c930c441343ed20d8414d5`;
- setup protected-asset before/after digest check: PASS;
- review scene Build Settings exclusion and forbidden product-owner check: PASS.

The wider `StageRunRoutePlayModeTests` bundle was also exercised twice. Each full bundle
reported `25/26`, but the failed test name moved while the exception stack remained
identical: an old Olympus scene's `PlayableDirector.stopped` callback activates summon
prewarm during scene unload, then `SpatialOneShotVfxPool.GetOrCreate` tries to move an
object into the unloading scene. The isolated failed test passed. The same pre-existing
teardown defect remained observable after the final combined canonical/Olympus XML had
already been saved: the Unity shutdown log emitted four instances of the same
`Destination scene is being unloaded` / summon VFX prewarm stack. Those shutdown entries
did not change the `36/36` XML totals or top-level exit code 0, but they remain known debt.
CF-01 is absent from the stack and has no PlayableDirector, SceneManager, summon, or VFX
dependency. The defect is neither counted as a CF-01 pass nor hidden by the green focused
and combined regression totals.

## Strict CF-01 deferrals

| Deferred surface | CF-01 boundary |
|---|---|
| Runtime enemy spawning | No prefab instantiation, pooling, scene anchor binding, delay clock, or spawn activation |
| Dynamic combat participation | No target registration, AI sensor injection, player lock-on, damage, death observation, or cleanup lease |
| `CombatEncounterController` and terminal ownership | Existing player/boss terminal authority is untouched; simulated `DefeatAll` cannot decide a product outcome |
| `StageRun` facts and result | No admission, segment transition, fact collection, durable result, Clear/Fail UI, Replay, Retry, or Lobby dispatch |
| Rewards and progression | No drops, first-clear reward, stars, mastery, unlock, inventory, currency, or grant transaction |
| Product route and catalog | No new `UIRouteId`, route row, catalog entry, Build Settings scene, or second selectable stage |
| Persistence and server | No local save, account state, backend API, live configuration, telemetry, scheduling, or network authority |
| Content assets and scenes | CF-01 owns only its review schema/session/controller, generated review profile and independent review scene; canonical stage definitions, archetypes, prefabs, maps, combat scenes, and product UI scenes remain unchanged |

The `Stage -> Encounter -> Wave -> Spawn -> DefeatAll` shape is a review hypothesis until the next runtime gates prove it. CF-01 must not be cited as evidence that the product already supports waves or required-defeat objectives.

## ArkData structural evidence

ArkData is used only to compare responsibility boundaries and explicit link grammar. The observations below were rechecked on 2026-07-18 against the stored files. They are static-data observations, not shipped-runtime reconstruction.

### Punishing: Gray Raven `Stage.json`

Exact source:

`\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\raw\alt3ri-pgr-data\2026-06-14\files\extracted_repo\PGR_Data-master\EN\bytes\share\fuben\Stage.json`

Provenance:

- source snapshot: `2026-06-14`;
- source repository commit: `856a0e4534d0854fa440040e961b74a97ba732e2`;
- observed shape: 10,916 rows and 83 fields;
- no license was detected in the reviewed snapshot, so this is static structural evidence only.

Relevant observed fields include `StageId`, `Name`, `Description`, `PreEventId`, `ClearEventId`, `PreStageId`, `BeginStoryIds`, `BeginConditions`, `EndStoryIds`, `EndConditions`, `StarDesc`, `SettleLoseTipId`, `FirstGotoSkipId`, `FunctionLeftBtn`, `FunctionRightBtn`, `RebootId`, `Restartable`, and `NextStageId`.

This supports keeping stage identity, descriptive/presentation references, story/condition references, restart behavior, and predecessor/successor links at a Stage-facing boundary. It does not prove a universal PGR Stage -> Encounter -> Wave -> Spawn runtime join, a completion evaluator, or DimensionBrawl field names. `DefeatAll` remains a local CF-01 review decision and is not copied from any `StarDesc` value.

### Punishing: Gray Raven GuideFight-to-Stage links

Exact helper source:

`\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\enemies-stages\pgr-guidefight-stage-reading-links.csv`

The helper contains eight data rows across EN and ZH and records an exact `guidefight_id -> stage_id` join, with source paths and commit provenance back to `GuideFight.json` and `Stage.json`. It supports one narrow structural observation: guide-fight configuration can remain a separate identity surface linked to a Stage.

It does not prove GuideFightStep overlay behavior, Encounter or Wave ownership, runtime event dispatch, ordering, exact tutorial presentation, or result semantics. CF-01 therefore does not fold tutorial/guide data into the encounter plan.

### Aether Gazer topology and wave context

Exact helper source:

`\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\enemies-stages\aether-gazer-stage-topology-wave-context.csv`

Provenance:

- helper snapshot: `2026-06-20`;
- source repository commit: `9a0e927fbb4f87dbc4a8740561e18073f4002622`;
- no reviewed license basis for derivative product use; static structural evidence only.

The header separates `stage_id`, `level_id`, `wave_id`, `team_id`, `node_id`, `map_id`, and `template_id`, then records explicit relation columns including `next_node_ids`, `linked_wave_ids`, `linked_level_ids`, `linked_team_ids`, and `linked_stage_ids`. Rechecked high-confidence `reforge-level-wave-list` rows contain one `level_id` and a serialized, pipe-delimited `linked_wave_ids` list. This supports distinct identities plus explicit ordered-link grammar between a level-like owner and Wave-like rows.

It does not prove exact enemy spawn placement, a universal stage family, delay units, runtime activation, `DefeatAll`, combat terminal ownership, or result dispatch. The serialized link order is useful review evidence; its shipped runtime interpretation is unverified.

### Provenance and no-copy boundary

- No external text, dialogue, IDs, assets, icons, art, audio, layout, measurements, timings, colors, balance values, enemy compositions, code, or schema names enter DimensionBrawl product data.
- Foreign IDs may appear only in source evidence outside product/review fixture identities; CF-01 fixture IDs use the `cf01.review.*` namespace.
- No raw or helper file is copied into `Assets` or converted into a Unity asset.
- Unknown or undetected source licenses prohibit derivative content use. The allowed takeaway is responsibility separation and explicit link grammar only.
- The reviewed helpers are curated/derived evidence. They are not official runtime traces, decoded official prefabs, or proof that every source field is consumed by the shipped games.
- If a later claim needs exact spawn placement, completion behavior, timing, or runtime joins, it requires new direct evidence and a separate local product decision.

## Gates toward an actual second stage

CF-01 is Gate 0, not the second stage itself.

1. **Review-contract gate.** Complete for CF-01: core implementation, setup verification,
   focused tests, exact 21-capture matrix, separate human visual review, canonical UI
   regression, and Olympus actual-play regression are sealed. The ownership fields remain
   review-only and canonical product assets stay unchanged.
2. **Combatant-participation gate.** Complete in A0: exact player/enemy registration,
   acquire/attack/damage, synchronous terminal/unload/Retry cleanup, and preserved boss
   ownership are verified for admitted runtime Adds.
3. **Collection-executor gate.** Complete in A1: the executor consumes ordered count-one
   rows with direct archetype ownership, independent tickets, whole-plan rollback/cleanup,
   and a typed receipt.
4. **Reviewed-loadout gate.** Functionally complete in A2, visual review pending: the
   second ticket uses one dedicated RifleCrossfire physical-projectile loadout with bounded
   ownership and actual Station damage evidence.
5. **Neutral route foundation gate.** B0-1 route topology/active roles, B0-2 truthful
   one-row facts/result, and B0-3 neutral bootstrap/terminal adapters are complete. Next
   close B0-4 multi-entry catalog/build plumbing without changing accepted Olympus
   identities.
6. **Second-stage product gate.** After B0, build one small one-scene arena from promoted
   local modular environment assets and reuse the neutral route/result shell.
7. **Runtime Encounter/Wave gate.** Only if the lean second stage actually requires
   ordered waves or ordinary-enemy roster completion, promote the minimum reviewed
   composition owner and one narrow `DefeatAll` resolver. Do not change boss/result
   ownership by accident.
8. **Offline progression gate.** After the second stage is real, add one monotonic local
   first-clear prerequisite so the first route unlocks the second across restart. Rewards,
   economy, account sync, and live service remain later decisions.

The stop rule is simple: if a gate starts cloning the Olympus route, reviving the disconnected `PveStageData`/`PveEncounterDirector` as a second authority, or building a general quest/live-service framework, stop and narrow the work back to the first unproven responsibility.

## Later UI candidates

Two useful UI seams remain after the content path is cheaper:

- `RET-01`: a typed, exact-once Result -> Lobby arrival handoff so the current return greeting is driven by an ephemeral, digest-bound arrival context rather than a manual mock control. It owns no reward, progression, persistence, or service state.
- `POST-01`: a committed-Clear-only post-battle presentation handoff. It may be considered only after durable settlement exists for the relevant content; Fail, abort, Retry, stale, and duplicate paths dispatch nothing, and presentation cannot rewrite settlement.

Neither candidate reduces the cost of authoring the second encounter or stage. They remain downstream of the CF-01 review decision and the runtime gates above, rather than being folded into this slice.

## Definition of done

The implemented CF-01 core is complete at the review-contract level when:

- the exact three-Wave/seven-combatant fixture and canonical digest validate;
- the disposable session exposes only the five documented states and fails invalid or
  duplicate transitions closed;
- the controller renders exact profile truth through five local review actions;
- the generated scene remains outside Build Settings and contains no combat, StageRun,
  result, reward, route, persistence, or service owner;
- deterministic setup, independent verification, and both focused PlayMode fixtures pass;
- protected canonical assets remain digest-identical.

Those core conditions and the full review-slice sealing checks now pass: the capture runner
produced exactly 21 valid PNGs, a human separately accepted all 21 captures, canonical
UI-route and Olympus actual-play-path regressions passed, and the evidence is recorded.

Even after those checks pass, `TEMP_DO_NOT_SHIP` remains mandatory. Shipping requires a
separate runtime Encounter/Wave admission task and cannot be inferred from this review
profile, session, scene, tests, or visual evidence.
