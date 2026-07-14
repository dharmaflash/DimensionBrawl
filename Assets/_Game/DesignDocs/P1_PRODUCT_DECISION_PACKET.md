# P1 Product Decision Packet

## Status

- Drafted: 2026-07-14
- Status: D1-D3/D4a approval-ready recommendation; D4b provisional feasibility contract; analysis only; not yet a product decision
- Source root: `\\DESKTOP-69817L3\ArkData\SubcultureGameData`
- Local baseline: current DimensionBrawl workspace, including uncommitted stabilization and optimization work
- Production gate: P0 remains `full STALE / natural STALE / retry MISSING / lobby MISSING`
- Roadmap source: [Subculture Dataset Gap Roadmap](SUBCULTURE_DATASET_GAP_ROADMAP.md)
- Contract companions: [Playable Stage Reference Spine Spec](PLAYABLE_STAGE_REFERENCE_SPINE_SPEC.md) and [Stage Run and Result Contract Spec](STAGE_RUN_RESULT_CONTRACT_SPEC.md)

This packet turns the remaining P1-0/P1-A product choices into one bounded approval surface and records D4b as a separate feasibility-gated engineering candidate. It does not authorize code, scene, asset, progression, reward, or economy work. The product recommendations can be approved while P0 is blocked, but production authoring and implementation remain behind the P0 route/navigation gate and D4b-dependent arbitration work remains behind its inventory/proof gate.

## Executive Recommendation

| Decision | Recommended product value | Why this is the smallest coherent choice |
|---|---|---|
| D1 — logical stage and route identity | approve `OLYMPUS-INVASION-01`, revision `1`, ordered segments `corridor_intro_tutorial -> station_entry_combat`, and the three typed terminal actions below | the playable operation spans two scenes; no current UI ID, scene name, or scene-segment definition truthfully owns the whole route |
| D2 — result action availability | offer Replay plus Lobby after Clear, and Retry plus Lobby after Fail; defer Stage Select and Next Stage | both outcomes need re-entry and escape, but clear replay and failed-run retry must remain distinct intents for later progression/reward policy |
| D3 — first Fail presentation | promote the additive product result UI into one outcome-aware shared result shell; Fail uses Retry primary and Lobby secondary | one committed-summary consumer avoids another result owner while preserving a visible, explainable failure state |
| D4a — simultaneous terminal product policy | use authoritative causal-event order rather than callback arrival; if boss death and player down are both valid in the same causal terminal event, Clear wins and the player-down fact remains; an already closed lower independent player-terminal event is not reopened | boss defeat is the current canonical Station clear trigger while survival remains a truthful result/mastery fact; no coordinator/token implementation is approved by this row |
| D4b — arbitration engineering candidate | provisionally investigate pre-mutation admission, root ordering/tokens, synchronous work drain, and two-subject finalization | feasibility-gated only; the complete mutation-path inventory and proof of concept must pass before this mechanism can freeze or authorize P1-A implementation |

These are local product recommendations. PGR and HI3 support separating static identities and authoring fields, while selected client materials support separating result, retry, and exit ownership. None of the bounded inspected sources proves which buttons DimensionBrawl should offer or how its double terminal should resolve.

## Approval State Versus Execution Gate

| State | May happen now? | Meaning |
|---|---:|---|
| recommendation ready | yes; this packet is that artifact | the four choices are bounded and evidence limits are explicit |
| product review/approval | may happen now while P0 evidence is stale/missing | D1-D3/D4a may be accepted as planning vocabulary, but no approval has yet been recorded and no runtime or asset parity is implied |
| P1-0 implementation freeze | no; wait for the P0 terminal-owner and navigation gate | the approved fields are authored on the final route shell, validated, and given a production revision/digest |
| P1-0 phase exit | no; after authoring and validators pass | downstream P1-A may consume the immutable route contract |

“Approved recommendation,” “implemented contract freeze,” and “P0 PASS” are different states. Product review need not idle behind the unavailable Unity rerun, while production work must not bypass it.

## Current Local Facts

| Fact | Current evidence | Decision consequence |
|---|---|---|
| the selected catalog row now selects the runtime combat route | `StageSelectScreenPresenter` resolves the selected `UIStageCatalog.StageEntry` and forwards its scene and loading card to the router; current catalog IDs still share one Corridor definition | the selected catalog entry can resolve and launch one canonical logical route, but its catalog ID never owns that route; physical segment composition still needs the typed playable-stage contract |
| the route spans Corridor and Station | Corridor hard-codes a single-load to Station; the current stage definition owns only Corridor | the product identity must compose two physical segment definitions |
| Clear has a product surface; Fail does not | `OlympusStationCombatResultPresenter` observes only `CombatEncounterController.Won`; `Failed` produces only the encounter fail marker on that path | P1-A needs a committed-summary Fail presentation, not another raw callback subscriber |
| Station has one product combat-session surface | the legacy Review result owner is retired; `CombatSessionOverlayPresenter` owns pause, settings, and failure while the clear UI owns victory actions | P1-A still needs one typed executor for the remaining product actions, without duplicate result ownership |
| the clear UI Lobby action is truthfully named | `StageClearScreenPresenter.lobbyButton` and `HandleLobbyClicked()` route to `UI_Lobby`, matching the visible `로비로` copy | the typed contract must preserve this Lobby meaning rather than inventing a nonexistent next-stage action |
| terminal outcome is callback-order dependent | the first `Died` handler immediately changes `CombatEncounterController` from Running and suppresses the other handler | a product tie policy cannot be implemented after the existing state collapse |
| no authoritative terminal-resolution admission or queue exists | canonical damage producers call `CombatHealth.TryApplyDamage` directly, `DamageInfo` carries no root token, and `Died` publishes synchronously | provisional D4b requires a new P1-A encounter-owned admission/order/queue seam; documentation must not imply the current callbacks already supply a root sequence or epoch |

The 2026-07-14 11:10 full route PASS and 10:38 natural PASS remain historical only. Station was saved at 11:15:21, and neither result button has been executed. This packet does not convert those reports into current P0 proof.

## Evidence Strength and Boundary

Grades in this packet mean:

- **A — local executable or directly inspected runtime structure:** sufficient to describe the current gap, but not automatically a product preference.
- **B — source-linked static/client structure:** sufficient to support separation of contracts or authoring fields, not shipped runtime behavior.
- **C — analogous vocabulary only:** useful as a rejection or validation checklist.
- **Missing:** the preserved material cannot decide the policy.

| Source | Directly supports | Grade | Does not support |
|---|---|---:|---|
| DimensionBrawl current code/assets | two-scene route drift, split terminal owners, direct scene loading, callback-order terminal collapse | A | external market convention or player preference |
| PGR stage/GuideFight material | stage identity separate from display/loadout/story hooks; pre/next, result/reward, and policy-like reboot field presence as static authoring concerns | B | decoded reboot consumption, route revision, multi-scene ownership, button execution, new-run semantics, Fail action set, or double terminal |
| HI3 stage/result material | one stage identity joining entry/Lua, challenge, prerequisite, and restriction authoring; score/reward/display fields and lose-description references kept separately | B | per-run result facts, authoritative outcome commit, result UI order, retry/exit action, multi-scene route, or double terminal |
| Reverse: 1999 client flow | result recording before end-fight publication and separation of result/progress/bonus categories | B | server durability, atomic settlement, receipt behavior, or result button policy |
| Neural Cloud client flow | result, temporary failure-result UI, reward display/claim, retry availability, restart, and exit as separate concerns in an older decompiled regional client snapshot | B- | decompiler correctness, final shipped failure UI, server idempotency, free retry, exact target, or DimensionBrawl button availability |
| Ash Echoes static data | fail-retry and hidden-result policy fields separate from directed stage links | B- | actual click execution, retry cost, save transaction, or result ownership |
| Wuthering Waves tutorial data | success/failure/skip/break plus authored in-dungeon reset and cleanup-related fields as attempt-time concerns | B | executed cleanup behavior, post-result Retry, or terminal navigation |
| FGO quest policy data | `afterClear: repeatLast` as repeat-after-clear policy distinct from failure retry | B | failure Retry, automatic Next, Lobby, or client result flow |
| bounded peer sources inspected for this decision | no raw simultaneous player/boss terminal trace, authoritative epoch, or tie policy was found | Missing | Clear-wins, Fail-wins, draw, timing window, epoch, or callback precedence |

Evidence discipline:

- PGR policy-like reboot field presence supports keeping retry semantics explicit in local authoring; without decoded consumption it does not prove a shipped retry policy or choose Corridor, Station, cost, or availability.
- HI3 lose-description references and score/reward/display tables support separate failure copy and result-related authoring/display fields; they do not prove per-run facts or a shared/dedicated result screen.
- FGO `repeatLast` is not failure retry, and `enableFollowQuest` is not automatic Next navigation.
- Wuthering in-dungeon reset is not post-result retry.
- Limbus post-battle hooks are not failure, retry, Next, or Lobby actions.
- No bounded peer source inspected here is promoted into evidence for simultaneous-terminal precedence.

## D1 — Logical Stage and Route Identity

### Recommended freeze

| Concern | Recommended value | Ownership rule |
|---|---|---|
| logical playable stage ID | `OLYMPUS-INVASION-01` | belongs only to the final `PlayableStageDefinition` route shell |
| route revision | `1` | changes when ordered route, action, or terminal-resolution semantics change |
| Corridor segment ID | `corridor_intro_tutorial` | references the existing Corridor scene-segment definition |
| Corridor definition ID | retain `OLYMPUS-CORRIDOR-INTRO-COMBAT-01` | remains a physical scene-segment definition, not the product stage ID |
| Station segment ID | `station_entry_combat` | covers the Station entry guide and canonical encounter |
| Station definition ID | author `OLYMPUS-STATION-COMBAT-01` | owns Station scene path/binding and later Station-local anchors/spawns/ports |
| failed-run retry action | `olympus-invasion.retry`, kind `Retry`, target `OLYMPUS-INVASION-01`, entry at Corridor, allowed only for Fail | a new run starts only after successful canonical Corridor entry |
| clear replay action | `olympus-invasion.replay`, kind `Replay`, target `OLYMPUS-INVASION-01`, entry at Corridor, allowed only for Clear | keeps manual replay after victory distinct from failure recovery and later repeat/economy policy |
| lobby action | `olympus-invasion.to-lobby`, kind `UIRoute`, target `UIRouteId.Lobby` | navigation only; it is not outcome proof or a post-clear hook |
| terminal outcome semantics | authoritative causal root order; same-root boss death plus player down commits Clear while retaining the player-down fact; a closed lower independent player-terminal root is not reopened; no draw/callback-wins/grace window | this is the recommended D4a product value; if approved, it is deep-snapshotted at entry and participates in route revision/digest |
| terminal arbitration mechanism | provisional D4b candidate: pre-mutation admission, root sequence/token, synchronous queue, two-subject finalization, and coordinator lifecycle | not part of this product freeze; P1-0 must inventory every mutation path and prove exclusive coverage/closure before D4b can freeze or P1-A can implement it |

The final `PlayableStageDefinition` is created once in P1-0 as the minimal route shell. P1-B fills optional template, result, progression, briefing, and cinematic joins on that same asset. It must not create a parallel identity record or reinterpret an active run from a later asset version.

### Options considered

| Option | Benefit | Cost/risk | Verdict |
|---|---|---|---|
| approve the proposed logical identity | gives entry, handoff, result, retry, and later progression one product key | requires route-shell authoring and migration from constants | **Recommend** |
| reuse Corridor `StageDefinitionProfile.stageId` | no new ID | falsely claims a two-scene product stage is one physical segment and leaves Station ownerless | Reject |
| reuse `story_v1_training_route` | appears close to selection UI | selection now forwards that row's raw scene route, but a second row aliases the same definition and neither carries the two-segment logical playable-stage identity | Reject |
| reuse scene name or `UIRouteId.Combat` | familiar existing value | scene identity and UI-screen domain cannot express route revision or two ordered segments | Reject |
| defer all identity decisions until P1-B | avoids an early approval | P1-A would be forced back to constants or an interim record, creating migration debt | Reject |

Approval of D1 freezes contract vocabulary, not current production parity. P0 still has to prove the retained current clear-screen re-entry button reaches Corridor and Lobby reaches the lobby before the route shell is authored; P1-A later maps that Clear control to Replay.

## D2 — Outcome-Filtered Terminal Actions

### Recommended first action matrix

| Typed action | Clear | Fail | First-slice presentation guidance | Reason |
|---|---:|---:|---|---|
| `olympus-invasion.replay` | allow | do not allow | Clear: `다시 하기` | manual replay after Clear starts a new run at Corridor without becoming failure recovery |
| `olympus-invasion.retry` | do not allow | allow | Fail: `재도전` | failed-run retry starts a new run at Corridor without inheriting clear-only policy |
| `olympus-invasion.to-lobby` | allow | allow | `로비` | both outcomes need a non-destructive escape path; the action is navigation, not completion proof |
| Stage Select | do not author | do not author | none | the pre-run selected row now controls a raw scene route, but no typed post-result Stage Select target exists and both rows alias one definition; add only after canonical target and parity tests exist |
| Next Stage | do not author | do not author | none | no next playable-stage contract exists; Lobby must not be mislabeled as Next |
| pre-result active-run restart | not a result action | not a result action | later policy | P2-A authors `activeRunRestartPolicy`; P1-A alone owns request validation, latch, sealed dispatch, later diagnostic abort, and actual dispatch; P2-B supplies requests/closure receipts only |

Action presence never implies availability. Under the recommendation, Replay serializes `allowedOutcomes = { Clear }`, Retry serializes `allowedOutcomes = { Fail }`, and Lobby serializes `allowedOutcomes = { Clear, Fail }`. The committed summary projects only matching action IDs, and the presenter never adds a button because a route target happens to exist.

Labels, visual priority, and order are not `StageRouteActionRef` semantics. The shared result view owns a `ResultActionPresentation` mapping keyed by `(outcome, actionId)` with `labelKey`, `role`, and `displayOrder`; it may not enable an action absent from the committed summary. P1-A's first view profile supplies the mapping above, and P1-B's result-definition join references that same profile rather than copying it. Missing or duplicate presentation entries disable the affected control and fail validation. Copy/order changes do not change route revision, while action ID/target/availability changes do.

Required invariants:

1. Replay after Clear and Retry after Fail both dispose the old presented context and create a new run only at canonical Corridor entry, but preserve their distinct action ID/kind.
2. Lobby after either outcome disposes the old context and dispatches only the sealed `UIRouteId.Lobby` payload.
3. Fail cannot dispatch post-clear story, progression success, mastery success, or rewards merely because Lobby is allowed.
4. Double-click, Replay/Retry/Lobby race, stale revision/digest, missing summary, and load failure cannot select a second action.
5. The review HUD either exposes no independent result controls on the canonical route or delegates the same summary and executor.

Revision 1 Replay and Retry are manual, no-entry-cost result actions. “Immediate failed-run retry” means the committed Fail surface offers Retry without a progression/reward write or extra cost gate; it does not auto-navigate or bypass summary commit. A future automatic repeat, entry cost, fast clear, or reward-altering repeat policy must use the existing clear-only Replay identity plus a versioned policy, or replace it with a new typed action under a new route revision; it never silently overloads failed-run Retry.

Why Lobby is also offered after Fail: forcing Retry creates a trap, while immediate scene exit without a committed Fail hides useful cause and run facts. A typed Lobby action gives the player control without pretending the run cleared. The peer datasets support separating exit/retry concerns but do not choose this policy; this is a local usability and ownership decision.

## D3 — First Fail Presentation

### Options

| Option | Player effect | Implementation cost | Regression/ownership risk | Verdict |
|---|---|---:|---:|---|
| outcome-aware shared additive result shell | visible reason, truthful facts, Retry/Lobby choice, consistent interaction | medium | medium-low if it consumes only committed summaries | **Recommend** |
| dedicated Fail scene/surface | maximum visual separation | medium-high | high: another scene, presenter, route binding, and parity matrix | Hold until content proves need |
| immediate Retry after Fail | fastest loop | low initially | high: hides cause, removes choice, and can create a run before result disposal is proven | Reject |
| 3D fail marker only | preserves current minimum | low | high player-flow gap: no product action surface or committed-summary proof | Reject |
| retain the review HUD as Fail owner | reuses existing copy | low initially | critical: second encounter/result owner and Station-reload Retry conflict | Reject |

### Recommended first slice

The existing additive product result UI becomes an outcome-aware shell. Its current scene/class names may migrate later; they are not contract identity. It opens only after `RunResultSummary` reaches Committed.

For Clear, the shell may keep clear-specific art/audio and renders total/combat time plus the first two truthful summon-identity proofs. For Fail, it must use distinct title, accent, audio policy, and copy; clear BGM, success copy, reward reveal, mastery success, and post-clear hooks remain absent.

Minimum Fail projection:

- title: explicit operation failure/player-down language;
- reason: typed failure reason rendered as copy, never parsed back into logic;
- facts: combat time, player-down state, and one truthful next-attempt hint derived from committed facts or authored failure copy;
- actions: the Fail `ResultActionPresentation` projects Retry primary and Lobby secondary from the two offered action IDs;
- safety: missing/invalid committed summary produces diagnostic-safe copy with both actions disabled.

The product shell can share layout and executor while using outcome-specific presentation assets. Shared shell does not mean identical Clear/Fail audiovisual treatment.

## D4 — Same-Epoch Player/Boss Terminal Arbitration

### D4a — Recommended product-policy freeze

Freeze only these player-visible semantics at product review:

- authoritative causal root order, never callback/subscriber arrival, decides which independent terminal event resolves first;
- when boss death and player down are both valid final states inside one same-root terminal epoch, commit Clear and retain the player-down fact;
- a player-only terminal result already closed in a lower independent root is not reopened by a later boss death; and
- no draw outcome, callback-wins policy, or frame/millisecond grace window exists in revision 1.

This approves the outcome policy, not the coordinator class, token shape, queue algorithm, or proof that every current mutation path can use them.

### D4b — Provisional technical contract and feasibility gate

The current candidate mechanism is one `SameTerminalResolutionEpoch` window owned by a new authoritative Station `EncounterTerminalResolutionCoordinator`:

1. A canonical combat producer must call `CanonicalCombatRootAdmission` before any Player/Boss terminal-state mutation and before any `Damaged`, `Died`, or terminal-observer callback can run. The coordinator assigns a unique monotonic `RootAdmissionSequence`; a damage callback, terminal callback, presenter, or fact collector may never create a root admission.
2. Lower `RootAdmissionSequence` is the revision-1 authoritative causal order. Independent roots are intentionally not simultaneous: the lower sequence resolves completely first even if both were admitted during one rendered frame. Reversing the root sequence may therefore change Clear versus Fail by design; reversing callbacks while preserving the sequence may not.
3. When the next admission becomes active, the coordinator issues one `RootResolutionToken` and one `EncounterTerminalEpoch`. Later independent admissions remain ordered pending records without mutation authority; they receive a token only if the run remains active after every lower sequence closes.
4. Every mutation capable of changing a bound `{ Player, Boss }` subject's current/max health, alive/down/dead state, or terminal candidate must execute through the active queue and token. This includes damage and, if a canonical path exposes them, heal, reset, reconfigure, revive, or forced-death operations. Initialization before terminal-subject binding is outside the window; an unsupported reset/revive after binding faults the run rather than reviving old result state.
5. Same-root mutation or reaction work created while the active queue drains receives a deterministic intra-root sequence and remains in the epoch. The root producer and every queued handler are synchronous and non-yielding; they may enqueue only through the active context before returning and may not retain a token for a coroutine, task, later frame, or unrelated callback.
6. The coordinator lifecycle is `Idle -> Open -> Draining -> Finalizing -> EpochClosed`, with `Faulted` and `Cancelled` exits from any active substate. `Open` runs the admitted root producer; `Draining` begins after it returns and consumes same-token work; when no handler is executing and the queue is empty, enqueue is structurally sealed and the coordinator enters `Finalizing`.
7. `Finalizing` performs one synchronous handshake with both bound subject adapters. Each adapter must return exactly one token/epoch-matching final health/down snapshot even when that subject was untouched. Missing, disabled, rebound, duplicate, throwing, or asynchronous adapters fault instead of leaving the coordinator waiting.
8. At `QueueDrainedAndSubjectsFinalized`, the arbiter validates candidate/final-state agreement and resolves at most once, then seals the per-root record as `EpochClosed`. A nonterminal close invalidates the token and follows `EpochClosed -> Idle -> Open(next)` when a pending admission exists, or remains `Idle` when none exists. Clear/Fail invalidates every pending admission and first reaches `EpochClosed -> TerminalClosed`; that contender must atomically win the shared terminal-or-restart latch, seal `TerminalFinalizationAuthority`, and enter `TerminalFinalizing`. Only that winner gathers deterministic final facts, seals course traversal/quiescence, requires P1-C `RunFinalization`, and closes the current presentation-adapter generation before `OutcomeFactsSealed`; admitted mastery and P2-A variability closure then complete before P1-A may enter `CommitRequested`.
9. Work exceptions, direct mutation bypass, current-run token/epoch/order mismatch, adapter loss, or snapshot failure enter `Faulted`; scene unload, explicit run abort, or coordinator disposal enter `Cancelled`. Either path atomically invalidates active and pending current-run authority, discards queued work, seals at most one diagnostic abort while the run is active, and publishes no product summary.
10. `Time.frameCount`, `FixedUpdate` count, rendered frames, elapsed milliseconds, health-callback arrival, and subscriber order are not valid substitutes for admission, root sequence, token, epoch, or the synchronous close barrier.

D4a approval freezes only the product semantics above. D4b remains provisional until P1-0 inventories every canonical Station terminal-state mutation path and a bounded proof of concept demonstrates exclusive pre-mutation admission plus synchronous closure for the full inventoried set. Only then may the admission/order kind, active boundary, subject roles, terminal-state coverage, synchronous work rule, nested/independent-root rules, coordinator lifecycle, token-state handling, close handshake, and final-state requirements be frozen on the P1-0 route shell and copied into `StageRunRouteSnapshot`; the arbiter never reads mutable latest policy after entry. Changing a frozen technical policy changes route revision and canonical digest. If exclusive coverage or synchronous closure cannot be proven, D4b remains unapproved, P1-A fails closed, and no double-terminal-support claim is permitted; if D1-D3 or D4a are separately approved, those product approvals remain valid.

### Token and coordinator state handling

| Observed authority | Required handling |
|---|---|
| `ActiveCurrent` token with matching run/root/epoch | accept only through the active synchronous queue; any malformed current-run authority faults the active run |
| `IdleCurrent` canonical root admission | assign the next sequence and open it immediately when no lower pending admission exists |
| `DeferredCurrent` root admission | keep an ordered pending record only; it has no token and cannot mutate until promoted |
| `ClosedSameRun` token while the run is still active | reject before mutation, enter `Faulted`, and seal the one current-run diagnostic abort |
| `WrongRun` or foreign generation | reject and log without mutation; never abort an unrelated active run |
| `PostTerminal` token after `CommitRequested`, `Committed`, `Presented`, or `Disposed` | reject and log only; do not alter the immutable summary, reopen commit, or create a second abort |
| coordinator already `Faulted` or `Cancelled` | reject and log only; no queued work, second abort, or product summary may appear |

### Truth table

| Boss candidate | Boss final dead | Player candidate | Player final down | Resolution |
|---:|---:|---:|---:|---|
| no | no | no | no | close epoch with no product commit; run remains active |
| yes | yes | no | no | commit Clear with `BossTerminal` |
| no | no | yes | yes | commit Fail with `PlayerTerminal` plus typed failure reason |
| yes | yes | yes | yes | commit Clear with `SimultaneousTerminalClear` and player-down fact retained |
| any candidate/final mismatch | — | — | — | seal `InvalidTerminalEvidence` diagnostic abort; no product summary |

A candidate/final mismatch includes candidate-with-live-final-state, final-terminal-state-without-the-same-epoch candidate, wrong current run/segment/root/epoch/order authority, direct terminal-state mutation outside the coordinator, or a close attempt before the queue/subjects finalize. Wrong-run and post-terminal authority instead follow the reject/log-only rows above; neither can mutate or reinterpret a result. The arbiter never guesses Clear or Fail from whichever callback arrived first.

For simultaneous Clear, the summary still records the resolved lethal damage and player down. A later no-down mastery objective therefore fails even though the stage clears. The result should retain an enum-equivalent diagnostic such as `SimultaneousTerminalClear` so QA and presentation do not have to infer the tie from callback order.

### Why Clear-wins is recommended

- The current canonical Station runtime clear trigger is boss death; no current stage rule says survival is mandatory for clear. The older Corridor definition's broader description is not used as evidence for this Station tie policy.
- Survival can remain a separate result/mastery fact, preserving truthful cost without erasing the achieved boss defeat.
- A dramatic final trade is less likely to feel arbitrarily reversed in the short second-round demo.
- The rule is narrow: a boss death in a higher independent root sequence after a committed player-only lower sequence does not retroactively clear the run.

| Alternative | Consequence | Verdict |
|---|---|---|
| Fail-wins | makes survival an implicit mandatory clear condition that is not authored anywhere | Reject for revision 1; reconsider only with an explicit stage rule |
| Draw/restart | adds a third product outcome, copy, action policy, persistence semantics, and tests without source or demo need | Reject |
| first callback wins | changes with damage/subscription order and cannot express product intent | Reject |
| arbitrary millisecond grace window | depends on frame rate/time-scale and invites late damage ambiguity | Reject |

Known presentation risk: a simultaneous Clear can open the result while the player avatar is down. The outcome coordinator must freeze combat and presentation from the committed facts; it must not revive/reset the bound subject, accept a post-terminal mutation, or wait for a living-player state. Replay creates a new run, and Lobby exits the disposed one.

## Active-Run Restart Boundary

This packet does not reopen the previously resolved boundary:

- Replay after committed Clear and Retry after committed Fail use their separate P1-0 result actions and the same P1-A terminal executor.
- Restart before an outcome is not Clear, Fail, or a dormant result button.
- P2-A authors `activeRunRestartPolicy`. A source submits a pure request before cleanup; P1-A validates it, wins the latch, enters `RestartClosing`, and seals `ResolvedActiveRunRestartDispatch` first. P1-A then requests every independently admitted P1-E lesson/attempt, P2-B course, P1-C execution, P2-A variability, and P2-B presentation barrier and seals one diagnostic abort only after all results are known. Success alone disposes and dispatches; fault enters `ClosureFaulted` with no dispatch.
- Neither path reuses the other's action, summary, or lifecycle state.

Wuthering's authored in-dungeon reset fields and FGO's post-clear repeat policy reinforce this separation of static concerns, but they do not prove executed cleanup or dictate DimensionBrawl targets and button availability.

## Priority and Dependency Handoff

| Order | Work | What this packet changes |
|---:|---|---|
| P0 | refresh full/natural route proof, retain one product terminal surface, execute Retry-to-Corridor and Lobby | no change; still the hard implementation gate |
| P1-0 | author the final minimal `PlayableStageDefinition` route shell and Station segment definition; inventory every terminal-state mutation path and prove or reject the D4b mechanism | D1 and D2 remove product ambiguity; D4b is frozen separately only after feasibility evidence |
| P1-A | after D4b freezes, add run snapshot, canonical root admission/order, synchronous terminal-state queue, exactly-once summary, shared result shell, and typed executor | D3 and D4a define product behavior; D4b supplies the separately proven implementation contract |
| P1-B onward | fill optional joins, then encounter/mastery/tutorial/progression work in roadmap order | no reordering or early feature expansion |

Do not implement a dedicated Fail scene, Stage Select action, Next action, draw outcome, reward reveal, or active-run restart merely because this packet names their boundaries.

## Acceptance Evidence After Approval

| Scenario | Required proof |
|---|---|
| route identity | one immutable entry snapshot contains the approved logical ID/revision, both ordered segment/definition/scene identities, Replay/Retry/Lobby records, all allowed-outcome sets, the terminal resolution policy, and the canonical digest |
| catalog/route drift | selected catalog projection, Build Settings, current flow request, result targets, and both physical scene bindings either agree with the route shell or validation fails |
| Clear result | committed Clear opens the shared shell once and offers Replay plus Lobby only |
| Fail result | committed Fail opens the distinct Fail projection once, offers Retry plus Lobby only, and dispatches no success hook/progress/reward |
| Replay after Clear | one sealed snapshot-derived Replay dispatch returns to Corridor; successful entry creates a different run ID with reset facts |
| Retry after Fail | one sealed snapshot-derived Retry dispatch returns to Corridor; successful entry creates a different run ID with reset facts |
| Lobby after either outcome | one sealed `UIRouteId.Lobby` dispatch occurs after old-context disposal |
| missing/stale summary | shell may show diagnostic-safe state, but terminal actions stay disabled |
| terminal callback permutation | with fixed root-admission sequence, boss-then-player and player-then-boss candidates in the same epoch and matching final states both commit one Clear with simultaneous-terminal diagnostic and player down recorded |
| independent-root causal order | lower root-admission sequence resolves first; deliberately reversing the authoritative root sequence may change Clear versus Fail, while merely reversing callback/subscriber delivery may not |
| callback root creation | damage, `Died`, terminal-observer, presenter, and collector callbacks cannot mint a new root; an attempt faults the active current run before mutation |
| terminal window boundary | player-only terminal in a closed lower-sequence epoch followed by boss death in a higher sequence remains Fail; no late candidate reopens commit |
| resolution boundary | nested same-root terminal-state work remains in one epoch; an independent admission waits without a token and is processed only in a later epoch, never merged by frame/time proximity |
| synchronous close and cycle | root producer and nested work return, the queue drains, both untouched/touched subjects synchronously snapshot, and the coordinator reaches `EpochClosed` without a frame/task/timer/leaked scope; nonterminal close returns through `Idle` and opens the next pending admission, while terminal close becomes `TerminalClosed` |
| fault/cancel | work exception, adapter loss/rebind, unload, or explicit abort invalidates active/pending authority atomically, publishes one diagnostic abort at most, and never commits a summary |
| token-state matrix | active-current malformed authority aborts the active run; closed-same-run authority faults it; wrong-run and post-terminal authority are reject/log-only; no case mutates an immutable summary or creates a second abort |
| exclusive terminal-state coverage | every canonical Station mutation that can change Player/Boss terminal state carries the active root/epoch token; a direct bypass seals `InvalidTerminalEvidence` abort and no summary |
| review HUD | cannot independently reload Station or publish a second canonical result |

## Approval Record

Record each decision independently; approval of D1-D3 or D4a is not contingent on accepting the current D4b mechanism:

1. D1 IDs, revision, ordered segments, and Corridor-targeted Replay/Retry.
2. D2 `Clear -> Replay + Lobby` and `Fail -> Retry + Lobby`, with Stage Select/Next deferred.
3. D3 outcome-aware shared additive result shell.
4. D4a authoritative causal-order semantics and same-epoch Clear-wins with the player-down fact retained.
5. D4b provisional pre-mutation admission/order, synchronous terminal-state queue lifecycle, and token-state contract, frozen only after the complete P1-0 mutation-path inventory and proof of concept pass.

D1-D3 and D4a may receive product approval before D4b feasibility closes. P1-A terminal-arbitration implementation remains blocked until D4b is separately frozen. Changing D1, D2 action identity/kind/target/availability, D4a semantics, or a frozen D4b contract changes the route digest and requires a new route revision after production authoring. Changing only D2 label/role/order or D3 presentation is view work if ownership and action semantics remain identical. A D4a or frozen-D4b change also requires contract, result-compatibility, and test review before any saved result or progression exists.

Until explicit approval, all values in this packet remain recommendations. Documentation agreement must not be reported as a production freeze or a passing P0 gate.

## Evidence Sources

Local sources inspected:

- `Assets/_Game/UI/StageSelect/StageSelectScreenPresenter.cs`
- `Assets/_Game/UI/StageSelect/UIStageCatalog.cs`
- `Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset`
- `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset`
- `Assets/_Game/Scripts/LevelDesign/OlympusCorridorCombatFlowController.cs`
- `Assets/_Game/Scripts/Combat/CombatEncounterController.cs`
- `Assets/_Game/Scripts/LevelDesign/OlympusStationCombatResultPresenter.cs`
- `Assets/_Game/Scripts/LevelDesign/OlympusStageClearOverlay.cs`
- `Assets/_Game/Scripts/UI/StageClear/StageClearScreenPresenter.cs`
- `Assets/_Game/Scenes/UI/UI_StageClear.unity`
- `Assets/_Game/Tests/PlayMode/CombatEncounterResultPlayModeTests.cs`

Bounded Ark sources inspected under the source root:

- `games/punishing-gray-raven/read-first/pgr-development-context-direct-readfirst-slices.md`
- `games/punishing-gray-raven/enemies-stages/pgr-tutorial-stage-context-rollup.csv`
- `games/punishing-gray-raven/enemies-stages/pgr-guidefight-stage-label-context.csv`
- `games/honkai-impact-3rd/combat/hi3-combat-stage-direct-readfirst.md`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-table-summary.csv`
- `games/honkai-impact-3rd/enemies-stages/hi3-stage-row-samples.csv`
- `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/lua/modules/logic/fight/rpc/FightRpc.lua`
- `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/lua/modules/logic/fight/model/FightResultModel.lua`
- `games/reverse-1999/raw/re1999-data/2026-06-14/files/extracted_repo/re1999-data-main/data/lua/modules/logic/dungeon/rpc/DungeonRpc.lua`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/Sector/LevelDetail/UILevelRewards.lua`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/BattleDungeon/UI/UIDungeonResult.lua`
- `games/girls-frontline-neural-cloud/raw/dimbreath-gflpncdata/2026-06-15/files/extracted_repo/GFLPNCData-master/zh-CN/lua/Game/BattleDungeon/UI/UIDungeonFailureResult_Temp.lua`
- `games/ash-echoes/raw/ash-echoes-gamedata/2026-06-14/files/extracted_repo/GameData-master/data/chapter_levels.dat`
- `games/ash-echoes/raw/ash-echoes-gamedata/2026-06-14/files/extracted_repo/GameData-master/data/level_reward.dat`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/GuideStep.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/GuideGroup.json`
- `games/wuthering-waves/raw/wutheringdata/2026-06-13/files/extracted_repo/WutheringData-master/ConfigDB/ComboTeachingCondition.json`
- `games/fate-grand-order/raw/atlasacademy-api/2026-06-14/files/data/NA/nice_war.json`
- `games/fate-grand-order/raw/atlasacademy-api/2026-06-14/files/data/JP/nice_war.json`
- `games/fate-grand-order/enemies-stages/fgo-war-quest-summary.csv`

Community and extracted materials remain structural evidence only. No proprietary code, IDs, text, tuning, layouts, assets, or audiovisual content should be copied.
