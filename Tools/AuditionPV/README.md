# PV60 capture orchestrator

`Invoke-AuditionPv60Capture.ps1` owns the exact 19-run pre-edit capture sequence. A named mutex is
held for the whole invocation. It permits only one Unity Editor process at a time, blocks After
Effects, Media Encoder, aerender, and ffmpeg, and uses a first-round family smoke before the
remaining repetitions:

1. G04, S050 take 1, G08, G07, G06, S030, City.
2. G04, G08, G07, G06, S030, City take 2.
3. G04, G08, G07, G06, S030, City take 3.

This order is deliberate: the smallest batch capture comes first, followed by the unique
low-angle, finisher, pattern, ultimate/counter, city-combat, and largest City risks before any
family repetition.

Every Unity invocation includes `-pv60ApprovedEvidence`. City, S030, S050, G06, G07, and G08 are
graphics-capable headful runs without `-batchmode`, `-quit`, or `-nographics`. G04 alone uses
`-batchmode -noaudio`, still with graphics and without `-quit` or `-nographics`. S050 is pinned to
`-s050TakeOrdinal=1`. The independent golden schedule/contract SHA-256 is
`10ff100cb0e40b854967e159d9122028e5c67bc663d6e4c182aefdb47557f538`; the 19 runs require exactly
37 evidence-receipt checks.

The live contract rejects override drift from these exact values before capture:

- Project: `C:\Git\DimensionBrawl`
- Output: `D:\DimensionBrawl_PV\01_capture_video\PREEDIT_GOLD`
- Unity: `C:\Program Files\Unity\Hub\Editor\6000.3.5f2\Editor\Unity.exe`
- Unity provenance: `6000.3.5f2 (3fa8bc678cb0)`, Recorder `5.1.6`, URP `17.3.0`, pipeline
  `Assets/Settings/PC_RPAsset.asset`

Before starting a live sequence, commit the orchestrator and all capture code, then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File `
  C:\Git\DimensionBrawl\Tools\AuditionPV\Test-AuditionPv60CaptureOrchestrator.ps1

powershell -NoProfile -ExecutionPolicy Bypass -File `
  C:\Git\DimensionBrawl\Tools\AuditionPV\Invoke-AuditionPv60Capture.ps1 `
  -PinnedHead <40-character-clean-HEAD> -DryRun

powershell -NoProfile -ExecutionPolicy Bypass -File `
  C:\Git\DimensionBrawl\Tools\AuditionPV\Invoke-AuditionPv60Capture.ps1 `
  -PinnedHead <40-character-clean-HEAD>
```

Fresh mode accepts only a missing or empty `RunRoot`; it never reuses a prior state, journal,
report, or log. `StatePath` is always the canonical `RunRoot\capture_state.json`. Resume only the
same pinned sequence with `-Resume`: state, report, and every journal event must share one sealed
run identity, exact paths, HEAD, and contract SHA. Every saved run row and any fixed log/Unity
argument vector must match the 19-run golden schedule before `-DryRun` can return ready or live
resume state can be assigned. Journal sequence/count must exactly match state, while the report's
monotonic event snapshot may safely trail an interrupted run. `-DryRun` performs these fresh/resume
conflict checks but creates no directory or file.

A valid terminal manifest can recover an interrupted `running` state. A retained failure output or
ambiguous new-output set is never deleted or retried automatically. Failure discovery checks the
capture root and direct `evidence` directory for exact and `yyyyMMddHHmmssfff`-suffixed artifacts,
including S030 and S050. State, the append-only event journal, unique Unity logs, and the report live
under `C:\tmp\DimensionBrawl-PV60-Capture-Orchestrator` by default. Before marking completion, the
orchestrator revalidates all 19 outputs, all 37 receipts, manifest provenance, required evidence,
failure absence, unique capture IDs/outputs/manifests/logs, and exact state/run/argument metadata.

The verifier reads bounded JSON and hashes evidence files using streaming `Get-FileHash`. It never
pixel-decodes a PNG or loads a full PNG into a PowerShell byte array. All Git reads use
`--no-optional-locks`, so DryRun cannot refresh or lock the index. The Pester-free test parses the
script, exercises all 15 fail-closed mutations and the real DryRun entry point, fingerprints the
probe RunRoot/Git index/tool files for zero-write verification, and independently guards the main
terminal-audit call, its real output verifier, and its exact 19-iteration loop.
