# Action Foundation Testing

Last updated: 2026-06-13 KST

This note records how to inspect the first direct-control ARPG action-feel foundation without expanding the scope into summons, bosses, progression, or full HUD work.

## Test Setup

- Target scene: `Assets/_Game/Scenes/ActionFoundationTest.unity`.
- The scene contains the existing player gameplay root, one curated CombatGirl SwordShield visual child, promoted MaintenanceWorker sci-fi soldier samples for `ClosePunish`, `LungeStrike`, `HeavyWindup`, `LinePressure`, `FanPressure`, one `TrainingDeck` deck-driven sample, and six extended inspection samples for `RetreatShot`, `RetreatBlink`, `GuardBreak`, `GeneralDeck`, `EliteDeck`, and `EliteTraits`. It also keeps inactive soldier placeholder fallbacks, shared `CombatTargetSensor` wiring, `EnemyAttackTelegraphPresenter` markers, a camera, and win/fail markers.
- This is an authored inspection scene, not production runtime generation.
- The CombatGirl visual is promoted into `_Game` as game-owned model, sixteen selected animation clips, Animator Controller, primary textures, and Unity Toon Shader materials. It must not reference `_Imported/`.
- The basic soldier visual is promoted into `_Game` as a minimal game-owned MaintenanceWorker model/avatar, fourteen selected clips (`Idle`, `Run`, `Attack`, `AttackCombo2`, `AttackCombo3`, `CrouchForward`, `CrouchIdleCombat`, `RepairHigh`, `RepairLow`, `TypeOnConsole`, `Turn90Right`, `Hit`, `HitHeavy`, `Death`), and one Animator Controller. Gameplay timing remains owned by serialized `BasicSoldierEnemy` values; animation clips are presentation requests, not timing authority.
- Editor validation menu: `DimensionBrawl > Validate Action Foundation Test Scene`.
- The validator opens the test scene and checks required objects, ownership components, key references, shared target-sensor wiring, enemy telegraph presenter wiring, Animator wiring, weapon-socket binding, root-motion-off state, reference-backed timing values, twelve authored enemy target candidates, extended pattern deck samples, and elite trait profile wiring.
- If the open scene was already loaded before CombatGirl material promotion, `Assets > Refresh` may not update unpacked scene renderer slots. Use `DimensionBrawl > Reapply Action Foundation CombatGirl Materials` to reassign the open scene's CombatGirl renderers to `_Game` material assets and save the scene.
- If CombatGirl stop animations leave `add_weapon_l` or `add_weapon_r` drifting, use `DimensionBrawl > Reapply Action Foundation CombatGirl Weapon Sockets` to bind those sockets to `hand_l` and `hand_r` while preserving their authored offsets.
- If StopStep responsiveness drifts after reimport, use `DimensionBrawl > Reapply Action Foundation StopStep Responsiveness` to restore the trimmed StopStep import range, fast StopStep Animator transitions, and scene movement tuning.
- If the basic soldier visual or Animator wiring drifts after asset reimport, use `DimensionBrawl > Reapply Action Foundation MaintenanceWorker Enemy Visual` to rebuild the promoted soldier visual, selected enemy animation clips, Animator Controller, scene references, and hit-feedback renderer list.
- Extended enemy pattern data menu: `DimensionBrawl > Reapply Action Foundation Extended Enemy Patterns`.
- The extended enemy pattern menu creates reusable profile/deck assets for `RetreatShot`, `RetreatBlink`, `GuardBreak`, `ShieldCycle`, `ArmorBreak`, `AuraBuffer`, `SummonPackage`, and `PhaseSwap`, then refreshes the six extended authored scene samples and the player's twelve-candidate target list. These are data/runtime foundation assets first; they are not a production encounter spawner and do not promote additional imported model/animation sets by themselves.
- Enemy role catalog menu: `DimensionBrawl > Reapply Action Foundation Enemy Role Decks`.
- The enemy role catalog menu creates linear-run role profiles under `Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyRoles/`. These profiles combine existing pattern decks and elite trait data into general/elite soldier roles for `EntryRead`, `BreakGate`, `Backline`, `PressureRescue`, `BossBreakHandoff`, and `FinalStand`. They are planning/authoring data for future prefab/model swaps, not runtime wave spawning.
- Enemy archetype catalog menu: `DimensionBrawl > Reapply Action Foundation Enemy Archetypes`.
- The enemy archetype catalog menu creates role-to-presentation candidate profiles under `Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/`. These profiles map current soldier roles to promoted MaintenanceWorker visual candidates, record FORGE3D turret promotion candidates, and keep the future dragon boss candidate outside soldier role decks. They are data-only mapping assets, not prefab builders, package importers, runtime spawners, or wave logic.
- Enemy prefab candidate menu: `DimensionBrawl > Reapply Action Foundation Enemy Prefab Candidates`.
- The enemy prefab candidate menu promotes the reviewed `ClosePunish` sci-fi melee soldier scene sample into `Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab`, clears scene target/camera references, keeps telegraph/VFX references local to the prefab, and maps `SciFiSoldier.Melee` to that prefab candidate. It is an authored prefab review step, not a runtime prefab builder or encounter spawner.
- Enemy prefab review scene: `Assets/_Game/Scenes/ActionFoundationEnemyPrefabReview.unity`.
- The review scene keeps the existing player/camera/test arena setup, removes the twelve multi-pattern sample enemies, and places one prefab instance named `EnemyPrefabReview_SciFiSoldier_Melee`. This is the manual Play-mode check for the reusable prefab candidate; the scene injects player/enemy target candidates and camera bindings explicitly.
- PlayMode smoke tests: `Assets/_Game/Tests/PlayMode/ActionFoundationPlayModeTests.cs`.
- The smoke tests load the scene, move the player, verify promoted `StartRun`, fast-window `StopStep`, and 90-degree turn Animator routing, verify CombatGirl weapon sockets stay hand-pinned, trigger directional dodge, verify no-input backward dodge, verify dodge tint feedback, verify short action camera cue cleanup and FOV widening, verify that camera orbit is independent from instant player facing changes, verify the lower close-rear camera preset, verify a five-hit basic combo can reach `Attack5` from timed buffered input, verify attack damage without global slow motion, verify shared enemy/future-summon team targeting, verify the twelve authored player target candidates, verify promoted soldier visual/Animator wiring, verify enemy Attack/Hit/Death animation requests, verify pattern-specific prepare/attack/hit trigger data, verify enemy windup/active telegraph readability, verify `LinePressure` lane-lock side-dodge behavior, verify authored `TrainingDeck` selection and `FanPressure` cone hit behavior, verify extended general/elite pattern assets and scene samples, verify enemy role profiles cover the linear run segments, verify enemy archetype profiles map role intent to promoted or promotion-pending presentation candidates without raw `_Imported` prefab references, verify the melee soldier prefab candidate is scene-free and mapped to `SciFiSoldier.Melee`, verify the prefab review scene wires one prefab instance for manual combat, verify elite signal animation trigger data, verify shared elite damage-modification behavior, and verify win/fail encounter states.

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
- Basic soldier readable telegraph presentation: windup scale grows from `(0.35, 0.02, 0.65)` to `(1.05, 0.02, 1.55)`, active flash starts at `(1.25, 0.025, 1.8)`, and the promoted soldier visual briefly offsets back `0.08m` during windup then forward `0.12m` on release. This is a presentation bridge around the minimal promoted enemy clips, not a damage or AI authority.
- Pattern sample profiles now own attack shape data as well as timing/camera/telegraph data: `ClosePunish` and `HeavyWindup` use `MeleeArc`, `LungeStrike` and `LinePressure` use `ForwardLine`, and `FanPressure` uses `ForwardFan`. `LinePressure` uses a `6.2m` forward strip, `0.38m` half width, `0.78s` telegraph, `0.18s` active window, and locks its attack direction when windup starts so sideways dodge timing remains readable instead of becoming auto-tracking damage.
- `FanPressure` follows the collected `Windup -> FanShot -> Recover` reference shape with a `4.8m` range, `30` degree half-angle cone, `0.72s` telegraph, `0.20s` active window, locked windup direction, wider teal warning marker, and `FanPressure` camera cue. The cone hit check is runtime behavior data from `CombatAiPatternProfile`, not a pattern-id branch.
- `RetreatShot` follows the collected `Backstep -> Aim -> Shot -> Recenter` reference shape with a `0.32s` pre-attack retreat, `4.6m/s` retreat speed, `5.4m` forward-line range, `0.62s` telegraph, `0.12s` active window, and locked shot direction after the retreat setup.
- `RetreatBlink` is the faster support/reposition variant with a `0.18s` pre-attack retreat, `8.5m/s` retreat speed, `4.4m` forward-line range, `0.52s` telegraph, and lower deck priority so it reads as a spacing reset instead of replacing core attacks.
- `GuardBreak` is authored as an elite soldier attack profile, not an AI branch: `2.05m` melee arc, `42` degree half-angle, `1.12s` telegraph, `0.18s` active window, `0.92s` recovery, and a stronger guard-break camera cue.
- `DB_BasicSoldier_TrainingDeck` is the first multi-pattern deck for a single soldier type. It chooses among `ClosePunish` (`0-1.9m`, `0.55s` cooldown, priority `4`), `LungeStrike` (`1.2-3.1m`, `0.85s`, priority `3`), `FanPressure` (`2.0-5.1m`, `1.0s`, priority `2.5`), and `LinePressure` (`3.2-6.8m`, `1.1s`, priority `2`) using distance, cooldown, and priority data. The authored `Enemy_SciFiSoldier_TrainingDeck` scene sample starts from `ClosePunish`, owns this deck, and exposes the active deck entry index for tests/debug readability. Once a deck row is selected during approach, the soldier keeps it while the target remains inside that row's distance band so a committed `LungeStrike` cannot collapse into `ClosePunish` just because the soldier stepped into a closer overlap.
- `DB_BasicSoldier_GeneralPatternDeck` expands the general soldier vocabulary to six rows: `ClosePunish`, `LungeStrike`, `RetreatShot`, `FanPressure`, `LinePressure`, and `RetreatBlink`. The original `TrainingDeck` remains smaller and stable for focused scene inspection.
- `DB_EliteSoldier_PatternDeck` combines `ClosePunish`, `GuardBreak`, `HeavyWindup`, `FanPressure`, `LinePressure`, and `RetreatShot`. `DB_EliteSoldier_PhaseTwoPatternDeck` is the data target for `PhaseSwap` and favors `GuardBreak`, `HeavyWindup`, `RetreatBlink`, `FanPressure`, and `LinePressure`.
- Elite trait profiles are data assets consumed by `EnemyElitePatternController`: `ShieldCycle` owns a `70` guard meter and `0.35x` damage intake until break, `ArmorBreak` owns a `120` guard meter and `0.55x` damage intake until one-time break, `AuraBuffer` is a persistent priority signal that can protect explicitly authored receiver targets at `0.85x` damage intake, `SummonPackage` is a health-gated summon-call signal at `0.75` health ratio that can activate pre-authored signal objects, and `PhaseSwap` swaps to the phase-two deck/profile at `0.5` health ratio.
- Linear run role catalog: 7 general roles (`EntryProbe`, `CloseGuard`, `LungeChaser`, `LineCaster`, `FanSuppressor`, `BacklineShooter`, `Skirmisher`) and 5 elite roles (`ShieldBreaker`, `AuraCaptain`, `SummonCaller`, `PhaseDuelist`, `FinalStandCommander`) combine the current pattern profiles/decks into the collected 3-5 minute run beats: Entry Read, Break Gate, Backline, Pressure Rescue, Boss Break Handoff, and Final Stand. These roles are intentionally data-only so future enemy prefabs, summon AI reuse, and model/animation swaps can share the same combat grammar.
- Enemy archetype catalog: 3 mobile soldier archetypes (`SciFiSoldier.Melee`, `SciFiSoldier.Ranged`, `SciFiSoldier.Elite`), 2 static turret candidates (`Forge3D.LineTurret`, `Forge3D.MissileTurret`), and 1 future boss candidate (`DragonBoss.Future`) map role intent to presentation candidates without changing role deck behavior. Only promoted `_Game` object references are allowed; raw FORGE3D and dragon source paths are recorded as text promotion notes until a reviewed prefab slice promotes selected parts.
- First melee soldier prefab candidate: `PF_Enemy_SciFiSoldier_Melee_ClosePunish` is authored under `_Game/Prefabs/Enemies/ActionFoundation/`, starts from the reviewed `ClosePunish` profile, keeps target candidates empty, keeps its telegraph and VFX pool local, and is assigned to the `SciFiSoldier.Melee` archetype gameplay prefab slot.
- First melee soldier review scene: `ActionFoundationEnemyPrefabReview.unity` places one prefab instance at the front of the player, wires the prefab target sensor to the player, wires the player target selector to that enemy, and gives the enemy instance the scene camera controller for windup cues.
- Basic soldier promoted animation set: `Idle`, `Run`, `Attack`, `AttackCombo2`, `AttackCombo3`, `CrouchForward`, `CrouchIdleCombat`, `RepairHigh`, `RepairLow`, `TypeOnConsole`, `Turn90Right`, `Hit`, `HitHeavy`, and `Death`. The selected clips follow the collected enemy taxonomy (`Advance`, `Windup`, `Attack`, `Recover`, `HitReact`, `Stagger`, `Death`, `SummonCall`/assist-like signals) without promoting the full source pack.
- Pattern animation mapping is data-owned, not pattern-id code:
  - `ClosePunish`: `Attack`, `Hit`, `Death`.
  - `LungeStrike`: `AttackCombo2`, `Hit`, `Death`.
  - `HeavyWindup`: `AttackHeavy` using the promoted three-hit combo clip at slower state speed, `HitHeavy`, `Death`.
  - `LinePressure`: `AttackLinePressure` using `RepairHigh` as an authored aim/line-pressure read.
  - `FanPressure`: `AttackFanPressure` using `RepairLow` as an authored wider pressure read.
  - `RetreatShot`: prepare `RetreatBackstep` using `CrouchForward`, then `AttackRetreatShot`.
  - `RetreatBlink`: prepare `RetreatBlink` using faster `CrouchForward`, then `AttackRetreatBlink`.
  - `GuardBreak`: `AttackGuardBreak` using the promoted three-hit combo clip at heavier state speed, plus `HitHeavy`.
  - Elite signals: `EliteShieldCycle` uses `CrouchIdleCombat`, `EliteArmorBreak` uses `HitHeavy`, `EliteAuraBuffer` uses `RepairLow`, `EliteSummonPackage` uses `TypeOnConsole`, and `ElitePhaseSwap` uses `Turn90Right`.
- These enemy animation matches are foundation-grade reads, not final content lock. `RepairHigh`, `RepairLow`, and `TypeOnConsole` are readable local substitutes for ranged/command/buff signals until a dedicated ranged soldier, caster, or commander animation slice is reviewed.
- Basic soldier `Death` clip uses feet-based Y import (`heightFromFeet = true`, `keepOriginalPositionY = false`) so the final pose settles on the ground instead of preserving the source root height in midair.
- Basic soldier identity: `enemyTypeId = SciFiSoldier.Basic` and `patternId = ClosePunish`, matching the reference enemy-pattern deck shape `Track -> Windup -> MeleeBurst -> Recover`.
- Shared target sensor: one authored player-health candidate, search radius `12m`, and retarget interval `0.2s`, kept serialized so enemy types and future summon roles can tune sensing per prefab without adding broad managers or scene-wide target searches.
- Team rules: `Player` and future `AllySummon` actors are allied; `Enemy` actors are hostile to both. Neutral actors are not valid combat targets.
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
- The soldier samples approach, telegraph, attack, recover, stagger when hit, and die. The `LinePressure` and `FanPressure` samples reuse the same soldier execution code but read their attack shape, windup lock, telegraph, and camera cue data from `CombatAiPatternProfile`.
- A deck-configured soldier can switch among several `CombatAiPatternProfile` assets through `CombatAiPatternDeck` distance/cooldown/priority entries without scene searches, runtime asset loading, or hardcoded pattern ids. The inspection scene includes authored `TrainingDeck`, `GeneralDeck`, `EliteDeck`, and `EliteTraits` samples so this behavior can be watched without temporarily mutating the primary `ClosePunish` sample.
- `CombatEnemyRoleProfile` assets define general/elite soldier roles for the linear run without adding a wave spawner. They identify where a monster belongs in the run, which pattern deck it starts from, and which future player/summon answer it is meant to teach.
- `CombatEnemyArchetypeProfile` assets map those roles to current presentation candidates without adding a wave spawner or prefab builder. The current melee/ranged/elite soldier entries can point at the promoted MaintenanceWorker visual, while FORGE3D turret and dragon entries stay promotion-pending until reviewed `_Game` prefabs exist.
- The first melee soldier prefab candidate can be reviewed as a reusable authored prefab without carrying ActionFoundationTest player targets or camera references. Future scenes/encounters should inject target candidates and camera bindings explicitly.
- `ActionFoundationEnemyPrefabReview.unity` is the intended manual test scene for this prefab: press Play there to check approach, telegraph, hit reaction, death, player targeting, enemy targeting, and enemy camera cues without the twelve-pattern inspection lineup.
- A profile can request a pre-attack reposition phase through `prepareSeconds` and `prepareRetreatSpeed`; `BasicSoldierEnemy` reports this as `CombatAiPatternState.Repositioning` before windup, so `RetreatShot` and `RetreatBlink` do not require pattern-id branches.
- `EnemyElitePatternController` consumes `CombatAiElitePatternProfile` assets for shield/armor/aura/summon-signal/phase behavior. It uses `CombatHealth.DamageModifying` for mitigation, guard-meter consumption, and explicitly authored aura receiver protection, so shield, armor, or aura behavior is not hardcoded into `CombatHealth` or `BasicSoldierEnemy`.
- The promoted MaintenanceWorker visual handles soldier `MoveSpeed`, pattern-specific prepare/attack triggers, light/heavy hit triggers, elite signal triggers, and `Death` requests while root movement, damage timing, and state transitions remain on `BasicSoldierEnemy` and `EnemyElitePatternController`.
- The soldier uses `CombatTargetSensor` for hostile acquisition from authored candidates instead of owning private-only target lookup or scene-wide searches, while its model, Animator Controller, animation trigger names, and pattern id remain prefab-level data.
- `EnemyAttackTelegraphPresenter` makes the soldier windup readable by growing the attack range marker, shifting color toward danger, and adding a small temporary promoted-visual windup/release offset.
- Normal attack hits keep global time scale unchanged, so hit feedback cannot feel like an unexplained perfect-dodge slow-motion reward.
- The camera follows the player from a lower authored orbit, accepts manual orbit input, and biases focus toward the current threat without instantly rotating with player facing.
- `ActionCameraCueDriver` listens to movement start, stop-settle, sharp turn, dodge, basic attack start, and successful hit events, then asks `ActionCameraController` for short additive offset/FOV/distance/focus cues from presentation code.
- The player visibly changes tint during dodge movement, then returns to its authored material colors instead of staying in the old cyan placeholder tint.
- VFX cue infrastructure now uses `DB_CombatVfxCues_ActionFoundation` plus promoted game-owned mesh cue prefabs under `_Game/Art/VFX/CombatCues`. The first useful cue set covers player attack start/hit, player dodge, enemy windup/active, enemy hit/death, authored soldier pattern cues, and elite signal cues without direct raw `_Imported` prefab references.
- Win marker appears when the soldier dies.
- Fail marker appears when the player dies.
- The inspection scene must not render with pink/missing-shader materials.

## Verification Evidence

- `dotnet build C:\Git\DimensionBrawl\DimensionBrawl.slnx -m:1` passes. If local raw VFX pack demo/editor warnings appear after Unity regenerates project files, treat them as third-party local-only noise until curated effects are promoted; project-owned action code should stay warning/error clean.
- Unity batchmode extended pattern setup passes through `DimensionBrawl.Editor.ActionFoundationEnemyPatternExpansionSetup.ReapplyExtendedEnemyPatternsMenu` and creates the extended general/elite pattern profile assets, six extended scene samples, and twelve authored player target candidates.
- Unity batchmode scene validation passes with `Action foundation test scene validation passed.` after closing the open Editor instance.
- Unity batchmode combat VFX validation passes with `Action foundation combat VFX cue validation passed.` and rejects missing cue ids, raw `_Imported` prefab refs, first-pass particle shards, and missing player/enemy cue-driver bindings.
- Unity batchmode enemy role validation passes with `Action foundation enemy role deck validation passed.` and rejects missing role profiles, empty role ids, missing starting patterns/decks, invalid deck distance bands, missing run segment coverage, and mixed-up general/elite trait ownership.
- Unity batchmode enemy archetype validation passes with `Action foundation enemy archetype validation passed.` and rejects missing archetype profiles, empty role mappings on participating entries, raw `_Imported` object references, missing static turret candidates, missing future boss candidate tracking, and static turret entries that are not marked for dedicated prefab promotion.
- Unity batchmode enemy prefab candidate validation passes with `ActionFoundation enemy prefab candidate validation passed.` and rejects missing prefab assets, scene target candidates, serialized scene camera controllers, raw `_Imported` object references, and external scene references inside the prefab.
- Unity batchmode enemy prefab review scene validation passes with `ActionFoundation enemy prefab review scene validation passed.` and rejects scenes that have more than one soldier, lose the prefab instance connection, miss player/enemy target candidates, or omit the scene camera binding.
- Unity PlayMode smoke tests pass with `37` tests run, `37` passed, `0` failed for movement, promoted locomotion routing, stop-settle release, fast-window `StopStep` Animator routing, CombatGirl weapon socket alignment, directional dodge motion, no-input backward dodge, five-hit timed-buffer combo routing, attack damage without global slow motion, shared target sensing/team rules, promoted MaintenanceWorker enemy visual/Animator wiring, pattern-specific enemy animation trigger data, enemy Attack/Hit/Death animation requests including fatal-damage direct death routing and grounded death bounds, enemy windup telegraph presentation, `LinePressure` lane-lock side-dodge behavior, authored `TrainingDeck` row commitment, selection, `FanPressure` cone hit behavior, extended general/elite pattern data, linear-run enemy role profile coverage, role-to-archetype presentation candidate coverage, scene-free melee soldier prefab candidate mapping, prefab review scene wiring, elite signal animation trigger data, shared elite damage-modification behavior, authored aura receiver protection, authored summon signal activation, encounter win/fail, dodge tint feedback, short action-camera cue cleanup/FOV widening, independent camera orbit, and the lower close-rear camera preset.
- Batchmode PlayMode test command should omit `-quit`; `-runTests` exits on completion, while `-quit` can terminate before the Test Runner writes results.
- Manual Editor inspection confirmed `Left Shift` dodge makes the player placeholder flash/tint visibly.
- Manual Editor inspection confirmed the CombatGirl visual is attached to the player root and its Unity Toon Shader material colors render correctly after reapplying scene renderer slots.
- If the CombatGirl visual returns to pink after refresh, run `DimensionBrawl > Reapply Action Foundation CombatGirl Materials`, then validate the scene again.
- Manual subjective feel review in the open Unity Editor is still useful for tuning speed, acceleration, dodge distance, and camera offset.
- Visual inspection in the open Unity Editor should confirm the test scene uses `_Game` URP materials instead of the built-in default material.

## Deliberately Not Included

- Actual summon behavior.
- Summon spawning, slots, cooldowns, AI, UI, or role execution.
- `SummonPackage` runtime spawning. The current profile can only activate pre-authored signal objects to avoid adding unreviewed runtime instantiation.
- Scene-search-based aura application. `AuraBuffer` only protects explicitly authored receiver targets; squad/encounter wiring needs a separate reviewed slice.
- Boss phases.
- Progression, currencies, rewards, or full stage loop.
- Full mobile HUD.
- Runtime scene generation for production play.
- Imported asset mass promotion.
- Direct references to raw FORGE3D or dragon source prefabs from role/archetype data.
- Original asset-pack matcap and mask behavior beyond the copied primary texture color pass.
- Unreviewed 6-7 hit extension, baseline parry, or card UI.
- Normal-hit global slow motion or hit-stop presentation. Reintroduce time-scale effects only through a reviewed perfect-dodge, counter, ultimate, or cue-bundle slice.
