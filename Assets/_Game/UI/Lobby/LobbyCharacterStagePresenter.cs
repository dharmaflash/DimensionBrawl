using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyCharacterStagePresenter : MonoBehaviour
    {
        [Serializable]
        private struct MotionProfile
        {
            [SerializeField] private string motionKey;
            [SerializeField] private Vector3 peakEulerAngles;
            [SerializeField] private Vector3 peakPositionOffset;

            public string MotionKey => motionKey;
            public Vector3 PeakEulerAngles => peakEulerAngles;
            public Vector3 PeakPositionOffset => peakPositionOffset;
        }

        [SerializeField] private Transform targetRoot;
        [SerializeField] private LobbyCharacterStageInputChannel inputChannel;
        [SerializeField] private LobbyGuideFeedbackCatalog feedbackCatalog;
        [SerializeField] private LobbyGuideCondition defaultCondition = LobbyGuideCondition.Default;
        [SerializeField] private LobbyGuideCondition tapCondition = LobbyGuideCondition.CharacterTapped;
        [SerializeField] private MotionProfile[] motionProfiles = Array.Empty<MotionProfile>();
        [SerializeField, Range(0f, 15f)] private float idleYawAmplitude = 2.5f;
        [SerializeField, Min(0f)] private float idleYawFrequency = 0.12f;
        [SerializeField, Range(0f, 0.08f)] private float idleBobAmplitude = 0.012f;
        [SerializeField, Min(0f)] private float idleBobFrequency = 0.32f;
        [SerializeField, Range(0f, 35f)] private float dragYawLimit = 18f;
        [SerializeField, Min(0f)] private float dragDegreesPerPixel = 0.08f;
        [SerializeField, Min(0f)] private float dragReturnSpeed = 6f;
        [SerializeField] private bool useUnscaledTime = true;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation = Quaternion.identity;
        private Vector3 activeReactionEuler;
        private Vector3 activeReactionOffset;
        private float targetDragYaw;
        private float currentDragYaw;
        private float reactionRemaining;
        private float reactionDuration;
        private float lastTapReactionTime = -1000f;
        private bool capturedBasePose;
        private bool isDragging;

        private void Reset()
        {
            targetRoot = transform;
        }

        private void Awake()
        {
            CaptureBasePose();
        }

        private void OnEnable()
        {
            CaptureBasePose();
            SubscribeInput();
            TryPlayFeedback(defaultCondition, false);
        }

        private void OnDisable()
        {
            UnsubscribeInput();
            RestoreBasePose();
        }

        private void Update()
        {
            if (targetRoot == null)
            {
                return;
            }

            float deltaTime = DeltaTime;
            float time = CurrentTime;

            if (!isDragging)
            {
                targetDragYaw = Mathf.Lerp(targetDragYaw, 0f, DampedStep(dragReturnSpeed, deltaTime));
            }

            currentDragYaw = Mathf.Lerp(currentDragYaw, targetDragYaw, DampedStep(dragReturnSpeed, deltaTime));

            float idleYaw = Mathf.Sin(time * Mathf.PI * 2f * idleYawFrequency) * idleYawAmplitude;
            float idleBob = Mathf.Sin(time * Mathf.PI * 2f * idleBobFrequency) * idleBobAmplitude;
            float reactionScale = EvaluateReactionScale(deltaTime);
            Vector3 reactionEuler = activeReactionEuler * reactionScale;
            Vector3 reactionOffset = activeReactionOffset * reactionScale;

            targetRoot.localPosition = baseLocalPosition + Vector3.up * idleBob + reactionOffset;
            targetRoot.localRotation = baseLocalRotation * Quaternion.Euler(
                reactionEuler.x,
                currentDragYaw + idleYaw + reactionEuler.y,
                reactionEuler.z);
        }

        public void BeginInteraction()
        {
            isDragging = true;
        }

        public void DragHorizontal(float deltaPixels)
        {
            if (targetRoot == null)
            {
                return;
            }

            targetDragYaw = Mathf.Clamp(
                targetDragYaw - deltaPixels * dragDegreesPerPixel,
                -dragYawLimit,
                dragYawLimit);
        }

        public void EndInteraction()
        {
            isDragging = false;
        }

        public void PlayTapReaction()
        {
            TryPlayFeedback(tapCondition, true);
        }

        private void CaptureBasePose()
        {
            if (targetRoot == null)
            {
                targetRoot = transform;
            }

            baseLocalPosition = targetRoot.localPosition;
            baseLocalRotation = targetRoot.localRotation;
            capturedBasePose = true;
        }

        private void RestoreBasePose()
        {
            if (targetRoot == null || !capturedBasePose)
            {
                return;
            }

            targetRoot.localPosition = baseLocalPosition;
            targetRoot.localRotation = baseLocalRotation;
            targetDragYaw = 0f;
            currentDragYaw = 0f;
            reactionRemaining = 0f;
            reactionDuration = 0f;
            activeReactionEuler = Vector3.zero;
            activeReactionOffset = Vector3.zero;
            isDragging = false;
        }

        private bool TryPlayFeedback(LobbyGuideCondition condition, bool respectCooldown)
        {
            if (feedbackCatalog == null || !feedbackCatalog.TryGetFirst(condition, out LobbyGuideFeedbackCatalog.FeedbackEntry entry))
            {
                return false;
            }

            if (respectCooldown && CurrentTime - lastTapReactionTime < entry.CooldownSeconds)
            {
                return true;
            }

            ApplyMotionKey(entry.MotionKey, entry.DurationSeconds);

            if (respectCooldown)
            {
                lastTapReactionTime = CurrentTime;
            }

            return true;
        }

        private void ApplyMotionKey(string motionKey, float durationSeconds)
        {
            MotionProfile profile = FindMotionProfile(motionKey);
            activeReactionEuler = profile.PeakEulerAngles;
            activeReactionOffset = profile.PeakPositionOffset;
            reactionDuration = Mathf.Max(0.05f, durationSeconds);
            reactionRemaining = reactionDuration;
        }

        private MotionProfile FindMotionProfile(string motionKey)
        {
            for (int i = 0; i < motionProfiles.Length; i++)
            {
                if (string.Equals(motionProfiles[i].MotionKey, motionKey, StringComparison.Ordinal))
                {
                    return motionProfiles[i];
                }
            }

            return default;
        }

        private float EvaluateReactionScale(float deltaTime)
        {
            if (reactionRemaining <= 0f)
            {
                return 0f;
            }

            reactionRemaining = Mathf.Max(0f, reactionRemaining - deltaTime);
            float normalized = 1f - reactionRemaining / Mathf.Max(0.01f, reactionDuration);
            return Mathf.Sin(normalized * Mathf.PI);
        }

        private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;

        private static float DampedStep(float speed, float deltaTime)
        {
            return 1f - Mathf.Exp(-Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime));
        }

        private void SubscribeInput()
        {
            if (inputChannel == null)
            {
                return;
            }

            inputChannel.BeginInteractionRequested += BeginInteraction;
            inputChannel.HorizontalDragRequested += DragHorizontal;
            inputChannel.EndInteractionRequested += EndInteraction;
            inputChannel.TapRequested += PlayTapReaction;
        }

        private void UnsubscribeInput()
        {
            if (inputChannel == null)
            {
                return;
            }

            inputChannel.BeginInteractionRequested -= BeginInteraction;
            inputChannel.HorizontalDragRequested -= DragHorizontal;
            inputChannel.EndInteractionRequested -= EndInteraction;
            inputChannel.TapRequested -= PlayTapReaction;
        }
    }
}
