using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    public enum AuditionPvCityShot
    {
        G01,
        G02,
        G03
    }

    internal enum AuditionPvCityInputTarget
    {
        MoveJoystick,
        BasicAttack,
        Dodge
    }

    internal enum AuditionPvCityPointerPhase
    {
        Down,
        Drag,
        Up
    }

    [Serializable]
    internal struct AuditionPvCityInputCommand
    {
        public int frame;
        public AuditionPvCityInputTarget target;
        public AuditionPvCityPointerPhase phase;
        public Vector2 value;

        public AuditionPvCityInputCommand(
            int newFrame,
            AuditionPvCityInputTarget newTarget,
            AuditionPvCityPointerPhase newPhase,
            Vector2 newValue = default)
        {
            frame = newFrame;
            target = newTarget;
            phase = newPhase;
            value = newValue;
        }
    }

    [Serializable]
    public struct AuditionPvCityCameraRailPose
    {
        public Vector3 position;
        public Vector3 lookAt;
        public float fieldOfView;

        public AuditionPvCityCameraRailPose(
            Vector3 newPosition,
            Vector3 newLookAt,
            float newFieldOfView)
        {
            position = newPosition;
            lookAt = newLookAt;
            fieldOfView = newFieldOfView;
        }
    }

    /// <summary>
    /// Product-state, frame-index-authoritative contracts for the three City Hero
    /// Pocket golden sources. Recorder lifecycle remains runner-owned.
    /// </summary>
    public static class AuditionPvCityHeroPocketCapture
    {
        internal const string CityScenePath =
            "Assets/_Game/Scenes/CityHeroPocketStage.unity";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvCityHeroPocketCapture.cs";
        internal const string CaptureTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHeroPocketCaptureTests.cs";
        internal const string ExitTransitionPath =
            "Assets/_Game/Scripts/LevelDesign/CityHeroPocketExitTransitionController.cs";
        internal const string PresentationClockPath =
            "Assets/_Game/Scripts/Presentation/PresentationClock.cs";
        internal const string ActionCameraPath =
            "Assets/_Game/Scripts/Presentation/ActionCameraController.cs";
        internal const string RangedActionPath =
            "Assets/_Game/Scripts/Player/PlayerRangedBasicAttackAction.cs";
        internal const string EncounterPath =
            "Assets/_Game/Scripts/Combat/CombatEncounterController.cs";
        internal const string HudPointerPath =
            "Assets/_Game/UI/CombatHud/CombatHudPointerActionInput.cs";
        internal const string HudJoystickPath =
            "Assets/_Game/UI/CombatHud/CombatHudVirtualJoystick.cs";

        internal const string G01ShotId = "g01";
        internal const string G02ShotId = "g02";
        internal const string G03ShotId = "g03";
        internal const int FirstFrame = 0;
        internal const int G01LastFrame = 239;
        internal const int G01ExpectedFrameCount = 240;
        internal const int G02LastFrame = 419;
        internal const int G02ExpectedFrameCount = 420;
        internal const int G03LastFrame = 299;
        internal const int G03ExpectedFrameCount = 300;
        internal const int Bl01SourceFrame = 120;
        internal const int Bl02SourceFrame = 240;
        internal const string Bl01FileName =
            "BL01_CITY_HERO_WIDE__HUDOFF__t02.000000.png";
        internal const string Bl02FileName =
            "BL02_CITY_RIFLE_DODGE__HUDON__t04.000000.png";
        internal const string BaselinesFolderName = "baselines";
        internal const int DeterministicRandomSeed = 0xC172;

        internal const int G02FirstMoveDownFrame = 12;
        internal const int G02FirstAttackDownFrame = 54;
        internal const int G02FirstAttackUpFrame = 55;
        internal const int G02SecondAttackDownFrame = 84;
        internal const int G02SecondAttackUpFrame = 85;
        internal const int G02MoveDragFrame = 108;
        internal const int G02ThirdAttackDownFrame = 126;
        internal const int G02ThirdAttackUpFrame = 127;
        internal const int G02FirstMoveUpFrame = 156;
        internal const int G02SecondMoveDownFrame = 192;
        internal const int G02DodgeDownFrame = 240;
        internal const int G02DodgeUpFrame = 241;
        internal const int G02SecondMoveUpFrame = 242;
        internal const int G02AttackHoldDownFrame = 264;
        internal const int G02AttackHoldUpFrame = 324;
        internal const int G02ThirdMoveDownFrame = 330;
        internal const int G02ThirdMoveUpFrame = 378;
        internal const int G03JoystickPointerId = 401;
        internal const int G03TriggerAcceptFirstPreRollFrame = 36;
        internal const int G03TriggerAcceptLastPreRollFrame = 84;
        internal const int G03PreRollTimeoutFrame = 120;
        internal static readonly Vector2 G03JoystickInput = new(0.05f, 1f);
        internal const float CameraRailMicroShakePositionClamp = 0.035f;
        internal const float CameraRailMicroShakeEulerClamp = 0.18f;
        internal const float CameraRailPositionReadbackTolerance = 0.0001f;
        internal const float CameraRailRotationReadbackToleranceDegrees = 0.01f;
        internal const float CameraRailFovReadbackTolerance = 0.0001f;
        internal const float AuthoredJoystickInputRadiusRatio = 0.62f;

        private readonly struct RailKnot
        {
            public readonly int frame;
            public readonly AuditionPvCityCameraRailPose pose;

            public RailKnot(
                int newFrame,
                Vector3 position,
                Vector3 lookAt,
                float fieldOfView)
            {
                frame = newFrame;
                pose = new AuditionPvCityCameraRailPose(
                    position,
                    lookAt,
                    fieldOfView);
            }
        }

        private static readonly RailKnot[] G01Rail =
        {
            new(0, new Vector3(5.4f, 6.2f, -15.5f), new Vector3(0f, 2.15f, 1.2f), 48f),
            new(239, new Vector3(4.2f, 5.3f, -13.2f), new Vector3(0f, 1.95f, 2f), 46f)
        };

        private static readonly RailKnot[] G02Rail =
        {
            new(0, new Vector3(-0.35f, 2.35f, -10.2f), new Vector3(-0.2f, 1.15f, 1.7f), 52f),
            new(192, new Vector3(1.45f, 2.35f, -6.2f), new Vector3(0.3f, 1.15f, 4.2f), 52f),
            new(240, new Vector3(3f, 2.25f, -4.8f), new Vector3(0.5f, 1.15f, 5.2f), 55f),
            new(419, new Vector3(1.1f, 2.4f, -4.8f), new Vector3(0.3f, 1.15f, 5.4f), 52f)
        };

        private static readonly RailKnot[] G03Rail =
        {
            new(0, new Vector3(2.6f, 2.15f, 1.4f), new Vector3(0f, 2.4f, 10.25f), 43f),
            new(180, new Vector3(1.25f, 2.35f, 4.8f), new Vector3(0f, 2.8f, 10.45f), 38f),
            new(299, new Vector3(0.4f, 2.5f, 6.7f), new Vector3(0f, 3f, 10.55f), 36f)
        };

        internal static AuditionPvCityInputCommand[] CreateG02InputSchedule()
        {
            return new[]
            {
                new AuditionPvCityInputCommand(12, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Down, new Vector2(0.34f, 0.72f)),
                new AuditionPvCityInputCommand(54, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Down),
                new AuditionPvCityInputCommand(55, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(84, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Down),
                new AuditionPvCityInputCommand(85, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(108, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Drag, new Vector2(-0.48f, 0.18f)),
                new AuditionPvCityInputCommand(126, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Down),
                new AuditionPvCityInputCommand(127, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(156, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(192, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Down, new Vector2(0.50f, 0.08f)),
                new AuditionPvCityInputCommand(240, AuditionPvCityInputTarget.Dodge, AuditionPvCityPointerPhase.Down),
                new AuditionPvCityInputCommand(241, AuditionPvCityInputTarget.Dodge, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(242, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(264, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Down),
                new AuditionPvCityInputCommand(324, AuditionPvCityInputTarget.BasicAttack, AuditionPvCityPointerPhase.Up),
                new AuditionPvCityInputCommand(330, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Down, new Vector2(-0.62f, 0.10f)),
                new AuditionPvCityInputCommand(378, AuditionPvCityInputTarget.MoveJoystick, AuditionPvCityPointerPhase.Up)
            };
        }

        internal static AuditionPvShotManifestEntry[] CreateShotManifestEntries()
        {
            return new[]
            {
                new AuditionPvShotManifestEntry
                {
                    id = G01ShotId,
                    scenePath = CityScenePath,
                    startFrame = FirstFrame,
                    endFrame = G01LastFrame,
                    expectedFrameCount = G01ExpectedFrameCount,
                    hudMode = "hud-off",
                    notes = "City establishing source; exact frame-index C1 SmoothStep rail; BL01 f120; authored ActionCamera remains enabled; 2560x1440 PNG at 60fps."
                },
                new AuditionPvShotManifestEntry
                {
                    id = G02ShotId,
                    scenePath = CityScenePath,
                    startFrame = FirstFrame,
                    endFrame = G02LastFrame,
                    expectedFrameCount = G02ExpectedFrameCount,
                    hudMode = "hud-on",
                    notes = "Uncut City product combat; exact real ExecuteEvents pointer schedule f12..f378; natural projectile damage/death/Won only; BL02 f240; exact frame-index C1 SmoothStep rail."
                },
                new AuditionPvShotManifestEntry
                {
                    id = G03ShotId,
                    scenePath = CityScenePath,
                    startFrame = FirstFrame,
                    endFrame = G03LastFrame,
                    expectedFrameCount = G03ExpectedFrameCount,
                    hudMode = "hud-off",
                    notes = "Same-session G02 Won continuation; real HUD joystick id401 at p0 with input (0.05,1.0), accepted trigger p36..p84 without capture-owned movement or transition start, HUD hidden before Recorder warm-up, authored 18/42/234/294 exit transition, and exact frame-index C1 SmoothStep rail."
                }
            };
        }

        internal static AuditionPvBaselineManifestEntry[] CreateBaselineManifestEntries()
        {
            return new[]
            {
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl01", shotId = G01ShotId, sourceFrame = Bl01SourceFrame,
                    fileName = Bl01FileName, hudMode = "hud-off", status = "captured"
                },
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl02", shotId = G02ShotId, sourceFrame = Bl02SourceFrame,
                    fileName = Bl02FileName, hudMode = "hud-on", status = "captured"
                }
            };
        }

        internal static int GetLastFrame(AuditionPvCityShot shot)
        {
            return shot switch
            {
                AuditionPvCityShot.G01 => G01LastFrame,
                AuditionPvCityShot.G02 => G02LastFrame,
                AuditionPvCityShot.G03 => G03LastFrame,
                _ => throw new ArgumentOutOfRangeException(nameof(shot))
            };
        }

        internal static int GetFirstFrame(AuditionPvCityShot shot)
        {
            _ = GetLastFrame(shot);
            return FirstFrame;
        }

        internal static int GetExpectedFrameCount(AuditionPvCityShot shot)
        {
            return GetLastFrame(shot) + 1;
        }

        internal static string GetShotId(AuditionPvCityShot shot)
        {
            return shot switch
            {
                AuditionPvCityShot.G01 => G01ShotId,
                AuditionPvCityShot.G02 => G02ShotId,
                AuditionPvCityShot.G03 => G03ShotId,
                _ => throw new ArgumentOutOfRangeException(nameof(shot))
            };
        }

        internal static string FrameFileName(AuditionPvCityShot shot, int frameIndex)
        {
            ValidateFrameIndex(shot, frameIndex);
            return $"frame_{frameIndex:0000}.png";
        }

        internal static float FrameTimeSeconds(AuditionPvCityShot shot, int frameIndex)
        {
            ValidateFrameIndex(shot, frameIndex);
            return frameIndex / (float)AuditionPvCaptureContract.Fps;
        }

        internal static bool IsG02PlayerFramingSampleFrame(int frameIndex)
        {
            return frameIndex == 0 || frameIndex == 120
                || frameIndex == 240 || frameIndex == 419;
        }

        internal static bool IsG02EnemyFramingSampleFrame(int frameIndex)
        {
            return frameIndex == 0 || frameIndex == 120 || frameIndex == 240;
        }

        internal static AuditionPvCityCameraRailPose EvaluateRail(
            AuditionPvCityShot shot,
            int frameIndex)
        {
            ValidateFrameIndex(shot, frameIndex);
            RailKnot[] knots = shot switch
            {
                AuditionPvCityShot.G01 => G01Rail,
                AuditionPvCityShot.G02 => G02Rail,
                AuditionPvCityShot.G03 => G03Rail,
                _ => throw new ArgumentOutOfRangeException(nameof(shot))
            };

            for (int index = 0; index < knots.Length - 1; index++)
            {
                RailKnot left = knots[index];
                RailKnot right = knots[index + 1];
                if (frameIndex > right.frame)
                {
                    continue;
                }

                float linear = Mathf.InverseLerp(left.frame, right.frame, frameIndex);
                float smooth = linear * linear * (3f - 2f * linear);
                return new AuditionPvCityCameraRailPose(
                    Vector3.LerpUnclamped(left.pose.position, right.pose.position, smooth),
                    Vector3.LerpUnclamped(left.pose.lookAt, right.pose.lookAt, smooth),
                    Mathf.LerpUnclamped(left.pose.fieldOfView, right.pose.fieldOfView, smooth));
            }

            return knots[^1].pose;
        }

        internal static bool PositiveDepthViewportRectIntersects(
            IReadOnlyList<Vector3> viewportCorners)
        {
            if (viewportCorners == null || viewportCorners.Count == 0)
            {
                return false;
            }
            bool hasPositiveDepth = false;
            float minimumX = float.PositiveInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float maximumY = float.NegativeInfinity;
            for (int index = 0; index < viewportCorners.Count; index++)
            {
                Vector3 point = viewportCorners[index];
                if (point.z <= 0f)
                {
                    continue;
                }
                hasPositiveDepth = true;
                minimumX = Mathf.Min(minimumX, point.x);
                minimumY = Mathf.Min(minimumY, point.y);
                maximumX = Mathf.Max(maximumX, point.x);
                maximumY = Mathf.Max(maximumY, point.y);
            }
            return hasPositiveDepth
                && maximumX >= 0f && minimumX <= 1f
                && maximumY >= 0f && minimumY <= 1f;
        }

        internal static bool CameraReadbackMatchesExpectedComposition(
            AuditionPvCityCameraRailPose basePose,
            Vector3 microShakeSourcePosition,
            Vector3 microShakeSourceEuler,
            Vector3 actualPosition,
            Quaternion actualRotation,
            float actualFieldOfView,
            out float positionError,
            out float rotationErrorDegrees,
            out float fieldOfViewError)
        {
            Vector3 lookDirection = basePose.lookAt - basePose.position;
            if (lookDirection.sqrMagnitude <= 0.00000001f)
            {
                positionError = float.MaxValue;
                rotationErrorDegrees = float.MaxValue;
                fieldOfViewError = float.MaxValue;
                return false;
            }

            Quaternion baseRotation = Quaternion.LookRotation(
                lookDirection.normalized,
                Vector3.up);
            Vector3 composedLocalPosition = Vector3.ClampMagnitude(
                microShakeSourcePosition,
                CameraRailMicroShakePositionClamp);
            Vector3 composedEuler = Vector3.ClampMagnitude(
                microShakeSourceEuler,
                CameraRailMicroShakeEulerClamp);
            Vector3 expectedPosition = basePose.position
                + baseRotation * composedLocalPosition;
            Quaternion expectedRotation = baseRotation
                * Quaternion.Euler(composedEuler);

            positionError = Vector3.Distance(actualPosition, expectedPosition);
            rotationErrorDegrees = Quaternion.Angle(actualRotation, expectedRotation);
            fieldOfViewError = Mathf.Abs(actualFieldOfView - basePose.fieldOfView);
            return positionError <= CameraRailPositionReadbackTolerance
                && rotationErrorDegrees <= CameraRailRotationReadbackToleranceDegrees
                && fieldOfViewError <= CameraRailFovReadbackTolerance;
        }

        internal static string[] ExplicitProductDependencyPaths()
        {
            return new[]
            {
                CityScenePath,
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
                HudJoystickPath,
                HudPointerPath,
                "Assets/_Game/UI/CombatHud/CombatHudAimDragInput.cs",
                "Assets/_Game/UI/CombatHud/CombatSessionOverlayPresenter.cs",
                "Assets/_Game/Scripts/Player/PlayerMovementController.cs",
                "Assets/_Game/Scripts/Player/PlayerActionController.cs",
                "Assets/_Game/Scripts/Player/PlayerCombatModeController.cs",
                "Assets/_Game/Scripts/Player/PlayerInputLockSource.cs",
                RangedActionPath,
                "Assets/_Game/Scripts/Player/PlayerCombatTargetSelector.cs",
                "Assets/_Game/Scripts/Enemies/BasicSoldierEnemy.cs",
                "Assets/_Game/Scripts/Enemies/BasicSoldierProjectileAttackDriver.cs",
                "Assets/_Game/Scripts/LevelDesign/CityHeroPocketEnemyProjectileRootBinder.cs",
                EncounterPath,
                ActionCameraPath,
                PresentationClockPath,
                ExitTransitionPath,
                "Assets/_Game/Art/VFX/CombatCues/Prefabs/DB_VFX_PlayerSummonPreSpawnPortal.prefab",
                CaptureScriptPath,
                CaptureTestPath,
                "Assets/_Game/Editor/AuditionPV/AuditionPvCaptureContract.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvRecorderSettingsFactory.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvCaptureManifest.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvEnvironmentProbe.cs",
                "Assets/_Game/Editor/AuditionPV/AuditionPvCityHeroPocketGoldenRunner.cs",
                "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHeroPocketGoldenRunnerTests.cs"
            };
        }

        internal static AuditionPvCityHeroPocketOutput ReserveNewOutput(
            DateTime startedAtUtc,
            AuditionPvGitSnapshot gitSnapshot = null)
        {
            AuditionPvGitSnapshot git = gitSnapshot
                ?? AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "City output reservation requires successful Git provenance: "
                    + git.probeError);
            }

            string outputId = AuditionPvOutputPaths.CreateOutputId(
                "g01-g03-city-hero-pocket",
                startedAtUtc,
                git.commitSha,
                git.isDirty,
                git.dirtyStateHashSha256);
            return ReserveNewOutputForRoot(
                AuditionPvCaptureContract.OutputRoot,
                outputId);
        }

        internal static AuditionPvCityHeroPocketOutput ReserveNewOutputForRoot(
            string outputRoot,
            string outputId)
        {
            string outputDirectory =
                AuditionPvOutputPaths.CreateUniqueOutputDirectory(outputRoot, outputId);
            string baselineDirectory = Path.Combine(
                outputDirectory,
                BaselinesFolderName);
            Directory.CreateDirectory(baselineDirectory);
            var bundles = new Dictionary<AuditionPvCityShot, AuditionPvRecorderSettingsBundle>();
            try
            {
                foreach (AuditionPvCityShot shot in Enum.GetValues(typeof(AuditionPvCityShot)))
                {
                    AuditionPvRecorderSettingsBundle bundle =
                        CreateRecorderSettingsForExistingOutput(outputDirectory, shot);
                    bundles.Add(shot, bundle);
                }

                return new AuditionPvCityHeroPocketOutput(
                    new DirectoryInfo(outputDirectory).Name,
                    outputDirectory,
                    baselineDirectory,
                    bundles);
            }
            catch
            {
                foreach (AuditionPvRecorderSettingsBundle bundle in bundles.Values)
                {
                    bundle.Dispose();
                }
                throw;
            }
        }

        internal static AuditionPvRecorderSettingsBundle
            CreateRecorderSettingsForExistingOutput(
                string outputDirectory,
                AuditionPvCityShot shot)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory)
                || !Path.IsPathRooted(outputDirectory)
                || !Directory.Exists(outputDirectory))
            {
                throw new DirectoryNotFoundException(
                    "A reserved absolute City output directory is required.");
            }

            AuditionPvRecorderSettingsBundle bundle =
                AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence(
                    outputDirectory,
                    GetShotId(shot));
            try
            {
                AuditionPvRecorderSettingsFactory.Validate(bundle);
                return bundle;
            }
            catch
            {
                bundle.Dispose();
                throw;
            }
        }

        internal static AuditionPvCaptureManifest CreateFinalManifest(
            AuditionPvCityHeroPocketOutput output,
            DateTime startedAtUtc,
            IEnumerable<AuditionPvTestResult> testResults,
            AuditionPvGitSnapshot gitSnapshot,
            AuditionPvEngineSnapshot engineSnapshot,
            AuditionPvDependencyHash[] dependencyHashes)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            return AuditionPvCaptureManifestFactory.CreateForRoot(
                output.captureId,
                output.outputRoot,
                output.outputDirectory,
                CreateShotManifestEntries(),
                CreateBaselineManifestEntries(),
                testResults,
                createdAtUtc: startedAtUtc,
                gitSnapshot: gitSnapshot,
                engineSnapshot: engineSnapshot,
                dependencyHashSnapshot: dependencyHashes);
        }

        internal static AuditionPvCaptureManifest CreateFinalManifestForExistingOutput(
            string captureId,
            string outputRoot,
            string outputDirectory,
            DateTime startedAtUtc,
            IEnumerable<AuditionPvTestResult> testResults,
            AuditionPvGitSnapshot gitSnapshot,
            AuditionPvEngineSnapshot engineSnapshot,
            AuditionPvDependencyHash[] dependencyHashes)
        {
            return AuditionPvCaptureManifestFactory.CreateForRoot(
                captureId,
                outputRoot,
                outputDirectory,
                CreateShotManifestEntries(),
                CreateBaselineManifestEntries(),
                testResults,
                createdAtUtc: startedAtUtc,
                gitSnapshot: gitSnapshot,
                engineSnapshot: engineSnapshot,
                dependencyHashSnapshot: dependencyHashes);
        }

        internal static AuditionPvCityHeroPocketDirector AttachToFreshActiveScene(
            AuditionPvCityShot shot)
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "The City shot director can only run in Play Mode.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded
                || !string.Equals(scene.path, CityScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "City capture requires a fresh CityHeroPocketStage PlayMode scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<AuditionPvCityHeroPocketDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The active scene already owns a City shot director.");
            }
            if (shot != AuditionPvCityShot.G01)
            {
                throw new InvalidOperationException(
                    "The one-scene City sequence must attach at G01.");
            }

            var root = new GameObject($"[AuditionPV_{GetShotId(shot).ToUpperInvariant()}_Director]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            AuditionPvCityHeroPocketDirector director =
                root.AddComponent<AuditionPvCityHeroPocketDirector>();
            AuditionPvCityHeroPocketCameraRail rail =
                root.AddComponent<AuditionPvCityHeroPocketCameraRail>();
            director.Configure(shot, rail);
            rail.Configure(director);
            return director;
        }

        internal static void ReopenProductSceneAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before reopening the City product scene.");
            }
            EditorSceneManager.OpenScene(CityScenePath, OpenSceneMode.Single);
        }

        internal static void ValidateRuntimeProof(
            AuditionPvCityHeroPocketRuntimeProof proof)
        {
            if (proof == null)
            {
                throw new ArgumentNullException(nameof(proof));
            }

            bool common = proof.directorCompleted
                && proof.lastLogicalFrame == proof.expectedFrameCount - 1
                && proof.presentedFrameCount == proof.expectedFrameCount
                && proof.presentedFramesExact
                && proof.presentationClockExact
                && proof.hudModeExact
                && proof.actionCameraStayedEnabled
                && proof.cameraRailAppliedFrameCount == proof.expectedFrameCount
                && proof.cameraRailBasePoseExact
                && proof.cameraRailFovExact
                && proof.cameraRailActualReadbackFrameCount
                    == proof.expectedFrameCount
                && proof.cameraRailActualComposedPoseExact
                && proof.maximumCameraPositionReadbackError >= 0f
                && proof.maximumCameraPositionReadbackError
                    <= CameraRailPositionReadbackTolerance
                && proof.maximumCameraRotationReadbackErrorDegrees >= 0f
                && proof.maximumCameraRotationReadbackErrorDegrees
                    <= CameraRailRotationReadbackToleranceDegrees
                && proof.maximumCameraFovReadbackError >= 0f
                && proof.maximumCameraFovReadbackError
                    <= CameraRailFovReadbackTolerance
                && proof.deterministicRandomSeed == DeterministicRandomSeed
                && proof.microShakeWithinClamp
                && proof.stateRestored
                && proof.presentationClockReleased
                && proof.pointerLeasesReleased
                && proof.hudStateRestored
                && proof.cameraStateRestored
                && proof.actionCameraStateRestored
                && proof.actionCameraTransientStateRestored
                && proof.cameraRailReleased
                && proof.transitionStateRestored;
            if (!common)
            {
                throw new InvalidOperationException(
                    "City runtime proof is missing exact frame, rail, HUD, clock, or restoration evidence.");
            }

            switch (proof.shotId)
            {
                case G01ShotId:
                    if (proof.expectedFrameCount != G01ExpectedFrameCount
                        || proof.pointerDownCount != 0 || proof.pointerDragCount != 0
                        || proof.pointerUpCount != 0 || proof.enemyDiedCount != 0
                        || proof.encounterWonCount != 0
                        || proof.rangedProjectileFiredCount != 0
                        || proof.enemyDamagedCount != 0
                        || proof.g01PlayerDrift > 0.01f
                        || proof.g01EnemyDrift > 0.01f
                        || Mathf.Abs(proof.playerHealthAtShotStart
                            - proof.playerHealthAtShotEnd) > 0.001f
                        || Mathf.Abs(proof.enemyHealthAtShotStart
                            - proof.enemyHealthAtShotEnd) > 0.001f
                        || proof.ammoAtShotStart != proof.ammoAtShotEnd
                        || proof.playerProjectileCountAtShotStart
                            != proof.playerProjectileCountAtShotEnd
                        || proof.enemyFiredCountAtShotStart
                            != proof.enemyFiredCountAtShotEnd
                        || !proof.g01GameplaySuspensionExact
                        || !proof.g01HudRootStayedActive
                        || proof.g01CompositionSampleCount != 3
                        || proof.g01CompositionPassingSampleCount < 1
                        || !proof.g01ForegroundDepthObserved
                        || !proof.g01MidgroundActorsObserved
                        || !proof.g01BackgroundDepthObserved
                        || !proof.g01PlayerEnemyLineOfSightClear
                        || !proof.g01ThreeDepthCompositionObserved
                        || !proof.productOutcomePreservedForContinuation)
                    {
                        throw new InvalidOperationException(
                            "G01 proof is missing exact capture locks, HUD-off activity, or unchanged actor/health/ammo/projectile evidence.");
                    }
                    break;
                case G02ShotId:
                    if (proof.expectedFrameCount != G02ExpectedFrameCount
                        || !proof.g02PointerScheduleExact
                        || proof.pointerDownCount != 8
                        || proof.pointerDragCount != 1
                        || proof.pointerUpCount != 8
                        || proof.rangedProjectileFiredCount < 10
                        || proof.ammoAtShotStart - proof.ammoAtShotEnd
                            != proof.rangedProjectileFiredCount
                        || proof.g02ReloadStartedCount != 0
                        || proof.g02PlayerPathLength < 6f
                        || proof.g02PlayerNetDisplacement < 2f
                        || proof.g02MaximumFrameDisplacement
                            > 0.751f
                        || proof.g02DodgeDownFrame != G02DodgeDownFrame
                        || proof.g02DodgeStartedCount != 1
                        || proof.g02DodgeEndedCount != 1
                        || proof.g02DodgeDirectionRailRightDot
                            <= 0.55f
                        || !proof.g02PlayerAliveAtEnd
                        || !proof.g02PlayerStayedInBounds
                        || proof.g02EnemyHealthAtEnd > 0.001f
                        || proof.enemyDiedCount != 1
                        || proof.encounterWonCount != 1
                        || !proof.naturalEnemyDeathObserved
                        || !proof.naturalWonObserved
                        || !proof.g02EnemyTelegraphObserved
                        || proof.g02EnemyTelegraphVisibleFrameCount <= 0
                        || proof.g02EnemyFiredDelta < 1
                        || !proof.g02ProjectileRootsIndependentAndSceneOwned
                        || proof.g02PlayerProjectileVisibleFrameCount <= 0
                        || proof.g02EnemyProjectileVisibleFrameCount <= 0
                        || proof.g02PlayerFramingSampleCount != 4
                        || proof.g02PlayerFramingPassCount != 4
                        || proof.g02EnemyFramingSampleCount != 3
                        || proof.g02EnemyFramingPassCount != 3
                        || proof.g02RifleFeedbackRequestDelta <= 0
                        || proof.g02MicroShakeRequestDelta <= 0
                        || proof.microShakeSourceFrameCount <= 0
                        || proof.microShakeComposedFrameCount < 5
                        || proof.enemyDamagedCount <= 0
                        || !proof.g02EndedOutsideExitTrigger
                        || !proof.g02EndedSouthOfExitTrigger
                        || !proof.productOutcomePreservedForContinuation)
                    {
                        throw new InvalidOperationException(
                            "G02 proof is missing exact hardware input, movement/dodge, projectile/telegraph, camera feedback, natural death/Won, or G03 handoff evidence.");
                    }
                    break;
                case G03ShotId:
                    if (proof.expectedFrameCount != G03ExpectedFrameCount
                        || !proof.continuityFromPreviousShot
                        || proof.encounterInstanceId <= 0
                        || proof.playerInstanceId <= 0
                        || proof.enemyInstanceId <= 0
                        || !proof.g03StartedAlreadyWon
                        || !proof.g03StartedTransitionArmed
                        || !proof.g03StartedOutsideExitTrigger
                        || !proof.g03StartedSouthOfExitTrigger
                        || !proof.naturalEnemyDeathObserved
                        || !proof.naturalWonObserved
                        || proof.g03NewDamageEventCount != 0
                        || proof.g03NewDeathEventCount != 0
                        || proof.g03NewWonEventCount != 0
                        || proof.enemyDamagedCount != 0
                        || proof.enemyDiedCount != 0
                        || proof.encounterWonCount != 0
                        || proof.g03JoystickPointerId != G03JoystickPointerId
                        || Vector2.Distance(proof.g03JoystickInput, G03JoystickInput)
                            > 0.0001f
                        || !proof.g03JoystickInputExact
                        || !proof.g03JoystickInputMaintainedUntilTrigger
                        || !proof.g03JoystickInputExactAtTrigger
                        || proof.g03TriggerAcceptedPreRollFrame
                            < G03TriggerAcceptFirstPreRollFrame
                        || proof.g03TriggerAcceptedPreRollFrame
                            > G03TriggerAcceptLastPreRollFrame
                        || proof.g03PreRollPathLength <= 0f
                        || proof.g03PreRollNetDisplacement <= 0f
                        || proof.transitionRejectedTriggerEnterBaseline != 0
                        || proof.transitionRejectedTriggerEnterEnd != 0
                        || proof.transitionRejectedTriggerEnterDelta != 0
                        || proof.captureTransitionStartCallCount != 0
                        || proof.captureHudMutationCount != 0
                        || proof.transitionTriggerAcceptedCount != 1
                        || proof.transitionStartedCount != 1
                        || proof.transitionHudHiddenCount != 1
                        || proof.transitionFullCoverCount != 1
                        || proof.transitionExitReadyCount != 1
                        || !proof.hudHiddenBeforeLogicalFrameZero
                        || proof.transitionFrameBeforeLogicalFrameZero < 18
                        || proof.transitionFrameBeforeLogicalFrameZero > 24
                        || proof.transitionFrameAtLogicalFrameZero < 19
                        || proof.transitionFrameAtLogicalFrameZero > 25
                        || !proof.transitionAdvancedOnceBeforeRailFrameZero
                        || proof.transitionPortalAuthoredLogicalFrame < 0
                        || proof.transitionPortalAuthoredLogicalFrame
                            + proof.transitionFrameAtLogicalFrameZero != 42
                        || proof.transitionCoverStartedLogicalFrame < 0
                        || proof.transitionCoverStartedLogicalFrame
                            + proof.transitionFrameAtLogicalFrameZero != 234
                        || proof.transitionFullCoverLogicalFrame < 0
                        || proof.transitionFullCoverLogicalFrame > 276
                        || proof.transitionFullCoverLogicalFrame
                            + proof.transitionFrameAtLogicalFrameZero != 294
                        || proof.cleanCoverFrameCount < 24
                        || !proof.transitionPortalReachedAuthoredScale
                        || !proof.transitionCoverReachedFull
                        || !proof.transitionInputAndAiLocked)
                    {
                        throw new InvalidOperationException(
                            "G03 proof is missing same-session Won identity, real joystick trigger truth, zero new gameplay events, or offset-based product transition evidence.");
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown City runtime proof shot '{proof.shotId}'.");
            }
        }

        internal static AuditionPvCityHeroPocketRuntimeProof DeepCopyRuntimeProof(
            AuditionPvCityHeroPocketRuntimeProof source)
        {
            return source == null
                ? null
                : JsonUtility.FromJson<AuditionPvCityHeroPocketRuntimeProof>(
                    JsonUtility.ToJson(source));
        }

        private static void ValidateFrameIndex(
            AuditionPvCityShot shot,
            int frameIndex)
        {
            if (frameIndex < FirstFrame || frameIndex > GetLastFrame(shot))
            {
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            }
        }
    }

    internal sealed class AuditionPvCityHeroPocketOutput : IDisposable
    {
        private readonly Dictionary<AuditionPvCityShot, AuditionPvRecorderSettingsBundle>
            recorderSettings;

        public readonly string captureId;
        public readonly string outputRoot;
        public readonly string outputDirectory;
        public readonly string baselineDirectory;

        internal AuditionPvCityHeroPocketOutput(
            string newCaptureId,
            string newOutputDirectory,
            string newBaselineDirectory,
            Dictionary<AuditionPvCityShot, AuditionPvRecorderSettingsBundle> newRecorderSettings)
        {
            captureId = newCaptureId;
            outputDirectory = Path.GetFullPath(newOutputDirectory)
                .Replace('\\', '/').TrimEnd('/');
            outputRoot = Path.GetDirectoryName(outputDirectory)
                ?.Replace('\\', '/').TrimEnd('/')
                ?? throw new ArgumentException("Output needs a parent root.");
            baselineDirectory = Path.GetFullPath(newBaselineDirectory)
                .Replace('\\', '/').TrimEnd('/');
            recorderSettings = newRecorderSettings
                ?? throw new ArgumentNullException(nameof(newRecorderSettings));
        }

        public AuditionPvRecorderSettingsBundle GetRecorderSettings(
            AuditionPvCityShot shot)
        {
            return recorderSettings.TryGetValue(shot, out AuditionPvRecorderSettingsBundle value)
                ? value
                : throw new ArgumentOutOfRangeException(nameof(shot));
        }

        public void Dispose()
        {
            foreach (AuditionPvRecorderSettingsBundle bundle in recorderSettings.Values)
            {
                bundle.Dispose();
            }
            recorderSettings.Clear();
        }
    }

    [Serializable]
    public sealed class AuditionPvCityHeroPocketRuntimeProof
    {
        public string schema = "dimension-brawl.audition-pv.city-runtime-proof.v1";
        public string shotId = string.Empty;
        public bool directorCompleted;
        public int lastLogicalFrame = -1;
        public int expectedFrameCount;
        public int presentedFrameCount;
        public bool presentedFramesExact = true;
        public bool presentationClockExact = true;
        public bool hudModeExact = true;
        public bool actionCameraStayedEnabled = true;
        public int cameraRailAppliedFrameCount;
        public bool cameraRailBasePoseExact = true;
        public bool cameraRailFovExact = true;
        public int cameraRailActualReadbackFrameCount;
        public bool cameraRailActualComposedPoseExact = true;
        public float maximumCameraPositionReadbackError;
        public float maximumCameraRotationReadbackErrorDegrees;
        public float maximumCameraFovReadbackError;
        public int deterministicRandomSeed;
        public int microShakeSourceFrameCount;
        public int microShakeComposedFrameCount;
        public float maximumComposedMicroShakePosition;
        public float maximumComposedMicroShakeEuler;
        public bool microShakeWithinClamp = true;
        public int pointerDownCount;
        public int pointerDragCount;
        public int pointerUpCount;
        public bool g02PointerScheduleExact;
        public int rangedProjectileFiredCount;
        public int enemyDamagedCount;
        public int enemyDiedCount;
        public int encounterWonCount;
        public bool naturalEnemyDeathObserved;
        public bool naturalWonObserved;
        public int transitionTriggerAcceptedCount;
        public int transitionRejectedTriggerEnterBaseline;
        public int transitionRejectedTriggerEnterEnd;
        public int transitionRejectedTriggerEnterDelta;
        public int transitionStartedCount;
        public int transitionHudHiddenCount;
        public int transitionFullCoverCount;
        public int transitionExitReadyCount;
        public bool transitionInputAndAiLocked = true;
        public bool stateRestored;
        public bool presentationClockReleased;
        public bool pointerLeasesReleased;
        public bool hudStateRestored;
        public bool cameraStateRestored;
        public bool actionCameraStateRestored;
        public bool transitionStateRestored;
        public int encounterInstanceId;
        public int playerInstanceId;
        public int enemyInstanceId;
        public bool continuityFromPreviousShot;
        public bool g02EndedOutsideExitTrigger;
        public bool g02EndedSouthOfExitTrigger;
        public bool g03StartedAlreadyWon;
        public bool g03StartedTransitionArmed;
        public bool g03StartedOutsideExitTrigger;
        public bool g03StartedSouthOfExitTrigger;
        public int g03NewDamageEventCount;
        public int g03NewDeathEventCount;
        public int g03NewWonEventCount;
        public int g03JoystickPointerId = -1;
        public Vector2 g03JoystickInput;
        public bool g03JoystickInputExact;
        public bool g03JoystickInputMaintainedUntilTrigger = true;
        public bool g03JoystickInputExactAtTrigger;
        public int g03TriggerAcceptedPreRollFrame = -1;
        public float g03PreRollPathLength;
        public float g03PreRollNetDisplacement;
        public int captureTransitionStartCallCount;
        public int captureHudMutationCount;
        public bool hudHiddenBeforeLogicalFrameZero;
        public int transitionFrameBeforeLogicalFrameZero = -1;
        public int transitionFrameAtLogicalFrameZero = -1;
        public bool transitionAdvancedOnceBeforeRailFrameZero;
        public int transitionPortalAuthoredLogicalFrame = -1;
        public int transitionCoverStartedLogicalFrame = -1;
        public int transitionFullCoverLogicalFrame = -1;
        public int cleanCoverFrameCount;
        public float g01PlayerDrift;
        public float g01EnemyDrift;
        public float playerHealthAtShotStart;
        public float playerHealthAtShotEnd;
        public float enemyHealthAtShotStart;
        public float enemyHealthAtShotEnd;
        public int ammoAtShotStart;
        public int ammoAtShotEnd;
        public int playerProjectileCountAtShotStart;
        public int playerProjectileCountAtShotEnd;
        public int enemyFiredCountAtShotStart;
        public int enemyFiredCountAtShotEnd;
        public bool g01GameplaySuspensionExact;
        public bool g01HudRootStayedActive = true;
        public int g01CompositionSampleCount;
        public int g01CompositionPassingSampleCount;
        public bool g01ForegroundDepthObserved;
        public bool g01MidgroundActorsObserved;
        public bool g01BackgroundDepthObserved;
        public bool g01PlayerEnemyLineOfSightClear;
        public bool g01ThreeDepthCompositionObserved;
        public float g02PlayerPathLength;
        public float g02PlayerNetDisplacement;
        public float g02MaximumFrameDisplacement;
        public int g02DodgeStartedCount;
        public int g02DodgeEndedCount;
        public int g02DodgeDownFrame = -1;
        public float g02DodgeDirectionRailRightDot;
        public bool g02PlayerStayedInBounds;
        public bool g02EnemyTelegraphObserved;
        public int g02EnemyTelegraphVisibleFrameCount;
        public int g02EnemyFiredDelta;
        public bool g02ProjectileRootsIndependentAndSceneOwned;
        public int g02PlayerProjectileVisibleFrameCount;
        public int g02EnemyProjectileVisibleFrameCount;
        public int g02PlayerFramingSampleCount;
        public int g02PlayerFramingPassCount;
        public int g02EnemyFramingSampleCount;
        public int g02EnemyFramingPassCount;
        public int g02RifleFeedbackRequestDelta;
        public int g02MicroShakeRequestDelta;
        public int g02ReloadStartedCount;
        public bool g02PlayerAliveAtEnd;
        public float g02EnemyHealthAtEnd;
        public int transitionPresentationFrameAtEnd = -1;
        public bool transitionPortalReachedAuthoredScale;
        public bool transitionCoverReachedFull;
        public bool productOutcomePreservedForContinuation;
        public bool actionCameraTransientStateRestored;
        public bool cameraRailReleased;
    }

    /// <summary>
    /// Early deterministic input/product-state owner. It never calls a health
    /// mutation API and never disables the authored ActionCamera.
    /// </summary>
    [DefaultExecutionOrder(-32500)]
    public sealed class AuditionPvCityHeroPocketDirector : MonoBehaviour
    {
        private const float FloatTolerance = 0.001f;
        private const float G01MaximumDrift = 0.01f;
        private const float G02MaximumFrameDisplacement = 0.75f;
        private const float G02MinimumPathLength = 6f;
        private const float G02MinimumNetDisplacement = 2f;
        private const float G02MinimumDodgeRailRightDot = 0.55f;

        private readonly List<AuditionPvCityInputCommand> executedG02Commands = new(20);
        private AuditionPvCityShot shot;
        private AuditionPvCityHeroPocketCameraRail rail;
        private CombatEncounterController encounter;
        private CombatHealth playerHealth;
        private CombatHealth enemyHealth;
        private PlayerMovementController playerMovement;
        private PlayerActionController playerAction;
        private PlayerCombatModeController playerCombatMode;
        private PlayerRangedBasicAttackAction rangedAction;
        private BasicSoldierEnemy enemySoldier;
        private BasicSoldierProjectileAttackDriver enemyProjectileDriver;
        private EnemyAttackTelegraphPresenter enemyTelegraphPresenter;
        private ActionCameraController actionCamera;
        private Camera gameplayCamera;
        private CombatHudVirtualJoystick moveJoystick;
        private CombatHudAimDragInput aimDrag;
        private OneRowCombatHudBinder hudBinder;
        private CanvasGroup hud;
        private CityHeroPocketExitTransitionController exitTransition;
        private CharacterController characterController;
        private RectTransform attackButton;
        private RectTransform dodgeButton;
        private Transform g01Truck;
        private Transform g01Bicycle;
        private Transform g01SignalLeft;
        private Transform g01SignalRight;
        private Transform g01WireCross;
        private Transform g01BackgroundEndLeft;
        private Transform g01BackgroundEndRight;
        private EventSystem eventSystem;
        private PresentationClock.ManualLease presentationClockLease;
        private PointerLease movePointer;
        private PointerLease attackPointer;
        private PointerLease dodgePointer;
        private PointerLease g03MovePointer;
        private AuditionPvCityHeroPocketRuntimeProof proof;
        private AuditionPvCityHeroPocketRuntimeProof lastSealedRuntimeProof;
        private UnityEngine.Random.State savedRandomState;
        private bool savedRandomStateValid;
        private int savedCaptureFramerate;
        private int savedTargetFrameRate;
        private float savedHudAlpha;
        private bool savedHudInteractable;
        private bool savedHudBlocksRaycasts;
        private Vector3 savedCameraPosition;
        private Quaternion savedCameraRotation;
        private float savedCameraFieldOfView;
        private float savedCameraNearClipPlane;
        private bool savedActionCameraEnabled;
        private Transform savedActionCameraTarget;
        private Transform savedActionCameraThreat;
        private bool restorableStateCaptured;
        private bool restoring;
        private bool currentShotRestored;
        private bool finalRestoreCompleted;
        private bool continuationInProgress;
        private bool g01SuspensionOwned;
        private int currentFrame = -1;
        private int nextExpectedPresentedFrame;
        private int nextMovePointerId = 5101;
        private int nextAttackPointerId = 5201;
        private int nextDodgePointerId = 5301;
        private int sequenceEncounterInstanceId;
        private int sequencePlayerInstanceId;
        private int sequenceEnemyInstanceId;
        private int g02EncounterInstanceId;
        private int g02PlayerInstanceId;
        private int g02EnemyInstanceId;
        private Vector3 shotPlayerStartPosition;
        private Vector3 shotEnemyStartPosition;
        private Vector3 lastObservedPlayerPosition;
        private int actionCameraRifleFeedbackAtShotStart;
        private int actionCameraMicroShakeAtShotStart;
        private int activeG03PreRollFrame = -1;
        private Vector3 g03PreRollStartPosition;
        private Vector3 g03PreRollLastPosition;
        private Vector3 g03AcceptedPosition;

        public event Action<int> FramePresented;

        public AuditionPvCityShot Shot => shot;
        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public Exception Failure { get; private set; }
        public int CurrentFrame => currentFrame;
        public AuditionPvCityHeroPocketRuntimeProof RuntimeProof => proof;
        public AuditionPvCityHeroPocketRuntimeProof LastSealedRuntimeProof =>
            AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(
                lastSealedRuntimeProof);
        public bool StateRestored => currentShotRestored;

        internal void Configure(
            AuditionPvCityShot newShot,
            AuditionPvCityHeroPocketCameraRail newRail)
        {
            if (IsPrepared || IsRunning || proof != null)
            {
                throw new InvalidOperationException("A running City director cannot be reconfigured.");
            }
            if (newShot != AuditionPvCityShot.G01)
            {
                throw new InvalidOperationException(
                    "The same-session City sequence must attach at G01.");
            }
            shot = newShot;
            rail = newRail ?? throw new ArgumentNullException(nameof(newRail));
            ResetShotProof(newShot);
        }

        public IEnumerator PrepareFreshProductState()
        {
            if (rail == null || proof == null || shot != AuditionPvCityShot.G01
                || IsPrepared || IsRunning || currentShotRestored
                || finalRestoreCompleted || continuationInProgress)
            {
                throw new InvalidOperationException(
                    "Attach the same-session City director at G01 and prepare it exactly once.");
            }

            ValidateFreshScene();
            ResolveBindings();
            CaptureRestorableState();
            sequenceEncounterInstanceId = encounter.GetInstanceID();
            sequencePlayerInstanceId = playerHealth.GetInstanceID();
            sequenceEnemyInstanceId = enemyHealth.GetInstanceID();
            IEnumerator preparation = PrepareCurrentProductState(isContinuation: false);
            while (preparation.MoveNext())
            {
                yield return preparation.Current;
            }
        }

        public IEnumerator PrepareContinuationShot(AuditionPvCityShot nextShot)
        {
            if (continuationInProgress || finalRestoreCompleted || currentShotRestored
                || !IsComplete || IsRunning || !IsPrepared)
            {
                throw new InvalidOperationException(
                    "A City continuation requires one completed, un-restored current shot.");
            }
            AuditionPvCityShot expected = shot switch
            {
                AuditionPvCityShot.G01 => AuditionPvCityShot.G02,
                AuditionPvCityShot.G02 => AuditionPvCityShot.G03,
                _ => throw new InvalidOperationException("G03 has no continuation shot.")
            };
            if (nextShot != expected)
            {
                throw new InvalidOperationException(
                    $"Illegal City continuation order: {shot} -> {nextShot}; expected {expected}.");
            }

            continuationInProgress = true;
            bool prepared = false;
            try
            {
                proof.productOutcomePreservedForContinuation = shot switch
                {
                    AuditionPvCityShot.G01 => encounter.IsRunning
                        && playerHealth.IsAlive && enemyHealth.IsAlive,
                    AuditionPvCityShot.G02 => encounter.IsWon
                        && !enemyHealth.IsAlive && exitTransition.IsArmed
                        && !exitTransition.IsTransitionRunning,
                    _ => false
                };
                CleanupCurrentShot(finalCleanup: false);
                lastSealedRuntimeProof =
                    AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(proof);

                ResetShotProof(nextShot);
                IEnumerator preparation = PrepareCurrentProductState(isContinuation: true);
                while (preparation.MoveNext())
                {
                    yield return preparation.Current;
                }
                prepared = true;
            }
            finally
            {
                continuationInProgress = false;
                if (!prepared && !currentShotRestored)
                {
                    RestoreShotState();
                }
            }
        }

        public AuditionPvCityHeroPocketRuntimeProof SnapshotRuntimeProof()
        {
            return AuditionPvCityHeroPocketCapture.DeepCopyRuntimeProof(proof);
        }

        private IEnumerator PrepareCurrentProductState(bool isContinuation)
        {
            bool prepared = false;
            try
            {
                ValidateProductStateForShot(isContinuation);
                if (!savedRandomStateValid)
                {
                    savedRandomState = UnityEngine.Random.state;
                    savedRandomStateValid = true;
                }
                UnityEngine.Random.InitState(
                    AuditionPvCityHeroPocketCapture.DeterministicRandomSeed);
                proof.deterministicRandomSeed =
                    AuditionPvCityHeroPocketCapture.DeterministicRandomSeed;
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
                presentationClockLease = PresentationClock.AcquireManual(
                    this,
                    AuditionPvCaptureContract.Fps);
                SubscribeProductEvidence();
                CaptureShotStartEvidence();
                Canvas.ForceUpdateCanvases();

                if (shot == AuditionPvCityShot.G01)
                {
                    AcquireG01Suspension();
                    SetHudModeForCapture();
                }
                else if (shot == AuditionPvCityShot.G02)
                {
                    SetHudModeForCapture();
                }
                else
                {
                    IEnumerator preRoll = PrepareG03TransitionPreRoll();
                    while (preRoll.MoveNext())
                    {
                        yield return preRoll.Current;
                    }
                    if (!IsHudModeExact())
                    {
                        throw new InvalidOperationException(
                            "G03 Recorder preparation began before the product HUD-hidden state.");
                    }
                }

                presentationClockLease.SetFrame(0);
                rail.Prepare(gameplayCamera, actionCamera);
                IsPrepared = true;
                prepared = true;
            }
            finally
            {
                if (!prepared && !currentShotRestored)
                {
                    RestoreShotState();
                }
            }
        }

        public void BeginShot()
        {
            BeginShotCore(recorderOwnsCadence: false);
        }

        public void BeginShotForRecorder()
        {
            BeginShotCore(recorderOwnsCadence: true);
        }

        private void BeginShotCore(bool recorderOwnsCadence)
        {
            if (!IsPrepared || IsRunning || IsComplete || currentShotRestored
                || finalRestoreCompleted || continuationInProgress)
            {
                throw new InvalidOperationException(
                    "Prepare the City product state exactly once before capture.");
            }
            if (Time.timeScale <= 0f)
            {
                throw new InvalidOperationException("City capture cannot run at timeScale zero.");
            }
            if (recorderOwnsCadence)
            {
                float minimum = 1f / AuditionPvCaptureContract.Fps;
                if (Time.captureDeltaTime < minimum
                    || Time.captureDeltaTime >= minimum + 0.001f)
                {
                    throw new InvalidOperationException(
                        $"Recorder cadence is not active: {Time.captureDeltaTime:F9}.");
                }
            }
            else
            {
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
            }

            currentFrame = 0;
            nextExpectedPresentedFrame = 0;
            presentationClockLease.SetFrame(0);
            if (shot == AuditionPvCityShot.G03)
            {
                proof.hudHiddenBeforeLogicalFrameZero = exitTransition.IsHudHidden
                    && exitTransition.HudHiddenCount == 1
                    && IsHudModeExact();
                proof.transitionFrameBeforeLogicalFrameZero =
                    exitTransition.PresentationFrame;
                if (!proof.hudHiddenBeforeLogicalFrameZero
                    || proof.transitionFrameBeforeLogicalFrameZero < 18)
                {
                    throw new InvalidOperationException(
                        "G03 logical f0 must begin after the product HUD-hidden milestone.");
                }
            }
            IsRunning = true;
        }

        private void Update()
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }
            try
            {
                if (!actionCamera.isActiveAndEnabled)
                {
                    proof.actionCameraStayedEnabled = false;
                    throw new InvalidOperationException(
                        "The authored ActionCamera was disabled during City capture.");
                }
                presentationClockLease.SetFrame(currentFrame);
                if (shot == AuditionPvCityShot.G02)
                {
                    ExecuteG02InputFrame(currentFrame);
                }
                if (encounter.IsFailed || encounter.IsFaulted)
                {
                    throw new InvalidOperationException(
                        "The City product encounter failed during golden capture.");
                }
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        internal void ExecuteLatePreRailAction(int frameIndex)
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }
            EnforceHudModeForRenderedFrame();
        }

        internal void CompleteFrameFromRail(int frameIndex)
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }
            try
            {
                proof.presentedFramesExact &= frameIndex == nextExpectedPresentedFrame
                    && frameIndex == currentFrame;
                proof.presentationClockExact &= PresentationClock.IsManuallyDriven
                    && Mathf.Abs(PresentationClock.UnscaledTime
                        - frameIndex / (float)AuditionPvCaptureContract.Fps) <= 0.00001f
                    && Mathf.Abs(PresentationClock.UnscaledDeltaTime
                        - 1f / AuditionPvCaptureContract.Fps) <= 0.00001f;
                proof.hudModeExact &= IsHudModeExact();
                proof.actionCameraStayedEnabled &= actionCamera.isActiveAndEnabled;
                proof.cameraRailAppliedFrameCount++;
                AuditionPvCityCameraRailPose expectedBasePose =
                    AuditionPvCityHeroPocketCapture.EvaluateRail(shot, frameIndex);
                proof.cameraRailBasePoseExact &= rail.LastBaseFrame == frameIndex
                    && PoseApproximately(rail.LastBasePose, expectedBasePose);
                Vector3 microShakeSourcePosition =
                    actionCamera.LastMicroShakeLocalOffset;
                Vector3 microShakeSourceEuler =
                    actionCamera.LastMicroShakeEulerOffset;
                bool actualReadbackMatches = AuditionPvCityHeroPocketCapture
                    .CameraReadbackMatchesExpectedComposition(
                        expectedBasePose,
                        microShakeSourcePosition,
                        microShakeSourceEuler,
                        gameplayCamera.transform.position,
                        gameplayCamera.transform.rotation,
                        gameplayCamera.fieldOfView,
                        out float positionReadbackError,
                        out float rotationReadbackErrorDegrees,
                        out float fovReadbackError);
                proof.cameraRailActualReadbackFrameCount++;
                proof.cameraRailActualComposedPoseExact &= actualReadbackMatches;
                proof.maximumCameraPositionReadbackError = Mathf.Max(
                    proof.maximumCameraPositionReadbackError,
                    positionReadbackError);
                proof.maximumCameraRotationReadbackErrorDegrees = Mathf.Max(
                    proof.maximumCameraRotationReadbackErrorDegrees,
                    rotationReadbackErrorDegrees);
                proof.maximumCameraFovReadbackError = Mathf.Max(
                    proof.maximumCameraFovReadbackError,
                    fovReadbackError);
                proof.cameraRailFovExact &= fovReadbackError
                    <= AuditionPvCityHeroPocketCapture.CameraRailFovReadbackTolerance;
                Vector3 composedPosition = Vector3.ClampMagnitude(
                    microShakeSourcePosition,
                    AuditionPvCityHeroPocketCapture.CameraRailMicroShakePositionClamp);
                Vector3 composedEuler = Vector3.ClampMagnitude(
                    microShakeSourceEuler,
                    AuditionPvCityHeroPocketCapture.CameraRailMicroShakeEulerClamp);
                float composedPositionMagnitude = composedPosition.magnitude;
                float composedEulerMagnitude = composedEuler.magnitude;
                bool hadMicroShakeSource =
                    microShakeSourcePosition.sqrMagnitude > 0.00000001f
                    || microShakeSourceEuler.sqrMagnitude > 0.00000001f;
                if (hadMicroShakeSource)
                {
                    proof.microShakeSourceFrameCount++;
                }
                if (composedPositionMagnitude > 0.000001f
                    || composedEulerMagnitude > 0.000001f)
                {
                    proof.microShakeComposedFrameCount++;
                }
                proof.maximumComposedMicroShakePosition = Mathf.Max(
                    proof.maximumComposedMicroShakePosition,
                    composedPositionMagnitude);
                proof.maximumComposedMicroShakeEuler = Mathf.Max(
                    proof.maximumComposedMicroShakeEuler,
                    composedEulerMagnitude);
                proof.microShakeWithinClamp &= composedPositionMagnitude
                        <= AuditionPvCityHeroPocketCapture.CameraRailMicroShakePositionClamp
                            + FloatTolerance
                    && composedEulerMagnitude
                        <= AuditionPvCityHeroPocketCapture.CameraRailMicroShakeEulerClamp
                            + FloatTolerance;
                ObservePerFrameProductEvidence(frameIndex);

                proof.presentedFrameCount++;
                proof.lastLogicalFrame = frameIndex;
                FramePresented?.Invoke(frameIndex);
                nextExpectedPresentedFrame++;

                if (frameIndex == AuditionPvCityHeroPocketCapture.GetLastFrame(shot))
                {
                    ValidateCompletedProductShot();
                    proof.directorCompleted = true;
                    IsRunning = false;
                    IsComplete = true;
                    return;
                }
                currentFrame++;
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        public void RestoreShotState()
        {
            if (currentShotRestored || restoring)
            {
                return;
            }
            CleanupCurrentShot(finalCleanup: true);
            finalRestoreCompleted = true;
        }

        private void ResolveBindings()
        {
            Scene scene = SceneManager.GetActiveScene();
            encounter = RequireSingleInScene<CombatEncounterController>(scene);
            playerHealth = encounter.PlayerHealth
                ?? throw new InvalidOperationException("City encounter has no player health.");
            enemyHealth = encounter.EnemyHealth
                ?? throw new InvalidOperationException("City encounter has no enemy health.");
            playerMovement = RequireSingleInScene<PlayerMovementController>(scene);
            playerAction = RequireSingleInScene<PlayerActionController>(scene);
            playerCombatMode = RequireSingleInScene<PlayerCombatModeController>(scene);
            rangedAction = RequireSingleInScene<PlayerRangedBasicAttackAction>(scene);
            enemySoldier = RequireSingleInScene<BasicSoldierEnemy>(scene);
            enemyProjectileDriver =
                RequireSingleInScene<BasicSoldierProjectileAttackDriver>(scene);
            enemyTelegraphPresenter =
                RequireSingleInScene<EnemyAttackTelegraphPresenter>(scene);
            actionCamera = RequireSingleInScene<ActionCameraController>(scene);
            gameplayCamera = actionCamera.GetComponent<Camera>()
                ?? throw new InvalidOperationException("ActionCamera has no Camera.");
            moveJoystick = RequireSingleInScene<CombatHudVirtualJoystick>(scene);
            aimDrag = RequireSingleInScene<CombatHudAimDragInput>(scene);
            hudBinder = RequireSingleInScene<OneRowCombatHudBinder>(scene);
            exitTransition = RequireSingleInScene<CityHeroPocketExitTransitionController>(scene);
            if (!exitTransition.IsConfigured || exitTransition.Encounter != encounter)
            {
                throw new InvalidOperationException(
                    "City exit transition is not bound to the canonical encounter.");
            }
            characterController = exitTransition.PlayerController;
            if (exitTransition.PlayerMovement != playerMovement
                || exitTransition.PlayerAction != playerAction
                || exitTransition.PlayerCombatMode != playerCombatMode
                || exitTransition.PlayerRangedAttack != rangedAction
                || exitTransition.EnemyAi != enemySoldier
                || exitTransition.EnemyProjectileDriver != enemyProjectileDriver)
            {
                throw new InvalidOperationException(
                    "City exit transition product bindings do not match the canonical actors.");
            }
            hud = exitTransition.HudCanvasGroup;
            attackButton = RequireNamedRect(hudBinder.transform, "BasicAttackButton");
            dodgeButton = RequireNamedRect(hudBinder.transform, "DodgeButton");
            g01Truck = RequireNamedTransform(scene, "PROP_TRUCK");
            g01Bicycle = RequireNamedTransform(scene, "PROP_BICYCLE");
            g01SignalLeft = RequireNamedTransform(scene, "SIGNAL_L");
            g01SignalRight = RequireNamedTransform(scene, "SIGNAL_R");
            g01WireCross = RequireNamedTransform(scene, "WIRE_CROSS");
            g01BackgroundEndLeft = RequireNamedTransform(scene, "BG_END_L");
            g01BackgroundEndRight = RequireNamedTransform(scene, "BG_END_R");
            eventSystem = EventSystem.current
                ?? throw new InvalidOperationException("City capture requires EventSystem.current.");
        }

        private void CaptureRestorableState()
        {
            savedCaptureFramerate = Time.captureFramerate;
            savedTargetFrameRate = Application.targetFrameRate;
            savedHudAlpha = hud.alpha;
            savedHudInteractable = hud.interactable;
            savedHudBlocksRaycasts = hud.blocksRaycasts;
            savedCameraPosition = gameplayCamera.transform.position;
            savedCameraRotation = gameplayCamera.transform.rotation;
            savedCameraFieldOfView = gameplayCamera.fieldOfView;
            savedCameraNearClipPlane = gameplayCamera.nearClipPlane;
            savedActionCameraEnabled = actionCamera.enabled;
            savedActionCameraTarget = actionCamera.Target;
            savedActionCameraThreat = actionCamera.Threat;
            restorableStateCaptured = true;
        }

        private void ValidateProductStateForShot(bool isContinuation)
        {
            if (!savedActionCameraEnabled || !actionCamera.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "City capture requires the authored ActionCamera to remain enabled.");
            }
            if (isContinuation
                && (encounter.GetInstanceID() != sequenceEncounterInstanceId
                    || playerHealth.GetInstanceID() != sequencePlayerInstanceId
                    || enemyHealth.GetInstanceID() != sequenceEnemyInstanceId))
            {
                throw new InvalidOperationException(
                    "City same-session encounter/player/enemy identity changed between shots.");
            }

            if (shot == AuditionPvCityShot.G01)
            {
                if (isContinuation || !encounter.IsRunning || !playerHealth.IsAlive
                    || !enemyHealth.IsAlive || exitTransition.IsArmed
                    || exitTransition.IsTransitionRunning || exitTransition.IsExitReady)
                {
                    throw new InvalidOperationException(
                        "G01 requires the fresh live, unarmed City product state.");
                }
                if (playerMovement.IsCinematicMoveInputLocked
                    || playerAction.IsCinematicInputLocked
                    || playerCombatMode.IsCinematicInputLocked
                    || rangedAction.IsCinematicInputLocked
                    || enemySoldier.IsGameplaySuspended)
                {
                    throw new InvalidOperationException(
                        "G01 cannot acquire its capture-owned locks over pre-existing product locks.");
                }
                return;
            }

            if (!isContinuation)
            {
                throw new InvalidOperationException(
                    "G02 and G03 must be prepared through same-director continuation.");
            }
            if (shot == AuditionPvCityShot.G02)
            {
                if (!encounter.IsRunning || !playerHealth.IsAlive || !enemyHealth.IsAlive
                    || exitTransition.IsArmed || exitTransition.IsTransitionRunning
                    || exitTransition.IsExitReady)
                {
                    throw new InvalidOperationException(
                        "G02 requires the unchanged live outcome from G01.");
                }
                return;
            }

            bool outside = !exitTransition.ExitTrigger.bounds.Intersects(
                characterController.bounds);
            bool south = IsPlayerSouthOfExitTrigger();
            if (!encounter.IsWon || enemyHealth.IsAlive || !playerHealth.IsAlive
                || !exitTransition.IsArmed || exitTransition.IsTransitionRunning
                || exitTransition.IsExitReady || exitTransition.TriggerAcceptedCount != 0
                || exitTransition.RejectedTriggerEnterCount != 0
                || exitTransition.TransitionStartedCount != 0
                || !outside || !south
                || g02EncounterInstanceId != encounter.GetInstanceID()
                || g02PlayerInstanceId != playerHealth.GetInstanceID()
                || g02EnemyInstanceId != enemyHealth.GetInstanceID())
            {
                throw new InvalidOperationException(
                    "G03 requires the exact G02 Won/armed identity and south/outside player handoff.");
            }
        }

        private void CaptureShotStartEvidence()
        {
            proof.encounterInstanceId = encounter.GetInstanceID();
            proof.playerInstanceId = playerHealth.GetInstanceID();
            proof.enemyInstanceId = enemyHealth.GetInstanceID();
            proof.continuityFromPreviousShot = shot == AuditionPvCityShot.G01
                || proof.encounterInstanceId == sequenceEncounterInstanceId
                    && proof.playerInstanceId == sequencePlayerInstanceId
                    && proof.enemyInstanceId == sequenceEnemyInstanceId;
            proof.playerHealthAtShotStart = playerHealth.CurrentHealth;
            proof.enemyHealthAtShotStart = enemyHealth.CurrentHealth;
            proof.ammoAtShotStart = rangedAction.CurrentAmmo;
            proof.playerProjectileCountAtShotStart = rangedAction.ActiveProjectileCount;
            proof.enemyFiredCountAtShotStart = enemyProjectileDriver.FiredCount;
            shotPlayerStartPosition = characterController.transform.position;
            shotEnemyStartPosition = enemyHealth.transform.position;
            lastObservedPlayerPosition = shotPlayerStartPosition;

            if (shot == AuditionPvCityShot.G01)
            {
                proof.g01GameplaySuspensionExact = true;
                proof.g01HudRootStayedActive = hud.gameObject.activeInHierarchy;
                return;
            }
            if (shot == AuditionPvCityShot.G02)
            {
                proof.g02PlayerStayedInBounds = Mathf.Abs(shotPlayerStartPosition.x) <= 6f
                    && Mathf.Abs(shotPlayerStartPosition.z) <= 9f;
                actionCameraRifleFeedbackAtShotStart =
                    actionCamera.RifleFireFeedbackRequestCount;
                actionCameraMicroShakeAtShotStart =
                    actionCamera.MicroShakeRequestCount;
                Transform playerProjectileRoot = rangedAction.ProjectileRoot;
                Transform enemyProjectileRoot = enemyProjectileDriver.RuntimeProjectileRoot;
                Scene scene = encounter.gameObject.scene;
                proof.g02ProjectileRootsIndependentAndSceneOwned =
                    playerProjectileRoot != null
                    && enemyProjectileRoot != null
                    && playerProjectileRoot != enemyProjectileRoot
                    && playerProjectileRoot.gameObject.scene == scene
                    && enemyProjectileRoot.gameObject.scene == scene
                    && !playerProjectileRoot.IsChildOf(playerHealth.transform)
                    && !enemyProjectileRoot.IsChildOf(enemyHealth.transform)
                    && enemyProjectileDriver.HasIndependentRuntimeProjectileRoot;
                return;
            }

            proof.g03StartedAlreadyWon = encounter.IsWon && !enemyHealth.IsAlive;
            proof.g03StartedTransitionArmed = exitTransition.IsArmed;
            proof.g03StartedOutsideExitTrigger =
                !exitTransition.ExitTrigger.bounds.Intersects(characterController.bounds);
            proof.g03StartedSouthOfExitTrigger = IsPlayerSouthOfExitTrigger();
            proof.naturalEnemyDeathObserved = !enemyHealth.IsAlive;
            proof.naturalWonObserved = encounter.IsWon;
        }

        private void AcquireG01Suspension()
        {
            playerMovement.SetCinematicMoveInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerCombatMode.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            rangedAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            enemySoldier.SetGameplaySuspended(true);
            g01SuspensionOwned = true;
            proof.g01GameplaySuspensionExact &=
                playerMovement.IsCinematicMoveInputLocked
                && playerAction.IsCinematicInputLocked
                && playerCombatMode.IsCinematicInputLocked
                && rangedAction.IsCinematicInputLocked
                && enemySoldier.IsGameplaySuspended;
        }

        private void ReleaseG01Suspension()
        {
            if (!g01SuspensionOwned)
            {
                return;
            }
            enemySoldier.SetGameplaySuspended(false);
            rangedAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                false);
            playerCombatMode.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                false);
            playerAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                false);
            playerMovement.SetCinematicMoveInputLocked(
                PlayerInputLockSource.EditorVerification,
                false);
            g01SuspensionOwned = false;
            proof.g01GameplaySuspensionExact &=
                !playerMovement.IsCinematicMoveInputLocked
                && !playerAction.IsCinematicInputLocked
                && !playerCombatMode.IsCinematicInputLocked
                && !rangedAction.IsCinematicInputLocked
                && !enemySoldier.IsGameplaySuspended;
        }

        private void CleanupCurrentShot(bool finalCleanup)
        {
            if (proof == null || currentShotRestored || restoring)
            {
                return;
            }
            restoring = true;
            IsRunning = false;
            IsPrepared = false;
            Exception firstFailure = null;
            try
            {
                CaptureRestoreFailure(ref firstFailure, ReleaseAllPointers);
                CaptureRestoreFailure(ref firstFailure, () => rail?.Stop());
                CaptureRestoreFailure(ref firstFailure, ReleaseG01Suspension);
                CaptureRestoreFailure(ref firstFailure, UnsubscribeProductEvidence);
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    presentationClockLease?.Dispose();
                    presentationClockLease = null;
                });
                if (finalCleanup && shot == AuditionPvCityShot.G03)
                {
                    CaptureRestoreFailure(
                        ref firstFailure,
                        () => exitTransition?.ResetForRestart());
                }
                CaptureRestoreFailure(ref firstFailure, RestoreHudState);
                CaptureRestoreFailure(ref firstFailure, RestoreActionCameraState);
                if (finalCleanup)
                {
                    CaptureRestoreFailure(ref firstFailure, () =>
                    {
                        if (restorableStateCaptured)
                        {
                            Time.captureFramerate = savedCaptureFramerate;
                            Application.targetFrameRate = savedTargetFrameRate;
                        }
                        if (savedRandomStateValid)
                        {
                            UnityEngine.Random.state = savedRandomState;
                        }
                    });
                }
            }
            finally
            {
                proof.presentationClockReleased = !PresentationClock.IsManuallyDriven;
                proof.pointerLeasesReleased = ArePointerLeasesReleased();
                proof.hudStateRestored = IsHudStateRestored();
                proof.cameraStateRestored = IsCameraStateRestored();
                proof.actionCameraTransientStateRestored =
                    actionCamera == null
                    || !actionCamera.HasActiveCue
                        && !actionCamera.HasActiveMicroShake
                        && actionCamera.AimOrbitInput.sqrMagnitude <= 0.0001f
                        && actionCamera.LookPeekInput.sqrMagnitude <= 0.0001f;
                proof.actionCameraStateRestored = !restorableStateCaptured
                    || actionCamera == null
                    || actionCamera.enabled == savedActionCameraEnabled
                        && actionCamera.Target == savedActionCameraTarget
                        && actionCamera.Threat == savedActionCameraThreat;
                proof.cameraRailReleased = rail == null || !rail.IsPrepared;
                proof.transitionStateRestored = !finalCleanup
                    || shot != AuditionPvCityShot.G03
                    || exitTransition == null
                    || !exitTransition.IsArmed
                        && !exitTransition.IsTransitionRunning
                        && !exitTransition.IsExitReady
                        && !exitTransition.IsInputLocked
                        && !exitTransition.IsAiLocked
                        && exitTransition.TriggerAcceptedCount == 0
                        && exitTransition.RejectedTriggerEnterCount == 0
                        && exitTransition.TransitionStartedCount == 0;
                proof.stateRestored = firstFailure == null
                    && proof.presentationClockReleased
                    && proof.pointerLeasesReleased
                    && proof.hudStateRestored
                    && proof.cameraStateRestored
                    && proof.actionCameraStateRestored
                    && proof.actionCameraTransientStateRestored
                    && proof.cameraRailReleased
                    && proof.transitionStateRestored;
                currentShotRestored = true;
                restoring = false;
            }
            if (firstFailure != null)
            {
                throw new InvalidOperationException(
                    "City shot-state restoration encountered an error.",
                    firstFailure);
            }
        }

        private void RestoreHudState()
        {
            if (!restorableStateCaptured || hud == null)
            {
                return;
            }
            hud.alpha = savedHudAlpha;
            hud.interactable = savedHudInteractable;
            hud.blocksRaycasts = savedHudBlocksRaycasts;
        }

        private void RestoreActionCameraState()
        {
            if (!restorableStateCaptured || gameplayCamera == null || actionCamera == null)
            {
                return;
            }
            var handoffRoot = new GameObject("[AuditionPV_City_CameraRestoreHandoff]")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            Camera handoffCamera = null;
            try
            {
                handoffCamera = handoffRoot.AddComponent<Camera>();
                handoffCamera.enabled = false;
                handoffCamera.transform.SetPositionAndRotation(
                    savedCameraPosition,
                    savedCameraRotation);
                handoffCamera.fieldOfView = savedCameraFieldOfView;
                handoffCamera.nearClipPlane = savedCameraNearClipPlane;
                actionCamera.SetOrbitInput(Vector2.zero);
                actionCamera.SetAimOrbitInput(Vector2.zero);
                actionCamera.SetLookPeekInput(Vector2.zero);
                actionCamera.SetAimModifierActive(false);
                actionCamera.PrimeFromHandoffCamera(handoffCamera);
                actionCamera.ConfigureTargets(
                    savedActionCameraTarget,
                    savedActionCameraThreat);
                actionCamera.enabled = savedActionCameraEnabled;
                gameplayCamera.nearClipPlane = savedCameraNearClipPlane;
                gameplayCamera.fieldOfView = savedCameraFieldOfView;
                gameplayCamera.transform.SetPositionAndRotation(
                    savedCameraPosition,
                    savedCameraRotation);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(handoffRoot);
            }
        }

        private bool IsHudStateRestored()
        {
            return !restorableStateCaptured || hud == null
                || Mathf.Abs(hud.alpha - savedHudAlpha) <= FloatTolerance
                    && hud.interactable == savedHudInteractable
                    && hud.blocksRaycasts == savedHudBlocksRaycasts;
        }

        private bool IsCameraStateRestored()
        {
            return !restorableStateCaptured || gameplayCamera == null
                || Vector3.Distance(gameplayCamera.transform.position, savedCameraPosition)
                        <= FloatTolerance
                    && Quaternion.Angle(
                        gameplayCamera.transform.rotation,
                        savedCameraRotation) <= 0.01f
                    && Mathf.Abs(
                        gameplayCamera.fieldOfView - savedCameraFieldOfView)
                        <= FloatTolerance
                    && Mathf.Abs(
                        gameplayCamera.nearClipPlane - savedCameraNearClipPlane)
                        <= FloatTolerance;
        }

        private bool IsPlayerSouthOfExitTrigger()
        {
            return characterController.bounds.max.z
                <= exitTransition.ExitTrigger.bounds.min.z + FloatTolerance;
        }

        private void ObserveG01CompositionSample()
        {
            proof.g01CompositionSampleCount++;
            bool foreground = HasGameplayCameraVisibleRenderer(g01Truck)
                && HasGameplayCameraVisibleRenderer(g01Bicycle);
            bool midground = HasGameplayCameraVisibleRenderer(playerHealth.transform)
                && HasGameplayCameraVisibleRenderer(enemyHealth.transform);
            bool background = HasGameplayCameraVisibleRenderer(g01SignalLeft)
                && HasGameplayCameraVisibleRenderer(g01SignalRight)
                && (HasGameplayCameraVisibleRenderer(g01WireCross)
                    || HasGameplayCameraVisibleRenderer(g01BackgroundEndLeft)
                    || HasGameplayCameraVisibleRenderer(g01BackgroundEndRight));
            bool lineOfSight = HasClearSolidLineOfSight(playerHealth.transform)
                && HasClearSolidLineOfSight(enemyHealth.transform);
            proof.g01ForegroundDepthObserved |= foreground;
            proof.g01MidgroundActorsObserved |= midground;
            proof.g01BackgroundDepthObserved |= background;
            proof.g01PlayerEnemyLineOfSightClear |= lineOfSight;
            bool passed = foreground && midground && background && lineOfSight;
            proof.g01ThreeDepthCompositionObserved |= passed;
            if (passed)
            {
                proof.g01CompositionPassingSampleCount++;
            }
        }

        private bool HasGameplayCameraVisibleRenderer(Transform root)
        {
            return root != null && root.GetComponentsInChildren<Renderer>(true)
                .Any(IsGameplayCameraVisible);
        }

        private bool IsGameplayCameraVisible(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled
                || !renderer.gameObject.activeInHierarchy)
            {
                return false;
            }
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(gameplayCamera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds)
                && BoundsIntersectsGameplayViewport(renderer.bounds);
        }

        private bool BoundsIntersectsGameplayViewport(Bounds bounds)
        {
            Vector3 minimum = bounds.min;
            Vector3 maximum = bounds.max;
            var viewportCorners = new Vector3[8];
            int index = 0;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        viewportCorners[index++] = gameplayCamera.WorldToViewportPoint(
                            new Vector3(
                                x == 0 ? minimum.x : maximum.x,
                                y == 0 ? minimum.y : maximum.y,
                                z == 0 ? minimum.z : maximum.z));
                    }
                }
            }
            return AuditionPvCityHeroPocketCapture
                .PositiveDepthViewportRectIntersects(viewportCorners);
        }

        private bool BoundsCenterInsideSafeGameplayViewport(Bounds bounds)
        {
            if (!GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(gameplayCamera),
                    bounds)
                || !BoundsIntersectsGameplayViewport(bounds))
            {
                return false;
            }
            Vector3 center = gameplayCamera.WorldToViewportPoint(bounds.center);
            return center.z > 0f
                && center.x >= 0.05f && center.x <= 0.95f
                && center.y >= 0.05f && center.y <= 0.95f;
        }

        private bool HasClearSolidLineOfSight(Transform targetRoot)
        {
            if (!TryGetCombinedRendererBounds(targetRoot, out Bounds bounds))
            {
                return false;
            }
            Vector3 origin = gameplayCamera.transform.position;
            Vector3 offset = bounds.center - origin;
            float distance = offset.magnitude;
            if (distance <= 0.001f)
            {
                return true;
            }
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                offset / distance,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore);
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            for (int index = 0; index < hits.Length; index++)
            {
                Collider collider = hits[index].collider;
                if (collider == null || collider.gameObject.layer == ignoreRaycastLayer
                    || collider.transform == targetRoot
                    || collider.transform.IsChildOf(targetRoot))
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        private static bool TryGetCombinedRendererBounds(
            Transform root,
            out Bounds bounds)
        {
            Renderer[] renderers = root != null
                ? root.GetComponentsInChildren<Renderer>(true)
                : Array.Empty<Renderer>();
            bool found = false;
            bounds = default;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || !renderer.enabled
                    || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            return found;
        }

        private void ResetShotProof(AuditionPvCityShot newShot)
        {
            shot = newShot;
            proof = new AuditionPvCityHeroPocketRuntimeProof
            {
                shotId = AuditionPvCityHeroPocketCapture.GetShotId(newShot),
                expectedFrameCount =
                    AuditionPvCityHeroPocketCapture.GetExpectedFrameCount(newShot)
            };
            executedG02Commands.Clear();
            IsPrepared = false;
            IsRunning = false;
            IsComplete = false;
            Failure = null;
            currentShotRestored = false;
            currentFrame = -1;
            nextExpectedPresentedFrame = 0;
            nextMovePointerId = 5101;
            nextAttackPointerId = 5201;
            nextDodgePointerId = 5301;
            activeG03PreRollFrame = -1;
        }

        private IEnumerator PrepareG03TransitionPreRoll()
        {
            proof.transitionRejectedTriggerEnterBaseline =
                exitTransition.RejectedTriggerEnterCount;
            g03PreRollStartPosition = characterController.transform.position;
            g03PreRollLastPosition = g03PreRollStartPosition;
            g03AcceptedPosition = g03PreRollStartPosition;
            activeG03PreRollFrame = 0;
            proof.g03JoystickPointerId = AuditionPvCityHeroPocketCapture.G03JoystickPointerId;
            proof.g03JoystickInput = AuditionPvCityHeroPocketCapture.G03JoystickInput;
            g03MovePointer = PointerLease.Press(
                eventSystem,
                (RectTransform)moveJoystick.transform,
                AuditionPvCityHeroPocketCapture.G03JoystickPointerId,
                JoystickPoint(AuditionPvCityHeroPocketCapture.G03JoystickInput));
            proof.g03JoystickInputExact = Vector2.Distance(
                moveJoystick.CurrentInput,
                AuditionPvCityHeroPocketCapture.G03JoystickInput) <= 0.01f;
            if (!proof.g03JoystickInputExact)
            {
                throw new InvalidOperationException(
                    "G03 real HUD joystick did not expose the scheduled (0.05,1.0) input.");
            }

            for (int preRollFrame = 0;
                preRollFrame <= AuditionPvCityHeroPocketCapture.G03PreRollTimeoutFrame;
                preRollFrame++)
            {
                activeG03PreRollFrame = preRollFrame;
                presentationClockLease.SetFrame(preRollFrame);
                yield return WaitForNextPlayerFrame();

                Vector3 position = characterController.transform.position;
                if (proof.g03TriggerAcceptedPreRollFrame < 0)
                {
                    proof.g03JoystickInputMaintainedUntilTrigger &=
                        Vector2.Distance(
                            moveJoystick.CurrentInput,
                            AuditionPvCityHeroPocketCapture.G03JoystickInput)
                        <= 0.01f;
                    proof.g03PreRollPathLength += Vector3.Distance(
                        position,
                        g03PreRollLastPosition);
                    g03AcceptedPosition = position;
                }
                g03PreRollLastPosition = position;

                if (proof.g03TriggerAcceptedPreRollFrame >= 0 && g03MovePointer != null)
                {
                    g03MovePointer.Release();
                    g03MovePointer = null;
                }
                if (exitTransition.HudHiddenCount == 1
                    && exitTransition.IsHudHidden)
                {
                    break;
                }
                if (encounter.IsFailed || encounter.IsFaulted)
                {
                    throw new InvalidOperationException(
                        "G03 same-session transition pre-roll entered a failed encounter state.");
                }
            }

            g03MovePointer?.Release();
            g03MovePointer = null;
            activeG03PreRollFrame = -1;
            proof.g03PreRollNetDisplacement = Vector3.Distance(
                g03AcceptedPosition,
                g03PreRollStartPosition);
            proof.transitionRejectedTriggerEnterDelta =
                exitTransition.RejectedTriggerEnterCount
                - proof.transitionRejectedTriggerEnterBaseline;
            if (proof.g03TriggerAcceptedPreRollFrame
                    < AuditionPvCityHeroPocketCapture.G03TriggerAcceptFirstPreRollFrame
                || proof.g03TriggerAcceptedPreRollFrame
                    > AuditionPvCityHeroPocketCapture.G03TriggerAcceptLastPreRollFrame
                || exitTransition.TriggerAcceptedCount != 1
                || exitTransition.TransitionStartedCount != 1
                || exitTransition.HudHiddenCount != 1
                || !exitTransition.IsHudHidden
                || !exitTransition.IsTransitionRunning
                || !exitTransition.IsInputLocked
                || !exitTransition.IsAiLocked
                || !proof.g03JoystickInputMaintainedUntilTrigger
                || proof.transitionRejectedTriggerEnterBaseline != 0
                || exitTransition.RejectedTriggerEnterCount != 0
                || proof.transitionRejectedTriggerEnterDelta != 0)
            {
                throw new InvalidOperationException(
                    "G03 did not enter the real Won-gated trigger once in p36..p84 and reach the product HUD-hidden state by p120.");
            }
        }

        private void ExecuteG02InputFrame(int frameIndex)
        {
            AuditionPvCityInputCommand[] schedule =
                AuditionPvCityHeroPocketCapture.CreateG02InputSchedule();
            for (int index = 0; index < schedule.Length; index++)
            {
                AuditionPvCityInputCommand command = schedule[index];
                if (command.frame != frameIndex)
                {
                    continue;
                }
                ExecuteG02InputCommand(command);
                executedG02Commands.Add(command);
                switch (command.phase)
                {
                    case AuditionPvCityPointerPhase.Down:
                        proof.pointerDownCount++;
                        break;
                    case AuditionPvCityPointerPhase.Drag:
                        proof.pointerDragCount++;
                        break;
                    case AuditionPvCityPointerPhase.Up:
                        proof.pointerUpCount++;
                        break;
                }
            }
        }

        private void ExecuteG02InputCommand(AuditionPvCityInputCommand command)
        {
            switch (command.target)
            {
                case AuditionPvCityInputTarget.MoveJoystick:
                    if (command.phase == AuditionPvCityPointerPhase.Down)
                    {
                        movePointer = PointerLease.Press(
                            eventSystem,
                            (RectTransform)moveJoystick.transform,
                            nextMovePointerId++,
                            JoystickPoint(command.value));
                    }
                    else if (command.phase == AuditionPvCityPointerPhase.Drag)
                    {
                        RequirePointer(movePointer, "move").Drag(JoystickPoint(command.value));
                    }
                    else
                    {
                        RequirePointer(movePointer, "move").Release();
                        movePointer = null;
                    }
                    break;
                case AuditionPvCityInputTarget.BasicAttack:
                    if (command.phase == AuditionPvCityPointerPhase.Down)
                    {
                        attackPointer = PointerLease.Press(
                            eventSystem,
                            attackButton,
                            nextAttackPointerId++,
                            ScreenCenter(attackButton));
                    }
                    else
                    {
                        RequirePointer(attackPointer, "attack").Release();
                        attackPointer = null;
                    }
                    break;
                case AuditionPvCityInputTarget.Dodge:
                    if (command.phase == AuditionPvCityPointerPhase.Down)
                    {
                        proof.g02DodgeDownFrame = command.frame;
                        dodgePointer = PointerLease.Press(
                            eventSystem,
                            dodgeButton,
                            nextDodgePointerId++,
                            ScreenCenter(dodgeButton));
                    }
                    else
                    {
                        RequirePointer(dodgePointer, "dodge").Release();
                        dodgePointer = null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command.target));
            }
        }

        private void ObservePerFrameProductEvidence(int frameIndex)
        {
            Vector3 playerPosition = characterController.transform.position;
            if (shot == AuditionPvCityShot.G01)
            {
                proof.g01PlayerDrift = Mathf.Max(
                    proof.g01PlayerDrift,
                    Vector3.Distance(playerPosition, shotPlayerStartPosition));
                proof.g01EnemyDrift = Mathf.Max(
                    proof.g01EnemyDrift,
                    Vector3.Distance(enemyHealth.transform.position, shotEnemyStartPosition));
                proof.g01GameplaySuspensionExact &= g01SuspensionOwned
                    && playerMovement.IsCinematicMoveInputLocked
                    && playerAction.IsCinematicInputLocked
                    && playerCombatMode.IsCinematicInputLocked
                    && rangedAction.IsCinematicInputLocked
                    && enemySoldier.IsGameplaySuspended;
                proof.g01HudRootStayedActive &= hud.gameObject.activeInHierarchy;
                if (frameIndex == 0 || frameIndex == 120 || frameIndex == 239)
                {
                    ObserveG01CompositionSample();
                }
                proof.presentationClockExact &=
                    Mathf.Abs(playerHealth.CurrentHealth - proof.playerHealthAtShotStart)
                        <= FloatTolerance
                    && rangedAction.CurrentAmmo == proof.ammoAtShotStart
                    && rangedAction.ActiveProjectileCount
                        == proof.playerProjectileCountAtShotStart
                    && enemyProjectileDriver.FiredCount
                        == proof.enemyFiredCountAtShotStart;
                return;
            }

            if (shot == AuditionPvCityShot.G02)
            {
                if (AuditionPvCityHeroPocketCapture
                    .IsG02PlayerFramingSampleFrame(frameIndex))
                {
                    proof.g02PlayerFramingSampleCount++;
                    if (BoundsCenterInsideSafeGameplayViewport(
                            characterController.bounds))
                    {
                        proof.g02PlayerFramingPassCount++;
                    }
                }
                if (AuditionPvCityHeroPocketCapture
                        .IsG02EnemyFramingSampleFrame(frameIndex)
                    && enemyHealth.IsAlive)
                {
                    proof.g02EnemyFramingSampleCount++;
                    if (TryGetCombinedRendererBounds(
                            enemyHealth.transform,
                            out Bounds enemyBounds)
                        && HasGameplayCameraVisibleRenderer(enemyHealth.transform)
                        && BoundsCenterInsideSafeGameplayViewport(enemyBounds))
                    {
                        proof.g02EnemyFramingPassCount++;
                    }
                }
                float displacement = Vector3.Distance(
                    playerPosition,
                    lastObservedPlayerPosition);
                proof.g02PlayerPathLength += displacement;
                proof.g02MaximumFrameDisplacement = Mathf.Max(
                    proof.g02MaximumFrameDisplacement,
                    displacement);
                proof.g02PlayerNetDisplacement = Vector3.Distance(
                    playerPosition,
                    shotPlayerStartPosition);
                proof.g02PlayerStayedInBounds &= Mathf.Abs(playerPosition.x) <= 6f
                    && Mathf.Abs(playerPosition.z) <= 9f;
                lastObservedPlayerPosition = playerPosition;
                if (HasGameplayCameraVisibleRenderer(rangedAction.ProjectileRoot))
                {
                    proof.g02PlayerProjectileVisibleFrameCount++;
                }
                if (HasGameplayCameraVisibleRenderer(
                        enemyProjectileDriver.RuntimeProjectileRoot))
                {
                    proof.g02EnemyProjectileVisibleFrameCount++;
                }
                if (enemyTelegraphPresenter.IsVisible
                    && enemyTelegraphPresenter.TelegraphRenderer != null
                    && enemyTelegraphPresenter.TelegraphRenderer.enabled
                    && IsGameplayCameraVisible(
                        enemyTelegraphPresenter.TelegraphRenderer))
                {
                    proof.g02EnemyTelegraphVisibleFrameCount++;
                }
                return;
            }

            if (frameIndex == 0)
            {
                proof.transitionFrameAtLogicalFrameZero =
                    exitTransition.PresentationFrame;
                proof.transitionAdvancedOnceBeforeRailFrameZero =
                    proof.transitionFrameAtLogicalFrameZero
                    == proof.transitionFrameBeforeLogicalFrameZero + 1;
            }
            proof.transitionPresentationFrameAtEnd =
                exitTransition.PresentationFrame;
            proof.transitionPortalReachedAuthoredScale |=
                exitTransition.PortalGrowProgress01 >= 1f - FloatTolerance
                && Vector3.Distance(
                    exitTransition.PortalRoot.localScale,
                    exitTransition.PortalAuthoredScale) <= FloatTolerance;
            if (proof.transitionPortalAuthoredLogicalFrame < 0
                && exitTransition.PresentationFrame >= 42
                && proof.transitionPortalReachedAuthoredScale)
            {
                proof.transitionPortalAuthoredLogicalFrame = frameIndex;
            }
            if (proof.transitionCoverStartedLogicalFrame < 0
                && exitTransition.PresentationFrame >= 234)
            {
                proof.transitionCoverStartedLogicalFrame = frameIndex;
            }
            if (exitTransition.IsFullCover)
            {
                proof.transitionCoverReachedFull = true;
                if (proof.transitionFullCoverLogicalFrame < 0)
                {
                    proof.transitionFullCoverLogicalFrame = frameIndex;
                }
                if (frameIndex >= 276)
                {
                    proof.cleanCoverFrameCount++;
                }
            }
            proof.transitionInputAndAiLocked &= exitTransition.IsInputLocked
                && exitTransition.IsAiLocked;
        }

        private void ValidateCompletedProductShot()
        {
            if (!IsHudModeExact() || !actionCamera.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "City capture ended with HUD or ActionCamera contract drift.");
            }
            if (shot == AuditionPvCityShot.G01)
            {
                proof.playerHealthAtShotEnd = playerHealth.CurrentHealth;
                proof.enemyHealthAtShotEnd = enemyHealth.CurrentHealth;
                proof.ammoAtShotEnd = rangedAction.CurrentAmmo;
                proof.playerProjectileCountAtShotEnd = rangedAction.ActiveProjectileCount;
                proof.enemyFiredCountAtShotEnd = enemyProjectileDriver.FiredCount;
                if (!encounter.IsRunning || proof.enemyDiedCount != 0
                    || proof.encounterWonCount != 0
                    || proof.g01PlayerDrift > G01MaximumDrift
                    || proof.g01EnemyDrift > G01MaximumDrift
                    || !proof.g01GameplaySuspensionExact)
                {
                    throw new InvalidOperationException(
                        "G01 must remain a frozen, live, non-terminal product state.");
                }
                return;
            }
            if (shot == AuditionPvCityShot.G02)
            {
                proof.g02PointerScheduleExact = ScheduleMatches(
                    executedG02Commands,
                    AuditionPvCityHeroPocketCapture.CreateG02InputSchedule());
                proof.playerHealthAtShotEnd = playerHealth.CurrentHealth;
                proof.enemyHealthAtShotEnd = enemyHealth.CurrentHealth;
                proof.ammoAtShotEnd = rangedAction.CurrentAmmo;
                proof.playerProjectileCountAtShotEnd = rangedAction.ActiveProjectileCount;
                proof.enemyFiredCountAtShotEnd = enemyProjectileDriver.FiredCount;
                proof.g02EnemyFiredDelta = proof.enemyFiredCountAtShotEnd
                    - proof.enemyFiredCountAtShotStart;
                proof.g02RifleFeedbackRequestDelta =
                    actionCamera.RifleFireFeedbackRequestCount
                    - actionCameraRifleFeedbackAtShotStart;
                proof.g02MicroShakeRequestDelta = actionCamera.MicroShakeRequestCount
                    - actionCameraMicroShakeAtShotStart;
                proof.g02PlayerAliveAtEnd = playerHealth.IsAlive;
                proof.g02EnemyHealthAtEnd = enemyHealth.CurrentHealth;
                proof.g02EndedOutsideExitTrigger =
                    !exitTransition.ExitTrigger.bounds.Intersects(characterController.bounds);
                proof.g02EndedSouthOfExitTrigger = IsPlayerSouthOfExitTrigger();
                g02EncounterInstanceId = encounter.GetInstanceID();
                g02PlayerInstanceId = playerHealth.GetInstanceID();
                g02EnemyInstanceId = enemyHealth.GetInstanceID();
                if (!proof.g02PointerScheduleExact || !encounter.IsWon
                    || proof.enemyDiedCount != 1 || proof.encounterWonCount != 1
                    || !proof.g02EndedOutsideExitTrigger
                    || !proof.g02EndedSouthOfExitTrigger)
                {
                    throw new InvalidOperationException(
                        "G02 did not finish its exact pointer schedule, natural Won, and south/outside handoff state.");
                }
                return;
            }

            proof.playerHealthAtShotEnd = playerHealth.CurrentHealth;
            proof.enemyHealthAtShotEnd = enemyHealth.CurrentHealth;
            proof.ammoAtShotEnd = rangedAction.CurrentAmmo;
            proof.playerProjectileCountAtShotEnd = rangedAction.ActiveProjectileCount;
            proof.enemyFiredCountAtShotEnd = enemyProjectileDriver.FiredCount;
            proof.g03NewDamageEventCount = proof.enemyDamagedCount;
            proof.g03NewDeathEventCount = proof.enemyDiedCount;
            proof.g03NewWonEventCount = proof.encounterWonCount;
            proof.transitionTriggerAcceptedCount = exitTransition.TriggerAcceptedCount;
            proof.transitionRejectedTriggerEnterEnd =
                exitTransition.RejectedTriggerEnterCount;
            proof.transitionRejectedTriggerEnterDelta =
                proof.transitionRejectedTriggerEnterEnd
                - proof.transitionRejectedTriggerEnterBaseline;
            proof.transitionStartedCount = exitTransition.TransitionStartedCount;
            proof.transitionHudHiddenCount = exitTransition.HudHiddenCount;
            proof.transitionFullCoverCount = exitTransition.FullCoverCount;
            proof.transitionExitReadyCount = exitTransition.ExitReadyCount;
            if (!exitTransition.IsExitReady
                || proof.transitionTriggerAcceptedCount != 1
                || proof.transitionStartedCount != 1
                || proof.transitionHudHiddenCount != 1
                || proof.transitionFullCoverCount != 1
                || proof.transitionExitReadyCount != 1
                || proof.transitionRejectedTriggerEnterBaseline != 0
                || proof.transitionRejectedTriggerEnterEnd != 0
                || proof.transitionRejectedTriggerEnterDelta != 0
                || proof.g03NewDamageEventCount != 0
                || proof.g03NewDeathEventCount != 0
                || proof.g03NewWonEventCount != 0
                || proof.captureTransitionStartCallCount != 0)
            {
                throw new InvalidOperationException(
                    "G03 authored exit transition did not complete once without capture-owned gameplay or transition mutation.");
            }
        }

        private void SubscribeProductEvidence()
        {
            rangedAction.RangedProjectileFired += HandleRangedProjectileFired;
            rangedAction.RangedReloadStarted += HandleRangedReloadStarted;
            playerAction.DodgeStarted += HandleDodgeStarted;
            playerAction.DodgeEnded += HandleDodgeEnded;
            enemySoldier.PatternStateChanged += HandleEnemyPatternStateChanged;
            enemyHealth.Damaged += HandleEnemyDamaged;
            enemyHealth.Died += HandleEnemyDied;
            encounter.Won += HandleEncounterWon;
            exitTransition.TriggerAccepted += HandleTransitionTriggerAccepted;
        }

        private void UnsubscribeProductEvidence()
        {
            if (rangedAction != null)
            {
                rangedAction.RangedProjectileFired -= HandleRangedProjectileFired;
                rangedAction.RangedReloadStarted -= HandleRangedReloadStarted;
            }
            if (playerAction != null)
            {
                playerAction.DodgeStarted -= HandleDodgeStarted;
                playerAction.DodgeEnded -= HandleDodgeEnded;
            }
            if (enemySoldier != null)
            {
                enemySoldier.PatternStateChanged -= HandleEnemyPatternStateChanged;
            }
            if (enemyHealth != null)
            {
                enemyHealth.Damaged -= HandleEnemyDamaged;
                enemyHealth.Died -= HandleEnemyDied;
            }
            if (encounter != null)
            {
                encounter.Won -= HandleEncounterWon;
            }
            if (exitTransition != null)
            {
                exitTransition.TriggerAccepted -= HandleTransitionTriggerAccepted;
            }
        }

        private void HandleRangedProjectileFired(LaneActionProjectile _)
        {
            proof.rangedProjectileFiredCount++;
        }

        private void HandleEnemyDamaged(DamageInfo info)
        {
            proof.enemyDamagedCount++;
        }

        private void HandleEnemyDied()
        {
            proof.enemyDiedCount++;
            proof.naturalEnemyDeathObserved = true;
        }

        private void HandleEncounterWon()
        {
            proof.encounterWonCount++;
            proof.naturalWonObserved = encounter.IsWon;
        }

        private void HandleRangedReloadStarted()
        {
            proof.g02ReloadStartedCount++;
        }

        private void HandleDodgeStarted()
        {
            proof.g02DodgeStartedCount++;
            if (shot != AuditionPvCityShot.G02)
            {
                return;
            }
            AuditionPvCityCameraRailPose pose =
                AuditionPvCityHeroPocketCapture.EvaluateRail(shot, currentFrame);
            Quaternion rotation = Quaternion.LookRotation(
                (pose.lookAt - pose.position).normalized,
                Vector3.up);
            proof.g02DodgeDirectionRailRightDot = Vector3.Dot(
                playerAction.LastDodgeDirection.normalized,
                rotation * Vector3.right);
        }

        private void HandleDodgeEnded()
        {
            proof.g02DodgeEndedCount++;
        }

        private void HandleEnemyPatternStateChanged(
            CombatAiPatternState state,
            CombatAiPatternProfile _)
        {
            if (shot == AuditionPvCityShot.G02 && state == CombatAiPatternState.Windup)
            {
                proof.g02EnemyTelegraphObserved = true;
            }
        }

        private void HandleTransitionTriggerAccepted()
        {
            if (shot == AuditionPvCityShot.G03
                && activeG03PreRollFrame >= 0
                && proof.g03TriggerAcceptedPreRollFrame < 0)
            {
                proof.g03TriggerAcceptedPreRollFrame = activeG03PreRollFrame;
                proof.g03JoystickInputExactAtTrigger = g03MovePointer != null
                    && Vector2.Distance(
                            moveJoystick.CurrentInput,
                            AuditionPvCityHeroPocketCapture.G03JoystickInput)
                        <= 0.01f;
                proof.g03JoystickInputMaintainedUntilTrigger &=
                    proof.g03JoystickInputExactAtTrigger;
                g03AcceptedPosition = characterController.transform.position;
                proof.g03PreRollPathLength += Vector3.Distance(
                    g03AcceptedPosition,
                    g03PreRollLastPosition);
            }
        }

        private void SetHudModeForCapture()
        {
            if (shot == AuditionPvCityShot.G03)
            {
                proof.captureHudMutationCount++;
                throw new InvalidOperationException(
                    "G03 HUD state is product-owned and cannot be mutated by capture code.");
            }
            bool visible = shot == AuditionPvCityShot.G02;
            hud.alpha = visible ? 1f : 0f;
            hud.interactable = visible;
            hud.blocksRaycasts = visible;
        }

        private void EnforceHudModeForRenderedFrame()
        {
            if (shot == AuditionPvCityShot.G01 || shot == AuditionPvCityShot.G02)
            {
                SetHudModeForCapture();
            }
            else if (!IsHudModeExact())
            {
                throw new InvalidOperationException(
                    "G03 product HUD-hidden state drifted during the recorded transition.");
            }
        }

        private bool IsHudModeExact()
        {
            bool visible = shot == AuditionPvCityShot.G02;
            return hud.gameObject.activeInHierarchy
                && Mathf.Abs(hud.alpha - (visible ? 1f : 0f)) <= FloatTolerance
                && hud.interactable == visible
                && hud.blocksRaycasts == visible;
        }

        private Vector2 JoystickPoint(Vector2 normalizedInput)
        {
            RectTransform rect = (RectTransform)moveJoystick.transform;
            float visualRadius = Mathf.Max(1f,
                Mathf.Min(rect.rect.width, rect.rect.height) * 0.5f);
            Vector2 local = rect.rect.center
                + Vector2.ClampMagnitude(normalizedInput, 1f)
                    * visualRadius
                    * AuditionPvCityHeroPocketCapture.AuthoredJoystickInputRadiusRatio;
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null
                && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            return RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                rect.TransformPoint(local));
        }

        private static Vector2 ScreenCenter(RectTransform rect)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null
                && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            return RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                rect.TransformPoint(rect.rect.center));
        }

        private void ReleaseAllPointers()
        {
            movePointer?.Release();
            movePointer = null;
            attackPointer?.Release();
            attackPointer = null;
            dodgePointer?.Release();
            dodgePointer = null;
            g03MovePointer?.Release();
            g03MovePointer = null;
        }

        private bool ArePointerLeasesReleased()
        {
            return movePointer == null && attackPointer == null
                && dodgePointer == null && g03MovePointer == null
                && (moveJoystick == null || !moveJoystick.IsPointerHeld
                    && moveJoystick.CurrentInput.sqrMagnitude <= 0.0001f)
                && (aimDrag == null || !aimDrag.IsPointerHeld)
                && (rangedAction == null || !rangedAction.HasExternalFireHeldInput);
        }

        private void ValidateFreshScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded
                || !string.Equals(
                    scene.path,
                    AuditionPvCityHeroPocketCapture.CityScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The director is not in a fresh CityHeroPocketStage scene.");
            }
        }

        private void Fail(Exception exception)
        {
            Failure = exception;
            IsRunning = false;
            try
            {
                RestoreShotState();
            }
            catch (Exception restoreException)
            {
                Debug.LogException(restoreException, this);
            }
            Debug.LogException(exception, this);
        }

        internal void ReportFailure(Exception exception)
        {
            Fail(exception ?? new InvalidOperationException(
                "City capture reported an unspecified failure."));
        }

        private void OnDisable()
        {
            TryRestoreFromLifecycle();
        }

        private void OnDestroy()
        {
            TryRestoreFromLifecycle();
        }

        private void TryRestoreFromLifecycle()
        {
            try
            {
                RestoreShotState();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private static bool ScheduleMatches(
            IReadOnlyList<AuditionPvCityInputCommand> actual,
            IReadOnlyList<AuditionPvCityInputCommand> expected)
        {
            if (actual.Count != expected.Count)
            {
                return false;
            }
            for (int index = 0; index < actual.Count; index++)
            {
                if (actual[index].frame != expected[index].frame
                    || actual[index].target != expected[index].target
                    || actual[index].phase != expected[index].phase
                    || Vector2.Distance(actual[index].value, expected[index].value)
                        > 0.0001f)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool PoseApproximately(
            AuditionPvCityCameraRailPose actual,
            AuditionPvCityCameraRailPose expected)
        {
            return Vector3.Distance(actual.position, expected.position) <= FloatTolerance
                && Vector3.Distance(actual.lookAt, expected.lookAt) <= FloatTolerance
                && Mathf.Abs(actual.fieldOfView - expected.fieldOfView) <= FloatTolerance;
        }

        private static T RequireSingleInScene<T>(Scene scene) where T : Component
        {
            T[] matches = UnityEngine.Object.FindObjectsByType<T>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(value => value != null && value.gameObject.scene == scene)
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"City capture requires exactly one {typeof(T).Name}; found {matches.Length}.");
            }
            return matches[0];
        }

        private static RectTransform RequireNamedRect(Transform root, string objectName)
        {
            RectTransform[] matches = root.GetComponentsInChildren<RectTransform>(true)
                .Where(value => value.name == objectName)
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"City HUD requires exactly one '{objectName}'; found {matches.Length}.");
            }
            return matches[0];
        }

        private static Transform RequireNamedTransform(Scene scene, string objectName)
        {
            Transform[] matches = UnityEngine.Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(value => value != null
                    && value.gameObject.scene == scene
                    && value.name == objectName)
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"City product scene requires exactly one '{objectName}'; found {matches.Length}.");
            }
            return matches[0];
        }

        private static PointerLease RequirePointer(PointerLease pointer, string label)
        {
            return pointer ?? throw new InvalidOperationException(
                $"The {label} pointer has no active real ExecuteEvents lease.");
        }

        private static WaitUntil WaitForNextPlayerFrame()
        {
            int frame = Time.frameCount;
            return new WaitUntil(() => Time.frameCount > frame);
        }

        private static void CaptureRestoreFailure(
            ref Exception firstFailure,
            Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception exception)
            {
                firstFailure ??= exception;
            }
        }

        private sealed class PointerLease
        {
            private readonly PointerEventData data;
            private readonly GameObject pressHandler;
            private readonly GameObject dragHandler;
            private bool released;

            private PointerLease(
                PointerEventData newData,
                GameObject newPressHandler,
                GameObject newDragHandler)
            {
                data = newData;
                pressHandler = newPressHandler;
                dragHandler = newDragHandler;
            }

            public static PointerLease Press(
                EventSystem eventSystem,
                RectTransform expectedHandler,
                int pointerId,
                Vector2 screenPosition)
            {
                var data = new PointerEventData(eventSystem)
                {
                    pointerId = pointerId,
                    button = PointerEventData.InputButton.Left,
                    position = screenPosition,
                    pressPosition = screenPosition
                };
                var hits = new List<RaycastResult>();
                eventSystem.RaycastAll(data, hits);
                RaycastResult selected = default;
                GameObject handler = null;
                for (int index = 0; index < hits.Count; index++)
                {
                    GameObject candidate = ExecuteEvents.GetEventHandler<IPointerDownHandler>(
                        hits[index].gameObject);
                    if (candidate == null)
                    {
                        continue;
                    }
                    selected = hits[index];
                    handler = candidate;
                    break;
                }
                if (handler == null || handler != expectedHandler.gameObject)
                {
                    throw new InvalidOperationException(
                        $"Topmost actionable HUD raycast resolved '{handler?.name ?? "<none>"}', "
                        + $"expected '{expectedHandler.name}'.");
                }
                data.pointerCurrentRaycast = selected;
                data.pointerPressRaycast = selected;
                data.rawPointerPress = selected.gameObject;
                data.pointerPress = ExecuteEvents.ExecuteHierarchy(
                    selected.gameObject,
                    data,
                    ExecuteEvents.pointerDownHandler);
                data.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(
                    selected.gameObject);
                if (data.pointerPress != handler)
                {
                    throw new InvalidOperationException(
                        $"Pointer-down routed to '{data.pointerPress?.name}', expected '{handler.name}'.");
                }
                return new PointerLease(data, handler, data.pointerDrag);
            }

            public void Drag(Vector2 screenPosition)
            {
                if (released || dragHandler == null)
                {
                    throw new InvalidOperationException(
                        "A released or non-draggable pointer cannot be dragged.");
                }
                data.delta = screenPosition - data.position;
                data.position = screenPosition;
                ExecuteEvents.Execute(dragHandler, data, ExecuteEvents.dragHandler);
            }

            public void Release()
            {
                if (released)
                {
                    return;
                }
                released = true;
                ExecuteEvents.Execute(pressHandler, data, ExecuteEvents.pointerUpHandler);
            }
        }
    }

    /// <summary>
    /// Runs after the product ActionCamera and transition. It replaces only the
    /// base shot pose, then composes the public, clamped product micro-shake.
    /// </summary>
    [DefaultExecutionOrder(32500)]
    public sealed class AuditionPvCityHeroPocketCameraRail : MonoBehaviour
    {
        private AuditionPvCityHeroPocketDirector director;
        private Camera gameplayCamera;
        private ActionCameraController actionCamera;
        private bool prepared;

        public int LastBaseFrame { get; private set; } = -1;
        public AuditionPvCityCameraRailPose LastBasePose { get; private set; }
        public bool IsPrepared => prepared;

        internal void Configure(AuditionPvCityHeroPocketDirector newDirector)
        {
            director = newDirector ?? throw new ArgumentNullException(nameof(newDirector));
        }

        internal void Prepare(
            Camera newGameplayCamera,
            ActionCameraController newActionCamera)
        {
            gameplayCamera = newGameplayCamera
                ?? throw new ArgumentNullException(nameof(newGameplayCamera));
            actionCamera = newActionCamera
                ?? throw new ArgumentNullException(nameof(newActionCamera));
            if (!actionCamera.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "The City camera rail may not replace a disabled ActionCamera.");
            }
            prepared = true;
        }

        internal void Stop()
        {
            prepared = false;
        }

        private void LateUpdate()
        {
            if (!prepared || director == null || !director.IsRunning
                || director.Failure != null)
            {
                return;
            }
            try
            {
                int frame = director.CurrentFrame;
                director.ExecuteLatePreRailAction(frame);
                AuditionPvCityCameraRailPose pose =
                    AuditionPvCityHeroPocketCapture.EvaluateRail(
                        director.Shot,
                        frame);
                LastBaseFrame = frame;
                LastBasePose = pose;
                Quaternion baseRotation = Quaternion.LookRotation(
                    (pose.lookAt - pose.position).normalized,
                    Vector3.up);
                Vector3 sourcePosition = actionCamera.LastMicroShakeLocalOffset;
                Vector3 sourceEuler = actionCamera.LastMicroShakeEulerOffset;
                Vector3 composedPosition = Vector3.ClampMagnitude(
                    sourcePosition,
                    AuditionPvCityHeroPocketCapture.CameraRailMicroShakePositionClamp);
                Vector3 composedEuler = Vector3.ClampMagnitude(
                    sourceEuler,
                    AuditionPvCityHeroPocketCapture.CameraRailMicroShakeEulerClamp);
                gameplayCamera.transform.SetPositionAndRotation(
                    pose.position + baseRotation * composedPosition,
                    baseRotation * Quaternion.Euler(composedEuler));
                gameplayCamera.fieldOfView = pose.fieldOfView;
                director.CompleteFrameFromRail(frame);
            }
            catch (Exception exception)
            {
                director.ReportFailure(exception);
            }
        }
    }
}
