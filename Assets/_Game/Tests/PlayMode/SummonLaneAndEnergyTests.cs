using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
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
        public void EnergyRewardPulseCanOpenCurrentTierSpend()
        {
            GameObject playerObject = new GameObject("Player");
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();

            energy.GrantCurrentTierEnergy(99f);
            Assert.IsFalse(energy.CanSpend);
            Assert.AreEqual(1, energy.ChargingTier);
            Assert.AreEqual(99f, energy.CurrentTierEnergy, 0.001f);

            energy.GrantCurrentTierEnergy(1f);
            Assert.IsTrue(energy.CanSpend);
            Assert.AreEqual(1, energy.AvailableTier);
            Assert.AreEqual(2, energy.ChargingTier);
            Assert.AreEqual(0f, energy.CurrentTierEnergy, 0.001f);

            Assert.IsTrue(energy.TrySpend(out int spentTier));
            Assert.AreEqual(1, spentTier);
            Assert.AreEqual(0, energy.AvailableTier);
            Assert.AreEqual(1, energy.ChargingTier);

            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void EnergyRewardPulseCarriesOverflowIntoHigherTierReadiness()
        {
            GameObject playerObject = new GameObject("Player");
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();

            energy.GrantCurrentTierEnergy(200f);

            Assert.IsTrue(energy.CanSpend, "A large EN pulse should still leave a spendable tier available.");
            Assert.AreEqual(2, energy.AvailableTier, "Overflow EN should carry through LV1 and open LV2 readiness.");
            Assert.AreEqual(3, energy.ChargingTier, "After opening LV2, the ladder should keep charging toward LV3.");
            Assert.AreEqual(0f, energy.CurrentTierEnergy, 0.001f);

            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void SummonFrontlineProxyReportsLifetimeAndDefeatExitReasons()
        {
            GameObject proxyObject = new GameObject("SummonProxy");
            CombatHealth health = proxyObject.AddComponent<CombatHealth>();
            health.ConfigureTeam(DamageTeam.AllySummon);
            health.ResetHealthToFull();
            SummonFrontlineProxy proxy = proxyObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(health);

            proxy.Activate(Vector3.zero, Vector3.forward, 1, 0.2f, 1f, 1f, 0.1f);
            Assert.IsTrue(proxy.IsActive);
            proxy.Tick(0.25f);
            Assert.IsFalse(proxy.IsActive);
            Assert.AreEqual(SummonFrontlineProxyExitReason.LifetimeExpired, proxy.LastExitReason);

            proxyObject.SetActive(true);
            Vector3 targetPosition = new Vector3(2f, 0f, 5f);
            proxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, targetPosition, 0.5f);
            Assert.IsTrue(proxy.IsActive);
            Assert.IsFalse(proxy.HasLifetimeLimit);
            Assert.IsTrue(float.IsPositiveInfinity(proxy.RemainingLifetimeSeconds));
            Assert.AreEqual(targetPosition, proxy.AdvanceTargetPosition);
            proxy.Tick(10f);
            Assert.IsTrue(
                proxy.IsActive,
                "A zero actor lifetime should mean a normal summon actor persists until defeated or recalled.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, proxy.LastExitReason);

            proxyObject.SetActive(true);
            proxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 1f, 0.1f);
            Assert.IsTrue(proxy.IsActive);
            Assert.AreEqual(1f, proxy.HealthRatio, 0.001f);

            Assert.IsTrue(health.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                999f,
                Vector3.zero,
                Vector3.back,
                0f)));
            Assert.IsFalse(proxy.IsActive);
            Assert.AreEqual(SummonFrontlineProxyExitReason.Defeated, proxy.LastExitReason);

            Object.DestroyImmediate(proxyObject);
        }

        [Test]
        public void SummonBodyAndPressureScreenResolveDifferentCombatContacts()
        {
            GameObject actorObject = new GameObject("EnemySummonActor");
            SphereCollider bodyCollider = actorObject.AddComponent<SphereCollider>();
            CombatHealth actorHealth = actorObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.Enemy);
            actorHealth.ResetHealthToFull();

            GameObject screenObject = new GameObject("PressureScreen");
            screenObject.transform.SetParent(actorObject.transform, worldPositionStays: false);
            SphereCollider screenCollider = screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = screenObject.AddComponent<SummonPressureScreen>();
            pressureScreen.Activate(DamageTeam.Enemy, 1, 1f, 1f);

            GameObject projectileObject = new GameObject("PlayerProjectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectile = projectileObject.AddComponent<LaneActionProjectile>();
            projectile.Configure(null, DamageTeam.Player, 30f, Vector3.forward, 0f, 1f, 0.2f);

            Assert.IsFalse(
                projectile.TryApplyImpact(screenCollider, Vector3.zero),
                "Pressure-screen contact should be handled by the screen, not by summon body health.");
            Assert.AreEqual(1f, actorHealth.HealthRatio, 0.001f);

            Assert.IsTrue(pressureScreen.TryIntercept(projectile));
            Assert.IsFalse(projectile.IsActive);

            projectileObject.SetActive(true);
            projectile.Configure(null, DamageTeam.Player, 30f, Vector3.forward, 0f, 1f, 0.2f);
            Assert.IsTrue(projectile.TryApplyImpact(bodyCollider, Vector3.zero));
            Assert.Less(actorHealth.HealthRatio, 1f);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(actorObject);
        }

        [Test]
        public void BossBarrageProjectileCanDefeatAllySummonBody()
        {
            GameObject actorObject = new GameObject("AllySummonActor");
            SphereCollider bodyCollider = actorObject.AddComponent<SphereCollider>();
            CombatHealth actorHealth = actorObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.AllySummon);
            actorHealth.ResetHealthToFull();
            SummonFrontlineProxy proxy = actorObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(actorHealth);
            proxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 1f, 0.1f);

            GameObject projectileObject = new GameObject("BossProjectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 999f, Vector3.back, 0f, 1f, 0.2f);

            Assert.IsTrue(projectile.TryApplyImpact(bodyCollider, Vector3.zero));
            Assert.IsFalse(proxy.IsActive);
            Assert.AreEqual(SummonFrontlineProxyExitReason.Defeated, proxy.LastExitReason);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(actorObject);
        }

        [Test]
        public void SummonFrontlineClashDamagesHostileSummonsAndHoldsAdvance()
        {
            GameObject allyObject = new GameObject("AllySummonActor");
            SphereCollider allyCollider = allyObject.AddComponent<SphereCollider>();
            allyCollider.isTrigger = true;
            allyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth allyHealth = allyObject.AddComponent<CombatHealth>();
            allyHealth.ConfigureTeam(DamageTeam.AllySummon);
            allyHealth.ResetHealthToFull();
            SummonFrontlineProxy allyProxy = allyObject.AddComponent<SummonFrontlineProxy>();
            allyProxy.ConfigureHealth(allyHealth);
            SummonFrontlineClash allyClash = allyObject.AddComponent<SummonFrontlineClash>();
            allyClash.ConfigureReferences(allyProxy, allyHealth);
            allyClash.ConfigureTuning(100f, 0.2f, 0f, 0.3f);
            GameObject pulseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulseObject.name = "TierPulseCore";
            pulseObject.transform.SetParent(allyObject.transform, worldPositionStays: false);
            Collider pulseCollider = pulseObject.GetComponent<Collider>();
            Object.DestroyImmediate(pulseCollider);
            Renderer pulseRenderer = pulseObject.GetComponent<Renderer>();
            SummonFrontlineProxyPresenter allyPresenter = allyObject.AddComponent<SummonFrontlineProxyPresenter>();
            allyPresenter.ConfigurePresentation(allyProxy, pulseObject.transform, new[] { pulseRenderer });
            allyPresenter.ConfigureClashReference(allyClash);

            GameObject enemyObject = new GameObject("EnemySummonActor");
            SphereCollider enemyCollider = enemyObject.AddComponent<SphereCollider>();
            enemyCollider.isTrigger = true;
            enemyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ResetHealthToFull();
            SummonFrontlineProxy enemyProxy = enemyObject.AddComponent<SummonFrontlineProxy>();
            enemyProxy.ConfigureHealth(enemyHealth);

            allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 3f, 1f);
            enemyProxy.Activate(Vector3.forward * 0.6f, Vector3.back, 1, 2f, 1f, 3f, 1f);

            Assert.IsTrue(allyClash.TryProcessClash(enemyCollider));
            Assert.Less(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(enemyProxy.IsAdvanceHeld);
            Assert.IsTrue(allyClash.IsClashing);
            Assert.AreEqual(1, allyClash.TotalClashCount);
            Assert.AreEqual(DamageTeam.Enemy, allyClash.LastOpponentTeam);
            allyPresenter.RefreshNow();
            Assert.IsTrue(allyPresenter.IsShowing);
            Assert.AreEqual(1, allyPresenter.LastObservedClashCount);
            Assert.AreEqual(
                1,
                allyPresenter.ClashFlashCount,
                "The summon proxy presenter should pulse when body clash damage occurs so the duel is readable in world space.");

            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(allyObject);
        }

        [Test]
        public void SummonFrontlineClashDamagesHostileBodyTargetAndHoldsAdvance()
        {
            GameObject allyObject = new GameObject("AllySummonActor");
            SphereCollider allyCollider = allyObject.AddComponent<SphereCollider>();
            allyCollider.isTrigger = true;
            allyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth allyHealth = allyObject.AddComponent<CombatHealth>();
            allyHealth.ConfigureTeam(DamageTeam.AllySummon);
            allyHealth.ResetHealthToFull();
            SummonFrontlineProxy allyProxy = allyObject.AddComponent<SummonFrontlineProxy>();
            allyProxy.ConfigureHealth(allyHealth);
            SummonFrontlineClash allyClash = allyObject.AddComponent<SummonFrontlineClash>();
            allyClash.ConfigureReferences(allyProxy, allyHealth);
            allyClash.ConfigureTuning(90f, 0.2f, 0f, 0.3f);

            GameObject bossObject = new GameObject("BossBodyTarget");
            bossObject.transform.position = Vector3.forward * 0.7f;
            SphereCollider bossCollider = bossObject.AddComponent<SphereCollider>();
            bossObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ResetHealthToFull();

            allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, Vector3.forward * 4f, 3f);

            Assert.IsTrue(allyClash.TryProcessClash(bossCollider));
            Assert.Less(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(allyClash.IsClashing);
            Assert.AreEqual(1, allyClash.TotalClashCount);
            Assert.AreEqual(0, allyClash.LastOpponentTier);
            Assert.AreEqual(DamageTeam.Enemy, allyClash.LastOpponentTeam);

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(allyObject);
        }

        [Test]
        public void SummonSlot1EntryStartsInFrontOfPlayerBodyAndAdvancesPastFrontline()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(2.25f, -2f);
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
            energy.ConfigureReferences(lane, playerObject.transform);
            energy.GrantCurrentTierEnergy(energy.CurrentTierTarget);

            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ, 1.4f);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            GameObject projectilePrefabObject = new GameObject("SummonProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectilePrefab = projectilePrefabObject.AddComponent<LaneActionProjectile>();
            projectilePrefabObject.SetActive(false);

            GameObject actorPrefabObject = new GameObject("SummonActorPrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.AllySummon);
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefabObject.SetActive(false);

            PlayerSummonSlot1Action summonAction = playerObject.AddComponent<PlayerSummonSlot1Action>();
            summonAction.ConfigureReferences(
                energy,
                playerHealth,
                null,
                bossHealth,
                lane,
                projectilePrefab,
                null,
                null,
                null,
                actorPrefab,
                null);

            Assert.IsTrue(summonAction.TryUseSummonSlot1());
            Vector2 entryLane = lane.GetLaneCoordinates(summonAction.LastEntryPosition);
            Vector2 playerLane = lane.GetLaneCoordinates(playerObject.transform.position);
            Assert.AreEqual(playerLane.x, entryLane.x, 0.001f);
            Assert.AreEqual(playerLane.y + 1.35f, entryLane.y, 0.001f);

            SummonFrontlineProxy activeProxy = null;
            SummonFrontlineProxy[] proxies = Object.FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < proxies.Length; i++)
            {
                if (proxies[i] != null && proxies[i].IsActive)
                {
                    activeProxy = proxies[i];
                    break;
                }
            }

            Assert.IsNotNull(activeProxy);
            Assert.IsTrue(
                lane.IsPastForwardBoundary(activeProxy.AdvanceTargetPosition),
                "The summon starts at the player's body front, but its frontline advance is allowed to cross the player boundary.");
            Vector2 advanceTargetLane = lane.GetLaneCoordinates(activeProxy.AdvanceTargetPosition);
            Assert.AreEqual(
                lane.BossProxyZ,
                advanceTargetLane.y,
                0.001f,
                "A normal summon actor should advance toward the far/frontline target instead of stopping after a short fixed distance.");
            Assert.AreEqual(
                0f,
                advanceTargetLane.x,
                0.001f,
                "SummonSlot1 should pressure the boss/frontline target lane, not remain locked to the player's lateral entry line.");
            Assert.IsFalse(activeProxy.HasLifetimeLimit);
            Assert.IsTrue(
                float.IsPositiveInfinity(activeProxy.RemainingLifetimeSeconds),
                "Default normal summon actors should not disappear from a generic actor timer.");
            Vector3 actorPositionBeforeTick = activeProxy.transform.position;
            activeProxy.Tick(1f);
            Vector2 actorLaneBeforeTick = lane.GetLaneCoordinates(actorPositionBeforeTick);
            Vector2 actorLaneAfterOneSecond = lane.GetLaneCoordinates(activeProxy.transform.position);
            Assert.Greater(
                actorLaneAfterOneSecond.y,
                actorLaneBeforeTick.y + 0.5f,
                "A summon actor should visibly march forward after appearing in front of the player.");
            Assert.Less(
                activeProxy.AdvanceProgress01,
                0.25f,
                "A normal summon actor should not snap to the far target during the first second of travel.");
            Assert.Less(
                actorLaneAfterOneSecond.y,
                advanceTargetLane.y - 2f,
                "A normal summon should still be crossing the corridor after one second, not already parked at the boss lane.");

            energy.GrantCurrentTierEnergy(energy.CurrentTierTarget);
            Assert.IsTrue(summonAction.TryUseSummonSlot1());
            int activeProxyCount = 0;
            proxies = Object.FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < proxies.Length; i++)
            {
                if (proxies[i] != null && proxies[i].IsActive)
                {
                    activeProxyCount++;
                }
            }

            Assert.AreEqual(
                1,
                activeProxyCount,
                "The first review slice should keep one active actor per summon slot instead of accumulating unlimited persistent actors.");

            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void SummonSlot1MaxActiveActorPolicyAllowsAuthoredMultiSummon()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, -2f);
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
            energy.ConfigureReferences(lane, playerObject.transform);

            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ, 1.4f);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            GameObject projectilePrefabObject = new GameObject("SummonProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectilePrefab = projectilePrefabObject.AddComponent<LaneActionProjectile>();
            projectilePrefabObject.SetActive(false);

            GameObject actorPrefabObject = new GameObject("SummonActorPrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.AllySummon);
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefabObject.SetActive(false);

            PlayerSummonSlot1Action summonAction = playerObject.AddComponent<PlayerSummonSlot1Action>();
            summonAction.ConfigureReferences(
                energy,
                playerHealth,
                null,
                bossHealth,
                lane,
                projectilePrefab,
                null,
                null,
                null,
                actorPrefab,
                null);

            SerializedObject serializedAction = new SerializedObject(summonAction);
            SerializedProperty maxActiveActors = serializedAction.FindProperty("maxActiveSummonActors");
            Assert.IsNotNull(maxActiveActors);
            maxActiveActors.intValue = 2;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            energy.GrantCurrentTierEnergy(energy.CurrentTierTarget);
            Assert.IsTrue(summonAction.TryUseSummonSlot1());
            energy.GrantCurrentTierEnergy(energy.CurrentTierTarget);
            Assert.IsTrue(summonAction.TryUseSummonSlot1());

            Assert.AreEqual(
                2,
                summonAction.ActiveSummonActorCount,
                "Raising the authored active actor cap should allow multiple persistent summon actors before recall logic trims them.");

            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureCostGainsFasterWhenBossCommitsForward()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject bossObject = new GameObject("Boss");
            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);

            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ);
            bossCost.Tick(1f);
            float backlineCost = bossCost.CurrentTierCost;

            bossCost.ResetLadder();
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.ForwardBoundaryZ);
            bossCost.Tick(1f);
            float forwardCost = bossCost.CurrentTierCost;

            Assert.Greater(
                forwardCost,
                backlineCost,
                "A boss that commits toward the frontline should build action cost faster than a boss staying safely back.");

            bossCost.GrantCurrentTierCost(300f);
            Assert.AreEqual(3, bossCost.AvailableTier, "Boss cost should expose LV3 readiness after enough gain.");
            Assert.IsTrue(bossCost.TrySpend(out int spentTier));
            Assert.AreEqual(3, spentTier);
            Assert.AreEqual(0, bossCost.AvailableTier);
            Assert.AreEqual(1, bossCost.ChargingTier);

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressurePositionControllerCommitsForwardAsBossCostBuilds()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject bossObject = new GameObject("BossProxy");
            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            BossPressurePositionController positionController =
                bossObject.AddComponent<BossPressurePositionController>();
            positionController.ConfigureReferences(lane, bossCost, director, bossObject.transform);
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ, 1.6f);

            positionController.Tick(1f);
            float restRisk = bossCost.EvaluateBossForwardRisk01(bossObject.transform.position);

            bossCost.GrantCurrentTierCost(50f);
            positionController.Tick(1f);
            float buildingRisk = bossCost.EvaluateBossForwardRisk01(bossObject.transform.position);

            bossCost.GrantCurrentTierCost(50f);
            positionController.Tick(1f);
            float readyRisk = bossCost.EvaluateBossForwardRisk01(bossObject.transform.position);

            director.SetActionsEnabled(false);
            positionController.Tick(1f);
            float disabledRisk = bossCost.EvaluateBossForwardRisk01(bossObject.transform.position);

            Assert.AreEqual(0.08f, restRisk, 0.001f);
            Assert.Greater(buildingRisk, restRisk);
            Assert.Greater(readyRisk, buildingRisk);
            Assert.Less(disabledRisk, readyRisk);

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorQueuesCostedPriorityPattern()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile levelOnePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile levelThreePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(300f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, null, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    levelOnePattern,
                    BossPressureActionKind.SkillPattern,
                    1,
                    1,
                    0f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    levelThreePattern,
                    BossPressureActionKind.PunishOverextend,
                    3,
                    1,
                    0f,
                    usePlayerForwardRiskGate: true,
                    minimumPlayerForwardRisk01: 0.66f,
                    maximumPlayerForwardRisk01: 1f)
            });

            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(levelThreePattern, emitter.QueuedPriorityPattern);
            Assert.AreSame(levelThreePattern, emitter.CurrentPattern);
            Assert.AreEqual(0, bossCost.AvailableTier, "Queuing a costed boss action should spend boss pressure cost.");
            Assert.AreEqual(BossPressureActionKind.PunishOverextend, director.LastActionKind);
            Assert.AreEqual(1, director.TotalActionCount);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(levelThreePattern);
            Object.DestroyImmediate(levelOnePattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorHoldsOverextendPunishUntilPlayerIsForward()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile linePressurePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile punishPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(300f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, null, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    linePressurePattern,
                    BossPressureActionKind.SkillPattern,
                    1,
                    1,
                    0f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    punishPattern,
                    BossPressureActionKind.PunishOverextend,
                    3,
                    1,
                    0f,
                    usePlayerForwardRiskGate: true,
                    minimumPlayerForwardRisk01: 0.66f,
                    maximumPlayerForwardRisk01: 1f)
            });

            Assert.AreEqual(0f, director.CurrentPlayerForwardRisk01, 0.001f);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(
                linePressurePattern,
                emitter.QueuedPriorityPattern,
                "A backline player should not receive the overextend punish even when the boss has LV3 cost.");
            Assert.AreEqual(BossPressureActionKind.SkillPattern, director.LastActionKind);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(punishPattern);
            Object.DestroyImmediate(linePressurePattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorDoesNotSpendSummonPressureWithoutSummonOwner()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(200f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f)
            });

            Assert.IsFalse(director.TryQueueBestAvailableAction());
            Assert.AreEqual(2, bossCost.AvailableTier);
            Assert.AreEqual(0, director.TotalActionCount);
            Assert.IsFalse(emitter.HasQueuedPriorityPattern);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorReleasesCostedSummonPressure()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            GameObject actorPrefabObject = new GameObject("BossSummonPressurePrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = actorPrefabObject.AddComponent<SummonPressureScreen>();
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigurePresentation(actorPrefabObject.transform, pressureScreen);
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(200f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, summonAction);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f)
            });

            Assert.IsTrue(director.TryQueueBestAvailableAction());

            Assert.AreSame(summonPattern, emitter.QueuedPriorityPattern);
            Assert.AreEqual(0, bossCost.AvailableTier, "A boss summon pressure action should spend boss pressure cost.");
            Assert.AreEqual(1, summonAction.TotalReleaseCount);
            Assert.AreEqual(2, summonAction.LastReleasedTier);
            Assert.AreEqual(1, summonAction.ActiveSummonActorCount);
            Assert.AreEqual(1, summonAction.ActivePressureScreenCount);
            Assert.Greater(summonAction.ActivePressureScreenRemainingIntercepts, 0);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorCanHoldLevelOneForNextTierSummonPressure()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile linePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            GameObject actorPrefabObject = new GameObject("BossSummonPressurePrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = actorPrefabObject.AddComponent<SummonPressureScreen>();
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigurePresentation(actorPrefabObject.transform, pressureScreen);
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(100f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    linePattern,
                    BossPressureActionKind.SkillPattern,
                    1,
                    1,
                    0f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f,
                    usePlayerForwardRiskGate: true,
                    minimumPlayerForwardRisk01: 0.32f,
                    maximumPlayerForwardRisk01: 1f)
            });
            director.SetHoldForNextTierActionWhenGateAllows(true);

            Assert.IsFalse(
                director.TryQueueBestAvailableAction(),
                "With the hold policy enabled, LV1 should wait when an authored LV2 summon-pressure action is gated open.");
            Assert.AreEqual(1, bossCost.AvailableTier);
            Assert.IsFalse(emitter.HasQueuedPriorityPattern);

            bossCost.GrantCurrentTierCost(100f);

            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(summonPattern, emitter.QueuedPriorityPattern);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.AreEqual(2, summonAction.LastReleasedTier);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(linePattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
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
        public void BossBarrageCoverFireCanTargetLaneCenterInsteadOfTrackingPlayerSide()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            serializedObject.FindProperty("laneCenterLateralRatio").floatValue = 0f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float targetLateralX = pattern.ResolveTargetLateralX(3.5f, 5f);

            Assert.AreEqual(
                0f,
                targetLateralX,
                0.001f,
                "CoverFire-style lane pressure should be able to suppress the authored center path instead of following the player's current side.");

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
        public void BossBarrageSideClampCanMirrorPressureDirection()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.SideClamp;
            serializedObject.FindProperty("sideClampDirection").floatValue = 1f;
            serializedObject.FindProperty("sideClampCrossReachRatio").floatValue = 0.25f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float firstOffset = pattern.GetLateralOffset(0, 5, 0f);
            float lastOffset = pattern.GetLateralOffset(4, 5, 0f);

            Assert.Greater(firstOffset, 0f, "Right clamp should start pressure from the right side.");
            Assert.Less(lastOffset, 0f, "Right clamp should reach across center to narrow the left-side safe gap.");
            Assert.Less(
                Mathf.Abs(lastOffset),
                Mathf.Abs(firstOffset),
                "Mirrored side clamp should keep the opposite-side escape gap readable.");

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
        public void BossBarrageEscortScreenAlternatesCurtainSidesAndTightensForwardRisk()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.EscortScreen;
            serializedObject.FindProperty("backlineHalfSpread").floatValue = 4f;
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 1.6f;
            serializedObject.FindProperty("escortScreenInnerGapRatio").floatValue = 0.35f;
            serializedObject.FindProperty("backlineDepthSpread").floatValue = 2.4f;
            serializedObject.FindProperty("forwardDepthSpread").floatValue = 0.9f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float outerLeft = pattern.GetLateralOffset(0, 6, 0f);
            float outerRight = pattern.GetLateralOffset(1, 6, 0f);
            float innerLeftBackline = pattern.GetLateralOffset(4, 6, 0f);
            float innerLeftForward = pattern.GetLateralOffset(4, 6, 1f);
            float backlineDepth = pattern.GetTargetDepthOffset(5, 6, 0f);
            float forwardDepth = pattern.GetTargetDepthOffset(5, 6, 1f);

            Assert.Less(outerLeft, 0f);
            Assert.Greater(outerRight, 0f);
            Assert.Less(Mathf.Abs(innerLeftBackline), Mathf.Abs(outerLeft));
            Assert.Less(Mathf.Abs(innerLeftForward), Mathf.Abs(innerLeftBackline));
            Assert.Less(forwardDepth, backlineDepth);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageLayeredSalvoUsesDepthRowsAndTightensForwardRisk()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.LayeredSalvo;
            serializedObject.FindProperty("backlineHalfSpread").floatValue = 4.2f;
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 1.75f;
            serializedObject.FindProperty("layeredSalvoRowCount").intValue = 3;
            serializedObject.FindProperty("backlineDepthSpread").floatValue = 3.2f;
            serializedObject.FindProperty("forwardDepthSpread").floatValue = 1.1f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float firstRowLeft = pattern.GetLateralOffset(0, 9, 0f);
            float firstRowRight = pattern.GetLateralOffset(2, 9, 0f);
            float secondRowLeft = pattern.GetLateralOffset(3, 9, 0f);
            float secondRowRight = pattern.GetLateralOffset(5, 9, 0f);
            float thirdRowLeft = pattern.GetLateralOffset(6, 9, 0f);
            float thirdRowRight = pattern.GetLateralOffset(8, 9, 0f);
            float backDepthA = pattern.GetTargetDepthOffset(0, 9, 0f);
            float backDepthB = pattern.GetTargetDepthOffset(3, 9, 0f);
            float backDepthC = pattern.GetTargetDepthOffset(6, 9, 0f);
            float forwardDepthA = pattern.GetTargetDepthOffset(0, 9, 1f);
            float forwardDepthC = pattern.GetTargetDepthOffset(6, 9, 1f);

            Assert.Less(firstRowLeft, 0f);
            Assert.Greater(firstRowRight, 0f);
            Assert.Greater(secondRowLeft, 0f);
            Assert.Less(secondRowRight, 0f);
            Assert.Less(Mathf.Abs(thirdRowLeft), Mathf.Abs(firstRowLeft));
            Assert.Less(Mathf.Abs(thirdRowRight), Mathf.Abs(firstRowRight));
            Assert.Less(backDepthA, backDepthB);
            Assert.Less(backDepthB, backDepthC);
            Assert.Less(Mathf.Abs(forwardDepthC - forwardDepthA), Mathf.Abs(backDepthC - backDepthA));

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageStaggeredCrossfireAlternatesPairsAndTightensForwardRisk()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("lateralShape").enumValueIndex = (int)BossBarrageLateralShape.StaggeredCrossfire;
            serializedObject.FindProperty("backlineHalfSpread").floatValue = 4.35f;
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 1.95f;
            serializedObject.FindProperty("crossfireInnerGapRatio").floatValue = 0.3f;
            serializedObject.FindProperty("backlineDepthSpread").floatValue = 2.8f;
            serializedObject.FindProperty("forwardDepthSpread").floatValue = 0.95f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float firstLeft = pattern.GetLateralOffset(0, 6, 0f);
            float firstRight = pattern.GetLateralOffset(1, 6, 0f);
            float secondRight = pattern.GetLateralOffset(2, 6, 0f);
            float secondLeft = pattern.GetLateralOffset(3, 6, 0f);
            float lastBackline = pattern.GetLateralOffset(5, 6, 0f);
            float lastForward = pattern.GetLateralOffset(5, 6, 1f);
            float backDepthA = pattern.GetTargetDepthOffset(0, 6, 0f);
            float backDepthC = pattern.GetTargetDepthOffset(4, 6, 0f);
            float forwardDepthA = pattern.GetTargetDepthOffset(0, 6, 1f);
            float forwardDepthC = pattern.GetTargetDepthOffset(4, 6, 1f);

            Assert.Less(firstLeft, 0f);
            Assert.Greater(firstRight, 0f);
            Assert.Greater(secondRight, 0f, "The second crossfire pair should reverse side order.");
            Assert.Less(secondLeft, 0f, "The second crossfire pair should reverse side order.");
            Assert.Less(Mathf.Abs(lastBackline), Mathf.Abs(firstLeft));
            Assert.Less(Mathf.Abs(lastForward), Mathf.Abs(lastBackline));
            Assert.Less(backDepthA, backDepthC);
            Assert.Less(Mathf.Abs(forwardDepthC - forwardDepthA), Mathf.Abs(backDepthC - backDepthA));

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageProjectileDeactivatesWhenSourceHealthDies()
        {
            GameObject bossObject = new GameObject("Boss");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            GameObject projectileObject = new GameObject("BossProjectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
            projectile.Configure(
                bossHealth,
                DamageTeam.Enemy,
                10f,
                Vector3.back,
                3f,
                4f,
                0.3f);

            Assert.IsTrue(projectile.IsActive);
            Assert.IsTrue(bossHealth.IsAlive);

            bossHealth.TryApplyDamage(new DamageInfo(null, DamageTeam.Player, 9999f, Vector3.zero, Vector3.zero, 0f));
            Assert.IsFalse(bossHealth.IsAlive);
            projectile.Tick(0.02f);

            Assert.IsFalse(projectile.IsActive);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(bossObject);
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
            Assert.AreEqual(1, screen.ActiveTier);
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
        public void SummonPressureScreenScansOverlappingHostileProjectilesDuringTick()
        {
            GameObject screenObject = new GameObject("SummonPressureScreen");
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen screen = screenObject.AddComponent<SummonPressureScreen>();
            screen.Activate(DamageTeam.AllySummon, 1, 1.25f, 1f);

            GameObject enemyProjectileObject = new GameObject("EnemyProjectile");
            enemyProjectileObject.transform.position = screenObject.transform.position + Vector3.right * 0.25f;
            enemyProjectileObject.AddComponent<SphereCollider>();
            enemyProjectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile enemyProjectile = enemyProjectileObject.AddComponent<BossBarrageProjectile>();
            enemyProjectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);

            Physics.SyncTransforms();
            Assert.IsTrue(enemyProjectile.IsActive);

            screen.Tick(0.01f);

            Assert.IsFalse(
                enemyProjectile.IsActive,
                "Summon pressure screens should actively absorb overlapping hostile boss projectiles, not only rely on trigger-enter timing.");
            Assert.AreEqual(1, screen.InterceptedProjectiles);

            Object.DestroyImmediate(enemyProjectileObject);
            Object.DestroyImmediate(screenObject);
        }

        [Test]
        public void SummonPressureScreenPresenterShowsActivationAndFinalHitFlash()
        {
            GameObject screenObject = new GameObject("SummonPressureScreen");
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen screen = screenObject.AddComponent<SummonPressureScreen>();

            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualObject.name = "PressureScreenVisual";
            visualObject.transform.SetParent(screenObject.transform, worldPositionStays: false);
            Collider visualCollider = visualObject.GetComponent<Collider>();
            Object.DestroyImmediate(visualCollider);
            Renderer visualRenderer = visualObject.GetComponent<Renderer>();
            SummonPressureScreenPresenter presenter = screenObject.AddComponent<SummonPressureScreenPresenter>();
            presenter.ConfigurePresentation(screen, visualObject.transform, new[] { visualRenderer });

            screen.Activate(DamageTeam.AllySummon, 1, 1.25f, 1f, 3);
            Assert.IsTrue(presenter.IsShowing);
            Assert.IsTrue(visualObject.activeSelf);
            Assert.AreEqual(3, screen.ActiveTier);
            Assert.AreEqual(
                3,
                presenter.LastObservedTier,
                "The pressure-screen presenter should read the active summon tier so LV1-LV3 blocks are not HUD-only.");
            Vector3 visualLocalPositionBeforeIntercept = visualObject.transform.localPosition;

            GameObject enemyProjectileObject = new GameObject("EnemyProjectile");
            enemyProjectileObject.AddComponent<SphereCollider>();
            enemyProjectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile enemyProjectile = enemyProjectileObject.AddComponent<BossBarrageProjectile>();
            enemyProjectileObject.transform.position = new Vector3(0f, 0f, 0.75f);
            enemyProjectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);

            Assert.IsTrue(screen.TryIntercept(enemyProjectile));
            Assert.IsTrue(
                presenter.IsShowing,
                "The final intercept should linger briefly instead of disappearing on the same frame.");
            Assert.AreEqual(1, presenter.InterceptFlashCount);
            Assert.Greater(
                (visualObject.transform.localPosition - visualLocalPositionBeforeIntercept).sqrMagnitude,
                0.0001f,
                "The pressure screen visual should briefly punch on intercept so the boss-fire block reads in world space.");

            screen.Activate(DamageTeam.AllySummon, 1, 1.25f, 1f);
            screen.Deactivate();
            Assert.IsFalse(
                presenter.IsShowing,
                "A pressure screen with no intercept flash should hide when it deactivates.");

            Object.DestroyImmediate(enemyProjectileObject);
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

        [Test]
        public void BossBarrageEmitterStopsFiringAfterSourceHealthDeath()
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
            emitter.ConfigurePattern(pattern, projectilePrefab, pattern.ProjectilesPerWave * 2);

            Assert.IsTrue(emitter.BeginWindup());
            int spawnedBeforeDeath = emitter.FirePendingWave();
            Assert.AreEqual(pattern.ProjectilesPerWave, spawnedBeforeDeath);

            int activeBeforeDeath = emitter.ActiveProjectileCount;
            Assert.Greater(activeBeforeDeath, 0);

            bool died = bossHealth.TryApplyDamage(new DamageInfo(null, DamageTeam.Player, 9999f, Vector3.zero, Vector3.zero, 0f));
            Assert.IsTrue(died);
            Assert.IsFalse(bossHealth.IsAlive);

            for (int i = 0; i < 20; i++)
            {
                emitter.Tick(0.25f);
            }

            Assert.AreEqual(0, emitter.ActiveProjectileCount);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(pattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossBarrageEmitterDisablesWindupAndClearsActiveProjectiles()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");

            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            SerializedObject serializedPattern = new SerializedObject(pattern);
            serializedPattern.FindProperty("windupSeconds").floatValue = 0.01f;
            serializedPattern.FindProperty("waveIntervalSeconds").floatValue = 0f;
            serializedPattern.FindProperty("initialDelaySeconds").floatValue = 0.2f;
            serializedPattern.ApplyModifiedPropertiesWithoutUndo();

            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, null);
            emitter.ConfigurePattern(pattern, projectilePrefab, pattern.ProjectilesPerWave * 2);

            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            Assert.Greater(emitter.ActiveProjectileCount, 0);

            emitter.SetFiringEnabled(false);
            Assert.IsFalse(emitter.IsFiringEnabled);
            Assert.IsFalse(emitter.IsWindupActive);
            Assert.AreEqual(0, emitter.ActiveProjectileCount, "Disable should clear all currently active boss projectiles.");

            emitter.SetFiringEnabled(true);
            for (int i = 0; i < 15; i++)
            {
                emitter.Tick(0.01f);
            }

            Assert.AreEqual(0, emitter.ActiveProjectileCount, "Re-enable should respect pattern delay before re-arming barrage.");

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(pattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }
    }
}
