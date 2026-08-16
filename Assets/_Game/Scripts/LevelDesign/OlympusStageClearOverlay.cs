using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
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
                && resultSummary != null
                && string.IsNullOrEmpty(presentedResultDigest);
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

            resultSummary = summary;
            presentedResultDigest = string.Empty;
            LastPresentationError = string.Empty;
            LastAftermathError = string.Empty;
            shown = true;
            aftermathGateAttached = false;
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
                if (!aftermathGateAttached)
                {
                    FreezeWorldForResult();
                }

                stageClearRoutine = StartCoroutine(
                    RunPresentationSafely(aftermathGateAttached));
                return true;
            }
            catch (Exception exception)
            {
                shown = false;
                error = "The result overlay could not start safely: " + exception.Message;
                LastPresentationError = error;
                CancelAftermathAndRelease(error);
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
                float aftermathTimeoutAt = Time.realtimeSinceStartup
                    + remaining
                    + Mathf.Max(0.1f, bossTerminalAftermathWaitSlackSeconds);
                while (bossTerminalAftermath != null
                    && !bossTerminalAftermath.IsComplete
                    && !bossTerminalAftermath.IsCancelled)
                {
                    if (Time.realtimeSinceStartup > aftermathTimeoutAt)
                    {
                        FailPresentation("The authored Station boss-terminal aftermath timed out.");
                        yield break;
                    }

                    yield return null;
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

                FreezeWorldForResult();
                bossTerminalAftermath.ReleaseInputLeaseForResultSurface();
                if (bossTerminalAftermath.InputLeaseActive)
                {
                    LastAftermathError = bossTerminalAftermath.LastError;
                    FailPresentation(
                        "The authored Station boss-terminal aftermath input lease did not release exactly.");
                    yield break;
                }

                aftermathGateAttached = false;
            }
            else if (postBossDefeatHoldSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(postBossDefeatHoldSeconds);
            }

            bool sceneLoadRequested = false;
            try
            {
                Scene clearScene = SceneManager.GetSceneByName(ClearUiSceneName);
                if (!clearScene.IsValid() || !clearScene.isLoaded)
                {
#if UNITY_EDITOR
                    EditorSceneManager.LoadSceneInPlayMode(
                        ClearUiScenePath,
                        new LoadSceneParameters(LoadSceneMode.Additive));
#else
                    SceneManager.LoadScene(ClearUiSceneName, LoadSceneMode.Additive);
#endif
                    sceneLoadRequested = true;
                }
            }
            catch (Exception exception)
            {
                FailPresentation($"Failed to load authored clear UI scene: {exception.Message}");
                yield break;
            }

            if (sceneLoadRequested)
            {
                yield return null;
            }

            bool configured = false;
            float timeoutAt = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup <= timeoutAt)
            {
                configured = ConfigureStageClearPresenters(
                    SceneManager.GetSceneByName(ClearUiSceneName),
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

            presentedResultDigest = resultSummary?.ResultSummaryDigest ?? string.Empty;
            LastPresentationError = string.Empty;
            stageClearRoutine = null;
            InvokePresentationSucceededSafely(resultSummary);
        }

        private void FailPresentation(string error, bool logAsError = true)
        {
            LastPresentationError = error ?? string.Empty;
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

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
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
            if (bossTerminalAftermath != null
                && bossTerminalAftermath.IsStarted
                && (!bossTerminalAftermath.IsComplete
                    || bossTerminalAftermath.InputLeaseActive))
            {
                bossTerminalAftermath.CancelAndRelease(reason);
            }

            aftermathGateAttached = false;
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
