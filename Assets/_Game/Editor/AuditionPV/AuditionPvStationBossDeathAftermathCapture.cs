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
using DimensionBrawl.UI.StageClear;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Deterministic G08 contract plus the product-only shot director. Preparation
    /// enters Station through the public canonical Corridor handoff and uses two
    /// strictly non-lethal setup hits to reach Phase2/12 HP. During logical
    /// f0..f359 the only gameplay action issued by this director is one public
    /// PlayerRangedBasicAttackAction.TryFire at f1. The pooled physical projectile
    /// must naturally sweep into the authored boss collider at f62.
    /// </summary>
    public static class AuditionPvStationBossDeathAftermathCapture
    {
        internal const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        internal const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        internal const string StageClearScenePath =
            "Assets/_Game/Scenes/UI/UI_StageClear.unity";
        internal const string TransitionOverlayPrefabPath =
            "Assets/_Game/UI/Transitions/PF_UI_TransitionOverlay.prefab";
        internal const string StageClearSceneName = "UI_StageClear";
        internal const string StationEntryConditionId = "corridor.tutorial.completed";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationBossDeathAftermathCapture.cs";
        internal const string ProductBaseCommit =
            "adc67879016100365d935c30e36849a145da1f81";

        internal const string ShotId = "g08";
        internal const string BaselinesFolderName = "baselines";
        internal const string Bl10FileName =
            "BL10_AKAZA_BOSS_DEATH_IMPACT__HUDON__t01.033333.png";
        internal const string Bl11FileName =
            "BL11_AKAZA_BOSS_DEATH_AFTERMATH__HUDON__t01.933333.png";
        internal const string Bl12FileName =
            "BL12_OLYMPUS_STATION_CLEAR_RESULT__HUDON__t04.100000.png";

        internal const int FirstFrame = 0;
        internal const int LastFrame = 359;
        internal const int ExpectedFrameCount = 360;
        internal const int FireFrame = 1;
        internal const int ImpactFrame = 62;
        internal const int AftermathHeroFrame = 116;
        internal const int DeathHoldProofFrame = 129;
        internal const int ResultRequestFrame = 218;
        internal const int InteractiveResultFrame = 246;
        internal const int DeterministicRandomSeed = 0x4808;
        internal const float PreparedBossHealth = 12f;
        internal const float AuthoredProjectileDamage = 12f;
        internal const float AuthoredProjectileSpeed = 24f;
        internal const float AuthoredProjectileRadius = 0.31f;
        internal const float NaturalImpactTargetDistance = 24.2f;
        internal const float NaturalImpactDistanceTolerance = 0.12f;
        internal const int PhaseTwoSettleFrames = 90;
        internal const int PostRecordingSettleFrameBudget = 30;

        internal static int PredictNaturalImpactFrame(float sweepDistance)
        {
            if (!float.IsFinite(sweepDistance) || sweepDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sweepDistance));
            }

            float travelPerFrame = AuthoredProjectileSpeed
                / AuditionPvCaptureContract.Fps;
            return FireFrame + Mathf.CeilToInt(sweepDistance / travelPerFrame);
        }

        internal static AuditionPvShotManifestEntry CreateShotManifestEntry()
        {
            return new AuditionPvShotManifestEntry
            {
                id = ShotId,
                scenePath = StationScenePath,
                startFrame = FirstFrame,
                endFrame = LastFrame,
                expectedFrameCount = ExpectedFrameCount,
                hudMode = "hud-on-to-result",
                notes =
                    "Canonical CorridorActive run: SkipIntroCutscene, public tutorial-route seal, "
                    + "public single-load segment seal/handoff, dedicated Station FromHandoffPending receipt. "
                    + "Entry guide reaches Released; real non-lethal threshold damage and one public "
                    + "TrySkipTransition reach Phase2; a second strictly non-lethal setup hit leaves 12 HP. "
                    + "Logical f1 calls only PlayerRangedBasicAttackAction.TryFire. The authored 12-damage, "
                    + "24m/s pooled LaneActionProjectile naturally impacts at f62, producing real Died, "
                    + "BossTerminal clear, the 2.6s unscaled aftermath, freeze/result request f218, and "
                    + "interactive committed result f246. BL10=f62, BL11=f116, BL12=f246 byte-exact."
            };
        }

        internal static AuditionPvBaselineManifestEntry[] CreateBaselineManifestEntries()
        {
            return new[]
            {
                Baseline("bl10", ImpactFrame, Bl10FileName),
                Baseline("bl11", AftermathHeroFrame, Bl11FileName),
                Baseline("bl12", InteractiveResultFrame, Bl12FileName)
            };
        }

        private static AuditionPvBaselineManifestEntry Baseline(
            string id,
            int sourceFrame,
            string fileName)
        {
            return new AuditionPvBaselineManifestEntry
            {
                id = id,
                shotId = ShotId,
                sourceFrame = sourceFrame,
                fileName = fileName,
                hudMode = sourceFrame < ResultRequestFrame
                    ? "hud-on"
                    : "authored-result",
                status = "captured"
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

        internal static string[] ExplicitProductDependencyPaths()
        {
            return new[]
            {
                CorridorScenePath,
                StationScenePath,
                StageClearScenePath,
                TransitionOverlayPrefabPath,
                CaptureScriptPath,
                "Assets/_Game/Scripts/Combat/CombatHealth.cs",
                "Assets/_Game/Scripts/Combat/CombatEncounterController.cs",
                "Assets/_Game/Scripts/Combat/EncounterTerminalResolutionCoordinator.cs",
                "Assets/_Game/Scripts/Combat/LaneActionProjectile.cs",
                "Assets/_Game/Scripts/LevelDesign/StageRunRuntime.cs",
                "Assets/_Game/Scripts/LevelDesign/StageRunHandoff.cs",
                "Assets/_Game/Scripts/LevelDesign/StageRunFacts.cs",
                "Assets/_Game/Scripts/LevelDesign/OlympusCorridorCombatFlowController.cs",
                "Assets/_Game/Scripts/LevelDesign/OlympusStationAkazaPhase2FlowController.cs",
                "Assets/_Game/Scripts/LevelDesign/OlympusStationBossTerminalAftermathPresenter.cs",
                "Assets/_Game/Scripts/LevelDesign/OlympusStationCombatResultPresenter.cs",
                "Assets/_Game/Scripts/LevelDesign/OlympusStageClearOverlay.cs",
                "Assets/_Game/Scripts/Player/PlayerRangedBasicAttackAction.cs",
                "Assets/_Game/Scripts/Player/PlayerRangedProjectilePool.cs",
                "Assets/_Game/Scripts/Player/PlayerLockTargetController.cs",
                "Assets/_Game/Scripts/Player/PlayerInputLockSource.cs",
                "Assets/_Game/Scripts/Presentation/BossBarrageCameraCueDriver.cs",
                "Assets/_Game/Scripts/Presentation/BossBarrageVisualCueDriver.cs",
                "Assets/_Game/Scripts/Presentation/AkazaPhase2CombatMotionDriver.cs",
                "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.cs",
                "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Timing.cs",
                "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Camera.cs",
                "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Signals.cs",
                "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Bindings.cs",
                "Assets/_Game/Scripts/UI/StageClear/StageClearScreenPresenter.cs",
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset",
                "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller",
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab"
            };
        }

        internal static AuditionPvStationBossDeathAftermathOutput ReserveNewOutput(
            DateTime startedAtUtc,
            AuditionPvGitSnapshot gitSnapshot = null)
        {
            AuditionPvGitSnapshot git = gitSnapshot
                ?? AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "G08 output reservation requires a successful Git provenance probe: "
                    + git.probeError);
            }

            string outputId = AuditionPvOutputPaths.CreateOutputId(
                "g08-station-boss-death-aftermath",
                startedAtUtc,
                git.commitSha,
                git.isDirty,
                git.dirtyStateHashSha256);
            return ReserveNewOutputForRoot(AuditionPvCaptureContract.OutputRoot, outputId);
        }

        internal static AuditionPvStationBossDeathAftermathOutput ReserveNewOutputForRoot(
            string outputRoot,
            string outputId,
            Action<string> createBaselineDirectory = null,
            Func<string, AuditionPvRecorderSettingsBundle> createRecorderSettings = null)
        {
            string outputDirectory =
                AuditionPvOutputPaths.CreateUniqueOutputDirectory(outputRoot, outputId);
            string captureId = new DirectoryInfo(outputDirectory).Name;
            string baselineDirectory = Path.Combine(
                outputDirectory,
                BaselinesFolderName);
            AuditionPvRecorderSettingsBundle recorderSettings = null;
            try
            {
                (createBaselineDirectory ?? (path => Directory.CreateDirectory(path)))(
                    baselineDirectory);
                recorderSettings = createRecorderSettings != null
                    ? createRecorderSettings(outputDirectory)
                    : AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence(
                        outputDirectory,
                        ShotId);
                AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
                return new AuditionPvStationBossDeathAftermathOutput(
                    captureId,
                    outputDirectory,
                    baselineDirectory,
                    recorderSettings);
            }
            catch
            {
                recorderSettings?.Dispose();
                CleanupFailedReservationForRoot(
                    outputRoot,
                    captureId,
                    outputDirectory);
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
                    "Refused to remove a failed G08 reservation outside its exact output-root child.");
            }

            if (Directory.Exists(actual))
            {
                Directory.Delete(actual, recursive: true);
            }
        }

        internal static AuditionPvStationBossDeathAftermathDirector
            AttachToFreshCorridorScene()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "The G08 product-state director can only run in Play Mode.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(scene.path, CorridorScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 requires a fresh canonical Corridor PlayMode scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<
                    AuditionPvStationBossDeathAftermathDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The active PlayMode session already owns a G08 director.");
            }

            var root = new GameObject("[AuditionPV_G08_BossDeathAftermathDirector]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            UnityEngine.Object.DontDestroyOnLoad(root);
            return root.AddComponent<AuditionPvStationBossDeathAftermathDirector>();
        }

        internal static void ReopenCorridorAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before reopening the G08 Corridor scene.");
            }

            EditorSceneManager.OpenScene(CorridorScenePath, OpenSceneMode.Single);
        }
    }

    internal sealed class AuditionPvStationBossDeathAftermathOutput : IDisposable
    {
        public readonly string captureId;
        public readonly string outputRoot;
        public readonly string outputDirectory;
        public readonly string baselineDirectory;
        public readonly AuditionPvRecorderSettingsBundle recorderSettings;

        internal AuditionPvStationBossDeathAftermathOutput(
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

        public void Dispose() => recorderSettings.Dispose();
    }

    [DefaultExecutionOrder(-32000)]
    public sealed class AuditionPvStationBossDeathAftermathDirector : MonoBehaviour
    {
        private const double RouteTimeoutSeconds = 12d;
        private const double EntryGuideTimeoutSeconds = 10d;
        private const double PhaseTwoTimeoutSeconds = 8d;
        private const float Tolerance = 0.001f;

        private OlympusCorridorCombatFlowController corridorFlow;
        private StageRunContext runContext;
        private OlympusStationAkazaPhase2FlowController phaseFlow;
        private CombatEncounterController encounter;
        private CombatHealth playerHealth;
        private CombatHealth bossHealth;
        private PlayerMovementController movement;
        private PlayerActionController action;
        private PlayerRangedBasicAttackAction ranged;
        private PlayerSkill1Action skill;
        private PlayerSummonSlot1Action summon;
        private PlayerSupportSummonSlotAction[] support =
            Array.Empty<PlayerSupportSummonSlotAction>();
        private PlayerCombatModeController combatMode;
        private PlayerLockTargetController lockTarget;
        private BossSummonPressureAction bossPressureAction;
        private BossPressurePositionController bossPressurePosition;
        private OlympusStationBossTerminalAftermathPresenter aftermath;
        private OlympusStationCombatResultPresenter resultPresenter;
        private OlympusStageClearOverlay overlay;
        private BossBarrageCameraCueDriver deathCamera;
        private BossBarrageVisualCueDriver deathVisual;
        private AkazaPhase2CombatMotionDriver deathMotion;
        private ActionCinematicCueDirector cinematic;
        private UISceneTransitionHandoffOwner transitionOwner;
        private GameObject captureOwnedTransitionRoot;
        private CanvasGroup combatHud;
        private ICombatEntryGuideGate entryGuide;
        private LaneActionProjectile firedProjectile;
        private StageClearScreenPresenter clearPresenter;
        private PresentationClock.ManualLease presentationClockLease;
        private IDisposable cadenceSuspensionLease;

        private UnityEngine.Random.State savedRandom;
        private string savedRandomJson = string.Empty;
        private bool savedRandomValid;
        private int savedCaptureFramerate;
        private int savedTargetFrameRate;
        private float savedFixedDeltaTime;
        private bool globalStateCaptured;
        private bool restoring;
        private bool stateRestored;
        private bool eventsSubscribed;
        private Exception cleanupFailure;
        private string transitionFailure = string.Empty;
        private bool transitionDestinationArrived;
        private bool transitionHandoffCompleted;
        private bool transitionEventsSubscribed;
        private uint transitionGeneration;

        private int currentFrame = -1;
        private int nextPresentedFrame;
        private int rangedFireStartedCount;
        private int projectileFiredCount;
        private int projectileDamageAppliedCount;
        private int bossDamagedDuringShotCount;
        private int bossDiedCount;
        private int encounterTerminalResolvedCount;
        private int overlayPresentationSucceededCount;
        private int aftermathStartedCount;
        private int aftermathCompletedCount;
        private int eventSequence;
        private int projectileFiredSequence;
        private int bossDiedSequence;
        private int projectileImpactSequence;
        private int terminalResolvedSequence;
        private int fireFrame = -1;
        private int projectileFiredFrame = -1;
        private int bossDiedFrame = -1;
        private int projectileImpactFrame = -1;
        private int terminalResolvedFrame = -1;
        private int firstFreezeFrame = -1;
        private int firstResultSceneFrame = -1;
        private int firstResultConfiguredFrame = -1;
        private int firstInteractiveFrame = -1;
        private int aftermathCompletedFrame = -1;
        private int inputLeaseReleasedFrame = -1;
        private int deathStateHeldFrame = -1;
        private int pressureScreensBeforeDismiss;
        private int pressureSummonsDismissed;
        private int pressureScreensAfterDismiss = -1;
        private int predictedNaturalImpactFrame = -1;
        private bool bossPressureMovementOwnershipAcquired;
        private bool bossPressureMovementLeaseActive;
        private bool savedBossPressureMovementEnabled;
        private bool bossPressureMovementHeldForShot;
        private bool bossPressureMovementRestoredExactly;
        private bool bossPoseTrackingArmed;
        private bool bossPoseStableThroughImpact;
        private bool physicalProjectileObservedActiveBeforeImpact;
        private bool projectileMovedBeforeImpact;
        private bool noEarlyFreeze = true;
        private bool resultAbsentBeforeRequest = true;
        private bool allEightLocksObservedAtImpact;
        private bool allEightLocksReleasedAtResult;
        private bool deathStateAtAftermathHero;
        private Vector3 projectileSpawnPosition;
        private Vector3 projectilePositionAtFrame61;
        private Vector3 projectileImpactPoint;
        private Vector3 projectileImpactDirection;
        private Vector3 bossPositionAtShotArm;
        private Vector3 bossPositionAtImpact;
        private Quaternion bossRotationAtShotArm = Quaternion.identity;
        private Quaternion bossRotationAtImpact = Quaternion.identity;
        private int projectileInstanceId;
        private float bossHealthBeforeShot;
        private float predictedBossSweepDistance;
        private float preShotPlayerPlanarStepDistance;
        private float maximumBossPositionDriftThroughImpact;
        private float maximumBossRotationDriftThroughImpact;
        private float minimumObservedTimeScale = float.PositiveInfinity;
        private float maximumObservedTimeScale = float.NegativeInfinity;
        private readonly RaycastHit[] naturalImpactSweepHits = new RaycastHit[32];

        public event Action<int> FramePresented;

        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public Exception Failure { get; private set; }
        public Exception CleanupFailure => cleanupFailure;
        public int CurrentFrame => currentFrame;
        public int LastPresentedFrame { get; private set; } = -1;
        public Camera GameplayCamera => deathCamera != null
            ? deathCamera.CameraController?.GetComponent<Camera>()
            : null;
        public Transform PlayerRendererRoot => movement != null ? movement.transform : null;
        public Transform BossRendererRoot => bossHealth != null ? bossHealth.transform : null;
        public StageClearScreenPresenter ClearPresenter => clearPresenter;

        public string RunId { get; private set; } = string.Empty;
        public string PlayableStageId { get; private set; } = string.Empty;
        public int RouteRevision { get; private set; }
        public string RouteDigest { get; private set; } = string.Empty;
        public string TransitionTokenId { get; private set; } = string.Empty;
        public string TransitionTokenDigest { get; private set; } = string.Empty;
        public long LoaderGeneration { get; private set; }
        public string SegmentEntryReceiptId { get; private set; } = string.Empty;
        public string SegmentEntryReceiptDigest { get; private set; } = string.Empty;
        public string HandoffTerminalReceiptId { get; private set; } = string.Empty;
        public string HandoffTerminalReceiptDigest { get; private set; } = string.Empty;
        public bool EnteredFromHandoffPending { get; private set; }
        public bool ExactHandoffReceiptChain { get; private set; }
        public bool ProductTransitionProviderObserved { get; private set; }
        public bool ProductTransitionDestinationArrived => transitionDestinationArrived;
        public bool ProductTransitionHandoffCompleted => transitionHandoffCompleted;
        public uint ProductTransitionGeneration => transitionGeneration;
        public bool EntryGuideObservedPlaying { get; private set; }
        public bool EntryGuideReleased { get; private set; }
        public int PhaseTransitionStartCount { get; private set; }
        public int PhaseTransitionCompletionCount { get; private set; }
        public bool PhaseTwoApplied { get; private set; }
        public float PreparedHealthObserved { get; private set; }
        public int FireFrame => fireFrame;
        public int ProjectileFiredFrame => projectileFiredFrame;
        public int BossDiedFrame => bossDiedFrame;
        public int ProjectileImpactFrame => projectileImpactFrame;
        public int TerminalResolvedFrame => terminalResolvedFrame;
        public int FirstFreezeFrame => firstFreezeFrame;
        public int FirstResultSceneFrame => firstResultSceneFrame;
        public int FirstResultConfiguredFrame => firstResultConfiguredFrame;
        public int FirstInteractiveFrame => firstInteractiveFrame;
        public int AftermathCompletedFrame => aftermathCompletedFrame;
        public int InputLeaseReleasedFrame => inputLeaseReleasedFrame;
        public int DeathStateHeldFrame => deathStateHeldFrame;
        public int RangedFireStartedCount => rangedFireStartedCount;
        public int ProjectileFiredCount => projectileFiredCount;
        public int ProjectileDamageAppliedCount => projectileDamageAppliedCount;
        public int BossDamagedDuringShotCount => bossDamagedDuringShotCount;
        public int BossDiedCount => bossDiedCount;
        public int EncounterTerminalResolvedCount => encounterTerminalResolvedCount;
        public int OverlayPresentationSucceededCount => overlayPresentationSucceededCount;
        public int AftermathStartedCount => aftermathStartedCount;
        public int AftermathCompletedCount => aftermathCompletedCount;
        public int ProjectileInstanceId => projectileInstanceId;
        public int ProjectileFiredSequence => projectileFiredSequence;
        public int BossDiedSequence => bossDiedSequence;
        public int ProjectileImpactSequence => projectileImpactSequence;
        public int TerminalResolvedSequence => terminalResolvedSequence;
        public Vector3 ProjectileSpawnPosition => projectileSpawnPosition;
        public Vector3 ProjectilePositionAtFrame61 => projectilePositionAtFrame61;
        public Vector3 ProjectileImpactPoint => projectileImpactPoint;
        public Vector3 ProjectileImpactDirection => projectileImpactDirection;
        public bool PhysicalProjectileObservedActiveBeforeImpact =>
            physicalProjectileObservedActiveBeforeImpact;
        public bool ProjectileMovedBeforeImpact => projectileMovedBeforeImpact;
        public bool NoEarlyFreeze => noEarlyFreeze;
        public bool ResultAbsentBeforeRequest => resultAbsentBeforeRequest;
        public bool AllEightLocksObservedAtImpact => allEightLocksObservedAtImpact;
        public bool AllEightLocksReleasedAtResult => allEightLocksReleasedAtResult;
        public bool DeathStateAtAftermathHero => deathStateAtAftermathHero;
        public float BossHealthBeforeShot => bossHealthBeforeShot;
        public int PressureScreensBeforeDismiss => pressureScreensBeforeDismiss;
        public int PressureSummonsDismissed => pressureSummonsDismissed;
        public int PressureScreensAfterDismiss => pressureScreensAfterDismiss;
        public float PredictedBossSweepDistance => predictedBossSweepDistance;
        public int PredictedNaturalImpactFrame => predictedNaturalImpactFrame;
        public float PreShotPlayerPlanarStepDistance =>
            preShotPlayerPlanarStepDistance;
        public bool BossPressureMovementWasEnabled =>
            savedBossPressureMovementEnabled;
        public bool BossPressureMovementHoldAcquired =>
            bossPressureMovementOwnershipAcquired
            && bossPressureMovementLeaseActive
            && bossPressureMovementHeldForShot
            && bossPressurePosition != null
            && !bossPressurePosition.MovementEnabled;
        public bool BossPoseStableThroughImpact => bossPoseStableThroughImpact;
        public Vector3 BossPositionAtShotArm => bossPositionAtShotArm;
        public Vector3 BossPositionAtImpact => bossPositionAtImpact;
        public float MaximumBossPositionDriftThroughImpact =>
            maximumBossPositionDriftThroughImpact;
        public float MaximumBossRotationDriftThroughImpact =>
            maximumBossRotationDriftThroughImpact;
        public float MinimumObservedTimeScale => minimumObservedTimeScale;
        public float MaximumObservedTimeScale => maximumObservedTimeScale;
        public bool StateRestored => stateRestored;
        public bool EventsReleased => stateRestored && !eventsSubscribed;
        public bool PresentationClockReleased => stateRestored
            && !PresentationClock.IsManuallyDriven;
        public bool CadenceReleased => stateRestored
            && BossCombatCadenceScheduler.ExternalSuspensionCount == 0;
        public bool BossPressureMovementRestored => stateRestored
            && !bossPressureMovementLeaseActive
            && (!bossPressureMovementOwnershipAcquired
                || (bossPressureMovementRestoredExactly
                    && bossPressurePosition != null
                    && bossPressurePosition.MovementEnabled
                        == savedBossPressureMovementEnabled));
        public bool TransitionCaptureStateReleased => stateRestored
            && !transitionEventsSubscribed
            && captureOwnedTransitionRoot == null;
        public bool GlobalCaptureStateRestored => stateRestored
            && Time.captureFramerate == savedCaptureFramerate
            && Application.targetFrameRate == savedTargetFrameRate
            && Mathf.Abs(Time.fixedDeltaTime - savedFixedDeltaTime) <= 0.000001f;

        public int BossDeathCameraRequestCount => deathCamera?.BossDeathCueRequestCount ?? -1;
        public int BossDeathCameraVersion => deathCamera?.BossDeathCameraCueVersion ?? -1;
        public bool BossDeathCameraInterrupted => deathCamera == null
            || deathCamera.BossDeathCueWasInterrupted;
        public bool BossDeathCameraComplete => deathCamera != null
            && deathCamera.IsBossDeathCueComplete;
        public int BossDeathVfxRequestCount =>
            deathVisual?.BossDeathWorldVfxCueRequestCount ?? -1;
        public int BossDeathAudioSourceDelta =>
            deathVisual?.BossDeathProfileAudioSourceDelta ?? -1;
        public bool BossDeathUsesPhaseTwoAnchor => deathVisual != null
            && deathVisual.LastBossDeathCueAnchor == deathVisual.PulseRoot
            && deathVisual.PulseRoot != null
            && Vector3.Distance(
                deathVisual.LastBossDeathCueWorldPosition,
                deathVisual.PulseRoot.position) <= 0.001f;
        public int DeathMotionRequestCount => deathMotion?.DeathRequestCount ?? -1;
        public bool MotionIsDead => deathMotion != null && deathMotion.IsDead;
        public bool MotionAttacksStopped => deathMotion != null && deathMotion.AttacksStopped;
        public bool AnimatorInDeathState => deathVisual?.Animator != null
            && deathVisual.Animator.GetCurrentAnimatorStateInfo(0).IsName("Death");
        public bool AftermathCompletedSuccessfully => aftermath != null
            && aftermath.CompletedSuccessfully;
        public string AftermathLastError => aftermath?.LastError ?? string.Empty;
        public string AftermathQualityWarning => aftermath?.LastQualityWarning ?? string.Empty;
        public bool AftermathScaleOneObserved => aftermath != null
            && aftermath.ScaleOneObserved;
        public bool AftermathScaleOneViolated => aftermath == null
            || aftermath.ScaleOneViolationRecorded;
        public int AftermathBeginCount => aftermath?.BeginCount ?? -1;
        public int AftermathCompleteCount => aftermath?.CompleteCount ?? -1;
        public float AftermathElapsedSeconds => aftermath?.ElapsedUnscaledSeconds ?? -1f;
        public bool OverlayShown => overlay != null && overlay.IsShown;
        public bool OverlayFrozen => overlay != null && overlay.IsWorldFrozenForResult;
        public bool ResultSummarySameInstance { get; private set; }
        public bool PresentedSummarySameInstance { get; private set; }
        public string CommittedSummaryDigest { get; private set; } = string.Empty;
        public string PresentedSummaryDigest { get; private set; } = string.Empty;
        public string OutcomeFactDigest { get; private set; } = string.Empty;
        public long RootAdmissionSequence { get; private set; }
        public long TerminalEpoch { get; private set; }
        public string TerminalEpochEvidenceDigest { get; private set; } = string.Empty;
        public string TerminalClosureDigest { get; private set; } = string.Empty;
        public int TerminalRecordReceiptCount { get; private set; }
        public bool TerminalFactsExact { get; private set; }
        public bool HudWasActiveAtFire { get; private set; }
        public bool HudWasActiveAtImpact { get; private set; }
        public bool HudYieldedAtResult { get; private set; }
        public bool ResultInteractiveAt246 { get; private set; }

        public IEnumerator PrepareFreshProductState()
        {
            if (IsPrepared || IsRunning || stateRestored)
            {
                throw new InvalidOperationException(
                    "The G08 director can be prepared exactly once.");
            }

            CaptureGlobalState();
            bool succeeded = false;
            try
            {
                yield return EnterCanonicalStation();
                ResolveStationBindings();
                yield return ReleaseEntryGuide();
                yield return PreparePhaseTwoAndHealth();
                ConfigureDeterministicPlayerShot();
                SubscribeShotEvents();

                savedRandom = UnityEngine.Random.state;
                savedRandomJson = JsonUtility.ToJson(savedRandom);
                savedRandomValid = true;
                UnityEngine.Random.InitState(
                    AuditionPvStationBossDeathAftermathCapture.DeterministicRandomSeed);
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
                Time.fixedDeltaTime = 1f / AuditionPvCaptureContract.Fps;
                presentationClockLease = PresentationClock.AcquireManual(
                    this,
                    AuditionPvCaptureContract.Fps);
                cadenceSuspensionLease =
                    BossCombatCadenceScheduler.AcquireExternalSuspension(this);

                for (int frame = 0;
                    frame < AuditionPvStationBossDeathAftermathCapture.PhaseTwoSettleFrames;
                    frame++)
                {
                    presentationClockLease.SetFrame(frame);
                    yield return WaitForNextPlayerFrame();
                }

                yield return PrepareNaturalBossImpactOwnership();
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
                        RestoreCaptureOwnedState();
                    }
                    catch (Exception exception)
                    {
                        cleanupFailure = Combine(cleanupFailure, exception);
                        Debug.LogException(exception, this);
                    }
                }
            }
        }

        public void BeginShotForRecorder()
        {
            if (!IsPrepared || IsRunning || IsComplete || stateRestored)
            {
                throw new InvalidOperationException(
                    "Prepare the fresh G08 product state exactly once before capture.");
            }

            float minimum = 1f / AuditionPvCaptureContract.Fps;
            if (Time.captureDeltaTime <= minimum
                || Time.captureDeltaTime >= minimum + 0.001f)
            {
                throw new InvalidOperationException(
                    "G08 Recorder padding is not active at logical f0.");
            }

            RevalidateNaturalBossImpactAtShotArm();
            ValidateReadyForShot();
            ArmBossPoseStabilityProof();
            presentationClockLease.SetFrame(0);
            currentFrame = AuditionPvStationBossDeathAftermathCapture.FirstFrame;
            IsRunning = true;
        }

        // RECORDING CONTRACT BEGIN. The sole gameplay action issued in this
        // region is ranged.TryFire() at logical f1. Source tests fail closed on
        // direct damage/impact, death, cue, overlay, and result-publication APIs.
        private void Update()
        {
            if (!IsRunning || Failure != null)
            {
                return;
            }

            try
            {
                presentationClockLease.SetFrame(currentFrame);
                if (currentFrame
                    == AuditionPvStationBossDeathAftermathCapture.FireFrame)
                {
                    if (!ranged.TryFire())
                    {
                        throw new InvalidOperationException(
                            "G08 public TryFire was rejected at logical f1: "
                            + ranged.LastUseBlockedReason);
                    }

                    fireFrame = currentFrame;
                }

                minimumObservedTimeScale = Mathf.Min(
                    minimumObservedTimeScale,
                    Time.timeScale);
                maximumObservedTimeScale = Mathf.Max(
                    maximumObservedTimeScale,
                    Time.timeScale);
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
                ObserveFrameState();
                if (currentFrame
                    == AuditionPvStationBossDeathAftermathCapture.LastFrame)
                {
                    ValidateCompletedShot();
                    IsRunning = false;
                    IsComplete = true;
                }

                if (currentFrame != nextPresentedFrame)
                {
                    throw new InvalidOperationException(
                        $"G08 presented f{currentFrame}; expected f{nextPresentedFrame}.");
                }

                LastPresentedFrame = currentFrame;
                FramePresented?.Invoke(currentFrame);
                nextPresentedFrame++;
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
            if (stateRestored)
            {
                yield break;
            }

            IsRunning = false;
            Exception failure = null;
            for (int frame = 0;
                frame < AuditionPvStationBossDeathAftermathCapture
                    .PostRecordingSettleFrameBudget;
                frame++)
            {
                if (clearPresenter != null
                    && clearPresenter.EntranceCompleted
                    && !clearPresenter.IsEntrancePlaying)
                {
                    break;
                }

                yield return null;
            }

            try
            {
                RestoreCaptureOwnedState();
            }
            catch (Exception exception)
            {
                failure = exception;
                cleanupFailure = Combine(cleanupFailure, exception);
            }

            if (failure != null)
            {
                throw new InvalidOperationException(
                    "G08 exhaustive capture-owned cleanup failed.",
                    failure);
            }
        }

        public void RestoreCaptureOwnedState()
        {
            if (stateRestored || restoring || !globalStateCaptured)
            {
                return;
            }

            restoring = true;
            Exception failure = null;
            try
            {
                CaptureFailure(ref failure, UnsubscribeShotEvents);
                CaptureFailure(ref failure, UnsubscribeTransitionEvents);
                CaptureFailure(ref failure, () =>
                {
                    if (firedProjectile != null)
                    {
                        firedProjectile.DamageApplied -= HandleProjectileDamageApplied;
                    }
                });
                CaptureFailure(ref failure, () =>
                {
                    cadenceSuspensionLease?.Dispose();
                    cadenceSuspensionLease = null;
                });
                CaptureFailure(ref failure, () =>
                {
                    presentationClockLease?.Dispose();
                    presentationClockLease = null;
                });
                CaptureFailure(ref failure, () => lockTarget?.ClearHardLock());
                CaptureFailure(ref failure, RestoreBossPressureMovementHold);
                CaptureFailure(ref failure, () =>
                {
                    if (captureOwnedTransitionRoot != null)
                    {
                        Destroy(captureOwnedTransitionRoot);
                        captureOwnedTransitionRoot = null;
                    }

                    transitionOwner = null;
                });
            }
            finally
            {
                CaptureFailure(ref failure, () =>
                {
                    Time.captureFramerate = savedCaptureFramerate;
                    Application.targetFrameRate = savedTargetFrameRate;
                    Time.fixedDeltaTime = savedFixedDeltaTime;
                });
                CaptureFailure(ref failure, () =>
                {
                    if (savedRandomValid)
                    {
                        UnityEngine.Random.state = savedRandom;
                        if (!string.Equals(
                            JsonUtility.ToJson(UnityEngine.Random.state),
                            savedRandomJson,
                            StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                "G08 random-state restoration was not byte-stable.");
                        }

                        savedRandomValid = false;
                    }
                });
                restoring = false;
            }

            if (failure != null)
            {
                cleanupFailure = Combine(cleanupFailure, failure);
                throw new InvalidOperationException(
                    "G08 capture-owned state restoration encountered an error.",
                    failure);
            }

            stateRestored = true;
            if (!EventsReleased
                || !PresentationClockReleased
                || !CadenceReleased
                || !BossPressureMovementRestored
                || !TransitionCaptureStateReleased
                || !GlobalCaptureStateRestored)
            {
                stateRestored = false;
                throw new InvalidOperationException(
                    "G08 event, clock, cadence, or global capture state did not restore exactly.");
            }
        }

        private IEnumerator EnterCanonicalStation()
        {
            Scene corridor = SceneManager.GetActiveScene();
            if (!corridor.IsValid()
                || !corridor.isLoaded
                || !string.Equals(
                    corridor.path,
                    AuditionPvStationBossDeathAftermathCapture.CorridorScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 preparation did not begin in the dedicated Corridor scene.");
            }

            corridorFlow = RequireSingleSceneComponent<
                OlympusCorridorCombatFlowController>(corridor);
            runContext = StageRunRuntime.ActiveContext
                ?? throw new InvalidOperationException(
                    "Corridor did not atomically admit a canonical StageRun.");
            if (runContext.LifecycleState != StageRunLifecycleState.CorridorActive)
            {
                throw new InvalidOperationException(
                    "G08 requires StageRunLifecycleState.CorridorActive before skip.");
            }

            RunId = runContext.Identity.RunId;
            PlayableStageId = runContext.Identity.PlayableStageId;
            RouteRevision = runContext.Identity.RouteRevision;
            RouteDigest = runContext.Identity.RouteSnapshotDigest;
            int corridorHandle = corridor.handle;
            EnsureProductTransitionOwner();
            SubscribeTransitionEvents();
            corridorFlow.SkipIntroCutscene();
            yield return null;
            yield return null;
            if (!string.Equals(
                corridorFlow.CanonicalStageRunId,
                RunId,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Corridor intro skip lost the canonical StageRun identity.");
            }

            yield return CompleteCorridorTutorialThroughPublicGameplay(corridor);

            double pendingDeadline = Time.realtimeSinceStartupAsDouble + RouteTimeoutSeconds;
            while (runContext.LifecycleState != StageRunLifecycleState.HandoffPending
                && Time.realtimeSinceStartupAsDouble < pendingDeadline)
            {
                yield return null;
            }

            StageRunHandoffToken pendingToken = runContext.PendingHandoffToken;
            if (runContext.TutorialRouteSummaryFact == null
                || runContext.LifecycleState != StageRunLifecycleState.HandoffPending
                || pendingToken == null)
            {
                throw new InvalidOperationException(
                    "G08 did not observe the Corridor flow's public tutorial/segment seal.");
            }

            if (runContext.LifecycleState != StageRunLifecycleState.HandoffPending
                || runContext.PendingHandoffToken != pendingToken
                || !string.Equals(
                    pendingToken.DestinationScenePath,
                    AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                    StringComparison.Ordinal)
                || !string.Equals(pendingToken.RunId, RunId, StringComparison.Ordinal)
                || !string.Equals(pendingToken.RouteDigest, RouteDigest, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 observed handoff token did not retain the exact run/route/destination identity.");
            }

            TransitionTokenId = pendingToken.TokenId;
            TransitionTokenDigest = pendingToken.CanonicalDigest;
            LoaderGeneration = pendingToken.LoaderGeneration;
            UISceneTransitionTicket activeTicket = transitionOwner.ActiveTicket;
            if (!transitionOwner.HasActiveTicket
                || !activeTicket.IsValid
                || activeTicket.RouteId != UIRouteId.Combat
                || activeTicket.SourceSceneHandle != corridorHandle
                || !string.Equals(
                    activeTicket.DestinationScenePath,
                    pendingToken.DestinationScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 Corridor flow did not hand the sealed dispatch to the real UI transition owner.");
            }

            ProductTransitionProviderObserved = UITransitionHandoffService.HasProvider;
            transitionGeneration = activeTicket.Generation;
            double deadline = Time.realtimeSinceStartupAsDouble + RouteTimeoutSeconds;
            while (!string.Equals(
                    SceneManager.GetActiveScene().path,
                    AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                    StringComparison.Ordinal)
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            Scene station = SceneManager.GetActiveScene();
            if (!station.IsValid()
                || !station.isLoaded
                || !string.Equals(
                    station.path,
                    AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                    StringComparison.Ordinal)
                || station.handle == corridorHandle
                || corridor.isLoaded
                || StageRunRuntime.ActiveContext != runContext
                || runContext.LifecycleState != StageRunLifecycleState.StationActive)
            {
                throw new InvalidOperationException(
                    "G08 did not enter the dedicated Station from the exact pending handoff.");
            }

            double completionDeadline =
                Time.realtimeSinceStartupAsDouble + RouteTimeoutSeconds;
            while (!transitionHandoffCompleted
                && string.IsNullOrEmpty(transitionFailure)
                && Time.realtimeSinceStartupAsDouble < completionDeadline)
            {
                yield return null;
            }

            if (!ProductTransitionProviderObserved
                || !transitionDestinationArrived
                || !transitionHandoffCompleted
                || !string.IsNullOrEmpty(transitionFailure))
            {
                throw new InvalidOperationException(
                    "G08 real UI transition handoff did not arrive/reveal successfully: "
                    + transitionFailure);
            }

            StageSegmentEntryReceipt entry = runContext.SegmentEntryReceipt;
            StageSegmentHandoffTerminalReceipt terminal =
                runContext.HandoffTerminalReceipt;
            if (entry == null || terminal == null)
            {
                throw new InvalidOperationException(
                    "G08 Station admission did not publish both handoff receipts.");
            }

            SegmentEntryReceiptId = entry.SegmentEntryReceiptId;
            SegmentEntryReceiptDigest = entry.CanonicalDigest;
            HandoffTerminalReceiptId = terminal.SegmentHandoffTerminalReceiptId;
            HandoffTerminalReceiptDigest = terminal.CanonicalDigest;
            EnteredFromHandoffPending = entry.FromHandoffPending;
            ExactHandoffReceiptChain = entry.FromHandoffPending
                && entry.ToDestinationActive
                && string.Equals(entry.RunId, RunId, StringComparison.Ordinal)
                && string.Equals(entry.RouteSnapshotDigest, RouteDigest, StringComparison.Ordinal)
                && string.Equals(entry.TransitionTokenId, TransitionTokenId, StringComparison.Ordinal)
                && string.Equals(entry.TransitionTokenDigest, TransitionTokenDigest, StringComparison.Ordinal)
                && string.Equals(
                    entry.ActualScenePath,
                    AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                    StringComparison.Ordinal)
                && terminal.Disposition
                    == StageSegmentHandoffClosedDisposition.DestinationBound
                && terminal.LoaderGeneration == LoaderGeneration
                && terminal.LoaderGenerationInvalidated
                && terminal.PendingLoadCallbackCount == 0
                && terminal.PendingBindCallbackCount == 0
                && terminal.PendingUnloadCallbackCount == 0
                && string.Equals(
                    terminal.SegmentEntryReceiptId,
                    entry.SegmentEntryReceiptId,
                    StringComparison.Ordinal)
                && string.Equals(
                    terminal.SegmentEntryReceiptDigest,
                    entry.CanonicalDigest,
                    StringComparison.Ordinal);
            if (!ExactHandoffReceiptChain)
            {
                throw new InvalidOperationException(
                    "G08 Station entry/handoff receipt identity or loader closure drifted.");
            }

            yield return null;
        }

        private void EnsureProductTransitionOwner()
        {
            transitionOwner = UISceneTransitionHandoffOwner.CurrentOwner;
            if (transitionOwner == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    AuditionPvStationBossDeathAftermathCapture.TransitionOverlayPrefabPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException(
                        "G08 could not load the authored product transition overlay prefab.");
                }

                GameObject instance = Instantiate(prefab);
                instance.name = "G08_CaptureOwned_ProductTransitionOverlay";
                transitionOwner = UISceneTransitionHandoffOwner.CurrentOwner;
                captureOwnedTransitionRoot = transitionOwner != null
                    ? transitionOwner.transform.root.gameObject
                    : instance.transform.root.gameObject;
            }

            if (transitionOwner == null
                || !transitionOwner.isActiveAndEnabled
                || !UITransitionHandoffService.HasProvider)
            {
                throw new InvalidOperationException(
                    "G08 requires the real product UI transition handoff provider before Corridor completion.");
            }
        }

        private void SubscribeTransitionEvents()
        {
            if (transitionEventsSubscribed || transitionOwner == null)
            {
                return;
            }

            transitionOwner.DestinationArrived += HandleTransitionDestinationArrived;
            transitionOwner.HandoffCompleted += HandleTransitionHandoffCompleted;
            transitionOwner.HandoffFailed += HandleTransitionHandoffFailed;
            transitionEventsSubscribed = true;
        }

        private void UnsubscribeTransitionEvents()
        {
            if (!transitionEventsSubscribed || transitionOwner == null)
            {
                transitionEventsSubscribed = false;
                return;
            }

            transitionOwner.DestinationArrived -= HandleTransitionDestinationArrived;
            transitionOwner.HandoffCompleted -= HandleTransitionHandoffCompleted;
            transitionOwner.HandoffFailed -= HandleTransitionHandoffFailed;
            transitionEventsSubscribed = false;
        }

        private IEnumerator CompleteCorridorTutorialThroughPublicGameplay(Scene corridor)
        {
            OlympusCorridorTutorialDirector tutorial = RequireSingleSceneComponent<
                OlympusCorridorTutorialDirector>(corridor);
            PlayerMovementController corridorMovement = RequireSingleSceneComponent<
                PlayerMovementController>(corridor);
            PlayerActionController corridorAction = RequireSingleSceneComponent<
                PlayerActionController>(corridor);
            PlayerRangedBasicAttackAction corridorRanged = RequireSingleSceneComponent<
                PlayerRangedBasicAttackAction>(corridor);
            PlayerCombatModeController corridorCombatMode = RequireSingleSceneComponent<
                PlayerCombatModeController>(corridor);
            PlayerLockTargetController corridorLockTarget = RequireSingleSceneComponent<
                PlayerLockTargetController>(corridor);

            double deadline = Time.realtimeSinceStartupAsDouble + 45d;
            yield return WaitForTutorialWindow(tutorial, "Melee", deadline);
            RequireTutorialHardLock(
                corridor,
                corridorLockTarget,
                corridorMovement.transform.position);
            corridorAction.QueueBasicAttack();
            yield return WaitForTutorialStepToAdvance(tutorial, "Melee", deadline);

            yield return WaitForTutorialWindow(tutorial, "Move", deadline);
            corridorMovement.SetMoveInput(Vector2.up);
            try
            {
                yield return WaitForTutorialStepToAdvance(tutorial, "Move", deadline);
            }
            finally
            {
                corridorMovement.SetMoveInput(Vector2.zero);
            }

            yield return WaitForTutorialWindow(tutorial, "SwapToRanged", deadline);
            corridorCombatMode.QueueCombatModeSwap();
            yield return WaitForTutorialStepToAdvance(
                tutorial,
                "SwapToRanged",
                deadline);

            yield return WaitForTutorialWindow(tutorial, "Fire", deadline);
            RequireTutorialHardLock(
                corridor,
                corridorLockTarget,
                corridorMovement.transform.position);
            corridorRanged.SetFireHeld(true);
            try
            {
                double fireDeadline = Time.realtimeSinceStartupAsDouble + 4d;
                while (string.Equals(
                        tutorial.CurrentStepId,
                        "Fire",
                        StringComparison.Ordinal)
                    && Time.realtimeSinceStartupAsDouble < fireDeadline)
                {
                    yield return null;
                }

                if (string.Equals(
                    tutorial.CurrentStepId,
                    "Fire",
                    StringComparison.Ordinal))
                {
                    throw new TimeoutException(
                        "G08 real held-fire Corridor tutorial path did not advance Fire: "
                        + corridorRanged.LastUseBlockedReason);
                }
            }
            finally
            {
                corridorRanged.SetFireHeld(false);
            }

            yield return WaitForTutorialWindow(tutorial, "Dodge", deadline);
            corridorAction.QueueDodge();
            yield return WaitForTutorialStepToAdvance(tutorial, "Dodge", deadline);

            yield return WaitForTutorialWindow(tutorial, "ClearTargets", deadline);
            corridorRanged.SetFireHeld(true);
            while (!tutorial.IsCompleted
                && runContext.LifecycleState == StageRunLifecycleState.CorridorActive
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                if (string.Equals(
                    tutorial.CurrentPhaseId,
                    "AwaitingAction",
                    StringComparison.Ordinal))
                {
                    TryRequestTutorialHardLock(
                        corridor,
                        corridorLockTarget,
                        corridorMovement.transform.position);
                }

                yield return null;
            }

            corridorRanged.SetFireHeld(false);
            corridorRanged.ClearAimInput();
            corridorMovement.SetMoveInput(Vector2.zero);
            corridorLockTarget.ClearHardLock();
            if (!tutorial.IsCompleted
                && runContext.LifecycleState != StageRunLifecycleState.HandoffPending)
            {
                throw new TimeoutException(
                    "G08 public Corridor tutorial actions did not reach product completion.");
            }
        }

        private static IEnumerator WaitForTutorialWindow(
            OlympusCorridorTutorialDirector tutorial,
            string step,
            double deadline)
        {
            while ((!string.Equals(tutorial.CurrentStepId, step, StringComparison.Ordinal)
                    || !string.Equals(
                        tutorial.CurrentPhaseId,
                        "AwaitingAction",
                        StringComparison.Ordinal))
                && !tutorial.IsCompleted
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (tutorial.IsCompleted
                || !string.Equals(tutorial.CurrentStepId, step, StringComparison.Ordinal)
                || !string.Equals(
                    tutorial.CurrentPhaseId,
                    "AwaitingAction",
                    StringComparison.Ordinal))
            {
                throw new TimeoutException(
                    $"G08 Corridor tutorial did not expose public {step}/AwaitingAction.");
            }
        }

        private static IEnumerator WaitForTutorialStepToAdvance(
            OlympusCorridorTutorialDirector tutorial,
            string step,
            double deadline)
        {
            while (string.Equals(tutorial.CurrentStepId, step, StringComparison.Ordinal)
                && !tutorial.IsCompleted
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (string.Equals(tutorial.CurrentStepId, step, StringComparison.Ordinal)
                && !tutorial.IsCompleted)
            {
                throw new TimeoutException(
                    $"G08 public Corridor tutorial action did not advance {step}.");
            }
        }

        private static void RequireTutorialHardLock(
            Scene corridor,
            PlayerLockTargetController lockTarget,
            Vector3 playerPosition)
        {
            if (!TryRequestTutorialHardLock(corridor, lockTarget, playerPosition))
            {
                throw new InvalidOperationException(
                    "G08 Corridor tutorial exposed no active authored enemy target.");
            }
        }

        private static bool TryRequestTutorialHardLock(
            Scene corridor,
            PlayerLockTargetController lockTarget,
            Vector3 playerPosition)
        {
            CombatHealth[] candidates = FindSceneComponents<CombatHealth>(corridor);
            CombatHealth nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < candidates.Length; i++)
            {
                CombatHealth candidate = candidates[i];
                if (candidate == null
                    || !candidate.gameObject.activeInHierarchy
                    || !candidate.IsAlive
                    || candidate.Team != DamageTeam.Enemy)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(
                    candidate.transform.position - playerPosition);
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }

            if (nearest == null)
            {
                return false;
            }

            lockTarget.RequestHardLock(nearest);
            return true;
        }

        private void HandleTransitionDestinationArrived(
            UISceneTransitionTicket ticket,
            Scene scene)
        {
            if (ticket.Generation != transitionGeneration && transitionGeneration != 0)
            {
                transitionFailure = "The UI transition destination arrived on a substituted generation.";
                return;
            }

            transitionGeneration = ticket.Generation;
            transitionDestinationArrived = scene.IsValid()
                && string.Equals(
                    scene.path,
                    AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                    StringComparison.Ordinal);
            if (!transitionDestinationArrived)
            {
                transitionFailure = "The UI transition arrived at a non-Station destination.";
            }
        }

        private void HandleTransitionHandoffCompleted(UISceneTransitionTicket ticket)
        {
            if (ticket.Generation != transitionGeneration || !transitionDestinationArrived)
            {
                transitionFailure = "The UI transition completed without the exact Station arrival.";
                return;
            }

            transitionHandoffCompleted = true;
        }

        private void HandleTransitionHandoffFailed(
            UISceneTransitionTicket ticket,
            string error)
        {
            transitionFailure = string.IsNullOrWhiteSpace(error)
                ? $"UI transition generation {ticket.Generation} failed without a diagnostic."
                : error;
        }

        private void ResolveStationBindings()
        {
            Scene station = SceneManager.GetActiveScene();
            phaseFlow = RequireSingleSceneComponent<
                OlympusStationAkazaPhase2FlowController>(station);
            encounter = RequireSingleSceneComponent<CombatEncounterController>(station);
            aftermath = RequireSingleSceneComponent<
                OlympusStationBossTerminalAftermathPresenter>(station);
            resultPresenter = RequireSingleSceneComponent<
                OlympusStationCombatResultPresenter>(station);
            overlay = RequireSingleSceneComponent<OlympusStageClearOverlay>(station);
            deathCamera = RequireSingleSceneComponent<BossBarrageCameraCueDriver>(station);
            deathVisual = RequireSingleSceneComponent<BossBarrageVisualCueDriver>(station);
            deathMotion = RequireSingleSceneComponent<AkazaPhase2CombatMotionDriver>(station);
            cinematic = RequireSingleSceneComponent<ActionCinematicCueDirector>(station);

            playerHealth = phaseFlow.PlayerHealth;
            bossHealth = phaseFlow.BossHealth;
            movement = phaseFlow.PlayerMovement;
            action = phaseFlow.PlayerActionController;
            ranged = phaseFlow.PlayerRangedBasicAttackAction;
            skill = movement?.GetComponent<PlayerSkill1Action>();
            summon = movement?.GetComponent<PlayerSummonSlot1Action>();
            support = movement?.GetComponents<PlayerSupportSummonSlotAction>()
                ?? Array.Empty<PlayerSupportSummonSlotAction>();
            combatMode = movement?.GetComponent<PlayerCombatModeController>();
            lockTarget = movement?.GetComponent<PlayerLockTargetController>();
            bossPressureAction = phaseFlow.PressureActionDirector
                ?.SummonPressureAction;
            bossPressurePosition = phaseFlow.PressurePositionController;
            combatHud = phaseFlow.CombatHudCanvasGroup;
            entryGuide = FindSingleSceneInterface<ICombatEntryGuideGate>(station);

            if (playerHealth == null
                || bossHealth == null
                || movement == null
                || action == null
                || ranged == null
                || skill == null
                || summon == null
                || support.Length != 2
                || support.Any(value => value == null)
                || combatMode == null
                || lockTarget == null
                || bossPressureAction == null
                || bossPressurePosition == null
                || bossPressurePosition.MovedTransform == null
                || combatHud == null
                || entryGuide == null
                || aftermath.BossHealth != bossHealth
                || aftermath.ActionCinematicCueDirector != cinematic
                || encounter.EnemyHealth != bossHealth
                || !encounter.UsesCoordinatedTerminalResolution
                || !resultPresenter.HasCanonicalStageRun)
            {
                throw new InvalidOperationException(
                    "G08 could not resolve the exact Station combat, eight-owner input, aftermath, result, or entry-guide graph.");
            }
        }

        private IEnumerator ReleaseEntryGuide()
        {
            EntryGuideObservedPlaying = entryGuide.State
                == CombatEntryGuideState.Playing;
            double deadline = Time.realtimeSinceStartupAsDouble
                + EntryGuideTimeoutSeconds;
            while (entryGuide.State != CombatEntryGuideState.Released
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                EntryGuideObservedPlaying |= entryGuide.State
                    == CombatEntryGuideState.Playing;
                if (entryGuide.State == CombatEntryGuideState.Interrupted)
                {
                    throw new InvalidOperationException(
                        "G08 Station entry guide was interrupted.");
                }

                if (entryGuide.IsAwaitingAdvance)
                {
                    entryGuide.RequestAdvance();
                }

                yield return null;
            }

            EntryGuideReleased = entryGuide.State == CombatEntryGuideState.Released;
            if (!EntryGuideObservedPlaying || !EntryGuideReleased)
            {
                throw new TimeoutException(
                    "G08 Station entry guide did not traverse Playing -> Released.");
            }

            double runningDeadline = Time.realtimeSinceStartupAsDouble + 2d;
            while (!encounter.IsRunning
                && Time.realtimeSinceStartupAsDouble < runningDeadline)
            {
                yield return null;
            }

            if (!encounter.IsRunning || bossHealth.IsInvulnerable)
            {
                throw new InvalidOperationException(
                    "G08 Station encounter did not become damageable after entry release.");
            }
        }

        private IEnumerator PreparePhaseTwoAndHealth()
        {
            int transitionCompletedEvents = 0;
            void HandleTransitionCompleted() => transitionCompletedEvents++;
            phaseFlow.TransitionCompleted += HandleTransitionCompleted;
            try
            {
                float threshold = bossHealth.MaxHealth * phaseFlow.PhaseThreshold01;
                ApplyStrictlyNonlethalSetupDamage(
                    Mathf.Max(0.01f, bossHealth.CurrentHealth - threshold + 0.01f),
                    "Phase1 threshold");
                if (phaseFlow.CurrentPhase
                        != OlympusStationAkazaPhase2FlowController.Phase.Transitioning
                    || phaseFlow.TransitionStartCount != 1
                    || phaseFlow.TransitionCompletionCount != 0)
                {
                    throw new InvalidOperationException(
                        "G08 real Phase1 threshold hit did not start exactly one transition.");
                }

                if (!phaseFlow.TrySkipTransition())
                {
                    throw new InvalidOperationException(
                        "G08 public TrySkipTransition rejected the active transition.");
                }

                double deadline = Time.realtimeSinceStartupAsDouble
                    + PhaseTwoTimeoutSeconds;
                while ((phaseFlow.CurrentPhase
                            != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                        || phaseFlow.TransitionCompletionCount != 1)
                    && Time.realtimeSinceStartupAsDouble < deadline)
                {
                    yield return null;
                }

                PhaseTransitionStartCount = phaseFlow.TransitionStartCount;
                PhaseTransitionCompletionCount = phaseFlow.TransitionCompletionCount;
                PhaseTwoApplied = phaseFlow.PhaseTwoApplied;
                if (phaseFlow.CurrentPhase
                        != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                    || PhaseTransitionStartCount != 1
                    || PhaseTransitionCompletionCount != 1
                    || transitionCompletedEvents != 1
                    || !PhaseTwoApplied)
                {
                    throw new TimeoutException(
                        "G08 skipped transition did not commit exact Phase2 completion.");
                }

                ApplyStrictlyNonlethalSetupDamage(
                    bossHealth.CurrentHealth
                        - AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth,
                    "Phase2 HP12 calibration");
                PreparedHealthObserved = bossHealth.CurrentHealth;
                if (!bossHealth.IsAlive
                    || Mathf.Abs(
                        PreparedHealthObserved
                            - AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth)
                        > Tolerance)
                {
                    throw new InvalidOperationException(
                        "G08 non-lethal setup did not leave the real Phase2 boss at exactly 12 HP.");
                }
            }
            finally
            {
                phaseFlow.TransitionCompleted -= HandleTransitionCompleted;
            }
        }

        private void ApplyStrictlyNonlethalSetupDamage(float requestedAmount, string label)
        {
            float before = bossHealth.CurrentHealth;
            if (!float.IsFinite(requestedAmount)
                || requestedAmount <= 0f
                || requestedAmount >= before)
            {
                throw new InvalidOperationException(
                    $"G08 {label} amount must be strictly non-lethal: before={before}, amount={requestedAmount}.");
            }

            var damage = new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                requestedAmount,
                bossHealth.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None);
            if (!bossHealth.TryApplyDamage(damage)
                || !bossHealth.IsAlive
                || bossHealth.CurrentHealth <= 0f
                || bossHealth.CurrentHealth >= before)
            {
                throw new InvalidOperationException(
                    $"G08 {label} was not a real accepted strictly non-lethal hit.");
            }
        }

        private void ConfigureDeterministicPlayerShot()
        {
            combatMode.SetRangedMode();
            lockTarget.RequestHardLock(bossHealth);
            ranged.ClearAimInput();
            Physics.SyncTransforms();
            if (!combatMode.IsRangedMode
                || !lockTarget.HasLockTarget
                || lockTarget.CurrentTargetHealth != bossHealth
                || !ranged.IsFireReady
                || ranged.CurrentAmmo != ranged.MagazineSize
                || ranged.ActiveProjectileCount != 0
                || Mathf.Abs(PreparedHealthObserved
                    - AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth)
                    > Tolerance)
            {
                throw new InvalidOperationException(
                    "G08 public ranged-mode/hard-lock pre-roll did not yield one ready authored shot.");
            }
        }

        private IEnumerator PrepareNaturalBossImpactOwnership()
        {
            AcquireBossPressureMovementHold();

            // A Phase2 handoff may still own a hostile pressure actor. The public
            // cinematic-dismissal API is intentionally idempotent: zero active
            // screens is already unobstructed, while any observed screen must be
            // removed before the one recorded projectile.
            pressureScreensBeforeDismiss = bossPressureAction.ActivePressureScreenCount;
            pressureSummonsDismissed = bossPressureAction.DismissActivePressureSummons();
            Physics.SyncTransforms();
            pressureScreensAfterDismiss = bossPressureAction.ActivePressureScreenCount;
            if (pressureScreensBeforeDismiss < 0
                || pressureSummonsDismissed < 0
                || pressureScreensAfterDismiss != 0
                || (pressureScreensBeforeDismiss > 0
                    && pressureSummonsDismissed < pressureScreensBeforeDismiss))
            {
                throw new InvalidOperationException(
                    "G08 could not prove an unobstructed authored Phase2 shot lane: "
                    + $"before={pressureScreensBeforeDismiss}, "
                    + $"dismissed={pressureSummonsDismissed}, "
                    + $"after={pressureScreensAfterDismiss}.");
            }

            Transform fireOrigin = ResolveAuthoredPlayerFireOrigin();
            Vector3 playerStart = movement.transform.position;
            int preparationClockFrame =
                AuditionPvStationBossDeathAftermathCapture.PhaseTwoSettleFrames;
            const int MaximumAdjustments = 3;
            const int StepSettleFrames = 24;
            const float StepSeconds = 0.2f;
            const float MaximumStepMeters = 3f;

            for (int attempt = 0; attempt <= MaximumAdjustments; attempt++)
            {
                predictedBossSweepDistance = MeasureNaturalBossSweepDistance(
                    fireOrigin);
                predictedNaturalImpactFrame =
                    AuditionPvStationBossDeathAftermathCapture
                        .PredictNaturalImpactFrame(predictedBossSweepDistance);
                float targetDelta = predictedBossSweepDistance
                    - AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactTargetDistance;
                bool centeredInExactFrame = predictedNaturalImpactFrame
                        == AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                    && Mathf.Abs(targetDelta)
                        <= AuditionPvStationBossDeathAftermathCapture
                            .NaturalImpactDistanceTolerance;
                if (centeredInExactFrame)
                {
                    break;
                }

                if (attempt == MaximumAdjustments
                    || Mathf.Abs(targetDelta) > MaximumStepMeters)
                {
                    throw new InvalidOperationException(
                        $"G08 public pre-roll could not center the natural boss sweep at f62: distance={predictedBossSweepDistance:0.000}, predictedFrame={predictedNaturalImpactFrame}.");
                }

                Vector3 towardBoss = Vector3.ProjectOnPlane(
                    bossHealth.transform.position - movement.transform.position,
                    Vector3.up);
                if (towardBoss.sqrMagnitude <= 0.0001f)
                {
                    throw new InvalidOperationException(
                        "G08 cannot resolve a planar public player step toward the boss.");
                }

                Vector3 stepDirection = targetDelta >= 0f
                    ? towardBoss.normalized
                    : -towardBoss.normalized;
                movement.BeginAuthoredPlanarStep(
                    stepDirection,
                    Mathf.Abs(targetDelta),
                    StepSeconds);
                for (int frame = 0; frame < StepSettleFrames; frame++)
                {
                    presentationClockLease.SetFrame(preparationClockFrame++);
                    yield return WaitForNextPlayerFrame();
                }

                movement.SetMoveInput(Vector2.zero);
                Physics.SyncTransforms();
            }

            for (int frame = 0; frame < 6; frame++)
            {
                presentationClockLease.SetFrame(preparationClockFrame++);
                yield return WaitForNextPlayerFrame();
            }

            Physics.SyncTransforms();
            predictedBossSweepDistance = MeasureNaturalBossSweepDistance(fireOrigin);
            predictedNaturalImpactFrame =
                AuditionPvStationBossDeathAftermathCapture
                    .PredictNaturalImpactFrame(predictedBossSweepDistance);
            preShotPlayerPlanarStepDistance = Vector3.ProjectOnPlane(
                movement.transform.position - playerStart,
                Vector3.up).magnitude;
            if (predictedNaturalImpactFrame
                    != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || Mathf.Abs(
                    predictedBossSweepDistance
                    - AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactTargetDistance)
                    > AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactDistanceTolerance
                || preShotPlayerPlanarStepDistance <= 0.25f
                || preShotPlayerPlanarStepDistance > MaximumStepMeters)
            {
                throw new InvalidOperationException(
                    $"G08 natural-impact calibration was not stable at shot arm: distance={predictedBossSweepDistance:0.000}, predictedFrame={predictedNaturalImpactFrame}, publicStep={preShotPlayerPlanarStepDistance:0.000}.");
            }
        }

        private void AcquireBossPressureMovementHold()
        {
            if (bossPressureMovementOwnershipAcquired
                || bossPressureMovementLeaseActive
                || bossPressurePosition == null
                || bossPressurePosition.MovedTransform == null)
            {
                throw new InvalidOperationException(
                    "G08 boss-pressure movement ownership could not be acquired exactly once.");
            }

            savedBossPressureMovementEnabled = bossPressurePosition.MovementEnabled;
            bossPressureMovementOwnershipAcquired = true;
            bossPressureMovementLeaseActive = true;
            bossPressureMovementRestoredExactly = false;
            bossPressurePosition.SetMovementEnabled(false);
            bossPressureMovementHeldForShot = !bossPressurePosition.MovementEnabled;
            if (!savedBossPressureMovementEnabled || !bossPressureMovementHeldForShot)
            {
                throw new InvalidOperationException(
                    "G08 canonical Phase2 boss movement was not enabled before the capture-owned hold, or the public hold was rejected.");
            }
        }

        private void RestoreBossPressureMovementHold()
        {
            if (!bossPressureMovementLeaseActive)
            {
                return;
            }

            if (bossPressurePosition == null)
            {
                throw new InvalidOperationException(
                    "G08 cannot restore the capture-owned boss movement hold because its exact owner was destroyed.");
            }

            bossPressurePosition.SetMovementEnabled(savedBossPressureMovementEnabled);
            if (bossPressurePosition.MovementEnabled
                != savedBossPressureMovementEnabled)
            {
                throw new InvalidOperationException(
                    "G08 boss-pressure movement state did not restore to its exact pre-capture value.");
            }

            bossPressureMovementLeaseActive = false;
            bossPressureMovementRestoredExactly = true;
        }

        private Transform ResolveAuthoredPlayerFireOrigin()
        {
            Transform[] candidates = movement
                .GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(value => value != null
                    && value.gameObject.activeInHierarchy
                    && string.Equals(value.name, "Muzzle", StringComparison.Ordinal))
                .ToArray();
            if (candidates.Length != 1)
            {
                throw new InvalidOperationException(
                    $"G08 requires one active authored player Muzzle; found {candidates.Length}.");
            }

            return candidates[0];
        }

        private float MeasureNaturalBossSweepDistance(Transform fireOrigin)
        {
            if (fireOrigin == null
                || !ranged.TryGetAimPreviewDirection(out Vector3 direction)
                || direction.sqrMagnitude <= 0.0001f)
            {
                throw new InvalidOperationException(
                    "G08 public ranged aim preview is unavailable for natural-impact calibration.");
            }

            Physics.SyncTransforms();
            int hitCount = Physics.SphereCastNonAlloc(
                fireOrigin.position,
                AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileRadius,
                direction.normalized,
                naturalImpactSweepHits,
                64f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            if (hitCount >= naturalImpactSweepHits.Length)
            {
                throw new InvalidOperationException(
                    "G08 natural-impact sweep overflowed its fail-closed hit buffer.");
            }

            float nearestBossDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = naturalImpactSweepHits[index];
                if (hit.collider != null
                    && CombatHealth.ResolveFromCollider(hit.collider) == bossHealth)
                {
                    nearestBossDistance = Mathf.Min(
                        nearestBossDistance,
                        hit.distance);
                }
            }

            if (!float.IsFinite(nearestBossDistance))
            {
                throw new InvalidOperationException(
                    "G08 public aim preview does not physically sweep the authored boss collider.");
            }

            return nearestBossDistance;
        }

        private void RevalidateNaturalBossImpactAtShotArm()
        {
            predictedBossSweepDistance = MeasureNaturalBossSweepDistance(
                ResolveAuthoredPlayerFireOrigin());
            predictedNaturalImpactFrame =
                AuditionPvStationBossDeathAftermathCapture
                    .PredictNaturalImpactFrame(predictedBossSweepDistance);
        }

        private void ArmBossPoseStabilityProof()
        {
            Transform movedBoss = bossPressurePosition?.MovedTransform;
            if (movedBoss == null
                || !bossPressureMovementOwnershipAcquired
                || !bossPressureMovementLeaseActive
                || bossPressurePosition.MovementEnabled)
            {
                throw new InvalidOperationException(
                    "G08 cannot arm the boss-pose proof without its capture-owned public movement hold.");
            }

            bossPositionAtShotArm = movedBoss.position;
            bossRotationAtShotArm = movedBoss.rotation;
            bossPositionAtImpact = Vector3.zero;
            bossRotationAtImpact = Quaternion.identity;
            maximumBossPositionDriftThroughImpact = 0f;
            maximumBossRotationDriftThroughImpact = 0f;
            bossPoseStableThroughImpact = true;
            bossPoseTrackingArmed = true;
        }

        private void ObserveBossPoseThroughImpact(bool atPhysicalImpact)
        {
            if (!bossPoseTrackingArmed)
            {
                bossPoseStableThroughImpact = false;
                return;
            }

            Transform movedBoss = bossPressurePosition?.MovedTransform;
            if (movedBoss == null)
            {
                bossPoseStableThroughImpact = false;
                return;
            }

            float positionDrift = Vector3.Distance(
                movedBoss.position,
                bossPositionAtShotArm);
            float rotationDrift = Quaternion.Angle(
                movedBoss.rotation,
                bossRotationAtShotArm);
            maximumBossPositionDriftThroughImpact = Mathf.Max(
                maximumBossPositionDriftThroughImpact,
                positionDrift);
            maximumBossRotationDriftThroughImpact = Mathf.Max(
                maximumBossRotationDriftThroughImpact,
                rotationDrift);
            bossPoseStableThroughImpact &= bossPressureMovementLeaseActive
                && !bossPressurePosition.MovementEnabled
                && positionDrift <= Tolerance
                && rotationDrift <= Tolerance;

            if (atPhysicalImpact)
            {
                bossPositionAtImpact = movedBoss.position;
                bossRotationAtImpact = movedBoss.rotation;
                bossPoseTrackingArmed = false;
            }
        }

        private void SubscribeShotEvents()
        {
            ranged.RangedFireStarted += HandleRangedFireStarted;
            ranged.RangedProjectileFired += HandleProjectileFired;
            bossHealth.Damaged += HandleBossDamaged;
            bossHealth.Died += HandleBossDied;
            encounter.TerminalResolved += HandleEncounterTerminalResolved;
            aftermath.AftermathStarted += HandleAftermathStarted;
            aftermath.AftermathCompleted += HandleAftermathCompleted;
            overlay.PresentationSucceeded += HandleOverlayPresentationSucceeded;
            eventsSubscribed = true;
        }

        private void UnsubscribeShotEvents()
        {
            if (!eventsSubscribed)
            {
                return;
            }

            if (ranged != null)
            {
                ranged.RangedFireStarted -= HandleRangedFireStarted;
                ranged.RangedProjectileFired -= HandleProjectileFired;
            }

            if (bossHealth != null)
            {
                bossHealth.Damaged -= HandleBossDamaged;
                bossHealth.Died -= HandleBossDied;
            }

            if (encounter != null)
            {
                encounter.TerminalResolved -= HandleEncounterTerminalResolved;
            }

            if (aftermath != null)
            {
                aftermath.AftermathStarted -= HandleAftermathStarted;
                aftermath.AftermathCompleted -= HandleAftermathCompleted;
            }

            if (overlay != null)
            {
                overlay.PresentationSucceeded -= HandleOverlayPresentationSucceeded;
            }

            if (firedProjectile != null)
            {
                firedProjectile.DamageApplied -= HandleProjectileDamageApplied;
            }

            eventsSubscribed = false;
        }

        private void ValidateReadyForShot()
        {
            Scene station = SceneManager.GetActiveScene();
            if (!station.IsValid()
                || !station.isLoaded
                || !string.Equals(
                    station.path,
                    AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                    StringComparison.Ordinal)
                || StageRunRuntime.ActiveContext != runContext
                || runContext.LifecycleState != StageRunLifecycleState.StationActive
                || !ExactHandoffReceiptChain
                || !EntryGuideReleased
                || phaseFlow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                || !phaseFlow.PhaseTwoApplied
                || !encounter.IsRunning
                || !bossHealth.IsAlive
                || Mathf.Abs(bossHealth.CurrentHealth
                    - AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth)
                    > Tolerance
                || ranged.ActiveProjectileCount != 0
                || !ranged.IsFireReady
                || movement.HasMoveInput
                || ranged.IsCinematicInputLocked
                || movement.IsCinematicMoveInputLocked
                || action.IsCinematicInputLocked
                || skill.IsCinematicInputLocked
                || summon.IsCinematicInputLocked
                || support.Any(value => value.IsCinematicInputLocked)
                || combatMode.IsCinematicInputLocked
                || aftermath.IsStarted
                || overlay.IsShown
                || pressureScreensBeforeDismiss < 0
                || pressureSummonsDismissed < 0
                || pressureScreensAfterDismiss != 0
                || (pressureScreensBeforeDismiss > 0
                    && pressureSummonsDismissed < pressureScreensBeforeDismiss)
                || bossPressureAction.ActivePressureScreenCount != 0
                || !bossPressureMovementOwnershipAcquired
                || !bossPressureMovementLeaseActive
                || !savedBossPressureMovementEnabled
                || !bossPressureMovementHeldForShot
                || bossPressurePosition.MovementEnabled
                || predictedNaturalImpactFrame
                    != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || Mathf.Abs(
                    predictedBossSweepDistance
                    - AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactTargetDistance)
                    > AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactDistanceTolerance
                || Time.timeScale != 1f
                || !BossCombatCadenceScheduler.IsExternallySuspended
                || BossCombatCadenceScheduler.ExternalSuspensionCount != 1)
            {
                throw new InvalidOperationException(
                    "G08 canonical Station Phase2/HP12/projectile/input/time baseline is not exact: "
                    + $"pressureBefore={pressureScreensBeforeDismiss}, "
                    + $"pressureDismissed={pressureSummonsDismissed}, "
                    + $"pressureAfter={pressureScreensAfterDismiss}.");
            }

            bossHealthBeforeShot = bossHealth.CurrentHealth;
        }

        private void ObserveFrameState()
        {
            if (currentFrame
                < AuditionPvStationBossDeathAftermathCapture.ImpactFrame)
            {
                ObserveBossPoseThroughImpact(atPhysicalImpact: false);
            }

            if (currentFrame
                < AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame)
            {
                noEarlyFreeze &= Time.timeScale != 0f
                    && !overlay.IsWorldFrozenForResult;
            }

            if (currentFrame < AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame)
            {
                Scene earlyResult = SceneManager.GetSceneByName(
                    AuditionPvStationBossDeathAftermathCapture.StageClearSceneName);
                resultAbsentBeforeRequest &= !earlyResult.IsValid()
                    || !earlyResult.isLoaded;
            }

            if (firedProjectile != null
                && currentFrame > AuditionPvStationBossDeathAftermathCapture.FireFrame
                && currentFrame < AuditionPvStationBossDeathAftermathCapture.ImpactFrame)
            {
                physicalProjectileObservedActiveBeforeImpact |=
                    firedProjectile.IsActive;
                projectileMovedBeforeImpact |= Vector3.Distance(
                    firedProjectile.transform.position,
                    projectileSpawnPosition) > 0.5f;
            }

            if (currentFrame == 61 && firedProjectile != null)
            {
                projectilePositionAtFrame61 = firedProjectile.transform.position;
            }

            if (firstFreezeFrame < 0
                && overlay.IsWorldFrozenForResult
                && Time.timeScale == 0f)
            {
                firstFreezeFrame = currentFrame;
            }

            Scene clearScene = SceneManager.GetSceneByName(
                AuditionPvStationBossDeathAftermathCapture.StageClearSceneName);
            if (firstResultSceneFrame < 0
                && clearScene.IsValid()
                && clearScene.isLoaded)
            {
                firstResultSceneFrame = currentFrame;
            }

            if (clearScene.IsValid() && clearScene.isLoaded)
            {
                StageClearScreenPresenter[] presenters = FindSceneComponents<
                    StageClearScreenPresenter>(clearScene);
                if (presenters.Length == 1)
                {
                    clearPresenter = presenters[0];
                    if (firstResultConfiguredFrame < 0
                        && clearPresenter.IsConfigured)
                    {
                        firstResultConfiguredFrame = currentFrame;
                    }

                    CanvasGroup group = clearPresenter.GetComponent<CanvasGroup>();
                    bool interactive = clearPresenter.IsConfigured
                        && clearPresenter.EntranceCompleted
                        && !clearPresenter.IsEntrancePlaying
                        && group != null
                        && group.interactable
                        && group.blocksRaycasts;
                    if (firstInteractiveFrame < 0 && interactive)
                    {
                        firstInteractiveFrame = currentFrame;
                    }
                }
            }

            if (inputLeaseReleasedFrame < 0
                && aftermath.IsComplete
                && !aftermath.InputLeaseActive)
            {
                inputLeaseReleasedFrame = currentFrame;
            }

            if (currentFrame
                == AuditionPvStationBossDeathAftermathCapture.AftermathHeroFrame)
            {
                deathStateAtAftermathHero = AnimatorInDeathState;
            }

            if (deathStateHeldFrame < 0
                && currentFrame
                    == AuditionPvStationBossDeathAftermathCapture.DeathHoldProofFrame
                && AnimatorInDeathState)
            {
                deathStateHeldFrame = currentFrame;
            }

            if (currentFrame
                == AuditionPvStationBossDeathAftermathCapture.FireFrame)
            {
                HudWasActiveAtFire = combatHud.gameObject.activeInHierarchy
                    && combatHud.alpha > 0.99f;
            }

            if (currentFrame
                == AuditionPvStationBossDeathAftermathCapture.ImpactFrame)
            {
                HudWasActiveAtImpact = combatHud.gameObject.activeInHierarchy
                    && combatHud.alpha > 0.01f;
                allEightLocksObservedAtImpact = ExactAllEightInputLocks(true)
                    && aftermath.InputLeaseFullyAcquired
                    && aftermath.InputLeaseActive
                    && movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.BossTerminalAftermath);
            }

            if (currentFrame
                == AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame)
            {
                allEightLocksReleasedAtResult = ExactAllEightInputLocks(false)
                    && !aftermath.InputLeaseActive;
            }

            if (currentFrame
                == AuditionPvStationBossDeathAftermathCapture.InteractiveResultFrame)
            {
                ResultInteractiveAt246 = clearPresenter != null
                    && clearPresenter.IsConfigured
                    && clearPresenter.EntranceCompleted
                    && !clearPresenter.IsEntrancePlaying
                    && clearPresenter.GetComponent<CanvasGroup>() is CanvasGroup group
                    && group.interactable
                    && group.blocksRaycasts;
                HudYieldedAtResult = !combatHud.gameObject.activeSelf;
                CaptureTerminalFactsAndResultIdentity();
            }
        }

        private bool ExactAllEightInputLocks(bool expected)
        {
            return movement.IsCinematicMoveInputLocked == expected
                && action.IsCinematicInputLocked == expected
                && ranged.IsCinematicInputLocked == expected
                && skill.IsCinematicInputLocked == expected
                && summon.IsCinematicInputLocked == expected
                && support.Length == 2
                && support.All(value => value.IsCinematicInputLocked == expected)
                && combatMode.IsCinematicInputLocked == expected;
        }

        private void CaptureTerminalFactsAndResultIdentity()
        {
            StageRunResultSummary summary = resultPresenter.CommittedSummary;
            ResultSummarySameInstance = summary != null
                && ReferenceEquals(summary, runContext.CommittedSummary)
                && ReferenceEquals(summary, overlay.ResultSummary);
            PresentedSummarySameInstance = summary != null
                && clearPresenter != null
                && ReferenceEquals(summary, clearPresenter.ResultSummary);
            CommittedSummaryDigest = summary?.ResultSummaryDigest ?? string.Empty;
            PresentedSummaryDigest = overlay.PresentedResultDigest;
            OutcomeFactDigest = summary?.OutcomeFact?.CanonicalDigest ?? string.Empty;
            TerminalRecordReceiptCount = runContext.TerminalRecordReceiptCount;

            EncounterTerminalResolutionCoordinator coordinator =
                encounter.TerminalCoordinator;
            EncounterTerminalEpochEvidence epoch =
                coordinator?.TerminalEpochEvidence;
            if (epoch != null)
            {
                RootAdmissionSequence = epoch.Resolution.RootAdmissionSequence;
                TerminalEpoch = epoch.Resolution.Epoch;
                TerminalEpochEvidenceDigest = epoch.CanonicalDigest;
            }

            TerminalClosureDigest =
                runContext.TerminalEpochClosureRecord?.CanonicalDigest
                ?? string.Empty;
            TerminalFactsExact = summary != null
                && summary.Outcome == StageRouteOutcome.Clear
                && summary.OutcomeFact != null
                && summary.OutcomeFact.OutcomeDisposition
                    == StageOutcomeDisposition.Clear
                && summary.OutcomeFact.ClearReason
                    == StageClearReason.BossTerminal
                && summary.SegmentResultCount == 2
                && string.Equals(
                    summary.GetSegmentResult(0).SegmentId,
                    "corridor_intro_tutorial",
                    StringComparison.Ordinal)
                && string.Equals(
                    summary.GetSegmentResult(1).SegmentId,
                    "station_entry_combat",
                    StringComparison.Ordinal)
                && summary.TutorialRouteSummaryFact != null
                && summary.TutorialRouteSummaryFact.RouteState
                    == StageTutorialRouteState.Completed
                && resultPresenter.CommitReceipt != null
                && string.Equals(
                    resultPresenter.CommitReceipt.ResultSummaryDigest,
                    summary.ResultSummaryDigest,
                    StringComparison.Ordinal)
                && epoch != null
                && epoch.QueueDrained
                && epoch.BothSubjectsFinalized
                && epoch.ActiveTokenInvalidated
                && epoch.SubjectSnapshotCount == 2
                && epoch.CandidateCoverageCount == 1
                && runContext.TerminalEpochClosureRecord != null
                && runContext.TerminalEpochClosureRecord
                    .QueueDrainedAndSubjectsFinalized
                && runContext.TerminalEpochClosureRecord.ActiveTokenInvalidated
                && runContext.TerminalFinalizationAuthority != null
                && runContext.OwnerCoverageRecord != null
                && runContext.OwnerCoverageRecord.ZeroPendingFinalizationOwners
                && runContext.LifecycleState == StageRunLifecycleState.Presented;
        }

        private void ValidateCompletedShot()
        {
            CaptureTerminalFactsAndResultIdentity();
            if (fireFrame != AuditionPvStationBossDeathAftermathCapture.FireFrame
                || projectileFiredFrame != fireFrame
                || bossDiedFrame != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || projectileImpactFrame != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || terminalResolvedFrame != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || firstFreezeFrame
                    != AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame
                || firstResultSceneFrame
                    != AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame
                || firstResultConfiguredFrame
                    != AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame
                || firstInteractiveFrame
                    != AuditionPvStationBossDeathAftermathCapture.InteractiveResultFrame)
            {
                throw new InvalidOperationException(
                    "G08 exact schedule drifted: "
                    + $"fire={fireFrame}, projectile={projectileFiredFrame}, died={bossDiedFrame}, "
                    + $"impact={projectileImpactFrame}, terminal={terminalResolvedFrame}, "
                    + $"freeze={firstFreezeFrame}, resultScene={firstResultSceneFrame}, "
                    + $"configured={firstResultConfiguredFrame}, interactive={firstInteractiveFrame}.");
            }

            if (rangedFireStartedCount != 1
                || projectileFiredCount != 1
                || projectileDamageAppliedCount != 1
                || bossDamagedDuringShotCount != 1
                || bossDiedCount != 1
                || encounterTerminalResolvedCount != 1
                || aftermathStartedCount != 1
                || aftermathCompletedCount != 1
                || overlayPresentationSucceededCount != 1
                || pressureScreensBeforeDismiss < 0
                || pressureSummonsDismissed < 0
                || pressureScreensAfterDismiss != 0
                || (pressureScreensBeforeDismiss > 0
                    && pressureSummonsDismissed < pressureScreensBeforeDismiss)
                || !bossPressureMovementOwnershipAcquired
                || !bossPressureMovementLeaseActive
                || !savedBossPressureMovementEnabled
                || !bossPressureMovementHeldForShot
                || bossPressurePosition.MovementEnabled
                || !bossPoseStableThroughImpact
                || bossPoseTrackingArmed
                || Vector3.Distance(
                    bossPositionAtShotArm,
                    bossPositionAtImpact) > Tolerance
                || maximumBossPositionDriftThroughImpact > Tolerance
                || maximumBossRotationDriftThroughImpact > Tolerance
                || predictedNaturalImpactFrame
                    != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || Mathf.Abs(
                    predictedBossSweepDistance
                    - AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactTargetDistance)
                    > AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactDistanceTolerance
                || preShotPlayerPlanarStepDistance <= 0.25f
                || projectileInstanceId == 0
                || !physicalProjectileObservedActiveBeforeImpact
                || !projectileMovedBeforeImpact
                || Vector3.Distance(projectileSpawnPosition, projectilePositionAtFrame61)
                    <= 10f
                || projectileFiredSequence <= 0
                || bossDiedSequence <= projectileFiredSequence
                || projectileImpactSequence <= bossDiedSequence
                || terminalResolvedSequence <= projectileFiredSequence)
            {
                throw new InvalidOperationException(
                    "G08 physical projectile identity, flight, impact, death, or exact-once event chain failed: "
                    + $"pressureBefore={pressureScreensBeforeDismiss}, "
                    + $"pressureDismissed={pressureSummonsDismissed}, "
                    + $"pressureAfter={pressureScreensAfterDismiss}.");
            }

            if (!NoEarlyFreeze
                || !ResultAbsentBeforeRequest
                || !allEightLocksObservedAtImpact
                || !allEightLocksReleasedAtResult
                || aftermathCompletedFrame
                    != AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame
                || inputLeaseReleasedFrame
                    != AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame
                || deathStateHeldFrame
                    != AuditionPvStationBossDeathAftermathCapture.DeathHoldProofFrame
                || !deathStateAtAftermathHero
                || !aftermath.IsComplete
                || !aftermath.CompletedSuccessfully
                || aftermath.BeginCount != 1
                || aftermath.CompleteCount != 1
                || aftermath.InputLeaseActive
                || !aftermath.ScaleOneObserved
                || aftermath.ScaleOneViolationRecorded
                || !deathCamera.IsBossDeathCueComplete
                || deathCamera.BossDeathCueWasInterrupted
                || deathCamera.BossDeathCueRequestCount != 1
                || deathVisual.BossDeathWorldVfxCueRequestCount != 1
                || deathVisual.BossDeathProfileAudioSourceDelta <= 0
                || !BossDeathUsesPhaseTwoAnchor
                || deathMotion.DeathRequestCount != 1
                || !deathMotion.IsDead
                || !deathMotion.AttacksStopped
                || !AnimatorInDeathState)
            {
                throw new InvalidOperationException(
                    "G08 aftermath input/time/camera/VFX/audio/motion contract failed: "
                    + aftermath.LastError + " | " + aftermath.LastQualityWarning);
            }

            if (!ResultInteractiveAt246
                || !HudWasActiveAtFire
                || !HudWasActiveAtImpact
                || !HudYieldedAtResult
                || !overlay.IsShown
                || !overlay.IsWorldFrozenForResult
                || Time.timeScale != 0f
                || !ResultSummarySameInstance
                || !PresentedSummarySameInstance
                || !string.Equals(
                    CommittedSummaryDigest,
                    PresentedSummaryDigest,
                    StringComparison.Ordinal)
                || !TerminalFactsExact
                || TerminalRecordReceiptCount != 1
                || RootAdmissionSequence <= 0
                || TerminalEpoch <= 0
                || string.IsNullOrWhiteSpace(TerminalEpochEvidenceDigest)
                || string.IsNullOrWhiteSpace(TerminalClosureDigest))
            {
                throw new InvalidOperationException(
                    "G08 committed fact/result/HUD/freeze/interactive identity contract failed.");
            }
        }

        private void HandleRangedFireStarted()
        {
            CaptureEventContract(() => rangedFireStartedCount++);
        }

        private void HandleProjectileFired(LaneActionProjectile projectile)
        {
            CaptureEventContract(() =>
            {
                projectileFiredCount++;
                projectileFiredFrame = currentFrame;
                projectileFiredSequence = ++eventSequence;
                SphereCollider sphere = projectile != null
                    ? projectile.GetComponent<SphereCollider>()
                    : null;
                Vector3 sphereScale = sphere != null
                    ? sphere.transform.lossyScale
                    : Vector3.zero;
                float worldRadius = sphere != null
                    ? sphere.radius * Mathf.Max(
                        Mathf.Abs(sphereScale.x),
                        Mathf.Abs(sphereScale.y),
                        Mathf.Abs(sphereScale.z))
                    : 0f;
                if (projectile == null
                    || firedProjectile != null
                    || projectile.SourceHealth != playerHealth
                    || projectile.SourceTeam != DamageTeam.Player
                    || Mathf.Abs(projectile.Damage
                        - AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileDamage)
                        > Tolerance
                    || Mathf.Abs(worldRadius
                        - AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileRadius)
                        > Tolerance
                    || !projectile.IsActive)
                {
                    throw new InvalidOperationException(
                        "G08 RangedProjectileFired did not expose the one authored physical 12-damage projectile.");
                }

                firedProjectile = projectile;
                projectileInstanceId = projectile.GetInstanceID();
                projectileSpawnPosition = projectile.transform.position;
                projectile.DamageApplied += HandleProjectileDamageApplied;
            });
        }

        private void HandleProjectileDamageApplied(
            LaneActionProjectile projectile,
            CombatHealth target,
            Vector3 impactPoint,
            Vector3 direction)
        {
            CaptureEventContract(() =>
            {
                projectileDamageAppliedCount++;
                projectileImpactFrame = currentFrame;
                projectileImpactSequence = ++eventSequence;
                projectileImpactPoint = impactPoint;
                projectileImpactDirection = direction;
                ObserveBossPoseThroughImpact(atPhysicalImpact: true);
                if (projectile == null
                    || projectile != firedProjectile
                    || projectile.GetInstanceID() != projectileInstanceId
                    || target != bossHealth
                    || projectile.LastImpactTargetHealth != bossHealth
                    || projectile.LastImpactResult != ProjectileImpactResult.AppliedDamage)
                {
                    throw new InvalidOperationException(
                        "G08 projectile impact did not retain fired-instance/target/applied-damage identity.");
                }
            });
        }

        private void HandleBossDamaged(DamageInfo info)
        {
            CaptureEventContract(() => bossDamagedDuringShotCount++);
        }

        private void HandleBossDied()
        {
            CaptureEventContract(() =>
            {
                bossDiedCount++;
                bossDiedFrame = currentFrame;
                bossDiedSequence = ++eventSequence;
            });
        }

        private void HandleEncounterTerminalResolved(
            EncounterTerminalResolution resolution)
        {
            CaptureEventContract(() =>
            {
                encounterTerminalResolvedCount++;
                terminalResolvedFrame = currentFrame;
                terminalResolvedSequence = ++eventSequence;
                if (resolution.Outcome != EncounterTerminalOutcome.Clear
                    || resolution.Reason != EncounterTerminalReason.BossTerminal)
                {
                    throw new InvalidOperationException(
                        "G08 coordinator resolved a non-victory/non-boss terminal.");
                }
            });
        }

        private void HandleAftermathStarted()
        {
            CaptureEventContract(() => aftermathStartedCount++);
        }

        private void HandleAftermathCompleted()
        {
            CaptureEventContract(() =>
            {
                aftermathCompletedCount++;
                aftermathCompletedFrame = currentFrame;
            });
        }

        private void HandleOverlayPresentationSucceeded(StageRunResultSummary summary)
        {
            CaptureEventContract(() =>
            {
                overlayPresentationSucceededCount++;
                if (summary == null || resultPresenter.CommittedSummary != summary)
                {
                    throw new InvalidOperationException(
                        "G08 overlay published a substituted result summary instance.");
                }
            });
        }

        private void CaptureEventContract(Action observe)
        {
            try
            {
                observe?.Invoke();
            }
            catch (Exception exception)
            {
                // Capture observers never throw back through product mutation.
                Fail(exception);
            }
        }

        private void CaptureGlobalState()
        {
            savedCaptureFramerate = Time.captureFramerate;
            savedTargetFrameRate = Application.targetFrameRate;
            savedFixedDeltaTime = Time.fixedDeltaTime;
            globalStateCaptured = true;
        }

        private void OnDisable()
        {
            TryLifecycleRestore();
        }

        private void OnDestroy()
        {
            TryLifecycleRestore();
        }

        private void TryLifecycleRestore()
        {
            if (!globalStateCaptured || stateRestored)
            {
                return;
            }

            try
            {
                RestoreCaptureOwnedState();
            }
            catch (Exception exception)
            {
                cleanupFailure = Combine(cleanupFailure, exception);
                Debug.LogException(exception, this);
            }
        }

        private void Fail(Exception exception)
        {
            Failure ??= exception;
            IsRunning = false;
        }

        private static T RequireSingleSceneComponent<T>(Scene scene)
            where T : Component
        {
            T[] values = FindSceneComponents<T>(scene);
            if (values.Length != 1)
            {
                throw new InvalidOperationException(
                    $"G08 expected exactly one {typeof(T).Name} in {scene.path}; found {values.Length}.");
            }

            return values[0];
        }

        private static TInterface FindSingleSceneInterface<TInterface>(Scene scene)
            where TInterface : class
        {
            TInterface[] values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .OfType<TInterface>()
                .ToArray();
            if (values.Length != 1)
            {
                throw new InvalidOperationException(
                    $"G08 expected exactly one {typeof(TInterface).Name} in {scene.path}; found {values.Length}.");
            }

            return values[0];
        }

        private static T[] FindSceneComponents<T>(Scene scene)
            where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return Array.Empty<T>();
            }

            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void CaptureFailure(ref Exception failure, Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }
        }

        private static Exception Combine(Exception first, Exception next)
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

        private static WaitUntil WaitForNextPlayerFrame()
        {
            int frame = Time.frameCount;
            return new WaitUntil(() => Time.frameCount != frame);
        }
    }
}
