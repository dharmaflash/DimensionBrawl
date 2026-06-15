# Decisions

## 2026-06-12: Restart From Clean Baseline

Decision: Start `DimensionBrawl` as a clean Unity project instead of continuing to repair the old project.

Reason: The previous project had unstable AI-generated code and unclear direction. A small baseline is safer than further salvage work.

## 2026-06-12: Raw Asset Packs Are Local-Only

Decision: Store imported asset packs under `Assets/_Imported/` and ignore them in Git.

Reason: The packs are large and should not pollute the repository history. Only curated game-ready assets should be copied or authored under `Assets/_Game/`.

## 2026-06-12: Prefab/Scene Authoring Before Runtime Generation

Decision: Prefer authored prefabs, scene objects, ScriptableObjects, and Inspector configuration over runtime mass generation.

Reason: The project must stay inspectable in Unity and avoid giant AI-authored runtime builders.

## 2026-06-12: No Legacy/Fallback By Default

Decision: New systems should not start with legacy compatibility, broad fallback paths, or old-project restoration logic.

Reason: The project is a restart. Compatibility code is allowed only when a concrete current feature needs it and the removal condition is documented.

## 2026-06-12: Small Vertical Slice First

Decision: Build the first playable around one player, one basic enemy, one attack loop, and one defeat condition.

Reason: A small complete loop exposes real needs earlier than large speculative architecture.

## 2026-06-12: Direct-Control ARPG With Summon Slots

Decision: `DimensionBrawl` V1 should follow a direct-control ARPG shape: the player manually moves, attacks, dodges, and clears linear combat sections, while three summon slots are reserved as later combat resources.

Reason: The target references and current visual direction point to player-driven action first, not a backline commander, automatic attacker, hand-of-cards UI, or summon-first implementation.

## 2026-06-12: Player Action Before Summon Implementation

Decision: Implement the first playable action slice around player movement, short manual basic attacks, dodge, health/damage, and one basic sci-fi soldier before building actual summon behavior.

Reason: Summons need a readable combat baseline to support. Building them before the player action loop is playable would hide unresolved movement, attack, dodge, camera, and hit-feedback problems.

## 2026-06-12: Action Feel Is The First Quality Gate

Decision: The first implementation quality gate is not feature count. It is responsive movement, natural stop/settle feel, readable camera, short manual combo rhythm, trustworthy dodge, and visible hit feedback against one basic soldier.

Reason: The project restart exists to avoid unstable AI-generated system sprawl. Action-game value comes from feel first; larger systems should only be built after the basic action loop is reviewable.

## 2026-06-12: Basic Combo Can Grow With Curated Clips

Decision: The V1 basic attack chain is no longer capped at 2-3 hits. It may grow to 5-7 hits when the selected CombatGirl clips read naturally as a basic chain, but each hit still needs explicit timing, damage, cancel, and camera-cue values.

Reason: The imported CombatGirl pack includes more usable attack animation than the first placeholder scope assumed. A longer chain can improve ARPG feel, but only if it remains authored, inspectable, and animation-backed instead of becoming code-only attack spam.

## 2026-06-13: General And Elite Enemy Patterns Are Data-Driven

Decision: General and elite soldier behavior should grow through `CombatAiPatternProfile`, `CombatAiPatternDeck`, and `CombatAiElitePatternProfile` assets before adding new enemy-specific code. `RetreatShot`, `RetreatBlink`, `GuardBreak`, `ShieldCycle`, `ArmorBreak`, `AuraBuffer`, `SummonPackage`, and `PhaseSwap` are authored as reusable data/runtime foundations, not pattern-id branches.

Reason: The same AI grammar needs to serve enemies, future ally summons, and later bosses. Data-backed decks and narrow trait controllers keep patterns inspectable in Unity while avoiding runtime instantiation, scene-wide searches, and hardcoded behavior that would make later model/animation swaps painful.

## 2026-06-13: Enemy Animation Requests Are Pattern Data

Decision: Enemy prepare, attack, hit, death, and elite signal animation requests should live on `CombatAiPatternProfile` and `CombatAiElitePatternProfile` data, then route through a shared promoted Animator Controller. Runtime enemy scripts should not branch on pattern ids to pick clips.

Reason: Enemy, future summon, and later boss actors need to share the same AI grammar while swapping model/animation sets. Keeping animation triggers in profile data lets designers replace placeholder MaintenanceWorker reads with better clips without rewriting behavior code.

## 2026-06-13: Android Is The Mobile-First Build Baseline

Decision: Android is the default product/build baseline, while PC/Standalone remains a convenient development and debug target. Shared UI and scene-flow work should inherit the Android Build Profile, landscape orientation, safe-area assumptions, and common input action names before branching into feature work.

Reason: The target game is a mobile-first direct-control ARPG. Locking Android package/orientation/backend/architecture settings early reduces later merge churn and prevents UI work from accidentally hardening around PC window behavior.

## 2026-06-13: Combat VFX Use Promoted Cue Profiles

Decision: Raw VFX store packs stay local under `Assets/_Imported/AssetStore/VFX/`. Combat code should use `CombatVfxCueProfile` data and presentation drivers that play selected `_Game/Art/VFX` prefabs through bounded pools, not direct references to raw asset-pack paths or unbounded runtime spawning.

Reason: VFX must become part of action readability without polluting the repository or gameplay code. A cue-profile layer keeps player, enemy, future summon, and boss effects swappable while preserving the project rule that source packs are local-only. Ranged soldier shot motion can live as authored cue-local forward travel, but damage authority remains in pattern hit-shape data until a reviewed gameplay projectile system is explicitly needed.

## 2026-06-13: Enemy Variety Starts As Role Data

Decision: General/elite monster variety for the first linear ARPG run should start with `CombatEnemyRoleProfile` role assets that combine existing `CombatAiPatternDeck` and `CombatAiElitePatternProfile` data before adding new enemy controllers, encounter spawners, or prefab/model variants.

Reason: The collected run-design data points to roles such as entry probe, break gate, backline pressure, rescue pressure, boss handoff, and final stand. Capturing those as data keeps the future summon-AI reuse path open and lets designers review monster purpose before code or prefab sprawl.

## 2026-06-13: Enemy Roles And Presentation Candidates Stay Separate

Decision: `CombatEnemyRoleProfile` and pattern decks stay behavior/intent data, while `CombatEnemyArchetypeProfile` maps those roles to promoted or promotion-pending presentation candidates such as melee soldier, ranged soldier, elite soldier, FORGE3D turret, and future dragon boss. Role and archetype data must not directly reference raw `_Imported` asset-store prefabs.

Reason: Enemy, summon, and later boss content need to share a small combat grammar without forcing art choices into behavior data. Keeping role decks separate from presentation candidates lets the team review purpose first, then promote only selected models, turret parts, animations, and VFX into `_Game` when they are ready.

## 2026-06-14: Linear Stage Design Starts As Data Templates

Decision: The first linear stage structure should be authored as `LinearStageTemplateProfile`, `LinearStageSegmentProfile`, and pocket/role-slot data before adding runtime wave spawning, reward payout, stage-select UI, boss phases, or summon behavior.

Reason: The collected run and reward research already defines a first stage lesson sequence, but turning that directly into runtime code would recreate the project risk of hidden generators and over-large systems. Data templates let designers review the intended route, pressure rhythm, relief beats, and future summon needs while reusing the existing enemy role catalog.

Impact: Stage templates may reference promoted `_Game` segment and role profile assets only. Actual prefab placement, encounter runtime ownership, reward grants, and stage navigation remain separate reviewed slices.

## 2026-06-15: Stage Environment Assets Use Game-Ready Copies

Decision: Authored stage environments may promote selected Asset Store meshes, shaders, materials, prefabs, and texture copies into `_Game`, but they must not copy raw high-resolution source textures such as original `.tga` files into the repository or Git LFS by default. If a stage needs demo-scene-level presentation, preserve that quality through curated placement, shader/post-process setup, and game-ready texture copies with explicit import limits instead of committing source-pack texture originals.

Reason: The first Spring Isles dressing pass showed that raw environment texture sources can exceed GitHub LFS budget quickly without improving the actual authored scene contract. The project needs reviewable, cross-PC stage content, but the repository should contain selected game-ready assets rather than full source-pack payloads.

Impact: Raw environment packs stay under local `_Imported`. A promoted stage slice should avoid direct `_Imported` references, avoid raw source texture duplication, and document any future large-asset exception before it is committed. If the project later needs full-fidelity source art syncing across machines, use a deliberate asset-depot choice such as paid LFS capacity, Unity Version Control, Perforce, or a separate artifact pipeline instead of silently growing the gameplay repository.

## 2026-06-15: Art Source Depot Is Separate From Gameplay Git

Decision: Keep the gameplay Git repository focused on code, docs, authored scenes, gameplay data, and reviewed game-ready assets. Full raw packs, source textures, vendor demo scenes, and demo-scene-scale art sources should move to a deliberate art source depot such as Unity Version Control or Perforce before the project commits more full-fidelity stage art. Until that depot exists, raw art remains local-only under `Assets/_Imported/`.

Reason: The repository has already become heavy with binary assets, and the account has hit the Git LFS budget. Demo-scene-level stages still need coherent terrain, water, lighting, postprocess, particles, wind, and dense authored placement, but using the gameplay Git repository as the raw asset archive would make collaboration and pushing fragile.

Impact: Use `Assets/_Game/DesignDocs/ART_ASSET_STORAGE_WORKFLOW.md` and `Assets/_Game/DesignDocs/SPRING_ISLES_DEMO_ADAPTATION_PLAN.md` before the next Spring Isles stage art pass. Do not treat "promote only what is needed" as "promote a tiny sample"; promote coherent reviewed composition layers while keeping raw source packs out of Git.

## 2026-06-13: First Enemy Prefab Candidate Is Authored And Scene-Free

Decision: The first reusable sci-fi melee soldier should be promoted as an authored `_Game/Prefabs/Enemies/ActionFoundation` prefab candidate before adding more enemy variants, waves, or spawners. The prefab may carry local health, AI, target sensor, Animator, telegraph, VFX, and cue-driver references, but it must not serialize scene target candidates or a scene camera controller.

Reason: Enemy/summon reuse needs a prefab-level baseline that can be reviewed in Unity. Keeping player targets, camera controllers, and encounter membership outside the prefab prevents a scene sample from quietly becoming a hidden global dependency.

## 2026-06-12: Cinemachine In-Game Cutscene Reference Baseline

Decision: Use `Assets/_Game/DesignDocs/CINEMACHINE_INGAME_CUTSCENE_REFERENCE_RESEARCH.md` and `Assets/_Game/DesignData/CinemachineIngameCutsceneReferenceDataset.json` as the active Unity Cinemachine/Timeline reference baseline for boss intros, summon/ultimate cut-ins, dialogue staging, camera shot sequencing, input/AI/time locks, impulse, Timeline signals, and gameplay camera return.

Reason: The project needs a Unity-native production path for short character cutscenes and combat cut-ins that can sit on top of the existing ARPG camera and cue system without turning combat into uncontrolled scripted sequences.

Rejected alternatives: `hand-author cutscene camera scripts per event`, `drive all cut-ins only through ad hoc BattleCamera offsets`, `block combat with long unskippable cinematics`, `ignore gameplay return state`, `use Timeline without explicit bindings and lock contracts`, `claim reference games provide public Cinemachine source assets`.

Impact: `CutsceneCueProfile`, `CinemachineShotProfile`, `CameraModifierStackProfile`, `CameraTrackProfile`, `StoryCameraBindingProfile`, `TimelineBindingProfile`, `GameplayLockProfile`, `CameraReturnProfile`, `SignalEventProfile`, `ImpulseCueProfile`, boss intro/summon/ultimate/final-kill cut-in authoring.

Evidence handling: Reference games did not yield public Cinemachine/Timeline source assets. ZZZ/PGR/HI3 public data mirrors are used only for field-shape and production-contract evidence such as camera modifiers, camera track LUTs, story/camera site tables, time-slow/screen-effect stacks, and cleanup/end-action patterns.

Legacy handling: Existing `BattleCamera` cue types and optional Cinemachine impulse hooks remain useful seeds. New Cinemachine/Timeline work should wrap or bridge those hooks, not silently replace V2 combat authority or reintroduce lane/manual-target assumptions.

## 2026-06-15: Fixed-Rear Boss-Barrage And Summon-First Pivot

Decision: Pivot the V1 combat target from melee direct-control ARPG first to fixed-rear boss-barrage + summon-first lane combat. The player controls movement, dodge, lane position, forward-risk summon-energy gain, and close-threat local defense, while `SummonSlot1` provides the main battlefield swing against the far boss/proxy pressure.

Reason: Direction review found the current melee/action-route focus too close to a generic RPG/action game. The latest direction clarifies the intended core tension: the far boss keeps firing projectiles, the player cannot cross the midline/forward boundary, forward position charges summon energy faster but is riskier, and summons are the main exchange tool.

Impact: `COMBAT_V1_SPEC.md`, `ACTION_FEEL_TARGETS.md`, `ACTION_FOUNDATION_OWNERSHIP.md`, and `LINEAR_STAGE_DESIGN_FOUNDATION.md` now supersede the earlier "player melee action before summon implementation" direction. Existing CombatGirl movement, camera, enemy, VFX, and stage work is preserved as reusable foundation/checkpoint work, but it should not keep expanding as the product center until the boss-barrage lane + summon-first loop is playable.

## 2026-06-15: Local-Defense Attack Before Rifle Animation Dependency

Decision: Validate the player-side defense slice with one simple authored local-defense attack before installing or depending on a rifle animation pack. The expression can be a short slash, short magic projectile, or gun-like fire.

Reason: The immediate product question is whether boss-barrage risk positioning plus a meaningful summon call works. A rifle locomotion/weapon animation dependency can add import and tuning cost before the core loop is proven.

Impact: The next implementation should define `BasicDefenseAttack`, one hit/projectile cue with clear damage authority, and one `SummonSlot1` action. Rifle/gun assets remain optional later candidates if the local-defense loop needs them.

## 2026-06-15: Fixed-Rear Boss Barrage With Forward-Risk Summon Energy

Decision: The next V1 combat target is a fixed rear camera lane where a far boss or boss proxy continuously fires projectile patterns. The player can move forward and backward on the player side, but can never cross the authored midline/forward boundary. Moving closer to that boundary charges summon energy faster, while staying farther back is safer because projectile spacing/risk is looser.

Reason: The latest direction clarifies that the game is not a free-chase ARPG. Its core tension is boss projectile pressure, risk-positioning, summon-energy gain, and summon-vs-boss exchange.

Impact: The next implementation should define lane bounds, an uncrossable midline/forward boundary, back safety zone, summon-energy gain by forward position, and one readable boss/proxy projectile pattern before full boss phases or chapter art. Camera should stay fixed rear for the first slice.

## 2026-06-15: Player Basic Attack Is Local Defense, Not Boss Main DPS

Decision: The player may directly attack monsters that approach the player side. This action is named `BasicDefenseAttack` in planning docs and may be expressed as a short slash, short magic projectile, or gun-like fire after asset/readability review.

Reason: Close monsters need a local answer, but the player's basic attack should not become the main boss damage route or revive the melee-combo-first direction.

Impact: `BasicRangedAttack` is superseded as a planning term by `BasicDefenseAttack`. Keep player attack ownership narrow and tune it for close/approaching threats. Summon action remains the main battlefield swing.

## 2026-06-15: EN Tier Ladder Drives Skill And Summon Strategy

Decision: Use a shared `EN LV1 -> EN LV2 -> EN LV3` ladder for the first active skill and first summon slot. When a level fills, the corresponding skill/summon tier becomes available. The player may spend early or keep charging toward a stronger tier. Spending skill or summon energy resets the ladder to empty `EN LV1` charging.

Reason: The desired strategy is not only "fill gauge, press summon." The player should choose between using a lower-level answer immediately or taking forward-position risk long enough to unlock a stronger version of the same skill/summon.

Impact: Implement only LV1-LV3 first. `Skill1` fires immediately at the available tier. `SummonSlot1` should summon the same concept at LV1/LV2/LV3, with stronger presentation/effect by tier, appearing from a magic circle in front of the player before entering the frontline exchange. Do not expand this into a roster, rarity, inventory, or upgrade economy until the tiered combat loop is accepted.

## 2026-06-15: Summon Frontline Coordinates Are Not Player Movement Coordinates

Decision: Player movement must remain clamped to the player-side lane and forward boundary, but summon/frontline actions may cross that boundary and the lateral player rails when authored role or tier data requires it. Player actions use player-zone safety constraints; summon actions use battlefield coordinates.

Reason: The current boss-barrage/summon-first direction depends on the player being unable to cross the midline while summons still enter and act in the contested space. If summons inherit the player clamp, they cannot create the intended front-line exchange.

Impact: `PlayerSummonSlot1Action` uses `SummonLaneSpace.GetBattlefieldWorldPoint` for entry and assist shots. Tests must continue to cover player clamp versus summon crossing behavior before expanding summon actors, summon AI, or boss/frontline exchange systems.

## 2026-06-15: Boss Skill And Summon-Like Pressure Are Later Pressure Modules

Decision: Enemy bosses may eventually use skill or summon-like pressure, but V1 should treat this as future boss pressure-module design, not as a symmetric full summon system.

Reason: The first loop needs the player EN ladder, projectile read, and `SummonSlot1` exchange to work before the boss gains comparable complexity.

Impact: The first boss/proxy may fire projectiles and expose simple pressure windows. Boss skills, adds, or summon-like calls should be authored later through explicit pattern/module data, not hidden in the first EN/summon implementation.
