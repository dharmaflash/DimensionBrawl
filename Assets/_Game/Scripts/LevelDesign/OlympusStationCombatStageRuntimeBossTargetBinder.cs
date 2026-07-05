using System;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusStationCombatStageRuntimeBossTargetBinder : MonoBehaviour
    {
        private const string BossProxyRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const float BossTargetingDistance = 80f;

        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private PlayerLockTargetController lockTargetController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private ActionFoundationTestEncounter encounter;

        private bool appliedAfterStart;
        private CombatHealth subscribedBossHealth;
        private bool bindingLogged;

        private void Awake()
        {
            ApplyBindings();
        }

        private void OnEnable()
        {
            ApplyBindings();
        }

        private void OnDisable()
        {
            UnsubscribeBossDamageLog();
            bindingLogged = false;
        }

        private void Start()
        {
            ApplyBindings();
            appliedAfterStart = true;
        }

        private void LateUpdate()
        {
            if (appliedAfterStart)
            {
                return;
            }

            ApplyBindings();
            appliedAfterStart = true;
        }

        public void ApplyBindings()
        {
            ResolveReferences();
            ReleasePlayerInputLocks();
            if (playerHealth == null || bossHealth == null || targetSelector == null)
            {
                return;
            }

            playerHealth.ConfigureTeam(DamageTeam.Player);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            targetSelector.ConfigureTargetCandidates(new[] { bossHealth }, refreshNow: true);
            encounter?.ConfigureCombatants(playerHealth, bossHealth);

            SetField(targetSelector, "selectionRadius", BossTargetingDistance);
            SetField(targetSelector, "attackAimRadius", BossTargetingDistance);

            if (lockTargetController != null)
            {
                SetField(lockTargetController, "targetSelector", targetSelector);
                SetField(lockTargetController, "sourceHealth", playerHealth);
                SetField(lockTargetController, "softLockDistance", BossTargetingDistance);
                SetField(lockTargetController, "lockBreakDistance", BossTargetingDistance);
                SetField(lockTargetController, "softLockAngleDegrees", 120f);
                SetField(lockTargetController, "retainedLockAngleDegrees", 160f);
            }

            if (rangedBasicAttackAction != null)
            {
                SetField(rangedBasicAttackAction, "targetSelector", targetSelector);
                SetField(rangedBasicAttackAction, "lockTargetController", lockTargetController);
                SetField(rangedBasicAttackAction, "sourceHealth", playerHealth);
                SetField(rangedBasicAttackAction, "useAimAssist", true);
                SetField(rangedBasicAttackAction, "disableAimAssistWithManualInput", false);
                SetField(rangedBasicAttackAction, "aimAssistDistance", BossTargetingDistance);
                SetField(rangedBasicAttackAction, "hipAimAssistAngleDegrees", 45f);
                SetField(rangedBasicAttackAction, "aimedAimAssistAngleDegrees", 45f);
                SetField(rangedBasicAttackAction, "aimAssistMaxTurnDegrees", 45f);
                SetField(rangedBasicAttackAction, "cameraAimIgnoresNonTargetHits", true);
            }

            PlayerSummonSlot1Action summonSlot1 = FindFirstObjectByType<PlayerSummonSlot1Action>();
            if (summonSlot1 != null)
            {
                SetField(summonSlot1, "frontlineTargetHealth", bossHealth);
            }

            PlayerSupportSummonSlotAction[] supportSummons =
                FindObjectsByType<PlayerSupportSummonSlotAction>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < supportSummons.Length; i++)
            {
                SetField(supportSummons[i], "frontlineTargetHealth", bossHealth);
            }

            BossBarragePocketReviewOwner pocketOwner = FindFirstObjectByType<BossBarragePocketReviewOwner>();
            if (pocketOwner != null)
            {
                SetField(pocketOwner, "playerHealth", playerHealth);
                SetField(pocketOwner, "closeThreatHealth", null);
                SetField(pocketOwner, "bossHealth", bossHealth);
            }

            BossBarrageLaneReviewHud reviewHud = FindFirstObjectByType<BossBarrageLaneReviewHud>();
            if (reviewHud != null)
            {
                SetField(reviewHud, "playerHealth", playerHealth);
                SetField(reviewHud, "closeThreatHealth", null);
                SetField(reviewHud, "bossHealth", bossHealth);
            }

            MonoBehaviour combatHudBinder = FindFirstBehaviourByTypeName("BossBarrageLaneReviewCombatHudBinder");
            if (combatHudBinder != null)
            {
                SetField(combatHudBinder, "playerHealth", playerHealth);
                SetField(combatHudBinder, "bossHealth", bossHealth);
            }

            SubscribeBossDamageLog();
            LogBindingOnce();
        }

        private void ReleasePlayerInputLocks()
        {
            PlayerMovementController movement = ResolvePlayerComponent<PlayerMovementController>();
            EnableBehaviour(movement);
            movement?.ClearCinematicMoveInputSpeedScale();
            movement?.ClearActionMoveInputSpeedScale();
            movement?.SetMoveInput(Vector2.zero);
            movement?.SetLookInput(Vector2.zero);

            PlayerActionController actionController = ResolvePlayerComponent<PlayerActionController>();
            EnableBehaviour(actionController);
            actionController?.SetCinematicInputLocked(false);

            PlayerCombatModeController combatModeController = ResolvePlayerComponent<PlayerCombatModeController>();
            EnableBehaviour(combatModeController);
            combatModeController?.SetCinematicInputLocked(false);

            PlayerRangedAimController rangedAimController = ResolvePlayerComponent<PlayerRangedAimController>();
            EnableBehaviour(rangedAimController);
            rangedAimController?.SetAimHeld(false);
            rangedAimController?.SetFireAimHeld(false);
            rangedAimController?.SetAimInput(Vector2.zero);

            EnableBehaviour(rangedBasicAttackAction);
            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetCinematicInputLocked(false);
                rangedBasicAttackAction.SetFireHeld(false);
                rangedBasicAttackAction.SetExternalAimPreviewHeld(false);
                rangedBasicAttackAction.ClearAimInput();
            }

            PlayerSkill1Action skill1Action = ResolvePlayerComponent<PlayerSkill1Action>();
            EnableBehaviour(skill1Action);
            skill1Action?.SetCinematicInputLocked(false);

            PlayerSummonSlot1Action summonSlot1 = ResolvePlayerComponent<PlayerSummonSlot1Action>();
            EnableBehaviour(summonSlot1);
            summonSlot1?.SetCinematicInputLocked(false);

            PlayerSupportSummonSlotAction[] supportSummons =
                FindObjectsByType<PlayerSupportSummonSlotAction>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < supportSummons.Length; i++)
            {
                EnableBehaviour(supportSummons[i]);
                supportSummons[i]?.SetCinematicInputLocked(false);
            }
        }

        private void ResolveReferences()
        {
            targetSelector ??= FindFirstObjectByType<PlayerCombatTargetSelector>();
            lockTargetController ??= FindFirstObjectByType<PlayerLockTargetController>();
            rangedBasicAttackAction ??= FindFirstObjectByType<PlayerRangedBasicAttackAction>();
            encounter ??= FindFirstObjectByType<ActionFoundationTestEncounter>();

            if (playerHealth == null && targetSelector != null)
            {
                playerHealth = targetSelector.SelfHealth != null
                    ? targetSelector.SelfHealth
                    : targetSelector.GetComponent<CombatHealth>();
            }

            if (bossHealth == null)
            {
                GameObject bossRoot = GameObject.Find(BossProxyRootName);
                bossHealth = bossRoot != null ? bossRoot.GetComponent<CombatHealth>() : ResolveBossHealthByHeuristic();
            }
        }

        private T ResolvePlayerComponent<T>() where T : Component
        {
            if (playerHealth != null && playerHealth.TryGetComponent(out T component))
            {
                return component;
            }

            return FindFirstObjectByType<T>();
        }

        private static void EnableBehaviour(Behaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        private static CombatHealth ResolveBossHealthByHeuristic()
        {
            CombatHealth[] healths =
                FindObjectsByType<CombatHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health != null
                    && health.name.Contains("Boss", StringComparison.Ordinal)
                    && health.MaxHealth > 1000f)
                {
                    return health;
                }
            }

            return null;
        }

        private static MonoBehaviour FindFirstBehaviourByTypeName(string typeName)
        {
            MonoBehaviour[] behaviours =
                FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    return behaviour;
                }
            }

            return null;
        }

        private void SubscribeBossDamageLog()
        {
            if (subscribedBossHealth == bossHealth)
            {
                return;
            }

            UnsubscribeBossDamageLog();
            if (bossHealth == null)
            {
                return;
            }

            subscribedBossHealth = bossHealth;
            subscribedBossHealth.Damaged += HandleBossDamaged;
        }

        private void UnsubscribeBossDamageLog()
        {
            if (subscribedBossHealth == null)
            {
                return;
            }

            subscribedBossHealth.Damaged -= HandleBossDamaged;
            subscribedBossHealth = null;
        }

        private void HandleBossDamaged(DamageInfo damageInfo)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log(
                $"OlympusStation boss damaged. scene={sceneName} amount={damageInfo.Amount:0.###} " +
                $"sourceTeam={damageInfo.SourceTeam} hp={bossHealth.CurrentHealth:0.###}/{bossHealth.MaxHealth:0.###}",
                this);
        }

        private void LogBindingOnce()
        {
            if (bindingLogged)
            {
                return;
            }

            bindingLogged = true;
            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log(
                $"OlympusStation boss HP binder active. scene={sceneName} boss={bossHealth.name} " +
                $"hp={bossHealth.CurrentHealth:0.###}/{bossHealth.MaxHealth:0.###} targetCandidates=1",
                this);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
