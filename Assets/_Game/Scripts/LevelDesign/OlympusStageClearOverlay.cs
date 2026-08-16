using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(1600)]
    [DisallowMultipleComponent]
    public sealed class OlympusStageClearOverlay : MonoBehaviour, IStageRunResultOverlay
    {
        private const string ClearUiSceneName = "UI_StageClear";
        private const string ClearUiScenePath = "Assets/_Game/Scenes/UI/UI_StageClear.unity";

        private static readonly string[] CombatHudExitRootNames =
        {
            "BossBarrageLaneReview_CombatHudCanvas",
            "PF_UI_CombatHud",
            "PF_UI_CombatHudPresentation"
        };

        [SerializeField] private int sortOrder = 7000;
        [SerializeField, Min(0f)] private float combatHudExitSeconds = 0.42f;
        [SerializeField, Min(0f)] private float postBossDefeatHoldSeconds = 1.1f;
        [SerializeField, Min(0f)] private float hudExitSlidePixels = 128f;
        [SerializeField] private OlympusStationBossTerminalAftermathPresenter bossTerminalAftermath;
        [SerializeField, Min(0.1f)] private float bossTerminalAftermathWaitSlackSeconds = 0.5f;

        private Coroutine stageClearRoutine;
        private bool shown;
        private bool combatLocked;
        private bool worldTimeScaleFrozen;
        private bool aftermathGateAttached;
        private bool aftermathSignalsSubscribed;
        private bool aftermathHandoffCompleted;
        private bool resultSceneLoadRequested;
        private int resultSceneLoadLeaseToken;
        private bool resultSurfaceFinalized;
        private bool presentationFailureFinalized;
        private float previousTimeScale = 1f;
        private StageRunResultSummary resultSummary;
        private string presentedResultDigest = string.Empty;

        public bool IsShown => shown;
        public StageRunResultSummary ResultSummary => resultSummary;
        public string PendingResultDigest => shown && string.IsNullOrEmpty(presentedResultDigest)
            ? resultSummary?.ResultSummaryDigest ?? string.Empty
            : string.Empty;
        public string PresentedResultDigest => presentedResultDigest;
        public string LastPresentationError { get; private set; } = string.Empty;
        public string LastAftermathError { get; private set; } = string.Empty;
        public bool IsCombatPreparedForResult => combatLocked;
        public bool IsWorldFrozenForResult => worldTimeScaleFrozen;
        public bool IsWaitingForBossTerminalAftermath => aftermathGateAttached
            && bossTerminalAftermath != null
            && !bossTerminalAftermath.IsComplete;

        public event Action<StageRunResultSummary> PresentationSucceeded;
        public event Action<StageRunResultSummary, string> PresentationFailed;

        private void OnDisable()
        {
            bool presentationWasPending = shown
                && !resultSurfaceFinalized
                && !presentationFailureFinalized;
            if (stageClearRoutine != null)
            {
                StopCoroutine(stageClearRoutine);
                stageClearRoutine = null;
            }

            if (presentationWasPending)
            {
                FailPresentation(
                    "The result overlay was disabled before presentation acknowledgement.",
                    logAsError: false);
            }

            CancelOwnedResultSceneLoad();
            CancelAftermathAndRelease("The result overlay was disabled before terminal handoff cleanup.");
            RestoreCombatTimeScale();
        }

        public void Show()
        {
            Show(null);
        }

        public void Show(StageRunResultSummary summary)
        {
            if (!TryShow(summary, out string error) && !string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError($"[{nameof(OlympusStageClearOverlay)}] {error}", this);
            }
        }

        public bool TryShow(StageRunResultSummary summary, out string error)
        {
            error = string.Empty;
            string requestedDigest = summary?.ResultSummaryDigest ?? string.Empty;
            string currentDigest = resultSummary?.ResultSummaryDigest ?? string.Empty;
            if (shown)
            {
                if (ReferenceEquals(resultSummary, summary)
                    || string.Equals(currentDigest, requestedDigest, StringComparison.Ordinal))
                {
                    return true;
                }

                error = "A different result presentation request is already active.";
                return false;
            }

            if (!isActiveAndEnabled)
            {
                error = "The result overlay is not active and enabled.";
                LastPresentationError = error;
                CancelAftermathAndRelease(error);
                return false;
            }

            RefreshResultSceneLoadLease();
            ResultScenePreloadLease.RetryCancellation();
            if (ResultScenePreloadLease.IsBusy)
            {
                error = "A previous authored result-scene preload is still being cleaned up.";
                LastPresentationError = error;
                return false;
            }

            resultSummary = summary;
            presentedResultDigest = string.Empty;
            LastPresentationError = string.Empty;
            LastAftermathError = string.Empty;
            shown = true;
            aftermathGateAttached = false;
            aftermathHandoffCompleted = false;
            resultSceneLoadRequested = false;
            resultSurfaceFinalized = false;
            presentationFailureFinalized = false;
            bool useBossTerminalAftermath = IsBossTerminalClear(summary)
                && bossTerminalAftermath != null;
            if (useBossTerminalAftermath
                && !bossTerminalAftermath.TryAttachResult(summary, out string aftermathError))
            {
                shown = false;
                error = "The authored Station boss-terminal aftermath rejected result attachment: "
                    + aftermathError;
                LastPresentationError = error;
                LastAftermathError = aftermathError;
                CancelAftermathAndRelease(error);
                return false;
            }

            aftermathGateAttached = useBossTerminalAftermath;
            try
            {
                PrepareCombatAfterClear();
                if (aftermathGateAttached)
                {
                    SubscribeAftermathSignals();
                    if (bossTerminalAftermath.IsHandoffImminent)
                    {
                        HandleAftermathHandoffImminent();
                    }

                    if (shown && bossTerminalAftermath.IsComplete)
                    {
                        HandleAftermathCompleted();
                    }
                }
                else
                {
                    FreezeWorldForResult();
                }

                if (!shown)
                {
                    error = LastPresentationError;
                    return false;
                }

                if (!resultSurfaceFinalized)
                {
                    stageClearRoutine = StartCoroutine(
                        RunPresentationSafely(useBossTerminalAftermath));
                }

                return true;
            }
            catch (Exception exception)
            {
                shown = false;
                error = "The result overlay could not start safely: " + exception.Message;
                LastPresentationError = error;
                CancelAftermathAndRelease(error);
                CancelOwnedResultSceneLoad();
                RestoreCombatTimeScale();
                return false;
            }
        }

        private IEnumerator RunPresentationSafely(bool waitForBossTerminalAftermath)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(ShowAuthoredStageClearSceneRoutine(waitForBossTerminalAftermath));
            while (stack.Count > 0)
            {
                IEnumerator current = stack.Peek();
                bool moved;
                object yielded;
                try
                {
                    moved = current.MoveNext();
                    yielded = moved ? current.Current : null;
                }
                catch (Exception exception)
                {
                    DisposeRoutineStackSafely(stack);
                    FailPresentation(
                        "The authored result presentation failed safely: " + exception.Message);
                    yield break;
                }

                if (!moved)
                {
                    stack.Pop();
                    DisposeRoutineSafely(current);
                    continue;
                }

                if (yielded is IEnumerator nested)
                {
                    stack.Push(nested);
                    continue;
                }

                yield return yielded;
            }
        }

        private static void DisposeRoutineStackSafely(Stack<IEnumerator> stack)
        {
            while (stack.Count > 0)
            {
                DisposeRoutineSafely(stack.Pop());
            }
        }

        private static void DisposeRoutineSafely(IEnumerator routine)
        {
            if (routine is not IDisposable disposable)
            {
                return;
            }

            try
            {
                disposable.Dispose();
            }
            catch
            {
                // Cleanup must never mask the presentation failure boundary.
            }
        }

        private IEnumerator ShowAuthoredStageClearSceneRoutine(bool waitForBossTerminalAftermath)
        {
            yield return PlayCombatHudExitRoutine();

            if (waitForBossTerminalAftermath)
            {
                float remaining = bossTerminalAftermath != null
                    ? Mathf.Max(
                        0f,
                        bossTerminalAftermath.AftermathDurationSeconds
                            - bossTerminalAftermath.ElapsedUnscaledSeconds)
                    : 0f;
                float aftermathTimeoutAt = PresentationClock.UnscaledTime
                    + remaining
                    + Mathf.Max(0.1f, bossTerminalAftermathWaitSlackSeconds);
                while (bossTerminalAftermath != null
                    && !bossTerminalAftermath.IsComplete
                    && !bossTerminalAftermath.IsCancelled)
                {
                    if (PresentationClock.UnscaledTime > aftermathTimeoutAt)
                    {
                        FailPresentation("The authored Station boss-terminal aftermath timed out.");
                        yield break;
                    }

                    yield return null;
                }

                if (!shown || presentationFailureFinalized || resultSurfaceFinalized)
                {
                    yield break;
                }

                if (bossTerminalAftermath == null
                    || bossTerminalAftermath.IsCancelled
                    || !bossTerminalAftermath.CompletedSuccessfully)
                {
                    LastAftermathError = bossTerminalAftermath != null
                        ? bossTerminalAftermath.LastError
                        : "The authored Station boss-terminal aftermath owner was lost.";
                    FailPresentation(
                        "The authored Station boss-terminal aftermath did not complete successfully: "
                        + LastAftermathError);
                    yield break;
                }

                if (!aftermathHandoffCompleted)
                {
                    HandleAftermathCompleted();
                }

                if (!shown || resultSurfaceFinalized)
                {
                    yield break;
                }
            }
            else if (postBossDefeatHoldSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(postBossDefeatHoldSeconds);
            }

            if (!TryRequestStageClearSceneLoad(out string sceneLoadError))
            {
                FailPresentation(sceneLoadError);
                yield break;
            }

            bool configured = false;
            float timeoutAt = PresentationClock.UnscaledTime + 2f;
            while (PresentationClock.UnscaledTime <= timeoutAt)
            {
                configured = ConfigureStageClearPresenters(
                    ResolveRequestedResultScene(),
                    sortOrder,
                    resultSummary);
                if (configured)
                {
                    break;
                }

                yield return null;
            }

            if (!configured)
            {
                FailPresentation(
                    $"Authored stage clear scene did not configure an exact {nameof(StageClearScreenPresenter)}.");
                yield break;
            }

            CompleteSuccessfulPresentation();
        }

        private void SubscribeAftermathSignals()
        {
            if (aftermathSignalsSubscribed || bossTerminalAftermath == null)
            {
                return;
            }

            bossTerminalAftermath.AftermathHandoffImminent +=
                HandleAftermathHandoffImminent;
            bossTerminalAftermath.AftermathCompleted += HandleAftermathCompleted;
            aftermathSignalsSubscribed = true;
        }

        private void UnsubscribeAftermathSignals()
        {
            if (!aftermathSignalsSubscribed)
            {
                return;
            }

            if (bossTerminalAftermath != null)
            {
                bossTerminalAftermath.AftermathHandoffImminent -=
                    HandleAftermathHandoffImminent;
                bossTerminalAftermath.AftermathCompleted -= HandleAftermathCompleted;
            }

            aftermathSignalsSubscribed = false;
        }

        private void HandleAftermathHandoffImminent()
        {
            if (!shown || !aftermathGateAttached || resultSurfaceFinalized)
            {
                return;
            }

            if (!TryRequestStageClearSceneLoad(out string error))
            {
                FailPresentation(error);
            }
        }

        private void HandleAftermathCompleted()
        {
            if (!shown
                || !aftermathGateAttached
                || aftermathHandoffCompleted
                || resultSurfaceFinalized)
            {
                return;
            }

            try
            {
                if (bossTerminalAftermath == null
                    || bossTerminalAftermath.IsCancelled
                    || !bossTerminalAftermath.CompletedSuccessfully)
                {
                    LastAftermathError = bossTerminalAftermath != null
                        ? bossTerminalAftermath.LastError
                        : "The authored Station boss-terminal aftermath owner was lost.";
                    FailPresentation(
                        "The authored Station boss-terminal aftermath did not complete successfully: "
                        + LastAftermathError);
                    return;
                }

                FreezeWorldForResult();
                bossTerminalAftermath.ReleaseInputLeaseForResultSurface();
                if (bossTerminalAftermath.InputLeaseActive)
                {
                    LastAftermathError = bossTerminalAftermath.LastError;
                    FailPresentation(
                        "The authored Station boss-terminal aftermath input lease did not release exactly.");
                    return;
                }

                aftermathHandoffCompleted = true;
                aftermathGateAttached = false;
                UnsubscribeAftermathSignals();
                if (!TryRequestStageClearSceneLoad(out string sceneLoadError))
                {
                    FailPresentation(sceneLoadError);
                    return;
                }

                Scene clearScene = ResolveRequestedResultScene();
                if (clearScene.IsValid() && clearScene.isLoaded)
                {
                    if (!ConfigureStageClearPresenters(
                        clearScene,
                        sortOrder,
                        resultSummary))
                    {
                        FailPresentation(
                            $"Authored stage clear scene did not configure an exact {nameof(StageClearScreenPresenter)}.");
                        return;
                    }

                    CompleteSuccessfulPresentation();
                }
            }
            catch (Exception exception)
            {
                FailPresentation(
                    "The authored Station boss-terminal completion handoff failed safely: "
                    + exception.Message);
            }
        }

        private bool TryRequestStageClearSceneLoad(out string error)
        {
            error = string.Empty;
            Scene clearScene = SceneManager.GetSceneByName(ClearUiSceneName);
            if (clearScene.IsValid() && clearScene.isLoaded)
            {
                resultSceneLoadRequested = true;
                return true;
            }

            if (resultSceneLoadRequested)
            {
                return true;
            }

            if (!ResultScenePreloadLease.TryAcquire(
                ClearUiSceneName,
                out int leaseToken,
                out error))
            {
                return false;
            }

            resultSceneLoadLeaseToken = leaseToken;
            resultSceneLoadRequested = true;
            try
            {
                Scene requestedScene;
#if UNITY_EDITOR
                requestedScene = EditorSceneManager.LoadSceneInPlayMode(
                    ClearUiScenePath,
                    new LoadSceneParameters(LoadSceneMode.Additive));
#else
                requestedScene = SceneManager.LoadScene(
                    ClearUiSceneName,
                    new LoadSceneParameters(LoadSceneMode.Additive));
#endif
                ResultScenePreloadLease.RecordRequestedScene(
                    resultSceneLoadLeaseToken,
                    requestedScene);
                return true;
            }
            catch (Exception exception)
            {
                ResultScenePreloadLease.Abandon(resultSceneLoadLeaseToken);
                resultSceneLoadLeaseToken = 0;
                resultSceneLoadRequested = false;
                error = "Failed to load authored clear UI scene: " + exception.Message;
                return false;
            }
        }

        private bool ResultSceneLoadOwned =>
            ResultScenePreloadLease.IsOwned(resultSceneLoadLeaseToken);

        private void RefreshResultSceneLoadLease()
        {
            if (resultSceneLoadLeaseToken == 0
                || ResultScenePreloadLease.IsOwned(resultSceneLoadLeaseToken))
            {
                return;
            }

            resultSceneLoadLeaseToken = 0;
            resultSceneLoadRequested = false;
        }

        private Scene ResolveRequestedResultScene()
        {
            if (resultSceneLoadLeaseToken != 0
                && ResultScenePreloadLease.TryResolve(
                    resultSceneLoadLeaseToken,
                    out Scene ownedScene))
            {
                return ownedScene;
            }

            return resultSceneLoadLeaseToken == 0
                ? SceneManager.GetSceneByName(ClearUiSceneName)
                : default;
        }

        private void CancelOwnedResultSceneLoad()
        {
            if (!ResultSceneLoadOwned || resultSurfaceFinalized)
            {
                return;
            }

            ResultScenePreloadLease.Cancel(resultSceneLoadLeaseToken);
        }

        private void RelinquishOwnedResultSceneLoad()
        {
            ResultScenePreloadLease.Relinquish(resultSceneLoadLeaseToken);
            resultSceneLoadLeaseToken = 0;
        }

        private void CompleteSuccessfulPresentation()
        {
            if (resultSurfaceFinalized || presentationFailureFinalized)
            {
                return;
            }

            resultSurfaceFinalized = true;
            RelinquishOwnedResultSceneLoad();
            UnsubscribeAftermathSignals();
            presentedResultDigest = resultSummary?.ResultSummaryDigest ?? string.Empty;
            LastPresentationError = string.Empty;
            stageClearRoutine = null;
            InvokePresentationSucceededSafely(resultSummary);
        }

        private void FailPresentation(string error, bool logAsError = true)
        {
            if (presentationFailureFinalized || resultSurfaceFinalized)
            {
                return;
            }

            presentationFailureFinalized = true;
            LastPresentationError = error ?? string.Empty;
            CancelOwnedResultSceneLoad();
            CancelAftermathAndRelease(LastPresentationError);
            RestoreCombatTimeScale();
            shown = false;
            presentedResultDigest = string.Empty;
            stageClearRoutine = null;
            if (logAsError)
            {
                Debug.LogError(
                    $"[{nameof(OlympusStageClearOverlay)}] {LastPresentationError}",
                    this);
            }
            else
            {
                Debug.LogWarning(
                    $"[{nameof(OlympusStageClearOverlay)}] {LastPresentationError}",
                    this);
            }
            InvokePresentationFailedSafely(resultSummary, LastPresentationError);
        }

        private void InvokePresentationSucceededSafely(StageRunResultSummary summary)
        {
            Action<StageRunResultSummary> callback = PresentationSucceeded;
            if (callback == null)
            {
                return;
            }

            Delegate[] listeners = callback.GetInvocationList();
            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    ((Action<StageRunResultSummary>)listeners[i]).Invoke(summary);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void InvokePresentationFailedSafely(
            StageRunResultSummary summary,
            string error)
        {
            Action<StageRunResultSummary, string> callback = PresentationFailed;
            if (callback == null)
            {
                return;
            }

            Delegate[] listeners = callback.GetInvocationList();
            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    ((Action<StageRunResultSummary, string>)listeners[i]).Invoke(summary, error);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private IEnumerator PlayCombatHudExitRoutine()
        {
            List<UiExitTarget> targets = CollectCombatHudExitTargets();
            if (targets.Count == 0)
            {
                HideCombatHudRootsImmediate();
                yield break;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].SetInteractive(false);
            }

            float duration = Mathf.Max(0.01f, combatHudExitSeconds);
            if (combatHudExitSeconds <= 0f)
            {
                ApplyHudExit(targets, 1f);
                HideCombatHudRootsImmediate();
                yield break;
            }

            for (float elapsed = 0f;
                elapsed < duration;
                elapsed += PresentationClock.UnscaledDeltaTime)
            {
                ApplyHudExit(targets, EaseOutCubic(elapsed / duration));
                yield return null;
            }

            ApplyHudExit(targets, 1f);
            HideCombatHudRootsImmediate();
        }

        private static void ApplyHudExit(List<UiExitTarget> targets, float progress)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Apply(progress);
            }
        }

        private List<UiExitTarget> CollectCombatHudExitTargets()
        {
            var targets = new List<UiExitTarget>(8);
            var seen = new HashSet<Transform>();
            for (int i = 0; i < CombatHudExitRootNames.Length; i++)
            {
                GameObject root = GameObject.Find(CombatHudExitRootNames[i]);
                AddUiExitTarget(root != null ? root.transform : null, targets, seen);
            }

            return targets;
        }

        private void AddUiExitTarget(
            Transform target,
            List<UiExitTarget> targets,
            HashSet<Transform> seen)
        {
            if (target is not RectTransform rectTransform || !seen.Add(target))
            {
                return;
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 direction = screenPoint - center;
            if (direction.sqrMagnitude < 16f)
            {
                direction = Vector2.down;
            }
            else
            {
                direction.Normalize();
            }

            if (Mathf.Abs(direction.x) < 0.18f)
            {
                direction.x = 0f;
            }

            if (Mathf.Abs(direction.y) < 0.18f)
            {
                direction.y = screenPoint.y >= center.y ? 0.22f : -0.22f;
            }

            targets.Add(new UiExitTarget(
                canvasGroup,
                rectTransform,
                direction.normalized * Mathf.Max(0f, hudExitSlidePixels)));
        }

        private static void HideCombatHudRootsImmediate()
        {
            for (int i = 0; i < CombatHudExitRootNames.Length; i++)
            {
                GameObject root = GameObject.Find(CombatHudExitRootNames[i]);
                if (root == null)
                {
                    continue;
                }

                Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
                for (int canvasIndex = 0; canvasIndex < canvases.Length; canvasIndex++)
                {
                    if (canvases[canvasIndex] != null)
                    {
                        canvases[canvasIndex].enabled = false;
                    }
                }

                root.SetActive(false);
            }
        }

        private static bool ConfigureStageClearPresenters(
            Scene clearScene,
            int resolvedSortOrder,
            StageRunResultSummary summary)
        {
            if (!clearScene.IsValid() || !clearScene.isLoaded)
            {
                return false;
            }

            bool configuredAny = false;
            GameObject[] roots = clearScene.GetRootGameObjects();
            PromoteCanvases(roots, resolvedSortOrder);
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                StageClearScreenPresenter[] presenters =
                    roots[rootIndex].GetComponentsInChildren<StageClearScreenPresenter>(true);
                for (int presenterIndex = 0; presenterIndex < presenters.Length; presenterIndex++)
                {
                    StageClearScreenPresenter presenter = presenters[presenterIndex];
                    presenter.ConfigureResult(summary);
                    bool configured = summary == null
                        || (presenter.IsConfigured
                            && ReferenceEquals(presenter.ResultSummary, summary));
                    if (configured)
                    {
                        presenter.PlayEntrance();
                        configuredAny = true;
                    }
                }
            }

            return configuredAny;
        }

        private static void PromoteCanvases(GameObject[] roots, int resolvedSortOrder)
        {
            int canvasIndex = 0;
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Canvas[] canvases = roots[rootIndex].GetComponentsInChildren<Canvas>(true);
                for (int i = 0; i < canvases.Length; i++)
                {
                    Canvas canvas = canvases[i];
                    if (canvas == null)
                    {
                        continue;
                    }

                    canvas.overrideSorting = true;
                    canvas.sortingOrder = resolvedSortOrder + canvasIndex;
                    canvasIndex++;
                }
            }
        }

        private void PrepareCombatAfterClear()
        {
            if (combatLocked)
            {
                return;
            }

            combatLocked = true;

            CombatHealth playerHealth = FindHealthByTeam(DamageTeam.Player);
            playerHealth?.SetInvulnerableUntil(Time.time + 3600f);
            DismissCombatSessionOverlays();
            DisableEncounterFailureHooks();
            StopHostileCombat();
        }

        private void FreezeWorldForResult()
        {
            if (!combatLocked)
            {
                PrepareCombatAfterClear();
            }

            if (worldTimeScaleFrozen)
            {
                return;
            }

            // The aftermath gate intentionally permits the lethal hit-stop to
            // settle before it validates scale 1. Snapshot only when this
            // overlay actually takes scale ownership, never at terminal commit.
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            worldTimeScaleFrozen = true;
        }

        private void RestoreCombatTimeScale()
        {
            if (!combatLocked)
            {
                return;
            }

            if (worldTimeScaleFrozen && Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = previousTimeScale;
            }

            worldTimeScaleFrozen = false;
            combatLocked = false;
        }

        private void CancelAftermathAndRelease(string reason)
        {
            UnsubscribeAftermathSignals();
            if (bossTerminalAftermath != null
                && bossTerminalAftermath.IsStarted
                && (!bossTerminalAftermath.IsComplete
                    || bossTerminalAftermath.InputLeaseActive))
            {
                bossTerminalAftermath.CancelAndRelease(reason);
            }

            aftermathGateAttached = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetResultScenePreloadLease()
        {
            ResultScenePreloadLease.Reset();
        }

#if UNITY_INCLUDE_TESTS
        public static bool IsResultScenePreloadLeaseBusyForTests =>
            ResultScenePreloadLease.IsBusy;
#endif

        private static class ResultScenePreloadLease
        {
            private static int nextToken;
            private static int activeToken;
            private static int activeSceneHandle;
            private static string activeSceneName = string.Empty;
            private static bool cancellationPending;
            private static bool sceneLoadedSubscribed;
            private static AsyncOperation unloadOperation;

            public static bool IsBusy => activeToken != 0;

            public static bool TryAcquire(
                string sceneName,
                out int token,
                out string error)
            {
                token = 0;
                error = string.Empty;
                if (activeToken != 0)
                {
                    error = "Another authored result-scene preload lease is still active.";
                    return false;
                }

                unchecked
                {
                    nextToken++;
                    if (nextToken <= 0)
                    {
                        nextToken = 1;
                    }
                }

                activeToken = nextToken;
                activeSceneHandle = 0;
                activeSceneName = sceneName ?? string.Empty;
                cancellationPending = false;
                unloadOperation = null;
                SubscribeSceneLoaded();
                token = activeToken;
                return true;
            }

            public static void RecordRequestedScene(int token, Scene scene)
            {
                if (!IsOwned(token) || !scene.IsValid())
                {
                    return;
                }

                activeSceneHandle = scene.handle;
            }

            public static bool IsOwned(int token)
            {
                return token != 0 && token == activeToken;
            }

            public static bool TryResolve(int token, out Scene scene)
            {
                scene = default;
                if (!IsOwned(token))
                {
                    return false;
                }

                scene = ResolveActiveScene();
                return scene.IsValid();
            }

            public static void RetryCancellation()
            {
                if (activeToken == 0
                    || !cancellationPending
                    || unloadOperation != null)
                {
                    return;
                }

                Scene scene = ResolveActiveScene();
                if (scene.IsValid() && scene.isLoaded)
                {
                    BeginUnload(activeToken, scene);
                }
            }

            public static void Cancel(int token)
            {
                if (!IsOwned(token))
                {
                    return;
                }

                cancellationPending = true;
                Scene scene = ResolveActiveScene();
                if (scene.IsValid() && scene.isLoaded)
                {
                    BeginUnload(token, scene);
                }
                else
                {
                    SubscribeSceneLoaded();
                }
            }

            public static void Relinquish(int token)
            {
                if (IsOwned(token) && !cancellationPending)
                {
                    Clear(token);
                }
            }

            public static void Abandon(int token)
            {
                if (IsOwned(token))
                {
                    Clear(token);
                }
            }

            public static void Reset()
            {
                UnsubscribeSceneLoaded();
                activeToken = 0;
                activeSceneHandle = 0;
                activeSceneName = string.Empty;
                cancellationPending = false;
                unloadOperation = null;
            }

            private static void SubscribeSceneLoaded()
            {
                if (sceneLoadedSubscribed)
                {
                    return;
                }

                SceneManager.sceneLoaded += HandleSceneLoaded;
                sceneLoadedSubscribed = true;
            }

            private static void UnsubscribeSceneLoaded()
            {
                if (!sceneLoadedSubscribed)
                {
                    return;
                }

                SceneManager.sceneLoaded -= HandleSceneLoaded;
                sceneLoadedSubscribed = false;
            }

            private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (activeToken == 0
                    || !string.Equals(scene.name, activeSceneName, StringComparison.Ordinal)
                    || (activeSceneHandle != 0 && scene.handle != activeSceneHandle))
                {
                    return;
                }

                activeSceneHandle = scene.handle;
                UnsubscribeSceneLoaded();
                if (cancellationPending)
                {
                    BeginUnload(activeToken, scene);
                }
            }

            private static Scene ResolveActiveScene()
            {
                if (activeSceneHandle != 0)
                {
                    for (int sceneIndex = 0;
                        sceneIndex < SceneManager.sceneCount;
                        sceneIndex++)
                    {
                        Scene byHandle = SceneManager.GetSceneAt(sceneIndex);
                        if (byHandle.IsValid() && byHandle.handle == activeSceneHandle)
                        {
                            return byHandle;
                        }
                    }

                    return default;
                }

                return SceneManager.GetSceneByName(activeSceneName);
            }

            private static void BeginUnload(int token, Scene scene)
            {
                if (!IsOwned(token)
                    || unloadOperation != null
                    || (activeSceneHandle != 0 && scene.handle != activeSceneHandle))
                {
                    return;
                }

                try
                {
                    AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
                    if (operation == null)
                    {
                        Debug.LogError(
                            "[OlympusStageClearOverlay] The owned result-scene preload did not return an unload operation.");
                        return;
                    }

                    unloadOperation = operation;
                    operation.completed += _ => CompleteUnload(token);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "[OlympusStageClearOverlay] Failed to unload the owned result-scene preload: "
                        + exception.Message);
                }
            }

            private static void CompleteUnload(int token)
            {
                if (IsOwned(token))
                {
                    Clear(token);
                }
            }

            private static void Clear(int token)
            {
                if (!IsOwned(token))
                {
                    return;
                }

                UnsubscribeSceneLoaded();
                activeToken = 0;
                activeSceneHandle = 0;
                activeSceneName = string.Empty;
                cancellationPending = false;
                unloadOperation = null;
            }
        }

        private static bool IsBossTerminalClear(StageRunResultSummary summary)
        {
            return summary != null
                && summary.Outcome == StageRouteOutcome.Clear
                && summary.OutcomeFact != null
                && summary.OutcomeFact.OutcomeDisposition == StageOutcomeDisposition.Clear
                && summary.OutcomeFact.ClearReason == StageClearReason.BossTerminal;
        }

        private static CombatHealth FindHealthByTeam(DamageTeam team)
        {
            CombatHealth[] healthComponents = FindObjectsByType<CombatHealth>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < healthComponents.Length; i++)
            {
                if (healthComponents[i] != null && healthComponents[i].Team == team)
                {
                    return healthComponents[i];
                }
            }

            return null;
        }

        private static void DismissCombatSessionOverlays()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ICombatSessionOverlay overlay)
                {
                    overlay.DismissForStageClear();
                }
            }
        }

        private static void DisableEncounterFailureHooks()
        {
            CombatEncounterController[] encounters = FindObjectsByType<CombatEncounterController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < encounters.Length; i++)
            {
                if (encounters[i] != null)
                {
                    encounters[i].enabled = false;
                }
            }
        }

        private static void StopHostileCombat()
        {
            BossBarrageEmitter[] barrageEmitters = FindObjectsByType<BossBarrageEmitter>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < barrageEmitters.Length; i++)
            {
                barrageEmitters[i]?.SetFiringEnabled(false);
            }

            BossBasicFireEmitter[] basicFireEmitters = FindObjectsByType<BossBasicFireEmitter>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < basicFireEmitters.Length; i++)
            {
                basicFireEmitters[i]?.SetFiringEnabled(false);
            }

            BossPressureActionDirector[] actionDirectors = FindObjectsByType<BossPressureActionDirector>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < actionDirectors.Length; i++)
            {
                actionDirectors[i]?.SetActionsEnabled(false);
            }

            BossPressurePositionController[] positionControllers = FindObjectsByType<BossPressurePositionController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < positionControllers.Length; i++)
            {
                positionControllers[i]?.SetMovementEnabled(false);
            }

            EnemySummonPacingDirector[] pacingDirectors = FindObjectsByType<EnemySummonPacingDirector>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < pacingDirectors.Length; i++)
            {
                pacingDirectors[i]?.SetPacingEnabled(false);
            }

            BossPressureCostLadder[] costLadders = FindObjectsByType<BossPressureCostLadder>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < costLadders.Length; i++)
            {
                costLadders[i]?.SetGainEnabled(false);
            }
        }

        private static float EaseOutCubic(float value)
        {
            float t = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private sealed class UiExitTarget
        {
            private readonly CanvasGroup canvasGroup;
            private readonly RectTransform rectTransform;
            private readonly Vector2 startPosition;
            private readonly Vector2 endPosition;

            public UiExitTarget(
                CanvasGroup canvasGroup,
                RectTransform rectTransform,
                Vector2 exitOffset)
            {
                this.canvasGroup = canvasGroup;
                this.rectTransform = rectTransform;
                startPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
                endPosition = startPosition + exitOffset;
            }

            public void SetInteractive(bool interactive)
            {
                if (canvasGroup == null)
                {
                    return;
                }

                canvasGroup.interactable = interactive;
                canvasGroup.blocksRaycasts = interactive;
            }

            public void Apply(float progress)
            {
                float t = Mathf.Clamp01(progress);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, t);
                }
            }
        }
    }
}
