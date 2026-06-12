# Action Feel Targets

Last updated: 2026-06-12 KST

## North Star

`DimensionBrawl` must first feel like a responsive direct-control action RPG.

The project should chase the action feel found in games such as `Punishing: Gray Raven`, `Honkai Impact 3rd`, and `Zenless Zone Zero`: readable movement, camera support, short manual combo rhythm, satisfying hit feedback, and dodge timing that makes the player feel in control.

More systems do not matter until the basic action loop feels good.

## Required Feel Pillars

### Movement And Stop Feel

Movement must not feel like a capsule abruptly switching between full speed and idle.

Targets:

- Acceleration and deceleration should be tunable.
- Direction changes should feel responsive without instant puppet rotation.
- Stopping should have a visible settle or stop-step feel when animation support exists.
- Movement code should expose values for speed, acceleration, deceleration, turn rate, and stop threshold.
- The player should be able to cancel movement into attack or dodge without feeling stuck.

Do not accept:

- Hard snapping from run to idle as the final feel.
- Hidden magic constants for movement feel.
- Animation-independent movement that makes the character slide forever.
- Overbuilt locomotion graphs before the first movement test works.

### Camera Feel

The camera is part of combat readability, not just a following transform.

Targets:

- The camera should keep player, current threat, and forward danger readable.
- Camera movement should use damping and target bias rather than harsh teleporting.
- Combat cues may add short shake, zoom, offset, or focus bias.
- Camera cues must be short, additive, and explicitly cleaned up.
- The camera must not hide enemy telegraphs or trap the player behind UI.

Do not accept:

- A camera that follows position only and ignores combat direction.
- Long cinematic locks during normal attacks.
- Camera logic hidden inside player attack code.
- Unbounded shake or FOV changes on every hit.

### Manual Attack Feel

The first attack loop is a short manual basic combo, not automatic combat.

Targets:

- V1 basic attack can grow to 5-7 hits when curated animation clips support readable recovery, cancel timing, and a clear finisher read.
- Each hit should have readable startup, active, hit-confirm, recovery, and cancel windows.
- Input buffering should make the next hit feel intentional but not mashy.
- Hit feedback should include at least damage, reaction, and a small presentation cue.
- Attack direction should bias toward the current threat or facing direction.

Do not accept:

- Long combo strings that are not backed by curated clips, readable input windows, and explicit cancellation rules.
- Extra attack buttons before the first chain feels good.
- Damage applied with no readable hit timing.
- All attack, animation, camera, and damage logic packed into one giant class.

### Dodge Feel

Dodge must be immediate enough to trust and structured enough to tune.

Targets:

- Dodge should have a short duration, recovery, and damage-avoidance window.
- Dodge should be cancellable from sensible points in movement or attack.
- Perfect-dodge, counter, or summon opportunity systems are later layers, not V1 requirements.
- Dodge feedback should clearly show that the player avoided danger.

Do not accept:

- A dodge that is only a movement burst with no timing definition.
- A dodge that locks the player so long it feels worse than walking.
- A baseline parry button before dodge and basic attack feel good.

### Hit And Enemy Feedback

The first enemy exists to test feel, not to prove AI complexity.

Targets:

- The basic soldier should attack in a readable way.
- Player hits should produce reaction, damage, and a visible state change.
- Enemy attacks should have a telegraph if they can damage the player.
- Death, hit stun, and invulnerability rules should be simple and visible.

Do not accept:

- Enemy pressure with no telegraph.
- Damage numbers without physical or animation response.
- Complex squads, boss phases, affixes, or summon-answer mechanics before the basic soldier test works.

## Data Documents To Use Actively

Use the collected research as design guardrails, not as code to copy.

Primary references:

- `COMBAT_V1_SPEC.md`: first implementation scope.
- `COMBAT_FEEL_FRAME_REFERENCE_RESEARCH.md`: timing windows, hit-stop, dodge, cue bundles, frame tags.
- `COMBO_SYSTEM_REFERENCE_RESEARCH.md`: short manual combo and assist/QTE translation.
- `ARPG_REFERENCE_RESEARCH.md`: camera, enemy pattern, and horizontal ARPG translation.
- `BOSS_ENEMY_RUN_REFERENCE_RESEARCH.md`: later pressure pockets and relief windows.
- `SUMMON_SYSTEM_REFERENCE_RESEARCH.md`: later summon opportunity windows.

When implementing an action feature, state which reference idea is being used and which scope-expanding parts are deliberately excluded.

For timing and tuning values, consult the collected numeric ranges before inventing new defaults. Start from documented ranges in `COMBAT_FEEL_FRAME_REFERENCE_RESEARCH.md`, `ComboSystemReferenceDataset.json`, `CombatFeelFrameReferenceDataset.json`, and related design data, then record any deliberate deviation.

## Architecture Guardrails

Action feel work must stay inspectable and modular.

- Player code owns input interpretation, movement, attack requests, dodge, and local animation requests.
- Combat code owns damage, hit validation, health, and temporary combat state.
- Enemy code owns enemy movement, attack execution, hit reaction, and death.
- Presentation code owns animation, VFX, SFX, camera cues, explicit authored cue bundles, and UI feedback. Normal attacks must not add global time-scale changes unless a later perfect-dodge, counter, ultimate, or authored cue slice explicitly owns that effect.
- Data owns reusable tuning values and timing profiles.

Do not combine all of this into a single manager.

If a feature needs more than three new scripts, stop and write the ownership split before implementation.

## First Quality Gate

Before summon behavior, boss phases, progression, or full HUD work, the project should pass this quality gate:

1. Player movement feels responsive and does not hard-stop unnaturally.
2. Camera keeps player and basic soldier readable.
3. Basic attack chain has at least 3 readable hits and may extend to a reviewed 5-7 hit chain when animation support is strong.
4. Dodge can avoid a simple enemy attack.
5. Player and enemy can damage each other.
6. Hit feedback is visible enough to review from a short recording.
7. The encounter can be won or failed.

If this gate does not pass, do not build larger systems to cover the weakness.

## Future Codex Goal Template

When asking Codex to implement action features, include this instruction:

```text
Read these first:
- Assets/_Game/DesignDocs/COMBAT_V1_SPEC.md
- Assets/_Game/DesignDocs/ACTION_FEEL_TARGETS.md
- AI_CODE_CONTRACT.md
- ARCHITECTURE_BOUNDARIES.md

Implement only the next smallest action-feel step.
Do not implement summon behavior, boss phases, progression, full HUD, or broad runtime generation.
Use the collected design docs actively: name the timing/feel idea being implemented and the parts deliberately excluded.
Use the collected reference numeric data actively: choose initial timing/tuning values from documented ranges for movement, stop feel, attack startup/active/recovery, dodge duration/avoidance/recovery, hit feedback, camera cue duration, and enemy telegraph timing; record any deliberate deviation. Do not add normal-hit global slow motion from hit-stop reference data without a separate explicit perfect-dodge/cue-system goal.
Keep code ownership narrow and inspectable.
Stop before adding more than three new gameplay scripts without an ownership review.
```

Good first goals:

- Implement player movement with tunable acceleration, deceleration, turn rate, and stop threshold.
- Add a camera follow/target-bias prototype that keeps player and one soldier readable.
- Add a curated 3-5 hit basic attack chain with explicit timing windows; extend toward 6-7 only after visual review confirms the later clips still read like normal attacks.
- Add dodge with a tunable avoidance window and recovery.
- Add one basic soldier with readable attack, health, hit reaction, and death.

Bad first goals:

- Build the summon system.
- Build all mobile HUD controls.
- Build a boss run.
- Import and wire every asset pack.
- Generate a complete scene at runtime.
