# Combo System Reference Research

Last updated: 2026-06-11 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` combo and action-feel planning. It was written after reading:

1. `PROJECT_BRIEF.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. `AGENTS.md`
5. `AI_CODE_CONTRACT.md`
6. `ARCHITECTURE_BOUNDARIES.md`
7. `HUD_COMBAT_SPEC.md`
8. `Assets/_Game/DesignDocs/ARPG_REFERENCE_RESEARCH.md`
9. `Assets/_Game/DesignDocs/SUBCULTURE_UI_REFERENCE_RESEARCH.md`

Active extraction rule: use reference games to extract concrete combo production patterns: basic-attack sequences, hold/branch attacks, dodge counters, assist/QTE entry, chain attacks, break/stun follow-up windows, animation state naming, event hooks, VFX/SFX binding, camera cues, hit-stop, and reward timing. The boundary is asset/code ownership: do not import proprietary animation clips, meshes, textures, audio, source code, or full raw tables as project assets.

## Scope Guard

This is a seed catalog, not a scope fence.

The goal is not to turn `IsekaiBrawl` into a manual combo-action game or add many attack buttons. Current project identity remains:

- `position + direction + dodge + summon timing`
- `direction-biased auto basic fire`
- `one dodge button`
- `one ultimate`
- `clear summon or assist action slot`
- `summon-first battlefield swing`

Combo research is useful because it gives timing, transition, animation, VFX, SFX, camera, and reward-window language for the current verbs. It should be applied as:

- `perfect dodge -> empowered basic-fire pressure`
- `summon call -> assist-like entry hit -> pocket shift`
- `boss pressure break -> short burst or summon follow-up window`
- `auto basic fire -> readable sequence beats`
- `ultimate -> short high-impact finisher`

It should not become:

- `manual basic attack spam as the main skill expression`
- `many active attack skills`
- `hard target-selection UI`
- `parry as a baseline button`
- `full character-switch action game`

## Source Inventory

| ID | Game | Source type | Useful combo data | Confidence | URL |
|---|---|---:|---|---:|---|
| `zzz_wiki_basic_attack` | Zenless Zone Zero | Wiki | Basic attack category, basic attack damage/daze scaling sources, basic attack as input button | High | https://zenless-zone-zero.fandom.com/wiki/Basic_Attack |
| `zzz_wiki_dodge` | Zenless Zone Zero | Wiki | Dodge, Perfect Dodge, Dodge Counter, dash attack, invulnerability, projectile-deflect notes | High | https://zenless-zone-zero.fandom.com/wiki/Dodge |
| `zzz_wiki_special_attack` | Zenless Zone Zero | Wiki | EX Special activation with energy, hold variants, invulnerability, buff follow-ups | High | https://zenless-zone-zero.fandom.com/wiki/Special_Attack |
| `zzz_wiki_assist` | Zenless Zone Zero | Wiki | Perfect Assist, Assist Follow-Up, Quick Assist chaining, follow-up with basic attack | High | https://zenless-zone-zero.fandom.com/wiki/Assist |
| `zzz_wiki_chain_attack` | Zenless Zone Zero | Wiki | Daze-triggered chain attack, Bangboo chain, Ultimate, invulnerability, switch/QTE flow | High | https://zenless-zone-zero.fandom.com/wiki/Chain_Attack |
| `zzz_data_combo` | Zenless Zone Zero | GitHub data mirror | Perfect combo counter/reset, WitchTime dodge reward, camera events, attack sound/effect hooks | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `pgr_wiki_combat_guide` | Punishing: Gray Raven | Wiki | 3-Ping, QTE, Matrix, Matrix-Ping, shield class roles, swap-in as 3-Ping | Medium | https://punishing-gray-raven.fandom.com/wiki/Combat_Guide |
| `pgr_data_fight` | Punishing: Gray Raven | GitHub data mirror | NPC animator action taxonomy, role effects, screen effects, fight button style table | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab/tree/master/table/client/fight |
| `hi3_wiki_combat` | Honkai Impact 3rd | Wiki | Basic attack sequences, evasion cancel, branch/charge attack, QTE, weapon skill, burst mode, button replace | High | https://honkaiimpact3.fandom.com/wiki/Combat |
| `hi3_data_skills` | Honkai Impact 3rd | GitHub data mirror | Avatar skill button names, attack speed params, subskill params, attack punish data | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `wuwa_wiki_combat` | Wuthering Waves | Wiki | Basic attack variants, heavy/mid-air/dodge counter, Resonance Skill, Liberation, Forte, Intro/Outro, Echo | Medium | https://wutheringwaves.fandom.com/wiki/Combat |
| `genshin_wiki_normal_attack` | Genshin Impact | Wiki | Normal attack, charged attack, plunging attack, 3-6 strike sequences, infusion/conversion | Medium | https://genshin-impact.fandom.com/wiki/Normal_Attack |
| `silver_palace_polygon` | Silver Palace | News / preview | Dynamic melee plus third-person shooting, real-time party switching, bold UI/action presentation | Low | https://www.polygon.com/gaming/598703/silver-palace-rpg-trailer |

## Observed Production Data Shapes

### ZZZ: Basic attack is a skill type and a button-driven state family

Public wiki data separates `Basic Attack`, `Dodge`, `Assist`, `Special Attack`, `Chain Attack`, and `Ultimate` as combat skill types. Basic attack is not just damage; it is a route into dash attack, dodge counter, assist follow-up, charge consumption, and daze/DMG bonus logic.

Public data mirror paths show the production side:

- `Billy_Attack_Normal_01_BulletType_01.json`
- `Billy_Attack_Normal_02_BulletType_01.json`
- `Billy_Attack_Normal_03_BulletType_01.json`
- `Billy_CameraEvent.json`
- `Anton_Attack_Nomral_Enhance_Effects.json`
- `Anton_Attack_Nomral_Enhance_Sound.json`

Practical read:

- A basic attack chain can be split into named states such as `Attack_Normal_02_Start`, `Attack_Normal_02_A`, `Attack_Normal_02_B`, and `Attack_Normal_02_End`.
- Camera modifiers can attach to those attack states.
- VFX and SFX can be keyed from animation/combat events instead of from a generic attack button press.

IsekaiBrawl application:

- Keep auto basic fire, but give it named beat states: `BasicLoopThin`, `BasicLoopTurn`, `BasicEmpoweredStart`, `BasicEmpoweredHit`, `BasicEmpoweredEnd`.
- Let each beat own cue bundles for VFX/SFX/camera cleanup.
- Do not require repeated manual basic-attack input to get readable combo beats.

### ZZZ: Perfect combo can be counted as animation-state data

The public `Anbi_PerfectCombo.json` sample exposes:

- `AnimatorStateName`: `Attack_Normal_04_Perfect`, `Attack_Branch_01_Perfect`, `Attack_Branch_04_Perfect`
- `ActwithStateFrameMixin`
- `Frame: 0`
- `AS_PerfectComboIndex`
- `PerfectComboModifier_Count`
- `PerfectComboModifier_Reset`
- `Bool_PerfectCombo`
- `Duration: 1.0`

Practical read:

- Perfect combo state is counted and reset by animation-state entry, not by a free-floating integer in UI.
- A short 1 second reset/count window is a useful first reference for chaining a special follow-up without letting the state linger.

IsekaiBrawl application:

- Use a similar short window for `PerfectDodgeFollowUpWindow`.
- Store the state as a combat cue, not as a new manual combo button chain.

### ZZZ: WitchTime shows a dodge reward as a timed team modifier

The public `Ability_WitchTime.json` sample exposes:

- `OnDodgeSuccess`
- `DurationTime: 4`
- team-wide modifier application
- block management and cleanup
- slow-down mixin
- sound on end

Practical read:

- A perfect dodge reward can be a timed combat state with cleanup and audio hooks.
- The data does not need to be a long cinematic; it can be a concise timed opportunity.

IsekaiBrawl application:

- Current baseline already wants `brief slow + stronger follow-up basic-fire pressure`.
- First-pass authoring range: `0.35-1.20s` for visible slow/flash and `1.0-4.0s` for empowered follow-up state, tuned down if it overpowers summons.

### ZZZ: Chain attack is a break-state QTE route

The public Chain Attack page describes chain attacks as a QTE-like route after the enemy's daze condition reaches maximum. It can include Bangboo chain attacks and ultimate logic.

Practical read:

- A combo finisher can be gated by enemy break/daze state, not by a long button sequence.
- A helper unit can participate in the chain before or between main character attacks.

IsekaiBrawl application:

- Convert this into `BossPatternBreak -> SummonFollowUpPrompt`.
- For v1, do not add character switching. Use the current summon slot as the assist/QTE receiver.

### ZZZ: Assist follow-up is a defensive/assist bridge

The public Assist page describes follow-up attacks after Perfect Assist and shows examples where pressing basic attack after assist activates an invulnerable follow-up. Some skills then transition into EX Special with a hold action.

Practical read:

- Assist is not only a swap. It is a timing bridge: defensive read -> follow-up -> possible next action.
- A single follow-up button can safely reuse the basic attack input after a defensive trigger.

IsekaiBrawl application:

- Use the summon button or current basic-fire state as the follow-up receiver after `PerfectDodge` or `SummonGuard`.
- Keep the player input small: do not add a separate assist-follow-up button unless the prototype proves it is necessary.

### PGR: 3-Ping and Matrix are resource-shape combo grammar

PGR's public combat guide defines:

- `3-Ping`: three same-color orbs consumed by one press for a stronger attack.
- `QTE`: a teammate color match after 3-Ping allows a teammate to jump out, do a skill, and leave.
- `EX Dodge`: perfect dodge that triggers Matrix.
- `Matrix`: time-slow around the character with a special Matrix-Ping opportunity.
- Swap-in attack counts as a 3-Ping and can charge energy or activate core passives.

Practical read:

- Combo grammar can be built from resource shape plus timing: match -> stronger attack -> QTE/assist -> energy.
- QTE does not require full character control handoff.

IsekaiBrawl application:

- `SummonCall` can be treated as an earned 3-Ping-like breakthrough, not a random pet spawn.
- A perfect dodge can temporarily upgrade the next basic-fire or summon call without adding manual attack chains.

### PGR: combat animation taxonomy is broad and trigger-aware

The public `NpcAnimatorAction.tab` includes:

- state-like actions: `Stand`, `Run`, `Walk`, `Falldown`, `Hitdown`, `Stun`
- trigger actions: `Born`, `Hit1`, `Hit2`, `Hit3`, `Attack01` through many attack ranges, `Move1`, `AttackQte`
- `IsTrigger` flag

`RoleEffect.tab` and `ScreenEffect.tab` separate role effect and screen effect concerns, while `UiFightButtonDefaultStyle.tab` exists as its own combat-button style table.

Practical read:

- Combo production needs at least four separated layers:
  - action state/trigger
  - actor effect
  - screen effect
  - input UI state

IsekaiBrawl application:

- Do not hide all combo polish inside `PlayerSkillController`.
- Later data should split `ComboActionState`, `ActorEffectCue`, `ScreenEffectCue`, and `CombatButtonCue`.

### Honkai Impact 3rd: basic attack sequences are cancelable

The public combat page describes basic attack sequences that progress with repeated basic attack use and can be cancelled by evasion, ultimate, or special skills. It also describes branch and charge attacks:

- Branch attack can appear after basic/evasion plus hold/timing.
- Some characters require continuous attack pressing after a branch to keep attacking.
- Charge attack can use hold and release.
- QTE can tag in under special conditions.
- Burst mode can replace normal attacks with stronger burst attacks.
- Button replace can change a button into a combo trigger.

Practical read:

- The highest-value data is not the exact combo list, but the cancel hierarchy:
  - basic sequence can be interrupted by survival/action verbs
  - hold/branch gives a heavier route
  - QTE/tag-in gates special entry
  - burst mode can temporarily transform basic attacks

IsekaiBrawl application:

- Auto basic fire should be cancellable by dodge, summon, and ultimate.
- `PerfectDodge` can temporarily transform basic fire into `EmpoweredBasic`.
- Summon entry can act like a QTE/tag-in without full party switching.

### Honkai Impact 3rd: skill data separates button identity, icon, subskill, and attack speed

The public data mirror exposes:

- `AvatarSkillData.json`: `buttonName` examples such as `ATK`, `SKL01`, plus icon paths and skill IDs.
- `AvatarSubSkillData.json`: subskill IDs, max level, param base/add fields, tag lists.
- `AvatarAttackSpeedParam.json`: per-avatar attack speed parameter rows, including fixed-speed flags and multiple tuning constants.
- `AttackPunishData.json`: level-difference damage-reduction curve.

Practical read:

- A combo system needs separate data for:
  - button identity
  - icon/UI state
  - skill tree or subskill tuning
  - attack speed tuning
  - damage/punish scaling

IsekaiBrawl application:

- Keep v1 simple, but do not hardwire animation speed or button identity into one class.
- Add data fields later only when the playable loop proves the need.

### Wuthering Waves: one button branches by timing/context

The public combat page lists active abilities such as Basic Attack, Resonance Skill, Liberation, Forte, Intro/Outro, and Echo. It also describes Basic Attack variants such as Heavy Attack, Mid-air Attack, and Dodge Counter depending on timing, location, or how the button is pressed. Intro/Outro skills trigger through Concerto energy, and Echo can be summoned or transformed into.

Practical read:

- A single attack button can branch by context without adding many UI buttons.
- Echo is a useful analogy for a summon that can appear briefly as a high-impact action.

IsekaiBrawl application:

- Contextual branching is allowed if it stays readable:
  - moving + basic -> directional pressure
  - perfect dodge + basic state -> empowered counter wave
  - summon ready + break window -> assist entry

### Genshin Impact: normal attack has a compact input taxonomy

The public normal attack page separates Normal, Charged, and Plunging attacks. Normal attacks commonly form 3-6 strike sequences, charged attacks use a heavier input concept, and elemental infusion/conversion can change the attack's damage identity.

Practical read:

- Even a simpler ARPG can get variety from a small taxonomy:
  - repeated light sequence
  - hold heavy
  - contextual air/drop route
  - temporary infusion/empower state

IsekaiBrawl application:

- The likely v1 variant set should be smaller:
  - neutral auto basic
  - empowered post-dodge basic
  - summon-assist impact
  - ultimate impact

### Silver Palace: low-confidence style reference

Public preview coverage describes Silver Palace as an anime action RPG with dynamic melee, third-person shooting, real-time party switching, a gothic/Victorian identity, and a bold UI direction. Since the game was unreleased at the time of source capture, treat it as visual and presentation reference, not reliable raw combo data.

IsekaiBrawl application:

- Use it for bold action presentation, not combat-system truth.

## Combo Pattern Catalog

| Pattern ID | Source inspiration | Input shape | Function | Presentation data to capture | IsekaiBrawl use |
|---|---|---|---|---|---|
| `basic_sequence_beats` | HI3, Genshin, ZZZ | repeated basic or auto loop | creates readable rhythm | state names, hit frames, swing trails, SFX tiers, camera stability | Give auto basic fire visible beats without manual mashing. |
| `hold_branch_attack` | HI3, ZZZ EX hold, Genshin charged | hold then release or hold during skill | heavier attack or alternate route | charge glow, hold SFX, release flash, recovery length | Candidate only if current controls need one more expressive input; not v1 default. |
| `dodge_counter_followup` | ZZZ, Wuthering, PGR Matrix | perfect dodge then basic/follow-up | read-react payoff | dodge flash, slow window, counter VFX, camera snap, invulnerability cue | Main candidate for `perfect dodge -> empowered basic-fire pressure`. |
| `assist_qte_entry` | ZZZ Assist, PGR QTE, Wuthering Intro | condition trigger then assist/summon | helper entry hit or buff | entry angle, call SFX, character/summon motion, cleanup | Convert current summon call into a stronger assist-like entry. |
| `break_chain_window` | ZZZ Chain/Daze, Wuthering Vibration, PGR Shield | enemy break then chain prompt | reward window after pressure | enemy stagger pose, UI prompt, camera pull/push, SFX sting | `BossPatternBreak -> SummonFollowUpPrompt`. |
| `temporary_empowered_basic` | HI3 Burst, ZZZ buffs, PGR Matrix-Ping | timed state after trigger | transforms basic loop briefly | color shift, hit VFX upgrade, button/resource glow, end cue | Post-perfect-dodge empowered sword-wave window. |
| `swap_in_as_attack` | PGR swap-in, Wuthering Intro, ZZZ Chain | switch/call acts as attack | entry damage or buff | entry camera, landing VFX, invulnerability/readability | Use summon entry; avoid full party switching. |
| `resource_shape_combo` | PGR 3-Ping, ZZZ Decibel/Daze, Wuthering Concerto | resource threshold + timing | converts setup into payoff | resource fill, threshold flash, prompt, result | Summon energy and boss break should create clear payoff windows. |
| `dash_or_move_attack` | ZZZ Dash Attack, Wuthering context basic | move/dash + basic | mobility-based strike | speed lines, ground dust, camera lead, target priority | Direction-biased auto-fire can gain a move-intent beat. |
| `ultimate_impact_finisher` | HI3/ZZZ ultimates | one ultimate button | high-impact finisher or safety | time stop, camera hold, screen effect, big SFX, recovery | Keep one ultimate with strong but short presentation. |
| `button_replace_context` | HI3 button replace, ZZZ contextual buttons | existing button changes meaning | avoids extra buttons | icon swap, label, glow, cooldown clarity | Use sparingly for `SummonFollowUp` during break windows. |
| `enemy_followup_cancel` | ZZZ Assist canceling enemy behavior | enemy reaction after player defense | prevents free loop exploit | black/glitch cue, warning, combo cutoff | Boss can occasionally deny repeated follow-up loops, with clear telegraph. |

## Suggested Timing Ranges

These are first-pass authoring ranges, not final tuning.

| Window | Suggested range | Notes |
|---|---:|---|
| Basic beat spacing | 0.28-0.55s | Auto-fire can visually step through these beats while retaining automated input. |
| Basic hit flash | 0.04-0.10s | Short flash/SFX tier per hit. |
| Light hit-stop | 0.03-0.06s | Keep small; mobile readability matters. |
| Heavy/branch hit-stop | 0.08-0.14s | Use for summon impact, guard break, ultimate, not every hit. |
| Dodge i-frame readable cue | 0.20-0.45s | Visual readability, not exact physics contract. |
| Perfect dodge flash | 0.18-0.40s | UI/edge/camera cue. |
| Perfect dodge empowered state | 1.00-4.00s | ZZZ WitchTime sample suggests longer states exist; start shorter to protect summons. |
| Counter snap camera | 0.18-0.45s | Short target-relative push or yaw correction. |
| Summon assist entry | 0.35-0.90s | Long enough to read, short enough to keep control. |
| Break/stagger window | 2.00-6.00s | Small enemies lower; boss pressure body should be brief until final stand. |
| Ultimate impact hold | 0.45-1.20s | Use high presentation but avoid long cinematic lock for v1. |
| Combo reset grace | 0.60-1.20s | ZZZ perfect combo sample uses a 1 second modifier duration; good seed value. |

## Animation And Cue Layering

### Minimum useful action-state taxonomy

- `Idle`
- `Move`
- `BasicStart`
- `BasicLoopA`
- `BasicLoopB`
- `BasicEnd`
- `EmpoweredBasicStart`
- `EmpoweredBasicHit`
- `DodgeStart`
- `DodgeSuccess`
- `CounterWindow`
- `SummonCallStart`
- `SummonEntryImpact`
- `UltimateStart`
- `UltimateImpact`
- `HitReact`
- `Stagger`
- `Break`
- `Recover`
- `Death`

### Cue layers per meaningful hit

Every important combo beat should define:

- `input or trigger`: what caused it
- `animation state`: where the character/summon/enemy is
- `hit frame`: when damage or pressure applies
- `VFX`: trail, impact, aura, screen overlay
- `SFX`: swing, hit, confirm, bass/impact, cleanup
- `camera`: follow, snap, zoom, shake, hold, cleanup
- `UI`: button pulse, resource flash, prompt, cooldown
- `cancel rules`: what can interrupt or branch
- `cleanup`: what must end even if interrupted

## Suggested Data Contracts

These are implementation targets for later, not code requirements for this research task.

```json
{
  "ComboAction": {
    "id": "EmpoweredBasicHitA",
    "trigger": "PerfectDodgeFollowUp",
    "inputShape": "auto_or_basic_state",
    "animationState": "EmpoweredBasicHit",
    "hitFrame": 12,
    "cancelInto": ["Dodge", "SummonCall", "Ultimate"],
    "locksMovementSeconds": 0.08
  },
  "ComboCueBundle": {
    "id": "PerfectDodgeCounterWave",
    "actionId": "EmpoweredBasicHitA",
    "vfx": ["counter_edge_flash", "sword_wave_empowered"],
    "sfx": ["dodge_success", "counter_wave_hit"],
    "cameraCue": "CounterSnap",
    "uiCue": "DodgeButtonSuccessPulse",
    "cleanupPolicy": "OnActionEndOrInterrupt"
  },
  "ComboWindow": {
    "id": "BossPatternBreakSummonFollowUp",
    "opensOn": "BossPatternBreak",
    "durationSeconds": 3.0,
    "preferredAction": "SummonCall",
    "fallbackAction": "EmpoweredBasic",
    "uiPrompt": "SummonFollowUp"
  }
}
```

## IsekaiBrawl Application

### What to adopt first

1. `PerfectDodgeFollowUpWindow`
Use a short window after perfect dodge that strengthens the next basic-fire beat. This fits the current baseline and does not add buttons.

2. `SummonAssistEntry`
Treat summon call as an assist/QTE entry: one clear motion, entry hit or shield/heal/interrupt, camera/VFX/SFX bundle, cleanup.

3. `BossPatternBreakWindow`
When boss pressure is correctly read or interrupted, open a short summon-first follow-up window. This is a better fit than long player combo strings.

4. `BasicFireBeatStates`
Split current auto basic fire into visible sequence beats. The input remains simple, but the animation/VFX/SFX rhythm becomes more action-like.

### What to avoid for now

- Multiple active attack buttons.
- Long manual combo strings.
- Full character switching.
- Baseline parry button.
- Combo UI that steals attention from dodge/summon/ultimate.
- Exact clone of any reference game's button system.

### Practical next extraction tasks

1. Frame-tag 10 to 20 second clips for `basic sequence`, `perfect dodge counter`, `assist/QTE`, `break/stun chain`, and `ultimate`.
2. For each clip, record `input`, `animation state`, `hit frame`, `SFX`, `VFX`, `camera`, `UI pulse`, `cancel/branch`, and `cleanup`.
3. Prototype only three cue bundles first:
   - `PerfectDodgeCounterWave`
   - `SummonAssistEntry`
   - `BossPatternBreakSummonFollowUp`
4. Do not prototype hold-branch attack until the current v1 control shell proves it can absorb one more input distinction.

