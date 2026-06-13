# Cinemachine In-Game Cutscene Reference Research

Last updated: 2026-06-12 KST

## Baseline

This document is a Unity-side research handoff for `IsekaiBrawl` in-game character cutscenes, combat cut-ins, boss intros, summon entries, ultimate shots, break moments, final-kill bridges, dialogue staging, camera transitions, input locks, AI locks, time control, Timeline signals, impulse, and return-to-gameplay rules.

It was written after reading:

1. `PROJECT_BRIEF.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. `AGENTS.md`
5. `AI_CODE_CONTRACT.md`
6. `ARCHITECTURE_BOUNDARIES.md`
7. `HUD_COMBAT_SPEC.md`
8. Existing ARPG, UI, combo, summon, boss/run, stage/reward, and combat-feel reference docs
9. Local `BattleCamera`, `CameraShake`, `PlayerController`, and `StoryPveEncounterRuntime` cue hooks
10. Unity package docs and local package samples for Cinemachine, Timeline, and Animation Rigging

Installed local package baseline:

- `com.unity.cinemachine`: `3.1.5`
- `com.unity.timeline`: `1.8.10`
- `com.unity.animation.rigging`: `1.4.0`
- `com.unity.inputsystem`: `1.17.0`

Unity's online docs currently show later patch documentation in the same package streams (`Cinemachine 3.1.7`, `Timeline 1.8.12`). Treat online docs as package-stream reference, and local `Library/PackageCache` files as the exact installed reference when behavior differs.

## Scope Guard

This is a production seed catalog for Unity implementation, not a mandate to turn the game into long scripted cinematics.

Use it to answer:

- Which cutscene tier should handle a moment: raw `BattleCamera` cue, Cinemachine camera, Sequencer Camera, or Timeline?
- Which Cinemachine shot owns target, lens, body, aim, blend, impulse, and return behavior?
- Which Timeline signals fire VFX, SFX, hit pause, damage, UI pulse, animation state, input lock, AI lock, and cleanup?
- How does a boss intro, summon call, ultimate, break, final kill, or dialogue beat return to the active ARPG camera?
- Which data fields must be declared before a cutscene can be implemented safely?

Do not use it to reintroduce:

- long unskippable cutscenes inside the 3 to 5 minute run
- manual basic attack strings or extra active skill buttons
- hard target-selection UI
- lane-first or corridor-first camera assumptions
- Timeline clips with hidden scene bindings and no data contract
- ad hoc one-off camera scripts for every event
- cutscenes that leave input, AI, time scale, camera priority, or UI state dirty after playback

Current identity remains:

- `landscape / horizontal ARPG-facing Story PvE`
- `position + direction + dodge + summon timing`
- `summon-first battlefield swing`
- `one dodge button`
- `one ultimate`
- `one clear summon / assist action slot`
- short, readable, stylish cut-ins that support combat instead of replacing it

Asset/code ownership boundary: do not import proprietary animation clips, raw tables, meshes, textures, audio, camera files, or full source files from other games. Extract shot grammar, timing envelopes, schema shapes, production relationships, and Unity-authoring patterns.

## Evidence Correction: Reference Games vs Cinemachine

No public evidence was found that ZZZ, PGR, HI3, Wuthering Waves, Genshin, Tower of Fantasy, or the other reference titles expose their original Unity Cinemachine or Timeline assets.

In this document, `Cinemachine` means the `IsekaiBrawl` Unity implementation layer. Reference-game rows are not claims that those games use Cinemachine. They are:

- observable in-game camera, cut-in, hit-stop, time-slow, UI, VFX, SFX, and return-to-control patterns
- public wiki / official-help mechanic data used to size windows, costs, i-frames, readiness cues, and enemy-state gates
- public data-mirror file-name and field-shape evidence where available, used only as schema inspiration

Strongest newly confirmed data-mirror evidence:

- ZZZ public data mirror exposes ability JSONs with `CameraZoomAction`, `CameraStretchAction`, `CameraOverrideTrackAction`, `CameraOverrideTrackEndAction`, `CameraModifier`, `QTECameraModifier`, `ExQTECameraModifier`, `WitchSlowDownMixin`, screen effects, HP locks, UI hiding, protect time, and boss phase cutscene fields.
- PGR public data mirror exposes camera helper/table surfaces such as `XCameraHelper.lua`, `CameraParam.tab`, `VCameraLut.tab`, `SceneCamera.tab`, `CameraTrackLut.tab`, `CameraNoiseLut.tab`, `ScreenEffect.tab`, `Movie.tab`, and `Story.tab`.
- HI3 public data mirror exposes story/camera-adjacent tables such as `HybridSiteCamera.json`, `PlotData.json`, `RandomPlotData.json`, and many `Effect` / `StageData_Story` tables.

Therefore the correct claim is: public sources support a data-driven combat camera/cutscene contract for our Unity Cinemachine layer. They do not provide a lawful or direct Cinemachine asset import path.

## Source Inventory

| ID | Source | Type | Useful data | Confidence | URL / local path |
|---|---|---:|---|---:|---|
| `unity_cm_index` | Unity Cinemachine manual | Official docs | Cinemachine purpose, camera control, blending, Timeline compatibility | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/index.html |
| `unity_cm_camera` | Cinemachine Camera component | Official docs | Tracking target, Look At target, lens, Dutch, body/aim components | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCamera.html |
| `unity_cm_brain` | Cinemachine Brain component | Official docs | live camera selection, cut/blend, default/custom blends, event dispatch, ignore time scale | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBrain.html |
| `unity_cm_blending` | Cinemachine blending | Official docs | custom blends, ANY CAMERA fallback, named camera pairs, blend style/time | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBlending.html |
| `unity_cm_timeline` | Cinemachine and Timeline | Official docs/local package docs | shot clips, cuts, overlaps, track override order, Timeline returning control to Brain | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/concept-timeline.html |
| `unity_cm_sequencer` | Cinemachine Sequencer Camera | Local package docs | simple shot sequence without Timeline; child camera hold/blend/cut order | High | `Library/PackageCache/com.unity.cinemachine@*/Documentation~/CinemachineSequencerCamera.md` |
| `unity_cm_clearshot` | Cinemachine Clear Shot | Local package docs | automatic best child-camera selection, obstruction/quality-based fallback | Medium | `Library/PackageCache/com.unity.cinemachine@*/Documentation~/CinemachineClearShot.md` |
| `unity_cm_state_driven` | Cinemachine State-Driven Camera | Local package docs | Animator-state-to-camera mapping, wait/min duration, fallback | Medium | `Library/PackageCache/com.unity.cinemachine@*/Documentation~/CinemachineStateDrivenCamera.md` |
| `unity_cm_mixing` | Cinemachine Mixing Camera | Local package docs | weighted camera blending, Timeline-animatable weight slots | Medium | `Library/PackageCache/com.unity.cinemachine@*/Documentation~/CinemachineMixingCamera.md` |
| `unity_cm_target_group` | Cinemachine Target Group | Official docs | framing multiple targets through weight/radius, group center/average | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineTargetGroup.html |
| `unity_cm_third_person` | Third Person Follow | Official docs | shoulder offset, vertical arm, camera side, distance, collision | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineThirdPersonFollow.html |
| `unity_cm_rotation_composer` | Rotation Composer | Official docs | target screen position, damping, dead/soft zone, look-at composition | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineRotationComposer.html |
| `unity_cm_spline_dolly` | Spline Dolly | Official docs | authored fly-in/path shot, automatic dolly, normalized/distance/knot position | Medium | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSplineDolly.html |
| `unity_cm_impulse` | Cinemachine Impulse | Official docs | impulse source/listener, event-driven camera shake, channels, gain | High | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulse.html |
| `unity_timeline_index` | Unity Timeline manual | Official docs | cinematic/gameplay/audio/particle sequence authoring, asset and instance split | High | https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/index.html |
| `unity_timeline_asset_instance` | Timeline assets and instances | Official docs | reusable Timeline asset vs scene binding instance/PlayableDirector | High | https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/tl-overview.html |
| `unity_timeline_signals` | Timeline signals | Official docs | markers, Signal assets, Signal Receiver, frame-placed event dispatch | High | https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/wf-signals.html |
| `unity_timeline_control_clip` | Timeline Control clip | Local package docs | sub-timelines, particle systems, prefab instances, ITimeControl, post-playback state | High | `Library/PackageCache/com.unity.timeline@*/Documentation~/insp-clip-control.md` |
| `unity_timeline_activation` | Timeline Activation clip | Local package docs | GameObject activation windows and post-playback state | High | `Library/PackageCache/com.unity.timeline@*/Documentation~/insp-clip-act.md` |
| `unity_timeline_humanoid` | Animate a humanoid | Official docs | animation clip placement, matching offsets, blends, foot-slide cleanup | Medium | https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/wf-anim-human.html |
| `unity_timeline_override` | Animation Override track | Official docs | upper-body override through Avatar Mask while base track continues | Medium | https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/wf-anim-override.html |
| `unity_timeline_subtimeline` | Sub-Timelines | Official docs | nested Timeline orchestration through Control tracks | Medium | https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/wf-subtimeline.html |
| `unity_timeline_samples` | Gameplay Sequence Demo / Customization samples | Local package samples | camera switching, markers, activation, animation, audio, time dilation, annotation, custom tracks | High | `Library/PackageCache/com.unity.timeline@*/Samples~` |
| `unity_animation_rigging` | Animation Rigging manual | Official docs/local package docs | runtime constraints, IK, aim rigs for cutscene posing | Medium | https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/index.html |
| `zzz_dodge` | Zenless Zone Zero Dodge | Wiki | Perfect Dodge, Dodge Counter, invulnerability, enemy flash read, counter interruption | High | https://zenless-zone-zero.fandom.com/wiki/Dodge |
| `zzz_daze_chain` | Zenless Zone Zero Daze / Chain Attack | Wiki | Daze-to-stun, heavy-hit chain trigger, normal/elite/boss chain counts, Bangboo chain slot | High | https://zenless-zone-zero.fandom.com/wiki/Daze / https://zenless-zone-zero.fandom.com/wiki/Chain_Attack |
| `zzz_assist` | Zenless Zone Zero Assist | Wiki | Defensive Assist, assist parry, massive Daze, invulnerability, assist point pressure language | Medium | https://zenless-zone-zero.fandom.com/wiki/Assist |
| `zzz_decibels` | Zenless Zone Zero Decibels | Wiki | individual ultimate meter, 3000 default cap/cost, Perfect Dodge/Assist/Chain rewards, Decibel HUD text | Medium | https://zenless-zone-zero.fandom.com/wiki/Decibels |
| `pgr_matrix` | Punishing: Gray Raven Matrix | Wiki | EX Dodge time-slow pocket and countermeasure window | High | https://punishing-gray-raven.fandom.com/wiki/Matrix |
| `pgr_combat_guide` | Punishing: Gray Raven Combat Guide | Wiki | 3-Ping, QTE, Matrix-Ping, 15s Matrix cooldown, swap-in as 3-Ping, QTE invulnerability/heal overlap | High | https://punishing-gray-raven.fandom.com/wiki/Combat_Guide |
| `hi3_qte` | Honkai Impact 3rd QTE | Wiki | condition-based tag-in skill, UI highlight, airborne/frozen/stun/shieldbreak/time-slow triggers | High | https://honkaiimpact3.fandom.com/wiki/QTE |
| `hi3_time_fracture` | Honkai Impact 3rd Time Fracture | Wiki | time-slow triggered by battlesuit skills/evasion, QTE prerequisite, timer slowdown | Medium | https://honkaiimpact3.fandom.com/wiki/Time_Fracture |
| `wuwa_concerto_intro_outro` | Wuthering Waves Concerto / Intro / Outro | Wiki | damage/dodge energy gain, full-gauge icon glow, outgoing Outro + incoming Intro on switch | High | https://wutheringwaves.fandom.com/wiki/Concerto_Energy / https://wutheringwaves.fandom.com/wiki/Intro_Skill / https://wutheringwaves.fandom.com/wiki/Outro_Skill |
| `wuwa_vibration` | Wuthering Waves Vibration Strength | Wiki | break gauge depletion, immobilized window, damage increase, counterattack as strong reduction source | High | https://wutheringwaves.fandom.com/wiki/Vibration_Strength |
| `wuwa_liberation` | Wuthering Waves Resonance Liberation | Wiki | character-specific ultimate / liberation action family and state-change examples | Medium | https://wutheringwaves.fandom.com/wiki/Resonance_Liberation |
| `genshin_burst_iframe` | Genshin Impact Elemental Burst / Invincibility Frame | Wiki | energy/cooldown ultimate gate, HUD ready glow/audio, burst i-frames, dash i-frame timing reference | Medium | https://genshin-impact.fandom.com/wiki/Elemental_Burst / https://genshin-impact.fandom.com/wiki/Invincibility_Frame |
| `tof_phantasia` | Tower of Fantasy Phantasia | Wiki | red flash dodge cue, localized time-slow sphere, 7m radius, about 8s duration, about 20s cooldown, weapon charge reward | Medium | https://toweroffantasy.fandom.com/wiki/Phantasia |
| `zzz_data_camera_actions` | ZZZ public data mirror camera ability files | Public data mirror | `CameraZoomAction`, `CameraStretchAction`, `CameraOverrideTrackAction`, QTE/EX QTE camera modifiers, assist camera modifier, boss phase cutscene fields | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `zzz_data_witch_time` | ZZZ public data mirror `Ability_WitchTime.json` | Public data mirror | dodge-triggered time-slow, invincible buff, screen effects, sound actions, global event send/end, duration scale | Medium | https://raw.githubusercontent.com/360NENZ/Dimbreath-ZenlessData/master/Data/Ability_WitchTime.json |
| `pgr_data_camera_tables` | PGR public data mirror camera tables | Public data mirror | camera parameter table, VCamera/SceneCamera/CameraTrack LUTs, camera helper, screen effects, story/movie table split | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab |
| `hi3_data_story_camera_tables` | HI3 public data mirror story/camera tables | Public data mirror | `HybridSiteCamera`, plot/story/stage/effect tables; useful for story-camera binding shape, not direct shot timing | Low-Medium | https://github.com/nairieberry/HonkaiImpactData |
| `tales_arise_boost_strike` | Tales of Arise Boost Strike overview | Secondary encyclopedia / official-linked article | party interaction focus, evasion/counter emphasis, multi-party destructive attack under conditions | Low | https://en.wikipedia.org/wiki/Tales_of_Arise |
| `granblue_relink_party_action` | Granblue Fantasy: Relink party action overview | Secondary encyclopedia / official-linked article | four-person party / AI companions and high-spectacle action reference; use only for broad party-cinematic grammar | Low | https://en.wikipedia.org/wiki/Granblue_Fantasy:_Relink |
| `local_battle_camera` | IsekaiBrawl `BattleCamera` | Repo | cue priorities, follow/lookAhead/shake-offset, optional Cinemachine impulse listener hook | High | `Assets/_Game/Scripts/Core/BattleCamera.cs` |
| `local_camera_shake` | IsekaiBrawl `CameraShake` | Repo | optional Cinemachine Impulse Source reflection bridge | High | `Assets/_Game/Scripts/Core/CameraShake.cs` |
| `local_player_cues` | IsekaiBrawl `PlayerController` | Repo | dodge, summon, burst, and hit camera cue calls | High | `Assets/_Game/Scripts/Player/PlayerController.cs` |
| `local_boss_cues` | IsekaiBrawl `StoryPveEncounterRuntime` | Repo | boss break and final kill camera cue calls | High | `Assets/_Game/Scripts/Battle/StoryPveEncounterRuntime.cs` |

## Public Data Mirror Findings

These findings are useful because they show how commercial ARPGs separate gameplay events, camera modifiers, screen effects, time control, and cutscene cleanup into data surfaces. They are not import instructions.

| Game / source | Confirmed public file surfaces | Useful shape | `IsekaiBrawl` translation |
|---|---|---|---|
| ZZZ data mirror | `Anton_Camera.json`, `Aokaku_AssaultAidCamera.json`, `BoringMachine_CameraOverrideTrack.json`, `ComplexCorrupted_SwitchPhase_Cutscene.json`, `Ability_WitchTime.json` | character/assist/QTE camera modifiers, zoom/stretch actions, override track start/end actions, animation-state gated frame counts, boss HP lock/protect time/UI hide, time-slow screen effects | Add `cameraModifierKeys`, `zoomProfile`, `stretchProfile`, `overrideTrackKey`, `overrideTrackEndSignal`, `phaseLock`, `hideUiDuringCutscene`, `protectTimeSeconds`, `screenEffectStack`, `timePocketProfile` fields. |
| PGR data mirror | `CameraParam.tab`, `VCameraLut.tab`, `SceneCamera.tab`, `CameraTrackLut.tab`, `CameraNoiseLut.tab`, `ScreenEffect.tab`, `Movie.tab`, `Story.tab`, `XCameraHelper.lua` | camera parameters are separated from virtual camera prefab LUTs, scene-camera LUTs, camera-track LUTs, noise LUTs, and story/movie tables | Split our authoring data into `CinemachineShotProfile`, `CameraTrackProfile`, `CameraNoiseProfile`, `StoryCameraBinding`, and `CutsceneCueProfile` instead of one giant timeline blob. |
| HI3 data mirror | `HybridSiteCamera.json`, `PlotData.json`, `RandomPlotData.json`, `StageData_Story.json`, `StageDetail_Effect.json`, `AvatarEffectInfo.json` | story and camera-site IDs are table-driven; camera-site binding is separate from plot and stage-effect data | For dialogue / short in-run story beats, keep `storyBeatId`, `cameraSiteId`, `speakerAnchor`, `offset`, `stageEffectId`, and `returnCameraPolicy` separate. |
| Wiki/official-help mechanic sources | ZZZ Dodge/Chain/Assist/Decibels, PGR Matrix/QTE, HI3 QTE/Time Fracture, WuWa Intro/Outro/Vibration, Genshin Burst/i-frame, ToF Phantasia | combat trigger, resource, invulnerability, time-slow, and enemy-state rules | Use these for timing windows and affordance contracts; do not treat them as camera file data. |
| Low-confidence extra references | Tales of Arise, Granblue Fantasy: Relink, Aether Gazer search attempt | party action / cooperative finisher grammar; Aether Gazer source access was unreliable this pass | Keep as taste references only until a stable official/manual/data source is found. |

## Reference Game Pattern Pass

This pass exists because Unity-side tooling alone does not define taste. The reference games below show how subculture ARPGs make short in-game cutscenes feel expensive: they bind camera interruption, invulnerability, resource readiness, character entry, SFX/VFX, UI prompt, and enemy vulnerability into one readable package.

Do not copy their assets or exact character skills. Copy the production contracts.

| Pattern ID | Reference games | Observed data | Reusable contract for `IsekaiBrawl` |
|---|---|---|---|
| `break_to_chain_cinematic_ladder` | ZZZ Daze / Chain Attack, Wuthering Waves Vibration Strength | A filled break/stun gauge stops or weakens the enemy, then opens a limited action ladder. ZZZ scales chain count by enemy class; Wuthering Waves uses an immobilized vulnerability window. | Boss/elite break should expose `breakWindowSeconds`, `allowedFollowups`, `cameraTier`, `summonPreferredRoles`, and `returnReliefSeconds`. Use this for `boss_pressure_break_reframe` and `summon_break_entry_cutin`. |
| `assist_as_defensive_cutin` | ZZZ Assist, PGR QTE, HI3 QTE | Incoming danger or a fulfilled condition makes an off-field unit eligible. The entry skill is protected, visible, and often doubles as parry, heal, break, or rescue. | Our summon button should feel like an assist cut-in: `triggerCondition`, `entryAnchor`, `targetPriority`, `invulnerableDuringEntry`, `impactSignal`, `exitOrPersistPolicy`. |
| `perfect_dodge_time_pocket` | PGR Matrix, HI3 Time Fracture, Tower of Fantasy Phantasia, ZZZ Perfect Dodge | A tight enemy read plus dodge creates time control and a follow-up opportunity. The reward can be local slow, global slow, counter, meter, or tag-in readiness. | Perfect dodge can open a short `summonOpportunity` without adding a parry button. Data needs `readCue`, `dodgeWindow`, `timeScaleMode`, `opportunityDuration`, `allowedActions`, and `cameraCue`. |
| `resource_ready_cinematic_gate` | ZZZ Decibels, Genshin Elemental Burst, Wuthering Waves Concerto Energy | High-impact actions are gated by a visible meter, icon glow, sound, and character-specific action. Some grant invulnerability or HP locking during the action. | Ultimate and summon cut-ins should not be raw button effects. Use `meterReadyCue`, `uiGlow`, `readySfx`, `inputLock`, `iFramePolicy`, `damageApplySignal`, and `cooldownRestore`. |
| `tag_in_entry_exit_pair` | Wuthering Waves Intro/Outro, HI3 QTE, PGR swap/QTE | Switch or QTE often has two halves: outgoing effect and incoming entry. The outgoing unit can leave a buff/field, while the incoming unit arrives with a target-facing action. | Summons can mimic this without party switching: `callerSetupSignal`, `summonEntryShot`, `summonActiveEffect`, `summonExitOrField`, `playerReturnShot`. This supports Heal/Tank/Arrow/Break roles. |
| `localized_time_field` | Tower of Fantasy Phantasia, HI3 Time Fracture | Time effects are not always whole-game pauses. They can be local, radius-based, or tied to target state and timer behavior. | Prefer local pocket slow for summon/break moments. Data needs `fieldRadius`, `affectedActors`, `projectilePolicy`, `timerPolicy`, `bossExemptions`, and `restoreSignal`. |
| `cinematic_invulnerability_contract` | ZZZ Dodge/Assist/Chain, Genshin Burst, PGR QTE | Spectacle moments frequently grant invulnerability, but the protection scope differs: hitbox removal, HP lock, action invulnerability, or off-field QTE safety. | Every cut-in must declare `invulnerabilityType`: `none`, `hitbox_removed`, `hp_locked`, `entry_protected`, `target_only_pause`, or `full_scene_pause`. Do not hide this in animation. |
| `enemy_break_visibility` | ZZZ Daze, Wuthering Waves Vibration Strength, PGR shield/tank role | Break systems are readable because gauge, enemy state, camera emphasis, and follow-up affordance point at the same target. | Use `TargetGroupBreakPullback`, top-band hint, enemy outline/VFX, and summon-role prompt together; avoid only changing HP or damage multipliers invisibly. |
| `companion_effect_persistence` | Wuthering Waves Outro examples, PGR support QTE, ZZZ Bangboo Chain | A companion or off-field unit can enter briefly, then leave a timed buff, field, heal, projectile, or chain slot. | Summon cutscenes should not end at spawn. Each role needs `entryImpact`, `persistentEffect`, `duration`, `cleanupVfx`, and `roleSuccessMetric`. |
| `boss_class_scales_cinematic_budget` | ZZZ normal/elite/boss chain counts, PGR boss Matrix/QTE pressure, Wuthering Waves immobilize windows | Bigger enemies justify longer or multi-step cinematic windows, but only when their state actually changes. | Normal enemies stay Tier 0. Elites can use Tier 1. Boss phase break, intro, and final kill can use Tier 2. Reserve Tier 3 for stage boundaries and result bridges. |
| `data_driven_camera_modifier_stack` | ZZZ data mirror, PGR camera tables | Camera polish is not only one camera prefab. It can be a stack of zoom, stretch, override track, noise, screen effect, time pocket, end action, and cleanup keys fired by animation frames or combat state. | `CinemachineShotProfile` should allow modifier stacks: `zoom`, `lensPulse`, `targetOffset`, `screenStretch`, `impulse`, `postFx`, `overrideTrack`, `endActions`, `cleanupSignals`. |
| `story_camera_binding_split` | PGR story/movie/camera LUTs, HI3 HybridSiteCamera/PlotData | Story camera IDs, scene camera resources, plot rows, and stage effects are separate data surfaces. | Do not bind dialogue cameras directly into scenes. Use `storyBeatId -> cameraBindingId -> shotProfileId -> returnPolicy`. |
| `party_finisher_cinematic_burst` | Tales of Arise Boost Strike, Granblue Fantasy: Relink, ZZZ Chain Attack | Multi-character finishers feel expensive when the game clearly marks the eligibility condition, pulls camera authority briefly, then restores control with a resource/result payoff. | Summon/boss-break finishers can borrow this grammar with `eligibilityCue`, `assistRoster`, `sharedImpactFrame`, `resultReward`, `skipPolicy`, and `returnRelief`. |

### Reference Game Extraction Rules

- Treat `gauge filled -> enemy state changes -> follow-up affordance -> camera/VFX/SFX/UI proof -> gameplay return` as the strongest reusable chain.
- Use exact mechanic data only when it teaches scale: e.g. ZZZ default 3000 Decibel ultimate cost, PGR 15s Matrix cooldown, Genshin dash i-frame timing, Tower of Fantasy 7m/8s/20s Phantasia values.
- Do not import party switching wholesale. Convert tag-in/QTE ideas into summon entry, summon exit, and pocket-shift contracts.
- Do not overfit to one title. ZZZ is the best model for flashy assist/chain polish; PGR for dodge-time-pocket economy; HI3 for condition QTE; Wuthering Waves for intro/outro and break-window language; Genshin for ultimate ready/i-frame readability; Tower of Fantasy for localized time-field structure.
- Do not claim a reference game provided Cinemachine or Timeline source data unless a source explicitly says so. Current pass found no such public evidence.
- Public data mirrors can justify schema shapes and naming relationships, not asset import.
- If a cut-in does not change enemy state, summon state, player risk, or UI readiness, keep it as a Tier 0 camera/impulse cue.

## Extracted Unity Production Patterns

### 1. Camera Selection Layer

Cinemachine separates the logical game camera from the Unity Camera. A `CinemachineBrain` on the Unity Camera monitors active Cinemachine cameras, chooses the live camera, and resolves cuts/blends. This maps cleanly to our desired split:

- `BattleCamera`: gameplay authority and current cue seed
- `CinemachineBrain`: final camera output mixer
- `CinemachineCamera`: authored shot presets
- `Timeline` or `Sequencer Camera`: temporary shot-sequence authority
- `CameraReturnProfile`: explicit handoff back to gameplay camera

Implementation consequence: do not let a cutscene change gameplay camera state implicitly. Every cinematic cue must declare who temporarily owns the camera and how ownership returns.

### 2. Cinemachine Camera Shot Fields

A cutscene shot should be treated as a data object, not just a scene camera:

- `shotId`
- `trackingTargetKey`
- `lookAtTargetKey`
- `positionControl`
- `rotationControl`
- `lensPreset`
- `fieldOfView`
- `dutch`
- `cameraSide`
- `followDistance`
- `screenX`
- `screenY`
- `deadZone`
- `softZone`
- `damping`
- `blendIn`
- `hold`
- `blendOut`
- `impulseChannels`
- `returnHint`

Useful shot components:

- `ThirdPersonFollow` for gameplay-aligned over-shoulder and hero/summon entry shots
- `RotationComposer` for target-relative combat composition
- `SplineDolly` for boss intro fly-ins and stage establishing shots
- `TargetGroup` for player + boss + summon framing
- `ImpulseListener` for impact shake routed by channel

### 3. Timeline Shot Sequencing

Timeline is the safest path for predictable, choreographed multi-shot moments:

- Cinemachine Shot Clips declare which Cinemachine camera is active.
- Adjacent clips create cuts.
- Overlapping clips create blends.
- Lower Cinemachine tracks override higher tracks.
- Timeline temporarily overrides priority-based Brain selection.
- When Timeline stops driving Cinemachine, camera control returns to Brain.

This is the right layer for:

- boss intro
- summon entry cut-in
- ultimate cut-in
- boss break reveal
- final-kill bridge
- short character dialogue or reaction staging

### 4. Sequencer Camera For Micro Sequences

`CinemachineSequencerCamera` is useful when a full Timeline asset would be too heavy:

- child camera A hold
- cut/blend to child camera B
- cut/blend to child camera C
- optional loop
- final child holds until Sequencer is deactivated

Use it for `0.6s to 1.6s` combat cut-ins where no humanoid animation track, dialogue, or signal-heavy choreography is needed.

### 5. Timeline Asset / Instance Split

Timeline assets store tracks, clips, and recorded animations without scene-object bindings. Timeline instances, through `PlayableDirector`, store the scene bindings.

This is critical for reusable cutscenes:

- `BossIntro_TL` should not hardcode one boss scene object.
- `SummonEntry_Break_TL` should bind to the active player, selected summon, target enemy, combat camera, and VFX anchors at runtime.
- Every reusable Timeline needs a `TimelineBindingProfile`.

Required binding keys:

- `player_root`
- `player_head`
- `player_chest`
- `summon_root`
- `summon_spawn_anchor`
- `boss_root`
- `boss_head`
- `focus_target`
- `combat_camera`
- `cutscene_camera_root`
- `vfx_spawn_root`
- `sfx_bus`
- `ui_overlay_root`
- `signal_receiver`

### 6. Signals As Frame Tags

Timeline markers and signals should become the cutscene equivalent of combat frame tags.

Use signals for:

- `camera_cut`
- `camera_blend_start`
- `input_lock_start`
- `input_lock_end`
- `ai_pause_start`
- `ai_pause_end`
- `time_scale_set`
- `spawn_vfx`
- `spawn_afterimage`
- `play_sfx`
- `play_voice`
- `damage_apply`
- `summon_spawn`
- `boss_guard_break`
- `ui_flash`
- `hud_hide`
- `hud_show`
- `return_to_gameplay`
- `cleanup`

This keeps cinematic timing inspectable instead of scattering the sequence across coroutines and animation events.

### 7. Time Control

Unity gives at least three relevant time-control points:

- `PlayableDirector.UpdateMethod`: Game Time, Unscaled Game Time, DSP, or Manual
- `CinemachineBrain.Ignore Time Scale`: camera damping and input can continue during slow motion
- Gameplay time-scale or local simulation slowdown controlled by our runtime

Cutscene data must declare:

- `timeScaleMode`: `game_time`, `unscaled_timeline`, `manual`, `local_hitstop`, `global_slowmo`
- `affectedSystems`: player, enemies, summons, projectiles, UI, particles, audio
- `restoreTimeScale`: required

Default recommendation: use local hit-stop or short local slow motion first. Use global slow motion only when the shot is already authored to avoid gameplay desync.

### 8. Character Staging

Unity Timeline can blend humanoid clips and use Animation Override tracks. Animation Rigging can add runtime aim/IK corrections for cutscene-quality staging without needing bespoke clips for every target.

Useful staging patterns:

- upper-body dialogue or aim override while lower body stays in locomotion or idle
- head/chest `MultiAimConstraint` toward boss, summon, or camera-facing dummy
- hand/weapon `TwoBoneIKConstraint` for weapon pose and summon gesture alignment
- match offsets between approach, turn, land, and recovery clips
- short gesture tracks triggered by signals instead of full-scene animation replacement

This matters because our cutscenes are mostly in-game ARPG moments. They should feel authored without freezing the entire combat scene into a separate movie.

### 9. Impulse / Shake Routing

The repo already has optional Cinemachine impulse hooks in `CameraShake` and `BattleCamera`. That is a strong bridge point.

Recommended impulse channels:

- `HitLight`
- `HitHeavy`
- `PerfectDodge`
- `SummonLanding`
- `BossBreak`
- `UltimateImpact`
- `FinalKill`
- `EnvironmentCollapse`

Rules:

- Small hits should not all trigger full-screen shake.
- `SummonLanding`, `BossBreak`, `UltimateImpact`, and `FinalKill` can use stronger low-frequency impulse.
- Impulse source belongs to the event position; impulse listener belongs to the active Cinemachine camera.
- Cutscene shots should declare which impulse channels they listen to.

### 10. Existing Local Cue Bridge

Current local runtime already expresses camera importance through cue types:

- `PressureRead`
- `Dodge`
- `Hit`
- `SummonCall`
- `Burst`
- `BossBreak`
- `FinalKill`

Use these as compatibility seeds. A new cutscene system should not silently replace them. It should bridge:

- `BattleCameraCueType.SummonCall` -> optional `summon_break_entry_cutin`
- `BattleCameraCueType.Burst` -> optional `ultimate_short_cutin`
- `BattleCameraCueType.BossBreak` -> optional `boss_pressure_break_reframe`
- `BattleCameraCueType.FinalKill` -> optional `final_kill_result_bridge`
- `BattleCameraCueType.Dodge` -> stay mostly `BattleCamera`/Impulse unless promoted by special boss pattern

## Cutscene Tier Model

| Tier | Name | Duration | Owner | Use for | Lock policy |
|---:|---|---:|---|---|---|
| 0 | `CombatCue` | `0.08s to 0.6s` | `BattleCamera` + impulse | hit, dodge, quick summon nudge, pressure read | no hard lock; optional micro hit-stop |
| 1 | `MicroCinematic` | `0.6s to 1.6s` | Cinemachine camera or Sequencer Camera | summon landing, counter snap, elite break, boss reframe | short input lock or buffered input; selective AI pause |
| 2 | `CombatCutIn` | `1.6s to 3.5s` | Timeline or Sequencer Camera | ultimate, boss intro, break reward, final-kill start | explicit input/AI/time/UI locks |
| 3 | `SceneCutscene` | `3.5s to 12s` | Timeline + PlayableDirector | stage intro, dialogue, result bridge, chapter moment | skippable; full lock contract |

Default ARPG rule: stay in tiers `0` to `2` during combat. Use tier `3` only at stage boundaries or before/after the run.

## First Cutscene Use Cases

### `stage_start_pan`

- Goal: tell the player where the boss/elite pressure is before control begins.
- Recommended owner: Timeline or SplineDolly shot.
- Duration: `2.2s to 3.8s`.
- Data: stage anchor, player spawn, boss far anchor, first objective marker, HUD reveal signal.
- Return: gameplay follow camera with player control enabled on final `0.3s` blend.

### `boss_intro_pressure_read`

- Goal: show boss scale, threat direction, first pattern language, and safe camera baseline.
- Recommended owner: Timeline.
- Duration: `2.0s to 3.2s`.
- Shots: wide stage/boss, low hero reaction, boss telegraph close, gameplay return.
- Signals: hide HUD, boss roar SFX, telegraph VFX warmup, show top-band oracle hint, enable input.
- Return: soft-lock gameplay follow with boss weighted in TargetGroup for the first pressure beat.

### `summon_break_entry`

- Goal: make a good summon call feel like an assist entry that changes the pocket.
- Recommended owner: Sequencer Camera for first implementation; Timeline if animation/VFX signals need authored tracks.
- Duration: `0.9s to 1.35s`.
- Shots: gameplay alignment, summon spawn/landing close, target/boss break pullback, gameplay return.
- Signals: lock input, spawn summon, landing VFX, impact SFX, impulse, apply stagger/break, unlock input.
- Return: current BattleCamera with `SummonCall` or `BossBreak` cue recovery.

### `ultimate_short_cutin`

- Goal: sell the ultimate as a strong but short tactical answer, not a long movie.
- Recommended owner: Timeline.
- Duration: `1.6s to 2.4s`.
- Shots: hero close, target reaction, VFX charge, impact hold, gameplay return.
- Signals: lock input, set time scale/local slow, hide noisy HUD, spawn VFX layers, apply damage, impulse, restore time/UI.
- Return: gameplay follow with action recovery and input buffer consumed.

### `boss_pressure_break_reframe`

- Goal: prove the player created a breathing window.
- Recommended owner: Tier 1 Sequencer or Tier 2 Timeline depending on boss importance.
- Duration: `0.9s to 1.8s`.
- Shots: boss stagger, player/summon in frame, path/pocket opens.
- Signals: relief window start, projectile pause, top-band hint, reward pulse.
- Return: gameplay camera widened for `1.6s to 3.0s` relief read.

### `final_kill_result_bridge`

- Goal: connect final hit, boss collapse, player/summon victory pose, and result UI.
- Recommended owner: Timeline.
- Duration: `2.2s to 4.0s`.
- Shots: impact, boss collapse, hero/summon reaction, result background framing.
- Signals: stop enemy spawns, final-kill impulse, disable combat input, BGM transition, result UI handoff.
- Return: result scene/UI camera state, not normal combat camera.

### `dialogue_over_shoulder`

- Goal: support short in-run character beats without leaving the ARPG field.
- Recommended owner: Timeline with Animation Override tracks and look-at rigging.
- Duration: `3.0s to 8.0s`.
- Shots: over-shoulder hero, NPC/summon reaction, threat insert if combat-relevant.
- Signals: subtitle/voice, facial/gesture events, skip/advance.
- Return: previous gameplay state or stage-start control.

## Shot Archetype Catalog

| Shot ID | Use | Components | Targets | Default duration | Notes |
|---|---|---|---|---:|---|
| `GameplaySoftLockFollow` | default combat camera | ThirdPersonFollow + RotationComposer | player tracking, boss/target look-at or TargetGroup | persistent | Base return target for all combat cut-ins. |
| `HeroCloseReaction` | dodge, ultimate, dialogue reaction | ThirdPersonFollow or Follow + RotationComposer | player chest/head | `0.25s to 0.7s` | Avoid hiding incoming threats unless input/AI are locked. |
| `BossIntroWide` | boss reveal | SplineDolly or Follow + RotationComposer | boss root/head + stage anchor | `0.8s to 1.4s` | Establish direction and threat lane/field without returning to corridor logic. |
| `BossTelegraphInsert` | readable dangerous tell | Follow + Hard Look At/RotationComposer | boss weapon/hand/weakpoint | `0.25s to 0.65s` | Must end before player control becomes urgent. |
| `SummonLandingClose` | assist-like summon arrival | ThirdPersonFollow/Follow + RotationComposer | summon root + target | `0.25s to 0.45s` | Use impulse channel `SummonLanding`. |
| `TargetGroupBreakPullback` | pocket shift proof | TargetGroup + RotationComposer | player, summon, boss/elite | `0.4s to 0.9s` | Show the new safer space after break. |
| `UltimateChargeProfile` | ultimate anticipation | Hero close + lens/dutch optional | player chest/weapon | `0.35s to 0.8s` | Works best with local slow/time freeze and strong SFX lead-in. |
| `UltimateImpactHold` | ultimate hit | TargetGroup + impulse | player, target, VFX anchor | `0.2s to 0.55s` | Strong shake, flash, and hit-stop must not erase target readability. |
| `FinalKillHero` | victory emphasis | Follow/TargetGroup | player + defeated boss | `0.6s to 1.2s` | Bridge to result UI instead of normal combat control. |
| `DialogueOTS` | character conversation | Follow + RotationComposer + Animation Rigging | speaker/listener heads | `1.0s to 2.5s` per shot | Use Avatar Mask/override for upper body. |

## Data Contracts

### `CutsceneCueProfile`

Required fields:

- `cueId`
- `tier`
- `trigger`
- `priority`
- `duration`
- `owner`
- `canSkip`
- `canInterrupt`
- `inputLockProfile`
- `aiLockProfile`
- `timeProfile`
- `uiProfile`
- `cameraSequence`
- `timelineBindingProfile`
- `signalEvents`
- `returnProfile`
- `cleanupRules`

### `CinemachineShotProfile`

Required fields:

- `shotId`
- `cameraName`
- `trackingTargetKey`
- `lookAtTargetKey`
- `targetGroupKeys`
- `positionControl`
- `rotationControl`
- `lens`
- `fov`
- `dutch`
- `followOffset`
- `shoulderOffset`
- `cameraSide`
- `followDistance`
- `screenPosition`
- `deadZone`
- `softZone`
- `damping`
- `blendIn`
- `hold`
- `blendOut`
- `impulseChannels`
- `modifierStackProfile`
- `overrideTrackKey`
- `screenEffectStack`
- `cameraResourceKey`
- `notes`

### `CameraModifierStackProfile`

Required fields:

- `modifierStackId`
- `startFrame`
- `endFrame`
- `zoomProfile`
- `stretchProfile`
- `lensPulse`
- `targetOffset`
- `cameraNoise`
- `impulse`
- `postFx`
- `overrideTrack`
- `timePocket`
- `startActions`
- `endActions`
- `cleanupSignals`

Use this for the ZZZ-style layer where a combat move fires camera zoom, stretch, screen effect, override track, and cleanup from animation/state gates. In Unity, these become Cinemachine lens/target/impulse/post-processing changes plus Timeline or Sequencer Camera ownership when needed.

### `CameraTrackProfile`

Required fields:

- `trackId`
- `trackResourceKey`
- `shotProfileIds`
- `blendPolicy`
- `loopPolicy`
- `authorityPolicy`
- `endActionPolicy`

Use this for the PGR-style LUT split: scene camera, virtual camera, camera track, and noise are separate resource references, not one ad hoc scene script.

### `StoryCameraBindingProfile`

Required fields:

- `storyBeatId`
- `cameraBindingId`
- `cameraSiteId`
- `speakerAnchor`
- `listenerAnchor`
- `offset`
- `stageEffectId`
- `returnCameraPolicy`

Use this for HI3/PGR-style short story and dialogue beats, especially when a stage event needs a fixed camera site without contaminating normal combat camera state.

### `TimelineBindingProfile`

Required fields:

- `timelineAsset`
- `playableDirectorKey`
- `bindingKeys`
- `signalReceiverKey`
- `runtimeBindingPolicy`
- `missingBindingPolicy`
- `postPlaybackState`

Missing binding policy should default to `abort_cutscene_and_return_to_gameplay`, not `play_partial`.

### `GameplayLockProfile`

Required fields:

- `inputLock`
- `movementLock`
- `dodgeLock`
- `summonLock`
- `ultimateLock`
- `inputBufferPolicy`
- `playerAnimationAuthority`
- `enemyAiPolicy`
- `summonAiPolicy`
- `projectilePolicy`

Suggested AI policies:

- `none`
- `pause_all`
- `pause_target_only`
- `continue_projectiles`
- `freeze_projectiles`
- `keep_boss_telegraph_only`

### `TimeControlProfile`

Required fields:

- `timelineUpdateMethod`
- `brainIgnoreTimeScale`
- `globalTimeScale`
- `localHitStop`
- `restoreTimeScale`
- `affectedSystems`
- `audioPolicy`
- `particlePolicy`

### `SignalEventProfile`

Required fields:

- `eventId`
- `time`
- `frame60`
- `signalAsset`
- `receiver`
- `payloadKey`
- `mustFire`
- `rollbackOrCleanup`

### `CameraReturnProfile`

Required fields:

- `returnTarget`
- `returnCamera`
- `returnBlend`
- `returnDuration`
- `restorePriority`
- `restoreBattleCameraCue`
- `clearImpulseChannels`
- `restoreHud`
- `restoreInput`

### `ImpulseCueProfile`

Required fields:

- `impulseId`
- `channel`
- `sourceKey`
- `gain`
- `duration`
- `frequency`
- `falloff`
- `listenerPolicy`

### `CharacterStagingProfile`

Required fields:

- `characterKey`
- `baseAnimation`
- `overrideTracks`
- `avatarMasks`
- `rigConstraints`
- `lookAtTargets`
- `ikTargets`
- `rootMotionPolicy`
- `returnAnimatorState`

## First Implementation Profiles

### `summon_break_entry_cutin`

Recommended first prototype because it directly supports the game's differentiator.

- Tier: `MicroCinematic`
- Duration: `1.05s`
- Owner: `SequencerCamera` first, Timeline later if signals need authoring
- Trigger: successful Break/Tank/Arrow/Heal summon call during `perfect_dodge`, `boss_pattern_break`, `structure_break`, or `pressure_danger`
- Camera shots:
  - `GameplaySoftLockFollow` align: `0.12s`
  - `SummonLandingClose`: `0.34s`
  - `TargetGroupBreakPullback`: `0.42s`
  - gameplay return blend: `0.17s`
- Lock:
  - movement: soft lock `0.45s`
  - dodge: buffer accepted after `0.55s`
  - summon: locked during sequence
  - ultimate: buffered only
  - AI: pause target pocket only; boss global pressure may hold if not involved
- Signals:
  - `summon_spawn`
  - `landing_vfx`
  - `landing_sfx`
  - `summon_landing_impulse`
  - `apply_break_or_relief`
  - `return_to_gameplay`
- Return:
  - `BattleCameraCueType.SummonCall` or `BossBreak`
  - relief camera widened for `1.6s to 3.0s`

### `ultimate_short_cutin`

- Tier: `CombatCutIn`
- Duration: `1.8s`
- Owner: Timeline
- Trigger: player presses ultimate with valid energy and target context
- Camera shots:
  - `HeroCloseReaction`: `0.35s`
  - `UltimateChargeProfile`: `0.55s`
  - `UltimateImpactHold`: `0.45s`
  - gameplay return: `0.45s`
- Lock:
  - input: full lock until impact; buffer movement/dodge near end
  - AI: pause dangerous new actions, allow already-authored reaction
  - projectiles: freeze or slow depending cue
- Time:
  - local hit-stop or local slow preferred
  - if global slow is used, `restoreTimeScale` must be mandatory
- Signals:
  - `hud_hide_soft`
  - `ultimate_charge_vfx`
  - `ultimate_voice_or_sfx`
  - `damage_apply`
  - `ultimate_impact_impulse`
  - `hud_show`

### `boss_intro_pressure_read`

- Tier: `CombatCutIn`
- Duration: `2.4s`
- Owner: Timeline
- Trigger: first boss pressure encounter or boss phase reveal
- Camera shots:
  - `BossIntroWide`: `0.85s`
  - `BossTelegraphInsert`: `0.45s`
  - `HeroCloseReaction`: `0.35s`
  - gameplay return with boss in TargetGroup: `0.75s`
- Lock:
  - input disabled until final return blend
  - enemy AI disabled except staged telegraph
  - no damage application during intro unless explicitly tutorialized
- Signals:
  - `boss_roar_sfx`
  - `telegraph_vfx_warmup`
  - `top_band_hint_show`
  - `enable_player_control`

### `boss_pressure_break_reframe`

- Tier: `MicroCinematic` or `CombatCutIn`
- Duration: `1.2s`
- Owner: Sequencer Camera
- Trigger: boss/elite pattern break, structure break, or strong summon answer
- Camera shots:
  - boss stagger insert: `0.35s`
  - player/summon advantage frame: `0.35s`
  - widened gameplay return: `0.5s`
- Lock:
  - no full combat pause unless the player created the break through a major action
  - disable new boss pressure during relief proof
- Signals:
  - `break_vfx`
  - `break_sfx`
  - `boss_break_impulse`
  - `relief_window_start`
  - `energy_reward_pulse`

### `final_kill_result_bridge`

- Tier: `CombatCutIn` or `SceneCutscene`
- Duration: `2.6s`
- Owner: Timeline
- Trigger: final boss defeated
- Camera shots:
  - `UltimateImpactHold` or final hit hold: `0.35s`
  - boss collapse: `0.85s`
  - `FinalKillHero`: `0.8s`
  - result UI bridge: `0.6s`
- Lock:
  - full combat input off
  - enemy/summon cleanup
  - projectiles removed or faded
- Signals:
  - `final_kill_impulse`
  - `stop_combat_bgm`
  - `victory_stinger`
  - `result_ui_prepare`
  - `result_ui_show`
- Return:
  - result UI/camera, not combat control

## Authoring Rules For Our Game

1. Author combat cutscenes as data first: cue, tier, duration, locks, time, camera sequence, signals, return, cleanup.
2. Keep `BattleCamera` as gameplay authority. Cinemachine/Timeline should temporarily override and then return cleanly.
3. Use Timeline for authored sequences longer than roughly `1.2s`, or when animation, VFX, SFX, UI, and damage must be frame-aligned.
4. Use `CinemachineSequencerCamera` for short three-shot action moments where a full Timeline asset would slow iteration.
5. Use `TargetGroup` to keep player, summon, boss, and break target readable in the same shot.
6. Use `SplineDolly` for stage fly-ins and boss intros, not for every combat cue.
7. Use Impulse by channel. Do not route every hit into the same strong camera shake.
8. Use Timeline signals as cinematic frame tags.
9. Every Timeline asset needs an explicit binding profile, because reusable assets do not store scene object links.
10. Every cutscene must restore input, AI, HUD, time scale, camera priority, and live gameplay camera.
11. Avoid long cinematic stops in the middle of combat. The best combat cut-ins are short enough to feel like an action beat, not an interruption.
12. For summon-first identity, the first polished cutscene should be `summon_break_entry_cutin`, not a player-only ultimate.

## Risk Register

| Risk | Failure mode | Prevention |
|---|---|---|
| Timeline/Brain conflict | Cutscene camera loses control or never returns | one owner per cue, explicit `CameraReturnProfile`, Timeline stopped/cleared after playback |
| Missing bindings | Timeline plays with wrong or null targets | `TimelineBindingProfile` with required keys and abort policy |
| Dirty time scale | gameplay remains slow or camera damping feels wrong | mandatory restore event and runtime guard |
| Overlong cut-ins | 3 to 5 minute ARPG run feels blocked | tier duration caps and skip policy |
| Camera hides danger | player regains control while threat is off-screen | AI/input lock until danger is readable again |
| Too much shake | patterns become unreadable | impulse channels, gain caps, cue cooldowns |
| Local-only scene setup | main PC cannot reproduce shot | data-first profiles and prefab/asset binding keys |
| Cutscene replaces combat identity | player-only cinematic fantasy overpowers summons | prioritize summon cut-in and boss break/pocket-shift proof |

## Recommended Next Implementation Path

1. Create a small `CutsceneCueProfile` ScriptableObject or serializable data class.
2. Build `CutsceneDirector` as a runtime bridge that owns PlayableDirector binding, locks, time restoration, and camera return.
3. Add a gameplay camera Cinemachine setup that mirrors current `BattleCamera` values before replacing behavior.
4. Prototype `summon_break_entry_cutin` with three shot profiles and no new combat rules.
5. Add Timeline signals only after the first Sequencer version proves the shot duration and return feel.
6. Add `ultimate_short_cutin` second.
7. Add `boss_intro_pressure_read` third.
8. Add validation logs for missing bindings, dirty time scale, unreturned camera, and stuck input lock.

## Handoff Summary

New active Unity cutscene baseline:

- Cinemachine/Timeline are production layers for controlled in-game cutscenes.
- `BattleCamera` remains the gameplay authority and seed cue layer.
- Short ARPG combat moments should use tiers `0` to `2`.
- Every cue must declare shot type, duration, locks, time behavior, Timeline bindings, signals, impulse, return, and cleanup.
- First target should be a summon/break cut-in that proves the game's unique identity before player-only spectacle expands.
