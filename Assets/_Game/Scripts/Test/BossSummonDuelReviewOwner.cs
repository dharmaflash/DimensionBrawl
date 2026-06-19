using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Test
{
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
        [SerializeField, Min(0)] private int requiredBossSummonReleases = 1;
        [SerializeField, Min(0)] private int requiredPlayerSummonUses = 2;
        [SerializeField, Min(0)] private int requiredAllyPressureBlocks = 1;
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
        private int observedAllyPressureBlocks;
        private int observedBossPressureActions;
        private int observedBossSummonReleases;
        private int observedBossPressureBlocks;
        private int observedSkill1ResponseUses;
        private int highestPlayerSummonTier;
        private int highestBossPressureTier;
        private int highestBossSummonTier;
        private float bossDamageFromPlayerSide;
        private float skill1ResponseDamageFromPlayerSide;
        private float skill1ResponseDamageUntilTime;

        public bool IsCleared => cleared;
        public bool IsFailed => failed;
        public int ObservedSkill1Uses => observedSkill1Uses;
        public int ObservedPlayerSummonUses => observedPlayerSummonUses;
        public int ObservedAllyPressureBlocks => observedAllyPressureBlocks;
        public int ObservedBossPressureActions => observedBossPressureActions;
        public int ObservedBossSummonReleases => observedBossSummonReleases;
        public int ObservedBossPressureBlocks => observedBossPressureBlocks;
        public int ObservedSkill1ResponseUses => observedSkill1ResponseUses;
        public int HighestPlayerSummonTier => highestPlayerSummonTier;
        public int HighestBossPressureTier => highestBossPressureTier;
        public int HighestBossSummonTier => highestBossSummonTier;
        public float BossDamageFromPlayerSide => bossDamageFromPlayerSide;
        public float Skill1ResponseDamageFromPlayerSide => skill1ResponseDamageFromPlayerSide;
        public int RequiredBossPressureActions => requiredBossPressureActions;
        public int RequiredBossSummonReleases => requiredBossSummonReleases;
        public int RequiredPlayerSummonUses => requiredPlayerSummonUses;
        public int RequiredAllyPressureBlocks => requiredAllyPressureBlocks;
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
                    return "Duel loop verified: cost, boss pressure, summon exchange, and counter damage all happened";
                }

                if (failed)
                {
                    return "Player defeated; use backline safety, dodge, then build EN for SummonSlot1";
                }

                if (observedBossPressureActions < requiredBossPressureActions)
                {
                    return "Move between safety and forward risk while the boss builds costed pressure";
                }

                if (observedBossSummonReleases < requiredBossSummonReleases)
                {
                    return "Bait the boss toward LV2 pressure so its summon screen enters the lane";
                }

                if (observedPlayerSummonUses < requiredPlayerSummonUses)
                {
                    return "Build EN and spend SummonSlot1 more than once to contest the frontline";
                }

                if (observedAllyPressureBlocks < requiredAllyPressureBlocks)
                {
                    return "Use SummonSlot1 where its screen can block boss fire";
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
            + $"bossSummon {observedBossSummonReleases}/{requiredBossSummonReleases} "
            + $"summon {observedPlayerSummonUses}/{requiredPlayerSummonUses} "
            + $"block {observedAllyPressureBlocks}/{requiredAllyPressureBlocks} "
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
            observedAllyPressureBlocks = 0;
            observedBossPressureActions = 0;
            observedBossSummonReleases = 0;
            observedBossPressureBlocks = 0;
            observedSkill1ResponseUses = 0;
            highestPlayerSummonTier = 0;
            highestBossPressureTier = 0;
            highestBossSummonTier = 0;
            bossDamageFromPlayerSide = 0f;
            skill1ResponseDamageFromPlayerSide = 0f;
            skill1ResponseDamageUntilTime = 0f;
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
                && observedBossSummonReleases >= requiredBossSummonReleases
                && HasMetSummonExchangeGoal()
                && HasMetSkillResponseGoal()
                && bossDamageFromPlayerSide >= requiredBossDamage;
        }

        private bool HasMetSummonExchangeGoal()
        {
            return observedPlayerSummonUses >= requiredPlayerSummonUses
                && observedAllyPressureBlocks >= requiredAllyPressureBlocks;
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

        private void HandleAllyPressureBlocked(int tier)
        {
            observedAllyPressureBlocks++;
            highestPlayerSummonTier = Mathf.Max(highestPlayerSummonTier, tier);
        }

        private void HandleBossPressureActionQueued(
            BossPressureActionDirector director,
            BossPressureActionKind actionKind,
            BossBarragePatternProfile pattern,
            int spentTier)
        {
            observedBossPressureActions++;
            highestBossPressureTier = Mathf.Max(highestBossPressureTier, spentTier);
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
