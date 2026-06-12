# UI V1 Spec

Last updated: 2026-06-13 KST

This document defines the first safe UI work split for parallel development on another PC. It turns the existing UI research into implementation boundaries for login, lobby, and combat HUD work without reopening old card/lane UI assumptions or mixing UI with gameplay systems.

## Goal

Build a small authored UI foundation that can be inspected in Unity and later connected to the action foundation.

V1 UI should prove:

- A title/login flow can enter the next screen without fake account/server complexity.
- A lobby can present the project fantasy, one primary PvE entry, and a small set of secondary anchors.
- A combat HUD can display the direct-control ARPG action vocabulary without owning combat logic.
- UI prefabs, scenes, data, and presentation cues are organized so another PC can work without touching the active combat scene.

## Parallel Work Rule

UI work may happen on another PC if it follows these rules:

- Work under `Assets/_Game/UI/`, `Assets/_Game/Scenes/UI/`, and optional `Assets/_Game/DesignData/UI/`.
- Do not edit `Assets/_Game/Scenes/ActionFoundationTest.unity` for UI layout experiments.
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

- Allowed route: `UI_LoginTest -> UI_LobbyTest -> UI_CombatHudTest -> UI_LobbyTest`.
- The flow may use fade panels, loading-card placeholders, transition duration data, and local button events.
- Scene route names or scene references must be serialized or data-driven in one small route asset/component, not duplicated as magic strings across button scripts.
- Scene flow code must not own save data, account login, progression unlocks, combat result resolution, or gameplay state.
- The combat HUD test scene may simulate `Start Combat`, `Win`, `Fail`, and `Return Lobby` with mock UI state only.
- Do not connect the flow directly to `ActionFoundationTest.unity` until an explicit integration pass.
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
- Bottom-center HP/resource placeholder.
- Bottom-right basic attack, dodge, skill, and ultimate button visuals.
- Top-right three summon slot visuals as UI placeholders.
- Top-right utility/settings placeholder.
- Event hooks or small presenter methods for `SetHealth`, `SetObjective`, `SetTimer`, `SetSummonSlotState`, `SetSkillCooldown`, and `SetInputMode`.

Not allowed:

- Actual summon behavior.
- Summon spawning, summon AI, cooldown economy, or target selection.
- Hand-of-cards UI.
- Lane-first input UI.
- Direct target-selection UI as the default control.
- UI code that calls player/enemy methods directly to apply damage, dodge, attack, or summon.
- Runtime generation of the whole HUD hierarchy.

Reference direction:

- Follow `COMBAT_V1_SPEC.md` canonical actions: `Move`, `Look` / `TargetBias`, `BasicAttack`, `Dodge`, `Skill1`, `Ultimate`, `SummonSlot1`, `SummonSlot2`, `SummonSlot3`, and `Pause`.
- Combat HUD should support fast action without stealing focus from the combat field.
- Summon slots are visual direction only until a reviewed summon slice exists.

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
- No changes to `ActionFoundationTest.unity` unless explicitly coordinated.
- No direct references to `_Imported/`.
- No full runtime UI hierarchy construction.
- Scene navigation is limited to the UI test route unless explicitly coordinated.
- No summon gameplay or economy implementation.
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
