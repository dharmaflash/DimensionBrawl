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
        private Vector2 motionBaseAnchoredPosition;
        private bool hasMotionBaseAnchoredPosition;
        private string preparedMotionId;

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
            if (!TryGetMotion(motionId, out UIMotionCatalog.MotionEntry motion))
            {
                return;
            }

            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
                RestoreCanvasGroupState();
            }

            if (!string.Equals(preparedMotionId, motionId, System.StringComparison.Ordinal))
            {
                ClearPreparedMotion();
            }

            if (!isActiveAndEnabled)
            {
                RestoreCanvasGroupState();
                PrepareMotionBase(motionId);
                Apply(motion.FadeTo, motion.ScaleTo, motion.AnchoredOffsetTo);
                ClearPreparedMotion();
                motionRoutine = null;
                return;
            }

            motionRoutine = StartCoroutine(PlayRoutine(motion));
        }

        public void ApplyStartState(string motionId)
        {
            if (!TryGetMotion(motionId, out UIMotionCatalog.MotionEntry motion))
            {
                return;
            }

            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
                motionRoutine = null;
            }

            RestoreCanvasGroupState();
            if (!string.Equals(preparedMotionId, motionId, System.StringComparison.Ordinal))
            {
                ClearPreparedMotion();
            }

            PrepareMotionBase(motionId);
            Apply(motion.FadeFrom, motion.ScaleFrom, motion.AnchoredOffsetFrom);
        }

        public void ApplyEndState(string motionId)
        {
            if (!TryGetMotion(motionId, out UIMotionCatalog.MotionEntry motion))
            {
                return;
            }

            RestoreCanvasGroupState();
            if (!string.Equals(preparedMotionId, motionId, System.StringComparison.Ordinal))
            {
                ClearPreparedMotion();
            }

            PrepareMotionBase(motionId);
            Apply(motion.FadeTo, motion.ScaleTo, motion.AnchoredOffsetTo);
            ClearPreparedMotion();
        }

        private IEnumerator PlayRoutine(UIMotionCatalog.MotionEntry motion)
        {
            PrepareMotionBase(motion.Id);
            Apply(motion.FadeFrom, motion.ScaleFrom, motion.AnchoredOffsetFrom);

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
                Apply(motion.FadeTo, motion.ScaleTo, motion.AnchoredOffsetTo);
                RestoreCanvasGroupState();
                ClearPreparedMotion();
                motionRoutine = null;
                yield break;
            }

            for (float elapsed = 0f; elapsed < duration; elapsed += DeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Ease(t, motion.Easing);
                float fade = Mathf.LerpUnclamped(motion.FadeFrom, motion.FadeTo, eased);
                float scale = Mathf.LerpUnclamped(motion.ScaleFrom, motion.ScaleTo, eased);
                Vector2 offset = Vector2.LerpUnclamped(motion.AnchoredOffsetFrom, motion.AnchoredOffsetTo, eased);
                Apply(fade, scale, offset);
                yield return null;
            }

            Apply(motion.FadeTo, motion.ScaleTo, motion.AnchoredOffsetTo);
            RestoreCanvasGroupState();
            ClearPreparedMotion();
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

        private void Apply(float alpha, float scale, Vector2 anchoredOffset)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }

            if (targetRect != null)
            {
                targetRect.localScale = new Vector3(scale, scale, 1f);
                targetRect.anchoredPosition = motionBaseAnchoredPosition + anchoredOffset;
            }
        }

        private bool TryGetMotion(string motionId, out UIMotionCatalog.MotionEntry motion)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(motionId) || !catalog.TryGetMotion(motionId, out motion))
            {
                motion = default;
                return false;
            }

            return true;
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

        private void PrepareMotionBase(string motionId)
        {
            if (targetRect != null && !hasMotionBaseAnchoredPosition)
            {
                motionBaseAnchoredPosition = targetRect.anchoredPosition;
                hasMotionBaseAnchoredPosition = true;
            }

            preparedMotionId = motionId;
        }

        private void ClearPreparedMotion()
        {
            hasMotionBaseAnchoredPosition = false;
            preparedMotionId = null;
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
