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
        [SerializeField] private OlympusStationBossTerminalAftermathPresenter bossTerminalAftermath;
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
        public string LastPresentationWarning { get; private set; } = string.Empty;

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
            CancelAftermathIfOwned("The Station result presenter was disabled before result handoff completed.");
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
            if (encounter == null
                || stageClearOverlay == null
                || ResultSurface == null
                || (useCanonicalTerminal && bossTerminalAftermath == null))
            {
                CancelAftermathIfOwned("The Station result presenter is missing an authored result dependency.");
                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Missing authored encounter, result surface, stage-clear overlay, or canonical aftermath gate.",
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
                    CancelAftermathIfOwned(
                        "The Station run fact collector could not bind to the canonical run: "
                        + entryError);
                }
            }

            CanonicalStageRunEntryError = entryError;
        }

        private void HandleEncounterWon()
        {
            try
            {
                ResultSurface?.DismissForStageClear();
                string error = stageClearOverlay != null
                    ? string.Empty
                    : "The legacy Station stage-clear overlay is missing.";
                bool shown = stageClearOverlay != null
                    && stageClearOverlay.TryShow(null, out error);
                if (!shown)
                {
                    CancelAftermathIfOwned(
                        "The legacy Station result overlay rejected clear presentation: " + error);
                }
            }
            catch (System.Exception exception)
            {
                CancelAftermathNoThrow(
                    "The legacy Station result presentation threw safely: " + exception.Message,
                    exception);
            }
        }

        private void HandleEncounterFailed()
        {
            try
            {
                ResultSurface?.ShowFailure();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void HandleCanonicalTerminalResolved(EncounterTerminalResolution resolution)
        {
            try
            {
                HandleCanonicalTerminalResolvedCore(resolution);
            }
            catch (System.Exception exception)
            {
                CancelAftermathNoThrow(
                    "The canonical terminal result callback threw safely: " + exception.Message,
                    exception);
            }
        }

        private void HandleCanonicalTerminalResolvedCore(EncounterTerminalResolution resolution)
        {
            if (factCollector == null || !factCollector.PrepareForTerminal())
            {
                LastCommitError = factCollector != null
                    ? factCollector.LastFactError
                    : "Station run fact collector is missing.";
                CancelAftermathIfOwned(
                    "Canonical terminal fact preparation failed: " + LastCommitError);
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

                CancelAftermathIfOwned("Canonical terminal result commit failed: " + error);
                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Canonical terminal commit rejected: {error}",
                    this);
                return;
            }

            PublishCommittedResult(summary, receipt);
        }

        private void HandleCanonicalDiagnosticAborted(EncounterTerminalDiagnostic diagnostic)
        {
            try
            {
                HandleCanonicalDiagnosticAbortedCore(diagnostic);
            }
            catch (System.Exception exception)
            {
                CancelAftermathNoThrow(
                    "The canonical terminal diagnostic callback threw safely: "
                    + exception.Message,
                    exception);
            }
        }

        private void HandleCanonicalDiagnosticAbortedCore(EncounterTerminalDiagnostic diagnostic)
        {
            LastCommitError = diagnostic.Message;
            CancelAftermathIfOwned(
                "The canonical terminal coordinator aborted before result handoff: "
                + diagnostic.Message);
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
                StageRunResultSummary summary;
                StageRunResultCommitReceipt receipt;
                string error;
                bool recovered;
                try
                {
                    recovered = StageRunRuntime.TryRecoverPendingResultCommit(
                        out summary,
                        out receipt,
                        out error);
                }
                catch (System.Exception exception)
                {
                    LastCommitError =
                        "Durable result commit recovery threw safely: " + exception.Message;
                    CancelAftermathIfOwned(LastCommitError);
                    commitRecoveryRoutine = null;
                    Debug.LogException(exception, this);
                    yield break;
                }

                if (recovered)
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
            CancelAftermathIfOwned(
                "Durable result commit recovery was exhausted: " + LastCommitError);
            commitRecoveryRoutine = null;
        }

        private void PublishCommittedResult(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            try
            {
                PublishCommittedResultCore(summary, receipt);
            }
            catch (System.Exception exception)
            {
                CancelAftermathNoThrow(
                    "The durable canonical result presentation threw safely: " + exception.Message,
                    exception);
            }
        }

        private void PublishCommittedResultCore(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            // Seal durable truth before invoking presentation collaborators. A
            // later UI fault must never make a successful commit look absent.
            LastCommitError = string.Empty;
            LastPresentationWarning = string.Empty;
            CommittedSummary = summary;
            CommitReceipt = receipt;

            try
            {
                ResultSurface?.DismissForStageClear();
            }
            catch (System.Exception exception)
            {
                LastPresentationWarning =
                    "The combat result surface could not dismiss, but the durable result remains committed: "
                    + exception.Message;
                Debug.LogException(exception, this);
            }

            string presentationError = stageClearOverlay != null
                ? string.Empty
                : "The canonical Station stage-clear overlay is missing.";
            bool shown = stageClearOverlay != null
                && bossTerminalAftermath != null
                && stageClearOverlay.TryShow(summary, out presentationError);
            if (shown)
            {
                return;
            }

            LastCommitError =
                "The committed result could not attach to the authored result overlay: "
                + presentationError;
            CancelAftermathIfOwned(LastCommitError);
            Debug.LogError(
                $"[{nameof(OlympusStationCombatResultPresenter)}] {LastCommitError}",
                this);
        }

        private void CancelAftermathIfOwned(string reason)
        {
            if (bossTerminalAftermath != null
                && bossTerminalAftermath.IsStarted
                && (bossTerminalAftermath.IsRunning || bossTerminalAftermath.InputLeaseActive))
            {
                bossTerminalAftermath.CancelAndRelease(reason);
            }
        }

        private void CancelAftermathNoThrow(string reason, System.Exception exception)
        {
            try
            {
                LastCommitError = reason;
                CancelAftermathIfOwned(reason);
                Debug.LogException(exception, this);
            }
            catch (System.Exception cleanupException)
            {
                try
                {
                    Debug.LogException(cleanupException, this);
                }
                catch
                {
                    // Never escape into CombatEncounterController terminal mutation.
                }
            }
        }
    }
}
