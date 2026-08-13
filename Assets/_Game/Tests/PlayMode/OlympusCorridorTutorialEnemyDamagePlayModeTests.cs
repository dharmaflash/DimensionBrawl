using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCorridorTutorialEnemyDamagePlayModeTests
    {
        [Test]
        public void PassiveTutorialSoldierReceivesLethalDamageWithoutRunningGameplay()
        {
            GameObject soldierObject = new GameObject("TutorialFrontSoldier");
            GameObject directorObject = new GameObject("TutorialDirector");
            try
            {
                soldierObject.AddComponent<CharacterController>();
                CombatHealth health = soldierObject.AddComponent<CombatHealth>();
                health.ConfigureTeam(DamageTeam.Enemy);
                soldierObject.AddComponent<CombatTargetSensor>();
                BasicSoldierEnemy soldier = soldierObject.AddComponent<BasicSoldierEnemy>();
                OlympusCorridorTutorialDirector director =
                    directorObject.AddComponent<OlympusCorridorTutorialDirector>();

                SetPrivateField(
                    director,
                    "tutorialEnemyGameplayBehaviours",
                    new Behaviour[] { health, soldier });
                InvokeEnemyGameplayPolicy(director, enabled: false, keepHealthDamageable: true);

                Assert.That(health.enabled, Is.True, "The passive tutorial target must remain damageable.");
                Assert.That(
                    soldier.enabled,
                    Is.False,
                    "The suspended soldier must not contribute an idle Update callback.");
                Assert.That(soldier.IsGameplaySuspended, Is.True);

                bool applied = health.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    health.MaxHealth + 1f,
                    soldierObject.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly));

                Assert.That(applied, Is.True);
                Assert.That(health.IsAlive, Is.False);
                Assert.That(
                    soldier.CurrentPatternState,
                    Is.EqualTo(CombatAiPatternState.Death),
                    "A lethal tutorial hit must immediately reach the soldier death presentation even while AI is suspended.");

                InvokeEnemyGameplayPolicy(director, enabled: false, keepHealthDamageable: false);
                Assert.That(health.enabled, Is.False, "Terminal tutorial cleanup may disable target health.");
                Assert.That(soldier.enabled, Is.False, "Terminal tutorial cleanup may disable the soldier observer.");
                Assert.That(soldier.IsGameplaySuspended, Is.False, "Terminal cleanup must release the suspended observer lease.");
            }
            finally
            {
                Object.DestroyImmediate(directorObject);
                Object.DestroyImmediate(soldierObject);
            }
        }

        private static void InvokeEnemyGameplayPolicy(
            OlympusCorridorTutorialDirector director,
            bool enabled,
            bool keepHealthDamageable)
        {
            MethodInfo method = typeof(OlympusCorridorTutorialDirector).GetMethod(
                "SetEnemyGameplayEnabled",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(director, new object[] { enabled, keepHealthDamageable });
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
