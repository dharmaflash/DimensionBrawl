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
    /// Contract and product-state director for the independent G07 Station
    /// pattern-relay source. The source intentionally advances the authored
    /// barrage emitter through Tick only: the opening priority Curtain is
    /// followed by sequence-index-zero Hover Lance without capture profiles,
    /// projectile replacement, or manual fire calls.
    /// </summary>
    public static class AuditionPvStationPhase2PatternRelayCapture
    {
        internal const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        internal const string CurtainProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset";
        internal const string HoverProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaHoverLance.asset";
        internal const string SpiralProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSpiralVolley.asset";
        internal const string CrushNetProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaCrushNet.asset";
        internal const string PressureActionDeckPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossPressureActionDeck_AkazaPhase2.asset";
        internal const string PhaseTwoProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_AkazaPhase2.prefab";
        internal const string CombatVfxProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";
        internal const string GameplayPostProcessPath =
            "Assets/_Game/Art/Environment/OlympusCorridor/Profiles/DB_OlympusCorridor_PostProcess.asset";
        internal const string NoCrossWallPrefabPath =
            "Assets/_Game/Prefabs/VFX/Environment/PF_OlympusStation_NoCrossRedCubeZone.prefab";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhase2PatternRelayCapture.cs";
        internal const string FlowControllerPath =
            "Assets/_Game/Scripts/LevelDesign/OlympusStationAkazaPhase2FlowController.cs";
        internal const string LaneSpacePath =
            "Assets/_Game/Scripts/LevelDesign/SummonLaneSpace.cs";
        internal const string EmitterPath =
            "Assets/_Game/Scripts/Combat/BossBarrageEmitter.cs";
        internal const string PatternProfilePath =
            "Assets/_Game/Scripts/Combat/BossBarragePatternProfile.cs";
        internal const string ProjectilePath =
            "Assets/_Game/Scripts/Combat/BossBarrageProjectile.cs";
        internal const string CadenceSchedulerPath =
            "Assets/_Game/Scripts/Combat/BossCombatCadenceScheduler.cs";
        internal const string TimeDilationReceiverPath =
            "Assets/_Game/Scripts/Combat/CombatTimeDilationReceiver.cs";
        internal const string EncounterPath =
            "Assets/_Game/Scripts/Combat/BossBarrageEncounterController.cs";
        internal const string BasicFirePath =
            "Assets/_Game/Scripts/Combat/BossBasicFireEmitter.cs";
        internal const string PressureActionPath =
            "Assets/_Game/Scripts/Combat/BossPressureActionDirector.cs";
        internal const string EnemySummonPacingPath =
            "Assets/_Game/Scripts/Combat/EnemySummonPacingDirector.cs";
        internal const string CombatHealthPath =
            "Assets/_Game/Scripts/Combat/CombatHealth.cs";
        internal const string PlayerMovementPath =
            "Assets/_Game/Scripts/Player/PlayerMovementController.cs";
        internal const string PlayerActionPath =
            "Assets/_Game/Scripts/Player/PlayerActionController.cs";
        internal const string PlayerRangedPath =
            "Assets/_Game/Scripts/Player/PlayerRangedBasicAttackAction.cs";
        internal const string PlayerSummonPath =
            "Assets/_Game/Scripts/Player/PlayerSummonSlot1Action.cs";
        internal const string PresentationClockPath =
            "Assets/_Game/Scripts/Presentation/PresentationClock.cs";
        internal const string VisualCueDriverPath =
            "Assets/_Game/Scripts/Presentation/BossBarrageVisualCueDriver.cs";
        internal const string TelegraphPresenterPath =
            "Assets/_Game/Scripts/Presentation/BossBarrageLaneTelegraphPresenter.cs";
        internal const string CameraCueDriverPath =
            "Assets/_Game/Scripts/Presentation/BossBarrageCameraCueDriver.cs";
        internal const string ActionCameraPath =
            "Assets/_Game/Scripts/Presentation/ActionCameraController.cs";
        internal const string MotionDriverPath =
            "Assets/_Game/Scripts/Presentation/AkazaPhase2CombatMotionDriver.cs";
        internal const string HudBinderPath =
            "Assets/_Game/UI/CombatHud/BossBarrageLaneReviewCombatHudBinder.cs";

        internal const string ShotId = "g07";
        internal const string BaselinesFolderName = "baselines";
        internal const string Bl08FileName =
            "BL08_AKAZA_PHASE2_SUMMON_CURTAIN__HUDON__t01.133333.png";
        internal const string Bl09FileName =
            "BL09_AKAZA_PHASE2_HOVER_LANCE__HUDON__t06.966667.png";
        internal const int FirstFrame = 0;
        internal const int LastFrame = 419;
        internal const int ExpectedFrameCount = 420;
        internal const int CurtainWindupFrame = 10;
        internal const int CurtainFireFrame = 68;
        internal const int HoverWindupFrame = 368;
        internal const int HoverFireFrame = 418;
        internal const int CurtainMoveFirstFrame = 17;
        internal const int CurtainMoveLastFrame = 46;
        internal const int CurtainStopFrame = 47;
        internal const int HoverMoveFirstFrame = 374;
        internal const int HoverMoveLastFrame = 406;
        internal const int HoverStopFrame = 407;
        internal const int PhaseTwoSettleFrames = 90;
        internal const int PostRecordingSettleFrameBudget = 240;
        internal const int CurtainProjectileCount = 7;
        internal const int HoverProjectileCount = 4;
        internal const float MinimumCurtainRiskDecrease = 0.12f;
        internal const float MinimumHoverLateralDisplacement = 1.5f;
        internal const float MinimumHoverDirectionDot = 0.98f;
        internal const int DeterministicRandomSeed = 0x4707;

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
                    "Fresh Station threshold->public skip->Phase2 transition, then 90 unrecorded fixed-60Hz settle frames. "
                    + "The real emitter Tick path produces priority AkazaSummonCurtain windup f10/fire f68 (7), "
                    + "then sequence-index-0 AkazaHoverLance windup f368/fire f418 (4). "
                    + "Player response uses PlayerMovementController.SetMoveInput only: lane-back f17..f46, stop f47; "
                    + "opposite-lateral preview answer f374..f406, stop f407. No summon screen, intercept, damage, "
                    + "capture profile, manual windup/fire, camera/VFX/material override, or transform staging during recording. "
                    + "Logical f419 is the final safely framed hero composition. 2560x1440 PNG at 60fps; "
                    + "BL08=f68 and BL09=f418 are byte-exact event-frame copies."
            };
        }

        internal static AuditionPvBaselineManifestEntry[] CreateBaselineManifestEntries()
        {
            return new[]
            {
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl08",
                    shotId = ShotId,
                    sourceFrame = CurtainFireFrame,
                    fileName = Bl08FileName,
                    hudMode = "hud-on",
                    status = "captured"
                },
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl09",
                    shotId = ShotId,
                    sourceFrame = HoverFireFrame,
                    fileName = Bl09FileName,
                    hudMode = "hud-on",
                    status = "captured"
                }
            };
        }

        internal static string FrameFileName(int frameIndex)
        {
            if (frameIndex < FirstFrame || frameIndex > LastFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            }

            return $"frame_{frameIndex:0000}.png";
        }

        internal static float FrameTimeSeconds(int frameIndex)
        {
            if (frameIndex < FirstFrame || frameIndex > LastFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            }

            return frameIndex / (float)AuditionPvCaptureContract.Fps;
        }

        internal static PatternSchedule DeriveFloat32Schedule()
        {
            const float Delta = 1f / 60f;
            float cooldown = 0.18f;
            float windup = 0f;
            bool winding = false;
            bool curtain = true;
            var result = new PatternSchedule
            {
                curtainWindupFrame = -1,
                curtainFireFrame = -1,
                hoverWindupFrame = -1,
                hoverFireFrame = -1
            };

            for (int frame = FirstFrame; frame <= LastFrame; frame++)
            {
                if (winding)
                {
                    windup -= Delta;
                    if (windup > 0f)
                    {
                        continue;
                    }

                    winding = false;
                    if (curtain)
                    {
                        result.curtainFireFrame = frame;
                        curtain = false;
                        cooldown = 5f;
                    }
                    else
                    {
                        result.hoverFireFrame = frame;
                        break;
                    }

                    continue;
                }

                cooldown -= Delta;
                if (cooldown > 0f)
                {
                    continue;
                }

                winding = true;
                if (curtain)
                {
                    result.curtainWindupFrame = frame;
                    windup = 0.96f;
                }
                else
                {
                    result.hoverWindupFrame = frame;
                    windup = 0.82f;
                }
            }

            return result;
        }

        internal static string[] ExplicitProductDependencyPaths()
        {
            return new[]
            {
                StationScenePath,
                CurtainProfilePath,
                HoverProfilePath,
                SpiralProfilePath,
                CrushNetProfilePath,
                PressureActionDeckPath,
                PhaseTwoProjectilePrefabPath,
                CombatVfxProfilePath,
                GameplayPostProcessPath,
                NoCrossWallPrefabPath,
                CaptureScriptPath,
                FlowControllerPath,
                LaneSpacePath,
                EmitterPath,
                PatternProfilePath,
                ProjectilePath,
                CadenceSchedulerPath,
                TimeDilationReceiverPath,
                EncounterPath,
                BasicFirePath,
                PressureActionPath,
                EnemySummonPacingPath,
                CombatHealthPath,
                PlayerMovementPath,
                PlayerActionPath,
                PlayerRangedPath,
                PlayerSummonPath,
                PresentationClockPath,
                VisualCueDriverPath,
                TelegraphPresenterPath,
                CameraCueDriverPath,
                ActionCameraPath,
                MotionDriverPath,
                HudBinderPath
            };
        }

        internal static AuditionPvStationPhase2PatternRelayOutput ReserveNewOutput(
            DateTime startedAtUtc,
            AuditionPvGitSnapshot gitSnapshot = null)
        {
            AuditionPvGitSnapshot git = gitSnapshot
                ?? AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "G07 output reservation requires a successful Git provenance probe: "
                    + git.probeError);
            }

            string outputId = AuditionPvOutputPaths.CreateOutputId(
                "g07-station-phase2-pattern-relay",
                startedAtUtc,
                git.commitSha,
                git.isDirty,
                git.dirtyStateHashSha256);
            return ReserveNewOutputForRoot(AuditionPvCaptureContract.OutputRoot, outputId);
        }

        internal static AuditionPvStationPhase2PatternRelayOutput ReserveNewOutputForRoot(
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
                return new AuditionPvStationPhase2PatternRelayOutput(
                    new DirectoryInfo(outputDirectory).Name,
                    outputDirectory,
                    baselineDirectory,
                    recorderSettings);
            }
            catch (Exception reservationFailure)
            {
                Exception cleanupFailure = null;
                try
                {
                    recorderSettings?.Dispose();
                }
                catch (Exception exception)
                {
                    cleanupFailure = exception;
                }

                try
                {
                    CleanupFailedReservationForRoot(
                        outputRoot,
                        outputId,
                        outputDirectory);
                }
                catch (Exception exception)
                {
                    cleanupFailure = cleanupFailure == null
                        ? exception
                        : new AggregateException(cleanupFailure, exception);
                }

                if (cleanupFailure != null)
                {
                    throw new AggregateException(reservationFailure, cleanupFailure);
                }

                throw;
            }
        }

        internal static void CleanupFailedReservationForRoot(
            string outputRoot,
            string outputId,
            string outputDirectory)
        {
            string root = Path.GetFullPath(outputRoot).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string expected = Path.GetFullPath(
                    AuditionPvOutputPaths.ResolveOutputDirectory(root, outputId))
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            string actual = Path.GetFullPath(outputDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(
                    Path.GetDirectoryName(actual),
                    root,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Refused to remove a failed G07 reservation outside its exact output root child.");
            }

            if (Directory.Exists(actual))
            {
                Directory.Delete(actual, recursive: true);
            }
        }

        internal static AuditionPvStationPhase2PatternRelayDirector AttachToFreshActiveScene()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "The G07 product-state director can only run in Play Mode.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(scene.path, StationScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G07 requires a fresh OlympusStationCombatStage PlayMode scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<
                    AuditionPvStationPhase2PatternRelayDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The active scene already owns a G07 pattern-relay director.");
            }

            var root = new GameObject("[AuditionPV_G07_PatternRelayDirector]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            return root.AddComponent<AuditionPvStationPhase2PatternRelayDirector>();
        }

        internal static void ReopenProductSceneAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before reopening the G07 product scene.");
            }

            EditorSceneManager.OpenScene(StationScenePath, OpenSceneMode.Single);
        }

        [Serializable]
        internal struct PatternSchedule
        {
            public int curtainWindupFrame;
            public int curtainFireFrame;
            public int hoverWindupFrame;
            public int hoverFireFrame;
        }
    }

    internal sealed class AuditionPvStationPhase2PatternRelayOutput : IDisposable
    {
        public readonly string captureId;
        public readonly string outputRoot;
        public readonly string outputDirectory;
        public readonly string baselineDirectory;
        public readonly AuditionPvRecorderSettingsBundle recorderSettings;

        internal AuditionPvStationPhase2PatternRelayOutput(
            string captureId,
            string outputDirectory,
            string baselineDirectory,
            AuditionPvRecorderSettingsBundle recorderSettings)
        {
            this.captureId = captureId;
            this.outputDirectory = Path.GetFullPath(outputDirectory)
                .Replace('\\', '/')
                .TrimEnd('/');
            outputRoot = Path.GetDirectoryName(this.outputDirectory)
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

    [DefaultExecutionOrder(-32000)]
    public sealed class AuditionPvStationPhase2PatternRelayDirector : MonoBehaviour
    {
        private const double PhaseTwoPreparationTimeoutSeconds = 15d;
        private const double PostRecordingSettleTimeoutSeconds =
            AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget
            / (double)AuditionPvCaptureContract.Fps;
        private const float Tolerance = 0.001f;
        private readonly Vector2[] hoverPreview = new Vector2[16];

        private OlympusStationAkazaPhase2FlowController flow;
        private BossBarrageEncounterController encounter;
        private BossBarrageEmitter emitter;
        private BossBasicFireEmitter basicFire;
        private BossPressureActionDirector pressureAction;
        private BossPressurePositionController pressurePosition;
        private EnemySummonPacingDirector enemySummonPacing;
        private BossBarragePatternProfile curtain;
        private BossBarragePatternProfile hover;
        private BossBarrageProjectile expectedPhaseTwoProjectilePrefab;
        private CombatVfxCueProfile expectedCombatVfxProfile;
        private BossBarrageVisualCueDriver visualCue;
        private BossBarrageLaneTelegraphPresenter telegraph;
        private BossBarrageCameraCueDriver cameraCue;
        private AkazaPhase2CombatMotionDriver motion;
        private ActionCameraController actionCamera;
        private Camera gameplayCamera;
        private CombatHealth playerHealth;
        private CombatHealth bossHealth;
        private PlayerMovementController movement;
        private PlayerActionController playerAction;
        private PlayerRangedBasicAttackAction ranged;
        private PlayerSummonSlot1Action summon;
        private SummonEnergyLadder energy;
        private SummonLaneSpace lane;
        private CanvasGroup hud;
        private Canvas hudCanvas;
        private BossBarrageLaneReviewCombatHudBinder hudBinder;
        private PresentationClock.ManualLease presentationClockLease;
        private IDisposable cadenceSuspensionLease;
        private BossBarragePatternProfile[] authoredPatternSequence =
            Array.Empty<BossBarragePatternProfile>();
        private Transform[] authoredSpawnOrigins = Array.Empty<Transform>();
        private int authoredWavesPerPattern;

        private UnityEngine.Random.State savedRandom;
        private string savedRandomJson = string.Empty;
        private bool savedRandomValid;
        private bool randomStateRestored;
        private int savedCaptureFramerate;
        private int savedTargetFrameRate;
        private float savedFixedDeltaTime;
        private float savedTimeScale;
        private bool savedEncounterSuspended;
        private bool savedEmitterFiring;
        private bool savedPressureMovementEnabled;
        private bool savedLaneConstraintEnabled;
        private bool savedHudValid;
        private float savedHudAlpha;
        private bool savedHudInteractable;
        private bool savedHudBlocksRaycasts;
        private bool savedEntryEnabled;
        private bool savedTutorialEnabled;
        private bool savedVisualCueEnabled;
        private bool savedTelegraphEnabled;
        private bool savedCameraCueEnabled;
        private bool savedMotionEnabled;
        private SceneEntryNoticeOverlay entryNotice;
        private OlympusCorridorTutorialDirector tutorial;
        private Vector3 savedPlayerPosition;
        private Quaternion savedPlayerRotation = Quaternion.identity;
        private Vector3 savedPlayerScale = Vector3.one;
        private Vector3 savedBossPosition;
        private Quaternion savedBossRotation = Quaternion.identity;
        private Vector3 savedBossScale = Vector3.one;
        private Vector3 savedCameraPosition;
        private Quaternion savedCameraRotation = Quaternion.identity;
        private float savedCameraFieldOfView;
        private int initialAmmo;
        private float initialEnergy;
        private float initialPlayerHealth;
        private float initialBossHealth;
        private int initialEnemySummonReleaseCount;
        private int initialVisualWindupCount;
        private int initialVisualReleaseCount;
        private int initialTelegraphWindupCount;
        private int initialTelegraphReleaseCount;
        private int initialCameraWindupCount;
        private int initialCameraFireCount;
        private int initialMotionReleaseCount;
        private int currentFrame = -1;
        private int nextExpectedEventIndex;
        private int transitionCompletedEventCount;
        private int windupEventCount;
        private int waveEventCount;
        private int curtainWindupFrame = -1;
        private int curtainFireFrame = -1;
        private int curtainSpawnedCount;
        private bool curtainWasPriority;
        private int hoverWindupFrame = -1;
        private int hoverFireFrame = -1;
        private int hoverSpawnedCount;
        private bool hoverWasPriority;
        private int hoverSequenceIndexAfterFire = -1;
        private int emitterTickCount;
        private float minimumEmitterTimeScale = float.PositiveInfinity;
        private float maximumEmitterTimeScale = float.NegativeInfinity;
        private int runStartedCount;
        private int stopSettleCount;
        private int curtainMoveFirstAppliedFrame = -1;
        private int curtainMoveLastAppliedFrame = -1;
        private int curtainZeroAppliedFrame = -1;
        private int hoverMoveFirstAppliedFrame = -1;
        private int hoverMoveLastAppliedFrame = -1;
        private int hoverZeroAppliedFrame = -1;
        private int basicVolleyEventCount;
        private int pressureActionEventCount;
        private int playerDamageEventCount;
        private int bossDamageEventCount;
        private int playerBasicStartedCount;
        private int playerBasicHitCount;
        private int dodgeStartedCount;
        private int dodgeEndedCount;
        private int perfectDodgeCount;
        private int summonUsedCount;
        private int summonBlockedCount;
        private int summonUseBlockedCount;
        private float curtainRiskBefore = -1f;
        private float curtainRiskAfter = -1f;
        private bool stayedInsideForwardBoundary = true;
        private int hoverPreviewCount;
        private float hoverPreviewAverageLateral;
        private Vector3 expectedHoverWorldDirection;
        private Vector3 hoverMoveStart;
        private Vector3 hoverMoveEnd;
        private float hoverLateralDisplacement;
        private float hoverDirectionDot = -1f;
        private string curtainWindupPatternId = string.Empty;
        private string curtainFirePatternId = string.Empty;
        private string hoverWindupPatternId = string.Empty;
        private string hoverFirePatternId = string.Empty;
        private bool eventSubscriptionsActive;
        private bool restorableStateCaptured;
        private bool globalStateCaptured;
        private bool handoffStateCaptured;
        private bool preparedStateCaptured;
        private bool restoring;
        private bool stateRestored;
        private bool lifecycleEmergencyResetUsed;
        private Exception cleanupFailure;
        private int postRecordingSettleFrames;
        private float postRecordingSettleSeconds;
        private int curtainWindupVisibleMarkerCount;
        private int curtainFireVisibleMarkerCount;
        private int hoverWindupVisibleMarkerCount;
        private int hoverFireVisibleMarkerCount;
        private int curtainWindupVisibleRendererCount;
        private int curtainFireVisibleRendererCount;
        private int hoverWindupVisibleRendererCount;
        private int hoverFireVisibleRendererCount;
        private bool telegraphMarkerCollidersNonBlocking = true;
        private Color curtainWindupMarkerColor;
        private Color curtainFireMarkerColor;
        private Color hoverWindupMarkerColor;
        private Color hoverFireMarkerColor;

        public event Action<int> FramePresented;

        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public Exception Failure { get; private set; }
        public Exception CleanupFailure => cleanupFailure;
        public bool LifecycleEmergencyResetUsed => lifecycleEmergencyResetUsed;
        public int CurrentFrame => currentFrame;
        public int LastPresentedFrame { get; private set; } = -1;
        public Camera GameplayCamera => gameplayCamera;
        public Transform PlayerRendererRoot => movement != null
            ? movement.transform
            : null;
        public Transform BossRendererRoot => pressurePosition != null
            ? pressurePosition.MovedTransform
            : null;
        public BossBarrageLaneTelegraphPresenter TelegraphPresenter => telegraph;
        public int TransitionCompletedEventCount => transitionCompletedEventCount;
        public int WindupEventCount => windupEventCount;
        public int WaveEventCount => waveEventCount;
        public int CurtainWindupFrame => curtainWindupFrame;
        public int CurtainFireFrame => curtainFireFrame;
        public int CurtainSpawnedCount => curtainSpawnedCount;
        public bool CurtainWasPriority => curtainWasPriority;
        public int HoverWindupFrame => hoverWindupFrame;
        public int HoverFireFrame => hoverFireFrame;
        public int HoverSpawnedCount => hoverSpawnedCount;
        public bool HoverWasPriority => hoverWasPriority;
        public int EmitterTickCount => emitterTickCount;
        public float MinimumEmitterTimeScale => minimumEmitterTimeScale;
        public float MaximumEmitterTimeScale => maximumEmitterTimeScale;
        public int RunStartedCount => runStartedCount;
        public int StopSettleCount => stopSettleCount;
        public int CurtainMoveFirstAppliedFrame => curtainMoveFirstAppliedFrame;
        public int CurtainMoveLastAppliedFrame => curtainMoveLastAppliedFrame;
        public int CurtainZeroAppliedFrame => curtainZeroAppliedFrame;
        public int HoverMoveFirstAppliedFrame => hoverMoveFirstAppliedFrame;
        public int HoverMoveLastAppliedFrame => hoverMoveLastAppliedFrame;
        public int HoverZeroAppliedFrame => hoverZeroAppliedFrame;
        public float CurtainRiskBefore => curtainRiskBefore;
        public float CurtainRiskAfter => curtainRiskAfter;
        public bool StayedInsideForwardBoundary => stayedInsideForwardBoundary;
        public int HoverPreviewCount => hoverPreviewCount;
        public float HoverPreviewAverageLateral => hoverPreviewAverageLateral;
        public float HoverLateralDisplacement => hoverLateralDisplacement;
        public float HoverDirectionDot => hoverDirectionDot;
        public int BasicVolleyEventCount => basicVolleyEventCount;
        public int PressureActionEventCount => pressureActionEventCount;
        public int PlayerDamageEventCount => playerDamageEventCount;
        public int BossDamageEventCount => bossDamageEventCount;
        public int PlayerBasicStartedCount => playerBasicStartedCount;
        public int PlayerBasicHitCount => playerBasicHitCount;
        public int DodgeStartedCount => dodgeStartedCount;
        public int DodgeEndedCount => dodgeEndedCount;
        public int PerfectDodgeCount => perfectDodgeCount;
        public int SummonUsedCount => summonUsedCount;
        public int SummonBlockedCount => summonBlockedCount;
        public int SummonUseBlockedCount => summonUseBlockedCount;
        public int EnemySummonReleaseCountDelta => enemySummonPacing != null
            ? enemySummonPacing.TotalPacingReleaseCount - initialEnemySummonReleaseCount
            : -1;
        public bool PlayerHealthUnchanged => playerHealth != null
            && Mathf.Abs(playerHealth.CurrentHealth - initialPlayerHealth) <= Tolerance;
        public bool BossHealthUnchanged => bossHealth != null
            && Mathf.Abs(bossHealth.CurrentHealth - initialBossHealth) <= Tolerance;
        public bool ResourcesUnchanged => ranged != null
            && ranged.CurrentAmmo == initialAmmo
            && !ranged.IsReloading
            && energy != null
            && Mathf.Abs(energy.CurrentMana - initialEnergy) <= Tolerance;
        public int VisualWindupDelta => visualCue != null
            ? visualCue.WindupWorldVfxCueRequestCount - initialVisualWindupCount
            : -1;
        public int VisualReleaseDelta => visualCue != null
            ? visualCue.ReleaseWorldVfxCueRequestCount - initialVisualReleaseCount
            : -1;
        public int TelegraphWindupDelta => telegraph != null
            ? telegraph.WindupRefreshCount - initialTelegraphWindupCount
            : -1;
        public int TelegraphReleaseDelta => telegraph != null
            ? telegraph.ReleaseFlashCount - initialTelegraphReleaseCount
            : -1;
        public int CameraWindupDelta => cameraCue != null
            ? cameraCue.WindupCueRequestCount - initialCameraWindupCount
            : -1;
        public int CameraFireDelta => cameraCue != null
            ? cameraCue.FireCueRequestCount - initialCameraFireCount
            : -1;
        public int MotionReleaseDelta => motion != null
            ? motion.HeavyReleaseRequestCount - initialMotionReleaseCount
            : -1;
        public string CurtainWindupPatternId => curtainWindupPatternId;
        public string CurtainFirePatternId => curtainFirePatternId;
        public string HoverWindupPatternId => hoverWindupPatternId;
        public string HoverFirePatternId => hoverFirePatternId;
        public int HoverSequenceIndexAfterFire => hoverSequenceIndexAfterFire;
        public int CurtainWindupVisibleMarkerCount => curtainWindupVisibleMarkerCount;
        public int CurtainFireVisibleMarkerCount => curtainFireVisibleMarkerCount;
        public int HoverWindupVisibleMarkerCount => hoverWindupVisibleMarkerCount;
        public int HoverFireVisibleMarkerCount => hoverFireVisibleMarkerCount;
        public int CurtainWindupVisibleRendererCount => curtainWindupVisibleRendererCount;
        public int CurtainFireVisibleRendererCount => curtainFireVisibleRendererCount;
        public int HoverWindupVisibleRendererCount => hoverWindupVisibleRendererCount;
        public int HoverFireVisibleRendererCount => hoverFireVisibleRendererCount;
        public bool TelegraphMarkerCollidersNonBlocking =>
            telegraphMarkerCollidersNonBlocking;
        public Color CurtainWindupMarkerColor => curtainWindupMarkerColor;
        public Color CurtainFireMarkerColor => curtainFireMarkerColor;
        public Color HoverWindupMarkerColor => hoverWindupMarkerColor;
        public Color HoverFireMarkerColor => hoverFireMarkerColor;
        public bool ExactHudAndBindings => ValidateHudAndBindings(throwOnFailure: false);
        public bool StateRestored => stateRestored && !lifecycleEmergencyResetUsed;
        public bool EventsReleased => stateRestored && !eventSubscriptionsActive;
        public bool PresentationClockReleased => stateRestored
            && !PresentationClock.IsManuallyDriven;
        public bool CadenceReleased => stateRestored
            && BossCombatCadenceScheduler.ExternalSuspensionCount == 0
            && encounter != null
            && encounter.IsExternalCombatSuspended == savedEncounterSuspended;
        public bool EmitterRestored => stateRestored
            && emitter != null
            && !emitter.IsFiringEnabled
            && !emitter.IsWindupActive
            && emitter.ActiveProjectileCount == 0
            && emitter.CurrentPatternSequenceIndex == 0
            && emitter.HasQueuedPriorityPattern
            && emitter.QueuedPriorityPattern == curtain
            && emitter.QueuedPriorityWavesRemaining == 1
            && SpawnOriginOrderMatchesSnapshot();
        public bool SpawnOriginOrderRestored => stateRestored
            && SpawnOriginOrderMatchesSnapshot();
        public bool PlayerStateRestored => stateRestored
            && movement != null
            && movement.LaneConstraintEnabled == savedLaneConstraintEnabled
            && !movement.IsCinematicMoveInputLocked
            && Vector3.Distance(movement.transform.position, savedPlayerPosition) <= Tolerance
            && Quaternion.Angle(movement.transform.rotation, savedPlayerRotation) <= 0.01f
            && Vector3.Distance(movement.transform.localScale, savedPlayerScale) <= Tolerance;
        public bool BossStateRestored => stateRestored
            && pressurePosition != null
            && pressurePosition.MovementEnabled == savedPressureMovementEnabled
            && pressurePosition.MovedTransform != null
            && Vector3.Distance(pressurePosition.MovedTransform.position, savedBossPosition) <= Tolerance
            && Quaternion.Angle(pressurePosition.MovedTransform.rotation, savedBossRotation) <= 0.01f
            && Vector3.Distance(pressurePosition.MovedTransform.localScale, savedBossScale) <= Tolerance;
        public bool CameraStateRestored => stateRestored
            && gameplayCamera != null
            && Vector3.Distance(gameplayCamera.transform.position, savedCameraPosition) <= Tolerance
            && Quaternion.Angle(gameplayCamera.transform.rotation, savedCameraRotation) <= 0.01f
            && Mathf.Abs(gameplayCamera.fieldOfView - savedCameraFieldOfView) <= Tolerance;
        public bool HudStateRestored => stateRestored
            && savedHudValid
            && hud != null
            && Mathf.Abs(hud.alpha - savedHudAlpha) <= Tolerance
            && hud.interactable == savedHudInteractable
            && hud.blocksRaycasts == savedHudBlocksRaycasts;
        public bool GlobalStateRestored => stateRestored
            && Time.captureFramerate == savedCaptureFramerate
            && Application.targetFrameRate == savedTargetFrameRate
            && Mathf.Abs(Time.fixedDeltaTime - savedFixedDeltaTime) <= 0.000001f
            && Time.timeScale == savedTimeScale
            && randomStateRestored;
        public bool ExactProjectileAndVfxBindings => emitter != null
            && emitter.PooledProjectilePrefab == expectedPhaseTwoProjectilePrefab
            && emitter.PooledProjectileCount > 0
            && visualCue != null
            && visualCue.CuePlayer != null
            && visualCue.CuePlayer.Profile == expectedCombatVfxProfile;
        public int PostRecordingSettleFrames => postRecordingSettleFrames;
        public float PostRecordingSettleSeconds => postRecordingSettleSeconds;

        public IEnumerator PrepareFreshProductState()
        {
            if (IsPrepared || IsRunning || stateRestored)
            {
                throw new InvalidOperationException(
                    "The G07 director can be prepared exactly once.");
            }

            ValidateFreshScene();
            ResolveBindings();
            CapturePreTransitionState();
            bool succeeded = false;
            try
            {
                SuppressEntryAndTutorial();
                savedRandom = UnityEngine.Random.state;
                savedRandomJson = JsonUtility.ToJson(savedRandom);
                savedRandomValid = true;
                UnityEngine.Random.InitState(
                    AuditionPvStationPhase2PatternRelayCapture.DeterministicRandomSeed);
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
                Time.fixedDeltaTime = 1f / AuditionPvCaptureContract.Fps;
                if (Time.timeScale != 1f
                    || Mathf.Abs(Time.fixedDeltaTime - 1f / 60f) > 0.000001f)
                {
                    throw new InvalidOperationException(
                        "G07 requires exact global time scale one and fixed 60 Hz cadence.");
                }

                flow.TransitionCompleted += HandleTransitionCompleted;
                eventSubscriptionsActive = true;
                ApplyActualThresholdDamage();
                double deadline = Time.realtimeSinceStartupAsDouble
                    + PhaseTwoPreparationTimeoutSeconds;
                while ((flow.CurrentPhase
                            != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                        || flow.TransitionCompletionCount != 1)
                    && Time.realtimeSinceStartupAsDouble < deadline)
                {
                    if (flow.CurrentPhase
                        == OlympusStationAkazaPhase2FlowController.Phase.Transitioning)
                    {
                        flow.TrySkipTransition();
                    }

                    yield return WaitForNextPlayerFrame();
                }

                ValidateCompletedPhaseTwoHandoff();
                CaptureAuthoredPatternSequence();
                handoffStateCaptured = true;
                AcquireExclusiveCadence();
                presentationClockLease = PresentationClock.AcquireManual(
                    this,
                    AuditionPvCaptureContract.Fps);
                for (int frame = 0;
                    frame < AuditionPvStationPhase2PatternRelayCapture.PhaseTwoSettleFrames;
                    frame++)
                {
                    presentationClockLease.SetFrame(frame);
                    yield return WaitForNextPlayerFrame();
                }

                CapturePreparedState();
                StageHudAndInputSafety();
                SubscribeRecordingEvents();
                ValidateReadyForShot();
                IsPrepared = true;
                succeeded = true;
            }
            finally
            {
                if (!succeeded)
                {
                    try
                    {
                        RestoreShotState();
                    }
                    catch (Exception cleanupException)
                    {
                        // Never replace the preparation exception with cleanup
                        // fallout. Preserve it explicitly so the guarded runner's
                        // failure artifact carries both causal chains.
                        RecordCleanupFailure(cleanupException);
                        Debug.LogException(cleanupException, this);
                    }
                }
            }
        }

        public void BeginShotForRecorder()
        {
            BeginShotCore(recorderOwnsCadence: true);
        }

        public void BeginShot()
        {
            BeginShotCore(recorderOwnsCadence: false);
        }

        private void BeginShotCore(bool recorderOwnsCadence)
        {
            if (!IsPrepared || IsRunning || IsComplete || stateRestored)
            {
                throw new InvalidOperationException(
                    "Prepare the fresh G07 product state exactly once before capture.");
            }

            if (recorderOwnsCadence)
            {
                float minimum = 1f / AuditionPvCaptureContract.Fps;
                if (Time.captureDeltaTime <= minimum
                    || Time.captureDeltaTime >= minimum + 0.001f)
                {
                    throw new InvalidOperationException(
                        "G07 Recorder padding is not active at logical f0.");
                }
            }
            else
            {
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
            }

            ValidateReadyForShot();
            presentationClockLease.SetFrame(0);
            emitter.SetFiringEnabled(true);
            if (!emitter.IsFiringEnabled
                || emitter.CurrentPattern != curtain
                || !emitter.CurrentPatternIsPriority)
            {
                throw new InvalidOperationException(
                    "G07 could not open the authored Curtain firing window.");
            }

            currentFrame = AuditionPvStationPhase2PatternRelayCapture.FirstFrame;
            IsRunning = true;
        }

        // RECORDING CONTRACT BEGIN: only product input plus one emitter Tick is
        // allowed between logical f0 and f419. Source tests lock this section.
        private void Update()
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }

            try
            {
                if (!BossCombatCadenceScheduler.IsExternallySuspended
                    || BossCombatCadenceScheduler.ExternalSuspensionCount != 1)
                {
                    throw new InvalidOperationException(
                        "G07 lost its owner-scoped cadence suspension.");
                }

                presentationClockLease.SetFrame(currentFrame);
                ApplyPlayerResponseInput(currentFrame);
                if (Time.timeScale != 1f)
                {
                    throw new InvalidOperationException(
                        "G07 logical recording requires exact Time.timeScale=1.");
                }

                float emitterScale = CombatTimeDilationReceiver.ResolveTimeScale(emitter);
                if (emitterScale != 1f)
                {
                    throw new InvalidOperationException(
                        "G07 logical recording requires exact emitter time scale 1.");
                }

                minimumEmitterTimeScale = Mathf.Min(minimumEmitterTimeScale, emitterScale);
                maximumEmitterTimeScale = Mathf.Max(maximumEmitterTimeScale, emitterScale);
                emitter.Tick((1f / 60f) * emitterScale);
                emitterTickCount++;
                CapturePresentationAfterEvent();
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
                stayedInsideForwardBoundary &=
                    !lane.IsPastForwardBoundary(movement.transform.position);
                if (currentFrame
                    == AuditionPvStationPhase2PatternRelayCapture.LastFrame)
                {
                    CaptureMovementResults();
                    ValidateCompletedShot();
                    IsRunning = false;
                    IsComplete = true;
                }

                LastPresentedFrame = currentFrame;
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
        // RECORDING CONTRACT END

        public IEnumerator RestoreAfterRecording()
        {
            if (stateRestored || !restorableStateCaptured)
            {
                yield break;
            }

            IsRunning = false;
            movement?.SetMoveInput(Vector2.zero);
            UnsubscribeRecordingEvents();
            Exception firstFailure = null;
            try
            {
                if (handoffStateCaptured)
                {
                    AdvanceEmitterAuthoredSequenceToZero();
                }
            }
            catch (Exception exception)
            {
                firstFailure = exception;
                RecordCleanupFailure(exception);
            }

            int settleStartFrame = Mathf.Max(0, LastPresentedFrame);
            while (HasActivePresentationCue()
                && postRecordingSettleFrames
                    < AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget)
            {
                postRecordingSettleFrames++;
                presentationClockLease?.SetFrame(
                    settleStartFrame + postRecordingSettleFrames);
                yield return null;
            }

            postRecordingSettleSeconds = postRecordingSettleFrames
                / (float)AuditionPvCaptureContract.Fps;
            if (HasActivePresentationCue()
                || postRecordingSettleFrames
                    > AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget
                || postRecordingSettleSeconds > PostRecordingSettleTimeoutSeconds)
            {
                var timeout = new TimeoutException(
                    "G07 product presentation did not settle within the exact four-second "
                    + "unrecorded frame budget. "
                    + BuildActivePresentationDiagnostics());
                firstFailure = CombineExceptions(firstFailure, timeout);
                RecordCleanupFailure(timeout);
            }

            try
            {
                RestoreShotState();
            }
            catch (Exception exception)
            {
                firstFailure = CombineExceptions(firstFailure, exception);
                RecordCleanupFailure(exception);
            }

            // ActionCameraController owns its final pose in a later LateUpdate.
            // Give its public prime one unrecorded product frame to prove the
            // restored camera is durable, not an immediate transform self-check.
            yield return new WaitForEndOfFrame();
            if (preparedStateCaptured && !CameraStateRestored)
            {
                var cameraFailure = new InvalidOperationException(
                    "G07 gameplay camera did not retain its restored pose across product LateUpdate.");
                firstFailure = CombineExceptions(firstFailure, cameraFailure);
                RecordCleanupFailure(cameraFailure);
            }

            if (firstFailure != null)
            {
                throw new InvalidOperationException(
                    "G07 asynchronous exhaustive cleanup encountered an error.",
                    firstFailure);
            }
        }

        public void RestoreShotState()
        {
            if (stateRestored || restoring)
            {
                return;
            }

            if (!restorableStateCaptured)
            {
                return;
            }

            restoring = true;
            IsRunning = false;
            Exception firstFailure = null;
            try
            {
                CaptureRestoreFailure(ref firstFailure, UnsubscribeRecordingEvents);
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (flow != null)
                    {
                        flow.TransitionCompleted -= HandleTransitionCompleted;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    movement?.SetMoveInput(Vector2.zero);
                    if (movement != null)
                    {
                        movement.SetCinematicMoveInputLocked(
                            PlayerInputLockSource.EditorVerification,
                            false);
                        movement.SetLaneConstraintEnabled(savedLaneConstraintEnabled);
                    }

                    playerAction?.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false);
                    ranged?.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false);
                    summon?.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false);
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                    encounter?.SetExternalCombatSuspended(savedEncounterSuspended));
                if (handoffStateCaptured)
                {
                    // Encounter pacing is restored first while the owner-scoped
                    // scheduler lease remains held. Its public side effects may
                    // toggle the emitter, so the exact authored idle is always
                    // the final emitter mutation.
                    CaptureRestoreFailure(
                        ref firstFailure,
                        FinalizeEmitterAuthoredIdleWithoutAdvancing);
                }

                if (preparedStateCaptured)
                {
                    CaptureRestoreFailure(ref firstFailure, RestorePlayerPose);
                    CaptureRestoreFailure(ref firstFailure, RestoreBossPose);
                    CaptureRestoreFailure(ref firstFailure, RestoreCameraPose);
                }
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (savedHudValid && hud != null)
                    {
                        hud.alpha = savedHudAlpha;
                        hud.interactable = savedHudInteractable;
                        hud.blocksRaycasts = savedHudBlocksRaycasts;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (entryNotice != null)
                    {
                        entryNotice.enabled = savedEntryEnabled;
                    }

                    if (tutorial != null)
                    {
                        tutorial.enabled = savedTutorialEnabled;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    cadenceSuspensionLease?.Dispose();
                    cadenceSuspensionLease = null;
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    presentationClockLease?.Dispose();
                    presentationClockLease = null;
                });
            }
            finally
            {
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (savedRandomValid)
                    {
                        UnityEngine.Random.state = savedRandom;
                        savedRandomValid = false;
                        randomStateRestored = string.Equals(
                            JsonUtility.ToJson(UnityEngine.Random.state),
                            savedRandomJson,
                            StringComparison.Ordinal);
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (globalStateCaptured)
                    {
                        Time.captureFramerate = savedCaptureFramerate;
                        Application.targetFrameRate = savedTargetFrameRate;
                        Time.fixedDeltaTime = savedFixedDeltaTime;
                        Time.timeScale = savedTimeScale;
                    }
                });
                restoring = false;
            }

            if (firstFailure != null)
            {
                RecordCleanupFailure(firstFailure);
                throw new InvalidOperationException(
                    "G07 exhaustive shot-state restoration encountered an error.",
                    firstFailure);
            }

            stateRestored = true;
            if (handoffStateCaptured && (!EmitterRestored
                || !HudStateRestored
                || !GlobalStateRestored
                || !EventsReleased
                || !PresentationClockReleased
                || !CadenceReleased
                || HasActivePresentationCue()))
            {
                stateRestored = false;
                throw new InvalidOperationException(
                    "G07 restoration did not satisfy the post-handoff emitter contract.");
            }

            if (preparedStateCaptured && (!PlayerStateRestored
                || !BossStateRestored
                || !CameraStateRestored
                || !HudStateRestored
                || !GlobalStateRestored
                || !EventsReleased
                || !PresentationClockReleased
                || !CadenceReleased
                || HasActivePresentationCue()))
            {
                stateRestored = false;
                throw new InvalidOperationException(
                    "G07 restoration did not satisfy the emitter, pose, HUD, camera, global, event, clock, or cadence contract.");
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

        private void TryRestoreFromLifecycle()
        {
            if (!restorableStateCaptured)
            {
                return;
            }

            try
            {
                RestoreFromLifecycleEmergency();
            }
            catch (Exception exception)
            {
                RecordCleanupFailure(exception);
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        /// Synchronous domain/lifecycle fallback only. The successful golden path
        /// must use RestoreAfterRecording so authored sequence advancement and cue
        /// decay remain observable product behavior. When Unity can no longer run
        /// that coroutine, this fallback restores the read-only authored snapshot
        /// through public product reset APIs and permanently marks the take as
        /// failure-only provenance.
        /// </summary>
        public void RestoreFromLifecycleEmergency()
        {
            if (stateRestored || !restorableStateCaptured)
            {
                return;
            }

            lifecycleEmergencyResetUsed = true;
            IsRunning = false;
            Exception failure = null;
            CaptureRestoreFailure(ref failure, UnsubscribeRecordingEvents);
            CaptureRestoreFailure(ref failure, ResetPresentationAndEmitterForLifecycle);
            CaptureRestoreFailure(ref failure, RestoreShotState);
            if (failure != null)
            {
                RecordCleanupFailure(failure);
                throw new InvalidOperationException(
                    "G07 lifecycle emergency restoration encountered an error.",
                    failure);
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
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G07 requires a fresh Station PlayMode scene.");
            }
        }

        private void ResolveBindings()
        {
            flow = UnityEngine.Object.FindFirstObjectByType<
                OlympusStationAkazaPhase2FlowController>(FindObjectsInactive.Include);
            encounter = flow != null ? flow.EncounterController : null;
            emitter = flow != null ? flow.BarrageEmitter : null;
            pressureAction = flow != null ? flow.PressureActionDirector : null;
            pressurePosition = flow != null ? flow.PressurePositionController : null;
            playerHealth = flow != null ? flow.PlayerHealth : null;
            bossHealth = flow != null ? flow.BossHealth : null;
            movement = flow != null ? flow.PlayerMovement : null;
            playerAction = flow != null ? flow.PlayerActionController : null;
            ranged = flow != null ? flow.PlayerRangedBasicAttackAction : null;
            hud = flow != null ? flow.CombatHudCanvasGroup : null;
            hudCanvas = hud != null
                ? hud.GetComponentInParent<Canvas>(includeInactive: true)
                : null;
            basicFire = UnityEngine.Object.FindObjectsByType<BossBasicFireEmitter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate != null
                    && candidate.gameObject == emitter?.gameObject);
            enemySummonPacing = UnityEngine.Object.FindObjectsByType<
                    EnemySummonPacingDirector>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate != null
                    && candidate.gameObject == emitter?.gameObject);
            summon = UnityEngine.Object.FindObjectsByType<PlayerSummonSlot1Action>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate != null
                    && candidate.gameObject == playerHealth?.gameObject);
            energy = encounter != null ? encounter.EnergyLadder : null;
            lane = movement != null ? movement.LaneSpace : null;
            curtain = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath);
            hover = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath);
            GameObject expectedProjectileRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                AuditionPvStationPhase2PatternRelayCapture.PhaseTwoProjectilePrefabPath);
            expectedPhaseTwoProjectilePrefab = expectedProjectileRoot != null
                ? expectedProjectileRoot.GetComponent<BossBarrageProjectile>()
                : null;
            expectedCombatVfxProfile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(
                AuditionPvStationPhase2PatternRelayCapture.CombatVfxProfilePath);
            visualCue = UnityEngine.Object.FindObjectsByType<BossBarrageVisualCueDriver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate.BossBarrageEmitter == emitter);
            telegraph = UnityEngine.Object.FindObjectsByType<
                    BossBarrageLaneTelegraphPresenter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate.BossBarrageEmitter == emitter);
            cameraCue = UnityEngine.Object.FindObjectsByType<BossBarrageCameraCueDriver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate.BossBarrageEmitter == emitter);
            motion = UnityEngine.Object.FindObjectsByType<AkazaPhase2CombatMotionDriver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate => candidate.BossBarrageEmitter == emitter);
            actionCamera = cameraCue != null ? cameraCue.CameraController : null;
            gameplayCamera = actionCamera != null
                ? actionCamera.GetComponent<Camera>()
                : null;
            hudBinder = UnityEngine.Object.FindFirstObjectByType<
                BossBarrageLaneReviewCombatHudBinder>(FindObjectsInactive.Include);
            entryNotice = UnityEngine.Object.FindFirstObjectByType<SceneEntryNoticeOverlay>(
                FindObjectsInactive.Include);
            tutorial = UnityEngine.Object.FindFirstObjectByType<
                OlympusCorridorTutorialDirector>(FindObjectsInactive.Include);

            if (flow == null
                || encounter == null
                || emitter == null
                || basicFire == null
                || pressureAction == null
                || pressurePosition == null
                || pressurePosition.MovedTransform == null
                || enemySummonPacing == null
                || playerHealth == null
                || bossHealth == null
                || movement == null
                || playerAction == null
                || ranged == null
                || summon == null
                || energy == null
                || lane == null
                || curtain == null
                || hover == null
                || expectedPhaseTwoProjectilePrefab == null
                || expectedCombatVfxProfile == null
                || visualCue == null
                || telegraph == null
                || cameraCue == null
                || motion == null
                || actionCamera == null
                || gameplayCamera == null
                || hud == null
                || hudCanvas == null
                || hudBinder == null)
            {
                throw new InvalidOperationException(
                    "G07 could not resolve its exact Station gameplay, HUD, emitter, player, boss, or presentation bindings.");
            }
        }

        private void CapturePreTransitionState()
        {
            savedHudValid = hud != null;
            savedHudAlpha = hud.alpha;
            savedHudInteractable = hud.interactable;
            savedHudBlocksRaycasts = hud.blocksRaycasts;
            savedEntryEnabled = entryNotice != null && entryNotice.enabled;
            savedTutorialEnabled = tutorial != null && tutorial.enabled;
            savedVisualCueEnabled = visualCue.enabled;
            savedTelegraphEnabled = telegraph.enabled;
            savedCameraCueEnabled = cameraCue.enabled;
            savedMotionEnabled = motion.enabled;
            savedLaneConstraintEnabled = movement.LaneConstraintEnabled;
            savedCaptureFramerate = Time.captureFramerate;
            savedTargetFrameRate = Application.targetFrameRate;
            savedFixedDeltaTime = Time.fixedDeltaTime;
            savedTimeScale = Time.timeScale;
            globalStateCaptured = true;
            if (flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase1
                || flow.TransitionStartCount != 0
                || flow.TransitionCompletionCount != 0
                || !savedLaneConstraintEnabled)
            {
                throw new InvalidOperationException(
                    "G07 requires the untouched Phase1 Station lane baseline.");
            }

            restorableStateCaptured = true;
        }

        private void CapturePreparedState()
        {
            savedPressureMovementEnabled = pressurePosition.MovementEnabled;
            savedPlayerPosition = movement.transform.position;
            savedPlayerRotation = movement.transform.rotation;
            savedPlayerScale = movement.transform.localScale;
            Transform boss = pressurePosition.MovedTransform;
            savedBossPosition = boss.position;
            savedBossRotation = boss.rotation;
            savedBossScale = boss.localScale;
            savedCameraPosition = gameplayCamera.transform.position;
            savedCameraRotation = gameplayCamera.transform.rotation;
            savedCameraFieldOfView = gameplayCamera.fieldOfView;
            initialAmmo = ranged.CurrentAmmo;
            initialEnergy = energy.CurrentMana;
            initialPlayerHealth = playerHealth.CurrentHealth;
            initialBossHealth = bossHealth.CurrentHealth;
            initialEnemySummonReleaseCount = enemySummonPacing.TotalPacingReleaseCount;
            initialVisualWindupCount = visualCue.WindupWorldVfxCueRequestCount;
            initialVisualReleaseCount = visualCue.ReleaseWorldVfxCueRequestCount;
            initialTelegraphWindupCount = telegraph.WindupRefreshCount;
            initialTelegraphReleaseCount = telegraph.ReleaseFlashCount;
            initialCameraWindupCount = cameraCue.WindupCueRequestCount;
            initialCameraFireCount = cameraCue.FireCueRequestCount;
            initialMotionReleaseCount = motion.HeavyReleaseRequestCount;
            preparedStateCaptured = true;
        }

        private void CaptureAuthoredPatternSequence()
        {
            var serialized = new SerializedObject(emitter);
            SerializedProperty sequence = serialized.FindProperty("patternSequence");
            SerializedProperty waves = serialized.FindProperty("wavesPerPattern");
            SerializedProperty origins = serialized.FindProperty("projectileSpawnOrigins");
            if (sequence == null
                || !sequence.isArray
                || waves == null
                || origins == null
                || !origins.isArray)
            {
                throw new InvalidOperationException(
                    "G07 could not read the authored emitter sequence snapshot.");
            }

            authoredPatternSequence = new BossBarragePatternProfile[sequence.arraySize];
            for (int index = 0; index < sequence.arraySize; index++)
            {
                authoredPatternSequence[index] = sequence
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue as BossBarragePatternProfile;
            }

            authoredWavesPerPattern = waves.intValue;
            authoredSpawnOrigins = new Transform[origins.arraySize];
            for (int index = 0; index < origins.arraySize; index++)
            {
                authoredSpawnOrigins[index] = origins.GetArrayElementAtIndex(index)
                    .objectReferenceValue as Transform;
            }
            BossBarragePatternProfile spiral =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2PatternRelayCapture.SpiralProfilePath);
            BossBarragePatternProfile crush =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2PatternRelayCapture.CrushNetProfilePath);
            BossBarragePatternProfile[] expected =
            {
                hover,
                curtain,
                spiral,
                hover,
                curtain,
                crush
            };
            if (authoredWavesPerPattern != 1
                || authoredPatternSequence.Length != expected.Length
                || authoredPatternSequence.Where((value, index) => value != expected[index]).Any()
                || authoredSpawnOrigins.Length != 6
                || authoredSpawnOrigins.Any(value => value == null
                    || value.gameObject.scene != emitter.gameObject.scene
                    || !value.IsChildOf(emitter.transform.root))
                || authoredSpawnOrigins.Distinct().Count() != authoredSpawnOrigins.Length)
            {
                throw new InvalidOperationException(
                    "G07 authored Phase2 sequence/spawn-origin snapshot is not exact Hover/Curtain/Spiral/Hover/Curtain/Crush x1 with six ordered internal muzzles.");
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
            float thresholdHealth = bossHealth.MaxHealth * flow.PhaseThreshold01;
            float amount = Mathf.Max(1f, bossHealth.CurrentHealth - thresholdHealth);
            var damage = new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                amount,
                bossHealth.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None);
            if (!bossHealth.TryApplyDamage(damage)
                || flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Transitioning
                || flow.TransitionStartCount != 1)
            {
                throw new InvalidOperationException(
                    "G07 real threshold damage did not start exactly one Phase2 transition.");
            }
        }

        private void ValidateCompletedPhaseTwoHandoff()
        {
            if (flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                || !flow.PhaseTwoApplied
                || flow.TransitionStartCount != 1
                || flow.TransitionCompletionCount != 1
                || transitionCompletedEventCount != 1
                || flow.TransitionFaultedOpen)
            {
                throw new InvalidOperationException(
                    "G07 did not observe one exact authored Phase2 completion.");
            }

            if (!emitter.HasQueuedPriorityPattern
                || emitter.QueuedPriorityPattern != curtain
                || emitter.QueuedPriorityWavesRemaining != 1
                || emitter.IsWindupActive
                || emitter.CurrentPatternSequenceIndex != 0)
            {
                throw new InvalidOperationException(
                    "G07 expected dormant priority Curtain and sequence index zero Hover after handoff.");
            }

            if (!ExactProjectileAndVfxBindings)
            {
                throw new InvalidOperationException(
                    "G07 Phase2 handoff did not retain the exact authored projectile prefab, pool, and Combat VFX cue profile bindings.");
            }

            if (!string.Equals(curtain.PatternId, "AkazaSummonCurtain", StringComparison.Ordinal)
                || curtain.ProjectilesPerWave
                    != AuditionPvStationPhase2PatternRelayCapture.CurtainProjectileCount
                || !string.Equals(hover.PatternId, "AkazaHoverLance", StringComparison.Ordinal)
                || hover.ProjectilesPerWave
                    != AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount)
            {
                throw new InvalidOperationException(
                    "G07 authored Curtain/Hover identities or projectile counts changed.");
            }
        }

        private void AcquireExclusiveCadence()
        {
            // Capture the post-handoff state before taking both public suspension
            // paths. The queue stays dormant while the fixed settle runs.
            savedEncounterSuspended = encounter.IsExternalCombatSuspended;
            savedEmitterFiring = emitter.IsFiringEnabled;
            if (savedEncounterSuspended || savedEmitterFiring)
            {
                throw new InvalidOperationException(
                    "G07 fresh post-handoff state must begin externally unsuspended with the emitter disabled.");
            }

            encounter.SetExternalCombatSuspended(true);
            cadenceSuspensionLease = BossCombatCadenceScheduler.AcquireExternalSuspension(this);
            if (!encounter.IsExternalCombatSuspended
                || !BossCombatCadenceScheduler.IsExternallySuspended
                || BossCombatCadenceScheduler.ExternalSuspensionCount != 1
                || emitter.IsFiringEnabled
                || emitter.IsWindupActive
                || !emitter.HasQueuedPriorityPattern
                || emitter.QueuedPriorityPattern != curtain
                || emitter.QueuedPriorityWavesRemaining != 1
                || emitter.CurrentPatternSequenceIndex != 0)
            {
                throw new InvalidOperationException(
                    "G07 could not acquire exclusive encounter and cadence suspension while retaining the authored opening.");
            }
        }

        private void StageHudAndInputSafety()
        {
            hud.alpha = 1f;
            hud.interactable = true;
            hud.blocksRaycasts = true;
            movement.SetLaneConstraintEnabled(true);
            movement.SetCinematicMoveInputLocked(
                PlayerInputLockSource.EditorVerification,
                false);
            playerAction.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            ranged.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            summon.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            movement.SetMoveInput(Vector2.zero);
            if (!ValidateHudAndBindings(throwOnFailure: true)
                || initialAmmo != ranged.MagazineSize
                || ranged.IsReloading
                || playerHealth.IsInvulnerable
                || summon.ActivePressureScreenCount != 0
                || summon.ActiveProjectileCount != 0
                || summon.ActiveSummonActorCount != 0)
            {
                throw new InvalidOperationException(
                    "G07 HUD/resources or player safety baseline is not exact.");
            }
        }

        private bool ValidateHudAndBindings(bool throwOnFailure)
        {
            bool exact = hud == flow?.CombatHudCanvasGroup
                && hud.gameObject.activeInHierarchy
                && Mathf.Abs(hud.alpha - 1f) <= Tolerance
                && hudCanvas != null
                && hudCanvas.enabled
                && hudCanvas.gameObject.activeInHierarchy
                && flow.PlayerHealth == playerHealth
                && flow.BossHealth == bossHealth
                && flow.PlayerMovement == movement
                && flow.PlayerActionController == playerAction
                && flow.PlayerRangedBasicAttackAction == ranged
                && encounter.EnergyLadder == energy
                && movement.LaneSpace == lane
                && visualCue.BossBarrageEmitter == emitter
                && telegraph.BossBarrageEmitter == emitter
                && telegraph.LaneSpace == lane
                && cameraCue.BossBarrageEmitter == emitter
                && cameraCue.CameraController == actionCamera
                && motion.BossBarrageEmitter == emitter
                && motion.BossHealth == bossHealth
                && ExactProjectileAndVfxBindings;

            if (exact)
            {
                var serialized = new SerializedObject(hudBinder);
                exact = serialized.FindProperty("encounterController")?.objectReferenceValue == encounter
                    && serialized.FindProperty("playerHealth")?.objectReferenceValue == playerHealth
                    && serialized.FindProperty("bossHealth")?.objectReferenceValue == bossHealth
                    && serialized.FindProperty("energyLadder")?.objectReferenceValue == energy
                    && serialized.FindProperty("actionController")?.objectReferenceValue == playerAction
                    && serialized.FindProperty("movementController")?.objectReferenceValue == movement
                    && serialized.FindProperty("rangedBasicAttackAction")?.objectReferenceValue == ranged
                    && serialized.FindProperty("summonSlot1Action")?.objectReferenceValue == summon;
            }

            if (!exact && throwOnFailure)
            {
                throw new InvalidOperationException(
                    "G07 exact HUD/player/boss/resource/camera binding validation failed.");
            }

            return exact;
        }

        private void ValidateReadyForShot()
        {
            if (flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                || flow.TransitionCompletionCount != 1
                || transitionCompletedEventCount != 1
                || !encounter.IsExternalCombatSuspended
                || !BossCombatCadenceScheduler.IsExternallySuspended
                || BossCombatCadenceScheduler.ExternalSuspensionCount != 1
                || emitter.IsFiringEnabled
                || emitter.IsWindupActive
                || !emitter.HasQueuedPriorityPattern
                || emitter.QueuedPriorityPattern != curtain
                || emitter.QueuedPriorityWavesRemaining != 1
                || emitter.CurrentPatternSequenceIndex != 0
                || emitter.ActiveProjectileCount != 0
                || !ExactHudAndBindings
                || Time.timeScale != 1f
                || playerHealth.IsInvulnerable
                || movement.IsCinematicMoveInputLocked
                || !playerAction.IsCinematicInputLocked
                || !ranged.IsCinematicInputLocked
                || !summon.IsCinematicInputLocked)
            {
                throw new InvalidOperationException(
                    "G07 canonical product state is not ready for logical f0.");
            }
        }

        private void SubscribeRecordingEvents()
        {
            emitter.WindupStarted += HandleWindupStarted;
            emitter.WaveFired += HandleWaveFired;
            movement.RunStarted += HandleRunStarted;
            movement.StopSettleStarted += HandleStopSettleStarted;
            basicFire.VolleyFired += HandleBasicVolley;
            pressureAction.ActionQueued += HandlePressureAction;
            playerHealth.Damaged += HandlePlayerDamaged;
            bossHealth.Damaged += HandleBossDamaged;
            playerAction.BasicAttackStarted += HandlePlayerBasicStarted;
            playerAction.BasicAttackHit += HandlePlayerBasicHit;
            playerAction.DodgeStarted += HandleDodgeStarted;
            playerAction.DodgeEnded += HandleDodgeEnded;
            playerAction.PerfectDodgeTriggered += HandlePerfectDodge;
            summon.SummonSlot1Used += HandleSummonUsed;
            summon.SummonPressureBlocked += HandleSummonBlocked;
            summon.SummonSlot1UseBlocked += HandleSummonUseBlocked;
            eventSubscriptionsActive = true;
        }

        private void UnsubscribeRecordingEvents()
        {
            if (!eventSubscriptionsActive)
            {
                return;
            }

            if (emitter != null)
            {
                emitter.WindupStarted -= HandleWindupStarted;
                emitter.WaveFired -= HandleWaveFired;
            }

            if (movement != null)
            {
                movement.RunStarted -= HandleRunStarted;
                movement.StopSettleStarted -= HandleStopSettleStarted;
            }

            if (basicFire != null)
            {
                basicFire.VolleyFired -= HandleBasicVolley;
            }

            if (pressureAction != null)
            {
                pressureAction.ActionQueued -= HandlePressureAction;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged -= HandlePlayerDamaged;
            }

            if (bossHealth != null)
            {
                bossHealth.Damaged -= HandleBossDamaged;
            }

            if (playerAction != null)
            {
                playerAction.BasicAttackStarted -= HandlePlayerBasicStarted;
                playerAction.BasicAttackHit -= HandlePlayerBasicHit;
                playerAction.DodgeStarted -= HandleDodgeStarted;
                playerAction.DodgeEnded -= HandleDodgeEnded;
                playerAction.PerfectDodgeTriggered -= HandlePerfectDodge;
            }

            if (summon != null)
            {
                summon.SummonSlot1Used -= HandleSummonUsed;
                summon.SummonPressureBlocked -= HandleSummonBlocked;
                summon.SummonSlot1UseBlocked -= HandleSummonUseBlocked;
            }

            eventSubscriptionsActive = false;
        }

        private void ApplyPlayerResponseInput(int frame)
        {
            if (frame
                == AuditionPvStationPhase2PatternRelayCapture.CurtainMoveFirstFrame)
            {
                curtainRiskBefore = lane.EvaluateForwardRisk01(
                    movement.transform.position);
            }

            if (frame >= AuditionPvStationPhase2PatternRelayCapture.CurtainMoveFirstFrame
                && frame <= AuditionPvStationPhase2PatternRelayCapture.CurtainMoveLastFrame)
            {
                if (curtainMoveFirstAppliedFrame < 0)
                {
                    curtainMoveFirstAppliedFrame = frame;
                }

                curtainMoveLastAppliedFrame = frame;
                movement.SetMoveInput(WorldDirectionToMovementInput(-lane.transform.forward));
            }
            else if (frame
                == AuditionPvStationPhase2PatternRelayCapture.CurtainStopFrame)
            {
                movement.SetMoveInput(Vector2.zero);
                curtainZeroAppliedFrame = frame;
                curtainRiskAfter = lane.EvaluateForwardRisk01(
                    movement.transform.position);
            }

            if (frame == AuditionPvStationPhase2PatternRelayCapture.HoverMoveFirstFrame)
            {
                if (hoverPreviewCount
                        != AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount
                    || expectedHoverWorldDirection.sqrMagnitude <= 0.99f)
                {
                    throw new InvalidOperationException(
                        "G07 could not derive the opposite-lateral answer from the four-point Hover preview.");
                }

                hoverMoveStart = movement.transform.position;
            }

            if (frame >= AuditionPvStationPhase2PatternRelayCapture.HoverMoveFirstFrame
                && frame <= AuditionPvStationPhase2PatternRelayCapture.HoverMoveLastFrame)
            {
                if (hoverMoveFirstAppliedFrame < 0)
                {
                    hoverMoveFirstAppliedFrame = frame;
                }

                hoverMoveLastAppliedFrame = frame;
                movement.SetMoveInput(
                    WorldDirectionToMovementInput(expectedHoverWorldDirection));
            }
            else if (frame == AuditionPvStationPhase2PatternRelayCapture.HoverStopFrame)
            {
                movement.SetMoveInput(Vector2.zero);
                hoverZeroAppliedFrame = frame;
                hoverMoveEnd = movement.transform.position;
            }
        }

        private Vector2 WorldDirectionToMovementInput(Vector3 worldDirection)
        {
            Vector3 desired = Vector3.ProjectOnPlane(worldDirection, Vector3.up).normalized;
            Vector3 cameraForward = Vector3.ProjectOnPlane(
                gameplayCamera.transform.forward,
                Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(
                gameplayCamera.transform.right,
                Vector3.up).normalized;
            return Vector2.ClampMagnitude(
                new Vector2(
                    Vector3.Dot(desired, cameraRight),
                    Vector3.Dot(desired, cameraForward)),
                1f);
        }

        private void CapturePresentationAfterEvent()
        {
            if (currentFrame == curtainWindupFrame)
            {
                CaptureTelegraphEvidence(
                    curtain,
                    AuditionPvStationPhase2PatternRelayCapture.CurtainProjectileCount,
                    out curtainWindupVisibleMarkerCount,
                    out curtainWindupVisibleRendererCount,
                    out curtainWindupMarkerColor);
                if (!string.Equals(
                        visualCue.LastWindupTrigger,
                        "EliteSummonPackage",
                        StringComparison.Ordinal)
                    || telegraph.VisiblePattern != curtain
                    || !string.Equals(
                        telegraph.LastPatternId,
                        curtain.PatternId,
                        StringComparison.Ordinal)
                    || cameraCue.WindupCueRequestCount - initialCameraWindupCount != 1
                    || motion.HeavyReleaseRequestCount - initialMotionReleaseCount != 1)
                {
                    throw new InvalidOperationException(
                        "G07 Curtain windup did not cross the authored visual, telegraph, camera, and motion event path.");
                }
            }
            else if (currentFrame == curtainFireFrame)
            {
                CaptureTelegraphEvidence(
                    curtain,
                    AuditionPvStationPhase2PatternRelayCapture.CurtainProjectileCount,
                    out curtainFireVisibleMarkerCount,
                    out curtainFireVisibleRendererCount,
                    out curtainFireMarkerColor);
                if (!string.Equals(
                        visualCue.LastReleaseTrigger,
                        "AttackFanPressure",
                        StringComparison.Ordinal)
                    || telegraph.VisiblePattern != curtain
                    || cameraCue.FireCueRequestCount - initialCameraFireCount != 1)
                {
                    throw new InvalidOperationException(
                        "G07 Curtain fire did not cross the authored release presentation path.");
                }
            }
            else if (currentFrame == hoverWindupFrame)
            {
                CaptureTelegraphEvidence(
                    hover,
                    AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount,
                    out hoverWindupVisibleMarkerCount,
                    out hoverWindupVisibleRendererCount,
                    out hoverWindupMarkerColor);
                if (!string.Equals(
                        visualCue.LastWindupTrigger,
                        "EliteAuraBuffer",
                        StringComparison.Ordinal)
                    || telegraph.VisiblePattern != hover
                    || cameraCue.WindupCueRequestCount - initialCameraWindupCount != 2
                    || motion.HeavyReleaseRequestCount - initialMotionReleaseCount != 2)
                {
                    throw new InvalidOperationException(
                        "G07 Hover windup did not cross the authored visual, telegraph, camera, and motion event path.");
                }
            }
            else if (currentFrame == hoverFireFrame)
            {
                CaptureTelegraphEvidence(
                    hover,
                    AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount,
                    out hoverFireVisibleMarkerCount,
                    out hoverFireVisibleRendererCount,
                    out hoverFireMarkerColor);
                if (!string.Equals(
                        visualCue.LastReleaseTrigger,
                        "AttackLinePressure",
                        StringComparison.Ordinal)
                    || telegraph.VisiblePattern != hover
                    || cameraCue.FireCueRequestCount - initialCameraFireCount != 2)
                {
                    throw new InvalidOperationException(
                        "G07 Hover fire did not cross the authored release presentation path.");
                }

                hoverSequenceIndexAfterFire = emitter.CurrentPatternSequenceIndex;
            }
        }

        private void CaptureTelegraphEvidence(
            BossBarragePatternProfile expectedPattern,
            int expectedCount,
            out int visibleMarkerCount,
            out int visibleRendererCount,
            out Color markerColor)
        {
            visibleMarkerCount = telegraph.VisibleMarkerCount;
            markerColor = telegraph.LastMarkerColor;
            visibleRendererCount = 0;
            var serialized = new SerializedObject(telegraph);
            SerializedProperty renderers = serialized.FindProperty("markerRenderers");
            if (renderers == null || !renderers.isArray)
            {
                throw new InvalidOperationException(
                    "G07 could not inspect the authored telegraph renderers.");
            }

            for (int index = 0; index < renderers.arraySize; index++)
            {
                var renderer = renderers.GetArrayElementAtIndex(index)
                    .objectReferenceValue as Renderer;
                if (renderer != null
                    && renderer.enabled
                    && renderer.gameObject.activeInHierarchy)
                {
                    visibleRendererCount++;
                }
            }

            if (telegraph.EnabledMarkerColliderCount != 0)
            {
                telegraphMarkerCollidersNonBlocking = false;
                throw new InvalidOperationException(
                    $"G07 {expectedPattern.PatternId} telegraph presentation enabled "
                    + telegraph.EnabledMarkerColliderCount
                    + " marker collider(s); visual warnings must never alter movement or physics.");
            }

            bool windup = emitter.IsWindupActive;
            Color expectedColor = windup
                ? expectedPattern.TelegraphWindupColor
                : expectedPattern.TelegraphReleaseColor;
            if (telegraph.VisiblePattern != expectedPattern
                || telegraph.LastPreviewCount != expectedCount
                || visibleMarkerCount != expectedCount
                || visibleRendererCount != expectedCount
                || Mathf.Abs(markerColor.r - expectedColor.r) > Tolerance
                || Mathf.Abs(markerColor.g - expectedColor.g) > Tolerance
                || Mathf.Abs(markerColor.b - expectedColor.b) > Tolerance)
            {
                throw new InvalidOperationException(
                    $"G07 {expectedPattern.PatternId} telegraph evidence is not exact: "
                    + $"markers={visibleMarkerCount}, renderers={visibleRendererCount}, "
                    + $"preview={telegraph.LastPreviewCount}, color={markerColor}.");
            }
        }

        private void CaptureMovementResults()
        {
            Vector3 displacement = Vector3.ProjectOnPlane(
                hoverMoveEnd - hoverMoveStart,
                Vector3.up);
            hoverLateralDisplacement = displacement.magnitude;
            hoverDirectionDot = displacement.sqrMagnitude > 0.0001f
                ? Vector3.Dot(
                    displacement.normalized,
                    expectedHoverWorldDirection.normalized)
                : -1f;
        }

        private void ValidateCompletedShot()
        {
            if (emitterTickCount
                    != AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount
                || windupEventCount != 2
                || waveEventCount != 2
                || curtainWindupFrame
                    != AuditionPvStationPhase2PatternRelayCapture.CurtainWindupFrame
                || curtainFireFrame
                    != AuditionPvStationPhase2PatternRelayCapture.CurtainFireFrame
                || curtainSpawnedCount
                    != AuditionPvStationPhase2PatternRelayCapture.CurtainProjectileCount
                || !curtainWasPriority
                || hoverWindupFrame
                    != AuditionPvStationPhase2PatternRelayCapture.HoverWindupFrame
                || hoverFireFrame
                    != AuditionPvStationPhase2PatternRelayCapture.HoverFireFrame
                || hoverSpawnedCount
                    != AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount
                || hoverWasPriority
                || hoverSequenceIndexAfterFire != 1
                || emitter.CurrentPatternSequenceIndex != 1
                || curtainWindupVisibleMarkerCount != 7
                || curtainFireVisibleMarkerCount != 7
                || hoverWindupVisibleMarkerCount != 4
                || hoverFireVisibleMarkerCount != 4
                || curtainWindupVisibleRendererCount != 7
                || curtainFireVisibleRendererCount != 7
                || hoverWindupVisibleRendererCount != 4
                || hoverFireVisibleRendererCount != 4
                || !telegraphMarkerCollidersNonBlocking
                || runStartedCount != 2
                || stopSettleCount != 2
                || curtainMoveFirstAppliedFrame != 17
                || curtainMoveLastAppliedFrame != 46
                || curtainZeroAppliedFrame != 47
                || hoverMoveFirstAppliedFrame != 374
                || hoverMoveLastAppliedFrame != 406
                || hoverZeroAppliedFrame != 407
                || curtainRiskBefore - curtainRiskAfter
                    < AuditionPvStationPhase2PatternRelayCapture.MinimumCurtainRiskDecrease
                || !stayedInsideForwardBoundary
                || hoverPreviewCount
                    != AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount
                || hoverLateralDisplacement
                    < AuditionPvStationPhase2PatternRelayCapture.MinimumHoverLateralDisplacement
                || hoverDirectionDot
                    <= AuditionPvStationPhase2PatternRelayCapture.MinimumHoverDirectionDot
                || minimumEmitterTimeScale != 1f
                || maximumEmitterTimeScale != 1f
                || VisualWindupDelta != 2
                || VisualReleaseDelta != 2
                || TelegraphWindupDelta != 2
                || TelegraphReleaseDelta != 2
                || CameraWindupDelta != 2
                || CameraFireDelta != 2
                || MotionReleaseDelta != 2
                || basicVolleyEventCount != 0
                || pressureActionEventCount != 0
                || EnemySummonReleaseCountDelta != 0
                || playerDamageEventCount != 0
                || bossDamageEventCount != 0
                || playerBasicStartedCount != 0
                || playerBasicHitCount != 0
                || dodgeStartedCount != 0
                || dodgeEndedCount != 0
                || perfectDodgeCount != 0
                || summonUsedCount != 0
                || summonBlockedCount != 0
                || summonUseBlockedCount != 0
                || !PlayerHealthUnchanged
                || !BossHealthUnchanged
                || !ResourcesUnchanged
                || summon.ActivePressureScreenCount != 0
                || summon.ActiveProjectileCount != 0
                || summon.ActiveSummonActorCount != 0
                || !ExactHudAndBindings)
            {
                throw new InvalidOperationException(
                    "G07 completed frames but failed its exact Tick schedule, product presentation, player response, no-damage, no-extra-action, HUD, or identity contract.");
            }
        }

        private bool HasActivePresentationCue()
        {
            return visualCue != null && visualCue.IsCueActive
                || visualCue != null
                    && visualCue.CuePlayer != null
                    && (visualCue.CuePlayer.ScheduledCueReleaseCount > 0
                        || visualCue.CuePlayer.ActiveProfileAudioSourceCount > 0)
                || telegraph != null
                    && (telegraph.IsRefreshing || telegraph.VisibleMarkerCount > 0)
                || cameraCue != null
                    && cameraCue.CameraController != null
                    && (cameraCue.CameraController.HasActiveCue
                        || cameraCue.CameraController.HasActiveMicroShake)
                || motion != null && motion.IsHeavyReleaseActive;
        }

        private string BuildActivePresentationDiagnostics()
        {
            return "visual=" + (visualCue != null && visualCue.IsCueActive)
                + ", scheduledVfx=" + (visualCue != null && visualCue.CuePlayer != null
                    ? visualCue.CuePlayer.ScheduledCueReleaseCount
                    : -1)
                + ", activeAudio=" + (visualCue != null && visualCue.CuePlayer != null
                    ? visualCue.CuePlayer.ActiveProfileAudioSourceCount
                    : -1)
                + ", telegraphRefreshing=" + (telegraph != null && telegraph.IsRefreshing)
                + ", telegraphMarkers=" + (telegraph != null ? telegraph.VisibleMarkerCount : -1)
                + ", cameraCue=" + (cameraCue != null
                    && cameraCue.CameraController != null
                    && cameraCue.CameraController.HasActiveCue)
                + ", cameraShake=" + (cameraCue != null
                    && cameraCue.CameraController != null
                    && cameraCue.CameraController.HasActiveMicroShake)
                + ", heavyRelease=" + (motion != null && motion.IsHeavyReleaseActive)
                + ".";
        }

        private void AdvanceEmitterAuthoredSequenceToZero()
        {
            if (emitter == null)
            {
                return;
            }

            if (authoredPatternSequence == null
                || authoredPatternSequence.Length != 6
                || authoredPatternSequence.Any(value => value == null)
                || authoredWavesPerPattern != 1)
            {
                throw new InvalidOperationException(
                    "G07 lost the exact read-only authored sequence snapshot before cleanup.");
            }

            // Never reconstruct or assign the authored sequence. After the
            // recorded Hover wave truthfully advances index 0 -> 1, cleanup
            // (with Recorder stopped and measurement callbacks released)
            // advances the remaining authored entries through the same public
            // Tick path. A false edge after each wave deactivates its projectiles
            // before the next pool demand while preserving the advanced index.
            emitter.SetFiringEnabled(true);
            emitter.SetFiringEnabled(false);
            int safety = authoredPatternSequence.Length;
            while (emitter.CurrentPatternSequenceIndex != 0 && safety-- > 0)
            {
                int indexBefore = emitter.CurrentPatternSequenceIndex;
                BossBarragePatternProfile expected = authoredPatternSequence[indexBefore];
                emitter.SetFiringEnabled(true);
                if (emitter.CurrentPattern != expected)
                {
                    throw new InvalidOperationException(
                        "G07 cleanup encountered an unexpected authored sequence profile at index "
                        + indexBefore + ".");
                }

                float initialStep = expected.InitialDelaySeconds + 1f;
                float windupStep = expected.WindupSeconds + 1f;
                if (!float.IsFinite(initialStep)
                    || !float.IsFinite(windupStep)
                    || initialStep <= 0f
                    || windupStep <= 0f)
                {
                    throw new InvalidOperationException(
                        "G07 cleanup encountered non-finite authored cadence at sequence index "
                        + indexBefore + ".");
                }

                emitter.Tick(initialStep);
                if (!emitter.IsWindupActive || emitter.CurrentPattern != expected)
                {
                    throw new InvalidOperationException(
                        "G07 cleanup Tick did not enter the expected authored windup at index "
                        + indexBefore + ".");
                }

                emitter.Tick(windupStep);
                int expectedNext = (indexBefore + 1) % authoredPatternSequence.Length;
                if (emitter.IsWindupActive
                    || emitter.CurrentPatternSequenceIndex != expectedNext)
                {
                    throw new InvalidOperationException(
                        "G07 cleanup Tick did not advance the authored sequence from "
                        + indexBefore + " to " + expectedNext + ".");
                }

                emitter.SetFiringEnabled(false);
            }

            emitter.SetFiringEnabled(false);
            if (emitter.CurrentPatternSequenceIndex != 0
                || emitter.IsWindupActive
                || emitter.ActiveProjectileCount != 0)
            {
                throw new InvalidOperationException(
                    "G07 could not advance the authored emitter sequence to index zero through public Tick.");
            }
        }

        private void FinalizeEmitterAuthoredIdleWithoutAdvancing()
        {
            if (emitter == null)
            {
                return;
            }

            // This is the synchronous/lifecycle-safe edge: it never emits a
            // wave. A nonzero index requires RestoreAfterRecording's coroutine
            // normalization before this final dormant queue can be restored.
            emitter.SetFiringEnabled(false);
            emitter.ConfigureSpawnOrigins(authoredSpawnOrigins);
            if (emitter.CurrentPatternSequenceIndex != 0
                || emitter.IsWindupActive
                || emitter.ActiveProjectileCount != 0
                || !SpawnOriginOrderMatchesSnapshot()
                || !emitter.QueuePriorityPatternForNextFiringWindow(curtain, 1))
            {
                throw new InvalidOperationException(
                    "G07 final emitter restore requires sequence index zero and could not establish the dormant one-wave Curtain.");
            }
        }

        private void ResetPresentationAndEmitterForLifecycle()
        {
            // This path is forbidden to successful proof and is never reachable
            // from the recording region. Disabling the product presentation
            // subscribers prevents the emergency sequence reset from fabricating
            // additional cues while Unity is unable to run natural settle frames.
            if (visualCue != null)
            {
                visualCue.enabled = false;
            }

            if (telegraph != null)
            {
                telegraph.enabled = false;
            }

            if (cameraCue != null)
            {
                cameraCue.enabled = false;
            }

            if (motion != null)
            {
                motion.enabled = false;
            }

            Exception failure = null;
            try
            {
                CaptureRestoreFailure(ref failure, () =>
                {
                    emitter?.SetFiringEnabled(false);
                    if (handoffStateCaptured)
                    {
                        if (authoredPatternSequence == null
                            || authoredPatternSequence.Length != 6
                            || authoredPatternSequence.Any(value => value == null)
                            || authoredWavesPerPattern != 1)
                        {
                            throw new InvalidOperationException(
                                "G07 lifecycle reset lost the exact authored sequence snapshot.");
                        }

                        emitter.ConfigurePatternSequence(
                            authoredPatternSequence,
                            authoredWavesPerPattern);
                        emitter.SetFiringEnabled(false);
                        emitter.ConfigureSpawnOrigins(authoredSpawnOrigins);
                        if (!emitter.QueuePriorityPatternForNextFiringWindow(curtain, 1))
                        {
                            throw new InvalidOperationException(
                                "G07 lifecycle reset could not restore the dormant Curtain queue.");
                        }
                    }
                });
                CaptureRestoreFailure(ref failure, () =>
                    visualCue?.CuePlayer?.StopAllActiveCuesForReview());
                CaptureRestoreFailure(ref failure, () => motion?.RestoreOriginalPose());
                CaptureRestoreFailure(ref failure, () =>
                {
                    if (preparedStateCaptured && gameplayCamera != null)
                    {
                        gameplayCamera.transform.SetPositionAndRotation(
                            savedCameraPosition,
                            savedCameraRotation);
                        gameplayCamera.fieldOfView = savedCameraFieldOfView;
                        actionCamera?.PrimeFromHandoffCamera(gameplayCamera);
                    }
                });
            }
            finally
            {
                CaptureRestoreFailure(ref failure, () =>
                {
                    if (visualCue != null)
                    {
                        visualCue.enabled = savedVisualCueEnabled;
                    }

                    if (telegraph != null)
                    {
                        telegraph.enabled = savedTelegraphEnabled;
                    }

                    if (cameraCue != null)
                    {
                        cameraCue.enabled = savedCameraCueEnabled;
                    }

                    if (motion != null)
                    {
                        motion.enabled = savedMotionEnabled;
                    }
                });
            }

            if (failure != null)
            {
                throw new InvalidOperationException(
                    "G07 lifecycle presentation/emitter hard reset encountered an error.",
                    failure);
            }
        }

        private bool SpawnOriginOrderMatchesSnapshot()
        {
            if (emitter == null
                || authoredSpawnOrigins == null
                || authoredSpawnOrigins.Length != 6
                || emitter.ConfiguredSpawnOriginCount != authoredSpawnOrigins.Length)
            {
                return false;
            }

            var serialized = new SerializedObject(emitter);
            SerializedProperty origins = serialized.FindProperty("projectileSpawnOrigins");
            if (origins == null || !origins.isArray
                || origins.arraySize != authoredSpawnOrigins.Length)
            {
                return false;
            }

            for (int index = 0; index < authoredSpawnOrigins.Length; index++)
            {
                if (origins.GetArrayElementAtIndex(index).objectReferenceValue
                    != authoredSpawnOrigins[index])
                {
                    return false;
                }
            }

            return true;
        }

        private void RecordCleanupFailure(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            cleanupFailure = CombineExceptions(cleanupFailure, exception);
        }

        private static Exception CombineExceptions(Exception first, Exception next)
        {
            if (first == null)
            {
                return next;
            }

            if (next == null || ReferenceEquals(first, next))
            {
                return first;
            }

            return new AggregateException(first, next);
        }

        private void RestorePlayerPose()
        {
            if (!restorableStateCaptured || movement == null)
            {
                return;
            }

            CharacterController controller = movement.GetComponent<CharacterController>();
            bool enabled = controller != null && controller.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }

            movement.transform.SetPositionAndRotation(
                savedPlayerPosition,
                savedPlayerRotation);
            movement.transform.localScale = savedPlayerScale;
            if (controller != null)
            {
                controller.enabled = enabled;
            }

            Physics.SyncTransforms();
        }

        private void RestoreBossPose()
        {
            if (!restorableStateCaptured
                || pressurePosition == null
                || pressurePosition.MovedTransform == null)
            {
                return;
            }

            pressurePosition.SetMovementEnabled(false);
            Transform boss = pressurePosition.MovedTransform;
            boss.SetPositionAndRotation(savedBossPosition, savedBossRotation);
            boss.localScale = savedBossScale;
            pressurePosition.SetMovementEnabled(savedPressureMovementEnabled);
        }

        private void RestoreCameraPose()
        {
            if (!restorableStateCaptured || gameplayCamera == null)
            {
                return;
            }

            gameplayCamera.transform.SetPositionAndRotation(
                savedCameraPosition,
                savedCameraRotation);
            gameplayCamera.fieldOfView = savedCameraFieldOfView;
            actionCamera.PrimeFromHandoffCamera(gameplayCamera);
        }

        private void HandleTransitionCompleted()
        {
            transitionCompletedEventCount++;
        }

        private void HandleWindupStarted(
            BossBarrageEmitter source,
            BossBarragePatternProfile pattern)
        {
            windupEventCount++;
            if (nextExpectedEventIndex == 0
                && pattern == curtain
                && source.CurrentPatternIsPriority)
            {
                curtainWindupFrame = currentFrame;
                curtainWindupPatternId = pattern.PatternId;
                nextExpectedEventIndex = 1;
                return;
            }

            if (nextExpectedEventIndex == 2
                && pattern == hover
                && !source.CurrentPatternIsPriority
                && source.CurrentPatternSequenceIndex == 0)
            {
                hoverWindupFrame = currentFrame;
                hoverWindupPatternId = pattern.PatternId;
                hoverPreviewCount = source.BuildPendingLaneTargetPreview(hoverPreview);
                if (hoverPreviewCount
                    != AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount)
                {
                    throw new InvalidOperationException(
                        "G07 Hover windup did not expose the authored four-point preview.");
                }

                float sum = 0f;
                for (int index = 0; index < hoverPreviewCount; index++)
                {
                    sum += hoverPreview[index].x;
                }

                hoverPreviewAverageLateral = sum / hoverPreviewCount;
                float playerLateral = lane.GetLaneCoordinates(
                    movement.transform.position).x;
                float oppositeSign = hoverPreviewAverageLateral >= playerLateral
                    ? -1f
                    : 1f;
                expectedHoverWorldDirection =
                    (lane.transform.right * oppositeSign).normalized;
                nextExpectedEventIndex = 3;
                return;
            }

            throw new InvalidOperationException(
                $"G07 observed unexpected windup #{windupEventCount} at f{currentFrame}: "
                + (pattern != null ? pattern.PatternId : "<null>"));
        }

        private void HandleWaveFired(
            BossBarrageEmitter source,
            BossBarragePatternProfile pattern,
            int spawnedCount)
        {
            waveEventCount++;
            if (nextExpectedEventIndex == 1
                && pattern == curtain
                && source.LastFiredWaveWasPriority)
            {
                curtainFireFrame = currentFrame;
                curtainFirePatternId = pattern.PatternId;
                curtainSpawnedCount = spawnedCount;
                curtainWasPriority = source.LastFiredWaveWasPriority;
                nextExpectedEventIndex = 2;
                return;
            }

            if (nextExpectedEventIndex == 3
                && pattern == hover
                && !source.LastFiredWaveWasPriority
                && source.CurrentPatternSequenceIndex == 0)
            {
                hoverFireFrame = currentFrame;
                hoverFirePatternId = pattern.PatternId;
                hoverSpawnedCount = spawnedCount;
                hoverWasPriority = source.LastFiredWaveWasPriority;
                nextExpectedEventIndex = 4;
                return;
            }

            throw new InvalidOperationException(
                $"G07 observed unexpected wave #{waveEventCount} at f{currentFrame}: "
                + (pattern != null ? pattern.PatternId : "<null>"));
        }

        private void HandleRunStarted() => runStartedCount++;
        private void HandleStopSettleStarted() => stopSettleCount++;
        private void HandleBasicVolley(BossBasicFireEmitter source, int count) =>
            basicVolleyEventCount++;
        private void HandlePressureAction(
            BossPressureActionDirector source,
            BossPressureActionKind kind,
            BossBarragePatternProfile pattern,
            int tier) => pressureActionEventCount++;
        private void HandlePlayerDamaged(DamageInfo info) => playerDamageEventCount++;
        private void HandleBossDamaged(DamageInfo info) => bossDamageEventCount++;
        private void HandlePlayerBasicStarted(int combo) => playerBasicStartedCount++;
        private void HandlePlayerBasicHit(int combo) => playerBasicHitCount++;
        private void HandleDodgeStarted() => dodgeStartedCount++;
        private void HandleDodgeEnded() => dodgeEndedCount++;
        private void HandlePerfectDodge(DamageInfo info) => perfectDodgeCount++;
        private void HandleSummonUsed(int tier) => summonUsedCount++;
        private void HandleSummonBlocked(int tier) => summonBlockedCount++;
        private void HandleSummonUseBlocked() => summonUseBlockedCount++;

        private void Fail(Exception exception)
        {
            Failure ??= exception;
            IsRunning = false;
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
                firstFailure = CombineExceptions(firstFailure, exception);
            }
        }

        private static WaitUntil WaitForNextPlayerFrame()
        {
            int frame = Time.frameCount;
            return new WaitUntil(() => Time.frameCount != frame);
        }
    }
}
