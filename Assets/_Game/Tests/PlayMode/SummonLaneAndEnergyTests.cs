using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.AI;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class SummonLaneAndEnergyTests
    {
        private const string ShieldBreakerEliteAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/RoleVariants/ShieldBreakerElite/DB_ShieldBreakerElite_Role.controller";
        private const string CanonicalBossAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/SciFiSoldier01/DB_SciFiSoldier01_GeneralDeck.controller";
        private const string HitOnlyBossAnimatorControllerPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller";
        private const string MissingHitAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";

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
            float backlineGainMultiplier = energy.CurrentGainMultiplier;
            SummonEnergyRiskBand backlineRiskBand = energy.CurrentRiskBand;

            energy.ResetLadder();
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            energy.Tick(1f);
            float forwardEnergy = energy.CurrentTierEnergy;
            float forwardGainMultiplier = energy.CurrentGainMultiplier;
            SummonEnergyRiskBand forwardRiskBand = energy.CurrentRiskBand;

            Assert.Greater(
                forwardEnergy,
                backlineEnergy * 8f,
                "Forward-risk positioning should charge summon energy dramatically faster than the backline.");
            Assert.AreEqual(
                SummonEnergyRiskBand.BackSafety,
                backlineRiskBand,
                "Backline positioning should be classified as safe, slow EN farming.");
            Assert.AreEqual(
                SummonEnergyRiskBand.ForwardRisk,
                forwardRiskBand,
                "Forward boundary positioning should be classified as the high-gain risk band.");
            Assert.Less(
                backlineGainMultiplier,
                0.35f,
                "Backline EN gain should be slow enough that hiding does not refill summons quickly.");
            Assert.Greater(
                forwardGainMultiplier,
                2f,
                "Forward-risk EN gain should be visually obvious within a short demo beat.");

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
        public void EnergyLadderKeepsRiskBandCurrentWhileGainIsDisabled()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
            energy.ConfigureReferences(lane, playerObject.transform);

            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            energy.Tick(1f);
            float manaBeforeDisable = energy.CurrentMana;
            energy.SetGainEnabled(false);
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            energy.Tick(1f);

            Assert.AreEqual(SummonEnergyRiskBand.ForwardRisk, energy.CurrentRiskBand);
            Assert.AreEqual(manaBeforeDisable, energy.CurrentMana, 0.001f);

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
        public void EnergyLadderCanSpendSummonCostWithoutResettingSharedManaBank()
        {
            GameObject playerObject = new GameObject("Player");
            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();

            energy.GrantCurrentTierEnergy(300f);

            Assert.AreEqual(3, energy.AvailableTier);
            Assert.AreEqual(300f, energy.CurrentMana, 0.001f);
            int changeCount = 0;
            int spentEventTier = 0;
            energy.EnergyChanged += () => changeCount++;
            energy.EnergySpent += tier => spentEventTier = tier;

            Assert.IsTrue(energy.TrySpend(100f, out int spentTier));
            Assert.AreEqual(1, spentTier, "A costed 100 EN summon should spend the low-cost role tier even from a full bank.");
            Assert.AreEqual(1, changeCount, "Shared mana spend should notify presentation/fill listeners.");
            Assert.AreEqual(1, spentEventTier, "Shared mana spend should emit the cost tier for ready/spend cues.");
            Assert.AreEqual(200f, energy.CurrentMana, 0.001f);
            Assert.AreEqual(2, energy.AvailableTier, "Spending a 100-cost summon from a full bank should leave LV2 mana ready.");
            Assert.AreEqual(3, energy.ChargingTier);
            Assert.AreEqual(0f, energy.CurrentTierEnergy, 0.001f);

            Assert.IsFalse(energy.TrySpend(300f, out int blockedTier));
            Assert.AreEqual(0, blockedTier);
            Assert.AreEqual(1, changeCount, "A blocked spend should not emit a new energy change.");
            Assert.AreEqual(200f, energy.CurrentMana, 0.001f);

            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void MobileHudSummonReadoutsSeparateSharedManaCostAndSlotCooldown()
        {
            GameObject playerObject = new GameObject("Player");
            try
            {
                SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
                PlayerSummonSlot1Action slot1 = playerObject.AddComponent<PlayerSummonSlot1Action>();
                PlayerSupportSummonSlotAction slot2 = playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerSupportSummonSlotAction slot3 = playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                slot1.ConfigureRequiredSummonMana(100f);
                slot1.ConfigureSlotCooldown(1.25f);
                slot2.ConfigureRequiredSummonMana(200f);
                slot2.ConfigureMinimumSummonTier(2);
                slot2.ConfigureSlotCooldown(1.5f);
                slot3.ConfigureRequiredSummonMana(300f);
                slot3.ConfigureMinimumSummonTier(3);
                slot3.ConfigureSlotCooldown(1.5f);

                energy.GrantCurrentTierEnergy(100f);

                StringAssert.Contains(
                    "READY LV1",
                    CombatHudSummonReadoutFormatter.BuildPrimarySummonLabel(
                        BossBarrageSummonBalance.Slot1HudLabel,
                        energy,
                        slot1));
                StringAssert.Contains(
                    "NEED +100 EN",
                    CombatHudSummonReadoutFormatter.BuildSupportSummonLabel(
                        slot2,
                        BossBarrageSummonBalance.Slot2HudLabel,
                        BossBarrageSummonBalance.LockedSummonLabel,
                        energy));
                StringAssert.Contains(
                    "NEED +200 EN",
                    CombatHudSummonReadoutFormatter.BuildSupportSummonLabel(
                        slot3,
                        BossBarrageSummonBalance.Slot3HudLabel,
                        BossBarrageSummonBalance.LockedSummonLabel,
                        energy));
                Assert.AreEqual(
                    1f,
                    CombatHudSummonReadoutFormatter.ResolvePrimarySummonFill01(energy, slot1),
                    0.001f);
                Assert.AreEqual(
                    0.5f,
                    CombatHudSummonReadoutFormatter.ResolveSupportSummonFill01(energy, slot2),
                    0.001f);
                Assert.AreEqual(
                    1f / 3f,
                    CombatHudSummonReadoutFormatter.ResolveSupportSummonFill01(energy, slot3),
                    0.001f);

                energy.GrantCurrentTierEnergy(100f);

                StringAssert.Contains(
                    "READY LV2",
                    CombatHudSummonReadoutFormatter.BuildSupportSummonLabel(
                        slot2,
                        BossBarrageSummonBalance.Slot2HudLabel,
                        BossBarrageSummonBalance.LockedSummonLabel,
                        energy));
                StringAssert.Contains(
                    "NEED +100 EN",
                    CombatHudSummonReadoutFormatter.BuildSupportSummonLabel(
                        slot3,
                        BossBarrageSummonBalance.Slot3HudLabel,
                        BossBarrageSummonBalance.LockedSummonLabel,
                        energy));
                Assert.AreEqual(
                    1f,
                    CombatHudSummonReadoutFormatter.ResolveSupportSummonFill01(energy, slot2),
                    0.001f);
                Assert.AreEqual(
                    2f / 3f,
                    CombatHudSummonReadoutFormatter.ResolveSupportSummonFill01(energy, slot3),
                    0.001f);

                SetPrivateInstanceField(slot2, "slotCooldownRemaining", 0.8f);

                StringAssert.Contains(
                    "CD 0.8s",
                    CombatHudSummonReadoutFormatter.BuildSupportSummonLabel(
                        slot2,
                        BossBarrageSummonBalance.Slot2HudLabel,
                        BossBarrageSummonBalance.LockedSummonLabel,
                        energy));
                Assert.AreEqual(
                    1f - 0.8f / 1.5f,
                    CombatHudSummonReadoutFormatter.ResolveSupportSummonFill01(energy, slot2),
                    0.001f);
                StringAssert.Contains(
                    "NEED +100 EN",
                    CombatHudSummonReadoutFormatter.BuildSupportSummonLabel(
                        slot3,
                        BossBarrageSummonBalance.Slot3HudLabel,
                        BossBarrageSummonBalance.LockedSummonLabel,
                        energy),
                    "Slot2 cooldown must not make Slot3 look globally locked or ready.");
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void RouteIncentiveForecastsSupportHoldTradeoff()
        {
            GameObject playerObject = new GameObject("Player");
            GameObject ownerObject = new GameObject("PocketOwner");
            try
            {
                SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
                PlayerSupportSummonSlotAction slot2 = playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerSupportSummonSlotAction slot3 = playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                slot2.ConfigureRequiredSummonMana(200f);
                slot2.ConfigureMinimumSummonTier(2);
                slot3.ConfigureRequiredSummonMana(300f);
                slot3.ConfigureMinimumSummonTier(3);

                BossBarrageEncounterController owner = ownerObject.AddComponent<BossBarrageEncounterController>();
                SetPrivateInstanceField(owner, "energyLadder", energy);
                SetPrivateInstanceField(owner, "summonSlot2Action", slot2);
                SetPrivateInstanceField(owner, "summonSlot3Action", slot3);
                SetPrivateInstanceField(owner, "closeThreatDefeated", true);

                energy.GrantCurrentTierEnergy(200f);

                Assert.That(owner.RouteIncentiveCue, Does.Contain("Summon cover"));
                Assert.That(owner.RouteIncentiveCue, Does.Contain("S2 ready now"));
                Assert.That(owner.RouteIncentiveCue, Does.Contain("hold 300 EN"));
                Assert.That(owner.RouteIncentiveCue, Does.Contain("S3 suppress"));

                energy.GrantCurrentTierEnergy(100f);

                Assert.That(owner.RouteIncentiveCue, Does.Contain("S2 ready for tempo"));
                Assert.That(owner.RouteIncentiveCue, Does.Contain("S3 ready for suppress"));
                Assert.That(owner.RouteIncentiveCue, Does.Contain("Slot1 recharge"));
            }
            finally
            {
                Object.DestroyImmediate(ownerObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void SummonEnergyVfxCuePresenterPlaysForwardRiskReadyAndSpendReads()
        {
            GameObject laneObject = new GameObject("Lane");
            GameObject playerObject = new GameObject("Player");
            GameObject cuePrefab = new GameObject("EnergyStateCuePrefab");
            CombatVfxCueProfile cueProfile = null;
            try
            {
                SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
                SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
                energy.ConfigureReferences(lane, playerObject.transform);

                CombatVfxCuePlayer cuePlayer = playerObject.AddComponent<CombatVfxCuePlayer>();
                cueProfile = CreateEnergyVfxCueProfile(cuePrefab);
                ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);

                SummonEnergyVfxCuePresenter presenter = playerObject.AddComponent<SummonEnergyVfxCuePresenter>();
                presenter.Configure(energy, cuePlayer, playerObject.transform, null);

                playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
                energy.Tick(0.01f);

                Assert.AreEqual(
                    1,
                    presenter.ForwardRiskCueRequestCount,
                    "Entering the forward-risk EN band should create a visible player-side state cue through RiskBandChanged without frame polling.");

                energy.GrantCurrentTierEnergy(energy.CurrentTierTarget);

                Assert.AreEqual(
                    1,
                    presenter.TierReadyCueRequestCount,
                    "Opening an EN spend tier should create a visible ready cue at the player.");
                Assert.AreEqual(1, presenter.LastReadyTier);

                Assert.IsTrue(energy.TrySpend(out int spentTier));

                Assert.AreEqual(1, spentTier);
                Assert.AreEqual(
                    1,
                    presenter.SpendCueRequestCount,
                    "Spending EN should create a visible reset/spend cue at the player.");
                Assert.AreEqual(1, presenter.LastSpentTier);
                Assert.IsNotNull(
                    playerObject.transform.Find(cuePrefab.name),
                    "Energy state cues should be spawned through CombatVfxCuePlayer, not only counted in code.");
            }
            finally
            {
                if (cueProfile != null)
                {
                    Object.DestroyImmediate(cueProfile);
                }

                Object.DestroyImmediate(cuePrefab);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(laneObject);
            }
        }

        [Test]
        public void ActionScreenCuePresenterReadsSummonEnergyStateSignals()
        {
            GameObject laneObject = new GameObject("Lane");
            GameObject playerObject = new GameObject("Player");
            GameObject presenterObject = new GameObject("ScreenCuePresenter");
            try
            {
                SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
                SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
                energy.ConfigureReferences(lane, playerObject.transform);

                ActionScreenCuePresenter presenter = presenterObject.AddComponent<ActionScreenCuePresenter>();
                presenter.Configure(null, null, null, energy, null, null, null, null, null, null, null);

                playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
                energy.Tick(0.01f);

                Assert.AreEqual("Energy.ForwardRisk", presenter.LastCueId);
                Assert.AreEqual(1, presenter.EnergyCueRequestCount);
                Assert.AreEqual(1, presenter.ForwardRiskCueRequestCount);
                Assert.AreEqual(SummonEnergyRiskBand.ForwardRisk, presenter.LastEnergyRiskBand);

                energy.GrantCurrentTierEnergy(energy.CurrentTierTarget);

                Assert.AreEqual("Energy.ReadyLV1", presenter.LastCueId);
                Assert.AreEqual(2, presenter.EnergyCueRequestCount);
                Assert.AreEqual(1, presenter.EnergyReadyCueRequestCount);
                Assert.AreEqual(1, presenter.LastEnergyCueTier);

                Assert.IsTrue(energy.TrySpend(out int spentTier));

                Assert.AreEqual(1, spentTier);
                Assert.AreEqual("Energy.SpentLV1", presenter.LastCueId);
                Assert.AreEqual(3, presenter.EnergyCueRequestCount);
                Assert.AreEqual(1, presenter.EnergySpendCueRequestCount);
                Assert.AreEqual(3, presenter.PlayerCueRequestCount);
            }
            finally
            {
                Object.DestroyImmediate(presenterObject);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(laneObject);
            }
        }

        [Test]
        public void ActionScreenCuePresenterReadsPlayerDamageSignal()
        {
            GameObject playerObject = new GameObject("Player");
            GameObject presenterObject = new GameObject("ScreenCuePresenter");
            try
            {
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                playerHealth.ConfigureMaxHealth(120f);

                ActionScreenCuePresenter presenter = presenterObject.AddComponent<ActionScreenCuePresenter>();
                presenter.Configure(null, playerHealth, null, null, null, null, null, null, null, null, null);

                Assert.IsTrue(playerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    36f,
                    playerObject.transform.position,
                    Vector3.back,
                    0f)));

                Assert.AreEqual("Player.Damaged", presenter.LastCueId);
                Assert.AreEqual(1, presenter.PlayerDamageCueRequestCount);
                Assert.AreEqual(1, presenter.DamageFeedbackRequestCount);
                Assert.AreEqual(1, presenter.PlayerCueRequestCount);
                Assert.Greater(presenter.LastCueIntensity, 0.74f);
                Assert.IsTrue(presenter.HasActiveDamageFeedback);
                Assert.Greater(presenter.LastDamageFeedbackIntensity, 0.5f);
                Assert.Greater(presenter.LastDamageFeedbackDuration, presenter.DamageVignetteSeconds);
                Assert.AreEqual(DamageResponsePolicy.Default, presenter.LastDamageResponsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.InterruptAction, presenter.LastDamageControlLockPolicy);
                Assert.IsTrue(presenter.LastDamageFeedbackInterruptedAction);
                Assert.AreEqual(1f, presenter.LastDamageFeedbackPolicyScale, 0.001f);
                Assert.Greater(presenter.LastDamageScreenDirection.y, 0.9f);
            }
            finally
            {
                Object.DestroyImmediate(presenterObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void ActionScreenCuePresenterSoftensPressureDamageWithoutControlLock()
        {
            GameObject playerObject = new GameObject("Player");
            GameObject presenterObject = new GameObject("ScreenCuePresenter");
            try
            {
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                playerHealth.ConfigureMaxHealth(120f);

                ActionScreenCuePresenter presenter = presenterObject.AddComponent<ActionScreenCuePresenter>();
                presenter.Configure(null, playerHealth, null, null, null, null, null, null, null, null, null);

                Assert.IsTrue(playerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    36f,
                    playerObject.transform.position,
                    Vector3.back,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None)));

                float expectedDuration =
                    (presenter.DamageVignetteSeconds + presenter.HeavyDamageExtraSeconds)
                    * presenter.PressureDamageFeedbackScale;
                float expectedIntensity = Mathf.Clamp01((0.54f + 0.30f) * presenter.PressureDamageFeedbackScale);

                Assert.AreEqual("Player.Damaged", presenter.LastCueId);
                Assert.AreEqual(1, presenter.PlayerDamageCueRequestCount);
                Assert.AreEqual(1, presenter.DamageFeedbackRequestCount);
                Assert.AreEqual(DamageResponsePolicy.FlashOnly, presenter.LastDamageResponsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.None, presenter.LastDamageControlLockPolicy);
                Assert.IsFalse(presenter.LastDamageFeedbackInterruptedAction);
                Assert.AreEqual(presenter.PressureDamageFeedbackScale, presenter.LastDamageFeedbackPolicyScale, 0.001f);
                Assert.AreEqual(expectedDuration, presenter.LastDamageFeedbackDuration, 0.001f);
                Assert.AreEqual(expectedIntensity, presenter.LastDamageFeedbackIntensity, 0.001f);
                Assert.Less(presenter.LastDamageFeedbackDuration, presenter.DamageVignetteSeconds);
            }
            finally
            {
                Object.DestroyImmediate(presenterObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void PlayerCombatVfxCueDriverPlaysDamageAndCriticalStateCues()
        {
            GameObject playerObject = new GameObject("Player");
            GameObject damagedCuePrefab = new GameObject("PlayerDamagedCuePrefab");
            GameObject criticalCuePrefab = new GameObject("PlayerCriticalCuePrefab");
            CombatVfxCueProfile cueProfile = null;
            try
            {
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                playerHealth.ConfigureMaxHealth(100f);

                CombatVfxCuePlayer cuePlayer = playerObject.AddComponent<CombatVfxCuePlayer>();
                cueProfile = CreatePlayerDamageVfxCueProfile(damagedCuePrefab, criticalCuePrefab);
                ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);

                PlayerCombatVfxCueDriver driver = playerObject.AddComponent<PlayerCombatVfxCueDriver>();
                driver.ConfigureDamageFeedback(playerHealth, playerObject.transform);

                Assert.IsTrue(playerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    40f,
                    playerObject.transform.position,
                    Vector3.back,
                    0f)));

                Assert.AreEqual(0, driver.DamageVfxCueRequestCount);
                Assert.AreEqual(0, driver.CriticalVfxCueRequestCount);
                Assert.AreEqual(DamageResponsePolicy.Default, driver.LastDamageResponsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.InterruptAction, driver.LastDamageControlLockPolicy);
                Assert.IsTrue(driver.LastDamageCueInterruptedAction);
                Assert.AreEqual(1f, driver.LastDamageCuePolicyScale, 0.001f);
                Assert.AreEqual(1.14f, driver.LastDamageCueIntensity, 0.001f);
                Assert.IsNull(
                    playerObject.transform.Find(damagedCuePrefab.name),
                    "Player damage VFX should stay suppressed during the temporary VFX cleanup pass.");

                Assert.IsTrue(playerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    30f,
                    playerObject.transform.position,
                    Vector3.back,
                    0f)));

                Assert.AreEqual(0, driver.DamageVfxCueRequestCount);
                Assert.AreEqual(0, driver.CriticalVfxCueRequestCount);
                Assert.IsNull(
                    playerObject.transform.Find(criticalCuePrefab.name),
                    "Critical player damage VFX should stay suppressed during the temporary VFX cleanup pass.");
            }
            finally
            {
                if (cueProfile != null)
                {
                    Object.DestroyImmediate(cueProfile);
                }

                Object.DestroyImmediate(criticalCuePrefab);
                Object.DestroyImmediate(damagedCuePrefab);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void ReviewedCombatFeedbackProfileAllowsHitFeedbackCues()
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            try
            {
                SerializedObject serializedProfile = new SerializedObject(profile);
                serializedProfile.FindProperty("playbackMode").enumValueIndex =
                    (int)CombatVfxCuePlaybackMode.ReviewedCombatFeedbackOnly;
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();

                Assert.IsTrue(profile.AllowsPlayback(CombatVfxCueId.PlayerRangedMuzzleFlash));
                Assert.IsTrue(profile.AllowsPlayback(CombatVfxCueId.PlayerRangedProjectileImpact));
                Assert.IsTrue(profile.AllowsPlayback(CombatVfxCueId.PlayerDamaged));
                Assert.IsTrue(profile.AllowsPlayback(CombatVfxCueId.PlayerCritical));
                Assert.IsTrue(profile.AllowsPlayback(CombatVfxCueId.EnemyHit));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void PlayerCombatVfxCueDriverSoftensNonLockingPressureDamage()
        {
            GameObject playerObject = new GameObject("Player");
            GameObject damagedCuePrefab = new GameObject("PlayerDamagedCuePrefab");
            GameObject criticalCuePrefab = new GameObject("PlayerCriticalCuePrefab");
            CombatVfxCueProfile cueProfile = null;
            try
            {
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                playerHealth.ConfigureMaxHealth(100f);

                CombatVfxCuePlayer cuePlayer = playerObject.AddComponent<CombatVfxCuePlayer>();
                cueProfile = CreatePlayerDamageVfxCueProfile(damagedCuePrefab, criticalCuePrefab);
                ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);

                PlayerCombatVfxCueDriver driver = playerObject.AddComponent<PlayerCombatVfxCueDriver>();
                driver.ConfigureDamageFeedback(playerHealth, playerObject.transform);

                Assert.IsTrue(playerHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    40f,
                    playerObject.transform.position,
                    Vector3.back,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None)));

                float expectedIntensity = (1f + 0.4f * 0.35f) * driver.PressureDamageCueScale;
                Transform damageCue = playerObject.transform.Find(damagedCuePrefab.name);

                Assert.AreEqual(0, driver.DamageVfxCueRequestCount);
                Assert.AreEqual(0, driver.CriticalVfxCueRequestCount);
                Assert.AreEqual(DamageResponsePolicy.FlashOnly, driver.LastDamageResponsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.None, driver.LastDamageControlLockPolicy);
                Assert.IsFalse(driver.LastDamageCueInterruptedAction);
                Assert.AreEqual(driver.PressureDamageCueScale, driver.LastDamageCuePolicyScale, 0.001f);
                Assert.AreEqual(expectedIntensity, driver.LastDamageCueIntensity, 0.001f);
                Assert.IsNull(damageCue);
            }
            finally
            {
                if (cueProfile != null)
                {
                    Object.DestroyImmediate(cueProfile);
                }

                Object.DestroyImmediate(criticalCuePrefab);
                Object.DestroyImmediate(damagedCuePrefab);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void EnemyCombatVfxCueDriverSoftensNonLockingPressureDamage()
        {
            GameObject enemyObject = new GameObject("Enemy");
            GameObject damageCuePrefab = new GameObject("EnemyHitCuePrefab");
            CombatVfxCueProfile cueProfile = null;
            try
            {
                CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
                enemyHealth.ConfigureTeam(DamageTeam.Enemy);
                enemyHealth.ResetHealthToFull();

                CombatVfxCuePlayer cuePlayer = enemyObject.AddComponent<CombatVfxCuePlayer>();
                cueProfile = CreateSummonDamageVfxCueProfile(damageCuePrefab);
                ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);

                EnemyCombatVfxCueDriver driver = enemyObject.AddComponent<EnemyCombatVfxCueDriver>();

                Assert.IsTrue(enemyHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    18f,
                    enemyObject.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None)));

                float expectedIntensity = driver.DamageCueIntensity * driver.PressureDamageCueScale;
                Transform damageCue = enemyObject.transform.Find(damageCuePrefab.name);

                Assert.AreEqual(0, driver.DamageVfxCueRequestCount);
                Assert.AreEqual(DamageResponsePolicy.FlashOnly, driver.LastDamageResponsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.None, driver.LastDamageControlLockPolicy);
                Assert.IsFalse(driver.LastDamageCueInterruptedAction);
                Assert.AreEqual(driver.PressureDamageCueScale, driver.LastDamageCuePolicyScale, 0.001f);
                Assert.AreEqual(expectedIntensity, driver.LastDamageCueIntensity, 0.001f);
                Assert.IsNull(damageCue);
            }
            finally
            {
                if (cueProfile != null)
                {
                    Object.DestroyImmediate(cueProfile);
                }

                Object.DestroyImmediate(damageCuePrefab);
                Object.DestroyImmediate(enemyObject);
            }
        }

        [Test]
        public void CombatHitFeedbackCountsEnemyBodyFlashOnDamage()
        {
            GameObject enemyObject = new GameObject("Enemy");
            GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Material bodyMaterial = null;
            try
            {
                bodyObject.name = "EnemyBody";
                bodyObject.transform.SetParent(enemyObject.transform, worldPositionStays: false);
                Renderer bodyRenderer = bodyObject.GetComponent<Renderer>();
                Assert.IsNotNull(bodyRenderer);
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                bodyMaterial = new Material(shader);
                bodyMaterial.SetColor("_BaseColor", new Color(0.18f, 0.24f, 0.32f, 1f));
                bodyMaterial.SetColor("_Color", new Color(0.18f, 0.24f, 0.32f, 1f));
                bodyRenderer.sharedMaterial = bodyMaterial;

                CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
                enemyHealth.ConfigureTeam(DamageTeam.Enemy);
                enemyHealth.ConfigureMaxHealth(100f);

                CombatHitFeedback feedback = enemyObject.AddComponent<CombatHitFeedback>();
                feedback.enabled = false;
                SerializedObject serializedFeedback = new SerializedObject(feedback);
                serializedFeedback.FindProperty("health").objectReferenceValue = enemyHealth;
                SerializedProperty flashRenderers = serializedFeedback.FindProperty("flashRenderers");
                flashRenderers.arraySize = 1;
                flashRenderers.GetArrayElementAtIndex(0).objectReferenceValue = bodyRenderer;
                serializedFeedback.FindProperty("renderHitFeedback").boolValue = true;
                serializedFeedback.FindProperty("applyIdleColorOnEnable").boolValue = false;
                serializedFeedback.ApplyModifiedPropertiesWithoutUndo();
                feedback.enabled = true;

                Assert.AreEqual(0, feedback.DamageFlashCount);
                Assert.AreEqual(1, feedback.FlashRendererCount);

                Assert.IsTrue(enemyHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    12f,
                    enemyObject.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None)));

                Assert.AreEqual(
                    1,
                    feedback.DamageFlashCount,
                    "Enemy body shader feedback should react to non-locking hit presentation events.");
                MaterialPropertyBlock appliedBlock = new MaterialPropertyBlock();
                bodyRenderer.GetPropertyBlock(appliedBlock);
                Color appliedColor = appliedBlock.GetColor(Shader.PropertyToID("_Color"));
                Assert.Greater(
                    appliedColor.g,
                    0.45f,
                    "Enemy body shader feedback should push the body toward a clearly visible warm flash, not a barely changed base color.");

                Assert.IsTrue(enemyHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    4f,
                    enemyObject.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)));

                Assert.AreEqual(
                    1,
                    feedback.DamageFlashCount,
                    "DamageOnly events should not request body hit flash presentation.");
            }
            finally
            {
                if (bodyMaterial != null)
                {
                    Object.DestroyImmediate(bodyMaterial);
                }

                Object.DestroyImmediate(enemyObject);
            }
        }

        [Test]
        public void ActionCameraControllerTracksMostRecentlyEnabledInstance()
        {
            ActionCameraController previousController = ActionCameraController.ActiveInstance;
            GameObject firstCameraObject = new GameObject("FirstActionCamera");
            GameObject secondCameraObject = new GameObject("SecondActionCamera");
            try
            {
                firstCameraObject.AddComponent<Camera>();
                ActionCameraController firstController =
                    firstCameraObject.AddComponent<ActionCameraController>();
                Assert.AreSame(firstController, ActionCameraController.ActiveInstance);

                secondCameraObject.AddComponent<Camera>();
                ActionCameraController secondController =
                    secondCameraObject.AddComponent<ActionCameraController>();
                Assert.AreSame(secondController, ActionCameraController.ActiveInstance);

                secondCameraObject.SetActive(false);
                Assert.AreSame(firstController, ActionCameraController.ActiveInstance);

                firstCameraObject.SetActive(false);
                Assert.AreSame(previousController, ActionCameraController.ActiveInstance);
            }
            finally
            {
                Object.DestroyImmediate(secondCameraObject);
                Object.DestroyImmediate(firstCameraObject);
            }
        }

        [Test]
        public void CombatHitFeedbackRequestsCameraRecoilAndHitStopForHeavyHit()
        {
            GameObject enemyObject = new GameObject("Enemy");
            GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            GameObject cameraObject = new GameObject("ActionCamera");
            Material bodyMaterial = null;
            float previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 1f;
                bodyObject.name = "EnemyBody";
                bodyObject.transform.SetParent(enemyObject.transform, worldPositionStays: false);
                Renderer bodyRenderer = bodyObject.GetComponent<Renderer>();
                Assert.IsNotNull(bodyRenderer);
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                bodyMaterial = new Material(shader);
                bodyMaterial.SetColor("_BaseColor", new Color(0.18f, 0.24f, 0.32f, 1f));
                bodyRenderer.sharedMaterial = bodyMaterial;

                Camera camera = cameraObject.AddComponent<Camera>();
                Assert.IsNotNull(camera);
                ActionCameraController cameraController = cameraObject.AddComponent<ActionCameraController>();

                CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
                enemyHealth.ConfigureTeam(DamageTeam.Enemy);
                enemyHealth.ConfigureMaxHealth(100f);

                CombatHitFeedback feedback = enemyObject.AddComponent<CombatHitFeedback>();
                feedback.enabled = false;
                SerializedObject serializedFeedback = new SerializedObject(feedback);
                serializedFeedback.FindProperty("health").objectReferenceValue = enemyHealth;
                serializedFeedback.FindProperty("cameraController").objectReferenceValue = cameraController;
                serializedFeedback.FindProperty("visualRecoilRoot").objectReferenceValue = bodyObject.transform;
                SerializedProperty flashRenderers = serializedFeedback.FindProperty("flashRenderers");
                flashRenderers.arraySize = 1;
                flashRenderers.GetArrayElementAtIndex(0).objectReferenceValue = bodyRenderer;
                serializedFeedback.FindProperty("renderHitFeedback").boolValue = true;
                serializedFeedback.FindProperty("applyIdleColorOnEnable").boolValue = false;
                serializedFeedback.ApplyModifiedPropertiesWithoutUndo();
                feedback.enabled = true;

                Assert.IsTrue(enemyHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    18f,
                    enemyObject.transform.position,
                    Vector3.forward,
                    0.04f,
                    DamageResponsePolicy.Stagger,
                    CombatControlLockPolicy.InterruptAction)));

                Assert.AreEqual(CombatHitFeedbackTier.Heavy, feedback.LastHitFeedbackTier);
                Assert.AreEqual(1, feedback.CameraImpulseRequestCount);
                Assert.AreEqual(1, feedback.VisualRecoilRequestCount);
                Assert.AreEqual(1, feedback.HitStopRequestCount);
                Assert.IsTrue(cameraController.HasActiveCue);
                Assert.AreEqual(1, cameraController.MicroShakeRequestCount);
                Assert.IsTrue(cameraController.HasActiveMicroShake);
                Assert.AreNotEqual(Vector3.zero, bodyObject.transform.localPosition);
                Assert.Less(Time.timeScale, 1f);
            }
            finally
            {
                Time.timeScale = previousTimeScale;

                if (bodyMaterial != null)
                {
                    Object.DestroyImmediate(bodyMaterial);
                }

                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(enemyObject);
            }
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
        public void CombatResourceReadoutReportsHealthEnergyAndBossCostForHud()
        {
            GameObject playerObject = new GameObject("Player");
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            playerHealth.ConfigureMaxHealth(200f);
            playerHealth.TryApplyDamage(new DamageInfo(null, DamageTeam.Enemy, 50f, Vector3.zero, Vector3.back, 0f));

            CombatResourceReadout healthReadout =
                CombatResourceReadout.FromHealth("Player HP", playerHealth, Color.green);
            Assert.AreEqual("Player HP", healthReadout.Label);
            Assert.AreEqual("150/200", healthReadout.ValueText);
            Assert.AreEqual("alive", healthReadout.StateText);
            Assert.AreEqual(0.75f, healthReadout.Fill01, 0.001f);
            StringAssert.Contains("Player HP", healthReadout.Line);

            CombatResourceReadout survivalReadout =
                CombatResourceReadout.FromSurvivalHealth("Player HP", playerHealth, Color.green);
            Assert.AreEqual("stable", survivalReadout.StateText);
            StringAssert.Contains("stable", survivalReadout.Line);

            playerHealth.TryApplyDamage(new DamageInfo(null, DamageTeam.Enemy, 40f, Vector3.zero, Vector3.back, 0f));
            survivalReadout = CombatResourceReadout.FromSurvivalHealth("Player HP", playerHealth, Color.green);
            Assert.AreEqual("pressured", survivalReadout.StateText);
            Assert.AreEqual(0.55f, survivalReadout.Fill01, 0.001f);

            playerHealth.TryApplyDamage(new DamageInfo(null, DamageTeam.Enemy, 50f, Vector3.zero, Vector3.back, 0f));
            survivalReadout = CombatResourceReadout.FromSurvivalHealth("Player HP", playerHealth, Color.green);
            Assert.AreEqual("critical", survivalReadout.StateText);
            Assert.AreEqual(0.3f, survivalReadout.Fill01, 0.001f);

            SummonEnergyLadder energy = playerObject.AddComponent<SummonEnergyLadder>();
            energy.GrantCurrentTierEnergy(100f);
            CombatResourceReadout energyReadout = CombatResourceReadout.FromEnergy("Player EN", energy);
            Assert.AreEqual("Player EN", energyReadout.Label);
            Assert.AreEqual("100/300 EN", energyReadout.ValueText);
            Assert.AreEqual("READY LV1", energyReadout.StateText);
            Assert.AreEqual(1f / 3f, energyReadout.Fill01, 0.001f);
            Assert.IsTrue(energyReadout.IsReady);

            GameObject bossObject = new GameObject("Boss");
            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.GrantCurrentTierCost(200f);
            CombatResourceReadout costReadout = CombatResourceReadout.FromBossCost("Boss Cost", bossCost);
            Assert.AreEqual("Boss Cost", costReadout.Label);
            Assert.AreEqual("LV3 0%", costReadout.ValueText);
            Assert.AreEqual("READY LV2", costReadout.StateText);
            Assert.AreEqual(0f, costReadout.Fill01, 0.001f);
            Assert.IsTrue(costReadout.IsReady);

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void ActionScreenCuePresenterKeepsFollowupCueReadableAgainstLowerPrioritySpam()
        {
            GameObject presenterObject = new GameObject("ScreenCuePresenter");
            ActionScreenCuePresenter presenter = presenterObject.AddComponent<ActionScreenCuePresenter>();

            RequestScreenCueForTest(presenter, "Followup.Window", Color.green, 0.24f, 1f, "Followup");
            RequestScreenCueForTest(presenter, "Boss.Fire", Color.red, 0.16f, 1f, "Boss");
            RequestScreenCueForTest(presenter, "Player.LowPriorityPulse", Color.cyan, 0.09f, 0.42f, "Player");

            Assert.AreEqual("Followup.Window", presenter.LastCueId);
            Assert.AreEqual(1, presenter.CueRequestCount);
            Assert.AreEqual(1, presenter.FollowupCueRequestCount);
            Assert.AreEqual(0, presenter.BossCueRequestCount);
            Assert.AreEqual(0, presenter.PlayerCueRequestCount);
            Assert.AreEqual(2, presenter.SuppressedCueRequestCount);

            RequestScreenCueForTest(presenter, "Followup.Hit", Color.yellow, 0.18f, 1.1f, "Followup");

            Assert.AreEqual("Followup.Hit", presenter.LastCueId);
            Assert.AreEqual(2, presenter.CueRequestCount);
            Assert.AreEqual(2, presenter.FollowupCueRequestCount);
            Assert.AreEqual(2, presenter.SuppressedCueRequestCount);

            Object.DestroyImmediate(presenterObject);
        }

        [Test]
        public void ActionCameraCueDriverRoutesPocketResultCues()
        {
            GameObject cameraObject = new GameObject("PocketResultCamera");
            cameraObject.AddComponent<Camera>();
            ActionCameraController cameraController = cameraObject.AddComponent<ActionCameraController>();
            ActionCameraCueDriver cueDriver = cameraObject.AddComponent<ActionCameraCueDriver>();

            cueDriver.RequestPocketClearCue(3);

            Assert.AreEqual(1, cueDriver.PocketClearCueRequestCount);
            Assert.AreEqual(3, cueDriver.LastPocketClearTier);
            Assert.IsTrue(cameraController.HasActiveCue, "Pocket clear should leave a short action-camera result cue.");

            cueDriver.RequestPocketFailCue(1);

            Assert.AreEqual(1, cueDriver.PocketFailCueRequestCount);
            Assert.AreEqual(1, cueDriver.LastPocketFailTier);
            Assert.IsTrue(cameraController.HasActiveCue, "Pocket failure should also leave a short action-camera result cue.");

            Object.DestroyImmediate(cameraObject);
        }

        [Test]
        public void BossBarragePocketCameraCueBridgeRoutesPocketResultCameraCues()
        {
            GameObject cameraObject = new GameObject("PocketResultBridgeCamera");
            cameraObject.AddComponent<Camera>();
            ActionCameraController cameraController = cameraObject.AddComponent<ActionCameraController>();
            ActionCameraCueDriver cueDriver = cameraObject.AddComponent<ActionCameraCueDriver>();
            GameObject bridgeObject = new GameObject("PocketResultBridge");
            BossBarragePocketCameraCueBridge bridge = bridgeObject.AddComponent<BossBarragePocketCameraCueBridge>();

            SerializedObject serializedBridge = new SerializedObject(bridge);
            serializedBridge.FindProperty("cameraCueDriver").objectReferenceValue = cueDriver;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();

            InvokePocketCameraBridgeHandlerForTest(bridge, "HandlePocketCleared");

            Assert.AreEqual(1, cueDriver.PocketClearCueRequestCount);
            Assert.AreEqual(3, cueDriver.LastPocketClearTier);
            Assert.IsTrue(cameraController.HasActiveCue, "Pocket clear bridge should request the action-camera result cue.");

            InvokePocketCameraBridgeHandlerForTest(bridge, "HandlePocketFailed");

            Assert.AreEqual(1, cueDriver.PocketFailCueRequestCount);
            Assert.AreEqual(1, cueDriver.LastPocketFailTier);
            Assert.IsTrue(cameraController.HasActiveCue, "Pocket failure bridge should request the action-camera result cue.");

            Object.DestroyImmediate(bridgeObject);
            Object.DestroyImmediate(cameraObject);
        }

        [Test]
        public void BossFollowupHitReactionUsesTieredCanonicalAnimatorTriggers()
        {
            GameObject bossObject = new GameObject("BossFollowupReaction");
            Animator animator = bossObject.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CanonicalBossAnimatorControllerPath);
            Assert.IsNotNull(animator.runtimeAnimatorController);
            BossBarrageVisualCueDriver cueDriver = bossObject.AddComponent<BossBarrageVisualCueDriver>();
            cueDriver.ConfigurePresentation(null, animator, bossObject.transform, new Renderer[0]);

            cueDriver.RequestFollowupHitReaction(1, 24f);

            Assert.AreEqual(1, cueDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual(1, cueDriver.LastFollowupHitReactionTier);
            Assert.AreEqual(24f, cueDriver.LastFollowupHitReactionDamage, 0.001f);
            Assert.AreEqual("Hit", cueDriver.LastFollowupHitReactionRequestedTrigger);
            Assert.AreEqual("Hit", cueDriver.LastFollowupHitReactionResolvedTrigger);

            cueDriver.RequestFollowupHitReaction(2, 48f);

            Assert.AreEqual(2, cueDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual(2, cueDriver.LastFollowupHitReactionTier);
            Assert.AreEqual("Hit", cueDriver.LastFollowupHitReactionRequestedTrigger);
            Assert.AreEqual("Hit", cueDriver.LastFollowupHitReactionResolvedTrigger);

            cueDriver.RequestFollowupHitReaction(3, 72f);

            Assert.AreEqual(3, cueDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual(3, cueDriver.LastFollowupHitReactionTier);
            Assert.AreEqual("HitHeavy", cueDriver.LastFollowupHitReactionRequestedTrigger);
            Assert.AreEqual("HitHeavy", cueDriver.LastFollowupHitReactionResolvedTrigger);

            Object.DestroyImmediate(bossObject);
        }

        [Test]
        public void BossFollowupHitReactionRejectsUnconfirmedDamageAndFallsBackToHit()
        {
            GameObject bossObject = new GameObject("BossFollowupFallback");
            Animator animator = bossObject.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(HitOnlyBossAnimatorControllerPath);
            Assert.IsNotNull(animator.runtimeAnimatorController);
            BossBarrageVisualCueDriver cueDriver = bossObject.AddComponent<BossBarrageVisualCueDriver>();
            cueDriver.ConfigurePresentation(null, animator, bossObject.transform, new Renderer[0]);

            cueDriver.RequestFollowupHitReaction(3, 0f);
            cueDriver.RequestFollowupHitReaction(3, -1f);
            cueDriver.RequestFollowupHitReaction(3, float.NaN);

            Assert.AreEqual(0, cueDriver.FollowupHitReactionRequestCount);

            cueDriver.RequestFollowupHitReaction(3, 48f);

            Assert.AreEqual(1, cueDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual("HitHeavy", cueDriver.LastFollowupHitReactionRequestedTrigger);
            Assert.AreEqual("Hit", cueDriver.LastFollowupHitReactionResolvedTrigger);

            GameObject missingTriggerObject = new GameObject("BossFollowupMissingTrigger");
            Animator missingTriggerAnimator = missingTriggerObject.AddComponent<Animator>();
            missingTriggerAnimator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(MissingHitAnimatorControllerPath);
            Assert.IsNotNull(missingTriggerAnimator.runtimeAnimatorController);
            BossBarrageVisualCueDriver missingTriggerDriver =
                missingTriggerObject.AddComponent<BossBarrageVisualCueDriver>();
            missingTriggerDriver.ConfigurePresentation(
                null,
                missingTriggerAnimator,
                missingTriggerObject.transform,
                new Renderer[0]);

            Assert.DoesNotThrow(() => missingTriggerDriver.RequestFollowupHitReaction(3, 48f));
            Assert.AreEqual(1, missingTriggerDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual("HitHeavy", missingTriggerDriver.LastFollowupHitReactionRequestedTrigger);
            Assert.AreEqual(string.Empty, missingTriggerDriver.LastFollowupHitReactionResolvedTrigger);

            GameObject missingAnimatorObject = new GameObject("BossFollowupMissingAnimator");
            BossBarrageVisualCueDriver missingAnimatorDriver =
                missingAnimatorObject.AddComponent<BossBarrageVisualCueDriver>();
            Assert.DoesNotThrow(() => missingAnimatorDriver.RequestFollowupHitReaction(3, 48f));
            Assert.AreEqual(1, missingAnimatorDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual(string.Empty, missingAnimatorDriver.LastFollowupHitReactionResolvedTrigger);

            Object.DestroyImmediate(missingAnimatorObject);
            Object.DestroyImmediate(missingTriggerObject);
            Object.DestroyImmediate(bossObject);
        }

        [Test]
        public void BossFollowupVfxBridgeRoutesConfirmedHitToOneBossReaction()
        {
            GameObject bossObject = new GameObject("BossFollowupBridgeReaction");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            Animator animator = bossObject.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CanonicalBossAnimatorControllerPath);
            BossBarrageVisualCueDriver cueDriver = bossObject.AddComponent<BossBarrageVisualCueDriver>();
            cueDriver.ConfigurePresentation(null, animator, bossObject.transform, new Renderer[0]);

            GameObject bridgeObject = new GameObject("BossFollowupVfxBridge");
            bridgeObject.SetActive(false);
            GameObject cuePrefab = new GameObject("BossFollowupHitCue");
            CombatVfxCueProfile cueProfile = CreateFollowupHitVfxCueProfile(cuePrefab);
            CombatVfxCuePlayer cuePlayer = bridgeObject.AddComponent<CombatVfxCuePlayer>();
            ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);
            BossBarrageEncounterController encounterController =
                bridgeObject.AddComponent<BossBarrageEncounterController>();
            BossBarragePocketVfxCueBridge bridge =
                bridgeObject.AddComponent<BossBarragePocketVfxCueBridge>();
            SerializedObject serializedBridge = new SerializedObject(bridge);
            serializedBridge.FindProperty("encounterController").objectReferenceValue = encounterController;
            serializedBridge.FindProperty("bossVisualCueDriver").objectReferenceValue = cueDriver;
            serializedBridge.FindProperty("cuePlayer").objectReferenceValue = cuePlayer;
            serializedBridge.FindProperty("followupHitAnchor").objectReferenceValue = bossObject.transform;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
            bridgeObject.SetActive(true);

            InvokePocketVfxBridgeFollowupHitHandlerForTest(bridge, 3, 0f);
            InvokePocketVfxBridgeFollowupHitHandlerForTest(bridge, 3, -1f);
            InvokePocketVfxBridgeFollowupHitHandlerForTest(bridge, 3, float.NaN);
            Assert.AreEqual(0, cueDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual(0, bridge.FollowupHitCueRequestCount);

            RaiseEncounterFollowupHitConfirmedForTest(encounterController, 3, 64f);

            Assert.AreEqual(1, cueDriver.FollowupHitReactionRequestCount);
            Assert.AreEqual(3, cueDriver.LastFollowupHitReactionTier);
            Assert.AreEqual("HitHeavy", cueDriver.LastFollowupHitReactionResolvedTrigger);
            Assert.AreEqual(3, bridge.LastFollowupHitTier);
            Assert.AreEqual(64f, bridge.LastFollowupHitDamage, 0.001f);
            Assert.AreEqual(
                1,
                bridge.FollowupHitCueRequestCount,
                "The existing semantic follow-up world cue should remain exactly once per confirmed hit.");
            Assert.AreEqual(1, CountDirectChildrenNamed(bossObject.transform, cuePrefab.name));
            Assert.AreEqual(0, cuePlayer.ActiveProfileAudioSourceCount);

            serializedBridge.Update();
            serializedBridge.FindProperty("cuePlayer").objectReferenceValue = null;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
            RaiseEncounterFollowupHitConfirmedForTest(encounterController, 2, 32f);
            Assert.AreEqual(
                2,
                cueDriver.FollowupHitReactionRequestCount,
                "The event subscription should keep the boss reaction even when optional VFX playback is unavailable.");
            Assert.AreEqual(1, bridge.FollowupHitCueRequestCount);
            Assert.AreEqual(1, CountDirectChildrenNamed(bossObject.transform, cuePrefab.name));

            Assert.IsTrue(bossHealth.TryApplyDamage(
                new DamageInfo(null, DamageTeam.Player, 1f, bossObject.transform.position, Vector3.forward, 0f)));
            Assert.Greater(cueDriver.LastDamageCueIntensity, 0f);
            Assert.AreEqual(
                2,
                cueDriver.FollowupHitReactionRequestCount,
                "Generic damage feedback must not duplicate the semantic follow-up reaction.");

            Object.DestroyImmediate(cueProfile);
            Object.DestroyImmediate(cuePrefab);
            Object.DestroyImmediate(bridgeObject);
            Object.DestroyImmediate(bossObject);
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
        public void PlayerTargetSelectorPrioritizesActiveHostileSummonFrontline()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = Vector3.zero;
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            PlayerCombatTargetSelector targetSelector = playerObject.AddComponent<PlayerCombatTargetSelector>();

            GameObject bossObject = new GameObject("Boss");
            bossObject.transform.position = new Vector3(0f, 0f, 8f);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            GameObject summonObject = new GameObject("BossSummon");
            summonObject.transform.position = new Vector3(0f, 0f, 3f);
            CombatHealth summonHealth = summonObject.AddComponent<CombatHealth>();
            summonHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy summonProxy = summonObject.AddComponent<SummonFrontlineProxy>();
            summonProxy.ConfigureHealth(summonHealth);
            summonProxy.Activate(
                summonObject.transform.position,
                Vector3.back,
                2,
                0f,
                1f,
                summonObject.transform.position,
                0.2f,
                180f,
                0.8f);

            var serializedSelector = new SerializedObject(targetSelector);
            SerializedProperty candidates = serializedSelector.FindProperty("targetCandidates");
            candidates.arraySize = 1;
            candidates.GetArrayElementAtIndex(0).objectReferenceValue = bossHealth;
            serializedSelector.ApplyModifiedPropertiesWithoutUndo();

            Assert.IsTrue(targetSelector.IncludesActiveHostileSummons);
            Assert.GreaterOrEqual(SummonFrontlineProxy.ActiveRegisteredProxyCount, 1);
            Assert.IsTrue(targetSelector.RefreshTarget());
            Assert.AreSame(
                summonHealth,
                targetSelector.CurrentTargetHealth,
                "Player targeting should answer an active boss-side summon body before shooting through to the boss proxy.");

            summonHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                9999f,
                summonObject.transform.position,
                Vector3.forward,
                0f));

            Assert.IsFalse(summonProxy.IsActive);
            Assert.IsTrue(targetSelector.RefreshTarget());
            Assert.AreSame(
                bossHealth,
                targetSelector.CurrentTargetHealth,
                "After the frontline summon is defeated, player targeting should return to the authored boss candidate.");

            Object.DestroyImmediate(summonObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void PlayerTargetSelectorKeepsAuthoredBossWhileRuntimeCandidatesComeAndGo()
        {
            GameObject playerObject = new GameObject("RuntimeCandidatePlayer");
            GameObject bossObject = new GameObject("AuthoredBoss");
            GameObject firstAddObject = new GameObject("RuntimeAddA");
            GameObject secondAddObject = new GameObject("RuntimeAddB");
            try
            {
                playerObject.transform.position = Vector3.zero;
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                PlayerCombatTargetSelector targetSelector =
                    playerObject.AddComponent<PlayerCombatTargetSelector>();

                bossObject.transform.position = new Vector3(0f, 0f, 8f);
                CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
                bossHealth.ConfigureTeam(DamageTeam.Enemy);

                firstAddObject.transform.position = new Vector3(0f, 0f, 2f);
                CombatHealth firstAddHealth = firstAddObject.AddComponent<CombatHealth>();
                firstAddHealth.ConfigureTeam(DamageTeam.Enemy);

                secondAddObject.transform.position = new Vector3(0f, 0f, 3f);
                CombatHealth secondAddHealth = secondAddObject.AddComponent<CombatHealth>();
                secondAddHealth.ConfigureTeam(DamageTeam.Enemy);

                targetSelector.ConfigureTargetCandidates(new[] { bossHealth }, refreshNow: false);
                Assert.That(targetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
                Assert.That(
                    targetSelector.TryRegisterRuntimeTargetCandidate(
                        firstAddHealth,
                        out string firstError,
                        refreshNow: false),
                    Is.True,
                    firstError);
                Assert.That(
                    targetSelector.TryRegisterRuntimeTargetCandidate(
                        secondAddHealth,
                        out string secondError,
                        refreshNow: false),
                    Is.True,
                    secondError);
                Assert.That(
                    targetSelector.TryRegisterRuntimeTargetCandidate(
                        firstAddHealth,
                        out string duplicateError,
                        refreshNow: false),
                    Is.True,
                    duplicateError);

                Assert.That(targetSelector.TargetCandidateCount, Is.EqualTo(1));
                Assert.That(targetSelector.RuntimeTargetCandidateCount, Is.EqualTo(2));
                Assert.That(targetSelector.RefreshTarget(), Is.True);
                Assert.That(targetSelector.CurrentTargetHealth, Is.SameAs(firstAddHealth));
                Assert.That(
                    targetSelector.TryGetAttackAimDirection(
                        Vector3.forward,
                        2.5f,
                        out Vector3 meleeAimDirection,
                        out CombatHealth meleeAimTarget),
                    Is.True);
                Assert.That(meleeAimTarget, Is.SameAs(firstAddHealth));
                Assert.That(Vector3.Dot(meleeAimDirection, Vector3.forward), Is.GreaterThan(0.99f));
                Assert.That(
                    targetSelector.TryGetRangedAimAssistDirection(
                        playerObject.transform.position,
                        Vector3.forward,
                        10f,
                        45f,
                        out Vector3 rangedAimDirection,
                        out CombatHealth rangedAimTarget),
                    Is.True);
                Assert.That(rangedAimTarget, Is.SameAs(firstAddHealth));
                Assert.That(Vector3.Dot(rangedAimDirection, Vector3.forward), Is.GreaterThan(0.99f));
                Assert.That(
                    targetSelector.TryGetBestLockTarget(
                        playerObject.transform.position,
                        Vector3.forward,
                        10f,
                        45f,
                        null,
                        0f,
                        out CombatHealth lockTarget,
                        out Vector3 _,
                        out float _),
                    Is.True);
                Assert.That(lockTarget, Is.SameAs(firstAddHealth));

                Assert.That(
                    targetSelector.UnregisterRuntimeTargetCandidate(firstAddHealth),
                    Is.True);
                Assert.That(targetSelector.RuntimeTargetCandidateCount, Is.EqualTo(1));
                Assert.That(targetSelector.ContainsAuthoredTargetCandidate(bossHealth), Is.True);
                Assert.That(targetSelector.CurrentTargetHealth, Is.SameAs(secondAddHealth));

                secondAddObject.SetActive(false);
                Assert.That(targetSelector.RuntimeTargetCandidateCount, Is.Zero);
                Assert.That(targetSelector.ContainsRuntimeTargetCandidate(secondAddHealth), Is.False);
                Assert.That(targetSelector.RefreshTarget(), Is.True);
                Assert.That(targetSelector.CurrentTargetHealth, Is.SameAs(bossHealth));
            }
            finally
            {
                Object.DestroyImmediate(secondAddObject);
                Object.DestroyImmediate(firstAddObject);
                Object.DestroyImmediate(bossObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void PlayerTargetSelectorDisableRejectsReentrantRuntimeCandidateRegistration()
        {
            GameObject playerObject = new GameObject("ReentrantRuntimeCandidatePlayer");
            GameObject bossObject = new GameObject("ReentrantAuthoredBoss");
            GameObject addObject = new GameObject("ReentrantRuntimeAdd");
            try
            {
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                PlayerCombatTargetSelector targetSelector =
                    playerObject.AddComponent<PlayerCombatTargetSelector>();

                bossObject.transform.position = new Vector3(0f, 0f, 8f);
                CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
                bossHealth.ConfigureTeam(DamageTeam.Enemy);
                targetSelector.ConfigureTargetCandidates(new[] { bossHealth }, refreshNow: false);

                addObject.transform.position = new Vector3(0f, 0f, 2f);
                CombatHealth addHealth = addObject.AddComponent<CombatHealth>();
                addHealth.ConfigureTeam(DamageTeam.Enemy);

                Assert.That(
                    targetSelector.TryRegisterRuntimeTargetCandidate(
                        addHealth,
                        out string registrationError),
                    Is.True,
                    registrationError);
                Assert.That(targetSelector.CurrentTargetHealth, Is.SameAs(addHealth));

                int nullTargetCallbackCount = 0;
                bool reentrantQueryAccepted = true;
                bool reentrantRegistrationAccepted = true;
                string reentrantError = string.Empty;
                targetSelector.TargetChanged += HandleTargetChanged;

                targetSelector.enabled = false;

                Assert.That(nullTargetCallbackCount, Is.EqualTo(1));
                Assert.That(reentrantQueryAccepted, Is.False);
                Assert.That(reentrantRegistrationAccepted, Is.False);
                Assert.That(reentrantError, Does.Contain("blocked"));
                Assert.That(targetSelector.RuntimeTargetCandidateCount, Is.Zero);
                Assert.That(targetSelector.CurrentTargetHealth, Is.Null);

                void HandleTargetChanged(CombatHealth nextTarget)
                {
                    if (!ReferenceEquals(nextTarget, null))
                    {
                        return;
                    }

                    nullTargetCallbackCount++;
                    reentrantQueryAccepted =
                        targetSelector.TryGetCurrentTarget(out Transform _, out CombatHealth _);
                    reentrantRegistrationAccepted =
                        targetSelector.TryRegisterRuntimeTargetCandidate(
                            addHealth,
                            out reentrantError,
                            refreshNow: false);
                }
            }
            finally
            {
                Object.DestroyImmediate(addObject);
                Object.DestroyImmediate(bossObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void RangedBasicManualAimStillAppliesWeakAssistForCloseThreat()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            PlayerCombatTargetSelector targetSelector = playerObject.AddComponent<PlayerCombatTargetSelector>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();

            GameObject targetObject = new GameObject("CloseThreat");
            targetObject.transform.position = new Vector3(0.6f, 0f, 5f);
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Enemy);

            var serializedSelector = new SerializedObject(targetSelector);
            serializedSelector.FindProperty("selfHealth").objectReferenceValue = playerHealth;
            serializedSelector.FindProperty("selectionOrigin").objectReferenceValue = playerObject.transform;
            SerializedProperty candidates = serializedSelector.FindProperty("targetCandidates");
            candidates.arraySize = 1;
            candidates.GetArrayElementAtIndex(0).objectReferenceValue = targetHealth;
            serializedSelector.ApplyModifiedPropertiesWithoutUndo();

            rangedAction.ConfigureReferences(null, null, null, targetSelector, playerHealth, null, null);
            var serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("aimFromCameraViewport").boolValue = false;
            serializedAction.FindProperty("preserveVerticalAim").boolValue = false;
            serializedAction.FindProperty("useAimAssist").boolValue = true;
            serializedAction.FindProperty("disableAimAssistWithManualInput").boolValue = false;
            serializedAction.FindProperty("aimInputYawDegrees").floatValue = 0f;
            serializedAction.FindProperty("aimAssistDistance").floatValue = 10f;
            serializedAction.FindProperty("hipAimAssistAngleDegrees").floatValue = 12f;
            serializedAction.FindProperty("aimAssistMaxTurnDegrees").floatValue = 12f;
            serializedAction.FindProperty("spawnForwardOffset").floatValue = 0f;
            serializedAction.FindProperty("spawnHeight").floatValue = 0f;
            serializedAction.FindProperty("targetHeight").floatValue = 0f;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            rangedAction.SetAimInput(Vector2.right);

            Assert.IsTrue(rangedAction.TryGetAimPreviewDirection(out Vector3 assistedDirection));
            Assert.IsTrue(rangedAction.HasAimAssistTarget);
            Assert.AreSame(targetHealth, rangedAction.AimAssistTargetHealth);
            Assert.Greater(rangedAction.AimAssistStrength01, 0f);
            Vector3 expectedTargetDirection = (targetObject.transform.position - playerObject.transform.position).normalized;
            Assert.Less(
                Vector3.Angle(assistedDirection, expectedTargetDirection),
                0.5f,
                "Manual Look/TargetBias input should not disable the weak close-threat assist around the center aim line.");

            Object.DestroyImmediate(targetObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicAimAssistUsesTargetColliderBodyInsteadOfTransformOrigin()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            PlayerCombatTargetSelector targetSelector = playerObject.AddComponent<PlayerCombatTargetSelector>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();

            GameObject targetObject = new GameObject("OffsetBodyThreat");
            targetObject.transform.position = new Vector3(4f, 0f, 5f);
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Enemy);
            BoxCollider targetBody = targetObject.AddComponent<BoxCollider>();
            targetBody.center = new Vector3(-3.4f, 0.6f, 0f);
            targetBody.size = new Vector3(1.2f, 1.2f, 1.2f);

            var serializedSelector = new SerializedObject(targetSelector);
            serializedSelector.FindProperty("selfHealth").objectReferenceValue = playerHealth;
            serializedSelector.FindProperty("selectionOrigin").objectReferenceValue = playerObject.transform;
            SerializedProperty candidates = serializedSelector.FindProperty("targetCandidates");
            candidates.arraySize = 1;
            candidates.GetArrayElementAtIndex(0).objectReferenceValue = targetHealth;
            serializedSelector.ApplyModifiedPropertiesWithoutUndo();

            rangedAction.ConfigureReferences(null, null, null, targetSelector, playerHealth, null, null);
            var serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("aimFromCameraViewport").boolValue = false;
            serializedAction.FindProperty("preserveVerticalAim").boolValue = false;
            serializedAction.FindProperty("useAimAssist").boolValue = true;
            serializedAction.FindProperty("disableAimAssistWithManualInput").boolValue = false;
            serializedAction.FindProperty("aimInputYawDegrees").floatValue = 0f;
            serializedAction.FindProperty("aimAssistDistance").floatValue = 10f;
            serializedAction.FindProperty("hipAimAssistAngleDegrees").floatValue = 12f;
            serializedAction.FindProperty("aimAssistMaxTurnDegrees").floatValue = 12f;
            serializedAction.FindProperty("spawnForwardOffset").floatValue = 0f;
            serializedAction.FindProperty("spawnHeight").floatValue = 0f;
            serializedAction.FindProperty("targetHeight").floatValue = 0f;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            Physics.SyncTransforms();
            Assert.IsTrue(rangedAction.TryGetAimPreviewDirection(out Vector3 assistedDirection));
            Assert.IsTrue(
                rangedAction.HasAimAssistTarget,
                "A visible body collider near the aim line should still activate ranged assist even when the root transform is off to the side.");
            Assert.AreSame(targetHealth, rangedAction.AimAssistTargetHealth);
            Vector3 normalizedDirection = assistedDirection.normalized;
            float projectedDistance = Mathf.Max(
                0f,
                Vector3.Dot(targetBody.bounds.center - playerObject.transform.position, normalizedDirection));
            Vector3 closestPointOnPath = playerObject.transform.position + normalizedDirection * projectedDistance;
            Assert.Less(
                targetBody.bounds.SqrDistance(closestPointOnPath),
                0.0001f,
                "The assisted basic shot should pass through the combat body, not the off-center target root transform.");

            Object.DestroyImmediate(targetObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicCameraAimIgnoresAllySummonAndTargetsEnemyBehind()
        {
            GameObject playerObject = new GameObject("Player");
            GameObject cameraObject = new GameObject("ActionCamera");
            GameObject allyObject = new GameObject("AllySummon");
            GameObject enemyObject = new GameObject("Enemy");

            try
            {
                Vector3 testOrigin = new Vector3(1000f, 100f, 1000f);
                playerObject.transform.SetPositionAndRotation(testOrigin, Quaternion.LookRotation(Vector3.forward));
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                PlayerCombatTargetSelector targetSelector = playerObject.AddComponent<PlayerCombatTargetSelector>();
                PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.nearClipPlane = 0.01f;
                cameraObject.transform.SetPositionAndRotation(
                    testOrigin + new Vector3(0f, 1f, -5f),
                    Quaternion.LookRotation(Vector3.forward));
                ActionCameraController cameraController = cameraObject.AddComponent<ActionCameraController>();

                allyObject.transform.position = testOrigin + new Vector3(0f, 1f, 4f);
                allyObject.AddComponent<SphereCollider>().radius = 0.75f;
                CombatHealth allyHealth = allyObject.AddComponent<CombatHealth>();
                allyHealth.ConfigureTeam(DamageTeam.AllySummon);

                enemyObject.transform.position = testOrigin + new Vector3(0f, 1f, 8f);
                enemyObject.AddComponent<SphereCollider>().radius = 0.75f;
                CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
                enemyHealth.ConfigureTeam(DamageTeam.Enemy);

                targetSelector.ConfigureTargetCandidates(new[] { enemyHealth }, refreshNow: false);
                rangedAction.ConfigureReferences(
                    null,
                    null,
                    null,
                    targetSelector,
                    playerHealth,
                    cameraController,
                    null);

                Physics.SyncTransforms();

                Assert.IsTrue(rangedAction.TryGetAimPreviewDirection(out _));
                Assert.IsTrue(rangedAction.HasAimAssistTarget);
                Assert.AreSame(
                    enemyHealth,
                    rangedAction.AimAssistTargetHealth,
                    "A player-side summon crossing the camera ray must not replace the hostile aim target behind it.");
                Assert.AreNotSame(allyHealth, rangedAction.AimAssistTargetHealth);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(allyObject);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void RangedBasicStartsReloadWhenMagazineRunsEmpty()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();

            GameObject projectilePrefabObject = new GameObject("RangedProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectilePrefab = projectilePrefabObject.AddComponent<LaneActionProjectile>();
            projectilePrefabObject.SetActive(false);

            SerializedObject serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            serializedAction.FindProperty("projectileRoot").objectReferenceValue = playerObject.transform;
            serializedAction.FindProperty("magazineSize").intValue = 2;
            serializedAction.FindProperty("reloadSeconds").floatValue = 1f;
            serializedAction.FindProperty("fireIntervalSeconds").floatValue = 0.01f;
            serializedAction.FindProperty("prewarmCount").intValue = 0;
            serializedAction.FindProperty("aimFromCameraViewport").boolValue = false;
            serializedAction.FindProperty("useAimAssist").boolValue = false;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();
            SetPrivateInstanceField(rangedAction, "ammoInitialized", true);
            SetPrivateInstanceField(rangedAction, "currentAmmo", 2);

            int reloadStartedCount = 0;
            rangedAction.RangedReloadStarted += () => reloadStartedCount++;

            Assert.IsTrue(rangedAction.TryFire());
            Assert.AreEqual(1, rangedAction.CurrentAmmo);
            Assert.IsFalse(rangedAction.IsReloading);

            SetPrivateInstanceField(rangedAction, "nextFireTime", Time.time - 0.01f);
            Assert.IsTrue(rangedAction.TryFire());

            Assert.AreEqual(0, rangedAction.CurrentAmmo);
            Assert.IsTrue(rangedAction.IsReloading);
            Assert.AreEqual(1, reloadStartedCount);
            Assert.IsFalse(rangedAction.IsFireReady);

            Assert.IsFalse(rangedAction.TryFire());
            StringAssert.Contains("Reloading", rangedAction.LastUseBlockedReason);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicRefillsMagazineAfterReloadFinishes()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();

            GameObject projectilePrefabObject = new GameObject("RangedProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectilePrefab = projectilePrefabObject.AddComponent<LaneActionProjectile>();
            projectilePrefabObject.SetActive(false);

            SerializedObject serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            serializedAction.FindProperty("projectileRoot").objectReferenceValue = playerObject.transform;
            serializedAction.FindProperty("magazineSize").intValue = 2;
            serializedAction.FindProperty("reloadSeconds").floatValue = 0.25f;
            serializedAction.FindProperty("fireIntervalSeconds").floatValue = 0.01f;
            serializedAction.FindProperty("prewarmCount").intValue = 0;
            serializedAction.FindProperty("aimFromCameraViewport").boolValue = false;
            serializedAction.FindProperty("useAimAssist").boolValue = false;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();
            SetPrivateInstanceField(rangedAction, "ammoInitialized", true);
            SetPrivateInstanceField(rangedAction, "currentAmmo", 1);

            int reloadCompletedCount = 0;
            rangedAction.RangedReloadCompleted += () => reloadCompletedCount++;

            Assert.IsTrue(rangedAction.TryFire());
            Assert.AreEqual(0, rangedAction.CurrentAmmo);
            Assert.IsTrue(rangedAction.IsReloading);

            SetPrivateInstanceField(rangedAction, "reloadFinishTime", Time.time - 0.01f);
            SetPrivateInstanceField(rangedAction, "nextFireTime", Time.time - 0.01f);
            Assert.IsTrue(rangedAction.TryFire());

            Assert.IsFalse(rangedAction.IsReloading);
            Assert.AreEqual(1, reloadCompletedCount);
            Assert.AreEqual(1, rangedAction.CurrentAmmo, "Reload should refill to magazine size before the next shot consumes one round.");

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicStartsReloadWhenAimIsReleasedWithSpentAmmo()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            PlayerRangedAimController aimController = playerObject.AddComponent<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            rangedAction.ConfigureReferences(null, aimController, null, null, null, null, null);

            SerializedObject serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("magazineSize").intValue = 2;
            serializedAction.FindProperty("reloadSeconds").floatValue = 1f;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();
            SetPrivateInstanceField(rangedAction, "ammoInitialized", true);
            SetPrivateInstanceField(rangedAction, "currentAmmo", 1);

            int reloadStartedCount = 0;
            rangedAction.RangedReloadStarted += () => reloadStartedCount++;

            aimController.SetAimMode(true);
            Assert.IsTrue(aimController.IsAiming);
            Assert.IsFalse(rangedAction.IsReloading);

            aimController.SetAimMode(false);

            Assert.IsFalse(aimController.IsAiming);
            Assert.IsTrue(rangedAction.IsReloading);
            Assert.AreEqual(1, reloadStartedCount);
            Assert.AreEqual(1, rangedAction.CurrentAmmo);

            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicCancelsAimReleaseReloadWhenAimResumesWithAmmo()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            PlayerRangedAimController aimController = playerObject.AddComponent<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            rangedAction.ConfigureReferences(null, aimController, null, null, null, null, null);

            SerializedObject serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("magazineSize").intValue = 2;
            serializedAction.FindProperty("reloadSeconds").floatValue = 1f;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();
            SetPrivateInstanceField(rangedAction, "ammoInitialized", true);
            SetPrivateInstanceField(rangedAction, "currentAmmo", 1);

            int reloadCanceledCount = 0;
            rangedAction.RangedReloadCanceled += () => reloadCanceledCount++;

            aimController.SetAimMode(true);
            aimController.SetAimMode(false);
            Assert.IsTrue(rangedAction.IsReloading);

            aimController.SetAimMode(true);

            Assert.IsFalse(rangedAction.IsReloading);
            Assert.AreEqual(1, reloadCanceledCount);
            Assert.AreEqual(1, rangedAction.CurrentAmmo);

            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicCanFireImmediatelyAfterCancelingAimReleaseReload()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            PlayerRangedAimController aimController = playerObject.AddComponent<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            rangedAction.ConfigureReferences(null, aimController, null, null, null, null, null);

            GameObject projectilePrefabObject = new GameObject("RangedProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectilePrefab = projectilePrefabObject.AddComponent<LaneActionProjectile>();
            projectilePrefabObject.SetActive(false);

            SerializedObject serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            serializedAction.FindProperty("projectileRoot").objectReferenceValue = playerObject.transform;
            serializedAction.FindProperty("magazineSize").intValue = 2;
            serializedAction.FindProperty("reloadSeconds").floatValue = 1f;
            serializedAction.FindProperty("fireIntervalSeconds").floatValue = 0.01f;
            serializedAction.FindProperty("prewarmCount").intValue = 0;
            serializedAction.FindProperty("aimFromCameraViewport").boolValue = false;
            serializedAction.FindProperty("useAimAssist").boolValue = false;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();
            SetPrivateInstanceField(rangedAction, "ammoInitialized", true);
            SetPrivateInstanceField(rangedAction, "currentAmmo", 1);

            aimController.SetAimMode(true);
            aimController.SetAimMode(false);
            Assert.IsTrue(rangedAction.IsReloading);

            aimController.SetAimMode(true);
            Assert.IsTrue(rangedAction.TryFire());

            Assert.AreEqual(0, rangedAction.CurrentAmmo);
            Assert.IsTrue(
                rangedAction.IsReloading,
                "After the remaining round is fired, the empty magazine should begin its non-cancelable reload.");

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void RangedBasicKeepsReloadingWhenAimResumesWithEmptyMagazine()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            PlayerRangedAimController aimController = playerObject.AddComponent<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            rangedAction.ConfigureReferences(null, aimController, null, null, null, null, null);

            SerializedObject serializedAction = new SerializedObject(rangedAction);
            serializedAction.FindProperty("magazineSize").intValue = 2;
            serializedAction.FindProperty("reloadSeconds").floatValue = 1f;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();
            SetPrivateInstanceField(rangedAction, "ammoInitialized", true);
            SetPrivateInstanceField(rangedAction, "currentAmmo", 0);

            int reloadCanceledCount = 0;
            rangedAction.RangedReloadCanceled += () => reloadCanceledCount++;

            aimController.SetAimMode(true);
            aimController.SetAimMode(false);
            Assert.IsTrue(rangedAction.IsReloading);

            aimController.SetAimMode(true);

            Assert.IsTrue(rangedAction.IsReloading);
            Assert.AreEqual(0, reloadCanceledCount);
            Assert.IsFalse(rangedAction.TryFire());
            StringAssert.Contains("Reloading", rangedAction.LastUseBlockedReason);

            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void SummonHealthBarPresenterShowsAfterActivationAndTracksDamage()
        {
            GameObject proxyObject = new GameObject("SummonProxy");
            CombatHealth health = proxyObject.AddComponent<CombatHealth>();
            health.ConfigureTeam(DamageTeam.AllySummon);
            health.ResetHealthToFull();
            SummonFrontlineProxy proxy = proxyObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(health);

            GameObject barRoot = new GameObject("HealthBarRoot");
            barRoot.transform.SetParent(proxyObject.transform, worldPositionStays: false);
            GameObject backObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backObject.name = "HealthBarBack";
            backObject.transform.SetParent(barRoot.transform, worldPositionStays: false);
            Object.DestroyImmediate(backObject.GetComponent<Collider>());
            Renderer backRenderer = backObject.GetComponent<Renderer>();

            GameObject fillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObject.name = "HealthBarFill";
            fillObject.transform.SetParent(barRoot.transform, worldPositionStays: false);
            Object.DestroyImmediate(fillObject.GetComponent<Collider>());
            Renderer fillRenderer = fillObject.GetComponent<Renderer>();

            SummonFrontlineHealthBarPresenter presenter =
                proxyObject.AddComponent<SummonFrontlineHealthBarPresenter>();
            presenter.ConfigurePresentation(
                proxy,
                health,
                barRoot.transform,
                fillObject.transform,
                new[] { backRenderer, fillRenderer });
            presenter.RefreshNow();
            Assert.IsFalse(presenter.IsShowing, "The summon HP bar should stay hidden before the actor is summoned.");

            proxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 1f, 0.2f);
            presenter.RefreshNow();
            float fullFillWidth = fillObject.transform.localScale.x;
            Assert.IsTrue(presenter.IsShowing, "The summon HP bar should become visible after activation.");
            Assert.AreEqual(1f, presenter.LastHealthRatio, 0.001f);

            Assert.IsTrue(health.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                40f,
                Vector3.zero,
                Vector3.back,
                0f)));
            presenter.RefreshNow();

            Assert.AreEqual(0.6f, presenter.LastHealthRatio, 0.001f);
            Assert.AreEqual(
                fullFillWidth * 0.6f,
                fillObject.transform.localScale.x,
                0.001f,
                "The in-world summon HP fill should follow CombatHealth after the summoned actor takes damage.");

            proxy.Deactivate();
            presenter.RefreshNow();
            Assert.IsFalse(presenter.IsShowing, "The summon HP bar should hide again when the actor leaves.");

            Object.DestroyImmediate(proxyObject);
        }

        [Test]
        public void SummonFrontlineProxyUsesMoveSpeedAndReportsCombatState()
        {
            GameObject proxyObject = new GameObject("SummonProxy");
            CombatHealth health = proxyObject.AddComponent<CombatHealth>();
            health.ConfigureTeam(DamageTeam.AllySummon);
            SummonFrontlineProxy proxy = proxyObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(health);
            Assert.AreEqual(SummonFrontlineProxyActionPhase.Inactive, proxy.ActionPhase);

            proxy.Activate(
                Vector3.zero,
                Vector3.forward,
                1,
                0f,
                1f,
                new Vector3(0f, 0f, 6f),
                0.1f,
                180f,
                1.5f);

            Assert.AreEqual(180f, proxy.MaxHealth, 0.001f);
            Assert.AreEqual(1f, proxy.HealthRatio, 0.001f);
            Assert.AreEqual(DamageTeam.AllySummon, proxy.OwnerTeam);
            Assert.AreEqual(SummonFrontlineProxyState.Advancing, proxy.CurrentState);
            Assert.AreEqual(SummonFrontlineProxyActionPhase.Locomotion, proxy.ActionPhase);
            Assert.AreEqual(1.5f, proxy.ActiveMoveSpeed, 0.001f);

            proxy.Tick(1f);
            Assert.AreEqual(1.5f, proxy.transform.position.z, 0.001f);
            Assert.AreEqual(0.25f, proxy.AdvanceProgress01, 0.001f);

            proxy.RequestAdvanceHold(0.25f);
            Assert.AreEqual(SummonFrontlineProxyState.Engaging, proxy.CurrentState);
            Assert.AreEqual(SummonFrontlineProxyActionPhase.Engage, proxy.ActionPhase);
            Vector3 heldPosition = proxy.transform.position;
            proxy.Tick(0.1f);
            Assert.AreEqual(heldPosition.z, proxy.transform.position.z, 0.001f);

            proxy.NotifyAttackPerformed(0.2f);
            Assert.AreEqual(SummonFrontlineProxyState.Attacking, proxy.CurrentState);
            Assert.AreEqual(SummonFrontlineProxyActionPhase.Attack, proxy.ActionPhase);
            proxy.Tick(0.3f);
            Assert.AreEqual(SummonFrontlineProxyState.Advancing, proxy.CurrentState);
            Assert.AreEqual(SummonFrontlineProxyActionPhase.Locomotion, proxy.ActionPhase);
            proxy.Tick(1f);
            Assert.AreEqual(
                3f,
                proxy.transform.position.z,
                0.001f,
                "Speed-based summon advance should continue until the target distance is reached, not stop after the old duration value.");

            proxy.Deactivate();
            Assert.AreEqual(SummonFrontlineProxyActionPhase.Inactive, proxy.ActionPhase);

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

            Assert.IsTrue(
                projectile.TryApplyImpact(screenCollider, Vector3.zero),
                "Hostile pressure-screen contact should synchronously consume the projectile.");
            Assert.AreEqual(1f, actorHealth.HealthRatio, 0.001f);
            Assert.AreEqual(1, pressureScreen.InterceptedProjectiles);
            Assert.IsFalse(projectile.IsActive);

            projectileObject.SetActive(true);
            projectile.Configure(null, DamageTeam.Player, 30f, Vector3.forward, 0f, 1f, 0.2f);
            Assert.IsTrue(projectile.TryApplyImpact(bodyCollider, Vector3.zero));
            Assert.AreEqual(ProjectileImpactResult.AppliedDamage, projectile.LastImpactResult);
            Assert.AreSame(actorHealth, projectile.LastImpactTargetHealth);
            Assert.Less(actorHealth.HealthRatio, 1f);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(actorObject);
        }

        [Test]
        public void LaneActionProjectileSweepHitsPressureScreenBeforePlayer()
        {
            GameObject targetObject = new GameObject("LaneProjectileSweepCoveredTarget");
            targetObject.transform.position = Vector3.zero;
            targetObject.AddComponent<SphereCollider>().radius = 0.5f;
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Player);
            targetHealth.ResetHealthToFull();
            float healthBefore = targetHealth.CurrentHealth;

            GameObject screenObject = new GameObject("LaneProjectileSweepPressureScreen");
            screenObject.transform.position = Vector3.forward * 2f;
            screenObject.AddComponent<SphereCollider>().radius = 0.75f;
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = screenObject.AddComponent<SummonPressureScreen>();
            int interceptEventCount = 0;
            pressureScreen.ActionProjectileIntercepted += (_, _) => interceptEventCount++;
            pressureScreen.Activate(DamageTeam.AllySummon, 1, 0.75f, 1f);

            GameObject projectileObject = new GameObject("LaneProjectileSweepScreenSource");
            projectileObject.transform.position = Vector3.forward * 5f;
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectile = projectileObject.AddComponent<LaneActionProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 20f, 1f, 0.2f);
            Physics.SyncTransforms();

            projectile.Tick(0.5f);

            Assert.AreEqual(1, pressureScreen.InterceptedProjectiles);
            Assert.AreEqual(1, interceptEventCount);
            Assert.AreEqual(healthBefore, targetHealth.CurrentHealth, 0.001f);
            Assert.IsFalse(projectile.IsActive);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(screenObject);
            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void LaneActionProjectileGivesActivePressureScreenPriorityOverSummonBody()
        {
            GameObject actorObject = new GameObject("LaneScreenedAllySummonActor");
            SphereCollider bodyCollider = actorObject.AddComponent<SphereCollider>();
            CombatHealth actorHealth = actorObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.AllySummon);
            actorHealth.ResetHealthToFull();
            SummonFrontlineProxy proxy = actorObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(actorHealth);
            proxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 1f, 0.1f);

            GameObject screenObject = new GameObject("LaneAllyPressureScreen");
            screenObject.transform.SetParent(actorObject.transform, worldPositionStays: false);
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = screenObject.AddComponent<SummonPressureScreen>();
            pressureScreen.Activate(DamageTeam.AllySummon, 1, 1f, 1f);

            GameObject projectileObject = new GameObject("LaneScreenPriorityProjectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectile = projectileObject.AddComponent<LaneActionProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 999f, Vector3.back, 0f, 1f, 0.2f);

            Assert.IsTrue(projectile.TryApplyImpact(bodyCollider, Vector3.zero));
            Assert.AreEqual(1, pressureScreen.InterceptedProjectiles);
            Assert.AreEqual(1f, actorHealth.HealthRatio, 0.001f);
            Assert.IsTrue(proxy.IsActive);
            Assert.IsFalse(projectile.IsActive);

            screenObject.transform.localPosition = Vector3.forward * 5f;
            pressureScreen.Activate(DamageTeam.AllySummon, 1, 1f, 1f);
            projectileObject.SetActive(true);
            projectile.Configure(null, DamageTeam.Enemy, 999f, Vector3.back, 0f, 1f, 0.2f);
            Physics.SyncTransforms();

            Assert.IsTrue(projectile.TryApplyImpact(bodyCollider, Vector3.zero));
            Assert.Less(actorHealth.HealthRatio, 1f);
            Assert.IsFalse(proxy.IsActive);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(actorObject);
        }

        [Test]
        public void LaneActionProjectileReportsInactiveSummonAndDeadBodyContacts()
        {
            GameObject inactiveSummonObject = new GameObject("InactiveSummonActor");
            SphereCollider inactiveSummonCollider = inactiveSummonObject.AddComponent<SphereCollider>();
            CombatHealth inactiveSummonHealth = inactiveSummonObject.AddComponent<CombatHealth>();
            inactiveSummonHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy inactiveProxy = inactiveSummonObject.AddComponent<SummonFrontlineProxy>();
            inactiveProxy.ConfigureHealth(inactiveSummonHealth);

            GameObject projectileObject = new GameObject("PlayerProjectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectile = projectileObject.AddComponent<LaneActionProjectile>();
            projectile.Configure(null, DamageTeam.Player, 30f, Vector3.forward, 0f, 1f, 0.2f);

            Assert.IsFalse(projectile.TryApplyImpact(inactiveSummonCollider, Vector3.zero));
            Assert.AreEqual(ProjectileImpactResult.IgnoredInactiveSummon, projectile.LastImpactResult);
            Assert.AreSame(inactiveProxy, projectile.LastImpactTargetProxy);

            GameObject deadBodyObject = new GameObject("DeadBodyTarget");
            SphereCollider deadBodyCollider = deadBodyObject.AddComponent<SphereCollider>();
            CombatHealth deadBodyHealth = deadBodyObject.AddComponent<CombatHealth>();
            deadBodyHealth.ConfigureTeam(DamageTeam.Enemy);
            deadBodyHealth.ResetHealthToFull();
            Assert.IsTrue(deadBodyHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                999f,
                Vector3.zero,
                Vector3.forward,
                0f)));

            projectile.Configure(null, DamageTeam.Player, 30f, Vector3.forward, 0f, 1f, 0.2f);
            Assert.IsFalse(projectile.TryApplyImpact(deadBodyCollider, Vector3.zero));
            Assert.AreEqual(ProjectileImpactResult.IgnoredDeadTarget, projectile.LastImpactResult);
            Assert.AreSame(deadBodyHealth, projectile.LastImpactTargetHealth);

            Object.DestroyImmediate(deadBodyObject);
            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(inactiveSummonObject);
        }

        [Test]
        public void LaneActionProjectileDefaultsToNonLockingFlashDamageAndCanEscalate()
        {
            GameObject targetObject = new GameObject("ProjectilePolicyTarget");
            SphereCollider targetCollider = targetObject.AddComponent<SphereCollider>();
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Enemy);
            targetHealth.ResetHealthToFull();
            DamageInfo? lastDamageInfo = null;
            targetHealth.Damaged += damageInfo => lastDamageInfo = damageInfo;

            GameObject projectileObject = new GameObject("ProjectilePolicySource");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            LaneActionProjectile projectile = projectileObject.AddComponent<LaneActionProjectile>();

            projectile.Configure(null, DamageTeam.Player, 10f, Vector3.forward, 0f, 1f, 0.2f);
            Assert.AreEqual(0f, projectile.HitStopSeconds, 0.001f);
            Assert.AreEqual(DamageResponsePolicy.FlashOnly, projectile.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, projectile.ControlLockPolicy);
            Assert.IsTrue(projectile.TryApplyImpact(targetCollider, Vector3.zero));
            Assert.IsTrue(lastDamageInfo.HasValue);
            Assert.AreEqual(DamageResponsePolicy.FlashOnly, lastDamageInfo.Value.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, lastDamageInfo.Value.ControlLockPolicy);

            projectile.Configure(
                null,
                DamageTeam.Player,
                10f,
                Vector3.forward,
                0f,
                1f,
                0.2f,
                DamageResponsePolicy.Stagger,
                CombatControlLockPolicy.InterruptAction,
                0.035f);
            Assert.AreEqual(0.035f, projectile.HitStopSeconds, 0.001f);
            Assert.AreEqual(DamageResponsePolicy.Stagger, projectile.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, projectile.ControlLockPolicy);
            Assert.IsTrue(projectile.TryApplyImpact(targetCollider, Vector3.zero));
            Assert.IsTrue(lastDamageInfo.HasValue);
            Assert.AreEqual(0.035f, lastDamageInfo.Value.HitStopSeconds, 0.001f);
            Assert.AreEqual(DamageResponsePolicy.Stagger, lastDamageInfo.Value.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, lastDamageInfo.Value.ControlLockPolicy);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(targetObject);
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
            Assert.AreEqual(ProjectileImpactResult.AppliedDamage, projectile.LastImpactResult);
            Assert.AreSame(proxy, projectile.LastImpactTargetProxy);
            Assert.IsFalse(proxy.IsActive);
            Assert.AreEqual(SummonFrontlineProxyExitReason.Defeated, proxy.LastExitReason);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(actorObject);
        }

        [Test]
        public void BossBarrageProjectileSweepsAcrossPlayerAtLargeDelta()
        {
            GameObject targetObject = new GameObject("BossProjectileSweepTarget");
            targetObject.AddComponent<SphereCollider>().radius = 0.5f;
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Player);
            targetHealth.ResetHealthToFull();
            float healthBefore = targetHealth.CurrentHealth;

            GameObject projectileObject = new GameObject("BossProjectileSweepSource");
            projectileObject.transform.position = Vector3.forward * 5f;
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 20f, 1f, 0.2f);
            Physics.SyncTransforms();

            projectile.Tick(0.5f);

            Assert.Less(targetHealth.CurrentHealth, healthBefore);
            Assert.AreEqual(ProjectileImpactResult.AppliedDamage, projectile.LastImpactResult);
            Assert.AreSame(targetHealth, projectile.LastImpactTargetHealth);
            Assert.IsFalse(projectile.IsActive);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void BossBarrageProjectileSweepHitsPressureScreenBeforePlayer()
        {
            GameObject targetObject = new GameObject("BossProjectileSweepCoveredTarget");
            targetObject.transform.position = Vector3.zero;
            targetObject.AddComponent<SphereCollider>().radius = 0.5f;
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Player);
            targetHealth.ResetHealthToFull();
            float healthBefore = targetHealth.CurrentHealth;

            GameObject screenObject = new GameObject("BossProjectileSweepPressureScreen");
            screenObject.transform.position = Vector3.forward * 2f;
            screenObject.AddComponent<SphereCollider>().radius = 0.75f;
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = screenObject.AddComponent<SummonPressureScreen>();
            pressureScreen.Activate(DamageTeam.AllySummon, 1, 0.75f, 1f);

            GameObject projectileObject = new GameObject("BossProjectileSweepScreenSource");
            projectileObject.transform.position = Vector3.forward * 5f;
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 20f, 1f, 0.2f);
            Physics.SyncTransforms();

            projectile.Tick(0.5f);

            Assert.AreEqual(1, pressureScreen.InterceptedProjectiles);
            Assert.AreEqual(healthBefore, targetHealth.CurrentHealth, 0.001f);
            Assert.IsFalse(projectile.IsActive);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(screenObject);
            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void BossBarrageProjectileGivesActivePressureScreenPriorityOverSummonBody()
        {
            GameObject actorObject = new GameObject("ScreenedAllySummonActor");
            SphereCollider bodyCollider = actorObject.AddComponent<SphereCollider>();
            CombatHealth actorHealth = actorObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.AllySummon);
            actorHealth.ResetHealthToFull();
            SummonFrontlineProxy proxy = actorObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(actorHealth);

            GameObject screenObject = new GameObject("AllyPressureScreen");
            screenObject.transform.SetParent(actorObject.transform, worldPositionStays: false);
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen pressureScreen = screenObject.AddComponent<SummonPressureScreen>();
            proxy.ConfigurePresentation(null, pressureScreen);
            proxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 1f, 0.1f);
            pressureScreen.Activate(DamageTeam.AllySummon, 1, 1f, 1f);

            GameObject projectileObject = new GameObject("BossProjectile");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 999f, Vector3.back, 0f, 1f, 0.2f);

            Assert.IsTrue(
                projectile.TryApplyImpact(bodyCollider, Vector3.zero),
                "A covered summon body hit should be consumed by the active pressure screen first.");
            Assert.AreEqual(1, pressureScreen.InterceptedProjectiles);
            Assert.AreEqual(1f, actorHealth.HealthRatio, 0.001f);
            Assert.IsTrue(proxy.IsActive);
            Assert.IsFalse(projectile.IsActive);

            screenObject.transform.localPosition = Vector3.forward * 5f;
            pressureScreen.Activate(DamageTeam.AllySummon, 1, 1f, 1f);
            projectileObject.SetActive(true);
            projectile.Configure(null, DamageTeam.Enemy, 999f, Vector3.back, 0f, 1f, 0.2f);

            Assert.IsTrue(
                projectile.TryApplyImpact(bodyCollider, Vector3.zero),
                "Outside the pressure-screen radius, the summon body should still take boss projectile damage.");
            Assert.AreEqual(ProjectileImpactResult.AppliedDamage, projectile.LastImpactResult);
            Assert.AreSame(proxy, projectile.LastImpactTargetProxy);
            Assert.IsFalse(proxy.IsActive);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(actorObject);
        }

        [Test]
        public void BossBarrageProjectileDefaultsToNonLockingFlashDamage()
        {
            GameObject targetObject = new GameObject("BossProjectilePolicyTarget");
            SphereCollider targetCollider = targetObject.AddComponent<SphereCollider>();
            CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
            targetHealth.ConfigureTeam(DamageTeam.Player);
            targetHealth.ResetHealthToFull();
            DamageInfo? lastDamageInfo = null;
            targetHealth.Damaged += damageInfo => lastDamageInfo = damageInfo;

            GameObject projectileObject = new GameObject("BossProjectilePolicySource");
            projectileObject.AddComponent<SphereCollider>();
            projectileObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.2f);

            Assert.AreEqual(DamageResponsePolicy.FlashOnly, projectile.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, projectile.ControlLockPolicy);
            Assert.IsTrue(projectile.TryApplyImpact(targetCollider, Vector3.zero));
            Assert.IsTrue(lastDamageInfo.HasValue);
            Assert.AreEqual(DamageResponsePolicy.FlashOnly, lastDamageInfo.Value.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, lastDamageInfo.Value.ControlLockPolicy);

            Object.DestroyImmediate(projectileObject);
            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void BossProjectileProfilesDefaultToNonLockingFlashDamage()
        {
            BossBarragePatternProfile barrageProfile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBasicFireProfile basicFireProfile = ScriptableObject.CreateInstance<BossBasicFireProfile>();

            Assert.AreEqual(DamageResponsePolicy.FlashOnly, barrageProfile.DamageResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, barrageProfile.ControlLockPolicy);
            Assert.AreEqual(DamageResponsePolicy.FlashOnly, basicFireProfile.DamageResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, basicFireProfile.ControlLockPolicy);

            Object.DestroyImmediate(basicFireProfile);
            Object.DestroyImmediate(barrageProfile);
        }

        [Test]
        public void PlayerSkill1ProjectilesDeclareCommittedHitPolicy()
        {
            GameObject skillObject = new GameObject("Skill1Policy");
            PlayerSkill1Action skill1 = skillObject.AddComponent<PlayerSkill1Action>();

            Assert.AreEqual(DamageResponsePolicy.Stagger, skill1.ProjectileResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, skill1.ProjectileControlLockPolicy);

            Object.DestroyImmediate(skillObject);
        }

        [Test]
        public void PlayerActionProfileTreatsUnsetBasicHitsAsNonLockingUntilFinisher()
        {
            PlayerActionProfile.AttackStep legacyStep = default;

            Assert.AreEqual(
                DamageResponsePolicy.FlashOnly,
                PlayerActionProfile.ResolveResponsePolicy(legacyStep, 0, 5));
            Assert.AreEqual(
                CombatControlLockPolicy.None,
                PlayerActionProfile.ResolveControlLockPolicy(legacyStep, 0, 5));

            Assert.AreEqual(
                DamageResponsePolicy.Stagger,
                PlayerActionProfile.ResolveResponsePolicy(legacyStep, 4, 5));
            Assert.AreEqual(
                CombatControlLockPolicy.InterruptAction,
                PlayerActionProfile.ResolveControlLockPolicy(legacyStep, 4, 5));

            Assert.AreEqual(
                DamageResponsePolicy.Stagger,
                PlayerActionProfile.ResolveResponsePolicy(legacyStep, 0, 1));
            Assert.AreEqual(
                CombatControlLockPolicy.InterruptAction,
                PlayerActionProfile.ResolveControlLockPolicy(legacyStep, 0, 1));
        }

        [Test]
        public void PlayerActionProfileDefaultComboTagsOnlyFinisherAsInterrupt()
        {
            PlayerActionProfile profile = ScriptableObject.CreateInstance<PlayerActionProfile>();
            PlayerActionProfile.AttackStep[] combo = profile.BasicCombo;

            Assert.AreEqual(5, combo.Length);
            for (int i = 0; i < combo.Length - 1; i++)
            {
                Assert.AreEqual(DamageResponsePolicy.FlashOnly, combo[i].responsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.None, combo[i].controlLockPolicy);
                Assert.AreEqual(DamageResponsePolicy.FlashOnly, PlayerActionProfile.ResolveResponsePolicy(combo[i], i, combo.Length));
                Assert.AreEqual(CombatControlLockPolicy.None, PlayerActionProfile.ResolveControlLockPolicy(combo[i], i, combo.Length));
            }

            PlayerActionProfile.AttackStep finisher = combo[combo.Length - 1];
            Assert.AreEqual(DamageResponsePolicy.Stagger, finisher.responsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, finisher.controlLockPolicy);
            Assert.AreEqual(
                DamageResponsePolicy.Stagger,
                PlayerActionProfile.ResolveResponsePolicy(finisher, combo.Length - 1, combo.Length));
            Assert.AreEqual(
                CombatControlLockPolicy.InterruptAction,
                PlayerActionProfile.ResolveControlLockPolicy(finisher, combo.Length - 1, combo.Length));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void CombatAiPatternProfileDefaultsToNonLockingFlashDamage()
        {
            EnemyPatternProfile profile = ScriptableObject.CreateInstance<EnemyPatternProfile>();

            Assert.AreEqual(DamageResponsePolicy.FlashOnly, profile.DamageResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, profile.ControlLockPolicy);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void CombatAiPatternProfileCanDeclareCommittedActionLock()
        {
            EnemyPatternProfile profile = ScriptableObject.CreateInstance<EnemyPatternProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            serializedObject.FindProperty("damageResponsePolicy").enumValueIndex = (int)DamageResponsePolicy.Stagger;
            serializedObject.FindProperty("controlLockPolicy").enumValueIndex =
                (int)CombatControlLockPolicy.InterruptAction;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(DamageResponsePolicy.Stagger, profile.DamageResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, profile.ControlLockPolicy);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void CombatAiPatternProfileInfersLegacyCommittedAttackPolicy()
        {
            EnemyPatternProfile profile = ScriptableObject.CreateInstance<EnemyPatternProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            serializedObject.FindProperty("damage").floatValue = 30f;
            serializedObject.FindProperty("damageResponsePolicy").enumValueIndex = (int)DamageResponsePolicy.Default;
            serializedObject.FindProperty("controlLockPolicy").enumValueIndex = (int)CombatControlLockPolicy.None;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(DamageResponsePolicy.Stagger, profile.DamageResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, profile.ControlLockPolicy);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void SummonFrontlineClashDamagesHostileSummonsAndHoldsAdvance()
        {
            GameObject allyObject = new GameObject("AllySummonActor");
            SphereCollider allyCollider = allyObject.AddComponent<SphereCollider>();
            allyCollider.isTrigger = true;
            allyCollider.center = new Vector3(0f, 0.9f, 0f);
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
            DamageInfo? enemyDamageInfo = null;
            enemyHealth.Damaged += damageInfo => enemyDamageInfo = damageInfo;
            SummonFrontlineProxy enemyProxy = enemyObject.AddComponent<SummonFrontlineProxy>();
            enemyProxy.ConfigureHealth(enemyHealth);

            allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 3f, 1f);
            enemyProxy.Activate(Vector3.forward * 0.6f, Vector3.back, 1, 2f, 1f, 3f, 1f);

            Assert.IsTrue(allyClash.TryProcessClash(enemyCollider));
            Assert.Less(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
            Assert.IsTrue(enemyDamageInfo.HasValue);
            Assert.AreEqual(
                DamageResponsePolicy.FlashOnly,
                enemyDamageInfo.Value.ResponsePolicy,
                "Summon clash damage should explicitly route contact readability through clash feedback instead of a full-body hit animation.");
            Assert.AreEqual(
                CombatControlLockPolicy.None,
                enemyDamageInfo.Value.ControlLockPolicy,
                "Summon clash damage should not declare an action lock for the victim.");
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(enemyProxy.IsAdvanceHeld);
            Assert.IsTrue(allyClash.IsClashing);
            Assert.AreEqual(1, allyClash.TotalClashCount);
            Assert.AreEqual(DamageTeam.Enemy, allyClash.LastOpponentTeam);
            Assert.AreEqual(SummonFrontlineClashTargetKind.HostileSummon, allyClash.LastTargetKind);
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
        public void SummonFrontlineClashReusesPrewarmedContactDamageVfx()
        {
            GameObject contactVfxPrefab = new GameObject("ContactDamageVfxPrefab");
            GameObject allyObject = new GameObject("PooledVfxAllySummon");
            GameObject enemyObject = new GameObject("PooledVfxEnemySummon");
            try
            {
                contactVfxPrefab.AddComponent<ParticleSystem>();
                contactVfxPrefab.SetActive(false);

                SphereCollider allyCollider = allyObject.AddComponent<SphereCollider>();
                allyCollider.isTrigger = true;
                CombatHealth allyHealth = allyObject.AddComponent<CombatHealth>();
                allyHealth.ConfigureTeam(DamageTeam.AllySummon);
                SummonFrontlineProxy allyProxy = allyObject.AddComponent<SummonFrontlineProxy>();
                allyProxy.ConfigureHealth(allyHealth);
                SummonFrontlineClash allyClash = allyObject.AddComponent<SummonFrontlineClash>();
                allyClash.ConfigureReferences(allyProxy, allyHealth);
                allyClash.ConfigureTuning(1f, 0.35f, 0f, 0.1f);
                SetPrivateInstanceField(allyClash, "contactDamageVfxPrefab", contactVfxPrefab);
                SetPrivateInstanceField(allyClash, "contactDamageVfxPrewarmCount", 3);

                System.Reflection.MethodInfo prewarmMethod = typeof(SummonFrontlineClash).GetMethod(
                    "PrewarmContactDamageVfxPool",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.IsNotNull(prewarmMethod);
                prewarmMethod.Invoke(allyClash, null);

                SphereCollider enemyCollider = enemyObject.AddComponent<SphereCollider>();
                enemyCollider.isTrigger = true;
                CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
                enemyHealth.ConfigureTeam(DamageTeam.Enemy);
                enemyHealth.ConfigureMaxHealth(1000f);
                SummonFrontlineProxy enemyProxy = enemyObject.AddComponent<SummonFrontlineProxy>();
                enemyProxy.ConfigureHealth(enemyHealth);
                SummonFrontlineClash enemyClash = enemyObject.AddComponent<SummonFrontlineClash>();
                enemyClash.ConfigureReferences(enemyProxy, enemyHealth);
                SetPrivateInstanceField(enemyClash, "contactDamageVfxPrefab", contactVfxPrefab);
                SetPrivateInstanceField(enemyClash, "contactDamageVfxPrewarmCount", 3);
                prewarmMethod.Invoke(enemyClash, null);

                allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 2f, 1f, 3f, 1f);
                enemyProxy.Activate(Vector3.forward * 0.5f, Vector3.back, 1, 2f, 1f, 3f, 1f);

                int prewarmedCount = allyClash.ContactDamageVfxPoolSize;
                Assert.AreEqual(3, prewarmedCount);
                Assert.AreEqual(
                    prewarmedCount,
                    enemyClash.ContactDamageVfxPoolSize,
                    "Actors using the same impact prefab should share one scene-level VFX pool.");
                for (int hitIndex = 0; hitIndex < prewarmedCount; hitIndex++)
                {
                    SetPrivateInstanceField(allyClash, "nextDamageTime", 0f);
                    Assert.IsTrue(allyClash.TryProcessClash(enemyCollider));
                }

                Assert.AreEqual(prewarmedCount, allyClash.ContactDamageVfxPoolSize);
                Assert.AreEqual(prewarmedCount, allyClash.ActiveContactDamageVfxCount);

                allyObject.SetActive(false);
                Assert.AreEqual(
                    prewarmedCount,
                    enemyClash.ActiveContactDamageVfxCount,
                    "World-space contact VFX should finish independently when the source actor is pooled.");
                Assert.AreEqual(prewarmedCount, allyClash.ContactDamageVfxPoolSize);
            }
            finally
            {
                if (SpatialOneShotVfxPool.ActiveInstance != null)
                {
                    Object.DestroyImmediate(SpatialOneShotVfxPool.ActiveInstance.gameObject);
                }

                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(allyObject);
                Object.DestroyImmediate(contactVfxPrefab);
            }
        }

        [Test]
        public void SummonFrontlineProxyPresenterLocksInitialAdvanceDuringSpawnPresentation()
        {
            GameObject proxyObject = new GameObject("SpawnLockedSummonProxy");
            try
            {
                SummonFrontlineProxy proxy = proxyObject.AddComponent<SummonFrontlineProxy>();
                SummonFrontlineProxyPresenter presenter = proxyObject.AddComponent<SummonFrontlineProxyPresenter>();
                presenter.ConfigurePresentation(proxy, null, System.Array.Empty<Renderer>());
                SetPrivateInstanceField(presenter, "lockAdvanceDuringSpawnState", false);
                SetPrivateInstanceField(presenter, "spawnMovementLockSeconds", 0.22f);

                proxy.Activate(
                    Vector3.zero,
                    Vector3.forward,
                    1,
                    0f,
                    1f,
                    Vector3.forward * 2f,
                    1f,
                    120f,
                    3f);

                Vector3 positionBeforeTick = proxy.transform.position;
                presenter.RefreshNow();
                proxy.Tick(0.1f);

                Assert.AreEqual(
                    positionBeforeTick.z,
                    proxy.transform.position.z,
                    0.001f,
                    "Summon proxies should not slide forward during the first spawn presentation beat.");
                Assert.AreEqual(
                    SummonFrontlineProxyState.Spawned,
                    proxy.CurrentState,
                    "Spawn presentation lock should hold the gameplay proxy out of Advancing until the readable entry beat clears.");
                Assert.AreEqual(
                    SummonFrontlineProxyActionPhase.Entry,
                    proxy.ActionPhase,
                    "The common summon action contract should expose the spawn presentation lock as Entry instead of Locomotion.");
            }
            finally
            {
                Object.DestroyImmediate(proxyObject);
            }
        }

        [Test]
        public void SummonFrontlineProxyPresenterShowsAttackDamageAndDeathFeedback()
        {
            GameObject proxyObject = new GameObject("SummonProxy");
            CombatHealth health = proxyObject.AddComponent<CombatHealth>();
            health.ConfigureTeam(DamageTeam.AllySummon);
            health.ResetHealthToFull();
            SummonFrontlineProxy proxy = proxyObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(health);

            GameObject pulseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulseObject.name = "TierPulseCore";
            pulseObject.transform.SetParent(proxyObject.transform, worldPositionStays: false);
            Collider pulseCollider = pulseObject.GetComponent<Collider>();
            Object.DestroyImmediate(pulseCollider);
            Renderer pulseRenderer = pulseObject.GetComponent<Renderer>();
            GameObject visualObject = new GameObject("SummonSlot1Visual_ShieldBreakerElite");
            visualObject.transform.SetParent(proxyObject.transform, worldPositionStays: false);
            Animator animator = visualObject.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ShieldBreakerEliteAnimatorControllerPath);
            Assert.IsNotNull(animator.runtimeAnimatorController);

            SummonFrontlineProxyPresenter presenter = proxyObject.AddComponent<SummonFrontlineProxyPresenter>();
            presenter.ConfigurePresentation(proxy, pulseObject.transform, new[] { pulseRenderer });
            presenter.ConfigureAnimator(animator);

            proxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 2f, 2f, 120f, 1f);
            presenter.RefreshNow();
            Assert.IsTrue(presenter.IsShowing);
            Assert.AreEqual(1, presenter.AnimatorSpawnTriggerCount);
            Assert.Greater(presenter.AnimatorMoveSpeedSetCount, 0);
            Assert.Greater(
                animator.GetFloat(presenter.MoveSpeedParameter),
                0f,
                "Advancing summon proxies should drive the promoted visual walk read.");

            proxy.NotifyAttackPerformed(0.2f);
            presenter.RefreshNow();
            Assert.AreEqual(
                1,
                presenter.AttackFlashCount,
                "Summon attack state should produce an in-world attack flash instead of only HUD state.");
            Assert.AreEqual(1, presenter.AnimatorAttackTriggerCount);
            Assert.AreEqual(
                0f,
                animator.GetFloat(presenter.MoveSpeedParameter),
                0.001f,
                "Attack state should stop the promoted visual walk read.");

            Assert.IsTrue(health.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                30f,
                Vector3.zero,
                Vector3.back,
                0f)));
            Assert.AreEqual(1, presenter.DamageFlashCount);
            Assert.AreEqual(0, presenter.AnimatorHitTriggerCount);

            Assert.IsTrue(health.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                999f,
                Vector3.zero,
                Vector3.back,
                0f)));
            Assert.IsFalse(proxy.IsActive);
            Assert.IsTrue(proxy.IsPresentationVisible);
            presenter.RefreshNow();
            Assert.IsTrue(presenter.IsShowing);
            Assert.AreEqual(
                1,
                presenter.DeathFlashCount,
                "Defeated summons should linger briefly for death feedback before the pooled actor hides.");
            Assert.AreEqual(1, presenter.AnimatorDeathTriggerCount);
            Assert.AreEqual(0f, animator.GetFloat(presenter.MoveSpeedParameter), 0.001f);

            proxy.Tick(1.1f);
            Assert.IsFalse(proxy.IsPresentationVisible);

            Object.DestroyImmediate(proxyObject);
        }

        [Test]
        public void SummonFrontlineProxyPresenterHonorsDamageResponsePolicy()
        {
            GameObject victimObject = new GameObject("VictimSummonProxy");
            CombatHealth victimHealth = victimObject.AddComponent<CombatHealth>();
            victimHealth.ConfigureTeam(DamageTeam.AllySummon);
            victimHealth.ResetHealthToFull();
            SummonFrontlineProxy victimProxy = victimObject.AddComponent<SummonFrontlineProxy>();
            victimProxy.ConfigureHealth(victimHealth);

            GameObject pulseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulseObject.name = "TierPulseCore";
            pulseObject.transform.SetParent(victimObject.transform, worldPositionStays: false);
            Collider pulseCollider = pulseObject.GetComponent<Collider>();
            Object.DestroyImmediate(pulseCollider);
            Renderer pulseRenderer = pulseObject.GetComponent<Renderer>();
            GameObject visualObject = new GameObject("SummonSlot1Visual_ShieldBreakerElite");
            visualObject.transform.SetParent(victimObject.transform, worldPositionStays: false);
            Animator animator = visualObject.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ShieldBreakerEliteAnimatorControllerPath);
            Assert.IsNotNull(animator.runtimeAnimatorController);

            SummonFrontlineProxyPresenter presenter = victimObject.AddComponent<SummonFrontlineProxyPresenter>();
            presenter.ConfigurePresentation(victimProxy, pulseObject.transform, new[] { pulseRenderer });
            presenter.ConfigureAnimator(animator);
            victimProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 2f, 2f, 120f, 1f);
            presenter.RefreshNow();

            GameObject sourceObject = new GameObject("EnemySummonProxy");
            CombatHealth sourceHealth = sourceObject.AddComponent<CombatHealth>();
            sourceHealth.ConfigureTeam(DamageTeam.Enemy);
            sourceHealth.ResetHealthToFull();
            SummonFrontlineProxy sourceProxy = sourceObject.AddComponent<SummonFrontlineProxy>();
            sourceProxy.ConfigureHealth(sourceHealth);

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                12f,
                Vector3.zero,
                Vector3.back,
                0f)));
            Assert.AreEqual(1, presenter.DamageFlashCount);
            Assert.AreEqual(
                0,
                presenter.AnimatorHitTriggerCount,
                "Default damage should flash the summon body without forcing a full-body hit animation.");

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                12f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.Default,
                CombatControlLockPolicy.None)));
            Assert.AreEqual(2, presenter.DamageFlashCount);
            Assert.AreEqual(
                0,
                presenter.AnimatorHitTriggerCount,
                "Default-looking pressure damage should keep body flash but not play a full-body hit animation unless it also declares action lock.");
            Assert.AreEqual(DamageResponsePolicy.Default, presenter.LastDamageResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, presenter.LastDamageControlLockPolicy);

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                12f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.FlashOnly)));
            Assert.AreEqual(3, presenter.DamageFlashCount);
            Assert.AreEqual(
                0,
                presenter.AnimatorHitTriggerCount,
                "Flash-only damage should keep readability without forcing a full-body hit animation.");

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                12f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None)));
            Assert.AreEqual(
                3,
                presenter.DamageFlashCount,
                "Damage-only policy should stay out of presentation hooks.");
            Assert.AreEqual(0, presenter.AnimatorHitTriggerCount);

            Object.DestroyImmediate(sourceObject);
            Object.DestroyImmediate(victimObject);
        }

        [Test]
        public void SummonFrontlineProxyPresenterBudgetsRepeatedFullBodyHitReactions()
        {
            GameObject victimObject = new GameObject("VictimSummonProxy");
            CombatHealth victimHealth = victimObject.AddComponent<CombatHealth>();
            victimHealth.ConfigureTeam(DamageTeam.AllySummon);
            victimHealth.ResetHealthToFull();
            SummonFrontlineProxy victimProxy = victimObject.AddComponent<SummonFrontlineProxy>();
            victimProxy.ConfigureHealth(victimHealth);

            GameObject pulseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulseObject.name = "TierPulseCore";
            pulseObject.transform.SetParent(victimObject.transform, worldPositionStays: false);
            Collider pulseCollider = pulseObject.GetComponent<Collider>();
            Object.DestroyImmediate(pulseCollider);
            Renderer pulseRenderer = pulseObject.GetComponent<Renderer>();
            GameObject visualObject = new GameObject("SummonSlot1Visual_ShieldBreakerElite");
            visualObject.transform.SetParent(victimObject.transform, worldPositionStays: false);
            Animator animator = visualObject.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ShieldBreakerEliteAnimatorControllerPath);
            Assert.IsNotNull(animator.runtimeAnimatorController);

            SummonFrontlineProxyPresenter presenter = victimObject.AddComponent<SummonFrontlineProxyPresenter>();
            presenter.ConfigurePresentation(victimProxy, pulseObject.transform, new[] { pulseRenderer });
            presenter.ConfigureAnimator(animator);
            victimProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 2f, 2f, 120f, 1f);
            presenter.RefreshNow();

            GameObject sourceObject = new GameObject("EnemySummonProxy");
            CombatHealth sourceHealth = sourceObject.AddComponent<CombatHealth>();
            sourceHealth.ConfigureTeam(DamageTeam.Enemy);
            sourceHealth.ResetHealthToFull();

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                10f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.Stagger,
                CombatControlLockPolicy.InterruptAction)));
            Assert.AreEqual(1, presenter.DamageFlashCount);
            Assert.AreEqual(0, presenter.AnimatorHitTriggerCount);
            Assert.AreEqual(0, presenter.SuppressedAnimatorHitTriggerCount);
            Assert.IsFalse(presenter.LastFullBodyHitReactionSuppressed);

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                10f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.Stagger,
                CombatControlLockPolicy.InterruptAction)));
            Assert.AreEqual(
                2,
                presenter.DamageFlashCount,
                "Repeated hits should keep rendering material flash while hit animation stays gated.");
            Assert.AreEqual(
                0,
                presenter.AnimatorHitTriggerCount,
                "Routine repeated hits should not keep forcing the summon back into full-body hit animation.");
            Assert.AreEqual(0, presenter.SuppressedAnimatorHitTriggerCount);
            Assert.IsFalse(presenter.LastFullBodyHitReactionSuppressed);

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                sourceHealth,
                DamageTeam.Enemy,
                10f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.Break,
                CombatControlLockPolicy.HardLock)));
            Assert.AreEqual(3, presenter.DamageFlashCount);
            Assert.AreEqual(
                0,
                presenter.AnimatorHitTriggerCount,
                "Major authored reactions should keep the material flash without cutting through suppressed hit animation feedback.");
            Assert.AreEqual(0, presenter.SuppressedAnimatorHitTriggerCount);
            Assert.IsFalse(presenter.LastFullBodyHitReactionSuppressed);

            Object.DestroyImmediate(sourceObject);
            Object.DestroyImmediate(victimObject);
        }

        [Test]
        public void SummonFrontlineProxyPresenterSoftensNonLockingDamageVfx()
        {
            GameObject victimObject = new GameObject("VictimSummonProxy");
            GameObject damageCuePrefab = new GameObject("SummonDamageCuePrefab");
            CombatVfxCueProfile cueProfile = null;
            try
            {
                CombatHealth victimHealth = victimObject.AddComponent<CombatHealth>();
                victimHealth.ConfigureTeam(DamageTeam.AllySummon);
                victimHealth.ResetHealthToFull();
                SummonFrontlineProxy victimProxy = victimObject.AddComponent<SummonFrontlineProxy>();
                victimProxy.ConfigureHealth(victimHealth);

                GameObject pulseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pulseObject.name = "TierPulseCore";
                pulseObject.transform.SetParent(victimObject.transform, worldPositionStays: false);
                Collider pulseCollider = pulseObject.GetComponent<Collider>();
                Object.DestroyImmediate(pulseCollider);
                Renderer pulseRenderer = pulseObject.GetComponent<Renderer>();

                GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bodyObject.name = "PromotedSummonBody";
                bodyObject.transform.SetParent(victimObject.transform, worldPositionStays: false);
                bodyObject.transform.localPosition = new Vector3(0f, 1.25f, 0f);
                Collider bodyCollider = bodyObject.GetComponent<Collider>();
                Object.DestroyImmediate(bodyCollider);

                CombatVfxCuePlayer cuePlayer = victimObject.AddComponent<CombatVfxCuePlayer>();
                cueProfile = CreateSummonDamageVfxCueProfile(damageCuePrefab);
                ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);

                SummonFrontlineProxyPresenter presenter = victimObject.AddComponent<SummonFrontlineProxyPresenter>();
                presenter.ConfigurePresentation(victimProxy, pulseObject.transform, new[] { pulseRenderer });
                presenter.ConfigureVfxCuePlayer(cuePlayer, victimObject.transform, null);
                victimProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 2f, 2f, 120f, 1f);
                presenter.RefreshNow();

                Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Enemy,
                    12f,
                    Vector3.zero,
                    Vector3.back,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None)));

                float expectedIntensity = 0.9f * presenter.PressureDamageCueScale;

                Assert.AreEqual(1, presenter.DamageVfxCueRequestCount);
                Assert.Greater(
                    presenter.DamageFlashRendererCount,
                    0,
                    "Summon damage VFX should resolve promoted body renderers rather than the hidden tier pulse.");
                Assert.IsNotNull(presenter.DamageVfxAnchor);
                Assert.Greater(
                    presenter.DamageVfxAnchor.position.y,
                    victimObject.transform.position.y + 0.4f,
                    "Summon damage VFX should spawn around the torso/body bounds instead of the floor root.");
                Assert.AreEqual(DamageResponsePolicy.FlashOnly, presenter.LastDamageResponsePolicy);
                Assert.AreEqual(CombatControlLockPolicy.None, presenter.LastDamageControlLockPolicy);
                Assert.IsFalse(presenter.LastDamageCueInterruptedAction);
                Assert.AreEqual(presenter.PressureDamageCueScale, presenter.LastDamageCuePolicyScale, 0.001f);
                Assert.AreEqual(expectedIntensity, presenter.LastDamageCueIntensity, 0.001f);
                Transform damageCue = presenter.DamageVfxAnchor.Find(damageCuePrefab.name);
                Assert.IsNotNull(damageCue);
            }
            finally
            {
                if (cueProfile != null)
                {
                    Object.DestroyImmediate(cueProfile);
                }

                Object.DestroyImmediate(damageCuePrefab);
                Object.DestroyImmediate(victimObject);
            }
        }

        [Test]
        public void DamageModificationPreservesDamageResponseAndControlLockPolicies()
        {
            GameObject victimObject = new GameObject("PolicyPreservingVictim");
            CombatHealth victimHealth = victimObject.AddComponent<CombatHealth>();
            victimHealth.ConfigureTeam(DamageTeam.AllySummon);
            victimHealth.ResetHealthToFull();

            DamageInfo? resolvedDamageInfo = null;
            victimHealth.DamageModifying += context => context.ScaleAmount(0.5f);
            victimHealth.Damaged += damageInfo => resolvedDamageInfo = damageInfo;

            Assert.IsTrue(victimHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                20f,
                Vector3.zero,
                Vector3.back,
                0f,
                DamageResponsePolicy.Break,
                CombatControlLockPolicy.HardLock)));

            Assert.IsTrue(resolvedDamageInfo.HasValue);
            Assert.AreEqual(10f, resolvedDamageInfo.Value.Amount, 0.001f);
            Assert.AreEqual(DamageResponsePolicy.Break, resolvedDamageInfo.Value.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.HardLock, resolvedDamageInfo.Value.ControlLockPolicy);

            Object.DestroyImmediate(victimObject);
        }

        [Test]
        public void BasicSoldierOnlyStaggersWhenDamageDeclaresControlLock()
        {
            GameObject soldierObject = new GameObject("ControlLockPolicySoldier");
            BasicSoldierEnemy soldier = soldierObject.AddComponent<BasicSoldierEnemy>();
            CombatHealth soldierHealth = soldierObject.GetComponent<CombatHealth>();
            soldierHealth.ConfigureTeam(DamageTeam.Enemy);
            soldierHealth.ResetHealthToFull();

            Assert.AreEqual(CombatAiPatternState.Tracking, soldier.CurrentPatternState);
            Assert.IsTrue(soldierHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                8f,
                Vector3.zero,
                Vector3.forward,
                0f,
                DamageResponsePolicy.FlashOnly,
                CombatControlLockPolicy.None)));
            Assert.AreEqual(
                CombatAiPatternState.Tracking,
                soldier.CurrentPatternState,
                "Non-locking damage should not force the enemy out of its current action state.");

            Assert.IsTrue(soldierHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                8f,
                Vector3.zero,
                Vector3.forward,
                0f,
                DamageResponsePolicy.Stagger,
                CombatControlLockPolicy.InterruptAction)));
            Assert.AreEqual(
                CombatAiPatternState.Stagger,
                soldier.CurrentPatternState,
                "Damage that declares an action lock should still drive the authored stagger state.");

            Object.DestroyImmediate(soldierObject);
        }

        [Test]
        public void SummonFrontlineClashAutoScansNearbyHostileAndHoldsBothActors()
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
            allyClash.ConfigureTuning(100f, 0.2f, 0f, 0.3f, 1.1f);

            GameObject enemyObject = new GameObject("EnemySummonActor");
            enemyObject.transform.position = Vector3.forward * 0.7f;
            SphereCollider enemyCollider = enemyObject.AddComponent<SphereCollider>();
            enemyCollider.isTrigger = true;
            enemyCollider.center = new Vector3(0f, 0.9f, 0f);
            enemyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ResetHealthToFull();
            SummonFrontlineProxy enemyProxy = enemyObject.AddComponent<SummonFrontlineProxy>();
            enemyProxy.ConfigureHealth(enemyHealth);

            allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 4f, 4f, 140f, 1f);
            enemyProxy.Activate(enemyObject.transform.position, Vector3.back, 1, 0f, 1f, 4f, 4f, 120f, 1f);
            Physics.SyncTransforms();

            allyClash.Tick(0.1f);

            Assert.Less(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(enemyProxy.IsAdvanceHeld);
            Assert.AreEqual(SummonFrontlineProxyState.Attacking, allyProxy.CurrentState);
            Assert.AreEqual(SummonFrontlineProxyState.Attacking, enemyProxy.CurrentState);
            Assert.AreEqual(1, allyClash.TotalClashCount);
            Assert.AreEqual(1, allyClash.ContactScanCount);
            Assert.AreEqual(DamageTeam.Enemy, allyClash.LastOpponentTeam);
            Assert.AreEqual(SummonFrontlineClashTargetKind.HostileSummon, allyClash.LastTargetKind);

            for (int tickIndex = 0; tickIndex < 5; tickIndex++)
            {
                allyClash.Tick(0.01f);
            }

            Assert.AreEqual(
                1,
                allyClash.ContactScanCount,
                "Short frame ticks should reuse the recent contact scan instead of querying physics every frame.");
            allyClash.Tick(0.04f);
            Assert.AreEqual(2, allyClash.ContactScanCount);

            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(allyObject);
        }

        [Test]
        public void SummonFrontlineClashSweepsBetweenScanPositions()
        {
            GameObject allyObject = new GameObject("SweptAllySummonActor");
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
            allyClash.ConfigureTuning(100f, 0.2f, 0f, 0.3f, 0.45f);

            GameObject enemyObject = new GameObject("SweptEnemySummonActor");
            SphereCollider enemyCollider = enemyObject.AddComponent<SphereCollider>();
            enemyCollider.isTrigger = true;
            enemyCollider.center = new Vector3(0f, 0.9f, 0f);
            enemyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ResetHealthToFull();
            SummonFrontlineProxy enemyProxy = enemyObject.AddComponent<SummonFrontlineProxy>();
            enemyProxy.ConfigureHealth(enemyHealth);

            Vector3 allyStart = Vector3.back * 2f;
            Vector3 allyEnd = Vector3.forward * 2f;
            allyProxy.Activate(allyStart, Vector3.forward, 1, 0f, 1f, 4f, 4f, 140f, 1f);
            enemyProxy.Activate(Vector3.zero, Vector3.back, 1, 0f, 1f, 4f, 4f, 120f, 1f);
            Physics.SyncTransforms();

            allyClash.Tick(0.1f);
            Assert.AreEqual(0, allyClash.TotalClashCount);

            allyObject.transform.position = allyEnd;
            Physics.SyncTransforms();
            allyClash.Tick(0.1f);

            Assert.AreEqual(1, allyClash.TotalClashCount);
            Assert.Less(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
            Assert.AreEqual(SummonFrontlineClashTargetKind.HostileSummon, allyClash.LastTargetKind);
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(enemyProxy.IsAdvanceHeld);

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
            DamageInfo? bossDamageInfo = null;
            bossHealth.Damaged += damageInfo => bossDamageInfo = damageInfo;

            allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, Vector3.forward * 4f, 3f);

            Assert.IsTrue(allyClash.TryProcessClash(bossCollider));
            Assert.Less(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            Assert.IsTrue(bossDamageInfo.HasValue);
            Assert.AreEqual(DamageResponsePolicy.Default, bossDamageInfo.Value.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.InterruptAction, bossDamageInfo.Value.ControlLockPolicy);
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(allyClash.IsClashing);
            Assert.AreEqual(1, allyClash.TotalClashCount);
            Assert.AreEqual(0, allyClash.LastOpponentTier);
            Assert.AreEqual(DamageTeam.Enemy, allyClash.LastOpponentTeam);
            Assert.AreEqual(SummonFrontlineClashTargetKind.HostileBody, allyClash.LastTargetKind);

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(allyObject);
        }

        [Test]
        public void SummonFrontlineClashCapsPlayerBodyContactDamage()
        {
            GameObject enemyObject = new GameObject("EnemySummonActor");
            SphereCollider enemyCollider = enemyObject.AddComponent<SphereCollider>();
            enemyCollider.isTrigger = true;
            enemyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth enemyHealth = enemyObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ResetHealthToFull();
            SummonFrontlineProxy enemyProxy = enemyObject.AddComponent<SummonFrontlineProxy>();
            enemyProxy.ConfigureHealth(enemyHealth);
            SummonFrontlineClash enemyClash = enemyObject.AddComponent<SummonFrontlineClash>();
            enemyClash.ConfigureReferences(enemyProxy, enemyHealth);
            enemyClash.ConfigureTuning(200f, 0.2f, 0f, 0.3f);

            GameObject playerObject = new GameObject("PlayerBodyTarget");
            CharacterController playerCollider = playerObject.AddComponent<CharacterController>();
            PlayerMovementController movementController = playerObject.AddComponent<PlayerMovementController>();
            movementController.enabled = false;
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            playerHealth.ConfigureMaxHealth(120f);
            DamageInfo? playerDamageInfo = null;
            playerHealth.Damaged += damageInfo => playerDamageInfo = damageInfo;

            enemyProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, Vector3.forward * 4f, 3f);

            Assert.IsTrue(enemyClash.TryProcessClash(playerCollider));
            Assert.IsTrue(playerHealth.IsAlive);
            Assert.AreEqual(116f, playerHealth.CurrentHealth, 0.001f);
            Assert.IsTrue(playerDamageInfo.HasValue);
            Assert.AreEqual(DamageResponsePolicy.FlashOnly, playerDamageInfo.Value.ResponsePolicy);
            Assert.AreEqual(CombatControlLockPolicy.None, playerDamageInfo.Value.ControlLockPolicy);
            Assert.AreEqual(4f, enemyClash.LastDamageAmount, 0.001f);
            Assert.AreEqual(SummonFrontlineClashTargetKind.HostileBody, enemyClash.LastTargetKind);

            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(enemyObject);
        }

        [Test]
        public void SummonFrontlineClashPrioritizesHostileSummonBeforeBodyTarget()
        {
            GameObject allyObject = new GameObject("AllySummonActor");
            SphereCollider allyCollider = allyObject.AddComponent<SphereCollider>();
            allyCollider.isTrigger = true;
            allyCollider.center = new Vector3(0f, 0.9f, 0f);
            allyObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth allyHealth = allyObject.AddComponent<CombatHealth>();
            allyHealth.ConfigureTeam(DamageTeam.AllySummon);
            allyHealth.ResetHealthToFull();
            SummonFrontlineProxy allyProxy = allyObject.AddComponent<SummonFrontlineProxy>();
            allyProxy.ConfigureHealth(allyHealth);
            SummonFrontlineClash allyClash = allyObject.AddComponent<SummonFrontlineClash>();
            allyClash.ConfigureReferences(allyProxy, allyHealth);
            allyClash.ConfigureTuning(100f, 0.2f, 0f, 0.3f, 1.2f);

            GameObject bossObject = new GameObject("BossBodyTarget");
            bossObject.transform.position = Vector3.forward * 0.35f;
            SphereCollider bossCollider = bossObject.AddComponent<SphereCollider>();
            bossCollider.center = new Vector3(0f, 0.9f, 0f);
            bossObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            bossHealth.ResetHealthToFull();

            GameObject enemySummonObject = new GameObject("EnemySummonActor");
            enemySummonObject.transform.position = Vector3.forward * 0.75f;
            SphereCollider enemyCollider = enemySummonObject.AddComponent<SphereCollider>();
            enemyCollider.isTrigger = true;
            enemyCollider.center = new Vector3(0f, 0.9f, 0f);
            enemySummonObject.AddComponent<Rigidbody>().isKinematic = true;
            CombatHealth enemyHealth = enemySummonObject.AddComponent<CombatHealth>();
            enemyHealth.ConfigureTeam(DamageTeam.Enemy);
            enemyHealth.ResetHealthToFull();
            SummonFrontlineProxy enemyProxy = enemySummonObject.AddComponent<SummonFrontlineProxy>();
            enemyProxy.ConfigureHealth(enemyHealth);

            allyProxy.Activate(Vector3.zero, Vector3.forward, 1, 0f, 1f, 4f, 4f, 140f, 1f);
            enemyProxy.Activate(enemySummonObject.transform.position, Vector3.back, 1, 0f, 1f, 4f, 4f, 120f, 1f);
            Physics.SyncTransforms();

            allyClash.Tick(0.1f);

            Assert.Less(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
            Assert.AreEqual(
                bossHealth.MaxHealth,
                bossHealth.CurrentHealth,
                0.001f,
                "A hostile summon inside engage range should block body/boss damage priority.");
            Assert.AreEqual(SummonFrontlineClashTargetKind.HostileSummon, allyClash.LastTargetKind);
            Assert.IsTrue(allyProxy.IsAdvanceHeld);
            Assert.IsTrue(enemyProxy.IsAdvanceHeld);
            Assert.AreEqual(
                Quaternion.LookRotation(Vector3.forward, Vector3.up).eulerAngles.y,
                allyObject.transform.rotation.eulerAngles.y,
                0.001f,
                "The clashing ally summon should face the hostile summon instead of staying visually detached.");

            Object.DestroyImmediate(enemySummonObject);
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
            summonAction.ConfigureSlotCooldown(0f);
            SetPrivateInstanceField(summonAction, "summonActorSpawnDelaySeconds", 0f);

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
                if (proxies[i] != null
                    && proxies[i].IsActive
                    && proxies[i].transform.IsChildOf(playerObject.transform))
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
            Assert.AreEqual(230f, activeProxy.MaxHealth, 0.001f);
            Assert.AreEqual(1.45f, activeProxy.ActiveMoveSpeed, 0.001f);
            Assert.AreEqual(SummonFrontlineProxyState.Advancing, activeProxy.CurrentState);
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
                if (proxies[i] != null
                    && proxies[i].IsActive
                    && proxies[i].transform.IsChildOf(playerObject.transform))
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
            summonAction.ConfigureSlotCooldown(0f);
            SetPrivateInstanceField(summonAction, "summonActorSpawnDelaySeconds", 0f);

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
        public void BossPressureCostKeepsRiskBandCurrentWhileGainIsDisabled()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject bossObject = new GameObject("Boss");
            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);

            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ);
            bossCost.Tick(1f);
            float costBeforeDisable = bossCost.CurrentTierCost;
            bossCost.SetGainEnabled(false);
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.ForwardBoundaryZ);
            bossCost.Tick(1f);

            Assert.AreEqual(BossPressureRiskBand.ForwardCommit, bossCost.CurrentRiskBand);
            Assert.AreEqual(costBeforeDisable, bossCost.CurrentTierCost, 0.001f);

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
            SetPrivateInstanceField(positionController, "forwardPressureOscillationEnabled", false);
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

            Assert.AreEqual(0.18f, restRisk, 0.001f);
            Assert.Greater(buildingRisk, restRisk);
            Assert.Greater(readyRisk, buildingRisk);
            Assert.Less(disabledRisk, readyRisk);

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressurePositionControllerStrafesAfterObservedBasicFire()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ, 1.6f);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            GameObject projectilePrefabObject = new GameObject("BossBasicProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab =
                projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBasicFireProfile fireProfile = ScriptableObject.CreateInstance<BossBasicFireProfile>();
            BossBasicFireEmitter basicFireEmitter = bossObject.AddComponent<BossBasicFireEmitter>();
            basicFireEmitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            basicFireEmitter.ConfigureProfile(fireProfile, projectilePrefab, 2);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            BossBarrageEmitter barrageEmitter = bossObject.AddComponent<BossBarrageEmitter>();
            barrageEmitter.ConfigureReferences(lane, playerObject.transform, bossHealth);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(
                bossCost,
                barrageEmitter,
                null,
                lane,
                playerObject.transform,
                basicFireEmitter);

            BossPressurePositionController positionController =
                bossObject.AddComponent<BossPressurePositionController>();
            positionController.ConfigureReferences(lane, bossCost, director, bossObject.transform);
            SetPrivateInstanceField(positionController, "forwardPressureOscillationEnabled", false);

            int firedCount = basicFireEmitter.FireVolley();
            positionController.Tick(0.1f);

            Assert.AreEqual(fireProfile.ProjectilesPerVolley, firedCount);
            Assert.AreEqual(1, director.TotalBasicShotVolleys);
            Assert.AreEqual(firedCount, director.LastBasicShotProjectileCount);
            Assert.AreEqual(0f, director.LastBasicShotAgeSeconds, 0.001f);
            Assert.AreEqual(
                0.52f,
                positionController.CurrentTargetRisk01,
                0.001f,
                "A boss that has just fired basic shots should strafe instead of reading as a static turret.");
            Assert.Greater(
                Mathf.Abs(lane.GetLaneCoordinates(bossObject.transform.position).x),
                0.01f,
                "The basic-fire strafe intent should visibly move the boss laterally.");

            Object.DestroyImmediate(fireProfile);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressurePositionControllerTracksPlayerLateralPosition()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(lane.HalfWidth * 0.72f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ, 1.6f);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            BossPressurePositionController positionController =
                bossObject.AddComponent<BossPressurePositionController>();
            positionController.ConfigureReferences(
                lane,
                bossCost,
                director,
                bossObject.transform,
                playerObject.transform);

            positionController.Tick(0.5f);
            float rightResponseX = lane.GetLaneCoordinates(bossObject.transform.position).x;

            playerObject.transform.position = lane.GetLaneWorldPoint(-lane.HalfWidth * 0.72f, lane.BackLimitZ);
            positionController.Tick(1f);
            float leftResponseX = lane.GetLaneCoordinates(bossObject.transform.position).x;

            Assert.Greater(
                rightResponseX,
                0.25f,
                "Boss pressure movement should answer the player's lateral lane instead of idling at center.");
            Assert.Less(
                leftResponseX,
                rightResponseX - 0.5f,
                "Boss pressure movement should re-aim its strafe when the player crosses the lane.");

            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
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
        public void BossPressureActionDirectorSuppressesBasicFireDuringCostedAction()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossBasicFireEmitter basicFireEmitter = bossObject.AddComponent<BossBasicFireEmitter>();
            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(100f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, null, lane, playerObject.transform, basicFireEmitter);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    specialPattern,
                    BossPressureActionKind.SpecialSkill,
                    1,
                    1,
                    0f)
            });

            Assert.IsFalse(basicFireEmitter.IsAutoFireSuppressed);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreEqual(BossPressureActionKind.SpecialSkill, director.LastActionKind);
            Assert.IsTrue(
                basicFireEmitter.IsAutoFireSuppressed,
                "A committed boss pressure action should briefly hold regular basic fire so both reads do not overlap.");
            Assert.AreEqual(
                director.BasicFireSuppressionSecondsAfterPressureAction,
                basicFireEmitter.AutoFireSuppressionRemaining,
                0.001f);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(specialPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorRequiresBasicFireVolleysBeforeCostedAction()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBasicFireProfile basicFireProfile = ScriptableObject.CreateInstance<BossBasicFireProfile>();
            GameObject projectilePrefabObject = new GameObject("BossProjectilePrefab");
            projectilePrefabObject.AddComponent<SphereCollider>();
            projectilePrefabObject.AddComponent<Rigidbody>();
            BossBarrageProjectile projectilePrefab = projectilePrefabObject.AddComponent<BossBarrageProjectile>();
            projectilePrefabObject.SetActive(false);

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(basePattern, projectilePrefab, basePattern.ProjectilesPerWave * 2);

            BossBasicFireEmitter basicFireEmitter = bossObject.AddComponent<BossBasicFireEmitter>();
            basicFireEmitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            basicFireEmitter.ConfigureProfile(basicFireProfile, projectilePrefab, 4);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(100f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, null, lane, playerObject.transform, basicFireEmitter);
            director.ConfigureBasicFireRhythmGate(3, 0f);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    specialPattern,
                    BossPressureActionKind.SpecialSkill,
                    1,
                    1,
                    0f)
            });

            Assert.IsFalse(
                director.TryQueueBestAvailableAction(),
                "The boss should not spend into a skill before regular rifle fire has established the combat rhythm.");

            Assert.AreEqual(1, basicFireEmitter.FireVolley());
            Assert.AreEqual(1, director.BasicFireVolleysSinceLastPressureAction);
            Assert.IsFalse(director.TryQueueBestAvailableAction());

            Assert.AreEqual(1, basicFireEmitter.FireVolley());
            Assert.AreEqual(1, basicFireEmitter.FireVolley());
            Assert.AreEqual(3, director.BasicFireVolleysSinceLastPressureAction);
            Assert.IsTrue(director.IsBasicFireRhythmGateOpen);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreEqual(BossPressureActionKind.SpecialSkill, director.LastActionKind);
            Assert.AreEqual(0, director.BasicFireVolleysSinceLastPressureAction);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(basicFireProfile);
            Object.DestroyImmediate(specialPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorUsesThinkIntervalBetweenAutomaticDecisions()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            SerializedObject serializedDirector = new SerializedObject(director);
            serializedDirector.FindProperty("globalRecoverySeconds").floatValue = 0f;
            serializedDirector.FindProperty("decisionThinkIntervalSeconds").floatValue = 0.25f;
            serializedDirector.ApplyModifiedPropertiesWithoutUndo();
            director.ConfigureReferences(bossCost, emitter);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    specialPattern,
                    BossPressureActionKind.SpecialSkill,
                    1,
                    1,
                    0f)
            });

            director.Tick(0.1f);

            Assert.AreEqual(1, director.TotalActionCount);
            Assert.AreEqual(0.25f, director.DecisionThinkIntervalSeconds, 0.001f);
            Assert.Greater(director.DecisionThinkRemainingSeconds, 0f);
            emitter.CancelQueuedPriorityPattern(specialPattern);
            bossCost.GrantCurrentTierCost(100f);

            director.Tick(0.1f);

            Assert.AreEqual(
                1,
                director.TotalActionCount,
                "Automatic boss decisions should wait for the authored think interval even when cost and slots are ready.");

            director.Tick(0.2f);

            Assert.AreEqual(2, director.TotalActionCount);
            Assert.AreSame(specialPattern, emitter.QueuedPriorityPattern);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(specialPattern);
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
        public void BossPressureActionDirectorDoesNotReplaceActiveBossPressureSummon()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            bossCost.GrantCurrentTierCost(300f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    specialPattern,
                    BossPressureActionKind.SpecialSkill,
                    1,
                    1,
                    0f,
                    selectionPriority: 20,
                    movementIntent: BossPressureMovementIntent.StrafeFire),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    3,
                    1,
                    0f,
                    selectionPriority: 60,
                    movementIntent: BossPressureMovementIntent.RetreatAndSummon)
            });

            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(summonPattern, emitter.QueuedPriorityPattern);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.AreEqual(1, summonAction.ActiveSummonActorCount);

            emitter.CancelQueuedPriorityPattern(summonPattern);
            bossCost.GrantCurrentTierCost(300f);
            director.Tick(0.5f);

            Assert.AreEqual(
                1,
                summonAction.TotalReleaseCount,
                "An active boss pressure summon should stay in play instead of being replaced by another summon-pressure slot.");
            Assert.AreEqual(BossPressureActionKind.SpecialSkill, director.LastActionKind);
            Assert.AreSame(specialPattern, emitter.QueuedPriorityPattern);
            Assert.IsTrue(director.LastDecisionContext.HasActiveBossPressureSummon);
            Assert.AreEqual(1, director.LastDecisionContext.ActiveBossPressureSummonCount);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(specialPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorMarksPlayerSummonResponseAction()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile skillPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    skillPattern,
                    BossPressureActionKind.SkillPattern,
                    2,
                    1,
                    0f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f,
                    usePlayerSummonResponseGate: true,
                    minimumPlayerSummonTier: 1)
            });

            director.NotifyPlayerSummonFrontlineCreated(2);

            Assert.IsTrue(director.IsPlayerSummonResponseWindowActive);
            Assert.AreEqual(2, director.LastObservedPlayerSummonTier);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(
                summonPattern,
                emitter.QueuedPriorityPattern,
                "A boss answer inside the player-summon response window should prefer the authored summon-pressure slot at the same tier.");
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.IsTrue(director.LastActionRespondedToPlayerSummon);
            Assert.AreEqual(1, director.TotalPlayerSummonResponseCount);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastPlayerSummonResponseKind);
            Assert.AreEqual(2, director.LastPlayerSummonResponseTier);
            Assert.AreEqual(0f, director.PlayerSummonResponseRemainingSeconds, 0.001f);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(skillPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorDoesNotConsumeSummonResponseWindowForUngatedAction()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile proactivePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonResponsePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            bossCost.GrantCurrentTierCost(100f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, null, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    proactivePattern,
                    BossPressureActionKind.SkillPattern,
                    1,
                    1,
                    0f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonResponsePattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f,
                    usePlayerSummonResponseGate: true,
                    minimumPlayerSummonTier: 1)
            });

            director.NotifyPlayerSummonFrontlineCreated(1);

            Assert.IsTrue(director.IsPlayerSummonResponseWindowActive);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(proactivePattern, emitter.QueuedPriorityPattern);
            Assert.AreEqual(BossPressureActionKind.SkillPattern, director.LastActionKind);
            Assert.IsFalse(
                director.LastActionRespondedToPlayerSummon,
                "Only slots authored with the player-summon response gate should consume or count the response window.");
            Assert.AreEqual(0, director.TotalPlayerSummonResponseCount);
            Assert.Greater(director.PlayerSummonResponseRemainingSeconds, 0f);

            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonResponsePattern);
            Object.DestroyImmediate(proactivePattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorDoesNotBoostUngatedSummonPressureDuringSummonResponseWindow()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile punishPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile ungatedSummonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            bossCost.GrantCurrentTierCost(300f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    punishPattern,
                    BossPressureActionKind.PunishOverextend,
                    3,
                    1,
                    0f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    ungatedSummonPattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f)
            });

            director.NotifyPlayerSummonFrontlineCreated(2);

            Assert.IsTrue(director.IsPlayerSummonResponseWindowActive);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(
                punishPattern,
                emitter.QueuedPriorityPattern,
                "A summon-pressure slot should need the explicit response gate before the response window boosts its priority.");
            Assert.AreEqual(BossPressureActionKind.PunishOverextend, director.LastActionKind);
            Assert.IsFalse(director.LastActionRespondedToPlayerSummon);
            Assert.AreEqual(0, director.TotalPlayerSummonResponseCount);
            Assert.Greater(director.PlayerSummonResponseRemainingSeconds, 0f);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(ungatedSummonPattern);
            Object.DestroyImmediate(punishPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorKeepsSummonResponseOnlySlotClosedUntilPlayerSummons()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonResponsePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonResponsePattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f,
                    usePlayerSummonResponseGate: true,
                    minimumPlayerSummonTier: 1)
            });

            Assert.IsFalse(
                director.TryQueueBestAvailableAction(),
                "A boss player-summon response slot should stay closed until the player has created a frontline summon.");
            Assert.AreEqual(2, bossCost.AvailableTier);
            Assert.IsFalse(emitter.HasQueuedPriorityPattern);
            Assert.AreEqual(0, director.TotalActionCount);

            director.NotifyPlayerSummonFrontlineCreated(1);

            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(summonResponsePattern, emitter.QueuedPriorityPattern);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.IsTrue(director.LastActionRespondedToPlayerSummon);
            Assert.AreEqual(1, director.TotalPlayerSummonResponseCount);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonResponsePattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorPrefersSummonResponseOverHigherTierPunish()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile punishPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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
            bossCost.GrantCurrentTierCost(300f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    punishPattern,
                    BossPressureActionKind.PunishOverextend,
                    3,
                    1,
                    0f,
                    usePlayerForwardRiskGate: true,
                    minimumPlayerForwardRisk01: 0.66f,
                    maximumPlayerForwardRisk01: 1f),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    2,
                    1,
                    0f,
                    usePlayerSummonResponseGate: true,
                    minimumPlayerSummonTier: 1)
            });

            director.NotifyPlayerSummonFrontlineCreated(2);

            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(
                summonPattern,
                emitter.QueuedPriorityPattern,
                "A player-summon response window should make boss summon pressure beat a higher-tier overextend punish.");
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.AreEqual(3, director.LastSpentTier);
            Assert.IsTrue(director.LastActionRespondedToPlayerSummon);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastPlayerSummonResponseKind);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(punishPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorSelectsReviewedResponseDeckByContext()
        {
            RunScenario(
                "LV1 backline pressure should use the regular special shot pattern.",
                playerForward: false,
                grantedCost: 100f,
                observedPlayerSummonTier: 0,
                expectedActionKind: BossPressureActionKind.SpecialSkill,
                expectedSlotIndex: 0,
                expectedSpentTier: 1,
                expectedMovementIntent: BossPressureMovementIntent.StrafeFire,
                expectedPositionRisk01: 0.52f,
                expectedRespondsToPlayerSummon: false,
                expectedSummonTier: 0);

            RunScenario(
                "A fresh player summon should pull the boss into the authored response slot before LV3.",
                playerForward: false,
                grantedCost: 200f,
                observedPlayerSummonTier: 2,
                expectedActionKind: BossPressureActionKind.SummonPressure,
                expectedSlotIndex: 2,
                expectedSpentTier: 2,
                expectedMovementIntent: BossPressureMovementIntent.RetreatAndSummon,
                expectedPositionRisk01: 0.1f,
                expectedRespondsToPlayerSummon: true,
                expectedSummonTier: 2);

            RunScenario(
                "LV3 without overextend risk should release the laser summon pressure slot.",
                playerForward: false,
                grantedCost: 300f,
                observedPlayerSummonTier: 0,
                expectedActionKind: BossPressureActionKind.SummonPressure,
                expectedSlotIndex: 3,
                expectedSpentTier: 3,
                expectedMovementIntent: BossPressureMovementIntent.RetreatAndSummon,
                expectedPositionRisk01: 0.1f,
                expectedRespondsToPlayerSummon: false,
                expectedSummonTier: 3);

            RunScenario(
                "LV3 with the player committed forward should pick the overextend punish over more summon pressure.",
                playerForward: true,
                grantedCost: 300f,
                observedPlayerSummonTier: 0,
                expectedActionKind: BossPressureActionKind.PunishOverextend,
                expectedSlotIndex: 4,
                expectedSpentTier: 3,
                expectedMovementIntent: BossPressureMovementIntent.CommitForward,
                expectedPositionRisk01: 0.9f,
                expectedRespondsToPlayerSummon: false,
                expectedSummonTier: 0);

            static void RunScenario(
                string scenarioName,
                bool playerForward,
                float grantedCost,
                int observedPlayerSummonTier,
                BossPressureActionKind expectedActionKind,
                int expectedSlotIndex,
                int expectedSpentTier,
                BossPressureMovementIntent expectedMovementIntent,
                float expectedPositionRisk01,
                bool expectedRespondsToPlayerSummon,
                int expectedSummonTier)
            {
                GameObject laneObject = new GameObject("Lane");
                SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
                GameObject playerObject = new GameObject("Player");
                playerObject.transform.position = lane.GetLaneWorldPoint(
                    0f,
                    playerForward ? lane.ForwardBoundaryZ : lane.BackLimitZ);
                GameObject bossObject = new GameObject("BossProxy");
                bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ);
                CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
                bossHealth.ConfigureTeam(DamageTeam.Enemy);

                BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                BossBarragePatternProfile summonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                BossBarragePatternProfile punishPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();

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

                BossBasicFireEmitter basicFireEmitter = bossObject.AddComponent<BossBasicFireEmitter>();
                BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
                bossCost.ConfigureReferences(lane, bossObject.transform);
                bossCost.GrantCurrentTierCost(grantedCost);

                BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
                director.ConfigureReferences(
                    bossCost,
                    emitter,
                    summonAction,
                    lane,
                    playerObject.transform,
                    basicFireEmitter);
                director.ConfigureActionSlots(new[]
                {
                    new BossPressureActionDirector.BossPressureActionSlot(
                        specialPattern,
                        BossPressureActionKind.SpecialSkill,
                        1,
                        1,
                        0f,
                        responseId: "DodgeBossLinePressureSpecial",
                        selectionPriority: 15,
                        movementIntent: BossPressureMovementIntent.StrafeFire),
                    new BossPressureActionDirector.BossPressureActionSlot(
                        summonPattern,
                        BossPressureActionKind.SummonPressure,
                        1,
                        1,
                        0f,
                        responseId: "BossSummonPressureLV1",
                        selectionPriority: 5,
                        movementIntent: BossPressureMovementIntent.RetreatAndSummon),
                    new BossPressureActionDirector.BossPressureActionSlot(
                        summonPattern,
                        BossPressureActionKind.SummonPressure,
                        2,
                        1,
                        0f,
                        usePlayerSummonResponseGate: true,
                        minimumPlayerSummonTier: 2,
                        responseId: "CounterPlayerSummonWithBossPressure",
                        selectionPriority: 30,
                        summonResponsePriorityBonus: 140,
                        movementIntent: BossPressureMovementIntent.RetreatAndSummon),
                    new BossPressureActionDirector.BossPressureActionSlot(
                        summonPattern,
                        BossPressureActionKind.SummonPressure,
                        3,
                        1,
                        0f,
                        responseId: "LaserSoldierDodgeLine",
                        selectionPriority: 35,
                        movementIntent: BossPressureMovementIntent.RetreatAndSummon),
                    new BossPressureActionDirector.BossPressureActionSlot(
                        punishPattern,
                        BossPressureActionKind.PunishOverextend,
                        3,
                        1,
                        0f,
                        usePlayerForwardRiskGate: true,
                        minimumPlayerForwardRisk01: 0.66f,
                        maximumPlayerForwardRisk01: 1f,
                        responseId: "PunishOverextendCommit",
                        selectionPriority: 80,
                        forwardRiskPriorityBonus: 80,
                        movementIntent: BossPressureMovementIntent.CommitForward)
                });

                BossPressurePositionController positionController =
                    bossObject.AddComponent<BossPressurePositionController>();
                positionController.ConfigureReferences(
                    lane,
                    bossCost,
                    director,
                    bossObject.transform);
                SetPrivateInstanceField(positionController, "forwardPressureOscillationEnabled", false);

                if (observedPlayerSummonTier > 0)
                {
                    director.NotifyPlayerSummonFrontlineCreated(observedPlayerSummonTier);
                }

                Assert.IsTrue(director.TryQueueBestAvailableAction(), scenarioName);
                Assert.AreEqual(expectedActionKind, director.LastActionKind, scenarioName);
                Assert.AreEqual(expectedSlotIndex, director.LastQueuedActionSlotIndex, scenarioName);
                Assert.AreEqual(expectedSpentTier, director.LastSpentTier, scenarioName);
                Assert.AreEqual(expectedMovementIntent, director.LastMovementIntent, scenarioName);
                positionController.Tick(0.1f);
                Assert.AreEqual(
                    expectedPositionRisk01,
                    positionController.CurrentTargetRisk01,
                    0.001f,
                    scenarioName);
                Assert.AreEqual(expectedRespondsToPlayerSummon, director.LastActionRespondedToPlayerSummon, scenarioName);
                Assert.IsTrue(
                    basicFireEmitter.IsAutoFireSuppressed,
                    "Costed response actions should hold basic fire briefly so the boss read stays legible.");

                if (expectedActionKind == BossPressureActionKind.SummonPressure)
                {
                    Assert.AreEqual(expectedSummonTier, summonAction.LastReleasedTier, scenarioName);
                    Assert.AreEqual(1, summonAction.TotalReleaseCount, scenarioName);
                }
                else
                {
                    Assert.AreEqual(0, summonAction.TotalReleaseCount, scenarioName);
                }

                if (expectedRespondsToPlayerSummon)
                {
                    Assert.AreEqual(1, director.TotalPlayerSummonResponseCount, scenarioName);
                    Assert.AreEqual(0f, director.PlayerSummonResponseRemainingSeconds, 0.001f, scenarioName);
                }

                Object.DestroyImmediate(actorRoot);
                Object.DestroyImmediate(actorPrefabObject);
                Object.DestroyImmediate(projectilePrefabObject);
                Object.DestroyImmediate(punishPattern);
                Object.DestroyImmediate(summonPattern);
                Object.DestroyImmediate(specialPattern);
                Object.DestroyImmediate(basePattern);
                Object.DestroyImmediate(bossObject);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(laneObject);
            }
        }

        [Test]
        public void BossPressureLoopObservesBasicFireSpecialSummonAndPunishMovement()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile punishPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBasicFireProfile basicFireProfile = ScriptableObject.CreateInstance<BossBasicFireProfile>();

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

            BossBasicFireEmitter basicFireEmitter = bossObject.AddComponent<BossBasicFireEmitter>();
            basicFireEmitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            basicFireEmitter.ConfigureProfile(basicFireProfile, projectilePrefab, 4);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            SerializedObject serializedDirector = new SerializedObject(director);
            serializedDirector.FindProperty("globalRecoverySeconds").floatValue = 0.35f;
            serializedDirector.FindProperty("decisionThinkIntervalSeconds").floatValue = 0.25f;
            serializedDirector.ApplyModifiedPropertiesWithoutUndo();
            director.ConfigureReferences(
                bossCost,
                emitter,
                summonAction,
                lane,
                playerObject.transform,
                basicFireEmitter);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    specialPattern,
                    BossPressureActionKind.SpecialSkill,
                    1,
                    1,
                    0f,
                    selectionPriority: 15,
                    movementIntent: BossPressureMovementIntent.StrafeFire),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    1,
                    1,
                    0f,
                    responseId: "LaserSoldierDodgeLine",
                    selectionPriority: 15,
                    movementIntent: BossPressureMovementIntent.RetreatAndSummon),
                new BossPressureActionDirector.BossPressureActionSlot(
                    punishPattern,
                    BossPressureActionKind.PunishOverextend,
                    3,
                    1,
                    0f,
                    usePlayerForwardRiskGate: true,
                    minimumPlayerForwardRisk01: 0.66f,
                    maximumPlayerForwardRisk01: 1f,
                    selectionPriority: 80,
                    forwardRiskPriorityBonus: 80,
                    movementIntent: BossPressureMovementIntent.CommitForward)
            });

            BossPressurePositionController positionController =
                bossObject.AddComponent<BossPressurePositionController>();
            positionController.ConfigureReferences(lane, bossCost, director, bossObject.transform);
            SetPrivateInstanceField(positionController, "forwardPressureOscillationEnabled", false);

            basicFireEmitter.Tick(basicFireProfile.InitialDelaySeconds + 0.05f);

            Assert.GreaterOrEqual(director.TotalBasicShotVolleys, 1);
            Assert.Greater(director.LastBasicShotProjectileCount, 0);

            bossCost.GrantCurrentTierCost(100f);
            director.Tick(0.3f);
            positionController.Tick(0.1f);

            Assert.AreEqual(BossPressureActionKind.SpecialSkill, director.LastActionKind);
            Assert.AreEqual(BossPressureMovementIntent.StrafeFire, director.LastMovementIntent);
            Assert.AreEqual(0.52f, positionController.CurrentTargetRisk01, 0.001f);
            emitter.CancelQueuedPriorityPattern(specialPattern);

            bossCost.GrantCurrentTierCost(100f);
            director.Tick(0.45f);
            positionController.Tick(0.1f);

            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.AreEqual(BossPressureMovementIntent.RetreatAndSummon, director.LastMovementIntent);
            Assert.AreEqual(1, summonAction.LastReleasedTier);
            Assert.AreEqual(1, summonAction.TotalReleaseCount);
            Assert.AreEqual(1, summonAction.ActiveSummonActorCount);
            Assert.AreEqual(0.1f, positionController.CurrentTargetRisk01, 0.001f);
            emitter.CancelQueuedPriorityPattern(summonPattern);

            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            bossCost.GrantCurrentTierCost(300f);
            director.Tick(0.45f);
            positionController.Tick(0.1f);

            Assert.AreEqual(BossPressureActionKind.PunishOverextend, director.LastActionKind);
            Assert.AreEqual(BossPressureMovementIntent.CommitForward, director.LastMovementIntent);
            Assert.AreEqual(1, summonAction.TotalReleaseCount);
            Assert.IsTrue(director.LastDecisionContext.HasActiveBossPressureSummon);
            Assert.AreEqual(0.9f, positionController.CurrentTargetRisk01, 0.001f);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(basicFireProfile);
            Object.DestroyImmediate(punishPattern);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(specialPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossSummonPressureActorAdvancesIntoPlayerSideOfBoundary()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            GameObject bossObject = new GameObject("BossProxy");

            GameObject actorPrefabObject = new GameObject("BossSummonPressurePrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);

            Assert.IsTrue(summonAction.TryReleasePressureSummon(2));

            SummonFrontlineProxy activeProxy = null;
            SummonFrontlineProxy[] proxies = Object.FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < proxies.Length; i++)
            {
                if (proxies[i] != null
                    && proxies[i].IsActive
                    && proxies[i].transform.IsChildOf(actorRoot.transform))
                {
                    activeProxy = proxies[i];
                    break;
                }
            }

            Assert.IsNotNull(activeProxy);
            Vector2 startLane = lane.GetLaneCoordinates(activeProxy.AdvanceStartPosition);
            Vector2 targetLane = lane.GetLaneCoordinates(activeProxy.AdvanceTargetPosition);
            Assert.Greater(
                startLane.y,
                lane.ForwardBoundaryZ,
                "Boss pressure summons should still enter from the boss/frontline side.");
            Assert.Less(
                targetLane.y,
                lane.ForwardBoundaryZ - 0.5f,
                "Boss pressure summons should cross into the player side instead of stopping at the boundary entry line.");

            float actorLaneBeforeTick = lane.GetLaneCoordinates(activeProxy.transform.position).y;
            activeProxy.Tick(1f);
            float actorLaneAfterTick = lane.GetLaneCoordinates(activeProxy.transform.position).y;
            Assert.Less(
                actorLaneAfterTick,
                actorLaneBeforeTick - 0.5f,
                "Boss pressure summons should visibly march toward the player side after spawning.");
            Assert.Less(
                activeProxy.AdvanceProgress01,
                0.75f,
                "Boss pressure summons should cross the corridor quickly by walking, not by snapping to the player side.");

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossSummonPressureSuppressionReportsScreensSeparatelyFromActors()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");

            GameObject actorPrefabObject = new GameObject("BossSummonPressurePrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonPressureScreen prefabScreen = actorPrefabObject.AddComponent<SummonPressureScreen>();
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefab.ConfigurePresentation(null, prefabScreen);
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);

            Assert.IsTrue(summonAction.TryReleasePressureSummon(2));
            Assert.AreEqual(1, summonAction.SuppressActivePressureScreens(3));
            Assert.AreEqual(1, summonAction.TotalPressureScreenSuppressCount);
            Assert.AreEqual(1, summonAction.TotalPressureActorSuppressCount);
            Assert.AreEqual(0, summonAction.ActiveSummonActorCount);

            Assert.IsTrue(summonAction.TryReleasePressureSummon(2));
            Assert.AreEqual(2, summonAction.SuppressActivePressureResponses(3));
            Assert.AreEqual(2, summonAction.TotalPressureScreenSuppressCount);
            Assert.AreEqual(2, summonAction.TotalPressureActorSuppressCount);
            Assert.AreEqual(0, summonAction.ActiveSummonActorCount);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossLaserSummonStagesAtRangeBeforeSharedLaserPattern()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");

            GameObject actorPrefabObject = new GameObject("BossLaserSummonPrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefabObject.AddComponent<BossLaserSummonPattern>();
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);

            Assert.IsTrue(summonAction.TryReleasePressureSummon(1));

            SummonFrontlineProxy activeProxy = summonAction.LastSummonActor;
            Assert.IsNotNull(activeProxy);
            Assert.AreEqual("LaserSoldier", summonAction.LastSummonActorRoleId);
            Assert.IsNotNull(
                activeProxy.GetComponent<BossLaserSummonPattern>(),
                "Boss LaserSoldier should keep the shared laser pattern component.");
            Assert.LessOrEqual(
                activeProxy.AdvanceDistance,
                2.95f,
                "Boss LaserSoldier should stage at ranged standoff instead of walking all the way into the player.");

            Vector2 startLane = lane.GetLaneCoordinates(activeProxy.AdvanceStartPosition);
            Vector2 targetLane = lane.GetLaneCoordinates(activeProxy.AdvanceTargetPosition);
            Assert.Greater(
                startLane.y,
                lane.ForwardBoundaryZ,
                "Boss LaserSoldier should still enter from the boss/frontline side.");
            Assert.Greater(
                targetLane.y,
                lane.ForwardBoundaryZ,
                "Boss LaserSoldier should stop on the boss/frontline side and attack at range, not become a melee pressure body.");

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void EnemySummonPacingReleasesResponseSlotOneSummonWithoutBossCost()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");

            GameObject actorPrefabObject = new GameObject("BossSummonPressurePrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);

            EnemySummonPacingDirector pacingDirector = bossObject.AddComponent<EnemySummonPacingDirector>();
            pacingDirector.ConfigureReferences(summonAction);
            pacingDirector.ConfigurePacing(
                newInitialDelaySeconds: 1.0f,
                newRespawnIntervalSeconds: 5.0f,
                newSummonTier: 1,
                newRetryIntervalSeconds: 0.25f);

            pacingDirector.Tick(0.5f);

            Assert.AreEqual(0, pacingDirector.TotalPacingReleaseCount);
            Assert.AreEqual(0, summonAction.TotalReleaseCount);

            pacingDirector.Tick(0.6f);

            Assert.AreEqual(1, pacingDirector.TotalPacingReleaseCount);
            Assert.AreEqual(1, pacingDirector.LastPacingReleasedTier);
            Assert.AreEqual(1, summonAction.TotalReleaseCount);
            Assert.AreEqual(1, summonAction.LastReleasedTier);
            Assert.AreEqual(1, summonAction.ActiveSummonActorCount);

            pacingDirector.Tick(10f);

            Assert.AreEqual(
                2,
                pacingDirector.TotalPacingReleaseCount,
                "Enemy summon pacing should not stop just because a previous boss summon actor is still active.");
            Assert.AreEqual(
                2,
                summonAction.TotalReleaseCount,
                "Enemy summon pacing should keep releasing on its own cadence; actor replacement policy belongs to the summon action pool, not the pacing director.");

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void EnemySummonPacingCyclesConfiguredSummonTiers()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");

            GameObject actorPrefabObject = new GameObject("BossSummonPressurePrefab");
            actorPrefabObject.AddComponent<SphereCollider>();
            actorPrefabObject.AddComponent<Rigidbody>();
            CombatHealth actorHealth = actorPrefabObject.AddComponent<CombatHealth>();
            actorHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy actorPrefab = actorPrefabObject.AddComponent<SummonFrontlineProxy>();
            actorPrefab.ConfigureHealth(actorHealth);
            actorPrefabObject.SetActive(false);

            GameObject actorRoot = new GameObject("BossSummonActorRoot");
            BossSummonPressureAction summonAction = bossObject.AddComponent<BossSummonPressureAction>();
            summonAction.ConfigureReferences(lane, playerObject.transform, actorPrefab, actorRoot.transform);
            SerializedObject serializedSummonAction = new SerializedObject(summonAction);
            SerializedProperty maxActiveActors = serializedSummonAction.FindProperty("maxActiveSummonActors");
            Assert.IsNotNull(maxActiveActors);
            maxActiveActors.intValue = 3;
            serializedSummonAction.ApplyModifiedPropertiesWithoutUndo();

            EnemySummonPacingDirector pacingDirector = bossObject.AddComponent<EnemySummonPacingDirector>();
            pacingDirector.ConfigureReferences(summonAction);
            pacingDirector.ConfigurePacing(
                newInitialDelaySeconds: 0.1f,
                newRespawnIntervalSeconds: 0.5f,
                newSummonTier: 1,
                newRetryIntervalSeconds: 0.1f,
                newSummonTierSequence: new[] { 1, 2, 3 });

            Assert.AreEqual(3, pacingDirector.SummonTierSequenceCount);
            Assert.AreEqual(1, pacingDirector.NextPacingTier);

            pacingDirector.Tick(0.1f);

            Assert.AreEqual(1, summonAction.LastReleasedTier);
            Assert.AreEqual(2, pacingDirector.NextPacingTier);

            pacingDirector.Tick(0.5f);

            Assert.AreEqual(2, summonAction.LastReleasedTier);
            Assert.AreEqual(3, pacingDirector.NextPacingTier);

            pacingDirector.Tick(0.5f);

            Assert.AreEqual(3, summonAction.LastReleasedTier);
            Assert.AreEqual(1, pacingDirector.NextPacingTier);
            Assert.AreEqual(3, pacingDirector.TotalPacingReleaseCount);
            Assert.AreEqual(
                3,
                summonAction.ActiveSummonActorCount,
                "Pacing should cycle through configured tiers while previous summons are still alive when the summon action pool allows it.");

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossLaserSummonPatternAimsDownFromHighOriginToTargetHeight()
        {
            GameObject bossObject = new GameObject("BossLaserSummon");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            SummonFrontlineProxy proxy = bossObject.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(bossHealth);
            BossLaserSummonPattern laserPattern = bossObject.AddComponent<BossLaserSummonPattern>();

            GameObject originObject = new GameObject("HighLaserOrigin");
            originObject.transform.SetParent(bossObject.transform, worldPositionStays: true);
            originObject.transform.position = new Vector3(0f, 3.2f, 6f);

            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = Vector3.zero;
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);

            var serializedPattern = new SerializedObject(laserPattern);
            serializedPattern.FindProperty("laserOrigin").objectReferenceValue = originObject.transform;
            serializedPattern.FindProperty("targetHeightOffset").floatValue = 1.05f;
            serializedPattern.ApplyModifiedPropertiesWithoutUndo();
            laserPattern.ConfigurePattern(playerObject.transform, DamageTeam.Enemy, 58f, 0.12f, 4f);

            MethodInfo resolveDirection = typeof(BossLaserSummonPattern).GetMethod(
                "ResolveTargetDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(resolveDirection);

            Vector3 resolvedDirection = (Vector3)resolveDirection.Invoke(laserPattern, null);
            Vector3 expectedDirection =
                (playerObject.transform.position + Vector3.up * 1.05f - originObject.transform.position).normalized;

            Assert.Less(
                resolvedDirection.y,
                -0.1f,
                "Boss laser summons must aim down from a high muzzle to the player's body height instead of firing a horizontal capsule above the player.");
            Assert.AreEqual(expectedDirection.x, resolvedDirection.x, 0.001f);
            Assert.AreEqual(expectedDirection.y, resolvedDirection.y, 0.001f);
            Assert.AreEqual(expectedDirection.z, resolvedDirection.z, 0.001f);

            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(originObject);
            Object.DestroyImmediate(bossObject);
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
                    maximumPlayerForwardRisk01: 1f,
                    usePlayerSummonResponseGate: true,
                    minimumPlayerSummonTier: 1)
            });
            director.SetHoldForNextTierActionWhenGateAllows(true);
            director.NotifyPlayerSummonFrontlineCreated(1);

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
        public void BossPressureActionDirectorReleasesResponseSlotOneLaserAfterOpeningAction()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            GameObject bossObject = new GameObject("BossProxy");
            bossObject.transform.position = lane.GetBattlefieldWorldPoint(0f, lane.BossProxyZ);
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile basePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile specialPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile summonPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            BossBarragePatternProfile punishPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
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

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            SerializedObject serializedDirector = new SerializedObject(director);
            serializedDirector.FindProperty("globalRecoverySeconds").floatValue = 0f;
            serializedDirector.ApplyModifiedPropertiesWithoutUndo();
            director.ConfigureReferences(bossCost, emitter, summonAction, lane, playerObject.transform);
            director.ConfigureActionSlots(new[]
            {
                new BossPressureActionDirector.BossPressureActionSlot(
                    specialPattern,
                    BossPressureActionKind.SpecialSkill,
                    1,
                    1,
                    0f,
                    responseId: "DodgeBossLinePressureSpecial",
                    selectionPriority: 15,
                    movementIntent: BossPressureMovementIntent.StrafeFire),
                new BossPressureActionDirector.BossPressureActionSlot(
                    summonPattern,
                    BossPressureActionKind.SummonPressure,
                    1,
                    1,
                    0f,
                    responseId: "LaserSoldierDodgeLine",
                    selectionPriority: 15,
                    movementIntent: BossPressureMovementIntent.RetreatAndSummon),
                new BossPressureActionDirector.BossPressureActionSlot(
                    punishPattern,
                    BossPressureActionKind.PunishOverextend,
                    3,
                    1,
                    0f,
                    usePlayerForwardRiskGate: true,
                    minimumPlayerForwardRisk01: 0.66f,
                    maximumPlayerForwardRisk01: 1f,
                    responseId: "RetreatOrSpendHighTierAnswer",
                    selectionPriority: 80,
                    forwardRiskPriorityBonus: 80,
                    movementIntent: BossPressureMovementIntent.CommitForward)
            });
            director.SetHoldForNextTierActionWhenGateAllows(true);

            bossCost.GrantCurrentTierCost(100f);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreEqual(BossPressureActionKind.SpecialSkill, director.LastActionKind);
            Assert.AreEqual(1, director.LastSpentTier);
            emitter.CancelQueuedPriorityPattern(specialPattern);
            director.Tick(8f);

            bossCost.GrantCurrentTierCost(100f);
            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.AreEqual(1, director.LastSpentTier);
            Assert.AreEqual(1, summonAction.LastReleasedTier);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
            Object.DestroyImmediate(projectilePrefabObject);
            Object.DestroyImmediate(punishPattern);
            Object.DestroyImmediate(summonPattern);
            Object.DestroyImmediate(specialPattern);
            Object.DestroyImmediate(basePattern);
            Object.DestroyImmediate(bossObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(laneObject);
        }

        [Test]
        public void BossPressureActionDirectorPreservesSummonResponseWindowWhileHoldingNextTier()
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
            emitter.ConfigurePattern(basePattern, null, 0);

            BossPressureCostLadder bossCost = bossObject.AddComponent<BossPressureCostLadder>();
            bossCost.ConfigureReferences(lane, bossObject.transform);
            bossCost.GrantCurrentTierCost(100f);

            BossPressureActionDirector director = bossObject.AddComponent<BossPressureActionDirector>();
            SerializedObject serializedDirector = new SerializedObject(director);
            serializedDirector.FindProperty("playerSummonResponseWindowSeconds").floatValue = 1f;
            serializedDirector.FindProperty("heldResponseWindowFloorSeconds").floatValue = 0.5f;
            serializedDirector.FindProperty("maxHeldResponseWindowExtensionSeconds").floatValue = 1f;
            serializedDirector.ApplyModifiedPropertiesWithoutUndo();
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
                    maximumPlayerForwardRisk01: 1f,
                    usePlayerSummonResponseGate: true,
                    minimumPlayerSummonTier: 1)
            });
            director.SetHoldForNextTierActionWhenGateAllows(true);
            director.NotifyPlayerSummonFrontlineCreated(1);

            director.Tick(0.8f);

            Assert.IsFalse(emitter.HasQueuedPriorityPattern);
            Assert.AreEqual(1, bossCost.AvailableTier);
            Assert.GreaterOrEqual(
                director.PlayerSummonResponseRemainingSeconds,
                director.HeldResponseWindowFloorSeconds,
                "A boss that is intentionally holding LV1 for the next-tier summon response should not drop the response window immediately before LV2 arrives.");
            Assert.Less(
                director.HeldResponseWindowExtensionRemainingSeconds,
                1f,
                "The held response grace should spend from a bounded extension budget instead of becoming an endless wait.");

            bossCost.GrantCurrentTierCost(100f);

            Assert.IsTrue(director.TryQueueBestAvailableAction());
            Assert.AreSame(summonPattern, emitter.QueuedPriorityPattern);
            Assert.AreEqual(BossPressureActionKind.SummonPressure, director.LastActionKind);
            Assert.IsTrue(director.LastActionRespondedToPlayerSummon);
            Assert.AreEqual(0f, director.PlayerSummonResponseRemainingSeconds, 0.001f);

            Object.DestroyImmediate(actorRoot);
            Object.DestroyImmediate(actorPrefabObject);
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
        public void BossBarragePatternMarksCostedSharedSkillCandidateWithoutChangingFireLogic()
        {
            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("skillPatternFamily").enumValueIndex =
                (int)LaneSkillPatternFamily.LinePressure;
            serializedObject.FindProperty("skillTransferMode").enumValueIndex =
                (int)LaneSkillTransferMode.SharedPvpSkillCandidate;
            serializedObject.FindProperty("playerSkillTranslationNote").stringValue =
                "Costed player skill with visible rail startup.";
            serializedObject.FindProperty("counterplayNote").stringValue =
                "Move off the marked rail before release.";
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(LaneSkillPatternFamily.LinePressure, pattern.SkillPatternFamily);
            Assert.AreEqual(LaneSkillTransferMode.SharedPvpSkillCandidate, pattern.SkillTransferMode);
            Assert.IsTrue(pattern.IsPlayerSkillCandidate);
            Assert.IsTrue(pattern.PlayerSkillTranslationNote.Contains("Costed"));
            Assert.IsTrue(pattern.CounterplayNote.Contains("marked rail"));
            Assert.AreEqual(
                BossBarrageLateralShape.CenterSpread,
                pattern.LateralShape,
                "Shared skill metadata should not mutate projectile shape or fire behavior.");

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void BossBarrageLinePressureAndLayeredSalvoExposeDifferentTelegraphReads()
        {
            BossBarragePatternProfile linePattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var lineSerializedObject = new SerializedObject(linePattern);
            lineSerializedObject.FindProperty("lateralShape").enumValueIndex =
                (int)BossBarrageLateralShape.LinePressure;
            lineSerializedObject.FindProperty("telegraphMarkerWidthScale").floatValue = 0.48f;
            lineSerializedObject.FindProperty("telegraphMarkerDepthScale").floatValue = 1.85f;
            lineSerializedObject.FindProperty("telegraphPulseScale").floatValue = 1.35f;
            lineSerializedObject.FindProperty("telegraphWindupColor").colorValue =
                new Color(0.12f, 0.9f, 1f, 0.72f);
            lineSerializedObject.FindProperty("projectileColor").colorValue =
                new Color(0.2f, 0.95f, 1f, 1f);
            lineSerializedObject.FindProperty("projectileVisualScale").vector3Value =
                new Vector3(0.72f, 0.72f, 2.35f);
            lineSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            BossBarragePatternProfile layeredPattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var layeredSerializedObject = new SerializedObject(layeredPattern);
            layeredSerializedObject.FindProperty("lateralShape").enumValueIndex =
                (int)BossBarrageLateralShape.LayeredSalvo;
            layeredSerializedObject.FindProperty("telegraphMarkerWidthScale").floatValue = 1.28f;
            layeredSerializedObject.FindProperty("telegraphMarkerDepthScale").floatValue = 0.58f;
            layeredSerializedObject.FindProperty("telegraphPulseScale").floatValue = 0.85f;
            layeredSerializedObject.FindProperty("telegraphWindupColor").colorValue =
                new Color(1f, 0.24f, 0.72f, 0.7f);
            layeredSerializedObject.FindProperty("projectileColor").colorValue =
                new Color(1f, 0.28f, 0.78f, 1f);
            layeredSerializedObject.FindProperty("projectileVisualScale").vector3Value =
                new Vector3(1.45f, 0.58f, 0.9f);
            layeredSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.Less(
                linePattern.TelegraphMarkerWidthScale,
                layeredPattern.TelegraphMarkerWidthScale,
                "LinePressure should read as a narrow rail compared with LayeredSalvo rows.");
            Assert.Greater(
                linePattern.TelegraphMarkerDepthScale,
                layeredPattern.TelegraphMarkerDepthScale,
                "LinePressure should stretch along depth while LayeredSalvo should read as row plates.");
            Assert.AreNotEqual(linePattern.TelegraphWindupColor, layeredPattern.TelegraphWindupColor);
            Assert.Greater(linePattern.TelegraphPulseScale, layeredPattern.TelegraphPulseScale);
            Assert.AreNotEqual(linePattern.ProjectileColor, layeredPattern.ProjectileColor);
            Assert.Greater(
                linePattern.ProjectileVisualScale.z,
                layeredPattern.ProjectileVisualScale.z,
                "LinePressure fired projectiles should stay visually stretched along their travel rail.");
            Assert.Greater(
                layeredPattern.ProjectileVisualScale.x,
                linePattern.ProjectileVisualScale.x,
                "LayeredSalvo fired projectiles should read wider than LinePressure rail bolts.");

            Object.DestroyImmediate(layeredPattern);
            Object.DestroyImmediate(linePattern);
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
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 0f;
            serializedObject.FindProperty("linePressureDirection").floatValue = 1f;
            serializedObject.FindProperty("linePressureCenterRatio").floatValue = 0.72f;
            serializedObject.FindProperty("linePressureHalfSpreadRatio").floatValue = 0.08f;
            serializedObject.FindProperty("backlineDepthSpread").floatValue = 2.2f;
            serializedObject.FindProperty("forwardDepthSpread").floatValue = 0.85f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            float firstOffset = pattern.GetLateralOffset(0, 4, 0f);
            float lastOffset = pattern.GetLateralOffset(3, 4, 0f);
            float firstForwardOffset = pattern.GetLateralOffset(0, 4, 1f);
            float secondForwardOffset = pattern.GetLateralOffset(1, 4, 1f);
            float backlineDepth = pattern.GetTargetDepthOffset(3, 4, 0f);
            float forwardDepth = pattern.GetTargetDepthOffset(3, 4, 1f);

            Assert.Greater(firstOffset, 0f, "Right-side LinePressure should commit pressure to one rail.");
            Assert.Greater(lastOffset, 0f, "LinePressure scatter should stay on the committed rail.");
            Assert.Less(
                Mathf.Abs(lastOffset - firstOffset),
                pattern.EvaluateHalfSpread(0f),
                "LinePressure should read as a narrow lane instead of a full spread.");
            Assert.Less(
                Mathf.Abs(firstForwardOffset),
                Mathf.Abs(firstOffset),
                "Forward-risk LinePressure should commit its rail closer to the player lane than the backline.");
            Assert.Less(
                new Vector2(secondForwardOffset, pattern.GetTargetDepthOffset(1, 4, 1f)).magnitude,
                0.75f,
                "Forward-risk LinePressure should create a physical dodge tax while backline play keeps the rail wider.");
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
            Assert.AreEqual(ProjectileImpactResult.AppliedDamage, projectile.LastImpactResult);
            Assert.AreEqual(90f, targetHealth.CurrentHealth, 0.001f);

            GameObject neutralObject = new GameObject("Neutral");
            SphereCollider neutralCollider = neutralObject.AddComponent<SphereCollider>();
            CombatHealth neutralHealth = neutralObject.AddComponent<CombatHealth>();
            neutralHealth.ConfigureTeam(DamageTeam.Neutral);

            projectile.Configure(null, DamageTeam.Enemy, 10f, Vector3.back, 0f, 1f, 0.3f);
            Assert.IsFalse(projectile.TryApplyImpact(neutralCollider, Vector3.zero));
            Assert.AreEqual(ProjectileImpactResult.IgnoredNonHostile, projectile.LastImpactResult);
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
        public void SummonPressureScreenIdentifiesAndConsumesIntersectingSkillBeam()
        {
            GameObject screenObject = new GameObject("SummonPressureScreen");
            screenObject.transform.position = Vector3.forward * 4f;
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen screen = screenObject.AddComponent<SummonPressureScreen>();
            screen.Activate(DamageTeam.Enemy, 2, 1f, 1f);

            int interceptedBeamCount = 0;
            screen.SkillBeamIntercepted += _ => interceptedBeamCount++;

            Assert.IsFalse(
                SummonPressureScreen.TryInterceptAnySkillBeam(
                    DamageTeam.Enemy,
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.right,
                    10f,
                    0.25f,
                    out int ignoredBeamIndex,
                    out float ignoredBeamDistance));
            Assert.AreEqual(-1, ignoredBeamIndex);
            Assert.IsTrue(float.IsPositiveInfinity(ignoredBeamDistance));

            Assert.IsTrue(
                SummonPressureScreen.TryInterceptAnySkillBeam(
                    DamageTeam.Player,
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.right,
                    10f,
                    0.25f,
                    out int forwardBeamIndex,
                    out float forwardBeamDistance));
            Assert.AreEqual(0, forwardBeamIndex);
            Assert.AreEqual(3f, forwardBeamDistance, 0.001f);
            Assert.AreEqual(1, screen.RemainingIntercepts);

            screenObject.transform.position = Vector3.left * 4f;
            Assert.IsTrue(
                SummonPressureScreen.TryInterceptAnySkillBeam(
                    DamageTeam.Player,
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.right,
                    10f,
                    0.25f,
                    out int leftBeamIndex,
                    out float leftBeamDistance));
            Assert.AreEqual(3, leftBeamIndex);
            Assert.AreEqual(3f, leftBeamDistance, 0.001f);
            Assert.AreEqual(2, interceptedBeamCount);
            Assert.IsFalse(screen.IsActive);

            Object.DestroyImmediate(screenObject);
        }

        [Test]
        public void PlayerLaserSweepDamagesTargetBeforeButNotBehindPressureScreen()
        {
            GameObject playerObject = new GameObject("LaserSweepPlayer");
            CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            PlayerSkill1LaserSweepAction laserSweepAction =
                playerObject.AddComponent<PlayerSkill1LaserSweepAction>();

            GameObject frontTargetObject = new GameObject("LaserSweepFrontTarget");
            frontTargetObject.transform.position = Vector3.forward * 2f;
            frontTargetObject.AddComponent<SphereCollider>();
            CombatHealth frontTargetHealth = frontTargetObject.AddComponent<CombatHealth>();
            frontTargetHealth.ConfigureTeam(DamageTeam.Enemy);
            frontTargetHealth.ResetHealthToFull();
            float frontHealthBefore = frontTargetHealth.CurrentHealth;

            GameObject rearTargetObject = new GameObject("LaserSweepRearTarget");
            rearTargetObject.transform.position = Vector3.forward * 6f;
            rearTargetObject.AddComponent<SphereCollider>();
            CombatHealth rearTargetHealth = rearTargetObject.AddComponent<CombatHealth>();
            rearTargetHealth.ConfigureTeam(DamageTeam.Enemy);
            rearTargetHealth.ResetHealthToFull();
            float rearHealthBefore = rearTargetHealth.CurrentHealth;

            GameObject screenObject = new GameObject("LaserSweepEnemyScreen");
            screenObject.transform.position = Vector3.forward * 4f;
            screenObject.AddComponent<SphereCollider>();
            screenObject.AddComponent<Rigidbody>();
            SummonPressureScreen screen = screenObject.AddComponent<SummonPressureScreen>();
            screen.Activate(DamageTeam.Enemy, 1, 1f, 1f);

            Assert.IsTrue(
                SummonPressureScreen.TryInterceptAnySkillBeam(
                    DamageTeam.Player,
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.right,
                    10f,
                    0.25f,
                    out int blockedBeamIndex,
                    out float blockedBeamDistance));

            MethodInfo applyDamage = typeof(PlayerSkill1LaserSweepAction).GetMethod(
                "ApplyDamageForBeamSpace",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(applyDamage);
            applyDamage.Invoke(
                laserSweepAction,
                new object[]
                {
                    Vector3.zero,
                    playerObject.transform,
                    10f,
                    true,
                    blockedBeamIndex,
                    blockedBeamDistance
                });

            Assert.Less(frontTargetHealth.CurrentHealth, frontHealthBefore);
            Assert.AreEqual(rearHealthBefore, rearTargetHealth.CurrentHealth, 0.001f);

            Object.DestroyImmediate(screenObject);
            Object.DestroyImmediate(rearTargetObject);
            Object.DestroyImmediate(frontTargetObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void PlayerLaserSweepRestoresAuthoredBeamPoseBeforePoolReuse()
        {
            const string laserPrefabPath =
                "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1_4SidesLaser_HOVL.prefab";
            GameObject playerObject = new GameObject("LaserSweepPoolReusePlayer");
            GameObject targetObject = null;
            try
            {
                CombatHealth playerHealth = playerObject.AddComponent<CombatHealth>();
                playerHealth.ConfigureTeam(DamageTeam.Player);
                PlayerSkill1LaserSweepAction laserSweepAction =
                    playerObject.AddComponent<PlayerSkill1LaserSweepAction>();
                GameObject laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(laserPrefabPath);
                Assert.IsNotNull(laserPrefab);
                SetPrivateInstanceField(laserSweepAction, "laserPrefab", laserPrefab);

                Assert.IsTrue(laserSweepAction.TryCastLaserSweep(1));
                FieldInfo beamSpaceField = typeof(PlayerSkill1LaserSweepAction).GetField(
                    "pooledBeamSpace",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(beamSpaceField);
                Transform pooledBeamSpace = beamSpaceField.GetValue(laserSweepAction) as Transform;
                Assert.IsNotNull(pooledBeamSpace);
                pooledBeamSpace.localRotation = Quaternion.Euler(0f, 30f, 0f);

                targetObject = new GameObject("LaserSweepPoolReuseTarget");
                targetObject.transform.position = Vector3.forward * 3f;
                targetObject.AddComponent<SphereCollider>();
                CombatHealth targetHealth = targetObject.AddComponent<CombatHealth>();
                targetHealth.ConfigureTeam(DamageTeam.Enemy);
                targetHealth.ResetHealthToFull();
                float healthBeforeReuse = targetHealth.CurrentHealth;

                Assert.IsTrue(laserSweepAction.TryCastLaserSweep(1));
                Assert.Less(
                    targetHealth.CurrentHealth,
                    healthBeforeReuse,
                    "A reused four-way laser must restart from its authored beam axes before its first damage tick.");
            }
            finally
            {
                if (targetObject != null)
                {
                    Object.DestroyImmediate(targetObject);
                }

                Object.DestroyImmediate(playerObject);
            }
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
            GameObject cuePrefab = new GameObject("PressureScreenCuePrefab");
            CombatVfxCueProfile cueProfile = CreatePressureScreenCueProfile(cuePrefab);
            CombatVfxCuePlayer cuePlayer = screenObject.AddComponent<CombatVfxCuePlayer>();
            ConfigureCombatVfxCuePlayer(cuePlayer, cueProfile);
            presenter.ConfigureVfxCuePlayer(cuePlayer, screen.transform, null);

            screen.Activate(DamageTeam.AllySummon, 1, 1.25f, 1f, 3);
            Assert.IsTrue(presenter.IsShowing);
            Assert.IsFalse(presenter.RenderVisuals);
            Assert.IsFalse(visualObject.activeSelf);
            Assert.AreEqual(
                1,
                presenter.ActivationVfxCueRequestCount,
                "Pressure-screen activation should request a promoted shield-state VFX cue, not only tint the primitive screen.");
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
            Assert.AreEqual(
                1,
                presenter.InterceptVfxCueRequestCount,
                "Pressure-screen intercepts should layer an in-world block cue so absorbed boss fire reads as an authored effect.");
            Assert.Less(
                (visualObject.transform.localPosition - visualLocalPositionBeforeIntercept).sqrMagnitude,
                0.0001f,
                "The pressure screen gameplay should remain active while the temporary screen visual stays inert.");

            screen.Activate(DamageTeam.AllySummon, 1, 1.25f, 1f);
            screen.Deactivate();
            Assert.IsFalse(
                presenter.IsShowing,
                "A pressure screen with no intercept flash should hide when it deactivates.");

            Object.DestroyImmediate(enemyProjectileObject);
            Object.DestroyImmediate(screenObject);
            Object.DestroyImmediate(cuePrefab);
            Object.DestroyImmediate(cueProfile);
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
        public void BossBarrageEmitterSamplesPlayerForwardRiskForPreviewSpacing()
        {
            GameObject laneObject = new GameObject("Lane");
            SummonLaneSpace lane = laneObject.AddComponent<SummonLaneSpace>();
            GameObject playerObject = new GameObject("Player");
            GameObject bossObject = new GameObject("BossProxy");
            CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
            bossHealth.ConfigureTeam(DamageTeam.Enemy);

            BossBarragePatternProfile pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
            var serializedObject = new SerializedObject(pattern);
            serializedObject.FindProperty("projectilesPerWave").intValue = 3;
            serializedObject.FindProperty("backlineHalfSpread").floatValue = 4f;
            serializedObject.FindProperty("forwardHalfSpread").floatValue = 1f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            BossBarrageEmitter emitter = bossObject.AddComponent<BossBarrageEmitter>();
            emitter.ConfigureReferences(lane, playerObject.transform, bossHealth);
            emitter.ConfigurePattern(pattern, null, 0);

            Vector2[] preview = new Vector2[3];
            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.BackLimitZ);
            Assert.IsTrue(emitter.BeginWindup());
            Assert.AreEqual(0f, emitter.PendingForwardRisk01, 0.001f);
            Assert.AreEqual(3, emitter.BuildPendingLaneTargetPreview(preview));
            float backlinePreviewWidth = preview[2].x - preview[0].x;

            playerObject.transform.position = lane.GetLaneWorldPoint(0f, lane.ForwardBoundaryZ);
            Assert.IsTrue(emitter.BeginWindup());
            Assert.AreEqual(1f, emitter.PendingForwardRisk01, 0.001f);
            Assert.AreEqual(3, emitter.BuildPendingLaneTargetPreview(preview));
            float forwardPreviewWidth = preview[2].x - preview[0].x;

            Assert.AreEqual(pattern.EvaluateHalfSpread(0f) * 2f, backlinePreviewWidth, 0.001f);
            Assert.AreEqual(pattern.EvaluateHalfSpread(1f) * 2f, forwardPreviewWidth, 0.001f);
            Assert.Greater(
                backlinePreviewWidth,
                forwardPreviewWidth,
                "The same boss barrage pattern should preview wider, safer gaps from the backline and tighter gaps near the forward-risk boundary.");

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

        private static void RequestScreenCueForTest(
            ActionScreenCuePresenter presenter,
            string cueId,
            Color color,
            float durationSeconds,
            float intensity,
            string categoryName)
        {
            System.Type categoryType = typeof(ActionScreenCuePresenter).GetNestedType(
                "ScreenCueCategory",
                System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(categoryType);

            System.Reflection.MethodInfo requestMethod = typeof(ActionScreenCuePresenter).GetMethod(
                "RequestScreenCue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(requestMethod);

            object category = System.Enum.Parse(categoryType, categoryName);
            requestMethod.Invoke(
                presenter,
                new object[] { cueId, color, durationSeconds, intensity, category });
        }

        private static void InvokePocketCameraBridgeHandlerForTest(
            BossBarragePocketCameraCueBridge bridge,
            string handlerName)
        {
            System.Reflection.MethodInfo method = typeof(BossBarragePocketCameraCueBridge).GetMethod(
                handlerName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            method.Invoke(bridge, null);
        }

        private static void InvokePocketVfxBridgeFollowupHitHandlerForTest(
            BossBarragePocketVfxCueBridge bridge,
            int tier,
            float damage)
        {
            MethodInfo method = typeof(BossBarragePocketVfxCueBridge).GetMethod(
                "HandleSummonFollowupHitConfirmed",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            method.Invoke(bridge, new object[] { tier, damage });
        }

        private static void RaiseEncounterFollowupHitConfirmedForTest(
            BossBarrageEncounterController encounterController,
            int tier,
            float damage)
        {
            FieldInfo eventField = typeof(BossBarrageEncounterController).GetField(
                "SummonFollowupHitConfirmed",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(eventField);
            System.Action<int, float> callback =
                eventField.GetValue(encounterController) as System.Action<int, float>;
            Assert.IsNotNull(callback, "The enabled VFX bridge should subscribe to the encounter follow-up event.");

            callback.Invoke(tier, damage);
        }

        private static void SetPrivateInstanceField<T>(object target, string fieldName, T value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"{target.GetType().Name}.{fieldName} should exist.");
            field.SetValue(target, value);
        }

        private static CombatVfxCueProfile CreatePressureScreenCueProfile(GameObject prefab)
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = serializedObject.FindProperty("cues");
            cues.arraySize = 2;
            ConfigureCue(cues.GetArrayElementAtIndex(0), CombatVfxCueId.EliteShieldSignal, prefab);
            ConfigureCue(cues.GetArrayElementAtIndex(1), CombatVfxCueId.SummonBlockOpportunity, prefab);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static CombatVfxCueProfile CreateEnergyVfxCueProfile(GameObject prefab)
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = serializedObject.FindProperty("cues");
            cues.arraySize = 3;
            ConfigureCue(cues.GetArrayElementAtIndex(0), CombatVfxCueId.EliteAuraSignal, prefab);
            ConfigureCue(cues.GetArrayElementAtIndex(1), CombatVfxCueId.SummonFollowupWindow, prefab);
            ConfigureCue(cues.GetArrayElementAtIndex(2), CombatVfxCueId.SummonFollowupMissed, prefab);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static CombatVfxCueProfile CreateFollowupHitVfxCueProfile(GameObject prefab)
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = serializedObject.FindProperty("cues");
            cues.arraySize = 1;
            ConfigureCue(cues.GetArrayElementAtIndex(0), CombatVfxCueId.SummonFollowupHit, prefab);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static int CountDirectChildrenNamed(Transform parent, string childName)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == childName)
                {
                    count++;
                }
            }

            return count;
        }

        private static CombatVfxCueProfile CreatePlayerDamageVfxCueProfile(
            GameObject damagedPrefab,
            GameObject criticalPrefab)
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = serializedObject.FindProperty("cues");
            cues.arraySize = 2;
            ConfigureCue(cues.GetArrayElementAtIndex(0), CombatVfxCueId.PlayerDamaged, damagedPrefab);
            ConfigureCue(cues.GetArrayElementAtIndex(1), CombatVfxCueId.PlayerCritical, criticalPrefab);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static CombatVfxCueProfile CreateSummonDamageVfxCueProfile(GameObject damagePrefab)
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = serializedObject.FindProperty("cues");
            cues.arraySize = 1;
            ConfigureCue(cues.GetArrayElementAtIndex(0), CombatVfxCueId.EnemyHit, damagePrefab);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static void ConfigureCue(
            SerializedProperty cue,
            CombatVfxCueId cueId,
            GameObject prefab)
        {
            cue.FindPropertyRelative("cueId").enumValueIndex = (int)cueId;
            cue.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            cue.FindPropertyRelative("localPositionOffset").vector3Value = Vector3.zero;
            cue.FindPropertyRelative("localEulerOffset").vector3Value = Vector3.zero;
            cue.FindPropertyRelative("localScale").vector3Value = Vector3.one;
            cue.FindPropertyRelative("lifetimeSeconds").floatValue = 0f;
            cue.FindPropertyRelative("prewarmCount").intValue = 0;
            cue.FindPropertyRelative("parentToAnchor").boolValue = true;
            cue.FindPropertyRelative("alignForwardToDirection").boolValue = false;
        }

        private static void ConfigureCombatVfxCuePlayer(
            CombatVfxCuePlayer cuePlayer,
            CombatVfxCueProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(cuePlayer);
            serializedObject.FindProperty("profile").objectReferenceValue = profile;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
