# Olympus Chapter Hub Review Vertical Slice (CHUB-01)

Status: implemented / verified review slice  
Scope label: `REVIEW SAMPLE / TEMP_DO_NOT_SHIP`  
Canonical product state changed: no  
Last updated: 2026-07-18

## Outcome

CHUB-01 is an isolated, mobile-landscape review slice for one complete front-end flow:

`ChapterHub -> StageMap -> StageDetail -> ReviewConfirm`

It evaluates the information hierarchy, touch flow, stage-node composition, and responsive presentation expected from an early stage-based mobile action game. It does not add a product route, launch combat, admit a `StageRun`, persist progression, grant rewards, or claim that placeholder stages are playable.

The slice contains exactly:

- one real stage projected from the canonical `DB_UIStageCatalog` entry;
- one explicitly labeled `InProduction` review slot; and
- one explicitly labeled `Announced` review slot.

`InProduction` and `Announced` are production communication states, not player progression states. They must never be represented as locked, cleared, uncleared, stamina-gated, purchasable, or reward-bearing content.

## Product boundary

- Runtime controller: `Assets/_Game/UI/ChapterHubReview/OlympusChapterHubReviewController.cs`.
- Review scene: `Assets/_Game/Scenes/Review/UI_OlympusChapterHubReview.unity`.
- Canonical stage source: `Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset`.
- Canonical briefing source: `UIStageRouteProjection.StageBriefingReadModel`.
- The review scene must remain outside enabled Build Settings.
- The scene must not reference or call `UISceneFlowRouter`, `UISceneRouteLoader`, `RequestRoute`, or `RequestRouteWithScene`.
- The scene must not create, admit, restart, complete, or mutate `StageRunRuntime` or any result/progression state.
- The scene must not modify `DB_UIRouteTable.asset`, `DB_UIScreenCatalog.asset`, `DB_UIStageCatalog.asset`, its projection generation/digest, `DB_PlayableStage_OlympusInvasion.asset`, its templates, or canonical scene assets.
- `ReviewConfirm` confirms only that the UI review path was reached. It is not a deployment confirmation and must not load a scene.

The current product route contract remains `Login`, `Lobby`, `StageSelect`, and `Combat`. Chapter Hub and Stage Detail are review-local screen states, not new `UIRouteId` values.

## Flow contract

### ChapterHub

- Presents the Olympus chapter identity and one primary action leading to `StageMap`.
- May show neutral chapter-level descriptive copy authored for this review.
- Must not show chapter completion percentage, clear count, lock state, currency, stamina, reward, or account eligibility.
- Back/close behavior is local to the review scene; it does not route to the canonical Lobby.

### StageMap

- Shows one canonical stage node and the two production-status slots.
- The canonical stage node reads its title and displayable briefing fields from a current `UIStageRouteProjection`.
- `InProduction` and `Announced` use review-only IDs and presentation labels. They do not reference a `PlayableStageDefinition` and are never passed to `UIStageCatalog` projection APIs.
- Selecting any slot may open `StageDetail`, but only the canonical stage can expose the review-confirm action.
- The map may borrow the existing prototype's node placement and panel-motion principles, but not its hard-coded content or state claims.

### StageDetail

- For the canonical stage, render only fields admitted by `StageBriefingReadModel` dispositions.
- For `InProduction`, show a neutral production-status explanation and a back action. Do not show a disabled combat button that could be interpreted as a gameplay lock.
- For `Announced`, show a neutral announcement-status explanation and a back action. Do not infer a release date, unlock requirement, cost, or reward.
- The canonical stage's review action advances locally to `ReviewConfirm`; it does not request a route.

### ReviewConfirm

- States that the Chapter Hub review path is complete.
- Provides local back/restart/close controls only.
- Dispatches at most one review-session completion signal.
- Does not load combat, write persistence, mutate progression, or grant anything.

### Back-stack expectations

- `ChapterHub -> StageMap`.
- `StageMap -> ChapterHub` on back.
- `StageMap -> StageDetail` on node selection.
- `StageDetail -> StageMap` on back.
- canonical `StageDetail -> ReviewConfirm` on review confirmation.
- `ReviewConfirm -> StageDetail` or `ChapterHub` according to the final labeled control; the choice must be deterministic and covered by tests.
- Rapid repeated taps must not double-transition or double-dispatch completion.

## Slot model

| Slot | Source | Route eligibility | Allowed presentation | Forbidden claims |
|---|---|---:|---|---|
| Canonical stage | Current `UIStageRouteProjection` from `DB_UIStageCatalog` | Review-local confirm only | Disposition-admitted briefing fields | Fake reward, stamina/cost, lock, clear, score, account eligibility |
| `InProduction` | Review-only static definition | None | Stable review ID, title, `InProduction` badge, neutral status copy | Playable stage reference, route, reward, cost, lock/clear state, release promise |
| `Announced` | Review-only static definition | None | Stable review ID, title, `Announced` badge, neutral status copy | Playable stage reference, route, reward, cost, lock/clear state, invented date |

The production-status slots exist to judge composition at realistic list density. They must be visibly distinct from the canonical playable stage without using padlocks, completion ticks, stars, clear ranks, stamina icons, reward boxes, or dimming conventions that imply progression.

## Responsibility separation

CHUB-01 must keep four responsibilities separate even if the first implementation is compact.

### 1. Static definition

Static review definitions own only stable review IDs, display labels, production status, ordering, and review presentation hints. They do not own player state, a scene route, a `PlayableStageDefinition`, rewards, costs, or eligibility.

The real stage is not copied into a second definition. Its current product-facing data comes through `UIStageCatalog` projection. Review-only slots are clearly marked `TEMP_DO_NOT_SHIP` and cannot be projected.

### 2. Progression state

There is no persisted progression model in this slice. The controller may hold transient session values such as current screen, selected review slot, and whether the one-shot review completion event has fired. These values must not be serialized to `PlayerPrefs`, save data, account data, `StageRunRuntime`, stage result, or progression nodes.

Terms and visuals for `locked`, `unlocked`, `cleared`, `uncleared`, `first clear`, stars, score, chapter completion, and claimed/unclaimed rewards are prohibited because no verified progression source is connected.

### 3. Routing

`ChapterHub`, `StageMap`, `StageDetail`, and `ReviewConfirm` are local controller states backed by four independently bound panels/`CanvasGroup`s. Navigation changes panel state only.

Canonical routing remains owned by `UISceneFlowRouter` and `UISceneRouteLoader`. CHUB-01 must have no router field, no route-table override, and no combat-scene handoff. A future productization task may define a route adapter only after the canonical catalog supports more than the current single-stage contract.

### 4. Service exposure

The review scene exposes no global singleton, backend API, live-service endpoint, economy service, progression service, or mutation command. The controller receives serialized UI references and read-only stage projection input through explicit configuration helpers.

If a later product slice introduces a service boundary, use separate read models for catalog definition, account progression, route eligibility, and service availability. Do not merge these into a single `StageCard` boolean bundle.

## Canonical data binding

`UIStageCatalog` currently enforces exactly one entry for route projection, canonical digest calculation, and projection-currentness checks. CHUB-01 must not add review slots to `DB_UIStageCatalog.asset`; doing so would make the canonical projection path reject the catalog.

The canonical node binding sequence is:

1. Read the existing `UIStageCatalog` asset without mutation.
2. Request the one canonical projection for `UIRouteId.Combat`.
3. Reject the real node presentation if projection construction fails.
4. Bind title, objective, combat lesson, and optional fields from `StageBriefingReadModel`.
5. Use each field's disposition to decide visibility.
6. Keep all review navigation local even when the projection is current.

Disposition policy:

- `Present`: render the field when its value also passes the relevant non-empty/range check.
- `NoVerifiedSource`: hide the row entirely.
- `NotAdmittedByCurrentSchema`: hide the row entirely.
- `NotAuthoredForCurrentSchema`: hide the row entirely.
- Any unknown/future disposition: fail closed and hide the row.

The current canonical template verifies the title, objective, and combat lesson. Recommended power, loadout, target duration, featured threat, featured summon need, and reward preview are not verified and therefore remain hidden. Legacy strings on the raw stage entry are not a substitute for the briefing read model.

No fallback may invent reward items, stamina/energy cost, recommended power, stage duration, threat labels, clear state, lock state, or service availability. Empty space should be resolved through layout, not fictional data.

CHUB-01 is a current-schema snapshot, not an automatic forward-compatible renderer for every future `StageBriefingReadModel` field. The controller explicitly supports title, objective, combat lesson, story entry, and route segments. Recommended power, loadout, duration, threat, summon, and reward rows are deliberately hard-hidden in this slice even if a future schema starts returning `Present`. Admitting any of those fields requires a separate schema-migration task, explicit layout work, provenance review, and new tests; a disposition change alone must not silently expose them.

## Mobile presentation contract

- Art/layout master: `1920 x 1080`, landscape.
- Required exact-resolution checks: `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`.
- `CanvasScaler` and anchors must preserve the 16:9 composition while accommodating 20:9 and 21:9 widths.
- Critical titles, node labels, badges, detail copy, and controls remain inside safe-area insets.
- Primary interactive targets are at least 48 logical pixels at the reference resolution and retain separation at ultrawide widths.
- Backgrounds extend or crop; character or emblem art must not stretch.
- Stage nodes stay readable without relying only on color. `InProduction` and `Announced` require visible text labels.
- Detail text wraps at the intended size; it must not shrink to unreadable type to fit ultrawide or localized copy.
- The canonical action and navigation actions have distinct hierarchy. Status-only slots do not reserve a fake combat CTA.
- Keyboard/mouse operation may support editor review, but touch order and focus order remain deterministic.

## ArkData structural evidence and usage limit

ArkData is a read-only research source for responsibility boundaries and interaction grammar. It is not a content source for shipped text, art, icons, numerical values, layouts, or progression rules.

### PGR guide flow

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\ui\pgr-guide-flow-label-context-pack.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\ui\pgr-guide-flow-label-promotion-pack.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\ui\pgr-guide-flow-ui-motion-rows.csv`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\ui\pgr-guide-step-chain-label-context.csv`

The exact guide-chain evidence distinguishes main entry, chapter/list targeting, stage-content targeting, detail entry, and later combat preparation as separate UI steps. CHUB-01 adopts only that separation and explicit transition grammar. It does not reproduce PGR copy, IDs, art, panel geometry, animation timing, or gameplay/economy behavior. The helper semantic labels remain browsing aids rather than runtime proof.

### Honkai Impact 3rd structure

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\ui\hi3-ui-presentation-direct-readfirst.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\ui\hi3-ui-presentation-reference-pack.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\ui\hi3-ui-presentation-reference-pack.csv`

These helpers keep open/close transitions, story/dialog flow, layout-resource linkage, live-service panels, and battle HUD surfaces as distinct evidence slices. CHUB-01 adopts the separation of screen flow, presentation resources, and service-facing state. The material is public helper/repository evidence, not decoded official prefabs or live runtime traces.

### Aether Gazer structure

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\ui\aether-gazer-ui-transition-line-direct-readfirst.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\ui\aether-gazer-ui-material-transition-pack.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\read-first\aether-gazer-development-context-ui-motion-readfirst.csv`

These slices separate position/alpha motion, input gates, canvas activation, fades, masks, and dialog/typewriter signals. CHUB-01 adopts explicit panel state and input gating, not source text, assets, timings, or exact transitions. The evidence is Lua/config/helper material, not authored official tween graphs or runtime capture.

### Blue Archive structure

- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\blue-archive\ui\bluearchive-ui-resource-motion-supplemental-direct-readfirst.md`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\blue-archive\ui\bluearchive-ui-resource-motion-supplemental-direct-readfirst.csv`
- `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\blue-archive\enemies-stages\bluearchive-stage-encounter-read-first-queue.md`

These helpers distinguish UI/resource routes, stage-list context, skill/cut-in presentation, and data/language table routes. CHUB-01 adopts only the separation between stage definition, presentation resource, and exposed service/data state. It does not copy Blue Archive stage values, icons, layout, or encounter content. The evidence is public helper/proxy material, not decoded shipped client UI.

### No-copy rule

- No ArkData text, dialogue, icon, art, texture, audio, prefab, shader, layout measurement, stage number, reward, price, stamina cost, or progression value is copied into CHUB-01.
- No comparison-game identifier is used as a product identifier.
- Structural observations must remain traceable to the paths above and be rewritten as DimensionBrawl-owned contracts.
- Any future generated visual/audio placeholder requires separate provenance, license notes, generation date, and `TEMP_DO_NOT_SHIP` labeling.

## Current repository evidence

- `Assets/_Game/UI/StageSelect/UIStageCatalog.cs`: canonical projection, digest, currentness, single-entry invariant, and unverified-reward rejection.
- `Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset`: the one real stage and its canonical projection digest.
- `Assets/_Game/Scripts/LevelDesign/StageBriefingReadModel.cs`: disposition-bearing detail read model.
- `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusInvasionTutorialStationRun.asset`: current authored/absent briefing dispositions.
- `Assets/_Game/UI/StageSelect/StageSelectScreenPresenter.cs`: stale-projection validation pattern; its real route request must not be reused by CHUB-01.
- `Assets/_Game/UI/ChapterMapPrototype/ChapterMapPrototypeController.cs`: overview/region/stage/detail panel and motion reference only.
- `Assets/_Game/UI/ChapterMapPrototype/ChapterMapPrototypeStageNode.cs`: node interaction reference only; its hard-coded lock, clear, reward, and energy fields are non-canonical and prohibited in CHUB-01.
- `Assets/_Game/Editor/NarrativeReview/OlympusChapterNarrativeReviewSetup.cs`: precedent for an independent review scene, Build Settings exclusion, reference validation, and canonical-asset protection.

The `ChapterMapPrototype` is not a product data source. CHUB-01 may reuse an interaction pattern or refactor a visual primitive into its own review namespace, but it must not surface the prototype's fabricated state fields.

## Implementation inventory

- Review definition and transient state: `Assets/_Game/UI/ChapterHubReview/ChapterHubReviewProfile.cs` and `ChapterHubReviewSession.cs`.
- Runtime presentation: `Assets/_Game/UI/ChapterHubReview/OlympusChapterHubReviewController.cs`.
- Independent scene and review profile: `Assets/_Game/Scenes/Review/UI_OlympusChapterHubReview.unity` and `Assets/_Game/DesignData/UI/Review/DB_UIChapterHub_OlympusReview.asset`.
- Deterministic scene generation and contract validation: `Assets/_Game/Editor/ChapterHubReview/OlympusChapterHubReviewSetup.cs`.
- Exact-resolution capture and automated state verification: `Assets/_Game/Editor/ChapterHubReview/OlympusChapterHubReviewVisualQaCapture.cs`.
- Runtime tests: `Assets/_Game/Tests/PlayMode/ChapterHubReviewSessionPlayModeTests.cs` and `OlympusChapterHubReviewControllerPlayModeTests.cs`.
- Original generated review background: `Assets/_Game/UI/ChapterHubReview/Art/BG_OlympusChapterHub_Review.png`; provenance is recorded in `Assets/_Game/UI/ChapterHubReview/GENERATED_ASSET_PROVENANCE.md`.

## Test plan

### Controller PlayMode tests

Target: `Assets/_Game/Tests/PlayMode/OlympusChapterHubReviewControllerPlayModeTests.cs`.

- Initial state is exactly `ChapterHub` and only its panel is interactable/raycastable.
- The forward path reaches `ReviewConfirm` through all four states.
- Every back transition follows the documented back stack.
- Rapid/repeated taps cannot overlap transitions or dispatch review completion twice.
- The canonical node is configured from a valid current projection.
- `InProduction` and `Announced` cannot produce a route request or enter `ReviewConfirm` as a combat/deploy action.
- A failed canonical projection fails closed with no fabricated fallback.
- Non-`Present` briefing rows are hidden.
- Missing optional labels/art do not throw or block back navigation.
- No `UISceneFlowRouter` or `UISceneRouteLoader` component/reference is required.
- `StageRunRuntime` has no active/admitted context before and after the complete flow.
- No progression, result, reward, cost, lock, or clear mutation occurs.

### Scene and contract validation

- The review scene contains one controller, one event system, and the intended mobile canvas.
- Four screen panels are bound independently: `ChapterHub`, `StageMap`, `StageDetail`, `ReviewConfirm`.
- Exactly one canonical node and exactly one each of `InProduction` and `Announced` are present.
- The review scene is absent from enabled Build Settings.
- The scene contains no router/loader and no canonical combat-flow component.
- `DB_UIStageCatalog` still has exactly one entry and unchanged generation/digest after setup and validation.
- Canonical route table, playable-stage, template, scene, and progression assets are byte/hash or timestamp unchanged.
- Existing canonical UI route and Olympus play-path tests remain green.

### Recorded automated result (2026-07-18)

- Scene setup and serialization round-trip: PASS, exit 0; `C:/tmp/DimensionBrawl-OlympusChapterHubReview-Setup-11.log`.
- Standalone scene/canonical-boundary verification: PASS, exit 0; `C:/tmp/DimensionBrawl-OlympusChapterHubReview-Verify-Final.log`.
- `ChapterHubReviewSessionPlayModeTests`: 11/11 passed; `C:/tmp/DimensionBrawl-CHUB01-Session-Final.xml`.
- `OlympusChapterHubReviewControllerPlayModeTests`: 8/8 passed; `C:/tmp/DimensionBrawl-CHUB01-Controller-Final.xml`.
- `CanonicalUiRoutePlayModeTests`: 34/34 passed; `C:/tmp/DimensionBrawl-CHUB01-CanonicalUiRoute-Final.xml`.
- `OlympusCorridorActualPlayPathTests`: 2/2 passed; `C:/tmp/DimensionBrawl-CHUB01-ActualPlayPath-Final.xml`.
- Total PlayMode evidence: 55/55 passed, consisting of 19 CHUB-01 tests and 36 existing canonical regressions.
- The setup verifier compared SHA-256 values for the canonical catalog/route/screen/playable-stage, corridor/station definitions, template, result/progression assets, canonical gameplay and UI scenes, and the canonical StageSelect prefab before and after the operation. No boundary asset changed.

## Visual QA plan

Automated captures must use the real configured review scene and exact output dimensions, not editor-window screenshots.

Minimum matrix:

- `ChapterHub` at 1920, 2400, and 2520 x 1080.
- `StageMap` at all three resolutions.
- canonical `StageDetail` at all three resolutions.
- `InProduction` `StageDetail` at all three resolutions.
- `Announced` `StageDetail` at all three resolutions.
- `ReviewConfirm` at all three resolutions.

This is a minimum of 18 reviewed PNG captures plus a machine-readable report. QA must verify safe-area fit, no clipped/wrapped controls, no overlapping badges, no stretched art, readable detail copy, correct hidden rows, touch-target spacing, and the absence of reward/cost/lock/clear imagery or text.

Visual QA is not complete until a human reviews the generated contact sheet or individual captures and records findings. Temporary defects must be fixed and the affected matrix recaptured.

### Recorded visual QA result (2026-07-18)

- Automated capture check: PASS, 18/18 PNGs across all six states at `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`.
- Output directory: `C:/tmp/DimensionBrawl-OlympusChapterHubReview-QA`.
- Machine-readable manifest: `C:/tmp/DimensionBrawl-OlympusChapterHubReview-QA/capture-manifest.json`.
- Human-readable automated report: `C:/tmp/DimensionBrawl-OlympusChapterHubReview-QA/capture-report.md`.
- Human visual review: PASS. All 18 individual captures were inspected for safe-area fit, title/node/detail readability, placeholder semantics, hidden unverified rows, CTA separation, background behavior, and confirmation state.
- Review findings fixed before the final recapture: card/node/header pivot alignment, node label overlap, detail objective/story/segment clipping, detail/CTA overlap, empty placeholder detail, confirmation acknowledgment labeling, and virtual safe-area application.
- No remaining P1/P2/P3 visual finding was recorded. The final images contain no reward, cost, lock, clear, score, completion, or route-dispatch claim.
- The capture manifest intentionally retains `HumanReviewed=false`: the automated harness may report only machine evidence and must not self-attest human inspection. This document is the separate human-review record.

## P1 risks

- Adding either review slot to `DB_UIStageCatalog.asset` violates the canonical single-entry invariant and can break all stage projections.
- Reusing `StageSelectScreenPresenter.HandleStartClicked` or adding a router reference can load the real combat scene from a review sample.
- Treating production status as progression can silently introduce fake lock/clear semantics.
- Filling disposition-hidden rows with plausible values can turn unverified reward, cost, threat, power, or duration into false product data.
- Holding a projection across a catalog generation change can display stale canonical data; configuration must be refreshed when the review scene/session is rebuilt.
- Allowing placeholder IDs into a future service payload can make review-only content externally addressable. Service exposure remains zero for CHUB-01.

## Definition of done

CHUB-01 is complete only when all of the following are true:

- The four-state review flow is implemented in the independent review scene.
- One canonical stage plus explicit `InProduction` and `Announced` slots are visible and correctly differentiated.
- Static definition, transient review state, routing, and service exposure remain separated as specified.
- No fake reward, cost/stamina, lock, clear, score, stars, completion, or account state is displayed or stored.
- Canonical briefing rows obey dispositions and fail closed.
- The review scene remains outside Build Settings and contains no router/loader.
- `StageRunRuntime`, results, rewards, and progression remain unchanged.
- Controller PlayMode tests, scene validation, and existing canonical regression suites pass with recorded counts and logs.
- The 18-capture mobile visual QA matrix passes at `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`, with a human-reviewed report and output path recorded.
- Canonical assets are proven unchanged by the setup/verification evidence.
- A page titled `CHUB-01 - Olympus Chapter Hub Review Vertical Slice` is created or updated in the user's Docker-hosted Notion service without a browser extension. It records this document path, final scene/controller/test paths, ArkData provenance paths, test results, visual QA report/output, deferred work, and the implementation commit hash.
- The completed slice is committed in a focused git commit. The commit hash and subject are recorded in both the Notion page and final handoff, and unrelated worktree changes are not included.

The implementation and verification evidence above promotes CHUB-01 to a verified review slice only. It remains `TEMP_DO_NOT_SHIP`, outside Build Settings, disconnected from product routing, and non-shippable until the deferred product decisions are implemented through separate tasks.

## Deferred product decisions

- Product-level Chapter Hub and Stage Detail route ownership.
- A multi-stage canonical catalog schema and migration away from the single-entry projection invariant.
- Account progression, stage unlock rules, clear grades, replay state, first-clear state, stamina/cost, and reward services.
- Live-service availability, maintenance, download, event schedule, and announcement feeds.
- Combat deployment, team formation, loading cards, and `StageRun` admission.
- Final chapter art, stage thumbnails, iconography, animation timing, localization, accessibility, audio, and haptics.

These are separate product tasks and must not be simulated by CHUB-01 review data.
