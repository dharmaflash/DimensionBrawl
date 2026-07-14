# Subculture Dataset Gap Roadmap

## Status

- Started: 2026-07-13
- Working source root: `\\DESKTOP-69817L3\ArkData\SubcultureGameData`
- Completed focused pass: Punishing: Gray Raven, Honkai Impact 3rd, Aether Gazer, Blue Archive, Wuthering Waves, Arknights, Ash Echoes, Reverse: 1999, Limbus Company, and Last Origin
- Completed bounded pass: CounterSide, the Girls' Frontline family, Path to Nowhere, Fate/Grand Order, Heaven Burns Red, Epic Seven, Brown Dust 2, Princess Connect! Re:Dive, and Stella Sora
- Completed presentation lifecycle pass: Genshin Impact; Brown Dust 2 retained only as a third-party viewer cleanup boundary; Honkai: Star Rail retained as an insufficient-evidence boundary
- Supporting cross-checks: Zenless Zone Zero, NIKKE, and Girls' Frontline Neural Cloud client-flow material
- Indirect QA-only source: Snowbreak MAA material
- Remaining-archive preflight completed: Stella Sora was audited as community structural/negative transaction evidence; no further dataset enters the deep queue without stronger source material
- Current route snapshot: `OlympusCorridorInvasionStage -> OlympusStationCombatStage -> UI_StageClear` after the 2026-07-13 split-route correction
- Evidence snapshot: the full cross-scene probe passed at 2026-07-14 11:10, but Station was saved again at 11:15:21; the Corridor scene was then saved at 14:21 and its tutorial PlayMode test source at 13:33. The full and 10:38 natural reports are historical rather than current. P0 is therefore `full STALE / natural STALE / retry MISSING / lobby MISSING`, with the Station review HUD's conflicting Station-retry path unresolved; separately, P1-E tutorial cancel/disable/unload coverage is missing
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
- Project baseline: current DimensionBrawl workspace, including uncommitted stabilization work
- This document is a long-term decision ledger. It does not authorize immediate feature expansion.

## Objective

Use source-anchored game datasets to identify which production structures DimensionBrawl is missing, which existing structures should be expanded, and which reference patterns should be rejected because they dilute the summon-first fixed-rear combat identity.

The output is an ordered backlog, not a feature shopping list.

## Current Executive Decision

| Decision | What belongs here now | Why |
|---|---|---|
| Protect and verify | Fixed-rear forward-risk summon combat, real-event tutorial validation, existing AI role/deck/elite layers, action/cinematic camera systems | These are already meaningful strengths; comparison games do not show that DimensionBrawl needs a replacement core |
| Add first | Run-scoped result facts, later mastery evaluation, a canonical stage outcome/reference boundary, shared stage briefing, and one route-to-spawn execution bridge | These are the missing links between the current good combat slice and a repeatable stage game |
| Expand carefully | Tutorial presentation/attempt/loadout/course layers, stage rules, one modifier, enemy runtime variants, and thin story handoffs | Current implementations work but are scene-bound or not reusable across story/practice/challenge contexts |
| Add only after proof | Persistent progression, conditional first/mastery rewards, and one growth action | They become valuable only after stable stage IDs and truthful run results exist |
| Hold | Stamina, random drops, shops, gacha/equipment breadth, roguelike affix graphs, daily/liveops shells, generic score/combo ranking | High scope and identity risk with little current demo or replay value |

Comparison verdict: the main shortfall is not originality or lack of combat systems. It is the production spine that turns the original summon-answer combat into authored stages, truthful results, replayable practice, and safe progression.

## Guardrails

- Stabilization and second-round demo readiness remain ahead of expansion.
- Do not copy proprietary assets, source code, exact formulas, dialogue, stage layouts, UI art, animation, or audiovisual content.
- Extract reusable data shapes, authoring boundaries, pacing relationships, validation rules, and player-facing flow patterns.
- A reference pattern is promoted only when it solves a demonstrated DimensionBrawl gap.
- Preserve DimensionBrawl's fixed-rear boss-barrage, forward-risk energy, and summon-answer identity.
- Do not import PGR signal-orb, three-ping, QTE, or HI3 equipment/gacha structures merely because the datasets expose them.
- Treat community repository data as structural evidence, not authoritative shipped runtime behavior.

## Existing DimensionBrawl Baseline

| Area | Current evidence | Current maturity |
|---|---|---|
| Stage map and runtime wiring | **StageDefinitionProfile** owns identity, scene path, anchors, spawns, cutscene handoffs, and runtime-state references. | Implemented foundation |
| Canonical product route | The current flow single-loads from the Corridor tutorial scene into the Station combat scene; Station gates its entry guide, owns the boss encounter, and `OlympusStationCombatResultPresenter` opens the clear UI on `CombatEncounterController.Won`. | The 2026-07-14 11:10 full probe passed Corridor runtime tutorial inputs through Station guide, boss clear, additive clear UI, and the configured Corridor retry target for that snapshot. It forces intro time, does not execute retry/lobby, and became stale when Station was saved again at 11:15:21; scene handoff also remains partly duplicated in flow constants |
| Linear encounter design | **LinearStageTemplateProfile** and **LinearStageSegmentProfile** own route, pacing, lesson, mastery text, reward hook, segments, and pockets. | Authored data; runtime consumption is intentionally absent |
| Playable tutorial | **OlympusCorridorTutorialDirector** owns a scene-specific sequence from melee and movement through ranged fire, dodge, and target clear. It observes real combat/input events and has staged cue/observation/commit behavior. Normal completion clears several input/presentation/target domains, but cancel does not restore blockers, bounds, target candidates, target pose/health/AI, or source-owned invulnerability, and no exact prior-state restore is proven. | Strong normal-path runtime and PlayMode coverage, but monolithic, scene-bound, and missing terminal/reset parity |
| Cinematic and story handoff | **CinematicSequenceProfile** already owns stage context plus movement/input/HUD lock intent and a `GameplayHandoffCue` with return mode, target, release delay, and HUD/time-scale/camera restore flags. **CinematicSequenceRunner** restores driven camera pose, actor controllers/visibility, fade, and explicitly disabled behaviours on natural completion or `Stop()`. | Capable profile/runner foundation, but most handoff fields are not executed by the generic runner; the current Olympus intro uses scene-specific `PlayableDirector` and flow-controller wiring for skip, cameras/listeners, roots, HUD, and input |
| Stage selection and briefing | **UIStageCatalog.StageEntry** references `StageDefinitionProfile` but also stores its own ID, name, summary, threat tags, recommended summon role, mock reward, and loading-card ID. **ChapterMapPrototypeStageNode** separately stores stage copy, objective, reward, energy cost, lock, and clear flags on UI objects. | Functional/prototype UI data with duplicate stage read models; not progression-state driven. `StageSelectScreenPresenter` now forwards the selected row's scene route/loading card, but both rows alias the same Corridor definition and no canonical playable-stage identity crosses the handoff |
| Stage clear | **StageClearScreenPresenter** exposes Corridor retry and lobby navigation with authored entrance presentation. **CombatSessionOverlayPresenter** is the sole in-combat pause, settings, and failure surface. | Shell only; no run proof, rank, mastery, reward, or persisted clear state. P1-A later replaces direct scene loading with one shared typed action executor. |
| Progression and rewards | **STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md** supplies historical vocabulary, while [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md) now separates P1-D state, pure resolution, conditional buckets, deltas, journal, and receipts. | Provisional analysis contracts only; no matching persistent production owner or payout found |
| Enemy/run composition | Combat role profiles and the canonical Olympus flow cover enemy pressure and boss/summon exchanges. A separate `PveStageData` / `PveEncounterDirector` prototype executes raw trigger-Z groups and placements. | Canonical scene flow plus a noncanonical prototype. Neither joins linear pockets to `StageDefinitionProfile.SpawnRef`; the prototype also lacks the execution-generation, fail-closed spawn, retry/scene-exit cancellation, and owned cleanup contract required by P1-C. |

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

- Aether Gazer has the strongest direct field evidence in this peer pass, but it is still preserved public configuration rather than official runtime code or shipped traces.
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

- Keep the authoritative order `P1-0 approval + minimal PlayableStageDefinition route shell -> P1-A RunResult -> P1-B complete stage spine -> P1-C EncounterGroup -> P1-D Mastery/StageProgressState -> P1-E tutorial extraction -> P2-A stage variants -> P2-B lesson chain/presentation -> P2-C RunRewardPlan/RewardReceipt`.
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

- This is A-grade static-data shape, not shipped execution code. The archive does not prove result-screen order, retry, skip/replay execution, camera/UI/input/time-scale cleanup, or the order and side effects of `story.exit` versus `stageScriptNameAfterClear`. Do not copy IDs, script names, reward values, illustration pivots, or infer automatic next-stage flow from theater row order.

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

- Keep `recommendedNextStageId` separate from `requiredCompletedStageIds`; validate target existence and allowed cycles without requiring reciprocal edges.
- Strengthen P1-C as `playable stage -> ordered wave/group refs -> existing encounter payload/spawn refs`, while keeping group rewards outside combat execution.
- Keep `baseRewardRef`, `firstClearRewardRef`, and `allObjectivesRewardRef` as separate authoring references. A local resolver defines eligibility from committed results and prior progress; source labels do not prove repeat-grant policy.
- Generate reward preview from the authoritative local plan and resolution instead of treating preview/catalog rows as payout owners.
- Keep the current priority order. The data sharpens P1-B/P1-C/P2-C validators but does not replace P1-A result truth or the local receipt/idempotency design.

Princess Connect boundary:

- `games/princess-connect-redive/raw/redive-master-db-diff/2026-06-13/files/extracted_repo/redive_master_db_diff-master/` contains 1,910 SQL and 50 text files, but no runtime code, player prior-state, or server result payload.
- The strongest join is 7,206 event-quest candidates to 5,522 event-mission candidates, with 3,374 mission rows referencing an existing quest ID. This supports a separately referenced `StageObjective/MissionDefinition`, not first/repeat/mastery reward, next-stage unlock, or result execution.
- Most columns are hashed. Reward-like numbers, parent-like references, and mission text cannot be promoted to typed semantics or payout joins without decoded fields and runtime evidence.

Evidence boundary:

- Both sources lack detected reuse licenses. Do not copy game IDs, hashes, text, reward values, drop rates, mission rules, or payloads. Last Origin provides B+-grade static relationships, not grant execution; Princess Connect provides only a conservative quest-to-objective boundary.

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
| Canonical end-to-end demo route still undergoing scene/tutorial stabilization | Must finish before expansion | Current workspace and active Olympus flow work | Any new system raises regression risk before the second-round demo is stable |
| A logical stage spans Corridor and Station single-load scenes without a stage-wide run context | Missing lifecycle boundary | Current `LoadTutorialCombatScene`, Station encounter/result wiring, additive clear UI, and Corridor retry route | Tutorial proof, elapsed time, run ID, and stage identity will reset or drift unless explicitly handed across the scene boundary |
| Stage design data is not consumed by a canonical reusable runtime route executor | Missing production bridge; separate prototype exists | Linear stage docs explicitly exclude runtime spawning; `PveEncounterDirector` consumes its own raw PVE placement model instead of the playable-stage/SpawnRef spine | Authored stage data cannot become repeatable content without bespoke scene code, while blindly adding another executor would create a third spawn authority |
| Tutorial ordering, presentation, allowed input, encounter setup, completion evidence, and exit cleanup share one large scene director | Existing strong feature that should be separated incrementally | Current tutorial director; PGR course/practice catalogs; ZZZ presentation targets; Wuthering success/fail/skip/break/reset fields | New lessons or practice stages require more branching; replacing the evaluator too early risks regressions in working input-lock, normal-path cleanup, and event proof while cancel/disable/unload restoration is still incomplete |
| No executable `BasicLesson -> FreePractice -> SummonMasteryChallenge` chain exists | Missing P2-B composition and lifecycle boundary | Station guide is cue-only; Boss Barrage guide is not product-bound; no Free Practice entry/exit/baseline owner exists; PGR separates Tutorial/Challenge nodes and practice follow-ups | The summon-first identity cannot be taught, rehearsed without judgment, then truthfully tested; prematurely wiring the current guide would mistake acknowledgement/local booleans for proof and carry dirty gameplay state into Challenge |
| Stage identity, route design, typed prerequisites, progression metadata, and chapter-map UI are separate islands | Missing canonical playable-stage contract | StageDefinitionProfile, LinearStageTemplateProfile, ChapterMapPrototypeStageNode, Arknights stage-to-level graph separation, Ash Echoes explicit pre/post/map references, and Last Origin's independently directed prerequisite/next links | Duplicate strings and manual wiring can drift across scene, UI, execution, result, and unlock flow |
| The current two-scene stage route is duplicated and its older stage definition still describes a same-scene handoff | Confirmed contract drift | `LoadTutorialCombatScene` constants, build-readiness Station constant, UI route docs, and `DB_Stage_OlympusCorridorIntroCombat` purpose/scene fields | Validators can pass scene presence while stage identity, handoff intent, retry, and result ownership still disagree |
| Stage select projects raw aliased scene data instead of a canonical playable-stage identity | P1-B hard drift | `StageSelectScreenPresenter` forwards the selected row's scene name/path/loading card, but both current catalog rows alias one Corridor definition and the router receives no logical playable-stage ID/revision | Distinct-looking catalog authoring can still launch the same physical route and cannot anchor result/progression truth |
| Intro handoff static references disagree with the actually played presentation | P1-B hard drift | stage definition references the base intro profile/Timeline; scene director plays `_OlympusBombingPrelude`; combined profile shares `intro-to-stage`; generic runner profile is null | String foreign keys can resolve while runtime plays a different sequence and lifecycle policy |
| Mastery and clear-condition intent is mostly stored as strings rather than typed condition plus parameters | Missing evaluator contract | HI3 `StageChallengeData`, GF2 and Ash Echoes challenge/evaluator separation, Path to Nowhere visibility/order boundary, and current `masteryObjective`, `clearCondition`, and cue strings | UI copy can claim a condition that runtime never measured, or different stages can interpret the same text differently |
| Stage briefing fields are split across level data, stage-select catalogs, scene logic, and UI | Existing data that should gain one read model | HI3 stage row joins and current project profile/catalog split | Stage card, loading/briefing, runtime, and result can disagree about objective, route, or recommendation |
| Clear UI does not receive structured run proof | Missing result contract | StageClearScreenPresenter and existing reward research | The player cannot see why a summon-first run was tactically successful |
| No persistent stage unlock/clear/mastery state was found | Missing progression state | Repo search and prototype-only clear flags | Stage select cannot become a real chapter loop |
| Enemy roles are not yet a general variant/configuration matrix | Existing role system that should be expanded | HI3 monster AI/stat/config separation; Wuthering identity/growth/behavior/skill references; Arknights and GF1/GF2 stage-local composition/placement overrides | Reusing enemies across story, tutorial, and challenge modes risks prefab or scene duplication |
| Stage conditions and mode modifiers are not first-class data | Missing extension | HI3 stage condition/buff rows; Path to Nowhere display-only buff records | New stage rules would tend to become one-off scene scripts or infer executable behavior from presentation prose |
| Ordered encounter groups/waves do not bridge route intent to concrete spawns | Missing canonical production bridge | Aether topology/waves, ZZZ floor/group/member placement, Arknights ordered level actions, GF2 stage/group/placement joins, Last Origin stage-to-wave/mob-group references, and the isolated local PVE prototype | A new bridge could duplicate `LinearStagePocket`, `StageDefinitionProfile.SpawnRef`, or raw PVE placement fields unless one stage-local binding and one runtime owner are fixed first |
| Tutorial completion evidence and mask/highlight/prompt/media presentation are not reusable layers | Existing feature that should be separated | PGR lesson catalogs, ZZZ guide-target/media rows, and GF2 section/group finish-condition separation | New lessons can duplicate overlay logic or bind directly to brittle scene paths |
| Input, loadout, revive/retry, and cleanup restrictions are not one stage-rule contract | Missing extension | PGR restrictions and Aether stage rules | Restrictions can leak past success, skip, retry, or scene handoff and recreate soft locks |
| Camera/cinematic systems exist, but transition cleanup is not one route-wide executable contract | Validation and adapter gap, not a missing camera system | Aether set/reset lifecycle; Wuthering flow acquire/close patterns; Genshin show/close and skip/fade separation; Brown Dust 2 third-party viewer generation-token/unmount failure checks; `CinematicSequenceRunner.Stop()` restoration; Olympus flow/tests | A visually correct path can still leave stale input, HUD, camera/DOF, time scale, listener, actor visibility, disabled behaviours, or late async callbacks on skip/cancel/retry/scene exit |
| Stage entry/exit story handoffs are not a thin data link to the existing cinematic system | Missing integration, not missing presentation capability | HI3 stage-to-plot/dialog links; Genshin pre/perform/next/finish joins; Limbus role-labelled before/after story references; `StageDefinitionProfile.CutsceneHandoffRef`; `CinematicSequenceProfile` stage/handoff fields; usage audit | No production consumer was found joining the local handoff-ID surfaces. The profile records return mode/target/input delay/HUD/time-scale restore intent, but the generic runner mostly marks handoff reached and consumes camera restore; bespoke scene wiring still owns actual skip and gameplay release |
| Reward/growth contracts exist only as research and provisional review artifacts | Planned, not implemented | STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md; [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md); Last Origin's separate base/first/all-objective authoring references and drift-prone preview boundary | Result and replay motivation cannot mature beyond a shell, and a preview catalog could become a false payout owner |
| Run facts, prior progress, progression delta, reward eligibility/payload, granted receipt, and retry/reset now have separate review boundaries but no runtime owners | Missing later transaction implementation | P1-A/P1-D/P2-C review contracts; Blue Archive reward buckets; Reverse: 1999 prior-state-before-update client flow; Limbus battle-stage/progression-node separation; NIKKE/EpinelPS duplicate-grant failures; Stella Sora emulator claim-before-inventory ordering risk; Neural Cloud owner separation | Without the reviewed transaction boundary, progress-first mutation, identity conflation, or claim-before-grant persistence can still misclassify, double-award, or permanently lose first-clear/mastery rewards |
| Broad daily/liveops/economy surfaces are absent | Intentionally deferred | PGR/HI3 table inventories | Low impact until the combat-stage-result loop is worth repeating |

## Decision Scoring

This is a working comparison tool, not an ROI claim. Each factor uses 1 (low) to 5 (high):

- `Impact`: value to demo clarity, replayability, or content production.
- `Identity`: reinforcement of fixed-rear, forward-risk, summon-answer combat.
- `Evidence`: strength and cross-game consistency of the source evidence plus current-code fit.
- `Cost`: implementation and content-authoring size.
- `Dependency`: number and depth of prerequisites.
- `Regression`: risk to the current playable route, input ownership, presentation, or persistence.

Working score:

`2 * Impact + 2 * Identity + Evidence - Cost - Dependency - Regression`

Hard gates override score: P0 stabilization, a stable ID boundary, and required predecessors always come first. Scores should be revised after each vertical slice rather than used to justify a large batch.

| Candidate | Impact | Identity | Evidence | Cost | Dependency | Regression | Score | Readiness consequence |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| Cross-scene StageRunContext, result collector, and clear-shell binding | 5 | 5 | 5 | 4 | 3 | 3 | 15 | First structural/player-facing slice after P0 and the shared identity preflight; no reward payout |
| End-to-end transition lifecycle verifier | 5 | 3 | 5 | 2 | 1 | 1 | 17 | P0 support; the latest forced-intro full PASS is stale after the 11:15:21 Station save, the natural PASS is stale, and single-terminal-owner plus actual retry/lobby clicks remain |
| Typed mastery evaluator over immutable run facts | 4 | 5 | 5 | 3 | 3 | 2 | 15 | After result facts exist; persistence is a later step |
| Canonical playable-stage reference spine and briefing read model | 5 | 4 | 5 | 4 | 2 | 3 | 14 | Its minimum ID/route subset is the shared preflight; the full spine is mandatory before another stage or route change |
| Tutorial presentation extraction | 4 | 4 | 5 | 3 | 2 | 4 | 12 | Move first; retain current evaluator/order, and do not claim gameplay-reset coverage from its no-op disposition |
| Three-entry summon lesson course chain | 4 | 5 | 5 | 5 | 5 | 5 | 8 | P2-B after P1-E/P2-A and presentation parity; Free Practice exit/baseline is the hardest missing owner, so prove an isolated fixture before product binding |
| StageRuleSet and one modifier | 4 | 5 | 5 | 3 | 4 | 4 | 12 | P2 despite score because cleanup and stage spine are prerequisites |
| One EncounterGroup route executor | 5 | 4 | 5 | 4 | 4 | 4 | 11 | After playable-stage references are canonical, P1-A accepts the static-plan/gate/quiescence extension, one real count-1 Add fixture exists, and PVE/canonical startup share an atomic scene lease |
| Persistent clear/mastery plus canonical stage-select projection | 4 | 3 | 5 | 3 | 4 | 2 | 10 | After stage IDs and result schema stabilize; chapter-map binding waits for a real typed node fixture |
| Enemy runtime variant adapter | 4 | 4 | 5 | 4 | 4 | 4 | 9 | P2; reuse one enemy in three contexts before scaling |
| Stage-to-story/cinematic handoff adapter | 3 | 3 | 5 | 3 | 3 | 4 | 7 | Integrate existing presentation; do not build a new camera stack |
| Typed tutorial evaluator replacement | 4 | 4 | 3 | 4 | 4 | 5 | 6 | Late P1, one rule at a time with parity gates |
| First/repeat reward plan and one growth sink | 3 | 2 | 5 | 4 | 5 | 3 | 3 | P2 after repeat play and persistence work |
| Broad stamina/shop/liveops/activity shell | 2 | 1 | 5 | 5 | 5 | 4 | -3 | P3 hold |

### Current execution sequence

1. **P0:** retain the 11:10 full-route PASS only as historical snapshot evidence, refresh both the full and natural paths, remove/align the conflicting Station result retry, and execute canonical retry/lobby navigation through the current product surface.
2. **P1-0 shared identity preflight:** [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md) may receive planning approval while P0 is still being closed, but P1-0 authoring remains after P0 in this execution sequence. Obtain explicit approval for one `playableStageId`, `routeRevision`, ordered Corridor/Station segment IDs, typed Replay/Retry/Lobby actions, outcome/action availability, and the full terminal resolution policy. Inventory every canonical Station Player/Boss terminal-state mutation path and fail freeze unless canonical pre-mutation root admission, exclusive synchronous token-queue coverage, two-subject finalization, and cancellation are feasible. Then author the final `PlayableStageDefinition` asset in its minimal route-shell phase with existing Corridor and new Station stage-definition refs; P1-B fills the same asset. These are new contract decisions, not a discovered production manager.
3. **P1-A:** establish one `StageRunContext` across Corridor and Station, introduce authoritative pre-mutation root admission/order plus the synchronous terminal-resolution queue lifecycle, deterministically commit a truthful `RunResultSummary`, and bind two summon-identity proofs plus clear time to the result UI only after commit.
4. **P1-B:** complete canonical playable-stage references, typed prerequisite states, route/cinematic validators, and one shared briefing read model without copying existing profile data.
5. **P1-C:** execute one P1-B-approved segment/pocket through a minimal ordered `EncounterGroup` that references real, non-placeholder spawn IDs; no current pocket is freeze-ready yet.
6. **P1-D:** add typed mastery evaluation, durable clear/mastery application, and one read-only binding to the corrected canonical stage-select entry; defer chapter-map binding until a real typed node fixture exists.
7. **P1-E:** extract Move presentation, then its immutable attempt result, then shadow/promote `MoveDistance`; separately use Fire as the first non-no-op gameplay-reset fixture while retaining the current Fire evaluator. Generalize only one lesson at a time after parity.
8. **P2-A:** follow the [Stage Rule, Modifier, and Enemy Variant Spec](STAGE_RULE_MODIFIER_ENEMY_VARIANT_SPEC.md): freeze the variability snapshot and recommendation first, then one source-scoped restriction, one typed modifier, one enemy with story/practice/challenge variants, and finally active-run restart integration.
9. **P2-B:** follow [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md) for one run-scoped strict-linear `BasicLesson -> FreePractice -> SummonMasteryChallenge` chain, while [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md) remains the separate presentation-ownership contract.
10. **P2-C:** add first-clear/repeat-clear distinction and one growth action only if the stage loop already earns replay.
11. **P3:** broaden courses, challenge modes, dailies, economy, and live operations only from measured demand.

## Detailed Backlog

The `Current execution sequence` above is the authoritative order. The sections below are grouped by related contract rather than execution order; every heading carries its exact phase tag so document position must not be interpreted as priority.

### P0 — Protect the current demo

1. Finish and verify the canonical tutorial-to-combat-to-stage-clear path.
2. Lock a known-good demo build and performance baseline before adding production systems.
3. Preserve the active optimization session's scope; this roadmap remains analysis-only during stabilization.
4. Add no new progression, economy, or broad tutorial framework to the demo branch until the route passes its end-to-end checks.
5. Verify the current split route across normal completion, intro skip, Corridor tutorial completion, Corridor unload, Station entry-guide gates, Station combat, retry, clear-UI additive load, and scene unload.
6. At every handoff, assert the expected scene-local input/movement/camera owner, HUD visibility and interactivity, time scale, BGM, phase, and bounded time to the next state.
7. Retain the 2026-07-14 11:10 `OlympusCorridorCombatFlowPlayModeProbe` PASS as historical evidence for its tested snapshot. It followed the 10:47 tutorial-director write and 10:59 Station save, but the later 11:15:21 Station save makes it stale. Obtain a newer full rerun; the historical probe forced intro time and did not execute either terminal action.
8. Retain the 10:38 natural intro-handoff PASS as historical only because it predates the 10:47 tutorial-director write; obtain a fresh unforced rerun.
9. Resolve the two active terminal route surfaces before executable navigation smoke: the additive clear UI points retry to Corridor, while the enabled Station review HUD reloads the active Station scene. For P0, activate the one retained current product surface to prove a new Corridor load and a separate lobby load; typed action migration belongs to P1-A.

Implementation boundary:

- Reuse the current observable flow and presentation state in editor/PlayMode verification.
- Do not add a second camera or transition framework for this gate.
- Snowbreak-style screen recognition is not needed; inspect authoritative component state directly.

Exit condition:

- One deterministic run from Corridor tutorial entry through the Station entry guide, boss clear, additive result UI, retry-to-Corridor, and next/lobby navigation.
- The Station player and guide are owned by the Station scene, not leaked from the unloaded Corridor scene.
- Exactly one product terminal-result owner resolves Retry/Lobby; review-only overlays cannot present a conflicting result or route.
- No stuck input ownership, leaked movement/joystick lock, unintended tutorial trigger, duplicate clear owner, stale BGM/HUD, or missing scene handoff.

### P1-0 / P1-B / P1-C — Shared identity, stage spine, and encounter bridge

The provisional reference/validator contract for P1-0/P1-B is maintained in [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md). The P1-C execution/lifecycle boundary is maintained in [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md). Both are thin layers over current authorities, not replacement stage databases, and their new IDs are contract proposals rather than values discovered in production.

#### P1-0 — Shared identity preflight

Decision readiness before either P1-A or P1-B production code:

| Concern | Review value | Readiness |
|---|---|---|
| physical route | Corridor -> Station -> additive clear UI | source wiring plus the historical 11:10 full probe support the shape, but current parity is stale after the 11:15:21 Station save |
| logical product ID | `OLYMPUS-INVASION-01` | recommended new contract value derived from existing chapter naming; requires explicit product approval |
| route revision | `1` | recommended new contract value for the first explicit two-segment route; requires approval |
| segment IDs | `corridor_intro_tutorial`, `station_entry_combat` | recommended new contract values; neither is a current production field |
| physical segment refs | existing Corridor `StageDefinitionProfile` plus a new Station `StageDefinitionProfile` with stable ID, `MapScenePath`, and scene binding | P1-0 must fully own this minimum route identity so P1-A can verify handoff and retry without constants or scene strings; P1-B may enrich non-route content only |
| failed-run retry action | `olympus-invasion.retry`, target the same logical stage, allowed for Fail | desired failure recovery, but not freeze-ready until the Station review-HUD re-entry conflict is removed or delegated |
| clear replay action | `olympus-invasion.replay`, target the same logical stage, allowed for Clear | separates manual post-clear replay from failed-run retry and later repeat/economy policy |
| lobby action | `olympus-invasion.to-lobby`, target `UIRouteId.Lobby` | configured target is source-backed and no real next stage exists; actual lobby execution is still MISSING |
| outcome/action availability | recommended `Clear -> Replay + Lobby`, `Fail -> Retry + Lobby`; defer Stage Select/Next | [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md) makes the local product case; explicit approval is still required and action presence never makes a button legal |
| terminal resolution policy | recommended `SameTerminalResolutionEpoch`; owner `EncounterTerminalResolutionCoordinator`; admission `CanonicalCombatRootAdmission`; causal order `RootAdmissionSequence` issued before mutation/callback; active boundary `RootResolutionToken`; subjects `{ Player, Boss }`; coverage `ExclusiveQueuedTerminalStateMutationForBoundSubjects`; work `SynchronousNonYieldingResolution`; same-root nested work stays; independent admissions follow lower sequence into later epochs; lifecycle `Idle/Open/Draining/Finalizing/EpochClosed/TerminalClosed/Faulted/Cancelled` with nonterminal return to `Idle/Open(next)`; authority states `IdleCurrent/ActiveCurrent/DeferredCurrent/ClosedSameRun/WrongRun/PostTerminal`; close `QueueDrainedAndSubjectsFinalized`; simultaneous outcome `Clear` | no bounded peer source decides this policy; the packet derives a local rule from the Station clear trigger, makes lower independent-root sequence an explicit product choice, requires complete mutation/closure inventory, and stores all semantics in the immutable route snapshot |

After approval and the remaining P0 gate, P1-0 creates the same `PlayableStageDefinition` asset that P1-B will complete, not a parallel identity record. Its required route shell contains the approved logical ID/revision, two ordered `StageSceneSegmentRef` entries, the existing Corridor definition, a newly authored Station definition whose stable ID, `MapScenePath`, and scene binding are already valid, typed Replay/Retry/Lobby actions, each action's allowed clear/fail outcomes, and the typed terminal resolution policy. Before freeze it inventories every canonical Station Player/Boss terminal-state mutation path and verifies that pre-mutation admission, exclusive synchronous queue coverage, two-subject finalization, and cancel/fault closure are implementable. At entry P1-A deep-snapshots segment order, stable scene identities, action kind/target/outcome policy, coordinator/admission/root-order/active-boundary semantics, subject roles, coverage, work execution, nested/independent-root rules, epoch, lifecycle/token handling, finalization/barrier, tie/final requirements, and a canonical digest over all of them. It pre-validates the existing hard-coded Corridor-to-Station loader request against that snapshot and dispatches Replay/Retry only from a sealed snapshot-derived payload; it never re-reads the latest asset after context disposal. P1-B replaces the forward loader constant and fills template/result/progression/briefing/cinematic joins on the same asset, and may enrich non-route Station anchors/spawns/ports, but cannot defer or change P1-0's physical scene identity/binding. New P1-B fields apply only to new-schema runs; existing active/committed P1-A snapshots remain immutable and unresolved for those later fields. Documentation agreement alone is not production implementation.

#### P1-B — Canonical playable-stage references

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

For the current split route, P1-0 already requires ordered `StageSceneSegmentRef` entries containing stable segment ID, referenced stage definition/scene, entry condition, exit condition, and handoff policy. The clear UI remains a presentation surface, not a combat scene segment. P1-B adds the remaining joins and validators; once parity is proven, flow code should resolve the Station route from this same contract rather than duplicate scene name/path constants.

The first route validator must fail when the stage contract, UI stage catalog, selected-stage runtime projection, Build Settings route, flow handoff, any active terminal retry destination, stage-definition purpose, or story/cinematic handoff disagrees. It must join each `StageDefinitionProfile.CutsceneHandoffRef` and scene port to one directly referenced `CinematicSequenceProfile`, its handoff/anchor/runtime-state IDs, and the timeline actually used by the runtime consumer rather than merely checking nonempty strings. In particular, an unresolved `nextStageId`, a same-scene handoff description, or the current base-profile reference beside an actually played `_OlympusBombingPrelude` timeline cannot silently coexist with the product route.

#### P1-C — Ordered encounter execution bridge

Minimum route execution bridge:

`PlayableStageDefinition.encounterExecutions[] -> LinearStageSegment / LinearStagePocket -> EncounterGroupSequence -> ordered SpawnRef IDs -> StageDefinitionProfile.SpawnRef -> AnchorRef`

The first `EncounterGroup` needs only one fully identified activation command, ordered spawn references, typed Add-defeat completion, cancellation, owned cleanup, and next-local-group handoff. Cross-game evidence supports richer action vocabulary, but the local `SpawnRef` already owns payload, anchor, count, and delay and has no interval. Revision 1 therefore snapshots the static plan at P1-A run admission, uses an injected scaled gameplay clock, treats each `delaySeconds` as absolute from group activation, preserves serialized spawn-reference order for equal due times, requires the first fixture to be `Add` with `count == 1`, and defers per-unit interval/member overrides. Group and sequence states are separate: intermediate group completion leaves the sequence active, while only the final group can complete the sequence and satisfy one named local gate. The bridge must not duplicate transforms, become a general graph editor, or absorb progression and result logic.

No current canonical fixture is freeze-ready: the current templates do not truthfully match the Corridor/Station route, Corridor Add payloads are placeholders, the boss is cutscene-owned, and Station has no stage definition/binding/spawn set. P1-B must first freeze one exact segment/pocket and author one real Station Add spawn, unique anchor, and stable non-placeholder payload identity tied to a concrete archetype/prefab authoring target without enabling runtime execution. P1-C then owns the typed resolver/factory.

The existing `PveStageData` / `PveEncounterDirector` path is a noncanonical prototype, not evidence that this bridge already exists. It owns duplicate raw placements, sorts by `triggerZ`, has no explicit retry/scene-exit cancellation or owned cleanup API, and currently allows a failed delayed spawn attempt to leave an empty group that clears. P1-C must isolate that owner from the canonical route and may reuse only separately tested lifecycle primitives.

P1-C is complete only when one immutable `EncounterGroupSequence` deterministically resolves its ordered group/spawn references, faults rather than clears on a required spawn failure or unexpected disappearance, proves activation and typed defeat completion, invalidates stale execution generations, cancels pending actions on every terminal dispatch/scene exit, cleans every owned full or partial runtime object/subscription, advances exactly once, satisfies its named local gate, and releases the one atomic scene execution lease before navigation. Detailed validators, lifecycle, acceptance fixtures, and ordered sub-slices are defined in [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md). A generic condition language or event graph is explicitly outside the first slice.

Why before tutorial generalization:

- It converts existing authored work into reusable playable content.
- It reduces scene-specific wiring before the tutorial system is generalized.

### P1-A / P1-D — Add run proof first, then mastery

#### P1-A — Stage-wide facts and truthful result

The provisional contract and acceptance matrix for this slice are maintained in [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md). It remains analysis-only until P0 refreshes both stale route probes, leaves one current product terminal surface, and proves actual retry and lobby clicks. P0 does not require the future committed-outcome coordinator or typed executor; those are P1-A responsibilities.

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

The current project already emits enough signals for a narrow first result contract. The newly verified missing piece is a stage-wide context that preserves stable identity and immutable facts across the Corridor-to-Station single-load boundary; it is not a new scoring or reward system.

| Result fact | Current source | Coverage | Required bridge |
|---|---|---|---|
| Stage identity and run ID across scenes | Corridor `StageDefinitionProfile` identity plus hard-coded Station load route | Missing stage-wide owner | Create a narrow `StageRunContext` or explicit handoff payload; never carry scene-object references across the single-load transition |
| Canonical clear/fail | Station currently collapses the first terminal into `CombatEncounterController.Won/Failed` and `IsWon/IsFailed`; `OlympusStationCombatResultPresenter` opens `OlympusStageClearOverlay` on `Won` | Authoritative single-terminal state, but it suppresses a competing terminal candidate; current damage callers mutate `CombatHealth` directly, `DamageInfo` has no admission/root/epoch authority, `Died` is synchronous, and no authoritative terminal coordinator lifecycle exists | Inventory every canonical bound-subject terminal-state path, then add `CanonicalCombatRootAdmission` before mutation/callback, lower-sequence independent-root causal order, active-token-only synchronous mutation, same-root nested draining, two-subject finalization, explicit nonterminal `EpochClosed -> Idle/Open(next)` cycling plus terminal/fault/cancel closure, and at-most-one resolved request before any result presenter opens |
| Summon-follow-up, pressure suppression, counter recovery proof | `BossBarrageEncounterController` proof fields/events and optional `RouteResultRecord` | Strong but encounter-specific | Store semantic proof IDs/tokens that were actually observed; do not require or invent a route record when the canonical boss-death clear uses a different boundary |
| Player damage taken | `CombatHealth.Damaged` with resolved `DamageInfo.Amount` | Observable | Subscribe only to the run's player health and accumulate resolved hostile damage |
| Player down | `CombatHealth.Died` | Observable | Count or flag downs inside the run boundary |
| Perfect dodges | Both canonical scenes contain `PlayerActionController`; neither contains legacy `PlayerController` | Observable and unambiguous for the current route | Use `PlayerActionController.PerfectDodgeTriggered` as the sole canonical adapter; retain the legacy event only for noncanonical legacy scenes |
| Summon usage and spent tier | `PlayerSummonSlot1Action.SummonSlot1Used`; support-slot `SummonUsed`; per-slot total-use fields | Observable | Normalize slot/role ID, tier, and timestamp without coupling the result contract to Slot 1/2/3 classes |
| Correct summon answer | Boss encounter block/follow-up/counter events; optional `RouteResultRecord` only when it actually commits | Strong for the current reviewed route | Record normalized semantic proof such as `pressure_block` or `counter_recovery`; do not require an undefined route token or infer correctness from any summon use |
| Structure break | Legacy `BattleStructure.OnStructureDestroyed`; elite break state is currently queried from `EnemyElitePatternController` | Fragmented | Add an objective/proof adapter before using this as cross-stage mastery; do not equate boss pressure suppression with a literal structure break |
| Forward-pressure time | `SummonEnergyLadder.CurrentForwardRisk01`, `CurrentRiskBand`, and `RiskBandChanged` | Live state only | Accumulate seconds while the authoritative ladder reports `ForwardRisk`; no duration counter exists yet |
| Tutorial completion | Corridor `OlympusCorridorTutorialDirector.Completed` | Whole tutorial only and destroyed by the Station single-load | Follow [Tutorial Lesson, Attempt, and Gameplay Reset Spec](TUTORIAL_LESSON_ATTEMPT_RESET_SPEC.md): synchronously seal the route summary and any ordered immutable lesson facts before loading Station; retain the current director until parity tests pass |
| Persistent clear/mastery | Prototype UI booleans only | Missing | Persist only after canonical stage ID and result schema are stable |

Existing precedent:

- The legacy `BattleResultUI` already aggregates just dodges, skill casts, structure breaks, and base damage into display text.
- That code proves the event sources are usable, but its counters are UI-owned, scene-specific, not a reusable result object, and not persisted.
- The current canonical route completes the Corridor tutorial, releases movement/joystick ownership, single-loads Station, gates a two-step entry guide, fights the Station boss, then additively loads the clear UI. The additive clear UI's desired canonical retry target is Corridor, while the enabled Station review HUD currently reloads Station.
- Current terminal-capable damage paths call `CombatHealth.TryApplyDamage` directly; `DamageInfo` carries no admission/root/epoch authority and `CombatHealth.Died` fires synchronously. The proposed pre-mutation admission/order and synchronous terminal-resolution lifecycle are therefore new P1-A infrastructure, not existing behavior inferred from the code. P1-0 must inventory the canonical Station path rather than importing noncanonical reset/respawn assumptions from other controllers.
- `OlympusCorridorCombatFlowPlayModeProbe` passed at 2026-07-14 11:10 after the 10:47 tutorial-director write and 10:59 Station save, but the 11:15:21 Station save makes that report historical/stale for the current workspace. It verified runtime tutorial inputs, scene-local player/guide ownership, Station BGM, movement/joystick release, boss HUD, encounter win, clear overlay, additive clear UI, and the configured clear-UI retry target for its snapshot. It forced intro time, did not activate retry or lobby, and did not catch the enabled review HUD's conflicting Station retry.
- Station keeps `BossBarrageLaneReview_PocketOwner` active, but its current `closeThreatHealth` reference is null while canonical clear is driven independently by `CombatEncounterController.Won`; `OlympusStationCombatResultPresenter` then opens the clear overlay.
- `RouteResultRecord` is therefore a useful optional encounter-proof adapter, not the canonical stage outcome and not evidence that a complete run summary already exists.
- This separation is correct: `clear condition` and optional `mastery proof` must remain distinct, and opening the result overlay must never manufacture summon-answer success.
- The current `StageClearScreenPresenter` is therefore best kept as a view: it should receive a summary instead of becoming the next statistics owner.

#### Minimum first result slice

Candidate normalized fields, to be locked only after the peer-game pass:

- identity: `schemaVersion`, `runId`, canonical playable-stage ID, route revision, route snapshot digest, ordered/current segment IDs, both resolved stage-definition/scene identities, and the complete snapshotted terminal-resolution policy from the P1-0 route shell, with the optional template join explicitly unresolved until P1-B supplies it for new runs;
- outcome: the closed clear/fail arm with typed failure-reason absence/presence plus canonical integer total-stage and combat-segment elapsed milliseconds; UI derives seconds;
- survival: resolved player damage taken and player-down count;
- action proof: perfect-dodge count and normalized summon-use records;
- identity proof: semantic summon-answer proof IDs and encounter proof IDs, plus `optionalRouteProofAdapter = None | CommittedRouteResultRecord(exact normalized encounter-proof fields/digest)` when that adapter actually commits; this is never outcome authority;
- optional adapters: structure-break count and forward-risk seconds;
- mastery boundary: `masteryEvaluationState = NotEvaluated` and an empty result list in P1-A; P1-D later evaluates immutable facts, never UI strings;
- handoff: outcome-filtered offered action IDs from the one P1-0 `PlayableStageDefinition` route shell, not copied route definitions;
- diagnostic abort: a separate immutable abort record containing run/route identity, last lifecycle state, reason, and sequence; it never becomes a product `RunResultSummary` or progression/reward input.

First vertical slice:

1. Create `StageRunContext` at logical stage entry with a new run ID by deep-snapshotting the approved P1-0 `PlayableStageDefinition` route shell, including segment/scene identities, full action semantics, terminal batch owner/root boundary, subject roles, exclusive coverage, nested/independent-root rules, epoch/barrier, tie/final requirements, and canonical digest; fail closed if either segment definition, outcome/action policy, terminal policy, or requested stage/revision is missing or disagrees.
2. Make the flow controller synchronously seal Corridor facts through the run owner immediately before requesting the Station single-load; compare the current loader's requested destination to the immutable snapshot and do not rely on independent `Completed` subscriber order. Transfer only serializable IDs and facts. An unexpected or failed handoff enters abort closing and seals one diagnostic abort only after admitted closure results are known; successful closure disposes, while a timeout/fault enters `ClosureFaulted`, and neither branch commits a product result. P1-B later removes the hard-coded forward loader.
3. Replace the ambiguous guide boolean with an explicit `NotStarted / Playing / Released / Interrupted` state or a release event. Start combat time only from `Released`.
4. In Station, bind fresh scene-local collectors to player health/actions, summon actions, energy ladder, boss encounter, and encounter-proof events before gameplay release; bind typed `{ Player, Boss }` terminal subjects to their authoritative health objects and unsubscribe on every exit.
5. Before the current encounter collapses terminal state into only `Won` or `Failed`, introduce one authoritative `EncounterTerminalResolutionCoordinator`. A canonical combat producer obtains `RootAdmissionSequence` before any bound-subject mutation/callback; lower sequence is the intended causal order, callbacks cannot admit roots, and only the active admission receives `RootResolutionToken`/epoch authority. Every terminal-state mutation enters the synchronous non-yielding queue; same-root nested work drains in that epoch, while higher independent admissions wait without authority. Each root follows `Open -> Draining -> Finalizing -> EpochClosed` and synchronously snapshots both touched/untouched subjects at `QueueDrainedAndSubjectsFinalized`; nonterminal close returns through `Idle` and opens the next pending admission. A terminal close reaches `TerminalClosed`, wins the shared terminal-or-restart latch, seals `TerminalFinalizationAuthority`, and enters `TerminalFinalizing`. Only that winner finalizes deterministic collectors, course traversal/quiescence, P1-C `RunFinalization`, and the current presentation generation before `OutcomeFactsSealed`; admitted P1-D mastery and then P2-A variability closure seal before `CommitRequested`. Work/adapter/integrity failure faults; unload/explicit abort cancels; both atomically invalidate active/pending authority and map to one diagnostic run abort. Wrong-run or post-terminal authority is reject/log-only. Only the complete ordered success path deep-freezes and publishes one `Committed` summary.
6. Remove raw encounter ownership from terminal presenters. Pass the committed summary into the additive result presenter and render clearly labelled total/combat time plus two identity proofs; a missing summary disables actions and reports a diagnostic rather than inventing facts.
7. Route Replay/Retry/Lobby through one typed terminal-action executor. One compare-and-set derives a complete dispatch payload from the run's immutable route snapshot, seals action ID/kind/target plus revision/digest before disposal, and never re-reads the current stage asset or reopens on double-click, competing input, stale UI, or dispatch/load failure. It revalidates or awaits the fixed admitted owner set in order—P1-E lesson, P2-B course, P1-C execution, P2-A variability, and P2-B presentation—using exact receipts or typed `NotAdmitted`; success alone disposes/navigates, while failure preserves the committed result and sealed selection in `Presented` with dispatch blocked. A fresh run is created only after successful Corridor re-entry. Review-only result controls must be disabled or delegate this executor.
8. Treat `RouteResultRecord` as an optional adapter only when it actually commits; canonical clear must not wait for its unrelated close-threat prerequisite.
9. Treat the tutorial-enabled Corridor-to-Station path as the only canonical route for this logical stage. A Corridor-only fallback cannot commit the same product outcome, and a direct Station load with no active canonical context is diagnostic-only and cannot manufacture a run.
10. Review the local recommendation in [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md): an outcome-aware shared result shell, Retry plus Lobby for Fail, lower pre-mutation root-admission sequence as independent-root causal order, and Clear-wins only when both candidate/final terminals agree in the same active epoch. Explicit approval remains required before the production slice, and no policy may depend on render frames, timers, health-callback arrival, or subscriber order.
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

1. Add one versioned `StageRuleSet` whose entries distinguish `RecommendationOnly` from `EnforcedRestriction`. Recommendation has no gameplay mutation or cleanup claim; enforcement requires an exact source-scoped port, captured prior state, and exhaustive release receipts.
2. Resolve the rule set, zero-or-one modifier ref, optional versioned `StageEnemyVariantBindingSet`, and authored restart definition into one immutable `StageVariabilityPlanSnapshot` at logical route entry. Encode modifier absence as an empty array and binding-set absence as typed `None`, store the sole `ResolvedActiveRunRestartPolicy` there, include its semantic digest in new-schema route/result provenance, and never reread newer assets into the active run.
3. Add one `StageModifierDefinition` with display metadata separate from a closed typed payload, required executable adapter capability, apply/remove lifecycle, owned-domain ledger, stale-generation guard, and `StageVariabilityQuiescenceBarrier`. No modifier graph, stack solver, or random pool.
4. Bind one existing P1-C scoped spawn key to one `EnemyVariantProfile` through the reachable versioned binding set. P1-C's payload mapping is the sole gameplay-prefab authority. Configure the frozen role/deck/elite composition only through a typed adapter while the P1-C staging root is inactive, require a matching receipt before activation, and never copy payload, anchor, order, count, delay, or object lifetime.
5. Keep target time, recommended power, featured summon need, and combat lesson on the linear template; keep story cues on canonical cinematic references; keep post-result Replay/Retry on P1-0/P1-A typed actions. P2-A owns only rule-derived recommendation/restriction, modifier/variant identity, and pre-result restart policy.
6. Keep active-run restart, revive, and post-result Replay/Retry as different typed policies. The first schema treats revive as unsupported and fail-closed. A raw active-restart request reaches P1-A before cleanup; P1-A validates the nested policy and must win the shared terminal-or-restart latch before a terminal arm enters `TerminalFinalizing`. It then enters `RestartClosing` and seals the restart dispatch record before independently requesting P1-E lesson, P2-B course, P1-C execution, P2-A variability, and P2-B presentation quiescence. It seals the one evidence-complete abort only after closure results are known. Successful closure alone disposes and performs the actual dispatch; a failed barrier leaves the old run `ClosureFaulted` and creates no new run.
7. Preserve the local ownership boundary: existing archetype/role/candidate authorities compose `EnemyVariantProfile`; `StageEnemyVariantBindingSet` owns only versioned membership and each binding adds variant identity over a P1-C scoped spawn key. The typed adapter returns a configuration receipt during P1-C inactive staging, while P1-C alone retains group/order/payload/prefab/anchor/lifetime; neither layer redefines the whole enemy behavior system.

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

First bounded slices, in P2-A then P2-B order:

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

1. Reuse and transactionally extend the minimal `StageProgressState`, durable result intent, applied delta, and global application ledger established by P1-D; do not create a second progress owner. For P2-C-capable new runs, cut over from the standalone P1-D writer to one combined progress/reward settlement so the same run is not applied twice. Keep state separate from the authored `StageProgressionNode` and preserve clear count, first-clear run ID, achieved mastery IDs, best total-active elapsed milliseconds/provenance, state revision, and per-run application history.
2. Store explicit typed prerequisite states and next-progression-node IDs in the same identity domain used by `StageProgressState`; derive the linked playable-stage route only after resolving the target node. Never infer progression from numeric or lexical IDs or collapse `PASS` and `COMPLETE` into one undocumented boolean.
3. Keep `battleStageId` separate from `progressionNodeId`. Author explicit pre-battle story, post-battle story, and optional after-clear script references rather than deriving them from ID equality or row order.
4. Commit victory before dispatching post-battle story or after-clear hooks. Fail, retry, abort, stale, and duplicate paths cannot dispatch post-clear side effects.
5. Snapshot the current node plus every prerequisite/unlock-relevant `StageProgressState` and revision, then resolve the committed run purely into `ProgressionResolution`: first-clear flag, newly achieved mastery IDs, newly unlocked nodes, and eligible reward-bucket IDs. Never mutate state before first/new-state decisions are complete.
6. Apply the resolved state transition exactly once for the run; failed, stale, aborted, or duplicate callback/application paths do not add a second mutation. A legitimate replay after clear is a distinct new run.
7. Use one revisioned `RunRewardPlan` with conditional buckets such as `EveryClear`, `FirstClear`, and `FirstMastery` instead of parallel plan objects.
8. Keep authoring references such as `baseRewardRef`, `firstClearRewardRef`, and `allObjectivesRewardRef` distinct inside that plan. A label like base/default does not by itself mean every-repeat grant.
9. Derive reward preview from the authoritative plan plus current progress; preview/catalog rows never grant or override eligibility.
10. Keep authoritative result/progress update, categorized reward payload, and durable receipt conceptually distinct even if one local transaction writes them together.
11. Resolve the full inventory delta before mutating progress/application state. The first bounded slice must atomically commit progress, the application ledger, one versioned balance, journals, and receipts in one reviewed transactional local store and publish the committed generation last. A backend that offers only a recoverable journal/outbox is deferred to a future split-store review and does not satisfy this first-slice gate. Never copy the observed unsafe shape `persist claimed -> call fallible inventory mutation` without recovery.
12. Produce a `RewardReceipt` with an idempotency key derived from run ID, reward-plan ID, plan revision, and bucket ID. A duplicate request returns the same receipt/result instead of a generic failure; the result UI displays the receipt but never grants it.
13. Before enabling rewards over P1-D progress, choose an explicit migration policy for already-cleared/mastered state. If P1-D used a different or journal-backed store, stage and verify a complete state/application-ledger import, then atomically publish the P2-C schema/epoch and imported committed root in the selected transactional store before accepting a new P2-C entry; never dual-write the old and new stores. A later repeat clear must never receive an ordinary `FirstClear` bucket merely because no older reward receipt exists.
14. Start with one reward path tied to the first summon lesson and one recommended growth action, not a full inventory or equipment maze.
15. For revision 1, keep failed-run Retry free and available after its committed Fail summary because Fail writes no progress. Keep manual clear Replay/Lobby free but enable them only after the P1-D progress application or P2-C combined settlement is durably committed; summary commit alone is not enough for a persistent Clear. Each action remains distinct and creates a new run. Defer automatic repeat, entry/claim/refund cost, random drops, and fast-clear behavior until repeat play justifies a versioned policy.

### P3 — Broaden content operations only after repeat play works

- Daily practice tasks that reinforce real combat behaviors.
- Multiple practice courses and character/summon-specific lessons.
- Challenge variants, score submission, and reusable enemy configuration sets.
- Additional result presentation and camera/cut-in polish.
- Economy, stamina, shop, passive/base rewards, and liveops only when the core loop earns repeat play.

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

Completed focused or supporting pass:

- Punishing: Gray Raven course, practice, teaching, guide-fight, and result-related surfaces.
- Honkai Impact 3rd early stage, typed challenge, enemy variant, result, and plot/dialog surfaces.
- Aether Gazer topology/rules/presentation lifecycle, with ZZZ group/member and tutorial-presentation cross-check.
- Wuthering Waves dungeon briefing, tutorial attempt/reset, enemy reference, and flow cleanup surfaces.
- Snowbreak classified as indirect QA automation evidence only.

Next queue:

1. PGR follow-up only for unresolved result/progress field joins.
2. HI3 follow-up only if decoded challenge meanings or authoritative early-wave joins become available.
3. Snowbreak only if game-internal data appears; external MAA material remains QA-only.

Questions:

- How is one mechanic isolated, validated, repeated, and then combined?
- Which data belongs to stage, tutorial, enemy configuration, result, or progression?
- How are camera and input restrictions released on success, failure, cancel, restart, and scene handoff?

### Stage/progression specialists

Completed supporting pass:

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

1. No further deep dataset audit is currently justified; return to the P1-0/P1-A/P1-B implementation-review decisions and the remaining P0 navigation evidence.
2. Revisit Stella Sora only if official/direct result validation or transaction evidence appears; the community emulator remains a negative test source.
3. Revisit Epic Seven, CounterSide, Path to Nowhere, Princess Connect, or other excluded archives only if stronger decoded runtime material becomes available.

Questions:

- How are prerequisite, star/mastery, first clear, repeat clear, and next-stage recommendation separated?
- How much information belongs on the stage card versus prep and result screens?

### Presentation and narrative support

Completed focused or boundary pass:

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

## Promotion Rule

A reference-derived idea enters implementation only when all answers are yes:

1. Does it solve a verified current gap?
2. Does it strengthen the summon-first combat identity?
3. Can it be implemented as a small vertical slice?
4. Does it avoid destabilizing the canonical demo route?
5. Is the evidence strong enough to define a testable contract?

## Next Analysis Actions

1. Retain the 2026-07-14 11:10 full cross-scene PASS only as historical snapshot evidence because Station was saved again at 11:15:21; obtain both a newer full-route report and a newer natural-handoff report than the stale 10:38 PASS.
2. Resolve the enabled Station review HUD versus additive clear UI retry conflict, then close P0 with actual retry-to-Corridor and lobby button execution; a configured target string is not sufficient proof.
3. Review [P1 Product Decision Packet](P1_PRODUCT_DECISION_PACKET.md), then obtain explicit approval or revision of its four coupled recommendations: P1-0 identity, `Clear -> Replay + Lobby` and `Fail -> Retry + Lobby`, the outcome-aware shared result shell, and D4's pre-mutation root order plus synchronous lifecycle and same-epoch Clear-wins arbitration.
4. Update [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md) around the newly confirmed gaps: author a Station segment definition, do not force any existing mismatched linear template, collapse or explicitly variant-label the duplicate catalog row, lift the selected row's working raw scene projection into canonical playable-stage resolution, and validate the actual combined intro timeline through a direct cinematic-profile reference.
5. Review [Ordered Encounter Execution Bridge Spec](ORDERED_ENCOUNTER_EXECUTION_BRIDGE_SPEC.md), then during P1-C0 approve the stage-local binding; run-admission plan/digest and canonical scene reservation; sole activation envelope with stale-command classification; scaled clock and cancel precedence; separate group/sequence states; `CombatHealth.Died` terminal; binding-root-local scene-ready pose capture/tolerances; inactive staging plus transactional payload mapping; named completion-gate CAS/phase-open order; canonical-priority PVE scene lease; and exact fixture IDs. Do not freeze a fixture until P1-A lifecycle/quiescence and P1-B's exact current-route pocket plus real count-1 Station Add payload/anchor exist.
6. Review [Stage Presentation Handoff Lifecycle Spec](STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md) against the `intro-to-stage` combined-profile mismatch, choose its first input/HUD ownership fixture, and add the named presentation quiescence/fault boundary. Then review [Tutorial Course Lesson Chain Spec](TUTORIAL_COURSE_LESSON_CHAIN_SPEC.md) as a separate one-run course contract; do not freeze it until the exact summon Basic, Practice host/baseline, and Challenge proof/objective exist.
7. Review [Typed Mastery and Progress Application Spec](TYPED_MASTERY_PROGRESS_APPLICATION_SPEC.md), then during P1-D0 approve objective-ID semantic permanence plus its identity manifest/tombstones, objective-set canonicalization, bundle-invalid policy, canonical total-active milliseconds, exact qualified summon-proof fixture, save-profile namespace, durable prepared-intent acknowledgment boundary, corrected stage-select projection, and one fault-injectable checksummed generation store. Retain [Stage Progression and Reward Transaction Spec](STAGE_PROGRESSION_REWARD_TRANSACTION_SPEC.md) as the later P2-C gate that replaces the standalone writer for new cohorts and extends the same state/ledger store with one versioned balance, settlement generations/reservations, frozen reward revision, and receipt retention; do not implement the reward layer early.
8. Carry forward Brown Dust 2's stale-async cleanup checks and Limbus Company's explicit before/after story plus separate battle/progression IDs without promoting either above the current P1/P2 order.
9. Carry forward Last Origin's directed prerequisite/next separation, stage-to-group join, reward-reference split, and preview drift; retain Princess Connect only as a weak objective-separation boundary.
10. Carry forward Stella Sora only as a community structural/negative source: never accept client-supplied stage ID as outcome proof, require atomic progress/inventory/receipt settlement, and make duplicate settlement replay return the stored receipt.
11. Stop widening the dataset search for now. Resume with the P0 terminal-owner/navigation gate and P1-0/P1-A/P1-B decisions above.
12. Revisit PGR, HI3, Epic Seven, CounterSide, Path to Nowhere, HSR, HBR, or other incomplete datasets only when stronger raw fields or runtime joins appear.
13. Re-score the matrix after each bounded slice using measured implementation cost and observed regressions.
