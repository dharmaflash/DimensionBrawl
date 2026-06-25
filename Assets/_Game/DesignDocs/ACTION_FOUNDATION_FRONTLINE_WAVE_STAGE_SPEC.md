# Action Foundation Frontline Wave Stage Spec

## Purpose

This document fixes the next design hypothesis before implementation.

The goal is not to prove that the current idea is the final answer. The goal is to stop the next Codex pass from drifting between unrelated interpretations such as pure boss fight, lane defense, summon sandbox, or story stage.

Current strongest hypothesis:

> DimensionBrawl's first satisfying one-match loop should be a corridor standoff wave battle where the player body and boss body cannot cross the contested center line, while projectiles, waves, pressure objects, and summons can cross it.

The player should finish the pocket understanding:

> "Summons are not just damage buttons. They are the units I send across the contested line to hold, block, or break the frontline pressure that my body and the boss body cannot personally cross."

## Status

This is a design lock for the next review pocket, not a production chapter spec.

- Scene basis: `ActionFoundationBossBarrageLaneReview.unity`
- Prototype length: 75-100 seconds
- Primary test target: one complete pocket, not a full stage map
- Main question: does the summon read as frontline agency instead of an extra attack button?
- Balance policy: do not spend this pass on numeric balance tuning. Use collected reference data to lock structure, reads, roles, and stage grammar first.

## Reference Data Commitments

This pass should lean on collected data structure, not on ad hoc tuning guesses.

The most useful NIKKE reference is not a damage number. It is the way stage data is organized:

`stage row -> wave group -> monster/pressure slots -> reward/scenario/result hooks`

From the stored NIKKE data snapshot:

- `CampaignStageTable.json` has 1,167 campaign stage rows.
- All 1,167 campaign stages resolve to wave rows in the helper join.
- `stage-wave-monster-slots.csv` expands those stages into 29,489 monster slot rows.
- Non-campaign modes add 1,523 stage rows, 1,360 wave-matched rows, and 30,308 monster slot rows.
- NIKKE wave rows preserve `battle_time`, `monster_count`, `point_data`, `target_list`, `wave_data`, `background_name`, and close/mid/far monster counts.
- NIKKE monster slot rows preserve `wave_index`, `slot_index`, `wave_path`, `spawn_type`, monster ID/model, stat group, AI, movement, monster skill weapon type, and monster skill fire type.
- Common spawn types in the stored joins are `Dash`, `Drop`, `Jump`, and `Normal`.
- Common monster skill fire types include `Instant`, `ProjectileCurve`, `Suicide`, `Range`, `Barrier`, `ObjectCreate`, and `Calling`.

Translate this into DimensionBrawl as structural grammar:

- A review pocket should have a stage shell.
- The stage shell should name the combat promise, entry/result text, and reward hook.
- The stage shell should point to a short wave plan.
- The wave plan should list pressure slots, not just enemy counts.
- Each pressure slot should say what player action or summon action answers it.
- The result should confirm which wave/frontline problem was solved.

Do not copy NIKKE's exact numbers. Copy the data discipline.

## Collected-Data-Derived Rules

### Rule 1: Stage Rows Need Hooks

NIKKE stage rows keep stage category/type, time limit, theme, scenario hooks, field monster context, reward ID, and wave joins together.

For this review pocket, do not leave combat floating as a sandbox. Even without real progression, define:

- stage intent
- entry cue
- wave group ID
- success result
- fail result
- mock reward or analysis hook
- next review question

### Rule 2: Waves Need Slot Roles

NIKKE wave data does not just say "spawn enemies." It preserves wave indices, spawn paths, target lists, close/mid/far counts, and monster skill fields.

For this review pocket, every pressure slot should be typed:

- `CloseProbe`: enters or threatens the player side
- `LineProjectile`: crosses the contested line as dodge/fire pressure
- `ScreenCurtain`: asks for summon screen or timing answer
- `FrontlineBody`: can clash with summon body
- `BackPressure`: pressures from boss side and is hard to solve with body movement
- `CoreExpose`: short follow-up target after the wave answer

### Rule 3: Spawn Type Is A Read, Not A Number

Use the NIKKE spawn families as readability patterns:

- `Drop` means sudden lane arrival or pressure appearing in a marked spot.
- `Dash` means fast ground pressure that tests reaction and line holding.
- `Jump` means arcing or delayed entry that asks for prediction.
- `Normal` means stable baseline pressure.

DimensionBrawl does not need to implement these as literal NIKKE spawn systems right now. It should label its pressure slots with equivalent read families so future tuning does not become vague.

### Rule 4: Monster Skill Fire Type Maps To Player Question

Use NIKKE fire-type families as design prompts:

- `Instant`: snap fire, asks for cover/dodge timing.
- `ProjectileCurve`: visible travel, asks for screen/intercept or lane movement.
- `Suicide`: rush body, asks for early priority target or summon clash.
- `Range`: area denial, asks for reposition.
- `Barrier`: protection, asks for break/summon/angle.
- `ObjectCreate` or `Calling`: creates a new problem, asks for target priority.

For the next pass, pick only a few equivalents. Avoid a broad enemy roster.

### Rule 5: Runtime State Matters

The NIKKE LostSector/runtime bridge data is useful because it separates static stage rows from runtime state: map data, stage spawner links, cleared stage state, completed scenario hooks, reward/item flow, and clear handlers.

For DimensionBrawl, even in review form, make clear what changes after the pocket:

- `frontline stabilized`
- `wave suppressed`
- `summon route analyzed`
- `pressure source exposed`

This can be mock text now. It should still be a named state, not just `BOSS CLEAR`.

## Design Pillars

### 1. Standoff, Not Chase

The player and boss are opposing bodies on two sides of a corridor-style battlefield.

- The player body cannot cross into the boss body zone.
- The boss body cannot cross into the player body zone.
- The contested area between them is where pressure is exchanged.
- This boundary is a rule of the match, not a wall puzzle.

The player should not feel like the game forgot to let them walk forward. They should feel like this match is about projecting force across an unsafe frontline.

### 2. Waves Are The Real Stage Shape

The stage is not "hit the boss until HP reaches zero."

The boss is the pressure source and commander. The actual one-match shape is a sequence of waves:

- incoming projectile pressure
- small rush or probe units
- shield or screen pressure
- enemy summon or frontline object
- short counter window after the frontline is solved

The boss HP may exist, but it should not be the only visible objective.

Use the NIKKE-like wave grammar:

- each beat is a wave row
- each wave has pressure slots
- each pressure slot has a readable spawn/attack family
- each wave has a player answer and a summon answer
- the result hook names the solved pressure

### 3. Summon Means Frontline Agency

`SummonSlot1` must be understood as the first way to influence the contested area beyond the player's body limit.

The summon can:

- enter the contested or enemy-side battlefield
- absorb or intercept boss projectile pressure
- clash with enemy pressure bodies
- create a follow-up window
- make player `Skill1` or ranged fire matter

The summon should not read as:

- a generic damage spell
- a cooldown nuke
- a pet that passively attacks without changing the match state
- a roster preview

### 4. The Player Still Plays Actively

The summon does not replace the player.

The player still:

- dodges and strafes boss patterns
- chooses when to take forward risk for faster EN
- clears close threats on the player side
- decides whether to spend EN now or wait for a higher tier
- confirms the summon-created opening with `Skill1` or ranged fire

The desired rhythm is:

`survive -> charge -> send summon -> hold/interrupt pressure -> confirm opening -> reset into next wave`

## Non-Goals

Do not implement these in the next pass:

- full story chapter
- permanent progression economy
- gacha, roster, inventory, rarity, or upgrade systems
- multiple production summon characters
- large enemy variety
- pure boss phase manager
- full Nikke clone camera/input structure
- cinematic-heavy tutorial
- full stage select reward claim loop

The next pass is allowed to show placeholder reward text, but it must not invent a real economy.

## Core Rule Model

### Zones

Use three conceptual zones, even if the scene keeps the current `SummonLaneSpace` implementation.

1. Player Zone
   - Player body can move here.
   - Player can dodge, aim, fire, and build EN.
   - Close threats may enter this zone as pressure.

2. Contested Line
   - Projectiles cross here.
   - Summons and enemy pressure bodies collide here.
   - The player cannot freely solve this by walking over the line.

3. Boss Zone
   - Boss body lives here.
   - Boss pressure originates here.
   - Summons and projectiles may affect this zone.

### Crossing Rules

Allowed to cross the contested line:

- player projectiles
- boss projectiles
- `SummonFrontlineProxy`
- enemy summon or pressure bodies
- VFX/telegraphs

Not allowed to cross the contested line:

- player body
- boss body

### EN Rule

Forward risk should still matter.

Player movement closer to the contested line should charge EN faster through `SummonEnergyLadder`.

This creates a clean decision:

- staying back is safer but slower
- moving forward charges faster but invites pressure
- spending EN sends a summon or uses `Skill1`
- correct summon use opens a follow-up window

Do not tune EN values in this pass. Confirm the structure:

- safer position charges slower
- forward-risk position charges faster
- correct summon action gives a visible state change
- follow-up window exists

## One-Pocket Match Flow

Target duration: 75-100 seconds.

The times below are review pacing hints, not balance targets. If implementation time is short, preserve the order and purpose of the beats before tuning exact seconds.

### Beat 0: Match Read

Target time: 0-8 seconds.

Purpose:

- establish the corridor standoff
- show that player and boss bodies stay on opposite sides
- show the contested line visually

Player-facing objective:

> Hold the line. Read the first pressure wave.

Implementation notes:

- Start with boss visible in the far lane.
- Show a subtle line/rail/horizon at the contested boundary.
- Avoid opening with a boss HP burn objective.
- Data shape: stage entry hook plus wave group intro, similar to NIKKE stage rows carrying enter scenario, theme, and wave references.

### Beat 1: Probe Wave

Target time: 8-24 seconds.

Purpose:

- teach local defense and EN gain
- make basic fire useful but not decisive

Pressure:

- light boss projectile pattern
- one close or mid probe threat on player side
- spawn/read family: `Normal` plus one `Dash` or `Drop` equivalent
- slot roles: `LineProjectile` and `CloseProbe`

Player action:

- strafe/dodge
- fire basic shots
- move toward forward-risk space to charge EN faster

Success state:

- close threat is cleared or pushed back
- EN reaches at least LV1

Player-facing objective:

> Build EN while stopping the probe wave.

### Beat 2: First Summon Need

Target time: 24-45 seconds.

Purpose:

- create a problem that direct player fire alone answers poorly
- make `SummonSlot1` visibly solve a contested-line problem

Pressure:

- boss creates a projectile curtain, screen, or enemy pressure body
- direct fire can chip, but cannot comfortably stabilize the line
- spawn/read family: `ProjectileCurve` or `Barrier` equivalent
- slot role: `ScreenCurtain`

Player action:

- spend `SummonSlot1`
- summon enters beyond the player boundary
- summon screen/body intercepts pressure

Success state:

- boss pressure is blocked or interrupted
- a short follow-up window opens

Player-facing objective:

> Send a summon across the line to break the pressure screen.

### Beat 3: Follow-Up Window

Target time: 45-55 seconds.

Purpose:

- prove summon use created an opening, not just damage

Player action:

- fire `Skill1` or sustained ranged fire into exposed boss/core

Success state:

- boss/core takes a clear chunk of damage
- EN pulse or visible resource reward confirms the correct answer

Player-facing objective:

> Confirm the opening with Skill1.

### Beat 4: Enemy Counter Wave

Target time: 55-80 seconds.

Purpose:

- show the enemy also contests the line
- make the match feel like wave pressure, not a single scripted prompt

Pressure:

- enemy summon/probe body
- side clamp or line pressure pattern
- spawn/read family: `Dash`, `Calling`, or `ObjectCreate` equivalent
- slot roles: `FrontlineBody` plus `LineProjectile`

Player action:

- decide between saving EN, spending LV1 immediately, or waiting for LV2/LV3
- use summon body/screen to hold the line
- dodge if overcommitted

Success state:

- second pressure is stabilized
- final window opens

Player-facing objective:

> Survive the counter wave and hold the frontline.

### Beat 5: Suppression Result

Target time: 80-100 seconds.

Purpose:

- end on stage success, not debug success

Result copy should not say only:

> BOSS CLEAR

Preferred result language:

> WAVE SUPPRESSED
> Frontline stabilized. Summon route analysis complete.

or

> RIFT PRESSURE BROKEN
> The summon screen held the line and opened a strike window.

## Objective Copy

Use short, readable objective lines. Avoid debug-style counters as primary copy.

Good primary objective examples:

- `Hold the line`
- `Build EN at the forward edge`
- `Send summon to block the curtain`
- `Confirm the opening with Skill1`
- `Suppress the counter wave`

Debug counters may still exist in review HUD, but they should not be the top-level player motivation.

Bad primary objective examples:

- `Goal: Mission clear 2/3`
- `Boss Answer window summon LV1`
- `Checks 3/3`
- `Damage boss 180/260`

These are useful for review, but weak as a stage fantasy.

## Result Copy

Use result language that confirms what changed in the match.

Clear examples:

- `WAVE SUPPRESSED`
- `FRONTLINE STABILIZED`
- `RIFT PRESSURE BROKEN`
- `SUMMON ROUTE SECURED`

Detail examples:

- `Summon screen blocked the boss curtain. Skill1 confirmed the opening.`
- `The frontline held through two pressure waves.`
- `Cross-line pressure route analyzed.`

Fail examples:

- `LINE COLLAPSED`
- `Pressure wave reached player zone.`
- `Summon screen missed the boss curtain.`

## Mechanics Mapping

### Existing Systems To Reuse

Use these instead of inventing parallel systems:

- `ActionFoundationBossBarrageLaneReview.unity`
- `SummonLaneSpace`
- `SummonEnergyLadder`
- `PlayerSummonSlot1Action`
- `SummonFrontlineProxy`
- `SummonPressureScreen`
- `BossBarrageEmitter`
- `BossPressureCostLadder`
- `BossPressureActionDirector`
- `BossBarragePocketReviewOwner`
- `BossBarrageLaneReviewHud`
- `BossBarrageLaneReviewMobileHud`
- `ActionScreenCuePresenter`
- `BossBarragePocketVfxCueBridge`
- `BossBarragePocketCameraCueBridge`

### Existing Data To Reuse

- `DB_BossPressureActionDeck_PocketReview.asset`
- `DB_SummonSlot1_ShieldBreaker.asset`
- `DB_SummonOpportunity_BossPressureBlock.asset`
- current boss barrage pattern profiles
- current support summon prototypes only if needed for comparison

### Data Interpretation

`DB_BossPressureActionDeck_PocketReview.asset` already contains a useful answer matrix:

- LV1 skill pressure asks for dodge/strafe, not summon
- LV1 escort probe can be answered by cheap summon or ranged fire
- LV2 summon-pressure exchange asks for `SummonSlot1` screen
- LV3 overextend punish asks for retreat or high-tier summon screen

The next implementation should surface this matrix as stage readability.

## Proposed Review Data Shape

Before implementing code, represent the next review pocket in this shape, even if it stays as comments, serialized fields, or a simple ScriptableObject later.

### Stage Shell

Fields:

- `stageId`: `AF_BarrageWave_001`
- `displayName`: `Rift Wave Suppression`
- `combatPromise`: player and boss bodies stay separated while waves, projectiles, and summons contest the line
- `entryCue`: hold the line and read the first wave
- `successResult`: wave suppressed / frontline stabilized
- `failResult`: line collapsed / pressure reached player side
- `mockRewardHook`: summon route analysis complete
- `waveGroupId`: `AF_WaveGroup_BarrageLine_001`

### Wave Group

Fields:

- `waveGroupId`
- `theme`
- `battleTimeBudget`
- `waves`

Do not tune the time budget deeply now. It only prevents the pocket from becoming an endless sandbox.

### Wave Row

Fields:

- `waveIndex`
- `wavePurpose`
- `primaryPressureSlot`
- `secondaryPressureSlot`
- `playerAnswer`
- `summonAnswer`
- `successOpens`

Example:

```text
waveIndex: 2
wavePurpose: First summon-needed pressure
primaryPressureSlot: ScreenCurtain
secondaryPressureSlot: LineProjectile
playerAnswer: dodge/read while charging EN
summonAnswer: SummonSlot1 pressure screen intercepts curtain
successOpens: Skill1 follow-up window
```

### Pressure Slot

Fields:

- `slotRole`: CloseProbe / LineProjectile / ScreenCurtain / FrontlineBody / BackPressure / CoreExpose
- `spawnRead`: Normal / Dash / Drop / Jump equivalent
- `fireRead`: Instant / ProjectileCurve / Range / Barrier / Calling equivalent
- `targetPriority`: player, summon, frontline, boss/core
- `answerType`: dodge, fire, summon screen, summon clash, Skill1 follow-up
- `resultCue`: what visibly changes when answered

This structure is more important than exact enemy HP or projectile damage in the next pass.

## NIKKE-Informed First Pocket

Use this as the concrete first implementation target.

### Wave 1: Probe And EN Read

- `slotRole`: `CloseProbe`
- `spawnRead`: `Dash` or `Drop`
- `fireRead`: `Instant`
- player answer: strafe/basic fire and move toward forward-risk space
- summon answer: not required
- result cue: EN ready read, wave pressure thins

### Wave 2: Screen Curtain

- `slotRole`: `ScreenCurtain`
- `spawnRead`: `Normal`
- `fireRead`: `ProjectileCurve` or `Barrier`
- player answer: survive/space while EN is ready
- summon answer: `SummonSlot1` screen intercepts or body-holds the curtain
- result cue: pressure break VFX and follow-up objective

### Wave 3: Frontline Counter

- `slotRole`: `FrontlineBody`
- `spawnRead`: `Dash` or `Calling`
- `fireRead`: `Range` or `ObjectCreate`
- player answer: do not overcommit; keep firing and dodge line pressure
- summon answer: summon body clashes or holds contested line
- result cue: core or boss-side pressure source exposes briefly

### Final Window: Core Expose

- `slotRole`: `CoreExpose`
- `spawnRead`: none
- `fireRead`: none
- player answer: `Skill1` or ranged confirm
- summon answer: already created the opening
- result cue: `WAVE SUPPRESSED` or `FRONTLINE STABILIZED`

## What Not To Do Because Balance Time Is Limited

Do not spend the next pass on:

- exact damage curves
- exact HP curves
- exact spawn counts
- perfect EN fill seconds
- large wave count tuning
- multiple summon balance comparison

Spend it on:

- wave role labels
- readable pressure slots
- objective/result language
- summon answer visibility
- follow-up state confirmation
- stage shell/result hook continuity

If a number is needed, use the existing current value and mark it as placeholder. The structural answer should survive later tuning.

## What Must Be True In The Next Review

### Player Understanding

A first-time reviewer should be able to say:

1. "The boss and I are locked on opposite sides."
2. "The danger comes in waves across the line."
3. "Moving forward charges EN faster but is dangerous."
4. "The summon crosses/holds the line better than I can."
5. "A correct summon creates the opening for Skill1 or fire."

### Combat Feel

The pocket should not clear from:

- only basic fire
- only dodging until timers expire
- pressing summon without the pressure problem being visible
- draining a boss HP bar without solving wave pressure

The pocket should clear from:

- surviving a wave
- using forward risk to earn EN
- sending summon into a visible pressure problem
- confirming the summon-created opening

### Readability

The contested line must be readable through at least two channels:

- floor/rail/horizon VFX
- HUD/objective text
- camera framing
- summon entry path
- projectile direction and collision

The summon solution must be readable through at least two channels:

- summon body crossing/holding the line
- projectile interception or clash
- pressure break VFX
- short camera cue
- follow-up objective text

## Anti-Drift Instructions For Codex

When implementing from this document:

1. Do not start by adding new characters.
2. Do not start by adding a new full stage manager.
3. Do not convert the scene into a pure boss HP fight.
4. Do not add permanent rewards or save data.
5. Do not use more UI text to hide an unclear combat exchange.
6. Do not add more buttons until `SummonSlot1` clearly changes the frontline state.
7. Prefer tuning and wiring existing systems over creating parallel systems.
8. Keep the first pass inspectable in `ActionFoundationBossBarrageLaneReview.unity`.

## Implementation Order

### Pass 1: Language And Result Lock

Update review-facing copy so the pocket reads as wave suppression/frontline stabilization.

Expected work:

- objective strings
- result banner title/detail
- HUD labels that currently say boss-clear/checks as primary language

Do not change core mechanics in this pass unless required by copy binding.

### Pass 2: 90-Second Wave Script

Author a simple three-beat pressure sequence in the existing boss-barrage lane review scene.

Required beats:

1. probe wave
2. summon-needed pressure screen
3. counter wave into final suppression

Use existing `BossPressureActionDirector` slots where possible.

### Pass 3: Summon Frontline Read

Make the summon action visually own the contested line.

Expected work:

- stronger summon entry path
- clearer pressure screen or clash read
- follow-up window VFX
- ensure direct basic fire alone does not feel like the intended answer

### Pass 4: Validation And Review

Run validation and, if possible, play the pocket manually or through PlayMode coverage.

Minimum checks:

- player body stays in player zone
- boss body stays in boss zone
- summon proxy crosses/holds contested space
- EN fills faster near forward risk
- `SummonSlot1` blocks or interrupts a visible pressure
- follow-up window opens
- result copy confirms wave/frontline outcome

## Acceptance Criteria

The next review pass is successful if all are true:

- The top-level objective is wave/frontline suppression, not boss HP burn.
- The player has at least one moment where direct fire is insufficient or risky.
- `SummonSlot1` visibly changes the contested-line state.
- A correct summon action opens a short follow-up window.
- The result screen explains what was stabilized, suppressed, or secured.
- A reviewer can describe the summon as frontline agency rather than a damage button.

The pass is not successful if the strongest reviewer takeaway is:

> "Wait for EN, press summon, shoot boss."

## Optional A/B Test

If direction remains uncertain, build three lightweight variants using the same scene:

1. Boss HP variant
   - objective is boss damage
   - expected risk: feels like a distant HP sponge

2. Wave suppression variant
   - objective is clearing pressure waves
   - expected benefit: makes the standoff rules meaningful

3. Summon frontline variant
   - objective requires summon block/clash before damage window
   - expected benefit: proves summon identity most directly

Use the same success question for all variants:

> Did the summon feel like a tool for controlling the frontline, or just another attack?

## Source Anchors

Local anchors from the current project investigation:

- `COMBAT_V1_SPEC.md`: warns not to hide a weak EN/summon loop with art, enemies, or buttons.
- `LINEAR_STAGE_DESIGN_FOUNDATION.md`: says old route names are stage-design promises only and the current pivot should focus on fixed-rear boss-barrage plus `SummonSlot1`.
- `DB_BossPressureActionDeck_PocketReview.asset`: already defines role/answer data for LV1, LV2, and LV3 boss pressure.
- `BossBarragePocketReviewOwner`: already requires local defense, summon pressure block, and confirmed follow-up hit for pocket clear.
- `SummonLaneSpace`: already separates clamped player space from battlefield coordinates used by summons.
- PGR tutorial/stage reference data: successful reference games tie stage nodes, guide steps, combat overlays, and rewards together instead of leaving combat prompts as isolated debug reads.
- NIKKE `stage-wave-join.csv`: campaign stage rows keep stage type/category, theme, scenario hooks, reward IDs, wave groups, monster slots, close/mid/far counts, and target lists together.
- NIKKE `stage-wave-monster-slots.csv`: encounter slots preserve wave index, slot index, path, spawn type, monster ID/model, stat group, AI, skill weapon type, and skill fire type.
- NIKKE `noncampaign-stage-wave-join.csv` and `nikke-noncampaign-stage-wave-rollup.csv`: event, tower, simulation, lost-sector, and raid modes reuse the same wave-group reading pattern instead of inventing unrelated encounter grammar.
- NIKKE LostSector runtime bridge: public runtime shape separates map loading, StageSpawner links, cleared-stage state, completed-scenario hooks, reward/item flow, and clear handling.

## 2026-06-26 Implementation Lock: Frontline Motivation Review

The first implementation pass is a separate review scene, not an overwrite of the existing boss-barrage lane review scene.

New authored targets:

- Scene: `Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity`
- Data shell: `Assets/_Game/DesignData/Profiles/ActionFoundation/DB_FrontlineWaveStage_MotivationReview.asset`
- Runtime data type: `FrontlineWaveStageProfile`
- Editor setup: `ActionFoundationFrontlineMotivationReviewSetup`

The profile uses the ArkData references as structural evidence only:

- NIKKE `stage-wave-join.csv`: 90-second stage shell, scenario/result hooks, reward ID, wave group, close/mid/far counts.
- NIKKE `stage-wave-monster-slots.csv`: spawn families (`Drop`, `Dash`, `Jump`, `Normal`), wave paths, monster/skill/fire slots.
- PGR tutorial runner contract: condition gate -> combat observer -> completion record -> reward/state hook.
- Combat payload family guide: action -> target selector -> projectile/hit event -> presentation feedback.

Local review beats are locked as:

1. Match read: player and boss bodies stay separated by the contested line.
2. Close probe: local defense stops the first close threat.
3. First summon need: `SummonSlot1` answers the boss curtain because the player cannot cross.
4. Follow-up window: `Skill1` confirms the route after summon pressure suppression.
5. Counter wave: missed follow-up returns boss pressure instead of silently failing.
6. Suppression result: result copy records `frontline stabilized` / `summon route analyzed`, not boss HP death.

Route stability is now part of the review-scene contract:

- The profile carries a route-stability budget derived from the same 90-second stage shell and wave-slot read.
- The profile also carries six structured pressure slots (`BackPressure`, `CloseProbe`, `ScreenCurtain`, `CoreExpose`, `FrontlineBody`, `RecordHook`) derived from NIKKE spawn family/path slot grammar and mapped to local review beats.
- CloseProbe, SummonNeed, and CounterWave beats drain route stability until the player answers the local objective.
- Close-threat defeat, `SummonSlot1` pressure block, and confirmed `Skill1` follow-up restore stability.
- If stability collapses, the pocket fails as `LINE COLLAPSED` even when player HP remains, so the match is not reducible to HP trading or button timing.
- The HUD and route record must show the active pressure slot, stability, route progress, and target time.
- The route record must also expose the observer-completion snapshot (`close`, `summon`, `followup` as `pending`/`recorded`) so the pocket follows the PGR-style condition gate -> combat observer -> completion record chain instead of ending as loose debug text.

The profile is review-only. It must not grant rewards, unlock progression, replace the stage-select flow, or become a general combat manager.
