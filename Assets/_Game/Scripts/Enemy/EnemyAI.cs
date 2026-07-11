using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int CastTriggerHash = Animator.StringToHash("Cast");
        private static readonly BossTacticState[] BossTactics =
        {
            BossTacticState.RearGuard,
            BossTacticState.EscortWave,
            BossTacticState.ContestMid,
            BossTacticState.SiegeStructure,
            BossTacticState.PunishOverextend,
            BossTacticState.CommitPush,
            BossTacticState.FallBack
        };
        private static readonly SummonIntentState[] SummonIntents =
        {
            SummonIntentState.Probe,
            SummonIntentState.HoldLine,
            SummonIntentState.EscortPush,
            SummonIntentState.BreakPost,
            SummonIntentState.PunishHero,
            SummonIntentState.BaseRush
        };

        private enum EnemyPhase
        {
            Opening,
            Pressure,
            Siege,
            FinalPush
        }

        private enum BossTacticState
        {
            RearGuard,
            EscortWave,
            ContestMid,
            SiegeStructure,
            PunishOverextend,
            CommitPush,
            FallBack
        }

        private enum SummonIntentState
        {
            Probe,
            HoldLine,
            EscortPush,
            BreakPost,
            PunishHero,
            BaseRush
        }

        private readonly struct ProjectileLaneShot
        {
            public ProjectileLaneShot(float targetX, EnemyProjectile.ProjectileProfile profile)
            {
                TargetX = targetX;
                Profile = profile;
            }

            public float TargetX { get; }
            public EnemyProjectile.ProjectileProfile Profile { get; }
        }

        private readonly struct LineVolleyShot
        {
            public LineVolleyShot(float targetX, EnemyLineProjectile.ProjectileProfile profile)
            {
                TargetX = targetX;
                Profile = profile;
            }

            public float TargetX { get; }
            public EnemyLineProjectile.ProjectileProfile Profile { get; }
        }

        private readonly struct BossFormationSnapshot
        {
            public BossFormationSnapshot(
                int enemyUnitCount,
                int playerUnitCount,
                float enemyFrontZ,
                float playerFrontZ,
                float playerHeroZ,
                float enemyAnchorX,
                float playerAnchorX)
            {
                EnemyUnitCount = enemyUnitCount;
                PlayerUnitCount = playerUnitCount;
                EnemyFrontZ = enemyFrontZ;
                PlayerFrontZ = playerFrontZ;
                PlayerHeroZ = playerHeroZ;
                EnemyAnchorX = enemyAnchorX;
                PlayerAnchorX = playerAnchorX;
            }

            public int EnemyUnitCount { get; }
            public int PlayerUnitCount { get; }
            public float EnemyFrontZ { get; }
            public float PlayerFrontZ { get; }
            public float PlayerHeroZ { get; }
            public float EnemyAnchorX { get; }
            public float PlayerAnchorX { get; }
            public float ClashCenterZ => (EnemyFrontZ + PlayerFrontZ) * 0.5f;
            public float ClashAnchorX => (EnemyAnchorX + PlayerAnchorX) * 0.5f;
            public float PlayerOverextendDistance => PlayerHeroZ - PlayerFrontZ;
        }

        private readonly struct BossDecisionContext
        {
            public BossDecisionContext(
                BossFormationSnapshot formation,
                float enemyAdvantage,
                BattleStructure priorityStructure,
                int structuresRemaining,
                float playerBaseRatio,
                float laneLength,
                EnemyPhase phase)
            {
                Formation = formation;
                EnemyAdvantage = Mathf.Clamp01(enemyAdvantage);
                PriorityStructure = priorityStructure;
                StructuresRemaining = Mathf.Max(0, structuresRemaining);
                PlayerBaseRatio = Mathf.Clamp01(playerBaseRatio);
                LaneLength = Mathf.Max(1f, laneLength);
                Phase = phase;

                FrontGap = Mathf.Abs(formation.PlayerFrontZ - formation.EnemyFrontZ);
                WaveDelta = formation.EnemyUnitCount - formation.PlayerUnitCount;
                HasWave = formation.EnemyUnitCount >= 2;
                HasPriorityStructure = priorityStructure != null && !priorityStructure.IsDestroyed;
                WaveSupport01 = Mathf.InverseLerp(1f, 4.5f, formation.EnemyUnitCount);
                WaveLead01 = Mathf.InverseLerp(-2f, 3f, WaveDelta);
                BackFoot01 = 1f - EnemyAdvantage;
                HeroPressure01 = Mathf.InverseLerp(3.15f, 8.2f, formation.PlayerOverextendDistance);
                ClashPressure01 = 1f - Mathf.InverseLerp(7.5f, 18f, FrontGap);
                CloseOut01 = Mathf.Max(
                    Mathf.InverseLerp(0.58f, 0.26f, PlayerBaseRatio),
                    phase == EnemyPhase.FinalPush ? 0.42f : 0f);

                if (HasPriorityStructure)
                {
                    float forwardGap = formation.EnemyFrontZ - priorityStructure.transform.position.z;
                    float healthRatio = priorityStructure.MaxHP <= 0.001f
                        ? 1f
                        : Mathf.Clamp01(priorityStructure.CurrentHP / priorityStructure.MaxHP);
                    float structureAccess = 1f - Mathf.InverseLerp(2.2f, 12.5f, Mathf.Abs(forwardGap - 4.8f));
                    float structureDepth = Mathf.InverseLerp(LaneLength * 0.88f, LaneLength * 0.38f, priorityStructure.transform.position.z);
                    float structureCountWeight = Mathf.InverseLerp(1f, 4f, StructuresRemaining);

                    StructureOpportunity01 = Mathf.Clamp01((structureAccess * 0.48f) + ((1f - healthRatio) * 0.26f) + (structureDepth * 0.16f) + (structureCountWeight * 0.1f));
                }
                else
                {
                    StructureOpportunity01 = 0f;
                }
            }

            public BossFormationSnapshot Formation { get; }
            public float EnemyAdvantage { get; }
            public BattleStructure PriorityStructure { get; }
            public int StructuresRemaining { get; }
            public float PlayerBaseRatio { get; }
            public float LaneLength { get; }
            public EnemyPhase Phase { get; }
            public float FrontGap { get; }
            public int WaveDelta { get; }
            public bool HasWave { get; }
            public bool HasPriorityStructure { get; }
            public float WaveSupport01 { get; }
            public float WaveLead01 { get; }
            public float BackFoot01 { get; }
            public float HeroPressure01 { get; }
            public float ClashPressure01 { get; }
            public float CloseOut01 { get; }
            public float StructureOpportunity01 { get; }
            public bool EnemyAhead => EnemyAdvantage >= 0.58f || WaveDelta >= 1;
            public bool EnemyBehind => EnemyAdvantage <= 0.34f || WaveDelta <= -2;
        }

        public event Action<SummonData> OnNextSummonDecided;
        public event Action<float> OnNextSummonCountdownChanged;
        public event Action<float> OnProjectileCountdownChanged;
        public event Action<string> OnPhaseChanged;
        public event Action<string> OnVolleyPatternChanged;
        public event Action<string> OnBossTacticChanged;
        public event Action<string> OnSummonIntentChanged;
        public event Action<SummonData, Vector3, bool> OnSummonSpawned;
        public event Action<float, float> OnEnergyChanged;

        [SerializeField] private List<SummonData> enemyDeck = new();
        [SerializeField] private Transform summonSpawnPoint;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private EnemyLineProjectile lineProjectilePrefab;
        [SerializeField] private float maxEnergy = 132f;
        [SerializeField] private float startingEnergy = 30f;
        [SerializeField] private float baseChargeRate = 3.2f;
        [SerializeField] private float waveChargeBonus = 0.32f;
        [SerializeField] private float pressureChargeBonus = 0.46f;
        [SerializeField] private float comebackChargeBonus = 0.62f;
        [SerializeField] private bool allowAutomaticSummonsInStoryPve = false;
        [SerializeField] private float summonInterval = 5f;
        [SerializeField] private float projectileInterval = 3f;
        [SerializeField] private float lineProjectileInterval = 4.6f;
        [SerializeField] private float directProjectileLockLeadTime = 0.9f;
        [SerializeField] private float directProjectileGlobalRecovery = 3f;
        [SerializeField] private float directProjectileRetryDelay = 0.35f;
        [SerializeField] private float directProjectileExposureHoldTime = 0.85f;
        [SerializeField] private float directProjectileMaxRange = 48f;
        [SerializeField] private int directProjectileMaxLaneDelta = 1;
        [SerializeField] private float openingSummonLeadTime = 3.2f;
        [SerializeField] private float openingVolleyLeadTime = 7.4f;
        [SerializeField] private float openingLineVolleyLeadTime = 5.4f;
        [SerializeField] private float openingMinimumDuration = 24f;
        [SerializeField] private float pressureMinimumDuration = 72f;
        [SerializeField] private float siegeMinimumDuration = 118f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float lineProjectileDamage = 14f;
        [SerializeField] private float lineProjectileBaseDamage = 18f;
        [SerializeField] private float lineProjectileStructureDamageMultiplier = 1.1f;
        [SerializeField] private float laneOffsetSpacing = 0.9f;
        [SerializeField] private float forwardOffsetSpacing = 0.45f;
        [SerializeField] private float heroPatternSpawnHeight = 3.1f;
        [SerializeField] private float heroPatternTargetForwardOffset = 0.05f;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private float bossMoveSpeed = 5.2f;
        [SerializeField] private float bossTrackStrength = 0.82f;
        [SerializeField] private float bossRearInset = 6.5f;
        [SerializeField] private float bossAdvanceDepth = 44f;
        [SerializeField] private float bossContactDamage = 16f;
        [SerializeField] private float bossContactCooldown = 1.15f;
        [SerializeField] private float bossContactRange = 1.45f;
        [SerializeField] private float bossPushbackDistance = 1.9f;
        [SerializeField] private float bossDecisionInterval = 0.45f;
        [SerializeField] private float bossTacticSwapThreshold = 0.72f;
        [SerializeField] private float bossTacticStickyBias = 0.42f;
        [SerializeField] private float bossMinimumTacticCommit = 0.95f;
        [SerializeField] private float bossMaximumTacticCommit = 1.95f;
        [SerializeField] private float summonIntentSwapThreshold = 0.58f;
        [SerializeField] private float summonIntentStickyBias = 0.28f;
        [SerializeField] private float summonIntentMinimumCommit = 0.72f;
        [SerializeField] private float summonIntentMaximumCommit = 1.48f;
        [SerializeField] private float bossSupportRadius = 7.6f;
        [SerializeField] private float bossSupportDuration = 1.6f;
        [SerializeField] private float bossSupportDamageMultiplier = 1.18f;
        [SerializeField] private float bossSupportMoveMultiplier = 1.12f;
        [SerializeField] private float bossSupportPulseCooldown = 2.2f;
        [SerializeField] private float bossSupportPulseHeal = 8f;
        [SerializeField] private float bossStructurePressureRange = 3.8f;
        [SerializeField] private float bossStructurePressureDamage = 46f;
        [SerializeField] private float bossStructurePressureCooldown = 2.6f;
        [SerializeField] private float bossStructurePressureTelegraphDelay = 0.48f;

        private readonly List<SummonData> runtimeEnemyDeck = new();
        private readonly List<ProjectileLaneShot> projectilePatternBuffer = new();
        private readonly List<LineVolleyShot> lineVolleyPatternBuffer = new();
        private SummonData nextSummon;
        private float summonTimer;
        private float projectileTimer;
        private float lineProjectileTimer;
        private float initialBattleDuration = 180f;
        private PlayerController playerController;
        private EnemyPhase currentPhase;
        private float currentSummonInterval;
        private float currentProjectileInterval;
        private float currentLineProjectileInterval;
        private int summonCount;
        private int projectileVolleyCount;
        private int lineProjectileVolleyCount;
        private int activeDirectProjectileCount;
        private string currentVolleyPatternName = "Pattern Unknown";
        private Rigidbody cachedRigidbody;
        private CapsuleCollider cachedCapsuleCollider;
        private float nextBossContactTime;
        private float nextBossDecisionTime;
        private float nextBossTacticShiftAllowedTime;
        private float nextSummonIntentShiftAllowedTime;
        private float nextBossSupportPulseTime;
        private float nextBossStructurePressureTime;
        private float nextDirectProjectileReadyTime;
        private float currentEnergy;
        private float currentEnergyChargeRate;
        private BossTacticState currentBossTactic = BossTacticState.RearGuard;
        private float currentBossTacticScore;
        private SummonIntentState currentSummonIntent;
        private float currentSummonIntentScore;
        private BossFormationSnapshot currentBossFormation;
        private BossDecisionContext currentBossDecisionContext;
        private float currentEnemyAdvantage = 0.45f;
        private BattleStructure currentPriorityStructure;
        private int initialStructureCount;
        private SummonData lastSummonData;
        private SummonType lastSummonType;
        private int repeatTypeStreak;
        private int repeatCardStreak;
        private int waveLaneAnchorIndex = -1;
        private int lastResolvedSummonLaneIndex = -1;
        private float waveLaneAnchorExpireTime;
        private bool needsSummonReplan;
        private bool siegeStrikePending;
        private Transform cachedGroundVisualRoot;
        private Renderer[] cachedGroundRenderers = Array.Empty<Renderer>();
        private float lastDirectProjectileExposureTime = float.NegativeInfinity;

        public SummonData NextSummon => nextSummon;
        public float RemainingSummonCountdown => ResolveSummonReadyCountdown();
        public float RemainingProjectileCountdown => projectileTimer;
        public float RemainingDirectProjectileRecovery => Mathf.Max(0f, nextDirectProjectileReadyTime - Time.time);
        public int ActiveDirectProjectileCount => activeDirectProjectileCount;
        public bool IsDirectProjectileLocking =>
            activeDirectProjectileCount <= 0 &&
            projectileTimer <= directProjectileLockLeadTime &&
            Time.time >= nextDirectProjectileReadyTime &&
            Time.time - lastDirectProjectileExposureTime <= directProjectileExposureHoldTime;
        public bool IsDirectProjectileDangerActive => IsDirectProjectileLocking || activeDirectProjectileCount > 0;
        public string CurrentPhaseName => GetPhaseDisplayName(currentPhase);
        public string CurrentVolleyPatternName => currentVolleyPatternName;
        public string CurrentBossTacticName => GetBossTacticDisplayName(currentBossTactic);
        public string CurrentSummonIntentName => GetSummonIntentDisplayName(currentSummonIntent);
        public string CurrentBossCueShort => ResolveBossCueShort(currentBossTactic);
        public string CurrentBossCue => ResolveBossCue(currentBossTactic, CurrentBossConfidence01);
        public string CurrentSummonCueShort => ResolveSummonCueShort(currentSummonIntent);
        public string CurrentSummonCue => ResolveSummonCue(currentSummonIntent, CurrentSummonConfidence01);
        public string CurrentCounterAdvice => ResolveCounterAdvice();
        public Color CurrentSignalColor => GetBossTacticColor(currentBossTactic);
        public float CurrentBossConfidence01 => Mathf.InverseLerp(1.4f, 4.8f, currentBossTacticScore);
        public float CurrentSummonConfidence01 => Mathf.InverseLerp(1.2f, 5f, currentSummonIntentScore);
        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float CurrentEnergyChargeRate => currentEnergyChargeRate;
        public float NextSummonCost => nextSummon != null ? nextSummon.energyCost : 0f;
        public float NextSummonShortage => nextSummon != null ? Mathf.Max(0f, nextSummon.energyCost - currentEnergy) : 0f;
        public bool IsBankingForNextSummon => nextSummon != null && NextSummonShortage > 0.01f;

        private void Awake()
        {
            EnsureBossCollisionBody();
            BuildRuntimeDeck();
            currentEnergy = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
        }

        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                summonSpawnPoint = summonSpawnPoint != null ? summonSpawnPoint : BattleManager.Instance.EnemySummonSpawnPoint;
                playerController = BattleManager.Instance.PlayerController;
            }

            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>();
            }

            if (GameManager.Instance != null)
            {
                initialBattleDuration = Mathf.Max(1f, GameManager.Instance.RemainingTime);
            }

            initialStructureCount = CountActiveStructures();
            ApplyPhaseSettings(forceNotify: true);
            AlignVisualToGround();
            summonTimer = currentPhase == EnemyPhase.Opening
                ? Mathf.Min(currentSummonInterval, openingSummonLeadTime)
                : currentSummonInterval;
            projectileTimer = currentPhase == EnemyPhase.Opening
                ? Mathf.Max(currentProjectileInterval, openingVolleyLeadTime)
                : currentProjectileInterval;
            lineProjectileTimer = currentPhase == EnemyPhase.Opening
                ? Mathf.Max(currentLineProjectileInterval, openingLineVolleyLeadTime)
                : currentLineProjectileInterval;
            if (playerController != null)
            {
                EvaluateBossTactic(forceNotify: true);
            }

            UpdateSummonIntent(forceNotify: true);
            TryReplanNextSummon();
            if (playerController != null)
            {
                BuildProjectilePattern(projectilePatternBuffer);
            }

            currentEnergyChargeRate = ResolveEnemyChargeRate();
            NotifyEnergyChanged();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (playerController == null && BattleManager.Instance != null)
            {
                playerController = BattleManager.Instance.PlayerController;
            }

            ApplyPhaseSettings(forceNotify: false);
            EvaluateBossTactic(forceNotify: false);
            UpdateSummonIntent(forceNotify: false);
            UpdateEnemyEnergy();
            TryReplanNextSummon();
            UpdateSummonLogic();
            UpdateProjectileLogic();
            UpdateLineProjectileLogic();
            UpdateBossMovement();
            UpdateBossSupportPulse();
            UpdateBossStructurePressure();
            HandleBossCollisionPressure();
        }

        private void LateUpdate()
        {
            AlignVisualToGround();
        }

        private void BuildRuntimeDeck()
        {
            runtimeEnemyDeck.Clear();
            if (enemyDeck.Count == 0)
            {
                return;
            }

            runtimeEnemyDeck.AddRange(PrototypeDeckFactory.BuildPrototypeDeck(enemyDeck));
        }

        private void UpdateSummonLogic()
        {
            if (BattleModeContext.CurrentMode == BattleMode.StoryPve && !allowAutomaticSummonsInStoryPve)
            {
                OnNextSummonCountdownChanged?.Invoke(0f);
                return;
            }

            if (runtimeEnemyDeck.Count == 0 || summonSpawnPoint == null)
            {
                return;
            }

            summonTimer -= Time.deltaTime;
            OnNextSummonCountdownChanged?.Invoke(ResolveSummonReadyCountdown());

            if (summonTimer > 0f)
            {
                return;
            }

            if (nextSummon != null && !CanAffordSummon(nextSummon))
            {
                return;
            }

            SummonData spawnedSummon = nextSummon;
            SpawnNextSummon();
            summonTimer = ResolveNextSummonDelay(spawnedSummon);
            DecideNextSummon();
        }

        private void UpdateEnemyEnergy()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            currentEnergyChargeRate = ResolveEnemyChargeRate();
            if (currentEnergyChargeRate <= 0f)
            {
                return;
            }

            AddEnergy(currentEnergyChargeRate * Time.deltaTime);
        }

        private float ResolveEnemyChargeRate()
        {
            float phaseMultiplier = currentPhase switch
            {
                EnemyPhase.Opening => 0.96f,
                EnemyPhase.Pressure => 1.02f,
                EnemyPhase.Siege => 1.1f,
                EnemyPhase.FinalPush => 1.18f,
                _ => 1f
            };

            float rate = baseChargeRate * phaseMultiplier;
            rate += currentBossDecisionContext.HasWave ? waveChargeBonus : 0f;
            rate += pressureChargeBonus * Mathf.Max(currentBossDecisionContext.ClashPressure01 * 0.7f, currentBossDecisionContext.StructureOpportunity01 * 0.55f);
            rate += comebackChargeBonus * currentBossDecisionContext.BackFoot01;

            if (currentBossTactic == BossTacticState.CommitPush || currentSummonIntent == SummonIntentState.BaseRush)
            {
                rate += 0.24f;
            }

            return Mathf.Clamp(rate, 1.8f, 5.25f);
        }

        private void AddEnergy(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float previousEnergy = currentEnergy;
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
            if (!Mathf.Approximately(previousEnergy, currentEnergy))
            {
                NotifyEnergyChanged();
            }
        }

        private bool SpendEnergy(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (currentEnergy + 0.01f < amount)
            {
                return false;
            }

            currentEnergy = Mathf.Clamp(currentEnergy - amount, 0f, maxEnergy);
            NotifyEnergyChanged();
            return true;
        }

        private bool CanAffordSummon(SummonData summonData)
        {
            return summonData != null && currentEnergy + 0.01f >= summonData.energyCost;
        }

        private float ResolveTimeUntilAffordable(SummonData summonData)
        {
            if (summonData == null)
            {
                return 0f;
            }

            float shortage = Mathf.Max(0f, summonData.energyCost - currentEnergy);
            if (shortage <= 0.01f)
            {
                return 0f;
            }

            float rate = Mathf.Max(0.01f, currentEnergyChargeRate > 0.01f ? currentEnergyChargeRate : ResolveEnemyChargeRate());
            return shortage / rate;
        }

        private float ResolveSummonReadyCountdown()
        {
            float cadenceCountdown = Mathf.Max(0f, summonTimer);
            if (nextSummon == null)
            {
                return cadenceCountdown;
            }

            float energyCountdown = ResolveTimeUntilAffordable(nextSummon);
            return Mathf.Max(cadenceCountdown, energyCountdown);
        }

        private void NotifyEnergyChanged()
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        private void UpdateProjectileLogic()
        {
            if (playerController == null || projectileSpawnPoint == null)
            {
                return;
            }

            RefreshDirectProjectileExposure();
            projectileTimer -= Time.deltaTime;
            OnProjectileCountdownChanged?.Invoke(Mathf.Max(0f, projectileTimer));
            if (projectileTimer > 0f)
            {
                return;
            }

            if (!CanOpenDirectProjectileWindow())
            {
                projectileTimer = ResolveDirectProjectileRetryDelay();
                OnProjectileCountdownChanged?.Invoke(Mathf.Max(0f, projectileTimer));
                return;
            }

            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger(CastTriggerHash);
            }

            BuildProjectilePattern(projectilePatternBuffer);
            if (projectilePatternBuffer.Count == 0)
            {
                projectileTimer = currentProjectileInterval;
                return;
            }

            int firedProjectiles = 0;
            for (int index = 0; index < projectilePatternBuffer.Count; index++)
            {
                ProjectileLaneShot shot = projectilePatternBuffer[index];
                Vector3 spawnPosition = ResolveProjectileSpawnPosition(shot.TargetX, index, projectilePatternBuffer.Count);
                Vector3 direction = ResolveProjectileDirection(spawnPosition, shot.TargetX);
                EnemyProjectile projectile = SpawnProjectile(spawnPosition, direction);
                if (projectile == null)
                {
                    continue;
                }

                activeDirectProjectileCount++;
                firedProjectiles++;
                projectile.Initialize(direction, ResolveProjectileDamage(), playerController, shot.Profile, HandleDirectProjectileResolved);
            }

            if (firedProjectiles > 0)
            {
                projectileVolleyCount++;
                nextDirectProjectileReadyTime = Time.time + directProjectileGlobalRecovery;
            }

            projectileTimer = currentProjectileInterval;
        }

        private void RefreshDirectProjectileExposure()
        {
            if (IsPlayerWithinDirectProjectileWindow())
            {
                lastDirectProjectileExposureTime = Time.time;
            }
        }

        private bool CanOpenDirectProjectileWindow()
        {
            if (playerController == null || projectileSpawnPoint == null)
            {
                return false;
            }

            if (activeDirectProjectileCount > 0)
            {
                return false;
            }

            if (Time.time < nextDirectProjectileReadyTime)
            {
                return false;
            }

            RefreshDirectProjectileExposure();
            return Time.time - lastDirectProjectileExposureTime <= directProjectileExposureHoldTime;
        }

        private float ResolveDirectProjectileRetryDelay()
        {
            float retryDelay = Mathf.Max(0.12f, directProjectileRetryDelay);
            if (Time.time < nextDirectProjectileReadyTime)
            {
                retryDelay = Mathf.Max(retryDelay, nextDirectProjectileReadyTime - Time.time);
            }

            return retryDelay;
        }

        private bool IsPlayerWithinDirectProjectileWindow()
        {
            if (playerController == null)
            {
                return false;
            }

            Vector3 planarDelta = playerController.transform.position - transform.position;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude > directProjectileMaxRange * directProjectileMaxRange)
            {
                return false;
            }

            float[] lanes = GetProjectileLaneAnchors();
            if (lanes == null || lanes.Length == 0)
            {
                return true;
            }

            int playerLaneIndex = GetNearestLaneIndex(playerController.transform.position.x, lanes);
            int bossLaneIndex = GetNearestLaneIndex(transform.position.x, lanes);
            return Mathf.Abs(playerLaneIndex - bossLaneIndex) <= Mathf.Max(0, directProjectileMaxLaneDelta);
        }

        private void HandleDirectProjectileResolved(EnemyProjectile projectile)
        {
            activeDirectProjectileCount = Mathf.Max(0, activeDirectProjectileCount - 1);
        }

        private void UpdateLineProjectileLogic()
        {
            if (projectileSpawnPoint == null)
            {
                return;
            }

            lineProjectileTimer -= Time.deltaTime;
            if (lineProjectileTimer > 0f)
            {
                return;
            }

            BuildLineVolleyPattern(lineVolleyPatternBuffer);
            if (lineVolleyPatternBuffer.Count == 0)
            {
                lineProjectileTimer = currentLineProjectileInterval;
                return;
            }

            for (int index = 0; index < lineVolleyPatternBuffer.Count; index++)
            {
                LineVolleyShot shot = lineVolleyPatternBuffer[index];
                Vector3 spawnPosition = ResolveLineProjectileSpawnPosition(shot.TargetX, index, lineVolleyPatternBuffer.Count);
                Vector3 direction = ResolveLineProjectileDirection(spawnPosition, shot.TargetX);
                EnemyLineProjectile projectile = SpawnLineProjectile(spawnPosition, direction);
                if (projectile == null)
                {
                    continue;
                }

                projectile.Initialize(
                    direction,
                    ResolveLineProjectileDamage(),
                    ResolveLineProjectileBaseDamage(),
                    ResolveLineProjectileStructureDamageMultiplier(),
                    5.4f,
                    shot.Profile);
            }

            lineProjectileVolleyCount++;
            lineProjectileTimer = currentLineProjectileInterval;
        }

        private EnemyProjectile SpawnProjectile(Vector3 spawnPosition, Vector3 direction)
        {
            if (projectilePrefab != null)
            {
                return Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction.normalized));
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "EnemyProjectile_Runtime";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.LookRotation(direction.normalized);
            projectileObject.transform.localScale = Vector3.one * 0.35f;

            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer >= 0)
            {
                projectileObject.layer = projectileLayer;
            }

            Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
            if (projectileRenderer != null)
            {
                projectileRenderer.material.color = new Color(1f, 0.45f, 0.22f);
            }

            Rigidbody rigidbodyComponent = projectileObject.AddComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            return projectileObject.AddComponent<EnemyProjectile>();
        }

        private EnemyLineProjectile SpawnLineProjectile(Vector3 spawnPosition, Vector3 direction)
        {
            if (lineProjectilePrefab != null)
            {
                return Instantiate(lineProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction.normalized));
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "EnemyLineProjectile_Runtime";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.LookRotation(direction.normalized);
            projectileObject.transform.localScale = Vector3.one * 0.22f;

            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer >= 0)
            {
                projectileObject.layer = projectileLayer;
            }

            Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
            if (projectileRenderer != null)
            {
                projectileRenderer.material.color = new Color(1f, 0.72f, 0.36f, 1f);
            }

            Rigidbody rigidbodyComponent = projectileObject.AddComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            return projectileObject.AddComponent<EnemyLineProjectile>();
        }

        private void SpawnNextSummon()
        {
            if (nextSummon == null || nextSummon.prefab == null)
            {
                return;
            }

            if (!SpendEnergy(nextSummon.energyCost))
            {
                return;
            }

            Vector3 spawnOffset = ResolveSummonSpawnOffset(nextSummon, summonCount);
            GameObject summonObject = Instantiate(nextSummon.prefab, summonSpawnPoint.position + spawnOffset, summonSpawnPoint.rotation);
            SummonUnit summonUnit = summonObject.GetComponent<SummonUnit>();
            if (summonUnit != null)
            {
                summonUnit.Init(nextSummon, false);
                summonUnit.SetAssignedLane(lastResolvedSummonLaneIndex >= 0 ? lastResolvedSummonLaneIndex : BattleLaneUtility.DefaultLaneCount / 2);
            }

            OnSummonSpawned?.Invoke(nextSummon, summonObject.transform.position, false);
            UpdateSummonHistory(nextSummon);
            UpdateWaveLaneAnchor(nextSummon, lastResolvedSummonLaneIndex);
            summonCount++;
        }

        private void DecideNextSummon()
        {
            if (runtimeEnemyDeck.Count == 0)
            {
                nextSummon = null;
                OnNextSummonDecided?.Invoke(null);
                return;
            }

            nextSummon = PickWeightedCard();
            needsSummonReplan = false;
            OnNextSummonDecided?.Invoke(nextSummon);
        }

        private SummonData PickWeightedCard()
        {
            float readySoonWindow = Mathf.Max(1.4f, currentSummonInterval * 0.66f);
            if (TryRollWeightedCard(readySoonWindow, out SummonData readySoonCard))
            {
                return readySoonCard;
            }

            if (TryRollWeightedCard(float.PositiveInfinity, out SummonData fallbackCard))
            {
                return fallbackCard;
            }

            return runtimeEnemyDeck.Count > 0 ? runtimeEnemyDeck[runtimeEnemyDeck.Count - 1] : null;
        }

        private bool TryRollWeightedCard(float affordabilityWindow, out SummonData resolvedCard)
        {
            resolvedCard = null;
            float totalWeight = 0f;
            for (int index = 0; index < runtimeEnemyDeck.Count; index++)
            {
                SummonData candidate = runtimeEnemyDeck[index];
                if (candidate == null || ResolveTimeUntilAffordable(candidate) > affordabilityWindow)
                {
                    continue;
                }

                totalWeight += GetCardWeight(candidate);
            }

            if (totalWeight <= 0.001f)
            {
                return false;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            for (int index = 0; index < runtimeEnemyDeck.Count; index++)
            {
                SummonData candidate = runtimeEnemyDeck[index];
                if (candidate == null || ResolveTimeUntilAffordable(candidate) > affordabilityWindow)
                {
                    continue;
                }

                roll -= GetCardWeight(candidate);
                if (roll <= 0f)
                {
                    resolvedCard = candidate;
                    return true;
                }
            }

            resolvedCard = runtimeEnemyDeck[runtimeEnemyDeck.Count - 1];
            return true;
        }

        private float GetCardWeight(SummonData card)
        {
            if (card == null)
            {
                return 0.1f;
            }

            float weight = 1f;
            switch (currentPhase)
            {
                case EnemyPhase.Opening:
                    weight += card.energyCost <= 40f ? 2f : 0.5f;
                    weight += card.summonType == SummonType.Melee || card.summonType == SummonType.Ranged ? 1f : 0f;
                    break;
                case EnemyPhase.Pressure:
                    weight += card.summonType == SummonType.Ranged ? 1.4f : 0.8f;
                    weight += card.structureDamageMultiplier > 1.2f ? 1f : 0f;
                    break;
                case EnemyPhase.Siege:
                    weight += card.summonType == SummonType.Tank ? 2f : 0f;
                    weight += card.summonType == SummonType.Support ? 1.6f : 0f;
                    weight += card.splashRadius > 0.1f ? 1.2f : 0f;
                    break;
                case EnemyPhase.FinalPush:
                    weight += card.baseDamageMultiplier > 1.1f ? 2.5f : 0.6f;
                    weight += card.structureDamageMultiplier > 1.3f ? 1.8f : 0f;
                    weight += card.summonType == SummonType.Support ? 0.3f : 0.8f;
                    break;
            }

            switch (currentBossTactic)
            {
                case BossTacticState.RearGuard:
                    weight += card.summonType == SummonType.Ranged ? 1.5f : 0f;
                    weight += card.summonType == SummonType.Tank ? 1.2f : 0f;
                    weight += card.energyCost <= 42f ? 0.8f : 0f;
                    break;
                case BossTacticState.EscortWave:
                    weight += card.summonType == SummonType.Tank ? 1.35f : 0f;
                    weight += card.summonType == SummonType.Support ? 1.25f : 0f;
                    weight += card.summonType == SummonType.Ranged ? 0.8f : 0f;
                    break;
                case BossTacticState.ContestMid:
                    weight += card.splashRadius > 0.1f ? 1.45f : 0f;
                    weight += card.summonType == SummonType.Tank ? 0.95f : 0f;
                    weight += card.summonType == SummonType.Support ? 0.65f : 0f;
                    break;
                case BossTacticState.SiegeStructure:
                    weight += card.structureDamageMultiplier > 1.25f ? 2.2f : 0f;
                    weight += card.summonType == SummonType.Tank ? 0.9f : 0f;
                    break;
                case BossTacticState.PunishOverextend:
                    weight += card.summonType == SummonType.Melee ? 1.45f : 0f;
                    weight += card.summonType == SummonType.Ranged ? 0.7f : 0f;
                    weight += card.energyCost <= 42f ? 0.8f : 0f;
                    break;
                case BossTacticState.CommitPush:
                    weight += card.baseDamageMultiplier > 1.1f ? 2.1f : 0f;
                    weight += card.summonType == SummonType.Support ? 0.85f : 0f;
                    weight += card.summonType == SummonType.Melee ? 0.8f : 0f;
                    break;
                case BossTacticState.FallBack:
                    weight += card.summonType == SummonType.Tank ? 1.4f : 0f;
                    weight += card.summonType == SummonType.Ranged ? 1.1f : 0f;
                    weight += card.summonType == SummonType.Support ? 0.75f : 0f;
                    break;
            }

            weight += GetIntentWeight(card);
            weight += GetContextWeight(card);
            weight += GetSequencingWeight(card);
            weight *= GetRepeatSuppression(card);
            weight *= ResolveEnergySelectionWeight(card);
            return Mathf.Max(0.25f, weight);
        }

        private float ResolveEnergySelectionWeight(SummonData card)
        {
            if (card == null)
            {
                return 0.2f;
            }

            float timeUntilAffordable = ResolveTimeUntilAffordable(card);
            if (timeUntilAffordable <= 0.01f)
            {
                return 1.16f;
            }

            if (timeUntilAffordable <= 1.2f)
            {
                return 0.92f;
            }

            if (timeUntilAffordable <= 2.4f)
            {
                return 0.68f;
            }

            if (timeUntilAffordable <= 4f)
            {
                return 0.42f;
            }

            return 0.22f;
        }

        private void ApplyPhaseSettings(bool forceNotify)
        {
            EnemyPhase newPhase = EvaluateCurrentPhase();
            bool phaseChanged = forceNotify || newPhase != currentPhase;
            currentPhase = newPhase;

            float modeMultiplier = BattleModeContext.CurrentMode switch
            {
                BattleMode.StoryPve => 1.06f,
                BattleMode.AsyncPvp => 0.88f,
                BattleMode.Sandbox => 1.08f,
                _ => 1f
            };

            int structureCountBaseline = Mathf.Max(1, initialStructureCount);
            int structuresDestroyed = Mathf.Max(0, structureCountBaseline - CountActiveStructures());
            float structureTempo = Mathf.Clamp01((float)structuresDestroyed / structureCountBaseline);

            switch (currentPhase)
            {
                case EnemyPhase.Opening:
                    currentSummonInterval = summonInterval * 1.02f * modeMultiplier;
                    currentProjectileInterval = projectileInterval * 1.42f * modeMultiplier;
                    currentLineProjectileInterval = lineProjectileInterval * 1.24f * modeMultiplier;
                    break;
                case EnemyPhase.Pressure:
                    currentSummonInterval = summonInterval * 0.84f * modeMultiplier;
                    currentProjectileInterval = projectileInterval * 1.08f * modeMultiplier;
                    currentLineProjectileInterval = lineProjectileInterval * 1f * modeMultiplier;
                    break;
                case EnemyPhase.Siege:
                    currentSummonInterval = summonInterval * 0.68f * modeMultiplier;
                    currentProjectileInterval = projectileInterval * 0.92f * modeMultiplier;
                    currentLineProjectileInterval = lineProjectileInterval * 0.88f * modeMultiplier;
                    break;
                case EnemyPhase.FinalPush:
                    currentSummonInterval = summonInterval * 0.5f * modeMultiplier;
                    currentProjectileInterval = projectileInterval * 0.72f * modeMultiplier;
                    currentLineProjectileInterval = lineProjectileInterval * 0.72f * modeMultiplier;
                    break;
            }

            currentSummonInterval *= Mathf.Lerp(1f, 0.8f, structureTempo);
            currentProjectileInterval *= Mathf.Lerp(1f, 0.88f, structureTempo);
            currentLineProjectileInterval *= Mathf.Lerp(1f, 0.86f, structureTempo);

            if (phaseChanged)
            {
                OnPhaseChanged?.Invoke(CurrentPhaseName);
                needsSummonReplan = true;
            }
        }

        private EnemyPhase EvaluateCurrentPhase()
        {
            BattleManager battleManager = BattleManager.Instance;
            GameManager gameManager = GameManager.Instance;
            float elapsedTime = initialBattleDuration <= 0.001f || gameManager == null
                ? 0f
                : Mathf.Max(0f, initialBattleDuration - gameManager.RemainingTime);
            int structureCountBaseline = Mathf.Max(1, initialStructureCount);
            int structuresRemaining = CountActiveStructures();
            int structuresDestroyed = Mathf.Max(0, structureCountBaseline - structuresRemaining);
            float playerBaseRatio = battleManager == null || battleManager.PlayerBaseMaxHP <= 0.001f
                ? 1f
                : battleManager.CurrentPlayerBaseHP / battleManager.PlayerBaseMaxHP;
            float enemyBaseRatio = battleManager == null || battleManager.EnemyBaseMaxHP <= 0.001f
                ? 1f
                : battleManager.CurrentEnemyBaseHP / battleManager.EnemyBaseMaxHP;
            bool earlySiegeTrigger = structuresDestroyed >= 2 || playerBaseRatio <= 0.78f || enemyBaseRatio <= 0.78f;
            bool finalPushTrigger = structuresRemaining <= 2 || playerBaseRatio <= 0.4f || enemyBaseRatio <= 0.4f;

            if (elapsedTime < openingMinimumDuration && structuresDestroyed == 0)
            {
                return EnemyPhase.Opening;
            }

            if (elapsedTime < pressureMinimumDuration && !earlySiegeTrigger)
            {
                return EnemyPhase.Pressure;
            }

            if (elapsedTime < siegeMinimumDuration && !finalPushTrigger)
            {
                return EnemyPhase.Siege;
            }

            return EnemyPhase.FinalPush;
        }

        private static int CountActiveStructures()
        {
            return BattleStructure.ActiveCount;
        }

        private void UpdateSummonIntent(bool forceNotify)
        {
            SummonIntentState newIntent = ResolveSummonIntentState(forceNotify, out float intentScore);
            bool intentChanged = forceNotify || newIntent != currentSummonIntent;
            currentSummonIntent = newIntent;
            currentSummonIntentScore = intentScore;
            if (!intentChanged)
            {
                return;
            }

            nextSummonIntentShiftAllowedTime = Time.time + ResolveSummonIntentCommitDuration(currentSummonIntent, currentSummonIntentScore);
            ClearWaveLaneAnchor();
            needsSummonReplan = true;
            OnSummonIntentChanged?.Invoke(CurrentSummonIntentName);
        }

        private void TryReplanNextSummon()
        {
            if (!needsSummonReplan || runtimeEnemyDeck.Count == 0)
            {
                return;
            }

            if (nextSummon != null && summonTimer <= 0.35f)
            {
                return;
            }

            DecideNextSummon();
        }

        private SummonIntentState ResolveSummonIntentState(bool forceNotify, out float resolvedScore)
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null || playerController == null)
            {
                resolvedScore = 1f;
                return currentPhase == EnemyPhase.Opening ? SummonIntentState.Probe : SummonIntentState.HoldLine;
            }

            BossDecisionContext context = currentBossDecisionContext;
            SummonIntentState bestIntent = currentSummonIntent;
            float bestScore = float.MinValue;
            float currentScore = float.MinValue;

            for (int index = 0; index < SummonIntents.Length; index++)
            {
                SummonIntentState intent = SummonIntents[index];
                float score = ScoreSummonIntent(intent, context);
                if (intent == currentSummonIntent)
                {
                    currentScore = score;
                }

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestIntent = intent;
            }

            if (forceNotify)
            {
                resolvedScore = bestScore;
                return bestIntent;
            }

            if (bestIntent == currentSummonIntent)
            {
                resolvedScore = currentScore;
                return currentSummonIntent;
            }

            float swapThreshold = summonIntentSwapThreshold;
            if (Time.time < nextSummonIntentShiftAllowedTime)
            {
                swapThreshold += summonIntentStickyBias;
            }

            if (currentBossTactic == BossTacticState.CommitPush || currentBossTactic == BossTacticState.PunishOverextend)
            {
                swapThreshold -= 0.1f;
            }

            if ((bestScore - currentScore) < swapThreshold)
            {
                resolvedScore = currentScore;
                return currentSummonIntent;
            }

            resolvedScore = bestScore;
            return bestIntent;
        }

        private float ScoreSummonIntent(SummonIntentState intent, BossDecisionContext context)
        {
            bool followThroughWindow = projectileTimer <= Mathf.Max(0.55f, currentProjectileInterval * 0.34f)
                || lineProjectileTimer <= Mathf.Max(0.65f, currentLineProjectileInterval * 0.36f);
            bool isOpening = currentPhase == EnemyPhase.Opening;
            bool isFinalPush = currentPhase == EnemyPhase.FinalPush;

            float score = intent switch
            {
                SummonIntentState.Probe => 0.85f
                    + ((1f - context.WaveSupport01) * 1.95f)
                    + ((1f - context.CloseOut01) * 0.65f)
                    + (isOpening ? 0.95f : 0f)
                    - (context.HeroPressure01 * 0.7f)
                    - (context.StructureOpportunity01 * 0.42f),
                SummonIntentState.HoldLine => 0.8f
                    + (context.BackFoot01 * 2.35f)
                    + (context.ClashPressure01 * 1.15f)
                    + ((1f - context.WaveSupport01) * 0.85f)
                    + (currentBossTactic == BossTacticState.FallBack ? 0.85f : 0f),
                SummonIntentState.EscortPush => 0.8f
                    + (context.WaveSupport01 * 2.2f)
                    + (context.EnemyAdvantage * 0.88f)
                    + (followThroughWindow ? 0.42f : 0f)
                    + (currentBossTactic == BossTacticState.EscortWave ? 0.75f : currentBossTactic == BossTacticState.ContestMid ? 0.3f : 0f)
                    - (context.BackFoot01 * 0.35f),
                SummonIntentState.BreakPost => 0.72f
                    + (context.StructureOpportunity01 * 3.05f)
                    + (context.EnemyAdvantage * 0.82f)
                    + (currentBossTactic == BossTacticState.SiegeStructure ? 0.92f : 0f)
                    - (context.HeroPressure01 * 0.36f),
                SummonIntentState.PunishHero => 0.72f
                    + (context.HeroPressure01 * 3.2f)
                    + (context.WaveSupport01 * 0.7f)
                    + (currentBossTactic == BossTacticState.PunishOverextend ? 0.96f : 0f)
                    - (context.CloseOut01 * 0.25f),
                SummonIntentState.BaseRush => 0.7f
                    + (context.CloseOut01 * 3.1f)
                    + (context.EnemyAdvantage * 0.96f)
                    + (context.WaveSupport01 * 0.8f)
                    + (currentBossTactic == BossTacticState.CommitPush ? 0.95f : 0f)
                    + (isFinalPush ? 0.7f : 0f)
                    - (context.BackFoot01 * 0.8f),
                _ => 0f
            };

            if (!context.HasWave)
            {
                if (intent == SummonIntentState.Probe || intent == SummonIntentState.HoldLine)
                {
                    score += 0.38f;
                }
                else
                {
                    score -= 0.8f;
                }
            }

            if (!context.HasPriorityStructure && intent == SummonIntentState.BreakPost)
            {
                score -= 2.3f;
            }

            if (context.HeroPressure01 < 0.24f && intent == SummonIntentState.PunishHero)
            {
                score -= 1.1f;
            }

            if (context.CloseOut01 < 0.26f && intent == SummonIntentState.BaseRush && !isFinalPush)
            {
                score -= 0.95f;
            }

            if (context.EnemyAdvantage < 0.4f && (intent == SummonIntentState.BaseRush || intent == SummonIntentState.BreakPost))
            {
                score -= 0.7f;
            }

            if (isOpening)
            {
                if (intent == SummonIntentState.BreakPost || intent == SummonIntentState.BaseRush)
                {
                    score -= 1.2f;
                }

                if (intent == SummonIntentState.Probe)
                {
                    score += 0.25f;
                }
            }

            return score;
        }

        private float ResolveSummonIntentCommitDuration(SummonIntentState intent, float intentScore)
        {
            float confidence = Mathf.InverseLerp(1.2f, 5f, intentScore);
            float baseDuration = intent switch
            {
                SummonIntentState.PunishHero => summonIntentMinimumCommit,
                SummonIntentState.BaseRush => Mathf.Lerp(summonIntentMinimumCommit + 0.08f, summonIntentMaximumCommit, 0.8f),
                SummonIntentState.BreakPost => Mathf.Lerp(summonIntentMinimumCommit, summonIntentMaximumCommit, 0.72f),
                SummonIntentState.HoldLine => Mathf.Lerp(summonIntentMinimumCommit + 0.06f, summonIntentMaximumCommit, 0.6f),
                _ => Mathf.Lerp(summonIntentMinimumCommit, summonIntentMaximumCommit, 0.5f)
            };

            return Mathf.Lerp(baseDuration, summonIntentMaximumCommit, confidence * 0.38f);
        }

        private float GetIntentWeight(SummonData card)
        {
            if (card == null)
            {
                return 0f;
            }

            bool isRush = IsRushUnit(card);
            bool isBreaker = IsStructureBreaker(card);
            bool isLane = IsLaneControl(card);
            bool isSplash = IsSplashCaster(card);
            bool isTank = card.summonType == SummonType.Tank;
            bool isSupport = card.summonType == SummonType.Support;
            bool isBaseRush = IsBasePressurer(card);
            bool isCheap = card.energyCost <= 40f;

            return currentSummonIntent switch
            {
                SummonIntentState.Probe => (isRush ? 2.7f : 0f) + (isLane ? 1.9f : 0f) + (isCheap ? 1.6f : 0.4f) + (isTank ? 0.7f : 0f) + (isSupport ? -0.8f : 0f),
                SummonIntentState.HoldLine => (isTank ? 3.2f : 0f) + (isLane ? 2.4f : 0f) + (isSplash ? 2.1f : 0f) + (isCheap ? 1.1f : 0f) + (isSupport ? -0.4f : 0f),
                SummonIntentState.EscortPush => (isTank ? 2.5f : 0f) + (isSupport ? 2.7f : 0f) + (isLane ? 1.8f : 0f) + (isBaseRush ? 0.9f : 0f),
                SummonIntentState.BreakPost => (isBreaker ? 3.3f : 0f) + (isTank ? 2.2f : 0f) + (isSplash ? 1.7f : 0f) + (isSupport ? 0.5f : 0f),
                SummonIntentState.PunishHero => (card.summonType == SummonType.Melee ? 2.4f : 0f) + (isRush ? 2.2f : 0f) + (isLane ? 1.7f : 0f) + (isCheap ? 1.2f : 0f) + (isSupport ? -0.7f : 0f),
                SummonIntentState.BaseRush => (isBaseRush ? 3.4f : 0f) + (isBreaker ? 2.2f : 0f) + (isRush ? 2f : 0f) + (isLane ? 0.9f : 0f) + (isSupport ? -1.2f : 0f) + (isTank ? -0.4f : 0f),
                _ => 0f
            };
        }

        private float GetContextWeight(SummonData card)
        {
            if (card == null)
            {
                return 0f;
            }

            float weight = 0f;
            bool isRush = IsRushUnit(card);
            bool isBreaker = IsStructureBreaker(card);
            bool isLane = IsLaneControl(card);
            bool isSplash = IsSplashCaster(card);
            bool isTank = card.summonType == SummonType.Tank;
            bool isSupport = card.summonType == SummonType.Support;
            bool isBaseRush = IsBasePressurer(card);
            bool isCheap = card.energyCost <= 40f;

            if (currentBossDecisionContext.BackFoot01 >= 0.52f)
            {
                weight += isTank ? 1f : 0f;
                weight += isLane ? 0.85f : 0f;
                weight += isSplash ? 0.55f : 0f;
                weight += isSupport && currentBossDecisionContext.HasWave ? 0.2f : 0f;
            }

            if (currentBossDecisionContext.StructureOpportunity01 >= 0.38f)
            {
                weight += isBreaker ? 1.05f : 0f;
                weight += isTank ? 0.45f : 0f;
            }

            if (currentBossDecisionContext.HeroPressure01 >= 0.36f)
            {
                weight += isRush ? 0.95f : 0f;
                weight += card.summonType == SummonType.Melee ? 0.55f : 0f;
                weight += isCheap ? 0.45f : 0f;
                weight -= isSupport ? 0.4f : 0f;
            }

            if (currentBossDecisionContext.CloseOut01 >= 0.34f)
            {
                weight += isBaseRush ? 1.1f : 0f;
                weight += isBreaker ? 0.55f : 0f;
                weight += isSupport && currentBossDecisionContext.HasWave ? 0.2f : 0f;
            }

            if (!currentBossDecisionContext.HasWave)
            {
                weight -= isSupport ? 0.95f : 0f;
            }

            return weight;
        }

        private float GetSequencingWeight(SummonData card)
        {
            if (card == null || lastSummonData == null)
            {
                return 0f;
            }

            bool isRush = IsRushUnit(card);
            bool isBreaker = IsStructureBreaker(card);
            bool isLane = IsLaneControl(card);
            bool isSplash = IsSplashCaster(card);
            bool isTank = card.summonType == SummonType.Tank;
            bool isSupport = card.summonType == SummonType.Support;
            bool isMelee = card.summonType == SummonType.Melee;
            bool isRanged = card.summonType == SummonType.Ranged;
            bool isCheap = card.energyCost <= 40f;

            bool lastWasTank = lastSummonType == SummonType.Tank;
            bool lastWasSupport = lastSummonType == SummonType.Support;
            bool lastWasMelee = lastSummonType == SummonType.Melee;
            bool lastWasRanged = lastSummonType == SummonType.Ranged;
            bool lastWasBreaker = IsStructureBreaker(lastSummonData);
            bool lastWasRush = IsRushUnit(lastSummonData);
            float weight = 0f;

            switch (currentSummonIntent)
            {
                case SummonIntentState.Probe:
                    if (lastWasRush)
                    {
                        weight += isLane ? 0.82f : 0f;
                        weight += isTank ? 0.42f : 0f;
                        weight -= isSupport ? 0.4f : 0f;
                    }
                    else if (lastWasRanged)
                    {
                        weight += isRush ? 0.58f : 0f;
                    }

                    break;
                case SummonIntentState.HoldLine:
                    if (lastWasTank)
                    {
                        weight += isLane ? 1.15f : 0f;
                        weight += isSplash ? 0.82f : 0f;
                        weight += isSupport ? 0.58f : 0f;
                    }
                    else if (lastWasRanged)
                    {
                        weight += isTank ? 0.92f : 0f;
                        weight += isMelee ? 0.35f : 0f;
                    }

                    break;
                case SummonIntentState.EscortPush:
                    if (lastWasTank || lastWasMelee)
                    {
                        weight += isSupport ? 1.28f : 0f;
                        weight += isLane ? 1.08f : 0f;
                        weight += isBreaker ? 0.44f : 0f;
                    }
                    else if (lastWasSupport)
                    {
                        weight += isTank ? 0.82f : 0f;
                        weight += isRush ? 0.62f : 0f;
                    }

                    break;
                case SummonIntentState.BreakPost:
                    if (lastWasTank || lastWasBreaker)
                    {
                        weight += isBreaker ? 1.12f : 0f;
                        weight += isLane ? 0.92f : 0f;
                        weight += isSupport ? 0.66f : 0f;
                    }
                    else if (lastWasSupport)
                    {
                        weight += isBreaker ? 0.8f : 0f;
                        weight += isTank ? 0.52f : 0f;
                    }

                    break;
                case SummonIntentState.PunishHero:
                    if (lastWasRush || lastWasMelee)
                    {
                        weight += isCheap ? 0.72f : 0f;
                        weight += isMelee ? 0.96f : 0f;
                        weight += isRanged ? 0.78f : 0f;
                        weight -= isSupport ? 0.76f : 0f;
                    }
                    else if (lastWasRanged)
                    {
                        weight += isRush ? 0.9f : 0f;
                        weight += isMelee ? 0.52f : 0f;
                    }

                    break;
                case SummonIntentState.BaseRush:
                    if (lastWasTank || lastWasMelee || lastWasBreaker)
                    {
                        weight += isBreaker ? 1f : 0f;
                        weight += isRush ? 0.86f : 0f;
                        weight += isSupport ? 0.42f : 0f;
                    }
                    else if (lastWasSupport)
                    {
                        weight += isBreaker ? 0.86f : 0f;
                        weight += isMelee ? 0.74f : 0f;
                    }

                    break;
            }

            return weight;
        }

        private float ResolveNextSummonDelay(SummonData spawnedSummon)
        {
            float delay = currentSummonInterval;
            if (spawnedSummon == null)
            {
                return delay;
            }

            bool isRush = IsRushUnit(spawnedSummon);
            bool isBreaker = IsStructureBreaker(spawnedSummon);
            bool isTank = spawnedSummon.summonType == SummonType.Tank;
            bool isRanged = spawnedSummon.summonType == SummonType.Ranged;
            bool isSupport = spawnedSummon.summonType == SummonType.Support;
            float confidence = Mathf.Max(CurrentBossConfidence01, CurrentSummonConfidence01);
            float multiplier = 1f;

            switch (currentSummonIntent)
            {
                case SummonIntentState.Probe:
                    multiplier = isRush ? 0.78f : isRanged ? 0.9f : 1.02f;
                    break;
                case SummonIntentState.HoldLine:
                    multiplier = isTank ? 0.82f : isRanged ? 0.9f : isSupport ? 0.98f : 1.08f;
                    break;
                case SummonIntentState.EscortPush:
                    multiplier = isTank || isRush ? 0.68f : isSupport ? 0.88f : isRanged ? 0.8f : 0.9f;
                    break;
                case SummonIntentState.BreakPost:
                    multiplier = isBreaker ? 0.62f : isTank ? 0.72f : isSupport ? 0.92f : 0.84f;
                    break;
                case SummonIntentState.PunishHero:
                    multiplier = isRush ? 0.66f : spawnedSummon.summonType == SummonType.Melee ? 0.72f : isRanged ? 0.86f : 1.08f;
                    break;
                case SummonIntentState.BaseRush:
                    multiplier = isBreaker ? 0.58f : isRush ? 0.62f : isTank ? 0.74f : isSupport ? 0.9f : 0.82f;
                    break;
            }

            if (!currentBossDecisionContext.HasWave && (isTank || isRush || isBreaker))
            {
                multiplier *= 0.88f;
            }

            if (currentBossTactic == BossTacticState.CommitPush || currentBossTactic == BossTacticState.PunishOverextend)
            {
                multiplier = Mathf.Lerp(multiplier, multiplier * 0.85f, confidence * 0.7f);
            }
            else if (currentBossTactic == BossTacticState.FallBack)
            {
                multiplier = Mathf.Lerp(multiplier, multiplier * 1.12f, 0.65f - (confidence * 0.18f));
            }

            if (isSupport && currentBossDecisionContext.HasWave)
            {
                multiplier *= 0.94f;
            }

            float minDelay = currentPhase == EnemyPhase.FinalPush ? 0.68f : currentPhase == EnemyPhase.Siege ? 0.78f : 0.9f;
            return Mathf.Clamp(delay * multiplier, minDelay, currentSummonInterval * 1.24f);
        }

        private float GetRepeatSuppression(SummonData card)
        {
            if (card == null || lastSummonData == null)
            {
                return 1f;
            }

            float suppression = 1f;
            if (card == lastSummonData)
            {
                suppression *= repeatCardStreak >= 2 ? 0.24f : 0.4f;
            }

            if (card.summonType == lastSummonType)
            {
                suppression *= repeatTypeStreak >= 3
                    ? 0.42f
                    : repeatTypeStreak >= 2 ? 0.56f : 0.74f;
            }

            return suppression;
        }

        private void UpdateSummonHistory(SummonData summonData)
        {
            if (summonData == null)
            {
                return;
            }

            repeatCardStreak = summonData == lastSummonData ? repeatCardStreak + 1 : 1;
            repeatTypeStreak = lastSummonData != null && summonData.summonType == lastSummonType ? repeatTypeStreak + 1 : 1;
            lastSummonData = summonData;
            lastSummonType = summonData.summonType;
        }

        private float ResolveProjectileDamage()
        {
            float phaseBonus = currentPhase switch
            {
                EnemyPhase.Opening => 0f,
                EnemyPhase.Pressure => 2f,
                EnemyPhase.Siege => 4f,
                EnemyPhase.FinalPush => 7f,
                _ => 0f
            };

            return projectileDamage + phaseBonus;
        }

        private float ResolveLineProjectileDamage()
        {
            float phaseBonus = currentPhase switch
            {
                EnemyPhase.Opening => 0f,
                EnemyPhase.Pressure => 2f,
                EnemyPhase.Siege => 4f,
                EnemyPhase.FinalPush => 6f,
                _ => 0f
            };

            return lineProjectileDamage + phaseBonus;
        }

        private float ResolveLineProjectileBaseDamage()
        {
            return lineProjectileBaseDamage + (currentPhase == EnemyPhase.FinalPush ? 4f : currentPhase == EnemyPhase.Siege ? 2f : 0f);
        }

        private float ResolveLineProjectileStructureDamageMultiplier()
        {
            float tacticBonus = currentSummonIntent == SummonIntentState.BreakPost || currentBossTactic == BossTacticState.SiegeStructure ? 0.3f : 0f;
            float phaseBonus = currentPhase >= EnemyPhase.Siege ? 0.1f : 0f;
            return lineProjectileStructureDamageMultiplier + tacticBonus + phaseBonus;
        }

        private Vector3 GetFormationOffset(int spawnIndex)
        {
            float xOffset = ((spawnIndex % 3) - 1) * laneOffsetSpacing;
            float zOffset = (spawnIndex % 2) * -forwardOffsetSpacing;
            return new Vector3(xOffset, 0f, zOffset);
        }

        private Vector3 ResolveSummonSpawnOffset(SummonData summonData, int spawnIndex)
        {
            Vector3 formationOffset = GetFormationOffset(spawnIndex);
            float[] lanes = GetProjectileLaneAnchors();
            float laneHalfWidth = BattleManager.Instance != null ? BattleManager.Instance.LaneHalfWidth : 5.75f;
            int centerLaneIndex = lanes.Length / 2;
            int playerLaneIndex = playerController != null ? GetNearestLaneIndex(playerController.transform.position.x, lanes) : centerLaneIndex;
            int escortLaneIndex = GetNearestLaneIndex(currentBossFormation.EnemyAnchorX, lanes);
            int clashLaneIndex = GetNearestLaneIndex(currentBossFormation.ClashAnchorX, lanes);
            int structureLaneIndex = currentPriorityStructure != null
                ? GetNearestLaneIndex(currentPriorityStructure.transform.position.x, lanes)
                : clashLaneIndex;

            int targetLaneIndex = ResolveSummonLaneIndex(summonData, lanes, playerLaneIndex, escortLaneIndex, clashLaneIndex, structureLaneIndex, centerLaneIndex);
            lastResolvedSummonLaneIndex = targetLaneIndex;
            float laneAnchorX = lanes[targetLaneIndex];
            float anchoredX = Mathf.Clamp(laneAnchorX + (formationOffset.x * 0.42f), -laneHalfWidth * 0.74f, laneHalfWidth * 0.74f);

            float zOffset = formationOffset.z * 0.6f;
            zOffset += summonData != null ? summonData.summonType switch
            {
                SummonType.Melee => -0.42f,
                SummonType.Tank => -0.2f,
                SummonType.Ranged => 0.22f,
                SummonType.Support => 0.52f,
                _ => 0f
            } : 0f;

            if (currentBossTactic == BossTacticState.CommitPush || currentSummonIntent == SummonIntentState.BaseRush)
            {
                zOffset -= summonData != null && summonData.summonType == SummonType.Melee ? 0.35f : 0.18f;
            }
            else if (currentBossTactic == BossTacticState.FallBack || currentSummonIntent == SummonIntentState.HoldLine)
            {
                zOffset += summonData != null && summonData.summonType == SummonType.Support ? 0.16f : 0.32f;
            }

            return new Vector3(anchoredX, 0f, zOffset);
        }

        private int ResolveSummonLaneIndex(
            SummonData summonData,
            float[] lanes,
            int playerLaneIndex,
            int escortLaneIndex,
            int clashLaneIndex,
            int structureLaneIndex,
            int centerLaneIndex)
        {
            bool isSupport = summonData != null && summonData.summonType == SummonType.Support;
            bool isTank = summonData != null && summonData.summonType == SummonType.Tank;
            bool isRanged = summonData != null && summonData.summonType == SummonType.Ranged;
            bool isBreaker = IsStructureBreaker(summonData);
            bool isBaseRush = IsBasePressurer(summonData);
            int playerPressureLane = ResolveTowardCenterLane(playerLaneIndex, centerLaneIndex, ResolvePressureSide(playerLaneIndex, centerLaneIndex));
            int escortSupportLane = ResolveTowardCenterLane(escortLaneIndex, centerLaneIndex, escortLaneIndex < centerLaneIndex ? -1 : 1);
            int structureSupportLane = ResolveTowardCenterLane(structureLaneIndex, centerLaneIndex, structureLaneIndex < centerLaneIndex ? -1 : 1);

            if (currentBossTactic == BossTacticState.SiegeStructure || currentSummonIntent == SummonIntentState.BreakPost)
            {
                if (isBreaker || isTank)
                {
                    return ApplyWaveLaneAnchor(structureLaneIndex, summonData, lanes.Length, centerLaneIndex);
                }

                int structureLane = isSupport || isRanged ? structureSupportLane : structureLaneIndex;
                return ApplyWaveLaneAnchor(structureLane, summonData, lanes.Length, centerLaneIndex);
            }

            if (currentBossTactic == BossTacticState.PunishOverextend || currentSummonIntent == SummonIntentState.PunishHero)
            {
                if (isSupport)
                {
                    return ApplyWaveLaneAnchor(playerPressureLane, summonData, lanes.Length, centerLaneIndex);
                }

                int punishLane = isTank ? playerPressureLane : playerLaneIndex;
                return ApplyWaveLaneAnchor(punishLane, summonData, lanes.Length, centerLaneIndex);
            }

            if (currentBossTactic == BossTacticState.CommitPush || currentSummonIntent == SummonIntentState.BaseRush)
            {
                if (isTank)
                {
                    return ApplyWaveLaneAnchor(clashLaneIndex, summonData, lanes.Length, centerLaneIndex);
                }

                if (isSupport || isRanged)
                {
                    return ApplyWaveLaneAnchor(playerPressureLane, summonData, lanes.Length, centerLaneIndex);
                }

                int rushLane = isBaseRush ? playerLaneIndex : clashLaneIndex;
                return ApplyWaveLaneAnchor(rushLane, summonData, lanes.Length, centerLaneIndex);
            }

            if (currentBossTactic == BossTacticState.EscortWave || currentSummonIntent == SummonIntentState.EscortPush)
            {
                if (isSupport || isRanged)
                {
                    return ApplyWaveLaneAnchor(escortSupportLane, summonData, lanes.Length, centerLaneIndex);
                }

                return ApplyWaveLaneAnchor(escortLaneIndex, summonData, lanes.Length, centerLaneIndex);
            }

            if (currentBossTactic == BossTacticState.FallBack || currentSummonIntent == SummonIntentState.HoldLine)
            {
                if (isSupport)
                {
                    return ApplyWaveLaneAnchor(escortLaneIndex, summonData, lanes.Length, centerLaneIndex);
                }

                int holdLane = isTank || isRanged ? clashLaneIndex : playerPressureLane;
                return ApplyWaveLaneAnchor(holdLane, summonData, lanes.Length, centerLaneIndex);
            }

            if (isSupport)
            {
                return ApplyWaveLaneAnchor(escortLaneIndex, summonData, lanes.Length, centerLaneIndex);
            }

            if (isRanged)
            {
                return ApplyWaveLaneAnchor(clashLaneIndex, summonData, lanes.Length, centerLaneIndex);
            }

            int fallbackLane = currentBossDecisionContext.HeroPressure01 >= 0.36f ? playerLaneIndex : clashLaneIndex;
            return ApplyWaveLaneAnchor(fallbackLane, summonData, lanes.Length, centerLaneIndex);
        }

        private int ApplyWaveLaneAnchor(int baseLaneIndex, SummonData summonData, int laneCount, int centerLaneIndex)
        {
            if (waveLaneAnchorIndex < 0 || Time.time >= waveLaneAnchorExpireTime)
            {
                return Mathf.Clamp(baseLaneIndex, 0, laneCount - 1);
            }

            if (currentSummonIntent == SummonIntentState.Probe && CurrentSummonConfidence01 < 0.4f)
            {
                return Mathf.Clamp(baseLaneIndex, 0, laneCount - 1);
            }

            int clampedAnchor = Mathf.Clamp(waveLaneAnchorIndex, 0, laneCount - 1);
            bool isSupport = summonData != null && summonData.summonType == SummonType.Support;
            bool isRanged = summonData != null && summonData.summonType == SummonType.Ranged;
            bool isTank = summonData != null && summonData.summonType == SummonType.Tank;
            bool useStrictAnchor = currentSummonIntent == SummonIntentState.BreakPost
                || currentSummonIntent == SummonIntentState.PunishHero
                || currentSummonIntent == SummonIntentState.BaseRush;

            if (isSupport || isRanged)
            {
                int supportLane = ResolveTowardCenterLane(clampedAnchor, centerLaneIndex, clampedAnchor < centerLaneIndex ? -1 : 1);
                return Mathf.Clamp(useStrictAnchor ? supportLane : Mathf.RoundToInt(Mathf.Lerp(baseLaneIndex, supportLane, 0.78f)), 0, laneCount - 1);
            }

            if (useStrictAnchor || isTank)
            {
                return clampedAnchor;
            }

            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(baseLaneIndex, clampedAnchor, 0.7f)), 0, laneCount - 1);
        }

        private void UpdateWaveLaneAnchor(SummonData summonData, int laneIndex)
        {
            if (laneIndex < 0)
            {
                return;
            }

            float confidence = Mathf.Max(CurrentBossConfidence01, CurrentSummonConfidence01);
            if (confidence < 0.18f && currentSummonIntent == SummonIntentState.Probe)
            {
                ClearWaveLaneAnchor();
                return;
            }

            float duration = currentSummonIntent switch
            {
                SummonIntentState.Probe => 0.72f,
                SummonIntentState.HoldLine => 1.05f,
                SummonIntentState.EscortPush => 1.55f,
                SummonIntentState.BreakPost => 1.7f,
                SummonIntentState.PunishHero => 1.28f,
                SummonIntentState.BaseRush => 1.6f,
                _ => 1f
            };

            if (summonData != null && summonData.summonType == SummonType.Support)
            {
                duration += 0.12f;
            }

            waveLaneAnchorIndex = laneIndex;
            waveLaneAnchorExpireTime = Time.time + Mathf.Lerp(duration * 0.82f, duration * 1.12f, confidence);
        }

        private void ClearWaveLaneAnchor()
        {
            waveLaneAnchorIndex = -1;
            waveLaneAnchorExpireTime = 0f;
        }

        private void BuildProjectilePattern(List<ProjectileLaneShot> shots)
        {
            shots.Clear();
            float[] lanes = GetProjectileLaneAnchors();
            int centerLaneIndex = lanes.Length / 2;
            int playerLaneIndex = GetNearestLaneIndex(playerController.transform.position.x, lanes);
            int escortLaneIndex = GetNearestLaneIndex(currentBossFormation.EnemyAnchorX, lanes);
            int clashLaneIndex = GetNearestLaneIndex(currentBossFormation.ClashAnchorX, lanes);
            int pressureSide = ResolvePressureSide(playerLaneIndex, centerLaneIndex);
            int sideInnerLane = pressureSide < 0 ? 1 : 3;
            int sideOuterLane = pressureSide < 0 ? 0 : 4;
            int oppositeInnerLane = pressureSide < 0 ? 3 : 1;
            string volleyPatternName = currentVolleyPatternName;

            if (currentBossTactic == BossTacticState.SiegeStructure)
            {
                AddLaneShot(shots, lanes, playerLaneIndex, CreateMeteorProfile());
                AddLaneShot(shots, lanes, ResolveTowardCenterLane(playerLaneIndex, centerLaneIndex, pressureSide), CreateNeedleProfile(0.94f));
                AddLaneShot(shots, lanes, centerLaneIndex, CreateNeedleProfile(0.9f));
                volleyPatternName = "Anchor Curse";
            }
            else if (currentBossTactic == BossTacticState.PunishOverextend)
            {
                AddLaneShot(shots, lanes, playerLaneIndex, CreateOrbProfile());
                AddLaneShot(shots, lanes, Mathf.Max(0, playerLaneIndex - 1), CreateNeedleProfile(0.95f));
                AddLaneShot(shots, lanes, Mathf.Min(lanes.Length - 1, playerLaneIndex + 1), CreateNeedleProfile(0.95f));
                volleyPatternName = "Punish Net";
            }
            else if (currentBossTactic == BossTacticState.RearGuard)
            {
                AddLaneShot(shots, lanes, clashLaneIndex, CreateNeedleProfile(0.92f));
                AddLaneShot(shots, lanes, centerLaneIndex, CreateNeedleProfile(0.9f));
                volleyPatternName = "Cover Fire";
            }
            else if (currentBossTactic == BossTacticState.EscortWave)
            {
                AddLaneShot(shots, lanes, escortLaneIndex, CreateOrbProfile());
                AddLaneShot(shots, lanes, ResolveTowardCenterLane(escortLaneIndex, centerLaneIndex, escortLaneIndex < centerLaneIndex ? -1 : 1), CreateNeedleProfile());
                volleyPatternName = "Escort Screen";
            }
            else
            {
                switch (currentPhase)
                {
                    case EnemyPhase.Opening:
                        AddLaneShot(shots, lanes, playerLaneIndex, CreateNeedleProfile());
                        volleyPatternName = "Needle Lock";
                        break;
                    case EnemyPhase.Pressure:
                        AddLaneShot(shots, lanes, playerLaneIndex, CreateOrbProfile());
                        AddLaneShot(shots, lanes, ResolveTowardCenterLane(playerLaneIndex, centerLaneIndex, pressureSide), CreateNeedleProfile());
                        volleyPatternName = pressureSide < 0 ? "Left Clamp" : "Right Clamp";
                        break;
                    case EnemyPhase.Siege:
                        AddLaneShot(shots, lanes, sideOuterLane, CreateNeedleProfile());
                        AddLaneShot(shots, lanes, sideInnerLane, CreateOrbProfile());
                        AddLaneShot(shots, lanes, centerLaneIndex, CreateMeteorProfile());
                        volleyPatternName = pressureSide < 0 ? "Left Wall + Core" : "Right Wall + Core";
                        break;
                    case EnemyPhase.FinalPush:
                        AddLaneShot(shots, lanes, sideOuterLane, CreateNeedleProfile());
                        AddLaneShot(shots, lanes, sideInnerLane, CreateOrbProfile());
                        AddLaneShot(shots, lanes, centerLaneIndex, CreateMeteorProfile());
                        AddLaneShot(shots, lanes, oppositeInnerLane, CreateNeedleProfile(1.08f));
                        volleyPatternName = pressureSide < 0 ? "Left Crush" : "Right Crush";
                        break;
                }
            }

            if (!string.Equals(currentVolleyPatternName, volleyPatternName, StringComparison.Ordinal))
            {
                currentVolleyPatternName = volleyPatternName;
                OnVolleyPatternChanged?.Invoke(currentVolleyPatternName);
            }
        }

        private Vector3 ResolveProjectileSpawnPosition(float targetX, int shotIndex, int shotCount)
        {
            float targetBiasOffset = Mathf.Clamp(targetX * 0.18f, -1.25f, 1.25f);
            float formationOffset = shotCount <= 1 ? 0f : (shotIndex - ((shotCount - 1) * 0.5f)) * 0.28f;
            return projectileSpawnPoint.position + (transform.right * (targetBiasOffset + formationOffset)) + (Vector3.up * heroPatternSpawnHeight);
        }

        private Vector3 ResolveProjectileDirection(Vector3 spawnPosition, float targetX)
        {
            float playerPlaneZ = playerController.transform.position.z + heroPatternTargetForwardOffset;
            Vector3 targetPosition = new(targetX, ResolveProjectileTargetHeight(), playerPlaneZ);
            Vector3 direction = targetPosition - spawnPosition;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.back;
        }

        private float ResolveProjectileTargetHeight()
        {
            if (playerController == null)
            {
                return 1.2f;
            }

            Collider playerCollider = playerController.GetComponent<Collider>();
            if (playerCollider != null)
            {
                return Mathf.Lerp(playerCollider.bounds.center.y, playerCollider.bounds.max.y, 0.35f);
            }

            return playerController.transform.position.y + 1.1f;
        }

        private void BuildLineVolleyPattern(List<LineVolleyShot> shots)
        {
            shots.Clear();
            float[] lanes = GetProjectileLaneAnchors();
            int centerLaneIndex = lanes.Length / 2;
            int playerLaneIndex = playerController != null ? GetNearestLaneIndex(playerController.transform.position.x, lanes) : centerLaneIndex;
            int escortLaneIndex = GetNearestLaneIndex(currentBossFormation.EnemyAnchorX, lanes);
            int clashLaneIndex = GetNearestLaneIndex(currentBossFormation.ClashAnchorX, lanes);
            int structureLaneIndex = currentPriorityStructure != null
                ? GetNearestLaneIndex(currentPriorityStructure.transform.position.x, lanes)
                : clashLaneIndex;
            int pressureSide = ResolvePressureSide(playerLaneIndex, centerLaneIndex);
            int pressureAdjacentLane = ResolveTowardCenterLane(playerLaneIndex, centerLaneIndex, pressureSide);
            int escortSupportLane = ResolveTowardCenterLane(escortLaneIndex, centerLaneIndex, escortLaneIndex < centerLaneIndex ? -1 : 1);

            if (currentBossTactic == BossTacticState.SiegeStructure && currentPriorityStructure != null)
            {
                AddLineShot(shots, lanes, structureLaneIndex, CreateLineBreakerProfile(1.05f));
                AddLineShot(shots, lanes, ResolveTowardCenterLane(structureLaneIndex, centerLaneIndex, structureLaneIndex < centerLaneIndex ? -1 : 1), CreateLineOrbProfile(1f));
                if (currentBossDecisionContext.HeroPressure01 >= 0.34f)
                {
                    AddLineShot(shots, lanes, playerLaneIndex, CreateLineNeedleProfile(0.96f));
                }

                return;
            }

            if (currentBossTactic == BossTacticState.PunishOverextend)
            {
                AddLineShot(shots, lanes, playerLaneIndex, CreateLineNeedleProfile(1.1f));
                AddLineShot(shots, lanes, pressureAdjacentLane, CreateLineNeedleProfile(0.96f));
                if (currentBossDecisionContext.WaveSupport01 >= 0.42f)
                {
                    AddLineShot(shots, lanes, centerLaneIndex, CreateLineOrbProfile(0.98f));
                }

                return;
            }

            if (currentBossTactic == BossTacticState.CommitPush)
            {
                AddLineShot(shots, lanes, playerLaneIndex, CreateLineOrbProfile(1.08f));
                AddLineShot(shots, lanes, centerLaneIndex, CreateLineBreakerProfile(0.96f));
                AddLineShot(shots, lanes, pressureAdjacentLane, CreateLineNeedleProfile(currentPhase == EnemyPhase.FinalPush ? 1.06f : 0.98f));
                return;
            }

            if (currentBossTactic == BossTacticState.FallBack)
            {
                AddLineShot(shots, lanes, clashLaneIndex, CreateLineOrbProfile(0.98f));
                AddLineShot(shots, lanes, escortLaneIndex, CreateLineNeedleProfile(0.92f));
                return;
            }

            switch (currentSummonIntent)
            {
                case SummonIntentState.Probe:
                    AddLineShot(shots, lanes, Mathf.Abs(currentBossFormation.PlayerAnchorX) > 0.8f ? playerLaneIndex : clashLaneIndex, CreateLineNeedleProfile());
                    break;
                case SummonIntentState.HoldLine:
                    AddLineShot(shots, lanes, clashLaneIndex, CreateLineOrbProfile());
                    AddLineShot(shots, lanes, pressureAdjacentLane, CreateLineNeedleProfile(0.94f));
                    break;
                case SummonIntentState.EscortPush:
                    AddLineShot(shots, lanes, escortLaneIndex, CreateLineOrbProfile(1.06f));
                    AddLineShot(shots, lanes, escortSupportLane, CreateLineNeedleProfile());
                    break;
                case SummonIntentState.BreakPost:
                    AddLineShot(shots, lanes, structureLaneIndex, CreateLineBreakerProfile());
                    AddLineShot(shots, lanes, ResolveTowardCenterLane(structureLaneIndex, centerLaneIndex, structureLaneIndex < centerLaneIndex ? -1 : 1), CreateLineOrbProfile(0.96f));
                    break;
                case SummonIntentState.PunishHero:
                    AddLineShot(shots, lanes, playerLaneIndex, CreateLineNeedleProfile(1.08f));
                    AddLineShot(shots, lanes, pressureAdjacentLane, CreateLineNeedleProfile(0.92f));
                    break;
                case SummonIntentState.BaseRush:
                    AddLineShot(shots, lanes, playerLaneIndex, CreateLineOrbProfile(1.1f));
                    AddLineShot(shots, lanes, centerLaneIndex, CreateLineBreakerProfile(0.92f));
                    if (currentPhase == EnemyPhase.FinalPush)
                    {
                        AddLineShot(shots, lanes, pressureAdjacentLane, CreateLineNeedleProfile(1.04f));
                    }

                    break;
            }
        }

        private Vector3 ResolveLineProjectileSpawnPosition(float targetX, int shotIndex, int shotCount)
        {
            float targetBiasOffset = Mathf.Clamp(targetX * 0.16f, -1.1f, 1.1f);
            float formationOffset = shotCount <= 1 ? 0f : (shotIndex - ((shotCount - 1) * 0.5f)) * 0.34f;
            Vector3 spawnPosition = projectileSpawnPoint.position + (transform.right * (targetBiasOffset + formationOffset)) + new Vector3(0f, 0f, -0.25f);
            spawnPosition.y = 1.15f;
            return spawnPosition;
        }

        private Vector3 ResolveLineProjectileDirection(Vector3 spawnPosition, float targetX)
        {
            float targetZ = 0.25f;
            Transform playerBase = BattleManager.Instance != null ? BattleManager.Instance.GetOpposingBaseTransform(isPlayerTeam: false) : null;
            if (playerBase != null)
            {
                targetZ = playerBase.position.z + 0.12f;
            }

            Vector3 targetPosition = new(targetX, spawnPosition.y, targetZ);
            Vector3 direction = targetPosition - spawnPosition;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.back;
        }

        private float[] GetProjectileLaneAnchors()
        {
            return BattleManager.Instance != null
                ? BattleManager.Instance.GetLaneAnchors()
                : BattleLaneUtility.BuildLaneAnchors(5.75f);
        }

        private static int GetNearestLaneIndex(float currentX, float[] lanes)
        {
            return BattleLaneUtility.GetNearestLaneIndex(currentX, lanes);
        }

        private int ResolvePressureSide(int playerLaneIndex, int centerLaneIndex)
        {
            if (playerLaneIndex < centerLaneIndex)
            {
                return -1;
            }

            if (playerLaneIndex > centerLaneIndex)
            {
                return 1;
            }

            return projectileVolleyCount % 2 == 0 ? -1 : 1;
        }

        private static int ResolveTowardCenterLane(int playerLaneIndex, int centerLaneIndex, int pressureSide)
        {
            if (playerLaneIndex == centerLaneIndex)
            {
                return pressureSide < 0 ? centerLaneIndex - 1 : centerLaneIndex + 1;
            }

            return playerLaneIndex < centerLaneIndex
                ? Mathf.Min(centerLaneIndex, playerLaneIndex + 1)
                : Mathf.Max(centerLaneIndex, playerLaneIndex - 1);
        }

        private static void AddLaneShot(List<ProjectileLaneShot> shots, float[] lanes, int laneIndex, EnemyProjectile.ProjectileProfile profile)
        {
            int clampedIndex = Mathf.Clamp(laneIndex, 0, lanes.Length - 1);
            float targetX = lanes[clampedIndex];
            for (int index = 0; index < shots.Count; index++)
            {
                if (Mathf.Abs(shots[index].TargetX - targetX) <= 0.05f)
                {
                    return;
                }
            }

            shots.Add(new ProjectileLaneShot(targetX, profile));
        }

        private static void AddLineShot(List<LineVolleyShot> shots, float[] lanes, int laneIndex, EnemyLineProjectile.ProjectileProfile profile)
        {
            int clampedIndex = Mathf.Clamp(laneIndex, 0, lanes.Length - 1);
            float targetX = lanes[clampedIndex];
            for (int index = 0; index < shots.Count; index++)
            {
                if (Mathf.Abs(shots[index].TargetX - targetX) <= 0.05f)
                {
                    return;
                }
            }

            shots.Add(new LineVolleyShot(targetX, profile));
        }

        private static EnemyProjectile.ProjectileProfile CreateNeedleProfile(float sizeMultiplier = 1f)
        {
            return new EnemyProjectile.ProjectileProfile(
                0.54f * sizeMultiplier,
                0.82f,
                1.35f,
                new Color(0.94f, 0.7f, 1f, 1f),
                0.2f,
                1.4f);
        }

        private static EnemyProjectile.ProjectileProfile CreateOrbProfile()
        {
            return new EnemyProjectile.ProjectileProfile(
                0.78f,
                1.2f,
                1.02f,
                new Color(0.58f, 0.86f, 1f, 1f),
                0.24f,
                1.74f);
        }

        private static EnemyProjectile.ProjectileProfile CreateMeteorProfile()
        {
            return new EnemyProjectile.ProjectileProfile(
                1.02f,
                1.58f,
                0.84f,
                new Color(1f, 0.4f, 0.72f, 1f),
                0.28f,
                2.1f);
        }

        private static EnemyLineProjectile.ProjectileProfile CreateLineNeedleProfile(float sizeMultiplier = 1f)
        {
            return new EnemyLineProjectile.ProjectileProfile(
                0.86f * sizeMultiplier,
                1.12f,
                0.92f,
                new Color(1f, 0.74f, 0.36f, 1f));
        }

        private static EnemyLineProjectile.ProjectileProfile CreateLineOrbProfile(float sizeMultiplier = 1f)
        {
            return new EnemyLineProjectile.ProjectileProfile(
                1.08f * sizeMultiplier,
                0.94f,
                1.14f,
                new Color(1f, 0.56f, 0.32f, 1f));
        }

        private static EnemyLineProjectile.ProjectileProfile CreateLineBreakerProfile(float sizeMultiplier = 1f)
        {
            return new EnemyLineProjectile.ProjectileProfile(
                1.18f * sizeMultiplier,
                0.82f,
                1.24f,
                new Color(1f, 0.42f, 0.22f, 1f));
        }

        private static string GetPhaseDisplayName(EnemyPhase phase)
        {
            return phase switch
            {
                EnemyPhase.Opening => "Opening",
                EnemyPhase.Pressure => "Pressure",
                EnemyPhase.Siege => "Siege",
                EnemyPhase.FinalPush => "Final Push",
                _ => "Pressure"
            };
        }

        private static string GetSummonIntentDisplayName(SummonIntentState intent)
        {
            return intent switch
            {
                SummonIntentState.Probe => "Probe",
                SummonIntentState.HoldLine => "Hold Line",
                SummonIntentState.EscortPush => "Escort Push",
                SummonIntentState.BreakPost => "Break Post",
                SummonIntentState.PunishHero => "Punish Hero",
                SummonIntentState.BaseRush => "Base Rush",
                _ => "Probe"
            };
        }

        private static bool IsRushUnit(SummonData card)
        {
            return card != null
                && card.summonType == SummonType.Melee
                && card.energyCost <= 28f
                && card.moveSpeed >= 3f;
        }

        private static bool IsStructureBreaker(SummonData card)
        {
            return card != null
                && card.summonType == SummonType.Melee
                && card.structureDamageMultiplier >= 1.8f;
        }

        private static bool IsLaneControl(SummonData card)
        {
            return card != null
                && card.summonType == SummonType.Ranged
                && card.attackRange >= 7f;
        }

        private static bool IsSplashCaster(SummonData card)
        {
            return card != null
                && card.summonType == SummonType.Ranged
                && card.splashRadius > 0.1f;
        }

        private static bool IsBasePressurer(SummonData card)
        {
            return card != null
                && (card.baseDamageMultiplier > 1.1f || card.moveSpeed >= 3.05f || card.structureDamageMultiplier >= 1.9f);
        }

        private void UpdateBossMovement()
        {
            if (playerController == null)
            {
                return;
            }

            BattleManager battleManager = BattleManager.Instance;
            float currentLaneLength = battleManager != null ? battleManager.LaneLength : 84f;
            float currentLaneHalfWidth = battleManager != null ? battleManager.LaneHalfWidth : 6.25f;
            float rearZ = currentLaneLength - bossRearInset;
            float desiredX = ResolveBossDesiredX(currentBossFormation, currentLaneHalfWidth);
            float desiredZ = ResolveBossDesiredZ(currentBossFormation, currentLaneLength, rearZ, currentEnemyAdvantage);

            Vector3 currentPosition = cachedRigidbody != null ? cachedRigidbody.position : transform.position;
            Vector3 desiredPosition = new(desiredX, currentPosition.y, desiredZ);
            float moveSpeedMultiplier = currentBossTactic switch
            {
                BossTacticState.RearGuard => 0.78f,
                BossTacticState.EscortWave => 1f,
                BossTacticState.ContestMid => 1.08f,
                BossTacticState.SiegeStructure => 1.02f,
                BossTacticState.PunishOverextend => 1.24f,
                BossTacticState.CommitPush => 1.18f,
                BossTacticState.FallBack => 1.12f,
                _ => 1f
            };
            Vector3 nextPosition = Vector3.MoveTowards(currentPosition, desiredPosition, bossMoveSpeed * moveSpeedMultiplier * Time.deltaTime);

            if (cachedRigidbody != null)
            {
                cachedRigidbody.position = nextPosition;
            }

            transform.position = nextPosition;

            Vector3 lookTarget = ResolveBossLookTargetPosition(currentBossFormation, desiredX, desiredZ);
            Vector3 lookDirection = lookTarget - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection.normalized, Vector3.up), 10f * Time.deltaTime);
            }
        }

        private void EvaluateBossTactic(bool forceNotify)
        {
            if (!forceNotify && Time.time < nextBossDecisionTime)
            {
                return;
            }

            BattleManager battleManager = BattleManager.Instance;
            float laneLength = battleManager != null ? battleManager.LaneLength : 84f;
            currentBossFormation = EvaluateBossFormation(laneLength);
            currentEnemyAdvantage = ResolveEnemyAdvantage(battleManager);
            currentPriorityStructure = FindPriorityStructure(currentBossFormation, laneLength);

            float playerBaseRatio = battleManager == null || battleManager.PlayerBaseMaxHP <= 0.001f
                ? 1f
                : battleManager.CurrentPlayerBaseHP / battleManager.PlayerBaseMaxHP;
            BossDecisionContext decisionContext = new(
                currentBossFormation,
                currentEnemyAdvantage,
                currentPriorityStructure,
                CountActiveStructures(),
                playerBaseRatio,
                laneLength,
                currentPhase);
            currentBossDecisionContext = decisionContext;

            BossTacticState newTactic = ResolveBossTacticState(decisionContext, forceNotify, out float tacticScore);
            bool tacticChanged = forceNotify || newTactic != currentBossTactic;
            currentBossTactic = newTactic;
            currentBossTacticScore = tacticScore;
            nextBossDecisionTime = Time.time + Mathf.Max(0.2f, bossDecisionInterval);

            if (!tacticChanged)
            {
                return;
            }

            nextBossTacticShiftAllowedTime = Time.time + ResolveBossTacticCommitDuration(currentBossTactic, currentBossTacticScore);
            ClearWaveLaneAnchor();
            OnBossTacticChanged?.Invoke(CurrentBossTacticName);
            BattlePresentationController.Instance?.AddFeedMessage($"Enemy boss tactic: {CurrentBossTacticName}", GetBossTacticColor(currentBossTactic));
            BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 2.5f, 0f), CurrentBossTacticName.ToUpperInvariant(), GetBossTacticColor(currentBossTactic), 3.6f, 0.75f);
            needsSummonReplan = true;
        }

        private BossFormationSnapshot EvaluateBossFormation(float laneLength)
        {
            IReadOnlyList<SummonUnit> summonUnits = SummonUnit.ActiveUnits;
            float enemyFrontZ = laneLength - bossRearInset;
            float playerHeroZ = playerController != null ? Mathf.Max(playerController.transform.position.z, 0f) : laneLength * 0.1f;
            float playerFrontZ = 0f;
            float enemyWeightedX = 0f;
            float playerWeightedX = 0f;
            float enemyWeightTotal = 0f;
            float playerWeightTotal = 0f;
            int enemyUnitCount = 0;
            int playerUnitCount = 0;

            for (int index = 0; index < summonUnits.Count; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive)
                {
                    continue;
                }

                float normalizedProgress = Mathf.Clamp01(1f - (summonUnit.transform.position.z / Mathf.Max(1f, laneLength)));
                float weight = 0.8f + (normalizedProgress * 1.75f);
                if (summonUnit.IsPlayerTeam)
                {
                    playerUnitCount++;
                    playerFrontZ = Mathf.Max(playerFrontZ, summonUnit.transform.position.z);
                    playerWeightedX += summonUnit.transform.position.x * weight;
                    playerWeightTotal += weight;
                }
                else
                {
                    enemyUnitCount++;
                    enemyFrontZ = Mathf.Min(enemyFrontZ, summonUnit.transform.position.z);
                    enemyWeightedX += summonUnit.transform.position.x * weight;
                    enemyWeightTotal += weight;
                }
            }

            float enemyAnchorX = enemyWeightTotal > 0.001f ? enemyWeightedX / enemyWeightTotal : 0f;
            float playerAnchorX = playerWeightTotal > 0.001f ? playerWeightedX / playerWeightTotal : playerController != null ? playerController.transform.position.x : 0f;
            playerFrontZ = Mathf.Max(playerFrontZ, Mathf.Min(playerHeroZ, laneLength * 0.12f));
            return new BossFormationSnapshot(enemyUnitCount, playerUnitCount, enemyFrontZ, playerFrontZ, playerHeroZ, enemyAnchorX, playerAnchorX);
        }

        private float ResolveEnemyAdvantage(BattleManager battleManager)
        {
            if (battleManager == null || !battleManager.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                return 0.45f;
            }

            return Mathf.InverseLerp(0.45f, -0.7f, frontlineState.Balance);
        }

        private BattleStructure FindPriorityStructure(BossFormationSnapshot formation, float laneLength)
        {
            IReadOnlyList<BattleStructure> structures = BattleStructure.ActiveInstances;
            BattleStructure bestStructure = null;
            float bestScore = float.MinValue;
            int playerInterventionLane = playerController != null
                ? playerController.FocusLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;

            for (int index = 0; index < structures.Count; index++)
            {
                BattleStructure structure = structures[index];
                if (structure == null || structure.IsDestroyed)
                {
                    continue;
                }

                float forwardGap = formation.EnemyFrontZ - structure.transform.position.z;
                if (forwardGap < -2.5f || forwardGap > 18f)
                {
                    continue;
                }

                float laneDistance = Mathf.Abs(structure.transform.position.x - formation.EnemyAnchorX);
                float clashDistance = Mathf.Abs(structure.transform.position.z - formation.ClashCenterZ);
                float healthRatio = structure.MaxHP <= 0.001f
                    ? 1f
                    : Mathf.Clamp01(structure.CurrentHP / structure.MaxHP);
                float structureDepth = Mathf.InverseLerp(laneLength * 0.88f, laneLength * 0.36f, structure.transform.position.z);
                int structureLaneIndex = BattleManager.Instance != null
                    ? BattleManager.Instance.GetNearestLaneIndex(structure.transform.position.x)
                    : BattleLaneUtility.DefaultLaneCount / 2;
                float roleScore = structure.Role switch
                {
                    BattleStructureRole.FrontlineBlocker => 2.4f,
                    BattleStructureRole.RewardObjective => 3.1f,
                    BattleStructureRole.SiegeObjective => 1.8f,
                    _ => 0f
                };
                float score = (14f - Mathf.Abs(forwardGap - 5.5f))
                    - (laneDistance * 1.35f)
                    - (clashDistance * 0.12f)
                    + (1f - healthRatio) * 2.2f
                    + structureDepth * 1.15f
                    + roleScore;
                if (structure.transform.position.z < laneLength * 0.72f)
                {
                    score += 1.2f;
                }

                if (structureLaneIndex == playerInterventionLane)
                {
                    score += 2.15f;
                }

                if (formation.PlayerHeroZ >= structure.transform.position.z + 3.5f)
                {
                    score += 0.9f;
                }

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestStructure = structure;
            }

            return bestStructure;
        }

        private BossTacticState ResolveBossTacticState(BossDecisionContext context, bool forceNotify, out float resolvedScore)
        {
            BossTacticState bestTactic = currentBossTactic;
            float bestScore = float.MinValue;
            float currentScore = float.MinValue;

            for (int index = 0; index < BossTactics.Length; index++)
            {
                BossTacticState tactic = BossTactics[index];
                float score = ScoreBossTactic(tactic, context);
                if (tactic == currentBossTactic)
                {
                    currentScore = score;
                }

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestTactic = tactic;
            }

            if (forceNotify)
            {
                resolvedScore = bestScore;
                return bestTactic;
            }

            if (bestTactic == currentBossTactic)
            {
                resolvedScore = currentScore;
                return currentBossTactic;
            }

            float swapThreshold = bossTacticSwapThreshold;
            if (Time.time < nextBossTacticShiftAllowedTime)
            {
                swapThreshold += bossTacticStickyBias;
            }

            if ((bestScore - currentScore) < swapThreshold)
            {
                resolvedScore = currentScore;
                return currentBossTactic;
            }

            resolvedScore = bestScore;
            return bestTactic;
        }

        private float ScoreBossTactic(BossTacticState tactic, BossDecisionContext context)
        {
            bool isOpening = context.Phase == EnemyPhase.Opening;
            bool isFinalPush = context.Phase == EnemyPhase.FinalPush;
            bool isSiege = context.Phase == EnemyPhase.Siege;
            float score = tactic switch
            {
                BossTacticState.RearGuard => 1f
                    + (context.BackFoot01 * 2.05f)
                    + ((1f - context.WaveSupport01) * 1.75f)
                    + ((1f - context.ClashPressure01) * 0.72f)
                    + (isOpening ? 0.95f : 0f)
                    - (context.CloseOut01 * 1.25f)
                    - (context.HeroPressure01 * 0.45f),
                BossTacticState.EscortWave => 0.95f
                    + (context.WaveSupport01 * 1.85f)
                    + (context.EnemyAdvantage * 0.8f)
                    + ((1f - context.ClashPressure01) * 1.1f)
                    + (context.StructureOpportunity01 * 0.25f)
                    - (context.CloseOut01 * 0.55f),
                BossTacticState.ContestMid => 0.95f
                    + (context.ClashPressure01 * 2.1f)
                    + (context.WaveSupport01 * 0.95f)
                    + (context.EnemyAdvantage * 0.62f)
                    + (isSiege ? 0.35f : 0f)
                    - (context.BackFoot01 * 0.3f),
                BossTacticState.SiegeStructure => 0.7f
                    + (context.StructureOpportunity01 * 2.8f)
                    + (context.EnemyAdvantage * 0.95f)
                    + (context.WaveSupport01 * 0.78f)
                    + (context.Phase >= EnemyPhase.Pressure ? 0.5f : -0.8f)
                    - (context.HeroPressure01 * 0.62f),
                BossTacticState.PunishOverextend => 0.72f
                    + (context.HeroPressure01 * 3f)
                    + (context.WaveSupport01 * 0.88f)
                    + (context.EnemyAdvantage * 0.65f)
                    + (context.HasWave ? 0.35f : -0.35f)
                    - (context.CloseOut01 * 0.35f),
                BossTacticState.CommitPush => 0.65f
                    + (context.CloseOut01 * 2.85f)
                    + (context.EnemyAdvantage * 1.18f)
                    + (context.WaveSupport01 * 0.94f)
                    + (isFinalPush ? 0.78f : 0f)
                    - (context.BackFoot01 * 0.95f),
                BossTacticState.FallBack => 0.82f
                    + (context.BackFoot01 * 2.65f)
                    + ((1f - context.WaveSupport01) * 1.02f)
                    + (context.ClashPressure01 * 0.7f)
                    + (context.EnemyBehind ? 0.5f : 0f)
                    - (context.CloseOut01 * 1.12f),
                _ => 0f
            };

            if (!context.HasWave)
            {
                if (tactic == BossTacticState.EscortWave
                    || tactic == BossTacticState.SiegeStructure
                    || tactic == BossTacticState.CommitPush
                    || tactic == BossTacticState.ContestMid)
                {
                    score -= 0.85f;
                }

                if (tactic == BossTacticState.RearGuard || tactic == BossTacticState.FallBack)
                {
                    score += 0.35f;
                }
            }

            if (!context.HasPriorityStructure && tactic == BossTacticState.SiegeStructure)
            {
                score -= 2.4f;
            }

            if (context.HeroPressure01 < 0.22f && tactic == BossTacticState.PunishOverextend)
            {
                score -= 0.95f;
            }

            if (context.CloseOut01 < 0.28f && tactic == BossTacticState.CommitPush && !isFinalPush)
            {
                score -= 0.8f;
            }

            if (context.EnemyAdvantage < 0.42f && tactic == BossTacticState.CommitPush)
            {
                score -= 0.8f;
            }

            if (isOpening)
            {
                if (tactic == BossTacticState.CommitPush || tactic == BossTacticState.SiegeStructure)
                {
                    score -= 1.15f;
                }

                if (tactic == BossTacticState.RearGuard)
                {
                    score += 0.25f;
                }
            }

            return score;
        }

        private float ResolveBossTacticCommitDuration(BossTacticState tactic, float tacticScore)
        {
            float confidence = Mathf.InverseLerp(1.4f, 4.8f, tacticScore);
            float baseDuration = tactic switch
            {
                BossTacticState.PunishOverextend => bossMinimumTacticCommit,
                BossTacticState.CommitPush => Mathf.Lerp(bossMinimumTacticCommit + 0.18f, bossMaximumTacticCommit - 0.1f, 0.72f),
                BossTacticState.SiegeStructure => Mathf.Lerp(bossMinimumTacticCommit + 0.08f, bossMaximumTacticCommit, 0.78f),
                BossTacticState.FallBack => Mathf.Lerp(bossMinimumTacticCommit, bossMaximumTacticCommit - 0.05f, 0.66f),
                _ => Mathf.Lerp(bossMinimumTacticCommit, bossMaximumTacticCommit, 0.54f)
            };

            return Mathf.Lerp(baseDuration, bossMaximumTacticCommit, confidence * 0.45f);
        }

        private float ResolveBossDesiredX(BossFormationSnapshot formation, float laneHalfWidth)
        {
            float escortX = formation.EnemyUnitCount > 0 ? formation.EnemyAnchorX : 0f;
            float clashX = formation.ClashAnchorX;
            float desiredX = currentBossTactic switch
            {
                BossTacticState.RearGuard => Mathf.Lerp(escortX, 0f, 0.35f),
                BossTacticState.EscortWave => escortX,
                BossTacticState.ContestMid => Mathf.Lerp(escortX, clashX, 0.45f),
                BossTacticState.SiegeStructure => currentPriorityStructure != null ? Mathf.Lerp(escortX, currentPriorityStructure.transform.position.x, 0.82f) : escortX,
                BossTacticState.PunishOverextend => Mathf.Lerp(escortX, playerController != null ? playerController.transform.position.x : clashX, 0.72f),
                BossTacticState.CommitPush => Mathf.Lerp(escortX, clashX, 0.68f),
                BossTacticState.FallBack => Mathf.Lerp(escortX, 0f, 0.5f),
                _ => escortX
            };

            return Mathf.Clamp(desiredX * bossTrackStrength, -laneHalfWidth * 0.58f, laneHalfWidth * 0.58f);
        }

        private float ResolveBossDesiredZ(BossFormationSnapshot formation, float laneLength, float rearZ, float enemyAdvantage)
        {
            float deepestAdvanceZ = Mathf.Max(laneLength * 0.26f, rearZ - bossAdvanceDepth);
            float desiredZ = rearZ;

            switch (currentBossTactic)
            {
                case BossTacticState.RearGuard:
                    desiredZ = formation.EnemyUnitCount > 0
                        ? Mathf.Max(formation.EnemyFrontZ + 10.5f, laneLength * 0.72f)
                        : rearZ;
                    break;
                case BossTacticState.EscortWave:
                    desiredZ = formation.EnemyFrontZ + Mathf.Lerp(9.5f, 7.4f, enemyAdvantage);
                    break;
                case BossTacticState.ContestMid:
                    desiredZ = Mathf.Min(formation.EnemyFrontZ + 5.4f, formation.ClashCenterZ + 6.2f);
                    break;
                case BossTacticState.SiegeStructure:
                    desiredZ = currentPriorityStructure != null
                        ? Mathf.Min(currentPriorityStructure.transform.position.z + 4.2f, formation.EnemyFrontZ + 5.8f)
                        : Mathf.Min(formation.EnemyFrontZ + 5.4f, formation.ClashCenterZ + 6.2f);
                    break;
                case BossTacticState.PunishOverextend:
                    desiredZ = Mathf.Min(formation.PlayerHeroZ + 4.8f, formation.EnemyFrontZ + 4.2f);
                    break;
                case BossTacticState.CommitPush:
                    desiredZ = Mathf.Min(formation.EnemyFrontZ + 2.8f, formation.ClashCenterZ + 3.8f);
                    break;
                case BossTacticState.FallBack:
                    desiredZ = Mathf.Max(formation.EnemyFrontZ + 11.5f, laneLength * 0.76f);
                    break;
            }

            desiredZ = Mathf.Clamp(desiredZ, deepestAdvanceZ, rearZ);
            desiredZ = Mathf.Max(ResolveBossSafetyFloorZ(formation, laneLength), desiredZ);
            return Mathf.Min(desiredZ, rearZ);
        }

        private float ResolveBossSafetyFloorZ(BossFormationSnapshot formation, float laneLength)
        {
            float heroSafetyZ = playerController != null ? playerController.transform.position.z : formation.PlayerHeroZ;
            return currentBossTactic switch
            {
                BossTacticState.RearGuard => Mathf.Max(Mathf.Max(heroSafetyZ + 4.8f, formation.EnemyFrontZ + 8.8f), laneLength * 0.68f),
                BossTacticState.EscortWave => Mathf.Max(heroSafetyZ + 3.2f, formation.EnemyFrontZ + 4.4f),
                BossTacticState.ContestMid => Mathf.Max(heroSafetyZ + 1.85f, formation.ClashCenterZ + 2.4f),
                BossTacticState.SiegeStructure => currentPriorityStructure != null
                    ? Mathf.Max(currentPriorityStructure.transform.position.z + 1.4f, formation.EnemyFrontZ + 2.8f)
                    : Mathf.Max(heroSafetyZ + 2.2f, formation.ClashCenterZ + 2.8f),
                BossTacticState.PunishOverextend => Mathf.Max(heroSafetyZ + 0.95f, formation.EnemyFrontZ + 1.4f),
                BossTacticState.CommitPush => Mathf.Max(heroSafetyZ + 0.45f, formation.EnemyFrontZ + 0.9f),
                BossTacticState.FallBack => Mathf.Max(Mathf.Max(heroSafetyZ + 5.6f, formation.EnemyFrontZ + 10f), laneLength * 0.74f),
                _ => heroSafetyZ + 2f
            };
        }

        private Vector3 ResolveBossLookTargetPosition(BossFormationSnapshot formation, float desiredX, float desiredZ)
        {
            Vector3 fallbackTarget = new(desiredX, transform.position.y, Mathf.Max(0f, desiredZ - 2.8f));
            return currentBossTactic switch
            {
                BossTacticState.SiegeStructure when currentPriorityStructure != null => currentPriorityStructure.transform.position + new Vector3(0f, 0.6f, 0f),
                BossTacticState.PunishOverextend when playerController != null => playerController.transform.position,
                BossTacticState.CommitPush when playerController != null => new Vector3(
                    playerController.transform.position.x,
                    transform.position.y,
                    Mathf.Max(formation.PlayerFrontZ, playerController.transform.position.z)),
                BossTacticState.ContestMid => new Vector3(formation.ClashAnchorX, transform.position.y, formation.ClashCenterZ),
                BossTacticState.EscortWave => new Vector3(formation.EnemyAnchorX, transform.position.y, Mathf.Max(0f, formation.EnemyFrontZ - 2.6f)),
                BossTacticState.RearGuard => new Vector3(formation.ClashAnchorX, transform.position.y, formation.ClashCenterZ),
                BossTacticState.FallBack => new Vector3(formation.EnemyAnchorX, transform.position.y, Mathf.Max(0f, formation.EnemyFrontZ - 1.6f)),
                _ => fallbackTarget
            };
        }

        private void UpdateBossSupportPulse()
        {
            if (Time.time < nextBossSupportPulseTime)
            {
                return;
            }

            bool shouldPulse = currentBossTactic == BossTacticState.EscortWave
                || currentBossTactic == BossTacticState.ContestMid
                || currentBossTactic == BossTacticState.SiegeStructure
                || currentBossTactic == BossTacticState.CommitPush
                || currentBossTactic == BossTacticState.FallBack;
            if (!shouldPulse)
            {
                return;
            }

            int enemyLayerMask = LayerMask.GetMask("EnemySummon");
            Collider[] colliders = Physics.OverlapSphere(transform.position, bossSupportRadius, enemyLayerMask);
            int affectedUnits = 0;
            float damageMultiplier = currentBossTactic == BossTacticState.CommitPush ? bossSupportDamageMultiplier + 0.1f : bossSupportDamageMultiplier;
            float moveMultiplier = currentBossTactic == BossTacticState.PunishOverextend ? bossSupportMoveMultiplier + 0.08f : bossSupportMoveMultiplier;
            float healAmount = currentBossTactic == BossTacticState.FallBack ? bossSupportPulseHeal * 1.35f : bossSupportPulseHeal;

            for (int index = 0; index < colliders.Length; index++)
            {
                SummonUnit summonUnit = colliders[index] != null ? colliders[index].GetComponentInParent<SummonUnit>() : null;
                if (summonUnit == null || !summonUnit.IsAlive || summonUnit.IsPlayerTeam)
                {
                    continue;
                }

                summonUnit.ApplyHeroSupport(bossSupportDuration, damageMultiplier, moveMultiplier, healAmount);
                affectedUnits++;
            }

            if (affectedUnits > 0)
            {
                Color supportColor = GetBossTacticColor(currentBossTactic);
                BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 2f, 0f), "COMMAND", supportColor, 3.4f, 0.58f);
                BattlePresentationController.Instance?.SpawnBurst(transform.position + Vector3.up * 1.2f, supportColor, 12, 0.16f, 2.4f, 0.18f, 0.4f);
            }

            nextBossSupportPulseTime = Time.time + bossSupportPulseCooldown;
        }

        private void UpdateBossStructurePressure()
        {
            if (currentPriorityStructure == null || currentPriorityStructure.IsDestroyed || Time.time < nextBossStructurePressureTime || siegeStrikePending)
            {
                return;
            }

            bool shouldPressureStructure = currentBossTactic == BossTacticState.SiegeStructure
                || (currentBossTactic == BossTacticState.CommitPush && currentEnemyAdvantage >= 0.74f);
            if (!shouldPressureStructure)
            {
                return;
            }

            Vector3 horizontalDelta = currentPriorityStructure.transform.position - transform.position;
            horizontalDelta.y = 0f;
            if (horizontalDelta.sqrMagnitude > bossStructurePressureRange * bossStructurePressureRange)
            {
                return;
            }

            nextBossStructurePressureTime = Time.time + bossStructurePressureCooldown;
            Color pressureColor = GetBossTacticColor(currentBossTactic);
            StartCoroutine(ExecuteSiegeStrike(currentPriorityStructure, pressureColor));
        }

        private IEnumerator ExecuteSiegeStrike(BattleStructure targetStructure, Color pressureColor)
        {
            if (targetStructure == null || targetStructure.IsDestroyed)
            {
                yield break;
            }

            siegeStrikePending = true;
            Vector3 markerPosition = targetStructure.transform.position + new Vector3(0f, 2.1f, 0f);
            BattlePresentationController.Instance?.ShowWorldText(markerPosition, "LOCK", pressureColor, 3.4f, bossStructurePressureTelegraphDelay + 0.18f);
            BattlePresentationController.Instance?.SpawnBurst(targetStructure.transform.position + Vector3.up * 0.8f, Color.Lerp(pressureColor, Color.white, 0.2f), 12, 0.14f, 1.8f, 0.14f, 0.36f);
            yield return new WaitForSeconds(bossStructurePressureTelegraphDelay);

            siegeStrikePending = false;
            if (targetStructure == null || targetStructure.IsDestroyed)
            {
                yield break;
            }

            targetStructure.TakeDamage(bossStructurePressureDamage, false);
            BattlePresentationController.Instance?.ShowWorldText(markerPosition, "SIEGE", pressureColor, 3.6f, 0.7f);
            BattlePresentationController.Instance?.SpawnBurst(targetStructure.transform.position + Vector3.up * 0.9f, pressureColor, 16, 0.18f, 2.8f, 0.16f, 0.5f);
        }

        private static string GetBossTacticDisplayName(BossTacticState tactic)
        {
            return tactic switch
            {
                BossTacticState.RearGuard => "Rear Guard",
                BossTacticState.EscortWave => "Escort Wave",
                BossTacticState.ContestMid => "Contest Mid",
                BossTacticState.SiegeStructure => "Siege Structure",
                BossTacticState.PunishOverextend => "Punish Overextend",
                BossTacticState.CommitPush => "Commit Push",
                BossTacticState.FallBack => "Fall Back",
                _ => "Escort Wave"
            };
        }

        private static string ResolveBossCueShort(BossTacticState tactic)
        {
            return tactic switch
            {
                BossTacticState.RearGuard => "Back screen",
                BossTacticState.EscortWave => "Wave escort",
                BossTacticState.ContestMid => "Mid contest",
                BossTacticState.SiegeStructure => "Post dive",
                BossTacticState.PunishOverextend => "Hero hunt",
                BossTacticState.CommitPush => "All-in",
                BossTacticState.FallBack => "Reset line",
                _ => "Wave escort"
            };
        }

        private static string ResolveSummonCueShort(SummonIntentState intent)
        {
            return intent switch
            {
                SummonIntentState.Probe => "Light probe",
                SummonIntentState.HoldLine => "Line hold",
                SummonIntentState.EscortPush => "Escort push",
                SummonIntentState.BreakPost => "Post break",
                SummonIntentState.PunishHero => "Hero punish",
                SummonIntentState.BaseRush => "Base rush",
                _ => "Flexible push"
            };
        }

        private static string ResolveBossCue(BossTacticState tactic, float confidence01)
        {
            return $"{ResolveBossCueShort(tactic)} · {ResolvePressureCommitWord(confidence01)}";
        }

        private static string ResolveSummonCue(SummonIntentState intent, float confidence01)
        {
            return $"{ResolveSummonCueShort(intent)} · {ResolvePressureCommitWord(confidence01)}";
        }

        private string ResolveCounterAdvice()
        {
            if (RemainingProjectileCountdown <= 0.95f)
            {
                return RemainingProjectileCountdown <= 0.4f
                    ? "HEX NOW - DODGE FIRST, RE-COMMIT AFTER"
                    : "HEX SOON - BAIT, THEN STEP BACK IN";
            }

            return currentSummonIntent switch
            {
                SummonIntentState.BreakPost => "POST BREAK - BODYGUARD THE STRUCTURE",
                SummonIntentState.PunishHero => "HERO HUNT - LET YOUR WAVE TANK FIRST",
                SummonIntentState.BaseRush => "BASE RUSH - HOLD MID, DON'T CHASE",
                SummonIntentState.EscortPush => "ESCORT PUSH - CUT THE FRONT SCREEN",
                SummonIntentState.HoldLine when currentBossTactic == BossTacticState.FallBack => "RESET WINDOW - TAKE SPACE SAFELY",
                SummonIntentState.HoldLine => "LINE HOLD - BUILD A CLEANER FRONT",
                SummonIntentState.Probe when currentBossTactic == BossTacticState.RearGuard => "LIGHT PROBE - BUILD BEFORE YOU SWING",
                SummonIntentState.Probe => "LIGHT PRESSURE - KEEP A SAFE LANE",
                _ => currentBossTactic switch
                {
                    BossTacticState.SiegeStructure => "SIEGE READ - PROTECT THE POST OR TRADE",
                    BossTacticState.PunishOverextend => "PUNISH READ - STEP IN WITH YOUR WAVE",
                    BossTacticState.CommitPush => "ALL-IN READ - BODYBLOCK THE CENTER",
                    BossTacticState.FallBack => "RESET READ - CLAIM GROUND WITHOUT OVERCHASING",
                    BossTacticState.EscortWave => "ESCORT READ - TRIM THE FRONT WAVE",
                    BossTacticState.ContestMid => "MID BRAWL - STABILIZE BEFORE COMMIT",
                    _ => "READ THE PUSH, THEN COMMIT"
                }
            };
        }

        private static string ResolvePressureCommitWord(float confidence01)
        {
            if (confidence01 >= 0.72f)
            {
                return "locked";
            }

            if (confidence01 >= 0.4f)
            {
                return "set";
            }

            return "reading";
        }

        private static Color GetBossTacticColor(BossTacticState tactic)
        {
            return tactic switch
            {
                BossTacticState.RearGuard => new Color(0.72f, 0.82f, 1f, 1f),
                BossTacticState.EscortWave => new Color(0.44f, 0.92f, 1f, 1f),
                BossTacticState.ContestMid => new Color(1f, 0.82f, 0.45f, 1f),
                BossTacticState.SiegeStructure => new Color(1f, 0.68f, 0.32f, 1f),
                BossTacticState.PunishOverextend => new Color(1f, 0.56f, 0.42f, 1f),
                BossTacticState.CommitPush => new Color(1f, 0.38f, 0.38f, 1f),
                BossTacticState.FallBack => new Color(0.6f, 1f, 0.78f, 1f),
                _ => Color.white
            };
        }

        private void HandleBossCollisionPressure()
        {
            if (playerController == null || Time.time < nextBossContactTime)
            {
                return;
            }

            Vector3 horizontalDelta = playerController.transform.position - transform.position;
            horizontalDelta.y = 0f;
            if (horizontalDelta.sqrMagnitude > bossContactRange * bossContactRange)
            {
                return;
            }

            nextBossContactTime = Time.time + bossContactCooldown;
            playerController.TakeDamage(bossContactDamage);
            Vector3 pushDirection = horizontalDelta.sqrMagnitude > 0.001f ? horizontalDelta.normalized : Vector3.back;
            playerController.ApplyExternalDisplacement(pushDirection * bossPushbackDistance);
            CameraShake.Instance?.PlayShake(0.14f, 0.16f);
            BattlePresentationController.Instance?.ShowWorldText(
                playerController.transform.position + new Vector3(0f, 2.1f, 0f),
                "BOSS HIT",
                new Color(1f, 0.56f, 0.42f, 1f),
                3.6f,
                0.62f);
        }

        private void EnsureBossCollisionBody()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            if (cachedRigidbody == null)
            {
                cachedRigidbody = gameObject.AddComponent<Rigidbody>();
            }

            cachedRigidbody.useGravity = false;
            cachedRigidbody.isKinematic = true;
            cachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            cachedCapsuleCollider = GetComponent<CapsuleCollider>();
            if (cachedCapsuleCollider == null)
            {
                cachedCapsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            cachedCapsuleCollider.isTrigger = false;
            cachedCapsuleCollider.height = 2.4f;
            cachedCapsuleCollider.radius = 0.72f;
            cachedCapsuleCollider.center = new Vector3(0f, 1.2f, 0f);
        }

        private void AlignVisualToGround()
        {
            if (transform.childCount == 0)
            {
                return;
            }

            Transform visualRoot = transform.GetChild(0);
            if (cachedGroundVisualRoot != visualRoot)
            {
                cachedGroundVisualRoot = visualRoot;
                cachedGroundRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            }

            Renderer[] renderers = cachedGroundRenderers;
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            float lowestPoint = float.MaxValue;
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] == null)
                {
                    continue;
                }

                lowestPoint = Mathf.Min(lowestPoint, renderers[index].bounds.min.y);
            }

            if (lowestPoint == float.MaxValue)
            {
                return;
            }

            float offset = transform.position.y - lowestPoint;
            if (Mathf.Abs(offset) <= 0.001f)
            {
                return;
            }

            visualRoot.position += Vector3.up * offset;
        }
    }
}
