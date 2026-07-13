using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneReviewCombatHudBinder : MonoBehaviour
    {
        private enum SummonStateTextKind
        {
            Locked,
            Cooldown,
            Ready,
            WaitingTier,
            WaitingShortage,
            WaitingShortageWithEta
        }

        private struct SummonStateTextCache
        {
            public bool Initialized;
            public SummonStateTextKind Kind;
            public int RequiredMana;
            public int ValueA;
            public int ValueB;
            public string Text;
        }

        [Header("UI")]
        [SerializeField] private CombatHudPresenter hudPresenter;
        [SerializeField] private CombatHudInputBridge inputBridge;
        [SerializeField] private CombatHudVirtualJoystick moveJoystick;
        [SerializeField] private BossBarrageLaneReviewOverlayHud overlayHud;
        [SerializeField] private BossBarrageLaneReviewTutorialGuide tutorialGuide;
        [SerializeField] private bool useSingleSummonPresentation;

        [Header("Combat State")]
        [FormerlySerializedAs("pocketReviewOwner")]
        [SerializeField] private BossBarrageEncounterController encounterController;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private BossPressureCostLadder bossCostLadder;

        [Header("Player Actions")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;

        [Header("Performance")]
        [SerializeField, Range(15f, 60f)] private float hudRefreshRate = 30f;

        private bool tutorialMoveInputLocked;
        private Coroutine hudRefreshRoutine;
        private float lastTutorialTickTime;
        private CombatHealth subscribedPlayerDamageHealth;
        private CombatHealth subscribedBossHealth;
        private PlayerCombatModeController subscribedCombatModeController;
        private PlayerRangedBasicAttackAction subscribedAimPreviewAction;
        private float lastPresentedPlayerHealth = float.NaN;
        private float lastPresentedPlayerMaxHealth = float.NaN;
        private float lastPresentedBossHealth = float.NaN;
        private float lastPresentedBossMaxHealth = float.NaN;
        private int cachedDodgeTenths = int.MinValue;
        private string cachedDodgeLabel;
        private bool ammoCacheInitialized;
        private bool cachedAmmoVisible;
        private bool cachedAmmoReloading;
        private int cachedAmmoCurrent = int.MinValue;
        private int cachedAmmoCapacity = int.MinValue;
        private int cachedAmmoReloadTenths = int.MinValue;
        private string cachedAmmoLabel;
        private bool combatModeCacheInitialized;
        private bool cachedCombatModeHasBoss;
        private bool cachedCombatModeMelee;
        private int cachedBossHealth = int.MinValue;
        private int cachedBossMaxHealth = int.MinValue;
        private string cachedCombatModeLabel;
        private bool energyInputCacheInitialized;
        private SummonEnergyRiskBand cachedEnergyRiskBand;
        private bool cachedEnergyCanSpend;
        private int cachedEnergyTier = int.MinValue;
        private int cachedEnergyMultiplierTenths = int.MinValue;
        private string cachedEnergyInputLabel;
        private int cachedSkillTier = int.MinValue;
        private string cachedSkillLabel;
        private SummonStateTextCache primarySummonTextCache;
        private SummonStateTextCache supportSummon2TextCache;
        private SummonStateTextCache supportSummon3TextCache;

        private void Awake()
        {
            if (hudPresenter == null)
            {
                hudPresenter = GetComponentInChildren<CombatHudPresenter>(includeInactive: true);
            }

            if (inputBridge == null)
            {
                inputBridge = GetComponentInChildren<CombatHudInputBridge>(includeInactive: true);
            }

            if (moveJoystick == null)
            {
                moveJoystick = GetComponentInChildren<CombatHudVirtualJoystick>(includeInactive: true);
            }

            if (tutorialGuide == null)
            {
                tutorialGuide = GetComponent<BossBarrageLaneReviewTutorialGuide>();
            }

            if (bossCostLadder == null)
            {
                bossCostLadder = FindFirstObjectByType<BossPressureCostLadder>();
            }

            ResolveMovementController();
            BindTutorialGuide();
        }

        private void OnEnable()
        {
            lastTutorialTickTime = Time.time;
            lastPresentedPlayerHealth = float.NaN;
            lastPresentedPlayerMaxHealth = float.NaN;
            lastPresentedBossHealth = float.NaN;
            lastPresentedBossMaxHealth = float.NaN;
            ResetTextCaches();
            BindTutorialGuide();

            if (inputBridge != null)
            {
                inputBridge.ActionRequested += HandleActionRequested;
                inputBridge.ActionHoldChanged += HandleActionHoldChanged;
            }

            RefreshHudNow();
            StartHudRefreshRoutine();
        }

        private void OnDisable()
        {
            StopHudRefreshRoutine();
            UnsubscribeImmediateReadoutEvents();
            UnsubscribeBossHealthReadout();
            UnsubscribePlayerDamageFeedback();

            if (inputBridge != null)
            {
                inputBridge.ActionRequested -= HandleActionRequested;
                inputBridge.ActionHoldChanged -= HandleActionHoldChanged;
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetFireHeld(false);
            }
            ClearTutorialMovementInputLock();
        }

        public void RefreshHudNow()
        {
            float scaledTime = Time.time;
            float tutorialDeltaTime = Mathf.Max(0f, scaledTime - lastTutorialTickTime);
            lastTutorialTickTime = scaledTime;
            RefreshHudState(tutorialDeltaTime);
        }

        private void StartHudRefreshRoutine()
        {
            if (hudRefreshRoutine != null
                || !isActiveAndEnabled
                || (hudPresenter == null && tutorialGuide == null))
            {
                return;
            }

            hudRefreshRoutine = StartCoroutine(RefreshHudAtReviewedRate());
        }

        private void StopHudRefreshRoutine()
        {
            if (hudRefreshRoutine == null)
            {
                return;
            }

            StopCoroutine(hudRefreshRoutine);
            hudRefreshRoutine = null;
        }

        private IEnumerator RefreshHudAtReviewedRate()
        {
            var refreshDelay = new WaitForSecondsRealtime(1f / Mathf.Max(1f, hudRefreshRate));
            while (isActiveAndEnabled)
            {
                yield return refreshDelay;
                if (!isActiveAndEnabled)
                {
                    break;
                }

                RefreshHudNow();
            }

            hudRefreshRoutine = null;
        }

        private void RefreshHudState(float tutorialDeltaTime)
        {
            if (tutorialGuide != null)
            {
                tutorialGuide.TickTutorial(tutorialDeltaTime);
            }

            UpdateTutorialMovementInputLock();
            if (isActiveAndEnabled)
            {
                SubscribePlayerDamageFeedback();
                SubscribeBossHealthReadout();
                SubscribeImmediateReadoutEvents();
            }

            if (hudPresenter == null)
            {
                return;
            }

            SyncBossHudVisibility();
            UpdateAimReticleReadout();
            UpdateHealthReadouts();
            UpdatePrimaryReadouts();
            UpdateActionReadouts();
            UpdateSummonReadouts();
            UpdateTutorialGuideReadouts();
        }

        private void SubscribeImmediateReadoutEvents()
        {
            if (subscribedCombatModeController != combatModeController)
            {
                if (subscribedCombatModeController != null)
                {
                    subscribedCombatModeController.CombatModeChanged -= HandleCombatModeChanged;
                }

                subscribedCombatModeController = combatModeController;
                if (subscribedCombatModeController != null)
                {
                    subscribedCombatModeController.CombatModeChanged += HandleCombatModeChanged;
                }
            }

            if (subscribedAimPreviewAction == rangedBasicAttackAction)
            {
                return;
            }

            if (subscribedAimPreviewAction != null)
            {
                subscribedAimPreviewAction.AimPreviewStateChanged -= HandleAimPreviewStateChanged;
            }

            subscribedAimPreviewAction = rangedBasicAttackAction;
            if (subscribedAimPreviewAction != null)
            {
                subscribedAimPreviewAction.AimPreviewStateChanged += HandleAimPreviewStateChanged;
            }
        }

        private void UnsubscribeImmediateReadoutEvents()
        {
            if (subscribedCombatModeController != null)
            {
                subscribedCombatModeController.CombatModeChanged -= HandleCombatModeChanged;
            }

            if (subscribedAimPreviewAction != null)
            {
                subscribedAimPreviewAction.AimPreviewStateChanged -= HandleAimPreviewStateChanged;
            }

            subscribedCombatModeController = null;
            subscribedAimPreviewAction = null;
        }

        private void HandleCombatModeChanged(PlayerCombatMode _)
        {
            if (hudPresenter == null)
            {
                return;
            }

            UpdateAimReticleReadout();
            UpdatePrimaryReadouts();
            UpdateActionReadouts();
        }

        private void HandleAimPreviewStateChanged()
        {
            if (hudPresenter != null)
            {
                UpdateAimReticleReadout();
            }
        }

        private void UpdatePrimaryReadouts()
        {
            string objective = tutorialGuide != null && tutorialGuide.HasReadoutOverride
                ? tutorialGuide.CurrentObjective
                : encounterController != null
                    ? encounterController.ObjectiveCue
                    : "Survive the boss lane.";
            hudPresenter.SetObjective(objective);
            hudPresenter.SetTimer(ResolveRemainingSeconds());
            if (bossCostLadder != null)
            {
                hudPresenter.SetBossResource(
                    bossCostLadder.CurrentTierCost,
                    Mathf.Max(1f, bossCostLadder.CurrentTierTarget));
            }

            if (energyLadder != null)
            {
                hudPresenter.SetResource(energyLadder.CurrentMana, Mathf.Max(1f, energyLadder.MaxMana));
                hudPresenter.SetInputMode(ResolveEnergyInputModeLabel());
            }

            hudPresenter.SetAmmo(ResolveAmmoReadout(), rangedBasicAttackAction != null && rangedBasicAttackAction.IsReloading);

            string feedback = tutorialGuide != null && tutorialGuide.HasReadoutOverride
                ? tutorialGuide.CurrentPrompt
                : ResolveCombatModeLabel();
            hudPresenter.SetActionFeedbackText(feedback);
        }

        private void UpdateAimReticleReadout()
        {
            bool rangedMode = combatModeController == null || combatModeController.IsRangedMode;
            bool aimActive = rangedBasicAttackAction != null && rangedBasicAttackAction.IsAimPreviewActive;
            hudPresenter.SetAimReticleVisible(rangedMode, aimActive);
        }

        private void UpdateHealthReadouts()
        {
            if (hudPresenter == null)
            {
                return;
            }

            if (playerHealth != null
                && (HealthValueChanged(lastPresentedPlayerHealth, playerHealth.CurrentHealth)
                    || HealthValueChanged(lastPresentedPlayerMaxHealth, playerHealth.MaxHealth)))
            {
                lastPresentedPlayerHealth = playerHealth.CurrentHealth;
                lastPresentedPlayerMaxHealth = playerHealth.MaxHealth;
                hudPresenter.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (bossHealth != null
                && (HealthValueChanged(lastPresentedBossHealth, bossHealth.CurrentHealth)
                    || HealthValueChanged(lastPresentedBossMaxHealth, bossHealth.MaxHealth)))
            {
                lastPresentedBossHealth = bossHealth.CurrentHealth;
                lastPresentedBossMaxHealth = bossHealth.MaxHealth;
                hudPresenter.SetBossHealth(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            }
        }

        private void SyncBossHudVisibility()
        {
            if (hudPresenter != null)
            {
                hudPresenter.SetBossHudVisible(bossHealth != null);
            }
        }

        private static bool HealthValueChanged(float previous, float current)
        {
            return float.IsNaN(previous) || !Mathf.Approximately(previous, current);
        }

        private void UpdateActionReadouts()
        {
            bool canSpend = energyLadder != null && energyLadder.CanSpend;
            int tier = canSpend ? energyLadder.AvailableTier : energyLadder != null ? energyLadder.ChargingTier : 0;
            hudPresenter.SetSkillCooldown(CombatHudActionId.BasicAttack, 0f, ResolveBasicAttackLabel());
            hudPresenter.SetSkillCooldown(
                CombatHudActionId.Dodge,
                ResolveDodgeCooldownFill01(),
                ResolveDodgeLabel(),
                actionController != null ? actionController.DodgeCooldownRemaining : -1f);
            hudPresenter.SetSkillCooldown(
                CombatHudActionId.Skill1,
                canSpend ? 0f : 1f,
                ResolveSkillLabel(tier));
            hudPresenter.SetSkillCooldown(CombatHudActionId.Ultimate, 0f, "SWAP");
        }

        private void UpdateSummonReadouts()
        {
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot1, true);
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot1,
                "S1",
                ResolvePrimarySummonState(),
                IsPrimarySummonReady(),
                ResolvePrimarySummonAvailabilityFill01());
            bool showSupportSummonSlots = !useSingleSummonPresentation;
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot2, showSupportSummonSlots);
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot3, showSupportSummonSlots);
            if (!showSupportSummonSlots)
            {
                return;
            }

            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot2,
                "S2",
                ResolveSupportSummonState(summonSlot2Action, ref supportSummon2TextCache),
                IsSupportSummonReady(summonSlot2Action),
                ResolveSupportSummonAvailabilityFill01(summonSlot2Action));
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot3,
                "S3",
                ResolveSupportSummonState(summonSlot3Action, ref supportSummon3TextCache),
                IsSupportSummonReady(summonSlot3Action),
                ResolveSupportSummonAvailabilityFill01(summonSlot3Action));
        }

        private void UpdateTutorialGuideReadouts()
        {
            if (tutorialGuide == null || !tutorialGuide.HasActiveStep)
            {
                hudPresenter.SetGuideFocus(CombatHudActionId.None, dimUnfocused: false);
                return;
            }

            hudPresenter.SetGuideFocus(
                tutorialGuide.CurrentFocusAction,
                tutorialGuide.CurrentFocusDimUnfocusedActions);
        }

        private void HandleActionRequested(CombatHudActionId actionId)
        {
            switch (actionId)
            {
                case CombatHudActionId.BasicAttack:
                    QueueBasicAttack();
                    break;
                case CombatHudActionId.Dodge:
                    if (actionController != null)
                    {
                        actionController.QueueDodge();
                    }
                    break;
                case CombatHudActionId.Skill1:
                    if (skill1Action != null)
                    {
                        skill1Action.QueueSkill1();
                    }
                    break;
                case CombatHudActionId.Ultimate:
                    if (combatModeController != null)
                    {
                        combatModeController.QueueCombatModeSwap();
                    }
                    break;
                case CombatHudActionId.SummonSlot1:
                    if (summonSlot1Action != null)
                    {
                        summonSlot1Action.QueueSummonSlot1();
                    }
                    break;
                case CombatHudActionId.SummonSlot2:
                    if (summonSlot2Action != null)
                    {
                        summonSlot2Action.QueueSummon();
                    }
                    break;
                case CombatHudActionId.SummonSlot3:
                    if (summonSlot3Action != null)
                    {
                        summonSlot3Action.QueueSummon();
                    }
                    break;
                case CombatHudActionId.Pause:
                    if (overlayHud != null)
                    {
                        overlayHud.OpenPauseMenu();
                    }
                    break;
            }
        }

        private void HandleActionHoldChanged(CombatHudActionId actionId, bool held)
        {
            if (actionId == CombatHudActionId.BasicAttack)
            {
                SetBasicAttackHeld(held);
            }
        }

        private void QueueBasicAttack()
        {
            if (combatModeController == null || combatModeController.IsRangedMode)
            {
                if (rangedBasicAttackAction != null)
                {
                    rangedBasicAttackAction.QueueFire();
                }
                return;
            }

            if (actionController != null)
            {
                actionController.QueueBasicAttack();
            }
        }

        private void SetBasicAttackHeld(bool held)
        {
            if (combatModeController == null || combatModeController.IsRangedMode)
            {
                if (actionController != null)
                {
                    actionController.SetBasicAttackHeld(false);
                }

                if (rangedBasicAttackAction != null)
                {
                    rangedBasicAttackAction.SetFireHeld(held);
                }
                return;
            }

            if (actionController != null)
            {
                actionController.SetBasicAttackHeld(held);
            }

            if (!held && rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetFireHeld(false);
            }
        }

        private void BindTutorialGuide()
        {
            if (tutorialGuide == null)
            {
                return;
            }

            tutorialGuide.BindRuntimeContext(
                encounterController,
                energyLadder,
                actionController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action);
        }

        private void SubscribePlayerDamageFeedback()
        {
            ResolvePlayerHealth();
            if (subscribedPlayerDamageHealth == playerHealth)
            {
                return;
            }

            UnsubscribePlayerDamageFeedback();
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.Damaged += HandlePlayerDamaged;
            playerHealth.DamageBlockedByInvulnerability += HandlePlayerDamageBlocked;
            subscribedPlayerDamageHealth = playerHealth;
        }

        private void UnsubscribePlayerDamageFeedback()
        {
            if (subscribedPlayerDamageHealth == null)
            {
                return;
            }

            subscribedPlayerDamageHealth.Damaged -= HandlePlayerDamaged;
            subscribedPlayerDamageHealth.DamageBlockedByInvulnerability -= HandlePlayerDamageBlocked;
            subscribedPlayerDamageHealth = null;
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            UpdateHealthReadouts();
            ShowPlayerDamageOverlayForHostileHit(damageInfo);
        }

        private void HandlePlayerDamageBlocked(DamageInfo damageInfo)
        {
            ShowPlayerDamageOverlayForHostileHit(damageInfo);
        }

        private void SubscribeBossHealthReadout()
        {
            if (subscribedBossHealth == bossHealth)
            {
                return;
            }

            UnsubscribeBossHealthReadout();
            if (bossHealth == null)
            {
                return;
            }

            bossHealth.Damaged += HandleBossDamaged;
            subscribedBossHealth = bossHealth;
        }

        private void UnsubscribeBossHealthReadout()
        {
            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Damaged -= HandleBossDamaged;
            }

            subscribedBossHealth = null;
        }

        private void HandleBossDamaged(DamageInfo _)
        {
            UpdateHealthReadouts();
        }

        private void ShowPlayerDamageOverlayForHostileHit(DamageInfo damageInfo)
        {
            if (playerHealth != null && CombatTeamUtility.AreAllied(damageInfo.SourceTeam, playerHealth.Team))
            {
                return;
            }

            if (hudPresenter != null)
            {
                hudPresenter.ShowPlayerDamageOverlay();
            }
        }

        private void ResolvePlayerHealth()
        {
            if (playerHealth != null)
            {
                return;
            }

            if (actionController != null)
            {
                playerHealth = actionController.GetComponent<CombatHealth>();
            }

            if (playerHealth == null && movementController != null)
            {
                playerHealth = movementController.GetComponent<CombatHealth>();
            }

            if (playerHealth == null && rangedBasicAttackAction != null)
            {
                playerHealth = rangedBasicAttackAction.GetComponent<CombatHealth>();
            }
        }

        private void ResolveMovementController()
        {
            if (movementController != null)
            {
                return;
            }

            if (actionController != null)
            {
                movementController = actionController.GetComponent<PlayerMovementController>();
            }

            if (movementController == null && rangedBasicAttackAction != null)
            {
                movementController = rangedBasicAttackAction.GetComponent<PlayerMovementController>();
            }

            if (movementController == null && combatModeController != null)
            {
                movementController = combatModeController.GetComponent<PlayerMovementController>();
            }
        }

        private void UpdateTutorialMovementInputLock()
        {
            bool shouldLock = tutorialGuide != null && tutorialGuide.ShouldBlockMoveInput;
            if (tutorialMoveInputLocked == shouldLock)
            {
                return;
            }

            tutorialMoveInputLocked = shouldLock;
            ResolveMovementController();
            if (moveJoystick != null)
            {
                moveJoystick.SetInputBlocked(shouldLock);
            }
            if (shouldLock)
            {
                if (movementController != null)
                {
                    movementController.SetMoveInput(Vector2.zero);
                    movementController.SetSharedMoveInputBlocked(true);
                }
                return;
            }

            if (movementController != null)
            {
                movementController.SetSharedMoveInputBlocked(false);
            }
        }

        private void ClearTutorialMovementInputLock()
        {
            if (!tutorialMoveInputLocked)
            {
                return;
            }

            tutorialMoveInputLocked = false;
            if (moveJoystick != null)
            {
                moveJoystick.SetInputBlocked(false);
            }
            ResolveMovementController();
            if (movementController != null)
            {
                movementController.SetSharedMoveInputBlocked(false);
            }
        }

        private float ResolveRemainingSeconds()
        {
            if (encounterController == null)
            {
                return 0f;
            }

            float target = encounterController.StageProfile != null
                ? encounterController.StageProfile.TargetDurationSeconds
                : 90f;
            return Mathf.Max(0f, target - encounterController.ElapsedSeconds);
        }

        private string ResolveBasicAttackLabel()
        {
            return combatModeController != null && combatModeController.IsMeleeMode ? "SLASH" : "FIRE";
        }

        private string ResolveDodgeLabel()
        {
            int displayedTenths = actionController == null || actionController.IsDodgeReady
                ? -1
                : Mathf.Max(0, Mathf.RoundToInt(actionController.DodgeCooldownRemaining * 10f));
            if (displayedTenths == cachedDodgeTenths && cachedDodgeLabel != null)
            {
                return cachedDodgeLabel;
            }

            cachedDodgeTenths = displayedTenths;
            cachedDodgeLabel = displayedTenths < 0
                ? "DODGE"
                : $"DODGE\n{displayedTenths * 0.1f:0.0}s";
            return cachedDodgeLabel;
        }

        private float ResolveDodgeCooldownFill01()
        {
            if (actionController == null || actionController.DodgeCooldownSeconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(actionController.DodgeCooldownRemaining / actionController.DodgeCooldownSeconds);
        }

        private string ResolveAmmoReadout()
        {
            bool visible = (combatModeController == null || !combatModeController.IsMeleeMode)
                && rangedBasicAttackAction != null
                && rangedBasicAttackAction.UsesMagazineReload;
            bool reloading = visible && rangedBasicAttackAction.IsReloading;
            int currentAmmo = visible ? rangedBasicAttackAction.CurrentAmmo : 0;
            int capacity = visible ? rangedBasicAttackAction.MagazineSize : 0;
            int reloadTenths = reloading
                ? Mathf.Max(0, Mathf.RoundToInt(rangedBasicAttackAction.ReloadRemaining * 10f))
                : -1;

            if (ammoCacheInitialized
                && cachedAmmoVisible == visible
                && cachedAmmoReloading == reloading
                && cachedAmmoCurrent == currentAmmo
                && cachedAmmoCapacity == capacity
                && cachedAmmoReloadTenths == reloadTenths)
            {
                return cachedAmmoLabel;
            }

            ammoCacheInitialized = true;
            cachedAmmoVisible = visible;
            cachedAmmoReloading = reloading;
            cachedAmmoCurrent = currentAmmo;
            cachedAmmoCapacity = capacity;
            cachedAmmoReloadTenths = reloadTenths;
            cachedAmmoLabel = !visible
                ? string.Empty
                : reloading
                    ? $"{currentAmmo}/{capacity} RLD {reloadTenths * 0.1f:0.0}"
                    : $"{currentAmmo}/{capacity}";
            return cachedAmmoLabel;
        }

        private string ResolveCombatModeLabel()
        {
            bool hasBoss = bossHealth != null;
            bool meleeMode = combatModeController != null && combatModeController.IsMeleeMode;
            int currentHealth = hasBoss
                ? Mathf.CeilToInt(Mathf.Max(0f, bossHealth.CurrentHealth))
                : 0;
            int maxHealth = hasBoss
                ? Mathf.CeilToInt(Mathf.Max(0f, bossHealth.MaxHealth))
                : 0;
            if (combatModeCacheInitialized
                && cachedCombatModeHasBoss == hasBoss
                && cachedCombatModeMelee == meleeMode
                && cachedBossHealth == currentHealth
                && cachedBossMaxHealth == maxHealth)
            {
                return cachedCombatModeLabel;
            }

            combatModeCacheInitialized = true;
            cachedCombatModeHasBoss = hasBoss;
            cachedCombatModeMelee = meleeMode;
            cachedBossHealth = currentHealth;
            cachedBossMaxHealth = maxHealth;
            cachedCombatModeLabel = hasBoss
                ? $"Boss {currentHealth}/{maxHealth}"
                : meleeMode ? "Melee" : "Ranged";
            return cachedCombatModeLabel;
        }

        private string ResolveEnergyInputModeLabel()
        {
            if (energyLadder == null)
            {
                return "EN";
            }

            SummonEnergyRiskBand riskBand = energyLadder.CurrentRiskBand;
            bool canSpend = energyLadder.CanSpend;
            int tier = canSpend ? energyLadder.AvailableTier : energyLadder.ChargingTier;
            int multiplierTenths = Mathf.RoundToInt(energyLadder.CurrentGainMultiplier * 10f);
            if (energyInputCacheInitialized
                && cachedEnergyRiskBand == riskBand
                && cachedEnergyCanSpend == canSpend
                && cachedEnergyTier == tier
                && cachedEnergyMultiplierTenths == multiplierTenths)
            {
                return cachedEnergyInputLabel;
            }

            energyInputCacheInitialized = true;
            cachedEnergyRiskBand = riskBand;
            cachedEnergyCanSpend = canSpend;
            cachedEnergyTier = tier;
            cachedEnergyMultiplierTenths = multiplierTenths;
            string band = riskBand switch
            {
                SummonEnergyRiskBand.ForwardRisk => "FRONT",
                SummonEnergyRiskBand.MidCharge => "MID",
                _ => "BACK"
            };
            cachedEnergyInputLabel = canSpend
                ? $"{band} READY LV{tier} x{multiplierTenths * 0.1f:0.0}"
                : $"{band} EN LV{tier} x{multiplierTenths * 0.1f:0.0}";
            return cachedEnergyInputLabel;
        }

        private string ResolveSkillLabel(int tier)
        {
            if (tier == cachedSkillTier && cachedSkillLabel != null)
            {
                return cachedSkillLabel;
            }

            cachedSkillTier = tier;
            cachedSkillLabel = tier > 0 ? $"SKILL LV{tier}" : "SKILL";
            return cachedSkillLabel;
        }

        private bool IsPrimarySummonReady()
        {
            return summonSlot1Action != null
                && energyLadder != null
                && !summonSlot1Action.IsSlotOnCooldown
                && energyLadder.CanSpendMana(summonSlot1Action.RequiredSummonMana);
        }

        private string ResolvePrimarySummonState()
        {
            if (summonSlot1Action == null || energyLadder == null)
            {
                return GetSummonStateText(
                    ref primarySummonTextCache,
                    SummonStateTextKind.Locked,
                    0,
                    0,
                    0);
            }

            int requiredMana = Mathf.CeilToInt(Mathf.Max(1f, summonSlot1Action.RequiredSummonMana));
            if (summonSlot1Action.IsSlotOnCooldown)
            {
                int cooldownTenths = Mathf.Max(
                    0,
                    Mathf.RoundToInt(summonSlot1Action.SlotCooldownRemaining * 10f));
                return GetSummonStateText(
                    ref primarySummonTextCache,
                    SummonStateTextKind.Cooldown,
                    requiredMana,
                    cooldownTenths,
                    0);
            }

            if (IsPrimarySummonReady())
            {
                return GetSummonStateText(
                    ref primarySummonTextCache,
                    SummonStateTextKind.Ready,
                    requiredMana,
                    energyLadder.AvailableTier,
                    0);
            }

            return ResolveWaitingSummonState(
                ref primarySummonTextCache,
                requiredMana,
                summonSlot1Action.RequiredSummonMana);
        }

        private bool IsSupportSummonReady(PlayerSupportSummonSlotAction action)
        {
            return action != null
                && energyLadder != null
                && !action.IsSlotOnCooldown
                && energyLadder.AvailableTier >= action.MinimumSummonTier
                && energyLadder.CanSpendMana(action.RequiredSummonMana);
        }

        private string ResolveSupportSummonState(
            PlayerSupportSummonSlotAction action,
            ref SummonStateTextCache textCache)
        {
            if (action == null || energyLadder == null)
            {
                return GetSummonStateText(
                    ref textCache,
                    SummonStateTextKind.Locked,
                    0,
                    0,
                    0);
            }

            int requiredMana = Mathf.CeilToInt(Mathf.Max(1f, action.RequiredSummonMana));
            if (action.IsSlotOnCooldown)
            {
                int cooldownTenths = Mathf.Max(0, Mathf.RoundToInt(action.SlotCooldownRemaining * 10f));
                return GetSummonStateText(
                    ref textCache,
                    SummonStateTextKind.Cooldown,
                    requiredMana,
                    cooldownTenths,
                    0);
            }

            if (IsSupportSummonReady(action))
            {
                return GetSummonStateText(
                    ref textCache,
                    SummonStateTextKind.Ready,
                    requiredMana,
                    energyLadder.AvailableTier,
                    0);
            }

            return ResolveWaitingSummonState(
                ref textCache,
                requiredMana,
                ResolveSupportGateMana(action));
        }

        private float ResolvePrimarySummonAvailabilityFill01()
        {
            if (summonSlot1Action == null || energyLadder == null)
            {
                return 0f;
            }

            if (summonSlot1Action.IsSlotOnCooldown)
            {
                return ResolveCooldownProgress01(
                    summonSlot1Action.SlotCooldownRemaining,
                    summonSlot1Action.SlotCooldownSeconds);
            }

            return ResolveManaProgress01(summonSlot1Action.RequiredSummonMana);
        }

        private float ResolveSupportSummonAvailabilityFill01(PlayerSupportSummonSlotAction action)
        {
            if (action == null || energyLadder == null)
            {
                return 0f;
            }

            if (action.IsSlotOnCooldown)
            {
                return ResolveCooldownProgress01(action.SlotCooldownRemaining, action.SlotCooldownSeconds);
            }

            return ResolveManaProgress01(ResolveSupportGateMana(action));
        }

        private float ResolveSupportGateMana(PlayerSupportSummonSlotAction action)
        {
            if (action == null)
            {
                return 1f;
            }

            float minimumTierMana = energyLadder != null
                ? energyLadder.GetMinimumManaForTier(action.MinimumSummonTier)
                : 1f;
            return Mathf.Max(action.RequiredSummonMana, minimumTierMana);
        }

        private string ResolveWaitingSummonState(
            ref SummonStateTextCache textCache,
            int displayedRequiredMana,
            float requiredMana)
        {
            float shortage = energyLadder.GetManaShortage(requiredMana);
            if (shortage <= 0.001f)
            {
                return GetSummonStateText(
                    ref textCache,
                    SummonStateTextKind.WaitingTier,
                    displayedRequiredMana,
                    energyLadder.ChargingTier,
                    0);
            }

            float seconds = energyLadder.EstimateSecondsToMana(requiredMana);
            int displayedShortage = Mathf.CeilToInt(shortage);
            if (seconds >= 0f)
            {
                return GetSummonStateText(
                    ref textCache,
                    SummonStateTextKind.WaitingShortageWithEta,
                    displayedRequiredMana,
                    displayedShortage,
                    Mathf.CeilToInt(seconds));
            }

            return GetSummonStateText(
                ref textCache,
                SummonStateTextKind.WaitingShortage,
                displayedRequiredMana,
                displayedShortage,
                0);
        }

        private float ResolveManaProgress01(float requiredMana)
        {
            if (energyLadder == null)
            {
                return 0f;
            }

            return Mathf.Clamp01(energyLadder.CurrentMana / Mathf.Max(1f, requiredMana));
        }

        private static float ResolveCooldownProgress01(float cooldownRemaining, float cooldownSeconds)
        {
            if (cooldownSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - Mathf.Max(0f, cooldownRemaining) / cooldownSeconds);
        }

        private static string GetSummonStateText(
            ref SummonStateTextCache cache,
            SummonStateTextKind kind,
            int requiredMana,
            int valueA,
            int valueB)
        {
            if (cache.Initialized
                && cache.Kind == kind
                && cache.RequiredMana == requiredMana
                && cache.ValueA == valueA
                && cache.ValueB == valueB)
            {
                return cache.Text;
            }

            cache.Initialized = true;
            cache.Kind = kind;
            cache.RequiredMana = requiredMana;
            cache.ValueA = valueA;
            cache.ValueB = valueB;
            cache.Text = kind switch
            {
                SummonStateTextKind.Locked => "LOCKED",
                SummonStateTextKind.Cooldown => $"{requiredMana}EN\nCD {valueA * 0.1f:0.0}s",
                SummonStateTextKind.Ready => $"{requiredMana}EN\nREADY LV{valueA}",
                SummonStateTextKind.WaitingTier => $"{requiredMana}EN\nLV{valueA}",
                SummonStateTextKind.WaitingShortage => $"{requiredMana}EN\n+{valueA}",
                SummonStateTextKind.WaitingShortageWithEta => $"{requiredMana}EN\n+{valueA} / {valueB}s",
                _ => "LOCKED"
            };
            return cache.Text;
        }

        private void ResetTextCaches()
        {
            cachedDodgeTenths = int.MinValue;
            cachedDodgeLabel = null;
            ammoCacheInitialized = false;
            cachedAmmoLabel = null;
            combatModeCacheInitialized = false;
            cachedCombatModeLabel = null;
            energyInputCacheInitialized = false;
            cachedEnergyInputLabel = null;
            cachedSkillTier = int.MinValue;
            cachedSkillLabel = null;
            primarySummonTextCache = default;
            supportSummon2TextCache = default;
            supportSummon3TextCache = default;
        }
    }
}
