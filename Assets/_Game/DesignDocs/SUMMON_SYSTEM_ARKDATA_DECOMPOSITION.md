# Summon System ArkData Decomposition

Last updated: 2026-06-29 KST

## Purpose

This document turns the local ArkData summon findings into a practical `DimensionBrawl` design decomposition.

The goal is not to import proprietary data, assets, tables, animation clips, audio, textures, meshes, or source code. Use the datasets as production references for schema shape, combat relationships, target policy, timing grammar, and presentation cue relationships.

The immediate product problem:

- Current summons can read too much like extra bullets, skills, or ordinary enemies.
- The first improved summon should read like a temporary mid-boss actor: large body, slow commitment, visible HP/screen presence, heavy entry, and a boss-lane attack loop.
- The summon must change the battlefield exchange, not only add hidden damage.

## Source Priority

| Priority | ArkData source | Best use | Directness |
|---|---|---|---|
| P0 | `\\DESKTOP-69817L3\ArkData\PGR_Tutorial_Stage_Data_2026-06-19` | Summon mechanics, boss-add interaction, boss attack grammar, VFX cue references | High |
| P0 | `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\girls-frontline` | Summoned unit schema, target AI policies, projectile/hurt/buff payload shape | High |
| P0 | `\\DESKTOP-69817L3\ArkData\CombatPayload_ApplyData_2026-06-25` | Runtime decomposition model from planning language to implementation language | High |
| P1 | `\\DESKTOP-69817L3\ArkData\NarrativeCinematicFeaturePack\pgr_cinematic_deep_dive` | Entry/cut-in/presentation/camera cue grammar | Medium |
| P1 | `\\DESKTOP-69817L3\ArkData\<PGR work summary>\unity_export` | Camera timing, draw/entry presentation metadata | Medium |
| P2 | `\\DESKTOP-69817L3\ArkData\HI3_CombatCutscene_ApplyData_2026-06-26` | QTE/switch/burst vocabulary, shield/stun/time-slow reference | Medium |
| P3 | `\\DESKTOP-69817L3\ArkData\pgr_base_assets` | Raw Unity asset-bundle material only; extraction required before practical use | Low |

## Existing Local Surface

The current project already has a usable summon skeleton:

| Local surface | Current role | ArkData design pressure |
|---|---|---|
| `_Game/Scripts/Player/SummonSlotActionProfile.cs` | Per-tier summon action data: damage, projectile, actor, screen, counter settings | Keep as the first tuning surface for Slot1, but it is too projectile-centric for heavy pattern authoring. |
| `_Game/Scripts/Player/PlayerSummonSlot1Action.cs` | Spends EN, spawns entry cue, actor, pressure screen, and projectiles | Good first implementation target. Needs heavier tier tuning before new systems. |
| `_Game/Scripts/Combat/SummonFrontlineProxy.cs` | Runtime actor lifecycle: activate, advance, HP, lifetime, defeat, recall | Good actor body. Needs explicit attack-pattern identity later. |
| `_Game/Scripts/Combat/SummonPressureScreen.cs` | Intercepts hostile projectiles by radius, intercept count, lifetime | Strong match for Tank/ShieldBreaker and PGR-style boss pressure answers. |
| `_Game/Scripts/Combat/SummonOpportunityWindowProfile.cs` | Opportunity windows and follow-up timing | Good place to express boss pressure break, close-threat relief, perfect dodge, structure break. |
| `_Game/Scripts/Data/SummonData.cs` | Older prototype deck in `IsekaiBrawl.Gameplay` namespace | Useful as historical role list only. Do not build the new V1 design around this old deck shape. |
| `_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ShieldBreaker.asset` | Current main Slot1 profile | First retune candidate for a mid-boss-like summon. |
| `_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_BacklineMarksman.asset` | Ranged support actor | Keep as artillery/marksman support after Slot1 feels dominant. |
| `_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_VanguardCommander.asset` | Tank/vanguard support actor | Keep as second tank expression after Slot1 establishes the base grammar. |

## Dataset Decomposition

### 1. Summon As Combat Actor

Primary source:

- `SubcultureGameData/games/girls-frontline/raw/.../StcSummoner.json`
- `CombatPayload_ApplyData_2026-06-25/docs/combat_payload_family_guide.md`

Observed data shape:

- Girls' Frontline `StcSummoner.json` has about `792` summon definitions by line-count proxy.
- Repeated fields include `code`, `hp`, `pow`, `hit`, `dodge`, `range`, `def`, `speed`, `number`, `armor_piercing`, `shield`, `rate`, `level`, `normal_attack`, `passive_skill`, `dynamic_passive_skill`, `unable_buff_id`, `scale`, `start_offset`, `camp`, `is_stay`, `is_hp_showed`, `is_damage_showed`, `is_damage_source`, and `is_sangvis`.
- Common code families include `Jidongyanti`, `Dinergate`, `gunslinger_drone`, `Strelet`, `Drone`, `Agent`, `Doppelsoldner`, and `Rodelero`.

Local design extraction:

- A summon definition needs body stats, spawn offset, scale, attack refs, passive refs, visibility rules, and team ownership.
- `SummonFrontlineProxy` already covers actor activation, HP, lifetime, scale, advance, and defeat.
- Missing first-class design fields: `targetPolicy`, `attackPatternSet`, `passiveStateRules`, `visibilityPolicy`, and `sourceCreditPolicy`.

Practical V1 decision:

- Treat Slot1 as `ShieldBreakerTitan`, a real actor with HP, lifetime, pressure screen, and heavy attack cadence.
- Do not let Slot1 read as only `projectileCount + entryCue`.

### 2. Target Policy

Primary source:

- `SubcultureGameData/games/girls-frontline/combat/gf1-target-ai-semantic-candidates.csv`
- `CombatPayload_ApplyData_2026-06-25/docs/combat_payload_family_guide.md`

Observed data shape:

- Target policy rows separate side, count, priority, ordering, and range policy.
- Useful semantic candidates include:
  - `self-single-default-priority`
  - `ally-all-or-unspecified-default-priority`
  - `enemy-all-or-unspecified-default-priority`
  - `enemy-single-boss-priority`
  - `enemy-single-ordered-stat-priority`
  - `enemy-multi-3-ordered-stat-priority`
  - `enemy-multi-5-ordered-stat-priority`

Local design extraction:

- Summons should prefer the far/frontline boss exchange by default.
- Local close-threat target selection should remain a player local-defense concern unless the summon role explicitly says otherwise.
- Anti-summon or anti-add behavior should be a separate target policy, not an incidental fallback.

Practical V1 target policies:

| Policy id | Use | Local rule |
|---|---|---|
| `FrontlineBossPriority` | Slot1 ShieldBreakerTitan | Prefer `frontlineTargetHealth`; fallback to boss proxy lane point. |
| `NearestBossLaneThreat` | Basic counterfire | Choose nearest hostile in boss/frontline side, not close player-side enemy. |
| `Multi3NearestFrontline` | BacklineMarksman | Fire spread/volley into up to three frontline threats. |
| `SummonedEntityPriority` | Future anti-summon answer | Prefer enemy summoned add, then boss. |
| `AllyPressureRelief` | Future heal/support | Prefer player or ally summon under pressure, no manual targeting UI. |

### 3. Attack Pattern

Primary source:

- `PGR_Tutorial_Stage_Data_2026-06-19/raw-json/.../DlcHuntBossDetail.json`
- `PGR_Tutorial_Stage_Data_2026-06-19/raw-json/.../BlackRockChessEffect.json`
- `SubcultureGameData/games/girls-frontline/combat/gf1-projectile-creation-profile.csv`

Observed boss-pattern grammar:

- Heavy attacks are described as committed actions with readable setup:
  - retreat or gather strength, then fire a cannon shot
  - follow with tracking orbs for ranged targets
  - rush forward, fly up, then land/crash on the aggro target
  - fire two lasers into a large forward area, then apply local AOE
  - fire massive missiles with travel and landing timing

Observed projectile shape:

- Girls' Frontline projectile rows expose projectile family, start/destination/route type, speed, duration, area bucket, hurt payload, buff payload, scale, and sound order.

Local design extraction:

- Current `SummonTierSettings` supports projectile count, speed, radius, lifetime, damage, and counter projectile values.
- It does not yet express named attack patterns, windup/telegraph/recovery, beam width, landing smash, repeated missile waves, or pattern cooldown.

Practical V1 attack pattern set:

| Pattern id | Tier | Behavior | Implementation now | Needs later |
|---|---:|---|---|---|
| `EntrySmash` | 1-3 | Large body appears in front of player, advances, flashes/lands with screen presence | Entry cue + actor scale + presenter flash | Dedicated landing damage/knockback event |
| `PressureScreenBlock` | 1-3 | Intercepts boss barrage, then fires counter projectile | Existing `SummonPressureScreen` + counter projectile | Stronger screen material/camera cue tuning |
| `HeavyCannonCounter` | 1-3 | Slow heavy cannon shot toward boss lane after block or engage | Existing counter projectile values | Windup/telegraph and named pattern profile |
| `LightningLine` | 2-3 | Committed line/beam-style attack | Approximate with projectile spread for now | Beam/line attack profile |
| `LandingCrash` | 3 | Mid-boss-like body slam or ground pressure | Not present | Attack pattern profile + area hit |

### 4. Status And State Rules

Primary source:

- `CombatPayload_ApplyData_2026-06-25/focused_lastorigin_skill_buff_state`
- `PGR_Tutorial_Stage_Data_2026-06-19/raw-json/.../TeachingRobot.json`

Observed data shape:

- Last Origin focused manifest contains `8012` skill rows, `13924` skilllevel rows, `8447` buffeffect rows, `43587` join rows, and `41` summon skilllevels.
- High-value state families include core stat modifiers, AP/tempo, target marks, reaction/extra attacks, control restrictions, dispel, barrier/damage reduction, guard pierce, and stack/duration/erase policies.
- PGR TeachingRobot examples use summoned adds as boss-state gates: destroy all summoned adds to paralyze or weaken the boss, or to end a laser-rain state.

Local design extraction:

- Slot1 should create visible pressure relief through screen intercepts and boss-lane counterfire first.
- Later, enemy summons should become boss-state gates: defeating summoned adds ends a pattern or weakens the boss.
- Player summons can apply short `PressureBreak`, `MarkedDamageAmp`, or `BossPatternSuppression`, but those states should be authored as explicit rules.

Practical state rules:

| State rule | Owner | V1 use |
|---|---|---|
| `PressureScreenIntercept` | Player summon | Already implemented; intercept count and radius scale by tier. |
| `BossPressureBreak` | Boss/proxy | Trigger after correct summon block, opening Skill1 follow-up. |
| `SummonBodyThreat` | Summon actor | Boss pattern should visually acknowledge the summon/frontline actor. |
| `MarkedDamageAmp` | Future summon | Short post-block boss vulnerability window. |
| `SummonedAddGate` | Future boss summon | Destroy adds to end enemy laser/rain/immune state. |

### 5. Presentation Cue Bundle

Primary source:

- `PGR_Tutorial_Stage_Data_2026-06-19/raw-json/.../BlackRockChessEffect.json`
- `NarrativeCinematicFeaturePack/pgr_cinematic_deep_dive/pgr_presentation_fx_catalog.csv`
- `NarrativeCinematicFeaturePack/pgr_cinematic_deep_dive/pgr_skill_cutin_candidate_catalog.csv`

PGR BlackRockChessEffect cue references worth mapping to local cue intent:

| PGR id | Note | Reference prefab path | Local cue intent |
|---:|---|---|---|
| 11 | Cannon Firing | `Assets/Product/Effect/Prefab/FxWanfa/FxBRSBigGunShoot.prefab` | Heavy cannon muzzle/read |
| 12 | Cannon Flying | `Assets/Product/Effect/Prefab/FxWanfa/FxBRSBigGunBullet.prefab` | Heavy projectile body/read |
| 13 | Cannon Exploded | `Assets/Product/Effect/Prefab/FxWanfa/FxBRSBigGunEX.prefab` | Boss-lane impact |
| 68 | Screen shake effect | `Assets/Product/Effect/Prefab/FxWanfa/Fxpingfengzdpfjiesuan.prefab` | Entry/impact camera read |
| 74 | Lightning skill expression | `Assets/Product/Effect/Prefab/FxWanfa/FxReXiao.prefab` | Pre-attack expression |
| 75 | Lightning skill effect | `Assets/Product/Effect/Prefab/FxWanfa/FxLightningChess.prefab` | Lightning line/strike |
| 76 | Signature summoning expression | `Assets/Product/Effect/Prefab/FxWanfa/FxReShengqi.prefab` | Summon entry burst |
| 77 | Signature disappearance effect | `Assets/Product/Effect/Prefab/FxWanfa/FxShunYiChess.prefab` | Recall/exit dissolve |
| 78 | Permanent lock effect | `Assets/Product/Effect/Prefab/FxWanfa/FxAttackPlace.prefab` | Ground warning / lock area |
| 87 | Floor smash | `Assets/Product/Effect/Prefab/FxWanfa/FxDownPunchRed.prefab` | Landing/entry smash |
| 92 | Enlarged floor smash | `Assets/Product/Effect/Prefab/FxWanfa/FxDownPunchRed.prefab`, scale `2|2|2` | LV3 landing smash |
| 97 | Lotus Calibur | `Assets/Product/Effect/Prefab/FxWanfa/FxCaliburChess.prefab` | Wide slash/line pressure |

Local design extraction:

- Do not copy the PGR prefab assets.
- Use these rows to name local cue jobs: `SummonEntryBurst`, `FloorSmashLarge`, `HeavyCannonMuzzle`, `HeavyCannonProjectile`, `HeavyCannonImpact`, `LightningLine`, `GroundLockWarning`, `RecallDissolve`.
- Presentation must include cleanup. Entry, attack, screen, exit, UI, camera, and VFX lifetime should be explicit.

## Concrete V1 Design: ShieldBreakerTitan

This is the first practical retune target, implemented through the existing Slot1 surfaces before adding new classes.

### Identity

| Field | Value |
|---|---|
| Slot | `SummonSlot1` |
| Working id | `ShieldBreakerTitan` |
| Existing profile to retune first | `DB_SummonSlot1_ShieldBreaker.asset` |
| Role | Tank + Break |
| Primary job | Block boss barrage, occupy frontline, counterfire into boss lane |
| Target policy | `FrontlineBossPriority` |
| Entry read | Player-front magic/summon cue, large body, slow advance |
| Attack read | Heavy cannon or lightning-like counter, not rapid small bullets |
| State read | Visible HP/body presence + pressure screen intercept budget |

### Tier Tuning Target

These are design targets, not final balance. They intentionally push body scale, HP, and cadence above the current `ShieldBreaker` profile so the first summon reads as a temporary mid-boss.

| Tier | ActorScale | ActorMaxHealth | ActorLifetime | MoveSpeed | Advance | Screen | Attack cadence | Projectile read |
|---:|---:|---:|---:|---:|---|---|---|---|
| LV1 | `2.6` | `320` | `5.0s` | `1.20` | `2.0m / 1.8s` | `2` intercepts, radius `1.55`, `2.6s` | `0.60s` interval | one heavy bolt |
| LV2 | `3.25` | `470` | `6.5s` | `1.05` | `2.8m / 2.2s` | `4` intercepts, radius `1.9`, `3.5s` | `0.70s` interval | two heavy bolts or one wider line |
| LV3 | `3.9` | `680` | `8.0s` | `0.95` | `3.6m / 2.75s` | `7` intercepts, radius `2.35`, `4.5s` | `0.85s` interval | one committed cannon/beam with impact |

Immediate changes should reduce "small bullet spam" by increasing impact scale and slowing cadence. LV3 should feel like the screen belongs to the summon for a moment.

### First Pass Through Existing Fields

Use existing `SummonTierSettings` before adding new code:

| Current field | Retune meaning |
|---|---|
| `ActorScale` | Main silhouette lever. Raise beyond current `2.025/2.43/2.88`. |
| `ActorMaxHealth` | Make the actor feel killable but not disposable. Raise above current `230/300/380`. |
| `ActorLifetimeSeconds` | Keep the body present long enough to read entry, block, and counter. |
| `ActorMoveSpeed` | Slow it down so it reads as weight, not a dash pet. |
| `ActorAdvanceSeconds` | Increase so advance is visible. |
| `ActorAttackDamagePerSecond` | Lower sustained DPS if needed, but make each hit look heavier. |
| `ActorAttackIntervalSeconds` | Increase from current `0.35s` toward `0.6s-0.85s`. |
| `ScreenIntercepts` | Keep tier identity: `2/4/7` is already good. |
| `ScreenRadius` | Increase slightly so the screen reads physically larger with the body. |
| `CounterDamage` | Make counter projectiles meaningful, especially after block. |
| `ProjectileCount` | Avoid making LV3 feel like three normal bullets. Prefer fewer, heavier shots once pattern support exists. |
| `CueScale` and `CueLifetimeSeconds` | Current cue scale is small; raise entry cue read only if local VFX remains clean. |

## Needed Data Profiles

The current profile is serviceable for a retune, but a dataset-shaped summon system should split concerns after the first pass.

### `SummonDefinitionProfile`

Purpose: owns summon identity and high-level role.

Suggested fields:

- `SummonId`
- `SlotId`
- `RoleTags`
- `TargetPolicyId`
- `BodyPrefab`
- `PresentationProfile`
- `TierProfiles`
- `SourceCreditPolicy`
- `VisibilityPolicy`

### `SummonTierProfile`

Purpose: owns per-tier body, screen, and pattern choices.

Suggested fields:

- `Tier`
- `ActorScale`
- `MaxHealth`
- `LifetimeSeconds`
- `MoveSpeed`
- `AdvanceDistance`
- `AdvanceSeconds`
- `PressureScreenProfile`
- `PrimaryAttackPattern`
- `CounterAttackPattern`
- `StateRules`
- `Readout`

### `SummonAttackPatternProfile`

Purpose: prevents every summon from collapsing into projectile count, radius, and speed.

Suggested fields:

- `PatternId`
- `PatternFamily`: `Smash`, `Cannon`, `Beam`, `Lightning`, `MissileVolley`, `ScreenCounter`, `HealField`
- `TargetPolicyId`
- `WindupSeconds`
- `TelegraphSeconds`
- `ActiveSeconds`
- `RecoverySeconds`
- `CooldownSeconds`
- `Damage`
- `ProjectileSpeed`
- `ProjectileCount`
- `LineWidth`
- `SplashRadius`
- `CanTriggerPressureBreak`
- `CueBundleId`

### `SummonTargetPolicyProfile`

Purpose: formalizes target selection without adding manual target UI.

Suggested fields:

- `TargetSide`
- `TargetCount`
- `PriorityKind`
- `OrderKind`
- `RangePolicy`
- `LanePolicy`
- `FallbackPolicy`

### `SummonPresentationCueBundle`

Purpose: keeps entry, attack, impact, exit, camera, UI, and cleanup explicit.

Suggested fields:

- `EntryCue`
- `EntryCameraCue`
- `GroundWarningCue`
- `AttackWindupCue`
- `ProjectileCue`
- `ImpactCue`
- `ScreenCue`
- `ExitCue`
- `CleanupPolicy`

## Implementation Sequence

### Step 1: Profile retune only

Retune `DB_SummonSlot1_ShieldBreaker.asset` toward `ShieldBreakerTitan`:

- Increase actor scale, HP, lifetime, and screen radius.
- Slow actor movement and attack cadence.
- Keep intercept tier counts.
- Make readout text emphasize giant body, screen block, and heavy counterfire.
- Keep this change contained to the profile asset and PlayMode expectations.

Why first:

- Existing code can already prove whether a larger, slower, tankier actor fixes the "bullet-like summon" read.
- No new class is needed to test presence.

### Step 2: Presentation cue pass

Use existing local VFX/audio cue infrastructure:

- Map `SummonEntryBurst`, `FloorSmashLarge`, `HeavyCannonMuzzle`, `HeavyCannonImpact`, `GroundLockWarning`, and `RecallDissolve` to local cue equivalents.
- Add a short additive camera read for entry and pressure-screen block.
- Avoid long cinematic locks during normal summon use.

### Step 3: Attack pattern split

Add `SummonAttackPatternProfile` after the profile retune proves the desired read:

- Convert counter projectile settings into a named `HeavyCannonCounter`.
- Add `LightningLine` or `LandingCrash` as the first non-projectile-count pattern.
- Keep `PlayerSummonSlot1Action` responsible for input/spend only; move pattern execution into a narrow executor.

### Step 4: Enemy summon gate

Use PGR TeachingRobot grammar:

- Boss summons adds.
- While adds exist, boss gains a strong state such as shield, invulnerability, laser rain, or damage reduction.
- Destroying all adds ends the state and opens a pressure break.

This should be implemented only after the player-side summon has enough presence, because enemy-side summons will otherwise add noise before the core fantasy is fixed.

## Acceptance Criteria

The first practical design pass is successful when:

- Slot1 summon is readable as a large actor before its projectile hits.
- The player can describe what the summon did: blocked boss fire, occupied frontline, countered the boss.
- LV1/LV2/LV3 read as the same summon getting stronger, not three unrelated effects.
- The summon prefers the boss/frontline exchange even when local-defense target selection is pointed at a close threat.
- Pressure screen intercepts produce in-world and camera feedback, not only projectile deletion.
- The actor can be damaged/defeated or time out, so it is not an invisible buff.
- The next data gap is clear: named attack patterns, not more raw projectile fields.
