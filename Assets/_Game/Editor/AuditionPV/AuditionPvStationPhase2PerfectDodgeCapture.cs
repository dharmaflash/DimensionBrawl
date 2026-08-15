using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Capture contract and output integration for the deterministic G05 Station
    /// gameplay source. The product-state director below owns shot preparation;
    /// Recorder orchestration can start after IsPrepared becomes true and stop
    /// after IsComplete becomes true.
    /// </summary>
    public static class AuditionPvStationPhase2PerfectDodgeCapture
    {
        internal const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        internal const string CrushNetProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaCrushNet.asset";
        internal const string PhaseTwoOpeningProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhase2PerfectDodgeCapture.cs";
        internal const string PresentationClockPath =
            "Assets/_Game/Scripts/Presentation/PresentationClock.cs";
        internal const string PhaseTwoFlowPath =
            "Assets/_Game/Scripts/LevelDesign/OlympusStationAkazaPhase2FlowController.cs";
        internal const string BarrageEmitterPath =
            "Assets/_Game/Scripts/Combat/BossBarrageEmitter.cs";
        internal const string BarrageProjectilePath =
            "Assets/_Game/Scripts/Combat/BossBarrageProjectile.cs";
        internal const string CadenceSchedulerPath =
            "Assets/_Game/Scripts/Combat/BossCombatCadenceScheduler.cs";
        internal const string PlayerActionPath =
            "Assets/_Game/Scripts/Player/PlayerActionController.cs";
        internal const string ActionCameraPath =
            "Assets/_Game/Scripts/Presentation/ActionCameraController.cs";
        internal const string ActionScreenPath =
            "Assets/_Game/Scripts/Presentation/ActionScreenCuePresenter.cs";
        internal const string PerfectDodgeTimeWarpPath =
            "Assets/_Game/Scripts/Presentation/PerfectDodgeTimeWarp.cs";

        internal const string ShotId = "g05";
        internal const string BaselinesFolderName = "baselines";
        internal const string Bl03FileName =
            "BL03_AKAZA_PHASE2_CRUSHNET__HUDON__t00.000000.png";
        internal const string Bl06FileName =
            "BL06_AKAZA_PHASE2_PERFECT_DODGE__HUDON__t03.150000.png";
        internal const int FirstFrame = 0;
        internal const int LastFrame = 196;
        internal const int ExpectedFrameCount = 197;
        internal const int BeginWindupFrame = 1;
        internal const int FirePendingWaveFrame = 71;
        internal const int QueueDodgeFrame = 186;
        internal const int ImpactFrame = 188;
        internal const int PhaseTwoSettleFrames = 60;
        internal const int Bl03SourceFrame = 0;
        // The product presenter intentionally opens its screen-domain coroutine on
        // the frame after the real impact event. f188 remains the collision proof;
        // f189 is the first honest rendered screen-domain hero baseline.
        internal const int Bl06SourceFrame = ImpactFrame + 1;
        internal const int DeterministicRandomSeed = 0x4705;
        internal const float CaptureOnlyScreenDomainAlpha = 0.42f;
        internal const float CaptureOnlyScreenInvertAlpha = 0.18f;
        internal const float CaptureOnlyScreenEdgeAlpha = 0.48f;
        internal const float CaptureOnlyScreenGlitchAlpha = 0.16f;

        internal static AuditionPvShotManifestEntry CreateShotManifestEntry()
        {
            return new AuditionPvShotManifestEntry
            {
                id = ShotId,
                scenePath = StationScenePath,
                startFrame = FirstFrame,
                endFrame = LastFrame,
                expectedFrameCount = ExpectedFrameCount,
                hudMode = "hud-on",
                notes =
                    "Fresh Station product state. Actual threshold transition to Akaza Phase 2; "
                    + "60 fixed-60 Phase 2 camera/UI/animation settle frames; "
                    + "CrushNet BeginWindup f1, FirePendingWave f71, QueueDodge f186, "
                    + "real active projectile impact f188; screen-domain hero f189; "
                    + "Station scene-default screen domain is leased to the G05 runtime profile "
                    + "(.42/.18/.48/.16) and restored; 2560x1440 PNG at 60fps."
            };
        }

        internal static AuditionPvBaselineManifestEntry[] CreateBaselineManifestEntries()
        {
            return new[]
            {
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl03",
                    shotId = ShotId,
                    sourceFrame = Bl03SourceFrame,
                    fileName = Bl03FileName,
                    hudMode = "hud-on",
                    status = "captured"
                },
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl06",
                    shotId = ShotId,
                    sourceFrame = Bl06SourceFrame,
                    fileName = Bl06FileName,
                    hudMode = "hud-on",
                    status = "captured"
                }
            };
        }

        internal static string FrameFileName(int frameIndex)
        {
            ValidateFrameIndex(frameIndex);
            return $"frame_{frameIndex:0000}.png";
        }

        internal static float FrameTimeSeconds(int frameIndex)
        {
            ValidateFrameIndex(frameIndex);
            return frameIndex / (float)AuditionPvCaptureContract.Fps;
        }

        internal static string[] ExplicitProductDependencyPaths()
        {
            return new[]
            {
                StationScenePath,
                CrushNetProfilePath,
                PhaseTwoOpeningProfilePath,
                CaptureScriptPath,
                PresentationClockPath,
                PhaseTwoFlowPath,
                BarrageEmitterPath,
                BarrageProjectilePath,
                CadenceSchedulerPath,
                PlayerActionPath,
                ActionCameraPath,
                ActionScreenPath,
                PerfectDodgeTimeWarpPath
            };
        }

        internal static AuditionPvStationPhase2PerfectDodgeOutput ReserveNewOutput(
            DateTime startedAtUtc,
            AuditionPvGitSnapshot gitSnapshot = null)
        {
            AuditionPvGitSnapshot git = gitSnapshot
                ?? AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "G05 output reservation requires a successful Git provenance probe: "
                    + git.probeError);
            }

            string outputId = AuditionPvOutputPaths.CreateOutputId(
                "g05-station-phase2-perfect-dodge",
                startedAtUtc,
                git.commitSha,
                git.isDirty,
                git.dirtyStateHashSha256);
            return ReserveNewOutputForRoot(
                AuditionPvCaptureContract.OutputRoot,
                outputId);
        }

        internal static AuditionPvStationPhase2PerfectDodgeOutput ReserveNewOutputForRoot(
            string outputRoot,
            string outputId)
        {
            string outputDirectory =
                AuditionPvOutputPaths.CreateUniqueOutputDirectory(outputRoot, outputId);
            string baselineDirectory = Path.Combine(
                outputDirectory,
                BaselinesFolderName);
            Directory.CreateDirectory(baselineDirectory);

            AuditionPvRecorderSettingsBundle recorderSettings = null;
            try
            {
                recorderSettings =
                    AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence(
                        outputDirectory,
                        ShotId);
                AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
                return new AuditionPvStationPhase2PerfectDodgeOutput(
                    new DirectoryInfo(outputDirectory).Name,
                    outputDirectory,
                    baselineDirectory,
                    recorderSettings);
            }
            catch
            {
                recorderSettings?.Dispose();
                throw;
            }
        }

        internal static AuditionPvCaptureManifest CreateFinalManifest(
            AuditionPvStationPhase2PerfectDodgeOutput output,
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
                new[] { CreateShotManifestEntry() },
                CreateBaselineManifestEntries(),
                testResults,
                createdAtUtc: startedAtUtc,
                gitSnapshot: gitSnapshot,
                engineSnapshot: engineSnapshot,
                dependencyHashSnapshot: dependencyHashes);
        }

        internal static AuditionPvStationPhase2PerfectDodgeDirector AttachToFreshActiveScene()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "The G05 product-state director can only run in Play Mode.");
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !activeScene.isLoaded
                || !string.Equals(
                    activeScene.path,
                    StationScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G05 requires a fresh OlympusStationCombatStage PlayMode scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<
                    AuditionPvStationPhase2PerfectDodgeDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The active scene already owns a G05 shot director.");
            }

            var root = new GameObject("[AuditionPV_G05_ProductStateDirector]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, activeScene);
            return root.AddComponent<AuditionPvStationPhase2PerfectDodgeDirector>();
        }

        internal static void ReopenProductSceneAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before reopening the product scene.");
            }

            EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
        }

        private static void ValidateFrameIndex(int frameIndex)
        {
            if (frameIndex < FirstFrame || frameIndex > LastFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            }
        }
    }

    internal sealed class AuditionPvStationPhase2PerfectDodgeOutput : IDisposable
    {
        public readonly string captureId;
        public readonly string outputRoot;
        public readonly string outputDirectory;
        public readonly string baselineDirectory;
        public readonly AuditionPvRecorderSettingsBundle recorderSettings;

        internal AuditionPvStationPhase2PerfectDodgeOutput(
            string captureId,
            string outputDirectory,
            string baselineDirectory,
            AuditionPvRecorderSettingsBundle recorderSettings)
        {
            this.captureId = captureId;
            this.outputDirectory = Path.GetFullPath(outputDirectory)
                .Replace('\\', '/')
                .TrimEnd('/');
            this.outputRoot = Path.GetDirectoryName(this.outputDirectory)
                ?.Replace('\\', '/')
                .TrimEnd('/')
                ?? throw new ArgumentException(
                    "Capture output must have a parent root.",
                    nameof(outputDirectory));
            this.baselineDirectory = Path.GetFullPath(baselineDirectory)
                .Replace('\\', '/')
                .TrimEnd('/');
            this.recorderSettings = recorderSettings
                ?? throw new ArgumentNullException(nameof(recorderSettings));
        }

        public void Dispose()
        {
            recorderSettings.Dispose();
        }
    }

    /// <summary>
    /// Runs only inside a freshly opened Station PlayMode scene. It advances a
    /// real authored boss transition and a real CrushNet projectile; it never
    /// invokes PerfectDodgeTriggered directly and never saves the scene.
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public sealed class AuditionPvStationPhase2PerfectDodgeDirector : MonoBehaviour
    {
        private const double PhaseTwoPreparationTimeoutSeconds = 15d;
        private const float HealthTolerance = 0.001f;
        private readonly List<ProjectileLease> projectileLeases = new(8);
        private readonly List<BossBarrageProjectile> activeProjectiles = new(8);

        private OlympusStationAkazaPhase2FlowController flow;
        private BossBarrageEncounterController encounter;
        private CombatEncounterController canonicalEncounter;
        private BossBarrageEmitter emitter;
        private BossBarragePatternProfile crushNet;
        private BossBarragePatternProfile phaseTwoOpening;
        private CanvasGroup combatHud;
        private Canvas combatHudCanvas;
        private CombatHealth playerHealth;
        private PlayerMovementController playerMovement;
        private PlayerActionController playerAction;
        private PlayerRangedBasicAttackAction playerRangedBasic;
        private SummonEnergyLadder energyLadder;
        private Collider playerCollider;
        private ActionCameraController actionCamera;
        private ActionScreenCuePresenter actionScreen;
        private SceneEntryNoticeOverlay entryNotice;
        private OlympusCorridorTutorialDirector tutorial;
        private IDisposable cadenceSuspensionLease;
        private PresentationClock.ManualLease presentationClockLease;
        private BossBarrageProjectile impactProjectile;

        private UnityEngine.Random.State savedRandomState;
        private bool savedRandomStateValid;
        private bool savedHudValid;
        private float savedHudAlpha;
        private bool savedHudInteractable;
        private bool savedHudBlocksRaycasts;
        private bool savedEmitterFiring;
        private bool savedEncounterSuspended;
        private bool savedEntryNoticeEnabled;
        private bool savedTutorialEnabled;
        private BossBarragePatternProfile savedQueuedPriorityPattern;
        private int savedQueuedPriorityWaveCount;
        private int savedCaptureFramerate;
        private int savedTargetFrameRate;
        private float savedEnergyMana;
        private bool savedEnergyGainEnabled;
        private bool savedScreenDomainValid;
        private bool savedScreenDomainEnabled;
        private float savedScreenDomainAlpha;
        private float savedScreenDomainInvertAlpha;
        private float savedScreenDomainEdgeAlpha;
        private float savedScreenDomainGlitchAlpha;
        private int initialCameraMicroShakeCount;
        private float initialPlayerHealth;
        private int currentFrame = -1;
        private int perfectDodgeCount;
        private int firedProjectileCount;
        private int blockedDamageObservationCount;
        private int modifyingDamageObservationCount;
        private bool impactAppliedOrBlocked;
        private bool sawCameraCue;
        private bool sawScreenCue;
        private bool screenCueActiveAtBaselineFrame;
        private bool preparationSafetyExpiredBeforeDodge;
        private float dodgeStartedAtTime = float.NaN;
        private bool stateRestored;
        private bool restoringState;
        private bool restorableStateCaptured;
        private Vector3 preparedCameraPosition;
        private Quaternion preparedCameraRotation = Quaternion.identity;
        private float preparedCameraFieldOfView;

        public event Action<int> FramePresented;

        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public Exception Failure { get; private set; }
        public int CurrentFrame => currentFrame;
        public int PerfectDodgeCount => perfectDodgeCount;
        public int FiredProjectileCount => firedProjectileCount;
        public bool ImpactAppliedOrBlocked => impactAppliedOrBlocked;
        public bool ImpactProjectileInactive =>
            impactProjectile != null && !impactProjectile.IsActive;
        public bool PlayerHealthUnchanged => playerHealth != null
            && Mathf.Abs(playerHealth.CurrentHealth - initialPlayerHealth) <= HealthTolerance;
        public bool CameraCueRequested => sawCameraCue;
        public bool ScreenCueRequested => sawScreenCue;
        public bool ScreenCueActiveAtBaselineFrame =>
            screenCueActiveAtBaselineFrame;
        public bool StateRestored => stateRestored;
        public int DamageBlockedObservationCount => blockedDamageObservationCount;
        public int DamageModifyingObservationCount => modifyingDamageObservationCount;
        public bool PreparationSafetyExpiredBeforeDodge =>
            preparationSafetyExpiredBeforeDodge;
        public bool CaptureOnlyScreenProfileActive => actionScreen != null
            && actionScreen.PlayPerfectDodgeScreenDomain
            && Mathf.Abs(
                actionScreen.MaxPerfectDodgeDomainAlpha
                    - AuditionPvStationPhase2PerfectDodgeCapture
                        .CaptureOnlyScreenDomainAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.MaxPerfectDodgeInvertAlpha
                    - AuditionPvStationPhase2PerfectDodgeCapture
                        .CaptureOnlyScreenInvertAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.MaxPerfectDodgeEdgeAlpha
                    - AuditionPvStationPhase2PerfectDodgeCapture
                        .CaptureOnlyScreenEdgeAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.PerfectDodgeGlitchOverlayAlpha
                    - AuditionPvStationPhase2PerfectDodgeCapture
                        .CaptureOnlyScreenGlitchAlpha) <= HealthTolerance;
        public bool ScreenProfileRestored => stateRestored
            && savedScreenDomainValid
            && actionScreen != null
            && actionScreen.PlayPerfectDodgeScreenDomain == savedScreenDomainEnabled
            && Mathf.Abs(actionScreen.MaxPerfectDodgeDomainAlpha
                - savedScreenDomainAlpha) <= HealthTolerance
            && Mathf.Abs(actionScreen.MaxPerfectDodgeInvertAlpha
                - savedScreenDomainInvertAlpha) <= HealthTolerance
            && Mathf.Abs(actionScreen.MaxPerfectDodgeEdgeAlpha
                - savedScreenDomainEdgeAlpha) <= HealthTolerance
            && Mathf.Abs(actionScreen.PerfectDodgeGlitchOverlayAlpha
                - savedScreenDomainGlitchAlpha) <= HealthTolerance;
        public Vector3 PreparedCameraPosition => preparedCameraPosition;
        public Quaternion PreparedCameraRotation => preparedCameraRotation;
        public float PreparedCameraFieldOfView => preparedCameraFieldOfView;
        public bool UsedActualCrushNetPattern =>
            crushNet != null && emitter != null && firedProjectileCount == crushNet.ProjectilesPerWave;
        public bool IsExactHudRenderable => combatHud != null
            && combatHud == flow?.CombatHudCanvasGroup
            && combatHud.gameObject.activeInHierarchy
            && combatHudCanvas != null
            && combatHudCanvas.enabled
            && combatHudCanvas.gameObject.activeInHierarchy;
        public bool IsHudResourceStateExact => playerRangedBasic != null
            && playerRangedBasic.CurrentAmmo == playerRangedBasic.MagazineSize
            && !playerRangedBasic.IsReloading
            && energyLadder != null
            && energyLadder.IsCapped
            && energyLadder.AvailableTier == 3
            && Mathf.Abs(energyLadder.CurrentMana - energyLadder.MaxMana)
                <= HealthTolerance;
        public bool UsesExactEnergyLadderBinding => energyLadder != null
            && energyLadder == encounter?.EnergyLadder;
        public int HudAmmo => playerRangedBasic != null
            ? playerRangedBasic.CurrentAmmo
            : -1;
        public int HudMagazineSize => playerRangedBasic != null
            ? playerRangedBasic.MagazineSize
            : -1;
        public float HudEnergyMana => energyLadder != null
            ? energyLadder.CurrentMana
            : -1f;
        public float HudEnergyMaxMana => energyLadder != null
            ? energyLadder.MaxMana
            : -1f;

        public IEnumerator PrepareFreshProductState()
        {
            if (IsPrepared || IsRunning || stateRestored)
            {
                throw new InvalidOperationException(
                    "The G05 director cannot be prepared more than once.");
            }

            ValidateFreshScene();
            ResolveBindings();
            StabilizeEnergyRestoreBaseline();
            CaptureRestorableState();
            bool preparedSuccessfully = false;
            try
            {
                SuppressEntryAndTutorial();
                if (Time.timeScale <= 0f)
                {
                    throw new InvalidOperationException(
                        "G05 must not use a Time.timeScale freeze.");
                }

                savedRandomState = UnityEngine.Random.state;
                savedRandomStateValid = true;
                UnityEngine.Random.InitState(
                    AuditionPvStationPhase2PerfectDodgeCapture.DeterministicRandomSeed);
                PerfectDodgeScreenDomainRuntime.Clear();
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;

                ApplyActualThresholdDamage();
                double preparationDeadline = Time.realtimeSinceStartupAsDouble
                    + PhaseTwoPreparationTimeoutSeconds;
                while ((flow.CurrentPhase
                            != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                        || flow.TransitionCompletionCount != 1)
                    && Time.realtimeSinceStartupAsDouble < preparationDeadline)
                {
                    if (flow.CurrentPhase
                        == OlympusStationAkazaPhase2FlowController.Phase.Transitioning)
                    {
                        flow.TrySkipTransition();
                    }

                    yield return WaitForNextPlayerFrame();
                }

                ValidateCompletedPhaseTwoHandoff();
                AcquireDeterministicEncounterControl();
                presentationClockLease = PresentationClock.AcquireManual(
                    this,
                    AuditionPvCaptureContract.Fps);
                for (int settleFrame = 0;
                    settleFrame
                        < AuditionPvStationPhase2PerfectDodgeCapture.PhaseTwoSettleFrames;
                    settleFrame++)
                {
                    presentationClockLease.SetFrame(settleFrame);
                    // UnityTest/editor coroutine iterations can advance more
                    // than once inside a player loop. Waiting on Time.frameCount
                    // makes these exactly 18 real camera/UI/animation frames and
                    // remains valid in headless focused-test runs.
                    yield return WaitForNextPlayerFrame();
                }

                CapturePreparedCameraState();
                // The authored handoff uses realtime duration, so any incidental
                // VFX consumption before this point must not affect the source
                // interval's deterministic random stream.
                UnityEngine.Random.InitState(
                    AuditionPvStationPhase2PerfectDodgeCapture.DeterministicRandomSeed);
                StageDeterministicCrushNet();
                IsPrepared = true;
                preparedSuccessfully = true;
            }
            finally
            {
                if (!preparedSuccessfully)
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
            if (!IsPrepared || IsRunning || IsComplete || stateRestored)
            {
                throw new InvalidOperationException(
                    "Prepare the fresh G05 product state exactly once before beginning capture.");
            }

            if (Time.timeScale <= 0f)
            {
                throw new InvalidOperationException(
                    "G05 cannot begin while gameplay time is frozen.");
            }

            ValidateCanonicalEncounterReadyForShot();
            if (Mathf.Abs(playerHealth.CurrentHealth - playerHealth.MaxHealth)
                    > HealthTolerance
                || playerHealth.IsInvulnerable)
            {
                throw new InvalidOperationException(
                    "G05 must begin with full HP and no unrelated invulnerability.");
            }

            if (recorderOwnsCadence)
            {
                float minimumRecorderDelta =
                    1f / AuditionPvCaptureContract.Fps;
                if (Time.captureDeltaTime <= minimumRecorderDelta
                    || Time.captureDeltaTime >= minimumRecorderDelta + 0.001f)
                {
                    throw new InvalidOperationException(
                        "G05 Recorder cadence/padding is not active before logical f0: "
                        + $"captureDeltaTime={Time.captureDeltaTime:F9}.");
                }
            }
            else
            {
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
            }
            presentationClockLease ??= PresentationClock.AcquireManual(
                this,
                AuditionPvCaptureContract.Fps);
            presentationClockLease.SetFrame(
                AuditionPvStationPhase2PerfectDodgeCapture.FirstFrame);
            initialCameraMicroShakeCount = actionCamera.MicroShakeRequestCount;
            initialPlayerHealth = playerHealth.CurrentHealth;
            currentFrame = AuditionPvStationPhase2PerfectDodgeCapture.FirstFrame;
            IsRunning = true;
        }

        public void RestoreShotState()
        {
            if (stateRestored || restoringState)
            {
                return;
            }

            restoringState = true;
            IsRunning = false;
            Exception firstFailure = null;
            if (!restorableStateCaptured)
            {
                restoringState = false;
                stateRestored = true;
                return;
            }

            try
            {
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    presentationClockLease?.Dispose();
                    presentationClockLease = null;
                });
                CaptureRestoreFailure(
                    ref firstFailure,
                    PerfectDodgeScreenDomainRuntime.Clear);
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (savedScreenDomainValid && actionScreen != null)
                    {
                        actionScreen.ConfigurePerfectDodgeDomainPresentation(
                            savedScreenDomainEnabled,
                            savedScreenDomainAlpha,
                            savedScreenDomainInvertAlpha,
                            savedScreenDomainEdgeAlpha,
                            savedScreenDomainGlitchAlpha);
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (playerAction != null)
                    {
                        playerAction.PerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
                        playerAction.DodgeStarted -= HandleDodgeStarted;
                        playerAction.SetCinematicInputLocked(
                            PlayerInputLockSource.EditorVerification,
                            false);
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                    playerMovement?.SetCinematicMoveInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false));
                CaptureRestoreFailure(ref firstFailure, () =>
                    playerRangedBasic?.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false));
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (playerHealth != null)
                    {
                        playerHealth.DamageBlockedByInvulnerability -=
                            HandleDamageBlockedObservation;
                        playerHealth.DamageModifying -= HandleDamageModifyingObservation;
                    }
                });
                for (int index = 0; index < projectileLeases.Count; index++)
                {
                    ProjectileLease lease = projectileLeases[index];
                    CaptureRestoreFailure(
                        ref firstFailure,
                        lease.RestoreAndDeactivate);
                }

                projectileLeases.Clear();
                activeProjectiles.Clear();
                // Let the encounter restore its authored Phase 2 pacing first;
                // that public transition may toggle the emitter. Restore the
                // exact post-handoff emitter state and dormant opening queue only
                // after those pacing side effects have completed.
                CaptureRestoreFailure(ref firstFailure, () =>
                    encounter?.SetExternalCombatSuspended(savedEncounterSuspended));
                CaptureRestoreFailure(ref firstFailure, RestoreEmitterState);
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (energyLadder != null)
                    {
                        energyLadder.ResetLadder();
                        // ResetLadder restores its fallback multiplier. Re-sample
                        // the exact bound lane/player risk while gain is disabled
                        // so no mana is generated during cleanup.
                        energyLadder.SetGainEnabled(false);
                        energyLadder.Tick(
                            1f / AuditionPvCaptureContract.Fps);
                        energyLadder.GrantCurrentTierEnergy(savedEnergyMana);
                        energyLadder.SetGainEnabled(savedEnergyGainEnabled);
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    cadenceSuspensionLease?.Dispose();
                    cadenceSuspensionLease = null;
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (savedHudValid && combatHud != null)
                    {
                        combatHud.alpha = savedHudAlpha;
                        combatHud.interactable = savedHudInteractable;
                        combatHud.blocksRaycasts = savedHudBlocksRaycasts;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (entryNotice != null)
                    {
                        entryNotice.enabled = savedEntryNoticeEnabled;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (tutorial != null)
                    {
                        tutorial.enabled = savedTutorialEnabled;
                    }
                });
            }
            finally
            {
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (savedRandomStateValid)
                    {
                        UnityEngine.Random.state = savedRandomState;
                        savedRandomStateValid = false;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    Time.captureFramerate = savedCaptureFramerate;
                    Application.targetFrameRate = savedTargetFrameRate;
                });
                // Every restore step above is independently best-effort. Mark the
                // attempt complete only after the critical globals have run so a
                // later OnDisable/OnDestroy cannot repeat partially destructive
                // emitter/energy restoration when one step reported an error.
                stateRestored = true;
                restoringState = false;
            }

            if (firstFailure != null)
            {
                throw new InvalidOperationException(
                    "G05 shot-state restoration encountered an error.",
                    firstFailure);
            }

        }

        private void RestoreEmitterState()
        {
            if (emitter == null)
            {
                return;
            }

            // Always cross the enabled->disabled edge before restoring the
            // authored firing state. The edge is the public product path that
            // terminates a capture windup, clears its queued pattern, and
            // deactivates any projectile left by a failed shot.
            emitter.SetFiringEnabled(true);
            emitter.SetFiringEnabled(false);
            emitter.SetFiringEnabled(savedEmitterFiring);
            if (savedQueuedPriorityPattern != null
                && !emitter.QueuePriorityPatternForNextFiringWindow(
                    savedQueuedPriorityPattern,
                    Mathf.Max(1, savedQueuedPriorityWaveCount)))
            {
                throw new InvalidOperationException(
                    "G05 could not restore the authored queued priority pattern.");
            }
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

        private void Update()
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }

            try
            {
                // Registration callbacks from other Phase 2 systems may wake the
                // singleton after preparation. Reassert this capture lease before
                // its default-order Update so only the explicit f1/f71 calls can
                // advance the Flow emitter.
                if (!BossCombatCadenceScheduler.IsExternallySuspended)
                {
                    throw new InvalidOperationException(
                        "G05 lost exclusive control of the boss cadence scheduler.");
                }

                presentationClockLease.SetFrame(currentFrame);
                ExecuteEarlyFrameAction(currentFrame);
                StageProjectileTravel(currentFrame);
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
                if (currentFrame
                    == AuditionPvStationPhase2PerfectDodgeCapture.ImpactFrame)
                {
                    ApplyActualProjectileImpact();
                }

                ObserveCueState();
                if (currentFrame
                    == AuditionPvStationPhase2PerfectDodgeCapture.LastFrame)
                {
                    ValidateCompletedShot();
                    IsRunning = false;
                    IsComplete = true;
                }

                FramePresented?.Invoke(currentFrame);
                if (IsRunning)
                {
                    currentFrame++;
                }
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private void OnDisable()
        {
            TryRestoreFromUnityLifecycle();
        }

        private void OnDestroy()
        {
            TryRestoreFromUnityLifecycle();
        }

        private void TryRestoreFromUnityLifecycle()
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

        private void ValidateFreshScene()
        {
            Scene scene = gameObject.scene;
            if (!Application.isPlaying
                || !scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.path,
                    AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G05 requires a fresh OlympusStationCombatStage PlayMode scene.");
            }
        }

        private void ResolveBindings()
        {
            flow = UnityEngine.Object.FindFirstObjectByType<
                OlympusStationAkazaPhase2FlowController>(
                FindObjectsInactive.Include);
            if (flow == null)
            {
                throw new InvalidOperationException(
                    "The Station Phase 2 flow controller is missing.");
            }

            encounter = flow.EncounterController;
            emitter = flow.BarrageEmitter;
            combatHud = flow.CombatHudCanvasGroup;
            combatHudCanvas = combatHud != null
                ? combatHud.GetComponentInParent<Canvas>(includeInactive: true)
                : null;
            playerHealth = flow.PlayerHealth;
            playerMovement = flow.PlayerMovement;
            playerAction = flow.PlayerActionController;
            playerRangedBasic = flow.PlayerRangedBasicAttackAction;
            energyLadder = encounter != null ? encounter.EnergyLadder : null;
            crushNet = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                AuditionPvStationPhase2PerfectDodgeCapture.CrushNetProfilePath);
            phaseTwoOpening =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2PerfectDodgeCapture
                        .PhaseTwoOpeningProfilePath);
            actionCamera = UnityEngine.Object.FindFirstObjectByType<ActionCameraController>(
                FindObjectsInactive.Exclude);
            actionScreen = UnityEngine.Object.FindFirstObjectByType<ActionScreenCuePresenter>(
                FindObjectsInactive.Exclude);
            entryNotice = UnityEngine.Object.FindFirstObjectByType<SceneEntryNoticeOverlay>(
                FindObjectsInactive.Include);
            tutorial = UnityEngine.Object.FindFirstObjectByType<OlympusCorridorTutorialDirector>(
                FindObjectsInactive.Include);
            canonicalEncounter = UnityEngine.Object.FindObjectsByType<
                    CombatEncounterController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate =>
                    candidate.PlayerHealth == playerHealth
                    && candidate.EnemyHealth == flow.BossHealth);

            if (encounter == null
                || emitter == null
                || combatHud == null
                || combatHudCanvas == null
                || playerHealth == null
                || playerMovement == null
                || playerAction == null
                || playerRangedBasic == null
                || energyLadder == null
                || canonicalEncounter == null
                || crushNet == null
                || phaseTwoOpening == null
                || actionCamera == null
                || actionScreen == null)
            {
                throw new InvalidOperationException(
                    "G05 could not resolve its exact Flow, HUD, player, pattern, camera, or screen bindings.");
            }

            if (playerAction.gameObject != playerHealth.gameObject
                || !playerAction.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "The exact Flow PlayerActionController is not active on the exact Flow player health.");
            }

            Collider[] playerColliders = playerHealth.GetComponents<Collider>();
            playerCollider = playerColliders.FirstOrDefault(candidate =>
                candidate != null
                && candidate.enabled
                && candidate.gameObject.activeInHierarchy
                && SummonFrontlineProxy.ResolveFromCollider(candidate) == null
                && CombatHealth.ResolveFromCollider(candidate) == playerHealth);
            if (playerCollider == null)
            {
                throw new InvalidOperationException(
                    "G05 requires a real active collider owned by the Flow player health.");
            }
        }

        private void CaptureRestorableState()
        {
            savedHudValid = combatHud != null;
            if (savedHudValid)
            {
                savedHudAlpha = combatHud.alpha;
                savedHudInteractable = combatHud.interactable;
                savedHudBlocksRaycasts = combatHud.blocksRaycasts;
            }

            savedEmitterFiring = emitter.IsFiringEnabled;
            savedEncounterSuspended = encounter.IsExternalCombatSuspended;
            savedEntryNoticeEnabled = entryNotice != null && entryNotice.enabled;
            savedTutorialEnabled = tutorial != null && tutorial.enabled;
            savedCaptureFramerate = Time.captureFramerate;
            savedTargetFrameRate = Application.targetFrameRate;
            savedEnergyMana = energyLadder.CurrentMana;
            savedEnergyGainEnabled = energyLadder.CurrentEnergyPerSecond > 0f;
            if (!savedEnergyGainEnabled)
            {
                throw new InvalidOperationException(
                    "The fresh Station summon-energy ladder must begin with gain enabled.");
            }

            savedScreenDomainValid = actionScreen != null;
            if (savedScreenDomainValid)
            {
                savedScreenDomainEnabled = actionScreen.PlayPerfectDodgeScreenDomain;
                savedScreenDomainAlpha = actionScreen.MaxPerfectDodgeDomainAlpha;
                savedScreenDomainInvertAlpha = actionScreen.MaxPerfectDodgeInvertAlpha;
                savedScreenDomainEdgeAlpha = actionScreen.MaxPerfectDodgeEdgeAlpha;
                savedScreenDomainGlitchAlpha =
                    actionScreen.PerfectDodgeGlitchOverlayAlpha;
            }

            restorableStateCaptured = true;
        }

        private void StabilizeEnergyRestoreBaseline()
        {
            // A freshly enabled ladder starts with multiplier=1 until its first
            // scheduler tick. Sample the exact bound lane/player risk through the
            // public product path without generating mana, so both capture and
            // cleanup use a semantic post-tick baseline even when the runner
            // attaches during the first PlayMode update.
            bool gainWasEnabled = energyLadder.CurrentEnergyPerSecond > 0f;
            if (!gainWasEnabled || energyLadder.IsCapped)
            {
                throw new InvalidOperationException(
                    "The fresh Station summon-energy ladder must expose enabled, uncapped gain before stabilization.");
            }

            energyLadder.SetGainEnabled(false);
            try
            {
                energyLadder.Tick(1f / AuditionPvCaptureContract.Fps);
            }
            finally
            {
                energyLadder.SetGainEnabled(true);
            }
        }

        private void SuppressEntryAndTutorial()
        {
            if (entryNotice != null)
            {
                entryNotice.HideImmediate();
                entryNotice.enabled = false;
            }

            if (tutorial != null)
            {
                tutorial.CancelTutorial();
                tutorial.enabled = false;
            }
        }

        private void ApplyActualThresholdDamage()
        {
            if (flow.CurrentPhase
                != OlympusStationAkazaPhase2FlowController.Phase.Phase1
                || flow.TransitionStartCount != 0
                || flow.TransitionCompletionCount != 0)
            {
                throw new InvalidOperationException(
                    "G05 must start from a fresh Phase 1 Station boss state.");
            }

            CombatHealth bossHealth = flow.BossHealth;
            if (bossHealth == null || !bossHealth.IsAlive)
            {
                throw new InvalidOperationException(
                    "The fresh Station boss health is missing or terminal.");
            }

            float thresholdHealth = bossHealth.MaxHealth * flow.PhaseThreshold01;
            float thresholdDamage = Mathf.Max(
                1f,
                bossHealth.CurrentHealth - thresholdHealth);
            var damage = new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                thresholdDamage,
                bossHealth.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None);
            if (!bossHealth.TryApplyDamage(damage))
            {
                throw new InvalidOperationException(
                    "The real Station boss rejected deterministic threshold damage.");
            }

            if (flow.CurrentPhase
                != OlympusStationAkazaPhase2FlowController.Phase.Transitioning
                || flow.TransitionStartCount != 1)
            {
                throw new InvalidOperationException(
                    "Threshold damage did not start exactly one authored Phase 2 transition.");
            }
        }

        private void ValidateCompletedPhaseTwoHandoff()
        {
            if (flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                || !flow.PhaseTwoApplied
                || flow.TransitionStartCount != 1
                || flow.TransitionCompletionCount != 1)
            {
                throw new InvalidOperationException(
                    "G05 could not reach exactly one completed authored Phase 2 handoff: "
                    + $"phase={flow.CurrentPhase}, applied={flow.PhaseTwoApplied}, "
                    + $"starts={flow.TransitionStartCount}, completions={flow.TransitionCompletionCount}, "
                    + $"faultedOpen={flow.TransitionFaultedOpen}, elapsed={flow.TransitionElapsedSeconds:F4}.");
            }
        }

        private void AcquireDeterministicEncounterControl()
        {
            // The phase transition is irreversible for this product-state shot,
            // so emitter cleanup restores the exact authored post-handoff state,
            // not the obsolete Phase 1 value captured before threshold damage.
            savedEmitterFiring = emitter.IsFiringEnabled;

            // Capture the exact authored handoff reservation before suspending
            // the encounter. SetExternalCombatSuspended(true) disables the
            // emitter and may clear this queue on its enabled->disabled edge.
            if (!emitter.HasQueuedPriorityPattern
                || emitter.QueuedPriorityPattern != phaseTwoOpening
                || emitter.QueuedPriorityWavesRemaining != 1
                || emitter.IsWindupActive)
            {
                throw new InvalidOperationException(
                    "G05 expected the dormant authored Phase 2 opening before capture control: "
                    + $"queued={(emitter.QueuedPriorityPattern != null ? emitter.QueuedPriorityPattern.name : "none")}, "
                    + $"waves={emitter.QueuedPriorityWavesRemaining}, "
                    + $"windup={emitter.IsWindupActive}.");
            }

            savedQueuedPriorityPattern = emitter.QueuedPriorityPattern;
            savedQueuedPriorityWaveCount = emitter.QueuedPriorityWavesRemaining;

            // External suspension must own the encounter before this capture-only
            // manual firing window re-enables the exact Flow emitter. The fixed
            // settle interval runs while every cadence owner remains suspended.
            encounter.SetExternalCombatSuspended(true);
            cadenceSuspensionLease =
                BossCombatCadenceScheduler.AcquireExternalSuspension(this);
            if (!BossCombatCadenceScheduler.IsExternallySuspended
                || BossCombatCadenceScheduler.ExternalSuspensionCount != 1)
            {
                throw new InvalidOperationException(
                    "G05 could not acquire its owner-scoped cadence suspension lease.");
            }

            // If the emitter was already disabled, the encounter's same-state
            // disable path intentionally leaves the dormant queue in place.
            // Cancel that exact reservation before staging CrushNet.
            if (emitter.HasQueuedPriorityPattern)
            {
                if (emitter.QueuedPriorityPattern != savedQueuedPriorityPattern
                    || emitter.QueuedPriorityWavesRemaining
                        != savedQueuedPriorityWaveCount
                    || emitter.IsWindupActive
                    || !emitter.CancelQueuedPriorityPattern(
                        savedQueuedPriorityPattern))
                {
                    throw new InvalidOperationException(
                        "G05 could not cancel the dormant authored Phase 2 opening "
                        + "before reserving CrushNet.");
                }
            }

            emitter.SetFiringEnabled(false);
        }

        private void CapturePreparedCameraState()
        {
            Camera camera = actionCamera.GetComponent<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "G05 could not resolve the authored gameplay Camera component.");
            }

            preparedCameraPosition = camera.transform.position;
            preparedCameraRotation = camera.transform.rotation;
            preparedCameraFieldOfView = camera.fieldOfView;
            if (!IsFinite(preparedCameraPosition.x)
                || !IsFinite(preparedCameraPosition.y)
                || !IsFinite(preparedCameraPosition.z)
                || !IsFinite(preparedCameraRotation.x)
                || !IsFinite(preparedCameraRotation.y)
                || !IsFinite(preparedCameraRotation.z)
                || !IsFinite(preparedCameraRotation.w)
                || !IsFinite(preparedCameraFieldOfView)
                || preparedCameraFieldOfView <= 0f)
            {
                throw new InvalidOperationException(
                    "G05 fixed-settle gameplay camera state is invalid.");
            }
        }

        private void StageDeterministicCrushNet()
        {
            emitter.SetFiringEnabled(true);
            if (!emitter.QueuePriorityPattern(crushNet, 1))
            {
                throw new InvalidOperationException(
                    "The Flow barrage emitter rejected the authored CrushNet priority pattern: "
                    + $"firing={emitter.IsFiringEnabled}, windup={emitter.IsWindupActive}, "
                    + $"queued={emitter.HasQueuedPriorityPattern}, "
                    + $"queuedName={(emitter.QueuedPriorityPattern != null ? emitter.QueuedPriorityPattern.name : "none")}, "
                    + $"profile={(crushNet != null ? crushNet.name : "none")}.");
            }

            if (Mathf.Abs(playerHealth.CurrentHealth - playerHealth.MaxHealth) > HealthTolerance)
            {
                throw new InvalidOperationException(
                    "G05 requires the fresh real Flow player at full health.");
            }

            if (playerHealth.IsInvulnerable)
            {
                throw new InvalidOperationException(
                    "G05 cannot stage while an unrelated player invulnerability is active.");
            }
            if (playerRangedBasic.CurrentAmmo != playerRangedBasic.MagazineSize
                || playerRangedBasic.IsReloading)
            {
                throw new InvalidOperationException(
                    "G05 requires the fresh Flow ranged action at full magazine and not reloading.");
            }

            energyLadder.ResetLadder();
            energyLadder.GrantCurrentTierEnergy(energyLadder.MaxMana);
            energyLadder.SetGainEnabled(false);
            actionScreen.ConfigurePerfectDodgeDomainPresentation(
                true,
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CaptureOnlyScreenDomainAlpha,
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CaptureOnlyScreenInvertAlpha,
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CaptureOnlyScreenEdgeAlpha,
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CaptureOnlyScreenGlitchAlpha);
            combatHud.alpha = 1f;
            combatHud.interactable = true;
            combatHud.blocksRaycasts = true;
            if (!IsExactHudRenderable)
            {
                throw new InvalidOperationException(
                    "The exact Flow CombatHudCanvasGroup or its owning Canvas is inactive/disabled.");
            }

            if (!IsHudResourceStateExact)
            {
                throw new InvalidOperationException(
                    "G05 HUD resources are not full-ammo, idle-reload, and capped tier-3 energy.");
            }

            playerMovement.SetCinematicMoveInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerRangedBasic.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerAction.PerfectDodgeTriggered += HandlePerfectDodgeTriggered;
            playerAction.DodgeStarted += HandleDodgeStarted;
            playerHealth.DamageBlockedByInvulnerability +=
                HandleDamageBlockedObservation;
            playerHealth.DamageModifying += HandleDamageModifyingObservation;
        }

        private void ExecuteEarlyFrameAction(int frameIndex)
        {
            switch (frameIndex)
            {
                case AuditionPvStationPhase2PerfectDodgeCapture.BeginWindupFrame:
                    if (!emitter.BeginWindup()
                        || emitter.CurrentPattern != crushNet
                        || !emitter.IsWindupActive)
                    {
                        throw new InvalidOperationException(
                            "G05 f1 did not begin the authored CrushNet windup.");
                    }

                    break;

                case AuditionPvStationPhase2PerfectDodgeCapture.FirePendingWaveFrame:
                    FireActualCrushNetWave();
                    break;

                case AuditionPvStationPhase2PerfectDodgeCapture.QueueDodgeFrame - 1:
                    // The f188 block must be owned only by the upcoming dodge,
                    // never by a transition or preparation immunity.
                    preparationSafetyExpiredBeforeDodge =
                        !playerHealth.IsInvulnerable;
                    if (!preparationSafetyExpiredBeforeDodge)
                    {
                        throw new InvalidOperationException(
                            "G05 preparation invulnerability remained active at f185.");
                    }

                    // Release only this capture owner's action lock. Movement and
                    // ranged input remain locked; QueueDodge must traverse the real
                    // public action-input path on the following frame.
                    playerAction.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false);
                    break;

                case AuditionPvStationPhase2PerfectDodgeCapture.QueueDodgeFrame:
                    if (playerAction.IsCinematicInputLocked)
                    {
                        throw new InvalidOperationException(
                            "G05 f186 action path remained locked after releasing the capture owner.");
                    }

                    playerAction.QueueDodge();
                    break;

                case AuditionPvStationPhase2PerfectDodgeCapture.ImpactFrame + 1:
                    playerAction.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        true);
                    break;
            }
        }

        private void FireActualCrushNetWave()
        {
            firedProjectileCount = emitter.FirePendingWave();
            int copiedCount = emitter.CopyActiveProjectiles(activeProjectiles);
            if (firedProjectileCount != crushNet.ProjectilesPerWave
                || copiedCount != firedProjectileCount
                || copiedCount <= 0)
            {
                throw new InvalidOperationException(
                    $"G05 f71 expected {crushNet.ProjectilesPerWave} live CrushNet projectiles; "
                    + $"fired={firedProjectileCount}, copied={copiedCount}.");
            }

            int impactIndex = copiedCount / 2;
            for (int index = 0; index < activeProjectiles.Count; index++)
            {
                BossBarrageProjectile projectile = activeProjectiles[index];
                var lease = new ProjectileLease(projectile);
                lease.SuspendAutomaticSimulation();
                projectileLeases.Add(lease);
            }

            impactProjectile = activeProjectiles[impactIndex];
        }

        private void StageProjectileTravel(int frameIndex)
        {
            if (projectileLeases.Count == 0
                || frameIndex
                    < AuditionPvStationPhase2PerfectDodgeCapture.FirePendingWaveFrame)
            {
                return;
            }

            float travel01 = Mathf.InverseLerp(
                AuditionPvStationPhase2PerfectDodgeCapture.FirePendingWaveFrame,
                AuditionPvStationPhase2PerfectDodgeCapture.ImpactFrame,
                frameIndex);
            float eased = travel01 * travel01 * (3f - 2f * travel01);
            for (int index = 0; index < projectileLeases.Count; index++)
            {
                projectileLeases[index].SampleTravel(eased);
            }

            Physics.SyncTransforms();
        }

        private void ApplyActualProjectileImpact()
        {
            if (impactProjectile == null || !impactProjectile.IsActive)
            {
                throw new InvalidOperationException(
                    "G05 f188 does not own the real active CrushNet projectile selected at f71.");
            }

            float healthBeforeImpact = playerHealth.CurrentHealth;
            bool wasDodgingAtImpact = playerAction.IsDodging;
            bool wasInvulnerableAtImpact = playerHealth.IsInvulnerable;
            bool hostileImpact = CombatTeamUtility.AreHostile(
                impactProjectile.SourceTeam,
                playerHealth.Team);
            float dodgeElapsedSeconds = IsFinite(dodgeStartedAtTime)
                ? Time.time - dodgeStartedAtTime
                : float.NaN;
            impactProjectile.transform.position = playerCollider.bounds.center;
            Physics.SyncTransforms();
            impactAppliedOrBlocked = impactProjectile.TryApplyImpact(
                playerCollider,
                playerCollider.bounds.center);

            if (!impactAppliedOrBlocked
                || impactProjectile.IsActive
                || perfectDodgeCount != 1
                || blockedDamageObservationCount != 1
                || modifyingDamageObservationCount != 0
                || Mathf.Abs(playerHealth.CurrentHealth - healthBeforeImpact) > HealthTolerance)
            {
                throw new InvalidOperationException(
                    "G05 f188 failed the real projectile, exactly-one perfect-dodge, "
                    + "inactive-projectile, or unchanged-HP contract: "
                    + $"appliedOrBlocked={impactAppliedOrBlocked}, "
                    + $"projectileActive={impactProjectile.IsActive}, "
                    + $"wasDodging={wasDodgingAtImpact}, "
                    + $"wasInvulnerable={wasInvulnerableAtImpact}, "
                    + $"dodgeElapsed={dodgeElapsedSeconds:F6}, "
                    + $"sourceTeam={impactProjectile.SourceTeam}, "
                    + $"playerTeam={playerHealth.Team}, hostile={hostileImpact}, "
                    + $"actionActive={playerAction.isActiveAndEnabled}, "
                    + $"blockedObserved={blockedDamageObservationCount}, "
                    + $"modifyingObserved={modifyingDamageObservationCount}, "
                    + $"impactResult={impactProjectile.LastImpactResult}, "
                    + $"impactHealth={(impactProjectile.LastImpactTargetHealth != null ? impactProjectile.LastImpactTargetHealth.name : "none")}, "
                    + $"impactSameHealth={ReferenceEquals(impactProjectile.LastImpactTargetHealth, playerHealth)}, "
                    + $"impactProxy={(impactProjectile.LastImpactTargetProxy != null ? impactProjectile.LastImpactTargetProxy.name : "none")}, "
                    + $"canonicalRunning={canonicalEncounter.IsRunning}, "
                    + $"canonicalFaulted={canonicalEncounter.IsFaulted}, "
                    + $"coordinatorState={(canonicalEncounter.TerminalCoordinator != null ? canonicalEncounter.TerminalCoordinator.State.ToString() : "none")}, "
                    + $"diagnostic={(canonicalEncounter.HasDiagnostic ? canonicalEncounter.Diagnostic.Reason + ":" + canonicalEncounter.Diagnostic.Message : "none")}, "
                    + $"perfectDodges={perfectDodgeCount}, "
                    + $"health={healthBeforeImpact:F3}->{playerHealth.CurrentHealth:F3}, "
                    + $"actionLocked={playerAction.IsCinematicInputLocked}.");
            }
        }

        private void ObserveCueState()
        {
            sawCameraCue |= actionCamera.MicroShakeRequestCount
                > initialCameraMicroShakeCount;
            sawScreenCue |= PerfectDodgeScreenDomainRuntime.HasActiveCue;
            if (currentFrame
                == AuditionPvStationPhase2PerfectDodgeCapture.Bl06SourceFrame)
            {
                screenCueActiveAtBaselineFrame =
                    PerfectDodgeScreenDomainRuntime.HasActiveCue;
            }
        }

        private void ValidateCompletedShot()
        {
            if (currentFrame
                    != AuditionPvStationPhase2PerfectDodgeCapture.LastFrame
                || perfectDodgeCount != 1
                || !impactAppliedOrBlocked
                || !preparationSafetyExpiredBeforeDodge
                || impactProjectile == null
                || impactProjectile.IsActive
                || !PlayerHealthUnchanged
                || actionCamera.MicroShakeRequestCount
                    < initialCameraMicroShakeCount + 2
                || !sawCameraCue
                || !sawScreenCue
                || !screenCueActiveAtBaselineFrame
                || !CaptureOnlyScreenProfileActive
                || combatHud.alpha != 1f
                || !combatHud.interactable
                || !combatHud.blocksRaycasts
                || !IsExactHudRenderable
                || !IsHudResourceStateExact
                || flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                || flow.TransitionCompletionCount != 1)
            {
                throw new InvalidOperationException(
                    "G05 final validation failed: frame interval, real dodge, HP, "
                    + "camera/screen cue, HUD, or Phase 2 state drifted: "
                    + $"frame={currentFrame}, dodge={perfectDodgeCount}, "
                    + $"impact={impactAppliedOrBlocked}, active={(impactProjectile != null && impactProjectile.IsActive)}, "
                    + $"hpUnchanged={PlayerHealthUnchanged}, cameraDelta={actionCamera.MicroShakeRequestCount - initialCameraMicroShakeCount}, "
                    + $"cameraCue={sawCameraCue}, screenCue={sawScreenCue}, "
                    + $"screenAtBL06={screenCueActiveAtBaselineFrame}, "
                    + $"screenEnabled={actionScreen.PlayPerfectDodgeScreenDomain}, "
                    + $"hud={IsExactHudRenderable}, resources={IsHudResourceStateExact}, "
                    + $"phase={flow.CurrentPhase}, completions={flow.TransitionCompletionCount}.");
            }
        }

        private void ValidateCanonicalEncounterReadyForShot()
        {
            EncounterTerminalResolutionCoordinator coordinator =
                canonicalEncounter != null
                    ? canonicalEncounter.TerminalCoordinator
                    : null;
            if (canonicalEncounter == null
                || !canonicalEncounter.IsRunning
                || canonicalEncounter.IsFaulted
                || coordinator == null
                || coordinator.State != EncounterTerminalCoordinatorState.Idle)
            {
                throw new InvalidOperationException(
                    "G05 canonical damage authority is not ready before the shot: "
                    + $"running={(canonicalEncounter != null && canonicalEncounter.IsRunning)}, "
                    + $"faulted={(canonicalEncounter != null && canonicalEncounter.IsFaulted)}, "
                    + $"state={(coordinator != null ? coordinator.State.ToString() : "none")}, "
                    + $"diagnostic={(canonicalEncounter != null && canonicalEncounter.HasDiagnostic ? canonicalEncounter.Diagnostic.Reason + ":" + canonicalEncounter.Diagnostic.Message : "none")}.");
            }
        }

        private void HandlePerfectDodgeTriggered(DamageInfo damageInfo)
        {
            perfectDodgeCount++;
        }

        private void HandleDodgeStarted()
        {
            dodgeStartedAtTime = Time.time;
        }

        private void HandleDamageBlockedObservation(DamageInfo damageInfo)
        {
            blockedDamageObservationCount++;
        }

        private void HandleDamageModifyingObservation(
            DamageModificationContext context)
        {
            modifyingDamageObservationCount++;
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

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static WaitUntil WaitForNextPlayerFrame()
        {
            int currentPlayerFrame = Time.frameCount;
            return new WaitUntil(() => Time.frameCount > currentPlayerFrame);
        }

        private sealed class ProjectileLease
        {
            private readonly BossBarrageProjectile projectile;
            private readonly bool behaviourEnabled;
            private readonly Collider[] colliders;
            private readonly bool[] colliderEnabled;
            private readonly Vector3 startPosition;
            private readonly Vector3 targetPosition;

            public ProjectileLease(BossBarrageProjectile projectile)
            {
                this.projectile = projectile
                    ?? throw new ArgumentNullException(nameof(projectile));
                behaviourEnabled = projectile.enabled;
                colliders = projectile.GetComponentsInChildren<Collider>(
                    includeInactive: true);
                colliderEnabled = new bool[colliders.Length];
                for (int index = 0; index < colliders.Length; index++)
                {
                    colliderEnabled[index] = colliders[index] != null
                        && colliders[index].enabled;
                }

                startPosition = projectile.transform.position;
                targetPosition = projectile.LastConfiguredTargetPosition;
            }

            public void SuspendAutomaticSimulation()
            {
                projectile.enabled = false;
                for (int index = 0; index < colliders.Length; index++)
                {
                    if (colliders[index] != null)
                    {
                        colliders[index].enabled = false;
                    }
                }
            }

            public void SampleTravel(float progress01)
            {
                if (projectile != null && projectile.IsActive)
                {
                    projectile.transform.position = Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        Mathf.Clamp01(progress01));
                }
            }

            public void RestoreAndDeactivate()
            {
                if (projectile == null)
                {
                    return;
                }

                projectile.enabled = behaviourEnabled;
                for (int index = 0; index < colliders.Length; index++)
                {
                    if (colliders[index] != null)
                    {
                        colliders[index].enabled = colliderEnabled[index];
                    }
                }

                projectile.Deactivate();
            }
        }

    }
}
