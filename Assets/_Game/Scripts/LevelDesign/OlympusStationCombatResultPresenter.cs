using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusStationCombatResultPresenter : MonoBehaviour
    {
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private OlympusStageClearOverlay stageClearOverlay;
        [SerializeField] private MonoBehaviour resultSurfaceBehaviour;
        [SerializeField] private OlympusStationRunFactCollector factCollector;

        private CombatEncounterController subscribedEncounter;
        private bool subscribedCanonicalTerminal;
        private bool hasStarted;
        private Coroutine commitRecoveryRoutine;
        private ICombatSessionOverlay ResultSurface => resultSurfaceBehaviour as ICombatSessionOverlay;

        public bool HasCanonicalStageRun { get; private set; }
        public string CanonicalStageRunEntryError { get; private set; } = string.Empty;
        public StageRunResultSummary CommittedSummary { get; private set; }
        public StageRunResultCommitReceipt CommitReceipt { get; private set; }
        public int CommitRecoveryAttemptCount { get; private set; }
        public string LastCommitError { get; private set; } = string.Empty;

        private void OnEnable()
        {
            if (hasStarted)
            {
                TryEnterCanonicalStageRun();
            }

            if (hasStarted || encounter == null || !encounter.UsesCoordinatedTerminalResolution)
            {
                SubscribeEncounter();
            }
        }

        private void Start()
        {
            hasStarted = true;
            TryEnterCanonicalStageRun();
            SubscribeEncounter();
        }

        private void OnDisable()
        {
            if (hasStarted
                && HasCanonicalStageRun
                && StageRunRuntime.TryAbortFromStationAdapterLoss(
                    this,
                    encounter,
                    StageRunAbortReason.StationResultPresenterLost,
                    out _,
                    out _))
            {
                HasCanonicalStageRun = false;
            }

            if (commitRecoveryRoutine != null)
            {
                StopCoroutine(commitRecoveryRoutine);
                commitRecoveryRoutine = null;
            }

            UnsubscribeEncounter();
        }

        private void SubscribeEncounter()
        {
            bool useCanonicalTerminal = encounter != null
                && encounter.UsesCoordinatedTerminalResolution
                && HasCanonicalStageRun;
            if (subscribedEncounter == encounter
                && subscribedCanonicalTerminal == useCanonicalTerminal)
            {
                return;
            }

            UnsubscribeEncounter();
            if (encounter == null || stageClearOverlay == null || ResultSurface == null)
            {
                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Missing authored encounter, result surface, or stage-clear overlay.",
                    this);
                return;
            }

            if (encounter.UsesCoordinatedTerminalResolution && !useCanonicalTerminal)
            {
                return;
            }

            subscribedEncounter = encounter;
            subscribedCanonicalTerminal = useCanonicalTerminal;
            if (subscribedCanonicalTerminal)
            {
                subscribedEncounter.TerminalResolved += HandleCanonicalTerminalResolved;
                subscribedEncounter.DiagnosticAborted += HandleCanonicalDiagnosticAborted;
                if (subscribedEncounter.HasDiagnostic)
                {
                    HandleCanonicalDiagnosticAborted(subscribedEncounter.Diagnostic);
                }
                else if (subscribedEncounter.HasTerminalResolution)
                {
                    HandleCanonicalTerminalResolved(subscribedEncounter.TerminalResolution);
                }

                return;
            }

            subscribedEncounter.Won += HandleEncounterWon;
            subscribedEncounter.Failed += HandleEncounterFailed;
            if (subscribedEncounter.IsWon)
            {
                HandleEncounterWon();
            }
            else if (subscribedEncounter.IsFailed)
            {
                HandleEncounterFailed();
            }
        }

        private void UnsubscribeEncounter()
        {
            if (subscribedEncounter == null)
            {
                return;
            }

            subscribedEncounter.TerminalResolved -= HandleCanonicalTerminalResolved;
            subscribedEncounter.DiagnosticAborted -= HandleCanonicalDiagnosticAborted;
            subscribedEncounter.Won -= HandleEncounterWon;
            subscribedEncounter.Failed -= HandleEncounterFailed;
            subscribedEncounter = null;
            subscribedCanonicalTerminal = false;
        }

        private void TryEnterCanonicalStageRun()
        {
            HasCanonicalStageRun = StageRunRuntime.TryEnterPendingSegment(
                gameObject.scene,
                out _,
                out string entryError);
            if (HasCanonicalStageRun)
            {
                factCollector ??= GetComponent<OlympusStationRunFactCollector>();
                if (factCollector == null || !factCollector.BindToActiveRun())
                {
                    entryError = factCollector != null
                        ? factCollector.LastFactError
                        : "Station run fact collector is missing.";
                }
            }

            CanonicalStageRunEntryError = entryError;
        }

        private void HandleEncounterWon()
        {
            ResultSurface?.DismissForStageClear();
            stageClearOverlay.Show();
        }

        private void HandleEncounterFailed()
        {
            ResultSurface?.ShowFailure();
        }

        private void HandleCanonicalTerminalResolved(EncounterTerminalResolution resolution)
        {
            if (factCollector == null || !factCollector.PrepareForTerminal())
            {
                LastCommitError = factCollector != null
                    ? factCollector.LastFactError
                    : "Station run fact collector is missing.";
                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Canonical terminal fact seal preparation rejected: {LastCommitError}",
                    this);
                return;
            }

            if (!StageRunRuntime.TryCommitTerminalResolution(
                encounter,
                resolution,
                out StageRunResultSummary summary,
                out StageRunResultCommitReceipt receipt,
                out string error))
            {
                LastCommitError = error;
                if (StageRunRuntime.ActiveContext?.LifecycleState
                    == StageRunLifecycleState.CommitRecoveryPending
                    && commitRecoveryRoutine == null
                    && isActiveAndEnabled)
                {
                    commitRecoveryRoutine = StartCoroutine(RecoverCommitRoutine());
                    return;
                }

                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Canonical terminal commit rejected: {error}",
                    this);
                return;
            }

            PublishCommittedResult(summary, receipt);
        }

        private void HandleCanonicalDiagnosticAborted(EncounterTerminalDiagnostic diagnostic)
        {
            LastCommitError = diagnostic.Message;
            if (!StageRunRuntime.TryAbortFromCoordinatorDiagnostic(
                    encounter,
                    diagnostic,
                    out _,
                    out string error))
            {
                LastCommitError = error;
                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Canonical diagnostic abort rejected: {error}",
                    this);
                return;
            }

            HasCanonicalStageRun = false;
        }

        private IEnumerator RecoverCommitRoutine()
        {
            const int maxAttempts = 5;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CommitRecoveryAttemptCount++;
                yield return new WaitForSecondsRealtime(0.1f * (attempt + 1));
                if (StageRunRuntime.TryRecoverPendingResultCommit(
                    out StageRunResultSummary summary,
                    out StageRunResultCommitReceipt receipt,
                    out string error))
                {
                    commitRecoveryRoutine = null;
                    PublishCommittedResult(summary, receipt);
                    yield break;
                }

                LastCommitError = error;
                if (StageRunRuntime.ActiveContext?.LifecycleState
                    != StageRunLifecycleState.CommitRecoveryPending)
                {
                    break;
                }
            }

            Debug.LogError(
                $"[{nameof(OlympusStationCombatResultPresenter)}] Durable result commit remains unavailable: {LastCommitError}",
                this);
            commitRecoveryRoutine = null;
        }

        private void PublishCommittedResult(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            LastCommitError = string.Empty;
            CommittedSummary = summary;
            CommitReceipt = receipt;
            ResultSurface?.DismissForStageClear();
            stageClearOverlay.Show(summary);
        }
    }
}
