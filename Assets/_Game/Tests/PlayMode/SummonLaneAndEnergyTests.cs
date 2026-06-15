using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class SummonLaneAndEnergyTests
    {
        [Test]
        public void LaneSpaceClampsPlayerZoneButKeepsForwardBattlefieldAvailable()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();

            Vector3 clamped = lane.ClampPlayerPosition(new Vector3(8f, 0f, 4f));

            Assert.AreEqual(5f, clamped.x, 0.001f, "Player lateral movement should clamp to the authored lane width.");
            Assert.AreEqual(0f, clamped.z, 0.001f, "Player movement should never cross the authored forward boundary.");

            Vector3 summonEntry = lane.GetLaneWorldPoint(0f, lane.SummonEntryZ);
            Assert.IsTrue(
                lane.IsPastForwardBoundary(summonEntry),
                "Summon entry/frontline coordinates must remain valid beyond the player forward boundary.");

            Vector3 offLaneSummonPoint = lane.GetBattlefieldWorldPoint(9f, lane.SummonEntryZ);
            Vector2 offLaneCoordinates = lane.GetLaneCoordinates(offLaneSummonPoint);
            Assert.AreEqual(
                9f,
                offLaneCoordinates.x,
                0.001f,
                "Summon/frontline battlefield coordinates must be able to cross authored lateral rails when a summon pattern needs it.");

            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void EnergyLadderGainsFasterNearForwardBoundaryAndResetsOnSpend()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
            energy.ConfigureReferences(lane, playerObject.transform);

            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            energy.Tick(1f);
            float backlineEnergy = energy.CurrentTierEnergy;

            energy.ResetLadder();
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            energy.Tick(1f);
            float forwardEnergy = energy.CurrentTierEnergy;

            Assert.Greater(
                forwardEnergy,
                backlineEnergy,
                "Forward-risk positioning should charge summon energy faster than the backline.");

            energy.Tick(20f);
            Assert.GreaterOrEqual(energy.AvailableTier, 1, "The EN ladder should expose at least LV1 after enough gain.");

            Assert.IsTrue(energy.TrySpend(out int spentTier));
            Assert.GreaterOrEqual(spentTier, 1);
            Assert.AreEqual(0, energy.AvailableTier, "Spending any available tier should reset availability.");
            Assert.AreEqual(1, energy.ChargingTier, "Spending should restart empty LV1 charging.");
            Assert.AreEqual(0f, energy.CurrentTierEnergy, 0.001f);

            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossBarragePatternTightensProjectileSpreadNearForwardBoundary()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();

            float backlineSpread = pattern.EvaluateHalfSpread(0f);
            float forwardSpread = pattern.EvaluateHalfSpread(1f);

            Assert.Less(
                forwardSpread,
                backlineSpread,
                "Forward-risk projectile gaps should be tighter than backline gaps.");
            Assert.AreEqual(-backlineSpread, pattern.GetLateralOffset(0, 3, 0f), 0.001f);
            Assert.AreEqual(0f, pattern.GetLateralOffset(1, 3, 0f), 0.001f);
            Assert.AreEqual(backlineSpread, pattern.GetLateralOffset(2, 3, 0f), 0.001f);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageSideClampLeavesReadableOppositeSideGap()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.SideClamp;
            serializedObject.FindProperty("sideClampDirection").floatValue = -1f;
            serializedObject.FindProperty("sideClampCrossReachRatio").floatValue = 0.25f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float firstOffset = pattern.GetLateralOffset(0, 5, 0f);
            float lastOffset = pattern.GetLateralOffset(4, 5, 0f);

            Assert.Less(firstOffset, 0f, "Left clamp should start pressure from the left side.");
            Assert.Greater(lastOffset, 0f, "Left clamp should reach across center to narrow the right-side safe gap.");
            Assert.Less(
                Mathf.Abs(lastOffset),
                Mathf.Abs(firstOffset),
                "Side clamp should leave the opposite-side gap larger than the pressured side width.");

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarragePunishNetCentersOnPlayerAndTightensForwardRisk()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.PunishNet;
            serializedObject.FindProperty("backlineHalfSpread").floatValue = 3.4f;
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 0.9f;
            serializedObject.FindProperty("punishNetInnerSpreadRatio").floatValue = 0.34f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float centerOffset = pattern.GetLateralOffset(0, 5, 0f);
            float innerLeftOffset = pattern.GetLateralOffset(1, 5, 0f);
            float innerRightOffset = pattern.GetLateralOffset(2, 5, 0f);
            float outerLeftBacklineOffset = pattern.GetLateralOffset(3, 5, 0f);
            float outerLeftForwardOffset = pattern.GetLateralOffset(3, 5, 1f);

            Assert.AreEqual(0f, centerOffset, 0.001f, "PunishNet should include a center-lock projectile.");
            Assert.Less(innerLeftOffset, 0f);
            Assert.Greater(innerRightOffset, 0f);
            Assert.Less(
                Mathf.Abs(innerLeftOffset),
                Mathf.Abs(outerLeftBacklineOffset),
                "PunishNet should place inner shots near center before the outer ring.");
            Assert.Less(
                Mathf.Abs(outerLeftForwardOffset),
                Mathf.Abs(outerLeftBacklineOffset),
                "Forward-risk PunishNet gaps should tighten compared with the backline.");

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageLinePressureCommitsToOneRailAndTightensDepthNearForwardRisk()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.LinePressure;
            serializedObject.FindProperty("backlineHalfSpread").floatValue = 4f;
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 3f;
            serializedObject.FindProperty("linePressureDirection").floatValue = 1f;
            serializedObject.FindProperty("linePressureCenterRatio").floatValue = 0.72f;
            serializedObject.FindProperty("linePressureHalfSpreadRatio").floatValue = 0.08f;
            serializedObject.FindProperty("backlineDepthSpread").floatValue = 2.2f;
            serializedObject.FindProperty("forwardDepthSpread").floatValue = 0.85f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float firstOffset = pattern.GetLateralOffset(0, 4, 0f);
            float lastOffset = pattern.GetLateralOffset(3, 4, 0f);
            float backlineDepth = pattern.GetTargetDepthOffset(3, 4, 0f);
            float forwardDepth = pattern.GetTargetDepthOffset(3, 4, 1f);

            Assert.Greater(firstOffset, 0f, "Right-side LinePressure should commit pressure to one rail.");
            Assert.Greater(lastOffset, 0f, "LinePressure scatter should stay on the committed rail.");
            Assert.Less(
                Mathf.Abs(lastOffset - firstOffset),
                pattern.EvaluateHalfSpread(0f),
                "LinePressure should read as a narrow lane instead of a full spread.");
            Assert.Greater(
                Mathf.Abs(backlineDepth),
                Mathf.Abs(forwardDepth),
                "Forward-risk LinePressure depth spacing should tighten compared with the backline.");

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageProjectileDamagesHostileTargetsOnly()
        {
            GameObject projectileObject = new GameObject("Projectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();

            GameObject targetObject = new GameObject("Target");
            SphereCollider targetCollider = targetObject.AddComponent<SphereCollider>();
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Player);

            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);
            Assert.IsTrue(projectile.TryApplyImpact(targetCollider, Vector3.zero));
            Assert.AreEqual(90f, targetHealth.CurrentHealth, 0.001f);

            GameObject neutralObject = new GameObject("Neutral");
            SphereCollider neutralCollider = neutralObject.AddComponent<SphereCollider>();
            CombatHealth neutralHealth = neutralObject.AddComponent<CombatHealth>();
            neutralHealth.ConfigureTeam(DamageTeam.Neutral);

            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);
            Assert.IsFalse(projectile.TryApplyImpact(neutralCollider, Vector3.zero));
            Assert.AreEqual(100f, neutralHealth.CurrentHealth, 0.001f);

            Object.DestroyImmediate(neutralObject);
            Object.DestroyImmediate(targetObject);
            Object.DestroyImmediate(projectileObject);
        }

        [Test]
        public void SummonPressureScreenInterceptsHostileBossProjectilesWithTierLimit()
        {
            GameObject screenObject = new GameObject("SummonPressureScreen");
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen screen = screenObject.AddComponent<SummonPressureScreen>();

            GameObject alliedProjectileObject = new GameObject("AlliedProjectile");
            alliedProjectileObject.AddComponent<SphereCollider>();
            alliedProjectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile alliedProjectile = alliedProjectileObject.AddComponent<BossBarrageProjectile>();
            alliedProjectile.Configure(null, DamageTeam.Player, 10f, Vector3.back, 0f, 1f, 0.3f);

            screen.Activate(DamageTeam.AllySummon, 1, 1.25f, 1f);
            Assert.IsFalse(
                screen.TryIntercept(alliedProjectile),
                "Summon pressure screens should ignore player-side projectiles.");
            Assert.IsTrue(alliedProjectile.IsActive);

            GameObject enemyProjectileObject = new GameObject("EnemyProjectile");
            enemyProjectileObject.AddComponent<SphereCollider>();
            enemyProjectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile enemyProjectile = enemyProjectileObject.AddComponent<BossBarrageProjectile>();
            enemyProjectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);

            Assert.IsTrue(screen.TryIntercept(enemyProjectile));
            Assert.IsFalse(enemyProjectile.IsActive);
            Assert.AreEqual(1, screen.InterceptedProjectiles);
            Assert.AreEqual(0, screen.RemainingIntercepts);
            Assert.IsFalse(screen.IsActive);

            GameObject secondEnemyProjectileObject = new GameObject("SecondEnemyProjectile");
            secondEnemyProjectileObject.AddComponent<SphereCollider>();
            secondEnemyProjectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile secondEnemyProjectile =
                secondEnemyProjectileObject.AddComponent<BossBarrageProjectile>();
            secondEnemyProjectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);

            Assert.IsFalse(
                screen.TryIntercept(secondEnemyProjectile),
                "A spent pressure screen should not keep deleting boss projectiles.");
            Assert.IsTrue(secondEnemyProjectile.IsActive);

            Object.DestroyImmediate(secondEnemyProjectileObject);
            Object.DestroyImmediate(enemyProjectileObject);
            Object.DestroyImmediate(alliedProjectileObject);
            Object.DestroyImmediate(screenObject);
        }

        [Test]
        public void BossBarrageEmitterFiresPooledProjectilesFromBossSide()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(pattern, projectilePrefab, pattern.ProjectilesPerWave);

            Assert.IsTrue(emitter.BeginWindup());
            int spawned = emitter.FirePendingWave();

            Assert.AreEqual(pattern.ProjectilesPerWave, spawned);
            Assert.AreEqual(pattern.ProjectilesPerWave, emitter.ActiveProjectileCount);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(pattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossBarrageEmitterAdvancesAuthoredPatternSequenceAfterWave()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile firstPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile secondPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(firstPattern, projectilePrefab, firstPattern.ProjectilesPerWave * 2);
            emitter.ConfigurePatternSequence(
                new[] { firstPattern, secondPattern },
                1);

            Assert.AreSame(firstPattern, emitter.CurrentPattern);
            Assert.AreEqual(0, emitter.CurrentPatternSequenceIndex);

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();

            Assert.AreSame(secondPattern, emitter.CurrentPattern);
            Assert.AreEqual(1, emitter.CurrentPatternSequenceIndex);

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();

            Assert.AreSame(firstPattern, emitter.CurrentPattern);
            Assert.AreEqual(0, emitter.CurrentPatternSequenceIndex);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(firstPattern);
            Object.DestroyImmediate(secondPattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }
    }
}
