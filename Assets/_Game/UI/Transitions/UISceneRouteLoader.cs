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

        public UITransitionPresenter TransitionPresenter => transitionPresenter;

        public IEnumerator Load(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler)
        {
            if (transitionPresenter != null)
            {
                stateHandler?.Invoke(route, UISceneFlowPhase.TransitionOut, 0f, "Transition", true);
                yield return transitionPresenter.PlayOut(route);
            }

            yield return LoadRouteRoutine(route, stateHandler, failureHandler);
        }

        public void SetProgress(float normalizedProgress, string label)
        {
            if (transitionPresenter != null)
            {
                transitionPresenter.SetProgress(normalizedProgress, label);
            }
        }

        private IEnumerator LoadRouteRoutine(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(route.ScenePath))
            {
                yield return SimulateMinimumLoading(route, stateHandler);
                stateHandler?.Invoke(route, UISceneFlowPhase.Activating, 1f, "Activating", true);

                string failure = LoadEditorScene(route);
                if (!string.IsNullOrWhiteSpace(failure))
                {
                    yield return FailRouteLoad(route, failure, stateHandler, failureHandler);
                }

                yield break;
            }
#endif

            if (route.UseAsyncLoading)
            {
                yield return LoadRouteAsync(route, stateHandler, failureHandler);
                yield break;
            }

            yield return SimulateMinimumLoading(route, stateHandler);
            stateHandler?.Invoke(route, UISceneFlowPhase.Activating, 1f, "Activating", true);

            string syncFailure = LoadScene(route);
            if (!string.IsNullOrWhiteSpace(syncFailure))
            {
                yield return FailRouteLoad(route, syncFailure, stateHandler, failureHandler);
            }
        }

        private IEnumerator LoadRouteAsync(
            UIScreenRouteTable.Route route,
            RouteLoadStateHandler stateHandler,
            RouteLoadFailureHandler failureHandler)
        {
            AsyncOperation operation = CreateAsyncOperation(route, out string failure);
            if (operation == null)
            {
                string reason = string.IsNullOrWhiteSpace(failure)
                    ? "Async scene operation was not created."
                    : failure;
                yield return FailRouteLoad(route, reason, stateHandler, failureHandler);
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
            RouteLoadFailureHandler failureHandler)
        {
            string detail = string.IsNullOrWhiteSpace(reason) ? "Unknown route load failure." : reason;
            Debug.LogWarning($"UI route failed to load {route.SceneName}: {detail}", this);
            failureHandler?.Invoke(detail);
            stateHandler?.Invoke(route, UISceneFlowPhase.Failed, 0f, "Route failed", false);

            if (transitionPresenter != null)
            {
                yield return transitionPresenter.PlayIn();
            }
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
