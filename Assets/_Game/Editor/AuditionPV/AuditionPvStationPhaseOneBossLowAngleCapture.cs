using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Canonical S050 source contract plus the capture-only Phase 1 camera director.
    /// The director never damages the boss, requests a transition, or saves the scene.
    /// </summary>
    public static class AuditionPvStationPhaseOneBossLowAngleCapture
    {
        internal const string SegmentId = "PV_S050";
        internal const string ShotId = "s050";
        internal const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        internal const string PhaseOneVisualPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_SciFiSoldier01_CommandoVisual.prefab";
        internal const string PhaseOneVisualPrefabGuid =
            "d405ed8ecd0740748a4c4f82842ebd49";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhaseOneBossLowAngleCapture.cs";
        internal const string FlowScriptPath =
            "Assets/_Game/Scripts/LevelDesign/OlympusStationAkazaPhase2FlowController.cs";
        internal const string CameraScriptPath =
            "Assets/_Game/Scripts/Presentation/ActionCameraController.cs";
        internal const string CadenceScriptPath =
            "Assets/_Game/Scripts/Combat/BossCombatCadenceScheduler.cs";
        internal const string PressurePositionScriptPath =
            "Assets/_Game/Scripts/Combat/BossPressurePositionController.cs";

        internal const string PhaseOneVisualProperty = "phaseOneVisualRoot";
        internal const string PhaseTwoVisualProperty = "phaseTwoVisualRoot";
        internal const string EyeCameraProperty = "eyeOpenCamera";
        internal const string WingCameraProperty = "wingDeployCamera";
        internal const string GameplayCameraControllerProperty =
            "gameplayCameraController";
        internal const string GameplayCameraProperty = "gameplayCamera";
        internal const string HudProperty = "combatHudCanvasGroup";
        internal const string PlayerSkill1Property = "playerSkill1Action";
        internal const string PlayerSummon1Property = "playerSummonSlot1Action";
        internal const string PlayerSummon2Property = "playerSummonSlot2Action";
        internal const string PlayerSummon3Property = "playerSummonSlot3Action";
        internal const string PlayerCombatModeProperty =
            "playerCombatModeController";

        internal const int RailPresetCount = 3;
        internal const int FirstSourceFrame = 0;
        internal const int LastSourceFrame = 599;
        internal const int SourceFrameCount = 600;
        internal const int PreHandleFrameCount = 180;
        internal const int SelectedFirstSourceFrame = 180;
        internal const int SelectedLastSourceFrame = 419;
        internal const int SelectedFrameCount = 240;
        internal const int PostHandleFrameCount = 180;
        internal const int DeterministicRandomSeed = 0x5050;
        internal const float CameraFieldOfView = 40f;
        internal const float MinimumSelectedProjectedHeight = 0.25f;
        internal const float MaximumSelectedProjectedHeight = 0.40f;
        internal const float MaximumLowAngleEyeRatio = 0.22f;
        internal const int DistanceSearchIterations = 20;
        internal const string GameplayState =
            "station-phase1-full-health-hud-off-boss-low-angle";
        internal const string TimelineId =
            "s050-phase1-low-angle-source-v1";

        private static readonly RailPreset[] Presets =
        {
            new RailPreset("rail-ltr-032", -18f, 18f, 0.32f, 0.32f),
            new RailPreset("rail-rtl-034", 18f, -18f, 0.34f, 0.34f),
            new RailPreset("rail-push-029-036", -7f, 7f, 0.29f, 0.36f)
        };

        internal readonly struct RailPreset
        {
            public RailPreset(
                string id,
                float startYawDegrees,
                float endYawDegrees,
                float startProjectedHeight,
                float endProjectedHeight)
            {
                Id = id;
                StartYawDegrees = startYawDegrees;
                EndYawDegrees = endYawDegrees;
                StartProjectedHeight = startProjectedHeight;
                EndProjectedHeight = endProjectedHeight;
            }

            public string Id { get; }
            public float StartYawDegrees { get; }
            public float EndYawDegrees { get; }
            public float StartProjectedHeight { get; }
            public float EndProjectedHeight { get; }
        }

        internal readonly struct CameraPose
        {
            public CameraPose(Vector3 position, Quaternion rotation, float fieldOfView)
            {
                Position = position;
                Rotation = rotation;
                FieldOfView = fieldOfView;
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public float FieldOfView { get; }
        }

        internal readonly struct CameraComposition
        {
            public CameraComposition(
                float projectedHeight,
                float minimumDepth,
                float eyeHeightRatio,
                float minimumViewportX,
                float maximumViewportX,
                float minimumViewportY,
                float maximumViewportY)
            {
                ProjectedHeight = projectedHeight;
                MinimumDepth = minimumDepth;
                EyeHeightRatio = eyeHeightRatio;
                MinimumViewportX = minimumViewportX;
                MaximumViewportX = maximumViewportX;
                MinimumViewportY = minimumViewportY;
                MaximumViewportY = maximumViewportY;
            }

            public float ProjectedHeight { get; }
            public float MinimumDepth { get; }
            public float EyeHeightRatio { get; }
            public float MinimumViewportX { get; }
            public float MaximumViewportX { get; }
            public float MinimumViewportY { get; }
            public float MaximumViewportY { get; }
            public bool AllCornersInFront => MinimumDepth > 0.001f;
        }

        internal static RailPreset GetRailPreset(int takeOrdinal)
        {
            if (takeOrdinal < 1 || takeOrdinal > RailPresetCount)
            {
                throw new ArgumentOutOfRangeException(nameof(takeOrdinal));
            }

            return Presets[takeOrdinal - 1];
        }

        internal static string CameraId(int takeOrdinal)
        {
            return "station-gameplay-camera-s050-"
                + GetRailPreset(takeOrdinal).Id;
        }

        internal static int DeterministicSeed(int takeOrdinal)
        {
            GetRailPreset(takeOrdinal);
            return DeterministicRandomSeed + takeOrdinal;
        }

        internal static int SourceToSelectedLogicalFrame(int sourceFrame)
        {
            ValidateSourceFrame(sourceFrame);
            return sourceFrame >= SelectedFirstSourceFrame
                && sourceFrame <= SelectedLastSourceFrame
                ? sourceFrame - SelectedFirstSourceFrame
                : -1;
        }

        internal static string SourceFrameRole(int sourceFrame)
        {
            ValidateSourceFrame(sourceFrame);
            if (sourceFrame < SelectedFirstSourceFrame)
            {
                return "prehandle";
            }

            return sourceFrame <= SelectedLastSourceFrame
                ? "selected"
                : "posthandle";
        }

        internal static float RailProgressForSourceFrame(int sourceFrame)
        {
            ValidateSourceFrame(sourceFrame);
            if (sourceFrame <= SelectedFirstSourceFrame)
            {
                return 0f;
            }

            if (sourceFrame >= SelectedLastSourceFrame)
            {
                return 1f;
            }

            return (sourceFrame - SelectedFirstSourceFrame)
                / (float)(SelectedFrameCount - 1);
        }

        internal static string FrameFileName(int sourceFrame)
        {
            ValidateSourceFrame(sourceFrame);
            return $"frame_{sourceFrame:0000}.png";
        }

        internal static AuditionPvShotManifestEntry CreateShotManifestEntry(
            int takeOrdinal)
        {
            RailPreset preset = GetRailPreset(takeOrdinal);
            return new AuditionPvShotManifestEntry
            {
                id = ShotId,
                scenePath = StationScenePath,
                startFrame = FirstSourceFrame,
                endFrame = LastSourceFrame,
                expectedFrameCount = SourceFrameCount,
                hudMode = "hud-off",
                notes =
                    $"{SegmentId} take ordinal {takeOrdinal} ({preset.Id}); fresh Station Phase 1; "
                    + "capture-only gameplay-camera rail; source f0..f599; "
                    + "prehandle f0..f179; select f180..f419 -> logical f0..f239; "
                    + "posthandle f420..f599; QHD lossless PNG at exact 60 fps; "
                    + "no boss HP, phase-transition, or product-asset mutation."
            };
        }

        internal static AuditionPvBaselineManifestEntry[]
            CreateBaselineManifestEntries(int takeOrdinal)
        {
            string prefix = $"S050_T{takeOrdinal:00}";
            return new[]
            {
                Baseline("select-start", SelectedFirstSourceFrame,
                    $"{prefix}_SELECT_START__HUDOFF__f0180.png"),
                Baseline("select-mid", 300,
                    $"{prefix}_SELECT_MID__HUDOFF__f0300.png"),
                Baseline("select-end", SelectedLastSourceFrame,
                    $"{prefix}_SELECT_END__HUDOFF__f0419.png")
            };
        }

        internal static string[] ExplicitDependencyPaths()
        {
            return new[]
            {
                StationScenePath,
                PhaseOneVisualPrefabPath,
                CaptureScriptPath,
                FlowScriptPath,
                CameraScriptPath,
                CadenceScriptPath,
                PressurePositionScriptPath
            };
        }

        internal static AuditionPvStationPhaseOneBossLowAngleDirector
            AttachToFreshActiveScene(int takeOrdinal)
        {
            GetRailPreset(takeOrdinal);
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "S050 director can only attach in Play Mode.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(scene.path, StationScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 requires a freshly opened Olympus Station scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<
                    AuditionPvStationPhaseOneBossLowAngleDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The active scene already contains an S050 director.");
            }

            var root = new GameObject($"[AuditionPV_S050_T{takeOrdinal:00}_Director]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            AuditionPvStationPhaseOneBossLowAngleDirector director =
                root.AddComponent<AuditionPvStationPhaseOneBossLowAngleDirector>();
            director.Prepare(takeOrdinal);
            return director;
        }

        internal static void ReopenProductSceneAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before reopening the Station scene.");
            }

            EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
        }

        internal static CameraPose ResolveRailPose(
            Bounds bossBounds,
            Vector3 viewerDirection,
            RailPreset preset,
            float progress)
        {
            if (bossBounds.size.y <= 0.01f)
            {
                throw new ArgumentException("Boss bounds have no usable height.", nameof(bossBounds));
            }

            progress = Mathf.Clamp01(progress);
            Vector3 flatViewer = Vector3.ProjectOnPlane(viewerDirection, Vector3.up);
            if (flatViewer.sqrMagnitude <= 0.0001f)
            {
                flatViewer = Vector3.back;
            }

            flatViewer.Normalize();
            float yaw = Mathf.Lerp(preset.StartYawDegrees, preset.EndYawDegrees, progress);
            Vector3 radial = Quaternion.AngleAxis(yaw, Vector3.up) * flatViewer;
            float desiredHeight = Mathf.Lerp(
                preset.StartProjectedHeight,
                preset.EndProjectedHeight,
                progress);
            Vector3 target = bossBounds.center
                + Vector3.up * (bossBounds.size.y * 0.06f);
            float eyeY = bossBounds.min.y + bossBounds.size.y * 0.14f;
            float nearDistance = Mathf.Max(0.35f, bossBounds.size.y * 0.55f);
            float farDistance = Mathf.Max(nearDistance + 1f, bossBounds.size.y * 20f);

            CameraPose pose = default;
            for (int iteration = 0; iteration < DistanceSearchIterations; iteration++)
            {
                float distance = (nearDistance + farDistance) * 0.5f;
                Vector3 position = target + radial * distance;
                position.y = eyeY;
                Quaternion rotation = Quaternion.LookRotation(target - position, Vector3.up);
                pose = new CameraPose(position, rotation, CameraFieldOfView);
                CameraComposition composition = EvaluateComposition(bossBounds, pose);
                if (!composition.AllCornersInFront
                    || composition.ProjectedHeight > desiredHeight)
                {
                    nearDistance = distance;
                }
                else
                {
                    farDistance = distance;
                }
            }

            float resolvedDistance = (nearDistance + farDistance) * 0.5f;
            Vector3 resolvedPosition = target + radial * resolvedDistance;
            resolvedPosition.y = eyeY;
            Quaternion resolvedRotation = Quaternion.LookRotation(
                target - resolvedPosition,
                Vector3.up);
            return new CameraPose(
                resolvedPosition,
                resolvedRotation,
                CameraFieldOfView);
        }

        internal static CameraComposition EvaluateComposition(
            Bounds bounds,
            CameraPose pose)
        {
            Vector3[] corners = BoundsCorners(bounds);
            Quaternion inverseRotation = Quaternion.Inverse(pose.Rotation);
            float tangent = Mathf.Tan(pose.FieldOfView * 0.5f * Mathf.Deg2Rad);
            float aspect = AuditionPvCaptureContract.Width
                / (float)AuditionPvCaptureContract.Height;
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float minDepth = float.PositiveInfinity;
            foreach (Vector3 corner in corners)
            {
                Vector3 local = inverseRotation * (corner - pose.Position);
                minDepth = Mathf.Min(minDepth, local.z);
                float safeDepth = Mathf.Max(0.0001f, local.z);
                float viewportX = 0.5f + local.x / (2f * safeDepth * tangent * aspect);
                float viewportY = 0.5f + local.y / (2f * safeDepth * tangent);
                minX = Mathf.Min(minX, viewportX);
                maxX = Mathf.Max(maxX, viewportX);
                minY = Mathf.Min(minY, viewportY);
                maxY = Mathf.Max(maxY, viewportY);
            }

            float eyeRatio = (pose.Position.y - bounds.min.y)
                / Mathf.Max(0.0001f, bounds.size.y);
            return new CameraComposition(
                maxY - minY,
                minDepth,
                eyeRatio,
                minX,
                maxX,
                minY,
                maxY);
        }

        private static AuditionPvBaselineManifestEntry Baseline(
            string idSuffix,
            int sourceFrame,
            string fileName)
        {
            return new AuditionPvBaselineManifestEntry
            {
                id = $"s050-{idSuffix}",
                shotId = ShotId,
                sourceFrame = sourceFrame,
                fileName = fileName,
                hudMode = "hud-off",
                status = "captured"
            };
        }

        private static Vector3[] BoundsCorners(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            return new[]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };
        }

        private static void ValidateSourceFrame(int sourceFrame)
        {
            if (sourceFrame < FirstSourceFrame || sourceFrame > LastSourceFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceFrame));
            }
        }
    }

    /// <summary>
    /// Runtime-only camera/HUD lease for one S050 take. All mutations are restored
    /// both explicitly and from Unity disable/destroy lifecycle callbacks.
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public sealed class AuditionPvStationPhaseOneBossLowAngleDirector : MonoBehaviour
    {
        private const float ScalarTolerance = 0.001f;

        private OlympusStationAkazaPhase2FlowController flow;
        private BossBarrageEncounterController encounter;
        private BossPressurePositionController pressurePosition;
        private CombatHealth bossHealth;
        private PlayerMovementController playerMovement;
        private PlayerActionController playerAction;
        private PlayerRangedBasicAttackAction rangedAction;
        private PlayerSkill1Action skill1Action;
        private PlayerSummonSlot1Action summon1Action;
        private PlayerSupportSummonSlotAction summon2Action;
        private PlayerSupportSummonSlotAction summon3Action;
        private PlayerCombatModeController combatMode;
        private GameObject phaseOneVisual;
        private GameObject phaseTwoVisual;
        private Camera eyeCamera;
        private Camera wingCamera;
        private ActionCameraController gameplayCameraController;
        private Camera gameplayCamera;
        private CanvasGroup combatHud;
        private Renderer[] bossRenderers = Array.Empty<Renderer>();
        private IDisposable cadenceLease;
        private PresentationClock.ManualLease presentationClockLease;

        private AuditionPvStationPhaseOneBossLowAngleCapture.RailPreset preset;
        private Bounds preparedBounds;
        private Vector3 viewerDirection;
        private UnityEngine.Random.State savedRandomState;
        private bool savedRandomStateValid;
        private bool savedEncounterSuspended;
        private bool savedMovementEnabled;
        private bool savedGameplayCameraControllerEnabled;
        private bool savedGameplayCameraEnabled;
        private bool savedEyeCameraEnabled;
        private bool savedWingCameraEnabled;
        private Vector3 savedCameraPosition;
        private Quaternion savedCameraRotation = Quaternion.identity;
        private float savedCameraFieldOfView;
        private float savedHudAlpha;
        private bool savedHudInteractable;
        private bool savedHudBlocksRaycasts;
        private bool savedMovementInputLocked;
        private bool savedActionInputLocked;
        private bool savedRangedInputLocked;
        private bool savedSkill1InputLocked;
        private bool savedSummon1InputLocked;
        private bool savedSummon2InputLocked;
        private bool savedSummon3InputLocked;
        private bool savedCombatModeInputLocked;
        private int savedCaptureFramerate;
        private int savedTargetFrameRate;
        private float initialBossHealth;
        private int initialTransitionStartCount;
        private int initialTransitionCompletionCount;
        private int observedTransitionStartedEvents;
        private int observedTransitionCompletedEvents;
        private int currentSourceFrame = -1;
        private int observedFrameCount;
        private int observedSelectedFrameCount;
        private float minimumProjectedHeight = float.PositiveInfinity;
        private float maximumProjectedHeight = float.NegativeInfinity;
        private float maximumEyeHeightRatio = float.NegativeInfinity;
        private float minimumCornerDepth = float.PositiveInfinity;
        private bool allSelectedFramesInFront = true;
        private bool allSelectedFramesLowAngle = true;
        private bool allSelectedFramesInCoverage = true;
        private bool allFramesHudOff = true;
        private bool allFramesPhaseOne = true;
        private bool cameraTakeoverObserved;
        private bool restorableStateCaptured;
        private bool restoring;
        private bool restorationVerified;

        public event Action<int> FramePresented;

        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public Exception Failure { get; private set; }
        public int TakeOrdinal { get; private set; }
        public string RailPresetId => preset.Id ?? string.Empty;
        public int CurrentSourceFrame => currentSourceFrame;
        public int ObservedFrameCount => observedFrameCount;
        public int ObservedSelectedFrameCount => observedSelectedFrameCount;
        public int ObservedTransitionStartedEvents => observedTransitionStartedEvents;
        public int ObservedTransitionCompletedEvents => observedTransitionCompletedEvents;
        public float MinimumProjectedHeight => IsFinite(minimumProjectedHeight)
            ? minimumProjectedHeight
            : 0f;
        public float MaximumProjectedHeight => IsFinite(maximumProjectedHeight)
            ? maximumProjectedHeight
            : 0f;
        public float MaximumEyeHeightRatio => IsFinite(maximumEyeHeightRatio)
            ? maximumEyeHeightRatio
            : 0f;
        public float MinimumCornerDepth => IsFinite(minimumCornerDepth)
            ? minimumCornerDepth
            : 0f;
        public bool AllSelectedFramesInFront => allSelectedFramesInFront;
        public bool AllSelectedFramesLowAngle => allSelectedFramesLowAngle;
        public bool AllSelectedFramesInCoverage => allSelectedFramesInCoverage;
        public bool AllFramesHudOff => allFramesHudOff;
        public bool AllFramesPhaseOne => allFramesPhaseOne;
        public bool CameraTakeoverObserved => cameraTakeoverObserved;
        public bool BossHealthUnchanged => bossHealth != null
            && Mathf.Abs(bossHealth.CurrentHealth - initialBossHealth) <= ScalarTolerance;
        public bool BossWasAndRemainsFullAndAlive => bossHealth != null
            && bossHealth.IsAlive
            && Mathf.Abs(initialBossHealth - bossHealth.MaxHealth) <= ScalarTolerance
            && BossHealthUnchanged;
        public bool TransitionStateUnchanged => flow != null
            && flow.CurrentPhase == OlympusStationAkazaPhase2FlowController.Phase.Phase1
            && !flow.PhaseTwoApplied
            && flow.TransitionStartCount == initialTransitionStartCount
            && flow.TransitionCompletionCount == initialTransitionCompletionCount
            && observedTransitionStartedEvents == 0
            && observedTransitionCompletedEvents == 0;
        public bool StateRestored => restorationVerified;

        internal void Prepare(int takeOrdinal)
        {
            if (IsPrepared || IsRunning || restorableStateCaptured)
            {
                throw new InvalidOperationException("An S050 director can be prepared only once.");
            }

            TakeOrdinal = takeOrdinal;
            preset = AuditionPvStationPhaseOneBossLowAngleCapture
                .GetRailPreset(takeOrdinal);
            ResolveAndValidateFreshBindings();
            CaptureRestorableState();
            bool prepared = false;
            try
            {
                AcquireCaptureLease();
                preparedBounds = ResolveBossBounds();
                Vector3 playerOffset = flow.PlayerHealth.transform.position
                    - preparedBounds.center;
                viewerDirection = Vector3.ProjectOnPlane(playerOffset, Vector3.up);
                if (viewerDirection.sqrMagnitude <= 0.0001f)
                {
                    viewerDirection = -pressurePosition.MovedTransform.forward;
                }

                ApplyCameraForSourceFrame(
                    AuditionPvStationPhaseOneBossLowAngleCapture.FirstSourceFrame);
                IsPrepared = true;
                prepared = true;
            }
            finally
            {
                if (!prepared)
                {
                    RestoreShotState();
                }
            }
        }

        public void BeginShotForRecorder()
        {
            if (!IsPrepared || IsRunning || IsComplete || restorationVerified)
            {
                throw new InvalidOperationException(
                    "Prepare a fresh S050 director before starting its Recorder interval.");
            }

            float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
            if (Time.captureDeltaTime < minimumDelta
                || Time.captureDeltaTime >= minimumDelta + 0.001f)
            {
                throw new InvalidOperationException(
                    "Recorder cadence padding is not active at S050 source f0.");
            }

            currentSourceFrame =
                AuditionPvStationPhaseOneBossLowAngleCapture.FirstSourceFrame;
            presentationClockLease.SetFrame(currentSourceFrame);
            IsRunning = true;
        }

        public void RestoreShotState()
        {
            if (restorationVerified || restoring)
            {
                return;
            }

            restoring = true;
            IsRunning = false;
            Exception firstFailure = null;
            if (!restorableStateCaptured)
            {
                restoring = false;
                restorationVerified = true;
                return;
            }

            CaptureRestoreFailure(ref firstFailure, () =>
            {
                if (flow != null)
                {
                    flow.TransitionStarted -= HandleTransitionStarted;
                    flow.TransitionCompleted -= HandleTransitionCompleted;
                }
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                presentationClockLease?.Dispose();
                presentationClockLease = null;
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                if (gameplayCamera != null)
                {
                    gameplayCamera.transform.SetPositionAndRotation(
                        savedCameraPosition,
                        savedCameraRotation);
                    gameplayCamera.fieldOfView = savedCameraFieldOfView;
                    gameplayCamera.enabled = savedGameplayCameraEnabled;
                }

                if (eyeCamera != null)
                {
                    eyeCamera.enabled = savedEyeCameraEnabled;
                }

                if (wingCamera != null)
                {
                    wingCamera.enabled = savedWingCameraEnabled;
                }

                if (gameplayCameraController != null)
                {
                    gameplayCameraController.enabled =
                        savedGameplayCameraControllerEnabled;
                }
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                if (combatHud != null)
                {
                    combatHud.alpha = savedHudAlpha;
                    combatHud.interactable = savedHudInteractable;
                    combatHud.blocksRaycasts = savedHudBlocksRaycasts;
                }
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                playerMovement?.SetCinematicMoveInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                playerAction?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                rangedAction?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                skill1Action?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                summon1Action?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                summon2Action?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                summon3Action?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
                combatMode?.SetCinematicInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    false);
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                if (pressurePosition != null)
                {
                    pressurePosition.SetMovementEnabled(savedMovementEnabled);
                }
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                if (encounter != null)
                {
                    encounter.SetExternalCombatSuspended(savedEncounterSuspended);
                }
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                cadenceLease?.Dispose();
                cadenceLease = null;
            });
            CaptureRestoreFailure(ref firstFailure, () =>
            {
                if (savedRandomStateValid)
                {
                    UnityEngine.Random.state = savedRandomState;
                    savedRandomStateValid = false;
                }

                Time.captureFramerate = savedCaptureFramerate;
                Application.targetFrameRate = savedTargetFrameRate;
            });

            restorationVerified = VerifyRestoredState();
            restoring = false;
            if (firstFailure != null)
            {
                throw new InvalidOperationException(
                    "S050 capture lease restoration failed.", firstFailure);
            }

            if (!restorationVerified)
            {
                throw new InvalidOperationException(
                    "S050 capture lease did not restore every captured property.");
            }
        }

        private void Update()
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }

            try
            {
                presentationClockLease.SetFrame(currentSourceFrame);
                ApplyCameraForSourceFrame(currentSourceFrame);
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private void LateUpdate()
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }

            try
            {
                ObservePresentedFrame(currentSourceFrame);
                if (currentSourceFrame
                    == AuditionPvStationPhaseOneBossLowAngleCapture.LastSourceFrame)
                {
                    ValidateCompletedShot();
                    IsRunning = false;
                    IsComplete = true;
                }

                FramePresented?.Invoke(currentSourceFrame);
                if (IsRunning)
                {
                    currentSourceFrame++;
                }
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private void OnDisable()
        {
            TryRestoreFromLifecycle();
        }

        private void OnDestroy()
        {
            TryRestoreFromLifecycle();
        }

        private void ResolveAndValidateFreshBindings()
        {
            Scene scene = gameObject.scene;
            if (!Application.isPlaying
                || !scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.path,
                    AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 requires the fresh Station PlayMode scene.");
            }

            OlympusStationAkazaPhase2FlowController[] flows =
                UnityEngine.Object.FindObjectsByType<
                    OlympusStationAkazaPhase2FlowController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            if (flows.Length != 1 || flows[0].gameObject.scene != scene)
            {
                throw new InvalidOperationException(
                    "S050 requires exactly one Station Phase 2 flow in the active scene.");
            }

            flow = flows[0];
            encounter = flow.EncounterController;
            pressurePosition = flow.PressurePositionController;
            bossHealth = flow.BossHealth;
            playerMovement = flow.PlayerMovement;
            playerAction = flow.PlayerActionController;
            rangedAction = flow.PlayerRangedBasicAttackAction;

            var serialized = new SerializedObject(flow);
            serialized.Update();
            phaseOneVisual = ReadObject<GameObject>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PhaseOneVisualProperty);
            phaseTwoVisual = ReadObject<GameObject>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PhaseTwoVisualProperty);
            eyeCamera = ReadObject<Camera>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.EyeCameraProperty);
            wingCamera = ReadObject<Camera>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.WingCameraProperty);
            gameplayCameraController = ReadObject<ActionCameraController>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .GameplayCameraControllerProperty);
            gameplayCamera = ReadObject<Camera>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.GameplayCameraProperty);
            combatHud = ReadObject<CanvasGroup>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.HudProperty);
            skill1Action = ReadObject<PlayerSkill1Action>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PlayerSkill1Property);
            summon1Action = ReadObject<PlayerSummonSlot1Action>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PlayerSummon1Property);
            summon2Action = ReadObject<PlayerSupportSummonSlotAction>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PlayerSummon2Property);
            summon3Action = ReadObject<PlayerSupportSummonSlotAction>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PlayerSummon3Property);
            combatMode = ReadObject<PlayerCombatModeController>(
                serialized,
                AuditionPvStationPhaseOneBossLowAngleCapture.PlayerCombatModeProperty);

            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                phaseOneVisual);
            bossRenderers = phaseOneVisual
                .GetComponentsInChildren<Renderer>(includeInactive: false)
                .Where(renderer => renderer != null
                    && renderer.enabled
                    && renderer.gameObject.activeInHierarchy)
                .ToArray();

            if (encounter == null
                || pressurePosition == null
                || pressurePosition.MovedTransform == null
                || bossHealth == null
                || flow.PlayerHealth == null
                || playerMovement == null
                || playerAction == null
                || rangedAction == null
                || skill1Action == null
                || summon1Action == null
                || summon2Action == null
                || summon3Action == null
                || combatMode == null
                || phaseOneVisual == null
                || phaseTwoVisual == null
                || eyeCamera == null
                || wingCamera == null
                || gameplayCameraController == null
                || gameplayCamera == null
                || combatHud == null
                || bossRenderers.Length == 0
                || !string.Equals(
                    prefabPath,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabPath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 could not resolve the exact Station Phase 1 boss/HUD/camera bindings.");
            }

            if (flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase1
                || flow.PhaseTwoApplied
                || flow.TransitionStartCount != 0
                || flow.TransitionCompletionCount != 0
                || !bossHealth.IsAlive
                || Mathf.Abs(bossHealth.CurrentHealth - bossHealth.MaxHealth)
                    > ScalarTolerance
                || !phaseOneVisual.activeInHierarchy
                || phaseTwoVisual.activeInHierarchy
                || encounter.IsExternalCombatSuspended
                || !pressurePosition.MovementEnabled)
            {
                throw new InvalidOperationException(
                    "S050 did not attach to untouched, full-health Station Phase 1 state.");
            }
        }

        private void CaptureRestorableState()
        {
            savedEncounterSuspended = encounter.IsExternalCombatSuspended;
            savedMovementEnabled = pressurePosition.MovementEnabled;
            savedGameplayCameraControllerEnabled = gameplayCameraController.enabled;
            savedGameplayCameraEnabled = gameplayCamera.enabled;
            savedEyeCameraEnabled = eyeCamera.enabled;
            savedWingCameraEnabled = wingCamera.enabled;
            savedCameraPosition = gameplayCamera.transform.position;
            savedCameraRotation = gameplayCamera.transform.rotation;
            savedCameraFieldOfView = gameplayCamera.fieldOfView;
            savedHudAlpha = combatHud.alpha;
            savedHudInteractable = combatHud.interactable;
            savedHudBlocksRaycasts = combatHud.blocksRaycasts;
            savedMovementInputLocked = playerMovement.IsCinematicMoveInputLocked;
            savedActionInputLocked = playerAction.IsCinematicInputLocked;
            savedRangedInputLocked = rangedAction.IsCinematicInputLocked;
            savedSkill1InputLocked = skill1Action.IsCinematicInputLocked;
            savedSummon1InputLocked = summon1Action.IsCinematicInputLocked;
            savedSummon2InputLocked = summon2Action.IsCinematicInputLocked;
            savedSummon3InputLocked = summon3Action.IsCinematicInputLocked;
            savedCombatModeInputLocked = combatMode.IsCinematicInputLocked;
            savedCaptureFramerate = Time.captureFramerate;
            savedTargetFrameRate = Application.targetFrameRate;
            initialBossHealth = bossHealth.CurrentHealth;
            initialTransitionStartCount = flow.TransitionStartCount;
            initialTransitionCompletionCount = flow.TransitionCompletionCount;
            savedRandomState = UnityEngine.Random.state;
            savedRandomStateValid = true;
            restorableStateCaptured = true;
        }

        private void AcquireCaptureLease()
        {
            flow.TransitionStarted += HandleTransitionStarted;
            flow.TransitionCompleted += HandleTransitionCompleted;
            UnityEngine.Random.InitState(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .DeterministicSeed(TakeOrdinal));
            encounter.SetExternalCombatSuspended(true);
            cadenceLease = BossCombatCadenceScheduler.AcquireExternalSuspension(this);
            pressurePosition.SetMovementEnabled(false);
            playerMovement.SetCinematicMoveInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            rangedAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            skill1Action.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            summon1Action.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            summon2Action.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            summon3Action.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            combatMode.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            combatHud.alpha = 0f;
            combatHud.interactable = false;
            combatHud.blocksRaycasts = false;
            eyeCamera.enabled = false;
            wingCamera.enabled = false;
            gameplayCameraController.enabled = false;
            gameplayCamera.enabled = true;
            gameplayCamera.fieldOfView =
                AuditionPvStationPhaseOneBossLowAngleCapture.CameraFieldOfView;
            Time.captureFramerate = AuditionPvCaptureContract.Fps;
            Application.targetFrameRate = AuditionPvCaptureContract.Fps;
            presentationClockLease = PresentationClock.AcquireManual(
                this,
                AuditionPvCaptureContract.Fps);
        }

        private Bounds ResolveBossBounds()
        {
            Renderer first = bossRenderers.FirstOrDefault(renderer => renderer != null)
                ?? throw new InvalidOperationException("S050 lost every Phase 1 renderer.");
            Bounds bounds = first.bounds;
            foreach (Renderer renderer in bossRenderers)
            {
                if (renderer != null)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (bounds.size.y <= 0.01f)
            {
                throw new InvalidOperationException("S050 Phase 1 renderer bounds are empty.");
            }

            return bounds;
        }

        private void ApplyCameraForSourceFrame(int sourceFrame)
        {
            float progress = AuditionPvStationPhaseOneBossLowAngleCapture
                .RailProgressForSourceFrame(sourceFrame);
            AuditionPvStationPhaseOneBossLowAngleCapture.CameraPose pose =
                AuditionPvStationPhaseOneBossLowAngleCapture.ResolveRailPose(
                    preparedBounds,
                    viewerDirection,
                    preset,
                    progress);
            gameplayCamera.transform.SetPositionAndRotation(
                pose.Position,
                pose.Rotation);
            gameplayCamera.fieldOfView = pose.FieldOfView;
        }

        private void ObservePresentedFrame(int sourceFrame)
        {
            observedFrameCount++;
            allFramesHudOff &= Mathf.Abs(combatHud.alpha) <= ScalarTolerance
                && !combatHud.interactable
                && !combatHud.blocksRaycasts;
            allFramesPhaseOne &= flow.CurrentPhase
                    == OlympusStationAkazaPhase2FlowController.Phase.Phase1
                && !flow.PhaseTwoApplied
                && phaseOneVisual.activeInHierarchy
                && !phaseTwoVisual.activeInHierarchy;
            cameraTakeoverObserved |= gameplayCamera.enabled
                && !gameplayCameraController.enabled
                && !eyeCamera.enabled
                && !wingCamera.enabled;

            if (sourceFrame
                    < AuditionPvStationPhaseOneBossLowAngleCapture
                        .SelectedFirstSourceFrame
                || sourceFrame
                    > AuditionPvStationPhaseOneBossLowAngleCapture
                        .SelectedLastSourceFrame)
            {
                return;
            }

            observedSelectedFrameCount++;
            Bounds currentBounds = ResolveBossBounds();
            var pose = new AuditionPvStationPhaseOneBossLowAngleCapture.CameraPose(
                gameplayCamera.transform.position,
                gameplayCamera.transform.rotation,
                gameplayCamera.fieldOfView);
            AuditionPvStationPhaseOneBossLowAngleCapture.CameraComposition composition =
                AuditionPvStationPhaseOneBossLowAngleCapture.EvaluateComposition(
                    currentBounds,
                    pose);
            minimumProjectedHeight = Mathf.Min(
                minimumProjectedHeight,
                composition.ProjectedHeight);
            maximumProjectedHeight = Mathf.Max(
                maximumProjectedHeight,
                composition.ProjectedHeight);
            maximumEyeHeightRatio = Mathf.Max(
                maximumEyeHeightRatio,
                composition.EyeHeightRatio);
            minimumCornerDepth = Mathf.Min(
                minimumCornerDepth,
                composition.MinimumDepth);
            allSelectedFramesInFront &= composition.AllCornersInFront;
            allSelectedFramesLowAngle &=
                composition.EyeHeightRatio
                    <= AuditionPvStationPhaseOneBossLowAngleCapture
                        .MaximumLowAngleEyeRatio
                && gameplayCamera.transform.forward.y > 0f;
            allSelectedFramesInCoverage &=
                composition.ProjectedHeight
                    >= AuditionPvStationPhaseOneBossLowAngleCapture
                        .MinimumSelectedProjectedHeight
                && composition.ProjectedHeight
                    <= AuditionPvStationPhaseOneBossLowAngleCapture
                        .MaximumSelectedProjectedHeight;
        }

        private void ValidateCompletedShot()
        {
            if (observedFrameCount
                    != AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount
                || observedSelectedFrameCount
                    != AuditionPvStationPhaseOneBossLowAngleCapture.SelectedFrameCount
                || !allFramesHudOff
                || !allFramesPhaseOne
                || !cameraTakeoverObserved
                || !allSelectedFramesInFront
                || !allSelectedFramesLowAngle
                || !allSelectedFramesInCoverage
                || !BossWasAndRemainsFullAndAlive
                || !TransitionStateUnchanged
                || !encounter.IsExternalCombatSuspended
                || !BossCombatCadenceScheduler.IsExternallySuspended
                || pressurePosition.MovementEnabled)
            {
                throw new InvalidOperationException(
                    "S050 completed without satisfying its exact Phase 1, HUD-off, "
                    + "low-angle composition, HP, transition, or cadence contract.");
            }
        }

        private bool VerifyRestoredState()
        {
            return gameplayCamera != null
                && gameplayCameraController != null
                && eyeCamera != null
                && wingCamera != null
                && combatHud != null
                && encounter != null
                && pressurePosition != null
                && gameplayCamera.enabled == savedGameplayCameraEnabled
                && gameplayCameraController.enabled
                    == savedGameplayCameraControllerEnabled
                && eyeCamera.enabled == savedEyeCameraEnabled
                && wingCamera.enabled == savedWingCameraEnabled
                && Vector3.Distance(
                    gameplayCamera.transform.position,
                    savedCameraPosition) <= 0.0001f
                && Quaternion.Angle(
                    gameplayCamera.transform.rotation,
                    savedCameraRotation) <= 0.001f
                && Mathf.Abs(gameplayCamera.fieldOfView - savedCameraFieldOfView)
                    <= ScalarTolerance
                && Mathf.Abs(combatHud.alpha - savedHudAlpha) <= ScalarTolerance
                && combatHud.interactable == savedHudInteractable
                && combatHud.blocksRaycasts == savedHudBlocksRaycasts
                && encounter.IsExternalCombatSuspended == savedEncounterSuspended
                && pressurePosition.MovementEnabled == savedMovementEnabled
                && playerMovement.IsCinematicMoveInputLocked
                    == savedMovementInputLocked
                && playerAction.IsCinematicInputLocked == savedActionInputLocked
                && rangedAction.IsCinematicInputLocked == savedRangedInputLocked
                && skill1Action.IsCinematicInputLocked == savedSkill1InputLocked
                && summon1Action.IsCinematicInputLocked == savedSummon1InputLocked
                && summon2Action.IsCinematicInputLocked == savedSummon2InputLocked
                && summon3Action.IsCinematicInputLocked == savedSummon3InputLocked
                && combatMode.IsCinematicInputLocked
                    == savedCombatModeInputLocked
                && Time.captureFramerate == savedCaptureFramerate
                && Application.targetFrameRate == savedTargetFrameRate;
        }

        private void HandleTransitionStarted()
        {
            observedTransitionStartedEvents++;
        }

        private void HandleTransitionCompleted()
        {
            observedTransitionCompletedEvents++;
        }

        private void Fail(Exception exception)
        {
            Failure ??= exception;
            IsRunning = false;
            try
            {
                RestoreShotState();
            }
            catch (Exception restoreFailure)
            {
                Failure = new AggregateException(Failure, restoreFailure);
            }
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

        private static T ReadObject<T>(SerializedObject owner, string propertyName)
            where T : UnityEngine.Object
        {
            SerializedProperty property = owner.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"Station flow is missing serialized field '{propertyName}'.");
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new InvalidOperationException(
                    $"Station flow field '{propertyName}' is not an object reference.");
            }

            return property.objectReferenceValue as T
                ?? throw new InvalidOperationException(
                    $"Station flow field '{propertyName}' has no {typeof(T).Name} reference.");
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

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
