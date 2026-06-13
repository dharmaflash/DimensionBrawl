using System.Collections;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIMotionPresenter : MonoBehaviour
    {
        [SerializeField] private UIMotionCatalog catalog;
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private string defaultMotionId;
        [SerializeField] private bool playOnEnable;
        [SerializeField] private bool useUnscaledTime = true;

        private Coroutine motionRoutine;
        private bool cachedCanvasGroupState;
        private bool cachedInteractable;
        private bool cachedBlocksRaycasts;

        private void Reset()
        {
            targetRect = transform as RectTransform;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayDefault();
            }
        }

        private void OnDisable()
        {
            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
                motionRoutine = null;
            }

            RestoreCanvasGroupState();
        }

        public void PlayDefault()
        {
            PlayMotion(defaultMotionId);
        }

        public void PlayMotion(string motionId)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(motionId) || !catalog.TryGetMotion(motionId, out UIMotionCatalog.MotionEntry motion))
            {
                return;
            }

            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
                RestoreCanvasGroupState();
            }

            motionRoutine = StartCoroutine(PlayRoutine(motion));
        }

        public void ApplyEndState(string motionId)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(motionId) || !catalog.TryGetMotion(motionId, out UIMotionCatalog.MotionEntry motion))
            {
                return;
            }

            RestoreCanvasGroupState();
            Apply(motion.FadeTo, motion.ScaleTo);
        }

        private IEnumerator PlayRoutine(UIMotionCatalog.MotionEntry motion)
        {
            if (motion.DelaySeconds > 0f)
            {
                yield return WaitSeconds(motion.DelaySeconds);
            }

            CacheCanvasGroupState();
            if (motion.BlocksInputDuringMotion && canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }

            float duration = Mathf.Max(0f, motion.DurationSeconds);
            if (duration <= 0f)
            {
                Apply(motion.FadeTo, motion.ScaleTo);
                RestoreCanvasGroupState();
                motionRoutine = null;
                yield break;
            }

            for (float elapsed = 0f; elapsed < duration; elapsed += DeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Ease(t, motion.Easing);
                float fade = Mathf.LerpUnclamped(motion.FadeFrom, motion.FadeTo, eased);
                float scale = Mathf.LerpUnclamped(motion.ScaleFrom, motion.ScaleTo, eased);
                Apply(fade, scale);
                yield return null;
            }

            Apply(motion.FadeTo, motion.ScaleTo);
            RestoreCanvasGroupState();
            motionRoutine = null;
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

        private void Apply(float alpha, float scale)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }

            if (targetRect != null)
            {
                targetRect.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void CacheCanvasGroupState()
        {
            if (canvasGroup == null || cachedCanvasGroupState)
            {
                return;
            }

            cachedInteractable = canvasGroup.interactable;
            cachedBlocksRaycasts = canvasGroup.blocksRaycasts;
            cachedCanvasGroupState = true;
        }

        private void RestoreCanvasGroupState()
        {
            if (canvasGroup == null || !cachedCanvasGroupState)
            {
                return;
            }

            canvasGroup.interactable = cachedInteractable;
            canvasGroup.blocksRaycasts = cachedBlocksRaycasts;
            cachedCanvasGroupState = false;
        }

        private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private static float Ease(float t, UIMotionEasing easing)
        {
            switch (easing)
            {
                case UIMotionEasing.EaseOut:
                    return 1f - Mathf.Pow(1f - t, 3f);
                case UIMotionEasing.EaseInOut:
                    return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
                case UIMotionEasing.DisplaySpring:
                    return Mathf.Clamp01(1f - Mathf.Cos(t * Mathf.PI * 3f) * Mathf.Exp(-t * 5f));
                default:
                    return t;
            }
        }
    }
}
