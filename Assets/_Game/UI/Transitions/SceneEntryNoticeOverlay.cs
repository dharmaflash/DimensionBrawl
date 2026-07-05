using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class SceneEntryNoticeOverlay : MonoBehaviour
    {
        [SerializeField] private SceneEntryNoticeProfile profile;
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Image dimImage;
        [SerializeField] private Image panelBackgroundImage;
        [SerializeField] private Image topLineImage;
        [SerializeField] private Image bottomLineImage;
        [SerializeField] private Image leftAccentImage;
        [SerializeField] private Image rightAccentImage;
        [SerializeField] private Image scanLineImage;
        [SerializeField] private Text eyebrowText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text leftStatusText;
        [SerializeField] private Text rightStatusText;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool replayOnEnable;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool pauseGameplayDuringNotice = true;
        [SerializeField] private bool blockPointerInputDuringNotice = true;
        [SerializeField] private UnityEvent noticeStarted;
        [SerializeField] private UnityEvent noticeFinished;

        private Coroutine playRoutine;
        private bool started;
        private bool visible;
        private string bodySource = string.Empty;
        private bool gameplayPauseApplied;
        private float previousTimeScale = 1f;

        private void Reset()
        {
            rootGroup = GetComponent<CanvasGroup>();
            panelRoot = transform as RectTransform;
        }

        private void Awake()
        {
            ApplyProfile();
            HideImmediate();
        }

        private void OnEnable()
        {
            if (started && replayOnEnable && playOnStart)
            {
                Replay();
            }
        }

        private void Start()
        {
            started = true;
            if (playOnStart)
            {
                Replay();
            }
        }

        private void OnDisable()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            ReleaseGameplayPause();
            SetPointerInputBlocked(false);
        }

        private void Update()
        {
            if (!visible)
            {
                return;
            }

            UpdateScanLine(TimeValue);
        }

        public void SetProfile(SceneEntryNoticeProfile noticeProfile)
        {
            profile = noticeProfile;
            ApplyProfile();
        }

        public void Replay()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }

            playRoutine = StartCoroutine(PlayRoutine());
        }

        public void HideImmediate()
        {
            visible = false;
            ReleaseGameplayPause();
            SetPointerInputBlocked(false);
            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.blocksRaycasts = false;
                rootGroup.interactable = false;
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.blocksRaycasts = false;
                panelGroup.interactable = false;
            }

            SetLineScale(topLineImage, 0f);
            SetLineScale(bottomLineImage, 0f);
            SetGraphicAlpha(scanLineImage, 0f);
            SetBodyReveal(0);
        }

        private IEnumerator PlayRoutine()
        {
            ApplyProfile();
            PrepareIntroState();
            AcquireGameplayPause();
            SetPointerInputBlocked(blockPointerInputDuringNotice);

            float startupDelay = profile != null ? profile.StartupDelaySeconds : 0.18f;
            if (startupDelay > 0f)
            {
                yield return WaitSeconds(startupDelay);
            }

            visible = true;
            noticeStarted?.Invoke();
            PlayStartBeep();

            float revealSeconds = Mathf.Max(0.01f, profile != null ? profile.RevealSeconds : 0.36f);
            for (float elapsed = 0f; elapsed < revealSeconds; elapsed += DeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / revealSeconds);
                ApplyReveal(EaseOut(t));
                SetBodyReveal(ResolveTypewriterCount(elapsed));
                yield return null;
            }

            ApplyReveal(1f);

            float holdSeconds = Mathf.Max(0f, profile != null ? profile.HoldSeconds : 1.72f);
            for (float elapsed = 0f; elapsed < holdSeconds; elapsed += DeltaTime)
            {
                SetBodyReveal(ResolveTypewriterCount(revealSeconds + elapsed));
                UpdateScanLine(TimeValue);
                yield return null;
            }

            SetBodyReveal(bodySource.Length);

            float dismissSeconds = Mathf.Max(0.01f, profile != null ? profile.DismissSeconds : 0.32f);
            for (float elapsed = 0f; elapsed < dismissSeconds; elapsed += DeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / dismissSeconds);
                ApplyDismiss(EaseIn(t));
                yield return null;
            }

            HideImmediate();
            noticeFinished?.Invoke();
            playRoutine = null;
        }

        private void ApplyProfile()
        {
            bodySource = profile != null ? profile.BodyText : "Tactical support window is online.";
            SetText(eyebrowText, profile != null ? profile.EyebrowText : "SYSTEM NOTICE");
            SetText(titleText, profile != null ? profile.TitleText : "Operation Link Established");
            SetText(bodyText, bodySource);
            SetText(leftStatusText, profile != null ? profile.LeftStatusText : "GUIDE // ONLINE");
            SetText(rightStatusText, profile != null ? profile.RightStatusText : "SYNC // STABLE");

            Color accent = profile != null ? profile.AccentColor : new Color(0.22f, 0.94f, 1f, 1f);
            Color background = profile != null ? profile.BackgroundColor : new Color(0.01f, 0.045f, 0.065f, 0.86f);
            Color scan = profile != null ? profile.ScanColor : new Color(0.5f, 1f, 1f, 0.72f);
            Color dim = profile != null ? profile.DimColor : new Color(0f, 0.025f, 0.035f, 0.16f);

            SetImageColor(panelBackgroundImage, background);
            SetImageColor(topLineImage, accent);
            SetImageColor(bottomLineImage, accent);
            SetImageColor(leftAccentImage, accent);
            SetImageColor(rightAccentImage, accent);
            SetImageColor(scanLineImage, scan);
            SetImageColor(dimImage, dim);
            SetTextColor(eyebrowText, accent);
            SetTextColor(leftStatusText, accent);
            SetTextColor(rightStatusText, accent);
        }

        private void PrepareIntroState()
        {
            visible = false;
            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.blocksRaycasts = blockPointerInputDuringNotice;
                rootGroup.interactable = false;
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.blocksRaycasts = false;
                panelGroup.interactable = false;
            }

            if (panelRoot != null)
            {
                panelRoot.localScale = new Vector3(0.985f, 0.965f, 1f);
            }

            SetLineScale(topLineImage, 0f);
            SetLineScale(bottomLineImage, 0f);
            SetGraphicAlpha(scanLineImage, 0f);
            SetBodyReveal(0);
        }

        private void AcquireGameplayPause()
        {
            if (!pauseGameplayDuringNotice || gameplayPauseApplied)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            gameplayPauseApplied = true;
        }

        private void ReleaseGameplayPause()
        {
            if (!gameplayPauseApplied)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            gameplayPauseApplied = false;
        }

        private void SetPointerInputBlocked(bool blocked)
        {
            if (rootGroup != null)
            {
                rootGroup.blocksRaycasts = blocked;
                rootGroup.interactable = false;
            }

            if (dimImage != null)
            {
                dimImage.raycastTarget = blocked;
            }
        }

        private void ApplyReveal(float t)
        {
            if (rootGroup != null)
            {
                rootGroup.alpha = t;
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = Mathf.Clamp01(t * 1.18f);
            }

            if (panelRoot != null)
            {
                float scaleX = Mathf.Lerp(0.985f, 1f, t);
                float scaleY = Mathf.Lerp(0.965f, 1f, t);
                panelRoot.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            SetLineScale(topLineImage, Mathf.Clamp01(t * 1.12f));
            SetLineScale(bottomLineImage, Mathf.Clamp01(t * 1.12f));
            SetGraphicAlpha(leftAccentImage, t);
            SetGraphicAlpha(rightAccentImage, t);
            SetGraphicAlpha(scanLineImage, Mathf.Sin(t * Mathf.PI) * 0.86f);
            UpdateScanLine(TimeValue);
        }

        private void ApplyDismiss(float t)
        {
            float alpha = 1f - t;
            if (rootGroup != null)
            {
                rootGroup.alpha = alpha;
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = alpha;
            }

            if (panelRoot != null)
            {
                float scaleY = Mathf.Lerp(1f, 0.985f, t);
                panelRoot.localScale = new Vector3(1f, scaleY, 1f);
            }

            SetLineScale(topLineImage, alpha);
            SetLineScale(bottomLineImage, alpha);
            SetGraphicAlpha(scanLineImage, alpha * 0.42f);
        }

        private void UpdateScanLine(float time)
        {
            if (scanLineImage == null)
            {
                return;
            }

            RectTransform rectTransform = scanLineImage.rectTransform;
            if (rectTransform == null || panelRoot == null)
            {
                return;
            }

            float width = Mathf.Max(1f, panelRoot.rect.width);
            float loop = Mathf.Repeat(time * 0.58f, 1f);
            rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-width * 0.48f, width * 0.48f, loop), 0f);
            Color color = scanLineImage.color;
            color.a = Mathf.Lerp(0.22f, 0.68f, 0.5f + Mathf.Sin(time * 7.2f) * 0.5f);
            scanLineImage.color = color;
        }

        private void PlayStartBeep()
        {
            if (profile == null || profile.StartBeepClip == null || audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(profile.StartBeepClip);
        }

        private int ResolveTypewriterCount(float elapsedSeconds)
        {
            float speed = profile != null ? profile.TypewriterCharactersPerSecond : 46f;
            if (speed <= 0f)
            {
                return bodySource.Length;
            }

            return Mathf.Clamp(Mathf.CeilToInt(elapsedSeconds * speed), 0, bodySource.Length);
        }

        private void SetBodyReveal(int characterCount)
        {
            if (bodyText == null)
            {
                return;
            }

            int count = Mathf.Clamp(characterCount, 0, bodySource.Length);
            bodyText.text = count >= bodySource.Length ? bodySource : bodySource.Substring(0, count);
        }

        private IEnumerator WaitSeconds(float seconds)
        {
            if (useUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(seconds);
                yield break;
            }

            yield return new WaitForSeconds(seconds);
        }

        private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private float TimeValue => useUnscaledTime ? Time.unscaledTime : Time.time;

        private static float EaseOut(float t)
        {
            return 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
        }

        private static float EaseIn(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetTextColor(Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }

        private static void SetImageColor(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            color.a = Mathf.Clamp01(alpha);
            graphic.color = color;
        }

        private static void SetLineScale(Image image, float scale)
        {
            if (image == null)
            {
                return;
            }

            RectTransform rectTransform = image.rectTransform;
            if (rectTransform != null)
            {
                rectTransform.localScale = new Vector3(Mathf.Clamp01(scale), 1f, 1f);
            }
        }
    }
}
