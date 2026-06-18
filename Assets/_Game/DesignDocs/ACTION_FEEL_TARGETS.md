# Action Feel Targets

Last updated: 2026-06-18 KST

## North Star

`DimensionBrawl` must first feel like a responsive mobile-first fixed-rear boss-barrage game where the player controls spacing, risk, and summon timing while summons deliver the main battlefield swing.

The reference direction still includes `Punishing: Gray Raven`, `Honkai Impact 3rd`, and `Zenless Zone Zero`, but the current V1 priority is no longer a melee combo showcase. The priority is:

- readable third-person movement,
- fixed rear camera readability for far boss/projectile threats,
- quick dodge trust,
- simple local-defense attacks from the player against close threats,
- forward-position risk that charges summon energy faster,
- EN level strategy where waiting upgrades skill/summon power but spending resets the climb,
- a summon call that clearly changes the fight,
- visible boss/projectile/summon exchange and pocket clear feedback.

More systems do not matter until this boss-barrage + summon loop feels clear.

## Required Feel Pillars

### Movement And Stop Feel

Movement must not feel like a capsule abruptly switching between full speed and idle.

Targets:

- Acceleration and deceleration should be tunable.
- Direction changes should feel responsive without instant puppet rotation.
- Stopping should have a visible settle or stop-step feel when animation support exists.
- Movement code should expose values for speed, acceleration, deceleration, turn rate, and stop threshold.
- The player should be able to cancel movement into local-defense attack, dodge, or summon call without feeling stuck.
- The player must not be able to cross the authored midline/forward boundary.

Do not accept:

- Hard snapping from run to idle as the final feel.
- Hidden magic constants for movement feel.
- Animation-independent movement that makes the character slide forever.
- Overbuilt locomotion graphs before the first movement test works.

### Camera Feel

The camera is part of combat readability, not just a following transform.

Targets:

- The first slice should use a fixed rear/over-the-shoulder battle view.
- The camera should keep player, forward boundary, far boss/proxy, incoming projectiles, and summon impact readable.
- Camera movement should use small damping/cue offsets only after the fixed read is proven.
- Combat cues may add short shake, zoom, offset, or focus bias.
- Summon/assist camera cues must be short, additive, and explicitly cleaned up.
- The camera must not hide enemy telegraphs, summon impact, or route blockers behind UI.

Do not accept:

- A camera that follows position only and ignores combat direction.
- A free-orbit camera as the baseline before the boss projectile read is proven.
- Long cinematic locks during normal attacks.
- Camera logic hidden inside player attack code.
- Unbounded shake, FOV, or time-scale changes on every hit.

### Player Local-Defense Feel

The first player action loop is survival, positioning, and local defense, not a melee combo race or full shooter loop.

Targets:

- `BasicDefenseAttack` should respond quickly enough to handle close or approaching monsters.
- The attack can be a short slash, short magic projectile, or gun-like fire after asset/readability review.
- The attack should bias toward the current close threat, facing direction, or corridor target lane.
- The attack needs readable startup, release/active, impact, and recovery values.
- The player may drift or root briefly during the attack only when it improves readability and is exposed as serialized tuning.
- Hit feedback should include at least damage, reaction, and a small presentation cue.
- Player damage should not outshine the summon response in the first slice.

Do not accept:

- A melee combo chain as the new V1 goal.
- A projectile/slash that is only a visual effect with no clear damage authority.
- A hitscan/projectile/melee overlap that scene-searches for targets every frame.
- Attack, animation, camera, and damage logic packed into one giant class.

### Lane Risk And Summon Energy Feel

The player should understand why moving forward matters even when it is dangerous.

Targets:

- The lane should expose at least a back safety zone and a forward risk zone.
- The player cannot cross the midline/forward boundary. This is a core identity rule, not a temporary blocker.
- Summon energy gain should be visibly or measurably faster near the forward boundary.
- Backline play should feel safer but slower to charge.
- `EN LV1`, `EN LV2`, and `EN LV3` should feel like meaningful decision points.
- Spending at LV1 should feel valid for urgent defense, while waiting for LV2/LV3 should feel like a stronger but riskier plan.
- Using skill or summon energy should clearly reset the climb back to empty LV1 charging.
- Boss projectile spacing should feel tighter near the forward boundary and looser near the backline.
- Close monsters can punish passive backline play or force local-defense decisions, but they must not replace the far boss/summon standoff as the main structure.
- Tuning should be data-driven through lane-space curves, zone profiles, or serialized pattern values. `SummonEnergyLadder` risk-band boundaries are now serialized and should be tuned here before changing code defaults.

Do not accept:

- A forward boundary that is only an invisible wall with no combat meaning.
- Any player movement, dodge, attack, lock-on, or camera behavior that lets the player drift past the midline.
- A charge rule that is hidden in code and impossible to tune in Inspector/data.
- A tier ladder where the best answer is always to wait for LV3.
- A tier ladder where LV2/LV3 feel like only bigger numbers with no readable presentation upgrade.
- Spending energy without clear reset/readiness feedback.
- Projectile density that changes only by camera illusion while gameplay hit spacing remains identical.
- A design where the safest backline position is also the best summon-energy strategy.

### Summon Call Feel

The first summon must read as the reason the game exists.

Targets:

- `SummonSlot1` should be easy to understand and quick to test.
- The summon should enter or act with clear target context.
- The summon should prefer the far/frontline boss exchange over the player's temporary close-threat target unless a specific summon role says otherwise.
- The summon should visibly change the boss-barrage exchange through damage, projectile blocking, pressure break, tanking, field control, heal/field, or another explicit role.
- A successful summon pressure answer should open a short readable follow-up choice in the review pocket, such as a tier-aware EN pulse that lets the player immediately answer with `Skill1` and confirm the boss/proxy hit before the pocket can clear.
- Defeating the local close threat should produce an in-world summon-block opportunity read and a short additive camera cue before the player solves boss pressure, so the loop does not depend only on HUD text.
- The follow-up window, confirmed hit, and missed response should each have a small presentation read through authored camera/VFX cues, not only a HUD text change.
- LV1/LV2/LV3 summon versions should read as the same summon concept at stronger tiers, not three unrelated features.
- Summon use should read from the player side first, such as a magic circle in front of the player before the summon launches/enters.
- Entry, impact, exit, UI, camera, and cleanup cues should be explicit data or narrow components.
- The first implementation may be one summon/assist only; it must still feel more important than the player's basic shot.

Do not accept:

- A random auto-pet that does invisible background damage.
- A full roster, upgrade economy, or inventory before one call works.
- A tier system that immediately becomes a progression/upgrade economy.
- A hand-of-cards UI or manual target-selection UI as the default V1 control.
- Summon behavior hidden inside player, enemy, or encounter code.

### Dodge Feel

Dodge must be immediate enough to trust and structured enough to tune.

Targets:

- Dodge should have a short duration, recovery, and damage-avoidance window.
- Dodge should be cancellable from sensible points in movement or local-defense attack.
- Perfect-dodge, counter, and advanced summon opportunities are later layers.
- Dodge feedback should clearly show that the player avoided danger.

Do not accept:

- A dodge that is only a movement burst with no timing definition.
- A dodge that locks the player so long it feels worse than walking.
- A baseline parry button before dodge, local defense, and summon call feel good.

### Boss Projectile And Hit Feedback

The first far boss/proxy and enemies exist to test why the summon is useful.

Targets:

- The boss/proxy should fire readable regular projectiles and committed skill patterns from the far side.
- Regular boss fire should maintain rhythm and pressure without making every shot feel like a major pattern.
- Boss skill patterns should have visible windup, telegraph, sequence/cooldown data, and clear dodge or summon answers.
- Projectile patterns should make forward space riskier and back space safer without becoming unreadable.
- Close or approaching monsters may test the player's local-defense attack.
- Basic sci-fi enemies may still support the test as pressure pieces, but they are not the main V1 fantasy by themselves.
- Player shots and summon hits should produce reaction, damage, and a visible state change.
- Player basic fire should remain input-led with weak aim assist only. It should not hard-lock or inherit boss-pattern tracking.
- Player skill projectiles may later borrow boss-pattern shapes, but only as costed/cooldown skill actions with readable commitment and counterplay.
- Summon projectiles should read as frontline intervention and may share projectile mechanics, but should not become invisible bonus player shots.
- Enemy attacks should have a telegraph if they can damage the player.
- Death, hit stun, invulnerability, and pocket clear rules should be simple and visible.

Do not accept:

- A single projectile implementation that hides different gameplay verbs behind one uninspectable branch pile.
- Boss pattern fire copied directly into player basic fire.
- Player/PvP-facing shots that auto-track without readable startup or counterplay.
- Enemy pressure with no telegraph.
- Damage numbers without physical or animation response.
- Complex squads, boss phases, affixes, or reward loops before the first summon pocket works.

## Quantitative Design Inputs (arknights / Ark Analysis)

Use `/C:/Ark/SubcultureGameData/games/arknights/notes/` as a hard design reference for pressure pacing before making lane/route edits.

Reference values are observational and should guide relative balancing, not be copied as hardcoded formulas:

- Route pressure density (normalized):
  - route weighted pressure median / p90 / max = `22 / 58 / 600.85`
  - stage weighted pressure median / p90 / max = `654.4 / 1437.5 / 5209.9`
- 15-second pressure distribution:
  - all window weighted pressure median / p90 / max = `64.3 / 202.7 / 2328`
  - peak 15-second pressure share median / p90 / max = `28.98 / 45.49 / 95.54`
  - top-3 window pressure share median / p90 / max = `66.36 / 85.9 / 100`
- Route-pressure concentration:
  - dominant route share in top windows is common and should read as intentional burst points, not random spam.
- Endpoint concentration:
  - median dominant endpoint pair share = `39.05%`, p90 = `63.8%` (one entrance/goal pair can carry most pressure).
  - endpoint pair pressure concentration median / p90 / max pressure = `71 / 370.9 / 2547.05`.

When building the next pocket/segment pass, record and keep:

1. `targetPeakWindowSharePct` (how much pressure is packed into each burst window),
2. `targetTop3WindowSharePct` (how concentrated or spread the pressure is),
3. `routeDominanceShare` (whether one lane repeatedly owns pressure in a pocket),
4. `entryExitLaneBias` (whether forward-risk and backline lanes stay asymmetric in a readable way),
5. `timeToNextReliefWindow` (distance between high-pressure spikes),
6. `riskDifferential` (forward risk spacing vs backline risk spacing remains clearly visible).

The first review pocket does not need full parity with Arknights scale. It should just preserve the *shape*:
clear burst + readable recovery spacing + explicit overpressure lane burden.

## Data Documents To Use Actively

Use the collected research as design guardrails, not as code to copy.

Primary references:

- `COMBAT_V1_SPEC.md`: current fixed-rear boss-barrage + summon-first V1 scope.
- `SUMMON_SYSTEM_REFERENCE_RESEARCH.md`: summon opportunity windows, assist entry, role behavior, target-relative entry, and cleanup contracts.
- `COMBAT_FEEL_FRAME_REFERENCE_RESEARCH.md`: timing windows, hit-stop boundaries, dodge, cue bundles, and frame tags.
- `COMBO_SYSTEM_REFERENCE_RESEARCH.md`: cancel rhythm and assist/QTE translation, not a requirement to keep melee as the main loop.
- `ARPG_REFERENCE_RESEARCH.md`: camera, enemy pattern, and mobile ARPG readability.
- `BOSS_ENEMY_RUN_REFERENCE_RESEARCH.md`: later pressure pockets and relief windows.
- `LINEAR_STAGE_DESIGN_FOUNDATION.md`: corridor/pocket authoring language.

When implementing an action feature, state which reference idea is being used and which scope-expanding parts are deliberately excluded.

For timing and tuning values, consult the collected numeric ranges before inventing new defaults. Start from documented ranges in `COMBAT_FEEL_FRAME_REFERENCE_RESEARCH.md`, `ComboSystemReferenceDataset.json`, `CombatFeelFrameReferenceDataset.json`, summon research data, and related design data, then record any deliberate deviation.

## Architecture Guardrails

Action feel work must stay inspectable and modular.

- Player code owns input interpretation, movement, dodge, local-defense attack requests, target-bias intent, and local animation requests.
- Summon code owns summon entry/action/exit behavior, summon target use, summon timing, and summon-local animation requests.
- Combat code owns damage, hit validation, health, teams, and temporary combat state.
- Enemy code owns enemy movement, attack execution, hit reaction, and death.
- Presentation code owns animation, VFX, SFX, camera cues, explicit authored cue bundles, and UI feedback.
- Data owns reusable tuning values, timing profiles, role profiles, and summon entry/cue profiles.

Do not combine all of this into a single manager.

If a feature needs more than three new gameplay scripts, stop and write the ownership split before implementation.

## First Quality Gate

Before boss phases, progression, reward loops, full HUD, or chapter production art, the project should pass this quality gate:

1. Player movement feels responsive and does not hard-stop unnaturally.
2. Fixed rear camera keeps player, forward boundary, far boss/proxy, incoming projectiles, and summon impact readable.
3. Player cannot cross the authored midline/forward boundary.
4. Forward positioning charges summon energy faster than backline positioning.
5. `EN LV1~LV3` charging, button upgrade, and spend reset are visible enough to understand from play.
6. Projectile spacing/risk feels tighter near the forward boundary and looser near the backline.
7. `BasicDefenseAttack` can answer close or approaching monsters with clear startup, active/release, impact, and recovery when included.
8. Dodge can avoid a simple boss projectile or enemy attack.
9. `Skill1` can fire immediately at the current available EN level.
10. `SummonSlot1` can call one LV1/LV2/LV3 summon/assist that visibly changes the boss-barrage exchange, including a short ally screen that visibly pulses/flashes while intercepting hostile boss projectiles.
11. A correct summon pressure block can open a brief follow-up window where a small EN pulse enables an immediate `Skill1` response, and the review state can distinguish firing from actually hitting the boss/proxy before clear.
12. Player, summon, boss/proxy, and enemy can damage valid hostile targets through shared team rules.
13. Hit feedback, energy feedback, skill feedback, and summon feedback are visible enough to review from a short recording.
14. The pocket can be won, paused, or failed.

If this gate does not pass, do not build larger systems to cover the weakness.

## Future Codex Goal Template

When asking Codex to implement action features, include this instruction:

```text
Read these first:
- Assets/_Game/DesignDocs/COMBAT_V1_SPEC.md
- Assets/_Game/DesignDocs/ACTION_FEEL_TARGETS.md
- Assets/_Game/DesignDocs/SUMMON_SYSTEM_REFERENCE_RESEARCH.md
- AI_CODE_CONTRACT.md
- ARCHITECTURE_BOUNDARIES.md

Implement only the next smallest fixed-rear boss-barrage + summon-first step.
Do not rebuild the melee combo loop as the main product direction.
Do not implement full summon roster, boss phases, progression, full HUD, or broad runtime generation.
Use the collected design docs actively: name the timing/feel/summon idea being implemented and the parts deliberately excluded.
Use the collected reference numeric data actively: choose initial timing/tuning values from documented ranges for movement, dodge duration/avoidance/recovery, local-defense startup/active/recovery, projectile release/flight/impact when used, projectile spacing, forward-position summon-energy gain, EN tier thresholds, skill/summon spend reset, summon entry/impact/exit, hit feedback, camera cue duration, and enemy telegraph timing; record any deliberate deviation.
Do not add normal-hit global slow motion from hit-stop reference data without a separate explicit perfect-dodge/cue-system goal.
Keep code ownership narrow and inspectable.
Stop before adding more than three new gameplay scripts without an ownership review.
```

Good first goals:

- Add a fixed rear boss-barrage lane with a player forward boundary.
- Add a forward-position summon-energy gain curve.
- Add an `EN LV1~LV3` tier ladder with visible button upgrades and spend reset.
- Add one readable boss/proxy projectile pattern whose front/back risk difference can be tuned.
- Add one local-defense attack for close or approaching monsters.
- Add one immediate `Skill1` and one summon slot call with LV1/LV2/LV3 versions.
- Add minimal camera/VFX cues that make projectile threat, summon entry, and summon impact readable.

Bad first goals:

- Rebuild the melee combo system as the new center.
- Build the full summon system.
- Build all mobile HUD controls.
- Build a boss run.
- Import and wire every asset pack.
- Generate a complete scene at runtime.
