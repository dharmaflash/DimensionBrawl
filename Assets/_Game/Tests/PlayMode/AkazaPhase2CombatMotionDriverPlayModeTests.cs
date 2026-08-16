using System;
using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class AkazaPhase2CombatMotionDriverPlayModeTests
    {
        private const string ControllerPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller";
        private const string HoverLancePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaHoverLance.asset";
        private const string MotionDriverScriptPath =
            "Assets/_Game/Scripts/Presentation/AkazaPhase2CombatMotionDriver.cs";

        [Test]
        public void MotionDriverRunsAfterTheArenaTransformScheduler()
        {
            var executionOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AkazaPhase2CombatMotionDriver),
                typeof(DefaultExecutionOrder));
            Assert.That(executionOrder, Is.Not.Null);
            Assert.That(
                executionOrder.order,
                Is.GreaterThan(10000),
                "Root recoil and death settle must layer after the arena bob scheduler.");

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(
                MotionDriverScriptPath);
            Assert.That(script, Is.Not.Null);
            Assert.That(
                MonoImporter.GetExecutionOrder(script),
                Is.EqualTo(executionOrder.order),
                "The imported script order must not override the authored post-scheduler order.");
        }

        [UnityTest]
        public IEnumerator ActualLateUpdateOrder_PreservesDeathOverlayAfterArenaScheduler()
        {
            using var fixture = new MotionFixture(includeCombat: false);
            ActionFoundationArenaTransformMotion arenaMotion =
                fixture.MotionRoot.gameObject.AddComponent<
                    ActionFoundationArenaTransformMotion>();
            arenaMotion.Configure(
                Vector3.zero,
                Vector3.up,
                0.28f,
                0.2f,
                0f,
                lockLocalRotation: true,
                lockLocalScale: true);
            fixture.Driver.CaptureOriginalPose();
            Vector3 authoredRootPosition = fixture.MotionRoot.localPosition;
            Quaternion authoredRootRotation = fixture.MotionRoot.localRotation;
            fixture.Driver.PlayDeath();

            using (PresentationClock.ManualLease lease =
                PresentationClock.AcquireManual(this, 60))
            {
                for (int frame = 1; frame <= 55; frame++)
                {
                    lease.SetFrame(frame);
                    yield return null;
                }
            }

            Quaternion deathRotation = Quaternion.Euler(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees,
                0f,
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees);
            Vector3 deathPivot = Vector3.up
                * AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight;
            Vector3 expectedPosition = authoredRootPosition
                + authoredRootRotation * (deathPivot - deathRotation * deathPivot)
                + Vector3.down * AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance
                + Vector3.back * AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;
            Quaternion expectedRotation = authoredRootRotation * deathRotation;

            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(1f));
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, expectedPosition),
                Is.LessThan(0.0001f),
                "The arena scheduler must not replace the terminal death position.");
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, expectedRotation),
                Is.LessThan(0.01f),
                "The arena scheduler must not replace the terminal death rotation.");
        }

        [Test]
        public void TickPresentation_AddsHoverAndWingSway_DisableRestoresCapturedPose()
        {
            using var fixture = new MotionFixture(includeCombat: false);
            Vector3 originalRootPosition = fixture.MotionRoot.localPosition;
            Quaternion originalRootRotation = fixture.MotionRoot.localRotation;
            Quaternion[] originalWingRotations = fixture.CaptureWingRotations();

            fixture.Driver.TickPresentation(0.37f);

            Assert.That(fixture.Driver.OriginalPoseCaptured, Is.True);
            Assert.That(fixture.Driver.CapturedWingCount, Is.EqualTo(6));
            Assert.That(fixture.Driver.LastAppliedRootOffset.sqrMagnitude, Is.GreaterThan(0.000001f));
            Assert.That(fixture.Driver.LastWingOffsetDegrees, Is.GreaterThan(0.1f));
            Assert.That(
                Quaternion.Angle(fixture.WingRoots[0].localRotation, originalWingRotations[0]),
                Is.GreaterThan(0.1f));

            fixture.Driver.enabled = false;

            Assert.That(fixture.MotionRoot.localPosition, Is.EqualTo(originalRootPosition));
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, originalRootRotation),
                Is.LessThan(0.001f));
            for (int i = 0; i < fixture.WingRoots.Length; i++)
            {
                Assert.That(
                    Quaternion.Angle(fixture.WingRoots[i].localRotation, originalWingRotations[i]),
                    Is.LessThan(0.001f),
                    $"Wing {i} did not return to its captured local pose.");
            }
        }

        [Test]
        public void PlayHeavyRelease_UsesCanonicalTriggerAndMissingControllerFailsSafely()
        {
            using var fixture = new MotionFixture(includeCombat: false);
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null, "The canonical Phase 2 controller is missing.");
            fixture.Animator.runtimeAnimatorController = controller;

            bool accepted = fixture.Driver.PlayHeavyRelease();

            Assert.That(accepted, Is.True);
            Assert.That(fixture.Driver.LastAnimatorTriggerAccepted, Is.True);
            Assert.That(fixture.Driver.LastAnimatorTrigger, Is.EqualTo("AttackHeavy"));
            Assert.That(fixture.Driver.HeavyReleaseRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.IsHeavyReleaseActive, Is.True);

            fixture.Animator.runtimeAnimatorController = null;
            bool safelyRejected = fixture.Driver.PlayHeavyRelease();

            Assert.That(safelyRejected, Is.False);
            Assert.That(fixture.Driver.LastAnimatorTriggerAccepted, Is.False);
            Assert.That(fixture.Driver.HeavyReleaseRequestCount, Is.EqualTo(2));
            Assert.That(fixture.Driver.IsHeavyReleaseActive, Is.True);
        }

        [Test]
        public void BarrageWindupAutomaticallyStartsProceduralSixWingRelease()
        {
            using var fixture = new MotionFixture(includeCombat: true);
            BossBarragePatternProfile hoverLance =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(HoverLancePath);
            Assert.That(hoverLance, Is.Not.Null);
            fixture.Barrage.ConfigurePattern(hoverLance, null, 0);
            fixture.Barrage.SetFiringEnabled(true);
            int requestCount = fixture.Driver.HeavyReleaseRequestCount;

            Assert.That(fixture.Barrage.BeginWindup(), Is.True);

            Assert.That(fixture.Driver.HeavyReleaseRequestCount, Is.EqualTo(requestCount + 1));
            Assert.That(fixture.Driver.IsHeavyReleaseActive, Is.True);
        }

        [Test]
        public void DeathMotionDefaultsMatchRequiredAuthoredContract_AndSettleAtExactRootPose()
        {
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathSettleSeconds,
                Is.EqualTo(0.9f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance,
                Is.EqualTo(0.50f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance,
                Is.EqualTo(0.22f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees,
                Is.EqualTo(20f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees,
                Is.EqualTo(62f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight,
                Is.EqualTo(0.72f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathWingFoldDegrees,
                Is.EqualTo(52f));
            Assert.That(
                AkazaPhase2CombatMotionDriver.RequiredDeathWingYawDegrees,
                Is.EqualTo(20f));

            using var fixture = new MotionFixture(includeCombat: false);
            Vector3 deathBasePosition = fixture.MotionRoot.localPosition;
            Quaternion deathBaseRotation = fixture.MotionRoot.localRotation;

            Assert.That(
                fixture.Driver.DeathSettleDurationSeconds,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathSettleSeconds));
            Assert.That(
                fixture.Driver.DeathDropDistance,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance));
            Assert.That(
                fixture.Driver.DeathBackDistance,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance));
            Assert.That(
                fixture.Driver.DeathPitchDegrees,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees));
            Assert.That(
                fixture.Driver.DeathRollDegrees,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees));
            Assert.That(
                fixture.Driver.DeathPivotLocalHeight,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight));
            Assert.That(
                fixture.Driver.DeathWingFoldDegrees,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathWingFoldDegrees));
            Assert.That(
                fixture.Driver.DeathWingYawDegrees,
                Is.EqualTo(AkazaPhase2CombatMotionDriver.RequiredDeathWingYawDegrees));

            fixture.Driver.PlayDeath();
            Assert.That(fixture.Driver.DeathBasePoseCaptured, Is.True);
            fixture.Driver.TickPresentation(fixture.Driver.DeathSettleDurationSeconds + 0.1f);

            Quaternion deathRotation = Quaternion.Euler(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees,
                0f,
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees);
            Vector3 deathPivot = Vector3.up
                * AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight;
            Vector3 pivotCorrection = deathBaseRotation
                * (deathPivot - deathRotation * deathPivot);
            Vector3 expectedOffset = pivotCorrection
                + Vector3.down * AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance
                + Vector3.back * AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;
            Vector3 expectedPosition = deathBasePosition + expectedOffset;
            Quaternion expectedRotation = deathBaseRotation * deathRotation;

            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(1f));
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, expectedPosition),
                Is.LessThan(0.00001f),
                "The final position must include pivot correction, drop, and backward settle.");
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, expectedRotation),
                Is.LessThan(0.001f),
                "The final local rotation must include both authored pitch and roll.");
            Assert.That(
                Vector3.Distance(
                    fixture.Driver.LastAppliedRootOffset,
                    expectedOffset),
                Is.LessThan(0.00001f));
        }

        [Test]
        public void DeathPoseIgnoresSchedulerOverwrites_AndDisableRestoresCapturedPose()
        {
            using var fixture = new MotionFixture(includeCombat: false);
            Vector3 originalRootPosition = fixture.MotionRoot.localPosition;
            Quaternion originalRootRotation = fixture.MotionRoot.localRotation;
            Quaternion[] originalWingRotations = fixture.CaptureWingRotations();

            fixture.Driver.TickPresentation(0.37f);
            fixture.Driver.PlayDeath();

            Assert.That(
                fixture.Driver.DeathBasePoseCaptured,
                Is.True,
                "Death must freeze the scheduler base before its terminal settle begins.");

            const int overwriteCount = 5;
            float settleStep = fixture.Driver.DeathSettleDurationSeconds * 0.2f;
            for (int sample = 0; sample < overwriteCount; sample++)
            {
                fixture.MotionRoot.localPosition = originalRootPosition + new Vector3(
                    0.25f * (sample + 1),
                    -0.12f * sample,
                    0.33f * (sample + 1));
                fixture.MotionRoot.localRotation = Quaternion.Euler(
                    17f + sample * 9f,
                    31f - sample * 4f,
                    -23f + sample * 7f);
                for (int wingIndex = 0; wingIndex < fixture.WingRoots.Length; wingIndex++)
                {
                    fixture.WingRoots[wingIndex].localRotation = Quaternion.Euler(
                        65f + sample * 3f,
                        -42f + wingIndex * 5f,
                        28f - sample * 2f);
                }

                fixture.Driver.TickPresentation(settleStep);
            }

            Quaternion deathRotation = Quaternion.Euler(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees,
                0f,
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees);
            Vector3 deathPivot = Vector3.up
                * AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight;
            Vector3 pivotCorrection = originalRootRotation
                * (deathPivot - deathRotation * deathPivot);
            Vector3 expectedRootPosition = originalRootPosition
                + pivotCorrection
                + Vector3.down * AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance
                + Vector3.back * AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;
            Quaternion expectedRootRotation = originalRootRotation * deathRotation;

            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(1f).Within(0.0001f));
            AssertDeathPoseMatchesFrozenBase(
                fixture,
                expectedRootPosition,
                expectedRootRotation,
                originalWingRotations);

            fixture.MotionRoot.localPosition = new Vector3(-8f, 6f, 4f);
            fixture.MotionRoot.localRotation = Quaternion.Euler(-71f, 48f, 93f);
            for (int i = 0; i < fixture.WingRoots.Length; i++)
            {
                fixture.WingRoots[i].localRotation = Quaternion.Euler(80f, i * 13f, -60f);
            }

            fixture.Driver.TickPresentation(0f);

            AssertDeathPoseMatchesFrozenBase(
                fixture,
                expectedRootPosition,
                expectedRootRotation,
                originalWingRotations);

            fixture.Driver.enabled = false;

            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, originalRootPosition),
                Is.LessThan(0.00001f));
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, originalRootRotation),
                Is.LessThan(0.001f));
            for (int i = 0; i < fixture.WingRoots.Length; i++)
            {
                Assert.That(
                    Quaternion.Angle(
                        fixture.WingRoots[i].localRotation,
                        originalWingRotations[i]),
                    Is.LessThan(0.001f),
                    $"Wing {i} did not restore its originally captured pose.");
            }

            Assert.That(fixture.Driver.IsDead, Is.False);
            Assert.That(fixture.Driver.DeathBasePoseCaptured, Is.False);
        }

        [Test]
        public void DeathSettlePreservesBobbedStart_ButEndsOnAuthoredOriginalBase()
        {
            using var fixture = new MotionFixture(includeCombat: false);
            Vector3 authoredRootPosition = fixture.MotionRoot.localPosition;
            Quaternion authoredRootRotation = fixture.MotionRoot.localRotation;
            Vector3 bobbedDeathPosition = authoredRootPosition
                + new Vector3(0.16f, 0.28f, -0.19f);
            Quaternion bobbedDeathRotation = authoredRootRotation
                * Quaternion.Euler(7f, -13f, 11f);
            fixture.MotionRoot.SetLocalPositionAndRotation(
                bobbedDeathPosition,
                bobbedDeathRotation);

            fixture.Driver.PlayDeath();

            Assert.That(fixture.Driver.DeathBasePoseCaptured, Is.True);
            fixture.Driver.TickPresentation(0f);
            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(0f));
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, bobbedDeathPosition),
                Is.LessThan(0.00001f),
                "The first death sample must preserve the scheduler-evaluated base position.");
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, bobbedDeathRotation),
                Is.LessThan(0.001f),
                "The first death sample must preserve the scheduler-evaluated base rotation.");

            fixture.MotionRoot.SetLocalPositionAndRotation(
                new Vector3(9f, -4f, 7f),
                Quaternion.Euler(81f, -37f, 44f));
            fixture.Driver.TickPresentation(fixture.Driver.DeathSettleDurationSeconds * 0.5f);

            const float halfEase = 0.5f;
            Vector3 halfBasePosition = Vector3.Lerp(
                bobbedDeathPosition,
                authoredRootPosition,
                halfEase);
            Quaternion halfBaseRotation = Quaternion.Slerp(
                bobbedDeathRotation,
                authoredRootRotation,
                halfEase);
            Quaternion halfDeathRotation = Quaternion.Euler(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees * halfEase,
                0f,
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees * halfEase);
            Vector3 deathPivot = Vector3.up
                * AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight;
            Vector3 halfPivotCorrection = halfBaseRotation
                * (deathPivot - halfDeathRotation * deathPivot);
            Vector3 expectedHalfPosition = halfBasePosition
                + halfPivotCorrection
                + Vector3.down
                    * (AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance * halfEase)
                + Vector3.back
                    * (AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance * halfEase);
            Quaternion expectedHalfRotation = halfBaseRotation * halfDeathRotation;

            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, expectedHalfPosition),
                Is.LessThan(0.00001f),
                "Mid-settle scheduler writes must not replace the frozen death base.");
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, expectedHalfRotation),
                Is.LessThan(0.001f));

            fixture.MotionRoot.SetLocalPositionAndRotation(
                new Vector3(-6f, 8f, -3f),
                Quaternion.Euler(-48f, 73f, -29f));
            fixture.Driver.TickPresentation(fixture.Driver.DeathSettleDurationSeconds);

            Quaternion finalDeathRotation = Quaternion.Euler(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees,
                0f,
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees);
            Vector3 finalPivotCorrection = authoredRootRotation
                * (deathPivot - finalDeathRotation * deathPivot);
            Vector3 expectedFinalPosition = authoredRootPosition
                + finalPivotCorrection
                + Vector3.down * AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance
                + Vector3.back * AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;
            Quaternion expectedFinalRotation = authoredRootRotation * finalDeathRotation;
            Vector3 staleBobbedFinalPosition = bobbedDeathPosition
                + bobbedDeathRotation * (deathPivot - finalDeathRotation * deathPivot)
                + Vector3.down * AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance
                + Vector3.back * AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;

            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(1f));
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, expectedFinalPosition),
                Is.LessThan(0.00001f),
                "The terminal pose must resolve against the authored original base.");
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, expectedFinalRotation),
                Is.LessThan(0.001f));
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, staleBobbedFinalPosition),
                Is.GreaterThan(0.05f),
                "The scheduler bob must not survive in the terminal pose.");
        }

        [Test]
        public void HealthEvents_DriveMicroRecoilAndLethalSettle_WhileStoppingBossAttacks()
        {
            using var fixture = new MotionFixture(includeCombat: true);
            Vector3 originalRootPosition = fixture.MotionRoot.localPosition;
            Quaternion originalRootRotation = fixture.MotionRoot.localRotation;
            Quaternion[] originalWingRotations = fixture.CaptureWingRotations();
            float originalTimeScale = Time.timeScale;

            Assert.That(fixture.ApplyDamage(20f), Is.True);
            Assert.That(fixture.Driver.HitReactionRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.IsHitReactionActive, Is.True);

            fixture.Driver.TickPresentation(0.05f);
            Assert.That(fixture.Driver.LastAppliedRootOffset.z, Is.LessThan(-0.001f));

            Assert.That(fixture.ApplyDamage(200f), Is.True);

            Assert.That(fixture.Driver.IsDead, Is.True);
            Assert.That(fixture.Driver.DeathRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.AttacksStopped, Is.True);
            Assert.That(fixture.Barrage.IsFiringEnabled, Is.False);
            Assert.That(fixture.BasicFire.IsFiringEnabled, Is.False);

            fixture.Driver.TickPresentation(fixture.Driver.DeathSettleDurationSeconds + 0.1f);

            Assert.That(fixture.Driver.DeathProgress01, Is.EqualTo(1f).Within(0.0001f));
            Quaternion finalDeathRotation = Quaternion.Euler(
                AkazaPhase2CombatMotionDriver.RequiredDeathPitchDegrees,
                0f,
                AkazaPhase2CombatMotionDriver.RequiredDeathRollDegrees);
            Vector3 deathPivot = Vector3.up
                * AkazaPhase2CombatMotionDriver.RequiredDeathPivotLocalHeight;
            Vector3 expectedFinalPosition = originalRootPosition
                + originalRootRotation
                    * (deathPivot - finalDeathRotation * deathPivot)
                + Vector3.down
                    * AkazaPhase2CombatMotionDriver.RequiredDeathDropDistance
                + Vector3.back
                    * AkazaPhase2CombatMotionDriver.RequiredDeathBackDistance;
            Quaternion expectedFinalRotation =
                originalRootRotation * finalDeathRotation;
            Assert.That(
                Vector3.Distance(
                    fixture.MotionRoot.localPosition,
                    expectedFinalPosition),
                Is.LessThan(0.00001f));
            Assert.That(
                Quaternion.Angle(
                    fixture.MotionRoot.localRotation,
                    expectedFinalRotation),
                Is.LessThan(0.001f));
            Assert.That(
                Quaternion.Angle(fixture.WingRoots[0].localRotation, originalWingRotations[0]),
                Is.GreaterThan(5f));
            Assert.That(Time.timeScale, Is.EqualTo(originalTimeScale));

            fixture.Driver.enabled = false;
            Assert.That(fixture.MotionRoot.localPosition, Is.EqualTo(originalRootPosition));
            for (int i = 0; i < fixture.WingRoots.Length; i++)
            {
                Assert.That(
                    Quaternion.Angle(fixture.WingRoots[i].localRotation, originalWingRotations[i]),
                    Is.LessThan(0.001f));
            }
        }

        private static void AssertDeathPoseMatchesFrozenBase(
            MotionFixture fixture,
            Vector3 expectedRootPosition,
            Quaternion expectedRootRotation,
            Quaternion[] deathBaseWingRotations)
        {
            Assert.That(
                Vector3.Distance(fixture.MotionRoot.localPosition, expectedRootPosition),
                Is.LessThan(0.00001f),
                "Scheduler writes must not move the frozen terminal root pose.");
            Assert.That(
                Quaternion.Angle(fixture.MotionRoot.localRotation, expectedRootRotation),
                Is.LessThan(0.001f),
                "Scheduler writes must not rotate the frozen terminal root pose.");

            for (int i = 0; i < fixture.WingRoots.Length; i++)
            {
                float side = (i & 1) == 0 ? -1f : 1f;
                Quaternion expectedWingRotation = deathBaseWingRotations[i]
                    * Quaternion.Euler(
                        AkazaPhase2CombatMotionDriver.RequiredDeathWingFoldDegrees,
                        side * AkazaPhase2CombatMotionDriver.RequiredDeathWingYawDegrees,
                        side
                            * AkazaPhase2CombatMotionDriver.RequiredDeathWingFoldDegrees
                            * 0.35f);
                Assert.That(
                    Quaternion.Angle(
                        fixture.WingRoots[i].localRotation,
                        expectedWingRotation),
                    Is.LessThan(0.001f),
                    $"Wing {i} drifted away from its death-time base pose.");
            }
        }

        private sealed class MotionFixture : IDisposable
        {
            private readonly GameObject sourceRoot;

            public MotionFixture(bool includeCombat)
            {
                Root = new GameObject("Akaza Phase 2 Motion Driver Test Root");
                Root.SetActive(false);

                MotionRoot = new GameObject("Akaza Visual Motion Root").transform;
                MotionRoot.SetParent(Root.transform, false);
                MotionRoot.localPosition = new Vector3(0.2f, 1.4f, -0.35f);
                MotionRoot.localRotation = Quaternion.Euler(2f, -7f, 1f);
                Animator = MotionRoot.gameObject.AddComponent<Animator>();

                WingRoots = new Transform[6];
                for (int i = 0; i < WingRoots.Length; i++)
                {
                    Transform wing = new GameObject($"akArmRoot{(char)('A' + i)}_jnt").transform;
                    wing.SetParent(MotionRoot, false);
                    wing.localRotation = Quaternion.Euler(i * 1.5f, i * -2f, i * 2.25f);
                    WingRoots[i] = wing;
                }

                if (includeCombat)
                {
                    BossHealth = Root.AddComponent<CombatHealth>();
                    ConfigureHealth(BossHealth, DamageTeam.Enemy, 100f);
                    SummonLaneSpace laneSpace = Root.AddComponent<SummonLaneSpace>();
                    Barrage = Root.AddComponent<BossBarrageEmitter>();
                    BasicFire = Root.AddComponent<BossBasicFireEmitter>();
                    Barrage.ConfigureReferences(laneSpace, MotionRoot, BossHealth);

                    sourceRoot = new GameObject("Akaza Motion Driver Damage Source");
                    sourceRoot.SetActive(false);
                    SourceHealth = sourceRoot.AddComponent<CombatHealth>();
                    ConfigureHealth(SourceHealth, DamageTeam.Player, 100f);
                    sourceRoot.SetActive(true);
                }

                Driver = Root.AddComponent<AkazaPhase2CombatMotionDriver>();
                Driver.Configure(
                    Animator,
                    BossHealth,
                    MotionRoot,
                    WingRoots,
                    Barrage,
                    BasicFire);
                Root.SetActive(true);
            }

            public GameObject Root { get; }
            public AkazaPhase2CombatMotionDriver Driver { get; }
            public Animator Animator { get; }
            public Transform MotionRoot { get; }
            public Transform[] WingRoots { get; }
            public CombatHealth BossHealth { get; }
            public CombatHealth SourceHealth { get; }
            public BossBarrageEmitter Barrage { get; }
            public BossBasicFireEmitter BasicFire { get; }

            public Quaternion[] CaptureWingRotations()
            {
                var rotations = new Quaternion[WingRoots.Length];
                for (int i = 0; i < WingRoots.Length; i++)
                {
                    rotations[i] = WingRoots[i].localRotation;
                }

                return rotations;
            }

            public bool ApplyDamage(float amount)
            {
                Assert.That(BossHealth, Is.Not.Null);
                return BossHealth.TryApplyDamage(new DamageInfo(
                    SourceHealth,
                    DamageTeam.Player,
                    amount,
                    BossHealth.transform.position,
                    Vector3.forward,
                    0f));
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root);
                }

                if (sourceRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(sourceRoot);
                }
            }

            private static void ConfigureHealth(CombatHealth health, DamageTeam team, float maxHealth)
            {
                var serialized = new SerializedObject(health);
                serialized.FindProperty("team").enumValueIndex = (int)team;
                serialized.FindProperty("maxHealth").floatValue = maxHealth;
                serialized.FindProperty("startAtFullHealth").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
