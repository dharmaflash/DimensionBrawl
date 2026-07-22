using System;
using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class OneRowCombatHudBinder : MonoBehaviour
    {
        private const string DefaultObjectiveText = "Defeat the enemy.";

        [Header("UI")]
        [SerializeField] private CombatHudPresenter hudPresenter;
        [SerializeField] private CombatHudInputBridge inputBridge;
        [SerializeField] private CombatHudVirtualJoystick moveJoystick;
        [SerializeField] private MonoBehaviour sessionOverlayBehaviour;

        [Header("Combat State")]
        [SerializeField] private CombatEncounterController encounterController;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private string objectiveText = DefaultObjectiveText;

        [Header("Player Actions")]
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;

        [Header("Performance")]
        [SerializeField, Range(15f, 60f)] private float hudRefreshRate = 30f;

        private Coroutine hudRefreshRoutine;
        private ICombatSessionOverlay sessionOverlay;
        private ICombatSessionOverlay subscribedSessionOverlay;
        private MonoBehaviour subscribedSessionOverlayBehaviour;
        private CombatEncounterController subscribedEncounter;
        private CombatHealth subscribedPlayerHealth;
        private CombatHealth subscribedBossHealth;
        private CombatHudAimDragInput aimDragInput;
        private CombatHudPointerActionInput[] pointerActionInputs = Array.Empty<CombatHudPointerActionInput>();
        private bool combatMenuInputLocked;
        private float lastPresentedPlayerHealth = float.NaN;
        private float lastPresentedPlayerMaxHealth = float.NaN;
        private float lastPresentedBossHealth = float.NaN;
        private float lastPresentedBossMaxHealth = float.NaN;

        public bool IsCombatMenuInputLocked => combatMenuInputLocked;

        public void Configure(
            CombatHudPresenter hud,
            CombatHudInputBridge bridge,
            CombatHudVirtualJoystick joystick,
            MonoBehaviour sessionSurface,
            CombatEncounterController encounter,
            CombatHealth player,
            CombatHealth boss,
            PlayerMovementController movement,
            PlayerActionController action,
            PlayerCombatModeController mode = null,
            PlayerRangedBasicAttackAction ranged = null,
            PlayerSkill1Action skill = null,
            PlayerSummonSlot1Action summon1 = null,
            PlayerSupportSummonSlotAction summon2 = null,
            PlayerSupportSummonSlotAction summon3 = null)
        {
            bool rebindLive = Application.isPlaying && isActiveAndEnabled;
            if (rebindLive)
            {
                UnsubscribeInputBridge();
                UnbindCombatSessionOverlay();
                UnbindEncounter();
                UnsubscribeHealthReadouts();
                SetCombatMenuInputLocked(false);
            }

            hudPresenter = hud;
            inputBridge = bridge;
            moveJoystick = joystick;
            sessionOverlayBehaviour = sessionSurface;
            encounterController = encounter;
            playerHealth = player;
            bossHealth = boss;
            movementController = movement;
            actionController = action;
            combatModeController = mode;
            rangedBasicAttackAction = ranged;
            skill1Action = skill;
            summonSlot1Action = summon1;
            summonSlot2Action = summon2;
            summonSlot3Action = summon3;

            ResolveCombatSessionOverlay();
            ResolveCombatMenuInputs();
            ResetPresentationCache();

            if (!rebindLive)
            {
                return;
            }

            SubscribeInputBridge();
            BindCombatSessionOverlay();
            BindEncounter();
            SubscribeHealthReadouts();
            RefreshHudNow();
            SyncHudRefreshRoutine();
        }

        private void Awake()
        {
            ResolveLocalUiReferences();
            ResolveCombatSessionOverlay();
            ResolveCombatMenuInputs();
        }

        private void OnEnable()
        {
            ResetPresentationCache();
            SubscribeInputBridge();
            BindCombatSessionOverlay();
            BindEncounter();
            SubscribeHealthReadouts();
            RefreshHudNow();
            SyncHudRefreshRoutine();
        }

        private void OnDisable()
        {
            StopHudRefreshRoutine();
            UnsubscribeInputBridge();
            UnbindCombatSessionOverlay();
            UnbindEncounter();
            UnsubscribeHealthReadouts();
            SetCombatMenuInputLocked(false);
            ReleaseBasicAttackHold();
        }

        public void RefreshHudNow()
        {
            if (hudPresenter == null)
            {
                return;
            }

            SubscribeHealthReadouts();
            hudPresenter.SetObjective(ResolveObjectiveText());
            UpdateHealthReadouts();
            UpdateActionReadouts();
            UpdateSummonReadouts();
        }

        private void ResolveLocalUiReferences()
        {
            hudPresenter ??= GetComponentInChildren<CombatHudPresenter>(includeInactive: true);
            inputBridge ??= GetComponentInChildren<CombatHudInputBridge>(includeInactive: true);
            moveJoystick ??= GetComponentInChildren<CombatHudVirtualJoystick>(includeInactive: true);
        }

        private void SubscribeInputBridge()
        {
            if (inputBridge == null)
            {
                return;
            }

            inputBridge.ActionRequested -= HandleActionRequested;
            inputBridge.ActionHoldChanged -= HandleActionHoldChanged;
            inputBridge.ActionRequested += HandleActionRequested;
            inputBridge.ActionHoldChanged += HandleActionHoldChanged;
        }

        private void UnsubscribeInputBridge()
        {
            if (inputBridge == null)
            {
                return;
            }

            inputBridge.ActionRequested -= HandleActionRequested;
            inputBridge.ActionHoldChanged -= HandleActionHoldChanged;
        }

        private void HandleActionRequested(CombatHudActionId actionId)
        {
            if (combatMenuInputLocked && actionId != CombatHudActionId.Pause)
            {
                return;
            }

            switch (actionId)
            {
                case CombatHudActionId.BasicAttack:
                    QueueBasicAttack();
                    break;
                case CombatHudActionId.Dodge:
                    actionController?.QueueDodge();
                    break;
                case CombatHudActionId.Skill1:
                    skill1Action?.QueueSkill1();
                    break;
                case CombatHudActionId.Ultimate:
                    combatModeController?.QueueCombatModeSwap();
                    break;
                case CombatHudActionId.SummonSlot1:
                    summonSlot1Action?.QueueSummonSlot1();
                    break;
                case CombatHudActionId.SummonSlot2:
                    summonSlot2Action?.QueueSummon();
                    break;
                case CombatHudActionId.SummonSlot3:
                    summonSlot3Action?.QueueSummon();
                    break;
                case CombatHudActionId.Pause:
                    ResolveCombatSessionOverlay();
                    if (sessionOverlayBehaviour != null)
                    {
                        sessionOverlay?.ShowPause();
                    }
                    break;
            }

            RefreshHudNow();
        }

        private void HandleActionHoldChanged(CombatHudActionId actionId, bool held)
        {
            if (actionId != CombatHudActionId.BasicAttack)
            {
                return;
            }

            SetBasicAttackHeld(!combatMenuInputLocked && held);
        }

        private void QueueBasicAttack()
        {
            if (ShouldUseRangedBasicAttack())
            {
                rangedBasicAttackAction.QueueFire();
                return;
            }

            actionController?.QueueBasicAttack();
        }

        private void SetBasicAttackHeld(bool held)
        {
            if (ShouldUseRangedBasicAttack())
            {
                actionController?.SetBasicAttackHeld(false);
                rangedBasicAttackAction.SetFireHeld(held);
                return;
            }

            actionController?.SetBasicAttackHeld(held);
            if (!held)
            {
                rangedBasicAttackAction?.SetFireHeld(false);
            }
        }

        private bool ShouldUseRangedBasicAttack()
        {
            return rangedBasicAttackAction != null
                && (combatModeController == null || combatModeController.IsRangedMode);
        }

        private void ReleaseBasicAttackHold()
        {
            if (actionController != null)
            {
                actionController.SetBasicAttackHeld(false);
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetFireHeld(false);
            }
        }

        private void ResolveCombatSessionOverlay()
        {
            sessionOverlay = sessionOverlayBehaviour != null
                ? sessionOverlayBehaviour as ICombatSessionOverlay
                : null;
        }

        private void BindCombatSessionOverlay()
        {
            ResolveCombatSessionOverlay();
            if (subscribedSessionOverlay == sessionOverlay
                && subscribedSessionOverlayBehaviour == sessionOverlayBehaviour)
            {
                SetCombatMenuInputLocked(
                    sessionOverlayBehaviour != null
                    && sessionOverlay != null
                    && sessionOverlay.IsVisible);
                return;
            }

            UnbindCombatSessionOverlay();
            subscribedSessionOverlay = sessionOverlay;
            subscribedSessionOverlayBehaviour = sessionOverlayBehaviour;
            if (subscribedSessionOverlayBehaviour != null
                && subscribedSessionOverlay != null)
            {
                subscribedSessionOverlay.CombatInputBlockChanged += HandleCombatInputBlockChanged;
            }

            SetCombatMenuInputLocked(
                sessionOverlayBehaviour != null
                && subscribedSessionOverlay != null
                && subscribedSessionOverlay.IsVisible);
        }

        private void UnbindCombatSessionOverlay()
        {
            ICombatSessionOverlay overlay = subscribedSessionOverlay;
            MonoBehaviour overlayBehaviour = subscribedSessionOverlayBehaviour;
            subscribedSessionOverlay = null;
            subscribedSessionOverlayBehaviour = null;
            if (overlayBehaviour == null || overlay == null)
            {
                return;
            }

            overlay.CombatInputBlockChanged -= HandleCombatInputBlockChanged;
        }

        private void HandleCombatInputBlockChanged(bool blocked)
        {
            SetCombatMenuInputLocked(blocked);
        }

        private void BindEncounter()
        {
            if (subscribedEncounter == encounterController)
            {
                return;
            }

            UnbindEncounter();
            subscribedEncounter = encounterController;
            if (subscribedEncounter == null)
            {
                return;
            }

            subscribedEncounter.Won += HandleEncounterWon;
            subscribedEncounter.Failed += HandleEncounterFailed;
            if (subscribedEncounter.IsFailed)
            {
                HandleEncounterFailed();
            }
        }

        private void UnbindEncounter()
        {
            CombatEncounterController encounter = subscribedEncounter;
            subscribedEncounter = null;
            if (encounter == null)
            {
                return;
            }

            encounter.Won -= HandleEncounterWon;
            encounter.Failed -= HandleEncounterFailed;
        }

        private void HandleEncounterWon()
        {
            RefreshHudNow();
        }

        private void HandleEncounterFailed()
        {
            RefreshHudNow();
            ResolveCombatSessionOverlay();
            if (sessionOverlayBehaviour != null)
            {
                sessionOverlay?.ShowFailure();
            }
        }

        private void ResolveCombatMenuInputs()
        {
            aimDragInput = GetComponentInChildren<CombatHudAimDragInput>(includeInactive: true);
            pointerActionInputs = GetComponentsInChildren<CombatHudPointerActionInput>(includeInactive: true);
        }

        private void SetCombatMenuInputLocked(bool locked)
        {
            if (combatMenuInputLocked == locked)
            {
                return;
            }

            combatMenuInputLocked = locked;
            ResolveCombatMenuInputs();

            if (moveJoystick != null)
            {
                moveJoystick.SetInputBlocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (aimDragInput != null)
            {
                aimDragInput.SetInputBlocked(PlayerInputLockSource.CombatMenu, locked);
            }

            for (int i = 0; i < pointerActionInputs.Length; i++)
            {
                CombatHudPointerActionInput pointerInput = pointerActionInputs[i];
                if (pointerInput != null)
                {
                    pointerInput.SetInputBlocked(PlayerInputLockSource.CombatMenu, locked);
                }
            }

            if (movementController != null)
            {
                movementController.SetSharedMoveInputBlocked(PlayerInputLockSource.CombatMenu, locked);
                movementController.SetSharedLookActionBlocked(PlayerInputLockSource.CombatMenu, locked);
                movementController.SetSharedFacingRequestsBlocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (actionController != null)
            {
                actionController.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (combatModeController != null)
            {
                combatModeController.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (skill1Action != null)
            {
                skill1Action.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (summonSlot2Action != null)
            {
                summonSlot2Action.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (summonSlot3Action != null)
            {
                summonSlot3Action.SetCinematicInputLocked(PlayerInputLockSource.CombatMenu, locked);
            }

            if (locked)
            {
                ReleaseBasicAttackHold();
            }
        }

        private void SubscribeHealthReadouts()
        {
            if (subscribedPlayerHealth != playerHealth)
            {
                if (subscribedPlayerHealth != null)
                {
                    subscribedPlayerHealth.Damaged -= HandlePlayerDamaged;
                    subscribedPlayerHealth.DamageBlockedByInvulnerability -= HandlePlayerDamageBlocked;
                }

                subscribedPlayerHealth = playerHealth;
                if (subscribedPlayerHealth != null)
                {
                    subscribedPlayerHealth.Damaged += HandlePlayerDamaged;
                    subscribedPlayerHealth.DamageBlockedByInvulnerability += HandlePlayerDamageBlocked;
                }
            }

            if (subscribedBossHealth == bossHealth)
            {
                return;
            }

            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Damaged -= HandleBossDamaged;
            }

            subscribedBossHealth = bossHealth;
            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Damaged += HandleBossDamaged;
            }
        }

        private void UnsubscribeHealthReadouts()
        {
            if (subscribedPlayerHealth != null)
            {
                subscribedPlayerHealth.Damaged -= HandlePlayerDamaged;
                subscribedPlayerHealth.DamageBlockedByInvulnerability -= HandlePlayerDamageBlocked;
            }

            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Damaged -= HandleBossDamaged;
            }

            subscribedPlayerHealth = null;
            subscribedBossHealth = null;
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            UpdateHealthReadouts();
            if (playerHealth == null || !CombatTeamUtility.AreAllied(damageInfo.SourceTeam, playerHealth.Team))
            {
                hudPresenter?.ShowPlayerDamageOverlay();
            }
        }

        private void HandlePlayerDamageBlocked(DamageInfo damageInfo)
        {
            if (playerHealth == null || !CombatTeamUtility.AreAllied(damageInfo.SourceTeam, playerHealth.Team))
            {
                hudPresenter?.ShowPlayerDamageOverlay();
            }
        }

        private void HandleBossDamaged(DamageInfo _)
        {
            UpdateHealthReadouts();
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

            bool showBoss = bossHealth != null;
            hudPresenter.SetBossHudVisible(showBoss);
            if (showBoss
                && (HealthValueChanged(lastPresentedBossHealth, bossHealth.CurrentHealth)
                    || HealthValueChanged(lastPresentedBossMaxHealth, bossHealth.MaxHealth)))
            {
                lastPresentedBossHealth = bossHealth.CurrentHealth;
                lastPresentedBossMaxHealth = bossHealth.MaxHealth;
                hudPresenter.SetBossHealth(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            }
        }

        private void UpdateActionReadouts()
        {
            bool rangedMode = ShouldUseRangedBasicAttack();
            hudPresenter.SetAimReticleVisible(
                rangedBasicAttackAction != null && rangedMode,
                rangedBasicAttackAction != null && rangedBasicAttackAction.IsAimPreviewActive);
            hudPresenter.SetInputMode(
                combatModeController == null
                    ? string.Empty
                    : combatModeController.IsMeleeMode ? "MELEE" : "RANGED");
            hudPresenter.SetAmmo(ResolveAmmoReadout(), rangedBasicAttackAction != null && rangedBasicAttackAction.IsReloading);
            hudPresenter.SetSkillCooldown(
                CombatHudActionId.BasicAttack,
                0f,
                rangedMode ? "FIRE" : "ATTACK");

            float dodgeRemaining = actionController != null ? actionController.DodgeCooldownRemaining : 0f;
            float dodgeDuration = actionController != null ? actionController.DodgeCooldownSeconds : 0f;
            float dodgeFill = dodgeDuration > 0f ? Mathf.Clamp01(dodgeRemaining / dodgeDuration) : 0f;
            hudPresenter.SetSkillCooldown(
                CombatHudActionId.Dodge,
                dodgeFill,
                "DODGE",
                actionController != null ? dodgeRemaining : -1f);
            hudPresenter.SetSkillCooldown(
                CombatHudActionId.Skill1,
                skill1Action != null ? 0f : 1f,
                skill1Action != null ? "SKILL" : "LOCKED");
            hudPresenter.SetSkillCooldown(
                CombatHudActionId.Ultimate,
                combatModeController != null ? 0f : 1f,
                combatModeController != null ? "SWAP" : "LOCKED");
        }

        private void UpdateSummonReadouts()
        {
            UpdatePrimarySummonReadout();
            UpdateSupportSummonReadout(CombatHudActionId.SummonSlot2, "S2", summonSlot2Action);
            UpdateSupportSummonReadout(CombatHudActionId.SummonSlot3, "S3", summonSlot3Action);
        }

        private void UpdatePrimarySummonReadout()
        {
            bool visible = summonSlot1Action != null;
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot1, visible);
            if (!visible)
            {
                return;
            }

            bool ready = !summonSlot1Action.IsSlotOnCooldown;
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot1,
                "S1",
                ResolveSummonState(ready, summonSlot1Action.SlotCooldownRemaining),
                ready,
                ResolveAvailabilityFill(
                    summonSlot1Action.SlotCooldownRemaining,
                    summonSlot1Action.SlotCooldownSeconds));
        }

        private void UpdateSupportSummonReadout(
            CombatHudActionId actionId,
            string label,
            PlayerSupportSummonSlotAction action)
        {
            bool visible = action != null;
            hudPresenter.SetSummonSlotVisible(actionId, visible);
            if (!visible)
            {
                return;
            }

            bool ready = !action.IsSlotOnCooldown;
            hudPresenter.SetSummonSlotState(
                actionId,
                label,
                ResolveSummonState(ready, action.SlotCooldownRemaining),
                ready,
                ResolveAvailabilityFill(action.SlotCooldownRemaining, action.SlotCooldownSeconds));
        }

        private string ResolveObjectiveText()
        {
            if (encounterController != null)
            {
                if (encounterController.IsWon)
                {
                    return "Objective complete.";
                }

                if (encounterController.IsFailed)
                {
                    return "Combat failed.";
                }
            }

            return string.IsNullOrWhiteSpace(objectiveText)
                ? DefaultObjectiveText
                : objectiveText.Trim();
        }

        private string ResolveAmmoReadout()
        {
            if (!ShouldUseRangedBasicAttack()
                || rangedBasicAttackAction == null
                || !rangedBasicAttackAction.UsesMagazineReload)
            {
                return string.Empty;
            }

            if (rangedBasicAttackAction.IsReloading)
            {
                return $"{rangedBasicAttackAction.CurrentAmmo}/{rangedBasicAttackAction.MagazineSize} "
                    + $"RLD {rangedBasicAttackAction.ReloadRemaining:0.0}";
            }

            return $"{rangedBasicAttackAction.CurrentAmmo}/{rangedBasicAttackAction.MagazineSize}";
        }

        private static string ResolveSummonState(bool ready, float cooldownRemaining)
        {
            return ready ? "AVAILABLE" : $"CD {Mathf.Max(0f, cooldownRemaining):0.0}";
        }

        private static float ResolveAvailabilityFill(float cooldownRemaining, float cooldownDuration)
        {
            if (cooldownDuration <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - Mathf.Max(0f, cooldownRemaining) / cooldownDuration);
        }

        private static bool HealthValueChanged(float previous, float current)
        {
            return float.IsNaN(previous) || !Mathf.Approximately(previous, current);
        }

        private void ResetPresentationCache()
        {
            lastPresentedPlayerHealth = float.NaN;
            lastPresentedPlayerMaxHealth = float.NaN;
            lastPresentedBossHealth = float.NaN;
            lastPresentedBossMaxHealth = float.NaN;
        }

        private void SyncHudRefreshRoutine()
        {
            if (hudPresenter == null)
            {
                StopHudRefreshRoutine();
                return;
            }

            StartHudRefreshRoutine();
        }

        private void StartHudRefreshRoutine()
        {
            if (hudRefreshRoutine != null || !isActiveAndEnabled)
            {
                return;
            }

            hudRefreshRoutine = StartCoroutine(RefreshHudAtConfiguredRate());
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

        private IEnumerator RefreshHudAtConfiguredRate()
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
    }
}
