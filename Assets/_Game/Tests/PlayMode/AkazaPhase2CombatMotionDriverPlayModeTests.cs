using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class AkazaPhase2CombatMotionDriverPlayModeTests
    {
        private const string ControllerPath =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller";
        private const string HoverLancePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaHoverLance.asset";

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
        public void HealthEvents_DriveMicroRecoilAndLethalSettle_WhileStoppingBossAttacks()
        {
            using var fixture = new MotionFixture(includeCombat: true);
            Vector3 originalRootPosition = fixture.MotionRoot.localPosition;
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
            Assert.That(fixture.MotionRoot.localPosition.y, Is.LessThan(originalRootPosition.y - 0.1f));
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
