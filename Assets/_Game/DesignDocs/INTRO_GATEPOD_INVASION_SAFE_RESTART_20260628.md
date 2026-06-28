# Intro GatePod Invasion Safe Restart

Date: 2026-06-28

This note is the restart lock after the failed invasion-addition pass and the
partial revert correction. It exists to prevent repeating the same mistake:
editing protected first-person/manual placement data while trying to improve the
soldier invasion section.

## Current Git Baseline

- Current safe restart commit: `909351873 fix: restore initial intro cutscene runtime`.
- The initial stage runtime port integration must stay alive:
  - `61a249d1 Add Olympus intro cutscene stage runtime`
  - `StageCutscenePort`
  - `StageDefinitionSceneBinding` cutscene port references
  - `OlympusCorridorInvasionStage` initial intro port/runtime binding
- Later invasion additions remain removed from the active file state:
  - ArkData-driven soldier/portal review cutscene pass.
  - Portal fallback material.
  - Invasion bridge refresh pass.
  - First-person recovery/audit commits that were only repair attempts.
- Known unrelated dirty file to preserve:
  - `Assets/_Game/Art/Materials/ActionFoundation/AF_SummonPressureScreen.mat`

## Protected Areas

Do not touch these unless the user explicitly asks for that exact area:

- Existing capsule/system/voice/first-person sequence.
- `IntroGatePodReview_FirstPersonViewMarker`.
- `IntroGatePodReview_InoriPlacement`.
- Existing hand-look / scan / capsule eye-open camera beats.
- User/manual `IntroGatePodPortPayload_Visuals` placement.

Do not run these regeneration paths casually:

- `EnsureReviewScene()`
- `RunBatchSetupCaptureAndValidation`
- `ApplyIntroGatePodPayloadToOlympusStage`

Allowed direction:

- Additive work after the protected first-person boundary.
- Separate runtime/sequence roots.
- Evidence-first beat planning.
- Validation after every schema/runtime/scene checkpoint.

## Rechecked ArkData Facts

The source files below were reopened from ArkData on 2026-06-28 before any new
scene edit.

| Source | Rechecked Result | Decision |
| --- | --- | --- |
| `pgr_true3d_command_timeline.csv` | 318 rows across 8 scenes / 4 movie files: `Show3DDialogue` 128, `PlayActorAnimation` 88, `PlayCamera` 48, `SpawnActor3D` 40, `LoadCameraSet` 8, `Load3DScene` 6. | Use PGR as 3D story grammar: camera signal, actor spawn, body animation, response/dialogue. |
| `pgr_true3d_camera_shots.csv` | 48 camera rows. Blend values: `0` x28, `4` x2, `7` x2, `8` x6, `10` x4, `40` x6. Non-zero blends: 20. | Use cut / short blend / long hold classes, not literal seconds. |
| `pgr_true3d_actor_blocking.csv` | 128 rows: `PlayActorAnimation` 88, `SpawnActor3D` 40. 10 roles, 88 body animation rows, 50 face rows, 40 position rows. | Soldier beats need real spawn/blocking/animation; no static spawn-only placement. |
| `first_cutscene_workshop/pgr_3d_story_action_evidence.csv` | 185 rows: type 708 x64, 707 x44, 702 x30, 703 x24, 704 x20, 711 x3. | Use workshop evidence for camera/actor/dialogue locking. |
| `gfl2_event_camera_context.csv` | 5426 rows. Event types: 5 x2343, 6 x1556, 2 x646, 4 x611, 1 x220, 12 x32, 13 x9, 3 x7, 9 x2. Frame index min 0, max 472, avg 49.05. | Use event-triggered rhythm for portal, impact, soldier attack, and kick beats. |
| `ash_cutscene_duration.csv` | 162 rows. Averages: combat-cinematic 4977.4 ms, cutscene 4593.3 ms, boss-cinematic 4325.2 ms, ultra-skill-cinematic 2881.8 ms. | Keep action clusters around 3-5 seconds, not disconnected 1-second prompts. |
| `pgr_camera_curve_shape_summary.csv` | 39 rows: `impact_recoil_return` 31, `micro_motion` 5, `static_or_empty` 3. | Use recoil as additive impact only, never as the main camera path. |
| Imported Rina package | `Rina_QuestStart_OriginalExtracted.anim` samples from 0.000 to 7.500 seconds at 1/60 second intervals. README says adjust model/pivot/facing, not the camera rig. | Use only for final third-person/back-view reveal, outside the protected first-person section. |

## Rechecked Project Assets

Use these only after validation, not as final proof by filename alone.

| Need | Candidate |
| --- | --- |
| Airstrike / bombing | `Effect_41_BombExplosion.prefab`, `Effect_41_BulletExplosion.prefab`, `Jet_04.fbx`, `Bomber_02.fbx` |
| Portal pre-spawn signal | `Effect_16_SpaceWarpPortal.prefab`, `Effect_16_WarpArrive.prefab` |
| Masked soldiers | `PF_Enemy_SciFiSoldier_GeneralDeck.prefab`, `PF_Enemy_SciFiSoldier_EliteDeck.prefab`, `PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab` |
| Soldier attack | `HBA_MeleeAttackWeapon.fbx`, `HBA_MeleeAttackForwardWeapon.fbx` |
| Sword handoff | `Sword_1.fbx` |
| Kick candidates | `CLazy@Attack_Kick_A.FBX`, `Lucy_Kick*.FBX` candidates; retarget validation required before use. |

## Locked Beat Direction

The existing `0.000-12.883s` capsule / first-person / hand-look section is not a
work area for this pass.

| Beat | Time Range | Job | Basis | Camera Rule |
| --- | --- | --- | --- | --- |
| 0 | `0.000-12.883s` | Preserve existing awakening, scan, hand-look. | Current project baseline. | No edit. |
| 1 | `12.883-14.250s` | Heaven-wide alarm cut before soldiers. | PGR hard cuts and quality guardrail. | Hard cut or very short blend; no face reveal. |
| 2 | `14.250-16.200s` | Airstrike impact chain across the stage. | Ash 3-5s chunks, PGR impact recoil. | Wide/establishing with additive zero-return impact. |
| 3 | `16.200-18.100s` | Portal breach signals before spawn. | GFL2 frame/event rhythm. | Medium-wide, staggered portals, no empty spawn. |
| 4 | `18.100-20.900s` | Soldier surge with attack/hit states. | PGR actor blocking / GFL2 event links. | Helmeted readable soldiers, staggered groups. |
| 5 | `20.900-23.300s` | Protagonist face-hidden interrupt kick. | PGR body-action grammar, kick fallback pending validation. | Low/side/shoulder framing; face hidden. |
| 6 | `23.300-26.100s` | Sword claim and melee resolve. | Workshop weapon insert template. | Hand/sword/boots/threat line, no glamour hold. |
| 7 | `26.100-33.600s` | Rina-style transition to back-view combat handoff. | Imported Rina 7.5s rig plus PGR handoff grammar. | Final 0.2s must align to gameplay camera. |

## Next Implementation Gate

Before editing Unity scenes, profiles, timelines, or setup code:

1. State the exact file/root to edit.
2. Confirm the edit is outside `0.000-12.883s`.
3. Confirm it does not regenerate `IntroGatePodPortPayload_Visuals`.
4. Prefer a new additive root or data asset.
5. Run validation immediately after the smallest meaningful change.
6. Commit only clean checkpoints.

If any step requires touching protected first-person/manual placement data, stop
and ask the user first.
