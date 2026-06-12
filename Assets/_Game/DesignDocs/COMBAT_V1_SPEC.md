# Combat V1 Spec

Last updated: 2026-06-12 KST

## Goal

`DimensionBrawl` V1 is a direct-control action RPG combat slice with visible summon slots.

The first implementation must prove the player action feel before building the summon system. The target feel is closer to `Punishing: Gray Raven`, `Honkai Impact 3rd`, and `Zenless Zone Zero`: the player directly moves, attacks, dodges, and fights through a linear combat route, while summons are reserved as later combat resources shown in the HUD.

## V1 Slice

Build only a small combat validation slice:

1. One playable sword character candidate.
2. One basic sci-fi soldier enemy candidate.
3. One inspectable test scene or combat area.
4. Manual short basic attack chain, starting from 3 hits and allowed to grow to 5-7 curated hits when animation support stays readable.
5. Dodge with short invulnerability or damage-avoidance timing.
6. Health and hit feedback for player and enemy.
7. Basic enemy pressure sufficient to test movement, attack, dodge, and damage.
8. Clear/fail condition for a small encounter.

Do not start by implementing the summon system, progression, currencies, boss phases, stage reward loops, daily tasks, or a full mobile UI shell.

## Screen Direction

Use the current visual target as the HUD direction:

- Top left: pause, timer, and current area objective.
- Bottom left: movement joystick on mobile.
- Bottom center: player HP/resource readout.
- Bottom right: primary attack, dodge, and future skill buttons.
- Top right: three summon slots with portraits, cooldown/count/resource state.
- Top right utility: settings/menu.

For V1 implementation, the HUD may be placeholder-only. The important rule is that PC/gamepad input and mobile buttons map to the same gameplay actions.

## Canonical Input Actions

Use these action names as the shared vocabulary across PC, gamepad, and mobile HUD.

| Action | PC / Keyboard Mouse | Gamepad | Mobile HUD | V1 implementation |
|---|---|---|---|---|
| `Move` | WASD / left stick equivalent | Left stick | Left joystick | Required |
| `Look` / `TargetBias` | Mouse / camera stick equivalent | Right stick | Optional drag or auto-bias | Minimal camera/target direction support |
| `BasicAttack` | Left click or attack key | Face button | Large attack button | Required |
| `Dodge` | Shift / space / dodge key | Face button | Dodge button | Required |
| `Skill1` | Key/button | Face/shoulder button | Skill button | Placeholder only |
| `Ultimate` | Key/button | Shoulder/face button | Ultimate button | Placeholder only |
| `SummonSlot1` | Number/key | D-pad/shoulder combo | Top-right summon slot 1 | UI direction only |
| `SummonSlot2` | Number/key | D-pad/shoulder combo | Top-right summon slot 2 | UI direction only |
| `SummonSlot3` | Number/key | D-pad/shoulder combo | Top-right summon slot 3 | UI direction only |
| `Pause` | Esc | Start/Menu | Pause button | Optional |

Existing Unity sample actions may be renamed or wrapped later, but gameplay code should not invent separate PC-only and mobile-only action paths.

## Player Action Requirements

The player owns movement, facing, attack input interpretation, dodge, animation requests, and local hit response.

V1 required behavior:

- Move in a readable third-person/ARPG combat space.
- Face or bias toward the current threat while attacking.
- Execute a short manual basic attack chain.
- Allow movement-to-attack and attack-to-dodge flow without long input lock.
- Apply damage to the basic soldier through authored hit windows or simple melee hit checks.
- Receive damage and show hit feedback.
- Die or fail the encounter when health reaches zero.

V1 excluded behavior:

- Long combo strings that are not supported by curated animation clips, readable timing, and cancellation rules.
- Full character switching.
- Parry as a baseline button.
- Runtime-built player prefab composition.
- Auto-generated full UI hierarchy.
- Hidden fallback logic that masks missing scene setup.

## Enemy Validation Requirements

The first enemy should be a basic sci-fi soldier, promoted from raw imported assets only when needed.

The soldier must provide enough pressure to validate player actions:

- Idle/approach.
- One simple attack or ranged pressure pattern.
- Health, hit reaction, and death.
- Clear attack telegraph if it can damage the player.
- Readable windup/active presentation can use a narrow presentation component before final enemy animations are promoted, but that component must not own target choice, damage, or pattern state.
- No complex squads, boss phases, affixes, or summon-answer mechanics yet.

The first enemy AI should still prepare the correct shared contract for later summons:

- Enemy and future summon actors share the same small target-sensing, team, and hostile-selection rules.
- Target candidates should be authored or supplied by encounter code; do not use scene-wide searches as the normal AI targeting path.
- The first soldier can use the reference `ClosePunish` pattern shape: track, windup, melee burst, recover.
- Enemy type id, pattern id, visual model, Animator Controller, animation trigger names, and tuning values stay serialized/prefab-level data.
- Do not hardcode sci-fi soldier model paths, animation clip paths, material paths, or `_Imported/` paths in behavior code.
- Do not create a broad AI manager or monolithic enemy brain for one soldier test.
- When the imported soldier art is promoted, start from the smallest useful animation set: `Idle`, `Run`, `Attack`, `Hit`, and `Death`. Seconds-based reference data should justify serialized timing windows and tests, not become hidden hardcoded frame logic inside runtime behavior.

If the imported soldier art is not ready for game-owned promotion, use a temporary authored placeholder in the test scene and keep the raw pack under `Assets/_Imported/`.

## Summon Boundary

Summons are part of the game's identity but are not the first implementation target.

For this V1 spec:

- Reserve three top-right summon slots in the screen model.
- Keep the future roles open for later `Break`, `Tank`, `Arrow`, and `Heal` decisions.
- It is acceptable for enemy code to share team and target-sensing contracts with future summons, because this prevents duplicate AI paths later.
- Do not implement summon targeting, summon AI, cooldown economy, or summon animations before the player action loop is playable.
- Do not return to hand-of-cards UI or direct target-selection UI as the default summon control.

The first summon implementation should happen after the player can move, attack, dodge, damage a basic soldier, take damage, and finish a small encounter.

## Linear Combat Route

The intended game shape is a linear action route: the player clears combat sections and reaches the final encounter.

For the first implementation, do not build the whole route. Build one small validation section that can later become the first combat pocket.

Future route shape:

1. Entry read.
2. Basic enemy pressure.
3. A stronger combat pocket that creates a reason to use summons.
4. Final encounter.
5. Result/next action.

Only step 2 is required for the immediate player-action slice.

## Asset Rules

- Raw asset packs stay under `Assets/_Imported/` and remain ignored by Git.
- Promote only selected player, enemy, material, and animation assets into `Assets/_Game/`.
- Prefer authored prefabs and scene objects over runtime construction.
- If an asset is only being inspected or converted, do not commit it as game-owned content.

Initial asset candidates:

- Player: CombatGirl sword/shield character pack.
- Enemy: Protofactor sci-fi soldier pack.
- Boss/future summon visual: dragon assets, not part of the first action slice.

## Implementation Order

Implement in this order:

1. Promote or author the smallest player test prefab.
2. Create a small game-owned combat test scene.
3. Implement movement and facing.
4. Implement manual basic attack chain with explicit timing windows, starting from 3 hits and expanding toward 5-7 only when promoted clips support it.
5. Implement dodge.
6. Add health, damage, and hit feedback.
7. Add one basic soldier enemy.
8. Add clear/fail condition.
9. Add placeholder HUD/action prompts only after the action loop is testable.
10. Revisit summon slot interaction.

Stop before adding more than three new gameplay scripts without reviewing ownership.

## Acceptance Checklist

The first action slice is acceptable when:

- The player can move, attack, dodge, take damage, and defeat one basic soldier.
- The test scene is inspectable in Unity and does not rebuild itself at runtime.
- PC/gamepad/mobile HUD plans share the same canonical action names.
- The code does not depend on raw `_Imported` paths, hardcoded asset GUIDs, or broad scene searches.
- The first implementation does not include summon behavior beyond documented placeholder slots.
- A reviewer can identify which object owns movement, attacks, enemy behavior, health, and encounter completion.
