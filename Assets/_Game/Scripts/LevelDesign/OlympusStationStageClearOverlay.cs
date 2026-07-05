using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.AI;
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

        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private int sortOrder = 7000;

        private BossBarragePocketReviewOwner subscribedOwner;
        private CombatHealth subscribedBossHealth;
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

        private void Awake()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            RestoreCombatTimeScale();
        }

        private void Update()
        {
            if (shown)
            {
                return;
            }

            if (pocketReviewOwner == null || bossHealth == null)
            {
                ResolveReferences();
                Subscribe();
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

            StopActiveProjectiles();
            DismissHostileSummons();
            DisableHostileAiAgents();
        }

        private static void StopActiveProjectiles()
        {
            BossBarrageProjectile[] bossProjectiles = FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < bossProjectiles.Length; i++)
            {
                if (bossProjectiles[i] != null && bossProjectiles[i].IsActive)
                {
                    bossProjectiles[i].Deactivate();
                }
            }

            LaneActionProjectile[] laneProjectiles = FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < laneProjectiles.Length; i++)
            {
                if (laneProjectiles[i] != null && laneProjectiles[i].IsActive)
                {
                    laneProjectiles[i].Deactivate();
                }
            }
        }

        private static void DismissHostileSummons()
        {
            SummonFrontlineProxy[] proxies = FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < proxies.Length; i++)
            {
                SummonFrontlineProxy proxy = proxies[i];
                if (proxy != null
                    && proxy.IsActive
                    && !CombatTeamUtility.IsPlayerSide(proxy.OwnerTeam))
                {
                    proxy.Deactivate(SummonFrontlineProxyExitReason.Recalled);
                }
            }
        }

        private static void DisableHostileAiAgents()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour is not ICombatAiAgent agent
                    || agent.SelfHealth == null
                    || CombatTeamUtility.IsPlayerSide(agent.SelfHealth.Team))
                {
                    continue;
                }

                behaviour.enabled = false;
            }
        }
    }
}
