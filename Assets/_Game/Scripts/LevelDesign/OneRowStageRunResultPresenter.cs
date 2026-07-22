using System;
using DimensionBrawl.Combat;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(9250)]
    [DisallowMultipleComponent]
    public sealed class OneRowStageRunResultPresenter : MonoBehaviour, ITerminalStageRunAdapterLossOwner
    {
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private MonoBehaviour resultOverlayBehaviour;
        [SerializeField] private MonoBehaviour resultSurfaceBehaviour;
        [SerializeField] private OneRowStageRunFactAdapter factAdapter;

        private CombatEncounterController subscribedEncounter;
        private bool hasStarted;
        private Coroutine presentationRetryRoutine;
        private string presentationRequestedDigest = string.Empty;
        private string presentedResultDigest = string.Empty;
        private string dismissedResultDigest = string.Empty;
        private string presentationRetryRunId = string.Empty;
        private string presentationRetryDigest = string.Empty;
        private int presentationRetryCount;

        private IStageRunResultOverlay ResultOverlay => resultOverlayBehaviour as IStageRunResultOverlay;
        private ICombatSessionOverlay ResultSurface => resultSurfaceBehaviour as ICombatSessionOverlay;

        StageRunAbortReason ITerminalStageRunAdapterLossOwner.AdapterLossReason =>
            StageRunAbortReason.TerminalResultPresenterLost;

        public bool HasCanonicalStageRun { get; private set; }
        public string CanonicalStageRunBindingError { get; private set; } = string.Empty;
        public StageRunResultSummary CommittedSummary { get; private set; }
        public StageRunResultCommitReceipt CommitReceipt { get; private set; }
        public int CommitRecoveryAttemptCount { get; private set; }
        public string LastCommitError { get; private set; } = string.Empty;

        private void OnEnable()
        {
            StageRunCommitRecoveryPump.Attempted += HandleRecoveryAttempted;
            StageRunCommitRecoveryPump.Recovered += HandleCommitRecovered;
            StageRunCommitRecoveryPump.RecoveryDelayed += HandleRecoveryDelayed;
            if (ResultOverlay != null)
            {
                ResultOverlay.PresentationSucceeded += HandlePresentationSucceeded;
                ResultOverlay.PresentationFailed += HandlePresentationFailed;
                if (!string.IsNullOrEmpty(presentationRequestedDigest)
                    && !string.Equals(
                        ResultOverlay.PendingResultDigest,
                        presentationRequestedDigest,
                        StringComparison.Ordinal)
                    && !string.Equals(
                        ResultOverlay.PresentedResultDigest,
                        presentationRequestedDigest,
                        StringComparison.Ordinal))
                {
                    presentationRequestedDigest = string.Empty;
                }
            }
            if (!hasStarted)
            {
                return;
            }

            BindToActiveRun();
            SubscribeEncounter();
            ResumePendingCommitOrPublish();
        }

        private void Start()
        {
            hasStarted = true;
            BindToActiveRun();
            SubscribeEncounter();
            ResumePendingCommitOrPublish();
        }

        private void OnDisable()
        {
            if (HasCanonicalStageRun
                && StageRunRuntime.TryAbortFromTerminalAdapterLoss(
                    this,
                    encounter,
                    StageRunAbortReason.TerminalResultPresenterLost,
                    out _,
                    out _))
            {
                HasCanonicalStageRun = false;
            }

            UnsubscribeEncounter();
            presentationRequestedDigest = string.Empty;
            if (presentationRetryRoutine != null)
            {
                StopCoroutine(presentationRetryRoutine);
                presentationRetryRoutine = null;
            }
            presentationRetryRunId = string.Empty;
            presentationRetryDigest = string.Empty;

            if (ResultOverlay != null)
            {
                ResultOverlay.PresentationSucceeded -= HandlePresentationSucceeded;
                ResultOverlay.PresentationFailed -= HandlePresentationFailed;
            }

            StageRunCommitRecoveryPump.Attempted -= HandleRecoveryAttempted;
            StageRunCommitRecoveryPump.Recovered -= HandleCommitRecovered;
            StageRunCommitRecoveryPump.RecoveryDelayed -= HandleRecoveryDelayed;
        }

        private void Update()
        {
            if (!HasCanonicalStageRun)
            {
                return;
            }

            StageRunContext context = StageRunRuntime.ActiveContext;
            if (context == null
                || context.CurrentSceneHandle != gameObject.scene.handle
                || !context.IsTerminalStageAdapterOwner(
                    this,
                    TerminalStageRunAdapterRole.ResultPresentation))
            {
                HasCanonicalStageRun = false;
                UnsubscribeEncounter();
                return;
            }

            if (context.LifecycleState != StageRunLifecycleState.StationActive)
            {
                return;
            }

            bool factBindingLive = factAdapter != null
                && factAdapter.isActiveAndEnabled
                && factAdapter.IsBound;
            string error = string.Empty;
            if (factBindingLive && TryValidateReferences(out error))
            {
                return;
            }

            CanonicalStageRunBindingError = factBindingLive
                ? error
                : "The one-row result presenter lost its exact terminal fact adapter.";
            StageRunRuntime.TryAbortFromTerminalAdapterLoss(
                this,
                encounter,
                StageRunAbortReason.TerminalResultPresenterLost,
                out _,
                out _);
            HasCanonicalStageRun = false;
            UnsubscribeEncounter();
        }

        public bool BindToActiveRun()
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (!OneRowStageRunAdapterContract.TryValidateContext(
                context,
                gameObject.scene.handle,
                out string error))
            {
                HasCanonicalStageRun = false;
                CanonicalStageRunBindingError = error;
                return false;
            }

            bool supportedLifecycle = context.LifecycleState == StageRunLifecycleState.StationActive
                || context.LifecycleState == StageRunLifecycleState.CommitRecoveryPending
                || context.LifecycleState == StageRunLifecycleState.Committed
                || context.LifecycleState == StageRunLifecycleState.Presented;
            if (!supportedLifecycle || !TryValidateReferences(out error))
            {
                HasCanonicalStageRun = false;
                CanonicalStageRunBindingError = string.IsNullOrWhiteSpace(error)
                    ? $"One-row result presentation cannot bind from {context.LifecycleState}."
                    : error;
                return false;
            }

            if (context.LifecycleState == StageRunLifecycleState.StationActive
                && (factAdapter == null || !factAdapter.BindToActiveRun()))
            {
                HasCanonicalStageRun = false;
                CanonicalStageRunBindingError = factAdapter != null
                    ? factAdapter.LastFactError
                    : "The one-row result presenter has no terminal fact adapter.";
                return false;
            }

            if (!context.TryBindTerminalStageAdapter(
                this,
                TerminalStageRunAdapterRole.ResultPresentation,
                out error))
            {
                HasCanonicalStageRun = false;
                CanonicalStageRunBindingError = error;
                return false;
            }

            HasCanonicalStageRun = true;
            CanonicalStageRunBindingError = string.Empty;
            return true;
        }

        internal bool TryValidateAuthoring(
            CombatEncounterController expectedEncounter,
            OneRowStageRunFactAdapter expectedFactAdapter,
            out string error)
        {
            if (!ReferenceEquals(encounter, expectedEncounter)
                || !ReferenceEquals(factAdapter, expectedFactAdapter)
                || !expectedFactAdapter.UsesResultSurface(resultSurfaceBehaviour))
            {
                error =
                    "The one-row result presenter does not reference the bootstrap encounter and fact adapter.";
                return false;
            }

            return TryValidateReferences(out error);
        }

        private bool TryValidateReferences(out string error)
        {
            error = string.Empty;
            if (encounter == null
                || encounter.gameObject.scene.handle != gameObject.scene.handle
                || !encounter.UsesCoordinatedTerminalResolution
                || resultOverlayBehaviour == null
                || !resultOverlayBehaviour.isActiveAndEnabled
                || resultOverlayBehaviour.gameObject.scene.handle != gameObject.scene.handle
                || resultSurfaceBehaviour == null
                || !resultSurfaceBehaviour.isActiveAndEnabled
                || resultSurfaceBehaviour.gameObject.scene.handle != gameObject.scene.handle
                || ResultOverlay == null
                || ResultSurface == null)
            {
                error =
                    "A one-row result presenter requires a same-scene coordinated encounter, result overlay, and combat-session surface.";
                return false;
            }

            return true;
        }

        private void SubscribeEncounter()
        {
            UnsubscribeEncounter();
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (!HasCanonicalStageRun
                || context == null
                || context.LifecycleState != StageRunLifecycleState.StationActive
                || encounter == null)
            {
                return;
            }

            subscribedEncounter = encounter;
            subscribedEncounter.TerminalResolved += HandleTerminalResolved;
            subscribedEncounter.DiagnosticAborted += HandleDiagnosticAborted;
            if (subscribedEncounter.HasDiagnostic)
            {
                HandleDiagnosticAborted(subscribedEncounter.Diagnostic);
            }
            else if (subscribedEncounter.HasTerminalResolution)
            {
                HandleTerminalResolved(subscribedEncounter.TerminalResolution);
            }
        }

        private void UnsubscribeEncounter()
        {
            if (subscribedEncounter == null)
            {
                return;
            }

            subscribedEncounter.TerminalResolved -= HandleTerminalResolved;
            subscribedEncounter.DiagnosticAborted -= HandleDiagnosticAborted;
            subscribedEncounter = null;
        }

        private void HandleTerminalResolved(EncounterTerminalResolution resolution)
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (context == null
                || !context.CanAbortBeforeCommit()
                || !OwnsRun(context.Identity.RunId))
            {
                return;
            }

            string presenterError = string.Empty;
            if (!isActiveAndEnabled
                || !context.IsTerminalStageAdapterOwner(
                    this,
                    TerminalStageRunAdapterRole.ResultPresentation)
                || !TryValidateReferences(out presenterError))
            {
                AbortResolvedAdapterFailure(
                    this,
                    StageRunAbortReason.TerminalResultPresenterLost,
                    resolution,
                    string.IsNullOrWhiteSpace(presenterError)
                        ? "The exact one-row result presenter was not live at terminal resolution."
                        : presenterError);
                return;
            }

            if (factAdapter == null || !factAdapter.PrepareForTerminal())
            {
                string factError = factAdapter != null
                    ? factAdapter.LastFactError
                    : "The one-row terminal fact adapter is missing.";
                AbortResolvedAdapterFailure(
                    factAdapter,
                    StageRunAbortReason.TerminalFactAdapterLost,
                    resolution,
                    factError);
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
                    == StageRunLifecycleState.CommitRecoveryPending)
                {
                    StartCommitRecoveryIfNeeded();
                    return;
                }

                Debug.LogError(
                    $"[{nameof(OneRowStageRunResultPresenter)}] Terminal commit rejected: {error}",
                    this);
                return;
            }

            PublishCommittedResult(summary, receipt);
        }

        private void AbortResolvedAdapterFailure(
            Component adapter,
            StageRunAbortReason reason,
            EncounterTerminalResolution resolution,
            string failure)
        {
            LastCommitError = failure ?? string.Empty;
            if (!StageRunRuntime.TryAbortFromResolvedTerminalAdapterFailure(
                    adapter,
                    encounter,
                    resolution,
                    reason,
                    out _,
                    out string abortError))
            {
                LastCommitError = string.IsNullOrWhiteSpace(abortError)
                    ? LastCommitError
                    : $"{LastCommitError} Abort closure failed: {abortError}";
                Debug.LogError(
                    $"[{nameof(OneRowStageRunResultPresenter)}] {LastCommitError}",
                    this);
            }

            HasCanonicalStageRun = false;
            UnsubscribeEncounter();
        }

        private void HandleDiagnosticAborted(EncounterTerminalDiagnostic diagnostic)
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (context == null
                || !context.CanAbortBeforeCommit()
                || !OwnsRun(context.Identity.RunId))
            {
                return;
            }

            LastCommitError = diagnostic.Message;
            if (!StageRunRuntime.TryAbortFromCoordinatorDiagnostic(
                encounter,
                diagnostic,
                out _,
                out string error))
            {
                LastCommitError = error;
                Debug.LogError(
                    $"[{nameof(OneRowStageRunResultPresenter)}] Coordinator diagnostic abort rejected: {error}",
                    this);
                return;
            }

            HasCanonicalStageRun = false;
        }

        private void ResumePendingCommitOrPublish()
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (!HasCanonicalStageRun || context == null)
            {
                return;
            }

            if (context.LifecycleState == StageRunLifecycleState.CommitRecoveryPending)
            {
                StartCommitRecoveryIfNeeded();
            }
            else if ((context.LifecycleState == StageRunLifecycleState.Committed
                    || context.LifecycleState == StageRunLifecycleState.Presented)
                && context.CommittedSummary != null
                && context.CommitReceipt != null)
            {
                PublishCommittedResult(context.CommittedSummary, context.CommitReceipt);
            }
        }

        private void StartCommitRecoveryIfNeeded()
        {
            StageRunCommitRecoveryPump.RequestRecovery(this);
        }

        internal void PublishRecoveredResult(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            if (summary != null && OwnsRun(summary.Identity.RunId))
            {
                PublishCommittedResult(summary, receipt);
            }
        }

        private void HandleRecoveryAttempted(string runId, int attempt)
        {
            if (OwnsRun(runId))
            {
                CommitRecoveryAttemptCount = Math.Max(CommitRecoveryAttemptCount, attempt);
            }
        }

        private void HandleCommitRecovered(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            if (summary != null && OwnsRun(summary.Identity.RunId))
            {
                PublishRecoveredResult(summary, receipt);
            }
        }

        private void HandleRecoveryDelayed(string runId, string error)
        {
            if (!OwnsRun(runId))
            {
                return;
            }

            LastCommitError = error ?? string.Empty;
            Debug.LogWarning(
                $"[{nameof(OneRowStageRunResultPresenter)}] Durable result commit is still pending; low-frequency recovery remains active: {LastCommitError}",
                this);
        }

        private bool OwnsRun(string runId)
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            return context != null
                && context.CurrentSceneHandle == gameObject.scene.handle
                && context.IsTerminalStageAdapterOwner(
                    this,
                    TerminalStageRunAdapterRole.ResultPresentation)
                && string.Equals(context.Identity.RunId, runId, StringComparison.Ordinal);
        }

        private void PublishCommittedResult(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            LastCommitError = string.Empty;
            CommittedSummary = summary;
            CommitReceipt = receipt;
            if (summary == null)
            {
                return;
            }

            try
            {
                string digest = summary.ResultSummaryDigest;
                if (string.Equals(
                    ResultOverlay?.PresentedResultDigest,
                    digest,
                    StringComparison.Ordinal)
                    && IsCanonicalPresentationAcknowledged(summary))
                {
                    presentedResultDigest = digest;
                    presentationRequestedDigest = string.Empty;
                    return;
                }

                if (string.Equals(presentedResultDigest, digest, StringComparison.Ordinal))
                {
                    return;
                }

                if (string.Equals(presentationRequestedDigest, digest, StringComparison.Ordinal))
                {
                    if (string.Equals(
                        ResultOverlay?.PendingResultDigest,
                        digest,
                        StringComparison.Ordinal))
                    {
                        SchedulePresentationRetry();
                        return;
                    }

                    presentationRequestedDigest = string.Empty;
                }

                if (!string.Equals(dismissedResultDigest, digest, StringComparison.Ordinal))
                {
                    ResultSurface?.DismissForStageClear();
                    dismissedResultDigest = digest;
                }

                string error = string.Empty;
                IStageRunResultOverlay overlay = ResultOverlay;
                if (overlay == null || !overlay.TryShow(summary, out error))
                {
                    LastCommitError = string.IsNullOrWhiteSpace(error)
                        ? "The result overlay rejected the committed summary."
                        : error;
                    SchedulePresentationRetry();
                    return;
                }

                presentationRequestedDigest = digest;
                if (string.Equals(
                    ResultOverlay.PresentedResultDigest,
                    digest,
                    StringComparison.Ordinal))
                {
                    HandlePresentationSucceeded(summary);
                    return;
                }

                SchedulePresentationRetry();
            }
            catch (Exception exception)
            {
                LastCommitError = $"Result presentation boundary threw: {exception.Message}";
                SchedulePresentationRetry();
            }
        }

        private void HandlePresentationSucceeded(StageRunResultSummary summary)
        {
            if (summary == null
                || !OwnsRun(summary.Identity.RunId)
                || !string.Equals(
                    ResultOverlay?.PresentedResultDigest,
                    summary.ResultSummaryDigest,
                    StringComparison.Ordinal)
                || !IsCanonicalPresentationAcknowledged(summary))
            {
                LastCommitError =
                    "The result overlay acknowledged before the canonical result presentation was sealed.";
                SchedulePresentationRetry();
                return;
            }

            presentedResultDigest = summary.ResultSummaryDigest;
            presentationRequestedDigest = string.Empty;
            presentationRetryCount = 0;
            LastCommitError = string.Empty;
            CancelPresentationRetry();
        }

        private static bool IsCanonicalPresentationAcknowledged(StageRunResultSummary summary)
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            return summary != null
                && context != null
                && context.LifecycleState == StageRunLifecycleState.Presented
                && ReferenceEquals(context.CommittedSummary, summary)
                && context.ResultPresentationSnapshot != null
                && context.ResultPresentationAudit != null;
        }

        private void HandlePresentationFailed(StageRunResultSummary summary, string error)
        {
            if (summary == null || !OwnsRun(summary.Identity.RunId))
            {
                return;
            }

            presentationRequestedDigest = string.Empty;
            LastCommitError = error ?? string.Empty;
            SchedulePresentationRetry();
        }

        private void SchedulePresentationRetry()
        {
            StageRunResultSummary summary = CommittedSummary;
            if (!isActiveAndEnabled
                || summary == null
                || !OwnsRun(summary.Identity.RunId))
            {
                return;
            }

            string runId = summary.Identity.RunId;
            string digest = summary.ResultSummaryDigest;
            if (presentationRetryRoutine != null)
            {
                if (string.Equals(presentationRetryRunId, runId, StringComparison.Ordinal)
                    && string.Equals(
                        presentationRetryDigest,
                        digest,
                        StringComparison.Ordinal))
                {
                    return;
                }

                StopCoroutine(presentationRetryRoutine);
                presentationRetryRoutine = null;
            }

            presentationRetryCount++;
            if (presentationRetryCount == 3)
            {
                Debug.LogWarning(
                    $"[{nameof(OneRowStageRunResultPresenter)}] Result presentation remains unavailable; low-frequency retry remains active: {LastCommitError}",
                    this);
            }

            presentationRetryRunId = runId;
            presentationRetryDigest = digest;
            presentationRetryRoutine = StartCoroutine(
                RetryPresentationRoutine(
                    runId,
                    digest,
                    presentationRetryCount));
        }

        private System.Collections.IEnumerator RetryPresentationRoutine(
            string runId,
            string digest,
            int retryOrdinal)
        {
            float delaySeconds = retryOrdinal <= 3
                ? 0.25f * retryOrdinal
                : Mathf.Min(5f, 0.5f * (retryOrdinal - 2));
            yield return new WaitForSecondsRealtime(delaySeconds);
            presentationRetryRoutine = null;
            presentationRetryRunId = string.Empty;
            presentationRetryDigest = string.Empty;
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (OwnsRun(runId)
                && context != null
                && context.CommittedSummary != null
                && context.CommitReceipt != null)
            {
                StageRunResultSummary summary = context.CommittedSummary;
                if (string.Equals(
                    summary.ResultSummaryDigest,
                    digest,
                    StringComparison.Ordinal))
                {
                    PublishCommittedResult(summary, context.CommitReceipt);
                }
            }
        }

        private void CancelPresentationRetry()
        {
            if (presentationRetryRoutine != null)
            {
                StopCoroutine(presentationRetryRoutine);
                presentationRetryRoutine = null;
            }

            presentationRetryRunId = string.Empty;
            presentationRetryDigest = string.Empty;
        }
    }
}
