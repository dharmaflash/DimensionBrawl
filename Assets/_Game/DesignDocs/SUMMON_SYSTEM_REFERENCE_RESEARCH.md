# Summon System Reference Research

Last updated: 2026-06-11 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` summon, assist, QTE, companion, and field-skill planning. It was written after reading:

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

Active extraction rule: use reference games to extract concrete summon integration patterns: opportunity windows, target-relative entry, short assist/QTE attacks, persistent field companions, rescue/taunt calls, heal fields, projectile helpers, screen/camera/audio cleanup, and role-specific target selection.

## Scope Guard

This is a seed catalog, not a conservative scope fence.

The project identity remains:

- `position + direction + dodge + summon timing`
- `direction-biased auto basic fire`
- `one dodge button`
- `one ultimate`
- `one clear summon or assist action slot`
- `summon-first battlefield swing`

Summon research should be applied as:

- `perfect dodge -> summon assist opening`
- `boss pattern break -> summon follow-up`
- `structure break -> summon tempo reward`
- `pressure danger -> Tank rescue or Heal tempo call`
- `basic-fire beat -> Arrow or Break timing bridge`
- `ultimate -> summon-supported finisher or setup`

It should not become:

- `random pet auto-battle with no tactical call`
- `full party-switch action combat`
- `hand-of-cards UI`
- `manual target-selection UI`
- `many active skill buttons`
- `summon placement drag as the default v1 verb`

Asset/code ownership boundary: do not import proprietary animation clips, meshes, textures, audio, source code, or full raw tables as project assets. Extract patterns, timing ranges, schema shapes, field names, and cue relationships.

## Source Inventory

| ID | Game | Source type | Useful summon data | Confidence | URL |
|---|---|---:|---|---:|---|
| `zzz_wiki_bangboo` | Zenless Zone Zero | Wiki | Combat Bangboo as battle companions, active support skills, Bangboo Chain Attack cooldown | High | https://zenless-zone-zero.fandom.com/wiki/Bangboo |
| `zzz_wiki_assist` | Zenless Zone Zero | Wiki | Quick Assist, Defensive Assist, Assist Follow-Up, invulnerable assist bridges | High | https://zenless-zone-zero.fandom.com/wiki/Assist |
| `zzz_wiki_chain_attack` | Zenless Zone Zero | Wiki | Daze-triggered Chain Attack, Bangboo Chain Attack, Ultimate link, invulnerability | High | https://zenless-zone-zero.fandom.com/wiki/Chain_Attack |
| `zzz_data_summon_qte` | Zenless Zone Zero | GitHub data mirror | Buddy QTE ranges, target-relative spawn, QTE tags, camera/screen cleanup, projectile helper | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `pgr_wiki_combat_guide` | Punishing: Gray Raven | Wiki | 3-Ping to QTE, teammate jump-out skill then leave, Tank shield shred, Support heal | Medium | https://punishing-gray-raven.fandom.com/wiki/Combat_Guide |
| `pgr_data_fight` | Punishing: Gray Raven | GitHub data mirror | NPC animator taxonomy, `AttackQte`, role/screen effect tables, fight button style | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab |
| `hi3_wiki_combat` | Honkai Impact 3rd | Wiki | QTE tag-in conditions, timed buffs, perfect evasion, button replacement, ultimate i-frame | High | https://honkaiimpact3.fandom.com/wiki/Combat |
| `hi3_wiki_qte` | Honkai Impact 3rd | Wiki | QTE list by trigger condition such as stun, float, frozen | Medium | https://honkaiimpact3.fandom.com/wiki/QTE |
| `hi3_data_elf` | Honkai Impact 3rd | GitHub data mirror | ELF attack cooldown, ultra cooldown/SP cost, skill type, icon/audio/model paths | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `wuwa_wiki_combat` | Wuthering Waves | Wiki | Perfect dodge, Vibration Strength break, Intro/Outro, Echo system | Medium | https://wutheringwaves.fandom.com/wiki/Combat |
| `wuwa_wiki_intro_outro` | Wuthering Waves | Wiki | Concerto Energy full -> outgoing Outro + incoming Intro, role buffs and entry attacks | Medium | https://wutheringwaves.fandom.com/wiki/Intro_Skill |
| `wuwa_wiki_echo_skill` | Wuthering Waves | Wiki | Echo Skill as active skill derived from equipped Echo | Medium | https://wutheringwaves.fandom.com/wiki/Echo_Skill |
| `genshin_oz` | Genshin Impact | Wiki | Persistent companion: summon damage, 10s duration, 1s interval attacks, reposition call | Medium | https://genshin-impact.fandom.com/wiki/Nightrider |
| `genshin_yuegui` | Genshin Impact | Wiki | Heal/damage field companion with HP-threshold targeting and homing projectiles | Medium | https://genshin-impact.fandom.com/wiki/Raphanus_Sky_Cluster |
| `genshin_jumpy_dumpty` | Genshin Impact | Wiki | Projectile/trap skill, bounce, mines, charges, delayed area denial | Medium | https://genshin-impact.fandom.com/wiki/Jumpy_Dumpty |
| `silver_palace_preview` | Silver Palace | News / preview | Horizontal anime ARPG presentation, real-time party switching, melee plus third-person shooting | Low | https://www.polygon.com/gaming/598703/silver-palace-rpg-trailer |

## Observed Production Data Shapes

### ZZZ: Buddy QTE is a target-relative summon entry contract

The public `Bangboo_FightingCommon_QTE.json` sample exposes:

- `$type`: `BuddyQTEMixin`
- `BangbooQTEIndex`: `10`
- `BangbooQTEPosRot.CoordsOption`: `Relative`
- `NeedRaycastGround`: `true`
- `OffsetReferTarget`: `Target`
- `PositionOffset`: `x=-1.5`, `z=-2.0`
- `NeedOverridePosRot`: `true`
- `QTEEnterRange`: `7`
- `QTEExitRange`: `10`
- `QTELockTargetID`: `QTE_Tick_LockTarget_01`
- `OnBuddyQTEStart`: audio start/stop actions and animator zone tag `InBangbooQTE`
- `QTEOpenModifier.Duration`: `1.0`
- `QTECloseModifier.Duration`: `0.1`
- QTE open camera/screen actions: zoom, stretch, override track, radial blur, vignette, color adjustments, input mute, invincible buff
- QTE close cleanup: time-slow interrupt, camera end, screen effect interrupt, camera-lock pop, UI interrupt, input restore, invincible buff removal

Practical read:

- A polished assist call is not just `spawn actor and attack`.
- It has an explicit opportunity window, target-relative entry, lock target, UI state, camera state, input state, and cleanup.
- The close path is as important as the entry path because it prevents camera/UI residue.

IsekaiBrawl application:

- Add `SummonAssistEntry` data around `role`, `opportunityWindow`, `entryPosRef`, `entryOffset`, `enterRange`, `exitRange`, `lockTargetMode`, and `cleanupCue`.
- Use target-relative spawn for Break/Tank/Arrow when a valid local threat exists.
- Use player-relative or forward-pocket fallback only when there is no valid threat.

### ZZZ: Assist attacks are tag/window driven

The public `Anbi_AidAttack.json` sample exposes:

- `$type`: `AidAttackMixin`
- `AidAttackConditionList`
- tags such as `AidAttack_Common`, `AidAttack_Parry_L`, `AidAttack_Parry_H`
- `CloseDelayTime`: `1.5` for common hit-fly/common aid windows
- `Priority`: `1`, `3`, `10`
- `SwitchInIndex`: `30` or `50`
- `SwitchInFadeTime`: `0.05`
- `SwitchOutFadeTime`: `0.1`
- `TargetRange`: `20`
- `CheckIsInView`, `CheckTargetRangeOnlyOpenWindow`, `DisableInWitchTime`
- target-relative `SwitchInPosRot`

Practical read:

- Assist is data-driven by tags and short close delays.
- Different assist reasons can share the same summon button but resolve by priority.
- Camera/target range checks keep assist prompts from opening offscreen or against unreadable threats.

IsekaiBrawl application:

- Represent summon openings as `SummonOpportunityWindow` rows, not hardcoded button cases.
- Candidate windows: `PerfectDodge`, `BossPatternBreak`, `StructureBreak`, `PlayerPressureDanger`, `BasicFireBeatEnd`, `UltimateWindup`.
- Add priority so Tank rescue can beat Arrow pressure when the player is about to be overwhelmed.

### ZZZ: Perfect Assist is a short reward modifier with VFX/material feedback

The public `Ability_PerfectAssist.json` sample exposes:

- `PerfectAssistModifier.Duration`: `0.1`
- `ModifyPerfectSwitchChargeValueAction.Amount`: `720`
- `FireEffect`: `Buff_Common_Attack`
- `ModifyMaterialPropertyAction.Tag`: `CommonAttackMaterial`
- `RemoveAbilityAction`: `BUFF_EXQTE`

Practical read:

- A perfect assist does not need long duration to feel strong.
- The call window can be tiny if the entry impact, resource change, material glow, and sound are clear.

IsekaiBrawl application:

- Give perfect-dodge summon windows a short visible spark, not a long tutorial prompt.
- Store the reward as `summonWindowCharge`, `cueBundle`, and `consumesWindowOnCall`.

### ZZZ: Bangboo QTE animation owns camera cleanup

The public `Bangboo_Bagboo_BangbooQTE_AnimEvent.json` sample exposes:

- `AttachStateWithModifierMixin`
- `AnimatorStateName`: `SwitchIn_Attack`, frame `0-25`
- `AnimatorStateName`: `SwitchIn_Attack_Launch`, frame `0-45`
- `CamEndModifier.OnRemoved`: multiple `CameraZoomEndAction`, `CameraStretchEndAction`, `CameraOverrideTrackEndAction`
- `QTEEndModifier.OnRemoved`: `BangbooQTEEndAction`, lock-target fallback, `OverrideSwitchInIndex`, `ByIsInBuddyQTE`
- `IgnoreQTEHideUIModifier.Duration`: `0.1`
- animator zone tag cleanup: remove `InBangbooQTE`, add/remove `AfterBangbooQTE`

Practical read:

- Assist camera is tied to animation-state lifetime, not only to button press.
- Entry state, launch state, QTE end, and post-QTE UI hide grace are separate concerns.

IsekaiBrawl application:

- First data contract should include `entryState`, `impactState`, `exitState`, `cameraCueKeys`, `uiMuteTags`, and `postExitGrace`.
- Do not let summon camera snaps live only inside `BattleCamera.Shake`.

### ZZZ: Avocaboo QTE shows a heal/support projectile as a combat object

The public `Bangboo_Avocaboo_QTE.json` sample exposes:

- `$type`: `BulletMixin`
- `AliveDuration`: `10`
- `SphereColliderRadius`: `0.3`
- `BulletEffect.EffectPattern`: `Eff_Bangboo_Avocaboo_Bullet_Fly`
- `HitAnimEventID`: `Bangboo_Avocaboo_Attackproperty_Attack_QTE`
- `HitExplodeRadius`: `3`
- `InitVelocity.z`: `10`
- `Targetting`: `Enemy`
- `OnBulletHitStageGroundExplodeAction`: trigger explode ability
- `OnBulletHitStageWallExplodeAction`: anim event, camera shake, audio
- `OnBulletHitTargetExplodeAction`: camera shake, audio, hit effect

Practical read:

- A support summon can still use a real projectile, hit event, camera shake, and impact audio.
- Heal/support roles should not look like invisible stat correction.

IsekaiBrawl application:

- `Heal` should enter as a visible tempo field or projectile, with explicit impact, SFX, and VFX.
- `Arrow` should use a similar projectile object contract but with target priority and off-angle origin.

### PGR: QTE is a resource-shape teammate call, not full control handoff

The public combat guide describes:

- `3-Ping`: three same-color Signal Orbs consumed by one press for a stronger attack.
- `QTE`: a matching teammate portrait flashes after 3-Ping; pressing it makes the teammate jump out, do a skill, and leave.
- `Tank`: shield shred role.
- `Support`: healing and buffs.
- Some healing examples call allies onto the field and let a QTE heal nearby allies.

Practical read:

- QTE can be triggered by resource shape plus timing, then resolved as a temporary helper action.
- Role identity is readable through effect type: tank breaks shields, support heals/buffs, attacker bursts.

IsekaiBrawl application:

- Treat the current `4-card cycle` as a light resource-shape generator.
- A good summon call should feel like a earned QTE breakthrough, not random deployment.
- `Break` maps well to PGR Tank shield-shred grammar.
- `Heal` maps to QTE/field heal that rewards timing and proximity.

### HI3: QTE is condition-triggered tag-in, and ELF data separates companion stats from skills

The public combat page describes QTE as a tag-in launched when special conditions such as float, frozen, or stun are satisfied. The QTE page lists many QTEs by trigger condition.

The public `ElfData.json` and `ElfSkillData.json` samples expose:

- `ultraSkillCD`: examples `30`, `40`
- `ultraSkillSPCost`: examples `75`, `50`
- `attackSkillCD`: example `3.3`
- `AIName`: `Behavior_Elf_01`, `Behavior_Elf_02`
- `selectAudio`, `LevelUpAudio`, `UISoundbankName`
- `ElfUIModelPath`, `IconPath`, `ElfChibiIcon`
- `ElfSkillData.SkillType`: examples `0`, `1`, `2`
- skill layout fields: `UIPointColumn`, `UIPointRow`
- `AbilityParamBase_1`, `AbilityParamBase_2`, `AbilityParamBase_3`

Practical read:

- Companion systems benefit from separating actor identity, UI/model/audio metadata, cooldown/cost, and skill parameters.
- QTE trigger conditions can be authored as compact condition rows.

IsekaiBrawl application:

- Keep summon identity data separate from summon role behavior.
- Suggested split: `SummonDefinition`, `SummonRoleBehavior`, `SummonSkillCue`, `SummonOpportunityWindow`.

### Wuthering Waves: Intro/Outro and Echo show earned active-helper timing

Public combat pages describe:

- Perfect dodge as an immunity-timing mechanic.
- Vibration Strength depletion leading to enemy immobilize.
- Concerto Energy full -> switch triggers outgoing Outro and incoming Intro.
- Echo Skill as an active skill derived from the equipped Echo.

Practical read:

- A helper/entry action can be earned by dealing damage or dodging, then spent on an entry attack or support buff.
- Echo Skill is useful as a reference for "single active helper burst" without adding many character buttons.

IsekaiBrawl application:

- Use `PerfectDodge`, `boss break`, and `aggressive position energy` as summon-opportunity feeders.
- Keep one summon slot, but let the current role create an Echo-like burst or short persistent field.

### Genshin: field companions show persistent target rules and delayed area denial

Nightrider exposes:

- Summon damage on cast.
- `10s` duration.
- `25s` cooldown.
- Oz attacks nearby opponents at `1-second intervals`.
- Hold can adjust summon location.
- Re-press can resummon Oz to the character's side.

Raphanus Sky Cluster exposes:

- `15s` cooldown.
- hold mode to adjust throw direction.
- Yuegui projectiles deal damage and heal in the same AoE.
- Target choice depends on nearby character HP: above 70% -> attack enemies, at or below 70% -> heal lowest HP percentage.
- maximum 2 instances.
- one projectile every second for 10 projectiles.
- cylindrical targeting zone: radius `8m`, height `10m`.

Jumpy Dumpty exposes:

- projectile bounces three times.
- splits into mines.
- mines explode on contact or after a short duration.
- starts with 2 charges.
- mine duration `15s`.

Practical read:

- Persistent helpers need target priority, interval, duration, max instances, relocation, and cleanup.
- Heal helpers are more interesting when they switch targeting by HP threshold.
- Trap/mines are useful references for `Break` and `Tank` space-making.

IsekaiBrawl application:

- `Arrow`: off-angle persistent pressure, target interval, priority target rules.
- `Heal`: HP-threshold targeting, field/projectile heal, short duration, clear AoE.
- `Tank`: collision/taunt/mines/anchor behavior, pressure redirect.
- `Break`: delayed or immediate structure/guard-break impact, not generic damage.

### Silver Palace: current value is presentation scope, not summon implementation detail

Public preview coverage is low-confidence for system extraction because the game is not fully released. The useful signal is that anime ARPG competitors are combining stylish UI, real-time party switching, dynamic melee, and third-person shooting inside horizontal presentation.

Practical read:

- It supports the project's horizontal ARPG presentation ambition.
- It does not provide reliable summon-system data yet.

IsekaiBrawl application:

- Use it only as presentation pressure: summon entries must look like premium ARPG actions, not small prototype effects.

## Pattern Catalog For IsekaiBrawl

### `perfect_dodge_to_summon_assist`

- Trigger: player lands perfect dodge during readable threat.
- Window: `0.5-1.2s`.
- Best roles: `Break`, `Arrow`, `Tank`.
- Feel: small slow/flash, current summon card pulse, short input leniency.
- Camera: `CounterSnap` or `AssistEntryPush`, max `0.35-0.75s`.
- Risk: if too long, perfect dodge overshadows summon timing.

### `boss_break_to_summon_followup`

- Trigger: boss pattern break, daze, shield break, or immobilize-like state.
- Window: `1.5-3.0s`.
- Best roles: `Break`, `Arrow`, `Heal`.
- Feel: boss vulnerability marker, summon entry becomes the finisher/setup.
- Camera: short target-centered impact shot.
- Risk: must not become full character-switch chain attack.

### `structure_break_to_summon_tempo_reward`

- Trigger: structure/core/turret anchor destroyed.
- Window: `2.0-4.0s`.
- Best roles: any, with role-specific emphasis.
- Feel: energy refund, summon card "armed" cue, clear pocket breathing space.
- Camera: pullback to show opened lane or cleared pocket.
- Risk: immediate pressure escalation would erase the reward.

### `tank_rescue_taunt_entry`

- Trigger: player HP low, projectile density high, boss pressure cone active, or player overextended.
- Window: no long prompt; direct high-priority call if current card is Tank.
- Role behavior: enter between player and threat, taunt/guard, body-block, push or stagger nearby threats.
- Camera: minimal; keep player control readable.
- Risk: Tank must not become passive invincibility.

### `arrow_priority_pickoff`

- Trigger: support enemy, turret, ranged threat, boss weak point, or post-dodge empowered beat.
- Role behavior: off-angle entry, volley, pierce, mark or execute low-HP priority target.
- Camera: light aim-line or impact shake, no full cinematic.
- Risk: avoid full manual target-selection UI.

### `heal_tempo_field`

- Trigger: HP threshold, post-perfect-dodge safe window, structure break breathing window, boss phase transition.
- Role behavior: visible field/projectile heal, can damage enemies or cleanse pressure in same AoE.
- Targeting: low HP player first, then nearby summon/frontline, then enemy if safe.
- Camera: no heavy lock; use clean color/audio language.
- Risk: avoid passive background regen that hides the decision.

### `summon_as_basic_fire_beat_bridge`

- Trigger: auto basic-fire loop reaches a readable beat boundary.
- Window: `0.3-0.8s`.
- Best roles: `Arrow`, `Break`.
- Feel: current beat creates a small opportunity cue so the summon call reads as part of action rhythm.
- Risk: do not add manual basic combo spam.

### `ultimate_to_summon_setup`

- Trigger: ultimate activation or ultimate ending frames.
- Role behavior: current summon either amplifies the ultimate or prepares the next pocket shift.
- Camera: ultimate impact hold can reveal summon silhouette or follow-through.
- Risk: ultimate must not eat the summon role identity.

### `target_relative_qte_spawn`

- Trigger: any assist-like summon with a valid target.
- Data: target-relative offset, backup self-relative offset, raycast ground, enter/exit range, lock-target key.
- Best roles: `Break`, `Tank`, `Arrow`.
- Risk: bad offsets can spawn helpers offscreen or inside enemy geometry.

### `summon_exit_cleanup`

- Trigger: summon entry state ends, persistent duration ends, target lost, or window canceled.
- Data: camera end, screen effect interrupt, input/UI unmute, animator tag remove, lock-target fallback, audio stop.
- Applies to all roles.
- Risk: missing cleanup creates sticky camera/UI bugs.

## Role Applications

### Break

- Job: create the breakthrough by damaging shields, interrupting guard states, cracking structures, or opening boss pattern breaks.
- Best call moments: boss windup punish, structure core exposed, elite armor active, perfect dodge follow-up.
- Target priority: structure core > shielded elite > boss breakable part > dense enemy cluster.
- Preferred distance: near target or slightly behind/side of target.
- Entry pattern: target-relative slam, drill, mine burst, or shield-crack hit.
- Cue package: sharp hit-stop, white/yellow break flash, low camera shake, debris or crack VFX, strong one-shot SFX.
- Pocket shift target: enemy pressure should visibly dip within `2-3s`.

### Tank

- Job: rescue, taunt, anchor, body-block, and redirect pressure.
- Best call moments: player overextended, HP danger, boss cone/projectile density, ranged enemies focusing player.
- Target priority: nearest threat to player > boss projectile source > enemy cluster in player path.
- Preferred distance: between player and threat, or slightly forward of player.
- Entry pattern: intercept dash, shield plant, taunt shout, guard wall, shockwave push.
- Cue package: heavy shield SFX, blue/steel flash, short camera bump, threat-line redirect.
- Pocket shift target: player gains a readable breathing window without losing control.

### Arrow

- Job: off-angle pressure, priority pickoff, weak-point poke, ranged target control.
- Best call moments: support enemy appears, boss weak point exposed, perfect dodge window, basic-fire beat end.
- Target priority: support/ranged enemy > boss weak point > low-HP elite > closest hostile in firing cone.
- Preferred distance: flank or rear-side of target, away from player collision path.
- Entry pattern: side-step volley, pierce shot, mark-and-fire, rain arrow.
- Cue package: aim-line, thin projectile trail, crisp hit tick, small impact shake.
- Pocket shift target: one priority threat removed or visibly suppressed within `2-3s`.

### Heal

- Job: preserve tempo without becoming passive regen.
- Best call moments: HP threshold, after successful dodge, after structure break, before boss phase burst.
- Target priority: player below threshold > active summon/frontline below threshold > enemy if player safe.
- Preferred distance: near player or between player and safe advance path.
- Entry pattern: projectile heal, field pulse, cleanse burst, heal-and-damage AoE.
- Cue package: clear green/white field, soft but distinct SFX, no heavy cinematic, HP rail/card response.
- Pocket shift target: player can keep pressure instead of retreating into churn.

## Suggested Data Contracts

### `SummonDefinition`

- `id`
- `role`
- `displayName`
- `iconKey`
- `modelKey`
- `audioBankKey`
- `baseCooldown`
- `energyCost`
- `maxInstances`
- `canRepeatInCycle`

### `SummonRoleBehavior`

- `role`
- `targetPriority`
- `preferredDistance`
- `anchorRule`
- `fallbackWhenNoTarget`
- `pocketShiftGoalSeconds`
- `threatResponse`

### `SummonOpportunityWindow`

- `id`
- `trigger`
- `duration`
- `priority`
- `eligibleRoles`
- `cardCue`
- `cameraCue`
- `consumesOnCall`
- `cancelConditions`

### `SummonAssistEntry`

- `id`
- `role`
- `entryMode`
- `entryPosRef`
- `entryOffset`
- `backupPosRef`
- `backupOffset`
- `raycastGround`
- `enterRange`
- `exitRange`
- `lockTargetMode`
- `switchInFade`
- `switchOutFade`

### `SummonCueBundle`

- `id`
- `animationStates`
- `vfxKeys`
- `sfxKeys`
- `cameraKeys`
- `screenEffectKeys`
- `inputMuteTags`
- `uiHideTags`
- `cleanupKeys`

### `SummonProjectileOrField`

- `id`
- `role`
- `duration`
- `tickInterval`
- `collider`
- `movement`
- `targeting`
- `hitActions`
- `expireActions`
- `maxHitCount`

## First Adoption Recommendations

1. Build `SummonOpportunityWindow` first.
   - This directly solves `why this summon now`.
   - Start with `PerfectDodge`, `BossPatternBreak`, `StructureBreak`, and `PressureDanger`.

2. Give each active role one distinct target rule and one distinct anchor rule.
   - Break: breakable target / structure / shield.
   - Tank: player-threat line.
   - Arrow: support or ranged priority.
   - Heal: HP-threshold target choice.

3. Treat every summon call as a cue bundle.
   - Button/card pulse, entry animation, VFX/SFX, camera, hit/field effect, and cleanup should be authored together.

4. Keep cinematic time short.
   - Good first range: `0.15-0.35s` anticipation, `0.35-0.90s` entry impact, `2-3s` pocket shift.
   - Save longer holds for ultimate or boss phase moments.

5. Add cleanup data early.
   - Camera locks, input mutes, UI hides, and screen effects must have explicit end paths.

## Implementation Boundary Notes

- Do not implement code from this document before syncing active docs.
- Do not reopen `hand-of-cards UI`.
- Do not add direct target selection as the default summon control.
- Do not turn the current player into a full manual combo character.
- Do not treat `Rush` or `Meteor` as active v1 summons until direction changes.
- Do not let Heal become invisible passive sustain.
- Do not let Tank become permanent invulnerability.

## Next Small Tasks

1. Draft `SUMMON_COMBAT_SPEC.md` with the concrete v1 data contract and naming rules.
2. Implement a minimal `SummonOpportunityWindow` table for `PerfectDodge`, `BossPatternBreak`, `StructureBreak`, and `PressureDanger`.
3. Prototype one cue-complete summon call for `Tank` or `Break`, including camera cleanup and target-relative entry fallback.
