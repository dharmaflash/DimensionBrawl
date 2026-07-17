# Lobby Operations Drawer Review Vertical Slice (OPS-01)

Status: implemented / verification passed / review-only
Scope label: `REVIEW SAMPLE / TEMP_DO_NOT_SHIP`
Canonical product state changed: no
Last updated: 2026-07-18

## Outcome

OPS-01 is an isolated mobile-landscape review slice for the service-facing lobby flow:

`DrawerClosed -> Directory -> EntryDetail -> ReviewConfirm`

It evaluates the hierarchy and state language needed for notices, mailbox, missions, and an event calendar without pretending that an account, backend, server clock, schedule verdict, unread counter, progress ledger, reward ledger, or notification service exists.

The slice contains exactly four review entries:

- `Notice`: one DimensionBrawl-authored local fixture, explicitly labeled as a UI review fixture rather than an operations message;
- `Mailbox`: a review shell whose service and account sources are `NoVerifiedSource`;
- `Missions`: a review shell whose definition/account/progress sources are not connected and cannot be interpreted as zero progress;
- `EventCalendar`: a definition-only shell with no server clock and no current/upcoming/ended verdict.

OPS-01 does not change the roadmap priority: the content factory and three-stage loop remain the product bottleneck. This slice is a bounded UI/service-contract sample requested alongside that work, not proof that Gate E or Gate F exists.

## Product boundary

- Review scene: `Assets/_Game/Scenes/Review/UI_LobbyOperationsDrawerReview.unity`.
- Runtime namespace: `DimensionBrawl.UI.LobbyOperationsReview`.
- The scene remains outside enabled Build Settings.
- The scene contains no `UISceneFlowRouter`, `UISceneRouteLoader`, `UIPanelRouter`, route request, scene load, external URL, network request, or global singleton.
- The scene does not instantiate or reference `PF_UI_LobbyScreen.prefab`. That prefab currently contains active mock account identity, level, currency, dated notice, `New`, and reward language that would look authoritative in this review.
- The scene may reference only its OPS-01 profile, the existing responsive-layout catalog, the neutral project-owned lobby background sprite, and shared UI fonts/common safe-area components. It must not modify shared asset import settings.
- `UI_Lobby.unity`, `PF_UI_LobbyScreen.prefab`, `DB_UITextCatalog.asset`, `DB_UIPanelCatalog.asset`, `DB_UIRouteTable.asset`, `DB_UIScreenCatalog.asset`, and their metadata remain unchanged and are hash-protected during setup/verification.
- No new `UIRouteId`, screen entry, panel catalog entry, service endpoint, account model, or Build Settings entry is introduced.
- `StageRunRuntime`, results, progression, inventory, currency, rewards, and persistence remain untouched.

## Implemented artifacts

- `Assets/_Game/DesignData/UI/Review/DB_UILobbyOperationsReview.asset`: four-entry review catalog with separate production, service, account, server-clock, schedule, progress, attention, and action dispositions.
- `Assets/_Game/UI/LobbyOperationsReview/LobbyOperationsReviewProfile.cs`: validated source/disposition schema with no account, count, reward, schedule verdict, route, URL, or service payload fields.
- `Assets/_Game/UI/LobbyOperationsReview/LobbyOperationsReviewSession.cs`: deterministic `Closed -> Directory -> EntryDetail -> ReviewConfirm` state model and same-session exact-once acknowledgement latch.
- `Assets/_Game/UI/LobbyOperationsReview/LobbyOperationsReviewController.cs`: fail-closed authored binding, single-active-panel/raycast ownership, deterministic focus, stale-copy clearing, and Notice-only local confirmation.
- `Assets/_Game/Editor/LobbyOperationsReview/LobbyOperationsDrawerReviewSetup.cs`: deterministic scene/profile generation and independent boundary verification.
- `Assets/_Game/Editor/LobbyOperationsReview/LobbyOperationsDrawerReviewVisualQaCapture.cs`: public-navigation-only 24-frame capture, runtime contract evidence, safe-area/text/spacing/overlap checks, canonical digest, and a deliberately separate human-review flag.
- `Assets/_Game/Tests/PlayMode/LobbyOperationsReviewSessionPlayModeTests.cs` and `LobbyOperationsReviewControllerPlayModeTests.cs`: 22 focused PlayMode tests.

## Flow contract

### DrawerClosed

- Shows a neutral lobby background, review label, and one visible `OPERATIONS REVIEW` opener.
- Shows no profile name/ID, level, currency, stamina, red dot, unread count, mission progress, event timer, banner date, or reward.
- The opener is the deterministic default focus.

### Directory

- Shows the four exact entries in stable order: Notice, Mailbox, Missions, Event Calendar.
- Every entry can open an explanation detail; no entry uses a padlock, disabled gameplay CTA, clear marker, count badge, or red dot.
- Each row displays a textual source-status label in addition to color.
- Opening the drawer blocks background raycasts and moves focus to the first row.

### EntryDetail

- Shows production, service, account, server-clock, schedule, progress, attention, and action dispositions as independent rows.
- `NoVerifiedSource` means the source is absent from this slice. It does not mean empty, zero, false, offline, maintenance, unavailable, or locked.
- Notice may show only the local fixture copy and a `REVIEW THIS FIXTURE` action.
- Mailbox, Missions, and Event Calendar are explanation-only. They expose no read, claim, collect, retry, subscribe, enter, or deep-link action.
- Back returns to Directory; Close returns directly to DrawerClosed.

### ReviewConfirm

- Can be entered only from the Notice local fixture.
- Confirms only that the local UI review path was inspected.
- `ACKNOWLEDGE REVIEW` latches before invoking a local instance event and dispatches at most once per session.
- It does not mark a notice read, mutate notification state, claim an attachment, grant a reward, persist a flag, or call a service.
- Back returns to Notice detail; Close returns to DrawerClosed while retaining the same-session acknowledgement latch.

### Back and focus

- Hardware/UI Back: `ReviewConfirm -> EntryDetail -> Directory -> DrawerClosed`.
- Back in DrawerClosed is a no-op.
- Close from any open state returns to DrawerClosed and restores focus to the opener.
- Directory focuses Notice; detail focuses Back; ReviewConfirm focuses Acknowledge.
- Hidden panels are inactive for interaction and raycasts, and hidden actions are excluded from navigation.

## Data responsibility model

OPS-01 must not collapse source truth into `isAvailable`, `isLocked`, `hasData`, or a nullable count. The profile keeps the following dispositions independent:

- production: local review fixture, definition-only shell, or review shell with no product commitment;
- service: not required for the local fixture or `NoVerifiedSource`;
- account: not required or `NoVerifiedSource`;
- server clock: not required or `NoVerifiedSource`;
- schedule: not admitted, definition-only with no verdict, or `NoVerifiedSource`;
- progress: not admitted or `NoVerifiedSource`;
- attention: not admitted or `NoVerifiedSource`;
- action: local review confirmation or explanation-only.

The profile schema admits stable review ID, kind, order, localization keys with required fallbacks, neutral descriptive copy, and the dispositions above. It deliberately has no account ID, timestamp, date window, counter, progress number, reward, attachment, cost, currency, transaction, URL, route, or service payload field.

Future live-service work must use separate definition, transport/load, account binding, server-clock, schedule verdict, attention, and mutation models. It must not reinterpret OPS-01 fallbacks as backend data.

## Repository evidence

- `Assets/_Game/UI/Lobby/PF_UI_LobbyScreen.prefab`: current canonical lobby surface and active mock content; protected, not reused as a review shell.
- `Assets/_Game/Scenes/UI/UI_Lobby.unity`: canonical Lobby route scene; protected and unchanged.
- `Assets/_Game/UI/Lobby/LobbyScreenPresenter.cs`: product Lobby primary CTA presenter; not reused because it owns a real route request.
- `Assets/_Game/UI/Common/UIPanelRouter.cs`: simple local panel primitive; not reused so OPS-01 can expose and test its stricter source/state contract directly.
- `Assets/_Game/DesignData/UI/DB_UITextCatalog.asset`: contains canonical/mock lobby text; protected and not treated as service truth.
- `Assets/_Game/DesignData/UI/DB_UIPanelCatalog.asset`: has no Operations Drawer product entry and remains unchanged.
- `Assets/_Game/UI/ChapterHubReview/*` and `Assets/_Game/Editor/ChapterHubReview/*`: precedent for independent review state, deterministic editor-authored scenes, hash boundaries, exact-resolution capture, and separate human review.

## ArkData structural evidence and usage limit

ArkData is read-only structural research. No comparison-game text, identifier, time, reward, art, icon, layout measurement, color, animation, code, or row value is copied.

### Punishing: Gray Raven

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\economy\pgr-broad-economy-table-summary.csv`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\ui\pgr-guide-ui-chain-rollup.csv`

The broad helpers distinguish activity/calendar definition, mail template/reward reference, task definition/condition/result, tab control, and sign-in rewards. OPS-01 adopts only that responsibility separation. Red-point configuration is not evidence of an account's unread state, and public client data is not proof of the backend.

### Honkai Impact 3rd

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\economy-liveops\hi3-economy-liveops-table-summary.csv`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\ui\hi3-ui-presentation-reference-pack.csv`

The helpers keep lobby activity notices, activity schedule pages, activity panels, sign-in reward data, and mission data as distinct definitions. OPS-01 adopts only the separation of notice, schedule, mission, and presentation responsibilities.

### Aether Gazer

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\client-code\aether-gazer-lua-config-table-schema-pack.csv`

The schema helper separates activity definitions, main advertisement/banner references, mail templates/special letters, assignments, sign-in, and activity rewards. OPS-01 uses that only as evidence that a lobby entry is not the same object as account progress or a reward transaction.

### Snowbreak

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\snowbreak-containment-zone\ui\snowbreak-maa-flow-template-ui-motion-context-pack.csv`

This is MAA automation evidence, not official client UI data. It supports only the abstract need to distinguish successful, empty, retry/failure, modal-close, and main-return paths. OPS-01 does not copy its scripts or treat automation selectors as authored UI truth.

### Provenance boundary

- The PGR, HI3, and Aether snapshots have no detected reusable license in the research record; clean-room structural restatement only.
- The Snowbreak MAA source is MIT, but it is still automation material rather than official product UI and is used only for abstract flow comparison.
- The research cannot establish actual server architecture, current event state, unread/claimable/progress, reward idempotency, server time, remote configuration, maintenance, or patch status.

## Test plan

### Session PlayMode tests

- A valid profile contains exactly the four stable entries and validates every required disposition/fallback.
- Duplicate IDs/kinds, wrong order, blank fallbacks, or a forbidden disposition combination fail closed.
- Initial state is DrawerClosed with no selection and an unacknowledged latch.
- Open, select, back, close, and hardware-back transitions follow the documented stack.
- Notice alone may enter ReviewConfirm; the other three entries reject the request.
- Acknowledgement succeeds once, returns the selected Notice ID, and rejects repeats.
- Close/reopen within the same session preserves the acknowledgement latch; a new session resets it.
- Mailbox, Missions, and Event Calendar preserve distinct missing-source dispositions and never synthesize empty/zero/current states.

### Controller PlayMode tests

- Exactly one panel is active/interactable/raycastable for each phase.
- Directory rows bind the four exact stable IDs and visible textual source labels.
- Detail content and action visibility follow dispositions; missing sources clear stale text and hide the CTA.
- Repeated taps do not double-transition or double-dispatch acknowledgement.
- Listener binding remains balanced across disable/enable without resetting the session latch.
- Default focus is deterministic and restored to the opener on close.
- There is no router/loader, route request, service call, persistent callback, or active `StageRunRuntime` context.

### Scene and boundary validation

- One controller, Canvas, camera, AudioListener, EventSystem, safe-area root, and responsive root.
- Four independent panels: DrawerClosed, Directory, EntryDetail, ReviewConfirm.
- Exactly four directory rows with the exact entry IDs/kinds.
- Scene absent from enabled Build Settings.
- No product router/loader/panel router, network component, persistence component, or runtime hierarchy builder.
- Canonical Lobby scene/prefab/text/panel/route/screen assets and the referenced background sprite/importer are SHA-256 unchanged before/after setup and validation.
- Existing canonical UI route tests remain green.

## Mobile visual QA

Exact resolutions: `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`.

Eight states per resolution, 24 captures total:

1. DrawerClosed;
2. Directory;
3. Notice local-fixture detail;
4. Mailbox service+account-source-missing detail;
5. Missions progress/account-source-missing detail;
6. Event Calendar definition-only/no-clock detail;
7. ReviewConfirm before acknowledgement;
8. ReviewConfirm after acknowledgement.

Automated evidence checks state, selection, disposition labels, CTA visibility, exact-once acknowledgement, hidden forbidden fields, inactive-panel raycasts, deterministic focus, safe-area fit, minimum 48px controls, text preferred size, no overlap, no `StageRun`, and canonical asset hashes. Asymmetric left/right virtual notch insets are checked in both orientations. Automated capture success never self-attests human review.

Human review inspects all 24 PNGs for hierarchy, contrast, readable copy, drawer width, background blocking, ultrawide composition, status distinction without color-only meaning, and the absence of account/reward/unread/progress/schedule claims.

### Recorded QA result

- Automated capture: `24 / 24 PASS` at all three exact resolutions, with four left-notch and four right-notch states per resolution.
- The generated manifest keeps `HumanReviewRequired = true` and `HumanReviewed = false`; automation is not allowed to attest its own composition review.
- Separate human review: `24 / 24 PASS` after inspecting every PNG. Directory, detail, and confirm surfaces remain readable at 16:9 and ultrawide widths; the scrim/drawer hierarchy, textual source distinctions, and absence of invented service/account values are clear.
- Human review initially caught the closed-card status copy sitting too close to the opener CTA. The card was re-authored, all 24 captures were regenerated, and the three closed frames were re-inspected. Automated QA now also measures the status-to-CTA minimum vertical gap in screen coordinates.
- Non-blocking polish debt: some secondary boundary copy and the disabled acknowledgement CTA have low contrast, and the confirmation surface mixes Korean title copy with English contract copy. These remain accessibility/localization work rather than contract failures.

## Verification evidence

- Scene/profile generation: exit `0`; `C:/tmp/DimensionBrawl-OPS01-Setup-Final4.log`.
- Independent scene/boundary verification: exit `0`; `C:/tmp/DimensionBrawl-OPS01-Verify-Final3.log`.
- Session PlayMode tests: `16 / 16 PASS`; `C:/tmp/DimensionBrawl-OPS01-SessionTests-Final.xml`.
- Controller PlayMode tests: `6 / 6 PASS`; `C:/tmp/DimensionBrawl-OPS01-ControllerTests-Final.xml`.
- Existing canonical UI route regression: `34 / 34 PASS`; `C:/tmp/DimensionBrawl-OPS01-CanonicalUiTests.xml`.
- Total recorded PlayMode tests: `56 / 56 PASS`, zero failed or skipped.
- Visual QA: `24 / 24 PASS`; `C:/tmp/DimensionBrawl-OPS01-VisualQA-Final7.log`, report, JSON manifest, and PNGs under `C:/tmp/DimensionBrawl-LobbyOperationsDrawerReview-QA`.
- Canonical digest boundary covers exactly 15 canonical product assets: the `UI_Lobby` scene, three UI prefabs, the Lobby background, two Pretendard OTF sources, eight UI catalogs, and each corresponding `.meta` file. The OPS-01 review scene/profile are outputs, not members of this immutable comparison boundary. Transient dynamic TMP atlas cache data is not treated as immutable source evidence and is clean after normal editor exit.

## P1 risks

- Reusing the active canonical Lobby prefab exposes mock account identity, level, currency, dates, `New`, and reward language as if they were true.
- Treating `NoVerifiedSource` as empty, zero, false, offline, maintenance, unavailable, locked, or current silently invents service/account state.
- Calling acknowledgement `read`, `claim`, `collect`, or persisting it turns a local review signal into a product mutation.
- Adding Operations/Mail/Mission/Event route or panel IDs crosses the review boundary and changes the canonical product contract.
- Holding stale detail content while switching entries can display the local Notice fixture under a missing-source channel.
- Automatic QA cannot mark its own output human-reviewed.

## Definition of done

- The four-state flow and four exact entries are implemented in an independent authored review scene.
- Production, service, account, server-clock, schedule, progress, attention, and action dispositions remain separate.
- Only Notice can reach local ReviewConfirm; acknowledgement is exact-once and non-persistent.
- No fake account identity, currency, count, unread, progress, reward, date, schedule verdict, maintenance, or availability claim is shown.
- The scene remains outside Build Settings with no router/loader/network/persistence/StageRun ownership.
- Session/controller tests, scene validation, canonical UI regression, and the 24-capture mobile matrix pass with recorded evidence and separate human review.
- Canonical Lobby/UI assets and the referenced neutral background are hash-proven unchanged.
- A Docker-hosted Notion child page records scope, data/source distinctions, ArkData paths, tests, QA, deferred work, and the focused commit hash.
- The slice is committed separately with unrelated worktree changes excluded.

OPS-01 has its implementation and verification evidence, but remains intentionally non-shippable: it is a review contract and local UI fixture, not a live-service integration.

## Deferred product work

- Actual notice feed transport, cache/stale policy, pagination, and deep links.
- Mail account binding, unread/read state, attachments, claim transactions, bulk actions, and retry safety.
- Mission definitions, account progress, completion, rewards, reset schedule, and claim transactions.
- Server clock, remote enable flags, event schedule verdicts, maintenance, and time-zone policy.
- Red dots/attention aggregation and notification permission/device-token ownership.
- Patch catalog, download progress/resume, content version, and rollback UI.
- Localization service, accessibility audit, final art/icons/motion/audio/haptics, and product Lobby integration.
