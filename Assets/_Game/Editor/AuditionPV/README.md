# Audition PV capture foundation

This folder owns the deterministic pre-edit capture contract. It deliberately does not load scenes,
drive gameplay, move cameras, or start a recording session yet.

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
baselines, dependency file SHA-256 values, and test results. A future shot director supplies the
actual shot and baseline entries and is the only component that should call `RecorderController`.

The factory intentionally leaves the controller in Manual mode because this foundation does not own
shot timing. The future shot director must switch each shot to Recorder 5.1.6
`SetRecordModeToFrameInterval(startFrame, endFrame)`; the end frame is inclusive and the manifest
validates `expectedFrameCount == endFrame - startFrame + 1`. It must also provide a resettable shared
presentation clock for effects driven by `Time.unscaledTime`, and toggle only the scene flow's
serialized combat HUD root instead of searching for or disabling arbitrary canvases.

## Validation

- Menu: `DimensionBrawl/Audition PV/Validate Capture Foundation`
- Batch method: `DimensionBrawl.Editor.AuditionPV.AuditionPvCaptureFoundationValidator.ValidateBatch`
- EditMode filter: `DimensionBrawl.Editor.AuditionPV.Tests.AuditionPvCaptureFoundationTests`

The batch validator writes only `C:/tmp/DimensionBrawl-AuditionPvCaptureFoundationValidation.json`
and a unique temporary directory that it deletes. It does not write into the golden-source root.
