# Scene Graphics Stability Audit

Date: 2026-07-18

## Incident classification

Opening `UI_OlympusChapterNarrativeReview` from an Editor session that was already in Play
Mode with both Domain Reload and Scene Reload disabled completed scene deserialization and
integration, then failed on the first D3D12 present. The native signature was
`D3D12Fence::Wait -> QueuePresent -> 0x887A0006 device removal`. The log explicitly ruled
out local and non-local graphics-memory exhaustion, and contained no managed, Timeline,
audio, TMP, or scene-deserialization exception.

The same scene subsequently opened and passed its independent setup verifier in a D3D11
batch Editor process. Historical Windows events also contain NVIDIA display-driver resets
outside this one scene. The incident is therefore treated as a Windows Editor D3D12/device
stability failure with a scene-switch trigger, not as a product runtime authority or a
proven corrupt scene.

## Applied safety policy

- Windows Standalone graphics APIs are explicit and D3D11-only. This also gives an
  unforced Windows Editor launch the stable API selected from Player Settings.
- Android graphics APIs and the Android build profile are not changed.
- Enter Play Mode Options remain explicit with neither disable flag set, restoring full
  Domain and Scene reloads.
- Double-click opening of every scene under `Assets/_Game/Scenes` is blocked while the
  Editor is entering, running, or leaving Play Mode. Scripted Edit Mode validators are not
  intercepted.
- The narrative review diorama's one soft-shadow point light is now shadow-free. Realtime
  shadows do not contribute to the ST-01 UI/lifecycle acceptance claim.
- `SceneGraphicsStabilityAudit` opens every game scene in one D3D11 process, rejects missing
  scripts or simultaneous active listeners, and forces a camera render for every scene that
  owns an active camera. Non-VFX scenes add a synchronous readback; VFX scenes submit the
  render without readback, and the intentional overlay-only result scene is recorded as a
  structural skip. It also runs all five review setup verifiers and the canonical playable
  stage validator.

## Operator rule

Stop Play Mode and wait for Edit Mode before opening another scene. If Unity reports D3D12
for this project, close it and restart with `-force-d3d11` before inspecting scenes.

## Evidence outputs

The latest machine-readable and human-readable audit outputs are written outside the
repository:

- `C:/tmp/DimensionBrawl-SceneGraphicsStabilityAudit.json`
- `C:/tmp/DimensionBrawl-SceneGraphicsStabilityAudit.md`

The audit is a bounded Editor graphics smoke test. It does not replace PlayMode route,
combat, lifecycle, mobile render-budget, or device testing.

## Verification record

- D3D11 settings setup and compile: PASS, Unity exit `0`.
- Unforced Editor restart: PASS, selected `Direct3D 11.0` from committed Player Settings
  without a command-line graphics override.
- All-scene audit: PASS for all 11 game scenes. Ten scenes submitted a camera render; eight
  non-VFX scenes also completed a synchronous readback, both VFX combat scenes completed
  render-only smoke, and the overlay-only Stage Clear scene recorded its intentional skip.
- Missing scripts: `0` across all 11 loaded scenes.
- Active AudioListeners: no scene exceeded one during the Edit Mode audit.
- Review/canonical validators: all five review setup verifiers and the playable-stage
  validator passed in the same process.
- Narrative visual QA after removing the point-light shadow: `15/15 PASS`. The five
  1920x1080 state captures were also visually inspected; composition, lighting, subtitles,
  and transition-state UI remained readable.
- Previously sealed ST-01 regression selection inside the full run: `137/137 PASS`.
- Full PlayMode assembly observation: `419/426 PASS`. Five order-sensitive hot-path tests
  then passed `7/7` in a clean isolated process. Two independent pre-existing product debts
  remain reproducible in isolation and are outside this graphics-stability change:
  `OlympusStationCombatStage` reports 20 runtime callbacks against a reviewed budget of 17,
  and `PlayerSkill1Action` still declares an `Update()` method rejected by the presentation
  idle-loop policy test. Neither failure enters a changed file, review scene, graphics
  setting, or scene-open guard.
- No D3D device-removal, GPU out-of-memory, native crash, or Unity crash report was produced
  by the D3D11 setup, unforced-selection, all-scene, visual-QA, full-regression, or isolated
  verification processes.
