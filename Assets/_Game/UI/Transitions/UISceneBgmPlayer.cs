using System.Collections;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class UISceneBgmPlayer : MonoBehaviour
    {
        [SerializeField] private UIScreenCatalog screenCatalog;
        [SerializeField] private UISoundContextCatalog soundContextCatalog;
        [SerializeField] private UIRouteId routeId = UIRouteId.None;
        [SerializeField] private AudioSource source;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool stopOnDisable = true;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

        private Coroutine fadeRoutine;

        public UIRouteId RouteId => routeId;
        public string CurrentContextId { get; private set; }
        public AudioClip CurrentClip => source != null ? source.clip : null;

        private void Reset()
        {
            source = GetComponent<AudioSource>();
            ConfigureSourceForBgm();
        }

        private void Awake()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
            }

            ConfigureSourceForBgm();
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            StopFade();
            if (stopOnDisable && source != null)
            {
                source.Stop();
            }
        }

        public bool Play()
        {
            if (!TryResolveContext(out UISoundContextCatalog.SoundContext context))
            {
                StopSource();
                return false;
            }

            CurrentContextId = context.Id;
            if (context.Clip == null)
            {
                Debug.LogWarning($"UI BGM context has no AudioClip assigned: {context.Id}", this);
                StopSource();
                return false;
            }

            ConfigureSourceForBgm();
            source.clip = context.Clip;
            source.loop = context.Loop;

            float targetVolume = Mathf.Clamp01(context.Volume * masterVolume);
            StopFade();
            if (context.FadeCrossSeconds > 0f && gameObject.activeInHierarchy)
            {
                source.volume = 0f;
                source.Play();
                fadeRoutine = StartCoroutine(FadeVolume(targetVolume, context.FadeCrossSeconds));
            }
            else
            {
                source.volume = targetVolume;
                source.Play();
            }

            return true;
        }

        private bool TryResolveContext(out UISoundContextCatalog.SoundContext context)
        {
            context = default;
            if (screenCatalog == null || soundContextCatalog == null)
            {
                Debug.LogWarning("UI BGM player is missing its screen or sound context catalog.", this);
                return false;
            }

            if (!screenCatalog.TryGetScreen(routeId, out UIScreenCatalog.ScreenEntry screen))
            {
                Debug.LogWarning($"UI BGM player route is not configured: {routeId}", this);
                return false;
            }

            if (!soundContextCatalog.TryGetContext(screen.BgmContextId, out context))
            {
                Debug.LogWarning($"UI BGM context is not configured: {screen.BgmContextId}", this);
                return false;
            }

            return true;
        }

        private IEnumerator FadeVolume(float targetVolume, float durationSeconds)
        {
            float duration = Mathf.Max(0.001f, durationSeconds);
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
            fadeRoutine = null;
        }

        private void ConfigureSourceForBgm()
        {
            if (source == null)
            {
                return;
            }

            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        private void StopFade()
        {
            if (fadeRoutine == null)
            {
                return;
            }

            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        private void StopSource()
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }
}
