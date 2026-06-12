# ARPG Reference Research

Last updated: 2026-06-11 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` v1 Story PvE. It was written after reading:

1. `PROJECT_BRIEF.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. `AGENTS.md`
5. `AI_CODE_CONTRACT.md`
6. `ARCHITECTURE_BOUNDARIES.md`
7. `HUD_COMBAT_SPEC.md`

Active extraction rule: reference games are used to extract concrete development data such as camera movement patterns, timing windows, state transitions, enemy pattern structure, hit-stop feel, screen-effect layering, and observed tuning ranges. The boundary is asset/code ownership: do not import proprietary meshes, textures, animation clips, audio, source code, or full raw tables as project assets.

## 2026-06-11 Direction Correction

The root project documents now include a `2026-06-11` override for `landscape / horizontal ARPG-facing Story PvE`. Older vertical/corridor sections may remain as historical context, but they are no longer the active implementation constraint.

Until the root source-of-truth documents are updated, this research document follows the latest user correction for reference extraction:

- Treat horizontal ARPG camera, soft lock-on, target-relative framing, and short target-centered impact shots as valid candidates.
- Treat melee combo timing, dodge counter, assist/QTE-like summon entry, boss pattern phases, and stronger VFX layering as useful reference areas.
- Keep summon-first identity as an advantage, but do not force every reference back into a narrow vertical corridor interpretation.
- Avoid old prototype assumptions such as lane-first input and hand-of-cards UI unless the docs are deliberately changed later.
Use concrete patterns aggressively. Camera movement patterns, lock-on behavior, timing, state graphs, enemy pressure loops, and effect layer recipes are valid reference data. The hard limit is direct asset/code reuse, not learning from how another action game is built.

## Scope Guard

This document is a seed catalog, not a scope fence.

The named games and sources are enough to start work, but they must not narrow future research. Main-PC follow-up may freely add more ARPG references, footage studies, frame tags, community databases, GitHub datasets, official guides, and extracted schema observations when they help camera, animation, enemy AI, VFX, lock-on, dodge/counter, boss phase, or summon/assist implementation.

The current source set should be read as `includes PGR / Honkai Impact 3rd / Zenless Zone Zero / Silver Palace / Wuthering Waves / Aether Gazer`, not `limited to these games`.

## Notion Alignment

Notion search found:

- `notion_pgr_page`: only `UI` content was present, so it did not add combat baseline data.
- `notion_zzz_evelyn_entry_camera`: useful as a future camera-beat note. Its own page says the original was not directly inspected and should later be reduced to camera start position, movement direction, emphasis moment, and character scale timing.

No existing Notion page was found that overrides the repo baseline for this ARPG reference task.

## Source Inventory

| ID | Game | Source type | Useful data | Confidence | Notes |
|---|---|---:|---|---:|---|
| `pgr_wiki_combat_guide` | Punishing: Gray Raven | Wiki | 3-Ping, QTE, Matrix, shield, class roles, matrix cooldown | Medium | Good for mechanic relationships, not raw frame data. |
| `pgr_wiki_matrix` | Punishing: Gray Raven | Wiki | Perfect dodge triggers brief time-slow counter window | Medium | Confirms Matrix fantasy. |
| `pgr_github_tab` | Punishing: Gray Raven | GitHub data/code | Camera param tables, NPC animation action names, NPC affix table | Medium | Public repo is archived; use patterns, schema, and observed ranges as reference. |
| `hi3_wiki_combat` | Honkai Impact 3rd | Official wiki | Basic attack sequence/cancel model, evasion, block, parry, QTE, ultimate categories | Medium | Useful for input grammar and cancel windows. |
| `hi3_wiki_time_fracture` | Honkai Impact 3rd | Official wiki | Time Fracture slows time/timers and gates QTEs | Medium | Useful for dodge reward and timed content pressure. |
| `hi3_github_data` | Honkai Impact 3rd | GitHub data | Monster config, boss data, skill/effect tables, attack punish data | Medium | Public extracted data; use patterns, schema, and observed ranges as reference. |
| `zzz_wiki_daze` | Zenless Zone Zero | Official wiki | Daze formula, enemy-specific daze gauge, stun duration, stun damage multiplier | High | Best public structured enemy-readability data found. |
| `zzz_wiki_dodge` | Zenless Zone Zero | Official wiki | Dodge, Perfect Dodge, Dodge Counter, dash projectile deflect rules | High | Directly useful for dodge reward grammar. |
| `zzz_wiki_chain_attack` | Zenless Zone Zero | Official wiki | Chain Attack, Bangboo chain, Ultimate, invulnerability notes | High | Useful for summon/assist and burst timing. |
| `zzz_github_data` | Zenless Zone Zero | GitHub data | Ability camera modifiers, WitchTime dodge, bullet emitters, enemy check, buff/effect data | Medium | Public extracted data; use patterns, schema, and observed ranges as reference. |
| `silver_palace_polygon` | Silver Palace | News / preview | Dynamic melee + third-person shooting, real-time party switching, grappling/traversal claims | Low | Unreleased game; no reliable raw combat data located. |
| `wuwa_wikipedia_gameplay` | Wuthering Waves | Web encyclopedia | Intro/Outro skills, Echo summon/transform, open-world ARPG traversal, dodge/parry, Tune Break | Medium | Useful horizontal ARPG comparison from another Kuro action title. |
| `aether_gazer_fandom_main` | Aether Gazer | Wiki | Team-fight RPG, combo/squad/Modifier framing | Low | Thin source, but useful as a team-action positioning reference. |

## Raw-Source Path Catalog

These are the fastest follow-up paths for deeper extraction. They should be treated as reference databases, not imported project assets.

| Game | Repo/source | High-value paths | What to extract next |
|---|---|---|---|
| ZZZ | `360NENZ/Dimbreath-ZenlessData` | `Data/*_Camera.json`, `Data/*CameraEvent.json`, `Data/*CameraOverrideTrack.json` | animation-state camera windows, zoom/stretch/override cue keys |
| ZZZ | `360NENZ/Dimbreath-ZenlessData` | `Data/BUFF_WitchTime*.json`, `Data/Common_WitchTime.json` | dodge reward duration, slow layers, invulnerability, cleanup events |
| ZZZ | `360NENZ/Dimbreath-ZenlessData` | `Data/*Bullet*.json`, `Data/*BulletEmitter*.json` | projectile speed/count/spawn pattern, emitter fan/line shape |
| ZZZ | `360NENZ/Dimbreath-ZenlessData` | `Data/*EnemyCheck.json`, `Data/*HitTimesManager.json` | target checks, hit count gates, enemy condition logic |
| PGR | `Kengxxiao/Punishing_GrayRaven_Tab` | `table/client/camera/CameraParam.tab` | distance/angle/viewport offset clamps per camera context |
| PGR | `Kengxxiao/Punishing_GrayRaven_Tab` | `table/client/fight/npc/NpcAnimatorAction.tab` | action taxonomy and trigger/state split |
| PGR | `Kengxxiao/Punishing_GrayRaven_Tab` | `table/client/fight/npc/NpcAffix.tab` | modular enemy behavior library and tuning ranges |
| PGR | `Kengxxiao/Punishing_GrayRaven_Tab` | `table/client/fight/ScreenEffect.tab`, `table/client/fight/RoleEffect.tab` | screen/role effect naming and event separation |
| HI3 | `nairieberry/HonkaiImpactData` | `Global/ExcelOutputAsset/MonsterConfigData.json` | AI name/config split, category, stats, subtype identity |
| HI3 | `nairieberry/HonkaiImpactData` | `Global/ExcelOutputAsset/ExBossMonsterData.json`, `ExbossSkillTips.json` | boss config, boss skill tip taxonomy |
| HI3 | `nairieberry/HonkaiImpactData` | `Global/ExcelOutputAsset/StageDetail_Monster.json`, `StageDetail_Effect.json` | encounter spawn/effect composition |
| HI3 | `nairieberry/HonkaiImpactData` | `Global/ExcelOutputAsset/AvatarAttackSpeedParam.json`, `AvatarSkillData.json`, `AvatarSubSkillData.json` | animation speed assumptions, skill categories |
| HI3 | `nairieberry/HonkaiImpactData` | `Global/ExcelOutputAsset/AttackPunishData.json` | threshold-based difficulty clamp curve |

## Camera Findings

### ZZZ: animation-state-bound camera cues

The `Dimbreath-ZenlessData` mirror includes `Data/Anton_Camera.json`. Its structure is useful because camera behavior is attached to animator states, not just global combat state:

- `AnimatorStateName`: examples include normal-enhance attacks, counter, assist, QTE, EX QTE.
- Time gates: `FrameCountHigh`, `FrameCountLow`, `NormalizedTimeHigh`, `NormalizedTimeLow`.
- Modifier names: attack/counter/QTE camera modifiers.
- Added/removed actions: `CameraZoomAction`, `CameraStretchAction`, `CameraOverrideTrackEndAction`, `InterruptShaderCustomAction`.

Observed sample windows:

| Animator state | Window | Modifier | Presentation actions |
|---|---:|---|---|
| `Attack_Counter_02` | frame 0-41 | `Counter02CameraModifier` | zoom + stretch |
| `Attack_Counter_02_End` | frame 0-3 | `Counter02CameraModifier` cleanup | zoom/stretch end |
| `Attack_BeHitAid_Enhance` | frame 0-41 | `BeHitAidEnhanceCameraModifier` | zoom + stretch |
| `Attack_BeHitAid_Enhance_End` | frame 0-3 | `BeHitAidEnhanceCameraModifier` cleanup | zoom/stretch end |
| `SwitchIn_Attack_03` | frame 0-20 | `QTECameraModifier` | QTE camera cleanup includes zoom/stretch/override-track end |
| `SwitchIn_Attack_Ex_05` | frame 0-18 | `ExQTECameraModifier` | EX QTE camera cleanup includes zoom/stretch and shader fog interrupt |

Practical read: commercial action camera cues are often measured in tens of animation frames, not arbitrary seconds. For a 60 FPS target, 18-41 frames roughly maps to `0.30-0.68s`, a useful first range for counter, assist, and heavy-hit emphasis before retuning.

IsekaiBrawl application:

- Add a small data contract later for `CombatCameraCue`.
- Cue source should be animation/combat event driven: `DodgePerfect`, `SummonCall`, `UltimateStart`, `StructureBreak`, `BossRetreat`.
- The cue should be additive over the authored `BattleCamera` scene values.
- For the horizontal ARPG direction, allow short target-relative pushes, soft lock-on pivots, and impact zooms when they improve readability.
- Avoid only the bad version: a persistent free orbit that hides threats, confuses input direction, or breaks mobile readability.

### PGR: tabular per-context camera parameters

The PGR data repo includes `table/client/camera/CameraParam.tab` with columns like:

- `IsDragOrRotate`
- `IsTweenCamera`
- `AllowZoom`
- `Distance`, `MinDistance`, `MaxDistance`
- `TargetAngleX`, `TargetAngleY`
- `OffsetViewportX`, `OffsetViewportY`

Most visible samples are UI/dorm/room contexts rather than combat. Still, it confirms a useful production pattern: camera behavior is table-driven by context and can be bounded per context.

Observed sample rows:

| ID | Distance | Min/Max distance | TargetAngleX | TargetAngleY | OffsetViewportX/Y | Notes |
|---|---:|---:|---:|---:|---:|---|
| `UIdomitory` | 16 | 0.2 / 50 | 0 | 0 | 8.99 / 0.18 | allows drag, tween camera |
| `UiRoomCharacter` | 3.51 | 0 / 6 | 0 | 0 | -0.24 / -0.73 | close character view |
| `DormTest` | 8 | 0.2 / 75 | 0.04079235 | 27 | 0 / 0 | allows X/Y axis |
| `sushe003` | 65.6 | 0.2 / 130 | 294.3 | 38 | 0 / 1.62 | broad room camera |
| `Room` | 9.5 | 9 / 11 | 269.8688 | 22.69049 | 0 / 0 | zoom allowed, tight distance bounds |

Practical read: even non-combat camera tables expose a strong pattern: each camera context defines distance clamps, angle clamps, axis permissions, and viewport offsets. IsekaiBrawl should do the same for horizontal combat presets.

IsekaiBrawl application:

- Treat `BattleCamera` scene values as the base context.
- Use cue-driven values only for short-lived emphasis.
- Store later camera tuning as small named presets, not scene-name branches in runtime code.
- Add landscape-oriented presets such as `ExplorationFollow`, `CombatSoftLock`, `BossPressure`, `DodgeCounter`, `SummonEntry`, and `UltimateImpact`.

### Honkai Impact 3rd: camera data exists but is weaker for our slice

`HonkaiImpactData` includes `HybridSiteCamera.json` with `CameraSite`, `ChapterID`, `HybridSiteID`, and `OffsetX`. This is more stage/site camera data than moment-to-moment combat camera evidence.

IsekaiBrawl application:

- Use Honkai mainly for input/cancel/evasion grammar.
- Use ZZZ/PGR more for cue tables and readable dodge-time reward.

### Practical camera movement patterns to extract

These are not protected assets. They are reusable movement/timing patterns that should be studied from footage, data tables, and captured clips.

| Pattern | Source inspiration | What to measure | IsekaiBrawl use |
|---|---|---|---|
| `CombatSoftLockFollow` | ZZZ, Wuthering Waves, Honkai | camera yaw lag, target re-center speed, player screen offset | default landscape ARPG combat camera |
| `AttackStateZoom` | ZZZ `Anton_Camera.json` | zoom start frame, end frame, return blend | basic heavy, summon impact, counter hit |
| `CounterSnap` | PGR Matrix, ZZZ dodge counter | snap duration, target screen position, hit-stop overlap | perfect dodge counter and parry reward |
| `AssistEntryPush` | ZZZ QTE / Chain Attack, Wuthering Intro | camera target swap timing, entry angle, exit cleanup | summon entry or helper strike |
| `BossPhasePullback` | Honkai boss presentation, ZZZ boss stun windows | pullback distance, boss silhouette hold, player control lock | boss phase shift / pattern deck change |
| `ProjectileThreatBias` | ZZZ projectile/bullet data | threat source priority, offscreen warning angle | keep ranged threats readable in horizontal view |
| `UltimateImpactHold` | ZZZ Ultimate, Honkai Ultimate | pause length, field of view delta, screen effect stack | one ultimate with strong but short presentation |

## Animation Findings

### PGR: enemy animation action taxonomy

`table/client/fight/npc/NpcAnimatorAction.tab` lists action IDs such as:

- Stand, Run, Walk, Born, Death
- Hit1/Hit2/Hit3/Hit4
- Behitfly, Hoverhit, Falldown, Hitdown, Standup
- TurnLeft, TurnRight
- Attack01 through Attack59 ranges
- Move1 through Move3
- AttackQte

Each row has `IsTrigger`, which separates passive state-like actions from event-driven animation triggers.

IsekaiBrawl application:

- For enemies and summons, define `ActionKind` and `TriggerMode`.
- Minimum useful taxonomy:
  - `Idle`, `Advance`, `Retreat`, `Strafe`, `Windup`, `Attack`, `Recover`, `HitReact`, `Stagger`, `Death`, `SummonCall`, `QteLikeAssist`
- With the horizontal ARPG direction, the taxonomy can grow when it directly supports melee, dodge-counter, boss phase, or summon-entry readability.

### Honkai: sequence attacks with cancel points

The Honkai combat page distinguishes basic attack sequences and notes that sequence-based basic attacks can be cancelled by evasion, ultimate, or special skills.

IsekaiBrawl application:

- Auto basic fire can still have visible loop beats, but the horizontal ARPG direction should also allow stronger authored melee or short-skill animation beats if the input model expands.
- Dodge and ultimate should be allowed to interrupt basic-fire presentation cleanly.
- Summon call should have its own short commitment window, but must not trap the player through a dodge-critical moment.

### ZZZ: camera and effect hooks are tied to animation states

ZZZ data suggests a strong pattern: animation state windows are the authority for when camera zoom, stretch, shader, and screen effects begin and end.

IsekaiBrawl application:

- Later, bind `UltimateStart`, `DodgeSuccess`, and `SummonImpact` presentation to animation or combat cue windows.
- Keep effect end actions explicit. A cue that starts zoom/screen effects must also own its cleanup.
- Add ARPG-specific cue candidates: `PerfectCounter`, `Launch`, `SlamImpact`, `GuardBreak`, `AssistEntry`, `BossPhaseShift`.

### Wuthering Waves: swap-entry and echo-action grammar

Wuthering Waves is useful because it is also a Kuro action title and its public gameplay description emphasizes ARPG combat more than the old IsekaiBrawl vertical slice did:

- Intro skills trigger when a character enters combat or swaps in.
- Outro skills trigger when a character leaves combat or swaps out.
- Echoes can briefly summon or transform into monsters and provide combat stats.
- Traversal and combat include aerial movement, dodges, and parries.
- A later Tune Break-style feature is described as an enemy special state after repeated attacks, allowing a damaging interrupt/cancel action.

IsekaiBrawl application:

- Treat summon entry as a cousin of `IntroSkill`, not only a spawned pet.
- Treat summon exit or cooldown handoff as a cousin of `OutroSkill`.
- Let some summons behave like short Echo actions: a quick monster/ally manifestation with a clear hit, shield, heal, or interrupt.
- Use Tune Break as a model for `BossPatternInterrupt`: repeated correct pressure opens a short interrupt/counter window.

## Enemy AI And Pattern Findings

### ZZZ: Daze as a public enemy-readability layer

The ZZZ wiki has concrete enemy Daze gauge values, stun durations, and stun damage multipliers. Examples captured:

| Enemy | Daze gauge | Stun duration | Stun damage multiplier |
|---|---:|---:|---:|
| Guard Jaeger | 600 | 6.5s | 125% |
| Rookie Jaeger | 600 | 6.5s | 150% |
| Dead End Butcher | 5991 / 5543 variants | 12s | 150% |
| Unknown Corruption Complex | 7189 / 5947 variants | 12s | 150% |
| Ahriman | 600 | 2s | 200% |
| Dullahan | 3502 | 10s | 150% |
| Hati | 2168 | 10s | 200% |
| Arlaune | 2335 | 7s | 200% |

IsekaiBrawl application:

- Treat observed values as reference data. We can compare gauges, stun windows, cooldowns, camera durations, and effect layer order to understand production scale.
- Retune values for IsekaiBrawl instead of importing exact source tables.
- Use the model: enemy strength controls how many actions are needed to earn a relief window.
- Convert to a `PressureBreak` model:
  - Small add: short interrupt/stagger.
  - Elite/summon problem: 2 to 4 second pocket shift.
  - Boss pressure body: early phases are not killable; only pressure can be interrupted briefly.
  - Final boss stand: real HP kill plus summon contribution.
- For the horizontal ARPG direction, also treat stun/daze as a real melee opportunity window: launch, back attack, counter chain, or summon assist can become valid follow-up actions if later input scope supports them.

### PGR: affixes as compact enemy behavior modules

The PGR NPC affix table contains modular enemy behaviors. Useful patterns:

- Element/physical vulnerability or resistance levels.
- Damage reduction and damage amplification levels.
- Movement haste that scales with distance to the target.
- Shield cycle: periodic shield based on max HP.
- Berserk: attack frequency increase by level.
- Death bomb.
- Rigid body: incoming damage becomes 1 until hit-count threshold breaks it, then recovers after a delay.
- Radiation aura.
- Distance-based damage reduction from far attackers.
- Nearby ally buff.
- Lone-unit buff.
- Resist shred on hit with stack duration.
- Retaliatory lightning at attacker position with cooldown.
- Invisibility after a delay, revealed on hit.
- Chase teleport when far, retreat teleport when close.
- Split into copies on death.
- Periodic black hole.
- Eight-direction laser.
- Elemental on-hit status: burn, freeze, stun, lightning mark.
- Healing reduction or healing disable.
- Temporary barrier/cage.

IsekaiBrawl application:

- Excellent source for Story PvE enemy modules, because it maps directly to “why call this summon now”.
- Recommended first subset:
  - `ShieldCycle`: creates a Break summon reason.
  - `DistanceHaste`: punishes passive rear camping and rewards forward pressure.
  - `DeathBomb`: creates dodge/advance timing.
  - `AllyAura`: makes Arrow or Break target priority meaningful.
  - `RetreatBlink`: boss/support unit reposition grammar.
  - `BlackholeLite`: lane-free area denial, only if readable.
- Higher-ARPG candidates to keep on the table:
  - `ParryBait`: delayed single hit that teaches counter timing.
  - `ArmorBreak`: enemy resists stagger until a shield/guard meter breaks.
  - `Launcher`: pop-up threat that makes air/ground recovery rules matter.
  - `ClosePunish`: boss punishes face-hugging to keep movement alive.
  - `RetreatShot`: ranged boss step-back plus projectile pressure.
  - `PhaseSwap`: boss changes pattern deck at HP or pocket state thresholds.
- Watchlist, not banned:
  - Invisibility, eight-direction laser spam, rigid body, clone split, full cage. These are usable only if readability and camera framing are already stable.

### Honkai Impact 3rd: monster rows expose AI names and config separation

`MonsterConfigData.json` sample rows include:

- `AIName`, such as `Behavior_BOSS_230`.
- `categoryName`, such as `Boss`.
- `configFile`, such as `BOSS_230_Config`.
- Stats such as `HP`, `attack`, `defense`.
- Subtype/type naming.

IsekaiBrawl application:

- Split enemy authoring into:
  - identity row: visible name, role, category
  - stats row: HP, attack, stagger/pressure break
  - behavior row: AI package / pattern deck
  - presentation row: telegraph, camera cue, VFX cue
- For horizontal ARPG, add optional rows for lock-on radius, orbit distance, melee punish range, camera threat priority, and phase thresholds.

### Honkai AttackPunishData: level gap as damage reduction

`AttackPunishData.json` includes a simple level-difference curve:

- Difference 0 through 6: `0.0` damage reduction.
- Difference 7: `0.5`.
- Difference 8: `0.6`.
- Difference 9: `0.7`.
- Difference 10: `0.8`.

IsekaiBrawl application:

- If v1 still excludes meta progression, do not use level-gap punishment.
- If the horizontal ARPG scope adds stronger RPG tuning, the threshold curve is useful as a difficulty clamp: avoid subtle scaling until a clear threshold, then escalate strongly.

### Practical enemy AI pattern deck

These are implementation-facing pattern modules distilled from PGR affixes, ZZZ Daze/stun, Honkai monster config separation, and Wuthering Waves interrupt/swap grammar.

| Module | Inputs | State flow | Camera/VFX needs | Good player answer |
|---|---|---|---|---|
| `LinePressure` | target position, lane-free direction vector, cooldown | `Idle -> Windup -> FireLine -> Recover` | line telegraph, mild camera threat bias | side dodge, Tank screen |
| `FanPressure` | target position, spread angle, projectile count | `Windup -> FanShot -> Recover` | fan warning, projectile readability | forward/back reposition |
| `ClosePunish` | player distance below threshold | `Track -> Windup -> MeleeBurst -> Backstep` | target snap only on windup | dodge out, counter after burst |
| `RetreatShot` | player too close or boss phase rule | `Backstep -> Aim -> Shot -> Recenter` | camera maintains boss + projectile | chase carefully, Arrow pressure |
| `ShieldCycle` | shield cooldown, shield HP/hit count | `Normal -> Shielded -> BreakStagger -> Normal` | shield crack, guard-break camera kick | Break summon / heavy hit |
| `AuraBuffer` | nearby ally count, aura radius | `Anchor -> BuffPulse -> Reposition` | aura ring, priority marker | Arrow or focused summon |
| `SummonPackage` | encounter phase, boss cooldown | `CastTell -> SpawnAdds -> CommandAdds -> Recover` | summoning circle, top-band warning | Tank/Arrow, clear support |
| `DeathBomb` | enemy death event, delay | `Death -> BombTell -> Explosion` | ground ring, post-kill warning | dodge after kill |
| `ParryBait` | player aggression, cooldown | `HoldPose -> DelayedStrike -> Vulnerable` | long windup, hit flash | perfect dodge / counter |
| `ArmorBreak` | guard meter, incoming hit type | `Guarded -> Cracked -> Stagger -> Recover` | crack stack stages | Break/heavy/summon impact |
| `Launcher` | melee range, phase | `Windup -> LaunchHit -> AirThreat -> Recover` | launch arc cue, recovery cue | dodge, air recovery/counter later |
| `PhaseSwap` | HP threshold, time, or pocket clear | `EndPattern -> PhaseTell -> NewDeck` | pullback, color shift, boss silhouette | reset positioning, choose summon |

## Dodge, Time Slow, And Counter Reward

### PGR Matrix

PGR Matrix triggers from a precisely timed dodge. The combat guide records:

- Matrix cooldown: 15 seconds.
- Matrix-Ping: during Matrix, flashing orbs can be treated as a 3-Ping.
- Separate characters can bypass Matrix cooldown by swapping.

IsekaiBrawl application:

- Our current documented v1 does not have orb ping or character swap.
- Translate to: `PerfectDodgeWindow` grants a short `safe call / energy boost / aim clarity` benefit.
- Recommended first implementation target:
  - Perfect dodge grants brief local time emphasis and small energy pulse.
  - If a summon is ready, top-band oracle can flash the next pressure answer for under 1 second.
- If the horizontal ARPG input model expands, Perfect Dodge may also enable a counter hit, assist entry, or short target lock-on snap.

### Honkai Time Fracture

Time Fracture slows time and even slows round timers. It can be triggered by specific battlesuit skills such as evasion and is required for specific QTEs.

IsekaiBrawl application:

- Avoid timer manipulation in v1.
- Use time slow only as presentation and reaction clarity.
- Keep timer fair and readable.
- For landscape ARPG, time-slow can be a stronger style beat around parry, perfect dodge, or ultimate, as long as it does not hide enemy follow-up telegraphs.

### ZZZ WitchTime Dodge

`BUFF_WitchTime_Dodge.json` gives a concrete 4-second `DurationTime` and applies:

- Team avatar modifier.
- Witch slow down mixin.
- Invincible buff.
- Screen effect layers: vignette, radial blur, VR effects, screen effects, FX fog, color adjustments.
- Explicit interruption/removal for screen effects.

Observed sample values:

| Field | Value | Use |
|---|---:|---|
| `AbilitySpecials.DurationTime` | 4s | long dodge reward reference |
| `TeamEntityModifier.Duration` | 30s | team-level modifier management window |
| `WitchModifier.Duration` | -1 | manually removed state |
| `InvincibleBuffModifier.Duration` | -1 | attached/removed invulnerability |
| `WitchSlowDownMixin.TotalDuration` | -1 | slow-down lives until explicit removal |

IsekaiBrawl application:

- Use the architecture, not the exact 4-second duration.
- The observed 4-second duration is still meaningful reference data: it shows that a commercial ARPG can afford a long stylized dodge reward when the surrounding system supports it.
- Our first target can be shorter, but the horizontal ARPG direction allows stronger momentary emphasis than the old corridor note:
  - `0.15-0.35s` hit-stop or local time emphasis on successful dodge.
  - `0.6-1.2s` reduced pressure window after successful summon/pocket action.
  - Explicit cleanup for every screen effect.

## Effects Findings

Useful production pattern from ZZZ:

- Treat VFX/screen effects as named layers with start and end actions.
- Keep effect type explicit: vignette, radial blur, color adjustment, fog, screen overlay.
- Use combat event keys for global events.

Useful production pattern from PGR:

- Store screen effects and role effects as small tables.
- Keep enemy affix icons/descriptions separate from behavior data.

IsekaiBrawl application:

- Start with a readable effect budget, then scale up for horizontal ARPG impact:
  - `DodgeSuccess`: thin flash + micro slow + dodge-button pulse.
  - `SummonCall`: bottom-right card pulse + spawn ring.
  - `StructureBreak`: directional shock + energy burst toward the resource UI.
  - `BossPressure`: top-band danger pulse + projectile telegraph, not full-screen clutter.
  - `Ultimate`: short camera cue + strong silhouette/impact, then fast cleanup.
- Add ARPG candidates:
  - `PerfectCounter`: hit-stop + target snap + slash arc.
  - `GuardBreak`: shield crack mesh/VFX + enemy stagger pose + short camera kick.
  - `AssistEntry`: character/summon streak line + landing impact.
  - `BossPhaseShift`: arena lighting pulse + camera pull/rotate + boss silhouette hold.

### Practical VFX layer recipes

| Cue | Layers | Timing notes | Cleanup |
|---|---|---|---|
| `PerfectDodge` | silhouette smear, short radial blur, button pulse, optional color shift | start on dodge success; 0.15-0.35s hit-stop; optional 0.6-1.2s clarity tail | interrupt blur/color shift explicitly |
| `PerfectCounter` | target snap, slash arc, hit spark, enemy pose snap | counter starts immediately after dodge window; camera returns before next threat | clear target snap and hit-stop flag |
| `SummonEntry` | card pulse, spawn circle, entry streak, landing dust | entry readable within 0.2-0.5s; impact before 1s | remove spawn circle and streak |
| `GuardBreak` | shield crack decals, burst ring, stagger flash, camera kick | crack stacks can build over hits; break impact gets stronger cue | clear shield layer, leave short stagger state |
| `BossPhaseShift` | screen desaturation or color grade, boss aura, camera pullback, phase title pulse | 1-2s presentation can be acceptable if control state is clear | restore color grade and camera preset |
| `UltimateImpact` | FOV push, strong bloom/flash, silhouette hold, shockwave, final hit spark | can be the strongest cue; must not leave long clutter after damage | explicit end event for every screen layer |

## Silver Palace Note

Only preview-level information was located. Polygon reported the announcement as an anime-inspired action RPG with dynamic melee combat, third-person shooting, real-time party switching, investigation, grappling/traversal, and no confirmed release date or platform at the time of that article.

This is not enough for raw combat data. Keep Silver Palace as a high-level ARPG direction reference, especially for party-switch, melee/shooter blend, and traversal mood, but do not treat it as a data-backed mechanics baseline yet.

## Recommended IsekaiBrawl Reference Model

### 1. Camera Cue Model

Use this later as a ScriptableObject or serializable table:

| Field | Meaning |
|---|---|
| `cueId` | Stable key, e.g. `DodgeSuccess`, `SummonBreakImpact`. |
| `trigger` | Combat/animation event source. |
| `duration` | Short cue duration. |
| `priority` | Prevent ultimate and summon cue conflict. |
| `zoomDelta` | Additive, not absolute camera replacement. |
| `offsetDelta` | Additive offset that preserves target/threat visibility. |
| `shakeProfile` | Optional camera impulse. |
| `screenEffectProfile` | Optional named effect layer. |
| `cleanupPolicy` | Always explicit. |

### 2. Enemy Pattern Module Model

Use modules instead of bespoke enemies:

| Module | Purpose | v1 fit |
|---|---|---|
| `LinePressure` | Teaches dodge side-step timing. | High |
| `FanPressure` | Tests left/right and distance positioning. | High |
| `SummonPackage` | Boss calls sparse support pressure. | High |
| `ShieldCycle` | Break summon reason. | High |
| `AuraBuffer` | Arrow/target-priority reason. | Medium |
| `DistanceHaste` | Anti-camping pressure. | Medium |
| `DeathBomb` | Push timing and dodge timing. | Medium |
| `RetreatBlink` | Boss/support reposition. | Medium |
| `BlackholeLite` | Area denial. | Later |
| `CloneSplit` | Noise-heavy. | Later |
| `FullInvisibility` | Readability risk. | Later |

### 3. Summon-First Translation

Reference games often center the player character. IsekaiBrawl should translate their combat grammar through summons:

- ZZZ Daze -> `PressureBreak` / pocket relief window.
- PGR Matrix -> perfect dodge gives energy/call clarity, not a new input mode.
- Honkai QTE -> summon call moment and short support appearance, not character swap.
- PGR affixes -> enemy modules that create readable reasons to call Break/Tank/Arrow/Heal.
- Wuthering Waves Intro/Outro -> summon entry and exit/handoff timing.
- Wuthering Waves Echo -> short-lived summon manifestation or transform-like strike.

### 4. Horizontal ARPG Control Translation

If the game is now landscape/horizontal, the reference extraction should support more ARPG behavior than the old vertical slice:

- Soft lock-on is allowed as camera/aim assist.
- Hard target selection is still optional, not required.
- Player-facing movement can remain simple, but camera should understand current target, active attacker, boss, and projectile source.
- Dodge counter and guard break are valid expansion points.
- Summons can be both persistent helpers and short assist entries.
- Bosses should be authored as pattern decks with phase transitions.
- Hit presentation should pair animation pose, VFX, hit-stop, and camera cue, not just particles.

## Next Implementation Candidates

1. Draft `CombatCameraCue` data contract and map it to existing `BattleCamera` without changing the camera authority rule.
2. Draft `StoryPveEnemyPatternModule` design page with first-pass horizontal ARPG modules: `LinePressure`, `SummonPackage`, `ShieldCycle`, `AuraBuffer`, `DeathBomb`, `ClosePunish`, `ParryBait`, and `PhaseSwap`.
3. Add a `PerfectDodgeReward` design note: micro time emphasis + energy pulse + optional oracle flicker, no new button.
4. Capture 5-10 short gameplay clips per target game and tag camera events by frame: follow, lock, snap, zoom, shake, return.
5. Prototype one horizontal ARPG camera preset: `CombatSoftLockFollow + CounterSnap + UltimateImpactHold`.

## Source URLs

- PGR Combat Guide: https://punishing-gray-raven.fandom.com/wiki/Combat_Guide
- PGR Matrix: https://punishing-gray-raven.fandom.com/wiki/Matrix
- PGR GitHub data repo: https://github.com/Kengxxiao/Punishing_GrayRaven_Tab
- PGR CameraParam sample: https://raw.githubusercontent.com/Kengxxiao/Punishing_GrayRaven_Tab/master/table/client/camera/CameraParam.tab
- PGR NpcAnimatorAction sample: https://raw.githubusercontent.com/Kengxxiao/Punishing_GrayRaven_Tab/master/table/client/fight/npc/NpcAnimatorAction.tab
- PGR NpcAffix sample: https://raw.githubusercontent.com/Kengxxiao/Punishing_GrayRaven_Tab/master/table/client/fight/npc/NpcAffix.tab
- Honkai Combat: https://honkaiimpact3.fandom.com/wiki/Combat
- Honkai Time Fracture: https://honkaiimpact3.fandom.com/wiki/Time_Fracture
- Honkai GitHub data repo: https://github.com/nairieberry/HonkaiImpactData
- Honkai MonsterConfigData sample: https://raw.githubusercontent.com/nairieberry/HonkaiImpactData/master/Global/ExcelOutputAsset/MonsterConfigData.json
- Honkai AttackPunishData sample: https://raw.githubusercontent.com/nairieberry/HonkaiImpactData/master/Global/ExcelOutputAsset/AttackPunishData.json
- ZZZ Daze: https://zenless-zone-zero.fandom.com/wiki/Daze
- ZZZ Dodge: https://zenless-zone-zero.fandom.com/wiki/Dodge
- ZZZ Chain Attack: https://zenless-zone-zero.fandom.com/wiki/Chain_Attack
- ZZZ GitHub data repo: https://github.com/360NENZ/Dimbreath-ZenlessData
- ZZZ Anton camera sample: https://raw.githubusercontent.com/360NENZ/Dimbreath-ZenlessData/master/Data/Anton_Camera.json
- ZZZ WitchTime dodge sample: https://raw.githubusercontent.com/360NENZ/Dimbreath-ZenlessData/master/Data/BUFF_WitchTime_Dodge.json
- Silver Palace preview: https://www.polygon.com/gaming/598703/silver-palace-rpg-trailer
- Wuthering Waves gameplay overview: https://en.wikipedia.org/wiki/Wuthering_Waves
- Aether Gazer wiki: https://aether-gazer.fandom.com/wiki/Aether_Gazer_Wiki
