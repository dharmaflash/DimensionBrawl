# Frontline Combat Structure Anchor

Last updated: 2026-06-26 KST

This document is the checkpoint for the Frontline combat pass. Read and update it before any major edit, batch run series, or milestone commit.

## Current Canonical Context

- Manual review lane: `ActionFoundationBossBarrageLaneReview.unity` remains the authored fixed-rear boss-barrage lane review scene from `CURRENT_STATE.md` and `COMBAT_V1_SPEC.md`.
- Policy evidence loop: `ActionFoundationFrontlineCombatPolicyReportTests` writes policy comparison reports to `C:\tmp\DimensionBrawl-FrontlineCombatPolicyReport.md` and `.json`.
- Motivation review pocket: the Frontline motivation scene/profile proves the clean route can exist, but route existence is not enough. The player must feel why summon/frontline structure matters.
- North star: a small corridor standoff match where player/boss bodies do not cross the contested line, while projectiles, pressure actors, and summons create pressure across it.

## Priority Premise

This pass is not a concrete balance-only pass.

The target is structural combat feel proven through repeated batch simulations. Each change must answer an ArkData-grounded missing piece, then show measurable separation between poor policies and the intended route.

## ArkData Comparison Lens

- NIKKE-like stage data: stage -> wave group -> pressure slots -> spawn/path/range counts -> scenario/result/reward hooks. Use this as data discipline, not copied numbers.
- CombatPayload-like grammar: Trigger -> Target -> Effect -> Status/Hit -> Presentation. Avoid bool-only proof when a combat event should explain who did what to whom and what changed.
- PGR-like combat state grammar: QTE/state lock-unlock, buff-gated punish windows, hit response tiers, dodge/matrix-style timing, and presentation bridges. Use the grammar, not tutorial UI.

## Active Structural Axes

1. NoSummon and GunOnly should lose meaningful HP, windows, or state because they ignore frontline pressure.
2. Summon block -> counter window -> Skill1 punish should read as one combat-state loop with pressure, answer, and reward.
3. Hit reactions should communicate damage/status without repeated full-body interruption, except true break, lock, death, or major punish cases.
4. Enemy summons and pressure actors should create unattended tactical cost, not instant death and not timer-only bookkeeping.

## Prohibitions

- Do not switch canon to a new scene or generated stage unless the user explicitly asks.
- Do not add tutorial HUD/proxy overlays as the solution to missing combat grammar.
- Do not restore removed VFX/audio banks or broad presentation passes unless the active hypothesis explicitly requires a narrow reviewed cue.
- Do not build a full summon roster, boss phase manager, reward economy, stage select, or production chapter flow.
- Do not treat one passing clean-route test as proof of fun.
- Do not spend the pass on detailed damage curves, enemy roster growth, or cleanup unrelated to the combat structure gate.

## Simulation Gate

Every attempt should compare at least these policies when practical:

- NoInput or NoSummon
- GunOnly
- IntendedRoute
- LateSummon

Track enough metrics to prove route/pressure separation:

- player HP lost
- boss damage
- clean route result and route stability
- summon blocks or frontline stabilizations
- unanswered pressure hits
- counter window opens/uses/misses
- Skill1 punish confirmations
- enemy pressure actor reach/attack/control cost
- hit-lock or interruption counts, separated by minor hit versus break/major hit

## Before Major Edit Checklist

- What ArkData missing piece is this change introducing?
- Which of the four structural axes does it serve?
- Which metric should improve or separate after simulation?
- Which current canonical scene/profile/test owns the evidence?
- Which prohibition could this accidentally violate?
- Is this narrow enough for the code style and ownership docs?

## Decision Log

- 2026-06-26: Anchor created after reading the prior thread failures, project docs, and recent commits. The main correction is to preserve the big premise: ArkData-grounded structure and simulation-proven combat feel, not isolated balance tuning or side-system work.
- 2026-06-26: Expanded `ActionFoundationFrontlineCombatPolicyReportTests` from four policies to six. Added `MissedFollowupCounterRecovery` to prove the missed follow-up -> counter wave -> ally hold -> final Skill1 loop, and `BossScreenBlockedFollowup` to prove enemy boss-screen pressure can block Skill1 and leave the route in a recovered-but-unfinished state.
- 2026-06-26 batch evidence: NoSummonNoFire HP lost 57.0 / stability 0%; GunOnly HP lost 39.0 / boss damage 70 / stability 0%; IntendedRoute HP lost 0 / boss damage 208 / stability 71%; LateSummon HP lost 3.6 / boss damage 208 / stability 63%; MissedFollowupCounterRecovery clears as `CounterRecoveryClear` with `followup_miss`, final window opened, Skill1 hits 3, boss damage 384; BossScreenBlockedFollowup records `boss_screen`, blocks 2 Skill1 projectiles, records boss-screen follow-up block true, opens final window, but remains `Running` with 0 boss damage.
- 2026-06-26 resolved gap: BossScreenBlockedFollowup now reports counter source `boss_screen` when boss-screen pressure blocks Skill1 projectiles. This is the first narrow CombatPayload grammar correction in the pass: the Trigger source is separated before any balance tuning.
- 2026-06-26 resolved authored-scene gap: `PocketReadsBossSummonBlockAsFollowupFailure` now treats boss-screen Skill1 interception as a real `boss_screen` counter trigger, keeps the blocked follow-up readable for camera/VFX/objective cues, and no longer collapses into an instant recovery window.
- 2026-06-26 structural decision: counter-wave ally hold must come from a fresh SummonSlot1 response after the counter trigger, not from a stale summon already present before the miss/block. This preserves the PGR-like lock -> answer -> unlock grammar and keeps boss-screen blocks in `answer_counter` until the player rebuilds the summon answer.
- 2026-06-26 batch evidence after fresh-answer gate: NoSummonNoFire HP lost 28.0 / stability 0%; GunOnly HP lost 18.8 / boss damage 70 / stability 0%; IntendedRoute HP lost 0 / boss damage 208 / stability 71%; LateSummon HP lost 8.0 / boss damage 208 / stability 63%; MissedFollowupCounterRecovery clears as `CounterRecoveryClear` with `followup_miss`, fresh summon count 2, final window opened, boss damage 208; BossScreenBlockedFollowup records `boss_screen`, blocks 2 Skill1 projectiles, stays `Running` with `answer_counter` pending and 0 boss damage until a fresh counter answer is supplied.
- 2026-06-26 batch evidence extension: Added `BossScreenBlockCounterRecovery` to prove the boss-screen block branch is not a dead end. Latest report separates `BossScreenBlockedFollowup` as `Running` / `boss_screen` / `answer_counter` pending from `BossScreenBlockCounterRecovery` as `CounterRecoveryClear` / `boss_screen` / final window opened / boss damage 208 after a fresh SummonSlot1 answer. This closes the Trigger -> Status lock -> fresh Answer -> final Skill1 punish loop for both follow-up miss and boss-screen block sources.
