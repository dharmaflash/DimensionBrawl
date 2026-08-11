using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UISceneRouteLoader : MonoBehaviour
    {
        public delegate void RouteLoadStateHandler(
            UIScreenRouteTable.Route route,
            UISceneFlowPhase phase,
            float normalizedProgress,
            string label,
            bool routing);

        public delegate void RouteLoadFailureHandler(string reason);

        [SerializeField] private UITransitionPresenter transitionPresenter;

        public UITransitionPresenter TransitionPresenter => ResolveTransitionPresenter();

        public IEnumerator Load(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler)
        {
            UISceneTransitionHandoffOwner handoffOwner = UISceneTransitionHandoffOwner.CurrentOwner;
            UISceneTransitionTicket ticket = default;
            bool hasHandoffTicket = handoffOwner != null;
            if (hasHandoffTicket
                && !handoffOwner.TryBeginRoute(route, gameObject.scene, out ticket, out string beginFailure))
            {
                ReportRouteFailure(route, beginFailure, stateHandler, failureHandler);
                yield break;
            }

            UITransitionPresenter presenter = ResolveTransitionPresenter();
            if (presenter != null)
            {
                stateHandler?.Invoke(route, UISceneFlowPhase.TransitionOut, 0f, "Transition", true);
                yield return presenter.PlayOut(route);
            }

            yield return LoadRouteRoutine(
                route,
                stateHandler,
                failureHandler,
                handoffOwner,
                ticket,
                hasHandoffTicket);
        }

        public void SetProgress(float normalizedProgress, string label)
        {
            UITransitionPresenter presenter = ResolveTransitionPresenter();
            if (presenter != null)
            {
                presenter.SetProgress(normalizedProgress, label);
            }
        }

        private IEnumerator LoadRouteRoutine(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler,
            UISceneTransitionHandoffOwner handoffOwner,
            UISceneTransitionTicket ticket,
            bool hasHandoffTicket)
        {
#if UNITY_EDITOR
            if (!route.UseAsyncLoading && !string.IsNullOrWhiteSpace(route.ScenePath))
            {
                yield return SimulateMinimumLoading(route, stateHandler);
                stateHandler?.Invoke(route, UISceneFlowPhase.Activating, 1f, "Activating", true);

                if (!TryMarkActivationRequested(handoffOwner, ticket, hasHandoffTicket, out string activationFailure))
                {
                    yield return FailRouteLoad(
                        route,
                        activationFailure,
                        stateHandler,
                        failureHandler,
                        handoffOwner,
                        ticket,
                        hasHandoffTicket);
                    yield break;
                }

                string failure = LoadEditorScene(route);
                if (!string.IsNullOrWhiteSpace(failure))
                {
                    yield return FailRouteLoad(
                        route,
                        failure,
                        stateHandler,
                        failureHandler,
                        handoffOwner,
                        ticket,
                        hasHandoffTicket);
                }

                yield break;
            }
#endif

            if (route.UseAsyncLoading)
            {
                yield return LoadRouteAsync(
                    route,
                    stateHandler,
                    failureHandler,
                    handoffOwner,
                    ticket,
                    hasHandoffTicket);
                yield break;
            }

            yield return SimulateMinimumLoading(route, stateHandler);
            stateHandler?.Invoke(route, UISceneFlowPhase.Activating, 1f, "Activating", true);

            if (!TryMarkActivationRequested(handoffOwner, ticket, hasHandoffTicket, out string syncActivationFailure))
            {
                yield return FailRouteLoad(
                    route,
                    syncActivationFailure,
                    stateHandler,
                    failureHandler,
                    handoffOwner,
                    ticket,
                    hasHandoffTicket);
                yield break;
            }

            string syncFailure = LoadScene(route);
            if (!string.IsNullOrWhiteSpace(syncFailure))
            {
                yield return FailRouteLoad(
                    route,
                    syncFailure,
                    stateHandler,
                    failureHandler,
                    handoffOwner,
                    ticket,
                    hasHandoffTicket);
            }
        }

        private IEnumerator LoadRouteAsync(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler,
            UISceneTransitionHandoffOwner handoffOwner,
            UISceneTransitionTicket ticket,
            bool hasHandoffTicket)
        {
            AsyncOperation operation = CreateAsyncOperation(route, out string failure);
            if (operation == null)
            {
                string reason = string.IsNullOrWhiteSpace(failure)
                    ? "Async scene operation was not created."
                    : failure;
                yield return FailRouteLoad(
                    route,
                    reason,
                    stateHandler,
                    failureHandler,
                    handoffOwner,
                    ticket,
                    hasHandoffTicket);
                yield break;
            }

            operation.allowSceneActivation = false;

            float elapsed = 0f;
            float minimumSeconds = Mathf.Max(0f, route.MinimumLoadingSeconds);
            while (operation.progress < 0.9f || elapsed < minimumSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float asyncProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float timeProgress = minimumSeconds > 0f ? Mathf.Clamp01(elapsed / minimumSeconds) : 1f;
                stateHandler?.Invoke(route, UISceneFlowPhase.Loading, Mathf.Min(asyncProgress, timeProgress), "Loading", true);
                yield return null;
            }

            stateHandler?.Invoke(route, UISceneFlowPhase.Activating, 1f, "Activating", true);
            if (!TryMarkActivationRequested(handoffOwner, ticket, hasHandoffTicket, out string activationFailure))
            {
                yield return FailRouteLoad(
                    route,
                    activationFailure,
                    stateHandler,
                    failureHandler,
                    handoffOwner,
                    ticket,
                    hasHandoffTicket);
                yield break;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }
        }

        private IEnumerator SimulateMinimumLoading(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler)
        {
            float minimumSeconds = Mathf.Max(0f, route.MinimumLoadingSeconds);
            if (minimumSeconds <= 0f)
            {
                stateHandler?.Invoke(route, UISceneFlowPhase.Loading, 1f, "Ready", true);
                yield break;
            }

            for (float elapsed = 0f; elapsed < minimumSeconds; elapsed += Time.unscaledDeltaTime)
            {
                stateHandler?.Invoke(route, UISceneFlowPhase.Loading, elapsed / minimumSeconds, "Loading", true);
                yield return null;
            }

            stateHandler?.Invoke(route, UISceneFlowPhase.Loading, 1f, "Ready", true);
        }

        private IEnumerator FailRouteLoad(
            UIScreenRouteTable.Route route,
            string reason,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler,
            UISceneTransitionHandoffOwner handoffOwner,
            UISceneTransitionTicket ticket,
            bool hasHandoffTicket)
        {
            string detail = ReportRouteFailure(route, reason, stateHandler, failureHandler);

            if (hasHandoffTicket)
            {
                if (handoffOwner != null)
                {
                    yield return handoffOwner.FailAndRestore(ticket, detail);
                }

                yield break;
            }

            UITransitionPresenter presenter = ResolveTransitionPresenter();
            if (presenter != null)
            {
                yield return presenter.PlayIn();
            }
        }

        private UITransitionPresenter ResolveTransitionPresenter()
        {
            UISceneTransitionHandoffOwner owner = UISceneTransitionHandoffOwner.CurrentOwner;
            if (owner != null && owner.Presenter != null)
            {
                return owner.Presenter;
            }

            return transitionPresenter;
        }

        private static bool TryMarkActivationRequested(
            UISceneTransitionHandoffOwner handoffOwner,
            UISceneTransitionTicket ticket,
            bool hasHandoffTicket,
            out string failure)
        {
            failure = string.Empty;
            if (!hasHandoffTicket)
            {
                return true;
            }

            if (handoffOwner == null)
            {
                failure = "The persistent transition handoff owner was destroyed before scene activation.";
                return false;
            }

            if (!handoffOwner.TryMarkActivationRequested(ticket))
            {
                failure = $"Transition handoff generation {ticket.Generation} became stale before scene activation.";
                return false;
            }

            return true;
        }

        private string ReportRouteFailure(
            UIScreenRouteTable.Route route,
            string reason,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler)
        {
            string detail = string.IsNullOrWhiteSpace(reason) ? "Unknown route load failure." : reason;
            Debug.LogWarning($"UI route failed to load {route.SceneName}: {detail}", this);
            failureHandler?.Invoke(detail);
            stateHandler?.Invoke(route, UISceneFlowPhase.Failed, 0f, "Route failed", false);
            return detail;
        }

#if UNITY_EDITOR
        private static string LoadEditorScene(UIScreenRouteTable.Route route)
        {
            try
            {
                EditorSceneManager.LoadSceneInPlayMode(route.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
#endif

        private static string LoadScene(UIScreenRouteTable.Route route)
        {
            try
            {
                SceneManager.LoadScene(route.SceneName, LoadSceneMode.Single);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private static AsyncOperation CreateAsyncOperation(UIScreenRouteTable.Route route, out string failure)
        {
            try
            {
                failure = string.Empty;
                return SceneManager.LoadSceneAsync(route.SceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                failure = exception.Message;
                return null;
            }
        }
    }
}
