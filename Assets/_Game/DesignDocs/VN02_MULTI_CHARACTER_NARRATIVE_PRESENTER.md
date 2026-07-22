# VN-02 Multi-Character Narrative Presenter

- Status: implemented and verified review sample
- Scope label: `REVIEW SAMPLE / TEMP_DO_NOT_SHIP`
- Canonical product state changed: no

## Outcome

VN-02 turns the Olympus narrative review scene from a one-line/one-portrait mock into a
reusable visual-novel presentation layer. It keeps up to three actors on a persistent
left/center/right stage, resolves expression portraits through a speaker catalog, focuses
the current actor while dimming the others, and exposes typewriter, Auto, choice, Log, and
Skip-confirmation state as one inspectable snapshot.

The review route remains:

`ChapterEntry -> VisualNovel -> TutorialCutscene -> StageBriefing -> Complete`

It still does not admit a `StageRun`, load combat, grant rewards, write progression, or add
the review scene to Build Settings.

## Implemented contract

- `NarrativeSpeakerPresentationCatalog` owns speaker staging names, default portrait slots,
  and `expressionId -> Sprite` mappings independently from dialogue lines.
- Each `NarrativeSequenceProfile.LineEntry` may carry an ordered list of presentation
  commands: `Present`, `HideSpeaker`, `ClearFocus`, or `ClearStage`.
- A line without commands uses the old line-owned portrait fields through a compatibility
  adapter. Existing profiles therefore do not need an immediate migration.
- Presenting an actor removes only that same actor from an old slot. Other actors persist.
- A newly presented actor is focused at alpha `1.0`; other occupied slots remain visible at
  alpha `0.48`.
- Occupancy is character state, not asset readiness. `HasPortraitSprite` separately exposes
  whether reviewed art exists, and an absent Sprite disables the `Image` instead of showing
  an empty white card.
- `NarrativeVisualNovelPresentationSnapshot` exposes the current line, reveal completion,
  Auto, choice, Log, Skip confirmation, and all three slot states for tests and future route
  adapters.
- `LastPortraitCommandStatus` gives a compact, inspectable diagnostic when a generated scene
  or catalog binding is wrong.

The Olympus sample authors one explicit ordered command per line:

| Line | Command | Resulting intent |
|---|---|---|
| 1 | `ClearStage` | Enter with no stale actors |
| 2 | `Present field_agent / Center / neutral` | Establish the field actor |
| 3 | `Present operator / Right / alert` | Keep the field actor and focus the operator |
| 4 | `ClearFocus` | Preserve both actors while narration owns focus |
| 5 | `Present operator / Right / focused` | Change expression without duplicating the actor |
| 6 | `Present field_agent / Left / alert` | Move the same actor and preserve the operator |
| 7 | `Present operator / Right / decision` | Change only the operator expression |
| 8 | `Present field_agent / Center / resolve` | Move/focus the field actor for the handoff |

## Main assets

- Runtime catalog:
  `Assets/_Game/Scripts/Presentation/Narrative/NarrativeSpeakerPresentationCatalog.cs`
- Persistent presenter:
  `Assets/_Game/Scripts/UI/NarrativeReview/NarrativeVisualNovelPresenter.cs`
- Extended narrative profile:
  `Assets/_Game/Scripts/Presentation/Narrative/NarrativeSequenceProfile.cs`
- Review controller integration:
  `Assets/_Game/UI/NarrativeReview/OlympusChapterNarrativeReviewController.cs`
- Speaker catalog asset:
  `Assets/_Game/DesignData/Narrative/Review/DB_NarrativeSpeakerPresentation_OlympusReview.asset`
- Review sequence:
  `Assets/_Game/DesignData/Narrative/Review/DB_Narrative_OlympusChapterEntryReview.asset`
- Generated review scene:
  `Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity`
- Generator and validator:
  `Assets/_Game/Editor/NarrativeReview/OlympusChapterNarrativeReviewSetup.cs`
- Exact-resolution capture runner:
  `Assets/_Game/Editor/NarrativeReview/OlympusChapterNarrativeReviewVisualQaCapture.cs`

## ArkData structural evidence and copy boundary

The implementation uses only system structure observed in the local research bank.

- PGR represents a movie as ordered action nodes with IDs and next-action links. Background,
  text, actor, and audio actions are distinct. This supports commands that are separate from
  dialogue content.
  - `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\raw\alt3ri-pgr-data\2026-06-14\files\extracted_repo\PGR_Data-master\EN\bytes\client\movie\movies\MovieMC00000BA.json`
  - `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven\camera-animation\pgr-movie-action-type-rollup.csv`
- Honkai Impact 3rd keeps fields such as screen side, avatar, emotion, lip motion, audio, and
  predecessor dialogue separate from localized content. This supports a speaker/expression
  catalog and stable dialogue IDs.
  - `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\honkai-impact-3rd\raw\nairieberry-honkaiimpactdata\2026-06-15\files\extracted_repo\HonkaiImpactData-master\Global\ExcelOutputAsset\DialogData.json`
  - the same directory's `DialogImageData.json` and `StageDialogData.json`
- Aether Gazer presentation evidence keeps actor position and effect state across individual
  commands while typewriter, voice duration, name plate, and dialogue UI have distinct
  state. This supports persistent slots plus inspectable narrative-control state.
  - `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\aether-gazer\cutscene-cinematic\aether-gazer-arvick-p0-story-presentation-context.csv`
  - the same directory's `.md` context record

No external dialogue, character names, character designs, portraits, voice, UI layout,
coordinates, timing values, identifiers, or source code were copied. The sample uses the
temporary DimensionBrawl role IDs `field_agent` and `operator` and original staging art.
Snowbreak automation observations were not used to define VN-02 presentation behavior.

## Generated portrait provenance

- Tool: OpenAI built-in image generation
- Generation date: 2026-07-20
- Reference images: none
- Status: `TEMP_DO_NOT_SHIP`; replace after character-art direction and rights review

Prompt set recorded for reproducibility:

```text
Create one horizontal three-panel expression sheet for an original adult near-future field
agent for a mobile 3D action-game visual-novel review mockup. Waist-up portrait, clean
silhouette, practical dark tactical jacket with restrained cyan accents, short dark hair,
fully original face and costume, neutral studio background, consistent camera/lighting and
identity across all panels. Expressions from left to right: neutral and observant; alert and
concerned; resolved and confident. No text, logos, trademarks, existing characters, weapons,
UI, border, watermark, or cropped head/hands. High-quality semi-realistic game key-art.

Create one horizontal three-panel expression sheet for an original adult near-future command
operator for a mobile 3D action-game visual-novel review mockup. Waist-up portrait, clean
silhouette, pale operations jacket with restrained violet/cyan interface accents and a small
original communications headset, long dark auburn hair, fully original face and costume,
neutral studio background, consistent camera/lighting and identity across all panels.
Expressions from left to right: urgent alert; calm focused analysis; firm decision. No text,
logos, trademarks, existing characters, weapons, UI, border, watermark, or cropped head/hands.
High-quality semi-realistic game key-art.
```

Generation sources retained locally:

- `C:/Users/dharm/.codex/generated_images/019f6e6f-9f82-7bf0-85de-3a3b0384105f/exec-66930be9-4181-4ef8-a473-a5f7eb06570d.png`
- `C:/Users/dharm/.codex/generated_images/019f6e6f-9f82-7bf0-85de-3a3b0384105f/exec-9780bdd7-8d5c-4d24-a43a-edb0ae51beea.png`

The triptychs were deterministically cropped with `System.Drawing`; no additional generative
edit was applied. Runtime crops live under
`Assets/_Game/UI/NarrativeReview/Art/Portraits/`:

- field agent: `Neutral`, `Alert`, `Resolve`, each 537 x 931
- operator: `Alert`, `Focused`, `Decision`, each 560 x 916

## Verification evidence - 2026-07-20

- Unity batch setup and setup-owned catalog/scene validation: passed.
- Broader presenter + narrative session + review controller + story/tutorial transition
  regression: 54/54 passed, 0 failed, 0 skipped.
  - XML: `C:/tmp/DimensionBrawl-VN02-Broad-PlayMode.xml`
  - log: `C:/tmp/DimensionBrawl-VN02-Broad-PlayMode.log`
- Exact-resolution visual QA: 15/15 passed across five route states at 1920 x 1080,
  2400 x 1080, and 2520 x 1080.
  - report: `C:/tmp/DimensionBrawl-OlympusNarrativeReview-QA/capture-report.md`
  - captures: `C:/tmp/DimensionBrawl-OlympusNarrativeReview-QA`
- The VN capture advances through the actual review controller to a two-actor state, then
  verifies a dimmed center `field_agent` and focused right `operator`, both with resolved
  Sprites.
- All captures assert that canonical `StageRunRuntime` state remains unchanged.

## Review instructions

Open `Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity`, enter Play Mode,
select the chapter/stage card, and advance the dialogue. Lines 2-3 show actor persistence;
later lines demonstrate expression changes and same-speaker relocation. Auto, choices, Log,
and Skip confirmation remain available from the existing review controls.

## Product admission gates and next slice

VN-02 is presentation evidence, not story-route authority. Product use still needs a route
owner, localization resolution, save/resume and archive policy, voice lifecycle, final art,
accessibility policy, and explicit complete/skip/cancel/unload receipts. It must not be
silently attached to a canonical combat scene.

The next highest-value content-factory slice is A1: a reusable ordered two-add encounter
executor with typed spawn/participation/quiescence evidence. After A1 owns that repeated
combat grammar, the existing chapter, preparation, VN, tutorial, and stage fragments can be
reviewed as one longer route without manufacturing progression or service authority.
