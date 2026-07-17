# P1 Product Decision Packet

## Current P1-B closure

- P1-B Station Add and full-exit closure (2026-07-16): `SNAP-P1B-STATION-ADD-AUTHORING-REMEDIATION3-ACCEPTED-11` binds `C:\tmp\DimensionBrawl-P1B-StationAdd-Remediation3-Bundle.md` at SHA-256 `9378bc021b09495c350b331a85755eac7b956a2372d78ecca848a94c2d570c76`; source `128/128` matches digest `4c3dbe952bea5e4f5c57632d70e6fba815d7f6900dc9e1dcbee6af69bae86c89`, artifacts `11/11` match digest `eb5699917083d9be13d571f2a64aa0f69048304552b962df3467b89f3469ce2b`, validator/inventory `8/4/1/1/0`, integrated focused `8/8`, Canonical UI `34/34`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `99/99` all pass with three independent audits at blocker `0`. Revision-1 pose remains relative to `StageDefinitionSceneBinding.transform`; Station `MapRoot` is topology containment only. `ACC-P1B-STATION-ADD-AUTHORING = PASS`; the foreign-evidence row remains PASS through explicit rejection only; `SNAP-P1B-FULL-EXIT-ACCEPTED-12` closes `ACC-P1B-FULL-EXIT-AUDIT = PASS`, so P1-B is **ACCEPTED / VERIFIED-COMPLETE**. This admits no P1-C runtime owner: only the prospective authoring-ledger freeze may start, and runtime work remains gated by `ACC-OPS-AUTHORING-LEDGER-CONTRACT-FROZEN`.

## Status

- P1-B result/progression Remediation3 acceptance: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION3-ACCEPTED-08` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation3-Bundle.md` at SHA-256 `94fa969979bdb2a2b91dfbdf8a5395aed0a69ddd8907831bb7c99da06b139a5b`; source `116/116` matches digest `271793a22e2afc24779a3aeeace7cb9768aae77b7bbbf18a075fa15ea409efb2`, artifacts `14/14` match list digest `c3642305e13c085f710e8db62df807463aea58d8a57331cd7526460eb7a404fc`, validator/inventory `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `98/98` all pass. Independent source, artifact/test, and semantic-contract audits find blocker `0`: route/sidecar-owned canonical catalog identity is independent of the result definition, public Corridor admission and the editor validator require exact object identity, and catalog-only plus coherent catalog/profile/localization clones reject before run creation. Frozen route/policy/join/lifetime digests remain unchanged. `ACC-P1B-RESULT-PROGRESSION-JOINS = PASS / VERIFIED PARTIAL`; Candidate-07 remains immutable historical FAIL. Station count-one Add authoring is now unheld as the next separate P1-B gate, while live PGR/HI3 disposition, P1-B full exit, and P1-C execution remain OPEN and no P1-D/P2-C owner is admitted.
- P1-B result/progression Remediation2 candidate audit: `SNAP-P1B-RESULT-PROGRESSION-JOINS-REV3B-REMEDIATION2-CANDIDATE-07` binds `C:\tmp\DimensionBrawl-P1B-ResultProgression-Remediation2-Bundle.md` at SHA-256 `a4e2e2873ec4f53ba81a6c6a3269949b4b2f19255f566d333fcb058e3eeb6de8`; its submitted source manifest matches `116/116` with digest `f4c6f0a6065a2f304acd1a56f7d126b4b2be49582f752f707757d87f37c35583`, all `14/14` artifacts match list digest `96176b861dc7ce0a9aaccd86fe035aa59433513383713132248e51f974b6228a`, validator/inventory is `8/4/1/1/0`, focused `7/7`, Canonical UI `33/33`, exact full route `1/1`, and graphics aggregate `98/98` pass. Independent source/contract/test audits verify that Candidate-06's three blocker groups, locale/graph rows, and exact durable-decision byte preservation are closed, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / VERIFIED-FAILED-CANDIDATE-PARTIAL` on one remaining admission defect: the result definition self-selects its catalog, so a catalog-only clone or coherent catalog/profile/localization clone can evade the intended exact-identity gate. The post-bundle route-owned catalog-anchor WIP changes five submitted files and cannot retroactively amend this cutoff. Station Add and P1-B full exit remain held until a new sealed-source candidate passes.
- P1-B result/progression joint-freeze: `P1B-RESULT-PROGRESSION-JOINS-01` Rev3B proposal artifacts match SHA-256 `b6e63b11e3e270302dc33f95b7b69740565e4e27a13ffe017a17f2899256c88f` / `eb65cf30eb961a271f135bc38a9874cccae49e47d8a9d0af5a6dd5f0d7211199` / `933c13943e5397f5fa7a1be531ae34bd28f595e09feee14f18429daa81a8e603`. Fresh PowerShell, independent Node, and a third row reconstruction preserve the seven `15/35/15/17/8/9/38` blocks, sidecar/join snapshot digest `a2ae9df451bd6f2ff48b83098db3bfbdaf2120e23dfaf3612a31f18a022c41fa`, all predecessor digests, and the separate 11-row lifetime-contract digest `3b6cf33325a0a83db74ee2253da9799e589b5664f4fb677b2b021389b0714c0e`. Exact `(ID, revision)` edge resolution and the no-token `Stage Select A -> pre-admission mutation B -> fresh Corridor B` boundary pass. Verdict is **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**. This authorizes implementation only: `ACC-P1B-RESULT-PROGRESSION-JOINS`, Station Add, foreign evidence, and P1-B full exit remain **OPEN**, and no P1-C/P1-D/P2-C owner or P1-A digest change is admitted.
- P1-B result/progression Rev3B implementation candidate audit: `C:\tmp\DimensionBrawl-P1B-ResultProgression-Implementation-Bundle.md` matches SHA-256 `35b1b1a5523bc457ad1936190d1d41143dd1bc8a3489624cdb600631c3a6daa1`; submitted source manifest `116/116` matches digest `1b3dba021b40a4be9d728c6fd4f2039864abb399bbff6d2907e4af274bec24ec`, all `14/14` declared artifacts match list digest `249da60824d3ef617937e648e1257b1fde9b50dc28082a904b78513ca7c76023`, both contract verifiers pass, validator/inventory is `8/4/1/1/0`, focused `2/2`, Canonical UI `28/28`, exact Victory-and-Replay full route `1/1`, and graphics aggregate `93/93` pass. These green artifacts are verified, but `ACC-P1B-RESULT-PROGRESSION-JOINS = FAIL / SOURCE-CONTRACT-FAILED-CANDIDATE`: canonical profile/localization object identity is not enforced at admission, the `Presented -> terminal action` path omits the exact pinned join/presentation/audit authority gate and audit self-integrity, and representative deep snapshot damage can throw instead of returning a typed rejection. Direct clone/damage/dispatch, recovery/process-loss, locale, and production graph acceptance rows remain open. The Rev3B joint freeze and every accepted predecessor cutoff/digest remain unchanged; Station Add and P1-B full exit stay held pending remediation and a new sealed-source bundle.
- Drafted: 2026-07-14
- Status: D1-D3/D4a approved; D4b/P1-0 frozen; P1-A **PASS/CLOSED**; P1-B **ACCEPTED / VERIFIED-COMPLETE** at Station Add Remediation3 plus the explicit foreign-packet rejection disposition and full-exit audit. Historical failed candidates remain immutable. P1-C runtime is not admitted; the prospective authoring-ledger freeze is next.
- Historical configured source root: `\\DESKTOP-69817L3\ArkData\SubcultureGameData` (currently unavailable); bounded reproducibility mirror: `C:/Ark/SubcultureGameData`. Neither location is packet-admission authority without the exact source/provenance cohort.
- Local baseline: current DimensionBrawl workspace, including uncommitted stabilization and optimization work
- Production gate: accepted P0 baseline is PASS at 28/28. D4b scoped coverage, canonical route/endpoints, mutation inventory `8/4/1`, authorized core `1`, bypass `0`, and P1-0 aggregate 37/37 close P1-0/D4b. P1-A's historical 45/49/54/59/68/75 cutoffs remain separate non-additive evidence. The final current-schema cutoff matches all 11 source hashes under ordered manifest digest `e59884ca0bcbec0506502ccb2638d9227e5f098bfb7f271e3a7adf16a2656427`; Combat 21/21 in 0.2007977s, StageRun 23/23 in 90.5924556s, canonical UI 15/15 in 80.8148816s, graphics aggregate 79/79 in 278.751148s with class counts 15/21/3/1/16/23 and 79 unique full names, exact canonical full route 1/1 in 27.6556836s, and compile/validator PASS. Every accepted cutoff preserves the frozen route/policy digests and inventory `8/4/1`, authorized `1`, bypass `0`.
- Fact-slice scope: the current revision-1 Olympus route seals explicit guide state, route-owned total/combat clocks and optional forward-risk duration, the seven-row Corridor `LegacyOpaque/NoResultExpected` tutorial summary before SingleLoad, two segment results, resolved damage/down/dodge/summon facts, source-qualified semantic proofs, and a closed `StageOutcomeFact`; the immutable result-summary digest covers that payload. The exactly-one Station collector, its authored references, and fact DTOs without `UnityEngine.Object` are validator/test-backed. The earlier 45/45 cutoff used the thin `StageRunOwnerCoverageRecord` and remains historical evidence for that payload only.
- Finalization scope: the current route/run schema-1 `NonCourseStationTerminal` path now validates the exact scene-reference-free epoch evidence, seals `TerminalEpochClosureRecord` and `TerminalFinalizationAuthority` after winning `TerminalWon`, then seals the fixed four-row `TerminalFinalizationOwnerCoverageRecord` with truthful `NotAdmitted` rows and zero pending before `OutcomeFactsSealed`. Stored result decisions/receipts are schema 2 and compare the exact complete coverage ID/digest with `NotRequired` preparation; the old thin production types are removed, and cache-clear reconciliation no longer depends on a live Station coordinator. Future schemas cannot reuse the current NotAdmitted-only factory.
- Current-schema exit boundary: 79/79 preserves every earlier closure and directly proves exact encounter-bound abort replay with null/foreign rejection, truthful pre-Station replacement plus exact Station coordinator cancellation/root/epoch preservation, exact registered diagnostic provenance, and transactional final-snapshot/evidence exception conversion to one typed fault/abort with no summary or result UI. Independent source audit found no remaining current-schema production blocker, so P1-A is **CLOSED**. Current fixed owner rows remain truthful `NotAdmitted`/zero-pending and do not prove future admitted-owner quiescence.
- P1-B direct-presentation cutoff: the first accepted 11-source manifest matches 11/11 under digest `38ea238f58adbc49bbf6f0ac7c1ffd846bc4bc5c549fc0dbfcf542f88af72990`; intentional missing-arm validation fails, final validation passes, focused identity is 1/1, natural ActualPlayPath is 2/2, the exact full route is 1/1, and the graphics aggregate is 80/80. This verifies the Corridor-entry definition/profile/combined-Timeline/port/director/existing-flow identity join without changing the frozen route/policy digests or reopening P1-A.
- P1-B presentation-residue cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1-B-PresentationResidue-Bundle.md` matches SHA-256 `6098a3fc32e74990fbabb9ddfe5b1a6b951a4b5aba07557e2273da94558a907c`, and its 11-source manifest matches 11/11 under digest `bbde61085d0801886ec33c1741561c7262449ef666246727ecd63109b14b753f`. Two intentional negatives fail at the missing binding/port boundaries; the final validator passes with 39 current bindings and zero null, stale, or unresolved rows; two unowned `StageCutscenePort` components are removed while their payload GameObjects are retained; focused identity is 1/1, natural ActualPlayPath is 2/2, the exact `FullRoute-2` lane is 1/1, and the graphics aggregate is 80/80. `FullRoute-1` ran zero tests and is explicitly excluded. This closes only the P1-B port-inventory and current-Timeline-binding-integrity subgates. At this historical second cutoff the six extra scene anchors and two dangling `requiresStageDefinition` profiles remained open; only the separate third cutoff below closes them. Canonical catalog/briefing/template, result/progression, Station Add, foreign-evidence disposition, and the full-exit audit remain open.
- P1-B anchor/profile hygiene cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1-B-AnchorProfile-Hygiene-Bundle.md` matches SHA-256 `1a88295b46c43658c964589c554d8286c40ae2f132036204c5fb6fd7b1e7e8e7`; its ordered 23-entry source/absence manifest matches 19 present hashes plus 4 exact absences under digest `7116d6430ce78b11d5e5f1553559e36c0f3e2372febdd8f96b1636ca22e7cd84`. Three intentional negatives reject Corridor 10-versus-4 anchors and the zero-resolve `boss-entrance`/`combat-start` profiles. The final validator passes with Corridor definition/binding 4/4, Station 0/0, exact ID/group/authored-pose and remaining-profile stage-context checks, unchanged `8/4/1/1/0` mutation inventory, and frozen route/policy digests. Accepted `Focused-2` is 1/1, ActualPath is 2/2, exact Replay full route is 1/1, and the graphics aggregate is 80/80 with 80 unique names; `Focused-1` is excluded because it incorrectly treated Timeline-driven runtime pose as immutable authoring state. Six unowned components are absent while payload GameObjects/Transforms remain, and the two zero-production-reference dangling profiles plus meta files are absent. This closes only `ACC-P1B-PRESENTATION-ANCHOR-PROFILE-DISPOSITION`; perceptual quality, catalog/template/briefing, result/progression, Station Add, foreign evidence, and full exit remain open. Any later UI/catalog WIP requires its own manifest and cannot be mixed into this cutoff.
- P1-B canonical-selection rejected candidate: bundle `C:\tmp\DimensionBrawl-P1-B-CatalogProjection-Bundle.md` matches SHA-256 `078208742bf4033b40543f032bf2a0012c2b2aa52063be438baacadefbf51771`; all 19 sources reproduce manifest `83efd18b0658a501f7472556493431be68b453bb97dc94b79e3af7f0d3616183`, projection digest `3a2d630f34c6518b8783bcffaf4ac0c21be1a97cbff8e80372b26bec3537549c` reconstructs, and the three negatives, validator, 6/6, UI 19/19, exact `FullRoute-2` 1/1, and graphics aggregate 84/84 all match. It is not a fourth accepted cutoff: the submitted prefab has no bound authored reward Text row to preserve/hide, and the submitted `SelectStage(null/empty/whitespace)` retains its prior projection/latch by returning before invalidation. `ACC-P1B-CANONICAL-SELECTION = fail` for this snapshot. Later remediation WIP changes five manifest sources and requires its own bundle; P1-A and the three accepted P1-B cutoffs are unchanged.
- P1-B canonical-selection accepted remediation cutoff: the independently audited Candidate-05 bundle `C:\tmp\DimensionBrawl-P1-B-CatalogProjection-Remediation-Bundle.md` matches SHA-256 `2b71350f7e16c54503e03a64c13cb9a04fff3aea3b9fe05799168db1ddabf8b6`; all 19 submitted sources reproduce manifest `05da141460d851ffaaf9a5d1a52fbab9932c2a0d2c1b252e8c4b43b0e2a01dfa` while the frozen projection digest remains `3a2d630f34c6518b8783bcffaf4ac0c21be1a97cbff8e80372b26bec3537549c`. The product prefab preserves and binds the exact inactive, empty `CurrentChapterRewardText` row; null, empty, whitespace, and unknown public selections invalidate the prior projection/latch first and directly prove disabled Start plus zero router request, event, non-null start SFX, run, admission, and abort side effects. The final validator passes with inventory `8/4/1/1/0` and unchanged route/policy digests; focused 8/8 in 0.6209749s, Canonical UI 21/21 in 81.6142512s, exact VictoryAndReplay full route 1/1 in 27.6428807s, and graphics aggregate 86/86 in 278.9029475s with 86 unique names and class counts `21/21/3/2/16/23` all pass with zero failed, inconclusive, or skipped cases. `ACC-P1B-CANONICAL-SELECTION = pass` for `SNAP-P1B-CATALOG-SELECTION-CANDIDATE-05`; Candidate-04 remains an immutable historical failure. At that cutoff the reference/template/briefing joint freeze was open; rev2A later froze that contract, and the separate truthful-join implementation cutoff below now passes while P1-B full exit remains open.
- Historical first P1-B truthful-joins proposal audit: `C:\tmp\DimensionBrawl-P1-B-TruthfulJoins-Contract-Proposal.md` matches SHA-256 `e5305d04937991e7120bb5edc8cd61905c4df923c689adc923c3df65fca9fe5d` and is independently classified **AMEND / PROPOSAL ONLY**. It is neither accepted nor frozen and grants no implementation authority. Its amended IDs/copy and digest-construction requirements became input to the later rev2/rev2A candidates; the accepted rev2A disposition is recorded in the following row. Candidate-04 remains failed, Candidate-05 remains passed, and P1-B full exit remains **OPEN**.
- P1-B truthful-joins rev2/rev2A contract audit: the historical 71/27/78 rev2 submission remains **AMEND / NOT FROZEN** under proposal/digest/verifier/Node hashes `491d72ec79260201d3d23cb203cf043ceb566487d839792c13466488ab69235d` / `2f36e21782bb8eb79253840f2328629160c80c8fa416688929cb76f25321efe9` / `760bc5d7fa666827a80a71324a9a28916896a3e8b39f5f4c468f073ac2d93673` / `473a42e8196c9740b56e17cb720c72958361ef3ffb584d910c025cf495fa46e0` because its briefing omitted the typed pre-result active-run-restart absence. The separately audited rev2A replacements match `21f7cbda4fe767ec7c2b29cd7d24cf00af6432b0bee858216c8a318d3b4f678f` / `1c60f44ef70a6e8ff7dc1595e7f7fa50951535ca7b93dd4f01a36b4282543be9` / `d85b7d3d3d83ad98b7fbd768ce13ba522d00a5797a9bdcd7dc16a77d7b767cda` / `aef9bffcf077979a8d10692ecd2b2b4ea05eeddd0d3a3230e965309b638ada06` and independently reconstruct template 71 rows digest `3eec8a5f94c4dfd47ae9255a49ff3b5961d5130cf386f2c6ba96b0525c502e55`, reference 27 rows digest `b93e1e23845983c3abdb2e13f551e66025942e40ddfde1a2b123054a65db0791`, and briefing 80 rows digest `71b17e4c39364da14aa1deb0906b87eb88ed44e1242723a3b5b76064f2a89f60`. The two inserted ordered briefing rows are `briefing.activeRunRestartPolicyDisposition=3` and `briefing.activeRunRestartPolicyDigest=<EMPTY>` immediately after story-exit and before action count. `P1B-TRUTHFUL-JOINS-01` rev2A therefore remains **ACCEPT / JOINT-FROZEN / IMPLEMENTATION-ADMITTED**.
- P1-B truthful-joins implementation cutoff: the independently audited bundle `C:\tmp\DimensionBrawl-P1B-TruthfulJoins-Implementation-Bundle.md` matches SHA-256 `8ef3a8e234f53ef561dfdd5d805d0f69c8ddbb55d2a2534ca427f2da821a9d0a`; all 51 ordered source hashes match manifest digest `1d2fc6a142fa7582e76095c8a928ca1f61f4453ac7061f5d50525673d1480324`, and all 13 declared artifacts match. PowerShell and Node independently reconstruct `71/27/80`; the validator passes `8/4/1/1/0`; focused 7/7, canonical UI 26/26, exact full route 1/1, and graphics aggregate 91/91 pass with 91 unique full names and class counts `26/21/3/2/16/23`. Frozen route, policy, projection, template, reference, and briefing digests all match. `ACC-P1B-TRUTHFUL-JOINS` is **PASS / VERIFIED PARTIAL**; P1-B full exit remains **OPEN**. At its later historical cutoff, Candidate-06 fails `ACC-P1B-RESULT-PROGRESSION-JOINS` on three blocker groups. Remediation2 Candidate-07 subsequently closes those groups but still fails one independent canonical-catalog identity anchor; a new sealed-source candidate is next, then Station Add, live PGR/HI3 foreign evidence, and the full-exit audit. This cutoff adds no P1-C execution owner, result/progression/reward join or owner, or pre-result active-run restart.
- P1-B foreign-evidence candidate: the deterministic retained-mirror report `_Game/DesignDocs/P1B_PGR_HI3_STAGE_SPINE_RAW_CANDIDATE.json` hashes to `04ebf0a5be6db2535730088b3b7bcd7b6a50c48844292a43e1f9070418efed3d` with canonical digest `f305cc6fdde04fa8b7a2e755b3995e62b297fa9bc08eac73550c00c3056d9b2d`. It reproduces exact PGR GuideFight IDs `100001`-`100004` and HI3 Global `10101` as 70 cells, including the added PGR `100004/10010005`, two loadout-to-null drifts, unchanged shared PGR stage identities, and exact HI3 reconciliation to the 2021 control. The separate correction audit hashes to `5240701338c92f3395ec3bc4716dd1f953637038382a4b557cf6f7d16fbebdda` and records the supporting cohort as `0/9` admitted: seven exact paths absent and two exact paths present but provenance-incomplete. This is candidate evidence only: no raw/helper source enters packet `inScopeSourceIds`; active report/claim mappings/`crosswalkRows` remain empty; all three live acceptance rows remain OPEN; and no product decision, implementation authority, foreign parity, or runtime claim changes.
- Roadmap source: [Subculture Dataset Gap Roadmap](SUBCULTURE_DATASET_GAP_ROADMAP.md)
- Contract companions: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md) and [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md)

This packet records the approved D1-D3/D4a choices and separately frozen D4b revision-1 contract. P1-0 is complete, and the final manifest-bound 79/79 audit closes P1-A for the current schema. P1-B has three separately immutable accepted presentation cutoffs, the rejected Candidate-04 catalog cutoff, accepted Candidate-05 canonical selection, historical proposal/rev2 AMEND records, the rev2A truthful-join freeze and accepted implementation, plus the separate Remediation3 result/progression acceptance. P1-B remains open: Station Add readiness, live PGR/HI3 foreign-evidence disposition, and the full-exit audit are not complete. Current typed absences are not future-owner success, and this cutoff adds no P1-C execution, durable progress/reward, or pre-result active-run-restart owner.

## P1-B Truthful-Joins Proposal Audit — AMEND / PROPOSAL ONLY

The proposal's contract ID `P1B-TRUTHFUL-JOINS-01` is approved as the identifier for a future joint-freeze candidate, not as an implemented or frozen contract. Its exact current-route identity proposal is amended to:

| Field | Required joint-freeze candidate value |
|---|---|
| template ID | `olympus-invasion.tutorial-station-run` |
| segment 0 | `olympus-invasion.corridor-tutorial` -> route segment `corridor_intro_tutorial` |
| segment 1 | `olympus-invasion.station-guide-combat` -> route segment `station_entry_combat` |
| Corridor pocket | `olympus-invasion.corridor.core-tutorial` |
| Station guide pocket | `olympus-invasion.station.replica-summon-guide` |
| Station encounter pocket | `olympus-invasion.station.boss-encounter` |
| existing Corridor source plan | retain `olympus.corridor.core-tutorial` as source semantic identity, not as the new pocket ID |

The approved authored Korean copy candidate is exact and must not be inferred from legacy threat/summon/reward rows:

- title: `기억의 회랑`
- objective: `하층 세계에서 발생한 차원의 미세한 균열.` followed by one actual LF and `그 징후의 진원지를 조사하라.`
- combat lesson: `회랑에서 근접 공격, 이동, 원거리 전환과 사격, 회피, 표적 정리를 차례로 익힌다. 정거장에서는 레플리카 지급과 소환 안내를 확인한 뒤 보스 격파를 목표로 한다.`

The candidate revisions are `referenceSchemaVersion=1`, `referenceRevision=1`, `templateSchemaVersion=1`, `templateRevision=1`, `briefingSchemaVersion=1`, and the additionally required `briefingRevision=1`. The reference digest row set must expand from 24 to 27 by inserting the following story-entry identities after the proposed cinematic-sequence row and before the trigger/completion rows:

```text
reference.storyEntryExpectedPortId=intro-gatepod-port
reference.storyEntryStageAnchorId=IntroCutscene_End_PlayerHandoffAnchor
reference.storyEntryStageRuntimeStateId=state-intro-handoff
```

The canonical encoder is the existing `key=valueLength:value\n` form: `valueLength` is C# `String.Length` in UTF-16 code units; null becomes empty; booleans are `1`/`0`; enum values use their explicit ordinal; integers use invariant formatting; values are not trimmed or Unicode-normalized; rows end with a final LF; and the complete UTF-8 payload is SHA-256 hashed to lowercase 64-hex. Reference, template, and briefing digests are separate. The briefing digest covers its provenance and content but excludes `canonicalBriefingDigest` itself, so construction order is template -> reference -> briefing.

This proposal is not freeze-ready because it does not yet enumerate the exact ordered template row set, does not enumerate the exact ordered briefing row set, and does not provide the final `canonicalTemplateDigest`, `canonicalReferenceDigest`, or `canonicalBriefingDigest`. Each `ExistingSceneOwner / P1CNotAdmitted` statement must also be represented by two typed digest fields rather than one composite label: `currentExecutionOwnerDisposition=ExistingSceneOwner` and `p1cAdmissionDisposition=NotAdmitted`.

The already frozen revision-1 catalog projection digest remains `3a2d630f34c6518b8783bcffaf4ac0c21be1a97cbff8e80372b26bec3537549c`; truthful-join fields do not retroactively enter that digest. The selected Start latch must nevertheless bind one exact projection instance plus catalog generation and the projection/reference/template/briefing digests, and any stale or mismatched member must fail closed before route request, event, SFX, or admission.

The three amended pockets are sufficient to describe the current truthful route topology; no speculative fourth Add pocket is admitted. `olympus-invasion.station.boss-encounter` is only a stable later target candidate. Closing Station count-1 Add authoring remains a separate gate requiring an exact Station segment/template segment/pocket, stable non-placeholder payload target, `SpawnKind.Add`, count 1, finite delay, static/live anchor and group agreement, binding-root-local expected pose, `UsageKind.CombatSpawn`, exact position ID, and non-interference with boss/result/cutscene ownership. It authorizes neither a resolver/executor nor P1-C runtime ownership.

## Approved Product Decisions and Frozen Engineering Contract

| Decision | Approved/frozen value | Why this is the smallest coherent choice |
|---|---|---|
| D1 — logical stage and route identity | approve `OLYMPUS-INVASION-01`, revision `1`, ordered segments `corridor_intro_tutorial -> station_entry_combat`, and the three typed terminal actions below | the playable operation spans two scenes; no current UI ID, scene name, or scene-segment definition truthfully owns the whole route |
| D2 — result action availability | offer Replay plus Lobby after Clear, and Retry plus Lobby after Fail; defer Stage Select and Next Stage | both outcomes need re-entry and escape, but clear replay and failed-run retry must remain distinct intents for later progression/reward policy |
| D3 — first Fail presentation | promote the additive product result UI into one outcome-aware shared result shell; Fail uses Retry primary and Lobby secondary | one committed-summary consumer avoids another result owner while preserving a visible, explainable failure state |
| D4a — simultaneous terminal product policy | use authoritative causal-event order rather than callback arrival; if boss death and player down are both valid in the same causal terminal event, Clear wins and the player-down fact remains; an already closed lower independent player-terminal event is not reopened | boss defeat is the current canonical Station clear trigger while survival remains a truthful result/mastery fact; no coordinator/token implementation is approved by this row |
| D4b — arbitration engineering contract | freeze revision-1 pre-mutation admission, root ordering/tokens, synchronous work drain, and two-subject finalization under the exact policy digest below | the complete mutation-path inventory, bypass-zero validator, bounded/scoped/full-route tests, and route-frozen 37/37 aggregate passed before this technical contract authorized P1-A implementation |

These are local decisions; external datasets supplied comparison boundaries, not authority for the final values. PGR and HI3 support separating static identities and authoring fields, while selected client materials support separating result, retry, and exit ownership. D1-D3/D4a were product-approved by the master's 2026-07-14 directive and follow-up instruction. D4b was deliberately excluded from product approval, then separately frozen as a technical contract on 2026-07-15 only after its stated feasibility and regression gates passed.

## Approval State Versus Execution Gate

| State | May happen now? | Meaning |
|---|---:|---|
| recommendation ready | complete | the four bounded product choices and evidence limits were reviewed |
| product review/approval | complete for D1-D3/D4a; D4b is a separate technical freeze | the exact product values are approved and the engineering mechanism is evidence-frozen without conflating those two decisions |
| P1-0 route-shell authoring | complete | the approved identity/action fields and both physical definitions exist on the single final route shell |
| P1-0 implementation freeze | complete | route/Station validation, D4b inventory and bypass-zero, exact revision/digests, and the P1-0 freeze-point graphics-enabled aggregate all pass; later P1-A evidence is tracked separately through historical fact 45/45, finalization 49/49, post-49 54/54, post-54 adversarial 59/59, exit-candidate 68/68, and remediation 75/75 snapshots |
| P1-0 phase exit | yes | P1-A consumes the immutable route contract; historical 68/75 cutoffs remain bounded evidence and the final 79/79 unchanged-source audit closes the current-schema exit. P1-B is unblocked |

“Approved product decision,” “strict P0 PASS,” and “implemented contract freeze” remain different states. All three relevant gates are independently evidenced: P0 remains the baseline, the route/D4b contract is frozen, and P1-A has started from that immutable input.

## Current Local Facts

| Fact | Current evidence | Decision consequence |
|---|---|---|
| the selected catalog row now selects the runtime combat route | accepted Candidate-05 keeps one presentation ID, directly projects `OLYMPUS-INVASION-01`, preserves one exact inactive authored reward row, invalidates every invalid public selection before an old bundle/latch can survive, and dispatches Combat through one cached route projection only after router acceptance | `ACC-P1B-CANONICAL-SELECTION` passes without making the catalog ID a run/admission owner; Candidate-04 remains the historical failed cutoff and truthful template/briefing/result/progression joins remain separate |
| the route spans Corridor and Station | the frozen route shell composes validated Corridor and Station definitions; P1-A1 now validates and seals the handoff, and the Corridor flow loads the snapshot dispatch destination instead of a forward scene constant | P1-B enriches static joins and catalog projection without creating another runtime destination authority |
| Clear and Fail share one committed-summary surface | `OlympusStationCombatResultPresenter` consumes the exact coordinated `TerminalClosed` record; additive `UI_StageClear` fail-closes without its committed summary and configures Clear/Fail from outcome-filtered actions; the summary now carries the verified current-route fact payload | retain one outcome owner; later finalization/presentation work must extend this seam rather than reopen raw `Won/Failed` UI ownership |
| Station has one product combat-session surface and one typed result executor | the legacy Review result owner is retired; `CombatSessionOverlayPresenter` owns in-combat pause/settings/failure, while P1-A2 commits the fact-bearing result, yields the HUD, and lets the shared shell invoke one route/run-owned typed action CAS | preserve the accepted competing/stale/resolver/load-failure, adapter-loss, digest, destination, closure-fault, cancellation, subject, unload, lifecycle, exact duplicate/replacement/diagnostic, and snapshot-exception boundaries without duplicating result or navigation ownership |
| the Lobby action is truthfully typed | `StageClearScreenPresenter` dispatches `olympus-invasion.to-lobby`; the route/run owner resolves it through the serialized `DB_UIRouteTable` resolver to `UI_Lobby` | preserve this Lobby meaning rather than inventing a nonexistent next-stage action, retain the covered resolver-rejection/load-failure boundary, and add only the remaining exit/unload/closure cases |
| accepted baseline terminal outcome was callback-order dependent | in legacy mode the first `Died` handler immediately changes `CombatEncounterController` from Running and suppresses the other handler; Corridor intentionally retains this mode | the accepted P0 baseline did not implement D4a, so it remains evidence for navigation rather than simultaneous-terminal semantics |
| the Station D4b coordinator implements the frozen technical boundary | Station opts into coordinated terminal resolution; bound `CombatHealth.TryApplyDamage` delegates into the coordinator, while bound reset/reconfigure attempts are rejected and faulted. `DamageInfo` still carries no public root token and `Died` remains synchronous inside an authorized queued mutation | scoped 14/14, full-route 1/1, terminal buttons 2/2, exact inventory `8/4/1` plus authorized core `1` and bypass `0`, route/Station validator PASS, and route-frozen aggregate 37/37 close the D4b gate |

The older 2026-07-14 11:10 full-route and 10:38 natural reports remain historical and are not gate evidence. The accepted pre-D4b aggregate passed 28/28. The first D4b aggregate regressed to 34/37; its initialization and test-only bound-reset conflicts were repaired without weakening the Station bypass rule. The later headless route-frozen run reached 31/37 only because `RenderTexture.Create` is unavailable under `-nographics` and is excluded from acceptance. The graphics-enabled route-frozen aggregate then passed 37/37, so the freeze record uses that XML together with the dedicated validator, not the environmental failure.

## Evidence Strength and Boundary

Grades in this packet mean:

- **A — local executable or directly inspected runtime structure:** sufficient to describe the current gap, but not automatically a product preference.
- **B — source-linked static/client structure:** sufficient to support separation of contracts or authoring fields, not shipped runtime behavior.
- **C — analogous vocabulary only:** useful as a rejection or validation checklist.
- **Missing:** the preserved material cannot decide the policy.

Only the local DimensionBrawl evidence and the registered PGR/HI3 candidate/control records currently have exact entries in the evidence index. The PGR/HI3 `B` rows below describe bounded candidate-supported static observations; they are not active packet admission because the supporting cohort remains `0/9` admitted. Other peer rows are retained as historical section-only comparison context and are not assigned a reproducible-source letter grade until exact source/claim records are admitted.

| Source | Directly supports | Grade | Does not support |
|---|---|---:|---|
| DimensionBrawl current code/assets | the historical two-scene/terminal/navigation drift; current snapshot dispatch, epoch closure/finalization authority/current-schema owner coverage, schema-2 durable result/shared shell/endpoints, the non-additive 45/49/54/59/68/75 evidence, and the final 79/79 current-schema exit closure; later P1-B and progress/settlement gaps remain | A | external market convention or player preference |
| PGR stage/GuideFight material | stage identity separate from display/loadout/story hooks; pre/next, result/reward, and policy-like reboot field presence as static authoring concerns | B candidate / packet OPEN | decoded reboot consumption, route revision, multi-scene ownership, button execution, new-run semantics, Fail action set, or double terminal |
| HI3 stage/result material | one stage identity joining entry/Lua, challenge, prerequisite, and restriction authoring; score/reward/display fields and lose-description references kept separately | B candidate / packet OPEN | per-run result facts, authoritative outcome commit, result UI order, retry/exit action, multi-scene route, or double terminal |
| Reverse: 1999 client-flow narrative | historical section-only note about result recording before end-fight publication and separation of result/progress/bonus categories | Registry-unverified | server durability, atomic settlement, receipt behavior, or result button policy |
| Neural Cloud client-flow narrative | historical section-only note about result, temporary failure-result UI, reward display/claim, retry availability, restart, and exit as separate concerns in an older decompiled regional client snapshot | Registry-unverified | decompiler correctness, final shipped failure UI, server idempotency, free retry, exact target, or DimensionBrawl button availability |
| Ash Echoes static-data narrative | historical section-only note about fail-retry and hidden-result policy fields separate from directed stage links | Registry-unverified | actual click execution, retry cost, save transaction, or result ownership |
| Wuthering Waves tutorial narrative | historical section-only note about success/failure/skip/break plus authored in-dungeon reset and cleanup-related fields as attempt-time concerns | Registry-unverified | executed cleanup behavior, post-result Retry, or terminal navigation |
| FGO quest-policy narrative | historical section-only note about `afterClear: repeatLast` as repeat-after-clear policy distinct from failure retry | Registry-unverified | failure Retry, automatic Next, Lobby, or client result flow |
| bounded peer narrative for this decision | no registered raw simultaneous player/boss terminal trace, authoritative epoch, or tie policy supports this decision | Registry-unverified / Missing | Clear-wins, Fail-wins, draw, timing window, epoch, or callback precedence |

Evidence discipline:

- PGR policy-like reboot field presence supports keeping retry semantics explicit in local authoring; without decoded consumption it does not prove a shipped retry policy or choose Corridor, Station, cost, or availability.
- HI3 lose-description references and score/reward/display tables support separate failure copy and result-related authoring/display fields; they do not prove per-run facts or a shared/dedicated result screen.
- FGO `repeatLast` is not failure retry, and `enableFollowQuest` is not automatic Next navigation.
- Wuthering in-dungeon reset is not post-result retry.
- Limbus post-battle hooks are not failure, retry, Next, or Lobby actions.
- No bounded peer source inspected here is promoted into evidence for simultaneous-terminal precedence.

## D1 — Logical Stage and Route Identity

### Approved product freeze

| Concern | Approved value | Ownership rule |
|---|---|---|
| logical playable stage ID | `OLYMPUS-INVASION-01` | belongs only to the final `PlayableStageDefinition` route shell |
| route revision | `1` | changes when ordered route, action, or terminal-resolution semantics change |
| Corridor segment ID | `corridor_intro_tutorial` | references the existing Corridor scene-segment definition |
| Corridor definition ID | retain `OLYMPUS-CORRIDOR-INTRO-COMBAT-01` | remains a physical scene-segment definition, not the product stage ID |
| Station segment ID | `station_entry_combat` | covers the Station entry guide and canonical encounter |
| Station definition ID | author `OLYMPUS-STATION-COMBAT-01` | owns Station scene path/binding and later Station-local anchors/spawns/ports |
| failed-run retry action | `olympus-invasion.retry`, kind `Retry`, target `OLYMPUS-INVASION-01`, entry at Corridor, allowed only for Fail | a new run starts only after successful canonical Corridor entry |
| clear replay action | `olympus-invasion.replay`, kind `Replay`, target `OLYMPUS-INVASION-01`, entry at Corridor, allowed only for Clear | keeps manual replay after victory distinct from failure recovery and later repeat/economy policy |
| lobby action | `olympus-invasion.to-lobby`, kind `UIRoute`, target `UIRouteId.Lobby` | navigation only; it is not outcome proof or a post-clear hook |
| terminal outcome semantics | authoritative causal root order; same-root boss death plus player down commits Clear while retaining the player-down fact; a closed lower independent player-terminal root is not reopened; no draw/callback-wins/grace window | this is the approved D4a product value; once authored, it is deep-snapshotted at entry and participates in route revision/digest |
| terminal arbitration mechanism | frozen D4b revision-1 contract: pre-mutation admission, root sequence/token, synchronous queue, two-subject finalization, and coordinator lifecycle | separately technically frozen after exact inventory, bypass-zero, validator, digest, and regression evidence; it is not relabelled as a D4a product decision |

### Revision-1 route-segment authoring contract

The approved D1 identity is realized by these P1-0 technical values:

| Sequence | Segment | Entry | Exit | Handoff |
|---:|---|---|---|---|
| 0 | `corridor_intro_tutorial` | `run.entry.admitted` | `corridor.tutorial.completed` | `SingleLoad` |
| 1 | `station_entry_combat` | `corridor.tutorial.completed` | `station.encounter.terminal` | `ReturnToOwner` |

`run.entry.admitted` requires one atomic immutable-snapshot, route-validator, owner-registration, and first-segment-activation admission and is typed `RunEntrySnapshotValidatedAndFirstSegmentActivated`. `corridor.tutorial.completed` is typed `CorridorTutorialFactsAndClosureSealedForSingleLoad` and is satisfied only by the sealed current-run pre-load handoff, not by a raw completion callback. `station.encounter.terminal` is typed `StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched`: it is the exact current-run coordinated terminal resolution at `TerminalClosed` after queue drain, both-subject finalization, and candidate/final agreement, not `Died`, `Won/Failed`, summary commit, or UI visibility. Corridor `SingleLoad` freezes `successor = NextOrderedSegment`, `destination = SuccessorStageDefinitionScene`, `transitionToken = SealedCurrentRunSegmentHandoff`, `loaderGeneration = ActiveRunRouteLoaderGeneration`, `navigationAuthority = P1AStageRunRouteOwner`, and both Return fields as typed `None`. Station `ReturnToOwner` freezes successor, destination, transition token, loader generation, and navigation authority as typed `None`, retains Station as host, and returns the exact terminal record to `P1AStageRunRouteOwner` under `ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented`. The committed-result owner may then open the separate additive `UI_StageClear` presentation; a later typed action alone owns navigation/unload. These values are now frozen in route revision `1` and its validated digest. Any revision-1 condition-meaning change requires a new condition ID and route revision/digest.

The final minimal `PlayableStageDefinition` is frozen once in P1-0 with route revision `1`, route digest `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9`, policy revision `1`, and policy digest `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`. P1-B fills optional template, result, progression, briefing, and cinematic joins on that same asset. It must not create a parallel identity record or reinterpret an active run from a later asset version.

### Options considered

| Option | Benefit | Cost/risk | Verdict |
|---|---|---|---|
| approve the proposed logical identity | gives entry, handoff, result, retry, and later progression one product key | requires route-shell authoring and migration from constants | **Approved** |
| reuse Corridor `StageDefinitionProfile.stageId` | no new ID | falsely claims a two-scene product stage is one physical segment and leaves Station ownerless | Reject |
| reuse `story_v1_training_route` as the logical stage ID | appears close to selection UI | the D1-era catalog had two aliased raw-scene rows; accepted Candidate-05 keeps this string only as presentation identity and projects the separate `OLYMPUS-INVASION-01` route, preserving the identity separation that Candidate-04 had structured but not accepted | Reject |
| reuse scene name or `UIRouteId.Combat` | familiar existing value | scene identity and UI-screen domain cannot express route revision or two ordered segments | Reject |
| defer all identity decisions until P1-B | avoids an early approval | P1-A would be forced back to constants or an interim record, creating migration debt | Reject |

Approval of D1 froze contract vocabulary; P1-0 authored and validated the route shell. P1-A1 validates and seals route-snapshot/context admission; P1-A2 maps Clear to typed Replay, Fail to typed Retry, and both to typed Lobby. Actual Clear Replay/Lobby and Fail Retry/Lobby pass. Canonical UI covers missing summary, replaced-run stale summary, resolver rejection, scene-load failure, duplicate/competing selection, direct route/result/current-schema/coverage/receipt integrity, actual loader completion semantics, and both `ClosureIntegrity`/`UnexpectedSceneExit` terminal-action fault arms. The remaining exit gap is the pre-result product abort/unload lifecycle, not result-action destination binding.

## D2 — Outcome-Filtered Terminal Actions

### Approved first action matrix

| Typed action | Clear | Fail | First-slice presentation guidance | Reason |
|---|---:|---:|---|---|
| `olympus-invasion.replay` | allow | do not allow | Clear: `다시 하기` | manual replay after Clear starts a new run at Corridor without becoming failure recovery |
| `olympus-invasion.retry` | do not allow | allow | Fail: `재도전` | failed-run retry starts a new run at Corridor without inheriting clear-only policy |
| `olympus-invasion.to-lobby` | allow | allow | `로비` | both outcomes need a non-destructive escape path; the action is navigation, not completion proof |
| Stage Select | do not author | do not author | none | Candidate-05 accepts the corrected pre-run catalog projection only; no typed post-result Stage Select target exists, so terminal actions remain Replay/Retry/Lobby until a separately frozen canonical target and parity tests exist |
| Next Stage | do not author | do not author | none | no next playable-stage contract exists; Lobby must not be mislabeled as Next |
| pre-result active-run restart | not a result action | not a result action | later policy | P2-A authors `activeRunRestartPolicy`; P1-A alone owns request validation, latch, sealed dispatch, later diagnostic abort, and actual dispatch; P2-B supplies requests/closure receipts only |

Action presence never implies availability. Under approved D2, Replay serializes `allowedOutcomes = { Clear }`, Retry serializes `allowedOutcomes = { Fail }`, and Lobby serializes `allowedOutcomes = { Clear, Fail }`. The committed summary projects only matching action IDs, and the presenter never adds a button because a route target happens to exist.

Labels, visual priority, and order are not `StageRouteActionRef` semantics. The shared result view owns a `ResultActionPresentation` mapping keyed by `(outcome, actionId)` with `labelKey`, `role`, and `displayOrder`; it may not enable an action absent from the committed summary. P1-A's first view profile supplies the mapping above, and P1-B's result-definition join references that same profile rather than copying it. Missing or duplicate presentation entries disable the affected control and fail validation. Copy/order changes do not change route revision, while action ID/target/availability changes do.

Required invariants:

1. Replay after Clear and Retry after Fail both dispose the old presented context and create a new run only at canonical Corridor entry, but preserve their distinct action ID/kind.
2. Lobby after either outcome disposes the old context and dispatches only the sealed `UIRouteId.Lobby` payload.
3. Fail cannot dispatch post-clear story, progression success, mastery success, or rewards merely because Lobby is allowed.
4. Double-click, Replay/Retry/Lobby race, stale revision/digest, missing summary, and load failure cannot select a second action.
5. The review HUD either exposes no independent result controls on the canonical route or delegates the same summary and executor.

Revision 1 Replay and Retry are manual, no-entry-cost result actions. “Immediate failed-run retry” means the committed Fail surface offers Retry without a progression/reward write or extra cost gate; it does not auto-navigate or bypass summary commit. A future automatic repeat, entry cost, fast clear, or reward-altering repeat policy must use the existing clear-only Replay identity plus a versioned policy, or replace it with a new typed action under a new route revision; it never silently overloads failed-run Retry.

Why Lobby is also offered after Fail: forcing Retry creates a trap, while immediate scene exit without a committed Fail hides useful cause and run facts. A typed Lobby action gives the player control without pretending the run cleared. The peer datasets support separating exit/retry concerns but do not choose this policy; this is a local usability and ownership decision.

## D3 — First Fail Presentation

### Options

| Option | Player effect | Implementation cost | Regression/ownership risk | Verdict |
|---|---|---:|---:|---|
| outcome-aware shared additive result shell | visible reason, truthful facts, Retry/Lobby choice, consistent interaction | medium | medium-low if it consumes only committed summaries | **Approved** |
| dedicated Fail scene/surface | maximum visual separation | medium-high | high: another scene, presenter, route binding, and parity matrix | Hold until content proves need |
| immediate Retry after Fail | fastest loop | low initially | high: hides cause, removes choice, and can create a run before result disposal is proven | Reject |
| 3D fail marker only | preserves current minimum | low | high player-flow gap: no product action surface or committed-summary proof | Reject |
| retain the review HUD as Fail owner | reuses existing copy | low initially | critical: second encounter/result owner and Station-reload Retry conflict | Reject |

### Approved first slice

The existing additive product result UI becomes an outcome-aware shell. Its current scene/class names may migrate later; they are not contract identity. It opens only after `RunResultSummary` reaches Committed.

For Clear, the shell may keep clear-specific art/audio and renders total/combat time plus the first two truthful summon-identity proofs. For Fail, it must use distinct title, accent, audio policy, and copy; clear BGM, success copy, reward reveal, mastery success, and post-clear hooks remain absent.

Minimum Fail projection:

- title: explicit operation failure/player-down language;
- reason: typed failure reason rendered as copy, never parsed back into logic;
- facts: combat time, player-down state, and one truthful next-attempt hint derived from committed facts or authored failure copy;
- actions: the Fail `ResultActionPresentation` projects Retry primary and Lobby secondary from the two offered action IDs;
- safety: missing/invalid committed summary produces diagnostic-safe copy with both actions disabled.

The product shell can share layout and executor while using outcome-specific presentation assets. Shared shell does not mean identical Clear/Fail audiovisual treatment.

## D4 — Same-Epoch Player/Boss Terminal Arbitration

### D4a — Approved product-policy freeze

Freeze only these player-visible semantics at product review:

- authoritative causal root order, never callback/subscriber arrival, decides which independent terminal event resolves first;
- when boss death and player down are both valid final states inside one same-root terminal epoch, commit Clear and retain the player-down fact;
- a player-only terminal result already closed in a lower independent root is not reopened by a later boss death; and
- no draw outcome, callback-wins policy, or frame/millisecond grace window exists in revision 1.

This approves the outcome policy, not the coordinator class, token shape, queue algorithm, or proof that every current mutation path can use them.

### D4b — Frozen revision-1 technical contract

The frozen mechanism is one `SameTerminalResolutionEpoch` window owned by the authoritative Station `EncounterTerminalResolutionCoordinator`:

1. A canonical combat producer must call `CanonicalCombatRootAdmission` before any Player/Boss terminal-state mutation and before any `Damaged`, `Died`, or terminal-observer callback can run. The coordinator assigns a unique monotonic `RootAdmissionSequence`; a damage callback, terminal callback, presenter, or fact collector may never create a root admission.
2. Lower `RootAdmissionSequence` is the revision-1 authoritative causal order. Independent roots are intentionally not simultaneous: the lower sequence resolves completely first even if both were admitted during one rendered frame. Reversing the root sequence may therefore change Clear versus Fail by design; reversing callbacks while preserving the sequence may not.
3. When the next admission becomes active, the coordinator issues one `RootResolutionToken` and one `EncounterTerminalEpoch`. Later independent admissions remain ordered pending records without mutation authority; they receive a token only if the run remains active after every lower sequence closes.
4. Every mutation capable of changing a bound `{ Player, Boss }` subject's current/max health, alive/down/dead state, or terminal candidate must execute through the active queue and token. This includes damage and, if a canonical path exposes them, heal, reset, reconfigure, revive, or forced-death operations. Initialization before terminal-subject binding is outside the window; an unsupported reset/revive after binding faults the run rather than reviving old result state.
5. Same-root mutation or reaction work created while the active queue drains receives a deterministic intra-root sequence and remains in the epoch. The root producer and every queued handler are synchronous and non-yielding; they may enqueue only through the active context before returning and may not retain a token for a coroutine, task, later frame, or unrelated callback.
6. The coordinator lifecycle is `Idle -> Open -> Draining -> Finalizing -> EpochClosed`, with `Faulted` and `Cancelled` exits from any active substate. `Open` runs the admitted root producer; `Draining` begins after it returns and consumes same-token work; when no handler is executing and the queue is empty, enqueue is structurally sealed and the coordinator enters `Finalizing`.
7. `Finalizing` performs one synchronous handshake with both bound subject adapters. Each adapter must return exactly one token/epoch-matching final health/down snapshot even when that subject was untouched. Missing, disabled, rebound, duplicate, throwing, or asynchronous adapters fault instead of leaving the coordinator waiting.
8. At `QueueDrainedAndSubjectsFinalized`, the arbiter validates candidate/final-state agreement and resolves at most once, then seals the per-root record as `EpochClosed`. A nonterminal close invalidates the token and follows `EpochClosed -> Idle -> Open(next)` when a pending admission exists, or remains `Idle` when none exists. Clear/Fail invalidates every pending admission and first reaches `EpochClosed -> TerminalClosed`; that contender must atomically win the shared terminal-or-restart latch, seal `TerminalFinalizationAuthority`, and enter `TerminalFinalizing`. Only that winner gathers deterministic final facts, seals course traversal/quiescence, requires P1-C `RunFinalization`, and closes the current presentation-adapter generation. P1-A then seals those fixed owner rows in `TerminalFinalizationOwnerCoverageRecord`, enters `TerminalFinalizationOwnersSealed`, and only afterward enters `OutcomeFactsSealed`; admitted mastery and P2-A variability closure then complete. A schema that requires durable progress/settlement preparation must reach exact `PreparationPrepared` before P1-A may enter `CommitRequested`; `NotRequired` enters commit directly.
9. Work exceptions, direct mutation bypass, current-run token/epoch/order mismatch, adapter loss, or snapshot failure enter `Faulted`; scene unload, explicit run abort, or coordinator disposal enter `Cancelled`. Either path atomically invalidates active and pending current-run authority, discards queued work, seals at most one diagnostic abort while the run is active, and publishes no product summary.
10. `Time.frameCount`, `FixedUpdate` count, rendered frames, elapsed milliseconds, health-callback arrival, and subscriber order are not valid substitutes for admission, root sequence, token, epoch, or the synchronous close barrier.

D4a approval freezes only the product semantics above. D4b is separately frozen by `DimensionBrawl-P1-0-RouteValidator-Final.log`: all inventoried Station paths match the exact allowlist (`damageProducers=8`, `resetCallers=4`, `configureMaxCallers=1`, `authorizedCoreCallers=1`, `bypass=0`), the route and scene contracts validate, and the graphics-enabled route-frozen aggregate passes 37/37. The frozen policy is `olympus-invasion.same-terminal-epoch`, semantic revision `1`, digest `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`; it joins route revision `1`, digest `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9`. P1-A1 deep-copies this exact contract into `StageRunRouteSnapshot`; the existing Station D4b binding retains scene-local subject/coordinator ownership. On a terminal path the coordinator enters `TerminalClosed`, seals immutable epoch evidence before `Resolved` publication/route-owner handoff, and P1-A validates that evidence into `TerminalEpochClosureRecord`, `TerminalFinalizationAuthority`, and the current-schema `TerminalFinalizationOwnerCoverageRecord`. Stored decision/receipt schema 2 compares that exact coverage ID/digest. No owner reads mutable latest policy after entry, and the current NotAdmitted-only coverage factory rejects future route schemas. Any technical-policy or condition-meaning change requires a new condition ID where applicable plus route revision and canonical digest change. Future loss of exclusive coverage or synchronous closure fails validation closed and invalidates a double-terminal-support claim; it does not rewrite the historical D1-D3/D4a approvals.

### Token and coordinator state handling

| Observed authority | Required handling |
|---|---|
| `ActiveCurrent` token with matching run/root/epoch | accept only through the active synchronous queue; any malformed current-run authority faults the active run |
| `IdleCurrent` canonical root admission | assign the next sequence and open it immediately when no lower pending admission exists |
| `DeferredCurrent` root admission | keep an ordered pending record only; it has no token and cannot mutate until promoted |
| `ClosedSameRun` token while the run is still active | reject before mutation, enter `Faulted`, and seal the one current-run diagnostic abort |
| `WrongRun` or foreign generation | reject and log without mutation; never abort an unrelated active run |
| `PostTerminal` token after `PreparationRequested`, during preparation/commit recovery or persistence quarantine, after `CommitRequested`, `Committed`, `Presented`, or `Disposed` | reject and log only; do not alter the immutable candidate/summary, reopen preparation/commit, or create a second abort |
| coordinator already `Faulted` or `Cancelled` | reject and log only; no queued work, second abort, or product summary may appear |

### Truth table

| Boss candidate | Boss final dead | Player candidate | Player final down | Resolution |
|---:|---:|---:|---:|---|
| no | no | no | no | close epoch with no product commit; run remains active |
| yes | yes | no | no | commit Clear with `BossTerminal` |
| no | no | yes | yes | commit Fail with `PlayerTerminal` plus typed failure reason |
| yes | yes | yes | yes | commit Clear with `SimultaneousTerminalClear` and player-down fact retained |
| any candidate/final mismatch | — | — | — | seal `InvalidTerminalEvidence` diagnostic abort; no product summary |

A candidate/final mismatch includes candidate-with-live-final-state, final-terminal-state-without-the-same-epoch candidate, wrong current run/segment/root/epoch/order authority, direct terminal-state mutation outside the coordinator, or a close attempt before the queue/subjects finalize. Wrong-run and post-terminal authority instead follow the reject/log-only rows above; neither can mutate or reinterpret a result. The arbiter never guesses Clear or Fail from whichever callback arrived first.

For simultaneous Clear, the summary still records the resolved lethal damage and player down. A later no-down mastery objective therefore fails even though the stage clears. The result should retain an enum-equivalent diagnostic such as `SimultaneousTerminalClear` so QA and presentation do not have to infer the tie from callback order.

### Why Clear-wins was approved

- The current canonical Station runtime clear trigger is boss death; no current stage rule says survival is mandatory for clear. The older Corridor definition's broader description is not used as evidence for this Station tie policy.
- Survival can remain a separate result/mastery fact, preserving truthful cost without erasing the achieved boss defeat.
- A dramatic final trade is less likely to feel arbitrarily reversed in the short second-round demo.
- The rule is narrow: a boss death in a higher independent root sequence after a committed player-only lower sequence does not retroactively clear the run.

| Alternative | Consequence | Verdict |
|---|---|---|
| Fail-wins | makes survival an implicit mandatory clear condition that is not authored anywhere | Reject for revision 1; reconsider only with an explicit stage rule |
| Draw/restart | adds a third product outcome, copy, action policy, persistence semantics, and tests without source or demo need | Reject |
| first callback wins | changes with damage/subscription order and cannot express product intent | Reject |
| arbitrary millisecond grace window | depends on frame rate/time-scale and invites late damage ambiguity | Reject |

Known presentation risk: a simultaneous Clear can open the result while the player avatar is down. The outcome coordinator must freeze combat and presentation from the committed facts; it must not revive/reset the bound subject, accept a post-terminal mutation, or wait for a living-player state. Replay creates a new run, and Lobby exits the disposed one.

## Active-Run Restart Boundary

This packet does not reopen the previously resolved boundary:

- Replay after committed Clear and Retry after committed Fail use their separate P1-0 result actions and the same P1-A terminal executor.
- Restart before an outcome is not Clear, Fail, or a dormant result button.
- P2-A authors `activeRunRestartPolicy`. A source submits a pure request before cleanup; P1-A validates it, wins the latch, enters `RestartClosing`, and seals `ResolvedActiveRunRestartDispatch` first. P1-A then requests every independently admitted P1-E lesson/attempt, P2-B course, P1-C execution, P2-A variability, and P2-B presentation barrier and seals one diagnostic abort only after all results are known. Success alone disposes and dispatches; fault enters `ClosureFaulted` with no dispatch.
- Neither path reuses the other's action, summary, or lifecycle state.

Wuthering's authored in-dungeon reset fields and FGO's post-clear repeat policy reinforce this separation of static concerns, but they do not prove executed cleanup or dictate DimensionBrawl targets and button availability.

## Priority and Dependency Handoff

| Order | Work | What this packet changes |
|---:|---|---|
| P0 | current full/natural route proof, one product terminal surface, actual Retry-to-Corridor and Lobby | complete; strict P0 is PASS and remains the regression baseline |
| P1-0 | final minimal `PlayableStageDefinition`, Station definition, exact mutation inventory, D4b proof, digests, and final regression | complete; the route and D4b revision-1 technical contract are frozen |
| P1-A | route snapshot/context admission, canonical terminal-record consumption, durable exactly-once result decision, current-route fact payload, current-schema terminal-finalization authority/owner coverage, shared result shell, and typed executor | historical cutoffs remain non-additive; final Combat 21/21, StageRun 23/23, UI 15/15, aggregate 79/79, full route 1/1, validator, manifest, and source audit close the current-schema exit |
| P1-B onward | preserve the three accepted presentation cutoffs, rejected Candidate-04, accepted Candidate-05, proposal/rev2 AMEND records, rev2A/truthful-join acceptance, and Remediation3 result/progression acceptance separately; next close Station Add, then live PGR/HI3 foreign evidence and the full-exit audit | no reordering, retroactive evidence mixing, treating the hidden row as reward truth, or inferring P1-C execution, durable progress/reward, or pre-result restart ownership; P1-B remains OPEN |

Do not implement a dedicated Fail scene, Stage Select action, Next action, draw outcome, reward reveal, or active-run restart merely because this packet names their boundaries.

## Acceptance Evidence After Approval

| Scenario | Required proof |
|---|---|
| route identity | one immutable entry snapshot contains the approved logical ID/revision, both ordered segment/definition/scene identities, Replay/Retry/Lobby records, all allowed-outcome sets, the terminal resolution policy, and the canonical digest |
| catalog/route drift | selected catalog projection, Build Settings, current flow request, result targets, and both physical scene bindings either agree with the route shell or validation fails |
| Clear result | committed Clear opens the shared shell once and offers Replay plus Lobby only |
| Fail result | committed Fail opens the distinct Fail projection once, offers Retry plus Lobby only, and dispatches no success hook/progress/reward |
| Replay after Clear | one sealed snapshot-derived Replay dispatch returns to Corridor; successful entry creates a different run ID with reset facts |
| Retry after Fail | one sealed snapshot-derived Retry dispatch returns to Corridor; successful entry creates a different run ID with reset facts |
| Lobby after either outcome | one sealed `UIRouteId.Lobby` dispatch occurs after old-context disposal |
| missing/stale summary | shell may show diagnostic-safe state, but terminal actions stay disabled |
| terminal callback permutation | with fixed root-admission sequence, boss-then-player and player-then-boss candidates in the same epoch and matching final states both commit one Clear with simultaneous-terminal diagnostic and player down recorded |
| independent-root causal order | lower root-admission sequence resolves first; deliberately reversing the authoritative root sequence may change Clear versus Fail, while merely reversing callback/subscriber delivery may not |
| callback root creation | damage, `Died`, terminal-observer, presenter, and collector callbacks cannot mint a new root; an attempt faults the active current run before mutation |
| terminal window boundary | player-only terminal in a closed lower-sequence epoch followed by boss death in a higher sequence remains Fail; no late candidate reopens commit |
| resolution boundary | nested same-root terminal-state work remains in one epoch; an independent admission waits without a token and is processed only in a later epoch, never merged by frame/time proximity |
| synchronous close and cycle | root producer and nested work return, the queue drains, both untouched/touched subjects synchronously snapshot, and the coordinator reaches `EpochClosed` without a frame/task/timer/leaked scope; nonterminal close returns through `Idle` and opens the next pending admission, while terminal close becomes `TerminalClosed` |
| fault/cancel | work exception, adapter loss/rebind, unload, or explicit abort invalidates active/pending authority atomically, publishes one diagnostic abort at most, and never commits a summary |
| token-state matrix | active-current malformed authority aborts the active run; closed-same-run authority faults it; wrong-run and post-terminal authority are reject/log-only; no case mutates an immutable summary or creates a second abort |
| exclusive terminal-state coverage | every canonical Station mutation that can change Player/Boss terminal state carries the active root/epoch token; a direct bypass seals `InvalidTerminalEvidence` abort and no summary |
| review HUD | cannot independently reload Station or publish a second canonical result |

## Approval Record

Approval of D1-D3 or D4a is not contingent on accepting the current D4b mechanism:

| Decision | Recorded state | Frozen product value / remaining gate |
|---|---|---|
| D1 | **Approved — 2026-07-14** | `OLYMPUS-INVASION-01`, revision `1`, ordered `corridor_intro_tutorial -> station_entry_combat`, Corridor-targeted Replay/Retry identities |
| D2 | **Approved — 2026-07-14** | `Clear -> Replay + Lobby`, `Fail -> Retry + Lobby`; Stage Select/Next deferred |
| D3 | **Approved — 2026-07-14** | one outcome-aware shared additive result shell |
| D4a | **Approved — 2026-07-14** | authoritative causal order; same active-root/epoch boss death plus player down resolves Clear and retains player-down; a lower closed independent player-terminal event is not reopened |
| D4b | **Technically frozen — 2026-07-15** | policy `olympus-invasion.same-terminal-epoch` revision `1`, digest `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`; route revision `1`, digest `2b912058cefb5b9ad14ed9d11336e2344dd12efa9789fc2df676a7ac74e821b9` |

Approval basis: the master's source-task instruction registered the complete roadmap and explicitly approved D1-D3/D4a while initially keeping D4b provisional. The later requested gate evidence was then delivered in full and explicitly submitted for D4b freeze/P1-A unblock on 2026-07-15. No prior explicit user instruction in the reviewed task history conflicts with this product-approval and separate technical-freeze split.

P1-A current-schema implementation is complete at the final 79/79 unchanged-source cutoff. The earlier failed or invalid artifacts remain excluded exactly as recorded and do not weaken the final set. Changing D1, D2 action identity/kind/target/availability, D4a semantics, a revision-1 condition meaning, or the frozen D4b contract changes the route digest and requires a new route revision; a condition-meaning change also requires a new condition ID. Changing only D2 label/role/order or D3 presentation is view work if ownership and action semantics remain identical. A D4a or frozen-D4b change also requires contract, result-compatibility, and test review before any saved result or progression exists.

## Evidence Sources

Local sources inspected:

- `Assets/_Game/UI/StageSelect/StageSelectScreenPresenter.cs`
- `Assets/_Game/UI/StageSelect/UIStageCatalog.cs`
- `Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset`
- `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset`
- `Assets/_Game/Scripts/LevelDesign/OlympusCorridorCombatFlowController.cs`
- `Assets/_Game/Scripts/Combat/CombatEncounterController.cs`
- `Assets/_Game/Scripts/LevelDesign/OlympusStationCombatResultPresenter.cs`
- `Assets/_Game/Scripts/LevelDesign/OlympusStageClearOverlay.cs`
- `Assets/_Game/Scripts/UI/StageClear/StageClearScreenPresenter.cs`
- `Assets/_Game/Scenes/UI/UI_StageClear.unity`
- `Assets/_Game/Tests/PlayMode/CombatEncounterResultPlayModeTests.cs`

Historical inspected-path notes under the configured source root:

These paths record prior review context only. They do not prove current bytes or satisfy the nine-source supporting cohort. Current exact raw-candidate and `7 absent / 2 present-but-unadmitted` state is recorded in the evidence index and the foreign-evidence candidate status above.

- `games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices.md`
- `games/punishing-gray-raven/enemies-stages/pgr-tutorial-stage-context-rollup.csv`
- `games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-label-context.csv`
- `games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.md`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv`
- `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/lua/modules/logic/fight/rpc/FightRpc.lua`
- `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/lua/modules/logic/fight/model/FightResultModel.lua`
- `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/lua/modules/logic/dungeon/rpc/DungeonRpc.lua`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/Sector/LevelDetail/UILevelRewards.lua`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/BattleDungeon/UI/UIDungeonResult.lua`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/BattleDungeon/UI/UIDungeonFailureResult_Temp.lua`
- `games/ash-echoes/raw/ash-echoes-gamedata/2026-06-14/files/extracted_repo/GameData-master/data/chapter_levels.dat`
- `games/ash-echoes/raw/ash-echoes-gamedata/2026-06-14/files/extracted_repo/GameData-master/data/level_reward.dat`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/GuideStep.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/GuideGroup.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/ComboTeachingCondition.json`
- `games/fate-grand-order/raw/atlasacademy-api/2026-06-14/files/data/NA/nice_war.json`
- `games/fate-grand-order/raw/atlasacademy-api/2026-06-14/files/data/JP/nice_war.json`
- `games/fate-grand-order/enemies-stages/fgo-war-quest-summary.csv`

Community and extracted materials remain structural evidence only. No proprietary code, IDs, text, tuning, layouts, assets, or audiovisual content should be copied.
