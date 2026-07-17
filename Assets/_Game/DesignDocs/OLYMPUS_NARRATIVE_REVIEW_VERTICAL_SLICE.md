# Olympus Narrative Review Vertical Slice

Status: verified review sample  
Scope label: `REVIEW SAMPLE / TEMP_DO_NOT_SHIP`  
Canonical product state changed: no

## Outcome

The first narrative-facing UI sample is an isolated review scene with one complete flow:

`ChapterEntry → VisualNovel → TutorialCutscene → StageBriefing → Complete`

The scene demonstrates the presentation grammar expected from an early stage-based mobile action game without loading a combat scene, admitting a `StageRun`, granting rewards, or writing player progression.

## Product boundary

- Scene: `Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity`
- Narrative data: `Assets/_Game/DesignData/Narrative/Review/DB_Narrative_OlympusChapterEntryReview.asset`
- Cutscene Timeline: `Assets/_Game/DesignData/Timelines/Review/DB_Timeline_OlympusTutorialReview.playable`
- Canonical briefing source: `Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset`
- The review scene stays outside Build Settings.
- The canonical `OlympusCorridorInvasionStage` scene, its 36.57-second intro Timeline, route digests, and runtime ownership remain untouched.

## Why these responsibilities are separate

The local ArkData research snapshots show the same broad separation in several shipped stage-based games:

- PGR links Chapter and Stage records by stable IDs and lets a Stage reference entry/exit story IDs. Movie actions, actors, faces, dialogue UI, UI guides, and combat guide overlays are separate records.
- Honkai Impact 3rd links a level to a Plot dialogue range. Dialog, avatar, image/CG, in-level dialogue, story stages, and newbie guides remain separate tables.
- Aether Gazer story scripts compose dialogue UI, actor slots, Timeline/expression cues, camera, audio, background, masks, and input blocking, while chapter and battle-stage configuration remain separate.

The reusable rule is therefore:

1. Apply persistent presentation state such as background and portrait slot.
2. Execute one-shot presentation cues such as fade, SFX, or Timeline.
3. Present a localized line and optional voice.
4. Wait for tap, voice completion, time, or a constrained choice.
5. Move to the next stable node.
6. Dispatch completion exactly once.

Tutorial steps and in-combat dialogue must remain separate systems because they own different triggers, input gates, pause policies, and repeat rules.

## First sample contract

### Chapter entry

- Shows one Olympus chapter card and one available stage.
- Reads stage title and objective from a current `UIStageRouteProjection`.
- Does not invent reward, clear, lock, stamina, or account state.

### Visual novel

- One profile with stable sequence, line, and choice IDs.
- Eight staging lines using the temporary roles `system`, `field_agent`, and `operator`.
- Localization keys plus Korean staging fallback text.
- Left, center, and right portrait slots; a slot with no reviewed sprite is hidden cleanly.
- Tap-to-finish-typewriter, next, Auto, current-sequence Log, Skip confirmation, and one two-choice rejoin.
- Choice has no reward, affection, unlock, or persistent branch effect.
- Missing voice must never block progression.

### Tutorial cutscene

- New review-only Timeline and one `PlayableDirector` own playback.
- A `StageCutscenePort` records the review handoff boundary.
- Normal completion and skip call the same idempotent finalizer.
- The canonical combat cutscene runner and combat flow controller are not reused.

### Stage briefing

- Reads title, objective, combat lesson, threat, recommended summon role, target duration, and briefing digest from the canonical projection.
- Reward row is hidden when the canonical value is absent or unverified.
- The final button records review completion only; it does not load the stage.

## Mobile review matrix

- Art/layout master: 1920×1080 landscape.
- Required visual checks: 1920×1080, 2400×1080, and 2520×1080.
- Interactive targets: at least 48 logical pixels at the reference resolution.
- Dialogue remains two or three lines; text wraps instead of shrinking below the intended size.
- Critical text, choices, and controls remain inside safe-area insets.
- Ultrawide layouts extend or crop backgrounds; they do not stretch characters.

## Acceptance checks

- Normal tap flow reaches `Complete`.
- Auto pauses at the choice and while Log or Skip confirmation is open.
- Both choices rejoin and reach the same cutscene.
- Skip at VN start/middle/end reaches the cutscene once.
- Cutscene normal end and skip reach the same briefing once.
- Rapid taps cannot duplicate completion dispatch.
- Log includes only lines seen in the current session, in progression order.
- Missing voice clips do not block Auto or manual progression.
- Briefing digest matches `DB_UIStageCatalog` projection and reward remains hidden when empty.
- `StageRunRuntime` is unchanged after the complete review flow.
- Existing canonical Olympus and UI route tests continue to pass.

## Verification evidence — 2026-07-18

- Unity batch setup and independent reload verification: passed.
- Narrative session and review controller PlayMode tests: 10/10 passed.
- Existing `CanonicalUiRoutePlayModeTests` plus `OlympusCorridorActualPlayPathTests`: 36/36 passed.
- Exact-resolution visual QA: 15/15 passed across ChapterEntry, VisualNovel, TutorialCutscene, StageBriefing, and Complete at 1920×1080, 2400×1080, and 2520×1080.
- Visual QA output: `C:/tmp/DimensionBrawl-OlympusNarrativeReview-QA` with exactly 15 PNG files plus JSON/Markdown reports.
- The visual runner enters TutorialCutscene through the narrative session's real completion event, requires one cutscene finalizer dispatch before briefing/complete, and rejects any active `StageRunRuntime` context.
- Manual capture review removed empty portrait blocks, resolved briefing column overlap, and moved VN progress away from Auto/Log/Skip controls.
- Temporary background: OpenAI image generation, 1672×941, provenance and SHA-256 recorded.
- Temporary Korean operator VO: ElevenLabs Eleven v3 / System Female, approximately 7 seconds, 45 credits, provenance and SHA-256 recorded.
- The review scene remains outside Build Settings; canonical Olympus scene and Timeline assets were not modified.

## Deferred decisions

- Canon character names, event name, and final dialogue.
- Final illustration and portrait identity.
- Formal voice language, cast, and direction.
- Persistent story history, replay archive, branching, save/resume, affection, and reward integration.
- Tutorial highlight/mask steps and in-combat dialogue cues.

Generated visual or audio assets must carry model/service, prompt or script, generation date, license/provenance notes, locale, and `TEMP_DO_NOT_SHIP` status until reviewed.
