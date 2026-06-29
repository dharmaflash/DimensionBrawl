# Summon Presentation VFX Direction

Date: 2026-06-29

This note locks the presentation direction for the next summon-feel pass. It
extends `SUMMON_SYSTEM_ARKDATA_DECOMPOSITION.md`, which currently focuses on
Slot1/ShieldBreakerTitan, and applies the later Frontline report evidence to
Slot2, Slot3, and LV3/high-tier summon reads.

## Evidence Lock

Authoritative inputs:

- Current conversation goal: make summons feel flashy, strong, and effective
  without using current bullet/enemy-projectile presentation as the answer.
- Deep-link thread `019f039a-824b-7f80-bc52-a7644d4cfc53`: Slot2 and Slot3 are
  not interchangeable upgrades.
- `FRONTLINE_COMBAT_STRUCTURE_ANCHOR.md`: Slot2 is a tempo/marksman damage route;
  Slot3 is a high-cost prevention route that buys boss-screen suppress or safer
  counter recovery after an LV3 wait.
- `SUMMON_SYSTEM_ARKDATA_DECOMPOSITION.md`: summons should read as combat actors
  and committed stage events, not small projectile spam.
- ArkData CombatPayload model: `Action -> Trigger -> Target -> Effect/Status ->
  Hit -> Presentation -> Result` should stay connected.
- Owned local effects: `_Game/Art/VFX/CombatCues` already contains promoted
  stable cue prefabs; raw `_Imported` packages are promotion candidates only.

Important current numbers from the anchor:

- Slot2 full-bank: tempo/no-recovery marksman route, `support_marksman_clear`.
- Slot3 hold-front: about `11.3s`, HP about `70.9`, suppress `2/3`,
  `support_vanguard_clear`.
- Slot3 retreat/recommit: about `17.7s`, HP about `54.8`, back/forward agency,
  `counter_recovery`.
- LV3 direct suppress already preserves hit-confirm presentation through a
  non-interrupting frame overlay while stronger cut-ins remain active.

## Non-Goals

- Do not treat Slot3 as a universal damage upgrade over Slot2.
- Do not buff the Vanguard body just because Slot3 costs HP before support;
  the body-cost evidence shows that tax is paid before the Vanguard appears.
- Do not solve the pass with more small bullets, enemy projectile reuse, or
  current temporary projectile-only cues.
- Do not point runtime profiles directly at raw `_Imported` assets. Promote
  reviewed VFX/SFX into `_Game` first.
- Do not introduce reward economy, final coaster UI, broad HUD polish, or a new
  summon combat system in the presentation pass.

## Presentation Spine

Each important summon should read as a short assist/QTE event:

1. Decision signal: the player understands what role is being bought.
2. Entry: the summon body or strike source appears with a readable lane claim.
3. Lock-on or ground warning: the target line/zone is clear before impact.
4. Windup: short charge, shield flare, beam column, or weapon raise.
5. Execution: beam, shield break, shockwave, slash wave, or screen suppress.
6. Hit-confirm: frame stamp, impact flash, audio transient, and result hook.
7. Residual state: brief glow, field, smoke, shield fragments, or recall dissolve.

The sequence should be flashy, but it must remain tied to combat semantics. The
player should be able to say what the summon bought: tempo damage, screen block,
boss-screen suppress, recovery, or line safety.

## Slot2 Backline Marksman

Role: fast tempo, precision damage, marksman/no-recovery route.

Current profile:

- `DB_SummonSlot2_BacklineMarksman.asset`
- LV1/LV2/LV3 currently read as cover shots and volleys.
- Current fields still express projectile count, but the next presentation pass
  should make the same gameplay feel like a precision beam/line strike.

Target read:

- "A marksman opened the lane and cut the boss line now."
- Fast, clean, controlled, thinner than Slot3.
- Minimal screen occupation after impact.
- Hit result should emphasize boss damage and tempo, not protection.

Preferred attack language:

| Moment | Cue direction | Promotion candidates |
|---|---|---|
| Entry | side-lane materialize, small holo ring, short snap | SpecialSkills `Effect_12_HoloScan`, Hovl marker circle |
| Lock | narrow target line from summon to boss lane | Hovl marker/pointer, `_Game` line pressure cue style |
| Windup | blue-white charge at weapon/body core | Vefects electric discharge, PixPlays aura as light source |
| Execute | thin beam or instant line lance | PixPlays `ElementalBeams/WindBeam` or `WaterBeam`, SpecialSkills `PurifierBeam` |
| Impact | sharp electrical/arcane hit, no large dust wall | Vefects electric impact, PixPlays beam hit, promoted `DB_VFX_PlayerRangedProjectileImpact` as fallback |
| Audio | crisp cast plus bright impact | Action RPG magic cast/impact, RPG3 Electric light/heavy impact |

Slot2 should not use a huge shield, lingering ground quake, or screen-wide
explosion. If LV3 gets larger, it should widen/brighten the beam or add a quick
second line, not become a Vanguard suppress field.

Acceptance gates:

- Readout/result still says tempo/marksman, not generic high-tier summon.
- Support report keeps Slot2 separate from Slot3: no-recovery/tempo lane, not
  boss-screen suppress.
- Hit-confirm is present even if a short cut-in or camera cue is active.
- The player can identify the target line before impact.

## Slot3 Vanguard Commander

Role: prevention, line hold, boss-screen suppress, costly high-tier protection.

Current profile:

- `DB_SummonSlot3_VanguardCommander.asset`
- LV1/LV2/LV3 already carries screen intercept counts `2/4/7`.
- The current readout correctly says body block, hold line, and break wall.

Target read:

- "I paid the wait cost to hold the front and break the boss screen."
- Slower and heavier than Slot2.
- More screen ownership, but only during the committed suppress/hold moment.
- The payoff is prevention plus suppress, not racing Slot2 on boss damage.

Preferred attack language:

| Moment | Cue direction | Promotion candidates |
|---|---|---|
| Entry | heavy ground arrival, shield silhouette, lane claim | SpecialSkills `GuardianShield`, `HoloShield`, Hovl marker zone |
| Lock | broad warning zone or vertical shield wall | Hovl danger zone, PixPlays `EarthShield` ring |
| Windup | shield brightens, ground fractures, core charge | PixPlays `EarthAura`, `EarthSlamSpikesAoeVFX`, Vefects electric flow |
| Execute | shield-break pulse, frontal shockwave, or heavy beam wall | SpecialSkills `GroundScatter`, `OneHandSmash`, `SateliteBeam`; PixPlays `EarthBlast` |
| Impact | chunks, shield shards, white frame stamp, low-frequency hit | Vefects explosion/electric impact, DAVFX only for LV3/boss-scale impact |
| Audio | heavy shield metal, ground hard impact, low boom | Action RPG shield/ground/heavy impact, Vefects explosion/electric impact |

Slot3 can be visually huge, but it must preserve the existing route split:
hold-front should feel like fast suppress; retreat/recommit should feel like
safer recovery, not the same effect with lower damage.

Acceptance gates:

- Slot3 hold-front remains `support_vanguard_clear` with visible suppress.
- Slot3 retreat/recommit remains readable as safer recovery with less HP tax.
- The VFX makes the wait payoff visible without implying that Vanguard body
  stats were secretly buffed.
- Boss-screen suppress has a hit-confirm stamp and a short residual break state.

## LV3 / High-Tier Direct Suppress

Role: committed high-cost answer. The screen belongs to the summon briefly.

Target read:

- "The player survived the wait and cashed out a major answer."
- Entry and windup may be dramatic, but the hit-confirm must remain immediate.
- The effect may use orbital, beam column, or impact-wall language.

Preferred attack language:

| Moment | Cue direction | Promotion candidates |
|---|---|---|
| Pre-signal | full-bank/300 EN role forecast | existing route-incentive forecast, no new HUD system |
| Entry | holo scan, orbital marker, large ring | SpecialSkills `Effect_12_HoloScan`, `HoloOrbitalstrike`, `SpaceWarpPortal` |
| Lock | floor marker or vertical reticle | Hovl marker circle/danger zone, SpecialSkills `Effect_12_Strike` |
| Execute | orbital pillar, satellite beam, or wide shield-break beam | SpecialSkills `OrbitalAnnihilationBeam`, `SateliteCannon`, `SateliteBeam`; PixPlays Fire/Earth Beam |
| Impact | short shockwave, fragments, screen pulse | SpecialSkills `HoloExplosion`, `GroundScatter`, `MagmaStrike`; DAVFX explosion only after mobile review |
| Hit-confirm | non-interrupting frame overlay and impact SFX | existing ActionCinematicCueDirector hit stamp behavior |

Do not make LV3 a longer version of the same bolt stream. Use fewer, more
committed beats: lock, charge, beam/impact, frame stamp, residual field.

Acceptance gates:

- Direct LV3 suppress preserves micro hit/frame evidence while a cut-in is active.
- The presentation proves `Status/Hit -> Presentation`, not only "pretty VFX".
- The report still distinguishes high-tier suppress from Slot2 tempo and Slot3
  retreat/recovery.

## Owned Asset Promotion Queue

Promote candidates into `_Game` in this order. Each promotion should create a
small review prefab under `_Game/Art/VFX/...` and then reference that promoted
prefab from cue profiles.

1. Slot2 beam prototype
   - PixPlays `ElementalBeams/WindBeam` or `WaterBeam`.
   - Vefects electric impact once-shot.
   - Action RPG/RPG3 magic cast and electric impact SFX.

2. Slot3 shield/suppress prototype
   - PixPlays `ElementalShields/EarthShield` plus `EarthShieldHit`.
   - SpecialSkills `GuardianShield` or `HoloShield(IncludeHit)`.
   - SpecialSkills `GroundScatter` or PixPlays `EarthBlast` for suppress impact.

3. LV3 orbital/beam prototype
   - SpecialSkills `HoloScan`, `HoloOrbitalstrike`, `OrbitalAnnihilationBeam`,
     `SateliteBeam`, or `SateliteCannon`.
   - DAVFX explosions only for a short LV3 impact review, not regular summon hits.

4. Melee/slash fallback
   - Hovl `Slash wave` or SpecialSkills `IntangibleSlash`.
   - Use only if a summon role becomes slash/duelist themed; do not replace the
     marksman/vanguard split with generic sword spectacle.

5. Audio banks
   - Slot2: light magic cast, electric impact, bright beam transient.
   - Slot3: shield metal movement/impact, ground hard impact, low boom.
   - LV3: summon/cast, heavy impact, controlled explosion, optional dark/electric
     tail. Avoid long loops unless cleanup is explicit.

## Implementation Priority

### P0: Documentation and cue contract

- Keep this document and `FRONTLINE_COMBAT_STRUCTURE_ANCHOR.md` aligned.
- Add cue ids or named placeholders for:
  - `SummonSlot2BeamWindup`
  - `SummonSlot2BeamHit`
  - `SummonSlot3ShieldEntry`
  - `SummonSlot3SuppressImpact`
  - `SummonLv3HoloLock`
  - `SummonLv3OrbitalBeam`
  - `SummonHitConfirmStamp`

### P1: Promote minimal VFX review prefabs

- Promote one Slot2 beam, one Slot2 impact, one Slot3 shield, one Slot3 suppress
  impact, one LV3 lock marker, and one LV3 beam/impact.
- Keep old projectile prefabs in place until gameplay replacement is reviewed;
  the first pass can layer beam/impact cues over current authority.

### P2: Wire cue profile and report checks

- Add promoted cue refs to `DB_CombatVfxCues_ActionFoundation.asset` or a summon
  sub-profile if the cue table becomes crowded.
- Extend policy report/readout checks so Slot2, Slot3, and LV3 each prove:
  - role-specific cue fired,
  - hit-confirm fired,
  - result hook stayed distinct,
  - no raw `_Imported` runtime references.

### P3: Pattern split

- After cue layering proves the read, introduce or promote
  `SummonAttackPatternProfile` so beams/shields/shockwaves are not forced through
  projectile-count semantics.
- Keep `PlayerSummonSlot1Action` and support slot executors as spend/input owners;
  pattern execution should be a narrow presentation/gameplay bridge.

## Verification Checklist

The next implementation pass is acceptable only when all of these are true:

- Slot2 reads as a fast marksman beam/line strike, not a bigger Slot3.
- Slot3 reads as shield, prevention, line hold, and boss-screen suppress.
- LV3 reads as a committed high-cost answer with lock-on and hit-confirm.
- Existing bullets and enemy projectiles are not used as the core fantasy for
  summon attacks.
- Runtime references point to promoted `_Game` assets, not raw `_Imported` prefabs.
- Follow-up hit-confirm remains visible even under cut-in or strong camera cues.
- Policy report evidence still separates:
  - Slot2 tempo/no-recovery,
  - Slot3 hold-front suppress,
  - Slot3 retreat/recommit recovery,
  - direct LV3 suppress.
- The player can describe the payoff in one sentence after a run.

