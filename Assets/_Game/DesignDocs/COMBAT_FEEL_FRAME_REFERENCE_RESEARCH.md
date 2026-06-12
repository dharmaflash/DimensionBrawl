# Combat Feel Frame Reference Research

Last updated: 2026-06-12 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` player feel, hit reaction, pressure break, combat energy, frame tagging, and cue authoring. It was written after reading:

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
11. `Assets/_Game/DesignDocs/BOSS_ENEMY_RUN_REFERENCE_RESEARCH.md`
12. `Assets/_Game/DesignDocs/STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md`

Active extraction rule: use reference games to extract combat-production patterns below the high-level mechanic layer: dodge/counter timing, invulnerability windows, hit-stop, hit reaction, poise/stagger/break, energy reward sources, camera cue binding, screen effect binding, SFX/VFX event hooks, and animation frame tags.

## Scope Guard

This is a feel and authoring seed catalog, not a new input-scope expansion.

Use it to answer:

- How long does a move commit the player?
- Where are the invulnerability, perfect-dodge, counter, summon-call, and cancel windows?
- What should happen on hit: hit-stop, shake, reaction, energy, UI pulse, VFX, SFX?
- How does pressure break create a readable `2 to 3 second` pocket shift?
- Which frame/event makes `Break`, `Tank`, `Arrow`, or `Heal` the correct summon answer?

Do not use it to reintroduce:

- `manual basic attack mash` as the main skill expression
- `many active attack buttons`
- `baseline parry button`
- `full party switching`
- `hard target-selection UI`
- `long fighting-game combo execution`
- `nonstop hit-stun or camera shake that hides danger readability`

Current identity remains:

- `position + direction + dodge + summon timing`
- `direction-biased auto basic fire`
- `one dodge button`
- `one ultimate`
- `one clear summon / assist action slot`
- `summon-first battlefield swing`

Asset/code ownership boundary: do not import proprietary animation clips, full source files, raw tables, meshes, textures, or audio. Extract schemas, timing ranges, field relationships, and authoring ideas.

## Source Inventory

| ID | Game / Source | Type | Useful combat-feel data | Confidence | URL |
|---|---|---:|---|---:|---|
| `zzz_wiki_dodge` | Zenless Zone Zero | Wiki | Dodge, dash attack, Perfect Dodge, Dodge Counter, invulnerability and counter action family | High | https://zenless-zone-zero.fandom.com/wiki/Dodge |
| `zzz_wiki_daze` | Zenless Zone Zero | Wiki | Daze accumulation, Stun, damage multiplier, Chain Attack opportunity and enemy-strength pacing | High | https://zenless-zone-zero.fandom.com/wiki/Daze |
| `zzz_wiki_chain_attack` | Zenless Zone Zero | Wiki | Stun-to-chain flow, enemy strength chain count, assist-like handoff | High | https://zenless-zone-zero.fandom.com/wiki/Chain_Attack |
| `zzz_wiki_decibels` | Zenless Zone Zero | Wiki | Ultimate energy economy, combat actions that grant Decibels, perfect-dodge and assist reward values | Medium | https://zenless-zone-zero.fandom.com/wiki/Decibels |
| `zzz_data_witch_time` | ZZZ data mirror | GitHub data | `Ability_WitchTime`, `DurationTime = 4`, `OnDodgeSuccess`, slowdown/post-process/VR effect keys, invincible modifier | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `zzz_data_frame_mixins` | ZZZ data mirror | GitHub data | `ActwithStateFrameMixin`, `AttachStateWithModifierMixin`, `AnimatorStateName`, `Frame`, `NormalizedTimeLow/High` | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `zzz_data_camera_event` | ZZZ data mirror | GitHub data | Camera modifier attached to normal attack animation states, camera zoom/stretch end actions | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `pgr_wiki_combat_guide` | Punishing: Gray Raven | Wiki | Matrix, 3-Ping, QTE, dodge reward, class roles, super armor references | Medium | https://punishing-gray-raven.fandom.com/wiki/Combat_Guide |
| `pgr_data_fight_tables` | PGR data mirror | GitHub data | `FightConfig`, `RoleEffect`, `ScreenEffect`, `NpcAnimatorAction`, camera parameter and LUT tables | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab |
| `hi3_wiki_combat` | Honkai Impact 3rd | Wiki | Evasion, Ultimate Evasion, attack chains, QTE, special button replacement, invulnerability style | High | https://honkaiimpact3.fandom.com/wiki/Combat |
| `hi3_wiki_time_fracture` | Honkai Impact 3rd | Wiki | Time-slow reward model and QTE routing reference | Medium | https://honkaiimpact3.fandom.com/wiki/Time_Fracture |
| `hi3_data_attack_speed` | HI3 data mirror | GitHub data | `AvatarAttackSpeedParam` with avatar-specific curve constants and fixed-speed flags | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `hi3_data_attack_punish` | HI3 data mirror | GitHub data | `AttackPunishData` level-difference damage reduction curve examples | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `wuwa_wiki_combat` | Wuthering Waves | Wiki | Basic/heavy/mid-air/dodge-counter taxonomy, Intro/Outro, Resonance Skill/Liberation | Medium | https://wutheringwaves.fandom.com/wiki/Combat |
| `wuwa_wiki_vibration` | Wuthering Waves | Wiki | Vibration Strength depletion, immobilize window, counterattack as strong break source | Medium | https://wutheringwaves.fandom.com/wiki/Vibration_Strength |
| `wuwa_wiki_concerto` | Wuthering Waves | Wiki | Concerto Energy, full-gauge switch handoff, Intro/Outro cue readiness | Medium | https://wutheringwaves.fandom.com/wiki/Concerto_Energy |
| `genshin_wiki_hitlag` | Genshin Impact | Wiki | Hitlag, hitlag extension, attacker/target time slowdown, sharpness/poise relation | Medium | https://genshin-impact.fandom.com/wiki/Hitlag |
| `genshin_wiki_interruption` | Genshin Impact | Wiki | Interruption resistance, poise, stagger/knockback relation | Medium | https://genshin-impact.fandom.com/wiki/Interruption_Resistance |
| `local_code_v2` | IsekaiBrawl local code | Repo | Existing energy caps, shot cooldown, camera cue priorities, structure relief constants, summon queue status | High | local files |

## Observed Production Data Shapes

### ZZZ: dodge success routes to a time-and-action reward stack

Public Dodge data separates `Dodge`, `Dash Attack`, `Perfect Dodge`, and `Dodge Counter` as related but distinct action outcomes. The useful shape is not only "press dodge to avoid damage"; it is:

1. Read incoming attack.
2. Enter dodge movement.
3. If timing succeeds, grant an upgraded branch.
4. Let the player counter or continue pressure without manually selecting a new target.

The ZZZ data mirror makes that lower-level structure clearer. `Ability_WitchTime.json` contains:

- `AbilityName = Ability_WitchTime`
- `AbilitySpecials.DurationTime.Value = 4`
- `DefaultModifier.OnDodgeSuccess -> TriggerAbilityAction`
- `BlockManagementModifier.Duration = %DurationTime`
- modifier actions that open/close a `WitchTime_Dodge` block
- slow-down and post-processing keys such as radial blur, vignette, VR effects, fog, screen effects, and color adjustment
- an invincible buff modifier in the same reward stack

IsekaiBrawl application:

- Use a shorter local combat accent than a full copied `4s` WitchTime.
- Recommended first tuning:
  - perfect-dodge visual freeze/slow accent: `0.12s to 0.22s`
  - dodge overdrive / empowered auto-fire: `2.0s to 3.0s`
  - summon opportunity flash: `0.9s to 1.4s`
  - energy reward: `+12 to +18`
- The authored result should read as `perfect dodge -> safe beat -> faster pressure -> correct summon is now attractive`.

### ZZZ: animation-state mixins are better than button-level effects

`Anbi_PerfectCombo.json` shows two useful authoring patterns:

- `AttachStateWithModifierMixin`
- `ActwithStateFrameMixin`

Observed fields include:

- `AnimatorStateName`
- `LayerIndex`
- `Frame`
- `FrameCountLow`
- `FrameCountHigh`
- `MaxFrameCountHigh`
- `ForceTriggerOnTransitionIn`
- `ForceTriggerOnTransitionOut`
- `ModifierNameList`

The important production idea is that a combo modifier can be attached to named animation states and exact frames instead of being fired only from input code.

IsekaiBrawl application:

- Treat every meaningful action as an `ActionFrameProfile`.
- Bind camera, VFX, SFX, hitbox, energy, and summon opportunity to frame tags.
- For current auto basic fire, the "combo" should be a visual beat chain, not manual execution:
  - `AutoBasic_01_Windup`
  - `AutoBasic_01_Release`
  - `AutoBasic_01_Recoil`
  - `AutoBasic_Empowered_Release`

### ZZZ: camera events attach to attack-state ranges

`Billy_CameraEvent.json` shows camera modifier names attached to attack animation states. Examples include:

- `Attack_Normal_02_Start`
- `Attack_Normal_02_A`
- `Attack_Normal_02_B`
- `Attack_Normal_02_End`
- `Attack_Normal_03_Start`
- `Attack_Normal_03_A`
- `Attack_Normal_03_End`
- `CameraEventModifier_Normal_02`
- `CameraEventModifier_Normal_03`

Observed ranges include broad `NormalizedTimeLow = 0.0` to `NormalizedTimeHigh = 1.0` blocks, plus short end blocks such as roughly `0.216` and `0.212` normalized end segments.

IsekaiBrawl application:

- `BattleCameraCueType.Hit` should not trigger on every projectile spawn.
- Trigger it on `hitConfirm` or `impactFrame` tags with a repeat-suppression rule.
- Current `BattleCamera` already has cue priority:
  - `FinalKill = 7`
  - `BossBreak = 6`
  - `Burst = 5`
  - `Dodge = 4`
  - `Hit = 3`
  - `SummonCall = 2`
  - `PressureRead = 1`
- The next pass should author `cueWindow` per action and let priority decide suppression.

### ZZZ: Daze is a pressure-conversion system, not just a stun bar

Public Daze and Chain Attack data expose a useful combat shape:

- attacks build a pressure/stun meter
- reaching the threshold creates a temporary action-stop state
- a heavy/qualifying hit turns the stop state into a follow-up opportunity
- enemy strength changes how many follow-ups are allowed
- the opportunity has a presentation layer: camera, switch/assist, cue, and damage amplifier

IsekaiBrawl application:

- Use `PressureBreak`, not copied Daze.
- Small enemy break: `0.4s to 1.0s` interrupt or knockback.
- Elite break: `1.4s to 2.4s` stagger plus summon call glow.
- Boss pressure break: `2.0s to 4.0s` pocket shift or retreat handoff.
- Final boss break: real kill/damage window plus summon contribution.
- Break must have a visible role answer:
  - `Break` cracks blocker/armor.
  - `Tank` converts danger into aggro relief.
  - `Arrow` interrupts exposed backline/windup.
  - `Heal` sustains the pocket after the player survives danger.

### ZZZ: combat energy can reward precision, not only damage

Public Decibel data is useful because reward sources include precision and team-flow actions, not just raw DPS. The exact values are ZZZ-specific, but the production pattern is important:

- stun/break events grant energy-like value
- interrupt events grant energy-like value
- perfect dodge grants energy-like value
- assist and follow-up actions grant much larger team-flow value
- Ultimate sits behind a clearly named threshold

IsekaiBrawl application:

- Summon energy should not be only passive timer fill.
- First pass event rewards:
  - passive trickle: `0.8 to 1.4 energy / sec`
  - aggressive safe forward zone: `+0.5 to +1.2 / sec`
  - perfect dodge: `+12 to +18`
  - correct role summon call: `+4 to +8 refund`
  - structure/blocker break: `+14 to +22`
  - boss pressure break: `+18 to +28`
  - neutral auto-fire hit: `0 to +0.6`, intentionally low
- This keeps skill expression around reading and positioning, not idle DPS farming.

### PGR: Matrix compresses a dodge reward into time, input, and role flow

PGR's public combat guide is useful for the `Matrix` shape:

- timed dodge creates a slow-time opportunity
- Matrix has a cooldown and cannot be spammed every attack
- Matrix-Ping lets the player treat one signal as a higher-value attack route
- QTE and class roles make the next action feel prepared by the dodge

IsekaiBrawl application:

- Perfect dodge should not be a pure damage nuke.
- It should prepare:
  - empowered basic-fire pressure
  - safer summon call
  - brief camera read
  - role-specific opportunity highlight
- Recommended cooldown feel:
  - base dodge cooldown: `0.75s to 1.15s`
  - perfect-dodge reward internal cooldown: `1.8s to 3.0s`
  - repeated minor dodge camera suppression: `0.08s to 0.14s`

### PGR data mirror: combat presentation is table-driven

Downloaded PGR data mirror files show several useful authoring surfaces:

- `table/client/fight/FightConfig.tab`
- `table/client/fight/RoleEffect.tab`
- `table/client/fight/ScreenEffect.tab`
- `table/client/fight/npc/NpcAnimatorAction.tab`
- `table/client/camera/CameraParam.tab`
- camera/effect/sound LUT tables

Useful field examples:

- `FightConfig.tab`
  - `HpTransitionDuration = 0.8`
  - `EnergyTransitionDuration = 0.3`
  - `HpShakeDuration = 0.2`
  - `HpShakeRange = 3`
- `NpcAnimatorAction.tab`
  - `Stand1`, `Stand2`, `Stand3`, `Run`, `Walk`, `Born`, `Death`
  - `Hit1`, `Hit2`, `Behitfly`
  - `IsTrigger` flag
- `RoleEffect.tab`
  - `Type`
  - `Order`
  - `Models[]`
  - `Intensity`
  - `FadeIn`
  - `FadeOut`
  - `Params[]`
- `ScreenEffect.tab`
  - effect prefab path
  - sort order
  - `PlayAlone`
- `CameraParam.tab`
  - distance
  - zoom speed
  - X/Y angle limits and speed
  - viewport offset

IsekaiBrawl application:

- Current project should keep `CombatCueBundle` data separate from raw action logic.
- A hit reaction should choose:
  - reaction animation tag: `HitLight`, `HitHeavy`, `Launch`, `Down`, `ArmorFlinch`
  - screen effect: `none`, `edgeFlash`, `dangerVignette`, `healPulse`, `breakFlash`
  - camera cue: `Hit`, `Dodge`, `SummonCall`, `BossBreak`, `FinalKill`
  - UI transition duration: HP `0.6s to 0.9s`, energy `0.2s to 0.35s`

### HI3: evasion and QTE are compact action windows

HI3 combat data is useful because it keeps a small button shell but still creates different action outcomes:

- normal attacks progress through a sequence
- evasion can cancel out of pressure
- Ultimate Evasion can trigger a stronger outcome
- QTE is triggered by enemy or battle state instead of manual target selection
- some characters replace the special button or enter alternate modes

IsekaiBrawl application:

- Do not add a parry button.
- Use `PerfectDodgeReward` plus `SummonOpportunity`.
- QTE-like logic maps to summon call:
  - `enemy airborne`
  - `boss break`
  - `structure crack`
  - `player danger`
  - `ally low HP`
  - `backline exposed`

### HI3 data mirror: combat curves are explicit tables

Downloaded HI3 data mirror files show:

- `AvatarAttackSpeedParam.json`
  - `AvatarID`
  - `IsFixAttackSpeed`
  - `K1` through `K12` curve parameters
- `AttackPunishData.json`
  - `LevelDifference`
  - `DamageReduceRate`
  - example curve: `0` through `6` level difference have no reduction, then higher differences step into heavy reduction values

IsekaiBrawl application:

- Do not hide feel tuning inside hardcoded constants only.
- Put attack speed, dodge reward, break resistance, and camera cue durations into named profiles.
- If level/stat growth is later added, preserve a `feel floor`:
  - dodge window should not shrink just because difficulty rises
  - summon opportunity should remain readable
  - break resistance may rise, but feedback should be explicit

### WuWa: counter, vibration break, and Intro/Outro show role handoff timing

WuWa combat is useful for the link between:

- normal/heavy/mid-air/dodge-counter action families
- Vibration Strength as a break/immobilize layer
- Concerto Energy as a handoff readiness layer
- Intro/Outro as a switch-entry presentation layer

IsekaiBrawl application:

- `SummonOpportunityFrameTag` should work like a handoff readiness flag.
- Correct role call should be allowed from combat state, not from manual target selection.
- Suggested first triggers:
  - `perfect_dodge_success -> any front card gets +priority, matching need glows`
  - `boss_pressure_break -> Break or Arrow gets high priority`
  - `player_hp_danger -> Tank or Heal gets high priority`
  - `backline_windup -> Arrow gets high priority`
  - `structure_cracked -> Break gets high priority`

### Genshin: hitlag and poise show why small freezes matter

Genshin's public combat mechanics are useful because they separate:

- hitlag as time manipulation for impact readability
- hitlag extension when striking valid active enemies
- poise/interruption resistance as a hidden stability layer
- stagger/knockback as a result of poise damage and resistance

IsekaiBrawl application:

- Use hit-stop very sparingly:
  - light auto-fire impact: `0.02s to 0.04s`
  - empowered basic impact: `0.04s to 0.07s`
  - summon entry hit: `0.06s to 0.10s`
  - boss break hit: `0.10s to 0.16s`
  - ultimate final hit: `0.14s to 0.22s`
- Hit-stop should never stack into unreadable projectile danger.
- Use hit reaction profiles instead of generic damage numbers.

### IsekaiBrawl local code: current feel hooks already exist

Local code already exposes useful hooks and current mismatches:

- `BattleEnergySystem`
  - default max/start examples: `130 / 40`
  - Story PvE examples: `124 / 28`
  - stage overrides can alter starting energy
- `PlayerSkillController`
  - current `shotCooldown = 0.72`
  - empowered shot cooldown multiplier around `0.68`
  - impact shake defaults around `0.08s / 0.08`
  - dodge overdrive exists as a post-dodge pressure layer
- `BattleCamera`
  - cue types already include `PressureRead`, `Dodge`, `Burst`, `Hit`, `SummonCall`, `BossBreak`, `FinalKill`
  - repeat minor cue recovery and priority suppression already exist
- `StoryPveEncounterRuntime`
  - current blocker break breathing window is about `0.9s`
  - boss cue request durations are about `0.26s` for boss break and `0.42s` for final kill
- `SummonPresentationUtility`
  - current need labels already map to `BREAK`, `TANK`, `ARROW`, `HEAL`

IsekaiBrawl application:

- The project does not need a new combat-feel engine first.
- It needs a small authoring layer that feeds existing hooks with consistent tags.
- Current `0.9s` blocker breathing is probably too short for the documented `2 to 3 second` pocket shift; the next implementation pass should test `1.6s to 3.0s` relief envelopes.

## IsekaiBrawl Frame Tag Dictionary

Author in seconds as the source of truth, with optional 60fps frame equivalents for tuning review.

At 60fps:

- `0.05s = 3f`
- `0.083s = 5f`
- `0.10s = 6f`
- `0.15s = 9f`
- `0.20s = 12f`
- `0.30s = 18f`
- `0.50s = 30f`
- `0.72s = 43f`
- `1.00s = 60f`
- `2.00s = 120f`
- `3.00s = 180f`

Recommended tags:

| Tag | Meaning | Typical range |
|---|---|---:|
| `read` | Telegraph/readable danger or opportunity begins | `0.25s to 0.90s` |
| `startup` | Actor commits before active threat or hitbox | `0.12s to 0.45s` |
| `commit` | Player cannot freely redirect without cancel | action-specific |
| `active` | Hitbox/projectile/heal/taunt/effect is live | `0.04s to 0.45s` |
| `hitConfirm` | First valid hit point for reward/cue | frame or event |
| `hitstop` | Micro freeze/impact pause | `0.02s to 0.22s` |
| `reaction` | Target stagger/armor flinch/launch/down | `0.12s to 1.40s` |
| `cancelToDodge` | Dodge cancel allowed | usually after `hitConfirm` or late `startup` |
| `cancelToSummon` | Summon call accepted safely | `0.12s to 1.40s` window |
| `counterWindow` | Perfect dodge/counter branch accepted | `0.12s to 0.22s` |
| `recovery` | Actor regains normal control | `0.10s to 0.60s` |
| `relief` | Enemy pressure is visibly lower | `1.6s to 3.0s` target for good summon/break |
| `cleanup` | Screen/camera/UI effects fade and reset | `0.10s to 0.45s` |

## Proposed Data Contracts

### `CombatActionFrameProfile`

Use this for player, summon, enemy, structure, and boss actions.

```json
{
  "id": "player_auto_basic_empowered_01",
  "actorKind": "player",
  "actionKind": "auto_basic",
  "durationSeconds": 0.49,
  "frames60": 29,
  "windows": [
    { "tag": "startup", "start": 0.0, "end": 0.10 },
    { "tag": "active", "start": 0.10, "end": 0.16 },
    { "tag": "recovery", "start": 0.16, "end": 0.49 }
  ],
  "cancelRules": [
    { "to": "dodge", "fromTag": "startup", "after": 0.06 },
    { "to": "summon_call", "fromTag": "hitConfirm", "windowSeconds": 0.75 }
  ],
  "hitstopProfileId": "hitstop_empowered_light",
  "cameraCueIds": ["hit_empowered_light"],
  "vfxCueIds": ["sword_wave_empowered"],
  "sfxCueIds": ["blade_release_high"],
  "energyEvents": ["auto_hit_small"],
  "summonOpportunityTags": ["basic_fire_beat"]
}
```

### `DodgeCounterWindowProfile`

Use this for the one-button dodge shell.

```json
{
  "id": "dodge_default_v1",
  "totalDuration": 0.62,
  "inputBuffer": 0.1,
  "invulnerableFrom": 0.05,
  "invulnerableTo": 0.40,
  "perfectWindowBeforeImpact": 0.18,
  "perfectWindowAfterImpact": 0.06,
  "recovery": 0.18,
  "perfectReward": {
    "slowAccent": 0.16,
    "overdriveDuration": 2.4,
    "energy": 14,
    "summonOpportunity": 1.2,
    "cameraCue": "dodge_counter_snap"
  }
}
```

### `HitReactionProfile`

Use this to avoid generic damage-only feedback.

```json
{
  "id": "enemy_light_stagger",
  "hitStrength": "light",
  "poiseDamage": 12,
  "staggerSeconds": 0.24,
  "knockbackMeters": 0.35,
  "canInterruptWindup": false,
  "armorInteraction": "flinch_if_unarmored",
  "cameraCue": "hit_light",
  "screenEffect": "none",
  "vfxCue": "small_spark"
}
```

### `PressureBreakProfile`

Use this for blocker crack, elite break, and boss pressure break.

```json
{
  "id": "boss_pressure_break_short",
  "targetKind": "boss_pressure_body",
  "breakGauge": 100,
  "requiredEvents": ["perfect_dodge", "correct_summon", "structure_crack"],
  "immobilizeSeconds": 1.0,
  "reliefSeconds": 2.4,
  "damageMultiplier": 1.15,
  "summonFollowupWindow": 1.4,
  "cameraCue": "boss_break_pullback",
  "uiCallout": "OPEN"
}
```

### `CombatEnergyEvent`

Use this to make energy economy visible and tunable.

```json
{
  "id": "perfect_dodge_reward",
  "source": "perfect_dodge",
  "amount": 14,
  "internalCooldown": 2.0,
  "uiPulse": "energy_gold_flash",
  "summonHintPriorityBonus": 2,
  "notes": "Rewards reading and timing more than raw DPS."
}
```

### `CombatCueBundle`

Use this to bind camera, screen, audio, VFX, and hit-stop as one unit.

```json
{
  "id": "summon_break_entry_hit",
  "cameraCue": {
    "type": "SummonCall",
    "duration": 0.28,
    "priority": 2
  },
  "hitstop": 0.08,
  "shake": {
    "duration": 0.10,
    "magnitude": 0.13
  },
  "screenEffect": {
    "type": "break_flash",
    "fadeIn": 0.03,
    "fadeOut": 0.18
  },
  "vfx": ["summon_landing_ring", "armor_crack_sparks"],
  "sfx": ["summon_drop", "crack_high"],
  "cleanupSeconds": 0.22
}
```

### `SummonOpportunityFrameTag`

Use this to make summons feel like ARPG assists without adding manual targeting.

```json
{
  "id": "opportunity_backline_windup_arrow",
  "trigger": "enemy_backline_windup_read",
  "role": "Arrow",
  "validFrom": 0.0,
  "validUntil": 1.2,
  "priority": 3,
  "targetingRule": "highest_priority_backline_windup",
  "pocketShiftTarget": "interrupt_cast",
  "uiHint": "ARROW"
}
```

## First Tuning Envelopes

### Player movement and dodge

- Base dodge total: `0.55s to 0.70s`
- Invulnerable section: `0.28s to 0.42s`
- Perfect timing acceptance: `0.14s to 0.22s`
- Input buffer: `0.08s to 0.12s`
- Dodge recovery before full control: `0.12s to 0.22s`
- Perfect-dodge energy: `+12 to +18`
- Perfect-dodge overdrive: `2.0s to 3.0s`
- Perfect-dodge camera cue: `0.20s to 0.32s`

### Auto basic fire

- Current local baseline: `shotCooldown = 0.72s`, about `43f` at 60fps.
- Current empowered multiplier: about `0.68`, resulting in about `0.49s`, about `29f`.
- Neutral hit-stop: `0.02s to 0.04s`.
- Empowered hit-stop: `0.04s to 0.07s`.
- Neutral hit energy should stay low: `0 to +0.6`.
- Direction bias should be readable but not hard aim.

### Summon call

- Input acceptance after opportunity: `0.75s to 1.4s`.
- Entry anticipation: `0.10s to 0.18s`.
- Entry impact: `0.18s to 0.32s`.
- Summon entry hit-stop: `0.06s to 0.10s`.
- Correct-role refund: `+4 to +8`.
- Visible pocket shift target: `2.0s to 3.0s`.
- Global summon cooldown should stay clear in UI before increasing energy complexity.

### Hit reaction and pressure break

- Light enemy stagger: `0.18s to 0.35s`.
- Heavy enemy stagger: `0.40s to 0.85s`.
- Launch/down for small enemy: `0.70s to 1.25s`.
- Elite armor flinch: `0.10s to 0.22s`, with reduced knockback.
- Elite pressure break: `1.4s to 2.4s`.
- Boss pressure break: `2.0s to 4.0s`, but should often move/retreat rather than stand still.
- Blocker break relief: test `1.6s`, `2.2s`, and `3.0s`; current `0.9s` is likely too short.

### Cue bundle intensity

- Repeated light hit: no FOV change, tiny shake only if needed.
- Empowered hit: short shake and VFX size increase, no long camera steal.
- Perfect dodge: lateral/target snap plus time accent.
- Summon call: focus bias to landing or target pocket.
- Boss break: pullback/focus reset so the player sees new safe pocket.
- Final kill: highest priority cue, but still cleanly returns control.

## First Production Profiles

### `player_dodge_default`

- total: `0.62s`
- input buffer: `0.10s`
- invulnerability: `0.05s to 0.40s`
- perfect window: `0.18s before contact` to `0.06s after contact`
- recovery: `0.18s`
- perfect reward: `0.16s slow accent`, `+14 energy`, `2.4s overdrive`, `1.2s summon opportunity`

### `player_auto_basic_neutral`

- cooldown: `0.72s`
- impact cue: light
- hit-stop: `0.03s`
- shake: `0.05s / 0.05`
- energy: `0 to +0.4`
- summon opportunity: only if a role need is already active

### `player_auto_basic_empowered`

- cooldown: `0.49s`
- impact cue: empowered light
- hit-stop: `0.05s`
- shake: `0.07s / 0.08`
- energy: `+0.4 to +0.8`
- summon opportunity: `basic_fire_beat`, `0.75s`

### `summon_break_entry`

- anticipation: `0.14s`
- impact: `0.24s`
- hit-stop: `0.08s`
- structure/break damage multiplier: keep role-differentiated
- blocker relief: test `2.2s`
- energy refund if correct: `+6`
- camera: `SummonCall`, then `BossBreak` or `Hit` only if break succeeds

### `boss_pressure_break`

- trigger sources: perfect dodge chain, correct summon, structure crack, ultimate hit
- break cue: `0.26s to 0.36s`
- relief: `2.4s`
- summon follow-up: `1.4s`
- camera: `BossBreak`
- UI: one callout, not a combat log stack

## Implementation Recommendations

1. Add a data-only `CombatActionFrameProfile` authoring surface before touching many action scripts.
2. Start with seconds and generated frame equivalents at 60fps; do not hardcode frame-count-only logic in Unity gameplay.
3. Wire existing `BattleCamera.RequestCue` from frame tags instead of ad hoc call sites.
4. Add one central `CombatCueBundle` table so hit-stop, shake, VFX, SFX, and UI pulse stay synchronized.
5. Tune energy as event rewards first, not as passive trickle only.
6. Preserve one-button dodge and summon-first identity; do not add a parry or manual combo layer.
7. Validate feel with video/frame review: record at 60fps, tag `read/startup/active/hit/recovery/relief`.

## Next Smallest Follow-Ups

1. Create `COMBAT_FEEL_FRAME_SPEC.md` with final v1 profile IDs and initial numeric values.
2. Add `CombatActionFrameProfile` and `CombatCueBundle` data structures or ScriptableObject equivalents.
3. Do one Unity playtest pass only on `player_dodge_default`, `player_auto_basic_empowered`, `summon_break_entry`, and `boss_pressure_break`.
4. Capture a 60fps video of one run and tag five moments: first dodge, first summon call, first blocker break, first boss break, final kill.
