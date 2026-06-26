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
- 2026-06-26 measurement correction: Added `Frontline Clash Cost` metrics to the policy report and added `BossScreenIgnoredNoRecovery` as an explicit no-answer counter policy. The first run exposed an axis-4 gap: enemy frontline presence and summon-vs-summon clashes were measurable, but ignored boss-screen pressure still produced 0 body hits / 0 player damage because stale ally summons physically held pressure even after the route required a fresh answer.
- 2026-06-26 structural correction: Changed `DB_SummonSlot1_ShieldBreaker` actor lifetimes from infinite to timed frontline holds (LV1 4.4s / LV2 5.2s / LV3 6.2s), and changed `DB_BossSummonPressure_SummonCaller` from slow walk pressure into readable BodyRush pressure (move speeds LV1 2.35 / LV2 2.15 / LV3 1.95 with larger engage radii). ArkData reason: enemy pressure was previously `Calling/ObjectCreate` presence without enough `Dash/BodyRush` consequence. PGR reason: stale summon bodies must not undermine the lock -> fresh answer -> unlock rule.
- 2026-06-26 batch evidence after BodyRush correction: NoSummonNoFire now records HP lost 69.1, enemy body hits 9, enemy clash damage 12.1; GunOnly remains `Running` with boss damage 70 and HP lost 30.6; IntendedRoute remains `CleanFollowupClear` with HP lost 0, boss damage 208, and no enemy body hits; BossScreenIgnoredNoRecovery now records HP lost 10.8, enemy clashes 18, body hits 8, body damage 10.8; BossScreenBlockCounterRecovery still clears with boss damage 104.0, 1 Skill1 hit, and only 1 body hit / 1.3 HP lost before fresh summon recovery. This is the first verified separation where ignoring enemy pressure actors creates a physical tactical cost, while the recovery route converts that pressure into summon clashes and a final Skill1 punish.
- 2026-06-26 active hypothesis: Axis 2 is still thin. `BossScreenBlockCounterRecovery` proves lock -> fresh answer -> final window, but the payoff is only 104 boss damage / 1 Skill1 hit because a successful counter answer can remain in the Critical band and compress the final punish to 0.65x. ArkData/CombatPayload gap: Trigger and Status are present, but Effect/Reward is weak. PGR gap: an earned unlock should escape Critical enough to read as a punish window. Next narrow attempt: raise the counter-wave stabilize route bonus only if simulation shows recovery payoff improves without making ignored pressure safe.
- 2026-06-26 batch evidence after counter-unlock payoff change: raised `counterWaveStabilizeRouteBonus01` from 0.14 to 0.22 so a fresh counter answer can escape Critical compression. Focused policy report with new assertions passed: `BossScreenBlockCounterRecovery` now opens the final window at x0.85, lands at least 2 Skill1 hits, and deals clean-followup-or-better boss damage (latest 384.0, repeated run 208.0) while `BossScreenIgnoredNoRecovery` stays `Running` with 9 body hits / 12.1 HP lost and 0 boss damage. `NoSummonNoFire` and `GunOnly` still end at 0% route stability, so the route separation remains intact.
- 2026-06-26 focused authored-scene evidence: `FrontlineMissedFollowupRecordsCounterWaveBeforeRecovery` now treats the fresh counter answer as a real recovery payoff, allowing route stability to recover past the pre-answer pressure state, locking the screen cue at/above the unstable window threshold, and recording `Record A: Counter recovery` at `pressure 62%`.
- 2026-06-26 active hypothesis: Axis 3 should be verified at the policy-report layer before adding more presentation. CombatPayload's useful gap is `Status/Hit -> Presentation`: the current runtime already tags summon-vs-summon and player-body pressure as `FlashOnly` / no control lock, while major damage can still use interrupt or hard-lock. PGR reference data also keeps hit effect ids and lock/state candidates separate. Next narrow attempt: extend the Frontline policy report with summon damage-flash, full-body hit-trigger, suppression, and non-locking-vs-locking cue counts so minor frontline clashes prove they are readable without repeated full-body interruption.
- 2026-06-26 batch evidence after hit-reaction reporting: extended `ActionFoundationFrontlineCombatPolicyReportTests` with `Hit Reaction Presentation` metrics. Latest policy report passed with `BossScreenBlockCounterRecovery` producing 34 summon damage flashes, 0 full-body hit reactions, and 34 non-locking damage cues; `BossScreenIgnoredNoRecovery` produced 21 summon damage flashes and 0 full-body hit reactions while still costing 9 body hits / 12.1 HP lost. Axis 3 is now measured in the same policy loop as axis 4 instead of relying on a separate presenter unit test.
- 2026-06-26 active hypothesis: Axis 3 still needs the positive contrast: minor pressure damage is non-locking, but true punish hits should still register as control-lock / full-body-eligible damage events. CombatPayload reason: `ProjectileOrHitEvent` should carry damage response policy before presentation; PGR reason: hit effects and lock/state candidates are separate rows, so "no repeated full-body reaction" must not erase major hit reads. Next narrow attempt: add damage-response policy counts for player, boss, and close threat to the same policy report, proving gun/basic pressure stays non-locking while Skill1/final punish hits register as locking.
- 2026-06-26 batch evidence after damage-response reporting: added `Damage Response Policy` counts to the policy report. Latest run passed with no-action player pressure at `15/0/0` non-lock/lock/full-body-eligible, gun-only boss chip at `5/0/0`, intended Skill1 boss hits at `0/2/2`, and boss-screen recovery boss hits at `0/2/2`. This gives Axis 3 both sides of the contract: routine pressure and summon clashes stay non-locking, while true Skill1 punish hits remain locking/full-body-eligible.
