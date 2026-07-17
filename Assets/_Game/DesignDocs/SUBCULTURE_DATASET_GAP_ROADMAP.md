# Subculture Dataset Gap Roadmap

## Current P1-B closure

- P1-B Station Add and full-exit closure (2026-07-16): `SNAP-P1B-STATION-ADD-AUTHORING-REMEDIATION3-ACCEPTED-11` binds `C:\tmp\DimensionBrawl-P1B-StationAdd-Remediation3-Bundle.md` at SHA-256 `9378bc021b09495c350b331a85755eac7b956a2372d78ecca848a94c2d570c76`; source `128/128` matches digest `4c3dbe952bea5e4f5c57632d70e6fba815d7f6900dc9e1dcbee6af69bae86c89`, artifacts `11/11` match digest `eb5699917083d9be13d571f2a64aa0f69048304552b962df3467b89f3469ce2b`, validator/inventory `8/4/1/1/0`, integrated focused `8/8`, Canonical UI `34/34`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `99/99` all pass with three independent audits at blocker `0`. Revision-1 pose remains relative to `StageDefinitionSceneBinding.transform`; Station `MapRoot` is topology containment only. `ACC-P1B-STATION-ADD-AUTHORING = PASS`; the foreign-evidence row remains PASS through explicit rejection only; `SNAP-P1B-FULL-EXIT-ACCEPTED-12` closes `ACC-P1B-FULL-EXIT-AUDIT = PASS`, so P1-B is **ACCEPTED / VERIFIED-COMPLETE**. This admits no P1-C runtime owner: only the prospective authoring-ledger freeze may start, and runtime work remains gated by `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`.

## Status

- P1-B result/progression Remediation3 acceptance: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION3-ACCEPTED-08` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation3-Bundle.md` at SHA-256 `94fa969979bdb2a2b91dfbdf8a5395aed0a69ddd8907831bb7c99da06b139a5b`; source `116/116` matches digest `271793a22e2afc24779a3aeeace7cb9768aae77b7bbbf18a075fa15ea409efb2`, artifacts `14/14` match list digest `c3642305e13c085f710e8db62df807463aea58d8a57331cd7526460eb7a404fc`, validator/inventory `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `98/98` all pass. Independent source, artifact/test, and semantic-contract audits find blocker `0`: route/sidecar-owned canonical catalog identity is independent of the result definition, public Corridor admission and the editor validator require exact object identity, and catalog-only plus coherent catalog/profile/localization clones reject before run creation. Frozen route/policy/join/lifetime digests remain unchanged. `ACC-P1B-RESULT-PROGRESSION-JOINS = PASS / VERIFIED PARTIAL`; Candidate-07 remains immutable historical FAIL. Station count-one Add authoring is now unheld as the next separate P1-B gate, while live PGR/HI3 disposition, P1-B full exit, and P1-C execution remain OPEN and no P1-D/P2-C owner is admitted.
- P1-B result/progression Remediation2 candidate audit: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION2-CANDIDATE-07` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation2-Bundle.md` at SHA-256 `a4e2e2873ec4f53ba81a6c6a3269949b4b2f19255f566d333fcb058e3eeb6de8`; its submitted source manifest matches `116/116` with digest `f4c6f0a6065a2f304acd1a56f7d126b4b2be49582f752f707757d87f37c35583`, all `14/14` artifacts match list digest `96176b861dc7ce0a9aaccd86fe035aa59433513383713132248e51f974b6228a`, validator/inventory is `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact full route `1/1`, and graphics aggregate `98/98` pass. Independent source/contract/test audits verify that Candidate-06's three blocker groups, locale/graph rows, and exact durable-decision byte preservation are closed, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / VERIFIED-FAILED-CANDIDATE-PARTIAL` on one remaining admission defect: the result definition self-selects its catalog, so a catalog-only clone or coherent catalog/profile/localization clone can evade the intended exact-identity gate. The post-bundle route-owned catalog-anchor WIP changes five submitted files and cannot retroactively amend this cutoff. Station Add and P1-B full exit remain held until a new sealed-source candidate passes.
- P1-B result/progression joint-freeze: `P1B-RESULT-PROGRESSION-JOINS-01` Rev3B proposal artifacts match SHA-256 `b6e63b11e3e270302dc33f95b7b69740565e4e27a13ffe017a17f2899256c88f` / `eb65cf30eb961a271f135bc38a9874cccae49e47d8a9d0af5a6dd5f0d7211199` / `933c13943e5397f5fa7a1be531ae34bd28f595e09feee14f18429daa81a8e603`. Fresh PowerShell, independent Node, and a third row reconstruction preserve the seven `15/35/15/17/8/9/38` blocks, sidecar/join snapshot digest `a2ae9df451bd6f2ff48b83098db3bfbdaf2120e23dfaf3612a31f18a022c41fa`, all predecessor digests, and the separate 11-row lifetime-contract digest `3b6cf33325a0a83db74ee2253da9799e589b5664f4fb677b2b021389b0714c0e`. Exact `(ID, revision)` edge resolution and the no-token `Stage Select A -> pre-admission mutation B -> fresh Corridor B` boundary pass. Verdict is **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**. This authorizes implementation only: `ACC-P1B-RESULT-PROGRESSION-JOINS`, Station Add, foreign evidence, and P1-B full exit remain **OPEN**, and no P1-C/P1-D/P2-C owner or P1-A digest change is admitted.
- P1-B result/progression Rev3B implementation candidate audit: `C:\tmp\DimensionBrawl-P1B-ResultProgression-Implementation-Bundle.md` matches SHA-256 `35b1b1a5523bc457ad1936190d1d41143dd1bc8a3489624cdb600631c3a6daa1`; submitted source manifest `116/116` matches digest `1b3dba021b40a4be9d728c6fd4f2039864abb399bbff6d2907e4af274bec24ec`, all `14/14` declared artifacts match list digest `249da60824d3ef617937e648e1257b1fde9b50dc28082a904b78513ca7c76023`, both contract verifiers pass, validator/inventory is `8/4/1/1/0`, focused `2/2`, Canonical UI `28/28`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `93/93` pass. These green artifacts are verified, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / SOURCE-CONTRACT-FAILED-CANDIDATE`: canonical profile/localization object identity is not enforced at admission, the `Presented -> terminal action` path omits the exact pinned join/presentation/audit authority gate and audit self-integrity, and representative deep snapshot damage can throw instead of returning a typed rejection. Direct clone/damage/dispatch, recovery/process-loss, locale, and production graph acceptance rows remain open. The Rev3B joint freeze and every accepted predecessor cutoff/digest remain unchanged; Station Add and P1-B full exit stay held pending remediation and a new sealed-source bundle.
- Started: 2026-07-13
- Configured source root and current read-first authority: `\\DESKTOP-69817L3\ArkData\SubcultureGameData`
- Historical section-level review recorded: Punishing: Gray Raven, Honkai Impact 3rd, Aether Gazer, Blue Archive, Wuthering Waves, Arknights, Ash Echoes, Reverse: 1999, Limbus Company, and Last Origin
- Historical bounded narrative recorded: CounterSide, the Girls' Frontline family, Path to Nowhere, Fate/Grand Order, Heaven Burns Red, Epic Seven, Brown Dust 2, Princess Connect! Re:Dive, and Stella Sora
- Historical presentation-lifecycle narrative recorded: Genshin Impact; Brown Dust 2 retained only as a third-party viewer cleanup boundary; Honkai: Star Rail retained as an insufficient-evidence boundary
- Historical supporting cross-check narrative: Zenless Zone Zero, NIKKE, and Girls' Frontline Neural Cloud client-flow material
- Historical indirect QA-only note: Snowbreak MAA material
- Latest Ark access boundary: direct SMB read access was re-established on 2026-07-16 for both `ArkData` and `SubcultureGameData`; curated read-first summaries, rollups, source maps, and bounded rows are now the internal structural-research authority. The separate retained mirror `C:/Ark/SubcultureGameData` remains only the replay source for the earlier bounded PGR/HI3 five-row candidate. Four versioned PGR semantic successors, three versioned HI3 successors, and two byte-exact replay-authenticated HI3 helpers remain verified but unadmitted. The factual license-signal audit is not a permission conclusion, and policy/rights disposition plus one atomic eleven-source admission remain open, so the formal product-evidence cohort stays `0/9`; that narrow promotion state does not block non-copying archive comparison.
- Historical remaining-archive narrative recorded: Stella Sora remains community structural/negative transaction context; no further dataset enters the deep queue without stronger source material
- Registry boundary: outside the separately recorded PGR/HI3 and local historical-control records, these game sections are not exact admitted source/claim records in the current evidence index. Their prose is comparison context, not reproducible live-source evidence or a source grade.
- Current product flow: two route segments `OlympusCorridorInvasionStage -> OlympusStationCombatStage`, followed by separate additive committed-result presentation `UI_StageClear`, after the 2026-07-13 split-route correction
- Evidence snapshot: accepted P0 baseline is `full PASS / natural PASS / retry PASS / lobby PASS` at 28/28. P1-0 route/Station validation passes with mutation inventory `8/4/1`, authorized core `1`, bypass `0`, frozen route/policy digests, and aggregate 37/37. P1-A's 45/49/54/59/68/75 cutoffs remain separate non-additive evidence. The final current-schema cutoff matches all 11 sources under ordered manifest digest `e59884ca0bcbec0506502ccb2638d9227e5f098bfb7f271e3a7adf16a2656427`; Combat 21/21, StageRun 23/23, canonical UI 15/15, aggregate 79/79 with class counts 15/21/3/1/16/23 and 79 unique full names, exact full route 1/1, and compile/validator all pass. It closes exact abort duplicate identity, direct replacement cancellation/provenance, exact diagnostic provenance, and final-snapshot exception closure. P1-A current-schema full exit is **PASS/CLOSED**; P1-B becomes the active product wave. Future P1-E/P1-C/P2-B admitted-owner receipts remain separate later-schema evidence.
- P1-B direct-presentation cutoff: the first accepted bundle matches 11/11 sources under manifest digest `38ea238f58adbc49bbf6f0ac7c1ffd846bc4bc5c549fc0dbfcf542f88af72990`; the missing-arm negative, final validator, focused identity 1/1, natural ActualPlayPath 2/2, exact full route 1/1, and graphics aggregate 80/80 verify the Corridor-entry definition/profile/combined-Timeline/port/director/existing-flow identity join.
- P1-B presentation-residue cutoff: the second independently audited bundle `C:\tmp\DimensionBrawl-P1-B-PresentationResidue-Bundle.md` matches SHA-256 `6098a3fc32e74990fbabb9ddfe5b1a6b951a4b5aba07557e2273da94558a907c`, and its 11-source manifest matches digest `bbde61085d0801886ec33c1741561c7262449ef666246727ecd63109b14b753f`. Two intentional negatives fail at the intended binding/port boundaries; final current bindings are 39 with zero null/stale/unresolved rows; two unowned `StageCutscenePort` components are removed while their payload GameObjects remain; validator, focused 1/1, ActualPath 2/2, exact `FullRoute-2` 1/1, and graphics aggregate 80/80 pass. `FullRoute-1` is excluded because it ran zero tests. This closes port inventory and current Timeline binding integrity only.
- P1-B anchor/profile cutoff: the third independently audited bundle `C:\tmp\DimensionBrawl-P1-B-AnchorProfile-Hygiene-Bundle.md` matches SHA-256 `1a88295b46c43658c964589c554d8286c40ae2f132036204c5fb6fd7b1e7e8e7`, and all 19 present plus 4 absent manifest rows match digest `7116d6430ce78b11d5e5f1553559e36c0f3e2372febdd8f96b1636ca22e7cd84`, including the closing rehash. Three intentional negatives cover Corridor 10-versus-4 anchors and both zero-resolve profile contexts; final Corridor/Station inventories are 4/4 and 0/0, payload remains after six component removals, both zero-reference profiles are absent, validator and frozen digests pass, accepted `Focused-2` 1/1, ActualPath 2/2, exact Replay full route 1/1, and aggregate 80/80 pass; erroneous `Focused-1` is excluded. This closes `ACC-P1B-PRESENTATION-ANCHOR-PROFILE-DISPOSITION` only. P1-B remains **VERIFIED PARTIAL** and full exit **OPEN**; catalog/template/briefing/result/progression/Station Add/foreign rows remain.
- P1-B canonical-selection rejected candidate: the separate bundle `C:\tmp\DimensionBrawl-P1-B-CatalogProjection-Bundle.md` matches SHA-256 `078208742bf4033b40543f032bf2a0012c2b2aa52063be438baacadefbf51771`; 19/19 sources reproduce manifest `83efd18b0658a501f7472556493431be68b453bb97dc94b79e3af7f0d3616183`, projection digest `3a2d630f34c6518b8783bcffaf4ac0c21be1a97cbff8e80372b26bec3537549c` reconstructs, and three negatives, validator, focused 6/6, UI 19/19, exact `FullRoute-2` 1/1, and aggregate 84/84 all match. It does not close `ACC-P1B-CANONICAL-SELECTION`: the submitted prefab has no bound authored reward Text row to preserve/hide, and submitted `SelectStage(null/empty/whitespace)` returns before invalidating its old bundle/latch. This is a rejected fourth snapshot, not a fourth accepted cutoff.
- P1-B canonical-selection accepted remediation cutoff: the separate Candidate-05 bundle `C:\tmp\DimensionBrawl-P1-B-CatalogProjection-Remediation-Bundle.md` matches SHA-256 `2b71350f7e16c54503e03a64c13cb9a04fff3aea3b9fe05799168db1ddabf8b6`; 19/19 current sources reproduce manifest `05da141460d851ffaaf9a5d1a52fbab9932c2a0d2c1b252e8c4b43b0e2a01dfa`, and the same frozen projection digest reconstructs. The actual prefab preserves/binds one empty inactive `CurrentChapterRewardText`, and the null/empty/whitespace/unknown matrix proves old projection/latch invalidation plus disabled Start and zero router request/event/non-null-SFX/run/admission/abort. Validator, focused 8/8, UI 21/21, exact full route 1/1, and graphics aggregate 86/86 pass. `ACC-P1B-CANONICAL-SELECTION` is **PASS** for Candidate-05 only; Candidate-04 remains historical FAIL. This is the first accepted catalog cutoff and fourth accepted local P1-B subgate, not a fourth presentation cutoff. At this cutoff, the reference-block/template/briefing joint freeze and P1-B full exit were still **OPEN**; the later rev2A audit freezes only that contract while full exit remains open.
- Historical first P1-B truthful-join proposal audit: `C:\tmp\DimensionBrawl-P1-B-TruthfulJoins-Contract-Proposal.md` matches SHA-256 `e5305d04937991e7120bb5edc8cd61905c4df923c689adc923c3df65fca9fe5d` and is **AMEND / PROPOSAL ONLY**. It is not a sixth cutoff, an implementation authorization, or a joint freeze. Its direction and two-segment/three-pocket topology became inputs to the later rev2/rev2A audits. Candidate-04 remains historical FAIL and Candidate-05 remains the first accepted catalog cutoff.
- P1-B truthful-join implementation cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1B-TruthfulJoins-Implementation-Bundle.md` matches SHA-256 `8ef3a8e234f53ef561dfdd5d805d0f69c8ddbb55d2a2534ca427f2da821a9d0a`; all 51 ordered source hashes match manifest digest `1d2fc6a142fa7582e76095c8a928ca1f61f4453ac7061f5d50525673d1480324`, and all 13 declared artifacts match. PowerShell and Node independently reconstruct `71/27/80`; the validator passes `8/4/1/1/0`; focused 7/7, canonical UI 26/26, exact full route 1/1, and graphics aggregate 91/91 pass with 91 unique full names and class counts `26/21/3/2/16/23`. Frozen route, policy, projection, template, reference, and briefing digests all match. `ACC-P1B-TRUTHFUL-JOINS` is **PASS / VERIFIED PARTIAL**; P1-B full exit remains **OPEN**. At its later historical cutoff, Candidate-06 fails `ACC-P1B-RESULT-PROGRESSION-JOINS` on three blocker groups. Remediation2 Candidate-07 subsequently closes those groups but still fails one independent canonical-catalog identity anchor; a new sealed-source candidate is next, then Station Add, live PGR/HI3 foreign evidence, and full exit. This adds no P1-C execution owner, result/progression/reward join or owner, or pre-result active-run restart.
- Durable/finalization scope: the local atomic-file stored decision and receipt are schema 2 and compare `resultSummaryDigest + exact TerminalFinalizationOwnerCoverageRecord ID/digest + NotRequired preparation`. The route/run remains schema 1 and the current finalization factory accepts only `NonCourseStationTerminal`, with fixed P1-E lesson, P2-B course, P1-C execution, and P2-B presentation rows truthfully `NotAdmitted` and zero pending. Same-process identity, cache-clear byte-equivalent recovery without a live scene coordinator, conflict/corruption preservation and quarantine, direct transient read/write retry/reconciliation, and UI suppression before exact reconciliation are covered. This creates no P1-D progress or P2-C settlement persistence and proves no future admitted-owner closure.
- Fact/presentation scope: the revision-1 Olympus route seals explicit guide lifecycle, route-owned clocks, the seven-row pre-load tutorial summary, two segment results, damage/down/dodge/summon facts, source-qualified proofs, and a closed `StageOutcomeFact`; `resultSummaryDigest` covers that payload. The 45/49/54/59/68/75 artifacts retain their bounded historical meanings, and 79/79 closes the current-schema exit without creating later-schema owner receipts.
- Current P1-A review artifact: [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md)
- Current P1-B review artifact: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md)
- Current P1-C review artifact: [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md)
- Current P1-D review artifact: [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md)
- Current P1-E review artifact: [Tutorial Lesson, Attempt, and Gameplay Reset Spec](TUTORIAL_LESSON_ATTEMPT_RESET_SPEC.md)
- Current P2-A review artifact: [Stage Rule, Modifier, and Enemy Variant Spec](STAGE_RULE_MODIFIER_ENEMY_VARIANT_SPEC.md)
- Current P1 product approval surface: [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md)
- Current P2-B presentation-handoff sub-contract: [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md)
- Current P2-B course-chain sub-contract: [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md)
- Current P2-C review artifact: [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md)
- Current non-proprietary dataset evidence registry: [Subculture Dataset Evidence Index](SUBCULTURE_DATASET_EVIDENCE_INDEX.json)
- Current machine-readable execution registry: [Subculture Gap Backlog](SUBCULTURE_GAP_BACKLOG.json); lifecycle and implementation state are separate axes, evidence packets have a separate score profile, and the 30-item dependency graph is the authoritative execution order for this long-term goal
- Project baseline: current DimensionBrawl workspace, including uncommitted stabilization work
- This document is a long-term decision ledger. It does not authorize immediate feature expansion.

## Objective

Use source-anchored game datasets to identify which production structures DimensionBrawl is missing, which existing structures should be expanded, and which reference patterns should be rejected because they dilute the summon-first fixed-rear combat identity.

The output is an ordered backlog, not a feature shopping list.

## Current Executive Decision

| Decision | What belongs here now | Why |
|---|---|---|
| Protect and verify | Fixed-rear forward-risk summon combat, real-event tutorial validation, existing AI role/deck/elite layers, action/cinematic camera systems | These are already meaningful strengths; comparison games do not show that DimensionBrawl needs a replacement core |
| Add first | Preserve the closed P1-A boundary and accepted canonical reference/briefing/result-progression joins; next close Station Add authoring and the remaining P1-B exit before one route-to-spawn execution bridge and later typed mastery | These are the missing links between the current good combat slice and a repeatable stage game |
| Expand carefully | Tutorial presentation/attempt/loadout/course layers, stage rules, one modifier, enemy runtime variants, and thin story handoffs | Current implementations work but are scene-bound or not reusable across story/practice/challenge contexts |
| Add only after proof | Persistent progression, conditional first/mastery rewards, and one growth action | They become valuable only after stable stage IDs and truthful run results exist |
| Hold | Stamina, random drops, shops, gacha/equipment breadth, roguelike affix graphs, daily/liveops shells, generic score/combo ranking | High scope and identity risk with little current demo or replay value |

Comparison verdict: the main shortfall is not originality or lack of combat systems. It is the production spine that turns the original summon-answer combat into authored stages, truthful results, replayable practice, and safe progression.

### P1-B truthful-join proposal audit — historical amendment, rev2A joint freeze

The first proposal remained proposal-only. Its required revision-2 direction was to keep reference/template schema and revision `1`, add explicit briefing schema and revision `1`, preserve the frozen route/policy/catalog/projection digests byte-identically, and use exactly one new current-route template rather than reusing `S1-1` through `S1-5`. Rev2A now freezes that amended direction as recorded below.

The approved/amended stable identities are:

- template `olympus-invasion.tutorial-station-run`;
- segments `olympus-invasion.corridor-tutorial` and `olympus-invasion.station-guide-combat`;
- pockets `olympus-invasion.corridor.core-tutorial`, `olympus-invasion.station.replica-summon-guide`, and `olympus-invasion.station.boss-encounter`;
- distinct Corridor source provenance `olympus.corridor.core-tutorial` revision `1`, semantic digest `b1b00dd84e27fe8d06c6736d85b16ff6bfe141b7ccb70b01ea851144dd8182f2`.

The approved Korean copy is title `기억의 회랑`, objective `하층 세계에서 발생한 차원의 미세한 균열.` plus `그 징후의 진원지를 조사하라.` separated by an actual LF, and combat lesson `회랑에서 근접 공격, 이동, 원거리 전환과 사격, 회피, 표적 정리를 차례로 익힌다. 정거장에서는 레플리카 지급과 소환 안내를 확인한 뒤 보스 격파를 목표로 한다.` Rev2A freezes these strings as narrative briefing contract values only; no reward, power, mastery, difficulty, or enemy-role truth is implied.

The first submitted digest proposal was not freezeable. Its required successor had to expand the 24-row reference list to 27 by adding exact entry port, stage-anchor, and runtime-state rows; publish complete ordered template and briefing row/value sets; separate `currentExecutionOwnerDisposition=ExistingSceneOwner` from `p1cAdmissionDisposition=NotAdmitted`; include all typed-absence payloads, exact segment/pocket/source order, and complete outcome-policy actions; and publish independently reconstructed lowercase SHA-256 template/reference/briefing hashes. Rev2A satisfies those requirements plus the active-run-restart typed absence. The canonical calculation order remains template, reference, briefing, with no self-hashed briefing digest. The frozen catalog projection digest stays unchanged only while the Start latch covers the exact projection instance, catalog generation, and all four projection/reference/template/briefing digests.

The two segments and three pockets are sufficient truthful topology, so no speculative fourth pocket is permitted. The Station boss-encounter pocket is only a future Add-binding candidate. A separate Station authoring slice still needs exact spawn/payload IDs, `Add`, count one, finite delay, static/live anchor and `anchorGroupId`, binding-root-local pose, `CombatSpawn` usage and position ID, noncollision with boss/result/cutscene/guide/terminal owners, and complete P1-C resolver/lifecycle evidence. A later admitted product encounter binding changes encounter membership and must bump the route revision/digest.

The next P1-B order is fixed as: truthful joins and Remediation3 result/progression joins are complete; next close the separate Station count-one Add authoring gate, live PGR/HI3 foreign-evidence disposition, and finally the P1-B full-exit audit.

The first proposal above remains the historical AMEND record. The submitted 71-template / 27-reference / 78-briefing revision 2 also remains historical **AMEND**: although both PowerShell and Node reconstruction matched its three proposed digests and its IDs, copy, enum ordinals, typed-empty payloads, Station guide/boss source arms, presenter/Start tuple, and three-pocket topology passed, it omitted the current pre-result active-run-restart absence. Its immutable proposal/generator/PowerShell-verifier/Node-verifier hashes are `491d72ec79260201d3d23cb203cf043ceb566487d839792c13466488ab69235d`, `2f36e21782bb8eb79253840f2328629160c80c8fa416688929cb76f25321efe9`, `760bc5d7fa666827a80a71324a9a28916896a3e8b39f5f4c468f073ac2d93673`, and `473a42e8196c9740b56e17cb720c72958361ef3ffb584d910c025cf495fa46e0`.

The separate rev2A artifacts match hashes `21f7cbda4fe767ec7c2b29cd7d24cf00af6432b0bee858216c8a318d3b4f678f`, `1c60f44ef70a6e8ff7dc1595e7f7fa50951535ca7b93dd4f01a36b4282543be9`, `d85b7d3d3d83ad98b7fbd768ce13ba522d00a5797a9bdcd7dc16a77d7b767cda`, and `aef9bffcf077979a8d10692ecd2b2b4ea05eeddd0d3a3230e965309b638ada06`. They insert `briefing.activeRunRestartPolicyDisposition=3` and `briefing.activeRunRestartPolicyDigest=<EMPTY>` after story-exit and before action count. Independent reconstruction freezes template 71 rows / `3eec8a5f94c4dfd47ae9255a49ff3b5961d5130cf386f2c6ba96b0525c502e55`, reference 27 rows / `b93e1e23845983c3abdb2e13f551e66025942e40ddfde1a2b123054a65db0791`, and briefing 80 rows / `71b17e4c39364da14aa1deb0906b87eb88ed44e1242723a3b5b76064f2a89f60`. `P1B-TRUTHFUL-JOINS-01` rev2A remains **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**. The active-run restart policy remains currently absent and belongs to later P2-A/P2-B work. The Station Add carryforward must prove non-interference with Station guide and terminal-resolution owners in addition to boss/result/cutscene ownership. The separate implementation cutoff above passes `ACC-P1B-TRUTHFUL-JOINS` without adding P1-C execution, result/progression/reward ownership, or pre-result active-run restart.

## Guardrails

- Stabilization and second-round demo readiness remain ahead of expansion.
- Do not copy proprietary assets, source code, exact formulas, dialogue, stage layouts, UI art, animation, or audiovisual content.
- Extract reusable data shapes, authoring boundaries, pacing relationships, validation rules, and player-facing flow patterns.
- A reference pattern is promoted only when it solves a demonstrated DimensionBrawl gap.
- Preserve DimensionBrawl's fixed-rear boss-barrage, forward-risk energy, and summon-answer identity.
- Do not import PGR signal-orb, three-ping, QTE, or HI3 equipment/gacha structures merely because the datasets expose them.
- Treat community repository data as structural evidence, not authoritative shipped runtime behavior.

### Dataset evidence reproducibility lane

Direct SMB access to `\\DESKTOP-69817L3\ArkData\SubcultureGameData` was re-established on 2026-07-16 and is now the archive-reading authority; `C:/Ark/SubcultureGameData` remains only a retained replay mirror for the earlier bounded candidate. The Ark README explicitly separates collection/read-first research from reuse review: public development material may remain `license: unknown` / `reuse: review-needed` without blocking internal structural comparison, while copying raw bytes, authored content, or formulas into product assets remains a separate prohibited or reviewed action. Therefore the narrow nine-candidate registry is a **product-evidence promotion gate**, not a prerequisite for reading the archive or deriving non-copying gap hypotheses.

The direct curated baseline is materially broader than that old five-row packet. PGR exposes four validated read-first axes of `1,800` rows each (`7,200` total), with duplicate IDs/headers and required provenance gaps all zero; its stage folder additionally inventories `21,980` client-code stage/enemy candidates, `64,034` tutorial-focus rows, `4,206` joined tutorial-context rows, and eight compact exact `GuideFight -> Stage` links. HI3 exposes four validated runtime-context axes of `1,200 / 1,400 / 900 / 1,200` rows (`4,700` total), likewise with zero duplicate/provenance failures. These curated packs authorize bounded internal comparison across stage/combat, camera/cutscene, rendering, and UI/presentation. They still do not prove shipped runtime behavior or authorize source-content reuse.

#### Direct Ark gap pass 1 — stage/combat and presentation dependencies

This pass uses curated rows and their source maps, not a broad raw-payload scan. The current worktree contains the accepted Remediation3 result/progression sidecar but no admitted `EncounterGroupSequence`, `StageVariabilityPlanSnapshot`, `StageRuleSet`, `EnemyVariantProfile`, `TutorialAttemptResult`, `TutorialCourseDefinition`, or persistent `StageProgressState`. The external rows therefore sharpen ordering; they do not prove those owners already exist or bypass local acceptance.

| Order | Gap / bounded action | Direct Ark evidence | Player and production effect | Cost | Dependencies | Principal risk / negative boundary |
|---:|---|---|---|---:|---|---|
| 0 | Preserve the accepted P1-B result/progression join and keep unowned briefing fields typed absent | PGR's exact stage rows expose optional record time, story, NPC/loadout, recommendation, restriction, restart, objective, and reward-shaped fields, but peer presence supplies no DimensionBrawl authority | preserves one truthful stage identity and prevents UI/catalog text or a peer field from manufacturing product truth | 2 | accepted Remediation3 source-contract cutoff | filling every peer field now would create fake power, loadout, reward, or story owners |
| 1 | Author one real Station `Add` and close the minimal P1-C ordered encounter execution bridge | PGR inventories `21,980` stage/enemy client-code candidates and a curated 720-row direct stage/enemy runtime slice spanning wave/spawn, stage config, guide/fuben, map/room, DLC fight, and boss families | converts the current one-off scene into reusable stage content and unlocks a genuinely distinct second short stage | 3 | accepted P1-B; exact Station spawn/payload/anchor; current run cancellation and quiescence | static source lines do not prove runtime order; do not copy PGR actions or build a generic event graph |
| 2 | Separate tutorial stage identity, overlay/control steps, attempt result, reset, and later course entries | PGR has eight exact `GuideFight -> Stage` links while explicitly keeping `GuideFightStep` separate; `64,034` tutorial-focus and `4,206` context rows cover guide, practice, course, teaching, world, stage, and character references | makes lessons retryable and measurable without turning UI presentation into proof; supports Basic -> Practice -> Challenge later | 4 | accepted P1-C facts/execution and P1-D fact semantics; current director parity | do not import signal-orb/three-ping/QTE or infer a step-to-stage join that source evidence does not establish |
| 3 | Add one immutable stage-rule / enemy-variant snapshot and one source-scoped adapter | HI3 read-first contains 390 stage-condition/buff rows and 240 monster AI/stat rows; PGR separately exposes stage config and enemy/boss runtime families | provides bounded replay variation and authored enemy roles without duplicating spawn/lifetime ownership | 4 | accepted P1-C payload authority and stable P1-A facts | static conditions/stats do not prove live mutation order; no broad RNG, affix pool, or difficulty scaler |
| 4 | Generalize presentation ownership only through one existing cinematic/UI fixture | PGR read-first has 1,800 camera and 1,800 UI rows dominated by timeline camera and guide-overlay flow; HI3 has 1,200 camera/dialog rows and 1,200 UI rows including 802 open/close transitions | closes skip/cancel/unload/retry restoration and improves perceived stage continuity | 4 | P1-B full exit, stable P1-E/P1-C lifecycle, current build/perceptual capture | row counts do not justify a new generic cinematic/UI framework; static bindings do not prove perceptual quality |
| 5 | Admit mastery/progress and then only a demand-backed minimal reward transaction | PGR stage surfaces and HI3 skill/weapon/stage tables expose progression-shaped authoring, but they do not establish DimensionBrawl persistence, replay demand, or settlement correctness | gives clear meaning and durable replay progression after the playable loop is reusable | 5 | accepted P1-D store/migration, measured replay demand, P2-C single-store settlement | never copy peer economy, gacha, numeric thresholds, reward tables, or client-only grant order |

The dependency order remains product-led: completed `P1-B -> prospective P1-C authoring-ledger freeze -> P1-C runtime -> P1-D/P1-E -> P2-A/P2-B -> demand-gated P2-C/P3`. Direct Ark breadth raises evidence strength and exposes missing owners, but it does not justify skipping local persistence or lifecycle gates.

#### Cross-game consensus pass 2 — common structure, not row-volume imitation

This pass uses each pack's per-game rollup presence instead of treating raw row count as product importance. The combat/stage pack contains `10,686` cards across `36` rollup keys, camera `13,453` across `37`, UI `9,385` across `50`, and rendering `11,468` across `35`. The broader coverage matrix reads `490,647` master rows and reaches all four domains for all `36` normalized keys, but `4,178 / 6,453` game-domain-signal rows are still classified `thin`. These are automated screening denominators: aliases and reference-only pseudo-game keys remain in some packs, and classifier selection can produce false negatives. Presence supports a structural comparison; absence from a selected pack is not proof that a game or DimensionBrawl lacks a feature. PGR's automated `stage-wave-spawn=0`, for example, is overridden by its separate direct pack with `4,114` wave/spawn candidates and a curated `260`-row wave/spawn slice.

| Shared structure family | Per-game presence in the curated rollup | Action-game examples in this pass | Current DimensionBrawl state | Roadmap decision |
|---|---:|---|---|---|
| target selection / AI | `33 / 36` | PGR, HI3, Aether Gazer, Wuthering Waves, ZZZ, Snowbreak | player target lock, AI roles/decks, encounter coordination, and terminal authority already exist | protect and validate; do not replace the combat core |
| status/buff semantics | `26 / 36` | all seven selected action references | local health, damage, dodge, summon, and pressure facts exist, but no admitted generic modifier owner | keep effects source-scoped; admit one P2-A modifier only after P1-C |
| projectile / hit / collision | `25 / 36` | all seven selected action references | multiple authorized damage producers and telegraph/projectile families already exist | preserve the `8/4/1/1/0` mutation boundary; no generic payload rewrite |
| skill / action payload | `18 / 36` | HI3, Aether Gazer, Wuthering Waves, ZZZ, Snowbreak, Genshin | player, summon, boss, and enemy action layers already carry the original combat identity | extend through authored stage payload bindings, not peer mechanics |
| stage wave / spawn | `9 / 36` automated, plus direct PGR corroboration | Ash Echoes, HI3, Wuthering Waves, Genshin, Stella Sora; PGR is supported separately by its direct stage pack | one boss-specific `FrontlineWaveStageProfile` exists, but the canonical Station route still has no admitted ordered encounter group or real count-one `Add` binding | highest missing content-spine priority: Station Add, then minimal P1-C |
| enemy loadout / entity placement / route trigger | `5 / 36`, `3 / 36`, `1 / 36` | strongest here in HI3/Wuthering Waves and bounded direct PGR rows | no admitted route-owned variant snapshot, placement plan, or generic route graph | use one explicit sequence and one immutable variant later; sparse evidence rejects a broad graph framework |
| timeline camera / camera presentation / transition | `25 / 37`, `20 / 37`, `16 / 37` | PGR, Aether Gazer, Wuthering Waves, ZZZ; Snowbreak has timeline evidence | rich action-camera, cinematic-sequence, Timeline, screen-cue, and target-camera code already exists | P2-B should close ownership, skip/cancel/unload/retry restoration, and reusable binding—not build another camera engine |
| battle-skill UI / story flow / UI motion / cut-in | `38 / 50`, `24 / 50`, `23 / 50`, `22 / 50` | PGR, HI3, ZZZ, Genshin; Snowbreak supplies story/motion evidence | HUD, tutorial overlay, result shell, prompts, and cinematic overlays exist but remain split across stage-specific presenters | after P1-C/P1-E, unify lifecycle and truthful read models; no mandatory ultimate cut-in or character-marketing shell |
| material references / screen transition / toon / postprocess | `29 / 35`, `22 / 35`, `21 / 35`, with LUT/vignette `16 / 35` and bloom `14 / 35` | broad across PGR, HI3, Aether Gazer, Wuthering Waves, ZZZ, Snowbreak, Genshin | mobile render budget, runtime screen cues, target outline, telegraphs, and per-effect materials exist; worktree search finds no shared route-owned postprocess profile | retain as a P2-B visual-consistency follow-up behind content; require mobile budget and perceptual captures before promotion |
| reward / schedule / mission surface | `2 / 36` in this runtime pack | not a broad action-runtime consensus signal here | static result/progression identity is accepted, while durable progress, grants, and settlement remain absent | keep P2-C demand-gated; peer economies or low pack coverage cannot manufacture reward authority |
| development / release operations | unmeasured by these four domain packs | Snowbreak MAA and Stella Sora automation rows are external tooling evidence, not shipped-client operations | local build/performance tooling exists, but authoring ledgers, release activation/rollback, diagnostics/privacy, and support/incident ownership remain open | keep operations locally measured; do not infer liveops or production readiness from reward/schedule rows |

The consensus changes the interpretation more than the dependency order. DimensionBrawl is not missing the genre's baseline combat, camera, HUD, or effect vocabulary; it already has unusually deep local implementations of those families. The common missing layer is the **repeatable content spine** that binds a truthful stage selection to ordered spawn/payload execution, facts, result, retry, and the next authored stage. P1-B now closes that spine's static-authoring boundary. The near-term sequence is `prospective authoring-ledger freeze -> minimal P1-C runtime`. P1-B's accepted static Station Add remains outside that prospective clock. After the accepted first pocket, preserve its measured baseline and record only a non-binding short-second-stage target hypothesis. Continue through `P1-D/P1-E`, one bounded `P2-A` variant, and lifecycle-focused `P2-B`; only then freeze the exact second-stage candidate contract, deliver it, and perform the matched throughput comparison. P2-C rewards and P3 scale remain evidence- and throughput-gated.

#### Cross-game stage-authoring and operations pass 3 — execution truth before content volume

This pass freezes a research-only 13-file Ark cutoff by two consecutive matching SHA-256 reads. Its master summary contains `494,247` index rows, `494,247` source-map rows, and `12,234` rollup rows across `36` normalized keys and four domain axes. The dedicated combat/stage pack remains `10,686` curated cards; the broader enemy/stage focus contains `1,033,604` rows from `99` sources. These volumes are navigation surfaces, not feature votes. The current coverage matrix is one `1,800`-row generation behind the master, aliases/reference keys remain in some denominators, and PGR's automated wave/spawn zero is directly contradicted by `4,094` candidates and a curated `260`-row direct slice.

| Structure | Bounded Ark support | Current DimensionBrawl truth | Priority decision |
|---|---|---|---|
| stage topology and ordered execution | direct PGR, Arknights, NIKKE, Blue Archive, ZZZ, GFL2, Ash Echoes, and Stella Sora packs repeatedly separate rooms/maps, placement or wave rows, enemy/loadout rows, and terminal-shaped state | route/schema/validator depth and Remediation3 joins are high, but Station still has zero canonical anchors/spawns and no accepted portable pocket executor | author the exact count-one Station Add, then close minimal P1-C; do not build a generic event graph |
| authored plan versus runtime owner | config/formation/route rows are distinct from spawn, lifetime, cancellation, and terminal execution surfaces | P1-B truthfully describes three pockets while every P1-C admission remains typed absent; the legacy PVE executor is noncanonical | keep authoring identity, execution lease/generation, completion, cleanup, and quiescence as separate receipts |
| mission/objective versus combat outcome | Redive objective/schedule, ZZZ quest/mission, FGO quest phase, and Uma objective rows are separate from battle terminal data | title/objective copy and semantic proof rendering exist, but there is no canonical MissionDefinition, objective evaluator, or objective persistence owner | derive future objectives from committed facts; presentation never creates completion, and objective generalization stays after P1-C fact provenance |
| result, progression, and reward joins | progression focus spans `204,284` rows and `13` games but is `88.1%` MementoMori and mixes account/profile with character growth; FGO/Genshin/Redive reward rows are downstream and genre-specific | P1-A result/receipt is deep and Remediation3 accepts the P1-B static result/progression identity join; durable player progress and reward settlement remain absent | preserve typed node/graph/read models, then add P1-D persistence; P2-C remains demand-gated and separately transactional |
| replay/retry/checkpoint policy | no dedicated curated replay pack or exact common replay policy was found | current Clear Replay/Lobby and Fail Retry/Lobby are product-owned and verified under P1-A | preserve the local minimal policy; do not import costs, checkpoints, fast-clear, or repeat rewards without a targeted cohort and product demand |
| rule/modifier variation | HI3 stage conditions, Aether Gazer affixes, ZZZ modifier lifecycle, and GF2 rule/presentation coverage support typed optional rule surfaces | no admitted route-owned variant snapshot or generic modifier owner | one immutable source-scoped P2-A variant after P1-C; broad affix pools and RNG graphs remain held |
| production operations and performance | no dedicated cross-game development/release-operations pack exists; Snowbreak's `631` automation rows and HBR's `432` fan-reproduction rendering-setting rows are tooling/config proxies only | Android benchmark/build readers exist, while CI/release manifests, diagnostics/privacy, support/incident, authoring ledgers, and rollout/rollback remain open | measure DimensionBrawl directly: prospective ledgers, repeated stage-loop device soak, and hash-addressed release/rollback evidence; never promote peer tooling into runtime authority |

This pass also closes five roadmap consistency gaps without changing product acceptance: the P1-C authoring snapshot now depends mechanically on the frozen ledger contract; prospective ledgers require immutable opening receipts before their first in-scope mutation; the post-P1-C second-stage idea is only a shortlist hypothesis while the final contract remains after P2-B; second-stage content truth includes its own result/progression join, admission snapshot, host-specific isolation, and self-entry Replay/Retry; and the historical 2026-07-15 second-stage preflight is explicitly bounded to its cutoff rather than presented as current truthful-join state. Android and release rows now require repeated-loop/soak and content-bundle compatibility/rollback predicates. These are planning and evidence corrections, not implementation authority.

A non-proprietary schema-2 registry now exists at [Subculture Dataset Evidence Index](SUBCULTURE_DATASET_EVIDENCE_INDEX.json). It contains 30 source records and eleven claims: the prior 29/10 registry plus one research-only Ark stage-authoring/operations cohort source and its bounded structural-priority claim. That new cohort does not enter the active PGR/HI3 packet or product evidence admission. The earlier 29 records still comprise the prior 21 records plus one exact EN Stage producer-input dependency, four exact-static PGR replacement candidates, and three exact-static HI3 replacement candidates. The seven replacements are globally registered for provenance but remain outside the active packet cohort; the two historical helper source IDs remain inside the packet and now carry exact mixed-snapshot replay provenance, while their formal admission remains open. The priority packet remains `PGR GuideFight 4 rows + HI3 StageData_Main 10101`; the retained 2026-06-28 path listing alone still does not authenticate payload bytes. The [P1-B local stage-spine preflight](P1B_STAGE_SPINE_LOCAL_PREFLIGHT.json), immutable [static supplement](P1B_STAGE_SPINE_STATIC_SUPPLEMENT.json), and three separately accepted presentation cutoffs remain non-interchangeable local evidence. Their historical 19/20 and 43/46 replay states are unchanged, and none admits foreign rows. The raw candidate reproduces source records, manifests, raw hashes, five row selections, 70 explicit cells, and drift/reconciliation. The separate [supporting-citation recovery audit](P1B_PGR_HI3_SUPPORTING_CITATION_RECOVERY_AUDIT.json) preserves the historical seven-absent/two-present path observation. The follow-up [supporting provenance disposition audit](P1B_PGR_HI3_SUPPORTING_PROVENANCE_DISPOSITION_AUDIT.json) fixes the old-identity rule: all seven missing historical outputs are unrecoverable under their old IDs, while the two present helpers are ex-post byte-authenticated but not formally admitted. The new PGR and HI3 packages supply seven versioned successor identities without rewriting that history, and the helper package authenticates the two retained outputs without inventing new identities. Admission therefore stays `0/9`; the exact nine historical `inScopeSourceIds` remain unchanged, the active report path/hash stay null, claim mappings and packet `crosswalkRows` stay empty, and all three live acceptance results stay open.

The PGR negative control is the clean Git file `table/share/guide/GuideFight.tab` at upstream commit `d36135613b5fc3323cabe42a1cb6238e7f4aea4f`, SHA-256 `D846AB057E526ED4CD9DABAC534BA561D2A14A87FA605363B0730EF45A1BA590`. It has exactly three rows, IDs `100001`-`100003`, joined to stage IDs `10010001`-`10010003`; by itself it proves snapshot drift risk rather than the fourth row or current runtime behavior. The retained-mirror candidate now identifies the fourth exact row as `100004/10010005` and classifies its bounded drift without unioning snapshots. Packet promotion still requires both raw authority sources and all nine supporting identities to enter one authenticated eleven-source cohort; the historical control never substitutes for that admission.

The second exact control is [HI3 2021 StageData_Main 10101 Control](P1B_HI3_2021_STAGEDATA_10101_CONTROL.json). At public commit `01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1`, the exact Global blob is 30,600,482 bytes with SHA-256 `6AB32C175B399D89D035E9736D150760725DD4F85CC5BD9870C64093C51A7431`; numeric `levelId=10101` matches exactly one of 9,642 unique-ID rows and exposes 67 top-level fields. The no-payload 14-slot projection is deliberately conservative: ten slots are `present/proven-static`, while loadout, recommended-next, story-entry, and story-exit remain `unresolved/unknown` because shape alone cannot assign linked-table or Lua consumer semantics. The report stores hashes, paths, shape states, and negative boundaries but no source values. The retained-mirror candidate reconciles exactly to this 2021 Global file/row/key-set rather than proving a newer snapshot. The control still closes zero active packet rows and cannot substitute for atomic packet admission. The bounded non-payload candidate package described below is now complete; policy/rights disposition and one atomic eleven-source admission still gate every promotion:

Retained-mirror update, 2026-07-15 21:45 KST: this supersedes only the earlier statement that neither raw source had been registered. The local retained mirror `C:/Ark/SubcultureGameData` reproduces the historical bounded candidate, while the direct SMB archive is now separately available for curated read-first research. The deterministic [PGR/HI3 raw five-row candidate](P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE.json), generated by [its bounded verifier](P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE_GENERATOR.mjs), hashes to `04ebf0a5be6db2535730088b3b7bcd7b6a50c48844292a43e1f9070418efed3d` with canonical packet digest `f305cc6fdde04fa8b7a2e755b3995e62b297fa9bc08eac73550c00c3056d9b2d`. It reproduces exact PGR EN GuideFight rows `100001`, `100002`, `100003`, and added `100004/10010005`; the ZH sibling is byte-identical compare-only. Against the 2020 control, no row is removed and shared stage identities are unchanged; `100001` and `100003` move from populated loadout slots to exact-row null, while `100002` retains the same loadout identifiers across the JSON representation and gains a non-null record-time field. HI3 Global `levelId=10101` matches the 2021 control file, revision, canonical row, and key-set hashes exactly, so it is reconciliation rather than a newer snapshot. The candidate contains exactly 70 explicit cells: `present 16 / exact-row absent 6 / unresolved 48`, `proven-static 22 / unknown 48`, and zero copied source values. It is deliberately **not admitted as product-contract evidence**. The immutable candidate historically classified all nine supporting identities as path-missing; the correction audit (`report SHA-256 5240701338c92f3395ec3bc4716dd1f953637038382a4b557cf6f7d16fbebdda`, canonical audit digest `27398ca5a9d0dfae6f3fdd01fff9d42099d8cb546386dc8ddf6eaa243ee0c991`) preserves the exact seven-absent/two-present observation. The disposition audit (`report SHA-256 1946b34965536033dc872792e19fa06d009c36f7425a1013899ce1f035b75046`, canonical digest `fd7b5e7df2c908491637ad8e2fc321758cafabf09b5cf8ee67b64e0d4f9da737`) then classifies the seven as `replacement-contract-required` and the two HI3 helpers as `byte-exact replay authenticated / formal admission open`. Its retained wrapper and fresh exit-0 result pin 1,509 selected JSON inputs / 456,457,979 bytes / input-inventory digest `3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662`, and reproduce helper hashes `d8292d42...f5e7` and `5067a789...5d92`. All nine remain blocking only for that packet's formal promotion, so packet `inScopeSourceIds`, active report path/hash, claim mappings, live admissions, and all three live acceptance rows remain unchanged/open. Exact-row null is never promoted to tablewide, runtime, or gamewide absence.

PGR replacement update, 2026-07-16 01:05 KST: [PGR stage-spine read-first v1](P1B_PGR_STAGE_SPINE_READFIRST_V1.md), its [summary](P1B_PGR_STAGE_SPINE_READFIRST_V1_SUMMARY.json), [20-row label-context projection](P1B_PGR_GUIDEFIGHT_STAGE_LABEL_CONTEXT_V1.csv), and [56-row reading-link projection](P1B_PGR_GUIDEFIGHT_STAGE_READING_LINKS_V1.csv) are new versioned semantic successors, not reconstructions of the four missing historical files. The [source record](P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json), [producer manifest](P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json), and [package audit](P1B_PGR_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json) bind fifteen exact inputs, EN authority, ZH compare-only, main generator hash, commands, four output hashes, `4` exact joins, label rows `20/20`, reading links `56/56`, state counts `32/20/4`, and zero authored payload-copy. The package audit SHA-256 is `54b4cf14c6d72cf14415b301fa8b2bb79d801e329c28773d65c01b7b6f08ebd2`, canonical audit digest `59d21e7da9c6b3e7d70201294830133a925ca56d4dbb067df5a846d8a99253f8`, and package digest `09ca47fa01a1c457f4270e3cd696d0652849fad32f4adf19e9d705b06d74e800`. Independent reconstruction finds blocker `0`, but these four sources remain outside `inScopeSourceIds`. At this historical cutoff the three HI3 replacements and two helper provenance rows were next; the later cutoffs below complete those candidate packages without changing admission.

HI3 replacement update, 2026-07-16 03:00 KST: [HI3 stage-spine read-first v1](P1B_HI3_STAGE_SPINE_READFIRST_V1.md), its [summary](P1B_HI3_STAGE_SPINE_READFIRST_V1_SUMMARY.json), and [14-row reading-link projection](P1B_HI3_STAGEDATA_STAGE_READING_LINKS_V1.csv) are versioned semantic successors to the three unrecoverable historical citation identities, not byte reconstructions. The [source record](P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_SOURCE_RECORD.json), [producer manifest](P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PRODUCER_MANIFEST.json), and [package audit](P1B_HI3_STAGE_SPINE_REPLACEMENT_V1_PACKAGE_AUDIT.json) bind commit `01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1`, exact Global `StageData_Main`, one exact `10101` row, all 67 top-level field-shape rows, fourteen reading links, state counts `10 present / 4 unresolved`, and zero copied source values. The audit file is 3,670 bytes with SHA-256 `cdbb662179c10b035ab889b583341134ab5daef442dcd760e65b3807d3fd0d06`, canonical digest `7f9558bcbcfb41c65cc7abbbd9471704dfba5143c6ff1f6cb20222eb3da7a867`, and package digest `d8819674053194e69fb5b39393f58e6ead0d82d6620cf9673a012abf1c60dc44`. Independent reconstruction finds blocker `0`; all three remain candidates outside `inScopeSourceIds` and provide no runtime ownership claim.

HI3 helper provenance update, 2026-07-16 04:05 KST: the existing historical source IDs `hi3-stage-summary-csv` and `hi3-stage-samples-csv` are retained; their files `hi3-stage-table-summary.csv` and `hi3-stage-row-samples.csv` are byte-exact replay-authenticated rather than replaced. The [source record](P1B_HI3_STAGE_HELPER_PROVENANCE_V1_SOURCE_RECORD.json), [producer manifest](P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PRODUCER_MANIFEST.json), and [package audit](P1B_HI3_STAGE_HELPER_PROVENANCE_V1_PACKAGE_AUDIT.json) bind three upstream snapshots, 1,509 selected JSON inputs, 456,457,979 selected bytes, input-inventory digest `3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662`, and exact output hashes `d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7` / `5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92`. The 4,082-byte audit hashes to `383ba46684bcb51734c24073d65a95af31769633cf7d638cc2790a16198a706d`, with canonical digest `b16247c0877cbfd0609241dbc85a9672955aee302190da64ff6e6f075757bda6` and package digest `9de3bafab2b6695263f9c7e1e4d40ffa946a0656a3337de7b43c2cd19d3fea9a`. This proves derivation and byte identity only; it does not make the helpers exact `10101` semantic authority or formally admit them.

Supporting-cohort rights-signal and cumulative-manifest update, 2026-07-16 11:39 KST: the [license-signal audit](P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT.json) is 9,166 bytes, hashes to `bc418a4d6bc6809b89832dec73efa6e3dca18bb3d0c1e21157adecd65f9145e6`, and has canonical digest `ec3e3a1e6500ddb96a8b6fb907d8e49e22627db8f1633b4cd45febebeec008e2`. Its exact retained-archive scan finds no license-like entry in the three contributing repositories; the fourth archive contains an AGPL license file but contributes zero selected helper inputs. That is a factual signal inventory, not legal advice, permission, or a substitute for policy/rights review. The [nine-candidate cumulative manifest](P1B_PGR_HI3_SUPPORTING_COHORT_CANDIDATE_V1_MANIFEST.json) is 12,158 bytes with SHA-256 `2f6cf9f5b3e319239fe780a2dd605dedb4405f69b48a8183349610ccfd8efc9d`, canonical manifest digest `e9fc1d979b3fc44b17b161bb72511402c2ccc771853172ecee83a5517c861ac7`, and cohort package digest `d8f318474ca364cdd3791e60adb90b6897c411c92a6922618f3433dc2f58c5fe`. It orders PGR four, HI3 replacements three, and helpers two, rechecks the three pinned package audits and all 22 declared package files, and records `9 verified / 0 admitted / 0 live rows / 0 live cells`. The remaining gate is an explicit policy/rights disposition or an admissible replacement lineage where required, followed by one atomic eleven-source registry admission and LiveAcceptance; no candidate may be admitted piecemeal.

| Required manifest field | Purpose | Acceptance evidence |
|---|---|---|
| source title, snapshot date, evidence grade, and relative source path | distinguishes a specific inspected snapshot from a moving SMB folder | one manifest row per promoted source claim |
| byte size plus cryptographic hash for each directly cited source file or bounded archive | detects silent replacement and permits later audit | hash command and captured digest; no proprietary payload copied into the repo |
| decode/extract tool revision and exact bounded command | makes row counts and field inventories reproducible | command exits successfully from the named snapshot |
| generated aggregate/report path plus hash | separates raw evidence from analyst interpretation | report can be regenerated byte-for-byte or has an explained deterministic normalization |
| claim-to-source mapping and explicit negative boundary | prevents a static field from being promoted into unobserved runtime behavior | every roadmap promotion names both what the source supports and what it does not |
| cross-snapshot negative control and drift disposition | prevents a convenient older snapshot from being merged into a newer row count | one-snapshot row IDs plus added/removed/changed classification; no cross-snapshot union |

Until that package exists, reopen an older archive only when the conditional queue below receives stronger direct consumer or runtime evidence; do not widen the search merely because the share becomes reachable again.

## Existing DimensionBrawl Baseline

| Area | Current evidence | Current maturity |
|---|---|---|
| Stage map and runtime wiring | **StageDefinitionProfile** owns identity, scene path, anchors, spawns, cutscene handoffs, and runtime-state references. | Implemented foundation |
| Canonical product route | The current flow admits one immutable run/route snapshot, seals the Corridor tutorial summary before SingleLoad, enters Station with explicit guide lifecycle, collects the revision-1 fact payload, validates terminal closure, durably decides one committed summary, presents one additive shell, and dispatches typed Replay/Retry/Lobby. | Baseline 28/28 and frozen P1-0 37/37 remain green. Historical 45/49/54/59/68/75 cutoffs are distinct; the manifest-bound 79/79 cutoff closes P1-A current-schema exit. |
| Linear encounter design | **LinearStageTemplateProfile** and **LinearStageSegmentProfile** own route, pacing, lesson, mastery text, reward hook, segments, and pockets. | Authored data; runtime consumption is intentionally absent |
| Playable tutorial | **OlympusCorridorTutorialDirector** owns a scene-specific sequence from melee and movement through ranged fire, dodge, and target clear. It observes real combat/input events and has staged cue/observation/commit behavior. Normal completion clears several input/presentation/target domains, but cancel does not restore blockers, bounds, target candidates, target pose/health/AI, or source-owned invulnerability, and no exact prior-state restore is proven. | Strong normal-path runtime and PlayMode coverage, but monolithic, scene-bound, and missing terminal/reset parity |
| Cinematic and story handoff | **CinematicSequenceProfile** already owns stage context plus movement/input/HUD lock intent and a `GameplayHandoffCue` with return mode, target, release delay, and HUD/time-scale/camera restore flags. **CinematicSequenceRunner** restores driven camera pose, actor controllers/visibility, fade, and explicitly disabled behaviours on natural completion or `Stop()`. | Capable profile/runner foundation, but most handoff fields are not executed by the generic runner; the current Olympus intro uses scene-specific `PlayableDirector` and flow-controller wiring for skip, cameras/listeners, roots, HUD, and input |
| Stage selection and briefing | Candidate-05 accepts one presentation-only catalog row directly referencing `OLYMPUS-INVASION-01`, an immutable route projection/digest, no Retry alias, one preserved empty hidden reward row, and fail-closed public invalid selection; **ChapterMapPrototypeStageNode** still separately stores prototype copy/state | canonical selection is accepted without becoming a run owner. Truthful reference/template-derived briefing and progression remain unjoined; legacy card copy and the empty row are not gameplay/reward truth |
| Stage result | **StageClearScreenPresenter** receives an immutable summary plus schema-2 receipt, configures outcome-filtered actions, renders committed clocks/proof through exact profile/localization, and invokes only the route/run-owned executor; **CombatSessionOverlayPresenter** remains the in-combat surface. | Four endpoints, bounded missing/stale/competing/resolver/load, direct I/O, read-only localization, typed Station adapter-loss safety, summary/route/receipt integrity, terminal-action closure faults, loader-completion distinction, three-phase cancel, subject loss/rebind, named unload, exact duplicate/replacement/diagnostic provenance, and final-snapshot exception closure are verified at the final 79/79 cutoff. Preserve this P1-A boundary; later rank/mastery/reward/progress work remains separate. |
| Progression and rewards | **STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md** supplies historical vocabulary, while [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md) now separates P1-D state, pure resolution, conditional buckets, deltas, journal, and receipts. | Provisional analysis contracts only; no matching persistent production owner or payout found |
| Enemy/run composition | Combat role profiles and the canonical Olympus flow cover enemy pressure and boss/summon exchanges. A separate `PveStageData` / `PveEncounterDirector` prototype executes raw trigger-Z groups and placements. | Canonical scene flow plus a noncanonical prototype. Neither joins linear pockets to `StageDefinitionProfile.SpawnRef`; the prototype also lacks the execution-generation, fail-closed spawn, retry/scene-exit cancellation, and owned cleanup contract required by P1-C. |
| Build and performance baseline | The 2026-07-14 22:27 KST software-side reference uses Unity `6000.3.5f2`, development ARM64 IL2CPP, and the two canonical combat scenes. `DimensionBrawl-MobilePerformanceBuild.json` records a successful `300,628,811`-byte APK with 0 errors/79 warnings; SHA-256 is `8E841306D816C1D60B29C7646043BDE6CC89229F71E95A094D1B0B22278A3D8A`. Render-budget tests pass 21/21, the then-current canonical/performance regression passes 199/199, and Editor main-thread P95 is Station `2.434 ms`, Corridor `2.237 ms` against a `4 ms` software target. | Historical optimization reference only: it predates P1-0/P1-A worktree changes, its APK contains only the two benchmark scenes, Editor GC/GPU figures are non-authoritative, and no Android device was connected for GPU frame, PSS, thermal, throttle, or player-GC evidence. Every promoted wave must rebuild/retest rather than claim this artifact as current-worktree performance proof. |

## Content and Presentation Asset Coverage Ledger

Repository-wide authoring counts are not the same as product-route coverage. `.meta` files are excluded; an `unknown` canonical binding remains unknown rather than being inferred from a similarly named asset.

| Category and count unit | Current verified production/repo coverage | Reference evidence and product effect | First bounded target | Cost | Principal dependencies | Principal risk | Contract owner | Exit evidence |
|---|---|---|---|---:|---|---|---|---|
| Stage — logical `PlayableStageDefinition` | one logical revision-1 stage and two physical definitions/scenes; selection, truthful joins, result/progression joins, and the static Station count-one Add pass at separate immutable cutoffs | PGR distinct guide/stage links and HI3 `StageData_Main` support stable product identity. Effect: stage select, run, reference, template, briefing, admission-time result/progression presentation, and static Add identity share the canonical route key | P1-B bounded target closed; next product target is one ledger-gated P1-C runtime pocket, then a second short logical stage | 3 | frozen route/catalog/reference/template/briefing/result-progression/Station-Add joins | accepted joins are mistaken for durable progress, reward, P1-C execution, restart, or second-stage content, or route/ID drift creates competing identities | accepted P1-B spine; P1-0 route remains immutable input | P1-B exit closed without changing frozen digests or typed absences; P1-C still requires its own runtime evidence |
| Segment — route row plus physical definition | two canonical route rows; repo-wide eight segment profiles/eight pockets and five templates with 29 segment refs; truthful canonical template join zero | Aether Gazer/ZZZ pocket-group topology and Arknights ordered level actions support a separate ordered segment layer. Effect: P1-C can execute the intended pocket | retain the exact two rows and add one truthful template join | 3 | frozen route digest; reviewed tutorial/guide/boss intent | force-fitting a similarly named but semantically false template | P1-B authoring; P1-C consumes | contiguous order/digest unchanged; explicit join validator; no name-similarity inference |
| Encounter — executable canonical binding/sequence/group | two serialized `CombatEncounterController` instances, one per scene; Station is the terminal source; canonical P1-C binding/sequence/group zero | Aether Gazer/ZZZ, Arknights, GF2, and Last Origin support stage-to-ordered-group joins. Effect: authored spawn execution without another scene script | one binding -> one sequence -> one nonempty group -> one `Add` ticket with `count == 1` | 5 | P1-A closure/quiescence; P1-B anchor/pocket; payload resolver/factory; scene lease | false clear, stale generation, partial-spawn/object/subscription leak, or dual PVE owner | P1-C | placeholder-free payload/unique anchor; deterministic activation and defeat proof; spawn failure faults; cancel/unload/action leaves zero tickets/objects/subscriptions/lease |
| Lesson — stable typed definition/result | seven code-default steps: one cue-only and six observed actions; serialized typed lesson/result zero | PGR reusable tutorial catalogs and Wuthering guide/teaching separation support reusable proof. Effect: working tutorial behavior becomes safe practice content | bounded technical target: Move 1. Product reset target: Move plus Fire, 2 lessons total | 4 for Move; 5 through Fire | P1-A/P1-B identity; P1-D objective seam; input ownership; attempt generation; parity | evaluator/order regression, missing-binding false success, projectile/target/input cleanup leak | P1-E | bounded exit: Move immutable 0.75m proof and no-mutation receipt. Product exit: Fire exact gameplay reset plus natural/cancel/disable/unload/stale isolation and route parity |
| Course — product-bound runtime course definition | zero reusable course; Station's two-page acknowledgement is not learner proof | PGR tutorial/practice/challenge separation, with HI3 catalog separation as a boundary. Effect: teach -> unjudged practice -> truthful challenge | one product-bound course with exactly three entries: Basic, Practice, Challenge; isolated fixtures do not count | 5 | P1-E lesson; P1-C encounter; P1-D mastery; P2-A variant; P2-B lifecycle | acknowledgement misreported as proof, dirty Practice baseline, or two continuation owners | P2-B course owner | Basic result closes; Practice proceeds without fake pass/fail; Challenge uses qualified summon proof and run outcome; every transition has cleanup/fresh-baseline receipts; new run starts at Basic |
| Enemy / variant — typed identity/profile/binding/configuration receipt | nine archetypes, twelve roles plus eight role decks, twelve candidates, nineteen ActionFoundation enemy prefabs; canonical scenes have zero typed archetype/role/candidate refs and typed variants zero | HI3 identity/config/AI split, Wuthering identity-growth-behavior-skill split, and Arknights stage-local overrides support configuration reuse. Effect: mode breadth without asset or AI duplication | one identity and the same gameplay prefab with three profiles: Story, Practice, Challenge | 5 | P1-C count-one Add/payload map; inactive configuration adapter; candidate/prefab agreement; restore seam | prefab mismatch, silent defaults, second AI framework, or false numeric completeness without one stat owner | P2-A configuration; P1-C spawn/lifetime | same identity/prefab/base presentation; three reviewed digests and pre-activation receipts; no duplicate placement/count/delay; exact teardown |
| Story handoff — definition-to-consumer lifecycle chain | one accepted direct Corridor-entry chain, one owned scene port, 39 exact current bindings, Corridor anchors 4/4, Station 0/0, exact remaining-profile context, and retained payload after removing unowned components | HI3 plot entry/exit, Wuthering lifecycle, Genshin acquire/release, and Limbus before/after roles support explicit ownership. Effect: reuse current cinematic as a safe route transition | local static anchor/profile hygiene is closed; preserve it while the later P2-B adapter proves complete/skip/cancel/disable/unload/restart ownership and restoration | 5 | P1-A identity; P1-B full exit; P2-B registry/domain seams | camera/input/HUD/time-scale leak, late callback, double handoff, or static hygiene mistaken for lifecycle proof | P1-B static join plus P2-B lifecycle | definition/direct profile/actual Timeline/scene port/sole Director/anchor-profile inventory agree; complete/skip/cancel/disable/unload/restart restore exactly once; only normal entry publishes one handoff |
| Cinematic — direct profile plus played asset plus validated consumer | nine profiles, three Timelines; one accepted route-entry -> direct combined profile -> played combined Timeline -> sole Corridor Director/existing-flow chain. The second accepted cutoff has exactly 39 current output/binding rows with zero null/stale/unresolved rows; four clipped Audio tracks retain exact `AudioSource` bindings, and the clipless Audio/Cloud Deck rows are removed | HI3 presentation separation, Aether Gazer set/reset, and Genshin pre/perform/next/finish fields support a typed playback chain. Effect: convert existing value into product proof without a new framework | current P1-B static port/binding target met; preserve the exact direct identity and 39-row binding surface alongside the accepted catalog and truthful reference/template/briefing join | 3 | accepted P1-B catalog/reference/template/briefing; P2-B lifecycle adapter | profile/Timeline drift, Director/runner double-play, clipped audio deletion, or authored fields never executed | P1-B static join plus P2-B lifecycle | two intended negative fixtures fail, validator and natural path pass, and current-output↔binding bijection has no null/stale/unresolved row; perceptual audiovisual quality and complete lifecycle restoration remain P2-B evidence rather than being inferred from bindings |
| Result surface — committed-summary UI owner | one additive `UI_StageClear`; P1-A2 provides one shared presenter, two outcome configurations, three typed actions, one schema-2 durable run decision, and verified facts. Historical cutoffs and final 79/79 pass; clocks/proofs render through exact profile/localization and direct summary/route/receipt plus closure-integrity checks protect dispatch | HI3 result facts, PGR retry/result surfaces, and Limbus result/story/reward separation support one truthful summary consumer. Effect: a replayable Clear/Fail loop without a second terminal owner | current-schema result and exit targets are met; later admitted-owner evidence remains a separate schema gate | 4 for current surface; 5 for full result lane | frozen D4b; P1-A diagnostic lifecycle; P1-0 actions | terminal-owner duplication, stale/double-click race, exit/unload loss, or missing-summary fact fabrication | P1-A run/result owner | 79/79 closes exact duplicate, replacement, diagnostic, and snapshot-exception rows; preserve this boundary during P1-B |

Count discipline:

- two catalog rows are not two playable stages;
- eight segment profiles plus five templates are not thirteen canonical route segments;
- global enemy assets are not verified route diversity;
- nine cinematic profiles are not nine proven product playbacks.

All peer evidence in this ledger is historical-snapshot evidence until the seed evidence index is completed enough to permit a live rerun. Measured authoring/QA effort and defect capture begin prospectively only after `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN` passes; prior P1-B work is `RetrospectiveUnavailable` and cannot be reconstructed as the comparison baseline. The first true reuse proof is a second short stage authored without a new scene-specific route manager, not another inventory count.

### Second-stage candidate disposition preflight

The 2026-07-15 read-only replay matched all 13 template/segment asset hashes in `P1B_STAGE_SPINE_STATIC_SUPPLEMENT.json`. It found five structurally valid templates, eight segments, eight embedded pockets, 29/29 template-to-segment references, and 19/19 pocket role-slot references. It also found **zero product-ready second-stage candidates**: canonical template/segment joins and their production runtime consumers remain zero, while the product still has one logical stage, two physical route rows, one catalog row, and one result profile.

| Rank | Seed | Current disposition | Why it is useful | What blocks a product freeze |
|---:|---|---|---|---|
| 1 | `S1-1.BreakGate` | authoring seed only | smallest five-segment distinct content promise | executable Break owner, truthful definition/scene/encounter, separate route/briefing/result identity |
| 2 | `S1-3.TankRescue` | reuse-experiment seed only | closest to current Station pressure/summon-response behavior; useful for measuring shared-host reuse | actual Tank identity/answer and distinct encounter promise; high clone/rename false-content risk |
| 3 | `S1-2.BacklineSignal` | conditional seed | distinct ranged/priority-target lesson | admitted Arrow/equivalent answer and target/backline ownership |
| 4 | `S1-4.HealPocket` | hold | later sustain-pressure candidate | Heal owner, controlled-damage evaluator, health/reset ownership |
| 5 | `S1-5.BossStand` | reject as first slice | later composite capstone seed | depends on the preceding lesson/encounter chain and would be an oversized first reuse proof |

Corridor and Station are physical rows of `OLYMPUS-INVASION-01`, not two logical stages. `DB_FrontlineWaveStage_MotivationReview`, an individual segment/pocket, a dormant catalog card, Retry/Replay, a result/cinematic profile alone, or the current Station encounter with a new name/difficulty do not count. The current shared UI/result shell can be reused, but StageSelect validation is single-entry and the run/fact path remains `CorridorActive -> StationActive` plus Olympus-specific adapters. Therefore candidate admission waits for accepted P1-C/P2-B and must freeze one exact `contentPromiseId`/revision, scope fingerprint, route/condition contract, and catalog-generation transition before its first comparable edit.

Two physical authoring shapes remain open. A new lightweight scene makes definition/encounter truth clearer but adds manual-content scope. A route-selected shared host reduces duplicate scene content but requires deeper ordered-run, adapter, fact, and validator generalization with the highest P1-A regression risk. The selected shape must record foundation generalization separately from matched content-authoring time; neither option is frozen by this preflight.

#### `EXPANSION-CANDIDATE-LEDGER-01` — planning only

This ledger normalizes the five local second-stage seeds for comparison; it is not a product freeze, accepted evidence cutoff, implementation authorization, or substitute for live Ark evidence. Every row is bounded to `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715`: the local 13-hash structural preflight supports seed ordering and negative boundaries only, while product-ready candidate count remains `0`.

##### `DECISION-SNAPSHOT-EXPANSION-01`

| Metadata field | Exact planning value |
|---|---|
| evaluatedAt / source observedAt | `2026-07-15T19:57:24+09:00` / `2026-07-15T19:01:27+09:00` |
| source evidence ref / class | `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715` / read-only local product-and-throughput contract audit; exact local structural hashes and negative boundaries only, no live foreign row |
| decision scope / priority effect / product-ready count | `planning-order-only` / `none` / `0`; the existing five ranks, dispositions, product scores, and track priorities are unchanged |
| cost basis | `unmeasured-qualitative`; no prospective authoring ledger exists and prior P1-B is `RetrospectiveUnavailable` |
| confidence | evidence boundary `high` because the local source/ref/hash and negative boundary are explicit; player/production effect `low` because no candidate is product-authored or playtested; cost `low` because no prospective matched ledger exists; dependency `high` because item/acceptance prerequisites are explicit; identity/regression `medium` because current owners are inventoried but neither physical host nor runtime integration is proven |
| freshness / invalidation triggers | re-evaluate on source/hash or template/segment drift, a changed product-ready candidate count, amended P1-B/P1-C/P2-B ownership contracts, an admitted content promise or physical-host choice, the first prospective ledger measurement, or new directly relevant admitted evidence; none automatically promotes a candidate |
| next review gate | `ACC-OPS-SECOND-STAGE-CANDIDATE-CONTRACT-FROZEN` |
| pre-P1C measurement subgate | `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`; must pass before the first comparable P1-C mutation |

| Rank | Candidate and disposition | Evidence boundary/ref | Player/production effect | Qualitative cost | Dependencies | Identity/regression risk | Missing content truth | Physical-host choice boundary |
|---:|---|---|---|---|---|---|---|---|
| 1 | `S1-1.BreakGate` — authoring seed | `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715`; local structure only | smallest distinct stage promise; proves whether the spine can author genuinely new play instead of another Olympus alias | medium-high | accepted `P1C-COUNT1-ENCOUNTER`, accepted `P2B-PRESENTATION-LIFECYCLE`, and `ACC-OPS-FIRST-POCKET-AUTHORING-LEDGER=pass` | a template-only Break label becomes false content, or a new scene-specific route owner defeats reuse | executable Break owner, truthful definition/scene/encounter, separate route/template/briefing/result identities | choose and record either a new lightweight scene for clearer truth or a route-selected shared host with deeper adapter/fact generalization; neither is preferred or frozen yet |
| 2 | `S1-3.TankRescue` — reuse-experiment seed | `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715`; local structure only | tests reuse of current pressure/summon-response foundations and exposes the real cost of a shared host | medium if reuse is real; high if foundation changes dominate | accepted `P1C-COUNT1-ENCOUNTER`, accepted `P2B-PRESENTATION-LIFECYCLE`, and `ACC-OPS-FIRST-POCKET-AUTHORING-LEDGER=pass` | renamed Station pressure masquerades as new content; shared-host changes regress P1-A route, adapter, terminal, or fact ownership | actual Tank identity/answer and a distinct encounter/content promise | compare lightweight-scene truth against shared-host reuse under the same ledger; foundation generalization is excluded from comparable content time but still reported |
| 3 | `S1-2.BacklineSignal` — conditional seed | `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715`; local structure only | adds a ranged priority-target lesson that broadens tactical reading without changing the fixed-rear core | medium-high | accepted `P1C-COUNT1-ENCOUNTER`, accepted `P2B-PRESENTATION-LIFECYCLE`, `ACC-OPS-FIRST-POCKET-AUTHORING-LEDGER=pass`, and an admitted Arrow/equivalent answer | prose claims target priority without a target/backline owner, or shared targeting changes regress existing encounter behavior | admitted ranged answer, target/backline ownership, executable encounter, and truthful objective/proof | select lightweight scene or shared host only after target ownership is admitted; no host may infer target truth from layout alone |
| 4 | `S1-4.HealPocket` — hold | `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715`; local structure only | later sustain-pressure lesson and controlled-damage evaluation candidate | very high | the common P1-C/P2-B/first-ledger predecessors plus an admitted Heal owner, controlled-damage evaluator, and health/reset boundary | health/reset authority leaks, scripted damage fabricates proof, or recovery state contaminates later runs | Heal identity/owner, controlled-damage fact/evaluator, and exact health/reset lifecycle | host choice remains unevaluated while held; neither a new scene nor shared host supplies the missing health/evaluator authority |
| 5 | `S1-5.BossStand` — reject as first slice | `EV-OPS-SECOND-STAGE-AUTHORING-PREFLIGHT-20260715`; local structure only | possible later capstone after the smaller lesson/encounter chain exists | very high and oversized | accepted preceding lesson/encounter chain plus P1-C/P2-B and the first-ledger/second-stage candidate gates | duplicates the existing Station boss promise, collides with terminal/result ownership, and hides reuse cost inside a composite | distinct boss/content promise and every prerequisite lesson/encounter truth | no first-slice host decision; reconsider only after smaller candidates prove their ownership and throughput boundaries |

`ACC-OPS-SECOND-STAGE-CANDIDATE-CONTRACT-FROZEN` is a proposed new **pending** planning gate under `OPS-STAGE-THROUGHPUT`. After accepted P1-C/P2-B and the first-pocket ledger, it must select exactly one row and freeze its candidate ID, content-promise ID/revision, fresh six-axis candidate score, scope fingerprint, route/condition contract, catalog-generation transition, physical-host choice, foundation-versus-content accounting, exclusions, and acceptance/evidence plan. Passing it still authorizes no implementation. The existing `ACC-OPS-SECOND-STAGE` then becomes delivery-only and may pass only for that exact frozen candidate; it must no longer combine candidate selection/freeze with delivery.

This throughput ledger is separate from the demand-gated `P3-REPEAT-CONTENT` item. It cannot satisfy `ACC-P3-REPEAT-DEMAND`, `ACC-P3-REPEAT-SLICE-CONTRACT-FROZEN`, or `ACC-P3-REPEAT-CONTENT-SLICE`, and a later P3 family cannot inherit its ranking or score. P1-B is now closed; the prospective first-pocket ledger contract must freeze before any P1-C runtime mutation, and only later may the proposed second-stage candidate-contract gate be judged.

### `FOUR-DIMENSION-TRACEABILITY-01`

The machine-readable backlog now maps all 30 registered items and all five planning-only expansion seeds across the requested Function, Content, Presentation, and Operations dimensions. `D` means direct contract/acceptance ownership, `S` a formal supporting scope, `P` prose or intent only, `L` planning-ledger-only, and `N` outside that item's declared scope. This closes a traceability defect only; it does not close a product or evidence gap.

| Scope | Function | Content | Presentation | Operations | Decision boundary |
|---|---|---|---|---|---|
| 30 registered items | `D22 / S6 / P1 / N1` | `D14 / S7 / N9` | `D9 / S11 / P3 / N7` | `D25 / S5` | all 30 resolve to existing `items[itemId].acceptance`; no score, rank, dependency, or acceptance result changed |
| five expansion seeds | `L5` | `L5` | `L3 / P2` | `L5` | product-contract coverage remains `0/5`; candidate labels, effects, costs, hosts, and ranks remain planning-only |

The audit exposes one material comparison gap that was previously present only in the conditional queue: `PGR-HI3-EARLY-STAGE-ACTION-LIFECYCLE-01`. The missing evidence is not another static catalog count. It is one bounded causal chain per direct peer:

- PGR: `PracticeActivity -> direct runtime consumer/trace -> tutorial/practice/challenge attempt proof -> result/progress persistence -> reboot or retry cleanup`;
- HI3: `StageData_Main -> entry/Lua -> enemy and wave execution -> typed challenge evaluator -> lose/result/retry -> cleanup`.

The evidence index now registers `EVID-PGR-HI3-EARLY-STAGE-ACTION-LIFECYCLE-01` as the last, priority-8 conditional-reopen packet. Registration is ordering metadata only: the gap still has no registry item, acceptance, admitted source, product authority, or deep-read authorization, and existing evidence priorities 2 through 7 are unchanged. The exact PGR 2020 and HI3 2021 controls remain drift/row-shape controls only. The packet cannot enter a bounded read until `ACC-EVID-P1B-LIVE-PROVENANCE`, `ACC-EVID-P1B-EXACT-ROWS`, and `ACC-EVID-P1B-DRIFT-CLASSIFICATION` pass, `P1B-STAGE-SPINE` is accepted, one exact two-snapshot scope digest plus file ceiling is frozen, and a separate authorization names the packet. It must stop at the first broken direct edge rather than substituting another row, locale, snapshot, or static catalog. It cannot change the current critical path: Station Add authoring, live PGR/HI3 foreign-evidence disposition, P1-B full exit, then P1-C.

### `EARLY-STAGE-LIFECYCLE-DECISION-APPLICABILITY-01`

The priority-8 packet now has a seven-slot question contract before any foreign read. PGR requires one exact-static root plus six exact-runtime behavior slots. HI3 requires the same root plus exact-runtime entry, execution, proof, terminal, and cleanup, while `progressPersistence` is a packet-scope `not-applicable`, not an observed claim that the game lacks progress or rewards. This correction prevents PGR execution/proof/cleanup from being hidden inside five narrative nodes and prevents the packet's two-game terminal rule from demanding out-of-scope HI3 persistence.

| Lifecycle slot | Current DimensionBrawl truth | Future foreign question | Permitted decision use | Local review surface |
|---|---|---|---|---|
| `rootStageIdentity` | frozen P1-0/P1-A route and run identity; Candidate-05 selection and the truthful reference/template/briefing join are accepted; result/progression remains current P1-B work that must close before this packet can start | does one exact PGR PracticeActivity root and exact live HI3 `10101` root stay stable through every edge without snapshot/locale substitution? | identity-drift and root-substitution regression only | closed baseline by packet admission; no product gate effect |
| `entryConsumer` | presenter/router is not admission owner; Corridor flow plus `StageRunRuntime.TryAdmitFirstSegment` is the proven sole entry path | what direct consumer receives the exact root and begins one attempt? | stale/missing/mismatched/duplicate-consumer negative tests only | closed baseline by packet admission |
| `execution` | Corridor tutorial, Station guide, and boss execute through scene-specific owners; no reusable count-one Add binding/resolver/lease/completion contract exists | what direct executor call or trace preserves the root through actual stage/wave work? | structural vocabulary and failure taxonomy only | P1-B Station Add and P1-C review; cannot satisfy either acceptance |
| `attemptOrChallengeProof` | P1-A facts and qualified proofs exist, but typed lesson attempt, evaluator, and course traversal are absent | what attempt-bound proof/evaluator consumer publishes a typed result? | proof/evaluator boundary and negative cases only; no copied objective meaning or threshold | P1-D, P1-E, and P2-B review |
| `terminalResult` | current-schema terminal authority, committed result, result shell, typed actions, abort/dispatch closure, and durability are accepted | what handler or dispatcher preserves the same attempt through one terminal arm? | callback/order/display-as-authority regression only | P1-A remains closed |
| `progressPersistence` | durable result-decision receipt exists; player progress intent/state/application/store does not | PGR: is there a direct client write or receipt? HI3: retain typed packet-scope N/A | failure-checklist vocabulary only; never server durability, atomicity, reward, or exactly-once proof | P1-B result/progression join, P1-D progress, later P2-C |
| `retryRebootOrCleanup` | post-result Replay/Retry and current abort cleanup are accepted; lesson reset, course/presentation cleanup, and pre-result active restart are absent | what direct cleanup/re-entry edge proves closure and, if claimed, a fresh attempt identity? | keep post-result Retry, lesson reset, presentation cleanup, and active restart separate | P1-E, P2-B, and P2-AB review |

This matrix assigns no numeric score. Local-owner and dependency confidence is high; foreign effect confidence and both evidence-reading and implementation-cost confidence remain low until an exact live trace and local experiment exist. The main risks are high-confidence category errors rather than measured likelihoods: static-to-runtime promotion, client-state-to-durability promotion, result receipt-to-progress promotion, and collapsing Retry, Replay, lesson reset, active restart, revive, return, and cleanup into one operation. The full machine-readable questions, stop conditions, and forbidden inferences live in the evidence packet; current foreign answers remain `0/14`.

### `LOCAL-RESIDUAL-SURFACE-DISPOSITION-01`

The local preflight leaves several foreign-comparison fields and later product surfaces unresolved. The backlog now gives each one an explicit current disposition and future review owner. `typed-absence-now` means the field remains explicitly absent and no UI/runtime fallback may fabricate it; `add-later` requires the named acceptance; `hold` requires new demand/owner/measurement evidence; `reject-current-scope` prevents an unrelated economy or live-service surface from entering through briefing or catalog prose; `continuous-gate` must be refreshed for every applicable promotion.

| Surface group | Current disposition | Future review owner / decisive gate | Effect, qualitative cost, and principal risk |
|---|---|---|---|
| loadout + restrictions | both typed absent; narrow restrictions may be added later, broad loadout remains ownerless/held | P2-A ruleset owns only a future source-scoped restriction; no broad loadout acceptance exists | stage variety and safe admission; high cost; copied prose, modifier lifecycle, or leaked restrictions can be mistaken for loadout authority and soft-lock runs |
| recommended power + target time | typed absence | a revisioned template-to-briefing join; P2-A may supply calibration evidence but does not own either value, and time-as-mastery separately requires P1-D | truthful preparation/pacing; medium-high; dormant template values or actual P1-A elapsed time can masquerade as calibrated targets |
| entry cost | typed absence; current Retry/Replay remains free; hold and reject from current scope | P3 demand plus a separately approved economy/admission transaction owner; P2-C settlement is only a prerequisite, not that owner | no current benefit; very high; an unowned spend/paywall enters the demo loop |
| mastery + challenge | both typed absent, with separate owners | P1-D owns objective/capability/evaluation; P2-B separately owns Challenge traversal | truthful measured challenge feedback; high; UI prose, acknowledgement, Clear, evaluator, and course entry can be falsely collapsed into one proof |
| threat/enemy/summon preview | all typed absent, with separate owners | P1-B projects an exact encounter; P1-C owns spawn/lifetime, P2-A enemy identity/config and any later summon restriction | truthful tactical briefing; medium-high; legacy copy, observed summon use, correct-answer proof, boss terminal, and enemy identity can be falsely collapsed |
| result/progression/reward | committed result present; static result join, progression, preview authority, and settlement remain separately absent | P1-B joins result identity, P1-D owns durable progress, P2-C owns replay-demand/reward settlement | repeat motivation; very high; result UI, progression graph, persistent state, reward eligibility, preview, and grant receipt can be falsely collapsed |
| story exit | typed absence | P2-C Clear-only story handoff plus P2-B lifecycle | narrative continuation; high; skip/cancel blocks or duplicates settlement/navigation |
| Station count-1 Add | typed absence | P1-B authoring join, then P1-C execution | first truthful reusable encounter; medium-high; duplicate spawn/clear/terminal ownership |
| tutorial course link | typed absence | P2-B course traversal | learn -> rehearse -> prove; very high; acknowledgement or dirty practice state becomes false mastery |
| runtime presentation lifecycle | static P1-B joins only | P2-B all-terminal and perceptual review | cinematic quality with safe cleanup; high; stale camera/HUD/input/audio/time ownership |
| build + device performance | continuous gate | `OPS-BUILD-PERF` | current readiness; medium; historical green evidence masks an unbuildable/device-regressed candidate |
| localization + accessibility | bounded result localization only | `OPS-P2-READINESS` | readable/operable surfaces; high; display text becomes identity or authored-only metadata is called executed |
| diagnostics + privacy | typed absence | `OPS-DIAGNOSTICS-PRIVACY`, then redacted support | supportability; medium-high; diagnostics mutate truth or leak identifiers/free text |
| second-stage throughput | hold | prospective ledger, first pocket, exact candidate contract, truthful delivery, matched comparison | proves reuse value; high and unmeasured; renamed Station or scope drift is reported as cheap new content |

The 14-row matrix uses the local preflight and second-stage structural preflight only. PGR/HI3 historical controls remain permitted drift/shape context but provide no player-effect, cost, runtime-owner, or priority evidence. P1-B is closed independently; the active pre-P1-C gate is `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`, and this matrix changes no accepted P1-B identity or digest.

### Expansion candidate decision readiness

`EXPANSION-CANDIDATE-DECISION-READINESS-2026-07-15-01` records a sharper boundary than the planning rank alone: all five rows are readable as a planning order, but candidate-contract decision readiness is `0/5`. Every row still has zero candidate-specific evidence references, qualitative/unverified effect, unmeasured cost, four free-text unresolved dependencies, combined free-text identity/regression risk, no selected host, no content-promise revision, no scope fingerprint, no fresh product score, and no acceptance/evidence plan. Missing content-truth counts by rank are `5 / 4 / 4 / 3 / 4`.

The existing disposition aliases are cross-checked rather than normalized: `seed -> authoring-seed-only-not-freeze-ready`, `reuse-experiment -> reuse-experiment-seed-only-not-freeze-ready`, `conditional -> conditional-authoring-seed-only`, `hold -> hold`, and `reject-first -> reject-as-first-second-stage-slice`. The PGR/HI3 historical controls may explain drift or row shape but are excluded from candidate decisions. Rank, score, priority, acceptance result, host choice, and implementation authority remain unchanged; the next decision gate is still `ACC-OPS-SECOND-STAGE-CANDIDATE-CONTRACT-FROZEN`.

## First Source Pass

### Punishing: Gray Raven

Primary evidence:

- `games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices.md`
- `games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices-summary.json`
- `games/punishing-gray-raven/enemies-stages/pgr-tutorial-stage-context-rollup.csv`
- `games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-label-context.csv`
- `games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-reading-links.csv`
- `games/punishing-gray-raven/combat/index.md`
- `games/punishing-gray-raven/ui/index.md`

Observed production shapes:

- Guide fights are distinct stage records, not only branches inside one scene director.
- Guide-fight rows link stage identity, title, description, recommended/required level, optional record time, robot/NPC/weapon loadout, and story entry/exit hooks.
- General stage rows expose pre-stage/next-stage relationships, first/repeat reward surfaces, stamina/action-point fields, star descriptions, retry/reboot policy, assist and party restrictions, and input restrictions.
- Tutorial content is split into multiple reusable catalogs: basic controls, dodge, skill/orb practice, core skill, target priority, role practice, character-specific practice, teaching activities, and teaching robots.
- Course, prerequisite/unlock, lesson presentation, player/enemy/equipment loadout, follow-up practice, mastery thresholds, reward hooks, and progress are separable data concerns.
- `PracticeSkillDetails` keeps title, icon, phase descriptions, and presentation content separate from stage execution. This directly supports extracting DimensionBrawl's lesson presentation before replacing its working completion logic.
- The inspected tables do not prove PGR's per-action runtime success evaluator. DimensionBrawl should define typed rules from its own observable combat events rather than infer or copy hidden PGR logic.
- The dataset separates stage context from overlay vocabulary when a direct runtime join is not proven. DimensionBrawl should keep the same evidence discipline.

Useful scale signals from the current snapshot:

- CourseStage: 30 source records; 43 expanded stage references after prerequisite and lesson links are included.
- CourseChapter: 10 source records; 30 stage references. Six use level-style unlock fields and four use score/star-style gates.
- CourseStageShowType explicitly labels 12 Tutorial and 18 Challenge nodes; this is authoring classification, not learner-proof or runtime-completion evidence.
- PracticeChapter: 8 source records; 10 direct stage references plus 88 group references.
- PracticeGroup: 88 source records; 168 expanded stage references and 33 populated follow-up links across 178 candidate link slots.
- PracticeSkillDetails: 85 rows; 71 stage IDs join the current Stage table and only 40 overlap PracticeGroup stages, so the table remains presentation evidence rather than proof that every row is an executable practice node.
- TeachingActivity: 48 source records; 316 expanded base/link/challenge stage references.
- TeachingRobot: 139 rows.
- GuideFight: four initial stage-linked rows in this snapshot.
- PracticeActivity: 250 stage-linked rows in the broader rollup; this table was not part of the focused field audit and remains a follow-up source.

### Honkai Impact 3rd

Primary evidence:

- `games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.md`
- `games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst-summary.json`
- `games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-monster-summary.csv`
- `games/honkai-impact-3rd/ui/index.md`
- `games/honkai-impact-3rd/camera-animation/index.md`
- `games/honkai-impact-3rd/raw/devilpromt-bh3-data/2026-06-15/files/extracted_repo/BH3-Data-main/ExcelOutputAsset_deobf/AvatarTutorial.json`
- `games/honkai-impact-3rd/raw/devilpromt-bh3-data/2026-06-15/files/extracted_repo/BH3-Data-main/ExcelOutputAsset_deobf/MissionData.json`

Observed production shapes:

- `StageData_Main` combines level/chapter/difficulty/battle type, entry/Lua route, challenge references, prerequisites/unlock links, restrictions, recommended level, preview art, fast-bonus time, drops, and lose-description references under one stage identity.
- The sampled `10101 / 1-1 Urgent Mission` row joins title, description, tip, cost, preview/detail art, and a stage-specific Lua path.
- `StageChallengeData` separates `challengeId`, `conditionId`, `paramList`, difficulty, explanation, and hint period. The exact meanings of sampled condition IDs were not decoded, but the typed evaluator shape is direct evidence against storing mastery as display text only.
- Monster records separate attack, defense, HP, AI behavior, configuration variant, type, nature, and optional ability payloads. Examples include mainline/mirror/very-easy variants with different behavior or configuration references.
- `UniqueMonsterData` and `NPCLevelLogic` add HP/attack/defense/movement/resistance ratios, ability payloads, rank, HP segments, and difficulty grouping. DimensionBrawl needs a stage-slot adapter into its existing AI deck/elite system, not a second AI framework.
- Result table surfaces separate stage ID, minimum score, remaining-time/progress reward inputs, best time, and maximum score. They support binding real run facts to the existing clear shell, but do not justify importing HI3's score economy.
- `AvatarTutorial` adds 26 tutorial-catalog rows with ordered site arrays and optional mission arrays. Nineteen mission references all join `MissionData` and link back to their activity, but the site table/stage join and numeric finish/reward meanings remain unresolved. This supports catalog/optional-progress separation only, not a reusable course runtime.
- Plot/dialog rows join stage ID to entry/exit dialog ranges and keep duration, animation/face/lip, side/position, screen effect, audio, BGM, and next-row timing as data.
- UI evidence is organized into open/close transitions, story/dialog flow, layout/resource linkage, gacha/shop/liveops, battle HUD, timing events, and cut-in/video presentation families.
- Camera/cinematic evidence separates framing offsets, plot/dialog durations, lip/audio presentation, camera signals, and table inventory.

Evidence boundary:

- The HI3 pass is derived from public community repositories and helper tables.
- It is not decoded official battle-engine code, authoritative formulas, or live runtime traces.
- The exact 2021 Global `StageData_Main` control verifies one historical target row and field shape only. Its partial-clone target object is closed, but the full repository is not audited; it is neither a current Ark source nor a license-resolution substitute.

### Aether Gazer, with ZZZ cross-check

Primary evidence:

- `games/aether-gazer/read-first/aether-gazer-development-context-direct-readfirst-slices.md`
- `games/aether-gazer/enemies-stages/index.md`
- `games/aether-gazer/enemies-stages/aether-gazer-stage-topology-wave-context.md`
- `games/aether-gazer/notes/combat-stage-readable-joins-2026-06-15.md`
- `games/aether-gazer/camera-animation/index.md`
- `games/aether-gazer/camera-animation/aether-gazer-timeline-cinemachine-asset-reading-pack.md`
- `games/aether-gazer/cutscene-cinematic/aether-gazer-arvick-p0-story-presentation-timing-motion-matrix.md`
- `games/zenless-zone-zero/enemies-stages/index.md`
- `games/zenless-zone-zero/enemies-stages/zzz-public-code-candidate-client-code-d0-levelworld-floor-group-member-stage-layout-summary.json`
- `games/zenless-zone-zero/ui/zzz-ui-motion-transition-bridge-rollup.csv`

Observed production shapes:

- Aether Gazer separates stage-like records, ordered topology/waves, restrictions, modifiers, unlock/result metadata, and presentation timing rather than putting every concern in one scene controller.
- The useful minimum hierarchy is `stage -> segment/pocket -> encounter group/wave -> member/spawn -> anchor`; ZZZ floor/group/member placement rows independently support this shape.
- Aether camera evidence uses named surface presets plus explicit set/reset behavior, and composes camera, actor, voice, effect, fade, and text timing.
- ZZZ tutorial evidence separates guide-step timing, masks, highlight targets, button targets, and popup/media resources. This supports separating lesson completion evidence from its presentation cues.
- Stage restrictions, revive policy, affixes, public buffs, scaling, skill additions, and enemy payloads are separate catalogs. This supports a later split among `StageRuleSet`, `StageModifierDefinition`, and `EnemyVariantProfile`.

Useful scale signals from the current snapshot:

- Aether stage-like rows: 6,514; stage reference rows: 8,658; topology/wave rows: 2,339.
- Aether stage-affix context rows: 1,082; affix definitions: 1,134; public buffs: 3,667.
- ZZZ floor rows: 21; group rows: 878; floor/group links: 864; member placements: 4,155.
- ZZZ derived guide rows: 689 mask/click-timing; 1,069 mask-target-classified highlights; 76 button/tab-target-classified highlights; 232 popup/media. These signal-family counts are not raw `HighLightButtonClick` boolean counts.
- Those 689 steps preserve array order under 144 `NewbieGroup` records, but expose no trustworthy stage/course/practice/challenge/result/reset join. Use them only for presentation targeting and media separation.

Evidence boundary:

- The historical Aether Gazer section records a comparatively dense static-field inventory, but those rows are not exact admitted records in the current evidence index and are not official runtime code or shipped traces.
- ZZZ is supporting public-data evidence and should not be described as official client source.
- Snowbreak's current archive is mainly external MAA automation. It is excluded as game-internal design evidence and retained only as inspiration for an editor-only `expected state -> action -> next state -> timeout/retry` smoke verifier.

### Blue Archive and NIKKE progression cross-check

Primary evidence:

- `games/blue-archive/enemies-stages/bluearchive-stage-encounter-raw-field-audit.csv`
- `games/blue-archive/raw/schaledb/2026-06-14/files/extracted_repo/SchaleDB-main/data/en/stages.json`
- `games/blue-archive/raw/schaledb/2026-06-14/files/stage-summary.csv`
- `games/nikke/raw/alt3ri-nikke-data/2026-06-13/files/stage-wave-join.csv`
- `games/nikke/raw/alt3ri-nikke-data/2026-06-13/files/stage-reward-summary.csv`
- `games/nikke/raw/epinelps-server-schema-source/2026-06-21/files/selected-source/EpinelPS/LobbyServer/Stage/ClearStage.cs`
- sibling `EnterStage.cs`, `CheckCleared.cs`, `FastClear.cs`, and `Lostsector/GetPerfectReward.cs`

Observed production shapes:

- Blue Archive campaign data separates opaque star conditions, challenge conditions, entry cost, normal/default reward, first-clear reward, and three-star reward surfaces.
- The audited campaign slice has 216 stage rows, 432 star-condition rows, and 208 nonempty challenge-condition rows; eight stages have an empty challenge-condition array. Reward rollups show 400 default, 400 first-clear, and 400 three-star locale-stage entries, with 216 unique stages represented in each bucket; counts must not be interpreted as unique live payouts.
- The useful local contract is typed `MasteryObjectiveResult { objectiveId, achieved, actualValue }`, not copied numeric condition codes or turn goals.
- A single `RunRewardPlan` with conditional buckets is safer than parallel first/repeat plan fields: start conceptually with `EveryClear`, `FirstClear`, and `FirstMastery`.
- NIKKE campaign rows support stage category/type, power/time/reward references, but the inspected static data did not prove prerequisite or retry-cost semantics.
- The EpinelPS material is an external server reimplementation, not shipped behavior. Its separation of stage result, completed-stage state, reward response, and save is only indirect architectural evidence.
- More importantly, unchecked first-clear/perfect-reward TODO paths in that reimplementation are negative examples: reward resolution needs an idempotency key and must compare against prior persistent state.

Evidence boundary:

- SchaleDB and NikkeData are game-data-derived snapshots that support field/bucket structure, not authoritative runtime grant or save order.
- EpinelPS supports vocabulary and failure-mode analysis only. It must not be copied as a server implementation.
- Neither dataset proves retry refunds, stamina consumption timing, duplicate-prevention behavior, or an authoritative prerequisite graph. Free immediate retry remains the current safe policy.

### Wuthering Waves tutorial, enemy, and presentation cross-check

Primary evidence under `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/`:

- `InstanceDungeon.json`: 1,395 rows
- `GuideGroup.json`: 1,417 rows
- `GuideStep.json`: 4,439 rows
- `ComboTeaching.json`: 467 rows
- `ComboTeachingCondition.json`: 959 rows
- `GuideTutorial.json`: 496 rows
- `GuideTutorialPage.json`: 834 rows
- `MonsterInfo.json`: 249 rows
- `MonsterPropertyGrowth.json`: 480 rows
- `AiBase.json`: 759 rows
- `AiBaseSkill.json`: 428 rows
- `AiSkillInfos.json`: 4,274 rows
- `AiSkillPrecondition.json`: 990 rows
- `QuestData.json`, `Flow.json`, `FlowState.json`, and `FlowTemplateData.json`

Observed production shapes:

- Dungeon briefing keeps map/type, entity level, trial formation, monster preview, entry condition, recommendation, reward references, guide type, settlement button, and difficulty as references under one stage-like record.
- Tutorial presentation and combat attempt logic are distinct. `GuideStep` exposes success, failure, tick, skip, break, duration, minimum duration, and show delay; `ComboTeachingCondition` separately exposes complete/fail and cleanup-shaped fields.
- Of 959 combat-teaching condition rows, 770 contain a failure condition and only 15 populate buff removal. Of 1,417 guide groups, 302 opt into in-dungeon reset. Projectile/summon cleanup fields exist but are empty in this snapshot. These counts support an explicit attempt/reset authoring boundary, not copied numeric meanings or a claim that runtime cleanup executes.
- The guide joins are not perfectly closed: the audited snapshot has one missing GuideStep reference, one missing GuideGroup reference from ComboTeaching, and four missing ComboTeachingCondition references. Validators must fail or quarantine unresolved local references rather than normalize external drift into defaults.
- `GuideTutorial` contains 496 rows and 775 page references to 767 unique pages; all resolve into 834 `GuideTutorialPage` rows. `TutorialOrder` is zero throughout, so only the authored page-array order is evidence.
- Enemy data separates identity, growth, base behavior, skill set, individual skill, and skill preconditions. The inspected reference chains join cleanly inside the AI tables, but a final `MonsterInfo -> AiBase` runtime join was not proven.
- Quest/flow/template data supports a lifecycle-shaped story handoff: outcome requests a flow, presentation acquires camera/dialog state, then camera, input, and HUD ownership must be restored.
- `RepeatRewardId` and `LimitTime` are nonzero in zero inspected dungeon rows. Field presence does not justify repeat rewards or timed stages.

Local promotion:

- Keep P1 order unchanged. Strengthen P1-E as `LessonPresentation + LessonAttemptContract + GameplayResetPolicy`; test every supported complete/cancel/interrupt/scene-exit path and explicitly test rejection of failure/skip/break/retry until each is reviewed.
- Keep P2-A bounded to one existing enemy reused through the current archetype/role/candidate/pattern/deck owners. Treat the external identity/growth/behavior/skill/precondition split as a long-term separation target; do not invent missing local owners merely to mirror it.
- Keep P2-B as a thin handoff into existing camera/cinematic systems, with restore invariants rather than a new flow framework.

Evidence boundary:

- This is preserved community ConfigDB data, not official runtime source. Exact condition codes, AI weights/cooldowns, camera coordinates, QTE/resonance/echo mechanics, and asset paths are not implementation inputs.

### Arknights stage graph and encounter cross-check

Primary evidence:

- `games/arknights/raw/arknights-game-data/2026-06-13/files/stage_table.json`
- `games/arknights/raw/arknights-game-data/2026-06-13/files/level_samples/level_main_00-01.json`
- `games/arknights/raw/arknights-game-data/2026-06-13/files/enemy_database.json`
- `games/arknights/raw/arknights-game-data/2026-06-13/files/stage-level-join.csv`
- heuristic QA aids `normal-td-stage-pacing.csv` and `normal-td-spawn-window-summary.csv`

Observed production shapes:

- `stage_table.json` separates stage identity, level reference, zone, unlock conditions, cost/practice policy, and reward-display metadata from the level JSON's options, enemy references, waves, fragments, and ordered actions.
- Of 3,428 stages, 2,995 have prerequisites. The inspected graph contains 3,076 `PASS` edges and 746 `COMPLETE` edges, including `tr_01 <- main_00-01:PASS`, `main_00-02 <- tr_01:PASS`, and a branch requiring both a prior `PASS` and local `COMPLETE`.
- The useful local shape is `RequiredStageState = Cleared(progressionNodeId) | MasteryObjectiveAchieved(progressionNodeId, objectiveId)`. Exact Arknights completion/star algorithms were not proven and must not be inferred from the names alone.
- Level actions expose reusable ordering vocabulary such as action type, target ID, count, pre-delay, interval, and route anchor. DimensionBrawl should map only the needed ordering to existing `SpawnRef` IDs and anchors, not copy tile, lane, deployment-cost, block-count, or life rules.
- Enemy deployment references combine identity, level/config variant, and stage-local override. This reinforces `EnemyIdentity + ConfigVariant + StageLocalOverride` without requiring another AI framework.
- Reward-display categories are rich, but the inspected first-pass/complete/pass grant arrays are empty. Display buckets do not prove actual payout, first-clear mapping, or a receipt transaction.
- Practice permission and costs vary, but no authoritative consumption/refund runtime was present. They do not justify stamina or paid retry.

Local promotion:

- Keep the authoritative order: completed `P1-0 -> P1-A -> P1-B`; then `prospective authoring-ledger freeze -> P1-C EncounterGroup -> P1-D Mastery/StageProgressState -> P1-E tutorial extraction -> P2-A0-A4 stage-variability foundation -> P2-B0-B5 presentation/course foundation -> joint P2-A5/P2-B6 active-restart lifecycle -> P2-C RunRewardPlan/RewardReceipt`. Accepted P1-B static authoring adds no P1-C execution, durable progress/reward, or pre-result restart ownership.
- Add typed prerequisite state and an explicit metadata-to-execution reference to P1-B.
- Use ordered encounter actions only as a narrow adapter over existing spawn IDs in P1-C; do not import tower-defense mechanics.

Evidence boundary:

- The stage/enemy JSON is preserved game data. The `normal-td-*` files are heuristic derivatives and may support editor pacing QA only; they are not authoritative runtime formulas.

### Girls' Frontline family stage, encounter, and result cross-check

Primary evidence:

- `games/girls-frontline/enemies-stages/gf1-mission-summary.csv`
- `games/girls-frontline/enemies-stages/gf1-mission-topology-profile.csv`
- `games/girls-frontline/enemies-stages/gf1-spot-map-summary.csv`
- `games/girls-frontline/enemies-stages/gf1-enemy-team-member-summary.csv`
- `games/girls-frontline-2/raw/torikushiii-gfl2data/2026-06-13/files/extracted_repo/GFL2Data-main/tables/StageConfigData.json`
- sibling `StageEnemyGroupData.json`, `StageChallengeData.json`, `StageChallengeConditionData.json`, `SimCombatTutorialSectionData.json`, `SimCombatTutorialProgressData.json`, and `TutorialGroupData.json`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/Sector/LevelDetail/UILevelRewards.lua`
- sibling `BattleDungeon/UI/UIDungeonResult.lua` and `UIDungeonFailureResult_Temp.lua`

Observed production shapes:

- GF1 preserves 953 mission identities and 20,457 in-stage spots/edges. This supports validating a stage-internal route for reachability, isolated nodes, and a valid entry-to-win path, but not importing its turn-based tactical map.
- GF2 provides the strongest ordered encounter join in this cohort: 2,952 `StageConfigData` rows reference ordered enemy groups, 8,889 `StageEnemyGroupData` rows reference ordered enemies, and the resulting placement surface contains 29,620 rows.
- GF2 separates 448 challenge rows from 43 evaluator grammars. Display names and opaque `T_*` formulas must not be parsed; the local contract remains `objectiveId + kind + typed parameters + actualValue`.
- GF2 also separates four simulation-tutorial sections/progress rows from 307 tutorial groups containing condition, finish condition, timing, pause, and finish-group references. This reinforces lesson trigger/proof/presentation separation but does not prove retry/reset semantics.
- GF1 enemy data separates 513 archetypes, 6,153 team definitions, and 43,609 team-member rows. GF2 likewise separates enemy identity from stage placement and its birth point, rotation, modification, dead-camera, and expanded-skill overrides.
- Neural Cloud client-flow material separates first/normal reward display and claim request from dungeon result, retry availability/cost, exploration exit, and restart. It reinforces owner separation only; it does not justify stamina or prove server-side idempotency.

Local promotion:

- Make `EncounterGroupSequence -> ordered placements/spawn references -> trigger/completion -> cleanup` an explicit P1-C acceptance boundary. Do not create a general event/trigger DSL.
- Keep typed mastery in P1-D, tutorial extraction/reset parity in P1-E, and enemy archetype/variant/placement separation in P2.
- Keep result, progression resolution, reward settlement/receipt, and retry/reset as separate owners. A later manual-claim UI, if ever justified, remains only a caller of the same settlement boundary. First-reward handling stays after truthful results and persistence.

Evidence boundary:

- GF1/GF2 static data is strong for configuration shape but does not prove runtime execution order by itself.
- Neural Cloud Lua is preserved decompiled client code from an older regional snapshot and can contain decompiler errors. Heuristic candidate packs are not promoted unless the referenced Lua was opened directly.

### CounterSide bounded evidence check

Primary evidence:

- `games/counterside/raw/davxwang-cs-caesarcipher-serialize/2026-06-18/extracted/CS-CaesarCipher-Serialize-7ddc13e86018a7736b7568127daad9f283912cd9/CS_CaesarCipher+Serialize.py`
- `games/counterside/cross-domain/counterside-runtime-context-combat-stage-readfirst.csv`
- `games/counterside/counterside-master-lua-acquisition-map.csv`

Observed boundary:

- The public tool names separate dungeon, map, and warfare template families, which weakly supports a `StageDefinition -> MapDefinition` reference boundary.
- The five relevant master tables are all `missing-proxy` and absent from the reader tree. The 1,258-row combat-stage read-first slice contains zero stage, quest, mission, reward, or schedule identifiers.
- Apparent summon/respawn fields belong to unit skills or states and are not evidence for stage waves. Prerequisite, route, wave, clear/mastery, first/repeat reward, retry/cost, result, and save structures remain unverified.
- Public sheets separately preserve movement/range, level stats, state/animation/cooldown, and attack-event surfaces. This is secondary support for enemy identity/stat/behavior/attack-set separation, not for copied numbers.
- The 147,342-row story-stage-transition index describes dialogue background/BGM/filter/actor-slot changes, not gameplay stage transitions.

Local promotion:

- No priority or contract change. Retain only the Stage/Map separation and enemy-layering hints as low-to-medium strength support.

Evidence boundary:

- Do not infer fields from table names, proxy status, or public search results. The tool's MIT license does not license game data, public-sheet values, story, Spine, audio, or image assets.

### Ash Echoes stage, target, and reward cross-check

Primary evidence:

- `games/ash-echoes/raw/ash-echoes-gamedata/2026-06-14/files/extracted_repo/GameData-master/data/chapter_levels.dat`
- sibling `maps.dat`, `level_target.dat`, `level_achievement.dat`, `level_extra_target.dat`, `level_reward.dat`, `level_cutscene.dat`, `level_teaching_config.dat`, `npc_numeric.dat`, `npc_numeric_grade.dat`, and `npc_ai_templs.dat`
- `games/ash-echoes/enemies-stages/ash-stage-map-readable-join.csv`
- `games/ash-echoes/enemies-stages/ash-stage-target-monster-readable-join.csv`

Observed production shapes:

- The 2,877-row stage/map join resolves 2,562 map IDs, 2,302 objective descriptions, 1,007 first-reward references, 223 repeat-reward references, 204 affix references, and 257 specific target-enemy IDs.
- A representative stage joins chapter/level metadata through `map_info` to a map resource and `scripts/maps/.../scene.lua` path. The referenced map script is absent from the archive, so metadata-to-map/script is proven while runtime encounter execution is not.
- `chapter_levels.dat` contains 3,235 level IDs, 777 populated prerequisite links, 750 populated next links, 1,159 fail-retry flags, and 792 hidden-result flags. This reinforces explicit route/progression IDs and result-presentation policy without proving saved progress or retry transactions.
- Required clear targets are separate from 148 achievement records with 324 target references and 110 typed extra-target definitions. Direct evaluator vocabulary includes time, skill, buff, kill, death, and actor-add checks with parameters and comparison types.
- Reward authoring separates first and common rewards, while achievement targets have their own reward reference. No grant response, prior-claim state, save, idempotency key, or receipt is present.
- `combat_wave_bar.dat` describes 30 wave-label/progress records but has no enemy ID, quantity, spawn point, or timing schedule. It is not P1-C ordered-spawn evidence.
- Teaching data links 25 lesson IDs through prerequisite/next edges and stage references. It supports a tutorial catalog/course graph but not input proof, attempt result, or reset parity.
- Enemy data separates stage-level bonuses/passives/affixes, numeric templates and grades, and AI templates. The missing map scripts prevent a complete stage-spawn join.

Local promotion:

- Strengthen P1-B explicit prerequisite/next and metadata-to-map references.
- Strengthen P1-D separation of mandatory clear from optional typed mastery, but keep factual `RunResult` collection first.
- Strengthen P2 conditional reward buckets and enemy numeric/AI/stage-override references without moving them earlier.

Evidence boundary:

- The `.dat` fields are direct static configuration and the CSV joins are traceable derivatives. Map-script execution, ordered spawning, result packets, persistence, and reward receipts remain unverified. The preserved repository has no detected license; art, numeric tuning, elemental combat, formation rules, affixes, and roguelike content are not reuse inputs.

### Path to Nowhere bounded evidence check

Primary evidence:

- `games/path-to-nowhere/raw/ptndata/2026-06-14/files/extracted_repo/PtNData-main`
- `games/path-to-nowhere/raw/ptndata/2026-06-14/workbook-sheet-summary.csv`
- `games/path-to-nowhere/player-profile/ptn-nightmare-achievement-summary.csv`
- `games/path-to-nowhere/cross-domain/path-to-nowhere-runtime-context-combat-stage-readfirst.csv`

Observed boundary:

- All 53 preserved source files were inventoried. The archive contains character, skill, training-buff, profile, achievement, dispatch, and narrative data, but no stage/level runtime, encounter, wave/spawn, enemy master, result/save, or retry contract.
- The 1,400-row and 1,800-row files labelled `combat-stage` are derived slices of training buffs, skill coefficients, skill descriptions, and skill-category labels. Their filenames and `stage_signal` column do not establish gameplay-stage identity.
- Forty-two Nightmare achievement rows expose display order, hide flags, and natural-language conditions such as clear/win, party-size, modifier, move, hit, or killer constraints. They support collecting typed run facts, but do not expose evaluator type, parameters, progress counter, or stage join.
- `Mania_Training_Buffs.xlsx` contains 620 rows of ID, description, and icon only. This supports separating modifier display metadata from an executable payload reference; payload, trigger, and target must not be inferred from prose.
- Dispatch base/additional rewards and player-character compliance rewards are unrelated to stage first/repeat payout.

Local promotion:

- No priority change. Retain only two secondary lessons: mastery visibility/order is presentation metadata rather than evaluation logic, and `StageModifierDefinition` must separate display metadata from a typed executable payload/adapter.

Evidence boundary:

- This is unofficial, manually edited global-client text with no detected license. Natural-language achievements, tactical block/movement rules, event wave counts, skill/buff values, icons, and derived `combat-stage` labels are not runtime evidence or reuse candidates.

### Genshin Impact presentation lifecycle, with Honkai: Star Rail boundary check

Primary evidence:

- `games/genshin-impact/narrative/genshin-sycamore-talk-config-index.csv`
- `games/genshin-impact/narrative/genshin-sycamore-talk-performcfg-join.csv`
- `games/genshin-impact/raw/sycamore0-genshindata-selected/2026-06-15/files/extracted_selected/BinOutput/InterAction/QuestDialogue/AQ/Mengde2_372/Q37201pre.json`
- sibling `AQ/Inazuma2_2015/Q201505.json` and `EQ/V3.5FleurFlowerV2_40099/Q4009911.json`
- `games/genshin-impact/camera-animation/genshin-sycamore-interaction-camera-timing-direct-readfirst.md`
- `games/honkai-star-rail/ui/hsr-starrail-presentation-ui-camera-context-pack.csv`

Observed production shapes:

- The Genshin talk index contains 22,700 talk records, 8,632 perform links, 36 pre-perform references, and 3,785 next-talk links. A representative record separates pre-performance, main performance, multiple next talks, and finish hooks.
- A directly opened interaction file separates timeline asset, synchronous-load policy, fade-in/out policy, skippability, immediate-next behavior, and group/next-group IDs. This supports independent sequence, fade, skip, and completion-handoff fields, not the copied numeric timings.
- Another interaction explicitly shows a UI surface and later closes the same context with a separate close action. This supports acquire/release pairing for HUD/overlay ownership.
- Camera configuration separates movement, look-at, depth-of-field, transition, and actor staging. One direct sample enables DOF but no matching disable was proven, so cleanup cannot be inferred from an enable flag.
- No direct time-scale or input enable/disable action was found. World-time change actions are not Unity `Time.timeScale`. Stage result, retry, and replay-safe cleanup were also not proven.
- The inspected HSR pack is dominated by skill/ultimate presentation and story-message references; camera keyword hits include dialogue text rather than camera ownership. It is excluded from lifecycle decisions in this pass.

Local promotion:

- Keep a reusable presentation profile/adapter at P2-B. The evidence does not move it ahead of stage/result/spine work.
- Continue treating `complete / skip / cancel / disable / unload / retry` cleanup as a P0/P1 route safety invariant for existing content.
- The minimum P2-B runtime shape is `acquire captured presentation state -> play or skip/cancel -> release through one cleanup path -> commit one handoff -> retry from baseline`.
- Reuse `CinematicSequenceProfile` and fill its current execution gaps before inventing a general sequence graph or another profile family.

Evidence boundary:

- Genshin interaction JSON supports authoring shape, not shipped runtime guarantees. Helper CSV rows are used only when anchored back to opened source JSON. HSR is insufficient for ownership claims. Exact fade values, talk/action graphs, obfuscated keys, world-time behavior, and a single skip flag must not be copied or treated as proof of full cleanup.

### Fate/Grand Order quest-phase and after-clear policy cross-check

Primary evidence:

- `games/fate-grand-order/raw/atlasacademy-api/2026-06-14/files/data/NA/nice_war.json`
- sibling JP `nice_war.json`
- `games/fate-grand-order/enemies-stages/fgo-war-quest-summary.csv`

Observed production shapes:

- The 29,420-row NA/JP summary contains 8,303 quests with phase-script references, 8,586 with no-battle phases, 1,273 mixing battle and no-battle phases, and 7,313 containing only no-battle phases.
- A representative quest mixes an enemy phase with a following no-battle/story phase. This directly supports neutral `Battle` versus `StoryOnly` phase kinds, but script-array positions do not prove before/after semantics.
- Explicit quest-clear release conditions occur broadly, while `enableFollowQuest` is only a policy hint and does not prove an automatic UI transition.
- `afterClear:"repeatLast"` and loop-mark fields directly support a repeat-after-clear policy. They are not evidence for failure retry.
- Battle-result UI, failed-run retry, actual next-quest navigation, and camera/input/HUD cleanup are absent from the preserved runtime evidence.

Local promotion:

- Keep P2-B and progression order unchanged.
- Preserve separate typed concepts for `phaseKind`, neutral story-script references, `afterClearPolicy`, prerequisite/release conditions, and follow-quest policy.
- Keep failed-run Retry, clear Replay, next stage, and Lobby as distinct route actions. The current P1-0 recommendation is `Clear -> Replay + Lobby`, `Fail -> Retry + Lobby`; FGO supports only the static repeat-after-clear distinction, not these local targets or buttons.

Evidence boundary:

- Atlas Academy master data supports quest/phase policy shape, not official client UI/runtime behavior. Do not label script array positions as story-before/story-after, interpret `repeatLast` as retry, assume `enableFollowQuest` auto-navigates, infer camera cleanup from reset-name lists, or copy AP/reward values.

### Limbus Company battle-story and result-data cross-check

Primary evidence:

- `games/limbus-company/raw/pggb-limbus-company-data/2026-06-17/files/extracted_repo/Assets/Resources_moved/StaticData/static-data/battle-story/*.json`
- `games/limbus-company/raw/pggb-limbus-company-data/2026-06-17/files/extracted_repo/Assets/Resources_moved/StaticData/static-data/stagenodereward/*.json`
- preserved `storytheater-main-100.json`, `limbus-story-theater-surface-map.csv`, and `illust-pivots-br.json` cross-checks

Observed production shapes:

- Six battle-story JSON files contain 52 stage records: 39 have `story.enter[]`, 19 have `story.exit[]`, 18 have both, 19 have `stageScriptNameAfterClear`, and 47 contain multiple waves.
- Tutorial stage `10001` directly joins `story.enter: [S001B]`, its battle wave, `story.exit: [S001A]`, and after-clear script `Tutorial_1`. The story-theater source classifies `S001B` as `Before` and `S001A` as `After` for the same node.
- Across the preserved theater map, 38 of 39 enter references resolve to a `Before` story and all 19 exit references resolve to an `After` story. This is stronger role-labelled static evidence than inferring before/after from script-array position.
- Node identity is not interchangeable with battle-stage identity: only 29 of 39 enter and 18 of 19 exit references also match the same theater `nodeId`. One contract must not silently reuse a battle ID as a progression-node ID.
- Result illustration pivots and 66 node-reward records show separate static illustration-pivot metadata and normal/ex-clear reward authoring surfaces. They do not prove result-screen consumption or layout, the runtime order from victory through result UI to exit story, or any grant/save transaction.

Local promotion:

- Keep the current order unchanged, but keep `battleStageId` and `progressionNodeId` distinct and author explicit `preBattleStoryIds[]`, `postBattleStoryIds[]`, and optional `afterClearScriptId` references.
- Dispatch post-battle story and after-clear hooks only after one committed victory result. Fail, abort, and retry paths must not run post-clear hooks.
- Treat result illustration, reward plan, result UI, progression mutation, and story handoff as separate owners even if the first local slice presents them in one visible sequence.

Evidence boundary:

- This is historical section-only static-data shape, not a current evidence-registry grade or shipped execution code. The archive does not prove result-screen order, retry, skip/replay execution, camera/UI/input/time-scale cleanup, or the order and side effects of `story.exit` versus `stageScriptNameAfterClear`. Do not copy IDs, script names, reward values, illustration pivots, or infer automatic next-stage flow from theater row order.

### Heaven Burns Red insufficient-evidence boundary

Primary evidence:

- `games/heaven-burns-red/raw/hikarimy-data-center-web/2026-06-14/hbr-data-center-web-analysis-summary.json`
- sibling `hbr-sql-table-field-summary.csv`
- `games/heaven-burns-red/raw/kedzkiest-reproduce-heavenburnsred-walking/2026-07-10/files/source/Reproduce_HeavenBurnsRed_Walking/Assets/C#/PlayerControllerUsingDollyCart.cs`

Observed boundary:

- The 128-file Data Center source is a fan web application with 12 character/event/guide-oriented SQL tables. It contains no story, stage, battle-result, replay, or runtime ownership tables.
- Its 333 web routes and 133 motion rows are links/fetches/CSS-web motion, explicitly not in-game scene or camera transitions.
- The fan Unity source recreates walking in two scenes with two C# scripts. Input is polled directly and the code has no story/battle/result/skip/replay flow, input gate, UI restore, scene transition, `OnDisable` cleanup, or time-scale restoration.
- Static Cinemachine paths and empty camera events in that fan project are not an ownership lifecycle or official HBR behavior.

Local promotion:

- None. P2-B remains unchanged. Revisit only if actual story graph, battle-result controller, skip/replay state, or runtime traces become available.

Evidence boundary:

- Do not interpret website navigation/motion, a fan walking controller, empty Cinemachine events, installed Timeline packages, FOV/blend/speed values, or path coordinates as shipped HBR presentation policy. Both sources are unofficial and reuse-review-needed.

### Brown Dust 2 third-party viewer cleanup boundary

Primary evidence under `games/brown-dust-2/raw/jelosus2-bd2-l2d-viewer/2026-07-11/files/selected/src/`:

- `utils/cutscene_mappings.ts`
- `components/SpineViewer.vue`
- `components/AnimationSideBar.vue`
- `components/CharacterSideBar.vue`

Observed boundary and useful failure checks:

- The MIT third-party viewer contains 15 composite definitions and 46 segments with name, offset, source, skin, and hold metadata. Its runtime additionally tracks track/start/duration/additive/hold-until state.
- `resetComposite()` invalidates a generation token and clears timeline, progress, overlay, and layer state. Component or animation changes cancel export before reset/restart.
- Unmount removes keyboard/pointer/wheel/click and camera listeners, cancels tracked animation frames, clears caches, destroys the player, and disconnects its resize observer. Seek and export cleanup restore camera and playback state through additional paths.
- The implementation is a failure checklist rather than a complete model: some animation-frame handles are not stored, and camera/play restoration is separate from the main composite reset path.
- No game stage/story/result/retry contract, global time-scale ownership, or shipped Brown Dust 2 presentation lifecycle is present in the inspected archive.

Local promotion:

- Keep P2-B in place. Add a request-generation token so a load/start/export completion from a terminated presentation cannot reacquire input, HUD, camera, audio, actor, fade, or playback ownership.
- Track and cancel every owned coroutine, timer, animation-frame callback, async completion, listener, observer, stream, player, and transient cache through the common terminal path.
- Use the viewer only to enumerate cleanup failures. Reuse the existing DimensionBrawl runner/profile and captured-state contract rather than its viewer architecture.

Evidence boundary:

- Viewer code is third-party MIT material, not shipped game-client behavior. Game assets, animation paths, exact timing, camera values, and extracted media remain outside this roadmap and must not be copied. The archive cannot promote story/result/retry behavior.

### Reverse: 1999 result/progression join, with Epic Seven boundary check

Primary evidence under `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/`:

- `lua/modules/logic/fight/rpc/FightRpc.lua`, especially `EndFightRequest` and `EndFightPush`
- generated `DungeonModule_pb.lua` and `DungeonDef_pb.lua`
- `lua/modules/logic/fight/model/FightResultModel.lua`
- `lua/modules/logic/dungeon/rpc/DungeonRpc.lua`
- `lua/modules/logic/dungeon/controller/DungeonController.lua`
- `lua/modules/logic/dungeon/model/DungeonModel.lua`
- `lua/modules/logic/dungeon/config/DungeonConfig.lua`
- `json/episode.json`

Observed production shapes:

- `EndFightRequest` sends only abort intent, while `EndFightPush` records the authoritative fight result before publishing the local end-fight event.
- `EndDungeonPush` contains episode ID, star, first-pass state, and categorized first/normal/advanced/time-first bonus payloads. `FightResultModel` consumes those categories separately.
- `UserDungeon` carries episode ID, star, and challenge count. The update handler checks first pass against the prior local dungeon state before mutating it; the check compares new `star > 0` with prior `star == 0`.
- The 3,969 unique episode rows contain 1,587 valid internal `preEpisode` references plus smaller valid pre/unlock/chain reference sets. Runtime config/model code builds reverse next links and evaluates predecessor pass/star state.
- No receipt ID, request/transaction correlation, idempotency key, server database transaction, or atomic progression/reward commit was proven.

Local promotion:

- Keep priority order unchanged, but strengthen the later progression/reward acceptance sequence to `snapshot prior progress -> validate the committed result -> purely compute first/new state and eligibility -> atomically commit progress/inventory/receipts -> display the committed categorized payload`.
- Continue treating `RunResultSummary`, `StageProgressState`, `ProgressionResolution`, reward payload, and idempotent `RewardReceipt` as separate contracts. `EndDungeonPush` is a result/reward payload, not proof of a durable receipt.

Epic Seven boundary:

- The inspected README explicitly says stage scripts/enemy AI are not exposed and lists enemy/stage/PvE as future collection targets. The API manifest contains zero downloaded files/bytes. No prerequisite, result, persistence, reward, or receipt join is available, so Epic Seven contributes no current decision evidence.

Evidence boundary:

- Reverse: 1999 Lua/protobuf/JSON gives strong client-side ordering and field evidence, but server durability and atomicity remain inference. The public extracted source has no detected reuse license. Do not copy text, item IDs, quantities, economy values, or treat derived read-first tables as stronger than the opened source.

### Last Origin stage/reward references, with Princess Connect boundary

Primary Last Origin evidence under `games/last-origin/raw/hibikidesu-lastorigin-data/2026-06-18/files/extracted_repo/lastorigin-data-master/jp/table/`:

- `table_mapstage/table_mapstage.json`
- `table_mobgroup/table_mobgroup.json`
- `table_missionobject/table_missionobject.json`
- `table_reward/`
- `table_stagerewardview/table_stagerewardview.json`

Observed production shapes:

- The 761 stage records independently reference `NeedClearStageIndex`, `NextStageIndex`, up to nine wave groups plus a boss group, base `RewardIndex`, `Stage_FirstClear_Reward`, `Stage_AllMissionClear_Reward`, four stage missions, and clear-grade conditions.
- Stage-to-encounter joins resolve through 6,647 mob groups and their rank reward references into separate reward payload records. The inspected snapshot has 2,264 wave/boss-group references and 2,728 mission references with no missing targets.
- All 398 `NextStageIndex` and 663 `NeedClearStageIndex` references resolve, but only 392 links are reciprocal and six are intentionally or historically asymmetric. A recommended next route and a required completed-stage set are distinct directed relations; validators must not force inverse symmetry.
- The inspected readable reward fragments resolve all 720 first-clear and 658 all-mission reward references. `RewardIndex` is a distinct base authoring channel, but the static archive does not prove that it grants on every repeated clear.
- The 783-row reward-preview table has 111 stage keys absent from the inspected main/EW stage set. Preview rows are a drift-prone read model, not authoritative payout or stage identity.
- No result/progression/reward application code was found in the preserved 235 AI-oriented Lua files. Prior-state comparison, exactly-once apply, durable receipt, and idempotency remain unproven.

Local promotion:

- Normalize source `NextStageIndex`/`NeedClearStageIndex` values into `recommendedNextProgressionNodeId` and `requiredCompletedProgressionNodeIds`. Keep those directed relations separate, validate node-target existence and allowed cycles, and never require reciprocal edges.
- Strengthen P1-C as `playable stage -> ordered wave/group refs -> existing encounter payload/spawn refs`, while keeping group rewards outside combat execution.
- Keep `baseRewardRef`, `firstClearRewardRef`, and `allObjectivesRewardRef` as separate authoring references. A local resolver defines eligibility from committed results and prior progress; source labels do not prove repeat-grant policy.
- Generate reward preview from the authoritative local plan and resolution instead of treating preview/catalog rows as payout owners.
- Keep the current priority order. The data sharpens P1-B/P1-C/P2-C validators but does not replace P1-A result truth or the local receipt/idempotency design.

Princess Connect boundary:

- `games/princess-connect-redive/raw/redive-master-db-diff/2026-06-13/files/extracted_repo/redive_master_db_diff-master/` contains 1,910 SQL and 50 text files, but no runtime code, player prior-state, or server result payload.
- The strongest join is 7,206 event-quest candidates to 5,522 event-mission candidates, with 3,374 mission rows referencing an existing quest ID. This supports a separately referenced `StageObjective/MissionDefinition`, not first/repeat/mastery reward, next-stage unlock, or result execution.
- Most columns are hashed. Reward-like numbers, parent-like references, and mission text cannot be promoted to typed semantics or payout joins without decoded fields and runtime evidence.

Evidence boundary:

- Both sources lack detected reuse licenses. Do not copy game IDs, hashes, text, reward values, drop rates, mission rules, or payloads. The historical Last Origin section records static relationships without grant execution, and the Princess Connect section records only a conservative quest-to-objective boundary; neither is assigned a current evidence-registry grade.

### Stella Sora tutorial settle/reward audit, with remaining-archive boundary

Primary evidence:

- `games/stella-sora/raw/hiro420-stellasoradata/2026-07-10/files/extracted_repo/StellaSoraData-11d4de519b787972d9045dcbd7e134ecf3a5b408/EN/bin/TutorialLevel.json`
- sibling `TutorialLevelFloor.json`
- community emulator files under `games/stella-sora/raw/melledy-nebula/2026-07-10/files/extracted_repo/Nebula-81362f97a8a4aeddf175a0edf6a941b54f491722/`: `TutorialLevelDef.java`, `TutorialLevelLog.java`, `TutorialModule.java`, settle/reward-receive handlers, and the two generated protocol outer classes
- both snapshots' `source-record.md` files

Observed static-data and community-emulator shapes:

- Eight tutorial-level records join eight floor records through unique `FloorId -> Id` references with no missing or orphan targets. The floors hold scene/prefab/script type, ordered quest-flow IDs, BGM/leave event, theme, and monster level; the 31 quest-flow IDs are ordered authoring references, not proof of runtime completion.
- `TutorialLevelDef` names the exact level JSON and exposes level ID plus world and item/quantity-shaped fields. It omits `FloorId`, build/type/title, and the floor table, so this server definition does not prove consumption of the otherwise valid stage/floor spine.
- Both handlers parse one generic unsigned integer and delegate it as `id`; the generated settle/reward protocol outer classes are empty. There is no dedicated run ID, outcome proof, claim ID, receipt ID, or request correlation in the inspected wire surface.
- `settle` returns success immediately when the prior tutorial log already contains the ID. Otherwise it validates the level ID against the data table, creates an unclaimed log, inserts it into player progress, and calls a database update. It trusts a client-supplied level ID and observes no combat outcome, run facts, mastery proof, or result payload.
- The log serializes separate passed and reward-received states. `recvReward` rejects a missing or already-claimed log, validates the level data, marks the log claimed, calls the database update, and only then adds the item payload to inventory.
- The claim flag is a direct duplicate guard, but it is not a durable idempotency receipt. A repeated request returns failure rather than the original result, and no transaction, rollback, outbox, receipt replay, or atomic relation between claim persistence and inventory mutation is shown. Saving the claim before adding inventory is a concrete lost-reward failure risk if the later operation fails.

Local promotion:

- Keep priority order unchanged. This source strengthens P1-A's rule that an authoritative combat outcome must commit before progression; a client-supplied stage ID is never clear proof.
- Strengthen P2-C acceptance to `validate committed run and prior progress -> resolve payload -> atomically commit progress application/inventory/receipts -> return the same receipt on a duplicate settlement request`. A boolean claim guard alone prevents some duplicates but does not provide exactly-once delivery.
- Keep authored stage/floor references, server progress keys, result facts, reward eligibility, inventory mutation, and receipt as separate contracts. Do not call an item/quantity-shaped field a granted reward until the resolver and receipt prove it.
- The remaining-archive preflight is closed. Azur Lane, Fire Emblem Heroes, MementoMori, Touhou LostWord, and Uma Musume currently lack the runtime-resolution axis; BanG Dream/Project Sekai are rhythm/API-oriented, the HoYoverse pack is rendering-only, Snowbreak remains automation/model-heavy, and Magia Record is scenario/asset-route oriented.

Evidence boundary:

- The data snapshot is GPL-3.0 datamined/reference material and the AGPL-3.0 code is a separate community server emulator. Neither is official Stella Sora runtime/server evidence, and their cross-source field match does not prove identical shipped bytes or behavior. Do not copy code, IDs, text, item values, tuning, or protocol details; retain only independent contract separation, the vulnerable ordering, and its potential failure mode.

## Gap Classification

| Gap | Classification | Evidence | Current impact |
|---|---|---|---|
| Canonical end-to-end demo route | Accepted P0 baseline complete; P1-0 route and D4b revision-1 contract frozen | Accepted uninterrupted full route, natural handoff, terminal-button suites, and 28/28 baseline; scoped 14/14, full 1/1, buttons 2/2, inventory/bypass-zero validator PASS, and route-frozen GPU aggregate 37/37 | Preserve the accepted baseline and frozen-route evidence as continuous regression gates; the technical freeze is invalidated by later validator or route regression failure |
| The stage-wide run context preserves route/handoff, terminal epoch closure, current-schema finalization coverage, a schema-2 durable summary, and verified facts | Accepted P1-A current-schema lifecycle boundary | Historical 68/75 cutoffs retain their bounded meanings; the final 11-source cutoff passes Combat 21/21, StageRun 23/23, UI 15/15, aggregate 79/79, full route 1/1, and validator PASS | Current-schema full exit is closed. Future admitted-owner receipts are later-schema proof |
| Stage design data is not consumed by a canonical reusable runtime route executor | Missing production bridge; separate prototype exists | Linear stage docs explicitly exclude runtime spawning; `PveEncounterDirector` consumes its own raw PVE placement model instead of the playable-stage/SpawnRef spine | Authored stage data cannot become repeatable content without bespoke scene code, while blindly adding another executor would create a third spawn authority |
| Tutorial ordering, presentation, allowed input, encounter setup, completion evidence, and exit cleanup share one large scene director | Existing strong feature that should be separated incrementally | Current tutorial director; PGR course/practice catalogs; ZZZ presentation targets; Wuthering success/fail/skip/break/reset fields | New lessons or practice stages require more branching; replacing the evaluator too early risks regressions in working input-lock, normal-path cleanup, and event proof while cancel/disable/unload restoration is still incomplete |
| No executable `BasicLesson -> FreePractice -> SummonMasteryChallenge` chain exists | Missing P2-B composition and lifecycle boundary | Station guide is cue-only; Boss Barrage guide is not product-bound; no Free Practice entry/exit/baseline owner exists; PGR separates Tutorial/Challenge nodes and practice follow-ups | The summon-first identity cannot be taught, rehearsed without judgment, then truthfully tested; prematurely wiring the current guide would mistake acknowledgement/local booleans for proof and carry dirty gameplay state into Challenge |
| The frozen playable-stage route is not yet joined to typed prerequisites, progression metadata, or chapter-map projection | Remaining P1-B/P1-D integration islands | Frozen `PlayableStageDefinition` route shell plus StageDefinitionProfile, LinearStageTemplateProfile, ChapterMapPrototypeStageNode, Arknights stage-to-level graph separation, Ash Echoes explicit pre/post/map references, and Last Origin's independently directed prerequisite/next links | Route identity is canonical, but duplicate strings and manual wiring can still drift across content selection, result, progression, and unlock UI |
| Runtime forward destination authority has moved to the frozen snapshot; validation aliases remain | Resolved P1-A runtime drift with bounded static validation constants | `LoadTutorialCombatScene` now consumes `StageRunSingleLoadDispatch.DestinationScenePath/Name`; remaining Station literals are fail-closed validator/build-readiness expectations rather than load authority | Preserve snapshot-owned runtime dispatch. P1-B may enrich static joins but must not recreate a scene-string destination owner |
| Canonical stage-select projection blocker remediation is resolved | Candidate-05 accepted canonical-selection subgate; Candidate-04 historical FAIL | Candidate-04 exposed the missing authored row and stale blank-selection bundle. Candidate-05 separately proves the exact empty inactive product row, invalidates null/empty/whitespace/unknown selections before typed rejection, and passes 19/19 sources, validator, 8/8, UI 21/21, full route 1/1, and aggregate 86/86 | Preserve both immutable cutoffs separately. The accepted row is presentation selection only; do not infer briefing, reward/progression, or run ownership |
| Intro direct identity plus static port/binding and anchor/profile hygiene are resolved | P1-B accepted / verified complete | the earlier cutoffs remain immutable; later selection, truthful joins, result/progression, Station Add Remediation3, explicit foreign rejection, and the final 128-source audit close the full promotion gate | Preserve all P1-B cutoffs; next freeze the prospective P1-C authoring ledger |
| Mastery and clear-condition intent is mostly stored as strings rather than typed condition plus parameters | Missing evaluator contract | HI3 `StageChallengeData`, GF2 and Ash Echoes challenge/evaluator separation, Path to Nowhere visibility/order boundary, and current `masteryObjective`, `clearCondition`, and cue strings | UI copy can claim a condition that runtime never measured, or different stages can interpret the same text differently |
| Stage briefing fields are split across level data, stage-select catalogs, scene logic, and UI | Existing data that should gain one read model | HI3 stage row joins and current project profile/catalog split | Stage card, loading/briefing, runtime, and result can disagree about objective, route, or recommendation |
| Clear/Fail UI now renders committed clocks and qualified proof through a bounded presentation-only snapshot; future reward and admitted-owner surfaces remain separate | Resolved current-schema P1-A presentation slice, with later P2-C/admitted-owner boundaries | Exact profile/localization, hidden placeholders, post-49 UI 11/11 and aggregate 54/54; post-54 UI 12/12 and aggregate 59/59 reject summary-digest mutation; exit-candidate UI 15/15 and aggregate 68/68 add direct route/result/current-schema/coverage/receipt and dispatch-closure integrity; historical UI 15/15 and aggregate 75/75 preserve them under the remediation manifest; final UI 15/15 and unchanged-source aggregate 79/79 preserve the presentation boundary while closing the remaining current-schema exit rows | Preserve source result ID/digest and presentation-only provenance; do not let UI invent facts, rewards, navigation, or future admitted-owner success |
| No persistent stage unlock/clear/mastery state was found | Missing progression state | Repo search and prototype-only clear flags | Stage select cannot become a real chapter loop |
| Enemy roles are not yet a general variant/configuration matrix | Existing role system that should be expanded | HI3 monster AI/stat/config separation; Wuthering identity/growth/behavior/skill references; Arknights and GF1/GF2 stage-local composition/placement overrides | Reusing enemies across story, tutorial, and challenge modes risks prefab or scene duplication |
| Stage conditions and mode modifiers are not first-class data | Missing extension | HI3 stage condition/buff rows; Path to Nowhere display-only buff records | New stage rules would tend to become one-off scene scripts or infer executable behavior from presentation prose |
| Ordered encounter groups/waves do not bridge route intent to concrete spawns | Missing canonical production bridge | Aether topology/waves, ZZZ floor/group/member placement, Arknights ordered level actions, GF2 stage/group/placement joins, Last Origin stage-to-wave/mob-group references, and the isolated local PVE prototype | A new bridge could duplicate `LinearStagePocket`, `StageDefinitionProfile.SpawnRef`, or raw PVE placement fields unless one stage-local binding and one runtime owner are fixed first |
| Tutorial completion evidence and mask/highlight/prompt/media presentation are not reusable layers | Existing feature that should be separated | PGR lesson catalogs, ZZZ guide-target/media rows, and GF2 section/group finish-condition separation | New lessons can duplicate overlay logic or bind directly to brittle scene paths |
| Input, loadout, revive/retry, and cleanup restrictions are not one stage-rule contract | Missing extension | PGR restrictions and Aether stage rules | Restrictions can leak past success, skip, retry, or scene handoff and recreate soft locks |
| Camera/cinematic systems exist, but transition cleanup is not one route-wide executable contract | Validation and adapter gap, not a missing camera system | Aether set/reset lifecycle; Wuthering flow acquire/close patterns; Genshin show/close and skip/fade separation; Brown Dust 2 third-party viewer generation-token/unmount failure checks; `CinematicSequenceRunner.Stop()` restoration; Olympus flow/tests | A visually correct path can still leave stale input, HUD, camera/DOF, time scale, listener, actor visibility, disabled behaviours, or late async callbacks on skip/cancel/retry/scene exit |
| Stage entry/exit story handoffs are not a thin data link to the existing cinematic system | Missing integration, not missing presentation capability | HI3 stage-to-plot/dialog links; Genshin pre/perform/next/finish joins; Limbus role-labelled before/after story references; `StageDefinitionProfile.CutsceneHandoffRef`; `CinematicSequenceProfile` stage/handoff fields; usage audit | No production consumer was found joining the local handoff-ID surfaces. The profile records return mode/target/input delay/HUD/time-scale restore intent, but the generic runner mostly marks handoff reached and consumes camera restore; bespoke scene wiring still owns actual skip and gameplay release |
| Reward/growth contracts exist only as research and provisional review artifacts | Planned, not implemented | STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md; [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md); Last Origin's separate base/first/all-objective authoring references and drift-prone preview boundary | Result and replay motivation cannot mature beyond a shell, and a preview catalog could become a false payout owner |
| Current run facts have a P1-A runtime owner, while prior progress, progression delta, reward eligibility/payload, granted receipt, and retry/reset transaction state still have only separate review boundaries | Verified fact foundation; missing later transaction implementation | submitted P1-A fact payload plus P1-D/P2-C review contracts; Blue Archive reward buckets; Reverse: 1999 prior-state-before-update client flow; Limbus battle-stage/progression-node separation; NIKKE/EpinelPS duplicate-grant failures; Stella Sora emulator claim-before-inventory ordering risk; Neural Cloud owner separation | Without the later transaction owners, progress-first mutation, identity conflation, or claim-before-grant persistence can still misclassify, double-award, or permanently lose first-clear/mastery rewards |
| Broad daily/liveops/economy surfaces are absent | Intentionally deferred | PGR/HI3 table inventories | Low impact until the combat-stage-result loop is worth repeating |

## Decision Scoring

This is a working comparison tool, not an ROI claim. Each factor uses 1 (low) to 5 (high):

- `Impact`: value to demo clarity, replayability, or content production.
- `Identity`: reinforcement of fixed-rear, forward-risk, summon-answer combat.
- `Evidence`: strength and cross-game consistency of the source evidence plus current-code fit.
- `Cost`: implementation, content-authoring, verification, and QA size.
- `Dependency`: number and depth of prerequisites.
- `Regression`: risk to the current playable route, input ownership, presentation, or persistence.

Working score:

`2 * Impact + 2 * Identity + Evidence - Cost - Dependency - Regression`

Hard gates override score: P0 stabilization, a stable ID boundary, and required predecessors always come first. Scores should be revised after each vertical slice rather than used to justify a large batch. A role name in an owner column is a contract boundary, not staffing evidence; each slice must name its accountable person or team before admission.

The accepted P0 full/natural/Retry/Lobby suite is no longer a scored candidate. It is a continuous hard gate: any wave that breaks it stops promotion and first repairs its own regression. The first D4b aggregate triggered that rule; the repaired and route-frozen graphics-enabled aggregates restored the gate at 37/37, so the rule remains active rather than an open defect.

| Candidate | Impact | Identity | Evidence | Cost | Dependency | Regression | Score | Readiness consequence |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| P1-0 minimal route shell, Station definition, typed actions, and route validator | 5 | 4 | 5 | 3 | 1 | 3 | 16 | **Complete:** validators, final route/policy digests, parallel D4b lane, and route-frozen 37/37 regression all pass |
| D4b mutation inventory, bypass-zero proof, and technical freeze | 5 | 3 | 4 | 4 | 2 | 5 | 9 | **Complete hard gate:** scoped 14/14, exact inventory `8/4/1`, authorized core `1`, bypass `0`, validator/digests, and route-frozen aggregate pass |
| P1-A cross-scene run/result, shared outcome shell, and typed action executor | 5 | 5 | 5 | 5 | 4 | 5 | 11 | **P1-A current-schema accepted/closed:** separate 45/49/54/59/68/75 cutoffs retain bounded meaning and final 79/79 closes the full current-schema exit |
| P1-B static reference spine, briefing, catalog, cinematic, result/progression, and Station Add joins | 5 | 4 | 5 | 4 | 3 | 3 | 13 | **ACCEPTED / VERIFIED COMPLETE:** all required product rows pass; foreign disposition passes only by explicit rejection. Historical failed candidates remain immutable; no P1-C runtime owner is implied |
| P1-C one count-1 `EncounterGroup` route executor | 5 | 4 | 5 | 4 | 4 | 4 | 11 | Requires canonical references, one real Add fixture, and the scene lease/quiescence gates |
| P1-D pure mastery evaluator and result finalization | 4 | 5 | 5 | 3 | 4 | 3 | 13 | Prove immutable-fact semantics before adding a store |
| P1-D durable intent, atomic state store, and canonical projection | 5 | 4 | 5 | 5 | 5 | 5 | 8 | Separate crash/duplicate/corruption gate; evaluator PASS does not imply durability |
| P1-E Move presentation extraction | 4 | 4 | 5 | 3 | 4 | 4 | 10 | Preserve the current evaluator/order while moving only presentation ownership |
| P1-E immutable attempt and typed Move evaluator | 4 | 4 | 4 | 4 | 5 | 5 | 6 | Shadow parity before the single advancement owner changes |
| P1-E Fire gameplay-reset ownership | 5 | 4 | 5 | 5 | 5 | 5 | 8 | Actual reset-completion gate: projectile, target, input, blocker, and source-owned state restoration |
| P2-A one `StageRuleSet` and modifier | 4 | 5 | 5 | 3 | 4 | 4 | 12 | P2 despite score because route identity and exact cleanup precede variability |
| P2-A one enemy reused through Story/Practice/Challenge variants | 4 | 4 | 5 | 4 | 4 | 4 | 9 | One prefab and one AI system; only configuration varies |
| P2-B runtime presentation lifecycle adapter | 4 | 4 | 5 | 4 | 5 | 5 | 7 | Separate from P1-B's static cinematic join; must close complete/skip/cancel/restart/unload parity |
| P2-B three-entry summon lesson course chain | 4 | 5 | 5 | 5 | 5 | 5 | 8 | After P1-E/P2-A and lifecycle parity; Free Practice is the hardest missing owner |
| P2-C first/repeat reward plan and one growth sink | 3 | 2 | 5 | 4 | 5 | 3 | 3 | After repeat play and durable progression; no broad inventory/economy |
| Historical combined broad stamina/shop/liveops/activity shell, not the bounded repeat-content slice | 2 | 1 | 5 | 5 | 5 | 4 | -3 | Conservative P3 hold score retained only for the unsplit broad shell; rescore a bounded repeat-content slice when it is admitted |

The P1-B total `13` is retained as of `SNAP-P1B-ANCHOR-PROFILE-HYGIENE-03` for the **full P1-B scope**. The second and third cutoffs closed three static hygiene subgates but supplied no measured implementation-cost or regression evidence that changes an axis. `evidenceStrength = 5` describes the strength of the admitted local structural/runtime evidence used for the product decision; it does not mean P1-B full exit, live PGR/HI3 admission, or the foreign evidence packet is complete. The joint P2-A5/P2-B6 active-restart gate remains intentionally unscored until impact, identity, evidence, cost, dependency, and regression can all be measured; a cost-only `derived-high-risk` marker is not a product score. Likewise, the historical `-3` row above is not inherited by the now-separated bounded P3 repeat-content slice or by release operations.

### Current execution sequence

1. **P0 — accepted baseline complete and continuously gating:** the 28/28 artifacts prove the uninterrupted full route through Retry/fresh Corridor, the unforced natural intro, one retained product surface, and actual Retry/Lobby navigation. Every later wave must keep these lanes green.
2. **P1-0A route shell and P1-0B D4b — complete joint exit:** the minimal `PlayableStageDefinition`, Station definition, typed actions, exact mutation inventory, bypass-zero validator, frozen route/policy digests, and route-frozen GPU aggregate 37/37 all pass.
3. **P1-A — accepted predecessor:** separate 45/49/54/59/68/75 cutoffs retain their bounded meanings, and the unchanged-source 79/79 audit closes the current-schema exit. Preserve this regression set while P1-B is active.
4. **P1-B static spine — active verified partial:** preserve the accepted direct Corridor-entry join, one owned port, 39-row current-binding surface, Corridor 4/4 and Station 0/0 anchor ownership, exact remaining-profile context, Candidate-05's fail-closed catalog projection, the proposal/rev2 AMEND history, the truthful-join cutoff, and Remediation3 result/progression cutoff. Next close the Station count-1 Add authoring gate, live PGR/HI3 foreign evidence, and a fresh P1-B full-exit audit. Runtime presentation lifecycle remains P2-B.
5. **Pre-P1-C measurement subgate, then P1-C:** pass `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN` before the first comparable P1-C mutation, start the prospective first-pocket ledger at that mutation, then execute the approved count-1 Add pocket through a minimal ordered `EncounterGroup` and prove fault/cancel/retry/unload cleanup plus sole scene activation ownership.
6. **P1-D evaluator then durability:** close pure typed mastery/result finalization first; only then add the durable intent, atomic store, recovery, and canonical stage-select projection as a separate gate.
7. **P1-E presentation then attempt then reset:** extract Move presentation, shadow/promote immutable Move evaluation, and use Fire as the first non-no-op gameplay-reset ownership fixture. Generalize only one lesson at a time after parity.
8. **P2-A0-A4:** freeze the variability snapshot and recommendation first, then one source-scoped restriction, one typed modifier, and one enemy with Story/Practice/Challenge variants. Admit each tuning revision through `ACC-P2A-TUNING-CADENCE-ONE-VARIABLE`; author the nested restart policy here, but defer whole-course restart integration.
9. **P2-B0-B5:** close runtime presentation lifecycle parity first, then build the strict-linear `BasicLesson -> FreePractice -> SummonMasteryChallenge` foundations. Static cinematic reference validation does not satisfy this lane.
10. **Joint P2-A5/P2-B6:** integrate active-run restart only after every P1-E/course/P1-C/P2-A/presentation barrier exists, and pass one evidence-complete full-course lifecycle.
11. **P2-C:** add first-clear/repeat-clear distinction and one growth action only after the exact `ACC-P2C-REPLAY-DEMAND` predicate, durable progression migration/recovery and backup/rollback gates, and `ACC-P2C-STORE-MIGRATION-DISPOSITION` all pass.
12. **`OPS-STAGE-THROUGHPUT` second-stage proof:** after the accepted first-pocket ledger and P2-B presentation lifecycle, freeze exactly one expansion-candidate contract, deliver that distinct short logical stage, close its same-schema ledger/content-truth rows, and judge the matched throughput comparison without creating a P3 content family.
13. **P3 demand-gated repeat content:** only after its separate measured-demand predicate and accepted second-stage throughput predecessor, freeze and rescore one bounded repeat-content family. It cannot inherit the expansion-candidate rank, score, evidence, or delivery result; release operations remain a later operations-track gate.

Every incomplete promotion from P1-B onward must carry both `CONTINUOUS-P0-NONREGRESSION` and `CONTINUOUS-CURRENT-BUILD-PERF`. The former requires fresh full/natural/Retry/Lobby/build-route/relevant-validator evidence from the promoted candidate. The latter requires `ACC-OPS-BUILD-MANIFEST-CURRENT` against the exact current source/config/package revision for every promoted wave; `ACC-OPS-ANDROID-PERF-CURRENT` is a separate named-device verdict required before device/release promotion and is never inferred from an Editor run, a development APK, or the build manifest alone.

### Next bounded delivery waves

This table intentionally shows the nearest eight bounded waves. The authoritative sequence above continues afterward through the remaining P2-B course entries, joint P2-A5/P2-B6 restart, P2-C settlement, the second-stage authoring proof, and P3; omission here is not deferral or reordering.

| Wave | Category | Player/production effect | Cost and principal risk | Required predecessors | Exit evidence |
|---|---|---|---|---|---|
| 1. P1-0 dual lane — complete | Operations + content | removes route-identity drift and resolves D4b uncertainty | high; integration exposed and repaired a three-lane P0 regression, and headless graphics failure required an environment-correct rerun | accepted P0 baseline | **PASS:** route/Station validator, complete inventory/bypass `0`, frozen revision/digests, scoped/full-route/buttons, and GPU aggregate 37/37 |
| 2. P1-A run/result — accepted | Function + presentation + operations | shows summon-answer performance from authoritative run facts and unifies Clear/Fail recovery | current-schema risk closed; preserve regression | Wave 1 joint exit complete | final 79/79 unchanged-source audit closes the current-schema exit; future admitted-owner rows remain later-wave work |
| 3. P1-B static spine — verified partial | Content + presentation | unifies stage select, briefing, route, cinematic, and result/progression references under one identity | medium-high; duplicated assets, anchors, profiles, strings, or stale cached selection can become new owners | Wave 2 plus both continuous gates | preserve all accepted cutoffs and historical AMEND/FAIL records; next close Station Add, live PGR/HI3 foreign evidence, and the full-exit audit |
| 4. P1-C encounter bridge | Content + function + operations | turns one authored pocket into real spawns without bespoke scene sequencing | high; false clear, partial-spawn leak, stale callbacks | Wave 3 plus `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN=pass` before the first comparable mutation; start the prospective first-pocket ledger at that mutation | isolated Add reaches the canonical pocket; spawn failure faults; gate opens once; cancel/retry/unload leaves no late spawn or owned leak |
| 5. P1-D evaluator to durability | Function + operations | adds summon-specific mastery and persistent clear state | very high; crash, corruption, duplicate apply | Wave 4 | pure objective boundary PASS, then separately Prepared intent, atomic generation, crash/restart/race/duplicate recovery and stage-select projection PASS |
| 6. P1-E presentation to reset | Function + presentation + operations | makes the working tutorial safely reusable | very high; input soft-lock and projectile/target contamination | Wave 5 | Move visual/input parity; typed-attempt shadow parity; Fire exact projectile/target/source-state restore; cancel/disable/unload plus P0 aggregate PASS |
| 7. P2-A0-A4 variability | Content + function + operations | reuses the same enemy and combat identity across Story, Practice, and Challenge | high; restriction leaks, modifier rollback, accidental second AI system | Wave 6 and P1-C fixture | recommendation is nonmutating; nondefault restriction state restores; modifier apply/remove passes; same prefab has three profiles and inactive configuration receipts |
| 8. P2-B0-B1 lifecycle | Presentation + operations | makes intro/story handoffs safe across complete, skip, cancel, restart, and unload | high; camera, HUD, input, listener, or late-async leaks | Wave 7 route facts | `intro-to-stage` restores captured nondefault state for every terminal reason; stale generations cannot reacquire; restore faults are evidenced; no new camera framework |

## Detailed Backlog

The `Current execution sequence` above is the authoritative order. The sections below are grouped by related contract rather than execution order; every heading carries its exact phase tag so document position must not be interpreted as priority.

### P0 — Protect the current demo

1. Keep the now-passing canonical tutorial-to-combat-to-stage-clear path under regression.
2. Lock a named demo-build manifest and performance report before promoting a production wave: executable/package hash, Unity revision, target device/profile, scene route, warm-up rule, frame-time percentile, peak memory, load/handoff bounds, and allowed regression thresholds. Until that artifact exists and is linked here, P0 is a functional baseline only, not a claimed build/performance baseline.
3. Preserve the active optimization session's scope; P1-0 and P1-A current-schema exit are complete. P1-B is now the active production slice, while the P1-A regression set remains mandatory.
4. The end-to-end route gate is satisfied, but progression, economy, and broad tutorial-framework work remain excluded until their ordered phases.
5. Verify the current split route across normal completion, intro skip, Corridor tutorial completion, Corridor unload, Station entry-guide gates, Station combat, retry, clear-UI additive load, and scene unload.
6. At every handoff, assert the expected scene-local input/movement/camera owner, HUD visibility and interactivity, time scale, BGM, phase, and bounded time to the next state.
7. Retain the old 11:10 probe as historical only. Current P1-A acceptance uses the renamed `CanonicalFullRouteCompletesTutorialStationGuideVictoryAndReplay`, which reaches the committed additive result, invokes the typed Replay listener, and verifies a fresh Corridor run in the same execution.
8. Use the current `OlympusCorridorActualPlayPathTests` PASS as the unforced natural intro-handoff lane; the old 10:38 report remains historical.
9. Keep exactly one current product terminal-result owner. P1-A2 proves typed endpoints, bounded fail-closed UI/I-O/presentation, current-schema digest/schema/callback/authority/nonterminal/destination/dispatch/loader checks, typed Station adapter-loss safety, and the historical 75/75 phase/subject/unload matrix; the final unchanged-source 79/79 cutoff additionally closes exact duplicate caller identity, direct-replacement coordinator provenance, exact diagnostic provenance, and snapshot/evidence exception handling without another surface or executor. Preserve that full current-schema boundary during P1-B.

Implementation boundary:

- Reuse the current observable flow and presentation state in editor/PlayMode verification.
- Do not add a second camera or transition framework for this gate.
- Snowbreak-style screen recognition is not needed; inspect authoritative component state directly.

Exit condition:

- Two fresh deterministic scenarios from the same known-good baseline: one runs Corridor tutorial -> Station guide -> boss clear -> the current additive clear-result surface -> Retry -> a fresh Corridor run; the other independently reaches that same current surface and executes Lobby -> `UI_Lobby`. Executing either navigation ends that scenario; P0 does not claim a future committed summary, outcome-filtered action set, or typed selection latch.
- The Station player and guide are owned by the Station scene, not leaked from the unloaded Corridor scene.
- Exactly one product terminal-result owner resolves Retry/Lobby; review-only overlays cannot present a conflicting result or route.
- No stuck input ownership, leaked movement/joystick lock, unintended tutorial trigger, duplicate clear owner, stale BGM/HUD, or missing scene handoff.

The accepted full-route, natural-handoff, terminal-button, and 28/28 aggregate artifacts satisfy this P0 exit condition. The frozen D4b/route state independently preserves it with full route 1/1, Retry/Lobby 2/2, validator/inventory bypass `0`, and route-frozen GPU aggregate 37/37. The full-route lane uses the production skip handoff and ends in actual Retry/fresh Corridor; the independent natural lane proves the unforced intro, and the independent terminal-button lane proves Lobby.

### P1-0 / P1-B / P1-C — Shared identity, stage spine, and encounter bridge

The frozen P1-0 route contract and the **accepted / verified-complete** P1-B reference-spine contract are maintained in [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md). The P1-C execution/lifecycle boundary is maintained in [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md). Both are thin layers over current authorities, not replacement stage databases. Historical P1-B failures remain immutable; Station Add Remediation3 and the full-exit audit pass, while foreign evidence is rejected from promotion. P1-C runtime remains absent pending the prospective ledger freeze.

#### P1-0 — Shared identity preflight

Decision readiness before either P1-A or P1-B production code:

| Concern | Review value | Readiness |
|---|---|---|
| physical route and result presentation | route segments Corridor -> Station; separate additive clear UI after commit | current uninterrupted PlayMode evidence passes through Station guide/combat, authored victory, result, actual Retry, and fresh Corridor |
| logical product ID | `OLYMPUS-INVASION-01` | implementation-frozen on the validated route asset |
| route revision | `1` | route digest `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9` |
| segment IDs | `corridor_intro_tutorial`, `station_entry_combat` | frozen with their exact typed conditions, handoffs, and physical-definition refs |
| Corridor route semantics | entry `run.entry.admitted`; exit `corridor.tutorial.completed`; handoff `SingleLoad` | approved P1-0 rev1 authoring value; completion means sealed current-run pre-load handoff, then exact-next-segment single load |
| Station route semantics | entry `corridor.tutorial.completed`; exit `station.encounter.terminal`; handoff `ReturnToOwner` | approved P1-0 rev1 authoring value; terminal means current-run D4b `TerminalClosed`; final segment performs no load/unload and returns control to the run owner |
| physical segment refs | existing Corridor profile plus frozen `OLYMPUS-STATION-COMBAT-01`, valid `MapScenePath`, and Station scene binding | P1-A1 verifies handoff/direct-Station rejection; P1-A2 dispatches Replay/Retry back through the frozen entry snapshot. P1-B may enrich non-route content only |
| failed-run retry action | `olympus-invasion.retry`, target the same logical stage, allowed for Fail | product-approved D2 recovery action; P1-A2 Fail Retry actual click reaches a fresh Corridor run |
| clear replay action | `olympus-invasion.replay`, target the same logical stage, allowed for Clear | product-approved D2 action that separates manual post-clear replay from failed-run retry and later repeat/economy policy |
| lobby action | `olympus-invasion.to-lobby`, target `UIRouteId.Lobby` | product-approved D2 target and actual current Lobby execution PASS; no real next stage exists |
| outcome/action availability | approved `Clear -> Replay + Lobby`, `Fail -> Retry + Lobby`; defer Stage Select/Next | [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md) records approval; action presence still never makes a button legal without the matching committed outcome |
| D4a product terminal semantics | authoritative causal root order; same-root same-epoch boss death plus player down resolves Clear with player-down retained; a lower independent player-only terminal result is not reopened; no draw/callback/frame-window policy | product-approved independently from the technically frozen coordinator mechanism |
| D4b technical terminal mechanism | frozen `SameTerminalResolutionEpoch`, pre-mutation `CanonicalCombatRootAdmission`/`RootAdmissionSequence`, `RootResolutionToken`, exclusive synchronous `{ Player, Boss }` mutation queue, two-subject `QueueDrainedAndSubjectsFinalized` handshake, lifecycle/token/fault/cancel rules | policy revision `1`, digest `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`; complete inventory `8/4/1`, authorized core `1`, bypass `0`, validator and GPU aggregate PASS |

P1-0 has frozen the single `PlayableStageDefinition` asset that P1-B completes, not a parallel identity record. It contains the logical ID/revision, exact two `StageSceneSegmentRef` rows, validated Corridor and Station definitions/bindings, typed Replay/Retry/Lobby actions, allowed outcomes, and the D4a/D4b fields under the exact route/policy digests above. The bounded/scoped D4b tests, canonical route, buttons, complete Station mutation inventory with bypass zero, semantic/scene/build validator, and route-frozen GPU aggregate pass. At entry P1-A deep-snapshots only that frozen contract. P1-A1 supplies Corridor-to-Station through `StageRunSingleLoadDispatch.DestinationScenePath`/`DestinationSceneName`, and P1-A2 derives Replay/Retry from the immutable entry-segment snapshot; neither dispatch re-reads the latest asset after context disposal or treats a forward scene constant as runtime authority. P1-B now joins the canonical catalog, reference block, truthful template/briefing, cinematic identities, and Remediation3 admission-time result/progression presentation on that same asset. P1-B may enrich non-route Station anchors/spawns/ports, but cannot defer or change P1-0's physical scene identity/binding or reopen destination ownership. New P1-B fields apply only to new-schema runs; existing active/committed P1-A snapshots remain immutable and unresolved for those later fields.

#### P1-B — Canonical playable-stage references

The local P1-B evidence remains a sequence of immutable, non-interchangeable cutoffs. The final named cutoff is Station Add Remediation3: source `128/128`, artifacts `11/11`, validator PASS, focused `8/8`, UI `34/34`, exact full route `1/1`, and graphics aggregate `99/99`, with frozen digests preserved. The foreign packet is explicitly rejected from promotion. No local artifact admits P1-C execution, reward ownership, or pre-result restart; P1-B full exit is closed.

1. Complete the same P1-0 `PlayableStageDefinition` route shell as one canonical playable-stage contract that references, rather than duplicates:
   - one or more StageDefinitionProfile assets for scene/map/runtime anchors;
   - an ordered scene-segment route for Corridor, Station, and their explicit handoff policy;
   - a truthful LinearStageTemplateProfile for lesson, route, segment, and encounter intent; none of the current five Break/Backline/Tank/Heal/composite templates matches the current player-action tutorial plus Station summon/boss route, so do not force a convenient join;
   - progression identity with typed `RequiredStageState = Cleared(nodeId) | MasteryObjectiveAchieved(nodeId, objectiveId)` and an explicit next progression node; later availability/clear/mastery views derive from P1-D state rather than living on this authored node.
   - result definition for tracked proof and next action.
2. Validate the ordered route already defined by the P1-0 shell and prepare the forward-loader migration without re-authoring it; runtime spawning waits for P1-C.
3. Bind the corrected canonical stage-select entry to the contract and derived briefing instead of copied display strings. No chapter-map scene/prefab instance exists in the audited workspace; defer its binding until a real node can project typed progression prerequisites and P1-D state.
4. Keep reward payout and liveops out of this milestone.
5. Replace stage-select's selected-row scene-name/path handoff with resolution through the canonical playable stage, preserving the now-working selection behavior while rejecting unintended duplicate-row aliases.
6. Use a direct `CinematicSequenceProfile` asset reference as the cinematic identity. Treat string profile IDs as migration diagnostics, not the canonical join key.

Expose one derived stage-briefing read model for stage select, pre-run briefing, loading/handoff, and result. It should resolve title, objective, combat lesson, featured threat/summon need, recommended power, target time, restrictions, preview cue, retry policy, and story entry/exit references from canonical IDs instead of copying those strings into each UI.

For the current split route, P1-0 requires the exact revision-1 rows above. A matching condition string never grants authority by itself: Corridor exit must carry the sealed current-run handoff, and Station entry must validate the active route snapshot/current segment/token. `ReturnToOwner` is valid only on the final Station segment and retains the Station host while the run owner finalizes/commits; it does not return to Corridor, unload Station, or load a result scene. The clear UI remains a separate additive presentation dependency, not a combat segment. P1-A1 already resolves the Station load from the sealed snapshot dispatch. P1-B adds the remaining joins and validators while preserving that owner; validator/build-readiness Station aliases may remain fail-closed cross-checks, but cannot become a duplicate runtime load authority.

The frozen P1-0 validator covers the exact route rows, actions, physical definitions and scene bindings, Build Settings order, Station coordinator/mutation inventory, and both digests. Candidate-05 and the presentation/truthful/result-progression cutoffs remain immutable. Station Add Remediation3 adds the complete 128-source envelope, binding-transform-relative pose, Station MapRoot topology containment, canonical Corridor-to-Station route, and intermediate-parent drift negatives. The next validator expansion belongs to the ledger-gated P1-C runtime slice.

#### P1-C — Ordered encounter execution bridge

Minimum route execution bridge:

`PlayableStageDefinition.encounterExecutions[] -> LinearStageSegment / LinearStagePocket -> EncounterGroupSequence -> ordered SpawnRef IDs -> StageDefinitionProfile.SpawnRef -> AnchorRef`

The first `EncounterGroup` needs only one fully identified activation command, ordered spawn references, typed Add-defeat completion, cancellation, owned cleanup, and next-local-group handoff. Cross-game evidence supports richer action vocabulary, but the local `SpawnRef` already owns payload, anchor, count, and delay and has no interval. Revision 1 therefore snapshots the static plan at P1-A run admission, uses an injected scaled gameplay clock, treats each `delaySeconds` as absolute from group activation, preserves serialized spawn-reference order for equal due times, requires the first fixture to be `Add` with `count == 1`, and defers per-unit interval/member overrides. Group and sequence states are separate: intermediate group completion leaves the sequence active, while only the final group can complete the sequence and satisfy one named local gate. The bridge must not duplicate transforms, become a general graph editor, or absorb progression and result logic.

No current P1-C encounter fixture is freeze-ready: the current templates do not truthfully match the Corridor/Station route, Corridor Add payloads are placeholders, the boss is cutscene-owned, and the frozen Station definition/binding has no Add spawn set. P1-B must freeze one exact segment/pocket and author one real Station Add spawn, unique anchor, and stable non-placeholder payload identity tied to a concrete archetype/prefab authoring target without enabling runtime execution. P1-C then owns the typed resolver/factory.

The existing `PveStageData` / `PveEncounterDirector` path is a noncanonical prototype, not evidence that this bridge already exists. It owns duplicate raw placements, sorts by `triggerZ`, has no explicit retry/scene-exit cancellation or owned cleanup API, and currently allows a failed delayed spawn attempt to leave an empty group that clears. P1-C must isolate that owner from the canonical route and may reuse only separately tested lifecycle primitives.

P1-C is complete only when one immutable `EncounterGroupSequence` deterministically resolves its ordered group/spawn references, faults rather than clears on a required spawn failure or unexpected disappearance, proves activation and typed defeat completion, invalidates stale execution generations, cancels pending actions on every terminal dispatch/scene exit, cleans every owned full or partial runtime object/subscription, advances exactly once, satisfies its named local gate, and releases the one atomic scene execution lease before navigation. Detailed validators, lifecycle, acceptance fixtures, and ordered sub-slices are defined in [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md). A generic condition language or event graph is explicitly outside the first slice.

Why before tutorial generalization:

- It converts existing authored work into reusable playable content.
- It reduces scene-specific wiring before the tutorial system is generalized.

### P1-A / P1-D — Add run proof first, then mastery

#### P1-A — Stage-wide facts and truthful result

The implementation contract and acceptance matrix are maintained in [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md). P1-0 freezes route/D4b with inventory/bypass-zero, validator, digests, and 37/37. P1-A retains historical 45/49/54/59/68/75 cutoffs and closes the current-schema exit at the final manifest-bound 79/79 cutoff with both frozen digests unchanged.

1. Define RunResultSummary around signals already produced by combat:
   - clear time,
   - player damage taken/down state,
   - perfect dodges,
   - summon calls,
   - correct summon answers,
   - structure breaks,
   - forward-pressure time,
   - final boss completion.
2. Bind the stage-clear shell only to a committed immutable summary and resolved typed actions, never directly to raw encounter callbacks. Record `masteryEvaluationState = NotEvaluated` with no mastery-result rows; defer aggregate rank and all objective evaluation.

#### Runtime signal coverage audit

The current project seals the narrow revision-1 Olympus result payload. The 49/54/59/68/75 slices retain their bounded finalization, I/O, presentation, adversarial, and remediation meanings. The final 79/79 cutoff adds exact duplicate identity, truthful direct replacement, exact diagnostic provenance, and typed snapshot/evidence exception closure and closes the current-schema exit. This is not a scoring or reward system.

| Result fact | Current source | Coverage | Required bridge |
|---|---|---|---|
| Stage identity and run ID across scenes | Frozen route definition plus P1-A1 `StageRunRouteSnapshot`/`StageRunContext`; Corridor loads the dispatch destination from that snapshot | Route/handoff owner and fact payload are implemented; the 45/45 fact cutoff and later 49/49 finalization snapshot remain distinct evidence | Extend the same context only through diagnostic/admitted-owner closure; never carry scene-object references across the single-load transition |
| Canonical clear/fail | legacy encounters collapse the first `Died` into `CombatEncounterController.Won/Failed`; Station opts into `EncounterTerminalResolutionCoordinator` and exposes the exact terminal record | P1-A2 receives the exact Station record once, seals terminal epoch closure/current-schema owner coverage and the fact payload, and opens the shell only after schema-2 atomic-file reconciliation; 68/75 retain bounded coverage and 79/79 closes the exit | Preserve the accepted current-schema boundary. Future admitted-owner closure remains later-schema proof |
| Summon-follow-up, pressure suppression, counter recovery proof | `BossBarrageEncounterController` proof events plus optional `RouteResultRecord` | Current collector has source-qualified pressure/follow-up/counter adapters; accepted fact test directly proves pressure, while broader path coverage remains bounded | Preserve only actually observed semantic proof IDs/source kinds; do not require or invent a route record when the canonical boss-death clear uses a different boundary |
| Player damage taken | `CombatHealth.Damaged` with resolved `DamageInfo.Amount` | Current exactly-one Station collector accumulates resolved hostile damage; accepted fact test covers it | Keep collection scoped to the active run's player health and full lifecycle cleanup |
| Player down | `CombatHealth.Died` plus terminal subject snapshot | Current fact payload records down count and derives survival proof only when count remains zero; finalization evidence binds fixed Player/Boss snapshots and candidate/final agreement; 68/68 proves fixed-root callback-order full-summary digest invariance; 75/75 proves rebind/disabled faults; 79/79 proves throwing snapshot/evidence construction enters one typed fault/abort without result or UI | Preserve the accepted row; never use the callback as terminal authority |
| Perfect dodges | Both canonical scenes contain `PlayerActionController`; neither contains legacy `PlayerController` | Current Station collector uses `PlayerActionController.PerfectDodgeTriggered`; accepted fact test covers one dodge | Retain this sole canonical adapter and the legacy event only for noncanonical legacy scenes |
| Summon usage and spent tier | `PlayerSummonSlot1Action.SummonSlot1Used`; authored support-slot `SummonUsed` | Current collector records monotonic admission sequence, slot/role ID, spent tier, and segment-relative milliseconds | Preserve stable role IDs and close remaining exit/unload lifecycle coverage without coupling the summary to concrete slot classes |
| Correct summon answer | Boss encounter block/follow-up/counter events; optional `RouteResultRecord` only when it actually commits | Current summary carries source-qualified semantic proofs; accepted fact test directly proves pressure plus survival, not every proof source | Never infer correctness from summon use; add broader direct proof-path coverage as needed without making the optional route record outcome authority |
| Structure break | Legacy `BattleStructure.OnStructureDestroyed`; elite break state is currently queried from `EnemyElitePatternController` | Fragmented | Add an objective/proof adapter before using this as cross-stage mastery; do not equate boss pressure suppression with a literal structure break |
| Forward-pressure time | `SummonEnergyLadder.CurrentForwardRisk01`, `CurrentRiskBand`, and `RiskBandChanged` | Optional route-owned integer-millisecond duration is implemented under the Released + Running combat gate; direct dedicated proof-path assertion remains narrower than the wiring | Render only when present and retain remaining lifecycle validation; do not reinterpret absent duration as missing collector success |
| Tutorial completion | Corridor `OlympusCorridorTutorialDirector.Completed` | Before SingleLoad, current P1-A seals `olympus.corridor.core-tutorial` revision 1 and seven ordered `LegacyOpaque/NoResultExpected` coverage rows | Follow [Tutorial Lesson, Attempt, and Gameplay Reset Spec](TUTORIAL_LESSON_ATTEMPT_RESET_SPEC.md) for later typed per-lesson attempts/reset; retain the current director until parity tests pass |
| Persistent clear/mastery | Prototype UI booleans only | Missing | Persist only after canonical stage ID and result schema are stable |

Existing precedent:

- The legacy `BattleResultUI` already aggregates just dodges, skill casts, structure breaks, and base damage into display text.
- That code proves the event sources are usable, but its counters are UI-owned, scene-specific, not a reusable result object, and not persisted.
- The current canonical route completes the Corridor tutorial, releases movement/joystick ownership, single-loads Station, gates a two-step entry guide, fights the Station boss, then additively loads the retained clear UI. The legacy Review result owner is retired; the combat-session overlay retains only its in-combat pause/settings/failure role.
- Current producers still call `CombatHealth.TryApplyDamage`, but frozen D4b Station binding delegates that call through the coordinator; bound reset/reconfigure is rejected, while unbound Corridor tutorial targets retain their required legacy reset behavior. `DamageInfo` still carries no public admission/root/epoch authority and `CombatHealth.Died` remains synchronous inside an authorized mutation. P1-A2 consumes the exact coordinated record, validates immutable epoch evidence, seals current-schema finalization coverage, and durably decides the fact payload. Historical 68/75 cutoffs retain bounded evidence; final 79/79 closes duplicate identity, direct replacement, diagnostic provenance, and snapshot-exception closure. Future admitted-owner aggregate closure is later-schema proof.
- The old `OlympusCorridorCombatFlowPlayModeProbe` remains historical. Current evidence supersedes it with an uninterrupted runtime-input/real-joystick route through Station guide, authored victory, additive result, actual Retry, and fresh Corridor; separate current tests cover unforced intro, actual Lobby, and one terminal owner.
- Station keeps `BossBarrageLaneReview_PocketOwner` active, but its current `closeThreatHealth` reference is null. Canonical clear instead comes from the coordinated terminal record; `OlympusStationCombatResultPresenter` forwards that exact record to the route/run owner, and only its committed summary may open the shared overlay.
- `RouteResultRecord` is therefore a useful optional encounter-proof adapter, not the canonical stage outcome and not a substitute for either the verified current fact bundle or the still-open complete finalization lifecycle.
- This separation is correct: `clear condition` and optional `mastery proof` must remain distinct, and opening the result overlay must never manufacture summon-answer success.
- The current `StageClearScreenPresenter` is now kept as a view: it receives the fact-bearing committed summary and invokes the typed executor, without becoming a statistics, outcome, or navigation owner. A later validated presentation profile must render selected facts through this seam rather than move collection/evaluation into UI.

#### Minimum first result slice

Current revision-1 normalized payload and remaining contract fields:

- identity: `schemaVersion`, `runId`, canonical playable-stage ID, route revision, route snapshot digest, ordered/current segment IDs, both resolved stage-definition/scene identities, and the complete snapshotted terminal-resolution policy from the P1-0 route shell, with the optional template join explicitly unresolved until P1-B supplies it for new runs;
- outcome: the closed clear/fail arm with typed failure-reason absence/presence plus canonical integer total-stage and combat-segment elapsed milliseconds; UI derives seconds;
- survival: resolved player damage taken and player-down count;
- action proof: perfect-dodge count and normalized summon-use records;
- identity proof: semantic summon-answer proof IDs and encounter proof IDs, plus `optionalRouteProofAdapter = None | CommittedRouteResultRecord(exact normalized encounter-proof fields/digest)` when that adapter actually commits; this is never outcome authority;
- optional adapters: structure-break count and forward-risk seconds;
- mastery boundary: `masteryEvaluationState = NotEvaluated` and an empty result list in P1-A; P1-D later evaluates immutable facts, never UI strings;
- handoff: outcome-filtered offered action IDs from the one P1-0 `PlayableStageDefinition` route shell, not copied route definitions;
- diagnostic abort: a separate immutable abort record containing run/route identity, last lifecycle state, reason, and sequence; it never becomes a product `RunResultSummary` or progression/reward input.

First vertical-slice status. `Verified` names its evidence cutoff: 45/49/54/59/68/75 retain their bounded meanings, and the final 11-source cutoff proves Combat 21/21, StageRun 23/23, UI 15/15, aggregate 79/79, exact full route 1/1, validator PASS, and current-schema full exit. These overlapping suites are not summed. Earlier excluded artifacts remain excluded. Later-schema owner rows remain open:

1. **Verified:** `StageRunContext` is created at logical stage entry with a new run ID by deep-snapshotting the approved P1-0 `PlayableStageDefinition` route shell, including segment/scene identities, action and terminal-policy semantics, and canonical digest; missing or disagreeing route input fails closed.
2. **Fact/handoff and diagnostic paths verified:** fresh-Corridor replacement, wrong handoff destination, coordinator diagnostic, producer exception, adapter loss, Station unload, schema/digest guards, callback-origin rejection, authority/candidate separation, nonterminal cycling, three-phase cancel, rebind/disabled subjects, exact duplicate replay, direct replacement, exact diagnostic provenance, and snapshot/evidence exception closure create no false product result in their accepted rows. Future admitted-owner receipts are later-schema proof.
3. **Verified:** `ICombatEntryGuideGate` exposes `NotStarted / Playing / Released / Interrupted` plus `StateChanged`; combat time becomes eligible only from `Released` while the Station encounter is Running.
4. **Fact collector and typed loss path verified:** Station binds exactly one collector; active-root loss cancels authority and creates no product result. Rebind/disabled subjects, all three live cancellation phases, Finalizing unload, exact replacement, and final-snapshot exception closure are accepted for the current schema.
5. **D4b, fact seal, current-schema finalization, and full exit verified:** the coordinator supplies frozen token/epoch closure and P1-A seals schema-1 `NonCourseStationTerminal` coverage with truthful `NotAdmitted` rows. Historical 68/75 cutoffs retain bounded proof and final 79/79 closes the current-schema exit. Future admitted-owner receipts remain later-schema proof.
6. **Summary ownership, missing-summary fail-closed, and localized committed-fact presentation verified:** raw encounter ownership is removed from the canonical terminal presenter and the additive surface receives only the committed summary. Missing-summary behavior keeps actions disabled without inventing facts. Exact profile/catalog plus ko-KR/en-US key parity render committed total/combat time and a qualified proof while preserving `resultSummaryDigest`; reward placeholder remains hidden before P2-C.
7. **All four outcome-action endpoints and current dispatch/abort closure paths verified:** Replay/Retry/Lobby use one route-snapshot-backed terminal-action executor and one action compare-and-set, with no UI-owned scene string. Historical 68/75 evidence retains its bounded meaning and final 79/79 closes the pre-result abort/fault provenance rows.
8. Treat `RouteResultRecord` as an optional adapter only when it actually commits; canonical clear must not wait for its unrelated close-threat prerequisite.
9. Treat the tutorial-enabled Corridor-to-Station path as the only canonical route for this logical stage. A Corridor-only fallback cannot commit the same product outcome, and a direct Station load with no active canonical context is diagnostic-only and cannot manufacture a run.
10. Implement the approved D1-D3/D4a values and consume the separately frozen D4b revision-1 contract from [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md): an outcome-aware shared result shell, Retry plus Lobby for Fail, lower pre-mutation root-admission sequence as independent-root causal order, and Clear-wins only when both candidate/final terminals agree in the same active epoch. No policy may depend on render frames, timers, health-callback arrival, or subscriber order.
11. Keep ranks, currency payout, online submission, and broad analytics out.

`StageRunContext` is a one-run handoff object, not a permanent game manager. A clear/fail commit freezes mutable facts and detaches scene-local fact adapters; the immutable context remains through result presentation until one terminal-action selection is atomically sealed. Only successful revalidation/quiescence of the fixed admitted P1-E lesson, P2-B course, P1-C execution, P2-A variability, and P2-B presentation owner set disposes before navigation; failure preserves the committed result and sealed selection in `Presented` with dispatch blocked. Post-result Replay/Retry follows this presented-selection path. Pre-terminal abandon, failed handoff, or unexpected route exit instead invalidates old authority, enters abort closing, awaits that same admitted set, and seals one evidence-complete diagnostic abort after closure results are known. Successful closure follows `Aborted -> Disposed`; timeout/fault follows `Aborted -> ClosureFaulted`, and neither branch creates or reuses a product summary.

#### P1-D — Typed mastery and persistent clear state

Use [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md) as the review contract.

1. Define `MasteryObjectiveDefinition` as optional, `ClearOnly`, non-blocking goals over an entry-time deep snapshot and fully covered immutable P1-A fact candidate. A P1-D-capable run evaluates before final result commit; P1-A-only committed summaries remain `NotEvaluated` forever and are never backfilled.
2. Store a permanent `objectiveId`, closed condition kind, discriminated typed parameters, localization/presentation metadata, and `failDoesNotBlockClear = true`. One narrow version-controlled identity manifest retains each ID's immutable semantic digest and tombstone; kind, threshold, time metric, comparator, proof ID, or proof meaning cannot change under the same persisted objective ID.
3. Start the evaluator vocabulary with `ClearStage`, `ClearUnderTime`, `NoPlayerDown`, `PerfectDodgeCount`, and qualified semantic `UseSummonForNeed`. Never parse player-facing text, opaque external formulas, raw summon use, or a P1-C completion-gate name.
4. Record required fact capabilities and complete collector coverage so unavailable input cannot look like observed zero. Canonicalize active times to integer milliseconds once in P1-A; keep the first persistent best measure explicitly `bestTotalActiveElapsedMilliseconds`.
5. Treat hidden/visible state, localization, and display order as presentation metadata. Any structurally invalid objective makes the first-slice mastery bundle `InvalidDefinition`; Clear/count/best-time remain valid, but that run persists no mastery IDs.
6. Before acknowledging a P1-D Clear, durably prepare one self-contained `StageProgressApplicationIntent`. Then consume it through one checksummed generation/CAS that publishes the minimal canonical `StageProgressState`, a globally run-ID-unique `StageProgressApplicationRecord`, and the run-specific applied delta together. Exact duplicates return the stored record; mismatches reject without touching healthy data.
7. Prove the summary-to-writer crash boundary, duplicate application after restart, before/during/after generation publication, corrupt/ambiguous generation handling, and two distinct first-clear/first-mastery races. P1-D is not persistent-complete if an acknowledged Clear can lack recoverable intent, or record-only/state-only writes are observable.
8. Render objective results from the committed summary and `NEW`/first effects only from the committed applied delta. Bind the first durable projection to the corrected canonical stage-select entry after P1-B; no chapter-map instance currently exists, and serialized prototype `locked/cleared` fields remain nonauthoritative.
9. Do not add rewards, generic score/rank/stars, maximum combo, a formula DSL, or broad chapter/save/economy scope.
10. Treat current-schema atomicity, schema migration/recovery, and backup/rollback-read compatibility as three different claims. `ACC-P1D-PROGRESS-MIGRATION-RECOVERY` requires an exact old-to-new manifest, staged activation, interrupted-upgrade recovery, corrupt-latest fallback, and byte-identical application-record reconciliation. `ACC-P1D-PROGRESS-BACKUP-ROLLBACK-READ` separately requires backup restore plus a documented older-reader boundary that either reads the supported generation exactly or fails closed without mutating newer state. Neither gate is satisfied by ordinary crash/duplicate tests on one schema.

Why before rewards:

- The player must first understand why the run mattered.
- It validates DimensionBrawl's unique combat decisions without power creep.

### P1-E — Separate tutorial presentation and proof before generalizing

Use [Tutorial Lesson, Attempt, and Gameplay Reset Spec](TUTORIAL_LESSON_ATTEMPT_RESET_SPEC.md) as the review contract.

1. Keep `OlympusCorridorTutorialDirector` authoritative for sequence, input, current setup/evaluation, and presentation cleanup throughout P1-E. Add one captured course lease for blockers/bounds, target roots/candidates/hard lock, combat mode/forced facing, invulnerability, action/telegraph/enemy states, and every terminal path. `SoldierChallenge` remains a cue-only route prelude rather than a fabricated learner attempt.
2. Approve stable plan/lesson/presentation/attempt-contract/proof/reset IDs, entry-time semantic and presentation snapshots, per-attempt observation generation plus closure token, collector coverage, P1-A's integer-millisecond unscaled result clock, one route-wide scaled rule-window clock, and one pause-independent post-outcome closure watchdog before extraction.
3. First extract only the Move cue/confirmation/focus presentation. Resolve its control target through a stable adapter while preserving the current fallback, timing, and held-pointer behavior.
4. Next add a typed same-stack Move commit seam and one immutable result without parsing `LastCompletionRecord`. Then shadow `MoveDistance` from the exact action-window baseline and promote it only after exact-threshold, one-unit-below, time-scale/pause, frame-rate, held-pointer, duplicate, and stale-generation parity.
5. Give Move the `NoGameplayMutation` policy disposition, `PreservePlayerPosition` effect, and `ReleasedNoMutation` receipt. Forced facing remains a captured current-director course domain. Move owns no combat mutation and cannot satisfy the gameplay-reset gate.
6. Use Fire as the first non-no-op reset fixture while keeping its composite success evaluator delegated. Track only attempt-owned projectiles, capture a stable target entry snapshot, restore on retry/cancel/interrupt, and on natural success atomically return complete target-domain ownership to the current director's course lease, carrying current values only for explicitly reviewed fields into still-legacy Dodge.
7. Reserve every async handle before start/live exposure. At outcome freeze invalidate only proof observation, activate a proof-inert closure token, stop producers, and drain existing reservations under the watchdog before sealing/resetting/transferring source-owned entries. Timeout/fault creates a pending-aware `FaultSealed` diagnostic snapshot, never a successful cleanup receipt. Never globally clear buffs, projectiles, targets, summons, or loadout state; cleanup failure blocks the next lesson and Station handoff.
8. Missing director, target, collector, or binding is a system fault, never completion or observed zero. Tutorial failure/cancel/interruption is not Station stage Fail and produces no product result when it terminates the route.
9. Seal ordered scene-reference-free lesson facts and the route summary synchronously before Corridor single-load. Existing P1-A/P1-D summaries are never backfilled, and tutorial proof never automatically becomes mastery proof.
10. Migrate remaining evaluators one at a time in the order `SwapToRanged -> Melee -> Fire -> Dodge -> ClearTargets`, after each rule's semantics and attribution are fixed.
11. Defer the actual lesson chain to [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md), and continue deferring persistent TutorialProgress, rewards, broad loadout, and generic condition graphs. P2-B may take only presentation domains its adapter actually acquired and passed through terminal parity.

Do not add PGR's signal-orb mechanics, level/exam gates, exact star thresholds, promotional teaching activities, robot IDs, or content scale. Reuse only the organization and validation discipline.

### P2-A / P2-B — Expand stage variability and connect presentation handoffs

The reusable complete/skip/cancel/disable/unload/retry ownership contract is maintained in [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md). It reuses the current profile/runner and remains P2-B; route-specific cleanup assertions stay active during P0/P1.

#### P2-A — Stage rules, modifiers, and enemy variants

The authoritative P2-A ownership, snapshot, cleanup, evidence, fixture, and promotion contract is [Stage Rule, Modifier, and Enemy Variant Spec](STAGE_RULE_MODIFIER_ENEMY_VARIANT_SPEC.md). It is contract-ready for review but not fixture-freeze-ready: the canonical route, P1-C count-one Station `Add`, source-scoped restriction port, modifier port, and exact enemy triad remain predecessor facts.

A 2026-07-15 read-only current-worktree audit sharpens the implementation order without freezing a fixture. The Station stage definition still authors no spawn rows while the scene directly serializes its boss controller and action deck; the logical playable-stage schema still has no admitted rule/modifier/variant/seed references; and the existing boss pressure director already varies deterministic responses from player-risk and summon context. Therefore broad RNG, affix pools, difficulty scaling, and multi-modifier stacking have low immediate value and remain deferred. First establish the P1-C count-one Station host and sole payload/lifetime authority; then freeze one immutable `StageVariabilityPlanSnapshot`; then prove one source-scoped Station modifier over a currently consumed counter-pressure/window domain if a narrow capture/restore port is actually feasible; then configure the same admitted enemy identity/prefab for Story, Practice, and Challenge. Optional replay objectives such as no-player-down or qualified summon follow-up belong to P1-D's typed evaluator over existing immutable facts, not to the modifier layer and not to rewards.

1. Add one versioned `StageRuleSet` whose entries distinguish `RecommendationOnly` from `EnforcedRestriction`. Recommendation has no gameplay mutation or cleanup claim; enforcement requires an exact source-scoped port, captured prior state, and exhaustive release receipts.
2. Resolve the rule set, zero-or-one modifier ref, optional versioned `StageEnemyVariantBindingSet`, and authored restart definition into one immutable `StageVariabilityPlanSnapshot` at logical route entry. Encode modifier absence as an empty array and binding-set absence as typed `None`, store the sole `ResolvedActiveRunRestartPolicy` there, include its semantic digest in new-schema route/result provenance, and never reread newer assets into the active run.
3. Add one `StageModifierDefinition` with display metadata separate from a closed typed payload, required executable adapter capability, apply/remove lifecycle, owned-domain ledger, stale-generation guard, and `StageVariabilityQuiescenceBarrier`. No modifier graph, stack solver, or random pool.
4. Bind one existing P1-C scoped spawn key to one `EnemyVariantProfile` through the reachable versioned binding set. P1-C's payload mapping is the sole gameplay-prefab authority. Configure the frozen role/deck/elite composition only through a typed adapter while the P1-C staging root is inactive, require a matching receipt before activation, and never copy payload, anchor, order, count, delay, or object lifetime.
5. Keep target time, recommended power, featured summon need, and combat lesson on the linear template; keep story cues on canonical cinematic references; keep post-result Replay/Retry on P1-0/P1-A typed actions. P2-A owns only rule-derived recommendation/restriction, modifier/variant identity, and pre-result restart policy.
6. Keep active-run restart, revive, and post-result Replay/Retry as different typed policies. The first schema treats revive as unsupported and fail-closed. A raw active-restart request reaches P1-A before cleanup; P1-A validates the nested policy and must win the shared terminal-or-restart latch before a terminal arm enters `TerminalFinalizing`. It then enters `RestartClosing` and seals the restart dispatch record before independently requesting P1-E lesson, P2-B course, P1-C execution, P2-A variability, and P2-B presentation quiescence. It seals the one evidence-complete abort only after closure results are known. Successful closure alone disposes and performs the actual dispatch; a failed barrier leaves the old run `ClosureFaulted` and creates no new run.
7. Preserve the local ownership boundary: existing archetype/role/candidate authorities compose `EnemyVariantProfile`; `StageEnemyVariantBindingSet` owns only versioned membership and each binding adds variant identity over a P1-C scoped spawn key. The typed adapter returns a configuration receipt during P1-C inactive staging, while P1-C alone retains group/order/payload/prefab/anchor/lifetime; neither layer redefines the whole enemy behavior system.
8. Require `ACC-P2A-TUNING-CADENCE-ONE-VARIABLE` before promoting a tuned rule/modifier/variant revision. Freeze the baseline scenario and configuration digest, change exactly one variable family, compare committed clear/fail, active-time, damage-pressure, and qualified summon-answer facts, then record accept/reject rationale and the exact rollback revision. A three-profile authoring receipt without this comparison proves configuration identity, not tuning safety.

#### P2-B — Lesson chain and presentation handoff

Use [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md) as the authoritative course-chain review contract. It is a companion to, not a replacement for, the presentation lifecycle spec.

1. Make story handoff cleanup explicit: `sequence/flow request -> acquire presentation ownership -> fade/dialogue -> stop registered work -> restore captured state -> release gameplay ownership -> publish one receipt/handoff`.
2. Give every presentation request a generation token, immutable authoring identity, named quiescence barrier, and closure-fault evidence. Cancel every owned coroutine, timer, async completion, callback, listener, observer, playback resource, and transient cache in the common terminal path. A completion from an older generation cannot reacquire state or publish a handoff.
3. Implement the first handoff as an adapter over the existing `CinematicSequenceProfile` contract and the actual playback owner. The `intro-to-stage` fixture keeps its scene `PlayableDirector` as the sole playback driver; a later runner-backed fixture adapts `CinematicSequenceRunner` without binding both drivers to the same cues. Do not add another cinematic profile or router merely because `ReturnMode`, `TargetId`, input-release delay, HUD restore, and time-scale restore are not yet generically consumed.
4. Add one optional `TutorialCourseDefinitionRef` to the canonical spine only in a later schema. At logical entry, deep-snapshot exactly three ordered entries (`BasicLesson`, `FreePractice`, and `SummonMasteryChallenge`) into the one P1-A run. Do not create three persistent stage nodes or a TutorialProgress owner.
5. Basic may advance only from a closed P1-E `TutorialAttemptResult` with sealed gameplay and presentation boundaries. Presentation completion alone never opens Practice, and the current director plus course coordinator may not both advance the same entry.
6. Free Practice has no success/mastery/progress meaning and needs a later P1-C `NonTerminalPracticeActivity` binding; the current required-defeat sequence cannot be relabelled. Seal one typed `ProceedToChallenge` transition selection before cleanup, then await P1-C/P2-B and any separately reviewed P2-A entry-scoped receipts. Do not prematurely seal the run-level P2-A barrier. Cleanup fault keeps Challenge locked; revision 1 has no in-place Practice reset.
7. Challenge starts with fresh entry/execution generations inside the same P1-A run. P1-A owns Clear/Fail and P1-D alone evaluates the exact qualified `UseSummonForNeed` row. Clear without that row remains Clear but is not derived course mastery.
8. Treat retry before an outcome as a separate active-run restart. The raw request reaches P1-A before cleanup; P1-A seals the snapshot-derived dispatch, then independently awaits the P1-E lesson, P2-B course, P1-C execution, P2-A variability, and P2-B presentation barriers and seals one evidence-complete abort. Post-result Replay/Retry continues through the offered-action executor and every successful re-entry begins at Basic with a new run.
9. Keep TutorialProgress, checkpoints, persistent lesson unlock, rewards, branching, multiple courses, and copied external mechanics deferred.

P2-B lifecycle completion is not a perceptual verdict. `ACC-P2B-PRESENTATION-PERCEPTUAL-REVIEW` separately requires a hash-addressed capture set and versioned rubric from the same source/config/current build after terminal parity passes. It reviews the actual Timeline/Director camera, actor, fade, HUD, intended `AudioSource` output, skip readability, and handoff continuity; static bindings or cleanup receipts cannot satisfy it.

First bounded slices, in `P2-A0-A4 -> P2-B0-B5 -> joint P2-A5/P2-B6` order:

- one recommendation-only featured-summon rule projected from the immutable snapshot; it proves briefing/snapshot coverage but makes no gameplay or cleanup claim;
- one later enforced summon-answer availability rule only after a source-scoped action/loadout port can restore a nondefault prior state without releasing another owner's lock;
- one environmental modifier with typed apply/remove lifecycle, exact ownership receipts, stale-generation protection, and P2-A quiescence;
- one existing P1-C-bound enemy identity and sole mapping prefab reused by Story, Practice, and Challenge profiles, with distinct inactive-stage configuration receipts; only Story binds to the canonical route initially;
- one existing cinematic/story handoff whose natural completion, skip, cancel/disable, active-run restart, and scene-exit paths restore the captured camera/controller, input/movement, HUD, time scale, audio listener, actor visibility/controller, fade, and playback-lock states exactly once;
- one isolated three-entry Boss Barrage course contract after that presentation fixture, with closed Basic evidence, no-proof Practice, fresh-generation Challenge, and no product-route promotion;
- proof that pre-result restart is allowed only by the nested resolved P2-A policy, seals an immutable dispatch record before cleanup, performs actual dispatch only after successful closure/disposal, seals one evidence-complete abort after closure, and never fabricates a clear/fail summary or offered result action;
- proof that stale load/start/playback completions from an older request generation cannot reacquire ownership or publish a second handoff after retry or unload;
- proof that a handoff target is resolved from stable stage/cinematic IDs and that unsupported return modes fail validation instead of silently setting `GameplayHandoffReached`;
- no free-form affix combinations and no general modifier graph.

### P2-C — Connect progression and a minimal reward promise

Review contract: [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md). It is an analysis-only future gate and does not move P2-C ahead of the authoritative sequence.

Admission is demand-gated rather than inferred from peer economies. `ACC-P2C-REPLAY-DEMAND` requires one named versioned report bound to the tested candidate revision, cohort and denominator, observation window, voluntary manual Replay or repeated-practice event definition, predeclared threshold, observed numerator/rate, and reviewer decision. Mandatory regression reruns, staff preference, static repeat-reward fields, and automatic navigation do not count. If the product predecessors pass while this predicate does not, P2-C moves from dormant to hold rather than weakening the predicate.

1. Reuse and transactionally extend the minimal `StageProgressState`, durable result intent, applied delta, and global application ledger established by P1-D; do not create a second progress owner. For P2-C-capable new runs, cut over from the standalone P1-D writer to one combined progress/reward settlement so the same run is not applied twice. Keep state separate from the authored `StageProgressionNode` and preserve clear count, first-clear run ID, achieved mastery IDs, best total-active elapsed milliseconds/provenance, state revision, and per-run application history.
2. Store explicit typed prerequisite states and next-progression-node IDs in the same identity domain used by `StageProgressState`; derive the linked playable-stage route only after resolving the target node. Never infer progression from numeric or lexical IDs or collapse `PASS` and `COMPLETE` into one undocumented boolean.
3. Keep `battleStageId` separate from `progressionNodeId`. Author explicit pre-battle story, post-battle story, and optional after-clear script references rather than deriving them from ID equality or row order.
4. Commit victory before dispatching post-battle story or after-clear hooks. Fail, retry, abort, stale, and duplicate paths cannot dispatch post-clear side effects. `ACC-P2C-POST-CLEAR-STORY-HANDOFF` requires one non-null positive fixture: committed Clear issues one P2-B request, every non-Clear/stale/duplicate arm issues zero, story skip/cancel cannot change settlement, and navigation waits for the terminal quiescent presentation receipt.
5. Snapshot the current node plus every prerequisite/unlock-relevant `StageProgressState` and revision, then resolve the committed run purely into `ProgressionResolution`: first-clear flag, newly achieved mastery IDs, newly unlocked nodes, and eligible reward-bucket IDs. Never mutate state before first/new-state decisions are complete.
6. Apply the resolved state transition exactly once for the run; failed, stale, aborted, or duplicate callback/application paths do not add a second mutation. A legitimate replay after clear is a distinct new run.
7. Use one revisioned `RunRewardPlan` with conditional buckets such as `EveryClear`, `FirstClear`, and `FirstMastery` instead of parallel plan objects.
8. Keep authoring references such as `baseRewardRef`, `firstClearRewardRef`, and `allObjectivesRewardRef` distinct inside that plan. A label like base/default does not by itself mean every-repeat grant.
9. Derive reward preview from the authoritative plan plus current progress; preview/catalog rows never grant or override eligibility.
10. Keep authoritative result/progress update, categorized reward payload, and durable receipt conceptually distinct even if one local transaction writes them together.
11. Resolve the full inventory delta before mutating progress/application state. The first bounded slice must atomically commit progress, the application ledger, one versioned balance, journals, and receipts in one reviewed transactional local store and publish the committed generation last. A backend that offers only a recoverable journal/outbox is deferred to a future split-store review and does not satisfy this first-slice gate. Never copy the observed unsafe shape `persist claimed -> call fallible inventory mutation` without recovery.
12. Produce a `RewardReceipt` with an idempotency key derived from run ID, reward-plan ID, plan revision, and bucket ID. A duplicate request returns the same receipt/result instead of a generic failure; the result UI displays the receipt but never grants it.
13. Before enabling rewards over P1-D progress, close `ACC-P2C-STORE-MIGRATION-DISPOSITION`: name the retained store and schema epoch, choose an explicit migration policy for already-cleared/mastered state, stage and verify a complete state/application-ledger import, and atomically publish the P2-C schema/epoch plus imported committed root before accepting a new P2-C entry. Never dual-write the old and new stores. A later repeat clear must never receive an ordinary `FirstClear` bucket merely because no older reward receipt exists, and a typed `NoMigrationRequired` arm is legal only when exact schema/store identity proves it truthfully.
14. Start with one reward path tied to the first summon lesson and one recommended growth action, not a full inventory or equipment maze.
15. For revision 1, keep failed-run Retry free and available after its committed Fail summary because Fail writes no progress. Keep manual clear Replay/Lobby free but enable them only after the P1-D progress application or P2-C combined settlement is durably committed; summary commit alone is not enough for a persistent Clear. Each action remains distinct and creates a new run. Defer automatic repeat, entry/claim/refund cost, random drops, and fast-clear behavior until repeat play justifies a versioned policy.

### Four-axis closure audit — verified partial

The roadmap is broad enough to cover function, content, presentation, and operations, but it is not yet complete. This is a local contract audit only; it admits no new foreign claim and does not change the frozen P1-B cutoffs.

| Axis | Covered spine | Remaining closure rows now made explicit |
|---|---|---|
| Function | P1-A run/result/abort, P1-C execution, P1-D mastery/progress, P1-E lesson/reset, P2-A variability, P2-B lifecycle/course, P2-C settlement | `ACC-P1C-CANONICAL-LEASE-INTEGRATION` keeps an isolated count-1 test from substituting for the canonical lease/gate/quiescence join |
| Content | logical stage/segment, encounter, lesson/course, enemy variant, story and repeat-content holds | `ACC-OPS-SECOND-STAGE-CONTENT-TRUTH` requires a genuinely distinct logical stage; `ACC-P3-REPEAT-SLICE-CONTRACT-FROZEN` prevents a broad unfrozen content shell |
| Presentation | three accepted P1-B static hygiene cutoffs, one separate accepted canonical-selection cutoff, plus future P2-B ownership lifecycle | Candidate-05 adds no perceptual/runtime lifecycle claim; `ACC-P2B-PRESENTATION-PERCEPTUAL-REVIEW` supplies actual A/V review and `ACC-P2C-POST-CLEAR-STORY-HANDOFF` supplies the positive Clear-only story path |
| Operations | current build/perf, diagnostics/privacy, authoring ledgers, QA/localization/support/incident, release/rollback | `ACC-OPS-P2-ACCESSIBILITY`, prospective matched first/second authoring ledgers plus `ACC-OPS-SECOND-STAGE-THROUGHPUT-COMPARISON`, and `ACC-P3-RELEASE-CANDIDATE-READINESS-COVERAGE` prevent authored-only accessibility, baseline-only throughput promotion, coarse dependency blocking, and stale readiness reuse |

### Cross-cutting production operations gates

These gates are locally required production hygiene, not claims copied from a peer dataset and not permission to build live operations early. Priority is dependency order; a lower number is earlier, but its phase gate still controls when work may begin. Each row uses the same evidence/effect/cost/dependency/risk/owner/exit format as the product backlog. The machine registry treats the named acceptance IDs below as required rows, not prose reminders. Diagnostics and privacy remain distinct; build reproducibility and Android-device performance remain distinct; first-pocket measurement and second-stage throughput remain distinct; and the four P2 readiness proofs cannot be collapsed into one unchecked bundle.

| Priority | Earliest phase | Current evidence and operational gap | Player/production effect | First bounded slice | Cost | Dependencies | Principal risk | Contract owner | Exit evidence |
|---:|---|---|---|---|---:|---|---|---|---|
| 1 | P0/P1-0 and every promoted wave | A named pre-P1 software build/performance reference exists, but it is neither a current-candidate build manifest nor an Android-device verdict | prevents a function-green wave from silently changing package/config/source or regressing the device route | for every wave close `ACC-OPS-BUILD-MANIFEST-CURRENT` against exact source/config/package hashes; before device/release promotion separately close `ACC-OPS-ANDROID-PERF-CURRENT` on one named representative Android device across one cold route and warmed Replay/Retry/second-stage loops | 2 | `CONTINUOUS-P0-NONREGRESSION`; deterministic build inputs; connected representative device for the device arm | an old Editor/development APK is quoted as current proof, package reproducibility is conflated with device behavior, or one short pass hides cumulative memory/jank/thermal drift | build-manifest verifier and Android performance verifier; neither owns gameplay truth | current package/config/source revision and SHA-256 plus reproducible route/build report; separately exact device/OS/resolution/settings, frame p50/p95/p99 and jank, PSS/GPU-memory slope, thermal/time-to-throttle, player GC/load/shader spikes, background/resume, soak, thresholds, and variance rule |
| 2 | P1-A | no minimal run-health event schema or inert sealed-record adapter exists | makes handoff/result/action failures diagnosable without changing combat truth or collecting broad behavior | emit local, non-exporting events only for admitted run start, sealed handoff, committed outcome, diagnostic abort, typed action dispatch, and validator failure; exclude raw input, free text, device identity, economy, and uncommitted facts | 3 | P1-A immutable run identity, terminal record, summary/abort, and typed dispatch records | telemetry becomes a second outcome owner, duplicates mutable facts, or its failure changes gameplay | inert diagnostics adapter; never the run/result or privacy owner | `ACC-OPS-DIAGNOSTICS-SEALED-INERT`: fixed schema/revision, duplicate/stale classification, event-to-sealed-record equality, and offline/drop/throw injection with zero gameplay effect; no sink configured |
| 3 | P0 policy, P1 schema, before any export | no analytics classification, PII denylist, consent/access boundary, retention/delete/export policy, or processor approval exists | prevents diagnostics from silently becoming player surveillance or an irreversible external dependency | default to no external collection; freeze an allowlist plus PII/free-text/raw-input denylist, local retention/delete/export behavior, and zero-egress capture | 3 | proposed diagnostics schema; product/privacy decision; external sink typed absent | identifiers leak, retention becomes indefinite, or a vendor is adopted before responsibilities are approved | privacy governance owner, separate from diagnostics and gameplay | `ACC-OPS-PRIVACY-ALLOWLIST-ZERO-EGRESS`: versioned data map, denylist tests, local retention/delete/export verification, and zero-egress capture; any later sink requires separate approval |
| 4 | before the first P1-C execution/runtime mutation, then after P2-B presentation | repository counts exist, but no prospective matched time/edit/defect ledger proves either the first pocket or a second stage reduces bespoke work; prior P1-B effort is not reconstructable evidence, and reuse alone does not prove a second product stage | turns architecture reuse into measurable content throughput and exposes hidden scene/presentation/foundation cost without counting a catalog alias as content | first freeze `AUTHORING-LEDGER-01`, `THROUGHPUT-COMPARISON-01`, matched scope, opening-receipt schema, and a material threshold; open the receipt before the first P1-C runtime mutation and measure the accepted pocket; after P2-B freeze one exact candidate, open its receipt before mutation, and close delivery/truth/ledger/comparison | 4 | ledger contract before P1-C runtime work; accepted P1-C/P2-B plus first ledger for second-stage rows | retrospective or late-opened ledgers, two unmatched baselines, or a reusable-looking contract with bespoke scene/runtime work falsely promotes throughput/P3 | stage-spine/encounter/presentation authoring and validator owner | immutable start receipts plus same-schema session/artifact/manual-edit/defect/reuse ledgers; distinct route/template/briefing/progression/result/admission snapshot, executable encounter, exact self-entry endpoints, host-specific isolation, zero new scene-specific owner; preregistered threshold and both continuous gates |
| 5 | P1-D/P2-C | no production progress-store migration, backup, interrupted-upgrade recovery, or rollback-read boundary exists | protects clear/mastery/reward truth across upgrades and crashes | close `ACC-P1D-PROGRESS-MIGRATION-RECOVERY` and `ACC-P1D-PROGRESS-BACKUP-ROLLBACK-READ`; before reward cutover also close `ACC-P2C-STORE-MIGRATION-DISPOSITION` | 5 | P1-A committed result; P1-D identity/application ledger; selected P2-C settlement schema/store | irreversible loss, duplicate settlement, split ownership, or an older binary mutating newer state | sole progress/settlement store and migration owner | exact migration manifest, staged activation, interrupted/corrupt recovery, recovered application/receipt, backup restore, documented rollback-read limit, and P2-C import-or-truthful-no-migration receipt |
| 6 | P1-E/P2-B | canonical PlayMode coverage and authored accessibility catalog/labels/safe-area structures exist, but no reusable cross-input/resolution/lifecycle QA matrix or executed accessibility disposition exists | prevents a generalized tutorial/handoff or StageSelect from working only on one input/display profile or leaving authored-only accessibility promises | cover applicable keyboard/controller/touch input, aspect/resolution, complete/skip/cancel/retry/unload, focus loss, stale async completion, plus StageSelect Start/Back labels, focus order, minimum font, prompt, safe-area and explicit Unsupported arms | 4 | stable P1-E attempt/reset; accepted P2-B; current build manifest; accessibility/localization rows remain independently judged | input soft-lock, clipped prompts, double restore, late callback, or authored narration/contrast misreported as executed | QA matrix and accessibility disposition owners; runtime authorities supply machine-readable seams | `ACC-OPS-P2-QA-MATRIX` plus `ACC-OPS-P2-ACCESSIBILITY`: versioned execution/disposition matrices, bounded visual/device checks, and no leaked ownership or authored-only runtime claim |
| 7 | P2-A | no versioned tuning cadence or one-variable comparison governs rules/modifiers/variants | expands replay variety while protecting fixed-rear, forward-risk, summon-answer identity | snapshot tuning revision/scenario, change one variable family, compare committed route facts, and retain an exact rollback revision | 3 | P1-A route facts; P1-C deterministic encounter; P2-A resolved configuration digest | multi-variable drift makes outcomes unreviewable or variety erases the summon answer | stage-rule/variant tuning owner; runtime consumes frozen configuration | `ACC-P2A-TUNING-CADENCE-ONE-VARIABLE`: versioned report/digest, before/after facts, accept/reject rationale, and exact rollback revision |
| 8 | P2-B/P2-C | player-facing copy remains split across catalog, scenes, tutorial, result, and reward plans; no stable string-key owner or pseudo-locale gate exists | keeps briefing, lessons, results, and actions aligned and readable across locales | move only admitted copy behind stable string IDs; validate missing/duplicate keys, pseudo-locale, and layout stress | 3 | P1-B canonical read model; P1-E/P2-B surfaces; P2-C reward copy only when admitted | copied strings drift, a locale key becomes product identity, or translated UI hides an action | localization catalog/read-model owner; gameplay retains semantic IDs | `ACC-OPS-P2-LOCALIZATION`: pseudo-locale pass, missing/duplicate-key validator, overflow/resolution checklist, and no display string used as semantic identity |
| 9 | P2, before external support intake | no privacy-safe support bundle or synthetic incident drill exists | gives support actionable immutable evidence without raw player data or authority to rewrite gameplay truth | package only sealed IDs/digests, current build/config hash, fault codes, and redacted environment class; inject one named fault and exercise detection, triage, ownership, recovery, and closure | 3 | `ACC-OPS-DIAGNOSTICS-SEALED-INERT`; `ACC-OPS-PRIVACY-ALLOWLIST-ZERO-EGRESS`; current build manifest; QA seams | bundles expose identifiers, mutable screenshots/logs are treated as truth, or incident handling bypasses contracts | support/incident owner with severity/escalation matrix | `ACC-OPS-P2-REDACTED-SUPPORT` plus `ACC-OPS-P2-INCIDENT-DRILL`: redaction/schema tests, runbook, named-owner delivery, preserved canonical truth, and recovery/closure evidence |
| 10 | P3 | no release train, staged activation, live rollback, candidate-coherent readiness coverage, release-manifest owner, or approved metric inventory exists | permits repeatable content service only after current-candidate content, demand, persistence, QA, localization, accessibility, support, privacy, and deployment are proven | after every predecessor, define release cadence/rollback and bind app/package, each content-bundle digest, compatible schema range, activation cohort, in-flight-run policy, rollback target, cache invalidation, and every readiness row to one manifest as `FreshPass` or `CoveredHashesUnchanged`; metric inventory approval keeps the external sink typed absent | 5 | accepted bounded content slice; accepted matched second-stage throughput comparison; replay demand; progress/store migration; all OPS-P2 rows; both continuous gates; privacy approval | old readiness evidence survives changed reward/copy/UI bytes, an incompatible in-flight run crosses activation, rollback/cache invalidation cannot restore content safely, or metric approval silently becomes egress approval | named release owner; privacy retains data authority and support retains case authority | `ACC-P3-RELEASE-ROLLBACK`, `ACC-P3-RELEASE-MANIFEST-OWNERSHIP-METRICS`, and `ACC-P3-RELEASE-CANDIDATE-READINESS-COVERAGE` against one exact candidate |

#### `AUTHORING-LEDGER-01` / `THROUGHPUT-COMPARISON-01`

Freeze both schemas, the first matched scope fingerprint, measurement endpoint, exclusions, immutable opening-receipt shape, and a product-approved material-improvement threshold before the first comparable P1-C execution/runtime mutation. Existing P1-B work—including static Station Add identity/anchor/payload authoring—is `RetrospectiveUnavailable` and outside the prospective clock; estimates may explain history but cannot become the improvement baseline. Each first-pocket or second-stage ledger must seal its ID/schema, start manifest, scope fingerprint, timestamp, contributors, exclusions, and threshold before its first in-scope mutation; a late reconstruction is a non-pass.

Each ledger uses the same schema and records:

- exact candidate/content-promise/scope/source-manifest identity, start/end timestamps, reviewer, and decision rule;
- contributor session rows split into active person-time, tool wait, and excluded time with reason;
- every touched artifact with kind, before/after digest, authoring mode, and in-scope disposition;
- each manual scene/asset edit by stable object/property identity plus before/after digest, distinct from generated work;
- stable deduplicated defect IDs with class, severity, first failure, fix, retest, status, and blocking disposition;
- an eligible reuse denominator and per-capability reused/new/excluded disposition;
- separate `foundationInvestment`, `contentAuthoring`, and `stabilization` effort, plus exact formulas for active time, reuse ratio, manual edits, blocking defects, and new scene-specific owners.

Measurement ends only when the exact ledger manifest has content-truth, relevant validator, focused runtime, route, presentation-lifecycle, and both continuous-gate evidence accepted. The first validator PASS is not an endpoint. `ACC-OPS-SECOND-STAGE-THROUGHPUT-COMPARISON` is independently required: baseline-only, retrospective, scope-drifted, or otherwise incomparable ledgers are typed non-passes. A pass additionally requires the preregistered active-time reduction, all mandated shared capabilities reused, `newSceneSpecificOwnerCount == 0`, no increase in manual scene edits or blocking defects, and reviewer acceptance.

`OPS-P2-READINESS` is complete only when all five rows pass independently: `ACC-OPS-P2-QA-MATRIX`, `ACC-OPS-P2-LOCALIZATION`, `ACC-OPS-P2-ACCESSIBILITY`, `ACC-OPS-P2-REDACTED-SUPPORT`, and `ACC-OPS-P2-INCIDENT-DRILL`. A single archive containing five file names does not prove their predicates. QA/localization/accessibility need the current build manifest but not Android performance unless they make a device/performance claim; support/incident additionally need the separate diagnostics and privacy rows. The second short stage is the decisive content-operations proof only when it has a distinct logical-stage truth row, a same-schema authoring ledger after accepted P2-B presentation ownership, and a separately accepted matched comparison to the prospective first-pocket ledger. Passing one hand-authored Olympus route, one reuse-only stage, two standalone baselines, or a retrospective estimate does not prove production throughput. Operational rows do not outrank their acceptance-specific dependencies merely because their local cost is low.

### P3 — Broaden content operations only after repeat play works

- Daily practice tasks that reinforce real combat behaviors.
- Multiple practice courses and character/summon-specific lessons.
- Challenge variants, score submission, and reusable enemy configuration sets.
- Additional result presentation and camera/cut-in polish.
- Economy, stamina, shop, passive/base rewards, and liveops only when the core loop earns repeat play.
- Broaden only after two independent admission signals exist: the exact versioned replay-demand report predicate used by `ACC-P3-REPEAT-DEMAND`, and an accepted `ACC-OPS-SECOND-STAGE-THROUGHPUT-COMPARISON` backed by second-stage content truth after P2-B presentation ownership. Throughput is not part of the demand pass decision. Neither a designer preference, a static peer-data field, a baseline-only ledger, nor two incomparable measurements satisfies either signal. `ACC-P3-REPEAT-SLICE-CONTRACT-FROZEN` then selects exactly one content family, exclusions, owner, dependencies, fresh six-axis score, and evidence predicate; only that contract can govern `ACC-P3-REPEAT-CONTENT-SLICE`. Demand admission never auto-accepts implementation, and the historical broad-shell `-3` is not inherited.
- Keep release operations separate from product content. `ACC-P3-RELEASE-ROLLBACK` proves staged activation/rollback behavior, `ACC-P3-RELEASE-MANIFEST-OWNERSHIP-METRICS` proves the hash-addressed manifest, named owner/on-call boundary, and privacy-approved metric inventory while external egress remains typed absent, and `ACC-P3-RELEASE-CANDIDATE-READINESS-COVERAGE` binds every readiness proof to the exact candidate by fresh pass or covered-hash invariance. All remain held until persistence/migration, all five OPS-P2 rows, both continuous gates, measured demand, second-stage truth/ledgers plus the accepted matched throughput comparison, and the accepted bounded repeat-content slice pass.

## Explicit Rejections and Holds

| Reference pattern | Decision | Reason |
|---|---|---|
| PGR signal orbs, three-ping, QTE, character swap, exact star thresholds, or teaching-activity scale | Reject | These are another game's mechanics/content economy and would blur DimensionBrawl's summon-answer identity |
| Directly joining 14,482 `GuideFightStep` rows to four PGR GuideFight records | Reject as unproven | Current evidence supports regional overlay vocabulary, not a verified stage-runtime join |
| HI3 numeric challenge codes, exact monster formulas, score economy, or maximum-combo mastery | Reject or hold | Meanings/formulas are incomplete and combo/score is not yet a proven DimensionBrawl value |
| New generic camera, cinematic, UI tween, or tutorial-overlay framework | Reject for P1 | Existing systems are capable; integration, stable targets, and cleanup validation are the demonstrated gaps |
| Aether-style broad affix/buff catalog or roguelike topology | Hold for P3 | Combination and QA cost arrive before a repeatable base stage loop |
| ZZZ/PGR content volume and character-marketing tutorial shells | Reject as a target scale | Dataset row counts are not product requirements |
| Snowbreak MAA screen recognition as runtime architecture | Reject | It is external automation; only timeout/retry/state-transition test thinking is useful |
| BA/NIKKE entry-cost, refund, fast-clear, random-drop, or stamina policy | Hold | Static data does not prove transaction timing and the demo currently benefits from free retry |
| EpinelPS reward-grant code as an implementation template | Reject | It is an external reimplementation with visible duplicate-grant guard gaps; use it only as a negative test source |
| Wuthering numeric guide/condition codes, QTE/resonance/echo mechanics, AI weights, score tables, repeat-reward fields, or limit-time fields | Reject or hold | Runtime meanings are not decoded, score tables are mode-specific, and the inspected repeat/limit fields contain no nonzero dungeon values |
| Arknights tile/lane/deployment/block/life rules, AP/practice-ticket policy, or reward-display categories as payout mappings | Reject or hold | These are tower-defense/economy-specific, and the inspected static data does not prove grant, consumption, refund, or first-clear semantics |
| GF1 tactical-node/turn rules; GF2 grid/deployment, opaque `T_*` formula parsing, or general trigger DSL; Neural Cloud roguelike rooms, stamina, and daily limits | Reject or hold | The reusable evidence is contract separation and ordered composition, not those games' mode rules or economy |
| CounterSide stage/prerequisite/wave/result fields inferred from missing master-table names or dialogue-transition rows | Reject as unproven | The relevant proxies are missing and the inspected read-first material contains no gameplay-stage identifiers |
| Ash Echoes wave-bar rows as spawn schedules, exact target thresholds, affix/element/formation rules, or map-script behavior inferred from missing scripts | Reject as unproven or game-specific | The archive proves metadata references and typed target shapes, not encounter execution or transferable tuning |
| Path to Nowhere `combat-stage` filenames, natural-language achievement conditions, training-buff prose, dispatch/compliance rewards, or tactical block/move/wave rules | Reject as unproven or game-specific | The preserved source lacks stage runtime/encounter/result joins; derived labels and prose are not executable contracts |
| Genshin exact fade timings, talk/action graph, obfuscated keys, world-time actions, or a skip flag treated as full cleanup; HSR keyword/context pack as camera ownership | Reject as unproven or game-specific | Authoring separation is useful, but runtime input/time-scale/result/retry cleanup was not demonstrated |
| FGO phase-script array positions as before/after story, `repeatLast` as failure retry, `enableFollowQuest` as automatic navigation, or reset-name lists as camera cleanup | Reject as unproven | Quest master data proves phase and after-clear policy shapes, not client execution or ownership restoration |
| Limbus theater row order as automatic next-stage flow, battle-stage ID collapsed into progression-node ID, `story.exit` or `stageScriptNameAfterClear` invoked on failure/retry, or result pivots treated as result UI layout | Reject as unproven | Role-labelled before/after static data is strong, but runtime result/hook order, ID equivalence, navigation, and cleanup are not proven |
| HBR fan website routes/CSS motion, walking-recreation camera paths/events, FOV/blend/speed values, or installed package lists as shipped presentation policy | Reject as unrelated or unproven | The preserved sources contain no official story-stage-result lifecycle or cleanup owner |
| Brown Dust 2 viewer composites, timing, camera values, game assets, or seek/export controls treated as shipped story/result/retry behavior | Reject as unrelated or unproven | The third-party viewer is useful only as an async/listener/resource cleanup failure checklist; it contains no game stage lifecycle |
| Reverse: 1999 `EndDungeonPush` called a durable receipt or atomic server transaction; Epic Seven future-target manifests treated as stage data | Reject as unproven | Reverse proves client payload/order but exposes no receipt/idempotency or server durability; Epic Seven downloaded no relevant stage/PvE source |
| Last Origin `RewardIndex` assumed to mean every-repeat payout, preview rows treated as authority, next/prerequisite links forced symmetric, or Princess Connect hashed columns decoded by guesswork | Reject as unproven | Last Origin proves separate static references but no grant runtime and contains preview drift/asymmetric links; Princess Connect lacks decoded field/runtime evidence |
| Stella Sora emulator `settle(id)` treated as authoritative combat clear, a boolean claim flag called a receipt, or claim-before-inventory order copied as a transaction template | Reject as unsafe/unproven | The emulator trusts a generic client ID, has no run/outcome proof, returns failure on duplicate claim, and exposes no atomicity or recovery between claim persistence and inventory mutation |
| Full equipment, gacha, shop, base/idle, and liveops economy | Hold for P3 | It does not solve the current stage/result/replay gap and carries high identity and scope risk |

## Cross-Game Analysis Queue

### Direct 3D action peers

Historical section-level or supporting narrative (not registry-admitted reproducible evidence):

- Punishing: Gray Raven course, practice, teaching, guide-fight, and result-related surfaces.
- Honkai Impact 3rd early stage, typed challenge, enemy variant, result, and plot/dialog surfaces.
- Aether Gazer topology/rules/presentation lifecycle, with ZZZ group/member and tutorial-presentation cross-check.
- Wuthering Waves dungeon briefing, tutorial attempt/reset, enemy reference, and flow cleanup surfaces.
- Snowbreak classified as indirect QA automation evidence only.

Next queue:

Immediate evidence task, without widening product or implementation scope: use the direct Ark curated PGR/HI3 read-first packs to complete a **non-copying structural gap pass** across stage/combat first, then camera/cutscene and UI/presentation. This research lane may derive candidate shapes, joins, lifecycle questions, and negative boundaries from provenance-complete archive rows; it may not copy source payload into product assets or claim runtime ownership. Keep `P1B-PGR-HI3-STAGE-SPINE-01` as the narrower formal product-evidence packet: its retained-mirror generator, exact two-source provenance bytes, four PGR targets plus HI3 `10101`, seventy cells, drift, reconciliation, and nine candidate packages remain reproducible, but its `LiveAcceptance` is not the gate for reading the Ark archive.

Current correction to that earlier queue state: source-record-selected PGR/HI3 raw bytes, exact five-row selection, 70 candidate cells, PGR drift, and HI3 reconciliation are reproducible from the retained mirror, while direct SMB read-first access supplies the broader research corpus. Preserve the bounded packet and all three verified supporting packages without pretending any missing historical artifact can be recovered under its old identity: PGR has four new versioned successor candidates, HI3 has three new versioned successor candidates, and the two present HI3 helper CSVs retain their historical IDs with exact replay provenance. Those nine remain unadmitted for **formal product-evidence promotion** pending the packet-specific disposition and atomic cutoff; that status no longer disables archive comparison or the next bounded read-first gap pass.

The same index now records, but does not authorize, the next bounded packets. Priority 2 `P1C-ARK-GF2-ORDERED-ENCOUNTER-01` is `proposed-dormant-pending-priority-1` and names exact path-list candidates for Arknights `main_00-01` plus one reproducibly selected Girls' Frontline 2 stage-group-member chain. Priority 3 `P1D-HI3-GF2-TYPED-MASTERY-SHAPE-01` is `proposed-dormant-after-P1C` and names exact HI3/GF2 stage-challenge-condition candidates. Both carry `deepReadAuthorized=false`, acceptance fields, negative boundaries, and stop conditions. They are ordering records only: priority 2 cannot start until priority 1 is exact or explicitly rejected, and priority 3 cannot start until P1-C is accepted. Priorities 4 through 7 retain their existing order. Priority 8 `EVID-PGR-HI3-EARLY-STAGE-ACTION-LIFECYCLE-01` is a separate evidence-only conditional reopen: it appends after priority 7 without displacing any product-coupled packet and cannot start before its P1-B source, product-exit, exact-scope, and explicit-authorization gates.

| Evidence packet priority | Evidence gap | Player/production effect | Cost | Dependencies | Risk | Current disposition |
|---|---:|---:|---:|---:|---:|---|
| 1 — `P1B-PGR-HI3-STAGE-SPINE-01` | 5 | 4 | 2 | 3 | 2 | verified partial: historical local cutoffs/controls remain distinct; one retained-mirror candidate reproduces the exact two raw sources, five rows, 70 cells, PGR drift, and HI3 reconciliation. Four PGR successors, three HI3 successors, and two historical helper provenance candidates all pass their bounded source/producer/package audits, and the cumulative manifest verifies all nine. Supporting admission remains `0/9`; policy/rights disposition plus the atomic eleven-source cutoff, claims, crosswalk, and all three live acceptances remain open |
| 2 — `P1C-ARK-GF2-ORDERED-ENCOUNTER-01` | 5 | 5 | 3 | 5 | 4 | dormant; static ordered-authoring crosswalk only after priority 1 |
| 3 — `P1D-HI3-GF2-TYPED-MASTERY-SHAPE-01` | 4 | 5 | 3 | 5 | 4 | dormant; typed authoring shape only after accepted P1-C facts/execution |
| 8 — `EVID-PGR-HI3-EARLY-STAGE-ACTION-LIFECYCLE-01` | unscored | unscored | unmeasured | gated | high | evidence-only conditional reopen; exactly one PGR and one HI3 lifecycle chain, queued-read-disabled with no product, score, or priority effect |

These values use the roadmap's existing 1-5 evidence/effect/cost/dependency/risk axes. The order is not a naive score sum: the authoritative product dependency chain `P1-B -> P1-C -> P1-D` wins, so high-value mastery work cannot jump ahead of the execution and fact-provenance gate.

The evidence index also records the following later **read-disabled** peer queue. These are ordering records only, create no product-backlog work item, and keep `deepReadAuthorized = false`. Every packet must first admit an exact source record, snapshot/generation identity, byte hash, retained extraction command, bounded target row/file set, license/provenance boundary, and stop condition. A path-list candidate or historical section narrative cannot satisfy admission.

| Later order | Read-disabled packet | Earliest dependency-safe use | Required negative boundary |
|---:|---|---|---|
| 4 | `P1E-WW-TUTORIAL-ATTEMPT-RESET-01` | after priority 3 and accepted P1-D fact semantics; one exact Wuthering Waves tutorial/guide target plus a directly evidenced consumer or bounded runtime trace | static guide/condition/reset fields cannot prove attempt ownership, complete/fail/skip/break/reset order, or cleanup |
| 5 | `P2A-WW-ENEMY-LAYERING-01` | after packet 4 and accepted P1-C execution provenance; one exact identity/configuration/behavior-layer target | static joins cannot prove live AI selection, stat authority, mutation order, or teardown |
| 6 | `P2B-GENSHIN-PRESENTATION-AUTHORING-01` | after packet 5 and the P1-B canonical presentation identity; one exact presentation authoring target | pre/perform/finish fields cannot prove input/HUD/camera/time-scale ownership, cancellation, or restoration |
| 7 | `P2C-R1999-RESULT-PROGRESSION-CLIENT-ORDER-01` | after packet 6 and accepted P1-D durable progress; one exact Reverse: 1999 client result/progression ordering target | client request/push/model order cannot prove server transaction atomicity, durable idempotency, reward grant, or crash recovery |

The later queue never upgrades static authoring to runtime truth and never upgrades client ordering to server truth. If exact admission fails, record the rejection boundary and stop rather than substituting a mirror, another locale, or a different snapshot.

1. PGR follow-up only when one `PracticeActivity` row can be joined to a direct runtime consumer plus result/progress persistence.
2. HI3 follow-up only when one early-stage row can be joined through entry/Lua, wave/enemy execution, typed condition evaluation, and result/lose/retry ownership.
3. Wuthering Waves follow-up only when one guide can be joined to a direct consumer or runtime trace covering complete/fail/skip/break/reset and cleanup.
4. Aether Gazer follow-up only when one stage config can be joined through the runtime wave/entity executor to result and cleanup.
5. ZZZ follow-up only when one Floor/Group/Member and NewbieGroup identity can be joined to a trustworthy stage/course/attempt/reset runtime path.
6. Snowbreak remains outside the queue unless game-internal data appears; external MAA material remains QA-only.

Questions:

- How is one mechanic isolated, validated, repeated, and then combined?
- Which data belongs to stage, tutorial, enemy configuration, result, or progression?
- How are camera and input restrictions released on success, failure, cancel, restart, and scene handoff?

### Stage/progression specialists

Historical supporting narrative (not registry-admitted reproducible evidence):

- Blue Archive mastery/reward buckets.
- NIKKE static stage references and external-server failure-mode cross-check.
- Arknights typed prerequisite graph, metadata/level split, ordered level actions, and enemy stage-local overrides.
- Girls' Frontline mission topology, GF2 ordered encounter/challenge/tutorial joins, and Neural Cloud result/reward/retry owner separation.
- CounterSide bounded evidence check; retained only as weak Stage/Map and enemy-layering support because the actual stage masters are absent.
- Ash Echoes explicit pre/post/map references, mandatory-versus-optional target split, reward authoring buckets, tutorial graph, and missing runtime-script boundary.
- Path to Nowhere bounded evidence check; retained only for mastery presentation separation and modifier display/payload separation because stage runtime data is absent.
- Reverse: 1999 client result/progress/bonus ordering and explicit episode graph; receipt/idempotency remains unproven.
- Limbus Company role-labelled pre/post-battle story references, distinct battle/progression IDs, after-clear hook reference, and result/reward authoring boundary; runtime order remains unproven.
- Last Origin independently directed prerequisite/next links, stage-to-wave/group joins, and separate base/first/all-objective reward authoring; grant runtime remains unproven.
- Princess Connect retained only for conservative quest-to-objective separation because hashed master data does not prove result/progress/reward flow.
- Stella Sora bounded community cross-check: complete level/floor authoring join, ID-keyed passed/claimed progress, and a claim-before-inventory ordering risk; no official outcome proof, atomic receipt, actual failure trace, or runtime floor consumption was promoted.
- Epic Seven excluded because its inspected source explicitly omits stage/PvE runtime data and downloaded no candidate files.

Next queue:

1. P1-A current-schema full exit is closed; P1-B direct join, static port/current-binding hygiene, and anchor/profile stage-context hygiene are three separate accepted immutable presentation cutoffs. Candidate-04 remains a distinct historical failure and Candidate-05 a separate accepted canonical-selection cutoff. Preserve truthful-join proposal/rev2 AMEND history, rev2A/implementation acceptance, Candidate-06/07 historical failures, and the separate Remediation3 result/progression acceptance. Next close Station Add, live PGR/HI3 foreign evidence, and the P1-B full-exit audit. Keep every cutoff distinct.
2. Revisit Stella Sora only if official/direct result validation or transaction evidence appears; the community emulator remains a negative test source.
3. Revisit Epic Seven, CounterSide, Path to Nowhere, Princess Connect, or other excluded archives only if stronger decoded runtime material becomes available.

Questions:

- How are prerequisite, star/mastery, first clear, repeat clear, and next-stage recommendation separated?
- How much information belongs on the stage card versus prep and result screens?

### Presentation and narrative support

Historical section-level or boundary narrative (not registry-admitted reproducible evidence):

- Genshin Impact pre/perform/next/finish, fade/skip, UI show/close, and camera/DOF authoring shapes.
- Honkai: Star Rail excluded from lifecycle decisions because the current pack does not prove camera/input/HUD ownership or cleanup.
- Fate/Grand Order battle/story phase and after-clear policy separation; no result UI or cleanup runtime was inferred.
- Limbus Company explicit before/after battle-story links; no runtime result ordering or cleanup was inferred.
- Heaven Burns Red excluded because the preserved web/fan-walking sources contain no official story-stage-result lifecycle.
- Brown Dust 2 retained only as a third-party viewer cleanup-failure checklist; it contributes no shipped stage/story/result lifecycle.

Next queue:

1. HoYoverse rendering reference only for later visual polish, not lifecycle ownership
2. Revisit HSR, HBR, or Brown Dust 2 only if actual client/runtime lifecycle evidence becomes available

Questions:

- Which presentation behaviors deserve reusable profiles rather than scene-specific scripts?
- How should story handoffs frame a short combat stage without delaying replay?

### Conditional evidence-reopen queue

This queue is dormant. A candidate receives one bounded pass only when the named stronger evidence is available and the reproducibility manifest above can identify it. Meeting a stop condition ends that pass without inventing a product requirement.

| Rank | Candidate and unresolved question | Evidence required to reopen | Immediate stop condition |
|---:|---|---|---|
| 1 | PGR `PracticeActivity` plus GuideFight/result-progress consumer: how do tutorial, practice, challenge, attempt proof, result, progress, and reboot actually join? | one raw `PracticeActivity` row, its direct runtime consumer or trace, and the result/progress persistence join | only presentation rows or the large `GuideFightStep` catalog remain, while the four GuideFight/runtime joins stay absent |
| 2 | HI3 early-stage Lua and decoded StageChallenge/StageResult: who owns waves, typed challenge, lose/result, and retry? | one `StageData_Main` row joined through entry/Lua, enemy/wave execution, condition evaluator, and lose/result flow | only static/deobfuscated tables and opaque condition IDs remain |
| 3 | Wuthering Waves GuideStep/ComboTeachingCondition: what is the real complete/fail/skip/break/reset/cancel/handoff cleanup state machine? | one tutorial's direct client consumer or runtime trace plus ownership-release evidence | only ConfigDB fields/frequencies remain without an executing consumer |
| 4 | Aether Gazer stage topology/wave executor: how are ordered waves, restrictions, revive, and modifiers consumed by spawn/completion/cancel/result? | one stage config joined to runtime entity/executor, result, and cleanup | only public config and row counts repeat without the runtime chain |
| 5 | ZZZ Floor/Group/Member plus NewbieGroup: does guide presentation join stage/course/attempt/reset and encounter execution? | one trustworthy client consumer or trace joining a floor/tutorial ID through that chain | only UI target/media and placement rows remain without stage/course/result/reset ownership |
| Watch | Snowbreak | game-internal stage/runtime material | MAA-only evidence |

Conditional ranks 1 and 2 are the two fixed arms of `EVID-PGR-HI3-EARLY-STAGE-ACTION-LIFECYCLE-01`, not two independent scans. The packet permits one selected chain per game, stops each arm at its first broken edge, and cannot substitute or union another snapshot, locale, stage row, historical control, or broad catalog. Its global evidence priority is 8, so this consolidation does not renumber priorities 2 through 7 or create a product prerequisite.

## Promotion Rule

A reference-derived idea enters implementation only when all answers are yes:

1. Does it solve a verified current gap?
2. Does it strengthen the summon-first combat identity?
3. Can it be implemented as a small vertical slice?
4. Does it avoid destabilizing the canonical demo route?
5. Is the evidence strong enough to define a testable contract?

## Next Analysis Actions

1. Retain the accepted P0 artifacts as the known-good baseline at 28/28, and retain the frozen P1-0/D4b evidence separately: scoped 14/14, full route 1/1, actual Retry/Lobby 2/2, exact validator/inventory bypass `0`, and graphics-enabled route-frozen aggregate 37/37. The validator plus complete evidence set, not any single suite alone, supports the freeze.
2. Keep the retired Review result owner from reappearing; P1-A2 now upgrades the retained additive surface into the D3 shared shell and typed executor, the verified fact payload extends its committed summary, and the bounded post-49 slice projects committed clocks/proof through an exact localized presentation snapshot. Remaining finalization/closure and future admitted-owner or P2-C reward presentation must use that same seam without adding a second owner.
3. Treat D1 identity, D2 `Clear -> Replay + Lobby`/`Fail -> Retry + Lobby`, D3 outcome-aware shared result shell, and D4a causal-order/same-epoch Clear-wins semantics as product-approved. Treat D4b as a separate revision-1 technical freeze, not an additional product decision.
4. Record P1-0 complete: route/Station validators PASS, mutation inventory is `8/4/1` with authorized core `1` and bypass `0`, policy revision/digest and route revision/digest match the frozen asset, and the graphics-enabled route-frozen aggregate passes 37/37. Future inventory/digest/regression drift fails closed and blocks promotion.
5. Record P1-A current-schema full exit complete at the unchanged-source 79/79 cutoff. Record P1-B's presentation, catalog, truthful-join, and Remediation3 result/progression cutoffs separately, preserving Candidate-04/06/07 as historical failures and proposal/rev2 as AMEND. Next close Station Add, live PGR/HI3 foreign evidence, and full exit. Keep all accepted P1-A regression rows active.
6. Review [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md), then during P1-C0 approve the stage-local binding; run-admission plan/digest and canonical scene reservation; sole activation envelope with stale-command classification; scaled clock and cancel precedence; separate group/sequence states; `CombatHealth.Died` terminal; binding-root-local scene-ready pose capture/tolerances; inactive staging plus transactional payload mapping; named completion-gate CAS/phase-open order; canonical-priority PVE scene lease; and exact fixture IDs. Do not freeze a fixture until P1-A lifecycle/quiescence and P1-B's exact current-route pocket plus real count-1 Station Add payload/anchor exist.
7. Review [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md) against the verified `intro-to-stage` direct chain and all three closed local presentation-hygiene subgates. The prior anchor/profile prerequisite is satisfied, but do not begin the P2-B ownership fixture before P1-B full exit and the intervening roadmap predecessors; then choose its first input/HUD ownership fixture and add the named presentation quiescence/fault boundary. Review [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md) separately; do not freeze it until the exact summon Basic, Practice host/baseline, and Challenge proof/objective exist.
8. Review [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md), then during P1-D0 approve objective-ID semantic permanence plus its identity manifest/tombstones, objective-set canonicalization, bundle-invalid policy, canonical total-active milliseconds, exact qualified summon-proof fixture, save-profile namespace, durable prepared-intent acknowledgment boundary, corrected stage-select projection, and one fault-injectable checksummed generation store. Close `ACC-P1D-PROGRESS-MIGRATION-RECOVERY` and `ACC-P1D-PROGRESS-BACKUP-ROLLBACK-READ` independently of current-schema atomicity. Retain [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md) as the later P2-C gate that replaces the standalone writer for new cohorts and extends the same state/ledger store with one versioned balance, settlement generations/reservations, frozen reward revision, and receipt retention; require `ACC-P2C-REPLAY-DEMAND` and `ACC-P2C-STORE-MIGRATION-DISPOSITION` before that cutover and do not implement the reward layer early.
9. Carry forward Brown Dust 2's stale-async cleanup checks and Limbus Company's explicit before/after story plus separate battle/progression IDs without promoting either above the current P1/P2 order.
10. Carry forward Last Origin's directed prerequisite/next separation, stage-to-group join, reward-reference split, and preview drift; retain Princess Connect only as a weak objective-separation boundary.
11. Carry forward Stella Sora only as a community structural/negative source: never accept client-supplied stage ID as outcome proof, require atomic progress/inventory/receipt settlement, and make duplicate settlement replay return the stored receipt.
12. Continue bounded cross-game comparison through Ark's curated read-first summaries, per-game rollups, and source maps; do not reopen the billion-row master index or broad raw payloads without a decision-specific gap. P1-A and P1-B are closed while historical AMEND/FAIL cutoffs remain distinct. Keep the near-term product order `authoring-ledger contract freeze -> minimal P1-C`; retain the first-pocket measurement, then treat the short second stage as a non-binding candidate until its later contract gate and defer delivery/comparison until accepted P2-B. Formal foreign-evidence promotion remains a separate lane. Every incomplete product promotion must reference `CONTINUOUS-P0-NONREGRESSION` and `CONTINUOUS-CURRENT-BUILD-PERF`, close `ACC-OPS-BUILD-MANIFEST-CURRENT` against that exact candidate, and preserve the separate `ACC-OPS-ANDROID-PERF-CURRENT` boundary rather than treating a historical cutoff as proof of later edits.
13. Preserve the reproduced bounded PGR-four-row plus HI3-10101 candidate, all three audited supporting packages, the factual license-signal audit, and the nine-candidate cumulative manifest as the formal promotion lane. Direct SMB access is now available and must use Ark's curated read-first/source-map surfaces before reopening broad raw payloads. Continue the non-copying internal PGR/HI3 gap pass even while the narrow packet remains `0/9`; obtain a packet-specific disposition and atomic eleven-source `LiveAcceptance` only before promoting those exact rows into product-contract evidence. Do not reuse old artifact IDs, union snapshots, substitute either historical control, treat a license-file signal as permission, copy source payload, or promote any candidate/helper early. The queued P1-C/P1-D packets remain governed by their product prerequisites, not by mere archive readability.
14. Preserve the later read-disabled peer order exactly as `P1E-WW-TUTORIAL-ATTEMPT-RESET-01 -> P2A-WW-ENEMY-LAYERING-01 -> P2B-GENSHIN-PRESENTATION-AUTHORING-01 -> P2C-R1999-RESULT-PROGRESSION-CLIENT-ORDER-01`. Do not open any packet before exact-source admission and its predecessor; never infer runtime ownership from static authoring or server durability from client order.
15. Re-score the matrix after each bounded slice using measured implementation cost and observed regressions. Retain P1-B at `13` as of `SNAP-P1B-ANCHOR-PROFILE-HYGIENE-03` unless measured axes change; leave joint P2-A5/P2-B6 unscored until every axis exists; and rescore the bounded P3 repeat-content slice at admission instead of inheriting the historical broad-shell `-3`.
16. Keep the second-stage preflight at product-ready candidate count `0`. Before the first comparable P1-C mutation, jointly freeze `AUTHORING-LEDGER-01`, `THROUGHPUT-COMPARISON-01`, the matched scope fingerprint, endpoint/exclusions, and material threshold. After accepted P1-C/P2-B, choose exactly one seed in the current order `S1-1.BreakGate -> S1-3.TankRescue -> S1-2.BacklineSignal`, or record why a later evidence change alters that order; keep `S1-4.HealPocket` held and reject `S1-5.BossStand` as the first slice. Freeze its content promise, route/condition contract, catalog-generation transition, and physical-host strategy before editing. Do not let a catalog alias, extra Olympus segment, renamed/difficulty-only Station encounter, retrospective estimate, or two unmatched ledgers satisfy content truth or throughput.
