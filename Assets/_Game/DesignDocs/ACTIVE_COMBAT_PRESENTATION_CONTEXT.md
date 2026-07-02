# Active Combat Presentation Context

Last updated: 2026-07-02 KST

Read this before any combat-presentation, VFX, shader, animation, dodge, or HUD work. This file is a short memory anchor for the current direction after the ArkData reread.

## Non-Negotiables

- Keep the three summon slots. Do not collapse the combat into one generic summon action.
- Do not confuse the old HUD with the current boss-barrage lane review HUD.
- Do not use ArkData as a direct commercial art source. Use ArkData as contract, timing, state-machine, and presentation-layer evidence.
- Use local reviewed assets under `_Game/Art/VFX/ActionFoundation` or user-provided UI art for implementation.
- Keep work sequential. Finish and verify one numbered slice before moving to the next; commit at the end of each completed slice when requested.
- Do not introduce a broad boss phase manager, tutorial framework, generated scene, hidden fallback setup, or `_Imported` runtime dependency as a shortcut.

## ArkData Conclusion

ArkData is strongest as L2/L3 reference data, not as ready-to-import Unity art.

Useful ArkData entry points:

- `CombatPayload_ApplyData_2026-06-25`: combat runtime pipeline: action request -> trigger -> cost -> target -> projectile/hit -> effect/status -> presentation feedback -> state/log.
- ZZZ combat/rendering/postprocessing/camera folders: lifecycle hooks such as `OnHitOther`, `OnBeingHit`, `OnAttackLanded`, material property actions, rim/outline/emission/dissolve-like signals, screen effects, camera events, and time-slow presentation cues.
- `HI3_CombatCutscene_ApplyData_2026-06-26`: QTE/switch/evasion contract. Evasion should follow `evade_input -> perfect_evasion -> time_fracture_or_counter -> recovery`, not "slow time on every dodge".
- `PGR_CombatHUDOverlay_ApplyData_2026-06-26`: HUD focus/target/input/completion contracts. Useful for explaining and highlighting existing controls, not for cloning exact HUD layout.
- `UIMotionTransition_ApplyData_2026-06-26`: UI motion primitives and input-policy separation.
- `CutscenePattern_ApplyData_2026-06-24`: sequence grammar: scene setup -> camera cue -> actor state -> dialogue/reaction -> presentation FX -> gameplay handoff.

Direct ArkData art availability:

- Game `assets` folders mostly contain path-reference CSV/JSON/MD files, not decoded media.
- `pgr_base_assets` has Unity `.assets/.resS` files only. Do not treat them as ready production assets.

## Current Project Hooks

The project already has enough presentation hooks for the next work. Prefer wiring and tightening these over creating new managers.

- `PlayerRangedBasicVfxCueDriver`: already listens to `LaneActionProjectile.DamageApplied`. `playImpactVfx` is currently configured false in setup/tests even though `PlayerRangedProjectileImpact` exists.
- `EnemyCombatVfxCueDriver`: already listens to `CombatHealth.Damaged` and can emit `EnemyHit`.
- `CombatHitFeedback`: currently simple material-color flash. Improve only as a narrow hit-readability layer.
- `ActionScreenCuePresenter`: already handles player damage, dodge, energy, boss, frontline, follow-up, and result screen cues. Check whether current scene/test setup disables it before assuming the feature is broken.
- `JustDodgeDetector`: exists but has legacy namespace `IsekaiBrawl.Gameplay`. Treat as a candidate/reference until actual current-scene wiring is verified.
- `ActionCinematicCueDirector` and `ActionCinematicCueProfile`: already support short timeScale cues for authored cinematic/follow-up beats.

## Recommended Sequential Slices

1. Hit feedback first.
   - Enable/verify player ranged projectile impact.
   - Ensure enemy/boss/summon damage events visibly respond with impact VFX plus material feedback.
   - Use ArkData CombatPayload/ZZZ lifecycle as the evidence: presentation must be tied to a real projectile or damage event.

2. Exact dodge slow-motion second.
   - Trigger only from actual just-dodge/perfect-evasion success, not generic dodge start.
   - Use a short unscaled presentation window, roughly 0.18-0.26 seconds, and restore time scale reliably.
   - Pair with a small screen/camera/material cue rather than global cinematic takeover.

3. Jump slam replacement / charge presentation third.
   - Replace the awkward jump-down slam behavior with a charge/dash-style read.
   - Prefer local candidates `PF_SummonChargeRushTrail_SPECIAL` and `PF_SummonChargeImpact_SPECIAL`.
   - Remove or ignore the cheap default-material landing effect.

4. Summon HUD readability fourth.
   - Keep all three slots.
   - Use the user's badge/icon art.
   - Use PGR HUD overlay rules only for target focus, input policy, and completion logic.

5. Clean verification last.
   - Add or update tests only around the slice being changed.
   - Verifiers should prove that real events cause presentation feedback, not only that components exist.

## Immediate Risk Notes

- Do not "fix" play feel by adding more HUD text. The missing piece is event-tied combat feedback.
- Do not spend time extracting ArkData art unless a specific blocker appears and the user approves that path.
- Do not assume cue profile references are valid until Unity validation confirms them; some GUID lookup from text search did not resolve under `Assets`.
- Current worktree already had unrelated changes before this note: `InputSystem_Actions.inputactions`, Android build profile, `Settings/DefaultVolumeProfile.asset`, and `../.utmp/`.
