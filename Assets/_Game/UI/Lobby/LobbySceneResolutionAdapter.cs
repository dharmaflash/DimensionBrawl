using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DefaultExecutionOrder(-2000)]
    [DisallowMultipleComponent]
    public sealed class LobbySceneResolutionAdapter : MonoBehaviour
    {
        private const string LobbySceneName = "UI_LobbyTest";

        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private Vector2 referenceResolution = new Vector2(2560f, 1440f);
        [SerializeField] private bool adjustBackgroundFrames = true;

        private int lastScreenWidth;
        private int lastScreenHeight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ApplyToScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyToScene(scene);
        }

        private static void ApplyToScene(Scene scene)
        {
            if (scene.name != LobbySceneName)
            {
                return;
            }

            CanvasScaler[] scalers = FindObjectsByType<CanvasScaler>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            bool appliedAny = false;
            for (int i = 0; i < scalers.Length; i++)
            {
                CanvasScaler scaler = scalers[i];
                if (scaler == null || scaler.gameObject.scene != scene)
                {
                    continue;
                }

                LobbySceneResolutionAdapter adapter = scaler.GetComponent<LobbySceneResolutionAdapter>();
                if (adapter == null)
                {
                    adapter = scaler.gameObject.AddComponent<LobbySceneResolutionAdapter>();
                }

                adapter.canvasScaler = scaler;
                adapter.ApplyNow();
                appliedAny = true;
            }

            if (!appliedAny)
            {
                Debug.LogWarning($"No CanvasScaler found for lobby resolution adaptation in scene: {scene.name}");
            }
        }

        private void Awake()
        {
            canvasScaler ??= GetComponent<CanvasScaler>();
            ApplyNow();
        }

        private void OnEnable()
        {
            ApplyNow();
        }

        private void LateUpdate()
        {
            if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            {
                return;
            }

            ApplyNow();
        }

        public void ApplyNow()
        {
            if (gameObject.scene.name != LobbySceneName)
            {
                return;
            }

            canvasScaler ??= GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                return;
            }

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            Vector2 resolvedReference = new Vector2(
                Mathf.Max(1f, referenceResolution.x),
                Mathf.Max(1f, referenceResolution.y));
            float screenAspect = Screen.height > 0 ? Screen.width / (float)Screen.height : resolvedReference.x / resolvedReference.y;
            float referenceAspect = resolvedReference.x / resolvedReference.y;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = resolvedReference;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = screenAspect >= referenceAspect ? 0f : 1f;

            if (adjustBackgroundFrames)
            {
                ApplyFullBleedBackgrounds();
            }
        }

        private void ApplyFullBleedBackgrounds()
        {
            RectTransform[] rects = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect == null || !IsLobbyBackgroundFrame(rect))
                {
                    continue;
                }

                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);

                AspectRatioFitter fitter = rect.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                {
                    fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                    fitter.aspectRatio = 16f / 9f;
                }
            }
        }

        private static bool IsLobbyBackgroundFrame(RectTransform rect)
        {
            return rect.name == "LobbyBackgroundFrame"
                || rect.name == "LobbyBackgroundCover"
                || rect.name == "Dimension_Lobby_UI_0000_Background";
        }
    }
}
