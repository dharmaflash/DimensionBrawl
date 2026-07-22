# B0-3 Neutral One-Row Scene Adapters

Status: `IMPLEMENTED / VERIFIED`

Date: 2026-07-21 KST

## Outcome

B0-3 closes the scene-runtime seam between an authored one-row playable-stage definition
and the existing truthful terminal result core. A lean active scene can now admit one
`Entry|Terminal` segment, collect exact segment-zero facts, coordinate Clear or Fail,
recover a transient durable commit, present the result, and route Replay/Retry/Lobby
without copying the Olympus Corridor director, Station fact collector, or Station result
presenter.

This is a reusable runtime foundation, not a second product scene or catalog entry. The
test fixture authors the minimum scene dynamically, while a separate integration test
uses the production `OlympusStageClearOverlay` through the neutral
`IStageRunResultOverlay` boundary and reaches the additive `UI_StageClear` scene.

## Neutral component boundary

| Component | Responsibility | Execution boundary |
|---|---|---|
| `OneRowStageRunBootstrap` | Validate the complete scene contract, admit segment zero, and bind the exact encounter plus all three terminal adapter leases | `-10000`, one-shot `Start` |
| `OneRowStageRunFactAdapter` | Seal exact combat/player/UI sources, register the exact terminal coordinator, and record segment-zero facts and active time | `9000`, after admission and before normal encounter startup |
| `OneRowStageRunResultPresenter` | Validate terminal authority, commit or recover the exact result, obtain presentation acknowledgement, and release the combat surface once | `9250`, after the fact adapter |
| `StageRunCommitRecoveryPump` | Continue same-process durable commit recovery even if the scene presenter is disabled or unloaded | hidden `DontDestroyOnLoad` object created only while recovery is pending |
| `IStageRunResultOverlay` | Decouple result publication from a route-named UI implementation through pending/presented digests and success/failure callbacks | asynchronous presentation boundary |

The neutral files do not call `TryEnterPendingSegment`, mark a terminal guide, or depend
on Olympus, boss-barrage, Corridor, or Station components. They do reject any handoff
token or receipt found in a one-row context because such evidence would be fabricated for
this route shape.

## Exact authoring contract

Admission fails closed unless all of the following are true before a run exists:

1. the bootstrap is in the active scene and references an authored
   `PlayableStageDefinition` whose immutable snapshot has exactly one segment;
2. segment zero owns both `Entry` and `Terminal`, requires no tutorial fact, and creates no
   handoff evidence;
3. the scene contains exactly one live coordinated `CombatEncounterController`;
4. that encounter owns distinct, live, unbound, same-scene player and enemy health
   subjects;
5. one live fact adapter references the exact encounter, player health, combat-session
   surface, and the complete set of optional live player-action and summon sources;
6. one live result presenter references the exact encounter, fact adapter, shared
   combat-session surface, and an `IStageRunResultOverlay`; and
7. admission resolves the exact authored route digest and immediately binds the
   `EntryBootstrap`, `FactCollection`, and `ResultPresentation` leases.

An incomplete or duplicate noncanonical bootstrap disables only its own encounter before
combat can start. It cannot disable the exact encounter already admitted by the canonical
bootstrap.

## Runtime and authority sequence

```text
Validate complete scene
  -> admit one Entry|Terminal segment
  -> bind exact encounter + three adapter leases
  -> seal exact fact sources
  -> register exact coordinator and collect facts
  -> terminal resolution
  -> terminal topology rescan + fact seal
  -> durable result commit (or recovery pump)
  -> overlay TryShow
  -> exact digest acknowledgement
  -> mark Presented + dismiss combat surface once
  -> Replay / Retry / Lobby
```

The admission encounter, bootstrap, fact adapter, and result presenter are stable
reference identities. Unity destroyed-object fake-null behavior cannot silently transfer
authority. A replacement result presenter may take the lease only after the former owner
is actually destroyed and the context is already recovering, committed, or presented.

Runtime encounter ingress also checks the admission authority before coordinator creation.
A foreign or restarted encounter stops itself and cannot fault the canonical run. Once a
result is Presented, the exact encounter cannot restart.

## Fact and failure truth

The fact adapter seals its configured source set once. Normal `Update` work only checks
cached references and fixed arrays; it does not scan the hierarchy or allocate a new
collection each frame. The only full source scans occur during admission/authoring
validation and the one-shot terminal preparation. That terminal rescan detects a player
action or summon source activated after admission instead of silently omitting its facts.

Loss of the exact fact adapter or one of its sealed sources closes one typed
`TerminalFactAdapterLost` abort. Loss of the exact result presenter closes one typed
`TerminalResultPresenterLost` abort. If either owner disappears before a coordinator is
bound, the abort records `NotBoundBeforeTerminalCoordinator`; if a valid terminal event
has already closed its authority, the failure records `TerminalAuthorityInvalidated`
instead of misreporting a generic closure fault. Every adapter-loss path stops the
encounter so ownerless combat cannot continue.

## Commit and presentation recovery

A transient durable-store read/write disagreement moves the exact run into
`CommitRecoveryPending`. The recovery pump makes five fast attempts, then retries at a
bounded low frequency until the same-process context either commits or leaves recovery.
It can publish a result that another concurrent/manual recovery already committed, and
isolates subscriber exceptions so cleanup still runs.

Presentation is a separate acknowledged boundary. The presenter accepts success only when
the overlay's presented digest, the active run, the committed summary, the runtime
presentation snapshot, and the presentation audit all identify the same result. A failed
or thrown `TryShow` is retried; after the initial burst, retries continue at low frequency.
A pending watchdog recovers a lost callback, while a spurious success for another digest
is ignored. The combat-session surface is dismissed at most once for the accepted digest.

## Acceptance evidence

The focused B0-3 matrix covers Clear/Fail, Replay/Retry/Lobby, exact leases, missing or
swapped sources, duplicate bootstrap, pre-coordinator owner loss, terminal owner loss,
foreign coordinator and encounter ingress, terminal topology rescan, callback loss,
spurious acknowledgement, commit-recovery races, presenter disable, scene-owner loss, and
the no-two-row-advance rule.

Verification ledger:

- `C:\tmp\DimensionBrawl-B0-3-Compile5.log`: Unity batch compile exit `0`;
- `C:\tmp\DimensionBrawl-B0-3-OneRowFocused8.xml`: `20/20` passed;
- `C:\tmp\DimensionBrawl-B0-3-AuthoredStageClear.xml`: `1/1` passed through the
  production result overlay into `UI_StageClear`;
- `C:\tmp\DimensionBrawl-B0-3-StageRunRoute-All2.xml`: `56/56` passed; and
- `C:\tmp\DimensionBrawl-B0-3-CoreRegression.xml`: final integrated `255/255` passed,
  covering route, coordinator/result, canonical UI, Olympus full flow, and summon/energy
  suites after the production-boundary test was added.

`C:\tmp\DimensionBrawl-B0-3-PlayableStageValidator.log` also reports terminal mutation
inventory `PASS` and preserves the accepted Olympus identities:

- terminal policy digest:
  `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`;
- route digest:
  `878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0`; and
- result/progression join digest:
  `d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71`.

## Explicit deferrals

B0-3 deliberately does not add a second catalog entry, build scene, or product-authored
neutral arena. Those belong after the infrastructure order below is complete. It also
does not add a Wave/objective language, reward economy, server recovery, or cold-process
result reconstruction.

Recovery is same-process only. The pump can survive presenter disable and scene unload,
but an application restart cannot reconstruct an unpersisted presentation request. The
bootstrap also requires its scene to be active and follows the current single-load
admission contract; additive preload before activation is not auto-admitted. Unexpected
scene takeover during recovery remains fail-closed and should return through normal
dispatch/reload. If every authored presenter/UI reference is destroyed, a replacement or
reload is required.

The existing Olympus overlay now proves the interface seam; extracting a distinct neutral
visual skin is a later product-content decision, not a reason to duplicate result logic.

## Decision and next gate

B0-3 is complete. B0-4 remains the next bounded infrastructure gate:

1. project more than one immutable catalog entry without changing accepted Olympus data;
2. bind Stage Select cards to their exact entry scene and selected stage ID;
3. make validation enumerate every catalog/result/progression row; and
4. make build readiness walk each selected route instead of appending Station.

The first compact second scene is B1-1, and the actual two-card product presentation is
B1-2. Do not author that scene ahead of the B0-4 selection/build seam.
