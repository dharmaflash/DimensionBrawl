# Intro GatePod Invasion Extension Cinematic Plan

## Purpose

Extend the existing Intro GatePod cutscene from a compressed capsule awakening into a readable heaven-wide invasion sequence that ends in a natural third-person combat handoff.

This plan exists to prevent the old failure mode: placing soldiers directly from a short prompt or copying a weak second-by-second PGR-like table. The wide story situation is translated first into camera, actor, VFX, and timing beats, then implemented through the existing `IntroGatePod` runtime owners.

## Current Implemented Baseline

The current `DB_Cinematic_IntroGatePodAwakening` and `OlympusCorridorInvasionStage` already cover:

- Heaven is under attack.
- GatePods/capsules are visible.
- Voice and system cues imply soul synchronization below threshold.
- Inori wakes in first person.
- Inori looks down at her hands and scans outside the capsule.
- A compressed commando bridge, background explosion, and handoff beat already exist.

The extension should preserve those beats, but broaden the invasion before the final combat handoff.

## Source Priority

1. Existing project runtime ownership and scenes.
2. Current `OlympusCorridorInvasionStage` layout, props, lights, and GatePod placement.
3. Local Rina QuestStart camera rig package imported as reference under `_Imported/Reference/ZZZ_RinaLoopKit`.
4. ArkData evidence for timing density and shot grammar:
   - GFL2 `stage-skill-event-camera-animation-context.csv`: frame-indexed enemy/camera/effect rhythm.
   - Aether Gazer story presentation context: `time_`, `Vector3.Lerp`, main-camera-relative actor facing.
   - PGR true 3D command/camera rows and blendanimation curves: 3D actor/camera command grammar and 30 fps camera motion shape.
   - Ash Echoes cutscene/skill duration data: compact combat-cinematic duration ranges.
   - Arknights story camera command context: camera shake/focus/effect durations.
   - ZZZ timeline config: event spacing and large-scale timeline density.

## Reference Data Translation Rules

- GFL2 frame rhythm is used as density, not literal combat logic. A 100-frame enemy action can map to roughly 3.3 seconds at 30 fps, with major visual calls near frames 1, 25, 35, 47, 59, and 71.
- Aether story scripting is used for actor-to-camera facing and short transform interpolation rules.
- Rina QuestStart camera is used for the final character reveal and back-view handoff motion: foot/low-body start, upward read, face-safe framing, then gameplay back view.
- PGR camera curves inform small movement/shake shapes only; PGR is not a final cutscene source.
- Every shot must provide new information, show action, clarify threat direction, connect to weapon/system mechanics, or hand off to gameplay.

## Target Runtime Shape

The first implementation should stay inside narrow owners:

- `CinematicSequenceProfile` for the high-level cue plan.
- `IntroGatePodCinemachineShotPlayer` for authored shot switching.
- `IntroGatePodInvasionBridgeCue` or a sibling component for deterministic invasion sampling.
- Existing promoted sci-fi soldier prefabs and animation controllers.
- Existing Special Skills Effects Pack assets for jets, bombers, smoke, impact, rings, portals if suitable, and explosions.
- Existing Inori cinematic controller and face expression player for protagonist motion.

Do not hide encounter spawning, gameplay damage, or broad tutorial ownership inside the intro cutscene bridge.

## Beat And Shot Plan

| Beat | Time | Purpose | Camera | Actor/VFX Action | ArkData Basis |
| --- | ---: | --- | --- | --- | --- |
| 01 Capsule/system continuity | 0.0-12.8 | Preserve current awakening and hand scan | Existing first-person capsule shots | Existing voice/system/audio/capsule beats remain primary | Current project baseline |
| 02 Air raid establishes heaven-scale invasion | 12.8-16.6 | New information: heaven is being attacked beyond the room | Wide elevated exterior/corridor look, short hard cut after capsule scan | Jet/bomber pass overhead, distant impacts and smoke plumes, alarm sweep | Ash combat-cinematic 3-5s duration; Arknights 0.3-1.5s shake |
| 03 First portal opens in far zone | 16.6-19.2 | Threat direction | Reframe from wide blast to far portal lane | Ring/portal opens, light spill, first silhouettes visible, no clear faces | GFL2 frame 1 camera signal followed by frame 25 effect burst |
| 04 Multiple breach points | 19.2-22.4 | Spatial map of invasion | Lateral bridge pan or cut triptych across stage zones | Three portals/entry points activate on staggered offsets; soldiers emerge in groups | ZZZ 1.5/3.0s spacing; GFL2 repeated 25/35/47/59 effect cadence |
| 05 Soldiers open fire and impacts land | 22.4-25.6 | Action and threat | Low cover-height shot crossing muzzle fire and floor impacts | Ranged soldiers aim/fire, explosions and shield flashes across corridor | GFL2 OnEnemyAttack plus repeated effect rows; Arknights CameraShake median 0.5s |
| 06 Player body enters third person without face reveal | 25.6-28.6 | Character action | Low/side three-quarter, head cropped or hidden by shoulder/hair/backlight | Inori exits capsule space, kicks closest soldier, weapon still not glamour-framed | Aether actor lerp/facing; quality guardrail: action over body scan |
| 07 Sword pickup/guard line | 28.6-32.4 | Mechanic connection | Insert-to-medium reframe on floor sword and forward enemy line | Sword picked up from floor, rifle/hand weapon visibility reconciled, soldiers form line | Current floor sword; PGR actor-state-to-camera pair grammar |
| 08 Rina-style reveal to back view | 32.4-39.9 | Gameplay handoff | Rina QuestStart-derived foot/low-body rise, face-safe pass, settle to back view | Inori plants stance, face never becomes glamour focus, weapon points toward soldiers | Rina 7.5s 60fps camera rig; PGR blend curve smooth camera motion |
| 09 Gameplay handoff | 39.9-42.0 | Return control | Match existing action camera/FOV | HUD/input restore after final back-view alignment | Existing `GameplayHandoff` rules |

## Soldier Presentation Plan

Use soldiers as an invasion system, not as three decorative props.

- Far portal entry soldiers: silhouettes first, then synchronized step/aim loops.
- Midline ranged soldiers: use rifle idle/aim/shoot clips and muzzle VFX.
- Close guard soldier: receives Inori kick or stagger, then drops out of frame.
- Backline pressure soldiers: hold formation to define the gameplay target direction.

Frame-density target for a 3.2 second soldier breach:

- 0.00s: portal/camera signal.
- 0.80s: first burst/light/effect.
- 1.15s: first pair steps through.
- 1.55s: muzzle/scan pulse.
- 1.95s: second pair enters.
- 2.35s: impact/explosion.
- 2.75s: formation line readable.
- 3.20s: protagonist action interrupts or camera reframes.

This mirrors the GFL2 frame 1, 25, 35, 47, 59, 71 rhythm without copying game-specific content.

## Camera Notes

Rina package details:

- Source package: `Rina_QuestStart_CameraRig_Minimal_20260624 (1).unitypackage`.
- Imported reference root: `Assets/_Imported/Reference/ZZZ_RinaLoopKit`.
- Main rig: `Rina_QuestStart_MainCameraRig.prefab`.
- Clip: `Rina_QuestStart_OriginalExtracted.anim`.
- Clip rate/duration: 60 fps, approximately 7.5 seconds.
- Intended use: final reveal/back-view handoff reference, not broad runtime ownership.

The final implementation should either:

- instantiate the reference rig only in the authored scene as a cutscene camera source, or
- sample/extract equivalent shot poses into `_Game` cutscene profiles and keep the imported rig as evidence.

## First Implementation Checkpoints

1. Commit this plan and the imported Rina reference package.
2. Add editor/runtime support for the broadened invasion beats without changing unrelated P0/P1 modules.
3. Update `DB_Cinematic_IntroGatePodAwakening` duration and cue plan.
4. Update `OlympusCorridorInvasionStage` stage objects and bridge sampling for jets, portals, soldier groups, impacts, and Rina handoff.
5. Run Unity verification and capture review frames.
6. Commit the implemented cutscene pass only after verification is clean or the remaining limitation is clearly documented.
