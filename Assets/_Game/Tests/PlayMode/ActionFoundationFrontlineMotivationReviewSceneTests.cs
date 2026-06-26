using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationFrontlineMotivationReviewSceneTests
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity";
        private const string StageProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_FrontlineWaveStage_MotivationReview.asset";
        private const string PocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string RangedBasicProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab";

        [Test]
        public void PremiumHudLayoutAvoidsPrimaryPanelOverlapAcrossReviewViewports()
        {
            Vector2[] viewports =
            {
                new Vector2(360f, 640f),
                new Vector2(640f, 360f),
                new Vector2(800f, 600f),
                new Vector2(960f, 540f),
                new Vector2(1280f, 720f),
                new Vector2(1920f, 1080f)
            };

            foreach (Vector2 viewport in viewports)
            {
                BossBarrageLaneReviewHud.PremiumHudLayout layout =
                    BossBarrageLaneReviewHud.ResolvePremiumHudLayoutForReview(viewport.x, viewport.y, 18f);
                AssertRectInsideViewport(layout.ObjectiveRect, viewport, "objective", layout);
                AssertRectInsideViewport(layout.BossBarRect, viewport, "boss bar", layout);
                AssertRectInsideViewport(layout.PlayerPanelRect, viewport, "player panel", layout);
                Assert.GreaterOrEqual(
                    layout.ObjectiveRect.height,
                    118f,
                    $"{viewport} objective panel must keep room for route promise and beat subdetail lines.");
                AssertNoOverlap(layout.ObjectiveRect, layout.BossBarRect, viewport, "objective", "boss bar");
                AssertNoOverlap(layout.ObjectiveRect, layout.PlayerPanelRect, viewport, "objective", "player panel");
                AssertNoOverlap(layout.BossBarRect, layout.PlayerPanelRect, viewport, "boss bar", "player panel");
            }
        }

        [UnityTest]
        public IEnumerator FrontlineMotivationReviewScenePreservesRouteContract()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Assert.AreEqual(ScenePath, SceneManager.GetActiveScene().path);

            FrontlineWaveStageProfile stageProfile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            Assert.NotNull(stageProfile);
            Assert.AreEqual("FRONTLINE-MOTIVATION-REVIEW-01", stageProfile.StageId);
            Assert.That(stageProfile.CombatPromise, Does.Contain("Survive boss pressure"));
            Assert.That(stageProfile.EntryCue, Does.Contain("Stay alive"));
            Assert.That(stageProfile.CounterWaveCue, Does.Contain("Counter pressure"));
            Assert.AreEqual(90f, stageProfile.TargetDurationSeconds);
            Assert.AreEqual(0.62f, stageProfile.RouteStabilityStart01, 0.001f);
            Assert.That(stageProfile.RouteCollapseFailDetail, Does.Contain("HP survival remains the fail state"));
            Assert.That(stageProfile.CleanRouteRewardHook, Does.Contain("Clean survival"));
            Assert.That(stageProfile.CounterRecoveryRewardHook, Does.Contain("Counter recovery"));
            Assert.That(stageProfile.FailedRouteNextObjective, Does.Contain("visible curtain"));
            Assert.That(stageProfile.OpeningRecordPreview, Does.Contain("stop close probe").IgnoreCase);
            Assert.That(stageProfile.CounterRecoveryRecordPreview, Does.Contain("final follow-up"));
            Assert.That(stageProfile.CollapseWarningRecordPreview, Does.Contain("HP is the fail state"));
            Assert.That(stageProfile.RouteEvidencePattern, Does.Contain("trigger"));
            Assert.That(stageProfile.RouteEvidencePattern, Does.Contain("answer"));
            Assert.That(stageProfile.RouteEvidencePattern, Does.Contain("log"));
            Assert.Greater(stageProfile.CloseProbeRouteDrainPerSecond, 0f);
            Assert.Greater(stageProfile.CounterWaveRouteDrainPerSecond, stageProfile.CloseProbeRouteDrainPerSecond);
            Assert.Greater(stageProfile.CounterWaveStabilizeRouteBonus01, 0f);
            Assert.That(stageProfile.CounterWaveAllyHoldSeconds, Is.InRange(0.25f, 0.75f));
            Assert.That(stageProfile.UnansweredBossHitRoutePenalty01, Is.InRange(0.04f, 0.12f));
            Assert.That(stageProfile.CleanFollowupEnergyPulseOverride, Is.InRange(200f, 240f));
            Assert.Greater(stageProfile.CounterWaveEntryRoutePenalty01, 0f);
            Assert.Greater(
                stageProfile.CounterWaveStabilizeRouteBonus01,
                stageProfile.CounterWaveEntryRoutePenalty01);
            Assert.That(stageProfile.UnstableCounterWaveFinalWindowScale, Is.InRange(0.5f, 0.95f));
            Assert.Less(stageProfile.CriticalCounterWaveFinalWindowScale, stageProfile.UnstableCounterWaveFinalWindowScale);
            Assert.That(stageProfile.CounterWaveStabilizedCue, Does.Contain("held"));
            Assert.GreaterOrEqual(stageProfile.BeatCount, 6);
            Assert.GreaterOrEqual(stageProfile.PressureSlotCount, 6);
            Assert.GreaterOrEqual(stageProfile.SourceReferenceCount, 5);
            FrontlineWaveStageProfile.SourceReference combatPayloadSource = stageProfile.GetSourceReference(3);
            Assert.AreEqual("CombatPayload.EventEffectContract", combatPayloadSource.SourceId);
            Assert.That(combatPayloadSource.LocalTakeaway, Does.Contain("trigger condition"));
            FrontlineWaveStageProfile.PressureSlot closeProbeSlot = stageProfile.GetPressureSlot(1);
            Assert.AreEqual("CloseProbe", closeProbeSlot.Label);
            Assert.That(closeProbeSlot.SpawnFamily, Does.Contain("Drop"));
            Assert.That(closeProbeSlot.SpawnFamily, Does.Contain("Dash"));
            Assert.That(closeProbeSlot.WavePathPattern, Does.Contain("path_grd"));
            Assert.Greater(closeProbeSlot.RoutePressureWeight, 0f);

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(RequireRoot(HudRootName), "overlay HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");

            Assert.AreSame(stageProfile, GetObjectReference<FrontlineWaveStageProfile>(pocketOwner, "stageProfile"));
            Assert.AreSame(stageProfile, GetObjectReference<FrontlineWaveStageProfile>(reviewHud, "stageProfile"));
            Assert.AreEqual(stageProfile.ObjectiveStepCount, pocketOwner.ObjectiveStepCount);
            Assert.AreEqual("ActionFoundationFrontlineMotivationReview", overlayHud.RetrySceneName);
            Assert.AreEqual(ScenePath, overlayHud.RetryScenePath);

            Assert.That(reviewHud.StageBriefingReadout, Does.Contain("Survive boss pressure"));
            Assert.That(reviewHud.StageBriefingReadout, Does.Contain("Stay alive"));
            Assert.That(reviewHud.CompactStageBriefingReadout, Does.Contain("Stay alive"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Survive 1/3"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Not.Contain("Boss"));
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("HP").IgnoreCase);
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Pending 0/3"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("pressure 62%"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("target 90"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("close:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("summon:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("followup:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:pending(none)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:pending(none)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:pending(none)"));
            Assert.AreEqual("survive", pocketOwner.RouteDecisionState);
            Assert.AreEqual("keep_hp", pocketOwner.RouteDecisionReadout);
            Assert.AreEqual(0, pocketOwner.RouteDecisionChangeCount);
            Assert.AreEqual("survive", pocketOwner.LastRouteDecisionSnapshot.State);
            Assert.AreEqual("keep_hp", pocketOwner.LastRouteDecisionSnapshot.Readout);
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, pocketOwner.LastRouteDecisionSnapshot.IncentiveCue);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.ThreatDefense, pocketOwner.LastRouteDecisionSnapshot.Phase);
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:survive(keep_hp)"));
            Assert.AreEqual(4, pocketOwner.RouteProofStepCount);
            Assert.AreEqual(0, pocketOwner.CompletedRouteProofStepCount);
            Assert.AreEqual("pending", pocketOwner.RouteProofState);
            Assert.That(
                pocketOwner.RouteProofReadout,
                Does.Contain("0/4 trigger:pending threat:pending answer:pending log:pending"));
            Assert.That(
                reviewHud.RouteRecordReadout,
                Does.Contain("proof:pending(0/4 trigger:pending threat:pending answer:pending log:pending)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Evidence trigger -> threat -> answer -> cue -> log"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("No payout or progression grant"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("stop close probe").IgnoreCase);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("confirm Skill1"));
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("pressure 62%"));
            int frontlineCueCountBeforeProbe = screenCuePresenter.FrontlineCueRequestCount;
            pocketOwner.Tick(0.6f);
            Assert.AreEqual(1, pocketOwner.CurrentStageBeatIndex);
            Assert.AreEqual(1, pocketOwner.CurrentPressureSlotIndex);
            Assert.AreEqual("CloseProbe", pocketOwner.CurrentPressureSlotLabel);
            Assert.AreEqual(closeProbeSlot.RoutePressureWeight, pocketOwner.CurrentRoutePressureWeight, 0.001f);
            Assert.Less(pocketOwner.RouteStability01, stageProfile.RouteStabilityStart01);
            Assert.Greater(screenCuePresenter.FrontlineCueRequestCount, frontlineCueCountBeforeProbe);
            Assert.AreEqual(1, screenCuePresenter.LastFrontlineBeatIndex);
            Assert.AreEqual("Probe Wave", screenCuePresenter.LastFrontlineBeatLabel);
            Assert.That(screenCuePresenter.LastCueId, Does.Contain("FrontlineBeat.B1.CloseProbe"));
            Assert.IsTrue(screenCuePresenter.HasActiveCue);
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Probe Wave"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("close_probe_defeated"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("CloseProbe"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("Drop|Dash|Jump"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("Local defense"));
            SetField(pocketOwner, "closeThreatDefeated", true);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("close:recorded"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:summon_now(boss_curtain)"));
            Assert.That(
                pocketOwner.CompletionRecordReadout,
                Does.Contain("proof:threat_pending(1/4 trigger:close_probe threat:summon_needed answer:pending log:pending)"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("First Summon Need"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("summon_slot1_used"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("ScreenCurtain"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("Summon cover"));
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("summon:recorded"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:confirm(summon_opening)"));
            Assert.That(
                pocketOwner.CompletionRecordReadout,
                Does.Contain("proof:answer_pending(2/4 trigger:close_probe threat:boss_curtain answer:pending log:pending)"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Enemy Counter Wave"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("BodyRush"));

            ForcePocketState(pocketOwner, "Cleared");
            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(pocketOwner, "skill1FollowupHitConfirmed", true);
            SetField(pocketOwner, "skill1FollowupDamage", 123f);
            SetField(pocketOwner, "resultElapsedSeconds", 42f);

            Assert.IsTrue(reviewHud.ShouldShowResultBanner);
            Assert.AreEqual("PRESSURE BROKEN", reviewHud.ResultBannerTitle);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Summon opening confirmed"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Survive 3/3"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Not.Contain("Record"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("42.0/90.0s"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Not.Contain("BOSS CLEAR"));
            Assert.AreEqual("PRESSURE BROKEN", overlayHud.ResultTitleReadout);
            Assert.That(overlayHud.ResultSummaryReadout, Does.Contain("Summon opening confirmed"));
            Assert.That(overlayHud.ResultRewardReadout, Does.Contain("Clean survival logged"));
            Assert.That(overlayHud.ResultNextObjectiveReadout, Does.Contain("counter pressure"));
            Assert.That(overlayHud.ResultTitleReadout, Does.Not.Contain("BOSS CLEAR"));
            Assert.That(overlayHud.ResultSummaryReadout, Does.Not.Contain("Boss pressure answered"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Summon follow-up"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("close:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("summon:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("followup:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:avoided(none)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:not_needed(clean_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:not_needed(clean_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:clean_clear(clean_followup)"));
            Assert.That(
                reviewHud.RouteRecordReadout,
                Does.Contain("proof:committed(4/4 trigger:close_probe threat:boss_curtain answer:skill1_confirm log:committed)"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Suppression Result"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("survival_record_committed"));
        }

        [UnityTest]
        public IEnumerator FrontlineRouteStabilityBandWarnsBeforeCollapse()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");

            Assert.AreEqual(BossBarragePocketReviewOwner.RouteStabilityBand.Stable, pocketOwner.CurrentRouteStabilityBand);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("stable"));
            int routeDecisionEventCount = 0;
            BossBarragePocketReviewOwner.RouteDecisionSnapshot lastRouteDecisionEvent = default;
            pocketOwner.RouteDecisionChanged += snapshot =>
            {
                routeDecisionEventCount++;
                lastRouteDecisionEvent = snapshot;
            };

            SetField(pocketOwner, "routeStability01", 0.4005f);
            int stabilityCueCountBeforeUnstable = screenCuePresenter.FrontlineStabilityCueRequestCount;
            pocketOwner.Tick(0.1f);

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.AreEqual(BossBarragePocketReviewOwner.RouteStabilityBand.Unstable, pocketOwner.CurrentRouteStabilityBand);
            Assert.Greater(screenCuePresenter.FrontlineStabilityCueRequestCount, stabilityCueCountBeforeUnstable);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteStabilityBand.Unstable,
                screenCuePresenter.LastFrontlineStabilityBand);
            Assert.That(screenCuePresenter.LastCueId, Does.Contain("FrontlineStability.Unstable"));
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("unstable"));

            SetField(pocketOwner, "routeStability01", 0.2005f);
            int stabilityCueCountBeforeCritical = screenCuePresenter.FrontlineStabilityCueRequestCount;
            pocketOwner.Tick(0.1f);

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.AreEqual(BossBarragePocketReviewOwner.RouteStabilityBand.Critical, pocketOwner.CurrentRouteStabilityBand);
            Assert.Greater(screenCuePresenter.FrontlineStabilityCueRequestCount, stabilityCueCountBeforeCritical);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteStabilityBand.Critical,
                screenCuePresenter.LastFrontlineStabilityBand);
            Assert.That(screenCuePresenter.LastCueId, Does.Contain("FrontlineStability.Critical"));
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("critical"));
            Assert.AreEqual(1, routeDecisionEventCount);
            Assert.AreEqual("survive", lastRouteDecisionEvent.State);
            Assert.AreEqual("keep_hp", lastRouteDecisionEvent.Readout);
            Assert.That(lastRouteDecisionEvent.IncentiveCue, Does.Contain("HP is the fail state"));
            Assert.That(lastRouteDecisionEvent.CompletionReadout, Does.Contain("decision:survive(keep_hp)"));
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("HP is the fail state"));
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("pressure is critical"));
        }

        [UnityTest]
        public IEnumerator FrontlinePresenceChangesRoutePressureDrain()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");

            Assert.AreEqual(0, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(0, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(1f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("pressure x1.00 open"));
            float openDrain = pocketOwner.CurrentRouteStabilityDrainPerSecond;
            Assert.Greater(openDrain, 0f);

            SummonFrontlineProxy allyProxy = CreateActiveFrontlineProxy("Test_Ally_FrontlineProxy", DamageTeam.AllySummon);
            Assert.AreEqual(1, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(0, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(0.70f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.Less(pocketOwner.CurrentRouteStabilityDrainPerSecond, openDrain);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("pressure x0.70 covered"));

            SummonFrontlineProxy enemyProxy = CreateActiveFrontlineProxy("Test_Enemy_FrontlineProxy", DamageTeam.Enemy);
            Assert.AreEqual(1, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(1, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(0.85f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("pressure x0.85 contested"));

            allyProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
            Assert.AreEqual(0, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(1, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(1.20f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.Greater(pocketOwner.CurrentRouteStabilityDrainPerSecond, openDrain);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("pressure x1.20 pressed"));

            enemyProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
        }

        [UnityTest]
        public IEnumerator FrontlineEnemyHitConsumesUnansweredBossRoutePenalty()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            FrontlineWaveStageProfile stageProfile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            Assert.NotNull(stageProfile);
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            PlayerMovementController player = UnityEngine.Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.NotNull(player, "Frontline hit penalty test needs the scene player.");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");

            float stabilityBeforeHit = pocketOwner.RouteStability01;
            Assert.AreEqual(0, pocketOwner.UnansweredBossHitRoutePenaltyCount);
            Assert.IsTrue(playerHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                1f,
                player.transform.position,
                Vector3.back,
                0f)));

            Assert.AreEqual(1, pocketOwner.UnansweredBossHitRoutePenaltyCount);
            Assert.AreEqual(
                stageProfile.UnansweredBossHitRoutePenalty01,
                pocketOwner.LastUnansweredBossHitRoutePenalty,
                0.001f);
            Assert.AreEqual(
                stageProfile.UnansweredBossHitRoutePenalty01,
                pocketOwner.TotalUnansweredBossHitRoutePenalty,
                0.001f);
            Assert.AreEqual(
                Mathf.Max(0f, stabilityBeforeHit - stageProfile.UnansweredBossHitRoutePenalty01),
                pocketOwner.RouteStability01,
                0.001f);
        }

        [UnityTest]
        public IEnumerator FrontlineMissedFollowupRecordsCounterWaveBeforeRecovery()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(RequireRoot(HudRootName), "overlay HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(RequireRoot(PocketOwnerRootName), "pocket VFX cue bridge");
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonEnergyLadder energyLadder =
                RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            FrontlineWaveStageProfile stageProfile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            Assert.NotNull(stageProfile);

            Assert.IsFalse(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.AreEqual("pending", pocketOwner.CounterWaveRecordState);
            Assert.AreEqual(BossBarragePocketReviewOwner.CounterWaveSource.None, pocketOwner.CounterWaveObservedSource);
            Assert.AreEqual("none", pocketOwner.CounterWaveSourceReadout);
            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("none", pocketOwner.CounterWaveAnswerReadout);
            Assert.AreEqual("pending", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("none", pocketOwner.CounterWaveFinalWindowReadout);

            SetField(pocketOwner, "closeThreatDefeated", true);
            TickEnergyToTier(energyLadder, 1, 0.25f);
            Assert.IsTrue(
                summonSlot1Action.TryUseSummonSlot1(),
                "Counter recovery setup should still enter through the real SummonSlot1 action.");
            Assert.Greater(summonSlot1Action.ActivePressureScreenCount, 0);
            SummonPressureScreen openingScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile openingProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(
                openingScreen.TryIntercept(openingProjectile),
                "The initial route break should be recorded from a real summon pressure screen intercept.");
            pocketOwner.Tick(0f);
            Assert.IsTrue(pocketOwner.UsedSummonSlot1);
            Assert.IsTrue(pocketOwner.BlockedBossPressureWithSummon);
            DeactivateActiveFrontlineProxies(DamageTeam.AllySummon);

            SummonFrontlineProxy enemyProxy = CreateActiveFrontlineProxy("Test_CounterWave_EnemyProxy", DamageTeam.Enemy);
            int counterCueCountBeforeEnemy = screenCuePresenter.CounterWaveCueRequestCount;
            int counterVfxCueCountBeforeEnemy = pocketVfxCueBridge.CounterWaveCueRequestCount;
            int counterCameraCueCountBeforeEnemy = cameraCueDriver.CounterWaveCueRequestCount;
            float stabilityBeforeCounterWave = pocketOwner.RouteStability01;
            int routeDecisionCountBeforeCounter = pocketOwner.RouteDecisionChangeCount;
            BossBarragePocketReviewOwner.RouteDecisionSnapshot lastRouteDecisionEvent = default;
            pocketOwner.RouteDecisionChanged += snapshot => lastRouteDecisionEvent = snapshot;
            pocketOwner.Tick(0f);

            Assert.IsFalse(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter:pending(none)"));
            Assert.AreEqual(counterCueCountBeforeEnemy, screenCuePresenter.CounterWaveCueRequestCount);
            Assert.AreEqual(counterVfxCueCountBeforeEnemy, pocketVfxCueBridge.CounterWaveCueRequestCount);
            Assert.AreEqual(counterCameraCueCountBeforeEnemy, cameraCueDriver.CounterWaveCueRequestCount);

            pocketOwner.Tick(pocketOwner.SummonFollowupWindowRemainingSeconds + 0.01f);

            Assert.IsTrue(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.AreEqual("recorded", pocketOwner.CounterWaveRecordState);
            Assert.AreEqual(BossBarragePocketReviewOwner.CounterWaveSource.FollowupMissed, pocketOwner.CounterWaveObservedSource);
            Assert.AreEqual("followup_miss", pocketOwner.CounterWaveSourceReadout);
            Assert.AreEqual(stageProfile.CounterWaveEntryRoutePenalty01, pocketOwner.LastCounterWaveEntryPenalty, 0.001f);
            Assert.Less(pocketOwner.RouteStability01, stabilityBeforeCounterWave);
            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("awaiting", pocketOwner.CounterWaveAnswerReadout);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("awaiting_answer", pocketOwner.CounterWaveFinalWindowReadout);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.CounterWave, pocketOwner.CurrentPhase);
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("Counter pressure"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Hold counter pressure"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter:recorded(followup_miss)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:pending(awaiting)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_window:pending(awaiting_answer)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.That(
                pocketOwner.CompletionRecordReadout,
                Does.Contain("proof:threat_pending(1/4 trigger:counter_wave threat:ally_needed answer:pending log:pending)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:recorded(followup_miss)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:pending(awaiting)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:pending(awaiting_answer)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.Greater(pocketOwner.RouteDecisionChangeCount, routeDecisionCountBeforeCounter);
            Assert.AreEqual("recovery_needed", lastRouteDecisionEvent.State);
            Assert.AreEqual("answer_counter", lastRouteDecisionEvent.Readout);
            Assert.That(lastRouteDecisionEvent.IncentiveCue, Does.Contain("summon pressure held"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("summon pressure held"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Enemy Counter Wave"));
            Assert.Greater(screenCuePresenter.CounterWaveCueRequestCount, counterCueCountBeforeEnemy);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.FollowupMissed,
                screenCuePresenter.LastCounterWaveSource);
            Assert.Greater(pocketVfxCueBridge.CounterWaveCueRequestCount, counterVfxCueCountBeforeEnemy);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.FollowupMissed,
                pocketVfxCueBridge.LastCounterWaveSource);
            Assert.AreEqual(CombatVfxCueId.EnemyLinePressureActive, pocketVfxCueBridge.CounterWaveCueId);
            Assert.AreEqual(counterCameraCueCountBeforeEnemy + 1, cameraCueDriver.CounterWaveCueRequestCount);
            Assert.AreEqual(2, cameraCueDriver.LastCounterWaveTier);

            float stabilityBeforeAnswer = pocketOwner.RouteStability01;
            SetField(pocketOwner, "routeStability01", 0.24f);
            float unstableStabilityBeforeAnswer = pocketOwner.RouteStability01;
            TickEnergyToTier(energyLadder, 1, 0.25f);
            int summonUseCountBeforeCounterAnswer = summonSlot1Action.TotalUseCount;
            Assert.IsTrue(
                summonSlot1Action.TryUseSummonSlot1(),
                "Counter wave recovery should be answered by the real SummonSlot1 action.");
            Assert.Greater(summonSlot1Action.TotalUseCount, summonUseCountBeforeCounterAnswer);
            Assert.Greater(summonSlot1Action.ActiveSummonActorCount, 0);
            int counterAnswerCueCountBeforeAlly = screenCuePresenter.CounterWaveAnswerCueRequestCount;
            int followupCueCountBeforeAlly = screenCuePresenter.FollowupCueRequestCount;
            int counterStabilizedVfxCueCountBeforeAlly = pocketVfxCueBridge.CounterWaveStabilizedCueRequestCount;
            int counterStabilizedCameraCueCountBeforeAlly = cameraCueDriver.CounterWaveStabilizedCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.Greater(pocketOwner.ActiveAllyFrontlineProxyCount, 0);
            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("holding_0%", pocketOwner.CounterWaveAnswerReadout);
            Assert.AreEqual("pending", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("awaiting_answer", pocketOwner.CounterWaveFinalWindowReadout);
            Assert.AreEqual(stageProfile.CounterWaveAllyHoldSeconds, pocketOwner.CounterWaveAllyHoldRequiredSeconds, 0.001f);
            Assert.AreEqual(0f, pocketOwner.CounterWaveAllyHoldElapsedSeconds, 0.001f);
            Assert.AreEqual(0f, pocketOwner.CounterWaveAllyHoldProgress01, 0.001f);
            Assert.AreEqual(stageProfile.CounterWaveAllyHoldSeconds, pocketOwner.CounterWaveAllyHoldRemainingSeconds, 0.001f);
            Assert.Greater(
                pocketOwner.CurrentRouteStabilityDrainPerSecond,
                0f,
                "Counter wave recovery should keep route pressure active even if the previous follow-up relief window is still open.");
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:pending(holding_0%)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovery_needed(ally_holding)"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Hold counter pressure 0.5s"));
            Assert.AreEqual(counterAnswerCueCountBeforeAlly, screenCuePresenter.CounterWaveAnswerCueRequestCount);
            Assert.AreEqual(counterStabilizedVfxCueCountBeforeAlly, pocketVfxCueBridge.CounterWaveStabilizedCueRequestCount);
            Assert.AreEqual(counterStabilizedCameraCueCountBeforeAlly, cameraCueDriver.CounterWaveStabilizedCueRequestCount);

            pocketOwner.Tick(stageProfile.CounterWaveAllyHoldSeconds * 0.5f);

            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("holding_50%", pocketOwner.CounterWaveAnswerReadout);
            Assert.That(pocketOwner.CounterWaveAllyHoldProgress01, Is.InRange(0.49f, 0.51f));
            Assert.Greater(pocketOwner.CounterWaveAllyHoldRemainingSeconds, 0f);
            Assert.Less(pocketOwner.RouteStability01, unstableStabilityBeforeAnswer);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:pending(holding_50%)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovery_needed(ally_holding)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:pending(holding_50%)"));
            Assert.AreEqual(counterAnswerCueCountBeforeAlly, screenCuePresenter.CounterWaveAnswerCueRequestCount);

            float stabilityAfterInterruptedHalfHold = pocketOwner.RouteStability01;
            DeactivateActiveFrontlineProxies(DamageTeam.AllySummon);
            pocketOwner.Tick(0f);

            Assert.AreEqual(0, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.IsTrue(pocketOwner.WasCounterWaveAllyHoldInterrupted);
            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("interrupted", pocketOwner.CounterWaveAnswerReadout);
            Assert.AreEqual(0f, pocketOwner.CounterWaveAllyHoldElapsedSeconds, 0.001f);
            Assert.AreEqual(0f, pocketOwner.CounterWaveAllyHoldProgress01, 0.001f);
            Assert.AreEqual(stageProfile.CounterWaveAllyHoldSeconds, pocketOwner.CounterWaveAllyHoldRemainingSeconds, 0.001f);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:pending(interrupted)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.That(
                pocketOwner.CompletionRecordReadout,
                Does.Contain("proof:threat_pending(1/4 trigger:counter_wave threat:interrupted answer:pending log:pending)"));
            Assert.AreEqual(counterAnswerCueCountBeforeAlly, screenCuePresenter.CounterWaveAnswerCueRequestCount);
            Assert.AreEqual(counterStabilizedVfxCueCountBeforeAlly, pocketVfxCueBridge.CounterWaveStabilizedCueRequestCount);
            Assert.AreEqual(counterStabilizedCameraCueCountBeforeAlly, cameraCueDriver.CounterWaveStabilizedCueRequestCount);

            TickEnergyToTier(energyLadder, 1, 0.25f);
            Assert.IsTrue(
                summonSlot1Action.TryUseSummonSlot1(),
                "Interrupted counter recovery should require a fresh SummonSlot1 frontline hold.");
            pocketOwner.Tick(0f);

            Assert.Greater(pocketOwner.ActiveAllyFrontlineProxyCount, 0);
            Assert.IsFalse(pocketOwner.WasCounterWaveAllyHoldInterrupted);
            Assert.AreEqual("holding_0%", pocketOwner.CounterWaveAnswerReadout);

            pocketOwner.Tick(stageProfile.CounterWaveAllyHoldSeconds * 0.5f);

            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.AreEqual("holding_50%", pocketOwner.CounterWaveAnswerReadout);
            Assert.That(pocketOwner.CounterWaveAllyHoldProgress01, Is.InRange(0.49f, 0.51f));
            Assert.Less(pocketOwner.RouteStability01, stabilityAfterInterruptedHalfHold);
            Assert.AreEqual(counterAnswerCueCountBeforeAlly, screenCuePresenter.CounterWaveAnswerCueRequestCount);

            float stabilityBeforeHoldComplete = pocketOwner.RouteStability01;
            int routeDecisionCountBeforeRecovered = pocketOwner.RouteDecisionChangeCount;
            pocketOwner.Tick(stageProfile.CounterWaveAllyHoldSeconds * 0.5f + 0.01f);

            Assert.IsTrue(pocketOwner.IsCounterWaveStabilized);
            Assert.IsTrue(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("stabilized", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("ally_hold", pocketOwner.CounterWaveAnswerReadout);
            Assert.AreEqual("opened", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("final_followup", pocketOwner.CounterWaveFinalWindowReadout);
            Assert.AreEqual(1f, pocketOwner.CounterWaveAllyHoldProgress01, 0.001f);
            Assert.AreEqual(0f, pocketOwner.CounterWaveAllyHoldRemainingSeconds, 0.001f);
            Assert.AreEqual(stageProfile.CounterWaveStabilizeRouteBonus01, pocketOwner.LastCounterWaveStabilityBonus, 0.001f);
            Assert.GreaterOrEqual(
                pocketOwner.LastCounterWaveFinalWindowRouteScale,
                stageProfile.UnstableCounterWaveFinalWindowScale);
            Assert.LessOrEqual(pocketOwner.LastCounterWaveFinalWindowRouteScale, 1f);
            Assert.Greater(pocketOwner.RouteStability01, stabilityBeforeHoldComplete);
            Assert.Less(
                pocketOwner.RouteStability01,
                unstableStabilityBeforeAnswer + stageProfile.CounterWaveStabilizeRouteBonus01);
            Assert.Greater(pocketOwner.LastCounterWaveFinalWindowDuration, 0f);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.SummonFollowup, pocketOwner.CurrentPhase);
            Assert.Greater(pocketOwner.RouteStability01, unstableStabilityBeforeAnswer);
            Assert.GreaterOrEqual(
                pocketOwner.RouteStability01,
                stabilityBeforeAnswer,
                "A fresh counter answer should recover the route enough to read as a real final punish window.");
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("Confirm"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("window"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Follow-Up Window"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:stabilized(ally_hold)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovered(final_window)"));
            Assert.That(
                pocketOwner.CompletionRecordReadout,
                Does.Contain("proof:answer_pending(2/4 trigger:counter_wave threat:ally_hold answer:final_window log:pending)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:stabilized(ally_hold)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovered(final_window)"));
            Assert.Greater(pocketOwner.RouteDecisionChangeCount, routeDecisionCountBeforeRecovered);
            Assert.AreEqual("recovered", lastRouteDecisionEvent.State);
            Assert.AreEqual("final_window", lastRouteDecisionEvent.Readout);
            Assert.That(lastRouteDecisionEvent.IncentiveCue, Does.Contain("final follow-up"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("final follow-up"));
            Assert.Greater(screenCuePresenter.CounterWaveAnswerCueRequestCount, counterAnswerCueCountBeforeAlly);
            Assert.Greater(screenCuePresenter.FollowupCueRequestCount, followupCueCountBeforeAlly);
            Assert.AreEqual("ally_hold", screenCuePresenter.LastCounterWaveAnswer);
            Assert.AreEqual(
                pocketOwner.LastCounterWaveFinalWindowRouteScale,
                screenCuePresenter.LastFollowupWindowRouteScale,
                0.001f);
            Assert.That(screenCuePresenter.LastCueId, Does.Contain("Followup.Window"));
            Assert.GreaterOrEqual(screenCuePresenter.LastCueIntensity, 0.82f);
            Assert.Greater(
                pocketVfxCueBridge.CounterWaveStabilizedCueRequestCount,
                counterStabilizedVfxCueCountBeforeAlly);
            Assert.AreEqual(CombatVfxCueId.EliteShieldSignal, pocketVfxCueBridge.CounterWaveStabilizedCueId);
            Assert.AreEqual(
                counterStabilizedCameraCueCountBeforeAlly + 1,
                cameraCueDriver.CounterWaveStabilizedCueRequestCount);
            Assert.AreEqual(2, cameraCueDriver.LastCounterWaveStabilizedTier);

            float bossHealthBeforeFinalHit = bossHealth.CurrentHealth;
            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Assert.IsTrue(
                skill1Action.TryUseSkill1(),
                "Counter recovery should end through the real Skill1 action.");
            LaneActionProjectile finalFollowupProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(finalFollowupProjectile.TryApplyImpact(bossHitCollider, finalFollowupProjectile.transform.position));
            Assert.Less(bossHealth.CurrentHealth, bossHealthBeforeFinalHit);
            int counterRecoveryRecordEventCount = 0;
            BossBarragePocketReviewOwner.RouteResultRecord counterRecoveryEventRecord = default;
            pocketOwner.ResultRecordCommitted += record =>
            {
                counterRecoveryRecordEventCount++;
                counterRecoveryEventRecord = record;
            };
            pocketOwner.Tick(1f);

            Assert.IsTrue(pocketOwner.Skill1FollowupHitConfirmed);
            Assert.IsTrue(pocketOwner.IsCleared);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.Cleared, pocketOwner.CurrentPhase);
            Assert.IsTrue(pocketOwner.HasCommittedResultRecord);
            Assert.AreEqual(1, pocketOwner.ResultRecordCommitCount);
            Assert.AreEqual(1, counterRecoveryRecordEventCount);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteResultKind.CounterRecoveryClear,
                pocketOwner.LastResultRecord.ResultKind);
            Assert.IsTrue(pocketOwner.LastResultRecord.IsClear);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.FollowupMissed,
                pocketOwner.LastResultRecord.CounterWaveSource);
            Assert.AreEqual("recovery_clear", pocketOwner.LastResultRecord.DecisionState);
            Assert.AreEqual("counter_recovery", pocketOwner.LastResultRecord.DecisionReadout);
            Assert.That(pocketOwner.LastResultRecord.Title, Does.Contain("PRESSURE BROKEN"));
            Assert.That(pocketOwner.LastResultRecord.Summary, Does.Contain("Counter pressure held"));
            Assert.AreEqual("Counter recovery", pocketOwner.LastResultRecord.RouteLabel);
            Assert.That(pocketOwner.LastResultRecord.RewardHook, Does.Contain("Counter recovery logged"));
            Assert.That(pocketOwner.LastResultRecord.NextObjective, Does.Contain("earlier"));
            Assert.That(pocketOwner.LastResultRecord.CompletionReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(pocketOwner.LastResultRecord.ProofReadout, Does.Contain("answer:final_skill"));
            Assert.AreEqual(pocketOwner.LastResultRecord.ResultKind, counterRecoveryEventRecord.ResultKind);
            Assert.AreEqual(pocketOwner.LastResultRecord.Title, overlayHud.ResultTitleReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.Summary, overlayHud.ResultSummaryReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.RouteLabel, overlayHud.ResultRouteReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.RewardHook, overlayHud.ResultRewardReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.NextObjective, overlayHud.ResultNextObjectiveReadout);
            Assert.That(reviewHud.ResultBannerTitle, Does.Contain("PRESSURE BROKEN"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Counter pressure held"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("final follow-up"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Survive 3/3"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Not.Contain("Record"));
            Assert.That(overlayHud.ResultSummaryReadout, Does.Contain("Counter pressure held"));
            Assert.That(overlayHud.ResultRewardReadout, Does.Contain("Counter recovery logged"));
            Assert.That(overlayHud.ResultNextObjectiveReadout, Does.Contain("earlier"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Counter recovery"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Record A: Counter recovery"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("pressure 62%"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("followup:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovery_clear(counter_recovery)"));
            Assert.That(
                reviewHud.RouteRecordReadout,
                Does.Contain("proof:committed(4/4 trigger:counter_wave threat:ally_hold answer:final_skill log:committed)"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("Counter recovery"));

            enemyProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
        }

        [UnityTest]
        public IEnumerator FrontlineBossSummonReleaseRecordsCounterWave()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(RequireRoot(PocketOwnerRootName), "pocket VFX cue bridge");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossSummonPressureAction bossSummonPressureAction =
                UnityEngine.Object.FindFirstObjectByType<BossSummonPressureAction>();
            Assert.NotNull(bossSummonPressureAction, "Frontline review scene should keep the boss summon pressure action.");

            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(bossSummonPressureAction, "totalReleaseCount", bossSummonPressureAction.TotalReleaseCount + 1);
            int counterCueCountBeforeRelease = screenCuePresenter.CounterWaveCueRequestCount;
            int counterVfxCueCountBeforeRelease = pocketVfxCueBridge.CounterWaveCueRequestCount;
            int counterCameraCueCountBeforeRelease = cameraCueDriver.CounterWaveCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.AreEqual("recorded", pocketOwner.CounterWaveRecordState);
            Assert.AreEqual(BossBarragePocketReviewOwner.CounterWaveSource.BossSummonRelease, pocketOwner.CounterWaveObservedSource);
            Assert.AreEqual("boss_summon", pocketOwner.CounterWaveSourceReadout);
            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("awaiting", pocketOwner.CounterWaveAnswerReadout);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("awaiting_answer", pocketOwner.CounterWaveFinalWindowReadout);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.CounterWave, pocketOwner.CurrentPhase);
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Hold counter pressure"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter:recorded(boss_summon)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:pending(awaiting)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_window:pending(awaiting_answer)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:recorded(boss_summon)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:pending(awaiting)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:pending(awaiting_answer)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.Greater(screenCuePresenter.CounterWaveCueRequestCount, counterCueCountBeforeRelease);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.BossSummonRelease,
                screenCuePresenter.LastCounterWaveSource);
            Assert.Greater(pocketVfxCueBridge.CounterWaveCueRequestCount, counterVfxCueCountBeforeRelease);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.BossSummonRelease,
                pocketVfxCueBridge.LastCounterWaveSource);
            Assert.AreEqual(counterCameraCueCountBeforeRelease + 1, cameraCueDriver.CounterWaveCueRequestCount);
            Assert.AreEqual(2, cameraCueDriver.LastCounterWaveTier);
        }

        [UnityTest]
        public IEnumerator FrontlineRangedBasicSoftAssistBendsNearCenterFireTowardCloseThreat()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            Collider closeThreatCollider = RequireCombatHitCollider(closeThreatRoot, closeThreatHealth, "close threat");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            rangedBasicAttackAction.ClearAimInput();
            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.25f);

            Vector3 aimPlanarDirection = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            if (aimPlanarDirection.sqrMagnitude <= 0.0001f)
            {
                aimPlanarDirection = cameraController.GetAimPlanarForward();
            }

            aimPlanarDirection.Normalize();
            Vector3 aimRight = Vector3.Cross(Vector3.up, aimPlanarDirection).normalized;
            Vector3 closeThreatPosition =
                player.transform.position + aimPlanarDirection * 4.6f + aimRight * 1.1f;
            closeThreatPosition.y = closeThreatRoot.transform.position.y;
            closeThreatRoot.transform.SetPositionAndRotation(
                closeThreatPosition,
                Quaternion.LookRotation(-aimPlanarDirection, Vector3.up));
            closeThreatHealth.ResetHealthToFull();
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(
                GetBool(rangedBasicAttackAction, "useAimAssist"),
                "The frontline motivation scene should keep ranged soft aim assist enabled.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.IsTrue(
                rangedBasicAttackAction.HasAimAssistTarget,
                "A near-center close threat should be captured by ranged soft aim assist instead of requiring pixel-perfect center aim.");
            Assert.AreSame(closeThreatHealth, rangedBasicAttackAction.AimAssistTargetHealth);

            Vector3 targetAimPoint =
                closeThreatHealth.transform.position + Vector3.up * GetFloat(rangedBasicAttackAction, "targetHeight");
            Vector3 expectedTargetDirection =
                (targetAimPoint - rangedBasicAttackAction.FireOrigin.position).normalized;
            Assert.Less(
                Vector3.Angle(previewDirection, expectedTargetDirection),
                6f,
                "The preview direction should bend toward the close threat once aim assist has a target.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimAssistPreviewViewportPoint(out Vector2 assistViewportPoint));
            Assert.IsTrue(cameraController.TryWorldToViewportPoint(targetAimPoint, out Vector3 targetViewportPoint));
            Assert.Less(
                Vector2.Distance(assistViewportPoint, new Vector2(targetViewportPoint.x, targetViewportPoint.y)),
                0.08f,
                "The assist reticle point should describe the same assisted target as the fired projectile.");

            Assert.IsTrue(rangedBasicAttackAction.TryFire());
            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, expectedTargetDirection),
                6f,
                "The fired projectile should use the assisted close-threat direction.");
            Assert.IsTrue(
                closeThreatCollider.bounds.IntersectRay(
                    new Ray(playerProjectile.transform.position, playerProjectile.TravelDirection)),
                "The assisted projectile ray should intersect the close threat bounds.");

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }
        }

        [UnityTest]
        public IEnumerator FrontlineBossDirectAimKeepsPreviewHeightStableOnHighChildCollider()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossRootCollider = RequireComponent<Collider>(bossRoot, "boss root collider");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            SetField(rangedBasicAttackAction, "useFixedCenterAimViewport", false);
            SetField(rangedBasicAttackAction, "aimInputViewportOffsetY", 0.45f);
            rangedBasicAttackAction.ClearAimInput();
            rangedBasicAttackAction.SetAimInput(Vector2.up);
            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return WaitSeconds(0.25f);

            float stableBodyY = bossRootCollider.bounds.center.y;
            Vector2 elevatedViewportPoint = new Vector2(0.5f, 0.95f);
            Assert.IsTrue(cameraController.TryGetViewportAimRay(elevatedViewportPoint, out Ray elevatedRay));
            Vector3 highHitPoint = elevatedRay.GetPoint(12f);
            Assert.Greater(
                highHitPoint.y,
                stableBodyY + 0.7f,
                "The test collider must sit on an elevated aim ray to reproduce the boss aim jump.");
            GameObject highHitProxy = new GameObject("FrontlineBossAimHighHitProxy");
            highHitProxy.transform.SetParent(bossRoot.transform, worldPositionStays: true);
            highHitProxy.transform.position = highHitPoint;
            BoxCollider highHitCollider = highHitProxy.AddComponent<BoxCollider>();
            highHitCollider.size = new Vector3(2.4f, 2.4f, 2.4f);
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewWorldPoint(out Vector3 previewWorldPoint));
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.AreSame(bossHealth, rangedBasicAttackAction.AimAssistTargetHealth);
            Assert.Less(
                Mathf.Abs(previewWorldPoint.y - stableBodyY),
                0.18f,
                "Direct boss aim should use the stable body hit height instead of jumping to a high child collider.");
            Assert.Less(
                previewWorldPoint.y,
                highHitCollider.bounds.center.y - 0.45f,
                "The preview point should stay below the forced high collider hit.");
            Assert.IsTrue(rangedBasicAttackAction.TryFire());
            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, previewDirection),
                0.5f,
                "The fired projectile should match the stabilized boss aim preview.");

            UnityEngine.Object.Destroy(highHitProxy);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FrontlineFireReticleStaysScreenCenteredWithAimAssistEnabled()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "mobile HUD");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            rangedBasicAttackAction.ClearAimInput();
            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            closeThreatRoot.transform.position =
                player.transform.position + player.transform.forward * 4.2f + player.transform.right * 1.0f;
            closeThreatHealth.ResetHealthToFull();
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(
                GetBool(mobileHud, "fireAimReticleUsesScreenCenter"),
                "Frontline HUD reticle should stay at the input crosshair height instead of following monster body height.");
            Assert.IsFalse(
                GetBool(mobileHud, "fireAimReticleFollowsAssist"),
                "Aim assist should be communicated through reticle emphasis, not by dragging the reticle to the target.");
            Assert.IsTrue(
                GetBool(rangedBasicAttackAction, "useAimAssist"),
                "The projectile aim assist should stay enabled while the HUD reticle remains centered.");

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }
        }

        [UnityTest]
        public IEnumerator FrontlineGuidedPlayerActionFlowClearsAsCleanRoute()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            FrontlineWaveStageProfile stageProfile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            Assert.NotNull(stageProfile);
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            SummonEnergyLadder energyLadder =
                RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(RequireRoot(PocketOwnerRootName), "pocket VFX cue bridge");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(RequireRoot(HudRootName), "overlay HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "screen cue presenter");
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            Collider closeThreatCollider = RequireCombatHitCollider(closeThreatRoot, closeThreatHealth, "close threat");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            float guidedLaneZ = Mathf.Lerp(laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ, 0.4f);
            player.transform.position = laneSpace.GetLaneWorldPoint(0f, guidedLaneZ, player.transform.position.y);
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return WaitSeconds(0.22f);

            int closeThreatShotCount = 0;
            float closeThreatAttackSeconds = 0f;
            float fireIntervalSeconds = GetFloat(rangedBasicAttackAction, "fireIntervalSeconds");
            while (closeThreatHealth.IsAlive && closeThreatShotCount < 10)
            {
                Assert.IsTrue(
                    rangedBasicAttackAction.TryFire(),
                    "The Frontline review should start with actual ranged basic fire against the local close threat.");
                LaneActionProjectile closeThreatShot = RequireActivePlayerRangedProjectile();
                Assert.IsTrue(
                    closeThreatShot.TryApplyImpact(closeThreatCollider, closeThreatShot.transform.position),
                    "The close threat should fall through the real projectile impact path.");
                closeThreatShotCount++;

                if (closeThreatHealth.IsAlive)
                {
                    yield return WaitSeconds(fireIntervalSeconds + 0.02f);
                    closeThreatAttackSeconds += fireIntervalSeconds + 0.02f;
                }
            }

            Assert.IsFalse(closeThreatHealth.IsAlive);
            Assert.That(
                closeThreatShotCount,
                Is.InRange(3, 6),
                "The local threat should take a short burst, not a single accidental hit or a long attrition string.");
            pocketOwner.Tick(0f);
            Assert.IsTrue(pocketOwner.CloseThreatDefeated);
            Assert.IsTrue(pocketOwner.IsSummonBlockOpportunityCueActive);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:prepare_summon(cue_window)"));

            float energyReadySeconds = TickEnergyToTier(energyLadder, 1, 0.25f);
            float reliefSeconds = pocketOwner.PressureReliefRemainingSeconds + 0.02f;
            pocketOwner.Tick(reliefSeconds);
            Assert.IsTrue(pocketOwner.IsAwaitingSummonPressureBlock);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:summon_now(boss_curtain)"));

            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            Assert.Greater(summonSlot1Action.ActivePressureScreenCount, 0);
            SummonPressureScreen activeScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile bossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(activeScreen.TryIntercept(bossProjectile));

            int followupWindowCueCountBefore = cameraCueDriver.SummonFollowupWindowCueRequestCount;
            int followupWindowVfxCountBefore = pocketVfxCueBridge.FollowupWindowCueRequestCount;
            pocketOwner.Tick(0f);
            Assert.IsTrue(pocketOwner.BlockedBossPressureWithSummon);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:confirm(followup_window)"));
            Assert.AreEqual(followupWindowCueCountBefore + 1, cameraCueDriver.SummonFollowupWindowCueRequestCount);
            Assert.AreEqual(followupWindowVfxCountBefore + 1, pocketVfxCueBridge.FollowupWindowCueRequestCount);
            Assert.AreEqual(1f, screenCuePresenter.LastFollowupWindowRouteScale, 0.001f);
            Assert.AreEqual("Followup.Window", screenCuePresenter.LastCueId);
            Assert.IsTrue(energyLadder.CanSpend);
            Assert.AreEqual(
                stageProfile.CleanFollowupEnergyPulseOverride,
                pocketOwner.SummonFollowupEnergyPulse,
                0.001f);
            Assert.AreEqual(2, energyLadder.AvailableTier);

            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Assert.IsTrue(skill1Action.TryUseSkill1());
            Assert.AreEqual(2, skill1Action.LastSpentTier);
            Assert.AreEqual(2, skill1Action.LastFiredProjectileCount);
            Assert.AreEqual(2, ApplyActivePlayerSkillProjectiles(bossHitCollider));
            pocketOwner.Tick(0f);
            Assert.AreEqual(2, pocketOwner.HighestSummonFollowupSkillTier);
            Assert.AreEqual(2, pocketOwner.HighestSkill1FollowupHitTier);
            Assert.GreaterOrEqual(pocketOwner.Skill1FollowupDamage, 200f);

            int cleanRecordEventCount = 0;
            BossBarragePocketReviewOwner.RouteResultRecord cleanEventRecord = default;
            pocketOwner.ResultRecordCommitted += record =>
            {
                cleanRecordEventCount++;
                cleanEventRecord = record;
            };
            int resultCueCountBeforeClear = screenCuePresenter.ResultCueRequestCount;
            int pocketClearVfxCueCountBefore = pocketVfxCueBridge.PocketClearCueRequestCount;
            pocketOwner.Tick(0.77f);

            float guidedSuccessSeconds = closeThreatAttackSeconds
                + energyReadySeconds
                + reliefSeconds
                + GetFloat(pocketOwner, "skill1FollowupClearDelaySeconds");
            Assert.That(guidedSuccessSeconds, Is.InRange(8f, 12.8f));
            Assert.IsTrue(pocketOwner.IsCleared);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.Cleared, pocketOwner.CurrentPhase);
            Assert.IsTrue(pocketOwner.HasCommittedResultRecord);
            Assert.AreEqual(1, pocketOwner.ResultRecordCommitCount);
            Assert.AreEqual(1, cleanRecordEventCount);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteResultKind.CleanFollowupClear,
                pocketOwner.LastResultRecord.ResultKind);
            Assert.IsTrue(pocketOwner.LastResultRecord.IsClear);
            Assert.AreEqual(BossBarragePocketReviewOwner.RouteFailureReason.None, pocketOwner.LastResultRecord.FailureReason);
            Assert.AreEqual("clean_clear", pocketOwner.LastResultRecord.DecisionState);
            Assert.AreEqual("clean_followup", pocketOwner.LastResultRecord.DecisionReadout);
            Assert.That(pocketOwner.LastResultRecord.Title, Does.Contain("PRESSURE BROKEN"));
            Assert.That(pocketOwner.LastResultRecord.Summary, Does.Contain("Summon opening confirmed"));
            Assert.AreEqual("Clean summon follow-up", pocketOwner.LastResultRecord.RouteLabel);
            Assert.That(pocketOwner.LastResultRecord.RewardHook, Does.Contain("Clean survival logged"));
            Assert.That(pocketOwner.LastResultRecord.NextObjective, Does.Contain("counter pressure"));
            Assert.That(pocketOwner.LastResultRecord.CompletionReadout, Does.Contain("followup:recorded"));
            Assert.That(pocketOwner.LastResultRecord.ProofReadout, Does.Contain("log:committed"));
            Assert.AreEqual(pocketOwner.LastResultRecord.ResultKind, cleanEventRecord.ResultKind);
            Assert.AreEqual(resultCueCountBeforeClear + 1, screenCuePresenter.ResultCueRequestCount);
            Assert.AreEqual("Pocket.Cleared", screenCuePresenter.LastCueId);
            Assert.AreEqual(pocketClearVfxCueCountBefore + 1, pocketVfxCueBridge.PocketClearCueRequestCount);
            Assert.AreEqual(pocketOwner.LastResultRecord.Title, overlayHud.ResultTitleReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.Summary, overlayHud.ResultSummaryReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.RouteLabel, overlayHud.ResultRouteReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.RewardHook, overlayHud.ResultRewardReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.NextObjective, overlayHud.ResultNextObjectiveReadout);
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Summon follow-up"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:clean_clear(clean_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:avoided(none)"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Summon opening confirmed"));

            SetField(pocketOwner, "counterWaveStabilized", true);
            Assert.AreEqual("Clean summon follow-up", overlayHud.ResultRouteReadout);
            Assert.That(overlayHud.ResultSummaryReadout, Does.Contain("Summon opening confirmed"));
            Assert.That(overlayHud.ResultRewardReadout, Does.Contain("Clean survival logged"));

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }
        }

        [UnityTest]
        public IEnumerator FrontlineRouteStabilityZeroDoesNotFailWhilePlayerHealthSurvives()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(RequireRoot(HudRootName), "overlay HUD");
            PlayerMovementController player = UnityEngine.Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.NotNull(player, "Frontline route collapse test needs the scene player.");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            Assert.IsTrue(playerHealth.IsAlive);
            Assert.IsTrue(pocketOwner.IsRouteStabilityActive);

            SetField(pocketOwner, "routeStability01", 0.02f);
            pocketOwner.Tick(1f);

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsFalse(pocketOwner.IsFailed);
            Assert.IsFalse(pocketOwner.FailedFromRouteStabilityCollapse);
            Assert.AreEqual(BossBarragePocketReviewOwner.RouteFailureReason.None, pocketOwner.FailureReason);
            Assert.IsTrue(playerHealth.IsAlive, "HP, not route stability, should be the actual fail state.");
            Assert.IsFalse(reviewHud.ShouldShowResultBanner);
            Assert.AreEqual(string.Empty, overlayHud.ResultTitleReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("HP is the fail state"));
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("pressure 0%"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Pending 0/3"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("close:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:survive(keep_hp)"));
        }

        [UnityTest]
        public IEnumerator FrontlinePlayerDefeatKeepsPlayerDownFailureReason()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(RequireRoot(HudRootName), "overlay HUD");
            PlayerMovementController player = UnityEngine.Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.NotNull(player, "Frontline player defeat test needs the scene player.");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");

            int failRecordEventCount = 0;
            BossBarragePocketReviewOwner.RouteResultRecord failEventRecord = default;
            int routeDecisionCountBeforeFail = pocketOwner.RouteDecisionChangeCount;
            BossBarragePocketReviewOwner.RouteDecisionSnapshot failDecisionEvent = default;
            pocketOwner.ResultRecordCommitted += record =>
            {
                failRecordEventCount++;
                failEventRecord = record;
            };
            pocketOwner.RouteDecisionChanged += snapshot => failDecisionEvent = snapshot;
            playerHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                playerHealth.MaxHealth + 1f,
                player.transform.position,
                Vector3.back,
                0f));
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsFailed);
            Assert.AreEqual(BossBarragePocketReviewOwner.RouteFailureReason.PlayerDown, pocketOwner.FailureReason);
            Assert.IsTrue(pocketOwner.HasCommittedResultRecord);
            Assert.AreEqual(1, pocketOwner.ResultRecordCommitCount);
            Assert.AreEqual(1, failRecordEventCount);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteResultKind.PlayerDownFail,
                pocketOwner.LastResultRecord.ResultKind);
            Assert.Greater(pocketOwner.RouteDecisionChangeCount, routeDecisionCountBeforeFail);
            Assert.AreEqual("failed", failDecisionEvent.State);
            Assert.AreEqual("player_down", failDecisionEvent.Readout);
            Assert.That(failDecisionEvent.IncentiveCue, Does.Contain("Failure analysis"));
            Assert.IsFalse(pocketOwner.LastResultRecord.IsClear);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteFailureReason.PlayerDown,
                pocketOwner.LastResultRecord.FailureReason);
            Assert.AreEqual("failed", pocketOwner.LastResultRecord.DecisionState);
            Assert.AreEqual("player_down", pocketOwner.LastResultRecord.DecisionReadout);
            Assert.AreEqual("PLAYER DOWN", pocketOwner.LastResultRecord.Title);
            Assert.That(pocketOwner.LastResultRecord.Summary, Does.Contain("Player HP reached zero"));
            Assert.AreEqual("Player down", pocketOwner.LastResultRecord.RouteLabel);
            Assert.That(pocketOwner.LastResultRecord.RewardHook, Does.Contain("Failure analysis logged"));
            Assert.That(pocketOwner.LastResultRecord.NextObjective, Does.Contain("protect HP"));
            Assert.That(pocketOwner.LastResultRecord.CompletionReadout, Does.Contain("decision:failed(player_down)"));
            Assert.That(pocketOwner.LastResultRecord.ProofReadout, Does.Contain("log:failed"));
            Assert.AreEqual(pocketOwner.LastResultRecord.ResultKind, failEventRecord.ResultKind);
            Assert.AreEqual(pocketOwner.LastResultRecord.Title, overlayHud.ResultTitleReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.Summary, overlayHud.ResultSummaryReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.RouteLabel, overlayHud.ResultRouteReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.RewardHook, overlayHud.ResultRewardReadout);
            Assert.AreEqual(pocketOwner.LastResultRecord.NextObjective, overlayHud.ResultNextObjectiveReadout);
            Assert.AreEqual("PLAYER DOWN", reviewHud.ResultBannerTitle);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Player HP reached zero"));
            Assert.AreEqual("PLAYER DOWN", overlayHud.ResultTitleReadout);
            Assert.That(overlayHud.ResultSummaryReadout, Does.Contain("Player HP reached zero"));
            Assert.That(overlayHud.ResultRewardReadout, Does.Contain("Failure analysis logged"));
            Assert.AreEqual(pocketOwner.RouteIncentiveCue, reviewHud.RouteIncentiveReadout);
            Assert.That(reviewHud.RouteIncentiveReadout, Does.Contain("Failure analysis"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("reason player down"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:failed(player_down)"));
        }

        private static void AssertRectInsideViewport(
            Rect rect,
            Vector2 viewport,
            string label,
            BossBarrageLaneReviewHud.PremiumHudLayout layout)
        {
            Assert.GreaterOrEqual(rect.xMin, 0f, $"{viewport} {label} xMin should stay onscreen. Layout stacked={layout.IsStacked}.");
            Assert.GreaterOrEqual(rect.yMin, 0f, $"{viewport} {label} yMin should stay onscreen. Layout stacked={layout.IsStacked}.");
            Assert.LessOrEqual(rect.xMax, viewport.x, $"{viewport} {label} xMax should stay onscreen. Layout stacked={layout.IsStacked}.");
            Assert.LessOrEqual(rect.yMax, viewport.y, $"{viewport} {label} yMax should stay onscreen. Layout stacked={layout.IsStacked}.");
        }

        private static void AssertNoOverlap(Rect first, Rect second, Vector2 viewport, string firstLabel, string secondLabel)
        {
            Assert.IsFalse(
                first.Overlaps(second),
                $"{viewport} {firstLabel} and {secondLabel} panels should not overlap.");
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
            T[] found = UnityEngine.Object.FindObjectsByType<T>(
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

        private static LaneActionProjectile RequireActivePlayerSkillProjectile()
        {
            LaneActionProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive && projectiles[i].SourceTeam == DamageTeam.Player)
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected an active Player Skill1 projectile.");
            return null;
        }

        private static int ApplyActivePlayerSkillProjectiles(Collider hitCollider)
        {
            LaneActionProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            int hitCount = 0;
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive
                    && projectiles[i].SourceTeam == DamageTeam.Player
                    && projectiles[i].TryApplyImpact(hitCollider, projectiles[i].transform.position))
                {
                    hitCount++;
                }
            }

            return hitCount;
        }

        private static LaneActionProjectile RequireActivePlayerRangedProjectile()
        {
            LaneActionProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            GameObject expectedPrefab = LoadAsset<GameObject>(RangedBasicProjectilePrefabPath);
            float expectedRadius = expectedPrefab.transform.localScale.x;

            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive
                    && projectiles[i].SourceTeam == DamageTeam.Player
                    && Mathf.Abs(projectiles[i].transform.localScale.x - expectedRadius) < 0.001f)
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected an active Player ranged basic projectile.");
            return null;
        }

        private static SummonPressureScreen RequireActiveAllyPressureScreen()
        {
            SummonPressureScreen[] pressureScreens = UnityEngine.Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < pressureScreens.Length; i++)
            {
                if (pressureScreens[i].IsActive && pressureScreens[i].OwnerTeam == DamageTeam.AllySummon)
                {
                    return pressureScreens[i];
                }
            }

            Assert.Fail("Expected an active AllySummon pressure screen.");
            return null;
        }

        private static BossBarrageProjectile RequireActiveBossProjectile()
        {
            BossBarrageProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive)
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected an active boss barrage projectile.");
            return null;
        }

        private static SummonFrontlineProxy CreateActiveFrontlineProxy(string objectName, DamageTeam team)
        {
            GameObject proxyObject = new GameObject(objectName);
            CombatHealth health = proxyObject.AddComponent<CombatHealth>();
            health.ConfigureTeam(team);
            health.ConfigureMaxHealth(100f);
            SummonFrontlineProxy proxy = proxyObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(health);
            proxy.Activate(Vector3.zero, Vector3.forward, 1, 10f, 1f);
            return proxy;
        }

        private static void DeactivateActiveFrontlineProxies(DamageTeam team)
        {
            SummonFrontlineProxy[] proxies = UnityEngine.Object.FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < proxies.Length; i++)
            {
                CombatHealth health = proxies[i].Health;
                if (proxies[i].IsActive && health != null && health.Team == team)
                {
                    proxies[i].Deactivate(SummonFrontlineProxyExitReason.Recalled);
                }
            }
        }

        private static T GetObjectReference<T>(UnityEngine.Object target, string fieldName)
            where T : UnityEngine.Object
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return field.GetValue(target) as T;
        }

        private static void ForcePocketState(BossBarragePocketReviewOwner owner, string stateName)
        {
            Type stateType = typeof(BossBarragePocketReviewOwner).GetNestedType(
                "PocketState",
                BindingFlags.NonPublic);
            Assert.NotNull(stateType, "PocketState enum is missing.");
            object value = Enum.Parse(stateType, stateName);
            SetField(owner, "state", value);
        }

        private static void SetField(UnityEngine.Object target, string fieldName, object value)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            field.SetValue(target, value);
        }

        private static float TickEnergyToTier(SummonEnergyLadder energyLadder, int targetTier, float stepSeconds)
        {
            float elapsedSeconds = 0f;
            float safeStepSeconds = Mathf.Max(0.01f, stepSeconds);
            for (int i = 0; i < 240 && energyLadder.AvailableTier < targetTier; i++)
            {
                energyLadder.Tick(safeStepSeconds);
                elapsedSeconds += safeStepSeconds;
            }

            Assert.GreaterOrEqual(
                energyLadder.AvailableTier,
                targetTier,
                $"Energy ladder should reach tier {targetTier} during the guided frontline flow.");
            return elapsedSeconds;
        }

        private static IEnumerator WaitSeconds(float seconds)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.NotNull(asset, $"Missing asset {path}.");
            return asset;
        }

        private static float GetFloat(UnityEngine.Object target, string fieldName)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return (float)field.GetValue(target);
        }

        private static bool GetBool(UnityEngine.Object target, string fieldName)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return (bool)field.GetValue(target);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{type.Name} is missing private field {fieldName}.");
            return field;
        }
    }
}
