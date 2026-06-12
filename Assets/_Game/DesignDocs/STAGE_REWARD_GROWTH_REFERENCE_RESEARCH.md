# Stage Reward Growth Reference Research

Last updated: 2026-06-12 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` stage, result, reward, daily-routine, and growth-loop direction. It was written after reading:

1. `PROJECT_BRIEF.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. `AGENTS.md`
5. `AI_CODE_CONTRACT.md`
6. `ARCHITECTURE_BOUNDARIES.md`
7. `HUD_COMBAT_SPEC.md`
8. `Assets/_Game/DesignDocs/ARPG_REFERENCE_RESEARCH.md`
9. `Assets/_Game/DesignDocs/SUBCULTURE_UI_REFERENCE_RESEARCH.md`
10. `Assets/_Game/DesignDocs/COMBO_SYSTEM_REFERENCE_RESEARCH.md`
11. `Assets/_Game/DesignDocs/SUMMON_SYSTEM_REFERENCE_RESEARCH.md`
12. `Assets/_Game/DesignDocs/BOSS_ENEMY_RUN_REFERENCE_RESEARCH.md`

Active extraction rule: use reference games to extract production patterns for stage entry costs, mission unlocks, result rewards, repeat farming, daily and weekly tasks, account-level gates, passive base/cafe rewards, reward reveal pacing, growth sinks, and how those loops make the next combat run desirable.

## Scope Guard

This is a production seed catalog, not a v1 scope expansion request.

`PROJECT_BRIEF.md` still excludes `meta progression and collection` from the immediate v1 vertical slice. Use this document to prepare the loop that can wrap the combat slice once the core `3 to 5 minute` ARPG run is fun.

Do not use this document to reintroduce:

- `PvP`
- `hand-of-cards UI`
- `direct target selection`
- `lane-first input`
- `collection-first scope creep`
- `idle reward systems before combat feels good`
- `invisible stat inflation as the main progression proof`

Use it to answer:

- Why does the player start the next run?
- Which reward proves that the last run mattered?
- Which summon or run tool can grow without breaking the small input budget?
- Which daily tasks teach the intended combat behavior instead of asking for chores?
- How should stage select, result, and growth screens talk to each other?
- Which data tables should exist before the main PC starts implementing progression?

The source set includes `PGR / Honkai Impact 3rd / Zenless Zone Zero / Wuthering Waves / Genshin Impact / Honkai: Star Rail / Blue Archive / Arknights / NIKKE` and prior UI/action references. It is not limited to these games.

Asset/code ownership boundary: do not import proprietary assets, full raw tables, source code, audio, animation clips, textures, meshes, fonts, or screenshots as project assets. Extract schemas, pacing, reward relationships, and authoring patterns.

## Source Inventory

| ID | Game / Source | Type | Useful data | Confidence | URL |
|---|---|---:|---|---:|---|
| `zzz_battery_charge` | Zenless Zone Zero | Wiki | Battery cap, refill rhythm, activity costs, coffee/manual refill, reward-claim-only spend behavior | High | https://zenless-zone-zero.fandom.com/wiki/Battery_Charge |
| `zzz_inter_knot` | Zenless Zone Zero | Wiki | account level, rank-up commissions, difficulty/reward unlocks, agent/Bangboo/W-Engine cap gates | High | https://zenless-zone-zero.fandom.com/wiki/Inter-Knot_Level |
| `zzz_data_loop_tables` | Zenless Zone Zero | GitHub data mirror | daily quest templates, level quest templates, activity tasks, reward bars, daily VR level effects, drop templates | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `pgr_serum` | Punishing: Gray Raven | Wiki | Serum as stamina, 160 cap, 1 per 6 minute recovery, 1:1 Squad EXP, mission completion spend | Medium | https://punishing-gray-raven.fandom.com/wiki/Serum |
| `pgr_beginner_unlocks` | Punishing: Gray Raven | Wiki | story milestone unlocks for daily login, dailies, shop, dorm, War Zone, Phantom Pain Cage; retry without stamina cost | Medium | https://punishing-gray-raven.fandom.com/wiki/Beginner%27s_Guide |
| `pgr_war_zone` | Punishing: Gray Raven | Wiki | biweekly ranked cycle, no-serum unlimited stage entry, recruitment/battle/result phases | Medium | https://punishing-gray-raven.fandom.com/wiki/War_Zone |
| `pgr_pain_cage` | Punishing: Gray Raven | Wiki | boss-score mode, difficulty ladder, daily attempts, weekly character AP, discard-score retry protection | Medium | https://punishing-gray-raven.fandom.com/wiki/Phantom_Pain_Cage |
| `pgr_dorm` | Punishing: Gray Raven | Wiki | dorm tenants, chores, decor stats, mood/stamina, gift frequency | Medium | https://punishing-gray-raven.fandom.com/wiki/Dorm |
| `pgr_data_loop_tables` | Punishing: Gray Raven | GitHub data mirror | activity, task, shop, reward, stage, auto-fight result, dorm task, draw result, red-point condition file taxonomy | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab |
| `hi3_missions` | Honkai Impact 3rd | Wiki | daily combat missions, login stamina, material/story/stamina-spend tasks, accumulated BP rewards | High | https://honkaiimpact3.fandom.com/wiki/Missions |
| `hi3_data_loop_tables` | Honkai Impact 3rd | GitHub data mirror | `MissionData`, `RewardData`, `StageData_Main`, `DutyDailyData`, `LoginRewardSchedule`, `FarmMaterial`, activity rewards | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `wuwa_waveplate` | Wuthering Waves | Wiki | Waveplate reward claim resource, recurring challenge costs and Union Level gates | Medium | https://wutheringwaves.fandom.com/wiki/Waveplate |
| `wuwa_guidebook` | Wuthering Waves | Wiki | daily activity points, milestones, material spots, recurring challenges, path of growth, enemy tracing | Medium | https://wutheringwaves.fandom.com/wiki/Guidebook |
| `genshin_resin` | Genshin Impact | Wiki | Resin reward-claim resource, activity costs, Adventure EXP per Resin, reward claim separated from completion | Medium | https://genshin-impact.fandom.com/wiki/Original_Resin |
| `hsr_trailblaze_power` | Honkai: Star Rail | Wiki | Trailblaze Power reward-claim resource, per-power account EXP, material/relic/boss reward costs | Medium | https://honkai-star-rail.fandom.com/wiki/Trailblaze_Power |
| `blue_archive_mission` | Blue Archive | Wiki | Normal/Hard campaign farming, 3-star quick clear, hard fragment daily limits, star/turn objectives | Medium | https://bluearchive.fandom.com/wiki/Mission |
| `blue_archive_special_request` | Blue Archive | Wiki | AP-based EXP/credit farm stages, skip after 3-star, daily/weekly task hooks | Medium | https://bluearchive.fandom.com/wiki/Special_Request |
| `nikke_outpost` | NIKKE | Wiki | passive outpost reward accumulation, 24-hour cap, campaign stage count raises outpost level, Wipe Out | Medium | https://nikke-goddess-of-victory-international.fandom.com/wiki/Outpost |
| `arknights_sanity` | Arknights | Wiki | Sanity cost range, 1 per 6 minute regeneration, refund policy, level cap increase | Medium | https://arknights.fandom.com/wiki/Sanity |
| `arknights_riic` | Arknights | Wiki | base facilities, operator morale, passive LMD/Battle Record production, trust interaction loop | Medium | https://arknights.fandom.com/wiki/Rhodes_Island_Infrastructure_Complex |
| `local_result_shell` | IsekaiBrawl local code | Repo | current result UI, advance rewards, just dodge summary, stage context, structure energy rewards | High | local files |

## Observed Production Data Shapes

### ZZZ: stamina is a reward-claim budget, not only an entry ticket

Public Battery Charge data exposes:

- cap `240`
- automatic recovery: `1` point per `6 minutes`
- full empty-to-cap time: `24 hours`
- overcap storage up to `1000`, but regeneration resumes only below base cap
- activity costs such as `Combat Simulation 20-100`, `Expert Challenge 40`, `Routine Cleanup 60`, `Notorious Hunt 60`
- resource is consumed on completion / reward claim, so exit or restart does not consume it
- daily coffee can restore `60-80` Battery Charge

Practical read:

- The stamina system protects reward pacing while still allowing combat retry.
- Daily login routing includes one thematic refill interaction, not just a number ticking up.
- Activity cost is tied to reward type: simulation materials, boss materials, disc/equipment cleanup, weekly boss.

IsekaiBrawl application:

- Do not spend any long-term entry resource until the run is completed or a reward is claimed.
- Keep early combat retry instant and free.
- Later, if a stamina-like resource exists, name it around our world fantasy and let it gate reward claims, not learning attempts.
- A daily `Cafe/Command Room` interaction can refill a small amount and remind the player of today's recommended summon answer.

### ZZZ: account progression gates difficulty and growth caps

Inter-Knot Level data exposes:

- account level rises through account EXP/credit.
- every 10 levels from 20, rank-up commissions increase reputation rank.
- reputation increases difficulty and rewards for monsters, bosses, and challenges.
- account rank unlocks higher max levels for Agents, Bangboos, and W-Engines.
- new challenge difficulties unlock at rank thresholds.

Practical read:

- The account ladder is not just vanity. It bundles `cap increase + difficulty increase + reward increase`.
- Rank-up commissions are authored friction points that prove readiness before raising the reward tier.

IsekaiBrawl application:

- If we add account progression later, it should unlock `run tier + summon cap + boss deck modifiers` together.
- Rank-up tests should be bespoke short runs that verify one combat lesson, such as `Tank rescue`, `Break gate`, `Arrow backline`, or `Heal under pressure`.

### ZZZ data mirror: daily/quest/reward table names are explicit authoring surfaces

The public data mirror tree contains file families such as:

- `FileCfg/DailyQuestTemplateTb.json`
- `FileCfg/DailyLevelTemplateTb.json`
- `FileCfg/DailyRewardTypeTemplateTb.json`
- `FileCfg/DailyScheduleTemplateTb.json`
- `FileCfg/LevelQuestTemplateTb.json`
- `FileCfg/BattleRankingRewardsTemplateTb.json`
- `FileCfg/AbyssRewardBarTemplateTb.json`
- `FileCfg/ActivityTaskTemplateTb.json`
- `FileCfg/DropPackTemplateTb.json`
- `FileCfg/DungeonDropPoolTemplateTb.json`
- `Data/LevelDailyVR_EnterSceneEffect_*.json`
- `Data/Card_DailyUse_*.json`

Observed sample files are partially obfuscated, but the production shape is still useful:

- daily cards and daily level entries are separate.
- battle/abyss ranking reward bars are separate from level quests.
- drop pools and drop packs are separate from the stage objective row.
- daily VR entry has scene/effect data separate from the reward table.

IsekaiBrawl application:

- Keep `StageDefinition`, `RunRewardPlan`, `DailyRoutineTask`, `RewardTrack`, `DropPool`, and `StageEntryCue` separate.
- Do not hide result rewards inside the combat scene class.

### PGR: Serum spend rewards account EXP, but failure does not punish learning

Public Serum and beginner guide data expose:

- cap `160 ml`
- recovery `1 ml` every `6 minutes`
- consuming Serum awards `Squad EXP` at `1:1`
- PGR consumes stamina only when a stage is completed.
- the beginner guide explicitly frames retries as encouraged because failed attempts do not consume Serum.

Practical read:

- Stamina can coexist with skill-based ARPG learning if failure is cheap.
- Account EXP is a predictable byproduct of spending the main run resource.

IsekaiBrawl application:

- The first playable loop should allow unlimited retry.
- Later stamina/resource spend should pay `Account EXP` or `Command EXP` on clear.
- Use result messaging to encourage retry after failure instead of showing a harsh loss tax.

### PGR: story milestones unlock the daily loop in slices

The beginner guide lists unlocks tied to story clears:

- Novice Missions at `1-6 Normal`
- Daily Login Reward at `1-9 Normal`
- Dailies at `1-10 Normal`
- Gacha at `1-12 Normal`
- Shop at `2-3 Normal`
- Dorm at `4-2 Normal`
- War Zone at `4-8 Normal`
- Phantom Pain Cage at level `35` / `5-7 Normal`

Practical read:

- New players are not shown the entire economy at once.
- The story path acts as the tutorial and system-unlock spine.

IsekaiBrawl application:

- Unlock only what explains the next combat need.
- Recommended order:
  - `StageSelect + Result`: after first run
  - `Summon Upgrade`: after Break/Tank/Arrow/Heal are each taught
  - `Daily Practice`: after perfect dodge and summon answer are understood
  - `Boss Score Trial`: after first boss kill

### PGR: score modes separate practice, attempts, and final submission

War Zone data exposes:

- unlimited entry
- no Serum cost
- biweekly cycle
- recruitment, battle, and result phases
- leaderboard scoring from cleared stage scores

Phantom Pain Cage exposes:

- boss simulation scoring from completion time and team health
- daily attempts
- weekly character AP
- score discard or quitting can preserve attempts/AP
- difficulty ladder increases stats and attack patterns

Practical read:

- Score content can let the player practice without paying the same resource as material farming.
- Submission pressure is separated from practice pressure.

IsekaiBrawl application:

- A later `Boss Drill` mode can use practice-free attempts before score submission.
- For the v1-adjacent story loop, use score/rank only as result language: `Clear`, `Fast Clear`, `Clean Clear`, `Summon Answer`.

### PGR and Arknights: home/base systems produce resources through character assignment

PGR Dorm exposes:

- dormitories with up to three tenants.
- chores produce decor coins.
- decor has cosmetic stats.
- character stamina and mood drive chores and gift frequency.

Arknights RIIC exposes:

- base opens after an early operation.
- facilities generate LMD, Battle Records, trust, clues, recruitment refreshes, and crafting outputs.
- operators have morale that drains while working and recovers in dormitories.

Practical read:

- Off-combat bases are long-tail retention and character attachment systems.
- They are also resource production and light scheduling systems.

IsekaiBrawl application:

- This is not v1, but the eventual analog should be small:
  - `Summon Camp`
  - one active heroine/guide line
  - passive low-value materials
  - assignment slots for Break/Tank/Arrow/Heal training
- Do not build a full base before the combat run proves itself.

### HI3: daily missions are behavior routing, not just rewards

Public mission data exposes:

- daily login gives stamina.
- story, material, dorm, adventure, arena, abyss, and realm tasks all feed EXP/BP.
- daily tasks include both activity-specific tasks and broad resource-spend tasks.
- accumulated BP rewards unlock additional items at thresholds.

The public data mirror exposes:

- `Global/ExcelOutputAsset/Decrypted/MissionData.json`
- `Global/ExcelOutputAsset/Decrypted/RewardData.json`
- `Global/ExcelOutputAsset/Decrypted/StageData_Main.json`
- `Global/ExcelOutputAsset/DutyDailyData.json`
- `Global/ExcelOutputAsset/LoginRewardSchedule.json`
- `Global/ExcelOutputAsset/FarmMaterial.json`
- `Global/ExcelOutputAsset/GeneralActivityStageReward.json`

Observed `DutyDailyData.json` sample fields:

- `dutyId`
- `needDuty`
- `rewardId`
- `unlockLv`

Observed `MissionData.json` sample fields:

- `id`
- `type`
- `subType`
- `title`
- `description`
- `finishWay`
- `finishParaInt`
- `finishParaStr`
- `totalProgress`
- `rewardId`
- `LinkType`
- `LinkParams`
- `PreviewTime`
- `Priority`

Practical read:

- Daily missions point players toward specific surfaces and carry jump links.
- Reward thresholds are a separate table from the mission rows.
- Unlock level and priority are explicit.

IsekaiBrawl application:

- Daily tasks should route into combat lessons:
  - `Perfect dodge twice in Story PvE`
  - `Call Tank during boss pressure`
  - `Break one structure with Break`
  - `Clear a run with Arrow solving backline`
- Store task row, jump target, reward row, and threshold chest separately.

### WuWa: Guidebook unifies daily activity, material routing, training, and enemy tracing

Guidebook data exposes:

- daily activity tasks refresh on daily reset.
- every `20` Activity Points up to `100` grants stage rewards.
- milestones have phases with smaller tasks and phase-complete rewards.
- material spots list material collection challenges.
- recurring challenges list endgame or rotation modes.
- Path of Growth teaches combat through Tactical Hologram, Skill Training, and Basic Training.
- Enemy Tracing locates target enemy types from a list, sorted or filtered by threat.

Waveplate data exposes:

- Waveplates are used to claim rewards from recurring challenges.
- example costs: Simulation `40`, Boss `60`, Weekly `60`, Tacet Field `60`, Forgery `40`.
- Union Level gates unlock activities.

Practical read:

- One guide surface can replace many explanation popups.
- The best guidebook rows answer: `what should I do`, `why`, `where is it`, and `what reward will I get`.

IsekaiBrawl application:

- Build a compact `Tactical Guide` later:
  - `Today`
  - `Growth`
  - `Boss Drill`
  - `Summon Role Practice`
  - `Enemy Trace`
- It should deep-link to stage select or training runs, not explain in long text.

### Genshin and HSR: reward claim cost can be separated from encounter completion

Genshin Original Resin exposes:

- Resin claims challenge rewards from Ley Line Blossoms, Domains, and Bosses.
- activities may be completed without spending Resin, but reward cannot be claimed.
- Resin spend grants account EXP at a predictable ratio.

HSR Trailblaze Power exposes:

- Trailblaze Power claims rewards from material, relic, boss, and simulation activities.
- each power spent grants Trailblaze EXP.
- costs differ by activity type.

Practical read:

- This is useful for action games because a player can practice a fight without always paying the economy.
- The cost is tied to reward extraction, not the physical act of fighting.

IsekaiBrawl application:

- For our combat, never make learning the boss expensive.
- If reward stamina is added, use it at the result screen: `Claim Full Reward` vs `Practice Clear`.
- The first v1 slice can still use zero stamina and only local run rewards.

### Blue Archive: 3-star clear unlocks sweep, while hard stages create daily fragment caps

Mission data exposes:

- Normal missions primarily farm equipment.
- Hard missions farm character fragments.
- 3-star clear unlocks quick clear.
- Normal stage cost examples: `10 AP`.
- Hard stage cost examples: `20 AP`, `3` clears per day, optional paid reset.
- stage objectives include completion, S-rank counts, survival, time, and turn limits.

Special Request data exposes:

- EXP and credit farm stages unlock early.
- stages consume AP and can be skipped after 3-star.
- daily and weekly tasks hook into special request clears.

Practical read:

- Clear quality can unlock convenience without invalidating first-time combat.
- Hard material farming uses a per-stage daily cap to protect long-term pacing.

IsekaiBrawl application:

- `S-Rank` should initially mean combat quality, not raw stats:
  - no death
  - one correct summon answer
  - clear within target time
  - boss final stand clear
- Later, `Quick Replay` or `Simulated Clear` should unlock only after the player has proved mastery.

### NIKKE: campaign progress raises passive production

Outpost data exposes:

- passive accumulation of Credits, Battle Data, Core Dust, and Player EXP.
- rewards can be collected from Outpost or Home.
- storage reaches max capacity after `24 hours`.
- every `5` campaign stages cleared raises Outpost Defense level.
- Wipe Out grants immediate `2 hours` worth of resources; first daily Wipe Out is free.
- Tactics Academy lessons increase production or unlock slots.

Practical read:

- Campaign progress can improve passive resource income without making combat reward tables bloated.
- The home screen can surface idle reward claim as a simple return beat.

IsekaiBrawl application:

- Later, use `Camp Level` tied to Story PvE clear count.
- Do not add idle production to v1, but keep the data contract ready:
  - `passiveMaterialRate`
  - `storageCapHours`
  - `clearCountMilestone`
  - `trainingLessonUnlock`

### Arknights: stamina and base systems make planning explicit

Sanity data exposes:

- almost all operations consume sanity except training and some special cases.
- operation costs scale by difficulty, roughly `6-36`.
- quitting refunds all or most sanity depending on first-time/challenge mode.
- sanity regenerates at `1` every `6 minutes`.
- level-ups increase sanity cap and fully restore sanity.

RIIC data exposes:

- base facilities generate LMD, Battle Records, trust, clues, recruitment support, and crafting outputs.
- assigned operators drain morale and must rest.
- base layouts influence production.

Practical read:

- A serious economy should show costs and refund rules clearly.
- Base management can become a game by itself, so it must be delayed unless the project explicitly wants that scope.

IsekaiBrawl application:

- For now, use only the clarity rule: show cost, expected reward, first-clear reward, and retry policy on the stage card.
- Do not build facility management until combat and short-session retention are proven.

## IsekaiBrawl Loop Translation

### Active loop target

The immediate game should still be:

1. `Stage Select`
2. `3 to 5 minute horizontal ARPG Story PvE run`
3. `Result`
4. `Small reward proof`
5. `Retry / next stage / minor summon tuning`

This must not become a collection-first game before the combat is stable.

### Recommended v1-adjacent reward promise

Reward should support the current combat identity:

- Correct summon timing matters.
- Boss pressure can be answered.
- Aggressive forward play accelerates tempo.
- The player wants to retry fast.

So the first reward axes should be:

- `Run Clear`: did the final boss stand end?
- `Summon Answer`: did Break/Tank/Arrow/Heal solve their authored need?
- `Forward Pressure`: did the player spend enough time in the dangerous forward pocket?
- `Clean Dodge`: did perfect dodge create an empowered pressure beat?
- `Structure Break`: did Break open the reward/advance structure?

### Recommended first currencies

Use a tiny set first:

| Currency | Source | Sink | Why |
|---|---|---|---|
| `CommandExp` | run clear, first clear, daily practice | account/tutorial unlocks | proves the player is learning the command role |
| `SummonCore` | first-clear/challenge rewards | unlock summon role nodes | ties reward to the unique identity |
| `TacticChip` | repeat clear, summon-answer bonus | small per-role tuning | lets repeat play matter without gacha |
| `StyleMedal` | clean timing objectives | cosmetic/result rank only at first | rewards skill without power creep |

Avoid a large currency zoo until the stage loop is proven.

## Proposed Data Contracts

### `StageProgressionNode`

Purpose: where the player can go next and what this stage teaches.

Suggested fields:

- `id`
- `chapterId`
- `displayName`
- `unlockRule`
- `recommendedPower`
- `runDefinitionId`
- `firstClearRewardPlanId`
- `repeatRewardPlanId`
- `masteryObjectiveIds`
- `featuredSummonNeed`
- `bossPatternDeckId`
- `stageEntryCueId`
- `resultThemeId`
- `retryPolicy`

### `RunRewardPlan`

Purpose: what the result screen can grant.

Suggested fields:

- `id`
- `entryCost`
- `claimCost`
- `firstClearRewards`
- `repeatRewards`
- `rankRewards`
- `summonAnswerRewards`
- `masteryRewards`
- `accountExpPerClear`
- `practiceModeRewardScale`
- `dropPoolId`
- `rewardRevealCueId`

### `MasteryObjective`

Purpose: stage stars without importing lane/turn assumptions.

Suggested fields:

- `id`
- `kind`
- `targetValue`
- `displayPriority`
- `rewardTag`
- `recommendedSummon`
- `failDoesNotBlockClear`

Useful first kinds:

- `ClearStage`
- `ClearUnderTime`
- `NoPlayerDown`
- `UseSummonForNeed`
- `BreakStructure`
- `PerfectDodgeCount`
- `ForwardPressureSeconds`
- `FinalBossKill`

### `DailyRoutineTask`

Purpose: daily tasks that teach the real combat loop.

Suggested fields:

- `id`
- `category`
- `unlockRule`
- `objectiveKind`
- `targetValue`
- `linkedStageId`
- `linkedGuideId`
- `rewardId`
- `activityPoints`
- `priority`
- `expiresAtReset`

Good first tasks:

- `Clear one Story PvE run`
- `Trigger two perfect dodges`
- `Call Tank during immediate pressure`
- `Break one structure with Break`
- `Use Arrow against a backline threat`
- `Use Heal when HP is low`

### `GrowthSink`

Purpose: where rewards go.

Suggested fields:

- `id`
- `ownerKind`
- `ownerId`
- `tier`
- `cost`
- `unlockRule`
- `effectKind`
- `effectValue`
- `presentationCueId`
- `combatRiskNote`

Safe first sinks:

- `Break structure damage + small cue upgrade`
- `Tank spawn shield + relief duration test`
- `Arrow backline lock-on consistency`
- `Heal field duration / clarity`
- `Ultimate recharge tutorial upgrade`

Avoid:

- wide stat inflation
- many character equipment slots
- random artifact/relic substat grinding
- gacha-driven power planning

### `RewardRevealCue`

Purpose: result polish without one-off UI animation.

Suggested fields:

- `id`
- `rankStampCue`
- `rewardCardCue`
- `countUpDuration`
- `rareRewardGlint`
- `skipPolicy`
- `sfxTier`
- `bgmContext`
- `nextActionHighlight`

First result sequence:

1. `Outcome`: win/fail/time.
2. `Run Proof`: boss pushed, structures broken, summon answers.
3. `Rank`: clear / clean / tactical / breakthrough.
4. `Rewards`: first-clear or repeat rows.
5. `Growth Prompt`: only one recommended sink.
6. `Next`: retry or next stage.

## First Production Stage Loop

This is a proposed first loop after the current combat run is stable.

| Beat | Screen | Player sees | Data rows needed | Notes |
|---|---|---|---|---|
| 1 | Lobby | guide character / current energy / next stage | `LobbyGuideFeedback`, `DailyRoutineTask` | Reuse UI cue research; no full base yet. |
| 2 | Stage Select | boss threat, reward, recommended summon need | `StageProgressionNode`, `RunRewardPlan`, `BossPatternDeck` | Show `Break/Tank/Arrow/Heal` need, not long text. |
| 3 | Prep | current summon queue, one tactical hint | `StageEntryCue`, `SummonLoadoutHint` | No hand-of-cards return. |
| 4 | Combat | current 3-5 minute ARPG run | `BossRunDefinition`, `EncounterPressurePlan` | Existing boss/run research owns this. |
| 5 | Result | rank, action proof, rewards | `RunResultSummary`, `RewardRevealCue` | Reward reveal should be skippable. |
| 6 | Growth | one recommended upgrade | `GrowthSink`, `CurrencyBalance` | Show one next improvement, not a huge menu. |
| 7 | Next | retry / next stage / guide task | `NextActionRecommendation` | Instant retry remains visible. |

## First Stage Set

| Stage | Combat lesson | Featured reward | Mastery objective | Next unlock |
|---|---|---|---|---|
| `S1-1 Break Gate` | Break opens structure relief | `SummonCore: Break` | break one structure | `S1-2 Backline Signal` |
| `S1-2 Backline Signal` | Arrow removes backline pressure | `TacticChip: Arrow` | Arrow hits priority threat | `S1-3 Tank Rescue` |
| `S1-3 Tank Rescue` | Tank creates breathing window | `SummonCore: Tank` | call Tank during danger | `S1-4 Heal Pocket` |
| `S1-4 Heal Pocket` | Heal saves push under pressure | `TacticChip: Heal` | heal while under boss pressure | `S1-5 Boss Stand` |
| `S1-5 Boss Stand` | combine all summons into final boss kill | `CommandExp + StyleMedal` | final boss kill and no death bonus | `Daily Practice` |

## Result Ranking Model

Do not rank only by raw clear time. A summon-first game should rank the intended decisions.

Suggested result grades:

- `Clear`: finished the run.
- `Clean`: clear + low damage / no down.
- `Tactical`: clear + at least one authored summon-answer objective.
- `Breakthrough`: clear + boss final stand + 2 or more correct summon answers + forward pressure target.

Suggested result summary metrics:

- `ClearTime`
- `PlayerDamageTaken`
- `PerfectDodges`
- `SummonCalls`
- `CorrectSummonAnswers`
- `StructureBreaks`
- `ForwardPressureSeconds`
- `FinalBossHPRemovedBySummons`

Local code already tracks pieces of this through `BattleResultUI`, `StoryPveObjectiveSystem`, structure energy rewards, just dodge events, and run progress. The missing part is a clean data contract that tells the result screen which behaviors matter.

## Reward And Growth Risks

### Risk: growth hides combat problems

If an upgrade makes a bad run feel acceptable, it will hide missing combat readability.

Mitigation:

- keep first upgrades small.
- prefer behavior clarity upgrades over stat upgrades.
- test stages with baseline summons before tuning growth.

### Risk: daily tasks become chores

Daily tasks should not ask the player to touch every menu.

Mitigation:

- first daily tasks should be combat-behavior tasks.
- every task should deep-link to the relevant stage or guide entry.
- no more than 3 daily tasks in the first loop prototype.

### Risk: reward currencies get noisy

Many games use many currencies because they have years of content.

Mitigation:

- start with `CommandExp`, `SummonCore`, and `TacticChip`.
- add `StyleMedal` only when result ranks feel good.
- delay equipment/gacha/material subtypes.

## Implementation Recommendations

1. Draft `StageProgressionNode` and `RunRewardPlan` as data before adding a store, inventory, or base.
2. Extend the current result shell to show `run proof` first: clear time, perfect dodge, summon answer, structure break, final boss.
3. Add only one growth sink per core summon role.
4. Make daily tasks combat-teaching tasks, not menu chores.
5. Keep reward claim separate from combat retry if any stamina-like resource appears.
6. Delay idle/base production until the player wants to repeat the core run without being bribed.

## Next Smallest Follow-Ups

1. Write `STAGE_REWARD_GROWTH_SPEC.md` with exact `ScriptableObject` or JSON field names for `StageProgressionNode`, `RunRewardPlan`, `MasteryObjective`, and `GrowthSink`.
2. Add a first-pass `RunResultSummary` contract that maps current `BattleResultUI` signals to result ranks.
3. Prototype only one reward/growth path: `S1-1 Break Gate -> Break role upgrade -> S1-2 Backline Signal`.
