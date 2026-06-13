# Current State

Last updated: 2026-06-13 KST

## Repository

- Git repository is initialized at `C:\Git\DimensionBrawl`.
- Main branch is `main`.
- Raw imported asset packs are local-only under `Assets/_Imported/`.
- Game-owned work should live under `Assets/_Game/`.
- Android is the mobile-first build baseline. The shared Android Build Profile lives at `Assets/Settings/Build Profiles/Android.asset`, uses the Android target, and should be cherry-picked by UI branches before mobile HUD or scene-flow work.

## Committed Baseline

- Unity project baseline exists.
- Combat reference docs and datasets are imported.
- `COMBAT_V1_SPEC.md` defines the first direct-control ARPG action slice before summon implementation.
- `ACTION_FEEL_TARGETS.md` defines the movement, camera, attack, dodge, hit, and enemy feel quality targets that action work must improve.
- `ACTION_FOUNDATION_OWNERSHIP.md` defines the narrow runtime ownership split for the first action-feel implementation.
- `ACTION_FOUNDATION_TESTING.md` records the first test setup, control map, reference-backed values, first-pass deviations, and deliberate exclusions.
- `Assets/_Game/Scenes/ActionFoundationTest.unity` exists as the first authored action-feel inspection scene.
- `ActionFoundationTest.unity` uses game-owned URP test materials under `Assets/_Game/Art/Materials/ActionFoundation/` so the inspection scene does not rely on Unity's built-in default material or render pink/missing-shader placeholders.
- The player inspection placeholder has `PlayerDodgeFeedback`, a presentation-only dodge tint driven by `PlayerActionController` dodge events, so the dodge window is visible even before real player animations are wired.
- `ActionFoundationTest.unity` now keeps `Player_CombatGirl_ActionFoundation` as the gameplay root and mounts a curated `CombatGirlSwordShield_PlayerVisual` child under that root; movement, combat, health, and feedback ownership remain on the existing root components.
- The CombatGirl visual slice promotes only selected game-owned assets into `_Game`: one model/avatar, sixteen curated clips including `SS_StartRun.fbx`, `SS_StopStep.fbx`, `SS_TurnLeft90.fbx`, `SS_TurnRight90.fbx`, `SS_DodgeForward.fbx`, `SS_DodgeBack.fbx`, `SS_DodgeLeft.fbx`, `SS_DodgeRight.fbx`, and `SS_Attack1` through `SS_Attack5`, one Animator Controller, primary albedo textures, and Unity Toon Shader materials. It should not reference `_Imported/`.
- `CombatGirlWeaponSocketBinder` is attached to `CombatGirlSwordShield_PlayerVisual` and pins `add_weapon_l` / `add_weapon_r` to `hand_l` / `hand_r` with captured offsets, so the One Hand Base StopStep candidate cannot leave the sword or shield floating.
- The old capsule/sword proxy objects remain inactive fallback/reference objects in the scene. The active player hit/dodge feedback now targets CombatGirl renderers, and the old cyan idle tint is disabled so authored material colors can show between hit/dodge flashes.
- `ActionFoundationTest.unity` now has an `ActionCameraCueDriver` on the action camera. It listens to player run start, stop-settle, sharp turn, dodge, basic attack start, and successful hit events, then requests short bounded additive offset/FOV/distance/focus cues from `ActionCameraController`.
- `ActionCameraController` now uses a lower landscape ARPG third-person orbit preset instead of deriving the camera position from instant player rotation. Right mouse drag, gamepad right stick, or the mobile orbit hook can rotate the camera; player-facing assist recenters slowly instead of hard-snapping.
- `PlayerMovementController` keeps the current input response but now uses a promoted `StartRun` trigger, `0.26s` stop-settle, `0.16s` final-input hold, `0.06s` Animator move damping, a `0.24` stop-settle MoveSpeed floor, a `StopStep` trigger, and promoted 90-degree turn triggers for large running direction changes.
- `SS_StopStep` uses the reviewed One Hand Base run-to-idle candidate trimmed to source frame `4`, Animator speed `1.45`, and `0.015s` no-exit-time StopStep transitions so key release reads sooner.
- `PlayerActionController` now supports a five-hit basic chain using promoted `Attack1` through `Attack5` animation requests. The chain remains a single basic attack route with explicit startup, active, recovery, buffer, dodge-cancel, damage, hit-radius, hit-distance, and reserved hit-stop hint values for each hit, with a `0.10s` queue-open gate and `45%` recovery-chain point so later hits do not feel like buffered input drops after hit 3.
- `PlayerActionController` dodge now routes through directional CombatGirl Quickshift triggers (`DodgeForward`, `DodgeBack`, `DodgeLeft`, `DodgeRight`) and `PlayerMovementController` decays the dodge burst over the `0.56s` movement window so it reads more like a lunge/roll than a constant slide. Held movement input dodges along the current move direction; no-input dodge moves backward from the current facing direction.
- `PlayerActionController` attack overlap ignores colliders under the player root and resolves `CombatHealth` through parent objects, so authored CombatGirl body/weapon colliders do not block soldier hit validation.
- `CombatHealth` now includes `AllySummon` as a future player-side team and routes friendly-fire/hostile checks through `CombatTeamUtility`, so the first enemy and later summon actors can share one small team contract.
- `CombatHitFeedback` no longer applies normal-hit global slow motion or hit-stop. It handles damage flash and death color only; time-scale effects are reserved for a later explicit perfect-dodge, counter, ultimate, or authored cue bundle slice.
- `ActionFoundationTest.unity` now gives `Enemy_SciFiSoldier_ActionFoundation` a `CombatTargetSensor` with one authored player-health candidate, serialized search radius `12m`, and retarget interval `0.2s`; the sensor evaluates supplied candidates instead of scene-wide searches and does not own summon spawning, slots, cooldowns, or UI.
- `BasicSoldierEnemy` now declares serialized `enemyTypeId = SciFiSoldier.Basic` and `patternId = ClosePunish`, while model, Animator Controller, trigger names, and future pattern variants remain prefab-level data instead of hardcoded asset paths.
- `EnemyAttackTelegraphPresenter` is attached to the basic soldier as presentation-only readability: it grows the attack marker through windup, flashes larger on active release, shifts warning color, and temporarily offsets the placeholder body so the soldier attack is readable before real enemy animations are promoted.
- `Enemy_SciFiSoldier_ActionFoundation` now uses a promoted game-owned MaintenanceWorker visual under `_Game`, with a minimal Animator Controller and selected `Idle`, `Run`, `Attack`, `Hit`, and `Death` clips. The old capsule placeholder remains inactive as fallback/reference only.
- The basic soldier still owns timing and state through serialized `BasicSoldierEnemy` values; the promoted enemy Animator is presentation-only and receives `MoveSpeed`, `Attack`, `Hit`, and `Death` requests without hardcoded runtime asset paths.
- The promoted MaintenanceWorker `Death` clip uses feet-based Y import so the death pose settles near the controller base instead of floating at the source clip root height.
- `PlayerMovementController` no longer consumes gamepad right-stick fallback for character facing; right-stick fallback now belongs to camera orbit unless an explicit look action/mobile look hook is wired later.
- `ProjectSettings/EditorSettings.asset` intentionally has Enter Play Mode options enabled with option value `3` to reduce repeated Editor play-mode reload friction during action-feel testing.
- Android PlayerSettings are the current cross-PC baseline: company `dharmaflash`, package `com.dharmaflash.dimensionbrawl`, landscape-left/right only, URP scripting define, IL2CPP scripting backend, and ARM64 architecture. PC/Standalone remains useful as a development target, but mobile layout, safe area, and touch actions are the default product assumptions.
- UI V1 branch work now uses Android/mobile-first landscape assumptions: UI test scenes are authored under `Assets/_Game/Scenes/UI/`, the shared scene shell uses a `2400x1080` landscape Canvas Scaler reference, content is parented under an authored Safe Area root, and EventSystems use `InputSystemUIInputModule`.
- `UI_LobbyTest` now uses the first authored lobby PSD/PNG layer export under `Assets/_Game/UI/Lobby/Source/` and `Assets/_Game/UI/Lobby/Art/`. `PF_UI_LobbyScreen` contains an inspectable `LobbyArtRoot` with a full-bleed `LobbyBackgroundRoot`, a 16:9 `LobbyDesignFrame` UI layer stack, and a transparent `StoryPveButton` hit area over the main PvE card; currency, shop, storage, mail, event, and progression visuals remain static UI placeholders only.
- UI V1 display data includes the canonical combat HUD action vocabulary from `COMBAT_V1_SPEC.md`, including `Move`, `Look`, `TargetBias`, `BasicAttack`, `Dodge`, `Skill1`, `Ultimate`, `SummonSlot1`, `SummonSlot2`, `SummonSlot3`, and `Pause`, with summon slots still visual placeholders only.
- `ProjectSettings/EditorBuildSettings.asset` intentionally lists only the UI V1 test route scenes for the current contest/test build handoff: `UI_LoginTest`, `UI_LobbyTest`, `UI_StageSelectTest`, and `UI_CombatHudTest`, starting at login and returning to lobby through the UI route table.
- `DimensionBrawl > Validate Action Foundation Test Scene` validates required scene objects, component ownership, key references, shared target sensor wiring, promoted MaintenanceWorker enemy visual/Animator wiring, and reference-backed timing values from inside the Unity Editor.
- `DimensionBrawl > Validate Action Foundation Test Scene` also validates player Animator wiring, root-motion-off state, promoted locomotion states, fast StopStep transition tuning, CombatGirl weapon socket binding, directional Quickshift dodge states, promoted five-hit attack states, game-owned clip paths, and the serialized action/camera timing values for the curated visual child.
- `DimensionBrawl > Reapply Action Foundation CombatGirl Materials` exists for the open-Editor stale-scene case: unpacked CombatGirl renderers can keep old material slots after `Assets > Refresh`, so this menu reassigns the open scene to `_Game` CombatGirl material assets and saves it.
- `DimensionBrawl > Reapply Action Foundation CombatGirl Weapon Sockets` restores hand-pinned `add_weapon_l` / `add_weapon_r` bindings, and `DimensionBrawl > Reapply Action Foundation StopStep Responsiveness` restores StopStep trim, speed, transition, and scene movement tuning.
- `DimensionBrawl > Reapply Action Foundation MaintenanceWorker Enemy Visual` rebuilds the minimal promoted soldier visual, Animator Controller, scene references, disabled-placeholder state, and enemy hit-feedback renderer list if reimport or open-scene edits drift.
- Unity batchmode validation passes with `Action foundation test scene validation passed.` after closing the open Editor instance.
- `ActionFoundationPlayModeTests` passes `17` PlayMode tests for player movement, run-start/turn animation routing, stop-settle release, fast-window `StopStep` Animator routing, CombatGirl weapon socket alignment, directional dodge movement, no-input backward dodge, five-hit timed-buffer combo routing, basic attack damage without global slow motion, shared target sensing/team rules, promoted MaintenanceWorker enemy visual/Animator wiring, enemy Attack/Hit/Death animation requests including fatal-damage direct death routing and grounded death bounds, enemy windup telegraph presentation, win/fail states, dodge tint feedback, short action-camera cue cleanup/FOV widening, independent camera orbit, and the lower close-rear camera preset. Batchmode PlayMode test runs should omit `-quit` so `-runTests` can write results before Unity exits.
- Manual open-Editor inspection confirmed the player placeholder visibly flashes/tints when dodging with `Left Shift`.
- Raw imported asset packs are ignored.
- `_Game/Art` folder structure exists for curated game-ready assets.
- Project governance docs now define AI limits, folder ownership, code style, architecture boundaries, session workflow, and review checks.

## Local Assets

Raw packs currently staged only as local files:

- `Assets/_Imported/AssetStore/CombatGirlsCharacterPack`
- `Assets/_Imported/AssetStore/Animation`
- `Assets/_Imported/AssetStore/Protofactor`
- `Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3`

These must not be committed directly.

## Next Safe Step

Use `Assets/_Game/DesignDocs/COMBAT_V1_SPEC.md`, `Assets/_Game/DesignDocs/ACTION_FEEL_TARGETS.md`, `Assets/_Game/DesignDocs/ACTION_FOUNDATION_OWNERSHIP.md`, and `Assets/_Game/DesignDocs/ACTION_FOUNDATION_TESTING.md` as implementation guardrails. The next safe step is to keep the one-enemy scope and manually inspect the basic soldier's approach, telegraph, attack timing, hit reaction, death, and target sensing against the current player action slice before adding summon behavior, waves, bosses, or progression.

## Current Risk

- Unity may recreate project setting folders while packages are inspected. Do not commit package/project setting changes unless they are intentionally reviewed.
- Unity may briefly report stale or malformed `.meta` data while files are being promoted. Verify the final `.meta` GUIDs and refresh/reimport in the open Editor before assuming the asset files themselves are broken.
- If the already-open `ActionFoundationTest` scene shows a pink CombatGirl after material promotion, run `DimensionBrawl > Reapply Action Foundation CombatGirl Materials`; refresh alone does not always update unpacked scene renderer material slots.
- Unity batchmode reruns may be blocked by the Unity Licensing Client/headless editor state while Editor instances are open; use the open Editor validation menu for quick checks, or close Unity before a full batchmode rerun.
- The project direction is direct-control ARPG first, summon system second. Do not start by building summon behavior, boss phases, progression, or a full mobile UI shell before the player action loop is playable.
- Action feel is the first quality gate. Do not add larger systems to hide weak movement, camera, attack, dodge, or hit feedback.
- The current CombatGirl material pass is primary-color recovery only. Do not mass-promote original matcaps, masks, or the full asset-pack material stack without a separate reviewed slice.
- Current action camera cues are intentionally small normal-action cues. Do not expand them into boss, summon, ultimate, or full lock-on camera systems without a separate reviewed slice.
- The five-hit combo is the current reviewed chain. Do not wire `SS_Sp_Skill2`, `SS_Sp_Skill3`, or other special clips as hits 6-7 until manual visual review confirms they still read as normal basic-chain finishers.
- `StartRun`, `TurnLeft90`, and `TurnRight90` should stay on CombatGirl-owned clips with weapon attachment curves. The current `SS_StopStep` intentionally uses the reviewed One Hand Base `Run_A_F_To_Idle_InPlace` candidate for stronger stop weight transfer; sword/shield stability is handled by `CombatGirlWeaponSocketBinder`, but manual visual inspection is still required before treating the stop feel as final.
- Do not reintroduce normal-hit global slow motion from the reserved hit-stop data. Time-scale effects should wait for a reviewed perfect-dodge, counter, ultimate, or cue-bundle implementation.
