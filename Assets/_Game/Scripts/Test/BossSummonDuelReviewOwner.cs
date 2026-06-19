using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Test
{
    // Review-only orchestration for summon duel validation; production encounter flow should use a separate owner.
    [DisallowMultipleComponent]
    public sealed class BossSummonDuelReviewOwner : MonoBehaviour
    {
        public enum DuelPhase
        {
            BuildPressure,
            BossPressureAction,
            SummonExchange,
            SkillResponse,
            CounterDamage,
            Cleared,
            Failed
        }

        [Header("Combatants")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth bossHealth;

        [Header("Player Side")]
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;

        [Header("Boss Side")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureCostLadder bossPressureCostLadder;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private BossSummonPressureAction bossSummonPressureAction;

        [Header("Review Warm Start")]
        [SerializeField] private bool grantPlayerEnergyOnStart = true;
        [SerializeField, Min(0f)] private float startingPlayerEnergy = 115f;
        [SerializeField] private bool grantBossCostOnStart = true;
        [SerializeField, Min(0f)] private float startingBossCost = 130f;

        [Header("Review Goals")]
        [SerializeField, Min(0)] private int requiredBossPressureActions = 2;
        [SerializeField, Min(0)] private int requiredBossSkillPatterns = 1;
        [SerializeField, Min(0)] private int requiredBossSummonPressureActions = 1;
        [SerializeField, Min(0)] private int requiredBossPunishPatterns;
        [SerializeField, Min(0)] private int requiredBossSummonReleases = 1;
        [SerializeField, Min(0)] private int requiredBossPressureBlocks = 1;
        [SerializeField, Min(0)] private int requiredPlayerSummonUses = 2;
        [SerializeField, Min(0)] private int requiredSupportSummonUses = 1;
        [SerializeField, Min(0)] private int requiredAllyPressureBlocks = 1;
        [SerializeField, Min(0)] private int requiredSummonClashes = 1;
        [SerializeField, Min(0)] private int requiredSkill1ResponseUses = 1;
        [SerializeField, Min(0f)] private float requiredSkill1ResponseDamage = 60f;
        [SerializeField, Min(0.05f)] private float skill1ResponseDamageWindowSeconds = 2.5f;
        [SerializeField, Min(0f)] private float requiredBossDamage = 260f;
        [SerializeField] private bool failWhenPlayerDies = true;

        [Header("Review Result")]
        [SerializeField] private bool stopBarrageOnEnd = true;
        [SerializeField] private bool stopBossPressureCostOnEnd = true;
        [SerializeField] private bool stopBossPressureActionsOnEnd = true;
        [SerializeField] private bool stopEnergyGainOnEnd = true;
        [SerializeField] private GameObject clearMarker;
        [SerializeField] private GameObject failMarker;

        private CombatHealth subscribedBossHealth;
        private bool warmStartApplied;
        private bool failed;
        private bool cleared;
        private int observedSkill1Uses;
        private int observedPlayerSummonUses;
        private int observedSupportSummonUses;
        private int observedAllyPressureBlocks;
        private int observedSupportPressureBlocks;
        private int observedSummonClashes;
        private int observedBossPressureActions;
        private int observedBossSkillPatterns;
        private int observedBossSummonPressureActions;
        private int observedBossPunishPatterns;
        private int observedBossSummonReleases;
        private int observedBossPressureBlocks;
        private int observedSkill1ResponseUses;
        private int highestPlayerSummonTier;
        private int highestBossPressureTier;
        private int highestBossSummonTier;
        private float bossDamageFromPlayerSide;
        private float skill1ResponseDamageFromPlayerSide;
        private float skill1ResponseDamageUntilTime;
        private int lastObservedSummonSlot1Clashes;
        private int lastObservedSummonSlot2Clashes;
        private int lastObservedSummonSlot3Clashes;
        private int lastObservedBossSummonClashes;

        public bool IsCleared => cleared;
        public bool IsFailed => failed;
        public int ObservedSkill1Uses => observedSkill1Uses;
        public int ObservedPlayerSummonUses => observedPlayerSummonUses;
        public int ObservedSupportSummonUses => observedSupportSummonUses;
        public int ObservedAllyPressureBlocks => observedAllyPressureBlocks;
        public int ObservedSupportPressureBlocks => observedSupportPressureBlocks;
        public int ObservedSummonClashes => observedSummonClashes;
        public int ObservedBossPressureActions => observedBossPressureActions;
        public int ObservedBossSkillPatterns => observedBossSkillPatterns;
        public int ObservedBossSummonPressureActions => observedBossSummonPressureActions;
        public int ObservedBossPunishPatterns => observedBossPunishPatterns;
        public int ObservedBossSummonReleases => observedBossSummonReleases;
        public int ObservedBossPressureBlocks => observedBossPressureBlocks;
        public int ObservedSkill1ResponseUses => observedSkill1ResponseUses;
        public int HighestPlayerSummonTier => highestPlayerSummonTier;
        public int HighestBossPressureTier => highestBossPressureTier;
        public int HighestBossSummonTier => highestBossSummonTier;
        public float BossDamageFromPlayerSide => bossDamageFromPlayerSide;
        public float Skill1ResponseDamageFromPlayerSide => skill1ResponseDamageFromPlayerSide;
        public int RequiredBossPressureActions => requiredBossPressureActions;
        public int RequiredBossSkillPatterns => requiredBossSkillPatterns;
        public int RequiredBossSummonPressureActions => requiredBossSummonPressureActions;
        public int RequiredBossPunishPatterns => requiredBossPunishPatterns;
        public int RequiredBossSummonReleases => requiredBossSummonReleases;
        public int RequiredBossPressureBlocks => requiredBossPressureBlocks;
        public int RequiredPlayerSummonUses => requiredPlayerSummonUses;
        public int RequiredSupportSummonUses => requiredSupportSummonUses;
        public int RequiredAllyPressureBlocks => requiredAllyPressureBlocks;
        public int RequiredSummonClashes => requiredSummonClashes;
        public int RequiredSkill1ResponseUses => requiredSkill1ResponseUses;
        public float RequiredSkill1ResponseDamage => requiredSkill1ResponseDamage;
        public float RequiredBossDamage => requiredBossDamage;

        public DuelPhase CurrentPhase
        {
            get
            {
                if (cleared)
                {
                    return DuelPhase.Cleared;
                }

                if (failed)
                {
                    return DuelPhase.Failed;
                }

                if (observedBossPressureActions < requiredBossPressureActions)
                {
                    return DuelPhase.BuildPressure;
                }

                if (observedBossSkillPatterns < requiredBossSkillPatterns
                    || observedBossSummonPressureActions < requiredBossSummonPressureActions
                    || observedBossPunishPatterns < requiredBossPunishPatterns)
                {
                    return DuelPhase.BossPressureAction;
                }

                if (observedBossSummonReleases < requiredBossSummonReleases)
                {
                    return DuelPhase.BossPressureAction;
                }

                if (!HasMetSummonExchangeGoal())
                {
                    return DuelPhase.SummonExchange;
                }

                if (!HasMetSkillResponseGoal())
                {
                    return DuelPhase.SkillResponse;
                }

                return DuelPhase.CounterDamage;
            }
        }

        public string ObjectiveCue
        {
            get
            {
                if (cleared)
                {
                    return "Duel loop verified: boss cost, boss summon pressure, ally summons, block, and counter damage";
                }

                if (failed)
                {
                    return "Player defeated; use backline safety, dodge, then build EN for shield/support summons";
                }

                if (observedBossPressureActions < requiredBossPressureActions)
                {
                    return "Move between safety and forward risk while the boss builds costed pressure";
                }

                if (observedBossSkillPatterns < requiredBossSkillPatterns)
                {
                    return "Read at least one boss skill-pattern pressure before spending the summon answer";
                }

                if (observedBossSummonPressureActions < requiredBossSummonPressureActions)
                {
                    return "Force one boss summon-pressure action so the exchange is not only raw bullets";
                }

                if (observedBossPunishPatterns < requiredBossPunishPatterns)
                {
                    return "Overextend once and escape the boss punish pattern before countering";
                }

                if (observedBossSummonReleases < requiredBossSummonReleases)
                {
                    return "Bait the boss toward LV2 pressure so its summon screen enters the lane";
                }

                if (observedBossPressureBlocks < requiredBossPressureBlocks)
                {
                    return "Fire into the boss screen once so boss summon pressure actually answers player shots";
                }

                if (observedPlayerSummonUses < requiredPlayerSummonUses)
                {
                    return "Build EN and call multiple summons so the frontline exchange is visible";
                }

                if (observedSupportSummonUses < requiredSupportSummonUses)
                {
                    return "Use S2 Arrow or S3 Tank so the loop is not only the S1 shield";
                }

                if (observedAllyPressureBlocks < requiredAllyPressureBlocks)
                {
                    return "Use S1 Shield or S3 Tank where its screen can block boss fire";
                }

                if (observedSummonClashes < requiredSummonClashes)
                {
                    return "Let an ally summon and boss summon body-clash so the frontline duel is not only projectiles";
                }

                if (observedSkill1ResponseUses < requiredSkill1ResponseUses)
                {
                    return "Answer the summon block with Skill1 before going back to basic pressure";
                }

                if (skill1ResponseDamageFromPlayerSide < requiredSkill1ResponseDamage)
                {
                    return $"Confirm Skill1 response damage on boss ({skill1ResponseDamageFromPlayerSide:0}/{requiredSkill1ResponseDamage:0})";
                }

                return $"Damage boss with Skill1, ranged fire, or summon counters ({bossDamageFromPlayerSide:0}/{requiredBossDamage:0})";
            }
        }

        public string ProgressLine =>
            $"Duel boss {observedBossPressureActions}/{requiredBossPressureActions} "
            + $"bossSkill {observedBossSkillPatterns}/{requiredBossSkillPatterns} "
            + $"bossSP {observedBossSummonPressureActions}/{requiredBossSummonPressureActions} "
            + $"bossPunish {observedBossPunishPatterns}/{requiredBossPunishPatterns} "
            + $"bossSummon {observedBossSummonReleases}/{requiredBossSummonReleases} "
            + $"bossBlock {observedBossPressureBlocks}/{requiredBossPressureBlocks} "
            + $"summon {observedPlayerSummonUses}/{requiredPlayerSummonUses} "
            + $"support {observedSupportSummonUses}/{requiredSupportSummonUses} "
            + $"block {observedAllyPressureBlocks}/{requiredAllyPressureBlocks} "
            + $"sBlock {observedSupportPressureBlocks} "
            + $"clash {observedSummonClashes}/{requiredSummonClashes} "
            + $"skill {observedSkill1ResponseUses}/{requiredSkill1ResponseUses} "
            + $"skillDmg {skill1ResponseDamageFromPlayerSide:0}/{requiredSkill1ResponseDamage:0} "
            + $"dmg {bossDamageFromPlayerSide:0}/{requiredBossDamage:0}";

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            ApplyWarmStartOnce();
            SetMarkers();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (cleared || failed)
            {
                return;
            }

            if (failWhenPlayerDies && playerHealth != null && !playerHealth.IsAlive)
            {
                EnterFailed();
                return;
            }

            ObserveSummonClashes();
            if (HasMetReviewGoals())
            {
                EnterCleared();
            }
        }

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newBossHealth,
            SummonEnergyLadder newEnergyLadder,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction newSummonSlot2Action,
            PlayerSupportSummonSlotAction newSummonSlot3Action,
            BossBarrageEmitter newBossBarrageEmitter,
            BossPressureCostLadder newBossPressureCostLadder,
            BossPressureActionDirector newBossPressureActionDirector,
            BossSummonPressureAction newBossSummonPressureAction,
            GameObject newClearMarker,
            GameObject newFailMarker)
        {
            Unsubscribe();
            playerHealth = newPlayerHealth;
            bossHealth = newBossHealth;
            energyLadder = newEnergyLadder;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            summonSlot2Action = newSummonSlot2Action;
            summonSlot3Action = newSummonSlot3Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            bossPressureCostLadder = newBossPressureCostLadder;
            bossPressureActionDirector = newBossPressureActionDirector;
            bossSummonPressureAction = newBossSummonPressureAction;
            clearMarker = newClearMarker;
            failMarker = newFailMarker;
            Subscribe();
            SetMarkers();
        }

        public void ResetReviewState()
        {
            failed = false;
            cleared = false;
            observedSkill1Uses = 0;
            observedPlayerSummonUses = 0;
            observedSupportSummonUses = 0;
            observedAllyPressureBlocks = 0;
            observedSupportPressureBlocks = 0;
            observedSummonClashes = 0;
            observedBossPressureActions = 0;
            observedBossSkillPatterns = 0;
            observedBossSummonPressureActions = 0;
            observedBossPunishPatterns = 0;
            observedBossSummonReleases = 0;
            observedBossPressureBlocks = 0;
            observedSkill1ResponseUses = 0;
            highestPlayerSummonTier = 0;
            highestBossPressureTier = 0;
            highestBossSummonTier = 0;
            bossDamageFromPlayerSide = 0f;
            skill1ResponseDamageFromPlayerSide = 0f;
            skill1ResponseDamageUntilTime = 0f;
            lastObservedSummonSlot1Clashes = 0;
            lastObservedSummonSlot2Clashes = 0;
            lastObservedSummonSlot3Clashes = 0;
            lastObservedBossSummonClashes = 0;
            warmStartApplied = false;
            SetReviewSystemsEnabled(true);
            SetMarkers();
        }

        private void ApplyWarmStartOnce()
        {
            if (warmStartApplied)
            {
                return;
            }

            warmStartApplied = true;
            if (grantPlayerEnergyOnStart && energyLadder != null)
            {
                energyLadder.GrantCurrentTierEnergy(startingPlayerEnergy);
            }

            if (grantBossCostOnStart && bossPressureCostLadder != null)
            {
                bossPressureCostLadder.GrantCurrentTierCost(startingBossCost);
            }

            bossBarrageEmitter?.SetFiringEnabled(true);
            bossPressureCostLadder?.SetGainEnabled(true);
            bossPressureActionDirector?.SetActionsEnabled(true);
        }

        private bool HasMetReviewGoals()
        {
            return observedBossPressureActions >= requiredBossPressureActions
                && observedBossSkillPatterns >= requiredBossSkillPatterns
                && observedBossSummonPressureActions >= requiredBossSummonPressureActions
                && observedBossPunishPatterns >= requiredBossPunishPatterns
                && observedBossSummonReleases >= requiredBossSummonReleases
                && HasMetSummonExchangeGoal()
                && HasMetSkillResponseGoal()
                && bossDamageFromPlayerSide >= requiredBossDamage;
        }

        private bool HasMetSummonExchangeGoal()
        {
            return observedPlayerSummonUses >= requiredPlayerSummonUses
                && observedSupportSummonUses >= requiredSupportSummonUses
                && observedBossPressureBlocks >= requiredBossPressureBlocks
                && observedAllyPressureBlocks >= requiredAllyPressureBlocks
                && observedSummonClashes >= requiredSummonClashes;
        }

        private void ObserveSummonClashes()
        {
            observedSummonClashes += CountNewClashes(
                summonSlot1Action != null ? summonSlot1Action.LastSummonActorClashCount : 0,
                ref lastObservedSummonSlot1Clashes);
            observedSummonClashes += CountNewClashes(
                summonSlot2Action != null ? summonSlot2Action.LastSummonActorClashCount : 0,
                ref lastObservedSummonSlot2Clashes);
            observedSummonClashes += CountNewClashes(
                summonSlot3Action != null ? summonSlot3Action.LastSummonActorClashCount : 0,
                ref lastObservedSummonSlot3Clashes);
            observedSummonClashes += CountNewClashes(
                bossSummonPressureAction != null ? bossSummonPressureAction.LastSummonActorClashCount : 0,
                ref lastObservedBossSummonClashes);
        }

        private static int CountNewClashes(int currentCount, ref int lastObservedCount)
        {
            int delta = Mathf.Max(0, currentCount - lastObservedCount);
            lastObservedCount = Mathf.Max(lastObservedCount, currentCount);
            return delta;
        }

        private bool HasMetSkillResponseGoal()
        {
            return observedSkill1ResponseUses >= requiredSkill1ResponseUses
                && skill1ResponseDamageFromPlayerSide >= requiredSkill1ResponseDamage;
        }

        private void EnterCleared()
        {
            if (cleared || failed)
            {
                return;
            }

            cleared = true;
            SetReviewSystemsEnabled(false);
            SetMarkers();
        }

        private void EnterFailed()
        {
            if (cleared || failed)
            {
                return;
            }

            failed = true;
            SetReviewSystemsEnabled(false);
            SetMarkers();
        }

        private void SetReviewSystemsEnabled(bool enabled)
        {
            if (stopBarrageOnEnd && bossBarrageEmitter != null)
            {
                bossBarrageEmitter.SetFiringEnabled(enabled);
            }

            if (stopEnergyGainOnEnd && energyLadder != null)
            {
                energyLadder.SetGainEnabled(enabled);
            }

            if (stopBossPressureCostOnEnd && bossPressureCostLadder != null)
            {
                bossPressureCostLadder.SetGainEnabled(enabled);
            }

            if (stopBossPressureActionsOnEnd && bossPressureActionDirector != null)
            {
                bossPressureActionDirector.SetActionsEnabled(enabled);
            }
        }

        private void SetMarkers()
        {
            if (clearMarker != null)
            {
                clearMarker.SetActive(cleared);
            }

            if (failMarker != null)
            {
                failMarker.SetActive(failed);
            }
        }

        private void Subscribe()
        {
            if (skill1Action != null)
            {
                skill1Action.Skill1Used += HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used += HandlePlayerSummonUsed;
                summonSlot1Action.SummonPressureBlocked += HandleAllyPressureBlocked;
            }

            if (summonSlot2Action != null)
            {
                summonSlot2Action.SummonUsed += HandleSupportSummonUsed;
                summonSlot2Action.SummonPressureBlocked += HandleSupportPressureBlocked;
            }

            if (summonSlot3Action != null)
            {
                summonSlot3Action.SummonUsed += HandleSupportSummonUsed;
                summonSlot3Action.SummonPressureBlocked += HandleSupportPressureBlocked;
            }

            if (bossPressureActionDirector != null)
            {
                bossPressureActionDirector.ActionQueued += HandleBossPressureActionQueued;
            }

            if (bossSummonPressureAction != null)
            {
                bossSummonPressureAction.PressureSummonReleased += HandleBossPressureSummonReleased;
                bossSummonPressureAction.PressureSummonIntercepted += HandleBossPressureSummonIntercepted;
            }

            SubscribeBossHealth();
        }

        private void Unsubscribe()
        {
            if (skill1Action != null)
            {
                skill1Action.Skill1Used -= HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used -= HandlePlayerSummonUsed;
                summonSlot1Action.SummonPressureBlocked -= HandleAllyPressureBlocked;
            }

            if (summonSlot2Action != null)
            {
                summonSlot2Action.SummonUsed -= HandleSupportSummonUsed;
                summonSlot2Action.SummonPressureBlocked -= HandleSupportPressureBlocked;
            }

            if (summonSlot3Action != null)
            {
                summonSlot3Action.SummonUsed -= HandleSupportSummonUsed;
                summonSlot3Action.SummonPressureBlocked -= HandleSupportPressureBlocked;
            }

            if (bossPressureActionDirector != null)
            {
                bossPressureActionDirector.ActionQueued -= HandleBossPressureActionQueued;
            }

            if (bossSummonPressureAction != null)
            {
                bossSummonPressureAction.PressureSummonReleased -= HandleBossPressureSummonReleased;
                bossSummonPressureAction.PressureSummonIntercepted -= HandleBossPressureSummonIntercepted;
            }

            UnsubscribeBossHealth();
        }

        private void SubscribeBossHealth()
        {
            if (subscribedBossHealth == bossHealth)
            {
                return;
            }

            UnsubscribeBossHealth();
            if (bossHealth == null)
            {
                return;
            }

            subscribedBossHealth = bossHealth;
            subscribedBossHealth.Damaged += HandleBossDamaged;
        }

        private void UnsubscribeBossHealth()
        {
            if (subscribedBossHealth == null)
            {
                return;
            }

            subscribedBossHealth.Damaged -= HandleBossDamaged;
            subscribedBossHealth = null;
        }

        private void HandleSkill1Used(int tier)
        {
            observedSkill1Uses++;
            if (!HasMetSummonExchangeGoal())
            {
                return;
            }

            observedSkill1ResponseUses++;
            skill1ResponseDamageUntilTime = Mathf.Max(
                skill1ResponseDamageUntilTime,
                Time.time + skill1ResponseDamageWindowSeconds);
        }

        private void HandlePlayerSummonUsed(int tier)
        {
            observedPlayerSummonUses++;
            highestPlayerSummonTier = Mathf.Max(highestPlayerSummonTier, tier);
        }

        private void HandleSupportSummonUsed(PlayerSupportSummonSlotAction action, int tier)
        {
            observedSupportSummonUses++;
            HandlePlayerSummonUsed(tier);
        }

        private void HandleAllyPressureBlocked(int tier)
        {
            observedAllyPressureBlocks++;
            highestPlayerSummonTier = Mathf.Max(highestPlayerSummonTier, tier);
        }

        private void HandleSupportPressureBlocked(PlayerSupportSummonSlotAction action, int tier)
        {
            observedSupportPressureBlocks++;
            HandleAllyPressureBlocked(tier);
        }

        private void HandleBossPressureActionQueued(
            BossPressureActionDirector director,
            BossPressureActionKind actionKind,
            BossBarragePatternProfile pattern,
            int spentTier)
        {
            observedBossPressureActions++;
            highestBossPressureTier = Mathf.Max(highestBossPressureTier, spentTier);
            switch (actionKind)
            {
                case BossPressureActionKind.SkillPattern:
                    observedBossSkillPatterns++;
                    break;
                case BossPressureActionKind.SummonPressure:
                    observedBossSummonPressureActions++;
                    break;
                case BossPressureActionKind.PunishOverextend:
                    observedBossPunishPatterns++;
                    break;
            }
        }

        private void HandleBossPressureSummonReleased(BossSummonPressureAction action, int tier)
        {
            observedBossSummonReleases++;
            highestBossSummonTier = Mathf.Max(highestBossSummonTier, tier);
        }

        private void HandleBossPressureSummonIntercepted(BossSummonPressureAction action, int tier)
        {
            observedBossPressureBlocks++;
            highestBossSummonTier = Mathf.Max(highestBossSummonTier, tier);
        }

        private void HandleBossDamaged(DamageInfo damageInfo)
        {
            if (CombatTeamUtility.IsPlayerSide(damageInfo.SourceTeam))
            {
                bossDamageFromPlayerSide += damageInfo.Amount;
                if (Time.time <= skill1ResponseDamageUntilTime)
                {
                    skill1ResponseDamageFromPlayerSide += damageInfo.Amount;
                }
            }
        }
    }
}
