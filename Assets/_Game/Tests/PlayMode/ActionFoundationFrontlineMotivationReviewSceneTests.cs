using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Combat;
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
            Assert.AreEqual(90f, stageProfile.TargetDurationSeconds);
            Assert.AreEqual(0.62f, stageProfile.RouteStabilityStart01, 0.001f);
            Assert.That(stageProfile.RouteCollapseFailDetail, Does.Contain("Route stability collapsed"));
            Assert.Greater(stageProfile.CloseProbeRouteDrainPerSecond, 0f);
            Assert.Greater(stageProfile.CounterWaveRouteDrainPerSecond, stageProfile.CloseProbeRouteDrainPerSecond);
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
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:pending"));
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
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("First Summon Need"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("summon_slot1_used"));
            Assert.That(reviewHud.PressureSlotReadout, Does.Contain("ScreenCurtain"));
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("summon:recorded"));
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
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:avoided"));
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

            Assert.IsFalse(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.AreEqual("pending", pocketOwner.CounterWaveRecordState);

            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SummonFrontlineProxy enemyProxy = CreateActiveFrontlineProxy("Test_CounterWave_EnemyProxy", DamageTeam.Enemy);
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.AreEqual("recorded", pocketOwner.CounterWaveRecordState);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:recorded"));
            Assert.That(reviewHud.StageBeatReadout, Does.Contain("Enemy Counter Wave"));

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
            BossSummonPressureAction bossSummonPressureAction =
                UnityEngine.Object.FindFirstObjectByType<BossSummonPressureAction>();
            Assert.NotNull(bossSummonPressureAction, "Frontline review scene should keep the boss summon pressure action.");

            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(bossSummonPressureAction, "totalReleaseCount", bossSummonPressureAction.TotalReleaseCount + 1);
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsCounterWaveCompletionRecorded);
            Assert.That(pocketOwner.CompletionRecordReadout, Does.Contain("counter:recorded"));
            Assert.That(reviewHud.RouteRecordReadout, Does.Contain("counter:recorded"));
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

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{type.Name} is missing private field {fieldName}.");
            return field;
        }
    }
}
