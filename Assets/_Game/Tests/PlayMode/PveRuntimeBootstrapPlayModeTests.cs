using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using IsekaiBrawl.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class PveRuntimeBootstrapPlayModeTests
    {
        [UnityTest]
        public IEnumerator BattleManagerCreatesReadyStoryPveDirector()
        {
            BattleMode previousMode = BattleModeContext.CurrentMode;
            GameObject battleManagerObject = new("PveRuntimeBootstrapBattleManager");
            GameObject directorObject = null;
            PveStageData runtimeStage = null;

            try
            {
                BattleModeContext.SetMode(BattleMode.StoryPve);
                PveStageContext.Clear();

                BattleManager battleManager = battleManagerObject.AddComponent<BattleManager>();
                InvokePrivate(battleManager, "EnsureStoryPveRuntimeBootstrap");
                runtimeStage = PveStageContext.SelectedStage;

                yield return null;

                PveEncounterDirector director = Object.FindFirstObjectByType<PveEncounterDirector>();
                Assert.IsNotNull(director, "Story PVE bootstrap should create an encounter director.");
                directorObject = director.gameObject;
                Assert.IsTrue(director.enabled, "The runtime-created encounter director should remain enabled.");
                Assert.AreSame(runtimeStage, director.ActiveStage);
                Assert.IsNotNull(director.RuntimeRoot, "The runtime-created director should own a stage root.");
                Assert.AreEqual(
                    director.transform,
                    director.RuntimeRoot.parent,
                    "The generated stage root should be scoped to the director lifecycle.");
                Assert.Greater(director.EncounterGroupCount, 0, "The runtime prototype stage should build encounters.");
            }
            finally
            {
                PveStageContext.Clear();
                BattleModeContext.SetMode(previousMode);
                Object.Destroy(battleManagerObject);
                if (directorObject != null)
                {
                    Object.Destroy(directorObject);
                }
                if (runtimeStage != null)
                {
                    Object.Destroy(runtimeStage);
                }
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator DelayedEnemyPlacementKeepsEncounterOpenUntilDelayElapses()
        {
            BattleMode previousMode = BattleModeContext.CurrentMode;
            GameObject directorObject = new("PveDelayedSpawnDirector");
            PveStageData stage = ScriptableObject.CreateInstance<PveStageData>();

            try
            {
                PveEncounterGroup encounter = new()
                {
                    groupId = "delayed_spawn_test",
                    mustClearToAdvance = true
                };
                encounter.enemyPlacements.Add(new PveEnemyPlacement { spawnDelay = 0.08f });
                SetPrivateField(stage, "encounterGroups", new List<PveEncounterGroup> { encounter });

                BattleModeContext.SetMode(BattleMode.StoryPve);
                PveEncounterDirector director = directorObject.AddComponent<PveEncounterDirector>();
                director.ConfigureRuntimeBootstrap(stage);
                yield return null;

                IList runtimeGroups = GetPrivateField<IList>(director, "runtimeGroups");
                Assert.AreEqual(1, runtimeGroups.Count);
                object runtimeGroup = runtimeGroups[0];

                InvokePrivate(director, "SpawnEncounterContents", runtimeGroup);

                Assert.AreEqual(1, GetPublicProperty<int>(runtimeGroup, "PendingEnemySpawnCount"));
                Assert.IsFalse(
                    (bool)InvokePrivate(director, "IsGroupCleared", runtimeGroup),
                    "An encounter must not clear while a delayed enemy spawn is pending.");

                yield return new WaitForSeconds(0.12f);

                Assert.AreEqual(0, GetPublicProperty<int>(runtimeGroup, "PendingEnemySpawnCount"));
                Assert.IsTrue(
                    (bool)InvokePrivate(director, "IsGroupCleared", runtimeGroup),
                    "The empty test encounter may clear after its delayed spawn attempt completes.");
            }
            finally
            {
                BattleModeContext.SetMode(previousMode);
                Object.Destroy(directorObject);
                Object.Destroy(stage);
            }

            yield return null;
        }

        private static object InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Expected method '{methodName}'.");
            return method.Invoke(target, arguments);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected field '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static T GetPublicProperty<T>(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(property, $"Expected property '{propertyName}'.");
            return (T)property.GetValue(target);
        }
    }
}
