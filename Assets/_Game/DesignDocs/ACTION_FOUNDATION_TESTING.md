# Action Foundation Testing

Last updated: 2026-06-12 KST

This note records how to inspect the first direct-control ARPG action-feel foundation without expanding the scope into summons, bosses, progression, or full HUD work.

## Test Setup

- Target scene: `Assets/_Game/Scenes/ActionFoundationTest.unity`.
- The scene contains the existing player gameplay root, one curated CombatGirl SwordShield visual child, one basic sci-fi soldier placeholder, a camera, a readable telegraph marker, and win/fail markers.
- This is an authored inspection scene, not production runtime generation.
- The CombatGirl visual is promoted into `_Game` as game-owned model, minimal animation clips, Animator Controller, primary textures, and Unity Toon Shader materials. It must not reference `_Imported/`.
- Editor validation menu: `DimensionBrawl > Validate Action Foundation Test Scene`.
- The validator opens the test scene and checks required objects, ownership components, key references, Animator wiring, root-motion-off state, and reference-backed timing values.
- If the open scene was already loaded before CombatGirl material promotion, `Assets > Refresh` may not update unpacked scene renderer slots. Use `DimensionBrawl > Reapply Action Foundation CombatGirl Materials` to reassign the open scene's CombatGirl renderers to `_Game` material assets and save the scene.
- PlayMode smoke tests: `Assets/_Game/Tests/PlayMode/ActionFoundationPlayModeTests.cs`.
- The smoke tests load the scene, move the player, trigger dodge, verify dodge tint feedback, verify a basic attack damages the soldier, and verify win/fail encounter states.

## Controls

- Move: WASD, arrow keys, or gamepad left stick.
- Look/facing bias fallback: gamepad right stick.
- Basic attack: left mouse, Enter, or gamepad west button.
- Dodge: Space, Left Shift, or gamepad south button.
- Mobile hooks exist as `SetMoveInput`, `SetLookInput`, `QueueBasicAttack`, and `QueueDodge`; full mobile HUD is intentionally not included yet.

## Reference Values Used

- Input dead zone / buffer scale: `0.10`, from the collected `0.08-0.12s` input-buffer range.
- Basic attack windows:
  - Hit 1: startup `0.12s`, active `0.08s`, recovery `0.28s`, hit-stop `0.03s`.
  - Hit 2: startup `0.14s`, active `0.09s`, recovery `0.32s`, hit-stop `0.03s`.
  - Hit 3: startup `0.18s`, active `0.10s`, recovery `0.42s`, hit-stop `0.05s`.
- Dodge: total `0.62s`, invulnerable `0.05-0.40s`, recovery `0.18s`.
- Dodge feedback: player test renderers tint during the `0.62s` dodge movement window, then clear during recovery.
- Visual materials: the CombatGirl pass uses copied primary albedo textures on `_Game` Unity Toon Shader materials. Original asset-pack masks, matcaps, and advanced shader behavior are intentionally not promoted in this slice.
- Basic soldier telegraph: `0.65s`, from the collected readable enemy telegraph range of `0.45-0.9s`.
- Basic soldier active: `0.14s`, from the collected active-window range of `0.04-0.45s`.
- Basic soldier recovery: `0.45s`, from the collected enemy recovery range of `0.35-1.0s`.
- Basic soldier hit reaction: `0.24s`, from the collected light stagger range of `0.18-0.35s`.
- Camera cue duration: `0.24s`, from the collected camera-cue range of about `0.20-0.32s`.

## First-Pass Deviations

The collected references do not provide trustworthy numeric defaults for these values yet, so they are exposed in the Inspector:

- Player move speed, acceleration, deceleration, turn rate, and stop threshold.
- Dodge speed/distance.
- Soldier approach speed and knockback speed.
- Camera offset, follow damping, target/threat bias, and lead distance.
- Hit flash duration.

## Expected Result

- The player can move, stop with a short settle request, attack in a short three-hit chain, dodge with invulnerability timing, take damage, and die.
- The CombatGirl visual is visible under the existing player root; gameplay remains on the root components.
- Movement writes `MoveSpeed`, `MoveX`, `MoveY`, and `IsStopping`; dodge and basic attacks trigger `Dodge`, `Attack1`, `Attack2`, and `Attack3` on the minimal Animator Controller.
- The soldier approaches, telegraphs, attacks, recovers, staggers when hit, and dies.
- The camera follows the player and biases focus toward the current threat.
- The player visibly changes tint during dodge movement, then returns to its authored material colors instead of staying in the old cyan placeholder tint.
- Win marker appears when the soldier dies.
- Fail marker appears when the player dies.
- The inspection scene must not render with pink/missing-shader materials.

## Verification Evidence

- `dotnet build C:\Git\DimensionBrawl\DimensionBrawl.slnx` passes with zero warnings and zero errors.
- Unity batchmode scene validation passes with `Action foundation test scene validation passed.` after closing the open Editor instance.
- Isolated Unity PlayMode smoke tests previously passed with `2` tests run, `2` passed, `0` failed for movement, dodge motion, attack damage, and encounter win/fail.
- The current PlayMode test source adds `DodgeAppliesAndClearsVisibleFeedbackTint`; it compiles in `DimensionBrawl.PlayModeTests`, and should be rerun in Unity when batchmode licensing/headless state allows or confirmed manually in the open Editor.
- Manual Editor inspection confirmed `Left Shift` dodge makes the player placeholder flash/tint visibly.
- Manual Editor inspection confirmed the CombatGirl visual is attached to the player root and its Unity Toon Shader material colors render correctly after reapplying scene renderer slots.
- If the CombatGirl visual returns to pink after refresh, run `DimensionBrawl > Reapply Action Foundation CombatGirl Materials`, then validate the scene again.
- Manual subjective feel review in the open Unity Editor is still useful for tuning speed, acceleration, dodge distance, and camera offset.
- Visual inspection in the open Unity Editor should confirm the test scene uses `_Game` URP materials instead of the built-in default material.

## Deliberately Not Included

- Actual summon behavior.
- Boss phases.
- Progression, currencies, rewards, or full stage loop.
- Full mobile HUD.
- Runtime scene generation for production play.
- Imported asset mass promotion.
- Original asset-pack matcap and mask behavior beyond the copied primary texture color pass.
- Long combo strings, baseline parry, or card UI.
