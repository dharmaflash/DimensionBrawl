using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    [DefaultExecutionOrder(-32600)]
    internal sealed class AuditionPvCityRecorderWarmupBeginHarness : MonoBehaviour
    {
        private AuditionPvCityHeroPocketDirector director;
        private bool armed;

        public bool Began { get; private set; }
        public Exception Failure { get; private set; }

        public void Configure(AuditionPvCityHeroPocketDirector value)
        {
            director = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void Arm()
        {
            if (director == null || armed || Began || Failure != null)
            {
                throw new InvalidOperationException(
                    "The Recorder warmup begin harness can be armed exactly once.");
            }
            armed = true;
        }

        private void Update()
        {
            if (!armed || Began || Failure != null)
            {
                return;
            }
            armed = false;
            try
            {
                director.BeginShotForRecorder(2);
                Began = true;
            }
            catch (Exception exception)
            {
                Failure = exception;
            }
        }
    }

    public sealed class AuditionPvCityHeroPocketCaptureTests
    {
        [Test]
        public void Contracts_AreExactSixtyFpsRangesBaselinesAndSeed()
        {
            AuditionPvShotManifestEntry[] shots =
                AuditionPvCityHeroPocketCapture.CreateShotManifestEntries();
            Assert.That(shots.Select(value => value.id),
                Is.EqualTo(new[] { "g01", "g02", "g03" }));
            Assert.That(shots.Select(value => value.expectedFrameCount),
                Is.EqualTo(new[] { 600, 780, 660 }));
            Assert.That(shots.Select(value => value.endFrame),
                Is.EqualTo(new[] { 599, 779, 659 }));
            Assert.That(shots.Select(value => value.hudMode),
                Is.EqualTo(new[] { "hud-off", "hud-on", "hud-off" }));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
            Assert.That(
                AuditionPvCityHeroPocketCapture.DeterministicRandomSeed,
                Is.EqualTo(0xC172));
            Assert.That(
                AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime,
                Is.EqualTo(1f / 60f));
            Assert.That(
                AuditionPvCityHeroPocketCapture.FixedDeltaTimeRestoreTolerance,
                Is.EqualTo(0.0000001f));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetGateSemanticBeatIds(
                    AuditionPvCityShot.G01),
                Is.EqualTo(new[] { "city-alert", "city-skyline" }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetGateSemanticBeatIds(
                    AuditionPvCityShot.G02),
                Is.EqualTo(new[]
                {
                    "city-movement", "city-fire", "city-hud-gameplay"
                }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetGateSemanticBeatIds(
                    AuditionPvCityShot.G03),
                Is.EqualTo(new[]
                {
                    "dimensional-anomaly", "dimension-rift-transition"
                }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.G02IgnoredProjectileTriggerFrames,
                Is.EqualTo(new[] { 327, 329, 337 }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.G02ExpectedFiredCount,
                Is.EqualTo(11));
            Assert.That(
                AuditionPvCityHeroPocketCapture.G02ReloadLifecycleEventNames,
                Is.EqualTo(new[]
                {
                    "started", "canceled", "started", "canceled",
                    "started", "completed", "started"
                }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.G02ReloadLifecycleFrames,
                Is.EqualTo(new[] { 55, 84, 85, 126, 127, 248, 324 }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.G02ReloadLifecycleAmmo,
                Is.EqualTo(new[] { 23, 23, 22, 22, 21, 24, 16 }));
            Assert.That(
                AuditionPvCityHeroPocketCapture.G02ReloadLifecycleIsReloading,
                Is.EqualTo(new[]
                {
                    true, false, true, false, true, false, true
                }));

            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvCityHeroPocketCapture.CreateBaselineManifestEntries();
            Assert.That(baselines, Has.Length.EqualTo(2));
            Assert.That(baselines[0].sourceFrame, Is.EqualTo(300));
            Assert.That(baselines[0].fileName, Is.EqualTo(
                "BL01_CITY_HERO_WIDE__HUDOFF__t02.000000.png"));
            Assert.That(baselines[1].sourceFrame, Is.EqualTo(420));
            Assert.That(baselines[1].fileName, Is.EqualTo(
                "BL02_CITY_RIFLE_DODGE__HUDON__t04.000000.png"));
            Assert.That(
                AuditionPvCityHeroPocketCapture.FrameTimeSeconds(
                    AuditionPvCityShot.G02,
                    240),
                Is.EqualTo(4f));
            foreach (AuditionPvCityShot shot in new[]
                     {
                         AuditionPvCityShot.G01,
                         AuditionPvCityShot.G02,
                         AuditionPvCityShot.G03
                     })
            {
                Assert.That(
                    AuditionPvCityHeroPocketCapture.GetSelectStartFrame(shot),
                    Is.EqualTo(180));
                Assert.That(
                    AuditionPvCityHeroPocketCapture.SourceFrameRole(shot, 0),
                    Is.EqualTo("prehandle"));
                Assert.That(
                    AuditionPvCityHeroPocketCapture.SourceFrameRole(shot, 180),
                    Is.EqualTo("logical"));
                Assert.That(
                    AuditionPvCityHeroPocketCapture.SourceFrameRole(
                        shot,
                        AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot)),
                    Is.EqualTo("posthandle"));
            }
        }

        [Test]
        public void G02HardwareSchedule_IsExactAndUsesReviewedVectors()
        {
            AuditionPvCityInputCommand[] schedule =
                AuditionPvCityHeroPocketCapture.CreateG02InputSchedule();
            Assert.That(schedule.Select(value => value.frame), Is.EqualTo(new[]
            {
                12, 54, 55, 84, 85, 108, 126, 127, 156,
                192, 240, 241, 242, 264, 324, 330, 378
            }));
            Assert.That(schedule.Count(value =>
                    value.phase == AuditionPvCityPointerPhase.Down),
                Is.EqualTo(8));
            Assert.That(schedule.Count(value =>
                    value.phase == AuditionPvCityPointerPhase.Drag),
                Is.EqualTo(1));
            Assert.That(schedule.Count(value =>
                    value.phase == AuditionPvCityPointerPhase.Up),
                Is.EqualTo(8));
            Assert.That(schedule.Single(value => value.frame == 12).value,
                Is.EqualTo(new Vector2(0.34f, 0.72f)));
            Assert.That(schedule.Single(value => value.frame == 108).value,
                Is.EqualTo(new Vector2(-0.48f, 0.18f)));
            Assert.That(schedule.Single(value => value.frame == 192).value,
                Is.EqualTo(new Vector2(0.50f, 0.08f)));
            Assert.That(schedule.Single(value => value.frame == 330).value,
                Is.EqualTo(new Vector2(-0.62f, 0.10f)));
            Assert.That(schedule.Single(value => value.frame == 240).target,
                Is.EqualTo(AuditionPvCityInputTarget.Dodge));
        }

        [Test]
        public void G02ActorFramingSamples_AreExactReviewedFrames()
        {
            int[] playerFrames = Enumerable.Range(0, 420)
                .Where(AuditionPvCityHeroPocketCapture
                    .IsG02PlayerFramingSampleFrame)
                .ToArray();
            int[] enemyAliveFrames = Enumerable.Range(0, 420)
                .Where(AuditionPvCityHeroPocketCapture
                    .IsG02EnemyFramingSampleFrame)
                .ToArray();

            Assert.That(playerFrames, Is.EqualTo(new[] { 0, 120, 240, 419 }));
            Assert.That(enemyAliveFrames, Is.EqualTo(new[] { 0, 120, 240 }));
        }

        [Test]
        public void Rails_UseExactKnotsAndFrameIndexedSmoothStep()
        {
            AssertPose(AuditionPvCityShot.G01, 0,
                new Vector3(5.4f, 6.2f, -15.5f),
                new Vector3(0f, 2.15f, 1.2f), 48f);
            AssertPose(AuditionPvCityShot.G01, 239,
                new Vector3(4.2f, 5.3f, -13.2f),
                new Vector3(0f, 1.95f, 2f), 46f);
            AssertPose(AuditionPvCityShot.G02, 192,
                new Vector3(1.45f, 2.35f, -6.2f),
                new Vector3(0.3f, 1.15f, 4.2f), 52f);
            AssertPose(AuditionPvCityShot.G02, 240,
                new Vector3(3f, 2.25f, -4.8f),
                new Vector3(0.5f, 1.15f, 5.2f), 55f);
            AssertPose(AuditionPvCityShot.G02, 419,
                new Vector3(1.1f, 2.4f, -4.8f),
                new Vector3(0.3f, 1.15f, 5.4f), 52f);
            AssertPose(AuditionPvCityShot.G03, 180,
                new Vector3(1.25f, 2.35f, 4.8f),
                new Vector3(0f, 2.8f, 10.45f), 38f);
            AssertPose(AuditionPvCityShot.G03, 299,
                new Vector3(0.4f, 2.5f, 6.7f),
                new Vector3(0f, 3f, 10.55f), 36f);

            AuditionPvCityCameraRailPose half =
                AuditionPvCityHeroPocketCapture.EvaluateRail(
                    AuditionPvCityShot.G02,
                    96);
            Assert.That(half.position,
                Is.EqualTo(Vector3.LerpUnclamped(
                    new Vector3(-0.35f, 2.35f, -10.2f),
                    new Vector3(1.45f, 2.35f, -6.2f),
                    0.5f)));
        }

        [Test]
        public void PositiveDepthViewportRectIntersection_IsPureAndFailClosed()
        {
            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .PositiveDepthViewportRectIntersects(new[]
                    {
                        new Vector3(-0.2f, 0.2f, 2f),
                        new Vector3(0.2f, 0.8f, 2f)
                    }),
                Is.True);
            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .PositiveDepthViewportRectIntersects(new[]
                    {
                        new Vector3(0.2f, 0.2f, -1f),
                        new Vector3(0.8f, 0.8f, -1f)
                    }),
                Is.False);
            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .PositiveDepthViewportRectIntersects(new[]
                    {
                        new Vector3(1.1f, 0.2f, 1f),
                        new Vector3(1.4f, 0.8f, 1f)
                    }),
                Is.False);
            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .PositiveDepthViewportRectIntersects(Array.Empty<Vector3>()),
                Is.False);
        }

        [Test]
        public void CameraReadback_UsesActualComposedTransformAndRejectsOmittedWrite()
        {
            var basePose = new AuditionPvCityCameraRailPose(
                new Vector3(3f, 2f, -7f),
                new Vector3(0.5f, 1.5f, 4f),
                52f);
            Vector3 sourcePosition = new(0.08f, -0.01f, 0.02f);
            Vector3 sourceEuler = new(0.3f, -0.2f, 0.1f);
            Quaternion baseRotation = Quaternion.LookRotation(
                (basePose.lookAt - basePose.position).normalized,
                Vector3.up);
            Vector3 expectedPosition = basePose.position + baseRotation
                * Vector3.ClampMagnitude(
                    sourcePosition,
                    AuditionPvCityHeroPocketCapture
                        .CameraRailMicroShakePositionClamp);
            Quaternion expectedRotation = baseRotation * Quaternion.Euler(
                Vector3.ClampMagnitude(
                    sourceEuler,
                    AuditionPvCityHeroPocketCapture.CameraRailMicroShakeEulerClamp));

            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .CameraReadbackMatchesExpectedComposition(
                        basePose,
                        sourcePosition,
                        sourceEuler,
                        expectedPosition,
                        expectedRotation,
                        basePose.fieldOfView,
                        out float positionError,
                        out float rotationError,
                        out float fovError),
                Is.True);
            Assert.That(positionError, Is.LessThanOrEqualTo(
                AuditionPvCityHeroPocketCapture.CameraRailPositionReadbackTolerance));
            Assert.That(rotationError, Is.LessThanOrEqualTo(
                AuditionPvCityHeroPocketCapture
                    .CameraRailRotationReadbackToleranceDegrees));
            Assert.That(fovError, Is.LessThanOrEqualTo(
                AuditionPvCityHeroPocketCapture.CameraRailFovReadbackTolerance));

            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .CameraReadbackMatchesExpectedComposition(
                        basePose,
                        sourcePosition,
                        sourceEuler,
                        expectedPosition + Vector3.right * 0.01f,
                        expectedRotation,
                        basePose.fieldOfView,
                        out _, out _, out _),
                Is.False,
                "A wrong actual gameplay-camera transform must fail closed.");
            Assert.That(
                AuditionPvCityHeroPocketCapture
                    .CameraReadbackMatchesExpectedComposition(
                        basePose,
                        sourcePosition,
                        sourceEuler,
                        Vector3.zero,
                        Quaternion.identity,
                        60f,
                        out _, out _, out _),
                Is.False,
                "An omitted gameplay-camera write must fail closed.");
        }

        [Test]
        public void ExplicitDependencies_AreExactResolveAndIncludeCaptureFoundation()
        {
            string[] expected =
            {
                "Assets/_Game/Scenes/CityHeroPocketStage.unity",
                "Assets/_Game/Editor/CityHeroPocket/CityHeroPocketSceneSetup.cs",
                "Assets/_Game/Editor/CityHeroPocket/CityHeroPocketAuthoredPackValidator.cs",
                "Assets/_Game/Prefabs/Player/PF_Player_Inori_RangedActionFoundation.prefab",
                "Assets/_Game/Art/Environment/CityHeroPocket/Profiles/DB_CityHeroPocket_PostProcess.asset",
                "Assets/_Game/Art/Environment/CityHeroPocket/Materials/DB_CityHeroPocket_Asphalt.mat",
                "Assets/_Game/Art/Environment/CityHeroPocket/Materials/DB_CityHeroPocket_Sidewalk.mat",
                "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Ranged_RifleCrossfire.prefab",
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RifleCrossfire.asset",
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RifleCrossfireDeck.asset",
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab",
                "Assets/_Game/Prefabs/Combat/PF_EnemyProjectile_RifleCrossfire.prefab",
                "Assets/_Game/Scripts/Combat/LaneActionProjectile.cs",
                "Assets/_Game/Scripts/Combat/SummonEnergyLadder.cs",
                "Assets/_Game/Scripts/Combat/SummonFrontlineProxy.cs",
                "Assets/_Game/Scripts/Combat/SummonPressureScreen.cs",
                "Assets/_Game/Scripts/Player/PlayerSummonSlot1Action.cs",
                "Assets/_Game/Scripts/Player/PlayerSummonSlot1Action.Runtime.cs",
                "Assets/_Game/Scripts/Player/SummonSlotActionProfile.cs",
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset",
                "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab",
                "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab",
                "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab",
                "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab",
                "Assets/_Game/UI/CombatHud/OneRowCombatHudBinder.cs",
                "Assets/_Game/UI/CombatHud/CombatHudInputBridge.cs",
                "Assets/_Game/UI/CombatHud/CombatHudVirtualJoystick.cs",
                "Assets/_Game/UI/CombatHud/CombatHudPointerActionInput.cs",
                "Assets/_Game/UI/CombatHud/CombatHudAimDragInput.cs",
                "Assets/_Game/UI/CombatHud/CombatSessionOverlayPresenter.cs",
                "Assets/_Game/Scripts/Player/PlayerMovementController.cs",
                "Assets/_Game/Scripts/Player/PlayerActionController.cs",
                "Assets/_Game/Scripts/Player/PlayerCombatModeController.cs",
                "Assets/_Game/Scripts/Player/PlayerInputLockSource.cs",
                "Assets/_Game/Scripts/Player/PlayerRangedBasicAttackAction.cs",
                "Assets/_Game/Scripts/Player/PlayerCombatTargetSelector.cs",
                "Assets/_Game/Scripts/Enemies/BasicSoldierEnemy.cs",
                "Assets/_Game/Scripts/Enemies/BasicSoldierProjectileAttackDriver.cs",
                "Assets/_Game/Scripts/LevelDesign/CityHeroPocketEnemyProjectileRootBinder.cs",
                "Assets/_Game/Scripts/Combat/CombatEncounterController.cs",
                "Assets/_Game/Scripts/Presentation/ActionCameraController.cs",
                "Assets/_Game/Scripts/Presentation/PresentationClock.cs",
                "Assets/_Game/Scripts/LevelDesign/CityHeroPocketExitTransitionController.cs",
                "Assets/_Game/Art/VFX/CombatCues/Prefabs/DB_VFX_PlayerSummonPreSpawnPortal.prefab",
                "Assets/_Game/Editor/AuditionPV/AuditionPvCityHeroPocketCapture.cs",
                "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHeroPocketCaptureTests.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvCaptureContract.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvRecorderSettingsFactory.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvCaptureManifest.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvEnvironmentProbe.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvSixtySecondGateManifest.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvCityHeroPocketGoldenRunner.cs",
                "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHeroPocketGoldenRunnerTests.cs"
            };
            string[] actual =
                AuditionPvCityHeroPocketCapture.ExplicitProductDependencyPaths();
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(actual.Length));
            foreach (string path in actual)
            {
                Assert.That(AssetDatabase.LoadMainAssetAtPath(path), Is.Not.Null, path);
            }
        }

        [Test]
        public void CaptureSource_UsesOnlyTypedPublicGameplayAndCameraTruth()
        {
            string source = ReadProjectFile(
                AuditionPvCityHeroPocketCapture.CaptureScriptPath);
            Assert.That(source, Does.Contain("ExecuteEvents.ExecuteHierarchy("));
            Assert.That(source, Does.Contain("Topmost actionable HUD raycast"));
            Assert.That(source, Does.Contain("G03JoystickPointerId = 401"));
            Assert.That(source, Does.Contain("SetGameplaySuspended(true)"));
            Assert.That(source, Does.Contain("PrimeFromHandoffCamera("));
            Assert.That(source, Does.Contain("GeometryUtility.TestPlanesAABB("));
            Assert.That(source, Does.Contain("RejectedTriggerEnterCount"));
            Assert.That(source, Does.Contain("PrepareContinuationShot("));
            Assert.That(source, Does.Contain("DeepCopyRuntimeProof("));
            Assert.That(source, Does.Contain(
                "g03JoystickInputMaintainedUntilTrigger &="));
            Assert.That(source, Does.Contain(
                "g02PlayerFramingSampleCount++"));
            Assert.That(source, Does.Contain(
                "g02EnemyFramingSampleCount++"));
            Assert.That(source, Does.Contain("BuildG02CompletionDiagnostics()"));
            Assert.That(source, Does.Contain("ignoredLedgerExact="));
            Assert.That(source, Does.Contain("ArmG02RecorderWarmupSuspension()"));
            Assert.That(source, Does.Contain(
                "ReleaseG02RecorderWarmupSuspension("));
            Assert.That(source, Does.Contain(
                "AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime;"));
            Assert.That(source, Does.Contain(
                "Time.fixedDeltaTime = savedFixedDeltaTime;"));
            Assert.That(source, Does.Contain(
                "RequireCaptureFixedDeltaTimeExact(\"logical frame zero\")"));
            Assert.That(source, Does.Contain("RestoreSessionGlobalState()"));
            Assert.That(source, Does.Contain("RawRuntimeProofJson="));
            Assert.That(source, Does.Contain(
                "RangedReloadCompleted += HandleRangedReloadCompleted"));
            Assert.That(source, Does.Contain(
                "RangedReloadCompleted -= HandleRangedReloadCompleted"));
            Assert.That(source, Does.Contain(
                "RangedReloadCanceled += HandleRangedReloadCanceled"));
            Assert.That(source, Does.Contain(
                "RangedReloadCanceled -= HandleRangedReloadCanceled"));
            foreach (string forbidden in new[]
                     {
                         "characterController.Move(",
                         "CharacterController.Move(",
                         "g03PreRollAttack",
                         "StagePlayerOutsideExitTrigger",
                         "CrossExitTriggerAtFrameZero",
                         "StartTransition(",
                         "BeginTransition(",
                         "TriggerTransition(",
                         "TryApplyDamage(",
                         "ConfigureMaxHealth(",
                         "ResetHealthToFull(",
                         "Object.Instantiate(",
                         "SerializedObject",
                         "System.Reflection",
                         "BindingFlags",
                         "SendMessage(",
                         ".isVisible",
                         "playerHealth.transform.position =",
                         "enemyHealth.transform.position ="
                     })
            {
                Assert.That(source, Does.Not.Contain(forbidden), forbidden);
            }
        }

        [Test]
        public void RuntimeProof_PassingFixturesCoverAllThreeShots()
        {
            foreach (AuditionPvCityShot shot in Enum.GetValues(typeof(AuditionPvCityShot)))
            {
                Assert.DoesNotThrow(() =>
                    AuditionPvCityHeroPocketCapture.ValidateRuntimeProof(
                        PassingProof(shot)));
            }
        }

        [Test]
        public void RuntimeProof_RejectsMissingOrWrongActualCameraReadback()
        {
            foreach (AuditionPvCityShot shot in Enum.GetValues(typeof(AuditionPvCityShot)))
            {
                AssertRejected(shot,
                    value => value.cameraRailActualReadbackFrameCount--);
                AssertRejected(shot,
                    value => value.cameraRailActualComposedPoseExact = false);
                AssertRejected(shot, value =>
                    value.maximumCameraPositionReadbackError =
                        AuditionPvCityHeroPocketCapture
                            .CameraRailPositionReadbackTolerance + 0.00001f);
                AssertRejected(shot, value =>
                    value.maximumCameraRotationReadbackErrorDegrees =
                        AuditionPvCityHeroPocketCapture
                            .CameraRailRotationReadbackToleranceDegrees + 0.001f);
                AssertRejected(shot, value =>
                    value.maximumCameraFovReadbackError =
                        AuditionPvCityHeroPocketCapture
                            .CameraRailFovReadbackTolerance + 0.00001f);
            }
        }

        [Test]
        public void RuntimeProof_RejectsFixedTimestepDriftOrWrongCleanupOwnership()
        {
            foreach (AuditionPvCityShot shot in Enum.GetValues(typeof(AuditionPvCityShot)))
            {
                AssertRejected(shot, value => value.originalFixedDeltaTime = 0f);
                AssertRejected(shot, value => value.fixedDeltaTimeAtPreparation = 0.02f);
                AssertRejected(shot, value =>
                    value.fixedDeltaTimeAtLogicalFrameZero = 0.02f);
                AssertRejected(shot, value =>
                    value.fixedDeltaTimeAtLastLogicalFrame = 0.02f);
                AssertRejected(shot, value =>
                    value.fixedDeltaTimeLogicalFrameSampleCount--);
                AssertRejected(shot, value =>
                    value.fixedDeltaTimeExactThroughoutShot = false);
            }

            AssertRejected(AuditionPvCityShot.G01, value =>
                value.fixedDeltaTimeLeasePreservedForContinuation = false);
            AssertRejected(AuditionPvCityShot.G01, value =>
                value.fixedDeltaTimeRestored = true);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.fixedDeltaTimeLeasePreservedForContinuation = false);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.fixedDeltaTimeRestored = true);
            AssertRejected(AuditionPvCityShot.G03, value =>
                value.fixedDeltaTimeLeasePreservedForContinuation = true);
            AssertRejected(AuditionPvCityShot.G03, value =>
                value.fixedDeltaTimeRestored = false);
        }

        [Test]
        public void RuntimeProof_G02FailureNamesEveryPredicateAndEmbedsRawProofJson()
        {
            AuditionPvCityHeroPocketRuntimeProof proof =
                PassingProof(AuditionPvCityShot.G02);
            proof.ammoAtShotEnd = 13;
            proof.g02ReloadStartedCount = 3;
            proof.g02ReloadCompletedCount = 0;
            proof.g02ReloadCanceledCount = 3;
            proof.g02ReloadRefilledAmmoCount = 2;
            proof.g02ReloadingAtShotEnd = true;
            proof.g02ReloadLifecycleLedger = new[]
            {
                new AuditionPvCityReloadLifecycleLedgerEntry
                {
                    eventName = "started",
                    logicalFrame = 55,
                    unityFrame = 2055,
                    ammo = 22,
                    isReloading = true
                },
                new AuditionPvCityReloadLifecycleLedgerEntry
                {
                    eventName = "canceled",
                    logicalFrame = 84,
                    unityFrame = 2084,
                    ammo = 22,
                    isReloading = false
                }
            };

            InvalidOperationException failure = Assert.Throws<
                InvalidOperationException>(() =>
                AuditionPvCityHeroPocketCapture.ValidateRuntimeProof(proof));
            Assert.That(failure.Message, Does.Contain(
                nameof(proof.ammoAtShotEnd)));
            Assert.That(failure.Message, Does.Contain(
                nameof(proof.g02ReloadStartedCount)));
            Assert.That(failure.Message, Does.Contain(
                nameof(proof.g02ReloadLifecycleLedger)));
            Assert.That(failure.Message, Does.Contain(
                "ammoAtShotStart + g02ReloadRefilledAmmoCount - ammoAtShotEnd"));

            const string marker = "RawRuntimeProofJson=";
            int rawJsonStart = failure.Message.IndexOf(
                marker,
                StringComparison.Ordinal);
            Assert.That(rawJsonStart, Is.GreaterThanOrEqualTo(0));
            string rawJson = failure.Message.Substring(
                rawJsonStart + marker.Length);
            AuditionPvCityHeroPocketRuntimeProof preserved =
                JsonUtility.FromJson<AuditionPvCityHeroPocketRuntimeProof>(rawJson);
            Assert.That(preserved.shotId, Is.EqualTo("g02"));
            Assert.That(preserved.ammoAtShotEnd, Is.EqualTo(13));
            Assert.That(preserved.g02ReloadStartedCount, Is.EqualTo(3));
            Assert.That(preserved.g02ReloadCompletedCount, Is.EqualTo(0));
            Assert.That(preserved.g02ReloadCanceledCount, Is.EqualTo(3));
            Assert.That(preserved.g02ReloadRefilledAmmoCount, Is.EqualTo(2));
            Assert.That(preserved.g02ReloadingAtShotEnd, Is.True);
            Assert.That(preserved.g02ReloadLifecycleLedger, Has.Length.EqualTo(2));
            Assert.That(
                preserved.g02ReloadLifecycleLedger[1].eventName,
                Is.EqualTo("canceled"));
        }

        [Test]
        public void G01Proof_RejectsFreezeCompositionAndRestorationDrift()
        {
            AssertRejected(AuditionPvCityShot.G01, value => value.g01PlayerDrift = 0.011f);
            AssertRejected(AuditionPvCityShot.G01, value => value.g01EnemyDrift = 0.011f);
            AssertRejected(AuditionPvCityShot.G01, value => value.ammoAtShotEnd--);
            AssertRejected(AuditionPvCityShot.G01, value => value.enemyFiredCountAtShotEnd++);
            AssertRejected(AuditionPvCityShot.G01, value => value.rangedProjectileFiredCount = 1);
            AssertRejected(AuditionPvCityShot.G01, value => value.g01GameplaySuspensionExact = false);
            AssertRejected(AuditionPvCityShot.G01, value => value.g01HudRootStayedActive = false);
            AssertRejected(AuditionPvCityShot.G01, value => value.g01CompositionPassingSampleCount = 0);
            AssertRejected(AuditionPvCityShot.G01, value => value.g01PlayerEnemyLineOfSightClear = false);
            AssertRejected(AuditionPvCityShot.G01, value => value.actionCameraTransientStateRestored = false);
        }

        [Test]
        public void G02Proof_RejectsEveryGameplayCameraAndHandoffBoundary()
        {
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02RecorderWarmupSuspensionAcquired = false);
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02RecorderWarmupEndOfFrameCount = 1);
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02RecorderWarmupSuspensionHeldUntilLogicalFrameZero = false);
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02RecorderWarmupProductStateUnchanged = false);
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02RecorderWarmupSuspensionReleasedBeforeLogicalFrameZero = false);
            AssertRejected(AuditionPvCityShot.G02, value => value.rangedProjectileFiredCount = 9);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02UsesMagazineReload = false);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02MagazineSize = 23);
            AssertRejected(AuditionPvCityShot.G02, value => value.ammoAtShotStart = 23);
            AssertRejected(AuditionPvCityShot.G02, value => value.ammoAtShotEnd = 15);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadingAtShotStart = true);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadStartedCount = 3);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadCompletedCount = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadCanceledCount = 1);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadRefilledAmmoCount = 2);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadLifecycleStateExact = false);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadingAtShotEnd = false);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02ReloadLifecycleLedger[0].logicalFrame = 54);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02ReloadLifecycleLedger[1].eventName = "completed");
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02ReloadLifecycleLedger[5].ammo = 23);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02ReloadLifecycleLedger[6].isReloading = false);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02PlayerPathLength = 5.99f);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02PlayerNetDisplacement = 1.99f);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02MaximumFrameDisplacement = 0.752f);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02DodgeDownFrame = 239);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02DodgeStartedCount = 2);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02DodgeDirectionRailRightDot = 0.55f);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02EnemyTelegraphObserved = false);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02EnemyTelegraphVisibleFrameCount = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02EnemyFiredDelta = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ProjectileRootsIndependentAndSceneOwned = false);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02PlayerProjectileVisibleFrameCount = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02EnemyProjectileVisibleFrameCount = 0);
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02IgnoredLaneActionProjectileTriggerEnterCount = 2);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02IgnoredLaneActionProjectileTriggerEnterLedger[0].logicalFrame = 326);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02IgnoredLaneActionProjectileTriggerEnterLedger[1]
                    .projectileInstanceId = value
                    .g02IgnoredLaneActionProjectileTriggerEnterLedger[0]
                    .projectileInstanceId);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02IgnoredLaneActionProjectileTriggerEnterLedger[0]
                    .projectileWasActive = false);
            AssertRejected(AuditionPvCityShot.G02,
                value => value.g02RejectedTriggerEnterCount = 1);
            AssertRejected(AuditionPvCityShot.G02, value =>
                value.g02RejectedTriggerEnterLedger = new[]
                {
                    PassingIgnoredProjectileLedger()[0]
                });
            AssertRejected(AuditionPvCityShot.G02, value => value.g02PlayerFramingPassCount = 3);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02EnemyFramingPassCount = 2);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02RifleFeedbackRequestDelta = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02MicroShakeRequestDelta = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.microShakeComposedFrameCount = 4);
            AssertRejected(AuditionPvCityShot.G02, value => value.enemyDiedCount = 0);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02EndedSouthOfExitTrigger = false);
        }

        [Test]
        public void G03Proof_RejectsReplayWrongColliderHudMutationAndTimingRace()
        {
            AssertRejected(AuditionPvCityShot.G03, value => value.g03StartedAlreadyWon = false);
            AssertRejected(AuditionPvCityShot.G03, value => value.g03StartedSouthOfExitTrigger = false);
            AssertRejected(AuditionPvCityShot.G03, value => value.g03JoystickPointerId = 402);
            AssertRejected(AuditionPvCityShot.G03, value => value.g03JoystickInputExact = false);
            AssertRejected(AuditionPvCityShot.G03, value =>
                value.g03JoystickInputMaintainedUntilTrigger = false);
            AssertRejected(AuditionPvCityShot.G03, value =>
                value.g03JoystickInputExactAtTrigger = false);
            AssertRejected(AuditionPvCityShot.G03, value => value.g03TriggerAcceptedPreRollFrame = 35);
            AssertRejected(AuditionPvCityShot.G03, value => value.g03TriggerAcceptedPreRollFrame = 85);
            AssertRejected(AuditionPvCityShot.G03, value => value.g03NewDamageEventCount = 1);
            AssertRejected(AuditionPvCityShot.G03,
                value => value.transitionIgnoredLaneActionProjectileBaseline = 2);
            AssertRejected(AuditionPvCityShot.G03,
                value => value.transitionIgnoredLaneActionProjectileEnd = 4);
            AssertRejected(AuditionPvCityShot.G03,
                value => value.transitionIgnoredLaneActionProjectileDelta = 1);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionRejectedTriggerEnterBaseline = 1);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionRejectedTriggerEnterEnd = 1);
            AssertRejected(AuditionPvCityShot.G03, value => value.captureTransitionStartCallCount = 1);
            AssertRejected(AuditionPvCityShot.G03, value => value.captureHudMutationCount = 1);
            AssertRejected(AuditionPvCityShot.G03, value => value.hudHiddenBeforeLogicalFrameZero = false);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionAdvancedOnceBeforeRailFrameZero = false);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionFrameAtLogicalFrameZero = 20);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionPortalAuthoredLogicalFrame = 20);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionFullCoverLogicalFrame = 277);
            AssertRejected(AuditionPvCityShot.G03, value => value.cleanCoverFrameCount = 23);
            AssertRejected(AuditionPvCityShot.G03, value => value.transitionInputAndAiLocked = false);
        }

        [Test]
        public void ProofDeepCopy_G02SealCannotBeMutatedByG03Reuse()
        {
            AuditionPvCityHeroPocketRuntimeProof mutable =
                PassingProof(AuditionPvCityShot.G02);
            mutable.g02ReloadLifecycleLedger = new[]
            {
                new AuditionPvCityReloadLifecycleLedgerEntry
                {
                    eventName = "started",
                    logicalFrame = 55,
                    unityFrame = 2055,
                    ammo = 22,
                    isReloading = true
                }
            };
            AuditionPvCityHeroPocketRuntimeProof sealedCopy =
                AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(mutable);
            mutable.shotId = "g03";
            mutable.enemyDiedCount = 0;
            mutable.g02PlayerPathLength = 0f;
            mutable.g02IgnoredLaneActionProjectileTriggerEnterLedger[0]
                .colliderName = "mutated";
            mutable.g02ReloadLifecycleLedger[0].eventName = "mutated";

            Assert.That(sealedCopy, Is.Not.SameAs(mutable));
            Assert.That(sealedCopy.shotId, Is.EqualTo("g02"));
            Assert.That(sealedCopy.enemyDiedCount, Is.EqualTo(1));
            Assert.That(sealedCopy.g02PlayerPathLength, Is.EqualTo(6f));
            Assert.That(
                sealedCopy.g02IgnoredLaneActionProjectileTriggerEnterLedger[0]
                    .colliderName,
                Is.EqualTo("PF_PlayerRangedBasicProjectile_AimBolt(Clone)"));
            Assert.That(
                sealedCopy.g02ReloadLifecycleLedger[0].eventName,
                Is.EqualTo("started"));

            AuditionPvCityHeroPocketRuntimeProof exposed =
                AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(sealedCopy);
            exposed.g02PlayerPathLength = -1f;
            Assert.That(sealedCopy.g02PlayerPathLength, Is.EqualTo(6f));
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator SameDirectorNoRecorder_G01G02MeetExactG03Preconditions()
        {
            SceneSetup[] priorSceneSetup = EditorSceneManager.GetSceneManagerSetup();
            bool priorSceneSetupIsRestorable = IsRestorableSceneSetup(
                priorSceneSetup);
            Scene[] loadedScenes = Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .ToArray();
            bool hasInteractiveSceneRisk = priorSceneSetup.Length == 0
                || loadedScenes.Any(scene => !scene.IsValid()
                    || string.IsNullOrWhiteSpace(scene.path)
                    || scene.isDirty);
            if (!Application.isBatchMode && hasInteractiveSceneRisk)
            {
                throw new InvalidOperationException(
                    "The slow City no-Recorder diagnostic refuses to replace an "
                    + "unsaved or dirty interactive SceneSetup.");
            }

            Exception capturedFailure = null;
            try
            {
                EditorSceneManager.OpenScene(
                    AuditionPvCityHeroPocketCapture.CityScenePath,
                    OpenSceneMode.Single);
            }
            catch (Exception exception)
            {
                capturedFailure = exception;
            }
            if (capturedFailure != null)
            {
                try
                {
                    RestoreSceneSetupOrLeaveCleanEmpty(
                        priorSceneSetup,
                        priorSceneSetupIsRestorable);
                }
                finally
                {
                    Assert.Fail(
                        "The City product scene could not be opened safely: "
                        + capturedFailure);
                }
                yield break;
            }

            yield return new EnterPlayMode();
            yield return null;

            float fixedDeltaTimeBeforeDirector = Time.fixedDeltaTime;
            int captureFramerateBeforeDirector = Time.captureFramerate;
            int targetFrameRateBeforeDirector = Application.targetFrameRate;
            UnityEngine.Random.State randomStateBeforeDirector =
                UnityEngine.Random.state;

            string diagnostics = "G03 precondition diagnostics were not reached.";
            AuditionPvCityHeroPocketRuntimeProof observedG02Proof = null;
            AuditionPvCityHeroPocketDirector director = null;
            AuditionPvCityRecorderWarmupBeginHarness warmupBeginHarness = null;
            try
            {
                director = AuditionPvCityHeroPocketCapture.AttachToFreshActiveScene(
                    AuditionPvCityShot.G01);
            }
            catch (Exception exception)
            {
                capturedFailure = exception;
            }

            IEnumerator preparation = null;
            if (capturedFailure == null)
            {
                try
                {
                    preparation = director.PrepareFreshProductState();
                }
                catch (Exception exception)
                {
                    capturedFailure = exception;
                }
            }
            while (capturedFailure == null && preparation != null)
            {
                bool moved = TryMoveNext(
                    preparation,
                    out object yielded,
                    out Exception iterationFailure);
                capturedFailure ??= iterationFailure;
                if (capturedFailure != null || !moved)
                {
                    break;
                }
                yield return yielded;
            }
            if (capturedFailure == null
                && Time.fixedDeltaTime
                    != AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime)
            {
                capturedFailure = new InvalidOperationException(
                    "The City director did not acquire the exact 1/Fps fixed timestep "
                    + "before G01.");
            }

            if (capturedFailure == null)
            {
                try
                {
                    director.BeginShot();
                }
                catch (Exception exception)
                {
                    capturedFailure = exception;
                }
            }
            double deadline = Time.realtimeSinceStartupAsDouble + 12d;
            while (capturedFailure == null && director != null
                && !director.IsComplete && director.Failure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }
            if (capturedFailure == null && director != null)
            {
                capturedFailure = director.Failure;
                if (capturedFailure == null && !director.IsComplete)
                {
                    capturedFailure = new TimeoutException(
                        $"G01 no-Recorder diagnostic timed out at f{director.CurrentFrame}.");
                }
            }

            var ignoredProjectileLedger =
                new List<AuditionPvCityTriggerEnterLedgerEntry>();
            var rejectedLedger =
                new List<AuditionPvCityTriggerEnterLedgerEntry>();
            CityHeroPocketExitTransitionController transition = null;
            Action<Collider, LaneActionProjectile> handleIgnored =
                (collider, projectile) => ignoredProjectileLedger.Add(
                    AuditionPvCityHeroPocketDirector.CreateTriggerEnterLedgerEntry(
                        collider,
                        projectile,
                        director != null ? director.CurrentFrame : -1));
            Action<Collider> handleRejected = collider => rejectedLedger.Add(
                AuditionPvCityHeroPocketDirector.CreateTriggerEnterLedgerEntry(
                    collider,
                    collider != null
                        ? collider.GetComponentInParent<LaneActionProjectile>()
                        : null,
                    director != null ? director.CurrentFrame : -1));
            bool ignoredHandlerSubscribed = false;
            bool rejectedHandlerSubscribed = false;
            try
            {
                transition = UnityEngine.Object.FindFirstObjectByType<
                    CityHeroPocketExitTransitionController>(
                    FindObjectsInactive.Include);
                if (capturedFailure == null && transition == null)
                {
                    capturedFailure = new InvalidOperationException(
                        "No City exit transition was available for the G02 rejection ledger.");
                }
                if (transition != null)
                {
                    transition.LaneActionProjectileTriggerEnterIgnored += handleIgnored;
                    ignoredHandlerSubscribed = true;
                    transition.TriggerEnterRejected += handleRejected;
                    rejectedHandlerSubscribed = true;
                }
            }
            catch (Exception exception)
            {
                MergeFailure(ref capturedFailure, exception);
            }
            try
            {
            IEnumerator continuation = null;
            if (capturedFailure == null)
            {
                try
                {
                    continuation = director.PrepareContinuationShot(
                        AuditionPvCityShot.G02);
                }
                catch (Exception exception)
                {
                    capturedFailure = exception;
                }
            }
            while (capturedFailure == null && continuation != null)
            {
                bool moved = TryMoveNext(
                    continuation,
                    out object yielded,
                    out Exception iterationFailure);
                capturedFailure ??= iterationFailure;
                if (capturedFailure != null || !moved)
                {
                    break;
                }
                yield return yielded;
            }

            if (capturedFailure == null)
            {
                try
                {
                    warmupBeginHarness = director.gameObject.AddComponent<
                        AuditionPvCityRecorderWarmupBeginHarness>();
                    warmupBeginHarness.Configure(director);
                    director.ArmG02RecorderWarmupSuspension();
                }
                catch (Exception exception)
                {
                    capturedFailure = exception;
                }
            }
            if (capturedFailure == null)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                try
                {
                    warmupBeginHarness.Arm();
                }
                catch (Exception exception)
                {
                    capturedFailure = exception;
                }
            }
            if (capturedFailure == null)
            {
                double beginDeadline = Time.realtimeSinceStartupAsDouble + 2d;
                while (!warmupBeginHarness.Began
                    && warmupBeginHarness.Failure == null
                    && Time.realtimeSinceStartupAsDouble < beginDeadline)
                {
                    yield return null;
                }
                capturedFailure = warmupBeginHarness.Failure;
                if (capturedFailure == null && !warmupBeginHarness.Began)
                {
                    capturedFailure = new TimeoutException(
                        "The early Recorder warmup begin harness did not open logical f0 "
                        + "within two realtime seconds.");
                }
            }
            deadline = Time.realtimeSinceStartupAsDouble + 18d;
            while (capturedFailure == null && director != null
                && !director.IsComplete && director.Failure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }
            if (capturedFailure == null && director != null)
            {
                capturedFailure = director.Failure;
                if (capturedFailure == null && !director.IsComplete)
                {
                    capturedFailure = new TimeoutException(
                        $"G02 no-Recorder diagnostic timed out at f{director.CurrentFrame}.");
                }
            }

            if (capturedFailure == null)
            {
                try
                {
                    observedG02Proof = director.SnapshotRuntimeProof();
                    AuditionPvCityHeroPocketRuntimeProof g02Proof =
                        observedG02Proof;
                    diagnostics = director.GetG03ContinuationPreconditionDiagnostics();
                    diagnostics += " RawObservedG02BeforeSealJson="
                        + JsonUtility.ToJson(observedG02Proof) + ".";
                    if (g02Proof.presentedFrameCount != 420
                        || g02Proof.lastLogicalFrame != 419
                        || !g02Proof.directorCompleted
                        || !g02Proof.g02RecorderWarmupSuspensionAcquired
                        || g02Proof.g02RecorderWarmupEndOfFrameCount != 2
                        || !g02Proof
                            .g02RecorderWarmupSuspensionHeldUntilLogicalFrameZero
                        || !g02Proof.g02RecorderWarmupProductStateUnchanged
                        || !g02Proof
                            .g02RecorderWarmupSuspensionReleasedBeforeLogicalFrameZero
                        || !g02Proof.naturalWonObserved
                        || !g02Proof.naturalEnemyDeathObserved
                        || g02Proof.g02IgnoredLaneActionProjectileTriggerEnterCount != 3
                        || !AuditionPvCityHeroPocketCapture
                            .HasExactG02IgnoredProjectileTriggerLedger(
                                g02Proof
                                    .g02IgnoredLaneActionProjectileTriggerEnterLedger)
                        || !AuditionPvCityHeroPocketCapture
                            .HasExactG02IgnoredProjectileTriggerLedger(
                                ignoredProjectileLedger)
                        || g02Proof.g02RejectedTriggerEnterCount != 0
                        || g02Proof.g02RejectedTriggerEnterLedger.Length != 0
                        || rejectedLedger.Count != 0
                        || !g02Proof.g02EndedOutsideExitTrigger
                        || !g02Proof.g02EndedSouthOfExitTrigger
                        || g02Proof.fixedDeltaTimeAtPreparation
                            != AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime
                        || g02Proof.fixedDeltaTimeAtLogicalFrameZero
                            != AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime
                        || g02Proof.fixedDeltaTimeAtLastLogicalFrame
                            != AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime
                        || g02Proof.fixedDeltaTimeLogicalFrameSampleCount != 420
                        || !g02Proof.fixedDeltaTimeExactThroughoutShot)
                    {
                        capturedFailure = new InvalidOperationException(
                            "G02 did not reach its exact no-Recorder completion proof. "
                            + diagnostics);
                    }
                }
                catch (Exception exception)
                {
                    MergeFailure(ref capturedFailure, exception);
                }
            }
            if (capturedFailure == null)
            {
                try
                {
                    director.ValidateG03ContinuationPreconditions();
                }
                catch (Exception exception)
                {
                    capturedFailure = exception;
                }
            }
            }
            finally
            {
                if (transition != null && ignoredHandlerSubscribed)
                {
                    try
                    {
                        transition.LaneActionProjectileTriggerEnterIgnored -= handleIgnored;
                    }
                    catch (Exception exception)
                    {
                        MergeFailure(ref capturedFailure, exception);
                    }
                }
                if (transition != null && rejectedHandlerSubscribed)
                {
                    try
                    {
                        transition.TriggerEnterRejected -= handleRejected;
                    }
                    catch (Exception exception)
                    {
                        MergeFailure(ref capturedFailure, exception);
                    }
                }
                if (director != null)
                {
                    try
                    {
                        diagnostics = director.GetG03ContinuationPreconditionDiagnostics();
                    }
                    catch (Exception exception)
                    {
                        MergeFailure(ref capturedFailure, exception);
                        diagnostics += " G03 diagnostics unavailable: " + exception;
                    }
                }
                try
                {
                    diagnostics += $" testIgnoredProjectileLedgerCount="
                        + $"{ignoredProjectileLedger.Count}; "
                        + $"testIgnoredProjectileLedger=[{string.Join(" || ", ignoredProjectileLedger)}]; "
                        + $"testRejectedLedgerCount={rejectedLedger.Count}; "
                        + $"testRejectedLedger=[{string.Join(" || ", rejectedLedger)}].";
                }
                catch (Exception exception)
                {
                    MergeFailure(ref capturedFailure, exception);
                }
            }

            if (director != null)
            {
                if (capturedFailure == null)
                {
                    try
                    {
                        var injectedCleanupFailure = new InvalidOperationException(
                            "expected continuation cleanup fault");
                        director.InjectContinuationCleanupFailureForTest(
                            injectedCleanupFailure);
                        IEnumerator faultedContinuation =
                            director.PrepareContinuationShot(
                                AuditionPvCityShot.G03);
                        bool moved = TryMoveNext(
                            faultedContinuation,
                            out _,
                            out Exception observedCleanupFailure);
                        if (moved || observedCleanupFailure == null
                            || !observedCleanupFailure.ToString().Contains(
                                injectedCleanupFailure.Message))
                        {
                            throw new InvalidOperationException(
                                "The continuation-cleanup fault seam did not fail at the "
                                + "expected already-cleaned restoration boundary.",
                                observedCleanupFailure);
                        }
                        AssertSessionGlobalsExact(
                            captureFramerateBeforeDirector,
                            targetFrameRateBeforeDirector,
                            fixedDeltaTimeBeforeDirector,
                            randomStateBeforeDirector,
                            "faulted continuation cleanup");

                        AuditionPvCityHeroPocketRuntimeProof sealedG02Proof =
                            director.LastSealedRuntimeProof;
                        if (sealedG02Proof == null)
                        {
                            throw new InvalidOperationException(
                                "The faulted G02 continuation did not preserve its "
                                + "post-cleanup sealed runtime proof.");
                        }
                        string sealedG02Json = JsonUtility.ToJson(sealedG02Proof);
                        diagnostics += " RawSealedG02RuntimeProofJson="
                            + sealedG02Json + ".";
                        try
                        {
                            AuditionPvCityHeroPocketCapture.ValidateRuntimeProof(
                                sealedG02Proof);
                        }
                        catch (Exception validationFailure)
                        {
                            throw new InvalidOperationException(
                                "The sealed no-Recorder G02 runtime proof failed full "
                                + "validation. RawSealedG02RuntimeProofJson="
                                + sealedG02Json,
                                validationFailure);
                        }
                    }
                    catch (Exception cleanupRegressionException)
                    {
                        MergeFailure(
                            ref capturedFailure,
                            cleanupRegressionException);
                    }
                }
                try
                {
                    director.RestoreShotState();
                    AuditionPvCityHeroPocketRuntimeProof restoredProof =
                        director.SnapshotRuntimeProof();
                    AssertSessionGlobalsExact(
                        captureFramerateBeforeDirector,
                        targetFrameRateBeforeDirector,
                        fixedDeltaTimeBeforeDirector,
                        randomStateBeforeDirector,
                        "explicit RestoreShotState");
                    if (!restoredProof.fixedDeltaTimeRestored
                        || restoredProof.fixedDeltaTimeLeasePreservedForContinuation)
                    {
                        throw new InvalidOperationException(
                            "Final City cleanup did not restore the exact authored fixed "
                            + "timestep proof. "
                            + $"proofRestored={restoredProof.fixedDeltaTimeRestored}; "
                            + "proofContinuationLease="
                            + $"{restoredProof.fixedDeltaTimeLeasePreservedForContinuation}.");
                    }
                    director.enabled = false;
                    AssertSessionGlobalsExact(
                        captureFramerateBeforeDirector,
                        targetFrameRateBeforeDirector,
                        fixedDeltaTimeBeforeDirector,
                        randomStateBeforeDirector,
                        "director OnDisable");
                }
                catch (Exception restoreException)
                {
                    MergeFailure(ref capturedFailure, restoreException);
                }
                try
                {
                    UnityEngine.Object.Destroy(director.gameObject);
                }
                catch (Exception destroyException)
                {
                    MergeFailure(ref capturedFailure, destroyException);
                }
                yield return null;
                try
                {
                    AssertSessionGlobalsExact(
                        captureFramerateBeforeDirector,
                        targetFrameRateBeforeDirector,
                        fixedDeltaTimeBeforeDirector,
                        randomStateBeforeDirector,
                        "director OnDestroy");
                }
                catch (Exception destroyRestoreException)
                {
                    MergeFailure(ref capturedFailure, destroyRestoreException);
                }
            }

            yield return new ExitPlayMode();
            try
            {
                Assert.That(
                    capturedFailure,
                    Is.Null,
                    "Same-director G01/G02 -> G03 precondition diagnostic failed. "
                    + diagnostics);
            }
            finally
            {
                RestoreSceneSetupOrLeaveCleanEmpty(
                    priorSceneSetup,
                    priorSceneSetupIsRestorable);
            }
        }

        private static AuditionPvCityHeroPocketRuntimeProof PassingProof(
            AuditionPvCityShot shot)
        {
            int count = AuditionPvCityHeroPocketCapture.GetExpectedFrameCount(shot);
            var proof = new AuditionPvCityHeroPocketRuntimeProof
            {
                shotId = AuditionPvCityHeroPocketCapture.GetShotId(shot),
                directorCompleted = true,
                lastLogicalFrame = count - 1,
                expectedFrameCount = count,
                presentedFrameCount = count,
                presentedFramesExact = true,
                presentationClockExact = true,
                hudModeExact = true,
                actionCameraStayedEnabled = true,
                cameraRailAppliedFrameCount = count,
                cameraRailBasePoseExact = true,
                cameraRailFovExact = true,
                cameraRailActualReadbackFrameCount = count,
                cameraRailActualComposedPoseExact = true,
                deterministicRandomSeed = 0xC172,
                originalFixedDeltaTime = 0.02f,
                fixedDeltaTimeAtPreparation =
                    AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime,
                fixedDeltaTimeAtLogicalFrameZero =
                    AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime,
                fixedDeltaTimeAtLastLogicalFrame =
                    AuditionPvCityHeroPocketCapture.CaptureFixedDeltaTime,
                fixedDeltaTimeLogicalFrameSampleCount = count,
                fixedDeltaTimeExactThroughoutShot = true,
                fixedDeltaTimeLeasePreservedForContinuation =
                    shot != AuditionPvCityShot.G03,
                fixedDeltaTimeRestored = shot == AuditionPvCityShot.G03,
                microShakeWithinClamp = true,
                stateRestored = true,
                presentationClockReleased = true,
                pointerLeasesReleased = true,
                hudStateRestored = true,
                cameraStateRestored = true,
                actionCameraStateRestored = true,
                actionCameraTransientStateRestored = true,
                cameraRailReleased = true,
                transitionStateRestored = true,
                encounterInstanceId = 101,
                playerInstanceId = 102,
                enemyInstanceId = 103
            };
            if (shot == AuditionPvCityShot.G01)
            {
                proof.playerHealthAtShotStart = 100f;
                proof.playerHealthAtShotEnd = 100f;
                proof.enemyHealthAtShotStart = 300f;
                proof.enemyHealthAtShotEnd = 300f;
                proof.ammoAtShotStart = 24;
                proof.ammoAtShotEnd = 24;
                proof.g01GameplaySuspensionExact = true;
                proof.g01HudRootStayedActive = true;
                proof.g01CompositionSampleCount = 3;
                proof.g01CompositionPassingSampleCount = 1;
                proof.g01ForegroundDepthObserved = true;
                proof.g01MidgroundActorsObserved = true;
                proof.g01BackgroundDepthObserved = true;
                proof.g01PlayerEnemyLineOfSightClear = true;
                proof.g01ThreeDepthCompositionObserved = true;
                proof.productOutcomePreservedForContinuation = true;
                return proof;
            }
            if (shot == AuditionPvCityShot.G02)
            {
                proof.pointerDownCount = 8;
                proof.pointerDragCount = 1;
                proof.pointerUpCount = 8;
                proof.g02PointerScheduleExact = true;
                proof.g02RecorderWarmupSuspensionAcquired = true;
                proof.g02RecorderWarmupEndOfFrameCount = 2;
                proof.g02RecorderWarmupSuspensionHeldUntilLogicalFrameZero = true;
                proof.g02RecorderWarmupProductStateUnchanged = true;
                proof.g02RecorderWarmupSuspensionReleasedBeforeLogicalFrameZero = true;
                proof.rangedProjectileFiredCount = 11;
                proof.enemyDamagedCount = 11;
                proof.enemyDiedCount = 1;
                proof.encounterWonCount = 1;
                proof.naturalEnemyDeathObserved = true;
                proof.naturalWonObserved = true;
                proof.ammoAtShotStart = 24;
                proof.ammoAtShotEnd = 16;
                proof.g02PlayerPathLength = 6f;
                proof.g02PlayerNetDisplacement = 2f;
                proof.g02MaximumFrameDisplacement = 0.75f;
                proof.g02DodgeDownFrame = 240;
                proof.g02DodgeStartedCount = 1;
                proof.g02DodgeEndedCount = 1;
                proof.g02DodgeDirectionRailRightDot = 0.56f;
                proof.g02PlayerAliveAtEnd = true;
                proof.g02PlayerStayedInBounds = true;
                proof.g02EnemyHealthAtEnd = 0f;
                proof.g02EnemyTelegraphObserved = true;
                proof.g02EnemyTelegraphVisibleFrameCount = 1;
                proof.g02EnemyFiredDelta = 1;
                proof.g02ProjectileRootsIndependentAndSceneOwned = true;
                proof.g02PlayerProjectileVisibleFrameCount = 1;
                proof.g02EnemyProjectileVisibleFrameCount = 1;
                proof.g02IgnoredLaneActionProjectileTriggerEnterCount = 3;
                proof.g02IgnoredLaneActionProjectileTriggerEnterLedger =
                    PassingIgnoredProjectileLedger();
                proof.g02PlayerFramingSampleCount = 4;
                proof.g02PlayerFramingPassCount = 4;
                proof.g02EnemyFramingSampleCount = 3;
                proof.g02EnemyFramingPassCount = 3;
                proof.g02RifleFeedbackRequestDelta = 1;
                proof.g02MicroShakeRequestDelta = 1;
                proof.g02UsesMagazineReload = true;
                proof.g02MagazineSize = 24;
                proof.g02ReloadingAtShotStart = false;
                proof.g02ReloadStartedCount = 4;
                proof.g02ReloadCompletedCount = 1;
                proof.g02ReloadCanceledCount = 2;
                proof.g02ReloadRefilledAmmoCount = 3;
                proof.g02ReloadLifecycleStateExact = true;
                proof.g02ReloadingAtShotEnd = true;
                proof.g02ReloadLifecycleLedger = PassingReloadLifecycleLedger();
                proof.microShakeSourceFrameCount = 5;
                proof.microShakeComposedFrameCount = 5;
                proof.g02EndedOutsideExitTrigger = true;
                proof.g02EndedSouthOfExitTrigger = true;
                proof.productOutcomePreservedForContinuation = true;
                return proof;
            }

            proof.continuityFromPreviousShot = true;
            proof.g03StartedAlreadyWon = true;
            proof.g03StartedTransitionArmed = true;
            proof.g03StartedOutsideExitTrigger = true;
            proof.g03StartedSouthOfExitTrigger = true;
            proof.naturalEnemyDeathObserved = true;
            proof.naturalWonObserved = true;
            proof.g03JoystickPointerId = 401;
            proof.g03JoystickInput = new Vector2(0.05f, 1f);
            proof.g03JoystickInputExact = true;
            proof.g03JoystickInputMaintainedUntilTrigger = true;
            proof.g03JoystickInputExactAtTrigger = true;
            proof.g03TriggerAcceptedPreRollFrame = 48;
            proof.g03PreRollPathLength = 3f;
            proof.g03PreRollNetDisplacement = 2.5f;
            proof.transitionTriggerAcceptedCount = 1;
            proof.transitionIgnoredLaneActionProjectileBaseline = 3;
            proof.transitionIgnoredLaneActionProjectileEnd = 3;
            proof.transitionStartedCount = 1;
            proof.transitionHudHiddenCount = 1;
            proof.transitionFullCoverCount = 1;
            proof.transitionExitReadyCount = 1;
            proof.hudHiddenBeforeLogicalFrameZero = true;
            proof.transitionFrameBeforeLogicalFrameZero = 20;
            proof.transitionFrameAtLogicalFrameZero = 21;
            proof.transitionAdvancedOnceBeforeRailFrameZero = true;
            proof.transitionPortalAuthoredLogicalFrame = 21;
            proof.transitionCoverStartedLogicalFrame = 213;
            proof.transitionFullCoverLogicalFrame = 273;
            proof.cleanCoverFrameCount = 24;
            proof.transitionPortalReachedAuthoredScale = true;
            proof.transitionCoverReachedFull = true;
            proof.transitionInputAndAiLocked = true;
            proof.transitionPresentationFrameAtEnd = 294;
            return proof;
        }

        private static AuditionPvCityTriggerEnterLedgerEntry[]
            PassingIgnoredProjectileLedger()
        {
            const string cloneName =
                "PF_PlayerRangedBasicProjectile_AimBolt(Clone)";
            const string hierarchy =
                "CityHeroPocketRuntime/CityHeroPocket_PlayerProjectiles/"
                + cloneName;
            return AuditionPvCityHeroPocketCapture.G02IgnoredProjectileTriggerFrames
                .Select((frame, index) =>
                    new AuditionPvCityTriggerEnterLedgerEntry
                    {
                        logicalFrame = frame,
                        unityFrame = 1000 + frame,
                        colliderName = cloneName,
                        colliderType = typeof(SphereCollider).FullName,
                        colliderInstanceId = 1100 + index,
                        layer = 0,
                        layerName = "Default",
                        hierarchy = hierarchy,
                        rootName = "CityHeroPocketRuntime",
                        rootInstanceId = 1200,
                        position = new Vector3(index, 0.5f, 7.4f),
                        rootPosition = Vector3.zero,
                        projectileName = cloneName,
                        projectileInstanceId = 1300 + index,
                        projectileHierarchy = hierarchy,
                        projectileWasActive = true
                    })
                .ToArray();
        }

        private static AuditionPvCityReloadLifecycleLedgerEntry[]
            PassingReloadLifecycleLedger()
        {
            return Enumerable.Range(
                    0,
                    AuditionPvCityHeroPocketCapture
                        .G02ReloadLifecycleFrames.Length)
                .Select(index => new AuditionPvCityReloadLifecycleLedgerEntry
                {
                    eventName = AuditionPvCityHeroPocketCapture
                        .G02ReloadLifecycleEventNames[index],
                    logicalFrame = AuditionPvCityHeroPocketCapture
                        .G02ReloadLifecycleFrames[index],
                    unityFrame = 2000 + AuditionPvCityHeroPocketCapture
                        .G02ReloadLifecycleFrames[index],
                    ammo = AuditionPvCityHeroPocketCapture
                        .G02ReloadLifecycleAmmo[index],
                    isReloading = AuditionPvCityHeroPocketCapture
                        .G02ReloadLifecycleIsReloading[index]
                })
                .ToArray();
        }

        private static void AssertRejected(
            AuditionPvCityShot shot,
            Action<AuditionPvCityHeroPocketRuntimeProof> mutate)
        {
            AuditionPvCityHeroPocketRuntimeProof proof = PassingProof(shot);
            mutate(proof);
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketCapture.ValidateRuntimeProof(proof));
        }

        private static void AssertPose(
            AuditionPvCityShot shot,
            int frame,
            Vector3 position,
            Vector3 lookAt,
            float fieldOfView)
        {
            AuditionPvCityCameraRailPose pose =
                AuditionPvCityHeroPocketCapture.EvaluateRail(shot, frame);
            Assert.That(Vector3.Distance(pose.position, position),
                Is.LessThanOrEqualTo(0.00001f));
            Assert.That(Vector3.Distance(pose.lookAt, lookAt),
                Is.LessThanOrEqualTo(0.00001f));
            Assert.That(pose.fieldOfView,
                Is.EqualTo(fieldOfView).Within(0.00001f));
        }

        private static bool TryMoveNext(
            IEnumerator routine,
            out object yielded,
            out Exception failure)
        {
            yielded = null;
            failure = null;
            try
            {
                bool moved = routine.MoveNext();
                if (moved)
                {
                    yielded = routine.Current;
                }
                return moved;
            }
            catch (Exception exception)
            {
                failure = exception;
                return false;
            }
        }

        private static void MergeFailure(
            ref Exception capturedFailure,
            Exception additionalFailure)
        {
            if (additionalFailure == null)
            {
                return;
            }
            capturedFailure = capturedFailure == null
                ? additionalFailure
                : new AggregateException(capturedFailure, additionalFailure);
        }

        private static void AssertSessionGlobalsExact(
            int expectedCaptureFramerate,
            int expectedTargetFrameRate,
            float expectedFixedDeltaTime,
            UnityEngine.Random.State expectedRandomState,
            string phase)
        {
            Assert.That(
                Time.captureFramerate,
                Is.EqualTo(expectedCaptureFramerate),
                phase + " captureFramerate");
            Assert.That(
                Application.targetFrameRate,
                Is.EqualTo(expectedTargetFrameRate),
                phase + " targetFrameRate");
            Assert.That(
                Time.fixedDeltaTime,
                Is.EqualTo(expectedFixedDeltaTime).Within(
                    AuditionPvCityHeroPocketCapture.FixedDeltaTimeRestoreTolerance),
                phase + " fixedDeltaTime");
            Assert.That(
                UnityEngine.Random.state.Equals(expectedRandomState),
                Is.True,
                phase + " Random.state");
        }

        private static bool IsRestorableSceneSetup(SceneSetup[] setup)
        {
            return setup != null
                && setup.Length > 0
                && setup.Any(value => value.isLoaded)
                && setup.All(value => !string.IsNullOrWhiteSpace(value.path)
                    && AssetDatabase.LoadAssetAtPath<SceneAsset>(value.path) != null);
        }

        private static void RestoreSceneSetupOrLeaveCleanEmpty(
            SceneSetup[] priorSceneSetup,
            bool priorSceneSetupIsRestorable)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "SceneSetup restoration is only legal after returning to EditMode.");
            }
            if (priorSceneSetupIsRestorable)
            {
                EditorSceneManager.RestoreSceneManagerSetup(priorSceneSetup);
                return;
            }

            Scene cleanScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            if (!cleanScene.IsValid() || cleanScene.isDirty
                || !string.IsNullOrEmpty(cleanScene.path))
            {
                throw new InvalidOperationException(
                    "The batch diagnostic could not leave a clean, unsaved empty scene.");
            }
        }

        private static string ReadProjectFile(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project root is unavailable.");
            return File.ReadAllText(Path.Combine(projectRoot, assetPath));
        }
    }
}
