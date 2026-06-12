# Boss Enemy Run Reference Research

Last updated: 2026-06-12 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` boss, enemy AI, phase deck, telegraph, encounter pacing, run structure, and first-pass tuning. It was written after reading:

1. `PROJECT_BRIEF.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. `AGENTS.md`
5. `AI_CODE_CONTRACT.md`
6. `ARCHITECTURE_BOUNDARIES.md`
7. `HUD_COMBAT_SPEC.md`
8. `Assets/_Game/DesignDocs/ARPG_REFERENCE_RESEARCH.md`
9. `Assets/_Game/DesignDocs/COMBO_SYSTEM_REFERENCE_RESEARCH.md`
10. `Assets/_Game/DesignDocs/SUMMON_SYSTEM_REFERENCE_RESEARCH.md`

Active extraction rule: use reference games to extract concrete enemy and run-production patterns: boss phase decks, pressure modules, telegraph timing, break/stun relief windows, projectile emitters, enemy role affixes, encounter spawn/effect metadata, tuning envelopes, and the 3 to 5 minute Story PvE rhythm.

## Scope Guard

This is a production seed catalog, not a final balance sheet.

Use it to answer:

- What is the boss doing right now?
- What threat is the player meant to read?
- Which summon is the best answer?
- How long does pressure last before a readable relief window?
- When does auto basic-fire safely keep pressure without turning into manual attack spam?
- Where does `perfect dodge`, `summon call`, `boss break`, `structure break`, and `ultimate` fit in a 3 to 5 minute run?

Do not use it to reintroduce:

- `PvP`
- `hand-of-cards UI`
- `direct target selection`
- `lane-first input`
- `full manual combo action`
- `boss-only static arena`
- `unreadable bullet spam`

Asset/code ownership boundary: do not import proprietary assets, full data tables, source code, audio, animation clips, textures, or meshes. Extract schemas, timing ranges, pattern relationships, and tuning ideas.

## Source Inventory

| ID | Game / Source | Type | Useful data | Confidence | URL |
|---|---|---:|---|---:|---|
| `zzz_wiki_daze` | Zenless Zone Zero | Wiki | Daze gauge, stun duration, stun damage multiplier, chain count by enemy strength | High | https://zenless-zone-zero.fandom.com/wiki/Daze |
| `zzz_wiki_dodge` | Zenless Zone Zero | Wiki | Perfect Dodge, Dodge Counter, dash attack, invulnerability and counter grammar | High | https://zenless-zone-zero.fandom.com/wiki/Dodge |
| `zzz_wiki_chain_attack` | Zenless Zone Zero | Wiki | Stun -> heavy hit -> Chain Attack, enemy strength limits, Bangboo chain timing | High | https://zenless-zone-zero.fandom.com/wiki/Chain_Attack |
| `zzz_data_enemy_pattern` | Zenless Zone Zero | GitHub data mirror | BulletEmitter, EnemyCheck, BossFullHealthLock, WitchTime, attack/bullet files | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `pgr_wiki_combat_guide` | Punishing: Gray Raven | Wiki | 3-Ping, Matrix, QTE, Tank shield shred, Support healing, role answer grammar | Medium | https://punishing-gray-raven.fandom.com/wiki/Combat_Guide |
| `pgr_data_affix` | Punishing: Gray Raven | GitHub data mirror | NPC affix modules: damage amp/resist, distance haste, no-control, regen, shield-like traits | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab |
| `hi3_wiki_combat` | Honkai Impact 3rd | Wiki | Evasion, QTE, ultimate, button replacement, timed battle action grammar | High | https://honkaiimpact3.fandom.com/wiki/Combat |
| `hi3_wiki_time_fracture` | Honkai Impact 3rd | Wiki | Time-slow reward and QTE gate reference | Medium | https://honkaiimpact3.fandom.com/wiki/Time_Fracture |
| `hi3_data_monster_stage` | Honkai Impact 3rd | GitHub data mirror | Monster config, boss identity, AIName/config split, stage monster ability/resist/recommend tags, attack punish curve | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `wuwa_wiki_combat` | Wuthering Waves | Wiki | Perfect dodge, Dodge Counter, Echo, Intro/Outro, Vibration Strength immobilize | Medium | https://wutheringwaves.fandom.com/wiki/Combat |
| `wuwa_wiki_vibration` | Wuthering Waves | Wiki | Vibration Strength as enemy break/immobilize model | Medium | https://wutheringwaves.fandom.com/wiki/Vibration_Strength |
| `local_code_story_pve` | IsekaiBrawl local code | Repo | Current Story PvE phases, projectile intervals, telegraph lead, summon need hints, pressure relief constants | High | local files |

## Observed Production Data Shapes

### ZZZ: Daze turns pressure into a readable relief window

Public Daze data exposes:

- Daze accumulates per enemy until 100%.
- Stunned enemies temporarily stop acting and take increased damage.
- Heavy attacks on Stunned enemies start Chain Attack.
- Normal/Elite/Boss enemy strength changes how many Chain Attacks can happen.
- Public base rows include values such as:
  - small/common enemies around `600` Daze, `2s-6.5s` stun.
  - elite-sized enemies around `2168-4161` Daze, often `10s-13s` stun.
  - boss-sized enemies around `5543-10028` Daze, often `7s-12s` stun.
  - stun damage multiplier examples from `125%` to `200%`.

IsekaiBrawl application:

- Use `PressureBreak`, not copied Daze.
- Enemy strength should define how many correct actions are required before relief.
- Small enemy break: `0.6-1.4s` interrupt.
- Elite pressure break: `2-4s` pocket shift.
- Early boss pressure body: short interrupt or retreat handoff, not kill.
- Final boss stand: real HP kill plus summon contribution.

### ZZZ: BulletEmitter separates target, origin, bullet ability, effect, and lifetime

`Arlaune_Attack_01_BulletEmitter_01.json` exposes:

- `$type`: `BulletEmitterMixin`
- `AttackTargetPosRot`: refer to `SelfAttackTarget`, `AttachPoint: MiddlePoint`
- `BulletAbilityName`: `Arlaune_Attack_01_Bullet_01`
- `BulletInitPosRot`: refer to caster `HeadPoint`
- `EmmiterEffect.EffectPattern`: named path effect
- `OverrideEmitterLiftTime`: `7`

IsekaiBrawl application:

- Boss patterns should not be authored as direct damage calls.
- Split pattern data into `targeting`, `spawnOrigin`, `projectileAbility`, `telegraphEffect`, `activeLifetime`, and `cleanup`.
- This maps directly to current local `PveProjectileEmitter` fields: `emitterType`, `interval`, `leadTime`, `damage`, `usesWarningLine`.

### ZZZ: EnemyCheck shows field sensors driving AI params

`Bangboo_FightingCommon_EnemyCheck.json` exposes:

- `ActionsOnPropertyChangeMixin`
- custom property `EnemiesNum_InRange`
- `WriteAIParamAction` writes `Bool_NoEnermyInRange`
- `FieldRangeMixin`
- collider type `FanCylinder`
- angle `360`, radius `2.5`, height `2`
- field enter/exit actions increment/decrement the custom property
- `TotalDuration`: `-1`
- field follows owner

IsekaiBrawl application:

- Add enemy sensors as data rows, not one-off code.
- Useful first sensors:
  - `PlayerInCone`
  - `SummonInRange`
  - `NoTargetInRange`
  - `StructureInPressureRange`
  - `PlayerOverextended`
  - `BacklineThreatAlive`
- These should feed `BossPatternDeck` weights and `SummonNeedKind`.

### ZZZ: BossFullHealthLock is a boss state gate

`BossFullHealthLock.json` exposes:

- `LockLifePropertyMixin`
- `PerformBeHitEffect`: `true`
- `RatioLockList`
- `LockType`: `Total`
- `LockValue`: `0.95`

IsekaiBrawl application:

- This supports the current project direction: early boss presence can be pressure-only.
- Use boss lock/gate states intentionally:
  - `PressureOnly`: can be hit, shows reaction, cannot die.
  - `BreakOpen`: short retreat or summon-follow-up window.
  - `FinalStand`: real HP kill enabled.

### PGR: Affixes are compact enemy behavior modules

Public affix data and prior extraction expose:

- level-scaled vulnerability/resistance rows.
- damage down/up rows around `20/35/50%` or `30/45/60%`.
- distance-scaled haste examples around `15-30%`, `20-35%`, `25-40%`.
- no-control / super-armor style traits.
- regeneration examples around `0.1-0.3% max HP per second`.
- icon path and description separated from row identity.

IsekaiBrawl application:

- Enemy variety should come from readable modules:
  - `ShieldCycle`
  - `DistanceHaste`
  - `AuraBuffer`
  - `DeathBomb`
  - `RetreatBlink`
  - `SummonPackage`
  - `RegenAnchor`
  - `NoStaggerUntilBreak`
- Every module needs a preferred answer: Break, Tank, Arrow, Heal, perfect dodge, or ultimate.

### HI3: Monster config separates identity, stats, behavior, and variants

`MonsterConfigData.json` sample rows expose:

- `EliteType`
- `nature`
- `attack`
- `defense`
- `HP`
- `AIName`, such as `Behavior_BOSS_230`
- `categoryName`, such as `Boss`
- `configFile`, such as `BOSS_230_Config`
- `configType`, such as `Default`, `MainLine`, `Mirror_1`, `Default_SP01`
- `monsterName`
- `subTypeName`
- `typeName`, including `Easy`, `VeryEasy`, and tower variants

IsekaiBrawl application:

- Split authoring into:
  - `EnemyIdentity`
  - `EnemyStats`
  - `EnemyBehaviorPackage`
  - `DifficultyVariant`
  - `PresentationProfile`
- Do not pack boss phase, stats, UI icon, and AI behavior into one class.

### HI3: Stage monster/effect rows show ability and recommendation metadata

Observed stage detail rows expose:

- `BossType`
- `Nature`
- `ModelScale`
- `ControllerPath`
- `IconPath`
- `PrefabPath`
- `ModelPosition`
- `ModelRotation`
- `AbilitiesList`
- `ResistList`
- `RecommendTag`
- `UnRecommendTag`

Observed stage effect rows expose:

- `ID`
- `EffectText`
- `EffectIcon`

IsekaiBrawl application:

- Encounter definitions should own:
  - spawned enemy ids
  - spawned structure ids
  - hazard ids
  - recommended summon need
  - warning/effect icon keys
  - boss phase package
- Current `StoryPveEncounterDefinition` already has enemy, structure, hazard, cover, boss, pockets, and breakthrough markers. The missing piece is a data contract that names the intended pressure rhythm and summon answer.

### HI3: AttackPunish is a threshold difficulty curve

`AttackPunishData.json` exposes:

- level differences `0-6`: `0.0` damage reduction.
- difference `7`: `0.5`.
- difference `8`: `0.6`.
- difference `9`: `0.7`.
- difference `10`: `0.8`.

IsekaiBrawl application:

- v1 excludes meta progression, so do not use level-gap punishment.
- The usable pattern is "no hidden scaling until a clear threshold, then strong clamp."
- Apply this to pressure tuning:
  - below target pressure: no hidden damage correction.
  - repeated failure / late run: increase telegraph clarity or relief, not invisible stat cheats.
  - optional hard mode later: visible pressure tier, not hidden scaling.

### Wuthering Waves: Vibration Strength and Intro/Outro support break-to-entry grammar

Public combat pages describe:

- perfect timing dodge grants an immunity period.
- Basic Attack can become a Dodge Counter depending on timing/state.
- enemies enter immobilize when Vibration Strength is depleted.
- Intro/Outro skills trigger from full Concerto Energy.
- Echo can be summoned or transformed into to attack or buff.

IsekaiBrawl application:

- `BossPatternBreak` should open one of:
  - summon follow-up
  - empowered basic-fire
  - ultimate setup
  - retreat/advance gate
- Keep one summon slot, but let the summon behave like the entry receiver.

## Current Local Runtime Evidence

### Existing useful fields

Current code already has:

- `EnemyAI.EnemyPhase`: `Opening`, `Pressure`, `Siege`, `FinalPush`
- `BossTacticState`: `RearGuard`, `EscortWave`, `ContestMid`, `SiegeStructure`, `PunishOverextend`, `CommitPush`, `FallBack`
- `SummonIntentState`: `Probe`, `HoldLine`, `EscortPush`, `BreakPost`, `PunishHero`, `BaseRush`
- opening timing fields:
  - `openingSummonLeadTime`: `3.2`
  - `openingVolleyLeadTime`: `7.4`
  - `openingLineVolleyLeadTime`: `5.4`
  - `openingMinimumDuration`: `24`
  - `pressureMinimumDuration`: `72`
  - `siegeMinimumDuration`: `118`
- projectile timing fields:
  - `projectileInterval`: `3`
  - `lineProjectileInterval`: `4.6`
  - `directProjectileLockLeadTime`: `0.9`
  - `directProjectileGlobalRecovery`: `3`
  - `sharedProjectileRecovery`: `0.92`
  - `directProjectileExposureHoldTime`: `0.85`
- current Story PvE energy fields:
  - `maxEnergy`: `124`
  - `startingEnergy`: `28`
  - `baseChargeRate`: `1.75`
  - progress multiplier around `0.75 / 1.0 / 1.4`, or `1.65` under late extra pressure.
- current core summon variants:
  - Break cost `30`, damage `136`, structure multiplier `6.2`
  - Tank cost `46`, HP `520`
  - Arrow cost `26`, attack range `9.8`
  - Heal cost `26`, heal amount `22`, heal radius `5`
- current objective hints:
  - structure -> `Break`
  - low HP plus immediate pressure -> `Heal`
  - priority enemy -> `Arrow`
  - immediate pressure or boss pressure -> `Tank`

### Important mismatch to carry forward

Current documentation wants one good summon call or structure break to visibly shift the pocket within `2 to 3 seconds`.

Current runtime pressure relief constants are shorter:

- Tank spawn relief: `0.72s`
- blocker break relief: `0.9s`
- reward break relief: `1.05s`
- siege break relief: `1.2s`

This is not a code bug by itself, but it is a tuning mismatch for the next implementation pass. The next boss/enemy tuning pass should test `1.6-3.0s` relief envelopes and choose the shortest value that still reads as a pocket shift.

## Boss Pattern Deck Model

### Deck selection inputs

Use these inputs before adding more complex AI:

- `phase`
- `elapsedTime`
- `currentEncounterProgress01`
- `bossHpOrBreakRatio`
- `playerHpRatio`
- `structureCount`
- `priorityStructureAlive`
- `priorityEnemyAlive`
- `playerOverextended`
- `directThreatActive`
- `lineThreatActive`
- `lastPlayerSummonRole`
- `summonAnswerRecentlySucceeded`

### Pattern deck rows

| Pattern | Phase fit | Telegraph | Active threat | Best answer | Relief |
|---|---|---|---|---|---|
| `NeedleLock` | Opening | thin lock line, short cue | one aimed projectile | dodge, Tank if chained | short |
| `CoverFire` | Opening/Pressure | boss aims at pocket center | suppresses approach path | move sideways, Arrow backline | short |
| `LeftClamp` / `RightClamp` | Pressure | side warning + center shot | narrows safe space | dodge through, Tank | medium |
| `EscortScreen` | Pressure | summon/ally marker | supports enemy advance | Arrow support, Break blocker | medium |
| `PunishNet` | Pressure/Siege | player-centered net | punishes overextend | perfect dodge, Heal if damaged | medium |
| `AnchorCurse` | Siege | structure lock + circle | pressures structure/core | Break structure, Tank screen | medium |
| `LineBreaker` | Siege/Final | long warning line | forces lane-free reposition | dodge, Tank if trapped | medium |
| `SummonPackage` | Pressure/Siege | cast tell + spawn circle | adds support enemy | Arrow/Tank, clear support | medium |
| `PhasePullback` | any phase shift | boss silhouette hold | deck reset | reposition, choose summon | relief |
| `FinalCrush` | FinalPush | multi-line warning | final pressure burst | ultimate, Tank, perfect dodge | short after burst |

## Enemy Role Modules

| Module | Threat purpose | Telegraph | Best answer | Notes |
|---|---|---|---|---|
| `ShieldCycle` | blocks progress | shield aura, crack stages | Break | creates a clean Break reason |
| `AuraBuffer` | makes adds/boss stronger | aura ring, support marker | Arrow | priority target without target UI |
| `DistanceHaste` | punishes passive kiting | speed trail when far | move forward, Tank if caught | use lightly |
| `DeathBomb` | prevents autopilot cleanup | death ring delay | dodge | strong teaching tool |
| `RetreatBlink` | protects backline | blink tell, afterimage | Arrow | keep camera readable |
| `SummonPackage` | creates pocket overload | cast circle, add portal | Tank/Arrow | do not stack with final crush early |
| `RegenAnchor` | prevents slow chip | green tether, pulse | Break/Arrow | must be visibly interruptible |
| `NoStaggerUntilBreak` | guard/armor tutorial | armor shell | Break/ultimate | do not overuse |
| `ClosePunish` | stops face-hugging | long melee windup | perfect dodge | good boss tutorial |
| `LineCaster` | readable direct hazard | warning line | dodge/Tank | maps to current PveProjectileEmitter |

## Telegraph And Pressure Grammar

Every boss/enemy attack should have these stages unless it is a trivial chip attack:

1. `Intent`
   - top-band hint, boss pose, icon pulse, or short cast sound.
   - target range: `0.6-2.0s` before danger for major patterns.

2. `Windup`
   - visible line/circle/cone.
   - target range: `0.45-1.2s`.
   - current local direct projectile lead already uses about `0.9s`.

3. `Active`
   - projectile, line, melee burst, summon spawn, or structure lock.
   - should be readable without direct target selection.

4. `Recovery`
   - boss/add pauses, turns, or backs off.
   - target range: `0.35-1.0s`.

5. `Relief`
   - if player answers correctly, pressure dips.
   - target range: `1.6-3.0s` for visible summon/structure success.
   - `0.6-1.2s` for minor dodge-only relief.
   - auto basic-fire should read strongest here: the player keeps moving and aiming by direction while the boss/adds briefly stop escalating.

6. `Cleanup`
   - line/circle/effect/camera/HUD warning ends explicitly.

## 3 To 5 Minute Story PvE Run Shape

### Recommended first complete run

| Segment | Time | Goal | Pressure | Best summon read |
|---|---:|---|---|---|
| `Entry Read` | `0:00-0:20` | teach camera, move, first boss pressure | one aimed projectile or line | none/Tank hint only |
| `Pocket 1: Break Gate` | `0:20-1:05` | crack first structure/blocker | structure + light line pressure | Break |
| `Pocket 2: Backline` | `1:05-1:55` | clear support/ranged add | backline add + clamp shots | Arrow/Tank |
| `Pocket 3: Pressure Rescue` | `1:55-2:50` | survive boss/add pressure | line + direct threat + chip | Tank/Heal |
| `Boss Break Handoff` | `2:50-3:25` | create final approach | break/retreat window | Break/Arrow |
| `Final Stand` | `3:25-4:30` | kill boss | final deck, no endless setup | ultimate + any correct role |
| `Result Buffer` | `4:30-5:00` | cleanup, result, retry | no new pressure | none |

### Short vertical-slice run

Use when iteration time matters:

- `0:00-0:15`: entry read.
- `0:15-0:55`: Break gate.
- `0:55-1:35`: Arrow/Tank backline pressure.
- `1:35-2:10`: boss break handoff.
- `2:10-3:00`: final stand.

## First-Pass Tuning Envelopes

These are not final balance values. They are first-pass guardrails.

| Item | Start range | Reason |
|---|---:|---|
| Run length | `180-300s` | matches project brief |
| Encounter count | `3 + final stand` | enough to teach Break/Arrow/Tank/Heal |
| Major pattern telegraph | `0.6-1.2s` | readable mobile dodge |
| Minor projectile telegraph | `0.45-0.9s` | current code uses `0.9s` direct lead |
| Pattern active burst | `0.35-1.4s` | avoid long unavoidable danger |
| Pattern recovery | `0.35-1.0s` | lets player act |
| Perfect dodge cue | `0.18-0.40s` | from combo reference range |
| Perfect dodge reward | `1.0-4.0s` | start low to protect summon identity |
| Basic-fire beat window | `0.3-0.8s` | lets recovery/relief become sustained pressure without manual combo spam |
| Summon opportunity after dodge | `0.5-1.2s` | from summon reference |
| Boss break summon window | `1.5-3.0s` | summon follow-up clarity |
| Structure break tempo window | `2.0-4.0s` | reward should be felt |
| Success pressure relief | `1.6-3.0s` | current runtime likely too short |
| Player energy max | `120-135` | current Story PvE is `124` |
| Starting energy | `25-40` | current Story PvE is `28` |
| Passive energy rate | `1.6-2.2/s` | current Story PvE is `1.75/s` |
| Early boss pressure interval | `4.5-7.5s` | avoid immediate churn |
| Mid boss pressure interval | `3.5-5.5s` | stronger read loop |
| Final boss pressure interval | `2.4-4.2s` | intense but not spam |
| Tank relief | `1.8-3.0s` | should visibly buy room |
| Break relief | `2.0-3.5s` | opens path/pocket |
| Heal tempo field | `2.0-5.0s` | visible recovery/action field |
| Arrow suppression | `1.5-3.0s` | priority threat dip |

## Summon Answer Matrix

| Threat | Break | Tank | Arrow | Heal | Other answer |
|---|---|---|---|---|---|
| Shielded structure | primary | secondary screen | no | no | ultimate if ready |
| Boss guard/break part | primary | no | secondary weak point | no | perfect dodge setup |
| Direct projectile lock | no | primary | no | if low HP | dodge |
| Line/fan pressure | no | primary if trapped | no | if chip stacked | dodge/reposition |
| Backline ranged add | no | secondary | primary | no | medium aim bias |
| Aura support add | secondary | no | primary | no | boss break window |
| Boss overextend punish | no | primary after fail | no | if damaged | perfect dodge |
| Boss recovery after major pattern | no | no | secondary | no | auto basic-fire pressure |
| Death bomb | no | no | no | no | delayed dodge |
| Long final crush | secondary | primary | secondary | if low HP | ultimate |

## Recommended Data Contracts

### `BossRunDefinition`

- `id`
- `targetDurationSeconds`
- `encounterIds`
- `finalStandEncounterId`
- `globalPressureBudget`
- `summonTeachingOrder`
- `retryPolicy`

### `EncounterPressurePlan`

- `id`
- `timeRange`
- `primaryGoal`
- `pressureModules`
- `requiredSummonRead`
- `bossPhase`
- `reliefWindow`
- `advanceGate`
- `hudWarning`

### `BossPatternDeck`

- `id`
- `phase`
- `patterns`
- `selectionInputs`
- `repeatSuppression`
- `minimumCommitSeconds`
- `maximumCommitSeconds`

### `BossPattern`

- `id`
- `threatShape`
- `intentCue`
- `telegraphCue`
- `windupSeconds`
- `activeSeconds`
- `recoverySeconds`
- `cooldownSeconds`
- `targetingRule`
- `bestAnswers`
- `cameraCue`
- `cleanupCue`

### `EnemyRoleModule`

- `id`
- `role`
- `affixStyle`
- `sensorInputs`
- `priorityScore`
- `summonNeed`
- `telegraph`
- `failureRisk`
- `spawnBudget`

### `PressureReliefRule`

- `trigger`
- `minimumSeconds`
- `maximumSeconds`
- `suppressedPatternTypes`
- `cameraCue`
- `hudCue`
- `cancelConditions`

## First Adoption Recommendations

1. Build `BossPatternDeck` as data before adding more boss code.
   - Use current phase names first: `Opening`, `Pressure`, `Siege`, `FinalPush`.
   - Move current pattern names into rows: `NeedleLock`, `LeftClamp`, `RightClamp`, `AnchorCurse`, `PunishNet`, `FinalCrush`.

2. Add a visible `PressureReliefRule`.
   - Start by raising Tank/structure success relief toward `1.6-2.4s`.
   - Test whether it creates the documented `2-3s pocket shift`.

3. Make `SummonNeedKind` a first-class authoring field on encounters.
   - Current runtime infers needs from structure/enemy/threat.
   - The run designer also needs to state the intended lesson for each pocket.

4. Draft one complete 3 to 5 minute run table.
   - Do not expand enemy variety until this table feels playable.

5. Keep boss early phases pressure-only.
   - Use lock/break/retreat states until final stand.
   - Do not let the player chip-kill the boss before the final stand.

## Next Small Tasks

1. Draft `BOSS_COMBAT_SPEC.md` with concrete `BossPatternDeck`, `EncounterPressurePlan`, and `PressureReliefRule` schemas.
2. Convert current local `EnemyAI` phase/pattern names into a data table without changing behavior yet.
3. Tune and test Tank/structure relief windows against the documented `2-3s pocket shift` target.
