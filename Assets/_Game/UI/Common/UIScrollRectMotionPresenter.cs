using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIScrollRectMotionPresenter : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private bool configurePhysics = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField, Min(0f)] private float elasticity = 0.14f;
        [SerializeField, Range(0.01f, 1f)] private float decelerationRate = 0.18f;
        [SerializeField, Min(0f)] private float scrollSensitivity = 32f;
        [SerializeField] private bool snapOnEndDrag;
        [SerializeField, Range(0f, 1f)] private float viewportFocus = 0.5f;
        [SerializeField, Min(0f)] private float focusDurationSeconds = 0.3f;
        [SerializeField, Min(0f)] private float snapDelaySeconds = 0.08f;
        [SerializeField, Min(0f)] private float snapVelocityThreshold = 80f;

        private Coroutine moveRoutine;
        private Coroutine snapRoutine;

        private void Reset()
        {
            scrollRect = GetComponent<ScrollRect>();
            content = scrollRect != null ? scrollRect.content : null;
            viewport = scrollRect != null ? scrollRect.viewport : null;
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReady();
        }

        private void OnDisable()
        {
            StopMotion();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            StopMotion();
            ApplyInteractivePhysics();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!snapOnEndDrag)
            {
                return;
            }

            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
            }

            snapRoutine = StartCoroutine(SnapAfterDragRoutine());
        }

        public void FocusTarget(RectTransform target)
        {
            FocusTarget(target, focusDurationSeconds);
        }

        public void FocusTarget(RectTransform target, float durationSeconds)
        {
            if (target == null)
            {
                return;
            }

            EnsureReady();

            if (content == null || viewport == null)
            {
                return;
            }

            Vector2 focusedPosition = CalculateFocusedAnchoredPosition(target);
            StartMove(focusedPosition, Mathf.Max(0f, durationSeconds));
        }

        public void SnapToNearest()
        {
            EnsureReady();

            RectTransform nearestTarget = FindNearestDirectChild();
            if (nearestTarget != null)
            {
                FocusTarget(nearestTarget, focusDurationSeconds);
            }
        }

        private void EnsureReady()
        {
            EnsureReferences();
            ApplyInteractivePhysics();
        }

        private void EnsureReferences()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (scrollRect == null)
            {
                return;
            }

            if (content == null)
            {
                content = scrollRect.content;
            }

            if (viewport == null)
            {
                viewport = scrollRect.viewport != null ? scrollRect.viewport : transform as RectTransform;
            }
        }

        private void ApplyInteractivePhysics()
        {
            if (!configurePhysics || scrollRect == null)
            {
                return;
            }

            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.inertia = true;
            scrollRect.elasticity = elasticity;
            scrollRect.decelerationRate = decelerationRate;
            scrollRect.scrollSensitivity = scrollSensitivity;
        }

        private void ApplyProgrammaticPhysics()
        {
            if (scrollRect == null)
            {
                return;
            }

            StopScrollMovement();
            scrollRect.inertia = false;
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        }

        private Vector2 CalculateFocusedAnchoredPosition(RectTransform target)
        {
            Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, target);
            Vector2 targetCenterInViewport = viewport.InverseTransformPoint(content.TransformPoint(targetBounds.center));
            Rect viewportRect = viewport.rect;
            Vector2 desiredCenter = new Vector2(
                Mathf.Lerp(viewportRect.xMin, viewportRect.xMax, viewportFocus),
                Mathf.Lerp(viewportRect.yMin, viewportRect.yMax, viewportFocus));

            Vector2 offset = targetCenterInViewport - desiredCenter;
            Vector2 targetPosition = content.anchoredPosition - offset;
            if (scrollRect != null)
            {
                if (!scrollRect.horizontal)
                {
                    targetPosition.x = content.anchoredPosition.x;
                }

                if (!scrollRect.vertical)
                {
                    targetPosition.y = content.anchoredPosition.y;
                }
            }

            return ClampAnchoredPosition(targetPosition);
        }

        private Vector2 ClampAnchoredPosition(Vector2 position)
        {
            if (content == null || viewport == null)
            {
                return position;
            }

            if (scrollRect == null || scrollRect.horizontal)
            {
                float maxHorizontalOffset = Mathf.Max(0f, content.rect.width - viewport.rect.width);
                position.x = Mathf.Clamp(position.x, -maxHorizontalOffset, 0f);
            }

            if (scrollRect != null && scrollRect.vertical)
            {
                float maxVerticalOffset = Mathf.Max(0f, content.rect.height - viewport.rect.height);
                position.y = Mathf.Clamp(position.y, 0f, maxVerticalOffset);
            }

            return position;
        }

        private RectTransform FindNearestDirectChild()
        {
            if (content == null || viewport == null)
            {
                return null;
            }

            Rect viewportRect = viewport.rect;
            float targetAxisPosition = IsHorizontal
                ? Mathf.Lerp(viewportRect.xMin, viewportRect.xMax, viewportFocus)
                : Mathf.Lerp(viewportRect.yMin, viewportRect.yMax, viewportFocus);

            RectTransform nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform child = content.GetChild(i) as RectTransform;
                if (child == null || !child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Bounds childBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, child);
                Vector2 childCenterInViewport = viewport.InverseTransformPoint(content.TransformPoint(childBounds.center));
                float childAxisPosition = IsHorizontal ? childCenterInViewport.x : childCenterInViewport.y;
                float distance = Mathf.Abs(childAxisPosition - targetAxisPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = child;
                }
            }

            return nearest;
        }

        private IEnumerator SnapAfterDragRoutine()
        {
            if (snapDelaySeconds > 0f)
            {
                yield return WaitSeconds(snapDelaySeconds);
            }

            float waitTime = 0f;
            while (scrollRect != null && scrollRect.velocity.magnitude > snapVelocityThreshold && waitTime < 0.35f)
            {
                waitTime += DeltaTime;
                yield return null;
            }

            snapRoutine = null;
            StopScrollMovement();
            SnapToNearest();
        }

        private void StartMove(Vector2 targetPosition, float durationSeconds)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            if (!isActiveAndEnabled || durationSeconds <= 0f)
            {
                ApplyProgrammaticPhysics();
                content.anchoredPosition = targetPosition;
                ApplyInteractivePhysics();
                moveRoutine = null;
                return;
            }

            moveRoutine = StartCoroutine(MoveRoutine(targetPosition, durationSeconds));
        }

        private IEnumerator MoveRoutine(Vector2 targetPosition, float durationSeconds)
        {
            ApplyProgrammaticPhysics();

            Vector2 startPosition = content.anchoredPosition;
            for (float elapsed = 0f; elapsed < durationSeconds; elapsed += DeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / durationSeconds);
                float eased = Mathf.Sin(t * Mathf.PI * 0.5f);
                content.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, eased);
                yield return null;
            }

            content.anchoredPosition = targetPosition;
            ApplyInteractivePhysics();
            moveRoutine = null;
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

        private void StopMotion()
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }

            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
                snapRoutine = null;
            }

            if (scrollRect != null)
            {
                StopScrollMovement();
                ApplyInteractivePhysics();
            }
        }

        private void StopScrollMovement()
        {
            if (scrollRect == null)
            {
                return;
            }

            scrollRect.StopMovement();
            scrollRect.velocity = Vector2.zero;
        }

        private bool IsHorizontal => scrollRect == null || scrollRect.horizontal || !scrollRect.vertical;
        private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}
