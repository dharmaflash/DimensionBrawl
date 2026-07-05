using System;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Test;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] private int sortOrder = 7000;

        private BossBarragePocketReviewOwner subscribedOwner;
        private CombatHealth subscribedBossHealth;
        private bool shown;

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
            if (TryShowAuthoredStageClearScene())
            {
                return;
            }

            GameObject root = new GameObject("StageClearOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560f, 1440f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateImage("Dim", root.transform, new Color(0f, 0f, 0f, 0.68f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform panel = CreateImage(
                "StageClearPanel",
                root.transform,
                new Color(0.02f, 0.028f, 0.045f, 0.96f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(980f, 560f));
            panel.anchoredPosition = Vector2.zero;

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            CreateText(panel, "Title", "STAGE CLEAR", font, 72, new Color(0.76f, 0.94f, 1f, 1f), new Vector2(0f, 120f), new Vector2(900f, 120f));
            CreateText(panel, "Subtitle", "Olympus Station secured", font, 34, new Color(0.82f, 0.88f, 0.94f, 0.9f), new Vector2(0f, 38f), new Vector2(860f, 70f));

            Button retryButton = CreateButton(panel, "RetryButton", "RETRY", font, new Vector2(-190f, -140f));
            Button lobbyButton = CreateButton(panel, "LobbyButton", "LOBBY", font, new Vector2(190f, -140f));
            retryButton.onClick.AddListener(() => LoadSingleScene(CombatSceneName, RetryScenePath));
            lobbyButton.onClick.AddListener(() => LoadSingleScene(LobbySceneName, LobbyScenePath));
        }

        private bool TryShowAuthoredStageClearScene()
        {
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
                }

                return ConfigureStageClearPresenters(clearScene);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ConfigureStageClearPresenters(Scene clearScene)
        {
            if (!clearScene.IsValid() || !clearScene.isLoaded)
            {
                return false;
            }

            bool configuredAny = false;
            GameObject[] roots = clearScene.GetRootGameObjects();
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

        private static void LoadSingleScene(string sceneName, string scenePath)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif

            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }

        private static RectTransform CreateImage(
            string name,
            Transform parent,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = Vector2.zero;
            obj.GetComponent<Image>().color = color;
            return rect;
        }

        private static void CreateText(
            Transform parent,
            string name,
            string text,
            Font font,
            int fontSize,
            Color color,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Text label = obj.GetComponent<Text>();
            label.text = text;
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 20;
            label.resizeTextMaxSize = fontSize;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font, Vector2 anchoredPosition)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(300f, 86f);

            Image image = obj.GetComponent<Image>();
            image.color = new Color(0.08f, 0.42f, 0.72f, 0.96f);

            Button button = obj.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.08f, 0.42f, 0.72f, 0.96f);
            colors.highlightedColor = new Color(0.15f, 0.62f, 0.92f, 1f);
            colors.pressedColor = new Color(0.04f, 0.28f, 0.52f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            CreateText(obj.transform, "Label", label, font, 34, Color.white, Vector2.zero, new Vector2(260f, 68f));
            return button;
        }
    }
}
