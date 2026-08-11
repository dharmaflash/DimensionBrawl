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
        private const string SystemGuideSpeaker = "천계관리시스템";
        private const string SoldierGuideSpeaker = "병사";
        private const float SoldierChallengeOpeningVoicePaddingSeconds = 0.4f;

        public enum DialogueAudioCueId
        {
            None,
            SoldierChallenge,
            MeleeCue,
            MoveCue,
            SwapToRangedCue,
            FireCue,
            DodgeCue,
            ClearTargetsCue,
            MeleeConfirm,
            MoveConfirm,
            SwapToRangedConfirm,
            FireConfirm,
            DodgeConfirm,
            ClearTargetsConfirm
        }

        [Serializable]
        public struct DialogueAudioCue
        {
            [SerializeField] private DialogueAudioCueId cueId;
            [SerializeField] private AudioClip clip;
            [SerializeField, Range(0f, 1f)] private float volume;
            [SerializeField, Min(0f)] private float delaySeconds;

            public DialogueAudioCue(
                DialogueAudioCueId cueId,
                AudioClip clip,
                float volume,
                float delaySeconds)
            {
                this.cueId = cueId;
                this.clip = clip;
                this.volume = volume;
                this.delaySeconds = delaySeconds;
            }

            public DialogueAudioCueId CueId => cueId;
            public AudioClip Clip => clip;
            public float Volume => Mathf.Clamp01(volume);
            public float DelaySeconds => Mathf.Max(0f, delaySeconds);
        }

        private static readonly DialogueAudioCueId[] DefaultDialogueAudioCueIds =
        {
            DialogueAudioCueId.SoldierChallenge,
            DialogueAudioCueId.MeleeCue,
            DialogueAudioCueId.MoveCue,
            DialogueAudioCueId.SwapToRangedCue,
            DialogueAudioCueId.FireCue,
            DialogueAudioCueId.DodgeCue,
            DialogueAudioCueId.ClearTargetsCue,
            DialogueAudioCueId.MeleeConfirm,
            DialogueAudioCueId.MoveConfirm,
            DialogueAudioCueId.SwapToRangedConfirm,
            DialogueAudioCueId.FireConfirm,
            DialogueAudioCueId.DodgeConfirm,
            DialogueAudioCueId.ClearTargetsConfirm
        };

        private enum TutorialStep
        {
            Inactive,
            SoldierChallenge,
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
        [SerializeField] private bool preventPlayerDamageDuringTutorial = true;
        [SerializeField, Min(0.05f)] private float tutorialPlayerInvulnerabilityRefreshSeconds = 0.35f;
        [SerializeField, Min(0f)] private float cuePrimeSeconds = 0.45f;
        [SerializeField, Min(0f)] private float soldierChallengeReadSeconds = 1.8f;
        [SerializeField, Min(0f)] private float completionRecordSeconds = 0.7f;
        [SerializeField, Min(0.1f)] private float promptRepeatSeconds = 4.0f;
        [SerializeField, Min(0f)] private float minimumCueReadSeconds = 0.85f;
        [SerializeField, Min(0f)] private float minimumActionObserveSeconds = 0.35f;
        [SerializeField, Min(0f)] private float minimumCompletionReadSeconds = 1.15f;

        [Header("Movement Step")]
        [SerializeField, Min(0f)] private float movementCompleteDistance = 0.75f;

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
        [SerializeField] private MonoBehaviour moveInputGateBehaviour;
        [SerializeField] private AudioSource overlayAudioSource;
        [SerializeField] private AudioClip overlayOpenSfx;
        [SerializeField, Range(0f, 1f)] private float overlayOpenSfxVolume = 0.82f;
        [SerializeField] private DialogueAudioCue[] overlayDialogueAudioCues =
            CreateDefaultDialogueAudioCueSlots();
        [SerializeField] private PlayerMovementController player;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private PlayerLockTargetController lockTargetController;
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
        private bool rangedAimPreviewObserved;
        private bool rangedProjectileFiredObserved;
        private bool rangedTargetDamageObserved;
        private bool stepTargetDeathObserved;
        private bool dodgeObserved;
        private bool hasRuntimeBoundsCenter;
        private bool hasCachedActionEnabledStates;
        private bool hasCachedBossTelegraphStates;
        private bool cachedRangedBasicEnabled;
        private bool cachedSkill1Enabled;
        private bool cachedSummonSlot1Enabled;
        private bool[] cachedSupportEnabled = Array.Empty<bool>();
        private BossBarrageLaneTelegraphPresenter[] cachedBossTelegraphPresenters =
            Array.Empty<BossBarrageLaneTelegraphPresenter>();
        private bool[] cachedBossTelegraphEnabled = Array.Empty<bool>();
        private float stepTimer;
        private float phaseTimer;
        private float rangedAimPreviewHeldSeconds;
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

        public static DialogueAudioCue[] CreateDefaultDialogueAudioCueSlots()
        {
            var cues = new DialogueAudioCue[DefaultDialogueAudioCueIds.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                cues[i] = new DialogueAudioCue(DefaultDialogueAudioCueIds[i], null, 1f, 0f);
            }

            return cues;
        }

        public static DialogueAudioCue[] NormalizeDialogueAudioCueSlots(DialogueAudioCue[] cues)
        {
            var results = new DialogueAudioCue[DefaultDialogueAudioCueIds.Length];
            for (int i = 0; i < DefaultDialogueAudioCueIds.Length; i++)
            {
                DialogueAudioCueId cueId = DefaultDialogueAudioCueIds[i];
                results[i] = FindDialogueAudioCue(cues, cueId, out DialogueAudioCue cue)
                    ? cue
                    : new DialogueAudioCue(cueId, null, 1f, 0f);
            }

            return results;
        }

        public void BindRuntimeContext(
            PlayerMovementController newPlayer,
            PlayerCombatModeController newCombatModeController,
            PlayerCombatTargetSelector newTargetSelector,
            PlayerActionController newActionController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction[] newSupportSummonActions,
            CinematicTutorialPromptPresenter newPromptPresenter,
            CombatHealth[] newTutorialTargets,
            Behaviour[] newTutorialEnemyGameplayBehaviours,
            Collider[] newTutorialRouteBlockers)
        {
            player = newPlayer != null ? newPlayer : player;
            playerHealth = player != null ? player.GetComponent<CombatHealth>() : playerHealth;
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
            promptPresenter = newPromptPresenter != null ? newPromptPresenter : promptPresenter;
            tutorialTargets = newTutorialTargets ?? tutorialTargets ?? Array.Empty<CombatHealth>();
            tutorialEnemyGameplayBehaviours =
                newTutorialEnemyGameplayBehaviours
                ?? tutorialEnemyGameplayBehaviours
                ?? Array.Empty<Behaviour>();
            tutorialRouteBlockers = newTutorialRouteBlockers ?? tutorialRouteBlockers ?? Array.Empty<Collider>();
        }

        public void ConfigureOverlayAudio(
            AudioSource audioSource,
            AudioClip openSfx,
            float volume)
        {
            overlayAudioSource = audioSource != null ? audioSource : overlayAudioSource;
            overlayOpenSfx = openSfx != null ? openSfx : overlayOpenSfx;
            overlayOpenSfxVolume = Mathf.Clamp01(volume);
            ApplyOverlayPresentationBindings();
        }

        public void ConfigureOverlayDialogueAudio(DialogueAudioCue[] dialogueAudioCues)
        {
            overlayDialogueAudioCues = NormalizeDialogueAudioCueSlots(dialogueAudioCues);
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
            CacheAndDisableBossTelegraphs();
            meleeHitObserved = false;
            movementObserved = false;
            rangedModeObserved = false;
            rangedAimPreviewObserved = false;
            rangedProjectileFiredObserved = false;
            rangedTargetDamageObserved = false;
            stepTargetDeathObserved = false;
            dodgeObserved = false;
            rangedAimPreviewHeldSeconds = 0f;
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
            ApplyTutorialPlayerInvulnerability();

            if (combatModeController != null)
            {
                combatModeController.enabled = true;
                combatModeController.SetMeleeMode();
            }

            StartStep(TutorialStep.SoldierChallenge);
        }

        public void CancelTutorial()
        {
            if (step == TutorialStep.Inactive)
            {
                return;
            }

            UnsubscribeObservers();
            SetTutorialAimPreviewHeld(false);
            SetMovementInputLocked(false);
            SetPlayerActionInputLocked(false);
            SetCombatModeInputLocked(false);
            SetRangedBasicAttackInputLocked(false);
            promptPresenter?.HidePrompt();
            overlayPresenter?.Hide();
            SetEnemyGameplayEnabled(false, keepHealthDamageable: false);
            RestoreBossTelegraphs();
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
            ApplyTutorialPlayerInvulnerability();
            ApplyStepInputLocks();
            RepeatPromptIfNeeded();

            switch (stepPhase)
            {
                case TutorialStepPhase.Cue:
                    if (phaseTimer >= ResolveCueReadSeconds())
                    {
                        if (step == TutorialStep.SoldierChallenge)
                        {
                            StartStep(TutorialStep.Melee);
                            return;
                        }

                        ActivateStepInputWindow();
                    }
                    return;
                case TutorialStepPhase.AwaitingAction:
                    UpdateAwaitingActionStep();
                    return;
                case TutorialStepPhase.Committed:
                    if (phaseTimer >= ResolveCompletionReadSeconds())
                    {
                        AdvanceAfterCommittedStep();
                    }
                    return;
            }
        }

        private void UpdateAwaitingActionStep()
        {
            UpdateCurrentStepObservation();
            if (!HasActionObservationWindowElapsed())
            {
                return;
            }

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
                case TutorialStep.SoldierChallenge:
                    ConfigureTargetCandidates(tutorialTargets);
                    SetMeleeMode();
                    SetCombatModeInputLocked(true);
                    SetRangedFireEnabled(false);
                    SetOptionalActionsEnabled(false);
                    SetEnemyGameplayEnabled(false);
                    break;
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
                    rangedAimPreviewObserved = false;
                    rangedAimPreviewHeldSeconds = 0f;
                    ConfigureTargetCandidates(tutorialTargets);
                    SetRangedMode();
                    SetCombatModeInputLocked(true);
                    SetRangedFireEnabled(true);
                    SetTutorialAimPreviewHeld(false);
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
            SetMovementInputLocked(false);
            SetPlayerActionInputLocked(false);
            SetCombatModeInputLocked(false);
            SetRangedBasicAttackInputLocked(false);
            UnsubscribeObservers();
            SetEnemyGameplayEnabled(false, keepHealthDamageable: false);
            SetCombatHealthRootCollidersEnabled(tutorialTargets, false);
            SetCombatHealthRootsActive(tutorialTargets, false);
            SetCollidersEnabled(tutorialRouteBlockers, false);
            SetObjectsActive(tutorialBoundsRoots, false);
            ConfigureTargetCandidates(Array.Empty<CombatHealth>());
            RestoreBossTelegraphs();
            RestoreActionEnabledStates();
            overlayPresenter?.Hide();
            promptPresenter?.HidePrompt();
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
                    rangedAimPreviewObserved = false;
                    rangedProjectileFiredObserved = false;
                    rangedTargetDamageObserved = false;
                    rangedAimPreviewHeldSeconds = 0f;
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
            if (player == null)
            {
                return movementCompleteDistance <= 0f && movementObserved;
            }

            Vector3 offset = Vector3.ProjectOnPlane(
                player.transform.position - movementStartPosition,
                Vector3.up);
            if (movementCompleteDistance <= 0f)
            {
                return movementObserved && offset.magnitude > 0.05f;
            }

            return offset.magnitude >= movementCompleteDistance;
        }

        private bool HasCompletedSwapStep()
        {
            return rangedModeObserved
                || (combatModeController != null && combatModeController.IsRangedMode);
        }

        private bool HasCompletedFireStep()
        {
            return rangedAimPreviewObserved
                && rangedAimPreviewHeldSeconds >= fireAimPreviewLeadSeconds
                && rangedProjectileFiredObserved
                && (rangedTargetDamageObserved || stepTargetDeathObserved);
        }

        private void UpdateCurrentStepObservation()
        {
            if (step != TutorialStep.Fire || stepPhase != TutorialStepPhase.AwaitingAction)
            {
                return;
            }

            if (rangedBasicAttackAction != null && rangedBasicAttackAction.IsAimPreviewActive)
            {
                rangedAimPreviewObserved = true;
                rangedAimPreviewHeldSeconds += Time.deltaTime;
            }
        }

        private bool HasActionObservationWindowElapsed()
        {
            return phaseTimer >= minimumActionObserveSeconds;
        }

        private float ResolveCueReadSeconds()
        {
            float readSeconds = Mathf.Max(cuePrimeSeconds, minimumCueReadSeconds);
            if (step != TutorialStep.SoldierChallenge)
            {
                return readSeconds;
            }

            readSeconds = Mathf.Max(readSeconds, soldierChallengeReadSeconds);
            DialogueAudioCue cue = ResolveDialogueAudioCue(DialogueAudioCueId.SoldierChallenge);
            if (cue.Clip != null)
            {
                readSeconds = Mathf.Max(
                    readSeconds,
                    cue.DelaySeconds + cue.Clip.length + SoldierChallengeOpeningVoicePaddingSeconds);
            }

            return readSeconds;
        }

        private float ResolveCompletionReadSeconds()
        {
            return Mathf.Max(completionRecordSeconds, minimumCompletionReadSeconds);
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
                case TutorialStep.SoldierChallenge:
                    ShowGuide(
                        DialogueAudioCueId.SoldierChallenge,
                        SoldierGuideSpeaker,
                        "뭐하는 놈이냐!",
                        string.Empty,
                        OlympusTutorialOverlayPresenter.FocusKind.None,
                        new Vector2(0.5f, 0.5f));
                    break;
                case TutorialStep.Melee:
                    ShowGuide(
                        DialogueAudioCueId.MeleeCue,
                        SystemGuideSpeaker,
                        "근접 공격 버튼을 사용해 가까운 적을 공격할 수 있습니다.",
                        "근접 공격",
                        OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Move:
                    ShowGuide(
                        DialogueAudioCueId.MoveCue,
                        SystemGuideSpeaker,
                        "조이스틱 버튼을 사용해 이동할 수 있습니다.",
                        "이동",
                        OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                        new Vector2(0.16f, 0.16f));
                    break;
                case TutorialStep.SwapToRanged:
                    ShowGuide(
                        DialogueAudioCueId.SwapToRangedCue,
                        SystemGuideSpeaker,
                        "전환 버튼을 사용해 원거리 사격으로 변경할 수 있습니다.",
                        "모드 전환",
                        OlympusTutorialOverlayPresenter.FocusKind.SwapMode,
                        new Vector2(0.82f, 0.24f));
                    break;
                case TutorialStep.Fire:
                    ShowGuide(
                        DialogueAudioCueId.FireCue,
                        SystemGuideSpeaker,
                        "사격 버튼을 길게 누르면 조준 상태에 돌입합니다. 조준 중 적을 명중시키십시오.",
                        "조준 사격",
                        OlympusTutorialOverlayPresenter.FocusKind.RangedAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Dodge:
                    ShowGuide(
                        DialogueAudioCueId.DodgeCue,
                        SystemGuideSpeaker,
                        "적의 공격을 정확한 타이밍에 회피하면 일정 시간 동안 무적 상태에 돌입합니다.",
                        "회피",
                        OlympusTutorialOverlayPresenter.FocusKind.Dodge,
                        new Vector2(0.92f, 0.24f));
                    break;
                case TutorialStep.ClearTargets:
                    ShowGuide(
                        DialogueAudioCueId.ClearTargetsCue,
                        SystemGuideSpeaker,
                        "남은 적을 처치하십시오.",
                        "전투 완료",
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
                        DialogueAudioCueId.MeleeConfirm,
                        SystemGuideSpeaker,
                        "근접 공격 입력이 확인되었습니다.",
                        "\ud655\uc778",
                        OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Move:
                    ShowGuide(
                        DialogueAudioCueId.MoveConfirm,
                        SystemGuideSpeaker,
                        "이동 입력이 확인되었습니다.",
                        "\ud655\uc778",
                        OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                        new Vector2(0.16f, 0.16f));
                    break;
                case TutorialStep.SwapToRanged:
                    ShowGuide(
                        DialogueAudioCueId.SwapToRangedConfirm,
                        SystemGuideSpeaker,
                        "원거리 사격 모드 전환이 확인되었습니다.",
                        "\ud655\uc778",
                        OlympusTutorialOverlayPresenter.FocusKind.SwapMode,
                        new Vector2(0.82f, 0.24f));
                    break;
                case TutorialStep.Fire:
                    ShowGuide(
                        DialogueAudioCueId.FireConfirm,
                        SystemGuideSpeaker,
                        "조준 및 사격 명중이 확인되었습니다.",
                        "\ud655\uc778",
                        OlympusTutorialOverlayPresenter.FocusKind.RangedAttack,
                        new Vector2(0.92f, 0.10f));
                    break;
                case TutorialStep.Dodge:
                    ShowGuide(
                        DialogueAudioCueId.DodgeConfirm,
                        SystemGuideSpeaker,
                        "회피 입력이 확인되었습니다.",
                        "\ud655\uc778",
                        OlympusTutorialOverlayPresenter.FocusKind.Dodge,
                        new Vector2(0.92f, 0.24f));
                    break;
                case TutorialStep.ClearTargets:
                    ShowGuide(
                        DialogueAudioCueId.ClearTargetsConfirm,
                        SystemGuideSpeaker,
                        "긴급 무장 프로토콜 종료.",
                        "\ud655\uc778",
                        OlympusTutorialOverlayPresenter.FocusKind.Route,
                        new Vector2(0.5f, 0.76f));
                    break;
            }
        }

        private void ShowGuide(
            DialogueAudioCueId dialogueAudioCueId,
            string speaker,
            string dialogue,
            string inputLabel,
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            Vector2 anchor)
        {
            Vector2 resolvedAnchor = ResolveHudAnchor(focusKind, anchor);
            if (overlayPresenter != null)
            {
                overlayPresenter.SetGuideProgress(
                    ResolveTutorialStepIndex(),
                    ResolveTutorialStepCount(),
                    ResolveTutorialPhaseLabel());
                DialogueAudioCue audioCue = ResolveDialogueAudioCue(dialogueAudioCueId);
                overlayPresenter.Show(
                    speaker,
                    dialogue,
                    inputLabel,
                    focusKind,
                    resolvedAnchor,
                    audioCue.Clip,
                    audioCue.Volume,
                    audioCue.DelaySeconds);
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

        private DialogueAudioCue ResolveDialogueAudioCue(DialogueAudioCueId cueId)
        {
            return FindDialogueAudioCue(overlayDialogueAudioCues, cueId, out DialogueAudioCue cue)
                ? cue
                : default;
        }

        private static bool FindDialogueAudioCue(
            DialogueAudioCue[] cues,
            DialogueAudioCueId cueId,
            out DialogueAudioCue cue)
        {
            if (cueId == DialogueAudioCueId.None || cues == null)
            {
                cue = default;
                return false;
            }

            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].CueId == cueId)
                {
                    cue = cues[i];
                    return true;
                }
            }

            cue = default;
            return false;
        }

        private Vector2 ResolveHudAnchor(
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            Vector2 fallbackAnchor)
        {
            if (TryResolveCombatHudAnchor(focusKind, out Vector2 combatHudAnchor))
            {
                return combatHudAnchor;
            }

            return fallbackAnchor;
        }

        private int ResolveTutorialStepIndex()
        {
            switch (step)
            {
                case TutorialStep.Melee:
                    return 1;
                case TutorialStep.Move:
                    return 2;
                case TutorialStep.SwapToRanged:
                    return 3;
                case TutorialStep.Fire:
                    return 4;
                case TutorialStep.Dodge:
                    return 5;
                case TutorialStep.ClearTargets:
                case TutorialStep.Completed:
                    return 6;
                default:
                    return 0;
            }
        }

        private static int ResolveTutorialStepCount()
        {
            return 6;
        }

        private string ResolveTutorialPhaseLabel()
        {
            switch (stepPhase)
            {
                case TutorialStepPhase.Cue:
                    return "READ";
                case TutorialStepPhase.AwaitingAction:
                    return "ACT";
                case TutorialStepPhase.Committed:
                    return "OK";
                default:
                    return string.Empty;
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

        private void ApplyTutorialPlayerInvulnerability()
        {
            if (!preventPlayerDamageDuringTutorial || !IsRunning)
            {
                return;
            }

            CombatHealth health = ResolvePlayerHealth();
            health?.SetInvulnerableUntil(
                Time.time + Mathf.Max(0.05f, tutorialPlayerInvulnerabilityRefreshSeconds));
        }

        private CombatHealth ResolvePlayerHealth()
        {
            if (playerHealth != null)
            {
                return playerHealth;
            }

            if (player != null)
            {
                playerHealth = player.GetComponent<CombatHealth>();
            }

            if (playerHealth == null && actionController != null)
            {
                playerHealth = actionController.GetComponent<CombatHealth>();
            }

            return playerHealth;
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
                lockTargetController = lockTargetController != null
                    ? lockTargetController
                    : player.GetComponent<PlayerLockTargetController>();
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
                overlayPresenter = GetComponent<OlympusTutorialOverlayPresenter>();
            }

            if (overlayPresenter == null && Application.isPlaying)
            {
                overlayPresenter = gameObject.AddComponent<OlympusTutorialOverlayPresenter>();
            }

            if (moveInputGateBehaviour is not ICombatMoveInputGate)
            {
                MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is ICombatMoveInputGate)
                    {
                        moveInputGateBehaviour = behaviours[i];
                        break;
                    }
                }
            }

            ApplyOverlayPresentationBindings();
        }

        private void ApplyOverlayPresentationBindings()
        {
            if (overlayPresenter == null)
            {
                return;
            }

            overlayPresenter.ConfigureCommunicatorAudio(
                overlayAudioSource,
                overlayOpenSfx,
                overlayOpenSfxVolume);
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

        private void CacheAndDisableBossTelegraphs()
        {
            if (hasCachedBossTelegraphStates)
            {
                return;
            }

            cachedBossTelegraphPresenters =
                UnityEngine.Object.FindObjectsByType<BossBarrageLaneTelegraphPresenter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            int presenterCount = cachedBossTelegraphPresenters != null
                ? cachedBossTelegraphPresenters.Length
                : 0;
            cachedBossTelegraphEnabled = new bool[presenterCount];

            for (int i = 0; i < presenterCount; i++)
            {
                BossBarrageLaneTelegraphPresenter presenter = cachedBossTelegraphPresenters[i];
                if (presenter == null)
                {
                    continue;
                }

                cachedBossTelegraphEnabled[i] = presenter.enabled;
                presenter.enabled = false;
            }

            hasCachedBossTelegraphStates = true;
        }

        private void RestoreBossTelegraphs()
        {
            if (!hasCachedBossTelegraphStates)
            {
                return;
            }

            int presenterCount = cachedBossTelegraphPresenters != null
                ? cachedBossTelegraphPresenters.Length
                : 0;
            for (int i = 0; i < presenterCount; i++)
            {
                BossBarrageLaneTelegraphPresenter presenter = cachedBossTelegraphPresenters[i];
                if (presenter != null)
                {
                    presenter.enabled = i < cachedBossTelegraphEnabled.Length
                        && cachedBossTelegraphEnabled[i];
                }
            }

            cachedBossTelegraphPresenters = Array.Empty<BossBarrageLaneTelegraphPresenter>();
            cachedBossTelegraphEnabled = Array.Empty<bool>();
            hasCachedBossTelegraphStates = false;
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
            combatModeController?.SetCinematicInputLocked(PlayerInputLockSource.CorridorTutorial, locked);
        }

        private void SetPlayerActionInputLocked(bool locked)
        {
            actionController?.SetCinematicInputLocked(PlayerInputLockSource.CorridorTutorial, locked);
        }

        private void SetRangedBasicAttackInputLocked(bool locked)
        {
            SetRangedBasicAttackInputLocked(locked, false);
        }

        private void SetRangedBasicAttackInputLocked(bool locked, bool preserveHeldAim)
        {
            rangedBasicAttackAction?.SetCinematicInputLocked(
                PlayerInputLockSource.CorridorTutorial,
                locked,
                preserveHeldAim);
        }

        private void SetMovementInputLocked(bool locked)
        {
            if (player == null)
            {
                (moveInputGateBehaviour as ICombatMoveInputGate)?.SetInputBlocked(
                    PlayerInputLockSource.CorridorTutorial,
                    locked);
                return;
            }

            if (locked)
            {
                (moveInputGateBehaviour as ICombatMoveInputGate)?.SetInputBlocked(
                    PlayerInputLockSource.CorridorTutorial,
                    true);
                player.SetMoveInput(Vector2.zero);
                player.SetCinematicMoveInputLocked(PlayerInputLockSource.CorridorTutorial, true);
                return;
            }

            player.SetCinematicMoveInputLocked(PlayerInputLockSource.CorridorTutorial, false);
            (moveInputGateBehaviour as ICombatMoveInputGate)?.SetInputBlocked(
                PlayerInputLockSource.CorridorTutorial,
                false);
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
                case TutorialStep.SoldierChallenge:
                    SetMovementInputLocked(true);
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.Melee:
                    SetMovementInputLocked(true);
                    SetPlayerActionInputLocked(cueLocked || committed);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.Move:
                    SetMovementInputLocked(cueLocked || committed);
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.SwapToRanged:
                    SetMovementInputLocked(true);
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(cueLocked || committed);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.Fire:
                    SetMovementInputLocked(true);
                    SetPlayerActionInputLocked(true);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(cueLocked || committed, committed);
                    break;
                case TutorialStep.Dodge:
                    SetMovementInputLocked(cueLocked);
                    SetPlayerActionInputLocked(cueLocked || committed);
                    SetCombatModeInputLocked(true);
                    SetRangedBasicAttackInputLocked(true);
                    break;
                case TutorialStep.ClearTargets:
                    SetMovementInputLocked(cueLocked || committed);
                    SetPlayerActionInputLocked(cueLocked || committed);
                    SetCombatModeInputLocked(false);
                    SetRangedBasicAttackInputLocked(cueLocked || committed);
                    break;
                case TutorialStep.Completed:
                case TutorialStep.Inactive:
                    SetMovementInputLocked(false);
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
            CombatHealth[] resolvedCandidates = candidates ?? Array.Empty<CombatHealth>();
            if (targetSelector != null)
            {
                targetSelector.ConfigureTargetCandidates(resolvedCandidates);
            }

            CombatHealth firstTarget = FindFirstAlive(resolvedCandidates);
            if (firstTarget != null)
            {
                FocusTutorialTarget(firstTarget, step == TutorialStep.Fire || step == TutorialStep.ClearTargets);
            }
            else
            {
                lockTargetController?.ClearHardLock();
            }
        }

        private void SetEnemyGameplayEnabled(bool enabled, bool keepHealthDamageable = true)
        {
            if (tutorialEnemyGameplayBehaviours == null)
            {
                return;
            }

            for (int i = 0; i < tutorialEnemyGameplayBehaviours.Length; i++)
            {
                Behaviour behaviour = tutorialEnemyGameplayBehaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                // Active tutorial targets remain damageable while their AI, sensors, and
                // presenters are quiesced. Terminal cleanup may disable health explicitly.
                behaviour.enabled = behaviour is CombatHealth
                    ? keepHealthDamageable || enabled
                    : enabled;
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
            FocusTutorialTarget(target, hardLock: false);
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
            FocusTutorialTarget(target, hardLock: true);
        }

        private CombatHealth FindFirstAliveTutorialTarget()
        {
            return FindFirstAlive(tutorialTargets);
        }

        private static CombatHealth FindFirstAlive(CombatHealth[] targets)
        {
            if (targets == null)
            {
                return null;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                CombatHealth target = targets[i];
                if (target != null && target.gameObject.activeInHierarchy && target.IsAlive)
                {
                    return target;
                }
            }

            return null;
        }

        private void FocusTutorialTarget(CombatHealth target, bool hardLock)
        {
            if (target == null || !target.IsAlive)
            {
                return;
            }

            targetSelector?.NotifyTargetContact(target);
            if (lockTargetController != null)
            {
                if (hardLock)
                {
                    lockTargetController.RequestHardLock(target);
                }
                else
                {
                    lockTargetController.ClearHardLock();
                }
            }

            if (player == null)
            {
                return;
            }

            Vector3 targetDirection = Vector3.ProjectOnPlane(
                target.transform.position - player.transform.position,
                Vector3.up);
            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                player.RequestFacingDirection(targetDirection.normalized, 0.25f, snapImmediately: true);
            }
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
