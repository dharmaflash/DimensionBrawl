# Combat V1 Spec

Last updated: 2026-06-15 KST

## Goal

`DimensionBrawl` V1 is now a fixed-rear-camera boss-barrage lane slice where the playable character survives, pressures, and charges summon energy while summons provide the main battlefield exchange.

One-line direction: a 3D summon standoff action game where the player stays behind the uncrossable midline, dodges boss projectiles from a fixed rear view, earns summon energy by taking forward-position risk, and uses summons to push/pull the front line.

The first implementation must prove a small but honest loop:

- The player can move and dodge inside the back half of the lane without ever crossing the midline/forward boundary.
- A far boss or boss proxy continuously sends readable projectiles toward the player side.
- Moving closer to the forward boundary charges summon energy faster.
- Staying farther back is safer because projectile spacing is looser, but summon energy gain is weaker.
- Energy accumulates through `EN LV1 -> EN LV2 -> EN LV3`; waiting longer unlocks stronger skill/summon tiers, while spending resets the energy climb.
- One summon can enter or act as the primary battlefield exchange against the far threat.
- Boss, player, enemy, and summon target/team rules stay shared and inspectable.

The previous CombatGirl melee action foundation is preserved as a useful movement, camera, hit, enemy, and authoring checkpoint. It is not the new V1 north star.

## V1 Slice

Build only a small boss-barrage + summon-first validation slice:

1. One fixed rear camera lane.
2. One player movement area with a forward boundary the player cannot cross.
3. One far boss or boss proxy that fires repeated readable projectile patterns.
4. One summon-energy gain rule based on how close the player is to the forward boundary.
5. One shared `EN LV1~LV3` tier ladder for the first skill and first summon.
6. One summon slot that can call one reviewed summon actor or assist action at the current available EN tier.
7. One simple skill action that can fire immediately at the current available EN tier.
8. One simple player local-defense action for close or approaching threats. It may be a short slash, a short magic projectile, or gun-like fire depending on the reviewed asset direction.
9. Health, hit feedback, projectile hit/fail rules, and clear/fail conditions.
10. Placeholder UI for the top-right summon slots and skill readiness, with slot 1 functionally validated first.

Do not start by rebuilding the full melee combo loop, full summon roster, summon economy, boss phases, stage reward loops, daily tasks, or a full mobile UI shell.

## Screen Direction

Use the current mobile-first action HUD direction, but make the summon slots a primary signal instead of a future placeholder:

- Top left: pause, timer, and current area objective.
- Bottom left: movement joystick on mobile.
- Bottom center: player HP/resource readout.
- Bottom right: local-defense basic action, dodge, and future skill buttons.
- Top right: three summon slots with portrait, ready/cooldown/count/resource state.
- Top-right summon slot 1 is the first functional slot.
- Top right utility: settings/menu.
- Camera: fixed rear/over-the-shoulder battlefield view for the first slice. Do not add free orbit as a baseline control until the boss-barrage read is proven.

For V1 implementation, the HUD may stay simple. The important rule is that PC, gamepad, and mobile buttons map to the same gameplay actions.

## Canonical Input Actions

Use these action names as the shared vocabulary across PC, gamepad, and mobile HUD.

| Action | PC / Keyboard Mouse | Gamepad | Mobile HUD | V1 implementation |
|---|---|---|---|---|
| `Move` | WASD / left stick equivalent | Left stick | Left joystick | Required |
| `Look` / `TargetBias` | Mouse / camera stick equivalent | Right stick | Optional drag or auto-bias | Optional aim/target bias only; first slice camera stays fixed rear |
| `BasicDefenseAttack` | Left click or attack key | Face button | Large attack button | Required |
| `Dodge` | Shift / space / dodge key | Face button | Dodge button | Required |
| `Skill1` | Key/button | Face/shoulder button | Skill button | Required first tiered skill action |
| `SummonSlot1` | Number/key | D-pad/shoulder combo | Top-right summon slot 1 | Required first tiered summon action |
| `SummonSlot2` | Number/key | D-pad/shoulder combo | Top-right summon slot 2 | Placeholder only |
| `SummonSlot3` | Number/key | D-pad/shoulder combo | Top-right summon slot 3 | Placeholder only |
| `Ultimate` | Key/button | Shoulder/face button | Ultimate button | Placeholder only |
| `Pause` | Esc | Start/Menu | Pause button | Optional |

Existing Unity sample actions may be renamed or wrapped later, but gameplay code must not invent separate PC-only and mobile-only action paths.

## Player Requirements

The player owns movement, dodge, local-defense attack input, target-bias intent, animation requests, and local hit response.

V1 required behavior:

- Move in a readable fixed-rear camera lane.
- Stay constrained to the player side of the lane; the midline/forward boundary is the closest legal position to the boss and must never be crossed by player movement.
- Gain summon energy faster near the forward boundary and slower near the back of the lane.
- Use one simple local-defense attack against close or approaching monsters on the player side. The implementation can be a short melee slash, a short magic projectile, or gun-like fire.
- Aim or bias that local-defense attack toward the current close threat, facing direction, or corridor lane target.
- Dodge out of simple enemy pressure.
- Trigger `SummonSlot1` as the main tactical combat answer.
- Decide whether to spend current EN on `Skill1` / `SummonSlot1` now or wait for a higher EN level.
- Read and dodge incoming boss projectiles with perspective/density differences between front and back positions.
- Take damage, show hit feedback, and fail when health reaches zero.

V1 excluded behavior:

- Full rifle animation pack dependency before the boss-barrage/summon loop proves itself.
- Melee combo expansion as the main product direction.
- Full character switching.
- Parry as a baseline button.
- Runtime-built player prefab composition.
- Auto-generated full UI hierarchy.
- Hidden fallback logic that masks missing scene setup.

## Player Local-Defense Attack Boundary

The first player attack is a validation tool for local defense, not the main boss-killing system.

- Prefer one authored short attack path: either a small melee arc, a short projectile prefab, or an authored cue object with serialized range, lifetime, radius, damage, and impact VFX references.
- Damage authority must stay in combat code, not in a pure visual cue.
- Pooling is allowed when scoped to one projectile/attack owner or cue player.
- Do not scene-search for targets every frame as the default.
- Do not hardcode asset paths, GUIDs, materials, or scene object names.
- Do not expand this into a melee combo tree, full shooter controller, or boss primary-DPS route before the summon loop proves itself.
- If the visual starts as magic or slash, do not install or wire rifle animation packs just to prove the local-defense timing.

## Boss Barrage Lane Rules

The first product shape is closer to a boss-barrage standoff than a free chase arena.

- The boss or boss proxy stays far beyond the player's forward boundary.
- The camera stays behind the player and looks forward along the lane.
- The player cannot cross the lane midpoint/frontline. The forward boundary is a designed risk line, not a door to the boss. The space beyond that line belongs to summons, boss pressure, and enemy/frontline exchange.
- The boss repeatedly fires projectile patterns toward the player side.
- Close or approaching monsters may enter the player side and should be answerable through the player's local-defense attack.
- Projectile readability should use perspective and authored pattern spacing: closer to the forward boundary, gaps between incoming projectiles feel tighter; farther back, gaps feel wider and safer.
- This risk difference must be expressed through authored pattern data or lane-space sampling, not only through a camera illusion.
- Summon energy gain should increase as the player occupies riskier forward space.
- Backline play is allowed as a safety choice, but it should not charge the summon fast enough to be the dominant strategy.
- The first slice may use a boss proxy and simple projectile primitives. It should not start with a full boss phase controller.

## Energy Tier Rules

The first strategic layer is a shared energy tier ladder.

- Implement only `EN LV1`, `EN LV2`, and `EN LV3` first.
- Energy gain is continuous and affected by lane position; forward-risk positioning charges faster than backline positioning.
- When `EN LV1` fills, `Skill1 LV1` and `SummonSlot1 LV1` become available.
- After `EN LV1` fills, the meter starts charging toward `EN LV2` while the LV1 skill/summon buttons remain available.
- If the player uses `Skill1 LV1` or `SummonSlot1 LV1` before `EN LV2` fills, the meter resets to empty `EN LV1`.
- When `EN LV2` fills, the available buttons upgrade to `Skill1 LV2` and `SummonSlot1 LV2`, then the meter starts charging toward `EN LV3`.
- If the player uses `Skill1 LV2` or `SummonSlot1 LV2` before `EN LV3` fills, the meter resets to empty `EN LV1`.
- When `EN LV3` fills, the available buttons upgrade to `Skill1 LV3` and `SummonSlot1 LV3`.
- LV3 is the first implementation cap. Do not add LV4+, rarity ladders, inventory, or upgrade economy in this slice.
- Higher EN levels should be stronger versions of the same skill/summon concept, not unrelated new systems.
- The first UI only needs to show current charging level, current available level, and whether skill/summon spend is available.
- Skill use should fire immediately.
- Summon use should appear from a magic circle directly in front of the player character, then enter/launch into the frontline exchange.
- Enemy boss/proxy may later use skill or summon-like actions, but V1 should not build a full symmetric boss summon system before the player EN ladder works.

## Summon Boundary

Summons are now the first product identity, but the first implementation is still narrow.

For this V1 spec:

- Implement only `SummonSlot1` first.
- Use one reviewed summon actor or assist action.
- Treat `SummonSlot2` and `SummonSlot3` as visible future slots only.
- The first summon may be a short assist, a short persistent helper, or a role actor, but it must create a visible battlefield exchange with the far boss/proxy or its pressure.
- `SummonSlot1` should have LV1, LV2, and LV3 versions of the same summon concept. Higher levels may improve power, duration, count, projectile strength, or impact, but should stay inspectable as tier data.
- Spending any summon level resets EN back to empty LV1 charging.
- The summon must use shared team, hostile-target, and authored candidate rules with enemies.
- The summon must not be a random auto-pet that plays invisibly in the background.
- The summon must not use a hand-of-cards UI, manual target-selection UI, or broad placement-drag UI as the default V1 verb.

Use `SUMMON_SYSTEM_REFERENCE_RESEARCH.md` actively. The first data contracts should be shaped by:

- `SummonOpportunityWindow`
- `SummonAssistEntry`
- `SummonRoleBehavior`
- `SummonCueBundle`
- target-relative or player-relative entry offsets
- explicit camera/UI/input cleanup for any cut-in-like assist

Do not build the full summon roster, cooldown economy, upgrade economy, or summon inventory before the first slot is playable.

## Enemy Validation Requirements

The first enemies and boss proxy matter because they create the need for summons.

Required enemy behavior:

- Boss-proxy projectile pressure or simple corridor pressure.
- Optional close/approaching monster pressure that asks for the player's local-defense attack.
- One or more readable attacks with windup, active, recovery, and clear telegraph.
- Health, hit reaction, and death.
- Enough pressure to make forward positioning and `SummonSlot1` useful.
- Shared team/target rules that can later serve summons.

Allowed reuse:

- Existing `CombatAiPatternProfile`, `CombatAiPatternDeck`, role profiles, and role candidate prefabs remain useful.
- Existing sci-fi soldier roles may become the first summon-test threats.
- Enemy AI grammar can later be reused for allied summon actors when ownership is explicit.

Not allowed:

- Broad AI manager.
- Runtime wave generator hidden inside enemy code.
- Scene-wide target searches as normal behavior.
- Hardcoded sci-fi soldier model paths, animation clip paths, material paths, or `_Imported/` paths.

## Corridor Combat Route

The intended game shape is now a fixed-rear boss-barrage exchange:

1. Enter a readable lane or room.
2. Camera frames the far boss/proxy, player, incoming projectiles, and summon field.
3. Player moves within the back half, dodges projectiles, clears close threats with a local-defense attack, and chooses forward risk for faster summon energy.
4. `SummonSlot1` answers the pressure or creates a counter-exchange.
5. The pocket clears, the boss pressure pauses, or the next route beat opens.

The first implementation should validate one pocket, not a whole chapter.

Stage dressing and Spring Isles work are preserved as stage-art experiments. They are not the next product direction by themselves. A future corridor-friendly environment pack, such as an Olympus-style corridor demo, can become the first chapter art candidate after review and art-depot handling.

## Asset Rules

- Raw asset packs stay under `Assets/_Imported/` and remain ignored by Git.
- Raw asset packs and vendor demo scenes belong in the art depot workflow, not gameplay Git.
- Promote only selected player, summon, enemy, material, VFX, and animation assets into `Assets/_Game/`.
- Prefer authored prefabs and scene objects over runtime construction.
- If an asset is only being inspected or converted, do not commit it as game-owned content.

Initial candidate interpretation:

- Player: local-defense placeholder first; CombatGirl melee visual is reference/checkpoint, not the final control fantasy unless a short slash proves best for close-threat handling.
- Enemy: Protofactor sci-fi soldiers remain useful first threats.
- Summon: first slot can reuse a promoted enemy/creature/placeholder actor if the role and presentation are clear.
- Boss/large summon visuals: dragon assets are future candidates, not the first slot requirement.

## Implementation Order

Implement in this order:

1. Re-align docs and ownership boundaries around fixed-rear boss barrage + summon-first lane combat.
2. Define the lane-space contract: player zone, forward boundary, backline safety zone, and boss/proxy side.
3. Define the summon-energy gain curve by player forward position.
4. Define the `EN LV1~LV3` tier ladder and spend/reset rule.
5. Author one boss/proxy projectile pattern with readable front/back density difference.
6. Define the minimal `BasicDefenseAttack` action contract for close/approaching threats. Choose slash, short magic projectile, or gun-like fire by asset/readability review.
7. Implement one tiered `Skill1` action that fires immediately at LV1-LV3.
8. Implement one tiered summon slot action with one summon actor or assist at LV1-LV3.
9. Reuse the existing team/target sensing rules for player, boss/proxy, enemies, and summon.
10. Place one lane/pocket review scene that creates a summon reason.
11. Add only the smallest UI prompt or placeholder needed to test EN level, skill readiness, and `SummonSlot1` readiness.
12. Review feel before expanding summon slots, enemy packs, full boss phases, or chapter art.

Stop before adding more than three new gameplay scripts without reviewing ownership.

## Detailed Implementation Notes

Use this section as the practical order for the next implementation pass. The goal is to make one playable pocket where the player can immediately understand the new game shape: stay behind the line, dodge boss pressure, take forward risk for faster EN, then spend that EN on `Skill1` or `SummonSlot1`.

### 0. Freeze The Slice Scope

Output:

- One review scene or one duplicated review scene dedicated to the fixed-rear lane slice.
- One player-side lane with visible authored boundary markers.
- One boss proxy, not a full boss.
- One skill button path and one summon slot path.

Do not include:

- Full chapter art.
- Full boss phase data.
- Summon roster, summon inventory, rarity, or permanent upgrades.
- New melee combo expansion.
- Runtime scene or prefab construction.

Validation:

- A reviewer can open the scene and point to the player zone, forward boundary, backline zone, boss side, boss projectile source, EN display/debug readout, and summon entry point.

### 1. Lane Space First

Output:

- An authored lane object or lane config that defines `BackLimit`, `ForwardBoundary`, lateral width, player spawn, boss proxy anchor, and summon entry anchor.
- Player movement clamped to this authored space.
- Camera fixed behind the player side and aimed down the lane.

Gameplay rules:

- The player may move forward/back and left/right inside the player zone.
- The player must never cross `ForwardBoundary`, including during dodge, knockback, attack movement, or future skill movement.
- Space beyond `ForwardBoundary` is for boss pressure, summon action, and enemy/frontline exchange.

Implementation guardrails:

- Prefer serialized `Transform` anchors or a small lane-config asset over hardcoded coordinates.
- Do not search the scene for boundary objects by name at runtime.
- Keep camera orbit/free-look disabled for this first slice unless it is only a debug toggle.

Validation:

- Hold forward and dodge forward repeatedly; the player still stops at the forward boundary.
- Hold backward; the player does not leave the playable backline.
- Move laterally; the player stays inside lane width.
- Starting the scene frames player, boss proxy, and incoming lane clearly.

### 2. Forward-Risk EN Gain

Output:

- A small combat-resource owner for the shared EN ladder.
- A lane-position sampler that converts player forward position into gain-rate multiplier.
- Debug text or simple UI showing current charging level, available level, current fill, and gain multiplier.

Gameplay rules:

- Backline play is safer but charges slowly.
- Mid-lane charges at baseline speed.
- Near the forward boundary charges clearly faster.
- The difference must be large enough to feel in manual testing.

Suggested first tuning:

- Backline gain: about `0.55x`.
- Middle gain: about `1.0x`.
- Forward-risk gain: about `1.45x` to `1.75x`.
- Time from empty to LV1 at middle: short enough for repeated tests, about `6s` to `9s`.
- LV2 and LV3 may take longer, but should still be testable in one pocket.

Implementation guardrails:

- Store thresholds and multipliers in serialized fields or ScriptableObject data.
- Do not mutate shared ScriptableObjects at runtime.
- Do not tie EN directly to UI code.

Validation:

- Stand in the backline and record approximate LV1 fill time.
- Stand near the forward boundary and record approximate LV1 fill time.
- Forward fill must be obviously faster.
- EN does not increase while paused/failure/death state is active.

### 3. EN LV1-LV3 Ladder And Spend Reset

Output:

- `EN LV1`, `EN LV2`, and `EN LV3` as the only supported tiers.
- One available-tier state exposed to skill/summon input.
- One spend API that consumes the currently available tier and resets to empty LV1 charging.

Gameplay rules:

- Before LV1 fills, no skill or summon spend is available.
- Once LV1 fills, LV1 skill/summon remains available while the meter charges toward LV2.
- Once LV2 fills, available tier upgrades from LV1 to LV2.
- Once LV3 fills, available tier upgrades from LV2 to LV3 and caps there.
- Spending at any available tier resets to empty LV1 charging.

Implementation guardrails:

- Keep this as combat resource state, not player progression.
- Do not create inventory, rarity, card, or account-level upgrade data.
- Do not duplicate separate energy systems for skill and summon in this slice.

Validation:

- Fill to LV1, spend `Skill1`, meter resets.
- Fill to LV1, wait for LV2, spend `SummonSlot1`, meter resets.
- Fill to LV3, available tier remains LV3 until spent.
- Skill and summon read the same available tier.

### 4. Boss Proxy Projectile Pattern

Output:

- One boss proxy object with one authored projectile pattern.
- One projectile prefab or pooled projectile actor with damage, travel, lifetime, hit response, and cleanup.
- Pattern data that can express tighter pressure near the forward boundary and looser pressure near the backline.

Gameplay rules:

- Projectiles travel from boss side toward player side.
- The pattern must be readable from fixed rear view.
- Near the forward boundary, safe gaps should feel tighter.
- Near the backline, safe gaps should feel wider.
- Projectile collision must punish careless forward-risk play without becoming random.

Implementation guardrails:

- Projectiles may be instantiated or pooled, but only from authored prefabs and with clear cleanup.
- Do not instantiate from `Update`.
- Do not make VFX own damage.
- Do not build a full boss phase controller yet.

Validation:

- Player can avoid at least one repeated pattern by moving laterally/back.
- Standing still in the forward-risk zone is dangerous.
- Standing still in the backline is safer but not always free.
- Projectiles despawn or pool-return cleanly.

### 5. Local-Defense Attack

Output:

- One `BasicDefenseAttack` route for close/approaching threats.
- One authored hit shape or short projectile with serialized range, radius, damage, active time, and cue references.
- One simple close-threat test enemy or reused soldier role that can enter the player side.

Gameplay rules:

- This action is for local defense, not boss DPS.
- It can be slash, magic shot, or gun-like fire depending on readability.
- It should bias toward a close threat if one is inside the authored local-defense region.

Implementation guardrails:

- Do not revive the melee-combo-first direction.
- Do not install rifle animation dependency just to prove this timing.
- Do not add manual lock-on UI for this slice.

Validation:

- A close threat can be hit and defeated or interrupted.
- The attack does not meaningfully solve the far boss by itself.
- Attack movement, if any, still respects `ForwardBoundary`.

### 6. Skill1 Tier Action

Output:

- One immediate `Skill1` action with LV1, LV2, and LV3 variants of the same concept.
- Tier data controlling damage, radius/count, duration, projectile count, or presentation intensity.

Gameplay rules:

- `Skill1` fires immediately when pressed and a tier is available.
- Higher tier should feel stronger but not become a different unrelated system.
- Spending `Skill1` resets EN.

Implementation guardrails:

- Keep skill ownership separate from EN ownership.
- Do not add full skill trees, cooldown economy, or character progression.
- Reuse combat hit/VFX cue paths where possible.

Validation:

- Skill unavailable before LV1.
- LV1/LV2/LV3 each spend correctly and reset EN.
- Higher tier is visibly or numerically stronger.

### 7. SummonSlot1 Tier Action

Output:

- One `SummonSlot1` action with LV1, LV2, and LV3 versions.
- One magic-circle entry cue in front of the player.
- One summon actor or assist effect that creates a visible exchange toward the boss/frontline.

Gameplay rules:

- Summon appears from the player-side entry point, then acts toward the frontline.
- Higher tiers improve the same summon concept through strength, duration, count, projectile pressure, or impact.
- Spending `SummonSlot1` resets EN.
- Summon result should matter more than `BasicDefenseAttack`.

Implementation guardrails:

- Do not build the full summon roster.
- Do not add summon inventory, rarity, permanent upgrades, or hand-of-cards UI.
- Summon actor ownership must be separate from player input and EN resource ownership.
- Spawned summon actors need a cleanup path.

Validation:

- LV1 summon creates a visible answer.
- LV2/LV3 are stronger versions, not unrelated actions.
- Summon does not require the player to cross the forward boundary.
- Summon cleans up after duration, death, or pocket end.

### 8. One Pocket Review Scene

Output:

- A single authored review scene that demonstrates the loop in one pocket.
- Lane anchors, player, boss proxy, projectile pattern, optional close threat, EN debug/UI, `Skill1`, and `SummonSlot1`.

Review script:

1. Start at backline and observe slow EN gain.
2. Move forward and observe faster EN gain.
3. Dodge boss projectiles near the forward boundary.
4. Spend LV1 early and confirm reset.
5. Wait for LV2 or LV3 and confirm stronger skill/summon.
6. Use local-defense attack only against close threat.
7. Clear, pause, or meaningfully answer the pocket through summon action.

Implementation guardrails:

- The scene must be inspectable and authored.
- It must not hide setup in a runtime scene builder.
- It must not require raw `_Imported` asset references.

### 9. Minimal UI Hook

Output:

- Temporary but readable UI or debug readout for EN charging tier, available tier, skill readiness, and summon readiness.
- Existing canonical action names reused by PC/gamepad/mobile HUD plans.

Gameplay rules:

- The player should know whether spending now gives LV1/LV2/LV3.
- The player should understand that moving forward charges faster.

Implementation guardrails:

- UI displays combat state; it does not own EN rules.
- Do not build lobby, card UI, full mobile shell, or summon roster UI in this slice.

Validation:

- UI updates when EN fills, upgrades, caps, and spends.
- Skill and summon buttons show the same available tier.

### 10. Review Before Expansion

Only expand after the review scene proves the core loop. The next expansion choices should be made in this order:

1. Tune lane/camera/projectile readability.
2. Tune EN gain and spend timing.
3. Improve `SummonSlot1` impact and identity.
4. Add a second boss projectile pattern.
5. Add one close-threat variant.
6. Add `SummonSlot2` only after slot 1 is fun.
7. Revisit chapter art/corridor environment after the loop is accepted.

Do not use art, more enemies, or more buttons to hide a weak EN/summon loop.

## Acceptance Checklist

The first boss-barrage summon-first slice is acceptable when:

- The player can move, dodge, use one local-defense attack, take damage, and survive/fail.
- The player cannot cross the authored midline/forward boundary.
- Forward positioning charges summon energy faster than backline positioning.
- `EN LV1~LV3` fills, upgrades available skill/summon level, and resets to empty LV1 charging after spend.
- Incoming projectile spacing/risk reads tighter near the forward boundary and looser near the backline.
- `SummonSlot1` produces a visible combat result that is more important than the player's basic attack.
- `Skill1` can fire immediately at the current available EN level.
- `SummonSlot1` appears from a magic circle in front of the player and enters the frontline exchange.
- One boss-barrage pocket can be cleared, paused, or meaningfully answered through player survival/positioning plus summon action.
- The scene is inspectable in Unity and does not rebuild itself at runtime.
- PC/gamepad/mobile HUD plans share the same canonical action names.
- The code does not depend on raw `_Imported` paths, hardcoded asset GUIDs, or broad scene searches.
- A reviewer can identify which object owns player movement, local-defense attack, summon action, enemy/boss-proxy behavior, health, VFX/camera cues, and encounter completion.
