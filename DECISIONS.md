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
