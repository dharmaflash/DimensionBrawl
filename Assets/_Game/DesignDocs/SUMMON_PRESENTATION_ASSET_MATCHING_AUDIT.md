# Summon Presentation Asset Matching Audit

Date: 2026-06-29 KST

Status: static asset, runtime, and dataset audit. No prefab or code wiring has
been changed by this document.

Companion documents:

- `SUMMON_SYSTEM_ARKDATA_DECOMPOSITION.md`
- `SUMMON_PRESENTATION_VFX_DIRECTION.md`

## Decision Summary

The current owned project data supports this split:

| Role | Locked direction | Confidence |
|---|---|---|
| Slot2 BacklineMarksman | Keep the current BacklineShooter actor, but present it as a fast beam/line-strike summon instead of a normal bolt volley. | High for actor/anchor fit, medium until beam VFX is promoted and reviewed in motion. |
| Slot3 VanguardCommander | Keep the current FinalStandCommanderElite actor and make shield/intercept/suppress the core fantasy. This is the strongest current runtime match. | High for pressure-screen runtime fit, medium until stronger shield/suppress VFX is promoted. |
| Boss AuraCaptain pressure summon | Keep as an enemy-side pressure mirror and visual reference for hostile summon threat. Do not reuse as a player summon without team/color/tuning changes. | High as boss-side reference, low as immediate player asset. |

The major risk is not asset availability. The major risk is that the current
runtime still drives support summons through projectile-count volleys and a
coarse presenter that only triggers `EliteSummonPackage`, `Attack`, `Hit`, and
`Death`. To reach the requested "flashy, strong, effective summon" read, the
next implementation pass needs slot-specific animation/cue routing and promoted
beam/shield VFX.

## Verification Labels

| Label | Meaning |
|---|---|
| Confirmed runtime anchor | The current scene/code/prefab can already produce this beat or provides a direct transform/event for it. |
| Confirmed asset candidate | The asset exists locally and semantically matches the beat, but needs promotion/review before runtime use. |
| Needs cue or driver expansion | The asset/animation exists, but current runtime does not trigger it at the right beat yet. |
| Reject for this role | The asset or behavior is available, but would blur the role or contradict the source requirement. |

## Evidence Used

| Source | What was verified | Design implication |
|---|---|---|
| Local voice transcript in Downloads | Early requirement says summons currently feel like extra projectiles/obstacles, should feel like a mid-boss-grade actor, and ranged summons can use beam-like reads. | Do not ship a final pass that is only more small bolts. |
| `CombatPayload_ApplyData_2026-06-25/docs/combat_payload_family_guide.md` | Combat should decompose through action request, trigger/resource checks, target selection, payload/hit, effect/status, and presentation feedback. | Every summon VFX must prove a gameplay beat, not just decorate it. |
| `CombatPayload_ApplyData_2026-06-25/normalized/combat_event_effect_contract.json` | Presentation cues require family/timing/effect refs/cleanup/source refs. | Raw VFX selection must be promoted into a cue contract before final runtime use. |
| `CombatPayload_ApplyData_2026-06-25/focused_pgr_magicid_behavior` | High-confidence rows support state gates, camera/presentation control, hit response, and hard-lock families as review categories. | Big summon beats should have explicit state/cue/hit-confirm gates. |
| `PGR_Tutorial_Stage_Data_2026-06-19` | Contains BlackRockChess effect, tutorial, guide, stage, and teaching data useful for cue grammar. | Good for naming/role grammar; not a direct asset source. |
| `pgr_base_assets` | Only exposed `globalgamemanagers`, `resources.assets`, and `.resS` binary Unity asset files in this audit. | Not suitable for direct exact prefab matching without extraction; use as runtime taxonomy hint only. |
| Local `DimensionBrawl` prefabs, controllers, profiles, cues | Slot2/Slot3/boss pressure actor wiring, animator params, VFX cue ids, projectile prefabs, and audio candidates were checked. | Local project data is the source of truth for exact matching. |

## Current Runtime Anchors

| Runtime surface | Confirmed behavior | Matching impact |
|---|---|---|
| `PlayerSupportSummonSlotAction` | Owns support summon cost, cooldown, projectile prefab, entry cue, actor prefab, first volley delay, volley interval, and max volley count. | Slot2/Slot3 are currently still projectile-authority summons. |
| `SupportSummonSlotExecutor` | Spawns entry cue, actor, optional pressure screen, and persistent volleys from `ProjectileOrigin`. | Beam can be layered on volley events first; a real beam pattern needs later attack-pattern work. |
| `SupportSummonSlotExecutor.ConfigureActorCombat` | If `ActorRoleId == "BacklineMarksman"`, hostile body damage is set to zero. | Slot2 should not be sold as melee/clash power. Its power must be beam/line/hit-confirm. |
| `SummonFrontlineProxyPresenter` | Current animator triggers: `EliteSummonPackage`, `Attack`, `Hit`, `Death`. Current cues: entry `EliteSummonSignal`, attack `EnemyAttackActive`, clash `EliteShieldSignal`, damage `EnemyHit`, death `EnemyDeath`. | Good base actor presenter, but too coarse for final summon spectacle. |
| `SummonPressureScreen` | Intercepts `BossBarrageProjectile` and `LaneActionProjectile`; exposes activation/intercept/deactivation events. | Slot3 shield/block read is already strongly grounded. |
| `SummonPressureScreenPresenter` | Current cues: activation `EliteShieldSignal`, intercept `SummonBlockOpportunity`; has radius/scale/color/punch visuals. | Slot3 can be upgraded with low systemic risk. |
| `LaneActionProjectile` | Owns moving projectile collision/damage; projectile prefabs have no direct audio clip assigned in their `AudioSource`. | Current bolts are authority carriers, not enough final presentation by themselves. |

## Current Cue Map

Relevant entries in `DB_CombatVfxCues_ActionFoundation.asset`:

| Cue id | Cue name | Current prefab | Use in summon audit |
|---:|---|---|---|
| 4 | `EnemyAttackActive` | `DB_VFX_EnemyAttackActive_Generic.prefab` | Current support summon attack cue. Temporary only for Slot2/Slot3 final reads. |
| 5 | `EnemyHit` | `DB_VFX_EnemyHit.prefab` | Current generic damage fallback. |
| 6 | `EnemyDeath` | `DB_VFX_EnemyDeath.prefab` | Current actor death/failed fallback. |
| 24 | `EliteShieldSignal` | `DB_VFX_EliteShieldSignal.prefab` | Confirmed shield/clash activation cue. Good Slot3 base. |
| 26 | `EliteAuraSignal` | `DB_VFX_EliteAuraSignal.prefab` | Candidate focus/empower cue. |
| 27 | `EliteSummonSignal` | `DB_VFX_EliteSummonSignal.prefab` | Confirmed summon entry cue. |
| 28 | `ElitePhaseSwapSignal` | `DB_VFX_ElitePhaseSwapSignal.prefab` | Candidate high-tier charge/phase cue. |
| 29 | `SummonFollowupWindow` | `DB_VFX_SummonFollowupWindow.prefab` | Candidate target/assist window cue. |
| 30 | `SummonFollowupHit` | `DB_VFX_PlayerRangedProjectileImpact.prefab` | Good hit-confirm fallback. |
| 32 | `SummonBlockOpportunity` | `DB_VFX_SummonBlockOpportunity.prefab` | Confirmed intercept/opportunity cue. Excellent Slot3 beat. |
| 34 | `PlayerRangedProjectileImpact` | `DB_VFX_PlayerRangedProjectileImpact.prefab` | Slot2 hit-confirm fallback until beam impact is promoted. |

The cue profile has enough generic summon/shield/follow-up vocabulary for a
first review, but not enough named cues for final Slot2 beam or Slot3 suppress
identity. New cue ids or a summon sub-profile should be added before final
wiring.

## Current Exact Actor Matches

### Slot2: BacklineMarksman

| Surface | Exact local asset | Audit result |
|---|---|---|
| Gameplay profile | `_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_BacklineMarksman.asset` | Confirmed current tier data: ranged support, 2/3/4 projectile counts, no pressure screen. |
| Presentation profile | `_Game/DesignData/Profiles/ActionFoundation/DB_SummonPresentation_PlayerBacklineMarksman.asset` | Confirmed candidate profile for player backline marksman. |
| Actor prefab | `_Game/Prefabs/Combat/PF_SummonSlot2Actor_MarksmanProxy.prefab` | Confirmed runtime actor. Keep. |
| Visual source | `_Game/Art/Characters/Enemies/SciFiSoldiers/RoleVariants/BacklineShooter/Models/SK_BacklineShooter_Spikarian.fbx` | Good enough for current slice: reads as sci-fi ranged support, not tank. |
| Visual child | `SummonSlot2Visual_BacklineShooter` | Confirmed in actor prefab. |
| Animator controller | `_Game/Art/Animations/Enemies/SciFiSoldiers/RoleVariants/BacklineShooter/DB_BacklineShooter_Role.controller` | Confirmed. Has better triggers than current presenter uses. |
| Runtime anchors | `ProjectileOrigin`, `RefPosLightningGun_Action`, `RefPosLaserGatlinGun_Action`, `SummonStateVfx_MarksmanFocusAura`, `SummonPulseVfx_MagicMissilesPulse` | Strong candidate anchors for muzzle/beam/focus cues. |
| Current projectile | `_Game/Prefabs/Combat/PF_SummonSlot2Projectile_MarksmanBolt.prefab` | Keep as gameplay authority fallback; reject as final fantasy. |

Important Slot2 constraint: body/clash damage is disabled for the
`BacklineMarksman` role in support summon runtime. Therefore, Slot2 should be
sold through lock-on, beam/line strike, and hit-confirm, not through melee body
impact.

### Slot3: VanguardCommander

| Surface | Exact local asset | Audit result |
|---|---|---|
| Gameplay profile | `_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_VanguardCommander.asset` | Confirmed current tier data: vanguard, large actor, 2/4/7 screen intercepts. |
| Presentation profile | `_Game/DesignData/Profiles/ActionFoundation/DB_SummonPresentation_PlayerVanguardCommander.asset` | Confirmed candidate profile for player vanguard. |
| Actor prefab | `_Game/Prefabs/Combat/PF_SummonSlot3Actor_VanguardProxy.prefab` | Confirmed runtime actor. Best current summon fit. |
| Visual source | `_Game/Art/Characters/Enemies/SciFiSoldiers/RoleVariants/FinalStandCommanderElite/Models/SK_FinalStandCommanderElite_SciFiHeavyBattleArmor.fbx` | Strong match for heavy commander/tank silhouette. |
| Visual child | `SummonSlot3Visual_FinalStandCommanderElite` | Confirmed in actor prefab. |
| Animator controller | `_Game/Art/Animations/Enemies/SciFiSoldiers/RoleVariants/FinalStandCommanderElite/DB_FinalStandCommanderElite_Role.controller` | Confirmed. Has shield/heavy triggers available. |
| Runtime anchors | `PressureScreen`, `PressureScreenVisual`, `SummonShieldVfx_MagicMissilesShieldCircle`, `SummonStateVfx_VanguardGuardAura`, `RefPos_GatllinGun_Action`, `ProjectileOrigin` | Strong direct match for shield/intercept/suppress. |
| Current projectile | `_Game/Prefabs/Combat/PF_SummonSlot3Projectile_VanguardBolt.prefab` | Keep as authority fallback; should become secondary to shield/suppress read. |

Slot3 already has the gameplay route the transcript asks for: a large actor,
slow/heavy presence, visible intercept budget, and a payoff that changes enemy
pressure. It needs stronger presentation, not a role rewrite.

### Boss Pressure Mirror: AuraCaptain

| Surface | Exact local asset | Audit result |
|---|---|---|
| Presentation profile | `_Game/DesignData/Profiles/ActionFoundation/DB_SummonPresentation_BossAuraCaptain.asset` | Confirmed boss-side pressure candidate. |
| Actor prefab | `_Game/Prefabs/Combat/PF_BossSummonPressureActor_Proxy.prefab` | Confirmed enemy pressure actor. |
| Visual source | `_Game/Art/Characters/Enemies/SciFiSoldiers/RoleVariants/AuraCaptainElite/Models/SK_AuraCaptainElite_SciFiFemaleCombatSuit.fbx` | Good command/pressure read. |
| Runtime anchors | `RoleWeapon_BeamGun`, `RefPosLightningGun_Action`, `PressureOrigin`, `PressureScreen`, `PressureScreenVisual`, `SummonStateVfx_BossPressureAura`, `SummonShieldVfx_MagicMissilesShieldCircle` | Excellent boss-side beam/screen reference. |

Use this as the hostile mirror for "enemy summon approaches and feels
dangerous." Do not treat it as the player Slot2/Slot3 answer until team, color,
and gameplay costs are reauthored.

## Animator Match

The BacklineShooter, FinalStandCommanderElite, and AuraCaptainElite controllers
share the important trigger vocabulary:

- `Attack`
- `AttackHeavy`
- `AttackLinePressure`
- `AttackFanPressure`
- `AttackGuardBreak`
- `EliteShieldCycle`
- `EliteArmorBreak`
- `EliteAuraBuffer`
- `EliteSummonPackage`
- `ElitePhaseSwap`
- `Hit`
- `HitHeavy`
- `Death`

Current presenter usage:

| Beat | Current trigger | Final target |
|---|---|---|
| Spawn | `EliteSummonPackage` | Keep. |
| Generic attack | `Attack` | Keep only as fallback. |
| Slot2 beam windup | Not currently driven | Drive `AttackLinePressure` or `EliteAuraBuffer`. |
| Slot2 LV3 charge | Not currently driven | Drive `ElitePhaseSwap` plus beam cue. |
| Slot3 shield raise | Not currently driven | Drive `EliteShieldCycle`. |
| Slot3 heavy suppress/break wall | Not currently driven | Drive `AttackHeavy`, `AttackGuardBreak`, or `EliteArmorBreak` depending on visual review. |

This means the animation library is not the blocker. The blocker is the missing
slot-specific presentation driver/cue map.

## Owned VFX Candidates

Raw `_Imported` candidates must be promoted under `_Game/Art/VFX/...` before
runtime profile references. The paths below are evidence that the owned assets
exist locally, not approval to wire raw imports directly.

### Slot2 Beam / Precision Strike

| Rank | Candidate | Exact local source | Reason |
|---:|---|---|---|
| 1 | Clean beam | `_Imported/AssetStore/VFX/PixPlays/ElementalBeams/WindBeam/Version_BuiltIn/WindBeam.prefab` | Best first pass for a thin, fast, readable marksman line. |
| 2 | Clean blue beam | `_Imported/AssetStore/VFX/PixPlays/ElementalBeams/WaterBeam/Version_BuiltIn/WaterBeam.prefab` | Good alternate if WindBeam reads too airy. |
| 3 | Strong holy/energy beam | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_28_PurifierBeam/Effect_28_PurifierBeam.prefab` | Better for LV2/LV3 beam authority; review for scale/noise. |
| 4 | Sci-fi core beam | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_38_CoreBeam/Effect_38_CoreBeam.prefab` | Fits high-energy line strike; script-based package requires cleanup review. |
| 5 | Electric hit | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_38_CoreBeam/Effect_38_ElectricExplosion.prefab` | Good beam hit-confirm candidate. |
| LV3 only | Orbital annihilation | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_03_OrbitalStrike/Effect_03_OrbitalAnnihilationBeam.prefab` | Too large for normal Slot2 shots; good full-bank/high-tier review. |
| LV3 only | Satellite beam | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_54_SateliteCannon/Effect_54_SateliteBeam.prefab` | High-tier beam column candidate. |
| LV3 only | Annihilation beam | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_39_AnnihilationBeam/Effect_39_AnnihilationBeam.prefab` | Potentially too dominant; review as cinematic suppress only. |
| Lock marker | Holo scan | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_12_HoloScan/Effect_12_HoloScan.prefab` | Good target-lock/readability helper before beam. |

Reject for normal Slot2:

- Generic `EnemyAttackActive` as final attack read.
- Reusing only `PF_SummonSlot2Projectile_MarksmanBolt`.
- Large explosions for every marksman hit.
- Slot3 shield VFX as Slot2 identity.

### Slot3 Shield / Suppress / Break Wall

| Rank | Candidate | Exact local source | Reason |
|---:|---|---|---|
| 1 | Earth shield | `_Imported/AssetStore/VFX/PixPlays/ElementalShields/EarthShield/Version_BuiltIn/EarthShield.prefab` | Best first pass for heavy physical wall. |
| 2 | Earth shield hit | `_Imported/AssetStore/VFX/PixPlays/ElementalShields/EarthShield/Version_BuiltIn/EarthShieldHit.prefab` | Direct match for projectile intercept feedback. |
| 3 | Guardian shield | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_09_GuardianShield/Effect_09_GuardianShield.prefab` | Good heroic guard read; review color/scale. |
| 4 | Earth blast | `_Imported/AssetStore/VFX/PixPlays/ElementalBlastVFX/EarthBlast/Version_BuiltIn/EarthBlast.prefab` | Good suppress/counter impact candidate. |
| 5 | Earth slam spikes | `_Imported/AssetStore/VFX/PixPlays/ElementalAOE/EarthAOE/Version_BuiltIn/EarthSlamSpikesAoeVFX.prefab` | Strong LV3 break-wall ground read; review screen clutter. |
| 6 | Ground scatter | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_21_GroundScatter/Effect_21_GroundScatter.prefab` | Strong suppress/break visual. |
| LV3 only | Magma strike | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_30_MagmaStrike/Effect_30_MagmaStrike.prefab` | Use only if fire/earth identity is desired for high-tier impact. |
| LV3 only | Bombing review explosion | `_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Prefabs/PF_BombingReview_DavfxExplosion09.prefab` or `PF_BombingReview_DavfxExplosion20.prefab` | Already promoted, but too explosive for normal summon hits. Use only as a one-off LV3 impact candidate. |

Reject for normal Slot3:

- Slot2 thin beam as the main fantasy.
- More bolt counts as the main power read.
- Continuous screen-wide explosion loops.
- Any shield loop without explicit cleanup.

### LV3 / Full-Bank Summon Moment

| Candidate | Exact local source | Use |
|---|---|---|
| Holo lock | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_12_HoloScan/Effect_12_HoloScan.prefab` | Pre-signal / target confirmation. |
| Holo orbital strike | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_12_HoloScan/Effect_12_HoloOrbitalstrike.prefab` | High-tier lock-to-strike bridge. |
| Holo explosion | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_12_HoloScan/Effect_12_HoloExplosion.prefab` | Hit-confirm or residual burst. |
| Orbital annihilation beam | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_03_OrbitalStrike/Effect_03_OrbitalAnnihilationBeam.prefab` | LV3 beam column. |
| Satellite cannon | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_54_SateliteCannon/Effect_54_SateliteCannon.prefab` | LV3 full event candidate. |
| Satellite beam | `_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_54_SateliteCannon/Effect_54_SateliteBeam.prefab` | LV3 execution candidate. |

LV3 should be fewer, larger, clearer beats: lock, charge, execute,
hit-confirm, residual. It should not be a longer stream of ordinary bolts.

## Audio Candidates

Current promoted audio that is already aligned with combat cues:

| Use | Exact local assets | Result |
|---|---|---|
| Summon entry | `_Game/Art/Audio/SFX/CombatCues/DB_SFX_EliteSummonSignal_01.wav` through `_03.wav` | Confirmed base entry audio. |
| Block/opportunity | `_Game/Art/Audio/SFX/CombatCues/DB_SFX_SummonBlockOpportunity_01.wav` through `_03.wav` | Confirmed Slot3 intercept/opportunity base audio. |
| Ranged impact | `_Game/Art/Audio/SFX/CombatCues/DB_SFX_PlayerRangedProjectileImpact_01.wav` through `_03.wav` | Slot2 hit-confirm fallback. |
| Heavy movement | `_Game/Art/Audio/SFX/Footsteps/DB_SFX_Footstep_HeavyGround_01.wav` through `_03.wav` | Good Slot3 weight layer. |

Imported audio candidates to review/promote:

| Use | Exact local source | Reason |
|---|---|---|
| Shield cast | `_Imported/AssetStore/Vefects/Stylized Sound Effects Bundle/Vignette VFX/SFX_Vefects_Vignettes_Shield_Cast.wav` | Slot3 screen activation. |
| Shield loop | `_Imported/AssetStore/Vefects/Stylized Sound Effects Bundle/Vignette VFX/SFX_Vefects_Vignettes_Shield_Loop.wav` | Only if cleanup is explicit. |
| Shield end | `_Imported/AssetStore/Vefects/Stylized Sound Effects Bundle/Vignette VFX/SFX_Vefects_Vignettes_Shield_End.wav` | Slot3 deactivation. |
| Electric trail | `_Imported/AssetStore/Vefects/Stylized Sound Effects Bundle/Trails VFX/SFX_Vefects_Trail_Electric.wav` | Slot2 beam/line transient candidate. |
| Electric slash/transient | `_Imported/AssetStore/Vefects/Stylized Sound Effects Bundle/Stylized VFX/Slashes Piercing/SFX_Vefects_Stylized_Slash_Electric.wav` | Slot2 beam hit or lock-snap candidate. |

## Exact Beat Matching

### Slot2 BacklineMarksman: Beam Marksman

| Beat | Exact match | Status |
|---|---|---|
| Entry body | `PF_SummonSlot2Actor_MarksmanProxy` with `EliteSummonPackage` and `EliteSummonSignal` | Confirmed runtime anchor. |
| Entry enhancement | `Effect_12_HoloScan` or existing `SummonFollowupWindow` style marker | Confirmed asset candidate; needs promoted cue. |
| Focus/windup | `SummonStateVfx_MarksmanFocusAura`; animation trigger `EliteAuraBuffer` or `AttackLinePressure` | Asset/animation exists; needs driver expansion. |
| Attack authority | `PF_SummonSlot2Projectile_MarksmanBolt` fired from `ProjectileOrigin` | Confirmed runtime authority; fallback only. |
| Beam overlay | PixPlays `WindBeam` first, `WaterBeam` second; `PurifierBeam` for stronger review | Confirmed asset candidates; needs promoted beam prefab and cue. |
| LV3 beam | `OrbitalAnnihilationBeam`, `SateliteBeam`, or `AnnihilationBeam` | Confirmed asset candidates; LV3 only, needs visual/perf review. |
| Hit-confirm | `SummonProjectileDamageApplied` event plus `PlayerRangedProjectileImpact` / `SummonFollowupHit` fallback | Confirmed runtime/event path. |
| Strong hit VFX | `Effect_38_ElectricExplosion` | Confirmed asset candidate; needs promoted cue. |
| Audio | `DB_SFX_EliteSummonSignal_*`, `DB_SFX_PlayerRangedProjectileImpact_*`, Vefects electric transient | Base confirmed; beam-specific audio needs promotion. |

Final Slot2 read: a clean, fast, precision line. The summon should snap into
place, lock a line, charge briefly, fire a beam, and leave a sharp hit stamp.
It should not occupy the screen after the hit the way Slot3 does.

### Slot3 VanguardCommander: Shield Commander

| Beat | Exact match | Status |
|---|---|---|
| Entry body | `PF_SummonSlot3Actor_VanguardProxy` with `EliteSummonPackage` and `EliteSummonSignal` | Confirmed runtime anchor. |
| Guard surface | `PressureScreen`, `PressureScreenVisual`, `SummonShieldVfx_MagicMissilesShieldCircle` | Confirmed runtime anchor. |
| Shield activation | `SummonPressureScreenPresenter` playing `EliteShieldSignal` | Confirmed runtime anchor. |
| Shield animation | `EliteShieldCycle` on FinalStandCommanderElite controller | Animation exists; needs driver expansion. |
| Intercept | `SummonPressureScreen` intercept event plus `SummonBlockOpportunity` cue | Confirmed runtime anchor. |
| Shield VFX upgrade | PixPlays `EarthShield` and `EarthShieldHit`; SpecialSkills `GuardianShield` | Confirmed asset candidates; promote and review. |
| Counter authority | `PF_SummonSlot3Projectile_VanguardBolt` fired from `ProjectileOrigin` | Confirmed runtime authority; secondary read only. |
| Suppress impact | `EarthBlast`, `EarthSlamSpikesAoeVFX`, or `GroundScatter` | Confirmed asset candidates; needs promoted cue. |
| LV3 break wall | `EliteArmorBreak` or `AttackHeavy` plus `GroundScatter` / one-shot promoted bombing explosion | Needs driver/cue expansion and mobile noise review. |
| Audio | `DB_SFX_SummonBlockOpportunity_*`, `DB_SFX_EliteSummonSignal_*`, `DB_SFX_Footstep_HeavyGround_*`, Vefects shield cast/end | Base confirmed; shield loop only with cleanup. |

Final Slot3 read: a large body claims the front, raises a readable shield,
blocks or suppresses pressure, and produces a heavier residual field than Slot2.
Its payoff is prevention and boss-screen control, not faster marksman damage.

### Boss AuraCaptain: Hostile Pressure Reference

| Beat | Exact match | Status |
|---|---|---|
| Enemy pressure actor | `PF_BossSummonPressureActor_Proxy` | Confirmed runtime actor. |
| Beam/command props | `RoleWeapon_BeamGun`, `RefPosLightningGun_Action`, `PressureOrigin` | Confirmed anchors. |
| Pressure screen | `PressureScreen`, `PressureScreenVisual`, `SummonStateVfx_BossPressureAura` | Confirmed anchors. |
| Recommended use | Enemy-side summon pressure mirror | Use as reference, not player slot replacement. |

This asset should help validate the transcript requirement that enemy summons
feel dangerous when they approach. It should not become the player answer unless
its team, palette, and tuning are intentionally changed.

## Future Dragon / Large Creature Audit

This audit also checked the long-term dragon question. Result: there are real
dragon actor assets and breath/spit animations locally, but the dragon pack does
not appear to contain dedicated beam/laser/ray VFX files. A dragon beam summon
would need dragon animation plus a promoted external beam VFX attached to a new
mouth/breath origin.

Current local design marker:

| Surface | Exact local asset | Audit result |
|---|---|---|
| Future archetype | `_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_DragonBoss_Future.asset` | Confirms `DragonBoss.Future` is intentionally separate from the current soldier/slot summon slice. |
| Source candidate text | `HEROIC FANTASY CREATURES FULL PACK VOL3 raw dragon prefabs remain local-only` | Confirms the dragon pack is tracked, but not promoted. |
| Reuse flag | `candidateForFutureSummonAiReuse: 0` | Current design data says not to force this into the summon AI reuse path yet. |

Available dragon prefabs:

| Dragon | PBR prefab | Notes |
|---|---|---|
| Desert | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Desert Dragon/Prefabs/DesertDragon_PBR.prefab` | Fireball/spread-fire animations exist. |
| Forest | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Forest Dragon/Prefabs/ForestDragon_PBR.prefab` | Fireball/spread-fire animations exist. |
| Ocean | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Ocean Dragon/Prefabs/OceanDragon_PBR.prefab` | Acid spit/spread-acid-breath animations exist. |
| Plains | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Plains Dragon/Prefabs/PlainsDragon_PBR.prefab` | Fireball/spread-fire animations exist. |
| Polar | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Polar Dragon/Prefabs/PolarDragon_PBR.prefab` | Frozen-ball/spread-frozen-breath animations exist. |
| Undead | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Undead Dragon/Prefabs/HellDragon_PBR.prefab` | Acid spit/spread-acid-breath animations exist. |
| Volcano | `_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/Prefabs/VolcanoDragon_PBR.prefab` | Fireball/spread-fire/roar-spread-fire animations exist. |

Representative breath/spit animation files:

| Family | Exact local animation examples | Beam verdict |
|---|---|---|
| Fire | `VolcanoDragon@SpitFireBall.FBX`, `VolcanoDragon@FlyStationarySpreadFire.FBX`, `VolcanoDragon@RoarSpreadFire.FBX`, `DesertDragon@SpreadFire.FBX`, `ForestDragon@SpitFireBall.FBX`, `PlainsDragon@SpitFireBall.FBX` | Breath/fireball animation, not beam by itself. |
| Acid | `OceanDragon@SpreadAcidBreath.FBX`, `OceanDragon@SpitAcid.FBX`, `HellDragon@SpreadAcidBreath.FBX`, `HellDragon@SpitAcid.FBX` | Breath/spit animation, not beam by itself. |
| Ice | `PolarDragon@SpreadFrozenBreath.FBX`, `PolarDragon@SpitFrozenBall.FBX` | Breath/projectile animation, not beam by itself. |

Additional local check:

- Sample PBR prefabs include head/tongue/neck bones such as `OceanDragon_ Head`,
  `PolarDragon_ Head`, and `VolcanoDragon_Tongue_*`.
- No dedicated `Beam`, `Laser`, or `Ray` files were found inside the Elemental
  Dragons pack.
- No explicit mouth-origin child was confirmed by name. Promotion should add a
  reviewed `BreathOrigin` child under the promoted dragon prefab.

Future dragon beam direction:

| Desired read | Use dragon asset for | Use separate promoted VFX for | Status |
|---|---|---|---|
| Fire dragon breath | `VolcanoDragon_PBR` or `DesertDragon_PBR` plus spread-fire/spit-fire animation | PixPlays `FireBeam`, SpecialSkills `OrbitalAnnihilationBeam`, or tuned fire cone/impact | Candidate only; needs promotion and new attack-pattern driver. |
| Ice dragon breath | `PolarDragon_PBR` plus frozen-breath animation | PixPlays `WaterBeam`, ice/frost impact audio, or tuned beam cone | Candidate only; needs promotion and visual review. |
| Acid/poison dragon breath | `OceanDragon_PBR` or `HellDragon_PBR` plus acid-breath animation | Water/acid-colored beam or custom shader/VFX, plus cleanup policy | Candidate only; not appropriate for current Slot2/Slot3. |

Do not mix the dragon pack into current Slot2/Slot3 final matching. It is a
separate boss/summon slice because it needs a promoted creature prefab,
mouth-origin anchors, scale/camera framing, attack-pattern data, and a different
review scene. It can absolutely support a dramatic future beam/breath summon,
but that support is a composite plan, not an already-wired runtime match.

## Reject / Defer List

| Candidate | Decision | Reason |
|---|---|---|
| More current Slot2 bolts as final presentation | Reject | Directly conflicts with "not just another projectile" requirement. |
| Slot2 melee/clash impact as primary power | Reject | BacklineMarksman body damage is disabled by runtime. |
| Generic `EnemyAttackActive` as final Slot2/Slot3 attack cue | Reject as final | Useful fallback, but does not communicate beam, shield, or suppress. |
| Raw `_Imported` prefab references in gameplay profiles | Reject | Must promote to `_Game` and add cleanup/source refs. |
| Raw ArkData assets | Reject | Dataset is reference/contract material, not production asset source. |
| DAVFX/bombing explosions on every shot | Reject | Too noisy and role-blurring; reserve for LV3 one-shot review. |
| Boss AuraCaptain as player Slot2/Slot3 | Defer | Strong enemy-side fit, but wrong team/palette/ownership by default. |
| Continuous shield/audio loops without cleanup | Reject | Violates cue cleanup requirement and risks leaks/noise. |
| Dragon/giant monster normal summon | Defer | Dragon prefabs and breath/spit animations exist, but no promoted runtime actor, no mouth-origin anchor, and no dedicated dragon beam cue are wired yet. |

## Technical Readiness Follow-Up

This pass checked whether the visual candidates can survive the current runtime
shape, not only whether they look semantically correct.

| Runtime constraint | Verified detail | Matching consequence |
|---|---|---|
| Render pipeline | Project uses URP 17.3 packages, `UNITY_PIPELINE_URP`, and a custom URP render pipeline asset. | Any Built-In or package-specific shader must be reviewed in a URP scene before promotion. |
| Cue player contract | `CombatVfxCuePlayer` pools prefabs, plays `CombatVfxCueVisual`, `ParticleSystem`, `VisualEffect`, and audio, then stops/clears and deactivates them after cue lifetime. | Raw imported scripts that call `Destroy(gameObject)` or spawn unmanaged children can break pooling. |
| Cue data shape | `CombatVfxCueProfile` stores prefab, offsets, scale, lifetime, prewarm, parent, and align-to-direction only. | A source-to-target beam cannot be fully driven by the existing cue profile alone. |
| Summon presenter shape | `SummonFrontlineProxyPresenter` and `SummonPressureScreenPresenter` pass anchor, planar direction, tier, and intensity. | Slot-specific beam length, hit point, muzzle, and target transforms need a narrow summon beam driver or adapter. |

Readiness of the main candidates:

| Candidate | Verified technical shape | Readiness decision |
|---|---|---|
| Existing `_Game/Art/VFX/CombatCues` prefabs | Use project-owned `CombatVfxCueVisual`/`CombatVfxCueAudioRandomizer`, particles, lights, and short lifetimes. | Safe baseline for review scenes. |
| PixPlays `WindBeam`, `WaterBeam`, `FireBeam` | 12-14 particle systems, `BeamVfx`, `ParticleSystemStartStopLifetime`, ShaderGraph materials. `BeamVfx.Play(VfxData)` expects source, target, and duration; `BaseVfx` schedules destroy. | Good visual match, but not direct cue-profile safe. Promote behind a beam adapter or build a native project beam cue. |
| PixPlays `EarthShieldHit` | Scriptless particle hit prefab with ShaderGraph materials plus one Built-In material. | Best first Slot3 shield-hit import candidate after URP material review. |
| PixPlays `EarthShield` | `EarthShield.cs`, 17 mesh renderers, Built-In materials, raycast/collider/rigidbody behavior, and spawned shard effects. | Do not wire raw. Use as visual reference, or promote a stripped/non-physics shield visual. |
| PixPlays `EarthBlast` / `EarthSlamSpikesAoeVFX` | `PlayableDirector`, `Animator`, `LocationVfx`, `PlayableVfx`, many mesh renderers. | LV3/suppress candidate only after timeline cleanup, performance, and screen-clutter review. |
| SpecialSkills `Effect_28_PurifierBeam` | 5 particle systems, custom `Shader_IntegratedEffect`, and `ScaleFactorApplyToMaterial` referencing sample-scene global state. | Good stronger beam look, but script must be removed/adapted before promotion. |
| SpecialSkills `OrbitalAnnihilationBeam` | 12 particle systems plus `DelayActive`/`NewMaterialChange`; `NewMaterialChange` destroys objects after mask fade. | Strong LV3 candidate, not direct pooled cue-safe. |
| SpecialSkills `SateliteBeam`, `GuardianShield`, `AnnihilationBeam`, `ElectricExplosion` | Scriptless outer prefabs in the inspected files, particle/mesh-heavy, custom SpecialSkills shaders. | Plausible promotion candidates after URP visual review and lifetime cleanup. |
| SpecialSkills `GroundScatter`, `HoloScan`, `HoloOrbitalstrike` | Demo behavior scripts such as delayed activation, spawned objects, movement, or many MonoBehaviours. | Defer until behavior is isolated; do not use as normal attack feedback. |
| Dragon breath/beam | Dragon prefabs and breath/spit animations exist, but no dedicated beam prefab or mouth-origin child is wired. | Composite future boss/summon feature only: dragon animation + authored `BreathOrigin` + separate beam/cone driver. |

The most important correction from this follow-up is that the folder labels are
not enough evidence. Some "NotScriptBased" candidates still carry scripts, and
some "ScriptBased" composites hide their actual particles in nested prefabs.
Promotion must inspect the prefab component graph, script behavior, shader
compatibility, cleanup policy, and runtime event source.

## Implementation Plan

### P0: Promote a Minimal Review Set

Create promoted review prefabs under `_Game/Art/VFX/Summons/...`:

1. Slot2 beam proof: `WindBeam` first, `WaterBeam` as backup, but behind a
   project-owned beam driver/adapter rather than direct cue-profile pooling.
2. Slot2 strong hit: `Effect_38_ElectricExplosion` or a tuned
   `PlayerRangedProjectileImpact` variant.
3. Slot3 first shield-hit proof: `EarthShieldHit`, plus existing pressure screen
   presenter cues.
4. Slot3 shield wall: stripped visual derived from `EarthShield`, or a native
   project shield mesh using the same art direction. Do not use raw
   `EarthShield.cs`.
5. Slot3 suppress: `EarthBlast` only after timeline/performance review;
   `GroundScatter` is deferred until its demo scripts are isolated.
6. LV3 lock/beam: `SateliteBeam`, `AnnihilationBeam`, or
   `OrbitalAnnihilationBeam` after custom shader and destroy-script cleanup.
   `HoloScan` is a later candidate, not the first lock-on proof.

Promote audio under `_Game/Art/Audio/SFX/Summons/...`:

1. Shield cast/end from Vefects.
2. Electric transient from Vefects.
3. Optional heavy-footstep layer using already promoted heavy ground clips.

### P1: Add Named Cue Routes

Add cue ids or a summon-specific cue profile for:

- `SummonSlot2BeamLock`
- `SummonSlot2BeamWindup`
- `SummonSlot2BeamFire`
- `SummonSlot2BeamHit`
- `SummonSlot3ShieldRaise`
- `SummonSlot3ShieldHit`
- `SummonSlot3SuppressImpact`
- `SummonLv3HoloLock`
- `SummonLv3OrbitalBeam`
- `SummonLv3ImpactStamp`

Every cue must include cleanup/lifetime policy. Raw imports should not remain in
scene or profile references.

### P2: Add Slot-Specific Presentation Driving

Extend `SummonFrontlineProxyPresenter` or add a narrow support-summon
presentation driver that can route by actor role/tier:

| Role/tier | Animator trigger | VFX cue |
|---|---|---|
| Slot2 normal attack | `AttackLinePressure` | `SummonSlot2BeamFire` |
| Slot2 focus | `EliteAuraBuffer` | `SummonSlot2BeamWindup` |
| Slot2 LV3 | `ElitePhaseSwap` | `SummonLv3OrbitalBeam` or bright beam variant |
| Slot3 shield activation | `EliteShieldCycle` | `SummonSlot3ShieldRaise` |
| Slot3 intercept | Optional `HitHeavy` or shield punch only | `SummonSlot3ShieldHit` |
| Slot3 suppress | `AttackHeavy` or `AttackGuardBreak` | `SummonSlot3SuppressImpact` |
| Slot3 LV3 break wall | `EliteArmorBreak` | `SummonLv3ImpactStamp` |

Near-term damage can remain projectile-authority while the promoted beam/shield
VFX overlays fire from the existing events and anchors. Long-term, replace
projectile-count semantics with a proper summon attack-pattern profile.

### P3: Keep Gameplay Semantics Visible

Do not let spectacle hide result feedback. Each summon use should expose:

- summon cost/spend succeeded,
- entry cue fired,
- actor spawned,
- role-specific attack cue fired,
- hit/intercept happened,
- hit-confirm or block-confirm fired,
- actor/cue cleanup completed.

## Verification Plan

Static checks:

- No runtime references to raw `_Imported` assets.
- New cue ids resolve to promoted `_Game` prefabs.
- `BacklineMarksman` does not rely on body damage.
- Slot3 keeps pressure-screen intercept counts and radius/lifetime semantics.
- Animator triggers configured in profiles exist in the target controller.
- VFX/audio loops have explicit cleanup.
- Imported VFX scripts do not call `Destroy` on pooled cue roots.
- Source/target/duration beams are driven by a beam adapter or native project
  cue, not only by `CombatVfxCueProfile`.
- Imported materials render correctly in URP without pink/black fallbacks,
  broken alpha sorting, or excessive overdraw.
- Particle/mesh-heavy LV3 candidates are not reused for normal attack beats.

Runtime counters to verify in review scenes:

- `SummonUsed`
- `SummonProjectileDamageApplied`
- `SummonPressureBlocked`
- `SummonFrontlineProxyPresenter.EntryVfxCueRequestCount`
- `SummonFrontlineProxyPresenter.AttackVfxCueRequestCount`
- `SummonPressureScreenPresenter.ActivationVfxCueRequestCount`
- `SummonPressureScreenPresenter.InterceptVfxCueRequestCount`
- active projectile count returns to zero after cleanup,
- active pressure screen count returns to zero after cleanup.

Review scenes:

- `_Game/Scenes/OlympusCorridorInvasionStage.unity`
- `_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity`
- `_Game/Scenes/ActionFoundationBossSummonDuelReview.unity`
- `_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity`

Visual QA gates:

- Slot2 LV2: a viewer can identify entry, lock line, beam fire, and hit-confirm
  within one use.
- Slot2 LV3: the bigger beam reads as a high-tier cashout without becoming a
  Slot3 shield/suppress field.
- Slot3 LV2: shield activation and at least one intercept are visible without
  hiding the player.
- Slot3 LV3: break-wall/suppress impact feels heavier than LV2, but does not
  become uncontrolled explosion spam.
- Boss AuraCaptain: enemy-side pressure reads hostile and distinct from player
  summons.
- Mobile framing: no beam/shield effect covers critical boss/player tells for
  longer than its authored beat.

## Final Locked Matching

| Slot | Actor | Animation direction | Primary VFX direction | Audio direction | Final caveat |
|---|---|---|---|---|---|
| Slot2 | `PF_SummonSlot2Actor_MarksmanProxy` / BacklineShooter | `EliteSummonPackage`, then `AttackLinePressure` / `EliteAuraBuffer`, LV3 `ElitePhaseSwap` | Implemented native `_Game` lock/fire/hit cue chain: `SummonSlot2BeamLock`, `SummonSlot2BeamFire`, `SummonSlot2BeamHit`; raw PixPlays beam remains adapter-only future swap | Elite summon entry + promoted MagicMissiles arcane overlays; no authored temp SFX on the new cue prefabs | First runtime pass is implemented and batch-validated; visual review decides whether this is strong enough or should swap to a real beam adapter. |
| Slot3 | `PF_SummonSlot3Actor_VanguardProxy` / FinalStandCommanderElite | `EliteSummonPackage`, `EliteShieldCycle`, `AttackHeavy` or `EliteArmorBreak` | Implemented native `_Game` shield activation/hit cue chain: `SummonSlot3ShieldRaise`, `SummonSlot3ShieldHit`; raw `EarthShield`/`EarthBlast` remain review-only | Elite summon entry + promoted MagicMissiles shield overlays; no authored temp SFX on the new cue prefabs | Best current runtime match is now wired; human review decides scale/readability, not role identity. |
| Boss pressure | `PF_BossSummonPressureActor_Proxy` / AuraCaptainElite | Existing boss pressure command vocabulary, optionally `AttackLinePressure` / `EliteAuraBuffer` | Boss pressure aura/screen with beam-gun anchors; can borrow LV3 beam language for enemy threat | Enemy pressure/summon palette later | Reference and hostile mirror only. |

## 2026-06-30 Implementation Lock

This pass moved the safest next pass from plan to implementation:

- Slot2 has a named presentation driver on the successful support summon path.
  `SummonUsed` requests `SummonSlot2BeamLock`, the actual volley requests
  `SummonSlot2BeamFire`, and projectile damage requests `SummonSlot2BeamHit`.
- Slot3 pressure screen activation now requests `SummonSlot3ShieldRaise`, and
  each intercepted projectile requests `SummonSlot3ShieldHit`.
- New native cue prefabs are saved under
  `_Game/Art/VFX/CombatCues/Prefabs/` and carry promoted MagicMissiles overlays:
  `CueAssetVfx_MagicMissilesArcaneBeamLock`,
  `CueAssetVfx_MagicMissilesArcaneBeamCharge`,
  `CueAssetVfx_MagicMissilesArcaneBeamHit`,
  `CueAssetVfx_MagicMissilesHolyShieldRaise`, and
  `CueAssetVfx_MagicMissilesHolyShieldHit`.
- ArkData support used for this decision:
  `CombatPayload_ApplyData_2026-06-25/docs/combat_payload_family_guide.md`
  separates effect payload, projectile/hit events, and presentation feedback;
  `PGR_Tutorial_Stage_Data_2026-06-19/derived/pgr-tutorial-stage-focus.csv`
  includes `ChessSkillKeyframe` rows where `Anim`, `Effect`, `Attack`, and
  `Cue` are explicit sequence beats. This supports lock/raise/fire/hit as
  separate presentation events instead of one vague effect.

Batch validation passed after this lock:

- `ReapplyBossBarrageLaneReviewSceneMenu`: `EXIT=0`
- `ValidateBossBarrageLaneReviewSceneMenu`: `EXIT=0`
- `ValidateCombatVfxCuesMenu`: `EXIT=0`
- `ReapplyBossSummonDuelReviewSceneMenu`: `EXIT=0`
- `ValidateBossSummonDuelReviewSceneMenu`: `EXIT=0`

Human review is still required for these points:

- Slot2 lock cue must feel like a committed beam target, not a small muzzle
  sparkle.
- Slot2 fire/hit must be strong enough to read over the projectile fallback.
- Slot3 shield raise must read as a protective wall without hiding player/boss
  tells.
- Slot3 hit must feel heavier at LV3 through tier intensity without becoming
  explosion spam.
- Dragon remains future-only: dragon prefabs and breath/spit animations exist,
  but no mouth-origin beam driver or promoted dragon beam cue is wired.
