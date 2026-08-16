# Audition PV capture foundation

This folder owns the deterministic pre-edit capture contract and shot-specific capture tools. The
foundation itself does not mutate product scenes; each shot runner must lease and restore every
temporary scene, camera, HUD, audio, rendering, and presentation state it changes.

## Golden-source contract

- Unity Recorder: `com.unity.recorder` 5.1.6 public scripting API.
- Source: opaque, post-processed Game View PNG sequence (lossless sRGB 8-bit).
- Resolution and cadence: 2560x1440 at constant, capped 60 fps.
- Root: `D:/DimensionBrawl_PV/01_capture_video/PREEDIT_GOLD`.
- Frame layout: `<capture-id>/frames/<shot-id>/frame_<Frame>.png`.
- Provenance: `<capture-id>/capture_manifest.json`.

Each capture ID includes UTC time, a shortened Git commit SHA, and either `clean` or a shortened
dirty-state SHA-256. The full dirty-state SHA-256 in the manifest covers HEAD, porcelain status,
and the SHA-256 of every present dirty working file. Capture directories are direct children of the
fixed root, path traversal is rejected, and an existing manifest is never overwritten.

The manifest schema records Git, Unity, Recorder, URP, active pipeline, dimensions, fps, shots,
baselines, dependency file SHA-256 values, and test results. Each shot director supplies its own
shot and baseline entries and is the only component that should call `RecorderController` or render
a deterministic frame sequence.

The factory intentionally leaves the controller in Manual mode because the foundation does not own
shot timing. A gameplay shot director must switch each shot to Recorder 5.1.6
`SetRecordModeToFrameInterval(startFrame, endFrame)`; the end frame is inclusive and the manifest
validates `expectedFrameCount == endFrame - startFrame + 1`. It must also provide a resettable shared
presentation clock for effects driven by `Time.unscaledTime`, and toggle only the scene flow's
serialized combat HUD root instead of searching for or disabling arbitrary canvases.

## Implemented golden shots

### G04 — Station C33 wing deploy to C34 eye open

- Menu: `DimensionBrawl/Audition PV/Capture G04 Station C33-C34 Golden Source`
- Batch method: `DimensionBrawl.Editor.AuditionPV.AuditionPvStationTransitionGoldenCapture.RunBatchCapture`
- Batch requirements: graphics enabled, `-batchmode -noaudio`, never `-nographics`.
- Frames: `0..237` inclusive at 60 fps (`238` PNGs, 3.966667 seconds).
- Camera cut: C33 on `0..95`, C34 from frame `96`.
- Baselines: BL04 frame `66` (wing open), BL05 frame `178` (eye open).
- HUD: exact Station flow `combatHudCanvasGroup` is leased off and restored.
- Validation: exact dimensions and frame sequence, exclusive camera routing, black/magenta pixel
  sanity, C33 wing growth, C34 iris growth, Git dirty-state stability, dependency hashes, and
  manifest round trip.
- EditMode filter:
  `DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvStationTransitionGoldenCaptureTests`

The runner directly evaluates the authored product Timeline. It never saves temporary product-scene
state and reopens the Station scene in `finally`.

### G05 Station Phase 2 CrushNet perfect dodge

- Menu: `DimensionBrawl/Audition PV/Capture G05 Station Phase 2 Perfect Dodge Golden Source`
- Unattended GUI method:
  `DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunBatchCapture`
- Exact invocation:
  `Unity.exe -projectPath C:/Git/DimensionBrawl -executeMethod DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunBatchCapture -noaudio -logFile C:/tmp/DimensionBrawl-G05-GoldenCapture.log`
- Do not pass `-batchmode`, `-quit`, or `-nographics`. Recorder 5.1.6 `GameViewInput`
  requires a graphics-capable headful Editor, and this asynchronous runner exits the Editor itself
  only after capture finalization.
- Recorder interval: raw `0..197` inclusive (`198` PNGs). Recorder raw frame `0` is retained as
  resolution warm-up evidence; a collision-free two-phase rename maps raw `1..197` to logical
  `0..196` (`197` final PNGs, 3.283333 seconds).
- Baselines: BL03 is a byte-exact copy of logical frame `0`; BL06 is a byte-exact copy of logical
  frame `189` (3.150000 seconds). The real projectile impact remains logical frame `188`; frame
  `189` is the first rendered perfect-dodge screen-domain hero frame.
- HUD: the exact serialized Station Flow combat HUD root stays on. Ammo and summon energy must be
  full and bound to the same product-state objects used by gameplay.
- Product screen profile: the authored Station values stay unchanged at enabled, domain `.14`,
  invert `.015`, edge `.18`, glitch `.03`, and duration `0.42s`; only transient runtime cue state
  is cleared afterward.
- Gameplay proof: fresh Station load, real threshold transition and skip into Phase 2, CrushNet
  authored `PunishOverextend/CommitForward` movement during a 90-frame fixed-60Hz settle, windup
  frame `1`, fire frame `71`, queued dodge frame `186`, one real active-projectile impact at frame
  `188`, exactly one perfect dodge, unchanged player HP, and exact boss-pose restoration.
- Output proof: exact 2560x1440 PNG dimensions and contiguous counts, warm-up evidence, black and
  missing-shader-magenta sanity, rendered HUD evidence, frame `188` to `189` screen-effect pixel
  delta, byte-exact baselines, deterministic `PresentationClock`, Recorder auto-stop/flush after
  logical frame `196`, Git/dependency/Station-scene SHA-256 stability, runtime proof JSON, failure
  artifact on error, and manifest write/read validation.
- EditMode filters:
  `DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvStationPhase2PerfectDodgeCaptureTests;DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvStationPhase2PerfectDodgeGoldenRunnerTests`

The runner persists ownership and phase through `SessionState`, refuses dirty open scenes, opens a
fresh Station scene, and never saves product-scene state. On success or failure it stops and flushes
Recorder, releases gameplay/presentation leases, exits Play Mode, and reopens the Station scene
clean before writing the final result or exiting the unattended Editor.

### G08 Olympus Station boss death aftermath

- Menu: `DimensionBrawl/Audition PV/Capture G08 Station Boss Death Aftermath Golden Source`
- Unattended GUI method:
  `DimensionBrawl.Editor.AuditionPV.AuditionPvStationBossDeathAftermathGoldenRunner.RunBatchCapture`
- Exact invocation:
  `Unity.exe -projectPath C:/Git/DimensionBrawl -executeMethod DimensionBrawl.Editor.AuditionPV.AuditionPvStationBossDeathAftermathGoldenRunner.RunBatchCapture -noaudio -logFile C:/tmp/DimensionBrawl-G08-GoldenCapture.log`
- Do not pass `-batchmode`, `-quit`, or `-nographics`. This is an asynchronous, graphics-capable
  Recorder session; the runner requests its own bounded exit only after edit-mode finalization.
- EditMode filter:
  `DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvStationBossDeathAftermathGoldenTests`

The shot begins from a clean fresh Corridor scene. Product gameplay completes the tutorial, owns
the route and single-load seals, and dispatches its real `UITransitionHandoffService` transition.
The capture observes the pending handoff token and proves the dedicated Station
`FromHandoffPending` entry and terminal receipt chain; it does not reseal the run or call a scene
load API. The Station entry guide must reach `Released`. Pre-roll uses public, strictly non-lethal
damage for the Phase 1 threshold, public `TrySkipTransition`, a bounded Phase 2 wait, and a second
strictly non-lethal hit that leaves the boss at exactly 12 HP.

Before Recorder arm, capture ownership idempotently dismisses any active Phase 2 pressure actor
through its public product API; zero screens is a valid already-unobstructed state, while any observed
screen must be removed and the actual before/dismissed/after counts remain in runtime proof. Capture
then stores the exact `BossPressurePositionController.MovementEnabled` value and uses public
`SetMovementEnabled(false)` while a read-only physical sphere sweep centers the player by an authored
public movement step. Each adjustment is capped at 3 m, the actual sweep is remeasured after every
settle, and both cumulative requested movement and final planar displacement are capped at 4 m. The
boss `MovedTransform` position and rotation must remain exact from shot arm through the real impact,
and success/failure cleanup restores the saved movement value.

Recorder writes raw `0..360`. Two end-of-frame warm-ups precede the early-Update logical arm. Raw
frame `0` is retained as `evidence/recorder_warmup_raw_frame_0000.png`; a collision-safe remap maps
raw `1..360` to logical `f0..f359`, exactly 360 QHD60 frames. During logical recording the director
issues exactly one gameplay action: public `PlayerRangedBasicAttackAction.TryFire` at `f1`. The same
authored 12-damage, 24 m/s pooled projectile must move naturally and produce its physical impact,
one boss `Died`, and one `BossTerminal` clear at `f62`. Direct lethal damage/impact, boss-health
mutation, projectile pose or velocity writes, `PlayDeath`, cue calls, overlay calls, and result
publication are forbidden in the logical shot.

The death-anchored product aftermath must acquire all eight distinct
`BossTerminalAftermath` input leases at `f62`, preserve scale-one presentation after bounded lethal
hit-stop, and run its real camera, Phase 2-anchored VFX, non-silent death audio, and death motion.
The death animator remains terminal through the aftermath hero checkpoint. Product completion,
lease release, world freeze, result request, and result scene arrival are exact at `f218`; no early
freeze or result is allowed. The committed canonical facts and result summary must remain the same
instances presented by both overlay and result screen, which is fully interactive and stable at
`f246`.

Baselines are byte-exact logical-frame copies: BL10=`f62` death impact, BL11=`f116` aftermath hero,
and BL12=`f246` interactive result. The package also requires the 360-frame SHA-256 ledger, QHD
decode/dimension checks, black/magenta health, independent impact/aftermath/result pixel deltas,
result-surface color evidence, renderer-bounds/frustum pixel extent, exact Git/Unity/Recorder/URP
and dependency provenance, and exhaustive scene/global/input/event/Recorder restoration.

Pixel calibration is deliberately fail-closed while `PixelCalibrationLocked` is `false`. The first
honest clean same-HEAD take must validate all non-pixel runtime proof and preserve measured pixel
telemetry in `g08_capture_failure.json`, while publishing no manifest, baselines, success runtime
proof, or canonical frame ledger. An independent review must pin justified thresholds and their
boundary-negative tests before changing the sentinel to `true`. Only then can a new clean take
publish success artifacts. `capture_manifest.json` is the final fallible write and the immutable
terminal commit record; a valid committed manifest wins stale runner or terminal-fault state.
Failure cleanup is best-effort across every owned success artifact and records any cleanup fault in
the atomic failure artifact.

Edit-mode finalization is guarded by both `delayCall` and a persistent `EditorApplication.update`
watchdog. Import/update/play-mode transitions remain bounded waits; once the Editor is idle the
watchdog cancels any stale delayed callback and resumes the owned session directly, so failure
artifact publication and unattended exit cannot be lost to an `isUpdating` requeue.
The PlayMode transaction also recursively drives managed nested `IEnumerator` values itself while
leaving Unity-native waits intact. A nested preparation or cleanup exception therefore disposes every
active iterator, runs capture restoration and Recorder cleanup, records the failure, and reaches the
same bounded EditMode finalization/exit path.

## Validation

- Menu: `DimensionBrawl/Audition PV/Validate Capture Foundation`
- Batch method: `DimensionBrawl.Editor.AuditionPV.AuditionPvCaptureFoundationValidator.ValidateBatch`
- EditMode filter: `DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvCaptureFoundationTests`

The batch validator writes only `C:/tmp/DimensionBrawl-AuditionPvCaptureFoundationValidation.json`
and a unique temporary directory that it deletes. It does not write into the golden-source root.
