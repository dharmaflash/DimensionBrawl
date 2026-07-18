# Olympus Stage Preparation Review Vertical Slice

Status: `PREP-01 / TEMP_DO_NOT_SHIP`

This slice is an isolated mobile-landscape review of the information and interaction that can appear between chapter-stage selection and combat. It does not replace the shipping stage flow, and it remains excluded from Build Settings.

## Product boundary

The slice deliberately proves only four things:

1. a canonical stage entry can be projected into a readable pre-stage briefing;
2. the three canonical runtime summon slots can be inspected without copying or inventing player data;
3. a reviewer can change presentation-only tier choices inside a disposable local session;
4. the resulting choices can be acknowledged without saving, dispatching, or starting combat.

The persistent scene banner is:

`CANONICAL RUNTIME PRESET / NOT A STAGE RECOMMENDATION`

PREP-01 is **not** an actual account loadout screen, an automatic or authored stage-recommendation system, a roster/ownership viewer, a progression or combat-power surface, or a combat-start implementation. The confirmation action only increments an in-memory review acknowledgement and exposes a deterministic digest for QA.

## External reference interpretation

The ArkData reference material is used as a structural comparison, not as content to reproduce. All observations below are static-data observations, not claims about either game's runtime behavior.

- Punishing: Gray Raven evidence comes from the stored `alt3ri/PGR_Data` snapshot dated `2026-06-14`, commit `856a0e4534d0854fa440040e961b74a97ba732e2`. The exact raw table is `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\raw\alt3ri-pgr-data\2026-06-14\files\extracted_repo\PGR_Data-master\EN\bytes\client\fuben\StageRecommend.json`: 634 rows with `StageId`, `CharacterType`, and `CharacterElement`. Character inventory is represented separately in `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\characters\pgr-character-roster-readable.csv`; the wider character-table inventory is `pgr-broad-character-table-summary.csv` in the same folder. That separation is the relevant lesson: stage-facing guidance must not silently become character/account data. PREP-01 therefore projects the stage catalog independently and labels the existing runtime summon profiles as a fixed presentation.
- Aether Gazer evidence comes from `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\enemies-stages\aether-gazer-stage-readable-join.csv`. Its `BattleActivityStoryStageCfg` rows expose `team_type`, `need_default_team`, `hero_list`, and resolved `hero_names` as distinct columns. This supports keeping “what a stage expects,” “whether a default team is required,” and “which heroes are listed” as separate authoring decisions. PREP-01 does not map those fields into local roster claims, infer a recommended team, or copy hero lists.

This comparison is a non-copying design basis. No external game IDs, balance values, UI art, layouts, strings, recommendation rules, account data, or team compositions are imported into the generated assets.

## Canonical inputs

The setup's declared direct hash boundary contains, but must never edit, the following canonical assets and their `.meta` files:

| Purpose | Canonical asset |
|---|---|
| Stage projection | `Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset` |
| Summon slot 1 | `Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset` |
| Summon slot 2 | `Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_LaserSoldier.asset` |
| Summon slot 3 | `Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_FireDragon.asset` |
| Existing HUD icon 1 | `Assets/_Game/UI/CombatHud/Art/DimensionHud/Hud_SummonSlot1Icon.png` |
| Existing HUD icon 2 | `Assets/_Game/UI/CombatHud/Art/DimensionHud/Hud_SummonSlot2Icon.png` |
| Existing HUD icon 3 | `Assets/_Game/UI/CombatHud/Art/DimensionHud/Hud_SummonSlot3Icon.png` |
| Existing chapter-hub background | `Assets/_Game/UI/ChapterHubReview/Art/BG_OlympusChapterHub_Review.png` |
| Responsive-layout catalog | `Assets/_Game/DesignData/UI/DB_UIResponsiveLayouts.asset` |
| Font source boundary | Pretendard Medium and SemiBold source OTF files |

The stage projection is the exact `story_v1_training_route` entry. Its title, objective, and combat lesson must match the catalog's current combat projection exactly.

`UIStageCatalog.TryCreateRouteProjection` also reads transitive stage-definition, template, result, and progression-join assets. PREP-01 re-creates and semantically validates the exact projection before every forward or confirmation action, but those transitive assets are not part of this slice's byte-fingerprint list. The SHA-256 claim below therefore applies to the declared direct hash boundary, not every dependency reachable from the catalog.

The scene directly references `TMP_Pretendard_Medium_Dynamic.asset` and `TMP_Pretendard_SemiBold_Dynamic.asset`. Unity may mutate a Dynamic TMP asset's glyph/atlas cache during authoring, so these two derived cache-bearing assets are intentionally excluded from the immutable byte boundary. The scene validator instead requires every TMP text to reference exactly one of those two assets, requires both assets to be used, and the 21-capture QA verifies the rendered result. Their source OTFs remain byte-fingerprinted.

`RecommendedLoadout`, `FeaturedThreat`, and `FeaturedSummonNeed` must each remain `NoVerifiedSource`. PREP-01 represents that state as hidden/neutral copy. It must not turn missing evidence into a blank recommendation card, zero value, locked state, mystery value, inferred weakness, suggested summon, or other recommendation-like placeholder.

## Generated outputs

The deterministic editor setup owns only:

- `Assets/_Game/DesignData/UI/Review/DB_UIStagePreparation_OlympusReview.asset`
- `Assets/_Game/Scenes/Review/UI_OlympusStagePreparationReview.unity`

The profile contains exactly three slot presentations, each bound by direct object reference to one canonical `SummonSlotActionProfile` and one existing HUD icon. Each slot exposes presentation tiers `1`, `2`, and `3`; tiers are review-session labels, not progression, level, power, rarity, ownership, or unlock state.

The editor setup and validator live in:

- `Assets/_Game/Editor/StagePreparationReview/OlympusStagePreparationReviewSetup.cs`

The runtime review-only controller and disposable session live under:

- `Assets/_Game/UI/StagePreparationReview/`

## Four-panel flow

### 1. StageIntel

The first panel presents the exact canonical stage code, title, objective, and combat lesson. It deliberately does not consume the catalog's legacy `Summary`, `ThreatTags`, or `RecommendedSummonRole` strings as admitted briefing facts. Unverified threat and summon-need fields stay neutral and hidden. The only forward action opens the loadout overview.

### 2. LoadoutOverview

The overview presents the three canonical runtime summon slots as a fixed local presentation. Each card reuses its existing HUD icon and allows inspection. The persistent boundary makes clear that the preset is not a stage recommendation. Back returns to StageIntel; Review opens the confirmation panel only when the disposable session is internally valid.

### 3. SummonDetail

The detail surface is a blocking right-side drawer over a dimmed scrim. It shows the selected canonical profile's role and review copy, then permits a session-only choice among tiers `1`, `2`, and `3`. Back returns to the overview and focuses the inspected slot again.

### 4. ReviewConfirm

The confirmation surface is a blocking modal. It summarizes the three slot/tier selections and exposes an acknowledgement action. Acknowledgement does not save anything and does not begin combat. Restart clears the disposable session and returns to StageIntel.

The intended path is:

`StageIntel -> LoadoutOverview -> SummonDetail -> LoadoutOverview -> ReviewConfirm -> acknowledged/restart`

## Mobile interaction and layout contract

- The root canvas targets mobile landscape at `1920 x 1080` with width/height matching.
- All review panels live under `UISafeAreaRoot`.
- `UIResponsiveRoot` remains on the Canvas and references both the canonical responsive-layout catalog and the `UISafeAreaRoot`.
- Every actionable control has a minimum dimension of 48 px.
- SummonDetail and ReviewConfirm include raycast-blocking scrims and block interaction with the panels beneath them.
- Exactly one of the four primary panels is interactive at a time; hidden panels do not block raycasts.
- Keyboard/controller selection is restored to a deterministic target after every transition, while touch remains the primary mobile path.
- The existing chapter-hub image is decorative background only and uses envelope-parent aspect fitting.

## Deterministic setup

Editor menu commands:

- `Tools/DimensionBrawl/Review/Setup Olympus Stage Preparation Review`
- `Tools/DimensionBrawl/Review/Validate Olympus Stage Preparation Review`

Automation entry points:

- `OlympusStagePreparationReviewSetup.RunBatchSetup()`
- `OlympusStagePreparationReviewSetup.RunBatchVerification()`
- `OlympusStagePreparationReviewSetup.ComputeCanonicalBoundaryDigest()`

Setup is idempotent: it rewrites only the two hardcoded generated outputs in their dedicated Review paths, normalizes generated scene YAML whitespace, imports the outputs, reloads the scene from disk, and validates the reloaded object graph. Existing generated outputs are rejected if they import as the wrong asset type or an unexpected object already occupies the generated asset path.

Before setup or verification, the tool captures SHA-256 fingerprints for every declared direct hash input and its `.meta`. After generation/reload validation it recomputes the fingerprints and fails if any boundary file disappears or changes. `ComputeCanonicalBoundaryDigest()` supplies the same direct boundary as one stable digest for the separate visual-QA harness; transitive projection dependencies remain covered by exact semantic projection validation rather than byte fingerprinting.

## Verification contract

Batch verification fails when any of these conditions drift:

- the profile identity, fixed-presentation boundary, slot order, IDs, titles, roles, canonical profile references, icon references, or tier arrays;
- the exact `story_v1_training_route` combat projection or any of the three `NoVerifiedSource` dispositions;
- controller/profile/catalog wiring, panel references, slot bindings, confirmation event, or initial state;
- scene camera, canvas, event system, safe-area root, responsive controller, exact `TEMP_DO_NOT_SHIP` review marker, persistent product boundary, or blocking surfaces;
- the exact 13-button set, 48 px minimum target size, or presence of persistent UnityEvent callbacks;
- forbidden ownership such as combat launchers, navigation, persistence, rewards, inventory, roster, account, or StageRun components;
- inclusion of the review scene in Build Settings;
- missing expected YAML GUIDs, nondeterministic generated YAML normalization, or failed disk round-trip;
- any before/after SHA-256 change to a canonical asset or `.meta` file.

## Visual QA acceptance

The separate PREP-01 capture harness exercises seven states at each of three exact mobile-landscape resolutions, for 21 captures total:

- states: StageIntel, LoadoutOverview, slot 1/tier 1 detail, slot 2/tier 2 detail, slot 3/tier 3 detail, ReviewConfirm before acknowledgement, and ReviewConfirm after acknowledgement;
- resolutions: `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`;
- virtual notch orientation alternates left/right across the matrix.

The automated harness verifies:

- no safe-area clipping, TMP clipping, overlapping labels, critical icon/title collisions, or confirmation-action collisions;
- persistent visibility of `NOT A STAGE RECOMMENDATION` in every state;
- neutral treatment of all `NoVerifiedSource` fields;
- full-screen interaction blocking for detail and confirmation;
- stable focus targets, selected-tier background/label colors distinct from two matching unselected peers, exact before/after confirmation affordances, and digest output;
- no visible account, ownership, level, power, or inferred-recommendation badges.

Automated success does not attest composition, contrast, hierarchy, or visual polish. Those qualities require a separate human pass over all 21 PNGs; the final acceptance record must keep that manual result distinct from the machine report.

## Recorded verification — 2026-07-18

The final sealed verification completed with no PREP-01 failure or skipped test:

- `StagePreparationReviewSessionPlayModeTests`: `2/2` passed;
- `OlympusStagePreparationReviewControllerPlayModeTests`: `2/2` passed;
- `CanonicalUiRoutePlayModeTests`: `34/34` passed on the fresh full-fixture run;
- `OlympusCorridorActualPlayPathTests`: `2/2` passed;
- aggregate PlayMode result: `40/40` passed, `0` failed, `0` skipped;
- setup verification: passed before capture and after capture, with the generated scene still excluded from Build Settings;
- automated visual QA: `21/21` passed across seven states at `1920 x 1080`, `2400 x 1080`, and `2520 x 1080`;
- separate human visual QA: `21/21` passed for composition, hierarchy, perceptual contrast, safe areas, clipping, overlap, icon spacing, selected-tier emphasis, and before/after confirmation affordances;
- canonical boundary digest before and after setup/capture: `2a0427d04db9a4118f1536fce8d0616e5e58e0f8de5f03b5cc4c8e598456584f`;
- both dynamic Pretendard TMP assets remained clean in Git after the final setup and capture passes.

The machine report deliberately retains `HumanReviewed: false`; the separate `21/21` result above is the human acceptance record and does not rewrite machine-produced evidence. Final temporary evidence is stored at `C:\tmp\PREP01-Final2*`, `C:\tmp\PREP01-Final2b-*`, `C:\tmp\DimensionBrawl-PREP01-VisualQA-Final2b.log`, and `C:\tmp\DimensionBrawl-OlympusStagePreparationReview-QA`.

One first-pass canonical-route run exposed a pre-existing, nondeterministic Corridor scene-unload race in `PlayableDirector.stopped -> BeginTutorial -> SpatialOneShotVfxPool.GetOrCreate`. The failing test then passed alone `1/1`, and the fresh complete fixture passed `34/34`. The stack did not enter any PREP-01 type or scene, so this review commit does not alter gameplay lifecycle code and does not count the failed attempt as sealed evidence.

Human review caught and the implementation corrected several issues before sealing: StageIntel status overflow, detail-drawer stretch, confirmation-summary overflow, loadout/detail icon-title collisions, colliding confirmation actions, a lingering post-acknowledgement action, lost inspected-slot focus on Back, and stale Tier 1 emphasis while Tier 2 or Tier 3 readouts were active. The automation now guards rendered TMP fit and overlap, exact icon separation, exact temporary-marker count, confirmation affordance transitions, inspected-slot focus restoration, selected-tier background/label assignment, exact dynamic TMP references, and the canonical immutable/semantic boundaries described above.

## Deferred shipping work

Moving beyond this review slice requires separate product and data contracts for a real player roster, equipment or team editing, authored stage recommendations, validation against unlocked content, persistence, server authority, telemetry, matchmaking, energy/cost checks, stage routing, and combat launch. None of those systems should be inferred from PREP-01 or added to the review controller.

`TEMP_DO_NOT_SHIP` remains mandatory until those owners exist, the final visual language is approved, localization replaces review fallbacks, accessibility and device coverage are completed, and the production stage-entry contract is implemented independently.
