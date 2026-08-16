using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    /// <summary>
    /// Owns the Station boss-terminal camera cut as an explicit, manually sampled
    /// Timeline lease. The authored Timeline drives only the dedicated finisher
    /// camera; this owner switches Camera components without creating a second
    /// AudioListener and restores only values that still match its writes.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10240)]
    public sealed class OlympusStationBossTerminalFinisherCameraController : MonoBehaviour
    {
        public const double RequiredTimelineDurationSeconds = 2.6d;
        public const float RequiredResultCoverReleaseSeconds = 0.46f;

        private const double TimelineToleranceSeconds = 0.0001d;
        private const float ReleaseToleranceSeconds = 0.00001f;

        [Header("Authored Finisher Timeline")]
        [SerializeField] private PlayableDirector finisherDirector;
        [SerializeField] private TimelineAsset finisherTimeline;

        [Header("Exclusive Camera Components")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Camera finisherCamera;
        [SerializeField, Min(0.01f)] private float resultCoverReleaseSeconds =
            RequiredResultCoverReleaseSeconds;

        private UnityEngine.Object activeOwner;
        private PlayableDirector leasedDirector;
        private TimelineAsset leasedTimeline;
        private Camera leasedGameplayCamera;
        private Camera leasedFinisherCamera;
        private DirectorUpdateMode previousDirectorUpdateMode;
        private double previousDirectorTime;
        private bool previousGameplayCameraEnabled;
        private bool previousFinisherCameraEnabled;
        private bool directorModeWritten;
        private bool directorTimeWritten;
        private bool directorGraphEvaluated;
        private bool gameplayCameraEnabledWritten;
        private bool finisherCameraEnabledWritten;
        private bool leaseActive;
        private bool resultCoverReleaseScheduled;
        private float resultCoverReleaseElapsedSeconds;
        private double lastWrittenDirectorTime;
        private double lastSampledSeconds;
        private int acquireCount;
        private int releaseCount;
        private int requestVersion;
        private int sampleCount;
        private int resultCoverReleaseSampleCount;
        private bool hasReachedTerminalSample;
        private bool wasInterrupted;

        public PlayableDirector FinisherDirector => finisherDirector;
        public TimelineAsset FinisherTimeline => finisherTimeline;
        public Camera GameplayCamera => gameplayCamera;
        public Camera FinisherCamera => finisherCamera;
        public float ResultCoverReleaseSeconds => resultCoverReleaseSeconds;
        public bool IsLeaseActive => leaseActive;
        public bool IsResultCoverReleaseScheduled => resultCoverReleaseScheduled;
        public float ResultCoverReleaseElapsedSeconds => resultCoverReleaseElapsedSeconds;
        public int AcquireCount => acquireCount;
        public int ReleaseCount => releaseCount;
        public int RequestVersion => requestVersion;
        public int SampleCount => sampleCount;
        public int ResultCoverReleaseSampleCount => resultCoverReleaseSampleCount;
        public double LastSampledSeconds => lastSampledSeconds;
        public bool HasReachedTerminalSample => hasReachedTerminalSample;
        public bool WasInterrupted => wasInterrupted;
        public string LastError { get; private set; } = string.Empty;
        public Camera ActiveCamera
        {
            get
            {
                if (leaseActive && leasedFinisherCamera != null && leasedFinisherCamera.enabled)
                {
                    return leasedFinisherCamera;
                }

                if (gameplayCamera != null && gameplayCamera.enabled)
                {
                    return gameplayCamera;
                }

                return finisherCamera != null && finisherCamera.enabled
                    ? finisherCamera
                    : null;
            }
        }

        public bool Configure(
            PlayableDirector sourceDirector,
            TimelineAsset sourceTimeline,
            Camera sourceGameplayCamera,
            Camera sourceFinisherCamera)
        {
            if (leaseActive)
            {
                SetError("The Station finisher camera cannot be reconfigured while its lease is active.");
                return false;
            }

            finisherDirector = sourceDirector;
            finisherTimeline = sourceTimeline;
            gameplayCamera = sourceGameplayCamera;
            finisherCamera = sourceFinisherCamera;
            resultCoverReleaseSeconds = RequiredResultCoverReleaseSeconds;
            LastError = string.Empty;
            return true;
        }

        public bool ValidateConfiguration(out string error)
        {
            error = string.Empty;
            if (finisherDirector == null)
            {
                error = "The Station finisher PlayableDirector is missing.";
                return false;
            }

            if (finisherTimeline == null)
            {
                error = "The Station finisher TimelineAsset is missing.";
                return false;
            }

            if (gameplayCamera == null || finisherCamera == null)
            {
                error = "The Station finisher requires exact gameplay and finisher Camera references.";
                return false;
            }

            if (gameplayCamera == finisherCamera)
            {
                error = "The Station finisher Camera must be separate from the gameplay Camera.";
                return false;
            }

            if (!gameplayCamera.gameObject.activeInHierarchy
                || !finisherCamera.gameObject.activeInHierarchy)
            {
                error = "Both Station camera GameObjects must stay active so the lease can own only Camera.enabled.";
                return false;
            }

            if (finisherDirector.playableAsset != finisherTimeline)
            {
                error = "The Station finisher PlayableDirector is not bound to the exact authored TimelineAsset.";
                return false;
            }

            if (Math.Abs(finisherTimeline.duration - RequiredTimelineDurationSeconds)
                > TimelineToleranceSeconds)
            {
                error = $"The Station finisher Timeline duration must be exactly {RequiredTimelineDurationSeconds:R}s (actual={finisherTimeline.duration:R}s).";
                return false;
            }

            if (Mathf.Abs(resultCoverReleaseSeconds - RequiredResultCoverReleaseSeconds)
                > ReleaseToleranceSeconds)
            {
                error = $"The Station finisher result-cover release must be exactly {RequiredResultCoverReleaseSeconds:R}s.";
                return false;
            }

            if (!gameplayCamera.enabled || finisherCamera.enabled)
            {
                error = "The Station camera handoff must start with gameplay enabled and finisher disabled.";
                return false;
            }

            if (finisherDirector.state == PlayState.Playing)
            {
                error = "The dedicated Station finisher PlayableDirector is already playing.";
                return false;
            }

            AudioListener[] listeners =
                finisherCamera.GetComponentsInChildren<AudioListener>(true);
            if (listeners != null && listeners.Length > 0)
            {
                error = "The dedicated Station finisher camera hierarchy must not contain an AudioListener.";
                return false;
            }

            return true;
        }

        public bool TryAcquire(UnityEngine.Object owner, out int version)
        {
            version = requestVersion;
            if (owner == null)
            {
                SetError("A live owner is required to acquire the Station finisher camera.");
                return false;
            }

            if (leaseActive)
            {
                if (ReferenceEquals(activeOwner, owner))
                {
                    return true;
                }

                SetError("A foreign owner already holds the Station finisher camera lease.");
                return false;
            }

            if (!isActiveAndEnabled)
            {
                SetError("The Station finisher camera controller is not active and enabled.");
                return false;
            }

            if (!ValidateConfiguration(out string validationError))
            {
                SetError(validationError);
                return false;
            }

            CaptureLeaseState(owner);
            try
            {
                if (leasedDirector.timeUpdateMode != DirectorUpdateMode.Manual)
                {
                    leasedDirector.timeUpdateMode = DirectorUpdateMode.Manual;
                    directorModeWritten = true;
                }

                if (!Approximately(leasedDirector.time, 0d))
                {
                    leasedDirector.time = 0d;
                    directorTimeWritten = true;
                }

                lastWrittenDirectorTime = 0d;
                leasedDirector.Evaluate();
                directorGraphEvaluated = true;

                // Evaluate the authored t=0 pose before changing the live camera.
                leasedGameplayCamera.enabled = false;
                gameplayCameraEnabledWritten = true;
                leasedFinisherCamera.enabled = true;
                finisherCameraEnabledWritten = true;
            }
            catch (Exception exception)
            {
                SetError($"The Station finisher camera could not acquire safely: {exception.Message}");
                RestoreWrittenValues();
                ClearActiveLeaseReferences();
                return false;
            }

            leaseActive = true;
            acquireCount++;
            requestVersion = NextPositiveVersion(requestVersion);
            version = requestVersion;
            sampleCount = 0;
            resultCoverReleaseSampleCount = 0;
            resultCoverReleaseScheduled = false;
            resultCoverReleaseElapsedSeconds = 0f;
            lastSampledSeconds = 0d;
            hasReachedTerminalSample = false;
            wasInterrupted = false;
            LastError = string.Empty;
            return true;
        }

        public bool Sample(UnityEngine.Object owner, float elapsedPresentationSeconds)
        {
            if (!TryValidateOwner(owner, "sample"))
            {
                return false;
            }

            if (!IsFiniteNonNegative(elapsedPresentationSeconds))
            {
                InterruptAndRestore("The Station finisher received an invalid presentation sample.");
                return false;
            }

            double sampleSeconds = Math.Min(
                RequiredTimelineDurationSeconds,
                elapsedPresentationSeconds);
            if (sampleSeconds + TimelineToleranceSeconds < lastSampledSeconds)
            {
                InterruptAndRestore("The Station finisher presentation clock moved backwards.");
                return false;
            }

            if (!ValidateOwnedState(out string ownershipError))
            {
                InterruptAndRestore(ownershipError);
                return false;
            }

            try
            {
                leasedDirector.time = sampleSeconds;
                directorTimeWritten = true;
                lastWrittenDirectorTime = sampleSeconds;
                leasedDirector.Evaluate();
            }
            catch (Exception exception)
            {
                InterruptAndRestore(
                    $"The Station finisher Timeline sample failed safely: {exception.Message}");
                return false;
            }

            lastSampledSeconds = sampleSeconds;
            sampleCount++;
            if (sampleSeconds + TimelineToleranceSeconds
                >= RequiredTimelineDurationSeconds)
            {
                hasReachedTerminalSample = true;
            }

            return true;
        }

        public bool ScheduleReleaseAfterResultCover(UnityEngine.Object owner)
        {
            if (!TryValidateOwner(owner, "schedule its result-cover release"))
            {
                return false;
            }

            if (!ValidateOwnedState(out string ownershipError))
            {
                InterruptAndRestore(ownershipError);
                return false;
            }

            if (!hasReachedTerminalSample)
            {
                InterruptAndRestore(
                    "The Station finisher camera cannot release before its exact 2.6s terminal sample.");
                return false;
            }

            if (resultCoverReleaseScheduled)
            {
                return true;
            }

            resultCoverReleaseScheduled = true;
            resultCoverReleaseElapsedSeconds = 0f;
            resultCoverReleaseSampleCount = 0;
            return true;
        }

        public bool CancelAndRestore(UnityEngine.Object owner, string reason)
        {
            if (!TryValidateOwner(owner, "cancel"))
            {
                return false;
            }

            wasInterrupted = true;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                SetError(reason);
            }

            RestoreLease();
            return true;
        }

        public bool IsOwnedBy(UnityEngine.Object owner)
        {
            return leaseActive
                && owner != null
                && ReferenceEquals(activeOwner, owner);
        }

        private void Update()
        {
            if (!leaseActive)
            {
                return;
            }

            if (activeOwner == null)
            {
                InterruptAndRestore("The Station finisher camera owner was destroyed.");
                return;
            }

            if (!ValidateOwnedState(out string ownershipError))
            {
                InterruptAndRestore(ownershipError);
                return;
            }

            if (!resultCoverReleaseScheduled)
            {
                return;
            }

            float deltaTime = PresentationClock.UnscaledDeltaTime;
            if (!IsFiniteNonNegative(deltaTime))
            {
                InterruptAndRestore("The Station finisher result-cover clock sample was invalid.");
                return;
            }

            resultCoverReleaseElapsedSeconds += deltaTime;
            resultCoverReleaseSampleCount++;
            if (resultCoverReleaseElapsedSeconds + ReleaseToleranceSeconds
                >= RequiredResultCoverReleaseSeconds)
            {
                RestoreLease();
            }
        }

        private void OnDisable()
        {
            if (leaseActive)
            {
                wasInterrupted = true;
                SetError("The Station finisher camera controller was disabled during its lease.");
                RestoreLease();
            }
        }

        private void OnDestroy()
        {
            if (leaseActive)
            {
                wasInterrupted = true;
                SetError("The Station finisher camera controller was destroyed during its lease.");
                RestoreLease();
            }
        }

        private void CaptureLeaseState(UnityEngine.Object owner)
        {
            activeOwner = owner;
            leasedDirector = finisherDirector;
            leasedTimeline = finisherTimeline;
            leasedGameplayCamera = gameplayCamera;
            leasedFinisherCamera = finisherCamera;
            previousDirectorUpdateMode = leasedDirector.timeUpdateMode;
            previousDirectorTime = leasedDirector.time;
            previousGameplayCameraEnabled = leasedGameplayCamera.enabled;
            previousFinisherCameraEnabled = leasedFinisherCamera.enabled;
            directorModeWritten = false;
            directorTimeWritten = false;
            directorGraphEvaluated = false;
            gameplayCameraEnabledWritten = false;
            finisherCameraEnabledWritten = false;
        }

        private bool TryValidateOwner(UnityEngine.Object owner, string operation)
        {
            if (!leaseActive)
            {
                SetError($"The Station finisher camera has no active lease to {operation}.");
                return false;
            }

            if (owner == null || !ReferenceEquals(activeOwner, owner))
            {
                SetError($"A foreign owner cannot {operation} the Station finisher camera lease.");
                return false;
            }

            return true;
        }

        private bool ValidateOwnedState(out string error)
        {
            error = string.Empty;
            if (leasedDirector == null
                || leasedTimeline == null
                || leasedGameplayCamera == null
                || leasedFinisherCamera == null)
            {
                error = "A leased Station finisher camera reference was destroyed.";
                return false;
            }

            if (finisherDirector != leasedDirector
                || finisherTimeline != leasedTimeline
                || gameplayCamera != leasedGameplayCamera
                || finisherCamera != leasedFinisherCamera)
            {
                error = "The Station finisher camera references changed during the active lease.";
                return false;
            }

            if (leasedDirector.playableAsset != leasedTimeline
                || leasedDirector.timeUpdateMode != DirectorUpdateMode.Manual)
            {
                error = "The dedicated Station finisher Timeline ownership changed during the active lease.";
                return false;
            }

            if (!Approximately(leasedDirector.time, lastWrittenDirectorTime))
            {
                error = "The dedicated Station finisher Timeline time changed outside its owner.";
                return false;
            }

            if (Math.Abs(leasedTimeline.duration - RequiredTimelineDurationSeconds)
                > TimelineToleranceSeconds)
            {
                error = "The authored Station finisher Timeline duration changed during the active lease.";
                return false;
            }

            if (leasedGameplayCamera.enabled || !leasedFinisherCamera.enabled)
            {
                error = "The exclusive Station finisher Camera.enabled ownership was interrupted.";
                return false;
            }

            AudioListener[] listeners =
                leasedFinisherCamera.GetComponentsInChildren<AudioListener>(true);
            if (listeners != null && listeners.Length > 0)
            {
                error = "An AudioListener appeared on the dedicated Station finisher camera hierarchy.";
                return false;
            }

            return true;
        }

        private void InterruptAndRestore(string reason)
        {
            wasInterrupted = true;
            SetError(reason);
            RestoreLease();
        }

        private void RestoreLease()
        {
            RestoreWrittenValues();
            releaseCount++;
            leaseActive = false;
            resultCoverReleaseScheduled = false;
            ClearActiveLeaseReferences();
        }

        private void RestoreWrittenValues()
        {
            if (leasedFinisherCamera != null
                && finisherCameraEnabledWritten
                && leasedFinisherCamera.enabled)
            {
                leasedFinisherCamera.enabled = previousFinisherCameraEnabled;
            }

            if (leasedGameplayCamera != null
                && gameplayCameraEnabledWritten
                && !leasedGameplayCamera.enabled)
            {
                leasedGameplayCamera.enabled = previousGameplayCameraEnabled;
            }

            if (leasedDirector == null)
            {
                return;
            }

            bool stillOwnsDirectorMode =
                leasedDirector.timeUpdateMode == DirectorUpdateMode.Manual;
            bool stillOwnsDirectorTime =
                Approximately(leasedDirector.time, lastWrittenDirectorTime);
            bool stillOwnsDirectorGraph = directorGraphEvaluated
                && leasedDirector.playableAsset == leasedTimeline
                && stillOwnsDirectorMode
                && stillOwnsDirectorTime;
            if (stillOwnsDirectorGraph)
            {
                try
                {
                    leasedDirector.Stop();
                }
                catch (Exception exception)
                {
                    AppendError(
                        $"The Station finisher PlayableDirector could not stop safely: {exception.Message}");
                }
            }

            if (directorTimeWritten
                && leasedDirector.playableAsset == leasedTimeline
                && stillOwnsDirectorTime)
            {
                leasedDirector.time = previousDirectorTime;
            }

            if (directorModeWritten
                && stillOwnsDirectorMode
                && leasedDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                leasedDirector.timeUpdateMode = previousDirectorUpdateMode;
            }
        }

        private void ClearActiveLeaseReferences()
        {
            activeOwner = null;
            leasedDirector = null;
            leasedTimeline = null;
            leasedGameplayCamera = null;
            leasedFinisherCamera = null;
            directorModeWritten = false;
            directorTimeWritten = false;
            directorGraphEvaluated = false;
            gameplayCameraEnabledWritten = false;
            finisherCameraEnabledWritten = false;
        }

        private void SetError(string error)
        {
            LastError = string.IsNullOrWhiteSpace(error) ? string.Empty : error;
        }

        private void AppendError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return;
            }

            LastError = string.IsNullOrEmpty(LastError)
                ? error
                : LastError + " " + error;
        }

        private static bool Approximately(double left, double right)
        {
            return Math.Abs(left - right) <= TimelineToleranceSeconds;
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
        }

        private static int NextPositiveVersion(int current)
        {
            return current >= int.MaxValue ? 1 : current + 1;
        }
    }
}
