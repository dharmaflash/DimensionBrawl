using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class BossBarrageEmitterActiveProjectilePlayModeTests
    {
        [Test]
        public void CopyActiveProjectilesIncludesStandbyPoolAndExcludesDeactivatedProjectiles()
        {
            GameObject laneObject = null;
            GameObject playerObject = null;
            GameObject bossObject = null;
            GameObject firstPrefabObject = null;
            GameObject secondPrefabObject = null;
            BossBarragePatternProfile pattern = null;
            try
            {
                laneObject = new GameObject("Lane");
                SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
                playerObject = new GameObject("Player");
                playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
                bossObject = new GameObject("Boss");

                pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                SerializedObject serializedPattern = new SerializedObject(pattern);
                serializedPattern.FindProperty("projectilesPerWave").intValue = 1;
                serializedPattern.ApplyModifiedPropertiesWithoutUndo();

                BossBarrageProjectile firstPrefab = CreateProjectilePrefab(
                    "FirstProjectilePrefab",
                    out firstPrefabObject);
                BossBarrageProjectile secondPrefab = CreateProjectilePrefab(
                    "SecondProjectilePrefab",
                    out secondPrefabObject);

                BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
                emitter.ConfigureReferences(lane, playerObject.transform, null);
                emitter.ConfigurePattern(pattern, firstPrefab, 1);

                Assert.That(emitter.BeginWindup(), Is.True);
                Assert.That(emitter.FirePendingWave(), Is.EqualTo(1));

                var activeProjectiles = new List<BossBarrageProjectile>(2);
                Assert.That(emitter.CopyActiveProjectiles(activeProjectiles), Is.EqualTo(1));
                BossBarrageProjectile firstFiredProjectile = activeProjectiles[0];
                Assert.That(firstFiredProjectile.IsActive, Is.True);

                // Switching prefab keys moves the still-live first pool into
                // standby. Capture tooling must continue to observe its shot.
                emitter.ConfigurePattern(pattern, secondPrefab, 1);
                Assert.That(emitter.BeginWindup(), Is.True);
                Assert.That(emitter.FirePendingWave(), Is.EqualTo(1));

                Assert.That(emitter.CopyActiveProjectiles(activeProjectiles), Is.EqualTo(2));
                Assert.That(activeProjectiles[0], Is.SameAs(firstFiredProjectile));
                BossBarrageProjectile secondFiredProjectile = activeProjectiles[1];
                Assert.That(secondFiredProjectile, Is.Not.SameAs(firstFiredProjectile));
                Assert.That(emitter.ActiveProjectileCount, Is.EqualTo(2));

                var repeatedCopy = new List<BossBarrageProjectile>(2);
                Assert.That(emitter.CopyActiveProjectiles(repeatedCopy), Is.EqualTo(2));
                CollectionAssert.AreEqual(activeProjectiles, repeatedCopy,
                    "Unchanged pool state should produce a stable projectile order.");

                firstFiredProjectile.Deactivate();
                activeProjectiles.Add(firstPrefab);
                Assert.That(emitter.CopyActiveProjectiles(activeProjectiles), Is.EqualTo(1));
                Assert.That(activeProjectiles, Has.Count.EqualTo(1));
                Assert.That(activeProjectiles[0], Is.SameAs(secondFiredProjectile));
                Assert.That(emitter.ActiveProjectileCount, Is.EqualTo(1));

                secondFiredProjectile.Deactivate();
                Assert.That(emitter.CopyActiveProjectiles(activeProjectiles), Is.Zero);
                Assert.That(activeProjectiles, Is.Empty);
                Assert.That(emitter.ActiveProjectileCount, Is.Zero);
            }
            finally
            {
                DestroyImmediate(secondPrefabObject);
                DestroyImmediate(firstPrefabObject);
                DestroyImmediate(bossObject);
                DestroyImmediate(playerObject);
                DestroyImmediate(laneObject);
                DestroyImmediate(pattern);
            }
        }

        [Test]
        public void CopyActiveProjectilesRejectsNullDestination()
        {
            GameObject emitterObject = new GameObject("Boss");
            try
            {
                BossBarrageEmitter emitter = emitterObject.AddComponent<BossBarrageEmitter>();
                Assert.Throws<ArgumentNullException>(() => emitter.CopyActiveProjectiles(null));
            }
            finally
            {
                DestroyImmediate(emitterObject);
            }
        }

        private static BossBarrageProjectile CreateProjectilePrefab(
            string name,
            out GameObject prefabObject)
        {
            prefabObject = new GameObject(name);
            prefabObject.AddComponent<SphereCollider>();
            prefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = prefabObject.AddComponent<BossBarrageProjectile>();
            prefabObject.SetActive(false);
            return projectile;
        }

        private static void DestroyImmediate(UnityEngine.Object target)
        {
            if (target != null)
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
