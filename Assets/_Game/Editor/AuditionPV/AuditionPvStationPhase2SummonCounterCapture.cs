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
    /// Capture contract and output integration for the deterministic G06 Station
    /// gameplay source. The product-state director below owns shot preparation;
    /// Recorder orchestration can start after IsPrepared becomes true and stop
    /// after IsComplete becomes true.
    /// </summary>
    public static class AuditionPvStationPhase2SummonCounterCapture
    {
        internal const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        internal const string CrushNetProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaCrushNet.asset";
        internal const string PhaseTwoOpeningProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaSummonCurtain.asset";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhase2SummonCounterCapture.cs";
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
        internal const string PlayerSummonActionPath =
            "Assets/_Game/Scripts/Player/PlayerSummonSlot1Action.cs";
        internal const string PlayerSummonRuntimePath =
            "Assets/_Game/Scripts/Player/PlayerSummonSlot1Action.Runtime.cs";
        internal const string SummonSlotProfileScriptPath =
            "Assets/_Game/Scripts/Player/SummonSlotActionProfile.cs";
        internal const string SummonSlotProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset";
        internal const string SummonEnergyLadderPath =
            "Assets/_Game/Scripts/Combat/SummonEnergyLadder.cs";
        internal const string SummonFrontlineProxyPath =
            "Assets/_Game/Scripts/Combat/SummonFrontlineProxy.cs";
        internal const string SummonPressureScreenPath =
            "Assets/_Game/Scripts/Combat/SummonPressureScreen.cs";
        internal const string LaneActionProjectilePath =
            "Assets/_Game/Scripts/Combat/LaneActionProjectile.cs";
        internal const string SummonActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab";
        internal const string SummonProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        internal const string SummonEntryCuePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab";
        internal const string ActionCameraPath =
            "Assets/_Game/Scripts/Presentation/ActionCameraController.cs";
        internal const string ActionScreenPath =
            "Assets/_Game/Scripts/Presentation/ActionScreenCuePresenter.cs";
        internal const string PerfectDodgeTimeWarpPath =
            "Assets/_Game/Scripts/Presentation/PerfectDodgeTimeWarp.cs";
        internal const string PressurePositionControllerPath =
            "Assets/_Game/Scripts/Combat/BossPressurePositionController.cs";
        internal const string PressureActionDeckPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossPressureActionDeck_AkazaPhase2.asset";
        internal const string CombatVfxProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";
        internal const string PlayerCombatVfxDriverPath =
            "Assets/_Game/Scripts/Presentation/PlayerCombatVfxCueDriver.cs";
        internal const string GameplayPostProcessPath =
            "Assets/_Game/Art/Environment/OlympusCorridor/Profiles/DB_OlympusCorridor_PostProcess.asset";
        internal const string NoCrossWallPrefabPath =
            "Assets/_Game/Prefabs/VFX/Environment/PF_OlympusStation_NoCrossRedCubeZone.prefab";

        internal const string ShotId = "g06";
        internal const string BaselinesFolderName = "baselines";
        internal const string Bl03FileName =
            "BL03_AKAZA_PHASE2_CRUSHNET__HUDON__t00.000000.png";
        internal const string Bl06FileName =
            "BL06_AKAZA_PHASE2_PERFECT_DODGE__HUDON__t03.150000.png";
        internal const string Bl07FileName =
            "BL07_AKAZA_PHASE2_SUMMON_COUNTER__HUDON__t04.183333.png";
        internal const int FirstFrame = 0;
        internal const int LastFrame = 359;
        internal const int ExpectedFrameCount = 360;
        internal const int BeginWindupFrame = 1;
        internal const int FirePendingWaveFrame = 71;
        internal const int QueueDodgeFrame = 186;
        internal const int ImpactFrame = 188;
        internal const int ReleaseSummonInputFrame = 221;
        internal const int QueueSummonFrame = 222;
        internal const int RelockSummonInputFrame = 223;
        internal const int ScreenObservationFirstFrame = 239;
        internal const int ScreenObservationLastFrame = 249;
        internal const int RetainedProjectileInterceptFrame = 250;
        internal const int CounterHitFirstFrame = 251;
        internal const int CounterHitLastFrame = LastFrame;
        internal const int Bl07SourceFrame = RetainedProjectileInterceptFrame + 1;
        internal const int RecommendedSelectStartFrame = 180;
        internal const int RecommendedSelectEndFrame = 316;
        internal const int AuthoredSummonTier = 2;
        internal const float AuthoredSummonManaCost = 200f;
        internal const float AuthoredEnergyAfterUse = 100f;
        internal const float AuthoredCounterDamage = 29.439999f;
        internal const int PhaseTwoSettleFrames = 90;
        internal const int Bl03SourceFrame = 0;
        // The product presenter intentionally opens its screen-domain coroutine on
        // the frame after the real impact event. f188 remains the collision proof;
        // f189 is the first honest rendered screen-domain hero baseline.
        internal const int Bl06SourceFrame = ImpactFrame + 1;
        internal const int DeterministicRandomSeed = 0x4706;
        internal const float ProductScreenDomainAlpha = 0.14f;
        internal const float ProductScreenInvertAlpha = 0.015f;
        internal const float ProductScreenEdgeAlpha = 0.18f;
        internal const float ProductScreenGlitchAlpha = 0.03f;
        internal const float ProductScreenDomainSeconds = 0.42f;

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
                    + "90 fixed-60Hz Phase 2 camera/UI/animation settle frames; "
                    + "CrushNet BeginWindup f1, FirePendingWave f71, QueueDodge f186, "
                    + "real active projectile impact f188; screen-domain hero f189; "
                    + "authored Slot1 input release f221, public QueueSummonSlot1 f222, relock f223; "
                    + "unique tier-2 AllySummon pressure screen and retained CrushNet actual intercept f250; "
                    + "product automatic 29.44 counter projectile and exact boss CombatHealth.Damaged hit; "
                    + "the authored Station screen-domain profile (.14/.015/.18/.03, 0.42s) "
                    + "is used without a capture-time visual override; 2560x1440 PNG at 60fps."
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
                },
                new AuditionPvBaselineManifestEntry
                {
                    id = "bl07",
                    shotId = ShotId,
                    sourceFrame = Bl07SourceFrame,
                    fileName = Bl07FileName,
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
                PlayerSummonActionPath,
                PlayerSummonRuntimePath,
                SummonSlotProfileScriptPath,
                SummonSlotProfilePath,
                SummonEnergyLadderPath,
                SummonFrontlineProxyPath,
                SummonPressureScreenPath,
                LaneActionProjectilePath,
                SummonActorPrefabPath,
                SummonProjectilePrefabPath,
                SummonEntryCuePrefabPath,
                ActionCameraPath,
                ActionScreenPath,
                PerfectDodgeTimeWarpPath,
                PressurePositionControllerPath,
                PressureActionDeckPath,
                CombatVfxProfilePath,
                PlayerCombatVfxDriverPath,
                GameplayPostProcessPath,
                NoCrossWallPrefabPath
            };
        }

        internal static AuditionPvStationPhase2SummonCounterOutput ReserveNewOutput(
            DateTime startedAtUtc,
            AuditionPvGitSnapshot gitSnapshot = null)
        {
            AuditionPvGitSnapshot git = gitSnapshot
                ?? AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "G06 output reservation requires a successful Git provenance probe: "
                    + git.probeError);
            }

            string outputId = AuditionPvOutputPaths.CreateOutputId(
                "g06-station-phase2-summon-counter",
                startedAtUtc,
                git.commitSha,
                git.isDirty,
                git.dirtyStateHashSha256);
            return ReserveNewOutputForRoot(
                AuditionPvCaptureContract.OutputRoot,
                outputId);
        }

        internal static AuditionPvStationPhase2SummonCounterOutput ReserveNewOutputForRoot(
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
                return new AuditionPvStationPhase2SummonCounterOutput(
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
            AuditionPvStationPhase2SummonCounterOutput output,
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

        internal static AuditionPvStationPhase2SummonCounterDirector AttachToFreshActiveScene()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "The G06 product-state director can only run in Play Mode.");
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
                    "G06 requires a fresh OlympusStationCombatStage PlayMode scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<
                    AuditionPvStationPhase2SummonCounterDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The active scene already owns a G06 shot director.");
            }

            var root = new GameObject("[AuditionPV_G06_ProductStateDirector]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, activeScene);
            return root.AddComponent<AuditionPvStationPhase2SummonCounterDirector>();
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

    internal sealed class AuditionPvStationPhase2SummonCounterOutput : IDisposable
    {
        public readonly string captureId;
        public readonly string outputRoot;
        public readonly string outputDirectory;
        public readonly string baselineDirectory;
        public readonly AuditionPvRecorderSettingsBundle recorderSettings;

        internal AuditionPvStationPhase2SummonCounterOutput(
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
    public sealed class AuditionPvStationPhase2SummonCounterDirector : MonoBehaviour
    {
        private const double PhaseTwoPreparationTimeoutSeconds = 15d;
        private const float HealthTolerance = 0.001f;
        private readonly List<ProjectileLease> projectileLeases = new(8);
        private readonly List<BossBarrageProjectile> activeProjectiles = new(8);

        private OlympusStationAkazaPhase2FlowController flow;
        private BossBarrageEncounterController encounter;
        private CombatEncounterController canonicalEncounter;
        private BossBarrageEmitter emitter;
        private BossPressureActionDirector pressureActionDirector;
        private BossPressurePositionController pressurePositionController;
        private BossBarragePatternProfile crushNet;
        private BossBarragePatternProfile phaseTwoOpening;
        private CanvasGroup combatHud;
        private Canvas combatHudCanvas;
        private CombatHealth playerHealth;
        private PlayerMovementController playerMovement;
        private PlayerActionController playerAction;
        private PlayerRangedBasicAttackAction playerRangedBasic;
        private PlayerSummonSlot1Action playerSummon;
        private SummonSlotActionProfile summonProfile;
        private SummonEnergyLadder energyLadder;
        private CombatHealth bossHealth;
        private Collider playerCollider;
        private ActionCameraController actionCamera;
        private ActionScreenCuePresenter actionScreen;
        private SceneEntryNoticeOverlay entryNotice;
        private OlympusCorridorTutorialDirector tutorial;
        private IDisposable cadenceSuspensionLease;
        private BossPressurePositionController.MovementIntentLease
            bossMovementIntentLease;
        private PresentationClock.ManualLease presentationClockLease;
        private BossBarrageProjectile impactProjectile;
        private BossBarrageProjectile interceptedCrushNetProjectile;
        private SummonFrontlineProxy summonProxy;
        private SummonPressureScreen pressureScreen;
        private LaneActionProjectile counterProjectile;

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
        private float savedFixedDeltaTime;
        private float savedEnergyMana;
        private bool savedEnergyGainEnabled;
        private bool savedCaptureInputsUnlocked;
        private bool savedBossCompositionValid;
        private bool savedBossMovementEnabled;
        private Vector3 savedBossPosition;
        private Quaternion savedBossRotation = Quaternion.identity;
        private Vector3 savedBossLocalScale = Vector3.one;
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
        private float bossRiskAtFirstFrame;
        private float bossRiskAtFireFrame;
        private float bossRiskAtImpactFrame;
        private int initialSummonUseCount;
        private int initialSummonInterceptCount;
        private float initialSummonEnergy;
        private float lastObservedBossHealth;
        private float authoredCounterDamage;
        private int summonUsedEventCount;
        private int summonBlockedEventCount;
        private int screenInterceptEventCount;
        private int screenFirstObservedFrame = -1;
        private int retainedProjectileCountBeforeIntercept;
        private int activeCounterProjectileCountAfterIntercept;
        private int bossDamageEventCount;
        private int bossAllyDamageEventCount;
        private int bossCounterDamageEventCount;
        private int bossCounterDamageFrame = -1;
        private int counterProjectileDamageAppliedCount;
        private int counterProjectileDamageAppliedFrame = -1;
        private float bossCounterDamageAmount;
        private float bossCounterHealthDelta;
        private bool retainedProjectileImpactApplied;
        private bool fixedDeltaTimeExact;
        private bool actionEventsSubscribed;
        private bool healthEventsSubscribed;
        private bool summonActionEventsSubscribed;
        private bool bossDamageEventSubscribed;
        private bool pressureScreenEventSubscribed;
        private bool counterProjectileEventSubscribed;

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
        public bool ProductScreenProfileActive => actionScreen != null
            && actionScreen.PlayPerfectDodgeScreenDomain
            && Mathf.Abs(
                actionScreen.MaxPerfectDodgeDomainAlpha
                    - AuditionPvStationPhase2SummonCounterCapture
                        .ProductScreenDomainAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.MaxPerfectDodgeInvertAlpha
                    - AuditionPvStationPhase2SummonCounterCapture
                        .ProductScreenInvertAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.MaxPerfectDodgeEdgeAlpha
                    - AuditionPvStationPhase2SummonCounterCapture
                        .ProductScreenEdgeAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.PerfectDodgeGlitchOverlayAlpha
                    - AuditionPvStationPhase2SummonCounterCapture
                        .ProductScreenGlitchAlpha) <= HealthTolerance
            && Mathf.Abs(
                actionScreen.PerfectDodgeDomainSeconds
                    - AuditionPvStationPhase2SummonCounterCapture
                        .ProductScreenDomainSeconds) <= HealthTolerance;
        public bool ScreenProfileRestored => stateRestored
            && ProductScreenProfileActive
            && !PerfectDodgeScreenDomainRuntime.HasActiveCue;
        public bool FixedDeltaTimeRestored => stateRestored
            && Mathf.Abs(Time.fixedDeltaTime - savedFixedDeltaTime)
                <= 0.000001f;
        public bool CaptureInputLocksReleased => stateRestored
            && savedCaptureInputsUnlocked
            && playerAction != null
            && !playerAction.IsCinematicInputLocked
            && playerMovement != null
            && !playerMovement.IsCinematicMoveInputLocked
            && playerRangedBasic != null
            && !playerRangedBasic.IsCinematicInputLocked
            && playerSummon != null
            && !playerSummon.IsCinematicInputLocked;
        public bool CaptureHudStateRestored => stateRestored
            && savedHudValid
            && combatHud != null
            && Mathf.Abs(combatHud.alpha - savedHudAlpha) <= 0.000001f
            && combatHud.interactable == savedHudInteractable
            && combatHud.blocksRaycasts == savedHudBlocksRaycasts;
        public bool CaptureEventsReleased => stateRestored
            && !actionEventsSubscribed
            && !healthEventsSubscribed
            && !summonActionEventsSubscribed
            && !bossDamageEventSubscribed
            && !pressureScreenEventSubscribed
            && !counterProjectileEventSubscribed;
        public bool CaptureSummonArtifactsReleased => stateRestored
            && playerSummon != null
            && !playerSummon.IsCinematicInputLocked
            && !playerSummon.IsSlotOnCooldown
            && playerSummon.ActiveProjectileCount == 0
            && playerSummon.ActivePressureScreenCount == 0
            && playerSummon.ActiveSummonActorCount == 0;
        public bool BossCompositionRestored => stateRestored
            && savedBossCompositionValid
            && pressurePositionController != null
            && pressurePositionController.MovementIntentOverrideCount == 0
            && pressurePositionController.MovementEnabled == savedBossMovementEnabled
            && pressurePositionController.MovedTransform != null
            && Vector3.Distance(
                pressurePositionController.MovedTransform.position,
                savedBossPosition) <= 0.001f
            && Quaternion.Angle(
                pressurePositionController.MovedTransform.rotation,
                savedBossRotation) <= 0.01f
            && Vector3.Distance(
                pressurePositionController.MovedTransform.localScale,
                savedBossLocalScale) <= 0.001f;
        public Vector3 PreparedCameraPosition => preparedCameraPosition;
        public Quaternion PreparedCameraRotation => preparedCameraRotation;
        public float PreparedCameraFieldOfView => preparedCameraFieldOfView;
        public float BossRiskAtFirstFrame => bossRiskAtFirstFrame;
        public float BossRiskAtFireFrame => bossRiskAtFireFrame;
        public float BossRiskAtImpactFrame => bossRiskAtImpactFrame;
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
            && (currentFrame
                    < AuditionPvStationPhase2SummonCounterCapture.QueueSummonFrame
                ? energyLadder.IsCapped
                    && energyLadder.AvailableTier == 3
                    && Mathf.Abs(
                        energyLadder.CurrentMana - energyLadder.MaxMana)
                        <= HealthTolerance
                : energyLadder.AvailableTier == 1
                    && Mathf.Abs(
                        energyLadder.CurrentMana
                            - AuditionPvStationPhase2SummonCounterCapture
                                .AuthoredEnergyAfterUse) <= HealthTolerance);
        public bool UsesExactEnergyLadderBinding => energyLadder != null
            && energyLadder == encounter?.EnergyLadder;
        public int SummonUsedEventCount => summonUsedEventCount;
        public int SummonBlockedEventCount => summonBlockedEventCount;
        public int ScreenInterceptEventCount => screenInterceptEventCount;
        public int ScreenFirstObservedFrame => screenFirstObservedFrame;
        public int RetainedProjectileCountBeforeIntercept =>
            retainedProjectileCountBeforeIntercept;
        public bool RetainedProjectileImpactApplied =>
            retainedProjectileImpactApplied;
        public bool RetainedProjectileInactive =>
            interceptedCrushNetProjectile != null
            && !interceptedCrushNetProjectile.IsActive;
        public int ActiveCounterProjectileCountAfterIntercept =>
            activeCounterProjectileCountAfterIntercept;
        public int BossDamageEventCount => bossDamageEventCount;
        public int BossAllyDamageEventCount => bossAllyDamageEventCount;
        public int BossCounterDamageEventCount => bossCounterDamageEventCount;
        public int BossCounterDamageFrame => bossCounterDamageFrame;
        public int CounterProjectileDamageAppliedCount =>
            counterProjectileDamageAppliedCount;
        public int CounterProjectileDamageAppliedFrame =>
            counterProjectileDamageAppliedFrame;
        public float AuthoredCounterDamage => authoredCounterDamage;
        public float BossCounterDamageAmount => bossCounterDamageAmount;
        public float BossCounterHealthDelta => bossCounterHealthDelta;
        public bool FixedDeltaTimeExact => fixedDeltaTimeExact;
        public int SummonSpentTier => playerSummon != null
            ? playerSummon.LastSpentTier
            : 0;
        public int SummonUseCountDelta => playerSummon != null
            ? playerSummon.TotalUseCount - initialSummonUseCount
            : 0;
        public int SummonInterceptCountDelta => playerSummon != null
            ? playerSummon.TotalPressureScreenInterceptCount
                - initialSummonInterceptCount
            : 0;
        public int SummonPressureScreenTier => pressureScreen != null
            ? pressureScreen.ActiveTier
            : 0;
        public int SummonPressureScreenRemainingIntercepts =>
            pressureScreen != null ? pressureScreen.RemainingIntercepts : 0;
        public bool UniqueSummonPressureScreenObserved =>
            screenFirstObservedFrame >=
                AuditionPvStationPhase2SummonCounterCapture
                    .ScreenObservationFirstFrame
            && screenFirstObservedFrame <=
                AuditionPvStationPhase2SummonCounterCapture
                    .ScreenObservationLastFrame;
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
        public float SummonEnergyBeforeUse => initialSummonEnergy;
        public float SummonEnergyAfterUse => energyLadder != null
            ? energyLadder.CurrentMana
            : -1f;

        public IEnumerator PrepareFreshProductState()
        {
            if (IsPrepared || IsRunning || stateRestored)
            {
                throw new InvalidOperationException(
                    "The G06 director cannot be prepared more than once.");
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
                        "G06 must not use a Time.timeScale freeze.");
                }

                savedRandomState = UnityEngine.Random.state;
                savedRandomStateValid = true;
                UnityEngine.Random.InitState(
                    AuditionPvStationPhase2SummonCounterCapture.DeterministicRandomSeed);
                PerfectDodgeScreenDomainRuntime.Clear();
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;
                Time.fixedDeltaTime = 1f / AuditionPvCaptureContract.Fps;
                fixedDeltaTimeExact = Mathf.Abs(
                    Time.fixedDeltaTime
                        - 1f / AuditionPvCaptureContract.Fps) <= 0.000001f;
                if (!fixedDeltaTimeExact)
                {
                    throw new InvalidOperationException(
                        "G06 could not acquire an exact fixed 60 Hz physics cadence.");
                }

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
                CaptureBossCompositionState();
                AcquireDeterministicEncounterControl();
                AcquireBossCompositionLease();
                presentationClockLease = PresentationClock.AcquireManual(
                    this,
                    AuditionPvCaptureContract.Fps);
                for (int settleFrame = 0;
                    settleFrame
                        < AuditionPvStationPhase2SummonCounterCapture.PhaseTwoSettleFrames;
                    settleFrame++)
                {
                    presentationClockLease.SetFrame(settleFrame);
                    // UnityTest/editor coroutine iterations can advance more
                    // than once inside a player loop. Waiting on Time.frameCount
                    // makes these exact real camera/UI/animation frames and
                    // remains valid in headless focused-test runs.
                    yield return WaitForNextPlayerFrame();
                }

                CapturePreparedCameraState();
                // The authored handoff uses realtime duration, so any incidental
                // VFX consumption before this point must not affect the source
                // interval's deterministic random stream.
                UnityEngine.Random.InitState(
                    AuditionPvStationPhase2SummonCounterCapture.DeterministicRandomSeed);
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
                    "Prepare the fresh G06 product state exactly once before beginning capture.");
            }

            if (Time.timeScale <= 0f)
            {
                throw new InvalidOperationException(
                    "G06 cannot begin while gameplay time is frozen.");
            }

            ValidateCanonicalEncounterReadyForShot();
            if (Mathf.Abs(playerHealth.CurrentHealth - playerHealth.MaxHealth)
                    > HealthTolerance
                || playerHealth.IsInvulnerable)
            {
                throw new InvalidOperationException(
                    "G06 must begin with full HP and no unrelated invulnerability.");
            }

            if (recorderOwnsCadence)
            {
                float minimumRecorderDelta =
                    1f / AuditionPvCaptureContract.Fps;
                if (Time.captureDeltaTime <= minimumRecorderDelta
                    || Time.captureDeltaTime >= minimumRecorderDelta + 0.001f)
                {
                    throw new InvalidOperationException(
                        "G06 Recorder cadence/padding is not active before logical f0: "
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
                AuditionPvStationPhase2SummonCounterCapture.FirstFrame);
            initialCameraMicroShakeCount = actionCamera.MicroShakeRequestCount;
            initialPlayerHealth = playerHealth.CurrentHealth;
            initialSummonUseCount = playerSummon.TotalUseCount;
            initialSummonInterceptCount =
                playerSummon.TotalPressureScreenInterceptCount;
            initialSummonEnergy = energyLadder.CurrentMana;
            lastObservedBossHealth = bossHealth.CurrentHealth;
            playerSummon.SummonSlot1Used += HandleSummonSlot1Used;
            playerSummon.SummonPressureBlocked +=
                HandleSummonPressureBlocked;
            bossHealth.Damaged += HandleBossDamaged;
            summonActionEventsSubscribed = true;
            bossDamageEventSubscribed = true;
            currentFrame = AuditionPvStationPhase2SummonCounterCapture.FirstFrame;
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
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    bossMovementIntentLease?.Dispose();
                    bossMovementIntentLease = null;
                });
                CaptureRestoreFailure(
                    ref firstFailure,
                    PerfectDodgeScreenDomainRuntime.Clear);
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (playerAction != null)
                    {
                        playerAction.PerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
                        playerAction.DodgeStarted -= HandleDodgeStarted;
                        actionEventsSubscribed = false;
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
                        healthEventsSubscribed = false;
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (playerSummon != null)
                    {
                        playerSummon.SummonSlot1Used -= HandleSummonSlot1Used;
                        playerSummon.SummonPressureBlocked -=
                            HandleSummonPressureBlocked;
                        playerSummon.SetCinematicInputLocked(
                            PlayerInputLockSource.EditorVerification,
                            false);
                        playerSummon.DismissActivePressureScreens();
                        playerSummon.ClearSlotCooldown();
                        summonActionEventsSubscribed = false;
                    }

                    if (bossHealth != null)
                    {
                        bossHealth.Damaged -= HandleBossDamaged;
                        bossDamageEventSubscribed = false;
                    }

                    if (pressureScreen != null)
                    {
                        pressureScreen.Intercepted -=
                            HandlePressureScreenIntercepted;
                        pressureScreenEventSubscribed = false;
                    }

                    if (counterProjectile != null)
                    {
                        counterProjectile.DamageApplied -=
                            HandleCounterProjectileDamageApplied;
                        counterProjectileEventSubscribed = false;
                    }

                    DeactivateCaptureSummonArtifacts();
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
                CaptureRestoreFailure(ref firstFailure, RestoreBossCompositionState);
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
                    Time.fixedDeltaTime = savedFixedDeltaTime;
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
                    "G06 shot-state restoration encountered an error.",
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
                    "G06 could not restore the authored queued priority pattern.");
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
                        "G06 lost exclusive control of the boss cadence scheduler.");
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
                    == AuditionPvStationPhase2SummonCounterCapture.ImpactFrame)
                {
                    ApplyActualProjectileImpact();
                }

                ObserveSummonState();
                if (currentFrame
                    == AuditionPvStationPhase2SummonCounterCapture
                        .RetainedProjectileInterceptFrame)
                {
                    ApplyActualPressureScreenIntercept();
                }

                ObserveCueState();
                if (currentFrame
                    == AuditionPvStationPhase2SummonCounterCapture.LastFrame)
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
                    AuditionPvStationPhase2SummonCounterCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G06 requires a fresh OlympusStationCombatStage PlayMode scene.");
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
            pressureActionDirector = flow.PressureActionDirector;
            pressurePositionController = flow.PressurePositionController;
            combatHud = flow.CombatHudCanvasGroup;
            combatHudCanvas = combatHud != null
                ? combatHud.GetComponentInParent<Canvas>(includeInactive: true)
                : null;
            playerHealth = flow.PlayerHealth;
            playerMovement = flow.PlayerMovement;
            playerAction = flow.PlayerActionController;
            playerRangedBasic = flow.PlayerRangedBasicAttackAction;
            playerSummon = UnityEngine.Object.FindObjectsByType<
                    PlayerSummonSlot1Action>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(candidate =>
                    candidate != null
                    && candidate.gameObject == playerHealth?.gameObject);
            energyLadder = encounter != null ? encounter.EnergyLadder : null;
            bossHealth = flow.BossHealth;
            summonProfile = AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(
                AuditionPvStationPhase2SummonCounterCapture
                    .SummonSlotProfilePath);
            crushNet = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                AuditionPvStationPhase2SummonCounterCapture.CrushNetProfilePath);
            phaseTwoOpening =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2SummonCounterCapture
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
                || pressureActionDirector == null
                || pressurePositionController == null
                || pressurePositionController.LaneSpace == null
                || pressurePositionController.MovedTransform == null
                || combatHud == null
                || combatHudCanvas == null
                || playerHealth == null
                || playerMovement == null
                || playerAction == null
                || playerRangedBasic == null
                || playerSummon == null
                || summonProfile == null
                || energyLadder == null
                || bossHealth == null
                || canonicalEncounter == null
                || crushNet == null
                || phaseTwoOpening == null
                || actionCamera == null
                || actionScreen == null)
            {
                throw new InvalidOperationException(
                    "G06 could not resolve its exact Flow, HUD, player, pattern, camera, or screen bindings.");
            }

            if (playerAction.gameObject != playerHealth.gameObject
                || !playerAction.isActiveAndEnabled
                || playerSummon.gameObject != playerHealth.gameObject
                || !playerSummon.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "The exact Flow player action and Slot1 action are not active on the exact Flow player health.");
            }

            PlayerSummonSlot1Action.SummonTierSettings[] summonTiers =
                summonProfile.CopyTierSettings();
            int authoredTierIndex =
                AuditionPvStationPhase2SummonCounterCapture.AuthoredSummonTier - 1;
            if (playerSummon.SummonActionProfile != summonProfile
                || summonTiers.Length <= authoredTierIndex
                || Mathf.Abs(
                    playerSummon.RequiredSummonMana
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredSummonManaCost) > HealthTolerance
                || summonTiers[authoredTierIndex].ScreenIntercepts != 2
                || Mathf.Abs(
                    summonTiers[authoredTierIndex].CounterDamage
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredCounterDamage) > HealthTolerance)
            {
                throw new InvalidOperationException(
                    "G06 requires the authored Station Slot1 200-EN tier-2 "
                    + "ChargeBruiser screen/counter profile without a capture override.");
            }

            authoredCounterDamage = summonTiers[authoredTierIndex].CounterDamage;

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
                    "G06 requires a real active collider owned by the Flow player health.");
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
            savedFixedDeltaTime = Time.fixedDeltaTime;
            savedEnergyMana = energyLadder.CurrentMana;
            savedEnergyGainEnabled = energyLadder.CurrentEnergyPerSecond > 0f;
            savedCaptureInputsUnlocked = !playerAction.IsCinematicInputLocked
                && !playerMovement.IsCinematicMoveInputLocked
                && !playerRangedBasic.IsCinematicInputLocked
                && !playerSummon.IsCinematicInputLocked;
            if (!savedEnergyGainEnabled)
            {
                throw new InvalidOperationException(
                    "The fresh Station summon-energy ladder must begin with gain enabled.");
            }

            if (!savedCaptureInputsUnlocked)
            {
                throw new InvalidOperationException(
                    "G06 requires a fresh Station baseline with action, movement, ranged, and Slot1 input unlocked.");
            }

            if (!ProductScreenProfileActive)
            {
                throw new InvalidOperationException(
                    "G06 requires the authored Station perfect-dodge screen profile without capture-time overrides.");
            }

            restorableStateCaptured = true;
        }

        private void CaptureBossCompositionState()
        {
            Transform movedBoss = pressurePositionController.MovedTransform;
            savedBossMovementEnabled = pressurePositionController.MovementEnabled;
            savedBossPosition = movedBoss.position;
            savedBossRotation = movedBoss.rotation;
            savedBossLocalScale = movedBoss.localScale;
            savedBossCompositionValid = true;
        }

        private void AcquireBossCompositionLease()
        {
            ResolveAuthoredCrushNetMovement(
                out BossPressureMovementIntent movementIntent,
                out BossPressureActionKind actionKind);
            if (!savedBossCompositionValid
                || !savedBossMovementEnabled
                || !pressurePositionController.TryAcquireMovementIntentOverride(
                    this,
                    movementIntent,
                    actionKind,
                    out bossMovementIntentLease))
            {
                throw new InvalidOperationException(
                    "G06 could not acquire the authored CrushNet CommitForward movement lease.");
            }

            if (pressurePositionController.MovementIntentOverrideCount != 1)
            {
                throw new InvalidOperationException(
                    "G06 must own exactly one boss movement-intent lease.");
            }
        }

        private void ResolveAuthoredCrushNetMovement(
            out BossPressureMovementIntent movementIntent,
            out BossPressureActionKind actionKind)
        {
            int matchingSlotCount = 0;
            movementIntent = default;
            actionKind = default;
            for (int slotIndex = 0;
                slotIndex < pressureActionDirector.ActionSlotCount;
                slotIndex++)
            {
                if (!pressureActionDirector.TryGetActionSlot(
                        slotIndex,
                        out BossPressureActionDirector.BossPressureActionSlot slot)
                    || slot.Pattern != crushNet)
                {
                    continue;
                }

                matchingSlotCount++;
                movementIntent = slot.MovementIntent;
                actionKind = slot.ActionKind;
            }

            if (matchingSlotCount != 1
                || movementIntent != BossPressureMovementIntent.CommitForward
                || actionKind != BossPressureActionKind.PunishOverextend)
            {
                throw new InvalidOperationException(
                    "G06 requires exactly one authored CrushNet slot with "
                    + "PunishOverextend/CommitForward movement, but resolved "
                    + $"matches={matchingSlotCount}, action={actionKind}, movement={movementIntent}.");
            }
        }

        private void RestoreBossCompositionState()
        {
            if (!savedBossCompositionValid || pressurePositionController == null)
            {
                return;
            }

            Transform movedBoss = pressurePositionController.MovedTransform;
            if (movedBoss == null)
            {
                throw new InvalidOperationException(
                    "G06 lost the canonical moved boss transform during cleanup.");
            }

            pressurePositionController.SetMovementEnabled(false);
            movedBoss.SetPositionAndRotation(savedBossPosition, savedBossRotation);
            movedBoss.localScale = savedBossLocalScale;
            pressurePositionController.SetMovementEnabled(savedBossMovementEnabled);
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
                    "G06 must start from a fresh Phase 1 Station boss state.");
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
                    "G06 could not reach exactly one completed authored Phase 2 handoff: "
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
                    "G06 expected the dormant authored Phase 2 opening before capture control: "
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
                    "G06 could not acquire its owner-scoped cadence suspension lease.");
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
                        "G06 could not cancel the dormant authored Phase 2 opening "
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
                    "G06 could not resolve the authored gameplay Camera component.");
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
                    "G06 fixed-settle gameplay camera state is invalid.");
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
                    "G06 requires the fresh real Flow player at full health.");
            }

            if (playerHealth.IsInvulnerable)
            {
                throw new InvalidOperationException(
                    "G06 cannot stage while an unrelated player invulnerability is active.");
            }
            if (playerRangedBasic.CurrentAmmo != playerRangedBasic.MagazineSize
                || playerRangedBasic.IsReloading)
            {
                throw new InvalidOperationException(
                    "G06 requires the fresh Flow ranged action at full magazine and not reloading.");
            }

            energyLadder.ResetLadder();
            energyLadder.GrantCurrentTierEnergy(energyLadder.MaxMana);
            energyLadder.SetGainEnabled(false);
            if (!ProductScreenProfileActive)
            {
                throw new InvalidOperationException(
                    "G06 may not replace the authored Station perfect-dodge screen profile.");
            }
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
                    "G06 HUD resources are not full-ammo, idle-reload, and capped tier-3 energy.");
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
            playerSummon.SetCinematicInputLocked(
                PlayerInputLockSource.EditorVerification,
                true);
            playerAction.PerfectDodgeTriggered += HandlePerfectDodgeTriggered;
            playerAction.DodgeStarted += HandleDodgeStarted;
            playerHealth.DamageBlockedByInvulnerability +=
                HandleDamageBlockedObservation;
            playerHealth.DamageModifying += HandleDamageModifyingObservation;
            actionEventsSubscribed = true;
            healthEventsSubscribed = true;
        }

        private void ExecuteEarlyFrameAction(int frameIndex)
        {
            switch (frameIndex)
            {
                case AuditionPvStationPhase2SummonCounterCapture.BeginWindupFrame:
                    if (!emitter.BeginWindup()
                        || emitter.CurrentPattern != crushNet
                        || !emitter.IsWindupActive)
                    {
                        throw new InvalidOperationException(
                            "G06 f1 did not begin the authored CrushNet windup.");
                    }

                    break;

                case AuditionPvStationPhase2SummonCounterCapture.FirePendingWaveFrame:
                    FireActualCrushNetWave();
                    break;

                case AuditionPvStationPhase2SummonCounterCapture.QueueDodgeFrame - 1:
                    // The f188 block must be owned only by the upcoming dodge,
                    // never by a transition or preparation immunity.
                    preparationSafetyExpiredBeforeDodge =
                        !playerHealth.IsInvulnerable;
                    if (!preparationSafetyExpiredBeforeDodge)
                    {
                        throw new InvalidOperationException(
                            "G06 preparation invulnerability remained active at f185.");
                    }

                    // Release only this capture owner's action lock. Movement and
                    // ranged input remain locked; QueueDodge must traverse the real
                    // public action-input path on the following frame.
                    playerAction.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false);
                    break;

                case AuditionPvStationPhase2SummonCounterCapture.QueueDodgeFrame:
                    if (playerAction.IsCinematicInputLocked)
                    {
                        throw new InvalidOperationException(
                            "G06 f186 action path remained locked after releasing the capture owner.");
                    }

                    playerAction.QueueDodge();
                    break;

                case AuditionPvStationPhase2SummonCounterCapture.ImpactFrame + 1:
                    playerAction.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        true);
                    break;

                case AuditionPvStationPhase2SummonCounterCapture
                    .ReleaseSummonInputFrame:
                    playerSummon.SetCinematicInputLocked(
                        PlayerInputLockSource.EditorVerification,
                        false);
                    if (playerSummon.IsCinematicInputLocked)
                    {
                        throw new InvalidOperationException(
                            "G06 f221 Slot1 remains locked after releasing the capture owner.");
                    }

                    break;

                case AuditionPvStationPhase2SummonCounterCapture.QueueSummonFrame:
                    QueueActualSummonSlot1();
                    break;

                case AuditionPvStationPhase2SummonCounterCapture
                    .RelockSummonInputFrame:
                    playerSummon.SetCinematicInputLocked(
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
                    $"G06 f71 expected {crushNet.ProjectilesPerWave} live CrushNet projectiles; "
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

        private void QueueActualSummonSlot1()
        {
            if (playerSummon.IsCinematicInputLocked
                || playerSummon.IsSlotOnCooldown
                || Mathf.Abs(
                    playerSummon.RequiredSummonMana
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredSummonManaCost) > HealthTolerance
                || Mathf.Abs(
                    energyLadder.CurrentMana - energyLadder.MaxMana)
                    > HealthTolerance
                || energyLadder.AvailableTier != 3)
            {
                throw new InvalidOperationException(
                    "G06 f222 cannot enter the authored public Slot1 queue path: "
                    + $"locked={playerSummon.IsCinematicInputLocked}, "
                    + $"cooldown={playerSummon.SlotCooldownRemaining:F3}, "
                    + $"cost={playerSummon.RequiredSummonMana:F3}, "
                    + $"energy={energyLadder.CurrentMana:F3}/{energyLadder.MaxMana:F3}, "
                    + $"availableTier={energyLadder.AvailableTier}.");
            }

            playerSummon.QueueSummonSlot1();
            if (summonUsedEventCount != 1
                || playerSummon.TotalUseCount - initialSummonUseCount != 1
                || playerSummon.LastSpentTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || Mathf.Abs(
                    energyLadder.CurrentMana
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredEnergyAfterUse) > HealthTolerance
                || energyLadder.AvailableTier != 1)
            {
                throw new InvalidOperationException(
                    "G06 f222 did not complete exactly one real authored Slot1 use: "
                    + $"usedEvents={summonUsedEventCount}, "
                    + $"useDelta={playerSummon.TotalUseCount - initialSummonUseCount}, "
                    + $"spentTier={playerSummon.LastSpentTier}, "
                    + $"energy={initialSummonEnergy:F3}->{energyLadder.CurrentMana:F3}, "
                    + $"availableTier={energyLadder.AvailableTier}.");
            }
        }

        private void StageProjectileTravel(int frameIndex)
        {
            if (projectileLeases.Count == 0
                || frameIndex
                    < AuditionPvStationPhase2SummonCounterCapture.FirePendingWaveFrame)
            {
                return;
            }

            float travel01 = Mathf.InverseLerp(
                AuditionPvStationPhase2SummonCounterCapture.FirePendingWaveFrame,
                AuditionPvStationPhase2SummonCounterCapture.ImpactFrame,
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
                    "G06 f188 does not own the real active CrushNet projectile selected at f71.");
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
                    "G06 f188 failed the real projectile, exactly-one perfect-dodge, "
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

        private void ObserveSummonState()
        {
            if (currentFrame
                    < AuditionPvStationPhase2SummonCounterCapture
                        .ScreenObservationFirstFrame
                || currentFrame
                    > AuditionPvStationPhase2SummonCounterCapture
                        .RetainedProjectileInterceptFrame)
            {
                return;
            }

            SummonFrontlineProxy resolvedProxy = null;
            int matchingCount = 0;
            int activeProxyCount = SummonFrontlineProxy.ActiveRegisteredProxyCount;
            for (int index = 0; index < activeProxyCount; index++)
            {
                if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(
                        index,
                        out SummonFrontlineProxy candidate)
                    || candidate == null
                    || candidate.OwnerTeam != DamageTeam.AllySummon
                    || candidate.ActiveTier
                        != AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredSummonTier
                    || candidate.PressureScreen == null
                    || !candidate.PressureScreen.IsActive)
                {
                    continue;
                }

                matchingCount++;
                resolvedProxy = candidate;
            }

            if (matchingCount == 0)
            {
                if (currentFrame
                    >= AuditionPvStationPhase2SummonCounterCapture
                        .ScreenObservationLastFrame)
                {
                    throw new InvalidOperationException(
                        $"G06 did not observe the authored active tier-2 Slot1 pressure screen by f{currentFrame}.");
                }

                return;
            }

            if (matchingCount != 1
                || resolvedProxy == null
                || resolvedProxy.PressureScreen == null
                || resolvedProxy.PressureScreen.OwnerTeam
                    != DamageTeam.AllySummon
                || resolvedProxy.PressureScreen.ActiveTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || resolvedProxy.PressureScreen.MaxIntercepts != 2)
            {
                throw new InvalidOperationException(
                    "G06 requires exactly one active authored AllySummon tier-2 pressure screen: "
                    + $"matches={matchingCount}.");
            }

            if (summonProxy != null && summonProxy != resolvedProxy)
            {
                throw new InvalidOperationException(
                    "G06 observed a different Slot1 summon proxy during the same shot.");
            }

            summonProxy = resolvedProxy;
            if (pressureScreen == null)
            {
                pressureScreen = summonProxy.PressureScreen;
                pressureScreen.Intercepted += HandlePressureScreenIntercepted;
                pressureScreenEventSubscribed = true;
                screenFirstObservedFrame = currentFrame;
            }
            else if (pressureScreen != summonProxy.PressureScreen)
            {
                throw new InvalidOperationException(
                    "G06 pressure-screen identity changed before the retained projectile intercept.");
            }
        }

        private void ApplyActualPressureScreenIntercept()
        {
            if (summonProxy == null
                || pressureScreen == null
                || !pressureScreen.IsActive
                || pressureScreen.ActiveTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || pressureScreen.InterceptedProjectiles != 0
                || pressureScreen.RemainingIntercepts != 2
                || summonBlockedEventCount != 0
                || screenInterceptEventCount != 0)
            {
                throw new InvalidOperationException(
                    "G06 f250 did not own one fresh active authored tier-2 pressure screen.");
            }

            retainedProjectileCountBeforeIntercept =
                emitter.CopyActiveProjectiles(activeProjectiles);
            if (retainedProjectileCountBeforeIntercept
                    != firedProjectileCount - 1
                || retainedProjectileCountBeforeIntercept != 6
                || emitter.CurrentPattern != crushNet)
            {
                throw new InvalidOperationException(
                    "G06 f250 did not retain the six non-impact real CrushNet projectiles: "
                    + $"retained={retainedProjectileCountBeforeIntercept}, "
                    + $"fired={firedProjectileCount}, "
                    + $"pattern={(emitter.CurrentPattern != null ? emitter.CurrentPattern.name : "none")}.");
            }

            interceptedCrushNetProjectile = activeProjectiles
                .FirstOrDefault(candidate =>
                    candidate != null
                    && candidate != impactProjectile
                    && candidate.IsActive
                    && candidate.SourceTeam == DamageTeam.Enemy);
            SphereCollider screenCollider =
                pressureScreen.GetComponent<SphereCollider>();
            if (interceptedCrushNetProjectile == null
                || screenCollider == null
                || !screenCollider.enabled
                || !screenCollider.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(
                    "G06 f250 could not bind a retained real CrushNet projectile to the live screen collider.");
            }

            int activeActionProjectilesBefore =
                playerSummon.ActiveProjectileCount;
            interceptedCrushNetProjectile.transform.position =
                screenCollider.bounds.center;
            Physics.SyncTransforms();
            retainedProjectileImpactApplied =
                interceptedCrushNetProjectile.TryApplyImpact(
                    screenCollider,
                    screenCollider.bounds.center);
            activeCounterProjectileCountAfterIntercept =
                playerSummon.ActiveProjectileCount;

            LaneActionProjectile[] laneProjectiles =
                UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            LaneActionProjectile[] matchingCounters = laneProjectiles
                .Where(candidate =>
                    candidate != null
                    && candidate.IsActive
                    && candidate.SourceHealth == playerHealth
                    && candidate.SourceTeam == DamageTeam.AllySummon
                    && Mathf.Abs(candidate.Damage - authoredCounterDamage)
                        <= HealthTolerance)
                .ToArray();
            if (matchingCounters.Length == 1)
            {
                counterProjectile = matchingCounters[0];
                counterProjectile.DamageApplied +=
                    HandleCounterProjectileDamageApplied;
                counterProjectileEventSubscribed = true;
            }

            if (!retainedProjectileImpactApplied
                || interceptedCrushNetProjectile.IsActive
                || screenInterceptEventCount != 1
                || summonBlockedEventCount != 1
                || playerSummon.LastPressureScreenInterceptCount != 1
                || playerSummon.LastPressureScreenInterceptTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || playerSummon.TotalPressureScreenInterceptCount
                        - initialSummonInterceptCount != 1
                || pressureScreen.InterceptedProjectiles != 1
                || pressureScreen.RemainingIntercepts != 1
                || !pressureScreen.IsActive
                || activeActionProjectilesBefore != 0
                || activeCounterProjectileCountAfterIntercept != 1
                || matchingCounters.Length != 1
                || counterProjectile == null)
            {
                throw new InvalidOperationException(
                    "G06 f250 failed the real retained projectile -> screen -> automatic counter contract: "
                    + $"impact={retainedProjectileImpactApplied}, "
                    + $"retainedActive={interceptedCrushNetProjectile.IsActive}, "
                    + $"screenEvents={screenInterceptEventCount}, "
                    + $"blockedEvents={summonBlockedEventCount}, "
                    + $"actionIntercept={playerSummon.LastPressureScreenInterceptCount}, "
                    + $"screen={pressureScreen.InterceptedProjectiles}/{pressureScreen.MaxIntercepts}, "
                    + $"actionProjectiles={activeActionProjectilesBefore}->{activeCounterProjectileCountAfterIntercept}, "
                    + $"matchingCounters={matchingCounters.Length}.");
            }
        }

        private void DeactivateCaptureSummonArtifacts()
        {
            LaneActionProjectile[] projectiles =
                UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            for (int index = 0; index < projectiles.Length; index++)
            {
                LaneActionProjectile projectile = projectiles[index];
                if (projectile != null
                    && projectile.IsActive
                    && projectile.SourceHealth == playerHealth
                    && projectile.SourceTeam == DamageTeam.AllySummon)
                {
                    projectile.Deactivate();
                }
            }

            if (summonProxy != null && summonProxy.IsActive)
            {
                summonProxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
            }
        }

        private void ObserveCueState()
        {
            if (currentFrame == AuditionPvStationPhase2SummonCounterCapture.FirstFrame)
            {
                bossRiskAtFirstFrame = pressurePositionController.CurrentRisk01;
            }
            else if (currentFrame
                == AuditionPvStationPhase2SummonCounterCapture.FirePendingWaveFrame)
            {
                bossRiskAtFireFrame = pressurePositionController.CurrentRisk01;
            }
            else if (currentFrame
                == AuditionPvStationPhase2SummonCounterCapture.ImpactFrame)
            {
                bossRiskAtImpactFrame = pressurePositionController.CurrentRisk01;
            }

            sawCameraCue |= actionCamera.MicroShakeRequestCount
                > initialCameraMicroShakeCount;
            sawScreenCue |= PerfectDodgeScreenDomainRuntime.HasActiveCue;
            if (currentFrame
                == AuditionPvStationPhase2SummonCounterCapture.Bl06SourceFrame)
            {
                screenCueActiveAtBaselineFrame =
                    PerfectDodgeScreenDomainRuntime.HasActiveCue;
            }
        }

        private void ValidateCompletedShot()
        {
            if (currentFrame
                    != AuditionPvStationPhase2SummonCounterCapture.LastFrame
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
                || !ProductScreenProfileActive
                || bossRiskAtFirstFrame < 0.58f
                || bossRiskAtFireFrame < 0.86f
                || bossRiskAtImpactFrame < 0.88f
                || pressurePositionController.MovedTransform.localScale
                    != savedBossLocalScale
                || combatHud.alpha != 1f
                || !combatHud.interactable
                || !combatHud.blocksRaycasts
                || !IsExactHudRenderable
                || !IsHudResourceStateExact
                || !fixedDeltaTimeExact
                || Mathf.Abs(
                    Time.fixedDeltaTime
                        - 1f / AuditionPvCaptureContract.Fps) > 0.000001f
                || summonUsedEventCount != 1
                || summonBlockedEventCount != 1
                || screenInterceptEventCount != 1
                || !UniqueSummonPressureScreenObserved
                || playerSummon.LastSpentTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || playerSummon.TotalUseCount - initialSummonUseCount != 1
                || playerSummon.TotalPressureScreenInterceptCount
                        - initialSummonInterceptCount != 1
                || retainedProjectileCountBeforeIntercept != 6
                || !retainedProjectileImpactApplied
                || interceptedCrushNetProjectile == null
                || interceptedCrushNetProjectile.IsActive
                || activeCounterProjectileCountAfterIntercept != 1
                || counterProjectile == null
                || counterProjectile.IsActive
                || counterProjectile.LastImpactResult
                    != ProjectileImpactResult.AppliedDamage
                || counterProjectile.LastImpactTargetHealth != bossHealth
                || bossCounterDamageEventCount != 1
                || counterProjectileDamageAppliedCount != 1
                || bossCounterDamageFrame != counterProjectileDamageAppliedFrame
                || bossCounterDamageFrame
                    < AuditionPvStationPhase2SummonCounterCapture
                        .CounterHitFirstFrame
                || bossCounterDamageFrame
                    > AuditionPvStationPhase2SummonCounterCapture
                        .CounterHitLastFrame
                || Mathf.Abs(bossCounterDamageAmount - authoredCounterDamage)
                    > HealthTolerance
                || Mathf.Abs(bossCounterHealthDelta - authoredCounterDamage)
                    > HealthTolerance
                || flow.CurrentPhase
                    != OlympusStationAkazaPhase2FlowController.Phase.Phase2
                || flow.TransitionCompletionCount != 1)
            {
                throw new InvalidOperationException(
                    "G06 final validation failed: frame interval, real dodge, "
                    + "authored Slot1 intercept/counter, HP, HUD, or Phase 2 state drifted: "
                    + $"frame={currentFrame}, dodge={perfectDodgeCount}, "
                    + $"impact={impactAppliedOrBlocked}, active={(impactProjectile != null && impactProjectile.IsActive)}, "
                    + $"hpUnchanged={PlayerHealthUnchanged}, cameraDelta={actionCamera.MicroShakeRequestCount - initialCameraMicroShakeCount}, "
                    + $"cameraCue={sawCameraCue}, screenCue={sawScreenCue}, "
                    + $"screenAtBL06={screenCueActiveAtBaselineFrame}, "
                    + $"screenEnabled={actionScreen.PlayPerfectDodgeScreenDomain}, "
                    + $"bossRisk={bossRiskAtFirstFrame:F3}/{bossRiskAtFireFrame:F3}/{bossRiskAtImpactFrame:F3}, "
                    + $"summonUsed={summonUsedEventCount}, blocked={summonBlockedEventCount}, "
                    + $"screen={screenInterceptEventCount}@{screenFirstObservedFrame}, "
                    + $"retained={retainedProjectileCountBeforeIntercept}/{retainedProjectileImpactApplied}, "
                    + $"counterActiveAfter={activeCounterProjectileCountAfterIntercept}, "
                    + $"counterDamage={bossCounterDamageEventCount}/{counterProjectileDamageAppliedCount} "
                    + $"@{bossCounterDamageFrame}/{counterProjectileDamageAppliedFrame} "
                    + $"amount={bossCounterDamageAmount:F3}, delta={bossCounterHealthDelta:F3}, "
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
                    "G06 canonical damage authority is not ready before the shot: "
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

        private void HandleSummonSlot1Used(int tier)
        {
            if (currentFrame
                    != AuditionPvStationPhase2SummonCounterCapture.QueueSummonFrame
                || tier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier)
            {
                throw new InvalidOperationException(
                    $"G06 observed an out-of-contract Slot1 use at f{currentFrame}, tier {tier}.");
            }

            summonUsedEventCount++;
        }

        private void HandleSummonPressureBlocked(int tier)
        {
            if (currentFrame
                    != AuditionPvStationPhase2SummonCounterCapture
                        .RetainedProjectileInterceptFrame
                || tier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier)
            {
                throw new InvalidOperationException(
                    $"G06 observed an out-of-contract Slot1 pressure block at f{currentFrame}, tier {tier}.");
            }

            summonBlockedEventCount++;
        }

        private void HandlePressureScreenIntercepted(
            SummonPressureScreen screen,
            BossBarrageProjectile projectile)
        {
            if (currentFrame
                    != AuditionPvStationPhase2SummonCounterCapture
                        .RetainedProjectileInterceptFrame
                || screen != pressureScreen
                || projectile != interceptedCrushNetProjectile)
            {
                throw new InvalidOperationException(
                    "G06 observed a pressure-screen intercept outside the selected real f250 CrushNet projectile.");
            }

            screenInterceptEventCount++;
        }

        private void HandleBossDamaged(DamageInfo damageInfo)
        {
            bossDamageEventCount++;
            float resolvedDelta = Mathf.Max(
                0f,
                lastObservedBossHealth - bossHealth.CurrentHealth);
            lastObservedBossHealth = bossHealth.CurrentHealth;
            if (damageInfo.SourceTeam == DamageTeam.AllySummon)
            {
                bossAllyDamageEventCount++;
            }

            if (damageInfo.Source == playerHealth
                && damageInfo.SourceTeam == DamageTeam.AllySummon
                && Mathf.Abs(damageInfo.Amount - authoredCounterDamage)
                    <= HealthTolerance)
            {
                bossCounterDamageEventCount++;
                bossCounterDamageFrame = currentFrame;
                bossCounterDamageAmount = damageInfo.Amount;
                bossCounterHealthDelta = resolvedDelta;
            }
        }

        private void HandleCounterProjectileDamageApplied(
            LaneActionProjectile projectile,
            CombatHealth targetHealth,
            Vector3 impactPoint,
            Vector3 impactDirection)
        {
            if (projectile != counterProjectile
                || targetHealth != bossHealth
                || projectile.SourceHealth != playerHealth
                || projectile.SourceTeam != DamageTeam.AllySummon
                || Mathf.Abs(projectile.Damage - authoredCounterDamage)
                    > HealthTolerance
                || currentFrame
                    < AuditionPvStationPhase2SummonCounterCapture
                        .CounterHitFirstFrame
                || currentFrame
                    > AuditionPvStationPhase2SummonCounterCapture
                        .CounterHitLastFrame)
            {
                throw new InvalidOperationException(
                    "G06 automatic counter projectile reported an out-of-contract damage target or frame.");
            }

            counterProjectileDamageAppliedCount++;
            counterProjectileDamageAppliedFrame = currentFrame;
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
