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
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string ReportPath = "C:/tmp/DimensionBrawl-FrontlineCombatPolicyReport.md";
        private const string JsonPath = "C:/tmp/DimensionBrawl-FrontlineCombatPolicyReport.json";

        private enum PolicyKind
        {
            NoSummonNoFire,
            GunOnly,
            IntendedRoute,
            LateSummon,
            MissedFollowupCounterRecovery,
            BossScreenBlockedFollowup
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
                    PolicyKind.IntendedRoute,
                    PolicyKind.LateSummon,
                    PolicyKind.MissedFollowupCounterRecovery,
                    PolicyKind.BossScreenBlockedFollowup
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
                PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
                PolicyMetrics counterRecovery = RequireResult(results, PolicyKind.MissedFollowupCounterRecovery);
                PolicyMetrics blockedFollowup = RequireResult(results, PolicyKind.BossScreenBlockedFollowup);
                Assert.IsTrue(File.Exists(ReportPath), "Frontline combat policy report should be written.");
                Assert.IsTrue(File.Exists(JsonPath), "Frontline combat policy JSON should be written.");
                Assert.Greater(intended.SummonBlocks, 0, "The intended route must prove summon interception changes the run.");
                Assert.Greater(
                    noSummon.PlayerDamageTaken,
                    intended.PlayerDamageTaken,
                    "The report should distinguish unanswered boss pressure from the intended summon answer.");
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
                case PolicyKind.IntendedRoute:
                    yield return RunIntendedRoute(context);
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

        private static IEnumerator RunIntendedRoute(CombatPolicyContext context)
        {
            yield return DefeatCloseThreatWithBasicFire(context);
            yield return ChargeEnergyToTier(context, 1, 14f);
            yield return UseSummonAndBlockNextBossWave(context);
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

            context.Metrics.SummonUses++;
            yield return Advance(context, 0.2f);
            yield return ApplyBossWave(context, BossWaveAnswer.SummonScreen);
            context.PocketOwner.Tick(0f);
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

            context.Metrics.SkillUses++;
            yield return null;
            LaneActionProjectile[] projectiles = FindActivePlayerProjectiles();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].TryApplyImpact(context.BossCollider, projectiles[i].transform.position))
                {
                    context.Metrics.SkillProjectileHits++;
                }
            }

            context.PocketOwner.Tick(0f);
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

            context.Metrics.SkillUses++;
            context.PocketOwner.Tick(0f);
            yield return null;

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
            builder.AppendLine("- Stage/wave/pressure: each policy is one route through the same Frontline stage shell, so unanswered pressure, direct fire, intended summon, and late summon can be compared without changing the scene.");
            builder.AppendLine("- Trigger -> target -> effect -> status/presentation: follow-up windows, Skill1 hit confirms, boss-screen blocks, counter-wave observation, ally hold, and result records are emitted as measured route evidence.");
            builder.AppendLine("- QTE/state lock-unlock: summon block opens the follow-up window; missed or blocked follow-up can lock the route into counter pressure; ally hold can unlock the final recovery window.");
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
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics late = RequireResult(results, PolicyKind.LateSummon);
            PolicyMetrics counterRecovery = RequireResult(results, PolicyKind.MissedFollowupCounterRecovery);
            PolicyMetrics blockedFollowup = RequireResult(results, PolicyKind.BossScreenBlockedFollowup);
            builder.AppendLine($"- Intended route prevented {Mathf.Max(0f, noSummon.PlayerDamageTaken - intended.PlayerDamageTaken):0.0} player damage versus no-action pressure.");
            builder.AppendLine($"- Gun-only dealt {gunOnly.BossDamageTaken:0.0} boss damage but ended as `{gunOnly.ResultKind}` because the route contract still needs summon pressure blocking.");
            builder.AppendLine($"- Skill1 punish split: gun-only boss damage {gunOnly.BossDamageTaken:0.0}, intended follow-up boss damage {intended.BossDamageTaken:0.0}.");
            builder.AppendLine($"- Late summon ended as `{late.ResultKind}` with {late.PlayerDamageTaken:0.0} damage taken, so the report can compare timing quality without changing the scene.");
            builder.AppendLine($"- Intended route currently reads as `{ResolveRouteShape(intended)}`: follow-up window {FormatSeconds(intended.FirstFollowupWindowAtSeconds)}, counter {FormatSeconds(intended.FirstCounterWaveAtSeconds)}, Skill1 hit {FormatSeconds(intended.FirstFollowupHitAtSeconds)}.");
            builder.AppendLine($"- Route stability split: no-action {FormatPercent01(noSummon.RouteStability01)} final / {FormatPercent01(noSummon.MinRouteStability01)} min, gun-only {FormatPercent01(gunOnly.RouteStability01)} / {FormatPercent01(gunOnly.MinRouteStability01)}, intended {FormatPercent01(intended.RouteStability01)} / {FormatPercent01(intended.MinRouteStability01)}.");
            builder.AppendLine($"- Unanswered hit penalty split: no-action {FormatPercent01(noSummon.TotalUnansweredBossHitRoutePenalty01)} x{noSummon.UnansweredBossHitRoutePenaltyCount}, gun-only {FormatPercent01(gunOnly.TotalUnansweredBossHitRoutePenalty01)} x{gunOnly.UnansweredBossHitRoutePenaltyCount}, late {FormatPercent01(late.TotalUnansweredBossHitRoutePenalty01)} x{late.UnansweredBossHitRoutePenaltyCount}.");
            builder.AppendLine($"- Frontline exposure split: no-action enemy-only {FormatSeconds(noSummon.EnemyOnlyFrontlineSeconds)}, gun-only enemy-only {FormatSeconds(gunOnly.EnemyOnlyFrontlineSeconds)}, intended ally-only {FormatSeconds(intended.AllyOnlyFrontlineSeconds)} / contested {FormatSeconds(intended.ContestedFrontlineSeconds)}.");
            builder.AppendLine($"- Missed follow-up branch: `{counterRecovery.ResultKind}` with counter source `{counterRecovery.CounterWaveSource}`, final window `{counterRecovery.CounterWaveFinalWindowState}`, and Skill1 hits {counterRecovery.SkillProjectileHits}.");
            builder.AppendLine($"- Boss-screen branch: boss releases {blockedFollowup.BossPressureSummonReleases}, blocks {blockedFollowup.BossPressureScreenBlocks}, Skill1 projectiles blocked {blockedFollowup.SkillProjectilesBlockedByBossScreen}, boss-blocked follow-up `{blockedFollowup.BossBlockedSkill1Followup}`.");
            int maxEnemyFrontlines = ResolveMaxEnemyFrontlines(results);
            builder.AppendLine(maxEnemyFrontlines > 0
                ? $"- Enemy frontline pressure is measured: max enemy frontlines {maxEnemyFrontlines}, enemy-only exposure, and hit penalty now separate unanswered pressure from clean summon cover."
                : "- Enemy frontline max count stayed 0; enemy summon contact is not yet a real measured pressure source in this slice.");
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

        private static float ResolveAverage(float total, float elapsedSeconds)
        {
            return elapsedSeconds > 0f ? total / elapsedSeconds : 0f;
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
                builder.AppendLine($"      \"bossDamageTaken\": {result.BossDamageTaken:0.###},");
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
                Metrics.CounterWaveEntryPenalty01 = PocketOwner.LastCounterWaveEntryPenalty;
                Metrics.CounterWaveStabilityBonus01 = PocketOwner.LastCounterWaveStabilityBonus;
                Metrics.CounterWaveFinalWindowSeconds = PocketOwner.LastCounterWaveFinalWindowDuration;
                Metrics.CounterWaveFinalWindowRouteScale = PocketOwner.LastCounterWaveFinalWindowRouteScale;
                Metrics.CounterWaveAllyHoldRequiredSeconds = PocketOwner.CounterWaveAllyHoldRequiredSeconds;
                Metrics.CounterWaveAllyHoldElapsedSeconds = PocketOwner.CounterWaveAllyHoldElapsedSeconds;
                Metrics.CounterWaveAllyHoldProgress01 = PocketOwner.CounterWaveAllyHoldProgress01;
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

                if (deltaTime <= 0f)
                {
                    return;
                }

                Metrics.RouteDrainAccumulated01 += PocketOwner.CurrentRouteStabilityDrainPerSecond * deltaTime;
                Metrics.RoutePressureWeightSeconds += PocketOwner.CurrentRoutePressureWeight * deltaTime;
                Metrics.FrontlinePresenceScaleSeconds += PocketOwner.CurrentFrontlinePresenceDrainScale * deltaTime;
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

            public void Complete()
            {
                Sample();
                Metrics.PlayerHealthRemaining = PlayerHealth.CurrentHealth;
                Metrics.BossHealthRemaining = BossHealth.CurrentHealth;
                Metrics.CloseThreatHealthRemaining = CloseThreatHealth.CurrentHealth;
                Metrics.ResultKind = PocketOwner.HasCommittedResultRecord
                    ? PocketOwner.LastResultRecord.ResultKind.ToString()
                    : PocketOwner.IsCleared
                        ? "ClearedNoRecord"
                        : PocketOwner.IsFailed
                            ? PocketOwner.FailureReason.ToString()
                            : "Running";

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
            }

            private void OnBossDamaged(DamageInfo damageInfo)
            {
                Metrics.BossDamageTaken += damageInfo.Amount;
            }

            private void OnCloseThreatDamaged(DamageInfo damageInfo)
            {
                Metrics.CloseThreatDamageTaken += damageInfo.Amount;
            }

            private void OnSummonPressureBlocked(int tier)
            {
                Metrics.SummonBlocks++;
                Metrics.HighestSummonBlockTier = Mathf.Max(Metrics.HighestSummonBlockTier, tier);
            }

            private void OnBossPressureSummonReleased(BossSummonPressureAction action, int tier)
            {
                Metrics.BossPressureSummonReleases++;
                Metrics.HighestBossPressureSummonTier = Mathf.Max(
                    Metrics.HighestBossPressureSummonTier,
                    tier);
            }

            private void OnBossPressureSummonIntercepted(BossSummonPressureAction action, int tier)
            {
                Metrics.BossPressureScreenBlocks++;
                Metrics.HighestBossPressureScreenBlockTier = Mathf.Max(
                    Metrics.HighestBossPressureScreenBlockTier,
                    tier);
            }

            private void OnSummonFollowupWindowOpened(int tier)
            {
                Metrics.FollowupWindowOpenCount++;
                Metrics.HighestFollowupWindowTier = Mathf.Max(Metrics.HighestFollowupWindowTier, tier);
                if (Metrics.FirstFollowupWindowAtSeconds < 0f)
                {
                    Metrics.FirstFollowupWindowAtSeconds = Metrics.ElapsedSeconds;
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
            public float ElapsedSeconds { get; set; }
            public float PlayerHealthStart { get; set; }
            public float PlayerHealthRemaining { get; set; }
            public float PlayerDamageTaken { get; set; }
            public float BossHealthStart { get; set; }
            public float BossHealthRemaining { get; set; }
            public float BossDamageTaken { get; set; }
            public float CloseThreatHealthStart { get; set; }
            public float CloseThreatHealthRemaining { get; set; }
            public float CloseThreatDamageTaken { get; set; }
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
        }
    }
}
