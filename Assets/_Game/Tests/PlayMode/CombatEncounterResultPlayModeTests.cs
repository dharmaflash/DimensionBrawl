using DimensionBrawl.Combat;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class CombatEncounterResultPlayModeTests
    {
        [Test]
        public void EnemyDeathPublishesWinExactlyOnce()
        {
            RunTerminalResultTest(
                healthToDefeatIsEnemy: true,
                expectedWinCount: 1,
                expectedFailureCount: 0);
        }

        [Test]
        public void PlayerDeathPublishesFailureExactlyOnce()
        {
            RunTerminalResultTest(
                healthToDefeatIsEnemy: false,
                expectedWinCount: 0,
                expectedFailureCount: 1);
        }

        private static void RunTerminalResultTest(
            bool healthToDefeatIsEnemy,
            int expectedWinCount,
            int expectedFailureCount)
        {
            GameObject playerObject = new("EncounterResultPlayer");
            GameObject enemyObject = new("EncounterResultEnemy");
            GameObject encounterObject = new("EncounterResultController");

            try
            {
                CombatHealth playerHealth = CreateHealth(playerObject, DamageTeam.Player);
                CombatHealth enemyHealth = CreateHealth(enemyObject, DamageTeam.Enemy);
                CombatEncounterController encounter =
                    encounterObject.AddComponent<CombatEncounterController>();
                encounter.ConfigureCombatants(playerHealth, enemyHealth);

                int wonCount = 0;
                int failedCount = 0;
                encounter.Won += HandleWon;
                encounter.Failed += HandleFailed;

                CombatHealth firstDefeat = healthToDefeatIsEnemy ? enemyHealth : playerHealth;
                CombatHealth secondDefeat = healthToDefeatIsEnemy ? playerHealth : enemyHealth;
                DamageTeam firstSourceTeam = healthToDefeatIsEnemy
                    ? DamageTeam.Player
                    : DamageTeam.Enemy;
                DamageTeam secondSourceTeam = healthToDefeatIsEnemy
                    ? DamageTeam.Enemy
                    : DamageTeam.Player;

                Assert.That(ApplyLethalDamage(firstDefeat, firstSourceTeam), Is.True);
                Assert.That(encounter.IsRunning, Is.False);
                Assert.That(encounter.IsWon, Is.EqualTo(healthToDefeatIsEnemy));
                Assert.That(encounter.IsFailed, Is.EqualTo(!healthToDefeatIsEnemy));
                Assert.That(wonCount, Is.EqualTo(expectedWinCount));
                Assert.That(failedCount, Is.EqualTo(expectedFailureCount));

                Assert.That(ApplyLethalDamage(secondDefeat, secondSourceTeam), Is.True);
                Assert.That(
                    wonCount,
                    Is.EqualTo(expectedWinCount),
                    "A terminal encounter must not publish a later opposing result.");
                Assert.That(
                    failedCount,
                    Is.EqualTo(expectedFailureCount),
                    "A terminal encounter must publish its result exactly once.");

                encounter.Won -= HandleWon;
                encounter.Failed -= HandleFailed;

                void HandleWon()
                {
                    wonCount++;
                }

                void HandleFailed()
                {
                    failedCount++;
                }
            }
            finally
            {
                Object.DestroyImmediate(encounterObject);
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        private static CombatHealth CreateHealth(GameObject owner, DamageTeam team)
        {
            CombatHealth health = owner.AddComponent<CombatHealth>();
            health.ConfigureTeam(team);
            health.ConfigureMaxHealth(100f);
            return health;
        }

        private static bool ApplyLethalDamage(CombatHealth target, DamageTeam sourceTeam)
        {
            return target.TryApplyDamage(new DamageInfo(
                null,
                sourceTeam,
                target.MaxHealth + 1f,
                target.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None));
        }
    }
}
