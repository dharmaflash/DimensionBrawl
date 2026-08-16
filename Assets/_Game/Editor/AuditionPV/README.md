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
read-only sweep uses the configured collider radius after applying the authored projectile prefab and
pool-root scales; the firing action's configured AimBolt prefab path/GUID, local `0.31` radius,
authored `0.28` prefab scale, pool-root scale, and event-observed physical world radius are proved
independently.
The boss `MovedTransform` position and rotation must remain exact from shot arm through the real
impact, and success/failure cleanup restores the saved movement value.

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
hit-stop, advance from the presentation clock at exactly 60 samples per second, and run its real
Phase 2-anchored VFX, non-silent death audio, death motion, and dedicated authored finisher camera.
Logical `f0..f61` are the causal gameplay handle, not approved hero footage. `f61` must still be the
exclusive gameplay camera with the exact player-facing `소환 에너지를 충전하세요` objective and
`AKAZA` boss label; `f62` is the unique hard cut to the dedicated finisher Camera and its manually
sampled 2.6-second Timeline. At `f61` the full boss envelope only has to intersect the gameplay
capture as a modest causal/identity handle; actor safe-area and size ratios are not hero-composition
gates, and the shoulder player may stay cropped. The normal Station path must never request the
older additive
`BossBarrageCameraCueDriver` fallback.
The death animator remains terminal through the aftermath hero checkpoint. Product completion,
lease release, world freeze, result configuration, and result scene arrival are exact at `f218`;
no early freeze or visible/interactable result is allowed. A one-sample-early
`AftermathHandoffImminent` signal may request the additive scene in its transparent, unconfigured
state so Unity completes that load on `f218`; the completion event owns the atomic freeze, input
release, configuration, and entrance start. The finisher has received exactly 156 Timeline samples
and remains exclusively live under result cover at `f218`. Its exact 0.46-second cover lease consumes
28 QHD60 samples, disables the finisher Camera, and restores the gameplay Camera at `f246`. Until that
successful handoff, the overlay owns the preload and
unloads it after cancellation, failure, or disable without publishing duplicate failure callbacks.
The result presentation-clock `.02 s` delay plus `.42 s`
entrance spans exactly 28 logical frames: `f218..f220` remain the transparent gameplay surface,
`f221` is the first visible result frame, and `f246` is fully interactive and stable. The committed
canonical facts and result summary must remain the same instances presented by both overlay and
result screen.

Baselines are byte-exact logical-frame copies: BL10=`f62` death impact (`HUDON`), BL11=`f116`
aftermath hero (`HUDOFF`), and BL12=`f246` interactive result (`AUTHOREDRESULT`). The package also
requires the 360-frame SHA-256 ledger, QHD
decode/dimension checks, black/magenta health, independent impact/aftermath/result pixel deltas,
result-surface color evidence, tight baked-skinned-vertex core-body projection, separate full-boss
renderer-bounds envelope/frustum telemetry, exact
Git/Unity/Recorder/URP
and dependency provenance, and exhaustive scene/global/input/event/Recorder restoration.

The bounded product change behind this acceptance contract is exact: Akaza terminal settle is
`0.90 s` around local pivot Y `0.72`, with drop `0.50`, back travel `0.22`, pitch `20°`, roll `62°`,
wing fold `52°`, and wing yaw `20°`. The dedicated FOV `44°` finisher camera starts at
`(0,1.45,5.35)` looking at `(0,-0.40,0)`, then settles at `(0,1.40,5.60)` looking at
`(0,-0.78,0)`. These authored values are setup/test pins; publication still depends on the runtime
baked-geometry and projected-axis proof below, and the visual sentinel remains false until a new
exact take is independently reviewed.

Composition telemetry is the exact ordered set `f61/f62/f116/f181/f246`. Core-body acceptance is
computed from baked vertices of exactly
`DB_AkazaPhase2Combined_BodySilhouette` and
`DB_AkazaPhase2Combined_FaceHairDetail`, transformed by each renderer's actual local-to-world
matrix; wing-inclusive renderer AABBs are retained only as a separate full-boss identity/envelope
measurement. At `f62`, `f116`, and `f181`, only the finisher Camera may be enabled and the tight core
body must remain fully inside the safe viewport. At `f62` its vertical height must be `0.25..0.40`.
Because the authored collapse may become horizontal, `f116` and `f181` instead require tight
screen-height-equivalent `max(width * 16/9,height)` (equivalently
`max(pixelWidth,pixelHeight)/1440`) of `0.25..0.40`, with no more than `0.05` difference between
those terminal samples. Exact projected authored `CHakazaA:hip_C`-to-`CHakazaA:head_C` endpoints are likewise evaluated in
screen-height-equivalent coordinates `(dx * 16/9, dy)`; that axis must be at least `0.08` units
long, change orientation by at least `35` degrees from `f62` to both terminal samples, and drift no
more than `8` degrees from `f116` through `f181`. Each tight core center may drift no more than
`0.08` viewport units from every other finisher sample. Peripheral wing/envelope clipping is
explicitly allowed so long as the full envelope still intersects the capture; it can never make a
cropped core pass. The player must either be fully inside at `0.25..0.32` height or fully outside the
finisher frustum; partial clipping fails. BL10 alone remains HUD-on, while BL11 and `f181` are
HUD-off. The green PocketClear marker must be unbound and inactive, the terminal
NoCross visual must be hidden from `f62` through the authored result, internal objective tokens and
`ARCHON PROXY` are forbidden, and `f246` must show the real CLEAR icon while the redundant
`Claer!_Text` placeholder remains inactive.

Pixel calibration is locked from the independently reviewed same-HEAD telemetry capture
`20260816t084414z_g08-station-boss-death-aftermath_g174d6862472a_clean` at Git
`174d6862472abf89b295749e37fdd1b280f97c49`. Its intentional calibration failure SHA-256 is
`e44e24e74c31f9ad6b6b1e0e6ef903ee10f7181cce5fd22afca0e1eda5defa9a`, and an independently
reconstructed 360-frame ledger hashes to
`66577dd2934bae05f50c9812026d5e46e98f9de45de23c3c00393e1196d24de1`.
That take published exactly 360 logical QHD frames, one QHD warm-up, state, and failure telemetry;
it published no manifest, baselines, success proof, or canonical ledger. Its classification is
`runtime/pixel calibration take; visual acceptance pending`; the historical `_clean` output-name
suffix does not make it a clean or approved golden.

`VisualCompositionAcceptanceLocked` is intentionally `false` for the first clean take after the
finisher-camera and visual-truth change. That take is visual-acceptance calibration-only: it must
complete the canonical runtime, pixel analysis, five-frame composition telemetry, finisher request/
sample/terminal/release proof, and provenance checks, then fail with the dedicated visual-acceptance
exception. Its failure package may retain raw/logical frames, runner state, and telemetry, but must
contain no manifest, baselines, runtime success proof, or canonical ledger. Only independent review
of that exact take may justify locking the sentinel and running a separate publishable golden take.

The reviewed measurements were black/magenta/max-frame-magenta=`0/0/0`, healthy=`100%`, impact
`13.542403/.299323`, death evolution `30.489848/.549518`, first visible result appearance
`f218->f221=8.468069/.328385`, and visible entrance
`f221->f246=35.305295/.852348`. The fixed raw-bottom result ROI
`(256,180,2048,1080)` at stride 4 measured `76,646` bright (all channels `>=200`), `630`
navy-luma (`(54R+183G+19B)>>8 <=75`), and `80,369` blue (`B>=120`, `B>=R+25`,
`B>=G+10`) samples at `f246`. Frame deltas use stride 4 and an RGB-sum changed cutoff of `24`.
Locked gates retain at least about 20% headroom and executable
boundary-negative tests reject every crossing, frame-pair drift, ROI drift, stride drift, and sample
count drift. `capture_manifest.json` remains the final fallible write and immutable terminal commit
record; a valid committed manifest wins stale runner or terminal-fault state.
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
