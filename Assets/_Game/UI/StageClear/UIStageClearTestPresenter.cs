using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DimensionBrawl.UI.StageClear
{
    [DisallowMultipleComponent]
    public sealed class UIStageClearTestPresenter : MonoBehaviour
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform motionRoot;
        [Header("Clear Audio")]
        [SerializeField] private AudioSource clearBgmSource;
        [SerializeField] private AudioClip clearBgmClip;
        [SerializeField, Range(0f, 1f)] private float clearBgmVolume = 0.9f;
        [SerializeField] private string retrySceneName = "OlympusStationCombatStage";
        [SerializeField] private string retryScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        [SerializeField] private string lobbySceneName = "UI_LobbyTest";
        [SerializeField] private string lobbyScenePath = "Assets/_Game/Scenes/UI/UI_LobbyTest.unity";
        [SerializeField] private bool playEntranceOnEnable = true;
        [SerializeField, Min(0f)] private float entranceDelaySeconds = 0.02f;
        [SerializeField, Min(0.01f)] private float entranceDurationSeconds = 0.42f;
        [SerializeField, Range(0.5f, 1f)] private float entranceStartScale = 0.94f;
        [SerializeField] private Vector2 entranceOffset = new Vector2(96f, -8f);

        private Coroutine entranceRoutine;
        private RectTransform targetRect;
        private bool hasEntranceBaseTransform;
        private Vector2 entranceBasePosition;
        private Vector3 entranceBaseScale;
        private bool clearBgmPlayed;

        public int RetryClickCount { get; private set; }
        public int NextStageClickCount { get; private set; }

        public void ConfigureRoutes(
            string newRetrySceneName,
            string newRetryScenePath,
            string newLobbySceneName,
            string newLobbyScenePath)
        {
            retrySceneName = newRetrySceneName;
            retryScenePath = newRetryScenePath;
            lobbySceneName = newLobbySceneName;
            lobbyScenePath = newLobbyScenePath;
        }

        private void Awake()
        {
            ResolveButtons();
            ResolveMotionTargets();
        }

        private void OnEnable()
        {
            ResolveButtons();
            ResolveMotionTargets();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(HandleRetryClicked);
            }

            if (nextStageButton != null)
            {
                nextStageButton.onClick.AddListener(HandleNextStageClicked);
            }

            if (playEntranceOnEnable)
            {
                PlayEntrance();
            }
        }

        private void OnDisable()
        {
            if (entranceRoutine != null)
            {
                StopCoroutine(entranceRoutine);
                entranceRoutine = null;
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryClicked);
            }

            if (nextStageButton != null)
            {
                nextStageButton.onClick.RemoveListener(HandleNextStageClicked);
            }
        }

        public void PlayEntrance()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (entranceRoutine != null)
            {
                StopCoroutine(entranceRoutine);
            }

            PlayClearBgmOnce();
            entranceRoutine = StartCoroutine(EntranceRoutine());
        }

        private void HandleRetryClicked()
        {
            RetryClickCount++;
            LoadSingleScene(retrySceneName, retryScenePath);
        }

        private void HandleNextStageClicked()
        {
            NextStageClickCount++;
            LoadSingleScene(lobbySceneName, lobbyScenePath);
        }

        private void ResolveButtons()
        {
            retryButton ??= FindButton("RetryButton", "RetryButtonHitArea", "Retry", "RetryStageButton");
            nextStageButton ??= FindButton("LobbyButton", "NextStageButtonHitArea", "NextStageButton", "Lobby", "NextStage");
        }

        private void ResolveMotionTargets()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            targetRect ??= transform as RectTransform;
            if (motionRoot == null)
            {
                motionRoot = FindRectTransform("StageClearResponsiveRoot");
            }

            targetRect = motionRoot != null ? motionRoot : targetRect;
            if (targetRect != null && !hasEntranceBaseTransform)
            {
                entranceBasePosition = targetRect.anchoredPosition;
                entranceBaseScale = targetRect.localScale;
                hasEntranceBaseTransform = true;
            }
        }

        private IEnumerator EntranceRoutine()
        {
            ResolveMotionTargets();

            Vector3 endScale = targetRect != null && hasEntranceBaseTransform ? entranceBaseScale : Vector3.one;
            Vector2 endPosition = targetRect != null && hasEntranceBaseTransform ? entranceBasePosition : Vector2.zero;
            Vector2 startPosition = endPosition + entranceOffset;
            Vector3 startScale = new Vector3(
                endScale.x * entranceStartScale,
                endScale.y * entranceStartScale,
                endScale.z);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (targetRect != null)
            {
                targetRect.anchoredPosition = startPosition;
                targetRect.localScale = startScale;
            }

            if (entranceDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(entranceDelaySeconds);
            }

            float duration = Mathf.Max(0.01f, entranceDurationSeconds);
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.Clamp01(1f - Mathf.Pow(1f - t, 3f));

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = eased;
                }

                if (targetRect != null)
                {
                    targetRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
                    targetRect.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (targetRect != null)
            {
                targetRect.anchoredPosition = endPosition;
                targetRect.localScale = endScale;
            }

            entranceRoutine = null;
        }

        private Button FindButton(params string[] names)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                string targetName = names[nameIndex];
                for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                {
                    Button candidate = buttons[buttonIndex];
                    if (candidate != null && candidate.name == targetName)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private void PlayClearBgmOnce()
        {
            if (clearBgmPlayed || clearBgmClip == null)
            {
                return;
            }

            clearBgmPlayed = true;
            AudioSource source = ResolveClearBgmSource();
            if (source == null)
            {
                return;
            }

            source.clip = null;
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.priority = 24;
            source.volume = Mathf.Clamp01(clearBgmVolume);
            source.PlayOneShot(clearBgmClip, Mathf.Clamp01(clearBgmVolume));
        }

        private AudioSource ResolveClearBgmSource()
        {
            if (clearBgmSource != null)
            {
                return clearBgmSource;
            }

            clearBgmSource = GetComponent<AudioSource>();
            if (clearBgmSource == null)
            {
                clearBgmSource = gameObject.AddComponent<AudioSource>();
            }

            return clearBgmSource;
        }

        private RectTransform FindRectTransform(string objectName)
        {
            RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform candidate = rectTransforms[i];
                if (candidate != null && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void LoadSingleScene(string sceneName, string scenePath)
        {
            Time.timeScale = 1f;

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
    }
}
