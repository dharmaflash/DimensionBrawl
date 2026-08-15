using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
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
                Is.EqualTo(new[] { 240, 420, 300 }));
            Assert.That(shots.Select(value => value.endFrame),
                Is.EqualTo(new[] { 239, 419, 299 }));
            Assert.That(shots.Select(value => value.hudMode),
                Is.EqualTo(new[] { "hud-off", "hud-on", "hud-off" }));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
            Assert.That(
                AuditionPvCityHeroPocketCapture.DeterministicRandomSeed,
                Is.EqualTo(0xC172));

            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvCityHeroPocketCapture.CreateBaselineManifestEntries();
            Assert.That(baselines, Has.Length.EqualTo(2));
            Assert.That(baselines[0].sourceFrame, Is.EqualTo(120));
            Assert.That(baselines[0].fileName, Is.EqualTo(
                "BL01_CITY_HERO_WIDE__HUDOFF__t02.000000.png"));
            Assert.That(baselines[1].sourceFrame, Is.EqualTo(240));
            Assert.That(baselines[1].fileName, Is.EqualTo(
                "BL02_CITY_RIFLE_DODGE__HUDON__t04.000000.png"));
            Assert.That(
                AuditionPvCityHeroPocketCapture.FrameTimeSeconds(
                    AuditionPvCityShot.G02,
                    240),
                Is.EqualTo(4f));
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
            AssertRejected(AuditionPvCityShot.G02, value => value.rangedProjectileFiredCount = 9);
            AssertRejected(AuditionPvCityShot.G02, value => value.ammoAtShotEnd = 15);
            AssertRejected(AuditionPvCityShot.G02, value => value.g02ReloadStartedCount = 1);
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
            AuditionPvCityHeroPocketRuntimeProof sealedCopy =
                AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(mutable);
            mutable.shotId = "g03";
            mutable.enemyDiedCount = 0;
            mutable.g02PlayerPathLength = 0f;

            Assert.That(sealedCopy, Is.Not.SameAs(mutable));
            Assert.That(sealedCopy.shotId, Is.EqualTo("g02"));
            Assert.That(sealedCopy.enemyDiedCount, Is.EqualTo(1));
            Assert.That(sealedCopy.g02PlayerPathLength, Is.EqualTo(6f));

            AuditionPvCityHeroPocketRuntimeProof exposed =
                AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(sealedCopy);
            exposed.g02PlayerPathLength = -1f;
            Assert.That(sealedCopy.g02PlayerPathLength, Is.EqualTo(6f));
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
                proof.rangedProjectileFiredCount = 10;
                proof.enemyDamagedCount = 10;
                proof.enemyDiedCount = 1;
                proof.encounterWonCount = 1;
                proof.naturalEnemyDeathObserved = true;
                proof.naturalWonObserved = true;
                proof.ammoAtShotStart = 24;
                proof.ammoAtShotEnd = 14;
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
                proof.g02PlayerFramingSampleCount = 4;
                proof.g02PlayerFramingPassCount = 4;
                proof.g02EnemyFramingSampleCount = 3;
                proof.g02EnemyFramingPassCount = 3;
                proof.g02RifleFeedbackRequestDelta = 1;
                proof.g02MicroShakeRequestDelta = 1;
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

        private static string ReadProjectFile(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project root is unavailable.");
            return File.ReadAllText(Path.Combine(projectRoot, assetPath));
        }
    }
}
