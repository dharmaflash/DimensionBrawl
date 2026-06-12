# Subculture UI Reference Research

Last updated: 2026-06-11 KST

## Baseline

This document is a research handoff for `IsekaiBrawl` UI direction. It was written after reading:

1. `PROJECT_BRIEF.md`
2. `CURRENT_STATE.md`
3. `DECISIONS.md`
4. `AGENTS.md`
5. `AI_CODE_CONTRACT.md`
6. `ARCHITECTURE_BOUNDARIES.md`
7. `HUD_COMBAT_SPEC.md`
8. `Assets/_Game/DesignDocs/ARPG_REFERENCE_RESEARCH.md`

Active extraction rule: use reference games to extract concrete UI production patterns: scene composition, information hierarchy, transition timing, prefab/resource structure, animation-event hooks, SFX categories, VFX layering, loading/result pacing, and lobby character feedback loops. The boundary is asset/code ownership: do not import proprietary textures, meshes, audio, animation clips, fonts, source code, or full raw tables as project assets.

## Scope Guard

This is a seed catalog, not a scope fence.

The set includes the previously researched action games and expands into broader subculture gacha/anime UI references. It is intended to keep main-PC work from narrowing too early. Future follow-up may add more games, video frame tags, screenshots, UI database entries, official update pages, and local capture notes.

Read the current source set as `includes PGR / Honkai Impact 3rd / Zenless Zone Zero / Silver Palace / Wuthering Waves / Genshin Impact / Honkai: Star Rail / Blue Archive / Arknights / NIKKE`, not `limited to these games`.

## Source Inventory

| ID | Game | Source type | Useful UI data | Confidence | URL |
|---|---|---:|---|---:|---|
| `zzz_app_store` | Zenless Zone Zero | Official store page | ARPG positioning, squad/combat claims, visual style/music claims, official screenshots as visual source | Medium | https://apps.apple.com/us/app/zenless-zone-zero/id1606356401 |
| `zzz_data_audio_effects` | Zenless Zone Zero | GitHub data mirror | Animation-state-bound camera/effect/audio hooks, event IDs, effect attach points, 0.08s SFX modifier sample | Medium | https://github.com/360NENZ/Dimbreath-ZenlessData |
| `pgr_app_store` | Punishing: Gray Raven | Official store page | Title/lobby/combat screenshot set, dormitory/lobby interaction claims, action/SFX presentation claims | Medium | https://apps.apple.com/us/app/punishing-gray-raven/id1571685286 |
| `pgr_tab_ui` | Punishing: Gray Raven | GitHub data mirror | UI prefab registry, UI component prefab registry, optional 3D scene prefab per UI, cache time | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab/tree/master/table/client/ui |
| `pgr_tab_lobby_loading_sound` | Punishing: Gray Raven | GitHub data mirror | Loading card schema, BGM loop/fade schema, lobby signboard character feedback schema | Medium | https://github.com/Kengxxiao/Punishing_GrayRaven_Tab/tree/master/table/client |
| `hi3_app_store` | Honkai Impact 3rd | Official store page | Bridge/login fantasy, 3D combat, in-stage events, cinematics, LITE/daily flow, official screenshots | Medium | https://apps.apple.com/us/app/honkai-impact-3rd/id1336342304 |
| `hi3_data_files` | Honkai Impact 3rd | GitHub data mirror | ChatLobby, EntryTheme, ActivityPanel, LoadingTip, StageDetail, NewFeatureGuide, AudioPackage file taxonomy | Medium | https://github.com/nairieberry/HonkaiImpactData |
| `wuwa_app_store` | Wuthering Waves | Official store page | Full-scene login/loading impression, combat controls, extreme evasion/dodge counter/QTE, cinematic story/audio | Medium | https://apps.apple.com/us/app/wuthering-waves/id6475033368 |
| `genshin_app_store` | Genshin Impact | Official store page | Open-world menu/HUD context, natural time/weather, seamless BGM mood matching, elemental VFX spectacle | Medium | https://apps.apple.com/us/app/genshin-impact/id1517783697 |
| `hsr_app_store` | Honkai: Star Rail | Official store page | World/party/menu-heavy RPG shell, real-time cinematics, facial expressions, HOYO-MiX score, combat status clarity | Medium | https://apps.apple.com/us/app/honkai-star-rail/id1599719154 |
| `blue_archive_app_store` | Blue Archive | Official store page | Relationship/lobby identity, MomoTalk/private-chat fantasy, tactical squad flow, 2D/3D animation mix | Medium | https://apps.apple.com/us/app/blue-archive/id1571873795 |
| `arknights_app_store` | Arknights | Official store page | Dense base/operation/menu shell, soundtrack/voice emphasis, strategy screen hierarchy | Medium | https://apps.apple.com/us/app/arknights/id1464872022 |
| `nikke_app_store` | GODDESS OF VICTORY: NIKKE | Official store page | Customizable lobby/music/background, interactive character presentation, battle VFX, burst skill fantasy | Medium | https://apps.apple.com/us/app/goddess-of-victory-nikke/id1585915174 |
| `silver_palace_polygon` | Silver Palace | News / preview | Bold UI direction, Victorian/gothic style, real-time switching, detective scene fantasy, gameplay preview pointer | Low | https://www.polygon.com/gaming/598703/silver-palace-rpg-trailer |

## Observed Production Data Shapes

### PGR: UI is registered as prefabs plus optional 3D scene prefabs

The public PGR table mirror includes `table/client/ui/Ui.tab` with columns:

- `UiName`
- `ParentUiName`
- `LoadWithParent`
- `UiType`
- `UiResType`
- `ClassType`
- `PrefabUrl`
- `SceneUrl`
- `IgnoreFailure`
- `CacheTime`

Observed rows include activity and stage-detail UIs that point to regular UI prefabs and, sometimes, a separate `Scene3DPrefab` path.

Practical read:

- A polished subculture UI is often a UI prefab plus a scene-space or render-space presentation object.
- Cache time is treated as explicit data, not hidden behavior.
- Parent-child UI loading is data-driven, so a lobby or event hub can open child panels without rebuilding the whole shell.

IsekaiBrawl application:

- Define future UI screen data as `screenPrefab + optional presentationScenePrefab + cachePolicy`.
- Let lobby, stage-select, and result screens have different presentation objects, not just different panels.
- Keep V2 combat HUD separate from long-lived lobby or stage UI shells.

### PGR: reusable UI components are separately registered

`table/client/ui/UiComponent.tab` has a simple component registry shape:

- `Key`
- `PrefabUrl`

Observed component names include character property panels and settings panels.

Practical read:

- Common panels are registered as reusable component prefabs, separate from top-level screens.
- The top-level screen decides composition; the component registry decides reusable chunks.

IsekaiBrawl application:

- Use component-level data for repeated widgets: `CurrencyChip`, `PrimaryActionButton`, `SummonPreview`, `StageThreatTag`, `RewardCard`, `Toast`.
- Do not let a single giant lobby prefab own every small UI element forever.

### PGR: loading screens are weighted content cards

`table/client/loading/Loading.tab` uses:

- `Id`
- `Type`
- `Title`
- `Desc`
- `ImageUrl`
- `Weight`

Practical read:

- Loading is not just a spinner. It is a weighted content deck.
- Loading cards can teach lore, mechanics, or relationship systems while masking resource work.

IsekaiBrawl application:

- Use a weighted `LoadingCard` list for story PvE tips, summon role hints, boss-threat hints, and world flavor.
- Loading art can be generic early, but the data shape should exist before polish art arrives.

### PGR: sound has loop and fade policy as data

`table/client/sound/Sound.tab` uses:

- `Id`
- `SoundType`
- `Desc`
- `Path`
- `Volume`
- `FadeCrossTime`
- `Loop`
- `LoopInterval`

Observed rows include main screen, login, victory, failure, and fight BGM.

Practical read:

- UI audio needs transition policy: fade time, loop state, loop interval.
- Login, main lobby, battle, victory, and failure are separate sound contexts.

IsekaiBrawl application:

- Use `UISoundContext` entries for `Title`, `Lobby`, `StageSelect`, `Combat`, `ResultWin`, `ResultFail`.
- Treat BGM fade/crossfade as data so scene transitions are consistent.

### PGR: lobby character feedback is conditional voice + face + action

`table/client/signboard/SignBoardFeedback.tab` includes:

- `RoleId`
- `ConditionId`
- `ConditionParam`
- `Content`
- `CvId`
- `FaceId`
- `ActionId`
- `Duration`
- `Weight`
- `Validity`
- `CoolTime`
- `FavorLimit`
- `ShowButton`
- `ActivityId`
- `ShowTime`

Practical read:

- The polished lobby character is not only a static character image.
- It is a conditional feedback system: voice, expression, action, duration, weighting, cooldown, and visibility.

IsekaiBrawl application:

- For our lobby, start with one `LobbyGuideFeedback` table for the active heroine or summon mascot.
- Minimum fields: `condition`, `lineKey`, `voiceKey`, `motionKey`, `duration`, `weight`, `cooldown`, `visibleButtons`.

### ZZZ: audio and VFX are bound to animation and combat events

The ZZZ data mirror includes ability files such as:

- `Anton_Attack_Nomral_Enhance_Sound.json`
- `Anton_Attack_Nomral_Enhance_Effects.json`
- `Anton_Camera.json`
- `Billy_CameraEvent.json`
- `AudioResourceData.json`
- `*_EffectManager.json`
- `*_CameraOverrideTrack.json`

Observed structure:

- State hooks can use `AnimatorStateName`, `FrameCountLow`, `FrameCountHigh`, `NormalizedTimeLow`, and `NormalizedTimeHigh`.
- VFX uses actions such as `AttachEffect` with `EffectPattern`, attach target, attach point, position, and rotation settings.
- SFX can be triggered by animation event IDs, apply a short modifier, then fire an audio pattern.
- One observed SFX modifier duration was `0.08`, showing that tiny audio gates can be authored as data.

Practical read:

- Polished UI and combat UI are not just animated panels. They are event bundles.
- The most expensive-looking moments come from synchronized micro-events: UI state change, camera push, VFX attach, audio transient, and cleanup.

IsekaiBrawl application:

- Define `UICueBundle` and `CombatUICueBundle` as data. Each cue can own panel animation, SFX, VFX, camera request, and cleanup.
- Bind critical UI reactions to named events: `TitleEnter`, `LobbyFocusHero`, `StageSelect`, `CombatStart`, `PerfectDodge`, `SummonCall`, `UltimateReady`, `ResultRankReveal`, `RewardClaim`.

### Honkai Impact 3rd: feature UI is split by activity, lobby, entry theme, guide, stage, and loading data

The HI3 public data mirror contains file families such as:

- `ChatLobby*`
- `EntryTheme*`
- `ActivityPanel*`
- `GeneralActivity*`
- `LoadingTipData`
- `NewFeatureGuide`
- `NewbieGuide*`
- `StageDetail*`
- `StageDetail_Effect`
- `StageDialogData`
- `AudioPackageData`

Practical read:

- Mature subculture games separate long-lived UI surfaces by purpose: lobby, activity/event, guide/tutorial, stage, loading, and audio package.
- Feature-specific UI is expected. The polish comes from a shared transition/audio language, not from one universal screen doing everything.

IsekaiBrawl application:

- Do not force login, lobby, stage select, and combat HUD into one UI manager.
- Use shared data contracts for motion/audio/cue, while keeping screen ownership separate.

## Scene UI Composition Patterns

### 1. Login / Title

Purpose:

- Brand promise before control.
- Server/account/status entry.
- Resource patch or download state if needed.
- First BGM loop and first interaction SFX.

Common hierarchy:

1. Full-bleed background or 3D character/world shot.
2. Logo and event/version mark.
3. Minimal status row: server, account, version, resource download.
4. Single primary input: `Tap to Start` or equivalent.
5. Tiny utility buttons: account, language, repair, settings.

Polish recipe:

- Use a slow ambient background loop, not a busy menu panel.
- Put the first interaction on a small sound transient plus short logo/button pulse.
- Fade/crossfade BGM into lobby instead of abruptly restarting.
- If loading is required, use weighted lore/mechanic cards with artwork and a progress bar.

IsekaiBrawl use:

- Title screen should sell `horizontal ARPG + summon-first fantasy` with one character/summon/world composition.
- Avoid dumping all currencies, events, and daily tasks before the player enters the lobby.

### 2. Lobby / Home

Purpose:

- Daily return point.
- Emotional anchor through character, summon, room, or world presence.
- Primary route to `Story PvE`.
- Secondary route to upgrades, roster, events, settings.

Common hierarchy:

1. Character or environment presentation layer.
2. Top resource/status strip.
3. Main CTA: story, battle, commission, or current event.
4. Side rail or bottom rail for features.
5. Character feedback: idle motion, conditional line, voice, expression, touch reaction.

Polish recipe:

- Character feedback is conditional and rate-limited, not random chatter every click.
- Use short idle loops, occasional attention motion, and one or two motion states per high-value condition.
- Event banners can be loud, but the main daily route must stay easy to find.
- A good lobby has a quiet BGM identity and sharp, small UI SFX.

IsekaiBrawl use:

- The lobby can be a compact `field base / summoning prep` scene.
- One heroine or guide summon should react to `return from combat`, `new reward`, `summon ready`, and `failed run retry`.
- Main CTA should be `Story PvE`, not a scattered event wall.

### 3. Stage Select / Mission Prep

Purpose:

- Turn intent into a clear run.
- Communicate stage identity, threat type, reward, energy cost, and recommended setup.
- Offer quick start and retry.

Common hierarchy:

1. World/chapter map or stage list.
2. Stage card with name, rank, rewards, threat tags.
3. Party/summon preview.
4. Primary start button.
5. Optional modifiers and challenge tags.

Polish recipe:

- Stage cards should animate in as grouped information, not as isolated text rows.
- Use map pan, card expand, and reward sparkle sparingly.
- The start button should receive the strongest SFX in this scene.
- Loading into combat should continue the selected stage's mood.

IsekaiBrawl use:

- Stage prep should expose boss pressure and summon role hints.
- Avoid direct target-selection or hand-of-cards language here.
- `Retry` should skip most stage-select ceremony after failure.

### 4. In-Game HUD

Purpose:

- Support fast action without stealing focus from the combat field.
- Keep action availability, HP, energy, objective, danger, summon, dodge, and ultimate readable.

Common hierarchy:

1. Combat field and threats.
2. Character/action controls.
3. Immediate health/resource/objective status.
4. Contextual warnings and reward flashes.
5. Low-priority log/toast.

Polish recipe:

- Use pulses and small flashes only on state changes, not constant looping emphasis.
- Important moments should bundle UI, camera, VFX, SFX, and time-scale feedback.
- Combat UI should have clear cleanup so a perfect-dodge flash or ultimate-ready effect never gets stuck.
- In action games, high-polish HUD uses brief emphasis windows measured in frames or tenths of seconds.

IsekaiBrawl use:

- Preserve the current rule: `camera + animation + VFX/SFX` reinforce `neutral auto-fire`, `perfect dodge`, `ultimate`, and `summon call`.
- Summon remains the primary action identity.
- New landscape HUD can explore horizontal ARPG control placement, but must not revive old `5-card hand`, `lane-first`, or direct target selection defaults.

### 5. Result / Reward

Purpose:

- Close the emotional beat.
- Reveal rank/rewards/progression.
- Let the player retry or return without friction.

Common hierarchy:

1. Outcome title: clear win/fail.
2. Rank/score/breakdown.
3. Reward reveal.
4. Progression update.
5. Primary next action: `Retry`, `Next`, or `Lobby`.

Polish recipe:

- Reveal in layers: outcome, rank, rewards, progression, next button.
- Use count-up and reward sparkle, but keep total result time short.
- Failure results should be faster and more retry-focused than victory screens.

IsekaiBrawl use:

- The v1 promise includes instant retry after failure.
- The fail result should show cause and retry quickly, not force a full reward ceremony.

## Transition And Cue Timing Ranges

These are first-pass authoring ranges, not final tuning.

| Cue | Suggested duration | Layer stack | Notes |
|---|---:|---|---|
| Button down / tap feedback | 0.04-0.10s | scale/color + tap SFX | Fast enough to feel direct. |
| Button confirm | 0.08-0.18s | scale punch + confirm SFX + tiny flash | Stronger than hover/tap. |
| Tab switch | 0.12-0.25s | underline/selection slide + soft tick | Avoid full panel rebuild. |
| Panel open | 0.18-0.35s | dim/backplate + panel slide/fade + open SFX | Default workhorse. |
| Panel close/back | 0.12-0.25s | reverse panel + softer SFX | Faster than open. |
| Scene fade | 0.35-0.80s | fade + BGM crossfade + loading card if needed | Use for login/lobby/stage/combat. |
| Character lobby reaction | 1.20-4.00s | motion + expression + optional voice | Must be cooldown-gated. |
| Stage card expand | 0.20-0.45s | card scale/slide + reward glint | Should preserve scanability. |
| Combat start banner | 0.40-0.90s | top banner + camera settle + SFX | Avoid blocking first input too long. |
| Perfect dodge UI flash | 0.18-0.40s | edge flash + dodge button pulse + slow cue + SFX | Must cleanup immediately. |
| Summon call UI pulse | 0.25-0.60s | summon button pulse + card shine + entry VFX/SFX | Primary identity moment. |
| Ultimate ready cue | 0.35-0.70s | button flare + short audio sting | Do not loop loudly forever. |
| Result rank reveal | 0.50-1.20s | rank stamp + reward SFX | Victory can breathe; fail should not. |
| Reward count-up | 0.60-1.50s | numbers + tick SFX + sparkle | Skip/fast-forward should be allowed. |

## Resource Taxonomy

### Screen resources

- `UIScreenPrefab`: root canvas or panel prefab.
- `UIPresentationScenePrefab`: 3D scene, model stage, or render texture setup used behind/inside a UI.
- `UIBackgroundTexture`: full-screen or panel background.
- `UICharacterPose`: 2D/3D idle pose, expression state, or Live2D-like state.
- `UIIconAtlas`: icons, currencies, buttons, elemental/threat tags.
- `UIFontStyle`: headline, button, small status, number, rare/reward text.

### Motion resources

- `PanelTransition`: enter/exit/move/scale/alpha curve.
- `ButtonMotion`: press/confirm/disabled/ready/cooldown curves.
- `CharacterMotion`: idle, greet, tap, reward, warning, return-from-combat.
- `RewardMotion`: reveal/count-up/sparkle/rank-stamp.
- `CombatHudMotion`: perfect dodge, summon call, ultimate ready, damage warning.

### Audio resources

- `BgmContext`: title, lobby, stage select, combat, victory, failure.
- `UISfx`: tap, confirm, cancel, tab, panel open, panel close, denied, toast, reward, rare reward.
- `VoiceCue`: lobby line, guide line, story prompt, result line.
- `CombatHudSfx`: dodge success, summon call, ultimate ready, objective update, low HP, danger.

### Effect resources

- `UIFlash`: button flare, edge flash, confirm glint.
- `ParticleOverlay`: reward sparkle, event banner particles, summon card aura.
- `ScreenOverlay`: loading fade, danger vignette, time-slow tint.
- `AttachEffect`: effect attached to 3D UI model, weapon, summon card, or character anchor.

## Pattern Catalog

| Pattern ID | Scene | Reference inspiration | Data to author | IsekaiBrawl use |
|---|---|---|---|---|
| `title_ambient_start` | Login | ZZZ, Genshin, Wuthering Waves | background, logo, start prompt, BGM intro, first tap SFX | Quiet full-bleed title with one strong start interaction. |
| `login_patch_loading_deck` | Login / Loading | PGR loading table, HI3 loading tips | weighted cards, image, title, desc, progress | Hide patch/loading with summon/boss/world tips. |
| `lobby_character_signboard` | Lobby | PGR signboard feedback, Blue Archive bond fantasy, NIKKE lobby customization | condition, motion, expression, voice, duration, cooldown, weight | One guide heroine/summon reacts to run state and rewards. |
| `lobby_primary_cta_anchor` | Lobby | NIKKE, Blue Archive, HSR | main CTA, feature rail, event banner priority | Keep Story PvE visible above events and secondary menus. |
| `stage_card_expand` | Stage select | Arknights operation cards, HSR world cards, HI3 stage detail files | stage card, threat tags, reward list, start SFX | Compact stage prep with boss/summon hints. |
| `stage_to_combat_mood_bridge` | Stage select -> Combat | Genshin/WuWa seamless BGM mood, PGR BGM fade data | BGM crossfade, loading card, stage art tint | Preserve selected stage mood into combat start. |
| `combat_start_banner` | In-game | HI3/ZZZ action start feel | banner text, camera settle, SFX, short input delay policy | Short `Encounter Start` cue that does not block movement too long. |
| `perfect_dodge_feedback_bundle` | In-game HUD | ZZZ dodge/camera/audio event data, PGR Matrix-like timing | HUD flash, SFX, slow tint, camera cue, cleanup | Make perfect dodge readable without adding a parry button. |
| `summon_call_primary_pulse` | In-game HUD | ZZZ assist/QTE, Wuthering intro/echo, PGR QTE | button pulse, summon card shine, entry VFX, call SFX | Core identity cue for one-slot summon call. |
| `ultimate_ready_sting` | In-game HUD | HI3/ZZZ ultimate readability | button flare, short audio sting, optional edge glow | Show readiness strongly once, then stay calm. |
| `objective_toast_compact` | In-game HUD | Arknights/HSR objective status clarity | short text, icon, toast duration, priority | Top-band oracle hint without a verbose log. |
| `result_rank_reward_sequence` | Result | Genshin/HSR/NIKKE reward flows | rank reveal, reward reveal, count-up, skip policy | Victory ceremony with fast-forward; failure prioritizes retry. |
| `rare_reward_glint` | Result / Shop | gacha UI common pattern | rarity color, glow, audio tier, particle count | Use only for true milestones, not every small reward. |
| `denied_action_micro_toast` | Any | broad mobile gacha UX | denied SFX, short reason, red/amber pulse | Explain `not enough energy` or `cooldown` without panel spam. |
| `resource_download_screen` | Login / Settings | modern large mobile games | package list, required/optional, cleanup, storage | Later mobile build can separate core/combat/voice resources. |

## IsekaiBrawl UI Direction

### Global rule

Use subculture polish aggressively, but keep functional clarity first. The project is now landscape/horizontal ARPG-facing, so the old portrait combat HUD is not a final shell. However, the useful rule remains: the player must immediately understand movement, dodge, summon, ultimate, HP, energy, objective, and danger.

### Title

- Full-screen world/hero/summon presentation.
- One clear start prompt.
- Minimal account/server/version utility.
- BGM should fade into lobby.
- If loading is needed, use weighted loading cards, not a bare spinner.

### Lobby

- Lobby is the emotional home, not just a menu grid.
- Use one character/summon signboard with conditional feedback.
- Keep `Story PvE` as the main CTA.
- Secondary features can live in a rail or compact grid.
- Use quiet idle BGM and tiny, precise SFX.

### Stage Select

- Show current run target, boss/threat, reward, and recommended summon role.
- Use stage card expansion and map/world motion.
- Start button should be the strongest SFX in this scene.
- Retry from failure should skip unnecessary ceremony.

### In-Game HUD

- Landscape HUD can explore new placement, but must not revive `5-card hand`, `lane-first input`, or direct target selection.
- Summon is the primary action identity.
- Dodge and ultimate must have stable positions.
- Combat feedback should be cue bundles, not scattered one-off effects.

### Result

- Victory: outcome -> rank -> reward -> progression -> next.
- Failure: cause -> retry -> adjust summon/return.
- Result SFX should be satisfying but skippable.

## Suggested Data Contracts

These are implementation targets for later, not code requirements for this research task.

```json
{
  "UIScreen": {
    "id": "Lobby",
    "screenPrefab": "Assets/_Game/UI/Lobby/LobbyRoot.prefab",
    "presentationScenePrefab": "Assets/_Game/UI/Lobby/LobbyPresentation.prefab",
    "bgmContext": "Lobby",
    "cachePolicy": "Warm"
  },
  "UITransitionCue": {
    "id": "StageSelectToCombat",
    "from": "StageSelect",
    "to": "Combat",
    "duration": 0.55,
    "motion": "FadeAndPush",
    "sfx": "ui_confirm_start",
    "bgmCrossfadeMs": 700
  },
  "UICueBundle": {
    "id": "SummonCallPrimaryPulse",
    "event": "SummonCall",
    "uiMotion": "SummonButtonPulse",
    "sfx": "combat_ui_summon_call",
    "vfx": "summon_card_aura",
    "cameraCue": "AssistEntryPush",
    "cleanupPolicy": "OnCueEnd"
  }
}
```

## Practical Next Extraction Tasks

1. Capture 10 to 20 second clips for each target scene: title, lobby, stage select, combat HUD, result.
2. Frame-tag each clip with `input event`, `panel motion`, `SFX`, `VFX`, `BGM change`, `camera`, and `cleanup`.
3. Build a small Unity prototype for only three cue bundles: `TitleEnter`, `SummonCallPrimaryPulse`, and `ResultRankRewardSequence`.
4. Draft the new horizontal ARPG HUD/control spec before implementing a final landscape combat HUD.

