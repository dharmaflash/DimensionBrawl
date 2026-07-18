using System;
using System.Globalization;
using DimensionBrawl.LevelDesign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI.ContentFactoryReview
{
    [DisallowMultipleComponent]
    public sealed class ContentFactoryEncounterPlanReviewController : MonoBehaviour
    {
        public const int RequiredWaveCardCount = 3;
        public const string ReviewTitle = "STAGE ENCOUNTER PLAN REVIEW";
        private const string ReviewDisplayTitle = "STAGE ENCOUNTER\nPLAN REVIEW";
        public const string ReviewUnavailableStatus =
            "REVIEW UNAVAILABLE / PROFILE INVALID OR MISSING";

        private static readonly Color PendingAccent =
            new Color(0.22f, 0.28f, 0.34f, 1f);
        private static readonly Color ActiveAccent =
            new Color(0.18f, 0.82f, 1f, 1f);
        private static readonly Color ClearedAccent =
            new Color(0.28f, 0.86f, 0.54f, 1f);
        private static readonly Color InterruptedAccent =
            new Color(1f, 0.46f, 0.24f, 1f);
        private static readonly Color UnavailableAccent =
            new Color(0.14f, 0.16f, 0.19f, 1f);

        [Header("Review Model")]
        [SerializeField] private StageEncounterPlanProfile profile;

        [Header("Identity and Ownership Boundary")]
        [SerializeField] private TMP_Text admissionBoundaryText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text identityText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text currentSpawnText;
        [SerializeField] private TMP_Text ownershipBoundaryText;

        [Header("Three-Wave Review Cards")]
        [SerializeField] private TMP_Text[] waveTitleTexts =
            new TMP_Text[RequiredWaveCardCount];
        [SerializeField] private TMP_Text[] waveStateTexts =
            new TMP_Text[RequiredWaveCardCount];
        [SerializeField] private TMP_Text[] waveDetailTexts =
            new TMP_Text[RequiredWaveCardCount];
        [SerializeField] private Image[] waveAccentImages =
            new Image[RequiredWaveCardCount];

        [Header("Local Review Actions")]
        [SerializeField] private Button beginButton;
        [SerializeField] private Button resolveButton;
        [SerializeField] private Button advanceButton;
        [SerializeField] private Button interruptButton;
        [SerializeField] private Button resetButton;

        private StageEncounterPlanReviewSession session;
        private bool interactionsBound;
        private string profileValidationError = string.Empty;

        public StageEncounterPlanReviewSession Session => session;
        public StageEncounterPlanReviewState CurrentState => session != null
            ? session.State
            : StageEncounterPlanReviewState.Ready;
        public string ProfileValidationError => profileValidationError;
        public bool HasExactWaveCardArrays =>
            HasRequiredLength(waveTitleTexts)
            && HasRequiredLength(waveStateTexts)
            && HasRequiredLength(waveDetailTexts)
            && HasRequiredLength(waveAccentImages);

        public void ConfigureCore(StageEncounterPlanProfile newProfile)
        {
            bool rebind = BeginRuntimeReconfiguration();
            profile = newProfile;
            ReloadProfile();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureTextView(
            TMP_Text admissionBoundary,
            TMP_Text title,
            TMP_Text identity,
            TMP_Text objective,
            TMP_Text state,
            TMP_Text progress,
            TMP_Text currentSpawn,
            TMP_Text ownershipBoundary)
        {
            admissionBoundaryText = admissionBoundary;
            titleText = title;
            identityText = identity;
            objectiveText = objective;
            stateText = state;
            progressText = progress;
            currentSpawnText = currentSpawn;
            ownershipBoundaryText = ownershipBoundary;
            ApplyCurrentView();
        }

        public void ConfigureWaveCards(
            TMP_Text[] titles,
            TMP_Text[] states,
            TMP_Text[] details,
            Image[] accents)
        {
            waveTitleTexts = titles ?? Array.Empty<TMP_Text>();
            waveStateTexts = states ?? Array.Empty<TMP_Text>();
            waveDetailTexts = details ?? Array.Empty<TMP_Text>();
            waveAccentImages = accents ?? Array.Empty<Image>();
            ApplyCurrentView();
        }

        public void ConfigureActions(
            Button begin,
            Button resolve,
            Button advance,
            Button interrupt,
            Button reset)
        {
            bool rebind = BeginRuntimeReconfiguration();
            beginButton = begin;
            resolveButton = resolve;
            advanceButton = advance;
            interruptButton = interrupt;
            resetButton = reset;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BindInteractions();
            if (session == null)
            {
                ReloadProfile();
            }
            else
            {
                ApplyCurrentView();
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UnbindInteractions();
        }

        private void OnValidate()
        {
            waveTitleTexts ??= Array.Empty<TMP_Text>();
            waveStateTexts ??= Array.Empty<TMP_Text>();
            waveDetailTexts ??= Array.Empty<TMP_Text>();
            waveAccentImages ??= Array.Empty<Image>();
        }

        public bool ReloadProfile()
        {
            session = null;
            profileValidationError = string.Empty;

            if (profile == null)
            {
                profileValidationError = "The review profile is missing.";
            }
            else if (!profile.TryValidate(out profileValidationError))
            {
                profileValidationError ??= "The review profile is invalid.";
            }
            else if (profile.WaveCount != RequiredWaveCardCount)
            {
                profileValidationError =
                    $"CF-01 requires exactly {RequiredWaveCardCount} waves.";
            }
            else
            {
                try
                {
                    session = new StageEncounterPlanReviewSession(profile);
                }
                catch (ArgumentException exception)
                {
                    profileValidationError = exception.Message;
                }
            }

            ApplyCurrentView();
            return session != null;
        }

        public bool BeginEncounter()
        {
            bool changed = session != null && session.TryBegin();
            ApplyCurrentView();
            return changed;
        }

        public bool ResolveCurrentCombatant()
        {
            bool changed = session != null
                && session.TryGetNextUnresolvedSpawn(
                    out StageEncounterPlanProfile.SpawnDefinition spawn,
                    out _)
                && spawn != null
                && session.TryResolveCombatant(spawn.SpawnId);
            ApplyCurrentView();
            return changed;
        }

        public bool AdvanceWave()
        {
            bool changed = session != null && session.TryAdvanceWave();
            ApplyCurrentView();
            return changed;
        }

        public bool InterruptReview()
        {
            bool changed = session != null && session.TryInterrupt();
            ApplyCurrentView();
            return changed;
        }

        public bool ResetReview()
        {
            if (session == null || session.State == StageEncounterPlanReviewState.Ready)
            {
                ApplyCurrentView();
                return false;
            }

            session.Reset();
            ApplyCurrentView();
            return true;
        }

        public void RefreshCurrentView()
        {
            ApplyCurrentView();
        }

        private void BindInteractions()
        {
            if (interactionsBound)
            {
                return;
            }

            AddButtonListener(beginButton, HandleBeginClicked);
            AddButtonListener(resolveButton, HandleResolveClicked);
            AddButtonListener(advanceButton, HandleAdvanceClicked);
            AddButtonListener(interruptButton, HandleInterruptClicked);
            AddButtonListener(resetButton, HandleResetClicked);
            interactionsBound = true;
        }

        private void UnbindInteractions()
        {
            if (!interactionsBound)
            {
                return;
            }

            RemoveButtonListener(beginButton, HandleBeginClicked);
            RemoveButtonListener(resolveButton, HandleResolveClicked);
            RemoveButtonListener(advanceButton, HandleAdvanceClicked);
            RemoveButtonListener(interruptButton, HandleInterruptClicked);
            RemoveButtonListener(resetButton, HandleResetClicked);
            interactionsBound = false;
        }

        private bool BeginRuntimeReconfiguration()
        {
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            return rebind;
        }

        private void EndRuntimeReconfiguration(bool rebind)
        {
            if (rebind && Application.isPlaying && isActiveAndEnabled)
            {
                BindInteractions();
            }
        }

        private void HandleBeginClicked()
        {
            BeginEncounter();
        }

        private void HandleResolveClicked()
        {
            ResolveCurrentCombatant();
        }

        private void HandleAdvanceClicked()
        {
            AdvanceWave();
        }

        private void HandleInterruptClicked()
        {
            InterruptReview();
        }

        private void HandleResetClicked()
        {
            ResetReview();
        }

        private void ApplyCurrentView()
        {
            if (session == null)
            {
                RenderUnavailable();
                return;
            }

            SetText(
                admissionBoundaryText,
                $"ADMISSION {session.AdmissionDisposition} / LOCAL REVIEW SURFACE");
            SetText(titleText, ReviewDisplayTitle);
            SetText(
                identityText,
                $"PLAN {session.PlanId} / STAGE {session.StageId}\n"
                + $"ENCOUNTER {session.EncounterId} / REVISION {session.Revision}");

            StageEncounterPlanProfile.WaveDefinition firstWave =
                session.WaveCount > 0 ? session.GetWave(0) : null;
            SetText(
                objectiveText,
                firstWave != null
                    ? $"OBJECTIVE {firstWave.Objective} / {session.WaveCount} WAVES"
                    : "OBJECTIVE UNAVAILABLE");
            SetText(stateText, $"STATE {session.State}");
            SetText(
                progressText,
                $"WAVES {session.ClearedWaveCount:00} / {session.WaveCount:00}"
                + $" / REMAINING {session.CurrentRemainingCombatantCount:00}");
            SetText(currentSpawnText, BuildCurrentSpawnText());
            SetText(
                ownershipBoundaryText,
                $"EXTERNAL OWNERSHIP / OUTCOME {session.OutcomeOwner}"
                + $" / REWARD {session.RewardOwner}\n"
                + "NO STAGE ADMISSION, OUTCOME, REWARD, SAVE, OR ROUTE DISPATCH");

            RenderWaveCards();
            RefreshActionAvailability();
        }

        private void RenderUnavailable()
        {
            SetText(admissionBoundaryText, ReviewUnavailableStatus);
            SetText(titleText, ReviewDisplayTitle);
            SetText(identityText, string.Empty);
            SetText(objectiveText, "OBJECTIVE UNAVAILABLE");
            SetText(stateText, "STATE UNAVAILABLE");
            SetText(progressText, "WAVES -- / -- / REMAINING --");
            SetText(currentSpawnText, "CURRENT SPAWN --");
            SetText(
                ownershipBoundaryText,
                "EXTERNAL OWNERSHIP UNAVAILABLE / NO DISPATCH");

            for (int index = 0; index < RequiredWaveCardCount; index++)
            {
                SetText(GetAt(waveTitleTexts, index), "WAVE");
                SetText(GetAt(waveStateTexts, index), "UNAVAILABLE");
                SetText(GetAt(waveDetailTexts, index), string.Empty);
                SetAccent(GetAt(waveAccentImages, index), UnavailableAccent);
            }

            DisableAllActions();
        }

        private string BuildCurrentSpawnText()
        {
            if (session == null)
            {
                return "CURRENT SPAWN --";
            }

            if (session.TryGetNextUnresolvedSpawn(
                    out StageEncounterPlanProfile.SpawnDefinition spawn,
                    out int remainingCount)
                && spawn != null)
            {
                return $"SPAWN {spawn.SpawnId} / PAYLOAD {spawn.PayloadId}"
                    + $" / ANCHOR {spawn.AnchorId}\n"
                    + $"REMAINING {remainingCount:00} / DELAY "
                    + spawn.DelaySeconds.ToString("0.##", CultureInfo.InvariantCulture)
                    + "s";
            }

            return session.State switch
            {
                StageEncounterPlanReviewState.Ready => "CURRENT SPAWN -- / READY",
                StageEncounterPlanReviewState.WaveTransition =>
                    "CURRENT SPAWN CLEARED / ADVANCE AVAILABLE",
                StageEncounterPlanReviewState.Completed =>
                    "CURRENT SPAWN NONE / REVIEW COMPLETE",
                StageEncounterPlanReviewState.Interrupted =>
                    "CURRENT SPAWN NONE / REVIEW INTERRUPTED",
                _ => "CURRENT SPAWN UNAVAILABLE"
            };
        }

        private void RenderWaveCards()
        {
            for (int index = 0; index < RequiredWaveCardCount; index++)
            {
                StageEncounterPlanProfile.WaveDefinition wave = session.GetWave(index);
                StageEncounterWaveReviewStatus status = session.GetWaveStatus(index);
                SetText(GetAt(waveTitleTexts, index), "WAVE");
                SetText(
                    GetAt(waveStateTexts, index),
                    status.ToString().ToUpperInvariant());
                SetText(
                    GetAt(waveDetailTexts, index),
                    $"ID {wave.WaveId}\n"
                    + $"{wave.Activation} / {wave.Objective}\n"
                    + $"SPAWNS {wave.SpawnCount:00} / "
                    + $"COMBATANTS {wave.TotalCombatantCount:00}");
                SetAccent(GetAt(waveAccentImages, index), ResolveAccent(status));
            }
        }

        private void RefreshActionAvailability()
        {
            bool canResolve = session.State == StageEncounterPlanReviewState.WaveActive
                && session.TryGetNextUnresolvedSpawn(out _, out _);
            SetButtonInteractable(
                beginButton,
                session.State == StageEncounterPlanReviewState.Ready);
            SetButtonInteractable(resolveButton, canResolve);
            SetButtonInteractable(
                advanceButton,
                session.State == StageEncounterPlanReviewState.WaveTransition);
            SetButtonInteractable(
                interruptButton,
                session.State == StageEncounterPlanReviewState.WaveActive
                || session.State == StageEncounterPlanReviewState.WaveTransition);
            SetButtonInteractable(
                resetButton,
                session.State != StageEncounterPlanReviewState.Ready);
        }

        private void DisableAllActions()
        {
            SetButtonInteractable(beginButton, false);
            SetButtonInteractable(resolveButton, false);
            SetButtonInteractable(advanceButton, false);
            SetButtonInteractable(interruptButton, false);
            SetButtonInteractable(resetButton, false);
        }

        private static Color ResolveAccent(StageEncounterWaveReviewStatus status)
        {
            return status switch
            {
                StageEncounterWaveReviewStatus.Active => ActiveAccent,
                StageEncounterWaveReviewStatus.Cleared => ClearedAccent,
                StageEncounterWaveReviewStatus.Interrupted => InterruptedAccent,
                _ => PendingAccent
            };
        }

        private static bool HasRequiredLength<T>(T[] values)
        {
            return values != null && values.Length == RequiredWaveCardCount;
        }

        private static T GetAt<T>(T[] values, int index) where T : UnityEngine.Object
        {
            return values != null && index >= 0 && index < values.Length
                ? values[index]
                : null;
        }

        private static void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveButtonListener(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void SetAccent(Image target, Color color)
        {
            if (target != null)
            {
                target.color = color;
            }
        }
    }
}
