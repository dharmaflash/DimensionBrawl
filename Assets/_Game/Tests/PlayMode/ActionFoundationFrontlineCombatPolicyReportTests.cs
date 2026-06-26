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
            LateSummon
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
                    PolicyKind.LateSummon
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
                Assert.IsTrue(File.Exists(ReportPath), "Frontline combat policy report should be written.");
                Assert.IsTrue(File.Exists(JsonPath), "Frontline combat policy JSON should be written.");
                Assert.Greater(intended.SummonBlocks, 0, "The intended route must prove summon interception changes the run.");
                Assert.Greater(
                    noSummon.PlayerDamageTaken,
                    intended.PlayerDamageTaken,
                    "The report should distinguish unanswered boss pressure from the intended summon answer.");
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
                context.Sample();
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
            builder.AppendLine("| Policy | Result | Sim s | Player HP | HP lost | Boss dmg | Close shots | Boss waves | Player hits | Summons | Blocks | Skill1 | Enemy fronts | Decision |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");
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
                builder.Append(result.PlayerHealthRemaining.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.PlayerDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.BossDamageTaken.ToString("0.0"));
                builder.Append(" | ");
                builder.Append(result.CloseThreatBasicHits);
                builder.Append(" | ");
                builder.Append(result.BossWaves);
                builder.Append(" | ");
                builder.Append(result.BossProjectilesHitPlayer);
                builder.Append(" | ");
                builder.Append(result.SummonUses);
                builder.Append(" | ");
                builder.Append(result.SummonBlocks);
                builder.Append(" | ");
                builder.Append(result.SkillUses);
                builder.Append(" | ");
                builder.Append(result.MaxEnemyFrontlineCount);
                builder.Append(" | ");
                builder.Append(EscapeTable(result.RouteDecision));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Read");
            PolicyMetrics intended = RequireResult(results, PolicyKind.IntendedRoute);
            PolicyMetrics noSummon = RequireResult(results, PolicyKind.NoSummonNoFire);
            PolicyMetrics gunOnly = RequireResult(results, PolicyKind.GunOnly);
            PolicyMetrics late = RequireResult(results, PolicyKind.LateSummon);
            builder.AppendLine($"- Intended route prevented {Mathf.Max(0f, noSummon.PlayerDamageTaken - intended.PlayerDamageTaken):0.0} player damage versus no-action pressure.");
            builder.AppendLine($"- Gun-only dealt {gunOnly.BossDamageTaken:0.0} boss damage but ended as `{gunOnly.ResultKind}` because the route contract still needs summon pressure blocking.");
            builder.AppendLine($"- Late summon ended as `{late.ResultKind}` with {late.PlayerDamageTaken:0.0} damage taken, so the report can compare timing quality without changing the scene.");
            builder.AppendLine($"- Enemy frontline max count across policies was {ResolveMaxEnemyFrontlines(results)}; if this stays 0, enemy summon contact is not yet a real measured pressure source in this slice.");
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
                builder.AppendLine($"      \"maxEnemyFrontlineCount\": {result.MaxEnemyFrontlineCount},");
                builder.AppendLine($"      \"routeDecision\": \"{JsonEscape(result.RouteDecision)}\",");
                builder.AppendLine($"      \"completionReadout\": \"{JsonEscape(result.CompletionReadout)}\"");
                builder.Append("    }");
                builder.AppendLine(i + 1 < results.Count ? "," : string.Empty);
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
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
                PocketOwner.CounterWaveObserved += OnCounterWaveObserved;
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
            public BossBarragePocketReviewOwner PocketOwner { get; }
            public CombatHealth PlayerHealth { get; }
            public CombatHealth BossHealth { get; }
            public CombatHealth CloseThreatHealth { get; }
            public Collider PlayerCollider { get; }
            public Collider BossCollider { get; }
            public Collider CloseThreatCollider { get; }
            public PolicyMetrics Metrics { get; }

            public void Sample()
            {
                Metrics.PlayerHealthRemaining = PlayerHealth.CurrentHealth;
                Metrics.BossHealthRemaining = BossHealth.CurrentHealth;
                Metrics.CloseThreatHealthRemaining = CloseThreatHealth.CurrentHealth;
                Metrics.RouteDecision = $"{PocketOwner.RouteDecisionState}({PocketOwner.RouteDecisionReadout})";
                Metrics.CompletionReadout = PocketOwner.CompletionRecordReadout;
                Metrics.RouteStability01 = PocketOwner.RouteStability01;
                Metrics.MaxEnemyFrontlineCount = Mathf.Max(
                    Metrics.MaxEnemyFrontlineCount,
                    PocketOwner.ActiveEnemyFrontlineProxyCount);
                Metrics.MaxAllyFrontlineCount = Mathf.Max(
                    Metrics.MaxAllyFrontlineCount,
                    PocketOwner.ActiveAllyFrontlineProxyCount);
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
                PocketOwner.CounterWaveObserved -= OnCounterWaveObserved;
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

            private void OnCounterWaveObserved(BossBarragePocketReviewOwner.CounterWaveSource source)
            {
                Metrics.CounterWaves++;
                Metrics.LastCounterWaveSource = source.ToString();
            }

            private void OnResultRecordCommitted(BossBarragePocketReviewOwner.RouteResultRecord record)
            {
                Metrics.ResultRecords++;
                Metrics.ResultKind = record.ResultKind.ToString();
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
            public int CounterWaves { get; set; }
            public int ResultRecords { get; set; }
            public int MaxEnemyFrontlineCount { get; set; }
            public int MaxAllyFrontlineCount { get; set; }
            public float RouteStability01 { get; set; }
            public string LastCounterWaveSource { get; set; } = "none";
            public string RouteDecision { get; set; } = "unknown";
            public string CompletionReadout { get; set; } = string.Empty;
            public List<string> Notes { get; } = new List<string>();
        }
    }
}
