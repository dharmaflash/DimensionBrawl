# Stage Rule, Modifier, and Enemy Variant Spec

## Current P1-B closure

- P1-B Station Add and full-exit closure (2026-07-16): `SNAP-P1B-STATION-ADD-AUTHORING-REMEDIATION3-ACCEPTED-11` binds `C:\tmp\DimensionBrawl-P1B-StationAdd-Remediation3-Bundle.md` at SHA-256 `9378bc021b09495c350b331a85755eac7b956a2372d78ecca848a94c2d570c76`; source `128/128` matches digest `4c3dbe952bea5e4f5c57632d70e6fba815d7f6900dc9e1dcbee6af69bae86c89`, artifacts `11/11` match digest `eb5699917083d9be13d571f2a64aa0f69048304552b962df3467b89f3469ce2b`, validator/inventory `8/4/1/1/0`, integrated focused `8/8`, Canonical UI `34/34`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `99/99` all pass with three independent audits at blocker `0`. Revision-1 pose remains relative to `StageDefinitionSceneBinding.transform`; Station `MapRoot` is topology containment only. `ACC-P1B-STATION-ADD-AUTHORING = PASS`; the foreign-evidence row remains PASS through explicit rejection only; `SNAP-P1B-FULL-EXIT-ACCEPTED-12` closes `ACC-P1B-FULL-EXIT-AUDIT = PASS`, so P1-B is **ACCEPTED / VERIFIED-COMPLETE**. This admits no P1-C runtime owner: only the prospective authoring-ledger freeze may start, and runtime work remains gated by `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`.

- P1-B result/progression Remediation3 acceptance: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION3-ACCEPTED-08` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation3-Bundle.md` at SHA-256 `94fa969979bdb2a2b91dfbdf8a5395aed0a69ddd8907831bb7c99da06b139a5b`; source `116/116` matches digest `271793a22e2afc24779a3aeeace7cb9768aae77b7bbbf18a075fa15ea409efb2`, artifacts `14/14` match list digest `c3642305e13c085f710e8db62df807463aea58d8a57331cd7526460eb7a404fc`, validator/inventory `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `98/98` all pass. Independent source, artifact/test, and semantic-contract audits find blocker `0`: route/sidecar-owned canonical catalog identity is independent of the result definition, public Corridor admission and the editor validator require exact object identity, and catalog-only plus coherent catalog/profile/localization clones reject before run creation. Frozen route/policy/join/lifetime digests remain unchanged. `ACC-P1B-RESULT-PROGRESSION-JOINS = PASS / VERIFIED PARTIAL`; Candidate-07 remains immutable historical FAIL. Station count-one Add authoring is now unheld as the next separate P1-B gate, while live PGR/HI3 disposition, P1-B full exit, and P1-C execution remain OPEN and no P1-D/P2-C owner is admitted.
- P1-B result/progression Remediation2 candidate audit: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION2-CANDIDATE-07` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation2-Bundle.md` at SHA-256 `a4e2e2873ec4f53ba81a6c6a3269949b4b2f19255f566d333fcb058e3eeb6de8`; its submitted source manifest matches `116/116` with digest `f4c6f0a6065a2f304acd1a56f7d126b4b2be49582f752f707757d87f37c35583`, all `14/14` artifacts match list digest `96176b861dc7ce0a9aaccd86fe035aa59433513383713132248e51f974b6228a`, validator/inventory is `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact full route `1/1`, and graphics aggregate `98/98` pass. Independent source/contract/test audits verify that Candidate-06's three blocker groups, locale/graph rows, and exact durable-decision byte preservation are closed, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / VERIFIED-FAILED-CANDIDATE-PARTIAL` on one remaining admission defect: the result definition self-selects its catalog, so a catalog-only clone or coherent catalog/profile/localization clone can evade the intended exact-identity gate. The post-bundle route-owned catalog-anchor WIP changes five submitted files and cannot retroactively amend this cutoff. Station Add and P1-B full exit remain held until a new sealed-source candidate passes.
- P1-B result/progression joint-freeze: `P1B-RESULT-PROGRESSION-JOINS-01` Rev3B proposal artifacts match SHA-256 `b6e63b11e3e270302dc33f95b7b69740565e4e27a13ffe017a17f2899256c88f` / `eb65cf30eb961a271f135bc38a9874cccae49e47d8a9d0af5a6dd5f0d7211199` / `933c13943e5397f5fa7a1be531ae34bd28f595e09feee14f18429daa81a8e603`. Fresh PowerShell, independent Node, and a third row reconstruction preserve the seven `15/35/15/17/8/9/38` blocks, sidecar/join snapshot digest `a2ae9df451bd6f2ff48b83098db3bfbdaf2120e23dfaf3612a31f18a022c41fa`, all predecessor digests, and the separate 11-row lifetime-contract digest `3b6cf33325a0a83db74ee2253da9799e589b5664f4fb677b2b021389b0714c0e`. Exact `(ID, revision)` edge resolution and the no-token `Stage Select A -> pre-admission mutation B -> fresh Corridor B` boundary pass. Verdict is **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**. This authorizes implementation only: `ACC-P1B-RESULT-PROGRESSION-JOINS`, Station Add, foreign evidence, and P1-B full exit remain **OPEN**, and no P1-C/P1-D/P2-C owner or P1-A digest change is admitted.
- P1-B result/progression Rev3B implementation candidate audit: `C:\tmp\DimensionBrawl-P1B-ResultProgression-Implementation-Bundle.md` matches SHA-256 `35b1b1a5523bc457ad1936190d1d41143dd1bc8a3489624cdb600631c3a6daa1`; submitted source manifest `116/116` matches digest `1b3dba021b40a4be9d728c6fd4f2039864abb399bbff6d2907e4af274bec24ec`, all `14/14` declared artifacts match list digest `249da60824d3ef617937e648e1257b1fde9b50dc28082a904b78513ca7c76023`, both contract verifiers pass, validator/inventory is `8/4/1/1/0`, focused `2/2`, Canonical UI `28/28`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `93/93` pass. These green artifacts are verified, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / SOURCE-CONTRACT-FAILED-CANDIDATE`: canonical profile/localization object identity is not enforced at admission, the `Presented -> terminal action` path omits the exact pinned join/presentation/audit authority gate and audit self-integrity, and representative deep snapshot damage can throw instead of returning a typed rejection. Direct clone/damage/dispatch, recovery/process-loss, locale, and production graph acceptance rows remain open. The Rev3B joint freeze and every accepted predecessor cutoff/digest remain unchanged; Station Add and P1-B full exit stay held pending remediation and a new sealed-source bundle.
- Status: provisional P2-A contract; analysis only
- Roadmap source: [Subculture Dataset Gap Roadmap](SUBCULTURE_DATASET_GAP_ROADMAP.md), P2-A
- Canonical stage companion: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md), P1-0/P1-B
- Run/result companion: [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md), P1-A
- Encounter companion: [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md), P1-C
- Mastery companion: [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md), P1-D
- Tutorial boundary: [Tutorial Lesson, Attempt, and Gameplay Reset Spec](TUTORIAL_LESSON_ATTEMPT_RESET_SPEC.md), P1-E
- Presentation companion: [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md), P2-B
- Course-chain companion: [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md), P2-B
- Settlement companion: [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md), P2-C
- Implementation gate: not freeze-ready. The current product route, P1-C count-one `Add` fixture/payload mapping, versioned binding set, source-scoped restriction port, modifier port, inactive variant-configuration port/receipt, and exact enemy profile set are not yet approved production facts.
- P1-A predecessor snapshot: the historical 45/49/54/59/68/75 cutoffs remain separate non-additive evidence. The final unchanged-source cutoff passes focused 21/23/15, aggregate 79/79, full route 1/1, and validator checks and closes the audited duplicate/replacement/diagnostic/snapshot-exception defects; P1-A current-schema full exit is **CLOSED**. No P2-A rule, modifier, binding-set, or variant identity may be inferred from any cutoff; P2-A remains a truthful `NotAdmitted` zero-pending row, with its future admitted variability snapshot, success/fault receipt, and quiescence still open.
- P1-B predecessor boundary: three accepted immutable local cutoffs verify the direct Corridor presentation join, one-port/39-binding residue cleanup, and Corridor 4/4 plus Station 0/0 anchor/profile stage-context hygiene at 80/80 without changing route/policy digests. `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-04` remains the historical 19-source/84-test source-contract failure on the authored reward row and invalid-selection invalidation. The separate unchanged-source `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05` remediation passes its 19/19 manifest, authored hidden-row binding, four-row invalid-selection zero-side-effect matrix, focused 8/8, canonical UI 21/21, exact full route 1/1, aggregate 86/86, and validator checks, so `ACC-P1B-CANONICAL-SELECTION` is **VERIFIED PARTIAL** for Candidate-05. Canonical selection supplies no rule set, modifier, enemy-variant binding, source-scoped restriction port, variability snapshot/receipt, or quiescence truth. P2-A remains unimplemented and open.
- P1-B truthful-join rev2A boundary: rev2A jointly freezes 71/27/80 rows and explicitly encodes the current pre-result active-run-restart policy as `NotAdmittedByCurrentSchema (3)` with an empty independent digest. That typed absence is why the historical 71/27/78 rev2 remains AMEND; it does not author a P2-A rule, modifier, variability snapshot, restart policy, or receipt.
- P1-B truthful-join implementation cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1B-TruthfulJoins-Implementation-Bundle.md` matches SHA-256 `8ef3a8e234f53ef561dfdd5d805d0f69c8ddbb55d2a2534ca427f2da821a9d0a`; all 51 ordered sources match manifest digest `1d2fc6a142fa7582e76095c8a928ca1f61f4453ac7061f5d50525673d1480324`, all 13 declared artifacts match, PowerShell and Node reconstruct `71/27/80`, and the validator passes `8/4/1/1/0`. Focused 7/7, canonical UI 26/26, exact full route 1/1, and graphics aggregate 91/91 pass with 91 unique full names and class counts `26/21/3/2/16/23`; frozen route/policy/projection/template/reference/briefing digests match. `ACC-P1B-TRUTHFUL-JOINS` is **PASS / VERIFIED PARTIAL**, while P1-B full exit remains **OPEN**. At its later historical cutoff, Candidate-06 fails `ACC-P1B-RESULT-PROGRESSION-JOINS` on three blocker groups. Remediation2 Candidate-07 subsequently closes those groups but still fails one independent canonical-catalog identity anchor; a new sealed-source candidate is next, then Station Add, live PGR/HI3 foreign evidence, and full exit. This adds no P1-C execution owner, result/progression/reward join or owner, P2-A rule/variant/restart policy, or pre-result active-run restart.
- Safety rule: P2-A adds one bounded variability layer over the canonical stage/run/encounter owners. It does not create a second stage spine, spawner, outcome owner, AI framework, presentation runner, progression store, or reward path.

## Purpose

Define the smallest reusable contract for:

1. one typed stage rule set that distinguishes recommendation from enforced restriction;
2. one environmental modifier with an explicit executable adapter and apply/remove lifecycle;
3. one existing enemy identity reused through Story, Practice, and Challenge variants;
4. one immutable entry-time snapshot that prevents a run from being reinterpreted by later asset edits; and
5. one quiescence barrier that blocks restart or navigation until every P2-A-owned mutation is released.

The first slice validates composition and ownership. It does not import another game's mechanics, content volume, numeric balance, score economy, affix combinations, or difficulty formulas.

## Executive Decision

Use these canonical names throughout P2-A:

- `StageRuleSet`
- `ActiveRunRestartPolicyDefinition`
- `ResolvedActiveRunRestartPolicy`
- `StageModifierDefinition`
- `EnemyVariantProfile`
- `StageEnemyVariantBindingSet`
- `StageEnemyVariantBinding`
- `EnemyVariantConfigurationAdapter`
- `EnemyVariantConfigurationReceipt`
- `StageVariabilityPlanSnapshot`
- `StageVariabilityQuiescenceBarrier`

Use `StageModifierDefinition` consistently. It always separates display metadata from a typed executable adapter and payload.

The first-slice ownership chain is:

`PlayableStageDefinition -> StageRuleSet + StageModifierDefinitionRef[0..1] + StageEnemyVariantBindingSetRef[0..1] -> P1-C scoped spawn key -> EnemyVariantProfile`

At logical route entry, P1-A resolves those references into one scene-reference-free `StageVariabilityPlanSnapshot`. Runtime mutation begins only after the target segment's live ports validate against that snapshot.

## Current DimensionBrawl Baseline

### What exists

| Surface | Current evidence | P2-A consequence |
|---|---|---|
| canonical stage spine | frozen P1-0 `PlayableStageDefinition` exists; `PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md` proposes a later optional `StageRuleSetRef`, but no rule asset or binding exists | extend the one canonical spine after its predecessor gates; do not introduce a parallel stage identity |
| legacy Story PVE | `PveStageData` owns `timeLimit`, starting energy, a final objective, and raw encounter groups; `PveStageContext` is a mutable static selection | keep noncanonical under the P1-C isolation decision; do not use it as P2-A run identity |
| legacy timer | `GameManager` consumes `PveStageData.TimeLimit` only when it exceeds one second, decrements it with `Time.deltaTime`, and resolves timeout by base-HP comparison or `TimeUp` | this is a separate legacy hard-limit behavior, not the linear template's target time and not an approved P1-A outcome rule |
| linear stage intent | `LinearStageTemplateProfile` already owns recommended power tier, target duration, featured summon need, combat lesson, and route segments | retain those owners; P2-A must not copy target time, power, or lesson into a rule asset |
| stage execution facts | `StageDefinitionProfile.SpawnRef` owns kind, payload, anchor, count, and delay; P1-C reserves `(stageDefinitionId, spawnId)` as the scoped spawn key | P2-A variant binding references that key and copies none of its placement fields |
| prototype wave execution | `PveEncounterDirector` sorts raw groups by `triggerZ`, starts untracked delayed coroutines, and has no cancel/dispose/owned-object cleanup lifecycle | useful negative evidence only; it is neither the P1-C nor P2-A runtime owner |
| enemy role intent | `CombatEnemyRoleProfile` owns route/pressure intent plus starting pattern, pattern deck, and optional elite profiles | compose this existing behavior authority into the first variant adapter |
| enemy archetype/candidate | `CombatEnemyArchetypeProfile` owns archetype identity, compatible roles, and prefab candidates; `CombatEnemyRoleCandidateProfile` joins a role to an archetype, role prefab, presentation source, and VFX cues | reuse these references; do not fabricate a new full AI stack before an actual missing owner is proven |
| Station summon lesson | `FrontlineWaveStageProfile`, `BossBarrageEncounterController`, and the Station guide describe a featured SummonSlot1 answer | usable as recommendation vocabulary, not automatically a stage-rule execution port |
| current movement lock | `BossBarrageLaneReviewCombatHudBinder` sets joystick and shared movement blocking from the tutorial guide, then clears both to `false` rather than restoring a captured prior owner state | not admissible as the enforced P2-A fixture; it remains a tutorial/presentation-owned path until a source-scoped port exists |

### Readiness conclusion

No complete P2-A fixture is freeze-ready.

- P1-C has no approved count-one Station `Add`, scoped spawn key, payload mapping, or completion gate.
- No current source-scoped stage restriction port can acquire and release one action/loadout domain without unlocking another owner.
- `FrontlineWaveStageProfile` is a whole scene-bound stage profile, not a typed apply/remove modifier adapter.
- No current local growth-profile owner cleanly separates enemy numeric tuning from the prefab and behavior configuration.
- The exact Story, Practice, and Challenge host fixtures are not approved.

The 2026-07-15 read-only current-worktree audit adds three planning facts, not accepted fixture evidence:

- `DB_Stage_OlympusStationCombat.asset` still has an empty spawn collection, while `OlympusStationCombatStage.unity` directly serializes the boss encounter controller and its action deck. A P2-A variant cannot bypass the missing P1-C scoped spawn key or become a second spawner.
- The current `PlayableStageDefinition` schema carries stage/route identity, ordered segments/actions, and terminal policy but no admitted rule, modifier, variant, or seed reference. The first semantic addition must therefore be the immutable variability reference/snapshot join, not a mutable scene lookup.
- `BossPressureActionDirector` already performs deterministic context-sensitive pressure selection from player-risk and summon context. Generic RNG, affix pools, broad difficulty scaling, and stacking do not solve the nearest product gap; the first useful mutation remains one narrow source-scoped Station domain with exact prior-state capture, reverse restore, stale-generation rejection, and quiescence.

These observations are from a moving shared worktree and must be revalidated against the eventual P1-C/P2-A source manifest. They sharpen the dependency order but freeze no ID, numeric tuning value, adapter capability, or production binding.

P2-A documentation may freeze ownership and validation rules now. Production IDs and bindings remain open until their real predecessors exist.

## External Evidence

All archive evidence is preserved community/public configuration or derived helper data. It supports authoring boundaries, not shipped runtime behavior, cleanup, numeric tuning, or product policy.

### Punishing: Gray Raven

Direct EN `share/fuben/Stage.json` field evidence contains 10,916 stage rows:

| Field | Non-null EN rows | Safe local lesson |
|---|---:|---|
| `RecommandLevel` | 3,491 | recommendation is optional stage metadata, not a required difficulty formula |
| `CharacterLimitType` | 250 | party/loadout restriction deserves a typed optional rule rather than prose |
| `LimitBuffId` | 9,019 | stage-to-modifier reference can stay separate from modifier payload |
| `PassTimeLimit` | 4,410 | a hard time limit is distinct from a target/mastery time |
| `SuggestedConditionId` | 216 | suggestion and enforcement are separate concepts |
| `ForceConditionId` | 465 | enforced conditions require an executable owner and failure policy |
| `RebootId` | 3,449 | a reboot identifier is a reference surface, not proof of restart execution |
| `Restartable` | 5,701 | an allow flag is policy metadata, not proof of cleanup or routing |
| `DisableJoystick` | 373 | input restriction belongs to a typed, source-scoped owner rather than a copied boolean toggle |
| `RobotId` | 1,293 | forced/default party data is distinct from recommendation and restriction metadata |

`StandardUseTimeSec`, `CareerSuggestType`, and `AISuggestType` are entirely empty in this EN snapshot. Optional-looking columns must not be promoted into required DimensionBrawl systems merely because names exist.

The sibling `FightReboot.json` has 20 policy rows. `Stage.json` references 14 distinct reboot IDs and all 14 match that catalog; six catalog rows are unused by this stage snapshot. `StageFightEvent.json` has 557 stage-to-event rows and `FightEvent.json` has 6,643 event/payload rows. This supports separate stage policy and modifier-reference catalogs. It does not prove revive cost consumption, runtime application/removal, cleanup, or navigation. `StarDesc` and `StarRewardId` are string-encoded display surfaces in this snapshot and must never be parsed as evaluators.

### Honkai Impact 3rd

The Global `nairieberry` table inventory exposes deliberately separate surfaces:

| Table | Global rows | Relevant fields |
|---|---:|---|
| `StageChallengeData` | 489 | challenge ID, condition ID, typed parameter list, difficulty, explanation, hint period |
| `StageDetail` | 3,438 | enemy list, recommendation tags/avatar, stage effects, display guide/mission |
| `StageEnhanceData` | 51 | attack/defense/HP ratios, avatar buffs, countdown, level/type parameters |
| `StageRandomEffect` | 40 | stage ID and random-effect reference |
| `StageReChanllenge` | 676 | level ID and rechallenge field |
| `StageRestrictExtend` | 29 | max/requested party count plus three typed request slots and parameters |
| `StageReviveCostData` | 12 | revive type/count surface |
| `StageScoreReward` | 8,294 | score/time/progress reward inputs, intentionally outside P2-A |
| `MonsterConfigData` | 617 | monster name/type, attack/defense/HP, AI name, config file/type |

In `StageData_Main.json`, 4,250 of 9,642 stages carry challenge references: 12,700 reference occurrences over 364 distinct IDs, all matched to the 489 `StageChallengeData` definitions with zero unmatched references. This strongly supports `stage assignment -> typed condition definition + parameters`; the numeric condition codes and parameters remain opaque and are not reusable evaluator semantics. The same stage table exposes revive/reset/recommendation/restriction fields as distinct surfaces, while `fastBonusTime` must not be reinterpreted as a hard fail timer.

In the Global monster helper, 617 rows represent 143 distinct `monsterName` values. 102 names have multiple rows and 87 have multiple `configType` values. For example, `BossBronya` has Default, SP, Tower, Easy, and VeryEasy rows with distinct AI references and some config variants while retaining the same readable name; Default, Easy, and VeryEasy share config values. This supports identity-versus-configuration separation, not copied stats, AI names, or difficulty values.

The 175-row `EditorMonsterCardStage` table exposes `StageTime`, `StageEffect`, and `Wave1..3`, but it is a mode-specific editor table. It is not evidence for a universal stage-wave schema.

### Aether Gazer

The readable stage join contains 6,514 stage-like rows. Populated fields include:

- 1,406 `recommend_level` rows;
- 3,058 `team_type` rows;
- 3,093 `need_default_team` rows;
- 998 `hero_list` rows;
- 992 `combo_skill_id` rows;
- 618 `three_star_need` rows; and
- 550 stage rows with joined `affix_ids`.

The separate catalogs contain 1,082 stage-affix context rows, 1,134 affix definitions, and 3,667 public-buff rows. Of the joined rows inspected, 711 stage-affix assignments and 578 affix-definition rows resolve to public-buff IDs. This is strong static evidence for `stage assignment -> modifier definition/display -> executable payload reference` separation. It does not justify a random affix pool, modifier graph, stack solver, copied buffs, or runtime cleanup claims.

### Girls' Frontline 2 cross-check

`StageChallengeData.json` contains 448 records with separate `type`, title/description, `args`, and failure args; `StageChallengeConditionData.json` contains 43 opaque condition-grammar records. This independently supports keeping display copy apart from typed evaluator/parameter identity. Its turn-based objective meanings and opaque grammar are not DimensionBrawl rules or parser input.

### Wuthering Waves

The directly inspected ConfigDB surfaces contain:

| Table | Rows | Boundary |
|---|---:|---|
| `MonsterInfo` | 249 | readable identity/presentation metadata |
| `MonsterPropertyGrowth` | 480 | level/curve numeric growth ratios |
| `AiBase` | 759 | controller, behavior tree, sub-behavior references |
| `AiBaseSkill` | 428 | skill-set rows |
| `AiSkillInfos` | 4,274 | skill, weight, cooldown, precondition reference |
| `AiSkillPrecondition` | 990 | range/angle/height/tag/target requirements |

All 640 nonempty `AiBase -> AiBaseSkill` references matched. Of 5,488 skill slots, two were empty and all 5,486 nonempty `AiBaseSkill -> AiSkillInfos` references matched. All 4,274 `AiSkillInfos -> AiSkillPrecondition` references matched. The final `MonsterInfo -> AiBase` runtime join is not present in these inspected tables, so the five-layer vocabulary is architectural evidence only, not a ready local mapping.

## Evidence Limits

Do not infer any of the following:

- PGR `Restartable` or `RebootId` cleanup, UI, cost, or route semantics;
- HI3 challenge code meanings, revive economy, score formulas, or universal difficulty scaling;
- Aether affix stacking, application order, conflict resolution, or removal behavior;
- Wuthering AI weights, cooldowns, skill meaning, asset paths, or the missing identity-to-AI link;
- any peer game's numbers, names, IDs, text, assets, mechanics, or content scale.

## Ownership Matrix

| Concern | Authoritative owner | P2-A responsibility | Forbidden P2-A behavior |
|---|---|---|---|
| logical route/segments/actions | P1-0/P1-B `PlayableStageDefinition` | resolve variability references from the same spine | create a second stage ID, scene route, or terminal action list |
| result/outcome | P1-A | preserve rule/modifier/variant provenance in a new-schema snapshot/result | create Clear/Fail, reverse terminal policy, or turn restart into an outcome |
| encounter order/spawn/anchor/object lifetime | P1-C | bind an approved scoped spawn key to one variant | copy payload, anchor, count, delay, group order, or destroy spawned objects |
| mastery/progress | P1-D | expose no implicit proof; retain snapshot identity only | infer mastery from rule, modifier, variant, or display names |
| tutorial attempt/reset | P1-E | begin Station scope only after the Corridor course lease is fully released | acquire a Corridor tutorial domain or share a mutable loadout lease |
| presentation/story cleanup | P2-B | provide restart policy and disjoint gameplay ownership | drive camera, fade, dialogue, HUD, listener, or story navigation |
| reward/settlement | P2-C | contribute only immutable variability identity/digest | grant, price, refund, unlock, or select reward buckets |

## Identity, Revisions, and Digests

Every versioned P2-A authoring record has:

- stable nonempty ID;
- positive schema version and content revision;
- lifecycle status `Active` or `Retired` in a narrow version-controlled inventory;
- canonical semantic digest over every execution-affecting field;
- separate presentation digest over localization, labels, icons, ordering, and explanatory copy; and
- declared adapter capability ID/revision when runtime execution is required.

An ID may be retired but never reused. Changing a rule disposition, typed parameter, segment scope, restart policy, adapter capability, modifier payload, owned domain, variant composition, or stage binding requires a revision/digest change. Presentation-only edits must not change the semantic digest.

The run-level names are:

- `stageVariabilitySemanticDigest`
- `stageVariabilityPresentationDigest`

They are distinct from P1-D's `evaluationSnapshotDigest`, P1-E's `tutorialEvaluationSnapshotDigest`, and P1-C's encounter-content digest.

## `StageRuleSet`

### Required shape

A first-slice rule set contains:

- `schemaVersion`, `ruleSetId`, and `revision`;
- target playable-stage ID and route revision as validation facts;
- exact segment scope; first slice is Station-only;
- a bounded ordered `StageRuleEntry[]`;
- one authored `ActiveRunRestartPolicyDefinition`;
- revision-1 `revivePolicy = Unsupported`;
- semantic and presentation digests; and
- declared ownership domains and adapter capabilities.

The array is bounded to one recommendation and, only after an exact port exists, one enforced restriction. No generic rule graph or boolean expression language is admitted.

### Recommendation versus restriction

Every rule entry has one disposition:

| Disposition | Runtime authority | Required receipt | What it proves |
|---|---|---|---|
| `RecommendationOnly` | none | `NoGameplayMutation` | snapshot and briefing projection only |
| `EnforcedRestriction` | exact source-scoped port and captured lease | complete restore/release receipt | that one reviewed domain was restricted and restored |

`RecommendationOnly` cannot disable input, alter loadout, reject actions, modify outcome, or claim cleanup coverage. A recommendation may name one stable featured summon/action role for briefing and hints, but the runtime remains unchanged.

`EnforcedRestriction` must declare:

- a closed rule kind and typed parameter set;
- the exact segment phases in which it applies;
- the stable action/loadout IDs it permits or rejects;
- the source-scoped adapter capability;
- each gameplay domain it acquires;
- the prior-state fields it captures;
- conflict behavior when another owner already holds a domain; and
- exhaustive terminal restore/release policy.

Directly toggling `Behaviour.enabled`, an `InputAction`, a shared boolean, or a whole loadout without an owner token is forbidden. Releasing the P2-A token must not unlock a P1-E, P2-B, pause, cinematic, accessibility, or system lock.

### First rule vocabulary

The only approved first vocabulary is:

- one `FeaturedSummonRecommendation` with `RecommendationOnly`; and
- one future `SummonAnswerAvailabilityRestriction` with `EnforcedRestriction`, only after stable action IDs and a source-scoped availability port are approved.

Neither rule changes the Clear/Fail definition or arbitration, emits stage completion, grants mastery/reward, changes enemy stats, or owns encounter order. An enforced availability rule may still change the player's permitted actions within its exact scope.

### Target time is not a hard limit

The existing linear template's `targetRunDurationSeconds` remains briefing/mastery intent. It does not cause Fail or timeout.

The legacy `PveStageData.timeLimit` remains outside the canonical route. A future `HardTimeLimitRule` would alter outcome semantics, clock policy, result facts, and route revision; it requires a separate P1-A terminal-policy review and is explicitly outside the first P2-A slice.

## Active-Run Restart Definition and Resolution

`ActiveRunRestartPolicyDefinition` is authored inside `StageRuleSet`. It authorizes, but does not execute, pre-result restart and contains one closed arm: `Disallowed(typed absence of reasons, phases, cutoff, and target) | RestartFromPlayableStageEntry(nonempty stable request reasons, nonempty run/segment phases, cutoff before P1-A's shared terminal-or-restart latch selects TerminalWon/enters TerminalFinalizing, logical target CurrentPlayableStageEntry)`, plus the semantic digest covered by the rule-set revision.

It contains no resolved segment, stage definition, scene identity/string, UI label, result action ID, reward/cost, or mutable asset reference.

At logical entry, the route resolver produces one `ResolvedActiveRunRestartPolicy` inside `StageVariabilityPlanSnapshot`: `Disallowed(definition digest, typed absence of reasons/phases/cutoff/all resolved target fields) | RestartFromPlayableStageEntry(definition digest, exact playable-stage ID, route revision, entry segment ID, stage-definition ID, stable scene identity from the base route snapshot, nonempty allowed reasons/phases, cutoff)`, plus its resolved semantic digest. Both definition/resolved digests cover the complete selected arm and every typed absence. This nested resolved record is the sole runtime authority; `StageRunRouteSnapshot` contains it only through its one P2-A snapshot and does not serialize a second policy or target.

The sequence is:

1. UI, course, or presentation submits one pure `ActiveRunRestartRequest` to P1-A before any owner cleanup. A disallowed request has no cleanup/handoff side effect.
2. P1-A validates it against the immutable `ResolvedActiveRunRestartPolicy` and contends on the same `terminalOrRestartLatch` used by terminal finalization. Only a restart winner enters `RestartClosing` and invalidates further course/result/terminal selection authority; a terminal winner seals `TerminalFinalizationAuthority`, enters `TerminalFinalizing`, and makes this request reject-only.
3. P1-A seals one complete `ResolvedActiveRunRestartDispatch` from the same entry snapshot. It does not seal the abort record yet.
4. Only then does P1-A request and await every admitted P1-E/course, P1-C, P2-A, and P2-B presentation quiescence barrier.
5. P1-A seals exactly one immutable `StageRunAbortRecord` after the barrier results are known: normal active-restart reason plus closure-receipt digest on success, or closure-fault reason plus the available fault evidence on failure.
6. Only the success path disposes the old run and dispatches the already sealed entry target. A barrier timeout/fault leaves the old run in terminal `ClosureFaulted` quarantine, blocks that dispatch, and never reopens the shared latch or reports the run as disposed.
7. A new run ID and any course/entry generations are created only after successful route entry; a course-capable run starts again at Basic.

Once terminal resolution wins the shared latch and enters `TerminalFinalizing`, active restart is rejected during fact, course, P1-C, and presentation finalization and every later pre-commit phase. A committed Clear uses Replay and a committed Fail uses Retry through P1-0/P1-A typed terminal actions. No path fabricates or discards a pseudo-outcome.

Revive is a fourth, separate concept. Revision 1 supports no revive execution, cost, retained-HP, or token policy; any authored revive mode other than typed `Unsupported` fails admission. A later revive slice would require its own life-state, economy, result, and cleanup review and cannot reuse active restart, Replay, or Retry semantics.

## `StageModifierDefinition`

### Authoring shape

The first definition contains:

- `schemaVersion`, `modifierId`, and `revision`;
- one closed modifier kind;
- typed executable payload;
- required adapter capability ID/revision;
- apply scope and clock policy when relevant;
- exact owned domains and prior-state capture schema;
- complete remove/restore policy;
- semantic digest; and
- separate display metadata/presentation digest.

Player-facing description, icon, color, or external affix name never supplies executable meaning. A modifier without a supported typed adapter is invalid before mutation.

### First-slice constraints

- exactly zero or one modifier per admitted route;
- no stacking, ordering solver, random pool, reroll, level scaling, or combination graph;
- no spawn, reward, inventory, persistence, camera, HUD, or story ownership;
- no Clear/Fail or mastery change;
- no global object search or broad reset; and
- no direct mutation before every declared domain is captured and acquired.

A provisional local candidate is one already-consumed Station counter-pressure/window parameter from the current `FrontlineWaveStageProfile`/encounter path. It becomes a fixture only if a narrow typed port can capture and restore that exact domain without replacing the whole stage profile. No modifier ID or tuning value is frozen by this document.

## Existing Enemy Owners and `EnemyVariantProfile`

### Local composition, not a new AI framework

The first local adapter composes existing authorities:

| External architectural concept | First local source | Boundary |
|---|---|---|
| enemy identity/prefab family | `CombatEnemyArchetypeProfile` | retain archetype ID and compatible prefab candidates; it does not override the admitted P1-C payload mapping |
| route/pressure behavior | `CombatEnemyRoleProfile` | retain starting pattern, pattern deck, and elite-profile references |
| role/presentation/VFX candidate | `CombatEnemyRoleCandidateProfile` | validate role/archetype agreement; its role prefab must match the admitted P1-C gameplay prefab for the first fixture |
| instantiated gameplay prefab | P1-C typed payload mapping | sole runtime prefab authority for the scoped spawn key; P2-A may configure but never replace it |
| numeric growth profile | no approved independent local owner | deferred; do not hide prefab values behind a new name |
| skill set/preconditions | current pattern/deck/elite assets where typed | snapshot existing references; do not invent a generic skill/precondition database |

The Wuthering-style five-reference chain remains a long-term separation target, not a claim that five corresponding local asset types already exist.

### Variant shape

`EnemyVariantProfile` contains:

- `enemyVariantId`, schema version, and revision;
- `variantPurpose`: `Story`, `Practice`, or `Challenge`;
- one `CombatEnemyArchetypeProfile` identity/prefab-family reference;
- one compatible `CombatEnemyRoleProfile`;
- one agreeing `CombatEnemyRoleCandidateProfile`; its serialized role-prefab and base-presentation references participate in binding validation rather than replacing P1-C's mapping;
- the resolved starting-pattern, pattern-deck, and selected elite-profile identities/revisions/digests;
- optional profile-local adapter payload from a closed reviewed field set, only after its typed port exists;
- semantic digest and separate presentation digest.

The first slice permits no arbitrary numeric dictionary. Until a real growth owner is extracted, stat-variant completeness remains open and the profiles vary only through existing reviewed behavior/elite references or an explicitly approved closed override.

### Story, Practice, and Challenge reuse invariant

All three profiles must retain the same:

- archetype identity;
- admitted P1-C payload mapping and gameplay prefab;
- base presentation source; and
- P1-C compatible spawn kind.

They may select different existing compatible role/deck/elite references only when each candidate still resolves the same admitted gameplay prefab and base presentation source. Creating three prefabs or three enemy identities fails the reuse fixture. The currently inspected EntryProbe archetype and candidate assets point at different prefab/visual GUIDs, so they are behavioral evidence only and are not an admissible frozen triad until an exact agreeing mapping/candidate set exists.

Only Story binds to the canonical Station route in the first slice. Practice and Challenge initially use isolated single-entry test sets over the same P1-C fixture key/mapping so the same configuration seam and receipts are exercised; those sets are never referenced by the product spine and do not create selectable modes, progression nodes, rewards, or chapter content.

The later P2-B course contract cannot combine the two single-entry sets into one course run. Its isolated proof requires one separately reviewed `IsolatedTutorialCourseFixtureScope` set containing both Practice and Challenge bindings; product promotion requires one `ProductTutorialCourseScope` set containing the same bounded two-entry shape. Neither may relabel a single-entry fixture set as course or product content. The course snapshot, P1-C host/scoped keys, mapping prefab, variant digests, adapter capabilities, and configuration receipt identities must all agree before entry.

## `StageEnemyVariantBindingSet` and `StageEnemyVariantBinding`

`PlayableStageDefinition` reaches one optional `StageEnemyVariantBindingSetRef`. An isolated contract resolver may instead select a test-only set without making it spine-reachable. The versioned set contains:

- `bindingSetId`, schema version, revision, lifecycle status, and one closed typed host scope: first-slice `ProductRouteScope(playableStageId, routeRevision)`, `IsolatedEntryValidationFixtureScope(fixtureId, fixtureRevision, entryKind)`, later `ProductTutorialCourseScope(playableStageId, routeRevision, courseId, courseRevision)`, or `IsolatedTutorialCourseFixtureScope(fixtureId, fixtureRevision, courseId, courseRevision)`;
- a bounded ordered `StageEnemyVariantBinding[]`; first-slice product scope permits exactly one Story binding, isolated-entry scope permits exactly one matching Practice or Challenge binding, and either tutorial-course scope requires exactly two distinct entry-scoped bindings in strict Practice/Challenge order; and
- canonical semantic and presentation digests.

Each versioned binding contains only:

- stable `bindingId`, schema version, revision, lifecycle status, and semantic/presentation digests;
- canonical P1-C scoped spawn key `(stageDefinitionId, spawnId)`;
- one `enemyVariantId` and expected semantic digest; and
- optional presentation-only override from a closed field set.

A binding must not copy group/order, payload ID, prefab, anchor, transform, position ID, count, delay, HP, AI numbers, completion policy, or navigation. A course-scoped binding additionally names exactly one owning course-entry ID (`FreePractice` or `SummonMasteryChallenge`) without copying course order or transition policy. Changing membership, scoped key, entry owner, or variant changes the binding and set revisions/digests. Moving a product set to another route/scope also changes the base route revision; an isolated set can target only its named test fixture and is rejected by product catalog/progression resolution.

P1-C remains the object factory and lifetime owner. For a P2-A-capable run, its static plan validates that the existing payload mapping is compatible with the archetype and that its gameplay prefab equals every selected candidate's admitted role prefab. A missing, duplicate, unreachable, retired, or incompatible set/binding fails before any object is instantiated.

### Pre-activation variant configuration seam

After P1-C has created and fully owned an inactive staging root, but before the ticket is armed or any `Awake`/`OnEnable`-observable activation, it invokes one typed `EnemyVariantConfigurationAdapter` capability with the frozen binding/profile composition and the P1-C-owned handle. The adapter may call only reviewed component configuration ports for the resolved role, pattern, deck, elite profile, or approved closed override. It may not instantiate, destroy, reparent, activate, swap the prefab, register unbounded work, or take object-lifetime ownership.

Admission first issues one run-unique `configurationCallId` and monotonic `configurationCallAdmissionSequence`, binding-member ordinal, P1-C ticket ordinal, and exact `configurationScope = NonCourse(typed absence of all course fields) | Course(courseId, courseSessionId, courseGeneration, entryId, entryGeneration, CourseEntrySelection ID/canonical digest)`. The Course arm is required if and only if the resolved binding is course-hosted and must name its current selection; NonCourse forbids foreign course provenance. Every result and pending-call diagnostic repeats this exact call/scope tuple.

The call result is the closed union `Succeeded(EnemyVariantConfigurationReceipt)` or `Failed(EnemyVariantConfigurationFailureReceipt)`.

Success returns one immutable `EnemyVariantConfigurationReceipt` containing runtime-issued receipt ID, exact `configurationCallId`/admission sequence and binding-member/ticket ordinals, run/ticket identity, P1-C execution generation, P2-A variability generation, exact `configurationScope`, binding-set and binding identities/revisions/digests, variant/profile identities, P1-C mapping and gameplay-prefab identity, adapter capability revision, configured component/field digest, success disposition, canonical `configurationReceiptDigest`, and envelope checksum. The canonical digest covers the receipt ID, exact call/ticket/generations/scope arm, snapshot/binding/variant/mapping/capability identities, configured-field digest, and disposition; it excludes presentation metadata and the envelope checksum. P1-C stores that receipt on its owned handle and may cross the activation barrier only after it validates the receipt against the current ticket, execution/entry generations, authorizing selection when Course, and snapshot.

`EnemyVariantConfigurationFailureReceipt` contains runtime-issued failure-receipt ID, the same exact configuration-call ID/admission sequence, binding-member/ticket ordinals, run/ticket/generation/configuration-scope/snapshot/binding/variant/mapping/capability provenance, failed boundary/reason, partial configured-field digest, proof that the hierarchy remained inactive, adapter rollback disposition `RollbackComplete | RollbackFaulted`, rollback final-state digest and sequence, failure sequence, canonical `configurationFailureReceiptDigest`, and envelope checksum. Its canonical digest covers those exact fields and complete scope arm including typed absence and excludes presentation metadata and every envelope checksum.

Configuration failure returns that receipt while the hierarchy is still inactive. P1-C rolls back the whole factory transaction and staging root; the ticket cannot become `Armed` or count as a successful spawn. `RollbackComplete` may participate in an otherwise successful final P2-A quiescence receipt as a canonical failed-call result; `RollbackFaulted` enters closure-integrity fault and cannot. Revision 1 admits no pooling, so terminal object destruction remains P1-C work and no P2-A variant reset is guessed. A later pooling slice would require an explicit reverse/reset receipt.

### `StageVariabilityOperationFailureReceipt`

A validation, acquisition, apply, or configuration failure whose rollback fully completes seals one runtime-issued `variabilityOperationFailureReceiptId` with run/execution identity or typed `NotIssuedBeforeFailure`, exact failed boundary/reason/source, optional exact `EnemyVariantConfigurationFailureReceipt` ID/digest or typed absence, fixed source/domain rollback coverage in canonical source/domain order, zero remaining owner-token/work/configuration-call/callback/timer counts, rollback sequence, canonical `variabilityOperationFailureDigest`, and envelope checksum. Its digest covers those exact fields, every typed absence, and all ordered rollback rows while excluding presentation metadata and every envelope checksum. It proves a failed operation became ownership-quiescent; it is not a successful gameplay operation, configuration receipt, course-transition input, or product result.

## Entry-Time `StageVariabilityPlanSnapshot`

P1-A captures this snapshot at logical Corridor entry, before either Corridor or Station runtime can mutate gameplay. It contains no Unity object or mutable asset reference.

Required fields:

- run ID, playable-stage ID, route revision, and `coreRouteSemanticDigest` computed without P1-C/P2-A/P2-B extensions;
- rule-set ID/revision, ordered typed entries, authored restart-definition digest, revive policy, ownership declarations, semantic digest, and presentation digest;
- exactly one `ResolvedActiveRunRestartPolicy` and entry target nested here only;
- canonical `resolvedModifiers[]` with cardinality zero or one; zero is encoded as an empty array and participates in the digest, while one entry contains modifier ID/revision, typed payload, adapter capability, apply scope, clock policy, owned domains, restore policy, and digests;
- canonical `variantBindingSelection = None | ResolvedBindingSet`; `None` is explicit before P2-A4, while `ResolvedBindingSet` contains the exact typed host scope, set ID/revision/digests, and each binding's ID/revision, optional owning course-entry ID, scoped spawn key, variant ID/purpose/revision, resolved archetype, role, candidate, pattern, deck, elite, override, and semantic digest;
- fixed spine-order P1-C payload-mapping/encounter-plan/gate identities and digests needed to prove agreement, or typed empty coverage when no P1-C layer is admitted;
- when `ResolvedBindingSet` is selected, the P1-C-authoritative gameplay-prefab identity and required variant-configuration adapter capability for each bound key; with `None`, no variant-configuration capability is required;
- adapter capability manifest revision; and
- one canonical `stageVariabilitySemanticDigest`, separate presentation digest, and optional full snapshot-envelope checksum.

`stageVariabilitySemanticDigest` covers the canonical rule/modifier/binding-set/binding/variant/restart semantics including typed absence, adapter-capability manifest revision, `coreRouteSemanticDigest`, the complete binding-set host-scope identity arm including course ID/revision when course-hosted, and the exact earlier P1-C identities/digests. It includes no P2-B course semantic digest, course entry-order/transition semantics, or final route digest, so equal P2-A content remains one auditable cohort across runs without pretending its course host identity is absent. It excludes run ID, execution generations, and presentation metadata. An optional full-envelope checksum may bind the run ID and complete serialized snapshot without becoming content identity.

For a tutorial-course scope, the sole `ResolvedBindingSet` contains both ordered Practice and Challenge members plus their exact course-entry and P1-C host-scope agreement. It is one run-admission selection, not two sets and not a later asset lookup. Each pre-activation configuration call consumes only the member for the current entry and closes with its own `configurationReceiptDigest`; closing that call does not release the run-wide P2-A lifecycle or seal its quiescence barrier.

The course snapshot may exist during Basic, but P2-A runtime acquisition remains mutation-free until all Practice admission prerequisites are sealed: the independent P1-E lesson barrier succeeds with current-director ownership `FullyReleased` and zero outstanding loans; the exact current Practice `CourseEntrySelection` ID/digest/generations is valid; and the optional entry presentation is either typed `NotAuthoredBySnapshot` or has a successful current-selection `GameplayHandoff` result/receipt. Only then may acquisition win once for the Practice/Challenge span. The acquired run-wide lifecycle admits entry-scoped configuration calls and closes only for final result/abort/restart. Challenge entry presentation gates its Challenge-scoped configuration call and P1-C activation, not a second P2-A acquisition. P2-A never treats the Basic result, P1-E release receipt, or presentation result as its own work.

The P2-A semantic digest is included in the later P2-B course digest when present and then in the final new-schema route content/snapshot digest and result provenance. The core/P1-C layers never include P2-A, and P2-A includes neither P2-B nor the final route digest. Editing any source asset after entry cannot reinterpret the active run. Scene entry compares live capabilities and current authoring digests to the captured snapshot; disagreement faults instead of adopting the latest values.

Older runs without this snapshot are not P2-A-capable and are never backfilled.

### `StageVariabilityCloseCommand`

Every P2-A close begins with one immutable command containing runtime-issued `variabilityCloseCommandId`, run/route and variability-snapshot identity, `executionIdentity = Issued(executionInstanceId, executionGeneration) | NotIssuedBeforeClose`, exact `courseCloseContext`, close reason, and exact authority arm/ID/canonical digest: `TerminalFinalizationAuthority`, `ResolvedActiveRunRestartDispatch`, or `StageRunAbortCloseAuthority`. It also carries issued sequence, canonical `variabilityCloseCommandDigest`, and envelope checksum. The digest covers those exact fields and typed absences while excluding the checksum and presentation metadata.

The terminal-finalization arm is accepted only when P1-A has reached `OutcomeFactsSealed`, completed any admitted P1-D evaluation, and entered `VariabilityClosing` under that same authority. Restart and abort arms must match their sealed P1-A records and current lifecycle. A stale, foreign, mismatched, or duplicate command cannot acquire, configure, restore, release, or change a sealed receipt. Post-commit terminal actions only revalidate the already sealed P2-A receipt; they cannot issue a new close command or replace its authority.

## Runtime Lifecycle and Ownership Ledger

### Lifecycle

Primary path:

`Unbound -> Validating -> Acquiring -> Applied -> Closing -> Released`

No-acquisition close path:

`Unbound | Validating -> ClosingWithoutAcquisition -> Released`

A validation fault before acquisition, or an acquisition/apply/configuration fault whose rollback completes, seals one `StageVariabilityOperationFailureReceipt` and may then close with no live P2-A work; it does not by itself imply run-level `ClosureFaulted`. An incomplete rollback, unresolved in-flight configuration call, or failed restore/release enters `Faulted` instead of `Released`. This closure-integrity `Faulted` state never satisfies the quiescence barrier or implies that the run is disposed. One run-wide `variabilityAcquireOrCloseLatch` exists from route admission. Its acquisition arm is ineligible until the P1-E/Practice-selection/entry-presentation prerequisites above succeed; its close arm may win during Basic, while awaiting Practice selection, or after Practice selection but before acquisition. An acquisition winner atomically mints the P2-A execution identity before obtaining any token and must follow the primary path; a close winner mints no identity, cancels/drains validation, proves zero token/work/configuration, and follows `ClosingWithoutAcquisition`. Reaching Challenge requires the Practice acquisition winner, so a Challenge close always follows the issued-identity primary path. There is no gap in which both can win.

1. `Validating` checks snapshot, segment, adapter capability, binding-set reachability, P1-C scoped keys/mapping/prefab agreement, domain conflicts, and all restore capabilities without mutation.
2. `Acquiring` atomically captures exact prior values and obtains source-scoped tokens for every enforced rule/modifier domain.
3. Only after every acquisition succeeds does one transaction apply the declared mutations and enter `Applied` before gameplay input is released.
4. Failure while acquiring or applying restores any earlier acquisition in reverse order and seals `StageVariabilityOperationFailureReceipt`; incomplete rollback enters closure-integrity `Faulted`.
5. A raw Clear/Fail callback does not start cleanup. When the terminal arm wins P1-A's shared latch, `TerminalFinalizing` immediately makes P2-A acquisition/configuration admission reject-only; the still-applied lifecycle remains frozen while P1-A seals authoritative outcome/facts. After `OutcomeFactsSealed` and any P1-D evaluation, a P2-A-capable run enters `VariabilityClosing` and this lifecycle enters the one idempotent `Closing` path before `CommitRequested`.
6. Abort, active restart, owner disable/destroy, route replacement, and scene unload first enter P1-A `AbortClosing` or `RestartClosing`, invalidate old terminal/gameplay authority, and then use that same `Closing` path. Replay, Retry, and Lobby occur only after a committed result and revalidate an already sealed `Released` barrier rather than initiating cleanup.
7. Closing invalidates the execution generation, stops registered callbacks/timers, removes modifier effects, releases enforced restrictions, restores exact prior values, records receipts, and enters `Released` only when every required operation succeeds; otherwise it enters `Faulted` with partial evidence.

### `StageVariabilityOwnershipLedger`

Every acquired domain records:

- run ID, execution instance ID, and monotonically issued execution generation;
- rule/modifier source ID and revision;
- domain ID and source-scoped owner token;
- captured prior value and expected current/source revision;
- applied value or adapter receipt;
- terminal disposition;
- restore/release receipt; and
- fault details when exact restore is impossible.

Terminal dispositions are `NoGameplayMutation`, `RestoredPrior`, `ReleasedOwnedToken`, or `Faulted`. No global reset or guessed default is allowed.

Recommendation-only entries appear in snapshot coverage with `NoGameplayMutation`; they do not appear as acquired domains.

### `VariabilityDomainTerminalReceipt`

Every snapshotted source coverage element seals one immutable receipt. A recommendation contributes one source-level no-domain element; an enforced rule or modifier contributes one element per declared domain:

- runtime-issued receipt ID;
- run identity and `executionIdentity = Issued(executionInstanceId, executionGeneration) | NotIssuedBeforeClose`;
- exact `coverageIdentity = NoDomainForRecommendation(rule source ID/revision) | Domain(rule/modifier source ID/revision, declaredDomainOrdinal, stable domain ID)`, plus optional source-scoped owner-token identity only for the domain arm;
- acquisition disposition `NoGameplayMutation` exactly for `NoDomainForRecommendation`, or `NotAcquiredBeforeClose | Acquired` exactly for `Domain`;
- captured-prior, applied, expected-current, and final-value digests, each typed absent for `NotAcquiredBeforeClose` and `NoGameplayMutation`;
- terminal disposition `NotAcquiredBeforeClose | NoGameplayMutation | RestoredPrior | ReleasedOwnedToken`;
- exact lifecycle-sequence arm `NoMutation | NotAcquired(close sequence) | Acquired(apply sequence, remove sequence or typed absence, restore/release sequence)` matching the acquisition and terminal dispositions;
- canonical `variabilityDomainTerminalDigest` and envelope checksum.

The canonical digest covers the receipt/execution identity, exact coverage-identity arm, typed acquisition/absence, value digests, terminal disposition, and lifecycle-sequence arm. `NoDomainForRecommendation` cannot carry a domain ID/token/value or claim cleanup; `Domain` cannot use `NoGameplayMutation`. `NotAcquiredBeforeClose` is legal only when the admitted snapshot had not yet reached its reviewed acquisition boundary and the domain ledger proves no owner token, mutation, configuration call, callback, or timer ever existed; it is not a guessed restore. It excludes presentation-only metadata and every envelope checksum. `Faulted` cannot produce this success receipt; its partial ledger evidence belongs to `StageVariabilityClosureFaultEvidence`.

### Generation safety

Every callback, timer, or deferred modifier completion captures the P2-A execution generation. Closing invalidates it before any restore begins. A stale callback may report diagnostics but cannot reacquire a token, reapply a modifier, alter an enemy variant, publish a result, or affect the next run.

The first slice admits no P2-A-owned spawned object or unbounded asynchronous producer.

## `StageVariabilityQuiescenceBarrier`

The barrier result is the closed union `Succeeded(StageVariabilityQuiescenceReceipt)` or `Failed(StageVariabilityClosureFaultEvidence)`. It reports success only when:

- no rule/modifier callback or timer is pending;
- every applied modifier is removed;
- every enforced restriction token is released;
- every admitted configuration call is sealed as `Succeeded(EnemyVariantConfigurationReceipt)` or `FailedRollbackComplete(EnemyVariantConfigurationFailureReceipt)`, and no call remains in flight;
- every acquired domain has a complete terminal receipt;
- the ownership ledger is sealed; and
- no P2-A execution generation remains active.

`StageVariabilityQuiescenceReceipt` contains runtime-issued `variabilityQuiescenceReceiptId`, run identity, `executionIdentity = Issued(executionInstanceId, executionGeneration) | NotIssuedBeforeClose`, frozen variability snapshot and semantic digest, exact `courseCloseContext = CourseSessionBeforeFirstSelection | CurrentEntrySelection | BetweenCourseEntries | NonCourse` with the matching session, selection, or prior-transition/successor-entry provenance, exact `StageVariabilityCloseCommand` ID/canonical digest and authority arm/ID/canonical digest, close reason/sequence, `lifecycleDisposition = Released | ClosedWithoutAcquisition`, exact `operationalDisposition = Normal | RolledBackComplete(StageVariabilityOperationFailureReceipt ID/canonical digest)`, the `variabilityAcquireOrCloseLatch` identity/winner, fixed canonical coverage of `VariabilityDomainTerminalReceipt` IDs/digests for every source-level recommendation or declared rule/modifier domain, ordered configuration-call rows `Succeeded(EnemyVariantConfigurationReceipt ID/canonical digest) | FailedRollbackComplete(EnemyVariantConfigurationFailureReceipt ID/canonical digest)` for every admitted call, zero-pending counts, canonical `variabilityQuiescenceReceiptDigest`, and an envelope checksum. Every row repeats its exact call ID, admission sequence, binding-member ordinal, and P1-C ticket ordinal. The P2-A execution identity is minted only when the acquisition arm wins after the exact Practice prerequisites. `ClosedWithoutAcquisition` requires the close arm plus `NotIssuedBeforeClose`; it is legal before first selection, during Basic, between Basic `Advanced` and Practice selection, or after Practice is Selected but before acquisition wins. It is not legal after Practice execution admitted, Practice `Advanced`, or Challenge became Available/Selected. Every mutable `Domain` row must be `NotAcquiredBeforeClose`, each recommendation contributes exactly one `NoDomainForRecommendation + NoGameplayMutation` row, the configuration collection is empty, and all validation/token/work/callback counts are zero. Coverage rows order first by source kind (`StageRuleEntry` before `resolvedModifiers`), then immutable serialized source ordinal, then declared-domain ordinal; a recommendation's sole no-domain row uses the reserved source-level ordinal. Stable domain ID is the uniqueness check, and duplicate/missing source or declared-domain ordinals fault. Configuration rows use resolved binding-member ordinal (strict Practice/Challenge order for a course, otherwise zero), then P1-C static-plan ticket ordinal; duplicate ordinals, call IDs, or ticket identities fault. Neither collection uses apply, rollback, or callback completion order. The canonical digest covers receipt ID, execution-identity arm, exact close-command/authority, latch identity/winner, course-close context, semantic closure facts, lifecycle and operational dispositions, fixed source/domain coverage, and every typed configuration-call arm with exact constituent canonical receipt digests, not their envelope checksums or presentation-only metadata. `FailedRollbackComplete` requires the exact operation-failure receipt to cover it; `RollbackFaulted` or a pending call forces the failed barrier arm. `StageVariabilityClosureFaultEvidence` accompanies `Failed`; it never satisfies success, never claims `Released`/`ClosedWithoutAcquisition`, and cannot be used as a course transition receipt.

Enemy object death/destruction and P1-C spawn-ticket lifetime are not P2-A barrier work; only the pre-activation configuration call/receipt belongs to P2-A.

For Clear/Fail, the deterministic P2-A-capable commit sequence is:

1. P1-A reaches `TerminalClosed`, wins the shared latch, seals `TerminalFinalizationAuthority`, enters `TerminalFinalizing`, and rejects every new P2-A acquisition/configuration admission. After required fact, course traversal/quiescence, P1-C RunFinalization, and current-generation presentation aggregate coverage succeeds, it seals their fixed rows in `TerminalFinalizationOwnerCoverageRecord`, enters `TerminalFinalizationOwnersSealed`, and only then seals `OutcomeFactsSealed`.
2. P1-D evaluates and seals mastery when that schema is present; no P2-A cleanup value becomes a fact.
3. P1-A enters `VariabilityClosing`, requests P2-A close, and awaits `StageVariabilityQuiescenceBarrier`.
4. Success seals `VariabilitySealed`; only then may P1-A enter `CommitRequested`.
5. Timeout/restore/configuration-integrity fault instead enters `AbortClosing`, seals one diagnostic abort with closure evidence after the failed barrier result is known, enters run-level `ClosureFaulted`, and publishes no result or disposal claim.

For active restart, P1-A wins the shared terminal-or-restart latch and seals the complete dispatch record before cleanup but seals the abort record only after all admitted P1-E/course, P1-C, P2-A, and P2-B presentation barrier results are known. Successful closure follows `Aborted -> Disposed` and performs the actual dispatch; a failed barrier follows `Aborted -> ClosureFaulted` and cannot dispatch. For another pre-commit abort, P1-A likewise enters `AbortClosing`, closes admitted owners, then seals the single immutable abort with all available receipts/evidence and chooses the same success/failure terminal branch.

A post-commit terminal action seals its action first and revalidates that the P2-A barrier was already `VariabilitySealed/Released`, while also awaiting its other admitted barriers. A newly detected post-commit P2-A integrity fault preserves the immutable result, writes summary-external `StageDispatchClosureFaultRecord` evidence, blocks dispatch/navigation, and never creates or mutates `StageRunAbortRecord`, clears the selected terminal-action latch, or permits an alternate route. Pre-result active restart cannot enter this branch.

Optional `StageVariabilityClosureFaultEvidence` in a pre-commit `StageRunAbortRecord` contains runtime-issued `variabilityClosureFaultEvidenceId`; run identity; `executionIdentity = Issued(executionInstanceId, executionGeneration) | NotIssuedBeforeFault`; exact `StageVariabilityCloseCommand` ID/canonical digest and authority arm/ID/canonical digest; `variabilityAcquireOrCloseLatch` identity/state/winner; exact `courseCloseContext = CourseSessionBeforeFirstSelection | CurrentEntrySelection | BetweenCourseEntries | NonCourse`; failed validation/acquisition/rule/modifier/domain/configuration boundary; captured/expected/current values; ownership tokens; pending validation, token, configuration-call, callback, and timer evidence; fixed source/domain coverage where every row first carries the exact outer `coverageIdentity = NoDomainForRecommendation(rule source ID/revision) | Domain(rule/modifier source ID/revision, declaredDomainOrdinal, stable domain ID)` and then exactly one disposition `NotReached | Terminalized(VariabilityDomainTerminalReceipt ID/canonical digest that repeats the identical coverageIdentity) | PendingAtFault(captured/expected/current digests, optional token)`; every admitted configuration call as `Succeeded(EnemyVariantConfigurationReceipt ID/canonical digest) | Failed(EnemyVariantConfigurationFailureReceipt ID/canonical digest) | PendingAtFault(configurationCallId, configurationCallAdmissionSequence, binding-member ordinal, P1-C ticket ordinal, exact configurationScope, adapter phase, partial configured-field digest)`; frozen semantic digest; fault sequence; canonical `variabilityClosureFaultDigest`; and envelope checksum. Validation evidence uses fixed validation-phase ordinal. Source/domain coverage uses the same source-kind, serialized source ordinal, and declared-domain ordinal ordering as success; recommendation rows use the one no-domain arm. Configuration evidence uses binding-member then P1-C ticket ordinal, and callbacks/timers use registration sequence. Duplicate/missing order keys fault. The canonical fault digest covers its runtime evidence ID, identity arm, exact close-command/authority, latch and course-close provenance, every outer coverage identity and its typed disposition, those canonically ordered semantic evidence fields, every typed source/domain and configuration arm including exact receipt refs or fully expanded pending-call state, and fault sequence while excluding presentation-only metadata and every envelope checksum. It is diagnostic only.

## Integration Boundaries

### P1-B stage spine and briefing

The canonical spine references the rule set, zero-or-one modifier definition, and one optional versioned variant-binding set. P1-C's stage-local encounter binding remains the owner of every scoped spawn key named by that set. The resolver combines them only into the entry snapshot; it does not introduce a second authoring spine.

`StageBriefingReadModel` derives:

- title, objective, lesson, recommended power, target time, and featured summon need from the existing stage/template owners;
- recommendation/restriction copy from the snapshotted rule set;
- modifier label/icon/copy only when `resolvedModifiers[]` contains one entry; and
- enemy preview identity only when `variantBindingSelection` contains a resolved set.

Story cue comes from canonical cinematic references. Post-result Retry/Replay comes from typed route actions. P2-A copies neither.

### P1-A result provenance

A P2-A-schema result carries `stageVariabilitySemanticDigest` and the stable rule/modifier/binding-set/binding/variant cohort IDs or typed absence needed for audit. This is provenance, not proof that a recommendation was followed or a modifier caused success.

P2-A does not create run facts unless a separately reviewed P1-A semantic collector exists. Rule/modifier/variant names are never parsed as facts.

### P1-C encounter execution

P1-C retains activation, group order, clock, spawn tickets, factory transaction, anchor, object lifetime, typed death, completion gate, cancellation, cleanup, and scene lease.

P2-A extends a new-schema static plan only with the resolved binding-set/variant identity, agreement digest, required configuration-adapter capability, and expected receipt identity for an existing scoped spawn key. During P1-C's inactive factory transaction it configures only the reviewed ports and returns a receipt; P1-C remains the sole owner of activation, advancement, and object destruction.

### P1-D mastery/progress

Recommendation display, restriction application, modifier application, and variant selection are not mastery. A future objective requires an explicit P1-A typed semantic-proof adapter plus a P1-D schema review. Existing P1-D results and state remain unchanged.

### P1-E tutorial

The first P2-A runtime scope begins in Station only after the Corridor tutorial course/attempt ownership is fully closed or transferred back and then released at segment exit. P2-A cannot acquire the same input/loadout domain as an active P1-E course lease.

### P2-B presentation

P2-B owns camera, fade, dialogue, HUD, listener, actor visibility, playback, and presentation input capture. P2-A owns only declared gameplay rule/modifier domains. Overlapping domain declarations fail validation before either owner applies.

A P2-B presentation/course source may submit only a pure pre-result restart request before cleanup. P1-A validates the nested resolved P2-A policy and seals the dispatch record first, then closes presentation/course plus gameplay/execution barriers and seals the abort record from the resulting receipts/evidence. Only successful closure performs the actual dispatch.

The P2-B course coordinator may reference frozen P2-A Practice/Challenge identities and read completed configuration receipts, but it never authors the variant, configures a prefab, activates/destroys an object, or reports P2-A work as course-quiescent. An ordinary Practice-to-Challenge transition does not seal the run-level `StageVariabilityQuiescenceBarrier`; a future entry-scoped P2-A lease requires a distinct reviewed receipt. Course/P2-A snapshot disagreement is an admission fault.

### P2-C settlement

For new P2-A/P2-C cohorts, the settlement authoring snapshot includes the frozen `stageVariabilitySemanticDigest` as run provenance. P2-C may not derive reward eligibility, quantity, first-clear status, or progression from a rule/modifier/variant name. Any reward relationship remains explicit P2-C authoring.

## Validation Matrix

| Check | Failure condition |
|---|---|
| identity inventory | empty, duplicate, reused retired, or unknown-schema ID |
| semantic revision | execution field changed without revision/digest change |
| presentation separation | localization/order/icon edit changes semantic digest or execution reads presentation text |
| route scope | rule/modifier targets a different playable stage, route revision, or segment |
| recommendation disposition | recommendation declares a mutation, adapter, acquired domain, outcome, or cleanup coverage |
| enforced restriction | no stable action IDs, no source-scoped port, no prior capture, or incomplete terminal policy |
| ownership conflict | P1-E/P2-B/system owner and P2-A claim the same non-composable domain |
| modifier adapter | display-only record, unsupported capability/revision, free-form payload, or missing restore port |
| modifier count | more than one modifier in the first slice |
| modifier absence | zero modifiers is not encoded as the canonical empty `resolvedModifiers[]` value or changes digest nondeterministically |
| hard time confusion | target/mastery time is treated as Fail/time-up or legacy PVE timer is imported |
| restart definition | resolved segment/scene data, UI label, result action, cost, or post-commit allowance appears in authoring |
| resolved restart | entry target is unresolved, differs from the base route snapshot, or a second policy/target is serialized outside `StageVariabilityPlanSnapshot` |
| revive policy | any revision-1 mode other than typed `Unsupported`, or revive is treated as restart/Replay/Retry |
| binding reachability | a selected spine binding-set ref is duplicate/unresolved, set/binding identity is retired or unversioned, or membership/digest disagrees with route scope |
| binding host shape | a first-slice product/isolated-entry set has other than one matching member, a tutorial-course set has other than the ordered Practice/Challenge pair, course/P1-C host identity disagrees, or two single-entry sets are combined into one course run |
| binding absence | no binding set is selected but the snapshot does not encode canonical `None`, or runtime attempts variant configuration anyway |
| variant composition | role/candidate/archetype disagreement, missing resolved profile/digest, or candidate role prefab disagrees with the P1-C mapping prefab |
| reuse invariant | Story/Practice/Challenge select different identity, P1-C payload mapping/gameplay prefab, or base presentation source |
| P2-B course agreement | course entry references an isolated set as product, disagrees with scoped key/variant/configuration capability/receipt identity, or claims P2-A lifetime/cleanup authority |
| growth claim | numeric variant claimed without an approved independent stat owner/closed override |
| scoped binding | missing/duplicate `(stageDefinitionId, spawnId)` or incompatible P1-C payload mapping |
| placement duplication | variant copies anchor, transform, count, delay, payload, group, or completion data |
| configuration seam | missing/unsupported adapter, call occurs outside inactive P1-C staging, course selection/ticket/generation/snapshot receipt mismatch, or ticket activates after failed/unsealed configuration |
| snapshot admission | current assets cannot resolve every semantic input before logical entry |
| scene binding | live capability or current digest disagrees with the entry snapshot |
| terminal coverage | any supported exit lacks complete reverse-order restore/release receipts |
| stale generation | old callback can reapply, reacquire, publish, or affect a new run |
| broad system creep | generic DSL, affix graph, random pool, difficulty scaler, new spawner, new AI framework, or reward path appears |

## First Fixture Freeze Gate

| Required fixture fact | Current state | Required predecessor |
|---|---|---|
| logical route | frozen `OLYMPUS-INVASION-01`, revision `1`, route digest `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9` | completed P1-0 validator; P2-A must consume, not reinterpret, it |
| Station segment/definition | frozen `station_entry_combat` plus `OLYMPUS-STATION-COMBAT-01` definition/binding exist; content/variability binding remains absent | P1-B/P2-A content work without physical-identity change |
| P1-C scoped Add key | no truthful count-one Station Add or payload mapping | P1-C0 through P1-C3 |
| rule identity | no versioned `StageRuleSet` | P2-A0 review after route IDs exist |
| recommendation source | current featured SummonSlot1 intent exists, but not through canonical briefing snapshot | P1-B briefing + P2-A rule snapshot |
| enforced restriction port | no source-scoped action/loadout owner | approve and implement one narrow port; current tutorial binder is insufficient |
| modifier port | no typed apply/remove adapter | choose one exact existing domain and prove prior-state restore |
| enemy triad | no exact P1-C payload or approved Story/Practice/Challenge profile set | freeze after the P1-C Add mapping |
| variant binding/configuration | no versioned reachable binding set, agreeing mapping/candidate prefab, typed inactive-stage port, or receipt | P1-C Add mapping plus P2-A configuration adapter review |
| variability snapshot | missing from P1-A route schema | new-schema P1-A extension |
| quiescence/commit seam | no pre-commit `VariabilityClosing -> VariabilitySealed` barrier, abort attachment, or post-commit dispatch-fault record | new-schema P1-A/P2-A extension |
| active restart product policy | recommendation exists only at roadmap level | explicit allowed reason/phase decision plus P2-B integration |

Descriptive placeholders such as `first-rule`, `pressure-modifier`, or `first-enemy` are not freeze data.

## Bounded Vertical Slice

1. Use the canonical Station segment only, after P1-E ownership has ended.
2. Reuse the exact P1-C count-one `Add` scoped spawn key, payload mapping, and gameplay prefab through one reachable versioned binding set.
3. Add one `RecommendationOnly` featured-summon rule. It proves snapshot and briefing projection but leaves gameplay-cleanup status open.
4. Add one `EnforcedRestriction` only after a source-scoped action/loadout port exists. It must capture a nondefault prior state and preserve other owners' locks.
5. Add one modifier with a typed adapter over one exact existing Station gameplay domain. The current full stage profile is not itself the adapter.
6. Add Story, Practice, and Challenge profiles for the same enemy identity/prefab, plus one inactive-stage typed configuration adapter and receipt. Story binds to the canonical spawn; the other two remain isolated fixtures.
7. Add one entry-time variability snapshot, explicit empty-modifier encoding, and the pre-commit P2-A quiescence barrier.
8. Add the snapshotted active-run restart policy. Actual presentation-time restart completes only with the P2-B lifecycle fixture.

## Acceptance Evidence

### Authoring and snapshot

- every semantic record resolves uniquely at logical route entry;
- when selected, the spine resolves one exact binding set and its complete membership without a second lookup root; otherwise it resolves canonical `None`;
- an isolated or product tutorial-course scope resolves one exact two-member Practice/Challenge set in the sole `variantBindingSelection`; two single-entry fixture sets cannot satisfy that snapshot;
- zero selected modifiers serializes as one canonical empty array and produces the same digest across repeated builds;
- no selected binding set serializes as canonical `None`; adding the first set changes only the new run's revision/digest and never backfills an older snapshot;
- repeated builds produce the same canonical semantic digest;
- a post-entry source edit cannot change the admitted run;
- a semantic edit without revision change fails admission;
- a presentation-only edit changes only the presentation digest;
- no P2-A record duplicates P1-B route, P1-C placement, P1-D objective, P2-B story, or P2-C reward fields.

### Recommendation and restriction

- recommendation-only applies no mutation and reports no cleanup coverage;
- enforced restriction validates all ports before mutation;
- a nondefault prior action/loadout state restores exactly;
- another owner's preexisting lock remains after the P2-A token releases;
- missing/duplicate/foreign/stale action IDs fail before gameplay release;
- success/fail, abort, active restart, disable, destroy, route replacement, and unload initiate at most one release; Replay, Retry, and Lobby observe the already sealed release and cannot release again.

### Modifier

- unsupported adapter or payload fails before mutation;
- apply failure rolls back every acquired domain;
- natural and every terminal path remove the modifier and restore exact prior state;
- stale callbacks cannot reapply after release or affect a new run;
- pre-commit cleanup fault aborts before result publication and leaves the run `ClosureFaulted` rather than falsely disposed; a post-commit integrity fault blocks dispatch and never rewrites the result;
- the modifier never emits, reinterprets, or directly owns Clear/Fail, mastery, progress, reward, encounter order, or a presentation domain.

### Enemy variants

- Story, Practice, and Challenge share one archetype identity and gameplay prefab;
- each profile resolves compatible role/candidate/pattern/deck/elite references and its own digest;
- Story's scoped spawn binding, candidate role prefab, and archetype candidate set agree with the P1-C payload mapping and sole gameplay prefab;
- missing/incompatible/duplicate binding fails before object creation;
- while the P1-C staging root is inactive, each variant produces a distinct expected configuration receipt before activation; changing role/deck/elite changes the receipt/configured digest rather than silently running prefab defaults;
- every configuration receipt carries the exact ticket, P1-C/P2-A and optional course-entry generations plus authorizing selection ID/digest, snapshot/binding identity, canonical receipt digest, and full checksum; only its canonical digest feeds transition/quiescence provenance;
- failed or stale configuration prevents `Armed`/activation and P1-C removes the complete staged root;
- P1-C remains the only spawn, death, cleanup, and completion owner;
- no numeric-growth coverage is claimed before an independent local owner exists.

### Restart and quiescence

- active restart is accepted only for a snapshotted allowed reason/phase and by winning the shared P1-A terminal-or-restart latch before `TerminalFinalizing`;
- disallowed, duplicate, stale, foreign-run, post-outcome, and post-commit requests have no side effect;
- no Clear/Fail summary or offered terminal action is fabricated;
- Clear/Fail reaches `VariabilitySealed` before `CommitRequested`; a closure fault produces one evidence-complete abort, enters `ClosureFaulted`, and publishes no result;
- independently registered P1-E lesson, P2-B course, P1-C execution, P2-A variability, and P2-B presentation barriers all close before old-run disposal/dispatch;
- active restart seals its dispatch record before cleanup, performs actual dispatch only after successful closure/disposal, and seals its one abort record only after closure receipts/fault evidence are known;
- a post-commit P2-A integrity fault uses a summary-external dispatch-fault record and never mutates result or abort truth;
- a pre-commit abort/restart barrier timeout or fault never opens an alternate action/restart, never reports `Disposed`, and leaves the old run in `ClosureFaulted` quarantine; a post-commit terminal-action fault instead preserves `Presented`, the immutable result, and the sealed action while blocking dispatch through `StageDispatchClosureFaultRecord`;
- successful P2-A closure produces one `StageVariabilityQuiescenceReceipt` whose canonical digest covers exact constituent receipt digests and whose checksum protects the full envelope; fault evidence can never substitute for it;
- successful re-entry creates one new run ID from the sealed snapshot-derived target.

### Integration non-regression

- target time remains briefing/mastery intent, not a hard fail timer;
- P1-A result/outcome arbitration is unchanged;
- P1-D objective evaluation and progress state are unchanged;
- P2-B camera/HUD/story ownership is unchanged;
- P2-C writes no reward/progress from P2-A names or application state;
- legacy Story PVE remains isolated and cannot contend after canonical run admission wins the scene lease.

## Ordered Delivery and Priority

| Slice | Value | Cost | Dependency | Main risk | Exit gate |
|---|---:|---:|---|---|---|
| P2-A0 contract/identity | 5 | 2 | approved route/P1-C fixture facts | freezing invented IDs | exact owners, manifests, scopes, and fixture facts approved |
| P2-A1 snapshot + recommendation | 4 | 3 | P1-B briefing, P1-A new schema | mistaking recommendation for enforcement | immutable digest/read model passes; `NoGameplayMutation` explicit |
| P2-A2 enforced rule lease | 5 | 4 | source-scoped action/loadout port | unlocking another owner or leaking restriction | nondefault prior-state and every-terminal restore pass |
| P2-A3 one modifier | 4 | 4 | exact typed apply/remove port | prose-driven behavior or incomplete rollback | one domain applies/removes with generation and barrier proof |
| P2-A4 one enemy/three variants | 5 | 4 | P1-C count-one Add/payload mapping + inactive configuration port | prefab ambiguity, silent default behavior, or second AI framework | same mapping/prefab, compatible profiles, exact binding set, distinct pre-activation receipts |
| P2-A5 active restart integration | 4 | 5 | P1-A dispatch + P1-C/P2-A barriers + P2-B1-B5 owner barriers | pseudo-outcome, double dispatch, stale owner | joint P2-A5/P2-B6 snapshot-policy and full-course E2E restart pass |

Do not reorder P2-A ahead of its P1 predecessors because a documentation contract exists. Re-score cost and regression after P2-A2 and P2-A4 rather than scaling to more rules, modifiers, enemies, or modes.

## Explicit Deferrals

- generic condition/rule DSL or editor graph;
- multiple modifiers, ordering/stacking/conflict solver, random affix pool, rerolls, or roguelike composition;
- hard time limit, revive economy, score, rank, star, or difficulty formula;
- copied PGR/HI3/Aether/Wuthering numeric values, IDs, text, mechanics, assets, or content scale;
- a new growth/stat framework before the current prefab/stat owner is isolated;
- a generic skill-set or precondition database merely to mirror external schemas;
- automatic conversion from `StageEnemyRoleSlot` to a runtime payload/variant;
- new practice/challenge product routes, chapter nodes, progression, or rewards;
- P2-B product-course binding before a separately reviewed bounded Practice/Challenge host-scope extension; isolated validation sets remain test-only;
- Player, Boss, Objective, structure, emitter, cinematic, or tutorial-owned variant binding;
- stage-rule-driven outcome reversal, terminal action availability, mastery, progression, or rewards;
- P2-B presentation/story execution and P2-C settlement logic;
- adapting legacy `PveStageData` raw placements or mutable static context into the canonical route.

## Promotion Gate

P2-A is promotion-ready only when:

- P0 through P1-E predecessor gates are current and accepted;
- the P1-C canonical count-one Add fixture and scoped spawn key are approved;
- exact rule/modifier/binding-set/binding/variant IDs, revisions, semantic/presentation digests, and host fixtures are approved;
- recommendation versus enforcement is explicit;
- enforced rules and the modifier have source-scoped typed ports and exhaustive ownership manifests;
- the entry-time variability snapshot is included in the P1-A route digest and result provenance;
- P1-C payload mapping, candidate prefab, P2-A variant digest, typed inactive configuration adapter, and receipt agree without duplicated placement/lifetime authority;
- Story/Practice/Challenge reuse one enemy identity and gameplay prefab;
- all supported terminal paths pass nondefault prior-state restore and stale-generation tests;
- `StageVariabilityQuiescenceBarrier` closes before result commit and is revalidated by terminal actions/active restart;
- pre-commit cleanup fault aborts without a result, while post-commit integrity fault preserves the result and blocks dispatch through the separate diagnostic record;
- target time, hard limit, active restart, Retry, and Replay remain distinct; and
- no deferred broad system has entered the slice.

Passing the recommendation fixture alone does not close the enforced-rule or gameplay-cleanup gate. Passing the variant composition fixture without an independent numeric owner does not claim stat-variant completeness. Active-restart policy alone does not close the P2-B presentation-time restart E2E gate.

## Open Review Decisions

1. Exact approved P1-C Station count-one Add scoped spawn key and payload mapping.
2. Exact source-scoped action/loadout availability port and the first enforced restriction's stable action IDs.
3. Exact modifier domain, typed adapter, clock policy, and restore fields.
4. Exact versioned binding-set/binding identity, archetype, role, candidate, agreeing P1-C mapping prefab, pattern/deck/elite references, typed inactive configuration port, and receipt fields for the Story/Practice/Challenge triad.
5. Whether the first triad needs a closed numeric override after a real stat owner is isolated, or behavior-only variation is sufficient for the initial product value.
6. Allowed active-run restart reasons/phases and whether first product exposure waits entirely for P2-B.
7. Final identity-inventory format shared across P2-A records without creating another global manifest owner.

## Archive Sources

- `games/punishing-gray-raven/enemies-stages/pgr-broad-enemy-stage-field-summary.csv`
- `games/punishing-gray-raven/enemies-stages/pgr-broad-enemy-stage-sample-rows.csv`
- `games/punishing-gray-raven/enemies-stages/index.md`
- `games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14/files/extracted_repo/PGR_Data-master/EN/bytes/share/fuben/Stage.json`
- sibling `FightReboot.json`, `StageFightEvent.json`, and `../fight/FightEvent.json`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-monster-summary.csv`
- `games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.md`
- `games/honkai-impact-3rd/raw/nairieberry-honkaiimpactdata/2026-06-15/files/extracted_repo/HonkaiImpactData-master/Global/ExcelOutputAsset/Decrypted/StageData_Main.json`
- parent-directory `Global/ExcelOutputAsset/StageChallengeData.json`, `StageReviveCostData.json`, and `MonsterConfigData.json`
- `games/aether-gazer/enemies-stages/aether-gazer-stage-readable-join.csv`
- `games/aether-gazer/combat/aether-gazer-stage-affix-join.csv`
- `games/aether-gazer/combat/aether-gazer-affix-readable-join.csv`
- `games/aether-gazer/combat/aether-gazer-public-buff-readable.csv`
- `games/girls-frontline-2/raw/torikushiii-gfl2data/2026-06-13/files/extracted_repo/GFL2Data-main/tables/StageChallengeData.json`
- sibling `StageChallengeConditionData.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/MonsterInfo.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/MonsterPropertyGrowth.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/AiBase.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/AiBaseSkill.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/AiSkillInfos.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/AiSkillPrecondition.json`
