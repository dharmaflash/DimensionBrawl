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
            Assert.That(stageProfile.CombatPromise, Does.Contain("Bodies stay split"));
            Assert.That(stageProfile.EntryCue, Does.Contain("summon route"));
            Assert.That(stageProfile.CounterWaveCue, Does.Contain("Counter wave"));
            Assert.AreEqual(90f, stageProfile.TargetDurationSeconds);
            Assert.AreEqual(0.62f, stageProfile.RouteStabilityStart01, 0.001f);
            Assert.That(stageProfile.RouteCollapseFailDetail, Does.Contain("Route stability collapsed"));
            Assert.Greater(stageProfile.CloseProbeRouteDrainPerSecond, 0f);
            Assert.Greater(stageProfile.CounterWaveRouteDrainPerSecond, stageProfile.CloseProbeRouteDrainPerSecond);
            Assert.Greater(stageProfile.CounterWaveStabilizeRouteBonus01, 0f);
            Assert.That(stageProfile.CounterWaveStabilizedCue, Does.Contain("held"));
            Assert.GreaterOrEqual(stageProfile.BeatCount, 6);
            Assert.GreaterOrEqual(stageProfile.PressureSlotCount, 6);
            Assert.GreaterOrEqual(stageProfile.SourceReferenceCount, 3);
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

            Assert.That(reviewHud.StageBriefingReadout, Does.Contain("Bodies stay split"));
            Assert.That(reviewHud.StageBriefingReadout, Does.Contain("Hold line"));
            Assert.That(reviewHud.CompactStageBriefingReadout, Does.Contain("summon route"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Route 1/3"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Not.Contain("Boss"));
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("line").IgnoreCase);
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Pending 0/3"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("stability 62%"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("target 90"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("close:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("summon:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("followup:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:pending(none)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:pending(none)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:pending(none)"));
            Assert.AreEqual("build_route", pocketOwner.RouteDecisionState);
            Assert.AreEqual("hold_line", pocketOwner.RouteDecisionReadout);
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:build_route(hold_line)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Review-only route record"));
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("stability 62%"));
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
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("First Summon Need"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("summon_slot1_used"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("ScreenCurtain"));
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("summon:recorded"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:confirm(summon_opening)"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Enemy Counter Wave"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("FrontlineBody"));

            ForcePocketState(pocketOwner, "Cleared");
            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(pocketOwner, "skill1FollowupHitConfirmed", true);
            SetField(pocketOwner, "skill1FollowupDamage", 123f);
            SetField(pocketOwner, "resultElapsedSeconds", 42f);

            Assert.IsTrue(reviewHud.ShouldShowResultBanner);
            Assert.AreEqual("FRONTLINE STABILIZED", reviewHud.ResultBannerTitle);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Summon route analyzed"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Route 3/3"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Record S"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("42.0/90.0s"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Not.Contain("BOSS CLEAR"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Summon follow-up"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("close:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("summon:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("followup:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:avoided(none)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:not_needed(clean_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:not_needed(clean_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:clean_clear(clean_followup)"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Suppression Result"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("route_record_committed"));
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
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("frontline x1.00 open"));
            float openDrain = pocketOwner.CurrentRouteStabilityDrainPerSecond;
            Assert.Greater(openDrain, 0f);

            SummonFrontlineProxy allyProxy = CreateActiveFrontlineProxy("Test_Ally_FrontlineProxy", DamageTeam.AllySummon);
            Assert.AreEqual(1, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(0, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(0.70f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.Less(pocketOwner.CurrentRouteStabilityDrainPerSecond, openDrain);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("frontline x0.70 ally"));

            SummonFrontlineProxy enemyProxy = CreateActiveFrontlineProxy("Test_Enemy_FrontlineProxy", DamageTeam.Enemy);
            Assert.AreEqual(1, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(1, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(0.85f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("frontline x0.85 contest"));

            allyProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
            Assert.AreEqual(0, pocketOwner.ActiveAllyFrontlineProxyCount);
            Assert.AreEqual(1, pocketOwner.ActiveEnemyFrontlineProxyCount);
            Assert.AreEqual(1.20f, pocketOwner.CurrentFrontlinePresenceDrainScale, 0.001f);
            Assert.Greater(pocketOwner.CurrentRouteStabilityDrainPerSecond, openDrain);
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("frontline x1.20 enemy"));

            enemyProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
        }

        [UnityTest]
        public IEnumerator FrontlineEnemyBodyPressureRecordsCounterWave()
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
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SummonFrontlineProxy enemyProxy = CreateActiveFrontlineProxy("Test_CounterWave_EnemyProxy", DamageTeam.Enemy);
            int counterCueCountBeforeEnemy = screenCuePresenter.CounterWaveCueRequestCount;
            int counterVfxCueCountBeforeEnemy = pocketVfxCueBridge.CounterWaveCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.AreEqual("recorded", pocketOwner.CounterWaveRecordState);
            Assert.AreEqual(BossBarragePocketReviewOwner.CounterWaveSource.EnemyFrontlineBody, pocketOwner.CounterWaveObservedSource);
            Assert.AreEqual("enemy_body", pocketOwner.CounterWaveSourceReadout);
            Assert.IsFalse(pocketOwner.IsCounterWaveStabilized);
            Assert.AreEqual("pending", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("awaiting", pocketOwner.CounterWaveAnswerReadout);
            Assert.IsFalse(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("pending", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("awaiting_answer", pocketOwner.CounterWaveFinalWindowReadout);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.CounterWave, pocketOwner.CurrentPhase);
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("Counter wave"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Hold counter wave"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter:recorded(enemy_body)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:pending(awaiting)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_window:pending(awaiting_answer)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:recorded(enemy_body)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:pending(awaiting)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:pending(awaiting_answer)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovery_needed(answer_counter)"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Enemy Counter Wave"));
            Assert.Greater(screenCuePresenter.CounterWaveCueRequestCount, counterCueCountBeforeEnemy);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.EnemyFrontlineBody,
                screenCuePresenter.LastCounterWaveSource);
            Assert.Greater(pocketVfxCueBridge.CounterWaveCueRequestCount, counterVfxCueCountBeforeEnemy);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.CounterWaveSource.EnemyFrontlineBody,
                pocketVfxCueBridge.LastCounterWaveSource);
            Assert.AreEqual(CombatVfxCueId.EnemyLinePressureActive, pocketVfxCueBridge.CounterWaveCueId);

            float stabilityBeforeAnswer = pocketOwner.RouteStability01;
            SummonFrontlineProxy allyProxy = CreateActiveFrontlineProxy("Test_CounterWave_AllyProxy", DamageTeam.AllySummon);
            int counterAnswerCueCountBeforeAlly = screenCuePresenter.CounterWaveAnswerCueRequestCount;
            int followupCueCountBeforeAlly = screenCuePresenter.FollowupCueRequestCount;
            int counterStabilizedVfxCueCountBeforeAlly = pocketVfxCueBridge.CounterWaveStabilizedCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsCounterWaveStabilized);
            Assert.IsTrue(pocketOwner.IsCounterWaveFinalWindowOpened);
            Assert.AreEqual("stabilized", pocketOwner.CounterWaveAnswerState);
            Assert.AreEqual("ally_hold", pocketOwner.CounterWaveAnswerReadout);
            Assert.AreEqual("opened", pocketOwner.CounterWaveFinalWindowState);
            Assert.AreEqual("final_followup", pocketOwner.CounterWaveFinalWindowReadout);
            Assert.AreEqual(stageProfile.CounterWaveStabilizeRouteBonus01, pocketOwner.LastCounterWaveStabilityBonus, 0.001f);
            Assert.Greater(pocketOwner.LastCounterWaveFinalWindowDuration, 0f);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.SummonFollowup, pocketOwner.CurrentPhase);
            Assert.Greater(pocketOwner.RouteStability01, stabilityBeforeAnswer);
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("Confirm"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("route window"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Follow-Up Window"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_answer:stabilized(ally_hold)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("decision:recovered(final_window)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_answer:stabilized(ally_hold)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovered(final_window)"));
            Assert.Greater(screenCuePresenter.CounterWaveAnswerCueRequestCount, counterAnswerCueCountBeforeAlly);
            Assert.Greater(screenCuePresenter.FollowupCueRequestCount, followupCueCountBeforeAlly);
            Assert.AreEqual("ally_hold", screenCuePresenter.LastCounterWaveAnswer);
            Assert.Greater(
                pocketVfxCueBridge.CounterWaveStabilizedCueRequestCount,
                counterStabilizedVfxCueCountBeforeAlly);
            Assert.AreEqual(CombatVfxCueId.EliteShieldSignal, pocketVfxCueBridge.CounterWaveStabilizedCueId);

            CombatHealth bossHealth = GetObjectReference<CombatHealth>(pocketOwner, "bossHealth");
            Assert.NotNull(bossHealth, "Counter path clear needs the pocket owner's boss damage observer.");
            float bossHealthBeforeFinalHit = bossHealth.CurrentHealth;
            SetField(pocketOwner, "skillUsesAtSummonBreakStart", -1);
            Assert.IsTrue(bossHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                75f,
                bossHealth.transform.position,
                Vector3.forward,
                0f)));
            Assert.Less(bossHealth.CurrentHealth, bossHealthBeforeFinalHit);
            pocketOwner.Tick(1f);

            Assert.IsTrue(pocketOwner.Skill1FollowupHitConfirmed);
            Assert.IsTrue(pocketOwner.IsCleared);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.Cleared, pocketOwner.CurrentPhase);
            Assert.That(reviewHud.ResultBannerTitle, Does.Contain("FRONTLINE STABILIZED"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Counter wave held"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("final follow-up"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Counter recovery"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Counter recovery"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("followup:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter_window:opened(final_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:recovery_clear(counter_recovery)"));

            allyProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
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
            BossSummonPressureAction bossSummonPressureAction =
                UnityEngine.Object.FindFirstObjectByType<BossSummonPressureAction>();
            Assert.NotNull(bossSummonPressureAction, "Frontline review scene should keep the boss summon pressure action.");

            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(bossSummonPressureAction, "totalReleaseCount", bossSummonPressureAction.TotalReleaseCount + 1);
            int counterCueCountBeforeRelease = screenCuePresenter.CounterWaveCueRequestCount;
            int counterVfxCueCountBeforeRelease = pocketVfxCueBridge.CounterWaveCueRequestCount;
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
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Hold counter wave"));
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
        }

        [UnityTest]
        public IEnumerator FrontlineGuidedPlayerActionFlowClearsAsCleanRoute()
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
            Assert.IsTrue(energyLadder.CanSpend);

            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Assert.IsTrue(skill1Action.TryUseSkill1());
            LaneActionProjectile followupProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(followupProjectile.TryApplyImpact(bossHitCollider, followupProjectile.transform.position));
            pocketOwner.Tick(0f);

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
            Assert.AreEqual(resultCueCountBeforeClear + 1, screenCuePresenter.ResultCueRequestCount);
            Assert.AreEqual("Pocket.Cleared", screenCuePresenter.LastCueId);
            Assert.AreEqual(pocketClearVfxCueCountBefore + 1, pocketVfxCueBridge.PocketClearCueRequestCount);
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Summon follow-up"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:clean_clear(clean_followup)"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:avoided(none)"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Summon route analyzed"));

            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }
        }

        [UnityTest]
        public IEnumerator FrontlineRouteStabilityCollapseFailsRouteWithoutPlayerDefeat()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            PlayerMovementController player = UnityEngine.Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.NotNull(player, "Frontline route collapse test needs the scene player.");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            Assert.IsTrue(playerHealth.IsAlive);
            Assert.IsTrue(pocketOwner.IsRouteStabilityActive);

            SetField(pocketOwner, "routeStability01", 0.02f);
            pocketOwner.Tick(1f);

            Assert.IsTrue(pocketOwner.IsFailed);
            Assert.IsTrue(pocketOwner.FailedFromRouteStabilityCollapse);
            Assert.AreEqual(
                BossBarragePocketReviewOwner.RouteFailureReason.RouteStabilityCollapsed,
                pocketOwner.FailureReason);
            Assert.IsTrue(playerHealth.IsAlive, "Route collapse should be a frontline failure, not a hidden HP defeat.");
            Assert.AreEqual("LINE COLLAPSED", reviewHud.ResultBannerTitle);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Route stability collapsed"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Not.Contain("Player down"));
            Assert.That(reviewHud.RouteStabilityReadout, Does.Contain("stability 0%"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("Incomplete 0/3"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("reason route collapse"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("close:pending"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("decision:failed(route_collapse)"));
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
            PlayerMovementController player = UnityEngine.Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.NotNull(player, "Frontline player defeat test needs the scene player.");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");

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
            Assert.AreEqual("LINE COLLAPSED", reviewHud.ResultBannerTitle);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Player down"));
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

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{type.Name} is missing private field {fieldName}.");
            return field;
        }
    }
}
