# Ordered Encounter Execution Bridge Spec

## Current P1-B closure

- P1-B Station Add and full-exit closure (2026-07-16): `SNAP-P1B-STATION-ADD-AUTHORING-REMEDIATION3-ACCEPTED-11` binds `C:\tmp\DimensionBrawl-P1B-StationAdd-Remediation3-Bundle.md` at SHA-256 `9378bc021b09495c350b331a85755eac7b956a2372d78ecca848a94c2d570c76`; source `128/128` matches digest `4c3dbe952bea5e4f5c57632d70e6fba815d7f6900dc9e1dcbee6af69bae86c89`, artifacts `11/11` match digest `eb5699917083d9be13d571f2a64aa0f69048304552b962df3467b89f3469ce2b`, validator/inventory `8/4/1/1/0`, integrated focused `8/8`, Canonical UI `34/34`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `99/99` all pass with three independent audits at blocker `0`. Revision-1 pose remains relative to `StageDefinitionSceneBinding.transform`; Station `MapRoot` is topology containment only. `ACC-P1B-STATION-ADD-AUTHORING = PASS`; the foreign-evidence row remains PASS through explicit rejection only; `SNAP-P1B-FULL-EXIT-ACCEPTED-12` closes `ACC-P1B-FULL-EXIT-AUDIT = PASS`, so P1-B is **ACCEPTED / VERIFIED-COMPLETE**. This admits no P1-C runtime owner: only the prospective authoring-ledger freeze may start, and runtime work remains gated by `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`.

## Status

- P1-B result/progression Remediation3 acceptance: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION3-ACCEPTED-08` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation3-Bundle.md` at SHA-256 `94fa969979bdb2a2b91dfbdf8a5395aed0a69ddd8907831bb7c99da06b139a5b`; source `116/116` matches digest `271793a22e2afc24779a3aeeace7cb9768aae77b7bbbf18a075fa15ea409efb2`, artifacts `14/14` match list digest `c3642305e13c085f710e8db62df807463aea58d8a57331cd7526460eb7a404fc`, validator/inventory `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `98/98` all pass. Independent source, artifact/test, and semantic-contract audits find blocker `0`: route/sidecar-owned canonical catalog identity is independent of the result definition, public Corridor admission and the editor validator require exact object identity, and catalog-only plus coherent catalog/profile/localization clones reject before run creation. Frozen route/policy/join/lifetime digests remain unchanged. `ACC-P1B-RESULT-PROGRESSION-JOINS = PASS / VERIFIED PARTIAL`; Candidate-07 remains immutable historical FAIL. Station count-one Add authoring is now unheld as the next separate P1-B gate, while live PGR/HI3 disposition, P1-B full exit, and P1-C execution remain OPEN and no P1-D/P2-C owner is admitted.
- P1-B result/progression Remediation2 candidate audit: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION2-CANDIDATE-07` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation2-Bundle.md` at SHA-256 `a4e2e2873ec4f53ba81a6c6a3269949b4b2f19255f566d333fcb058e3eeb6de8`; its submitted source manifest matches `116/116` with digest `f4c6f0a6065a2f304acd1a56f7d126b4b2be49582f752f707757d87f37c35583`, all `14/14` artifacts match list digest `96176b861dc7ce0a9aaccd86fe035aa59433513383713132248e51f974b6228a`, validator/inventory is `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact full route `1/1`, and graphics aggregate `98/98` pass. Independent source/contract/test audits verify that Candidate-06's three blocker groups, locale/graph rows, and exact durable-decision byte preservation are closed, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / VERIFIED-FAILED-CANDIDATE-PARTIAL` on one remaining admission defect: the result definition self-selects its catalog, so a catalog-only clone or coherent catalog/profile/localization clone can evade the intended exact-identity gate. The post-bundle route-owned catalog-anchor WIP changes five submitted files and cannot retroactively amend this cutoff. Station Add and P1-B full exit remain held until a new sealed-source candidate passes.
- P1-B result/progression joint-freeze: `P1B-RESULT-PROGRESSION-JOINS-01` Rev3B proposal artifacts match SHA-256 `b6e63b11e3e270302dc33f95b7b69740565e4e27a13ffe017a17f2899256c88f` / `eb65cf30eb961a271f135bc38a9874cccae49e47d8a9d0af5a6dd5f0d7211199` / `933c13943e5397f5fa7a1be531ae34bd28f595e09feee14f18429daa81a8e603`. Fresh PowerShell, independent Node, and a third row reconstruction preserve the seven `15/35/15/17/8/9/38` blocks, sidecar/join snapshot digest `a2ae9df451bd6f2ff48b83098db3bfbdaf2120e23dfaf3612a31f18a022c41fa`, all predecessor digests, and the separate 11-row lifetime-contract digest `3b6cf33325a0a83db74ee2253da9799e589b5664f4fb677b2b021389b0714c0e`. Exact `(ID, revision)` edge resolution and the no-token `Stage Select A -> pre-admission mutation B -> fresh Corridor B` boundary pass. Verdict is **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**. This authorizes implementation only: `ACC-P1B-RESULT-PROGRESSION-JOINS`, Station Add, foreign evidence, and P1-B full exit remain **OPEN**, and no P1-C/P1-D/P2-C owner or P1-A digest change is admitted.
- P1-B result/progression Rev3B implementation candidate audit: `C:\tmp\DimensionBrawl-P1B-ResultProgression-Implementation-Bundle.md` matches SHA-256 `35b1b1a5523bc457ad1936190d1d41143dd1bc8a3489624cdb600631c3a6daa1`; submitted source manifest `116/116` matches digest `1b3dba021b40a4be9d728c6fd4f2039864abb399bbff6d2907e4af274bec24ec`, all `14/14` declared artifacts match list digest `249da60824d3ef617937e648e1257b1fde9b50dc28082a904b78513ca7c76023`, both contract verifiers pass, validator/inventory is `8/4/1/1/0`, focused `2/2`, Canonical UI `28/28`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `93/93` pass. These green artifacts are verified, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / SOURCE-CONTRACT-FAILED-CANDIDATE`: canonical profile/localization object identity is not enforced at admission, the `Presented -> terminal action` path omits the exact pinned join/presentation/audit authority gate and audit self-integrity, and representative deep snapshot damage can throw instead of returning a typed rejection. Direct clone/damage/dispatch, recovery/process-loss, locale, and production graph acceptance rows remain open. The Rev3B joint freeze and every accepted predecessor cutoff/digest remain unchanged; Station Add and P1-B full exit stay held pending remediation and a new sealed-source bundle.
- Drafted: 2026-07-14
- Status: provisional P1-C review contract; analysis only
- Roadmap source: [Subculture Dataset Gap Roadmap](SUBCULTURE_DATASET_GAP_ROADMAP.md), P1-C
- Identity and authoring companion: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md), P1-0/P1-B
- Run-lifecycle companion: [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md), P1-A
- Later mastery/progress companion: [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md), P1-D
- Later variability companion: [Stage Rule, Modifier, and Enemy Variant Spec](STAGE_RULE_MODIFIER_ENEMY_VARIANT_SPEC.md), P2-A
- Later course-chain companion: [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md), P2-B
- Product-decision companion: [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md)
- Archive root: `\\DESKTOP-69817L3\ArkData\SubcultureGameData`
- Current freeze state: **not freeze-ready**. No current route pocket has a concrete, non-placeholder, scene-resolvable `Add` spawn fixture.
- P1-A predecessor snapshot: the historical 45/49/54/59/68/75 cutoffs remain non-additive evidence. The later unchanged-source current-schema exit cutoff matches 11/11 source hashes under manifest digest `e59884ca0bcbec0506502ccb2638d9227e5f098bfb7f271e3a7adf16a2656427` and passes Combat 21/21, StageRun 23/23, canonical UI 15/15, graphics aggregate 79/79, canonical full route 1/1, and compile/validator checks. Independent source audit closes exact duplicate-request identity, direct replacement coordinator cancellation/provenance, exact diagnostic provenance, and final-snapshot exception-to-fault handling; P1-A current-schema full exit is **CLOSED**. Its P1-C execution rows remain truthfully `NotAdmitted` with zero pending, so this does not supply a `Succeeded` `RunFinalization` receipt, prove admitted execution quiescence, unblock P1-C, or satisfy this execution barrier.
- P1-B predecessor boundary: three distinct accepted immutable cutoffs verify the direct Corridor entry-presentation chain, one-port/39-binding residue cleanup, and Corridor 4/4 plus Station 0/0 anchor/profile stage-context hygiene at 80/80 with frozen route/policy digests unchanged. `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-04` remains the historical 19-source/84-test source-contract failure because its submitted prefab lacks the authored hidden reward row and blank public selection retains its old bundle/latch. The separate unchanged-source `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05` remediation passes its 19/19 manifest, authored hidden reward-row binding, four-row invalid-selection zero-side-effect matrix, focused 8/8, canonical UI 21/21, exact full route 1/1, aggregate 86/86, and validator checks, so `ACC-P1B-CANONICAL-SELECTION` is **VERIFIED PARTIAL** for that cutoff. This accepts only the canonical catalog-to-route selection projection: it adds no approved execution pocket, concrete count-one Station `Add` payload/anchor, resolver, activation receipt, or admitted P1-C execution truth. P1-C remains blocked on the rest of P1-B and its own exact authoring fixture.
- P1-B truthful-join proposal audit: `C:\tmp\DimensionBrawl-P1-B-TruthfulJoins-Contract-Proposal.md` matches SHA-256 `e5305d04937991e7120bb5edc8cd61905c4df923c689adc923c3df65fca9fe5d`. `P1B-TRUTHFUL-JOINS-01` is **AMEND / PROPOSAL ONLY**, not an accepted cutoff or freeze: its two-segment/three-pocket topology is usable after the exact amendments below, but its reference list has 24 rather than the required 27 rows and neither the template nor briefing supplies an exhaustive ordered row set plus final digest. It authorizes no implementation and closes no P1-B or P1-C gate.
- P1-B truthful-join rev2A update: preserve the first proposal and historical 71/27/78 rev2 as separate **AMEND** records. Rev2A freezes the same two segments and three pockets at template/reference/briefing row counts `71/27/80` with digests `3eec8a5f94c4dfd47ae9255a49ff3b5961d5130cf386f2c6ba96b0525c502e55` / `b93e1e23845983c3abdb2e13f551e66025942e40ddfde1a2b123054a65db0791` / `71b17e4c39364da14aa1deb0906b87eb88ed44e1242723a3b5b76064f2a89f60`, including `briefing.activeRunRestartPolicyDisposition=3` plus an empty independent digest. Its historical verdict remains **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**.
- P1-B truthful-join implementation cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1B-TruthfulJoins-Implementation-Bundle.md` matches SHA-256 `8ef3a8e234f53ef561dfdd5d805d0f69c8ddbb55d2a2534ca427f2da821a9d0a`; all 51 ordered source hashes match manifest digest `1d2fc6a142fa7582e76095c8a928ca1f61f4453ac7061f5d50525673d1480324`, and all 13 declared artifacts match. PowerShell and Node independently reconstruct `71/27/80`; the validator passes `8/4/1/1/0`; focused 7/7, canonical UI 26/26, exact full route 1/1, and graphics aggregate 91/91 pass with 91 unique full names and class counts `26/21/3/2/16/23`. Frozen route, policy, projection, template, reference, and briefing digests all match. `ACC-P1B-TRUTHFUL-JOINS` is **PASS / VERIFIED PARTIAL**; P1-B full exit remains **OPEN**. At its later historical cutoff, Candidate-06 fails `ACC-P1B-RESULT-PROGRESSION-JOINS` on three blocker groups. Remediation2 Candidate-07 subsequently closes those groups but still fails one independent canonical-catalog identity anchor; a new sealed-source candidate is next, followed by Station Add, live PGR/HI3 foreign evidence, and the full-exit audit. This cutoff adds no P1-C execution owner, result/progression/reward join or owner, or pre-result active-run restart.
- This document does not authorize P1-C production code, scene, prefab, or asset changes; rev2A authorizes only the bounded P1-B truthful-join implementation.

P1-C remains after P0, P1-0, P1-A, and P1-B. Documentation can be reviewed now, but implementation cannot begin merely because this contract exists.

## P1-B Truthful-Join Proposal Audit Boundary

The joint-freeze direction is accepted only after amendment. Keep contract ID `P1B-TRUTHFUL-JOINS-01`, `referenceSchemaVersion=1`, `referenceRevision=1`, `templateSchemaVersion=1`, `templateRevision=1`, and add explicit `briefingSchemaVersion=1` plus `briefingRevision=1`. Preserve the frozen route, terminal-policy, catalog contract, and catalog-projection digests byte-identically. Do not reuse `S1-1` through `S1-5` for the current Olympus route.

The revision-2 proposal must use these stable IDs:

| Surface | Exact ID | Audit disposition |
|---|---|---|
| truthful current-route template | `olympus-invasion.tutorial-station-run` | amended from the time-relative `olympus-invasion.current-route` |
| Corridor template segment | `olympus-invasion.corridor-tutorial` | approved |
| Station template segment | `olympus-invasion.station-guide-combat` | approved |
| Corridor pocket | `olympus-invasion.corridor.core-tutorial` | amended so the pocket ID does not collide with its source semantic ID |
| Station guide pocket | `olympus-invasion.station.replica-summon-guide` | amended to the stable product namespace |
| Station encounter pocket | `olympus-invasion.station.boss-encounter` | amended because `boss-terminal` would falsely describe only one terminal outcome |

The Corridor pocket may retain its distinct source provenance `sourceId=olympus.corridor.core-tutorial`, `sourceRevision=1`, and `sourceSemanticDigest=b1b00dd84e27fe8d06c6736d85b16ff6bfe141b7ccb70b01ea851144dd8182f2`; those provenance values are not the pocket identity.

The exact Korean revision-1 copy proposed for the joint freeze is:

```text
title=기억의 회랑
objective=하층 세계에서 발생한 차원의 미세한 균열.
그 징후의 진원지를 조사하라.
combatLesson=회랑에서 근접 공격, 이동, 원거리 전환과 사격, 회피, 표적 정리를 차례로 익힌다. 정거장에서는 레플리카 지급과 소환 안내를 확인한 뒤 보스 격파를 목표로 한다.
```

The objective separator is one actual `U+000A` LF, not the two literal characters `\` and `n`. Title and objective become approved authored narrative briefing only at the later joint freeze; they are not inferred runtime facts. The combat lesson states only the reviewed Corridor plan, Station entry-guide boundary, and boss objective and makes no power, reward, mastery, difficulty, or enemy-role claim.

The digest contract is not yet freezeable. Revision 2 must make these corrections:

- expand the reference digest from 24 to 27 ordered rows by adding `reference.storyEntryExpectedPortId=intro-gatepod-port`, `reference.storyEntryStageAnchorId=IntroCutscene_End_PlayerHandoffAnchor`, and `reference.storyEntryStageRuntimeStateId=state-intro-handoff`;
- enumerate the complete ordered template row set and exact values, including schema/ID/revision/kind, all present and typed-absence briefing arms and empty/zero payloads, two segment rows, three pocket rows in order, each source-provenance row, the two execution-disposition fields, and `enemyRoleCount=0`;
- represent `currentExecutionOwnerDisposition=ExistingSceneOwner` and `p1cAdmissionDisposition=NotAdmitted` as independent canonical fields, never as one combined display string;
- enumerate the complete ordered briefing row set and exact values, including `briefingRevision`, provenance, present copy, every typed absence and payload, ordered segments/pockets, story cue, and all three outcome-policy actions; sort actions by action ID using Ordinal comparison and cover each action's ID, kind, target, and outcome;
- calculate in the dependency order template digest, then reference digest, then briefing digest; `canonicalBriefingDigest` is an output and must not hash itself;
- use the existing canonical form: UTF-8 SHA-256 over final-LF `key=valueLength:value\n` rows with lowercase 64-hex output, `valueLength` equal to C# `String.Length` UTF-16 code units, null mapped to empty, bool mapped to `1|0`, explicit enum ordinals, invariant integers, and no trimming or Unicode normalization;
- publish the final `canonicalTemplateDigest`, `canonicalReferenceDigest`, and `canonicalBriefingDigest` values. Until all rows reconstruct those hashes independently, no joint freeze exists.

The frozen revision-1 `canonicalProjectionDigest` may remain unchanged only if the selection/Start latch binds the exact projection instance, catalog generation, and projection/reference/template/briefing digests and fails closed on any stale or mismatched join.

### Two segments, three pockets, and the Station Add boundary

Two route segments and three pockets are sufficient to describe the current truthful product topology. Do not invent a speculative fourth `Add` pocket. All three current pockets are truthful existing-scene mappings only: each must state `currentExecutionOwnerDisposition=ExistingSceneOwner` and `p1cAdmissionDisposition=NotAdmitted`, and none is an approved `EncounterExecutionBinding`.

`olympus-invasion.station.boss-encounter` is only the stable candidate pocket for a later Station count-one `Add` join. Closing that authoring gate still requires, in one separate accepted slice:

- an exact `spawnId` and non-placeholder `payloadTargetId`;
- `spawnKind=Add`, `count=1`, and its finite delay;
- exact static/live anchor identity, `anchorGroupId`, binding-root-local position and rotation, `UsageKind=CombatSpawn`, and exact position ID;
- explicit proof that the Add owner does not collide with boss, result, cutscene, guide, or terminal-resolution ownership;
- the P1-C binding, resolver, lifecycle owner, activation receipt, cancel/fault cleanup, and exactly-once completion evidence required by this specification.

The current three-pocket proposal therefore suffices for truthful template topology but not for Station Add readiness or P1-C admission. When a product P1-C encounter binding is later admitted, encounter membership changes; that product revision must bump and revalidate the route revision and canonical route digest rather than hiding the change only in a side digest.

## Bounded Scope

Interpret one P1-B-approved pocket as one scene-local immutable execution plan, execute its existing `StageDefinitionProfile.SpawnRef` records in deterministic order, cancel and clean them through the P1-A run lifecycle, and hand completion to the next local group exactly once.

P1-C does not own stage outcome, result commit, mastery, progression, rewards, retry navigation, story flow, or a general condition language.

## Decision Summary

| Decision | Recommended P1-C rule | Readiness |
|---|---|---|
| stage-local join | one `EncounterExecutionBinding` on the frozen canonical `PlayableStageDefinition` joins scene segment + linear segment + pocket to one sequence profile | proposed; the P1-B content join/binding asset does not exist yet |
| concrete spawn authority | `StageDefinitionProfile.SpawnRef` remains the only owner of payload ID, anchor ID, spawn kind, count, and delay | supported by current code |
| snapshot lifetime | P1-A run admission deep-copies the P1-C static plan/digest; Station binds only live scene handles | proposed new-schema extension; prevents mid-run asset reinterpretation |
| first activation | the sole `PlayableStageEncounterAdapter` sends one fully identified activation command; later groups use only `PreviousGroupCompleted` | proposed; avoids unowned triggers and a trigger DSL |
| first supported spawn kind | `Add` only | proposed; excludes player, cutscene-owned boss, backdrop, and objective ambiguity |
| first timing policy | injected scaled gameplay clock; each `SpawnRef.delaySeconds` is absolute from group activation; equal due times preserve serialized order; cancel/fault commands drain first | proposed; current `SpawnRef` has no interval |
| first count policy | `count` creates deterministic unit tickets at one due time, but the first fixture must use `count == 1` until multi-instance placement is authored and tested | proposed narrow gate |
| completion | all required tickets spawned successfully and emitted current-generation `CombatHealth.Died`; final sequence completion satisfies one named local gate | proposed; disappearance/failure/cancel never count as completion |
| failure | unresolved or failed required spawn faults, cancels pending work, cleans owned objects, and requests a diagnostic run abort from the route owner | proposed; never auto-clear |
| legacy Story PVE | classify it as noncanonical; P1-A admission reserves canonical scene ownership, while PVE may lease only standalone legacy scenes with no matching plan | recommended deterministic isolation decision |
| next handoff | one compare-and-seal latch advances only the next group in the same sequence | proposed; not campaign progression or Next-stage navigation |

## Current Local Implementation Audit

### Existing authoring authorities

| Surface | What exists | P1-C consequence |
|---|---|---|
| `LinearStageTemplateProfile` | ordered segment references and stage intent; its boundary explicitly excludes runtime wave spawning | keep as reusable intent, not a spawn owner |
| `LinearStageSegmentProfile` | segment identity, pacing, lesson, and `LinearStagePocket[]`; its boundary says there is no runtime spawning contract | add a stage-local binding rather than embedding scene-specific spawn IDs into the reusable segment asset |
| `LinearStagePocket` | pocket identity, objective, pacing, summon need, and enemy-role intent | validate the intended pocket, but do not infer a concrete payload or location from role prose |
| `StageDefinitionProfile.SpawnRef` | `spawnId`, kind, position ID, anchor ID, payload ID, count, and delay | retain as the concrete spawn authority; do not serialize those fields again in an encounter group |
| `StageDefinitionProfile.AnchorRef` | stable anchor identity and expected scene pose | validate the static reference, then resolve the live scene object through the scene binding |
| `StageDefinitionSceneBinding` | scene-local lookup of `StageAnchorPoint` by anchor ID | reuse for live anchor resolution; add duplicate detection because current lookup returns the first match |

A focused repository usage audit found no runtime consumer that joins `LinearStagePocket` to `StageDefinitionProfile.SpawnRef`. Current tests validate authored intent and ordering, not execution.

### Current Olympus route is not a fixture

`DB_Stage_OlympusCorridorIntroCombat` has `player-start`, `boss-center`, `add-left`, `add-right`, and `rift-backdrop`, but it is not a valid first P1-C fixture:

- `add-left` and `add-right` explicitly describe their payload identities as placeholders;
- `boss-center` is owned by the boss-entrance cutscene;
- player and rift records are not completion-required hostile spawns;
- P1-0 has corrected the profile's route scope, but no truthful canonical template/pocket execution binding selects these records;
- the Station-specific definition and scene binding now exist, but there is no P1-C binding/sequence/group or verified non-placeholder count-one Add SpawnRef plus unique live combat-spawn anchor.

The existing `S1.Segment.EntryRead / entry_probe_teach` pocket is a useful isolated validator candidate because it is small. It is not a truthful binding for the current canonical two-scene product route unless P1-B authors a truthful template/segment/pocket join and P1-C supplies a real resolvable payload, spawn binding, and lifetime owner.

### Existing Story PVE prototype

The repository already contains a separate encounter implementation:

- `PveStageData` owns another stage ID and raw `PveEncounterGroup` lists;
- a group owns `triggerZ`, clear gating, group delay, and raw enemy/structure/emitter placements;
- placements duplicate lane, depth, offset, payload, delay, and other runtime values instead of referencing `StageDefinitionProfile.SpawnRef`;
- `BattleManager.EnsureStoryPveRuntimeBootstrap()` creates a runtime prototype stage and `PveEncounterDirector` for `StoryPve`;
- `PveEncounterDirector` sorts groups by `triggerZ`, starts delayed coroutines, tracks spawned objects and pending enemy attempts, polls clear state, and advances its local index.

The prototype contains useful concepts: a per-group runtime ledger, duplicate-start/clear guards, and a pending-spawn barrier. It is not a production P1-C owner:

- its sort order is computed from `triggerZ`, not preserved serialized sequence order;
- it has no run ID, generation token, coroutine-handle ledger, cancellation state, or explicit retry/scene-exit drain;
- the inspected class has no `OnDisable`, `OnDestroy`, `Cancel`, or cleanup path that destroys every owned runtime object;
- a failed enemy spawn returns null, but the delayed attempt still decrements the pending count;
- `PveRuntimeBootstrapPlayModeTests.DelayedEnemyPlacementKeepsEncounterOpenUntilDelayElapses` deliberately accepts that the resulting empty group clears after the failed attempt.

That empty-after-failure behavior is a negative test source for P1-C. A required spawn that never existed cannot satisfy completion.

### Prototype migration decision

Before P1-C schema freeze, select exactly one canonical production owner:

1. Keep `PveStageData`, `PveStageContext`, and `PveEncounterDirector` outside the canonical `PlayableStageDefinition` route.
2. Make canonical and PVE startup atomically contend for the same loaded-scene execution lease; a validator or scan-then-start check is insufficient.
3. Reuse only independently testable lifecycle primitives, such as a pending-ticket ledger or completion latch. Do not adapt the raw PVE placement model into the canonical schema.
4. Do not use `PveStageContext.SelectedStage` as canonical playable-stage, route-revision, or run identity.
5. Revisit a broader migration only after the first P1-C fixture proves cancellation, cleanup, failure, and exactly-once advancement.

This recommendation avoids a third encounter model while also avoiding a risky rewrite of the working prototype during demo stabilization.

## Cross-Game Evidence and Boundary

The archive supports a narrow authoring hierarchy. It does not prove peer runtime cancellation or cleanup behavior.

| Source | Strong field-level evidence | What it does not prove |
|---|---|---|
| Arknights | `stage_table.levelId -> level waves[].fragments[].actions[]`; action fields include type, target key, count, pre-delay, interval, and route index | scheduler implementation, completion, cleanup, or transferable tower-defense rules |
| Girls' Frontline 2 | 2,952 stage rows with ordered enemy-group references -> 8,889 group rows with ordered enemy references -> 29,620 placement rows | actual execution order, trigger semantics, cleanup, or a reason to import opaque events/triggers |
| Last Origin | 761 stages with ordered wave-group slots and boss group -> 6,647 mob groups with ordered member slots | spawn timing, anchor resolution, runtime cleanup, or reward execution |
| Zenless Zone Zero | floor -> ordered group -> member placement with position/rotation fields | official client provenance or runtime behavior; it is supporting public-code-candidate data |
| Aether Gazer | stage-like record -> ordered wave-list IDs -> wave/map records | direct member spawn lists, placement, or execution; the source-linked notes explicitly report that gap |

PGR and HI3 remain important for stage/course/challenge/result structure, but the selected archive paths do not directly join ordered encounter groups to member placement and runtime cleanup. PGR's heuristic `wave-spawn-runtime` candidate label and HI3's stage/Lua/monster metadata are not promoted to P1-C execution evidence.

Therefore:

- use peer data to justify `stage -> segment/pocket -> ordered group -> ordered spawn reference` authoring and validation;
- derive cancellation, cleanup, stale-callback exclusion, spawn-failure policy, and exactly-once advancement from local lifecycle requirements;
- prove those lifecycle claims with local tests, not by inference from static peer configuration.

## Authority and Ownership

| Concern | Canonical owner | P1-C use |
|---|---|---|
| playable-stage ID, route revision, ordered scene segments | P1-0 `PlayableStageDefinition` route shell | validate and snapshot; never mint a parallel stage identity |
| run ID, route/encounter static-plan snapshot, required-gate state, run lifecycle, outcome, terminal arbitration, result commit | P1-A `StageRunContext` and result owners | P1-C registers immutable plan identity, emits one gate-satisfied command, and obeys disposal/abort; it never publishes outcome or result |
| reusable lesson, segment, and pocket intent | `LinearStageTemplateProfile` / segment / pocket | validate the stage-local binding |
| scene map, concrete spawn records, static anchor references | `StageDefinitionProfile` | resolve by stable IDs into the immutable execution snapshot |
| live anchor objects | one `StageDefinitionSceneBinding` for the referenced scene definition | resolve before activation and release on scene exit |
| payload-to-prefab/factory mapping | narrow typed `IStageSpawnPayloadResolver` or equivalent registry | validate and create an owned runtime handle |
| sequence/group order and local handoff | P1-C sequence profile and runtime instance | execute and advance locally exactly once |
| final local phase gate | stage-local binding plus one scene-segment encounter-gate adapter; P1-A snapshot marks it required for Clear | accept one sequence-complete command; never treat it as Clear/result proof |
| loaded-scene encounter ownership | one atomic lease registry used by canonical and legacy PVE startup | prevent simultaneous executors regardless of startup order |
| mastery/progression | P1-D | its evaluator consumes the sealed P1-A fact candidate before result commit and its progress writer consumes the committed summary; P1-C supplies no automatic mastery proof and performs no mutation |
| reward plan, payout, receipt | P2-C | no P1-C fields or calls |
| Replay/Retry/Lobby dispatch | P1-A typed action executor | P1-C only cancels and drains before disposal/navigation |

## Proposed Authoring Contract

Names in this section are review names, not existing production types.

### Stage-local binding

Add one stage-local join to the final P1-B `PlayableStageDefinition` rather than adding scene-specific spawn IDs to a reusable `LinearStageSegmentProfile`:

```text
EncounterExecutionBinding
  bindingHostScope
  sceneSegmentId
  linearStageSegmentId
  pocketId
  encounterGroupSequenceProfile (direct asset reference)
  executionPurpose (RequiredDefeatRouteSequence in canonical revision 1; later NonTerminalPracticeActivity)
  completionConsumer (RequiredLocalEncounterGate(gateId) or later NoDefeatCompletionConsumer)
```

`EncounterBindingHostScope` is the authoring union: first-slice `ProductRouteScope(playableStageId, routeRevision)`, later `ProductTutorialCourseScope(playableStageId, routeRevision, courseId, courseRevision, courseEntryId)`, `IsolatedValidationFixtureScope(fixtureId, fixtureRevision)`, or `IsolatedTutorialCourseFixtureScope(fixtureId, fixtureRevision, courseId, courseRevision, courseEntryId)`. The composite binding scope plus `(sceneSegmentId, linearStageSegmentId, pocketId)` must resolve exactly one binding. Zero is allowed for pockets that are intentionally presentation-only or still unimplemented; more than one is invalid. A product spine may resolve only a product arm. An isolated P1-C3 or course test is diagnostic-only and must agree on fixture ID/revision with its P2-A/course snapshots. A canonical `RequiredDefeatRouteSequence` must name one required local encounter gate. The later `NonTerminalPracticeActivity` arm must use `NoDefeatCompletionConsumer`, cannot satisfy a required-clear gate, and remains Active until the P2-B coordinator requests its typed cancellation. The binding scope, purpose, completion-consumer arm, and gate identity all participate in the canonical encounter digest. Revision 1 does not permit an implicitly parallel or ornamental production binding.

### Sequence profile

`EncounterGroupSequenceProfile` should contain only:

| Field | Rule |
|---|---|
| `schemaVersion` | required positive integer; unknown versions fail closed |
| `sequenceId` | stable and unique within the playable-stage route revision |
| `revision` | positive content revision independent from, but validated against, route revision |
| target IDs | playable stage, route revision, scene segment, stage definition, linear segment, and pocket IDs used as validation facts |
| `groups[]` | nonempty serialized order; that order is authoritative |
| derived `contentDigest` | generated canonical digest over every execution-affecting field and resolved spawn fact; not a hand-authored semantic input |

The profile uses a direct `StageDefinitionProfile` reference through the canonical scene segment. Its target IDs detect drift; they do not create duplicate authorities.

### Group record

The first `EncounterGroupRef` contains:

| Field | Revision-1 rule |
|---|---|
| `groupId` | stable, nonempty, unique within the sequence |
| serialized index | authoritative group order; no runtime sorting |
| `activationKind` | first group `ExplicitPocketActivation`; later group `PreviousGroupCompleted` |
| `spawnRefIds[]` | nonempty ordered IDs scoped to the sequence's one stage definition |
| `completionKind` | `AllRequiredSpawnInstancesTerminal` only |

The canonical spawn key is `(stageDefinitionId, spawnId)`. Because a revision-1 sequence is bound to one stage definition, the group serializes only `spawnId`; the snapshot stores the full scoped key.

The group must not copy payload IDs, anchors, transforms, position IDs, counts, delays, enemy stats, role weights, rewards, result rules, or navigation.

## Immutable Execution Snapshots

P1-C requires two deliberately separate snapshots.

### Run-admission static plan

For a new P1-C-capable schema, logical stage admission extends P1-A's `StageRunRouteSnapshot` with one `EncounterStaticPlanSnapshot` per bound production sequence. This happens at the same Corridor run admission as the route snapshot, not later at Station scene entry. It deep-copies:

- P1-A run ID, approved playable-stage ID, route revision, and `coreRouteSemanticDigest`;
- exact `EncounterBindingHostScope` arm and, when present, course/entry identity;
- scene-segment, stage-definition, linear-segment, pocket, sequence, sequence-revision, group, execution-purpose, completion-consumer, and optional completion-gate IDs;
- authoritative serialized group and spawn-reference order;
- each referenced `SpawnRef` kind, position ID, payload ID, anchor ID, count, and delay;
- the matching static `AnchorRef` ID, group ID, expected binding-root-local position/rotation, and fixed position/rotation tolerances;
- stable typed payload-mapping ID, revision, compatible kind, target archetype/prefab identity, and mapping digest;
- completion policy and derived canonical encounter content digest.

Digest composition is deliberately layered and acyclic. P1-A first computes `coreRouteSemanticDigest` over only P1-0/P1-B route semantics, excluding all P1-C encounter plans, P2-A variability, P2-B course content, and the final route digest. Each `EncounterStaticPlanSnapshot` binds that core digest plus its own P1-C semantics and stable host IDs/revisions; it contains no P2-A/P2-B semantic digest and derives its encounter content digest independently. The fixed-order encounter digests then feed P2-A, P2-B, and finally the route digest in that direction only. Runtime execution may carry the final route digest beside the plan as envelope provenance, but that value is not an input to the plan or encounter digest.

This is a route-snapshot schema extension for newly admitted runs only. Existing P1-A snapshots without encounter-plan identity cannot be reinterpreted as P1-C-capable runs.

### Scene-entry live binding

On scene readiness for a first-slice route binding, or after one current `CourseEntrySelection` seals for a course-scoped binding, `EncounterSceneBindingSnapshot` verifies the current assets and mapping against the run-admission IDs, revisions, and digests, then binds only:

- one newly minted P1-C `executionInstanceId` and monotonically issued `executionGeneration`;
- for a course-scoped binding, the exact course ID/session ID/course generation, course-entry ID/entry generation, and `courseEntrySelectionId`/canonical selection digest that authorized this instance;
- the loaded scene-instance ownership lease;
- live `StageAnchorPoint` handles whose anchor, group, usage, position, spawn-kind, and pose fields match the static plan;
- validated scene-local payload resolver/factory descriptors that match the snapshotted mapping identity; each later creation still requires its own transactional `SpawnLease`.

Neither snapshot stores a cross-scene `Transform`, `GameObject`, coroutine, or mutable asset reference. A Station-time digest mismatch faults instead of loading newer values. After `Ready`, execution does not re-read a changed profile, registry, mapping, or route asset.

Pose comparison and use are fixed for revision 1:

- authoring position/rotation are relative to the `StageDefinitionSceneBinding` component's transform, not `MapRoot` and not world origin;
- scene-ready validation computes binding-root-local position with `InverseTransformPoint` and local rotation with `Quaternion.Inverse(bindingRotation) * anchorRotation`;
- position uses `Vector3.Distance <= positionToleranceMeters`; rotation uses `Quaternion.Angle <= rotationToleranceDegrees` against `Quaternion.Euler(expectedEuler)`;
- both tolerance values are frozen in P1-C0 and included in the encounter digest;
- after validation, the scene snapshot captures one immutable world position/rotation for each referenced spawn at scene readiness;
- group activation and delayed due work use only that captured world pose. Later anchor or root movement cannot retarget an already admitted run.

`executionGeneration` is owned only by the P1-C scene-local executor. It is not a P1-A run or P2-B course generation: Replay/Retry creates a new P1-A `runId`, each admitted P1-C instance receives a new local generation, and cancel/fault atomically invalidates that generation before any drain or cleanup so stale callbacks can be rejected. For Practice or Challenge, P1-C may mint the instance/generation only after validating the current sealed `CourseEntrySelection`; a callback, presentation completion, P2-A receipt, or stale prior-entry selection cannot mint or reactivate one.

Asset edits therefore apply only to a newly admitted run with a new route/encounter snapshot. They cannot alter an active, cancelling, completed, or committed run.

### Later P2-A variant agreement

P2-A may extend a new-schema admission plan only through one spine-reachable, versioned `StageEnemyVariantBindingSet` that binds an existing P1-C scoped spawn key `(stageDefinitionId, spawnId)` to a snapshotted enemy-variant identity/digest. The P2-A set/binding copies no group/order, payload, prefab, anchor, transform, count, delay, clock, completion, or lifetime field. P1-C validates that its existing payload mapping is the sole gameplay-prefab authority and agrees with the frozen archetype/candidate before object creation; P2-A does not become a second resolver, factory, spawner, death observer, or cleanup owner.

For an admitted variant binding, P1-C exposes one typed inactive-stage configuration seam inside its existing factory transaction. P2-A may configure only the frozen reviewed role/pattern/deck/elite/closed-override ports and must return a ticket/generation/snapshot-matching `EnemyVariantConfigurationReceipt`. P1-C alone decides whether the receipt arms the ticket and crosses activation. Missing, stale, failed, or mismatched configuration faults the factory while the staging root remains owned and inactive.

P1-C's execution generation and `EncounterExecutionQuiescenceBarrier` remain independent from P2-A's variability generation and `StageVariabilityQuiescenceBarrier`. P1-A awaits both; neither barrier may report the other's work as drained or release the other's ownership.

### Later P2-B Practice/Challenge agreement

The P1-C revision-1 product binding remains one canonical required-defeat route sequence. It cannot be relabelled as no-proof Free Practice. A later P2-B course-capable schema may add exactly one bounded `NonTerminalPracticeActivity` binding and one Challenge binding only after their binding host scope, purpose/sequence/gate identities, payload mapping, P2-A set/binding agreement, and course snapshot are reviewed together.

- Each entry receives a distinct P1-C execution instance/generation under the same P1-A run.
- Practice has no defeat-success, stage-outcome, mastery, progress, or reward meaning. The P2-B course coordinator seals `PracticeExitSelection` and requests typed P1-C cancellation/disposal; P1-A does not duplicate this ordinary mid-run transition fan-out.
- The Practice arm has `NoDefeatCompletionConsumer`: group/ticket terminal events remain execution diagnostics and never open a required-clear gate or close the activity. Its successful transition evidence is the later cancellation/disposal receipt after `PracticeExitSelection`, not sequence defeat.
- P1-C reports only its independent quiescence receipt. It does not select or activate Challenge.
- Any P2-A pre-activation configuration transaction must already be closed before a Practice object activates. An ordinary Practice-to-Challenge transition does not seal the run-level `StageVariabilityQuiescenceBarrier`; only a separately reviewed entry-scoped P2-A lease would add its own transition receipt.
- Challenge remains Locked until Practice pending handles, owned full/partial objects, subscriptions, and scene lease are zero/released and every other required transition barrier succeeds.
- Practice and Challenge may never hold the scene execution lease concurrently.
- Challenge's required gate remains a lifecycle/precondition input, never Clear or mastery.

An isolated Boss Barrage contract fixture may prove this boundary without becoming a product binding. Product admission fails if it references an isolated P2-A scope or silently reuses the legacy review owner.

### Activation authority and envelope

One `PlayableStageEncounterAdapter` is the sole activation producer. It consumes the active P1-A context, the P1-B stage-local binding, a successfully acquired scene ownership lease, and scene-ready validation, then enqueues:

```text
EncounterPocketActivationCommand
  runId
  routeRevision
  routeSnapshotDigest
  bindingOrdinal
  bindingId / bindingRevision / canonicalEncounterDigest
  bindingHostScope (exact EncounterBindingHostScope)
  executionHostScope = NonCourse(all course fields typed absent)
                     | Course(courseId, courseSessionId, courseGeneration,
                              courseEntryId, entryGeneration,
                              courseEntrySelectionId, courseEntrySelectionDigest)
  sceneSegmentId
  stageDefinitionId
  linearStageSegmentId
  pocketId
  sequenceId
  sequenceRevision
  encounterContentDigest
  executionInstanceId
  executionGeneration
  activationCommandDigest
  envelopeChecksum
```

`EncounterExecutionHostScope` is the runtime union shown above. The command's immutable `bindingOrdinal` is the binding's fixed spine-order index in the admitted P1-A snapshot, and its binding ID/revision/digest plus `EncounterBindingHostScope` must byte-match that row. Runtime `Course` is required if and only if the binding arm is `ProductTutorialCourseScope` or `IsolatedTutorialCourseFixtureScope`; its course ID and entry ID must match that arm and its session/entry generations plus current selection must match the P2-B snapshot. Runtime `NonCourse` is required for `ProductRouteScope` or `IsolatedValidationFixtureScope` and forbids every foreign course field. Thus product and isolated bindings can never collapse to the same runtime scope. `activationCommandDigest` covers the run/route, binding ordinal/identity/revision/canonical digest, complete binding-host and execution-host arms including typed absences, scene/pocket/sequence/content, and execution identity; it excludes the envelope checksum. The executor classifies the envelope before any state mutation:

- a foreign/stale `runId`, execution-instance ID, execution generation, or course/session/entry selection identity/generation is reject/log-only and cannot fault the current run;
- the identical command repeated after its successful admission is a diagnostic no-op;
- a command naming the current execution identity but carrying a mismatched route/digest/course-selection/segment/pocket/sequence fact, or arriving from an unauthorized producer, is an invalid-evidence fault before spawning.

Only the validated current command may enter through one `Ready -> Active` compare-and-set. No callback may activate a sequence or mint an execution generation directly.

### Typed close command

Every non-fallback close enters through one immutable `EncounterExecutionCloseCommand`:

- run/route and `executionIdentity = Issued(executionInstanceId, executionGeneration) | NotIssuedBeforeClose`;
- exact binding/static-plan identity or typed `NoBindingForCurrentPhase`, plus the relevant per-binding `executionAdmissionOrCloseLatch` identity/state;
- close reason;
- exact close authority arm: sealed P1-A `StageRunAbortCloseAuthority` ID/digest, sealed `TerminalFinalizationAuthority` ID/digest, sealed `ResolvedTerminalActionSelection` ID/digest, sealed `ResolvedActiveRunRestartDispatch` ID/digest, or P2-B `PracticeExitSelection` ID/digest;
- for a course-scoped binding, exact `courseCloseContext = BeforeFirstSelection | CurrentEntrySelection | BetweenCourseEntries`, carrying course/session generations plus respectively typed no selection, the authorizing `CourseEntrySelection` ID/digest/generations, or the prior transition and successor reservation-state provenance;
- issued sequence, canonical `executionCloseCommandDigest`, and envelope checksum.

`executionCloseCommandDigest` covers run/route/execution-identity arm, binding/latch provenance, close reason, exact authority arm/ID/digest, optional course-selection or between-entry provenance and typed absence, and issued sequence; it excludes the envelope checksum. The executor validates this command against its current execution or unopened binding before invalidating the generation or sealing the close side of the latch. The `PracticeExitSelection` arm is accepted only for the matching current `NonTerminalPracticeActivity`; it is the sole ordinary same-run transition authority. The `TerminalFinalizationAuthority` arm is accepted only after the terminal arm wins P1-A's shared latch and before `OutcomeFactsSealed`; it authorizes run-finalization cleanup but is not a committed outcome or result. A stale/foreign/mismatched close command is reject/log-only and cannot cancel Practice, unlock Challenge, or affect a later execution. Owner disable/destroy or scene unload may immediately invalidate the local generation and begin fail-safe drain, but it must report to P1-A and receive the matching `StageRunAbortCloseAuthority` before a successful quiescence receipt can seal. If that authority never arrives, P1-C returns fault evidence with `AuthorityUnavailable`; no anonymous local fallback record may satisfy the close-authority union or masquerade as `ProceedToChallenge`.

## Revision-1 Scheduling Rules

1. Revision 1 uses one injected monotonic **scaled gameplay clock**. It advances with gameplay time, pauses while `timeScale == 0`, and never uses wall-clock or frame count as elapsed delay.
2. A valid `EncounterPocketActivationCommand` captures the first group's activation time. Each later group captures a new activation time only after the previous group cleanup and completion latch.
3. For every referenced `SpawnRef`, due time is `groupActivationTime + SpawnRef.delaySeconds`.
4. Sort by due time, then preserve the group's serialized `spawnRefIds[]` order. Never sort by anchor, payload, spawn kind, or runtime position.
5. `SpawnRef.count` creates deterministic unit ordinals `0..count-1` at that same due time.
6. The first production fixture requires `Add` and `count == 1`. Multi-instance tickets remain defined but blocked until a non-overlapping placement policy and test fixture exist.
7. There is no group-level delay, per-unit interval, member/action override, random role selection, or trigger expression in revision 1.
8. Lifecycle, due-spawn, and terminal callbacks enqueue commands into one non-reentrant executor queue; they never transition state or start the next group inside the originating callback stack.
9. Each drain processes admitted cancellation/fault/disposal commands before due spawns, then spawn results, typed defeat events, group completion, and local advancement. Cancellation admitted before a same-tick due command is drained wins and produces no spawn; cancellation after a committed spawn cleans that owned handle.
10. Synchronous events raised during factory creation are queued until the factory transaction and ticket ledger are sealed.
11. A later interval requirement must arrive as an explicit schema revision with its own clock, ownership, and ordering tests. It cannot be inferred from peer data or silently layered over `delaySeconds`.

These rules intentionally narrow the roadmap's earlier `delay/interval` wording. The local source owns delay but no interval, and the current product slice does not prove a per-unit cadence requirement.

## Runtime Contracts

### Payload resolver and owned handle

A narrow payload resolver must validate `(stageDefinitionId, spawnKind, payloadId)` against the snapshotted mapping before activation and return a typed factory descriptor.

Creation is transactional and activation-gated:

1. before calling the prefab factory, the executor reserves a `SpawnLease`, creates one inactive per-ticket staging root, and records that root in the owned ledger;
2. the prefab is born under that already-owned inactive root through an instantiate-with-parent/two-phase factory path, never instantiated active and reparented afterward;
3. while the hierarchy remains inactive, the factory validates components and registers every nested/partial root;
4. for a snapshotted P2-A binding, it invokes the exact variant-configuration capability and validates one immutable `EnemyVariantConfigurationReceipt`; the adapter cannot instantiate, destroy, reparent, activate, replace the prefab, or take lifetime ownership;
5. the factory binds the typed defeat observer and seals cleanup handles only after required configuration succeeds;
6. only after the ticket is `Armed` does the executor mark it `Spawned` and cross one activation barrier; `Awake`/`OnEnable` and other synchronous callbacks are queued and cannot execute encounter transitions inside activation;
7. the factory returns either one complete success handle with any required configuration receipt or a failure record whose lease still owns every partial root. Configuration, receipt, component, or activation failure cleans the entire staging root before the fault is sealed.

If a prefab/factory cannot prove that externally observable initialization is held behind this barrier, it is not admissible for revision 1.

A successful owned runtime handle contains:

- deterministic ticket ID;
- exact `EncounterExecutionHostScope` inherited unchanged from the current execution context;
- created root/object reference;
- typed `CombatHealth.Died` observation subscription;
- cleanup/despawn operation;
- exact `configurationCoverage = NotRequiredBySnapshot | Succeeded(EnemyVariantConfigurationReceipt ID/canonical digest)`; the success arm is required when the entry snapshot contains a binding for this ticket's scoped spawn key;
- optional diagnostic identity, never result or reward authority.

### `SpawnLeaseTerminalReceipt`

Every reserved ticket seals exactly one terminal lease receipt:

- runtime-issued receipt ID;
- run/route, exact `EncounterExecutionHostScope` arm, execution instance/generation, deterministic ticket ID, scoped spawn key, and unit ordinal;
- materialization disposition: `CancelledBeforeMaterialization`, `FailedInactiveStagingRolledBack`, `SpawnedDefeatedAndReleased`, `SpawnedCancelledAndReleased`, or `SpawnedFaultedAndReleased`;
- stable owned-root/object handle IDs and their `NeverExposed | DestroyedOwned | DespawnedOwned` dispositions, with zero retained owned roots/objects;
- observer/subscription release disposition;
- exact `configurationResultCoverage = NotRequiredBySnapshot | Succeeded(EnemyVariantConfigurationReceipt ID/canonical digest) | FailedInactiveRollbackComplete(EnemyVariantConfigurationFailureReceipt ID/canonical digest)` matching the ticket snapshot/materialization disposition;
- terminal sequence, canonical `spawnLeaseTerminalDigest`, and envelope checksum.

`spawnLeaseTerminalDigest` covers the receipt/ticket/execution/course provenance, scoped spawn identity, materialization and every owned-handle/subscription disposition, configuration-result ref or typed absence, zero-retained facts, and terminal sequence. It excludes Unity object references, presentation metadata, and every envelope checksum. A lease cannot seal this receipt while any partial/full root or observer remains owned.

The resolver cannot fall back from an unknown payload to a default prefab. A missing prefab, missing required component, factory exception, null result, unregistered partial root, or duplicate live ticket is a spawn failure. A factory that can instantiate outside the lease contract is not admissible.

### Ticket identity and states

Ticket ID is deterministically derived from:

`runId + complete EncounterExecutionHostScope arm/typed absences + executionInstanceId + executionGeneration + sequenceId + sequenceRevision + groupId + spawnRefId + unitOrdinal`

One ticket moves through:

`Planned -> PendingDelay -> Staging -> Armed -> Spawned -> Defeated`

Alternative terminal paths are `Failed` and `Cancelled`. A separate idempotent cleanup marker records whether every owned handle/subscription was released. `Failed` and `Cancelled` never satisfy successful group completion.

For the first `Add` fixture, `Defeated` means exactly one typed observer accepted `CombatHealth.Died` after the ticket entered `Spawned` and while the same execution generation remained active. The factory validator requires one live, non-dead `CombatHealth` source. Disable, destroy, despawn, missing object, or owner loss before `Died` is an unexpected-disappearance fault. Events produced after cancellation/fault generation invalidation or by cleanup are ignored and cannot become success.

### Required-defeat completion boundary

`RequiredDefeatRouteSequence` completion requires all of the following:

- every planned required ticket reached `Spawned` successfully;
- pending-delay, staging, armed, and activation counts are zero;
- every owned required instance reached typed `Defeated` through its bound `CombatHealth.Died` observer;
- the P1-C execution generation is still current and the group/sequence is not cancelling, faulted, or disposed;
- the completion latch seals successfully once.

Player, backdrop, cutscene-owned boss, pre-existing scene objects, and unrelated scene enemies cannot enter the completion set. Global scene searches are forbidden.

An empty group is invalid for the first fixture. A later explicit relief/no-enemy group belongs to a different completion policy and schema review; it must not emerge accidentally from failed spawns. These conditions may close a group inside `NonTerminalPracticeActivity`, but they never complete that sequence, satisfy a gate, or replace `PracticeExitSelection` plus cancellation/quiescence.

### Sequence lifecycle

| State | Allowed work | Exit rule |
|---|---|---|
| `Created` | retain immutable IDs only | validation begins; cancel/fault -> terminal path |
| `Validating` | resolve every static reference and factory descriptor | all valid -> `Ready`; cancel/fault -> terminal path |
| `Ready` | bind scene-local anchors and await one activation | activation -> `Active`; cancel/fault -> terminal path |
| `Active` | schedule tickets, observe typed defeat events, seal groups | required-defeat final group -> `Completing`; Practice final group -> remain `Active` with diagnostic `PracticeContentExhausted`; cancel/failure -> terminal path |
| `Completing` | required-defeat only: perform the final cleanup barrier, deliver/acknowledge one required local-gate command, and emit one sequence proof | accepted gate -> `Completed`; cancel/rejection/failure -> terminal path |
| `Completed` | required-defeat only; no new work or advancement | cleanup/disposal only |
| `Cancelling` | invalidate generation, cancel pending work, unsubscribe, clean owned objects | `Cancelled` |
| `Faulting` | capture first fault, invalidate generation, cancel and clean | `Faulted` |
| `Cancelled` / `Faulted` | no activation, spawn, completion, or advance | disposal only |
| `Disposed` | no references or callbacks retained | terminal |

Repeated activation, cancellation, fault, completion callback, scene-disable, or disposal calls must be harmless and must not reopen a sealed state.

### Group lifecycle

Group state is separate from sequence state:

| Group state | Meaning | Exit rule |
|---|---|---|
| `Dormant` | group has never been activated | first command or previous-group completion -> `Active` |
| `Active` | tickets may be pending/spawned and typed defeat events may arrive | successful barrier -> `Completing`; sequence terminal path -> matching group terminal path |
| `Completing` | one latch winner detaches observers and cleans terminal owned handles | cleanup barrier -> `Completed`; cleanup failure -> sequence `Faulting` |
| `Completed` | one group proof is sealed; no group work remains | sequence remains `Active` and may activate the next group |
| `Cancelling` / `Faulting` | generation is invalid and group work is draining | sequence terminal path owns final seal |

Only completion of the final serialized group in `RequiredDefeatRouteSequence` may transition the **sequence** from `Active -> Completing -> Completed`. An intermediate group becomes `Completed` while the sequence remains `Active`. In `NonTerminalPracticeActivity`, every group including the final group may become `Completed`, but the sequence remains `Active` until the P2-B coordinator's already sealed `PracticeExitSelection` requests cancellation/disposal. Revision 1 performs no automatic Practice respawn or in-place reset.

### Exactly-once local advancement

One group-completion latch is keyed by `runId`, `executionInstanceId`, `executionGeneration`, and group index. Only the caller that changes the **group** from `Active -> Completing -> Completed` may:

1. detach the completed group's observers and clean/despawn its terminal owned handles;
2. pass a cleanup barrier with pending handles and owned live objects at zero;
3. emit one `EncounterGroupCompleted` execution proof;
4. increment the local next-group index once;
5. while the sequence remains `Active`, activate the next serialized group; on the final group, only `RequiredDefeatRouteSequence` asks the sequence to enter `Completing`, while `NonTerminalPracticeActivity` records diagnostic content exhaustion and continues awaiting explicit close.

The group latch winner first enters `Completing`; it seals the group `Completed` only after the cleanup barrier. A later need to preserve corpses or effects must transfer them to a separately reviewed presentation owner. Revision 1 does not advance while group-owned terminal objects remain live.

For `RequiredDefeatRouteSequence`, the final group then contends for a separate sequence-completion latch keyed by the same execution identity. Its sole winner changes the sequence `Active -> Completing`, delivers exactly one gate command, waits for the current-run/current-execution-generation gate acknowledgement, emits one sequence proof, and seals `Completed`. Duplicate final callbacks cannot redeliver the gate or proof. `NonTerminalPracticeActivity` has no sequence-completion latch or gate command; its final-group diagnostic cannot close the course entry.

If cancellation/fault is admitted while the sequence is `Completing`, the execution generation is invalidated before a queued gate acknowledgement can apply. The stale acknowledgement is reject/log-only, the local phase does not open, and quiescence remains unsatisfied until the cancelling/faulting drain reaches `Disposed`.

Duplicate death events, object destruction callbacks, reentrant observers, and same-frame terminal events must lose the latch without side effects.

`EncounterGroupCompletionProof` is the exact schema behind the `EncounterGroupCompleted` event. It contains runtime-issued `groupCompletionProofId`; run/route and exact `EncounterExecutionHostScope`; binding/static-plan and execution instance/generation; sequence ID/revision/content digest; group ID/serialized index and group-completion-latch ID; canonical ticket-terminal-coverage digest with zero pending/live group-owned work; completed sequence; canonical `groupCompletionProofDigest`; and envelope checksum. Its digest covers those exact fields and excludes the checksum/presentation metadata. It is diagnostic execution evidence, not outcome or gate authority.

For `RequiredDefeatRouteSequence` only, the final group proof prepares one `EncounterGateSatisfiedCommand` containing runtime-issued `gateSatisfiedCommandId`; the exact final `EncounterGroupCompletionProof` ID/canonical digest; run/route, exact execution-host scope, binding/static-plan, execution instance/generation, and sequence ID/revision/content digest; required gate ID and sequence-completion-latch ID; issued sequence; canonical `gateSatisfiedCommandDigest`; and envelope checksum. The command digest covers those exact fields and excludes its checksum. The snapshotted `RequiredLocalEncounterGate` consumer accepts only this command. It may open the next existing local phase and is a required-clear precondition for the P1-A route snapshot; it cannot itself publish Clear/Fail, commit `RunResultSummary`, mutate mastery/progression/reward, or navigate. A missing/rejecting consumer faults instead of silently completing the canonical sequence.

After an `Opened` acknowledgement, the latch winner seals one `EncounterSequenceCompletionProof` containing runtime-issued `sequenceCompletionProofId`; exact command ID/digest; exact `EncounterGateAcknowledgementReceipt` ID/canonical digest; the same run/route/execution-host/binding/execution/sequence/gate/latch provenance; completed sequence; canonical `sequenceCompletionProofDigest`; and envelope checksum. Its digest covers those exact fields and excludes every envelope checksum. This proof is the schema behind `EncounterSequenceCompleted`. `OpenFailed` creates no sequence proof. `NonTerminalPracticeActivity` has no sequence-completion latch, gate command, acknowledgement, or sequence proof.

The required-defeat gate transaction has one serialized order:

1. validate current run ID, execution instance/generation, gate ID, and pending P1-A gate state;
2. compare-and-set the P1-A gate `Pending -> Satisfied`;
3. attempt to open the named existing local phase before releasing the non-reentrant command drain; any synchronous terminal callbacks from opening are queued until this transaction returns;
4. on success, first seal the immutable `EncounterGateAcknowledgementReceipt` with `Opened`, then emit/seal the one `EncounterSequenceCompletionProof` that references that receipt, and only then seal the sequence `Completed`; on failure, seal the acknowledgement receipt with `OpenFailed`, emit no sequence proof, and enter common abort closing without rolling the gate back.

The transaction seals one `EncounterGateAcknowledgementReceipt` containing runtime-issued receipt ID; exact `EncounterGateSatisfiedCommand` ID/canonical digest; the repeated run/route/execution-host/binding/execution/sequence/gate/latch provenance; prior `Pending` and final `Satisfied` state; `phaseOpenDisposition = Opened | OpenFailed`; acknowledgement/open sequence; canonical `gateAcknowledgementDigest`; and envelope checksum. The canonical digest covers those exact semantic fields and excludes presentation metadata and every envelope checksum. `Opened` permits the later `EncounterSequenceCompletionProof`; `OpenFailed` requires typed proof absence and enters abort closing. Required-defeat quiescence carries exact `gateCompletionCoverage = Opened(EncounterGateAcknowledgementReceipt ID/digest, EncounterSequenceCompletionProof ID/digest) | OpenFailed(EncounterGateAcknowledgementReceipt ID/digest, typed absence of sequence proof) | NotReachedBeforeClose(typed absence of command/receipt/proof)`. `NonTerminalPracticeActivity` instead uses `NotApplicableToPurpose` with every gate field absent.

A stale/foreign/duplicate gate command cannot change gate state or open the phase. If local phase opening fails after the gate CAS, the current run enters the common `AbortClosing` path before queued terminal work can commit, closes every admitted owner, and only then seals the one diagnostic abort with the resulting receipts/fault evidence; the gate is never rolled back or reused.

P1-C3's isolated fixture may assert only diagnostic completion. P1-C5 cannot claim canonical integration until the exact existing local phase consumer is recorded and a terminal Clear attempt before gate satisfaction is rejected or diagnostically aborted.

## Cancellation, Cleanup, and Failure

P1-C must receive cancellation for at least:

- P1-A `AbortClosing`/`RestartClosing` or later run-context disposal;
- every typed terminal-action disposal, including Replay, Retry, and Lobby, before dispatch/navigation;
- explicit active-run restart;
- scene exit/unload and owner disable/destroy fallback;
- payload, anchor, factory, or runtime spawn fault;
- terminal stage outcome while any registered sequence instance is not disposed.

The registered P1-C quiescence barrier covers every non-`Disposed` state, including validation, ready, active, completing, completed, cancelled, and faulted instances, plus selected/not-yet-admitted and successor-available/not-yet-selected bindings. Its result is the closed union `Succeeded(EncounterExecutionQuiescenceReceipt)` or `Failed(EncounterExecutionClosureFaultEvidence)`. `closureScope` is `EntryTransition` for the ordinary Practice-to-Challenge close and `RunFinalization` for P1-A terminal finalization, abort, restart, or action disposal. Each reserved binding has one `executionAdmissionOrCloseLatch`: a runtime-admission winner mints the execution identity and must use `ClosedExecution`; a run-close winner may use `NoExecutionStarted` after proving zero instance/work. The unopened Challenge latch already exists while its reservation is retained across Practice close, so an abort/restart between Practice `Advanced` and Challenge selection closes that latch rather than racing an untracked future admission. P1-A seals the exact close authority first: `TerminalFinalizationAuthority` for the pre-result terminal path, the selected terminal-action or active-restart dispatch record for those paths, or `StageRunAbortCloseAuthority` for a diagnostic abort. It then requests P1-C cancel/dispose as needed and awaits the required scope. Timeout or cleanup failure blocks final fact sealing, transition, or dispatch as applicable and records a diagnostic; it cannot reopen a latch or action choice. A pre-commit terminal/abort/restart failure follows run-level `Aborted -> ClosureFaulted`, not `Disposed`; a post-commit action remains presented with dispatch blocked.

`EncounterExecutionQuiescenceReceipt` contains runtime-issued `executionQuiescenceReceiptId`, closure scope, run/route identity, `bindingScope = Present(exact EncounterBindingHostScope, binding/static-plan identity) | NoBindingForCurrentPhase(typed binding-scope/plan absence)`, `executionDisposition = ClosedExecution | NoExecutionStarted`, exact accepted close-command and close-authority kind/ID/canonical digest, exact `courseCloseContext = NonCourse | BeforeFirstSelection | CurrentEntrySelection | BetweenCourseEntries` with its matching typed absence or course/session/entry/selection/transition provenance, close reason/sequence, fixed spine-order `admissionOrCloseLatchCoverage[]`, canonical reservation coverage, canonical `executionQuiescenceReceiptDigest`, and envelope checksum. Each latch row contains binding ordinal/identity, latch ID, and `AdmissionWon | CloseWon | RetainedOpen`; `RetainedOpen` is legal only for a future reservation in `EntryTransition`, never `RunFinalization`. `ClosedExecution` additionally contains exact `EncounterExecutionHostScope`, execution instance/generation, static-plan/encounter digests, execution purpose, invalidated generation, ordered `SpawnLeaseTerminalReceipt` IDs/digests, exact `gateCompletionCoverage` arm defined above, released scene-lease identity, and final `Disposed`. `NoExecutionStarted` carries typed absence of `EncounterExecutionHostScope`, execution identity, spawn/gate/active-lease fields; it requires the close arm to win every relevant unopened binding latch in `RunFinalization`, typed reason `NoBindingForCurrentPhase | BindingNotYetAdmitted | BetweenCourseEntries | NonCoursePreEntry`, exact binding plan when one exists, final `NoInstance`, and zero execution-registry, pending-ticket, owned-root/object, subscription, and active-lease counts. `BetweenCourseEntries` additionally carries the exact prior `CourseEntryTransitionReceipt` ID/digest, successor entry/binding identity, and retained successor `SceneExecutionReservationStateSnapshot` ID/digest while proving no current `CourseEntrySelection`; it is legal after either Basic or Practice `Advanced` and before the successor selection. The other no-execution arms are valid before first course selection, during Basic, at non-course pre-entry abort, or after Practice/Challenge selection but before instance admission. Any authority, scope, identity, latch, registry, reservation, or work disagreement faults. `EntryTransition` accepts only the exact `PracticeExitSelection`; `RunFinalization` rejects that arm and accepts only the matching terminal-finalization, abort, restart, or terminal-action authority. Success never accepts `AuthorityUnavailable`.

Reservation coverage depends on closure scope. `EntryTransition` terminalizes only the current Practice reservation/converted lease and carries fixed-order `retainedFutureReservation[]` `SceneExecutionReservationStateSnapshot` IDs/digests for the unopened Challenge binding; it cannot satisfy P1-A's run-level owner row. `RunFinalization` contains `SceneExecutionReservationTerminalReceipt` ID/digest coverage for every current and future P1-C reservation in fixed spine order and requires zero retained reservations. Spawn-lease rows use immutable static-plan ticket order (serialized group index, spawn-reference ordinal, then unit ordinal), never materialization, death, cleanup, or callback completion order. The canonical digest covers receipt/scope/disposition, complete binding-scope/execution-host-scope/course-close-context arms and typed absences, fixed latch coverage/winners, semantic lifecycle facts, exact accepted close command/authority, complete gate-coverage arm, terminal and retained reservation coverage as permitted by scope, fixed ticket-order coverage, zero counts, and constituent canonical receipt digests; it excludes presentation-only metadata and envelope checksums. A same-run Practice transition consumes only `EntryTransition`; P1-A accepts only `RunFinalization`.

`EncounterExecutionClosureFaultEvidence` contains runtime-issued `executionClosureFaultEvidenceId`, the same closure scope and run/route; exact `bindingScope = Present(EncounterBindingHostScope, binding/static-plan identity) | NoBindingForCurrentPhase`; exact `executionScope = Issued(EncounterExecutionHostScope, executionInstanceId, executionGeneration) | NotIssuedBeforeFault`; fixed binding-latch coverage; exact `courseCloseContext`; and one fault-only `closeAuthorityEvidence = Accepted(EncounterExecutionCloseCommand ID/canonical digest, exact authority kind/ID/canonical digest) | RunFinalizationAuthorityUnavailable(expected P1-A authority kind, local invalidation reason, invalidation sequence, typed absence of accepted command/authority) | EntryTransitionAuthorityUnavailable(expected PracticeExitSelection kind, local invalidation reason, invalidation sequence, typed absence of accepted command/authority)`. It also carries failed boundary, pending delay/ticket/object/subscription IDs, terminalized and retained reservation/lease facts, complete gate command/acknowledgement/sequence-proof partial coverage, fixed typed partial-receipt slots carrying exact receipt type/runtime ID/canonical digest or typed absence, fault sequence, canonical `executionClosureFaultDigest`, and envelope checksum. Binding/latch and retained/terminal reservation facts use fixed spine order; pending tickets use immutable static-plan ticket order; object and subscription IDs use their owning ticket then stable handle/subscription ordinal; partial receipt slots use the same spawn/gate/reservation owner order as success coverage, with otherwise unowned diagnostics in stable ID ordinal. `RunFinalization` permits only accepted terminal-finalization/abort/restart/action authority or `RunFinalizationAuthorityUnavailable`; `EntryTransition` permits only accepted `PracticeExitSelection` or `EntryTransitionAuthorityUnavailable`. An unavailable arm is legal only after local invalidation and before the matching scope-specific command arrived; it can never produce a success receipt. The canonical fault digest covers its runtime evidence ID and every complete scope/context/authority-evidence arm, that exact ordered provenance including typed absence, failed boundary, pending/retained ownership identities, typed partial canonical receipt refs, and fault sequence; it excludes presentation-only metadata and every envelope checksum. It accompanies `Failed`, never satisfies quiescence, and can only enter the course fault path or the appropriate P1-A abort/dispatch-fault diagnostic.

The common terminal order is:

1. atomically enter `Cancelling` or `Faulting` and invalidate the active generation;
2. cancel and drain every pending delay/spawn handle;
3. unsubscribe every combat, lifecycle, scene, and factory callback;
4. stop hostile emitters or behaviors owned by this sequence;
5. destroy/despawn only objects and partial roots recorded through sequence `SpawnLease` entries;
6. clear scene-local anchors, factories, roots, and registry entries;
7. seal `Cancelled` or `Faulted`, then dispose.

After step 1, old-generation work cannot spawn, publish completion, advance a group, or affect a later run. `OnDisable`/`OnDestroy` provide a final idempotent call into the same path; they are not a substitute for the route owner awaiting cancellation before navigation.

Runtime spawn failure policy is fail-closed:

- capture the first failure with its scoped spawn key and ticket ID;
- do not decrement the ticket into a successful terminal state;
- do not auto-clear the group and do not publish stage Fail/Clear;
- cancel remaining work and clean all owned objects;
- request a typed diagnostic abort from P1-A's route/run owner.

## Scene-Scoped Execution Ownership Lease

Static validation cannot prevent a later `BattleManager` bootstrap from creating `PveEncounterDirector`. Revision 1 therefore requires one canonical reservation plus atomic scene-instance lease registry shared by both startup paths:

- P1-A run admission creates one binding-scoped reservation for each canonical encounter binding, keyed by run ID plus fixed spine binding ordinal and carrying stable scene identity, route digest, and execution domain before that scene loads;
- when the scene loads, the reservation binds to the loaded scene instance; it is not awarded by `Start()`/callback order;
- multiple sequential reservations from the same run may name the same scene/domain, but only the current course/binding admission may convert and at most one active scene/domain lease may exist; the later reservation remains unopened rather than becoming a second owner;
- legacy PVE bootstrap must query the same registry before creating a director and may acquire only when no matching active or unopened canonical reservation/plan exists;
- canonical P1-C converts its current matching reservation into the sole active lease; a duplicate/out-of-order canonical owner faults, while PVE stays disabled and creates no spawn work;
- in a standalone legacy scene with no canonical reservation, PVE may atomically acquire the scene/domain lease;
- the lease stores owner kind and P1-A run ID when canonical, but never treats `PveStageContext.SelectedStage` as route identity;
- run abort before scene entry releases the reservation; a reservation already bound to a loaded scene but not yet converted releases with that exact scene-instance provenance; cleanup/disposal releases an active lease exactly once only after quiescence;
- tests require canonical ownership for PVE-first, canonical-first, and same-frame startup when a matching plan exists; sequential same-run Practice/Challenge reservations sharing one scene/domain with never more than one active lease; PVE ownership in a standalone legacy scene; and new-scene reacquisition after release.

A nonterminal course boundary or closure audit that retains or is about to terminalize an unopened reservation seals one immutable `SceneExecutionReservationStateSnapshot` containing runtime-issued snapshot ID, reservation ID, fixed spine binding ordinal, stable scene identity, run/route and execution-domain provenance, state `Reserved | SceneBound`, optional loaded scene-instance identity required only for `SceneBound`, snapshot sequence, canonical `sceneReservationStateDigest`, and envelope checksum. Its digest covers those exact semantic fields and excludes the checksum. `EntryTransition` uses it for retained Challenge coverage; `BetweenCourseEntries` uses it to bind the observed successor reservation immediately before run-finalization terminalization. It is retained/state evidence only, never a terminal receipt or active lease.

Every canonical reservation eventually seals one `SceneExecutionReservationTerminalReceipt` containing runtime-issued `sceneReservationTerminalReceiptId`, reservation ID, fixed spine binding ordinal, stable scene identity, run/route and execution-domain provenance, disposition `ReleasedBeforeSceneEntry | BoundToSceneThenReleasedWithoutConversion | ConvertedToLeaseAndReleased`, optional loaded scene-instance identity for the latter two arms, optional converted lease ID only for `ConvertedToLeaseAndReleased`, release sequence, canonical `sceneReservationTerminalDigest`, and envelope checksum. Its digest covers those exact semantic fields and typed absences and excludes the checksum. Fixed coverage uses immutable spine encounter-binding order; reservation ID plus binding ordinal is unique, while repeated scene/domain identity is legal only for ordered bindings in the same run and never permits overlapping active leases. `RunFinalization` cannot succeed while any reservation or converted active lease remains live. `EntryTransition` may retain only the exact unopened Challenge reservation declared through its `SceneExecutionReservationStateSnapshot`; every other reservation/lease in its scope must close.

If the PVE bootstrap cannot participate in this lease, canonical P1-C5 integration fails closed. A preflight scene scan alone is not sufficient.

## Validation Matrix

| Validation | Failure behavior |
|---|---|
| schema and sequence revisions supported | hard failure before activation |
| `EncounterBindingHostScope`, playable-stage ID/route revision or fixture ID/revision, scene segment, and stage definition agree with the P1-A/P1-B or synthetic fixture snapshot | hard failure |
| linear segment and pocket exist exactly once in the canonical template | hard failure |
| one stage-local execution binding resolves the pocket | hard failure on duplicate; explicit unimplemented status allowed only outside the selected fixture |
| execution purpose and completion arm | `RequiredDefeatRouteSequence` lacks exactly one required local gate, `NonTerminalPracticeActivity` has any defeat/required-clear consumer, or a current required-defeat binding is relabelled as Practice |
| sequence/group IDs are nonempty and unique; serialized order is stable | hard failure |
| first activation is explicit and every later activation is previous-group completion | hard failure |
| every spawn ID resolves exactly once within the referenced stage definition | hard failure |
| every run-admission route snapshot includes exact binding host scope, binding, sequence revision, completion gate, payload-mapping revision, encounter digest, and deep static plan | hard failure; older schemas are not silently upgraded |
| each `SpawnRef` resolves one `AnchorRef` and one live scene anchor | hard failure; duplicate static/live anchors also fail |
| live anchor semantics match | require `UsageKind.CombatSpawn`, the snapshotted Add kind and position ID, static `AnchorRef.groupId`, binding-root-local distance/`Quaternion.Angle` tolerances, then capture the immutable scene-ready world pose |
| every payload ID resolves the snapshotted typed mapping, compatible factory, exactly one live `CombatHealth`, and prefab/component set | hard failure |
| later P2-A binding-set membership, variant/archetype/candidate, mapping prefab, configuration capability, and expected receipt identity agree | hard failure before object creation; absent P2-A snapshot uses no configuration seam |
| later P2-B Practice/Challenge course identities, host/fixture scope, purposes, and generations agree with the course and P2-A snapshots, isolated scopes are not used as product, and entry bindings do not overlap | hard failure before activation; required-defeat binding cannot masquerade as no-proof Practice |
| course activation provenance: course-scoped command/context/handle/receipt omits or disagrees with current course session/generation, entry/generation, or sealed `CourseEntrySelection` ID/digest, or an execution instance is minted without that current selection | reject as stale/foreign before mutation; no root creation or activation |
| Practice close/Challenge activation boundary | hard failure if P1-C can select Challenge, its close command/receipt/fault does not bind the winning `PracticeExitSelection` ID/digest, Practice cleanup can report another owner's work, or both entries can hold the scene lease |
| first fixture kind is `Add`, count is 1, and delay is finite/nonnegative | hard failure |
| no placeholder payload or cutscene-owned boss is selected | hard failure |
| required-defeat completion group is nonempty and all tickets are completion-required | hard failure for `RequiredDefeatRouteSequence`; not a Practice completion rule |
| required-defeat completion consumer is `RequiredLocalEncounterGate` and its gate ID resolves exactly once | hard failure for `RequiredDefeatRouteSequence`; `NonTerminalPracticeActivity` instead requires `NoDefeatCompletionConsumer` |
| canonical admission reservation binds to the loaded scene lease, or standalone PVE proves no matching canonical plan | hard failure/disabled PVE; no call-order first-wins or scan-then-start fallback |
| content digest matches every snapshotted execution fact | hard failure |

Validation success is not runtime proof. The fixture still needs activation, terminal, cancellation, cleanup, and scene-exit tests.

## First Fixture Freeze Gate

No fixture is currently freeze-ready.

| Required fixture fact | Current state | Required predecessor |
|---|---|---|
| logical playable stage and revision | frozen `OLYMPUS-INVASION-01`, revision `1`, route digest `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9` | completed P1-0 route validator; P1-C must consume, not reinterpret, this identity |
| run, static-plan snapshot, and quiescent disposal owner | P1-A has no P1-C plan/gate fields yet | P1-A implementation plus new-schema encounter-plan identity/digest and terminal-action quiescence extension |
| scene segment | frozen `station_entry_combat` ref with `corridor.tutorial.completed -> station.encounter.terminal` and final `ReturnToOwner` | completed P1-0 route validator; P1-B content join remains |
| Station definition and scene binding | frozen `OLYMPUS-STATION-COMBAT-01` profile and scene binding exist; combat anchor/payload fixture remains absent | author the stable P1-C anchor without changing physical identity |
| truthful current-route template/segment/pocket | none of the current five templates matches the route | P1-B authors or explicitly revises one narrow template and freezes exact IDs |
| concrete spawn | Corridor adds are placeholders; boss is cutscene-owned; no Station spawn definition exists | P1-B authors one Station `Add` SpawnRef with count 1, a stable non-placeholder payload identity tied to a concrete archetype/prefab authoring target, and a unique anchor |
| typed payload resolution | no `SpawnRef.payloadId` registry/factory exists | P1-C0 freezes the exact mapping and P1-C1/P1-C2 add its validator and runtime factory |
| activation and clock | no canonical producer, envelope, executor generation, or injected clock exists | P1-C0 freezes the command/clock policy; P1-C2 implements it |
| completion consumer | no exact current local phase gate is selected | P1-C0 records the gate ID/consumer; P1-C5 proves pre-gate Clear rejection and post-gate phase opening |
| canonical execution owner | PVE prototype and scene-specific flows remain separate | P1-A reserves canonical priority at run admission; both startup paths use the same scene lease and PVE may win only without a matching canonical plan |

P1-C0 records the exact final values before code begins. Placeholder IDs such as “first add” or “entry group” are not acceptable freeze data.

## Acceptance Evidence

### Authoring and snapshot

- the selected canonical pocket resolves one sequence and one ordered group list;
- the binding's typed `EncounterBindingHostScope`, execution purpose, and completion-consumer arm are frozen in the static plan and canonical encounter digest; product resolution rejects isolated scope;
- every group resolves the same scoped spawn keys and digest across repeated builds;
- Corridor admission captures the full static encounter plan; editing the asset before Station entry causes a digest fault rather than changing that run;
- no group serializes a transform, payload ID, count, delay, enemy stat, reward, result, or route destination already owned elsewhere;
- missing/duplicate group, spawn, anchor, scene-anchor, or payload references fail before any runtime object exists;
- wrong anchor usage, group, position, spawn kind, or expected-pose tolerance fails before activation;
- moving an anchor after scene-ready capture does not change the delayed spawn pose for that admitted run;
- placeholder payload, unsupported kind, count above the first-fixture limit, and a lost scene-ownership lease fail before activation.

### Deterministic normal path

- one exact typed activation envelope from the sole adapter starts the first group once;
- duplicate activation is a no-op with a diagnostic;
- a delayed activation from an older run/instance/generation is reject/log-only and leaves the current sequence unchanged;
- spawn tickets execute by absolute delay and serialized tie order;
- scaled-clock pause freezes pending delays and resume preserves their remaining order;
- all required instances must spawn and emit current-generation `CombatHealth.Died` before completion;
- completed group handles are unsubscribed and cleaned before local advancement;
- a synthetic two-group fixture proves that intermediate group completion leaves the sequence `Active`, then advances exactly once;
- simultaneous or duplicate death callbacks emit one group completion and one local advance;
- for `RequiredDefeatRouteSequence`, the final group emits one sequence completion and exactly one local gate command, with no direct stage result/progression/reward/navigation mutation;
- for `NonTerminalPracticeActivity`, final-group defeat leaves the sequence Active, emits no sequence completion/gate/course transition, and waits for the already sealed explicit exit plus cancellation/quiescence;
- a local phase that emits a terminal callback synchronously on open observes the gate already `Satisfied`; stale/duplicate gate acknowledgements open nothing.

### Failure path

- unresolved payload/anchor and factory-null/exception paths fault rather than clear;
- a factory that creates a root and then throws or lacks `CombatHealth` cleans the lease-recorded partial root;
- a required P2-A configuration adapter that is missing, stale, throws, returns a mismatched receipt, or changes activation/lifetime ownership fails while inactive and cleans the complete staged root;
- Story/Practice/Challenge isolated fixtures prove distinct reviewed role/deck/elite configured digests on the same mapping prefab before activation; prefab defaults alone cannot satisfy P2-A acceptance;
- a later isolated course fixture seals `ProceedToChallenge` before Practice cancellation, carries that exact `PracticeExitSelection` ID/digest through the P1-C close command and success/fault evidence, reaches P1-C quiescence before Challenge activation, and gives Challenge a fresh execution generation without creating outcome/mastery/progress;
- only the current sealed Practice or Challenge `CourseEntrySelection` can mint its P1-C instance/generation; replaying the prior entry's command, selection, P2-A receipt, or staged handle creates no root and cannot activate the next entry;
- that Practice fixture uses `NonTerminalPracticeActivity + NoDefeatCompletionConsumer`; defeating or exhausting its current objects cannot satisfy a required-clear gate or close the activity;
- `Awake`/`OnEnable` spy fixtures prove no encounter callback escapes before the inactive root, ledger, observer, and ticket are armed; activation callbacks are processed only after the factory transaction returns;
- unexpected disable/destroy before `CombatHealth.Died` faults, while cleanup-generated events after invalidation are ignored;
- a failed delayed spawn cannot produce an empty successful group;
- first fault cancels remaining tickets, cleans every owned object/subscription, and requests one diagnostic abort;
- failure publishes no group completion, next-group activation, clear, fail, mastery, progress, or reward.

### Cancellation and stale-work path

Run the same fixture while cancelling:

- while `Ready`, before activation;
- before the first due time;
- between schedule entries;
- while an owned enemy is active;
- on the completion boundary;
- while `Completing`, before the required gate acknowledgement;
- at the exact same clock tick as a due spawn, with cancel admitted before drain;
- during Retry/Replay/Lobby disposal;
- during scene exit/unload and owner disable/destroy fallback.

Every path must end with pending handles 0, owned live objects 0, retained subscriptions 0, late spawns 0, late completions 0, late advances 0, and late local-phase opens 0. Success seals one `EncounterExecutionQuiescenceReceipt` with the exact released lease, canonical constituent receipt digests, `executionQuiescenceReceiptDigest`, and envelope checksum; injected cleanup failure instead seals fault evidence and never satisfies the barrier. A gate acknowledgement arriving after `Completing` cancellation is reject/log-only. Immediately starting a new run must show that old-generation callbacks cannot affect it.

### Integration boundary

- canonical execution cannot coexist with `PveEncounterDirector` ownership for the same loaded scene instance;
- with a matching canonical reservation, PVE-first, canonical-first, and same-frame startup all produce the canonical lease winner and zero PVE spawns; a standalone legacy scene produces the PVE winner;
- the route owner awaits the P1-C quiescence barrier for active or completed instances before any Single-load terminal action;
- for a later P2-A-bound ticket, P1-C owns the inactive root and activation decision, P2-A owns only the typed configuration call/receipt, and neither barrier claims the other's lifetime work;
- canonical sequence completion opens the named existing local gate exactly once, and a terminal Clear attempt before that gate is rejected/aborted;
- a later course transition consumes the canonical P1-C quiescence receipt digest, not its envelope checksum, but never treats a P1-C gate or sequence completion as learner proof, stage outcome, or mastery;
- P1-C emits execution/gate facts only. P1-A remains the only outcome/fact owner; P1-D alone evaluates an explicitly authored semantic-proof adapter before result commit and owns clear/mastery persistence afterward; P2-C owns the later reward extension.

## Ordered Implementation Backlog

| Slice | Work | Exit gate |
|---|---|---|
| P1-C0 | approve binding, activation producer/envelope, scaled clock/queue precedence, separate group/sequence states, typed terminal, completion gate, expected-pose tolerance, payload mapping, scene lease, and exact P1-B fixture IDs | all fixture and ownership values concrete; no placeholders |
| P1-C1 | add the smallest sequence/group schema, P1-A static-plan snapshot extension, scoped-ID/payload-mapping resolver, content digest, and editor validator | all static validation and edit-after-admission fixtures pass; no runtime spawn |
| P1-C2 | implement live scene binding, inactive staging/activation barrier, transactional payload factory/lease, ticket ledger, execution generation, activation queue, group/sequence lifecycle, scene ownership lease, quiescence, cancellation, and cleanup kernel | unit/edit tests prove state, pre-activation callback containment, partial-factory cleanup, and stale-generation invariants |
| P1-C3 | execute one `Add`, count-1 group in an isolated scene fixture | deterministic activation/spawn/terminal/advance proof |
| P1-C4 | add two-group, pause, same-tick cancel, partial-factory, unexpected-destroy, Replay/Retry/Lobby, scene-exit, duplicate-callback, and both-order owner-race PlayMode fixtures | no false clear, leak, late spawn, duplicate advance, or double owner |
| P1-C5 | bind the P1-B-approved canonical pocket and named local completion gate only after P0/P1-A/P1-B are current and accepted | pre-gate Clear rejected, post-gate existing phase opens once, route disposal is quiescent, and no result/progression/reward ownership drifts |

Re-score P1-C cost and regression after P1-C3 and P1-C4. Do not expand to a second stage or group type merely because the first fixture passes.

## Explicitly Deferred or Rejected

- generic condition DSL, event graph, behavior tree, or graph editor
- Arknights tile/lane/deployment/block/life rules
- GF2 grid/deployment rules or opaque event/trigger interpretation
- random role selection or automatic conversion from `StageEnemyRoleSlot` to payload
- member/action overrides, per-unit interval, branching, overlap, or parallel groups
- multi-anchor formation/offset policy and counts above the first fixture limit
- Player, Boss, Rift, Objective, structure, emitter, cinematic, or tutorial-owned spawn kinds
- enemy archetype/variant/stat expansion, difficulty scaling, modifiers, or stage rules
- tutorial evaluator/reset extraction
- product Practice/Challenge course bindings before the separate P2-B host/snapshot contract and bounded P2-A product-scope extension are approved
- boss/cutscene ownership migration
- pooling, Addressables, streaming, or broad content-generation tooling
- mastery, persistence, unlocks, rewards, receipts, economy, result UI, and retry/navigation logic
- copying the legacy `PveEncounterGroup` lane/depth/placement schema into the canonical route

## Archive Sources Used for This Contract

Paths are relative to `\\DESKTOP-69817L3\ArkData\SubcultureGameData`.

- `games/arknights/raw/arknights-game-data/2026-06-13/files/stage_table.json`
- `games/arknights/raw/arknights-game-data/2026-06-13/files/level_samples/level_main_00-01.json`
- `games/arknights/raw/arknights-game-data/2026-06-13/files/stage-level-join.csv`
- `games/girls-frontline-2/raw/torikushiii-gfl2data/2026-06-13/files/extracted_repo/GFL2Data-main/tables/StageConfigData.json`
- sibling `StageEnemyGroupData.json` and `StageEnemyData.json`
- `games/last-origin/raw/hibikidesu-lastorigin-data/2026-06-18/files/extracted_repo/lastorigin-data-master/jp/table/table_mapstage/table_mapstage.json`
- sibling `table_mobgroup/table_mobgroup.json`
- `games/zenless-zone-zero/enemies-stages/zzz-public-code-candidate-client-code-d0-levelworld-floor-group-member-stage-layout-summary.json`
- `games/aether-gazer/enemies-stages/aether-gazer-stage-topology-wave-context.md`
- `games/aether-gazer/enemies-stages/aether-gazer-stage-topology-wave-context.csv`
- `games/aether-gazer/notes/combat-stage-readable-joins-2026-06-15.md`

These are structural references only. Do not copy source code, assets, IDs, layouts, tuning, dialogue, formulas, or proprietary content.
