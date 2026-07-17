# Tutorial Lesson, Attempt, and Gameplay Reset Spec

## Current P1-B closure

- P1-B Station Add and full-exit closure (2026-07-16): `SNAP-P1B-STATION-ADD-AUTHORING-REMEDIATION3-ACCEPTED-11` binds `C:\tmp\DimensionBrawl-P1B-StationAdd-Remediation3-Bundle.md` at SHA-256 `9378bc021b09495c350b331a85755eac7b956a2372d78ecca848a94c2d570c76`; source `128/128` matches digest `4c3dbe952bea5e4f5c57632d70e6fba815d7f6900dc9e1dcbee6af69bae86c89`, artifacts `11/11` match digest `eb5699917083d9be13d571f2a64aa0f69048304552b962df3467b89f3469ce2b`, validator/inventory `8/4/1/1/0`, integrated focused `8/8`, Canonical UI `34/34`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `99/99` all pass with three independent audits at blocker `0`. Revision-1 pose remains relative to `StageDefinitionSceneBinding.transform`; Station `MapRoot` is topology containment only. `ACC-P1B-STATION-ADD-AUTHORING = PASS`; the foreign-evidence row remains PASS through explicit rejection only; `SNAP-P1B-FULL-EXIT-ACCEPTED-12` closes `ACC-P1B-FULL-EXIT-AUDIT = PASS`, so P1-B is **ACCEPTED / VERIFIED-COMPLETE**. This admits no P1-C runtime owner: only the prospective authoring-ledger freeze may start, and runtime work remains gated by `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`.

## Status

- P1-B result/progression Remediation3 acceptance: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION3-ACCEPTED-08` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation3-Bundle.md` at SHA-256 `94fa969979bdb2a2b91dfbdf8a5395aed0a69ddd8907831bb7c99da06b139a5b`; source `116/116` matches digest `271793a22e2afc24779a3aeeace7cb9768aae77b7bbbf18a075fa15ea409efb2`, artifacts `14/14` match list digest `c3642305e13c085f710e8db62df807463aea58d8a57331cd7526460eb7a404fc`, validator/inventory `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `98/98` all pass. Independent source, artifact/test, and semantic-contract audits find blocker `0`: route/sidecar-owned canonical catalog identity is independent of the result definition, public Corridor admission and the editor validator require exact object identity, and catalog-only plus coherent catalog/profile/localization clones reject before run creation. Frozen route/policy/join/lifetime digests remain unchanged. `ACC-P1B-RESULT-PROGRESSION-JOINS = PASS / VERIFIED PARTIAL`; Candidate-07 remains immutable historical FAIL. Station count-one Add authoring is now unheld as the next separate P1-B gate, while live PGR/HI3 disposition, P1-B full exit, and P1-C execution remain OPEN and no P1-D/P2-C owner is admitted.
- P1-B result/progression Remediation2 candidate audit: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION2-CANDIDATE-07` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation2-Bundle.md` at SHA-256 `a4e2e2873ec4f53ba81a6c6a3269949b4b2f19255f566d333fcb058e3eeb6de8`; its submitted source manifest matches `116/116` with digest `f4c6f0a6065a2f304acd1a56f7d126b4b2be49582f752f707757d87f37c35583`, all `14/14` artifacts match list digest `96176b861dc7ce0a9aaccd86fe035aa59433513383713132248e51f974b6228a`, validator/inventory is `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact full route `1/1`, and graphics aggregate `98/98` pass. Independent source/contract/test audits verify that Candidate-06's three blocker groups, locale/graph rows, and exact durable-decision byte preservation are closed, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / VERIFIED-FAILED-CANDIDATE-PARTIAL` on one remaining admission defect: the result definition self-selects its catalog, so a catalog-only clone or coherent catalog/profile/localization clone can evade the intended exact-identity gate. The post-bundle route-owned catalog-anchor WIP changes five submitted files and cannot retroactively amend this cutoff. Station Add and P1-B full exit remain held until a new sealed-source candidate passes.
- P1-B result/progression joint-freeze: `P1B-RESULT-PROGRESSION-JOINS-01` Rev3B proposal artifacts match SHA-256 `b6e63b11e3e270302dc33f95b7b69740565e4e27a13ffe017a17f2899256c88f` / `eb65cf30eb961a271f135bc38a9874cccae49e47d8a9d0af5a6dd5f0d7211199` / `933c13943e5397f5fa7a1be531ae34bd28f595e09feee14f18429daa81a8e603`. Fresh PowerShell, independent Node, and a third row reconstruction preserve the seven `15/35/15/17/8/9/38` blocks, sidecar/join snapshot digest `a2ae9df451bd6f2ff48b83098db3bfbdaf2120e23dfaf3612a31f18a022c41fa`, all predecessor digests, and the separate 11-row lifetime-contract digest `3b6cf33325a0a83db74ee2253da9799e589b5664f4fb677b2b021389b0714c0e`. Exact `(ID, revision)` edge resolution and the no-token `Stage Select A -> pre-admission mutation B -> fresh Corridor B` boundary pass. Verdict is **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**. This authorizes implementation only: `ACC-P1B-RESULT-PROGRESSION-JOINS`, Station Add, foreign evidence, and P1-B full exit remain **OPEN**, and no P1-C/P1-D/P2-C owner or P1-A digest change is admitted.
- P1-B result/progression Rev3B implementation candidate audit: `C:\tmp\DimensionBrawl-P1B-ResultProgression-Implementation-Bundle.md` matches SHA-256 `35b1b1a5523bc457ad1936190d1d41143dd1bc8a3489624cdb600631c3a6daa1`; submitted source manifest `116/116` matches digest `1b3dba021b40a4be9d728c6fd4f2039864abb399bbff6d2907e4af274bec24ec`, all `14/14` declared artifacts match list digest `249da60824d3ef617937e648e1257b1fde9b50dc28082a904b78513ca7c76023`, both contract verifiers pass, validator/inventory is `8/4/1/1/0`, focused `2/2`, Canonical UI `28/28`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `93/93` pass. These green artifacts are verified, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / SOURCE-CONTRACT-FAILED-CANDIDATE`: canonical profile/localization object identity is not enforced at admission, the `Presented -> terminal action` path omits the exact pinned join/presentation/audit authority gate and audit self-integrity, and representative deep snapshot damage can throw instead of returning a typed rejection. Direct clone/damage/dispatch, recovery/process-loss, locale, and production graph acceptance rows remain open. The Rev3B joint freeze and every accepted predecessor cutoff/digest remain unchanged; Station Add and P1-B full exit stay held pending remediation and a new sealed-source bundle.
- Drafted: 2026-07-14
- Status: provisional P1-E review contract; analysis only
- Roadmap source: [Subculture Dataset Gap Roadmap](SUBCULTURE_DATASET_GAP_ROADMAP.md), P1-E
- Run/result predecessor: [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md), P1-A
- Route/reference predecessor: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md), P1-B
- Encounter predecessor: [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md), P1-C
- Progress predecessor: [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md), P1-D
- Later presentation successor: [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md), P2-B
- Later course-chain successor: [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md), P2-B
- Working archive root: `\\DESKTOP-69817L3\ArkData\SubcultureGameData`
- Production order remains `P0 -> P1-0 -> P1-A -> P1-B -> P1-C -> P1-D -> P1-E`. This document does not authorize production work before those gates close.
- Current predecessor boundary: P1-A's verified route/run schema-1 `NonCourseStationTerminal` coverage marks the P1-E lesson row `NotAdmitted` with no typed receipt. The final 11-source cutoff passes focused 21/23/15, aggregate 79/79, full route 1/1, and validator checks and closes P1-A current-schema full exit while preserving that zero-admission row. The existing `LegacyOpaque` whole-tutorial fact seal is therefore neither a P1-E attempt result nor a lesson-quiescence receipt, and no P1-A cutoff proves P1-E cancel/disable/unload reset parity; this contract remains unimplemented.
- P1-B predecessor boundary: three accepted immutable 80/80 local cutoffs verify the existing Corridor intro consumer plus static port/binding and anchor/profile stage-context hygiene. `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-04` remains the historically rejected authored-reward-row/invalid-selection submission and is not retroactively accepted. Its unchanged-source remediation `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05` accepts `ACC-P1B-CANONICAL-SELECTION` for the frozen read-only catalog projection after validator, focused 8/8, Canonical UI 21/21, exact full route 1/1, and aggregate 86/86 evidence. At that cutoff the reference/template/briefing freeze remained open; rev2A later froze the contract, and the separate truthful-join implementation cutoff now passes while P1-B full exit remains OPEN. This still adds no typed lesson identity, attempt result, gameplay-acquisition ledger, reset/quiescence receipt, or lesson result/progression owner. P1-E remains unimplemented.
- P1-B truthful-join rev2A boundary: rev2A jointly freezes the 71/27/80 truthful route contract and admits implementation; the first proposal and 71/27/78 rev2 stay historical AMEND. Its active-run-restart pair remains typed current-schema absence, not a lesson retry, attempt result, gameplay-acquisition ledger, or reset/quiescence receipt.
- P1-B truthful-join implementation cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1B-TruthfulJoins-Implementation-Bundle.md` matches SHA-256 `8ef3a8e234f53ef561dfdd5d805d0f69c8ddbb55d2a2534ca427f2da821a9d0a`; all 51 ordered sources match manifest digest `1d2fc6a142fa7582e76095c8a928ca1f61f4453ac7061f5d50525673d1480324`, all 13 declared artifacts match, PowerShell and Node reconstruct `71/27/80`, and the validator passes `8/4/1/1/0`. Focused 7/7, canonical UI 26/26, exact full route 1/1, and graphics aggregate 91/91 pass with 91 unique full names and class counts `26/21/3/2/16/23`; frozen route/policy/projection/template/reference/briefing digests match. `ACC-P1B-TRUTHFUL-JOINS` is **PASS / VERIFIED PARTIAL**, while P1-B full exit remains **OPEN**. At its later historical cutoff, Candidate-06 fails `ACC-P1B-RESULT-PROGRESSION-JOINS` on three blocker groups. Remediation2 Candidate-07 subsequently closes those groups but still fails one independent canonical-catalog identity anchor; a new sealed-source candidate is next, then Station Add, live PGR/HI3 foreign evidence, and full exit. This adds no P1-C execution owner, result/progression/reward join or owner, pre-result active-run restart, or P1-E lesson/reset owner.

P1-E separates one tutorial lesson's authored presentation, one attempt's semantic proof, and gameplay state acquired by that attempt. It does not replace the whole Olympus tutorial director, create a generic lesson graph, persist tutorial progress, or move presentation cleanup to P2-B early. A later P2-B schema may consume a fully closed result as one strict course transition, but it does not change P1-E's proof/reset authority.

## Current Verdict

No complete P1-E fixture is freeze-ready.

- `OlympusCorridorTutorialDirector` is not serialized in the current Corridor scene. `OlympusCorridorCombatFlowController` creates it at runtime from code defaults, while the serialized `tutorialDirector` reference is null.
- The current chain is `SoldierChallenge -> Melee -> Move -> SwapToRanged -> Fire -> Dodge -> ClearTargets`. `SoldierChallenge` is a cue-only sequence prelude, not a proven lesson attempt.
- P1-A now seals `olympus.corridor.core-tutorial` revision 1 plus these seven ordered rows as `LegacyOpaque/NoResultExpected` before SingleLoad. That closes the current whole-route fact handoff only; it is not a P1-E lesson snapshot, attempt/result, evaluator, retry, or gameplay-reset proof.
- Ordinary lessons currently use at least 0.85 scaled seconds of cue, open one action-observation window, delay commit until at least 0.35 scaled seconds of observation, then show confirmation for at least 1.15 scaled seconds. Prompt repeat uses unscaled time instead.
- The director owns sequence order, input locks, combat-mode setup, target selection, target pose, target health, enemy behavior, boss telegraphs, presentation, observation, success, and cleanup in one component.
- There is no stable lesson ID, attempt ID, attempt generation, immutable lesson snapshot, typed result, gameplay ownership ledger, retry, lesson skip, explicit failure, or per-lesson route handoff.
- `LastCompletionRecord` is a mutable string such as `Move:space_created`; enum names and UI text are not semantic IDs.
- Normal completion clears several domains before raising `Completed`, after which the flow immediately single-loads Station. A result collected next frame would already be too late.
- `CancelTutorial()` does not deactivate tutorial target roots/colliders, restore route blockers/bounds, clear target candidates or hard lock, restore target pose/health/AI, restore the player's prior combat mode or pose, or explicitly release a source-owned invulnerability lease.
- Enemy behaviors are forced off rather than restored from captured prior values. Current input locks and boss-telegraph suppression are not source-scoped leases.
- A missing director may currently be treated by active-phase polling as tutorial completion. Missing/destroyed required ownership must instead become a typed system fault and diagnostic abort.
- `ClearTargets` may succeed when target bindings are empty. Missing required bindings must never be interpreted as zero remaining targets.
- Current P0 provides a fresh route-parity baseline: the uninterrupted full-route test passes through actual Retry/fresh Corridor, with independent current natural-handoff and Lobby evidence. P1-E still needs its own cancel/disable/unload, stale-callback, and per-lesson parity fixtures rather than treating P0 as typed attempt coverage.

The first presentation/attempt/proof candidate is `Move`, with review ID candidate `olympus-invasion.corridor.move`. Its current semantic rule is planar displacement of at least 0.75 metres from the position captured when the observation window opens. This is the narrowest candidate because it does not depend on damage attribution, target lifetime, AI, or projectile ownership and already has the strongest real-input coverage.

`Move` deliberately has no lesson-owned gameplay reset. Its policy disposition is `NoGameplayMutation`, its position effect is `PreservePlayerPosition`, and a successful close emits receipt status `ReleasedNoMutation`; the current director keeps input, forced-facing, and presentation ownership. Therefore a promoted Move slice does not satisfy P1-E's real gameplay-reset gate.

The first real gameplay-reset candidate is `Fire`, while its current composite evaluator remains delegated to the director. It can prove owned in-flight projectile cleanup, target entry-state capture, retry/cancel restoration, and an explicit natural-success transfer of reviewed target state back to the current director's course gameplay lease for the following lesson. It is not ready until projectile identity, target ownership, and transfer semantics exist.

## Evidence Boundary

The archive supports separating catalog/presentation, attempt conditions, and reset authoring. It does not prove that another game's runtime cleanup is complete, idempotent, or safe to copy.

| Source | Directly observed | Local use | Not proven |
|---|---|---|---|
| PGR guide, teaching, course, and practice tables | Derived focus scan covers 64,034 rows from 426 EN/ZH JSON files. Direct locale snapshots include `GuideFight` 4/4, `GuideFightStep` 14,482/14,482, `CourseStage` 30/30, and `PracticeSkillDetails` 85/85; the exact `GuideFight -> Stage` join has eight locale rows, four per locale, representing four semantic links | separate guide presentation, stage/course references, practice details, and current route proof | a join between the four fights and 14,482 overlay rows; runtime evaluator; reset or cleanup owner |
| GF2 simulation tutorial and guide tables | `SimCombatTutorialSectionData` 4 rows, `SimCombatTutorialProgressData` 4 rows, and `TutorialGroupData` 307 rows; 97 groups have nonzero `isPause` and three have a nonempty `finishGroup` | keep catalog/progress separate from trigger, finish, pause, and presentation authoring | a direct section-to-guide join, retry/reset semantics, runtime pause ownership, reward application |
| ZZZ guide and popup data | derived bridge contains 689 timing/mask rows, 1,069 mask-target-classified highlight rows, 76 button/tab-target-classified highlight rows, and 232 popup/media rows; raw guide config carries timing, mask, highlight path/button, and extra-prefab fields | use stable presentation-target adapters and keep tutorial media separate from proof | raw boolean counts, semantic target stability, completion rules, cleanup, or runtime media behavior |
| Wuthering Waves guide and combo-teaching data | `GuideStep` 4,439 rows with success/failure/tick/skip/break/time fields; `GuideGroup` 1,417 rows with 302 `ResetInDungeon=true`; `ComboTeachingCondition` 959 rows with 770 populated failure conditions and 15 populated buff-removal rows | justify separate attempt terminal policy and bounded reset authoring | decoded opaque conditions, reset execution, cleanup parity, post-result Retry; projectile/summon cleanup fields exist but are empty in this snapshot |
| Aether Gazer stronghold data | three exact stage-ID joins from `ActivityStrongholdCfg` into `BattleStrongholdStageCfg` separate activity-stage identity from stage tuning, team restrictions, hero lists, and revive rules | secondary precedent for later loadout/rule authoring | tutorial evaluator, attempt reset, or runtime enforcement |

Evidence paths used for this contract:

- PGR derived: `games/punishing-gray-raven/enemies-stages/pgr-tutorial-stage-focus.csv`, `pgr-tutorial-stage-context-rows.csv`, and `pgr-guidefight-stage-reading-links.csv`; direct locale suffixes under each `<EN-or-ZH>/bytes/` root include `share/guide/GuideFight.json`, `GuideFightStep.json`, `GuideGroup.json`, `share/fuben/teaching/Teaching.json`, `share/fuben/course/CourseStage.json`, and `client/fuben/practice/PracticeSkillDetails.json`.
- GF2: `games/girls-frontline-2/raw/torikushiii-gfl2data/2026-06-13/files/extracted_repo/GFL2Data-main/tables/`.
- ZZZ: `games/zenless-zone-zero/ui/zzz-ui-motion-transition-bridge.csv`, raw `Data/ConfigNewbie.json`, and sibling `FileCfg/PopupWindowConfigTemplateTb.json` under the 2026-06-14 mirror.
- Wuthering Waves: `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/GuideStep.json`, `GuideGroup.json`, and `ComboTeachingCondition.json`.
- Aether Gazer: `games/aether-gazer/enemies-stages/aether-gazer-stage-topology-wave-context.csv` and the linked 2026-06-14 `ActivityStrongholdCfg.lua` plus `BattleStrongholdStageCfg.lua` snapshots.

Every count above is a static or derived snapshot count, not a claim about unique shipped lessons. Locale mirrors are not summed into product scale. The local attempt-generation, ownership-ledger, terminal-CAS, and cleanup-barrier rules below are DimensionBrawl safety requirements, not copied game behavior.

## Decision Summary

| Concern | P1-E decision |
|---|---|
| first semantic fixture | `Move`; extract presentation first, then result adapter, then a shadow `MoveDistance` evaluator |
| first gameplay-reset fixture | `Fire`; keep its current composite success evaluator delegated while reset ownership is proven |
| sequence owner | current `OlympusCorridorTutorialDirector` throughout P1-E |
| presentation owner | current director throughout P1-E; P2-B may acquire only separately proven domains later |
| authored identity | stable lesson/plan/proof/reset IDs; enum names, scene paths, object names, and display strings are adapters only |
| evaluation source | immutable lesson snapshot plus generation-scoped observation facts and complete collector coverage |
| observation boundary | exact `Cue -> Observing` transition; earlier events and displacement are excluded |
| current Move parity | a pointer held from Cue may continue after unlock, but only displacement after the observation baseline counts |
| timing | result elapsed uses P1-A's run-active unscaled clock; the 0.35-second commit gate uses one route-wide injected scaled rule clock; a third pause-independent monotonic watchdog bounds post-outcome closure only |
| terminal decision | one compare-and-set freezes at most one attempt outcome; confirmation UI cannot rewrite it |
| missing binding | admission/system fault; never false, zero, target-cleared, or success |
| reset authority | only gameplay domains acquired and recorded by the attempt; no scene-wide scan |
| success cleanup | restore, destroy, preserve, or transfer each owned domain according to an explicit terminal disposition |
| reset failure | system fault that blocks next lesson and scene handoff; never a learner failure |
| P1-A integration | append ordered immutable facts only for snapshotted instrumented lessons before Corridor handoff; retain explicit legacy coverage and the whole-tutorial fact without backfill |
| persistence | none in P1-E; TutorialProgress and rewards remain deferred |

## Identity and Authoring Contracts

### `TutorialLessonPlanDefinition`

This is a minimal route-segment plan snapshot, not a generic `TutorialCourse`:

- stable `lessonPlanId`
- monotonically increasing `planRevision`
- `semanticContentDigest`
- owning `playableStageId`, route revision range, and `segmentId`
- ordered prelude and lesson binding records, each with `LegacyOpaque`, `ResultAdapter`, or `TypedEvaluator` instrumentation capability
- expected current-director adapter ID and adapter version
- required collector-capability IDs
- required stable scene-binding IDs

During P1-E this plan shadows and validates the current director's chain; it does not drive order. A partial slice emits lesson facts only for entries whose snapshotted capability promises them. Absence for `LegacyOpaque` is explicit coverage, not learner failure or an observed zero; absence for an admitted result-capable entry is an integrity fault. A sequence or instrumentation-capability change is a new semantic plan cohort. P2-B may later consume the same reviewed identities through [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md), but may not silently change a recorded P1-E plan.

`SoldierChallenge` is authored as a route prelude presentation record. It has no attempt ID, completion proof, or lesson-complete fact unless a future product decision turns it into a real learner action.

### `TutorialLessonDefinition`

- globally unique `lessonId`
- monotonically increasing `lessonRevision`
- immutable `lessonPurposeKind`
- `semanticContentDigest`
- `presentationId`
- `attemptContractId`
- optional `gameplayResetPolicyId`
- current-director step adapter ID
- stable entry and successful-exit references within the route plan

The review candidate `olympus-invasion.corridor.move` is not a production constant until P1-E0 approval. `CurrentStepId`, `TutorialStep.Move`, and `MoveJoystickRing` may be adapter inputs but never become stored identity.

A copy, icon, voice, or display-order change updates only presentation identity. A threshold, allowed-input, observation, rule, target, clock, reset-disposition, or sequence-binding change updates the semantic revision and digest. Reusing a lesson ID for a different pedagogic purpose or completion kind is forbidden; that requires a new ID. P1-E0 keeps one narrow version-controlled inventory for every P1-E-authored stable plan, lesson, presentation, attempt-contract, proof, and reset-policy ID, with first owner and Active/Retired state. Retired IDs are not reused. This inventory contains no runtime attempts, progress, or generic project identity framework.

### `TutorialLessonPresentation`

- stable `presentationId`, revision, and `presentationContentDigest`
- speaker and title localization keys
- cue and confirmation localization keys
- phase label/icon references
- voice/media cue references
- stable `focusTargetId`
- optional reviewed fallback normalized anchor
- repeat timing and presenter style

Presentation data cannot contain success predicates, mutate attempt state, or clean gameplay. A `TutorialPresentationTargetAdapter` resolves `focusTargetId` to the live HUD target. Display-object-name search is allowed only behind the current compatibility adapter until a stable binding passes parity; failure to resolve a required target is explicit and never changes proof.

Current Move presentation parity values are:

| Field | Current value to preserve during extraction |
|---|---|
| speaker | `천계관리시스템` |
| cue | `조이스틱 버튼을 사용해 이동할 수 있습니다.` |
| cue label | `이동` |
| focus | current `MoveStick` intent through a stable adapter |
| fallback anchor | `(0.16, 0.16)` |
| cue voice | current `MoveCue` slot |
| confirmation | `이동 입력이 확인되었습니다.` |
| confirmation voice | none in the current slot |
| repeat | 4 unscaled seconds |

These are parity observations, not success logic and not frozen localization IDs.

### `TutorialAttemptContract`

- stable `attemptContractId`, revision, and semantic digest
- closed `completionRuleKind` and discriminated parameters
- stable success `proofId`
- required collector-capability IDs
- allowed input-action IDs
- exact observation-open policy
- nonnegative integer `minimumObservationRuleMilliseconds`
- failure policy
- skip policy
- break policy
- cancel/interrupt mapping
- fixed route-level result, rule-window, and closure-watchdog clock policy IDs
- terminal gameplay-reset disposition reference

First-slice policy is intentionally narrow:

- failure: `Unsupported`; there is no current timeout or learner-failure rule;
- skip: `Unsupported`; intro skip is not tutorial-lesson skip;
- break: `Unsupported`; external table meanings are not guessed;
- retry: not exposed as product UI until the Fire reset gate passes; a request after any outcome has frozen is rejected and cannot replace that outcome;
- cancel: owner-requested cancellation becomes `Cancelled`;
- disable, unload, owner loss, or route replacement becomes `Interrupted` with a closed reason;
- definition, binding, clock, or attribution integrity errors detected before outcome freeze become `Interrupted + SystemFault` and force diagnostic abort;
- producer-drain, cleanup, transfer, presentation-step-boundary, or result-publication errors after outcome freeze do not create `Interrupted` or rewrite the frozen outcome; they attach typed closure-fault evidence to the route's diagnostic abort and publish no closed attempt result.

An unsupported request is rejected without inventing Completed, Failed, or Skipped. A later reviewed skip has a distinct `Skipped` result with no proof and can never count as completion, mastery, or progress.

## Immutable Snapshot and Runtime Identity

### `TutorialLessonPlanSnapshot`

The route owner deep-snapshots the plan at logical-stage admission and binds stable scene IDs only when the Corridor segment is scene-ready:

- schema version
- run, playable-stage, route, and segment identity/digests
- plan ID/revision and semantic digest
- ordered copied prelude/lesson identities
- copied lesson semantic records
- copied presentation records needed for this run
- ordered per-entry legacy/result/evaluator instrumentation coverage
- current-director adapter version
- required collector-capability set
- required stable-binding set
- P1-A run-active clock ID/frequency, route-wide scaled tutorial-rule clock ID/frequency, and pause-independent closure-watchdog clock ID/frequency
- canonical `planEvaluationDigest`
- separate `planPresentationDigest`
- full-envelope checksum

The evaluation digest excludes copy, icon, voice, presentation-row display order within a lesson, fallback anchor, and presentation revision. It always covers ordered lesson-plan sequence and per-entry instrumentation capability. The presentation digest excludes completion rules and gameplay reset. The envelope checksum protects both but does not participate in proof semantics.

Authoring changes after stage admission cannot reinterpret the current run. A scene binding is admitted only when its stable ID, expected component capability, and plan digest match. Missing or ambiguous bindings fault before the lesson opens.

### Host-scope unions

Every materialized attempt artifact carries one exact `TutorialAttemptHostScope = NonCourse(all course fields typed absent) | CourseBasicSelected(courseId, courseSessionId, courseGeneration, Basic entryId, entryGeneration, CourseEntrySelection ID/canonical digest)`. The selected arm is copied unchanged through context, outcome, result, continuation selection, retry reservation, closure-fault evidence, and every attempt-owned receipt; no producer may reconstruct it from whichever course fields happen to be present.

Barrier registration owns one `TutorialLessonBarrierHostBindingState = PendingBasicBinding(courseId, courseSessionId, courseGeneration, stable Basic entryId) | Frozen(TutorialLessonBarrierHostScope)`. A NonCourse admission is created directly as `Frozen(NonCourse)`. A course admission starts only as `PendingBasicBinding`: the initial Basic-selection compare-and-set atomically creates the exact `CourseEntrySelection` and changes the same P1-E registration to `Frozen(CourseBasicSelected(courseId, courseSessionId, courseGeneration, Basic entryId, entryGeneration, CourseEntrySelection ID/canonical digest))`. If close wins before Basic selection, the same course-owner serialization instead freezes it once as `Frozen(CourseSessionBeforeBasicSelection(courseId, courseSessionId, courseGeneration, stable Basic entryId, typed absence of entry generation and CourseEntrySelection))` and permanently prevents later Basic selection. A partial registration/selection update is impossible; exact duplicates return the stored state and mismatches fault.

Barrier and no-attempt artifacts carry only the frozen `TutorialLessonBarrierHostScope = NonCourse | CourseSessionBeforeBasicSelection(...) | CourseBasicSelected(the exact TutorialAttemptHostScope arm)`, never `PendingBasicBinding`. `CourseSessionBeforeBasicSelection` is legal only while `attemptPresence = NoAttemptStarted`; once a context exists, the barrier scope must equal that context's `NonCourse` or `CourseBasicSelected` arm. The frozen barrier arm is copied unchanged through presentation-step boundary, quiescence success, and quiescence fault evidence. Attempt materialization, outcome publication, or owner work admission is forbidden until the binding state is Frozen.

### `TutorialAttemptContext`

Every admitted attempt owns:

- `runId`, `segmentId`, `lessonId`, and lesson revision/digest
- exact `TutorialAttemptHostScope`
- globally unique `attemptId`
- one-based `attemptOrdinal` for that lesson in the run
- monotonically increasing `attemptGeneration`
- `tutorialEvaluationSnapshotDigest` and `tutorialPresentationSnapshotDigest`
- observation collector coverage/digest
- P1-A run-active, route-wide scaled rule-window, and pause-independent closure-watchdog clock identities/frequencies
- gameplay-lease ledger, possibly empty
- proof-facing `observationGeneration`
- terminal-only `closureTokenId`
- one terminal compare-and-set

A reviewed retry request creates one inert `RetryAttemptReservation` before old-result publication. It contains a one-time reservation ID, proposed next attempt ID/ordinal/attempt generation/observation generation, lesson/snapshot digests, the exact current `TutorialAttemptHostScope`, and checksum, but has no context, observers, clock starts, gameplay lease, or mutation authority. The old result/continuation CAS consumes that reservation; only after the old attempt closes may the route owner materialize the new `TutorialAttemptContext` under the same host scope. CAS failure, closure fault, course-generation change, or route abort permanently burns the reservation and every proposed identity; none may be reused. Retry never reopens or overwrites an old result.

Cue/opening work captures the attempt generation. Proof observers and ordinary target/projectile events capture `observationGeneration`, which is invalidated at outcome freeze. Outcome freeze activates one attempt-bound `closureTokenId` that only registered producer-drain, reset/transfer, confirmation, presentation-step-boundary, result, and abort work may use. That token cannot record proof and is invalidated when either a closed result or diagnostic abort seals. A foreign/stale token is diagnostics-only and cannot mutate, advance, or publish.

## Observation and Completion Rules

### Observation boundary

`ObservationOpened` is one atomic boundary:

1. validate the current attempt generation and required collector coverage;
2. capture rule baselines such as the current player position;
3. clear only this attempt's buffered observations;
4. record both run-active and rule-window start ticks;
5. enter internal `ObservationOpening`, make the generation gate queue-visible, and keep evaluation closed;
6. allow the current director to release the reviewed inputs. On success, atomically commit `Observing` and drain callbacks raised synchronously by that release against the current generation. On release failure, discard the queue, freeze `Interrupted + InputReleaseFault`, and enter common terminal cleanup without exposing an observation window.

Events and movement before that transaction are excluded. A joystick pointer pressed during Cue may remain held because current input parity permits it; the position baseline is still captured at `ObservationOpened`, so only resulting post-boundary displacement counts.

The minimum observation duration delays the earliest terminal commit, not observation itself. A valid fact received immediately after opening may be buffered and committed only when the minimum duration has elapsed. It must not be discarded merely because it arrived before 0.35 seconds.

### Closed rule vocabulary

| Rule kind | Typed parameters | Required proof |
|---|---|---|
| `MoveDistance` | planar up-axis, positive distance metres, source player binding | finite planar displacement from the observation baseline reaches the threshold |
| `CombatModeEntered` | exact combat mode | generation-scoped mode transition or reviewed current-state observation after opening |
| `BasicAttackHit` | player-side source qualification and target-set ID | qualified hit on a bound target |
| `ProjectileHit` | projectile source/attempt ownership, target-set ID, optional aim-preview requirement | owned projectile plus qualified target result; no unrelated death fallback |
| `DodgeStarted` | player binding only | current behavior parity only; it is not perfect dodge or threat evasion |
| `TargetsCleared` | nonempty required target-set ID and alive predicate version | every admitted target is authoritatively dead/cleared |
| `CorrectSummonAnswer` | qualified semantic adapter/proof ID | explicit adapter proof; never raw summon use |

This is a closed discriminated vocabulary, not a condition DSL. Each newly authoritative kind requires one fixture, exact attribution, coverage validation, stale-generation tests, and semantic review.

### First authoritative rule candidate: `MoveDistance`

- current review threshold: 0.75 metres;
- baseline: player world position at `ObservationOpened`;
- projection: `Vector3.ProjectOnPlane(current - baseline, Vector3.up)` parity behavior;
- success: finite projected magnitude `>= threshold`;
- `RunStarted` may remain a diagnostic input but cannot substitute for displacement when a player binding exists;
- required proof ID is stable and meaning-specific; `space_created` may be a compatibility record only;
- exact numeric serialization and boundary tests must be approved before shadow evaluation becomes authoritative.

The existing director remains authoritative for the first result-adapter phase. A typed evaluator runs in shadow, records mismatches without advancing, then becomes authoritative only after exact-threshold, one-unit-below, frame-rate, pause, held-pointer, and stale-event parity passes.

### Later-rule hazards

- `Melee` currently accepts a broad basic-hit signal, player-side target damage, or an unattributed target death. It needs stable target and source attribution before typed promotion.
- `SwapToRanged` is a small next evaluator candidate but must define whether current ranged state observed after opening is equivalent to a transition.
- `Fire` currently combines aim-preview observation, at least 0.7 scaled seconds held, fire/projectile observation, and damage/death. Its evaluator is not changed while it serves as the first reset fixture.
- `Dodge` currently proves only `DodgeStarted`. It cannot feed P1-D `PerfectDodgeCount` or claim successful attack evasion.
- `ClearTargets` must reject an empty/missing binding and distinguish dead, inactive, despawned, transferred, and unrelated targets before promotion.

## Attempt Lifecycle and Terminal Semantics

The logical states are:

`Validating -> Armed -> Cue -> ObservationOpening -> Observing -> OutcomeFrozen -> OwnershipDraining -> ResettingOrTransferring -> Closed`

Confirmation and next-lesson presentation occur after `OutcomeFrozen` and do not alter the outcome.

1. `Validating` checks snapshot, binding, collector, clock, and reset capabilities without mutating gameplay.
2. `Armed` acquires and captures every declared gameplay lease before the current director performs lesson-owned setup.
3. `Cue` delegates current presentation and input gating to the current director.
4. `ObservationOpening` queues only current-generation facts while the current director releases reviewed inputs; a failed release discards that queue and terminalizes as a system fault.
5. `Observing` admits the queued release callbacks and later current-generation facts only after the atomic boundary commits.
6. One terminal compare-and-set freezes `TutorialAttemptOutcome` as Completed, Failed, Skipped, Cancelled, or Interrupted, invalidates the observation generation, and activates the proof-inert closure token.
7. Under that closure token, close new gameplay-ownership admission into `AdmissionsClosed`, stop each registered producer, and drain every pre-reserved handle to Materialized or Cancelled. Zero pending reservations and a stable exhaustive partition seal the ledger as `DrainSealed`; drain failure transitions directly to `FaultSealed`.
8. The gameplay reset owner executes only the exhaustive terminal dispositions from `DrainSealed`, including any atomic successor transfer. Complete receipts transition to `DispositionSealed`; disposition or transfer failure transitions to `FaultSealed`.
9. The current director seals the prior step's required presentation/input boundary. A successful intermediate lesson may transfer its course-scoped ownership into the exact next step; cancel, interruption, or final route exit must take the full common cleanup path.
10. After the gameplay disposition and presentation step-boundary receipts close, prepare exactly one immutable `TutorialAttemptContinuationSelection`: advance once, terminate/suppress the route, or consume one inert `RetryAttemptReservation` only for `Cancelled + RetryRequested`. Every attempt owns a distinct `attemptResultPublicationRow = Open | Published(attemptContinuationSelectionId/digest, tutorialAttemptResultId/digest) | RestartSuppressed(...)`. One route-owner compare-and-set publishes the immutable `TutorialAttemptResult` and selection together by comparing that per-attempt row plus `(terminalOrRestartLatch=Open, restartArbitrationVersion=v)`. An intermediate `AdvanceLegacyLesson` or `RetryAttempt` publication consumes only its attempt row; it never reads or changes `basicClosePublicationLatch` or `nonCourseRouteClosePublicationLatch`. In that same publication transaction it creates one route-owned `AttemptContinuationConsumptionRow` with a runtime-issued row ID and state `Unconsumed(exact selection ID/digest, exact result ID/digest, exact RetryAttemptReservation ID/digest or typed absence)`. A concurrent accepted active restart that wins before publication changes the global version first, so the normal per-attempt CAS fails and the restart path may seal only `RestartSuppressed`. Exact duplicates return the stored attempt row and consumption row; mismatches fault.

`AttemptContinuationConsumptionRow` has the state union `Unconsumed(...) | Consumed(successor kind, exact successor lesson/binding identity and generation, successorContext = InstrumentedAttemptMaterialized(exact attempt/context IDs and generations) | LegacyOpaqueActivated(exact current-director course-lease ID/generation, typed absence of P1-E attempt/context identity), consumption sequence) | SuppressedByActiveRunRestart(exact dispatch ID/canonical digest, exact original selection/result IDs and digests, exact burned RetryAttemptReservation ID/digest or typed absence, suppression sequence)`. Its canonical terminal digest covers the row ID, complete state arm, exact refs/typed absence, and sequence while excluding its envelope checksum. Normal intermediate dispatch compares the same global restart version and atomically changes `Unconsumed -> Consumed` **in the same route-owner transaction that materializes or advances the named successor**. `RetryAttempt` must use `InstrumentedAttemptMaterialized`; `AdvanceLegacyLesson` selects the arm dictated by the snapshotted successor capability. Active-restart acceptance may instead atomically change `Unconsumed -> SuppressedByActiveRunRestart`; for Retry it permanently burns the reservation and proves that no successor attempt context was materialized. If `Consumed(InstrumentedAttemptMaterialized)` won first, restart closes that current successor attempt normally. If `Consumed(LegacyOpaqueActivated)` won, the prior immutable result remains the only P1-E attempt result and the barrier must instead close the named legacy successor's course lease/work through the exact typed continuation-closure arm below. Thus there is no published-result-to-successor gap and no legacy interval that can fabricate an empty attempt.

P1-A's global restart arbitration, every P1-E barrier-close publication latch, and every published `Unconsumed` intermediate continuation row live in the same route-owner serialized aggregate. Only a barrier-closing normal arm additionally compares its applicable local close latch as Open. Active-restart acceptance compares the same global version and atomically changes the global latch to its restart winner, every currently open applicable barrier-close latch to `RestartReserved(dispatch ID/digest, v+1)`, and every published `Unconsumed` intermediate row to `SuppressedByActiveRunRestart` before returning acceptance. Therefore no final normal barrier-close CAS or intermediate successor materialization can succeed after restart acceptance. If normal barrier close won first, the later restart records typed receipt reuse. If intermediate consumption won first, restart either closes the exact materialized instrumented successor or drains the exact `LegacyOpaqueActivated` successor and proves that closure without rewriting prior truth.

For a P2-B course Basic, `basicClosePublicationLatch` is consumed only by final `AdvanceToCourseEntry` or active-run-restart closure, never by an attempt retry. `NormalBasicClose` compares the per-attempt row, global version, and Basic close latch, then seals `AdvanceToCourseEntry`, the result, and the Basic-authorized `TutorialLessonQuiescenceReceipt` atomically. For the final NonCourse lesson, `nonCourseRouteClosePublicationLatch` is consumed only by final `TerminateRoute` or active-run-restart closure; `NormalNonCourseRouteClose` atomically seals the final selection, result, P1-A `NonCourseRouteContinuationAuthority`, and NonCourse-authorized receipt. `ActiveRestartClose` requires the exact globally sealed dispatch and applicable local `RestartReserved` arm. If it wins before result publication, it atomically seals the current attempt row as `RestartSuppressed`, embeds `SuppressedByActiveRunRestart` in the new result when a context exists, and seals the restart-authorized receipt. If an intermediate result was already published but its continuation remained `Unconsumed`, restart preserves that immutable result, seals the separate consumption row as suppressed, burns any Retry reservation, and the restart-authorized receipt carries exact prior-result plus suppression-row evidence. If `Consumed(LegacyOpaqueActivated)` already won, restart invalidates that exact successor generation, fully releases its named course lease and zeroes registered work, and carries the prior result plus `ConsumedIntermediateSuccessorClosed` evidence; it never fabricates a successor attempt result. If either final normal close won first, the later accepted restart reuses that already immutable successful receipt; P2-B suppresses an unconsumed Basic course transition, while P1-A prevents or closes the not-yet-consumed NonCourse segment handoff. P1-E never rewrites any result. The legacy director advance is disabled, and no owner may advance the same result twice. This is per-attempt quiescence, not a requirement to release the director's course-wide lease between successful legacy lessons. If preparation or publication fails, burn any reservation, retain the frozen outcome only in diagnostic closure-fault evidence, publish no closed result, and do not continue.
11. Dispatch only through the matching `AttemptContinuationConsumptionRow` compare-and-set. Successful consumption and successor materialization/advance are one route-owner transaction. A dispatch or later scene-load failure does not erase the already closed attempt result; it seals the ordinary P1-A route abort, exposes no product summary, and cannot select a different continuation.

For a successful current lesson, the semantic proof freezes at the existing commit point. Confirmation UI can finish later. If producer drain, cleanup, transfer, or the presentation step boundary fails before result publication, the outcome remains Completed but no closed result exists. If the already sealed continuation later fails to dispatch, the closed result remains only inside the aborted run context and never becomes a product summary.

### Terminal mapping

| Trigger | Attempt outcome | Route consequence in first slice |
|---|---|---|
| qualified proof | `Completed` | confirm, close owned state, then advance once |
| explicit learner failure rule | `Failed` | unsupported until a rule is reviewed; never stage Fail automatically |
| reviewed lesson skip | `Skipped` | unsupported; no proof or progress |
| reviewed retry request before outcome freeze | `Cancelled + RetryRequested` | reserve an inert one-time identity record, close/reset and publish the old attempt once, then materialize the new attempt context from that consumed reservation |
| accepted P1-A active-run restart before outcome freeze | `Cancelled + ActiveRunRestartRequested` | create no attempt-retry reservation; after full common cleanup, atomically seal `SuppressedByActiveRunRestart` plus the closed attempt result, dispatch no old continuation, and return the P1-E quiescence receipt |
| accepted active-run restart after outcome freeze | retain the already frozen outcome | finish closure without dispatching its prepared continuation; report the closed result or closure fault to the aborting run only |
| accepted active-run restart before any attempt context exists | no learner outcome; typed `NoAttemptStarted` disposition | release any acquired current-director course lease/work and return the P1-E quiescence receipt |
| explicit owner cancellation | `Cancelled` | common cleanup, no next lesson, diagnostic route termination if route ends |
| owner disable/destroy, scene unload, route replacement | `Interrupted` | common cleanup, no completion/handoff |
| missing binding/collector or bad snapshot/clock before outcome freeze | `Interrupted + SystemFault` | block next lesson and Station load; diagnostic abort |
| attempt-closure fault before result publication | no second outcome; retain the first outcome in closure-fault evidence | publish no closed result, block next lesson and Station load |
| sealed continuation/scene-load failure after result publication | no outcome/result rewrite | preserve the closed result only in the aborted run context; no product summary or alternate continuation |
| missing director before outcome freeze | `Interrupted + OwnerMissing` | never infer completion |

Tutorial Failed, Skipped, Cancelled, or Interrupted is not Station combat Fail. If it ends the playable route before a product outcome, P1-A records a diagnostic abort and emits no `RunResultSummary`.

## Clock Contract

P1-E uses three fixed route-level clocks with different named responsibilities:

1. `RunActiveClock` is the same P1-A/P1-D activity-gated monotonic real/unscaled clock and stable per-run frequency. It starts this attempt's measurement at `ObservationOpened`, ends at outcome CAS, and converts once to integer `observationElapsedMilliseconds` with P1-A's overflow-safe rule. This is the only elapsed value stored in the lesson fact.
2. `TutorialRuleWindowClockV1` is one injected, monotonic nondecreasing scaled-gameplay clock with one stable frequency for the whole run. It preserves the current 350-millisecond scaled earliest-commit behavior: it stops at time scale zero and accrues proportionally at other time scales. An explicit pause may stop it only when the same pause gate also suspends the current director's phase update; otherwise P1-E admission fails. Authoring stores integer `minimumObservationRuleMilliseconds`, and checked integer arithmetic converts it once to ticks as `ceil(milliseconds * frequency / 1000)`.
3. `TutorialClosureWatchdogClockV1` is a route-owned monotonic real clock with one stable frequency that remains available after outcome CAS and ignores gameplay time scale and explicit player pause. It measures only producer drain, reset/transfer, presentation-step-boundary, and result-publication closure against positive integer `cleanupDeadlineMilliseconds`. It never contributes to proof, lesson elapsed, mastery, or player-facing timing.

All three frequencies come from the run snapshot, not from individual attempts. Regression, frequency change, negative delta, or conversion overflow in a proof/result clock is a system fault before outcome publication. A watchdog failure or deadline expiration after outcome freeze immediately fault-seals diagnostic closure evidence. Rule-clock start tick, end tick, inherited frequency reference, and elapsed ticks are outcome audit fields. The rule clock only delays the earliest commit; it does not discard observations received before that gate opens. No lesson may choose a different scaled/unscaled policy in the first slice.

P1-A does not infer pause from time scale. Therefore time scale zero without the explicit route activity-pause gate stops the rule window but continues `RunActiveClock`; both values make that distinction auditable. Only the explicit shared pause gate stops result elapsed and, after it also suspends the director phase update, the rule window.

The watchdog starts when outcome CAS activates the closure token and stops when result/continuation publication or diagnostic abort seals. Checked integer arithmetic converts its deadline once to ticks. Application suspension may cause the monotonic deadline to be exceeded on resume; that is a closure timeout, not learner failure. If the watchdog itself becomes unavailable, the owner fault-seals immediately rather than waiting indefinitely.

The compatibility adapter may continue reading the current director's scaled phase timer while the director remains authoritative. Before typed `MoveDistance` promotion, the injected rule clock must match it in shadow at time scales 0, 0.5, 1, and 2. Cue and confirmation remain current scaled presentation behavior, while prompt repeat remains unscaled presentation behavior. Voice length and overlay animation never become proof. Direct reads of `Time.time`, `Time.unscaledTime`, frame count, or independently accumulated float seconds are not result authority.

## Typed Outcome and Result

### `TutorialAttemptOutcome`

Frozen by the terminal CAS:

- schema version
- runtime-issued `attemptOutcomeId`
- run/stage/route/segment/plan identity and semantic digests
- exact `TutorialAttemptHostScope`, matching the admitted context
- lesson ID/revision and `tutorialEvaluationSnapshotDigest`
- attempt ID/ordinal, attempt generation, observation generation, and closure-token identity
- terminal outcome and closed termination reason
- integrity status
- exact `proofDisposition = Proved(stable proof ID, discriminated proof value kind/units, QualifiedProofAttribution) | NoProof(explicit reason, typed absence of proof ID/value/attribution)`
- collector coverage/digest
- observation elapsed milliseconds
- rule-window clock identity, inherited frequency reference, start/end ticks, and elapsed ticks at terminal CAS
- terminal event sequence
- canonical `attemptOutcomeDigest` and envelope checksum

`QualifiedProofAttribution` contains the exact qualified source ID and `observationProvenance = (attemptId, observationGeneration, collectorCapabilityId, collectorCoverageDigest)` plus `targetCoverage = Required(target-set ID) | NotRequiredByProofDefinition`; it does not invent a separate observation ID. The selected arm must match the snapshotted proof definition and the outcome's exact attempt/generation/collector coverage. Revision 1 permits `Completed` if and only if `proofDisposition = Proved`. `Failed`, `Skipped`, `Cancelled`, and `Interrupted` require `NoProof`; an accepted active restart after outcome freeze preserves whichever already immutable arm won and never rewrites it. No Unity object, scene path, component reference, display text, icon, voice, HUD target, or mutable asset is serializable into this object.

`attemptOutcomeDigest` covers the runtime outcome ID, run/plan/lesson/attempt identities and generations, the complete host-scope arm, terminal outcome/reason/integrity, the complete proof-disposition and target-coverage arms including typed absences, collector coverage, observation/rule-clock facts, and terminal sequence. It excludes presentation-only metadata and the envelope checksum.

### `TutorialAttemptResult`

Published only after gameplay disposition and the current presentation owner reach their required boundary:

- runtime-issued `attemptResultId`
- immutable `TutorialAttemptOutcome`
- exact `TutorialAttemptHostScope`, matching the outcome/context
- exact `TutorialGameplayDispositionReceipt` ID/`gameplayDispositionDigest`
- exact `TutorialPresentationStepBoundaryReceipt` ID/`presentationStepBoundaryDigest` and its transferred/fully-released disposition
- exact `TutorialAttemptContinuationSelection` ID and canonical digest sealed by the same compare-and-set
- closure-watchdog identity and elapsed milliseconds
- exact-close event sequence
- canonical `attemptResultDigest`
- result checksum

The gameplay receipt identifies each acquired entry as Restored, DestroyedOwned, Preserved, Transferred, or ReleasedNoMutation. `attemptResultDigest` covers the attempt-result ID, `attemptOutcomeDigest`, the exact matching host-scope arm, gameplay-disposition receipt digest, presentation/course-ownership boundary disposition and receipt digest, exact continuation-selection ID/digest, watchdog identity/elapsed value, and close sequence. It excludes display copy/media, constituent checksums, and the result-envelope checksum. This is diagnostic lifecycle evidence, not a reward or mastery result. If a required receipt or presentation step boundary fails, this closed result does not exist; the immutable outcome and partial closure evidence belong only to the route's diagnostic abort record.

The route context appends by full attempt identity and semantic digest. An exact duplicate returns the existing row; a duplicate identity with different outcome, proof, digest, or disposition is an integrity fault. Result order is the plan order plus attempt ordinal, never callback order.

### `TutorialAttemptContinuationSelection`

- runtime-issued `attemptContinuationSelectionId`;
- run/plan/lesson/attempt identities and current generations;
- exact `TutorialAttemptHostScope`, matching the attempt outcome;
- exact attempt-outcome semantic digest;
- `continuationKind`: `AdvanceLegacyLesson`, `AdvanceToCourseEntry`, `RetryAttempt`, `TerminateRoute`, or `SuppressedByActiveRunRestart`;
- exactly one compatible target: successor lesson ID, successor course-entry ID, consumed retry-reservation ID, or typed `None` for terminal/suppressed kinds;
- monotonic selection sequence;
- canonical `attemptContinuationSelectionDigest` and envelope checksum.

The continuation digest covers its own selection ID, run/plan/lesson/attempt identities and generations, the exact matching host-scope arm, exact outcome digest, continuation kind, compatible target or typed absence, and selection sequence. It excludes the later result digest and its envelope checksum, so the same compare-and-set can seal this selection first and publish a result that references it without a cycle. `SuppressedByActiveRunRestart` records that no old continuation may dispatch; it is not restart authority. A course Basic transition accepts only `AdvanceToCourseEntry` naming the exact snapshotted Practice entry and carrying the same `CourseBasicSelected` arm. Duplicate or mismatched selection attempts are reject/fault and cannot redirect the already closed result.

### `TutorialAttemptClosureFaultEvidence`

When an admitted attempt context exists but pre-outcome cancellation/terminalization, producer drain, gameplay disposition, presentation step boundary, continuation preparation, or result publication fails, P1-E creates one scene-reference-free diagnostic attachment:

- runtime-issued `attemptClosureFaultEvidenceId`;
- run/plan/lesson/attempt identity, attempt and observation generations, and exact `TutorialAttemptHostScope`;
- closed `outcomePresence = NotFrozenBeforeClose | Frozen(TutorialAttemptOutcome ID, attemptOutcomeDigest)`;
- closed `closureAuthority = AttemptGenerationInvalidated(close reason, invalidation sequence) | FrozenOutcomeClosureToken(closureTokenId)`; the latter is required exactly for `Frozen`;
- closed failed-boundary kind and fault reason;
- closed watchdog arm `NotStartedBeforeOutcome | Started(identity, frequency, start/end ticks, elapsed ticks, deadline)`;
- closed gameplay-ledger evidence union `DispositionSealed(TutorialGameplayDispositionReceipt ID, gameplayDispositionDigest) | FaultSealed(TutorialGameplayFaultSnapshot ID, tutorialGameplayFaultDigest) | PreDispositionCloseFault(TutorialGameplayPreDispositionCloseSnapshot ID, tutorialGameplayPreDispositionCloseDigest)`, plus pending reserved-handle rows ordered by reservation sequence with stable handle ID as uniqueness check;
- three fixed partial-closure slots in gameplay-disposition, presentation-step-boundary, continuation-selection order: `NotReached | Succeeded(exact declared receipt/selection type, runtime ID, canonical digest) | FailedAtThisBoundary`;
- fault sequence;
- canonical `attemptClosureFaultDigest` and diagnostic envelope checksum.

`attemptClosureFaultDigest` covers the fault-evidence ID, attempt/course provenance, outcome-presence and closure-authority arms, generations/token or typed absence, failed boundary/reason, watchdog arm, the exact gameplay-ledger evidence type/ID/digest, reservation-ordered pending handle rows, all three fixed partial-closure slots including typed absence, and fault sequence; it excludes presentation-only metadata and every envelope checksum. The three success arms are respectively limited to `TutorialGameplayDispositionReceipt`, `TutorialPresentationStepBoundaryReceipt`, and `TutorialAttemptContinuationSelection`. `FailedAtThisBoundary` carries no anonymous nested evidence: at most one slot may use it, it must agree with the parent failed-boundary kind/reason/sequence, every earlier required slot is `Succeeded`, and every later slot is `NotReached`. A producer-drain, pre-disposition close, or result-publication failure outside those three named slots instead leaves the slots at their truthful succeeded/not-reached states and is identified only by the parent failed-boundary fields plus the exact gameplay-ledger evidence union. `NotFrozenBeforeClose` cannot carry learner outcome, closure token, or started post-outcome watchdog and requires the `PreDispositionCloseFault` ledger arm; `Frozen` requires either `DispositionSealed` or `FaultSealed`. P1-A embeds this attachment only in `StageRunAbortRecord`. It is not a closed attempt result or `TutorialAttemptFact`, cannot advance, and cannot feed mastery, progress, rewards, or UI success. Exact duplicate closure faults return the first attachment; mismatched duplicates are integrity faults.

The gameplay-ledger union and first partial slot cannot name independent receipts. `DispositionSealed` is legal if and only if the gameplay-disposition slot is `Succeeded`, and both must carry the exact same `TutorialGameplayDispositionReceipt` ID/canonical digest. `FaultSealed` or `PreDispositionCloseFault` forbids a succeeded gameplay-disposition slot. Any substitution or arm disagreement is a hard integrity fault before the closure-fault digest seals.

## Gameplay Reset Ownership

### `TutorialGameplayResetPolicyDefinition`

- stable policy ID, revision, and semantic digest
- required stable gameplay-binding IDs
- closed owned-domain entries
- capture schema/version for each domain
- dispositions for Completed, Failed, Skipped, Cancelled, Interrupted, and retry
- optional exact successor lesson ID for `TransferToCourseForNext`
- required cleanup capabilities
- positive integer `cleanupDeadlineMilliseconds` and closed fault policy

Allowed gameplay domains are bounded:

- source-owned temporary buffs/debuffs;
- attempt-owned projectiles;
- attempt-owned spawned or summoned units;
- admitted target pose, active/collider/targetable state, health, and AI state;
- explicitly temporary loadout state.

The policy never owns time scale, input locks, movement gates, prompt/focus UI, HUD, camera, audio presentation, route blockers, or cinematic state during P1-E. Those remain with the current director or route owner. P2-B may acquire a presentation domain only after its own parity gate; two owners never restore the same domain.

### `TutorialGameplayLeaseLedger`

Every mutable domain is captured before mutation:

- domain kind and stable binding ID;
- attempt ID/generation, exact `TutorialAttemptHostScope`, and acquisition sequence;
- prior-state snapshot and digest;
- owned runtime handle IDs;
- expected current ownership/source revision;
- terminal dispositions;
- release state and receipt.

Reset uses only ledger entries. Global object scans, tag-wide deletes, blanket buff removal, and clearing all projectiles or summons are forbidden. If ownership or expected revision no longer matches, cleanup faults instead of overwriting unrelated state.

Before any asynchronous producer starts or any projectile/spawn becomes live or externally observable, it atomically reserves one ledger handle with producer ID, reservation sequence, and expected binding. The producer may materialize only into that reservation; start failure marks it Cancelled. An unreserved producer or live object is an integrity fault before exposure. At outcome freeze, new reservations close; registered producers stop, and materialization/cancellation callbacks may finish only their existing reservations under the closure token until pending reservations reach zero. Captured policy domains receive immutable `ledgerOrdinal` values from the snapshotted policy-domain order before mutation; reservations receive a monotonic `reservationSequence` under the serialized ledger admission. Every canonical ledger/disposition coverage array orders captured entries by `ledgerOrdinal` and reservations by `reservationSequence`, with stable binding/handle ID as a uniqueness check, never by materialization, cleanup, or callback completion time.

Outcome freeze invalidates the observation generation before cleanup but keeps the proof-inert closure token valid. Late spawn/fire/damage/death callbacks cannot record proof or create reservations; they may only finish an already reserved entry during drain. Cleanup is idempotent per ledger entry; a duplicate terminal signal returns the first receipt. Every captured field and owned handle must appear in an exhaustive terminal partition as Restored, DestroyedOwned, Preserved, Transferred, or ReleasedNoMutation; an omitted field faults.

The ledger has two pre-disposition states and two terminal evidence states:

- `AdmissionsClosed` rejects every new reservation while already registered producers drain under the closure token. It cannot authorize disposition, result publication, or advance.
- `DrainSealed` requires zero pending reservations and a stable exhaustive partition of every captured field/handle. It is the only state from which reset/transfer dispositions may execute, but it cannot support a closed result.
- `DispositionSealed` requires zero pending reservations and one complete disposition receipt for every captured field/handle. Only this state may support a closed attempt result.
- `FaultSealed` is a diagnostic snapshot used when producer drain, watchdog, cleanup, or transfer cannot reach `DispositionSealed`. It records the closure-token/watchdog identity and elapsed ticks, fault reason, every acquired entry and current ownership state, every reservation's producer/materialized/cancelled/pending state, pending handle IDs, available partial receipts, and a checksum/digest. Pending entries are allowed here.

`FaultSealed` never counts as successful cleanup and cannot transfer, publish a result, or advance. It freezes evidence immediately when the watchdog deadline expires or the watchdog/producer fails, so diagnostic abort construction does not depend on a successful drain. Best-effort release may continue for safety, but it cannot rewrite the snapshot or reopen continuation.

### `TutorialGameplayFaultSnapshot`

The `FaultSealed` state is represented by one runtime-issued `tutorialGameplayFaultSnapshotId`. The snapshot contains the run/lesson/attempt and attempt/observation generations, exact `TutorialAttemptHostScope`, exact frozen-outcome and closure-token provenance, watchdog identity/ticks/deadline, fault reason, every acquired policy-domain row in immutable `ledgerOrdinal` order, every reservation row in `reservationSequence` order, each row's captured/current ownership and materialized/cancelled/pending state, reservation-ordered pending handle IDs or a typed empty set, available partial-disposition rows in the same canonical entry/reservation order, fault-seal sequence, canonical `tutorialGameplayFaultDigest`, and envelope checksum. The canonical digest covers the snapshot ID, all identity/generation/host-scope/token/watchdog/fault fields, the complete ordered entry/reservation coverage, pending rows including typed empty, partial-disposition rows including typed absence, and fault-seal sequence; it excludes every envelope checksum. A later safety release cannot replace its ID or digest.

### `TutorialGameplayPreDispositionCloseSnapshot`

If an admitted attempt context must close before a `TutorialAttemptOutcome` and therefore has no frozen-outcome closure token, one runtime-issued `tutorialGameplayPreDispositionCloseSnapshotId` captures `ledgerLifecycleState = ContextAdmittedNoAcquisition | AcquisitionInProgress | AdmissionsOpen`, the exact `TutorialAttemptHostScope`, the exact `AttemptGenerationInvalidated` authority, every acquired policy-domain row in immutable `ledgerOrdinal` order, every reservation row in `reservationSequence` order, captured/current ownership plus materialized/cancelled/pending state, reservation-ordered pending handle IDs or a typed empty set, available partial-disposition rows or typed empty, close sequence, canonical `tutorialGameplayPreDispositionCloseDigest`, and envelope checksum. The canonical digest covers the snapshot ID, run/lesson/attempt identities and generations, complete host-scope arm, lifecycle state, invalidation authority, all ordered acquired/reservation coverage, pending rows including typed empty, partial rows including typed empty, and close sequence; it excludes every envelope checksum. `AdmissionsClosed`, `DrainSealed`, `DispositionSealed`, and `FaultSealed` require the post-outcome closure-token path and are invalid lifecycle arms for this snapshot; it cannot authorize result publication, transfer, continuation, mastery, or progress.

### `TutorialGameplayDispositionReceipt`

- runtime-issued receipt ID;
- run/lesson/attempt, attempt/observation generations, exact `TutorialAttemptHostScope`, and closure-token identity;
- terminal outcome/reason;
- canonical ordered coverage of every ledger entry and reservation with stable binding/handle IDs, captured-state digest, materialized/cancelled state, terminal disposition, final-state/ownership digest, and terminal sequence;
- zero pending reservations, final ledger state `DispositionSealed`, and exact `transferCoverage = NoTransfer(typed absence of intent and commit) | TransferCommitted(TutorialGameplayTransferIntent ID/canonical digest, TutorialGameplayTransferCommitReceipt ID/canonical digest)`;
- canonical `gameplayDispositionDigest` and envelope checksum.

`gameplayDispositionDigest` covers the receipt/attempt/token provenance, complete host-scope arm, outcome/reason, exhaustive canonical entry/reservation coverage including the typed empty `ReleasedNoMutation` case, zero-pending/final-state facts, and the complete transfer-coverage arm including typed absence. `TransferCommitted` is required if and only if one or more ledger rows have terminal disposition `Transferred`; revision 1 permits exactly one intent/commit pair, whose commit receipt must name the exact intent and the identical transferred field/handle set. `NoTransfer` requires zero `Transferred` rows and forbids foreign transfer provenance. The digest excludes presentation-only metadata and every envelope checksum. `FaultSealed` cannot produce this receipt.

The first-slice `TransferToCourseForNext` prepares one `TutorialGameplayTransferIntent` containing runtime-issued intent ID, source attempt ID/generation, exact `TutorialAttemptHostScope`, destination `tutorialCourseGameplayLeaseId` and generation, exact successor lesson ID, plan/binding digests, exact field/handle set, transferred-state digest, canonical `gameplayTransferIntentDigest`, and envelope checksum. The intent digest covers those exact fields and excludes its checksum. The course lease validates the intent in non-owning `Prepared` state. One route-owner compare-and-set then atomically changes every source entry from SourceOwned to Transferred and the destination from PreparedNonOwning to CourseOwned; there is no dual-ownership interval. Success seals `TutorialGameplayTransferCommitReceipt` with runtime receipt ID, intent ID/digest, identical host-scope arm, source/destination identities, committed field/handle digest, commit sequence, canonical `gameplayTransferCommitDigest`, and envelope checksum; its canonical digest covers the complete host-scope arm and excludes every envelope checksum. Failure to prepare or commit leaves the source entries owned for fault cleanup, preserves the frozen outcome, and blocks advance. Anonymous carry-over and direct attempt-to-attempt transfer remain forbidden until the successor lesson is itself instrumented.

### Move no-op reset

Move acquires no gameplay mutation lease. Its policy disposition is `NoGameplayMutation`, its position effect is `PreservePlayerPosition`, and its successful gameplay receipt is `ReleasedNoMutation`. The player's intentional displacement is not rolled back. Held-pointer, input, and tutorial-forced facing remain current-director course domains; the extracted reset policy must not clear, unlock, or restore them.

This no-op is required for parity but is not evidence for target, projectile, buff, summon, AI, or loadout reset.

### Fire first real reset fixture

Fire may enter reset implementation only after all of the following exist:

- stable Fire lesson/attempt/proof/reset IDs and entry snapshot;
- a source-scoped registry for every player projectile admitted to the attempt generation;
- stable target binding and capture of pose, health, active/collider/targetable, and AI state;
- a reviewed boundary between current-director aim/input cleanup and gameplay reset;
- an explicit natural-success disposition for every still-live owned projectile handle;
- exact success transfer fields, exact successor lesson ID, and stable current-director course-lease identity/generation;
- no global projectile, target, or behavior scan.

Terminal dispositions for the candidate fixture are:

| Terminal | Owned projectile disposition | Target disposition | Advance |
|---|---|---|---|
| Completed | explicitly destroy or transfer each still-live owned handle according to the reviewed parity disposition; unresolved carry-over faults | for every captured field, either return its reviewed current value or restore its entry value, then atomically return the complete target-domain ownership to the exact course lease for the named following lesson | once, after both per-attempt boundaries close |
| retry | destroy owned handles | `RestoreThenReturnToCourse`: restore every captured entry value and atomically return complete ownership; the new attempt re-acquires only after the old result closes | no old-attempt advance |
| Cancelled | destroy owned handles | `RestoreThenReturnToCourse`, then let the course terminal path restore/release its own baseline | no |
| Interrupted | destroy owned handles where safe; missing scene objects do not skip remaining releases | `RestoreThenReturnToCourse` where bindings remain; inability to restore or return any field is a closure fault | no |
| cleanup fault | retain diagnostic ownership evidence | no silent partial transfer | blocked |

The current Fire completion rule remains delegated during this reset slice. The fixture proves reset/transfer ownership, not a new Fire evaluator. `Dodge` is not the first reset fixture because current enemy AI/projectile ownership and source-scoped invulnerability are less mature.

## Course-Scoped Current-Director Cleanup

Here, `course-scoped` means only the lifetime of one existing current-director tutorial sequence. It is a runtime ownership scope, not the deferred authored `TutorialCourse` or persistent TutorialProgress framework.

The current director retains one course-scoped lease for domains acquired at tutorial start and carried across legacy lessons. It captures exact prior state before its first mutation. The known omissions in `CancelTutorial()` are defects to close, not parity behavior to preserve.

`TutorialCourseOwnershipSnapshot` records the run/segment/plan digests, stable `tutorialCourseGameplayLeaseId` and generation, every stable domain binding and captured prior-state digest, source-scoped acquisition tokens, per-terminal disposition, outstanding lesson-loan IDs, and checksum. It is captured before `BeginTutorial()` mutates any course domain. No final result or route handoff may seal while a loan remains outstanding or the course receipt is incomplete.

### `TutorialPresentationStepBoundaryReceipt`

- runtime-issued receipt ID;
- run/segment/plan, exact `TutorialLessonBarrierHostScope`, and one closed `attemptBoundary = NoAttemptStarted(typed absence of attempt ID/ordinal, attempt/observation generations, outcome, and closure token) | AttemptContextExisted(attemptId, attemptOrdinal, attemptGeneration, observationGeneration, exact TutorialAttemptOutcome ID/canonical digest, closureTokenId)`;
- current-director course-lease ID/generation;
- boundary disposition `TransferredToLegacySuccessor | FullyReleased` and exact successor identity or typed absence;
- canonical ordered domain coverage with stable domain IDs, captured-state digest, restore/release/transfer disposition, final-state/ownership digest, and terminal sequence;
- outstanding lesson-loan IDs and count; `FullyReleased` requires zero;
- zero current-director presentation timers/observers/callbacks/handles retained for the closed boundary;
- close sequence, canonical `presentationStepBoundaryDigest`, and envelope checksum.

`presentationStepBoundaryDigest` covers the receipt/run, complete barrier-host-scope arm, complete attempt-boundary arm including every typed absence or exact outcome/token ref, lease provenance, boundary disposition/successor, exact domain captured/final-state/disposition coverage, loan identities/count, zero-work facts, and close sequence. `NoAttemptStarted` permits `NonCourse`, `CourseSessionBeforeBasicSelection`, or pre-context `CourseBasicSelected`; `AttemptContextExisted` requires the barrier host scope to equal the named attempt context/outcome host scope. Course-lease domain rows retain the immutable `TutorialCourseOwnershipSnapshot` domain order, with stable domain ID as a uniqueness check; lesson-loan rows use their snapshotted loan ordinal. Neither array uses release/callback order. The digest excludes presentation-only copy/media and every envelope checksum. A P2-B course Basic accepts only `FullyReleased`; legacy intermediate lessons may use the transfer arm.

| Domain | P1-E owner | Required success/terminal behavior |
|---|---|---|
| sequence timers, observers, and queued callbacks | current director plus attempt adapter | step success transfers only registered course work; cancel/disable/unload invalidates the proper token, stops timers, and detaches or generation-gates every callback |
| movement/action/combat-mode gates and controller enabled states | current director course lease using source-scoped tokens or exact captured state | successful steps transfer reviewed gates; terminal exit restores the prior state without releasing another owner's lock |
| prompt, overlay, focus, dialogue/audio, and aim-preview presentation | current director course lease | step boundary seals/transfers once; terminal exit hides/stops/releases every acquired presentation resource once |
| route blockers and bounds roots | current director/route course lease | capture prior enabled/active state; normal final handoff applies the explicit route-exit disposition, while cancel/interruption restores the captured state |
| target candidates and hard lock | current director course lease | remove only tutorial-owned candidates/lock and restore the captured prior selection state on terminal exit |
| target root/collider/targetable, pose, health, and AI fields | current director course gameplay lease except fields atomically loaned to an extracted lesson ledger | capture the course baseline; legacy paths use an explicit final/cancel disposition, and Fire returns its reviewed success fields atomically to this lease |
| player combat mode | current director course lease | transfer the reviewed mode between successful steps; restore the captured pre-course mode on cancel/interruption |
| player position | explicit first-route policy `PreservePlayerPosition` | preserve intentional movement because no current setup lease repositions the player; future repositioning requires a separate captured gameplay lease |
| player facing | current director course lease | capture facing before `RequestFacingDirection`; transfer reviewed facing between successful intermediate steps, then restore the captured facing on cancel, interruption, or final route exit before the final attempt result publishes |
| tutorial invulnerability | source-scoped course lease | refresh only its own token and revoke that exact token on every terminal path |
| action enabled states, boss telegraphs, and enemy behavior enabled states | current director course lease | capture and restore every individual prior value; never assume the terminal value is false or true |

When a lesson gameplay ledger temporarily owns a target/projectile/buff/spawn/loadout domain, the director may invoke existing mutation methods only as an actuator through a `TutorialGameplayMutationPort` carrying the current owner token. The port reserves or records the ledger entry before mutation. The director has no independent authority over that domain until an atomic transfer returns it to the course lease. Domains not yet extracted remain solely course-owned.

Flow-owner disable, director disable/destroy, and scene unload must converge on this course terminal path. Missing scene-local objects do not excuse release of still-reachable global/source-scoped tokens. Cleanup failure seals the P1-E closure-fault attachment and blocks continuation.

### `NonCourseRouteContinuationAuthority`

P1-A is the sole issuer of this pre-load authority. It contains a runtime-issued authority ID; run/playable-stage/route revision and final route digest; exact source Corridor segment identity and expected destination Station segment identity from the immutable route snapshot; the exact final NonCourse `TutorialAttemptResult` ID/canonical digest; the identical embedded `TutorialAttemptContinuationSelection` ID/canonical digest, whose host scope is `NonCourse`, kind is `TerminateRoute`, and target arm is typed `None`; the `nonCourseRouteClosePublicationLatch` identity/winner; authority sequence; canonical `nonCourseRouteContinuationAuthorityDigest`; and envelope checksum. Its digest covers those semantic fields and typed absence while excluding the envelope checksum.

This authority permits only the final NonCourse P1-E barrier and later P1-A pre-load validation. It is not a `StageSegmentTransitionToken`, scene-load command, course transition, outcome, or result action. The route-owner compare-and-set above computes the immutable result/selection first in the digest DAG and atomically seals this authority plus the successful lesson receipt only when `NormalNonCourseRouteClose` wins. An accepted restart winner creates no such authority. Exact duplicate publication returns the byte-identical stored authority/result/receipt set; a mismatch is an integrity fault.

### `TutorialLessonCloseAuthority`

Every first publication of the barrier freezes exactly one authority arm: `NonCourseRouteContinuation(NonCourseRouteContinuationAuthority ID/canonical digest) | BasicTransitionContinuation(TutorialAttemptContinuationSelection ID/canonical digest, exact authorizing Basic CourseEntrySelection ID/canonical digest) | ResolvedActiveRunRestartDispatch(ID/canonical digest) | StageRunAbortCloseAuthority(ID/canonical digest)`. The NonCourse arm is legal only for barrier host scope `NonCourse`, a final `ClosedAttempt`, and the exact authority's final `TerminateRoute` selection/result. The Basic arm is legal only for the same current Basic selection and `AdvanceToCourseEntry` continuation. Active restart and diagnostic abort must use their exact P1-A records; disable/destroy/unload may invalidate the local generation and begin fail-safe drain immediately, but cannot seal success until `StageRunAbortCloseAuthority` arrives. A failure before that authority arrives records the fault-only arm `AuthorityUnavailable(expected P1-A authority kind, local invalidation reason/sequence)` and never satisfies the barrier. Challenge terminal finalization does not close P1-E again; P1-A only revalidates the already sealed Basic-transition receipt. A later active restart also reuses an already published normal receipt when `NormalBasicClose` or `NormalNonCourseRouteClose` won its publication latch; reuse is not another close and does not substitute the authority arm. The selected arm, exact IDs/digests, and typed absences are canonical evidence.

### `TutorialLessonQuiescenceBarrierResult`

P1-E registers one barrier independently from the later P2-B course and presentation barriers. Its result is a closed union:

- `Succeeded(TutorialLessonQuiescenceReceipt)`; or
- `Failed(TutorialLessonQuiescenceFaultEvidence)`.

The success receipt contains:

- runtime-issued `lessonQuiescenceReceiptId`;
- run/stage/route/segment/plan identities and semantic digests;
- exact `TutorialLessonBarrierHostScope`;
- exact successful `TutorialLessonCloseAuthority` arm/ID/canonical digest; `AuthorityUnavailable` is forbidden;
- close reason, including `NonCourseRouteTransition`, `BasicTransition`, `ActiveRunRestartRequested`, route abort, disable/destroy, or unload;
- exact `attemptDisposition = NoAttemptStarted(typed absence of attempt result, attempt/observation/closure generations, and gameplay-disposition receipt; exact no-attempt TutorialPresentationStepBoundaryReceipt ID/canonical digest with FullyReleased) | ClosedAttempt(exact TutorialAttemptResult ID/canonical digest, invalidated attempt/observation/closure generations, continuationClosure = EmbeddedTerminalContinuation | SuppressedPriorIntermediate(exact terminal AttemptContinuationConsumptionRow ID/canonical digest) | ConsumedIntermediateSuccessorClosed(exact Consumed row ID/canonical digest, exact LegacyOpaque successor lesson/binding/generation and course-lease ID/generation, typed absence of successor attempt/context IDs and generations, exact no-attempt successor TutorialPresentationStepBoundaryReceipt ID/canonical digest with FullyReleased))`;
- zero pending producer reservations and retry reservations;
- zero P1-E-owned timer, observer, callback, source token, or presentation handle still assigned to the current director;
- close sequence, canonical `lessonQuiescenceReceiptDigest`, and envelope checksum.

`TutorialLessonQuiescenceFaultEvidence` is the barrier-level wrapper and contains runtime-issued `lessonQuiescenceFaultEvidenceId`, run/stage/route/segment/plan identity, exact `TutorialLessonBarrierHostScope`, exact `TutorialLessonCloseAuthority` including the fault-only `AuthorityUnavailable` arm when applicable, close reason, exact `attemptCoverage = NoAttemptStarted(typed absence of attempt/result/gameplay refs) | ClosedAttempt(TutorialAttemptResult ID/canonical digest) | AttemptClosureFailed(TutorialAttemptClosureFaultEvidence ID/canonical digest)`, failed P1-E boundary, captured/current course-lease disposition, outstanding lesson-loan IDs, pending timer/observer/callback/source-token/presentation-handle IDs, and one `presentationCourseBoundaryCoverage = NotReached | Succeeded(TutorialPresentationStepBoundaryReceipt ID/canonical digest) | FailedAtLessonBoundary | DerivedFromClosedAttempt(exact TutorialAttemptResult ID/canonical digest) | DerivedFromAttemptClosureFault(exact nested fault ID/canonical digest)`, plus close sequence, canonical `lessonQuiescenceFaultDigest`, and envelope checksum. `ClosedAttempt` and `AttemptClosureFailed` require the wrapper's host scope to match the exact nested artifact; `NoAttemptStarted` permits `NonCourse` or `CourseSessionBeforeBasicSelection`, and permits `CourseBasicSelected` only before context materialization. `ClosedAttempt` requires `DerivedFromClosedAttempt` naming the identical result and derives its gameplay-disposition and presentation-step-boundary refs only from that immutable result; it forbids a duplicate outer `Succeeded` receipt ref. `AttemptClosureFailed` requires `DerivedFromAttemptClosureFault` and derives all partial gameplay/presentation slots only from the identical nested fault. `Succeeded` is reserved for no-attempt lesson-boundary closure. No overlapping outer receipt slot is serialized or hashed independently. `FailedAtLessonBoundary` carries no anonymous receipt and uses the parent's failed boundary plus residual course-lease/work evidence. Outstanding loans use loan-issued sequence; pending timers/observers/callbacks/source tokens/presentation handles use their registration sequence within fixed kind order. Duplicate or missing order keys fault. A failure before attempt materialization remains evidence-complete without fabricating an attempt identity or learner outcome; a materialized context whose outcome never froze uses `AttemptClosureFailed` with the nested `NotFrozenBeforeClose` arm.

`lessonQuiescenceReceiptDigest` covers the receipt ID, run/plan identities, complete barrier-host-scope arm, exact successful close-authority arm/ID/digest, close reason, the complete attempt-disposition and continuation-closure arms with all exact refs or typed absences, zero reservation/work facts, fully released course-lease/zero-loan facts, and close sequence. It excludes presentation-only metadata, constituent checksums, and its envelope checksum. `ClosedAttempt` requires the receipt's host scope to match the exact immutable `TutorialAttemptResult` and derives the exact gameplay-disposition and presentation-step-boundary IDs/digests only from that result; the latter must be `FullyReleased` with zero loans. `EmbeddedTerminalContinuation` is legal for the result's terminal `TerminateRoute`, `AdvanceToCourseEntry`, or `SuppressedByActiveRunRestart` arm. `SuppressedPriorIntermediate` is legal only when the immutable result still carries `AdvanceLegacyLesson` or `RetryAttempt` and the exact named consumption row terminally proves `SuppressedByActiveRunRestart` by the same dispatch; for Retry it must name the identical burned reservation and prove typed absence of any successor context. `ConsumedIntermediateSuccessorClosed` is legal only when that immutable result carries `AdvanceLegacyLesson`, the exact named row is `Consumed(LegacyOpaqueActivated)` for the same successor, no instrumented attempt/context ever existed for that successor generation, and the additional no-attempt step-boundary receipt proves the named course lease fully released with zero loans/work after generation invalidation. A consumed `RetryAttempt` or `InstrumentedAttemptMaterialized` successor cannot use this arm; its current typed attempt must close normally. A first publication authorized by `NonCourseRouteContinuation` requires host scope `NonCourse`, close reason `NonCourseRouteTransition`, `ClosedAttempt(EmbeddedTerminalContinuation)`, and exact equality among the authority's final result/selection and the receipt's result with `TerminateRoute`; its gameplay and presentation boundary must be fully released, and no course field or continuation target may exist. A first publication authorized by `BasicTransitionContinuation` requires `ClosedAttempt(EmbeddedTerminalContinuation)`, and its continuation ID/digest must equal the exact `AdvanceToCourseEntry` continuation embedded by that result. A first publication authorized by `ResolvedActiveRunRestartDispatch` with `ClosedAttempt` requires `EmbeddedTerminalContinuation` whose result continuation is `SuppressedByActiveRunRestart`, `SuppressedPriorIntermediate` with the exact matching suppressed consumption row, or `ConsumedIntermediateSuccessorClosed` with the exact matching consumed legacy successor closure; with no materialized attempt or prior result it uses `NoAttemptStarted`. Diagnostic abort may use the same consumed-legacy arm under its own close authority. If a normal latch winner already published the NonCourse- or Basic-authorized receipt, a later restart consumes that exact immutable barrier result as `ReusedPriorNonCourseRouteClose` or `ReusedPriorBasicClose` outside the receipt payload; it cannot demand a restart authority arm, replace the continuation, or recompute the digest. Any substitution hard-faults. `NoAttemptStarted` is legal only for a true pre-attempt/pre-result close, never for an activated legacy successor represented by a consumed row. `lessonQuiescenceFaultDigest` covers the runtime fault-evidence ID, complete barrier-host-scope arm, exact close-authority or `AuthorityUnavailable` arm, corresponding failed-boundary fields, the complete attempt-coverage arm, the one presentation/course-boundary arm, canonically ordered pending/loan IDs, and close sequence while excluding every envelope checksum. Derived gameplay/presentation refs participate only through the exact attempt-result or nested-fault digest, never as duplicate outer fields.

Any P1-A fixed owner row that consumes a successful lesson barrier also carries one summary-external `TutorialLessonBarrierUse = FirstPublishedDuringThisClose | RevalidatedPriorNormalClose(usePurpose, exact first-published close-authority arm/ID/canonical digest, close-publication latch ID/winner, frozen TutorialLessonBarrierHostScope, lesson-barrier generation, NoHigherLessonBarrierGeneration)`. `usePurpose` is exactly `CourseChallengeTerminalFinalization | StationTerminalFinalization | ActiveRunRestart | DiagnosticAbort | RouteHandoff | TerminalActionDispatch(exact ResolvedTerminalActionSelection ID/canonical digest)`. The row always names the exact immutable `TutorialLessonQuiescenceReceipt` ID/canonical digest, and its enclosing aggregate digest covers the complete use arm. `FirstPublishedDuringThisClose` is legal only in the transaction that first seals the receipt. Every later Station terminal, Challenge terminal, restart, abort, route handoff, or terminal-action dispatch uses `RevalidatedPriorNormalClose`; it must prove the same run/plan/host scope, the original normal latch winner and authority, no higher/open lesson-barrier generation, and byte-identical receipt. Terminal-action dispatch additionally requires the exact already sealed selection named by its use arm. Revalidation never republishes the receipt, changes its authority, or invents a new outcome.

An accepted active restart before attempt materialization uses `NoAttemptStarted`; it is not learner completion or an empty successful attempt. If restart owns close publication after outcome freeze, P1-E preserves that outcome, publishes only `SuppressedByActiveRunRestart`, and closes normally or returns the barrier-level failure wrapper with the attempt attachment. If a normal NonCourse or Basic close already won, restart reuses that closed barrier result; P1-A prevents/terminates the pending NonCourse handoff or P2-B suppresses the unconsumed course transition, and P1-E never mutates the `TerminateRoute`/`AdvanceToCourseEntry` result. Fault evidence never satisfies the barrier. P1-A awaits this P1-E result independently from P2-B course, P1-C, P2-A, and P2-B presentation results. A presentation domain already migrated to the P2-B adapter is excluded from the P1-E receipt rather than restored twice.

## Current Director Integration

The compatibility adapter may:

- map an approved stable lesson binding to the current enum step;
- read current Cue/AwaitingAction/Committed transitions;
- request presentation from extracted data while preserving current timing/order;
- receive a new typed same-stack commit seam from the current evaluator carrying snapshotted lesson/attempt/generation identity and the direct typed proof payload;
- let the current director continue applying course-owned input locks and presentation cleanup, and apply combat setup only while its course lease owns the domain or while it acts through the gameplay owner's mutation port;
- invoke an extracted gameplay reset owner only for declared leases.

It must not:

- use enum text, prompt text, object names, or scene path as stored identity;
- parse `LastCompletionRecord` or its record string into proof; it remains diagnostic parity text only;
- allow both current and typed evaluators to advance;
- allow both current director and reset owner to mutate the same gameplay domain;
- move input/HUD/camera/time-scale cleanup into the gameplay reset policy;
- infer success when the director or a required binding disappears;
- single-load Station before the final attempt outcome, gameplay barrier, presentation cleanup, and P1-A fact seal are synchronous and complete.

`CompleteTutorial()` currently restores its domains and raises `Completed`, whose flow subscriber immediately loads Station. P1-E integration therefore freezes and transfers the final lesson/whole-tutorial facts synchronously through the route owner before the load-owning continuation. It cannot depend on subscriber ordering or a later frame.

## P1-A, P1-D, and P2-B Integration

### P1-A tutorial facts

The existing whole-tutorial fact remains the `TutorialRouteSummary` payload arm of P1-A's closed `TutorialAttemptFact` envelope. P1-E appends only `LessonAttempt` payload arms and never fabricates them from strings. Each lesson payload carries the exact plan/lesson/attempt identity, `TutorialAttemptHostScope`, immutable attempt result and gameplay-disposition receipt refs, evaluation/collector provenance, copied outcome proof-disposition arm, elapsed/segment facts, and typed absence of route-summary coverage. The route payload instead carries its own whole-tutorial source-record proof arm plus exhaustive `TutorialFactCoverage[]` and typed absence of every lesson-only field; it never claims a P1-E outcome or gameplay-disposition receipt.

The route coverage orders `LegacyOpaque(planOrdinal, lessonId, NoResultExpected)` or `Instrumented(planOrdinal, lessonId, ResultAdapter | TypedEvaluator, nonempty ordered AttemptCoverage[])` rows in plan ordinal. Each attempt row binds exact attempt ID/ordinal/generation, `TutorialAttemptResult` ID/canonical digest, and lesson-arm `TutorialAttemptFact` ID/canonical digest; it orders by attempt ordinal and exhausts retries as well as the final attempt. Duplicate, missing, substituted, cross-host-scope, or result/fact-mismatched rows fault the Corridor seal. Typed empty coverage is legal only for `LegacyOpaque`. The canonical `tutorialFactCoverageDigest` covers every arm/ref/typed absence and is included by the route-summary fact digest; lesson fact digests never reference the summary, so no digest cycle exists. Consumers therefore cannot mistake a partial P1-E migration for a complete lesson transcript.

Already committed P1-A or P1-D summaries and older route-only fact schemas are never backfilled. A current P1-E run may carry both one current-schema route summary and ordered lesson facts, but consumers must not double-count them. The P1-A route owner seals these facts before Corridor-to-Station load.

### P1-D isolation

Tutorial proof does not automatically become a mastery proof. `DodgeStarted` is not `PerfectDodgeCount`; raw summon use is not `UseSummonForNeed`; lesson completion is not stage Clear. A future explicit qualified adapter requires its own semantic proof ID and coverage contract.

P1-E writes no progress. Adding TutorialProgress later must extend the single prepared-intent/store-writer architecture through a separate migration review; it cannot introduce a parallel save owner.

### P2-B boundary

P2-B owns reusable lesson-chain and presentation-handoff work through separate contracts. It may take a presentation domain only after the adapter captures, restores, cancels stale work, and passes complete/skip/cancel/disable/unload/retry parity for that domain. The P1-E gameplay ledger remains separate. Presentation completion is never lesson proof.

For a course-capable Basic entry, P2-B may consume only a closed `TutorialAttemptResult` whose `attemptResultDigest`, exact `CourseBasicSelected` host-scope arm, `DispositionSealed` gameplay receipt, presentation step-boundary receipt, collector coverage, and continuation identity all agree with the entry snapshot. The accompanying P1-E quiescence receipt must report that same barrier-host-scope arm plus `FullyReleased` current-director course ownership with zero outstanding loans; a legacy transfer disposition cannot unlock Practice. An outcome frozen before closure cannot unlock Practice. The route-owner compare-and-set seals the result and exact course continuation together; the current director's parallel step advance must be disabled for that migrated cohort. P2-B creates no attempt result, retries no attempt, evaluates no lesson proof, and writes no persistent lesson state.

## Validation Matrix

### Authoring and admission

| Invalid condition | Required response |
|---|---|
| duplicate/retired P1-E plan, lesson, presentation, attempt-contract, proof, or reset-policy ID | hard validation failure |
| semantic change without revision/digest change | hard validation failure |
| display text or object name used as proof/target ID | hard validation failure |
| unknown rule, terminal policy, domain, or disposition | hard validation failure |
| nonpositive/nonfinite Move threshold | hard validation failure |
| empty required target set | admission fault, never `TargetsCleared` |
| missing collector capability | admission fault, never observed zero |
| missing/ambiguous stable scene binding | admission fault, never completion |
| P2-B Basic course/session/entry generation or authorizing `CourseEntrySelection` ID/digest is missing/stale/mismatched | admission fault before attempt creation; no proof, result, or continuation |
| `TransferToCourseForNext` without exact successor, current course-lease destination, and exhaustive field set | hard validation failure |
| reset domain with no capture/owned-handle capability | hard validation failure |
| presentation and gameplay owners claim same domain | hard validation failure |

### Runtime integrity

| Fault | Required response |
|---|---|
| wrong run/segment/lesson/attempt/generation | ignore as stale and diagnose |
| duplicate exact terminal signal | return first outcome/receipt; no second advance |
| duplicate exact retry request before outcome freeze | return the same inert reservation; never allocate a second identity |
| retry request after outcome freeze or burned reservation reuse | reject and diagnose; do not alter either attempt |
| duplicate identity with mismatched result | integrity fault; block route |
| continuation selection kind/target/digest disagrees with outcome, retry reservation, or course snapshot | integrity fault; publish no result/advance and burn any reserved successor identity |
| clock regression/frequency change/overflow | system fault; block route |
| authoring edited mid-attempt | continue from entry snapshot |
| source/target attribution unavailable | system fault; never accept generic death/hit |
| reset entry ownership revision changed | cleanup fault; do not overwrite unrelated state |
| cleanup or transfer incomplete | no next lesson or Station handoff |
| owner missing/destroyed before outcome freeze | Interrupted system fault; never success |
| owner missing/destroyed after outcome freeze | preserve the frozen outcome, attach closure-fault evidence, publish no result, and do not advance |
| active restart before an attempt exists and P1-E release fails | return one `TutorialLessonQuiescenceFaultEvidence` with `NoAttemptStarted`, no fabricated attempt attachment, and exact pending course-lease/work identities; never satisfy the barrier |

## Acceptance Matrix

### Move presentation, attempt, and proof

- extracted cue/confirmation copy, voice, focus, fallback, timing, and input behavior match the current route;
- cue-phase combat events are not proof;
- a pointer held from Cue may continue after unlock, but pre-open displacement is excluded by the new baseline;
- exact threshold succeeds and one reviewed numeric unit below fails;
- an observation immediately after opening is buffered but cannot commit before the minimum window;
- duplicate sampling/callbacks produce one outcome and one advance;
- a stale attempt generation cannot record displacement or advance;
- presentation-only edits change only the presentation digest;
- semantic edits after admission do not reinterpret the active attempt;
- Move completion preserves intentional player position and claims no gameplay-reset coverage.

### Terminal paths

- cancel, owner disable, owner destroy, scene unload, and route replacement each freeze at most one terminal outcome;
- unsupported skip/break/failure requests do not fabricate another state;
- a supported retry freezes the old attempt as `Cancelled + RetryRequested`, consumes exactly one inert reservation atomically with old-result publication, and materializes one new context only after the old attempt closes; a failed continuation burns its IDs;
- success racing with cancel/disable has one CAS winner and at most one advance;
- all current observers/timers/callbacks detach or become generation-inert;
- missing director/binding/collector before outcome freeze faults rather than completing; owner loss after freeze preserves the outcome but blocks result publication/advance through closure-fault abort evidence;
- each successful step seals or transfers its current presentation/input boundary once; terminal paths fully restore course-owned input/prompt/overlay/camera domains once;
- cancel/disable/unload restores captured blocker/bounds state, removes only tutorial target candidates/hard lock, applies the explicit target-root/collider/health/AI disposition, and restores the captured combat mode/controller state;
- first-route player position remains preserved by explicit policy; forced facing restores on cancel/interruption/final route exit before result publication, and the tutorial's source-scoped invulnerability token is always revoked;
- nondefault prior action/telegraph/enemy states are restored from captured ownership rather than assumed defaults;
- disabling only the flow owner cannot leave a separately enabled director, timer, observer, or course lease running;
- failed cleanup prevents next lesson and Station load.
- a stuck producer or disposition reaching the closure-watchdog deadline seals exactly one `FaultSealed` snapshot with pending IDs and partial receipts, publishes no attempt result, and cannot be rewritten by later best-effort safety release.
- active restart before attempt creation can close as `Succeeded(NoAttemptStarted)` only after full current-director lease release and zero P1-E work; a release fault returns the barrier-level wrapper without inventing an attempt.

### Fire gameplay reset

- every projectile spawned for the attempt is registered by stable owned handle and no unrelated projectile is removed;
- every async projectile/spawn reserves its ledger handle before start/live exposure, and outcome closure drains all existing reservations before reset;
- mid-flight cancel/retry invalidates proof observation, destroys owned projectiles, restores the target entry snapshot, and atomically returns complete target-domain ownership to the course lease before any retry reacquires it;
- late projectile/damage/death callbacks cannot enter a new attempt;
- natural success explicitly disposes or transfers every still-live owned projectile and atomically returns complete target-domain ownership to the exact current-director course lease, carrying current values only for reviewed fields and restored values for all others;
- reset/transfer runs once under duplicate terminal signals;
- missing target or projectile capability faults before mutation;
- no global scan, blanket buff removal, or scene-wide delete is used;
- current director remains the only input/aim/presentation cleanup owner;
- reset receipt and presentation closure both complete before advance.

### Route/result parity

- lesson order, input gates, current proof timing, confirmation, and Corridor-to-Station handoff remain behaviorally equivalent for the canonical route;
- final facts are serializable and scene-reference-free before single-load;
- no tutorial interruption fabricates Station Fail or a product result;
- P1-A legacy summaries and P1-D state remain unchanged;
- a fresh current-workspace P0 route run covers the required natural, retry/re-entry, and lobby paths; separate P1-E fixtures cover cancel/disable/unload and stale callbacks.

## Bounded Delivery Order

### P1-E0 — Decisions and fresh baseline

1. Close current P0 and P1-0 through P1-D gates on the current workspace.
2. Approve plan/lesson/proof/reset identity format, revisions, digests, and the candidate Move ID.
3. Approve the P1-A run-active/result clock, route-wide scaled rule-window clock, pause-independent closure-watchdog clock, integer result values, terminal mapping, and owner boundaries.
4. Inventory all current director steps and stable scene bindings without changing execution.
5. Add fresh parity coverage for current normal completion and the known cancel/disable/unload gaps before extraction.

### P1-E1 — Move presentation only

1. Create one data-only Move presentation record.
2. Resolve the move-control focus through a stable adapter with the current fallback.
3. Keep the director authoritative for order, input, observation, success, and cleanup.
4. Pass visual/audio/focus/timing and real held-pointer parity.

### P1-E2 — Immutable Move attempt result

1. Add snapshotted IDs, attempt identity/generation, collector coverage, and integer active-clock result fields.
2. Adapt the existing Move commit into one typed immutable outcome without replacing evaluation.
3. Append it idempotently to P1-A's route context.
4. Cover success/cancel/disable/unload/race/stale-generation behavior.

### P1-E3 — Typed Move evaluator

1. Run `MoveDistance` in shadow beside the current evaluator.
2. Prove exact boundary, time-scale 0/0.5/1/2, pause, frame-rate, held-pointer, baseline, and stale-event parity.
3. Switch one advancement authority only after zero unexplained mismatch.
4. Retain the current director as sequence and presentation owner.

### P1-E4 — Fire gameplay-reset fixture

1. Instrument Fire as `ResultAdapter` through the typed commit seam while leaving its current composite evaluator authoritative.
2. Add projectile ownership and target baseline/transfer capabilities without changing Fire success.
3. Return complete target-domain ownership to the current director's course lease for still-legacy Dodge, carrying current values only for reviewed fields; do not fabricate a Dodge attempt or fact.
4. Prove retry/cancel/interrupt restore and natural-success transfer.
5. Close gameplay and presentation barriers before any advance.
6. Keep P1-E's gameplay-reset status open until this non-no-op fixture passes.

### P1-E5 — Remaining lessons, one at a time

Priority after Move and Fire reset:

1. `SwapToRanged`, after transition-versus-current-state semantics are fixed;
2. `Melee`, after hit/death source and target identity are fixed;
3. `Fire` typed evaluator, after projectile/target attribution is stable;
4. `Dodge`, after deciding whether the lesson teaches input start or actual threat evasion;
5. `ClearTargets`, after nonempty binding and alive/despawn/transfer semantics are fixed.

Each lesson repeats presentation, result, shadow-evaluator, terminal, and any owned-reset parity. No bulk migration is allowed.

## Explicit Deferrals

- generic condition graph, reflection evaluator, or copied opaque condition numbers;
- generic/multi-course graph, persistent discovery/availability/mastery/replay state, or TutorialProgress; the later bounded one-run three-entry chain is specified separately by P2-B;
- tutorial rewards, currency, receipts, or parallel save owner;
- generic loadout framework before target/enemy/anchor ownership is stable;
- product-facing lesson retry/skip/failure UI before terminal/reset gates pass;
- PGR signal-orb mechanics, exact exams/stars, robot IDs, content scale, or promotional activities;
- replacement of the entire Olympus tutorial director in one pass;
- presentation cleanup transfer before P2-B domain-specific parity;
- treating archive reset flags or empty cleanup fields as runtime behavior.

## Promotion Gate

The Move vertical slice may promote only when:

- current P0 and P1 predecessor gates are closed;
- IDs, snapshot digests, binding adapters, attempt/observation generation, closure token, all three route clocks, and typed proof values are approved;
- the typed same-stack commit seam exists and `LastCompletionRecord` remains diagnostic only;
- fresh current-workspace normal and terminal parity passes, including the captured course lease's blocker/bounds, target/candidate/hard-lock, combat-mode/facing, invulnerability, action/telegraph/enemy, and flow-owner-disable assertions;
- current director remains the single sequence/input/presentation owner;
- Move's no-op gameplay disposition is explicitly reported as no reset coverage.

P1-E's gameplay-reset claim remains open until the Fire fixture additionally proves:

- source-scoped projectile handles and stable target capture without a global scan;
- reserve-before-live producer admission, proof-inert closure drain, zero pending handles before `DrainSealed`, and complete receipts before `DispositionSealed`;
- exact restore/destroy/transfer dispositions for every supported terminal path;
- atomic return of complete target-domain ownership to the current director's course lease, with only reviewed current values carried into legacy Dodge;
- stale-generation rejection and duplicate-terminal idempotence;
- cleanup failure blocks lesson/scene advancement;
- gameplay and presentation quiescence both close before handoff.

Passing either gate does not authorize TutorialProgress, rewards, a generic course graph, a generic condition DSL, or broad production refactoring. A later bounded P2-B Basic binding still requires a new schema cohort, one continuation owner, and the exact course-entry snapshot/receipt contract.
