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
- Capture-only screen profile: enabled with domain `.42`, invert `.18`, edge `.48`, and glitch
  `.16`; the authored Station profile and runtime cue state are restored afterward.
- Gameplay proof: fresh Station load, real threshold transition and skip into Phase 2, CrushNet
  windup frame `1`, fire frame `71`, queued dodge frame `186`, one real active-projectile impact at
  frame `188`, exactly one perfect dodge, and unchanged player HP.
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

## Validation

- Menu: `DimensionBrawl/Audition PV/Validate Capture Foundation`
- Batch method: `DimensionBrawl.Editor.AuditionPV.AuditionPvCaptureFoundationValidator.ValidateBatch`
- EditMode filter: `DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvCaptureFoundationTests`

The batch validator writes only `C:/tmp/DimensionBrawl-AuditionPvCaptureFoundationValidation.json`
and a unique temporary directory that it deletes. It does not write into the golden-source root.
