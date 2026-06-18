using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
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
            director.ConfigureReferences(bossCost, emitter);
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
                    0f)
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
