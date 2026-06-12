# Action Foundation Testing

Last updated: 2026-06-12 KST

This note records how to inspect the first direct-control ARPG action-feel foundation without expanding the scope into summons, bosses, progression, or full HUD work.

## Test Setup

- Target scene: `Assets/_Game/Scenes/ActionFoundationTest.unity`.
- The scene contains the existing player gameplay root, one curated CombatGirl SwordShield visual child, one basic sci-fi soldier placeholder, a camera, a readable telegraph marker, and win/fail markers.
- This is an authored inspection scene, not production runtime generation.
- The CombatGirl visual is promoted into `_Game` as game-owned model, sixteen selected animation clips, Animator Controller, primary textures, and Unity Toon Shader materials. It must not reference `_Imported/`.
- Editor validation menu: `DimensionBrawl > Validate Action Foundation Test Scene`.
- The validator opens the test scene and checks required objects, ownership components, key references, Animator wiring, weapon-socket binding, root-motion-off state, and reference-backed timing values.
- If the open scene was already loaded before CombatGirl material promotion, `Assets > Refresh` may not update unpacked scene renderer slots. Use `DimensionBrawl > Reapply Action Foundation CombatGirl Materials` to reassign the open scene's CombatGirl renderers to `_Game` material assets and save the scene.
- If CombatGirl stop animations leave `add_weapon_l` or `add_weapon_r` drifting, use `DimensionBrawl > Reapply Action Foundation CombatGirl Weapon Sockets` to bind those sockets to `hand_l` and `hand_r` while preserving their authored offsets.
- If StopStep responsiveness drifts after reimport, use `DimensionBrawl > Reapply Action Foundation StopStep Responsiveness` to restore the trimmed StopStep import range, fast StopStep Animator transitions, and scene movement tuning.
- PlayMode smoke tests: `Assets/_Game/Tests/PlayMode/ActionFoundationPlayModeTests.cs`.
- The smoke tests load the scene, move the player, verify promoted `StartRun`, fast-window `StopStep`, and 90-degree turn Animator routing, verify CombatGirl weapon sockets stay hand-pinned, trigger directional dodge, verify no-input backward dodge, verify dodge tint feedback, verify short action camera cue cleanup and FOV widening, verify that camera orbit is independent from instant player facing changes, verify the lower close-rear camera preset, verify a five-hit basic combo can reach `Attack5` from timed buffered input, verify attack damage without global slow motion, and verify win/fail encounter states.

## Controls

- Move: WASD, arrow keys, or gamepad left stick.
- Camera orbit / target-bias look: right mouse drag, gamepad right stick, or mobile look drag hook.
- Basic attack: left mouse, Enter, or gamepad west button.
- Dodge: Space, Left Shift, or gamepad south button.
- Player facing follows movement by default; right-stick fallback belongs to camera orbit, not character facing, unless an explicit look action is wired later.
- Mobile hooks exist as `SetMoveInput`, `SetLookInput`, `QueueBasicAttack`, and `QueueDodge`; full mobile HUD is intentionally not included yet.

## Reference Values Used

- Input dead zone / buffer scale: `0.10`, from the collected `0.08-0.12s` input-buffer range.
- Basic attack windows:
  - Hit 1: startup `0.12s`, active `0.08s`, recovery `0.28s`, reserved hit-stop hint `0.03s`.
  - Hit 2: startup `0.14s`, active `0.09s`, recovery `0.32s`, reserved hit-stop hint `0.03s`.
  - Hit 3: startup `0.16s`, active `0.10s`, recovery `0.30s`, reserved hit-stop hint `0.04s`.
  - Hit 4: startup `0.17s`, active `0.10s`, recovery `0.34s`, reserved hit-stop hint `0.05s`.
  - Hit 5: startup `0.20s`, active `0.12s`, recovery `0.46s`, reserved hit-stop hint `0.05s`.
- The active chain currently uses game-owned clips `SS_Attack1` through `SS_Attack5`. `SS_Attack5` is promoted from the first short special attack as a finisher candidate. Additional `SS_Sp_Skill2` / `SS_Sp_Skill3` style clips remain candidates for future 6-7 hit extension only after manual visual review.
- Combo queueing opens after `0.10s` and can chain the next hit at `45%` of recovery once buffered. This is meant to stop later hits from feeling like they drop input after the third hit without turning the basic chain into an every-frame spam path.
- Basic attack hit acquisition ignores colliders under the player root and resolves `CombatHealth` through the hit collider's parent chain, so authored body/weapon colliders on the CombatGirl visual cannot consume the overlap buffer before the soldier target is checked.
- Dodge: total `0.56s`, invulnerable `0.05-0.32s`, recovery `0.14s`, movement burst speed `10.2`.
- Dodge feedback: player test renderers tint during the `0.56s` dodge movement window, then clear during recovery.
- Dodge animation requests are directional: `DodgeForward`, `DodgeBack`, `DodgeLeft`, and `DodgeRight` use promoted CombatGirl Quickshift clips. Held movement input dodges along the current move direction; no-input dodge moves backward from the current facing direction. The movement burst decays over the dodge window so it reads more like a lunge/roll impulse than a flat slide.
- Locomotion animation requests: movement start triggers `StartRun`, movement release triggers `StopStep`, and large running direction changes over `65` degrees can trigger `TurnLeft90` or `TurnRight90` with a `0.32s` cooldown. These states use game-owned promoted clips under `Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/` and return through the Animator Controller instead of referencing `_Imported/`.
- `StartRun`, `TurnLeft90`, and `TurnRight90` must stay on CombatGirl-owned source clips with weapon attachment bones. The current `StopStep` is a reviewed One Hand Base `Run_A_F_To_Idle_InPlace` candidate promoted as `SS_StopStep` because it gives stronger weight transfer than the CombatGirl walk surrogate; `CombatGirlWeaponSocketBinder` keeps `add_weapon_l` and `add_weapon_r` pinned to `hand_l` and `hand_r` so this stop candidate does not depend on CombatGirl-specific attachment curves.
- Stop-settle tuning: `0.26s` stop-settle, `0.16s` final-input hold, `0.06s` Animator move damping, and `0.24` stop-settle MoveSpeed floor.
- StopStep responsiveness tuning: `SS_StopStep` starts at source frame `4`, plays at Animator state speed `1.45`, and all StopStep trigger transitions use `0.015s` fixed-duration blends with no exit time.
- Base camera preset: `cameraOffset (0, 1.05, -4.2)`, `lookOffset (0, 1.2, 0.55)`, threat bias `0.25`, and forward lead `0.35m` for a lower, closer rear ARPG view instead of the older top-down inspection view.
- Camera orbit input: right mouse drag uses `0.12` degrees per pixel; stick/mobile orbit uses `150` degrees per second with a `0.08` dead zone.
- Camera yaw assist: `0.18` target-facing blend at `2.2` assist speed. This keeps the camera from snapping to player rotation while still recentring slowly when the player is not dragging/looking.
- Camera cue profiles now affect bounded position offset, FOV, distance, and focus height:
  - Run start: `0.20s`, tiny forward-speed read, `+0.8` FOV, `-0.08m` distance.
  - Stop settle: `0.22s`, tiny pullback, `-0.8` FOV, `-0.12m` distance.
  - Sharp turn: `0.24s`, side/turn bias, `+0.6` FOV, `-0.06m` distance.
  - Dodge: `0.28s`, speed pullback/direction bias, `+2.2` FOV, `-0.20m` distance.
  - Attack start: `0.22s`, light forward push, `-1.2` FOV, `+0.12m` distance.
  - Attack hit: `0.18s`, tighter impact push, `-1.8` FOV, `+0.16m` distance.
- Attack camera cue finisher scale ramps across the five-hit chain instead of only distinguishing the third hit. This preserves a normal-action feel while making the later finisher read stronger.
- Successful normal attack damage does not change global `Time.timeScale`. `DamageInfo.HitStopSeconds` remains a reserved data hint, but V1 does not apply normal-hit global slow motion; time-slow belongs to a later explicit perfect-dodge, counter, ultimate, or authored cue bundle slice.
- Camera cue clamps: `0.55m` positional offset, `4` FOV delta, `0.45m` distance delta, and `0.25m` focus-height delta so normal action cues stay bounded and cannot become sticky cinematic locks.
- Visual materials: the CombatGirl pass uses copied primary albedo textures on `_Game` Unity Toon Shader materials. Original asset-pack masks, matcaps, and advanced shader behavior are intentionally not promoted in this slice.
- Basic soldier telegraph: `0.65s`, from the collected readable enemy telegraph range of `0.45-0.9s`.
- Basic soldier active: `0.14s`, from the collected active-window range of `0.04-0.45s`.
- Basic soldier recovery: `0.45s`, from the collected enemy recovery range of `0.35-1.0s`.
- Basic soldier hit reaction: `0.24s`, from the collected light stagger range of `0.18-0.35s`.
- Default camera cue duration: `0.24s`, from the collected camera-cue range of about `0.20-0.32s`. Stronger `0.30-0.68s` animation-state camera emphasis remains documented for later counter/assist/ultimate work, not this normal action pass.

## First-Pass Deviations

The collected references do not provide trustworthy numeric defaults for these values yet, so they are exposed in the Inspector:

- Player move speed, acceleration, deceleration, turn rate, and stop threshold.
- Dodge speed/distance.
- Soldier approach speed and knockback speed.
- Camera offset, follow damping, target/threat bias, and lead distance.
- Hit flash duration.

## Expected Result

- The player can move, start running through a short start clip, stop with a short settle request, perform a five-hit basic chain, dodge with invulnerability timing, take damage, and die.
- The CombatGirl visual is visible under the existing player root; gameplay remains on the root components.
- `CombatGirlWeaponSocketBinder` keeps the authored left/right weapon sockets aligned to the hand bones so generic humanoid stop clips cannot leave the sword or shield floating.
- Movement writes `MoveSpeed`, `MoveX`, `MoveY`, and `IsStopping`; run start triggers `StartRun`, release from run triggers `StopStep`, large running direction changes can trigger `TurnLeft90` / `TurnRight90`, dodge triggers `DodgeForward` / `DodgeBack` / `DodgeLeft` / `DodgeRight`, and basic attacks trigger `Attack1` through `Attack5` on the minimal Animator Controller.
- The soldier approaches, telegraphs, attacks, recovers, staggers when hit, and dies.
- Normal attack hits keep global time scale unchanged, so hit feedback cannot feel like an unexplained perfect-dodge slow-motion reward.
- The camera follows the player from a lower authored orbit, accepts manual orbit input, and biases focus toward the current threat without instantly rotating with player facing.
- `ActionCameraCueDriver` listens to movement start, stop-settle, sharp turn, dodge, basic attack start, and successful hit events, then asks `ActionCameraController` for short additive offset/FOV/distance/focus cues from presentation code.
- The player visibly changes tint during dodge movement, then returns to its authored material colors instead of staying in the old cyan placeholder tint.
- Win marker appears when the soldier dies.
- Fail marker appears when the player dies.
- The inspection scene must not render with pink/missing-shader materials.

## Verification Evidence

- `dotnet build C:\Git\DimensionBrawl\DimensionBrawl.slnx` passes with zero warnings and zero errors.
- Unity batchmode scene validation passes with `Action foundation test scene validation passed.` after closing the open Editor instance.
- Unity PlayMode smoke tests pass with `11` tests run, `11` passed, `0` failed for movement, promoted locomotion routing, stop-settle release, fast-window `StopStep` Animator routing, CombatGirl weapon socket alignment, directional dodge motion, no-input backward dodge, five-hit timed-buffer combo routing, attack damage without global slow motion, encounter win/fail, dodge tint feedback, short action-camera cue cleanup/FOV widening, independent camera orbit, and the lower close-rear camera preset.
- Batchmode PlayMode test command should omit `-quit`; `-runTests` exits on completion, while `-quit` can terminate before the Test Runner writes results.
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
- Unreviewed 6-7 hit extension, baseline parry, or card UI.
- Normal-hit global slow motion or hit-stop presentation. Reintroduce time-scale effects only through a reviewed perfect-dodge, counter, ultimate, or cue-bundle slice.
