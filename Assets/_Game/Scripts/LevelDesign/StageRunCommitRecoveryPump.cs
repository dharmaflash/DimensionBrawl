using System;
using System.Collections;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    internal sealed class StageRunCommitRecoveryPump : MonoBehaviour
    {
        private const int FastAttemptCount = 5;

        private static StageRunCommitRecoveryPump instance;
        private Coroutine recoveryRoutine;
        private string recoveringRunId = string.Empty;
        private OneRowStageRunResultPresenter recoveryPresenter;

        internal static event Action<string, int> Attempted;
        internal static event Action<StageRunResultSummary, StageRunResultCommitReceipt> Recovered;
        internal static event Action<string, string> RecoveryDelayed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntime()
        {
            instance = null;
            Attempted = null;
            Recovered = null;
            RecoveryDelayed = null;
        }

        internal static void RequestRecovery(OneRowStageRunResultPresenter presenter)
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (!Application.isPlaying
                || context == null
                || context.LifecycleState != StageRunLifecycleState.CommitRecoveryPending)
            {
                return;
            }

            if (instance == null)
            {
                var root = new GameObject("StageRunCommitRecoveryPump")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                DontDestroyOnLoad(root);
                instance = root.AddComponent<StageRunCommitRecoveryPump>();
            }

            instance.BeginFor(context.Identity.RunId, presenter);
        }

        private void BeginFor(string runId, OneRowStageRunResultPresenter presenter)
        {
            if (presenter != null)
            {
                recoveryPresenter = presenter;
            }

            if (recoveryRoutine != null
                && string.Equals(recoveringRunId, runId, StringComparison.Ordinal))
            {
                return;
            }

            if (recoveryRoutine != null)
            {
                StopCoroutine(recoveryRoutine);
            }

            recoveringRunId = runId ?? string.Empty;
            recoveryRoutine = StartCoroutine(RecoverRoutine(recoveringRunId));
        }

        private IEnumerator RecoverRoutine(string runId)
        {
            string lastError = string.Empty;
            for (int attempt = 1; ; attempt++)
            {
                float delaySeconds = attempt <= FastAttemptCount
                    ? 0.1f * attempt
                    : Mathf.Min(5f, 0.5f * (attempt - FastAttemptCount + 1));
                yield return new WaitForSecondsRealtime(delaySeconds);
                StageRunContext context = StageRunRuntime.ActiveContext;
                if (context == null
                    || !string.Equals(context.Identity.RunId, runId, StringComparison.Ordinal))
                {
                    Finish();
                    yield break;
                }

                if ((context.LifecycleState == StageRunLifecycleState.Committed
                        || context.LifecycleState == StageRunLifecycleState.Presented)
                    && context.CommittedSummary != null
                    && context.CommitReceipt != null)
                {
                    CompleteRecoveredPublication(
                        runId,
                        context.CommittedSummary,
                        context.CommitReceipt);
                    yield break;
                }

                if (context.LifecycleState != StageRunLifecycleState.CommitRecoveryPending)
                {
                    InvokeRecoveryDelayedSafely(
                        runId,
                        string.IsNullOrWhiteSpace(lastError)
                            ? context.FaultReason
                            : lastError);
                    Finish();
                    yield break;
                }

                InvokeAttemptedSafely(runId, attempt);
                if (StageRunRuntime.TryRecoverPendingResultCommit(
                    out StageRunResultSummary summary,
                    out StageRunResultCommitReceipt receipt,
                    out lastError))
                {
                    CompleteRecoveredPublication(runId, summary, receipt);
                    yield break;
                }

                context = StageRunRuntime.ActiveContext;
                if (context != null
                    && string.Equals(context.Identity.RunId, runId, StringComparison.Ordinal)
                    && (context.LifecycleState == StageRunLifecycleState.Committed
                        || context.LifecycleState == StageRunLifecycleState.Presented)
                    && context.CommittedSummary != null
                    && context.CommitReceipt != null)
                {
                    CompleteRecoveredPublication(
                        runId,
                        context.CommittedSummary,
                        context.CommitReceipt);
                    yield break;
                }

                if (context == null
                    || !string.Equals(context.Identity.RunId, runId, StringComparison.Ordinal)
                    || context.LifecycleState != StageRunLifecycleState.CommitRecoveryPending)
                {
                    InvokeRecoveryDelayedSafely(
                        runId,
                        string.IsNullOrWhiteSpace(lastError)
                            ? context?.FaultReason ?? "Commit recovery ended before publication."
                            : lastError);
                    break;
                }

                if (attempt == FastAttemptCount)
                {
                    InvokeRecoveryDelayedSafely(runId, lastError);
                }
            }

            Finish();
        }

        private void CompleteRecoveredPublication(
            string runId,
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            try
            {
                if (recoveryPresenter != null)
                {
                    try
                    {
                        recoveryPresenter.PublishRecoveredResult(summary, receipt);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, this);
                    }
                }

                InvokeRecoveredSafely(summary, receipt);
            }
            finally
            {
                try
                {
                    StageRunRuntime.ReleaseRecoveredContextIfOwnerSceneLost(runId);
                }
                finally
                {
                    Finish();
                }
            }
        }

        private void InvokeAttemptedSafely(string runId, int attempt)
        {
            Delegate[] subscribers = Attempted?.GetInvocationList();
            if (subscribers == null)
            {
                return;
            }

            for (int i = 0; i < subscribers.Length; i++)
            {
                try
                {
                    ((Action<string, int>)subscribers[i]).Invoke(runId, attempt);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void InvokeRecoveredSafely(
            StageRunResultSummary summary,
            StageRunResultCommitReceipt receipt)
        {
            Delegate[] subscribers = Recovered?.GetInvocationList();
            if (subscribers == null)
            {
                return;
            }

            for (int i = 0; i < subscribers.Length; i++)
            {
                try
                {
                    ((Action<StageRunResultSummary, StageRunResultCommitReceipt>)subscribers[i])
                        .Invoke(summary, receipt);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void InvokeRecoveryDelayedSafely(string runId, string error)
        {
            Delegate[] subscribers = RecoveryDelayed?.GetInvocationList();
            if (subscribers == null)
            {
                return;
            }

            for (int i = 0; i < subscribers.Length; i++)
            {
                try
                {
                    ((Action<string, string>)subscribers[i]).Invoke(runId, error);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void Finish()
        {
            recoveryRoutine = null;
            recoveringRunId = string.Empty;
            recoveryPresenter = null;
            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(instance, this))
            {
                instance = null;
            }
        }

#if UNITY_INCLUDE_TESTS
        internal static void ResetForTests()
        {
            if (instance != null && instance.gameObject != null)
            {
                DestroyImmediate(instance.gameObject);
            }

            instance = null;
            Attempted = null;
            Recovered = null;
            RecoveryDelayed = null;
        }
#endif
    }
}
