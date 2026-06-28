# Intro GatePod Invasion ArkData Evidence

Date: 2026-06-28

This is the evidence checkpoint required by
`INTRO_GATEPOD_INVASION_ARKDATA_GUARDRAIL.md`. No Unity scene/profile/runtime
implementation has been made in this checkpoint.

## Current Cutscene Boundary

- Current profile: `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening.asset`
- Current sequence id: `intro_gatepod_awakening`
- Current authored duration: `20.574354s`
- Existing camera beats to preserve:
  - `0.000-6.100s`: capsule left dolly.
  - `6.100-8.133s`: first-person eye open.
  - `8.133-9.683s`: first-person scan left.
  - `9.683-11.133s`: first-person scan right.
  - `11.133-12.883s`: first-person look down at hands.
- Problem area to replace/extend:
  - `12.883s+`: current commando bridge/legs/push-past soldier beat reads too
    small for "heaven is under invasion".
- Rule for this pass:
  - Do not replace the capsule, voice, first-person eye-open, scan, or hand-look
    sequence unless the user asks.

## Previous Thread Lessons

| Thread Source | Sample Count | Relevant Events | Timing / Density Evidence | Applicability | Decision |
| --- | ---: | --- | --- | --- | --- |
| `codex://threads/019f08e2-887f-79d0-a80b-35cc6fa02e49`, turns `019f09b2`, `019f09ba`, `019f09be`, `019f09d2` | 4 inspected turns | Empty cutscene port, visible payload copy, manual `IntroGatePodPortPayload_Visuals`, separate runtime copy, commit `61a249d1` | The failure was not timing data but workflow: structure-only work was invisible; regenerating visuals after manual placement would overwrite user work. | direct | Keep visible payload/runtime separated. Never regenerate the user's adjusted visual root casually. |
| `codex://threads/019f08e2-887f-79d0-a80b-35cc6fa02e49`, turns `019f099c`, `019f099f`, `019f09a5` | 3 inspected turns | "Olympus asset" misunderstanding, corridor stage, cutscene ports, StageDefinition/anchor/handoff | The correct direction is shared corridor/world plus cutscene port, not filling a cutscene review scene with temporary geometry. | direct | New invasion beats must target the intro port/stage handoff world, not an isolated review-scene decoration pass. |
| `codex://threads/019ef8af-b588-7101-9be1-6b8ec1bc5602` | 2 inspected turns | Reusable cinematic package plan; story layer belongs on `CinematicSequenceProfile` | Every cutscene needs purpose, camera reason, animation, and handoff condition. Story cutscene is emotion/gameplay transition via camera/body/face/sound. | direct | Use existing cinematic profile/cue system; do not create a separate story system for this intro extension. |
| `codex://threads/019ef7f0-35b0-7500-a5ad-9791ce29957b`, latest inspected turns | 6 inspected turns | Automatic asset reimport, broad YAML edit, disabled playback guard | Broad scene/YAML edits can accidentally disable Camera/Light. Disabled components must refuse direct playback. | direct | Implementation must be narrow, checkpointed, and validated after every runtime/schema and scene/profile step. |

## ArkData Source Boundary

| Source | Path | Sample Count | Relevant Command / Event Names | Timing Stats | Applicability | Decision |
| --- | --- | ---: | --- | --- | --- | --- |
| Data bank start guide | `\\DESKTOP-69817L3\ArkData\NarrativeCinematicFeaturePack\main_pc_3d_cinematic_data_bank\START_HERE.md` | 1 guide | PGR true 3D first, then Aether, Ash, ZZZ | The guide states P0/P1/P2 priority, not seconds. | direct | Treat PGR true 3D as the only ready-to-author core; use non-PGR as support after PGR grammar. |
| Ready/raw boundary | `...\what_is_ready_vs_raw.md` | 1 guide | PGR command timeline, camera shot list, actor blocking, dialogue beats | PGR true 3D is ready; P1 rows need mapping; P2 rows are support/research. | direct | Do not claim exact restored Timeline/keyframes unless they are in PGR true 3D or local imported Rina package. |
| PGR true 3D command timeline | `...\snapshots\pgr_true3d_command_timeline.csv` | 318 rows, 8 scenes, 4 movie files | `Load3DScene` 6, `LoadCameraSet` 8, `PlayCamera` 48, `SpawnActor3D` 40, `PlayActorAnimation` 88, `Show3DDialogue` 128 | Ordered action rows exist, but no literal second timeline. Density grammar is camera/actor/dialogue command alternation. | direct | Use as sequence grammar: scene signal, camera load/play, actor spawn, body/face animation, then dialogue/response. |
| PGR true 3D camera shots | `...\snapshots\pgr_true3d_camera_shots.csv` | 48 rows | `CameraPlay`, named cameras, camera asset paths | 42 full-transform rows; 20 non-zero blend rows; blend values `0,4,7,8,10,40`; movie shot counts: 18, 14, 14, 2. | direct | Translate blend values into classes: hard cut, short blend, long hold. Do not treat them as literal seconds. |
| PGR actor blocking | `...\snapshots\pgr_true3d_actor_blocking.csv` | 128 rows | `SpawnActor3D` 40, `PlayActorAnimation` 88 | 10 actor roles; 88 body animation rows; 50 face animation rows; 40 position rows. | direct | Soldier/protagonist beats must be actor cues with body animation and blocking, not static spawn-only placement. |
| PGR dialogue / animation vocab | `...\snapshots\pgr_true3d_dialogue_beats.csv`, `...\snapshots\pgr_true3d_animation_vocab.csv` | 128 dialogue rows, 13 animation vocab rows | `Show3DDialogue`, body names such as `PlotStand1`, `PlotSerious1Loop`, `PlotTalk1Loop`, `PlotAngry1Loop` | Dialogue is tied to role/action order, not VN portrait timing. | grammar-only | Use only to justify actor/dialogue locking and emotion-state transitions. The intro protagonist remains silent unless user asks otherwise. |
| PGR first-cutscene evidence | `...\first_cutscene_workshop\pgr_3d_story_action_evidence.csv` | 185 rows | PGR type 702/703/704/707/708/711 in the workshop summary | Workshop summary extracts: CameraLoad 30, CameraPlay 24, ModelLoad 20, ModelAnimationPlay 44, Dialog3D 64, VoicePlay 3. | direct | Use the workshop's "camera/actor/dialogue lock" templates as the primary local design grammar. |
| PGR camera lerp forensics | `...\first_cutscene_workshop\pgr_camera_lerp_forensics.md` | 1 report | `XMovieActionCameraPlay`, `XUiMovie:SwitchCamera` | Evidence: 48 story shots, 20 non-zero blend rows, 42 full transforms, EN curve samples, blend values `0,4,7,8,10,40`. | direct | Use Cinemachine-style default blend classes; impact camera motion is additive, not the main camera path. |
| PGR curve shapes | `...\first_cutscene_workshop\pgr_camera_curve_shape_summary.csv` | 39 rows | `impact_recoil_return`, `micro_motion`, `static_or_empty` | 31 impact recoil/return, 5 micro motion, 3 static/empty. | direct | Explosions, portal shock, sword lock, and soldier impact should use short zero-return recoil; no long shaky camera. |
| First cutscene beat map | `...\first_cutscene_workshop\first_cutscene_beat_template_map.md` | 8 beat rows | `our_heaven_collapse_establishing`, `our_weapon_pickup_insert`, `our_enemy_standoff_pullback`, `our_gameplay_backview_takeover` | Prior map covers `28-34s` enemy standoff and `34-39s` back-view entry for a longer first cutscene. | grammar-only | Reuse the beat roles, but remap around the current implemented `0-12.883s` capsule/POV boundary. |
| Quality guardrails | `...\first_cutscene_workshop\cinematic_quality_guardrails.md` | 1 report | No body scan, no glamour orbit, every shot needs purpose | Tuning starts: reveal orbit <= 25 deg, long establishing blend <= 1.8s, final gameplay match window 0.2s. | direct | Protagonist third-person reveal must be face-hidden or back-view until the intended reveal. |
| Aether support | `...\snapshots\aether_timeline_asset.csv`, `...\snapshots\aether_camera_usage.csv` | 60 rows, 29 rollup rows | Timeline/Cinemachine refs, camera runtime usage, story camera FOV lines | Camera usage rollup includes P0 camera preset rows 63, P0 runtime usage 115, P0 high-value narrative lines 115. | grammar-only | Useful for confirming camera preset/FOV/runtime categories exist in other games; not a direct timeline source. |
| Ash support | `...\snapshots\ash_cutscene_rollup.csv`, `...\snapshots\ash_cutscene_duration.csv` | 7 rollup rows, 162 duration rows | `combat-cinematic`, `boss-cinematic`, `cutscene`, `ultra-skill-cinematic` | Duration averages: combat 4977.4ms, cutscene 4593.3ms, boss 4325.2ms, ultra 2881.8ms; min combat 417ms, max 13800ms. | grammar-only | Use to keep invasion/action clusters around 3-5s chunks instead of many disconnected 1s prompts. |
| GFL2 support | `...\snapshots\gfl2_event_camera_context.csv` | 5426 rows | `direct-camera-id` 1027, `ultra-camera-id` 34, event types 1/2/3/4/5/6/9/12/13 | Frame index stats: min 0, max 472, avg 49.05. Event cameras are frame-triggered around skill/action events. | grammar-only | Portal opening, bomb impact, soldier attack, and protagonist kick should be frame/event-triggered cues, not purely camera-only shots. |
| ZZZ Rina restoration | `...\snapshots\zzz_rina_segments.jsonl`, `...\snapshots\zzz_rina_cinemachine.json` | 331 segment rows, 3 vcam records | Rina camera segments, `Avatar_Female_Size03_Rina_Cam_QuestSt` | Segment categories include camera 9, camera/effect/combat 5, camera/combat 3. Vcam export says Timeline pairing is required. | grammar-only | ArkData ZZZ alone is not enough for final timing. Use imported desktop Rina package as the actual camera reference. |
| Imported Rina camera package | `Assets/_Imported/Reference/ZZZ_RinaLoopKit/...` | 1 prefab, 1 controller, 1 anim clip | `Rina_QuestStart_MainCameraRig`, `Rina_QuestStart_OriginalExtracted.anim` | Animation sample times run `0.000-7.500s` at 1/60s intervals. | direct | Use only for the final third-person feet/upper/back-view reveal after PGR/Ash/GFL2 timing grammar is applied. |

## Project Asset Candidates

| Need | Candidate Asset | Evidence | Applicability | Decision |
| --- | --- | --- | --- | --- |
| Air raid / bombing pre-signal | `Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_41_Airstrike/Effect_41_Base/Effect_41_BombExplosion.prefab` and `Effect_41_BulletExplosion.prefab` | File exists under SpecialSkillsEffectsPack. | direct | Preferred for aerial bombardment impacts. |
| Wide orbital threat | `.../Effect_03_OrbitalStrike/Effect_03_OrbitalStrike.prefab`, `Effect_03_OrbitalAnnihilationBeam.prefab` | File exists. | direct | Use as distant/heaven-scale pressure, not as final portal. |
| Aircraft silhouettes | `Assets/_Imported/SpecialSkillsEffectsPack/Models/Jet_04.fbx`, `Bomber_02.fbx`, `Bomb_01.fbx`, `UAV_01.fbx` | Files exist. | direct | Use as background flyby/strike props if materials validate. |
| Portals | `Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_16_SpaceWarpPortal/Effect_16_SpaceWarpPortal.prefab` | File exists. | direct | Use before soldier spawn. This satisfies "no soldier spawn without pre-spawn signal". |
| Explosions / impacts | `Effect_10_SpaceFleetCall/.../Effect_10_ShellExplosion*.prefab`, `Effect_13_DangerClose/.../Effect_13_Explosion.prefab`, `Effect_50_DeathWave.prefab` | Files exist. | direct | Use for staggered background hits. Avoid poison-colored `Effect_23` unless recolor/readability is confirmed. |
| Sword | `Assets/_Imported/SpecialSkillsEffectsPack/Models/Sword_1.fbx` | Already referenced by `IntroGatePodCutsceneReviewSetup.cs` as `SwordModelPath`. | direct | Replace rifle-focused handoff with sword/melee confrontation. |
| Slash / melee FX | `Effect_08_GroundSlash.prefab`, `Effect_36_MadnessSlash.prefab`, SpecialSkills slash textures/materials | Files exist. | direct | Use sparingly for sword ready/deflection; no overpowered ultimate read. |
| Masked sci-fi soldiers | `Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_GeneralDeck.prefab`, `PF_Enemy_SciFiSoldier_EliteDeck.prefab`, `PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab` | Files exist in `_Game`. | direct | Use these instead of review-only primitive commandos. Keep faces hidden by helmet/angle. |
| Soldier attack/hit anims | `Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/SciFiHeavyBattleArmor/HBA_MeleeAttackWeapon.fbx`, `HBA_MeleeAttackForwardWeapon.fbx` | Files exist. | direct | Use for soldiers attacking through portals and being interrupted. |
| Protagonist kick option | `Assets/_Imported/AssetStore/CLazyRunnerActionAnimPack/Animations/P4_CLazyAttack/Attack_Kick_Run/CLazy@Attack_Kick_A.FBX` and `Knight_Zweihander_Animset/.../Lucy_Kick_Combo*.FBX` | Search found kick FBX/controller candidates. | grammar-only until retarget validated | Use for a face-hidden third-person interrupt if retarget/import validates; otherwise hand-authored fallback with existing combat state. |
| General humanoid soldier anim pool | `Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Animations` | 644 FBX files; examples include combo, aim, crouch, hit, death, idle sets. | grammar-only until state mapping validates | Use as animation vocabulary only after checking clip import/avatar mapping. |

## Beat Table

Tentative total duration after rewrite: about `33.600s`. The existing
`0.000-12.883s` section remains fixed. The extension begins where the current
cheap commando bridge starts.

| Beat | Time Range | Narrative Job | ArkData Evidence | Project Asset | Camera Rule | Actor/Animation |
| --- | --- | --- | --- | --- | --- | --- |
| 0. Existing Awakening Lock | `0.000-12.883s` | Preserve voice/capsules/first-person eye-open, scan, and hand-look. | Current profile has six existing camera cues ending at `src_c06_first_person_look_down_hands`. Guardrail says no replacement unless asked. | Existing GatePod profile, Timeline, Inori, capsules. | No new camera plan here except compatibility checks. | Keep `CIN_IntroLookAtHands` and existing face cues. |
| 1. Heaven-Wide Alarm Cut | `12.883-14.250s` | Pull out of the hand-look into the fact that heaven is under attack; establish pre-spawn signal before soldiers. | PGR camera shots support hard cuts (`blend_time=0`, 28/48 rows) and short blends. Quality guardrail requires every shot to add threat direction. | Air raid props `Jet_04.fbx`, `Bomber_02.fbx`, warning sweep screen FX. | Hard cut or very short blend from POV to high/wide damaged heaven/stage view. No protagonist face. | Inori remains off-camera or silhouette/shoulder only; no soldier spawn yet. |
| 2. Airstrike Impact Chain | `14.250-16.200s` | Show aircraft/bombing and explosions across multiple zones, so invasion feels broader than one bridge. | PGR curve shapes: `impact_recoil_return` 31/39. Ash action chunk averages support 3-5s action clusters; this is the first half of such a cluster. | `Effect_41_BombExplosion`, `Effect_41_BulletExplosion`, `Effect_03_OrbitalStrike`, aircraft/bomb models. | Wide lateral drift plus additive zero-return recoil only on impacts. Do not turn recoil into constant shake. | Background aircraft flyby and bomb impacts are event cues; no protagonist glamour shot. |
| 3. Portal Breach Stagger | `16.200-18.100s` | Open several breach points before soldiers arrive; make spawn causality visible. | GFL2 event camera rows show frame/event camera signals (`direct-camera-id` 1027, avg frame 49.05). PGR grammar supports action-ordered cue chains. | `Effect_16_SpaceWarpPortal`, warning sweep, smoke/flash. | Medium-wide angle with foreground/midground portals; 1-2 short cuts, not second-by-second prompt montage. | Portal open events are staggered; soldiers still silhouettes behind portals. |
| 4. Soldier Surge With Attacks | `18.100-20.900s` | Masked sci-fi soldiers pour out, attack, and are hit by surrounding blasts; not just three soldiers running on an empty bridge. | PGR actor blocking: `SpawnActor3D` 40, `PlayActorAnimation` 88, 40 rows with positions. GFL2 supports event-triggered camera/action links. | `PF_Enemy_SciFiSoldier_GeneralDeck`, `EliteDeck`, `Melee_ClosePunish`; `HBA_MeleeAttack*.fbx`; impact FX. | Low/over-shoulder and side cuts. Keep helmeted soldiers readable; use short blend class from PGR. | Soldier waves use run/attack/hit states, 2-3 portal groups, staggered start times. |
| 5. Protagonist Interrupt Kick | `20.900-23.300s` | As soldiers rush in, protagonist appears in third-person but face-hidden and interrupts one with a kick. | PGR actor/dialogue lock proves body animation should carry the beat; quality guardrail forbids face/body scan. Kick exact timing is hand-authored fallback until retarget validation. | Kick candidates from `CLazyRunnerActionAnimPack` or `Knight_Zweihander_Animset`; soldier hit anims/FX. | Camera starts at foot/leg/shoulder height, never clear face. Impact gets small zero-return recoil. | Inori kick or combat interrupt; target soldier hit/stagger/death. If kick retarget fails, use a melee shove/strike with existing combat state. |
| 6. Sword Claim / Combat Resolve | `23.300-26.100s` | Shift from survival reaction into melee confrontation; replace rifle-focused handoff with sword/close combat. | First-cutscene beat map has `our_weapon_pickup_insert` sword variant. PGR supports short blend and actor animation/prop state changes, but exact sword pickup is mixed/hand-authored. | `Sword_1.fbx`, `Effect_08_GroundSlash`, `Effect_36_MadnessSlash`, floor sword / hand socket. | Close insert on hand/sword/boots/threat line, not face. Short blend; no long pose hold. | Hide rifle path; show sword pickup/ready, then low guard stance. |
| 7. Standoff To Rina Back-View | `26.100-33.600s` | Line up the protagonist against the soldiers and naturally become third-person/back-view gameplay framing. | PGR `our_enemy_standoff_pullback` and `our_gameplay_backview_takeover` templates are grammar support; imported Rina package provides direct 7.5s camera rig samples. | `Rina_QuestStart_MainCameraRig.prefab`, `Rina_QuestStart_OriginalExtracted.anim`, sci-fi soldier line, stage handoff anchor. | Use the Rina feet-to-upper/body-to-back-view camera path, remapped to the stage. Last `0.2s` must match gameplay camera position/rotation/FOV. Face remains unrevealed or occluded. | Inori sword-ready stance, soldiers aiming/closing in, final combat idle aligned to handoff. |

## Design Decisions

- ArkData does not currently provide a complete second-by-second authored
  invasion Timeline for this exact scene.
- ArkData does provide a usable cinematic grammar:
  `camera signal -> actor spawn/blocking -> actor animation -> event impact ->
  next camera`.
- The previous "three commandos on a bridge" should be replaced, not polished.
- Soldier entry needs at least one pre-spawn signal: air raid, portal open,
  warning sweep, muzzle/impact, or explosion.
- Rina camera data is reserved for the final third-person/back-view reveal. It
  should not be used to justify earlier air raid/portal/soldier timing.
- Exact kick retarget and portal/soldier layout are implementation validation
  tasks, not evidence claims yet.

## Implementation Gates After This Document

1. Commit this evidence/beat-plan checkpoint.
2. Runtime/schema checkpoint:
   - Add cue data only if needed for air raid, portal, soldier, sword, and Rina
     camera references.
   - Compile validation before scene/profile generation.
3. Scene/profile/timeline checkpoint:
   - Regenerate/update `IntroGatePodCutsceneReview` and Olympus intro port without
     overwriting user-adjusted visual roots unexpectedly.
   - Validate `IntroGatePodCutsceneReview`, `OlympusCorridorInvasionStage`, and
     cinematic package.
4. Capture/polish checkpoint:
   - Capture at least: air raid, portal opening, soldier surge, protagonist kick,
     sword ready, Rina back-view handoff.
   - Review for no clear face reveal, no body scan, no empty-bridge soldier beat,
     and no rifle-focused handoff.
