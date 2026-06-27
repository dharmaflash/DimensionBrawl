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
        private const float CloseProbePhysicalFireFlightSeconds = 1.2f;
        private const float CloseProbeScreenCurtainObservationSeconds = 6f;
        private const float PhysicalSkill1ProbeFlightSeconds = 2.2f;
        private const float PhysicalNoPunishObservationSeconds = 8f;
        private const float SurvivalLimitProbeMaxSeconds = 45f;
        private const int RepeatabilityProbeRuns = 3;
        private const int CloseProbePhysicalFireMaxShots = 10;

        private enum PolicyKind
        {
            NoSummonNoFire,
            GunOnly,
            CloseProbeSelectorBiasProbe,
            CloseProbePhysicalFireProbe,
            CloseProbePhysicalThenScreenCurtainProbe,
            CloseProbePhysicalThenSummonPunishProbe,
            BossTunnelVisionIgnoresCloseProbe,
            NoSummonSurvivalLimit,
            GunOnlySurvivalLimit,
            PrematureSkill1NoSummon,
            BacklineEnergyProbe,
            ForwardRiskEnergyProbe,
            BacklineBarrageProbe,
            ForwardRiskBarrageProbe,
            BacklinePhysicalBarrageProbe,
            ForwardRiskPhysicalBarrageProbe,
            ForwardRiskPhysicalSummonBlockProbe,
            ForwardRiskPhysicalSummonNoPunishProbe,
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

        private static readonly PolicyKind[] ReportPolicyOrder =
        {
            PolicyKind.NoSummonNoFire,
            PolicyKind.GunOnly,
            PolicyKind.CloseProbeSelectorBiasProbe,
            PolicyKind.CloseProbePhysicalFireProbe,
            PolicyKind.CloseProbePhysicalThenScreenCurtainProbe,
            PolicyKind.CloseProbePhysicalThenSummonPunishProbe,
            PolicyKind.BossTunnelVisionIgnoresCloseProbe,
            PolicyKind.NoSummonSurvivalLimit,
            PolicyKind.GunOnlySurvivalLimit,
            PolicyKind.PrematureSkill1NoSummon,
            PolicyKind.BacklineEnergyProbe,
            PolicyKind.ForwardRiskEnergyProbe,
            PolicyKind.BacklineBarrageProbe,
            PolicyKind.ForwardRiskBarrageProbe,
            PolicyKind.BacklinePhysicalBarrageProbe,
            PolicyKind.ForwardRiskPhysicalBarrageProbe,
            PolicyKind.ForwardRiskPhysicalSummonBlockProbe,
            PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe,
            PolicyKind.ForwardRiskPhysicalSummonPunishProbe,
            PolicyKind.IntendedRoute,
            PolicyKind.IntendedDelayedFollowup,
            PolicyKind.LateSummon,
            PolicyKind.MissedFollowupCounterRecovery,
            PolicyKind.BossScreenBlockedFollowup,
            PolicyKind.BossScreenIgnoredNoRecovery,
            PolicyKind.BossScreenBlockCounterRecovery,
            PolicyKind.BossScreenDelayedCounterRecovery
        };

        private static readonly PolicyKind[] RepeatabilityPolicyOrder =
        {
            PolicyKind.NoSummonSurvivalLimit,
            PolicyKind.GunOnlySurvivalLimit,
            PolicyKind.CloseProbeSelectorBiasProbe,
            PolicyKind.CloseProbePhysicalFireProbe,
            PolicyKind.CloseProbePhysicalThenScreenCurtainProbe,
            PolicyKind.CloseProbePhysicalThenSummonPunishProbe,
            PolicyKind.BossTunnelVisionIgnoresCloseProbe,
            PolicyKind.PrematureSkill1NoSummon,
            PolicyKind.BacklinePhysicalBarrageProbe,
            PolicyKind.ForwardRiskPhysicalBarrageProbe,
            PolicyKind.ForwardRiskPhysicalSummonBlockProbe,
            PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe,
            PolicyKind.ForwardRiskPhysicalSummonPunishProbe,
            PolicyKind.BossScreenIgnoredNoRecovery,
            PolicyKind.BossScreenBlockCounterRecovery
        };

        [UnityTest]
        public IEnumerator WritesFrontlineCombatPolicyReport()
        {
            float previousTimeScale = Time.timeScale;
            Time.timeScale = 8f;
            List<PolicyMetrics> results = new List<PolicyMetrics>();
            List<PolicyMetrics> repeatabilityResults = new List<PolicyMetrics>();

            try
            {
                for (int i = 0; i < ReportPolicyOrder.Length; i++)
                {
                    yield return RunPolicySample(ReportPolicyOrder[i], results);
                }

                for (int repeatIndex = 0; repeatIndex < RepeatabilityProbeRuns; repeatIndex++)
                {
                    for (int i = 0; i < RepeatabilityPolicyOrder.Length; i++)
                    {
                        yield return RunPolicySample(RepeatabilityPolicyOrder[i], repeatabilityResults);
                    }
                }

                WriteReports(results, repeatabilityResults);

                PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
                PolicyMetrics delayedIntended = RequireResult(results, PolicyKind.IntendedDelayedFollowup);
                PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
                PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
                PolicyMetrics selectorProbe = RequireResult(results, PolicyKind.CloseProbeSelectorBiasProbe);
                PolicyMetrics physicalCloseFire = RequireResult(results, PolicyKind.CloseProbePhysicalFireProbe);
                PolicyMetrics physicalCloseCurtain =
                    RequireResult(results, PolicyKind.CloseProbePhysicalThenScreenCurtainProbe);
                PolicyMetrics physicalCloseChain =
                    RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe);
                PolicyMetrics bossTunnel = RequireResult(results, PolicyKind.BossTunnelVisionIgnoresCloseProbe);
                PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
                PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
                PolicyMetrics prematureSkill1 = RequireResult(results, PolicyKind.PrematureSkill1NoSummon);
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
                PolicyMetrics forwardRiskPhysicalSummonNoPunish = RequireResult(
                    results,
                    PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
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
                Assert.IsTrue(
                    markdown.Contains("## Stage Result Hook Contract"),
                    "The report should expose clean/counter/fail result hooks before route details.");
                Assert.IsTrue(
                    markdown.Contains("Result copy"),
                    "The report should expose result copy so stage hooks do not collapse into reward-only logging.");
                Assert.IsTrue(
                    markdown.Contains("## Policy Repeatability Gate"),
                    "The report should include repeated-run evidence before detailed policy tables.");
                AssertRepeatabilityGate(repeatabilityResults);
                Assert.IsTrue(
                    markdown.Contains("## Stage/Wave Beat Map"),
                    "The report should classify each route by the ArkData-style stage/wave beat it reaches.");
                Assert.IsTrue(
                    markdown.Contains("## Target Priority Contract"),
                    "The report should prove boss tunnel vision is not the same as clearing the close-probe target.");
                Assert.IsTrue(
                    markdown.Contains("## Local-Defense Selector Probe"),
                    "The report should prove the close-probe target is visible to runtime selection before forced effects.");
                Assert.IsTrue(
                    markdown.Contains("## Player Damage Presentation Bridge"),
                    "The report should prove player damage presentation instead of inferring it from HP loss.");
                Assert.IsTrue(
                    markdown.Contains("## Counter Wave Presentation Bridge"),
                    "The report should prove counter-wave presentation instead of inferring it from follow-up misses.");
                Assert.IsTrue(
                    markdown.Contains("## Energy Presentation Bridge"),
                    "The report should prove summon energy presentation instead of inferring it from resource numbers.");
                Assert.IsTrue(
                    markdown.Contains("## Skill Gate Contract"),
                    "The report should prove raw Skill1 hits are not the same as state-gated follow-up commits.");
                AssertStageWaveBeatMap(results);
                Assert.Greater(intended.SummonBlocks, 0, "The intended route must prove summon interception changes the run.");
                Assert.AreEqual(
                    0,
                    noSummon.ResultRecords,
                    "A short no-action route should not fabricate a clear or payout result while still running.");
                Assert.AreEqual(
                    0,
                    gunOnly.ResultRecords,
                    "A short gun-only route should not fabricate a clear or payout result while still running.");
                Assert.Greater(
                    selectorProbe.SelectorCandidateCount,
                    1,
                    "The selector probe should see both the close-probe target and the boss proxy as authored candidates.");
                Assert.AreEqual(
                    "CloseProbe",
                    selectorProbe.SelectorDefaultTarget,
                    "The runtime selector should default to the close-probe target before boss chip is useful.");
                Assert.AreEqual(
                    "CloseProbe",
                    selectorProbe.SelectorAttackAimTarget,
                    "The local-defense attack aim should bias toward the close-probe target without a hard-lock UI.");
                Assert.Greater(
                    selectorProbe.SelectorBossDistance,
                    selectorProbe.SelectorCloseDistance,
                    "The selector probe should preserve the close-before-boss distance read.");
                Assert.Greater(
                    physicalCloseFire.CloseThreatPhysicalProjectileHits,
                    0,
                    "The physical close-fire probe should prove real ranged projectiles can hit the close-probe target.");
                Assert.LessOrEqual(
                    physicalCloseFire.CloseThreatHealthRemaining,
                    0.01f,
                    "The physical close-fire probe should defeat the close-probe target without manual impact shortcuts.");
                Assert.AreEqual(
                    0f,
                    physicalCloseFire.BossDamageFromPlayer,
                    0.01f,
                    "Physical close-fire should not drift into a boss DPS route.");
                Assert.LessOrEqual(
                    physicalCloseCurtain.CloseThreatHealthRemaining,
                    0.01f,
                    "The close-to-curtain probe should begin from the same physical close-probe answer.");
                Assert.Greater(
                    physicalCloseCurtain.BossPressureSummonReleases,
                    0,
                    "A physical close-probe answer should advance into the summon-needed screen-curtain pressure slot.");
                Assert.Greater(
                    physicalCloseCurtain.MaxBossPressureActiveScreenCount,
                    0,
                    "The post-close screen curtain should exist as a real pressure screen, not only a stage label.");
                Assert.AreEqual(
                    "ScreenCurtain",
                    ResolveFirstUnresolvedBeat(physicalCloseCurtain),
                    "After the physical close answer, the first unresolved beat should become the screen curtain.");
                Assert.AreEqual(
                    0,
                    physicalCloseCurtain.ResultRecords,
                    "The close-to-curtain probe should not fabricate a committed result before the summon answer.");
                Assert.LessOrEqual(
                    physicalCloseChain.CloseThreatHealthRemaining,
                    0.01f,
                    "The live close-chain route should begin by physically defeating the close probe.");
                Assert.Greater(
                    physicalCloseChain.BossPressureSummonReleases,
                    0,
                    "The live close-chain route should continue into the summon-needed curtain pressure slot.");
                Assert.Greater(
                    physicalCloseChain.SummonBlocks,
                    0,
                    "The live close-chain route should answer the curtain with a real summon block.");
                Assert.Greater(
                    physicalCloseChain.SkillProjectileHits,
                    0,
                    "The live close-chain route should turn that block into a Skill1 punish.");
                Assert.Greater(
                    physicalCloseChain.FollowupHitCount,
                    0,
                    "The live close-chain route should count the Skill1 hit as a state-gated follow-up confirm.");
                Assert.AreEqual(
                    "CleanFollowupClear",
                    physicalCloseChain.ResultKind,
                    "Live close fire -> curtain block -> Skill1 should close as the clean stage route.");
                Assert.Greater(
                    bossTunnel.BossBasicHits,
                    0,
                    "The boss tunnel-vision policy should prove the player can spend basic fire on the boss.");
                Assert.AreEqual(
                    0,
                    bossTunnel.CloseThreatBasicHits,
                    "The boss tunnel-vision policy should leave the close probe unresolved.");
                Assert.AreEqual(
                    "CloseProbe",
                    ResolveFirstUnresolvedBeat(bossTunnel),
                    "Boss tunnel vision should stall at the first target-priority beat, not at the summon curtain.");
                Assert.AreEqual(
                    0,
                    bossTunnel.ResultRecords,
                    "Boss tunnel vision should not fabricate a stage result hook.");
                Assert.Greater(
                    gunOnly.CloseThreatBasicHits,
                    0,
                    "Gun-only should first prove the close probe can be answered with local-defense fire.");
                Assert.LessOrEqual(
                    gunOnly.CloseThreatHealthRemaining,
                    0.01f,
                    "Gun-only should defeat the close probe before moving on to boss chip.");
                Assert.Greater(
                    gunOnly.CloseThreatNonLockingDamageEvents,
                    0,
                    "Close-probe local-defense hits should produce readable damage events.");
                Assert.AreEqual(
                    0,
                    gunOnly.CloseThreatLockingDamageEvents,
                    "Close-probe local-defense fire should not masquerade as a major locking punish.");
                Assert.AreEqual(
                    0,
                    gunOnly.CloseThreatFullBodyEligibleDamageEvents,
                    "Close-probe local-defense fire should stay out of the full-body major-hit lane.");
                Assert.Greater(
                    prematureSkill1.SkillUses,
                    0,
                    "The premature Skill1 probe should spend the resource and fire the skill before a summon answer.");
                Assert.Greater(
                    prematureSkill1.SkillProjectileHits,
                    0,
                    "The premature Skill1 probe should prove that raw projectile damage alone is not enough to commit the route.");
                Assert.AreEqual(
                    0,
                    prematureSkill1.FollowupHitCount,
                    "A Skill1 fired without the summon-opened state must not be counted as a follow-up hit.");
                Assert.AreEqual(
                    0,
                    prematureSkill1.FollowupHitScreenCueRequests,
                    "A Skill1 fired without the summon-opened state must not request Followup.Hit presentation.");
                Assert.AreEqual(
                    0,
                    prematureSkill1.ResultRecords,
                    "A Skill1 fired without the summon-opened state must not commit the stage result hook.");
                Assert.IsFalse(
                    prematureSkill1.IsClearResult,
                    "A Skill1 fired without the summon-opened state must remain a stalled route, not a clean clear.");
                AssertStageResultHook(
                    noSummonSurvival,
                    "PlayerDownFail",
                    "Failure analysis logged",
                    "protect HP",
                    "No-summon survival should commit a failure-analysis hook, not a reward payout.");
                AssertStageResultCopy(
                    noSummonSurvival,
                    "PLAYER DOWN",
                    "HP",
                    "No-summon failure copy should name the HP survival failure, not a generic mission failure.");
                AssertStageResultHook(
                    gunOnlySurvival,
                    "PlayerDownFail",
                    "Failure analysis logged",
                    "protect HP",
                    "Gun-only survival should commit a failure-analysis hook, not a reward payout.");
                AssertStageResultCopy(
                    gunOnlySurvival,
                    "PLAYER DOWN",
                    "HP",
                    "Gun-only failure copy should name the HP survival failure, not a generic mission failure.");
                AssertStageResultHook(
                    forwardRiskPhysicalSummonPunish,
                    "CleanFollowupClear",
                    "Clean survival logged",
                    "counter pressure",
                    "The physical clean route should commit the clean survival hook.");
                AssertStageResultCopy(
                    forwardRiskPhysicalSummonPunish,
                    "PRESSURE BROKEN",
                    "Skill1",
                    "The physical clean route result should name the pressure break and Skill1 confirm.");
                AssertStageResultCopy(
                    physicalCloseChain,
                    "PRESSURE BROKEN",
                    "Skill1",
                    "The live close-chain result should name the pressure break and Skill1 confirm.");
                AssertStageResultHook(
                    blockedRecovery,
                    "CounterRecoveryClear",
                    "Counter recovery logged",
                    "earlier",
                    "The boss-screen recovery route should commit the counter-recovery hook.");
                AssertStageResultCopy(
                    blockedRecovery,
                    "PRESSURE BROKEN",
                    "Counter pressure",
                    "The boss-screen recovery result should name the counter-pressure answer.");
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
                    forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests,
                    0,
                    "The forward-risk energy probe should request the existing forward-risk screen cue.");
                Assert.Greater(
                    forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests,
                    0,
                    "The forward-risk energy probe should request the existing forward-risk VFX cue.");
                Assert.Greater(
                    forwardRiskEnergy.EnergyReadyScreenCueRequests,
                    0,
                    "The forward-risk energy probe should request an energy-ready screen cue when LV1 becomes available.");
                Assert.Greater(
                    forwardRiskEnergy.EnergyReadyVfxCueRequests,
                    0,
                    "The forward-risk energy probe should request an energy-ready VFX cue when LV1 becomes available.");
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
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonBlock.SummonPressureBlockCameraCueRequests,
                    forwardRiskPhysicalSummonBlock.SummonBlocks,
                    "A physical summon block should request a camera cue for each block so the block itself reads.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptFlashes,
                    forwardRiskPhysicalSummonBlock.SummonBlocks,
                    "A physical summon block should flash the pressure screen for each intercept.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptVfxCueRequests,
                    forwardRiskPhysicalSummonBlock.SummonBlocks,
                    "A physical summon block should request the reviewed intercept VFX cue for each block.");
                Assert.Greater(
                    forwardRiskPhysicalSummonNoPunish.FollowupMissCount,
                    0,
                    "A physical summon block without Skill1 should eventually miss the opened follow-up window.");
                Assert.Greater(
                    forwardRiskPhysicalSummonNoPunish.CounterWaves,
                    0,
                    "A missed physical follow-up should enter counter pressure instead of silently succeeding.");
                Assert.Greater(
                    forwardRiskPhysicalSummonNoPunish.CounterWaveScreenCueRequests,
                    0,
                    "A missed physical follow-up should request the existing counter-wave screen cue.");
                Assert.Greater(
                    forwardRiskPhysicalSummonNoPunish.CounterWaveCameraCueRequests,
                    0,
                    "A missed physical follow-up should request the existing counter-wave camera cue.");
                Assert.Greater(
                    forwardRiskPhysicalSummonNoPunish.CounterWaveVfxCueRequests,
                    0,
                    "A missed physical follow-up should request the existing counter-wave VFX cue.");
                Assert.AreEqual(
                    0,
                    forwardRiskPhysicalSummonNoPunish.SkillProjectileHits,
                    "The no-punish probe should not record a Skill1 hit.");
                Assert.AreEqual(
                    0f,
                    forwardRiskPhysicalSummonNoPunish.BossDamageFromPlayer,
                    0.01f,
                    "The no-punish probe should not attribute boss payoff to the player.");
                Assert.IsFalse(
                    forwardRiskPhysicalSummonNoPunish.IsClearResult,
                    "A summon block without the player-authored Skill1 confirm should not clear.");
                Assert.AreEqual(
                    0,
                    forwardRiskPhysicalSummonNoPunish.ResultRecords,
                    "A summon block without Skill1 should not commit a clean/counter/fail result during the observation window.");
                Assert.Greater(
                    forwardRiskPhysicalSummonNoPunish.UnansweredPressureBurdenShare01,
                    forwardRiskPhysicalSummonPunish.UnansweredPressureBurdenShare01,
                    "Leaving the physical follow-up unconfirmed should carry more unanswered pressure burden than the clean punish route.");
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
                    0f,
                    forwardRiskPhysicalSummonBlock.BossDamageFromPlayer,
                    0.01f,
                    "The physical block-only probe should not count player-authored boss damage before Skill1.");
                Assert.Greater(
                    forwardRiskPhysicalSummonPunish.BossDamageFromPlayer,
                    0f,
                    "A physical summon-punish route should attribute boss payoff to the player Skill1 hit.");
                Assert.Greater(
                    forwardRiskPhysicalSummonPunish.BossDamageFromPlayer,
                    forwardRiskPhysicalSummonBlock.BossDamageFromPlayer,
                    "Block -> Skill1 should create a player-authored boss payoff that block-only cannot provide.");
                Assert.Less(
                    forwardRiskPhysicalSummonBlock.BossDamageFromAllySummon,
                    forwardRiskPhysicalSummonPunish.BossDamageFromPlayer,
                    "The summon block-only route should not deal as much boss damage as the player-authored Skill1 payoff.");
                Assert.LessOrEqual(
                    forwardRiskPhysicalSummonBlock.BossDamageFromAllySummon,
                    forwardRiskPhysicalSummonPunish.BossDamageFromPlayer * 0.60f,
                    "The summon block-only route should stay visibly below the committed player-authored Skill1 payoff.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonPunish.BossDamagePlayerShare01,
                    0.72f,
                    "After a successful block, boss payoff should read primarily as the player's Skill1 punish, not summon auto-DPS.");
                Assert.AreEqual(
                    "CleanFollowupClear",
                    forwardRiskPhysicalSummonPunish.ResultKind,
                    "A physical summon-punish route should close the block -> follow-up -> Skill1 loop as a clean route.");
                Assert.AreEqual(
                    0,
                    forwardRiskPhysicalSummonPunish.CounterWaveScreenCueRequests,
                    "A clean physical summon-punish route should not request counter-wave presentation.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonPunish.SummonPressureBlockCameraCueRequests,
                    forwardRiskPhysicalSummonPunish.SummonBlocks,
                    "The physical summon-punish route should preserve block camera reads before the Skill1 confirm.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonPunish.SummonPressureScreenInterceptFlashes,
                    forwardRiskPhysicalSummonPunish.SummonBlocks,
                    "The physical summon-punish route should preserve pressure-screen intercept flashes before the Skill1 confirm.");
                Assert.GreaterOrEqual(
                    forwardRiskPhysicalSummonPunish.SummonPressureScreenInterceptVfxCueRequests,
                    forwardRiskPhysicalSummonPunish.SummonBlocks,
                    "The physical summon-punish route should preserve pressure-screen intercept VFX before the Skill1 confirm.");
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
                Assert.Greater(
                    blockedRecovery.EnergyReadyScreenCueRequests,
                    0,
                    "A counter answer pulse should be accompanied by an energy-ready screen cue.");
                Assert.Greater(
                    blockedRecovery.EnergyReadyVfxCueRequests,
                    0,
                    "A counter answer pulse should be accompanied by an energy-ready VFX cue.");
                Assert.Greater(
                    blockedRecovery.EnergySpendScreenCueRequests,
                    0,
                    "A recovered route should present the summon-answer energy spend on screen.");
                Assert.Greater(
                    blockedRecovery.EnergySpendVfxCueRequests,
                    0,
                    "A recovered route should present the summon-answer energy spend through VFX.");
                Assert.Greater(
                    blockedRecovery.CounterWaveAnswerScreenCueRequests,
                    0,
                    "A stabilized counter recovery should request the existing counter-answer screen cue.");
                Assert.Greater(
                    blockedRecovery.CounterWaveStabilizedCameraCueRequests,
                    0,
                    "A stabilized counter recovery should request the existing counter-answer camera cue.");
                Assert.Greater(
                    blockedRecovery.CounterWaveStabilizedVfxCueRequests,
                    0,
                    "A stabilized counter recovery should request the existing counter-answer VFX cue.");
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
                Assert.Greater(
                    noSummon.PlayerDamageScreenCueRequests,
                    0,
                    "Unanswered player damage should request the existing Player.Damaged screen cue.");
                Assert.Greater(
                    noSummon.PlayerDamageFeedbackRequests,
                    0,
                    "Unanswered player damage should request the existing damage-vignette feedback.");
                Assert.IsFalse(
                    noSummon.LastPlayerDamageFeedbackInterruptedAction,
                    "Routine player pressure feedback should not read as an action-interrupting lock.");
                Assert.AreEqual(
                    0,
                    noSummon.PlayerLockingDamageEvents,
                    "Routine pressure should not turn every player hit into a locking reaction.");
                Assert.Greater(
                    ignoredRecovery.PlayerDamageScreenCueRequests,
                    blockedRecovery.PlayerDamageScreenCueRequests,
                    "Ignoring counter pressure should show more player damage screen cues than the recovered route.");
                Assert.AreEqual(
                    0,
                    forwardRiskPhysicalSummonPunish.PlayerDamageScreenCueRequests,
                    "The clean physical summon-punish route should not leak player damage screen cues.");
                Assert.AreEqual(
                    0,
                    forwardRiskPhysicalSummonPunish.PlayerDamageFeedbackRequests,
                    "The clean physical summon-punish route should not leak damage-vignette feedback.");
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
                Assert.Greater(
                    intended.FollowupHitCinematicCueRequests,
                    0,
                    "A clean Skill1 follow-up punish should now request the reviewed micro-cinematic follow-up hit director cue.");
                Assert.Greater(
                    intended.FollowupHitCinematicFrameOverlayCount,
                    0,
                    "A clean Skill1 follow-up punish should activate the short frame overlay while the micro-cinematic cue is playing.");
                Assert.AreEqual(
                    0,
                    intended.FollowupHitSequenceBridgeRequests,
                    "The current Frontline pass should keep full cinematic sequence playback disabled; only the micro-cue director path is in scope.");
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
                    blockedFollowup.FollowupMissedCinematicCueRequests,
                    0,
                    "A blocked boss-screen follow-up should now request the reviewed micro-cinematic miss/recall cue.");
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
                Assert.Greater(
                    blockedRecovery.FollowupHitCinematicCueRequests,
                    0,
                    "Counter recovery should request the reviewed micro-cinematic follow-up hit director cue after the fresh summon answer.");
                Assert.Greater(
                    blockedRecovery.FollowupHitCinematicFrameOverlayCount,
                    0,
                    "Counter recovery follow-up hit should activate the short frame overlay without requiring a full sequence bridge.");
                Assert.AreEqual(
                    0,
                    blockedRecovery.FollowupHitSequenceBridgeRequests,
                    "Counter recovery should keep full sequence bridge playback disabled while the micro-cue director path is active.");
            }
            finally
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private static IEnumerator RunPolicySample(PolicyKind policy, List<PolicyMetrics> destination)
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            CombatPolicyContext context = BuildContext(policy);
            yield return RunPolicy(context);
            context.Complete();
            destination.Add(context.Metrics);
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
            SummonEnergyVfxCuePresenter energyVfxCuePresenter =
                RequireComponent<SummonEnergyVfxCuePresenter>(
                    player.gameObject,
                    "summon energy VFX cue presenter");
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
                energyVfxCuePresenter,
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
                case PolicyKind.CloseProbeSelectorBiasProbe:
                    yield return RunCloseProbeSelectorBiasProbe(context);
                    break;
                case PolicyKind.CloseProbePhysicalFireProbe:
                    yield return RunCloseProbePhysicalFireProbe(context);
                    break;
                case PolicyKind.CloseProbePhysicalThenScreenCurtainProbe:
                    yield return RunCloseProbePhysicalThenScreenCurtainProbe(context);
                    break;
                case PolicyKind.CloseProbePhysicalThenSummonPunishProbe:
                    yield return RunCloseProbePhysicalThenSummonPunishProbe(context);
                    break;
                case PolicyKind.BossTunnelVisionIgnoresCloseProbe:
                    yield return RunBossTunnelVisionIgnoresCloseProbe(context);
                    break;
                case PolicyKind.NoSummonSurvivalLimit:
                    yield return RunNoSummonSurvivalLimit(context);
                    break;
                case PolicyKind.GunOnlySurvivalLimit:
                    yield return RunGunOnlySurvivalLimit(context);
                    break;
                case PolicyKind.PrematureSkill1NoSummon:
                    yield return RunPrematureSkill1NoSummon(context);
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
                case PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe:
                    yield return RunPhysicalSummonNoPunishProbe(context, ForwardEnergyProbeForwardRisk01);
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

        private static IEnumerator RunCloseProbeSelectorBiasProbe(CombatPolicyContext context)
        {
            RecordCloseProbeSelectorSnapshot(context);
            context.Sample();
            yield return null;
        }

        private static IEnumerator RunCloseProbePhysicalFireProbe(CombatPolicyContext context)
        {
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);
        }

        private static IEnumerator RunCloseProbePhysicalThenScreenCurtainProbe(CombatPolicyContext context)
        {
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);

            if (context.CloseThreatHealth.IsAlive)
            {
                yield break;
            }

            float start = context.Metrics.ElapsedSeconds;
            while (context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - start < CloseProbeScreenCurtainObservationSeconds)
            {
                yield return Advance(context, 0.1f);
                context.PocketOwner.Tick(0f);
                context.Sample();
                if (context.Metrics.BossPressureSummonReleases > 0
                    && context.Metrics.MaxBossPressureActiveScreenCount > 0)
                {
                    break;
                }
            }

            if (context.Metrics.BossPressureSummonReleases <= 0)
            {
                context.Metrics.Notes.Add("post-close screen curtain did not release");
            }
        }

        private static IEnumerator RunCloseProbePhysicalThenSummonPunishProbe(CombatPolicyContext context)
        {
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);

            if (context.CloseThreatHealth.IsAlive)
            {
                yield break;
            }

            context.BossEmitter.SetFiringEnabled(true);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndWaitForScreenCurtainBlock(context);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator FireCloseProbePhysicalShots(CombatPolicyContext context)
        {
            int shots = 0;
            while (context.CloseThreatHealth.IsAlive && shots < CloseProbePhysicalFireMaxShots)
            {
                context.TargetSelector.RefreshTarget();
                while (!context.RangedBasicAttack.IsFireReady)
                {
                    yield return Advance(context, 0.05f);
                }

                if (!context.RangedBasicAttack.TryFire())
                {
                    context.Metrics.Notes.Add($"physical close fire blocked: {context.RangedBasicAttack.LastUseBlockedReason}");
                    break;
                }

                context.Metrics.BasicShots++;
                shots++;
                RecordPhysicalCloseFireAimRead(context);
                LaneActionProjectile projectile = FindActivePlayerProjectile();
                if (projectile == null)
                {
                    context.Metrics.Notes.Add("physical close fire produced no tracked projectile");
                    break;
                }

                float start = context.Metrics.ElapsedSeconds;
                while (projectile.IsActive
                    && context.Metrics.ElapsedSeconds - start < CloseProbePhysicalFireFlightSeconds)
                {
                    yield return Advance(context, 0.05f);
                }

                RecordPhysicalCloseFireResult(context, projectile);
                if (projectile.IsActive)
                {
                    projectile.Deactivate();
                }

                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (context.CloseThreatHealth.IsAlive)
            {
                context.Metrics.Notes.Add("physical close fire did not defeat close probe");
            }
        }

        private static IEnumerator RunBossTunnelVisionIgnoresCloseProbe(CombatPolicyContext context)
        {
            for (int i = 0; i < 4 && context.PlayerHealth.IsAlive; i++)
            {
                yield return FireBasicAt(context, context.BossHealth, context.BossCollider);
                yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
                yield return Advance(context, 1.0f);
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

        private static IEnumerator RunPrematureSkill1NoSummon(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);

            context.TargetSelector.NotifyTargetContact(context.BossHealth);
            context.TargetSelector.RefreshTarget();
            if (!context.Skill1Action.TryUseSkill1())
            {
                context.Metrics.Notes.Add($"premature skill1 blocked: {context.Skill1Action.LastUseBlockedReason}");
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
            yield return ApplyBossWave(context, BossWaveAnswer.PlayerTakesHit);
            yield return Advance(context, 1.0f);
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
            yield return WaitForCloseThreatReliefEnd(context, 3.5f);
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            GrantEnergyToTier(context, 1);
            context.Sample();

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

        private static IEnumerator RunPhysicalSummonNoPunishProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            yield return RunPhysicalSummonBlockProbe(context, forwardRisk01);
            yield return Advance(context, PhysicalNoPunishObservationSeconds);
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator RunPhysicalSummonPunishProbe(
            CombatPolicyContext context,
            float forwardRisk01)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return WaitForCloseThreatReliefEnd(context, 3.5f);
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            GrantEnergyToTier(context, 1);
            context.Sample();

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

        private static void GrantEnergyToTier(CombatPolicyContext context, int tier)
        {
            int guard = 0;
            while (context.EnergyLadder.AvailableTier < tier && guard++ < 4)
            {
                context.EnergyLadder.GrantCurrentTierEnergy(
                    Mathf.Max(1f, context.EnergyLadder.CurrentTierTarget + 1f));
            }
        }

        private static IEnumerator WaitForCloseThreatReliefEnd(CombatPolicyContext context, float maxSeconds)
        {
            float start = context.Metrics.ElapsedSeconds;
            while (context.PocketOwner.IsPressureReliefActive
                && context.Metrics.ElapsedSeconds - start < maxSeconds)
            {
                context.BossEmitter.SetFiringEnabled(false);
                DeactivateActiveBossProjectiles();
                yield return Advance(context, 0.05f);
            }

            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
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

        private static void RecordCloseProbeSelectorSnapshot(CombatPolicyContext context)
        {
            context.TargetSelector.RefreshTarget();
            context.Metrics.SelectorCandidateCount = context.TargetSelector.TargetCandidateCount;
            context.Metrics.SelectorDefaultTarget =
                ResolveSelectorTargetKind(context, context.TargetSelector.CurrentTargetHealth);
            context.Metrics.SelectorCloseDistance =
                ResolvePlanarDistance(context.Player.transform.position, context.CloseThreatHealth.transform.position);
            context.Metrics.SelectorBossDistance =
                ResolvePlanarDistance(context.Player.transform.position, context.BossHealth.transform.position);

            Vector3 fallbackDirection = Vector3.ProjectOnPlane(
                context.BossHealth.transform.position - context.Player.transform.position,
                Vector3.up);
            if (fallbackDirection.sqrMagnitude <= 0.0001f)
            {
                fallbackDirection = context.Player.transform.forward;
            }

            if (context.TargetSelector.TryGetAttackAimDirection(
                fallbackDirection,
                0f,
                out Vector3 attackAimDirection,
                out CombatHealth attackAimTarget))
            {
                context.Metrics.SelectorAttackAimTarget =
                    ResolveSelectorTargetKind(context, attackAimTarget);
                context.Metrics.SelectorAttackAimAngleToClose =
                    ResolvePlanarAngleToTarget(
                        context.Player.transform.position,
                        context.CloseThreatHealth.transform.position,
                        attackAimDirection);
                context.Metrics.SelectorAttackAimAngleToBoss =
                    ResolvePlanarAngleToTarget(
                        context.Player.transform.position,
                        context.BossHealth.transform.position,
                        attackAimDirection);
            }
        }

        private static void RecordPhysicalCloseFireResult(
            CombatPolicyContext context,
            LaneActionProjectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            context.Metrics.CloseThreatPhysicalLastImpactTarget =
                ResolveSelectorTargetKind(context, projectile.LastImpactTargetHealth);
            context.Metrics.CloseThreatPhysicalLastImpactResult =
                projectile.LastImpactResult.ToString();

            if (projectile.LastImpactTargetHealth == context.CloseThreatHealth)
            {
                context.Metrics.CloseThreatPhysicalProjectileImpactAttempts++;
                if (projectile.LastImpactResult == ProjectileImpactResult.AppliedDamage)
                {
                    context.Metrics.CloseThreatPhysicalProjectileHits++;
                    context.Metrics.CloseThreatBasicHits++;
                }
            }
            else if (projectile.LastImpactTargetHealth == context.BossHealth)
            {
                context.Metrics.CloseThreatPhysicalProjectileBossHits++;
                if (projectile.LastImpactResult == ProjectileImpactResult.AppliedDamage)
                {
                    context.Metrics.BossBasicHits++;
                }
            }

            context.Metrics.CloseThreatPhysicalFireReadout =
                $"shots {context.Metrics.BasicShots} close {context.Metrics.CloseThreatPhysicalProjectileHits}/"
                + $"{context.Metrics.CloseThreatPhysicalProjectileImpactAttempts} boss "
                + $"{context.Metrics.CloseThreatPhysicalProjectileBossHits} last "
                + $"{context.Metrics.CloseThreatPhysicalLastImpactTarget}/{projectile.LastImpactResult} aim "
                + $"{context.Metrics.CloseThreatPhysicalLastAimTarget} raw/res "
                + $"{context.Metrics.CloseThreatPhysicalLastRawAngleToClose:0.0}/"
                + $"{context.Metrics.CloseThreatPhysicalLastResolvedAngleToClose:0.0}";
        }

        private static void RecordPhysicalCloseFireAimRead(CombatPolicyContext context)
        {
            context.Metrics.CloseThreatPhysicalLastAimTarget =
                ResolveSelectorTargetKind(context, context.RangedBasicAttack.AimAssistTargetHealth);
            context.Metrics.CloseThreatPhysicalLastAimAssistStrength =
                context.RangedBasicAttack.AimAssistStrength01;
            context.Metrics.CloseThreatPhysicalLastRawAngleToClose =
                ResolvePlanarAngleToTarget(
                    context.Player.transform.position,
                    context.CloseThreatHealth.transform.position,
                    context.RangedBasicAttack.LastRawAimDirection);
            context.Metrics.CloseThreatPhysicalLastResolvedAngleToClose =
                ResolvePlanarAngleToTarget(
                    context.Player.transform.position,
                    context.CloseThreatHealth.transform.position,
                    context.RangedBasicAttack.LastResolvedFireDirection);
            context.Metrics.CloseThreatPhysicalLastRawAngleToBoss =
                ResolvePlanarAngleToTarget(
                    context.Player.transform.position,
                    context.BossHealth.transform.position,
                    context.RangedBasicAttack.LastRawAimDirection);
            context.Metrics.CloseThreatPhysicalLastResolvedAngleToBoss =
                ResolvePlanarAngleToTarget(
                    context.Player.transform.position,
                    context.BossHealth.transform.position,
                    context.RangedBasicAttack.LastResolvedFireDirection);

            if (context.Metrics.FirstBossPressureReleaseAtSeconds < 0f)
            {
                context.Metrics.CloseThreatPhysicalShotsBeforeBossPressure = context.Metrics.BasicShots;
            }
        }

        private static string ResolveSelectorTargetKind(CombatPolicyContext context, CombatHealth targetHealth)
        {
            if (targetHealth == null)
            {
                return "None";
            }

            if (targetHealth == context.CloseThreatHealth)
            {
                return "CloseProbe";
            }

            if (targetHealth == context.BossHealth)
            {
                return "BossProxy";
            }

            return targetHealth.name;
        }

        private static float ResolvePlanarDistance(Vector3 from, Vector3 to)
        {
            return Vector3.ProjectOnPlane(to - from, Vector3.up).magnitude;
        }

        private static float ResolvePlanarAngleToTarget(Vector3 origin, Vector3 target, Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            Vector3 targetDirection = Vector3.ProjectOnPlane(target - origin, Vector3.up);
            if (planarDirection.sqrMagnitude <= 0.0001f || targetDirection.sqrMagnitude <= 0.0001f)
            {
                return -1f;
            }

            return Vector3.Angle(planarDirection.normalized, targetDirection.normalized);
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

            BossBarrageProjectile[] projectiles = FindActiveBossProjectiles(pattern != null ? pattern.ProjectileMaterial : null);
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

            BossBarrageProjectile[] bossProjectiles = FindActiveBossProjectiles(pattern != null ? pattern.ProjectileMaterial : null);
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

        private static IEnumerator UseSummonAndWaitForScreenCurtainBlock(CombatPolicyContext context)
        {
            if (context.EnergyLadder.AvailableTier <= 0)
            {
                yield return ChargeEnergyToTier(context, 1, 8f);
            }

            int startBlocks = context.Metrics.SummonBlocks;
            int startWindows = context.Metrics.FollowupWindowOpenCount;
            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"live close-chain summon blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            float start = context.Metrics.ElapsedSeconds;
            while (context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - start < CloseProbeScreenCurtainObservationSeconds)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
                if (context.Metrics.SummonBlocks > startBlocks
                    && context.Metrics.FollowupWindowOpenCount > startWindows)
                {
                    break;
                }
            }

            if (context.Metrics.SummonBlocks <= startBlocks)
            {
                context.Metrics.Notes.Add("live close-chain summon did not block screen curtain");
            }

            if (context.Metrics.FollowupWindowOpenCount <= startWindows)
            {
                context.Metrics.Notes.Add("live close-chain summon did not open follow-up window");
            }
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

        private static void WriteReports(
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, BuildMarkdown(results, repeatabilityResults), Encoding.UTF8);
            File.WriteAllText(JsonPath, BuildJson(results, repeatabilityResults), Encoding.UTF8);
        }

        private static string BuildMarkdown(
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
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
            AppendPolicyRepeatabilityGate(builder, repeatabilityResults);
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
            AppendStageResultHookContract(builder, results);
            builder.AppendLine();
            AppendStageWaveBeatMap(builder, results);
            builder.AppendLine();
            AppendLocalDefenseSelectorProbe(builder, results);
            builder.AppendLine();
            AppendLocalDefensePhysicalFireProbe(builder, results);
            builder.AppendLine();
            AppendTargetPriorityContract(builder, results);
            builder.AppendLine();
            AppendSkillGateContract(builder, results);
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
            AppendUnconfirmedFollowupCost(builder, results);
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
            builder.AppendLine("## Energy Presentation Bridge");
            builder.AppendLine("| Policy | Energy screen total F/R/S | Energy VFX F/R/S | Last screen tier | Last VFX ready/spend tier | Counter answer pulse | Result |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(
                    $"{result.EnergyScreenCueRequests} "
                    + $"{result.ForwardRiskEnergyScreenCueRequests}/"
                    + $"{result.EnergyReadyScreenCueRequests}/"
                    + $"{result.EnergySpendScreenCueRequests}");
                builder.Append(" | ");
                builder.Append(
                    $"{result.ForwardRiskEnergyVfxCueRequests}/"
                    + $"{result.EnergyReadyVfxCueRequests}/"
                    + $"{result.EnergySpendVfxCueRequests}");
                builder.Append(" | ");
                builder.Append(result.LastEnergyScreenCueTier);
                builder.Append(" | ");
                builder.Append($"{result.LastEnergyReadyVfxTier}/{result.LastEnergySpendVfxTier}");
                builder.Append(" | ");
                builder.Append(result.CounterWaveAnswerEnergyPulse.ToString("0"));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.ResultKind));
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
            builder.AppendLine("## Boss Damage Attribution");
            builder.AppendLine("| Policy | Boss dmg | Player | Ally summon | Enemy | Neutral/other | Player share |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.BossDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageFromPlayer.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageFromAllySummon.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageFromEnemy.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageFromNeutralOrUnknown.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(FormatPercent01(result.BossDamagePlayerShare01));
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
            builder.AppendLine("## Player Damage Presentation Bridge");
            builder.AppendLine("| Policy | HP lost | Player non-lock/lock/full | Screen/feedback cues | Last policy/control | Feedback intensity/duration/scale | Interrupts action | Result |");
            builder.AppendLine("|---|---:|---:|---:|---|---:|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append($"{result.PlayerDamageTaken:0.0}");
                builder.Append(" | ");
                builder.Append($"{result.PlayerNonLockingDamageEvents}/{result.PlayerLockingDamageEvents}/{result.PlayerFullBodyEligibleDamageEvents}");
                builder.Append(" | ");
                builder.Append($"{result.PlayerDamageScreenCueRequests}/{result.PlayerDamageFeedbackRequests}");
                builder.Append(" | ");
                builder.Append(EscapeTable(
                    $"{result.LastPlayerDamageResponsePolicy}/{result.LastPlayerDamageControlLockPolicy}"));
                builder.Append(" | ");
                builder.Append(
                    $"{result.LastPlayerDamageFeedbackIntensity:0.00}/"
                    + $"{result.LastPlayerDamageFeedbackDuration:0.00}/"
                    + $"{result.LastPlayerDamageFeedbackPolicyScale:0.00}");
                builder.Append(" | ");
                builder.Append(result.LastPlayerDamageFeedbackInterruptedAction ? "yes" : "no");
                builder.Append(" | ");
                builder.Append(EscapeTable(result.ResultKind));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Summon Block Presentation Bridge");
            builder.AppendLine("| Policy | Blocks | Camera block/opportunity | Screen opportunity | Pressure-screen flash/VFX | Activation VFX | Showing max | Last block tier | Block->window | Result |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.SummonBlocks);
                builder.Append(" | ");
                builder.Append($"{result.SummonPressureBlockCameraCueRequests}/{result.SummonBlockOpportunityCameraCueRequests}");
                builder.Append(" | ");
                builder.Append(result.FollowupBlockOpportunityScreenCueRequests);
                builder.Append(" | ");
                builder.Append($"{result.SummonPressureScreenInterceptFlashes}/{result.SummonPressureScreenInterceptVfxCueRequests}");
                builder.Append(" | ");
                builder.Append(result.SummonPressureScreenActivationVfxCueRequests);
                builder.Append(" | ");
                builder.Append(result.MaxShowingSummonPressureScreenPresenters);
                builder.Append(" | ");
                builder.Append(result.LastSummonPressureBlockCameraTier);
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.BlockToFollowupWindowSeconds));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.ResultKind));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Counter Wave Presentation Bridge");
            builder.AppendLine("| Policy | Counter waves | Screen wave/answer | Camera wave/stable | VFX wave/stable | Last screen source/answer | Last camera tiers | Last VFX source | Result |");
            builder.AppendLine("|---|---:|---:|---:|---:|---|---:|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.CounterWaves);
                builder.Append(" | ");
                builder.Append($"{result.CounterWaveScreenCueRequests}/{result.CounterWaveAnswerScreenCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.CounterWaveCameraCueRequests}/{result.CounterWaveStabilizedCameraCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.CounterWaveVfxCueRequests}/{result.CounterWaveStabilizedVfxCueRequests}");
                builder.Append(" | ");
                builder.Append(EscapeTable(
                    $"{result.LastCounterWaveScreenSource}/{result.LastCounterWaveScreenAnswer}"));
                builder.Append(" | ");
                builder.Append($"{result.LastCounterWaveCameraTier}/{result.LastCounterWaveStabilizedCameraTier}");
                builder.Append(" | ");
                builder.Append(EscapeTable(result.LastCounterWaveVfxSource));
                builder.Append(" | ");
                builder.Append(EscapeTable(result.ResultKind));
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
            PolicyMetrics selectorProbe = RequireResult(results, PolicyKind.CloseProbeSelectorBiasProbe);
            PolicyMetrics physicalCloseFire = RequireResult(results, PolicyKind.CloseProbePhysicalFireProbe);
            PolicyMetrics physicalCloseCurtain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenScreenCurtainProbe);
            PolicyMetrics physicalCloseChain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe);
            PolicyMetrics bossTunnel = RequireResult(results, PolicyKind.BossTunnelVisionIgnoresCloseProbe);
            PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics prematureSkill1 = RequireResult(results, PolicyKind.PrematureSkill1NoSummon);
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
            PolicyMetrics forwardRiskPhysicalSummonNoPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
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
            builder.AppendLine($"- Local-defense selector probe: candidates {selectorProbe.SelectorCandidateCount}, default `{selectorProbe.SelectorDefaultTarget}`, attack aim `{selectorProbe.SelectorAttackAimTarget}`, close/boss distance {FormatOptionalDistance(selectorProbe.SelectorCloseDistance)}/{FormatOptionalDistance(selectorProbe.SelectorBossDistance)}.");
            builder.AppendLine($"- Local-defense physical fire: shots {physicalCloseFire.BasicShots}, close projectile hits {physicalCloseFire.CloseThreatPhysicalProjectileHits}/{physicalCloseFire.CloseThreatPhysicalProjectileImpactAttempts}, close HP {physicalCloseFire.CloseThreatHealthRemaining:0.0}, boss damage {physicalCloseFire.BossDamageFromPlayer:0.0}.");
            builder.AppendLine($"- Close-to-curtain transition: physical close HP {physicalCloseCurtain.CloseThreatHealthRemaining:0.0}, boss releases {physicalCloseCurtain.BossPressureSummonReleases}, max screens {physicalCloseCurtain.MaxBossPressureActiveScreenCount}, first unresolved `{ResolveFirstUnresolvedBeat(physicalCloseCurtain)}`.");
            builder.AppendLine($"- Live close-chain route: close hits {physicalCloseChain.CloseThreatPhysicalProjectileHits}/{physicalCloseChain.CloseThreatPhysicalProjectileImpactAttempts}, summon blocks {physicalCloseChain.SummonBlocks}, Skill1 hits {physicalCloseChain.SkillProjectileHits}, follow-up/result {physicalCloseChain.FollowupHitCount}/{physicalCloseChain.ResultKind}.");
            builder.AppendLine($"- Target priority split: boss tunnel vision landed boss basic hits {bossTunnel.BossBasicHits} while close-probe hits stayed {bossTunnel.CloseThreatBasicHits}; first unresolved beat `{ResolveFirstUnresolvedBeat(bossTunnel)}`.");
            builder.AppendLine($"- Long survival limit: no-summon player down {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)} / boss down {FormatSeconds(noSummonSurvival.FirstBossDownAtSeconds)}; gun-only player down {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)} / boss down {FormatSeconds(gunOnlySurvival.FirstBossDownAtSeconds)}.");
            builder.AppendLine($"- Skill1 punish split: gun-only boss damage {gunOnly.BossDamageTaken:0.0}, intended follow-up boss damage {intended.BossDamageTaken:0.0}.");
            builder.AppendLine($"- Skill gate split: premature Skill1 use/hit {prematureSkill1.SkillUses}/{prematureSkill1.SkillProjectileHits}, boss damage {prematureSkill1.BossDamageFromPlayer:0.0}, follow-up/result {prematureSkill1.FollowupHitCount}/{prematureSkill1.ResultRecords}, unresolved beat `{ResolveFirstUnresolvedBeat(prematureSkill1)}`.");
            builder.AppendLine($"- Late summon ended as `{late.ResultKind}` with {late.PlayerDamageTaken:0.0} damage taken, so the report can compare timing quality without changing the scene.");
            builder.AppendLine($"- Intended route currently reads as `{ResolveRouteShape(intended)}`: follow-up window {FormatSeconds(intended.FirstFollowupWindowAtSeconds)}, counter {FormatSeconds(intended.FirstCounterWaveAtSeconds)}, Skill1 hit {FormatSeconds(intended.FirstFollowupHitAtSeconds)}.");
            builder.AppendLine($"- Lock/unlock cadence: intended block->window {FormatSeconds(intended.BlockToFollowupWindowSeconds)}, window->hit {FormatSeconds(intended.FollowupWindowToHitSeconds)}; boss-screen recovery answer pulse {blockedRecovery.CounterWaveAnswerEnergyPulse:0}, counter->answer {FormatSeconds(blockedRecovery.CounterTriggerToAnswerSeconds)}, answer->stable {FormatSeconds(blockedRecovery.CounterAnswerToStableSeconds)}, stable->final {FormatSeconds(blockedRecovery.CounterStableToFinalWindowSeconds)}, final->hit {FormatSeconds(blockedRecovery.FinalWindowToHitSeconds)}.");
            builder.AppendLine($"- Punish window tolerance: delayed clean hit after {FormatSeconds(delayedIntended.FollowupHitWindowDelaySeconds)} with {FormatSeconds(delayedIntended.FollowupWindowRemainingAtFirstHitSeconds)} remaining; delayed boss-screen recovery hit after {FormatSeconds(delayedBlockedRecovery.FollowupHitWindowDelaySeconds)} with {FormatSeconds(delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds)} remaining.");
            builder.AppendLine($"- Forward-risk EN split: backline LV1 {FormatSeconds(backlineEnergy.EnergyTier1DurationSeconds)} at x{backlineEnergy.AverageEnergyGainMultiplier:0.00}, forward-risk LV1 {FormatSeconds(forwardRiskEnergy.EnergyTier1DurationSeconds)} at x{forwardRiskEnergy.AverageEnergyGainMultiplier:0.00}; forward route is {ResolveEnergySpeedup(backlineEnergy, forwardRiskEnergy):0.0}x faster.");
            builder.AppendLine($"- Energy presentation bridge: forward-risk energy screen/VFX F/R/S {forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests}/{forwardRiskEnergy.EnergyReadyScreenCueRequests}/{forwardRiskEnergy.EnergySpendScreenCueRequests} and {forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests}/{forwardRiskEnergy.EnergyReadyVfxCueRequests}/{forwardRiskEnergy.EnergySpendVfxCueRequests}; boss-screen recovery ready/spend screen {blockedRecovery.EnergyReadyScreenCueRequests}/{blockedRecovery.EnergySpendScreenCueRequests}, VFX {blockedRecovery.EnergyReadyVfxCueRequests}/{blockedRecovery.EnergySpendVfxCueRequests}.");
            builder.AppendLine($"- Forward-risk barrage shape: backline `{backlineBarrage.BarrageShapePatternId}` near-body {backlineBarrage.BarrageShapeNearProjectileCount}/{backlineBarrage.BarrageShapeProjectileCount}, avg lateral gap {backlineBarrage.BarrageShapeAverageLateralGap:0.00}, nearest {backlineBarrage.BarrageShapeNearestLaneDistance:0.00}, density {backlineBarrage.BarrageShapeThreatDensity:0.00}; forward near-body {forwardRiskBarrage.BarrageShapeNearProjectileCount}/{forwardRiskBarrage.BarrageShapeProjectileCount}, avg lateral gap {forwardRiskBarrage.BarrageShapeAverageLateralGap:0.00}, nearest {forwardRiskBarrage.BarrageShapeNearestLaneDistance:0.00}, density {forwardRiskBarrage.BarrageShapeThreatDensity:0.00}.");
            if (forwardRiskBarrage.BarrageShapeNearProjectileCount <= backlineBarrage.BarrageShapeNearProjectileCount)
            {
                builder.AppendLine("- Forward-risk barrage compression is measurable, but near-body projectile count did not rise; direct position-specific hit danger remains a follow-up gap.");
            }

            builder.AppendLine($"- Route stability split: no-action {FormatPercent01(noSummon.RouteStability01)} final / {FormatPercent01(noSummon.MinRouteStability01)} min, gun-only {FormatPercent01(gunOnly.RouteStability01)} / {FormatPercent01(gunOnly.MinRouteStability01)}, intended {FormatPercent01(intended.RouteStability01)} / {FormatPercent01(intended.MinRouteStability01)}.");
            builder.AppendLine($"- Forward-risk physical barrage: backline hits {backlinePhysicalBarrage.PhysicalBarragePlayerHits}/{backlinePhysicalBarrage.PhysicalBarrageTrackedProjectileCount}, damage {backlinePhysicalBarrage.PhysicalBarragePlayerDamage:0.0}; forward hits {forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}/{forwardRiskPhysicalBarrage.PhysicalBarrageTrackedProjectileCount}, damage {forwardRiskPhysicalBarrage.PhysicalBarragePlayerDamage:0.0}.");
            builder.AppendLine($"- Forward-risk physical summon block: blocks {forwardRiskPhysicalSummonBlock.SummonBlocks}, player hits {forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerHits}/{forwardRiskPhysicalSummonBlock.PhysicalBarrageTrackedProjectileCount}, damage {forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerDamage:0.0}, block->window {FormatSeconds(forwardRiskPhysicalSummonBlock.BlockToFollowupWindowSeconds)}, block camera/flash/VFX {forwardRiskPhysicalSummonBlock.SummonPressureBlockCameraCueRequests}/{forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptFlashes}/{forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptVfxCueRequests}.");
            builder.AppendLine($"- Forward-risk physical no-punish: follow-up misses {forwardRiskPhysicalSummonNoPunish.FollowupMissCount}, counter waves {forwardRiskPhysicalSummonNoPunish.CounterWaves}, result `{forwardRiskPhysicalSummonNoPunish.ResultKind}`, unanswered burden {FormatPercent01(forwardRiskPhysicalSummonNoPunish.UnansweredPressureBurdenShare01)}, boss damage player/summon {forwardRiskPhysicalSummonNoPunish.BossDamageFromPlayer:0.0}/{forwardRiskPhysicalSummonNoPunish.BossDamageFromAllySummon:0.0}.");
            builder.AppendLine($"- Counter-wave presentation bridge: physical no-punish wave cues screen/camera/VFX {forwardRiskPhysicalSummonNoPunish.CounterWaveScreenCueRequests}/{forwardRiskPhysicalSummonNoPunish.CounterWaveCameraCueRequests}/{forwardRiskPhysicalSummonNoPunish.CounterWaveVfxCueRequests}; boss-screen recovery answer cues screen/camera/VFX {blockedRecovery.CounterWaveAnswerScreenCueRequests}/{blockedRecovery.CounterWaveStabilizedCameraCueRequests}/{blockedRecovery.CounterWaveStabilizedVfxCueRequests}.");
            builder.AppendLine($"- Forward-risk physical summon punish: `{forwardRiskPhysicalSummonPunish.ResultKind}` with blocks {forwardRiskPhysicalSummonPunish.SummonBlocks}, player hits {forwardRiskPhysicalSummonPunish.PhysicalBarragePlayerHits}/{forwardRiskPhysicalSummonPunish.PhysicalBarrageTrackedProjectileCount}, Skill1 hits {forwardRiskPhysicalSummonPunish.SkillProjectileHits}, boss damage {forwardRiskPhysicalSummonPunish.BossDamageTaken:0.0}, window->hit {FormatSeconds(forwardRiskPhysicalSummonPunish.FollowupWindowToHitSeconds)}.");
            builder.AppendLine($"- Boss damage attribution: physical block-only player/summon boss damage {forwardRiskPhysicalSummonBlock.BossDamageFromPlayer:0.0}/{forwardRiskPhysicalSummonBlock.BossDamageFromAllySummon:0.0}; physical punish player/summon boss damage {forwardRiskPhysicalSummonPunish.BossDamageFromPlayer:0.0}/{forwardRiskPhysicalSummonPunish.BossDamageFromAllySummon:0.0}, player share {FormatPercent01(forwardRiskPhysicalSummonPunish.BossDamagePlayerShare01)}.");
            builder.AppendLine($"- Stage result hooks: no-summon fail `{ResolveResultHookClass(noSummonSurvival)}` / gun-only fail `{ResolveResultHookClass(gunOnlySurvival)}`; clean physical `{ResolveResultHookClass(forwardRiskPhysicalSummonPunish)}`; boss-screen recovery `{ResolveResultHookClass(blockedRecovery)}`. All committed hooks remain review-only analysis records, not payout/progression grants.");
            builder.AppendLine($"- Unanswered hit penalty split: no-action {FormatPercent01(noSummon.TotalUnansweredBossHitRoutePenalty01)} x{noSummon.UnansweredBossHitRoutePenaltyCount}, gun-only {FormatPercent01(gunOnly.TotalUnansweredBossHitRoutePenalty01)} x{gunOnly.UnansweredBossHitRoutePenaltyCount}, late {FormatPercent01(late.TotalUnansweredBossHitRoutePenalty01)} x{late.UnansweredBossHitRoutePenaltyCount}.");
            builder.AppendLine($"- Frontline exposure split: no-action enemy-only {FormatSeconds(noSummon.EnemyOnlyFrontlineSeconds)}, gun-only enemy-only {FormatSeconds(gunOnly.EnemyOnlyFrontlineSeconds)}, intended ally-only {FormatSeconds(intended.AllyOnlyFrontlineSeconds)} / contested {FormatSeconds(intended.ContestedFrontlineSeconds)}.");
            builder.AppendLine($"- ArkData effective pressure shape: no-action peak/top3 {FormatPercent01(noSummon.PeakPressureWindowShare01)}/{FormatPercent01(noSummon.Top3PressureWindowShare01)}, intended {FormatPercent01(intended.PeakPressureWindowShare01)}/{FormatPercent01(intended.Top3PressureWindowShare01)} with relief {FormatSeconds(intended.TimeToNextReliefWindowSeconds)}, ignored boss-screen unanswered burden {FormatPercent01(ignoredRecovery.UnansweredPressureBurdenShare01)} versus intended {FormatPercent01(intended.UnansweredPressureBurdenShare01)}.");
            builder.AppendLine($"- Enemy pressure actor cost: no-action clashes {noSummon.EnemyFrontlineClashes} / body hits {noSummon.EnemyFrontlineBodyHits} / clash damage {noSummon.EnemyFrontlineClashDamage:0.0}; intended route clashes {intended.EnemyFrontlineClashes} / body hits {intended.EnemyFrontlineBodyHits}.");
            builder.AppendLine($"- Hit reaction split: boss-screen recovery produced {blockedRecovery.TotalSummonDamageFlashes} summon damage flashes, {blockedRecovery.TotalSummonFullBodyHitReactions} full-body hit reactions, and {blockedRecovery.TotalNonLockingSummonDamageCues} non-locking damage cues.");
            builder.AppendLine($"- Damage response split: gun-only boss chip {gunOnly.BossNonLockingDamageEvents}/{gunOnly.BossLockingDamageEvents} non-lock/lock, intended Skill1 boss hits {intended.BossNonLockingDamageEvents}/{intended.BossLockingDamageEvents}, boss-screen recovery {blockedRecovery.BossNonLockingDamageEvents}/{blockedRecovery.BossLockingDamageEvents}.");
            builder.AppendLine($"- Follow-up presentation bridge: gun-only hit cues screen/camera/VFX {gunOnly.FollowupHitScreenCueRequests}/{gunOnly.FollowupHitCameraCueRequests}/{gunOnly.FollowupHitVfxCueRequests}, intended {intended.FollowupHitScreenCueRequests}/{intended.FollowupHitCameraCueRequests}/{intended.FollowupHitVfxCueRequests}, boss-screen recovery {blockedRecovery.FollowupHitScreenCueRequests}/{blockedRecovery.FollowupHitCameraCueRequests}/{blockedRecovery.FollowupHitVfxCueRequests}.");
            builder.AppendLine($"- Follow-up micro-cinematic bridge: gun-only hit director/sequence {gunOnly.FollowupHitCinematicCueRequests}/{gunOnly.FollowupHitSequenceBridgeRequests}, intended {intended.FollowupHitCinematicCueRequests}/{intended.FollowupHitSequenceBridgeRequests}, boss-screen recovery {blockedRecovery.FollowupHitCinematicCueRequests}/{blockedRecovery.FollowupHitSequenceBridgeRequests}; intended frame overlays {intended.FollowupHitCinematicFrameOverlayCount}.");
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

        private static void AppendPolicyRepeatabilityGate(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## Policy Repeatability Gate");
            builder.AppendLine($"Repeated sample count: `{RepeatabilityProbeRuns}` per selected policy. This gate checks structural direction, not exact identical numbers.");
            builder.AppendLine("| Policy | Runs | Result set | HP lost min/avg/max | Boss dmg min/avg/max | Player hits min/max | Blocks min/max | Skill1 min/max | Micro hit min/max | Seq hit min/max | Verdict |");
            builder.AppendLine("|---|---:|---|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < RepeatabilityPolicyOrder.Length; i++)
            {
                AppendPolicyRepeatabilityRow(builder, repeatabilityResults, RepeatabilityPolicyOrder[i]);
            }
        }

        private static void AppendPolicyRepeatabilityRow(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            builder.Append("| ");
            builder.Append(policy);
            builder.Append(" | ");
            builder.Append(CountPolicyResults(repeatabilityResults, policy));
            builder.Append(" | ");
            builder.Append(EscapeTable(BuildResultKindSet(repeatabilityResults, policy)));
            builder.Append(" | ");
            builder.Append(FormatMinAverageMax(
                MinMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                AverageMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken)));
            builder.Append(" | ");
            builder.Append(FormatMinAverageMax(
                MinMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                AverageMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                MaxMetric(repeatabilityResults, policy, result => result.BossDamageTaken)));
            builder.Append(" | ");
            builder.Append(FormatMinMax(
                MinMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits),
                MaxMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits)));
            builder.Append(" | ");
            builder.Append(FormatMinMax(
                MinMetric(repeatabilityResults, policy, result => result.SummonBlocks),
                MaxMetric(repeatabilityResults, policy, result => result.SummonBlocks)));
            builder.Append(" | ");
            builder.Append(FormatMinMax(
                MinMetric(repeatabilityResults, policy, result => result.SkillProjectileHits),
                MaxMetric(repeatabilityResults, policy, result => result.SkillProjectileHits)));
            builder.Append(" | ");
            builder.Append(FormatMinMax(
                MinMetric(repeatabilityResults, policy, result => result.FollowupHitCinematicCueRequests),
                MaxMetric(repeatabilityResults, policy, result => result.FollowupHitCinematicCueRequests)));
            builder.Append(" | ");
            builder.Append(FormatMinMax(
                MinMetric(repeatabilityResults, policy, result => result.FollowupHitSequenceBridgeRequests),
                MaxMetric(repeatabilityResults, policy, result => result.FollowupHitSequenceBridgeRequests)));
            builder.Append(" | ");
            builder.Append(ResolveRepeatabilityVerdict(repeatabilityResults, policy));
            builder.AppendLine(" |");
        }

        private static void AppendStructuralGateSummary(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics selectorProbe = RequireResult(results, PolicyKind.CloseProbeSelectorBiasProbe);
            PolicyMetrics physicalCloseFire = RequireResult(results, PolicyKind.CloseProbePhysicalFireProbe);
            PolicyMetrics physicalCloseCurtain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenScreenCurtainProbe);
            PolicyMetrics physicalCloseChain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe);
            PolicyMetrics bossTunnel = RequireResult(results, PolicyKind.BossTunnelVisionIgnoresCloseProbe);
            PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics prematureSkill1 = RequireResult(results, PolicyKind.PrematureSkill1NoSummon);
            PolicyMetrics forwardRiskPhysicalBarrage = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalBarrageProbe);
            PolicyMetrics forwardRiskPhysicalSummonBlock = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonBlockProbe);
            PolicyMetrics forwardRiskPhysicalSummonNoPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
            PolicyMetrics forwardRiskPhysicalSummonPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
            PolicyMetrics ignoredRecovery = RequireResult(results, PolicyKind.BossScreenIgnoredNoRecovery);
            PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);

            bool axis1Pass = noSummonSurvival.ResultKind == "PlayerDownFail"
                && gunOnlySurvival.ResultKind == "PlayerDownFail"
                && selectorProbe.SelectorCandidateCount > 1
                && selectorProbe.SelectorDefaultTarget == "CloseProbe"
                && selectorProbe.SelectorAttackAimTarget == "CloseProbe"
                && physicalCloseFire.CloseThreatPhysicalProjectileHits > 0
                && physicalCloseFire.CloseThreatHealthRemaining <= 0.01f
                && physicalCloseFire.BossDamageFromPlayer <= 0.01f
                && physicalCloseCurtain.CloseThreatHealthRemaining <= 0.01f
                && physicalCloseCurtain.BossPressureSummonReleases > 0
                && physicalCloseCurtain.MaxBossPressureActiveScreenCount > 0
                && ResolveFirstUnresolvedBeat(physicalCloseCurtain) == "ScreenCurtain"
                && bossTunnel.BossBasicHits > 0
                && bossTunnel.CloseThreatBasicHits == 0
                && ResolveFirstUnresolvedBeat(bossTunnel) == "CloseProbe"
                && bossTunnel.ResultRecords == 0
                && noSummonSurvival.FirstPlayerDownAtSeconds >= 0f
                && gunOnlySurvival.FirstPlayerDownAtSeconds >= 0f
                && gunOnlySurvival.FirstBossDownAtSeconds < 0f;
            bool axis2Pass = forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > 0
                && prematureSkill1.SkillUses > 0
                && prematureSkill1.SkillProjectileHits > 0
                && prematureSkill1.FollowupHitCount == 0
                && prematureSkill1.ResultRecords == 0
                && forwardRiskPhysicalSummonNoPunish.FollowupMissCount > 0
                && forwardRiskPhysicalSummonNoPunish.CounterWaves > 0
                && !forwardRiskPhysicalSummonNoPunish.IsClearResult
                && forwardRiskPhysicalSummonPunish.SummonBlocks > 0
                && forwardRiskPhysicalSummonBlock.SummonPressureBlockCameraCueRequests
                    >= forwardRiskPhysicalSummonBlock.SummonBlocks
                && forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptFlashes
                    >= forwardRiskPhysicalSummonBlock.SummonBlocks
                && forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptVfxCueRequests
                    >= forwardRiskPhysicalSummonBlock.SummonBlocks
                && forwardRiskPhysicalSummonNoPunish.CounterWaveScreenCueRequests > 0
                && forwardRiskPhysicalSummonNoPunish.CounterWaveCameraCueRequests > 0
                && forwardRiskPhysicalSummonNoPunish.CounterWaveVfxCueRequests > 0
                && blockedRecovery.CounterWaveAnswerScreenCueRequests > 0
                && blockedRecovery.CounterWaveStabilizedCameraCueRequests > 0
                && blockedRecovery.CounterWaveStabilizedVfxCueRequests > 0
                && blockedRecovery.EnergyReadyScreenCueRequests > 0
                && blockedRecovery.EnergyReadyVfxCueRequests > 0
                && blockedRecovery.EnergySpendScreenCueRequests > 0
                && blockedRecovery.EnergySpendVfxCueRequests > 0
                && forwardRiskPhysicalSummonPunish.PhysicalBarragePlayerHits == 0
                && forwardRiskPhysicalSummonPunish.ResultKind == "CleanFollowupClear"
                && forwardRiskPhysicalSummonPunish.SkillProjectileHits > 0
                && physicalCloseChain.CloseThreatPhysicalProjectileHits > 0
                && physicalCloseChain.SummonBlocks > 0
                && physicalCloseChain.FollowupHitCount > 0
                && physicalCloseChain.ResultKind == "CleanFollowupClear";
            bool axis3Pass = noSummon.PlayerNonLockingDamageEvents > 0
                && noSummon.PlayerLockingDamageEvents == 0
                && noSummon.PlayerDamageScreenCueRequests > 0
                && noSummon.PlayerDamageFeedbackRequests > 0
                && !noSummon.LastPlayerDamageFeedbackInterruptedAction
                && forwardRiskPhysicalSummonPunish.PlayerDamageScreenCueRequests == 0
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
                $"| 1. Bad routes lose state/HP | {FormatGateStatus(axis1Pass)} | no-summon down {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)}, gun-only down {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)}, selector default/aim `{selectorProbe.SelectorDefaultTarget}`/`{selectorProbe.SelectorAttackAimTarget}`, physical close hits {physicalCloseFire.CloseThreatPhysicalProjectileHits}/{physicalCloseFire.CloseThreatPhysicalProjectileImpactAttempts} HP {physicalCloseFire.CloseThreatHealthRemaining:0.0} boss dmg {physicalCloseFire.BossDamageFromPlayer:0.0}, close->curtain releases {physicalCloseCurtain.BossPressureSummonReleases} screens {physicalCloseCurtain.MaxBossPressureActiveScreenCount}, boss tunnel close/boss hits {bossTunnel.CloseThreatBasicHits}/{bossTunnel.BossBasicHits} unresolved `{ResolveFirstUnresolvedBeat(bossTunnel)}`, gun-only boss down {FormatSeconds(gunOnlySurvival.FirstBossDownAtSeconds)} |");
            builder.AppendLine(
                $"| 2. Block -> window -> Skill1 loop | {FormatGateStatus(axis2Pass)} | unblocked forward hits {forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}/{forwardRiskPhysicalBarrage.PhysicalBarrageTrackedProjectileCount}; premature Skill1 use/hit {prematureSkill1.SkillUses}/{prematureSkill1.SkillProjectileHits} but follow-up/result {prematureSkill1.FollowupHitCount}/{prematureSkill1.ResultRecords}; block presentation cam/flash/VFX {forwardRiskPhysicalSummonBlock.SummonPressureBlockCameraCueRequests}/{forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptFlashes}/{forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptVfxCueRequests}; no-punish misses {forwardRiskPhysicalSummonNoPunish.FollowupMissCount}, counters {forwardRiskPhysicalSummonNoPunish.CounterWaves}, counter cues {forwardRiskPhysicalSummonNoPunish.CounterWaveScreenCueRequests}/{forwardRiskPhysicalSummonNoPunish.CounterWaveCameraCueRequests}/{forwardRiskPhysicalSummonNoPunish.CounterWaveVfxCueRequests}, result `{forwardRiskPhysicalSummonNoPunish.ResultKind}`; recovery answer cues {blockedRecovery.CounterWaveAnswerScreenCueRequests}/{blockedRecovery.CounterWaveStabilizedCameraCueRequests}/{blockedRecovery.CounterWaveStabilizedVfxCueRequests}, energy ready/spend {blockedRecovery.EnergyReadyScreenCueRequests}/{blockedRecovery.EnergySpendScreenCueRequests} screen and {blockedRecovery.EnergyReadyVfxCueRequests}/{blockedRecovery.EnergySpendVfxCueRequests} VFX; physical punish blocks {forwardRiskPhysicalSummonPunish.SummonBlocks}, Skill1 hits {forwardRiskPhysicalSummonPunish.SkillProjectileHits}, `{forwardRiskPhysicalSummonPunish.ResultKind}`; live close-chain blocks/Skill1/result {physicalCloseChain.SummonBlocks}/{physicalCloseChain.SkillProjectileHits}/{physicalCloseChain.ResultKind} |");
            builder.AppendLine(
                $"| 3. Hit response and presentation | {FormatGateStatus(axis3Pass)} | player routine hits {noSummon.PlayerNonLockingDamageEvents}/{noSummon.PlayerLockingDamageEvents} non-lock/lock with damage cues {noSummon.PlayerDamageScreenCueRequests}/{noSummon.PlayerDamageFeedbackRequests}; clean route damage cues {forwardRiskPhysicalSummonPunish.PlayerDamageScreenCueRequests}/{forwardRiskPhysicalSummonPunish.PlayerDamageFeedbackRequests}; gun boss chip {gunOnly.BossNonLockingDamageEvents}/{gunOnly.BossLockingDamageEvents}; physical punish boss lock {forwardRiskPhysicalSummonPunish.BossLockingDamageEvents}, hit cues {forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests} |");
            builder.AppendLine(
                $"| 4. Enemy pressure actor cost | {FormatGateStatus(axis4Pass)} | no-action body hits {noSummon.EnemyFrontlineBodyHits}; ignored boss-screen body hits {ignoredRecovery.EnemyFrontlineBodyHits}, damage {ignoredRecovery.PlayerDamageTaken:0.0}; recovery body hits {blockedRecovery.EnemyFrontlineBodyHits}, damage {blockedRecovery.PlayerDamageTaken:0.0} |");
            builder.AppendLine(
                $"| Physical clean route reference | {FormatGateStatus(forwardRiskPhysicalSummonPunish.IsClearResult)} | physical summon-punish clears in {FormatSeconds(forwardRiskPhysicalSummonPunish.ElapsedSeconds)} with {forwardRiskPhysicalSummonPunish.PlayerDamageTaken:0.0} HP lost versus intended route {FormatSeconds(intended.ElapsedSeconds)} / {intended.PlayerDamageTaken:0.0} HP lost |");
        }

        private static void AppendStageResultHookContract(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Stage Result Hook Contract");
            builder.AppendLine("| Policy | Commit | Stage state | Hook class | Result copy | Reward hook | Next objective | Boundary |");
            builder.AppendLine("|---|---:|---|---|---|---|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(result.ResultRecords > 0
                    ? $"{result.ResultRecords} @ {FormatSeconds(result.ResultRecordElapsedSeconds)}"
                    : "pending");
                builder.Append(" | ");
                builder.Append(ResolveResultStageState(result));
                builder.Append(" | ");
                builder.Append(ResolveResultHookClass(result));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveResultCopyReadout(result)));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveCoverageValue(result.ResultRecordRewardHook)));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveCoverageValue(result.ResultRecordNextObjective)));
                builder.Append(" | ");
                builder.Append(IsReviewOnlyResultHook(result)
                    ? "review-only analysis"
                    : result.ResultRecords > 0 ? "review" : "no committed result");
                builder.AppendLine(" |");
            }
        }

        private static void AppendStageWaveBeatMap(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Stage/Wave Beat Map");
            builder.AppendLine("| Policy | CloseProbe | ScreenCurtain | Follow-up | CounterPressure | Result hook | First unresolved | Stage judgement |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(ResolveCloseProbeBeat(result));
                builder.Append(" | ");
                builder.Append(ResolveScreenCurtainBeat(result));
                builder.Append(" | ");
                builder.Append(ResolveFollowupBeat(result));
                builder.Append(" | ");
                builder.Append(ResolveCounterPressureBeat(result));
                builder.Append(" | ");
                builder.Append(ResolveResultHookBeat(result));
                builder.Append(" | ");
                builder.Append(ResolveFirstUnresolvedBeat(result));
                builder.Append(" | ");
                builder.Append(EscapeTable(ResolveStageWaveJudgement(result)));
                builder.AppendLine(" |");
            }
        }

        private static void AppendLocalDefenseSelectorProbe(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics selectorProbe = RequireResult(results, PolicyKind.CloseProbeSelectorBiasProbe);
            builder.AppendLine("## Local-Defense Selector Probe");
            builder.AppendLine("| Candidates | Default target | Attack aim target | Close dist | Boss dist | Aim angle close/boss | Read |");
            builder.AppendLine("|---:|---|---|---:|---:|---:|---|");
            builder.Append("| ");
            builder.Append(selectorProbe.SelectorCandidateCount);
            builder.Append(" | ");
            builder.Append(EscapeTable(selectorProbe.SelectorDefaultTarget));
            builder.Append(" | ");
            builder.Append(EscapeTable(selectorProbe.SelectorAttackAimTarget));
            builder.Append(" | ");
            builder.Append(FormatOptionalDistance(selectorProbe.SelectorCloseDistance));
            builder.Append(" | ");
            builder.Append(FormatOptionalDistance(selectorProbe.SelectorBossDistance));
            builder.Append(" | ");
            builder.Append(
                $"{FormatOptionalDistance(selectorProbe.SelectorAttackAimAngleToClose)}/{FormatOptionalDistance(selectorProbe.SelectorAttackAimAngleToBoss)}");
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSelectorProbeRead(selectorProbe)));
            builder.AppendLine(" |");
        }

        private static void AppendLocalDefensePhysicalFireProbe(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics physicalCloseFire = RequireResult(results, PolicyKind.CloseProbePhysicalFireProbe);
            builder.AppendLine("## Local-Defense Physical Fire Probe");
            builder.AppendLine("| Shots | Close projectile hits | Close dmg/HP | Close response N/L/F | Boss hits/dmg | Selector default/aim | Fire aim | Raw/res close deg | Pressure release | Read |");
            builder.AppendLine("|---:|---:|---:|---:|---:|---|---|---:|---|---|");
            builder.Append("| ");
            builder.Append(physicalCloseFire.BasicShots);
            builder.Append(" | ");
            builder.Append(
                $"{physicalCloseFire.CloseThreatPhysicalProjectileHits}/{physicalCloseFire.CloseThreatPhysicalProjectileImpactAttempts}");
            builder.Append(" | ");
            builder.Append(
                $"{physicalCloseFire.CloseThreatDamageTaken:0.0}/{physicalCloseFire.CloseThreatHealthRemaining:0.0}");
            builder.Append(" | ");
            builder.Append(
                $"{physicalCloseFire.CloseThreatNonLockingDamageEvents}/{physicalCloseFire.CloseThreatLockingDamageEvents}/{physicalCloseFire.CloseThreatFullBodyEligibleDamageEvents}");
            builder.Append(" | ");
            builder.Append(
                $"{physicalCloseFire.CloseThreatPhysicalProjectileBossHits}/{physicalCloseFire.BossDamageFromPlayer:0.0}");
            builder.Append(" | ");
            builder.Append(
                $"{EscapeTable(physicalCloseFire.SelectorDefaultTarget)}/{EscapeTable(physicalCloseFire.SelectorAttackAimTarget)}");
            builder.Append(" | ");
            builder.Append(
                $"{EscapeTable(physicalCloseFire.CloseThreatPhysicalLastAimTarget)} x{physicalCloseFire.CloseThreatPhysicalLastAimAssistStrength:0.00}");
            builder.Append(" | ");
            builder.Append(
                $"{physicalCloseFire.CloseThreatPhysicalLastRawAngleToClose:0.0}/{physicalCloseFire.CloseThreatPhysicalLastResolvedAngleToClose:0.0}");
            builder.Append(" | ");
            builder.Append(
                $"{FormatSeconds(physicalCloseFire.FirstBossPressureReleaseAtSeconds)} after shot {physicalCloseFire.CloseThreatPhysicalShotsBeforeBossPressure}");
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolvePhysicalCloseFireRead(physicalCloseFire)));
            builder.AppendLine(" |");
        }

        private static void AppendTargetPriorityContract(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Target Priority Contract");
            builder.AppendLine("| Policy | Basic shots | Close hits/dmg/HP | Close response N/L/F | Boss hits/dmg | First unresolved | Result records | Target read |");
            builder.AppendLine("|---|---:|---:|---:|---:|---|---:|---|");
            AppendTargetPriorityRow(builder, RequireResult(results, PolicyKind.CloseProbePhysicalFireProbe));
            AppendTargetPriorityRow(builder, RequireResult(results, PolicyKind.CloseProbePhysicalThenScreenCurtainProbe));
            AppendTargetPriorityRow(builder, RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe));
            AppendTargetPriorityRow(
                builder,
                RequireResult(results, PolicyKind.BossTunnelVisionIgnoresCloseProbe));
            AppendTargetPriorityRow(builder, RequireResult(results, PolicyKind.GunOnly));
            AppendTargetPriorityRow(builder, RequireResult(results, PolicyKind.PrematureSkill1NoSummon));
            AppendTargetPriorityRow(builder, RequireResult(results, PolicyKind.IntendedRoute));
        }

        private static void AppendTargetPriorityRow(StringBuilder builder, PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(result.BasicShots);
            builder.Append(" | ");
            builder.Append(
                $"{result.CloseThreatBasicHits}/{result.CloseThreatDamageTaken:0.0}/{result.CloseThreatHealthRemaining:0.0}");
            builder.Append(" | ");
            builder.Append(
                $"{result.CloseThreatNonLockingDamageEvents}/{result.CloseThreatLockingDamageEvents}/{result.CloseThreatFullBodyEligibleDamageEvents}");
            builder.Append(" | ");
            builder.Append($"{result.BossBasicHits}/{result.BossDamageFromPlayer:0.0}");
            builder.Append(" | ");
            builder.Append(ResolveFirstUnresolvedBeat(result));
            builder.Append(" | ");
            builder.Append(result.ResultRecords);
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveTargetPriorityRead(result)));
            builder.AppendLine(" |");
        }

        private static string ResolveTargetPriorityRead(PolicyMetrics result)
        {
            if (result.Policy == PolicyKind.CloseProbePhysicalFireProbe)
            {
                return ResolvePhysicalCloseFireRead(result);
            }

            if (result.Policy == PolicyKind.CloseProbePhysicalThenScreenCurtainProbe)
            {
                return result.BossPressureSummonReleases > 0 && result.MaxBossPressureActiveScreenCount > 0
                    ? "physical close clear advanced to screen curtain"
                    : "physical close clear did not advance pressure slot";
            }

            if (result.Policy == PolicyKind.CloseProbePhysicalThenSummonPunishProbe)
            {
                return result.IsClearResult && result.FollowupHitCount > 0
                    ? "physical close clear chained into summon punish"
                    : "physical close clear did not complete summon punish";
            }

            if (result.Policy == PolicyKind.BossTunnelVisionIgnoresCloseProbe)
            {
                return "boss chip before close-probe clear";
            }

            if (result.CloseThreatBasicHits > 0 && result.BossBasicHits > 0)
            {
                return "close probe first, then boss chip";
            }

            if (result.CloseThreatBasicHits > 0 && result.FollowupHitCount > 0)
            {
                return "close probe first, then state-gated punish";
            }

            return result.CloseThreatBasicHits > 0 ? "close probe answered" : "close probe unresolved";
        }

        private static string ResolveSelectorProbeRead(PolicyMetrics result)
        {
            if (result.SelectorCandidateCount < 2)
            {
                return "missing authored close/boss candidates";
            }

            if (result.SelectorDefaultTarget == "CloseProbe"
                && result.SelectorAttackAimTarget == "CloseProbe")
            {
                return "close probe selected before boss lane";
            }

            return "selector drift before close probe";
        }

        private static string ResolvePhysicalCloseFireRead(PolicyMetrics result)
        {
            if (result.CloseThreatPhysicalProjectileHits <= 0)
            {
                return "close projectile path missing";
            }

            if (result.CloseThreatHealthRemaining > 0.01f)
            {
                return "close projectile path incomplete";
            }

            if (result.BossDamageFromPlayer > 0.01f || result.CloseThreatPhysicalProjectileBossHits > 0)
            {
                return "close fire drifted into boss DPS";
            }

            return "physical close fire answered close probe";
        }

        private static void AppendSkillGateContract(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Skill Gate Contract");
            builder.AppendLine("| Policy | Skill use/hit | Boss dmg player/summon | Follow-up hit | Follow-up cues screen/camera/VFX | Result records | Stage gate read |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|");
            AppendSkillGateRow(builder, RequireResult(results, PolicyKind.PrematureSkill1NoSummon));
            AppendSkillGateRow(builder, RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe));
            AppendSkillGateRow(builder, RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe));
            AppendSkillGateRow(builder, RequireResult(results, PolicyKind.IntendedRoute));
            AppendSkillGateRow(builder, RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery));
        }

        private static void AppendSkillGateRow(StringBuilder builder, PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append($"{result.SkillUses}/{result.SkillProjectileHits}");
            builder.Append(" | ");
            builder.Append($"{result.BossDamageFromPlayer:0.0}/{result.BossDamageFromAllySummon:0.0}");
            builder.Append(" | ");
            builder.Append(result.FollowupHitCount);
            builder.Append(" | ");
            builder.Append($"{result.FollowupHitScreenCueRequests}/{result.FollowupHitCameraCueRequests}/{result.FollowupHitVfxCueRequests}");
            builder.Append(" | ");
            builder.Append(result.ResultRecords);
            builder.Append(" | ");
            builder.Append(EscapeTable(
                result.FollowupHitCount > 0
                    ? "state-gated follow-up commit"
                    : $"raw Skill1 only; unresolved {ResolveFirstUnresolvedBeat(result)}"));
            builder.AppendLine(" |");
        }

        private static void AppendUnconfirmedFollowupCost(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Unconfirmed Follow-up Cost");
            builder.AppendLine("| Policy | Result | Follow-up hit/miss | Counter waves | Unanswered burden | Boss dmg player/summon | Skill1 hits | Result records |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|");
            AppendUnconfirmedFollowupCostRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonBlockProbe));
            AppendUnconfirmedFollowupCostRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe));
            AppendUnconfirmedFollowupCostRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe));
        }

        private static void AppendUnconfirmedFollowupCostRow(
            StringBuilder builder,
            PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(result.FollowupHitCount);
            builder.Append("/");
            builder.Append(result.FollowupMissCount);
            builder.Append(" | ");
            builder.Append(result.CounterWaves);
            builder.Append(" | ");
            builder.Append(FormatPercent01(result.UnansweredPressureBurdenShare01));
            builder.Append(" | ");
            builder.Append($"{result.BossDamageFromPlayer:0.0}/{result.BossDamageFromAllySummon:0.0}");
            builder.Append(" | ");
            builder.Append(result.SkillProjectileHits);
            builder.Append(" | ");
            builder.Append(result.ResultRecords);
            builder.AppendLine(" |");
        }

        private static void AppendArkDataCoverageSummary(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics noSummonSurvival = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics selectorProbe = RequireResult(results, PolicyKind.CloseProbeSelectorBiasProbe);
            PolicyMetrics physicalCloseFire = RequireResult(results, PolicyKind.CloseProbePhysicalFireProbe);
            PolicyMetrics physicalCloseCurtain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenScreenCurtainProbe);
            PolicyMetrics physicalCloseChain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe);
            PolicyMetrics bossTunnel = RequireResult(results, PolicyKind.BossTunnelVisionIgnoresCloseProbe);
            PolicyMetrics gunOnlySurvival = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics prematureSkill1 = RequireResult(results, PolicyKind.PrematureSkill1NoSummon);
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
            PolicyMetrics forwardRiskPhysicalSummonNoPunish = RequireResult(
                results,
                PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
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
                && physicalCloseChain.IsClearResult
                && blockedRecovery.ResultKind == "CounterRecoveryClear"
                && HasSingleReviewOnlyResultHook(noSummonSurvival)
                && HasSingleReviewOnlyResultHook(gunOnlySurvival)
                && HasSingleReviewOnlyResultHook(forwardRiskPhysicalSummonPunish)
                && HasSingleReviewOnlyResultHook(physicalCloseChain)
                && HasSingleReviewOnlyResultHook(blockedRecovery)
                && IsStageResultCopy(noSummonSurvival)
                && IsStageResultCopy(gunOnlySurvival)
                && IsStageResultCopy(forwardRiskPhysicalSummonPunish)
                && IsStageResultCopy(physicalCloseChain)
                && IsStageResultCopy(blockedRecovery);
            bool pressureSlotMeasured = forwardRiskEnergy.EnergyTier1DurationSeconds >= 0f
                && backlineEnergy.EnergyTier1DurationSeconds >= 0f
                && forwardRiskEnergy.EnergyTier1DurationSeconds < backlineEnergy.EnergyTier1DurationSeconds
                && forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests > 0
                && forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests > 0
                && forwardRiskEnergy.EnergyReadyScreenCueRequests > 0
                && forwardRiskEnergy.EnergyReadyVfxCueRequests > 0
                && forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > backlinePhysicalBarrage.PhysicalBarragePlayerHits
                && ignoredRecovery.UnansweredPressureBurdenShare01 > intended.UnansweredPressureBurdenShare01;
            bool combatPayloadMeasured = forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > 0
                && selectorProbe.SelectorCandidateCount > 1
                && selectorProbe.SelectorDefaultTarget == "CloseProbe"
                && selectorProbe.SelectorAttackAimTarget == "CloseProbe"
                && physicalCloseFire.CloseThreatPhysicalProjectileHits > 0
                && physicalCloseFire.CloseThreatHealthRemaining <= 0.01f
                && physicalCloseFire.BossDamageFromPlayer <= 0.01f
                && physicalCloseCurtain.BossPressureSummonReleases > 0
                && physicalCloseCurtain.MaxBossPressureActiveScreenCount > 0
                && physicalCloseChain.CloseThreatPhysicalProjectileHits > 0
                && physicalCloseChain.CloseThreatHealthRemaining <= 0.01f
                && physicalCloseChain.SummonBlocks > 0
                && physicalCloseChain.FollowupHitCount > 0
                && physicalCloseChain.ResultKind == "CleanFollowupClear"
                && bossTunnel.BossBasicHits > 0
                && bossTunnel.CloseThreatBasicHits == 0
                && ResolveFirstUnresolvedBeat(bossTunnel) == "CloseProbe"
                && gunOnly.CloseThreatBasicHits > 0
                && gunOnly.CloseThreatHealthRemaining <= 0.01f
                && gunOnly.CloseThreatNonLockingDamageEvents > 0
                && gunOnly.CloseThreatLockingDamageEvents == 0
                && gunOnly.CloseThreatFullBodyEligibleDamageEvents == 0
                && noSummon.PlayerDamageScreenCueRequests > 0
                && noSummon.PlayerDamageFeedbackRequests > 0
                && prematureSkill1.SkillUses > 0
                && prematureSkill1.SkillProjectileHits > 0
                && prematureSkill1.FollowupHitCount == 0
                && prematureSkill1.FollowupHitScreenCueRequests == 0
                && prematureSkill1.ResultRecords == 0
                && forwardRiskPhysicalSummonPunish.PlayerDamageScreenCueRequests == 0
                && forwardRiskPhysicalSummonBlock.SummonBlocks > 0
                && forwardRiskPhysicalSummonBlock.PhysicalBarragePlayerHits == 0
                && forwardRiskPhysicalSummonBlock.SummonPressureBlockCameraCueRequests
                    >= forwardRiskPhysicalSummonBlock.SummonBlocks
                && forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptFlashes
                    >= forwardRiskPhysicalSummonBlock.SummonBlocks
                && forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptVfxCueRequests
                    >= forwardRiskPhysicalSummonBlock.SummonBlocks
                && forwardRiskPhysicalSummonNoPunish.FollowupMissCount > 0
                && forwardRiskPhysicalSummonNoPunish.CounterWaveScreenCueRequests > 0
                && forwardRiskPhysicalSummonNoPunish.CounterWaveCameraCueRequests > 0
                && forwardRiskPhysicalSummonNoPunish.CounterWaveVfxCueRequests > 0
                && !forwardRiskPhysicalSummonNoPunish.IsClearResult
                && forwardRiskPhysicalSummonPunish.SkillProjectileHits > 0
                && forwardRiskPhysicalSummonPunish.BossLockingDamageEvents > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests > 0;
            bool pgrStateMeasured = intended.BlockToFollowupWindowSeconds >= 0f
                && intended.BlockToFollowupWindowSeconds <= 0.35f
                && prematureSkill1.SkillUses > 0
                && prematureSkill1.FollowupHitCount == 0
                && prematureSkill1.FollowupHitCinematicCueRequests == 0
                && blockedRecovery.CounterWaveAnswerEnergyPulse > 0f
                && blockedRecovery.CounterTriggerToAnswerSeconds >= 0f
                && blockedRecovery.CounterTriggerToAnswerSeconds <= 0.35f
                && delayedIntended.FollowupWindowRemainingAtFirstHitSeconds > 0f
                && delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds > 0f
                && noSummon.PlayerLockingDamageEvents == 0
                && !noSummon.LastPlayerDamageFeedbackInterruptedAction
                && gunOnly.BossLockingDamageEvents == 0
                && blockedRecovery.CounterWaveAnswerScreenCueRequests > 0
                && blockedRecovery.CounterWaveStabilizedCameraCueRequests > 0
                && blockedRecovery.CounterWaveStabilizedVfxCueRequests > 0
                && blockedRecovery.EnergyReadyScreenCueRequests > 0
                && blockedRecovery.EnergyReadyVfxCueRequests > 0
                && blockedRecovery.EnergySpendScreenCueRequests > 0
                && blockedRecovery.EnergySpendVfxCueRequests > 0
                && physicalCloseChain.BlockToFollowupWindowSeconds >= 0f
                && physicalCloseChain.BlockToFollowupWindowSeconds <= 0.35f
                && physicalCloseChain.FollowupHitCinematicCueRequests > 0
                && forwardRiskPhysicalSummonPunish.BossLockingDamageEvents > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitCinematicCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitCinematicFrameOverlayCount > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitSequenceBridgeRequests == 0;
            bool v1ScopeHeld = forwardRiskPhysicalSummonPunish.FollowupHitCinematicCueRequests > 0
                && forwardRiskPhysicalSummonPunish.FollowupHitSequenceBridgeRequests == 0;

            builder.AppendLine("## ArkData Coverage Summary");
            builder.AppendLine("| Reference lens | Status | Current evidence | Boundary kept |");
            builder.AppendLine("|---|---|---|---|");
            builder.AppendLine(
                "| NIKKE stage-result runtime | "
                + $"{FormatCoverageStatus(stageResultMeasured, "PARTIAL")} | "
                + $"bad routes commit fail hooks at {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)} / {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)} with copy `{noSummonSurvival.ResultRecordTitle}`/`{gunOnlySurvival.ResultRecordTitle}`; clean physical `{ResolveResultHookClass(forwardRiskPhysicalSummonPunish)}` copy `{forwardRiskPhysicalSummonPunish.ResultRecordTitle}`; live close-chain `{ResolveResultHookClass(physicalCloseChain)}`; boss-screen recovery `{ResolveResultHookClass(blockedRecovery)}` copy `{blockedRecovery.ResultRecordRouteLabel}` | "
                + "Reward/item persistence and campaign clear are intentionally not implemented in this V1 combat slice. |");
            builder.AppendLine(
                "| Stage pressure-slot discipline | "
                + $"{FormatCoverageStatus(pressureSlotMeasured)} | "
                + $"forward-risk LV1 {FormatSeconds(forwardRiskEnergy.EnergyTier1DurationSeconds)} vs backline {FormatSeconds(backlineEnergy.EnergyTier1DurationSeconds)}; energy cues screen/VFX F/R/S {forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests}/{forwardRiskEnergy.EnergyReadyScreenCueRequests}/{forwardRiskEnergy.EnergySpendScreenCueRequests} and {forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests}/{forwardRiskEnergy.EnergyReadyVfxCueRequests}/{forwardRiskEnergy.EnergySpendVfxCueRequests}; physical barrage hits {backlinePhysicalBarrage.PhysicalBarragePlayerHits}->{forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}; ignored burden {FormatPercent01(ignoredRecovery.UnansweredPressureBurdenShare01)} vs intended {FormatPercent01(intended.UnansweredPressureBurdenShare01)} | "
                + "No new wave manager or generated stage; all policies use the same authored scene/profile pocket. |");
            builder.AppendLine(
                "| CombatPayload runtime pipeline | "
                + $"{FormatCoverageStatus(combatPayloadMeasured)} | "
                + $"Target selection: selector default/aim `{selectorProbe.SelectorDefaultTarget}`/`{selectorProbe.SelectorAttackAimTarget}`, physical close fire hits {physicalCloseFire.CloseThreatPhysicalProjectileHits}/{physicalCloseFire.CloseThreatPhysicalProjectileImpactAttempts} with boss damage {physicalCloseFire.BossDamageFromPlayer:0.0}, close->curtain releases/screens {physicalCloseCurtain.BossPressureSummonReleases}/{physicalCloseCurtain.MaxBossPressureActiveScreenCount}, live close-chain blocks/Skill1/result {physicalCloseChain.SummonBlocks}/{physicalCloseChain.SkillProjectileHits}/{physicalCloseChain.ResultKind}, boss tunnel close/boss hits {bossTunnel.CloseThreatBasicHits}/{bossTunnel.BossBasicHits} unresolved {ResolveFirstUnresolvedBeat(bossTunnel)}; Close Target->Effect/Status: hits/damage/HP {gunOnly.CloseThreatBasicHits}/{gunOnly.CloseThreatDamageTaken:0.0}/{gunOnly.CloseThreatHealthRemaining:0.0}, response {gunOnly.CloseThreatNonLockingDamageEvents}/{gunOnly.CloseThreatLockingDamageEvents}/{gunOnly.CloseThreatFullBodyEligibleDamageEvents}; Target->Hit: forward barrage {forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}/{forwardRiskPhysicalBarrage.PhysicalBarrageTrackedProjectileCount}; Resource/Skill gate: premature Skill1 use/hit {prematureSkill1.SkillUses}/{prematureSkill1.SkillProjectileHits} with follow-up/result {prematureSkill1.FollowupHitCount}/{prematureSkill1.ResultRecords}; Player Hit->Presentation: routine damage cues {noSummon.PlayerDamageScreenCueRequests}/{noSummon.PlayerDamageFeedbackRequests}, clean route {forwardRiskPhysicalSummonPunish.PlayerDamageScreenCueRequests}/{forwardRiskPhysicalSummonPunish.PlayerDamageFeedbackRequests}; Block->Status/Presentation: {forwardRiskPhysicalSummonBlock.SummonBlocks} blocks, {FormatSeconds(forwardRiskPhysicalSummonBlock.BlockToFollowupWindowSeconds)} to window, cues {forwardRiskPhysicalSummonBlock.SummonPressureBlockCameraCueRequests}/{forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptFlashes}/{forwardRiskPhysicalSummonBlock.SummonPressureScreenInterceptVfxCueRequests}; NoHit->Counter/Presentation: miss {forwardRiskPhysicalSummonNoPunish.FollowupMissCount} / counter {forwardRiskPhysicalSummonNoPunish.CounterWaves}, cues {forwardRiskPhysicalSummonNoPunish.CounterWaveScreenCueRequests}/{forwardRiskPhysicalSummonNoPunish.CounterWaveCameraCueRequests}/{forwardRiskPhysicalSummonNoPunish.CounterWaveVfxCueRequests}; Skill1 Hit->Presentation: {forwardRiskPhysicalSummonPunish.SkillProjectileHits} hits with cues {forwardRiskPhysicalSummonPunish.FollowupHitScreenCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitCameraCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitVfxCueRequests}; payoff source player/summon {forwardRiskPhysicalSummonPunish.BossDamageFromPlayer:0.0}/{forwardRiskPhysicalSummonPunish.BossDamageFromAllySummon:0.0} | "
                + "Candidate labels stay local test evidence, not fake universal opcodes. |");
            builder.AppendLine(
                "| PGR state-lock and hit-response grammar | "
                + $"{FormatCoverageStatus(pgrStateMeasured)} | "
                + $"premature Skill1 hit/follow-up {prematureSkill1.SkillProjectileHits}/{prematureSkill1.FollowupHitCount}; block->window {FormatSeconds(intended.BlockToFollowupWindowSeconds)}; counter answer pulse {blockedRecovery.CounterWaveAnswerEnergyPulse:0} with answer cues {blockedRecovery.CounterWaveAnswerScreenCueRequests}/{blockedRecovery.CounterWaveStabilizedCameraCueRequests}/{blockedRecovery.CounterWaveStabilizedVfxCueRequests} and energy ready/spend {blockedRecovery.EnergyReadyScreenCueRequests}/{blockedRecovery.EnergySpendScreenCueRequests} screen, {blockedRecovery.EnergyReadyVfxCueRequests}/{blockedRecovery.EnergySpendVfxCueRequests} VFX; delayed clean/recovery margins {FormatSeconds(delayedIntended.FollowupWindowRemainingAtFirstHitSeconds)} / {FormatSeconds(delayedBlockedRecovery.FollowupWindowRemainingAtFirstHitSeconds)}; routine lock counts {noSummon.PlayerLockingDamageEvents}/{gunOnly.BossLockingDamageEvents}, player feedback interrupt {noSummon.LastPlayerDamageFeedbackInterruptedAction}; punish boss locks {forwardRiskPhysicalSummonPunish.BossLockingDamageEvents}, micro-cine hit/frame {forwardRiskPhysicalSummonPunish.FollowupHitCinematicCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitCinematicFrameOverlayCount} | "
                + "Use lock/unlock and response tiers only; do not import tutorial HUD flow as the solution. |");
            builder.AppendLine(
                "| V1 scope guardrail | "
                + $"{FormatCoverageStatus(v1ScopeHeld)} | "
                + $"report scene `{ScenePath}`; micro-cinematic director/sequence hit counts {forwardRiskPhysicalSummonPunish.FollowupHitCinematicCueRequests}/{forwardRiskPhysicalSummonPunish.FollowupHitSequenceBridgeRequests}; physical clean route still clears `{forwardRiskPhysicalSummonPunish.ResultKind}` | "
                + "No new canonical scene, full sequence playback, broad VFX/audio restoration, roster, reward economy, or stage-select work. |");
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

        private static void AssertStageWaveBeatMap(IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics closeCurtain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenScreenCurtainProbe);
            PolicyMetrics closeChain =
                RequireResult(results, PolicyKind.CloseProbePhysicalThenSummonPunishProbe);
            PolicyMetrics bossTunnel = RequireResult(results, PolicyKind.BossTunnelVisionIgnoresCloseProbe);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics prematureSkill1 = RequireResult(results, PolicyKind.PrematureSkill1NoSummon);
            PolicyMetrics noPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
            PolicyMetrics physicalPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics blockedFollowup = RequireResult(results, PolicyKind.BossScreenBlockedFollowup);
            PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);

            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(closeCurtain),
                "After a physical close-probe answer, the stage beat should advance to the screen curtain.");
            Assert.That(
                ResolveStageWaveJudgement(closeCurtain),
                Does.Contain("ScreenCurtain"),
                "The physical close-to-curtain probe should stall at the next pressure slot, not at the close target.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(closeChain),
                "Live physical close fire followed by summon block and Skill1 should complete the clean beat chain.");
            Assert.AreEqual(
                "CloseProbe",
                ResolveFirstUnresolvedBeat(bossTunnel),
                "Boss tunnel vision should be classified as stopping before the close-probe target priority beat is answered.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(gunOnly),
                "Gun-only should be classified as stopping at the summon-needed screen curtain beat.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(prematureSkill1),
                "A premature Skill1 hit should still be classified as stopping at the summon-needed screen curtain beat.");
            Assert.AreEqual(
                "FollowupConfirm",
                ResolveFirstUnresolvedBeat(noPunish),
                "Physical summon block without Skill1 should be classified as an unconfirmed follow-up beat.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(physicalPunish),
                "Physical summon punish should complete the clean stage beat chain.");
            Assert.AreEqual(
                "CounterAnswer",
                ResolveFirstUnresolvedBeat(blockedFollowup),
                "A boss-screen-blocked follow-up should classify the next missing beat as the counter answer.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(blockedRecovery),
                "Boss-screen recovery should complete through the counter-answer beat.");
        }

        private static bool IsStageRoutePolicy(PolicyKind policy)
        {
            switch (policy)
            {
                case PolicyKind.NoSummonNoFire:
                case PolicyKind.GunOnly:
                case PolicyKind.CloseProbePhysicalFireProbe:
                case PolicyKind.CloseProbePhysicalThenScreenCurtainProbe:
                case PolicyKind.CloseProbePhysicalThenSummonPunishProbe:
                case PolicyKind.BossTunnelVisionIgnoresCloseProbe:
                case PolicyKind.NoSummonSurvivalLimit:
                case PolicyKind.GunOnlySurvivalLimit:
                case PolicyKind.PrematureSkill1NoSummon:
                case PolicyKind.ForwardRiskPhysicalSummonBlockProbe:
                case PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe:
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                case PolicyKind.IntendedRoute:
                case PolicyKind.IntendedDelayedFollowup:
                case PolicyKind.LateSummon:
                case PolicyKind.MissedFollowupCounterRecovery:
                case PolicyKind.BossScreenBlockedFollowup:
                case PolicyKind.BossScreenIgnoredNoRecovery:
                case PolicyKind.BossScreenBlockCounterRecovery:
                case PolicyKind.BossScreenDelayedCounterRecovery:
                    return true;
                default:
                    return false;
            }
        }

        private static string ResolveCloseProbeBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            return result.CloseThreatBasicHits > 0 ? "PASS" : "MISS";
        }

        private static string ResolveScreenCurtainBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            if (result.SummonBlocks > 0 && result.FollowupWindowOpenCount > 0)
            {
                return "PASS";
            }

            return result.ResultKind == "PlayerDownFail" ? "FAILED" : "PENDING";
        }

        private static string ResolveFollowupBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            if (result.FollowupHitCount > 0 && result.SkillProjectileHits > 0)
            {
                return "PASS";
            }

            if (result.BossBlockedSkill1Followup)
            {
                return "BLOCKED";
            }

            if (result.FollowupMissCount > 0)
            {
                return "MISS";
            }

            return result.ResultKind == "PlayerDownFail" ? "FAILED" : "PENDING";
        }

        private static string ResolveCounterPressureBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            if (result.CounterRecoveryConfirmed)
            {
                return "RECOVERED";
            }

            if (result.CounterWaves > 0)
            {
                return "PENDING";
            }

            if (result.IsClearResult && result.FollowupHitCount > 0)
            {
                return "AVOIDED";
            }

            return "-";
        }

        private static string ResolveResultHookBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            if (result.ResultKind == "PlayerDownFail")
            {
                return "FAIL";
            }

            if (result.ResultKind == "CleanFollowupClear")
            {
                return "CLEAN";
            }

            if (result.ResultKind == "CounterRecoveryClear")
            {
                return "RECOVERY";
            }

            return result.ResultRecords > 0 ? "RECORDED" : "PENDING";
        }

        private static string ResolveFirstUnresolvedBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "probe-only";
            }

            if (ResolveCloseProbeBeat(result) != "PASS")
            {
                return result.ResultKind == "PlayerDownFail" ? "HPFailBeforeCloseProbe" : "CloseProbe";
            }

            if (ResolveScreenCurtainBeat(result) != "PASS")
            {
                return result.ResultKind == "PlayerDownFail" ? "HPFailBeforeScreenCurtain" : "ScreenCurtain";
            }

            string followup = ResolveFollowupBeat(result);
            if (followup == "MISS")
            {
                return "FollowupConfirm";
            }

            if (followup == "BLOCKED")
            {
                return "CounterAnswer";
            }

            if (followup != "PASS")
            {
                return "FollowupConfirm";
            }

            string counter = ResolveCounterPressureBeat(result);
            if (counter == "PENDING")
            {
                return "CounterAnswer";
            }

            if (ResolveResultHookBeat(result) == "PENDING")
            {
                return "ResultHook";
            }

            return "Complete";
        }

        private static string ResolveStageWaveBeatOutcome(PolicyMetrics result)
        {
            return $"close={ResolveCloseProbeBeat(result)}; "
                + $"curtain={ResolveScreenCurtainBeat(result)}; "
                + $"followup={ResolveFollowupBeat(result)}; "
                + $"counter={ResolveCounterPressureBeat(result)}; "
                + $"result={ResolveResultHookBeat(result)}";
        }

        private static string ResolveStageWaveJudgement(PolicyMetrics result)
        {
            string unresolved = ResolveFirstUnresolvedBeat(result);
            if (unresolved == "probe-only")
            {
                return "measurement probe";
            }

            if (unresolved == "Complete")
            {
                return result.ResultKind == "CounterRecoveryClear"
                    ? "counter route complete"
                    : result.ResultKind == "CleanFollowupClear"
                        ? "clean route complete"
                        : "stage route complete";
            }

            if (result.ResultKind == "PlayerDownFail")
            {
                return $"failed at {unresolved}";
            }

            return $"stalled at {unresolved}";
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

        private static string ResolveResultCopyReadout(PolicyMetrics result)
        {
            if (result.ResultRecords <= 0)
            {
                return "pending";
            }

            return $"{result.ResultRecordTitle}: {result.ResultRecordSummary} [{result.ResultRecordRouteLabel}]";
        }

        private static string ResolveResultStageState(PolicyMetrics result)
        {
            if (result.ResultRecords <= 0)
            {
                return "pending";
            }

            if (result.ResultKind == "PlayerDownFail")
            {
                return "fail";
            }

            return result.IsClearResult ? "clear" : "recorded";
        }

        private static string ResolveResultHookClass(PolicyMetrics result)
        {
            if (result.ResultRecords <= 0)
            {
                return "pending";
            }

            switch (result.ResultKind)
            {
                case "CleanFollowupClear":
                    return "clean_survival";
                case "CounterRecoveryClear":
                    return "counter_recovery";
                case "PlayerDownFail":
                    return "failure_analysis";
                default:
                    return result.ResultKind;
            }
        }

        private static bool HasSingleReviewOnlyResultHook(PolicyMetrics result)
        {
            return result.ResultRecords == 1 && IsReviewOnlyResultHook(result);
        }

        private static bool IsReviewOnlyResultHook(PolicyMetrics result)
        {
            return result.ResultRecords > 0
                && ContainsOrdinalIgnoreCase(result.ResultRecordRewardHook, "logged")
                && !ContainsProgressionPayoutLanguage(result.ResultRecordRewardHook)
                && !ContainsProgressionPayoutLanguage(result.ResultRecordNextObjective);
        }

        private static bool IsStageResultCopy(PolicyMetrics result)
        {
            if (result.ResultRecords <= 0
                || ContainsBossHpResultLanguage(result.ResultRecordTitle)
                || ContainsBossHpResultLanguage(result.ResultRecordSummary)
                || ContainsBossHpResultLanguage(result.ResultRecordRouteLabel))
            {
                return false;
            }

            if (result.ResultKind == "PlayerDownFail")
            {
                return ContainsOrdinalIgnoreCase(result.ResultRecordTitle, "PLAYER DOWN")
                    && ContainsOrdinalIgnoreCase(result.ResultRecordSummary, "HP")
                    && ContainsOrdinalIgnoreCase(result.ResultRecordSummary, "pressure");
            }

            if (result.ResultKind == "CounterRecoveryClear")
            {
                return ContainsPressureResultTitle(result.ResultRecordTitle)
                    && ContainsOrdinalIgnoreCase(result.ResultRecordSummary, "Counter pressure")
                    && ContainsOrdinalIgnoreCase(result.ResultRecordRouteLabel, "Counter recovery");
            }

            if (result.ResultKind == "CleanFollowupClear")
            {
                return ContainsPressureResultTitle(result.ResultRecordTitle)
                    && ContainsOrdinalIgnoreCase(result.ResultRecordSummary, "Skill1")
                    && ContainsOrdinalIgnoreCase(result.ResultRecordRouteLabel, "Clean");
            }

            return ContainsPressureResultTitle(result.ResultRecordTitle)
                || ContainsOrdinalIgnoreCase(result.ResultRecordSummary, "pressure");
        }

        private static bool ContainsPressureResultTitle(string value)
        {
            return ContainsOrdinalIgnoreCase(value, "PRESSURE")
                || ContainsOrdinalIgnoreCase(value, "WAVE")
                || ContainsOrdinalIgnoreCase(value, "FRONTLINE");
        }

        private static bool ContainsBossHpResultLanguage(string value)
        {
            return ContainsOrdinalIgnoreCase(value, "BOSS CLEAR")
                || ContainsOrdinalIgnoreCase(value, "BOSS KILL")
                || ContainsOrdinalIgnoreCase(value, "BOSS HP")
                || ContainsOrdinalIgnoreCase(value, "DAMAGE BOSS")
                || ContainsOrdinalIgnoreCase(value, "HP BAR");
        }

        private static bool ContainsProgressionPayoutLanguage(string value)
        {
            return ContainsOrdinalIgnoreCase(value, "CommandExp")
                || ContainsOrdinalIgnoreCase(value, "TacticChip")
                || ContainsOrdinalIgnoreCase(value, "SummonCore")
                || ContainsOrdinalIgnoreCase(value, "StyleMedal")
                || ContainsOrdinalIgnoreCase(value, "currency")
                || ContainsOrdinalIgnoreCase(value, "inventory")
                || ContainsOrdinalIgnoreCase(value, "permanent")
                || ContainsOrdinalIgnoreCase(value, "progression grant")
                || ContainsOrdinalIgnoreCase(value, "reward_id")
                || ContainsOrdinalIgnoreCase(value, "item_id");
        }

        private static bool ContainsOrdinalIgnoreCase(string value, string pattern)
        {
            return (value ?? string.Empty).IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0;
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

        private static string BuildJson(
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
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
                builder.AppendLine($"      \"bossDamageFromPlayer\": {result.BossDamageFromPlayer:0.###},");
                builder.AppendLine($"      \"bossDamageFromAllySummon\": {result.BossDamageFromAllySummon:0.###},");
                builder.AppendLine($"      \"bossDamageFromEnemy\": {result.BossDamageFromEnemy:0.###},");
                builder.AppendLine($"      \"bossDamageFromNeutralOrUnknown\": {result.BossDamageFromNeutralOrUnknown:0.###},");
                builder.AppendLine($"      \"bossDamagePlayerShare01\": {result.BossDamagePlayerShare01:0.###},");
                builder.AppendLine($"      \"firstBossDownAtSeconds\": {JsonNullableSeconds(result.FirstBossDownAtSeconds)},");
                builder.AppendLine($"      \"survivalProbeMaxSeconds\": {JsonNullableSeconds(result.SurvivalProbeMaxSeconds)},");
                builder.AppendLine($"      \"closeThreatBasicHits\": {result.CloseThreatBasicHits},");
                builder.AppendLine($"      \"closeThreatDamageTaken\": {result.CloseThreatDamageTaken:0.###},");
                builder.AppendLine($"      \"closeThreatHealthRemaining\": {result.CloseThreatHealthRemaining:0.###},");
                builder.AppendLine($"      \"selectorCandidateCount\": {result.SelectorCandidateCount},");
                builder.AppendLine($"      \"selectorDefaultTarget\": \"{JsonEscape(result.SelectorDefaultTarget)}\",");
                builder.AppendLine($"      \"selectorAttackAimTarget\": \"{JsonEscape(result.SelectorAttackAimTarget)}\",");
                builder.AppendLine($"      \"selectorCloseDistance\": {JsonNullableSeconds(result.SelectorCloseDistance)},");
                builder.AppendLine($"      \"selectorBossDistance\": {JsonNullableSeconds(result.SelectorBossDistance)},");
                builder.AppendLine($"      \"selectorAttackAimAngleToClose\": {JsonNullableSeconds(result.SelectorAttackAimAngleToClose)},");
                builder.AppendLine($"      \"selectorAttackAimAngleToBoss\": {JsonNullableSeconds(result.SelectorAttackAimAngleToBoss)},");
                builder.AppendLine($"      \"closeThreatPhysicalProjectileImpactAttempts\": {result.CloseThreatPhysicalProjectileImpactAttempts},");
                builder.AppendLine($"      \"closeThreatPhysicalProjectileHits\": {result.CloseThreatPhysicalProjectileHits},");
                builder.AppendLine($"      \"closeThreatPhysicalProjectileBossHits\": {result.CloseThreatPhysicalProjectileBossHits},");
                builder.AppendLine($"      \"closeThreatPhysicalLastImpactTarget\": \"{JsonEscape(result.CloseThreatPhysicalLastImpactTarget)}\",");
                builder.AppendLine($"      \"closeThreatPhysicalLastImpactResult\": \"{JsonEscape(result.CloseThreatPhysicalLastImpactResult)}\",");
                builder.AppendLine($"      \"closeThreatPhysicalLastAimTarget\": \"{JsonEscape(result.CloseThreatPhysicalLastAimTarget)}\",");
                builder.AppendLine($"      \"closeThreatPhysicalLastAimAssistStrength\": {result.CloseThreatPhysicalLastAimAssistStrength:0.###},");
                builder.AppendLine($"      \"closeThreatPhysicalLastRawAngleToClose\": {result.CloseThreatPhysicalLastRawAngleToClose:0.###},");
                builder.AppendLine($"      \"closeThreatPhysicalLastResolvedAngleToClose\": {result.CloseThreatPhysicalLastResolvedAngleToClose:0.###},");
                builder.AppendLine($"      \"closeThreatPhysicalLastRawAngleToBoss\": {result.CloseThreatPhysicalLastRawAngleToBoss:0.###},");
                builder.AppendLine($"      \"closeThreatPhysicalLastResolvedAngleToBoss\": {result.CloseThreatPhysicalLastResolvedAngleToBoss:0.###},");
                builder.AppendLine($"      \"closeThreatPhysicalShotsBeforeBossPressure\": {result.CloseThreatPhysicalShotsBeforeBossPressure},");
                builder.AppendLine($"      \"closeThreatPhysicalFireReadout\": \"{JsonEscape(result.CloseThreatPhysicalFireReadout)}\",");
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
                builder.AppendLine($"      \"energyScreenCueRequests\": {result.EnergyScreenCueRequests},");
                builder.AppendLine($"      \"forwardRiskEnergyScreenCueRequests\": {result.ForwardRiskEnergyScreenCueRequests},");
                builder.AppendLine($"      \"energyReadyScreenCueRequests\": {result.EnergyReadyScreenCueRequests},");
                builder.AppendLine($"      \"energySpendScreenCueRequests\": {result.EnergySpendScreenCueRequests},");
                builder.AppendLine($"      \"lastEnergyScreenCueTier\": {result.LastEnergyScreenCueTier},");
                builder.AppendLine($"      \"forwardRiskEnergyVfxCueRequests\": {result.ForwardRiskEnergyVfxCueRequests},");
                builder.AppendLine($"      \"energyReadyVfxCueRequests\": {result.EnergyReadyVfxCueRequests},");
                builder.AppendLine($"      \"energySpendVfxCueRequests\": {result.EnergySpendVfxCueRequests},");
                builder.AppendLine($"      \"lastEnergyReadyVfxTier\": {result.LastEnergyReadyVfxTier},");
                builder.AppendLine($"      \"lastEnergySpendVfxTier\": {result.LastEnergySpendVfxTier},");
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
                builder.AppendLine($"      \"playerDamageScreenCueRequests\": {result.PlayerDamageScreenCueRequests},");
                builder.AppendLine($"      \"playerDamageFeedbackRequests\": {result.PlayerDamageFeedbackRequests},");
                builder.AppendLine($"      \"lastPlayerDamageFeedbackIntensity\": {result.LastPlayerDamageFeedbackIntensity:0.###},");
                builder.AppendLine($"      \"lastPlayerDamageFeedbackDuration\": {result.LastPlayerDamageFeedbackDuration:0.###},");
                builder.AppendLine($"      \"lastPlayerDamageFeedbackPolicyScale\": {result.LastPlayerDamageFeedbackPolicyScale:0.###},");
                builder.AppendLine($"      \"lastPlayerDamageResponsePolicy\": \"{JsonEscape(result.LastPlayerDamageResponsePolicy)}\",");
                builder.AppendLine($"      \"lastPlayerDamageControlLockPolicy\": \"{JsonEscape(result.LastPlayerDamageControlLockPolicy)}\",");
                builder.AppendLine($"      \"lastPlayerDamageFeedbackInterruptedAction\": {result.LastPlayerDamageFeedbackInterruptedAction.ToString().ToLowerInvariant()},");
                builder.AppendLine($"      \"bossNonLockingDamageEvents\": {result.BossNonLockingDamageEvents},");
                builder.AppendLine($"      \"bossLockingDamageEvents\": {result.BossLockingDamageEvents},");
                builder.AppendLine($"      \"bossFullBodyEligibleDamageEvents\": {result.BossFullBodyEligibleDamageEvents},");
                builder.AppendLine($"      \"closeThreatNonLockingDamageEvents\": {result.CloseThreatNonLockingDamageEvents},");
                builder.AppendLine($"      \"closeThreatLockingDamageEvents\": {result.CloseThreatLockingDamageEvents},");
                builder.AppendLine($"      \"closeThreatFullBodyEligibleDamageEvents\": {result.CloseThreatFullBodyEligibleDamageEvents},");
                builder.AppendLine($"      \"summonPressureBlockCameraCueRequests\": {result.SummonPressureBlockCameraCueRequests},");
                builder.AppendLine($"      \"lastSummonPressureBlockCameraTier\": {result.LastSummonPressureBlockCameraTier},");
                builder.AppendLine($"      \"summonBlockOpportunityCameraCueRequests\": {result.SummonBlockOpportunityCameraCueRequests},");
                builder.AppendLine($"      \"summonPressureScreenActivationVfxCueRequests\": {result.SummonPressureScreenActivationVfxCueRequests},");
                builder.AppendLine($"      \"summonPressureScreenInterceptFlashes\": {result.SummonPressureScreenInterceptFlashes},");
                builder.AppendLine($"      \"summonPressureScreenInterceptVfxCueRequests\": {result.SummonPressureScreenInterceptVfxCueRequests},");
                builder.AppendLine($"      \"maxShowingSummonPressureScreenPresenters\": {result.MaxShowingSummonPressureScreenPresenters},");
                builder.AppendLine($"      \"counterWaveScreenCueRequests\": {result.CounterWaveScreenCueRequests},");
                builder.AppendLine($"      \"counterWaveAnswerScreenCueRequests\": {result.CounterWaveAnswerScreenCueRequests},");
                builder.AppendLine($"      \"lastCounterWaveScreenSource\": \"{JsonEscape(result.LastCounterWaveScreenSource)}\",");
                builder.AppendLine($"      \"lastCounterWaveScreenAnswer\": \"{JsonEscape(result.LastCounterWaveScreenAnswer)}\",");
                builder.AppendLine($"      \"counterWaveCameraCueRequests\": {result.CounterWaveCameraCueRequests},");
                builder.AppendLine($"      \"counterWaveStabilizedCameraCueRequests\": {result.CounterWaveStabilizedCameraCueRequests},");
                builder.AppendLine($"      \"lastCounterWaveCameraTier\": {result.LastCounterWaveCameraTier},");
                builder.AppendLine($"      \"lastCounterWaveStabilizedCameraTier\": {result.LastCounterWaveStabilizedCameraTier},");
                builder.AppendLine($"      \"counterWaveVfxCueRequests\": {result.CounterWaveVfxCueRequests},");
                builder.AppendLine($"      \"counterWaveStabilizedVfxCueRequests\": {result.CounterWaveStabilizedVfxCueRequests},");
                builder.AppendLine($"      \"lastCounterWaveVfxSource\": \"{JsonEscape(result.LastCounterWaveVfxSource)}\",");
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
                builder.AppendLine($"      \"stageWaveBeatOutcome\": \"{JsonEscape(ResolveStageWaveBeatOutcome(result))}\",");
                builder.AppendLine($"      \"stageWaveFirstUnresolvedBeat\": \"{JsonEscape(ResolveFirstUnresolvedBeat(result))}\",");
                builder.AppendLine($"      \"stageWaveJudgement\": \"{JsonEscape(ResolveStageWaveJudgement(result))}\",");
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
                builder.AppendLine($"      \"resultRecordStageState\": \"{JsonEscape(ResolveResultStageState(result))}\",");
                builder.AppendLine($"      \"resultRecordHookClass\": \"{JsonEscape(ResolveResultHookClass(result))}\",");
                builder.AppendLine($"      \"resultRecordReviewOnly\": {JsonBool(IsReviewOnlyResultHook(result))},");
                builder.AppendLine($"      \"routeDecision\": \"{JsonEscape(result.RouteDecision)}\",");
                builder.AppendLine($"      \"completionReadout\": \"{JsonEscape(result.CompletionReadout)}\"");
                builder.Append("    }");
                builder.AppendLine(i + 1 < results.Count ? "," : string.Empty);
            }

            builder.AppendLine("  ],");
            builder.AppendLine("  \"repeatability\": [");
            for (int i = 0; i < repeatabilityResults.Count; i++)
            {
                PolicyMetrics result = repeatabilityResults[i];
                builder.AppendLine("    {");
                builder.AppendLine($"      \"policy\": \"{result.Policy}\",");
                builder.AppendLine($"      \"resultKind\": \"{JsonEscape(result.ResultKind)}\",");
                builder.AppendLine($"      \"elapsedSeconds\": {result.ElapsedSeconds:0.###},");
                builder.AppendLine($"      \"playerDamageTaken\": {result.PlayerDamageTaken:0.###},");
                builder.AppendLine($"      \"bossDamageTaken\": {result.BossDamageTaken:0.###},");
                builder.AppendLine($"      \"bossDamagePlayerShare01\": {result.BossDamagePlayerShare01:0.###},");
                builder.AppendLine($"      \"physicalBarragePlayerHits\": {result.PhysicalBarragePlayerHits},");
                builder.AppendLine($"      \"physicalBarrageTrackedProjectileCount\": {result.PhysicalBarrageTrackedProjectileCount},");
                builder.AppendLine($"      \"physicalBarragePlayerDamage\": {result.PhysicalBarragePlayerDamage:0.###},");
                builder.AppendLine($"      \"summonBlocks\": {result.SummonBlocks},");
                builder.AppendLine($"      \"skillProjectileHits\": {result.SkillProjectileHits},");
                builder.AppendLine($"      \"followupHitCount\": {result.FollowupHitCount},");
                builder.AppendLine($"      \"followupMissCount\": {result.FollowupMissCount},");
                builder.AppendLine($"      \"counterWaves\": {result.CounterWaves},");
                builder.AppendLine($"      \"unansweredPressureBurdenShare01\": {result.UnansweredPressureBurdenShare01:0.###},");
                builder.AppendLine($"      \"followupHitCinematicCueRequests\": {result.FollowupHitCinematicCueRequests},");
                builder.AppendLine($"      \"followupHitSequenceBridgeRequests\": {result.FollowupHitSequenceBridgeRequests},");
                builder.AppendLine($"      \"firstPlayerDownAtSeconds\": {JsonNullableSeconds(result.FirstPlayerDownAtSeconds)},");
                builder.AppendLine($"      \"firstBossDownAtSeconds\": {JsonNullableSeconds(result.FirstBossDownAtSeconds)},");
                builder.AppendLine($"      \"enemyFrontlineBodyHits\": {result.EnemyFrontlineBodyHits},");
                builder.AppendLine($"      \"resultRecords\": {result.ResultRecords},");
                builder.AppendLine($"      \"verdict\": \"{JsonEscape(ResolveRepeatabilityVerdict(repeatabilityResults, result.Policy))}\"");
                builder.Append("    }");
                builder.AppendLine(i + 1 < repeatabilityResults.Count ? "," : string.Empty);
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AssertRepeatabilityGate(IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            for (int i = 0; i < RepeatabilityPolicyOrder.Length; i++)
            {
                PolicyKind policy = RepeatabilityPolicyOrder[i];
                Assert.AreEqual(
                    RepeatabilityProbeRuns,
                    CountPolicyResults(repeatabilityResults, policy),
                    $"Repeatability policy {policy} should have the expected repeated run count.");
                Assert.IsTrue(
                    IsRepeatabilityPassForPolicy(repeatabilityResults, policy),
                    $"Repeatability policy {policy} should preserve the expected structural outcome. Results: {BuildResultKindSet(repeatabilityResults, policy)}");
            }

            Assert.LessOrEqual(
                MaxMetric(
                    repeatabilityResults,
                    PolicyKind.ForwardRiskPhysicalSummonPunishProbe,
                    result => result.FollowupHitSequenceBridgeRequests),
                0f,
                "Repeated physical punish runs should keep the full sequence bridge disabled; only the micro-cinematic cue is in scope.");
        }

        private static string ResolveRepeatabilityVerdict(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) != RepeatabilityProbeRuns)
            {
                return "FAIL missing runs";
            }

            return IsRepeatabilityPassForPolicy(repeatabilityResults, policy) ? "PASS" : "CHECK";
        }

        private static bool IsRepeatabilityPassForPolicy(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            int seen = 0;
            float ignoredBossScreenDamageMin = MinMetric(
                repeatabilityResults,
                PolicyKind.BossScreenIgnoredNoRecovery,
                result => result.PlayerDamageTaken);

            for (int i = 0; i < repeatabilityResults.Count; i++)
            {
                PolicyMetrics result = repeatabilityResults[i];
                if (result.Policy != policy)
                {
                    continue;
                }

                seen++;
                if (!IsRepeatabilitySamplePass(result, ignoredBossScreenDamageMin))
                {
                    return false;
                }
            }

            return seen == RepeatabilityProbeRuns;
        }

        private static bool IsRepeatabilitySamplePass(
            PolicyMetrics result,
            float ignoredBossScreenDamageMin)
        {
            switch (result.Policy)
            {
                case PolicyKind.NoSummonSurvivalLimit:
                    return result.ResultKind == "PlayerDownFail"
                        && result.FirstPlayerDownAtSeconds >= 0f
                        && result.FirstPlayerDownAtSeconds <= SurvivalLimitProbeMaxSeconds;
                case PolicyKind.GunOnlySurvivalLimit:
                    return result.ResultKind == "PlayerDownFail"
                        && result.FirstPlayerDownAtSeconds >= 0f
                        && result.FirstPlayerDownAtSeconds <= SurvivalLimitProbeMaxSeconds
                        && result.CloseThreatBasicHits > 0
                        && result.CloseThreatHealthRemaining <= 0.01f
                        && result.CloseThreatNonLockingDamageEvents > 0
                        && result.CloseThreatLockingDamageEvents == 0
                        && result.CloseThreatFullBodyEligibleDamageEvents == 0
                        && result.FirstBossDownAtSeconds < 0f;
                case PolicyKind.CloseProbeSelectorBiasProbe:
                    return result.SelectorCandidateCount > 1
                        && result.SelectorDefaultTarget == "CloseProbe"
                        && result.SelectorAttackAimTarget == "CloseProbe"
                        && result.SelectorBossDistance > result.SelectorCloseDistance;
                case PolicyKind.CloseProbePhysicalFireProbe:
                    return !result.IsClearResult
                        && result.SelectorDefaultTarget == "CloseProbe"
                        && result.CloseThreatPhysicalProjectileHits > 0
                        && result.CloseThreatHealthRemaining <= 0.01f
                        && result.CloseThreatNonLockingDamageEvents > 0
                        && result.CloseThreatLockingDamageEvents == 0
                        && result.CloseThreatFullBodyEligibleDamageEvents == 0
                        && result.BossDamageFromPlayer <= 0.01f;
                case PolicyKind.CloseProbePhysicalThenScreenCurtainProbe:
                    return !result.IsClearResult
                        && result.SelectorDefaultTarget == "CloseProbe"
                        && result.CloseThreatPhysicalProjectileHits > 0
                        && result.CloseThreatHealthRemaining <= 0.01f
                        && result.BossPressureSummonReleases > 0
                        && result.MaxBossPressureActiveScreenCount > 0
                        && ResolveFirstUnresolvedBeat(result) == "ScreenCurtain"
                        && result.ResultRecords == 0
                        && result.BossDamageFromPlayer <= 0.01f;
                case PolicyKind.CloseProbePhysicalThenSummonPunishProbe:
                    return result.ResultKind == "CleanFollowupClear"
                        && result.SelectorDefaultTarget == "CloseProbe"
                        && result.CloseThreatPhysicalProjectileHits > 0
                        && result.CloseThreatHealthRemaining <= 0.01f
                        && result.BossPressureSummonReleases > 0
                        && result.SummonBlocks > 0
                        && result.SkillProjectileHits > 0
                        && result.FollowupHitCount > 0
                        && result.FollowupHitCinematicCueRequests > 0
                        && ResolveFirstUnresolvedBeat(result) == "Complete";
                case PolicyKind.BossTunnelVisionIgnoresCloseProbe:
                    return !result.IsClearResult
                        && result.BasicShots > 0
                        && result.BossBasicHits > 0
                        && result.CloseThreatBasicHits == 0
                        && result.BossDamageFromPlayer > 0f
                        && result.PlayerDamageTaken > 0f
                        && result.ResultRecords == 0
                        && ResolveFirstUnresolvedBeat(result) == "CloseProbe";
                case PolicyKind.PrematureSkill1NoSummon:
                    return !result.IsClearResult
                        && result.CloseThreatBasicHits > 0
                        && result.CloseThreatHealthRemaining <= 0.01f
                        && result.CloseThreatNonLockingDamageEvents > 0
                        && result.CloseThreatLockingDamageEvents == 0
                        && result.CloseThreatFullBodyEligibleDamageEvents == 0
                        && result.SkillUses > 0
                        && result.SkillProjectileHits > 0
                        && result.FollowupHitCount == 0
                        && result.FollowupHitScreenCueRequests == 0
                        && result.FollowupHitCinematicCueRequests == 0
                        && result.ResultRecords == 0
                        && ResolveFirstUnresolvedBeat(result) == "ScreenCurtain";
                case PolicyKind.BacklinePhysicalBarrageProbe:
                    return result.PhysicalBarragePlayerHits == 0
                        && result.PhysicalBarragePlayerDamage <= 0.01f;
                case PolicyKind.ForwardRiskPhysicalBarrageProbe:
                    return result.PhysicalBarragePlayerHits >= 3
                        && result.PhysicalBarragePlayerDamage > 0f;
                case PolicyKind.ForwardRiskPhysicalSummonBlockProbe:
                    return result.PhysicalBarragePlayerHits == 0
                        && result.PlayerDamageTaken <= 0.01f
                        && result.SummonBlocks > 0
                        && result.FollowupWindowOpenCount > 0
                        && result.BlockToFollowupWindowSeconds >= 0f
                        && result.BlockToFollowupWindowSeconds <= 0.5f;
                case PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe:
                    return !result.IsClearResult
                        && result.FollowupMissCount > 0
                        && result.CounterWaves > 0
                        && result.SkillProjectileHits == 0
                        && result.ResultRecords == 0
                        && result.UnansweredPressureBurdenShare01 >= 0.5f;
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    return result.ResultKind == "CleanFollowupClear"
                        && result.PhysicalBarragePlayerHits == 0
                        && result.SummonBlocks >= 2
                        && result.SkillProjectileHits >= 2
                        && result.FollowupHitCinematicCueRequests > 0
                        && result.FollowupHitSequenceBridgeRequests == 0
                        && result.BossDamagePlayerShare01 >= 0.72f;
                case PolicyKind.BossScreenIgnoredNoRecovery:
                    return !result.IsClearResult
                        && result.EnemyFrontlineBodyHits > 0
                        && result.PlayerDamageTaken > 0f
                        && result.ResultRecords == 0;
                case PolicyKind.BossScreenBlockCounterRecovery:
                    return result.ResultKind == "CounterRecoveryClear"
                        && result.SkillProjectileHits >= 2
                        && result.FollowupHitCinematicCueRequests > 0
                        && result.FollowupHitSequenceBridgeRequests == 0
                        && ignoredBossScreenDamageMin > 0f
                        && result.PlayerDamageTaken < ignoredBossScreenDamageMin;
                default:
                    return false;
            }
        }

        private static int CountPolicyResults(IReadOnlyList<PolicyMetrics> results, PolicyKind policy)
        {
            int count = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Policy == policy)
                {
                    count++;
                }
            }

            return count;
        }

        private static string BuildResultKindSet(IReadOnlyList<PolicyMetrics> results, PolicyKind policy)
        {
            List<string> values = new List<string>();
            HashSet<string> seen = new HashSet<string>();
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Policy != policy)
                {
                    continue;
                }

                string value = string.IsNullOrEmpty(results[i].ResultKind) ? "-" : results[i].ResultKind;
                if (seen.Add(value))
                {
                    values.Add(value);
                }
            }

            return values.Count > 0 ? string.Join(",", values) : "-";
        }

        private static float MinMetric(
            IReadOnlyList<PolicyMetrics> results,
            PolicyKind policy,
            Func<PolicyMetrics, float> selector)
        {
            bool found = false;
            float min = float.PositiveInfinity;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Policy != policy)
                {
                    continue;
                }

                found = true;
                min = Mathf.Min(min, selector(results[i]));
            }

            return found ? min : -1f;
        }

        private static float MaxMetric(
            IReadOnlyList<PolicyMetrics> results,
            PolicyKind policy,
            Func<PolicyMetrics, float> selector)
        {
            bool found = false;
            float max = float.NegativeInfinity;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Policy != policy)
                {
                    continue;
                }

                found = true;
                max = Mathf.Max(max, selector(results[i]));
            }

            return found ? max : -1f;
        }

        private static float AverageMetric(
            IReadOnlyList<PolicyMetrics> results,
            PolicyKind policy,
            Func<PolicyMetrics, float> selector)
        {
            int count = 0;
            float sum = 0f;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Policy != policy)
                {
                    continue;
                }

                count++;
                sum += selector(results[i]);
            }

            return count > 0 ? sum / count : -1f;
        }

        private static string FormatMinAverageMax(float min, float average, float max)
        {
            return min >= 0f ? $"{min:0.#}/{average:0.#}/{max:0.#}" : "-";
        }

        private static string FormatMinMax(float min, float max)
        {
            return min >= 0f ? $"{min:0.#}/{max:0.#}" : "-";
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

        private static void AssertStageResultHook(
            PolicyMetrics result,
            string expectedResultKind,
            string expectedRewardHookText,
            string expectedNextObjectiveText,
            string message)
        {
            Assert.AreEqual(1, result.ResultRecords, message);
            Assert.AreEqual(expectedResultKind, result.ResultKind, message);
            Assert.That(result.ResultRecordRewardHook, Does.Contain(expectedRewardHookText), message);
            Assert.That(result.ResultRecordNextObjective, Does.Contain(expectedNextObjectiveText), message);
            Assert.IsTrue(IsReviewOnlyResultHook(result), message);
        }

        private static void AssertStageResultCopy(
            PolicyMetrics result,
            string expectedTitleText,
            string expectedSummaryText,
            string message)
        {
            Assert.AreEqual(1, result.ResultRecords, message);
            Assert.That(result.ResultRecordTitle, Does.Contain(expectedTitleText), message);
            Assert.That(result.ResultRecordSummary, Does.Contain(expectedSummaryText), message);
            Assert.IsTrue(IsStageResultCopy(result), message);
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

        private static void DeactivateActivePlayerProjectiles()
        {
            LaneActionProjectile[] projectiles = FindActivePlayerProjectiles();
            for (int i = 0; i < projectiles.Length; i++)
            {
                projectiles[i].Deactivate();
            }
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

        private static BossBarrageProjectile[] FindActiveBossProjectiles(Material material = null)
        {
            BossBarrageProjectile[] projectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            List<BossBarrageProjectile> active = new List<BossBarrageProjectile>();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive
                    && (material == null || projectiles[i].LastPresentationMaterial == material))
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
                SummonEnergyVfxCuePresenter energyVfxCuePresenter,
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
                EnergyVfxCuePresenter = energyVfxCuePresenter;
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
                observedScreenPlayerDamageCueRequestCount = screenCuePresenter.PlayerDamageCueRequestCount;
                observedDamageFeedbackRequestCount = screenCuePresenter.DamageFeedbackRequestCount;
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
            public SummonEnergyVfxCuePresenter EnergyVfxCuePresenter { get; }
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
                SamplePlayerDamagePresentationBridge();
                SampleEnergyPresentationBridge();
                SampleSummonBlockPresentationBridge();
                SampleCounterWavePresentationBridge();
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
            private int observedScreenPlayerDamageCueRequestCount;
            private int observedDamageFeedbackRequestCount;
            private int observedCinematicPlayCount;
            private int observedSequenceBridgePlayCount;

            private void SamplePlayerDamagePresentationBridge()
            {
                int playerDamageCueDelta = Mathf.Max(
                    0,
                    ScreenCuePresenter.PlayerDamageCueRequestCount
                    - observedScreenPlayerDamageCueRequestCount);
                int damageFeedbackDelta = Mathf.Max(
                    0,
                    ScreenCuePresenter.DamageFeedbackRequestCount
                    - observedDamageFeedbackRequestCount);
                Metrics.PlayerDamageScreenCueRequests = playerDamageCueDelta;
                Metrics.PlayerDamageFeedbackRequests = damageFeedbackDelta;
                if (playerDamageCueDelta <= 0 && damageFeedbackDelta <= 0)
                {
                    return;
                }

                Metrics.LastPlayerDamageFeedbackIntensity =
                    ScreenCuePresenter.LastDamageFeedbackIntensity;
                Metrics.LastPlayerDamageFeedbackDuration =
                    ScreenCuePresenter.LastDamageFeedbackDuration;
                Metrics.LastPlayerDamageFeedbackPolicyScale =
                    ScreenCuePresenter.LastDamageFeedbackPolicyScale;
                Metrics.LastPlayerDamageResponsePolicy =
                    ScreenCuePresenter.LastDamageResponsePolicy.ToString();
                Metrics.LastPlayerDamageControlLockPolicy =
                    ScreenCuePresenter.LastDamageControlLockPolicy.ToString();
                Metrics.LastPlayerDamageFeedbackInterruptedAction =
                    ScreenCuePresenter.LastDamageFeedbackInterruptedAction;
            }

            private void SampleEnergyPresentationBridge()
            {
                Metrics.EnergyScreenCueRequests =
                    ScreenCuePresenter.EnergyCueRequestCount;
                Metrics.ForwardRiskEnergyScreenCueRequests =
                    ScreenCuePresenter.ForwardRiskCueRequestCount;
                Metrics.EnergyReadyScreenCueRequests =
                    ScreenCuePresenter.EnergyReadyCueRequestCount;
                Metrics.EnergySpendScreenCueRequests =
                    ScreenCuePresenter.EnergySpendCueRequestCount;
                Metrics.LastEnergyScreenCueTier =
                    ScreenCuePresenter.LastEnergyCueTier;

                Metrics.ForwardRiskEnergyVfxCueRequests =
                    EnergyVfxCuePresenter.ForwardRiskCueRequestCount;
                Metrics.EnergyReadyVfxCueRequests =
                    EnergyVfxCuePresenter.TierReadyCueRequestCount;
                Metrics.EnergySpendVfxCueRequests =
                    EnergyVfxCuePresenter.SpendCueRequestCount;
                Metrics.LastEnergyReadyVfxTier =
                    EnergyVfxCuePresenter.LastReadyTier;
                Metrics.LastEnergySpendVfxTier =
                    EnergyVfxCuePresenter.LastSpentTier;
            }

            private void SampleSummonBlockPresentationBridge()
            {
                Metrics.SummonPressureBlockCameraCueRequests =
                    CameraCueDriver.SummonPressureBlockCueRequestCount;
                Metrics.LastSummonPressureBlockCameraTier =
                    CameraCueDriver.LastSummonPressureBlockTier;
                Metrics.SummonBlockOpportunityCameraCueRequests =
                    CameraCueDriver.SummonBlockOpportunityCueRequestCount;

                SummonPressureScreenPresenter[] presenters =
                    Object.FindObjectsByType<SummonPressureScreenPresenter>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None);
                int activationVfxRequests = 0;
                int interceptFlashes = 0;
                int interceptVfxRequests = 0;
                int showingPresenters = 0;
                for (int i = 0; i < presenters.Length; i++)
                {
                    SummonPressureScreenPresenter presenter = presenters[i];
                    if (presenter == null)
                    {
                        continue;
                    }

                    activationVfxRequests += presenter.ActivationVfxCueRequestCount;
                    interceptFlashes += presenter.InterceptFlashCount;
                    interceptVfxRequests += presenter.InterceptVfxCueRequestCount;
                    if (presenter.IsShowing)
                    {
                        showingPresenters++;
                    }
                }

                Metrics.SummonPressureScreenActivationVfxCueRequests = Mathf.Max(
                    Metrics.SummonPressureScreenActivationVfxCueRequests,
                    activationVfxRequests);
                Metrics.SummonPressureScreenInterceptFlashes = Mathf.Max(
                    Metrics.SummonPressureScreenInterceptFlashes,
                    interceptFlashes);
                Metrics.SummonPressureScreenInterceptVfxCueRequests = Mathf.Max(
                    Metrics.SummonPressureScreenInterceptVfxCueRequests,
                    interceptVfxRequests);
                Metrics.MaxShowingSummonPressureScreenPresenters = Mathf.Max(
                    Metrics.MaxShowingSummonPressureScreenPresenters,
                    showingPresenters);
            }

            private void SampleCounterWavePresentationBridge()
            {
                Metrics.CounterWaveScreenCueRequests =
                    ScreenCuePresenter.CounterWaveCueRequestCount;
                Metrics.CounterWaveAnswerScreenCueRequests =
                    ScreenCuePresenter.CounterWaveAnswerCueRequestCount;
                Metrics.LastCounterWaveScreenSource =
                    ScreenCuePresenter.LastCounterWaveSource.ToString();
                Metrics.LastCounterWaveScreenAnswer =
                    ScreenCuePresenter.LastCounterWaveAnswer ?? string.Empty;

                Metrics.CounterWaveCameraCueRequests =
                    CameraCueDriver.CounterWaveCueRequestCount;
                Metrics.CounterWaveStabilizedCameraCueRequests =
                    CameraCueDriver.CounterWaveStabilizedCueRequestCount;
                Metrics.LastCounterWaveCameraTier =
                    CameraCueDriver.LastCounterWaveTier;
                Metrics.LastCounterWaveStabilizedCameraTier =
                    CameraCueDriver.LastCounterWaveStabilizedTier;

                Metrics.CounterWaveVfxCueRequests =
                    PocketVfxCueBridge.CounterWaveCueRequestCount;
                Metrics.CounterWaveStabilizedVfxCueRequests =
                    PocketVfxCueBridge.CounterWaveStabilizedCueRequestCount;
                Metrics.LastCounterWaveVfxSource =
                    PocketVfxCueBridge.LastCounterWaveSource.ToString();
            }

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

                Metrics.FollowupWindowCinematicCueRequests = Mathf.Max(
                    Metrics.FollowupWindowCinematicCueRequests,
                    CinematicCueDirector.BossPressureBreakPlayCount);
                Metrics.FollowupHitCinematicCueRequests = Mathf.Max(
                    Metrics.FollowupHitCinematicCueRequests,
                    CinematicCueDirector.SummonFollowupHitPlayCount);
                Metrics.FollowupHitCinematicFrameOverlayCount = Mathf.Max(
                    Metrics.FollowupHitCinematicFrameOverlayCount,
                    CinematicCueDirector.SummonFollowupHitFrameOverlayCount);
                Metrics.FollowupMissedCinematicCueRequests = Mathf.Max(
                    Metrics.FollowupMissedCinematicCueRequests,
                    CinematicCueDirector.SummonRecallPlayCount);
                if (CinematicCueDirector.SummonFollowupHitPlayCount > 0)
                {
                    Metrics.LastFollowupHitCinematicTier =
                        CinematicCueDirector.LastSummonFollowupHitTier;
                    Metrics.LastFollowupHitCinematicCueId =
                        CinematicCueDirector.LastSummonFollowupHitCueId ?? string.Empty;
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
                switch (damageInfo.SourceTeam)
                {
                    case DamageTeam.Player:
                        Metrics.BossDamageFromPlayer += damageInfo.Amount;
                        break;
                    case DamageTeam.AllySummon:
                        Metrics.BossDamageFromAllySummon += damageInfo.Amount;
                        break;
                    case DamageTeam.Enemy:
                        Metrics.BossDamageFromEnemy += damageInfo.Amount;
                        break;
                    default:
                        Metrics.BossDamageFromNeutralOrUnknown += damageInfo.Amount;
                        break;
                }

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
            public float BossDamageFromPlayer { get; set; }
            public float BossDamageFromAllySummon { get; set; }
            public float BossDamageFromEnemy { get; set; }
            public float BossDamageFromNeutralOrUnknown { get; set; }
            public float BossDamagePlayerShare01 =>
                BossDamageTaken > 0f ? Mathf.Clamp01(BossDamageFromPlayer / BossDamageTaken) : 0f;
            public float FirstBossDownAtSeconds { get; set; } = -1f;
            public float CloseThreatHealthStart { get; set; }
            public float CloseThreatHealthRemaining { get; set; }
            public float CloseThreatDamageTaken { get; set; }
            public int SelectorCandidateCount { get; set; }
            public string SelectorDefaultTarget { get; set; } = "None";
            public string SelectorAttackAimTarget { get; set; } = "None";
            public float SelectorCloseDistance { get; set; } = -1f;
            public float SelectorBossDistance { get; set; } = -1f;
            public float SelectorAttackAimAngleToClose { get; set; } = -1f;
            public float SelectorAttackAimAngleToBoss { get; set; } = -1f;
            public int CloseThreatPhysicalProjectileImpactAttempts { get; set; }
            public int CloseThreatPhysicalProjectileHits { get; set; }
            public int CloseThreatPhysicalProjectileBossHits { get; set; }
            public string CloseThreatPhysicalLastImpactTarget { get; set; } = "None";
            public string CloseThreatPhysicalLastImpactResult { get; set; } = "None";
            public string CloseThreatPhysicalLastAimTarget { get; set; } = "None";
            public float CloseThreatPhysicalLastAimAssistStrength { get; set; }
            public float CloseThreatPhysicalLastRawAngleToClose { get; set; }
            public float CloseThreatPhysicalLastResolvedAngleToClose { get; set; }
            public float CloseThreatPhysicalLastRawAngleToBoss { get; set; }
            public float CloseThreatPhysicalLastResolvedAngleToBoss { get; set; }
            public int CloseThreatPhysicalShotsBeforeBossPressure { get; set; }
            public string CloseThreatPhysicalFireReadout { get; set; } = string.Empty;
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
            public int SummonPressureBlockCameraCueRequests { get; set; }
            public int LastSummonPressureBlockCameraTier { get; set; }
            public int SummonBlockOpportunityCameraCueRequests { get; set; }
            public int SummonPressureScreenActivationVfxCueRequests { get; set; }
            public int SummonPressureScreenInterceptFlashes { get; set; }
            public int SummonPressureScreenInterceptVfxCueRequests { get; set; }
            public int MaxShowingSummonPressureScreenPresenters { get; set; }
            public int CounterWaveScreenCueRequests { get; set; }
            public int CounterWaveAnswerScreenCueRequests { get; set; }
            public string LastCounterWaveScreenSource { get; set; } = "None";
            public string LastCounterWaveScreenAnswer { get; set; } = string.Empty;
            public int CounterWaveCameraCueRequests { get; set; }
            public int CounterWaveStabilizedCameraCueRequests { get; set; }
            public int LastCounterWaveCameraTier { get; set; }
            public int LastCounterWaveStabilizedCameraTier { get; set; }
            public int CounterWaveVfxCueRequests { get; set; }
            public int CounterWaveStabilizedVfxCueRequests { get; set; }
            public string LastCounterWaveVfxSource { get; set; } = "None";
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
            public int EnergyScreenCueRequests { get; set; }
            public int ForwardRiskEnergyScreenCueRequests { get; set; }
            public int EnergyReadyScreenCueRequests { get; set; }
            public int EnergySpendScreenCueRequests { get; set; }
            public int LastEnergyScreenCueTier { get; set; }
            public int ForwardRiskEnergyVfxCueRequests { get; set; }
            public int EnergyReadyVfxCueRequests { get; set; }
            public int EnergySpendVfxCueRequests { get; set; }
            public int LastEnergyReadyVfxTier { get; set; }
            public int LastEnergySpendVfxTier { get; set; }
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
            public int PlayerDamageScreenCueRequests { get; set; }
            public int PlayerDamageFeedbackRequests { get; set; }
            public float LastPlayerDamageFeedbackIntensity { get; set; }
            public float LastPlayerDamageFeedbackDuration { get; set; }
            public float LastPlayerDamageFeedbackPolicyScale { get; set; } = 1f;
            public string LastPlayerDamageResponsePolicy { get; set; } = "None";
            public string LastPlayerDamageControlLockPolicy { get; set; } = "None";
            public bool LastPlayerDamageFeedbackInterruptedAction { get; set; }
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
