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
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
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
        private const string SummonSlot1ActionProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset";
        private const string SummonSlot2ActionProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_LaserSoldier.asset";
        private const string SummonSlot3ActionProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_FireDragon.asset";
        private const float PressureWindowSeconds = 3f;
        private const float ReliefPressureWindowPeakRatio = 0.35f;
        private const float DelayedPunishInputSeconds = 1.1f;
        private const float BacklineEnergyProbeForwardRisk01 = 0.12f;
        private const float ForwardEnergyProbeForwardRisk01 = 0.88f;
        private const float EnergyProbeMaxSeconds = 13f;
        private const float EnergyTierLadderProbeMaxSeconds = 36f;
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
            BacklineEnergyTierLadderProbe,
            ForwardRiskEnergyTierLadderProbe,
            ForwardRiskTier1DecisionRoute,
            ForwardRiskTier2DecisionRoute,
            ForwardRiskTier3DecisionRoute,
            ForwardRiskTier1RecoveryRoute,
            ForwardRiskTier2RecoveryRoute,
            ForwardRiskTier3RecoveryRoute,
            ForwardRiskSlot2MarksmanRoute,
            ForwardRiskSlot3VanguardRoute,
            ForwardRiskSlot2ThenSlot1ComboRoute,
            ForwardRiskSlot3ThenSlot1BlockedRoute,
            ForwardRiskSlot2ThenDelayedSlot1Route,
            ForwardRiskSlot3ThenDelayedSlot1Route,
            ForwardRiskSlot2ThenDelayedRecoveryRoute,
            ForwardRiskSlot3ThenDelayedRecoveryRoute,
            ForwardRiskSlot3RetreatThenDelayedRecoveryRoute,
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
            PolicyKind.BacklineEnergyTierLadderProbe,
            PolicyKind.ForwardRiskEnergyTierLadderProbe,
            PolicyKind.ForwardRiskTier1DecisionRoute,
            PolicyKind.ForwardRiskTier2DecisionRoute,
            PolicyKind.ForwardRiskTier3DecisionRoute,
            PolicyKind.ForwardRiskTier1RecoveryRoute,
            PolicyKind.ForwardRiskTier2RecoveryRoute,
            PolicyKind.ForwardRiskTier3RecoveryRoute,
            PolicyKind.ForwardRiskSlot2MarksmanRoute,
            PolicyKind.ForwardRiskSlot3VanguardRoute,
            PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute,
            PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute,
            PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route,
            PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route,
            PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute,
            PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute,
            PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute,
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
            PolicyKind.ForwardRiskEnergyProbe,
            PolicyKind.NoSummonNoFire,
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
            PolicyKind.IntendedRoute,
            PolicyKind.ForwardRiskTier1DecisionRoute,
            PolicyKind.ForwardRiskTier2DecisionRoute,
            PolicyKind.ForwardRiskTier3DecisionRoute,
            PolicyKind.ForwardRiskTier1RecoveryRoute,
            PolicyKind.ForwardRiskTier2RecoveryRoute,
            PolicyKind.ForwardRiskTier3RecoveryRoute,
            PolicyKind.ForwardRiskSlot2MarksmanRoute,
            PolicyKind.ForwardRiskSlot3VanguardRoute,
            PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute,
            PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute,
            PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route,
            PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route,
            PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute,
            PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute,
            PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute,
            PolicyKind.BossScreenIgnoredNoRecovery,
            PolicyKind.BossScreenBlockCounterRecovery
        };

        private static readonly PolicyKind[] RequiredRepeatabilityGatePolicyOrder =
        {
            PolicyKind.ForwardRiskEnergyProbe,
            PolicyKind.NoSummonNoFire,
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
            PolicyKind.IntendedRoute,
            PolicyKind.ForwardRiskTier1DecisionRoute,
            PolicyKind.ForwardRiskTier2DecisionRoute,
            PolicyKind.ForwardRiskTier3DecisionRoute,
            PolicyKind.ForwardRiskTier1RecoveryRoute,
            PolicyKind.ForwardRiskTier2RecoveryRoute,
            PolicyKind.ForwardRiskTier3RecoveryRoute,
            PolicyKind.ForwardRiskSlot3VanguardRoute,
            PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route,
            PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute,
            PolicyKind.BossScreenIgnoredNoRecovery,
            PolicyKind.BossScreenBlockCounterRecovery
        };

        [UnityTest]
        [Timeout(360000)]
        public IEnumerator WritesFrontlineCombatPolicyReport()
        {
            float previousTimeScale = Time.timeScale;
            bool previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            Time.timeScale = 8f;
            // Batch editor target scanning can emit unrelated Android SDK lock exceptions; metric assertions remain the gate.
            LogAssert.ignoreFailingMessages = true;
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
                PolicyMetrics backlineEnergyTierLadder =
                    RequireResult(results, PolicyKind.BacklineEnergyTierLadderProbe);
                PolicyMetrics forwardRiskEnergyTierLadder =
                    RequireResult(results, PolicyKind.ForwardRiskEnergyTierLadderProbe);
                PolicyMetrics forwardRiskTier1Decision =
                    RequireResult(results, PolicyKind.ForwardRiskTier1DecisionRoute);
                PolicyMetrics forwardRiskTier2Decision =
                    RequireResult(results, PolicyKind.ForwardRiskTier2DecisionRoute);
                PolicyMetrics forwardRiskTier3Decision =
                    RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute);
                PolicyMetrics forwardRiskTier1Recovery =
                    RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute);
                PolicyMetrics forwardRiskTier2Recovery =
                    RequireResult(results, PolicyKind.ForwardRiskTier2RecoveryRoute);
                PolicyMetrics forwardRiskTier3Recovery =
                    RequireResult(results, PolicyKind.ForwardRiskTier3RecoveryRoute);
                PolicyMetrics forwardRiskSlot2Marksman =
                    RequireResult(results, PolicyKind.ForwardRiskSlot2MarksmanRoute);
                PolicyMetrics forwardRiskSlot3Vanguard =
                    RequireResult(results, PolicyKind.ForwardRiskSlot3VanguardRoute);
                PolicyMetrics forwardRiskSlot2Combo =
                    RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
                PolicyMetrics forwardRiskSlot3Blocked =
                    RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute);
                PolicyMetrics forwardRiskSlot2Delayed =
                    RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route);
                PolicyMetrics forwardRiskSlot3Delayed =
                    RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route);
                PolicyMetrics forwardRiskSlot2DelayedRecovery =
                    RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute);
                PolicyMetrics forwardRiskSlot3DelayedRecovery =
                    RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
                PolicyMetrics forwardRiskSlot3RetreatRecovery =
                    RequireResult(results, PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute);
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
                    markdown.Contains("support marksman `support_marksman_clear`"),
                    "The ArkData summary should keep support route result hooks visible at the top level.");
                Assert.IsTrue(
                    markdown.Contains("support vanguard `support_vanguard_clear`"),
                    "The ArkData summary should keep high-cost support payoff hooks visible at the top level.");
                Assert.IsTrue(
                    markdown.Contains("## Stage Result Hook Contract"),
                    "The report should expose clean/counter/fail result hooks before route details.");
                Assert.IsTrue(
                    markdown.Contains("## Stage Result Motivation Matrix"),
                    "The report should summarize stage-result motivation hooks before reward economy work.");
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
                    markdown.Contains("## Summon Roster Mana/Effect Identity Audit"),
                    "The report should preserve the summon roster mana/effect identity gap instead of drifting into generic tier balancing.");
                Assert.IsTrue(
                    markdown.Contains("## EN Spend Decision Route"),
                    "The report should connect EN tier waiting to the actual summon -> Skill1 route, not stop at resource timing.");
                Assert.IsTrue(
                    markdown.Contains("## EN Spend Recovery Route"),
                    "The report should prove whether a boss-screen-blocked EN spend branch closes through counter recovery.");
                Assert.IsTrue(
                    markdown.Contains("## Shared Mana Support Combo Branch"),
                    "The report should prove whether support summon cost preserves or consumes the next main answer.");
                Assert.IsTrue(
                    markdown.Contains("## Shared Mana Delayed Main Answer Branch"),
                    "The report should prove whether early support spend can recharge into the next main answer.");
                Assert.IsTrue(
                    markdown.Contains("## Shared Mana Delayed Counter Recovery Branch"),
                    "The report should prove whether delayed support branches close through counter recovery.");
                Assert.IsTrue(
                    markdown.Contains("## Support Decision Matrix"),
                    "The report should compare summon choices as combat decisions, not only isolated policy rows.");
                Assert.IsTrue(
                    markdown.Contains("| Choice | Cost path | Support effect | HP before main | Physical hits | Slot1 state | Recovery burden | Boss suppress | Sample time/dmg | Result | Repeat band | Timing verdict | Payoff verdict | Read |"),
                    "The support decision matrix should separate sampled time/damage from repeated route bands.");
                Assert.IsTrue(
                    markdown.Contains("HP before main min/avg/max"),
                    "The support decision matrix should expose repeated HP-before-main bands for route comparison.");
                Assert.IsTrue(
                    markdown.Contains("## Support Payoff Vector Matrix"),
                    "The report should split support payoff into damage, prevention, and relock vectors before tuning summon roles.");
                Assert.IsTrue(
                    markdown.Contains("damage route, no relock"),
                    "The support payoff vector should keep Slot2's marksman payoff distinct from Slot3's prevention payoff.");
                Assert.IsTrue(
                    markdown.Contains("## Support Body-Cost Phase Matrix"),
                    "The report should reveal whether support route body cost is paid before or after the support summon.");
                Assert.IsTrue(
                    markdown.Contains("## Support Wait Exposure Matrix"),
                    "The report should expose LV2/LV3 wait exposure before changing support payoff tuning.");
                Assert.IsTrue(
                    markdown.Contains("## Support Upgrade Delta Matrix"),
                    "The report should expose the marginal LV2-to-LV3 wait cost before changing support payoff tuning.");
                Assert.IsTrue(
                    markdown.Contains("## Support Upgrade Decision Readout Matrix"),
                    "The report should connect visible summon readouts to measured LV2/LV3 tradeoffs before HUD polish.");
                Assert.IsTrue(
                    markdown.Contains("S2 ready for tempo"),
                    "The support upgrade readout should include the live route-incentive forecast for full-bank choices.");
                Assert.IsTrue(
                    markdown.Contains("## Support Stage-Slot Timeline Matrix"),
                    "The report should expose support choices as ordered stage slots, not only balance rows.");
                Assert.IsTrue(
                    markdown.Contains("## Route Motivation/Dominance Matrix"),
                    "The report should compare clear-route motivation before tuning one fastest route into every answer.");
                Assert.IsTrue(
                    markdown.Contains("High-tier movement agency"),
                    "The structural gate summary should keep the high-tier movement choice visible before detailed tables.");
                Assert.IsTrue(
                    markdown.Contains("| Route role | Policy | Cost/risk | Sample payoff | Result hook | Repeat band | Decision read |"),
                    "The route dominance matrix should separate one sample payoff from the repeated evidence band.");
                Assert.IsTrue(
                    markdown.Contains("boss min/avg/max"),
                    "The route dominance matrix should expose repeat boss-damage bands before balance decisions.");
                Assert.IsTrue(
                    markdown.Contains("Guided clean loop | IntendedRoute | close hits"),
                    "The route dominance matrix should keep the guided clean loop visible.");
                Assert.IsTrue(
                    markdown.Contains("Slot3 retreat recovery | ForwardRiskSlot3RetreatThenDelayedRecoveryRoute"),
                    "The route dominance matrix should compare Slot3 hold-front payoff against the retreat/recommit recovery route.");
                Assert.IsFalse(
                    markdown.Contains("Guided clean loop | IntendedRoute | close hits 6, charge -"),
                    "The guided clean loop cost read should expose summon/block timing instead of a missing generic charge field.");
                Assert.IsTrue(
                    markdown.Contains("Payoff verdict"),
                    "The support decision read should compare clear time and boss damage before tuning support routes.");
                Assert.IsTrue(
                    markdown.Contains("## Summon Slot Readiness/Cooldown Matrix"),
                    "The report should distinguish shared EN cost lockout from per-slot cooldown lockout before UI/coaster polish.");
                Assert.IsTrue(
                    markdown.Contains("## Summon HUD Readiness Readout Matrix"),
                    "The report should prove the review HUD/coaster readout follows shared EN cost and per-slot cooldown gates.");
                Assert.IsTrue(
                    markdown.Contains("## Enemy Pressure Tactical Cost Matrix"),
                    "The report should prove enemy pressure actors create unattended tactical cost, not timer-only bookkeeping.");
                Assert.IsTrue(
                    markdown.Contains("## Physical Pressure Conversion Matrix"),
                    "The report should summarize physical pressure conversion before broader feel or balance changes.");
                Assert.IsTrue(
                    markdown.Contains("## Combat Decision Signal Matrix"),
                    "The report should connect combat decisions to cue/readout evidence before UI or balance changes.");
                Assert.IsTrue(
                    markdown.Contains("## High-Tier Wait Agency Matrix"),
                    "The report should expose whether high-tier waiting is active choice pressure or passive exposure.");
                Assert.IsTrue(
                    markdown.Contains("## Skill Gate Contract"),
                    "The report should prove raw Skill1 hits are not the same as state-gated follow-up commits.");
                AssertStageWaveBeatMap(results);
                string json = File.ReadAllText(JsonPath);
                Assert.IsTrue(
                    json.Contains("\"summonRosterIdentityAudit\""),
                    "The JSON report should expose the roster identity audit for follow-up batch comparisons.");
                Assert.IsTrue(
                    json.Contains("\"supportStageSlotTimelineMatrix\""),
                    "The JSON report should expose support route stage-slot timeline evidence for follow-up batch comparisons.");
                Assert.IsTrue(
                    json.Contains("\"routeMotivationDominanceMatrix\""),
                    "The JSON report should expose clear-route dominance evidence for follow-up batch comparisons.");
                Assert.IsTrue(
                    json.Contains("\"routeRole\": \"Slot3 retreat recovery\""),
                    "The JSON route dominance evidence should expose the Slot3 retreat/recommit agency branch.");
                Assert.IsTrue(
                    json.Contains("\"highTierWaitAgencyMatrix\""),
                    "The JSON report should expose high-tier wait agency evidence for follow-up batch comparisons.");
                Assert.IsTrue(
                    json.Contains("\"supportDecisionMatrix\""),
                    "The JSON report should expose support decision repeat-band evidence for follow-up batch comparisons.");
                Assert.IsTrue(
                    json.Contains("\"supportPayoffVectorMatrix\""),
                    "The JSON report should expose support payoff vectors for automated damage/prevention comparison.");
                Assert.IsTrue(
                    json.Contains("\"supportBodyCostPhaseMatrix\""),
                    "The JSON report should expose support body-cost phases for automated before/after support comparison.");
                Assert.IsTrue(
                    json.Contains("\"supportWaitExposureMatrix\""),
                    "The JSON report should expose support wait/exposure costs for automated LV2/LV3 route comparisons.");
                Assert.IsTrue(
                    json.Contains("\"supportUpgradeDeltaMatrix\""),
                    "The JSON report should expose marginal support upgrade costs for automated LV2/LV3 route comparisons.");
                Assert.IsTrue(
                    json.Contains("\"supportUpgradeDecisionReadoutMatrix\""),
                    "The JSON report should expose support upgrade readout evidence for automated LV2/LV3 choice comparisons.");
                Assert.IsTrue(
                    json.Contains("\"supportChoiceForecastReadoutBeforeSupport\""),
                    "The JSON report should expose the live route-incentive forecast before support spend.");
                Assert.IsTrue(
                    json.Contains("\"samplePayoff\""),
                    "The JSON route dominance evidence should label the single sample payoff explicitly.");
                Assert.IsTrue(
                    json.Contains("\"repeatBossDamageAverage\""),
                    "The JSON route dominance evidence should expose repeat boss-damage averages for automated comparisons.");
                Assert.IsTrue(
                    json.Contains("\"summonSlotReadinessCooldownMatrix\""),
                    "The JSON report should expose summon readiness/cooldown evidence for follow-up batch comparisons.");
                Assert.IsTrue(
                    json.Contains("\"summonHudReadinessReadoutMatrix\""),
                    "The JSON report should expose HUD/coaster readiness readouts for follow-up batch comparisons.");
                AssertSummonRosterIdentityAudit(results);
                AssertSupportSummonRouteIdentity(forwardRiskSlot2Marksman, forwardRiskSlot3Vanguard);
                AssertSharedManaSupportComboBranch(forwardRiskSlot2Combo, forwardRiskSlot3Blocked);
                AssertSharedManaDelayedMainAnswerBranch(forwardRiskSlot2Delayed, forwardRiskSlot3Delayed);
                AssertSharedManaDelayedCounterRecoveryBranch(
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3DelayedRecovery);
                AssertSummonSlotReadinessCooldownMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3DelayedRecovery);
                AssertSummonHudReadinessReadoutMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3DelayedRecovery);
                AssertEnemyPressureTacticalCost(
                    noSummon,
                    forwardRiskPhysicalSummonNoPunish,
                    ignoredRecovery,
                    blockedRecovery,
                    forwardRiskPhysicalSummonPunish);
                AssertPhysicalPressureConversionRepeatability(repeatabilityResults);
                AssertCombatDecisionSignalMatrix(
                    forwardRiskEnergy,
                    noSummon,
                    forwardRiskPhysicalSummonBlock,
                    forwardRiskPhysicalSummonNoPunish,
                    blockedRecovery,
                    forwardRiskPhysicalSummonPunish);
                AssertCombatDecisionSignalRepeatability(repeatabilityResults);
                AssertHighTierWaitAgencyMatrix(
                    forwardRiskTier3Decision,
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3DelayedRecovery,
                    forwardRiskSlot3RetreatRecovery);
                AssertSupportDecisionTimingVerdicts(
                    forwardRiskTier1Recovery,
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot3DelayedRecovery);
                AssertSupportPayoffVectorMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot3DelayedRecovery);
                AssertSupportBodyCostPhaseMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot3DelayedRecovery);
                AssertSupportWaitExposureMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot3DelayedRecovery);
                AssertSupportUpgradeDeltaMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3DelayedRecovery);
                AssertSupportUpgradeDecisionReadoutMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3DelayedRecovery);
                AssertSupportStageSlotTimelineMatrix(
                    forwardRiskSlot2Combo,
                    forwardRiskSlot2DelayedRecovery,
                    forwardRiskSlot3Blocked,
                    forwardRiskSlot3DelayedRecovery);
                AssertStageResultMotivationMatrix(
                    noSummonSurvival,
                    gunOnlySurvival,
                    forwardRiskPhysicalSummonPunish,
                    forwardRiskTier3Decision,
                    blockedRecovery,
                    forwardRiskSlot2Combo,
                    forwardRiskSlot3Delayed);
                AssertRouteMotivationDominanceMatrix(
                    intended,
                    forwardRiskPhysicalSummonPunish,
                    forwardRiskTier3Decision,
                    blockedRecovery,
                    forwardRiskSlot2Combo,
                    forwardRiskSlot3DelayedRecovery,
                    forwardRiskSlot3RetreatRecovery);
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
                Assert.AreEqual(
                    "pending",
                    ResolveResultHookClass(forwardRiskTier3Decision),
                    "The promoted S1 LV3 probe should stay diagnostic; Slot3 owns the high-tier suppress payoff.");
                Assert.AreEqual(
                    0,
                    forwardRiskTier3Decision.ResultRecords,
                    "The promoted S1 LV3 probe should not fabricate a result hook while the follow-up stays incomplete.");
                Assert.AreEqual(
                    "pending",
                    ResolveResultHookClass(forwardRiskSlot2Combo),
                    "The promoted low-cost Slot2 combo should stay diagnostic until the marksman follow-up actually closes.");
                Assert.AreEqual(
                    0,
                    forwardRiskSlot2Combo.ResultRecords,
                    "Slot2 full-bank should preserve the S1 spend without fabricating the old marksman clear.");
                AssertStageResultHook(
                    forwardRiskSlot3Delayed,
                    "CleanFollowupClear",
                    "Vanguard payoff logged",
                    "LV3 wait cost",
                    "The Slot3 delayed payoff should commit a vanguard-specific support payoff hook.");
                AssertStageResultCopy(
                    forwardRiskSlot3Delayed,
                    "PRESSURE BROKEN",
                    "LV3 wait",
                    "The Slot3 delayed payoff result should name the vanguard hold payoff.");
                Assert.AreEqual(
                    "support_vanguard_clear",
                    ResolveResultHookClass(forwardRiskSlot3Delayed),
                    "The Slot3 delayed payoff result hook class should stay distinguishable from a generic clean route.");
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
                Assert.Greater(
                    backlineEnergy.EnergyProbeElapsedSeconds,
                    0f,
                    "The backline energy probe should record the safe-position measurement window even when LV1 is not reached.");
                Assert.GreaterOrEqual(
                    forwardRiskEnergy.EnergyTier1ReadyAtSeconds,
                    0f,
                    "The forward-risk energy probe should prove LV1 becomes available.");
                Assert.LessOrEqual(
                    forwardRiskEnergy.EnergyTier1DurationSeconds,
                    EnergyProbeMaxSeconds,
                    "Forward-risk positioning should reach LV1 inside the probe window even when backline safety does not.");
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
                Assert.AreEqual(
                    3,
                    backlineEnergyTierLadder.EnergyProbeTargetTier,
                    "The backline tier ladder probe should explicitly wait for LV3.");
                Assert.AreEqual(
                    3,
                    forwardRiskEnergyTierLadder.EnergyProbeTargetTier,
                    "The forward-risk tier ladder probe should explicitly wait for LV3.");
                Assert.Greater(
                    backlineEnergyTierLadder.EnergyProbeElapsedSeconds,
                    0f,
                    "Backline waiting should record the safe-position LV3 probe window even when the tier never opens.");
                Assert.GreaterOrEqual(
                    forwardRiskEnergyTierLadder.EnergyTier2ReadyAtSeconds,
                    0f,
                    "Forward-risk waiting should at least expose the LV2 decision point before judging LV3.");
                Assert.GreaterOrEqual(
                    forwardRiskEnergyTierLadder.EnergyTier1ReadyAtSeconds,
                    0f,
                    "The tier ladder probe should prove forward-risk positioning reaches LV1.");
                Assert.LessOrEqual(
                    forwardRiskEnergyTierLadder.EnergyTier1DurationSeconds,
                    EnergyProbeMaxSeconds,
                    "Forward-risk LV1 readiness should fit inside the tier ladder probe window.");
                Assert.LessOrEqual(
                    forwardRiskEnergyTierLadder.EnergyTier2DurationSeconds,
                    EnergyProbeMaxSeconds,
                    "Forward-risk LV2 readiness should fit inside the tier ladder probe window.");
                Assert.Greater(
                    forwardRiskEnergyTierLadder.AverageEnergyGainMultiplier,
                    backlineEnergyTierLadder.AverageEnergyGainMultiplier + 0.75f,
                    "The tier ladder probe should keep the gain-multiplier split visible even when backline safety never reaches LV3.");
                Assert.Greater(
                    forwardRiskEnergyTierLadder.EnergyProbePlayerDamagePerSecond,
                    backlineEnergyTierLadder.EnergyProbePlayerDamagePerSecond,
                    "Waiting at forward risk should report a higher HP cost rate than backline waiting.");
                Assert.IsTrue(
                    forwardRiskEnergyTierLadder.EnergyTier3ReadyAtSeconds >= 0f
                    || forwardRiskEnergyTierLadder.FirstPlayerDownAtSeconds >= 0f,
                    "The forward-risk tier ladder probe should report either LV3 readiness or player-down while waiting.");
                AssertEnergyDecisionRoute(forwardRiskTier1Decision, 1);
                AssertEnergyDecisionRoute(forwardRiskTier2Decision, 2);
                AssertEnergyDecisionRoute(forwardRiskTier3Decision, 3);
                AssertEnergyRecoveryRoute(forwardRiskTier1Recovery, 1);
                AssertEnergyRecoveryRoute(forwardRiskTier2Recovery, 2);
                AssertEnergyRecoveryRoute(forwardRiskTier3Recovery, 3);
                Assert.Less(
                    forwardRiskTier1Decision.EnergyTier1DurationSeconds,
                    forwardRiskTier2Decision.EnergyTier2DurationSeconds,
                    "The spend-decision route should show that waiting for LV2 costs more time than using LV1.");
                Assert.Less(
                    forwardRiskTier2Decision.EnergyTier2DurationSeconds,
                    forwardRiskTier3Decision.EnergyTier3DurationSeconds,
                    "The spend-decision route should show that waiting for LV3 costs more time than using LV2.");
                Assert.Greater(
                    forwardRiskTier3Decision.LastSummonFollowupWindowDuration,
                    forwardRiskTier1Decision.LastSummonFollowupWindowDuration,
                    "A higher-tier summon spend should produce a larger follow-up state window.");
                Assert.GreaterOrEqual(
                    forwardRiskTier3Decision.SummonFollowupEnergyPulse,
                    forwardRiskTier1Decision.SummonFollowupEnergyPulse,
                    "A higher-tier summon spend should not reduce the follow-up energy pulse.");
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
                    ignoredRecovery.PressureBurdenSeconds,
                    intended.PressureBurdenSeconds,
                    "Ignoring boss-screen pressure should carry a longer unanswered pressure burden than the intended route.");
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
                Assert.Greater(
                    blockedRecovery.SkillProjectileHits,
                    0,
                    "Fresh counter recovery should convert the boss-screen block into a real Skill1 punish.");
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
                    forwardRiskSlot3Delayed.FollowupHitCinematicCueRequests,
                    0,
                    "The Slot3 delayed suppress route should preserve the earned follow-up hit micro-cinematic after the high-cost cut-in.");
                Assert.Greater(
                    forwardRiskSlot3Delayed.FollowupHitCinematicFrameOverlayCount,
                    0,
                    "The Slot3 delayed suppress route should keep the final hit frame overlay instead of letting the LV3 cut-in swallow the hit confirm.");
                Assert.AreEqual(
                    0,
                    forwardRiskSlot3Delayed.FollowupHitSequenceBridgeRequests,
                    "The Slot3 suppress payoff should still stay on the micro-cue director path, not full cinematic sequence playback.");
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
                    blockedFollowup.FollowupMissedScreenCueRequests,
                    0,
                    "A blocked boss-screen follow-up should request the reviewed missed-follow-up screen cue.");
                Assert.Greater(
                    blockedFollowup.FollowupMissedVfxCueRequests,
                    0,
                    "A blocked boss-screen follow-up should request the reviewed missed-follow-up VFX cue.");
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
                LogAssert.ignoreFailingMessages = previousIgnoreFailingMessages;
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
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonAction(player.gameObject, "SummonSlot3");
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
            CombatHitFeedback closeThreatHitFeedback =
                RequireComponent<CombatHitFeedback>(closeThreatRoot, "close threat hit feedback");
            EnemyCombatVfxCueDriver closeThreatVfxCueDriver =
                RequireComponent<EnemyCombatVfxCueDriver>(closeThreatRoot, "close threat VFX cue driver");
            Assert.IsTrue(
                closeThreatHitFeedback.RenderHitFeedback,
                "Close threat should flash its promoted renderers when damaged.");
            Assert.IsTrue(
                closeThreatVfxCueDriver.PlayDamageVfx,
                "Close threat should emit EnemyHit VFX cues when damaged so hits read in combat.");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket review owner");
            Assert.AreSame(
                summonSlot2Action,
                GetObjectReference<PlayerSupportSummonSlotAction>(pocketOwner, "summonSlot2Action"),
                "Pocket owner should serialize SummonSlot2 so manual scene and policy evidence use the same support action source.");
            Assert.AreSame(
                summonSlot3Action,
                GetObjectReference<PlayerSupportSummonSlotAction>(pocketOwner, "summonSlot3Action"),
                "Pocket owner should serialize SummonSlot3 so vanguard assist payoff is not test-only wiring.");
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
            GameObject hudRoot = RequireRoot(HudRootName);
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(
                    hudRoot,
                    "action screen cue presenter");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(
                    hudRoot,
                    "boss barrage review HUD");
            BossBarrageLaneReviewOverlayHud reviewOverlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(
                    hudRoot,
                    "boss barrage review overlay HUD");

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
                summonSlot2Action,
                summonSlot3Action,
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
                reviewHud,
                reviewOverlayHud,
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
                case PolicyKind.BacklineEnergyTierLadderProbe:
                    yield return RunEnergyRiskProbe(
                        context,
                        BacklineEnergyProbeForwardRisk01,
                        targetTier: 3,
                        maxSeconds: EnergyTierLadderProbeMaxSeconds);
                    break;
                case PolicyKind.ForwardRiskEnergyTierLadderProbe:
                    yield return RunEnergyRiskProbe(
                        context,
                        ForwardEnergyProbeForwardRisk01,
                        targetTier: 3,
                        maxSeconds: EnergyTierLadderProbeMaxSeconds);
                    break;
                case PolicyKind.ForwardRiskTier1DecisionRoute:
                    yield return RunForwardRiskTierDecisionRoute(context, 1, false);
                    break;
                case PolicyKind.ForwardRiskTier2DecisionRoute:
                    yield return RunForwardRiskTierDecisionRoute(context, 2, false);
                    break;
                case PolicyKind.ForwardRiskTier3DecisionRoute:
                    yield return RunForwardRiskTierDecisionRoute(context, 3, false);
                    break;
                case PolicyKind.ForwardRiskTier1RecoveryRoute:
                    yield return RunForwardRiskTierDecisionRoute(context, 1, true);
                    break;
                case PolicyKind.ForwardRiskTier2RecoveryRoute:
                    yield return RunForwardRiskTierDecisionRoute(context, 2, true);
                    break;
                case PolicyKind.ForwardRiskTier3RecoveryRoute:
                    yield return RunForwardRiskTierDecisionRoute(context, 3, true);
                    break;
                case PolicyKind.ForwardRiskSlot2MarksmanRoute:
                    yield return RunForwardRiskSupportSummonRoute(context, context.SummonSlot2Action, 2);
                    break;
                case PolicyKind.ForwardRiskSlot3VanguardRoute:
                    yield return RunForwardRiskSupportSummonRoute(context, context.SummonSlot3Action, 3);
                    break;
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    yield return RunForwardRiskSupportThenSlot1ComboRoute(context, context.SummonSlot2Action);
                    break;
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    yield return RunForwardRiskSupportThenSlot1ComboRoute(context, context.SummonSlot3Action);
                    break;
                case PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route:
                    yield return RunForwardRiskSupportThenDelayedSlot1Route(context, context.SummonSlot2Action, 2);
                    break;
                case PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route:
                    yield return RunForwardRiskSupportThenDelayedSlot1Route(context, context.SummonSlot3Action, 3);
                    break;
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    yield return RunForwardRiskSupportThenDelayedSlot1Route(
                        context,
                        context.SummonSlot2Action,
                        2,
                        true);
                    break;
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    yield return RunForwardRiskSupportThenDelayedSlot1Route(
                        context,
                        context.SummonSlot3Action,
                        3,
                        true);
                    break;
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    yield return RunForwardRiskSupportThenDelayedSlot1Route(
                        context,
                        context.SummonSlot3Action,
                        3,
                        true,
                        retreatAfterAvailableTier: 2,
                        retreatForwardRisk01: 0f,
                        recommitForwardRiskBeforeSlot1: ForwardEnergyProbeForwardRisk01,
                        forceCounterRecoveryAfterRecommit: true);
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
            float forwardRisk01,
            int targetTier = 1,
            float maxSeconds = EnergyProbeMaxSeconds)
        {
            MovePlayerToForwardRisk(context, forwardRisk01);
            context.EnergyLadder.ResetLadder();
            context.Metrics.EnergyProbeTargetForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            context.Metrics.EnergyProbeTargetTier = Mathf.Clamp(targetTier, 1, 3);
            context.Metrics.EnergyProbeStartAtSeconds = context.Metrics.ElapsedSeconds;
            context.Sample();

            float start = context.Metrics.ElapsedSeconds;
            while (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier
                && context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - start < maxSeconds)
            {
                yield return Advance(context, 0.1f);
            }

            if (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier)
            {
                context.Metrics.Notes.Add($"energy probe did not reach LV{context.Metrics.EnergyProbeTargetTier}");
            }
            else
            {
                yield return null;
                context.PocketOwner.Tick(0f);
                context.Sample();
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

        private static IEnumerator RunForwardRiskTierDecisionRoute(
            CombatPolicyContext context,
            int targetTier,
            bool recoverAfterBossScreenBlock)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);

            if (context.CloseThreatHealth.IsAlive)
            {
                yield break;
            }

            yield return WaitForCloseThreatReliefEnd(context, 3.5f);
            MovePlayerToForwardRisk(context, ForwardEnergyProbeForwardRisk01);
            context.EnergyLadder.ResetLadder();
            context.Metrics.EnergyProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.EnergyProbeTargetTier = Mathf.Clamp(targetTier, 1, 3);
            context.Metrics.EnergyProbeStartAtSeconds = context.Metrics.ElapsedSeconds;
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Sample();

            context.BossEmitter.SetFiringEnabled(true);
            float start = context.Metrics.ElapsedSeconds;
            while (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier
                && context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - start < EnergyTierLadderProbeMaxSeconds)
            {
                yield return Advance(context, 0.1f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            if (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier)
            {
                context.Metrics.Notes.Add($"tier decision route did not reach LV{context.Metrics.EnergyProbeTargetTier}");
                yield break;
            }

            if (!context.PlayerHealth.IsAlive)
            {
                yield break;
            }

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"tier decision summon blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return WaitForActiveAllyPressureScreen(context, "physical summon block");
            DeactivateActiveBossProjectiles();
            context.BossEmitter.SetFiringEnabled(false);
            context.BossEmitter.SetFiringEnabled(true);
            if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
            {
                context.Metrics.Notes.Add("tier decision priority barrage unavailable");
            }

            yield return ApplyPhysicalBossBarrageAndPunish(context, PhysicalBarrageProbeFlightSeconds);
            if (!recoverAfterBossScreenBlock)
            {
                yield break;
            }

            if (targetTier >= 3 && context.Metrics.BossScreenSuppressedByFollowup)
            {
                yield break;
            }

            if (!context.PocketOwner.IsCounterWaveCompletionRecorded)
            {
                context.Metrics.Notes.Add("tier recovery requested without counter wave");
                yield break;
            }

            yield return AnswerCounterWaveWithFreshSummon(context);
            yield return WaitForCounterFinalWindow(context, 3f);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator RunForwardRiskSupportSummonRoute(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction supportAction,
            int targetTier)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);

            if (context.CloseThreatHealth.IsAlive)
            {
                yield break;
            }

            yield return WaitForCloseThreatReliefEnd(context, 3.5f);
            MovePlayerToForwardRisk(context, ForwardEnergyProbeForwardRisk01);
            context.EnergyLadder.ResetLadder();
            context.Metrics.EnergyProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.EnergyProbeTargetTier = Mathf.Clamp(targetTier, 1, 3);
            context.Metrics.EnergyProbeStartAtSeconds = context.Metrics.ElapsedSeconds;
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.SupportSummonSlotId = supportAction.SlotActionName;
            context.Metrics.SupportSummonRequiredMana = supportAction.RequiredSummonMana;
            context.Sample();

            context.BossEmitter.SetFiringEnabled(true);
            float start = context.Metrics.ElapsedSeconds;
            while (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier
                && context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - start < EnergyTierLadderProbeMaxSeconds)
            {
                yield return Advance(context, 0.1f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            if (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier)
            {
                context.Metrics.Notes.Add(
                    $"{supportAction.SlotActionName} route did not reach LV{context.Metrics.EnergyProbeTargetTier}");
                yield break;
            }

            if (!context.PlayerHealth.IsAlive)
            {
                yield break;
            }

            RecordSupportBodyCostBeforeSupport(context);
            RecordSupportChoiceForecastBeforeSupport(context);
            if (!supportAction.TryUseSummon())
            {
                context.Metrics.Notes.Add(
                    $"{supportAction.SlotActionName} summon blocked: {supportAction.LastUseBlockedReason}");
                yield break;
            }

            RecordSupportSummonUse(context, supportAction);
            context.PocketOwner.Tick(0f);
            context.Sample();

            float actorWaitStart = context.Metrics.ElapsedSeconds;
            while (supportAction.ActiveSummonActorCount <= 0
                && context.Metrics.ElapsedSeconds - actorWaitStart < 1f)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (supportAction.ActiveSummonActorCount <= 0)
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} did not spawn a support actor");
                yield break;
            }

            if (supportAction.MinimumSummonTier >= 3)
            {
                yield return WaitForSupportPayoffReadiness(context, supportAction, $"{supportAction.SlotActionName} route");
            }
            else
            {
                yield return Advance(context, 0.35f);
            }

            RecordSupportSummonSnapshot(context, supportAction);
            DeactivateActiveBossProjectiles();
            context.BossEmitter.SetFiringEnabled(false);
            context.BossEmitter.SetFiringEnabled(true);
            if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} priority barrage unavailable");
            }

            yield return ApplyPhysicalBossBarrage(context, PhysicalBarrageProbeFlightSeconds);
            yield return Advance(context, 1.25f);
            RecordSupportSummonSnapshot(context, supportAction);
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator RunForwardRiskSupportThenSlot1ComboRoute(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction supportAction)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);

            if (context.CloseThreatHealth.IsAlive)
            {
                yield break;
            }

            yield return WaitForCloseThreatReliefEnd(context, 3.5f);
            MovePlayerToForwardRisk(context, ForwardEnergyProbeForwardRisk01);
            context.EnergyLadder.ResetLadder();
            context.Metrics.EnergyProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.EnergyProbeTargetTier = 3;
            context.Metrics.EnergyProbeStartAtSeconds = context.Metrics.ElapsedSeconds;
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.SupportSummonSlotId = supportAction.SlotActionName;
            context.Metrics.SupportSummonRequiredMana = supportAction.RequiredSummonMana;
            context.Metrics.SupportComboSlot1RequiredMana = context.SummonSlot1Action.RequiredSummonMana;
            context.Sample();

            context.BossEmitter.SetFiringEnabled(true);
            float start = context.Metrics.ElapsedSeconds;
            while (context.EnergyLadder.AvailableTier < 3
                && context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - start < EnergyTierLadderProbeMaxSeconds)
            {
                yield return Advance(context, 0.1f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            if (context.EnergyLadder.AvailableTier < 3)
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} combo route did not reach LV3");
                yield break;
            }

            if (!context.PlayerHealth.IsAlive)
            {
                yield break;
            }

            RecordSupportBodyCostBeforeSupport(context);
            RecordSupportChoiceForecastBeforeSupport(context);
            if (!supportAction.TryUseSummon())
            {
                context.Metrics.Notes.Add(
                    $"{supportAction.SlotActionName} combo summon blocked: {supportAction.LastUseBlockedReason}");
                yield break;
            }

            RecordSupportSummonUse(context, supportAction);
            context.Metrics.SupportComboManaAfterSupport = context.EnergyLadder.CurrentMana;
            context.Metrics.SupportComboSupportCooldownAfterSupport = supportAction.SlotCooldownRemaining;
            context.PocketOwner.Tick(0f);
            context.Sample();

            if (supportAction.SlotActionName == "SummonSlot2")
            {
                float supportActorWaitStart = context.Metrics.ElapsedSeconds;
                while (supportAction.ActiveSummonActorCount <= 0
                    && context.Metrics.ElapsedSeconds - supportActorWaitStart < 1f)
                {
                    yield return Advance(context, 0.05f);
                    context.PocketOwner.Tick(0f);
                    context.Sample();
                }

                if (supportAction.ActiveSummonActorCount <= 0)
                {
                    context.Metrics.Notes.Add($"{supportAction.SlotActionName} combo did not spawn a support actor");
                    yield break;
                }

                yield return WaitForSupportPayoffReadiness(
                    context,
                    supportAction,
                    $"{supportAction.SlotActionName} combo");
                RecordSupportSummonSnapshot(context, supportAction);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            context.Metrics.SupportComboManaBeforeSlot1 = context.EnergyLadder.CurrentMana;
            context.Metrics.SupportComboSupportCooldownBeforeSlot1 = supportAction.SlotCooldownRemaining;
            context.Metrics.SupportComboSlot1CooldownBeforeAttempt =
                context.SummonSlot1Action.SlotCooldownRemaining;
            RecordSupportComboHudBeforeSlot1(context, supportAction);
            RecordSupportBodyCostBeforeMainAnswer(context);
            context.Metrics.SupportComboSlot1Attempted = true;
            int slot1ScreensBefore = context.SummonSlot1Action.ActivePressureScreenCount;
            bool slot1Used = context.SummonSlot1Action.TryUseSummonSlot1();
            context.Metrics.SupportComboSlot1Used = slot1Used;
            context.Metrics.SupportComboSlot1BlockedReason =
                slot1Used ? string.Empty : context.SummonSlot1Action.LastUseBlockedReason ?? string.Empty;
            context.Metrics.SupportComboManaAfterSlot1 = context.EnergyLadder.CurrentMana;
            context.Metrics.SupportComboSlot1CooldownAfterAttempt =
                context.SummonSlot1Action.SlotCooldownRemaining;

            if (slot1Used)
            {
                RecordSummonUse(context, false);
                context.PocketOwner.Tick(0f);
                context.Sample();
                yield return WaitForNewSlot1PressureScreen(
                    context,
                    slot1ScreensBefore,
                    $"{supportAction.SlotActionName} combo Slot1");
                RecordSupportSummonSnapshot(context, supportAction);
                DeactivateActiveBossProjectiles();
                context.BossEmitter.SetFiringEnabled(false);
                context.BossEmitter.SetFiringEnabled(true);
                if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
                {
                    context.Metrics.Notes.Add($"{supportAction.SlotActionName} combo priority barrage unavailable");
                }

                int followupWindowsBeforeBarrage = context.Metrics.FollowupWindowOpenCount;
                yield return ApplyPhysicalBossBarrageAndPunish(
                    context,
                    PhysicalBarrageProbeFlightSeconds,
                    followupWindowsBeforeBarrage,
                    $"{supportAction.SlotActionName} combo Slot1");
                yield break;
            }

            context.PocketOwner.Tick(0f);
            context.Sample();

            float actorWaitStart = context.Metrics.ElapsedSeconds;
            while (supportAction.ActiveSummonActorCount <= 0
                && context.Metrics.ElapsedSeconds - actorWaitStart < 1f)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (supportAction.ActiveSummonActorCount <= 0)
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} combo did not spawn a support actor");
                yield break;
            }

            if (supportAction.MinimumSummonTier >= 3)
            {
                yield return WaitForSupportPayoffReadiness(context, supportAction, $"{supportAction.SlotActionName} combo");
            }
            else
            {
                yield return Advance(context, 0.35f);
            }

            RecordSupportSummonSnapshot(context, supportAction);
            DeactivateActiveBossProjectiles();
            context.BossEmitter.SetFiringEnabled(false);
            context.BossEmitter.SetFiringEnabled(true);
            if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} blocked-combo barrage unavailable");
            }

            yield return ApplyPhysicalBossBarrage(context, PhysicalBarrageProbeFlightSeconds);
            yield return Advance(context, 1.25f);
            RecordSupportSummonSnapshot(context, supportAction);
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator RunForwardRiskSupportThenDelayedSlot1Route(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction supportAction,
            int targetTier,
            bool continueCounterRecovery = false,
            int retreatAfterAvailableTier = 0,
            float retreatForwardRisk01 = ForwardEnergyProbeForwardRisk01,
            float recommitForwardRiskBeforeSlot1 = -1f,
            bool forceCounterRecoveryAfterRecommit = false)
        {
            BossBarragePatternProfile physicalPattern = context.BossEmitter.CurrentPattern;
            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            DeactivateActivePlayerProjectiles();
            RecordCloseProbeSelectorSnapshot(context);
            yield return FireCloseProbePhysicalShots(context);

            if (context.CloseThreatHealth.IsAlive)
            {
                yield break;
            }

            yield return WaitForCloseThreatReliefEnd(context, 3.5f);
            MovePlayerToForwardRisk(context, ForwardEnergyProbeForwardRisk01);
            context.EnergyLadder.ResetLadder();
            context.Metrics.EnergyProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.EnergyProbeTargetTier = Mathf.Clamp(targetTier, 1, 3);
            context.Metrics.EnergyProbeStartAtSeconds = context.Metrics.ElapsedSeconds;
            context.Metrics.PhysicalBarrageProbeTargetForwardRisk01 = ForwardEnergyProbeForwardRisk01;
            context.Metrics.SupportSummonSlotId = supportAction.SlotActionName;
            context.Metrics.SupportSummonRequiredMana = supportAction.RequiredSummonMana;
            context.Metrics.SupportComboSlot1RequiredMana = context.SummonSlot1Action.RequiredSummonMana;
            context.Sample();

            context.BossEmitter.SetFiringEnabled(true);
            float chargeStart = context.Metrics.ElapsedSeconds;
            bool retreatedDuringCharge = false;
            while (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier
                && context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - chargeStart < EnergyTierLadderProbeMaxSeconds)
            {
                if (!retreatedDuringCharge
                    && retreatAfterAvailableTier > 0
                    && context.EnergyLadder.AvailableTier >= retreatAfterAvailableTier)
                {
                    MovePlayerToForwardRisk(context, retreatForwardRisk01);
                    retreatedDuringCharge = true;
                    context.Sample();
                }

                yield return Advance(context, 0.1f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            if (context.EnergyLadder.AvailableTier < context.Metrics.EnergyProbeTargetTier)
            {
                context.Metrics.Notes.Add(
                    $"{supportAction.SlotActionName} delayed combo did not reach LV{context.Metrics.EnergyProbeTargetTier}");
                yield break;
            }

            if (!context.PlayerHealth.IsAlive)
            {
                yield break;
            }

            RecordSupportBodyCostBeforeSupport(context);
            RecordSupportChoiceForecastBeforeSupport(context);
            if (!supportAction.TryUseSummon())
            {
                context.Metrics.Notes.Add(
                    $"{supportAction.SlotActionName} delayed combo summon blocked: {supportAction.LastUseBlockedReason}");
                yield break;
            }

            RecordSupportSummonUse(context, supportAction);
            context.Metrics.SupportComboManaAfterSupport = context.EnergyLadder.CurrentMana;
            context.Metrics.SupportComboSupportCooldownAfterSupport = supportAction.SlotCooldownRemaining;
            context.PocketOwner.Tick(0f);
            context.Sample();

            float actorWaitStart = context.Metrics.ElapsedSeconds;
            while (supportAction.ActiveSummonActorCount <= 0
                && context.Metrics.ElapsedSeconds - actorWaitStart < 1f)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (supportAction.ActiveSummonActorCount <= 0)
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} delayed combo did not spawn a support actor");
                yield break;
            }

            if (supportAction.MinimumSummonTier >= 3)
            {
                yield return WaitForSupportPayoffReadiness(context, supportAction, $"{supportAction.SlotActionName} delayed combo");
            }
            else
            {
                yield return Advance(context, 0.65f);
            }

            RecordSupportSummonSnapshot(context, supportAction);
            context.PocketOwner.Tick(0f);
            context.Sample();

            float slot1WaitStart = context.Metrics.ElapsedSeconds;
            context.BossEmitter.SetFiringEnabled(true);
            while (!context.EnergyLadder.CanSpendMana(context.SummonSlot1Action.RequiredSummonMana)
                && context.PlayerHealth.IsAlive
                && context.Metrics.ElapsedSeconds - slot1WaitStart < EnergyTierLadderProbeMaxSeconds)
            {
                yield return Advance(context, 0.1f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            context.BossEmitter.SetFiringEnabled(false);
            DeactivateActiveBossProjectiles();
            context.Metrics.SupportComboSlot1ReadyDelaySeconds = context.Metrics.ElapsedSeconds - slot1WaitStart;
            if (recommitForwardRiskBeforeSlot1 >= 0f)
            {
                MovePlayerToForwardRisk(context, recommitForwardRiskBeforeSlot1);
                context.Sample();
            }

            context.Metrics.SupportComboManaBeforeSlot1 = context.EnergyLadder.CurrentMana;
            context.Metrics.SupportComboSupportCooldownBeforeSlot1 = supportAction.SlotCooldownRemaining;
            context.Metrics.SupportComboSlot1CooldownBeforeAttempt =
                context.SummonSlot1Action.SlotCooldownRemaining;
            context.Metrics.SupportComboPlayerDamageBeforeSlot1 = context.Metrics.PlayerDamageTaken;
            RecordSupportComboHudBeforeSlot1(context, supportAction);
            RecordSupportBodyCostBeforeMainAnswer(context);
            if (!context.EnergyLadder.CanSpendMana(context.SummonSlot1Action.RequiredSummonMana))
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} delayed combo did not recover Slot1 mana");
                yield break;
            }

            if (!context.PlayerHealth.IsAlive)
            {
                yield break;
            }

            context.Metrics.SupportComboSlot1Attempted = true;
            int slot1ScreensBefore = context.SummonSlot1Action.ActivePressureScreenCount;
            bool slot1Used = context.SummonSlot1Action.TryUseSummonSlot1();
            context.Metrics.SupportComboSlot1Used = slot1Used;
            context.Metrics.SupportComboSlot1BlockedReason =
                slot1Used ? string.Empty : context.SummonSlot1Action.LastUseBlockedReason ?? string.Empty;
            context.Metrics.SupportComboManaAfterSlot1 = context.EnergyLadder.CurrentMana;
            context.Metrics.SupportComboSlot1CooldownAfterAttempt =
                context.SummonSlot1Action.SlotCooldownRemaining;
            if (!slot1Used)
            {
                context.PocketOwner.Tick(0f);
                context.Sample();
                yield break;
            }

            RecordSummonUse(context, false);
            if (forceCounterRecoveryAfterRecommit)
            {
                context.PocketOwner.CancelVanguardAssistSuppressWindow();
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return WaitForNewSlot1PressureScreen(
                context,
                slot1ScreensBefore,
                $"{supportAction.SlotActionName} delayed combo Slot1");
            RecordSupportSummonSnapshot(context, supportAction);
            DeactivateActiveBossProjectiles();
            context.BossEmitter.SetFiringEnabled(false);
            context.BossEmitter.SetFiringEnabled(true);
            if (!context.BossEmitter.QueuePriorityPattern(physicalPattern, 1))
            {
                context.Metrics.Notes.Add($"{supportAction.SlotActionName} delayed combo priority barrage unavailable");
            }

            int followupWindowsBeforeBarrage = context.Metrics.FollowupWindowOpenCount;
            if (forceCounterRecoveryAfterRecommit)
            {
                yield return ApplyPhysicalBossBarrageAndForceCounterRecovery(
                    context,
                    PhysicalBarrageProbeFlightSeconds,
                    $"{supportAction.SlotActionName} retreat combo Slot1");
            }
            else
            {
                yield return ApplyPhysicalBossBarrageAndPunish(
                    context,
                    PhysicalBarrageProbeFlightSeconds,
                    followupWindowsBeforeBarrage,
                    $"{supportAction.SlotActionName} delayed combo Slot1",
                    ensureEnemyPressureScreenBeforeSkill1: supportAction.MinimumSummonTier >= 3,
                    acceptExistingFollowupWindow: IsFireDragonSupportRole(supportAction));
            }
            if (!continueCounterRecovery)
            {
                yield break;
            }

            if (context.PocketOwner.IsCounterWaveCompletionRecorded || context.Metrics.CounterWaves > 0)
            {
                yield return AnswerCounterWaveWithFreshSummon(context);
                yield return WaitForCounterFinalWindow(context, 3f);
                yield return ConfirmSkill1Followup(context);
            }

            yield return Advance(context, 1.0f);
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
            GrantEnergyForSlot1(context);
            context.Sample();

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"physical summon blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return WaitForActiveAllyPressureScreen(context, "physical summon punish");
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
            GrantEnergyForSlot1(context);
            context.Sample();

            if (!context.SummonSlot1Action.TryUseSummonSlot1())
            {
                context.Metrics.Notes.Add($"physical summon punish blocked: {context.SummonSlot1Action.LastUseBlockedReason}");
                yield break;
            }

            RecordSummonUse(context, false);
            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return WaitForActiveAllyPressureScreen(context, "tier decision");
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
            yield return ChargeEnergyForSlot1(context, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ConfirmSkill1Followup(context);
            yield return Advance(context, 1.0f);
        }

        private static IEnumerator RunIntendedDelayedFollowup(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyForSlot1(context, 14f);
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
                yield return ChargeEnergyForSlot1(context, 14f);
                yield return UseSummonAndBlockNextBossWave(context);
                yield return ConfirmSkill1Followup(context);
                yield return Advance(context, 1.0f);
            }
        }

        private static IEnumerator RunMissedFollowupCounterRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyForSlot1(context, 14f);
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
            yield return ChargeEnergyForSlot1(context, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ReleaseBossScreenAndBlockSkill1Followup(context);
            yield return WaitForCounterFinalWindow(context, 2f, expectWindowBeforeSkill1: false);
            yield return Advance(context, 0.25f);
        }

        private static IEnumerator RunBossScreenIgnoredNoRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyForSlot1(context, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
            yield return ReleaseBossScreenAndBlockSkill1Followup(context);
            yield return WaitForCounterFinalWindow(context, 2f, expectWindowBeforeSkill1: false);
            yield return Advance(context, 8f);
        }

        private static IEnumerator RunBossScreenBlockCounterRecovery(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyForSlot1(context, 14f);
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
            yield return ChargeEnergyForSlot1(context, 14f);
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

        private static IEnumerator ChargeEnergyForSlot1(
            CombatPolicyContext context,
            float maxSeconds)
        {
            float requiredMana = context.SummonSlot1Action.RequiredSummonMana;
            float start = context.Metrics.ElapsedSeconds;
            while (!context.EnergyLadder.CanSpendMana(requiredMana)
                && context.Metrics.ElapsedSeconds - start < maxSeconds)
            {
                yield return Advance(context, 0.1f);
            }

            if (!context.EnergyLadder.CanSpendMana(requiredMana))
            {
                context.Metrics.Notes.Add($"slot1 energy {requiredMana:0} EN not ready");
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

        private static void GrantEnergyForSlot1(CombatPolicyContext context)
        {
            float requiredMana = context.SummonSlot1Action.RequiredSummonMana;
            int guard = 0;
            while (!context.EnergyLadder.CanSpendMana(requiredMana) && guard++ < 4)
            {
                context.EnergyLadder.GrantCurrentTierEnergy(
                    Mathf.Max(1f, requiredMana - context.EnergyLadder.CurrentMana + 1f));
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

        private static IEnumerator ApplyPhysicalBossBarrageAndForceCounterRecovery(
            CombatPolicyContext context,
            float flightSeconds,
            string notePrefix)
        {
            BossBarragePatternProfile pattern = context.BossEmitter.CurrentPattern;
            float healthBefore = context.PlayerHealth.CurrentHealth;
            if (!context.BossEmitter.BeginWindup())
            {
                context.Metrics.Notes.Add($"{notePrefix} counter barrage windup unavailable");
                yield break;
            }

            context.Metrics.PhysicalBarragePatternId = pattern != null ? pattern.PatternId : "unknown";
            context.Metrics.PhysicalBarragePendingForwardRisk01 = context.BossEmitter.PendingForwardRisk01;
            int spawned = context.BossEmitter.FirePendingWave();
            context.Metrics.BossWaves++;
            context.Metrics.BossProjectilesSpawned += spawned;
            context.Metrics.PhysicalBarrageWaves++;
            context.Metrics.PhysicalBarrageProjectilesSpawned += spawned;

            BossBarrageProjectile[] bossProjectiles = FindActiveBossProjectiles(
                pattern != null ? pattern.ProjectileMaterial : null);
            context.Metrics.PhysicalBarrageTrackedProjectileCount += bossProjectiles.Length;
            SummonPressureScreen allyScreen = FindActiveAllyPressureScreen();
            if (allyScreen == null)
            {
                context.Metrics.Notes.Add($"{notePrefix} ally pressure screen missing for forced counter branch");
            }
            else if (bossProjectiles.Length == 0)
            {
                context.Metrics.Notes.Add($"{notePrefix} boss projectile missing for forced counter branch");
            }
            else if (!allyScreen.TryIntercept(bossProjectiles[0]))
            {
                context.Metrics.Notes.Add($"{notePrefix} ally pressure screen failed to intercept boss projectile");
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            if (context.PocketOwner.IsSummonFollowupWindowActive)
            {
                yield return ReleaseBossScreenAndBlockSkill1Followup(
                    context,
                    allowDirectBossHitOnScreenMiss: false);
            }
            else
            {
                context.Metrics.Notes.Add($"{notePrefix} forced block did not open follow-up window");
            }

            yield return Advance(context, flightSeconds);
            Physics.SyncTransforms();
            RecordPhysicalBossBarrageResults(context, bossProjectiles, healthBefore);
            DeactivateActiveBossProjectiles();
            context.PocketOwner.Tick(0f);
            context.Sample();
        }

        private static IEnumerator ApplyPhysicalBossBarrageAndPunish(
            CombatPolicyContext context,
            float flightSeconds,
            int requiredFollowupWindowCountBeforeBarrage = -1,
            string notePrefix = "physical summon punish",
            bool ensureEnemyPressureScreenBeforeSkill1 = false,
            bool acceptExistingFollowupWindow = false)
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
            while (!HasRequiredFollowupWindow(
                    context,
                    requiredFollowupWindowCountBeforeBarrage,
                    acceptExistingFollowupWindow)
                && context.Metrics.ElapsedSeconds - start < flightSeconds)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (!HasRequiredFollowupWindow(
                    context,
                    requiredFollowupWindowCountBeforeBarrage,
                    acceptExistingFollowupWindow))
            {
                context.Metrics.Notes.Add($"{notePrefix} follow-up window did not open before Skill1");
            }
            else
            {
                if (ensureEnemyPressureScreenBeforeSkill1)
                {
                    yield return EnsureActiveEnemyPressureScreenBeforeSkill1(context, notePrefix);
                }

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

        private static bool HasRequiredFollowupWindow(
            CombatPolicyContext context,
            int requiredFollowupWindowCountBeforeBarrage,
            bool acceptExistingFollowupWindow = false)
        {
            if (!context.PocketOwner.IsSummonFollowupWindowActive)
            {
                return false;
            }

            if (acceptExistingFollowupWindow)
            {
                return true;
            }

            return requiredFollowupWindowCountBeforeBarrage < 0
                || context.Metrics.FollowupWindowOpenCount > requiredFollowupWindowCountBeforeBarrage;
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
            context.PocketOwner.Tick(0f);
            context.Sample();
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
                context.Metrics.Notes.Add(ResolvePhysicalSkill1NoHitNote(context));
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            float clearDelay = GetFloat(context.PocketOwner, "skill1FollowupClearDelaySeconds") + 0.05f;
            context.PocketOwner.Tick(clearDelay);
            context.Metrics.ElapsedSeconds += clearDelay;
            context.Sample();
            yield return null;
        }

        private static IEnumerator EnsureActiveEnemyPressureScreenBeforeSkill1(
            CombatPolicyContext context,
            string notePrefix)
        {
            if (FindActiveEnemyPressureScreen() != null)
            {
                yield break;
            }

            if (!context.BossSummonPressureAction.TryReleasePressureSummon(1))
            {
                context.Metrics.Notes.Add($"{notePrefix} boss screen unavailable before Skill1");
                yield break;
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            float start = context.Metrics.ElapsedSeconds;
            while (FindActiveEnemyPressureScreen() == null
                && context.Metrics.ElapsedSeconds - start < 0.6f)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (FindActiveEnemyPressureScreen() == null)
            {
                context.Metrics.Notes.Add($"{notePrefix} boss screen did not become active before Skill1");
            }
        }

        private static string ResolvePhysicalSkill1NoHitNote(CombatPolicyContext context)
        {
            if (context.Metrics.BossScreenSuppressedByFollowup)
            {
                return "physical Skill1 suppressed boss screen instead of direct boss hit";
            }

            if (context.Metrics.BossBlockedSkill1Followup
                || context.Metrics.SkillProjectilesBlockedByBossScreen > 0)
            {
                return "physical Skill1 intercepted by boss screen; counter answer required";
            }

            if (context.Metrics.CounterWaves > 0 || context.PocketOwner.IsCounterWaveCompletionRecorded)
            {
                return "physical Skill1 miss transferred to counter answer";
            }

            return "physical skill1 did not hit boss";
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
            if (!context.EnergyLadder.CanSpendMana(context.SummonSlot1Action.RequiredSummonMana))
            {
                yield return ChargeEnergyForSlot1(context, 8f);
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
            if (!context.EnergyLadder.CanSpendMana(context.SummonSlot1Action.RequiredSummonMana))
            {
                yield return ChargeEnergyForSlot1(context, 8f);
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
            if (!context.EnergyLadder.CanSpendMana(context.SummonSlot1Action.RequiredSummonMana))
            {
                yield return ChargeEnergyForSlot1(context, 8f);
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

        private static IEnumerator ReleaseBossScreenAndBlockSkill1Followup(
            CombatPolicyContext context,
            bool allowDirectBossHitOnScreenMiss = true)
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
                else if (allowDirectBossHitOnScreenMiss
                    && projectiles[i].TryApplyImpact(context.BossCollider, projectiles[i].transform.position))
                {
                    context.Metrics.SkillProjectileHits++;
                }
                else if (!allowDirectBossHitOnScreenMiss)
                {
                    projectiles[i].Deactivate();
                }
            }

            context.PocketOwner.Tick(0f);
            context.Sample();
            yield return null;
        }

        private static void RecordSummonUse(CombatPolicyContext context, bool isCounterAnswer)
        {
            context.Metrics.SummonUses++;
            context.Metrics.HighestSummonSpentTier = Mathf.Max(
                context.Metrics.HighestSummonSpentTier,
                context.SummonSlot1Action.LastSpentTier);
            if (context.Metrics.FirstSummonUseAtSeconds < 0f)
            {
                context.Metrics.FirstSummonUseAtSeconds = context.Metrics.ElapsedSeconds;
            }

            if (isCounterAnswer && context.Metrics.FirstCounterAnswerSummonAtSeconds < 0f)
            {
                context.Metrics.FirstCounterAnswerSummonAtSeconds = context.Metrics.ElapsedSeconds;
            }
        }

        private static void RecordSupportSummonUse(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction action)
        {
            context.Metrics.SummonUses++;
            context.Metrics.HighestSummonSpentTier = Mathf.Max(
                context.Metrics.HighestSummonSpentTier,
                action.LastSpentTier);
            if (context.Metrics.FirstSummonUseAtSeconds < 0f)
            {
                context.Metrics.FirstSummonUseAtSeconds = context.Metrics.ElapsedSeconds;
            }

            RecordSupportSummonSnapshot(context, action);
        }

        private static void RecordSupportSummonSnapshot(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction action)
        {
            if (action == null)
            {
                return;
            }

            context.Metrics.SupportSummonSlotId = action.SlotActionName;
            context.Metrics.SupportSummonRequiredMana = action.RequiredSummonMana;
            context.Metrics.SupportSummonCooldownSeconds = action.SlotCooldownSeconds;
            context.Metrics.SupportSummonSpentTier = Mathf.Max(
                context.Metrics.SupportSummonSpentTier,
                action.LastSpentTier);
            context.Metrics.SupportSummonActorRoleId = action.LastSummonActorRoleId ?? string.Empty;
            context.Metrics.SupportSummonVolleyWaves = Mathf.Max(
                context.Metrics.SupportSummonVolleyWaves,
                action.TotalVolleyWaveCount);
            context.Metrics.SupportSummonBlocks = Mathf.Max(
                context.Metrics.SupportSummonBlocks,
                action.TotalPressureScreenInterceptCount);
            context.Metrics.SupportSummonMaxActiveActors = Mathf.Max(
                context.Metrics.SupportSummonMaxActiveActors,
                action.ActiveSummonActorCount);
            context.Metrics.SupportSummonActorHealthRatio = action.LastSummonActorHealthRatio;
        }

        private static void RecordSupportBodyCostBeforeSupport(CombatPolicyContext context)
        {
            context.Metrics.SupportBodyHitsBeforeSupport = context.Metrics.EnemyFrontlineBodyHits;
            context.Metrics.SupportDamageBeforeSupport = context.Metrics.PlayerDamageTaken;
            RecordSupportBodyCostFinal(context);
        }

        private static void RecordSupportChoiceForecastBeforeSupport(CombatPolicyContext context)
        {
            context.Metrics.SupportChoiceForecastReadoutBeforeSupport =
                context.PocketOwner != null ? context.PocketOwner.RouteIncentiveCue : string.Empty;
        }

        private static void RecordSupportBodyCostBeforeMainAnswer(CombatPolicyContext context)
        {
            context.Metrics.SupportBodyHitsBeforeMainAnswer = context.Metrics.EnemyFrontlineBodyHits;
            context.Metrics.SupportDamageBeforeMainAnswer = context.Metrics.PlayerDamageTaken;
            RecordSupportBodyCostFinal(context);
        }

        private static void RecordSupportBodyCostFinal(CombatPolicyContext context)
        {
            if (context.Metrics.SupportBodyHitsBeforeSupport < 0)
            {
                return;
            }

            context.Metrics.SupportBodyHitsFinal = context.Metrics.EnemyFrontlineBodyHits;
            context.Metrics.SupportDamageFinal = context.Metrics.PlayerDamageTaken;
        }

        private static void RecordSupportComboHudBeforeSlot1(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction supportAction)
        {
            if (supportAction == null)
            {
                return;
            }

            context.Metrics.SupportComboHudSupportLabelBeforeSlot1 =
                BossBarrageLaneReviewMobileHudLabels.BuildSupportSummonLabel(
                    supportAction,
                    ResolveSupportHudSlotLabel(supportAction),
                    "NEXT",
                    context.EnergyLadder);
            context.Metrics.SupportComboHudSupportFillBeforeSlot1 =
                BossBarrageLaneReviewMobileHudLabels.ResolveSupportSummonFill01(
                    context.EnergyLadder,
                    supportAction);
            context.Metrics.SupportComboHudSlot1LabelBeforeAttempt =
                BossBarrageLaneReviewMobileHudLabels.BuildPrimarySummonLabel(
                    BossBarrageSummonReviewContract.Slot1HudLabel,
                    context.EnergyLadder,
                    context.SummonSlot1Action);
            context.Metrics.SupportComboHudSlot1FillBeforeAttempt =
                BossBarrageLaneReviewMobileHudLabels.ResolvePrimarySummonFill01(
                    context.EnergyLadder,
                    context.SummonSlot1Action);
            context.Metrics.SupportComboOverlayHudReadoutBeforeSlot1 =
                context.ReviewHud != null
                    ? context.ReviewHud.SummonReadinessReadout
                    : string.Empty;
        }

        private static string ResolveSupportHudSlotLabel(PlayerSupportSummonSlotAction supportAction)
        {
            return supportAction != null && supportAction.SlotActionName == "SummonSlot3"
                ? BossBarrageSummonReviewContract.Slot3HudLabel
                : BossBarrageSummonReviewContract.Slot2HudLabel;
        }

        private static void RecordSkillUse(CombatPolicyContext context)
        {
            context.Metrics.SkillUses++;
            context.Metrics.HighestSkill1SpentTier = Mathf.Max(
                context.Metrics.HighestSkill1SpentTier,
                context.Skill1Action.LastSpentTier);
            if (context.Metrics.FirstSkill1UseAtSeconds < 0f)
            {
                context.Metrics.FirstSkill1UseAtSeconds = context.Metrics.ElapsedSeconds;
            }
        }

        private static IEnumerator WaitForCounterFinalWindow(
            CombatPolicyContext context,
            float maxSeconds,
            bool expectWindowBeforeSkill1 = true)
        {
            float start = context.Metrics.ElapsedSeconds;
            while (!context.PocketOwner.IsCounterWaveFinalWindowOpened
                && context.Metrics.ElapsedSeconds - start < maxSeconds)
            {
                yield return Advance(context, 0.1f);
            }

            if (!context.PocketOwner.IsCounterWaveFinalWindowOpened)
            {
                context.Metrics.Notes.Add(
                    expectWindowBeforeSkill1
                        ? "counter final window did not open before Skill1"
                        : "counter final window pending until fresh summon answer");
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
            AppendRouteMotivationDominanceMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendEnemyPressureTacticalCostMatrix(builder, results);
            builder.AppendLine();
            AppendPhysicalPressureConversionMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendCombatDecisionSignalMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendHighTierWaitAgencyMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportDecisionMatrixSummary(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportPayoffVectorMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportBodyCostPhaseMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportWaitExposureMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportUpgradeDeltaMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportUpgradeDecisionReadoutMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSupportStageSlotTimelineMatrix(builder, results, repeatabilityResults);
            builder.AppendLine();
            AppendSummonSlotReadinessCooldownMatrix(builder, results);
            builder.AppendLine();
            AppendSummonHudReadinessReadoutMatrix(builder, results);
            builder.AppendLine();
            AppendStageResultMotivationMatrix(builder, results);
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
            builder.AppendLine("| Policy | Target risk | Target tier | LV1 | LV2 | LV3 | Player down | HP lost | HP/s | Avg risk | Avg gain | Band seconds B/M/F | End tier/fill | Last band |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append(FormatOptionalPercent01(result.EnergyProbeTargetForwardRisk01));
                builder.Append(" | ");
                builder.Append(FormatEnergyProbeTargetTier(result));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.EnergyTier1DurationSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.EnergyTier2DurationSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.EnergyTier3DurationSeconds));
                builder.Append(" | ");
                builder.Append(FormatSeconds(result.FirstPlayerDownAtSeconds));
                builder.Append(" | ");
                builder.Append(result.PlayerDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.EnergyProbePlayerDamagePerSecond.ToString("0.0"));
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
            AppendEnergyTierDecisionPressure(builder, results);
            builder.AppendLine();
            AppendEnergySpendDecisionRoute(builder, results);
            builder.AppendLine();
            AppendEnergySpendRecoveryRoute(builder, results);
            builder.AppendLine();
            AppendSupportSummonRouteIdentity(builder, results);
            builder.AppendLine();
            AppendSharedManaSupportComboBranch(builder, results);
            builder.AppendLine();
            AppendSharedManaDelayedMainAnswerBranch(builder, results);
            builder.AppendLine();
            AppendSharedManaDelayedCounterRecoveryBranch(builder, results);
            builder.AppendLine();
            AppendSummonRosterIdentityAudit(builder, results);
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
            builder.AppendLine("| Policy | Screen window/hit/miss/suppress | Camera window/hit/miss/suppress | VFX window/hit/miss/suppress | Hit tier cam/vfx | Hit dmg cam/vfx | Last follow-up screen |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                builder.Append("| ");
                builder.Append(result.Policy);
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowScreenCueRequests}/{result.FollowupHitScreenCueRequests}/{result.FollowupMissedScreenCueRequests}/{result.FollowupSuppressScreenCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowCameraCueRequests}/{result.FollowupHitCameraCueRequests}/{result.FollowupMissedCameraCueRequests}/{result.FollowupSuppressCameraCueRequests}");
                builder.Append(" | ");
                builder.Append($"{result.FollowupWindowVfxCueRequests}/{result.FollowupHitVfxCueRequests}/{result.FollowupMissedVfxCueRequests}/{result.FollowupSuppressVfxCueRequests}");
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
            PolicyMetrics forwardRiskTier1Decision = RequireResult(results, PolicyKind.ForwardRiskTier1DecisionRoute);
            PolicyMetrics forwardRiskTier2Decision = RequireResult(results, PolicyKind.ForwardRiskTier2DecisionRoute);
            PolicyMetrics forwardRiskTier3Decision = RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute);
            PolicyMetrics forwardRiskTier1Recovery = RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute);
            PolicyMetrics forwardRiskTier2Recovery = RequireResult(results, PolicyKind.ForwardRiskTier2RecoveryRoute);
            PolicyMetrics forwardRiskTier3Recovery = RequireResult(results, PolicyKind.ForwardRiskTier3RecoveryRoute);
            PolicyMetrics forwardRiskSlot2Marksman =
                RequireResult(results, PolicyKind.ForwardRiskSlot2MarksmanRoute);
            PolicyMetrics forwardRiskSlot3Vanguard =
                RequireResult(results, PolicyKind.ForwardRiskSlot3VanguardRoute);
            PolicyMetrics forwardRiskSlot2Combo =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
            PolicyMetrics forwardRiskSlot3Blocked =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute);
            PolicyMetrics forwardRiskSlot2Delayed =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route);
            PolicyMetrics forwardRiskSlot3Delayed =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route);
            PolicyMetrics forwardRiskSlot2DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute);
            PolicyMetrics forwardRiskSlot3DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
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
            builder.AppendLine($"- EN spend branch: direct LV1/LV2/LV3 results `{forwardRiskTier1Decision.ResultKind}`/`{forwardRiskTier2Decision.ResultKind}`/`{forwardRiskTier3Decision.ResultKind}` with first unresolved `{ResolveFirstUnresolvedBeat(forwardRiskTier1Decision)}`/`{ResolveFirstUnresolvedBeat(forwardRiskTier2Decision)}`/`{ResolveFirstUnresolvedBeat(forwardRiskTier3Decision)}`; recovery results `{forwardRiskTier1Recovery.ResultKind}`/`{forwardRiskTier2Recovery.ResultKind}`/`{forwardRiskTier3Recovery.ResultKind}` with HP lost {forwardRiskTier1Recovery.PlayerDamageTaken:0.0}/{forwardRiskTier2Recovery.PlayerDamageTaken:0.0}/{forwardRiskTier3Recovery.PlayerDamageTaken:0.0}.");
            builder.AppendLine($"- Support answer split: Slot2 `{ResolveSupportAnswerBeat(forwardRiskSlot2Marksman)}` leaves `{ResolveFirstUnresolvedBeat(forwardRiskSlot2Marksman)}` after {forwardRiskSlot2Marksman.SupportSummonProjectileEnemySummonHits} enemy-frontline hits and {forwardRiskSlot2Marksman.PhysicalBarragePlayerHits}/{forwardRiskSlot2Marksman.PhysicalBarrageTrackedProjectileCount} physical player hits; Slot3 `{ResolveSupportAnswerBeat(forwardRiskSlot3Vanguard)}` leaves `{ResolveFirstUnresolvedBeat(forwardRiskSlot3Vanguard)}` after {forwardRiskSlot3Vanguard.SupportSummonProjectileEnemySummonHits}/{forwardRiskSlot3Vanguard.SupportSummonProjectileBossHits} enemy/boss breath hits and {forwardRiskSlot3Vanguard.PhysicalBarragePlayerHits}/{forwardRiskSlot3Vanguard.PhysicalBarrageTrackedProjectileCount} physical player hits.");
            builder.AppendLine($"- Shared-mana combo split: Slot2 leaves {forwardRiskSlot2Combo.SupportComboManaAfterSupport:0.#} EN after support, reaches {forwardRiskSlot2Combo.SupportComboManaBeforeSlot1:0.#} EN before Slot1, and Slot1 use `{forwardRiskSlot2Combo.SupportComboSlot1Used}` -> `{forwardRiskSlot2Combo.ResultKind}` / `{ResolveFirstUnresolvedBeat(forwardRiskSlot2Combo)}`; Slot3 leaves {forwardRiskSlot3Blocked.SupportComboManaAfterSupport:0.#} EN and Slot1 use `{forwardRiskSlot3Blocked.SupportComboSlot1Used}` ({forwardRiskSlot3Blocked.SupportComboSlot1BlockedReason}) -> `{ResolveFirstUnresolvedBeat(forwardRiskSlot3Blocked)}`.");
            builder.AppendLine($"- Delayed main-answer split: Slot2 early spend reopens Slot1 after {FormatSeconds(forwardRiskSlot2Delayed.SupportComboSlot1ReadyDelaySeconds)} with HP lost {forwardRiskSlot2Delayed.SupportComboPlayerDamageBeforeSlot1:0.0} before Slot1 and result `{forwardRiskSlot2Delayed.ResultKind}` / `{ResolveFirstUnresolvedBeat(forwardRiskSlot2Delayed)}`; Slot3 early spend reopens Slot1 after {FormatSeconds(forwardRiskSlot3Delayed.SupportComboSlot1ReadyDelaySeconds)} with HP lost {forwardRiskSlot3Delayed.SupportComboPlayerDamageBeforeSlot1:0.0} before Slot1 and result `{forwardRiskSlot3Delayed.ResultKind}` / `{ResolveFirstUnresolvedBeat(forwardRiskSlot3Delayed)}`.");
            builder.AppendLine($"- Delayed counter-recovery split: Slot2 recovery result `{forwardRiskSlot2DelayedRecovery.ResultKind}` / `{ResolveFirstUnresolvedBeat(forwardRiskSlot2DelayedRecovery)}` with counter->answer {FormatSeconds(forwardRiskSlot2DelayedRecovery.CounterTriggerToAnswerSeconds)} and final->hit {FormatSeconds(forwardRiskSlot2DelayedRecovery.FinalWindowToHitSeconds)}; Slot3 recovery result `{forwardRiskSlot3DelayedRecovery.ResultKind}` / `{ResolveFirstUnresolvedBeat(forwardRiskSlot3DelayedRecovery)}` with counter->answer {FormatSeconds(forwardRiskSlot3DelayedRecovery.CounterTriggerToAnswerSeconds)} and final->hit {FormatSeconds(forwardRiskSlot3DelayedRecovery.FinalWindowToHitSeconds)}.");
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
            builder.AppendLine($"- ArkData effective pressure shape: no-action peak/top3 {FormatPercent01(noSummon.PeakPressureWindowShare01)}/{FormatPercent01(noSummon.Top3PressureWindowShare01)}, intended {FormatPercent01(intended.PeakPressureWindowShare01)}/{FormatPercent01(intended.Top3PressureWindowShare01)} with relief {FormatSeconds(intended.TimeToNextReliefWindowSeconds)}, ignored boss-screen unanswered burden {FormatSeconds(ignoredRecovery.PressureBurdenSeconds)} versus intended {FormatSeconds(intended.PressureBurdenSeconds)}.");
            builder.AppendLine($"- Enemy pressure actor cost: no-action clashes {noSummon.EnemyFrontlineClashes} / body hits {noSummon.EnemyFrontlineBodyHits} / clash damage {noSummon.EnemyFrontlineClashDamage:0.0}; intended route clashes {intended.EnemyFrontlineClashes} / body hits {intended.EnemyFrontlineBodyHits}.");
            builder.AppendLine($"- Hit reaction split: boss-screen recovery produced {blockedRecovery.TotalSummonDamageFlashes} summon damage flashes, {blockedRecovery.TotalSummonFullBodyHitReactions} full-body hit reactions, and {blockedRecovery.TotalNonLockingSummonDamageCues} non-locking damage cues.");
            builder.AppendLine($"- Damage response split: gun-only boss chip {gunOnly.BossNonLockingDamageEvents}/{gunOnly.BossLockingDamageEvents} non-lock/lock, intended Skill1 boss hits {intended.BossNonLockingDamageEvents}/{intended.BossLockingDamageEvents}, boss-screen recovery {blockedRecovery.BossNonLockingDamageEvents}/{blockedRecovery.BossLockingDamageEvents}.");
            builder.AppendLine($"- Follow-up presentation bridge: gun-only hit cues screen/camera/VFX {gunOnly.FollowupHitScreenCueRequests}/{gunOnly.FollowupHitCameraCueRequests}/{gunOnly.FollowupHitVfxCueRequests}, intended {intended.FollowupHitScreenCueRequests}/{intended.FollowupHitCameraCueRequests}/{intended.FollowupHitVfxCueRequests}, boss-screen recovery {blockedRecovery.FollowupHitScreenCueRequests}/{blockedRecovery.FollowupHitCameraCueRequests}/{blockedRecovery.FollowupHitVfxCueRequests}, Slot3 suppress {forwardRiskSlot3Delayed.FollowupSuppressScreenCueRequests}/{forwardRiskSlot3Delayed.FollowupSuppressCameraCueRequests}/{forwardRiskSlot3Delayed.FollowupSuppressVfxCueRequests}.");
            builder.AppendLine($"- Follow-up micro-cinematic bridge: gun-only hit director/sequence {gunOnly.FollowupHitCinematicCueRequests}/{gunOnly.FollowupHitSequenceBridgeRequests}, intended {intended.FollowupHitCinematicCueRequests}/{intended.FollowupHitSequenceBridgeRequests}, boss-screen recovery {blockedRecovery.FollowupHitCinematicCueRequests}/{blockedRecovery.FollowupHitSequenceBridgeRequests}, Slot3 suppress {forwardRiskSlot3Delayed.FollowupHitCinematicCueRequests}/{forwardRiskSlot3Delayed.FollowupHitSequenceBridgeRequests}; frame overlays intended/Slot3 {intended.FollowupHitCinematicFrameOverlayCount}/{forwardRiskSlot3Delayed.FollowupHitCinematicFrameOverlayCount}.");
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
            builder.AppendLine($"Repeated sample count: `{RepeatabilityProbeRuns}` per selected policy. Required gate rows must preserve structural direction; CHECK rows outside the required set remain diagnostic.");
            builder.AppendLine("| Policy | Runs | Result set | HP lost min/avg/max | Boss dmg min/avg/max | Player hits min/max | Blocks min/max | Support proj min/max | Skill1 min/max | Micro hit min/max | Seq hit min/max | Verdict |");
            builder.AppendLine("|---|---:|---|---:|---:|---:|---:|---:|---:|---:|---:|---|");
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
                MinMetric(repeatabilityResults, policy, result => result.SupportSummonProjectileHits),
                MaxMetric(repeatabilityResults, policy, result => result.SupportSummonProjectileHits)));
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
            PolicyMetrics highTierSuppress = RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute);
            PolicyMetrics slot3DelayedPayoff =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
            PolicyMetrics slot3RetreatRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute);
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
                && forwardRiskPhysicalSummonNoPunish.EnemyFrontlineBodyHits > 0
                && ignoredRecovery.EnemyFrontlineBodyHits > blockedRecovery.EnemyFrontlineBodyHits
                && ignoredRecovery.PlayerDamageTaken > blockedRecovery.PlayerDamageTaken
                && blockedRecovery.EnemyFrontlineBodyHits == 0
                && blockedRecovery.EnemyFrontlineSummonHits > 0
                && forwardRiskPhysicalSummonPunish.EnemyFrontlineBodyHits == 0;
            bool highTierAgencyPass = slot3DelayedPayoff.BossScreenSuppressedByFollowup
                && slot3RetreatRecovery.CounterRecoveryConfirmed
                && !slot3RetreatRecovery.BossScreenSuppressedByFollowup
                && slot3RetreatRecovery.PlayerDamageTaken < slot3DelayedPayoff.PlayerDamageTaken
                && slot3RetreatRecovery.BackSafetyBandSeconds > slot3DelayedPayoff.BackSafetyBandSeconds + 1f
                && ResolveHighTierWaitAgencySeconds(slot3RetreatRecovery)
                    > ResolveHighTierWaitAgencySeconds(slot3DelayedPayoff);

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
                $"| 4. Enemy pressure actor cost | {FormatGateStatus(axis4Pass)} | no-action body hits {noSummon.EnemyFrontlineBodyHits}; no-punish body hits {forwardRiskPhysicalSummonNoPunish.EnemyFrontlineBodyHits}, damage {forwardRiskPhysicalSummonNoPunish.PlayerDamageTaken:0.0}; ignored boss-screen body hits {ignoredRecovery.EnemyFrontlineBodyHits}, damage {ignoredRecovery.PlayerDamageTaken:0.0}; recovery body/summon hits {blockedRecovery.EnemyFrontlineBodyHits}/{blockedRecovery.EnemyFrontlineSummonHits}, damage {blockedRecovery.PlayerDamageTaken:0.0}; clean punish body hits {forwardRiskPhysicalSummonPunish.EnemyFrontlineBodyHits} |");
            builder.AppendLine(
                $"| High-tier movement agency | {FormatGateStatus(highTierAgencyPass)} | hold-front Slot3 wait {FormatSeconds(ResolveHighTierWaitAgencySeconds(slot3DelayedPayoff))}, HP {slot3DelayedPayoff.PlayerDamageTaken:0.0}, suppress {FormatSupportDecisionBossSuppress(slot3DelayedPayoff)}, hook `{ResolveResultHookClass(slot3DelayedPayoff)}`; retreat/recommit wait {FormatSeconds(ResolveHighTierWaitAgencySeconds(slot3RetreatRecovery))}, HP {slot3RetreatRecovery.PlayerDamageTaken:0.0}, back/forward bands {FormatSeconds(slot3RetreatRecovery.BackSafetyBandSeconds)}/{FormatSeconds(slot3RetreatRecovery.ForwardRiskBandSeconds)}, hook `{ResolveResultHookClass(slot3RetreatRecovery)}` |");
            builder.AppendLine(
                $"| Physical clean route reference | {FormatGateStatus(forwardRiskPhysicalSummonPunish.IsClearResult)} | physical summon-punish clears in {FormatSeconds(forwardRiskPhysicalSummonPunish.ElapsedSeconds)} with {forwardRiskPhysicalSummonPunish.PlayerDamageTaken:0.0} HP lost versus intended route {FormatSeconds(intended.ElapsedSeconds)} / {intended.PlayerDamageTaken:0.0} HP lost |");
        }

        private static void AppendRouteMotivationDominanceMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            PolicyMetrics physicalPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
            PolicyMetrics counterRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
            PolicyMetrics highTierSuppress = RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute);
            PolicyMetrics marksmanCombo = RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
            PolicyMetrics vanguardPayoff =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
            PolicyMetrics vanguardRetreatRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute);

            builder.AppendLine("## Route Motivation/Dominance Matrix");
            builder.AppendLine("- ArkData/NIKKE lens: clear routes should expose distinct route motivation, not collapse into one fastest clear or one generic reward row.");
            builder.AppendLine("| Route role | Policy | Cost/risk | Sample payoff | Result hook | Repeat band | Decision read |");
            builder.AppendLine("|---|---|---|---|---|---|---|");
            AppendRouteMotivationDominanceRow(
                builder,
                "Fast execution reference",
                physicalPunish,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, physicalPunish.Policy));
            AppendRouteMotivationDominanceRow(
                builder,
                "Guided clean loop",
                intended,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, intended.Policy));
            AppendRouteMotivationDominanceRow(
                builder,
                "Counter recovery safety",
                counterRecovery,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, counterRecovery.Policy));
            AppendRouteMotivationDominanceRow(
                builder,
                "High-tier suppress payoff",
                highTierSuppress,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, highTierSuppress.Policy));
            AppendRouteMotivationDominanceRow(
                builder,
                "Slot2 full-bank support",
                marksmanCombo,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, marksmanCombo.Policy));
            AppendRouteMotivationDominanceRow(
                builder,
                "Slot3 delayed line-hold",
                vanguardPayoff,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, vanguardPayoff.Policy));
            AppendRouteMotivationDominanceRow(
                builder,
                "Slot3 retreat recovery",
                vanguardRetreatRecovery,
                FormatRouteMotivationRepeatSignal(repeatabilityResults, vanguardRetreatRecovery.Policy));
            builder.Append("- Dominance read: ");
            builder.AppendLine(BuildRouteMotivationDominanceRead(
                physicalPunish,
                intended,
                counterRecovery,
                highTierSuppress,
                marksmanCombo,
                vanguardPayoff,
                vanguardRetreatRecovery));
        }

        private static void AppendRouteMotivationDominanceRow(
            StringBuilder builder,
            string routeRole,
            PolicyMetrics result,
            string repeatSignal)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(routeRole));
            builder.Append(" | ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatRouteMotivationCostRisk(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatRouteMotivationPayoff(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveResultHookClass(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(repeatSignal));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveRouteMotivationDecisionRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatRouteMotivationCostRisk(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    return $"forward risk {FormatOptionalPercent01(result.PhysicalBarrageProbeTargetForwardRisk01)}, "
                        + $"HP {result.PlayerDamageTaken:0.0}, blocks {result.SummonBlocks}";
                case PolicyKind.IntendedRoute:
                    return $"close hits {result.CloseThreatBasicHits}, "
                        + $"summon at {FormatSeconds(result.FirstSummonUseAtSeconds)}, "
                        + $"block {FormatSeconds(result.SummonUseToBlockSeconds)}, "
                        + $"window {FormatSeconds(result.BlockToFollowupWindowSeconds)}, "
                        + $"HP {result.PlayerDamageTaken:0.0}";
                case PolicyKind.BossScreenBlockCounterRecovery:
                    return $"counter {result.CounterWaves}, answer {FormatSeconds(result.CounterTriggerToAnswerSeconds)}, "
                        + $"HP {result.PlayerDamageTaken:0.0}";
                case PolicyKind.ForwardRiskTier3DecisionRoute:
                    return $"tier {result.HighestSummonSpentTier}, HP {result.PlayerDamageTaken:0.0}, "
                        + $"suppress {FormatSupportDecisionBossSuppress(result)}";
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return $"cost {result.SupportSummonRequiredMana:0}, mana after {result.SupportComboManaAfterSupport:0.#}, "
                        + $"HP {result.PlayerDamageTaken:0.0}";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return $"cost {result.SupportSummonRequiredMana:0}, Slot1 wait {FormatSeconds(result.SupportComboSlot1ReadyDelaySeconds)}, "
                        + $"HP {result.PlayerDamageTaken:0.0}";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return $"cost {result.SupportSummonRequiredMana:0}, retreat wait {FormatSeconds(ResolveHighTierWaitAgencySeconds(result))}, "
                        + $"back/forward {FormatSeconds(result.BackSafetyBandSeconds)}/{FormatSeconds(result.ForwardRiskBandSeconds)}, "
                        + $"HP {result.PlayerDamageTaken:0.0}";
                default:
                    return $"HP {result.PlayerDamageTaken:0.0}, blocks {result.SummonBlocks}";
            }
        }

        private static string FormatRouteMotivationPayoff(PolicyMetrics result)
        {
            return $"{FormatSeconds(result.ElapsedSeconds)}, boss {result.BossDamageTaken:0.0} "
                + $"(player/ally {result.BossDamageFromPlayer:0.0}/{result.BossDamageFromAllySummon:0.0})";
        }

        private static string FormatRouteMotivationRepeatSignal(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return BuildResultKindSet(repeatabilityResults, policy)
                + " "
                + ResolveRepeatabilityVerdict(repeatabilityResults, policy)
                + "; time min/avg/max "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.ElapsedSeconds),
                    AverageMetric(repeatabilityResults, policy, result => result.ElapsedSeconds),
                    MaxMetric(repeatabilityResults, policy, result => result.ElapsedSeconds))
                + "s; HP min/avg/max "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                    AverageMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                    MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken))
                + "; boss min/avg/max "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    AverageMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    MaxMetric(repeatabilityResults, policy, result => result.BossDamageTaken));
        }

        private static string ResolveRouteMotivationDecisionRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    return "fastest only when the block-punish is clean";
                case PolicyKind.IntendedRoute:
                    return "baseline teaches close probe -> summon -> Skill1";
                case PolicyKind.BossScreenBlockCounterRecovery:
                    return "missed/blocked confirm can recover through counter answer";
                case PolicyKind.ForwardRiskTier3DecisionRoute:
                    return "HP-costly wait buys direct boss-screen suppress";
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return "full-bank support preserves Slot1 and adds marksman payoff";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return "high-cost line hold converts into boss-screen payoff";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return "retreat reduces HP exposure but gives up direct suppress for counter recovery";
                default:
                    return "not evaluated";
            }
        }

        private static string BuildRouteMotivationDominanceRead(
            PolicyMetrics physicalPunish,
            PolicyMetrics intended,
            PolicyMetrics counterRecovery,
            PolicyMetrics highTierSuppress,
            PolicyMetrics marksmanCombo,
            PolicyMetrics vanguardPayoff,
            PolicyMetrics vanguardRetreatRecovery)
        {
            return $"physical punish is fastest at {FormatSeconds(physicalPunish.ElapsedSeconds)} "
                + $"versus guided {FormatSeconds(intended.ElapsedSeconds)} and recovery {FormatSeconds(counterRecovery.ElapsedSeconds)}; "
                + $"the slower HP-cost routes keep separate hooks `{ResolveResultHookClass(highTierSuppress)}`, "
                + $"`{ResolveResultHookClass(marksmanCombo)}`, and `{ResolveResultHookClass(vanguardPayoff)}`. "
                + $"Slot3 hold-front versus retreat is a movement trade: {vanguardPayoff.PlayerDamageTaken:0.0} HP / "
                + $"`{ResolveResultHookClass(vanguardPayoff)}` against {vanguardRetreatRecovery.PlayerDamageTaken:0.0} HP / "
                + $"`{ResolveResultHookClass(vanguardRetreatRecovery)}`. "
                + "Decision: preserve route identities and tune only when a route loses its cost/payoff reason.";
        }

        private static void AppendEnemyPressureTacticalCostMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics noPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
            PolicyMetrics ignoredRecovery = RequireResult(results, PolicyKind.BossScreenIgnoredNoRecovery);
            PolicyMetrics blockedRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
            PolicyMetrics physicalPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe);

            builder.AppendLine("## Enemy Pressure Tactical Cost Matrix");
            builder.AppendLine("- ArkData lens: enemy pressure actors should create a readable unattended cost path, then change target/status when answered by summon structure.");
            builder.AppendLine("| Scenario | Tactical state | Enemy body hits | Enemy summon hits | Enemy clashes | Player damage | Unanswered burden | Result | Read |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---|---|");
            AppendEnemyPressureTacticalCostRow(
                builder,
                "No action",
                "unattended actor reaches body",
                noSummon,
                "body pressure is live without being instant death");
            AppendEnemyPressureTacticalCostRow(
                builder,
                "Block but no punish",
                "answer started, punish missed",
                noPunish,
                "opened state relocks into enemy pressure if Skill1 is skipped");
            AppendEnemyPressureTacticalCostRow(
                builder,
                "Ignore boss-screen counter",
                "partial answer left unresolved",
                ignoredRecovery,
                "enemy pressure keeps taxing HP and route stability");
            AppendEnemyPressureTacticalCostRow(
                builder,
                "Counter recovery",
                "fresh summon redirects pressure",
                blockedRecovery,
                "body cost converts into summon clash and clear route");
            AppendEnemyPressureTacticalCostRow(
                builder,
                "Clean physical punish",
                "pressure answered before actor cost",
                physicalPunish,
                "confirmed Skill1 prevents unattended actor cost");
        }

        private static void AppendEnemyPressureTacticalCostRow(
            StringBuilder builder,
            string scenario,
            string tacticalState,
            PolicyMetrics result,
            string read)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(scenario));
            builder.Append(" | ");
            builder.Append(EscapeTable(tacticalState));
            builder.Append(" | ");
            builder.Append(result.EnemyFrontlineBodyHits);
            builder.Append(" | ");
            builder.Append(result.EnemyFrontlineSummonHits);
            builder.Append(" | ");
            builder.Append(result.EnemyFrontlineClashes);
            builder.Append(" | ");
            builder.Append(result.PlayerDamageTaken.ToString("0.0"));
            builder.Append(" | ");
            builder.Append(FormatPercent01(result.UnansweredPressureBurdenShare01));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(read));
            builder.AppendLine(" |");
        }

        private static void AppendPhysicalPressureConversionMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            PolicyMetrics backline = RequireResult(results, PolicyKind.BacklinePhysicalBarrageProbe);
            PolicyMetrics forward = RequireResult(results, PolicyKind.ForwardRiskPhysicalBarrageProbe);
            PolicyMetrics block = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonBlockProbe);
            PolicyMetrics noPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
            PolicyMetrics punish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe);

            builder.AppendLine("## Physical Pressure Conversion Matrix");
            builder.AppendLine("- ArkData/CombatPayload/PGR lens: forward physical pressure must become a measured Target/Hit cost, then convert through summon block into a lock/unlock state branch.");
            builder.AppendLine("| Beat | Policy | Target/Hit read | Conversion state | Player cost | Boss payoff | Repeat floor | Verdict |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|");
            AppendPhysicalPressureConversionRow(
                builder,
                "Backline safety",
                backline,
                $"risk {FormatOptionalPercent01(backline.PhysicalBarrageProbeTargetForwardRisk01)}, {backline.PhysicalBarragePatternId}",
                $"hits {backline.PhysicalBarragePlayerHits}/{backline.PhysicalBarrageTrackedProjectileCount}",
                $"damage {backline.PhysicalBarragePlayerDamage:0.0}",
                "none",
                FormatPhysicalPressureRepeatSignal(repeatabilityResults, backline.Policy),
                "safe lane stays safe");
            AppendPhysicalPressureConversionRow(
                builder,
                "Forward danger floor",
                forward,
                $"risk {FormatOptionalPercent01(forward.PhysicalBarrageProbeTargetForwardRisk01)}, {forward.PhysicalBarragePatternId}",
                $"hits {forward.PhysicalBarragePlayerHits}/{forward.PhysicalBarrageTrackedProjectileCount}",
                $"damage {forward.PhysicalBarragePlayerDamage:0.0}",
                "none",
                FormatPhysicalPressureRepeatSignal(repeatabilityResults, forward.Policy),
                "forward read has real HP cost");
            AppendPhysicalPressureConversionRow(
                builder,
                "Summon converts hit",
                block,
                $"incoming {block.PhysicalBarrageTrackedProjectileCount}",
                $"blocks {block.SummonBlocks}, window {FormatSeconds(block.BlockToFollowupWindowSeconds)}",
                $"hits {block.PhysicalBarragePlayerHits}, damage {block.PhysicalBarragePlayerDamage:0.0}",
                $"ally {block.BossDamageFromAllySummon:0.0}",
                FormatPhysicalPressureRepeatSignal(repeatabilityResults, block.Policy),
                "block opens state instead of only deleting damage");
            AppendPhysicalPressureConversionRow(
                builder,
                "Window unconfirmed",
                noPunish,
                $"window {FormatSeconds(noPunish.FirstFollowupWindowAtSeconds)}",
                $"miss {noPunish.FollowupMissCount}, counter {noPunish.CounterWaves}",
                $"body {noPunish.EnemyFrontlineBodyHits}, damage {noPunish.PlayerDamageTaken:0.0}",
                $"player {noPunish.BossDamageFromPlayer:0.0}, ally {noPunish.BossDamageFromAllySummon:0.0}",
                FormatPhysicalPressureRepeatSignal(repeatabilityResults, noPunish.Policy),
                "unconfirmed window relocks into counter pressure");
            AppendPhysicalPressureConversionRow(
                builder,
                "Skill1 confirms",
                punish,
                $"window {FormatSeconds(punish.FirstFollowupWindowDurationSeconds)}",
                $"Skill1 {punish.SkillProjectileHits}, {punish.ResultKind}",
                $"hits {punish.PhysicalBarragePlayerHits}, damage {punish.PlayerDamageTaken:0.0}",
                $"player {punish.BossDamageFromPlayer:0.0}, ally {punish.BossDamageFromAllySummon:0.0}, share {FormatPercent01(punish.BossDamagePlayerShare01)}",
                FormatPhysicalPressureRepeatSignal(repeatabilityResults, punish.Policy),
                "player-authored punish completes the pressure slot");
        }

        private static void AppendPhysicalPressureConversionRow(
            StringBuilder builder,
            string beat,
            PolicyMetrics result,
            string targetHitRead,
            string conversionState,
            string playerCost,
            string bossPayoff,
            string repeatFloor,
            string verdict)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(beat));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.Policy.ToString()));
            builder.Append(" | ");
            builder.Append(EscapeTable(targetHitRead));
            builder.Append(" | ");
            builder.Append(EscapeTable(conversionState));
            builder.Append(" | ");
            builder.Append(EscapeTable(playerCost));
            builder.Append(" | ");
            builder.Append(EscapeTable(bossPayoff));
            builder.Append(" | ");
            builder.Append(EscapeTable(repeatFloor));
            builder.Append(" | ");
            builder.Append(EscapeTable(verdict));
            builder.AppendLine(" |");
        }

        private static string FormatPhysicalPressureRepeatSignal(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            switch (policy)
            {
                case PolicyKind.BacklinePhysicalBarrageProbe:
                case PolicyKind.ForwardRiskPhysicalBarrageProbe:
                    return "hits "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits),
                            MaxMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits))
                        + "; dmg "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerDamage),
                            MaxMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerDamage));
                case PolicyKind.ForwardRiskPhysicalSummonBlockProbe:
                    return "blocks "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.SummonBlocks),
                            MaxMetric(repeatabilityResults, policy, result => result.SummonBlocks))
                        + "; hits "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits),
                            MaxMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits))
                        + "; dmg "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerDamage),
                            MaxMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerDamage));
                case PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe:
                    return "miss "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.FollowupMissCount),
                            MaxMetric(repeatabilityResults, policy, result => result.FollowupMissCount))
                        + "; counter "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.CounterWaves),
                            MaxMetric(repeatabilityResults, policy, result => result.CounterWaves))
                        + "; body "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.EnemyFrontlineBodyHits),
                            MaxMetric(repeatabilityResults, policy, result => result.EnemyFrontlineBodyHits));
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    return "Skill1 "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.SkillProjectileHits),
                            MaxMetric(repeatabilityResults, policy, result => result.SkillProjectileHits))
                        + "; HP "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                            MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken))
                        + "; result "
                        + BuildResultKindSet(repeatabilityResults, policy);
                default:
                    return "runs " + CountPolicyResults(repeatabilityResults, policy);
            }
        }

        private static void AppendCombatDecisionSignalMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            PolicyMetrics forwardRiskEnergy = RequireResult(results, PolicyKind.ForwardRiskEnergyProbe);
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics block = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonBlockProbe);
            PolicyMetrics noPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe);
            PolicyMetrics recovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
            PolicyMetrics cleanPunish = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe);

            builder.AppendLine("## Combat Decision Signal Matrix");
            builder.AppendLine("- ArkData/CombatPayload/PGR lens: each route decision should expose a target/status/presentation signal before the result row judges it.");
            builder.AppendLine("| Decision beat | Policy | Pre-action signal | Post-action readout | Cue/readout counts | Repeat signal | Decision state | Guardrail |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|");
            AppendCombatDecisionSignalRow(
                builder,
                "Risk for EN",
                forwardRiskEnergy,
                $"ForwardRisk band {FormatSeconds(forwardRiskEnergy.ForwardRiskBandSeconds)}",
                $"LV1 ready {FormatSeconds(forwardRiskEnergy.EnergyTier1DurationSeconds)}, HP {forwardRiskEnergy.PlayerDamageTaken:0.0}",
                FormatEnergyDecisionSignal(forwardRiskEnergy),
                FormatCombatDecisionRepeatSignal(repeatabilityResults, forwardRiskEnergy.Policy),
                "resource readiness is measured before final coaster/UI feedback");
            AppendCombatDecisionSignalRow(
                builder,
                "Ignore pressure",
                noSummon,
                "no summon answer",
                $"body hits {noSummon.EnemyFrontlineBodyHits}, HP {noSummon.PlayerDamageTaken:0.0}",
                $"player damage {noSummon.PlayerDamageScreenCueRequests}/{noSummon.PlayerDamageFeedbackRequests}",
                FormatCombatDecisionRepeatSignal(repeatabilityResults, noSummon.Policy),
                "unanswered pressure must hurt without becoming instant death proof");
            AppendCombatDecisionSignalRow(
                builder,
                "Summon into barrage",
                block,
                $"physical barrage {block.PhysicalBarrageTrackedProjectileCount} tracked",
                $"blocks {block.SummonBlocks}, window {FormatSeconds(block.BlockToFollowupWindowSeconds)}",
                FormatBlockDecisionSignal(block),
                FormatCombatDecisionRepeatSignal(repeatabilityResults, block.Policy),
                "block must create a readable window, not only remove damage");
            AppendCombatDecisionSignalRow(
                builder,
                "Skip Skill1 punish",
                noPunish,
                $"window open {FormatSeconds(noPunish.FirstFollowupWindowAtSeconds)}",
                $"miss {noPunish.FollowupMissCount}, counter {noPunish.CounterWaves}, body hits {noPunish.EnemyFrontlineBodyHits}",
                FormatMissCounterDecisionSignal(noPunish),
                FormatCombatDecisionRepeatSignal(repeatabilityResults, noPunish.Policy),
                "missed punish should relock into counter pressure");
            AppendCombatDecisionSignalRow(
                builder,
                "Answer counter",
                recovery,
                $"counter source {recovery.LastCounterWaveSource}",
                $"pulse {recovery.CounterWaveAnswerEnergyPulse:0}, stable {FormatSeconds(recovery.CounterAnswerToStableSeconds)}, hit {FormatSeconds(recovery.FinalWindowToHitSeconds)}",
                FormatCounterAnswerDecisionSignal(recovery),
                FormatCombatDecisionRepeatSignal(repeatabilityResults, recovery.Policy),
                "fresh summon answer should unlock recovery through measured state beats");
            AppendCombatDecisionSignalRow(
                builder,
                "Confirm Skill1",
                cleanPunish,
                $"follow-up window {FormatSeconds(cleanPunish.FirstFollowupWindowDurationSeconds)}",
                $"boss damage {cleanPunish.BossDamageFromPlayer:0.0}, HP leak {cleanPunish.PlayerDamageTaken:0.0}",
                FormatFollowupHitDecisionSignal(cleanPunish),
                FormatCombatDecisionRepeatSignal(repeatabilityResults, cleanPunish.Policy),
                "clean payoff should read through hit cues and no player damage leak");
        }

        private static void AppendCombatDecisionSignalRow(
            StringBuilder builder,
            string decisionBeat,
            PolicyMetrics result,
            string preActionSignal,
            string postActionReadout,
            string cueCounts,
            string repeatSignal,
            string guardrail)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(decisionBeat));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.Policy.ToString()));
            builder.Append(" | ");
            builder.Append(EscapeTable(preActionSignal));
            builder.Append(" | ");
            builder.Append(EscapeTable(postActionReadout));
            builder.Append(" | ");
            builder.Append(EscapeTable(cueCounts));
            builder.Append(" | ");
            builder.Append(EscapeTable(repeatSignal));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveCombatDecisionSignalState(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(guardrail));
            builder.AppendLine(" |");
        }

        private static string FormatCombatDecisionRepeatSignal(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            switch (policy)
            {
                case PolicyKind.ForwardRiskEnergyProbe:
                    return "LV1 "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.EnergyTier1DurationSeconds),
                            MaxMetric(repeatabilityResults, policy, result => result.EnergyTier1DurationSeconds))
                        + "s; HP "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                            MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken))
                        + "; ready "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.EnergyReadyScreenCueRequests),
                            MaxMetric(repeatabilityResults, policy, result => result.EnergyReadyScreenCueRequests));
                case PolicyKind.NoSummonNoFire:
                    return "body "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.EnemyFrontlineBodyHits),
                            MaxMetric(repeatabilityResults, policy, result => result.EnemyFrontlineBodyHits))
                        + "; HP "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                            MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken))
                        + "; damage cues "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PlayerDamageScreenCueRequests),
                            MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageScreenCueRequests));
                case PolicyKind.ForwardRiskPhysicalSummonBlockProbe:
                    return "blocks "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.SummonBlocks),
                            MaxMetric(repeatabilityResults, policy, result => result.SummonBlocks))
                        + "; hits "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits),
                            MaxMetric(repeatabilityResults, policy, result => result.PhysicalBarragePlayerHits));
                case PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe:
                    return "miss "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.FollowupMissCount),
                            MaxMetric(repeatabilityResults, policy, result => result.FollowupMissCount))
                        + "; counter "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.CounterWaves),
                            MaxMetric(repeatabilityResults, policy, result => result.CounterWaves))
                        + "; body "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.EnemyFrontlineBodyHits),
                            MaxMetric(repeatabilityResults, policy, result => result.EnemyFrontlineBodyHits));
                case PolicyKind.BossScreenBlockCounterRecovery:
                    return "answer cue "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.CounterWaveAnswerScreenCueRequests),
                            MaxMetric(repeatabilityResults, policy, result => result.CounterWaveAnswerScreenCueRequests))
                        + "; skill1 "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.SkillProjectileHits),
                            MaxMetric(repeatabilityResults, policy, result => result.SkillProjectileHits));
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    return "skill1 "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.SkillProjectileHits),
                            MaxMetric(repeatabilityResults, policy, result => result.SkillProjectileHits))
                        + "; HP "
                        + FormatMinMax(
                            MinMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken),
                            MaxMetric(repeatabilityResults, policy, result => result.PlayerDamageTaken));
                default:
                    return "runs " + CountPolicyResults(repeatabilityResults, policy);
            }
        }

        private static string ResolveCombatDecisionSignalState(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskEnergyProbe:
                    return result.EnergyTier1ReadyAtSeconds >= 0f && result.EnergyReadyScreenCueRequests > 0
                        ? $"{result.ResultKind}/ENReady"
                        : $"{result.ResultKind}/ENPending";
                case PolicyKind.NoSummonNoFire:
                    return result.EnemyFrontlineBodyHits > 0 && result.PlayerDamageScreenCueRequests > 0
                        ? $"{result.ResultKind}/BodyPressure"
                        : $"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}";
                case PolicyKind.ForwardRiskPhysicalSummonBlockProbe:
                    return result.SummonBlocks > 0 && result.FollowupWindowOpenCount > 0
                        ? $"{result.ResultKind}/FollowupReady"
                        : $"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}";
                case PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe:
                    return result.FollowupMissCount > 0 && result.CounterWaves > 0
                        ? $"{result.ResultKind}/CounterAnswer"
                        : $"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}";
                case PolicyKind.BossScreenBlockCounterRecovery:
                    return result.CounterRecoveryConfirmed
                        ? $"{result.ResultKind}/CounterRecovered"
                        : $"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}";
                case PolicyKind.ForwardRiskPhysicalSummonPunishProbe:
                    return result.ResultKind == "CleanFollowupClear"
                        ? $"{result.ResultKind}/Complete"
                        : $"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}";
                default:
                    return $"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}";
            }
        }

        private static string FormatEnergyDecisionSignal(PolicyMetrics result)
        {
            return $"EN screen F/R/S {result.ForwardRiskEnergyScreenCueRequests}/{result.EnergyReadyScreenCueRequests}/{result.EnergySpendScreenCueRequests}; "
                + $"VFX {result.ForwardRiskEnergyVfxCueRequests}/{result.EnergyReadyVfxCueRequests}/{result.EnergySpendVfxCueRequests}";
        }

        private static string FormatBlockDecisionSignal(PolicyMetrics result)
        {
            return $"block cam/flash/VFX {result.SummonPressureBlockCameraCueRequests}/{result.SummonPressureScreenInterceptFlashes}/{result.SummonPressureScreenInterceptVfxCueRequests}; "
                + $"window screen/cam/VFX {result.FollowupWindowScreenCueRequests}/{result.FollowupWindowCameraCueRequests}/{result.FollowupWindowVfxCueRequests}";
        }

        private static string FormatMissCounterDecisionSignal(PolicyMetrics result)
        {
            return $"miss screen/cam/VFX {result.FollowupMissedScreenCueRequests}/{result.FollowupMissedCameraCueRequests}/{result.FollowupMissedVfxCueRequests}; "
                + $"counter screen/cam/VFX {result.CounterWaveScreenCueRequests}/{result.CounterWaveCameraCueRequests}/{result.CounterWaveVfxCueRequests}";
        }

        private static string FormatCounterAnswerDecisionSignal(PolicyMetrics result)
        {
            return $"answer screen/cam/VFX {result.CounterWaveAnswerScreenCueRequests}/{result.CounterWaveStabilizedCameraCueRequests}/{result.CounterWaveStabilizedVfxCueRequests}; "
                + $"EN ready/spend {result.EnergyReadyScreenCueRequests}/{result.EnergySpendScreenCueRequests}";
        }

        private static string FormatFollowupHitDecisionSignal(PolicyMetrics result)
        {
            return $"hit screen/cam/VFX {result.FollowupHitScreenCueRequests}/{result.FollowupHitCameraCueRequests}/{result.FollowupHitVfxCueRequests}; "
                + $"cine/frame {result.FollowupHitCinematicCueRequests}/{result.FollowupHitCinematicFrameOverlayCount}";
        }

        private static void AppendSupportDecisionMatrixSummary(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## Support Decision Matrix");
            builder.AppendLine("- ArkData/Blue Archive lens: support choice should read as cost, exposure, answer, and recovery-state tradeoff before UI/coaster feedback.");
            builder.AppendLine("| Choice | Cost path | Support effect | HP before main | Physical hits | Slot1 state | Recovery burden | Boss suppress | Sample time/dmg | Result | Repeat band | Timing verdict | Payoff verdict | Read |");
            builder.AppendLine("|---|---|---|---:|---:|---|---|---:|---:|---|---|---|---|---|");
            AppendSupportDecisionMatrixRow(
                builder,
                "Slot1 LV2 recovery",
                "200",
                "charge screen/counter opener",
                RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute),
                repeatabilityResults,
                "Slot1 locked below LV2",
                "promoted S1 is not an LV1 emergency answer");
            AppendSupportDecisionMatrixRow(
                builder,
                "Slot2 full-bank combo",
                "300 -> 100 -> 200",
                "marksman suppresses enemy frontline",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                "Slot1 preserved",
                "low-cost support preserves the main answer but the payoff is still diagnostic");
            AppendSupportDecisionMatrixRow(
                builder,
                "Slot2 delayed recovery",
                "200 -> 100 -> recharge -> 200",
                "marksman suppresses enemy frontline",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults,
                "Slot1 reopens",
                "early marksman spend still needs counter recovery");
            AppendSupportDecisionMatrixRow(
                builder,
                "Slot3 immediate lockout",
                "300 -> 0",
                "vanguard holds physical line",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults,
                "Slot1 blocked",
                "high-cost hold spends the immediate main-answer turn");
            AppendSupportDecisionMatrixRow(
                builder,
                "Slot3 delayed recovery",
                "300 -> recharge -> 200",
                "vanguard holds physical line",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                "Slot1 reopens",
                "vanguard assist suppresses boss screen for the confirm");
            PolicyMetrics slot1Recovery = RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute);
            PolicyMetrics slot2Combo = RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
            PolicyMetrics slot2DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute);
            PolicyMetrics slot3Immediate = RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute);
            PolicyMetrics slot3DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
            builder.AppendLine();
            builder.AppendLine(
                "- Dominance read: "
                + $"Slot2 delayed recovery matches Slot1's recovery result but costs more and shifts HP by {FormatSupportDecisionHpDelta(slot1Recovery, slot2DelayedRecovery)}; "
                + $"Slot2 full-bank pays {FormatSupportDecisionHpDelta(slot1Recovery, slot2Combo)} HP to preserve Slot1, then remains `{slot2Combo.ResultKind}` with repeat {FormatSupportDecisionRepeatTimeDamageBand(repeatabilityResults, slot2Combo.Policy)}; "
                + $"Slot2 delayed has recovery burden and stays capped against the Slot1 recovery repeat band {FormatSupportDecisionRepeatTimeDamageBand(repeatabilityResults, slot1Recovery.Policy)} versus its own {FormatSupportDecisionRepeatTimeDamageBand(repeatabilityResults, slot2DelayedRecovery.Policy)}; "
                + $"Slot3 immediate stays `{slot3Immediate.ResultKind}/{ResolveFirstUnresolvedBeat(slot3Immediate)}` despite line hold; "
                + $"Slot3 delayed pays {FormatSupportDecisionHpDelta(slot1Recovery, slot3DelayedRecovery)} HP for `{slot3DelayedRecovery.ResultKind}` with repeat {FormatSupportDecisionRepeatTimeDamageBand(repeatabilityResults, slot3DelayedRecovery.Policy)} and suppress {FormatSupportDecisionBossSuppress(slot3DelayedRecovery)}. "
                + "Decision: keep Slot2 full-bank as a visible diagnostic gap and preserve Slot3 as the stable suppress payoff.");
        }

        private static void AppendSupportDecisionMatrixRow(
            StringBuilder builder,
            string choice,
            string costPath,
            string supportEffect,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            string slot1State,
            string read)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(choice));
            builder.Append(" | ");
            builder.Append(EscapeTable(costPath));
            builder.Append(" | ");
            builder.Append(EscapeTable(supportEffect));
            builder.Append(" | ");
            builder.Append(FormatSupportDecisionHpBeforeMain(result));
            builder.Append(" | ");
            builder.Append($"{result.PhysicalBarragePlayerHits}/{result.PhysicalBarrageTrackedProjectileCount}");
            builder.Append(" | ");
            builder.Append(EscapeTable(slot1State));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportDecisionRecoveryBurden(result)));
            builder.Append(" | ");
            builder.Append(FormatSupportDecisionBossSuppress(result));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportDecisionTimeDamage(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable($"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}"));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportDecisionRepeatSignal(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportDecisionTimingVerdict(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportDecisionPayoffVerdict(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(read));
            builder.AppendLine(" |");
        }

        private static string FormatSupportDecisionRepeatSignal(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return BuildResultKindSet(repeatabilityResults, policy)
                + " "
                + ResolveRepeatabilityVerdict(repeatabilityResults, policy)
                + "; time min/avg/max "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.ElapsedSeconds),
                    AverageMetric(repeatabilityResults, policy, result => result.ElapsedSeconds),
                    MaxMetric(repeatabilityResults, policy, result => result.ElapsedSeconds))
                + "s; HP before main min/avg/max "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => ResolveSupportDecisionHpBeforeMain(result)),
                    AverageMetric(repeatabilityResults, policy, result => ResolveSupportDecisionHpBeforeMain(result)),
                    MaxMetric(repeatabilityResults, policy, result => ResolveSupportDecisionHpBeforeMain(result)))
                + "; boss min/avg/max "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    AverageMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    MaxMetric(repeatabilityResults, policy, result => result.BossDamageTaken))
                + "; skill1 min/max "
                + FormatMinMax(
                    MinMetric(repeatabilityResults, policy, result => result.SkillProjectileHits),
                    MaxMetric(repeatabilityResults, policy, result => result.SkillProjectileHits))
                + "; suppress min/max "
                + FormatMinMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup),
                    MaxMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup));
        }

        private static string FormatSupportDecisionRepeatTimeDamageBand(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return "time "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.ElapsedSeconds),
                    AverageMetric(repeatabilityResults, policy, result => result.ElapsedSeconds),
                    MaxMetric(repeatabilityResults, policy, result => result.ElapsedSeconds))
                + "s / boss "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    AverageMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    MaxMetric(repeatabilityResults, policy, result => result.BossDamageTaken));
        }

        private static string FormatSupportDecisionHpBeforeMain(PolicyMetrics result)
        {
            return ResolveSupportDecisionHpBeforeMain(result).ToString("0.0");
        }

        private static string FormatSupportDecisionHpDelta(
            PolicyMetrics baseline,
            PolicyMetrics candidate)
        {
            float delta = ResolveSupportDecisionHpBeforeMain(candidate)
                - ResolveSupportDecisionHpBeforeMain(baseline);
            return delta >= 0f ? $"+{delta:0.0}" : delta.ToString("0.0");
        }

        private static float ResolveSupportDecisionHpBeforeMain(PolicyMetrics result)
        {
            return result.SupportComboPlayerDamageBeforeSlot1 >= 0f
                ? result.SupportComboPlayerDamageBeforeSlot1
                : result.PlayerDamageTaken;
        }

        private static string FormatSupportDecisionRecoveryBurden(PolicyMetrics result)
        {
            if (result.CounterWaves <= 0)
            {
                return "none";
            }

            return $"counter {FormatSeconds(result.CounterTriggerToAnswerSeconds)} -> hit {FormatSeconds(result.FinalWindowToHitSeconds)}";
        }

        private static string FormatSupportDecisionBossSuppress(PolicyMetrics result)
        {
            return $"{result.BossPressureScreensSuppressedByFollowup}/{result.HighestBossScreenSuppressSummonTier}";
        }

        private static string FormatSupportDecisionTimeDamage(PolicyMetrics result)
        {
            return $"{FormatSeconds(result.ElapsedSeconds)} / {result.BossDamageTaken:0.0}";
        }

        private static void AppendSupportPayoffVectorMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## Support Payoff Vector Matrix");
            builder.AppendLine("- ArkData/CombatPayload lens: support payoff should separate damage, prevention, and relock cost instead of treating every slot as a damage race.");
            builder.AppendLine("| Choice | Policy | Sample damage vector | Prevention vector | Relock cost | Repeat damage band | Repeat prevention band | Payoff read |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|");
            AppendSupportPayoffVectorRow(
                builder,
                "Slot1 LV2 recovery",
                RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute),
                repeatabilityResults);
            AppendSupportPayoffVectorRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendSupportPayoffVectorRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults);
            AppendSupportPayoffVectorRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults);
            AppendSupportPayoffVectorRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendSupportPayoffVectorRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(choice));
            builder.Append(" | ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportPayoffDamageVector(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportPayoffPreventionVector(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportPayoffRelockCost(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportPayoffRepeatDamageBand(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportPayoffRepeatPreventionBand(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportPayoffVectorRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportPayoffDamageVector(PolicyMetrics result)
        {
            return $"boss {result.BossDamageTaken:0.0}, player/ally {result.BossDamageFromPlayer:0.0}/{result.BossDamageFromAllySummon:0.0}";
        }

        private static string FormatSupportPayoffPreventionVector(PolicyMetrics result)
        {
            return $"blocks {result.SupportSummonBlocks}, support hits E/B {result.SupportSummonProjectileEnemySummonHits}/{result.SupportSummonProjectileBossHits}, physical hits {result.PhysicalBarragePlayerHits}/{result.PhysicalBarrageTrackedProjectileCount}, suppress {FormatSupportDecisionBossSuppress(result)}";
        }

        private static string FormatSupportPayoffRelockCost(PolicyMetrics result)
        {
            return $"counter {result.CounterWaves}, body hits {result.EnemyFrontlineBodyHits}, unresolved {ResolveFirstUnresolvedBeat(result)}";
        }

        private static string FormatSupportPayoffRepeatDamageBand(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return "boss "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    AverageMetric(repeatabilityResults, policy, result => result.BossDamageTaken),
                    MaxMetric(repeatabilityResults, policy, result => result.BossDamageTaken))
                + "; ally "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossDamageFromAllySummon),
                    AverageMetric(repeatabilityResults, policy, result => result.BossDamageFromAllySummon),
                    MaxMetric(repeatabilityResults, policy, result => result.BossDamageFromAllySummon));
        }

        private static string FormatSupportPayoffRepeatPreventionBand(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return "support hits E "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.SupportSummonProjectileEnemySummonHits),
                    AverageMetric(repeatabilityResults, policy, result => result.SupportSummonProjectileEnemySummonHits),
                    MaxMetric(repeatabilityResults, policy, result => result.SupportSummonProjectileEnemySummonHits))
                + "; blocks "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.SupportSummonBlocks),
                    AverageMetric(repeatabilityResults, policy, result => result.SupportSummonBlocks),
                    MaxMetric(repeatabilityResults, policy, result => result.SupportSummonBlocks))
                + "; suppress "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup),
                    AverageMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup),
                    MaxMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup))
                + "; counter "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.CounterWaves),
                    AverageMetric(repeatabilityResults, policy, result => result.CounterWaves),
                    MaxMetric(repeatabilityResults, policy, result => result.CounterWaves));
        }

        private static string ResolveSupportPayoffVectorRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskTier1RecoveryRoute:
                    return "baseline recovery damage";
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return "damage route, no relock";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return "damage recovered through relock";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return "prevention-only until recharge";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return "prevention route, boss-screen suppress";
                default:
                    return "unclassified support payoff";
            }
        }

        private static void AppendSupportBodyCostPhaseMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## Support Body-Cost Phase Matrix");
            builder.AppendLine("- ArkData/NIKKE lens: support route cost should name which stage slot paid the body-pressure tax before changing vanguard tuning.");
            builder.AppendLine("| Choice | Policy | Body hits phase | Player damage phase | Post-support delta | Repeat final body | Repeat post-support delta | Read |");
            builder.AppendLine("|---|---|---|---|---:|---|---|---|");
            AppendSupportBodyCostPhaseRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendSupportBodyCostPhaseRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults);
            AppendSupportBodyCostPhaseRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults);
            AppendSupportBodyCostPhaseRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendSupportBodyCostPhaseRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(choice));
            builder.Append(" | ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportBodyHitPhase(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportDamagePhase(result)));
            builder.Append(" | ");
            builder.Append(FormatSupportBodyCostDelta(result));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportBodyCostRepeatFinalBody(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportBodyCostRepeatPostSupportDelta(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportBodyCostPhaseRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportBodyHitPhase(PolicyMetrics result)
        {
            return "before support "
                + FormatOptionalInt(result.SupportBodyHitsBeforeSupport)
                + " -> before Slot1 "
                + FormatOptionalInt(result.SupportBodyHitsBeforeMainAnswer)
                + " -> final "
                + FormatOptionalInt(result.SupportBodyHitsFinal);
        }

        private static string FormatSupportDamagePhase(PolicyMetrics result)
        {
            return "before support "
                + FormatOptionalFloat(result.SupportDamageBeforeSupport)
                + " -> before Slot1 "
                + FormatOptionalFloat(result.SupportDamageBeforeMainAnswer)
                + " -> final "
                + FormatOptionalFloat(result.SupportDamageFinal);
        }

        private static string FormatSupportBodyCostDelta(PolicyMetrics result)
        {
            float delta = ResolveSupportBodyHitsAfterSupportDelta(result);
            return delta >= 0f ? delta.ToString("0.#") : "-";
        }

        private static string FormatSupportBodyCostRepeatFinalBody(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            return FormatMinAverageMax(
                MinMetric(repeatabilityResults, policy, result => result.SupportBodyHitsFinal),
                AverageMetric(repeatabilityResults, policy, result => result.SupportBodyHitsFinal),
                MaxMetric(repeatabilityResults, policy, result => result.SupportBodyHitsFinal));
        }

        private static string FormatSupportBodyCostRepeatPostSupportDelta(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            return FormatMinAverageMax(
                MinMetric(repeatabilityResults, policy, ResolveSupportBodyHitsAfterSupportDelta),
                AverageMetric(repeatabilityResults, policy, ResolveSupportBodyHitsAfterSupportDelta),
                MaxMetric(repeatabilityResults, policy, ResolveSupportBodyHitsAfterSupportDelta));
        }

        private static float ResolveSupportBodyHitsAfterSupportDelta(PolicyMetrics result)
        {
            return result.SupportBodyHitsBeforeSupport >= 0 && result.SupportBodyHitsFinal >= 0
                ? result.SupportBodyHitsFinal - result.SupportBodyHitsBeforeSupport
                : -1f;
        }

        private static string ResolveSupportBodyCostPhaseRead(PolicyMetrics result)
        {
            if (result.SupportBodyHitsBeforeSupport < 0)
            {
                return "not measured";
            }

            float postSupportDelta = ResolveSupportBodyHitsAfterSupportDelta(result);
            if (postSupportDelta <= 0f)
            {
                return "body cost paid before support";
            }

            if (result.SupportSummonSlotId == "SummonSlot3" && result.SupportSummonProjectileHits > 0)
            {
                return "dragon fire still leaks body cost after support";
            }

            return "support phase still leaks body cost";
        }

        private static void AppendSupportWaitExposureMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## Support Wait Exposure Matrix");
            builder.AppendLine("- ArkData/Blue Archive lens: support-slot cost should expose how long the player stays under pressure before the LV2/LV3 answer is available.");
            builder.AppendLine("| Choice | Policy | Target/cost | Wait to support | Pre-support cost | Main-answer gate | Payoff | Repeat wait band | Repeat pre-support cost | Read |");
            builder.AppendLine("|---|---|---|---:|---|---|---|---|---|---|");
            AppendSupportWaitExposureRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendSupportWaitExposureRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults);
            AppendSupportWaitExposureRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults);
            AppendSupportWaitExposureRow(
                builder,
                "Slot3 delayed payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendSupportWaitExposureRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(choice));
            builder.Append(" | ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportWaitExposureTarget(result)));
            builder.Append(" | ");
            builder.Append(FormatSeconds(ResolveSupportWaitExposureSeconds(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportWaitExposureCost(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportWaitExposureMainAnswerGate(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportDecisionPayoffVerdict(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportWaitExposureRepeatBand(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportWaitExposureRepeatCostBand(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportWaitExposureRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportWaitExposureTarget(PolicyMetrics result)
        {
            int targetTier = ResolveSupportWaitExposureTargetTier(result);
            return $"{result.SupportSummonSlotId} LV{targetTier} / {result.SupportSummonRequiredMana:0} EN";
        }

        private static int ResolveSupportWaitExposureTargetTier(PolicyMetrics result)
        {
            return result.EnergyProbeTargetTier > 0
                ? result.EnergyProbeTargetTier
                : result.SupportSummonSpentTier;
        }

        private static float ResolveSupportWaitExposureSeconds(PolicyMetrics result)
        {
            return result.EnergyProbeStartAtSeconds >= 0f && result.FirstSummonUseAtSeconds >= 0f
                ? Mathf.Max(0f, result.FirstSummonUseAtSeconds - result.EnergyProbeStartAtSeconds)
                : -1f;
        }

        private static string FormatSupportWaitExposureCost(PolicyMetrics result)
        {
            return "body "
                + FormatOptionalInt(result.SupportBodyHitsBeforeSupport)
                + "; HP "
                + FormatOptionalFloat(result.SupportDamageBeforeSupport);
        }

        private static string FormatSupportWaitExposureMainAnswerGate(PolicyMetrics result)
        {
            if (result.SupportComboSlot1Attempted && !result.SupportComboSlot1Used)
            {
                return "Slot1 blocked: " + result.SupportComboSlot1BlockedReason;
            }

            if (result.SupportComboSlot1ReadyDelaySeconds > 0.05f)
            {
                return "Slot1 ready after " + FormatSeconds(result.SupportComboSlot1ReadyDelaySeconds);
            }

            if (result.SupportComboSlot1Used)
            {
                return "Slot1 preserved";
            }

            return "Slot1 pending";
        }

        private static string FormatSupportWaitExposureRepeatBand(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return FormatMinAverageMax(
                MinMetric(repeatabilityResults, policy, ResolveSupportWaitExposureSeconds),
                AverageMetric(repeatabilityResults, policy, ResolveSupportWaitExposureSeconds),
                MaxMetric(repeatabilityResults, policy, ResolveSupportWaitExposureSeconds))
                + "s";
        }

        private static string FormatSupportWaitExposureRepeatCostBand(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return "body "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.SupportBodyHitsBeforeSupport),
                    AverageMetric(repeatabilityResults, policy, result => result.SupportBodyHitsBeforeSupport),
                    MaxMetric(repeatabilityResults, policy, result => result.SupportBodyHitsBeforeSupport))
                + "; HP "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, result => result.SupportDamageBeforeSupport),
                    AverageMetric(repeatabilityResults, policy, result => result.SupportDamageBeforeSupport),
                    MaxMetric(repeatabilityResults, policy, result => result.SupportDamageBeforeSupport));
        }

        private static string ResolveSupportWaitExposureRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return "full-bank wait preserves Slot1 but leaves the marksman payoff incomplete";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return "LV2 spend lowers wait cost but still relocks into recovery";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return "LV3 wait buys line hold but spends the main-answer turn";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return "LV3 wait cost converts into boss-screen suppress payoff";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return "deep LV2 retreat trades longer wait for safer counter recovery";
                default:
                    return "not evaluated";
            }
        }

        private static void AppendSupportUpgradeDeltaMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            PolicyMetrics slot2DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute);
            builder.AppendLine("## Support Upgrade Delta Matrix");
            builder.AppendLine("- ArkData/Blue Archive lens: the LV2 hold/spend choice should expose the marginal wait cost before the LV3 payoff is judged.");
            builder.AppendLine("| Decision delta | From | To | Extra wait | Extra pre-support cost | Main-answer delta | Result shift | Repeat delta | Read |");
            builder.AppendLine("|---|---|---|---:|---|---|---|---|---|");
            AppendSupportUpgradeDeltaRow(
                builder,
                "Bank Slot2 to full LV3",
                "Slot2 LV2 now",
                slot2DelayedRecovery,
                "Slot2 full-bank",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendSupportUpgradeDeltaRow(
                builder,
                "Upgrade from Slot2 LV2 to Slot3 LV3",
                "Slot2 LV2 now",
                slot2DelayedRecovery,
                "Slot3 LV3 payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendSupportUpgradeDeltaRow(
            StringBuilder builder,
            string deltaLabel,
            string fromChoice,
            PolicyMetrics from,
            string toChoice,
            PolicyMetrics to,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(deltaLabel));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeChoice(fromChoice, from)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeChoice(toChoice, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSignedSeconds(ResolveSupportWaitDeltaSeconds(from, to))));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradePreSupportDelta(from, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeMainAnswerDelta(from, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeResultShift(from, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeRepeatDelta(repeatabilityResults, from.Policy, to.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportUpgradeDeltaRead(from, to)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportUpgradeChoice(string choice, PolicyMetrics result)
        {
            return choice + " (" + FormatSupportWaitExposureTarget(result) + ")";
        }

        private static float ResolveSupportWaitDeltaSeconds(PolicyMetrics from, PolicyMetrics to)
        {
            return ResolveSupportWaitExposureSeconds(to) - ResolveSupportWaitExposureSeconds(from);
        }

        private static float ResolveSupportPreSupportBodyDelta(PolicyMetrics from, PolicyMetrics to)
        {
            return to.SupportBodyHitsBeforeSupport - from.SupportBodyHitsBeforeSupport;
        }

        private static float ResolveSupportPreSupportDamageDelta(PolicyMetrics from, PolicyMetrics to)
        {
            return to.SupportDamageBeforeSupport - from.SupportDamageBeforeSupport;
        }

        private static float ResolveSupportMainAnswerDelaySeconds(PolicyMetrics result)
        {
            if (result.SupportComboSlot1ReadyDelaySeconds >= 0f)
            {
                return result.SupportComboSlot1ReadyDelaySeconds;
            }

            return result.SupportComboSlot1Used ? 0f : -1f;
        }

        private static float ResolveSupportMainAnswerDelayDelta(PolicyMetrics from, PolicyMetrics to)
        {
            return ResolveSupportMainAnswerDelaySeconds(to) - ResolveSupportMainAnswerDelaySeconds(from);
        }

        private static string FormatSupportUpgradePreSupportDelta(PolicyMetrics from, PolicyMetrics to)
        {
            return "body "
                + FormatSignedMetric(ResolveSupportPreSupportBodyDelta(from, to))
                + "; HP "
                + FormatSignedMetric(ResolveSupportPreSupportDamageDelta(from, to));
        }

        private static string FormatSupportUpgradeMainAnswerDelta(PolicyMetrics from, PolicyMetrics to)
        {
            return "Slot1 delay " + FormatSignedSeconds(ResolveSupportMainAnswerDelayDelta(from, to));
        }

        private static string FormatSupportUpgradeResultShift(PolicyMetrics from, PolicyMetrics to)
        {
            return ResolveSupportDecisionPayoffVerdict(from)
                + " -> "
                + ResolveSupportDecisionPayoffVerdict(to)
                + " ("
                + ResolveResultHookClass(from)
                + " -> "
                + ResolveResultHookClass(to)
                + ")";
        }

        private static string FormatSupportUpgradeRepeatDelta(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind fromPolicy,
            PolicyKind toPolicy)
        {
            if (CountPolicyResults(repeatabilityResults, fromPolicy) <= 0
                || CountPolicyResults(repeatabilityResults, toPolicy) <= 0)
            {
                return "not repeated";
            }

            float waitDelta = AverageMetric(repeatabilityResults, toPolicy, ResolveSupportWaitExposureSeconds)
                - AverageMetric(repeatabilityResults, fromPolicy, ResolveSupportWaitExposureSeconds);
            float bodyDelta = AverageMetric(repeatabilityResults, toPolicy, result => result.SupportBodyHitsBeforeSupport)
                - AverageMetric(repeatabilityResults, fromPolicy, result => result.SupportBodyHitsBeforeSupport);
            float damageDelta = AverageMetric(repeatabilityResults, toPolicy, result => result.SupportDamageBeforeSupport)
                - AverageMetric(repeatabilityResults, fromPolicy, result => result.SupportDamageBeforeSupport);
            return "avg wait "
                + FormatSignedSeconds(waitDelta)
                + "; body "
                + FormatSignedMetric(bodyDelta)
                + "; HP "
                + FormatSignedMetric(damageDelta);
        }

        private static string ResolveSupportUpgradeDeltaRead(PolicyMetrics from, PolicyMetrics to)
        {
            if (from.Policy == PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute
                && to.Policy == PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute)
            {
                return "extra full-bank wait keeps Slot1 immediate but leaves the marksman payoff incomplete";
            }

            if (from.Policy == PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute
                && to.Policy == PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute)
            {
                return "extra LV3 wait buys vanguard boss-screen suppress at visible HP/body cost";
            }

            return "unclassified support upgrade delta";
        }

        private static void AppendSupportUpgradeDecisionReadoutMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            PolicyMetrics slot2DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute);
            builder.AppendLine("## Support Upgrade Decision Readout Matrix");
            builder.AppendLine("- ArkData/Blue Archive lens: the visible EX-like slot readout must be judged against the measured LV2-now/LV3-later trade, not only against static cost labels.");
            builder.AppendLine("| Decision | Current visible state | Upgrade visible state | Measured extra cost | Measured payoff shift | Repeat check | Read |");
            builder.AppendLine("|---|---|---|---|---|---|---|");
            AppendSupportUpgradeDecisionReadoutRow(
                builder,
                "Bank Slot2 to full LV3",
                "Slot2 LV2 now",
                slot2DelayedRecovery,
                "Slot2 full-bank",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendSupportUpgradeDecisionReadoutRow(
                builder,
                "Upgrade from Slot2 LV2 to Slot3 LV3",
                "Slot2 LV2 now",
                slot2DelayedRecovery,
                "Slot3 LV3 payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendSupportUpgradeDecisionReadoutRow(
            StringBuilder builder,
            string decision,
            string fromChoice,
            PolicyMetrics from,
            string toChoice,
            PolicyMetrics to,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(decision));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeDecisionHudState(fromChoice, from)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeDecisionHudState(toChoice, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeDecisionMeasuredCost(from, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeDecisionMeasuredPayoff(from, to)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportUpgradeRepeatDelta(repeatabilityResults, from.Policy, to.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportUpgradeDecisionReadoutRead(from, to)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportUpgradeDecisionHudState(string choice, PolicyMetrics result)
        {
            return choice
                + ": support "
                + FormatHudLabelAndFill(
                    result.SupportComboHudSupportLabelBeforeSlot1,
                    result.SupportComboHudSupportFillBeforeSlot1)
                + "; Slot1 "
                + FormatHudLabelAndFill(
                    result.SupportComboHudSlot1LabelBeforeAttempt,
                    result.SupportComboHudSlot1FillBeforeAttempt)
                + "; overlay "
                + ResolveCoverageValue(result.SupportComboOverlayHudReadoutBeforeSlot1)
                + "; forecast "
                + ResolveCoverageValue(result.SupportChoiceForecastReadoutBeforeSupport);
        }

        private static string FormatSupportUpgradeDecisionMeasuredCost(PolicyMetrics from, PolicyMetrics to)
        {
            return "wait "
                + FormatSignedSeconds(ResolveSupportWaitDeltaSeconds(from, to))
                + "; body "
                + FormatSignedMetric(ResolveSupportPreSupportBodyDelta(from, to))
                + "; HP "
                + FormatSignedMetric(ResolveSupportPreSupportDamageDelta(from, to));
        }

        private static string FormatSupportUpgradeDecisionMeasuredPayoff(PolicyMetrics from, PolicyMetrics to)
        {
            return ResolveSupportDecisionPayoffVerdict(from)
                + " -> "
                + ResolveSupportDecisionPayoffVerdict(to)
                + "; "
                + FormatSupportUpgradeMainAnswerDelta(from, to);
        }

        private static string ResolveSupportUpgradeDecisionReadoutRead(PolicyMetrics from, PolicyMetrics to)
        {
            if (from.Policy == PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute
                && to.Policy == PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute)
            {
                return "visible readout proves independent slots; measured delta preserves Slot1 but keeps the marksman payoff incomplete";
            }

            if (from.Policy == PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute
                && to.Policy == PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute)
            {
                return "visible readout names the 300 EN gate; measured delta says holding buys boss-screen suppress, not a universal upgrade";
            }

            return "unclassified support upgrade readout";
        }

        private static string FormatSignedSeconds(float value)
        {
            return FormatSignedMetric(value) + "s";
        }

        private static string FormatSignedMetric(float value)
        {
            return value >= 0f ? $"+{value:0.#}" : value.ToString("0.#");
        }

        private static string ResolveSupportDecisionPayoffVerdict(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskTier1RecoveryRoute:
                    return "baseline recovery payoff";
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return result.IsClearResult && result.CounterWaves <= 0
                        ? "clean payoff, no recovery burden"
                        : "marksman payoff incomplete";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return result.CounterRecoveryConfirmed
                        ? "recovery baseline, burden"
                        : "delayed marksman payoff unresolved";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return result.IsClearResult
                        ? "unexpected vanguard clear"
                        : "no payoff until recharge";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return result.BossScreenSuppressedByFollowup
                        ? "boss-screen suppress payoff"
                        : "vanguard payoff unresolved";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return result.CounterRecoveryConfirmed
                        ? "safer counter-recovery payoff"
                        : "retreat payoff unresolved";
                default:
                    return "not evaluated";
            }
        }

        private static string ResolveSupportDecisionTimingVerdict(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskTier1RecoveryRoute:
                    return "emergency baseline";
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return ResolveFirstUnresolvedBeat(result) == "Complete" && result.CounterWaves <= 0
                        ? "intended marksman combo"
                        : "marksman combo incomplete";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return result.CounterRecoveryConfirmed
                        ? "mistimed LV2 spend"
                        : "delayed marksman unresolved";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return result.SupportComboSlot1Attempted && !result.SupportComboSlot1Used
                        ? "resource lockout"
                        : "vanguard lockout unresolved";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return result.BossScreenSuppressedByFollowup
                        ? "intended vanguard payoff"
                        : "vanguard recovery branch";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return result.CounterRecoveryConfirmed
                        ? "position-modulated recovery"
                        : "retreat branch unresolved";
                default:
                    return "not evaluated";
            }
        }

        private static void AppendSupportStageSlotTimelineMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## Support Stage-Slot Timeline Matrix");
            builder.AppendLine("- ArkData/NIKKE lens: support summons should read as ordered pressure slots, not isolated balance rows.");
            builder.AppendLine("| Route | Support slot | Support effect slot | Main-answer gate | Relock/payoff slot | Result hook | Repeat stage read | Decision read |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|");
            AppendSupportStageSlotTimelineRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendSupportStageSlotTimelineRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults);
            AppendSupportStageSlotTimelineRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults);
            AppendSupportStageSlotTimelineRow(
                builder,
                "Slot3 delayed payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendSupportStageSlotTimelineRow(
            StringBuilder builder,
            string route,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(route));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportStageSupportSlot(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportStageEffectSlot(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportStageMainAnswerGate(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportStageRelockPayoffSlot(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveResultHookClass(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportStageRepeatRead(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportStageDecisionRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportStageSupportSlot(PolicyMetrics result)
        {
            return $"{result.SupportSummonSlotId} cost {result.SupportSummonRequiredMana:0} -> mana {result.SupportComboManaAfterSupport:0.#}";
        }

        private static string FormatSupportStageEffectSlot(PolicyMetrics result)
        {
            if (result.SupportSummonSlotId == "SummonSlot2")
            {
                return $"marksman hits E/B {result.SupportSummonProjectileEnemySummonHits}/{result.SupportSummonProjectileBossHits}";
            }

            if (result.SupportSummonSlotId == "SummonSlot3")
            {
                return $"dragon hits E/B {result.SupportSummonProjectileEnemySummonHits}/{result.SupportSummonProjectileBossHits}, body hits {result.PhysicalBarragePlayerHits}/{result.PhysicalBarrageTrackedProjectileCount}";
            }

            return "support effect not measured";
        }

        private static string FormatSupportStageMainAnswerGate(PolicyMetrics result)
        {
            if (result.SupportComboSlot1Used)
            {
                string delay = result.SupportComboSlot1ReadyDelaySeconds >= 0f
                    ? FormatSeconds(result.SupportComboSlot1ReadyDelaySeconds)
                    : "immediate";
                return $"Slot1 used after {delay}, mana {result.SupportComboManaBeforeSlot1:0.#}";
            }

            if (result.SupportComboSlot1Attempted)
            {
                return $"Slot1 blocked: {result.SupportComboSlot1BlockedReason}";
            }

            return "Slot1 not attempted";
        }

        private static string FormatSupportStageRelockPayoffSlot(PolicyMetrics result)
        {
            if (result.BossScreenSuppressedByFollowup)
            {
                return $"boss-screen suppress {FormatSupportDecisionBossSuppress(result)}";
            }

            if (result.CounterWaves > 0)
            {
                return $"counter relock {result.CounterWaves}, answer {FormatSeconds(result.CounterTriggerToAnswerSeconds)}";
            }

            if (result.IsClearResult)
            {
                return $"clean hit {result.SkillProjectileHits}";
            }

            return $"unresolved {ResolveFirstUnresolvedBeat(result)}";
        }

        private static string FormatSupportStageRepeatRead(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return BuildResultKindSet(repeatabilityResults, policy)
                + " "
                + ResolveRepeatabilityVerdict(repeatabilityResults, policy)
                + "; Slot1 "
                + FormatMinMax(
                    MinMetric(repeatabilityResults, policy, result => result.SupportComboSlot1Used ? 1f : 0f),
                    MaxMetric(repeatabilityResults, policy, result => result.SupportComboSlot1Used ? 1f : 0f))
                + "; counter "
                + FormatMinMax(
                    MinMetric(repeatabilityResults, policy, result => result.CounterWaves),
                    MaxMetric(repeatabilityResults, policy, result => result.CounterWaves))
                + "; suppress "
                + FormatMinMax(
                    MinMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup),
                    MaxMetric(repeatabilityResults, policy, result => result.BossPressureScreensSuppressedByFollowup));
        }

        private static string ResolveSupportStageDecisionRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return "full-bank Slot2 preserves the main-answer slot";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return "early Slot2 buys tempo but relocks into counter recovery";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return "Slot3 spends the main-answer slot for line safety";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return "delayed Slot3 converts line hold into boss-screen payoff";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return "retreat Slot3 trades direct suppress for safer counter recovery";
                default:
                    return "not evaluated";
            }
        }

        private static void AppendSummonSlotReadinessCooldownMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Summon Slot Readiness/Cooldown Matrix");
            builder.AppendLine("- ArkData/Blue Archive lens: the summon bar is a shared EN bank with slot-specific costs, while each summon button keeps its own cooldown/readiness state before final UI/coaster feedback.");
            builder.AppendLine("| Choice | Support cd after/use-before-Slot1 | Slot1 cd before/after | Mana after support | Mana before Slot1 | Slot1 use/block | Result | Read |");
            builder.AppendLine("|---|---:|---:|---:|---:|---|---|---|");
            AppendSummonSlotReadinessCooldownRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute));
            AppendSummonSlotReadinessCooldownRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute));
            AppendSummonSlotReadinessCooldownRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute));
            AppendSummonSlotReadinessCooldownRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute));
        }

        private static void AppendSummonSlotReadinessCooldownRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(choice));
            builder.Append(" | ");
            builder.Append(
                $"{FormatSeconds(result.SupportComboSupportCooldownAfterSupport)}/{FormatSeconds(result.SupportComboSupportCooldownBeforeSlot1)}");
            builder.Append(" | ");
            builder.Append(
                $"{FormatSeconds(result.SupportComboSlot1CooldownBeforeAttempt)}/{FormatSeconds(result.SupportComboSlot1CooldownAfterAttempt)}");
            builder.Append(" | ");
            builder.Append(result.SupportComboManaAfterSupport.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(result.SupportComboManaBeforeSlot1.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatSupportComboSlot1UseState(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable($"{result.ResultKind}/{ResolveFirstUnresolvedBeat(result)}"));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSummonSlotReadinessCooldownRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatSupportComboSlot1UseState(PolicyMetrics result)
        {
            if (result.SupportComboSlot1Used)
            {
                return "used";
            }

            return result.SupportComboSlot1Attempted
                ? $"blocked:{result.SupportComboSlot1BlockedReason}"
                : "not attempted";
        }

        private static string ResolveSummonSlotReadinessCooldownRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return result.SupportComboSlot1Used
                        ? "Slot2 cooldown is active while Slot1 still fires because shared EN pays the Slot1 cost"
                        : "Slot2 combo did not prove Slot1 readiness";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return result.SupportComboSlot1BlockedReason.Contains("Requires 200 EN")
                        ? "Slot1 is blocked by shared EN, not by Slot3 cooldown or a global cooldown"
                        : "Slot3 lockout reason is ambiguous";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return result.SupportComboSlot1Used
                        ? "Slot2 cooldown clears during recharge; Slot1 reopens into counter recovery"
                        : "Slot2 delayed branch did not reopen Slot1";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return result.SupportComboSlot1Used
                        ? "Slot3 cooldown clears during recharge; Slot1 reopens into vanguard payoff"
                        : "Slot3 delayed branch did not reopen Slot1";
                default:
                    return "not a summon readiness branch";
            }
        }

        private static void AppendSummonHudReadinessReadoutMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Summon HUD Readiness Readout Matrix");
            builder.AppendLine("- ArkData/Blue Archive lens: the review HUD/coaster readout should expose the same shared EN bank and per-slot cooldown gates that the route simulation proves.");
            builder.AppendLine("| Choice | Mobile support HUD before Slot1 | Mobile Slot1 HUD before attempt | Overlay HUD before Slot1 | Support pulse | Slot1 pulse | Read |");
            builder.AppendLine("|---|---|---|---|---:|---:|---|");
            AppendSummonHudReadinessReadoutRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute));
            AppendSummonHudReadinessReadoutRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute));
            AppendSummonHudReadinessReadoutRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute));
            AppendSummonHudReadinessReadoutRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute));
        }

        private static void AppendSummonHudReadinessReadoutRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(choice));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHudLabelAndFill(
                result.SupportComboHudSupportLabelBeforeSlot1,
                result.SupportComboHudSupportFillBeforeSlot1)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHudLabelAndFill(
                result.SupportComboHudSlot1LabelBeforeAttempt,
                result.SupportComboHudSlot1FillBeforeAttempt)));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.SupportComboOverlayHudReadoutBeforeSlot1));
            builder.Append(" | ");
            builder.Append(FormatReadyPulse(result.SupportComboHudSupportFillBeforeSlot1));
            builder.Append(" | ");
            builder.Append(FormatReadyPulse(result.SupportComboHudSlot1FillBeforeAttempt));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSummonHudReadinessRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatHudLabelAndFill(string label, float fill01)
        {
            string normalizedLabel = string.IsNullOrWhiteSpace(label)
                ? "missing"
                : label.Replace("\r\n", " / ").Replace("\n", " / ");
            return $"{normalizedLabel} ({fill01:0.00})";
        }

        private static string FormatReadyPulse(float fill01)
        {
            return fill01 >= 0.995f ? "ON" : "off";
        }

        private static string ResolveSummonHudReadinessRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return "Slot2 shows cooldown while Slot1 coaster is ready";
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return "Slot3 shows cooldown but Slot1 coaster is empty, proving resource lockout";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return "Slot2 is resource-gated again while Slot1 coaster reopens";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return "Slot3 is resource-gated again while Slot1 coaster reopens";
                default:
                    return "not a HUD readiness branch";
            }
        }

        private static void AppendHighTierWaitAgencyMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.AppendLine("## High-Tier Wait Agency Matrix");
            builder.AppendLine("- ArkData/CombatPayload lens: high-tier waits should expose whether the player is making an active pressure-slot choice or only paying passive HP/body exposure before the spend.");
            builder.AppendLine("| Wait route | Target | Wait exposure | Pressure cost | Visible signal before spend | Payoff after spend | Repeat check | Agency read |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|");
            AppendHighTierWaitAgencyRow(
                builder,
                "LV3 measurement wait",
                RequireResult(results, PolicyKind.ForwardRiskEnergyTierLadderProbe),
                repeatabilityResults);
            AppendHighTierWaitAgencyRow(
                builder,
                "Promoted S1 LV3 diagnostic",
                RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute),
                repeatabilityResults);
            AppendHighTierWaitAgencyRow(
                builder,
                "Slot2 LV2 now",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults);
            AppendHighTierWaitAgencyRow(
                builder,
                "Slot2 full-bank",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults);
            AppendHighTierWaitAgencyRow(
                builder,
                "Slot3 delayed payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults);
            AppendHighTierWaitAgencyRow(
                builder,
                "Slot3 retreat/recommit payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute),
                repeatabilityResults);
        }

        private static void AppendHighTierWaitAgencyRow(
            StringBuilder builder,
            string waitRoute,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(waitRoute));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHighTierWaitAgencyTarget(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHighTierWaitAgencyExposure(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHighTierWaitAgencyPressureCost(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHighTierWaitAgencySignal(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHighTierWaitAgencyPayoff(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatHighTierWaitAgencyRepeat(repeatabilityResults, result.Policy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveHighTierWaitAgencyRead(result)));
            builder.AppendLine(" |");
        }

        private static string FormatHighTierWaitAgencyTarget(PolicyMetrics result)
        {
            if (!string.IsNullOrWhiteSpace(result.SupportSummonSlotId))
            {
                return FormatSupportWaitExposureTarget(result);
            }

            return result.EnergyProbeTargetTier > 0
                ? $"LV{result.EnergyProbeTargetTier}"
                : "energy measurement";
        }

        private static string FormatHighTierWaitAgencyExposure(PolicyMetrics result)
        {
            return "wait "
                + FormatSeconds(ResolveHighTierWaitAgencySeconds(result))
                + "; HP "
                + ResolveHighTierWaitAgencyDamage(result).ToString("0.#")
                + "; HP/s "
                + ResolveHighTierWaitAgencyDamagePerSecond(result).ToString("0.0");
        }

        private static string FormatHighTierWaitAgencyPressureCost(PolicyMetrics result)
        {
            if (!string.IsNullOrWhiteSpace(result.SupportSummonSlotId))
            {
                return "body "
                    + result.SupportBodyHitsBeforeSupport
                    + "; player hits "
                    + result.BossProjectilesHitPlayer;
            }

            return "player hits "
                + result.BossProjectilesHitPlayer
                + "; boss waves "
                + result.BossWaves;
        }

        private static string FormatHighTierWaitAgencySignal(PolicyMetrics result)
        {
            string forecast = ResolveCoverageValue(result.SupportChoiceForecastReadoutBeforeSupport);
            string energy =
                $"energy screen F/R/S {result.ForwardRiskEnergyScreenCueRequests}/{result.EnergyReadyScreenCueRequests}/{result.EnergySpendScreenCueRequests}";
            return string.IsNullOrWhiteSpace(result.SupportChoiceForecastReadoutBeforeSupport)
                ? energy
                : $"{forecast}; {energy}";
        }

        private static string FormatHighTierWaitAgencyPayoff(PolicyMetrics result)
        {
            return ResolveResultHookClass(result)
                + "; "
                + ResolveSupportDecisionPayoffVerdict(result)
                + "; boss "
                + result.BossDamageTaken.ToString("0.#");
        }

        private static string FormatHighTierWaitAgencyRepeat(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind policy)
        {
            if (CountPolicyResults(repeatabilityResults, policy) <= 0)
            {
                return "not repeated";
            }

            return "wait "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, ResolveHighTierWaitAgencySeconds),
                    AverageMetric(repeatabilityResults, policy, ResolveHighTierWaitAgencySeconds),
                    MaxMetric(repeatabilityResults, policy, ResolveHighTierWaitAgencySeconds))
                + "; HP "
                + FormatMinAverageMax(
                    MinMetric(repeatabilityResults, policy, ResolveHighTierWaitAgencyDamage),
                    AverageMetric(repeatabilityResults, policy, ResolveHighTierWaitAgencyDamage),
                    MaxMetric(repeatabilityResults, policy, ResolveHighTierWaitAgencyDamage));
        }

        private static float ResolveHighTierWaitAgencySeconds(PolicyMetrics result)
        {
            if (!string.IsNullOrWhiteSpace(result.SupportSummonSlotId))
            {
                float supportWait = ResolveSupportWaitExposureSeconds(result);
                if (supportWait >= 0f)
                {
                    return supportWait;
                }
            }

            float targetDuration = ResolveEnergyTargetDuration(result);
            if (targetDuration >= 0f)
            {
                return targetDuration;
            }

            return result.EnergyTier3DurationSeconds >= 0f
                ? result.EnergyTier3DurationSeconds
                : result.EnergyProbeElapsedSeconds;
        }

        private static float ResolveHighTierWaitAgencyDamage(PolicyMetrics result)
        {
            return !string.IsNullOrWhiteSpace(result.SupportSummonSlotId)
                && result.SupportDamageBeforeSupport >= 0f
                    ? result.SupportDamageBeforeSupport
                    : result.PlayerDamageTaken;
        }

        private static float ResolveHighTierWaitAgencyDamagePerSecond(PolicyMetrics result)
        {
            float waitSeconds = ResolveHighTierWaitAgencySeconds(result);
            return waitSeconds > 0f ? ResolveHighTierWaitAgencyDamage(result) / waitSeconds : 0f;
        }

        private static string ResolveHighTierWaitAgencyRead(PolicyMetrics result)
        {
            switch (result.Policy)
            {
                case PolicyKind.ForwardRiskEnergyTierLadderProbe:
                    return "measurement proves LV3 reachability but has no spend/payoff agency";
                case PolicyKind.ForwardRiskTier3DecisionRoute:
                    return "risk-position wait now diagnoses the promoted S1 follow-up gap; Slot3 owns the stable suppress payoff";
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return "LV2-now lowers wait exposure but accepts recovery burden";
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return "full-bank wait adds forecast and no-recovery tempo but still pays HP/body before support";
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return "full-bank wait adds role choice and suppress payoff; still needs more during-wait agency evidence";
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return "deep LV2 retreat lowers exposure, then forward recommit trades direct suppress for recovery";
                default:
                    return "not a high-tier wait route";
            }
        }

        private static void AppendStageResultMotivationMatrix(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            PolicyMetrics noSummonFail = RequireResult(results, PolicyKind.NoSummonSurvivalLimit);
            PolicyMetrics gunOnlyFail = RequireResult(results, PolicyKind.GunOnlySurvivalLimit);
            PolicyMetrics cleanPhysical = RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe);
            PolicyMetrics highTierSuppress = RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute);
            PolicyMetrics counterRecovery = RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery);
            PolicyMetrics marksmanClear = RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
            PolicyMetrics vanguardClear = RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route);

            builder.AppendLine("## Stage Result Motivation Matrix");
            builder.AppendLine("- NIKKE stage-result lens: route outcomes should produce distinct next-run motivation while staying review-only in this V1 slice.");
            builder.AppendLine("| Outcome | Policy | Hook class | Result token | Next state hook | Route label | Result copy | Next-run motivation | Overlay motivation | Boundary |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            AppendStageResultMotivationRow(
                builder,
                "Unanswered fail",
                noSummonFail,
                "failure names HP pressure and points back to summon protection");
            AppendStageResultMotivationRow(
                builder,
                "Gun-only fail",
                gunOnlyFail,
                "failure rejects boss-chip tunnel vision");
            AppendStageResultMotivationRow(
                builder,
                "Clean summon confirm",
                cleanPhysical,
                "clean clear reinforces block -> Skill1 before counter pressure");
            AppendStageResultMotivationRow(
                builder,
                "S1 LV3 diagnostic",
                highTierSuppress,
                "high-risk wait remains pending until the S1 follow-up closes");
            AppendStageResultMotivationRow(
                builder,
                "Counter recovery",
                counterRecovery,
                "recovery clear reinforces answering the counter earlier next time");
            AppendStageResultMotivationRow(
                builder,
                "Marksman support diagnostic",
                marksmanClear,
                "support route preserves Slot1 but still needs payoff closure");
            AppendStageResultMotivationRow(
                builder,
                "Vanguard support clear",
                vanguardClear,
                "support clear names Slot3 converting line hold into boss-screen break");
        }

        private static void AppendStageResultMotivationRow(
            StringBuilder builder,
            string outcome,
            PolicyMetrics result,
            string motivationRead)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(outcome));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.Policy.ToString()));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveResultHookClass(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveCoverageValue(result.ResultRecordTokenId)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveCoverageValue(result.ResultRecordNextStateHookId)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveCoverageValue(result.ResultRecordRouteLabel)));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveResultCopyReadout(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable($"{ResolveCoverageValue(result.ResultRecordNextObjective)} ({motivationRead})"));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveResultOverlayMotivationReadout(result)));
            builder.Append(" | ");
            builder.Append(IsReviewOnlyResultHook(result) ? "review-only analysis" : "invalid or missing");
            builder.AppendLine(" |");
        }

        private static string ResolveResultOverlayMotivationReadout(PolicyMetrics result)
        {
            if (result.ResultRecords <= 0)
            {
                return "pending";
            }

            return $"Reward: {ResolveCoverageValue(result.ResultOverlayRewardHook)}; Next: {ResolveCoverageValue(result.ResultOverlayNextObjective)}";
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
            builder.AppendLine("| Policy | CloseProbe | ScreenCurtain | SupportAnswer | Follow-up | CounterPressure | Result hook | First unresolved | Stage judgement |");
            builder.AppendLine("|---|---|---|---|---|---|---|---|---|");
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
                builder.Append(ResolveSupportAnswerBeat(result));
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

        private static void AppendEnergyTierDecisionPressure(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## EN Tier Decision Pressure");
            builder.AppendLine("| Policy | Target risk | Target tier | LV1 | LV2 | LV3 | Player down | HP lost | HP/s | Avg gain | Band seconds B/M/F | Read |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");
            AppendEnergyTierDecisionRow(builder, RequireResult(results, PolicyKind.BacklineEnergyProbe));
            AppendEnergyTierDecisionRow(builder, RequireResult(results, PolicyKind.ForwardRiskEnergyProbe));
            AppendEnergyTierDecisionRow(builder, RequireResult(results, PolicyKind.BacklineEnergyTierLadderProbe));
            AppendEnergyTierDecisionRow(builder, RequireResult(results, PolicyKind.ForwardRiskEnergyTierLadderProbe));
        }

        private static void AppendEnergyTierDecisionRow(StringBuilder builder, PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(FormatOptionalPercent01(result.EnergyProbeTargetForwardRisk01));
            builder.Append(" | ");
            builder.Append(FormatEnergyProbeTargetTier(result));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.EnergyTier1DurationSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.EnergyTier2DurationSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.EnergyTier3DurationSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.FirstPlayerDownAtSeconds));
            builder.Append(" | ");
            builder.Append(result.PlayerDamageTaken.ToString("0.0"));
            builder.Append(" | ");
            builder.Append(result.EnergyProbePlayerDamagePerSecond.ToString("0.0"));
            builder.Append(" | ");
            builder.Append($"x{result.AverageEnergyGainMultiplier:0.00}");
            builder.Append(" | ");
            builder.Append(
                $"{FormatSeconds(result.BackSafetyBandSeconds)}/"
                + $"{FormatSeconds(result.MidChargeBandSeconds)}/"
                + $"{FormatSeconds(result.ForwardRiskBandSeconds)}");
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveEnergyTierDecisionRead(result)));
            builder.AppendLine(" |");
        }

        private static string ResolveEnergyTierDecisionRead(PolicyMetrics result)
        {
            if (result.FirstPlayerDownAtSeconds >= 0f
                && result.EnergyTier3ReadyAtSeconds < 0f)
            {
                return "wait-for-LV3 failed before cap";
            }

            if (result.EnergyTier3ReadyAtSeconds >= 0f
                && result.EnergyProbePlayerDamagePerSecond >= 3f)
            {
                return "higher tier reachable with high HP/s pressure";
            }

            if (result.EnergyTier3ReadyAtSeconds >= 0f
                && result.PlayerDamageTaken > 0f)
            {
                return "higher tier reachable with HP cost";
            }

            if (result.EnergyTier3ReadyAtSeconds >= 0f)
            {
                return "higher tier reachable safely but slowly";
            }

            return "measurement only";
        }

        private static void AppendEnergySpendDecisionRoute(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## EN Spend Decision Route");
            builder.AppendLine("| Policy | Target tier | Ready at | HP lost | HP/s | Summon tier | Skill tier/hits | Window | Pulse | Suppress | Result | Read |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|---|");
            AppendEnergySpendDecisionRouteRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskTier1DecisionRoute));
            AppendEnergySpendDecisionRouteRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskTier2DecisionRoute));
            AppendEnergySpendDecisionRouteRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute));
        }

        private static void AppendEnergySpendDecisionRouteRow(StringBuilder builder, PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(FormatEnergyProbeTargetTier(result));
            builder.Append(" | ");
            builder.Append(FormatSeconds(ResolveEnergyTargetDuration(result)));
            builder.Append(" | ");
            builder.Append(result.PlayerDamageTaken.ToString("0.0"));
            builder.Append(" | ");
            builder.Append(result.EnergyProbePlayerDamagePerSecond.ToString("0.0"));
            builder.Append(" | ");
            builder.Append(result.HighestSummonSpentTier);
            builder.Append(" | ");
            builder.Append($"{result.HighestSkill1SpentTier}/{result.SkillProjectileHits}");
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.LastSummonFollowupWindowDuration));
            builder.Append(" | ");
            builder.Append(result.SummonFollowupEnergyPulse.ToString("0"));
            builder.Append(" | ");
            builder.Append($"{result.BossPressureScreensSuppressedByFollowup}/{result.HighestBossScreenSuppressSummonTier}");
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveEnergySpendDecisionRead(result)));
            builder.AppendLine(" |");
        }

        private static void AppendEnergySpendRecoveryRoute(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## EN Spend Recovery Route");
            builder.AppendLine("| Policy | Target tier | Ready at | HP lost | Summon tier | Summons | Skill tier/hits | Suppress | Counter->answer | Answer->stable | Final->hit | Boss dmg P/S | Result | Read |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|---|");
            AppendEnergySpendRecoveryRouteRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute));
            AppendEnergySpendRecoveryRouteRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskTier2RecoveryRoute));
            AppendEnergySpendRecoveryRouteRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskTier3RecoveryRoute));
        }

        private static void AppendEnergySpendRecoveryRouteRow(StringBuilder builder, PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(FormatEnergyProbeTargetTier(result));
            builder.Append(" | ");
            builder.Append(FormatSeconds(ResolveEnergyTargetDuration(result)));
            builder.Append(" | ");
            builder.Append(result.PlayerDamageTaken.ToString("0.0"));
            builder.Append(" | ");
            builder.Append(result.HighestSummonSpentTier);
            builder.Append(" | ");
            builder.Append(result.SummonUses);
            builder.Append(" | ");
            builder.Append($"{result.HighestSkill1SpentTier}/{result.SkillProjectileHits}");
            builder.Append(" | ");
            builder.Append($"{result.BossPressureScreensSuppressedByFollowup}/{result.HighestBossScreenSuppressSummonTier}");
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.CounterTriggerToAnswerSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.CounterAnswerToStableSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.FinalWindowToHitSeconds));
            builder.Append(" | ");
            builder.Append($"{result.BossDamageFromPlayer:0.0}/{result.BossDamageFromAllySummon:0.0}");
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveEnergySpendDecisionRead(result)));
            builder.AppendLine(" |");
        }

        private static void AppendSupportSummonRouteIdentity(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Support Summon Route Identity");
            builder.AppendLine("- ArkData lens: the same forward-risk pressure slot is answered by different roster rows, so cost and effect identity must show as route-outcome separation before UI polish.");
            builder.AppendLine("| Policy | Slot | Mana | Spent tier | Role | Volley waves | Blocks | Projectile hits B/S/Body | Physical hits/dmg | Boss dmg P/S | Ally clash/body | Actor max/HP | Result | Read |");
            builder.AppendLine("|---|---|---:|---:|---|---:|---:|---:|---:|---:|---:|---:|---|---|");
            AppendSupportSummonRouteIdentityRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot2MarksmanRoute));
            AppendSupportSummonRouteIdentityRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot3VanguardRoute));
        }

        private static void AppendSupportSummonRouteIdentityRow(StringBuilder builder, PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(result.SupportSummonSlotId));
            builder.Append(" | ");
            builder.Append(result.SupportSummonRequiredMana.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(result.SupportSummonSpentTier);
            builder.Append(" | ");
            builder.Append(EscapeTable(result.SupportSummonActorRoleId));
            builder.Append(" | ");
            builder.Append(result.SupportSummonVolleyWaves);
            builder.Append(" | ");
            builder.Append(result.SupportSummonBlocks);
            builder.Append(" | ");
            builder.Append(
                $"{result.SupportSummonProjectileBossHits}/{result.SupportSummonProjectileEnemySummonHits}/{result.SupportSummonProjectileEnemyBodyHits}");
            builder.Append(" | ");
            builder.Append($"{result.PhysicalBarragePlayerHits}/{result.PhysicalBarragePlayerDamage:0.0}");
            builder.Append(" | ");
            builder.Append($"{result.BossDamageFromPlayer:0.0}/{result.BossDamageFromAllySummon:0.0}");
            builder.Append(" | ");
            builder.Append($"{result.AllyFrontlineClashes}/{result.AllyFrontlineBodyHits}");
            builder.Append(" | ");
            builder.Append($"{result.SupportSummonMaxActiveActors}/{FormatPercent01(result.SupportSummonActorHealthRatio)}");
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSupportSummonRouteRead(result)));
            builder.AppendLine(" |");
        }

        private static string ResolveSupportSummonRouteRead(PolicyMetrics result)
        {
            if (result.Policy == PolicyKind.ForwardRiskSlot2MarksmanRoute)
            {
                if (result.SupportSummonBlocks > 0)
                {
                    return "marksman identity contaminated by screen block";
                }

                if (result.SupportSummonProjectileBossHits > 0)
                {
                    return "marksman contributes boss-lane fire but leaves physical pressure";
                }

                return result.SupportSummonProjectileEnemySummonHits > 0
                    ? "marksman suppresses enemy frontline but leaves physical pressure"
                    : "marksman did not produce target-confirmed pressure";
            }

            if (result.Policy == PolicyKind.ForwardRiskSlot3VanguardRoute)
            {
                return result.SupportSummonProjectileHits > 0
                    ? "dragon spends more mana for breath pressure but leaves physical risk"
                    : "dragon did not produce target-confirmed breath pressure";
            }

            return "not a support-summon route";
        }

        private static void AppendSharedManaSupportComboBranch(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Shared Mana Support Combo Branch");
            builder.AppendLine("- Blue Archive EX lens: lower-cost support should preserve shared resource for the next answer, while the high-cost support can consume the turn.");
            builder.AppendLine("| Policy | Support | Support cost | Mana after support | Mana before Slot1 | Slot1 cost | Slot1 use/block | Mana after Slot1 | Blocks | Skill1 hits | First unresolved | Result | Read |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---|---:|---:|---:|---|---|---|");
            AppendSharedManaSupportComboBranchRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute));
            AppendSharedManaSupportComboBranchRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute));
        }

        private static void AppendSharedManaSupportComboBranchRow(StringBuilder builder, PolicyMetrics result)
        {
            string slot1State = result.SupportComboSlot1Used
                ? "used"
                : result.SupportComboSlot1Attempted
                    ? $"blocked:{result.SupportComboSlot1BlockedReason}"
                    : "not attempted";

            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(result.SupportSummonSlotId));
            builder.Append(" | ");
            builder.Append(result.SupportSummonRequiredMana.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(result.SupportComboManaAfterSupport.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(result.SupportComboManaBeforeSlot1.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(result.SupportComboSlot1RequiredMana.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(EscapeTable(slot1State));
            builder.Append(" | ");
            builder.Append(result.SupportComboManaAfterSlot1.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(result.SummonBlocks);
            builder.Append(" | ");
            builder.Append(result.SkillProjectileHits);
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveFirstUnresolvedBeat(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSharedManaSupportComboRead(result)));
            builder.AppendLine(" |");
        }

        private static string ResolveSharedManaSupportComboRead(PolicyMetrics result)
        {
            if (result.Policy == PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute)
            {
                if (result.SupportComboSlot1Used && ResolveFirstUnresolvedBeat(result) == "Complete")
                {
                    return "Slot2 leaves enough shared mana for Slot1 to close the remaining curtain";
                }

                return result.SupportComboSlot1Used
                    ? "Slot2 preserved Slot1 spend but the follow-up chain did not close"
                    : "Slot2 did not preserve the expected Slot1 spend";
            }

            if (result.Policy == PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute)
            {
                if (result.SupportComboSlot1Attempted
                    && !result.SupportComboSlot1Used
                    && result.SupportComboManaAfterSupport + 0.001f < result.SupportComboSlot1RequiredMana)
                {
                    return "Slot3 spends the bank and blocks the immediate Slot1 answer";
                }

                return "Slot3 branch did not prove the expected bank-spend lockout";
            }

            return "not a shared-mana combo branch";
        }

        private static void AppendSharedManaDelayedMainAnswerBranch(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Shared Mana Delayed Main Answer Branch");
            builder.AppendLine("- Blue Archive EX lens: an early lower-cost support spend should show whether its tempo buys enough time to recharge into the next main answer.");
            builder.AppendLine("| Policy | Support | Target tier | Mana after support | Slot1 ready delay | HP before Slot1 | Mana before/after Slot1 | Slot1 use/block | Support hits/blocks | Physical hits | Skill1 hits | First unresolved | Result | Read |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---|---|");
            AppendSharedManaDelayedMainAnswerBranchRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route));
            AppendSharedManaDelayedMainAnswerBranchRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route));
        }

        private static void AppendSharedManaDelayedMainAnswerBranchRow(StringBuilder builder, PolicyMetrics result)
        {
            string slot1State = result.SupportComboSlot1Used
                ? "used"
                : result.SupportComboSlot1Attempted
                    ? $"blocked:{result.SupportComboSlot1BlockedReason}"
                    : "not attempted";

            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(result.SupportSummonSlotId));
            builder.Append(" | ");
            builder.Append(result.EnergyProbeTargetTier);
            builder.Append(" | ");
            builder.Append(result.SupportComboManaAfterSupport.ToString("0.#"));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.SupportComboSlot1ReadyDelaySeconds));
            builder.Append(" | ");
            builder.Append(result.SupportComboPlayerDamageBeforeSlot1.ToString("0.0"));
            builder.Append(" | ");
            builder.Append($"{result.SupportComboManaBeforeSlot1:0.#}/{result.SupportComboManaAfterSlot1:0.#}");
            builder.Append(" | ");
            builder.Append(EscapeTable(slot1State));
            builder.Append(" | ");
            builder.Append($"{result.SupportSummonProjectileEnemySummonHits}/{result.SupportSummonBlocks}");
            builder.Append(" | ");
            builder.Append($"{result.PhysicalBarragePlayerHits}/{result.PhysicalBarrageTrackedProjectileCount}");
            builder.Append(" | ");
            builder.Append(result.SkillProjectileHits);
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveFirstUnresolvedBeat(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSharedManaDelayedMainAnswerRead(result)));
            builder.AppendLine(" |");
        }

        private static string ResolveSharedManaDelayedMainAnswerRead(PolicyMetrics result)
        {
            if (result.Policy == PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route)
            {
                if (result.SupportComboSlot1Used && ResolveFirstUnresolvedBeat(result) == "Complete")
                {
                    return "Slot2 buys early support tempo, then recharges into Slot1 confirm";
                }

                return result.SupportComboSlot1Used
                    ? "Slot2 recharged into Slot1, then exposed the counter-answer recovery branch"
                    : "Slot2 early spend did not reach the delayed main answer";
            }

            if (result.Policy == PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route)
            {
                if (result.SupportComboSlot1Used && ResolveFirstUnresolvedBeat(result) == "Complete")
                {
                    return "Slot3 spends the bank, holds the line, then recharges into Slot1 confirm";
                }

                return result.SupportComboSlot1Used
                    ? "Slot3 recharged into Slot1, then exposed the counter-answer recovery branch"
                    : "Slot3 did not reopen Slot1 after the bank spend";
            }

            return "not a delayed main-answer branch";
        }

        private static void AppendSharedManaDelayedCounterRecoveryBranch(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            builder.AppendLine("## Shared Mana Delayed Counter Recovery Branch");
            builder.AppendLine("- PGR lens: a delayed main answer that is caught by boss-screen pressure should relock into a fresh summon answer, not silently fail.");
            builder.AppendLine("| Policy | Support | Target tier | Slot1 ready delay | HP before Slot1 | Counter->answer | Answer->stable | Final->hit | Skill1 hits | Boss suppress | Result records | First unresolved | Result | Read |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|---|---|");
            AppendSharedManaDelayedCounterRecoveryBranchRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute));
            AppendSharedManaDelayedCounterRecoveryBranchRow(
                builder,
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute));
        }

        private static void AppendSharedManaDelayedCounterRecoveryBranchRow(
            StringBuilder builder,
            PolicyMetrics result)
        {
            builder.Append("| ");
            builder.Append(result.Policy);
            builder.Append(" | ");
            builder.Append(EscapeTable(result.SupportSummonSlotId));
            builder.Append(" | ");
            builder.Append(result.EnergyProbeTargetTier);
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.SupportComboSlot1ReadyDelaySeconds));
            builder.Append(" | ");
            builder.Append(result.SupportComboPlayerDamageBeforeSlot1.ToString("0.0"));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.CounterTriggerToAnswerSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.CounterAnswerToStableSeconds));
            builder.Append(" | ");
            builder.Append(FormatSeconds(result.FinalWindowToHitSeconds));
            builder.Append(" | ");
            builder.Append(result.SkillProjectileHits);
            builder.Append(" | ");
            builder.Append($"{result.BossPressureScreensSuppressedByFollowup}/{result.HighestBossScreenSuppressSummonTier}");
            builder.Append(" | ");
            builder.Append(result.ResultRecords);
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveFirstUnresolvedBeat(result)));
            builder.Append(" | ");
            builder.Append(EscapeTable(result.ResultKind));
            builder.Append(" | ");
            builder.Append(EscapeTable(ResolveSharedManaDelayedCounterRecoveryRead(result)));
            builder.AppendLine(" |");
        }

        private static string ResolveSharedManaDelayedCounterRecoveryRead(PolicyMetrics result)
        {
            if (ResolveFirstUnresolvedBeat(result) == "Complete" && result.BossScreenSuppressedByFollowup)
            {
                return "vanguard assist lets the delayed main answer suppress boss screen directly";
            }

            if (ResolveFirstUnresolvedBeat(result) == "Complete" && result.CounterRecoveryConfirmed)
            {
                return "delayed branch relocks into counter recovery and commits the result";
            }

            if (ResolveFirstUnresolvedBeat(result) == "Complete")
            {
                return "delayed branch closes directly before counter recovery is needed";
            }

            return result.CounterWaves > 0
                ? "counter branch opened but did not commit"
                : "delayed branch did not reach the counter recovery check";
        }

        private static void AppendSummonRosterIdentityAudit(
            StringBuilder builder,
            IReadOnlyList<PolicyMetrics> results)
        {
            SummonRosterAuditRow[] rows = BuildSummonRosterAuditRows(results);
            builder.AppendLine("## Summon Roster Mana/Effect Identity Audit");
            builder.AppendLine("- ArkData lens: roster slots should preserve cost, role, target/effect, and stage-read differences instead of collapsing into one generic summon button.");
            builder.AppendLine($"- Cost verdict: {ResolveSummonRosterCostVerdict(rows)}");
            builder.AppendLine($"- Effect verdict: {ResolveSummonRosterEffectVerdict(rows)}");
            builder.AppendLine("| Slot | Action id | Cost source | Required tier | Required mana | Tier costs | Role ids | Volley dmg | Screen | Actor HP | Counter dmg | Read |");
            builder.AppendLine("|---|---|---|---:|---:|---:|---|---:|---:|---:|---:|---|");
            for (int i = 0; i < rows.Length; i++)
            {
                SummonRosterAuditRow row = rows[i];
                builder.Append("| ");
                builder.Append(EscapeTable(row.Slot));
                builder.Append(" | ");
                builder.Append(EscapeTable(row.ActionId));
                builder.Append(" | ");
                builder.Append(EscapeTable(row.CostSource));
                builder.Append(" | ");
                builder.Append("LV");
                builder.Append(row.MinimumTier);
                builder.Append(" | ");
                builder.Append(row.RequiredMana.ToString("0.#"));
                builder.Append(" | ");
                builder.Append(EscapeTable(FormatTierFloatReadout(row.TierCosts)));
                builder.Append(" | ");
                builder.Append(EscapeTable(FormatTierStringReadout(row.RoleIds)));
                builder.Append(" | ");
                builder.Append(EscapeTable(FormatTierFloatReadout(row.VolleyDamage)));
                builder.Append(" | ");
                builder.Append(EscapeTable(FormatTierIntReadout(row.ScreenIntercepts)));
                builder.Append(" | ");
                builder.Append(EscapeTable(FormatTierFloatReadout(row.ActorHealth)));
                builder.Append(" | ");
                builder.Append(EscapeTable(FormatTierFloatReadout(row.CounterDamage)));
                builder.Append(" | ");
                builder.Append(EscapeTable(row.Readout));
                builder.AppendLine(" |");
            }
        }

        private static float ResolveEnergyTargetDuration(PolicyMetrics result)
        {
            return result.EnergyProbeTargetTier switch
            {
                3 => result.EnergyTier3DurationSeconds,
                2 => result.EnergyTier2DurationSeconds,
                1 => result.EnergyTier1DurationSeconds,
                _ => -1f
            };
        }

        private static string ResolveEnergySpendDecisionRead(PolicyMetrics result)
        {
            if (result.EnergyProbeTargetTier <= 0)
            {
                return "not an EN spend-decision route";
            }

            if (result.HighestSummonSpentTier <= 0)
            {
                return "measurement only";
            }

            if (result.FirstPlayerDownAtSeconds >= 0f)
            {
                return "wait failed before answer";
            }

            if (result.ResultKind == "CounterRecoveryClear")
            {
                return "boss screen recovered after fresh answer";
            }

            if (result.ResultKind == "CleanFollowupClear" && result.BossScreenSuppressedByFollowup)
            {
                return "high-tier support suppresses boss screen; direct payoff";
            }

            if (result.ResultKind != "CleanFollowupClear")
            {
                if (result.BossBlockedSkill1Followup)
                {
                    return "boss screen blocks punish; recovery needed";
                }

                return $"answer incomplete at {ResolveFirstUnresolvedBeat(result)}";
            }

            if (result.EnergyProbeTargetTier >= 3
                && result.LastSummonFollowupWindowDuration > 0f
                && result.SummonFollowupEnergyPulse > 0f)
            {
                return "bigger summon window/pulse after longer risk";
            }

            if (result.EnergyProbeTargetTier >= 2)
            {
                return "higher spend clears after extra risk";
            }

            return "fastest clean answer";
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
            PolicyMetrics highTierSuppress = RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute);
            PolicyMetrics marksmanClear = RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
            PolicyMetrics vanguardClear =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
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
                && highTierSuppress.IsClearResult
                && marksmanClear.IsClearResult
                && vanguardClear.IsClearResult
                && physicalCloseChain.IsClearResult
                && blockedRecovery.ResultKind == "CounterRecoveryClear"
                && HasSingleReviewOnlyResultHook(noSummonSurvival)
                && HasSingleReviewOnlyResultHook(gunOnlySurvival)
                && HasSingleReviewOnlyResultHook(forwardRiskPhysicalSummonPunish)
                && HasSingleReviewOnlyResultHook(highTierSuppress)
                && HasSingleReviewOnlyResultHook(marksmanClear)
                && HasSingleReviewOnlyResultHook(vanguardClear)
                && HasSingleReviewOnlyResultHook(physicalCloseChain)
                && HasSingleReviewOnlyResultHook(blockedRecovery)
                && IsStageResultCopy(noSummonSurvival)
                && IsStageResultCopy(gunOnlySurvival)
                && IsStageResultCopy(forwardRiskPhysicalSummonPunish)
                && IsStageResultCopy(highTierSuppress)
                && IsStageResultCopy(marksmanClear)
                && IsStageResultCopy(vanguardClear)
                && IsStageResultCopy(physicalCloseChain)
                && IsStageResultCopy(blockedRecovery)
                && !string.IsNullOrWhiteSpace(noSummonSurvival.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(gunOnlySurvival.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(forwardRiskPhysicalSummonPunish.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(highTierSuppress.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(marksmanClear.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(vanguardClear.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(physicalCloseChain.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(blockedRecovery.ResultRecordTokenId)
                && !string.IsNullOrWhiteSpace(noSummonSurvival.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(gunOnlySurvival.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(forwardRiskPhysicalSummonPunish.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(highTierSuppress.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(marksmanClear.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(vanguardClear.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(physicalCloseChain.ResultRecordNextStateHookId)
                && !string.IsNullOrWhiteSpace(blockedRecovery.ResultRecordNextStateHookId);
            bool pressureSlotMeasured = forwardRiskEnergy.EnergyTier1DurationSeconds >= 0f
                && backlineEnergy.EnergyProbeElapsedSeconds > 0f
                && forwardRiskEnergy.AverageEnergyGainMultiplier > backlineEnergy.AverageEnergyGainMultiplier
                && forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests > 0
                && forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests > 0
                && forwardRiskEnergy.EnergyReadyScreenCueRequests > 0
                && forwardRiskEnergy.EnergyReadyVfxCueRequests > 0
                && forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits > backlinePhysicalBarrage.PhysicalBarragePlayerHits
                && ignoredRecovery.PressureBurdenSeconds > intended.PressureBurdenSeconds;
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
                + $"bad routes commit fail hooks at {FormatSeconds(noSummonSurvival.FirstPlayerDownAtSeconds)} / {FormatSeconds(gunOnlySurvival.FirstPlayerDownAtSeconds)} with copy `{noSummonSurvival.ResultRecordTitle}`/`{gunOnlySurvival.ResultRecordTitle}` and token `{noSummonSurvival.ResultRecordTokenId}->{noSummonSurvival.ResultRecordNextStateHookId}`; clean physical `{ResolveResultHookClass(forwardRiskPhysicalSummonPunish)}` token `{forwardRiskPhysicalSummonPunish.ResultRecordTokenId}->{forwardRiskPhysicalSummonPunish.ResultRecordNextStateHookId}`; S1 LV3 diagnostic `{ResolveResultHookClass(highTierSuppress)}` token `{highTierSuppress.ResultRecordTokenId}->{highTierSuppress.ResultRecordNextStateHookId}`; support marksman `support_marksman_clear` actual `{ResolveResultHookClass(marksmanClear)}` token `{marksmanClear.ResultRecordTokenId}->{marksmanClear.ResultRecordNextStateHookId}`; support vanguard `support_vanguard_clear` actual `{ResolveResultHookClass(vanguardClear)}` token `{vanguardClear.ResultRecordTokenId}->{vanguardClear.ResultRecordNextStateHookId}`; live close-chain `{ResolveResultHookClass(physicalCloseChain)}`; boss-screen recovery `{ResolveResultHookClass(blockedRecovery)}` token `{blockedRecovery.ResultRecordTokenId}->{blockedRecovery.ResultRecordNextStateHookId}` | "
                + "Reward/item persistence and campaign clear are intentionally not implemented in this V1 combat slice. |");
            builder.AppendLine(
                "| Stage pressure-slot discipline | "
                + $"{FormatCoverageStatus(pressureSlotMeasured)} | "
                + $"forward-risk LV1 {FormatSeconds(forwardRiskEnergy.EnergyTier1DurationSeconds)} vs backline probe {FormatSeconds(backlineEnergy.EnergyProbeElapsedSeconds)}; energy cues screen/VFX F/R/S {forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests}/{forwardRiskEnergy.EnergyReadyScreenCueRequests}/{forwardRiskEnergy.EnergySpendScreenCueRequests} and {forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests}/{forwardRiskEnergy.EnergyReadyVfxCueRequests}/{forwardRiskEnergy.EnergySpendVfxCueRequests}; physical barrage hits {backlinePhysicalBarrage.PhysicalBarragePlayerHits}->{forwardRiskPhysicalBarrage.PhysicalBarragePlayerHits}; ignored burden {FormatSeconds(ignoredRecovery.PressureBurdenSeconds)} vs intended {FormatSeconds(intended.PressureBurdenSeconds)} | "
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
            PolicyMetrics marksman = RequireResult(results, PolicyKind.ForwardRiskSlot2MarksmanRoute);
            PolicyMetrics vanguard = RequireResult(results, PolicyKind.ForwardRiskSlot3VanguardRoute);
            PolicyMetrics slot2Combo = RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute);
            PolicyMetrics slot3Blocked = RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute);
            PolicyMetrics slot2Delayed = RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route);
            PolicyMetrics slot3Delayed = RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route);
            PolicyMetrics slot2DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute);
            PolicyMetrics slot3DelayedRecovery =
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute);
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
            Assert.That(
                ResolveSupportAnswerBeat(marksman),
                Is.EqualTo("MISS").Or.EqualTo("SUPPRESS_ENEMY_FRONT"),
                "Standalone Slot2 should stay a diagnostic partial support route until its hostile-frontline hit timing is stable.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(marksman),
                "Slot2 marksman should remain a partial support answer, not a main screen-curtain solution.");
            Assert.AreEqual(
                "DRAGON_BREATH",
                ResolveSupportAnswerBeat(vanguard),
                "Slot3 should classify as a FireDragon breath support answer.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(vanguard),
                "Slot3 vanguard should remain a partial support answer until the main boss curtain is solved.");
            Assert.That(
                ResolveFirstUnresolvedBeat(slot2Combo),
                Is.EqualTo("FollowupConfirm").Or.EqualTo("CounterAnswer"),
                "Slot2 preserves enough shared mana for Slot1 but now stalls before a committed follow-up result.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(slot3Blocked),
                "Slot3 should spend the shared mana bank and leave the immediate Slot1 screen-curtain answer locked out.");
            Assert.That(
                ResolveFirstUnresolvedBeat(slot2Delayed),
                Is.EqualTo("FollowupConfirm").Or.EqualTo("CounterAnswer"),
                "Slot2 spent at LV2 should recharge into Slot1, then expose the unclosed follow-up/counter branch.");
            Assert.That(
                ResolveFirstUnresolvedBeat(slot3Delayed),
                Is.EqualTo("Complete").Or.EqualTo("CounterAnswer"),
                "Slot3 spent at LV3 should either close through its high-cost hold or expose the same counter answer.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(slot2DelayedRecovery),
                "Slot2 delayed counter-recovery should close the branch after the fresh answer.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(slot3DelayedRecovery),
                "Slot3 delayed counter-recovery should close directly or after the fresh answer.");
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
                case PolicyKind.ForwardRiskTier1DecisionRoute:
                case PolicyKind.ForwardRiskTier2DecisionRoute:
                case PolicyKind.ForwardRiskTier3DecisionRoute:
                case PolicyKind.ForwardRiskTier1RecoveryRoute:
                case PolicyKind.ForwardRiskTier2RecoveryRoute:
                case PolicyKind.ForwardRiskTier3RecoveryRoute:
                case PolicyKind.ForwardRiskSlot2MarksmanRoute:
                case PolicyKind.ForwardRiskSlot3VanguardRoute:
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                case PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route:
                case PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route:
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
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

            return result.CloseThreatBasicHits > 0
                || result.CloseThreatPhysicalProjectileHits > 0
                || result.CloseThreatHealthRemaining <= 0.01f
                ? "PASS"
                : "MISS";
        }

        private static string ResolveScreenCurtainBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            if ((result.SummonBlocks > 0 && result.FollowupWindowOpenCount > 0)
                || (result.BossScreenSuppressedByFollowup
                    && result.BossPressureScreensSuppressedByFollowup > 0))
            {
                return "PASS";
            }

            return result.ResultKind == "PlayerDownFail" ? "FAILED" : "PENDING";
        }

        private static string ResolveSupportAnswerBeat(PolicyMetrics result)
        {
            if (!IsStageRoutePolicy(result.Policy))
            {
                return "N/A";
            }

            if (result.SupportSummonSlotId == "SummonSlot2")
            {
                if (result.SupportSummonProjectileEnemySummonHits > 0)
                {
                    return "SUPPRESS_ENEMY_FRONT";
                }

                if (result.Policy == PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute
                    && result.SupportComboSlot1Used)
                {
                    return "RESOURCE_BRANCH";
                }

                return result.SupportSummonProjectileBossHits > 0
                    ? "SUPPORT_FIRE"
                    : result.SupportSummonProjectileHits > 0
                        ? "PARTIAL_FIRE"
                        : "MISS";
            }

            if (result.SupportSummonSlotId == "SummonSlot3")
            {
                if (result.Policy == PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute
                    && result.SupportComboSlot1Attempted
                    && !result.SupportComboSlot1Used)
                {
                    return "BANK_SPENT";
                }

                if (result.SupportSummonProjectileBossHits + result.SupportSummonProjectileEnemySummonHits > 0)
                {
                    return "DRAGON_BREATH";
                }

                return result.SupportSummonProjectileHits > 0 ? "PARTIAL_BREATH" : "MISS";
            }

            return "-";
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
                + $"support={ResolveSupportAnswerBeat(result)}; "
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
                    if (result.SupportSummonSlotId == "SummonSlot2")
                    {
                        return "support_marksman_clear";
                    }

                    if (result.SupportSummonSlotId == "SummonSlot3")
                    {
                        return "support_vanguard_clear";
                    }

                    if (result.ResultRecordTokenId == "review.clear.lv3_suppress")
                    {
                        return "high_tier_suppress_clear";
                    }

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

        private static void AssertSummonRosterIdentityAudit(IReadOnlyList<PolicyMetrics> results)
        {
            SummonRosterAuditRow[] rows = BuildSummonRosterAuditRows(results);
            Assert.AreEqual(3, rows.Length, "The roster audit should cover all three summon slots.");
            Assert.That(
                ResolveSummonRosterCostVerdict(rows),
                Does.Contain("PASS"),
                "The roster audit should prove slot-specific summon mana cost gates before UI readiness polish.");
            Assert.That(
                ResolveSummonRosterEffectVerdict(rows),
                Does.Contain("PASS"),
                "The current roster audit should prove profile effect budgets are not identical.");

            Assert.AreEqual(2, rows[0].MinimumTier, "SummonSlot1 should read as the saved-EN charge answer.");
            Assert.AreEqual(1, rows[1].MinimumTier, "SummonSlot2 should stay the low-cost laser poke.");
            Assert.AreEqual(3, rows[2].MinimumTier, "SummonSlot3 should require the LV3 vanguard mana gate.");
            Assert.AreEqual(200f, rows[0].RequiredMana, 0.001f);
            Assert.AreEqual(100f, rows[1].RequiredMana, 0.001f);
            Assert.AreEqual(300f, rows[2].RequiredMana, 0.001f);
            Assert.Less(rows[1].RequiredMana, rows[0].RequiredMana);
            Assert.Less(rows[0].RequiredMana, rows[2].RequiredMana);

            for (int tierIndex = 0; tierIndex < 3; tierIndex++)
            {
                Assert.Greater(
                    rows[1].ProjectileSpeed[tierIndex],
                    rows[0].ProjectileSpeed[tierIndex],
                    $"SummonSlot2 tier {tierIndex + 1} should preserve the fast laser pressure budget.");
                Assert.Greater(
                    rows[2].VolleyDamage[tierIndex],
                    rows[0].VolleyDamage[tierIndex],
                    $"SummonSlot3 tier {tierIndex + 1} should preserve the high-cost dragon breath burst budget.");
                Assert.Greater(
                    rows[2].VolleyDamage[tierIndex],
                    rows[1].VolleyDamage[tierIndex],
                    $"SummonSlot3 tier {tierIndex + 1} should out-damage the cheaper laser soldier.");
                Assert.Greater(
                    rows[0].ScreenIntercepts[tierIndex],
                    0,
                    $"SummonSlot1 tier {tierIndex + 1} should keep the emergency pressure-screen identity.");
                Assert.AreEqual(
                    0,
                    rows[1].ScreenIntercepts[tierIndex],
                    $"SummonSlot2 tier {tierIndex + 1} should not hide a shield-screen identity.");
                Assert.AreEqual(
                    0,
                    rows[2].ScreenIntercepts[tierIndex],
                    $"SummonSlot3 tier {tierIndex + 1} should stay a dragon fire payoff, not a hidden shield-screen route.");
                Assert.Greater(
                    rows[2].ActorHealth[tierIndex],
                    rows[0].ActorHealth[tierIndex],
                    $"SummonSlot3 tier {tierIndex + 1} should preserve the large dragon body budget.");
                Assert.Greater(
                    rows[0].ActorHealth[tierIndex],
                    rows[1].ActorHealth[tierIndex],
                    $"SummonSlot1 tier {tierIndex + 1} should remain tougher than the marksman.");
            }
        }

        private static void AssertSupportSummonRouteIdentity(
            PolicyMetrics marksman,
            PolicyMetrics vanguard)
        {
            Assert.AreEqual("SummonSlot2", marksman.SupportSummonSlotId);
            Assert.AreEqual("SummonSlot3", vanguard.SupportSummonSlotId);
            Assert.AreEqual(100f, marksman.SupportSummonRequiredMana, 0.001f);
            Assert.AreEqual(300f, vanguard.SupportSummonRequiredMana, 0.001f);
            Assert.AreEqual(1, marksman.SupportSummonSpentTier);
            Assert.AreEqual(3, vanguard.SupportSummonSpentTier);
            Assert.AreEqual("LaserSoldier", marksman.SupportSummonActorRoleId);
            Assert.AreEqual("FireDragon", vanguard.SupportSummonActorRoleId);
            Assert.Greater(marksman.SupportSummonVolleyWaves, 0);
            Assert.Greater(vanguard.SupportSummonVolleyWaves, 0);
            Assert.AreEqual(
                0,
                marksman.SupportSummonBlocks,
                "Slot2 should remain a marksman route without secretly gaining a pressure screen.");
            Assert.AreEqual(
                0,
                vanguard.SupportSummonBlocks,
                "Slot3 should stay a FireDragon damage route without secretly gaining a pressure screen.");
            Assert.GreaterOrEqual(
                marksman.SupportSummonProjectileHits,
                0,
                "Slot2 should record the standalone marksman projectile hit count for diagnostics.");
            Assert.That(
                ResolveSupportAnswerBeat(marksman),
                Is.EqualTo("MISS").Or.EqualTo("SUPPRESS_ENEMY_FRONT"),
                "Slot2 standalone marksman fire is currently diagnostic and may miss or suppress hostile frontline summons.");
            Assert.Greater(
                vanguard.SupportSummonProjectileHits,
                0,
                "Slot3 should prove the FireDragon route contributes target-confirmed breath fire.");
            Assert.Greater(
                vanguard.SupportSummonProjectileBossHits + vanguard.SupportSummonProjectileEnemySummonHits,
                0,
                "Slot3 FireDragon breath should hit the boss lane or suppress hostile frontline summons.");
        }

        private static void AssertEnemyPressureTacticalCost(
            PolicyMetrics noSummon,
            PolicyMetrics noPunish,
            PolicyMetrics ignoredRecovery,
            PolicyMetrics blockedRecovery,
            PolicyMetrics physicalPunish)
        {
            Assert.Greater(
                noSummon.EnemyFrontlineBodyHits,
                0,
                "No-action should prove unattended enemy pressure reaches the player body instead of staying timer-only.");
            Assert.Greater(
                noPunish.EnemyFrontlineSummonHits,
                0,
                "A summon block without Skill1 punish should keep enemy pressure alive through summon clashes.");
            Assert.Greater(
                noPunish.CounterWaves,
                0,
                "Skipping the punish should relock into counter pressure even when player HP stays protected.");
            Assert.Greater(
                ignoredRecovery.EnemyFrontlineBodyHits,
                blockedRecovery.EnemyFrontlineBodyHits,
                "Ignoring boss-screen counter pressure should leave more enemy body cost than answering recovery.");
            Assert.Greater(
                ignoredRecovery.PlayerDamageTaken,
                blockedRecovery.PlayerDamageTaken,
                "Ignoring boss-screen counter pressure should cost HP while recovery protects HP.");
            Assert.Less(
                blockedRecovery.EnemyFrontlineBodyHits,
                ignoredRecovery.EnemyFrontlineBodyHits,
                "Counter recovery should reduce enemy frontline body hits compared with ignoring the counter.");
            Assert.Greater(
                blockedRecovery.EnemyFrontlineSummonHits,
                0,
                "Counter recovery should redirect enemy pressure into summon clashes, not delete the actor cost silently.");
            Assert.AreEqual(
                0,
                physicalPunish.EnemyFrontlineBodyHits,
                "Clean physical punish should prevent unattended enemy frontline body cost.");
            Assert.AreEqual(
                0f,
                physicalPunish.PlayerDamageTaken,
                0.001f,
                "Clean physical punish should preserve HP while closing the pressure route.");
        }

        private static void AssertCombatDecisionSignalMatrix(
            PolicyMetrics forwardRiskEnergy,
            PolicyMetrics noSummon,
            PolicyMetrics block,
            PolicyMetrics noPunish,
            PolicyMetrics recovery,
            PolicyMetrics cleanPunish)
        {
            Assert.Greater(
                forwardRiskEnergy.ForwardRiskEnergyScreenCueRequests,
                0,
                "Forward-risk EN should expose the risk-band screen signal before UI/coaster polish.");
            Assert.Greater(
                forwardRiskEnergy.EnergyReadyScreenCueRequests,
                0,
                "Forward-risk EN should expose a ready screen signal before UI/coaster polish.");
            Assert.Greater(
                forwardRiskEnergy.ForwardRiskEnergyVfxCueRequests,
                0,
                "Forward-risk EN should expose the risk-band VFX signal before UI/coaster polish.");
            Assert.Greater(
                forwardRiskEnergy.EnergyReadyVfxCueRequests,
                0,
                "Forward-risk EN should expose the ready VFX signal before UI/coaster polish.");
            Assert.Greater(
                noSummon.EnemyFrontlineBodyHits,
                0,
                "Ignoring pressure should expose body-hit cost as the decision signal.");
            Assert.Greater(
                noSummon.PlayerDamageScreenCueRequests,
                0,
                "Ignoring pressure should expose player damage screen cues as the post-action readout.");
            Assert.GreaterOrEqual(
                block.SummonPressureBlockCameraCueRequests,
                block.SummonBlocks,
                "A summon block decision should expose camera cues for the intercepted pressure.");
            Assert.GreaterOrEqual(
                block.SummonPressureScreenInterceptFlashes,
                block.SummonBlocks,
                "A summon block decision should expose pressure-screen intercept flashes.");
            Assert.GreaterOrEqual(
                block.SummonPressureScreenInterceptVfxCueRequests,
                block.SummonBlocks,
                "A summon block decision should expose intercept VFX cues.");
            Assert.Greater(
                block.FollowupWindowScreenCueRequests,
                0,
                "A summon block decision should expose the follow-up window screen cue.");
            Assert.Greater(
                block.FollowupWindowCameraCueRequests,
                0,
                "A summon block decision should expose the follow-up window camera cue.");
            Assert.Greater(
                block.FollowupWindowVfxCueRequests,
                0,
                "A summon block decision should expose the follow-up window VFX cue.");
            Assert.Greater(
                noPunish.FollowupMissedScreenCueRequests,
                0,
                "Skipping Skill1 should expose a missed follow-up screen cue.");
            Assert.Greater(
                noPunish.CounterWaveScreenCueRequests,
                0,
                "Skipping Skill1 should expose the counter-wave screen cue.");
            Assert.Greater(
                noPunish.EnemyFrontlineSummonHits,
                cleanPunish.EnemyFrontlineSummonHits,
                "Skipping Skill1 should leave enemy pressure alive through summon clashes compared with the clean punish route.");
            Assert.Greater(
                noPunish.UnansweredPressureBurdenShare01,
                cleanPunish.UnansweredPressureBurdenShare01,
                "Skipping Skill1 should preserve unanswered pressure burden even when player body hits stay protected.");
            Assert.Greater(
                recovery.CounterWaveAnswerScreenCueRequests,
                0,
                "Counter recovery should expose the fresh-answer screen cue.");
            Assert.Greater(
                recovery.CounterWaveStabilizedCameraCueRequests,
                0,
                "Counter recovery should expose the stabilized camera cue.");
            Assert.Greater(
                recovery.CounterWaveStabilizedVfxCueRequests,
                0,
                "Counter recovery should expose the stabilized VFX cue.");
            Assert.Greater(
                recovery.EnergyReadyScreenCueRequests,
                0,
                "Counter recovery should show the answer-ready energy signal.");
            Assert.Greater(
                recovery.EnergySpendScreenCueRequests,
                0,
                "Counter recovery should show the answer-spend energy signal.");
            Assert.Greater(
                recovery.FollowupHitScreenCueRequests,
                0,
                "Counter recovery should close through a follow-up hit cue, not only a result record.");
            Assert.Greater(
                cleanPunish.FollowupHitScreenCueRequests,
                0,
                "Clean punish should expose the follow-up hit screen cue.");
            Assert.Greater(
                cleanPunish.FollowupHitCameraCueRequests,
                0,
                "Clean punish should expose the follow-up hit camera cue.");
            Assert.Greater(
                cleanPunish.FollowupHitVfxCueRequests,
                0,
                "Clean punish should expose the follow-up hit VFX cue.");
            Assert.AreEqual(
                0,
                cleanPunish.PlayerDamageScreenCueRequests,
                "Clean punish should not leak player-damage presentation into the payoff signal.");
            Assert.AreEqual(
                "CleanFollowupClear",
                cleanPunish.ResultKind,
                "Clean punish should end as the clean follow-up clear when the decision signals line up.");
            Assert.AreEqual(
                "Running/ENReady",
                ResolveCombatDecisionSignalState(forwardRiskEnergy),
                "Forward-risk EN should read as a resource-ready decision state, not only a running probe.");
            Assert.AreEqual(
                "Running/BodyPressure",
                ResolveCombatDecisionSignalState(noSummon),
                "Ignoring pressure should read as body pressure at the decision layer.");
            Assert.AreEqual(
                "Running/FollowupReady",
                ResolveCombatDecisionSignalState(block),
                "Summon block should read as follow-up-ready at the decision layer.");
            Assert.AreEqual(
                "Running/CounterAnswer",
                ResolveCombatDecisionSignalState(noPunish),
                "Skipping Skill1 should read as counter-answer needed once the miss has relocked the route.");
            Assert.AreEqual(
                "CounterRecoveryClear/CounterRecovered",
                ResolveCombatDecisionSignalState(recovery),
                "Counter recovery should read as a recovered counter-answer state.");
            Assert.AreEqual(
                "CleanFollowupClear/Complete",
                ResolveCombatDecisionSignalState(cleanPunish),
                "Clean punish should read as complete at the decision layer.");
        }

        private static void AssertPhysicalPressureConversionRepeatability(
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.BacklinePhysicalBarrageProbe),
                "Physical conversion repeatability should include the backline safety reference.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.BacklinePhysicalBarrageProbe, result => result.PhysicalBarragePlayerHits),
                0f,
                "Repeated backline physical samples should keep projectile hits off the player.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.BacklinePhysicalBarrageProbe, result => result.PhysicalBarragePlayerDamage),
                0.01f,
                "Repeated backline physical samples should preserve the safe-lane damage floor.");

            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.ForwardRiskPhysicalBarrageProbe),
                "Physical conversion repeatability should include the forward danger reference.");
            Assert.GreaterOrEqual(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalBarrageProbe, result => result.PhysicalBarragePlayerHits),
                3f,
                "Repeated forward physical samples should keep a real danger floor.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalBarrageProbe, result => result.PhysicalBarragePlayerDamage),
                0f,
                "Repeated forward physical samples should convert the preview slot into HP cost.");

            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe),
                "Physical conversion repeatability should include the summon block branch.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe, result => result.SummonBlocks),
                0f,
                "Repeated summon block samples should intercept physical pressure.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe, result => result.PhysicalBarragePlayerHits),
                0f,
                "Repeated summon block samples should convert physical hits away from the player.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe, result => result.PhysicalBarragePlayerDamage),
                0.01f,
                "Repeated summon block samples should remove physical HP leak before the opened state.");

            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe),
                "Physical conversion repeatability should include the unconfirmed-window branch.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe, result => result.FollowupMissCount),
                0f,
                "Repeated unconfirmed-window samples should miss the follow-up state.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe, result => result.CounterWaves),
                0f,
                "Repeated unconfirmed-window samples should relock into counter pressure.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe, result => result.IsClearResult ? 1f : 0f),
                0f,
                "Repeated unconfirmed-window samples should not fabricate a clear result.");

            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonPunishProbe),
                "Physical conversion repeatability should include the Skill1 confirm branch.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonPunishProbe, result => result.SkillProjectileHits),
                0f,
                "Repeated Skill1 confirm samples should land the player-authored punish.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonPunishProbe, result => result.PlayerDamageTaken),
                0.01f,
                "Repeated Skill1 confirm samples should keep player HP clean.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonPunishProbe, result => result.IsClearResult ? 1f : 0f),
                0f,
                "Repeated Skill1 confirm samples should complete the clean physical route.");
        }

        private static void AssertCombatDecisionSignalRepeatability(
            IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.ForwardRiskEnergyProbe),
                "Decision-signal repeatability should include the forward-risk EN decision.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskEnergyProbe, result => result.EnergyTier1DurationSeconds),
                0f,
                "Repeated forward-risk EN samples should all reach LV1 readiness.");
            Assert.LessOrEqual(
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskEnergyProbe, result => result.EnergyTier1DurationSeconds),
                EnergyProbeMaxSeconds,
                "Repeated forward-risk EN samples should reach LV1 inside the probe window.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskEnergyProbe, result => result.ForwardRiskEnergyScreenCueRequests),
                0f,
                "Repeated forward-risk EN samples should all expose the risk-band cue.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskEnergyProbe, result => result.EnergyReadyScreenCueRequests),
                0f,
                "Repeated forward-risk EN samples should all expose the ready cue.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskEnergyProbe, result => result.ForwardRiskBandSeconds),
                0f,
                "Repeated forward-risk EN samples should all remain in the risk band while measuring resource readiness.");
            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.NoSummonNoFire),
                "Decision-signal repeatability should include the ignore-pressure decision.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.NoSummonNoFire, result => result.EnemyFrontlineBodyHits),
                0f,
                "Repeated ignore-pressure samples should all expose enemy body-hit cost.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.NoSummonNoFire, result => result.PlayerDamageTaken),
                0f,
                "Repeated ignore-pressure samples should all cost player HP.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.NoSummonNoFire, result => result.PlayerDamageScreenCueRequests),
                0f,
                "Repeated ignore-pressure samples should all expose player damage cues.");
            Assert.AreEqual(
                RepeatabilityProbeRuns,
                CountPolicyResults(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe),
                "Decision-signal repeatability should include the summon block route.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe, result => result.SummonBlocks),
                0f,
                "Repeated summon block samples should all produce real block signals.");
            Assert.AreEqual(
                0f,
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonBlockProbe, result => result.PhysicalBarragePlayerHits),
                "Repeated summon block samples should keep physical barrage hits off the player.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe, result => result.FollowupMissCount),
                0f,
                "Repeated no-punish samples should all miss the follow-up window.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonNoPunishProbe, result => result.CounterWaves),
                0f,
                "Repeated no-punish samples should all enter counter pressure.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.BossScreenBlockCounterRecovery, result => result.CounterWaveAnswerScreenCueRequests),
                0f,
                "Repeated counter-recovery samples should all show the answer cue.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.BossScreenBlockCounterRecovery, result => result.SkillProjectileHits),
                0f,
                "Repeated counter-recovery samples should all close with Skill1 hits.");
            Assert.Greater(
                MinMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonPunishProbe, result => result.SkillProjectileHits),
                0f,
                "Repeated clean punish samples should all land Skill1.");
            Assert.AreEqual(
                0f,
                MaxMetric(repeatabilityResults, PolicyKind.ForwardRiskPhysicalSummonPunishProbe, result => result.PlayerDamageTaken),
                "Repeated clean punish samples should keep player HP clean.");
        }

        private static void AssertHighTierWaitAgencyMatrix(
            PolicyMetrics directTier3,
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3DelayedRecovery,
            PolicyMetrics slot3RetreatRecovery)
        {
            Assert.Greater(
                ResolveHighTierWaitAgencySeconds(directTier3),
                0f,
                "The promoted S1 LV3 diagnostic route should expose a measurable wait before spend.");
            Assert.Greater(
                ResolveHighTierWaitAgencyDamage(directTier3),
                0f,
                "The promoted S1 LV3 diagnostic route should keep the HP tax visible beside the pending payoff.");
            Assert.That(
                ResolveHighTierWaitAgencyRead(directTier3),
                Does.Contain("risk-position"),
                "The promoted S1 LV3 diagnostic route should read as risk-positioning before spend, not as a free upgrade.");
            Assert.That(
                slot2DelayedRecovery.SupportChoiceForecastReadoutBeforeSupport,
                Does.Contain("S2 ready now").And.Contain("hold 300 EN"),
                "The LV2-now route should expose the live decision to stop waiting or hold for LV3.");
            Assert.That(
                slot2Combo.SupportChoiceForecastReadoutBeforeSupport,
                Does.Contain("S2 ready for tempo").And.Contain("S3 ready for suppress"),
                "The full-bank Slot2 route should keep both role choices visible at spend time.");
            Assert.That(
                slot3DelayedRecovery.SupportChoiceForecastReadoutBeforeSupport,
                Does.Contain("S3 ready for suppress").And.Contain("Slot1 recharge"),
                "The Slot3 delayed payoff should keep the suppress role and delayed main-answer cost visible.");
            Assert.Greater(
                ResolveHighTierWaitAgencyDamage(slot3DelayedRecovery),
                ResolveHighTierWaitAgencyDamage(slot2DelayedRecovery),
                "Holding for Slot3 should preserve the extra HP exposure compared with the LV2-now branch.");
            Assert.That(
                ResolveHighTierWaitAgencyRead(slot3DelayedRecovery),
                Does.Contain("during-wait agency"),
                "The high-tier agency matrix should keep the current gap visible instead of declaring the wait solved.");
            Assert.Greater(
                slot3RetreatRecovery.BackSafetyBandSeconds,
                slot3DelayedRecovery.BackSafetyBandSeconds + 1f,
                "The deep LV2-retreat route should prove the player moved the LV3 wait into a safer band.");
            Assert.Less(
                slot3RetreatRecovery.ForwardRiskBandSeconds,
                slot3DelayedRecovery.ForwardRiskBandSeconds,
                "The deep LV2-retreat route should reduce time spent in the forward-risk band.");
            Assert.Greater(
                ResolveHighTierWaitAgencySeconds(slot3RetreatRecovery),
                ResolveHighTierWaitAgencySeconds(slot3DelayedRecovery),
                "Deep LV2 retreat should preserve the longer-wait tradeoff instead of becoming a free upgrade.");
            Assert.That(
                slot3RetreatRecovery.CounterRecoveryConfirmed || slot3RetreatRecovery.ResultKind == "PlayerDownFail",
                "The deep LV2-retreat Slot3 branch should either prove recovery or remain an explicit failed retreat diagnostic.");
            Assert.IsFalse(
                slot3RetreatRecovery.BossScreenSuppressedByFollowup,
                "The deep LV2-retreat Slot3 branch should keep the direct suppress payoff distinct from safer recovery.");
            Assert.That(
                ResolveHighTierWaitAgencyRead(slot3RetreatRecovery),
                Does.Contain("retreat").And.Contain("recovery"),
                "The high-tier agency matrix should name the retreat route as movement agency, not a balance-only outcome.");
        }

        private static void AssertStageResultMotivationMatrix(
            PolicyMetrics noSummonFail,
            PolicyMetrics gunOnlyFail,
            PolicyMetrics cleanPhysical,
            PolicyMetrics highTierSuppress,
            PolicyMetrics counterRecovery,
            PolicyMetrics marksmanClear,
            PolicyMetrics vanguardClear)
        {
            Assert.AreEqual(
                "failure_analysis",
                ResolveResultHookClass(noSummonFail),
                "No-summon survival should stay a failure-analysis result hook.");
            Assert.AreEqual(
                "failure_analysis",
                ResolveResultHookClass(gunOnlyFail),
                "Gun-only survival should stay a failure-analysis result hook.");
            Assert.AreEqual(
                "clean_survival",
                ResolveResultHookClass(cleanPhysical),
                "Clean physical punish should stay a clean-survival result hook.");
            Assert.AreEqual(
                "pending",
                ResolveResultHookClass(highTierSuppress),
                "The promoted S1 LV3 probe should stay diagnostic until it actually commits a suppress result.");
            Assert.AreEqual(
                "counter_recovery",
                ResolveResultHookClass(counterRecovery),
                "Boss-screen recovery should stay a counter-recovery result hook.");
            Assert.AreEqual(
                "pending",
                ResolveResultHookClass(marksmanClear),
                "Slot2 full-bank should stay diagnostic until the marksman route actually commits a result.");
            Assert.AreEqual(
                "support_vanguard_clear",
                ResolveResultHookClass(vanguardClear),
                "Slot3 delayed clear should stay vanguard-specific at the result hook.");
            Assert.That(
                noSummonFail.ResultRecordNextObjective,
                Does.Contain("protect HP"),
                "Failure motivation should point to HP protection, not a generic reward payout.");
            Assert.That(
                gunOnlyFail.ResultRecordNextObjective,
                Does.Contain("protect HP"),
                "Gun-only failure motivation should point away from boss-chip tunnel vision.");
            Assert.That(
                cleanPhysical.ResultRecordNextObjective,
                Does.Contain("counter pressure"),
                "Clean clear motivation should reinforce confirming before counter pressure.");
            Assert.That(
                counterRecovery.ResultRecordNextObjective,
                Does.Contain("earlier"),
                "Recovery motivation should point to answering counter pressure earlier.");
            Assert.That(
                vanguardClear.ResultRecordNextObjective,
                Does.Contain("Slot3"),
                "Vanguard support clear should preserve the Slot3 route motivation.");
            AssertResultToken(
                noSummonFail,
                "review.failure.hp_pressure",
                "next.practice.hp_protection");
            AssertResultToken(
                gunOnlyFail,
                "review.failure.hp_pressure",
                "next.practice.hp_protection");
            AssertResultToken(
                cleanPhysical,
                "review.clear.clean_followup",
                "next.practice.clean_followup_confirm");
            AssertResultToken(
                counterRecovery,
                "review.clear.counter_recovery",
                "next.practice.counter_answer_timing");
            AssertResultToken(
                vanguardClear,
                "review.clear.vanguard_payoff",
                "next.practice.slot3_vanguard_payoff");
            Assert.IsTrue(IsReviewOnlyResultHook(noSummonFail));
            Assert.IsTrue(IsReviewOnlyResultHook(gunOnlyFail));
            Assert.IsTrue(IsReviewOnlyResultHook(cleanPhysical));
            Assert.IsTrue(IsReviewOnlyResultHook(counterRecovery));
            Assert.IsTrue(IsReviewOnlyResultHook(vanguardClear));
            Assert.AreEqual(0, highTierSuppress.ResultRecords);
            Assert.AreEqual(0, marksmanClear.ResultRecords);

            AssertResultOverlayMatchesRecord(noSummonFail);
            AssertResultOverlayMatchesRecord(gunOnlyFail);
            AssertResultOverlayMatchesRecord(cleanPhysical);
            AssertResultOverlayMatchesRecord(counterRecovery);
            AssertResultOverlayMatchesRecord(vanguardClear);
        }

        private static void AssertRouteMotivationDominanceMatrix(
            PolicyMetrics intended,
            PolicyMetrics physicalPunish,
            PolicyMetrics highTierSuppress,
            PolicyMetrics counterRecovery,
            PolicyMetrics marksmanCombo,
            PolicyMetrics vanguardPayoff,
            PolicyMetrics vanguardRetreatRecovery)
        {
            Assert.AreEqual(
                "CleanFollowupClear",
                physicalPunish.ResultKind,
                "The fast physical route should still clear through the state-gated follow-up.");
            Assert.Less(
                physicalPunish.ElapsedSeconds,
                intended.ElapsedSeconds,
                "The dominance matrix should expose that the physical block-punish is the fastest reference route.");
            Assert.Greater(
                physicalPunish.SummonBlocks,
                0,
                "The fast route should remain a summon-conversion route, not pure boss chip.");
            Assert.AreEqual(
                "clean_survival",
                ResolveResultHookClass(physicalPunish),
                "The fast route should stay the generic clean-survival hook, not consume support or high-tier motivation.");

            Assert.AreEqual(
                "CleanFollowupClear",
                intended.ResultKind,
                "The guided route should still complete as the clean baseline loop.");
            Assert.Greater(
                intended.CloseThreatBasicHits,
                0,
                "The guided route should keep the close-probe beat before the summon answer.");
            Assert.Greater(
                intended.SummonBlocks,
                0,
                "The guided route should keep summon interception as its state transition.");
            Assert.GreaterOrEqual(
                intended.FirstSummonUseAtSeconds,
                0f,
                "The guided route should expose when the summon answer was spent.");
            Assert.GreaterOrEqual(
                intended.SummonUseToBlockSeconds,
                0f,
                "The guided route should expose summon spend -> block timing.");
            Assert.GreaterOrEqual(
                intended.BlockToFollowupWindowSeconds,
                0f,
                "The guided route should expose block -> follow-up window timing.");
            Assert.AreEqual(
                "clean_survival",
                ResolveResultHookClass(intended),
                "The guided route should remain the baseline clean-survival result hook.");

            Assert.AreEqual(
                "CounterRecoveryClear",
                counterRecovery.ResultKind,
                "Counter recovery should remain separate from clean follow-up routes.");
            Assert.Greater(
                counterRecovery.CounterWaves,
                0,
                "Counter recovery should keep the relock evidence that motivates earlier answers.");
            Assert.AreEqual(
                "counter_recovery",
                ResolveResultHookClass(counterRecovery),
                "Counter recovery should keep its recovery-specific result hook.");

            Assert.AreEqual(
                "pending",
                ResolveResultHookClass(highTierSuppress),
                "The promoted S1 LV3 route should stay diagnostic instead of borrowing Slot3's result hook.");
            Assert.GreaterOrEqual(
                highTierSuppress.PlayerDamageTaken,
                0f,
                "The promoted S1 LV3 diagnostic route should record its HP exposure without pretending to be the payoff route.");
            Assert.AreEqual(
                0,
                highTierSuppress.ResultRecords,
                "The promoted S1 LV3 diagnostic route should not fabricate a clear payoff.");

            Assert.AreEqual(
                "pending",
                ResolveResultHookClass(marksmanCombo),
                "Slot2 full-bank combo should stay marked as an incomplete marksman route.");
            Assert.Greater(
                marksmanCombo.SupportSummonProjectileEnemySummonHits
                    + marksmanCombo.SupportSummonProjectileEnemyBodyHits,
                0,
                "Slot2 support should keep its hostile-frontline suppress effect.");
            Assert.Greater(
                marksmanCombo.SupportComboManaAfterSupport,
                marksmanCombo.SupportComboSlot1RequiredMana - 0.001f,
                "Slot2 full-bank support should preserve the main-answer slot.");
            Assert.That(
                ResolveFirstUnresolvedBeat(marksmanCombo),
                Is.EqualTo("FollowupConfirm").Or.EqualTo("CounterAnswer"),
                "Slot2 full-bank should expose the remaining uncommitted follow-up/counter gap.");

            Assert.AreEqual(
                "support_vanguard_clear",
                ResolveResultHookClass(vanguardPayoff),
                "Slot3 delayed payoff should keep the vanguard-specific route motivation.");
            Assert.Greater(
                vanguardPayoff.SupportSummonProjectileHits,
                0,
                "Slot3 support should keep the FireDragon breath payoff.");
            Assert.Greater(
                vanguardPayoff.BossPressureScreensSuppressedByFollowup,
                0,
                "Slot3 delayed payoff should preserve the boss-screen suppress payoff.");
            Assert.Greater(
                vanguardPayoff.ElapsedSeconds,
                marksmanCombo.ElapsedSeconds,
                "Slot3 vanguard payoff should remain the slower high-cost line-hold branch, not a hidden Slot2 replacement.");
            Assert.That(
                vanguardRetreatRecovery.ResultKind,
                Is.EqualTo("CounterRecoveryClear").Or.EqualTo("PlayerDownFail"),
                "Slot3 retreat/recommit should either close as recovery or stay visible as a failed retreat diagnostic.");
            Assert.That(
                ResolveResultHookClass(vanguardRetreatRecovery),
                Is.EqualTo("counter_recovery").Or.EqualTo("failure_analysis"),
                "Slot3 retreat/recommit should keep a recovery or failed-retreat result hook.");
            Assert.That(
                vanguardRetreatRecovery.CounterRecoveryConfirmed || vanguardRetreatRecovery.ResultKind == "PlayerDownFail",
                "Slot3 retreat/recommit should prove recovery or explicitly fail before payoff.");
            Assert.IsFalse(
                vanguardRetreatRecovery.BossScreenSuppressedByFollowup,
                "Slot3 retreat/recommit should give up the direct boss-screen suppress payoff.");
            if (vanguardRetreatRecovery.ResultKind == "CounterRecoveryClear")
            {
                Assert.Less(
                    vanguardRetreatRecovery.PlayerDamageTaken,
                    vanguardPayoff.PlayerDamageTaken,
                    "Slot3 retreat/recommit should reduce HP exposure when it actually reaches recovery.");
            }
            else
            {
                Assert.Greater(
                    vanguardRetreatRecovery.PlayerDamageTaken,
                    vanguardPayoff.PlayerDamageTaken,
                    "A failed Slot3 retreat diagnostic should expose the HP cost that killed the route.");
            }
            Assert.Greater(
                vanguardRetreatRecovery.BackSafetyBandSeconds,
                vanguardPayoff.BackSafetyBandSeconds + 1f,
                "Slot3 retreat/recommit should prove a movement-band decision, not only a delayed spend.");
        }

        private static void AssertResultToken(
            PolicyMetrics result,
            string expectedTokenId,
            string expectedNextStateHookId)
        {
            Assert.AreEqual(
                expectedTokenId,
                result.ResultRecordTokenId,
                $"{result.Policy} should commit a review-only result token instead of a payout id.");
            Assert.AreEqual(
                expectedNextStateHookId,
                result.ResultRecordNextStateHookId,
                $"{result.Policy} should commit a next-state practice hook.");
            Assert.IsFalse(
                ContainsProgressionPayoutLanguage(result.ResultRecordTokenId),
                $"{result.Policy} result token must not masquerade as a payout.");
            Assert.IsFalse(
                ContainsProgressionPayoutLanguage(result.ResultRecordNextStateHookId),
                $"{result.Policy} next-state hook must not masquerade as a payout.");
        }

        private static void AssertResultOverlayMatchesRecord(PolicyMetrics result)
        {
            Assert.AreEqual(
                result.ResultRecordTitle,
                result.ResultOverlayTitle,
                $"{result.Policy} should surface the committed result title through the review overlay.");
            Assert.AreEqual(
                result.ResultRecordSummary,
                result.ResultOverlaySummary,
                $"{result.Policy} should surface the committed result summary through the review overlay.");
            Assert.AreEqual(
                result.ResultRecordRouteLabel,
                result.ResultOverlayRoute,
                $"{result.Policy} should surface the committed route label through the review overlay.");
            Assert.AreEqual(
                result.ResultRecordRewardHook,
                result.ResultOverlayRewardHook,
                $"{result.Policy} should surface the reward/state hook through the review overlay.");
            Assert.AreEqual(
                result.ResultRecordNextObjective,
                result.ResultOverlayNextObjective,
                $"{result.Policy} should surface the next-run objective through the review overlay.");
            Assert.IsFalse(
                ContainsProgressionPayoutLanguage(result.ResultOverlayRewardHook),
                $"{result.Policy} overlay reward hook should remain review-only until reward economy is in scope.");
            Assert.IsFalse(
                ContainsProgressionPayoutLanguage(result.ResultOverlayNextObjective),
                $"{result.Policy} overlay next objective should not masquerade as a payout.");
        }

        private static void AssertSharedManaSupportComboBranch(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot3Blocked)
        {
            Assert.AreEqual("SummonSlot2", slot2Combo.SupportSummonSlotId);
            Assert.AreEqual("SummonSlot3", slot3Blocked.SupportSummonSlotId);
            Assert.AreEqual("LaserSoldier", slot2Combo.SupportSummonActorRoleId);
            Assert.AreEqual("FireDragon", slot3Blocked.SupportSummonActorRoleId);
            Assert.AreEqual(100f, slot2Combo.SupportSummonRequiredMana, 0.001f);
            Assert.AreEqual(300f, slot3Blocked.SupportSummonRequiredMana, 0.001f);
            Assert.AreEqual(200f, slot2Combo.SupportComboSlot1RequiredMana, 0.001f);
            Assert.AreEqual(200f, slot3Blocked.SupportComboSlot1RequiredMana, 0.001f);
            Assert.GreaterOrEqual(
                slot2Combo.SupportComboManaAfterSupport,
                slot2Combo.SupportComboSlot1RequiredMana - 0.001f,
                "Slot2 should spend less than the full bank and preserve the immediate Slot1 answer.");
            Assert.GreaterOrEqual(
                slot2Combo.SupportComboManaBeforeSlot1,
                slot2Combo.SupportComboSlot1RequiredMana - 0.001f,
                "Slot2 should still have the Slot1 answer available after the support-fire timing beat.");
            Assert.Less(
                slot3Blocked.SupportComboManaAfterSupport,
                slot3Blocked.SupportComboSlot1RequiredMana,
                "Slot3 should consume the shared bank so the immediate Slot1 answer is not payable.");
            Assert.IsTrue(slot2Combo.SupportComboSlot1Attempted);
            Assert.IsTrue(slot2Combo.SupportComboSlot1Used);
            Assert.IsTrue(slot3Blocked.SupportComboSlot1Attempted);
            Assert.IsFalse(slot3Blocked.SupportComboSlot1Used);
            Assert.That(
                slot3Blocked.SupportComboSlot1BlockedReason,
                Does.Contain("Requires 200 EN"),
                "Slot3's immediate Slot1 failure should be a resource lockout, not cooldown or input ambiguity.");
            Assert.Greater(slot2Combo.SummonUses, 1);
            Assert.Greater(slot2Combo.SummonBlocks, 0);
            Assert.AreEqual(
                0,
                slot2Combo.SkillProjectileHits,
                "Slot2 full-bank currently preserves Slot1 but still diagnoses the missing follow-up hit.");
            Assert.That(
                ResolveFirstUnresolvedBeat(slot2Combo),
                Is.EqualTo("FollowupConfirm").Or.EqualTo("CounterAnswer"),
                "Slot2 -> Slot1 should expose the remaining uncommitted follow-up/counter gap instead of fabricating the old clear.");
            Assert.Greater(
                slot3Blocked.SupportSummonProjectileHits,
                0,
                "Slot3 should still prove high-cost FireDragon fire while failing the immediate Slot1 branch.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(slot3Blocked),
                "Slot3's high-cost hold should remain a partial answer, not a forced clear.");
        }

        private static void AssertSharedManaDelayedMainAnswerBranch(
            PolicyMetrics slot2Delayed,
            PolicyMetrics slot3Delayed)
        {
            Assert.AreEqual("SummonSlot2", slot2Delayed.SupportSummonSlotId);
            Assert.AreEqual("SummonSlot3", slot3Delayed.SupportSummonSlotId);
            Assert.AreEqual("LaserSoldier", slot2Delayed.SupportSummonActorRoleId);
            Assert.AreEqual("FireDragon", slot3Delayed.SupportSummonActorRoleId);
            Assert.AreEqual(2, slot2Delayed.EnergyProbeTargetTier);
            Assert.AreEqual(3, slot3Delayed.EnergyProbeTargetTier);
            Assert.AreEqual(100f, slot2Delayed.SupportSummonRequiredMana, 0.001f);
            Assert.AreEqual(300f, slot3Delayed.SupportSummonRequiredMana, 0.001f);
            Assert.Less(
                slot2Delayed.SupportComboManaAfterSupport,
                slot2Delayed.SupportComboSlot1RequiredMana,
                "Slot2-at-LV2 should spend the early support answer before the immediate Slot1 answer is payable.");
            Assert.Less(
                slot3Delayed.SupportComboManaAfterSupport,
                slot3Delayed.SupportComboSlot1RequiredMana,
                "Slot3-at-LV3 should spend the bank before the delayed Slot1 answer reopens.");
            Assert.GreaterOrEqual(
                slot2Delayed.SupportComboSlot1ReadyDelaySeconds,
                0f,
                "Slot2 delayed branch should measure the recharge exposure before Slot1.");
            Assert.GreaterOrEqual(
                slot3Delayed.SupportComboSlot1ReadyDelaySeconds,
                0f,
                "Slot3 delayed branch should measure the recharge exposure before Slot1.");
            Assert.GreaterOrEqual(
                slot2Delayed.SupportComboManaBeforeSlot1,
                slot2Delayed.SupportComboSlot1RequiredMana - 0.001f,
                "Slot2 delayed branch should actually reopen the Slot1 answer.");
            Assert.GreaterOrEqual(
                slot3Delayed.SupportComboManaBeforeSlot1,
                slot3Delayed.SupportComboSlot1RequiredMana - 0.001f,
                "Slot3 delayed branch should actually reopen the Slot1 answer.");
            Assert.IsTrue(slot2Delayed.SupportComboSlot1Attempted);
            Assert.IsTrue(slot2Delayed.SupportComboSlot1Used);
            Assert.IsTrue(slot3Delayed.SupportComboSlot1Attempted);
            Assert.IsTrue(slot3Delayed.SupportComboSlot1Used);
            Assert.GreaterOrEqual(
                slot2Delayed.SupportSummonProjectileEnemySummonHits,
                0,
                "Slot2 delayed branch should record marksman enemy-frontline hits even when the support shot misses.");
            Assert.Greater(
                slot3Delayed.SupportSummonProjectileHits,
                0,
                "Slot3 delayed branch should retain the FireDragon breath proof before the main answer.");
            Assert.AreEqual(
                0,
                slot2Delayed.PhysicalBarragePlayerHits,
                "Slot2 delayed branch should convert the final physical barrage through Slot1 instead of leaking player hits.");
            Assert.AreEqual(
                0,
                slot3Delayed.PhysicalBarragePlayerHits,
                "Slot3 delayed branch should convert the final physical barrage through Slot1 instead of leaking player hits.");
            Assert.Greater(slot2Delayed.SkillUses, 0);
            Assert.Greater(slot3Delayed.SkillUses, 0);
            Assert.That(
                ResolveFirstUnresolvedBeat(slot2Delayed),
                Is.EqualTo("FollowupConfirm").Or.EqualTo("CounterAnswer"),
                "Slot2 delayed branch should reveal the unclosed follow-up/counter state after the late Slot1 confirmation.");
            Assert.Greater(slot2Delayed.CounterWaves, 0);
            Assert.Greater(slot2Delayed.CounterWaveAnswerEnergyPulse, 0f);
            Assert.AreEqual(0, slot2Delayed.ResultRecords);
            Assert.That(
                ResolveFirstUnresolvedBeat(slot2Delayed),
                Is.EqualTo("FollowupConfirm").Or.EqualTo("CounterAnswer"),
                "Slot2 delayed support should recharge into Slot1 but still require follow-up/counter recovery.");
            if (ResolveFirstUnresolvedBeat(slot3Delayed) == "Complete")
            {
                Assert.Greater(
                    slot3Delayed.SkillProjectileHits,
                    0,
                    "Slot3 delayed direct branch should close through real Skill1 projectile hits.");
                Assert.Greater(
                    slot3Delayed.ResultRecords,
                    0,
                    "Slot3 delayed direct branch should commit a clean result when it beats the boss-screen timing.");
            }
            else
            {
                Assert.IsTrue(
                    slot3Delayed.BossBlockedSkill1Followup,
                    "Slot3 delayed branch should expose the counter answer when the boss screen catches the late Slot1 confirm.");
                Assert.Greater(slot3Delayed.CounterWaves, 0);
                Assert.Greater(slot3Delayed.CounterWaveAnswerEnergyPulse, 0f);
                Assert.AreEqual(0, slot3Delayed.ResultRecords);
                Assert.AreEqual(
                    "CounterAnswer",
                    ResolveFirstUnresolvedBeat(slot3Delayed),
                    "Slot3 delayed support should remain legible as a counter-answer branch when it does not directly clear.");
            }
        }

        private static void AssertSharedManaDelayedCounterRecoveryBranch(
            PolicyMetrics slot2Recovery,
            PolicyMetrics slot3Recovery)
        {
            Assert.AreEqual("SummonSlot2", slot2Recovery.SupportSummonSlotId);
            Assert.AreEqual("SummonSlot3", slot3Recovery.SupportSummonSlotId);
            Assert.AreEqual(2, slot2Recovery.EnergyProbeTargetTier);
            Assert.AreEqual(3, slot3Recovery.EnergyProbeTargetTier);
            Assert.IsTrue(slot2Recovery.SupportComboSlot1Used);
            Assert.IsTrue(slot3Recovery.SupportComboSlot1Used);
            Assert.AreEqual(
                0,
                slot2Recovery.PhysicalBarragePlayerHits,
                "Slot2 delayed recovery should still block the physical barrage before the recovery check.");
            Assert.AreEqual(
                0,
                slot3Recovery.PhysicalBarragePlayerHits,
                "Slot3 delayed recovery should still block the physical barrage before the recovery check.");
            Assert.Greater(
                slot2Recovery.CounterWaves,
                0,
                "Slot2 delayed recovery should expose the counter branch before the fresh answer.");
            Assert.Greater(
                slot2Recovery.CounterWaveAnswerEnergyPulse,
                0f,
                "Slot2 delayed recovery should receive the counter-answer energy pulse.");
            Assert.GreaterOrEqual(
                slot2Recovery.CounterTriggerToAnswerSeconds,
                0f,
                "Slot2 delayed recovery should record counter trigger -> fresh summon answer timing.");
            Assert.LessOrEqual(
                slot2Recovery.CounterTriggerToAnswerSeconds,
                PhysicalBarrageProbeFlightSeconds,
                "Slot2 delayed recovery should answer within the measured physical barrage exposure window.");
            Assert.IsTrue(
                slot2Recovery.CounterRecoveryConfirmed,
                "Slot2 delayed recovery should stabilize after the fresh summon answer.");
            Assert.Greater(slot2Recovery.SkillProjectileHits, 0);
            Assert.Greater(slot2Recovery.ResultRecords, 0);
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(slot2Recovery),
                "Slot2 delayed recovery should commit the result after the counter answer.");

            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(slot3Recovery),
                "Slot3 delayed recovery policy should end complete whether it cleared directly or through counter recovery.");
            Assert.Greater(slot3Recovery.SkillProjectileHits, 0);
            Assert.Greater(slot3Recovery.ResultRecords, 0);
            if (slot3Recovery.CounterWaves > 0)
            {
                Assert.Greater(
                    slot3Recovery.CounterWaveAnswerEnergyPulse,
                    0f,
                    "Slot3 delayed recovery should receive a pulse when it enters the counter branch.");
                Assert.IsTrue(
                    slot3Recovery.CounterRecoveryConfirmed,
                    "Slot3 delayed recovery should stabilize when it enters the counter branch.");
            }
        }

        private static void AssertSupportDecisionTimingVerdicts(
            PolicyMetrics slot1Recovery,
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.AreEqual(
                "emergency baseline",
                ResolveSupportDecisionTimingVerdict(slot1Recovery),
                "Slot1 should remain the emergency baseline classification, even though the promoted contract locks it below LV2.");
            Assert.AreEqual(
                "marksman combo incomplete",
                ResolveSupportDecisionTimingVerdict(slot2Combo),
                "Slot2 should show that it preserves enough shared mana but no longer closes the old marksman combo.");
            Assert.AreEqual(
                "mistimed LV2 spend",
                ResolveSupportDecisionTimingVerdict(slot2DelayedRecovery),
                "Slot2-at-LV2 delayed recovery should be classified as a mistimed tempo spend, not buffed into every route.");
            Assert.GreaterOrEqual(
                ResolveSupportDecisionHpBeforeMain(slot2DelayedRecovery),
                ResolveSupportDecisionHpBeforeMain(slot1Recovery),
                "The mistimed Slot2 delayed branch should stay at or above the emergency Slot1 recovery exposure band.");
            Assert.LessOrEqual(
                slot2DelayedRecovery.BossDamageTaken,
                slot3DelayedRecovery.BossDamageTaken + 44.5f,
                "The mistimed Slot2 delayed branch should stay in the recovery band, not become the stable Slot3 suppress route.");
            Assert.LessOrEqual(
                slot2DelayedRecovery.BossDamageFromAllySummon,
                50f,
                "Slot2 delayed recovery should not become a hidden ally-DPS payoff.");
            Assert.AreEqual(
                "resource lockout",
                ResolveSupportDecisionTimingVerdict(slot3Blocked),
                "Slot3 immediate should preserve the high-cost bank-spend lockout read.");
            Assert.AreEqual(
                "intended vanguard payoff",
                ResolveSupportDecisionTimingVerdict(slot3DelayedRecovery),
                "Slot3 delayed should preserve its high-cost boss-screen suppress payoff.");
        }

        private static void AssertSupportPayoffVectorMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.Greater(
                slot2Combo.SupportSummonProjectileEnemySummonHits
                    + slot2Combo.SupportSummonProjectileEnemyBodyHits,
                0,
                "Slot2 full-bank should keep a marksman frontline suppression contribution.");
            Assert.Greater(
                slot2Combo.CounterWaves,
                0,
                "Slot2 full-bank should expose the current counter-answer gap instead of pretending to be a no-relock clear.");
            Assert.Greater(
                slot2DelayedRecovery.CounterWaves,
                0,
                "Slot2 delayed should remain distinguishable as a recovery/relock branch.");
            Assert.Greater(
                slot3Blocked.SupportSummonProjectileHits,
                0,
                "Slot3 immediate should prove FireDragon pressure even when it spends the main-answer slot.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(slot3Blocked),
                "Slot3 immediate should still show that prevention alone is not the clear payoff.");
            Assert.Greater(
                slot3DelayedRecovery.BossPressureScreensSuppressedByFollowup,
                slot2Combo.BossPressureScreensSuppressedByFollowup,
                "Slot3 delayed should keep the vanguard-specific boss-screen suppress payoff.");
            Assert.Greater(
                slot3DelayedRecovery.BossDamageTaken,
                slot2Combo.BossDamageTaken,
                "Slot3 delayed should remain the actual boss-screen payoff while Slot2 full-bank is diagnostic.");
            Assert.AreEqual(
                "support_vanguard_clear",
                ResolveResultHookClass(slot3DelayedRecovery),
                "Slot3 delayed should keep its prevention payoff result hook.");
        }

        private static void AssertSupportBodyCostPhaseMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot3DelayedRecovery)
        {
            AssertSupportBodyCostPhaseMeasured(slot2Combo);
            AssertSupportBodyCostPhaseMeasured(slot2DelayedRecovery);
            AssertSupportBodyCostPhaseMeasured(slot3Blocked);
            AssertSupportBodyCostPhaseMeasured(slot3DelayedRecovery);
            Assert.GreaterOrEqual(
                slot3DelayedRecovery.SupportBodyHitsBeforeMainAnswer,
                slot3DelayedRecovery.SupportBodyHitsBeforeSupport,
                "Slot3 delayed phase read should show whether body pressure accumulates during the recharge wait.");
            Assert.GreaterOrEqual(
                slot3DelayedRecovery.SupportBodyHitsFinal,
                slot3DelayedRecovery.SupportBodyHitsBeforeMainAnswer,
                "Slot3 delayed final phase should include any body-pressure leak after the main answer.");
        }

        private static void AssertSupportBodyCostPhaseMeasured(PolicyMetrics result)
        {
            Assert.GreaterOrEqual(
                result.SupportBodyHitsBeforeSupport,
                0,
                $"{result.Policy} should record body hits before the support summon is spent.");
            Assert.GreaterOrEqual(
                result.SupportBodyHitsBeforeMainAnswer,
                result.SupportBodyHitsBeforeSupport,
                $"{result.Policy} should record body hits before the main answer without moving backward.");
            Assert.GreaterOrEqual(
                result.SupportBodyHitsFinal,
                result.SupportBodyHitsBeforeMainAnswer,
                $"{result.Policy} should record final body hits after the route has resolved.");
            Assert.GreaterOrEqual(
                result.SupportDamageBeforeSupport,
                0f,
                $"{result.Policy} should record player damage before support is spent.");
            Assert.GreaterOrEqual(
                result.SupportDamageFinal,
                result.SupportDamageBeforeSupport,
                $"{result.Policy} should record final player damage without moving backward.");
        }

        private static void AssertSupportWaitExposureMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot3DelayedRecovery)
        {
            AssertSupportWaitExposureMeasured(slot2Combo);
            AssertSupportWaitExposureMeasured(slot2DelayedRecovery);
            AssertSupportWaitExposureMeasured(slot3Blocked);
            AssertSupportWaitExposureMeasured(slot3DelayedRecovery);
            Assert.AreEqual(
                2,
                ResolveSupportWaitExposureTargetTier(slot2DelayedRecovery),
                "Slot2 delayed wait exposure should remain the LV2 support choice.");
            Assert.AreEqual(
                3,
                ResolveSupportWaitExposureTargetTier(slot3DelayedRecovery),
                "Slot3 delayed wait exposure should remain the LV3 support choice.");
            Assert.AreEqual(
                300f,
                slot3DelayedRecovery.SupportSummonRequiredMana,
                0.001f,
                "Slot3 delayed wait exposure should preserve the 300 EN vanguard cost.");
            Assert.Greater(
                ResolveSupportWaitExposureSeconds(slot3DelayedRecovery),
                ResolveSupportWaitExposureSeconds(slot2DelayedRecovery),
                "Slot3 delayed should visibly pay more pre-support wait exposure than the LV2 marksman branch.");
            Assert.GreaterOrEqual(
                slot3DelayedRecovery.SupportDamageBeforeSupport,
                slot2DelayedRecovery.SupportDamageBeforeSupport,
                "Slot3 delayed should show the vanguard cost is paid before the support appears, not after.");
            Assert.AreEqual(
                "support_vanguard_clear",
                ResolveResultHookClass(slot3DelayedRecovery),
                "Slot3 delayed wait exposure should end in the vanguard payoff hook.");
            Assert.AreEqual(
                "resource lockout",
                ResolveSupportDecisionTimingVerdict(slot3Blocked),
                "Slot3 immediate should remain a resource lockout when LV3 is spent before the main answer.");
        }

        private static void AssertSupportWaitExposureMeasured(PolicyMetrics result)
        {
            Assert.GreaterOrEqual(
                ResolveSupportWaitExposureSeconds(result),
                0f,
                $"{result.Policy} should record wait time from energy probe start to support spend.");
            Assert.Greater(
                result.SupportSummonRequiredMana,
                0f,
                $"{result.Policy} should record a support summon mana cost.");
            Assert.GreaterOrEqual(
                result.SupportBodyHitsBeforeSupport,
                0,
                $"{result.Policy} should record body hits before support spend for wait-exposure comparison.");
            Assert.GreaterOrEqual(
                result.SupportDamageBeforeSupport,
                0f,
                $"{result.Policy} should record player damage before support spend for wait-exposure comparison.");
        }

        private static void AssertSupportUpgradeDeltaMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.Greater(
                ResolveSupportWaitDeltaSeconds(slot2DelayedRecovery, slot2Combo),
                0f,
                "Banking Slot2 from LV2 to a full-bank LV3 route should record extra wait exposure.");
            Assert.Greater(
                ResolveSupportWaitDeltaSeconds(slot2DelayedRecovery, slot3DelayedRecovery),
                0f,
                "Upgrading from Slot2 LV2 to Slot3 LV3 should record extra wait exposure.");
            Assert.Greater(
                ResolveSupportPreSupportBodyDelta(slot2DelayedRecovery, slot3DelayedRecovery),
                0f,
                "The Slot3 LV3 upgrade should show extra pre-support body pressure.");
            Assert.Greater(
                ResolveSupportPreSupportDamageDelta(slot2DelayedRecovery, slot3DelayedRecovery),
                0f,
                "The Slot3 LV3 upgrade should show extra pre-support player damage.");
            Assert.Less(
                ResolveSupportMainAnswerDelaySeconds(slot2Combo),
                ResolveSupportMainAnswerDelaySeconds(slot2DelayedRecovery),
                "Banking Slot2 should keep the main answer more immediate than the LV2 delayed spend.");
            Assert.Greater(
                ResolveSupportMainAnswerDelaySeconds(slot3DelayedRecovery),
                ResolveSupportMainAnswerDelaySeconds(slot2DelayedRecovery),
                "Slot3 LV3 should keep the slower delayed main-answer gate visible.");
            Assert.AreEqual(
                "marksman payoff incomplete",
                ResolveSupportDecisionPayoffVerdict(slot2Combo),
                "The Slot2 full-bank delta should stay diagnostic until the marksman route actually closes.");
            Assert.AreEqual(
                "boss-screen suppress payoff",
                ResolveSupportDecisionPayoffVerdict(slot3DelayedRecovery),
                "The Slot3 LV3 delta should end in the vanguard boss-screen suppress payoff.");
        }

        private static void AssertSupportUpgradeDecisionReadoutMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.That(
                FormatSupportUpgradeDecisionHudState("Slot2 LV2 now", slot2DelayedRecovery),
                Does.Contain("S2 CD").And.Contain("S1 ready 200EN"),
                "The current Slot2 readout should show the slot cooldown and promoted Slot1 readiness before the next spend.");
            Assert.That(
                slot2DelayedRecovery.SupportChoiceForecastReadoutBeforeSupport,
                Does.Contain("S2 ready now").And.Contain("hold 300 EN").And.Contain("S3 suppress"),
                "The live route incentive should forecast the LV2-now versus LV3-later support trade before the support spend.");
            Assert.That(
                slot2Combo.SupportChoiceForecastReadoutBeforeSupport,
                Does.Contain("S2 ready for tempo").And.Contain("S3 ready for suppress"),
                "At a full bank, the live route incentive should keep both support choices visible instead of making S3 a universal upgrade.");
            Assert.That(
                FormatSupportUpgradeDecisionHudState("Slot3 LV3 payoff", slot3DelayedRecovery),
                Does.Contain("S3").And.Contain("CD").And.Contain("S1 ready 200EN"),
                "The Slot3 payoff readout should preserve slot cooldown plus promoted Slot1 readiness instead of reading as a global cooldown.");
            Assert.That(
                slot3DelayedRecovery.SupportChoiceForecastReadoutBeforeSupport,
                Does.Contain("S2 ready for tempo").And.Contain("S3 ready for suppress").And.Contain("Slot1 recharge"),
                "The live Slot3 forecast should say the full-bank choice is between tempo and suppress payoff.");
            Assert.That(
                FormatSupportUpgradeDecisionMeasuredCost(slot2DelayedRecovery, slot2Combo),
                Does.Contain("wait +").And.Contain("HP +"),
                "Banking Slot2 should keep its measured wait and HP tax visible beside the readout.");
            Assert.That(
                FormatSupportUpgradeDecisionMeasuredCost(slot2DelayedRecovery, slot3DelayedRecovery),
                Does.Contain("wait +").And.Contain("HP +"),
                "Upgrading to Slot3 should keep its measured wait and HP tax visible beside the readout.");
            Assert.That(
                ResolveSupportUpgradeDecisionReadoutRead(slot2DelayedRecovery, slot2Combo),
                Does.Contain("incomplete"),
                "The Slot2 full-bank read should keep the current payoff gap visible.");
            Assert.That(
                ResolveSupportUpgradeDecisionReadoutRead(slot2DelayedRecovery, slot3DelayedRecovery),
                Does.Contain("boss-screen suppress"),
                "The Slot3 LV3 read should say what the extra wait buys without making it a universal upgrade.");
        }

        private static void AssertSupportStageSlotTimelineMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.AreEqual(
                "SummonSlot2",
                slot2Combo.SupportSummonSlotId,
                "The support timeline should keep the full-bank combo on the Slot2 marksman branch.");
            Assert.GreaterOrEqual(
                slot2Combo.SupportComboManaAfterSupport,
                slot2Combo.SupportComboSlot1RequiredMana - 0.001f,
                "The Slot2 full-bank timeline should preserve the immediate main-answer slot.");
            Assert.IsTrue(
                slot2Combo.SupportComboSlot1Used,
                "The Slot2 full-bank timeline should spend Slot1 after the marksman support beat.");
            Assert.Greater(
                slot2Combo.CounterWaves,
                0,
                "The Slot2 full-bank timeline should expose the current counter-answer relock.");
            Assert.AreEqual(
                "pending",
                ResolveResultHookClass(slot2Combo),
                "The Slot2 full-bank timeline should stay diagnostic until the marksman route commits a result hook.");

            Assert.AreEqual(
                "SummonSlot2",
                slot2DelayedRecovery.SupportSummonSlotId,
                "The delayed recovery timeline should still be the Slot2 marksman branch.");
            Assert.Less(
                slot2DelayedRecovery.SupportComboManaAfterSupport,
                slot2DelayedRecovery.SupportComboSlot1RequiredMana,
                "The early Slot2 timeline should spend the shared bank before the main answer is payable.");
            Assert.IsTrue(
                slot2DelayedRecovery.SupportComboSlot1Used,
                "The early Slot2 timeline should recharge into Slot1 instead of staying a dead input.");
            Assert.Greater(
                slot2DelayedRecovery.CounterWaves,
                0,
                "The early Slot2 timeline should visibly relock into counter pressure.");
            Assert.IsTrue(
                slot2DelayedRecovery.CounterRecoveryConfirmed,
                "The early Slot2 timeline should prove the recovery slot closes only after the fresh answer.");

            Assert.AreEqual(
                "SummonSlot3",
                slot3Blocked.SupportSummonSlotId,
                "The immediate lockout timeline should stay on the Slot3 vanguard branch.");
            Assert.Less(
                slot3Blocked.SupportComboManaAfterSupport,
                slot3Blocked.SupportComboSlot1RequiredMana,
                "The Slot3 immediate timeline should spend the main-answer resource slot.");
            Assert.IsFalse(
                slot3Blocked.SupportComboSlot1Used,
                "The Slot3 immediate timeline should not pretend Slot1 was available.");
            Assert.AreEqual(
                "ScreenCurtain",
                ResolveFirstUnresolvedBeat(slot3Blocked),
                "The Slot3 immediate timeline should leave the screen-curtain beat unresolved.");

            Assert.AreEqual(
                "SummonSlot3",
                slot3DelayedRecovery.SupportSummonSlotId,
                "The delayed payoff timeline should stay on the Slot3 vanguard branch.");
            Assert.IsTrue(
                slot3DelayedRecovery.SupportComboSlot1Used,
                "The delayed Slot3 timeline should recharge into the main-answer slot.");
            Assert.IsTrue(
                slot3DelayedRecovery.BossScreenSuppressedByFollowup,
                "The delayed Slot3 timeline should convert line hold into the boss-screen suppress payoff.");
            Assert.AreEqual(
                "support_vanguard_clear",
                ResolveResultHookClass(slot3DelayedRecovery),
                "The delayed Slot3 timeline should end in the vanguard-specific result hook.");
        }

        private static void AssertSummonSlotReadinessCooldownMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.Greater(
                slot2Combo.SupportComboSupportCooldownAfterSupport,
                0f,
                "Slot2 should start its own cooldown immediately after the support spend.");
            Assert.AreEqual(
                0f,
                slot2Combo.SupportComboSlot1CooldownBeforeAttempt,
                0.001f,
                "Slot2 cooldown should not put Slot1 on cooldown before the preserved main answer.");
            Assert.IsTrue(
                slot2Combo.SupportComboSlot1Used,
                "Slot2 full-bank combo should prove Slot1 can still fire while Slot2's own cooldown is active.");
            Assert.Greater(
                slot2Combo.SupportComboSlot1CooldownAfterAttempt,
                0f,
                "Slot1 should start its own cooldown only after Slot1 actually fires.");

            Assert.Greater(
                slot3Blocked.SupportComboSupportCooldownAfterSupport,
                0f,
                "Slot3 should start its own cooldown immediately after the vanguard spend.");
            Assert.AreEqual(
                0f,
                slot3Blocked.SupportComboSlot1CooldownBeforeAttempt,
                0.001f,
                "Slot3 cooldown should not put Slot1 on cooldown before the immediate main-answer attempt.");
            Assert.IsFalse(
                slot3Blocked.SupportComboSlot1Used,
                "Slot3 immediate branch should fail the immediate Slot1 answer.");
            Assert.That(
                slot3Blocked.SupportComboSlot1BlockedReason,
                Does.Contain("Requires 200 EN"),
                "Slot3 immediate Slot1 failure should be shared-EN lockout, not cooldown or input ambiguity.");
            Assert.AreEqual(
                0f,
                slot3Blocked.SupportComboSlot1CooldownAfterAttempt,
                0.001f,
                "A mana-blocked Slot1 attempt should not start Slot1 cooldown.");

            Assert.Greater(
                slot2DelayedRecovery.SupportComboSupportCooldownBeforeSlot1,
                0f,
                "Slot2 delayed recovery should prove Slot1 can reopen while Slot2's own cooldown remains active.");
            Assert.AreEqual(
                0f,
                slot2DelayedRecovery.SupportComboSlot1CooldownBeforeAttempt,
                0.001f,
                "Slot2 delayed recovery should reopen Slot1 from shared EN readiness, not from a cooldown reset shortcut.");
            Assert.IsTrue(slot2DelayedRecovery.SupportComboSlot1Used);

            Assert.Greater(
                slot3DelayedRecovery.SupportComboSupportCooldownBeforeSlot1,
                0f,
                "Slot3 delayed recovery should prove Slot1 can reopen while Slot3's own cooldown remains active.");
            Assert.AreEqual(
                0f,
                slot3DelayedRecovery.SupportComboSlot1CooldownBeforeAttempt,
                0.001f,
                "Slot3 delayed recovery should reopen Slot1 from shared EN readiness, not from a cooldown reset shortcut.");
            Assert.IsTrue(slot3DelayedRecovery.SupportComboSlot1Used);
        }

        private static void AssertSummonHudReadinessReadoutMatrix(
            PolicyMetrics slot2Combo,
            PolicyMetrics slot3Blocked,
            PolicyMetrics slot2DelayedRecovery,
            PolicyMetrics slot3DelayedRecovery)
        {
            Assert.That(
                slot2Combo.SupportComboHudSupportLabelBeforeSlot1,
                Does.Contain("CD"),
                "Slot2 should visibly read as cooling down while the full-bank combo preserves Slot1.");
            Assert.That(
                slot2Combo.SupportComboHudSlot1LabelBeforeAttempt,
                Does.Contain("READY LV2"),
                "Slot1 should visibly read ready while Slot2's own cooldown is still active.");
            Assert.That(
                slot2Combo.SupportComboOverlayHudReadoutBeforeSlot1,
                Does.Contain("S2 CD").And.Contain("S1 ready"),
                "The review overlay should not collapse Slot2 cooldown and Slot1 readiness into a global summon readout.");
            Assert.Less(
                slot2Combo.SupportComboHudSupportFillBeforeSlot1,
                0.995f,
                "Slot2's cooling-down coaster should not pulse as ready.");
            Assert.GreaterOrEqual(
                slot2Combo.SupportComboHudSlot1FillBeforeAttempt,
                0.995f,
                "Slot1's preserved main answer should pulse as ready.");

            Assert.That(
                slot3Blocked.SupportComboHudSupportLabelBeforeSlot1,
                Does.Contain("CD"),
                "Slot3 should show its own cooldown immediately after the vanguard spend.");
            Assert.That(
                slot3Blocked.SupportComboHudSlot1LabelBeforeAttempt,
                Does.Not.Contain("CD"),
                "Slot1 should not look globally cooled down after Slot3 fires.");
            Assert.That(
                slot3Blocked.SupportComboOverlayHudReadoutBeforeSlot1,
                Does.Contain("S3 CD").And.Contain("S1 +200EN/200"),
                "The review overlay should name Slot3 cooldown while showing Slot1 is resource-empty, not globally cooling down.");
            Assert.Less(
                slot3Blocked.SupportComboHudSlot1FillBeforeAttempt,
                0.05f,
                "Slot3 immediate lockout should leave Slot1's coaster empty because shared EN was spent.");

            Assert.That(
                slot2DelayedRecovery.SupportComboHudSupportLabelBeforeSlot1,
                Does.Contain("CD"),
                "Slot2 delayed recovery should show the marksman slot's own cooldown before Slot1 fires.");
            Assert.That(
                slot2DelayedRecovery.SupportComboHudSlot1LabelBeforeAttempt,
                Does.Contain("READY LV2"));
            Assert.That(
                slot2DelayedRecovery.SupportComboOverlayHudReadoutBeforeSlot1,
                Does.Contain("S2 CD").And.Contain("S1 ready 200EN"),
                "Slot2 delayed recovery should read as slot cooldown while Slot1 is ready.");
            Assert.GreaterOrEqual(
                slot2DelayedRecovery.SupportComboHudSlot1FillBeforeAttempt,
                0.995f);

            Assert.That(
                slot3DelayedRecovery.SupportComboHudSupportLabelBeforeSlot1,
                Does.Contain("CD"),
                "Slot3 delayed recovery should show the vanguard slot's own cooldown before Slot1 fires.");
            Assert.That(
                slot3DelayedRecovery.SupportComboHudSlot1LabelBeforeAttempt,
                Does.Contain("READY LV2"));
            Assert.That(
                slot3DelayedRecovery.SupportComboOverlayHudReadoutBeforeSlot1,
                Does.Contain("S3 CD").And.Contain("S1 ready 200EN"),
                "Slot3 delayed recovery should read as slot cooldown while Slot1 is ready.");
            Assert.GreaterOrEqual(
                slot3DelayedRecovery.SupportComboHudSlot1FillBeforeAttempt,
                0.995f);
        }

        private static SummonRosterAuditRow[] BuildSummonRosterAuditRows(IReadOnlyList<PolicyMetrics> results)
        {
            float[] sharedCosts = ResolveSharedSummonTierCosts(results);
            int slot1MinimumTier = 2;
            int slot2MinimumTier = ResolveSupportSummonMinimumTier("SummonSlot2", 1);
            int slot3MinimumTier = ResolveSupportSummonMinimumTier("SummonSlot3", 3);
            float slot1RequiredMana =
                ResolveSummonSlot1RequiredMana(ResolveCumulativeSummonMana(sharedCosts, slot1MinimumTier));
            float slot2RequiredMana = ResolveSupportSummonRequiredMana(
                "SummonSlot2",
                ResolveCumulativeSummonMana(sharedCosts, slot2MinimumTier));
            float slot3RequiredMana = ResolveSupportSummonRequiredMana(
                "SummonSlot3",
                ResolveCumulativeSummonMana(sharedCosts, slot3MinimumTier));
            return new[]
            {
                BuildSummonRosterAuditRow(
                    "SummonSlot1",
                    SummonSlot1ActionProfilePath,
                    "explicit requiredSummonMana + shared SummonEnergyLadder spend gate",
                    sharedCosts,
                    slot1MinimumTier,
                    slot1RequiredMana,
                    "saved-EN jump slam; mid-cost pressure answer"),
                BuildSummonRosterAuditRow(
                    "SummonSlot2",
                    SummonSlot2ActionProfilePath,
                    "explicit requiredSummonMana + shared SummonEnergyLadder spend gate",
                    sharedCosts,
                    slot2MinimumTier,
                    slot2RequiredMana,
                    "low-cost laser poke; no screen; LV1 cost gate"),
                BuildSummonRosterAuditRow(
                    "SummonSlot3",
                    SummonSlot3ActionProfilePath,
                    "explicit requiredSummonMana + shared SummonEnergyLadder spend gate",
                    sharedCosts,
                    slot3MinimumTier,
                    slot3RequiredMana,
                    "dragon breath damage; no screen; LV3 cost gate")
            };
        }

        private static SummonRosterAuditRow BuildSummonRosterAuditRow(
            string slot,
            string assetPath,
            string costSource,
            float[] tierCosts,
            int minimumTier,
            float requiredMana,
            string readout)
        {
            SummonSlotActionProfile profile =
                AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(assetPath);
            Assert.NotNull(profile, $"Missing summon slot profile at {assetPath}.");
            PlayerSummonSlot1Action.SummonTierSettings[] settings = profile.CopyTierSettings();
            Assert.GreaterOrEqual(settings.Length, 3, $"{slot} should expose three tier settings.");

            float[] volleyDamage = new float[3];
            float[] projectileSpeed = new float[3];
            int[] screenIntercepts = new int[3];
            float[] actorHealth = new float[3];
            float[] counterDamage = new float[3];
            string[] roleIds = new string[3];
            for (int i = 0; i < 3; i++)
            {
                PlayerSummonSlot1Action.SummonTierSettings tier = settings[i];
                tier.Normalize();
                volleyDamage[i] = tier.Damage * tier.ProjectileCount;
                projectileSpeed[i] = tier.ProjectileSpeed;
                screenIntercepts[i] = tier.ScreenIntercepts;
                actorHealth[i] = tier.ActorMaxHealth;
                counterDamage[i] = tier.CounterDamage;
                roleIds[i] = tier.ActorRoleId;
            }

            return new SummonRosterAuditRow(
                slot,
                profile.ActionId,
                costSource,
                CopyFirstThree(tierCosts),
                Mathf.Clamp(minimumTier, 1, 3),
                Mathf.Max(0f, requiredMana),
                roleIds,
                volleyDamage,
                projectileSpeed,
                screenIntercepts,
                actorHealth,
                counterDamage,
                readout);
        }

        private static float[] ResolveSharedSummonTierCosts(IReadOnlyList<PolicyMetrics> results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                PolicyMetrics result = results[i];
                if (result.SummonManaCostTier1 > 0f
                    && result.SummonManaCostTier2 > 0f
                    && result.SummonManaCostTier3 > 0f)
                {
                    return new[]
                    {
                        result.SummonManaCostTier1,
                        result.SummonManaCostTier2,
                        result.SummonManaCostTier3
                    };
                }
            }

            return new[] { 0f, 0f, 0f };
        }

        private static int ResolveSupportSummonMinimumTier(string slotActionName, int fallbackTier)
        {
            PlayerSupportSummonSlotAction[] actions = Object.FindObjectsByType<PlayerSupportSummonSlotAction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < actions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = actions[i];
                if (action != null && string.Equals(action.SlotActionName, slotActionName, StringComparison.Ordinal))
                {
                    return action.MinimumSummonTier;
                }
            }

            return Mathf.Clamp(fallbackTier, 1, 3);
        }

        private static float ResolveSummonSlot1RequiredMana(float fallbackMana)
        {
            PlayerSummonSlot1Action action =
                Object.FindFirstObjectByType<PlayerSummonSlot1Action>(FindObjectsInactive.Include);
            return action != null ? action.RequiredSummonMana : Mathf.Max(0f, fallbackMana);
        }

        private static float ResolveSupportSummonRequiredMana(string slotActionName, float fallbackMana)
        {
            PlayerSupportSummonSlotAction[] actions = Object.FindObjectsByType<PlayerSupportSummonSlotAction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < actions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = actions[i];
                if (action != null && string.Equals(action.SlotActionName, slotActionName, StringComparison.Ordinal))
                {
                    return action.RequiredSummonMana;
                }
            }

            return Mathf.Max(0f, fallbackMana);
        }

        private static float ResolveCumulativeSummonMana(float[] tierCosts, int minimumTier)
        {
            int clampedTier = Mathf.Clamp(minimumTier, 1, 3);
            float total = 0f;
            for (int i = 0; i < clampedTier; i++)
            {
                total += tierCosts != null && tierCosts.Length > i ? Mathf.Max(0f, tierCosts[i]) : 0f;
            }

            return total;
        }

        private static string ResolveSummonRosterCostVerdict(SummonRosterAuditRow[] rows)
        {
            if (rows.Length < 3)
            {
                return "REVIEW roster rows missing";
            }

            bool tierSplit = rows[0].MinimumTier == 2
                && rows[1].MinimumTier == 1
                && rows[2].MinimumTier == 3;
            bool manaSplit = Mathf.Abs(rows[0].RequiredMana - 200f) <= 0.001f
                && Mathf.Abs(rows[1].RequiredMana - 100f) <= 0.001f
                && Mathf.Abs(rows[2].RequiredMana - 300f) <= 0.001f;

            return tierSplit && manaSplit
                ? "PASS explicit per-summon mana costs split 200/100/300"
                : "CHECK shared ladder still collapses summon mana costs";
        }

        private static string ResolveSummonRosterEffectVerdict(SummonRosterAuditRow[] rows)
        {
            if (rows.Length < 3)
            {
                return "REVIEW roster rows missing";
            }

            bool effectSplit = true;
            for (int tier = 0; tier < 3; tier++)
            {
                effectSplit &= rows[1].ProjectileSpeed[tier] > rows[0].ProjectileSpeed[tier];
                effectSplit &= rows[2].VolleyDamage[tier] > rows[0].VolleyDamage[tier];
                effectSplit &= rows[2].VolleyDamage[tier] > rows[1].VolleyDamage[tier];
                effectSplit &= rows[0].ScreenIntercepts[tier] > 0;
                effectSplit &= rows[1].ScreenIntercepts[tier] == 0;
                effectSplit &= rows[2].ScreenIntercepts[tier] == 0;
                effectSplit &= rows[2].ActorHealth[tier] > rows[0].ActorHealth[tier];
                effectSplit &= rows[0].ActorHealth[tier] > rows[1].ActorHealth[tier];
            }

            return effectSplit
                ? "PASS profile effect budgets split speed/screen/dragon-damage/health roles"
                : "REVIEW effect budgets still read interchangeable";
        }

        private static float[] CopyFirstThree(float[] values)
        {
            float[] copy = new float[3];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = values != null && values.Length > i ? values[i] : 0f;
            }

            return copy;
        }

        private static string FormatTierFloatReadout(float[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if (i > 0)
                {
                    builder.Append("/");
                }

                builder.Append(values != null && values.Length > i ? values[i].ToString("0.#") : "-");
            }

            return builder.ToString();
        }

        private static string FormatTierIntReadout(int[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if (i > 0)
                {
                    builder.Append("/");
                }

                builder.Append(values != null && values.Length > i ? values[i].ToString() : "-");
            }

            return builder.ToString();
        }

        private static string FormatTierStringReadout(string[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if (i > 0)
                {
                    builder.Append("/");
                }

                builder.Append(values != null && values.Length > i ? values[i] : "-");
            }

            return builder.ToString();
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

        private static string FormatEnergyProbeTargetTier(PolicyMetrics result)
        {
            return result.EnergyProbeTargetTier > 0 ? result.EnergyProbeTargetTier.ToString() : "-";
        }

        private static string FormatOptionalDistance(float value)
        {
            return value >= 0f ? value.ToString("0.00") : "-";
        }

        private static string FormatOptionalInt(int value)
        {
            return value >= 0 ? value.ToString() : "-";
        }

        private static string FormatOptionalFloat(float value)
        {
            return value >= 0f ? value.ToString("0.0") : "-";
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
                builder.AppendLine($"      \"supportSummonSlotId\": \"{JsonEscape(result.SupportSummonSlotId)}\",");
                builder.AppendLine($"      \"supportSummonRequiredMana\": {result.SupportSummonRequiredMana:0.###},");
                builder.AppendLine($"      \"supportChoiceForecastReadoutBeforeSupport\": \"{JsonEscape(result.SupportChoiceForecastReadoutBeforeSupport)}\",");
                builder.AppendLine($"      \"supportSummonCooldownSeconds\": {result.SupportSummonCooldownSeconds:0.###},");
                builder.AppendLine($"      \"supportSummonSpentTier\": {result.SupportSummonSpentTier},");
                builder.AppendLine($"      \"supportSummonActorRoleId\": \"{JsonEscape(result.SupportSummonActorRoleId)}\",");
                builder.AppendLine($"      \"supportSummonVolleyWaves\": {result.SupportSummonVolleyWaves},");
                builder.AppendLine($"      \"supportSummonBlocks\": {result.SupportSummonBlocks},");
                builder.AppendLine($"      \"supportSummonMaxActiveActors\": {result.SupportSummonMaxActiveActors},");
                builder.AppendLine($"      \"supportSummonActorHealthRatio\": {result.SupportSummonActorHealthRatio:0.###},");
                builder.AppendLine($"      \"supportSummonProjectileHits\": {result.SupportSummonProjectileHits},");
                builder.AppendLine($"      \"supportSummonProjectileBossHits\": {result.SupportSummonProjectileBossHits},");
                builder.AppendLine($"      \"supportSummonProjectileEnemySummonHits\": {result.SupportSummonProjectileEnemySummonHits},");
                builder.AppendLine($"      \"supportSummonProjectileEnemyBodyHits\": {result.SupportSummonProjectileEnemyBodyHits},");
                builder.AppendLine($"      \"supportSummonProjectileOtherHits\": {result.SupportSummonProjectileOtherHits},");
                builder.AppendLine($"      \"supportComboSlot1RequiredMana\": {result.SupportComboSlot1RequiredMana:0.###},");
                builder.AppendLine($"      \"supportComboManaAfterSupport\": {result.SupportComboManaAfterSupport:0.###},");
                builder.AppendLine($"      \"supportComboManaBeforeSlot1\": {result.SupportComboManaBeforeSlot1:0.###},");
                builder.AppendLine($"      \"supportComboSupportCooldownAfterSupport\": {result.SupportComboSupportCooldownAfterSupport:0.###},");
                builder.AppendLine($"      \"supportComboSupportCooldownBeforeSlot1\": {result.SupportComboSupportCooldownBeforeSlot1:0.###},");
                builder.AppendLine($"      \"supportComboSlot1CooldownBeforeAttempt\": {result.SupportComboSlot1CooldownBeforeAttempt:0.###},");
                builder.AppendLine($"      \"supportComboSlot1CooldownAfterAttempt\": {result.SupportComboSlot1CooldownAfterAttempt:0.###},");
                builder.AppendLine($"      \"supportComboSlot1Attempted\": {JsonBool(result.SupportComboSlot1Attempted)},");
                builder.AppendLine($"      \"supportComboSlot1Used\": {JsonBool(result.SupportComboSlot1Used)},");
                builder.AppendLine($"      \"supportComboSlot1BlockedReason\": \"{JsonEscape(result.SupportComboSlot1BlockedReason)}\",");
                builder.AppendLine($"      \"supportComboManaAfterSlot1\": {result.SupportComboManaAfterSlot1:0.###},");
                builder.AppendLine($"      \"supportComboSlot1ReadyDelaySeconds\": {JsonNullableSeconds(result.SupportComboSlot1ReadyDelaySeconds)},");
                builder.AppendLine($"      \"supportComboPlayerDamageBeforeSlot1\": {result.SupportComboPlayerDamageBeforeSlot1:0.###},");
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
                builder.AppendLine($"      \"energyProbeTargetTier\": {result.EnergyProbeTargetTier},");
                builder.AppendLine($"      \"energyProbeStartAtSeconds\": {JsonNullableSeconds(result.EnergyProbeStartAtSeconds)},");
                builder.AppendLine($"      \"energyTier1ReadyAtSeconds\": {JsonNullableSeconds(result.EnergyTier1ReadyAtSeconds)},");
                builder.AppendLine($"      \"energyTier2ReadyAtSeconds\": {JsonNullableSeconds(result.EnergyTier2ReadyAtSeconds)},");
                builder.AppendLine($"      \"energyTier3ReadyAtSeconds\": {JsonNullableSeconds(result.EnergyTier3ReadyAtSeconds)},");
                builder.AppendLine($"      \"summonManaCostTier1\": {result.SummonManaCostTier1:0.###},");
                builder.AppendLine($"      \"summonManaCostTier2\": {result.SummonManaCostTier2:0.###},");
                builder.AppendLine($"      \"summonManaCostTier3\": {result.SummonManaCostTier3:0.###},");
                builder.AppendLine($"      \"energyTier1DurationSeconds\": {JsonNullableSeconds(result.EnergyTier1DurationSeconds)},");
                builder.AppendLine($"      \"energyTier2DurationSeconds\": {JsonNullableSeconds(result.EnergyTier2DurationSeconds)},");
                builder.AppendLine($"      \"energyTier3DurationSeconds\": {JsonNullableSeconds(result.EnergyTier3DurationSeconds)},");
                builder.AppendLine($"      \"energyProbeElapsedSeconds\": {JsonNullableSeconds(result.EnergyProbeElapsedSeconds)},");
                builder.AppendLine($"      \"energyProbePlayerDamagePerSecond\": {result.EnergyProbePlayerDamagePerSecond:0.###},");
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
                builder.AppendLine($"      \"bossScreenSuppressedByFollowup\": {JsonBool(result.BossScreenSuppressedByFollowup)},");
                builder.AppendLine($"      \"bossPressureScreensSuppressedByFollowup\": {result.BossPressureScreensSuppressedByFollowup},");
                builder.AppendLine($"      \"highestBossScreenSuppressSummonTier\": {result.HighestBossScreenSuppressSummonTier},");
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
                builder.AppendLine($"      \"followupSuppressScreenCueRequests\": {result.FollowupSuppressScreenCueRequests},");
                builder.AppendLine($"      \"lastFollowupScreenCueId\": \"{JsonEscape(result.LastFollowupScreenCueId)}\",");
                builder.AppendLine($"      \"lastFollowupScreenCueIntensity\": {result.LastFollowupScreenCueIntensity:0.###},");
                builder.AppendLine($"      \"lastFollowupHitScreenCueIntensity\": {result.LastFollowupHitScreenCueIntensity:0.###},");
                builder.AppendLine($"      \"lastFollowupWindowRouteScale\": {result.LastFollowupWindowRouteScale:0.###},");
                builder.AppendLine($"      \"followupWindowCameraCueRequests\": {result.FollowupWindowCameraCueRequests},");
                builder.AppendLine($"      \"followupHitCameraCueRequests\": {result.FollowupHitCameraCueRequests},");
                builder.AppendLine($"      \"followupMissedCameraCueRequests\": {result.FollowupMissedCameraCueRequests},");
                builder.AppendLine($"      \"followupSuppressCameraCueRequests\": {result.FollowupSuppressCameraCueRequests},");
                builder.AppendLine($"      \"lastFollowupHitCameraTier\": {result.LastFollowupHitCameraTier},");
                builder.AppendLine($"      \"lastFollowupHitCameraDamage\": {result.LastFollowupHitCameraDamage:0.###},");
                builder.AppendLine($"      \"lastFollowupSuppressCameraTier\": {result.LastFollowupSuppressCameraTier},");
                builder.AppendLine($"      \"followupWindowVfxCueRequests\": {result.FollowupWindowVfxCueRequests},");
                builder.AppendLine($"      \"followupHitVfxCueRequests\": {result.FollowupHitVfxCueRequests},");
                builder.AppendLine($"      \"followupMissedVfxCueRequests\": {result.FollowupMissedVfxCueRequests},");
                builder.AppendLine($"      \"followupSuppressVfxCueRequests\": {result.FollowupSuppressVfxCueRequests},");
                builder.AppendLine($"      \"lastFollowupHitVfxTier\": {result.LastFollowupHitVfxTier},");
                builder.AppendLine($"      \"lastFollowupHitVfxDamage\": {result.LastFollowupHitVfxDamage:0.###},");
                builder.AppendLine($"      \"lastFollowupSuppressVfxTier\": {result.LastFollowupSuppressVfxTier},");
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
                builder.AppendLine($"      \"highestSummonSpentTier\": {result.HighestSummonSpentTier},");
                builder.AppendLine($"      \"highestSkill1SpentTier\": {result.HighestSkill1SpentTier},");
                builder.AppendLine($"      \"energyTargetDurationSeconds\": {JsonNullableSeconds(ResolveEnergyTargetDuration(result))},");
                builder.AppendLine($"      \"energySpendDecisionRead\": \"{JsonEscape(ResolveEnergySpendDecisionRead(result))}\",");
                builder.AppendLine($"      \"resultRecordRouteLabel\": \"{JsonEscape(result.ResultRecordRouteLabel)}\",");
                builder.AppendLine($"      \"resultRecordTitle\": \"{JsonEscape(result.ResultRecordTitle)}\",");
                builder.AppendLine($"      \"resultRecordSummary\": \"{JsonEscape(result.ResultRecordSummary)}\",");
                builder.AppendLine($"      \"resultRecordRewardHook\": \"{JsonEscape(result.ResultRecordRewardHook)}\",");
                builder.AppendLine($"      \"resultRecordNextObjective\": \"{JsonEscape(result.ResultRecordNextObjective)}\",");
                builder.AppendLine($"      \"resultRecordTokenId\": \"{JsonEscape(result.ResultRecordTokenId)}\",");
                builder.AppendLine($"      \"resultRecordNextStateHookId\": \"{JsonEscape(result.ResultRecordNextStateHookId)}\",");
                builder.AppendLine($"      \"resultRecordProofReadout\": \"{JsonEscape(result.ResultRecordProofReadout)}\",");
                builder.AppendLine($"      \"resultRecordDecision\": \"{JsonEscape(result.ResultRecordDecision)}\",");
                builder.AppendLine($"      \"resultRecordCounterWaveSource\": \"{JsonEscape(result.ResultRecordCounterWaveSource)}\",");
                builder.AppendLine($"      \"resultRecordStageState\": \"{JsonEscape(ResolveResultStageState(result))}\",");
                builder.AppendLine($"      \"resultRecordHookClass\": \"{JsonEscape(ResolveResultHookClass(result))}\",");
                builder.AppendLine($"      \"resultRecordReviewOnly\": {JsonBool(IsReviewOnlyResultHook(result))},");
                builder.AppendLine($"      \"resultOverlayTitle\": \"{JsonEscape(result.ResultOverlayTitle)}\",");
                builder.AppendLine($"      \"resultOverlaySummary\": \"{JsonEscape(result.ResultOverlaySummary)}\",");
                builder.AppendLine($"      \"resultOverlayRoute\": \"{JsonEscape(result.ResultOverlayRoute)}\",");
                builder.AppendLine($"      \"resultOverlayRewardHook\": \"{JsonEscape(result.ResultOverlayRewardHook)}\",");
                builder.AppendLine($"      \"resultOverlayNextObjective\": \"{JsonEscape(result.ResultOverlayNextObjective)}\",");
                builder.AppendLine($"      \"routeDecision\": \"{JsonEscape(result.RouteDecision)}\",");
                builder.AppendLine($"      \"completionReadout\": \"{JsonEscape(result.CompletionReadout)}\"");
                builder.Append("    }");
                builder.AppendLine(i + 1 < results.Count ? "," : string.Empty);
            }

            builder.AppendLine("  ],");
            builder.AppendLine("  \"summonRosterIdentityAudit\": [");
            SummonRosterAuditRow[] rosterRows = BuildSummonRosterAuditRows(results);
            for (int i = 0; i < rosterRows.Length; i++)
            {
                SummonRosterAuditRow row = rosterRows[i];
                builder.AppendLine("    {");
                builder.AppendLine($"      \"slot\": \"{JsonEscape(row.Slot)}\",");
                builder.AppendLine($"      \"actionId\": \"{JsonEscape(row.ActionId)}\",");
                builder.AppendLine($"      \"costSource\": \"{JsonEscape(row.CostSource)}\",");
                builder.AppendLine($"      \"minimumTier\": {row.MinimumTier},");
                builder.AppendLine($"      \"requiredMana\": {row.RequiredMana:0.###},");
                builder.AppendLine($"      \"tierCosts\": \"{JsonEscape(FormatTierFloatReadout(row.TierCosts))}\",");
                builder.AppendLine($"      \"roleIds\": \"{JsonEscape(FormatTierStringReadout(row.RoleIds))}\",");
                builder.AppendLine($"      \"volleyDamage\": \"{JsonEscape(FormatTierFloatReadout(row.VolleyDamage))}\",");
                builder.AppendLine($"      \"screenIntercepts\": \"{JsonEscape(FormatTierIntReadout(row.ScreenIntercepts))}\",");
                builder.AppendLine($"      \"actorHealth\": \"{JsonEscape(FormatTierFloatReadout(row.ActorHealth))}\",");
                builder.AppendLine($"      \"counterDamage\": \"{JsonEscape(FormatTierFloatReadout(row.CounterDamage))}\",");
                builder.AppendLine($"      \"readout\": \"{JsonEscape(row.Readout)}\"");
                builder.Append("    }");
                builder.AppendLine(i + 1 < rosterRows.Length ? "," : string.Empty);
            }

            builder.AppendLine("  ],");
            builder.AppendLine("  \"routeMotivationDominanceMatrix\": [");
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "Fast execution reference",
                RequireResult(results, PolicyKind.ForwardRiskPhysicalSummonPunishProbe),
                repeatabilityResults,
                true);
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "Guided clean loop",
                RequireResult(results, PolicyKind.IntendedRoute),
                repeatabilityResults,
                true);
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "Counter recovery safety",
                RequireResult(results, PolicyKind.BossScreenBlockCounterRecovery),
                repeatabilityResults,
                true);
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "High-tier suppress payoff",
                RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute),
                repeatabilityResults,
                true);
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "Slot2 full-bank support",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "Slot3 delayed line-hold",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonRouteMotivationDominanceRow(
                builder,
                "Slot3 retreat recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"highTierWaitAgencyMatrix\": [");
            AppendJsonHighTierWaitAgencyRow(
                builder,
                "LV3 measurement wait",
                RequireResult(results, PolicyKind.ForwardRiskEnergyTierLadderProbe),
                repeatabilityResults,
                true);
            AppendJsonHighTierWaitAgencyRow(
                builder,
                "Promoted S1 LV3 diagnostic",
                RequireResult(results, PolicyKind.ForwardRiskTier3DecisionRoute),
                repeatabilityResults,
                true);
            AppendJsonHighTierWaitAgencyRow(
                builder,
                "Slot2 LV2 now",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonHighTierWaitAgencyRow(
                builder,
                "Slot2 full-bank",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonHighTierWaitAgencyRow(
                builder,
                "Slot3 delayed payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonHighTierWaitAgencyRow(
                builder,
                "Slot3 retreat/recommit payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportDecisionMatrix\": [");
            AppendJsonSupportDecisionMatrixRow(
                builder,
                "Slot1 LV2 recovery",
                "200",
                "charge screen/counter opener",
                RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportDecisionMatrixRow(
                builder,
                "Slot2 full-bank combo",
                "300 -> 100 -> 200",
                "marksman suppresses enemy frontline",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportDecisionMatrixRow(
                builder,
                "Slot2 delayed recovery",
                "200 -> 100 -> recharge -> 200",
                "marksman suppresses enemy frontline",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportDecisionMatrixRow(
                builder,
                "Slot3 immediate lockout",
                "300 -> 0",
                "vanguard holds physical line",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportDecisionMatrixRow(
                builder,
                "Slot3 delayed recovery",
                "300 -> recharge -> 200",
                "vanguard holds physical line",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportPayoffVectorMatrix\": [");
            AppendJsonSupportPayoffVectorRow(
                builder,
                "Slot1 LV2 recovery",
                RequireResult(results, PolicyKind.ForwardRiskTier1RecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportPayoffVectorRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportPayoffVectorRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportPayoffVectorRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportPayoffVectorRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportBodyCostPhaseMatrix\": [");
            AppendJsonSupportBodyCostPhaseRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportBodyCostPhaseRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportBodyCostPhaseRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportBodyCostPhaseRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportWaitExposureMatrix\": [");
            AppendJsonSupportWaitExposureRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportWaitExposureRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportWaitExposureRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportWaitExposureRow(
                builder,
                "Slot3 delayed payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportUpgradeDeltaMatrix\": [");
            AppendJsonSupportUpgradeDeltaRow(
                builder,
                "Bank Slot2 to full LV3",
                "Slot2 LV2 now",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                "Slot2 full-bank",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportUpgradeDeltaRow(
                builder,
                "Upgrade from Slot2 LV2 to Slot3 LV3",
                "Slot2 LV2 now",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                "Slot3 LV3 payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportUpgradeDecisionReadoutMatrix\": [");
            AppendJsonSupportUpgradeDecisionReadoutRow(
                builder,
                "Bank Slot2 to full LV3",
                "Slot2 LV2 now",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                "Slot2 full-bank",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                repeatabilityResults,
                true);
            AppendJsonSupportUpgradeDecisionReadoutRow(
                builder,
                "Upgrade from Slot2 LV2 to Slot3 LV3",
                "Slot2 LV2 now",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                "Slot3 LV3 payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                repeatabilityResults,
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"supportStageSlotTimelineMatrix\": [");
            AppendJsonSupportStageSlotTimelineRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                true);
            AppendJsonSupportStageSlotTimelineRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                true);
            AppendJsonSupportStageSlotTimelineRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                true);
            AppendJsonSupportStageSlotTimelineRow(
                builder,
                "Slot3 delayed payoff",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"summonSlotReadinessCooldownMatrix\": [");
            AppendJsonSummonSlotReadinessCooldownRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                true);
            AppendJsonSummonSlotReadinessCooldownRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                true);
            AppendJsonSummonSlotReadinessCooldownRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                true);
            AppendJsonSummonSlotReadinessCooldownRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                false);
            builder.AppendLine("  ],");
            builder.AppendLine("  \"summonHudReadinessReadoutMatrix\": [");
            AppendJsonSummonHudReadinessReadoutRow(
                builder,
                "Slot2 full-bank combo",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute),
                true);
            AppendJsonSummonHudReadinessReadoutRow(
                builder,
                "Slot3 immediate lockout",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute),
                true);
            AppendJsonSummonHudReadinessReadoutRow(
                builder,
                "Slot2 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute),
                true);
            AppendJsonSummonHudReadinessReadoutRow(
                builder,
                "Slot3 delayed recovery",
                RequireResult(results, PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute),
                false);
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
                builder.AppendLine($"      \"supportSummonSlotId\": \"{JsonEscape(result.SupportSummonSlotId)}\",");
                builder.AppendLine($"      \"supportSummonBlocks\": {result.SupportSummonBlocks},");
                builder.AppendLine($"      \"supportSummonRequiredMana\": {result.SupportSummonRequiredMana:0.###},");
                builder.AppendLine($"      \"supportSummonCooldownSeconds\": {result.SupportSummonCooldownSeconds:0.###},");
                builder.AppendLine($"      \"supportSummonSpentTier\": {result.SupportSummonSpentTier},");
                builder.AppendLine($"      \"supportSummonProjectileHits\": {result.SupportSummonProjectileHits},");
                builder.AppendLine($"      \"supportSummonProjectileBossHits\": {result.SupportSummonProjectileBossHits},");
                builder.AppendLine($"      \"supportSummonProjectileEnemySummonHits\": {result.SupportSummonProjectileEnemySummonHits},");
                builder.AppendLine($"      \"supportSummonProjectileEnemyBodyHits\": {result.SupportSummonProjectileEnemyBodyHits},");
                builder.AppendLine($"      \"supportComboManaAfterSupport\": {result.SupportComboManaAfterSupport:0.###},");
                builder.AppendLine($"      \"supportComboManaBeforeSlot1\": {result.SupportComboManaBeforeSlot1:0.###},");
                builder.AppendLine($"      \"supportComboSupportCooldownAfterSupport\": {result.SupportComboSupportCooldownAfterSupport:0.###},");
                builder.AppendLine($"      \"supportComboSupportCooldownBeforeSlot1\": {result.SupportComboSupportCooldownBeforeSlot1:0.###},");
                builder.AppendLine($"      \"supportComboSlot1CooldownBeforeAttempt\": {result.SupportComboSlot1CooldownBeforeAttempt:0.###},");
                builder.AppendLine($"      \"supportComboSlot1CooldownAfterAttempt\": {result.SupportComboSlot1CooldownAfterAttempt:0.###},");
                builder.AppendLine($"      \"supportComboSlot1Used\": {JsonBool(result.SupportComboSlot1Used)},");
                builder.AppendLine($"      \"supportComboSlot1BlockedReason\": \"{JsonEscape(result.SupportComboSlot1BlockedReason)}\",");
                builder.AppendLine($"      \"supportComboManaAfterSlot1\": {result.SupportComboManaAfterSlot1:0.###},");
                builder.AppendLine($"      \"supportComboSlot1ReadyDelaySeconds\": {JsonNullableSeconds(result.SupportComboSlot1ReadyDelaySeconds)},");
                builder.AppendLine($"      \"supportComboPlayerDamageBeforeSlot1\": {result.SupportComboPlayerDamageBeforeSlot1:0.###},");
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

        private static void AppendJsonRouteMotivationDominanceRow(
            StringBuilder builder,
            string routeRole,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            PolicyKind policy = result.Policy;
            builder.AppendLine("    {");
            builder.AppendLine($"      \"routeRole\": \"{JsonEscape(routeRole)}\",");
            builder.AppendLine($"      \"policy\": \"{policy}\",");
            builder.AppendLine($"      \"costRisk\": \"{JsonEscape(FormatRouteMotivationCostRisk(result))}\",");
            builder.AppendLine($"      \"samplePayoff\": \"{JsonEscape(FormatRouteMotivationPayoff(result))}\",");
            builder.AppendLine($"      \"payoff\": \"{JsonEscape(FormatRouteMotivationPayoff(result))}\",");
            builder.AppendLine($"      \"resultHook\": \"{JsonEscape(ResolveResultHookClass(result))}\",");
            builder.AppendLine($"      \"repeatSignal\": \"{JsonEscape(FormatRouteMotivationRepeatSignal(repeatabilityResults, policy))}\",");
            builder.AppendLine($"      \"repeatRuns\": {CountPolicyResults(repeatabilityResults, policy)},");
            builder.AppendLine($"      \"repeatVerdict\": \"{JsonEscape(ResolveRepeatabilityVerdict(repeatabilityResults, policy))}\",");
            builder.AppendLine($"      \"repeatElapsedSecondsMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, repeated => repeated.ElapsedSeconds))},");
            builder.AppendLine($"      \"repeatElapsedSecondsAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.ElapsedSeconds))},");
            builder.AppendLine($"      \"repeatElapsedSecondsMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, repeated => repeated.ElapsedSeconds))},");
            builder.AppendLine($"      \"repeatPlayerDamageMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, repeated => repeated.PlayerDamageTaken))},");
            builder.AppendLine($"      \"repeatPlayerDamageAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.PlayerDamageTaken))},");
            builder.AppendLine($"      \"repeatPlayerDamageMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, repeated => repeated.PlayerDamageTaken))},");
            builder.AppendLine($"      \"repeatBossDamageMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"repeatBossDamageAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"repeatBossDamageMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"decisionRead\": \"{JsonEscape(ResolveRouteMotivationDecisionRead(result))}\",");
            builder.AppendLine($"      \"elapsedSeconds\": {result.ElapsedSeconds:0.###},");
            builder.AppendLine($"      \"playerDamageTaken\": {result.PlayerDamageTaken:0.###},");
            builder.AppendLine($"      \"bossDamageTaken\": {result.BossDamageTaken:0.###},");
            builder.AppendLine($"      \"bossDamageFromPlayer\": {result.BossDamageFromPlayer:0.###},");
            builder.AppendLine($"      \"bossDamageFromAllySummon\": {result.BossDamageFromAllySummon:0.###},");
            builder.AppendLine($"      \"summonBlocks\": {result.SummonBlocks},");
            builder.AppendLine($"      \"counterWaves\": {result.CounterWaves},");
            builder.AppendLine($"      \"bossPressureScreensSuppressedByFollowup\": {result.BossPressureScreensSuppressedByFollowup},");
            builder.AppendLine($"      \"supportSummonSlotId\": \"{JsonEscape(result.SupportSummonSlotId)}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportDecisionMatrixRow(
            StringBuilder builder,
            string choice,
            string costPath,
            string supportEffect,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            PolicyKind policy = result.Policy;
            builder.AppendLine("    {");
            builder.AppendLine($"      \"choice\": \"{JsonEscape(choice)}\",");
            builder.AppendLine($"      \"policy\": \"{policy}\",");
            builder.AppendLine($"      \"costPath\": \"{JsonEscape(costPath)}\",");
            builder.AppendLine($"      \"supportEffect\": \"{JsonEscape(supportEffect)}\",");
            builder.AppendLine($"      \"sampleTimeDamage\": \"{JsonEscape(FormatSupportDecisionTimeDamage(result))}\",");
            builder.AppendLine($"      \"repeatSignal\": \"{JsonEscape(FormatSupportDecisionRepeatSignal(repeatabilityResults, policy))}\",");
            builder.AppendLine($"      \"repeatRuns\": {CountPolicyResults(repeatabilityResults, policy)},");
            builder.AppendLine($"      \"repeatVerdict\": \"{JsonEscape(ResolveRepeatabilityVerdict(repeatabilityResults, policy))}\",");
            builder.AppendLine($"      \"repeatElapsedSecondsMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, repeated => repeated.ElapsedSeconds))},");
            builder.AppendLine($"      \"repeatElapsedSecondsAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.ElapsedSeconds))},");
            builder.AppendLine($"      \"repeatElapsedSecondsMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, repeated => repeated.ElapsedSeconds))},");
            builder.AppendLine($"      \"repeatHpBeforeMainMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, repeated => ResolveSupportDecisionHpBeforeMain(repeated)))},");
            builder.AppendLine($"      \"repeatHpBeforeMainAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => ResolveSupportDecisionHpBeforeMain(repeated)))},");
            builder.AppendLine($"      \"repeatHpBeforeMainMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, repeated => ResolveSupportDecisionHpBeforeMain(repeated)))},");
            builder.AppendLine($"      \"repeatBossDamageMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"repeatBossDamageAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"repeatBossDamageMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"hpBeforeMain\": {ResolveSupportDecisionHpBeforeMain(result):0.###},");
            builder.AppendLine($"      \"physicalBarragePlayerHits\": {result.PhysicalBarragePlayerHits},");
            builder.AppendLine($"      \"physicalBarrageTrackedProjectileCount\": {result.PhysicalBarrageTrackedProjectileCount},");
            builder.AppendLine($"      \"bossSuppress\": \"{JsonEscape(FormatSupportDecisionBossSuppress(result))}\",");
            builder.AppendLine($"      \"result\": \"{JsonEscape(result.ResultKind)}\",");
            builder.AppendLine($"      \"firstUnresolvedBeat\": \"{JsonEscape(ResolveFirstUnresolvedBeat(result))}\",");
            builder.AppendLine($"      \"resultHookClass\": \"{JsonEscape(ResolveResultHookClass(result))}\",");
            builder.AppendLine($"      \"timingVerdict\": \"{JsonEscape(ResolveSupportDecisionTimingVerdict(result))}\",");
            builder.AppendLine($"      \"payoffVerdict\": \"{JsonEscape(ResolveSupportDecisionPayoffVerdict(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonHighTierWaitAgencyRow(
            StringBuilder builder,
            string waitRoute,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            builder.AppendLine("    {");
            builder.AppendLine($"      \"waitRoute\": \"{JsonEscape(waitRoute)}\",");
            builder.AppendLine($"      \"policy\": \"{result.Policy}\",");
            builder.AppendLine($"      \"target\": \"{JsonEscape(FormatHighTierWaitAgencyTarget(result))}\",");
            builder.AppendLine($"      \"waitSeconds\": {JsonNullableMetric(ResolveHighTierWaitAgencySeconds(result))},");
            builder.AppendLine($"      \"waitDamage\": {JsonNullableMetric(ResolveHighTierWaitAgencyDamage(result))},");
            builder.AppendLine($"      \"waitDamagePerSecond\": {ResolveHighTierWaitAgencyDamagePerSecond(result):0.###},");
            builder.AppendLine($"      \"pressureCost\": \"{JsonEscape(FormatHighTierWaitAgencyPressureCost(result))}\",");
            builder.AppendLine($"      \"visibleSignalBeforeSpend\": \"{JsonEscape(FormatHighTierWaitAgencySignal(result))}\",");
            builder.AppendLine($"      \"payoffAfterSpend\": \"{JsonEscape(FormatHighTierWaitAgencyPayoff(result))}\",");
            builder.AppendLine($"      \"repeatCheck\": \"{JsonEscape(FormatHighTierWaitAgencyRepeat(repeatabilityResults, result.Policy))}\",");
            builder.AppendLine($"      \"agencyRead\": \"{JsonEscape(ResolveHighTierWaitAgencyRead(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportPayoffVectorRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            PolicyKind policy = result.Policy;
            builder.AppendLine("    {");
            builder.AppendLine($"      \"choice\": \"{JsonEscape(choice)}\",");
            builder.AppendLine($"      \"policy\": \"{policy}\",");
            builder.AppendLine($"      \"sampleDamageVector\": \"{JsonEscape(FormatSupportPayoffDamageVector(result))}\",");
            builder.AppendLine($"      \"samplePreventionVector\": \"{JsonEscape(FormatSupportPayoffPreventionVector(result))}\",");
            builder.AppendLine($"      \"sampleRelockCost\": \"{JsonEscape(FormatSupportPayoffRelockCost(result))}\",");
            builder.AppendLine($"      \"repeatDamageBand\": \"{JsonEscape(FormatSupportPayoffRepeatDamageBand(repeatabilityResults, policy))}\",");
            builder.AppendLine($"      \"repeatPreventionBand\": \"{JsonEscape(FormatSupportPayoffRepeatPreventionBand(repeatabilityResults, policy))}\",");
            builder.AppendLine($"      \"payoffRead\": \"{JsonEscape(ResolveSupportPayoffVectorRead(result))}\",");
            builder.AppendLine($"      \"bossDamageTaken\": {result.BossDamageTaken:0.###},");
            builder.AppendLine($"      \"bossDamageFromPlayer\": {result.BossDamageFromPlayer:0.###},");
            builder.AppendLine($"      \"bossDamageFromAllySummon\": {result.BossDamageFromAllySummon:0.###},");
            builder.AppendLine($"      \"supportSummonBlocks\": {result.SupportSummonBlocks},");
            builder.AppendLine($"      \"supportSummonProjectileEnemySummonHits\": {result.SupportSummonProjectileEnemySummonHits},");
            builder.AppendLine($"      \"supportSummonProjectileBossHits\": {result.SupportSummonProjectileBossHits},");
            builder.AppendLine($"      \"enemyFrontlineBodyHits\": {result.EnemyFrontlineBodyHits},");
            builder.AppendLine($"      \"bossPressureScreensSuppressedByFollowup\": {result.BossPressureScreensSuppressedByFollowup},");
            builder.AppendLine($"      \"counterWaves\": {result.CounterWaves},");
            builder.AppendLine($"      \"repeatBossDamageAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.BossDamageTaken))},");
            builder.AppendLine($"      \"repeatAllyBossDamageAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.BossDamageFromAllySummon))},");
            builder.AppendLine($"      \"repeatSupportEnemyHitsAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.SupportSummonProjectileEnemySummonHits))},");
            builder.AppendLine($"      \"repeatSupportBlocksAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.SupportSummonBlocks))},");
            builder.AppendLine($"      \"repeatBossSuppressAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.BossPressureScreensSuppressedByFollowup))},");
            builder.AppendLine($"      \"repeatCounterWavesAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.CounterWaves))}");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportBodyCostPhaseRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            PolicyKind policy = result.Policy;
            builder.AppendLine("    {");
            builder.AppendLine($"      \"choice\": \"{JsonEscape(choice)}\",");
            builder.AppendLine($"      \"policy\": \"{policy}\",");
            builder.AppendLine($"      \"bodyHitsBeforeSupport\": {JsonNullableMetric(result.SupportBodyHitsBeforeSupport)},");
            builder.AppendLine($"      \"bodyHitsBeforeMainAnswer\": {JsonNullableMetric(result.SupportBodyHitsBeforeMainAnswer)},");
            builder.AppendLine($"      \"bodyHitsFinal\": {JsonNullableMetric(result.SupportBodyHitsFinal)},");
            builder.AppendLine($"      \"bodyHitsAfterSupportDelta\": {JsonNullableMetric(ResolveSupportBodyHitsAfterSupportDelta(result))},");
            builder.AppendLine($"      \"damageBeforeSupport\": {JsonNullableMetric(result.SupportDamageBeforeSupport)},");
            builder.AppendLine($"      \"damageBeforeMainAnswer\": {JsonNullableMetric(result.SupportDamageBeforeMainAnswer)},");
            builder.AppendLine($"      \"damageFinal\": {JsonNullableMetric(result.SupportDamageFinal)},");
            builder.AppendLine($"      \"repeatBodyHitsFinalAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.SupportBodyHitsFinal))},");
            builder.AppendLine($"      \"repeatBodyHitsAfterSupportDeltaAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, ResolveSupportBodyHitsAfterSupportDelta))},");
            builder.AppendLine($"      \"repeatDamageFinalAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.SupportDamageFinal))},");
            builder.AppendLine($"      \"phaseRead\": \"{JsonEscape(ResolveSupportBodyCostPhaseRead(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportWaitExposureRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            PolicyKind policy = result.Policy;
            builder.AppendLine("    {");
            builder.AppendLine($"      \"choice\": \"{JsonEscape(choice)}\",");
            builder.AppendLine($"      \"policy\": \"{policy}\",");
            builder.AppendLine($"      \"supportSlot\": \"{JsonEscape(result.SupportSummonSlotId)}\",");
            builder.AppendLine($"      \"targetTier\": {ResolveSupportWaitExposureTargetTier(result)},");
            builder.AppendLine($"      \"requiredMana\": {result.SupportSummonRequiredMana:0.###},");
            builder.AppendLine($"      \"waitToSupportSeconds\": {JsonNullableMetric(ResolveSupportWaitExposureSeconds(result))},");
            builder.AppendLine($"      \"bodyHitsBeforeSupport\": {JsonNullableMetric(result.SupportBodyHitsBeforeSupport)},");
            builder.AppendLine($"      \"damageBeforeSupport\": {JsonNullableMetric(result.SupportDamageBeforeSupport)},");
            builder.AppendLine($"      \"mainAnswerDelaySeconds\": {JsonNullableMetric(result.SupportComboSlot1ReadyDelaySeconds)},");
            builder.AppendLine($"      \"mainAnswerGate\": \"{JsonEscape(FormatSupportWaitExposureMainAnswerGate(result))}\",");
            builder.AppendLine($"      \"payoffVerdict\": \"{JsonEscape(ResolveSupportDecisionPayoffVerdict(result))}\",");
            builder.AppendLine($"      \"resultHookClass\": \"{JsonEscape(ResolveResultHookClass(result))}\",");
            builder.AppendLine($"      \"result\": \"{JsonEscape(result.ResultKind)}\",");
            builder.AppendLine($"      \"firstUnresolvedBeat\": \"{JsonEscape(ResolveFirstUnresolvedBeat(result))}\",");
            builder.AppendLine($"      \"repeatWaitSecondsMin\": {JsonNullableMetric(MinMetric(repeatabilityResults, policy, ResolveSupportWaitExposureSeconds))},");
            builder.AppendLine($"      \"repeatWaitSecondsAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, ResolveSupportWaitExposureSeconds))},");
            builder.AppendLine($"      \"repeatWaitSecondsMax\": {JsonNullableMetric(MaxMetric(repeatabilityResults, policy, ResolveSupportWaitExposureSeconds))},");
            builder.AppendLine($"      \"repeatBodyHitsBeforeSupportAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.SupportBodyHitsBeforeSupport))},");
            builder.AppendLine($"      \"repeatDamageBeforeSupportAverage\": {JsonNullableMetric(AverageMetric(repeatabilityResults, policy, repeated => repeated.SupportDamageBeforeSupport))},");
            builder.AppendLine($"      \"readout\": \"{JsonEscape(ResolveSupportWaitExposureRead(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportUpgradeDeltaRow(
            StringBuilder builder,
            string deltaLabel,
            string fromChoice,
            PolicyMetrics from,
            string toChoice,
            PolicyMetrics to,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            builder.AppendLine("    {");
            builder.AppendLine($"      \"deltaLabel\": \"{JsonEscape(deltaLabel)}\",");
            builder.AppendLine($"      \"fromChoice\": \"{JsonEscape(fromChoice)}\",");
            builder.AppendLine($"      \"fromPolicy\": \"{from.Policy}\",");
            builder.AppendLine($"      \"fromTarget\": \"{JsonEscape(FormatSupportWaitExposureTarget(from))}\",");
            builder.AppendLine($"      \"toChoice\": \"{JsonEscape(toChoice)}\",");
            builder.AppendLine($"      \"toPolicy\": \"{to.Policy}\",");
            builder.AppendLine($"      \"toTarget\": \"{JsonEscape(FormatSupportWaitExposureTarget(to))}\",");
            builder.AppendLine($"      \"extraWaitSeconds\": {JsonSignedMetric(ResolveSupportWaitDeltaSeconds(from, to))},");
            builder.AppendLine($"      \"extraBodyHitsBeforeSupport\": {JsonSignedMetric(ResolveSupportPreSupportBodyDelta(from, to))},");
            builder.AppendLine($"      \"extraDamageBeforeSupport\": {JsonSignedMetric(ResolveSupportPreSupportDamageDelta(from, to))},");
            builder.AppendLine($"      \"mainAnswerDelayDeltaSeconds\": {JsonSignedMetric(ResolveSupportMainAnswerDelayDelta(from, to))},");
            builder.AppendLine($"      \"fromPayoffVerdict\": \"{JsonEscape(ResolveSupportDecisionPayoffVerdict(from))}\",");
            builder.AppendLine($"      \"toPayoffVerdict\": \"{JsonEscape(ResolveSupportDecisionPayoffVerdict(to))}\",");
            builder.AppendLine($"      \"fromResultHookClass\": \"{JsonEscape(ResolveResultHookClass(from))}\",");
            builder.AppendLine($"      \"toResultHookClass\": \"{JsonEscape(ResolveResultHookClass(to))}\",");
            builder.AppendLine($"      \"repeatExtraWaitSecondsAverage\": {JsonSignedMetric(ResolveAverageSupportUpgradeDelta(repeatabilityResults, from.Policy, to.Policy, ResolveSupportWaitExposureSeconds))},");
            builder.AppendLine($"      \"repeatExtraBodyHitsBeforeSupportAverage\": {JsonSignedMetric(ResolveAverageSupportUpgradeDelta(repeatabilityResults, from.Policy, to.Policy, result => result.SupportBodyHitsBeforeSupport))},");
            builder.AppendLine($"      \"repeatExtraDamageBeforeSupportAverage\": {JsonSignedMetric(ResolveAverageSupportUpgradeDelta(repeatabilityResults, from.Policy, to.Policy, result => result.SupportDamageBeforeSupport))},");
            builder.AppendLine($"      \"readout\": \"{JsonEscape(ResolveSupportUpgradeDeltaRead(from, to))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportUpgradeDecisionReadoutRow(
            StringBuilder builder,
            string decision,
            string fromChoice,
            PolicyMetrics from,
            string toChoice,
            PolicyMetrics to,
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            bool appendComma)
        {
            builder.AppendLine("    {");
            builder.AppendLine($"      \"decision\": \"{JsonEscape(decision)}\",");
            builder.AppendLine($"      \"fromChoice\": \"{JsonEscape(fromChoice)}\",");
            builder.AppendLine($"      \"fromPolicy\": \"{from.Policy}\",");
            builder.AppendLine($"      \"fromVisibleState\": \"{JsonEscape(FormatSupportUpgradeDecisionHudState(fromChoice, from))}\",");
            builder.AppendLine($"      \"fromForecastBeforeSupport\": \"{JsonEscape(from.SupportChoiceForecastReadoutBeforeSupport)}\",");
            builder.AppendLine($"      \"toChoice\": \"{JsonEscape(toChoice)}\",");
            builder.AppendLine($"      \"toPolicy\": \"{to.Policy}\",");
            builder.AppendLine($"      \"toVisibleState\": \"{JsonEscape(FormatSupportUpgradeDecisionHudState(toChoice, to))}\",");
            builder.AppendLine($"      \"toForecastBeforeSupport\": \"{JsonEscape(to.SupportChoiceForecastReadoutBeforeSupport)}\",");
            builder.AppendLine($"      \"measuredExtraCost\": \"{JsonEscape(FormatSupportUpgradeDecisionMeasuredCost(from, to))}\",");
            builder.AppendLine($"      \"measuredPayoffShift\": \"{JsonEscape(FormatSupportUpgradeDecisionMeasuredPayoff(from, to))}\",");
            builder.AppendLine($"      \"repeatDelta\": \"{JsonEscape(FormatSupportUpgradeRepeatDelta(repeatabilityResults, from.Policy, to.Policy))}\",");
            builder.AppendLine($"      \"decisionReadout\": \"{JsonEscape(ResolveSupportUpgradeDecisionReadoutRead(from, to))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static float ResolveAverageSupportUpgradeDelta(
            IReadOnlyList<PolicyMetrics> repeatabilityResults,
            PolicyKind fromPolicy,
            PolicyKind toPolicy,
            Func<PolicyMetrics, float> selector)
        {
            float fromAverage = AverageMetric(repeatabilityResults, fromPolicy, selector);
            float toAverage = AverageMetric(repeatabilityResults, toPolicy, selector);
            return fromAverage >= 0f && toAverage >= 0f ? toAverage - fromAverage : -1f;
        }

        private static void AppendJsonSummonSlotReadinessCooldownRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            bool appendComma)
        {
            builder.AppendLine("    {");
            builder.AppendLine($"      \"choice\": \"{JsonEscape(choice)}\",");
            builder.AppendLine($"      \"policy\": \"{result.Policy}\",");
            builder.AppendLine($"      \"supportSlot\": \"{JsonEscape(result.SupportSummonSlotId)}\",");
            builder.AppendLine($"      \"supportRequiredMana\": {result.SupportSummonRequiredMana:0.###},");
            builder.AppendLine($"      \"supportCooldownSeconds\": {result.SupportSummonCooldownSeconds:0.###},");
            builder.AppendLine($"      \"supportCooldownAfterSupport\": {result.SupportComboSupportCooldownAfterSupport:0.###},");
            builder.AppendLine($"      \"supportCooldownBeforeSlot1\": {result.SupportComboSupportCooldownBeforeSlot1:0.###},");
            builder.AppendLine($"      \"slot1RequiredMana\": {result.SupportComboSlot1RequiredMana:0.###},");
            builder.AppendLine($"      \"slot1CooldownBeforeAttempt\": {result.SupportComboSlot1CooldownBeforeAttempt:0.###},");
            builder.AppendLine($"      \"slot1CooldownAfterAttempt\": {result.SupportComboSlot1CooldownAfterAttempt:0.###},");
            builder.AppendLine($"      \"manaAfterSupport\": {result.SupportComboManaAfterSupport:0.###},");
            builder.AppendLine($"      \"manaBeforeSlot1\": {result.SupportComboManaBeforeSlot1:0.###},");
            builder.AppendLine($"      \"slot1UseState\": \"{JsonEscape(FormatSupportComboSlot1UseState(result))}\",");
            builder.AppendLine($"      \"result\": \"{JsonEscape(result.ResultKind)}\",");
            builder.AppendLine($"      \"firstUnresolvedBeat\": \"{JsonEscape(ResolveFirstUnresolvedBeat(result))}\",");
            builder.AppendLine($"      \"readout\": \"{JsonEscape(ResolveSummonSlotReadinessCooldownRead(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSupportStageSlotTimelineRow(
            StringBuilder builder,
            string route,
            PolicyMetrics result,
            bool appendComma)
        {
            builder.AppendLine("    {");
            builder.AppendLine($"      \"route\": \"{JsonEscape(route)}\",");
            builder.AppendLine($"      \"policy\": \"{result.Policy}\",");
            builder.AppendLine($"      \"supportSlot\": \"{JsonEscape(result.SupportSummonSlotId)}\",");
            builder.AppendLine($"      \"supportRequiredMana\": {result.SupportSummonRequiredMana:0.###},");
            builder.AppendLine($"      \"manaAfterSupport\": {result.SupportComboManaAfterSupport:0.###},");
            builder.AppendLine($"      \"supportEffectSlot\": \"{JsonEscape(FormatSupportStageEffectSlot(result))}\",");
            builder.AppendLine($"      \"mainAnswerGate\": \"{JsonEscape(FormatSupportStageMainAnswerGate(result))}\",");
            builder.AppendLine($"      \"relockPayoffSlot\": \"{JsonEscape(FormatSupportStageRelockPayoffSlot(result))}\",");
            builder.AppendLine($"      \"resultHookClass\": \"{JsonEscape(ResolveResultHookClass(result))}\",");
            builder.AppendLine($"      \"result\": \"{JsonEscape(result.ResultKind)}\",");
            builder.AppendLine($"      \"firstUnresolvedBeat\": \"{JsonEscape(ResolveFirstUnresolvedBeat(result))}\",");
            builder.AppendLine($"      \"decisionRead\": \"{JsonEscape(ResolveSupportStageDecisionRead(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AppendJsonSummonHudReadinessReadoutRow(
            StringBuilder builder,
            string choice,
            PolicyMetrics result,
            bool appendComma)
        {
            builder.AppendLine("    {");
            builder.AppendLine($"      \"choice\": \"{JsonEscape(choice)}\",");
            builder.AppendLine($"      \"policy\": \"{result.Policy}\",");
            builder.AppendLine($"      \"supportLabelBeforeSlot1\": \"{JsonEscape(result.SupportComboHudSupportLabelBeforeSlot1)}\",");
            builder.AppendLine($"      \"supportFillBeforeSlot1\": {result.SupportComboHudSupportFillBeforeSlot1:0.###},");
            builder.AppendLine($"      \"supportReadyPulseBeforeSlot1\": {JsonBool(result.SupportComboHudSupportFillBeforeSlot1 >= 0.995f)},");
            builder.AppendLine($"      \"slot1LabelBeforeAttempt\": \"{JsonEscape(result.SupportComboHudSlot1LabelBeforeAttempt)}\",");
            builder.AppendLine($"      \"slot1FillBeforeAttempt\": {result.SupportComboHudSlot1FillBeforeAttempt:0.###},");
            builder.AppendLine($"      \"slot1ReadyPulseBeforeAttempt\": {JsonBool(result.SupportComboHudSlot1FillBeforeAttempt >= 0.995f)},");
            builder.AppendLine($"      \"overlayReadoutBeforeSlot1\": \"{JsonEscape(result.SupportComboOverlayHudReadoutBeforeSlot1)}\",");
            builder.AppendLine($"      \"readout\": \"{JsonEscape(ResolveSummonHudReadinessRead(result))}\"");
            builder.Append("    }");
            builder.AppendLine(appendComma ? "," : string.Empty);
        }

        private static void AssertRepeatabilityGate(IReadOnlyList<PolicyMetrics> repeatabilityResults)
        {
            for (int i = 0; i < RequiredRepeatabilityGatePolicyOrder.Length; i++)
            {
                PolicyKind policy = RequiredRepeatabilityGatePolicyOrder[i];
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
            Assert.Greater(
                MinMetric(
                    repeatabilityResults,
                    PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route,
                    result => result.FollowupHitCinematicCueRequests),
                0f,
                "Repeated Slot3 delayed payoff runs should preserve the follow-up hit micro-cinematic after the high-cost cut-in.");
            Assert.LessOrEqual(
                MaxMetric(
                    repeatabilityResults,
                    PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route,
                    result => result.FollowupHitSequenceBridgeRequests),
                0f,
                "Repeated Slot3 delayed payoff runs should stay on the director micro-cue path instead of full sequence playback.");
        }

        private static void AssertEnergyDecisionRoute(PolicyMetrics result, int expectedTier)
        {
            Assert.AreEqual(
                expectedTier,
                result.EnergyProbeTargetTier,
                $"{result.Policy} should explicitly wait for the expected EN tier before spending.");
            Assert.GreaterOrEqual(
                ResolveEnergyTargetDuration(result),
                0f,
                $"{result.Policy} should record the target-tier ready time.");
            if (expectedTier < BossBarrageSummonReviewContract.Slot1MinimumTier)
            {
                Assert.AreEqual(
                    0,
                    result.HighestSummonSpentTier,
                    $"{result.Policy} should show SummonSlot1 is not spendable below the promoted minimum tier.");
                Assert.AreEqual(
                    0,
                    result.SummonBlocks,
                    $"{result.Policy} should not fabricate a summon block when LV{expectedTier} cannot pay SummonSlot1.");
                Assert.AreEqual(
                    0,
                    result.ResultRecords,
                    $"{result.Policy} should remain a measurement-only lockout branch below the promoted SummonSlot1 tier.");
                Assert.AreEqual(
                    "Running",
                    result.ResultKind,
                    $"{result.Policy} should remain running when LV{expectedTier} cannot pay SummonSlot1.");
                return;
            }

            int expectedSpentTier = BossBarrageSummonReviewContract.Slot1MinimumTier;
            Assert.AreEqual(
                expectedSpentTier,
                result.HighestSummonSpentTier,
                $"{result.Policy} should spend SummonSlot1 at its promoted cost tier after waiting for the target tier.");
            Assert.Greater(
                result.SummonBlocks,
                0,
                $"{result.Policy} should convert the physical boss barrage into a summon block.");
            Assert.AreEqual(
                0,
                result.PhysicalBarragePlayerHits,
                $"{result.Policy} should prevent player hits during the answered physical barrage.");
            Assert.Greater(
                result.SkillUses,
                0,
                $"{result.Policy} should spend Skill1 inside the opened follow-up window.");
            Assert.AreEqual(
                0,
                result.SkillProjectileHits,
                $"{result.Policy} should expose that the waited route does not land raw Skill1 boss damage.");
            Assert.Greater(
                result.CounterWaves,
                0,
                $"{result.Policy} should expose the next counter-answer state after the unconfirmed S1 spend.");
            Assert.AreEqual(
                "Running",
                result.ResultKind,
                $"{result.Policy} should remain unresolved instead of fabricating a clean result after an S1 spend.");
        }

        private static void AssertEnergyRecoveryRoute(PolicyMetrics result, int expectedTier)
        {
            Assert.AreEqual(
                expectedTier,
                result.EnergyProbeTargetTier,
                $"{result.Policy} should explicitly wait for the expected EN tier before spending.");
            Assert.GreaterOrEqual(
                ResolveEnergyTargetDuration(result),
                0f,
                $"{result.Policy} should record the target-tier ready time.");
            if (expectedTier < BossBarrageSummonReviewContract.Slot1MinimumTier)
            {
                Assert.AreEqual(
                    0,
                    result.HighestSummonSpentTier,
                    $"{result.Policy} should show recovery cannot start from SummonSlot1 below the promoted minimum tier.");
                Assert.AreEqual(
                    0,
                    result.SummonBlocks,
                    $"{result.Policy} should not fabricate recovery blocks when LV{expectedTier} cannot pay SummonSlot1.");
                Assert.AreEqual(
                    0,
                    result.ResultRecords,
                    $"{result.Policy} should stay uncommitted below the promoted SummonSlot1 tier.");
                Assert.AreEqual(
                    "Running",
                    result.ResultKind,
                    $"{result.Policy} should remain running when LV{expectedTier} cannot pay SummonSlot1 recovery.");
                return;
            }

            int expectedSpentTier = BossBarrageSummonReviewContract.Slot1MinimumTier;
            Assert.AreEqual(
                expectedSpentTier,
                result.HighestSummonSpentTier,
                $"{result.Policy} should spend the opening summon at its promoted cost tier before recovery.");
            Assert.GreaterOrEqual(
                result.SummonUses,
                2,
                $"{result.Policy} should use a fresh second summon to answer the counter branch.");
            Assert.GreaterOrEqual(
                result.FirstCounterWaveAtSeconds,
                0f,
                $"{result.Policy} should record the boss-screen counter wave timing.");
            Assert.Greater(
                result.FirstCounterAnswerSummonAtSeconds,
                -0.01f,
                $"{result.Policy} should record the fresh counter answer summon timing.");
            Assert.GreaterOrEqual(
                result.FirstCounterAnswerSummonAtSeconds,
                result.FirstCounterWaveAtSeconds,
                $"{result.Policy} should not answer before the boss-screen counter wave is observed.");
            Assert.AreEqual(
                "recorded",
                result.CounterWaveRecordState,
                $"{result.Policy} should record the counter branch that forced recovery.");
            Assert.AreEqual(
                "stabilized",
                result.CounterWaveAnswerState,
                $"{result.Policy} should stabilize the counter branch after the fresh summon answer.");
            Assert.AreEqual(
                "opened",
                result.CounterWaveFinalWindowState,
                $"{result.Policy} should open the final Skill1 window after the counter answer.");
            Assert.Greater(
                result.CounterWaves,
                0,
                $"{result.Policy} should expose the counter wave before recovery.");
            Assert.IsTrue(
                result.CounterRecoveryConfirmed,
                $"{result.Policy} should stabilize the counter branch after the fresh summon answer.");
            Assert.Greater(
                result.SkillProjectileHits,
                0,
                $"{result.Policy} should land the final Skill1 after the counter answer.");
            Assert.AreEqual(
                "CounterRecoveryClear",
                result.ResultKind,
                $"{result.Policy} should close as a counter recovery result, not a clean direct payoff.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(result),
                $"{result.Policy} should complete the stage beat after counter recovery.");
            Assert.Greater(
                result.ResultRecords,
                0,
                $"{result.Policy} should commit the recovered result hook.");
        }

        private static void AssertEnergyDirectPayoffRoute(PolicyMetrics result)
        {
            Assert.AreEqual(
                3,
                result.EnergyProbeTargetTier,
                $"{result.Policy} should wait for LV3 before proving the direct high-tier payoff branch.");
            Assert.GreaterOrEqual(
                ResolveEnergyTargetDuration(result),
                0f,
                $"{result.Policy} should record LV3 ready time.");
            Assert.AreEqual(
                3,
                result.HighestSummonSpentTier,
                $"{result.Policy} should spend SummonSlot1 at LV3.");
            Assert.IsTrue(
                result.BossScreenSuppressedByFollowup,
                $"{result.Policy} should suppress the active boss screen instead of being blocked by it.");
            Assert.Greater(
                result.BossPressureScreensSuppressedByFollowup,
                0,
                $"{result.Policy} should record at least one suppressed boss pressure screen.");
            Assert.Greater(
                result.FollowupSuppressScreenCueRequests,
                0,
                $"{result.Policy} should request a screen cue for the high-tier boss-screen suppress.");
            Assert.Greater(
                result.FollowupSuppressCameraCueRequests,
                0,
                $"{result.Policy} should request a camera cue for the high-tier boss-screen suppress.");
            Assert.Greater(
                result.FollowupSuppressVfxCueRequests,
                0,
                $"{result.Policy} should request a VFX cue for the high-tier boss-screen suppress.");
            Assert.AreEqual(
                3,
                result.HighestBossScreenSuppressSummonTier,
                $"{result.Policy} should attribute the suppress effect to the LV3 summon follow-up window.");
            Assert.IsFalse(
                result.BossBlockedSkill1Followup,
                $"{result.Policy} should not create a boss-screen blocked branch after LV3 suppression.");
            Assert.Greater(
                result.SkillProjectileHits,
                0,
                $"{result.Policy} should land Skill1 directly after suppressing the boss screen.");
            Assert.AreEqual(
                "CleanFollowupClear",
                result.ResultKind,
                $"{result.Policy} should close as a direct clean follow-up payoff.");
            Assert.AreEqual(
                "Complete",
                ResolveFirstUnresolvedBeat(result),
                $"{result.Policy} should complete the stage beat after direct high-tier payoff.");
            Assert.Greater(
                result.ResultRecords,
                0,
                $"{result.Policy} should commit the direct payoff result hook.");
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
                case PolicyKind.ForwardRiskEnergyProbe:
                    return result.ResultKind == "Running"
                        && result.EnergyProbeTargetTier == 1
                        && result.EnergyTier1DurationSeconds > 0f
                        && result.EnergyTier1DurationSeconds <= EnergyProbeMaxSeconds
                        && result.ForwardRiskEnergyScreenCueRequests > 0
                        && result.EnergyReadyScreenCueRequests > 0
                        && result.ForwardRiskBandSeconds > 0f;
                case PolicyKind.NoSummonNoFire:
                    return !result.IsClearResult
                        && result.EnemyFrontlineBodyHits > 0
                        && result.PlayerDamageTaken > 0f
                        && result.PlayerDamageScreenCueRequests > 0
                        && result.PlayerDamageFeedbackRequests > 0
                        && result.ResultRecords == 0;
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
                        && result.SummonBlocks > 0
                        && result.SkillProjectileHits > 0
                        && result.FollowupHitCinematicCueRequests > 0
                        && result.FollowupHitSequenceBridgeRequests == 0
                        && result.BossDamagePlayerShare01 >= 0.7f;
                case PolicyKind.IntendedRoute:
                    return result.ResultKind == "CleanFollowupClear"
                        && result.CloseThreatBasicHits > 0
                        && result.CloseThreatHealthRemaining <= 0.01f
                        && result.SummonBlocks > 0
                        && result.SkillProjectileHits > 0
                        && result.FollowupHitCinematicCueRequests > 0
                        && ResolveFirstUnresolvedBeat(result) == "Complete";
                case PolicyKind.ForwardRiskTier1DecisionRoute:
                    return IsEnergyDecisionRouteRepeatabilityPass(result, 1);
                case PolicyKind.ForwardRiskTier2DecisionRoute:
                    return IsEnergyDecisionRouteRepeatabilityPass(result, 2);
                case PolicyKind.ForwardRiskTier3DecisionRoute:
                    return IsEnergyDecisionRouteRepeatabilityPass(result, 3);
                case PolicyKind.ForwardRiskTier1RecoveryRoute:
                    return IsEnergyRecoveryRouteRepeatabilityPass(result, 1);
                case PolicyKind.ForwardRiskTier2RecoveryRoute:
                    return IsEnergyRecoveryRouteRepeatabilityPass(result, 2);
                case PolicyKind.ForwardRiskTier3RecoveryRoute:
                    return IsEnergyRecoveryRouteRepeatabilityPass(result, 3);
                case PolicyKind.ForwardRiskSlot2MarksmanRoute:
                    return IsSupportMarksmanRouteRepeatabilityPass(result);
                case PolicyKind.ForwardRiskSlot3VanguardRoute:
                    return IsSupportVanguardRouteRepeatabilityPass(result);
                case PolicyKind.ForwardRiskSlot2ThenSlot1ComboRoute:
                    return IsSupportSlot2ComboRouteRepeatabilityPass(result);
                case PolicyKind.ForwardRiskSlot3ThenSlot1BlockedRoute:
                    return IsSupportSlot3BlockedComboRouteRepeatabilityPass(result);
                case PolicyKind.ForwardRiskSlot2ThenDelayedSlot1Route:
                    return IsSupportDelayedSlot1RouteRepeatabilityPass(result, "SummonSlot2", 2);
                case PolicyKind.ForwardRiskSlot3ThenDelayedSlot1Route:
                    return IsSupportDelayedSlot1RouteRepeatabilityPass(result, "SummonSlot3", 3);
                case PolicyKind.ForwardRiskSlot2ThenDelayedRecoveryRoute:
                    return IsSupportDelayedRecoveryRouteRepeatabilityPass(result, "SummonSlot2", 2, false);
                case PolicyKind.ForwardRiskSlot3ThenDelayedRecoveryRoute:
                    return IsSupportDelayedRecoveryRouteRepeatabilityPass(result, "SummonSlot3", 3, true);
                case PolicyKind.ForwardRiskSlot3RetreatThenDelayedRecoveryRoute:
                    return IsSupportDelayedRecoveryRouteRepeatabilityPass(result, "SummonSlot3", 3, false);
                case PolicyKind.BossScreenIgnoredNoRecovery:
                    return !result.IsClearResult
                        && result.EnemyFrontlineBodyHits > 0
                        && result.PlayerDamageTaken > 0f
                        && result.ResultRecords == 0;
                case PolicyKind.BossScreenBlockCounterRecovery:
                    return result.ResultKind == "CounterRecoveryClear"
                        && result.SkillProjectileHits > 0
                        && result.FollowupHitCinematicCueRequests > 0
                        && result.FollowupHitSequenceBridgeRequests == 0
                        && ignoredBossScreenDamageMin > 0f
                        && result.PlayerDamageTaken < ignoredBossScreenDamageMin;
                default:
                    return false;
            }
        }

        private static bool IsEnergyDecisionRouteRepeatabilityPass(PolicyMetrics result, int expectedTier)
        {
            if (expectedTier < BossBarrageSummonReviewContract.Slot1MinimumTier)
            {
                return result.ResultKind == "Running"
                    && result.EnergyProbeTargetTier == expectedTier
                    && ResolveEnergyTargetDuration(result) >= 0f
                    && result.HighestSummonSpentTier == 0
                    && result.SummonBlocks == 0
                    && result.SkillUses == 0
                    && result.ResultRecords == 0;
            }

            int expectedSpentTier = BossBarrageSummonReviewContract.Slot1MinimumTier;
            return result.ResultKind == "Running"
                && result.EnergyProbeTargetTier == expectedTier
                && ResolveEnergyTargetDuration(result) >= 0f
                && result.HighestSummonSpentTier == expectedSpentTier
                && result.PhysicalBarragePlayerHits == 0
                && result.SummonBlocks > 0
                && result.SkillProjectileHits == 0
                && result.CounterWaves > 0
                && result.ResultRecords == 0;
        }

        private static bool IsEnergyRecoveryRouteRepeatabilityPass(PolicyMetrics result, int expectedTier)
        {
            if (expectedTier < BossBarrageSummonReviewContract.Slot1MinimumTier)
            {
                return result.ResultKind == "Running"
                    && result.EnergyProbeTargetTier == expectedTier
                    && ResolveEnergyTargetDuration(result) >= 0f
                    && result.HighestSummonSpentTier == 0
                    && result.SummonBlocks == 0
                    && result.SkillUses == 0
                    && result.ResultRecords == 0;
            }

            return result.ResultKind == "CounterRecoveryClear"
                && result.SummonUses >= 2
                && result.PhysicalBarragePlayerHits == 0
                && result.SkillProjectileHits > 0
                && result.CounterWaves > 0
                && ResolveFirstUnresolvedBeat(result) == "Complete"
                && result.ResultRecords > 0;
        }

        private static bool IsEnergyDirectPayoffRouteRepeatabilityPass(PolicyMetrics result)
        {
            return result.ResultKind == "CleanFollowupClear"
                && result.EnergyProbeTargetTier == 3
                && ResolveEnergyTargetDuration(result) >= 0f
                && result.HighestSummonSpentTier == 3
                && result.PhysicalBarragePlayerHits == 0
                && result.SummonBlocks > 0
                && result.BossScreenSuppressedByFollowup
                && result.BossPressureScreensSuppressedByFollowup > 0
                && result.FollowupSuppressScreenCueRequests > 0
                && result.FollowupSuppressCameraCueRequests > 0
                && result.FollowupSuppressVfxCueRequests > 0
                && result.HighestBossScreenSuppressSummonTier == 3
                && !result.BossBlockedSkill1Followup
                && result.SkillProjectileHits > 0
                && result.FollowupHitCinematicCueRequests > 0
                && result.FollowupHitCinematicFrameOverlayCount > 0
                && result.FollowupHitSequenceBridgeRequests == 0
                && ResolveFirstUnresolvedBeat(result) == "Complete"
                && result.ResultRecords > 0;
        }

        private static bool IsSupportMarksmanRouteRepeatabilityPass(PolicyMetrics result)
        {
            return result.SupportSummonSlotId == "SummonSlot2"
                && result.SupportSummonRequiredMana >= 99f
                && result.SupportSummonSpentTier == 1
                && result.SupportSummonActorRoleId == "LaserSoldier"
                && result.SupportSummonVolleyWaves > 0
                && result.SupportSummonBlocks == 0
                && result.SupportSummonProjectileHits > 0
                && result.SupportSummonProjectileBossHits + result.SupportSummonProjectileEnemySummonHits > 0;
        }

        private static bool IsSupportVanguardRouteRepeatabilityPass(PolicyMetrics result)
        {
            return result.SupportSummonSlotId == "SummonSlot3"
                && result.SupportSummonRequiredMana >= 299f
                && result.SupportSummonSpentTier == 3
                && result.SupportSummonActorRoleId == "FireDragon"
                && result.SupportSummonVolleyWaves > 0
                && result.SupportSummonBlocks == 0
                && result.SupportSummonProjectileHits > 0
                && result.SupportSummonProjectileBossHits + result.SupportSummonProjectileEnemySummonHits > 0;
        }

        private static bool IsSupportSlot2ComboRouteRepeatabilityPass(PolicyMetrics result)
        {
            return result.SupportSummonSlotId == "SummonSlot2"
                && result.SupportSummonRequiredMana >= 99f
                && result.SupportComboManaAfterSupport >= result.SupportComboSlot1RequiredMana - 0.001f
                && result.SupportComboManaBeforeSlot1 >= result.SupportComboSlot1RequiredMana - 0.001f
                && result.SupportComboSlot1Attempted
                && result.SupportComboSlot1Used
                && result.SummonUses >= 2
                && result.SummonBlocks > 0
                && result.SkillProjectileHits > 0
                && ResolveFirstUnresolvedBeat(result) == "Complete";
        }

        private static bool IsSupportSlot3BlockedComboRouteRepeatabilityPass(PolicyMetrics result)
        {
            return result.SupportSummonSlotId == "SummonSlot3"
                && result.SupportSummonRequiredMana >= 299f
                && result.SupportComboManaAfterSupport + 0.001f < result.SupportComboSlot1RequiredMana
                && result.SupportComboSlot1Attempted
                && !result.SupportComboSlot1Used
                && result.SupportComboSlot1BlockedReason.Contains("Requires 200 EN")
                && result.SupportSummonBlocks == 0
                && result.SupportSummonProjectileHits > 0
                && result.SkillProjectileHits == 0
                && ResolveFirstUnresolvedBeat(result) == "ScreenCurtain";
        }

        private static bool IsSupportDelayedSlot1RouteRepeatabilityPass(
            PolicyMetrics result,
            string expectedSlotId,
            int expectedTargetTier)
        {
            bool supportOpenedMainAnswer = result.SummonBlocks > 0
                || (expectedTargetTier >= 3
                    && result.BossScreenSuppressedByFollowup
                    && result.BossPressureScreensSuppressedByFollowup > 0);
            bool coreReopen = result.SupportSummonSlotId == expectedSlotId
                && result.SupportSummonSpentTier == expectedTargetTier
                && result.SupportComboManaAfterSupport + 0.001f < result.SupportComboSlot1RequiredMana
                && result.SupportComboSlot1ReadyDelaySeconds >= 0f
                && result.SupportComboManaBeforeSlot1 >= result.SupportComboSlot1RequiredMana - 0.001f
                && result.SupportComboSlot1Attempted
                && result.SupportComboSlot1Used
                && result.SummonUses >= 2
                && supportOpenedMainAnswer
                && result.PhysicalBarragePlayerHits == 0;
            if (!coreReopen)
            {
                return false;
            }

            bool counterAnswerBranch = result.CounterWaves > 0
                && ResolveFirstUnresolvedBeat(result) == "CounterAnswer"
                && result.ResultRecords == 0;
            bool marksmanLowReturnBranch = expectedSlotId == "SummonSlot2"
                && result.CounterWaves > 0
                && result.SkillProjectileHits == 0
                && ResolveFirstUnresolvedBeat(result) == "FollowupConfirm"
                && result.ResultRecords == 0;
            bool directClearBranch = expectedTargetTier >= 3
                && result.ResultKind == "CleanFollowupClear"
                && result.SkillProjectileHits > 0
                && ResolveFirstUnresolvedBeat(result) == "Complete"
                && result.ResultRecords > 0;
            return counterAnswerBranch || marksmanLowReturnBranch || directClearBranch;
        }

        private static bool IsSupportDelayedRecoveryRouteRepeatabilityPass(
            PolicyMetrics result,
            string expectedSlotId,
            int expectedTargetTier,
            bool expectBossScreenSuppress)
        {
            bool supportOpenedMainAnswer = result.SummonBlocks > 0
                || (expectBossScreenSuppress
                    && result.BossScreenSuppressedByFollowup
                    && result.BossPressureScreensSuppressedByFollowup > 0);
            bool coreReopen = result.SupportSummonSlotId == expectedSlotId
                && result.SupportSummonSpentTier == expectedTargetTier
                && result.SupportComboManaAfterSupport + 0.001f < result.SupportComboSlot1RequiredMana
                && result.SupportComboSlot1ReadyDelaySeconds >= 0f
                && result.SupportComboManaBeforeSlot1 >= result.SupportComboSlot1RequiredMana - 0.001f
                && result.SupportComboSlot1Attempted
                && result.SupportComboSlot1Used
                && result.SummonUses >= 2
                && supportOpenedMainAnswer
                && result.PhysicalBarragePlayerHits == 0
                && ResolveFirstUnresolvedBeat(result) == "Complete"
                && result.ResultRecords > 0;
            if (!coreReopen)
            {
                return false;
            }

            if (expectBossScreenSuppress)
            {
                return result.ResultKind == "CleanFollowupClear"
                    && result.SkillProjectileHits > 0;
            }

            return result.ResultKind == "CounterRecoveryClear"
                && result.CounterRecoveryConfirmed
                && result.CounterWaves > 0
                && !result.BossScreenSuppressedByFollowup
                && result.SkillProjectileHits > 0;
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

        private static string JsonNullableMetric(float value)
        {
            return value >= 0f ? value.ToString("0.###") : "null";
        }

        private static string JsonSignedMetric(float value)
        {
            return value.ToString("0.###");
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
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
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

        private static PlayerSupportSummonSlotAction RequireSupportSummonAction(
            GameObject gameObject,
            string slotActionName)
        {
            PlayerSupportSummonSlotAction[] actions = gameObject.GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = actions[i];
                if (action != null && string.Equals(action.SlotActionName, slotActionName, StringComparison.Ordinal))
                {
                    return action;
                }
            }

            Assert.Fail($"{gameObject.name} is missing support summon action {slotActionName}.");
            return null;
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

        private static IEnumerator WaitForActiveAllyPressureScreen(
            CombatPolicyContext context,
            string notePrefix)
        {
            float start = context.Metrics.ElapsedSeconds;
            while (FindActiveAllyPressureScreen() == null
                && context.Metrics.ElapsedSeconds - start < 0.6f)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (FindActiveAllyPressureScreen() == null)
            {
                context.Metrics.Notes.Add($"{notePrefix} ally pressure screen did not become active before barrage");
            }
        }

        private static IEnumerator WaitForSupportPayoffReadiness(
            CombatPolicyContext context,
            PlayerSupportSummonSlotAction supportAction,
            string notePrefix)
        {
            if (IsProjectileSupportRole(supportAction))
            {
                int projectileHitsBefore = context.Metrics.SupportSummonProjectileHits;
                float start = context.Metrics.ElapsedSeconds;
                while (context.Metrics.SupportSummonProjectileHits <= projectileHitsBefore
                    && context.Metrics.ElapsedSeconds - start < 1.15f)
                {
                    yield return Advance(context, 0.05f);
                    RecordSupportSummonSnapshot(context, supportAction);
                    context.PocketOwner.Tick(0f);
                    context.Sample();
                }

                if (context.Metrics.SupportSummonProjectileHits <= projectileHitsBefore)
                {
                    context.Metrics.Notes.Add(
                        $"{notePrefix} {supportAction.LastSummonActorRoleId} projectile did not hit before barrage");
                }

                yield break;
            }

            yield return WaitForActiveAllyPressureScreen(context, notePrefix);
        }

        private static bool IsProjectileSupportRole(PlayerSupportSummonSlotAction supportAction)
        {
            if (supportAction == null)
            {
                return false;
            }

            string roleId = supportAction.LastSummonActorRoleId;
            return string.Equals(roleId, "LaserSoldier", StringComparison.Ordinal)
                || IsFireDragonSupportRole(supportAction);
        }

        private static bool IsFireDragonSupportRole(PlayerSupportSummonSlotAction supportAction)
        {
            return supportAction != null
                && string.Equals(supportAction.LastSummonActorRoleId, "FireDragon", StringComparison.Ordinal);
        }

        private static IEnumerator WaitForNewSlot1PressureScreen(
            CombatPolicyContext context,
            int activeScreenCountBeforeUse,
            string notePrefix)
        {
            float start = context.Metrics.ElapsedSeconds;
            while (context.SummonSlot1Action.ActivePressureScreenCount <= activeScreenCountBeforeUse
                && context.Metrics.ElapsedSeconds - start < 0.6f)
            {
                yield return Advance(context, 0.05f);
                context.PocketOwner.Tick(0f);
                context.Sample();
            }

            if (context.SummonSlot1Action.ActivePressureScreenCount <= activeScreenCountBeforeUse)
            {
                context.Metrics.Notes.Add($"{notePrefix} pressure screen did not become active before barrage");
            }
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

        private static T GetObjectReference<T>(Object target, string fieldName)
            where T : Object
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{target.GetType().Name} is missing private field {fieldName}.");
            return field.GetValue(target) as T;
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
                PlayerSupportSummonSlotAction summonSlot2Action,
                PlayerSupportSummonSlotAction summonSlot3Action,
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
                BossBarrageLaneReviewHud reviewHud,
                BossBarrageLaneReviewOverlayHud reviewOverlayHud,
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
                SummonSlot2Action = summonSlot2Action;
                SummonSlot3Action = summonSlot3Action;
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
                ReviewHud = reviewHud;
                ReviewOverlayHud = reviewOverlayHud;
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
                Metrics.SummonManaCostTier1 = GetFloat(energyLadder, "levelOneEnergy");
                Metrics.SummonManaCostTier2 = GetFloat(energyLadder, "levelTwoEnergy");
                Metrics.SummonManaCostTier3 = GetFloat(energyLadder, "levelThreeEnergy");
                PocketOwner.ConfigureSupportSummonActions(summonSlot2Action, summonSlot3Action);
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
                SummonSlot2Action.SummonPressureBlocked += OnSupportSummonPressureBlocked;
                SummonSlot3Action.SummonPressureBlocked += OnSupportSummonPressureBlocked;
                SummonSlot2Action.SummonProjectileDamageApplied += OnSupportSummonProjectileDamageApplied;
                SummonSlot3Action.SummonProjectileDamageApplied += OnSupportSummonProjectileDamageApplied;
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
            public PlayerSupportSummonSlotAction SummonSlot2Action { get; }
            public PlayerSupportSummonSlotAction SummonSlot3Action { get; }
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
            public BossBarrageLaneReviewHud ReviewHud { get; }
            public BossBarrageLaneReviewOverlayHud ReviewOverlayHud { get; }
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
                Metrics.BossScreenSuppressedByFollowup |= PocketOwner.BossScreenSuppressedByFollowup;
                Metrics.BossPressureScreensSuppressedByFollowup = Mathf.Max(
                    Metrics.BossPressureScreensSuppressedByFollowup,
                    PocketOwner.BossPressureScreensSuppressedByFollowup);
                Metrics.HighestBossScreenSuppressSummonTier = Mathf.Max(
                    Metrics.HighestBossScreenSuppressSummonTier,
                    PocketOwner.HighestBossScreenSuppressSummonTier);
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
                if (EnergyLadder.AvailableTier >= 2 && Metrics.EnergyTier2ReadyAtSeconds < 0f)
                {
                    Metrics.EnergyTier2ReadyAtSeconds = Metrics.ElapsedSeconds;
                }
                if (EnergyLadder.AvailableTier >= 3 && Metrics.EnergyTier3ReadyAtSeconds < 0f)
                {
                    Metrics.EnergyTier3ReadyAtSeconds = Metrics.ElapsedSeconds;
                }

                SampleFrontlineClashCost();
                RecordSupportBodyCostFinal(this);
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
                Metrics.FollowupSuppressCameraCueRequests = CameraCueDriver.BossScreenSuppressCueRequestCount;
                Metrics.LastFollowupHitCameraTier = CameraCueDriver.LastSummonFollowupHitTier;
                Metrics.LastFollowupHitCameraDamage = CameraCueDriver.LastSummonFollowupHitDamage;
                Metrics.LastFollowupSuppressCameraTier = CameraCueDriver.LastBossScreenSuppressTier;

                Metrics.FollowupWindowVfxCueRequests = PocketVfxCueBridge.FollowupWindowCueRequestCount;
                Metrics.FollowupHitVfxCueRequests = PocketVfxCueBridge.FollowupHitCueRequestCount;
                Metrics.FollowupMissedVfxCueRequests = PocketVfxCueBridge.FollowupMissedCueRequestCount;
                Metrics.FollowupSuppressVfxCueRequests = PocketVfxCueBridge.BossScreenSuppressCueRequestCount;
                Metrics.LastFollowupHitVfxTier = PocketVfxCueBridge.LastFollowupHitTier;
                Metrics.LastFollowupHitVfxDamage = PocketVfxCueBridge.LastFollowupHitDamage;
                Metrics.LastFollowupSuppressVfxTier = PocketVfxCueBridge.LastBossScreenSuppressTier;
                Metrics.FollowupSuppressScreenCueRequests =
                    ScreenCuePresenter.FollowupSuppressCueRequestCount;

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
                    else if (cueId == "Followup.Suppress")
                    {
                        Metrics.FollowupSuppressScreenCueRequests = Mathf.Max(
                            Metrics.FollowupSuppressScreenCueRequests,
                            resolvedDelta);
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
                SummonSlot2Action.SummonPressureBlocked -= OnSupportSummonPressureBlocked;
                SummonSlot3Action.SummonPressureBlocked -= OnSupportSummonPressureBlocked;
                SummonSlot2Action.SummonProjectileDamageApplied -= OnSupportSummonProjectileDamageApplied;
                SummonSlot3Action.SummonProjectileDamageApplied -= OnSupportSummonProjectileDamageApplied;
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

            private void OnSupportSummonPressureBlocked(PlayerSupportSummonSlotAction action, int tier)
            {
                Metrics.SummonBlocks++;
                Metrics.SupportSummonBlocks++;
                Metrics.HighestSummonBlockTier = Mathf.Max(Metrics.HighestSummonBlockTier, tier);
                if (action != null)
                {
                    Metrics.SupportSummonSlotId = action.SlotActionName;
                    Metrics.SupportSummonSpentTier = Mathf.Max(Metrics.SupportSummonSpentTier, action.LastSpentTier);
                    Metrics.SupportSummonRequiredMana = action.RequiredSummonMana;
                    Metrics.SupportSummonActorRoleId = action.LastSummonActorRoleId;
                }

                if (Metrics.FirstSummonBlockAtSeconds < 0f)
                {
                    Metrics.FirstSummonBlockAtSeconds = Metrics.ElapsedSeconds;
                }
            }

            private void OnSupportSummonProjectileDamageApplied(
                PlayerSupportSummonSlotAction action,
                LaneActionProjectile projectile,
                CombatHealth targetHealth,
                Vector3 impactPoint,
                Vector3 impactDirection)
            {
                if (action != null)
                {
                    Metrics.SupportSummonSlotId = action.SlotActionName;
                    Metrics.SupportSummonSpentTier = Mathf.Max(Metrics.SupportSummonSpentTier, action.LastSpentTier);
                    Metrics.SupportSummonRequiredMana = action.RequiredSummonMana;
                    Metrics.SupportSummonActorRoleId = action.LastSummonActorRoleId;
                }

                Metrics.SupportSummonProjectileHits++;
                if (targetHealth == BossHealth)
                {
                    Metrics.SupportSummonProjectileBossHits++;
                    return;
                }

                if (targetHealth != null && targetHealth.Team == DamageTeam.Enemy)
                {
                    SummonFrontlineProxy targetProxy = targetHealth.GetComponentInParent<SummonFrontlineProxy>();
                    if (targetProxy != null)
                    {
                        Metrics.SupportSummonProjectileEnemySummonHits++;
                    }
                    else
                    {
                        Metrics.SupportSummonProjectileEnemyBodyHits++;
                    }

                    return;
                }

                Metrics.SupportSummonProjectileOtherHits++;
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
                Metrics.ResultRecordTokenId = record.ResultTokenId;
                Metrics.ResultRecordNextStateHookId = record.NextStateHookId;
                Metrics.ResultRecordProofReadout = record.ProofReadout;
                Metrics.ResultRecordDecision = $"{record.DecisionState}({record.DecisionReadout})";
                Metrics.ResultRecordCounterWaveSource = record.CounterWaveSource.ToString();
                if (ReviewOverlayHud != null)
                {
                    Metrics.ResultOverlayTitle = ReviewOverlayHud.ResultTitleReadout;
                    Metrics.ResultOverlaySummary = ReviewOverlayHud.ResultSummaryReadout;
                    Metrics.ResultOverlayRoute = ReviewOverlayHud.ResultRouteReadout;
                    Metrics.ResultOverlayRewardHook = ReviewOverlayHud.ResultRewardReadout;
                    Metrics.ResultOverlayNextObjective = ReviewOverlayHud.ResultNextObjectiveReadout;
                }
            }
        }

        private sealed class SummonRosterAuditRow
        {
            public SummonRosterAuditRow(
                string slot,
                string actionId,
                string costSource,
                float[] tierCosts,
                int minimumTier,
                float requiredMana,
                string[] roleIds,
                float[] volleyDamage,
                float[] projectileSpeed,
                int[] screenIntercepts,
                float[] actorHealth,
                float[] counterDamage,
                string readout)
            {
                Slot = slot;
                ActionId = actionId;
                CostSource = costSource;
                TierCosts = tierCosts;
                MinimumTier = minimumTier;
                RequiredMana = requiredMana;
                RoleIds = roleIds;
                VolleyDamage = volleyDamage;
                ProjectileSpeed = projectileSpeed;
                ScreenIntercepts = screenIntercepts;
                ActorHealth = actorHealth;
                CounterDamage = counterDamage;
                Readout = readout;
            }

            public string Slot { get; }
            public string ActionId { get; }
            public string CostSource { get; }
            public float[] TierCosts { get; }
            public int MinimumTier { get; }
            public float RequiredMana { get; }
            public string[] RoleIds { get; }
            public float[] VolleyDamage { get; }
            public float[] ProjectileSpeed { get; }
            public int[] ScreenIntercepts { get; }
            public float[] ActorHealth { get; }
            public float[] CounterDamage { get; }
            public string Readout { get; }
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
            public string SupportSummonSlotId { get; set; } = string.Empty;
            public float SupportSummonRequiredMana { get; set; }
            public string SupportChoiceForecastReadoutBeforeSupport { get; set; } = string.Empty;
            public float SupportSummonCooldownSeconds { get; set; }
            public int SupportSummonSpentTier { get; set; }
            public string SupportSummonActorRoleId { get; set; } = string.Empty;
            public int SupportSummonVolleyWaves { get; set; }
            public int SupportSummonBlocks { get; set; }
            public int SupportSummonMaxActiveActors { get; set; }
            public float SupportSummonActorHealthRatio { get; set; }
            public int SupportSummonProjectileHits { get; set; }
            public int SupportSummonProjectileBossHits { get; set; }
            public int SupportSummonProjectileEnemySummonHits { get; set; }
            public int SupportSummonProjectileEnemyBodyHits { get; set; }
            public int SupportSummonProjectileOtherHits { get; set; }
            public float SupportComboSlot1RequiredMana { get; set; }
            public float SupportComboManaAfterSupport { get; set; } = -1f;
            public float SupportComboManaBeforeSlot1 { get; set; } = -1f;
            public float SupportComboManaAfterSlot1 { get; set; } = -1f;
            public float SupportComboSupportCooldownAfterSupport { get; set; } = -1f;
            public float SupportComboSupportCooldownBeforeSlot1 { get; set; } = -1f;
            public float SupportComboSlot1CooldownBeforeAttempt { get; set; } = -1f;
            public float SupportComboSlot1CooldownAfterAttempt { get; set; } = -1f;
            public float SupportComboSlot1ReadyDelaySeconds { get; set; } = -1f;
            public float SupportComboPlayerDamageBeforeSlot1 { get; set; } = -1f;
            public string SupportComboHudSupportLabelBeforeSlot1 { get; set; } = string.Empty;
            public float SupportComboHudSupportFillBeforeSlot1 { get; set; } = -1f;
            public string SupportComboHudSlot1LabelBeforeAttempt { get; set; } = string.Empty;
            public float SupportComboHudSlot1FillBeforeAttempt { get; set; } = -1f;
            public string SupportComboOverlayHudReadoutBeforeSlot1 { get; set; } = string.Empty;
            public int SupportBodyHitsBeforeSupport { get; set; } = -1;
            public int SupportBodyHitsBeforeMainAnswer { get; set; } = -1;
            public int SupportBodyHitsFinal { get; set; } = -1;
            public float SupportDamageBeforeSupport { get; set; } = -1f;
            public float SupportDamageBeforeMainAnswer { get; set; } = -1f;
            public float SupportDamageFinal { get; set; } = -1f;
            public bool SupportComboSlot1Attempted { get; set; }
            public bool SupportComboSlot1Used { get; set; }
            public string SupportComboSlot1BlockedReason { get; set; } = string.Empty;
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
            public int HighestSummonSpentTier { get; set; }
            public int HighestSkill1SpentTier { get; set; }
            public float EnergyProbeTargetForwardRisk01 { get; set; } = -1f;
            public int EnergyProbeTargetTier { get; set; }
            public float EnergyProbeStartAtSeconds { get; set; } = -1f;
            public float EnergyTier1ReadyAtSeconds { get; set; } = -1f;
            public float EnergyTier2ReadyAtSeconds { get; set; } = -1f;
            public float EnergyTier3ReadyAtSeconds { get; set; } = -1f;
            public float SummonManaCostTier1 { get; set; }
            public float SummonManaCostTier2 { get; set; }
            public float SummonManaCostTier3 { get; set; }
            public float EnergyTier1DurationSeconds =>
                ResolveTimingDelta(EnergyProbeStartAtSeconds, EnergyTier1ReadyAtSeconds);
            public float EnergyTier2DurationSeconds =>
                ResolveTimingDelta(EnergyProbeStartAtSeconds, EnergyTier2ReadyAtSeconds);
            public float EnergyTier3DurationSeconds =>
                ResolveTimingDelta(EnergyProbeStartAtSeconds, EnergyTier3ReadyAtSeconds);
            public float EnergyProbeElapsedSeconds =>
                EnergyProbeStartAtSeconds >= 0f ? Mathf.Max(0f, ElapsedSeconds - EnergyProbeStartAtSeconds) : -1f;
            public float EnergyProbePlayerDamagePerSecond =>
                EnergyProbeElapsedSeconds > 0f ? PlayerDamageTaken / EnergyProbeElapsedSeconds : 0f;
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
            public int FollowupSuppressScreenCueRequests { get; set; }
            public string LastFollowupScreenCueId { get; set; } = string.Empty;
            public float LastFollowupScreenCueIntensity { get; set; }
            public float LastFollowupHitScreenCueIntensity { get; set; }
            public float LastFollowupWindowRouteScale { get; set; } = 1f;
            public int FollowupWindowCameraCueRequests { get; set; }
            public int FollowupHitCameraCueRequests { get; set; }
            public int FollowupMissedCameraCueRequests { get; set; }
            public int FollowupSuppressCameraCueRequests { get; set; }
            public int LastFollowupHitCameraTier { get; set; }
            public float LastFollowupHitCameraDamage { get; set; }
            public int LastFollowupSuppressCameraTier { get; set; }
            public int FollowupWindowVfxCueRequests { get; set; }
            public int FollowupHitVfxCueRequests { get; set; }
            public int FollowupMissedVfxCueRequests { get; set; }
            public int FollowupSuppressVfxCueRequests { get; set; }
            public int LastFollowupHitVfxTier { get; set; }
            public float LastFollowupHitVfxDamage { get; set; }
            public int LastFollowupSuppressVfxTier { get; set; }
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
            public bool BossScreenSuppressedByFollowup { get; set; }
            public int BossPressureScreensSuppressedByFollowup { get; set; }
            public int HighestBossScreenSuppressSummonTier { get; set; }
            public bool CleanFollowupConfirmed { get; set; }
            public bool CounterRecoveryConfirmed { get; set; }
            public float ResultRecordElapsedSeconds { get; set; }
            public float ResultRecordRouteStability01 { get; set; }
            public string ResultRecordRouteLabel { get; set; } = string.Empty;
            public string ResultRecordTitle { get; set; } = string.Empty;
            public string ResultRecordSummary { get; set; } = string.Empty;
            public string ResultRecordRewardHook { get; set; } = string.Empty;
            public string ResultRecordNextObjective { get; set; } = string.Empty;
            public string ResultRecordTokenId { get; set; } = string.Empty;
            public string ResultRecordNextStateHookId { get; set; } = string.Empty;
            public string ResultRecordProofReadout { get; set; } = string.Empty;
            public string ResultRecordDecision { get; set; } = string.Empty;
            public string ResultRecordCounterWaveSource { get; set; } = "None";
            public string ResultOverlayTitle { get; set; } = string.Empty;
            public string ResultOverlaySummary { get; set; } = string.Empty;
            public string ResultOverlayRoute { get; set; } = string.Empty;
            public string ResultOverlayRewardHook { get; set; } = string.Empty;
            public string ResultOverlayNextObjective { get; set; } = string.Empty;
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
