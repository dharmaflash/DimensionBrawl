using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationFrontlineAimTimelineReportTests
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string CsvPath = "C:/tmp/DimensionBrawl-FrontlineAimTimelineReport.csv";
        private const string ReportPath = "C:/tmp/DimensionBrawl-FrontlineAimTimelineReport.md";
        private const int WarmupFrames = 30;
        private const int SamplesPerScenario = 120;

        private enum AimTimelineScenario
        {
            NoTargetCenter,
            CloseThreatAssist,
            MixedSceneCenter,
            BossDirect,
            BossHighCollider,
            BossAimSweep
        }

        [UnityTest]
        public IEnumerator WritesFrontlineAimTimelineReport()
        {
            float previousTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            List<AimSample> samples = new List<AimSample>();

            try
            {
                foreach (AimTimelineScenario scenario in new[]
                {
                    AimTimelineScenario.NoTargetCenter,
                    AimTimelineScenario.CloseThreatAssist,
                    AimTimelineScenario.MixedSceneCenter,
                    AimTimelineScenario.BossDirect,
                    AimTimelineScenario.BossHighCollider,
                    AimTimelineScenario.BossAimSweep
                })
                {
                    EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                    yield return null;

                    AimTimelineContext context = BuildContext();
                    yield return PrepareScenario(context, scenario);
                    yield return WarmupScenario(context);
                    yield return CaptureScenario(context, scenario, samples);
                }

                WriteReports(samples);
                List<AimScenarioSummary> summaries = BuildSummaries(samples);
                Assert.Less(
                    RequireSummary(summaries, AimTimelineScenario.MixedSceneCenter).MaxWorldY,
                    4f,
                    "Mixed frontline aim preview should stay near the selected target instead of jumping to a far raycast hit.");
                Assert.Less(
                    RequireSummary(summaries, AimTimelineScenario.BossDirect).MaxWorldY,
                    4f,
                    "Boss soft-assist preview should stay near the selected target instead of jumping to a far raycast hit.");
                Assert.Less(
                    RequireSummary(summaries, AimTimelineScenario.BossHighCollider).MaxWorldY,
                    4f,
                    "Elevated boss collider diagnostics should remain bounded by the selected target preview point.");

                Assert.IsTrue(File.Exists(CsvPath), "Frontline aim timeline CSV should be written.");
                Assert.IsTrue(File.Exists(ReportPath), "Frontline aim timeline report should be written.");
                Assert.Greater(samples.Count, 0, "Frontline aim timeline report should contain samples.");
            }
            finally
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private static AimTimelineContext BuildContext()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "mobile HUD");
            SummonLaneSpace laneSpace =
                RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "summon lane space");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossRootCollider = RequireComponent<Collider>(bossRoot, "boss root collider");
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
            rangedBasicAttackAction.SetFireHeld(false);
            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);

            return new AimTimelineContext(
                player,
                combatModeController,
                aimController,
                rangedBasicAttackAction,
                targetSelector,
                cameraController,
                mobileHud,
                laneSpace,
                bossRoot,
                bossHealth,
                bossRootCollider,
                closeThreatRoot,
                closeThreatHealth);
        }

        private static IEnumerator PrepareScenario(AimTimelineContext context, AimTimelineScenario scenario)
        {
            SetField(context.TargetSelector, "includeActiveHostileSummons", false);
            ClearTargetSelector(context.TargetSelector);
            context.AimController.SetAimInput(Vector2.zero);
            context.RangedBasicAttackAction.ClearAimInput();
            SetField(context.RangedBasicAttackAction, "useFixedCenterAimViewport", true);
            SetField(context.RangedBasicAttackAction, "aimInputViewportOffsetX", 0.39f);
            SetField(context.RangedBasicAttackAction, "aimInputViewportOffsetY", 0.20f);
            SetTargetCandidates(context.TargetSelector, Array.Empty<CombatHealth>());

            switch (scenario)
            {
                case AimTimelineScenario.NoTargetCenter:
                    MoveOutOfAimRay(context.BossRoot, context.LaneSpace, 80f, context.BossRoot.transform.position.y);
                    MoveOutOfAimRay(context.CloseThreatRoot, context.LaneSpace, -80f, context.CloseThreatRoot.transform.position.y);
                    break;
                case AimTimelineScenario.CloseThreatAssist:
                    MoveOutOfAimRay(context.BossRoot, context.LaneSpace, 80f, context.BossRoot.transform.position.y);
                    SetTargetCandidates(context.TargetSelector, context.CloseThreatHealth);
                    PlaceCloseThreatNearCenter(context);
                    context.CloseThreatHealth.ResetHealthToFull();
                    context.TargetSelector.NotifyTargetContact(context.CloseThreatHealth);
                    context.TargetSelector.RefreshTarget();
                    break;
                case AimTimelineScenario.MixedSceneCenter:
                    SetTargetCandidates(context.TargetSelector, context.CloseThreatHealth, context.BossHealth);
                    context.CloseThreatHealth.ResetHealthToFull();
                    context.BossHealth.ResetHealthToFull();
                    context.TargetSelector.RefreshTarget();
                    break;
                case AimTimelineScenario.BossDirect:
                    MoveOutOfAimRay(
                        context.CloseThreatRoot,
                        context.LaneSpace,
                        -80f,
                        context.CloseThreatRoot.transform.position.y);
                    SetTargetCandidates(context.TargetSelector, context.BossHealth);
                    context.BossHealth.ResetHealthToFull();
                    context.TargetSelector.NotifyTargetContact(context.BossHealth);
                    context.TargetSelector.RefreshTarget();
                    break;
                case AimTimelineScenario.BossHighCollider:
                    MoveOutOfAimRay(
                        context.CloseThreatRoot,
                        context.LaneSpace,
                        -80f,
                        context.CloseThreatRoot.transform.position.y);
                    SetTargetCandidates(context.TargetSelector, context.BossHealth);
                    context.BossHealth.ResetHealthToFull();
                    SetField(context.RangedBasicAttackAction, "useFixedCenterAimViewport", false);
                    SetField(context.RangedBasicAttackAction, "aimInputViewportOffsetY", 0.45f);
                    context.AimController.SetAimInput(Vector2.up);
                    context.RangedBasicAttackAction.SetAimInput(Vector2.up);
                    context.TargetSelector.NotifyTargetContact(context.BossHealth);
                    context.TargetSelector.RefreshTarget();
                    CreateHighBossAimProxy(context);
                    break;
                case AimTimelineScenario.BossAimSweep:
                    MoveOutOfAimRay(
                        context.CloseThreatRoot,
                        context.LaneSpace,
                        -80f,
                        context.CloseThreatRoot.transform.position.y);
                    SetTargetCandidates(context.TargetSelector, context.BossHealth);
                    context.BossHealth.ResetHealthToFull();
                    SetField(context.RangedBasicAttackAction, "useFixedCenterAimViewport", false);
                    context.TargetSelector.NotifyTargetContact(context.BossHealth);
                    context.TargetSelector.RefreshTarget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
            }

            Physics.SyncTransforms();
            yield return null;
        }

        private static IEnumerator WarmupScenario(AimTimelineContext context)
        {
            for (int i = 0; i < WarmupFrames; i++)
            {
                Physics.SyncTransforms();
                context.RangedBasicAttackAction.TryGetAimPreviewDirection(out _);
                yield return null;
            }
        }

        private static IEnumerator CaptureScenario(
            AimTimelineContext context,
            AimTimelineScenario scenario,
            List<AimSample> samples)
        {
            for (int step = 0; step < SamplesPerScenario; step++)
            {
                if (scenario == AimTimelineScenario.BossAimSweep)
                {
                    float x = Mathf.Sin(step * 0.17f) * 0.65f;
                    float y = Mathf.Cos(step * 0.13f) * 0.35f;
                    Vector2 input = new Vector2(x, y);
                    context.AimController.SetAimInput(input);
                    context.RangedBasicAttackAction.SetAimInput(input);
                }

                Physics.SyncTransforms();
                samples.Add(CaptureSample(context, scenario.ToString(), step));
                yield return null;
            }
        }

        private static AimSample CaptureSample(AimTimelineContext context, string scenario, int step)
        {
            PlayerRangedBasicAttackAction action = context.RangedBasicAttackAction;
            bool hasActionViewport = action.TryGetAimPreviewViewportPoint(out Vector2 actionViewport);
            bool hasWorldPoint = action.TryGetAimPreviewWorldPoint(out Vector3 worldPoint);
            bool hasDirection = action.TryGetAimPreviewDirection(out Vector3 direction);
            bool hasAssistViewport = action.TryGetAimAssistPreviewViewportPoint(out Vector2 assistViewport);
            Vector2 inputViewport = ResolveInputViewport(action);
            Vector2 hudViewport = ResolveHudViewport(context.MobileHud, hasActionViewport, actionViewport);
            CombatHealth targetHealth = action.AimAssistTargetHealth;
            bool hasTargetViewport = TryResolveTargetViewport(
                context.CameraController,
                action,
                targetHealth,
                out Vector2 targetViewport);

            return new AimSample
            {
                Scenario = scenario,
                Step = step,
                Frame = Time.frameCount,
                TimeSeconds = Time.time,
                AimInput = action.AimInput,
                InputViewport = inputViewport,
                HasHudViewport = true,
                HudViewport = hudViewport,
                HasActionViewport = hasActionViewport,
                ActionViewport = actionViewport,
                HasAssistViewport = hasAssistViewport,
                AssistViewport = assistViewport,
                HasTargetViewport = hasTargetViewport,
                TargetViewport = targetViewport,
                HasWorldPoint = hasWorldPoint,
                WorldPoint = worldPoint,
                HasDirection = hasDirection,
                Direction = direction,
                RawDirection = action.LastRawAimDirection,
                AssistDirection = action.LastAimAssistDirection,
                HasAimAssistTarget = action.HasAimAssistTarget,
                AimAssistStrength01 = action.AimAssistStrength01,
                TargetName = targetHealth != null ? targetHealth.name : string.Empty
            };
        }

        private static Vector2 ResolveInputViewport(PlayerRangedBasicAttackAction action)
        {
            if (GetBool(action, "useFixedCenterAimViewport"))
            {
                return new Vector2(0.5f, 0.5f);
            }

            float deadZone = GetFloat(action, "aimInputDeadZone");
            Vector2 input = action.AimInput.sqrMagnitude > deadZone * deadZone
                ? Vector2.ClampMagnitude(action.AimInput, 1f)
                : Vector2.zero;
            return new Vector2(
                Mathf.Clamp01(0.5f + input.x * GetFloat(action, "aimInputViewportOffsetX")),
                Mathf.Clamp01(0.5f + input.y * GetFloat(action, "aimInputViewportOffsetY")));
        }

        private static Vector2 ResolveHudViewport(
            BossBarrageLaneReviewMobileHud mobileHud,
            bool hasActionViewport,
            Vector2 actionViewport)
        {
            if (GetBool(mobileHud, "fireAimReticleUsesScreenCenter"))
            {
                return new Vector2(0.5f, 0.5f);
            }

            return hasActionViewport ? actionViewport : new Vector2(0.5f, 0.5f);
        }

        private static bool TryResolveTargetViewport(
            ActionCameraController cameraController,
            PlayerRangedBasicAttackAction action,
            CombatHealth targetHealth,
            out Vector2 targetViewport)
        {
            targetViewport = default;
            if (cameraController == null || targetHealth == null)
            {
                return false;
            }

            Vector3 targetPoint = targetHealth.transform.position + Vector3.up * GetFloat(action, "targetHeight");
            if (!cameraController.TryWorldToViewportPoint(targetPoint, out Vector3 viewportPoint))
            {
                return false;
            }

            targetViewport = new Vector2(viewportPoint.x, viewportPoint.y);
            return true;
        }

        private static void PlaceCloseThreatNearCenter(AimTimelineContext context)
        {
            Vector3 aimPlanarDirection = Vector3.ProjectOnPlane(context.CameraController.transform.forward, Vector3.up);
            if (aimPlanarDirection.sqrMagnitude <= 0.0001f)
            {
                aimPlanarDirection = context.CameraController.GetAimPlanarForward();
            }

            aimPlanarDirection.Normalize();
            Vector3 aimRight = Vector3.Cross(Vector3.up, aimPlanarDirection).normalized;
            Vector3 closeThreatPosition =
                context.Player.transform.position + aimPlanarDirection * 4.6f + aimRight * 1.1f;
            closeThreatPosition.y = context.CloseThreatRoot.transform.position.y;
            context.CloseThreatRoot.transform.SetPositionAndRotation(
                closeThreatPosition,
                Quaternion.LookRotation(-aimPlanarDirection, Vector3.up));
        }

        private static void MoveOutOfAimRay(
            GameObject root,
            SummonLaneSpace laneSpace,
            float lateralX,
            float worldY)
        {
            root.transform.position = laneSpace.GetBattlefieldWorldPoint(lateralX, laneSpace.BossProxyZ, worldY);
        }

        private static void CreateHighBossAimProxy(AimTimelineContext context)
        {
            Vector2 elevatedViewportPoint = new Vector2(0.5f, 0.95f);
            Assert.IsTrue(context.CameraController.TryGetViewportAimRay(elevatedViewportPoint, out Ray elevatedRay));
            float stableBodyY = context.BossRootCollider.bounds.center.y;
            Vector3 highHitPoint = elevatedRay.GetPoint(12f);
            if (highHitPoint.y <= stableBodyY + 0.7f)
            {
                highHitPoint.y = stableBodyY + 1.2f;
            }

            GameObject highHitProxy = new GameObject("FrontlineAimTimelineHighHitProxy");
            highHitProxy.transform.SetParent(context.BossRoot.transform, worldPositionStays: true);
            highHitProxy.transform.position = highHitPoint;
            BoxCollider highHitCollider = highHitProxy.AddComponent<BoxCollider>();
            highHitCollider.size = new Vector3(2.4f, 2.4f, 2.4f);
        }

        private static void WriteReports(List<AimSample> samples)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CsvPath));
            File.WriteAllText(CsvPath, BuildCsv(samples), Encoding.UTF8);
            File.WriteAllText(ReportPath, BuildMarkdown(samples), Encoding.UTF8);
        }

        private static string BuildCsv(List<AimSample> samples)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "scenario,step,frame,time,aim_input_x,aim_input_y,input_vp_x,input_vp_y,hud_vp_x,hud_vp_y,action_vp_x,action_vp_y,assist_vp_x,assist_vp_y,target_vp_x,target_vp_y,world_x,world_y,world_z,dir_x,dir_y,dir_z,raw_dir_x,raw_dir_y,raw_dir_z,assist_dir_x,assist_dir_y,assist_dir_z,has_action_vp,has_assist_vp,has_target_vp,has_world,has_direction,has_aim_assist_target,aim_assist_strength,target_name");
            for (int i = 0; i < samples.Count; i++)
            {
                AimSample sample = samples[i];
                builder
                    .Append(CsvEscape(sample.Scenario)).Append(',')
                    .Append(sample.Step).Append(',')
                    .Append(sample.Frame).Append(',')
                    .Append(Format(sample.TimeSeconds)).Append(',')
                    .Append(Format(sample.AimInput.x)).Append(',')
                    .Append(Format(sample.AimInput.y)).Append(',')
                    .Append(Format(sample.InputViewport.x)).Append(',')
                    .Append(Format(sample.InputViewport.y)).Append(',')
                    .Append(Format(sample.HudViewport.x)).Append(',')
                    .Append(Format(sample.HudViewport.y)).Append(',')
                    .Append(Format(sample.ActionViewport.x)).Append(',')
                    .Append(Format(sample.ActionViewport.y)).Append(',')
                    .Append(Format(sample.AssistViewport.x)).Append(',')
                    .Append(Format(sample.AssistViewport.y)).Append(',')
                    .Append(Format(sample.TargetViewport.x)).Append(',')
                    .Append(Format(sample.TargetViewport.y)).Append(',')
                    .Append(Format(sample.WorldPoint.x)).Append(',')
                    .Append(Format(sample.WorldPoint.y)).Append(',')
                    .Append(Format(sample.WorldPoint.z)).Append(',')
                    .Append(Format(sample.Direction.x)).Append(',')
                    .Append(Format(sample.Direction.y)).Append(',')
                    .Append(Format(sample.Direction.z)).Append(',')
                    .Append(Format(sample.RawDirection.x)).Append(',')
                    .Append(Format(sample.RawDirection.y)).Append(',')
                    .Append(Format(sample.RawDirection.z)).Append(',')
                    .Append(Format(sample.AssistDirection.x)).Append(',')
                    .Append(Format(sample.AssistDirection.y)).Append(',')
                    .Append(Format(sample.AssistDirection.z)).Append(',')
                    .Append(sample.HasActionViewport).Append(',')
                    .Append(sample.HasAssistViewport).Append(',')
                    .Append(sample.HasTargetViewport).Append(',')
                    .Append(sample.HasWorldPoint).Append(',')
                    .Append(sample.HasDirection).Append(',')
                    .Append(sample.HasAimAssistTarget).Append(',')
                    .Append(Format(sample.AimAssistStrength01)).Append(',')
                    .Append(CsvEscape(sample.TargetName))
                    .AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildMarkdown(List<AimSample> samples)
        {
            List<AimScenarioSummary> summaries = BuildSummaries(samples);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Frontline Aim Timeline Report");
            builder.AppendLine();
            builder.AppendLine($"Generated at frame {Time.frameCount}, Unity time {Format(Time.time)}.");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("| Scenario | Samples | HUD max step | Action VP max step | Assist VP max step | World Y range | Max world Y step | Direction max deg | Target changes | Notes |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int i = 0; i < summaries.Count; i++)
            {
                AimScenarioSummary summary = summaries[i];
                builder.Append("| ")
                    .Append(summary.Scenario).Append(" | ")
                    .Append(summary.SampleCount).Append(" | ")
                    .Append(Format(summary.MaxHudViewportStep)).Append(" | ")
                    .Append(Format(summary.MaxActionViewportStep)).Append(" | ")
                    .Append(Format(summary.MaxAssistViewportStep)).Append(" | ")
                    .Append(Format(summary.MinWorldY)).Append("..").Append(Format(summary.MaxWorldY)).Append(" | ")
                    .Append(Format(summary.MaxWorldYStep)).Append(" | ")
                    .Append(Format(summary.MaxDirectionAngleStep)).Append(" | ")
                    .Append(summary.TargetChanges).Append(" | ")
                    .Append(summary.Notes).AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Largest Action Viewport Jumps");
            builder.AppendLine();
            builder.AppendLine("| Scenario | From step | To step | From action VP | To action VP | Action step | World Y step | Target before | Target after |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|---|");
            List<AimJump> jumps = BuildLargestJumps(samples, 10);
            for (int i = 0; i < jumps.Count; i++)
            {
                AimJump jump = jumps[i];
                builder.Append("| ")
                    .Append(jump.Scenario).Append(" | ")
                    .Append(jump.FromStep).Append(" | ")
                    .Append(jump.ToStep).Append(" | ")
                    .Append(FormatVector(jump.FromActionViewport)).Append(" | ")
                    .Append(FormatVector(jump.ToActionViewport)).Append(" | ")
                    .Append(Format(jump.ActionViewportStep)).Append(" | ")
                    .Append(Format(jump.WorldYStep)).Append(" | ")
                    .Append(string.IsNullOrEmpty(jump.FromTargetName) ? "(none)" : jump.FromTargetName).Append(" | ")
                    .Append(string.IsNullOrEmpty(jump.ToTargetName) ? "(none)" : jump.ToTargetName).AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Interpretation Guide");
            builder.AppendLine();
            builder.AppendLine("- HUD max step near 0 while Action VP or World Y moves means the centered HUD reticle is stable, but another preview/line/VFX is still following projectile aim.");
            builder.AppendLine("- BossDirect or BossHighCollider world Y above roughly 4 means the preview endpoint is escaping to a far raycast hit instead of staying near the selected target.");
            builder.AppendLine("- Repeated BossAimSweep movement is expected from manual viewport input; compare it against BossDirect to isolate unintended idle wobble.");
            return builder.ToString();
        }

        private static List<AimScenarioSummary> BuildSummaries(List<AimSample> samples)
        {
            Dictionary<string, List<AimSample>> groupedSamples = new Dictionary<string, List<AimSample>>();
            for (int i = 0; i < samples.Count; i++)
            {
                AimSample sample = samples[i];
                if (!groupedSamples.TryGetValue(sample.Scenario, out List<AimSample> scenarioSamples))
                {
                    scenarioSamples = new List<AimSample>();
                    groupedSamples.Add(sample.Scenario, scenarioSamples);
                }

                scenarioSamples.Add(sample);
            }

            List<AimScenarioSummary> summaries = new List<AimScenarioSummary>();
            foreach (KeyValuePair<string, List<AimSample>> pair in groupedSamples)
            {
                summaries.Add(BuildSummary(pair.Key, pair.Value));
            }

            return summaries;
        }

        private static AimScenarioSummary RequireSummary(
            List<AimScenarioSummary> summaries,
            AimTimelineScenario scenario)
        {
            string scenarioName = scenario.ToString();
            for (int i = 0; i < summaries.Count; i++)
            {
                if (string.Equals(summaries[i].Scenario, scenarioName, StringComparison.Ordinal))
                {
                    return summaries[i];
                }
            }

            Assert.Fail($"Missing aim timeline summary for {scenarioName}.");
            return null;
        }

        private static AimScenarioSummary BuildSummary(string scenario, List<AimSample> samples)
        {
            AimScenarioSummary summary = new AimScenarioSummary
            {
                Scenario = scenario,
                SampleCount = samples.Count,
                MinWorldY = float.PositiveInfinity,
                MaxWorldY = float.NegativeInfinity
            };

            for (int i = 0; i < samples.Count; i++)
            {
                AimSample sample = samples[i];
                if (sample.HasWorldPoint)
                {
                    summary.MinWorldY = Mathf.Min(summary.MinWorldY, sample.WorldPoint.y);
                    summary.MaxWorldY = Mathf.Max(summary.MaxWorldY, sample.WorldPoint.y);
                }

                if (i <= 0)
                {
                    continue;
                }

                AimSample previous = samples[i - 1];
                summary.MaxHudViewportStep = Mathf.Max(
                    summary.MaxHudViewportStep,
                    Vector2.Distance(previous.HudViewport, sample.HudViewport));
                if (previous.HasActionViewport && sample.HasActionViewport)
                {
                    summary.MaxActionViewportStep = Mathf.Max(
                        summary.MaxActionViewportStep,
                        Vector2.Distance(previous.ActionViewport, sample.ActionViewport));
                }

                if (previous.HasAssistViewport && sample.HasAssistViewport)
                {
                    summary.MaxAssistViewportStep = Mathf.Max(
                        summary.MaxAssistViewportStep,
                        Vector2.Distance(previous.AssistViewport, sample.AssistViewport));
                }

                if (previous.HasWorldPoint && sample.HasWorldPoint)
                {
                    summary.MaxWorldYStep = Mathf.Max(
                        summary.MaxWorldYStep,
                        Mathf.Abs(sample.WorldPoint.y - previous.WorldPoint.y));
                }

                if (previous.HasDirection && sample.HasDirection)
                {
                    summary.MaxDirectionAngleStep = Mathf.Max(
                        summary.MaxDirectionAngleStep,
                        Vector3.Angle(previous.Direction, sample.Direction));
                }

                if (!string.Equals(previous.TargetName, sample.TargetName, StringComparison.Ordinal))
                {
                    summary.TargetChanges++;
                }
            }

            if (float.IsPositiveInfinity(summary.MinWorldY))
            {
                summary.MinWorldY = 0f;
                summary.MaxWorldY = 0f;
            }

            summary.Notes = ResolveSummaryNotes(summary);
            return summary;
        }

        private static string ResolveSummaryNotes(AimScenarioSummary summary)
        {
            if (summary.MaxHudViewportStep <= 0.001f && summary.MaxActionViewportStep > 0.03f)
            {
                return "HUD stable; action preview moves.";
            }

            if (summary.MaxWorldYStep > 0.5f)
            {
                return "Large vertical world jump.";
            }

            if (summary.TargetChanges > 0)
            {
                return "Target changed during sampling.";
            }

            return "Stable within sample window.";
        }

        private static List<AimJump> BuildLargestJumps(List<AimSample> samples, int limit)
        {
            List<AimJump> jumps = new List<AimJump>();
            for (int i = 1; i < samples.Count; i++)
            {
                AimSample previous = samples[i - 1];
                AimSample current = samples[i];
                if (!string.Equals(previous.Scenario, current.Scenario, StringComparison.Ordinal)
                    || !previous.HasActionViewport
                    || !current.HasActionViewport)
                {
                    continue;
                }

                jumps.Add(new AimJump
                {
                    Scenario = current.Scenario,
                    FromStep = previous.Step,
                    ToStep = current.Step,
                    FromActionViewport = previous.ActionViewport,
                    ToActionViewport = current.ActionViewport,
                    ActionViewportStep = Vector2.Distance(previous.ActionViewport, current.ActionViewport),
                    WorldYStep = previous.HasWorldPoint && current.HasWorldPoint
                        ? Mathf.Abs(current.WorldPoint.y - previous.WorldPoint.y)
                        : 0f,
                    FromTargetName = previous.TargetName,
                    ToTargetName = current.TargetName
                });
            }

            jumps.Sort((left, right) => right.ActionViewportStep.CompareTo(left.ActionViewportStep));
            if (jumps.Count > limit)
            {
                jumps.RemoveRange(limit, jumps.Count - limit);
            }

            return jumps;
        }

        private static void SetTargetCandidates(PlayerCombatTargetSelector targetSelector, params CombatHealth[] candidates)
        {
            SetField(targetSelector, "targetCandidates", candidates);
            ClearTargetSelector(targetSelector);
            targetSelector.RefreshTarget();
        }

        private static void ClearTargetSelector(PlayerCombatTargetSelector targetSelector)
        {
            SetField(targetSelector, "currentTargetHealth", null);
            SetField(targetSelector, "currentTarget", null);
            SetField(targetSelector, "nextRetargetTime", 0f);
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

        private static void SetField(UnityEngine.Object target, string fieldName, object value)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            field.SetValue(target, value);
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

        private static string Format(float value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static string FormatVector(Vector2 value)
        {
            return $"{Format(value.x)}/{Format(value.y)}";
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        private sealed class AimTimelineContext
        {
            public AimTimelineContext(
                PlayerMovementController player,
                PlayerCombatModeController combatModeController,
                PlayerRangedAimController aimController,
                PlayerRangedBasicAttackAction rangedBasicAttackAction,
                PlayerCombatTargetSelector targetSelector,
                ActionCameraController cameraController,
                BossBarrageLaneReviewMobileHud mobileHud,
                SummonLaneSpace laneSpace,
                GameObject bossRoot,
                CombatHealth bossHealth,
                Collider bossRootCollider,
                GameObject closeThreatRoot,
                CombatHealth closeThreatHealth)
            {
                Player = player;
                CombatModeController = combatModeController;
                AimController = aimController;
                RangedBasicAttackAction = rangedBasicAttackAction;
                TargetSelector = targetSelector;
                CameraController = cameraController;
                MobileHud = mobileHud;
                LaneSpace = laneSpace;
                BossRoot = bossRoot;
                BossHealth = bossHealth;
                BossRootCollider = bossRootCollider;
                CloseThreatRoot = closeThreatRoot;
                CloseThreatHealth = closeThreatHealth;
            }

            public PlayerMovementController Player { get; }
            public PlayerCombatModeController CombatModeController { get; }
            public PlayerRangedAimController AimController { get; }
            public PlayerRangedBasicAttackAction RangedBasicAttackAction { get; }
            public PlayerCombatTargetSelector TargetSelector { get; }
            public ActionCameraController CameraController { get; }
            public BossBarrageLaneReviewMobileHud MobileHud { get; }
            public SummonLaneSpace LaneSpace { get; }
            public GameObject BossRoot { get; }
            public CombatHealth BossHealth { get; }
            public Collider BossRootCollider { get; }
            public GameObject CloseThreatRoot { get; }
            public CombatHealth CloseThreatHealth { get; }
        }

        private sealed class AimSample
        {
            public string Scenario { get; set; }
            public int Step { get; set; }
            public int Frame { get; set; }
            public float TimeSeconds { get; set; }
            public Vector2 AimInput { get; set; }
            public Vector2 InputViewport { get; set; }
            public bool HasHudViewport { get; set; }
            public Vector2 HudViewport { get; set; }
            public bool HasActionViewport { get; set; }
            public Vector2 ActionViewport { get; set; }
            public bool HasAssistViewport { get; set; }
            public Vector2 AssistViewport { get; set; }
            public bool HasTargetViewport { get; set; }
            public Vector2 TargetViewport { get; set; }
            public bool HasWorldPoint { get; set; }
            public Vector3 WorldPoint { get; set; }
            public bool HasDirection { get; set; }
            public Vector3 Direction { get; set; }
            public Vector3 RawDirection { get; set; }
            public Vector3 AssistDirection { get; set; }
            public bool HasAimAssistTarget { get; set; }
            public float AimAssistStrength01 { get; set; }
            public string TargetName { get; set; }
        }

        private sealed class AimScenarioSummary
        {
            public string Scenario { get; set; }
            public int SampleCount { get; set; }
            public float MaxHudViewportStep { get; set; }
            public float MaxActionViewportStep { get; set; }
            public float MaxAssistViewportStep { get; set; }
            public float MinWorldY { get; set; }
            public float MaxWorldY { get; set; }
            public float MaxWorldYStep { get; set; }
            public float MaxDirectionAngleStep { get; set; }
            public int TargetChanges { get; set; }
            public string Notes { get; set; }
        }

        private sealed class AimJump
        {
            public string Scenario { get; set; }
            public int FromStep { get; set; }
            public int ToStep { get; set; }
            public Vector2 FromActionViewport { get; set; }
            public Vector2 ToActionViewport { get; set; }
            public float ActionViewportStep { get; set; }
            public float WorldYStep { get; set; }
            public string FromTargetName { get; set; }
            public string ToTargetName { get; set; }
        }
    }
}
