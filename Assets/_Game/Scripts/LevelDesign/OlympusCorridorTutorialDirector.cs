using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusCorridorTutorialDirector : MonoBehaviour
    {
        private enum TutorialStep
        {
            Inactive,
            Melee,
            Move,
            SwapToRanged,
            Fire,
            Dodge,
            ClearTargets,
            Completed
        }

        private enum TutorialStepPhase
        {
            Inactive,
            Cue,
            AwaitingAction,
            Committed
        }

        [Header("Flow")]
        [SerializeField] private bool tutorialEnabled = true;
        [SerializeField, Min(0f)] private float cuePrimeSeconds = 0.45f;
        [SerializeField, Min(0f)] private float completionRecordSeconds = 0.55f;
        [SerializeField, Min(0.1f)] private float promptRepeatSeconds = 4.0f;

        [Header("Movement Step")]
        [SerializeField, Min(0f)] private float movementCompleteDistance = 1.25f;

        [Header("Melee Step")]
        [SerializeField] private bool positionFirstTargetForMeleeStep = true;
        [SerializeField, Min(0.25f)] private float meleeTargetDistance = 1.45f;
        [SerializeField] private float meleeTargetSideOffset;

        [Header("Dodge Step")]
        [SerializeField] private bool enableEnemyGameplayDuringDodgeStep = true;

        [Header("Fire Step")]
        [SerializeField, Min(0f)] private float fireAimPreviewLeadSeconds = 0.7f;
        [SerializeField] private bool positionFirstTargetForRangedStep = true;
        [SerializeField, Min(1f)] private float rangedTargetDistance = 7f;
        [SerializeField, Min(0f)] private float rangedTargetAimHeight = 1.05f;

        [Header("Clear Step")]
        [SerializeField] private bool requireTutorialTargetsDefeated = true;
        [SerializeField] private bool enableEnemyGameplayDuringClearStep = true;

        [Header("Route Guard")]
        [SerializeField] private bool constrainPlayerDuringTutorial = true;
        [SerializeField] private Transform tutorialBoundsCenter;
        [SerializeField] private Vector3 tutorialBoundsHalfExtents = new Vector3(6f, 4f, 7f);
        [SerializeField] private Collider[] tutorialRouteBlockers = Array.Empty<Collider>();
        [SerializeField] private GameObject[] tutorialBoundsRoots = Array.Empty<GameObject>();

        [Header("References")]
        [SerializeField] private CinematicTutorialPromptPresenter promptPresenter;
        [SerializeField] private OlympusTutorialOverlayPresenter overlayPresenter;
        [SerializeField] private BossBarrageLaneReviewMobileHud mobileHud;
        [SerializeField] private PlayerMovementController player;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction[] supportSummonActions =
            Array.Empty<PlayerSupportSummonSlotAction>();
        [SerializeField] private CombatHealth[] tutorialTargets = Array.Empty<CombatHealth>();
        [SerializeField] private Behaviour[] tutorialEnemyGameplayBehaviours =
            Array.Empty<Behaviour>();

        [Header("Debug")]
        [SerializeField] private TutorialStep step = TutorialStep.Inactive;
        [SerializeField] private TutorialStepPhase stepPhase = TutorialStepPhase.Inactive;

        private bool completedRaised;
        private bool meleeHitObserved;
        private bool movementObserved;
        private bool rangedModeObserved;
        private bool rangedProjectileFiredObserved;
        private bool rangedTargetDamageObserved;
        private bool stepTargetDeathObserved;
        private bool dodgeObserved;
        private bool hasRuntimeBoundsCenter;
        private bool hasCachedActionEnabledStates;
        private bool cachedRangedBasicEnabled;
        private bool cachedSkill1Enabled;
        private bool cachedSummonSlot1Enabled;
        private bool[] cachedSupportEnabled = Array.Empty<bool>();
        private float stepTimer;
        private float phaseTimer;
        private float nextPromptTime;
        private string lastCompletionRecord = string.Empty;
        private Vector3 movementStartPosition;
        private Vector3 runtimeBoundsCenter;

        public event Action Completed;

        public bool TutorialEnabled => tutorialEnabled;
        public bool IsRunning => step != TutorialStep.Inactive && step != TutorialStep.Completed;
        public bool IsCompleted => step == TutorialStep.Completed;
        public string CurrentStepId => step.ToString();
        public string CurrentPhaseId => stepPhase.ToString();
        public string LastCompletionRecord => lastCompletionRecord;

        public void BindRuntimeContext(
            PlayerMovementController newPlayer,
            PlayerCombatModeController newCombatModeController,
            PlayerCombatTargetSelector newTargetSelector,
            PlayerActionController newActionController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction[] newSupportSummonActions,
            BossBarrageLaneReviewMobileHud newMobileHud,
            CinematicTutorialPromptPresenter newPromptPresenter,
            CombatHealth[] newTutorialTargets,
            Behaviour[] newTutorialEnemyGameplayBehaviours,
            Collider[] newTutorialRouteBlockers)
        {
            player = newPlayer != null ? newPlayer : player;
            combatModeController = newCombatModeController != null ? newCombatModeController : combatModeController;
            targetSelector = newTargetSelector != null ? newTargetSelector : targetSelector;
            actionController = newActionController != null ? newActionController : actionController;
            rangedBasicAttackAction = newRangedBasicAttackAction != null
                ? newRangedBasicAttackAction
                : rangedBasicAttackAction;
            skill1Action = newSkill1Action != null ? newSkill1Action : skill1Action;
            summonSlot1Action = newSummonSlot1Action != null ? newSummonSlot1Action : summonSlot1Action;
            supportSummonActions =
                newSupportSummonActions ?? supportSummonActions ?? Array.Empty<PlayerSupportSummonSlotAction>();
            mobileHud = newMobileHud != null ? newMobileHud : mobileHud;
            promptPresenter = newPromptPresenter != null ? newPromptPresenter : promptPresenter;
            tutorialTargets = newTutorialTargets ?? tutorialTargets ?? Array.Empty<CombatHealth>();
            tutorialEnemyGameplayBehaviours =
                newTutorialEnemyGameplayBehaviours
                ?? tutorialEnemyGameplayBehaviours
                ?? Array.Empty<Behaviour>();
            tutorialRouteBlockers = newTutorialRouteBlockers ?? tutorialRouteBlockers ?? Array.Empty<Collider>();
        }

        public void BeginTutorial()
        {
            ResolveMissingReferences();
            completedRaised = false;
            hasCachedActionEnabledStates = false;

            if (!tutorialEnabled)
            {
                step = TutorialStep.Completed;
                stepPhase = TutorialStepPhase.Inactive;
                RaiseCompletedOnce();
                return;
            }

            CacheActionEnabledStates();
            meleeHitObserved = false;
            movementObserved = false;
            rangedModeObserved = false;
            rangedProjectileFiredObserved = false;
            rangedTargetDamageObserved = false;
            stepTargetDeathObserved = false;
            dodgeObserved = false;
            lastCompletionRecord = string.Empty;
            movementStartPosition = player != null ? player.transform.position : transform.position;
            runtimeBoundsCenter = tutorialBoundsCenter != null ? tutorialBoundsCenter.position : movementStartPosition;
            hasRuntimeBoundsCenter = true;

            SetObjectsActive(tutorialBoundsRoots, true);
            SetCollidersEnabled(tutorialRouteBlockers, true);
            SetCombatHealthRootsActive(tutorialTargets, true);
            ResetCombatHealthsToFull(tutorialTargets);
            SetCombatHealthRootCollidersEnabled(tutorialTargets, true);
            PositionFirstTargetForMeleeStep();
            SetEnemyGameplayEnabled(false);
            SubscribeObservers();

            if (combatModeController != null)
            {
                combatModeController.enabled = true;
                combatModeController.SetMeleeMode();
            }

            StartStep(TutorialStep.Melee);
        }

        public void CancelTutorial()
        {
            if (step == TutorialStep.Inactive)
            {
                return;
            }

            UnsubscribeObservers();
            SetTutorialAimPreviewHeld(false);
            SetPlayerActionInputLocked(false);
            SetCombatModeInputLocked(false);
            SetRangedBasicAttackInputLocked(false);
            promptPresenter?.HidePrompt();
            overlayPresenter?.Hide();
            SetEnemyGameplayEnabled(false);
            RestoreActionEnabledStates();
            step = TutorialStep.Inactive;
            stepPhase = TutorialStepPhase.Inactive;
        }

        public void HideGuide()
        {
            promptPresenter?.HidePrompt();
            overlayPresenter?.Hide();
        }

        private void OnDisable()
        {
            CancelTutorial();
        }

        private void Update()
        {
            if (!Application.isPlaying || !IsRunning)
            {
                return;
            }

            stepTimer += Time.deltaTime;
            phaseTimer += Time.deltaTime;
            EnforcePlayerTutorialBounds();
            ApplyStepInputLocks();
            UpdateTutorialAimPreviewHold();
            RepeatPromptIfNeeded();

            switch (stepPhase)
            {
                case TutorialStepPhase.Cue:
                    if (phaseTimer >= cuePrimeSeconds)
                    {
                        ActivateStepInputWindow();
                    }
                    return;
                case TutorialStepPhase.AwaitingAction:
                    UpdateAwaitingActionStep();
                    return;
                case TutorialStepPhase.Committed:
                    if (phaseTimer >= completionRecordSeconds)
                    {
                        AdvanceAfterCommittedStep();
                    }
                    return;
            }
        }

        private void UpdateAwaitingActionStep()
        {
            switch (step)
            {
                case TutorialStep.Melee:
                    if (HasCompletedMeleeStep())
                    {
                        CommitStepCompletion("melee_hit");
                    }
                    break;
                case TutorialStep.Move:
                    if (HasCompletedMovementStep())
                    {
                        CommitStepCompletion("space_created");
                    }
                    break;
                case TutorialStep.SwapToRanged:
                    if (HasCompletedSwapStep())
                    {
                        CommitStepCompletion("ranged_mode");
                    }
                    break;
                case TutorialStep.Fire:
                    if (HasCompletedFireStep())
                    {
                        CommitStepCompletion("ranged_hit");
                    }
                    break;
                case TutorialStep.Dodge:
                    if (dodgeObserved)
                    {
                        CommitStepCompletion("dodge_window");
                    }
                    break;
                case TutorialStep.ClearTargets:
                    if (HasCompletedClearTargetsStep())
                    {
                        CommitStepCompletion("targets_clear");
                    }
                    break;
            }
        }

        private void StartStep(TutorialStep nextStep)
        {
            step = nextStep;
            stepPhase = TutorialStepPhase.Cue;
            stepTimer = 0f;
            phaseTimer = 0f;
            nextPromptTime = 0f;
            stepTargetDeathObserved = false;
            if (step != TutorialStep.Fire)
            {
                SetTutorialAimPreviewHeld(false);
            }

            switch (step)
            {
                case TutorialStep.Melee:
                    meleeHitObserved = false;
                    ConfigureTargetCandidates(tutorialTargets);
                    SetMeleeMode();
                    SetCombatModeInputLocked(true);
                    SetRangedFireEnabled(false);
                    SetOptionalActionsEnabled(false);
                    SetEnemyGameplayEnabled(false);
                    break;
                case TutorialStep.Move:
                    movementStartPosition = player != null ? player.transform.position : transform.position;
                    ConfigureTargetCandidates(Array.Empty<CombatHealth>());
                    SetMeleeMode();
                    SetCombatModeInputLocked(true);
                    SetRangedFireEnabled(false);
                    SetOptionalActionsEnabled(false);
                    SetEnemyGameplayEnabled(false);
                    break;
                case TutorialStep.SwapToRanged:
                    rangedModeObserved = false;
                    ConfigureTargetCandidates(tutorialTargets);
                    SetMeleeMode();
                    SetCombatModeInputLocked(false);
                    SetRangedFireEnabled(false);
                    SetOptionalActionsEnabled(false);
                    SetEnemyGameplayEnabled(false);
                    break;
                case TutorialStep.Fire:
                    rangedProjectileFiredObserved = false;
                    rangedTargetDamageObserved = false;
                    ConfigureTargetCandidates(tutorialTargets);
                    SetRangedMode();
                    SetCombatModeInputLocked(true);
                    SetRangedFireEnabled(true);
                    SetTutorialAimPreviewHeld(true);
                    PositionFirstTargetForRangedStep();
                    SetOptionalActionsEnabled(false);
                    SetEnemyGameplayEnabled(false);
                    break;
                case TutorialStep.Dodge:
                    ConfigureTargetCandidates(tutorialTargets);
                    SetRangedMode();
                    SetCombatModeInputLocked(true);
                    SetRangedFireEnabled(false);
                    SetOptionalActionsEnabled(false);
                    SetEnemyGameplayEnabled(enableEnemyGameplayDuringDodgeStep);
                    break;
                case TutorialStep.ClearTargets:
                    ConfigureTargetCandidates(tutorialTargets);
                    SetCombatModeInputLocked(false);
                    SetRangedFireEnabled(true);
                    SetOptionalActionsEnabled(true);
                    SetEnemyGameplayEnabled(enableEnemyGameplayDuringClearStep);
                    break;
            }

            ShowCurrentStepGuide();
            nextPromptTime = Time.unscaledTime + promptRepeatSeconds;
            SetOverlayGuideState(OlympusTutorialOverlayPresenter.GuideState.Focus);
            ApplyStepInputLocks();
        }

        private void ActivateStepInputWindow()
        {
            if (stepPhase != TutorialStepPhase.Cue)
            {
                return;
            }

            stepPhase = TutorialStepPhase.AwaitingAction;
            phaseTimer = 0f;
            ResetCurrentStepObservers();
            if (step == TutorialStep.Move)
            {
                movementStartPosition = player != null ? player.transform.position : transform.position;
            }

            SetOverlayGuideState(OlympusTutorialOverlayPresenter.GuideState.Ready);
            ApplyStepInputLocks();
        }

        private void CommitStepCompletion(string recordId)
        {
            if (stepPhase != TutorialStepPhase.AwaitingAction)
            {
                return;
            }

            lastCompletionRecord = $"{step}:{recordId}";
            stepPhase = TutorialStepPhase.Committed;
            phaseTimer = 0f;
            nextPromptTime = Time.unscaledTime + promptRepeatSeconds;
            ShowCompletionGuide();
            SetOverlayGuideState(OlympusTutorialOverlayPresenter.GuideState.Confirmed);
            ApplyStepInputLocks();
        }

        private void AdvanceAfterCommittedStep()
        {
            switch (step)
            {
                case TutorialStep.Melee:
                    StartStep(TutorialStep.Move);
                    break;
                case TutorialStep.Move:
                    StartStep(TutorialStep.SwapToRanged);
                    break;
                case TutorialStep.SwapToRanged:
                    StartStep(TutorialStep.Fire);
                    break;
                case TutorialStep.Fire:
                    StartStep(TutorialStep.Dodge);
                    break;
                case TutorialStep.Dodge:
                    StartStep(TutorialStep.ClearTargets);
                    break;
                case TutorialStep.ClearTargets:
                    CompleteTutorial();
                    break;
            }
        }

        private void CompleteTutorial()
        {
            if (step == TutorialStep.Completed)
            {
                return;
            }

            step = TutorialStep.Completed;
            stepPhase = TutorialStepPhase.Inactive;
            SetTutorialAimPreviewHeld(false);
            SetPlayerActionInputLocked(false);
            SetCombatModeInputLocked(false);
            SetRangedBasicAttackInputLocked(false);
            UnsubscribeObservers();
            SetEnemyGameplayEnabled(false);
            SetCombatHealthRootCollidersEnabled(tutorialTargets, false);
            SetCollidersEnabled(tutorialRouteBlockers, false);
            SetObjectsActive(tutorialBoundsRoots, false);
            ConfigureTargetCandidates(Array.Empty<CombatHealth>());
            RestoreActionEnabledStates();
            ShowGuide(
                "\uc774\ub178\ub9ac",
                "\uc88b\uc544. \uc544\ub798 \ud1b5\ub85c\ub85c \ub0b4\ub824\uac00. \uac70\uae30\uc11c\ubd80\ud130\ub294 \uc9c4\uc9dc \uc804\ud22c\uac00 \uc2dc\uc791\ub3fc.",
                "ROUTE",
                OlympusTutorialOverlayPresenter.FocusKind.Route,
                new Vector2(0.5f, 0.76f));
            SetOverlayGuideState(OlympusTutorialOverlayPresenter.GuideState.Ready);
            RaiseCompletedOnce();
        }

        private void RaiseCompletedOnce()
        {
            if (completedRaised)
            {
                return;
            }

            completedRaised = true;
            Completed?.Invoke();
        }

        private void ResetCurrentStepObservers()
        {
            switch (step)
            {
                case TutorialStep.Melee:
                    meleeHitObserved = false;
                    stepTargetDeathObserved = false;
                    break;
                case TutorialStep.Move:
                    movementObserved = false;
                    break;
                case TutorialStep.SwapToRanged:
                    rangedModeObserved = false;
                    break;
                case TutorialStep.Fire:
                    rangedProjectileFiredObserved = false;
                    rangedTargetDamageObserved = false;
                    stepTargetDeathObserved = false;
                    break;
                case TutorialStep.Dodge:
                    dodgeObserved = false;
                    break;
                case TutorialStep.ClearTargets:
                    stepTargetDeathObserved = false;
                    break;
            }
        }

        private bool HasCompletedMeleeStep()
        {
            return meleeHitObserved || stepTargetDeathObserved;
        }

        private bool HasCompletedMovementStep()
        {
            if (movementObserved)
            {
                return true;
            }

            if (player == null || movementCompleteDistance <= 0f)
            {
                return false;
            }

            Vector3 offset = Vector3.ProjectOnPlane(
                player.transform.position - movementStartPosition,
                Vector3.up);
            return offset.magnitude >= movementCompleteDistance;
        }

        private bool HasCompletedSwapStep()
        {
            return rangedModeObserved
                || (combatModeController != null && combatModeController.IsRangedMode);
        }

        private bool HasCompletedFireStep()
        {
            return phaseTimer >= fireAimPreviewLeadSeconds
                && rangedProjectileFiredObserved
                && (rangedTargetDamageObserved || stepTargetDeathObserved);
        }

        private bool HasCompletedClearTargetsStep()
        {
            return !requireTutorialTargetsDefeated
                || !HasAny(tutorialTargets)
                || CountActiveAlive(tutorialTargets) == 0;
        }

        private void RepeatPromptIfNeeded()
        {
            if (stepPhase == TutorialStepPhase.Committed)
            {
                return;
            }

            if (Time.unscaledTime < nextPromptTime)
            {
                return;
            }

            nextPromptTime = Time.unscaledTime + promptRepeatSeconds;
            ShowCurrentStepGuide();
        }

        private void ShowCurrentStepGuide()
        {
            switch (step)
            {
                case TutorialStep.Melee:
                    ShowGuide(
                        "\uc774\ub178\ub9ac",
                        "\uc190\uc774 \ub5a8\ub824\ub3c4 \uad1c\ucc2e\uc544. \uba3c\uc800 \uc55e\uc758 \uc801\uc744 \ubca0\uc5b4\ub0b4.",
                        "\uacf5\uaca9 \ubc84\ud2bc",
                        OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Move:
                    ShowGuide(
                        "\uc624\ud37c\ub808\uc774\ud130",
                        "\uba48\ucd94\uba74 \ud3ec\uc704\ub3fc. \uc67c\ucabd \ud328\ub4dc\ub85c \uc606\uc73c\ub85c \ube60\uc838 \uacf5\uac04\uc744 \ub9cc\ub4e4\uc5b4.",
                        "\uc774\ub3d9 \ud328\ub4dc",
                        OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                        new Vector2(0.16f, 0.16f));
                    break;
                case TutorialStep.SwapToRanged:
                    ShowGuide(
                        "\uc624\ud37c\ub808\uc774\ud130",
                        "\uac70\ub9ac\uac00 \ubc8c\uc5b4\uc84c\uc5b4. \uc6d0\uac70\ub9ac \ubaa8\ub4dc\ub85c \uc804\ud658\ud574.",
                        "\uc804\ud658 \ubc84\ud2bc",
                        OlympusTutorialOverlayPresenter.FocusKind.SwapMode,
                        new Vector2(0.82f, 0.24f));
                    break;
                case TutorialStep.Fire:
                    ShowGuide(
                        "\uc774\ub178\ub9ac",
                        "\uc870\uc900\uc120 \uc548\uc5d0 \ub123\uace0 \uc3f4. \ud0c4\ub3c4 \ubcf4\uc815\uc740 \uba85\uc911\uae4c\uc9c0 \ud655\uc778\ud574\uc57c \ud574.",
                        "\uc0ac\uaca9 \ubc84\ud2bc",
                        OlympusTutorialOverlayPresenter.FocusKind.RangedAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Dodge:
                    ShowGuide(
                        "\uc624\ud37c\ub808\uc774\ud130",
                        "\uacbd\uace0\uc120\uc774 \ubcf4\uc774\uba74 \ub9de\ubc1b\uc544\uce58\uc9c0 \ub9c8. \uc9c0\uae08\uc740 \ud53c\ud574\uc57c \ud574.",
                        "\ud68c\ud53c \ubc84\ud2bc",
                        OlympusTutorialOverlayPresenter.FocusKind.Dodge,
                        new Vector2(0.92f, 0.24f));
                    break;
                case TutorialStep.ClearTargets:
                    ShowGuide(
                        "\uc774\ub178\ub9ac",
                        "\uc88b\uc544. \uc774\uc81c \ub0a8\uc740 \uc801\uc744 \uc804\ubd80 \uc815\ub9ac\ud574. \uc14b \ubaa8\ub450 \uc4f0\ub7ec\uc838\uc57c \uae38\uc774 \uc5f4\ub824.",
                        "ALL CLEAR",
                        OlympusTutorialOverlayPresenter.FocusKind.Route,
                        new Vector2(0.5f, 0.76f));
                    break;
            }
        }

        private void ShowCompletionGuide()
        {
            switch (step)
            {
                case TutorialStep.Melee:
                    ShowGuide(
                        "\uc774\ub178\ub9ac",
                        "\uadfc\uc811 \uc555\ubc15 \ucc98\ub9ac \ud655\uc778.",
                        "RECORDED",
                        OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Move:
                    ShowGuide(
                        "\uc624\ud37c\ub808\uc774\ud130",
                        "\uacf5\uac04 \ud655\ubcf4 \ud655\uc778.",
                        "RECORDED",
                        OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                        new Vector2(0.16f, 0.16f));
                    break;
                case TutorialStep.SwapToRanged:
                    ShowGuide(
                        "\uc624\ud37c\ub808\uc774\ud130",
                        "\uc6d0\uac70\ub9ac \uc804\ud658 \ud655\uc778.",
                        "RECORDED",
                        OlympusTutorialOverlayPresenter.FocusKind.SwapMode,
                        new Vector2(0.82f, 0.24f));
                    break;
                case TutorialStep.Fire:
                    ShowGuide(
                        "\uc774\ub178\ub9ac",
                        "\uba85\uc911 \ud655\uc778.",
                        "RECORDED",
                        OlympusTutorialOverlayPresenter.FocusKind.RangedAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Dodge:
                    ShowGuide(
                        "\uc624\ud37c\ub808\uc774\ud130",
                        "\ud68c\ud53c \ud655\uc778.",
                        "RECORDED",
                        OlympusTutorialOverlayPresenter.FocusKind.Dodge,
                        new Vector2(0.92f, 0.24f));
                    break;
                case TutorialStep.ClearTargets:
                    ShowGuide(
                        "\uc774\ub178\ub9ac",
                        "\ud3ec\ucf13 \uc815\ub9ac \ud655\uc778.",
                        "RECORDED",
                        OlympusTutorialOverlayPresenter.FocusKind.Route,
                        new Vector2(0.5f, 0.76f));
                    break;
            }
        }

        private void ShowGuide(
            string speaker,
            string dialogue,
            string inputLabel,
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            Vector2 anchor)
        {
            Vector2 resolvedAnchor = ResolveHudAnchor(focusKind, anchor);
            if (overlayPresenter != null)
            {
                overlayPresenter.Show(speaker, dialogue, inputLabel, focusKind, resolvedAnchor);
                return;
            }

            if (promptPresenter == null)
            {
                return;
            }

            var cue = new CinematicSequenceProfile.TutorialCue(
                focusKind.ToString(),
                ResolveFallbackCueKind(focusKind),
                0f,
                promptRepeatSeconds,
                inputLabel,
                dialogue,
                true,
                resolvedAnchor);
            promptPresenter.ShowCue(cue);
        }

        private Vector2 ResolveHudAnchor(
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            Vector2 fallbackAnchor)
        {
            if (TryResolveCombatHudAnchor(focusKind, out Vector2 combatHudAnchor))
            {
                return combatHudAnchor;
            }

            if (mobileHud == null)
            {
                return fallbackAnchor;
            }

            switch (focusKind)
            {
                case OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack:
                case OlympusTutorialOverlayPresenter.FocusKind.RangedAttack:
                    return mobileHud.BasicButtonScreenAnchor;
                case OlympusTutorialOverlayPresenter.FocusKind.Dodge:
                    return mobileHud.DodgeButtonScreenAnchor;
                case OlympusTutorialOverlayPresenter.FocusKind.MoveStick:
                    return mobileHud.MoveJoystickScreenAnchor;
                case OlympusTutorialOverlayPresenter.FocusKind.SwapMode:
                    return mobileHud.SwapButtonScreenAnchor;
                default:
                    return fallbackAnchor;
            }
        }

        private bool TryResolveCombatHudAnchor(
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            out Vector2 anchor)
        {
            if (TryResolveCombatHudGuiRect(focusKind, out Rect rect))
            {
                anchor = ToScreenAnchor(rect.center);
                return true;
            }

            anchor = default;
            return false;
        }

        private static bool TryResolveCombatHudGuiRect(
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            out Rect rect)
        {
            string objectName = ResolveCombatHudObjectName(focusKind);
            if (string.IsNullOrWhiteSpace(objectName))
            {
                rect = default;
                return false;
            }

            RectTransform rectTransform = FindSceneRectTransform(objectName);
            return TryGetGuiRect(rectTransform, out rect);
        }

        private static string ResolveCombatHudObjectName(OlympusTutorialOverlayPresenter.FocusKind focusKind)
        {
            switch (focusKind)
            {
                case OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack:
                case OlympusTutorialOverlayPresenter.FocusKind.RangedAttack:
                    return "BasicAttackButton";
                case OlympusTutorialOverlayPresenter.FocusKind.Dodge:
                    return "DodgeButton";
                case OlympusTutorialOverlayPresenter.FocusKind.MoveStick:
                    return "MoveJoystickRing";
                case OlympusTutorialOverlayPresenter.FocusKind.SwapMode:
                    return "UltimateButton";
                default:
                    return null;
            }
        }

        private static RectTransform FindSceneRectTransform(string objectName)
        {
            RectTransform[] rectTransforms = UnityEngine.Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform rectTransform = rectTransforms[i];
                if (rectTransform == null
                    || !rectTransform.gameObject.scene.IsValid()
                    || !string.Equals(rectTransform.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                return rectTransform;
            }

            return null;
        }

        private static bool TryGetGuiRect(RectTransform rectTransform, out Rect rect)
        {
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
            {
                rect = default;
                return false;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
                Vector2 guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
                min = Vector2.Min(min, guiPoint);
                max = Vector2.Max(max, guiPoint);
            }

            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return rect.width > 0.01f && rect.height > 0.01f;
        }

        private static Vector2 ToScreenAnchor(Vector2 guiPoint)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector2.zero;
            }

            return new Vector2(
                Mathf.Clamp01(guiPoint.x / Screen.width),
                Mathf.Clamp01(1f - guiPoint.y / Screen.height));
        }

        private static CinematicSequenceProfile.TutorialCueKind ResolveFallbackCueKind(
            OlympusTutorialOverlayPresenter.FocusKind focusKind)
        {
            switch (focusKind)
            {
                case OlympusTutorialOverlayPresenter.FocusKind.Dodge:
                    return CinematicSequenceProfile.TutorialCueKind.WarningPrompt;
                case OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack:
                case OlympusTutorialOverlayPresenter.FocusKind.RangedAttack:
                    return CinematicSequenceProfile.TutorialCueKind.ClickPrompt;
                default:
                    return CinematicSequenceProfile.TutorialCueKind.TimedGuide;
            }
        }

        private void EnforcePlayerTutorialBounds()
        {
            if (!constrainPlayerDuringTutorial || player == null || !hasRuntimeBoundsCenter)
            {
                return;
            }

            Vector3 halfExtents = new Vector3(
                Mathf.Max(0.1f, tutorialBoundsHalfExtents.x),
                Mathf.Max(0.1f, tutorialBoundsHalfExtents.y),
                Mathf.Max(0.1f, tutorialBoundsHalfExtents.z));
            Vector3 position = player.transform.position;
            Vector3 min = runtimeBoundsCenter - halfExtents;
            Vector3 max = runtimeBoundsCenter + halfExtents;
            Vector3 clamped = position;
            clamped.x = Mathf.Clamp(clamped.x, min.x, max.x);
            clamped.z = Mathf.Clamp(clamped.z, min.z, max.z);
            if (clamped.y < min.y)
            {
                clamped.y = min.y;
            }

            if ((clamped - position).sqrMagnitude <= 0.0001f)
            {
                return;
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            bool wasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            player.transform.position = clamped;
            if (characterController != null)
            {
                characterController.enabled = wasEnabled;
            }
        }

        private void SubscribeObservers()
        {
            UnsubscribeObservers();
            if (player != null)
            {
                player.RunStarted += HandleRunStarted;
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.RangedFireStarted += HandleRangedFireStarted;
                rangedBasicAttackAction.RangedProjectileFired += HandleRangedProjectileFired;
            }

            if (actionController != null)
            {
                actionController.BasicAttackHit += HandleBasicAttackHit;
                actionController.DodgeStarted += HandleDodgeStarted;
            }

            if (combatModeController != null)
            {
                combatModeController.CombatModeChanged += HandleCombatModeChanged;
            }

            SubscribeTargetDamageHandlers(true);
        }

        private void UnsubscribeObservers()
        {
            if (player != null)
            {
                player.RunStarted -= HandleRunStarted;
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.RangedFireStarted -= HandleRangedFireStarted;
                rangedBasicAttackAction.RangedProjectileFired -= HandleRangedProjectileFired;
            }

            if (actionController != null)
            {
                actionController.BasicAttackHit -= HandleBasicAttackHit;
                actionController.DodgeStarted -= HandleDodgeStarted;
            }

            if (combatModeController != null)
            {
                combatModeController.CombatModeChanged -= HandleCombatModeChanged;
            }

            SubscribeTargetDamageHandlers(false);
        }

        private void SubscribeTargetDamageHandlers(bool subscribe)
        {
            if (tutorialTargets == null)
            {
                return;
            }

            for (int i = 0; i < tutorialTargets.Length; i++)
            {
                CombatHealth health = tutorialTargets[i];
                if (health == null)
                {
                    continue;
                }

                health.Damaged -= HandleTargetDamaged;
                health.Died -= HandleTargetDied;
                if (subscribe)
                {
                    health.Damaged += HandleTargetDamaged;
                    health.Died += HandleTargetDied;
                }
            }
        }

        private void HandleRunStarted()
        {
            if (CanRecordStepAction(TutorialStep.Move))
            {
                movementObserved = true;
            }
        }

        private void HandleBasicAttackHit(int _)
        {
            if (CanRecordStepAction(TutorialStep.Melee))
            {
                meleeHitObserved = true;
            }
        }

        private void HandleCombatModeChanged(PlayerCombatMode combatMode)
        {
            if (CanRecordStepAction(TutorialStep.SwapToRanged) && combatMode == PlayerCombatMode.Ranged)
            {
                rangedModeObserved = true;
            }
        }

        private void HandleRangedFireStarted()
        {
            if (CanRecordStepAction(TutorialStep.Fire))
            {
                rangedProjectileFiredObserved = true;
            }
        }

        private void HandleRangedProjectileFired(LaneActionProjectile _)
        {
            if (CanRecordStepAction(TutorialStep.Fire))
            {
                rangedProjectileFiredObserved = true;
            }
        }

        private void HandleTargetDamaged(DamageInfo damageInfo)
        {
            if (!CombatTeamUtility.IsPlayerSide(damageInfo.SourceTeam))
            {
                return;
            }

            if (CanRecordStepAction(TutorialStep.Melee))
            {
                meleeHitObserved = true;
            }
            else if (CanRecordStepAction(TutorialStep.Fire))
            {
                rangedTargetDamageObserved = true;
            }
        }

        private void HandleTargetDied()
        {
            if (stepPhase == TutorialStepPhase.AwaitingAction)
            {
                stepTargetDeathObserved = true;
            }
        }

        private void HandleDodgeStarted()
        {
            if (CanRecordStepAction(TutorialStep.Dodge))
            {
                dodgeObserved = true;
            }
        }

        private bool CanRecordStepAction(TutorialStep expectedStep)
        {
            return step == expectedStep && stepPhase == TutorialStepPhase.AwaitingAction;
        }

        private void ResolveMissingReferences()
        {
            if (player == null)
            {
                player = GetComponentInParent<PlayerMovementController>()
                    ?? FindFirst<PlayerMovementController>();
            }

            if (player != null)
            {
                combatModeController = combatModeController != null
                    ? combatModeController
                    : player.GetComponent<PlayerCombatModeController>();
                targetSelector = targetSelector != null
                    ? targetSelector
                    : player.GetComponent<PlayerCombatTargetSelector>();
                actionController = actionController != null
                    ? actionController
                    : player.GetComponent<PlayerActionController>();
                rangedBasicAttackAction = rangedBasicAttackAction != null
                    ? rangedBasicAttackAction
                    : player.GetComponent<PlayerRangedBasicAttackAction>();
                skill1Action = skill1Action != null
                    ? skill1Action
                    : player.GetComponent<PlayerSkill1Action>();
                summonSlot1Action = summonSlot1Action != null
                    ? summonSlot1Action
                    : player.GetComponent<PlayerSummonSlot1Action>();
            }

            if (promptPresenter == null)
            {
                promptPresenter = FindFirst<CinematicTutorialPromptPresenter>();
            }

            if (overlayPresenter == null)
            {
                overlayPresenter = FindFirst<OlympusTutorialOverlayPresenter>();
            }

            if (overlayPresenter == null && Application.isPlaying)
            {
                overlayPresenter = gameObject.AddComponent<OlympusTutorialOverlayPresenter>();
            }

            if (mobileHud == null)
            {
                mobileHud = FindFirst<BossBarrageLaneReviewMobileHud>();
            }
        }

        private void CacheActionEnabledStates()
        {
            cachedRangedBasicEnabled = rangedBasicAttackAction == null || rangedBasicAttackAction.enabled;
            cachedSkill1Enabled = skill1Action == null || skill1Action.enabled;
            cachedSummonSlot1Enabled = summonSlot1Action == null || summonSlot1Action.enabled;
            int supportCount = supportSummonActions != null ? supportSummonActions.Length : 0;
            cachedSupportEnabled = new bool[supportCount];
            for (int i = 0; i < supportCount; i++)
            {
                cachedSupportEnabled[i] = supportSummonActions[i] == null || supportSummonActions[i].enabled;
            }

            hasCachedActionEnabledStates = true;
        }

        private void RestoreActionEnabledStates()
        {
            if (!hasCachedActionEnabledStates)
            {
                return;
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.enabled = cachedRangedBasicEnabled;
            }

            if (skill1Action != null)
            {
                skill1Action.enabled = cachedSkill1Enabled;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.enabled = cachedSummonSlot1Enabled;
            }

            int supportCount = supportSummonActions != null ? supportSummonActions.Length : 0;
            for (int i = 0; i < supportCount; i++)
            {
                if (supportSummonActions[i] != null)
                {
                    bool enabled = i < cachedSupportEnabled.Length && cachedSupportEnabled[i];
                    supportSummonActions[i].enabled = enabled;
                }
            }
        }

        private void SetMeleeMode()
        {
            if (combatModeController == null)
            {
                return;
            }

            combatModeController.enabled = true;
            combatModeController.SetMeleeMode();
        }

        private void SetRangedMode()
        {
            if (combatModeController == null)
            {
                return;
            }

            combatModeController.enabled = true;
            combatModeController.SetRangedMode();
        }

        private void SetCombatModeInputLocked(bool locked)
        {
            combatModeController?.SetCinematicInputLocked(locked);
        }

        private void SetPlayerActionInputLocked(bool locked)
        {
            actionController?.SetCinematicInputLocked(locked);
        }

        private void SetRangedBasicAttackInputLocked(bool locked)
        {
            rangedBasicAttackAction?.SetCinematicInputLocked(locked);
        }

        private void SetOverlayGuideState(OlympusTutorialOverlayPresenter.GuideState guideState)
        {
            overlayPresenter?.SetGuideState(guideState);
        }

        private void ApplyStepInputLocks()
        {
            bool cueLocked = stepPhase == TutorialStepPhase.Cue;
            bool committed = stepPhase == TutorialStepPhase.Committed;

            switch (step)
            {
                case TutorialStep.Melee:
                    SetPlayerActionInputLocked(cueLocked || committed);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.Move:
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.SwapToRanged:
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(cueLocked || committed);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.Fire:
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(cueLocked || committed);
                    break;
                case TutorialStep.Dodge:
                    SetPlayerActionInputLocked(cueLocked || committed);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.ClearTargets:
                    SetPlayerActionInputLocked(cueLocked || committed);
                    SetCombatModeInputLocked(false);
                    SetRangedBasicAttackInputLocked(cueLocked || committed);
                    break;
                case TutorialStep.Completed:
                case TutorialStep.Inactive:
                    SetPlayerActionInputLocked(false);
                    SetCombatModeInputLocked(false);
                    SetRangedBasicAttackInputLocked(false);
                    break;
            }
        }

        private void SetRangedFireEnabled(bool enabled)
        {
            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.enabled = enabled;
                if (enabled)
                {
                    SetTransformAndParentsActive(rangedBasicAttackAction.ProjectileRoot);
                }
            }
        }

        private static void SetTransformAndParentsActive(Transform target)
        {
            Transform current = target;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    current.gameObject.SetActive(true);
                }

                current = current.parent;
            }
        }

        private void SetOptionalActionsEnabled(bool enabled)
        {
            if (skill1Action != null)
            {
                skill1Action.enabled = enabled;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.enabled = enabled;
            }

            if (supportSummonActions == null)
            {
                return;
            }

            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                if (supportSummonActions[i] != null)
                {
                    supportSummonActions[i].enabled = enabled;
                }
            }
        }

        private void ConfigureTargetCandidates(CombatHealth[] candidates)
        {
            if (targetSelector != null)
            {
                targetSelector.ConfigureTargetCandidates(candidates ?? Array.Empty<CombatHealth>());
            }
        }

        private void SetEnemyGameplayEnabled(bool enabled)
        {
            if (tutorialEnemyGameplayBehaviours == null)
            {
                return;
            }

            for (int i = 0; i < tutorialEnemyGameplayBehaviours.Length; i++)
            {
                if (tutorialEnemyGameplayBehaviours[i] != null)
                {
                    tutorialEnemyGameplayBehaviours[i].enabled = enabled;
                }
            }
        }

        private void PositionFirstTargetForMeleeStep()
        {
            if (!positionFirstTargetForMeleeStep || player == null || tutorialTargets == null)
            {
                return;
            }

            CombatHealth target = FindFirstAliveTutorialTarget();
            if (target == null)
            {
                return;
            }

            Vector3 forward = ResolvePlayerPlanarForward();
            Vector3 right = ResolvePlanarRight(forward);
            Vector3 targetPosition = player.transform.position
                + forward * meleeTargetDistance
                + right * meleeTargetSideOffset;
            Transform targetTransform = target.transform;
            targetTransform.position = new Vector3(
                targetPosition.x,
                targetTransform.position.y,
                targetPosition.z);
            if (forward.sqrMagnitude > 0.0001f)
            {
                targetTransform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private void PositionFirstTargetForRangedStep()
        {
            if (!positionFirstTargetForRangedStep || player == null || tutorialTargets == null)
            {
                return;
            }

            CombatHealth target = FindFirstAliveTutorialTarget();
            if (target == null)
            {
                return;
            }

            Vector3 aimPoint = ResolveRangedTutorialAimPoint();
            Transform targetTransform = target.transform;
            targetTransform.position = aimPoint - Vector3.up * rangedTargetAimHeight;

            Vector3 lookDirection = Vector3.ProjectOnPlane(
                player.transform.position - targetTransform.position,
                Vector3.up);
            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                targetTransform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private CombatHealth FindFirstAliveTutorialTarget()
        {
            if (tutorialTargets == null)
            {
                return null;
            }

            for (int i = 0; i < tutorialTargets.Length; i++)
            {
                CombatHealth target = tutorialTargets[i];
                if (target != null && target.gameObject.activeInHierarchy && target.IsAlive)
                {
                    return target;
                }
            }

            return null;
        }

        private Vector3 ResolveRangedTutorialAimPoint()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Vector3 point = ray.GetPoint(rangedTargetDistance);
                point.y = player.transform.position.y + rangedTargetAimHeight;
                return point;
            }

            Vector3 forward = ResolvePlayerPlanarForward();
            return player.transform.position
                + forward * rangedTargetDistance
                + Vector3.up * rangedTargetAimHeight;
        }

        private Vector3 ResolvePlayerPlanarForward()
        {
            Vector3 forward = player != null
                ? player.FacingDirection
                : transform.forward;
            forward = Vector3.ProjectOnPlane(forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private static Vector3 ResolvePlanarRight(Vector3 forward)
        {
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
        }

        private void UpdateTutorialAimPreviewHold()
        {
            if (step == TutorialStep.Fire)
            {
                SetTutorialAimPreviewHeld(true);
            }
        }

        private void SetTutorialAimPreviewHeld(bool active)
        {
            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetExternalAimPreviewHeld(active);
            }
        }

        private static bool HasAny(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return false;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountActiveAlive(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health != null
                    && health.gameObject.activeInHierarchy
                    && health.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ResetCombatHealthsToFull(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                healths[i]?.ResetHealthToFull();
            }
        }

        private static T FindFirst<T>() where T : UnityEngine.Object
        {
            T[] objects = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            return objects.Length > 0 ? objects[0] : null;
        }

        private static void SetObjectsActive(GameObject[] objects, bool active)
        {
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null && objects[i].activeSelf != active)
                {
                    objects[i].SetActive(active);
                }
            }
        }

        private static void SetCollidersEnabled(Collider[] colliders, bool enabled)
        {
            if (colliders == null)
            {
                return;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = enabled;
                }
            }
        }

        private static void SetCombatHealthRootsActive(CombatHealth[] healths, bool active)
        {
            if (healths == null)
            {
                return;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null && healths[i].gameObject.activeSelf != active)
                {
                    healths[i].gameObject.SetActive(active);
                }
            }
        }

        private static void SetCombatHealthRootCollidersEnabled(CombatHealth[] healths, bool enabled)
        {
            if (healths == null)
            {
                return;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health == null)
                {
                    continue;
                }

                Collider[] colliders = health.GetComponentsInChildren<Collider>(includeInactive: true);
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    if (colliders[colliderIndex] != null)
                    {
                        colliders[colliderIndex].enabled = enabled;
                    }
                }
            }
        }
    }
}
