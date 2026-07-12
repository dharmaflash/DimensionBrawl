using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(1600)]
    [DisallowMultipleComponent]
    public sealed class OlympusStationStageClearOverlay : MonoBehaviour
    {
        private const string CombatSceneName = "OlympusStationCombatStage";
        private const string RetryScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string ClearUiSceneName = "UI_StageClearTest";
        private const string ClearUiScenePath = "Assets/_Game/Scenes/Experiments/UI_StageClearTest.unity";
        private const string LobbySceneName = "UI_LobbyTest";
        private const string LobbyScenePath = "Assets/_Game/Scenes/UI/UI_LobbyTest.unity";
        private const string BossObjectName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string StageClearPresenterTypeName = "DimensionBrawl.UI.StageClear.UIStageClearTestPresenter";
        private const int ReferenceResolveAttemptLimit = 40;
        private const float ReferenceResolveIntervalSeconds = 0.1f;
        private static readonly string[] CombatHudExitRootNames =
        {
            "BossBarrageLaneReview_CombatHudCanvas",
            "PF_UI_CombatHud",
            "BossBarrageLaneReview_DebugHud",
            "PF_UI_CombatHudPresentation"
        };

        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private int sortOrder = 7000;
        [SerializeField, Min(0f)] private float combatHudExitSeconds = 0.42f;
        [SerializeField, Min(0f)] private float postBossDefeatHoldSeconds = 1.1f;
        [SerializeField, Min(0f)] private float hudExitSlidePixels = 128f;

        private BossBarragePocketReviewOwner subscribedOwner;
        private CombatHealth subscribedBossHealth;
        private Coroutine referenceResolveRoutine;
        private Coroutine stageClearRoutine;
        private bool shown;
        private bool combatLocked;
        private float previousTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureForScene(scene);
        }

        private static void EnsureForScene(Scene scene)
        {
            if (scene.name != CombatSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<OlympusStationStageClearOverlay>() != null)
            {
                return;
            }

            new GameObject(nameof(OlympusStationStageClearOverlay)).AddComponent<OlympusStationStageClearOverlay>();
        }

        private void OnEnable()
        {
            BindAndEvaluateClearState();
            StartReferenceResolveRoutineIfNeeded();
        }

        private void OnDisable()
        {
            StopReferenceResolveRoutine();
            Unsubscribe();
            RestoreCombatTimeScale();
        }

        private void BindAndEvaluateClearState()
        {
            ResolveReferences();
            Subscribe();
            EvaluateClearState();
        }

        private void EvaluateClearState()
        {
            if (shown)
            {
                return;
            }

            if (pocketReviewOwner != null && pocketReviewOwner.IsCleared)
            {
                ShowClearOverlay();
                return;
            }

            if (bossHealth != null && !bossHealth.IsAlive)
            {
                ShowClearOverlay();
            }
        }

        private void StartReferenceResolveRoutineIfNeeded()
        {
            if (shown
                || referenceResolveRoutine != null
                || (pocketReviewOwner != null && bossHealth != null))
            {
                return;
            }

            referenceResolveRoutine = StartCoroutine(ResolveReferencesWhenReady());
        }

        private IEnumerator ResolveReferencesWhenReady()
        {
            var retryDelay = new WaitForSecondsRealtime(ReferenceResolveIntervalSeconds);
            for (int attempt = 0; attempt < ReferenceResolveAttemptLimit && !shown; attempt++)
            {
                yield return retryDelay;
                if (shown)
                {
                    break;
                }

                BindAndEvaluateClearState();
                if (pocketReviewOwner != null && bossHealth != null)
                {
                    break;
                }
            }

            referenceResolveRoutine = null;
        }

        private void StopReferenceResolveRoutine()
        {
            if (referenceResolveRoutine == null)
            {
                return;
            }

            StopCoroutine(referenceResolveRoutine);
            referenceResolveRoutine = null;
        }

        private void ResolveReferences()
        {
            if (pocketReviewOwner == null)
            {
                pocketReviewOwner = FindFirstObjectByType<BossBarragePocketReviewOwner>();
            }

            if (bossHealth == null)
            {
                GameObject bossObject = GameObject.Find(BossObjectName);
                if (bossObject != null)
                {
                    bossHealth = bossObject.GetComponent<CombatHealth>();
                }
            }

            if (bossHealth == null)
            {
                CombatHealth[] healthComponents = FindObjectsByType<CombatHealth>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
                for (int i = 0; i < healthComponents.Length; i++)
                {
                    CombatHealth candidate = healthComponents[i];
                    if (candidate != null && candidate.Team == DamageTeam.Enemy)
                    {
                        bossHealth = candidate;
                    }
                    else if (candidate != null && candidate.Team == DamageTeam.Player)
                    {
                        playerHealth = candidate;
                    }

                    if (bossHealth != null && playerHealth != null)
                    {
                        break;
                    }
                }
            }

            if (playerHealth == null)
            {
                CombatHealth[] healthComponents = FindObjectsByType<CombatHealth>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
                for (int i = 0; i < healthComponents.Length; i++)
                {
                    CombatHealth candidate = healthComponents[i];
                    if (candidate != null && candidate.Team == DamageTeam.Player)
                    {
                        playerHealth = candidate;
                        break;
                    }
                }
            }
        }

        private void Subscribe()
        {
            if (pocketReviewOwner != null && subscribedOwner != pocketReviewOwner)
            {
                if (subscribedOwner != null)
                {
                    subscribedOwner.PocketCleared -= HandlePocketCleared;
                }

                subscribedOwner = pocketReviewOwner;
                subscribedOwner.PocketCleared -= HandlePocketCleared;
                subscribedOwner.PocketCleared += HandlePocketCleared;
            }

            if (bossHealth != null && subscribedBossHealth != bossHealth)
            {
                if (subscribedBossHealth != null)
                {
                    subscribedBossHealth.Died -= HandleBossDied;
                }

                subscribedBossHealth = bossHealth;
                subscribedBossHealth.Died -= HandleBossDied;
                subscribedBossHealth.Died += HandleBossDied;
            }
        }

        private void Unsubscribe()
        {
            if (subscribedOwner != null)
            {
                subscribedOwner.PocketCleared -= HandlePocketCleared;
                subscribedOwner = null;
            }

            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Died -= HandleBossDied;
                subscribedBossHealth = null;
            }
        }

        private void HandlePocketCleared()
        {
            ShowClearOverlay();
        }

        private void HandleBossDied()
        {
            ShowClearOverlay();
        }

        private void ShowClearOverlay()
        {
            if (shown)
            {
                return;
            }

            shown = true;
            LockCombatAfterClear();

            if (stageClearRoutine == null && isActiveAndEnabled)
            {
                stageClearRoutine = StartCoroutine(ShowAuthoredStageClearSceneRoutine());
            }
        }

        private IEnumerator ShowAuthoredStageClearSceneRoutine()
        {
            yield return PlayCombatHudExitRoutine();

            if (postBossDefeatHoldSeconds > 0f)
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
                    clearScene = EditorSceneManager.LoadSceneInPlayMode(
                        ClearUiScenePath,
                        new LoadSceneParameters(LoadSceneMode.Additive));
#else
                    SceneManager.LoadScene(ClearUiSceneName, LoadSceneMode.Additive);
                    clearScene = SceneManager.GetSceneByName(ClearUiSceneName);
#endif
                    sceneLoadRequested = true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[OlympusStationStageClearOverlay] Failed to load authored clear UI scene: {exception.Message}");
                stageClearRoutine = null;
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
                Scene clearScene = SceneManager.GetSceneByName(ClearUiSceneName);
                configured = ConfigureStageClearPresenters(clearScene, sortOrder);
                if (configured)
                {
                    break;
                }

                yield return null;
            }

            if (!configured)
            {
                Debug.LogError("[OlympusStationStageClearOverlay] Authored stage clear scene loaded, but no UIStageClearTestPresenter was configured. Runtime fallback UI is intentionally disabled.");
            }

            stageClearRoutine = null;
        }

        private IEnumerator PlayCombatHudExitRoutine()
        {
            List<UiExitTarget> targets = CollectCombatHudExitTargets();
            if (targets.Count <= 0)
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
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].Apply(1f);
                }

                HideCombatHudRootsImmediate();
                yield break;
            }

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float eased = EaseOutCubic(elapsed / duration);
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].Apply(eased);
                }

                yield return null;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Apply(1f);
            }

            HideCombatHudRootsImmediate();
        }

        private List<UiExitTarget> CollectCombatHudExitTargets()
        {
            List<UiExitTarget> targets = new List<UiExitTarget>(8);
            HashSet<Transform> seen = new HashSet<Transform>();

            for (int i = 0; i < CombatHudExitRootNames.Length; i++)
            {
                GameObject root = GameObject.Find(CombatHudExitRootNames[i]);
                AddUiExitTarget(root != null ? root.transform : null, targets, seen);
            }

            BossBarrageLaneReviewOverlayHud[] overlayHuds = FindObjectsByType<BossBarrageLaneReviewOverlayHud>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < overlayHuds.Length; i++)
            {
                Transform target = overlayHuds[i] != null ? overlayHuds[i].transform : null;
                AddUiExitTarget(target, targets, seen);
            }

            return targets;
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

        private void AddUiExitTarget(Transform target, List<UiExitTarget> targets, HashSet<Transform> seen)
        {
            if (target == null || !seen.Add(target))
            {
                return;
            }

            RectTransform rectTransform = target as RectTransform;
            if (rectTransform == null)
            {
                return;
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            Vector2 exitOffset = CalculateHudExitOffset(rectTransform);
            targets.Add(new UiExitTarget(canvasGroup, rectTransform, exitOffset));
        }

        private Vector2 CalculateHudExitOffset(RectTransform rectTransform)
        {
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

            return direction.normalized * Mathf.Max(0f, hudExitSlidePixels);
        }

        private static bool ConfigureStageClearPresenters(Scene clearScene, int resolvedSortOrder)
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
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    MonoBehaviour presenter = behaviours[behaviourIndex];
                    if (presenter == null || presenter.GetType().FullName != StageClearPresenterTypeName)
                    {
                        continue;
                    }

                    ConfigurePresenterByReflection(presenter);
                    configuredAny = true;
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
                for (int canvasIterator = 0; canvasIterator < canvases.Length; canvasIterator++)
                {
                    Canvas canvas = canvases[canvasIterator];
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

        private static void ConfigurePresenterByReflection(MonoBehaviour presenter)
        {
            Type presenterType = presenter.GetType();
            MethodInfo configureRoutes = presenterType.GetMethod(
                "ConfigureRoutes",
                BindingFlags.Instance | BindingFlags.Public);
            configureRoutes?.Invoke(
                presenter,
                new object[] { CombatSceneName, RetryScenePath, LobbySceneName, LobbyScenePath });

            MethodInfo playEntrance = presenterType.GetMethod(
                "PlayEntrance",
                BindingFlags.Instance | BindingFlags.Public);
            playEntrance?.Invoke(presenter, Array.Empty<object>());
        }

        private static float EaseOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private void LockCombatAfterClear()
        {
            if (combatLocked)
            {
                return;
            }

            combatLocked = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            ResolveReferences();
            playerHealth?.SetInvulnerableUntil(Time.time + 3600f);
            DisableCombatResultOverlays();
            DisableEncounterFailureHooks();
            StopHostileCombat();
        }

        private void RestoreCombatTimeScale()
        {
            if (!combatLocked)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            combatLocked = false;
        }

        private static void DisableCombatResultOverlays()
        {
            BossBarrageLaneReviewOverlayHud[] overlays = FindObjectsByType<BossBarrageLaneReviewOverlayHud>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < overlays.Length; i++)
            {
                if (overlays[i] != null)
                {
                    overlays[i].enabled = false;
                }
            }
        }

        private static void DisableEncounterFailureHooks()
        {
            ActionFoundationTestEncounter[] encounters = FindObjectsByType<ActionFoundationTestEncounter>(
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

        private sealed class UiExitTarget
        {
            private readonly CanvasGroup canvasGroup;
            private readonly RectTransform rectTransform;
            private readonly Vector2 startPosition;
            private readonly Vector2 endPosition;

            public UiExitTarget(CanvasGroup canvasGroup, RectTransform rectTransform, Vector2 exitOffset)
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

            public void Apply(float t)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - Mathf.Clamp01(t);
                }

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, Mathf.Clamp01(t));
                }
            }
        }
    }
}
