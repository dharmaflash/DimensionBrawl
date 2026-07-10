# UI V1 Spec

Last updated: 2026-06-15 KST

This document defines the first safe UI work split for parallel development on another PC. It turns the existing UI research into implementation boundaries for login, lobby, and combat HUD work without reopening old card/lane UI assumptions or mixing UI with gameplay systems.

## Goal

Build a small authored UI foundation that can be inspected in Unity and later connected to the action foundation.

V1 UI should prove:

- A title/login flow can enter the next screen without fake account/server complexity.
- A lobby can present the project fantasy, one primary PvE entry, and a small set of secondary anchors.
- A stage-select test screen can bridge lobby CTA and combat HUD without owning progression or rewards.
- A combat HUD can display the fixed-rear boss-barrage + summon-first action vocabulary without owning combat logic.
- UI prefabs, scenes, data, and presentation cues are organized so another PC can work without touching the active combat scene.
- Android/mobile-first landscape is the default layout assumption; PC is a debug surface, not the sizing baseline.

## Android Mobile-First Baseline

- Default product target is Android landscape.
- UI test scenes should use `Scale With Screen Size` with a mobile landscape reference resolution.
- Safe Area must be represented by an authored scene/prefab root, not left as a later runtime-only concern.
- UI input prompts must use common action names across keyboard/mouse, gamepad, and mobile display rows.
- Avoid hardcoded device-specific branches in UI presenters. Device differences belong in prompt/layout data.
- For contest or local test builds that need to boot directly into the UI loop, Build Settings should register the UI route scenes in route-table order, starting with `UI_LoginTest`. This is a narrow scene-list setting, not a broader ProjectSettings ownership change.

## Parallel Work Rule

UI work may happen on another PC if it follows these rules:

- Work under `Assets/_Game/UI/`, `Assets/_Game/Scenes/UI/`, and optional `Assets/_Game/DesignData/UI/`.
- Do not edit canonical combat or runtime stage scenes for UI layout experiments.
- Use separate UI inspection scenes such as `UI_LoginTest`, `UI_LobbyTest`, and `UI_CombatHudTest`.
- Use authored prefabs and serialized references. Do not build the full UI hierarchy at runtime.
- Do not reference `Assets/_Imported/` directly.
- Do not add summon gameplay, account login, networking, currencies, progression, gacha, shop, reward economy, or final mobile HUD behavior in this slice.
- If a script becomes a broad `UIManager` that owns login, lobby, combat HUD, loading, audio, transitions, and game state together, stop and split ownership before continuing.

## Folder Direction

Suggested folders:

- `Assets/_Game/UI/Common/`
- `Assets/_Game/UI/Login/`
- `Assets/_Game/UI/Lobby/`
- `Assets/_Game/UI/CombatHud/`
- `Assets/_Game/UI/Transitions/`
- `Assets/_Game/Scenes/UI/`
- `Assets/_Game/DesignData/UI/`

Do not create a single catch-all UI folder with unrelated prefabs, sprites, data, and scripts mixed together.

## Scene Flow Boundary

Other-PC UI work may include a minimal scene-flow shell if it stays UI-owned:

- Allowed route: `UI_LoginTest -> UI_LobbyTest -> UI_StageSelectTest -> UI_CombatHudTest -> UI_LobbyTest`.
- The flow may use fade panels, loading-card placeholders, transition duration data, and local button events.
- Loading cards are conditional presentation for routes with a real wait reason. Immediate UI-to-UI routes should use a short fade without a card/progress layer.
- Scene route names or scene references must be serialized or data-driven in one small route asset/component, not duplicated as magic strings across button scripts.
- Scene flow code must not own save data, account login, progression unlocks, combat result resolution, or gameplay state.
- The combat HUD test scene may simulate `Start Combat`, `Win`, `Fail`, and `Return Lobby` with mock UI state only.
- Do not connect the flow directly to canonical combat scenes until an explicit integration pass.
- Do not create a permanent all-purpose `GameManager` just to move between UI scenes.

If a transition needs persistent objects, keep them narrow:

- `UISceneFlowRouter`: one responsibility, route requests between authored UI test scenes.
- `UITransitionPresenter`: fade/loading visuals only.
- `UIScreenRouteTable`: screen id, scene name/reference, transition id, and optional loading-card id.

## Screen Scope

### Login / Title

Allowed:

- Full-screen title composition.
- Project name or temporary logo text.
- One clear start prompt.
- Minimal version/server placeholder text.
- Optional loading card placeholder using local dummy data.
- Transition request to lobby test scene through the scene-flow shell or placeholder event.

Not allowed:

- Real account login.
- Patch/download implementation.
- Server list logic.
- Daily rewards, event panels, or lobby feature rails.
- Runtime-instantiated title scene composition.

Reference direction:

- Use `title_ambient_start` and `login_patch_loading_deck` from `SUBCULTURE_UI_REFERENCE_RESEARCH.md`.
- Scene fade should be short and calm, roughly `0.35-0.80s`.

### Lobby / Home

Allowed:

- One main lobby screen or prefab.
- One primary `Story PvE` / `Start Combat` CTA.
- Compact secondary anchors such as character, summon, settings, mail, or inventory placeholders.
- One guide heroine or summon presentation slot using placeholder art/model if needed.
- Small conditional feedback placeholders such as `return from combat`, `new reward`, or `summon ready`, but only as mock display state.

Not allowed:

- Full progression loop.
- Real currencies, shop, gacha, daily task system, or reward claim logic.
- A giant lobby prefab that permanently owns every future feature panel.
- Feature-specific panels that cannot be disabled or tested independently.

Reference direction:

- Use `lobby_character_signboard` and `lobby_primary_cta_anchor`.
- Lobby should feel like an emotional home and preparation base, not just a dense button grid.

### Combat HUD

Allowed:

- Top-left pause/timer/objective placeholders.
- Bottom-left movement joystick visual placeholder for mobile.
- Bottom-center HP and `EN LV1~LV3` resource placeholder.
- Bottom-right basic attack, dodge, skill, and ultimate button visuals.
- Top-right three summon slot visuals as UI placeholders.
- Top-right utility/settings placeholder.
- Event hooks or small presenter methods for `SetHealth`, `SetObjective`, `SetTimer`, `SetEnergyTierState`, `SetSummonSlotState`, `SetSkillTierState`, and `SetInputMode`.

Not allowed:

- Actual summon behavior.
- Summon spawning, summon AI, cooldown economy, or target selection. UI may expose `SummonSlot1` input only after the gameplay slice owns the action.
- Energy gain, tier advancement, or spend/reset rules. UI displays EN state from gameplay, not the other way around.
- Hand-of-cards UI.
- Lane-first input UI.
- Direct target-selection UI as the default control.
- UI code that calls player/enemy methods directly to apply damage, dodge, attack, or summon.
- Runtime generation of the whole HUD hierarchy.

Reference direction:

- Follow `COMBAT_V1_SPEC.md` canonical actions: `Move`, `Look` / `TargetBias`, `BasicDefenseAttack`, `Dodge`, `SummonSlot1`, `SummonSlot2`, `SummonSlot3`, `Skill1`, `Ultimate`, and `Pause`.
- Combat HUD should support fast action without stealing focus from the combat field.
- `SummonSlot1` may become a functional input bridge after a reviewed gameplay slice exists. `SummonSlot2` and `SummonSlot3` stay placeholder-only until later.
- `Skill1` and `SummonSlot1` should display the current available tier (`LV1`, `LV2`, or `LV3`) once gameplay exposes EN state.

## Ownership

Use narrow scripts if implementation starts:

- `UIScreenRoot`: screen-local root binding and show/hide state only.
- `UITransitionPresenter`: visual transition timing only.
- `LoginScreenPresenter`: title prompt, version text, and start event only.
- `LobbyScreenPresenter`: lobby mock-state binding and primary CTA event only.
- `CombatHudPresenter`: HUD display state only.
- `CombatHudInputBridge`: optional UI-button-to-canonical-action bridge only. It must route through existing public input hooks, not duplicate gameplay logic.

Do not create a global UI singleton unless there is a concrete scene-loading need and the responsibility is reviewed first.

## Data Direction

Prefer small ScriptableObject or serialized data rows for:

- Screen id, prefab reference, optional presentation prefab, BGM context, and cache policy.
- Transition id, duration, easing, SFX key, and cleanup behavior.
- HUD slot id, icon, cooldown display mode, enabled state, and placeholder text.
- EN tier display data: charging tier, available tier, fill ratio, spend-ready state, and reset feedback id.
- Lobby feedback condition, line key, motion key, duration, weight, and cooldown.

Data can be placeholder-only in V1, but it should be shaped so real content can replace it later.

## Scene Composition Rule

UI test scenes should be authored:

- One Canvas root.
- One EventSystem.
- One screen prefab or screen root.
- Optional camera/presentation object if the screen needs scene-space composition.
- Optional mock data provider.

The test scene may call simple presenter setup methods in `Awake` or `Start`, but it must not construct all visual children procedurally.

## Validation Checklist

Before merging UI work from another PC:

- The branch starts from the latest pushed `main`.
- No changes to canonical combat or runtime stage scenes unless explicitly coordinated.
- For UI-loop test builds, the first enabled Build Settings scene is `UI_LoginTest`, followed by the route-table UI scenes only.
- No direct references to `_Imported/`.
- No full runtime UI hierarchy construction.
- Scene navigation is limited to the UI test route unless explicitly coordinated.
- No summon gameplay or economy implementation.
- No EN gameplay ownership. Combat HUD may show `EN LV1~LV3`, but gameplay owns charge, tier upgrade, and spend reset.
- No hand-of-cards, lane-first, or direct target-selection default UI.
- Login, lobby, and combat HUD can be inspected separately.
- Combat HUD uses canonical action names from `COMBAT_V1_SPEC.md`.
- UI scripts are presenters/bridges, not gameplay owners.
- Text fits at common landscape widths and does not overlap controls.

## Recommended First UI Tasks

1. Create `UI_LoginTest` with a title screen root and start event placeholder.
2. Create `UI_LobbyTest` with a guide slot, primary PvE CTA, and compact secondary anchors.
3. Create `UI_CombatHudTest` with static HUD layout and mock state updates.
4. Only after the three screens are inspectable, add shared transition/audio/cue data.

## Implementation Notes

### 2026-06-15 Lobby Character Presentation

- `UI_LobbyTest` may contain a separate authored presentation prefab beside the Canvas for a RenderTexture-based lobby character stage.
- The current lobby character presentation is display-only: `PF_UI_LobbyCharacterStage` frames the game-owned CombatGirl visual with a UI-only camera and renders it into `RT_LobbyCharacterStage`, while `PF_UI_LobbyScreen` displays that texture as a transparent full-screen lobby art layer behind authored UI panels.
- Lobby character skeletal motion should stay display-only. The lobby stage uses a lobby-only signboard Animator Controller through `LobbyCharacterAnimatorPresenter`, while root/tap/drag reactions stay in `LobbyCharacterStagePresenter`. This follows the PGR-style signboard/action split without binding the lobby UI to the combat/action animator state machine.
- Lobby character framing uses a PGR-style low-FOV presentation camera plus explicit viewport-fill and composition settings, so size tweaks happen in the prefab instead of through scene transforms.
- The character RenderTexture should use the 2560x1440 landscape presentation ratio used by the lobby art, and `PresentationStage` should preserve that 16:9 ratio instead of stretching across extra-wide Game views. The stage camera should frame against that target texture aspect. The framer should refit after Play-mode presenters have updated once, ignore inactive hidden variant renderers, and use visible skinned body renderers for signboard framing before falling back to all visible renderers. It should keep humanoid foot/toe bones anchored above the lower viewport margin when available, falling back to renderer bounds only for non-humanoid presentation props. This keeps the Game view from clipping the model differently than the Scene view camera preview without hardcoding weapon or mesh names.
- Lobby character idle/tap reactions should read from `LobbyGuideFeedbackCatalog` rows shaped after the PGR signboard pattern: condition, line key, voice key placeholder, motion key, duration, weight, and cooldown.
- Lobby screen input should reach the character presentation through a narrow `LobbyCharacterStageInputChannel` asset so the UI prefab and presentation prefab stay inspectable without scene-wide object searches.
- Unused imported model variants in the lobby character stage, such as extra weapon meshes, may be hidden by `LobbyCharacterStageObjectVisibility`. This is a display-only prefab override list that applies in edit and play mode so the authored scene view matches runtime, and must not become equipment, inventory, progression, or combat weapon ownership.
- Lobby character render tuning should stay local to the presentation prefab first: adjust stage lights, transparent RenderTexture camera output, RenderTexture MSAA, and lobby-only `MaterialPropertyBlock` render profiles, including local unlit/rim/outline width and shade-feather tweaks, before mutating shared CombatGirl Unity Toon Shader materials that are also used by action inspection content.
- Outfit/body clipping observed around the current CombatGirl stocking and top areas should be treated as an art/model mask issue if it remains after lobby-only render tuning. Do not hide whole skin meshes from UI code unless an authored lobby display variant confirms that exposed hands, face, and skin are still valid.
- This slice must not connect the lobby character to player control, combat controllers, summon behavior, progression, account state, rewards, or gacha/economy systems.
