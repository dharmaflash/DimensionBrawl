using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationFrontlineCombatPolicyReportTests
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity";
        private const string PocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string ReportPath = "C:/tmp/DimensionBrawl-FrontlineCombatPolicyReport.md";
        private const string JsonPath = "C:/tmp/DimensionBrawl-FrontlineCombatPolicyReport.json";
        private const float PressureWindowSeconds = 3f;
        private const float ReliefPressureWindowPeakRatio = 0.35f;
        private const float DelayedPunishInputSeconds = 1.1f;
        private const float BacklineEnergyProbeForwardRisk01 = 0.12f;
        private const float ForwardEnergyProbeForwardRisk01 = 0.88f;
        private const float EnergyProbeMaxSeconds = 13f;
        private const float BarrageShapeProbeNearRadius = 1.25f;
        private const int BarrageShapePreviewCapacity = 16;
        private const float PhysicalBarrageProbeFlightSeconds = 3.4f;
        private const float PhysicalSkill1ProbeFlightSeconds = 2.2f;
        private const float SurvivalLimitProbeMaxSeconds = 45f;

        private enum PolicyKind
        {
            NoSummonNoFire,
            GunOnly,
            NoSummonSurvivalLimit,
            GunOnlySurvivalLimit,
            BacklineEnergyProbe,
            ForwardRiskEnergyProbe,
            BacklineBarrageProbe,
            ForwardRiskBarrageProbe,
            BacklinePhysicalBarrageProbe,
            ForwardRiskPhysicalBarrageProbe,
            ForwardRiskPhysicalSummonBlockProbe,
            ForwardRiskPhysicalSummonPunishProbe,
            IntendedRoute,
            IntendedDelayedFollowup,
            LateSummon,
            MissedFollowupCounterRecovery,
            BossScreenBlockedFollowup,
            BossScreenIgnoredNoRecovery,
            BossScreenBlockCounterRecovery,
            BossScreenDelayedCounterRecovery
        }

        [UnityTest]
        public IEnumerator WritesFrontlineCombatPolicyReport()
        {
            float previousTimeScale = Time.timeScale;
            Time.timeScale = 8f;
            List<PolicyMetrics> results = new List<PolicyMetrics>();

            try
            {
                foreach (PolicyKind policy in new[]
                {
                    PolicyKind.NoSummonNoFire,
                    PolicyKind.GunOnly,
                    PolicyKind.NoSummonSurvivalLimit,
                    PolicyKind.GunOnlySurvivalLimit,
                    PolicyKind.BacklineEnergyProbe,
                    PolicyKind.ForwardRiskEnergyProbe,
                    PolicyKind.BacklineBarrageProbe,
                    PolicyKind.ForwardRiskBarrageProbe,
                    PolicyKind.BacklinePhysicalBarrageProbe,
                    PolicyKind.ForwardRiskPhysicalBarrageProbe,
                    PolicyKind.ForwardRiskPhysicalSummonBlockProbe,
                    PolicyKind.ForwardRiskPhysicalSummonPunishProbe,
                    PolicyKind.IntendedRoute,
                    PolicyKind.IntendedDelayedFollowup,
                    PolicyKind.LateSummon,
                    PolicyKind.MissedFollowupCounterRecovery,
                    PolicyKind.BossScreenBlockedFollowup,
                    PolicyKind.BossScreenIgnoredNoRecovery,
                    PolicyKind.BossScreenBlockCounterRecovery,
                    PolicyKind.BossScreenDelayedCounterRecovery
                })
                {
                    EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                    yield return null;

                    CombatPolicyContext context = BuildContext(policy);
                    yield return RunPolicy(context);
                    context.Complete();
                    results.Add(context.Metrics);
                }

                WriteReports(results);

                PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
                PolicyMetrics delayedIntended = RequireResult(results, PolicyKind.IntendedDelayedFollowup);
                PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
                PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
                PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
                PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
                PolicyMetrics backlineEnergy = RequireResult(results, PolicyKind.BacklineEnergyProbe);
                PolicyMetrics forwardRiskEnergy = RequireResult(results, PolicyKind.ForwardRiskEnergyProbe);
                PolicyMetrics backlineBarrage = RequireResult(results, PolicyKind.BacklineBarrageProbe);
                PolicyMetrics forwardRiskBarrage = RequireResult(results, PolicyKind.ForwardRiskBarrageProbe);
                PolicyMetrics backlinePhysicalBarrage = RequireResult(
                    results,
                    PolicyKind.BacklinePhysicalBarrageProbe);
                PolicyMetrics forwardRiskPhysicalBarrage = RequireResult(
                    results,
                    PolicyKind.ForwardRiskPhysicalBarrageProbe);
                PolicyMetrics forwardRiskPhysicalSummonBlock = RequireResult(
                    results,
                    PolicyKind.ForwardRiskPhysicalSummonBlockProbe);
                PolicyMetrics forwardRiskPhysicalSummonPunish = RequireResult(
                    results,
                    PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
                PolicyMetrics counterRecovery = RequireResult(results, PolicyKind.MissedFollowupCounterRecovery);
                PolicyMetrics blockedFollowup = RequireResult(results, PolicyKind.BossScreenBlockedFollowup);
                PolicyMetrics ignoredRecovery = RequireResult(results, PolicyKind.BossScreenIgnoredNoRecovery);
                PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
                PolicyMetrics delayedBlockedRecovery = RequireResult(results, PolicyKind.BossScreenDelayedCounterRecovery);
                Assert.IsTrue(File.Exists(ReportPath), "Frontline combat policy report should be written.");
                Assert.IsTrue(File.Exists(JsonPath), "Frontline combat policy JSON should be written.");
                string markdown = File.ReadAllText(ReportPath);
                Assert.IsTrue(
                    markdown.Contains("## ArkData Coverage Summary"),
                    "The report should keep the ArkData comparison premise visible before detailed metrics.");
                Assert.IsTrue(
                    markdown.Contains("CombatPayload runtime pipeline"),
                    "The report should map policy metrics back to the Trigger -> Target -> Effect -> Status/Presentation contract.");
                Assert.IsTrue(
                    markdown.Contains("NIKKE stage-result runtime"),
                    "The report should keep stage result/reward boundaries explicit instead of drifting into balance-only tuning.");
                Assert.Greater(intended.SummonBlocks, 0, "The intended route must prove summon interception changes the run.");
                Assert.AreEqual(
                    "PlayerDownFail",
                    noSummonSurvival.ResultKind,
                    "Long no-summon survival should eventually end through the canonical HP fail state.");
                Assert.GreaterOrEqual(
                    noSummonSurvival.FirstPlayerDownAtSeconds,
                    0f,
                    "Long no-summon survival should report the player-down time.");
                Assert.LessOrEqual(
                    noSummonSurvival.FirstPlayerDownAtSeconds,
                    SurvivalLimitProbeMaxSeconds,
                    "Long no-summon survival should fail before the survival probe cap.");
                Assert.AreEqual(
                    "PlayerDownFail",
                    gunOnlySurvival.ResultKind,
                    "Long gun-only survival should fail before basic shots can replace the summon route.");
                Assert.GreaterOrEqual(
                    gunOnlySurvival.FirstPlayerDownAtSeconds,
                    0f,
                    "Long gun-only survival should report the player-down time.");
                Assert.Less(
                    gunOnlySurvival.FirstBossDownAtSeconds,
                    0f,
                    "Gun-only pressure should not defeat the boss before the player fails.");
                Assert.Less(
                    gunOnlySurvival.FirstPlayerDownAtSeconds,
                    SurvivalLimitProbeMaxSeconds,
                    "Long gun-only survival should fail before the survival probe cap.");
                Assert.GreaterOrEqual(
                    intended.BlockToFollowupWindowSeconds,
                    0f,
                    "The report should expose the summon block -> follow-up window cadence.");
                Assert.LessOrEqual(
                    intended.BlockToFollowupWindowSeconds,
                    0.35f,
                    "A clean summon block should unlock the follow-up window quickly enough to read as one combat beat.");
                Assert.GreaterOrEqual(
                    intended.FollowupWindowToHitSeconds,
                    0f,
                    "The report should expose the follow-up window -> Skill1 punish cadence.");
                Assert.Greater(
                    noSummon.PlayerDamageTaken,
                    intended.PlayerDamageTaken,
                    "The report should distinguish unanswered boss pressure from the intended summon answer.");
                Assert.Greater(
                    noSummon.PressureBurdenSeconds,
                    0f,
                    "The report should record ArkData-style pressure burden before judging route feel.");
                Assert.GreaterOrEqual(
                    backlineEnergy.EnergyTier1ReadyAtSeconds,
                    0f,
                    "The backline energy probe should prove LV1 still eventually becomes available.");
                Assert.GreaterOrEqual(
                    forwardRiskEnergy.EnergyTier1ReadyAtSeconds,
                    0f,
                    "The forward-risk energy probe should prove LV1 becomes available.");
                Assert.Less(
                    forwardRiskEnergy.EnergyTier1DurationSeconds,
                    backlineEnergy.EnergyTier1DurationSeconds * 0.6f,
                    "Forward-risk positioning should materially accelerate LV1 readiness versus backline safety.");
                Assert.Greater(
                    forwardRiskEnergy.AverageEnergyGainMultiplier,
                    backlineEnergy.AverageEnergyGainMultiplier + 0.75f,
                    "The energy probe should expose a clear gain-multiplier split between safety and forward risk.");
                Assert.Greater(
                    backlineEnergy.BackSafetyBandSeconds,
                    1f,
                    "The backline probe should spend measurable time in the BackSafety band.");
                Assert.Greater(
                    forwardRiskEnergy.ForwardRiskBandSeconds,
                    1f,
                    "The forward probe should spend measurable time in the ForwardRisk band.");
                Assert.Greater(
                    backlineBarrage.BarrageShapeProjectileCount,
                    0,
                    "The backline barrage probe should read the current boss barrage target preview.");
                Assert.AreEqual(
                    backlineBarrage.BarrageShapeProjectileCount,
                    forwardRiskBarrage.BarrageShapeProjectileCount,
                    "Backline and forward barrage probes should compare the same authored wave count.");
                Assert.Greater(
                    forwardRiskBarrage.BarrageShapeNearProjectileCount,
                    backlineBarrage.BarrageShapeNearProjectileCount,
                    "Forward-risk barrage pressure should create more near-body projectile pressure than backline safety.");
                Assert.Less(
                    forwardRiskBarrage.BarrageShapeAverageLateralGap,
                    backlineBarrage.BarrageShapeAverageLateralGap,
                    "Forward-risk barrage pressure should reduce the average lateral gap versus backline safety.");
                Assert.Less(
                    forwardRiskBarrage.BarrageShapeNearestLaneDistance,
                    backlineBarrage.BarrageShapeNearestLaneDistance,
                    "Forward-risk barrage pressure should move the closest lane pressure nearer to the player.");
                Assert.Greater(
                    forwardRiskBarrage.BarrageShapeThreatDensity,
                    backlineBarrage.BarrageShapeThreatDensity,
                    "Forward-risk barrage pressure should increase spatial threat density.");
                Assert.Greater(
                    backlinePhysicalBarrage.PhysicalBarrageProjectilesSpawned,
                    0,
                    "The backline physical barrage probe should spawn real boss projectiles.");
                Assert.AreEqual(
                    backlinePhysicalBarrage.PhysicalBarrageProjectilesSpawned,
                    forwardRiskPhysicalBarrage.PhysicalBarrageProjectilesSpawned,
                    "Backline and forward physical barrage probes should compare the same authored wave count.");
                Assert.Greater(
                    forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits,
                    backlinePhysicalBarrage.PhysicalBarragePlayerHits,
                    "Forward-risk barrage pressure should translate preview compression into real projectile hit pressure.");
                Assert.Greater(
                    forwardRiskPhysicalBarrage.PhysicalBarragePlayerDamage,
                    backlinePhysicalBarrage.PhysicalBarragePlayerDamage,
                    "Forward-risk barrage pressure should carry a larger physical HP cost than backline safety.");
                Assert.Greater(
                    forwardRiskPhysicalSummonBlock.SummonBlocks,
                    0,
                    "A physical summon-block probe should intercept real incoming boss projectiles.");
                Assert.Less(
                    forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerHits,
                    forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits,
                    "A physical summon answer should reduce the forward-risk projectile hit count.");
                Assert.Less(
                    forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerDamage,
                    forwardRiskPhysicalBarrage.PhysicalBarragePlayerDamage,
                    "A physical summon answer should reduce the forward-risk HP cost.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonBlock.FirstFollowupWindowAtSeconds,
                    0f,
                    "A physical summon block should open the follow-up window through the runtime intercept path.");
                Assert.LessOrEqual(
                    forwardRiskPhysicalSummonBlock.BlockToFollowupWindowSeconds,
                    0.35f,
                    "A physical summon block should still unlock the follow-up window as one combat beat.");
                Assert.Greater(
                    forwardRiskPhysicalSummonPunish.SummonBlocks,
                    0,
                    "A physical summon-punish probe should still intercept real incoming boss projectiles.");
                Assert.Less(
                    forwardRiskPhysicalSummonPunish.PhysicalBarragePlayerHits,
                    forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits,
                    "A physical summon-punish route should reduce the forward-risk projectile hit count before the punish.");
                Assert.Greater(
                    forwardRiskPhysicalSummonPunish.SkillProjectileHits,
                    0,
                    "A physical summon-punish route should let a real Skill1 projectile hit the boss.");
                Assert.AreEqual(
                    "CleanFollowupClear",
                    forwardRiskPhysicalSummonPunish.ResultKind,
                    "A physical summon-punish route should close the block -> follow-up -> Skill1 loop as a clean route.");
                Assert.LessOrEqual(
                    forwardRiskPhysicalSummonPunish.FollowupWindowToHitSeconds,
                    1.25f,
                    "A physical summon-punish route should turn the block window into a prompt Skill1 hit.");
                Assert.GreaterOrEqual(
                    noSummon.Top3PressureWindowShare01,
                    noSummon.PeakPressureWindowShare01,
                    "Top pressure windows should include the peak pressure window.");
                Assert.Greater(
                    ignoredRecovery.UnansweredPressureBurdenShare01,
                    intended.UnansweredPressureBurdenShare01,
                    "Ignoring boss-screen pressure should carry a larger unanswered pressure burden than the intended route.");
                Assert.GreaterOrEqual(
                    intended.TimeToNextReliefWindowSeconds,
                    0f,
                    "A clean summon answer should create a measurable effective pressure relief window.");
                Assert.GreaterOrEqual(
                    blockedRecovery.TimeToNextReliefWindowSeconds,
                    0f,
                    "A recovered boss-screen route should expose its post-answer relief window after the final punish.");
                Assert.AreEqual(
                    "CounterRecoveryClear",
                    counterRecovery.ResultKind,
                    "Missing the first follow-up should still prove the counter recovery route can be stabilized and cleared.");
                Assert.AreEqual(
                    "boss_screen",
                    blockedFollowup.CounterWaveSource,
                    "Boss-screen blocks must stay separated from generic follow-up misses.");
                Assert.Greater(
                    blockedFollowup.SkillProjectilesBlockedByBossScreen,
                    0,
                    "The boss-screen branch must prove enemy pressure can block Skill1 projectiles.");
                Assert.IsTrue(
                    blockedFollowup.BossBlockedSkill1Followup,
                    "The boss-screen branch must latch the blocked follow-up as a route state.");
                Assert.AreEqual(
                    "CounterRecoveryClear",
                    blockedRecovery.ResultKind,
                    "Boss-screen blocks should become recoverable when the player rebuilds the summon answer.");
                Assert.AreEqual(
                    "CleanFollowupClear",
                    delayedIntended.ResultKind,
                    "A delayed but still-in-window Skill1 should prove the clean route is not only an instant script.");
                Assert.GreaterOrEqual(
                    delayedIntended.FollowupHitWindowDelaySeconds,
                    DelayedPunishInputSeconds - 0.15f,
                    "The delayed clean route should actually wait inside the follow-up window before hitting.");
                Assert.Greater(
                    delayedIntended.FollowupWindowRemainingAtFirstHitSeconds,
                    0.35f,
                    "The delayed clean route should still have visible follow-up window margin when Skill1 lands.");
                Assert.AreEqual(
                    "CounterRecoveryClear",
                    delayedBlockedRecovery.ResultKind,
                    "A delayed final Skill1 should prove counter recovery has a usable punish window, not only an instant script.");
                Assert.GreaterOrEqual(
                    delayedBlockedRecovery.FollowupHitWindowDelaySeconds,
                    DelayedPunishInputSeconds - 0.15f,
                    "The delayed counter route should actually wait inside the final follow-up window before hitting.");
                Assert.Greater(
                    delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds,
                    0.35f,
                    "The delayed counter route should still have final follow-up window margin when Skill1 lands.");
                Assert.AreEqual(
                    "boss_screen",
                    blockedRecovery.CounterWaveSource,
                    "The recovered boss-screen branch must preserve the original trigger source.");
                Assert.Greater(
                    blockedRecovery.SkillProjectilesBlockedByBossScreen,
                    0,
                    "Recovered boss-screen runs must still prove the block happened before recovery.");
                Assert.Greater(
                    ignoredRecovery.EnemyFrontlineBodyHits,
                    0,
                    "Ignoring boss-screen counter pressure should now produce enemy body-contact cost.");
                Assert.Greater(
                    ignoredRecovery.PlayerDamageTaken,
                    blockedRecovery.PlayerDamageTaken,
                    "Fresh counter recovery should reduce the physical body-rush cost compared with ignoring the counter.");
                Assert.GreaterOrEqual(
                    blockedRecovery.CounterWaveFinalWindowRouteScale,
                    0.84f,
                    "Fresh counter recovery should unlock at least the unstable final-punish window instead of staying critical-compressed.");
                Assert.GreaterOrEqual(
                    blockedRecovery.SkillProjectileHits,
                    2,
                    "Fresh counter recovery should convert the boss-screen block into a real Skill1 punish, not a single clipped projectile.");
                Assert.GreaterOrEqual(
                    blockedRecovery.CounterTriggerToAnswerSeconds,
                    0f,
                    "The recovered boss-screen branch should expose the counter trigger -> fresh summon answer cadence.");
                Assert.LessOrEqual(
                    blockedRecovery.CounterTriggerToAnswerSeconds,
                    0.5f,
                    "Counter pressure should unlock the fresh summon answer promptly instead of waiting on passive recharge.");
                Assert.LessOrEqual(
                    counterRecovery.CounterTriggerToAnswerSeconds,
                    0.5f,
                    "Missed follow-up recovery should also expose an immediate summon answer after the counter trigger.");
                Assert.GreaterOrEqual(
                    blockedRecovery.CounterWaveAnswerEnergyPulse,
                    100f,
                    "The recovered boss-screen branch should prove the counter trigger grants a summon-answer resource pulse.");
                Assert.GreaterOrEqual(
                    blockedRecovery.CounterAnswerToStableSeconds,
                    0f,
                    "The recovered boss-screen branch should expose the fresh summon answer -> stabilized cadence.");
                Assert.GreaterOrEqual(
                    blockedRecovery.CounterStableToFinalWindowSeconds,
                    0f,
                    "The recovered boss-screen branch should expose the stabilize -> final punish window cadence.");
                Assert.GreaterOrEqual(
                    blockedRecovery.FinalWindowToHitSeconds,
                    0f,
                    "The recovered boss-screen branch should expose the final window -> Skill1 punish cadence.");
                Assert.GreaterOrEqual(
                    blockedRecovery.BossDamageTaken,
                    intended.BossDamageTaken,
                    "Counter recovery should not pay off weaker than the clean follow-up after the player supplies the fresh summon answer.");
                Assert.Greater(
                    blockedRecovery.TotalSummonDamageFlashes,
                    0,
                    "Recovered frontline clashes should still show summon damage feedback.");
                Assert.AreEqual(
                    0,
                    blockedRecovery.TotalSummonFullBodyHitReactions,
                    "Minor summon-vs-summon pressure should read through flash/VFX without repeating full-body hit animations.");
                Assert.AreEqual(
                    blockedRecovery.TotalSummonDamageFlashes,
                    blockedRecovery.TotalNonLockingSummonDamageCues,
                    "Recovered frontline clash damage should stay non-locking unless a true break/lock policy is introduced.");
                Assert.Greater(
                    ignoredRecovery.TotalSummonDamageFlashes,
                    0,
                    "Ignoring boss-screen pressure should still produce readable summon clash damage before it reaches the player body.");
                Assert.AreEqual(
                    0,
                    ignoredRecovery.TotalSummonFullBodyHitReactions,
                    "Ignored pressure may cost HP, but minor summon clashes should not spam full-body hit reactions.");
                Assert.Greater(
                    noSummon.PlayerNonLockingDamageEvents,
                    0,
                    "Unanswered boss/projectile/body pressure should damage the player without declaring routine control lock.");
                Assert.AreEqual(
                    0,
                    noSummon.PlayerLockingDamageEvents,
                    "Routine pressure should not turn every player hit into a locking reaction.");
                Assert.Greater(
                    gunOnly.BossNonLockingDamageEvents,
                    0,
                    "Gun-only boss chip should stay readable as non-locking damage.");
                Assert.AreEqual(
                    0,
                    gunOnly.BossLockingDamageEvents,
                    "Gun-only chip should not masquerade as a major punish hit.");
                Assert.Greater(
                    intended.BossLockingDamageEvents,
                    0,
                    "Skill1 follow-up should register as a true locking/major hit event.");
                Assert.GreaterOrEqual(
                    blockedRecovery.BossLockingDamageEvents,
                    intended.BossLockingDamageEvents,
                    "Counter recovery final punish should preserve at least the clean route's major-hit read.");
                Assert.AreEqual(
                    blockedRecovery.BossLockingDamageEvents,
                    blockedRecovery.BossFullBodyEligibleDamageEvents,
                    "Boss punish locking damage should remain full-body-eligible unless a softer major-hit profile is introduced.");
                Assert.AreEqual(
                    0,
                    gunOnly.FollowupHitCameraCueRequests,
                    "Gun-only boss chip should not emit follow-up hit presentation cues.");
                Assert.AreEqual(
                    0,
                    gunOnly.FollowupHitVfxCueRequests,
                    "Gun-only boss chip should not emit the promoted follow-up hit VFX cue.");
                Assert.AreEqual(
                    0,
                    gunOnly.FollowupHitScreenCueRequests,
                    "Gun-only boss chip should not leave a Followup.Hit screen cue.");
                Assert.AreEqual(
                    0,
                    gunOnly.FollowupHitCinematicCueRequests,
                    "Gun-only boss chip should not trigger a follow-up hit cinematic cue.");
                Assert.AreEqual(
                    0,
                    gunOnly.FollowupHitSequenceBridgeRequests,
                    "Gun-only boss chip should not play the follow-up hit cinematic sequence bridge.");
                Assert.Greater(
                    intended.FollowupHitCameraCueRequests,
                    0,
                    "A clean Skill1 follow-up punish should request the follow-up hit camera cue.");
                Assert.Greater(
                    intended.FollowupHitVfxCueRequests,
                    0,
                    "A clean Skill1 follow-up punish should request the promoted follow-up hit VFX cue.");
                Assert.Greater(
                    intended.FollowupHitScreenCueRequests,
                    0,
                    "A clean Skill1 follow-up punish should request the Followup.Hit screen cue before result copy takes over.");
                Assert.AreEqual(
                    0,
                    intended.FollowupHitCinematicCueRequests,
                    "The current canonical Frontline pass keeps cinematic playback disabled; follow-up hit feel must be judged through the verified screen/camera/VFX bridge until a dedicated cinematic pass changes setup.");
                Assert.AreEqual(
                    0,
                    intended.FollowupHitSequenceBridgeRequests,
                    "The current canonical Frontline pass should not silently rely on disabled cinematic sequence playback.");
                Assert.Greater(
                    blockedFollowup.FollowupMissedCameraCueRequests,
                    0,
                    "A boss-screen block should present as a missed/blocked follow-up before recovery is supplied.");
                Assert.AreEqual(
                    0,
                    blockedFollowup.FollowupHitCameraCueRequests,
                    "A blocked boss-screen follow-up should not masquerade as a landed hit presentation.");
                Assert.AreEqual(
                    0,
                    blockedFollowup.FollowupHitCinematicCueRequests,
                    "A blocked boss-screen follow-up should not masquerade as a landed hit cinematic.");
                Assert.Greater(
                    blockedRecovery.FollowupHitCameraCueRequests,
                    0,
                    "Counter recovery should still request a follow-up hit camera cue after the fresh summon answer.");
                Assert.Greater(
                    blockedRecovery.FollowupHitVfxCueRequests,
                    0,
                    "Counter recovery should still request the promoted follow-up hit VFX cue after the fresh summon answer.");
                Assert.Greater(
                    blockedRecovery.FollowupHitScreenCueRequests,
                    0,
                    "Counter recovery should still request a Followup.Hit screen cue after the fresh summon answer.");
                Assert.AreEqual(
                    0,
                    blockedRecovery.FollowupHitCinematicCueRequests,
                    "Counter recovery should not claim cinematic follow-up hit playback while the canonical scene keeps that path disabled.");
                Assert.AreEqual(
                    0,
                    blockedRecovery.FollowupHitSequenceBridgeRequests,
                    "Counter recovery should not claim sequence bridge playback while the canonical scene keeps that path disabled.");
            }
            finally
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private static CombatPolicyContext BuildContext(PolicyKind policy)
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttack =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic action");
            SummonEnergyLadder energyLadder =
                RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action =
                RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            SummonLaneSpace laneSpace =
                RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "summon lane space");

            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            BossBarrageEmitter bossEmitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossRoot, "boss summon pressure action");

            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket review owner");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>();
            ActionCinematicCueDirector cinematicCueDirector =
                RequireComponent<ActionCinematicCueDirector>(
                    cameraCueDriver.gameObject,
                    "action cinematic cue director");
            ActionCinematicSequenceBridge cinematicSequenceBridge =
                RequireComponent<ActionCinematicSequenceBridge>(
                    cinematicCueDirector.gameObject,
                    "action cinematic sequence bridge");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(
                    RequireRoot(HudRootName),
                    "action screen cue presenter");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            float guidedLaneZ = Mathf.Lerp(laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ, 0.4f);
            player.transform.position = laneSpace.GetLaneWorldPoint(0f, guidedLaneZ, player.transform.position.y);
            Physics.SyncTransforms();

            return new CombatPolicyContext(
                policy,
                player,
                rangedBasicAttack,
                skill1Action,
                summonSlot1Action,
                energyLadder,
                targetSelector,
                laneSpace,
                bossEmitter,
                bossSummonPressureAction,
                pocketOwner,
                pocketVfxCueBridge,
                cameraCueDriver,
                cinematicCueDirector,
                cinematicSequenceBridge,
                screenCuePresenter,
                RequireComponent<CombatHealth>(player.gameObject, "player health"),
                bossHealth,
                closeThreatHealth,
                RequireCombatHitCollider(player.gameObject, RequireComponent<CombatHealth>(player.gameObject, "player health"), "player"),
                RequireCombatHitCollider(bossRoot, bossHealth, "boss"),
                RequireCombatHitCollider(closeThreatRoot, closeThreatHealth, "close threat"));
        }

        private static IEnumerator RunPolicy(CombatPolicyContext context)
        {
            switch (context.Policy)
            {
                case PolicyKind.NoSummonNoFire:
                    yield return RunNoSummonNoFire(context);
                    break;
                case PolicyKind.GunOnly:
                    yield return RunGunOnly(context);
                    break;
                case PolicyKind.NoSummonSurvivalLimit:
                    yield return RunNoSummonSurvivalLimit(context);
                    break;
                case PolicyKind.GunOnlySurvivalLimit:
                    yield return RunGunOnlySurvivalLimit(context);
                    break;
                case PolicyKind.BacklineEnergyProbe:
                    yield return RunEnergyRiskProbe(context, BacklineEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.ForwardRiskEnergyProbe:
                    yield return RunEnergyRiskProbe(context, ForwardEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.BacklineBarrageProbe:
                    yield return RunBarrageShapeProbe(context, BacklineEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.ForwardRiskBarrageProbe:
                    yield return RunBarrageShapeProbe(context, ForwardEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.BacklinePhysicalBarrageProbe:
                    yield return RunPhysicalBarrageProbe(context, BacklineEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.ForwardRiskPhysicalBarrageProbe:
                    yield return RunPhysicalBarrageProbe(context, ForwardEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.ForwardRiskPhysicalSummonBlockProbe:
                    yield return RunPhysicalSummonBlockProbe(context, ForwardEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    yield return RunPhysicalSummonPunishProbe(context, ForwardEnergyProbeForwardRisk01);
                    break;
                case PolicyKind.IntendedRoute:
                    yield return RunIntendedRoute(context);
                    break;
                case PolicyKind.IntendedDelayedFollowup:
                    yield return RunIntendedDelayedFollowup(context);
                    break;
                case PolicyKind.LateSummon:
                    yield return RunLateSummon(context);
                    break;
                case PolicyKind.MissedFollowupCounterRecovery:
                    yield return RunMissedFollowupCounterRecovery(context);
                    break;
                case PolicyKind.BossScreenBlockedFollowup:
                    yield return RunBossScreenBlockedFollowup(context);
                    break;
                case PolicyKind.BossScreenIgnoredNoRecovery:
                    yield return RunBossScreenIgnoredNoRecovery(context);
                    break;
                case PolicyKind.BossScreenBlockCounterRecovery:
                    yield return RunBossScreenBlockCounterRecovery(context);
                    break;
                case PolicyKind.BossScreenDelayedCounterRecovery:
                    yield return RunBossScreenDelayedCounterRecovery(context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerator RunNoSummonNoFire(CombatPolicyContext context)
        {
            for (int wave = 0; wave < 6 && context.PlayerHealth.IsAlive; wave++)
            {
                yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
                yield return Advance(context, 2.25f);
            }
        }

        private static IEnumerator RunGunOnly(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);

            for (int i = 0; i < 5 && context.PlayerHealth.IsAlive; i++)
            {
                yield return FireBasicAt(context, context.BossHealth, context.BossCollider);
                yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
                yield return Advance(context, 1.35f);
            }
        }

        private static IEnumerator RunNoSummonSurvivalLimit(CombatPolicyContext context)
        {
            context.Metrics.SurvivalProbeMaxSeconds = SurvivalLimitProbeMaxSeconds;
            while (context.PlayerHealth.IsAlive
                && context.BossHealth.IsAlive
                && context.Metrics.ElapsedSeconds < SurvivalLimitProbeMaxSeconds)
            {
                yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
                yield return Advance(context, 2.25f);
            }
        }

        private static IEnumerator RunGunOnlySurvivalLimit(CombatPolicyContext context)
        {
            context.Metrics.SurvivalProbeMaxSeconds = SurvivalLimitProbeMaxSeconds;
            yield return DefeatCloseThreatWithBasicFire(context);
            while (context.PlayerHealth.IsAlive
                && context.BossHealth.IsAlive
                && context.Metrics.ElapsedSeconds < SurvivalLimitProbeMaxSeconds)
            {
                yield return FireBasicAt(context, context.BossHealth, context.BossCollider);
                if (!context.BossHealth.IsAlive)
                {
                    break;
                }

                yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
                yield return Advance(context, 1.35f);
            }
        }

        private static IEnumerator RunEnergyRiskProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.EnergyLadder.ResetLadder();
            context.Metrics.EnergyProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            context.Metrics.EnergyProbeStartAtSeconds = context.Metrics.ElapsedSeconds;
            context.Sample();

            float start = context.Metrics.ElapsedSeconds;
            while (context.EnergyLadder.AvailableTier < 1
                && context.Metrics.ElapsedSeconds - start < EnergyProbeMaxSeconds)
            {
                yield return Advance(context, 0.1f);
            }

            if (context.EnergyLadder.AvailableTier < 1)
            {
                context.Metrics.Notes.Add("energy probe did not reach LV1");
            }
        }

        private static IEnumerator RunBarrageShapeProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.Metrics.BarrageShapeProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            context.Sample();
            RecordBarrageShapeProbe(context);
            yield return null;
        }

        private static IEnumerator RunPhysicalBarrageProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            context.Sample();
            yield return ApplyPhysicalBossBarrage(context, PhysicalBarrageProbeFlightSeconds);
        }

        private static IEnumerator RunPhysicalSummonBlockProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            yield return DefeatCloseThreatWithBasicFire(context);
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            context.Sample();
            yield return ChargeEnergyToTier(context, 1, EnergyProbeMaxSeconds);

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"physical summon blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return Advance(context, 0.2f);
            DeactivateActiveBossProjectiles();
            context.BossEmitter.SetFiringEnabled(false);
            context.BossEmitter.SetFiringEnabled(true);
            if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
            {
                context.Metrics.Notes.Add("physical summon block priority barrage unavailable");
            }

            yield return ApplyPhysicalBossBarrage(context, PhysicalBarrageProbeFlightSeconds);
        }

        private static IEnumerator RunPhysicalSummonPunishProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            yield return DefeatCloseThreatWithBasicFire(context);
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            context.Sample();
            yield return ChargeEnergyToTier(context, 1, EnergyProbeMaxSeconds);

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"physical summon punish blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return Advance(context, 0.2f);
            DeactivateActiveBossProjectiles();
            context.BossEmitter.SetFiringEnabled(false);
            context.BossEmitter.SetFiringEnabled(true);
            if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
            {
                context.Metrics.Notes.Add("physical summon punish priority barrage unavailable");
            }

            yield return ApplyPhysicalBossBarrageAndPunish(context, PhysicalBarrageProbeFlightSeconds);
        }

        private static IEnumerator RunIntendedRoute(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator RunIntendedDelayedFollowup(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return DelayInsideFollowupWindow(context, DelayedPunishInputSeconds);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator RunLateSummon(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
            yield return Advance(context, 2.25f);
            yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
            yield return Advance(context, 2.25f);

            if (context.PlayerHealth.IsAlive)
            {
                yield return ChargeEnergyToTier(context, 1, 14f);
                yield return UseSummonAndBlockNextBossWave(context);
                yield return ConfirmSkill1Followup(context);
                yield return Advance(context, 1.0f);
            }
        }

        private static IEnumerator RunMissedFollowupCounterRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return LetFollowupWindowExpire(context);
            yield return AnswerCounterWaveWithFreshSummon(context);
            yield return WaitForCounterFinalWindow(context, 3f);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator RunBossScreenBlockedFollowup(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ReleaseBossScreenAndBlockSkill1Followup(context);
            yield return WaitForCounterFinalWindow(context, 2f);
            yield return Advance(context, 0.25f);
        }

        private static IEnumerator RunBossScreenIgnoredNoRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ReleaseBossScreenAndBlockSkill1Followup(context);
            yield return WaitForCounterFinalWindow(context, 2f);
            yield return Advance(context, 8f);
        }

        private static IEnumerator RunBossScreenBlockCounterRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ReleaseBossScreenAndBlockSkill1Followup(context);
            yield return AnswerCounterWaveWithFreshSummon(context);
            yield return WaitForCounterFinalWindow(context, 3f);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator RunBossScreenDelayedCounterRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ReleaseBossScreenAndBlockSkill1Followup(context);
            yield return AnswerCounterWaveWithFreshSummon(context);
            yield return WaitForCounterFinalWindow(context, 3f);
            yield return DelayInsideFollowupWindow(context, DelayedPunishInputSeconds);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator DefeatCloseThreatWithBasicFire(CombatPolicyContext context)
        {
            context.TargetSelector.NotifyTargetContact(context.CloseThreatHealth);
            context.TargetSelector.RefreshTarget();

            int shots = 0;
            while (context.CloseThreatHealth.IsAlive && shots < 10)
            {
                yield return FireBasicAt(context, context.CloseThreatHealth, context.CloseThreatCollider);
                shots++;
            }
        }

        private static IEnumerator FireBasicAt(
            CombatPolicyContext context,
            CombatHealth targetHealth,
            Collider targetCollider)
        {
            context.TargetSelector.NotifyTargetContact(targetHealth);
            context.TargetSelector.RefreshTarget();

            while (!context.RangedBasicAttack.IsFireReady)
            {
                yield return Advance(context, 0.05f);
            }

            if (!context.RangedBasicAttack.TryFire())
            {
                context.Metrics.Notes.Add($"basic blocked: {context.RangedBasicAttack.LastUseBlockedReason}");
                yield break;
            }

            context.Metrics.BasicShots++;
            LaneActionProjectile projectile = FindActivePlayerProjectile();
            if (projectile != null && projectile.TryApplyImpact(targetCollider, projectile.transform.position))
            {
                if (targetHealth == context.CloseThreatHealth)
                {
                    context.Metrics.CloseThreatBasicHits++;
                }
                else if (targetHealth == context.BossHealth)
                {
                    context.Metrics.BossBasicHits++;
                }
            }

            yield return Advance(context, GetFloat(context.RangedBasicAttack, "fireIntervalSeconds") + 0.02f);
        }

        private static IEnumerator ChargeEnergyToTier(
            CombatPolicyContext context,
            int tier,
            float maxSeconds)
        {
            float start = context.Metrics.ElapsedSeconds;
            while (context.EnergyLadder.AvailableTier < tier
                && context.Metrics.ElapsedSeconds - start < maxSeconds)
            {
                yield return Advance(context, 0.1f);
            }

            if (context.EnergyLadder.AvailableTier < tier)
            {
                context.Metrics.Notes.Add($"energy tier {tier} not ready");
            }
        }

        private static void MovePlayerToForwardRisk(CombatPolicyContext context, float forwardRisk01)
        {
            float laneZ = Mathf.Lerp(
                context.LaneSpace.BackLimitZ,
                context.LaneSpace.ForwardBoundaryZ,
                Mathf.Clamp01(forwardRisk01));
            context.Player.transform.position = context.LaneSpace.GetLaneWorldPoint(
                0f,
                laneZ,
                context.Player.transform.position.y);
            Physics.SyncTransforms();
        }

        private static void RecordBarrageShapeProbe(CombatPolicyContext context)
        {
            BossBarragePatternProfile pattern = context.BossEmitter.CurrentPattern;
            if (!context.BossEmitter.BeginWindup())
            {
                context.Metrics.Notes.Add("barrage shape probe windup unavailable");
                return;
            }

            Vector2[] targets = new Vector2[BarrageShapePreviewCapacity];
            int count = context.BossEmitter.BuildPendingLaneTargetPreview(targets);
            context.Metrics.BarrageShapePatternId = pattern != null ? pattern.PatternId : "unknown";
            context.Metrics.BarrageShapePendingForwardRisk01 = context.BossEmitter.PendingForwardRisk01;
            context.Metrics.BarrageShapeProjectileCount = count;
            if (count <= 0)
            {
                context.Metrics.Notes.Add("barrage shape probe target preview empty");
                return;
            }

            Vector2 playerLanePoint = context.LaneSpace.GetLaneCoordinates(context.Player.transform.position);
            float nearestDistance = float.MaxValue;
            float lateralGapTotal = 0f;
            float depthGapTotal = 0f;
            float laneDistanceTotal = 0f;
            float densityTotal = 0f;
            int nearCount = 0;
            for (int i = 0; i < count; i++)
            {
                Vector2 delta = targets[i] - playerLanePoint;
                float lateralGap = Mathf.Abs(delta.x);
                float depthGap = Mathf.Abs(delta.y);
                float laneDistance = delta.magnitude;
                lateralGapTotal += lateralGap;
                depthGapTotal += depthGap;
                laneDistanceTotal += laneDistance;
                nearestDistance = Mathf.Min(nearestDistance, laneDistance);
                if (laneDistance <= BarrageShapeProbeNearRadius)
                {
                    nearCount++;
                }

                densityTotal += 1f / Mathf.Max(0.1f, laneDistance + 0.35f);
            }

            float safeCount = Mathf.Max(1, count);
            context.Metrics.BarrageShapeNearProjectileCount = nearCount;
            context.Metrics.BarrageShapeNearestLaneDistance = nearestDistance < float.MaxValue
                ? nearestDistance
                : -1f;
            context.Metrics.BarrageShapeAverageLateralGap = lateralGapTotal / safeCount;
            context.Metrics.BarrageShapeAverageDepthGap = depthGapTotal / safeCount;
            context.Metrics.BarrageShapeAverageLaneDistance = laneDistanceTotal / safeCount;
            context.Metrics.BarrageShapeThreatDensity = densityTotal / safeCount;
            context.Metrics.BarrageShapeReadout =
                $"{context.Metrics.BarrageShapePatternId} {nearCount}/{count} near "
                + $"avgLat {context.Metrics.BarrageShapeAverageLateralGap:0.00} "
                + $"density {context.Metrics.BarrageShapeThreatDensity:0.00}";
        }

        private static IEnumerator ApplyPhysicalBossBarrage(
            CombatPolicyContext context,
            float flightSeconds)
        {
            BossBarragePatternProfile pattern = context.BossEmitter.CurrentPattern;
            float healthBefore = context.PlayerHealth.CurrentHealth;
            if (!context.BossEmitter.BeginWindup())
            {
                context.Metrics.Notes.Add("physical barrage probe windup unavailable");
                yield break;
            }

            context.Metrics.PhysicalBarragePatternId = pattern != null ? pattern.PatternId : "unknown";
            context.Metrics.PhysicalBarragePendingForwardRisk01 = context.BossEmitter.PendingForwardRisk01;
            int spawned = context.BossEmitter.FirePendingWave();
            context.Metrics.BossWaves++;
            context.Metrics.BossProjectilesSpawned += spawned;
            context.Metrics.PhysicalBarrageWaves++;
            context.Metrics.PhysicalBarrageProjectilesSpawned += spawned;

            BossBarrageProjectile[] projectiles = FindActiveBossProjectiles();
            context.Metrics.PhysicalBarrageTrackedProjectileCount += projectiles.Length;
            Physics.SyncTransforms();
            yield return Advance(context, flightSeconds);
            Physics.SyncTransforms();

            RecordPhysicalBossBarrageResults(context, projectiles, healthBefore);
            DeactivateActiveBossProjectiles();
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator ApplyPhysicalBossBarrageAndPunish(
            CombatPolicyContext context,
            float flightSeconds)
        {
            BossBarragePatternProfile pattern = context.BossEmitter.CurrentPattern;
            float healthBefore = context.PlayerHealth.CurrentHealth;
            if (!context.BossEmitter.BeginWindup())
            {
                context.Metrics.Notes.Add("physical punish barrage windup unavailable");
                yield break;
            }

            context.Metrics.PhysicalBarragePatternId = pattern != null ? pattern.PatternId : "unknown";
            context.Metrics.PhysicalBarragePendingForwardRisk01 = context.BossEmitter.PendingForwardRisk01;
            int spawned = context.BossEmitter.FirePendingWave();
            context.Metrics.BossWaves++;
            context.Metrics.BossProjectilesSpawned += spawned;
            context.Metrics.PhysicalBarrageWaves++;
            context.Metrics.PhysicalBarrageProjectilesSpawned += spawned;

            BossBarrageProjectile[] bossProjectiles = FindActiveBossProjectiles();
            context.Metrics.PhysicalBarrageTrackedProjectileCount += bossProjectiles.Length;
            Physics.SyncTransforms();

            float start = context.Metrics.ElapsedSeconds;
            while (!context.PocketOwner.IsSummonFollowupWindowActive
                && context.Metrics.ElapsedSeconds - start < Mathf.Min(1.0f, flightSeconds))
            {
                yield return Advance(context, 0.05f);
            }

            if (!context.PocketOwner.IsSummonFollowupWindowActive)
            {
                context.Metrics.Notes.Add("physical summon punish window did not open before Skill1");
            }
            else
            {
                yield return ConfirmSkill1FollowupPhysically(context, PhysicalSkill1ProbeFlightSeconds);
            }

            float elapsedSinceBarrage = context.Metrics.ElapsedSeconds - start;
            if (elapsedSinceBarrage < flightSeconds)
            {
                yield return Advance(context, flightSeconds - elapsedSinceBarrage);
            }

            Physics.SyncTransforms();
            RecordPhysicalBossBarrageResults(context, bossProjectiles, healthBefore);
            DeactivateActiveBossProjectiles();
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator ConfirmSkill1FollowupPhysically(
            CombatPolicyContext context,
            float flightSeconds)
        {
            if (context.EnergyLadder.AvailableTier <= 0)
            {
                yield return ChargeEnergyToTier(context, 1, 8f);
            }

            context.TargetSelector.NotifyTargetContact(context.BossHealth);
            context.TargetSelector.RefreshTarget();
            if (!context.Skill1Action.TryUseSkill1())
            {
                context.Metrics.Notes.Add($"physical skill1 blocked: {context.Skill1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSkillUse(context);
            LaneActionProjectile[] projectiles = FindActivePlayerProjectiles();
            if (projectiles.Length == 0)
            {
                context.Metrics.Notes.Add("physical skill1 produced no tracked projectile");
            }

            float start = context.Metrics.ElapsedSeconds;
            while (AnyLaneProjectileActive(projectiles)
                && context.Metrics.ElapsedSeconds - start < flightSeconds)
            {
                yield return Advance(context, 0.05f);
            }

            int hits = 0;
            for (int i = 0; i < projectiles.Length; i++)
            {
                LaneActionProjectile projectile = projectiles[i];
                if (projectile != null
                    && projectile.LastImpactTargetHealth == context.BossHealth
                    && projectile.LastImpactResult == ProjectileImpactResult.AppliedDamage)
                {
                    hits++;
                }
            }

            context.Metrics.SkillProjectileHits += hits;
            if (hits <= 0)
            {
                context.Metrics.Notes.Add("physical skill1 did not hit boss");
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            float clearDelay = GetFloat(context.PocketOwner, "skill1FollowupClearDelaySeconds") + 0.05f;
            context.PocketOwner.Tick(clearDelay);
            context.Metrics.ElapsedSeconds += clearDelay;
            context.Sample();
            yield return null;
        }

        private static void RecordPhysicalBossBarrageResults(
            CombatPolicyContext context,
            IReadOnlyList<BossBarrageProjectile> projectiles,
            float healthBefore)
        {
            int inactiveAfterFlight = 0;
            int playerImpactAttempts = 0;
            int playerHits = 0;
            for (int i = 0; i < projectiles.Count; i++)
            {
                BossBarrageProjectile projectile = projectiles[i];
                if (projectile == null)
                {
                    continue;
                }

                if (!projectile.IsActive)
                {
                    inactiveAfterFlight++;
                }

                if (projectile.LastImpactTargetHealth != context.PlayerHealth)
                {
                    continue;
                }

                playerImpactAttempts++;
                if (projectile.LastImpactResult == ProjectileImpactResult.AppliedDamage)
                {
                    playerHits++;
                }
            }

            float playerDamage = Mathf.Max(0f, healthBefore - context.PlayerHealth.CurrentHealth);
            context.Metrics.PhysicalBarrageInactiveAfterFlight += inactiveAfterFlight;
            context.Metrics.PhysicalBarragePlayerImpactAttempts += playerImpactAttempts;
            context.Metrics.PhysicalBarragePlayerHits += playerHits;
            context.Metrics.PhysicalBarragePlayerDamage += playerDamage;
            context.Metrics.BossProjectilesHitPlayer += playerHits;
            context.Metrics.PhysicalBarrageReadout =
                $"{context.Metrics.PhysicalBarragePatternId} hits {playerHits}/{projectiles.Count} "
                + $"damage {playerDamage:0.0} inactive {inactiveAfterFlight}";
        }

        private static bool AnyLaneProjectileActive(IReadOnlyList<LaneActionProjectile> projectiles)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i] != null && projectiles[i].IsActive)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerator UseSummonAndBlockNextBossWave(CombatPolicyContext context)
        {
            if (context.EnergyLadder.AvailableTier <= 0)
            {
                yield return ChargeEnergyToTier(context, 1, 8f);
            }

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"summon blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            yield return Advance(context, 0.2f);
            yield return ApplyBossWave(context, BossWaveAnswer.SummonScreen);
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator ConfirmSkill1Followup(CombatPolicyContext context)
        {
            if (context.PocketOwner.IsCounterWaveCompletionRecorded
                && !context.PocketOwner.IsCounterWaveFinalWindowOpened)
            {
                yield return WaitForCounterFinalWindow(context, 3f);
            }

            if (context.EnergyLadder.AvailableTier <= 0)
            {
                yield return ChargeEnergyToTier(context, 1, 8f);
            }

            context.TargetSelector.NotifyTargetContact(context.BossHealth);
            context.TargetSelector.RefreshTarget();
            if (!context.Skill1Action.TryUseSkill1())
            {
                context.Metrics.Notes.Add($"skill1 blocked: {context.Skill1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSkillUse(context);
            LaneActionProjectile[] projectiles = FindActivePlayerProjectiles();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].TryApplyImpact(context.BossCollider, projectiles[i].transform.position))
                {
                    context.Metrics.SkillProjectileHits++;
                }
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            float clearDelay = GetFloat(context.PocketOwner, "skill1FollowupClearDelaySeconds") + 0.05f;
            context.PocketOwner.Tick(clearDelay);
            context.Metrics.ElapsedSeconds += clearDelay;
            context.Sample();
            yield return null;
        }

        private static IEnumerator LetFollowupWindowExpire(CombatPolicyContext context)
        {
            float waitSeconds = Mathf.Max(
                context.PocketOwner.SummonFollowupWindowRemainingSeconds,
                context.Metrics.LastSummonFollowupWindowDuration);
            yield return Advance(context, waitSeconds + 0.1f);
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator DelayInsideFollowupWindow(CombatPolicyContext context, float waitSeconds)
        {
            if (!context.PocketOwner.IsSummonFollowupWindowActive)
            {
                context.Metrics.Notes.Add("delayed punish requested without an active follow-up window");
                yield break;
            }

            yield return Advance(context, Mathf.Max(0f, waitSeconds));
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator AnswerCounterWaveWithFreshSummon(CombatPolicyContext context)
        {
            if (context.EnergyLadder.AvailableTier <= 0)
            {
                yield return ChargeEnergyToTier(context, 1, 8f);
            }

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"counter summon blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, true);
            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return null;
        }

        private static IEnumerator ReleaseBossScreenAndBlockSkill1Followup(CombatPolicyContext context)
        {
            if (!context.BossSummonPressureAction.TryReleasePressureSummon(1))
            {
                context.Metrics.Notes.Add("boss summon pressure release blocked");
                yield break;
            }

            SummonPressureScreen enemyScreen = FindActiveEnemyPressureScreen();
            if (enemyScreen == null)
            {
                context.Metrics.Notes.Add("enemy pressure screen missing for Skill1 block");
                yield break;
            }

            if (context.EnergyLadder.AvailableTier <= 0)
            {
                yield return ChargeEnergyToTier(context, 1, 8f);
            }

            context.TargetSelector.NotifyTargetContact(context.BossHealth);
            context.TargetSelector.RefreshTarget();
            if (!context.Skill1Action.TryUseSkill1())
            {
                context.Metrics.Notes.Add($"skill1 blocked before boss screen: {context.Skill1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSkillUse(context);
            context.PocketOwner.Tick(0f);

            LaneActionProjectile[] projectiles = FindActivePlayerProjectiles();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (enemyScreen.IsActive && enemyScreen.TryIntercept(projectiles[i]))
                {
                    context.Metrics.SkillProjectilesBlockedByBossScreen++;
                }
                else if (projectiles[i].TryApplyImpact(context.BossCollider, projectiles[i].transform.position))
                {
                    context.Metrics.SkillProjectileHits++;
                }
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return null;
        }

        private static void RecordSummonUse(CombatPolicyContext context, bool isCounterAnswer)
        {
            context.Metrics.SummonUses++;
            if (context.Metrics.FirstSummonUseAtSeconds < 0f)
            {
                context.Metrics.FirstSummonUseAtSeconds = context.Metrics.ElapsedSeconds;
            }

            if (isCounterAnswer && context.Metrics.FirstCounterAnswerSummonAtSeconds < 0f)
            {
                context.Metrics.FirstCounterAnswerSummonAtSeconds = context.Metrics.ElapsedSeconds;
            }
        }

        private static void RecordSkillUse(CombatPolicyContext context)
        {
            context.Metrics.SkillUses++;
            if (context.Metrics.FirstSkill1UseAtSeconds < 0f)
            {
                context.Metrics.FirstSkill1UseAtSeconds = context.Metrics.ElapsedSeconds;
            }
        }

        private static IEnumerator WaitForCounterFinalWindow(
            CombatPolicyContext context,
            float maxSeconds)
        {
            float start = context.Metrics.ElapsedSeconds;
            while (!context.PocketOwner.IsCounterWaveFinalWindowOpened
                && context.Metrics.ElapsedSeconds - start < maxSeconds)
            {
                yield return Advance(context, 0.1f);
            }

            if (!context.PocketOwner.IsCounterWaveFinalWindowOpened)
            {
                context.Metrics.Notes.Add("counter final window did not open before Skill1");
            }
        }

        private static IEnumerator ApplyBossWave(
            CombatPolicyContext context,
            BossWaveAnswer answer)
        {
            if (!context.BossEmitter.BeginWindup())
            {
                context.Metrics.Notes.Add("boss windup unavailable");
                yield break;
            }

            int spawned = context.BossEmitter.FirePendingWave();
            context.Metrics.BossWaves++;
            context.Metrics.BossProjectilesSpawned += spawned;
            yield return null;

            BossBarrageProjectile[] projectiles = FindActiveBossProjectiles();
            if (answer == BossWaveAnswer.SummonScreen)
            {
                SummonPressureScreen screen = FindActiveAllyPressureScreen();
                if (screen == null)
                {
                    context.Metrics.Notes.Add("summon screen missing for boss wave");
                }
                else
                {
                    for (int i = 0; i < projectiles.Length && screen.IsActive; i++)
                    {
                        screen.TryIntercept(projectiles[i]);
                    }
                }
            }

            if (answer == BossWaveAnswer.PlayerTakesHit)
            {
                BossBarrageProjectile hitProjectile = FindFirstActiveBossProjectile();
                if (hitProjectile != null
                    && hitProjectile.TryApplyImpact(context.PlayerCollider, hitProjectile.transform.position))
                {
                    context.Metrics.BossProjectilesHitPlayer++;
                }
            }

            DeactivateActiveBossProjectiles();
            context.PocketOwner.Tick(0f);
        }

        private static IEnumerator Advance(CombatPolicyContext context, float seconds)
        {
            float remaining = Mathf.Max(0f, seconds);
            while (remaining > 0f)
            {
                yield return null;
                float deltaTime = Mathf.Max(0.001f, Time.deltaTime);
                remaining -= deltaTime;
                context.Metrics.ElapsedSeconds += deltaTime;
                context.Sample(deltaTime);
            }
        }

        private static void WriteReports(IReadOnlyList<PolicyMetrics> results)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, BuildMarkdown(results), Encoding.UTF8);
            File.WriteAllText(JsonPath, BuildJson(results), Encoding.UTF8);
        }

        private static string BuildMarkdown(IReadOnlyList<PolicyMetrics> results)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# DimensionBrawl Frontline Combat Policy Report");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Scene: `{ScenePath}`");
            builder.AppendLine();
            builder.AppendLine("## ArkData Read");
            builder.AppendLine("- Stage/runtime/pressure: each policy is one route through the same Frontline stage shell, so unanswered pressure, direct fire, intended summon, and late summon can be compared without changing the scene.");
            builder.AppendLine("- Trigger -> target -> effect -> status/presentation: follow-up windows, Skill1 hit confirms, boss-screen blocks, counter-wave observation, ally hold, and result records are emitted as measured route evidence.");
            builder.AppendLine("- QTE/state lock-unlock: summon block opens the follow-up window; missed or blocked follow-up can lock the route into counter pressure; ally hold can unlock the final recovery window.");
            builder.AppendLine();
            AppendArkDataCoverageSummary(builder, results);
            builder.AppendLine();
            AppendStructuralGateSummary(builder, results);
            builder.AppendLine();
            builder.AppendLine("| Policy | Result | Sim s | HP lost | Boss dmg | Stability | Min stability | Boss waves | Player hits | Summons | Blocks | Skill1 hits | Fronts A/E | Route shape | Decision |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.ResultKind);
                builder.Append(" | ");
                builder.Append(result.ElapsedSeconds.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.PlayerDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.RouteStability01));
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.MinRouteStability01));
                builder.Append(" | ");
                builder.Append(result.BossWaves);
                builder.Append(" | ");
                builder.Append(result.BossProjectilesHitPlayer);
                builder.Append(" | ");
                builder.Append(result.SummonUses);
                builder.Append(" | ");
                builder.Append(result.SummonBlocks);
                builder.Append(" | ");
                builder.Append(result.SkillProjectileHits);
                builder.Append(" | ");
                builder.Append(result.MaxAllyFrontlineCount);
                builder.Append("/");
                builder.Append(result.MaxEnemyFrontlineCount);
                builder.Append(" | ");
                builder.Append(ResolveRouteShape(result));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.RouteDecision));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Long Survival Limit");
            builder.AppendLine("| Policy | Cap | Player down | Boss down | HP lost | Boss dmg | Result | Decision |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                if (result.SurvivalProbeMaxSeconds <= 0f)
                {
                    continue;
                }

                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.SurvivalProbeMaxSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FirstPlayerDownAtSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FirstBossDownAtSeconds));
                builder.Append(" | ");
                builder.Append(result.PlayerDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.ResultKind));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.RouteDecision));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Route Evidence");
            builder.AppendLine("| Policy | Proof | Follow-up | Counter | Counter answer | Final window | Result record |");
            builder.AppendLine("|---|---|---|---|---|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(EscapeTable($"{result.RouteProofState}({result.RouteProofReadout})"));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveFollowupTiming(result)));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveCounterTiming(result)));
                builder.Append(" | ");
                builder.Append(EscapeTable($"{result.CounterWaveAnswerState}({result.CounterWaveAnswerReadout})"));
                builder.Append(" | ");
                builder.Append(EscapeTable(
                    $"{result.CounterWaveFinalWindowState}({result.CounterWaveFinalWindowReadout}) "
                    + $"{FormatSeconds(result.CounterWaveFinalWindowSeconds)} x{result.CounterWaveFinalWindowRouteScale:0.00}"));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveResultRecordReadout(result)));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Lock/Unlock Cadence");
            builder.AppendLine("| Policy | Summon->block | Block->window | Window->Skill1 | Window->hit | Block->hit | Boss release->screen | Answer pulse | Counter->answer | Answer->stable | Stable->final | Final->hit |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.SummonUseToBlockSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.BlockToFollowupWindowSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FollowupWindowToSkillUseSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FollowupHitWindowDelaySeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.BlockToFollowupHitSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.BossScreenReleaseToBlockSeconds));
                builder.Append(" | ");
                builder.Append(result.CounterWaveAnswerEnergyPulse.ToString("0"));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.CounterTriggerToAnswerSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.CounterAnswerToStableSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.CounterStableToFinalWindowSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FinalWindowToHitSeconds));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Punish Window Margin");
            builder.AppendLine("| Policy | First window | Hit window | Hit delay | Remaining at hit | Used share | Final scale | Result |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FirstFollowupWindowDurationSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FollowupWindowDurationAtFirstHitSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FollowupWindowToHitSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FollowupWindowRemainingAtFirstHitSeconds));
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.FollowupHitWindowUsedShare01));
                builder.Append(" | ");
                builder.Append($"x{result.CounterWaveFinalWindowRouteScale:0.00}");
                builder.Append(" | ");
                builder.Append(EscapeTable(result.ResultKind));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Forward-Risk Energy Split");
            builder.AppendLine("| Policy | Target risk | LV1 ready | Avg risk | Avg gain | Band seconds B/M/F | End tier/fill | Last band |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.EnergyProbeTargetForwardRisk01));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.EnergyTier1DurationSeconds));
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.AverageEnergyForwardRisk01));
                builder.Append(" | ");
                builder.Append($"x{result.AverageEnergyGainMultiplier:0.00}");
                builder.Append(" | ");
                builder.Append(
                    $"{FormatSeconds(result.BackSafetyBandSeconds)}/"
                    + $"{FormatSeconds(result.MidChargeBandSeconds)}/"
                    + $"{FormatSeconds(result.ForwardRiskBandSeconds)}");
                builder.Append(" | ");
                builder.Append($"LV{result.EnergyAvailableTier}/{FormatPercent01(result.EnergyFillRatio)}");
                builder.Append(" | ");
                builder.Append(EscapeTable(result.LastEnergyRiskBand));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Forward-Risk Barrage Shape");
            builder.AppendLine("| Policy | Target risk | Pending risk | Pattern | Projectiles | Near radius | Avg lateral gap | Avg depth gap | Nearest | Density | Readout |");
            builder.AppendLine("|---|---:|---:|---|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.BarrageShapeProbeTargetForwardRisk01));
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.BarrageShapePendingForwardRisk01));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.BarrageShapePatternId));
                builder.Append(" | ");
                builder.Append(result.BarrageShapeProjectileCount);
                builder.Append(" | ");
                builder.Append($"{result.BarrageShapeNearProjectileCount}/{BarrageShapeProbeNearRadius:0.00}");
                builder.Append(" | ");
                builder.Append(FormatOptionalDistance(result.BarrageShapeAverageLateralGap));
                builder.Append(" | ");
                builder.Append(FormatOptionalDistance(result.BarrageShapeAverageDepthGap));
                builder.Append(" | ");
                builder.Append(FormatOptionalDistance(result.BarrageShapeNearestLaneDistance));
                builder.Append(" | ");
                builder.Append(FormatOptionalDistance(result.BarrageShapeThreatDensity));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.BarrageShapeReadout));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Forward-Risk Physical Barrage");
            builder.AppendLine("| Policy | Target risk | Pending risk | Pattern | Waves | Spawned | Tracked | Inactive | Player attempts | Player hits | Player dmg | Readout |");
            builder.AppendLine("|---|---:|---:|---|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.PhysicalBarrageProbeTargetForwardRisk01));
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.PhysicalBarragePendingForwardRisk01));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.PhysicalBarragePatternId));
                builder.Append(" | ");
                builder.Append(result.PhysicalBarrageWaves);
                builder.Append(" | ");
                builder.Append(result.PhysicalBarrageProjectilesSpawned);
                builder.Append(" | ");
                builder.Append(result.PhysicalBarrageTrackedProjectileCount);
                builder.Append(" | ");
                builder.Append(result.PhysicalBarrageInactiveAfterFlight);
                builder.Append(" | ");
                builder.Append(result.PhysicalBarragePlayerImpactAttempts);
                builder.Append(" | ");
                builder.Append(result.PhysicalBarragePlayerHits);
                builder.Append(" | ");
                builder.Append(result.PhysicalBarragePlayerDamage.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.PhysicalBarrageReadout));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Pressure Exposure");
            builder.AppendLine("| Policy | Drain used | Hit penalty | Avg drain/s | Peak drain/s | Avg slot | Avg front scale | Enemy-only | Contested | Ally-only | Last front |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.RouteDrainAccumulated01));
                builder.Append(" | ");
                builder.Append($"{FormatPercent01(result.TotalUnansweredBossHitRoutePenalty01)} x{result.UnansweredBossHitRoutePenaltyCount}");
                builder.Append(" | ");
                builder.Append($"{ResolveAverage(result.RouteDrainAccumulated01, result.ElapsedSeconds):0.000}");
                builder.Append(" | ");
                builder.Append($"{result.MaxRouteStabilityDrainPerSecond:0.000}");
                builder.Append(" | ");
                builder.Append($"{ResolveAverage(result.RoutePressureWeightSeconds, result.ElapsedSeconds):0.00}");
                builder.Append(" | ");
                builder.Append($"{ResolveAverage(result.FrontlinePresenceScaleSeconds, result.ElapsedSeconds):0.00}");
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.EnemyOnlyFrontlineSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.ContestedFrontlineSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.AllyOnlyFrontlineSeconds));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.FrontlinePresenceReadout));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## ArkData Effective Pressure Shape");
            builder.AppendLine("| Policy | Window | Peak share | Top3 share | Peak at | Relief gap | Dominant burden | Unanswered/answered | Effective windows |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append($"{result.PressureWindowSeconds:0.0}s");
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.PeakPressureWindowShare01));
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.Top3PressureWindowShare01));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.PeakPressureWindowStartSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.TimeToNextReliefWindowSeconds));
                builder.Append(" | ");
                builder.Append(EscapeTable(
                    $"{result.DominantPressureBurdenState} {FormatPercent01(result.DominantPressureBurdenShare01)}"));
                builder.Append(" | ");
                builder.Append(
                    $"{FormatPercent01(result.UnansweredPressureBurdenShare01)}/{FormatPercent01(result.AnsweredPressureBurdenShare01)}");
                builder.Append(" | ");
                builder.Append(EscapeTable(result.PressureWindowReadout));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Frontline Clash Cost");
            builder.AppendLine("| Policy | Enemy clashes | Enemy body hits | Enemy summon hits | Enemy clash dmg | Enemy engaged max | Ally clashes | Ally summon hits | Ally engaged max |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.EnemyFrontlineClashes);
                builder.Append(" | ");
                builder.Append(result.EnemyFrontlineBodyHits);
                builder.Append(" | ");
                builder.Append(result.EnemyFrontlineSummonHits);
                builder.Append(" | ");
                builder.Append(result.EnemyFrontlineClashDamage.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.MaxEnemyFrontlineEngagedCount);
                builder.Append(" | ");
                builder.Append(result.AllyFrontlineClashes);
                builder.Append(" | ");
                builder.Append(result.AllyFrontlineSummonHits);
                builder.Append(" | ");
                builder.Append(result.MaxAllyFrontlineEngagedCount);
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Hit Reaction Presentation");
            builder.AppendLine("| Policy | Enemy flash | Enemy full-body | Enemy suppressed | Enemy non-lock/lock | Ally flash | Ally full-body | Ally suppressed | Ally non-lock/lock |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.EnemySummonDamageFlashes);
                builder.Append(" | ");
                builder.Append(result.EnemySummonFullBodyHitReactions);
                builder.Append(" | ");
                builder.Append(result.EnemySummonSuppressedHitReactions);
                builder.Append(" | ");
                builder.Append($"{result.EnemySummonNonLockingDamageCues}/{result.EnemySummonLockingDamageCues}");
                builder.Append(" | ");
                builder.Append(result.AllySummonDamageFlashes);
                builder.Append(" | ");
                builder.Append(result.AllySummonFullBodyHitReactions);
                builder.Append(" | ");
                builder.Append(result.AllySummonSuppressedHitReactions);
                builder.Append(" | ");
                builder.Append($"{result.AllySummonNonLockingDamageCues}/{result.AllySummonLockingDamageCues}");
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Damage Response Policy");
            builder.AppendLine("| Policy | Player non-lock/lock/full | Boss non-lock/lock/full | Close non-lock/lock/full |");
            builder.AppendLine("|---|---:|---:|---:|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append($"{result.PlayerNonLockingDamageEvents}/{result.PlayerLockingDamageEvents}/{result.PlayerFullBodyEligibleDamageEvents}");
                builder.Append(" | ");
                builder.Append($"{result.BossNonLockingDamageEvents}/{result.BossLockingDamageEvents}/{result.BossFullBodyEligibleDamageEvents}");
                builder.Append(" | ");
                builder.Append($"{result.CloseThreatNonLockingDamageEvents}/{result.CloseThreatLockingDamageEvents}/{result.CloseThreatFullBodyEligibleDamageEvents}");
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Follow-up Presentation Bridge");
            builder.AppendLine("| Policy | Screen window/hit/miss | Camera window/hit/miss | VFX window/hit/miss | Hit tier cam/vfx | Hit dmg cam/vfx | Last follow-up screen |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowScreenCueRequests}/{result.FollowupHitScreenCueRequests}/{result.FollowupMissedScreenCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowCameraCueRequests}/{result.FollowupHitCameraCueRequests}/{result.FollowupMissedCameraCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowVfxCueRequests}/{result.FollowupHitVfxCueRequests}/{result.FollowupMissedVfxCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.LastFollowupHitCameraTier}/{result.LastFollowupHitVfxTier}");
                builder.Append(" | ");
                builder.Append($"{result.LastFollowupHitCameraDamage:0.0}/{result.LastFollowupHitVfxDamage:0.0}");
                builder.Append(" | ");
                builder.Append(EscapeTable(
                    $"{result.LastFollowupScreenCueId} "
                    + $"x{result.LastFollowupScreenCueIntensity:0.00} "
                    + $"hitx{result.LastFollowupHitScreenCueIntensity:0.00} "
                    + $"routex{result.LastFollowupWindowRouteScale:0.00}"));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Follow-up Cinematic Bridge");
            builder.AppendLine("| Policy | Director window/hit/miss | Sequence window/hit/miss | Hit frame overlays | Hit tier director/sequence | Hit cue/profile |");
            builder.AppendLine("|---|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowCinematicCueRequests}/{result.FollowupHitCinematicCueRequests}/{result.FollowupMissedCinematicCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowSequenceBridgeRequests}/{result.FollowupHitSequenceBridgeRequests}/{result.FollowupMissedSequenceBridgeRequests}");
                builder.Append(" | ");
                builder.Append(result.FollowupHitCinematicFrameOverlayCount);
                builder.Append(" | ");
                builder.Append($"{result.LastFollowupHitCinematicTier}/{result.LastFollowupHitSequenceTier}");
                builder.Append(" | ");
                builder.Append(EscapeTable(
                    $"{result.LastFollowupHitCinematicCueId}/{result.LastFollowupHitSequenceProfile}"));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Boss Pressure Screen");
            builder.AppendLine("| Policy | Boss releases | Boss screen blocks | Skill1 blocked | Max screens | Remaining blocks | Boss blocked follow-up |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.BossPressureSummonReleases);
                builder.Append(" | ");
                builder.Append(result.BossPressureScreenBlocks);
                builder.Append(" | ");
                builder.Append(result.SkillProjectilesBlockedByBossScreen);
                builder.Append(" | ");
                builder.Append(result.MaxBossPressureActiveScreenCount);
                builder.Append(" | ");
                builder.Append(result.BossPressureActiveScreenRemainingIntercepts);
                builder.Append(" | ");
                builder.Append(result.BossBlockedSkill1Followup ? "yes" : "no");
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Read");
            PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
            PolicyMetrics delayedIntended = RequireResult(results, PolicyKind.IntendedDelayedFollowup);
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics backlineEnergy = RequireResult(results, PolicyKind.BacklineEnergyProbe);
            PolicyMetrics forwardRiskEnergy = RequireResult(results, PolicyKind.ForwardRiskEnergyProbe);
            PolicyMetrics backlineBarrage = RequireResult(results, PolicyKind.BacklineBarrageProbe);
            PolicyMetrics forwardRiskBarrage = RequireResult(results, PolicyKind.ForwardRiskBarrageProbe);
            PolicyMetrics backlinePhysicalBarrage = RequireResult(
                results,
                PolicyKind.BacklinePhysicalBarrageProbe);
            PolicyMetrics forwardRiskPhysicalBarrage = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalBarrageProbe);
            PolicyMetrics forwardRiskPhysicalSummonBlock = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonBlockProbe);
            PolicyMetrics forwardRiskPhysicalSummonPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics late = RequireResult(results, PolicyKind.LateSummon);
            PolicyMetrics counterRecovery = RequireResult(results, PolicyKind.MissedFollowupCounterRecovery);
            PolicyMetrics blockedFollowup = RequireResult(results, PolicyKind.BossScreenBlockedFollowup);
            PolicyMetrics ignoredRecovery = RequireResult(results, PolicyKind.BossScreenIgnoredNoRecovery);
            PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
            PolicyMetrics delayedBlockedRecovery = RequireResult(results, PolicyKind.BossScreenDelayedCounterRecovery);
            builder.AppendLine($"- Intended route prevented {Mathf.Max(0f, noSummon.PlayerDamageTaken - intended.PlayerDamageTaken):0.0} player damage versus no-action pressure.");
            builder.AppendLine($"- Gun-only dealt {gunOnly.BossDamageTaken:0.0} boss damage but ended as `{gunOnly.ResultKind}` because the route contract still needs summon pressure blocking.");
            builder.AppendLine($"- Long survival limit: no-summon player down {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)} / boss down {FormatSeconds(noSummonSurvival.FirstBossDownAtSeconds)}; gun-only player down {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)} / boss down {FormatSeconds(gunOnlySurvival.FirstBossDownAtSeconds)}.");
            builder.AppendLine($"- Skill1 punish split: gun-only boss damage {gunOnly.BossDamageTaken:0.0}, intended follow-up boss damage {intended.BossDamageTaken:0.0}.");
            builder.AppendLine($"- Late summon ended as `{late.ResultKind}` with {late.PlayerDamageTaken:0.0} damage taken, so the report can compare timing quality without changing the scene.");
            builder.AppendLine($"- Intended route currently reads as `{ResolveRouteShape(intended)}`: follow-up window {FormatSeconds(intended.FirstFollowupWindowAtSeconds)}, counter {FormatSeconds(intended.FirstCounterWaveAtSeconds)}, Skill1 hit {FormatSeconds(intended.FirstFollowupHitAtSeconds)}.");
            builder.AppendLine($"- Lock/unlock cadence: intended block->window {FormatSeconds(intended.BlockToFollowupWindowSeconds)}, window->hit {FormatSeconds(intended.FollowupWindowToHitSeconds)}; boss-screen recovery answer pulse {blockedRecovery.CounterWaveAnswerEnergyPulse:0}, counter->answer {FormatSeconds(blockedRecovery.CounterTriggerToAnswerSeconds)}, answer->stable {FormatSeconds(blockedRecovery.CounterAnswerToStableSeconds)}, stable->final {FormatSeconds(blockedRecovery.CounterStableToFinalWindowSeconds)}, final->hit {FormatSeconds(blockedRecovery.FinalWindowToHitSeconds)}.");
            builder.AppendLine($"- Punish window tolerance: delayed clean hit after {FormatSeconds(delayedIntended.FollowupHitWindowDelaySeconds)} with {FormatSeconds(delayedIntended.FollowupWindowRemainingAtFirstHitSeconds)} remaining; delayed boss-screen recovery hit after {FormatSeconds(delayedBlockedRecovery.FollowupHitWindowDelaySeconds)} with {FormatSeconds(delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds)} remaining.");
            builder.AppendLine($"- Forward-risk EN split: backline LV1 {FormatSeconds(backlineEnergy.EnergyTier1DurationSeconds)} at x{backlineEnergy.AverageEnergyGainMultiplier:0.00}, forward-risk LV1 {FormatSeconds(forwardRiskEnergy.EnergyTier1DurationSeconds)} at x{forwardRiskEnergy.AverageEnergyGainMultiplier:0.00}; forward route is {ResolveEnergySpeedup(backlineEnergy, forwardRiskEnergy):0.0}x faster.");
            builder.AppendLine($"- Forward-risk barrage shape: backline `{backlineBarrage.BarrageShapePatternId}` near-body {backlineBarrage.BarrageShapeNearProjectileCount}/{backlineBarrage.BarrageShapeProjectileCount}, avg lateral gap {backlineBarrage.BarrageShapeAverageLateralGap:0.00}, nearest {backlineBarrage.BarrageShapeNearestLaneDistance:0.00}, density {backlineBarrage.BarrageShapeThreatDensity:0.00}; forward near-body {forwardRiskBarrage.BarrageShapeNearProjectileCount}/{forwardRiskBarrage.BarrageShapeProjectileCount}, avg lateral gap {forwardRiskBarrage.BarrageShapeAverageLateralGap:0.00}, nearest {forwardRiskBarrage.BarrageShapeNearestLaneDistance:0.00}, density {forwardRiskBarrage.BarrageShapeThreatDensity:0.00}.");
            if (forwardRiskBarrage.BarrageShapeNearProjectileCount <= backlineBarrage.BarrageShapeNearProjectileCount)
            {
                builder.AppendLine("- Forward-risk barrage compression is measurable, but near-body projectile count did not rise; direct position-specific hit danger remains a follow-up gap.");
            }

            builder.AppendLine($"- Route stability split: no-action {FormatPercent01(noSummon.RouteStability01)} final / {FormatPercent01(noSummon.MinRouteStability01)} min, gun-only {FormatPercent01(gunOnly.RouteStability01)} / {FormatPercent01(gunOnly.MinRouteStability01)}, intended {FormatPercent01(intended.RouteStability01)} / {FormatPercent01(intended.MinRouteStability01)}.");
            builder.AppendLine($"- Forward-risk physical barrage: backline hits {backlinePhysicalBarrage.PhysicalBarragePlayerHits}/{backlinePhysicalBarrage.PhysicalBarrageTrackedProjectileCount}, damage {backlinePhysicalBarrage.PhysicalBarragePlayerDamage:0.0}; forward hits {forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}/{forwardRiskPhysicalBarrage.PhysicalBarrageTrackedProjectileCount}, damage {forwardRiskPhysicalBarrage.PhysicalBarragePlayerDamage:0.0}.");
            builder.AppendLine($"- Forward-risk physical summon block: blocks {forwardRiskPhysicalSummonBlock.SummonBlocks}, player hits {forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerHits}/{forwardRiskPhysicalSummonBlock.PhysicalBarrageTrackedProjectileCount}, damage {forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerDamage:0.0}, block->window {FormatSeconds(forwardRiskPhysicalSummonBlock.BlockToFollowupWindowSeconds)}.");
            builder.AppendLine($"- Forward-risk physical summon punish: `{forwardRiskPhysicalSummonPunish.ResultKind}` with blocks {forwardRiskPhysicalSummonPunish.SummonBlocks}, player hits {forwardRiskPhysicalSummonPunish.PhysicalBarragePlayerHits}/{forwardRiskPhysicalSummonPunish.PhysicalBarrageTrackedProjectileCount}, Skill1 hits {forwardRiskPhysicalSummonPunish.SkillProjectileHits}, boss damage {forwardRiskPhysicalSummonPunish.BossDamageTaken:0.0}, window->hit {FormatSeconds(forwardRiskPhysicalSummonPunish.FollowupWindowToHitSeconds)}.");
            builder.AppendLine($"- Unanswered hit penalty split: no-action {FormatPercent01(noSummon.TotalUnansweredBossHitRoutePenalty01)} x{noSummon.UnansweredBossHitRoutePenaltyCount}, gun-only {FormatPercent01(gunOnly.TotalUnansweredBossHitRoutePenalty01)} x{gunOnly.UnansweredBossHitRoutePenaltyCount}, late {FormatPercent01(late.TotalUnansweredBossHitRoutePenalty01)} x{late.UnansweredBossHitRoutePenaltyCount}.");
            builder.AppendLine($"- Frontline exposure split: no-action enemy-only {FormatSeconds(noSummon.EnemyOnlyFrontlineSeconds)}, gun-only enemy-only {FormatSeconds(gunOnly.EnemyOnlyFrontlineSeconds)}, intended ally-only {FormatSeconds(intended.AllyOnlyFrontlineSeconds)} / contested {FormatSeconds(intended.ContestedFrontlineSeconds)}.");
            builder.AppendLine($"- ArkData effective pressure shape: no-action peak/top3 {FormatPercent01(noSummon.PeakPressureWindowShare01)}/{FormatPercent01(noSummon.Top3PressureWindowShare01)}, intended {FormatPercent01(intended.PeakPressureWindowShare01)}/{FormatPercent01(intended.Top3PressureWindowShare01)} with relief {FormatSeconds(intended.TimeToNextReliefWindowSeconds)}, ignored boss-screen unanswered burden {FormatPercent01(ignoredRecovery.UnansweredPressureBurdenShare01)} versus intended {FormatPercent01(intended.UnansweredPressureBurdenShare01)}.");
            builder.AppendLine($"- Enemy pressure actor cost: no-action clashes {noSummon.EnemyFrontlineClashes} / body hits {noSummon.EnemyFrontlineBodyHits} / clash damage {noSummon.EnemyFrontlineClashDamage:0.0}; intended route clashes {intended.EnemyFrontlineClashes} / body hits {intended.EnemyFrontlineBodyHits}.");
            builder.AppendLine($"- Hit reaction split: boss-screen recovery produced {blockedRecovery.TotalSummonDamageFlashes} summon damage flashes, {blockedRecovery.TotalSummonFullBodyHitReactions} full-body hit reactions, and {blockedRecovery.TotalNonLockingSummonDamageCues} non-locking damage cues.");
            builder.AppendLine($"- Damage response split: gun-only boss chip {gunOnly.BossNonLockingDamageEvents}/{gunOnly.BossLockingDamageEvents} non-lock/lock, intended Skill1 boss hits {intended.BossNonLockingDamageEvents}/{intended.BossLockingDamageEvents}, boss-screen recovery {blockedRecovery.BossNonLockingDamageEvents}/{blockedRecovery.BossLockingDamageEvents}.");
            builder.AppendLine($"- Follow-up presentation bridge: gun-only hit cues screen/camera/VFX {gunOnly.FollowupHitScreenCueRequests}/{gunOnly.FollowupHitCameraCueRequests}/{gunOnly.FollowupHitVfxCueRequests}, intended {intended.FollowupHitScreenCueRequests}/{intended.FollowupHitCameraCueRequests}/{intended.FollowupHitVfxCueRequests}, boss-screen recovery {blockedRecovery.FollowupHitScreenCueRequests}/{blockedRecovery.FollowupHitCameraCueRequests}/{blockedRecovery.FollowupHitVfxCueRequests}.");
            builder.AppendLine($"- Follow-up cinematic bridge is currently disabled by canonical scene setup: gun-only hit director/sequence {gunOnly.FollowupHitCinematicCueRequests}/{gunOnly.FollowupHitSequenceBridgeRequests}, intended {intended.FollowupHitCinematicCueRequests}/{intended.FollowupHitSequenceBridgeRequests}, boss-screen recovery {blockedRecovery.FollowupHitCinematicCueRequests}/{blockedRecovery.FollowupHitSequenceBridgeRequests}; intended frame overlays {intended.FollowupHitCinematicFrameOverlayCount}.");
            builder.AppendLine($"- Blocked follow-up presentation: boss-screen blocked route has miss cues screen/camera/VFX {blockedFollowup.FollowupMissedScreenCueRequests}/{blockedFollowup.FollowupMissedCameraCueRequests}/{blockedFollowup.FollowupMissedVfxCueRequests} and hit cues {blockedFollowup.FollowupHitScreenCueRequests}/{blockedFollowup.FollowupHitCameraCueRequests}/{blockedFollowup.FollowupHitVfxCueRequests}.");
            builder.AppendLine($"- Blocked follow-up cinematic: boss-screen blocked route has miss director/sequence {blockedFollowup.FollowupMissedCinematicCueRequests}/{blockedFollowup.FollowupMissedSequenceBridgeRequests} and hit director/sequence {blockedFollowup.FollowupHitCinematicCueRequests}/{blockedFollowup.FollowupHitSequenceBridgeRequests}.");
            builder.AppendLine($"- Missed follow-up branch: `{counterRecovery.ResultKind}` with counter source `{counterRecovery.CounterWaveSource}`, final window `{counterRecovery.CounterWaveFinalWindowState}`, and Skill1 hits {counterRecovery.SkillProjectileHits}.");
            builder.AppendLine($"- Boss-screen branch: boss releases {blockedFollowup.BossPressureSummonReleases}, blocks {blockedFollowup.BossPressureScreenBlocks}, Skill1 projectiles blocked {blockedFollowup.SkillProjectilesBlockedByBossScreen}, boss-blocked follow-up `{blockedFollowup.BossBlockedSkill1Followup}`.");
            builder.AppendLine($"- Boss-screen ignored branch: `{ignoredRecovery.ResultKind}` for {FormatSeconds(ignoredRecovery.ElapsedSeconds)} with enemy clashes {ignoredRecovery.EnemyFrontlineClashes}, body hits {ignoredRecovery.EnemyFrontlineBodyHits}, and player damage {ignoredRecovery.PlayerDamageTaken:0.0}.");
            builder.AppendLine($"- Boss-screen recovery branch: `{blockedRecovery.ResultKind}` keeps source `{blockedRecovery.CounterWaveSource}`, opens final window `{blockedRecovery.CounterWaveFinalWindowState}`, and lands {blockedRecovery.SkillProjectileHits} Skill1 hits after a fresh summon answer.");
            builder.AppendLine($"- Counter payoff split: clean follow-up {intended.BossDamageTaken:0.0} boss damage versus boss-screen recovery {blockedRecovery.BossDamageTaken:0.0} at final-window scale x{blockedRecovery.CounterWaveFinalWindowRouteScale:0.00}.");
            int maxEnemyFrontlines = ResolveMaxEnemyFrontlines(results);
            builder.AppendLine(ignoredRecovery.EnemyFrontlineBodyHits > 0
                ? $"- Enemy frontline body cost is active: ignored boss-screen pressure produced {ignoredRecovery.EnemyFrontlineBodyHits} body hits while the recovered branch converts the same pressure into summon clashes."
                : $"- Enemy frontline presence is measured (max enemy frontlines {maxEnemyFrontlines}), but ignored boss-screen pressure still produced 0 body hits; this remains an axis-4 combat-grammar gap.");
            builder.AppendLine();
            builder.AppendLine("## Notes");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("- ");
                builder.Append(result.Policy);
                builder.Append(": ");
                builder.AppendLine(result.Notes.Count == 0 ? "no runner notes" : string.Join("; ", result.Notes));
            }

            return builder.ToString();
        }

        private static void AppendStructuralGateSummary(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics forwardRiskPhysicalBarrage = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalBarrageProbe);
            PolicyMetrics forwardRiskPhysicalSummonPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
            PolicyMetrics ignoredRecovery = RequireResult(results, PolicyKind.BossScreenIgnoredNoRecovery);
            PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);

            bool axis1Pass = noSummonSurvival.ResultKind == "PlayerDownFail"
                && gunOnlySurvival.ResultKind == "PlayerDownFail"
                && noSummonSurvival.FirstPlayerDownAtSeconds >= 0f
                && gunOnlySurvival.FirstPlayerDownAtSeconds >= 0f
                && gunOnlySurvival.FirstBossDownAtSeconds < 0f;
            bool axis2Pass = forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > 0
                && forwardRiskPhysicalSummonPunish.SummonBlocks > 0
                && forwardRiskPhysicalSummonPunish.PhysicalBarragePlayerHits == 0
                && forwardRiskPhysicalSummonPunish.ResultKind == "CleanFollowupClear"
                && forwardRiskPhysicalSummonPunish.SkillProjectileHits > 0;
            bool axis3Pass = noSummon.PlayerNonLockingDamageEvents > 0
                && noSummon.PlayerLockingDamageEvents == 0
                && gunOnly.BossNonLockingDamageEvents > 0
                && gunOnly.BossLockingDamageEvents == 0
                && forwardRiskPhysicalSummonPunish.BossLockingDamageEvents > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests > 0;
            bool axis4Pass = noSummon.EnemyFrontlineBodyHits > 0
                && ignoredRecovery.EnemyFrontlineBodyHits > blockedRecovery.EnemyFrontlineBodyHits
                && ignoredRecovery.PlayerDamageTaken > blockedRecovery.PlayerDamageTaken;

            builder.AppendLine("## Structural Gate Summary");
            builder.AppendLine("| Axis | Status | Evidence |");
            builder.AppendLine("|---|---|---|");
            builder.AppendLine(
                $"| 1. Bad routes lose state/HP | {FormatGateStatus(axis1Pass)} | no-summon down {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)}, gun-only down {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)}, gun-only boss down {FormatSeconds(gunOnlySurvival.FirstBossDownAtSeconds)} |");
            builder.AppendLine(
                $"| 2. Block -> window -> Skill1 loop | {FormatGateStatus(axis2Pass)} | unblocked forward hits {forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}/{forwardRiskPhysicalBarrage.PhysicalBarrageTrackedProjectileCount}; physical punish blocks {forwardRiskPhysicalSummonPunish.SummonBlocks}, player hits {forwardRiskPhysicalSummonPunish.PhysicalBarragePlayerHits}/{forwardRiskPhysicalSummonPunish.PhysicalBarrageTrackedProjectileCount}, Skill1 hits {forwardRiskPhysicalSummonPunish.SkillProjectileHits}, `{forwardRiskPhysicalSummonPunish.ResultKind}` |");
            builder.AppendLine(
                $"| 3. Hit response and presentation | {FormatGateStatus(axis3Pass)} | player routine hits {noSummon.PlayerNonLockingDamageEvents}/{noSummon.PlayerLockingDamageEvents} non-lock/lock; gun boss chip {gunOnly.BossNonLockingDamageEvents}/{gunOnly.BossLockingDamageEvents}; physical punish boss lock {forwardRiskPhysicalSummonPunish.BossLockingDamageEvents}, hit cues {forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests} |");
            builder.AppendLine(
                $"| 4. Enemy pressure actor cost | {FormatGateStatus(axis4Pass)} | no-action body hits {noSummon.EnemyFrontlineBodyHits}; ignored boss-screen body hits {ignoredRecovery.EnemyFrontlineBodyHits}, damage {ignoredRecovery.PlayerDamageTaken:0.0}; recovery body hits {blockedRecovery.EnemyFrontlineBodyHits}, damage {blockedRecovery.PlayerDamageTaken:0.0} |");
            builder.AppendLine(
                $"| Physical clean route reference | {FormatGateStatus(forwardRiskPhysicalSummonPunish.IsClearResult)} | physical summon-punish clears in {FormatSeconds(forwardRiskPhysicalSummonPunish.ElapsedSeconds)} with {forwardRiskPhysicalSummonPunish.PlayerDamageTaken:0.0} HP lost versus intended route {FormatSeconds(intended.ElapsedSeconds)} / {intended.PlayerDamageTaken:0.0} HP lost |");
        }

        private static void AppendArkDataCoverageSummary(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics backlineEnergy = RequireResult(results, PolicyKind.BacklineEnergyProbe);
            PolicyMetrics forwardRiskEnergy = RequireResult(results, PolicyKind.ForwardRiskEnergyProbe);
            PolicyMetrics backlinePhysicalBarrage = RequireResult(
                results,
                PolicyKind.BacklinePhysicalBarrageProbe);
            PolicyMetrics forwardRiskPhysicalBarrage = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalBarrageProbe);
            PolicyMetrics forwardRiskPhysicalSummonBlock = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonBlockProbe);
            PolicyMetrics forwardRiskPhysicalSummonPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
            PolicyMetrics delayedIntended = RequireResult(results, PolicyKind.IntendedDelayedFollowup);
            PolicyMetrics ignoredRecovery = RequireResult(results, PolicyKind.BossScreenIgnoredNoRecovery);
            PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
            PolicyMetrics delayedBlockedRecovery = RequireResult(
                results,
                PolicyKind.BossScreenDelayedCounterRecovery);

            bool stageResultMeasured = noSummonSurvival.ResultKind == "PlayerDownFail"
                && gunOnlySurvival.ResultKind == "PlayerDownFail"
                && forwardRiskPhysicalSummonPunish.IsClearResult
                && forwardRiskPhysicalSummonPunish.ResultRecords > 0;
            bool pressureSlotMeasured = forwardRiskEnergy.EnergyTier1DurationSeconds >= 0f
                && backlineEnergy.EnergyTier1DurationSeconds >= 0f
                && forwardRiskEnergy.EnergyTier1DurationSeconds < backlineEnergy.EnergyTier1DurationSeconds
                && forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > backlinePhysicalBarrage.PhysicalBarragePlayerHits
                && ignoredRecovery.UnansweredPressureBurdenShare01 > intended.UnansweredPressureBurdenShare01;
            bool combatPayloadMeasured = forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > 0
                && forwardRiskPhysicalSummonBlock.SummonBlocks > 0
                && forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerHits == 0
                && forwardRiskPhysicalSummonPunish.SkillProjectileHits > 0
                && forwardRiskPhysicalSummonPunish.BossLockingDamageEvents > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests > 0;
            bool pgrStateMeasured = intended.BlockToFollowupWindowSeconds >= 0f
                && intended.BlockToFollowupWindowSeconds <= 0.35f
                && blockedRecovery.CounterWaveAnswerEnergyPulse > 0f
                && blockedRecovery.CounterTriggerToAnswerSeconds >= 0f
                && blockedRecovery.CounterTriggerToAnswerSeconds <= 0.35f
                && delayedIntended.FollowupWindowRemainingAtFirstHitSeconds > 0f
                && delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds > 0f
                && noSummon.PlayerLockingDamageEvents == 0
                && gunOnly.BossLockingDamageEvents == 0
                && forwardRiskPhysicalSummonPunish.BossLockingDamageEvents > 0;
            bool v1ScopeHeld = forwardRiskPhysicalSummonPunish.FollowupHitCinematicCueRequests == 0
                && forwardRiskPhysicalSummonPunish.FollowupHitSequenceBridgeRequests == 0;

            builder.AppendLine("## ArkData Coverage Summary");
            builder.AppendLine("| Reference lens | Status | Current evidence | Boundary kept |");
            builder.AppendLine("|---|---|---|---|");
            builder.AppendLine(
                "| NIKKE stage-result runtime | "
                + $"{FormatCoverageStatus(stageResultMeasured, "PARTIAL")} | "
                + $"bad routes reach HP fail at {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)} / {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)}; clean physical route commits {forwardRiskPhysicalSummonPunish.ResultRecords} result record; reward hook `{EscapeTable(ResolveCoverageValue(forwardRiskPhysicalSummonPunish.ResultRecordRewardHook))}` | "
                + "Reward/item persistence and campaign clear are intentionally not implemented in this V1 combat slice. |");
            builder.AppendLine(
                "| Stage pressure-slot discipline | "
                + $"{FormatCoverageStatus(pressureSlotMeasured)} | "
                + $"forward-risk LV1 {FormatSeconds(forwardRiskEnergy.EnergyTier1DurationSeconds)} vs backline {FormatSeconds(backlineEnergy.EnergyTier1DurationSeconds)}; physical barrage hits {backlinePhysicalBarrage.PhysicalBarragePlayerHits}->{forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}; ignored burden {FormatPercent01(ignoredRecovery.UnansweredPressureBurdenShare01)} vs intended {FormatPercent01(intended.UnansweredPressureBurdenShare01)} | "
                + "No new wave manager or generated stage; all policies use the same authored scene/profile pocket. |");
            builder.AppendLine(
                "| CombatPayload runtime pipeline | "
                + $"{FormatCoverageStatus(combatPayloadMeasured)} | "
                + $"Target->Hit: forward barrage {forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}/{forwardRiskPhysicalBarrage.PhysicalBarrageTrackedProjectileCount}; Block->Status: {forwardRiskPhysicalSummonBlock.SummonBlocks} blocks and {FormatSeconds(forwardRiskPhysicalSummonBlock.BlockToFollowupWindowSeconds)} to window; Skill1 Hit->Presentation: {forwardRiskPhysicalSummonPunish.SkillProjectileHits} hits with cues {forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests} | "
                + "Candidate labels stay local test evidence, not fake universal opcodes. |");
            builder.AppendLine(
                "| PGR state-lock and hit-response grammar | "
                + $"{FormatCoverageStatus(pgrStateMeasured)} | "
                + $"block->window {FormatSeconds(intended.BlockToFollowupWindowSeconds)}; counter answer pulse {blockedRecovery.CounterWaveAnswerEnergyPulse:0}; delayed clean/recovery margins {FormatSeconds(delayedIntended.FollowupWindowRemainingAtFirstHitSeconds)} / {FormatSeconds(delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds)}; routine lock counts {noSummon.PlayerLockingDamageEvents}/{gunOnly.BossLockingDamageEvents}, punish boss locks {forwardRiskPhysicalSummonPunish.BossLockingDamageEvents} | "
                + "Use lock/unlock and response tiers only; do not import tutorial HUD flow as the solution. |");
            builder.AppendLine(
                "| V1 scope guardrail | "
                + $"{FormatCoverageStatus(v1ScopeHeld)} | "
                + $"report scene `{ScenePath}`; cinematic director/sequence hit counts {forwardRiskPhysicalSummonPunish.FollowupHitCinematicCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitSequenceBridgeRequests}; physical clean route still clears `{forwardRiskPhysicalSummonPunish.ResultKind}` | "
                + "No new canonical scene, broad VFX/audio restoration, roster, reward economy, or stage-select work. |");
        }

        private static string FormatGateStatus(bool passed)
        {
            return passed ? "PASS" : "REVIEW";
        }

        private static string FormatCoverageStatus(bool passed, string passLabel = "PASS")
        {
            return passed ? passLabel : "REVIEW";
        }

        private static string ResolveCoverageValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "empty" : value;
        }

        private static string ResolveRouteShape(PolicyMetrics result)
        {
            if (result.ResultKind == "CleanFollowupClear" || result.CleanFollowupConfirmed)
            {
                return "clean_followup";
            }

            if (result.ResultKind == "CounterRecoveryClear" || result.CounterRecoveryConfirmed)
            {
                return "counter_recovery";
            }

            if (result.CounterWaves > 0)
            {
                return "counter_pending";
            }

            return "route_pending";
        }

        private static string ResolveFollowupTiming(PolicyMetrics result)
        {
            return $"open:{FormatSeconds(result.FirstFollowupWindowAtSeconds)} "
                + $"hit:{FormatSeconds(result.FirstFollowupHitAtSeconds)} "
                + $"miss:{FormatSeconds(result.FirstFollowupMissAtSeconds)} "
                + $"windows:{result.FollowupWindowOpenCount} hits:{result.FollowupHitCount} "
                + $"tier:{result.HighestFollowupHitTier} "
                + $"dmg:{Mathf.Max(result.FollowupHitDamage, result.Skill1FollowupDamage):0.0}";
        }

        private static string ResolveCounterTiming(PolicyMetrics result)
        {
            return $"state:{result.CounterWaveRecordState}({result.CounterWaveSource}) "
                + $"seen:{FormatSeconds(result.FirstCounterWaveAtSeconds)} "
                + $"stable:{FormatSeconds(result.FirstCounterStabilizedAtSeconds)} "
                + $"hold:{result.CounterWaveAllyHoldElapsedSeconds:0.0}/{result.CounterWaveAllyHoldRequiredSeconds:0.0}s "
                + $"penalty:{FormatPercent01(result.CounterWaveEntryPenalty01)} "
                + $"bonus:{FormatPercent01(result.CounterWaveStabilityBonus01)}";
        }

        private static string ResolveResultRecordReadout(PolicyMetrics result)
        {
            if (result.ResultRecords <= 0)
            {
                return "pending";
            }

            return $"{result.ResultRecordRouteLabel} "
                + $"{FormatPercent01(result.ResultRecordRouteStability01)} "
                + $"{result.ResultRecordDecision}";
        }

        private static string FormatSeconds(float seconds)
        {
            return seconds >= 0f ? $"{seconds:0.0}s" : "-";
        }

        private static string FormatPercent01(float value)
        {
            return $"{Mathf.Clamp01(value) * 100f:0}%";
        }

        private static string FormatOptionalPercent01(float value)
        {
            return value >= 0f ? FormatPercent01(value) : "-";
        }

        private static string FormatOptionalDistance(float value)
        {
            return value >= 0f ? value.ToString("0.00") : "-";
        }

        private static float ResolveAverage(float total, float elapsedSeconds)
        {
            return elapsedSeconds > 0f ? total / elapsedSeconds : 0f;
        }

        private static float ResolveEnergySpeedup(PolicyMetrics slow, PolicyMetrics fast)
        {
            return slow.EnergyTier1DurationSeconds > 0f && fast.EnergyTier1DurationSeconds > 0f
                ? slow.EnergyTier1DurationSeconds / fast.EnergyTier1DurationSeconds
                : 0f;
        }

        private static string BuildJson(IReadOnlyList<PolicyMetrics> results)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine($"  \"scene\": \"{ScenePath}\",");
            builder.AppendLine("  \"policies\": [");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.AppendLine("    {");
                builder.AppendLine($"      \"policy\": \"{result.Policy}\",");
                builder.AppendLine($"      \"resultKind\": \"{JsonEscape(result.ResultKind)}\",");
                builder.AppendLine($"      \"elapsedSeconds\": {result.ElapsedSeconds:0.###},");
                builder.AppendLine($"      \"playerHealthRemaining\": {result.PlayerHealthRemaining:0.###},");
                builder.AppendLine($"      \"playerDamageTaken\": {result.PlayerDamageTaken:0.###},");
                builder.AppendLine($"      \"firstPlayerDownAtSeconds\": {JsonNullableSeconds(result.FirstPlayerDownAtSeconds)},");
                builder.AppendLine($"      \"bossDamageTaken\": {result.BossDamageTaken:0.###},");
                builder.AppendLine($"      \"firstBossDownAtSeconds\": {JsonNullableSeconds(result.FirstBossDownAtSeconds)},");
                builder.AppendLine($"      \"survivalProbeMaxSeconds\": {JsonNullableSeconds(result.SurvivalProbeMaxSeconds)},");
                builder.AppendLine($"      \"closeThreatBasicHits\": {result.CloseThreatBasicHits},");
                builder.AppendLine($"      \"bossBasicHits\": {result.BossBasicHits},");
                builder.AppendLine($"      \"bossWaves\": {result.BossWaves},");
                builder.AppendLine($"      \"bossProjectilesSpawned\": {result.BossProjectilesSpawned},");
                builder.AppendLine($"      \"bossProjectilesHitPlayer\": {result.BossProjectilesHitPlayer},");
                builder.AppendLine($"      \"summonUses\": {result.SummonUses},");
                builder.AppendLine($"      \"summonBlocks\": {result.SummonBlocks},");
                builder.AppendLine($"      \"skillUses\": {result.SkillUses},");
                builder.AppendLine($"      \"skillProjectileHits\": {result.SkillProjectileHits},");
                builder.AppendLine($"      \"skillProjectilesBlockedByBossScreen\": {result.SkillProjectilesBlockedByBossScreen},");
                builder.AppendLine($"      \"bossPressureSummonReleases\": {result.BossPressureSummonReleases},");
                builder.AppendLine($"      \"bossPressureScreenBlocks\": {result.BossPressureScreenBlocks},");
                builder.AppendLine($"      \"maxBossPressureActiveScreenCount\": {result.MaxBossPressureActiveScreenCount},");
                builder.AppendLine($"      \"bossPressureActiveScreenRemainingIntercepts\": {result.BossPressureActiveScreenRemainingIntercepts},");
                builder.AppendLine($"      \"firstSummonUseAtSeconds\": {JsonNullableSeconds(result.FirstSummonUseAtSeconds)},");
                builder.AppendLine($"      \"firstSummonBlockAtSeconds\": {JsonNullableSeconds(result.FirstSummonBlockAtSeconds)},");
                builder.AppendLine($"      \"firstBossPressureReleaseAtSeconds\": {JsonNullableSeconds(result.FirstBossPressureReleaseAtSeconds)},");
                builder.AppendLine($"      \"firstBossPressureScreenBlockAtSeconds\": {JsonNullableSeconds(result.FirstBossPressureScreenBlockAtSeconds)},");
                builder.AppendLine($"      \"firstSkill1UseAtSeconds\": {JsonNullableSeconds(result.FirstSkill1UseAtSeconds)},");
                builder.AppendLine($"      \"firstCounterAnswerSummonAtSeconds\": {JsonNullableSeconds(result.FirstCounterAnswerSummonAtSeconds)},");
                builder.AppendLine($"      \"firstCounterFinalWindowAtSeconds\": {JsonNullableSeconds(result.FirstCounterFinalWindowAtSeconds)},");
                builder.AppendLine($"      \"summonUseToBlockSeconds\": {JsonNullableSeconds(result.SummonUseToBlockSeconds)},");
                builder.AppendLine($"      \"blockToFollowupWindowSeconds\": {JsonNullableSeconds(result.BlockToFollowupWindowSeconds)},");
                builder.AppendLine($"      \"followupWindowToSkillUseSeconds\": {JsonNullableSeconds(result.FollowupWindowToSkillUseSeconds)},");
                builder.AppendLine($"      \"followupWindowToHitSeconds\": {JsonNullableSeconds(result.FollowupWindowToHitSeconds)},");
                builder.AppendLine($"      \"blockToFollowupHitSeconds\": {JsonNullableSeconds(result.BlockToFollowupHitSeconds)},");
                builder.AppendLine($"      \"bossScreenReleaseToBlockSeconds\": {JsonNullableSeconds(result.BossScreenReleaseToBlockSeconds)},");
                builder.AppendLine($"      \"counterTriggerToAnswerSeconds\": {JsonNullableSeconds(result.CounterTriggerToAnswerSeconds)},");
                builder.AppendLine($"      \"counterAnswerToStableSeconds\": {JsonNullableSeconds(result.CounterAnswerToStableSeconds)},");
                builder.AppendLine($"      \"counterStableToFinalWindowSeconds\": {JsonNullableSeconds(result.CounterStableToFinalWindowSeconds)},");
                builder.AppendLine($"      \"finalWindowToHitSeconds\": {JsonNullableSeconds(result.FinalWindowToHitSeconds)},");
                builder.AppendLine($"      \"firstFollowupWindowDurationSeconds\": {JsonNullableSeconds(result.FirstFollowupWindowDurationSeconds)},");
                builder.AppendLine($"      \"lastFollowupWindowAtSeconds\": {JsonNullableSeconds(result.LastFollowupWindowAtSeconds)},");
                builder.AppendLine($"      \"lastFollowupWindowDurationSeconds\": {JsonNullableSeconds(result.LastFollowupWindowDurationSeconds)},");
                builder.AppendLine($"      \"followupWindowAtFirstHitSeconds\": {JsonNullableSeconds(result.FollowupWindowAtFirstHitSeconds)},");
                builder.AppendLine($"      \"followupWindowDurationAtFirstHitSeconds\": {JsonNullableSeconds(result.FollowupWindowDurationAtFirstHitSeconds)},");
                builder.AppendLine($"      \"followupHitWindowDelaySeconds\": {JsonNullableSeconds(result.FollowupHitWindowDelaySeconds)},");
                builder.AppendLine($"      \"followupWindowRemainingAtFirstHitSeconds\": {JsonNullableSeconds(result.FollowupWindowRemainingAtFirstHitSeconds)},");
                builder.AppendLine($"      \"followupHitWindowUsedShare01\": {JsonNullableSeconds(result.FollowupHitWindowUsedShare01)},");
                builder.AppendLine($"      \"maxEnemyFrontlineCount\": {result.MaxEnemyFrontlineCount},");
                builder.AppendLine($"      \"maxAllyFrontlineCount\": {result.MaxAllyFrontlineCount},");
                builder.AppendLine($"      \"routeDrainAccumulated01\": {result.RouteDrainAccumulated01:0.###},");
                builder.AppendLine($"      \"unansweredBossHitRoutePenaltyCount\": {result.UnansweredBossHitRoutePenaltyCount},");
                builder.AppendLine($"      \"lastUnansweredBossHitRoutePenalty01\": {result.LastUnansweredBossHitRoutePenalty01:0.###},");
                builder.AppendLine($"      \"totalUnansweredBossHitRoutePenalty01\": {result.TotalUnansweredBossHitRoutePenalty01:0.###},");
                builder.AppendLine($"      \"averageRouteDrainPerSecond\": {ResolveAverage(result.RouteDrainAccumulated01, result.ElapsedSeconds):0.###},");
                builder.AppendLine($"      \"maxRouteStabilityDrainPerSecond\": {result.MaxRouteStabilityDrainPerSecond:0.###},");
                builder.AppendLine($"      \"averageRoutePressureWeight\": {ResolveAverage(result.RoutePressureWeightSeconds, result.ElapsedSeconds):0.###},");
                builder.AppendLine($"      \"averageFrontlinePresenceDrainScale\": {ResolveAverage(result.FrontlinePresenceScaleSeconds, result.ElapsedSeconds):0.###},");
                builder.AppendLine($"      \"enemyOnlyFrontlineSeconds\": {result.EnemyOnlyFrontlineSeconds:0.###},");
                builder.AppendLine($"      \"contestedFrontlineSeconds\": {result.ContestedFrontlineSeconds:0.###},");
                builder.AppendLine($"      \"allyOnlyFrontlineSeconds\": {result.AllyOnlyFrontlineSeconds:0.###},");
                builder.AppendLine($"      \"frontlinePresenceReadout\": \"{JsonEscape(result.FrontlinePresenceReadout)}\",");
                builder.AppendLine($"      \"energyProbeTargetForwardRisk01\": {JsonNullableSeconds(result.EnergyProbeTargetForwardRisk01)},");
                builder.AppendLine($"      \"energyProbeStartAtSeconds\": {JsonNullableSeconds(result.EnergyProbeStartAtSeconds)},");
                builder.AppendLine($"      \"energyTier1ReadyAtSeconds\": {JsonNullableSeconds(result.EnergyTier1ReadyAtSeconds)},");
                builder.AppendLine($"      \"energyTier1DurationSeconds\": {JsonNullableSeconds(result.EnergyTier1DurationSeconds)},");
                builder.AppendLine($"      \"energyChargingTier\": {result.EnergyChargingTier},");
                builder.AppendLine($"      \"energyAvailableTier\": {result.EnergyAvailableTier},");
                builder.AppendLine($"      \"energyFillRatio\": {result.EnergyFillRatio:0.###},");
                builder.AppendLine($"      \"averageEnergyForwardRisk01\": {result.AverageEnergyForwardRisk01:0.###},");
                builder.AppendLine($"      \"averageEnergyGainMultiplier\": {result.AverageEnergyGainMultiplier:0.###},");
                builder.AppendLine($"      \"backSafetyBandSeconds\": {result.BackSafetyBandSeconds:0.###},");
                builder.AppendLine($"      \"midChargeBandSeconds\": {result.MidChargeBandSeconds:0.###},");
                builder.AppendLine($"      \"forwardRiskBandSeconds\": {result.ForwardRiskBandSeconds:0.###},");
                builder.AppendLine($"      \"lastEnergyForwardRisk01\": {result.LastEnergyForwardRisk01:0.###},");
                builder.AppendLine($"      \"lastEnergyGainMultiplier\": {result.LastEnergyGainMultiplier:0.###},");
                builder.AppendLine($"      \"lastEnergyRiskBand\": \"{JsonEscape(result.LastEnergyRiskBand)}\",");
                builder.AppendLine($"      \"barrageShapeProbeTargetForwardRisk01\": {JsonNullableSeconds(result.BarrageShapeProbeTargetForwardRisk01)},");
                builder.AppendLine($"      \"barrageShapePendingForwardRisk01\": {JsonNullableSeconds(result.BarrageShapePendingForwardRisk01)},");
                builder.AppendLine($"      \"barrageShapePatternId\": \"{JsonEscape(result.BarrageShapePatternId)}\",");
                builder.AppendLine($"      \"barrageShapeProjectileCount\": {result.BarrageShapeProjectileCount},");
                builder.AppendLine($"      \"barrageShapeNearProjectileCount\": {result.BarrageShapeNearProjectileCount},");
                builder.AppendLine($"      \"barrageShapeNearRadius\": {BarrageShapeProbeNearRadius:0.###},");
                builder.AppendLine($"      \"barrageShapeAverageLateralGap\": {result.BarrageShapeAverageLateralGap:0.###},");
                builder.AppendLine($"      \"barrageShapeAverageDepthGap\": {result.BarrageShapeAverageDepthGap:0.###},");
                builder.AppendLine($"      \"barrageShapeAverageLaneDistance\": {result.BarrageShapeAverageLaneDistance:0.###},");
                builder.AppendLine($"      \"barrageShapeNearestLaneDistance\": {JsonNullableSeconds(result.BarrageShapeNearestLaneDistance)},");
                builder.AppendLine($"      \"barrageShapeThreatDensity\": {result.BarrageShapeThreatDensity:0.###},");
                builder.AppendLine($"      \"barrageShapeReadout\": \"{JsonEscape(result.BarrageShapeReadout)}\",");
                builder.AppendLine($"      \"physicalBarrageProbeTargetForwardRisk01\": {JsonNullableSeconds(result.PhysicalBarrageProbeTargetForwardRisk01)},");
                builder.AppendLine($"      \"physicalBarragePendingForwardRisk01\": {JsonNullableSeconds(result.PhysicalBarragePendingForwardRisk01)},");
                builder.AppendLine($"      \"physicalBarragePatternId\": \"{JsonEscape(result.PhysicalBarragePatternId)}\",");
                builder.AppendLine($"      \"physicalBarrageFlightSeconds\": {PhysicalBarrageProbeFlightSeconds:0.###},");
                builder.AppendLine($"      \"physicalBarrageWaves\": {result.PhysicalBarrageWaves},");
                builder.AppendLine($"      \"physicalBarrageProjectilesSpawned\": {result.PhysicalBarrageProjectilesSpawned},");
                builder.AppendLine($"      \"physicalBarrageTrackedProjectileCount\": {result.PhysicalBarrageTrackedProjectileCount},");
                builder.AppendLine($"      \"physicalBarrageInactiveAfterFlight\": {result.PhysicalBarrageInactiveAfterFlight},");
                builder.AppendLine($"      \"physicalBarragePlayerImpactAttempts\": {result.PhysicalBarragePlayerImpactAttempts},");
                builder.AppendLine($"      \"physicalBarragePlayerHits\": {result.PhysicalBarragePlayerHits},");
                builder.AppendLine($"      \"physicalBarragePlayerDamage\": {result.PhysicalBarragePlayerDamage:0.###},");
                builder.AppendLine($"      \"physicalBarrageReadout\": \"{JsonEscape(result.PhysicalBarrageReadout)}\",");
                builder.AppendLine($"      \"pressureWindowSeconds\": {result.PressureWindowSeconds:0.###},");
                builder.AppendLine($"      \"pressureBurdenSeconds\": {result.PressureBurdenSeconds:0.###},");
                builder.AppendLine($"      \"pressureWindowCount\": {result.PressureWindowCount},");
                builder.AppendLine($"      \"peakPressureWindowShare01\": {result.PeakPressureWindowShare01:0.###},");
                builder.AppendLine($"      \"top3PressureWindowShare01\": {result.Top3PressureWindowShare01:0.###},");
                builder.AppendLine($"      \"peakPressureWindowStartSeconds\": {JsonNullableSeconds(result.PeakPressureWindowStartSeconds)},");
                builder.AppendLine($"      \"timeToNextReliefWindowSeconds\": {JsonNullableSeconds(result.TimeToNextReliefWindowSeconds)},");
                builder.AppendLine($"      \"dominantPressureBurdenState\": \"{JsonEscape(result.DominantPressureBurdenState)}\",");
                builder.AppendLine($"      \"dominantPressureBurdenShare01\": {result.DominantPressureBurdenShare01:0.###},");
                builder.AppendLine($"      \"unansweredPressureBurdenShare01\": {result.UnansweredPressureBurdenShare01:0.###},");
                builder.AppendLine($"      \"answeredPressureBurdenShare01\": {result.AnsweredPressureBurdenShare01:0.###},");
                builder.AppendLine($"      \"pressureWindowReadout\": \"{JsonEscape(result.PressureWindowReadout)}\",");
                builder.AppendLine($"      \"enemyFrontlineClashes\": {result.EnemyFrontlineClashes},");
                builder.AppendLine($"      \"enemyFrontlineBodyHits\": {result.EnemyFrontlineBodyHits},");
                builder.AppendLine($"      \"enemyFrontlineSummonHits\": {result.EnemyFrontlineSummonHits},");
                builder.AppendLine($"      \"enemyFrontlineClashDamage\": {result.EnemyFrontlineClashDamage:0.###},");
                builder.AppendLine($"      \"enemyFrontlineBodyDamage\": {result.EnemyFrontlineBodyDamage:0.###},");
                builder.AppendLine($"      \"maxEnemyFrontlineEngagedCount\": {result.MaxEnemyFrontlineEngagedCount},");
                builder.AppendLine($"      \"maxEnemyFrontlineAttackingCount\": {result.MaxEnemyFrontlineAttackingCount},");
                builder.AppendLine($"      \"allyFrontlineClashes\": {result.AllyFrontlineClashes},");
                builder.AppendLine($"      \"allyFrontlineBodyHits\": {result.AllyFrontlineBodyHits},");
                builder.AppendLine($"      \"allyFrontlineSummonHits\": {result.AllyFrontlineSummonHits},");
                builder.AppendLine($"      \"allyFrontlineClashDamage\": {result.AllyFrontlineClashDamage:0.###},");
                builder.AppendLine($"      \"maxAllyFrontlineEngagedCount\": {result.MaxAllyFrontlineEngagedCount},");
                builder.AppendLine($"      \"maxAllyFrontlineAttackingCount\": {result.MaxAllyFrontlineAttackingCount},");
                builder.AppendLine($"      \"enemySummonDamageFlashes\": {result.EnemySummonDamageFlashes},");
                builder.AppendLine($"      \"enemySummonFullBodyHitReactions\": {result.EnemySummonFullBodyHitReactions},");
                builder.AppendLine($"      \"enemySummonSuppressedHitReactions\": {result.EnemySummonSuppressedHitReactions},");
                builder.AppendLine($"      \"enemySummonNonLockingDamageCues\": {result.EnemySummonNonLockingDamageCues},");
                builder.AppendLine($"      \"enemySummonLockingDamageCues\": {result.EnemySummonLockingDamageCues},");
                builder.AppendLine($"      \"allySummonDamageFlashes\": {result.AllySummonDamageFlashes},");
                builder.AppendLine($"      \"allySummonFullBodyHitReactions\": {result.AllySummonFullBodyHitReactions},");
                builder.AppendLine($"      \"allySummonSuppressedHitReactions\": {result.AllySummonSuppressedHitReactions},");
                builder.AppendLine($"      \"allySummonNonLockingDamageCues\": {result.AllySummonNonLockingDamageCues},");
                builder.AppendLine($"      \"allySummonLockingDamageCues\": {result.AllySummonLockingDamageCues},");
                builder.AppendLine($"      \"playerNonLockingDamageEvents\": {result.PlayerNonLockingDamageEvents},");
                builder.AppendLine($"      \"playerLockingDamageEvents\": {result.PlayerLockingDamageEvents},");
                builder.AppendLine($"      \"playerFullBodyEligibleDamageEvents\": {result.PlayerFullBodyEligibleDamageEvents},");
                builder.AppendLine($"      \"bossNonLockingDamageEvents\": {result.BossNonLockingDamageEvents},");
                builder.AppendLine($"      \"bossLockingDamageEvents\": {result.BossLockingDamageEvents},");
                builder.AppendLine($"      \"bossFullBodyEligibleDamageEvents\": {result.BossFullBodyEligibleDamageEvents},");
                builder.AppendLine($"      \"closeThreatNonLockingDamageEvents\": {result.CloseThreatNonLockingDamageEvents},");
                builder.AppendLine($"      \"closeThreatLockingDamageEvents\": {result.CloseThreatLockingDamageEvents},");
                builder.AppendLine($"      \"closeThreatFullBodyEligibleDamageEvents\": {result.CloseThreatFullBodyEligibleDamageEvents},");
                builder.AppendLine($"      \"followupBlockOpportunityScreenCueRequests\": {result.FollowupBlockOpportunityScreenCueRequests},");
                builder.AppendLine($"      \"followupWindowScreenCueRequests\": {result.FollowupWindowScreenCueRequests},");
                builder.AppendLine($"      \"followupHitScreenCueRequests\": {result.FollowupHitScreenCueRequests},");
                builder.AppendLine($"      \"followupMissedScreenCueRequests\": {result.FollowupMissedScreenCueRequests},");
                builder.AppendLine($"      \"lastFollowupScreenCueId\": \"{JsonEscape(result.LastFollowupScreenCueId)}\",");
                builder.AppendLine($"      \"lastFollowupScreenCueIntensity\": {result.LastFollowupScreenCueIntensity:0.###},");
                builder.AppendLine($"      \"lastFollowupHitScreenCueIntensity\": {result.LastFollowupHitScreenCueIntensity:0.###},");
                builder.AppendLine($"      \"lastFollowupWindowRouteScale\": {result.LastFollowupWindowRouteScale:0.###},");
                builder.AppendLine($"      \"followupWindowCameraCueRequests\": {result.FollowupWindowCameraCueRequests},");
                builder.AppendLine($"      \"followupHitCameraCueRequests\": {result.FollowupHitCameraCueRequests},");
                builder.AppendLine($"      \"followupMissedCameraCueRequests\": {result.FollowupMissedCameraCueRequests},");
                builder.AppendLine($"      \"lastFollowupHitCameraTier\": {result.LastFollowupHitCameraTier},");
                builder.AppendLine($"      \"lastFollowupHitCameraDamage\": {result.LastFollowupHitCameraDamage:0.###},");
                builder.AppendLine($"      \"followupWindowVfxCueRequests\": {result.FollowupWindowVfxCueRequests},");
                builder.AppendLine($"      \"followupHitVfxCueRequests\": {result.FollowupHitVfxCueRequests},");
                builder.AppendLine($"      \"followupMissedVfxCueRequests\": {result.FollowupMissedVfxCueRequests},");
                builder.AppendLine($"      \"lastFollowupHitVfxTier\": {result.LastFollowupHitVfxTier},");
                builder.AppendLine($"      \"lastFollowupHitVfxDamage\": {result.LastFollowupHitVfxDamage:0.###},");
                builder.AppendLine($"      \"followupWindowCinematicCueRequests\": {result.FollowupWindowCinematicCueRequests},");
                builder.AppendLine($"      \"followupHitCinematicCueRequests\": {result.FollowupHitCinematicCueRequests},");
                builder.AppendLine($"      \"followupMissedCinematicCueRequests\": {result.FollowupMissedCinematicCueRequests},");
                builder.AppendLine($"      \"followupHitCinematicFrameOverlayCount\": {result.FollowupHitCinematicFrameOverlayCount},");
                builder.AppendLine($"      \"lastFollowupHitCinematicTier\": {result.LastFollowupHitCinematicTier},");
                builder.AppendLine($"      \"lastFollowupHitCinematicCueId\": \"{JsonEscape(result.LastFollowupHitCinematicCueId)}\",");
                builder.AppendLine($"      \"followupWindowSequenceBridgeRequests\": {result.FollowupWindowSequenceBridgeRequests},");
                builder.AppendLine($"      \"followupHitSequenceBridgeRequests\": {result.FollowupHitSequenceBridgeRequests},");
                builder.AppendLine($"      \"followupMissedSequenceBridgeRequests\": {result.FollowupMissedSequenceBridgeRequests},");
                builder.AppendLine($"      \"lastFollowupHitSequenceTier\": {result.LastFollowupHitSequenceTier},");
                builder.AppendLine($"      \"lastFollowupHitSequenceProfile\": \"{JsonEscape(result.LastFollowupHitSequenceProfile)}\",");
                builder.AppendLine($"      \"routeShape\": \"{JsonEscape(ResolveRouteShape(result))}\",");
                builder.AppendLine($"      \"routeStability01\": {result.RouteStability01:0.###},");
                builder.AppendLine($"      \"minRouteStability01\": {result.MinRouteStability01:0.###},");
                builder.AppendLine($"      \"routeStabilityBand\": \"{JsonEscape(result.RouteStabilityBand)}\",");
                builder.AppendLine($"      \"routeProofState\": \"{JsonEscape(result.RouteProofState)}\",");
                builder.AppendLine($"      \"routeProofReadout\": \"{JsonEscape(result.RouteProofReadout)}\",");
                builder.AppendLine($"      \"counterWaves\": {result.CounterWaves},");
                builder.AppendLine($"      \"counterStabilizedCount\": {result.CounterStabilizedCount},");
                builder.AppendLine($"      \"lastCounterWaveSource\": \"{JsonEscape(result.LastCounterWaveSource)}\",");
                builder.AppendLine($"      \"counterWaveSource\": \"{JsonEscape(result.CounterWaveSource)}\",");
                builder.AppendLine($"      \"counterWaveRecordState\": \"{JsonEscape(result.CounterWaveRecordState)}\",");
                builder.AppendLine($"      \"counterWaveAnswerState\": \"{JsonEscape(result.CounterWaveAnswerState)}\",");
                builder.AppendLine($"      \"counterWaveAnswerReadout\": \"{JsonEscape(result.CounterWaveAnswerReadout)}\",");
                builder.AppendLine($"      \"counterWaveFinalWindowState\": \"{JsonEscape(result.CounterWaveFinalWindowState)}\",");
                builder.AppendLine($"      \"counterWaveFinalWindowReadout\": \"{JsonEscape(result.CounterWaveFinalWindowReadout)}\",");
                builder.AppendLine($"      \"counterWaveEntryPenalty01\": {result.CounterWaveEntryPenalty01:0.###},");
                builder.AppendLine($"      \"counterWaveStabilityBonus01\": {result.CounterWaveStabilityBonus01:0.###},");
                builder.AppendLine($"      \"counterWaveFinalWindowSeconds\": {result.CounterWaveFinalWindowSeconds:0.###},");
                builder.AppendLine($"      \"counterWaveFinalWindowRouteScale\": {result.CounterWaveFinalWindowRouteScale:0.###},");
                builder.AppendLine($"      \"counterWaveAllyHoldRequiredSeconds\": {result.CounterWaveAllyHoldRequiredSeconds:0.###},");
                builder.AppendLine($"      \"counterWaveAllyHoldElapsedSeconds\": {result.CounterWaveAllyHoldElapsedSeconds:0.###},");
                builder.AppendLine($"      \"counterWaveAllyHoldProgress01\": {result.CounterWaveAllyHoldProgress01:0.###},");
                builder.AppendLine($"      \"counterWaveAnswerEnergyPulse\": {result.CounterWaveAnswerEnergyPulse:0.###},");
                builder.AppendLine($"      \"firstCounterWaveAtSeconds\": {JsonNullableSeconds(result.FirstCounterWaveAtSeconds)},");
                builder.AppendLine($"      \"firstCounterStabilizedAtSeconds\": {JsonNullableSeconds(result.FirstCounterStabilizedAtSeconds)},");
                builder.AppendLine($"      \"followupWindowOpenCount\": {result.FollowupWindowOpenCount},");
                builder.AppendLine($"      \"followupHitCount\": {result.FollowupHitCount},");
                builder.AppendLine($"      \"followupMissCount\": {result.FollowupMissCount},");
                builder.AppendLine($"      \"highestFollowupWindowTier\": {result.HighestFollowupWindowTier},");
                builder.AppendLine($"      \"highestFollowupHitTier\": {result.HighestFollowupHitTier},");
                builder.AppendLine($"      \"followupHitDamage\": {result.FollowupHitDamage:0.###},");
                builder.AppendLine($"      \"firstFollowupWindowAtSeconds\": {JsonNullableSeconds(result.FirstFollowupWindowAtSeconds)},");
                builder.AppendLine($"      \"firstFollowupHitAtSeconds\": {JsonNullableSeconds(result.FirstFollowupHitAtSeconds)},");
                builder.AppendLine($"      \"firstFollowupMissAtSeconds\": {JsonNullableSeconds(result.FirstFollowupMissAtSeconds)},");
                builder.AppendLine($"      \"summonFollowupWindowRemainingSeconds\": {result.SummonFollowupWindowRemainingSeconds:0.###},");
                builder.AppendLine($"      \"summonFollowupEnergyPulse\": {result.SummonFollowupEnergyPulse:0.###},");
                builder.AppendLine($"      \"lastSummonFollowupWindowDuration\": {result.LastSummonFollowupWindowDuration:0.###},");
                builder.AppendLine($"      \"highestSummonFollowupSkillTier\": {result.HighestSummonFollowupSkillTier},");
                builder.AppendLine($"      \"highestSkill1FollowupHitTier\": {result.HighestSkill1FollowupHitTier},");
                builder.AppendLine($"      \"skill1FollowupDamage\": {result.Skill1FollowupDamage:0.###},");
                builder.AppendLine($"      \"bossBlockedSkill1Followup\": {JsonBool(result.BossBlockedSkill1Followup)},");
                builder.AppendLine($"      \"bossPressureBlocksDuringSummonFollowup\": {result.BossPressureBlocksDuringSummonFollowup},");
                builder.AppendLine($"      \"cleanFollowupConfirmed\": {JsonBool(result.CleanFollowupConfirmed)},");
                builder.AppendLine($"      \"counterRecoveryConfirmed\": {JsonBool(result.CounterRecoveryConfirmed)},");
                builder.AppendLine($"      \"resultRecords\": {result.ResultRecords},");
                builder.AppendLine($"      \"resultRecordElapsedSeconds\": {result.ResultRecordElapsedSeconds:0.###},");
                builder.AppendLine($"      \"resultRecordRouteStability01\": {result.ResultRecordRouteStability01:0.###},");
                builder.AppendLine($"      \"resultRecordRouteLabel\": \"{JsonEscape(result.ResultRecordRouteLabel)}\",");
                builder.AppendLine($"      \"resultRecordTitle\": \"{JsonEscape(result.ResultRecordTitle)}\",");
                builder.AppendLine($"      \"resultRecordSummary\": \"{JsonEscape(result.ResultRecordSummary)}\",");
                builder.AppendLine($"      \"resultRecordRewardHook\": \"{JsonEscape(result.ResultRecordRewardHook)}\",");
                builder.AppendLine($"      \"resultRecordNextObjective\": \"{JsonEscape(result.ResultRecordNextObjective)}\",");
                builder.AppendLine($"      \"resultRecordProofReadout\": \"{JsonEscape(result.ResultRecordProofReadout)}\",");
                builder.AppendLine($"      \"resultRecordDecision\": \"{JsonEscape(result.ResultRecordDecision)}\",");
                builder.AppendLine($"      \"resultRecordCounterWaveSource\": \"{JsonEscape(result.ResultRecordCounterWaveSource)}\",");
                builder.AppendLine($"      \"routeDecision\": \"{JsonEscape(result.RouteDecision)}\",");
                builder.AppendLine($"      \"completionReadout\": \"{JsonEscape(result.CompletionReadout)}\"");
                builder.Append("    }");
                builder.AppendLine(i + 1 < results.Count ? "," : string.Empty);
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string JsonNullableSeconds(float seconds)
        {
            return seconds >= 0f ? seconds.ToString("0.###") : "null";
        }

        private static string JsonBool(bool value)
        {
            return value ? "true" : "false";
        }

        private static int ResolveMaxEnemyFrontlines(IReadOnlyList<PolicyMetrics> results)
        {
            int max = 0;
            for (int i = 0; i < results.Count; i++)
            {
                max = Mathf.Max(max, results[i].MaxEnemyFrontlineCount);
            }

            return max;
        }

        private static string EscapeTable(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Replace("|", "/");
        }

        private static string JsonEscape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static PolicyMetrics RequireResult(IReadOnlyList<PolicyMetrics> results, PolicyKind policy)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Policy == policy)
                {
                    return results[i];
                }
            }

            Assert.Fail($"Missing policy result {policy}.");
            return null;
        }

        private static GameObject RequireRoot(string objectName)
        {
            GameObject root = GameObject.Find(objectName);
            Assert.NotNull(root, $"Missing scene object {objectName}.");
            return root;
        }

        private static T RequireComponent<T>(GameObject gameObject, string label) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            Assert.NotNull(component, $"{gameObject.name} is missing {label}.");
            return component;
        }

        private static T RequireObject<T>() where T : Component
        {
            T[] found = Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Assert.AreEqual(1, found.Length, $"Expected exactly one {typeof(T).Name} in the review scene.");
            return found[0];
        }

        private static Collider RequireCombatHitCollider(GameObject root, CombatHealth expectedHealth, string label)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].GetComponentInParent<CombatHealth>() == expectedHealth)
                {
                    return colliders[i];
                }
            }

            Assert.Fail($"{label} should expose at least one child collider under its CombatHealth root.");
            return null;
        }

        private static LaneActionProjectile FindActivePlayerProjectile()
        {
            LaneActionProjectile[] projectiles = FindActivePlayerProjectiles();
            return projectiles.Length > 0 ? projectiles[0] : null;
        }

        private static LaneActionProjectile[] FindActivePlayerProjectiles()
        {
            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            List<LaneActionProjectile> active = new List<LaneActionProjectile>();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive && projectiles[i].SourceTeam == DamageTeam.Player)
                {
                    active.Add(projectiles[i]);
                }
            }

            return active.ToArray();
        }

        private static SummonPressureScreen FindActiveAllyPressureScreen()
        {
            SummonPressureScreen[] screens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].IsActive && screens[i].OwnerTeam == DamageTeam.AllySummon)
                {
                    return screens[i];
                }
            }

            return null;
        }

        private static SummonPressureScreen FindActiveEnemyPressureScreen()
        {
            SummonPressureScreen[] screens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].IsActive && screens[i].OwnerTeam == DamageTeam.Enemy)
                {
                    return screens[i];
                }
            }

            return null;
        }

        private static BossBarrageProjectile FindFirstActiveBossProjectile()
        {
            BossBarrageProjectile[] projectiles = FindActiveBossProjectiles();
            return projectiles.Length > 0 ? projectiles[0] : null;
        }

        private static BossBarrageProjectile[] FindActiveBossProjectiles()
        {
            BossBarrageProjectile[] projectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            List<BossBarrageProjectile> active = new List<BossBarrageProjectile>();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive)
                {
                    active.Add(projectiles[i]);
                }
            }

            return active.ToArray();
        }

        private static void DeactivateActiveBossProjectiles()
        {
            BossBarrageProjectile[] projectiles = FindActiveBossProjectiles();
            for (int i = 0; i < projectiles.Length; i++)
            {
                projectiles[i].Deactivate();
            }
        }

        private static float GetFloat(Object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{target.GetType().Name} is missing private field {fieldName}.");
            return (float)field.GetValue(target);
        }

        private enum BossWaveAnswer
        {
            PlayerTakesHit,
            SummonScreen
        }

        private sealed class CombatPolicyContext
        {
            private readonly Dictionary<int, int> observedFrontlineClashCounts =
                new Dictionary<int, int>();
            private readonly Dictionary<int, int> observedSummonDamageFlashCounts =
                new Dictionary<int, int>();
            private readonly Dictionary<int, int> observedSummonAnimatorHitCounts =
                new Dictionary<int, int>();
            private readonly Dictionary<int, int> observedSuppressedSummonAnimatorHitCounts =
                new Dictionary<int, int>();
            private readonly List<float> pressureWindowBurden = new List<float>();
            private float openPressureBurden;
            private float enemyOnlyPressureBurden;
            private float contestedPressureBurden;
            private float allyOnlyPressureBurden;

            public CombatPolicyContext(
                PolicyKind policy,
                PlayerMovementController player,
                PlayerRangedBasicAttackAction rangedBasicAttack,
                PlayerSkill1Action skill1Action,
                PlayerSummonSlot1Action summonSlot1Action,
                SummonEnergyLadder energyLadder,
                PlayerCombatTargetSelector targetSelector,
                SummonLaneSpace laneSpace,
                BossBarrageEmitter bossEmitter,
                BossSummonPressureAction bossSummonPressureAction,
                BossBarragePocketReviewOwner pocketOwner,
                BossBarragePocketVfxCueBridge pocketVfxCueBridge,
                ActionCameraCueDriver cameraCueDriver,
                ActionCinematicCueDirector cinematicCueDirector,
                ActionCinematicSequenceBridge cinematicSequenceBridge,
                ActionScreenCuePresenter screenCuePresenter,
                CombatHealth playerHealth,
                CombatHealth bossHealth,
                CombatHealth closeThreatHealth,
                Collider playerCollider,
                Collider bossCollider,
                Collider closeThreatCollider)
            {
                Policy = policy;
                Player = player;
                RangedBasicAttack = rangedBasicAttack;
                Skill1Action = skill1Action;
                SummonSlot1Action = summonSlot1Action;
                EnergyLadder = energyLadder;
                TargetSelector = targetSelector;
                LaneSpace = laneSpace;
                BossEmitter = bossEmitter;
                BossSummonPressureAction = bossSummonPressureAction;
                PocketOwner = pocketOwner;
                PocketVfxCueBridge = pocketVfxCueBridge;
                CameraCueDriver = cameraCueDriver;
                CinematicCueDirector = cinematicCueDirector;
                CinematicSequenceBridge = cinematicSequenceBridge;
                ScreenCuePresenter = screenCuePresenter;
                PlayerHealth = playerHealth;
                BossHealth = bossHealth;
                CloseThreatHealth = closeThreatHealth;
                PlayerCollider = playerCollider;
                BossCollider = bossCollider;
                CloseThreatCollider = closeThreatCollider;
                Metrics = new PolicyMetrics(policy);
                Metrics.PlayerHealthStart = playerHealth.CurrentHealth;
                Metrics.BossHealthStart = bossHealth.CurrentHealth;
                Metrics.CloseThreatHealthStart = closeThreatHealth.CurrentHealth;
                observedScreenCueRequestCount = screenCuePresenter.CueRequestCount;
                observedScreenFollowupCueRequestCount = screenCuePresenter.FollowupCueRequestCount;
                observedCinematicPlayCount = cinematicCueDirector.TotalPlayCount;
                observedSequenceBridgePlayCount = cinematicSequenceBridge.TotalPlayCount;

                PlayerHealth.Damaged += OnPlayerDamaged;
                BossHealth.Damaged += OnBossDamaged;
                CloseThreatHealth.Damaged += OnCloseThreatDamaged;
                SummonSlot1Action.SummonPressureBlocked += OnSummonPressureBlocked;
                BossSummonPressureAction.PressureSummonReleased += OnBossPressureSummonReleased;
                BossSummonPressureAction.PressureSummonIntercepted += OnBossPressureSummonIntercepted;
                PocketOwner.SummonFollowupWindowOpened += OnSummonFollowupWindowOpened;
                PocketOwner.SummonFollowupHitConfirmed += OnSummonFollowupHitConfirmed;
                PocketOwner.SummonFollowupMissed += OnSummonFollowupMissed;
                PocketOwner.CounterWaveObserved += OnCounterWaveObserved;
                PocketOwner.CounterWaveStabilized += OnCounterWaveStabilized;
                PocketOwner.ResultRecordCommitted += OnResultRecordCommitted;
            }

            public PolicyKind Policy { get; }
            public PlayerMovementController Player { get; }
            public PlayerRangedBasicAttackAction RangedBasicAttack { get; }
            public PlayerSkill1Action Skill1Action { get; }
            public PlayerSummonSlot1Action SummonSlot1Action { get; }
            public SummonEnergyLadder EnergyLadder { get; }
            public PlayerCombatTargetSelector TargetSelector { get; }
            public SummonLaneSpace LaneSpace { get; }
            public BossBarrageEmitter BossEmitter { get; }
            public BossSummonPressureAction BossSummonPressureAction { get; }
            public BossBarragePocketReviewOwner PocketOwner { get; }
            public BossBarragePocketVfxCueBridge PocketVfxCueBridge { get; }
            public ActionCameraCueDriver CameraCueDriver { get; }
            public ActionCinematicCueDirector CinematicCueDirector { get; }
            public ActionCinematicSequenceBridge CinematicSequenceBridge { get; }
            public ActionScreenCuePresenter ScreenCuePresenter { get; }
            public CombatHealth PlayerHealth { get; }
            public CombatHealth BossHealth { get; }
            public CombatHealth CloseThreatHealth { get; }
            public Collider PlayerCollider { get; }
            public Collider BossCollider { get; }
            public Collider CloseThreatCollider { get; }
            public PolicyMetrics Metrics { get; }

            public void Sample(float deltaTime = 0f)
            {
                Metrics.PlayerHealthRemaining = PlayerHealth.CurrentHealth;
                Metrics.BossHealthRemaining = BossHealth.CurrentHealth;
                Metrics.CloseThreatHealthRemaining = CloseThreatHealth.CurrentHealth;
                if (!PlayerHealth.IsAlive && Metrics.FirstPlayerDownAtSeconds < 0f)
                {
                    Metrics.FirstPlayerDownAtSeconds = Metrics.ElapsedSeconds;
                }

                if (!BossHealth.IsAlive && Metrics.FirstBossDownAtSeconds < 0f)
                {
                    Metrics.FirstBossDownAtSeconds = Metrics.ElapsedSeconds;
                }

                Metrics.RouteDecision = $"{PocketOwner.RouteDecisionState}({PocketOwner.RouteDecisionReadout})";
                Metrics.CompletionReadout = PocketOwner.CompletionRecordReadout;
                Metrics.RouteStability01 = PocketOwner.RouteStability01;
                Metrics.MinRouteStability01 = Mathf.Min(
                    Metrics.MinRouteStability01,
                    PocketOwner.RouteStability01);
                Metrics.RouteStabilityBand = PocketOwner.CurrentRouteStabilityBand.ToString();
                Metrics.RouteProofState = PocketOwner.RouteProofState;
                Metrics.RouteProofReadout = PocketOwner.RouteProofReadout;
                Metrics.CounterWaveRecordState = PocketOwner.CounterWaveRecordState;
                Metrics.CounterWaveSource = PocketOwner.CounterWaveSourceReadout;
                Metrics.CounterWaveAnswerState = PocketOwner.CounterWaveAnswerState;
                Metrics.CounterWaveAnswerReadout = PocketOwner.CounterWaveAnswerReadout;
                Metrics.CounterWaveFinalWindowState = PocketOwner.CounterWaveFinalWindowState;
                Metrics.CounterWaveFinalWindowReadout = PocketOwner.CounterWaveFinalWindowReadout;
                if (PocketOwner.IsCounterWaveFinalWindowOpened
                    && Metrics.FirstCounterFinalWindowAtSeconds < 0f)
                {
                    Metrics.FirstCounterFinalWindowAtSeconds = Metrics.ElapsedSeconds;
                }

                Metrics.CounterWaveEntryPenalty01 = PocketOwner.LastCounterWaveEntryPenalty;
                Metrics.CounterWaveStabilityBonus01 = PocketOwner.LastCounterWaveStabilityBonus;
                Metrics.CounterWaveFinalWindowSeconds = PocketOwner.LastCounterWaveFinalWindowDuration;
                Metrics.CounterWaveFinalWindowRouteScale = PocketOwner.LastCounterWaveFinalWindowRouteScale;
                Metrics.CounterWaveAllyHoldRequiredSeconds = PocketOwner.CounterWaveAllyHoldRequiredSeconds;
                Metrics.CounterWaveAllyHoldElapsedSeconds = PocketOwner.CounterWaveAllyHoldElapsedSeconds;
                Metrics.CounterWaveAllyHoldProgress01 = PocketOwner.CounterWaveAllyHoldProgress01;
                Metrics.CounterWaveAnswerEnergyPulse = PocketOwner.LastCounterWaveAnswerEnergyPulse;
                Metrics.UnansweredBossHitRoutePenaltyCount = PocketOwner.UnansweredBossHitRoutePenaltyCount;
                Metrics.LastUnansweredBossHitRoutePenalty01 = PocketOwner.LastUnansweredBossHitRoutePenalty;
                Metrics.TotalUnansweredBossHitRoutePenalty01 = PocketOwner.TotalUnansweredBossHitRoutePenalty;
                Metrics.SummonFollowupWindowRemainingSeconds = PocketOwner.SummonFollowupWindowRemainingSeconds;
                Metrics.SummonFollowupEnergyPulse = PocketOwner.SummonFollowupEnergyPulse;
                Metrics.LastSummonFollowupWindowDuration = PocketOwner.LastSummonFollowupWindowDuration;
                Metrics.HighestSummonFollowupSkillTier = PocketOwner.HighestSummonFollowupSkillTier;
                Metrics.HighestSkill1FollowupHitTier = PocketOwner.HighestSkill1FollowupHitTier;
                Metrics.Skill1FollowupDamage = PocketOwner.Skill1FollowupDamage;
                Metrics.BossBlockedSkill1Followup |= PocketOwner.BossBlockedSkill1Followup;
                Metrics.BossPressureBlocksDuringSummonFollowup = Mathf.Max(
                    Metrics.BossPressureBlocksDuringSummonFollowup,
                    PocketOwner.BossPressureBlocksDuringSummonFollowup);
                Metrics.MaxBossPressureActiveScreenCount = Mathf.Max(
                    Metrics.MaxBossPressureActiveScreenCount,
                    BossSummonPressureAction.ActivePressureScreenCount);
                Metrics.BossPressureActiveScreenRemainingIntercepts =
                    BossSummonPressureAction.ActivePressureScreenRemainingIntercepts;
                Metrics.CleanFollowupConfirmed = PocketOwner.Skill1FollowupHitConfirmed
                    && !PocketOwner.IsCounterWaveCompletionRecorded;
                Metrics.CounterRecoveryConfirmed = PocketOwner.IsCounterWaveStabilized
                    || PocketOwner.IsCounterWaveFinalWindowOpened;
                int enemyFrontlineCount = PocketOwner.ActiveEnemyFrontlineProxyCount;
                int allyFrontlineCount = PocketOwner.ActiveAllyFrontlineProxyCount;
                Metrics.MaxEnemyFrontlineCount = Mathf.Max(
                    Metrics.MaxEnemyFrontlineCount,
                    enemyFrontlineCount);
                Metrics.MaxAllyFrontlineCount = Mathf.Max(
                    Metrics.MaxAllyFrontlineCount,
                    allyFrontlineCount);
                Metrics.FrontlinePresenceReadout = PocketOwner.FrontlinePresenceReadout;
                Metrics.MaxRouteStabilityDrainPerSecond = Mathf.Max(
                    Metrics.MaxRouteStabilityDrainPerSecond,
                    PocketOwner.CurrentRouteStabilityDrainPerSecond);
                Metrics.EnergyChargingTier = EnergyLadder.ChargingTier;
                Metrics.EnergyAvailableTier = EnergyLadder.AvailableTier;
                Metrics.EnergyFillRatio = EnergyLadder.CurrentTierFillRatio;
                Metrics.LastEnergyForwardRisk01 = EnergyLadder.CurrentForwardRisk01;
                Metrics.LastEnergyGainMultiplier = EnergyLadder.CurrentGainMultiplier;
                Metrics.LastEnergyRiskBand = EnergyLadder.CurrentRiskBand.ToString();
                if (EnergyLadder.AvailableTier >= 1 && Metrics.EnergyTier1ReadyAtSeconds < 0f)
                {
                    Metrics.EnergyTier1ReadyAtSeconds = Metrics.ElapsedSeconds;
                }

                SampleFrontlineClashCost();
                SampleFrontlineHitReactionPresentation();
                SampleFollowupPresentationBridge();

                if (deltaTime <= 0f)
                {
                    return;
                }

                Metrics.RouteDrainAccumulated01 += PocketOwner.CurrentRouteStabilityDrainPerSecond * deltaTime;
                Metrics.RoutePressureWeightSeconds += PocketOwner.CurrentRoutePressureWeight * deltaTime;
                Metrics.FrontlinePresenceScaleSeconds += PocketOwner.CurrentFrontlinePresenceDrainScale * deltaTime;
                Metrics.EnergySampleSeconds += deltaTime;
                Metrics.EnergyForwardRiskSeconds += EnergyLadder.CurrentForwardRisk01 * deltaTime;
                Metrics.EnergyGainMultiplierSeconds += EnergyLadder.CurrentGainMultiplier * deltaTime;
                switch (EnergyLadder.CurrentRiskBand)
                {
                    case SummonEnergyRiskBand.BackSafety:
                        Metrics.BackSafetyBandSeconds += deltaTime;
                        break;
                    case SummonEnergyRiskBand.MidCharge:
                        Metrics.MidChargeBandSeconds += deltaTime;
                        break;
                    case SummonEnergyRiskBand.ForwardRisk:
                        Metrics.ForwardRiskBandSeconds += deltaTime;
                        break;
                }

                SamplePressureShape(deltaTime, enemyFrontlineCount, allyFrontlineCount);
                if (allyFrontlineCount > 0 && enemyFrontlineCount > 0)
                {
                    Metrics.ContestedFrontlineSeconds += deltaTime;
                }
                else if (enemyFrontlineCount > 0)
                {
                    Metrics.EnemyOnlyFrontlineSeconds += deltaTime;
                }
                else if (allyFrontlineCount > 0)
                {
                    Metrics.AllyOnlyFrontlineSeconds += deltaTime;
                }
            }

            private void SamplePressureShape(float deltaTime, int enemyFrontlineCount, int allyFrontlineCount)
            {
                float pressureBurden = Mathf.Max(0f, PocketOwner.CurrentRouteStabilityDrainPerSecond)
                    * deltaTime;
                if (pressureBurden <= 0f)
                {
                    return;
                }

                float sampleMidpointSeconds = Mathf.Max(0f, Metrics.ElapsedSeconds - deltaTime * 0.5f);
                int windowIndex = Mathf.Max(
                    0,
                    Mathf.FloorToInt(sampleMidpointSeconds / PressureWindowSeconds));
                while (pressureWindowBurden.Count <= windowIndex)
                {
                    pressureWindowBurden.Add(0f);
                }

                pressureWindowBurden[windowIndex] += pressureBurden;
                Metrics.PressureBurdenSeconds += pressureBurden;

                if (allyFrontlineCount > 0 && enemyFrontlineCount > 0)
                {
                    contestedPressureBurden += pressureBurden;
                }
                else if (enemyFrontlineCount > 0)
                {
                    enemyOnlyPressureBurden += pressureBurden;
                }
                else if (allyFrontlineCount > 0)
                {
                    allyOnlyPressureBurden += pressureBurden;
                }
                else
                {
                    openPressureBurden += pressureBurden;
                }
            }

            private void CompletePressureShape()
            {
                EnsurePressureWindowCoverage();
                Metrics.PressureWindowSeconds = PressureWindowSeconds;
                Metrics.PressureWindowCount = pressureWindowBurden.Count;
                Metrics.PressureWindowReadout = BuildPressureWindowReadout();
                if (Metrics.PressureBurdenSeconds <= 0f || pressureWindowBurden.Count == 0)
                {
                    Metrics.DominantPressureBurdenState = "none";
                    return;
                }

                int peakWindowIndex = 0;
                float peakWindowBurden = pressureWindowBurden[0];
                for (int i = 1; i < pressureWindowBurden.Count; i++)
                {
                    if (pressureWindowBurden[i] > peakWindowBurden)
                    {
                        peakWindowBurden = pressureWindowBurden[i];
                        peakWindowIndex = i;
                    }
                }

                List<float> sortedWindows = new List<float>(pressureWindowBurden);
                sortedWindows.Sort((left, right) => right.CompareTo(left));
                float top3Burden = 0f;
                int topWindowCount = Mathf.Min(3, sortedWindows.Count);
                for (int i = 0; i < topWindowCount; i++)
                {
                    top3Burden += sortedWindows[i];
                }

                Metrics.PeakPressureWindowShare01 = peakWindowBurden / Metrics.PressureBurdenSeconds;
                Metrics.Top3PressureWindowShare01 = Mathf.Clamp01(top3Burden / Metrics.PressureBurdenSeconds);
                Metrics.PeakPressureWindowStartSeconds = peakWindowIndex * PressureWindowSeconds;
                Metrics.TimeToNextReliefWindowSeconds = ResolveReliefAfterPeakSeconds(
                    peakWindowIndex,
                    peakWindowBurden);
                ResolveDominantPressureBurden();
            }

            private float ResolveReliefAfterPeakSeconds(int peakWindowIndex, float peakWindowBurden)
            {
                float reliefThreshold = peakWindowBurden * ReliefPressureWindowPeakRatio;
                for (int i = peakWindowIndex + 1; i < pressureWindowBurden.Count; i++)
                {
                    if (pressureWindowBurden[i] <= reliefThreshold)
                    {
                        return (i - peakWindowIndex) * PressureWindowSeconds;
                    }
                }

                return -1f;
            }

            private void EnsurePressureWindowCoverage()
            {
                int requiredWindowCount = Mathf.Max(
                    0,
                    Mathf.CeilToInt(Metrics.ElapsedSeconds / PressureWindowSeconds));
                if (Metrics.IsClearResult)
                {
                    requiredWindowCount++;
                }

                while (pressureWindowBurden.Count < requiredWindowCount)
                {
                    pressureWindowBurden.Add(0f);
                }
            }

            private void ResolveDominantPressureBurden()
            {
                string dominantState = "open";
                float dominantBurden = openPressureBurden;
                if (enemyOnlyPressureBurden > dominantBurden)
                {
                    dominantState = "enemy_only";
                    dominantBurden = enemyOnlyPressureBurden;
                }

                if (contestedPressureBurden > dominantBurden)
                {
                    dominantState = "contested";
                    dominantBurden = contestedPressureBurden;
                }

                if (allyOnlyPressureBurden > dominantBurden)
                {
                    dominantState = "ally_only";
                    dominantBurden = allyOnlyPressureBurden;
                }

                float totalBurden = Metrics.PressureBurdenSeconds;
                float unansweredBurden = enemyOnlyPressureBurden + contestedPressureBurden * 0.5f;
                float answeredBurden = allyOnlyPressureBurden + contestedPressureBurden * 0.5f;
                Metrics.DominantPressureBurdenState = dominantState;
                Metrics.DominantPressureBurdenShare01 = dominantBurden / totalBurden;
                Metrics.UnansweredPressureBurdenShare01 = unansweredBurden / totalBurden;
                Metrics.AnsweredPressureBurdenShare01 = answeredBurden / totalBurden;
            }

            private string BuildPressureWindowReadout()
            {
                if (pressureWindowBurden.Count == 0)
                {
                    return "none";
                }

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < pressureWindowBurden.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append("; ");
                    }

                    float start = i * PressureWindowSeconds;
                    float end = start + PressureWindowSeconds;
                    builder.Append($"{start:0.0}-{end:0.0}s:{pressureWindowBurden[i]:0.00}");
                }

                return builder.ToString();
            }

            private void SampleFrontlineClashCost()
            {
                int enemyEngagedCount = 0;
                int enemyAttackingCount = 0;
                int allyEngagedCount = 0;
                int allyAttackingCount = 0;
                int proxyCount = SummonFrontlineProxy.ActiveRegisteredProxyCount;
                for (int i = 0; i < proxyCount; i++)
                {
                    if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy)
                        || proxy == null
                        || !proxy.IsActive)
                    {
                        continue;
                    }

                    DamageTeam team = proxy.Health != null ? proxy.Health.Team : DamageTeam.Neutral;
                    bool enemyProxy = team == DamageTeam.Enemy;
                    bool allyProxy = CombatTeamUtility.IsPlayerSide(team);
                    bool engaged = proxy.IsAdvanceHeld
                        || proxy.CurrentState == SummonFrontlineProxyState.Engaging
                        || proxy.CurrentState == SummonFrontlineProxyState.Attacking;
                    bool attacking = proxy.CurrentState == SummonFrontlineProxyState.Attacking;

                    if (enemyProxy)
                    {
                        if (engaged)
                        {
                            enemyEngagedCount++;
                        }

                        if (attacking)
                        {
                            enemyAttackingCount++;
                        }
                    }
                    else if (allyProxy)
                    {
                        if (engaged)
                        {
                            allyEngagedCount++;
                        }

                        if (attacking)
                        {
                            allyAttackingCount++;
                        }
                    }

                    SummonFrontlineClash clash = proxy.GetComponent<SummonFrontlineClash>();
                    if (clash == null)
                    {
                        continue;
                    }

                    int key = proxy.GetInstanceID();
                    int currentCount = clash.TotalClashCount;
                    if (!observedFrontlineClashCounts.TryGetValue(key, out int previousCount))
                    {
                        observedFrontlineClashCounts[key] = currentCount;
                        continue;
                    }

                    if (currentCount > previousCount)
                    {
                        int delta = currentCount - previousCount;
                        float damage = clash.LastDamageAmount * delta;
                        if (enemyProxy)
                        {
                            Metrics.EnemyFrontlineClashes += delta;
                            Metrics.EnemyFrontlineClashDamage += damage;
                            if (clash.LastTargetKind == SummonFrontlineClashTargetKind.HostileBody)
                            {
                                Metrics.EnemyFrontlineBodyHits += delta;
                                Metrics.EnemyFrontlineBodyDamage += damage;
                            }
                            else if (clash.LastTargetKind == SummonFrontlineClashTargetKind.HostileSummon)
                            {
                                Metrics.EnemyFrontlineSummonHits += delta;
                            }
                        }
                        else if (allyProxy)
                        {
                            Metrics.AllyFrontlineClashes += delta;
                            Metrics.AllyFrontlineClashDamage += damage;
                            if (clash.LastTargetKind == SummonFrontlineClashTargetKind.HostileBody)
                            {
                                Metrics.AllyFrontlineBodyHits += delta;
                            }
                            else if (clash.LastTargetKind == SummonFrontlineClashTargetKind.HostileSummon)
                            {
                                Metrics.AllyFrontlineSummonHits += delta;
                            }
                        }
                    }

                    observedFrontlineClashCounts[key] = currentCount;
                }

                Metrics.MaxEnemyFrontlineEngagedCount = Mathf.Max(
                    Metrics.MaxEnemyFrontlineEngagedCount,
                    enemyEngagedCount);
                Metrics.MaxEnemyFrontlineAttackingCount = Mathf.Max(
                    Metrics.MaxEnemyFrontlineAttackingCount,
                    enemyAttackingCount);
                Metrics.MaxAllyFrontlineEngagedCount = Mathf.Max(
                    Metrics.MaxAllyFrontlineEngagedCount,
                    allyEngagedCount);
                Metrics.MaxAllyFrontlineAttackingCount = Mathf.Max(
                    Metrics.MaxAllyFrontlineAttackingCount,
                    allyAttackingCount);
            }

            private void SampleFrontlineHitReactionPresentation()
            {
                int proxyCount = SummonFrontlineProxy.ActiveRegisteredProxyCount;
                for (int i = 0; i < proxyCount; i++)
                {
                    if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy)
                        || proxy == null
                        || !proxy.IsActive)
                    {
                        continue;
                    }

                    DamageTeam team = proxy.Health != null ? proxy.Health.Team : DamageTeam.Neutral;
                    bool enemyProxy = team == DamageTeam.Enemy;
                    bool allyProxy = CombatTeamUtility.IsPlayerSide(team);
                    if (!enemyProxy && !allyProxy)
                    {
                        continue;
                    }

                    SummonFrontlineProxyPresenter presenter = proxy.GetComponent<SummonFrontlineProxyPresenter>();
                    if (presenter == null)
                    {
                        continue;
                    }

                    int key = proxy.GetInstanceID();
                    int damageFlashCount = presenter.DamageFlashCount;
                    int animatorHitCount = presenter.AnimatorHitTriggerCount;
                    int suppressedAnimatorHitCount = presenter.SuppressedAnimatorHitTriggerCount;
                    observedSummonDamageFlashCounts.TryGetValue(key, out int previousDamageFlashCount);
                    observedSummonAnimatorHitCounts.TryGetValue(key, out int previousAnimatorHitCount);
                    observedSuppressedSummonAnimatorHitCounts.TryGetValue(
                        key,
                        out int previousSuppressedAnimatorHitCount);

                    int damageFlashDelta = Mathf.Max(0, damageFlashCount - previousDamageFlashCount);
                    int animatorHitDelta = Mathf.Max(0, animatorHitCount - previousAnimatorHitCount);
                    int suppressedAnimatorHitDelta = Mathf.Max(
                        0,
                        suppressedAnimatorHitCount - previousSuppressedAnimatorHitCount);

                    if (damageFlashDelta > 0)
                    {
                        bool interruptsAction =
                            DamageResponsePolicyUtility.InterruptsAction(presenter.LastDamageControlLockPolicy);
                        if (enemyProxy)
                        {
                            Metrics.EnemySummonDamageFlashes += damageFlashDelta;
                            if (interruptsAction)
                            {
                                Metrics.EnemySummonLockingDamageCues += damageFlashDelta;
                            }
                            else
                            {
                                Metrics.EnemySummonNonLockingDamageCues += damageFlashDelta;
                            }
                        }
                        else
                        {
                            Metrics.AllySummonDamageFlashes += damageFlashDelta;
                            if (interruptsAction)
                            {
                                Metrics.AllySummonLockingDamageCues += damageFlashDelta;
                            }
                            else
                            {
                                Metrics.AllySummonNonLockingDamageCues += damageFlashDelta;
                            }
                        }
                    }

                    if (animatorHitDelta > 0)
                    {
                        if (enemyProxy)
                        {
                            Metrics.EnemySummonFullBodyHitReactions += animatorHitDelta;
                        }
                        else
                        {
                            Metrics.AllySummonFullBodyHitReactions += animatorHitDelta;
                        }
                    }

                    if (suppressedAnimatorHitDelta > 0)
                    {
                        if (enemyProxy)
                        {
                            Metrics.EnemySummonSuppressedHitReactions += suppressedAnimatorHitDelta;
                        }
                        else
                        {
                            Metrics.AllySummonSuppressedHitReactions += suppressedAnimatorHitDelta;
                        }
                    }

                    observedSummonDamageFlashCounts[key] = damageFlashCount;
                    observedSummonAnimatorHitCounts[key] = animatorHitCount;
                    observedSuppressedSummonAnimatorHitCounts[key] = suppressedAnimatorHitCount;
                }
            }

            private int observedScreenCueRequestCount;
            private int observedScreenFollowupCueRequestCount;
            private int observedCinematicPlayCount;
            private int observedSequenceBridgePlayCount;

            private void SampleFollowupPresentationBridge()
            {
                Metrics.FollowupWindowCameraCueRequests = CameraCueDriver.SummonFollowupWindowCueRequestCount;
                Metrics.FollowupHitCameraCueRequests = CameraCueDriver.SummonFollowupHitCueRequestCount;
                Metrics.FollowupMissedCameraCueRequests = CameraCueDriver.SummonFollowupMissedCueRequestCount;
                Metrics.LastFollowupHitCameraTier = CameraCueDriver.LastSummonFollowupHitTier;
                Metrics.LastFollowupHitCameraDamage = CameraCueDriver.LastSummonFollowupHitDamage;

                Metrics.FollowupWindowVfxCueRequests = PocketVfxCueBridge.FollowupWindowCueRequestCount;
                Metrics.FollowupHitVfxCueRequests = PocketVfxCueBridge.FollowupHitCueRequestCount;
                Metrics.FollowupMissedVfxCueRequests = PocketVfxCueBridge.FollowupMissedCueRequestCount;
                Metrics.LastFollowupHitVfxTier = PocketVfxCueBridge.LastFollowupHitTier;
                Metrics.LastFollowupHitVfxDamage = PocketVfxCueBridge.LastFollowupHitDamage;

                int cueDelta = ScreenCuePresenter.CueRequestCount - observedScreenCueRequestCount;
                int followupDelta = ScreenCuePresenter.FollowupCueRequestCount - observedScreenFollowupCueRequestCount;
                if (cueDelta > 0 && followupDelta > 0)
                {
                    int resolvedDelta = Mathf.Min(cueDelta, followupDelta);
                    string cueId = ScreenCuePresenter.LastCueId ?? string.Empty;
                    if (cueId.StartsWith("Followup.Window", StringComparison.Ordinal))
                    {
                        Metrics.FollowupWindowScreenCueRequests += resolvedDelta;
                    }
                    else if (cueId == "Followup.Hit")
                    {
                        Metrics.FollowupHitScreenCueRequests += resolvedDelta;
                        Metrics.LastFollowupHitScreenCueIntensity = ScreenCuePresenter.LastCueIntensity;
                    }
                    else if (cueId == "Followup.Missed")
                    {
                        Metrics.FollowupMissedScreenCueRequests += resolvedDelta;
                    }
                    else if (cueId == "Followup.BlockOpportunity")
                    {
                        Metrics.FollowupBlockOpportunityScreenCueRequests += resolvedDelta;
                    }

                    Metrics.LastFollowupScreenCueId = cueId;
                    Metrics.LastFollowupScreenCueIntensity = ScreenCuePresenter.LastCueIntensity;
                    Metrics.LastFollowupWindowRouteScale = ScreenCuePresenter.LastFollowupWindowRouteScale;
                }

                observedScreenCueRequestCount = ScreenCuePresenter.CueRequestCount;
                observedScreenFollowupCueRequestCount = ScreenCuePresenter.FollowupCueRequestCount;

                int cinematicDelta = CinematicCueDirector.TotalPlayCount - observedCinematicPlayCount;
                if (cinematicDelta > 0)
                {
                    RecordCinematicCue(
                        CinematicCueDirector.LastPlayedKind,
                        CinematicCueDirector.LastPlayedTier,
                        CinematicCueDirector.LastPlayedCueId,
                        cinematicDelta,
                        CinematicCueDirector.HasActiveFrameOverlay);
                }

                observedCinematicPlayCount = CinematicCueDirector.TotalPlayCount;

                int sequenceDelta = CinematicSequenceBridge.TotalPlayCount - observedSequenceBridgePlayCount;
                if (sequenceDelta > 0)
                {
                    RecordSequenceBridgeCue(
                        CinematicSequenceBridge.LastPlayedKind,
                        CinematicSequenceBridge.LastPlayedTier,
                        CinematicSequenceBridge.LastPlayedProfile != null
                            ? CinematicSequenceBridge.LastPlayedProfile.name
                            : string.Empty,
                        sequenceDelta);
                }

                observedSequenceBridgePlayCount = CinematicSequenceBridge.TotalPlayCount;
            }

            private void RecordCinematicCue(
                ActionCinematicCueProfile.CueKind cueKind,
                int tier,
                string cueId,
                int count,
                bool frameOverlayActive)
            {
                switch (cueKind)
                {
                    case ActionCinematicCueProfile.CueKind.BossPressureBreak:
                        Metrics.FollowupWindowCinematicCueRequests += count;
                        break;
                    case ActionCinematicCueProfile.CueKind.SummonFollowupHit:
                        Metrics.FollowupHitCinematicCueRequests += count;
                        Metrics.LastFollowupHitCinematicTier = tier;
                        Metrics.LastFollowupHitCinematicCueId = cueId ?? string.Empty;
                        if (frameOverlayActive)
                        {
                            Metrics.FollowupHitCinematicFrameOverlayCount += count;
                        }

                        break;
                    case ActionCinematicCueProfile.CueKind.SummonRecall:
                        Metrics.FollowupMissedCinematicCueRequests += count;
                        break;
                }
            }

            private void RecordSequenceBridgeCue(
                ActionCinematicCueProfile.CueKind cueKind,
                int tier,
                string profileName,
                int count)
            {
                switch (cueKind)
                {
                    case ActionCinematicCueProfile.CueKind.BossPressureBreak:
                        Metrics.FollowupWindowSequenceBridgeRequests += count;
                        break;
                    case ActionCinematicCueProfile.CueKind.SummonFollowupHit:
                        Metrics.FollowupHitSequenceBridgeRequests += count;
                        Metrics.LastFollowupHitSequenceTier = tier;
                        Metrics.LastFollowupHitSequenceProfile = profileName ?? string.Empty;
                        break;
                    case ActionCinematicCueProfile.CueKind.SummonRecall:
                        Metrics.FollowupMissedSequenceBridgeRequests += count;
                        break;
                }
            }

            public void Complete()
            {
                Sample();
                Metrics.ResultKind = PocketOwner.HasCommittedResultRecord
                    ? PocketOwner.LastResultRecord.ResultKind.ToString()
                    : PocketOwner.IsCleared
                        ? "ClearedNoRecord"
                        : PocketOwner.IsFailed
                            ? PocketOwner.FailureReason.ToString()
                            : "Running";
                CompletePressureShape();
                Metrics.PlayerHealthRemaining = PlayerHealth.CurrentHealth;
                Metrics.BossHealthRemaining = BossHealth.CurrentHealth;
                Metrics.CloseThreatHealthRemaining = CloseThreatHealth.CurrentHealth;

                PlayerHealth.Damaged -= OnPlayerDamaged;
                BossHealth.Damaged -= OnBossDamaged;
                CloseThreatHealth.Damaged -= OnCloseThreatDamaged;
                SummonSlot1Action.SummonPressureBlocked -= OnSummonPressureBlocked;
                BossSummonPressureAction.PressureSummonReleased -= OnBossPressureSummonReleased;
                BossSummonPressureAction.PressureSummonIntercepted -= OnBossPressureSummonIntercepted;
                PocketOwner.SummonFollowupWindowOpened -= OnSummonFollowupWindowOpened;
                PocketOwner.SummonFollowupHitConfirmed -= OnSummonFollowupHitConfirmed;
                PocketOwner.SummonFollowupMissed -= OnSummonFollowupMissed;
                PocketOwner.CounterWaveObserved -= OnCounterWaveObserved;
                PocketOwner.CounterWaveStabilized -= OnCounterWaveStabilized;
                PocketOwner.ResultRecordCommitted -= OnResultRecordCommitted;
            }

            private void OnPlayerDamaged(DamageInfo damageInfo)
            {
                Metrics.PlayerDamageTaken += damageInfo.Amount;
                RecordDamageResponse(
                    damageInfo,
                    () => Metrics.PlayerNonLockingDamageEvents++,
                    () => Metrics.PlayerLockingDamageEvents++,
                    () => Metrics.PlayerFullBodyEligibleDamageEvents++);
            }

            private void OnBossDamaged(DamageInfo damageInfo)
            {
                Metrics.BossDamageTaken += damageInfo.Amount;
                RecordDamageResponse(
                    damageInfo,
                    () => Metrics.BossNonLockingDamageEvents++,
                    () => Metrics.BossLockingDamageEvents++,
                    () => Metrics.BossFullBodyEligibleDamageEvents++);
            }

            private void OnCloseThreatDamaged(DamageInfo damageInfo)
            {
                Metrics.CloseThreatDamageTaken += damageInfo.Amount;
                RecordDamageResponse(
                    damageInfo,
                    () => Metrics.CloseThreatNonLockingDamageEvents++,
                    () => Metrics.CloseThreatLockingDamageEvents++,
                    () => Metrics.CloseThreatFullBodyEligibleDamageEvents++);
            }

            private static void RecordDamageResponse(
                DamageInfo damageInfo,
                Action recordNonLocking,
                Action recordLocking,
                Action recordFullBodyEligible)
            {
                if (DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy))
                {
                    recordLocking();
                }
                else
                {
                    recordNonLocking();
                }

                if (DamageResponsePolicyUtility.PlaysFullBodyHitAnimation(damageInfo))
                {
                    recordFullBodyEligible();
                }
            }

            private void OnSummonPressureBlocked(int tier)
            {
                Metrics.SummonBlocks++;
                Metrics.HighestSummonBlockTier = Mathf.Max(Metrics.HighestSummonBlockTier, tier);
                if (Metrics.FirstSummonBlockAtSeconds < 0f)
                {
                    Metrics.FirstSummonBlockAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnBossPressureSummonReleased(BossSummonPressureAction action, int tier)
            {
                Metrics.BossPressureSummonReleases++;
                Metrics.HighestBossPressureSummonTier = Mathf.Max(
                    Metrics.HighestBossPressureSummonTier,
                    tier);
                if (Metrics.FirstBossPressureReleaseAtSeconds < 0f)
                {
                    Metrics.FirstBossPressureReleaseAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnBossPressureSummonIntercepted(BossSummonPressureAction action, int tier)
            {
                Metrics.BossPressureScreenBlocks++;
                Metrics.HighestBossPressureScreenBlockTier = Mathf.Max(
                    Metrics.HighestBossPressureScreenBlockTier,
                    tier);
                if (Metrics.FirstBossPressureScreenBlockAtSeconds < 0f)
                {
                    Metrics.FirstBossPressureScreenBlockAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnSummonFollowupWindowOpened(int tier)
            {
                Metrics.FollowupWindowOpenCount++;
                Metrics.HighestFollowupWindowTier = Mathf.Max(Metrics.HighestFollowupWindowTier, tier);
                Metrics.LastFollowupWindowAtSeconds = Metrics.ElapsedSeconds;
                Metrics.LastFollowupWindowDurationSeconds = PocketOwner.LastSummonFollowupWindowDuration;
                if (Metrics.FirstFollowupWindowAtSeconds < 0f)
                {
                    Metrics.FirstFollowupWindowAtSeconds = Metrics.ElapsedSeconds;
                    Metrics.FirstFollowupWindowDurationSeconds = PocketOwner.LastSummonFollowupWindowDuration;
                }

                Metrics.LastSummonFollowupWindowDuration = PocketOwner.LastSummonFollowupWindowDuration;
            }

            private void OnSummonFollowupHitConfirmed(int tier, float damage)
            {
                Metrics.FollowupHitCount++;
                Metrics.HighestFollowupHitTier = Mathf.Max(Metrics.HighestFollowupHitTier, tier);
                Metrics.FollowupHitDamage += damage;
                if (Metrics.FirstFollowupHitAtSeconds < 0f)
                {
                    Metrics.FirstFollowupHitAtSeconds = Metrics.ElapsedSeconds;
                    Metrics.FollowupWindowAtFirstHitSeconds = Metrics.LastFollowupWindowAtSeconds;
                    Metrics.FollowupWindowDurationAtFirstHitSeconds =
                        Metrics.LastFollowupWindowDurationSeconds;
                    Metrics.FollowupWindowRemainingAtFirstHitSeconds =
                        Metrics.ResolveFollowupWindowRemainingAt(Metrics.ElapsedSeconds);
                }
            }

            private void OnSummonFollowupMissed()
            {
                Metrics.FollowupMissCount++;
                if (Metrics.FirstFollowupMissAtSeconds < 0f)
                {
                    Metrics.FirstFollowupMissAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnCounterWaveObserved(BossBarragePocketReviewOwner.CounterWaveSource source)
            {
                Metrics.CounterWaves++;
                Metrics.LastCounterWaveSource = source.ToString();
                if (Metrics.FirstCounterWaveAtSeconds < 0f)
                {
                    Metrics.FirstCounterWaveAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnCounterWaveStabilized()
            {
                Metrics.CounterStabilizedCount++;
                if (Metrics.FirstCounterStabilizedAtSeconds < 0f)
                {
                    Metrics.FirstCounterStabilizedAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnResultRecordCommitted(BossBarragePocketReviewOwner.RouteResultRecord record)
            {
                Metrics.ResultRecords++;
                Metrics.ResultKind = record.ResultKind.ToString();
                Metrics.ResultRecordElapsedSeconds = record.ElapsedSeconds;
                Metrics.ResultRecordRouteStability01 = record.RouteStability01;
                Metrics.ResultRecordRouteLabel = record.RouteLabel;
                Metrics.ResultRecordTitle = record.Title;
                Metrics.ResultRecordSummary = record.Summary;
                Metrics.ResultRecordRewardHook = record.RewardHook;
                Metrics.ResultRecordNextObjective = record.NextObjective;
                Metrics.ResultRecordProofReadout = record.ProofReadout;
                Metrics.ResultRecordDecision = $"{record.DecisionState}({record.DecisionReadout})";
                Metrics.ResultRecordCounterWaveSource = record.CounterWaveSource.ToString();
            }
        }

        private sealed class PolicyMetrics
        {
            public PolicyMetrics(PolicyKind policy)
            {
                Policy = policy;
            }

            public PolicyKind Policy { get; }
            public string ResultKind { get; set; } = "Running";
            public bool IsClearResult =>
                ResultKind == "CleanFollowupClear"
                || ResultKind == "CounterRecoveryClear"
                || ResultKind == "ClearedNoRecord";
            public float ElapsedSeconds { get; set; }
            public float PlayerHealthStart { get; set; }
            public float PlayerHealthRemaining { get; set; }
            public float PlayerDamageTaken { get; set; }
            public float FirstPlayerDownAtSeconds { get; set; } = -1f;
            public float BossHealthStart { get; set; }
            public float BossHealthRemaining { get; set; }
            public float BossDamageTaken { get; set; }
            public float FirstBossDownAtSeconds { get; set; } = -1f;
            public float CloseThreatHealthStart { get; set; }
            public float CloseThreatHealthRemaining { get; set; }
            public float CloseThreatDamageTaken { get; set; }
            public float SurvivalProbeMaxSeconds { get; set; }
            public int BasicShots { get; set; }
            public int CloseThreatBasicHits { get; set; }
            public int BossBasicHits { get; set; }
            public int BossWaves { get; set; }
            public int BossProjectilesSpawned { get; set; }
            public int BossProjectilesHitPlayer { get; set; }
            public int SummonUses { get; set; }
            public int SummonBlocks { get; set; }
            public int HighestSummonBlockTier { get; set; }
            public int SkillUses { get; set; }
            public int SkillProjectileHits { get; set; }
            public int SkillProjectilesBlockedByBossScreen { get; set; }
            public int BossPressureSummonReleases { get; set; }
            public int HighestBossPressureSummonTier { get; set; }
            public int BossPressureScreenBlocks { get; set; }
            public int HighestBossPressureScreenBlockTier { get; set; }
            public int MaxBossPressureActiveScreenCount { get; set; }
            public int BossPressureActiveScreenRemainingIntercepts { get; set; }
            public float FirstSummonUseAtSeconds { get; set; } = -1f;
            public float FirstSummonBlockAtSeconds { get; set; } = -1f;
            public float FirstBossPressureReleaseAtSeconds { get; set; } = -1f;
            public float FirstBossPressureScreenBlockAtSeconds { get; set; } = -1f;
            public float FirstSkill1UseAtSeconds { get; set; } = -1f;
            public float FirstCounterAnswerSummonAtSeconds { get; set; } = -1f;
            public float FirstCounterFinalWindowAtSeconds { get; set; } = -1f;
            public float SummonUseToBlockSeconds =>
                ResolveTimingDelta(FirstSummonUseAtSeconds, FirstSummonBlockAtSeconds);
            public float BlockToFollowupWindowSeconds =>
                ResolveTimingDelta(FirstSummonBlockAtSeconds, FirstFollowupWindowAtSeconds);
            public float FollowupWindowToSkillUseSeconds =>
                ResolveTimingDelta(FirstFollowupWindowAtSeconds, FirstSkill1UseAtSeconds);
            public float FollowupWindowToHitSeconds =>
                ResolveTimingDelta(FirstFollowupWindowAtSeconds, FirstFollowupHitAtSeconds);
            public float BlockToFollowupHitSeconds =>
                ResolveTimingDelta(FirstSummonBlockAtSeconds, FirstFollowupHitAtSeconds);
            public float FollowupHitWindowDelaySeconds =>
                ResolveTimingDelta(FollowupWindowAtFirstHitSeconds, FirstFollowupHitAtSeconds);
            public float FollowupHitWindowUsedShare01 =>
                FollowupWindowDurationAtFirstHitSeconds > 0f
                    && FollowupHitWindowDelaySeconds >= 0f
                    ? Mathf.Clamp01(FollowupHitWindowDelaySeconds / FollowupWindowDurationAtFirstHitSeconds)
                    : -1f;
            public float BossScreenReleaseToBlockSeconds =>
                ResolveTimingDelta(FirstBossPressureReleaseAtSeconds, FirstBossPressureScreenBlockAtSeconds);
            public float CounterTriggerToAnswerSeconds =>
                ResolveTimingDelta(FirstCounterWaveAtSeconds, FirstCounterAnswerSummonAtSeconds);
            public float CounterAnswerToStableSeconds =>
                ResolveTimingDelta(FirstCounterAnswerSummonAtSeconds, FirstCounterStabilizedAtSeconds);
            public float CounterStableToFinalWindowSeconds =>
                ResolveTimingDelta(FirstCounterStabilizedAtSeconds, FirstCounterFinalWindowAtSeconds);
            public float FinalWindowToHitSeconds =>
                ResolveTimingDelta(FirstCounterFinalWindowAtSeconds, FirstFollowupHitAtSeconds);
            public int CounterWaves { get; set; }
            public int ResultRecords { get; set; }
            public int MaxEnemyFrontlineCount { get; set; }
            public int MaxAllyFrontlineCount { get; set; }
            public float RouteDrainAccumulated01 { get; set; }
            public int UnansweredBossHitRoutePenaltyCount { get; set; }
            public float LastUnansweredBossHitRoutePenalty01 { get; set; }
            public float TotalUnansweredBossHitRoutePenalty01 { get; set; }
            public float MaxRouteStabilityDrainPerSecond { get; set; }
            public float RoutePressureWeightSeconds { get; set; }
            public float FrontlinePresenceScaleSeconds { get; set; }
            public float EnemyOnlyFrontlineSeconds { get; set; }
            public float ContestedFrontlineSeconds { get; set; }
            public float AllyOnlyFrontlineSeconds { get; set; }
            public string FrontlinePresenceReadout { get; set; } = "pressure x1.00 open";
            public float EnergyProbeTargetForwardRisk01 { get; set; } = -1f;
            public float EnergyProbeStartAtSeconds { get; set; } = -1f;
            public float EnergyTier1ReadyAtSeconds { get; set; } = -1f;
            public float EnergyTier1DurationSeconds =>
                ResolveTimingDelta(EnergyProbeStartAtSeconds, EnergyTier1ReadyAtSeconds);
            public int EnergyChargingTier { get; set; }
            public int EnergyAvailableTier { get; set; }
            public float EnergyFillRatio { get; set; }
            public float LastEnergyForwardRisk01 { get; set; }
            public float LastEnergyGainMultiplier { get; set; }
            public string LastEnergyRiskBand { get; set; } = "Unknown";
            public float EnergySampleSeconds { get; set; }
            public float EnergyForwardRiskSeconds { get; set; }
            public float EnergyGainMultiplierSeconds { get; set; }
            public float BackSafetyBandSeconds { get; set; }
            public float MidChargeBandSeconds { get; set; }
            public float ForwardRiskBandSeconds { get; set; }
            public float AverageEnergyForwardRisk01 =>
                EnergySampleSeconds > 0f ? EnergyForwardRiskSeconds / EnergySampleSeconds : 0f;
            public float AverageEnergyGainMultiplier =>
                EnergySampleSeconds > 0f ? EnergyGainMultiplierSeconds / EnergySampleSeconds : 0f;
            public float BarrageShapeProbeTargetForwardRisk01 { get; set; } = -1f;
            public float BarrageShapePendingForwardRisk01 { get; set; } = -1f;
            public string BarrageShapePatternId { get; set; } = "-";
            public int BarrageShapeProjectileCount { get; set; }
            public int BarrageShapeNearProjectileCount { get; set; }
            public float BarrageShapeAverageLateralGap { get; set; } = -1f;
            public float BarrageShapeAverageDepthGap { get; set; } = -1f;
            public float BarrageShapeAverageLaneDistance { get; set; } = -1f;
            public float BarrageShapeNearestLaneDistance { get; set; } = -1f;
            public float BarrageShapeThreatDensity { get; set; } = -1f;
            public string BarrageShapeReadout { get; set; } = "none";
            public float PhysicalBarrageProbeTargetForwardRisk01 { get; set; } = -1f;
            public float PhysicalBarragePendingForwardRisk01 { get; set; } = -1f;
            public string PhysicalBarragePatternId { get; set; } = "-";
            public int PhysicalBarrageWaves { get; set; }
            public int PhysicalBarrageProjectilesSpawned { get; set; }
            public int PhysicalBarrageTrackedProjectileCount { get; set; }
            public int PhysicalBarrageInactiveAfterFlight { get; set; }
            public int PhysicalBarragePlayerImpactAttempts { get; set; }
            public int PhysicalBarragePlayerHits { get; set; }
            public float PhysicalBarragePlayerDamage { get; set; }
            public string PhysicalBarrageReadout { get; set; } = "none";
            public float PressureWindowSeconds { get; set; }
            public float PressureBurdenSeconds { get; set; }
            public int PressureWindowCount { get; set; }
            public float PeakPressureWindowShare01 { get; set; }
            public float Top3PressureWindowShare01 { get; set; }
            public float PeakPressureWindowStartSeconds { get; set; } = -1f;
            public float TimeToNextReliefWindowSeconds { get; set; } = -1f;
            public string DominantPressureBurdenState { get; set; } = "none";
            public float DominantPressureBurdenShare01 { get; set; }
            public float UnansweredPressureBurdenShare01 { get; set; }
            public float AnsweredPressureBurdenShare01 { get; set; }
            public string PressureWindowReadout { get; set; } = "none";
            public int EnemyFrontlineClashes { get; set; }
            public int EnemyFrontlineBodyHits { get; set; }
            public int EnemyFrontlineSummonHits { get; set; }
            public float EnemyFrontlineClashDamage { get; set; }
            public float EnemyFrontlineBodyDamage { get; set; }
            public int MaxEnemyFrontlineEngagedCount { get; set; }
            public int MaxEnemyFrontlineAttackingCount { get; set; }
            public int AllyFrontlineClashes { get; set; }
            public int AllyFrontlineBodyHits { get; set; }
            public int AllyFrontlineSummonHits { get; set; }
            public float AllyFrontlineClashDamage { get; set; }
            public int MaxAllyFrontlineEngagedCount { get; set; }
            public int MaxAllyFrontlineAttackingCount { get; set; }
            public int EnemySummonDamageFlashes { get; set; }
            public int EnemySummonFullBodyHitReactions { get; set; }
            public int EnemySummonSuppressedHitReactions { get; set; }
            public int EnemySummonNonLockingDamageCues { get; set; }
            public int EnemySummonLockingDamageCues { get; set; }
            public int AllySummonDamageFlashes { get; set; }
            public int AllySummonFullBodyHitReactions { get; set; }
            public int AllySummonSuppressedHitReactions { get; set; }
            public int AllySummonNonLockingDamageCues { get; set; }
            public int AllySummonLockingDamageCues { get; set; }
            public int TotalSummonDamageFlashes => EnemySummonDamageFlashes + AllySummonDamageFlashes;
            public int TotalSummonFullBodyHitReactions =>
                EnemySummonFullBodyHitReactions + AllySummonFullBodyHitReactions;
            public int TotalNonLockingSummonDamageCues =>
                EnemySummonNonLockingDamageCues + AllySummonNonLockingDamageCues;
            public int PlayerNonLockingDamageEvents { get; set; }
            public int PlayerLockingDamageEvents { get; set; }
            public int PlayerFullBodyEligibleDamageEvents { get; set; }
            public int BossNonLockingDamageEvents { get; set; }
            public int BossLockingDamageEvents { get; set; }
            public int BossFullBodyEligibleDamageEvents { get; set; }
            public int CloseThreatNonLockingDamageEvents { get; set; }
            public int CloseThreatLockingDamageEvents { get; set; }
            public int CloseThreatFullBodyEligibleDamageEvents { get; set; }
            public int FollowupBlockOpportunityScreenCueRequests { get; set; }
            public int FollowupWindowScreenCueRequests { get; set; }
            public int FollowupHitScreenCueRequests { get; set; }
            public int FollowupMissedScreenCueRequests { get; set; }
            public string LastFollowupScreenCueId { get; set; } = string.Empty;
            public float LastFollowupScreenCueIntensity { get; set; }
            public float LastFollowupHitScreenCueIntensity { get; set; }
            public float LastFollowupWindowRouteScale { get; set; } = 1f;
            public int FollowupWindowCameraCueRequests { get; set; }
            public int FollowupHitCameraCueRequests { get; set; }
            public int FollowupMissedCameraCueRequests { get; set; }
            public int LastFollowupHitCameraTier { get; set; }
            public float LastFollowupHitCameraDamage { get; set; }
            public int FollowupWindowVfxCueRequests { get; set; }
            public int FollowupHitVfxCueRequests { get; set; }
            public int FollowupMissedVfxCueRequests { get; set; }
            public int LastFollowupHitVfxTier { get; set; }
            public float LastFollowupHitVfxDamage { get; set; }
            public int FollowupWindowCinematicCueRequests { get; set; }
            public int FollowupHitCinematicCueRequests { get; set; }
            public int FollowupMissedCinematicCueRequests { get; set; }
            public int FollowupHitCinematicFrameOverlayCount { get; set; }
            public int LastFollowupHitCinematicTier { get; set; }
            public string LastFollowupHitCinematicCueId { get; set; } = string.Empty;
            public int FollowupWindowSequenceBridgeRequests { get; set; }
            public int FollowupHitSequenceBridgeRequests { get; set; }
            public int FollowupMissedSequenceBridgeRequests { get; set; }
            public int LastFollowupHitSequenceTier { get; set; }
            public string LastFollowupHitSequenceProfile { get; set; } = string.Empty;
            public float RouteStability01 { get; set; }
            public float MinRouteStability01 { get; set; } = 1f;
            public string RouteStabilityBand { get; set; } = "Unknown";
            public string RouteProofState { get; set; } = "unknown";
            public string RouteProofReadout { get; set; } = string.Empty;
            public string LastCounterWaveSource { get; set; } = "none";
            public string CounterWaveSource { get; set; } = "none";
            public string CounterWaveRecordState { get; set; } = "pending";
            public string CounterWaveAnswerState { get; set; } = "pending";
            public string CounterWaveAnswerReadout { get; set; } = "none";
            public string CounterWaveFinalWindowState { get; set; } = "pending";
            public string CounterWaveFinalWindowReadout { get; set; } = "none";
            public float CounterWaveEntryPenalty01 { get; set; }
            public float CounterWaveStabilityBonus01 { get; set; }
            public float CounterWaveFinalWindowSeconds { get; set; }
            public float CounterWaveFinalWindowRouteScale { get; set; } = 1f;
            public float CounterWaveAllyHoldRequiredSeconds { get; set; }
            public float CounterWaveAllyHoldElapsedSeconds { get; set; }
            public float CounterWaveAllyHoldProgress01 { get; set; }
            public float CounterWaveAnswerEnergyPulse { get; set; }
            public int CounterStabilizedCount { get; set; }
            public float FirstCounterWaveAtSeconds { get; set; } = -1f;
            public float FirstCounterStabilizedAtSeconds { get; set; } = -1f;
            public int FollowupWindowOpenCount { get; set; }
            public int FollowupHitCount { get; set; }
            public int FollowupMissCount { get; set; }
            public int HighestFollowupWindowTier { get; set; }
            public int HighestFollowupHitTier { get; set; }
            public float FollowupHitDamage { get; set; }
            public float FirstFollowupWindowAtSeconds { get; set; } = -1f;
            public float FirstFollowupHitAtSeconds { get; set; } = -1f;
            public float FirstFollowupMissAtSeconds { get; set; } = -1f;
            public float FirstFollowupWindowDurationSeconds { get; set; } = -1f;
            public float LastFollowupWindowAtSeconds { get; set; } = -1f;
            public float LastFollowupWindowDurationSeconds { get; set; } = -1f;
            public float FollowupWindowAtFirstHitSeconds { get; set; } = -1f;
            public float FollowupWindowDurationAtFirstHitSeconds { get; set; } = -1f;
            public float FollowupWindowRemainingAtFirstHitSeconds { get; set; } = -1f;
            public float SummonFollowupWindowRemainingSeconds { get; set; }
            public float SummonFollowupEnergyPulse { get; set; }
            public float LastSummonFollowupWindowDuration { get; set; }
            public int HighestSummonFollowupSkillTier { get; set; }
            public int HighestSkill1FollowupHitTier { get; set; }
            public float Skill1FollowupDamage { get; set; }
            public bool BossBlockedSkill1Followup { get; set; }
            public int BossPressureBlocksDuringSummonFollowup { get; set; }
            public bool CleanFollowupConfirmed { get; set; }
            public bool CounterRecoveryConfirmed { get; set; }
            public float ResultRecordElapsedSeconds { get; set; }
            public float ResultRecordRouteStability01 { get; set; }
            public string ResultRecordRouteLabel { get; set; } = string.Empty;
            public string ResultRecordTitle { get; set; } = string.Empty;
            public string ResultRecordSummary { get; set; } = string.Empty;
            public string ResultRecordRewardHook { get; set; } = string.Empty;
            public string ResultRecordNextObjective { get; set; } = string.Empty;
            public string ResultRecordProofReadout { get; set; } = string.Empty;
            public string ResultRecordDecision { get; set; } = string.Empty;
            public string ResultRecordCounterWaveSource { get; set; } = "None";
            public string RouteDecision { get; set; } = "unknown";
            public string CompletionReadout { get; set; } = string.Empty;
            public List<string> Notes { get; } = new List<string>();

            private static float ResolveTimingDelta(float startSeconds, float endSeconds)
            {
                if (startSeconds < 0f || endSeconds < 0f || endSeconds < startSeconds)
                {
                    return -1f;
                }

                return endSeconds - startSeconds;
            }

            public float ResolveFollowupWindowRemainingAt(float elapsedSeconds)
            {
                if (LastFollowupWindowAtSeconds < 0f
                    || LastFollowupWindowDurationSeconds <= 0f
                    || elapsedSeconds < LastFollowupWindowAtSeconds)
                {
                    return -1f;
                }

                return Mathf.Max(
                    0f,
                    LastFollowupWindowDurationSeconds - (elapsedSeconds - LastFollowupWindowAtSeconds));
            }
        }
    }
}
