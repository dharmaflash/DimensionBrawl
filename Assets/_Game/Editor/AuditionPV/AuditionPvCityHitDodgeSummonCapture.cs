using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.Editor.CityHeroPocket;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.AuditionPV
{
    [Serializable]
    internal sealed class AuditionPvCityHitDodgeSummonEvent
    {
        public string eventName = string.Empty;
        public int sourceFrame = -1;
        public int selectedLogicalFrame = -1;
        public int unityFrame = -1;
        public string sourceTeam = string.Empty;
        public float amount;
        public float healthBefore = -1f;
        public float healthAfter = -1f;
        public int tier;
        public int projectileInstanceId;
        public string sourceHierarchy = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvCityHitDodgeSummonRuntimeProof
    {
        public bool freshSceneValidated;
        public bool directorCompleted;
        public bool productBindingsExact;
        public bool hudRenderableEverySelectedFrame;
        public bool noLaneSpace;
        public bool existingProductRootsOnly;
        public bool usedNaturalHostileProjectile;
        public bool usedHudDodgePath;
        public bool usedActualPerfectDodgeSemantics;
        public bool usedHudSummonSlot1Path;
        public bool usedTierTwoChargeBruiser;
        public bool usedActualAllySummonDamage;
        public bool perfectDodgePreservedHealth;
        public bool hostileHitReducedHealth;
        public bool summonDamageReducedEnemyHealth;
        public bool selectedBeatOrderExact;
        public bool presentedFramesExact = true;
        public bool selectedMappingExact = true;
        public bool presentationClockExact = true;
        public bool recorderPaddingActiveAtSourceFrameZero;
        public bool recorderAutoStoppedAfterLastFrame;
        public bool stateRestored;
        public bool captureArtifactsReleased;
        public bool presentationClockReleased;
        public bool freshSceneReopened;
        public int deterministicRandomSeed;
        public int lastSourceFrame = -1;
        public int presentedFrameCount;
        public int preHandlePresentedFrameCount;
        public int selectedPresentedFrameCount;
        public int postHandlePresentedFrameCount;
        public int recorderWarmupEndOfFrameCount;
        public float recorderCaptureDeltaTimeAtSourceFrameZero;
        public int hostileProjectileFiredCount;
        public int hostileDamageCount;
        public int hudDodgeRequestCount;
        public int dodgeStartedCount;
        public int perfectDodgeCount;
        public int dodgeEndedCount;
        public int hudSummonSlot1RequestCount;
        public int summonSlot1UsedCount;
        public int allySummonDamageCount;
        public int firstHostileHitFrame = -1;
        public int dodgeRequestFrame = -1;
        public int dodgeStartedFrame = -1;
        public int perfectDodgeFrame = -1;
        public int dodgeEndedFrame = -1;
        public int summonRequestFrame = -1;
        public int summonUsedFrame = -1;
        public int summonDamageFrame = -1;
        public int summonSpentTier;
        public int summonProjectileCount;
        public float playerHealthAtStart = -1f;
        public float playerHealthAfterHostileHit = -1f;
        public float playerHealthAtDodgeRequest = -1f;
        public float playerHealthAtPerfectDodge = -1f;
        public float enemyHealthBeforeSummonDamage = -1f;
        public float enemyHealthAfterSummonDamage = -1f;
        public float summonEnergyBeforeRequest = -1f;
        public float summonEnergyAfterUse = -1f;
        public AuditionPvCityHitDodgeSummonEvent[] events =
            Array.Empty<AuditionPvCityHitDodgeSummonEvent>();
    }

    /// <summary>
    /// Minimal S030 authoring contract. The selected interval is exactly the
    /// middle 360 frames of a 720-frame physical source. All three semantic
    /// beats traverse public City product input/combat paths.
    /// </summary>
    public static class AuditionPvCityHitDodgeSummonCapture
    {
        internal const string SegmentId = "PV_S030";
        internal const string ShotId = "s030";
        internal const string CityScenePath =
            "Assets/_Game/Scenes/CityHeroPocketStage.unity";
        internal const string CaptureScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvCityHitDodgeSummonCapture.cs";
        internal const string CaptureTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHitDodgeSummonCaptureTests.cs";
        internal const string RunnerScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvCityHitDodgeSummonGoldenRunner.cs";
        internal const string RunnerTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHitDodgeSummonGoldenRunnerTests.cs";
        internal const string ChargeBruiserProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset";
        internal const string ChargeBruiserProfileGuid =
            "81a9e63aa866c9f4b8e2988a77a5a2f8";
        internal const string RifleCrossfirePrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Ranged_RifleCrossfire.prefab";
        internal const string RifleCrossfirePrefabGuid =
            "8bfdd1cbcce07134a9ef1cea5c7e8d23";
        internal const string GateEvidenceTestSuite =
            "AuditionPvSixtySecondEvidence";
        internal const string GateCameraId =
            "city-hero-pocket-action-camera";
        internal const string GateGameplayState =
            "city-riflecrossfire-hit-perfect-dodge-summon-tier2";
        internal const string GateTimelineId =
            "s030-product-clock-source-000-719-select-180-539-v1";
        internal const int FirstSourceFrame = 0;
        internal const int LastSourceFrame = 719;
        internal const int SourceFrameCount = 720;
        internal const int SelectedFirstSourceFrame = 180;
        internal const int SelectedLastSourceFrame = 539;
        internal const int SelectedFrameCount = 360;
        internal const int PreHandleFrameCount = 180;
        internal const int PostHandleFrameCount = 180;
        internal const int ReleaseHostileFrame = SelectedFirstSourceFrame;
        internal const int DeterministicRandomSeed = 0x5030;
        internal const float RequiredTierTwoEnergy = 200f;
        internal const float HostileStartDistance = 4.5f;
        internal const float DodgeRequestProjectileDistance = 2.4f;
        internal const float HealthTolerance = 0.001f;
        internal const string PlayerHitBaselineFileName =
            "BL_S030_CITY_PLAYER_HIT__HUDON.png";
        internal const string PerfectDodgeBaselineFileName =
            "BL_S030_CITY_PERFECT_DODGE__HUDON.png";
        internal const string SummonChainBaselineFileName =
            "BL_S030_CITY_SUMMON_CHAIN__HUDON.png";

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
                return "pre-handle";
            }

            return sourceFrame <= SelectedLastSourceFrame
                ? "selected"
                : "post-handle";
        }

        internal static string FrameFileName(int sourceFrame)
        {
            ValidateSourceFrame(sourceFrame);
            return $"frame_{sourceFrame:0000}.png";
        }

        internal static AuditionPvShotManifestEntry CreateShotManifestEntry()
        {
            return new AuditionPvShotManifestEntry
            {
                id = ShotId,
                scenePath = CityScenePath,
                startFrame = FirstSourceFrame,
                endFrame = LastSourceFrame,
                expectedFrameCount = SourceFrameCount,
                hudMode = "hud-on",
                notes =
                    "PV_S030 source f0..f719; select f180..f539 with exact "
                    + "180-frame pre/post handles. Fresh City product scene; "
                    + "natural RifleCrossfire hostile hit -> HUD RequestDodge "
                    + "actual PerfectDodgeTriggered -> HUD RequestSummonSlot1 "
                    + "tier-2 ChargeBruiser ally-summon damage."
            };
        }

        internal static AuditionPvBaselineManifestEntry[]
            CreateBaselineManifestEntries(
                AuditionPvCityHitDodgeSummonRuntimeProof proof)
        {
            ValidateRuntimeProof(proof);
            return new[]
            {
                Baseline(
                    "s030-player-hit",
                    BaselineFrameAfter(proof.firstHostileHitFrame),
                    PlayerHitBaselineFileName),
                Baseline(
                    "s030-perfect-dodge",
                    BaselineFrameAfter(proof.perfectDodgeFrame),
                    PerfectDodgeBaselineFileName),
                Baseline(
                    "s030-summon-chain",
                    BaselineFrameAfter(proof.summonDamageFrame),
                    SummonChainBaselineFileName)
            };
        }

        internal static string[] ExplicitProductDependencyPaths()
        {
            var paths = new HashSet<string>(
                AuditionPvCityHeroPocketCapture.ExplicitProductDependencyPaths(),
                StringComparer.OrdinalIgnoreCase)
            {
                CaptureScriptPath,
                RunnerScriptPath,
                CaptureTestPath,
                RunnerTestPath,
                "Assets/_Game/Editor/AuditionPV/AuditionPvSixtySecondGateManifest.cs"
            };
            return paths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        }

        internal static string[] GateSemanticBeatIds()
        {
            return new[]
            {
                "player-hit",
                "perfect-dodge",
                "summon-chain"
            };
        }

        internal static AuditionPvCityHitDodgeSummonDirector
            AttachToFreshActiveScene()
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "S030 product-state capture can only attach in Play Mode.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(scene.path, CityScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S030 requires a freshly opened CityHeroPocketStage scene.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<
                    AuditionPvCityHitDodgeSummonDirector>(
                    FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException(
                    "The City scene already owns an S030 director.");
            }

            var root = new GameObject("[AuditionPV_S030_ProductStateDirector]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            AuditionPvCityHitDodgeSummonDirector director =
                root.AddComponent<AuditionPvCityHitDodgeSummonDirector>();
            director.PrepareFreshProductState();
            return director;
        }

        internal static void ReopenProductSceneAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before reopening the S030 City scene.");
            }

            EditorSceneManager.OpenScene(CityScenePath, OpenSceneMode.Single);
        }

        internal static void ValidateRuntimeProof(
            AuditionPvCityHitDodgeSummonRuntimeProof proof)
        {
            bool FramesAreSelected(params int[] frames) =>
                frames.All(frame => frame >= SelectedFirstSourceFrame
                    && frame <= SelectedLastSourceFrame);

            if (proof == null
                || !proof.freshSceneValidated
                || !proof.directorCompleted
                || !proof.productBindingsExact
                || !proof.hudRenderableEverySelectedFrame
                || !proof.noLaneSpace
                || !proof.existingProductRootsOnly
                || !proof.usedNaturalHostileProjectile
                || !proof.usedHudDodgePath
                || !proof.usedActualPerfectDodgeSemantics
                || !proof.usedHudSummonSlot1Path
                || !proof.usedTierTwoChargeBruiser
                || !proof.usedActualAllySummonDamage
                || !proof.perfectDodgePreservedHealth
                || !proof.hostileHitReducedHealth
                || !proof.summonDamageReducedEnemyHealth
                || !proof.selectedBeatOrderExact
                || !proof.presentedFramesExact
                || !proof.selectedMappingExact
                || !proof.presentationClockExact
                || !proof.recorderPaddingActiveAtSourceFrameZero
                || !proof.recorderAutoStoppedAfterLastFrame
                || !proof.stateRestored
                || !proof.captureArtifactsReleased
                || !proof.presentationClockReleased
                || !proof.freshSceneReopened
                || proof.deterministicRandomSeed != DeterministicRandomSeed
                || proof.lastSourceFrame != LastSourceFrame
                || proof.presentedFrameCount != SourceFrameCount
                || proof.preHandlePresentedFrameCount != PreHandleFrameCount
                || proof.selectedPresentedFrameCount != SelectedFrameCount
                || proof.postHandlePresentedFrameCount != PostHandleFrameCount
                || proof.recorderWarmupEndOfFrameCount != 2
                || proof.hostileProjectileFiredCount < 2
                || proof.hostileDamageCount != 1
                || proof.hudDodgeRequestCount != 1
                || proof.dodgeStartedCount != 1
                || proof.perfectDodgeCount != 1
                || proof.dodgeEndedCount != 1
                || proof.hudSummonSlot1RequestCount != 1
                || proof.summonSlot1UsedCount != 1
                || proof.allySummonDamageCount < 1
                || proof.summonSpentTier != 2
                || proof.summonProjectileCount != 2
                || !FramesAreSelected(
                    proof.firstHostileHitFrame,
                    proof.dodgeRequestFrame,
                    proof.dodgeStartedFrame,
                    proof.perfectDodgeFrame,
                    proof.dodgeEndedFrame,
                    proof.summonRequestFrame,
                    proof.summonUsedFrame,
                    proof.summonDamageFrame)
                || !(proof.firstHostileHitFrame < proof.dodgeRequestFrame
                    && proof.dodgeRequestFrame <= proof.dodgeStartedFrame
                    && proof.dodgeStartedFrame <= proof.perfectDodgeFrame
                    && proof.perfectDodgeFrame < proof.dodgeEndedFrame
                    && proof.dodgeEndedFrame <= proof.summonRequestFrame
                    && proof.summonRequestFrame <= proof.summonUsedFrame
                    && proof.summonUsedFrame < proof.summonDamageFrame)
                || proof.playerHealthAtStart <= 0f
                || proof.playerHealthAfterHostileHit >= proof.playerHealthAtStart
                || Math.Abs(
                    proof.playerHealthAtPerfectDodge
                    - proof.playerHealthAtDodgeRequest) > HealthTolerance
                || Math.Abs(
                    proof.summonEnergyBeforeRequest
                    - RequiredTierTwoEnergy) > HealthTolerance
                || Math.Abs(proof.summonEnergyAfterUse) > HealthTolerance
                || proof.enemyHealthAfterSummonDamage
                    >= proof.enemyHealthBeforeSummonDamage
                || proof.events == null
                || proof.events.Length < 8)
            {
                throw new InvalidOperationException(
                    "S030 runtime proof does not satisfy the exact hostile-hit, "
                    + "HUD perfect-dodge, HUD tier-2 summon-damage, handle, and "
                    + "fresh-scene lifecycle contract.");
            }
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
                hudMode = "hud-on",
                status = "captured"
            };
        }

        private static int BaselineFrameAfter(int eventFrame)
        {
            if (eventFrame < SelectedFirstSourceFrame
                || eventFrame > SelectedLastSourceFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(eventFrame));
            }

            return Mathf.Min(SelectedLastSourceFrame, eventFrame + 1);
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
    /// Capture-only coordinator. It stages positions and energy, but the hit,
    /// dodge, perfect-dodge, summon use, and summon damage remain product events.
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public sealed class AuditionPvCityHitDodgeSummonDirector : MonoBehaviour
    {
        private readonly List<AuditionPvCityHitDodgeSummonEvent> eventLedger =
            new(16);

        private Scene scene;
        private GameObject playerRoot;
        private GameObject enemyRoot;
        private GameObject hudRoot;
        private CombatHealth playerHealth;
        private CombatHealth enemyHealth;
        private PlayerActionController playerAction;
        private SummonEnergyLadder energy;
        private PlayerSummonSlot1Action summon;
        private BasicSoldierEnemy soldier;
        private BasicSoldierProjectileAttackDriver projectileDriver;
        private CombatHudInputBridge hudInput;
        private Canvas hudCanvas;
        private PresentationClock.ManualLease presentationClockLease;

        private UnityEngine.Random.State savedRandomState;
        private bool savedRandomStateValid;
        private Vector3 savedPlayerPosition;
        private Quaternion savedPlayerRotation;
        private Vector3 savedEnemyPosition;
        private Quaternion savedEnemyRotation;
        private bool savedSoldierSuspended;
        private float savedEnergyMana;
        private bool savedEnergyGainEnabled;
        private float savedFixedDeltaTime;
        private int savedCaptureFramerate;
        private int savedTargetFrameRate;
        private int initialProductRootCount;
        private float initialPlayerHealth;
        private float initialEnemyHealth;
        private bool restorableStateCaptured;
        private bool eventsSubscribed;
        private bool restoring;
        private bool stateRestored;
        private bool captureArtifactsReleased;
        private int currentSourceFrame = -1;
        private int hostileDamageCount;
        private int hudDodgeRequestCount;
        private int dodgeStartedCount;
        private int perfectDodgeCount;
        private int dodgeEndedCount;
        private int hudSummonRequestCount;
        private int summonUsedCount;
        private int allySummonDamageCount;
        private int firstHostileHitFrame = -1;
        private int dodgeRequestFrame = -1;
        private int dodgeStartedFrame = -1;
        private int perfectDodgeFrame = -1;
        private int dodgeEndedFrame = -1;
        private int summonRequestFrame = -1;
        private int summonUsedFrame = -1;
        private int summonDamageFrame = -1;
        private int summonSpentTier;
        private int summonProjectileCount;
        private int dodgeCandidateProjectileInstanceId;
        private float playerHealthAfterHostileHit = -1f;
        private float playerHealthAtDodgeRequest = -1f;
        private float playerHealthAtPerfectDodge = -1f;
        private float enemyHealthBeforeSummonDamage = -1f;
        private float enemyHealthAfterSummonDamage = -1f;
        private float summonEnergyBeforeRequest = -1f;
        private float summonEnergyAfterUse = -1f;
        private bool hudRenderableEverySelectedFrame = true;
        private bool existingProductRootsOnly = true;
        private bool productBindingsExact;
        private bool noLaneSpace;

        public event Action<int> FramePresented;

        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public Exception Failure { get; private set; }
        public int CurrentSourceFrame => currentSourceFrame;
        public bool StateRestored => stateRestored;
        public bool CaptureArtifactsReleased => captureArtifactsReleased;

        public void PrepareFreshProductState()
        {
            if (IsPrepared || IsRunning || restorableStateCaptured)
            {
                throw new InvalidOperationException(
                    "S030 fresh product state can only be prepared once.");
            }

            ValidateFreshScene();
            ResolveBindings();
            CaptureRestorableState();
            bool success = false;
            try
            {
                if (Time.timeScale <= 0f)
                {
                    throw new InvalidOperationException(
                        "S030 cannot use a Time.timeScale freeze.");
                }

                savedRandomState = UnityEngine.Random.state;
                savedRandomStateValid = true;
                UnityEngine.Random.InitState(
                    AuditionPvCityHitDodgeSummonCapture.DeterministicRandomSeed);
                Time.fixedDeltaTime =
                    1f / AuditionPvCaptureContract.Fps;
                Time.captureFramerate = AuditionPvCaptureContract.Fps;
                Application.targetFrameRate = AuditionPvCaptureContract.Fps;

                soldier.SetGameplaySuspended(true);
                StageHostileAtReviewedDistance();
                energy.SetGainEnabled(false);
                energy.ResetLadder();
                energy.GrantCurrentTierEnergy(
                    AuditionPvCityHitDodgeSummonCapture.RequiredTierTwoEnergy);
                if (Mathf.Abs(
                        energy.CurrentMana
                        - AuditionPvCityHitDodgeSummonCapture.RequiredTierTwoEnergy)
                    > AuditionPvCityHitDodgeSummonCapture.HealthTolerance
                    || energy.AvailableTier != 2
                    || energy.ResolveTierForManaCost(summon.RequiredSummonMana) != 2)
                {
                    throw new InvalidOperationException(
                        "S030 could not stage exact tier-2 summon energy.");
                }

                SubscribeEvents();
                IsPrepared = true;
                success = true;
            }
            finally
            {
                if (!success)
                {
                    RestoreShotState();
                }
            }
        }

        public void BeginShotForRecorder()
        {
            if (!IsPrepared || IsRunning || IsComplete || stateRestored)
            {
                throw new InvalidOperationException(
                    "Prepare S030 exactly once before Recorder begins.");
            }

            float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
            if (Time.captureDeltaTime < minimumDelta
                || Time.captureDeltaTime >= minimumDelta + 0.001f)
            {
                throw new InvalidOperationException(
                    "S030 Recorder padding cadence is not active at source f0.");
            }

            presentationClockLease = PresentationClock.AcquireManual(
                this,
                AuditionPvCaptureContract.Fps);
            currentSourceFrame =
                AuditionPvCityHitDodgeSummonCapture.FirstSourceFrame;
            presentationClockLease.SetFrame(currentSourceFrame);
            initialPlayerHealth = playerHealth.CurrentHealth;
            initialEnemyHealth = enemyHealth.CurrentHealth;
            IsRunning = true;
        }

        internal void PopulateRuntimeProof(
            AuditionPvCityHitDodgeSummonRuntimeProof proof)
        {
            if (proof == null)
            {
                throw new ArgumentNullException(nameof(proof));
            }

            proof.freshSceneValidated = IsPrepared;
            proof.directorCompleted = IsComplete;
            proof.productBindingsExact = productBindingsExact;
            proof.hudRenderableEverySelectedFrame = hudRenderableEverySelectedFrame;
            proof.noLaneSpace = noLaneSpace;
            proof.existingProductRootsOnly = existingProductRootsOnly;
            proof.usedNaturalHostileProjectile =
                projectileDriver != null
                && projectileDriver.FiredCount >= 2
                && firstHostileHitFrame >= 0;
            proof.usedHudDodgePath = hudDodgeRequestCount == 1;
            proof.usedActualPerfectDodgeSemantics = perfectDodgeCount == 1;
            proof.usedHudSummonSlot1Path = hudSummonRequestCount == 1;
            proof.usedTierTwoChargeBruiser = summonSpentTier == 2
                && summonProjectileCount == 2;
            proof.usedActualAllySummonDamage = allySummonDamageCount > 0;
            proof.perfectDodgePreservedHealth =
                Mathf.Abs(
                    playerHealthAtPerfectDodge - playerHealthAtDodgeRequest)
                <= AuditionPvCityHitDodgeSummonCapture.HealthTolerance;
            proof.hostileHitReducedHealth =
                playerHealthAfterHostileHit < initialPlayerHealth;
            proof.summonDamageReducedEnemyHealth =
                enemyHealthAfterSummonDamage < enemyHealthBeforeSummonDamage;
            proof.selectedBeatOrderExact = firstHostileHitFrame >= 0
                && firstHostileHitFrame < dodgeRequestFrame
                && dodgeRequestFrame <= dodgeStartedFrame
                && dodgeStartedFrame <= perfectDodgeFrame
                && perfectDodgeFrame < dodgeEndedFrame
                && dodgeEndedFrame <= summonRequestFrame
                && summonRequestFrame <= summonUsedFrame
                && summonUsedFrame < summonDamageFrame;
            proof.deterministicRandomSeed =
                AuditionPvCityHitDodgeSummonCapture.DeterministicRandomSeed;
            proof.lastSourceFrame = currentSourceFrame;
            proof.hostileProjectileFiredCount =
                projectileDriver != null ? projectileDriver.FiredCount : 0;
            proof.hostileDamageCount = hostileDamageCount;
            proof.hudDodgeRequestCount = hudDodgeRequestCount;
            proof.dodgeStartedCount = dodgeStartedCount;
            proof.perfectDodgeCount = perfectDodgeCount;
            proof.dodgeEndedCount = dodgeEndedCount;
            proof.hudSummonSlot1RequestCount = hudSummonRequestCount;
            proof.summonSlot1UsedCount = summonUsedCount;
            proof.allySummonDamageCount = allySummonDamageCount;
            proof.firstHostileHitFrame = firstHostileHitFrame;
            proof.dodgeRequestFrame = dodgeRequestFrame;
            proof.dodgeStartedFrame = dodgeStartedFrame;
            proof.perfectDodgeFrame = perfectDodgeFrame;
            proof.dodgeEndedFrame = dodgeEndedFrame;
            proof.summonRequestFrame = summonRequestFrame;
            proof.summonUsedFrame = summonUsedFrame;
            proof.summonDamageFrame = summonDamageFrame;
            proof.summonSpentTier = summonSpentTier;
            proof.summonProjectileCount = summonProjectileCount;
            proof.playerHealthAtStart = initialPlayerHealth;
            proof.playerHealthAfterHostileHit = playerHealthAfterHostileHit;
            proof.playerHealthAtDodgeRequest = playerHealthAtDodgeRequest;
            proof.playerHealthAtPerfectDodge = playerHealthAtPerfectDodge;
            proof.enemyHealthBeforeSummonDamage =
                enemyHealthBeforeSummonDamage;
            proof.enemyHealthAfterSummonDamage = enemyHealthAfterSummonDamage;
            proof.summonEnergyBeforeRequest = summonEnergyBeforeRequest;
            proof.summonEnergyAfterUse = summonEnergyAfterUse;
            proof.stateRestored = stateRestored;
            proof.captureArtifactsReleased = captureArtifactsReleased;
            proof.presentationClockReleased =
                !PresentationClock.IsManuallyDriven;
            proof.events = eventLedger.ToArray();
        }

        public void RestoreShotState()
        {
            if (stateRestored || restoring)
            {
                return;
            }

            restoring = true;
            IsRunning = false;
            Exception firstFailure = null;
            try
            {
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    presentationClockLease?.Dispose();
                    presentationClockLease = null;
                });
                CaptureRestoreFailure(ref firstFailure, UnsubscribeEvents);
                CaptureRestoreFailure(ref firstFailure, () =>
                    soldier?.SetGameplaySuspended(true));
                CaptureRestoreFailure(ref firstFailure, ReleaseSummonArtifacts);
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    summon?.ClearSlotCooldown();
                    summon?.DismissActivePressureScreens();
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (playerHealth != null)
                    {
                        playerHealth.ResetHealthToFull();
                    }

                    if (enemyHealth != null)
                    {
                        enemyHealth.ResetHealthToFull();
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (playerRoot != null)
                    {
                        playerRoot.transform.SetPositionAndRotation(
                            savedPlayerPosition,
                            savedPlayerRotation);
                    }

                    if (enemyRoot != null)
                    {
                        enemyRoot.transform.SetPositionAndRotation(
                            savedEnemyPosition,
                            savedEnemyRotation);
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                {
                    if (energy != null)
                    {
                        energy.SetGainEnabled(false);
                        energy.ResetLadder();
                        energy.GrantCurrentTierEnergy(savedEnergyMana);
                        energy.SetGainEnabled(savedEnergyGainEnabled);
                    }
                });
                CaptureRestoreFailure(ref firstFailure, () =>
                    soldier?.SetGameplaySuspended(savedSoldierSuspended));
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
                    Time.fixedDeltaTime = savedFixedDeltaTime;
                    Time.captureFramerate = savedCaptureFramerate;
                    Application.targetFrameRate = savedTargetFrameRate;
                });
                captureArtifactsReleased = AreCaptureArtifactsReleased();
                stateRestored = restorableStateCaptured
                    && captureArtifactsReleased
                    && !PresentationClock.IsManuallyDriven;
                restoring = false;
            }

            if (firstFailure != null)
            {
                throw new InvalidOperationException(
                    "S030 state restoration encountered an error.",
                    firstFailure);
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
                if (currentSourceFrame
                    == AuditionPvCityHitDodgeSummonCapture.ReleaseHostileFrame)
                {
                    soldier.SetGameplaySuspended(false);
                    AddEvent("hostile-release");
                }

                TryRequestPerfectDodgeThroughHud();
                TryRequestTierTwoSummonThroughHud();
                if (currentSourceFrame
                        == AuditionPvCityHitDodgeSummonCapture
                            .SelectedLastSourceFrame + 1
                    && summonDamageFrame < 0)
                {
                    throw new InvalidOperationException(
                        "S030 semantic chain did not finish inside selected f180..f539.");
                }
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
                if (currentSourceFrame >=
                        AuditionPvCityHitDodgeSummonCapture.SelectedFirstSourceFrame
                    && currentSourceFrame <=
                        AuditionPvCityHitDodgeSummonCapture.SelectedLastSourceFrame)
                {
                    hudRenderableEverySelectedFrame &= hudRoot != null
                        && hudRoot.activeInHierarchy
                        && hudCanvas != null
                        && hudCanvas.enabled;
                }

                existingProductRootsOnly &= CountProductRoots() == initialProductRootCount;
                if (currentSourceFrame
                    == AuditionPvCityHitDodgeSummonCapture.LastSourceFrame)
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

        private void ValidateFreshScene()
        {
            scene = gameObject.scene;
            if (!Application.isPlaying
                || !scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.path,
                    AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S030 director is not in the fresh City product scene.");
            }

            if (!string.Equals(
                    AssetDatabase.AssetPathToGUID(
                        AuditionPvCityHitDodgeSummonCapture
                            .ChargeBruiserProfilePath),
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfileGuid,
                    StringComparison.Ordinal)
                || !string.Equals(
                    AssetDatabase.AssetPathToGUID(
                        AuditionPvCityHitDodgeSummonCapture
                            .RifleCrossfirePrefabPath),
                    AuditionPvCityHitDodgeSummonCapture
                        .RifleCrossfirePrefabGuid,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S030 reviewed ChargeBruiser/RifleCrossfire GUID changed.");
            }
        }

        private void ResolveBindings()
        {
            playerRoot = RequireRoot(CityHeroPocketSceneSetup.PlayerRootName);
            enemyRoot = RequireRoot(CityHeroPocketSceneSetup.EnemyRootName);
            hudRoot = RequireRoot(CityHeroPocketSceneSetup.HudRootName);
            playerHealth = RequireSingle<CombatHealth>(playerRoot);
            enemyHealth = RequireSingle<CombatHealth>(enemyRoot);
            playerAction = RequireSingle<PlayerActionController>(playerRoot);
            energy = RequireSingle<SummonEnergyLadder>(playerRoot);
            summon = RequireSingle<PlayerSummonSlot1Action>(playerRoot);
            soldier = RequireSingle<BasicSoldierEnemy>(enemyRoot);
            projectileDriver =
                RequireSingle<BasicSoldierProjectileAttackDriver>(enemyRoot);
            hudInput = RequireSingle<CombatHudInputBridge>(hudRoot);
            hudCanvas = hudRoot.GetComponentInParent<Canvas>(includeInactive: true)
                ?? RequireSingle<Canvas>(hudRoot);

            noLaneSpace = UnityEngine.Object.FindObjectsByType<SummonLaneSpace>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).All(value => value.gameObject.scene != scene);
            productBindingsExact = playerHealth.Team == DamageTeam.Player
                && enemyHealth.Team == DamageTeam.Enemy
                && summon.SummonActionProfile != null
                && string.Equals(
                    AssetDatabase.GetAssetPath(summon.SummonActionProfile),
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfilePath,
                    StringComparison.Ordinal)
                && Mathf.Abs(
                    summon.RequiredSummonMana
                    - AuditionPvCityHitDodgeSummonCapture.RequiredTierTwoEnergy)
                    <= AuditionPvCityHitDodgeSummonCapture.HealthTolerance
                && !summon.IsCinematicInputLocked
                && projectileDriver.SourceHealth == enemyHealth
                && projectileDriver.ProjectilePrefab != null
                && IsUnavailableAction("Skill1Button")
                && IsUnavailableAction("SummonSlot2Button")
                && IsUnavailableAction("SummonSlot3Button");
            if (!noLaneSpace || !productBindingsExact)
            {
                throw new InvalidOperationException(
                    "S030 City product bindings are not the exact S1-only, no-lane contract.");
            }

            if (!playerHealth.IsAlive
                || !enemyHealth.IsAlive
                || Mathf.Abs(playerHealth.CurrentHealth - playerHealth.MaxHealth)
                    > AuditionPvCityHitDodgeSummonCapture.HealthTolerance
                || Mathf.Abs(enemyHealth.CurrentHealth - enemyHealth.MaxHealth)
                    > AuditionPvCityHitDodgeSummonCapture.HealthTolerance
                || projectileDriver.FiredCount != 0)
            {
                throw new InvalidOperationException(
                    "S030 did not attach before fresh City combat changed.");
            }

            initialProductRootCount = CountProductRoots();
        }

        private void CaptureRestorableState()
        {
            savedPlayerPosition = playerRoot.transform.position;
            savedPlayerRotation = playerRoot.transform.rotation;
            savedEnemyPosition = enemyRoot.transform.position;
            savedEnemyRotation = enemyRoot.transform.rotation;
            savedSoldierSuspended = soldier.IsGameplaySuspended;
            savedEnergyMana = energy.CurrentMana;
            savedEnergyGainEnabled = energy.CurrentEnergyPerSecond > 0.001f;
            savedFixedDeltaTime = Time.fixedDeltaTime;
            savedCaptureFramerate = Time.captureFramerate;
            savedTargetFrameRate = Application.targetFrameRate;
            restorableStateCaptured = true;
        }

        private void StageHostileAtReviewedDistance()
        {
            Vector3 forward = ResolvePlanarDirection(playerRoot.transform.forward);
            Vector3 position = playerRoot.transform.position
                + forward
                    * AuditionPvCityHitDodgeSummonCapture.HostileStartDistance;
            position.y = savedEnemyPosition.y;
            enemyRoot.transform.position = position;
            enemyRoot.transform.rotation = Quaternion.LookRotation(
                ResolvePlanarDirection(
                    playerRoot.transform.position - enemyRoot.transform.position),
                Vector3.up);
        }

        private void TryRequestPerfectDodgeThroughHud()
        {
            if (firstHostileHitFrame < 0
                || dodgeRequestFrame >= 0
                || projectileDriver.FiredCount < 2)
            {
                return;
            }

            LaneActionProjectile projectile = projectileDriver.LastFiredProjectile;
            if (projectile == null || !projectile.IsActive)
            {
                return;
            }

            float distance = Vector3.Distance(
                Vector3.ProjectOnPlane(projectile.transform.position, Vector3.up),
                Vector3.ProjectOnPlane(playerRoot.transform.position, Vector3.up));
            if (distance
                > AuditionPvCityHitDodgeSummonCapture
                    .DodgeRequestProjectileDistance)
            {
                return;
            }

            dodgeRequestFrame = currentSourceFrame;
            dodgeCandidateProjectileInstanceId = projectile.GetInstanceID();
            playerHealthAtDodgeRequest = playerHealth.CurrentHealth;
            AddEvent(
                "hud-dodge-request",
                projectile.SourceTeam,
                projectileInstanceId: dodgeCandidateProjectileInstanceId);
            hudInput.RequestDodge();
        }

        private void TryRequestTierTwoSummonThroughHud()
        {
            if (perfectDodgeFrame < 0
                || dodgeEndedFrame < 0
                || summonRequestFrame >= 0
                || currentSourceFrame <= dodgeEndedFrame)
            {
                return;
            }

            soldier.SetGameplaySuspended(true);
            Vector3 forward = ResolvePlanarDirection(playerRoot.transform.forward);
            Vector3 targetPosition = playerRoot.transform.position + forward * 8f;
            targetPosition.y = savedEnemyPosition.y;
            enemyRoot.transform.position = targetPosition;
            enemyRoot.transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);

            summonEnergyBeforeRequest = energy.CurrentMana;
            if (Mathf.Abs(
                    summonEnergyBeforeRequest
                    - AuditionPvCityHitDodgeSummonCapture.RequiredTierTwoEnergy)
                > AuditionPvCityHitDodgeSummonCapture.HealthTolerance
                || energy.AvailableTier != 2
                || summon.IsSlotOnCooldown)
            {
                throw new InvalidOperationException(
                    "S030 cannot enter the HUD S1 path with exact tier-2 energy.");
            }

            summonRequestFrame = currentSourceFrame;
            AddEvent("hud-summon-slot1-request", DamageTeam.Player, tier: 2);
            hudInput.RequestSummonSlot1();
            summonEnergyAfterUse = energy.CurrentMana;
            if (summonUsedCount != 1 || summonSpentTier != 2)
            {
                throw new InvalidOperationException(
                    "S030 HUD S1 request did not synchronously traverse the product binder.");
            }
        }

        private void SubscribeEvents()
        {
            if (eventsSubscribed)
            {
                return;
            }

            playerHealth.Damaged += HandlePlayerDamaged;
            playerAction.DodgeStarted += HandleDodgeStarted;
            playerAction.PerfectDodgeTriggered += HandlePerfectDodge;
            playerAction.DodgeEnded += HandleDodgeEnded;
            hudInput.ActionRequested += HandleHudActionRequested;
            summon.SummonSlot1Used += HandleSummonUsed;
            enemyHealth.Damaged += HandleEnemyDamaged;
            eventsSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!eventsSubscribed)
            {
                return;
            }

            playerHealth.Damaged -= HandlePlayerDamaged;
            playerAction.DodgeStarted -= HandleDodgeStarted;
            playerAction.PerfectDodgeTriggered -= HandlePerfectDodge;
            playerAction.DodgeEnded -= HandleDodgeEnded;
            hudInput.ActionRequested -= HandleHudActionRequested;
            summon.SummonSlot1Used -= HandleSummonUsed;
            enemyHealth.Damaged -= HandleEnemyDamaged;
            eventsSubscribed = false;
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            if (damageInfo.SourceTeam != DamageTeam.Enemy)
            {
                return;
            }

            hostileDamageCount++;
            if (firstHostileHitFrame < 0)
            {
                firstHostileHitFrame = currentSourceFrame;
                playerHealthAfterHostileHit = playerHealth.CurrentHealth;
            }

            AddEvent(
                "player-hit",
                damageInfo.SourceTeam,
                damageInfo.Amount,
                playerHealth.CurrentHealth + damageInfo.Amount,
                playerHealth.CurrentHealth,
                source: damageInfo.Source);
        }

        private void HandleDodgeStarted()
        {
            dodgeStartedCount++;
            dodgeStartedFrame = currentSourceFrame;
            AddEvent("dodge-started", DamageTeam.Player);
        }

        private void HandlePerfectDodge(DamageInfo damageInfo)
        {
            perfectDodgeCount++;
            perfectDodgeFrame = currentSourceFrame;
            playerHealthAtPerfectDodge = playerHealth.CurrentHealth;
            soldier.SetGameplaySuspended(true);
            AddEvent(
                "perfect-dodge",
                damageInfo.SourceTeam,
                damageInfo.Amount,
                playerHealthAtDodgeRequest,
                playerHealth.CurrentHealth,
                projectileInstanceId: dodgeCandidateProjectileInstanceId,
                source: damageInfo.Source);
        }

        private void HandleDodgeEnded()
        {
            dodgeEndedCount++;
            dodgeEndedFrame = currentSourceFrame;
            AddEvent("dodge-ended", DamageTeam.Player);
        }

        private void HandleHudActionRequested(CombatHudActionId actionId)
        {
            if (actionId == CombatHudActionId.Dodge)
            {
                hudDodgeRequestCount++;
            }
            else if (actionId == CombatHudActionId.SummonSlot1)
            {
                hudSummonRequestCount++;
            }
        }

        private void HandleSummonUsed(int tier)
        {
            summonUsedCount++;
            summonUsedFrame = currentSourceFrame;
            summonSpentTier = tier;
            summonProjectileCount = summon.LastFiredProjectileCount;
            AddEvent(
                "summon-slot1-used",
                DamageTeam.AllySummon,
                tier: tier);
        }

        private void HandleEnemyDamaged(DamageInfo damageInfo)
        {
            if (damageInfo.SourceTeam != DamageTeam.AllySummon)
            {
                return;
            }

            allySummonDamageCount++;
            if (summonDamageFrame < 0)
            {
                summonDamageFrame = currentSourceFrame;
                enemyHealthAfterSummonDamage = enemyHealth.CurrentHealth;
                enemyHealthBeforeSummonDamage = Mathf.Min(
                    enemyHealth.MaxHealth,
                    enemyHealth.CurrentHealth + damageInfo.Amount);
            }

            AddEvent(
                "summon-damage",
                damageInfo.SourceTeam,
                damageInfo.Amount,
                Mathf.Min(
                    enemyHealth.MaxHealth,
                    enemyHealth.CurrentHealth + damageInfo.Amount),
                enemyHealth.CurrentHealth,
                summonSpentTier,
                source: damageInfo.Source);
        }

        private void ValidateCompletedShot()
        {
            bool selectedFrames = new[]
            {
                firstHostileHitFrame,
                dodgeRequestFrame,
                dodgeStartedFrame,
                perfectDodgeFrame,
                dodgeEndedFrame,
                summonRequestFrame,
                summonUsedFrame,
                summonDamageFrame
            }.All(frame => frame >=
                    AuditionPvCityHitDodgeSummonCapture.SelectedFirstSourceFrame
                && frame <=
                    AuditionPvCityHitDodgeSummonCapture.SelectedLastSourceFrame);

            if (!selectedFrames
                || hostileDamageCount != 1
                || hudDodgeRequestCount != 1
                || dodgeStartedCount != 1
                || perfectDodgeCount != 1
                || dodgeEndedCount != 1
                || hudSummonRequestCount != 1
                || summonUsedCount != 1
                || allySummonDamageCount < 1
                || summonSpentTier != 2
                || summonProjectileCount != 2
                || !hudRenderableEverySelectedFrame
                || !existingProductRootsOnly
                || Mathf.Abs(
                    playerHealthAtPerfectDodge - playerHealthAtDodgeRequest)
                    > AuditionPvCityHitDodgeSummonCapture.HealthTolerance
                || enemyHealthAfterSummonDamage
                    >= enemyHealthBeforeSummonDamage)
            {
                throw new InvalidOperationException(
                    "S030 completed without its exact selected semantic chain.");
            }
        }

        private void ReleaseSummonArtifacts()
        {
            LaneActionProjectile[] projectiles =
                UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            foreach (LaneActionProjectile projectile in projectiles)
            {
                if (projectile != null
                    && projectile.gameObject.scene == scene
                    && projectile.SourceTeam == DamageTeam.AllySummon
                    && projectile.IsActive)
                {
                    projectile.Deactivate();
                }
            }

            SummonFrontlineProxy[] actors =
                UnityEngine.Object.FindObjectsByType<SummonFrontlineProxy>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            foreach (SummonFrontlineProxy actor in actors)
            {
                if (actor != null
                    && actor.gameObject.scene == scene
                    && actor.IsActive)
                {
                    actor.Deactivate(SummonFrontlineProxyExitReason.Recalled);
                }
            }
        }

        private bool AreCaptureArtifactsReleased()
        {
            bool projectilesReleased = UnityEngine.Object
                .FindObjectsByType<LaneActionProjectile>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(projectile => projectile != null
                    && projectile.gameObject.scene == scene
                    && projectile.SourceTeam == DamageTeam.AllySummon)
                .All(projectile => !projectile.IsActive);
            bool actorsReleased = UnityEngine.Object
                .FindObjectsByType<SummonFrontlineProxy>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(actor => actor != null && actor.gameObject.scene == scene)
                .All(actor => !actor.IsActive);
            return projectilesReleased
                && actorsReleased
                && summon != null
                && !summon.IsSlotOnCooldown
                && summon.ActiveProjectileCount == 0
                && summon.ActiveSummonActorCount == 0
                && !eventsSubscribed;
        }

        private void AddEvent(
            string eventName,
            DamageTeam sourceTeam = DamageTeam.Neutral,
            float amount = 0f,
            float healthBefore = -1f,
            float healthAfter = -1f,
            int tier = 0,
            int projectileInstanceId = 0,
            CombatHealth source = null)
        {
            eventLedger.Add(new AuditionPvCityHitDodgeSummonEvent
            {
                eventName = eventName,
                sourceFrame = currentSourceFrame,
                selectedLogicalFrame = currentSourceFrame >=
                        AuditionPvCityHitDodgeSummonCapture
                            .SelectedFirstSourceFrame
                    && currentSourceFrame <=
                        AuditionPvCityHitDodgeSummonCapture
                            .SelectedLastSourceFrame
                        ? currentSourceFrame
                            - AuditionPvCityHitDodgeSummonCapture
                                .SelectedFirstSourceFrame
                        : -1,
                unityFrame = Time.frameCount,
                sourceTeam = sourceTeam.ToString(),
                amount = amount,
                healthBefore = healthBefore,
                healthAfter = healthAfter,
                tier = tier,
                projectileInstanceId = projectileInstanceId,
                sourceHierarchy = Hierarchy(source != null ? source.transform : null)
            });
        }

        private bool IsUnavailableAction(string buttonName)
        {
            Transform buttonTransform = FindDescendant(hudRoot.transform, buttonName);
            if (buttonTransform == null)
            {
                return true;
            }

            Button button = buttonTransform.GetComponent<Button>();
            return (button == null || !button.interactable)
                && buttonTransform.GetComponents<CombatHudPointerActionInput>().Length == 0;
        }

        private int CountProductRoots()
        {
            return scene.GetRootGameObjects().Count(root => root != gameObject);
        }

        private GameObject RequireRoot(string name)
        {
            GameObject[] matches = scene.GetRootGameObjects()
                .Where(root => string.Equals(root.name, name, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"S030 requires exactly one root '{name}', found {matches.Length}.");
            }

            return matches[0];
        }

        private static T RequireSingle<T>(GameObject root) where T : Component
        {
            T[] matches = root.GetComponentsInChildren<T>(includeInactive: true);
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"S030 root '{root.name}' requires exactly one {typeof(T).Name}; "
                    + $"found {matches.Length}.");
            }

            return matches[0];
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(child.name, name, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static Vector3 ResolvePlanarDirection(Vector3 value)
        {
            Vector3 planar = Vector3.ProjectOnPlane(value, Vector3.up);
            return planar.sqrMagnitude > 0.0001f
                ? planar.normalized
                : Vector3.forward;
        }

        private static string Hierarchy(Transform value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var names = new Stack<string>();
            Transform cursor = value;
            while (cursor != null)
            {
                names.Push(cursor.name);
                cursor = cursor.parent;
            }

            return string.Join("/", names);
        }

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
                firstFailure ??= exception;
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
            try
            {
                RestoreShotState();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
