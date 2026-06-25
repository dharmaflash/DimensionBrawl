using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsekaiBrawl.Gameplay
{
    public class LaneAnchorSet : MonoBehaviour
    {
        [SerializeField] private int laneIndex;
        [SerializeField] private Transform rearAnchor;
        [SerializeField] private Transform supportCoverAnchor;
        [SerializeField] private Transform peekAnchor;
        [SerializeField] private Transform advanceBaseAnchor;

        public int LaneIndex => laneIndex;
        public Transform RearAnchor => rearAnchor;
        public Transform SupportCoverAnchor => supportCoverAnchor;
        public Transform PeekAnchor => peekAnchor;
        public Transform AdvanceBaseAnchor => advanceBaseAnchor;

        public void Configure(int newLaneIndex, Transform rear, Transform supportCover, Transform peek, Transform advanceBase)
        {
            laneIndex = newLaneIndex;
            rearAnchor = rear;
            supportCoverAnchor = supportCover;
            peekAnchor = peek;
            advanceBaseAnchor = advanceBase;
        }
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public event Action<float> OnPlayerBaseHPChanged;
        public event Action<float> OnEnemyBaseHPChanged;
        public event Action<bool, float, float> OnBaseDamaged;

        [SerializeField] private float playerBaseMaxHP = 1000f;
        [SerializeField] private float enemyBaseMaxHP = 1000f;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawn;
        [SerializeField] private Transform summonSpawnPoint;
        [SerializeField] private Transform enemySummonSpawnPoint;
        [SerializeField] private Transform playerBaseTransform;
        [SerializeField] private Transform enemyBaseTransform;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform laneAnchorRoot;
        [SerializeField] private Transform battlefieldLayoutRoot;
        [SerializeField] private bool autoSpawnStructures = true;
        [SerializeField] private bool allowRuntimePrototypeBootstrap;
        [SerializeField] private bool allowRuntimePresentationBootstrap;
        [SerializeField] private float laneWidth = 12.5f;
        [SerializeField] private float laneLength = 84f;
        [SerializeField] private float playerForwardLimit = 79.5f;
        [SerializeField] private float playerLaneMinZ = 0.6f;
        [SerializeField] private float playerSpawnInset = 4.2f;
        [SerializeField] private float summonSpawnInset = 10.4f;
        [SerializeField] private float laneContestGap = 2.2f;
        [SerializeField] private float lanePushLeadDistance = 1.2f;
        [SerializeField] private float laneCollapseThreatZ = 9.4f;
        [SerializeField] private float laneRearSlotZ = 2.35f;
        [SerializeField] private float laneSupportBaseZ = 4.95f;
        [SerializeField] private float laneSupportRearOffset = 1.3f;
        [SerializeField] private float lanePushRearOffset = 0.9f;
        [SerializeField] private float lanePeekObjectiveClearance = 2.2f;
        [SerializeField] private float laneAdvanceObjectiveClearance = 0.75f;
        [SerializeField] private float supportEnvelopeLateralOffset = 0.88f;
        [SerializeField] private float heroRearMinGap = 1.8f;
        [SerializeField] private float laneAnchorVisualY = 0.05f;
        [SerializeField] private float runtimeFrontlineStructureNormalizedZ = 0.42f;
        [SerializeField] private float runtimeStructureHP = 150f;
        [SerializeField] private float runtimeStructureEnergyReward = 10f;
        [SerializeField] private float baseFlashDuration = 0.12f;
        [SerializeField] private Color playerBaseHitColor = new(0.45f, 0.9f, 1f, 1f);
        [SerializeField] private Color enemyBaseHitColor = new(1f, 0.5f, 0.4f, 1f);
        [SerializeField] private Color laneGuideColor = new(0.36f, 0.55f, 0.88f, 0.28f);
        [SerializeField] private Color centerGuideColor = new(0.4f, 0.95f, 1f, 0.32f);
        [SerializeField] private Color safeAdvanceGuideColor = new(0.48f, 0.9f, 1f, 0.34f);
        [SerializeField] private Color safeAdvanceWarningColor = new(1f, 0.78f, 0.36f, 0.48f);
        [SerializeField] private Color safeAdvanceDangerColor = new(1f, 0.48f, 0.34f, 0.58f);
        [SerializeField] private Color baseZoneGuideColor = new(1f, 0.34f, 0.34f, 0.42f);
        [SerializeField] private float territoryWarningLeadDistance = 1.4f;
        [SerializeField] private float territoryPressureTickInterval = 0.85f;
        [SerializeField] private float territoryPressureBaseDamage = 5f;
        [SerializeField] private float territoryPressureDepthDamage = 0.7f;
        [SerializeField] private float territoryCoverBreakRetreatThreshold = 1.15f;
        [SerializeField] private float territoryCoverBreakGraceDuration = 0.45f;
        [SerializeField] private float territoryPressureRampDuration = 1.8f;
        [SerializeField] private float territoryPressureRampStartMultiplier = 0.42f;
        [SerializeField] private float territoryCoverBreakPlayerMargin = 0.28f;
        [SerializeField] private float territoryBaseZoneStart = 11.5f;
        [SerializeField] private float territoryBaseZoneDamageBonus = 7f;
        [SerializeField] private bool writeLeashDebugReport = true;
        [SerializeField] private float leashDebugReportWriteInterval = 1f;

        private Renderer[] playerBaseRenderers;
        private Renderer[] enemyBaseRenderers;
        private Color[] playerBaseColors;
        private Color[] enemyBaseColors;
        private Coroutine playerBaseFlashRoutine;
        private Coroutine enemyBaseFlashRoutine;
        private Transform laneGuideRoot;
        private float nextTerritoryPressureTime;
        private int territoryStateFrame = -1;
        private PlayerTerritoryState cachedTerritoryState;
        private bool hasCachedTerritoryState;
        private float lastSafeAdvanceZ = float.NaN;
        private float overextendExposureStartTime = -1f;
        private float coverBreakGraceUntilTime;
        private bool lastTerritoryWasOverextended;
        private bool wasOverextended;
        private bool wasInCoverBreakGrace;
        private bool wasInEnemyBaseZone;
        private LaneAnchorSet[] cachedLaneAnchorSets = Array.Empty<LaneAnchorSet>();
        private readonly float[] friendlyLanePrimeUntil = new float[BattleLaneUtility.DefaultLaneCount];
        private readonly float[] laneRallyUntil = new float[BattleLaneUtility.DefaultLaneCount];
        private SummonSpawner summonSpawner;
        private bool subscribedSummonSpawner;
        private bool subscribedStructureEvents;
        private bool centerAdvanceUnlocked;
        private float nextLeashDebugReportWriteTime;
        private static readonly string LeashDebugReportPath = Path.Combine("C:\\tmp", "IsekaiBrawl_BattleLeashReport.html");

        public float CurrentPlayerBaseHP { get; private set; }
        public float CurrentEnemyBaseHP { get; private set; }
        public float PlayerBaseMaxHP => playerBaseMaxHP;
        public float EnemyBaseMaxHP => enemyBaseMaxHP;
        public PlayerController PlayerController => playerController;
        public Transform PlayerSpawn => playerSpawn;
        public Transform EnemySpawn => enemySpawn;
        public Transform SummonSpawnPoint => summonSpawnPoint;
        public Transform EnemySummonSpawnPoint => enemySummonSpawnPoint;
        public float LaneLength => laneLength;
        public float LaneHalfWidth => laneWidth * 0.5f;
        public int LaneCount => BattleLaneUtility.DefaultLaneCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentPlayerBaseHP = playerBaseMaxHP;
            CurrentEnemyBaseHP = enemyBaseMaxHP;
        }

        private void Start()
        {
            ResolveSceneReferences();
            if (allowRuntimePrototypeBootstrap)
            {
                EnsureStoryPveRuntimeBootstrap();
                ApplyRuntimePrototypeTuning();
                ApplyRuntimeBattlefieldLayout();
                if (ShouldUseStoryPveEncounterLayout())
                {
                    EnsurePrototypeBattlefieldPoints();
                }
                else
                {
                    EnsurePrototypeBattlefieldLayout();
                }
            }
            EnsureLaneAnchorSets();
            if (allowRuntimePrototypeBootstrap)
            {
                EnsureLaneGuides();
            }

            LayoutLaneAnchorSets();
            ConfigurePlayerAndCamera();
            if (allowRuntimePresentationBootstrap)
            {
                BattlePresentationController.EnsureExists();
            }

            CacheBaseVisuals();
            HookRuntimeEvents();
            NotifyBaseHPChanged();
        }

        private static bool ShouldUseStoryPveEncounterLayout()
        {
            return BattleModeContext.CurrentMode == BattleMode.StoryPve &&
                   UnityEngine.Object.FindFirstObjectByType<PveEncounterDirector>() != null;
        }

        private void EnsureStoryPveRuntimeBootstrap()
        {
            if (BattleModeContext.CurrentMode != BattleMode.StoryPve)
            {
                return;
            }

            if (PveStageContext.SelectedStage == null)
            {
                PveStageContext.SetStage(PveStageData.CreateRuntimePrototypeStage(
                    summonSpawner != null ? summonSpawner.AvailableSummons : null));
            }

            if (UnityEngine.Object.FindFirstObjectByType<PveEncounterDirector>() != null)
            {
                return;
            }

            GameObject directorObject = new("PveEncounterDirector_Runtime");
            directorObject.AddComponent<PveEncounterDirector>();
        }

        private void OnDisable()
        {
            if (summonSpawner != null && subscribedSummonSpawner)
            {
                summonSpawner.OnSummonSpawned -= HandleSummonSpawned;
                subscribedSummonSpawner = false;
            }

            if (subscribedStructureEvents)
            {
                BattleStructure.OnStructureDestroyed -= HandleStructureDestroyed;
                subscribedStructureEvents = false;
            }
        }

        private void Update()
        {
            HandlePerspectivePresetDebugInput();
            UpdateCachedTerritoryState();
            UpdateTerritoryGuides();
            UpdateTerritoryWarnings();
            UpdatePlayerTerritoryPressure();
            UpdateLeashDebugReport();
        }

        public void RegisterPlayer(PlayerController controller)
        {
            if (controller == null)
            {
                return;
            }

            playerController = controller;
            EnsurePlayerSkillController();
            ConfigurePlayerAndCamera();
        }

        public void DamagePlayerBase(float amount)
        {
            if (amount <= 0f || CurrentPlayerBaseHP <= 0f)
            {
                return;
            }

            CurrentPlayerBaseHP = Mathf.Max(0f, CurrentPlayerBaseHP - amount);
            OnPlayerBaseHPChanged?.Invoke(CurrentPlayerBaseHP);
            OnBaseDamaged?.Invoke(true, amount, CurrentPlayerBaseHP);
            PlayPlayerBaseFeedback();

            if (CurrentPlayerBaseHP <= 0f && GameManager.Instance != null)
            {
                GameManager.Instance.EndBattle(false);
            }
        }

        public void DamageEnemyBase(float amount)
        {
            if (amount <= 0f || CurrentEnemyBaseHP <= 0f)
            {
                return;
            }

            CurrentEnemyBaseHP = Mathf.Max(0f, CurrentEnemyBaseHP - amount);
            OnEnemyBaseHPChanged?.Invoke(CurrentEnemyBaseHP);
            OnBaseDamaged?.Invoke(false, amount, CurrentEnemyBaseHP);
            PlayEnemyBaseFeedback();

            if (CurrentEnemyBaseHP <= 0f && GameManager.Instance != null)
            {
                GameManager.Instance.EndBattle(true);
            }
        }

        public float GetRemainingBattleTime()
        {
            return GameManager.Instance != null ? GameManager.Instance.RemainingTime : 0f;
        }

        public Transform GetOpposingBaseTransform(bool isPlayerTeam)
        {
            return isPlayerTeam ? enemyBaseTransform : playerBaseTransform;
        }

        public Transform GetBaseTransform(bool isPlayerBase)
        {
            return isPlayerBase ? playerBaseTransform : enemyBaseTransform;
        }

        public Transform GetSummonSpawnPoint(bool isPlayerTeam)
        {
            return isPlayerTeam ? summonSpawnPoint : enemySummonSpawnPoint;
        }

        public float[] GetLaneAnchors()
        {
            return BattleLaneUtility.BuildLaneAnchors(LaneHalfWidth);
        }

        public float GetLaneCenterX(int laneIndex)
        {
            return BattleLaneUtility.GetLaneCenterX(laneIndex, LaneHalfWidth);
        }

        public int GetNearestLaneIndex(float worldX)
        {
            return BattleLaneUtility.GetNearestLaneIndex(worldX, GetLaneAnchors());
        }

        public LaneAnchorSet GetLaneAnchorSet(int laneIndex)
        {
            if (cachedLaneAnchorSets == null || cachedLaneAnchorSets.Length == 0)
            {
                EnsureLaneAnchorSets();
                LayoutLaneAnchorSets();
            }

            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            if (resolvedLaneIndex < 0 || resolvedLaneIndex >= cachedLaneAnchorSets.Length)
            {
                return null;
            }

            return cachedLaneAnchorSets[resolvedLaneIndex];
        }

        private Vector3 ResolveLaneAnchorPosition(Transform anchor, int laneIndex, float fallbackZ)
        {
            float laneX = GetLaneCenterX(laneIndex);
            float anchorY = playerSpawn != null ? playerSpawn.position.y : laneAnchorVisualY;
            if (anchor != null)
            {
                return new Vector3(anchor.position.x, anchorY, anchor.position.z);
            }

            return new Vector3(laneX, anchorY, fallbackZ);
        }

        private BattlefieldPoint FindHighestPriorityLanePoint(int laneIndex, BattlefieldPointType primaryType, BattlefieldPointType fallbackType)
        {
            return BattlefieldPoint.FindHighestPriorityInLane(laneIndex, primaryType) ??
                BattlefieldPoint.FindHighestPriorityInLane(laneIndex, fallbackType);
        }

        private BattlefieldPoint FindClosestLanePoint(
            int laneIndex,
            BattlefieldPointType primaryType,
            BattlefieldPointType fallbackType,
            bool isPlayerTeam,
            float referenceZ)
        {
            return BattlefieldPoint.FindClosestInLane(laneIndex, primaryType, isPlayerTeam, referenceZ) ??
                BattlefieldPoint.FindClosestInLane(laneIndex, fallbackType, isPlayerTeam, referenceZ);
        }

        private BattlefieldPoint FindOptionalLanePoint(int laneIndex, BattlefieldPointType pointType)
        {
            return BattlefieldPoint.FindHighestPriorityInLane(laneIndex, pointType);
        }

        private bool TryBuildLeashedLaneCombatState(int laneIndex, out LaneCombatState state)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            LaneRuntimeSnapshot snapshot = CollectLaneRuntimeSnapshot(resolvedLaneIndex);
            bool hasLiveAllies = snapshot.PlayerCount > 0;
            bool hasRecentPrime = IsLanePrimed(resolvedLaneIndex);
            int centerLaneIndex = LaneCount / 2;

            BattlefieldPoint readyPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.ReadyPocket,
                BattlefieldPointType.FallbackPocket);
            BattlefieldPoint fallbackPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.FallbackPocket,
                BattlefieldPointType.ReadyPocket);
            BattlefieldPoint centerReadyPoint = FindHighestPriorityLanePoint(
                centerLaneIndex,
                BattlefieldPointType.ReadyPocket,
                BattlefieldPointType.FallbackPocket);
            BattlefieldPoint joinPoint = FindClosestLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.JoinPocket,
                BattlefieldPointType.ApproachPocket,
                isPlayerTeam: true,
                summonSpawnInset);
            BattlefieldPoint supportLeftPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.SupportLeftPocket,
                BattlefieldPointType.JoinPocket);
            BattlefieldPoint supportCenterPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.SupportCenterPocket,
                BattlefieldPointType.JoinPocket);
            BattlefieldPoint supportRightPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.SupportRightPocket,
                BattlefieldPointType.JoinPocket);
            BattlefieldPoint peekPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.PeekPocket,
                BattlefieldPointType.BlockerHoldPocket);
            BattlefieldPoint blockerHoldPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.BlockerHoldPocket,
                BattlefieldPointType.ObjectiveAnchor);
            BattlefieldPoint breachPoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.BreachPocket,
                BattlefieldPointType.ObjectiveAnchor);
            BattlefieldPoint objectivePoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.ObjectivePocket,
                BattlefieldPointType.ObjectiveAnchor);
            BattlefieldPoint coreSiegePoint = FindHighestPriorityLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.CoreSiegePocket,
                BattlefieldPointType.AdvancePocket);

            HeroLaneDepthBand maxDepthBand = ResolveHeroLaneDepthBand(snapshot, hasLiveAllies, hasRecentPrime);
            EscortPhase escortPhase = ResolveEscortPhase(snapshot, hasLiveAllies, hasRecentPrime, maxDepthBand);

            Vector3 centerReadyAnchor = ResolveLaneAnchorPosition(
                centerReadyPoint != null ? centerReadyPoint.transform : null,
                centerLaneIndex,
                Mathf.Max(playerLaneMinZ + 0.45f, laneRearSlotZ));
            Vector3 joinAnchor = ResolveLaneAnchorPosition(
                joinPoint != null ? joinPoint.transform : readyPoint != null ? readyPoint.transform : centerReadyPoint != null ? centerReadyPoint.transform : null,
                joinPoint != null ? resolvedLaneIndex : centerLaneIndex,
                Mathf.Max(playerLaneMinZ + 0.45f, laneSupportBaseZ));
            Vector3 retreatAnchor = ResolveLaneAnchorPosition(
                fallbackPoint != null ? fallbackPoint.transform : centerReadyPoint != null ? centerReadyPoint.transform : joinPoint != null ? joinPoint.transform : null,
                fallbackPoint != null ? resolvedLaneIndex : centerLaneIndex,
                fallbackPoint != null
                    ? Mathf.Max(playerLaneMinZ + 0.35f, laneRearSlotZ)
                    : centerReadyAnchor.z);

            float supportFallbackZ = Mathf.Max(joinAnchor.z + 3.4f, laneSupportBaseZ + 2.2f);
            Vector3 supportLeftAnchor = ResolveLaneAnchorPosition(
                supportLeftPoint != null ? supportLeftPoint.transform : null,
                resolvedLaneIndex,
                supportFallbackZ);
            Vector3 supportCenterAnchor = ResolveLaneAnchorPosition(
                supportCenterPoint != null ? supportCenterPoint.transform : null,
                resolvedLaneIndex,
                supportFallbackZ + 0.25f);
            Vector3 supportRightAnchor = ResolveLaneAnchorPosition(
                supportRightPoint != null ? supportRightPoint.transform : null,
                resolvedLaneIndex,
                supportFallbackZ);

            if (escortPhase == EscortPhase.Ready)
            {
                Vector3 idleAnchor = BattleModeContext.CurrentMode == BattleMode.StoryPve
                    ? joinAnchor
                    : centerReadyAnchor;
                supportLeftAnchor = idleAnchor;
                supportCenterAnchor = idleAnchor;
                supportRightAnchor = idleAnchor;
            }
            else if (escortPhase == EscortPhase.Join)
            {
                supportLeftAnchor = joinAnchor;
                supportCenterAnchor = joinAnchor;
                supportRightAnchor = joinAnchor;
            }

            float supportForwardCap = playerForwardLimit - 1.35f;
            if (snapshot.HasFrontlineStructure)
            {
                supportForwardCap = Mathf.Min(
                    supportForwardCap,
                    snapshot.FrontlineStructureZ - (lanePeekObjectiveClearance + 0.45f));
            }
            else if (snapshot.RewardStructure != null || snapshot.SiegeStructure != null)
            {
                float objectiveSupportCap = objectivePoint != null
                    ? objectivePoint.transform.position.z - (lanePeekObjectiveClearance + 0.45f)
                    : snapshot.FrontlineObjectiveZ - (lanePeekObjectiveClearance + 0.45f);
                supportForwardCap = Mathf.Min(supportForwardCap, objectiveSupportCap);
            }

            float supportMinZ = Mathf.Max(playerLaneMinZ + 0.6f, retreatAnchor.z + 0.45f);
            supportForwardCap = Mathf.Max(supportMinZ, supportForwardCap);
            supportLeftAnchor.z = Mathf.Clamp(supportLeftAnchor.z, supportMinZ, supportForwardCap);
            supportCenterAnchor.z = Mathf.Clamp(supportCenterAnchor.z, supportMinZ, supportForwardCap);
            supportRightAnchor.z = Mathf.Clamp(supportRightAnchor.z, supportMinZ, supportForwardCap);
            retreatAnchor.z = Mathf.Clamp(
                retreatAnchor.z,
                playerLaneMinZ + 0.35f,
                Mathf.Max(playerLaneMinZ + 0.35f, supportMinZ));

            if (hasLiveAllies)
            {
                float allyRearCap = snapshot.PlayerFrontZ - heroRearMinGap;
                if (float.IsFinite(allyRearCap))
                {
                    float resolvedRearCap = Mathf.Max(retreatAnchor.z + 0.2f, allyRearCap);
                    supportLeftAnchor.z = Mathf.Min(supportLeftAnchor.z, resolvedRearCap);
                    supportCenterAnchor.z = Mathf.Min(supportCenterAnchor.z, resolvedRearCap);
                    supportRightAnchor.z = Mathf.Min(supportRightAnchor.z, resolvedRearCap);
                }
            }

            float supportEnvelopeMinZ = Mathf.Min(supportLeftAnchor.z, Mathf.Min(supportCenterAnchor.z, supportRightAnchor.z));
            float supportEnvelopeMaxZ = Mathf.Max(supportLeftAnchor.z, Mathf.Max(supportCenterAnchor.z, supportRightAnchor.z));
            Vector3 supportAnchor = supportCenterAnchor;

            float peekTargetZ = snapshot.HasFrontlineStructure
                ? peekPoint != null
                    ? peekPoint.transform.position.z
                    : blockerHoldPoint != null
                    ? blockerHoldPoint.transform.position.z
                    : snapshot.FrontlineStructureZ - lanePeekObjectiveClearance
                : breachPoint != null
                    ? breachPoint.transform.position.z
                    : objectivePoint != null
                        ? objectivePoint.transform.position.z - lanePeekObjectiveClearance
                    : snapshot.HasFrontlineObjective
                        ? snapshot.FrontlineObjectiveZ - lanePeekObjectiveClearance
                        : supportAnchor.z + 0.55f;
            if (snapshot.PlayerCount > 0 && snapshot.EnemyCount > 0)
            {
                peekTargetZ = Mathf.Max(peekTargetZ, snapshot.ClashZ - lanePeekObjectiveClearance * 0.5f);
            }

            if (snapshot.HasFrontlineStructure)
            {
                peekTargetZ = Mathf.Min(peekTargetZ, snapshot.FrontlineStructureZ - lanePeekObjectiveClearance);
            }

            Vector3 peekAnchor = new(
                GetLaneCenterX(resolvedLaneIndex),
                supportAnchor.y,
                Mathf.Clamp(peekTargetZ, supportEnvelopeMaxZ + 0.45f, playerForwardLimit - 0.95f));

            if (hasLiveAllies)
            {
                float allyPeekCap = snapshot.PlayerFrontZ - Mathf.Max(0.55f, heroRearMinGap * 0.28f);
                peekAnchor.z = Mathf.Min(peekAnchor.z, Mathf.Max(supportEnvelopeMaxZ + 0.2f, allyPeekCap));
            }

            float objectiveAdvanceZ = Mathf.Clamp(
                Mathf.Max(
                    supportEnvelopeMaxZ + 0.75f,
                    objectivePoint != null
                        ? objectivePoint.transform.position.z
                        : breachPoint != null
                            ? breachPoint.transform.position.z
                        : snapshot.HasFrontlineObjective
                            ? snapshot.FrontlineObjectiveZ - laneAdvanceObjectiveClearance
                            : peekAnchor.z + 0.65f),
                supportEnvelopeMaxZ + 0.55f,
                playerForwardLimit - 0.55f);

            Vector3 advanceAnchor = new(
                GetLaneCenterX(resolvedLaneIndex),
                supportAnchor.y,
                Mathf.Clamp(
                    Mathf.Max(
                        objectiveAdvanceZ,
                        coreSiegePoint != null && centerAdvanceUnlocked
                            ? Mathf.Max(objectiveAdvanceZ, coreSiegePoint.transform.position.z)
                            : objectiveAdvanceZ),
                    objectiveAdvanceZ,
                    playerForwardLimit - 0.25f));
            if (hasLiveAllies)
            {
                float allyObjectiveCap = snapshot.PlayerFrontZ - Mathf.Max(0.4f, heroRearMinGap * 0.12f);
                advanceAnchor.z = Mathf.Min(advanceAnchor.z, Mathf.Max(supportEnvelopeMaxZ + 0.35f, allyObjectiveCap));
            }

            float maxForwardZ = maxDepthBand switch
            {
                HeroLaneDepthBand.Fallback => retreatAnchor.z,
                HeroLaneDepthBand.Approach => supportEnvelopeMaxZ,
                HeroLaneDepthBand.Choke => peekAnchor.z,
                HeroLaneDepthBand.Objective => objectiveAdvanceZ,
                _ => advanceAnchor.z
            };

            switch (maxDepthBand)
            {
                case HeroLaneDepthBand.Fallback:
                    supportLeftAnchor = retreatAnchor;
                    supportCenterAnchor = retreatAnchor;
                    supportRightAnchor = retreatAnchor;
                    supportAnchor = retreatAnchor;
                    peekAnchor = retreatAnchor;
                    advanceAnchor = retreatAnchor;
                    break;

                case HeroLaneDepthBand.Approach:
                    peekAnchor = supportCenterAnchor;
                    advanceAnchor = supportCenterAnchor;
                    break;

                case HeroLaneDepthBand.Choke:
                    advanceAnchor = peekAnchor;
                    break;
            }

            supportLeftAnchor.z = Mathf.Min(supportLeftAnchor.z, maxForwardZ);
            supportCenterAnchor.z = Mathf.Min(supportCenterAnchor.z, maxForwardZ);
            supportRightAnchor.z = Mathf.Min(supportRightAnchor.z, maxForwardZ);
            supportAnchor = supportCenterAnchor;
            peekAnchor.z = Mathf.Min(peekAnchor.z, maxForwardZ);
            advanceAnchor.z = Mathf.Min(advanceAnchor.z, maxForwardZ);
            Vector3 primaryAnchor = ResolvePrimaryPhaseAnchor(escortPhase, supportAnchor, peekAnchor, advanceAnchor, retreatAnchor);
            supportEnvelopeMinZ = Mathf.Min(supportLeftAnchor.z, Mathf.Min(supportCenterAnchor.z, supportRightAnchor.z));
            supportEnvelopeMaxZ = Mathf.Max(supportLeftAnchor.z, Mathf.Max(supportCenterAnchor.z, supportRightAnchor.z));
            bool canOpenPeek = hasLiveAllies &&
                escortPhase != EscortPhase.Ready &&
                escortPhase != EscortPhase.Join &&
                maxDepthBand >= HeroLaneDepthBand.Choke;
            float laneThreatScore = ResolveLaneThreatScore(snapshot);
            float laneValueScore = ResolveLaneValueScore(snapshot, hasLiveAllies, hasRecentPrime);

            PlayerLaneSlot suggestedSlot = ClampPlayerLaneSlotToDepthBand(ResolveSuggestedPlayerSlot(snapshot, maxDepthBand, escortPhase), maxDepthBand);
            CoverState suggestedCoverState = suggestedSlot == PlayerLaneSlot.SupportCover || suggestedSlot == PlayerLaneSlot.Rear
                ? CoverState.SoftCover
                : CoverState.Exposed;

            state = new LaneCombatState(
                resolvedLaneIndex,
                snapshot.PressureState,
                suggestedSlot,
                suggestedCoverState,
                snapshot.HasFrontlineStructure,
                snapshot.FrontlineStructureZ,
                snapshot.HasFrontlineObjective,
                snapshot.FrontlineObjectiveZ,
                snapshot.ClashZ,
                snapshot.PlayerFrontZ,
                snapshot.EnemyFrontZ,
                snapshot.PlayerCount,
                snapshot.EnemyCount,
                Mathf.Clamp01(snapshot.ClashZ / Mathf.Max(1f, laneLength)),
                joinAnchor,
                supportAnchor,
                supportLeftAnchor,
                supportCenterAnchor,
                supportRightAnchor,
                peekAnchor,
                advanceAnchor,
                retreatAnchor,
                primaryAnchor,
                retreatAnchor,
                hasLiveAllies,
                hasRecentPrime,
                escortPhase,
                maxDepthBand,
                maxForwardZ,
                supportEnvelopeMinZ,
                supportEnvelopeMaxZ,
                canOpenPeek,
                ResolveHeroInterventionReasonInternal(
                    snapshot.PressureState,
                    snapshot.HasFrontlineStructure,
                    snapshot.RewardStructure != null || snapshot.SiegeStructure != null,
                    hasLiveAllies,
                    ManualTargetLockKind.None),
                laneThreatScore,
                laneValueScore);
            return true;
        }

        private LaneRuntimeSnapshot CollectLaneRuntimeSnapshot(int laneIndex)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            BattleStructure frontlineStructure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.FrontlineBlocker);
            BattleStructure rewardStructure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.RewardObjective);
            BattleStructure siegeStructure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.SiegeObjective);
            BattleStructure nearestStructure = BattleStructure.FindNearestActiveInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true);
            float playerFrontZ = playerSpawnInset;
            float enemyFrontZ = laneLength - playerSpawnInset;
            int playerCount = 0;
            int enemyCount = 0;

            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive || summonUnit.AssignedLaneIndex != resolvedLaneIndex)
                {
                    continue;
                }

                if (summonUnit.IsPlayerTeam)
                {
                    playerCount++;
                    playerFrontZ = Mathf.Max(playerFrontZ, summonUnit.transform.position.z);
                }
                else
                {
                    enemyCount++;
                    enemyFrontZ = Mathf.Min(enemyFrontZ, summonUnit.transform.position.z);
                }
            }

            bool hasPlayerUnits = playerCount > 0;
            bool hasEnemyUnits = enemyCount > 0;
            float mirroredEnemyDepth = laneLength - enemyFrontZ;
            float clashGap = enemyFrontZ - playerFrontZ;
            bool hasFrontlineStructure = frontlineStructure != null && !frontlineStructure.IsDestroyed;
            float frontlineStructureZ = hasFrontlineStructure ? frontlineStructure.transform.position.z : 0f;
            bool hasCoreSiegeAccess = resolvedLaneIndex == LaneCount / 2 && centerAdvanceUnlocked;
            bool hasFrontlineObjective = nearestStructure != null || hasEnemyUnits || hasCoreSiegeAccess;
            float frontlineObjectiveZ = nearestStructure != null
                ? nearestStructure.transform.position.z
                : hasEnemyUnits
                    ? enemyFrontZ
                    : hasCoreSiegeAccess && enemyBaseTransform != null
                        ? enemyBaseTransform.position.z - 1.4f
                        : 0f;
            float clashZ = hasPlayerUnits && hasEnemyUnits
                ? Mathf.Clamp((playerFrontZ + enemyFrontZ) * 0.5f, playerSpawnInset, laneLength - playerSpawnInset)
                : hasPlayerUnits
                    ? playerFrontZ
                    : hasEnemyUnits
                        ? enemyFrontZ
                        : laneSupportBaseZ;

            LanePressureState pressureState;
            if (!hasPlayerUnits && hasEnemyUnits)
            {
                pressureState = enemyFrontZ <= laneCollapseThreatZ
                    ? LanePressureState.Collapse
                    : LanePressureState.Empty;
            }
            else if (hasPlayerUnits && !hasEnemyUnits)
            {
                pressureState = LanePressureState.Push;
            }
            else if (!hasPlayerUnits && !hasEnemyUnits)
            {
                pressureState = LanePressureState.Empty;
            }
            else if (clashGap <= laneContestGap)
            {
                pressureState = LanePressureState.Contest;
            }
            else
            {
                float lead = playerFrontZ - mirroredEnemyDepth;
                pressureState = lead >= lanePushLeadDistance
                    ? LanePressureState.Push
                    : lead <= -lanePushLeadDistance
                        ? LanePressureState.Collapse
                        : LanePressureState.Contest;
            }

            return new LaneRuntimeSnapshot(
                resolvedLaneIndex,
                pressureState,
                frontlineStructure,
                rewardStructure,
                siegeStructure,
                nearestStructure,
                frontlineStructureZ,
                frontlineObjectiveZ,
                clashZ,
                playerFrontZ,
                enemyFrontZ,
                playerCount,
                enemyCount,
                hasFrontlineStructure,
                hasFrontlineObjective);
        }

        private HeroLaneDepthBand ResolveHeroLaneDepthBand(LaneRuntimeSnapshot snapshot, bool hasLiveAllies, bool hasRecentPrime)
        {
            if (!hasLiveAllies)
            {
                if (hasRecentPrime)
                {
                    return HeroLaneDepthBand.Approach;
                }

                return snapshot.LaneIndex == LaneCount / 2
                    ? HeroLaneDepthBand.Approach
                    : HeroLaneDepthBand.Fallback;
            }

            if (snapshot.HasFrontlineStructure)
            {
                return HeroLaneDepthBand.Choke;
            }

            if (snapshot.RewardStructure != null || snapshot.SiegeStructure != null)
            {
                return HeroLaneDepthBand.Objective;
            }

            if (snapshot.LaneIndex == LaneCount / 2 && centerAdvanceUnlocked)
            {
                return HeroLaneDepthBand.Advance;
            }

            return HeroLaneDepthBand.Choke;
        }

        private EscortPhase ResolveEscortPhase(
            LaneRuntimeSnapshot snapshot,
            bool hasLiveAllies,
            bool hasRecentPrime,
            HeroLaneDepthBand maxDepthBand)
        {
            if (!hasLiveAllies)
            {
                bool laneIsHot =
                    snapshot.HasFrontlineStructure ||
                    snapshot.HasFrontlineObjective ||
                    snapshot.EnemyCount > 0 ||
                    snapshot.PressureState == LanePressureState.Collapse;
                return laneIsHot
                    ? EscortPhase.Fallback
                    : EscortPhase.Ready;
            }

            if (snapshot.PressureState == LanePressureState.Collapse)
            {
                return EscortPhase.Fallback;
            }

            bool hasLaneEngagement = snapshot.HasFrontlineStructure || snapshot.EnemyCount > 0;

            if (snapshot.RewardStructure != null || snapshot.SiegeStructure != null || maxDepthBand >= HeroLaneDepthBand.Objective)
            {
                return EscortPhase.Objective;
            }

            if (hasLiveAllies && hasLaneEngagement)
            {
                return EscortPhase.BlockerHold;
            }

            return EscortPhase.Join;
        }

        private float ResolveLaneThreatScore(LaneRuntimeSnapshot snapshot)
        {
            float threatScore = snapshot.EnemyCount * 0.55f;
            threatScore += snapshot.PressureState switch
            {
                LanePressureState.Collapse => 2.2f,
                LanePressureState.Contest => 1.15f,
                LanePressureState.Push => 0.3f,
                _ => 0f
            };

            if (snapshot.HasFrontlineStructure)
            {
                threatScore += 1.1f;
            }

            if (snapshot.RewardStructure != null || snapshot.SiegeStructure != null)
            {
                threatScore += 0.85f;
            }

            threatScore += Mathf.Clamp01(snapshot.PlayerFrontZ / Mathf.Max(1f, laneLength)) * 0.65f;
            return threatScore;
        }

        private float ResolveLaneValueScore(LaneRuntimeSnapshot snapshot, bool hasLiveAllies, bool hasRecentPrime)
        {
            if (BattleModeContext.CurrentMode == BattleMode.StoryPve)
            {
                float storyValueScore = hasLiveAllies ? 1.05f : 0f;
                storyValueScore += hasRecentPrime ? 0.28f : 0f;
                storyValueScore += snapshot.HasFrontlineStructure ? 0.95f : 0f;
                storyValueScore += snapshot.RewardStructure != null ? 0.75f : 0f;
                storyValueScore += snapshot.SiegeStructure != null ? 0.95f : 0f;
                storyValueScore += Mathf.Min(0.9f, snapshot.EnemyCount * 0.22f);
                storyValueScore += snapshot.PressureState switch
                {
                    LanePressureState.Collapse => hasLiveAllies ? 0.22f : -0.12f,
                    LanePressureState.Contest => 0.14f,
                    LanePressureState.Push => 0.08f,
                    _ => 0f
                };
                return storyValueScore;
            }

            float valueScore = hasLiveAllies ? 2.1f : 0f;
            valueScore += hasRecentPrime ? 0.7f : 0f;
            valueScore += snapshot.PressureState switch
            {
                LanePressureState.Collapse when hasLiveAllies => 1.35f,
                LanePressureState.Contest when hasLiveAllies => 0.95f,
                LanePressureState.Push when hasLiveAllies => 0.55f,
                _ => 0f
            };

            if (snapshot.HasFrontlineStructure && hasLiveAllies)
            {
                valueScore += 1.1f;
            }

            if ((snapshot.RewardStructure != null || snapshot.SiegeStructure != null) && hasLiveAllies)
            {
                valueScore += 1.35f;
            }

            return valueScore;
        }

        private static PlayerLaneSlot ResolveSuggestedPlayerSlot(
            LaneRuntimeSnapshot snapshot,
            HeroLaneDepthBand maxDepthBand,
            EscortPhase escortPhase)
        {
            if (snapshot.PressureState == LanePressureState.Collapse)
            {
                return PlayerLaneSlot.Rear;
            }

            if (escortPhase == EscortPhase.Ready || escortPhase == EscortPhase.Join)
            {
                return PlayerLaneSlot.SupportCover;
            }

            if (escortPhase == EscortPhase.Fallback || maxDepthBand <= HeroLaneDepthBand.Fallback)
            {
                return PlayerLaneSlot.Rear;
            }

            if (escortPhase == EscortPhase.BlockerHold)
            {
                return PlayerLaneSlot.SupportCover;
            }

            if (escortPhase == EscortPhase.Objective)
            {
                return PlayerLaneSlot.SupportCover;
            }

            if (escortPhase == EscortPhase.Breach)
            {
                return PlayerLaneSlot.Peek;
            }

            if (maxDepthBand == HeroLaneDepthBand.Advance && snapshot.HasFrontlineObjective)
            {
                return PlayerLaneSlot.Peek;
            }

            if (snapshot.HasFrontlineObjective && maxDepthBand >= HeroLaneDepthBand.Choke)
            {
                return PlayerLaneSlot.Peek;
            }

            return PlayerLaneSlot.SupportCover;
        }

        private static PlayerLaneSlot ClampPlayerLaneSlotToDepthBand(PlayerLaneSlot slot, HeroLaneDepthBand maxDepthBand)
        {
            PlayerLaneSlot maxSlot = maxDepthBand switch
            {
                HeroLaneDepthBand.Fallback => PlayerLaneSlot.Rear,
                HeroLaneDepthBand.Approach => PlayerLaneSlot.SupportCover,
                HeroLaneDepthBand.Choke => PlayerLaneSlot.Peek,
                _ => PlayerLaneSlot.Advance
            };
            return slot > maxSlot ? maxSlot : slot;
        }

        private static HeroInterventionReason ResolveHeroInterventionReasonInternal(
            LanePressureState pressureState,
            bool hasFrontlineStructure,
            bool hasRewardObjective,
            bool hasLiveAllies,
            ManualTargetLockKind manualTargetKind)
        {
            if (manualTargetKind == ManualTargetLockKind.Boss && !hasLiveAllies)
            {
                return HeroInterventionReason.BossPressure;
            }

            if (hasLiveAllies && hasFrontlineStructure)
            {
                return HeroInterventionReason.BreakBlocker;
            }

            if (hasLiveAllies && hasRewardObjective)
            {
                return HeroInterventionReason.CashReward;
            }

            if (hasLiveAllies && (pressureState == LanePressureState.Contest || pressureState == LanePressureState.Collapse))
            {
                return HeroInterventionReason.AssistWave;
            }

            return manualTargetKind == ManualTargetLockKind.Boss
                ? HeroInterventionReason.BossPressure
                : HeroInterventionReason.Escort;
        }

#pragma warning disable CS0162
        public bool TryGetLaneCombatState(int laneIndex, out LaneCombatState state)
        {
            return TryBuildLeashedLaneCombatState(laneIndex, out state);

            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            LaneAnchorSet laneAnchorSet = GetLaneAnchorSet(resolvedLaneIndex);
            BattleStructure frontlineStructure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.FrontlineBlocker);
            BattleStructure rewardStructure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.RewardObjective);
            BattleStructure siegeStructure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.SiegeObjective);
            BattleStructure nearestStructure = BattleStructure.FindNearestActiveInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true);
            float playerFrontZ = playerSpawnInset;
            float enemyFrontZ = laneLength - playerSpawnInset;
            int playerCount = 0;
            int enemyCount = 0;

            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive || summonUnit.AssignedLaneIndex != resolvedLaneIndex)
                {
                    continue;
                }

                if (summonUnit.IsPlayerTeam)
                {
                    playerCount++;
                    playerFrontZ = Mathf.Max(playerFrontZ, summonUnit.transform.position.z);
                }
                else
                {
                    enemyCount++;
                    enemyFrontZ = Mathf.Min(enemyFrontZ, summonUnit.transform.position.z);
                }
            }

            bool hasPlayerUnits = playerCount > 0;
            bool hasEnemyUnits = enemyCount > 0;
            float mirroredEnemyDepth = laneLength - enemyFrontZ;
            float clashGap = enemyFrontZ - playerFrontZ;
            bool hasFrontlineStructure = frontlineStructure != null && !frontlineStructure.IsDestroyed;
            float frontlineStructureZ = hasFrontlineStructure ? frontlineStructure.transform.position.z : 0f;
            bool hasFrontlineObjective = nearestStructure != null || hasEnemyUnits;
            float frontlineObjectiveZ = nearestStructure != null
                ? nearestStructure.transform.position.z
                : hasEnemyUnits
                    ? enemyFrontZ
                    : 0f;
            bool hasAlliedPresence = hasPlayerUnits || IsLanePrimed(resolvedLaneIndex);
            bool canEnterChokeBand = hasAlliedPresence;
            bool canEnterObjectiveBand = hasAlliedPresence &&
                (resolvedLaneIndex != LaneCount / 2 || centerAdvanceUnlocked || siegeStructure == null);
            float clashZ = hasPlayerUnits && hasEnemyUnits
                ? Mathf.Clamp((playerFrontZ + enemyFrontZ) * 0.5f, playerSpawnInset, laneLength - playerSpawnInset)
                : hasPlayerUnits
                    ? playerFrontZ
                    : hasEnemyUnits
                        ? enemyFrontZ
                        : laneSupportBaseZ;

            LanePressureState pressureState;
            if (!hasPlayerUnits && hasEnemyUnits)
            {
                pressureState = enemyFrontZ <= laneCollapseThreatZ
                    ? LanePressureState.Collapse
                    : LanePressureState.Empty;
            }
            else if (hasPlayerUnits && !hasEnemyUnits)
            {
                pressureState = LanePressureState.Push;
            }
            else if (!hasPlayerUnits && !hasEnemyUnits)
            {
                pressureState = LanePressureState.Empty;
            }
            else if (clashGap <= laneContestGap)
            {
                pressureState = LanePressureState.Contest;
            }
            else
            {
                float lead = playerFrontZ - mirroredEnemyDepth;
                pressureState = lead >= lanePushLeadDistance
                    ? LanePressureState.Push
                    : lead <= -lanePushLeadDistance
                        ? LanePressureState.Collapse
                        : LanePressureState.Contest;
            }

            PlayerLaneSlot suggestedSlot;
            CoverState suggestedCoverState;
            if (!hasAlliedPresence)
            {
                suggestedSlot = PlayerLaneSlot.SupportCover;
                suggestedCoverState = CoverState.SoftCover;
            }
            else if (pressureState == LanePressureState.Collapse)
            {
                suggestedSlot = PlayerLaneSlot.Rear;
                suggestedCoverState = CoverState.SoftCover;
            }
            else if (hasFrontlineStructure)
            {
                suggestedSlot = pressureState == LanePressureState.Push
                    ? PlayerLaneSlot.Peek
                    : PlayerLaneSlot.SupportCover;
                suggestedCoverState = suggestedSlot == PlayerLaneSlot.SupportCover
                    ? CoverState.SoftCover
                    : CoverState.Exposed;
            }
            else if ((rewardStructure != null || (centerAdvanceUnlocked && siegeStructure == null)) && canEnterObjectiveBand)
            {
                suggestedSlot = pressureState == LanePressureState.Push
                    ? PlayerLaneSlot.Advance
                    : PlayerLaneSlot.Peek;
                suggestedCoverState = CoverState.Exposed;
            }
            else if (hasFrontlineObjective && canEnterChokeBand)
            {
                suggestedSlot = PlayerLaneSlot.Peek;
                suggestedCoverState = CoverState.Exposed;
            }
            else
            {
                suggestedSlot = PlayerLaneSlot.SupportCover;
                suggestedCoverState = CoverState.SoftCover;
            }

            float normalizedClash = Mathf.Clamp01(clashZ / Mathf.Max(1f, laneLength));
            Vector3 supportAnchor = ResolveLaneAnchorPosition(
                laneAnchorSet != null ? laneAnchorSet.SupportCoverAnchor : null,
                resolvedLaneIndex,
                laneSupportBaseZ);
            Vector3 retreatAnchor = ResolveLaneAnchorPosition(
                laneAnchorSet != null ? laneAnchorSet.RearAnchor : null,
                resolvedLaneIndex,
                laneRearSlotZ);
            Vector3 peekAnchor = ResolveLaneAnchorPosition(
                laneAnchorSet != null ? laneAnchorSet.PeekAnchor : null,
                resolvedLaneIndex,
                laneSupportBaseZ + Mathf.Max(1.4f, (laneLength * runtimeFrontlineStructureNormalizedZ - laneSupportBaseZ) * 0.55f));
            Vector3 advanceAnchor = ResolveLaneAnchorPosition(
                laneAnchorSet != null ? laneAnchorSet.AdvanceBaseAnchor : null,
                resolvedLaneIndex,
                laneLength * Mathf.Clamp01(runtimeFrontlineStructureNormalizedZ) - Mathf.Max(0.7f, laneAdvanceObjectiveClearance));

            // Support position tracks behind friendly frontline instead of fixed Z
            if (hasPlayerUnits)
            {
                float frontlineFollowZ = playerFrontZ - laneSupportRearOffset;
                supportAnchor.z = Mathf.Max(supportAnchor.z, frontlineFollowZ);
            }
            supportAnchor.z = Mathf.Clamp(supportAnchor.z, playerLaneMinZ + 0.6f, playerForwardLimit - 1.35f);
            retreatAnchor.z = Mathf.Clamp(retreatAnchor.z, playerLaneMinZ + 0.35f, supportAnchor.z - 0.5f);

            if (!hasFrontlineObjective)
            {
                peekAnchor = supportAnchor;
                advanceAnchor = supportAnchor;
            }
            else
            {
                float peekMaxZ = Mathf.Max(supportAnchor.z + 0.35f, frontlineObjectiveZ - lanePeekObjectiveClearance);
                // Peek tracks toward the clash zone when both sides are engaged
                if (hasPlayerUnits && hasEnemyUnits)
                {
                    float clashApproachZ = clashZ - lanePeekObjectiveClearance * 0.5f;
                    peekMaxZ = Mathf.Max(peekMaxZ, clashApproachZ);
                }
                peekAnchor.z = Mathf.Clamp(peekAnchor.z, supportAnchor.z + 0.35f, Mathf.Min(peekMaxZ, playerForwardLimit - 0.95f));

                float advanceMaxZ = Mathf.Max(peekAnchor.z + 0.45f, frontlineObjectiveZ - laneAdvanceObjectiveClearance);
                advanceAnchor.z = Mathf.Clamp(advanceAnchor.z, peekAnchor.z + 0.45f, Mathf.Min(advanceMaxZ, playerForwardLimit - 0.55f));
                // On push, advance follows just behind the leading friendly unit
                if (pressureState == LanePressureState.Push && hasPlayerUnits)
                {
                    float leadFollowZ = playerFrontZ - lanePushRearOffset;
                    advanceAnchor.z = Mathf.Clamp(
                        Mathf.Max(advanceAnchor.z, leadFollowZ),
                        peekAnchor.z + 0.45f,
                        Mathf.Min(advanceMaxZ, playerForwardLimit - 0.55f));
                }
            }

            BattlefieldPoint fallbackPoint = BattlefieldPoint.FindHighestPriorityInLane(resolvedLaneIndex, BattlefieldPointType.FallbackPocket);
            BattlefieldPoint approachPoint = BattlefieldPoint.FindClosestInLane(
                resolvedLaneIndex,
                BattlefieldPointType.ApproachPocket,
                isPlayerTeam: true,
                summonSpawnInset);
            BattlefieldPoint objectivePoint = BattlefieldPoint.FindHighestPriorityInLane(resolvedLaneIndex, BattlefieldPointType.ObjectiveAnchor);
            BattlefieldPoint advancePoint = BattlefieldPoint.FindHighestPriorityInLane(resolvedLaneIndex, BattlefieldPointType.AdvancePocket);
            if (fallbackPoint != null)
            {
                retreatAnchor = ResolveLaneAnchorPosition(fallbackPoint.transform, resolvedLaneIndex, retreatAnchor.z);
            }

            if (hasAlliedPresence && approachPoint != null)
            {
                supportAnchor = ResolveLaneAnchorPosition(approachPoint.transform, resolvedLaneIndex, supportAnchor.z);
            }

            supportAnchor.z = Mathf.Clamp(supportAnchor.z, playerLaneMinZ + 0.6f, playerForwardLimit - 1.35f);
            retreatAnchor.z = Mathf.Clamp(retreatAnchor.z, playerLaneMinZ + 0.35f, supportAnchor.z - 0.5f);

            if (!hasFrontlineObjective || !canEnterChokeBand)
            {
                peekAnchor = supportAnchor;
                advanceAnchor = supportAnchor;
            }
            else
            {
                float authoredPeekZ = hasFrontlineStructure
                    ? frontlineStructureZ - lanePeekObjectiveClearance
                    : objectivePoint != null
                        ? objectivePoint.transform.position.z
                        : frontlineObjectiveZ - lanePeekObjectiveClearance;
                if (hasPlayerUnits && hasEnemyUnits)
                {
                    authoredPeekZ = Mathf.Max(authoredPeekZ, clashZ - lanePeekObjectiveClearance * 0.5f);
                }

                peekAnchor.z = Mathf.Clamp(authoredPeekZ, supportAnchor.z + 0.35f, playerForwardLimit - 0.95f);

                float authoredAdvanceZ = objectivePoint != null
                    ? objectivePoint.transform.position.z
                    : frontlineObjectiveZ - laneAdvanceObjectiveClearance;
                if (advancePoint != null && centerAdvanceUnlocked)
                {
                    authoredAdvanceZ = Mathf.Max(authoredAdvanceZ, advancePoint.transform.position.z);
                }

                if (!canEnterObjectiveBand)
                {
                    authoredAdvanceZ = peekAnchor.z + 0.55f;
                }

                advanceAnchor.z = Mathf.Clamp(
                    Mathf.Max(peekAnchor.z + 0.45f, authoredAdvanceZ),
                    peekAnchor.z + 0.45f,
                    playerForwardLimit - 0.55f);
            }

            LaneCombatState provisionalState = new LaneCombatState(
                resolvedLaneIndex,
                pressureState,
                suggestedSlot,
                suggestedCoverState,
                hasFrontlineStructure,
                frontlineStructureZ,
                hasFrontlineObjective,
                frontlineObjectiveZ,
                clashZ,
                playerFrontZ,
                enemyFrontZ,
                playerCount,
                enemyCount,
                normalizedClash,
                supportAnchor,
                supportAnchor,
                supportAnchor,
                supportAnchor,
                supportAnchor,
                peekAnchor,
                advanceAnchor,
                retreatAnchor);
            state = new LaneCombatState(
                resolvedLaneIndex,
                pressureState,
                suggestedSlot,
                suggestedCoverState,
                hasFrontlineStructure,
                frontlineStructureZ,
                hasFrontlineObjective,
                frontlineObjectiveZ,
                clashZ,
                playerFrontZ,
                enemyFrontZ,
                playerCount,
                enemyCount,
                normalizedClash,
                supportAnchor,
                supportAnchor,
                supportAnchor,
                supportAnchor,
                supportAnchor,
                peekAnchor,
                advanceAnchor,
                retreatAnchor);
            return true;
        }
#pragma warning restore CS0162

        public float ResolvePlayerSlotZ(LaneCombatState state, PlayerLaneSlot slot)
        {
            PlayerLaneSlot resolvedSlot = ClampPlayerLaneSlotToDepthBand(slot, state.MaxDepthBand);
            return resolvedSlot switch
            {
                PlayerLaneSlot.Rear => state.RetreatAnchorZ,
                PlayerLaneSlot.SupportCover => state.SupportAnchorZ,
                PlayerLaneSlot.Peek => state.PeekAnchorZ,
                _ => state.AdvanceAnchorZ
            };
        }

        private static Vector3 ResolvePrimaryPhaseAnchor(
            EscortPhase escortPhase,
            Vector3 supportAnchor,
            Vector3 peekAnchor,
            Vector3 advanceAnchor,
            Vector3 retreatAnchor)
        {
            bool storyPveMode = BattleModeContext.CurrentMode == BattleMode.StoryPve;
            return escortPhase switch
            {
                EscortPhase.Ready => supportAnchor,
                EscortPhase.Join => supportAnchor,
                EscortPhase.BlockerHold => supportAnchor,
                EscortPhase.Breach => peekAnchor,
                EscortPhase.Objective => storyPveMode ? supportAnchor : advanceAnchor,
                EscortPhase.Fallback => retreatAnchor,
                _ => supportAnchor
            };
        }

        public Vector3 ResolvePlayerSlotAnchor(int laneIndex, PlayerLaneSlot slot, float worldY)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            float anchorX = GetLaneCenterX(resolvedLaneIndex);
            if (!TryGetLaneCombatState(resolvedLaneIndex, out LaneCombatState laneState))
            {
                return new Vector3(anchorX, worldY, Mathf.Clamp(laneSupportBaseZ, playerLaneMinZ, playerForwardLimit));
            }

            return ResolvePlayerSlotAnchor(laneState, slot, worldY);
        }

        public Vector3 ResolvePlayerSlotAnchor(LaneCombatState laneState, PlayerLaneSlot slot, float worldY)
        {
            PlayerLaneSlot resolvedSlot = ClampPlayerLaneSlotToDepthBand(slot, laneState.MaxDepthBand);
            Vector3 anchor = resolvedSlot switch
            {
                PlayerLaneSlot.Rear => laneState.RetreatAnchor,
                PlayerLaneSlot.SupportCover => laneState.SupportAnchor,
                PlayerLaneSlot.Peek => laneState.PeekAnchor,
                _ => laneState.AdvanceAnchor
            };

            anchor.y = worldY;
            anchor.z = Mathf.Min(anchor.z, laneState.MaxForwardZ);
            return anchor;
        }

        public bool TryGetFrontlineState(out FrontlineState state)
        {
            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            float playerFrontZ = playerSpawnInset;
            float enemyClosestZ = laneLength - playerSpawnInset;
            int playerCount = 0;
            int enemyCount = 0;

            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive)
                {
                    continue;
                }

                if (summonUnit.IsPlayerTeam)
                {
                    playerCount++;
                    playerFrontZ = Mathf.Max(playerFrontZ, summonUnit.transform.position.z);
                }
                else
                {
                    enemyCount++;
                    enemyClosestZ = Mathf.Min(enemyClosestZ, summonUnit.transform.position.z);
                }
            }

            float enemyPressure = laneLength - enemyClosestZ;
            float balanceDenominator = Mathf.Max(8f, laneLength * 0.34f);
            float balance = Mathf.Clamp((playerFrontZ - enemyPressure) / balanceDenominator, -1f, 1f);
            float clashCenterZ = playerCount > 0 || enemyCount > 0
                ? Mathf.Clamp01(((playerFrontZ + enemyClosestZ) * 0.5f) / Mathf.Max(1f, laneLength))
                : 0.5f;

            state = new FrontlineState(balance, playerFrontZ, enemyClosestZ, playerCount, enemyCount, clashCenterZ);
            return true;
        }

        public bool TryGetPlayerTerritoryState(out PlayerTerritoryState state)
        {
            UpdateCachedTerritoryState();
            state = cachedTerritoryState;
            return hasCachedTerritoryState;
        }

        public float GetTerritoryWarningLeadDistance()
        {
            return territoryWarningLeadDistance;
        }

        public bool IsLanePrimed(int laneIndex)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            return Time.time < friendlyLanePrimeUntil[resolvedLaneIndex];
        }

        public bool IsLaneRallyActive(int laneIndex)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            return Time.time < laneRallyUntil[resolvedLaneIndex];
        }

        public float GetLaneRallyRemaining(int laneIndex)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            return Mathf.Max(0f, laneRallyUntil[resolvedLaneIndex] - Time.time);
        }

        public bool IsCenterAdvanceUnlocked()
        {
            return centerAdvanceUnlocked;
        }

        public bool TryGetHeroLaneLeashState(int laneIndex, ManualTargetLockKind manualTargetKind, out HeroLaneLeashState state)
        {
            if (!TryGetLaneCombatState(laneIndex, out LaneCombatState laneState))
            {
                state = default;
                return false;
            }

            bool hasRewardObjective = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                laneState.LaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.RewardObjective) != null ||
                BattleStructure.FindNearestRoleInLaneAlongAdvance(
                    laneState.LaneIndex,
                    isPlayerTeam: true,
                    BattleStructureRole.SiegeObjective) != null;
            state = new HeroLaneLeashState(
                laneState.HasLiveAllies,
                laneState.HasRecentPrime,
                laneState.EscortPhase,
                laneState.MaxDepthBand,
                laneState.MaxForwardZ,
                laneState.PrimaryAnchor,
                laneState.FallbackAnchor,
                ResolveHeroInterventionReasonInternal(
                    laneState.PressureState,
                    laneState.HasFrontlineStructure,
                    hasRewardObjective,
                    laneState.HasLiveAllies,
                    manualTargetKind));
            return true;
        }

        public HeroInterventionReason ResolveHeroInterventionReason(int laneIndex, ManualTargetLockKind manualTargetKind = ManualTargetLockKind.None)
        {
            return TryGetHeroLaneLeashState(laneIndex, manualTargetKind, out HeroLaneLeashState leashState)
                ? leashState.InterventionReason
                : HeroInterventionReason.Escort;
        }

        public bool TryGetPreferredInterventionStructure(int laneIndex, out BattleStructure structure)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            structure = null;
            bool hasLiveAllies = TryGetLaneCombatState(resolvedLaneIndex, out LaneCombatState laneState) &&
                laneState.HasLiveAllies;

            if (hasLiveAllies)
            {
                structure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                    resolvedLaneIndex,
                    isPlayerTeam: true,
                    BattleStructureRole.FrontlineBlocker);
                if (structure != null)
                {
                    return true;
                }

                structure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                    resolvedLaneIndex,
                    isPlayerTeam: true,
                    BattleStructureRole.RewardObjective);
                if (structure != null)
                {
                    return true;
                }

                structure = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                    resolvedLaneIndex,
                    isPlayerTeam: true,
                    BattleStructureRole.SiegeObjective);
                if (structure != null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetHeroLaneContext(int laneIndex, out HeroLaneContext context)
        {
            if (!TryGetLaneCombatState(laneIndex, out LaneCombatState laneState))
            {
                context = default;
                return false;
            }

            context = new HeroLaneContext(
                laneState.LaneIndex,
                laneState.PlayerUnitCount,
                laneState.EnemyUnitCount,
                laneState.PressureState,
                laneState.HasFrontlineStructure,
                laneState.HasFrontlineObjective,
                laneState.PlayerFrontZ,
                laneState.EnemyFrontZ,
                laneState.JoinAnchor,
                laneState.SupportAnchors,
                laneState.PeekAnchor,
                laneState.AdvanceAnchor,
                laneState.FallbackAnchor,
                laneState.LaneThreatScore,
                laneState.LaneValueScore,
                laneState.HasRecentPrime,
                laneState.EscortPhase,
                laneState.SupportEnvelopeMinZ,
                laneState.SupportEnvelopeMaxZ,
                laneState.CanOpenPeek);
            return true;
        }

        public HeroLaneContext[] BuildHeroLaneContexts()
        {
            HeroLaneContext[] contexts = new HeroLaneContext[LaneCount];
            for (int laneIndex = 0; laneIndex < LaneCount; laneIndex++)
            {
                if (TryGetHeroLaneContext(laneIndex, out HeroLaneContext context))
                {
                    contexts[laneIndex] = context;
                }
            }

            return contexts;
        }

        public bool TryGetSummonLanePreview(int laneIndex, out SummonLanePreview preview)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            Vector3 landingPosition = new(GetLaneCenterX(resolvedLaneIndex), playerSpawn != null ? playerSpawn.position.y : 0f, summonSpawnInset + 0.8f);
            Vector3 firstPocketPosition = new(GetLaneCenterX(resolvedLaneIndex), landingPosition.y, laneSupportBaseZ);
            Vector3 blockerPosition = Vector3.zero;
            Vector3 rewardPosition = Vector3.zero;
            bool hasBlocker = false;
            bool hasReward = false;

            BattlefieldPoint pocketPoint = FindClosestLanePoint(
                resolvedLaneIndex,
                BattlefieldPointType.JoinPocket,
                BattlefieldPointType.ApproachPocket,
                isPlayerTeam: true,
                summonSpawnInset);
            if (pocketPoint == null)
            {
                pocketPoint = FindHighestPriorityLanePoint(
                    resolvedLaneIndex,
                    BattlefieldPointType.FallbackPocket,
                    BattlefieldPointType.ReadyPocket);
            }

            if (pocketPoint != null)
            {
                firstPocketPosition = pocketPoint.transform.position;
            }

            BattleStructure blocker = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.FrontlineBlocker);
            if (blocker != null)
            {
                blockerPosition = blocker.transform.position;
                hasBlocker = true;
            }

            BattleStructure reward = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                resolvedLaneIndex,
                isPlayerTeam: true,
                BattleStructureRole.RewardObjective);
            if (reward == null)
            {
                reward = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                    resolvedLaneIndex,
                    isPlayerTeam: true,
                    BattleStructureRole.SiegeObjective);
            }
            if (reward != null)
            {
                rewardPosition = reward.transform.position;
                hasReward = true;
            }

            SummonLanePreviewState previewState = hasReward
                ? SummonLanePreviewState.Reward
                : hasBlocker
                    ? SummonLanePreviewState.Break
                    : SummonLanePreviewState.Stall;
            preview = new SummonLanePreview(
                resolvedLaneIndex,
                previewState,
                landingPosition,
                firstPocketPosition,
                hasBlocker,
                blockerPosition,
                hasReward,
                rewardPosition);
            return true;
        }

        private void HookRuntimeEvents()
        {
            if (summonSpawner == null)
            {
                summonSpawner = FindFirstObjectByType<SummonSpawner>();
            }

            if (summonSpawner != null && !subscribedSummonSpawner)
            {
                summonSpawner.OnSummonSpawned += HandleSummonSpawned;
                subscribedSummonSpawner = true;
            }

            if (!subscribedStructureEvents)
            {
                BattleStructure.OnStructureDestroyed += HandleStructureDestroyed;
                subscribedStructureEvents = true;
            }
        }

        private void HandleSummonSpawned(SummonData summonData, Vector3 spawnPosition, bool isPlayerTeam)
        {
            if (!isPlayerTeam)
            {
                return;
            }

            int laneIndex = GetNearestLaneIndex(spawnPosition.x);
            friendlyLanePrimeUntil[laneIndex] = Time.time + ResolveLanePrimeDuration(summonData);
        }

        private void HandleStructureDestroyed(BattleStructure structure, bool causedByPlayerTeam)
        {
            if (!causedByPlayerTeam || structure == null)
            {
                return;
            }

            int laneIndex = GetNearestLaneIndex(structure.transform.position.x);
            switch (structure.Role)
            {
                case BattleStructureRole.RewardObjective:
                    laneRallyUntil[laneIndex] = Time.time + 6f;
                    BattlePresentationController.Instance?.AddFeedMessage(
                        $"Reward lane {laneIndex + 1} opened. Rally surge active.",
                        new Color(0.52f, 1f, 0.74f, 1f));
                    break;

                case BattleStructureRole.SiegeObjective:
                    centerAdvanceUnlocked = true;
                    LayoutLaneAnchorSets();
                    BattlePresentationController.Instance?.ShowBanner(
                        "CENTER OPEN",
                        "The siege anchor collapsed. Middle advance pocket is now live.",
                        new Color(0.54f, 0.95f, 1f, 1f),
                        1.15f);
                    break;
            }
        }

        private float ResolveLanePrimeDuration(SummonData summonData)
        {
            if (summonData == null)
            {
                return 2.8f;
            }

            bool isBreaker = summonData.summonType == SummonType.Melee && summonData.structureDamageMultiplier >= 1.8f;
            return summonData.summonType switch
            {
                SummonType.Support => 4.9f,
                SummonType.Ranged => 4.2f,
                SummonType.Tank => 3.5f,
                SummonType.Melee when isBreaker => 3.8f,
                _ => 2.8f
            };
        }

        private void ResolveSceneReferences()
        {
            if (battlefieldLayoutRoot == null)
            {
                GameObject layoutRoot = GameObject.Find("BattlefieldLayout");
                battlefieldLayoutRoot = layoutRoot != null ? layoutRoot.transform : null;
            }

            if (playerBaseTransform == null)
            {
                GameObject playerBase = GameObject.Find("PlayerBase");
                playerBaseTransform = playerBase != null ? playerBase.transform : null;
            }

            if (enemyBaseTransform == null)
            {
                GameObject enemyBase = GameObject.Find("EnemyBase");
                enemyBaseTransform = enemyBase != null ? enemyBase.transform : null;
            }

            if (playerSpawn == null)
            {
                GameObject spawn = GameObject.Find("PlayerSpawn");
                playerSpawn = spawn != null ? spawn.transform : null;
            }

            if (enemySpawn == null)
            {
                GameObject spawn = GameObject.Find("EnemySpawn");
                enemySpawn = spawn != null ? spawn.transform : null;
            }

            if (summonSpawnPoint == null)
            {
                GameObject spawn = GameObject.Find("SummonSpawnPoint");
                summonSpawnPoint = spawn != null ? spawn.transform : null;
            }

            if (enemySummonSpawnPoint == null)
            {
                GameObject spawn = GameObject.Find("EnemySummonSpawnPoint");
                enemySummonSpawnPoint = spawn != null ? spawn.transform : null;
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
                EnsurePlayerSkillController();
            }
        }

        private void EnsurePrototypeBattlefieldLayout()
        {
            if (battlefieldLayoutRoot == null)
            {
                GameObject rootObject = GameObject.Find("BattlefieldLayout");
                if (rootObject == null)
                {
                    rootObject = new GameObject("BattlefieldLayout");
                }

                battlefieldLayoutRoot = rootObject.transform;
            }

            EnsurePrototypeBattlefieldPoints();
            EnsurePrototypeBattlefieldStructures();
            LayoutLaneAnchorSets();
        }

        private void EnsurePrototypeBattlefieldPoints()
        {
            EnsureBattlefieldPoint("ReadyPocket_L3", 2, 8.6f, BattlefieldPointType.ReadyPocket, BattlefieldPointUnlockRule.Always, 1.6f);

            EnsureBattlefieldPoint("FallbackPocket_L1", 0, laneRearSlotZ + 3.8f, BattlefieldPointType.FallbackPocket, BattlefieldPointUnlockRule.Always, 0.6f);
            EnsureBattlefieldPoint("FallbackPocket_L2", 1, laneRearSlotZ + 3.95f, BattlefieldPointType.FallbackPocket, BattlefieldPointUnlockRule.Always, 0.9f);
            EnsureBattlefieldPoint("FallbackPocket_L3", 2, laneRearSlotZ + 4.2f, BattlefieldPointType.FallbackPocket, BattlefieldPointUnlockRule.Always, 1.1f);
            EnsureBattlefieldPoint("FallbackPocket_L4", 3, laneRearSlotZ + 3.95f, BattlefieldPointType.FallbackPocket, BattlefieldPointUnlockRule.Always, 0.9f);
            EnsureBattlefieldPoint("FallbackPocket_L5", 4, laneRearSlotZ + 3.8f, BattlefieldPointType.FallbackPocket, BattlefieldPointUnlockRule.Always, 0.6f);

            EnsureBattlefieldPoint("JoinPocket_L1", 0, 14.2f, BattlefieldPointType.JoinPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 0.9f);
            EnsureBattlefieldPoint("JoinPocket_L2", 1, 16.4f, BattlefieldPointType.JoinPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 1.2f);
            EnsureBattlefieldPoint("JoinPocket_L3", 2, 17.8f, BattlefieldPointType.JoinPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 1.35f);
            EnsureBattlefieldPoint("JoinPocket_L4", 3, 16.4f, BattlefieldPointType.JoinPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 1.2f);
            EnsureBattlefieldPoint("JoinPocket_L5", 4, 14.2f, BattlefieldPointType.JoinPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 0.9f);

            float supportPocketOffset = Mathf.Min(supportEnvelopeLateralOffset, LaneHalfWidth * 0.18f);
            EnsureBattlefieldPoint("SupportLeftPocket_L1", 0, 18.6f, BattlefieldPointType.SupportLeftPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.88f, -supportPocketOffset);
            EnsureBattlefieldPoint("SupportCenterPocket_L1", 0, 18.8f, BattlefieldPointType.SupportCenterPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1f);
            EnsureBattlefieldPoint("SupportRightPocket_L1", 0, 18.6f, BattlefieldPointType.SupportRightPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.88f, supportPocketOffset);
            EnsureBattlefieldPoint("SupportLeftPocket_L2", 1, 20.9f, BattlefieldPointType.SupportLeftPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.92f, -supportPocketOffset);
            EnsureBattlefieldPoint("SupportCenterPocket_L2", 1, 21.2f, BattlefieldPointType.SupportCenterPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.08f);
            EnsureBattlefieldPoint("SupportRightPocket_L2", 1, 20.9f, BattlefieldPointType.SupportRightPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.92f, supportPocketOffset);
            EnsureBattlefieldPoint("SupportLeftPocket_L3", 2, 22.2f, BattlefieldPointType.SupportLeftPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.95f, -supportPocketOffset);
            EnsureBattlefieldPoint("SupportCenterPocket_L3", 2, 22.6f, BattlefieldPointType.SupportCenterPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.15f);
            EnsureBattlefieldPoint("SupportRightPocket_L3", 2, 22.2f, BattlefieldPointType.SupportRightPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.95f, supportPocketOffset);
            EnsureBattlefieldPoint("SupportLeftPocket_L4", 3, 20.9f, BattlefieldPointType.SupportLeftPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.92f, -supportPocketOffset);
            EnsureBattlefieldPoint("SupportCenterPocket_L4", 3, 21.2f, BattlefieldPointType.SupportCenterPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.08f);
            EnsureBattlefieldPoint("SupportRightPocket_L4", 3, 20.9f, BattlefieldPointType.SupportRightPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.92f, supportPocketOffset);
            EnsureBattlefieldPoint("SupportLeftPocket_L5", 4, 18.6f, BattlefieldPointType.SupportLeftPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.88f, -supportPocketOffset);
            EnsureBattlefieldPoint("SupportCenterPocket_L5", 4, 18.8f, BattlefieldPointType.SupportCenterPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1f);
            EnsureBattlefieldPoint("SupportRightPocket_L5", 4, 18.6f, BattlefieldPointType.SupportRightPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.88f, supportPocketOffset);

            EnsureBattlefieldPoint("BlockerHoldPocket_L1", 0, 27.6f, BattlefieldPointType.BlockerHoldPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.95f);
            EnsureBattlefieldPoint("BlockerHoldPocket_L2", 1, 31.7f, BattlefieldPointType.BlockerHoldPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.1f);
            EnsureBattlefieldPoint("BlockerHoldPocket_L3", 2, 29.5f, BattlefieldPointType.BlockerHoldPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.2f);
            EnsureBattlefieldPoint("BlockerHoldPocket_L4", 3, 32f, BattlefieldPointType.BlockerHoldPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.1f);
            EnsureBattlefieldPoint("BlockerHoldPocket_L5", 4, 28.1f, BattlefieldPointType.BlockerHoldPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.95f);

            EnsureBattlefieldPoint("PeekPocket_L1", 0, 25.8f, BattlefieldPointType.PeekPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.9f);
            EnsureBattlefieldPoint("PeekPocket_L2", 1, 29.8f, BattlefieldPointType.PeekPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.02f);
            EnsureBattlefieldPoint("PeekPocket_L3", 2, 28.1f, BattlefieldPointType.PeekPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.15f);
            EnsureBattlefieldPoint("PeekPocket_L4", 3, 30.1f, BattlefieldPointType.PeekPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.02f);
            EnsureBattlefieldPoint("PeekPocket_L5", 4, 26.2f, BattlefieldPointType.PeekPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.9f);

            EnsureBattlefieldPoint("BreachPocket_L1", 0, 35.2f, BattlefieldPointType.BreachPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.9f);
            EnsureBattlefieldPoint("BreachPocket_L2", 1, 38.4f, BattlefieldPointType.BreachPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.05f);
            EnsureBattlefieldPoint("BreachPocket_L3", 2, 36.4f, BattlefieldPointType.BreachPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.15f);
            EnsureBattlefieldPoint("BreachPocket_L4", 3, 38.8f, BattlefieldPointType.BreachPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.05f);
            EnsureBattlefieldPoint("BreachPocket_L5", 4, 35.6f, BattlefieldPointType.BreachPocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 0.9f);

            EnsureBattlefieldPoint("ApproachPocket_L2", 1, 16.8f, BattlefieldPointType.ApproachPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 1.2f);
            EnsureBattlefieldPoint("ApproachPocket_L3", 2, 18.4f, BattlefieldPointType.ApproachPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 1.35f);
            EnsureBattlefieldPoint("ApproachPocket_L4", 3, 16.8f, BattlefieldPointType.ApproachPocket, BattlefieldPointUnlockRule.RequiresRecentSummon, 1.2f);

            EnsureBattlefieldPoint("ObjectivePocket_L2", 1, 45.8f, BattlefieldPointType.ObjectivePocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.25f);
            EnsureBattlefieldPoint("ObjectivePocket_L3", 2, 48.6f, BattlefieldPointType.ObjectivePocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.55f);
            EnsureBattlefieldPoint("ObjectivePocket_L4", 3, 45.8f, BattlefieldPointType.ObjectivePocket, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.25f);
            EnsureBattlefieldPoint("ObjectiveAnchor_L2", 1, 46.6f, BattlefieldPointType.ObjectiveAnchor, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.25f);
            EnsureBattlefieldPoint("ObjectiveAnchor_L3", 2, 49.6f, BattlefieldPointType.ObjectiveAnchor, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.55f);
            EnsureBattlefieldPoint("ObjectiveAnchor_L4", 3, 46.6f, BattlefieldPointType.ObjectiveAnchor, BattlefieldPointUnlockRule.RequiresAlliedPresence, 1.25f);

            EnsureBattlefieldPoint("CoreSiegePocket_L3", 2, 56.4f, BattlefieldPointType.CoreSiegePocket, BattlefieldPointUnlockRule.RequiresSiegeClear, 2.1f);
            EnsureBattlefieldPoint("AdvancePocket_L3", 2, 56.4f, BattlefieldPointType.AdvancePocket, BattlefieldPointUnlockRule.RequiresSiegeClear, 2f);
        }

        private void EnsurePrototypeBattlefieldStructures()
        {
            EnsureBattlefieldStructure(
                "FrontlineBlocker_L1",
                0,
                29.8f,
                new Vector3(1.05f, 1.4f, 1.05f),
                BattleStructureRole.FrontlineBlocker,
                148f,
                0f,
                new Color(0.9f, 0.78f, 0.36f, 1f));
            EnsureBattlefieldStructure(
                "FrontlineBlocker_L2",
                1,
                34f,
                new Vector3(1.05f, 1.4f, 1.05f),
                BattleStructureRole.FrontlineBlocker,
                164f,
                0f,
                new Color(0.54f, 0.88f, 1f, 1f));
            EnsureBattlefieldStructure(
                "FrontlineBlocker_L3",
                2,
                31.8f,
                new Vector3(1.2f, 1.55f, 1.2f),
                BattleStructureRole.FrontlineBlocker,
                232f,
                0f,
                new Color(1f, 0.72f, 0.34f, 1f));
            EnsureBattlefieldStructure(
                "FrontlineBlocker_L4",
                3,
                34.8f,
                new Vector3(1.05f, 1.4f, 1.05f),
                BattleStructureRole.FrontlineBlocker,
                164f,
                0f,
                new Color(0.54f, 0.88f, 1f, 1f));
            EnsureBattlefieldStructure(
                "FrontlineBlocker_L5",
                4,
                30.4f,
                new Vector3(1.05f, 1.4f, 1.05f),
                BattleStructureRole.FrontlineBlocker,
                148f,
                0f,
                new Color(0.9f, 0.78f, 0.36f, 1f));
            EnsureBattlefieldStructure(
                "RewardObjective_L2",
                1,
                48.2f,
                new Vector3(0.9f, 1.1f, 0.9f),
                BattleStructureRole.RewardObjective,
                96f,
                18f,
                new Color(0.4f, 1f, 0.68f, 1f));
            EnsureBattlefieldStructure(
                "SiegeObjective_L3",
                2,
                50.8f,
                new Vector3(1.35f, 1.2f, 1.35f),
                BattleStructureRole.SiegeObjective,
                238f,
                0f,
                new Color(1f, 0.5f, 0.3f, 1f));
            EnsureBattlefieldStructure(
                "RewardObjective_L4",
                3,
                48.2f,
                new Vector3(0.9f, 1.1f, 0.9f),
                BattleStructureRole.RewardObjective,
                96f,
                18f,
                new Color(0.4f, 1f, 0.68f, 1f));
        }

        private BattlefieldPoint EnsureBattlefieldPoint(
            string pointName,
            int laneIndex,
            float worldZ,
            BattlefieldPointType pointType,
            BattlefieldPointUnlockRule unlockRule,
            float priorityWeight)
        {
            return EnsureBattlefieldPoint(pointName, laneIndex, worldZ, pointType, unlockRule, priorityWeight, 0f);
        }

        private BattlefieldPoint EnsureBattlefieldPoint(
            string pointName,
            int laneIndex,
            float worldZ,
            BattlefieldPointType pointType,
            BattlefieldPointUnlockRule unlockRule,
            float priorityWeight,
            float lateralOffset)
        {
            Transform pointTransform = battlefieldLayoutRoot != null ? battlefieldLayoutRoot.Find(pointName) : null;
            if (pointTransform == null)
            {
                GameObject pointObject = new(pointName);
                pointTransform = pointObject.transform;
                pointTransform.SetParent(battlefieldLayoutRoot, false);
            }

            pointTransform.position = new Vector3(
                GetLaneCenterX(laneIndex) + lateralOffset,
                playerSpawn != null ? playerSpawn.position.y : laneAnchorVisualY,
                worldZ);
            BattlefieldPoint battlefieldPoint = pointTransform.GetComponent<BattlefieldPoint>();
            if (battlefieldPoint == null)
            {
                battlefieldPoint = pointTransform.gameObject.AddComponent<BattlefieldPoint>();
            }

            battlefieldPoint.Configure(laneIndex, pointType, unlockRule, priorityWeight);
            return battlefieldPoint;
        }

        private BattleStructure EnsureBattlefieldStructure(
            string structureName,
            int laneIndex,
            float worldZ,
            Vector3 worldScale,
            BattleStructureRole role,
            float maxHp,
            float energyReward,
            Color color)
        {
            Transform structureTransform = battlefieldLayoutRoot != null ? battlefieldLayoutRoot.Find(structureName) : null;
            if (structureTransform == null)
            {
                GameObject structureObject = GameObject.CreatePrimitive(role == BattleStructureRole.SiegeObjective ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                structureObject.name = structureName;
                structureTransform = structureObject.transform;
                structureTransform.SetParent(battlefieldLayoutRoot, false);
            }

            structureTransform.position = new Vector3(GetLaneCenterX(laneIndex), 0.8f, worldZ);
            structureTransform.localScale = worldScale;
            Renderer structureRenderer = structureTransform.GetComponent<Renderer>();
            if (structureRenderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                Material material = new(shader);
                material.color = color;
                structureRenderer.material = material;
            }

            BattleStructure structure = structureTransform.GetComponent<BattleStructure>();
            if (structure == null)
            {
                structure = structureTransform.gameObject.AddComponent<BattleStructure>();
            }

            structure.Configure(maxHp, energyReward, role);
            return structure;
        }

        private bool HasAlliedPresenceInLane(int laneIndex)
        {
            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, LaneCount);
            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit != null &&
                    summonUnit.IsAlive &&
                    summonUnit.IsPlayerTeam &&
                    summonUnit.AssignedLaneIndex == resolvedLaneIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyRuntimeBattlefieldLayout()
        {
            float centerZ = laneLength * 0.5f;
            float wallX = LaneHalfWidth + 0.35f;

            Transform corridor = GameObject.Find("Corridor")?.transform;
            if (corridor != null)
            {
                corridor.position = new Vector3(0f, 0f, centerZ);
                corridor.localScale = new Vector3(Mathf.Max(1f, laneWidth * 0.12f), 1f, Mathf.Max(2f, laneLength * 0.1f));
            }

            Transform leftWall = GameObject.Find("LeftWall")?.transform;
            if (leftWall != null)
            {
                leftWall.position = new Vector3(-wallX, 1.5f, centerZ);
                leftWall.localScale = new Vector3(0.5f, 3f, laneLength + 1.5f);
            }

            Transform rightWall = GameObject.Find("RightWall")?.transform;
            if (rightWall != null)
            {
                rightWall.position = new Vector3(wallX, 1.5f, centerZ);
                rightWall.localScale = new Vector3(0.5f, 3f, laneLength + 1.5f);
            }

            if (playerBaseTransform != null)
            {
                playerBaseTransform.position = new Vector3(0f, 0f, 0f);
                playerBaseTransform.localScale = new Vector3(Mathf.Clamp(laneWidth * 0.46f, 3f, 5.4f), 0.55f, 1.2f);
            }

            if (enemyBaseTransform != null)
            {
                enemyBaseTransform.position = new Vector3(0f, 0f, laneLength);
                enemyBaseTransform.localScale = new Vector3(Mathf.Clamp(laneWidth * 0.46f, 3f, 5.4f), 0.55f, 1.2f);
            }

            SetMarkerPosition(playerSpawn, new Vector3(0f, 0f, playerSpawnInset));
            SetMarkerPosition(enemySpawn, new Vector3(0f, 0f, laneLength - playerSpawnInset));
            SetMarkerPosition(summonSpawnPoint, new Vector3(0f, 0f, summonSpawnInset));
            SetMarkerPosition(enemySummonSpawnPoint, new Vector3(0f, 0f, laneLength - summonSpawnInset));

            Transform enemyController = GameObject.Find("EnemyController")?.transform;
            if (enemyController != null && enemySpawn != null)
            {
                enemyController.position = enemySpawn.position;
            }

            Transform projectileSpawn = GameObject.Find("ProjectileSpawn")?.transform;
            if (projectileSpawn != null && enemyController != null)
            {
                projectileSpawn.SetParent(enemyController, true);
                projectileSpawn.position = enemyController.position + new Vector3(0f, 1.5f, -0.9f);
            }

            if (playerController != null && playerSpawn != null)
            {
                playerController.transform.position = playerSpawn.position;
            }
        }

        private void ConfigurePlayerAndCamera()
        {
            EnsurePlayerSkillController();

            if (playerController != null)
            {
                float playerMinX = -laneWidth * 0.42f;
                float playerMaxX = laneWidth * 0.42f;
                playerController.ConfigureMovementBounds(playerMinX, playerMaxX, 1.4f, playerForwardLimit);
                playerController.ConfigureJustDodgeReward(18f);

                PlayerCombatController combatController = playerController.GetComponent<PlayerCombatController>();
                combatController?.ConfigureEconomyTuning(0f, 1.4f, 11f);
            }

            BattleCamera battleCamera = FindFirstObjectByType<BattleCamera>();
            if (battleCamera != null)
            {
                battleCamera.ConfigureHorizontalBounds(LaneHalfWidth);
                battleCamera.ConfigureOffset(new Vector3(0f, 7.2f, -11.8f));
                battleCamera.ConfigureLookAhead(new Vector3(0f, 1.45f, 11.6f));
            }
        }

        private void ApplyRuntimePrototypeTuning()
        {
            if (autoSpawnStructures)
            {
                autoSpawnStructures = false;
            }

            laneWidth = 12.5f;
            laneLength = 84f;
            playerSpawnInset = 4.2f;
            summonSpawnInset = 10.4f;
            runtimeStructureHP = 165f;
            runtimeStructureEnergyReward = 18f;
            centerAdvanceUnlocked = false;
            Array.Clear(friendlyLanePrimeUntil, 0, friendlyLanePrimeUntil.Length);
            Array.Clear(laneRallyUntil, 0, laneRallyUntil.Length);
            playerForwardLimit = ResolvePlayerForwardLimit();
        }

        private void HandlePerspectivePresetDebugInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                TryApplyPerspectivePreset(ProjectilePerspectivePreset.Balanced);
            }
            else if (Keyboard.current.f7Key.wasPressedThisFrame)
            {
                TryApplyPerspectivePreset(ProjectilePerspectivePreset.PathFirst);
            }
            else if (Keyboard.current.f8Key.wasPressedThisFrame)
            {
                TryApplyPerspectivePreset(ProjectilePerspectivePreset.StrikeZone);
            }
#else
            if (Input.GetKeyDown(KeyCode.F6))
            {
                TryApplyPerspectivePreset(ProjectilePerspectivePreset.Balanced);
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                TryApplyPerspectivePreset(ProjectilePerspectivePreset.PathFirst);
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                TryApplyPerspectivePreset(ProjectilePerspectivePreset.StrikeZone);
            }
#endif
        }

        private void TryApplyPerspectivePreset(ProjectilePerspectivePreset preset)
        {
            if (!EnemyProjectile.TrySetPerspectivePreset(preset))
            {
                return;
            }

            BattlePresentationController.Instance?.ShowBanner(
                "PROJECTILE READ",
                $"{EnemyProjectile.GetPerspectivePresetLabel(preset)}  F6/F7/F8",
                new Color(0.5f, 0.92f, 1f, 1f),
                0.9f);
        }

        private float ResolvePlayerForwardLimit()
        {
            float baseZoneEntryZ = Mathf.Max(playerSpawnInset + 8f, laneLength - territoryBaseZoneStart);
            float assaultDepth = Mathf.Max(4.8f, territoryBaseZoneStart * 0.52f);
            float deepestAllowedZ = laneLength - 3.6f;
            return Mathf.Clamp(baseZoneEntryZ + assaultDepth, playerSpawnInset + 16f, deepestAllowedZ);
        }

        private void UpdatePlayerTerritoryPressure()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle || playerController == null)
            {
                return;
            }

            if (Time.time < nextTerritoryPressureTime)
            {
                return;
            }

            if (!TryGetPlayerTerritoryState(out PlayerTerritoryState territoryState))
            {
                return;
            }

            if ((territoryState.OverextendDistance <= 0.01f && !territoryState.IsInEnemyBaseZone) || territoryState.PressureDamagePerTick <= 0.01f)
            {
                return;
            }

            playerController.TakeDamage(territoryState.PressureDamagePerTick);
            nextTerritoryPressureTime = Time.time + territoryPressureTickInterval;
            BattlePresentationController.Instance?.ShowWorldText(
                playerController.transform.position + new Vector3(0f, 2.2f, 0f),
                territoryState.IsInEnemyBaseZone ? "CORE FIRE" : "OVEREXTEND",
                territoryState.IsInEnemyBaseZone ? new Color(1f, 0.42f, 0.34f, 1f) : new Color(1f, 0.7f, 0.38f, 1f),
                3.8f,
                0.78f);
        }

        private void UpdateLeashDebugReport()
        {
            if (!writeLeashDebugReport ||
                playerController == null ||
                GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameState.Battle ||
                Time.time < nextLeashDebugReportWriteTime)
            {
                return;
            }

            nextLeashDebugReportWriteTime = Time.time + Mathf.Max(0.2f, leashDebugReportWriteInterval);

            try
            {
                string directory = Path.GetDirectoryName(LeashDebugReportPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                bool hasTerritoryState = TryGetPlayerTerritoryState(out PlayerTerritoryState territoryState);
                bool hasLockedTarget = playerController.TryGetManualTargetLock(out _, out int lockedLaneIndex, out ManualTargetLockKind lockedTargetKind);
                TryGetHeroLaneLeashState(
                    playerController.CurrentLaneIndex,
                    playerController.CurrentManualTargetLockKind,
                    out HeroLaneLeashState leashState);

                StringBuilder html = new();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html><head><meta charset=\"utf-8\"><meta http-equiv=\"refresh\" content=\"1\">");
                html.AppendLine("<title>IsekaiBrawl Leash Debug Report</title>");
                html.AppendLine("<style>body{font-family:Segoe UI,sans-serif;background:#08111a;color:#eaf6ff;padding:18px;}table{border-collapse:collapse;width:100%;margin-top:12px;}th,td{border:1px solid #23455f;padding:8px;text-align:left;}th{background:#102231;}tr:nth-child(even){background:#0d1823;}code{color:#9de6ff;} .warn{color:#ffd17a;} .bad{color:#ff8d7f;} .good{color:#8ef0b2;}</style></head><body>");
                html.AppendLine("<h1>Battle Leash Report</h1>");
                html.AppendLine($"<p>Updated: <code>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</code></p>");
                html.AppendLine("<h2>Current Player State</h2>");
                html.AppendLine("<ul>");
                html.AppendLine($"<li>Escort lane: <code>{playerController.EscortLaneIndex + 1}</code></li>");
                html.AppendLine($"<li>Active lane: <code>{playerController.CurrentLaneIndex + 1}</code></li>");
                html.AppendLine($"<li>Leash band: <code>{FormatLeashBand(playerController.CurrentLeashDepthBand)}</code>{(playerController.IsMovementLeashed ? " <span class=\"warn\">LEASHED</span>" : string.Empty)}</li>");
                html.AppendLine($"<li>Live allies / Phase: <code>{(playerController.CurrentLaneHasLiveAllies ? "ALLY" : "SOLO")} / {playerController.CurrentEscortPhase}</code></li>");
                html.AppendLine($"<li>Primary / Fallback Z: <code>{leashState.PrimaryAnchorZ:0.0} / {leashState.FallbackAnchorZ:0.0}</code></li>");
                html.AppendLine($"<li>Selected support anchor: <code>{playerController.CurrentSupportAnchorLabel} @ {playerController.SelectedSupportAnchorZ:0.0}</code></li>");
                html.AppendLine($"<li>Support scores: <code>{playerController.CurrentSupportAnchorScoresSummary}</code> (best <code>{playerController.CurrentSupportAnchorScore:0.00}</code>)</li>");
                html.AppendLine($"<li>Pending lane switch: <code>{(playerController.PendingLaneSwitchTarget >= 0 ? (playerController.PendingLaneSwitchTarget + 1).ToString() : "-")}</code></li>");
                html.AppendLine($"<li>Max forward Z: <code>{playerController.LeashMaxForwardZ:0.0}</code></li>");
                html.AppendLine($"<li>Intervention: <code>{playerController.CurrentInterventionReason}</code></li>");
                html.AppendLine($"<li>Movement reason: <code>{playerController.CurrentMovementReasonLabel}</code></li>");
                html.AppendLine($"<li>Retreat reason: <code>{playerController.CurrentRetreatReason}</code></li>");
                html.AppendLine($"<li>Lock: <code>{FormatLockLabel(hasLockedTarget, lockedTargetKind, lockedLaneIndex)}</code></li>");
                if (hasTerritoryState)
                {
                    html.AppendLine($"<li>Safe advance Z: <code>{territoryState.SafeAdvanceZ:0.0}</code></li>");
                    html.AppendLine($"<li>Overextend: <code>{territoryState.OverextendDistance:0.0}</code></li>");
                    html.AppendLine($"<li>Pressure per tick: <code>{territoryState.PressureDamagePerTick:0.0}</code></li>");
                }

                html.AppendLine("</ul>");
                html.AppendLine("<h2>Scenario Checklist</h2>");
                html.AppendLine("<ul>");
                html.AppendLine("<li>1. No summon: hero must stay within fallback/approach.</li>");
                html.AppendLine("<li>2. Prime only: hero may approach, but never enter choke.</li>");
                html.AppendLine("<li>3. Blocker alive: hard stop at choke/peek.</li>");
                html.AppendLine("<li>4. Blocker down plus reward alive: objective band may open.</li>");
                html.AppendLine("<li>5. Structures gone plus ally gone: hero must fall back immediately.</li>");
                html.AppendLine("<li>6. Other lane pushes deep: current lane leash still caps safe advance.</li>");
                html.AppendLine("<li>7. Boss lock without ally: lock persists, solo push does not.</li>");
                html.AppendLine("<li>8. Structure lock during ally loss: hero falls back instead of chasing.</li>");
                html.AppendLine("</ul>");
                html.AppendLine("<h2>Lane Snapshot</h2>");
                html.AppendLine("<table><thead><tr><th>Lane</th><th>Pressure</th><th>Allies</th><th>Prime</th><th>Band</th><th>Max Z</th><th>Threat</th><th>Value</th><th>Blocker</th><th>Reward</th><th>Siege</th></tr></thead><tbody>");
                for (int laneIndex = 0; laneIndex < LaneCount; laneIndex++)
                {
                    if (!TryGetLaneCombatState(laneIndex, out LaneCombatState laneState))
                    {
                        continue;
                    }

                    bool hasReward = BattleStructure.FindNearestRoleInLaneAlongAdvance(laneIndex, isPlayerTeam: true, BattleStructureRole.RewardObjective) != null;
                    bool hasSiege = BattleStructure.FindNearestRoleInLaneAlongAdvance(laneIndex, isPlayerTeam: true, BattleStructureRole.SiegeObjective) != null;
                    html.AppendLine(
                        $"<tr><td>{laneIndex + 1}</td><td>{FormatPressure(laneState.PressureState)}</td><td>{laneState.PlayerUnitCount}</td><td>{(laneState.HasRecentPrime ? "YES" : "NO")}</td><td>{FormatLeashBand(laneState.MaxDepthBand)}</td><td>{laneState.MaxForwardZ:0.0}</td><td>{laneState.LaneThreatScore:0.00}</td><td>{laneState.LaneValueScore:0.00}</td><td>{(laneState.HasFrontlineStructure ? "UP" : "DOWN")}</td><td>{(hasReward ? "UP" : "DOWN")}</td><td>{(hasSiege ? "UP" : "DOWN")}</td></tr>");
                }

                html.AppendLine("</tbody></table></body></html>");
                File.WriteAllText(LeashDebugReportPath, html.ToString(), Encoding.UTF8);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to write leash debug report: {exception.Message}");
            }
        }

        private float ResolveTerritoryPressureDamage(float overextendDistance, bool isInEnemyBaseZone)
        {
            float damage = territoryPressureBaseDamage + (Mathf.Max(0f, overextendDistance) * territoryPressureDepthDamage);
            if (isInEnemyBaseZone)
            {
                damage += territoryBaseZoneDamageBonus;
            }

            return damage;
        }

        private static string FormatPressure(LanePressureState pressureState)
        {
            return pressureState switch
            {
                LanePressureState.Push => "PUSH",
                LanePressureState.Contest => "CONTEST",
                LanePressureState.Collapse => "COLLAPSE",
                _ => "EMPTY"
            };
        }

        private static string FormatLeashBand(HeroLaneDepthBand depthBand)
        {
            return depthBand switch
            {
                HeroLaneDepthBand.Fallback => "FALLBACK",
                HeroLaneDepthBand.Choke => "CHOKE",
                HeroLaneDepthBand.Objective => "OBJECTIVE",
                HeroLaneDepthBand.Advance => "ADVANCE",
                _ => "APPROACH"
            };
        }

        private static string FormatLockLabel(bool hasLockedTarget, ManualTargetLockKind lockedTargetKind, int lockedLaneIndex)
        {
            if (!hasLockedTarget)
            {
                return "NONE";
            }

            return lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => $"BOSS@{lockedLaneIndex + 1}",
                ManualTargetLockKind.Structure => $"STRUCT@{lockedLaneIndex + 1}",
                _ => $"ENEMY@{lockedLaneIndex + 1}"
            };
        }

        private void UpdateCachedTerritoryState()
        {
            if (territoryStateFrame == Time.frameCount)
            {
                return;
            }

            territoryStateFrame = Time.frameCount;
            hasCachedTerritoryState = TryBuildTerritoryState(out cachedTerritoryState);
        }

        private bool TryBuildTerritoryState(out PlayerTerritoryState state)
        {
            state = default;
            if (playerController == null)
            {
                hasCachedTerritoryState = false;
                return false;
            }

            float frontlineZ = playerSpawnInset + 3.5f;
            int alliedUnitCount = 0;
            if (TryGetFrontlineState(out FrontlineState frontlineState))
            {
                frontlineZ = Mathf.Max(frontlineZ, frontlineState.PlayerFrontZ);
                alliedUnitCount = frontlineState.PlayerUnitCount;
            }

            float safeLead = 7.2f + (Mathf.Min(alliedUnitCount, 5) * 0.75f);
            float safeAdvanceZ = frontlineZ + safeLead;
            if (playerController != null &&
                TryGetHeroLaneLeashState(playerController.CurrentLaneIndex, playerController.CurrentManualTargetLockKind, out HeroLaneLeashState leashState))
            {
                float leashSafeAdvanceZ = leashState.MaxForwardZ + 0.65f;
                safeAdvanceZ = Mathf.Min(safeAdvanceZ, leashSafeAdvanceZ);
            }
            float playerZ = playerController.transform.position.z;
            float overextendDistance = Mathf.Max(0f, playerZ - safeAdvanceZ);
            float enemyBaseDistance = enemyBaseTransform != null
                ? enemyBaseTransform.position.z - playerZ
                : float.MaxValue;
            bool isInEnemyBaseZone = enemyBaseDistance <= territoryBaseZoneStart;
            float warningDistance = Mathf.Max(0f, safeAdvanceZ - playerZ);
            float safeAdvanceRetreatDistance = float.IsNaN(lastSafeAdvanceZ)
                ? 0f
                : Mathf.Max(0f, lastSafeAdvanceZ - safeAdvanceZ);
            bool isOverextended = overextendDistance > 0.01f;

            if (!isOverextended)
            {
                overextendExposureStartTime = -1f;
                coverBreakGraceUntilTime = 0f;
            }
            else if (!isInEnemyBaseZone)
            {
                bool lineBrokeUnderPlayer = !float.IsNaN(lastSafeAdvanceZ)
                    && safeAdvanceRetreatDistance >= territoryCoverBreakRetreatThreshold
                    && lastSafeAdvanceZ + territoryCoverBreakPlayerMargin >= playerZ;

                if (!lastTerritoryWasOverextended)
                {
                    if (lineBrokeUnderPlayer)
                    {
                        coverBreakGraceUntilTime = Time.time + territoryCoverBreakGraceDuration;
                        overextendExposureStartTime = coverBreakGraceUntilTime;
                    }
                    else
                    {
                        coverBreakGraceUntilTime = 0f;
                        overextendExposureStartTime = Time.time;
                    }
                }
                else if (lineBrokeUnderPlayer && Time.time < coverBreakGraceUntilTime + territoryPressureRampDuration)
                {
                    float extendedGraceUntil = Time.time + (territoryCoverBreakGraceDuration * 0.55f);
                    if (extendedGraceUntil > coverBreakGraceUntilTime)
                    {
                        coverBreakGraceUntilTime = extendedGraceUntil;
                        overextendExposureStartTime = Mathf.Max(overextendExposureStartTime, coverBreakGraceUntilTime);
                    }
                }
            }
            else
            {
                coverBreakGraceUntilTime = 0f;
                overextendExposureStartTime = Time.time;
            }

            bool isInCoverBreakGrace = !isInEnemyBaseZone && isOverextended && Time.time < coverBreakGraceUntilTime;
            float pressureMultiplier = 0f;
            if (isInEnemyBaseZone)
            {
                pressureMultiplier = 1f;
            }
            else if (isOverextended)
            {
                pressureMultiplier = isInCoverBreakGrace
                    ? 0f
                    : ResolveTerritoryPressureMultiplier();
            }

            float pressureDamage = ResolveTerritoryPressureDamage(overextendDistance, isInEnemyBaseZone);
            if (!isInEnemyBaseZone)
            {
                pressureDamage *= pressureMultiplier;
            }

            state = new PlayerTerritoryState(
                safeAdvanceZ,
                overextendDistance,
                enemyBaseDistance,
                isInEnemyBaseZone,
                alliedUnitCount,
                pressureDamage,
                warningDistance,
                pressureMultiplier,
                isInCoverBreakGrace,
                Mathf.Max(0f, coverBreakGraceUntilTime - Time.time),
                safeAdvanceRetreatDistance);

            lastSafeAdvanceZ = safeAdvanceZ;
            lastTerritoryWasOverextended = isOverextended;
            return true;
        }

        private float ResolveTerritoryPressureMultiplier()
        {
            if (territoryPressureRampDuration <= 0.01f || overextendExposureStartTime < 0f)
            {
                return 1f;
            }

            float exposureElapsed = Mathf.Max(0f, Time.time - overextendExposureStartTime);
            float rampT = Mathf.Clamp01(exposureElapsed / territoryPressureRampDuration);
            return Mathf.Lerp(territoryPressureRampStartMultiplier, 1f, rampT);
        }

        private void UpdateTerritoryGuides()
        {
            if (playerController == null)
            {
                return;
            }

            EnsureLaneGuides();
            if (!TryGetPlayerTerritoryState(out PlayerTerritoryState territoryState))
            {
                return;
            }

            Transform safeGuide = EnsureHorizontalGuide("SafeAdvanceGuide");
            Transform baseGuide = EnsureHorizontalGuide("BaseZoneGuide");
            float baseZoneZ = Mathf.Max(playerSpawnInset + 8f, laneLength - territoryBaseZoneStart);
            Color overextendGuideColor = territoryState.IsInCoverBreakGrace
                ? Color.Lerp(safeAdvanceWarningColor, Color.white, 0.2f + (Mathf.Sin(Time.time * 11f) * 0.12f))
                : Color.Lerp(safeAdvanceWarningColor, safeAdvanceDangerColor, territoryState.PressureRampMultiplier01);

            ConfigureHorizontalGuide(
                safeGuide,
                territoryState.SafeAdvanceZ,
                territoryState.IsInEnemyBaseZone
                    ? safeAdvanceDangerColor
                    : territoryState.IsInCoverBreakGrace
                        ? overextendGuideColor
                    : territoryState.OverextendDistance > 0.01f
                        ? overextendGuideColor
                        : territoryState.WarningDistance <= territoryWarningLeadDistance
                            ? safeAdvanceWarningColor
                            : safeAdvanceGuideColor,
                thickness: territoryState.IsInCoverBreakGrace
                    ? 0.19f
                    : territoryState.WarningDistance <= territoryWarningLeadDistance || territoryState.OverextendDistance > 0.01f ? 0.16f : 0.12f);
            ConfigureHorizontalGuide(
                baseGuide,
                baseZoneZ,
                territoryState.IsInEnemyBaseZone
                    ? Color.Lerp(baseZoneGuideColor, Color.white, 0.18f + (Mathf.Sin(Time.time * 10f) * 0.08f))
                    : baseZoneGuideColor,
                thickness: territoryState.IsInEnemyBaseZone ? 0.2f : 0.14f);
        }

        private void UpdateTerritoryWarnings()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle || playerController == null)
            {
                wasOverextended = false;
                wasInCoverBreakGrace = false;
                wasInEnemyBaseZone = false;
                return;
            }

            if (!TryGetPlayerTerritoryState(out PlayerTerritoryState territoryState))
            {
                return;
            }

            bool isOverextended = territoryState.OverextendDistance > 0.01f;
            if (territoryState.IsInCoverBreakGrace && !wasInCoverBreakGrace)
            {
                BattlePresentationController.Instance?.ShowWorldText(
                    playerController.transform.position + new Vector3(0f, 2.5f, 0f),
                    "LINE BROKE",
                    safeAdvanceWarningColor,
                    3.8f,
                    0.82f);
            }
            else if (isOverextended && !wasOverextended)
            {
                BattlePresentationController.Instance?.ShowWorldText(
                    playerController.transform.position + new Vector3(0f, 2.4f, 0f),
                    "LEAVING COVER",
                    safeAdvanceWarningColor,
                    3.6f,
                    0.76f);
            }

            if (territoryState.IsInEnemyBaseZone && !wasInEnemyBaseZone)
            {
                BattlePresentationController.Instance?.ShowWorldText(
                    playerController.transform.position + new Vector3(0f, 2.55f, 0f),
                    "CORE FIRE ZONE",
                    baseZoneGuideColor,
                    3.9f,
                    0.84f);
            }

            wasOverextended = isOverextended;
            wasInCoverBreakGrace = territoryState.IsInCoverBreakGrace;
            wasInEnemyBaseZone = territoryState.IsInEnemyBaseZone;
        }

        private void NotifyBaseHPChanged()
        {
            OnPlayerBaseHPChanged?.Invoke(CurrentPlayerBaseHP);
            OnEnemyBaseHPChanged?.Invoke(CurrentEnemyBaseHP);
        }

        private void EnsurePlayerSkillController()
        {
            if (playerController == null)
            {
                return;
            }

            if (playerController.GetComponent<PlayerSkillController>() == null)
            {
                playerController.gameObject.AddComponent<PlayerSkillController>();
            }

            if (playerController.GetComponent<PlayerCombatController>() == null)
            {
                playerController.gameObject.AddComponent<PlayerCombatController>();
            }
        }

        private void EnsureLaneAnchorSets()
        {
            if (laneAnchorRoot == null)
            {
                GameObject existingRoot = GameObject.Find("LaneAnchorSets");
                if (existingRoot == null && allowRuntimePrototypeBootstrap)
                {
                    existingRoot = new GameObject("LaneAnchorSets");
                }

                laneAnchorRoot = existingRoot != null ? existingRoot.transform : null;
            }

            if (laneAnchorRoot == null)
            {
                cachedLaneAnchorSets = Array.Empty<LaneAnchorSet>();
                return;
            }

            LaneAnchorSet[] discoveredSets = laneAnchorRoot.GetComponentsInChildren<LaneAnchorSet>(true);
            cachedLaneAnchorSets = new LaneAnchorSet[LaneCount];
            for (int index = 0; index < discoveredSets.Length; index++)
            {
                LaneAnchorSet discoveredSet = discoveredSets[index];
                if (discoveredSet == null)
                {
                    continue;
                }

                int laneIndex = BattleLaneUtility.ClampLaneIndex(discoveredSet.LaneIndex, LaneCount);
                if (cachedLaneAnchorSets[laneIndex] == null)
                {
                    cachedLaneAnchorSets[laneIndex] = discoveredSet;
                }
            }

            for (int laneIndex = 0; laneIndex < LaneCount; laneIndex++)
            {
                if (cachedLaneAnchorSets[laneIndex] != null)
                {
                    continue;
                }

                if (allowRuntimePrototypeBootstrap)
                {
                    cachedLaneAnchorSets[laneIndex] = CreateRuntimeLaneAnchorSet(laneIndex);
                }
            }
        }

        private void LayoutLaneAnchorSets()
        {
            if (cachedLaneAnchorSets == null || cachedLaneAnchorSets.Length == 0)
            {
                return;
            }

            float anchorY = playerSpawn != null ? playerSpawn.position.y : laneAnchorVisualY;

            for (int laneIndex = 0; laneIndex < cachedLaneAnchorSets.Length; laneIndex++)
            {
                LaneAnchorSet anchorSet = cachedLaneAnchorSets[laneIndex];
                if (anchorSet == null)
                {
                    continue;
                }

                float laneX = GetLaneCenterX(laneIndex);
                BattlefieldPoint fallbackPoint = FindHighestPriorityLanePoint(laneIndex, BattlefieldPointType.FallbackPocket, BattlefieldPointType.ReadyPocket);
                BattlefieldPoint approachPoint = FindClosestLanePoint(laneIndex, BattlefieldPointType.JoinPocket, BattlefieldPointType.ApproachPocket, isPlayerTeam: true, summonSpawnInset);
                BattlefieldPoint blockerHoldPoint = FindHighestPriorityLanePoint(laneIndex, BattlefieldPointType.BlockerHoldPocket, BattlefieldPointType.ObjectiveAnchor);
                BattlefieldPoint breachPoint = FindHighestPriorityLanePoint(laneIndex, BattlefieldPointType.BreachPocket, BattlefieldPointType.ObjectiveAnchor);
                BattlefieldPoint objectivePoint = FindHighestPriorityLanePoint(laneIndex, BattlefieldPointType.ObjectivePocket, BattlefieldPointType.ObjectiveAnchor);
                BattlefieldPoint advancePoint = FindHighestPriorityLanePoint(laneIndex, BattlefieldPointType.CoreSiegePocket, BattlefieldPointType.AdvancePocket);
                BattleStructure blocker = BattleStructure.FindNearestRoleInLaneAlongAdvance(
                    laneIndex,
                    isPlayerTeam: true,
                    BattleStructureRole.FrontlineBlocker);
                BattleStructure nearestStructure = BattleStructure.FindNearestActiveInLaneAlongAdvance(laneIndex, isPlayerTeam: true);

                float rearZ = fallbackPoint != null
                    ? fallbackPoint.transform.position.z
                    : Mathf.Clamp(laneRearSlotZ, playerLaneMinZ + 0.5f, playerForwardLimit - 2f);
                float supportZ = approachPoint != null
                    ? approachPoint.transform.position.z
                    : Mathf.Clamp(rearZ + 2.6f, rearZ + 0.8f, playerForwardLimit - 1.8f);
                if (fallbackPoint == null)
                {
                    supportZ = Mathf.Max(supportZ, laneSupportBaseZ);
                }

                float peekTargetZ = blocker != null
                    ? blockerHoldPoint != null
                        ? blockerHoldPoint.transform.position.z
                        : blocker.transform.position.z - lanePeekObjectiveClearance
                    : breachPoint != null
                        ? breachPoint.transform.position.z
                        : objectivePoint != null
                            ? objectivePoint.transform.position.z
                        : nearestStructure != null
                            ? nearestStructure.transform.position.z - lanePeekObjectiveClearance
                            : supportZ + 2.4f;
                float peekZ = Mathf.Clamp(peekTargetZ, supportZ + 0.45f, playerForwardLimit - 0.95f);

                float advanceTargetZ = advancePoint != null && centerAdvanceUnlocked
                    ? advancePoint.transform.position.z
                    : objectivePoint != null
                        ? objectivePoint.transform.position.z
                        : nearestStructure != null
                            ? nearestStructure.transform.position.z - laneAdvanceObjectiveClearance
                            : peekZ + 0.8f;
                float advanceZ = Mathf.Clamp(Mathf.Max(peekZ + 0.45f, advanceTargetZ), peekZ + 0.45f, playerForwardLimit - 0.55f);

                SetAnchorLocalPosition(anchorSet.RearAnchor, laneX, anchorY, rearZ);
                SetAnchorLocalPosition(anchorSet.SupportCoverAnchor, laneX, anchorY, supportZ);
                SetAnchorLocalPosition(anchorSet.PeekAnchor, laneX, anchorY, peekZ);
                SetAnchorLocalPosition(anchorSet.AdvanceBaseAnchor, laneX, anchorY, advanceZ);
                anchorSet.name = $"LaneAnchorSet_{laneIndex + 1}";
            }
        }

        private LaneAnchorSet CreateRuntimeLaneAnchorSet(int laneIndex)
        {
            GameObject rootObject = new($"LaneAnchorSet_{laneIndex + 1}");
            rootObject.transform.SetParent(laneAnchorRoot, false);

            Transform rearAnchor = CreateLaneAnchorChild(rootObject.transform, "Rear");
            Transform supportAnchor = CreateLaneAnchorChild(rootObject.transform, "SupportCover");
            Transform peekAnchor = CreateLaneAnchorChild(rootObject.transform, "Peek");
            Transform advanceAnchor = CreateLaneAnchorChild(rootObject.transform, "AdvanceBase");

            LaneAnchorSet anchorSet = rootObject.AddComponent<LaneAnchorSet>();
            anchorSet.Configure(laneIndex, rearAnchor, supportAnchor, peekAnchor, advanceAnchor);
            return anchorSet;
        }

        private static Transform CreateLaneAnchorChild(Transform parent, string childName)
        {
            GameObject childObject = new(childName);
            childObject.transform.SetParent(parent, false);
            return childObject.transform;
        }

        private static void SetAnchorLocalPosition(Transform anchor, float worldX, float worldY, float worldZ)
        {
            if (anchor == null)
            {
                return;
            }

            anchor.position = new Vector3(worldX, worldY, worldZ);
        }

        private void EnsureBattleStructures()
        {
            // Scene-authored battlefield layout is now the source of truth.
        }

        private void LayoutBattleStructures()
        {
            // Scene-authored battlefield layout is now the source of truth.
        }

        private Vector3[] GetStructurePositions()
        {
            Vector3[] positions = new Vector3[LaneCount];
            float structureZ = laneLength * Mathf.Clamp01(runtimeFrontlineStructureNormalizedZ);
            for (int laneIndex = 0; laneIndex < positions.Length; laneIndex++)
            {
                positions[laneIndex] = new Vector3(GetLaneCenterX(laneIndex), 0.8f, structureZ);
            }

            return positions;
        }

        private void CreateRuntimeStructure(int index, Vector3 position)
        {
            GameObject structureObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            structureObject.name = $"FrontlineStructure_Lane{index + 1}";
            structureObject.transform.position = position;
            structureObject.transform.localScale = new Vector3(0.95f, 0.8f, 0.95f);

            Renderer structureRenderer = structureObject.GetComponent<Renderer>();
            if (structureRenderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                Material material = new(shader);
                material.color = index % 2 == 0
                    ? new Color(0.95f, 0.78f, 0.38f, 1f)
                    : new Color(0.58f, 0.9f, 1f, 1f);
                structureRenderer.material = material;
            }

            BattleStructure structure = structureObject.AddComponent<BattleStructure>();
            structure.Configure(runtimeStructureHP, runtimeStructureEnergyReward, BattleStructureRole.FrontlineBlocker);
        }

        private static BattleStructure[] GetOrderedBattleStructures()
        {
            BattleStructure[] structures = FindObjectsByType<BattleStructure>(FindObjectsSortMode.None);
            Array.Sort(structures, (left, right) =>
            {
                if (left == null && right == null)
                {
                    return 0;
                }

                if (left == null)
                {
                    return 1;
                }

                if (right == null)
                {
                    return -1;
                }

                return string.CompareOrdinal(left.name, right.name);
            });
            return structures;
        }

        private void EnsureLaneGuides()
        {
            if (laneGuideRoot == null)
            {
                GameObject rootObject = GameObject.Find("LaneGuides");
                if (rootObject == null && allowRuntimePrototypeBootstrap)
                {
                    rootObject = new GameObject("LaneGuides");
                }

                laneGuideRoot = rootObject != null ? rootObject.transform : null;
            }

            if (laneGuideRoot == null)
            {
                return;
            }

            float[] laneAnchors = GetProjectileLaneAnchors();
            for (int index = 0; index < laneAnchors.Length; index++)
            {
                Transform guide = laneGuideRoot.Find($"LaneGuide_{index}");
                if (guide == null)
                {
                    if (!allowRuntimePrototypeBootstrap)
                    {
                        continue;
                    }

                    GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    guideObject.name = $"LaneGuide_{index}";
                    Destroy(guideObject.GetComponent<Collider>());
                    guideObject.transform.SetParent(laneGuideRoot, false);
                    guide = guideObject.transform;
                }

                guide.position = new Vector3(laneAnchors[index], 0.03f, laneLength * 0.5f);
                guide.localScale = new Vector3(index == 2 ? 0.2f : 0.14f, 0.03f, laneLength * 0.92f);

                Renderer guideRenderer = guide.GetComponent<Renderer>();
                if (guideRenderer != null)
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    if (guideRenderer.material == null || guideRenderer.material.shader != shader)
                    {
                        guideRenderer.material = new Material(shader);
                    }

                    guideRenderer.material.color = index == 2 ? centerGuideColor : laneGuideColor;
                }
            }
        }

        private Transform EnsureHorizontalGuide(string name)
        {
            Transform guide = laneGuideRoot != null ? laneGuideRoot.Find(name) : null;
            if (guide != null)
            {
                return guide;
            }

            if (laneGuideRoot == null || !allowRuntimePrototypeBootstrap)
            {
                return null;
            }

            GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guideObject.name = name;
            Destroy(guideObject.GetComponent<Collider>());
            guideObject.transform.SetParent(laneGuideRoot, false);
            return guideObject.transform;
        }

        private void ConfigureHorizontalGuide(Transform guide, float zPosition, Color color, float thickness)
        {
            if (guide == null)
            {
                return;
            }

            guide.position = new Vector3(0f, 0.035f, Mathf.Clamp(zPosition, 0.5f, laneLength - 0.5f));
            guide.localScale = new Vector3(laneWidth * 0.86f, 0.03f, thickness);

            Renderer guideRenderer = guide.GetComponent<Renderer>();
            if (guideRenderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (guideRenderer.material == null || guideRenderer.material.shader != shader)
            {
                guideRenderer.material = new Material(shader);
            }

            guideRenderer.material.color = color;
        }

        private float[] GetProjectileLaneAnchors()
        {
            float outerLane = LaneHalfWidth * 0.72f;
            float innerLane = LaneHalfWidth * 0.34f;
            return new[]
            {
                -outerLane,
                -innerLane,
                0f,
                innerLane,
                outerLane
            };
        }

        private void CacheBaseVisuals()
        {
            CacheVisualSet(playerBaseTransform, out playerBaseRenderers, out playerBaseColors);
            CacheVisualSet(enemyBaseTransform, out enemyBaseRenderers, out enemyBaseColors);
        }

        private static void CacheVisualSet(Transform root, out Renderer[] renderers, out Color[] colors)
        {
            renderers = root != null ? root.GetComponentsInChildren<Renderer>(true) : Array.Empty<Renderer>();
            colors = new Color[renderers.Length];

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                colors[index] = renderer != null && renderer.material.HasProperty("_Color")
                    ? renderer.material.color
                    : Color.white;
            }
        }

        private void PlayPlayerBaseFeedback()
        {
            if (playerBaseFlashRoutine != null)
            {
                StopCoroutine(playerBaseFlashRoutine);
            }

            playerBaseFlashRoutine = StartCoroutine(BaseFlashRoutine(playerBaseRenderers, playerBaseColors, playerBaseHitColor));
        }

        private void PlayEnemyBaseFeedback()
        {
            if (enemyBaseFlashRoutine != null)
            {
                StopCoroutine(enemyBaseFlashRoutine);
            }

            enemyBaseFlashRoutine = StartCoroutine(BaseFlashRoutine(enemyBaseRenderers, enemyBaseColors, enemyBaseHitColor));
        }

        private IEnumerator BaseFlashRoutine(Renderer[] renderers, Color[] baseColors, Color flashColor)
        {
            CameraShake.Instance?.PlayShake(0.1f, 0.12f);
            SetRendererColors(renderers, flashColor);
            yield return new WaitForSeconds(baseFlashDuration);
            RestoreRendererColors(renderers, baseColors);
        }

        private static void SetRendererColors(Renderer[] renderers, Color color)
        {
            if (renderers == null)
            {
                return;
            }

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || !renderer.material.HasProperty("_Color"))
                {
                    continue;
                }

                renderer.material.color = color;
            }
        }

        private static void RestoreRendererColors(Renderer[] renderers, Color[] baseColors)
        {
            if (renderers == null || baseColors == null)
            {
                return;
            }

            int length = Mathf.Min(renderers.Length, baseColors.Length);
            for (int index = 0; index < length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || !renderer.material.HasProperty("_Color"))
                {
                    continue;
                }

                renderer.material.color = baseColors[index];
            }
        }

        private static void SetMarkerPosition(Transform marker, Vector3 position)
        {
            if (marker != null)
            {
                marker.position = position;
            }
        }

        public readonly struct FrontlineState
        {
            public FrontlineState(float balance, float playerFrontZ, float enemyFrontZ, int playerUnitCount, int enemyUnitCount, float clashCenterNormalized)
            {
                Balance = balance;
                PlayerFrontZ = playerFrontZ;
                EnemyFrontZ = enemyFrontZ;
                PlayerUnitCount = playerUnitCount;
                EnemyUnitCount = enemyUnitCount;
                ClashCenterNormalized = clashCenterNormalized;
            }

            public float Balance { get; }
            public float PlayerFrontZ { get; }
            public float EnemyFrontZ { get; }
            public int PlayerUnitCount { get; }
            public int EnemyUnitCount { get; }
            public float ClashCenterNormalized { get; }
        }

        public enum LanePressureState
        {
            Empty = 0,
            Push = 1,
            Contest = 2,
            Collapse = 3
        }

        public enum PlayerLaneSlot
        {
            Rear = 0,
            SupportCover = 1,
            Peek = 2,
            Advance = 3
        }

        public enum CoverState
        {
            None = 0,
            SoftCover = 1,
            Exposed = 2
        }

        public enum SummonLanePreviewState
        {
            Stall = 0,
            Break = 1,
            Reward = 2
        }

        public enum HeroLaneDepthBand
        {
            Fallback = 0,
            Approach = 1,
            Choke = 2,
            Objective = 3,
            Advance = 4
        }

        public enum EscortPhase
        {
            Ready = 0,
            Join = 1,
            BlockerHold = 2,
            Breach = 3,
            Objective = 4,
            Fallback = 5
        }

        public readonly struct HeroLaneLeashState
        {
            public HeroLaneLeashState(
                bool hasLiveAllies,
                bool hasRecentPrime,
                EscortPhase escortPhase,
                HeroLaneDepthBand maxDepthBand,
                float maxForwardZ,
                Vector3 primaryAnchor,
                Vector3 fallbackAnchor,
                HeroInterventionReason interventionReason)
            {
                HasLiveAllies = hasLiveAllies;
                HasRecentPrime = hasRecentPrime;
                EscortPhase = escortPhase;
                MaxDepthBand = maxDepthBand;
                MaxForwardZ = maxForwardZ;
                PrimaryAnchor = primaryAnchor;
                FallbackAnchor = fallbackAnchor;
                InterventionReason = interventionReason;
            }

            public bool HasLiveAllies { get; }
            public bool HasRecentPrime { get; }
            public EscortPhase EscortPhase { get; }
            public HeroLaneDepthBand MaxDepthBand { get; }
            public float MaxForwardZ { get; }
            public Vector3 PrimaryAnchor { get; }
            public Vector3 FallbackAnchor { get; }
            public HeroInterventionReason InterventionReason { get; }
            public float PrimaryAnchorZ => PrimaryAnchor.z;
            public float FallbackAnchorZ => FallbackAnchor.z;
        }

        public readonly struct HeroLaneContext
        {
            public HeroLaneContext(
                int laneIndex,
                int allyCount,
                int enemyCount,
                LanePressureState pressureState,
                bool hasBlocker,
                bool hasObjective,
                float playerFrontZ,
                float enemyFrontZ,
                Vector3 joinAnchor,
                Vector3[] supportAnchors,
                Vector3 peekAnchor,
                Vector3 objectiveAnchor,
                Vector3 fallbackAnchor,
                float laneThreatScore,
                float laneValueScore,
                bool hasRecentPrime,
                EscortPhase escortPhase,
                float supportEnvelopeMinZ,
                float supportEnvelopeMaxZ,
                bool canOpenPeek)
            {
                LaneIndex = laneIndex;
                AllyCount = allyCount;
                EnemyCount = enemyCount;
                PressureState = pressureState;
                HasBlocker = hasBlocker;
                HasObjective = hasObjective;
                PlayerFrontZ = playerFrontZ;
                EnemyFrontZ = enemyFrontZ;
                JoinAnchor = joinAnchor;
                SupportAnchors = supportAnchors;
                PeekAnchor = peekAnchor;
                ObjectiveAnchor = objectiveAnchor;
                FallbackAnchor = fallbackAnchor;
                LaneThreatScore = laneThreatScore;
                LaneValueScore = laneValueScore;
                HasRecentPrime = hasRecentPrime;
                EscortPhase = escortPhase;
                SupportEnvelopeMinZ = supportEnvelopeMinZ;
                SupportEnvelopeMaxZ = supportEnvelopeMaxZ;
                CanOpenPeek = canOpenPeek;
            }

            public int LaneIndex { get; }
            public int AllyCount { get; }
            public int EnemyCount { get; }
            public LanePressureState PressureState { get; }
            public bool HasBlocker { get; }
            public bool HasObjective { get; }
            public float PlayerFrontZ { get; }
            public float EnemyFrontZ { get; }
            public Vector3 JoinAnchor { get; }
            public Vector3[] SupportAnchors { get; }
            public Vector3 PeekAnchor { get; }
            public Vector3 ObjectiveAnchor { get; }
            public Vector3 FallbackAnchor { get; }
            public float LaneThreatScore { get; }
            public float LaneValueScore { get; }
            public bool HasRecentPrime { get; }
            public EscortPhase EscortPhase { get; }
            public float SupportEnvelopeMinZ { get; }
            public float SupportEnvelopeMaxZ { get; }
            public bool CanOpenPeek { get; }
        }

        public readonly struct LaneCombatState
        {
            public LaneCombatState(
                int laneIndex,
                LanePressureState pressureState,
                PlayerLaneSlot suggestedSlot,
                CoverState suggestedCoverState,
                bool hasFrontlineStructure,
                float frontlineStructureZ,
                bool hasFrontlineObjective,
                float frontlineObjectiveZ,
                float laneFrontZ,
                float playerFrontZ,
                float enemyFrontZ,
                int playerUnitCount,
                int enemyUnitCount,
                float clashCenterNormalized,
                Vector3 joinAnchor,
                Vector3 supportAnchor,
                Vector3 supportLeftAnchor,
                Vector3 supportCenterAnchor,
                Vector3 supportRightAnchor,
                Vector3 peekAnchor,
                Vector3 advanceAnchor,
                Vector3 retreatAnchor)
                : this(
                    laneIndex,
                    pressureState,
                    suggestedSlot,
                    suggestedCoverState,
                    hasFrontlineStructure,
                    frontlineStructureZ,
                    hasFrontlineObjective,
                    frontlineObjectiveZ,
                    laneFrontZ,
                    playerFrontZ,
                    enemyFrontZ,
                    playerUnitCount,
                    enemyUnitCount,
                    clashCenterNormalized,
                    joinAnchor,
                    supportAnchor,
                    supportLeftAnchor,
                    supportCenterAnchor,
                    supportRightAnchor,
                    peekAnchor,
                    advanceAnchor,
                    retreatAnchor,
                    advanceAnchor,
                    retreatAnchor,
                    playerUnitCount > 0,
                    false,
                    EscortPhase.Objective,
                    HeroLaneDepthBand.Advance,
                    advanceAnchor.z,
                    supportAnchor.z,
                    supportAnchor.z,
                    true,
                    HeroInterventionReason.Escort,
                    0f,
                    0f)
            {
            }

            public LaneCombatState(
                int laneIndex,
                LanePressureState pressureState,
                PlayerLaneSlot suggestedSlot,
                CoverState suggestedCoverState,
                bool hasFrontlineStructure,
                float frontlineStructureZ,
                bool hasFrontlineObjective,
                float frontlineObjectiveZ,
                float laneFrontZ,
                float playerFrontZ,
                float enemyFrontZ,
                int playerUnitCount,
                int enemyUnitCount,
                float clashCenterNormalized,
                Vector3 joinAnchor,
                Vector3 supportAnchor,
                Vector3 supportLeftAnchor,
                Vector3 supportCenterAnchor,
                Vector3 supportRightAnchor,
                Vector3 peekAnchor,
                Vector3 advanceAnchor,
                Vector3 retreatAnchor,
                Vector3 primaryAnchor,
                Vector3 fallbackAnchor,
                bool hasLiveAllies,
                bool hasRecentPrime,
                EscortPhase escortPhase,
                HeroLaneDepthBand maxDepthBand,
                float maxForwardZ,
                float supportEnvelopeMinZ,
                float supportEnvelopeMaxZ,
                bool canOpenPeek,
                HeroInterventionReason interventionReason,
                float laneThreatScore,
                float laneValueScore)
            {
                LaneIndex = laneIndex;
                PressureState = pressureState;
                SuggestedPlayerSlot = suggestedSlot;
                SuggestedCoverState = suggestedCoverState;
                HasFrontlineStructure = hasFrontlineStructure;
                FrontlineStructureZ = frontlineStructureZ;
                HasFrontlineObjective = hasFrontlineObjective;
                FrontlineObjectiveZ = frontlineObjectiveZ;
                LaneFrontZ = laneFrontZ;
                PlayerFrontZ = playerFrontZ;
                EnemyFrontZ = enemyFrontZ;
                PlayerUnitCount = playerUnitCount;
                EnemyUnitCount = enemyUnitCount;
                ClashCenterNormalized = clashCenterNormalized;
                JoinAnchor = joinAnchor;
                SupportAnchor = supportAnchor;
                SupportAnchorLeft = supportLeftAnchor;
                SupportAnchorCenter = supportCenterAnchor;
                SupportAnchorRight = supportRightAnchor;
                PeekAnchor = peekAnchor;
                AdvanceAnchor = advanceAnchor;
                RetreatAnchor = retreatAnchor;
                PrimaryAnchor = primaryAnchor;
                FallbackAnchor = fallbackAnchor;
                HasLiveAllies = hasLiveAllies;
                HasRecentPrime = hasRecentPrime;
                EscortPhase = escortPhase;
                MaxDepthBand = maxDepthBand;
                MaxForwardZ = maxForwardZ;
                SupportEnvelopeMinZ = supportEnvelopeMinZ;
                SupportEnvelopeMaxZ = supportEnvelopeMaxZ;
                CanOpenPeek = canOpenPeek;
                InterventionReason = interventionReason;
                LaneThreatScore = laneThreatScore;
                LaneValueScore = laneValueScore;
            }

            public int LaneIndex { get; }
            public LanePressureState PressureState { get; }
            public PlayerLaneSlot SuggestedPlayerSlot { get; }
            public CoverState SuggestedCoverState { get; }
            public bool HasFrontlineStructure { get; }
            public float FrontlineStructureZ { get; }
            public bool HasFrontlineObjective { get; }
            public float FrontlineObjectiveZ { get; }
            public float LaneFrontZ { get; }
            public float PlayerFrontZ { get; }
            public float EnemyFrontZ { get; }
            public int PlayerUnitCount { get; }
            public int EnemyUnitCount { get; }
            public float ClashCenterNormalized { get; }
            public Vector3 JoinAnchor { get; }
            public Vector3 SupportAnchor { get; }
            public Vector3 SupportAnchorLeft { get; }
            public Vector3 SupportAnchorCenter { get; }
            public Vector3 SupportAnchorRight { get; }
            public Vector3 PeekAnchor { get; }
            public Vector3 AdvanceAnchor { get; }
            public Vector3 RetreatAnchor { get; }
            public Vector3 PrimaryAnchor { get; }
            public Vector3 FallbackAnchor { get; }
            public bool HasLiveAllies { get; }
            public bool HasRecentPrime { get; }
            public EscortPhase EscortPhase { get; }
            public HeroLaneDepthBand MaxDepthBand { get; }
            public float MaxForwardZ { get; }
            public float SupportEnvelopeMinZ { get; }
            public float SupportEnvelopeMaxZ { get; }
            public bool CanOpenPeek { get; }
            public HeroInterventionReason InterventionReason { get; }
            public float LaneThreatScore { get; }
            public float LaneValueScore { get; }
            public float SupportAnchorZ => SupportAnchor.z;
            public float PeekAnchorZ => PeekAnchor.z;
            public float AdvanceAnchorZ => AdvanceAnchor.z;
            public float RetreatAnchorZ => RetreatAnchor.z;
            public float PrimaryAnchorZ => PrimaryAnchor.z;
            public float FallbackAnchorZ => FallbackAnchor.z;
            public Vector3[] SupportAnchors => new[] { SupportAnchorLeft, SupportAnchorCenter, SupportAnchorRight };
        }

        public readonly struct SummonLanePreview
        {
            public SummonLanePreview(
                int laneIndex,
                SummonLanePreviewState previewState,
                Vector3 landingPosition,
                Vector3 firstPocketPosition,
                bool hasBlocker,
                Vector3 blockerPosition,
                bool hasRewardObjective,
                Vector3 rewardObjectivePosition)
            {
                LaneIndex = laneIndex;
                PreviewState = previewState;
                LandingPosition = landingPosition;
                FirstPocketPosition = firstPocketPosition;
                HasBlocker = hasBlocker;
                BlockerPosition = blockerPosition;
                HasRewardObjective = hasRewardObjective;
                RewardObjectivePosition = rewardObjectivePosition;
            }

            public int LaneIndex { get; }
            public SummonLanePreviewState PreviewState { get; }
            public Vector3 LandingPosition { get; }
            public Vector3 FirstPocketPosition { get; }
            public bool HasBlocker { get; }
            public Vector3 BlockerPosition { get; }
            public bool HasRewardObjective { get; }
            public Vector3 RewardObjectivePosition { get; }
        }

        private readonly struct LaneRuntimeSnapshot
        {
            public LaneRuntimeSnapshot(
                int laneIndex,
                LanePressureState pressureState,
                BattleStructure frontlineStructure,
                BattleStructure rewardStructure,
                BattleStructure siegeStructure,
                BattleStructure nearestStructure,
                float frontlineStructureZ,
                float frontlineObjectiveZ,
                float clashZ,
                float playerFrontZ,
                float enemyFrontZ,
                int playerCount,
                int enemyCount,
                bool hasFrontlineStructure,
                bool hasFrontlineObjective)
            {
                LaneIndex = laneIndex;
                PressureState = pressureState;
                FrontlineStructure = frontlineStructure;
                RewardStructure = rewardStructure;
                SiegeStructure = siegeStructure;
                NearestStructure = nearestStructure;
                FrontlineStructureZ = frontlineStructureZ;
                FrontlineObjectiveZ = frontlineObjectiveZ;
                ClashZ = clashZ;
                PlayerFrontZ = playerFrontZ;
                EnemyFrontZ = enemyFrontZ;
                PlayerCount = playerCount;
                EnemyCount = enemyCount;
                HasFrontlineStructure = hasFrontlineStructure;
                HasFrontlineObjective = hasFrontlineObjective;
            }

            public int LaneIndex { get; }
            public LanePressureState PressureState { get; }
            public BattleStructure FrontlineStructure { get; }
            public BattleStructure RewardStructure { get; }
            public BattleStructure SiegeStructure { get; }
            public BattleStructure NearestStructure { get; }
            public float FrontlineStructureZ { get; }
            public float FrontlineObjectiveZ { get; }
            public float ClashZ { get; }
            public float PlayerFrontZ { get; }
            public float EnemyFrontZ { get; }
            public int PlayerCount { get; }
            public int EnemyCount { get; }
            public bool HasFrontlineStructure { get; }
            public bool HasFrontlineObjective { get; }
        }

        public readonly struct PlayerTerritoryState
        {
            public PlayerTerritoryState(
                float safeAdvanceZ,
                float overextendDistance,
                float enemyBaseDistance,
                bool isInEnemyBaseZone,
                int alliedUnitCount,
                float pressureDamagePerTick,
                float warningDistance,
                float pressureRampMultiplier01,
                bool isInCoverBreakGrace,
                float coverBreakGraceRemaining,
                float safeAdvanceRetreatDistance)
            {
                SafeAdvanceZ = safeAdvanceZ;
                OverextendDistance = overextendDistance;
                EnemyBaseDistance = enemyBaseDistance;
                IsInEnemyBaseZone = isInEnemyBaseZone;
                AlliedUnitCount = alliedUnitCount;
                PressureDamagePerTick = pressureDamagePerTick;
                WarningDistance = warningDistance;
                PressureRampMultiplier01 = pressureRampMultiplier01;
                IsInCoverBreakGrace = isInCoverBreakGrace;
                CoverBreakGraceRemaining = coverBreakGraceRemaining;
                SafeAdvanceRetreatDistance = safeAdvanceRetreatDistance;
            }

            public float SafeAdvanceZ { get; }
            public float OverextendDistance { get; }
            public float EnemyBaseDistance { get; }
            public bool IsInEnemyBaseZone { get; }
            public int AlliedUnitCount { get; }
            public float PressureDamagePerTick { get; }
            public float WarningDistance { get; }
            public float PressureRampMultiplier01 { get; }
            public bool IsInCoverBreakGrace { get; }
            public float CoverBreakGraceRemaining { get; }
            public float SafeAdvanceRetreatDistance { get; }
        }
    }
}

namespace IsekaiBrawl.Gameplay
{
    public enum PveFinalObjectiveType
    {
        Core = 0,
        BossCore = 1
    }

    public enum PveProjectileEmitterType
    {
        Direct = 0,
        Line = 1,
        Spread = 2
    }

    public enum PveProjectileEmitterTriggerMode
    {
        OnEncounterStart = 0,
        OnGroupCleared = 1,
        LoopWhileAlive = 2
    }

    [Serializable]
    public sealed class PveEnemyPlacement
    {
        public SummonData summonData;
        [Range(0, 4)] public int laneIndex = 2;
        public float depthZ = 24f;
        public float lateralOffset;
        public float spawnDelay;
    }

    [Serializable]
    public sealed class PveStructurePlacement
    {
        public BattleStructureRole structureRole = BattleStructureRole.FrontlineBlocker;
        [Range(0, 4)] public int laneIndex = 2;
        public float depthZ = 32f;
        public float maxHpOverride = 140f;
        public float energyRewardOverride;
        public Vector3 worldScale = new(1.05f, 1.4f, 1.05f);
        public Color tint = new(0.9f, 0.78f, 0.36f, 1f);
    }

    [Serializable]
    public sealed class PveProjectileEmitterPlacement
    {
        public string emitterId = "Emitter";
        [Range(0, 4)] public int laneIndex = 2;
        public float depthZ = 30f;
        public PveProjectileEmitterType emitterType = PveProjectileEmitterType.Direct;
        public PveProjectileEmitterTriggerMode triggerMode = PveProjectileEmitterTriggerMode.OnEncounterStart;
        public float interval = 4.6f;
        public float leadTime = 0.9f;
        public float damage = 10f;
        public bool usesWarningLine = true;
    }

    [Serializable]
    public sealed class PveEncounterGroup
    {
        public string groupId = "Encounter";
        public float triggerZ = 18f;
        public bool mustClearToAdvance = true;
        public float spawnOnEnterDelay;
        public float cameraStopZ = -1f;
        public List<PveEnemyPlacement> enemyPlacements = new();
        public List<PveStructurePlacement> structurePlacements = new();
        public List<PveProjectileEmitterPlacement> projectileEmitterPlacements = new();
    }

    [CreateAssetMenu(fileName = "PveStageData", menuName = "IsekaiBrawl/PVE Stage Data")]
    public sealed class PveStageData : ScriptableObject
    {
        [SerializeField] private string stageId = "stage_01";
        [SerializeField] private string displayName = "스토리 전투";
        [SerializeField] [TextArea] private string description = string.Empty;
        [SerializeField] private float timeLimit = 165f;
        [SerializeField] private float startingEnergyOverride = -1f;
        [SerializeField] private float laneLengthOverride = -1f;
        [SerializeField] private PveFinalObjectiveType finalObjectiveType = PveFinalObjectiveType.Core;
        [SerializeField] private float finalObjectiveHP = 1000f;
        [SerializeField] private List<PveEncounterGroup> encounterGroups = new();

        public string StageId => stageId;
        public string DisplayName => displayName;
        public string Description => description;
        public float TimeLimit => timeLimit;
        public float StartingEnergyOverride => startingEnergyOverride;
        public float LaneLengthOverride => laneLengthOverride;
        public PveFinalObjectiveType FinalObjectiveType => finalObjectiveType;
        public float FinalObjectiveHP => finalObjectiveHP;
        public IReadOnlyList<PveEncounterGroup> EncounterGroups => encounterGroups;

        public static PveStageData CreateRuntimePrototypeStage(IReadOnlyList<SummonData> sourceDeck)
        {
            PveStageData stage = CreateInstance<PveStageData>();
            stage.hideFlags = HideFlags.DontSave;
            stage.stageId = "story_runtime_stage_01";
            stage.displayName = "스토리 전투";
            stage.description = "고정 배치 전장을 돌파하며 장치 직격과 보스 패턴을 회피하는 기본 스테이지입니다.";
            stage.timeLimit = 165f;
            stage.startingEnergyOverride = 40f;
            stage.laneLengthOverride = 84f;
            stage.finalObjectiveType = PveFinalObjectiveType.BossCore;
            stage.finalObjectiveHP = 1000f;
            stage.encounterGroups = BuildRuntimeEncounterGroups(sourceDeck);
            return stage;
        }

        private static List<PveEncounterGroup> BuildRuntimeEncounterGroups(IReadOnlyList<SummonData> sourceDeck)
        {
            SummonData rush = FindSummonByShortLabel(sourceDeck, "Rush");
            SummonData arrow = FindSummonByShortLabel(sourceDeck, "Arrow");
            SummonData breakCard = FindSummonByShortLabel(sourceDeck, "Break");

            List<PveEncounterGroup> groups = new();

            PveEncounterGroup groupOne = new()
            {
                groupId = "approach",
                triggerZ = 8f,
                mustClearToAdvance = true,
                spawnOnEnterDelay = 0f
            };
            groupOne.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.FrontlineBlocker,
                laneIndex = 2,
                depthZ = 24.8f,
                maxHpOverride = 126f,
                worldScale = new Vector3(0.96f, 1.26f, 0.96f),
                tint = new Color(0.86f, 0.74f, 0.34f, 1f)
            });
            groupOne.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.FrontlineBlocker,
                laneIndex = 1,
                depthZ = 28.6f,
                maxHpOverride = 132f,
                worldScale = new Vector3(1f, 1.32f, 1f),
                tint = new Color(0.84f, 0.72f, 0.32f, 1f)
            });
            if (rush != null)
            {
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = rush, laneIndex = 1, depthZ = 19.6f, lateralOffset = -0.12f });
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = rush, laneIndex = 2, depthZ = 18.8f, lateralOffset = -0.06f, spawnDelay = 0.08f });
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = rush, laneIndex = 3, depthZ = 19.2f, lateralOffset = 0.12f, spawnDelay = 0.16f });
            }
            if (arrow != null)
            {
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 2, depthZ = 22.6f, lateralOffset = 0.1f, spawnDelay = 0.24f });
            }

            PveEncounterGroup groupTwo = new()
            {
                groupId = "turret_hold",
                triggerZ = 26f,
                mustClearToAdvance = true,
                spawnOnEnterDelay = 0.25f
            };
            groupTwo.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.FrontlineBlocker,
                laneIndex = 2,
                depthZ = 38.2f,
                maxHpOverride = 168f,
                worldScale = new Vector3(1.08f, 1.42f, 1.08f),
                tint = new Color(0.92f, 0.66f, 0.28f, 1f)
            });
            groupTwo.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.RewardObjective,
                laneIndex = 3,
                depthZ = 45.8f,
                maxHpOverride = 92f,
                energyRewardOverride = 18f,
                worldScale = new Vector3(0.88f, 1.08f, 0.88f),
                tint = new Color(0.34f, 0.96f, 0.64f, 1f)
            });
            groupTwo.projectileEmitterPlacements.Add(new PveProjectileEmitterPlacement
            {
                emitterId = "Turret_L4",
                laneIndex = 3,
                depthZ = 42.2f,
                emitterType = PveProjectileEmitterType.Direct,
                triggerMode = PveProjectileEmitterTriggerMode.LoopWhileAlive,
                interval = 5.2f,
                leadTime = 0.9f,
                damage = 10f,
                usesWarningLine = true
            });
            if (arrow != null)
            {
                groupTwo.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 1, depthZ = 33.8f, lateralOffset = -0.16f });
                groupTwo.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 2, depthZ = 35.6f, lateralOffset = -0.08f, spawnDelay = 0.12f });
                groupTwo.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 3, depthZ = 34.6f, lateralOffset = 0.16f, spawnDelay = 0.2f });
            }

            PveEncounterGroup groupThree = new()
            {
                groupId = "final_gate",
                triggerZ = 48f,
                mustClearToAdvance = false,
                spawnOnEnterDelay = 0f
            };
            groupThree.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.SiegeObjective,
                laneIndex = 2,
                depthZ = 50.6f,
                maxHpOverride = 238f,
                worldScale = new Vector3(1.32f, 1.18f, 1.32f),
                tint = new Color(1f, 0.48f, 0.28f, 1f)
            });
            groupThree.projectileEmitterPlacements.Add(new PveProjectileEmitterPlacement
            {
                emitterId = "GateLine_L3",
                laneIndex = 2,
                depthZ = 51.4f,
                emitterType = PveProjectileEmitterType.Line,
                triggerMode = PveProjectileEmitterTriggerMode.OnEncounterStart,
                interval = 8.8f,
                leadTime = 1.05f,
                damage = 14f,
                usesWarningLine = true
            });
            groupThree.projectileEmitterPlacements.Add(new PveProjectileEmitterPlacement
            {
                emitterId = "BossSupportTurret_L3",
                laneIndex = 2,
                depthZ = 55.2f,
                emitterType = PveProjectileEmitterType.Direct,
                triggerMode = PveProjectileEmitterTriggerMode.LoopWhileAlive,
                interval = 6.1f,
                leadTime = 0.95f,
                damage = 11f,
                usesWarningLine = true
            });
            if (breakCard != null)
            {
                groupThree.enemyPlacements.Add(new PveEnemyPlacement { summonData = breakCard, laneIndex = 2, depthZ = 47.2f, lateralOffset = -0.1f });
            }

            groups.Add(groupOne);
            groups.Add(groupTwo);
            groups.Add(groupThree);
            return groups;
        }

        private static SummonData FindSummonByShortLabel(IReadOnlyList<SummonData> sourceDeck, string shortLabel)
        {
            if (sourceDeck == null || sourceDeck.Count == 0 || string.IsNullOrWhiteSpace(shortLabel))
            {
                return null;
            }

            for (int index = 0; index < sourceDeck.Count; index++)
            {
                SummonData summonData = sourceDeck[index];
                if (summonData != null && string.Equals(summonData.shortLabel, shortLabel, StringComparison.OrdinalIgnoreCase))
                {
                    return summonData;
                }
            }

            return sourceDeck[0];
        }
    }

    public static class PveStageContext
    {
        public static PveStageData SelectedStage { get; private set; }

        public static void SetStage(PveStageData stage)
        {
            SelectedStage = stage;
        }

        public static void Clear()
        {
            SelectedStage = null;
        }
    }

    public sealed class PveProjectileEmitter : MonoBehaviour
    {
        private static readonly List<PveProjectileEmitter> ActiveEmitters = new();

        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private EnemyProjectile directProjectilePrefab;
        [SerializeField] private float projectileSpawnHeight = 1.35f;
        [SerializeField] private float lineTelegraphLength = 8.5f;
        [SerializeField] private float lineLifetime = 5.4f;
        [SerializeField] private Color deviceTint = new(1f, 0.54f, 0.34f, 1f);
        [SerializeField] private Color warningTint = new(1f, 0.44f, 0.34f, 0.92f);

        private readonly List<Coroutine> runningCoroutines = new();

        private BattleStructure ownerStructure;
        private LineRenderer telegraphLine;
        private Renderer cachedRenderer;
        private PveProjectileEmitterType emitterType = PveProjectileEmitterType.Direct;
        private float interval = 4.6f;
        private float leadTime = 0.9f;
        private float damage = 10f;
        private bool usesWarningLine = true;
        private bool isContinuousActive;
        private bool isSequenceActive;
        private bool isDirectLocking;
        private int activeDirectProjectileCount;
        private float shotTimer;

        public bool IsDirectProjectileLocking => isDirectLocking;
        public bool IsDirectProjectileDangerActive => isDirectLocking || activeDirectProjectileCount > 0;

        public static bool HasAnyDirectProjectileLocking
        {
            get
            {
                for (int index = 0; index < ActiveEmitters.Count; index++)
                {
                    PveProjectileEmitter emitter = ActiveEmitters[index];
                    if (emitter != null && emitter.IsDirectProjectileLocking)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool HasAnyDirectProjectileDanger
        {
            get
            {
                for (int index = 0; index < ActiveEmitters.Count; index++)
                {
                    PveProjectileEmitter emitter = ActiveEmitters[index];
                    if (emitter != null && emitter.IsDirectProjectileDangerActive)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void OnEnable()
        {
            if (!ActiveEmitters.Contains(this))
            {
                ActiveEmitters.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveEmitters.Remove(this);
            StopAllSequences();
            SetTelegraphVisible(false, default, default);
        }

        private void Awake()
        {
            if (muzzlePoint == null)
            {
                muzzlePoint = transform;
            }

            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer != null && cachedRenderer.material != null && cachedRenderer.material.HasProperty("_Color"))
            {
                cachedRenderer.material.color = deviceTint;
            }

            EnsureTelegraphLine();
            SetTelegraphVisible(false, default, default);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (ownerStructure != null && ownerStructure.IsDestroyed)
            {
                Deactivate();
                return;
            }

            if (!isContinuousActive || isSequenceActive)
            {
                return;
            }

            shotTimer -= Time.deltaTime;
            if (shotTimer > 0f)
            {
                return;
            }

            TriggerSequence();
            shotTimer = Mathf.Max(0.45f, interval);
        }

        public void Configure(
            PveProjectileEmitterType newEmitterType,
            float newInterval,
            float newLeadTime,
            float newDamage,
            bool newUsesWarningLine)
        {
            emitterType = newEmitterType;
            interval = Mathf.Max(0.45f, newInterval);
            leadTime = Mathf.Max(0f, newLeadTime);
            damage = Mathf.Max(1f, newDamage);
            usesWarningLine = newUsesWarningLine;
            shotTimer = interval;
        }

        public void BindOwner(BattleStructure structure)
        {
            ownerStructure = structure;
        }

        public void ActivateContinuous()
        {
            isContinuousActive = true;
            shotTimer = Mathf.Min(shotTimer <= 0f ? interval : shotTimer, interval);
        }

        public void TriggerSingleShot()
        {
            if (isSequenceActive)
            {
                return;
            }

            TriggerSequence();
        }

        public void Deactivate()
        {
            isContinuousActive = false;
            isDirectLocking = false;
            StopAllSequences();
            SetTelegraphVisible(false, default, default);
        }

        private void TriggerSequence()
        {
            Coroutine routine = StartCoroutine(FireSequence());
            runningCoroutines.Add(routine);
        }

        private IEnumerator FireSequence()
        {
            isSequenceActive = true;
            Vector3 targetPoint = ResolveTargetPoint();
            Vector3 spawnPosition = ResolveSpawnPosition();

            if (leadTime > 0.01f)
            {
                if (emitterType == PveProjectileEmitterType.Direct)
                {
                    isDirectLocking = true;
                }

                SetTelegraphVisible(true, spawnPosition, targetPoint);
                BattlePresentationController.Instance?.ShowWorldText(
                    transform.position + new Vector3(0f, 1.8f, 0f),
                    emitterType == PveProjectileEmitterType.Line ? "LINE" : "LOCK",
                    warningTint,
                    3.5f,
                    Mathf.Clamp(leadTime + 0.18f, 0.32f, 1.4f));
                yield return new WaitForSeconds(leadTime);
            }

            isDirectLocking = false;
            SetTelegraphVisible(false, default, default);

            switch (emitterType)
            {
                case PveProjectileEmitterType.Line:
                    FireLineProjectile(spawnPosition, targetPoint);
                    break;

                case PveProjectileEmitterType.Spread:
                case PveProjectileEmitterType.Direct:
                default:
                    FireDirectProjectile(spawnPosition, targetPoint);
                    break;
            }

            isSequenceActive = false;
        }

        private void FireDirectProjectile(Vector3 spawnPosition, Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - spawnPosition).sqrMagnitude > 0.001f
                ? (targetPoint - spawnPosition).normalized
                : Vector3.back;

            EnemyProjectile projectile = SpawnDirectProjectile(spawnPosition, direction);
            if (projectile == null)
            {
                return;
            }

            activeDirectProjectileCount++;
            projectile.Initialize(
                direction,
                damage,
                BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null,
                EnemyProjectile.ProjectileProfile.Default,
                HandleDirectProjectileResolved);
        }

        private void FireLineProjectile(Vector3 spawnPosition, Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - spawnPosition).sqrMagnitude > 0.001f
                ? (targetPoint - spawnPosition).normalized
                : Vector3.back;

            EnemyLineProjectile projectile = SpawnLineProjectile(spawnPosition, direction);
            if (projectile == null)
            {
                return;
            }

            projectile.Initialize(
                direction,
                damage,
                damage,
                1f,
                lineLifetime,
                EnemyLineProjectile.ProjectileProfile.Default);
        }

        private void HandleDirectProjectileResolved(EnemyProjectile projectile)
        {
            activeDirectProjectileCount = Mathf.Max(0, activeDirectProjectileCount - 1);
        }

        private Vector3 ResolveSpawnPosition()
        {
            Vector3 basePosition = muzzlePoint != null ? muzzlePoint.position : transform.position;
            return basePosition + new Vector3(0f, projectileSpawnHeight, 0f);
        }

        private Vector3 ResolveTargetPoint()
        {
            BattleManager battleManager = BattleManager.Instance;
            PlayerController playerController = battleManager != null ? battleManager.PlayerController : null;
            if (playerController != null)
            {
                return playerController.transform.position + new Vector3(0f, 0.5f, 0.05f);
            }

            Transform fallbackBase = battleManager != null ? battleManager.GetBaseTransform(true) : null;
            return fallbackBase != null
                ? fallbackBase.position + new Vector3(0f, 0.45f, 0f)
                : transform.position + Vector3.back * 8f;
        }

        private EnemyProjectile SpawnDirectProjectile(Vector3 spawnPosition, Vector3 direction)
        {
            if (directProjectilePrefab != null)
            {
                return Instantiate(directProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction.normalized));
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "PveEmitterProjectile_Runtime";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.LookRotation(direction.normalized);
            projectileObject.transform.localScale = Vector3.one * 0.35f;

            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer >= 0)
            {
                projectileObject.layer = projectileLayer;
            }

            Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
            if (projectileRenderer != null && projectileRenderer.material != null && projectileRenderer.material.HasProperty("_Color"))
            {
                projectileRenderer.material.color = new Color(1f, 0.45f, 0.22f, 1f);
            }

            Rigidbody rigidbodyComponent = projectileObject.AddComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            return projectileObject.AddComponent<EnemyProjectile>();
        }

        private static EnemyLineProjectile SpawnLineProjectile(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "PveEmitterLineProjectile_Runtime";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.LookRotation(direction.normalized);
            projectileObject.transform.localScale = Vector3.one * 0.22f;

            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer >= 0)
            {
                projectileObject.layer = projectileLayer;
            }

            Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
            if (projectileRenderer != null && projectileRenderer.material != null && projectileRenderer.material.HasProperty("_Color"))
            {
                projectileRenderer.material.color = new Color(1f, 0.72f, 0.36f, 1f);
            }

            Rigidbody rigidbodyComponent = projectileObject.AddComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            return projectileObject.AddComponent<EnemyLineProjectile>();
        }

        private void EnsureTelegraphLine()
        {
            if (telegraphLine != null)
            {
                return;
            }

            GameObject lineObject = new("EmitterTelegraph");
            lineObject.transform.SetParent(transform, false);
            telegraphLine = lineObject.AddComponent<LineRenderer>();
            telegraphLine.useWorldSpace = true;
            telegraphLine.loop = false;
            telegraphLine.positionCount = 2;
            telegraphLine.numCapVertices = 4;
            telegraphLine.alignment = LineAlignment.View;
            telegraphLine.startWidth = 0.12f;
            telegraphLine.endWidth = 0.12f;
            telegraphLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            telegraphLine.receiveShadows = false;
            telegraphLine.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private void SetTelegraphVisible(bool isVisible, Vector3 start, Vector3 end)
        {
            if (telegraphLine == null)
            {
                return;
            }

            telegraphLine.enabled = isVisible && usesWarningLine;
            if (!telegraphLine.enabled)
            {
                return;
            }

            telegraphLine.startColor = warningTint;
            telegraphLine.endColor = new Color(warningTint.r, warningTint.g, warningTint.b, 0.2f);
            Vector3 resolvedStart = start;
            Vector3 resolvedEnd = end;
            if (emitterType == PveProjectileEmitterType.Line)
            {
                resolvedEnd = resolvedStart + (Vector3.back * Mathf.Max(4.5f, lineTelegraphLength));
            }

            telegraphLine.SetPosition(0, resolvedStart);
            telegraphLine.SetPosition(1, resolvedEnd);
        }

        private void StopAllSequences()
        {
            for (int index = 0; index < runningCoroutines.Count; index++)
            {
                Coroutine routine = runningCoroutines[index];
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
            }

            runningCoroutines.Clear();
            isSequenceActive = false;
        }
    }

    public sealed class PveEncounterDirector : MonoBehaviour
    {
        private sealed class RuntimeEncounterGroup
        {
            public PveEncounterGroup Source { get; set; }
            public readonly List<SummonUnit> SpawnedEnemies = new();
            public readonly List<BattleStructure> SpawnedStructures = new();
            public readonly List<PveProjectileEmitter> SpawnedEmitters = new();
            public bool Started { get; set; }
            public bool ContentsSpawned { get; set; }
            public bool Cleared { get; set; }
            public bool ClearedEmittersTriggered { get; set; }
        }

        [SerializeField] private PveStageData defaultStage;
        [SerializeField] private Transform runtimeRoot;
        [SerializeField] private bool allowRuntimeStageRootBootstrap;

        private readonly List<RuntimeEncounterGroup> runtimeGroups = new();

        private BattleManager battleManager;
        private PlayerController playerController;
        private PveStageData activeStage;
        private int nextEncounterIndex;

        private void Start()
        {
            if (BattleModeContext.CurrentMode != BattleMode.StoryPve)
            {
                enabled = false;
                return;
            }

            battleManager = BattleManager.Instance;
            playerController = battleManager != null ? battleManager.PlayerController : null;
            activeStage = PveStageContext.SelectedStage != null ? PveStageContext.SelectedStage : defaultStage;
            if (activeStage == null)
            {
                enabled = false;
                return;
            }

            if (runtimeRoot == null)
            {
                if (!allowRuntimeStageRootBootstrap)
                {
                    Debug.LogWarning(
                        "PveEncounterDirector requires an authored runtimeRoot. Runtime stage-root bootstrap is disabled.");
                    enabled = false;
                    return;
                }

                GameObject rootObject = new("PveRuntimeStage");
                runtimeRoot = rootObject.transform;
            }

            BuildRuntimeGroups();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (playerController == null && battleManager != null)
            {
                playerController = battleManager.PlayerController;
            }

            if (battleManager == null || playerController == null)
            {
                return;
            }

            TryActivatePendingGroups();
            UpdateGroupClearState();
        }

        private void BuildRuntimeGroups()
        {
            runtimeGroups.Clear();
            IReadOnlyList<PveEncounterGroup> encounterGroups = activeStage.EncounterGroups;
            for (int index = 0; index < encounterGroups.Count; index++)
            {
                PveEncounterGroup group = encounterGroups[index];
                if (group == null)
                {
                    continue;
                }

                runtimeGroups.Add(new RuntimeEncounterGroup { Source = group });
            }

            runtimeGroups.Sort((left, right) => left.Source.triggerZ.CompareTo(right.Source.triggerZ));
            nextEncounterIndex = 0;
        }

        private void TryActivatePendingGroups()
        {
            float playerZ = playerController.transform.position.z;
            while (nextEncounterIndex < runtimeGroups.Count)
            {
                RuntimeEncounterGroup group = runtimeGroups[nextEncounterIndex];
                if (!CanAdvanceToEncounter(nextEncounterIndex) || playerZ < group.Source.triggerZ)
                {
                    break;
                }

                ActivateEncounter(group);
                nextEncounterIndex++;
            }
        }

        private bool CanAdvanceToEncounter(int encounterIndex)
        {
            for (int index = 0; index < encounterIndex; index++)
            {
                RuntimeEncounterGroup previousGroup = runtimeGroups[index];
                if (previousGroup.Started && previousGroup.Source.mustClearToAdvance && !previousGroup.Cleared)
                {
                    return false;
                }
            }

            return true;
        }

        private void ActivateEncounter(RuntimeEncounterGroup group)
        {
            if (group.Started)
            {
                return;
            }

            group.Started = true;
            if (group.Source.spawnOnEnterDelay > 0.01f)
            {
                StartCoroutine(SpawnEncounterDelayed(group, group.Source.spawnOnEnterDelay));
                return;
            }

            SpawnEncounterContents(group);
        }

        private IEnumerator SpawnEncounterDelayed(RuntimeEncounterGroup group, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (group.Started)
            {
                SpawnEncounterContents(group);
            }
        }

        private void SpawnEncounterContents(RuntimeEncounterGroup group)
        {
            group.ContentsSpawned = true;

            for (int index = 0; index < group.Source.structurePlacements.Count; index++)
            {
                PveStructurePlacement placement = group.Source.structurePlacements[index];
                if (placement == null)
                {
                    continue;
                }

                BattleStructure structure = SpawnStructure(placement, index);
                if (structure != null)
                {
                    group.SpawnedStructures.Add(structure);
                }
            }

            for (int index = 0; index < group.Source.enemyPlacements.Count; index++)
            {
                PveEnemyPlacement placement = group.Source.enemyPlacements[index];
                if (placement == null)
                {
                    continue;
                }

                SummonUnit enemy = SpawnEnemyPlacement(placement);
                if (enemy != null)
                {
                    group.SpawnedEnemies.Add(enemy);
                }
            }

            for (int index = 0; index < group.Source.projectileEmitterPlacements.Count; index++)
            {
                PveProjectileEmitterPlacement placement = group.Source.projectileEmitterPlacements[index];
                if (placement == null)
                {
                    continue;
                }

                PveProjectileEmitter emitter = SpawnEmitterPlacement(placement, group.SpawnedStructures);
                if (emitter == null)
                {
                    continue;
                }

                group.SpawnedEmitters.Add(emitter);
                if (placement.triggerMode == PveProjectileEmitterTriggerMode.OnEncounterStart)
                {
                    emitter.TriggerSingleShot();
                }
                else if (placement.triggerMode == PveProjectileEmitterTriggerMode.LoopWhileAlive)
                {
                    emitter.ActivateContinuous();
                }
            }
        }

        private void UpdateGroupClearState()
        {
            for (int index = 0; index < runtimeGroups.Count; index++)
            {
                RuntimeEncounterGroup group = runtimeGroups[index];
                if (!group.Started || !group.ContentsSpawned || group.Cleared)
                {
                    continue;
                }

                if (!IsGroupCleared(group))
                {
                    continue;
                }

                group.Cleared = true;
                for (int emitterIndex = 0; emitterIndex < group.SpawnedEmitters.Count; emitterIndex++)
                {
                    PveProjectileEmitter emitter = group.SpawnedEmitters[emitterIndex];
                    if (emitter == null)
                    {
                        continue;
                    }

                    emitter.Deactivate();
                }

                if (group.ClearedEmittersTriggered)
                {
                    continue;
                }

                group.ClearedEmittersTriggered = true;
                for (int emitterPlacementIndex = 0; emitterPlacementIndex < group.Source.projectileEmitterPlacements.Count; emitterPlacementIndex++)
                {
                    PveProjectileEmitterPlacement placement = group.Source.projectileEmitterPlacements[emitterPlacementIndex];
                    if (placement == null || placement.triggerMode != PveProjectileEmitterTriggerMode.OnGroupCleared)
                    {
                        continue;
                    }

                    PveProjectileEmitter emitter = FindEmitterForPlacement(group, emitterPlacementIndex);
                    emitter?.TriggerSingleShot();
                }
            }
        }

        private bool IsGroupCleared(RuntimeEncounterGroup group)
        {
            for (int index = 0; index < group.SpawnedEnemies.Count; index++)
            {
                SummonUnit enemy = group.SpawnedEnemies[index];
                if (enemy != null && enemy.IsAlive)
                {
                    return false;
                }
            }

            for (int index = 0; index < group.SpawnedStructures.Count; index++)
            {
                BattleStructure structure = group.SpawnedStructures[index];
                if (structure != null && !structure.IsDestroyed)
                {
                    return false;
                }
            }

            return true;
        }

        private PveProjectileEmitter FindEmitterForPlacement(RuntimeEncounterGroup group, int placementIndex)
        {
            if (placementIndex < 0 || placementIndex >= group.SpawnedEmitters.Count)
            {
                return null;
            }

            return group.SpawnedEmitters[placementIndex];
        }

        private SummonUnit SpawnEnemyPlacement(PveEnemyPlacement placement)
        {
            if (placement.summonData == null || placement.summonData.prefab == null || battleManager == null)
            {
                return null;
            }

            Transform spawnPoint = battleManager.EnemySummonSpawnPoint != null
                ? battleManager.EnemySummonSpawnPoint
                : transform;
            float laneX = battleManager.GetLaneCenterX(placement.laneIndex) + placement.lateralOffset;
            Vector3 spawnPosition = new(
                laneX,
                spawnPoint.position.y,
                placement.depthZ);
            GameObject summonObject = Instantiate(placement.summonData.prefab, spawnPosition, spawnPoint.rotation, runtimeRoot);
            SummonUnit summonUnit = summonObject.GetComponent<SummonUnit>();
            if (summonUnit != null)
            {
                summonUnit.Init(placement.summonData, false);
                summonUnit.SetAssignedLane(BattleLaneUtility.ClampLaneIndex(placement.laneIndex));
            }

            return summonUnit;
        }

        private BattleStructure SpawnStructure(PveStructurePlacement placement, int structureIndex)
        {
            if (battleManager == null)
            {
                return null;
            }

            PrimitiveType primitiveType = placement.structureRole == BattleStructureRole.SiegeObjective
                ? PrimitiveType.Cube
                : PrimitiveType.Cylinder;
            GameObject structureObject = GameObject.CreatePrimitive(primitiveType);
            structureObject.name = $"PveStructure_{structureIndex}_{placement.structureRole}";
            structureObject.transform.SetParent(runtimeRoot, false);
            structureObject.transform.position = new Vector3(
                battleManager.GetLaneCenterX(placement.laneIndex),
                0.8f,
                placement.depthZ);
            structureObject.transform.localScale = placement.worldScale;

            Renderer renderer = structureObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                Material material = new(shader);
                material.color = placement.tint;
                renderer.material = material;
            }

            BattleStructure structure = structureObject.GetComponent<BattleStructure>();
            if (structure == null)
            {
                structure = structureObject.AddComponent<BattleStructure>();
            }

            structure.Configure(placement.maxHpOverride, placement.energyRewardOverride, placement.structureRole);
            return structure;
        }

        private PveProjectileEmitter SpawnEmitterPlacement(PveProjectileEmitterPlacement placement, List<BattleStructure> spawnedStructures)
        {
            if (battleManager == null)
            {
                return null;
            }

            GameObject emitterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            emitterObject.name = string.IsNullOrWhiteSpace(placement.emitterId)
                ? "PveProjectileEmitter"
                : placement.emitterId;
            emitterObject.transform.SetParent(runtimeRoot, false);
            emitterObject.transform.position = new Vector3(
                battleManager.GetLaneCenterX(placement.laneIndex),
                1.1f,
                placement.depthZ);
            emitterObject.transform.localScale = placement.emitterType == PveProjectileEmitterType.Line
                ? new Vector3(0.42f, 0.2f, 0.42f)
                : new Vector3(0.32f, 0.36f, 0.32f);
            Collider emitterCollider = emitterObject.GetComponent<Collider>();
            if (emitterCollider != null)
            {
                emitterCollider.enabled = false;
            }

            Renderer renderer = emitterObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                Material material = new(shader);
                material.color = placement.emitterType == PveProjectileEmitterType.Line
                    ? new Color(1f, 0.76f, 0.36f, 1f)
                    : new Color(1f, 0.46f, 0.34f, 1f);
                renderer.material = material;
            }

            PveProjectileEmitter emitter = emitterObject.AddComponent<PveProjectileEmitter>();
            emitter.Configure(
                placement.emitterType,
                placement.interval,
                placement.leadTime,
                placement.damage,
                placement.usesWarningLine);
            emitter.BindOwner(FindNearestSameLaneStructure(spawnedStructures, placement.laneIndex, placement.depthZ));
            return emitter;
        }

        private static BattleStructure FindNearestSameLaneStructure(List<BattleStructure> structures, int laneIndex, float worldZ)
        {
            if (structures == null || structures.Count == 0 || BattleManager.Instance == null)
            {
                return null;
            }

            BattleManager battleManager = BattleManager.Instance;
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
            float bestDistance = float.MaxValue;
            BattleStructure bestStructure = null;
            for (int index = 0; index < structures.Count; index++)
            {
                BattleStructure structure = structures[index];
                if (structure == null || structure.IsDestroyed)
                {
                    continue;
                }

                if (battleManager.GetNearestLaneIndex(structure.transform.position.x) != resolvedLaneIndex)
                {
                    continue;
                }

                float distance = Mathf.Abs(structure.transform.position.z - worldZ);
                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestStructure = structure;
            }

            return bestStructure;
        }
    }
}
