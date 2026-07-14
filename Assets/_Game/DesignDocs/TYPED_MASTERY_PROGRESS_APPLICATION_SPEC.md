# Typed Mastery and Progress Application Spec

## Status

- Drafted: 2026-07-14
- Status: provisional P1-D review contract; analysis only
- Roadmap source: [Subculture Dataset Gap Roadmap](SUBCULTURE_DATASET_GAP_ROADMAP.md), P1-D
- Run/result predecessor: [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md), P1-A
- Route/reference predecessor: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md), P1-B
- Encounter predecessor: [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md), P1-C
- Later variability boundary: [Stage Rule, Modifier, and Enemy Variant Spec](STAGE_RULE_MODIFIER_ENEMY_VARIANT_SPEC.md), P2-A
- Later course-chain consumer: [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md), P2-B; it may read one exact committed row but never evaluate or persist mastery
- Later reward extension: [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md), P2-C
- Working archive root: `\\DESKTOP-69817L3\ArkData\SubcultureGameData`
- Production order remains `P0 -> P1-0 -> P1-A -> P1-B -> P1-C -> P1-D`. This document does not authorize production work before those gates close.

P1-D turns a truthful run result into two things only:

1. optional, typed, non-blocking mastery results evaluated from the run's immutable facts; and
2. a minimal durable clear/mastery state applied exactly once for that run.

It does not add rewards, currency, rank, score, generic achievements, or a chapter-content framework.

## Current Verdict

No P1-D fixture is freeze-ready.

- No runtime `StageRunContext`, `RunResultSummary`, `StageResultDefinition`, typed `MasteryObjective`, `StageProgressionNode`, `StageProgressState`, application ledger, or gameplay save owner exists.
- `LinearStageTemplateProfile.masteryObjective`, `StageDefinitionProfile.clearCondition`, reward hooks, and current result readouts are prose. They are not evaluator input and must not be auto-migrated.
- `CombatEncounterController` owns only scene-local `Running / Won / Failed` state. `OlympusStationCombatResultPresenter` listens directly to raw `Won`, so it is not yet a committed-summary seam.
- `BossBarrageEncounterController.RouteResultRecord` is a useful scene-local proof adapter, but it lacks canonical run/stage identity, objective definitions, a digest, and persistence.
- The five linear templates are not joined to the product route. Their target times and mastery prose are not approved thresholds.
- Chapter-map scripts serialize `locked` and `cleared` presentation booleans, remain clickable while locked, and have no scene/prefab instance in the audited workspace. They are not a current persistence fixture.
- The active stage-select surface now forwards the selected row's scene route and loading card, but both catalog rows still resolve the same Corridor definition and no canonical playable-stage identity is carried. P1-B must replace that raw/aliased projection before P1-D may bind persistent state to it.
- No PlayerPrefs, database, transaction library, or runtime gameplay-progress repository was found. Runtime writes found in the audit are benchmark/probe/capture/debug outputs; editor tooling also writes generated assets and reports, but neither is a player-progress owner.

The first persistent projection should therefore target the one corrected canonical stage-select entry after P1-B, not claim coverage from a nonexistent chapter-map instance. A later real chapter node must derive from the same state instead of reviving serialized clear/lock booleans.

## Evidence Boundary

The archive supports the authoring split, not the local persistence algorithm.

| Source | Directly observed | Local use | Not proven |
|---|---|---|---|
| HI3 `StageChallengeData` | Global snapshot: 489 rows with `challengeId`, `conditionId`, `paramList`, explanation, hint period, and literal `diaplayTarget` key; `StageData_Main` references challenges | separate typed objective semantics from explanation/display | decoded condition meaning, runtime evaluation, player progress, atomic save |
| GF2 `StageChallengeData` / `StageChallengeConditionData` | 448 objective rows with ID/type/title/description/args/argsFail, plus 43 condition-grammar rows with ID/charge-time/condition/fail fields | `objectiveId + closed kind + typed params`; never parse opaque formula strings | runtime binding, evaluation order, persistence |
| Ash Echoes level target/achievement data | mandatory targets separated from 148 optional achievement rows and 110 typed extra-target definitions | keep clear ownership separate from optional mastery; keep visibility/count display separate | evaluator execution, saved achievement state, reward grant |
| Blue Archive campaign data | stage star/challenge conditions separated from Default, FirstClear, and ThreeStar authoring buckets | objective and later reward-plan references remain separate | prior-state comparison, grant/save order, idempotency |
| Path to Nowhere achievement summary | 42 records with natural-language description plus independent hidden/order metadata | hidden/order are presentation only | typed evaluator or stage/result join |
| PGR course/practice data | course/practice tables separate prerequisite, threshold, and lesson references; `PracticeSkillDetails` separately owns presentation details | organization boundary only | P1-D evaluator or durable progress |

The `StageProgressState + StageProgressApplicationRecord + atomic store` design below is a DimensionBrawl safety contract derived from the local result-to-writer crash boundary and required duplicate/partial-publication failure cases. It is not claimed as copied behavior from another game.

## Decision Summary

| Concern | P1-D decision |
|---|---|
| outcome ownership | P1-A remains sole owner of Clear/Fail; mastery never changes outcome |
| evaluation input | entry-time deep snapshot plus a fully covered immutable P1-A fact candidate |
| evaluation timing | after outcome/facts seal and before final result digest/commit |
| first-slice eligibility | every mastery objective is `ClearOnly` and `failDoesNotBlockClear = true` |
| legacy runs | P1-A-only summaries remain `NotEvaluated` forever and are never backfilled |
| invalid bundle | one structurally invalid objective invalidates mastery for the bundle; Clear/count/best time remain valid |
| integrity fault | wrong run/digest, unavailable required collector, malformed fact snapshot, or evaluator exception faults before product result publication; it is not `NotAchieved` |
| objective identity | semantic meaning is immutable for the lifetime of an `objectiveId`; a semantic change requires a new ID |
| persistence input | only an authoritative P1-D-schema committed Clear |
| durable handoff | a self-contained prepared application intent is the durable result-to-writer boundary |
| atomic unit | one checksummed generation publishes the node state, global run ledger record, and intent transition together |
| first read models | shared result shell for run mastery; corrected canonical stage-select entry for durable clear/mastery |
| later extension | P2-C replaces the standalone writer for new cohorts and extends the same store; no dual write |

## Ownership

| Owner | May own | Must not own |
|---|---|---|
| `StageResultDefinition` | stable result-definition identity, mastery-objective-set reference, supported proof/fact requirements | mutable player progress, UI state, rewards |
| `MasteryObjectiveDefinition` | immutable semantic ID, closed condition kind, typed params, clear-only policy | outcome, runtime counters, copied display text as logic |
| `MasteryEvaluationPlanSnapshot` | run-entry deep copy of result/progression identity, objective semantics, fact capabilities, canonical digests | scene objects, mutable latest assets, player state |
| P1-A fact finalizer | immutable outcome, times, counts, semantic proof facts, collector coverage | objective interpretation, persistence |
| `MasteryEvaluator` | pure definition-plus-fact evaluation | UI, save calls, asset lookup, wall clock, rewards |
| `RunResultSummary` | immutable outcome/facts, semantic evaluation identity, separate presentation snapshot, evaluated mastery rows | mutable progress or grant state |
| `ProgressResolver` | pure prior-state plus committed-result transition and applied delta | storage, UI, latest authoring |
| `StageProgressStore` | prepared intents, state map, global application ledger, atomic generation publication | objective evaluation, navigation, reward payload |
| result/stage-select UI | read-only rendering of committed summary, application delta, and state | evaluation, duplicate decisions, save mutation |

Later P2-A rule, modifier, and enemy-variant IDs/digests are provenance only. P1-D must not convert a recommendation, applied restriction, modifier label, or variant purpose into mastery. A future relationship requires an explicit P1-A typed semantic-proof collector and a separately reviewed P1-D objective schema; names and display copy are never parsed.

Later P2-B course identity and traversal coverage are likewise provenance only. Basic lesson completion, a Free Practice exit, course entry order, presentation completion, and a P1-C gate are not mastery. P2-B may derive a read-only `course mastered` view only from a committed Clear plus the exact achieved objective row; it cannot write `StageProgressState`, prepare/apply an intent, or create an application record.

## Authored Contract

### `StageResultDefinition` P1-D fields

- `resultDefinitionId`
- monotonically increasing `definitionRevision`
- `evaluationContentDigest`
- `MasteryObjectiveSetDefinition masterySet`
- ordered required fact-capability IDs
- ordered allowed semantic-proof IDs
- presentation reference or localization namespace

The P1-B playable-stage spine owns the direct reference to this definition and to one explicit progression node. A P1-D run must not discover either from scene name, catalog row, current UI selection, or the latest asset after entry.

### `MasteryObjectiveSetDefinition`

- stable `objectiveSetId`
- monotonically increasing `setRevision`
- `semanticContentDigest`
- separate `presentationContentDigest`
- objective references stored without evaluator-significant display order

The semantic canonical form sorts objective records by ordinal `objectiveId` and covers every evaluation-significant field. Array order, localization, icon, visibility, display priority, and `setRevision` are excluded. Presentation ordering is derived separately by `displayPriority` then ordinal `objectiveId`. Reordering visible rows or bumping an audit/presentation revision therefore cannot change the evaluator/result/application identity, while a semantic edit cannot hide inside a display-only revision.

### `MasteryObjectiveIdentityManifest`

One project-owned, version-controlled authoring manifest is the narrow lifetime-identity authority:

- manifest schema version/revision and content digest;
- `objectiveId` -> immutable `semanticContentDigest`, first owning result/objective-set identity, and Active or Retired state;
- retained tombstones for every retired ID.

The manifest is append-only across accepted/published revisions. CI and the authoring validator compare the candidate against the last accepted manifest, not only against its current rows. Allowed evolution is a new unique Active row or `Active -> Retired`. Key removal, semantic-digest or first-owner mutation, `Retired -> Active`, and tombstone deletion/reuse are hard failures. The validator also compares every current objective definition with its retained row before admission. It permits presentation-only changes and contains no player progress or generic condition registry. Changing semantics requires a new objective ID; any explicit historical migration remains outside P1-D.

### `MasteryObjectiveDefinition`

Common fields:

- globally stable `objectiveId`
- `conditionKind`
- one discriminated typed parameter payload
- `eligibleOutcome = ClearOnly`
- `failDoesNotBlockClear = true`
- `explanationLocalizationKey`
- `visibility`
- `displayPriority`

First closed condition vocabulary:

| Kind | Typed parameters | Exact result rule |
|---|---|---|
| `ClearStage` | none | achieved only when authoritative outcome is Clear |
| `ClearUnderTime` | `metricKind` and positive `targetMilliseconds` | Clear and selected canonical elapsed value `<= targetMilliseconds` |
| `NoPlayerDown` | none | Clear and final `playerDownCount == 0` |
| `PerfectDodgeCount` | positive `minimumCount` | Clear and canonical count `>= minimumCount` |
| `UseSummonForNeed` | one qualified `semanticProofId` and positive `minimumQualifiedCount` | Clear and matching qualified semantic-proof occurrences `>= minimumQualifiedCount` |

`metricKind` is closed to P1-A's `TotalActive` or `CombatActive` measures. The persistent first-slice best result is explicitly `bestTotalActive = None | Present(totalActiveElapsedMilliseconds, winningRunId)`. No objective is authored from `LinearStageTemplateProfile.targetRunDurationSeconds` until the target and integer fact contract are reviewed.

`UseSummonForNeed` never counts raw summon button use, slot use, energy spend, a result readout string, or a P1-C completion gate. It consumes only a P1-A `SemanticProofFact` whose exact proof ID and qualification adapter were admitted with the run.

### Objective-ID permanence

`objectiveId` is a lifetime progress key.

Changing any of the following requires a new objective ID, a new definition revision, and an explicit authoring migration decision:

- condition kind;
- threshold, comparator, or time metric;
- semantic proof ID or its qualification meaning;
- eligible outcome;
- any parameter that can change whether the same immutable run facts achieve the objective.

Localization, icon, visibility, and display order may change under the same objective ID because they do not alter evaluation. Deleted IDs remain tombstoned and cannot be reused. Automatic retuning or migration of historical mastery is outside P1-D.

## Entry-Time Snapshot

### `MasteryEvaluationPlanSnapshot`

Captured once at logical stage admission:

- `snapshotSchemaVersion`
- `runId`, `playableStageId`, `routeRevision`, and route digest
- `progressionNodeId`, node revision, and binding digest
- `resultDefinitionId`, definition revision, and evaluation-content digest
- objective-set ID/revision plus semantic and presentation content digests
- objective-identity-manifest revision/global digest for audit plus the immutable entry semantic digest for every snapshotted objective ID
- deep-copied objective semantic records, each assigned one unique immutable `snapshotObjectiveOrdinal` from the objective set's semantic serialized order before validation; duplicate/unknown objective IDs remain distinguishable by ordinal for diagnostic output
- presentation metadata snapshot needed by the result shell
- required fact-capability IDs
- allowed semantic-proof vocabulary and qualification versions
- P1-A fact schema version and collector-capability digest
- canonical `evaluationSnapshotDigest`
- separate `presentationSnapshotDigest`
- full-envelope checksum for corruption detection only

`evaluationSnapshotDigest` covers result-definition ID plus evaluation-content digest, objective-set ID plus semantic-content digest, every objective row in `snapshotObjectiveOrdinal` order including that ordinal and each referenced manifest-entry semantic digest or invalid raw identity evidence, required fact capabilities, allowed qualified proof vocabulary, and fact-schema/capability identity. Duplicate/missing ordinals fault snapshot construction; a valid bundle additionally requires unique objective IDs. It excludes definition/set audit revisions, presentation metadata/digest, and the global manifest revision/digest so unrelated objective additions or copy reordering do not change a run's semantic identity. `presentationSnapshotDigest` covers the copied localization/visibility/order metadata. The full-envelope checksum protects both snapshots in storage/transport but never participates in evaluation, progress duplicate identity, or settlement eligibility.

Admission fails closed for a P1-D-capable run when any of these are missing, duplicated, unsupported, noncanonical, or contradictory. It may explicitly admit the stage under an older P1-A schema cohort before gameplay starts, but it cannot silently downgrade an already admitted P1-D run to `NotEvaluated`.

P2-C later embeds or extends this snapshot with prerequisite graph and reward-plan revisions. It does not create a second objective snapshot.

## Fact Coverage and Canonical Values

P1-D consumes only sealed P1-A facts.

Required first-slice coverage includes:

- authoritative outcome;
- canonical nonnegative integer `totalActiveElapsedMilliseconds` and `combatActiveElapsedMilliseconds`;
- final `playerDownCount`;
- `perfectDodgeCount`;
- ordered qualified `SemanticProofFact` records;
- a collector-coverage record proving each required adapter was bound for its complete applicable phase.

The P1-A clock adapter accumulates nonnegative integer monotonic tick deltas across all active intervals under one stable per-run frequency; it never accumulates float seconds or rounds each interval separately. At final seal it converts the total once to nonnegative integer milliseconds using an overflow-safe integer quotient/remainder implementation of `ceil(totalActiveTicks * 1000 / frequency)`. Ceiling is the conservative boundary: a positive sub-millisecond excess cannot pass a `<=` target. Zero remains zero. Frequency change, negative delta, checked overflow, or an unsupported conversion range is an integrity fault before result publication. UI seconds are derived from the sealed integers, and the evaluator/state store never reconvert float seconds.

A successfully covered proof adapter with zero matching facts means zero and may produce `NotAchieved`. A missing, late-bound, interrupted, or wrong-generation adapter is not zero. It is an invalid evaluation input and faults before result publication.

P1-C sequence/group completion and completion-gate satisfaction remain execution facts. They become mastery evidence only through an explicitly authored P1-A semantic-proof adapter; no name or gate ID is inferred as correct summon use.

## Evaluation Lifecycle

For a P1-D-only run:

`TerminalClosed -> TerminalFinalizing -> OutcomeFactsSealed -> MasteryEvaluating -> MasterySealed -> CommitRequested -> Committed`

For a combined P1-D + P2-A run, the admitted variability barrier remains between mastery and commit:

`TerminalClosed -> TerminalFinalizing -> OutcomeFactsSealed -> MasteryEvaluating -> MasterySealed -> VariabilityClosing -> VariabilitySealed -> CommitRequested -> Committed`

`OutcomeFactsSealed`, `MasteryEvaluating`, and every later admitted pre-commit barrier occur before `CommitRequested` so an integrity failure has a truthful pre-publication fault path. No objective evaluator runs in UI, after summary commit, or against mutable latest authoring.

The evaluator is a total pure function:

`MasteryEvaluationPlanSnapshot + ImmutableRunFactCandidate -> MasteryEvaluationResult`

It does not use scene objects, services, save state, asset lookup, localization text, render frames, time scale, wall clock, randomness, or progress history.

### `ImmutableRunFactCandidate`

P1-A seals one runtime-issued `runFactCandidateId` at `OutcomeFactsSealed`. It contains run/stage/route identity and route digest; exact `StageOutcomeFact` plus `stageOutcomeFactDigest`; canonical nonnegative integer total/combat active milliseconds; and the following canonical collections. Embedded segment/combat/proof types are inlined, while P1-A tutorial facts use their now-defined exact envelopes:

- segment rows containing every `StageSceneSegmentState` field, ordered by unique `segmentSequenceIndex`;
- tutorial rows containing exact `tutorialFactId`/`tutorialAttemptFactDigest` plus the complete fact-scope/attempt-state/termination/proof-disposition/value/elapsed/segment fields and, for P1-E rows, complete plan/lesson/attempt/generation/evaluation/collector/disposition provenance; the route-summary row also carries exact `tutorialFactCoverageDigest`; rows order route-summary first and then snapshotted plan ordinal plus attempt ordinal, and recomputing any fact digest must match its envelope;
- one fixed combat row containing resolved damage/down/dodge/summon records and closed typed `None | Present` arms for forward-risk and structure-break values; summon records use their stable admission sequence;
- semantic-proof value rows containing `proofId`, `sourceKind`, count, typed actual value, first-observed segment milliseconds, and qualified state, ordered by `(proofId, sourceKind)` with duplicate keys rejected; and
- exactly one `BoundComplete(capabilityId, collectorGeneration, boundStartSequence, boundEndSequence, coverageDigest)` row for every capability promised by the entry snapshot, in immutable capability ordinal.

It also contains exact `evaluationSnapshotDigest`, canonical `runFactCandidateDigest`, and envelope checksum. The candidate digest covers its runtime ID, every inline row or exact tutorial fact/coverage digest and typed absence in those exact orders, full successful collector coverage, stage-outcome/evaluation identities, and all scalar values while excluding presentation metadata and every envelope checksum. Player-down and perfect-dodge projections are derived only from the one fixed combat row; no duplicate scalar authority exists. An integrity-fault/missing/late-bound collector row belongs only to finalization-fault evidence and prevents this candidate from sealing; it is never a candidate arm or an observed zero.

### `MasteryEvaluationResult`

The pure evaluator returns exact `runFactCandidateId`/`runFactCandidateDigest`, exact `evaluationSnapshotDigest`, aggregate `Evaluated | InvalidDefinition`, and exactly one `MasteryObjectiveResult` for every snapshotted objective. Rows remain in immutable `snapshotObjectiveOrdinal`; no ordinal may be omitted, duplicated, or added from newer authoring. Stable objective ID must also be unique only when the aggregate is `Evaluated`; an `InvalidDefinition` bundle preserves duplicate/unknown identity evidence by ordinal. It also returns canonical `masteryEvaluationResultDigest`. That digest covers the two exact input digests, aggregate arm, objective cardinality/order, and every complete discriminated row including typed absences. `NotEvaluated` is a legacy summary state and is never an evaluator output.

### Aggregate state

P1-D retains the current three aggregate states with strict meanings:

- `NotEvaluated`: P1-A/legacy result schema only. It is forbidden for a successfully admitted P1-D run.
- `Evaluated`: the complete snapshotted bundle is structurally valid and every row has a deterministic result.
- `InvalidDefinition`: one or more objective definitions escaped admission validation as structurally invalid. The whole bundle is mastery-ineligible for this run.

Every first-slice objective is `ClearOnly`. A Fail result still produces `Evaluated` rows, all `NotAchieved`, and no progress application. UI must not phrase this as a system failure or a lost saved achievement.

A bundle-level `InvalidDefinition`:

- never changes Clear to Fail;
- retains exactly one ordinal-keyed evaluated/diagnostic row per snapshotted objective;
- persists no mastery ID from that run;
- still permits clear count, first clear, and best-total-time application;
- is shown as unavailable/configuration-invalid, never as a missed objective.

A wrong run/snapshot digest, fact integrity failure, required-capability loss, or unexpected evaluator exception is not an invalid authored objective. It enters the P1-A diagnostic finalization-fault path before summary publication and creates no progress intent, state, or application record.

### `MasteryObjectiveResult`

Each row is one closed union:

- `Evaluated(snapshotObjectiveOrdinal, objectiveId, known objective kind, objectiveSemanticDigest, Achieved | NotAchieved, typed MasteryMeasuredValue actual, typed MasteryMeasuredValue target, ordered contributing semantic proof IDs)`; or
- `InvalidDefinition(snapshotObjectiveOrdinal, objectiveIdentityEvidence, kindEvidence = KnownKind(kind) | UnknownKind(raw token/hash), invalidPayloadDigest, diagnosticCode, typed absence of actual/target/proof IDs)`.

`objectiveIdentityEvidence` preserves the snapshotted raw/stable ID token and digest even when it duplicates another row; ordinal, not that invalid ID, remains the unique row key. `MasteryMeasuredValue` is a closed union of Boolean, Count, Milliseconds, and SemanticProofCount. UI formats it; it never parses it. `Evaluated` requires matching actual/target union arms for the known objective kind. `InvalidDefinition` cannot fabricate a comparable value or proof contribution when identity, kind, or payload is invalid. The semantic `resultSummaryDigest` covers `evaluationSnapshotDigest`, aggregate state, rows in `snapshotObjectiveOrdinal` order, each full row arm and typed absence, values/proof IDs when evaluated, invalid identity/kind/payload evidence when invalid, and the other authoritative run facts. It excludes `presentationSnapshotDigest`; an optional result-envelope checksum may protect the complete UI payload separately.

## Durable Progress Handoff

Atomic state-plus-record publication alone does not close the process-crash window between an in-memory summary commit and the writer receiving it. P1-D therefore adds an internal durable intent.

### `StageProgressApplicationIntent`

- `intentSchemaVersion`
- `runId`
- `playableStageId` and route revision/digest
- `progressionNodeId` and binding digest
- final candidate `resultSummaryDigest`
- authoritative Clear outcome
- aggregate mastery state
- ordered achieved mastery IDs only when the bundle is `Evaluated`
- `totalActiveElapsedMilliseconds`
- objective-set ID plus semantic-content digest and `evaluationSnapshotDigest`
- self-contained canonical `inputFingerprint`
- preparation generation and checksum
- internal state `Prepared`

For a P1-D Clear, preparing this exact self-contained intent in the progress store is the durable result-commit boundary. Only after the store acknowledges the `Prepared` generation may the immutable summary transition to `Committed`. An exact prepared intent is recoverable on startup even if the process dies before the in-memory commit event or writer response. The candidate `resultSummaryDigest` already contains `outcomeFactsSealedAtSequence` and every final semantic fact; it never contains the later actual commit sequence. P1-A records that later sequence only in the exact `RunResultCommitReceipt` sealed with `CommitRequested -> Committed`, outside this intent's fingerprint and outside `resultSummaryDigest`, so acknowledging `Prepared` cannot require a post-prepare digest rewrite.

`inputFingerprint` canonically covers, in order, `intentSchemaVersion`; run ID; playable-stage ID; route revision/digest; progression-node ID and binding digest; final `resultSummaryDigest`; fixed Clear outcome; aggregate mastery arm; achieved mastery IDs in immutable snapshot-objective ordinal (or a typed empty array for `InvalidDefinition`); integer total-active milliseconds; objective-set ID/semantic-content digest; and `evaluationSnapshotDigest`. It includes every typed empty/absence arm and excludes the fingerprint itself, preparation generation, internal `Prepared` state, checksum, presentation metadata, and later commit/application fields. Exact duplicates compare this complete tuple, not a caller-selected subset.

A Fail result creates no progress intent or application record. An abort, stale callback, direct-Station diagnostic run, or P1-A `NotEvaluated` legacy summary also creates none.

The prepared intent is internal recovery state, not a public application record and not a reward receipt. It is atomically consumed into the state plus committed record. It is never presented as durable progress on its own.

For a later P2-C-schema cohort, the standalone P1-D writer is disabled and this pre-commit boundary prepares one schema-extended, self-contained `StageSettlementSourceIntent` instead of the narrow intent. That source additionally contains the complete final result candidate and frozen settlement authoring snapshot needed to recover reward resolution without a later in-memory callback. It is consumed only by the combined P2-C transaction; the two intents/writers never run side by side.

## Persistent Contracts

### `StageProgressState`

Keyed by `progressionNodeId`:

- `stateSchemaVersion`
- `progressionNodeId`
- monotonically increasing `stateRevision`
- nonnegative `clearCount`
- write-once `firstClear = None | Present(runId)`
- canonical ordinal-sorted unique `achievedMastery[]`, each row containing `objectiveId` and its write-once `firstAchievementRunId`
- exact `bestTotalActive = None | Present(nonnegative milliseconds, winningRunId)`

Rules:

- an absent node key may bootstrap exactly one canonical empty state only when the Prepared intent carries the admitted progression-node/binding identity: `stateRevision = 0`, `clearCount = 0`, no first IDs, no achieved mastery, and no best time. Its first successful application commits revision 1;
- an absent state with an existing record for that node/run domain, an unadmitted node, or malformed nonempty bootstrap data is corruption and cannot be recreated from UI/latest authoring;
- one distinct committed Clear increments `clearCount` exactly once;
- `firstClear` changes from `None` to `Present(applied runId)` exactly on the first successful application and is never replaced;
- mastery IDs are appended logically then serialized in ordinal ID order, never display order;
- first-achievement provenance is written by the first transaction that adds that ID;
- best total-active time changes only for a valid Clear whose integer value is strictly smaller; an equal time retains existing provenance;
- retired objectives remain in history;
- scene, catalog, display order, and lexical stage order never key the state.
- `clearCount = 0` if and only if `firstClear=None`, `bestTotalActive=None`, and `achievedMastery[]` is empty; `clearCount > 0` requires both provenance arms `Present`. Achieved mastery rows may still be empty after a clear, but any achieved row requires `clearCount > 0`.

### `StageProgressAppliedDelta`

Stored in the application record:

- prior and committed clear counts
- exact `firstClearChange = Established(runId) | Unchanged(existingFirstClearRunId)`
- unique `newlyAchievedMasteryIds` sorted by the same canonical ordinal `objectiveId` comparator used by persistent state, projected from evaluation snapshot order before storage
- exact `bestTimeChange = Established(afterMilliseconds, winningRunId) | Improved(beforeMilliseconds, priorWinningRunId, afterMilliseconds, winningRunId) | Unchanged(currentBest = Present(milliseconds, winningRunId), evaluatedRunId)`
- prior and committed state revisions
- canonical `progressAppliedDeltaDigest`

The first-clear `Established` arm requires prior clear count zero/`firstClear=None`, committed clear count one, and the applied run ID; its `Unchanged` arm requires the exact prior/committed winner. Best-time `Established` is mandatory when the prior best is `None`. `Improved` requires `afterMilliseconds < beforeMilliseconds`; its new winning run is the applied run. Equal or worse time uses `Unchanged`, whose before/after provenance is the identical present `currentBest` arm and whose `evaluatedRunId` is the applied run, not a replacement winner. Duplicate newly achieved IDs fault before commit. The delta digest covers counts, the complete first-clear arm, ordered newly achieved IDs, the complete best-time arm including provenance, and state revisions. This delta lets duplicate requests and reopened result UI reproduce the same `NEW`/first-effect answer without re-resolving against newer state.

### `StageProgressApplicationRecord`

- `recordSchemaVersion`
- deterministic `progressApplicationRecordId = UUIDv5(fixed P1-D record namespace, runId + progressionNodeId + inputFingerprint)`
- globally unique `runId`
- `progressionNodeId`
- `resultSummaryDigest` and input fingerprint
- objective-set ID plus semantic-content digest and `evaluationSnapshotDigest`
- prior and committed `stateRevision`
- `StageProgressAppliedDelta`
- store commit generation and stable audit sequence
- public `status = Committed`

Exact duplicate `{ runId, progressionNodeId, resultSummaryDigest, evaluationSnapshotDigest, inputFingerprint }` returns the stored record and delta. These identities exclude presentation/global-manifest churn. The same run ID with a different node or semantic digest/fingerprint is rejected and audited without changing the healthy intent/record/state. Retention covers the supported save lifetime; `lastAppliedRunId` is forbidden.

The UUID namespace is a fixed schema constant and cannot vary by install/profile. A generated ID collision whose stored tuple differs is a corruption/integrity fault; the writer neither overwrites that record nor retries under a random ID.

## Store Boundary

### `StageProgressStoreRoot`

One versioned logical aggregate contains:

- store schema version;
- save-profile namespace;
- monotonically increasing store generation;
- canonical map of `progressionNodeId -> StageProgressState`;
- global `runId -> StageProgressApplicationRecord` ledger;
- internal `runId -> StageProgressApplicationIntent` map;
- commit sequence;
- envelope content digest/checksum.

P1-D0 must select one concrete local implementation that can fault-inject this boundary. The first implementation may use an atomic transactional backend or a checksummed generation/journal with an atomic publish pointer. Separate state and record files, PlayerPrefs, and fire-and-forget writes do not satisfy the contract.

Readers observe only a completely published valid generation. Startup chooses the newest valid committed generation and recovers every exact Prepared intent before accepting state-dependent navigation or another conflicting application. A corrupt newest generation falls back only when the prior generation is independently valid and the publish protocol proves the corrupt candidate was never committed; ambiguous committed corruption is quarantined, not guessed or re-applied.

## Application Algorithm

1. Accept only the exact prepared intent/source fingerprint created by the authoritative P1-D Clear commit.
2. Look up the global run ledger.
3. Return an exact existing record/delta; reject a same-run mismatch.
4. Look up the exact prepared intent. Missing intent with no record is corruption, not permission to synthesize from current assets.
5. Read the current node state and expected store generation/state revision, or construct only the canonical admitted revision-0 empty bootstrap defined above.
6. Purely resolve clear count, first clear, newly achieved IDs, first-achievement provenance, and best total-active time.
7. Build the complete next state, applied delta, and committed record.
8. Compare-and-swap the expected generation/revision and atomically publish next state plus ledger record while consuming the intent.
9. On a distinct-run conflict, discard the stale proposal, re-read, and rerun pure resolution.
10. Publish the application record/read models only after the committed generation is visible.

For two distinct first-clear or first-mastery runs, both records eventually commit and both increment clear count. Exactly one transaction wins each write-once first field in commit order. The losing transaction re-resolves and records an empty corresponding `newlyAchieved` delta. Best time is the minimum valid stored integer.

## Result and Navigation Barrier

The normal result shell may render run mastery only from the immutable committed summary. It may render `NEW`, first-clear, persistent clear count, or best-time change only from the committed application record/delta.

For a P1-D Clear:

`Committed summary -> ProgressApplying/Recovering -> ProgressCommitted -> ResultReady -> terminal action selection`

Replay/Lobby and any persistence-dependent projection remain disabled until `ProgressCommitted`. A prepared or retrying intent may expose a diagnostic `saving progress` state, but it cannot claim `NEW`, cleared, unlocked, or saved.

The first slice provides retry/resume of the same intent. It does not silently enable normal navigation on a permanent storage fault. Any future `exit without saving` escape requires an explicit product decision and must state that the run will not be durable; it cannot masquerade as the normal Lobby action.

Fail has no persistence barrier and uses the P1-A committed result/action path. It never creates a zero-delta progress record merely to drive UI.

For a later course-capable run, Clear without the exact `UseSummonForNeed` row Achieved remains a truthful Clear and follows the ordinary persistence contract, but no course-mastery view may be shown. Course traversal coverage cannot substitute for the objective row. P2-B remains read-only before, during, and after `ProgressCommitted`.

## First Product Fixture

After P0 through P1-C close:

1. P1-B authors one real `StageResultDefinition` and one explicit progression-node join on the approved Olympus playable-stage route.
2. P1-A supplies complete integer-time, player-down, perfect-dodge, and semantic-proof coverage.
3. The shared result shell renders one control objective, `NoPlayerDown`.
4. Add one identity objective, `UseSummonForNeed`, only after product review freezes one actually emitted qualified semantic proof ID. If that proof is not ready, ship the one-objective fixture rather than a placeholder or raw summon-use check.
5. Define `ClearStage`, `PerfectDodgeCount`, and `ClearUnderTime` in evaluator unit coverage, but do not author a product time objective until the active-time integer fact and target threshold are approved.
6. Apply the result to one progression node and project `clearCount > 0` plus achieved IDs into the corrected canonical stage-select entry.
7. Keep prototype chapter `locked/cleared` fields ignored or warning-only. Bind a chapter node only after a real instance and typed prerequisite projection exist.

The exact objective IDs, proof ID, progression-node ID, save-profile namespace, and result-view copy remain P1-D0 review values. Existing template prose, fictional `nextStageId`, and review reward hooks are not defaults.

## Validation Matrix

| Check | Hard failure |
|---|---|
| result/progression join | missing, duplicate, wrong-route, or inferred from scene/catalog/latest asset |
| snapshot | shallow copy, noncanonical ordering, missing revision/digest, or changed semantics without route/result revision |
| objective ID | empty/duplicate, missing manifest entry, candidate-versus-last-accepted manifest row removal/mutation, global/per-entry digest mismatch, tombstone deletion/reactivation/reuse, or semantic change under any existing ID |
| objective params | unsupported kind, wrong union arm, nonpositive threshold/count, unsupported metric, or unapproved proof ID |
| clear ownership | `failDoesNotBlockClear != true`, non-Clear eligibility, or evaluator can change outcome |
| fact capability | required adapter unavailable, partial-phase coverage, wrong generation, or absence conflated with zero |
| timing | float comparison, live clock read, unspecified metric, or conversion after fact seal |
| semantic proof | inferred from raw summon use, UI text, P1-C gate name, or unqualified event |
| result lifecycle | evaluation after commit, mutable result rows, or P1-D run silently left `NotEvaluated` |
| invalid bundle | partial mastery persisted under aggregate `InvalidDefinition` |
| progress input | Fail/abort/stale/uncommitted/legacy run, caller-supplied node, or latest-authoring reinterpretation |
| intent | result committed/acknowledged before durable exact intent, non-self-contained recovery data, or conflicting same-run overwrite |
| store atomicity | state-only, record-only, consumed-intent-only visibility, or no recoverable generation |
| duplicate | exact retry increments again or mismatch mutates healthy data |
| concurrency | stale proposed state retried without fresh resolution |
| UI | evaluates conditions, claims persisted state from summary alone, or uses serialized lock/clear as truth |
| scope | reward, currency, score/rank, generic DSL, or broad chapter framework enters P1-D |

## Acceptance Matrix

| Scenario | Required result |
|---|---|
| asset edits after run entry | snapshot semantics and digest remain unchanged |
| objective semantics edited under same ID | authoring validation fails |
| exact time boundary / one millisecond over | `<=` boundary passes; +1 ms fails |
| sub-millisecond clock boundary | exact integer conversion uses one final ceiling; a positive fractional-millisecond excess cannot be truncated into a pass |
| timing frequency change/negative delta/overflow | pre-publication integrity fault; no intent/state/record |
| Clear plus final player down in the same terminal epoch | `NoPlayerDown` is not achieved |
| raw summon use without qualified proof | `UseSummonForNeed` is not achieved |
| Basic completion, Practice exit, presentation completion, or P2-A recommendation | no mastery row changes; none is qualified summon proof |
| qualified proof under full collector coverage | objective achieves with contributing proof ID |
| missing proof collector | integrity fault or admission failure, never zero/not-achieved |
| Fail | evaluated clear-only rows are not achieved; no intent, state, or record |
| abort/stale/direct Station | no product mastery/progress artifacts |
| invalid objective bundle on otherwise truthful Clear | summary marks `InvalidDefinition`; clear/count/best may persist; no mastery ID persists |
| evaluator exception/digest mismatch | pre-publication diagnostic finalization fault; no intent/state/record |
| crash before intent prepare | no acknowledged Clear and no store mutation |
| crash after Prepared before in-memory response | startup recovers exact intent and applies once |
| interruption during atomic application | old state plus Prepared intent remains; no partial record/state |
| crash after commit before response | exact lookup returns the same record and delta |
| exact duplicate after restart | same record/delta, no second increment |
| same run with different node/digest/fingerprint/objective identity | reject/audit, preserve healthy data |
| two distinct first clears race | two clear increments, one first-clear winner |
| two distinct first mastery runs race | one new-achievement winner; loser re-resolves |
| newest generation corrupt/ambiguous | valid recovery or quarantine; never state-only/record-only repair by guess |
| result UI reopened | run row comes from summary; `NEW` comes from stored delta |
| course Clear without exact achieved row | Clear persists normally; derived course mastery remains false |
| P2-B course write attempt | no progress intent/state/application/save mutation path exists |
| restart projection | corrected stage-select entry agrees with committed state |
| reward/currency check | no payload, balance, receipt, or grant path exists |

## Bounded Delivery Order

### P1-D0 — Approvals and fixture readiness

- close P0/P1-0/P1-A/P1-B/P1-C gates;
- approve objective ID permanence/identity manifest, objective-set canonicalization, invalid-bundle policy, time metric, durable-intent acknowledgment boundary, save-profile namespace, and concrete atomic store;
- author the result definition, progression-node join, corrected stage-select projection, and exact first proof fixture;
- identify automated test owners.

### P1-D1 — Definition, snapshot, and pure evaluator

- add the closed objective union and authoring validator;
- deep-snapshot result/progression/objective/fact-capability identity at entry;
- add integer fact finalization and coverage proof;
- test every first vocabulary kind without UI or persistence.

### P1-D2 — Result finalization integration

- add `OutcomeFactsSealed -> MasteryEvaluating -> MasterySealed` before `CommitRequested`;
- finalize aggregate state, typed rows, and result digest exactly once;
- keep P1-A legacy summaries immutable and `NotEvaluated`.

### P1-D3 — Intent, resolver, and atomic store

- implement the self-contained Prepared intent;
- implement pure state/delta resolution;
- publish state, ledger record, and intent consumption in one generation CAS;
- run duplicate, conflict, concurrency, restart, corruption, and fault-injection tests.

### P1-D4 — Read-only product projection

- render mastery from the committed summary and `NEW` from the stored delta in the shared result shell;
- project durable clear/mastery into one corrected stage-select entry;
- keep terminal navigation behind `ProgressCommitted`;
- do not introduce rewards or a broad chapter framework.

## Explicit Deferrals

- reward eligibility, first/repeat/mastery payout, currency, inventory, receipts, growth, and claim UI: P2-C;
- generic score, rank, stars, maximum combo, leaderboard, or analytics;
- generic condition DSL, reflection registry, formula parsing, or external game's numeric condition codes;
- blocking mastery/clear conditions, stage rules, or outcome reversal;
- tutorial completion extraction: P1-E;
- P1-C encounter execution or completion-gate ownership;
- story, replay, retry, lobby, or post-clear hook ownership beyond the persistence barrier;
- historical P1-A backfill, automatic objective-retune migration, or deleted-ID reuse;
- cloud/account/network synchronization, anti-cheat, multi-profile merge, or server authority;
- broad chapter graph/editor work or unlock/reward settlement;
- product `ClearUnderTime` authoring before active-time and threshold review.

## Promotion Gate

P1-D may enter production only when all answers are yes:

1. Are P0 and P1-0/P1-A/P1-B/P1-C predecessors current and approved?
2. Does the run deep-snapshot one truthful result/progression/objective contract before gameplay?
3. Can every objective distinguish observed zero from unavailable coverage?
4. Is objective semantic identity immutable for persisted history?
5. Does the pre-commit evaluator produce a deterministic final summary without UI or latest-asset reads?
6. Does a durable prepared intent close the result-to-writer crash window before acknowledgment?
7. Can one fault-injected store prove state, record, and intent transition are atomic and idempotent?
8. Do result and stage-select views remain read-only consumers of committed data?
9. Are rewards, generic score/rank, and broad chapter systems still absent?
