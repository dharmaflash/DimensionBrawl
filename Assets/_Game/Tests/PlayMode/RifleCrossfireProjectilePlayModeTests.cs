using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class RifleCrossfireProjectilePlayModeTests
    {
        private const string RifleCrossfirePrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Ranged_RifleCrossfire.prefab";
        private const string RifleCrossfirePatternPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RifleCrossfire.asset";

        private readonly List<GameObject> ownedObjects = new();

        [UnityTearDown]
        public IEnumerator DestroyOwnedObjects()
        {
            for (int i = ownedObjects.Count - 1; i >= 0; i--)
            {
                if (ownedObjects[i] != null)
                {
                    Object.DestroyImmediate(ownedObjects[i]);
                }
            }

            ownedObjects.Clear();
            yield return null;
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator RifleCrossfireShotUsesHostileSourceAndIgnoresShooterMotion()
        {
            CombatAiPatternProfile pattern = LoadRequired<CombatAiPatternProfile>(
                RifleCrossfirePatternPath);
            CreateFixture(
                new Vector3(0f, 3f, 4.5f),
                out GameObject shooter,
                out BasicSoldierEnemy soldier,
                out CombatHealth sourceHealth,
                out CombatTargetSensor sensor,
                out BasicSoldierProjectileAttackDriver driver,
                out Transform projectileRoot,
                out CombatHealth playerHealth);
            yield return null;

            soldier.enabled = false;
            soldier.ConfigureTarget(playerHealth.transform, playerHealth);
            sensor.ConfigureTargetCandidates(new[] { playerHealth }, refreshNow: true);
            Assert.That(sensor.CurrentTargetHealth, Is.SameAs(playerHealth));

            int damageCount = 0;
            DamageInfo observedDamage = default;
            playerHealth.Damaged += HandlePlayerDamaged;
            InvokePatternState(driver, CombatAiPatternState.Tracking, pattern);
            InvokePatternState(driver, CombatAiPatternState.AttackActive, pattern);

            LaneActionProjectile projectile = driver.LastFiredProjectile;
            Assert.That(driver.FiredCount, Is.EqualTo(1));
            Assert.That(driver.OwnedProjectileCount, Is.EqualTo(1));
            Assert.That(driver.ActiveProjectileCount, Is.EqualTo(1));
            Assert.That(projectile, Is.Not.Null);
            Assert.That(projectile.IsActive, Is.True);
            Assert.That(projectile.SourceHealth, Is.SameAs(sourceHealth));
            Assert.That(projectile.SourceTeam, Is.EqualTo(DamageTeam.Enemy));
            Assert.That(projectile.Damage, Is.EqualTo(pattern.Damage).Within(0.001f));
            Assert.That(projectile.HitStopSeconds, Is.EqualTo(pattern.HitStopSeconds).Within(0.001f));
            Assert.That(projectile.ResponsePolicy, Is.EqualTo(pattern.DamageResponsePolicy));
            Assert.That(projectile.ControlLockPolicy, Is.EqualTo(pattern.ControlLockPolicy));
            Assert.That(projectile.transform.parent, Is.SameAs(projectileRoot));

            Vector3 positionBeforeShooterMotion = projectile.transform.position;
            shooter.transform.SetPositionAndRotation(
                new Vector3(20f, 0f, -3f),
                Quaternion.Euler(0f, 145f, 0f));
            Assert.That(
                projectile.transform.position,
                Is.EqualTo(positionBeforeShooterMotion),
                "A launched projectile must not inherit a later shooter transform change.");

            yield return null;
            Assert.That(
                Mathf.Abs(projectile.transform.position.x - positionBeforeShooterMotion.x),
                Is.LessThan(0.25f),
                "The projectile world lane shifted with the moving shooter.");

            float hitDeadline = Time.realtimeSinceStartup + 2f;
            while (damageCount == 0)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    hitDeadline,
                    "RifleCrossfire projectile did not reach the configured player target.");
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.15f);
            playerHealth.Damaged -= HandlePlayerDamaged;
            Assert.That(damageCount, Is.EqualTo(1));
            Assert.That(observedDamage.Source, Is.SameAs(sourceHealth));
            Assert.That(observedDamage.SourceTeam, Is.EqualTo(DamageTeam.Enemy));
            Assert.That(observedDamage.Amount, Is.EqualTo(pattern.Damage).Within(0.001f));
            Assert.That(observedDamage.HitStopSeconds, Is.EqualTo(pattern.HitStopSeconds).Within(0.001f));
            Assert.That(observedDamage.ResponsePolicy, Is.EqualTo(pattern.DamageResponsePolicy));
            Assert.That(observedDamage.ControlLockPolicy, Is.EqualTo(pattern.ControlLockPolicy));
            Assert.That(projectile.IsActive, Is.False);
            Assert.That(driver.ActiveProjectileCount, Is.Zero);
            Assert.That(driver.OwnedProjectileCount, Is.EqualTo(1));

            void HandlePlayerDamaged(DamageInfo damageInfo)
            {
                if (!ReferenceEquals(damageInfo.Source, sourceHealth))
                {
                    return;
                }

                observedDamage = damageInfo;
                damageCount++;
            }
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator RifleCrossfireWindupAndProjectileShareOneLockedWarningLane()
        {
            CreateFixture(
                new Vector3(0f, 3f, 4.5f),
                out _,
                out BasicSoldierEnemy soldier,
                out _,
                out CombatTargetSensor sensor,
                out BasicSoldierProjectileAttackDriver driver,
                out _,
                out CombatHealth playerHealth);
            yield return null;

            soldier.ConfigureTarget(playerHealth.transform, playerHealth);
            sensor.ConfigureTargetCandidates(new[] { playerHealth }, refreshNow: true);
            Assert.That(sensor.CurrentTargetHealth, Is.SameAs(playerHealth));

            float windupDeadline = Time.realtimeSinceStartup + 3f;
            while (soldier.CurrentPatternState != CombatAiPatternState.Windup)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    windupDeadline,
                    $"RifleCrossfire never entered its authored Windup; state={soldier.CurrentPatternState}.");
                yield return null;
            }

            Vector3 warnedDirection = Vector3.ProjectOnPlane(
                soldier.ResolvedAttackDirection,
                Vector3.up).normalized;
            Assert.That(warnedDirection.sqrMagnitude, Is.GreaterThan(0.99f));

            playerHealth.transform.position += Vector3.right * 4f;
            Physics.SyncTransforms();
            Vector3 retargetedDirection = Vector3.ProjectOnPlane(
                playerHealth.transform.position - driver.ProjectileOrigin.position,
                Vector3.up).normalized;
            Assert.That(
                Vector3.Dot(warnedDirection, retargetedDirection),
                Is.LessThan(0.95f),
                "The target move was too small to distinguish locked warning aim from snap aim.");
            Assert.That(
                Vector3.Dot(
                    warnedDirection,
                    Vector3.ProjectOnPlane(soldier.ResolvedAttackDirection, Vector3.up).normalized),
                Is.GreaterThan(0.999f),
                "The warned lane changed after the player moved during Windup.");

            float fireDeadline = Time.realtimeSinceStartup + 2f;
            while (driver.FiredCount == 0)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    fireDeadline,
                    "RifleCrossfire did not fire after its observed Windup.");
                yield return null;
            }

            LaneActionProjectile projectile = driver.LastFiredProjectile;
            Assert.That(projectile, Is.Not.Null);
            Assert.That(projectile.AllowsVerticalTravel, Is.True);
            Assert.That(projectile.TravelDirection.y, Is.GreaterThan(0.2f));
            Vector3 launchedDirection = Vector3.ProjectOnPlane(
                projectile.transform.forward,
                Vector3.up).normalized;
            Assert.That(
                Vector3.Dot(launchedDirection, warnedDirection),
                Is.GreaterThan(0.999f),
                "The physical projectile did not follow the lane shown during Windup.");
            Assert.That(
                Vector3.Dot(launchedDirection, retargetedDirection),
                Is.LessThan(0.95f),
                "The projectile snapped to the player's new position after the warning lane was locked.");
            projectile.Deactivate();
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator RifleCrossfireMissReuseDisableAndDeathReachSynchronousQuiescence()
        {
            CombatAiPatternProfile pattern = LoadRequired<CombatAiPatternProfile>(
                RifleCrossfirePatternPath);
            CreateFixture(
                new Vector3(100f, 0f, 100f),
                out _,
                out BasicSoldierEnemy soldier,
                out CombatHealth sourceHealth,
                out CombatTargetSensor sensor,
                out BasicSoldierProjectileAttackDriver driver,
                out Transform projectileRoot,
                out CombatHealth playerHealth);
            yield return null;

            soldier.enabled = false;
            soldier.ConfigureTarget(playerHealth.transform, playerHealth);
            sensor.ConfigureTargetCandidates(new[] { playerHealth }, refreshNow: true);

            InvokePatternState(driver, CombatAiPatternState.Tracking, pattern);
            InvokePatternState(driver, CombatAiPatternState.AttackActive, pattern);
            LaneActionProjectile pooledProjectile = driver.LastFiredProjectile;
            Assert.That(pooledProjectile, Is.Not.Null);
            Assert.That(pooledProjectile.IsActive, Is.True);

            float expiryDeadline = Time.realtimeSinceStartup + 2f;
            while (pooledProjectile.IsActive)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    expiryDeadline,
                    "A clean RifleCrossfire miss did not expire at its configured lifetime.");
                yield return null;
            }

            Assert.That(driver.ActiveProjectileCount, Is.Zero);
            Assert.That(driver.OwnedProjectileCount, Is.EqualTo(1));
            Assert.That(pooledProjectile.gameObject.activeSelf, Is.False);

            for (int shot = 0; shot < 8; shot++)
            {
                InvokePatternState(driver, CombatAiPatternState.Tracking, pattern);
                InvokePatternState(driver, CombatAiPatternState.AttackActive, pattern);
                Assert.That(driver.LastFiredProjectile, Is.SameAs(pooledProjectile));
                Assert.That(driver.ActiveProjectileCount, Is.EqualTo(1));
                Assert.That(driver.OwnedProjectileCount, Is.EqualTo(1));
                Assert.That(driver.OwnedProjectileCount, Is.LessThanOrEqualTo(driver.MaxOwnedProjectileCount));
                pooledProjectile.Deactivate();
            }

            InvokePatternState(driver, CombatAiPatternState.Tracking, pattern);
            InvokePatternState(driver, CombatAiPatternState.AttackActive, pattern);
            Assert.That(driver.ActiveProjectileCount, Is.EqualTo(1));
            driver.enabled = false;
            Assert.That(driver.ActiveProjectileCount, Is.Zero);
            Assert.That(pooledProjectile.gameObject.activeSelf, Is.False);
            Assert.That(pooledProjectile.transform.parent, Is.SameAs(projectileRoot));

            driver.enabled = true;
            InvokePatternState(driver, CombatAiPatternState.Tracking, pattern);
            InvokePatternState(driver, CombatAiPatternState.AttackActive, pattern);
            Assert.That(driver.ActiveProjectileCount, Is.EqualTo(1));
            Assert.That(
                sourceHealth.TryApplyDamage(
                    new DamageInfo(
                        null,
                        DamageTeam.Player,
                        sourceHealth.MaxHealth + 1f,
                        sourceHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)),
                Is.True);
            Assert.That(sourceHealth.IsAlive, Is.False);
            Assert.That(driver.ActiveProjectileCount, Is.Zero);
            Assert.That(pooledProjectile.gameObject.activeSelf, Is.False);
            Assert.That(pooledProjectile.transform.parent, Is.SameAs(projectileRoot));
            Assert.That(driver.OwnedProjectileCount, Is.EqualTo(1));
        }

        private void CreateFixture(
            Vector3 playerPosition,
            out GameObject shooter,
            out BasicSoldierEnemy soldier,
            out CombatHealth sourceHealth,
            out CombatTargetSensor sensor,
            out BasicSoldierProjectileAttackDriver driver,
            out Transform projectileRoot,
            out CombatHealth playerHealth)
        {
            GameObject owner = Own(new GameObject("RifleCrossfireCircuitOwner"));
            GameObject projectileRootObject = new GameObject("Projectiles");
            projectileRootObject.transform.SetParent(owner.transform, worldPositionStays: false);
            projectileRoot = projectileRootObject.transform;

            GameObject prefab = LoadRequired<GameObject>(RifleCrossfirePrefabPath);
            shooter = Object.Instantiate(prefab, owner.transform);
            shooter.name = "RifleCrossfireCircuitShooter";
            shooter.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            soldier = shooter.GetComponent<BasicSoldierEnemy>();
            sourceHealth = shooter.GetComponent<CombatHealth>();
            sensor = shooter.GetComponent<CombatTargetSensor>();
            driver = shooter.GetComponent<BasicSoldierProjectileAttackDriver>();
            Assert.That(soldier, Is.Not.Null);
            Assert.That(sourceHealth, Is.Not.Null);
            Assert.That(sourceHealth.Team, Is.EqualTo(DamageTeam.Enemy));
            Assert.That(sensor, Is.Not.Null);
            Assert.That(driver, Is.Not.Null);
            driver.ConfigureRuntimeProjectileRoot(projectileRoot);
            Assert.That(driver.HasIndependentRuntimeProjectileRoot, Is.True);

            GameObject player = Own(new GameObject("RifleCrossfireCircuitPlayer"));
            player.SetActive(false);
            player.transform.position = playerPosition;
            CapsuleCollider playerCollider = player.AddComponent<CapsuleCollider>();
            playerCollider.center = new Vector3(0f, 0.9f, 0f);
            playerCollider.height = 1.8f;
            playerCollider.radius = 0.42f;
            playerHealth = player.AddComponent<CombatHealth>();
            var serializedHealth = new SerializedObject(playerHealth);
            serializedHealth.FindProperty("team").enumValueIndex = (int)DamageTeam.Player;
            serializedHealth.FindProperty("maxHealth").floatValue = 250f;
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            player.SetActive(true);
            playerHealth.ResetHealthToFull();
        }

        private GameObject Own(GameObject gameObject)
        {
            ownedObjects.Add(gameObject);
            return gameObject;
        }

        private static T LoadRequired<T>(string assetPath) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.That(asset, Is.Not.Null, $"Missing required asset at {assetPath}.");
            return asset;
        }

        private static void InvokePatternState(
            BasicSoldierProjectileAttackDriver driver,
            CombatAiPatternState state,
            CombatAiPatternProfile pattern)
        {
            MethodInfo method = typeof(BasicSoldierProjectileAttackDriver).GetMethod(
                "HandlePatternStateChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(driver, new object[] { state, pattern });
        }
    }
}
