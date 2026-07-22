# B0-4 Multi-Entry Selection and Build Manifest

Status: `IMPLEMENTED / VERIFIED`

Date: 2026-07-21 KST

## Outcome

B0-4 removes the one-catalog-row assumptions between Stage Select, playable-stage
projection, validation, and Unity Build Settings. The current product still authors one
Olympus catalog entry. Dynamic fixtures prove the multi-entry seams without changing that
accepted product row or pretending that the second compact stage already exists.

The new boundary is strict: every catalog identity, route, result/progression sidecar,
route segment, card binding, and physical scene must agree before a product build route is
accepted. An invalid later row is never skipped to produce a partial build.

## Catalog projection contract

`UIStageCatalog` now accepts one or more rows and validates the complete entry-ID cohort
before every public lookup, projection, currentness check, and digest computation. A blank
or duplicate ID anywhere fail-closes named, first, indexed, and digest paths, including a
query for an otherwise unique row.

Projection digest computation is an authoring operation and therefore does not require the
entry scene to be in Build Settings. Runtime projection creation and currentness checks
still require that membership. This split removes the former circular dependency in which
the build-settings tool could not discover a new scene until that same tool had already
added it.

The catalog generation remains `2`, and the accepted Olympus projection digest remains:

`571b79d2fb47619383be714f88870752c4f8e1ce4d2864d6dc846307aecb6f1d`

A real second product row in B1 must bump the catalog generation and recompute every row's
projection digest together. B0-4 does not do that migration early.

## Exact Stage Select card bindings

Each `StageFocusEntry` now binds four identities as one bundle:

1. catalog entry ID;
2. the card's exact `Button`;
3. the same card's `RectTransform`; and
4. its chapter focus target.

When `requireExactStageCardBindings` is enabled, the presenter requires a bijection between
catalog rows and card bindings, unique IDs, unique buttons, and `button.transform ==
stageTarget`. Runtime listeners are stored and removed by exact delegate identity, so
disable/re-enable cannot accumulate callbacks. Invalid bindings expose no selectable route.

The current product prefab binds only `story_v1_training_route` to `01-1_StageCard`.
`01-2`, `01-3`, and `01-4` remain authored shells but are inactive, non-interactable, and
block no raycasts. B1-2, not B0-4, will populate a real second card.

## Route-derived product build manifest

`UIProductBuildRouteManifest` is a pure runtime data seam. It depends on neither
`AssetDatabase` nor current Build Settings. It walks:

```text
UIScreenRouteTable authored rows
  -> UIStageCatalog authored rows
    -> every StageRunRouteSnapshot segment in sequence
      -> Stage Clear
        -> first-occurrence physical-scene deduplication
```

It seals UI-route evidence, catalog identity, playable-stage identity, projection digest,
route digest, result-definition identity, progression-node identity, every logical segment,
the deduplicated physical scene order, and a deterministic SHA-256 manifest digest.
Duplicate reachable `PlayableStageId`, `ResultDefinitionId`, or `ProgressionNodeId` values
are rejected rather than treated as independent content.

The revision-2 Olympus route owns two logical segments, but both currently point at the
continuous Corridor host. The truthful product Build Settings are therefore:

1. `UI_Login`
2. `UI_Lobby`
3. `UI_StageSelect`
4. `OlympusCorridorInvasionStage`
5. `UI_StageClear`

The legacy standalone `OlympusStationCombatStage` is no longer appended by convention.
The scene asset remains available for explicit review and legacy tests; it is simply not a
reachable product-build scene for the accepted route revision.

## Transactional editor apply and validation

`UIV1BuildSettingsReadinessReporter` uses the same manifest for report, verification, and
apply. Apply first validates every referenced `SceneAsset`, stores the old settings, writes
the exact manifest order, then recreates and revalidates every runtime catalog projection.
Any failure restores the previous settings before throwing.

`PlayableStageDefinitionValidator` now enumerates every reachable catalog row, exact route,
result/progression join, and logical segment. Project-wide “exactly one result/node/graph
asset” gates were removed because review or future product assets may coexist. The exact
Olympus one-node graph, one-profile presentation catalog, references, and canonical digests
remain named regression fixtures rather than global inventory limits.

Olympus setup and review consumers now resolve `story_v1_training_route` explicitly instead
of assuming the first or only row. Re-running those tools after B1 therefore cannot silently
erase or select a future second entry.

## ArkData structural evidence and copy boundary

The implementation uses ArkData only for responsibility separation and explicit-link
grammar. It does not copy foreign schemas or claim to reconstruct shipped runtime behavior.

- PGR `Stage.json` at
  `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\raw\alt3ri-pgr-data\2026-06-14\files\extracted_repo\PGR_Data-master\EN\bytes\share\fuben\Stage.json`
  contains 10,916 reviewed rows and keeps `StageId`, `PreStageId`, `NextStageId`, story,
  condition, restart, and presentation/reward-adjacent fields as distinct references.
- HI3 `StageData_Main.json` at
  `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\raw\nairieberry-honkaiimpactdata\2026-06-15\files\extracted_repo\HonkaiImpactData-master\Global\ExcelOutputAsset\Decrypted\StageData_Main.json`
  keeps `levelId`, `luaFile`, `preLevelID`, `PreMissionList`, `UnlockedLink`, and
  `dropList`-family fields distinct.
- Aether Gazer's reviewed helper at
  `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\enemies-stages\aether-gazer-stage-topology-wave-context.csv`
  separates stage, level, wave, node, map, and explicit next/link identities.

The high-confidence local takeaway is only: keep selection identity, executable route,
physical host, result source, and progression evidence as explicit joined domains. These
sources do not prove Unity Build Settings order, scene-loading semantics, unlock execution,
reward grants, or DimensionBrawl field names. No foreign text, ID, value, code, art, icon,
audio, layout, timing, or asset enters product data.

## Acceptance evidence

The final verification checkpoint is complete:

- `dotnet build DimensionBrawl.PlayModeTests.csproj --no-restore`: zero warnings,
  zero errors;
- Unity batch compile: `C:\tmp\DimensionBrawl-B0-4-Compile-Final.log`, exit `0`;
- focused canonical UI + exact-card matrix:
  `C:\tmp\DimensionBrawl-B0-4-Focused.xml`, `40/40 PASS`;
- route-derived Build Settings readiness:
  `C:\tmp\DimensionBrawl-B0-4-BuildReadiness-Final.log`, exit `0`;
- complete playable-stage validator:
  `C:\tmp\DimensionBrawl-B0-4-PlayableStageValidator-Final.log`, exit `0`;
- integrated route/result/UI/Olympus/summon regression:
  `C:\tmp\DimensionBrawl-B0-4-CoreRegression.xml`, `261/261 PASS`;
- final `git diff --check`: pass.

The focused matrix includes a fully independent, `HideAndDontSave` second route with its
own stage definition, route, template, localization, result presentation profile/catalog,
result definition, progression node, one-node graph, and result/progression join. It also
covers duplicate playable-stage/result-definition/progression-node identities, invalid
catalog identity in first/middle/last positions, exact two-card selection, listener
disable/re-enable, and six invalid card-binding shapes. Every rejected manifest is null;
no valid first row is emitted as a partial result.

The accepted product checkpoint remains:

- five physical Build Settings scenes;
- one product catalog entry;
- two logical Olympus route segments hosted by one Corridor physical scene;
- manifest digest:
  `b0f1a128548f8f77aae5a0670586a2ac39c504d967ef722cf9681f56cd788d6b`;
- projection digest:
  `571b79d2fb47619383be714f88870752c4f8e1ce4d2864d6dc846307aecb6f1d`;
- terminal policy digest:
  `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`;
- route digest:
  `878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0`;
- result/progression join digest:
  `d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71`.

The earlier transactional apply evidence remains at
`C:\tmp\DimensionBrawl-B0-4-ApplyBuildSettings.log`, exit `0`.

## Explicit deferrals and next gate

B0-4 does not author a second product scene, balance an encounter, add first-clear
persistence, invent rewards, or expose a second shipped card. Those remain ordered work:

1. B1-1 authors the first compact second scene and its independent route/result/progression
   sidecars through the neutral B0-3 adapters;
2. B1-2 bumps catalog generation, adds the real second row, and presents two exact cards;
3. persistent first-clear unlock follows only when both routes are real and admission can
   recheck the route-owned progression graph.
