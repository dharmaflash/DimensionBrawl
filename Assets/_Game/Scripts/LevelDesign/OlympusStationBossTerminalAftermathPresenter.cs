using System;
using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    /// <summary>
    /// Station-owned presentation gate between the canonical boss-death fact and
    /// the shared result surface. This component observes death; it never creates
    /// damage, chooses an encounter outcome, commits facts, or owns time scale.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OlympusStationBossTerminalAftermathPresenter : MonoBehaviour
    {
        [Header("Terminal Source")]
        [SerializeField] private CombatHealth bossHealth;

        [Header("Presentation Owners")]
        [SerializeField] private BossBarrageCameraCueDriver cameraCueDriver;
        [SerializeField] private OlympusStationBossTerminalFinisherCameraController
            finisherCameraController;
        [SerializeField] private ActionCinematicCueDirector actionCinematicCueDirector;
        [SerializeField] private BossBarrageVisualCueDriver visualCueDriver;
        [SerializeField] private GameObject terminalBoundaryVisualRoot;
        [SerializeField, Min(0.1f)] private float aftermathDurationSeconds = 2.6f;
        [SerializeField, Min(0.1f)] private float unattachedResultLeaseTimeoutSeconds = 2f;
        [SerializeField, Min(0f)] private float initialHitStopRecoveryGraceSeconds = 0.35f;

        [Header("Player Input Owners")]
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private PlayerActionController playerActionController;
        [SerializeField] private PlayerSkill1Action playerSkill1Action;
        [SerializeField] private PlayerSummonSlot1Action playerSummonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction playerSummonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction playerSummonSlot3Action;
        [SerializeField] private PlayerRangedBasicAttackAction playerRangedBasicAttackAction;
        [SerializeField] private PlayerCombatModeController playerCombatModeController;

        private CombatHealth subscribedBossHealth;
        private Coroutine aftermathRoutine;
        private string attachedResultDigest = string.Empty;
        private bool started;
        private bool complete;
        private bool resultAttached;
        private bool inputLeaseActive;
        private bool resultLeaseTimedOut;
        private bool cancelled;
        private bool handoffImminent;
        private bool scaleOneObserved;
        private bool scaleOneViolationRecorded;
        private bool playerMovementLeaseHeld;
        private bool playerActionLeaseHeld;
        private bool playerSkill1LeaseHeld;
        private bool playerSummon1LeaseHeld;
        private bool playerSummon2LeaseHeld;
        private bool playerSummon3LeaseHeld;
        private bool playerRangedLeaseHeld;
        private bool playerCombatModeLeaseHeld;
        private bool terminalBoundaryVisualLeaseActive;
        private bool terminalBoundaryVisualWasActive;
        private bool terminalBoundaryVisualWritten;
        private float elapsedUnscaledSeconds;
        private float startedRealtimeSinceStartup = -1f;
        private float resultAttachedRealtimeSinceStartup = -1f;
        private float completedRealtimeSinceStartup = -1f;
        private int beginCount;
        private int handoffImminentCount;
        private int completeCount;

        public CombatHealth BossHealth => bossHealth;
        public OlympusStationBossTerminalFinisherCameraController FinisherCameraController =>
            finisherCameraController;
        public ActionCinematicCueDirector ActionCinematicCueDirector =>
            actionCinematicCueDirector;
        public GameObject TerminalBoundaryVisualRoot => terminalBoundaryVisualRoot;
        public float AftermathDurationSeconds => Mathf.Max(0.1f, aftermathDurationSeconds);
        public float ElapsedUnscaledSeconds => elapsedUnscaledSeconds;
        public float StartedRealtimeSinceStartup => startedRealtimeSinceStartup;
        public float ResultAttachedRealtimeSinceStartup => resultAttachedRealtimeSinceStartup;
        public float CompletedRealtimeSinceStartup => completedRealtimeSinceStartup;
        public bool IsStarted => started;
        public bool IsRunning => started && !complete;
        public bool IsComplete => complete;
        public bool IsResultAttached => resultAttached;
        public bool InputLeaseActive => inputLeaseActive;
        public bool InputLeaseFullyAcquired => playerMovementLeaseHeld
            && playerActionLeaseHeld
            && playerSkill1LeaseHeld
            && playerSummon1LeaseHeld
            && playerSummon2LeaseHeld
            && playerSummon3LeaseHeld
            && playerRangedLeaseHeld
            && playerCombatModeLeaseHeld;
        public bool ResultLeaseTimedOut => resultLeaseTimedOut;
        public bool IsCancelled => cancelled;
        public bool IsHandoffImminent => handoffImminent;
        public bool ScaleOneObserved => scaleOneObserved;
        public bool ScaleOneViolationRecorded => scaleOneViolationRecorded;
        public int BeginCount => beginCount;
        public int HandoffImminentCount => handoffImminentCount;
        public int CompleteCount => completeCount;
        public string AttachedResultDigest => attachedResultDigest;
        public string LastError { get; private set; } = string.Empty;
        public string LastQualityWarning { get; private set; } = string.Empty;
        public bool CameraCueSucceeded { get; private set; }
        public bool FinisherCameraSucceeded { get; private set; }
        public bool FinisherCameraReleaseScheduled { get; private set; }
        public bool FinisherCameraInterrupted { get; private set; }
        public bool FallbackCameraCueSucceeded { get; private set; }
        public bool TerminalBoundaryVisualHidden { get; private set; }
        public int FinisherCameraRequestVersion { get; private set; } = -1;
        public bool CinematicTakeoverSucceeded { get; private set; }
        public bool CinematicOwnedStateCleanupSucceeded { get; private set; }
        public bool CinematicStopRequestSucceeded { get; private set; }
        public bool VisualAudioCueSucceeded { get; private set; }
        public bool CompletedSuccessfully => complete
            && !cancelled
            && string.IsNullOrEmpty(LastError);

        public event Action AftermathStarted;
        public event Action AftermathHandoffImminent;
        public event Action AftermathCompleted;

        private void OnEnable()
        {
            SubscribeBossHealth();
            if (!started && bossHealth != null && !bossHealth.IsAlive)
            {
                HandleBossDied();
            }
        }

        private void OnDisable()
        {
            UnsubscribeBossHealth();
            if (aftermathRoutine != null)
            {
                StopCoroutine(aftermathRoutine);
                aftermathRoutine = null;
            }

            if (started
                && (!complete
                    || inputLeaseActive
                    || IsFinisherCameraLeaseOwned()
                    || terminalBoundaryVisualLeaseActive))
            {
                CancelAndRelease("The Station boss-terminal aftermath owner was disabled before result handoff.");
            }
        }

        /// <summary>
        /// Attaches the already committed canonical result to the death-anchored
        /// gate. Timing is never started here: it begins only from bossHealth.Died.
        /// </summary>
        public bool TryAttachResult(StageRunResultSummary summary, out string error)
        {
            error = string.Empty;
            if (!IsCanonicalBossTerminalClear(summary))
            {
                error = "The Station aftermath gate accepts only a canonical BossTerminal clear result.";
                return false;
            }

            if (!started)
            {
                error = "The canonical result arrived before the authored boss Died aftermath began.";
                return false;
            }

            if (resultLeaseTimedOut)
            {
                error = "The result handoff arrived after the aftermath input lease timed out.";
                return false;
            }

            if (cancelled)
            {
                error = "The Station aftermath gate was cancelled before result attachment.";
                return false;
            }

            string digest = summary.ResultSummaryDigest ?? string.Empty;
            if (resultAttached)
            {
                if (string.Equals(attachedResultDigest, digest, StringComparison.Ordinal))
                {
                    return true;
                }

                error = "A different canonical result is already attached to the Station aftermath gate.";
                return false;
            }

            if (bossHealth == null || bossHealth.IsAlive)
            {
                error = "The canonical BossTerminal result cannot attach while the authored boss is alive.";
                return false;
            }

            resultAttached = true;
            attachedResultDigest = digest;
            resultAttachedRealtimeSinceStartup = Time.realtimeSinceStartup;
            return true;
        }

        /// <summary>
        /// Releases only this component's independent input-lock bit. The result
        /// overlay calls this after it has atomically taken ownership via scale 0.
        /// </summary>
        public void ReleaseInputLeaseForResultSurface()
        {
            ScheduleFinisherCameraReleaseAfterResultCover();
            CommitTerminalBoundaryVisualForResultSurface();
            if (!inputLeaseActive)
            {
                return;
            }

            const PlayerInputLockSource source = PlayerInputLockSource.BossTerminalAftermath;
            playerMovementLeaseHeld = TryRelease(
                playerMovementLeaseHeld,
                () => playerMovement.SetCinematicMoveInputLocked(source, false),
                nameof(playerMovement));
            playerActionLeaseHeld = TryRelease(
                playerActionLeaseHeld,
                () => playerActionController.SetCinematicInputLocked(source, false),
                nameof(playerActionController));
            playerSkill1LeaseHeld = TryRelease(
                playerSkill1LeaseHeld,
                () => playerSkill1Action.SetCinematicInputLocked(source, false),
                nameof(playerSkill1Action));
            playerSummon1LeaseHeld = TryRelease(
                playerSummon1LeaseHeld,
                () => playerSummonSlot1Action.SetCinematicInputLocked(source, false),
                nameof(playerSummonSlot1Action));
            playerSummon2LeaseHeld = TryRelease(
                playerSummon2LeaseHeld,
                () => playerSummonSlot2Action.SetCinematicInputLocked(source, false),
                nameof(playerSummonSlot2Action));
            playerSummon3LeaseHeld = TryRelease(
                playerSummon3LeaseHeld,
                () => playerSummonSlot3Action.SetCinematicInputLocked(source, false),
                nameof(playerSummonSlot3Action));
            playerRangedLeaseHeld = TryRelease(
                playerRangedLeaseHeld,
                () => playerRangedBasicAttackAction.SetCinematicInputLocked(source, false),
                nameof(playerRangedBasicAttackAction));
            playerCombatModeLeaseHeld = TryRelease(
                playerCombatModeLeaseHeld,
                () => playerCombatModeController.SetCinematicInputLocked(source, false),
                nameof(playerCombatModeController));
            RefreshInputLeaseActive();
        }

        /// <summary>
        /// Terminal failure escape hatch used by result fact/commit/overlay owners.
        /// It never changes health, encounter state, result facts, or time scale.
        /// </summary>
        public void CancelAndRelease(string reason)
        {
            if (complete
                && !inputLeaseActive
                && !cancelled
                && !IsFinisherCameraLeaseOwned()
                && !terminalBoundaryVisualLeaseActive)
            {
                return;
            }

            if (!cancelled)
            {
                cancelled = true;
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    RecordError(reason);
                }
            }

            if (aftermathRoutine != null)
            {
                try
                {
                    StopCoroutine(aftermathRoutine);
                }
                catch (Exception exception)
                {
                    RecordError($"Could not stop the aftermath routine during cancellation: {exception.Message}");
                }

                aftermathRoutine = null;
            }

            CancelFinisherCameraImmediately(reason);
            RestoreTerminalBoundaryVisual();
            ReleaseInputLeaseForResultSurface();
        }

        private void SubscribeBossHealth()
        {
            if (subscribedBossHealth == bossHealth)
            {
                return;
            }

            UnsubscribeBossHealth();
            subscribedBossHealth = bossHealth;
            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Died += HandleBossDied;
            }
        }

        private void UnsubscribeBossHealth()
        {
            if (subscribedBossHealth != null)
            {
                subscribedBossHealth.Died -= HandleBossDied;
            }

            subscribedBossHealth = null;
        }

        private void HandleBossDied()
        {
            try
            {
                if (started)
                {
                    return;
                }

                BeginAftermathFromBossDied();
            }
            catch (Exception exception)
            {
                // CombatHealth invokes Died from inside its authoritative damage
                // mutation. Nothing in this presentation observer may escape
                // back into that mutation, including its own failure cleanup.
                try
                {
                    RecordError($"The boss Died aftermath boundary failed safely: {exception.Message}");
                    Debug.LogException(exception, this);
                    CancelAndRelease("The boss Died aftermath could not start safely.");
                }
                catch (Exception cleanupException)
                {
                    try
                    {
                        Debug.LogException(cleanupException, this);
                    }
                    catch
                    {
                        // Deliberately swallow the final diagnostic boundary.
                    }
                }
            }
        }

        private void BeginAftermathFromBossDied()
        {

            started = true;
            complete = false;
            resultAttached = false;
            resultLeaseTimedOut = false;
            cancelled = false;
            handoffImminent = false;
            scaleOneObserved = Mathf.Approximately(Time.timeScale, 1f);
            scaleOneViolationRecorded = false;
            elapsedUnscaledSeconds = 0f;
            startedRealtimeSinceStartup = Time.realtimeSinceStartup;
            resultAttachedRealtimeSinceStartup = -1f;
            completedRealtimeSinceStartup = -1f;
            attachedResultDigest = string.Empty;
            LastError = string.Empty;
            LastQualityWarning = string.Empty;
            CameraCueSucceeded = false;
            FinisherCameraSucceeded = false;
            FinisherCameraReleaseScheduled = false;
            FinisherCameraInterrupted = false;
            FallbackCameraCueSucceeded = false;
            TerminalBoundaryVisualHidden = false;
            FinisherCameraRequestVersion = -1;
            CinematicTakeoverSucceeded = false;
            CinematicOwnedStateCleanupSucceeded = false;
            CinematicStopRequestSucceeded = false;
            VisualAudioCueSucceeded = false;
            beginCount++;

            AcquireInputLease();
            if (!InputLeaseFullyAcquired)
            {
                CancelAndRelease(
                    "The authored Station aftermath could not acquire all eight input-owner leases.");
                return;
            }

            HideTerminalBoundaryVisual();

            try
            {
                CinematicTakeoverSucceeded = actionCinematicCueDirector != null
                    && actionCinematicCueDirector.CancelForBossTerminalAftermath();
                CinematicOwnedStateCleanupSucceeded = actionCinematicCueDirector != null
                    && actionCinematicCueDirector.LastBossTerminalOwnedStateCleanupSucceeded;
                CinematicStopRequestSucceeded = actionCinematicCueDirector != null
                    && actionCinematicCueDirector.LastBossTerminalStopRequestSucceeded;
            }
            catch (Exception exception)
            {
                RecordQualityWarning(
                    $"The action cinematic terminal takeover threw safely: {exception.Message}");
            }

            if (!CinematicTakeoverSucceeded)
            {
                RecordQualityWarning(
                    "The action cinematic terminal takeover could not secure the camera stream.");
            }
            else if (!CinematicOwnedStateCleanupSucceeded)
            {
                RecordQualityWarning(
                    "The action cinematic camera stream was secured, but an owned input/time cleanup step failed safely.");
            }

            if (CinematicTakeoverSucceeded && !CinematicStopRequestSucceeded)
            {
                RecordQualityWarning(
                    "The action cinematic camera stream was secured by suppression after its explicit stop request failed safely.");
            }

            TryAcquireFinisherCamera();
            if (!FinisherCameraSucceeded)
            {
                TryRequestFallbackCameraCue();
            }

            CameraCueSucceeded = FinisherCameraSucceeded || FallbackCameraCueSucceeded;

            if (!CameraCueSucceeded)
            {
                RecordQualityWarning("The authored boss-death camera streams were unavailable.");
            }

            try
            {
                VisualAudioCueSucceeded = visualCueDriver != null
                    && visualCueDriver.TryPlayBossDeathCue();
            }
            catch (Exception exception)
            {
                RecordQualityWarning(
                    $"The authored boss-death VFX/audio cue threw safely: {exception.Message}");
            }

            if (!VisualAudioCueSucceeded)
            {
                RecordQualityWarning("The authored boss-death VFX/audio cue was unavailable.");
            }

            InvokeSafely(AftermathStarted, "started");
            if (isActiveAndEnabled)
            {
                try
                {
                    aftermathRoutine = StartCoroutine(RunAftermath());
                }
                catch (Exception exception)
                {
                    RecordError($"The authored aftermath coroutine threw safely: {exception.Message}");
                    CompleteAftermath();
                    CancelAndRelease("The authored aftermath coroutine could not run.");
                }
            }
            else
            {
                RecordError("The Station aftermath owner was inactive at boss Died.");
                CompleteAftermath();
                CancelAndRelease(
                    "The Station aftermath owner became inactive while the Died delegate snapshot was dispatching.");
            }
        }

        private IEnumerator RunAftermath()
        {
            float duration = AftermathDurationSeconds;
            while (elapsedUnscaledSeconds + 0.00001f < duration)
            {
                yield return null;
                float deltaTime = Mathf.Max(
                    0f,
                    PresentationClock.UnscaledDeltaTime);
                elapsedUnscaledSeconds = Mathf.Min(duration, elapsedUnscaledSeconds + deltaTime);
                SampleFinisherCamera(elapsedUnscaledSeconds);
                ObserveAuthoredTimeScale();
                if (elapsedUnscaledSeconds
                    + Mathf.Max(0.00001f, deltaTime)
                    + 0.00001f >= duration)
                {
                    SignalHandoffImminent();
                }
            }

            CompleteAftermath();

            float unattachedElapsed = 0f;
            float timeout = Mathf.Max(0.1f, unattachedResultLeaseTimeoutSeconds);
            while (inputLeaseActive && !resultAttached && unattachedElapsed + 0.00001f < timeout)
            {
                yield return null;
                unattachedElapsed += Mathf.Max(
                    0f,
                    PresentationClock.UnscaledDeltaTime);
            }

            if (inputLeaseActive && !resultAttached)
            {
                resultLeaseTimedOut = true;
                aftermathRoutine = null;
                CancelAndRelease("No canonical result overlay attached before the aftermath input-lease timeout.");
                yield break;
            }

            aftermathRoutine = null;
        }

        private void CompleteAftermath()
        {
            if (complete)
            {
                return;
            }

            SignalHandoffImminent();

            if (FinisherCameraSucceeded
                && (finisherCameraController == null
                    || !finisherCameraController.IsOwnedBy(this)
                    || finisherCameraController.WasInterrupted))
            {
                FinisherCameraSucceeded = false;
                FinisherCameraInterrupted = true;
                CameraCueSucceeded = FallbackCameraCueSucceeded;
                RecordQualityWarning(
                    "The authored Station finisher camera was interrupted before the exact 2.6s result handoff.");
            }
            else if (FinisherCameraSucceeded
                && !finisherCameraController.HasReachedTerminalSample)
            {
                FinisherCameraSucceeded = false;
                CameraCueSucceeded = FallbackCameraCueSucceeded;
                RecordQualityWarning(
                    "The authored Station finisher Timeline did not receive its exact 2.6s terminal sample.");
            }

            if (FallbackCameraCueSucceeded
                && cameraCueDriver != null
                && cameraCueDriver.BossDeathCueWasInterrupted)
            {
                RecordQualityWarning(
                    "The authored boss-death camera cue was interrupted before the exact 2.6s result handoff.");
            }
            else if (FallbackCameraCueSucceeded
                && cameraCueDriver != null
                && !cameraCueDriver.IsBossDeathCueComplete)
            {
                RecordQualityWarning(
                    "The authored boss-death camera cue did not settle before the exact 2.6s result handoff.");
            }

            elapsedUnscaledSeconds = AftermathDurationSeconds;
            completedRealtimeSinceStartup = Time.realtimeSinceStartup;
            complete = true;
            completeCount++;
            InvokeSafely(AftermathCompleted, "completed");
        }

        private void SignalHandoffImminent()
        {
            if (handoffImminent)
            {
                return;
            }

            handoffImminent = true;
            handoffImminentCount++;
            InvokeSafely(AftermathHandoffImminent, "handoff-imminent");
        }

        private void TryAcquireFinisherCamera()
        {
            if (!CinematicTakeoverSucceeded)
            {
                RecordQualityWarning(
                    "The Station finisher camera did not acquire because the action-cinematic stream was not secured.");
                return;
            }

            if (finisherCameraController == null)
            {
                RecordQualityWarning("The authored Station finisher camera owner is missing.");
                return;
            }

            try
            {
                FinisherCameraSucceeded = finisherCameraController.TryAcquire(
                    this,
                    out int requestVersion);
                FinisherCameraRequestVersion = FinisherCameraSucceeded
                    ? requestVersion
                    : -1;
            }
            catch (Exception exception)
            {
                FinisherCameraSucceeded = false;
                RecordQualityWarning(
                    $"The authored Station finisher camera threw safely: {exception.Message}");
            }

            if (!FinisherCameraSucceeded)
            {
                string detail = finisherCameraController.LastError;
                RecordQualityWarning(string.IsNullOrWhiteSpace(detail)
                    ? "The authored Station finisher camera was unavailable."
                    : $"The authored Station finisher camera was unavailable: {detail}");
            }
        }

        private void TryRequestFallbackCameraCue()
        {
            try
            {
                FallbackCameraCueSucceeded = CinematicTakeoverSucceeded
                    && cameraCueDriver != null
                    && cameraCueDriver.TryRequestBossDeathCue(out _);
            }
            catch (Exception exception)
            {
                FallbackCameraCueSucceeded = false;
                RecordQualityWarning(
                    $"The fallback boss-death camera cue threw safely: {exception.Message}");
            }

            if (!FallbackCameraCueSucceeded)
            {
                RecordQualityWarning("The fallback boss-death camera cue was unavailable.");
            }
        }

        private void SampleFinisherCamera(float elapsedSeconds)
        {
            if (!FinisherCameraSucceeded || finisherCameraController == null)
            {
                return;
            }

            bool sampled;
            try
            {
                sampled = finisherCameraController.Sample(this, elapsedSeconds);
            }
            catch (Exception exception)
            {
                sampled = false;
                RecordQualityWarning(
                    $"The Station finisher Timeline sample threw safely: {exception.Message}");
                CancelFinisherCameraImmediately(
                    "The Station finisher Timeline sample could not continue safely.");
            }

            if (sampled)
            {
                return;
            }

            FinisherCameraSucceeded = false;
            FinisherCameraInterrupted = true;
            CameraCueSucceeded = FallbackCameraCueSucceeded;
            string detail = finisherCameraController.LastError;
            RecordQualityWarning(string.IsNullOrWhiteSpace(detail)
                ? "The Station finisher Timeline sampling was interrupted."
                : $"The Station finisher Timeline sampling was interrupted: {detail}");
        }

        private void ScheduleFinisherCameraReleaseAfterResultCover()
        {
            if (FinisherCameraReleaseScheduled
                || finisherCameraController == null
                || !finisherCameraController.IsOwnedBy(this))
            {
                return;
            }

            try
            {
                FinisherCameraReleaseScheduled =
                    finisherCameraController.ScheduleReleaseAfterResultCover(this);
            }
            catch (Exception exception)
            {
                RecordQualityWarning(
                    $"The Station finisher result-cover release threw safely: {exception.Message}");
            }

            if (FinisherCameraReleaseScheduled)
            {
                return;
            }

            FinisherCameraSucceeded = false;
            FinisherCameraInterrupted = true;
            CameraCueSucceeded = FallbackCameraCueSucceeded;
            CancelFinisherCameraImmediately(
                "The Station finisher result-cover release could not be scheduled safely.");
        }

        private void CancelFinisherCameraImmediately(string reason)
        {
            if (finisherCameraController == null
                || !finisherCameraController.IsOwnedBy(this))
            {
                return;
            }

            try
            {
                if (finisherCameraController.CancelAndRestore(this, reason))
                {
                    FinisherCameraInterrupted = true;
                }
            }
            catch (Exception exception)
            {
                RecordError(
                    $"Could not restore the Station finisher camera safely: {exception.Message}");
            }
        }

        private bool IsFinisherCameraLeaseOwned()
        {
            try
            {
                return finisherCameraController != null
                    && finisherCameraController.IsOwnedBy(this);
            }
            catch (Exception exception)
            {
                RecordError(
                    $"Could not inspect the Station finisher camera lease safely: {exception.Message}");
                return false;
            }
        }

        private void HideTerminalBoundaryVisual()
        {
            terminalBoundaryVisualLeaseActive = false;
            terminalBoundaryVisualWritten = false;
            TerminalBoundaryVisualHidden = false;
            if (terminalBoundaryVisualRoot == null)
            {
                RecordQualityWarning(
                    "The Station terminal boundary visual root is missing.");
                return;
            }

            try
            {
                terminalBoundaryVisualWasActive =
                    terminalBoundaryVisualRoot.activeSelf;
                terminalBoundaryVisualLeaseActive = true;
                if (terminalBoundaryVisualWasActive)
                {
                    terminalBoundaryVisualRoot.SetActive(false);
                    terminalBoundaryVisualWritten = true;
                }

                TerminalBoundaryVisualHidden =
                    !terminalBoundaryVisualRoot.activeSelf;
                if (!TerminalBoundaryVisualHidden)
                {
                    RecordQualityWarning(
                        "The Station terminal boundary visual did not hide at boss Died.");
                }
            }
            catch (Exception exception)
            {
                terminalBoundaryVisualLeaseActive = false;
                terminalBoundaryVisualWritten = false;
                RecordQualityWarning(
                    $"The Station terminal boundary visual could not hide safely: {exception.Message}");
            }
        }

        private void RestoreTerminalBoundaryVisual()
        {
            if (!terminalBoundaryVisualLeaseActive)
            {
                return;
            }

            try
            {
                if (terminalBoundaryVisualRoot != null
                    && terminalBoundaryVisualWritten
                    && !terminalBoundaryVisualRoot.activeSelf)
                {
                    terminalBoundaryVisualRoot.SetActive(
                        terminalBoundaryVisualWasActive);
                }

                TerminalBoundaryVisualHidden = terminalBoundaryVisualRoot != null
                    && !terminalBoundaryVisualRoot.activeSelf;
            }
            catch (Exception exception)
            {
                RecordError(
                    $"Could not restore the Station terminal boundary visual safely: {exception.Message}");
            }
            finally
            {
                terminalBoundaryVisualLeaseActive = false;
                terminalBoundaryVisualWritten = false;
            }
        }

        private void CommitTerminalBoundaryVisualForResultSurface()
        {
            if (!terminalBoundaryVisualLeaseActive)
            {
                return;
            }

            TerminalBoundaryVisualHidden = terminalBoundaryVisualRoot != null
                && !terminalBoundaryVisualRoot.activeSelf;
            terminalBoundaryVisualLeaseActive = false;
            terminalBoundaryVisualWritten = false;
        }

        private void AcquireInputLease()
        {
            if (inputLeaseActive)
            {
                return;
            }

            const PlayerInputLockSource source = PlayerInputLockSource.BossTerminalAftermath;
            playerMovementLeaseHeld = TryAcquire(
                playerMovement,
                () => playerMovement.SetCinematicMoveInputLocked(source, true),
                nameof(playerMovement));
            playerActionLeaseHeld = TryAcquire(
                playerActionController,
                () => playerActionController.SetCinematicInputLocked(source, true),
                nameof(playerActionController));
            playerSkill1LeaseHeld = TryAcquire(
                playerSkill1Action,
                () => playerSkill1Action.SetCinematicInputLocked(source, true),
                nameof(playerSkill1Action));
            playerSummon1LeaseHeld = TryAcquire(
                playerSummonSlot1Action,
                () => playerSummonSlot1Action.SetCinematicInputLocked(source, true),
                nameof(playerSummonSlot1Action));
            playerSummon2LeaseHeld = TryAcquire(
                playerSummonSlot2Action,
                () => playerSummonSlot2Action.SetCinematicInputLocked(source, true),
                nameof(playerSummonSlot2Action));
            playerSummon3LeaseHeld = TryAcquire(
                playerSummonSlot3Action,
                () => playerSummonSlot3Action.SetCinematicInputLocked(source, true),
                nameof(playerSummonSlot3Action));
            playerRangedLeaseHeld = TryAcquire(
                playerRangedBasicAttackAction,
                () => playerRangedBasicAttackAction.SetCinematicInputLocked(source, true),
                nameof(playerRangedBasicAttackAction));
            playerCombatModeLeaseHeld = TryAcquire(
                playerCombatModeController,
                () => playerCombatModeController.SetCinematicInputLocked(source, true),
                nameof(playerCombatModeController));
            RefreshInputLeaseActive();
        }

        private bool TryAcquire(UnityEngine.Object owner, Action action, string ownerName)
        {
            if (owner == null)
            {
                RecordError($"The authored {ownerName} input owner is missing.");
                return false;
            }

            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                RecordError($"Could not acquire {ownerName} aftermath input lease: {exception.Message}");
                return false;
            }
        }

        private bool TryRelease(bool leaseHeld, Action action, string ownerName)
        {
            if (!leaseHeld)
            {
                return false;
            }

            try
            {
                action.Invoke();
                return false;
            }
            catch (Exception exception)
            {
                RecordError($"Could not release {ownerName} aftermath input lease: {exception.Message}");
                return true;
            }
        }

        private void RefreshInputLeaseActive()
        {
            inputLeaseActive = playerMovementLeaseHeld
                || playerActionLeaseHeld
                || playerSkill1LeaseHeld
                || playerSummon1LeaseHeld
                || playerSummon2LeaseHeld
                || playerSummon3LeaseHeld
                || playerRangedLeaseHeld
                || playerCombatModeLeaseHeld;
        }

        private void ObserveAuthoredTimeScale()
        {
            bool scaleOne = Mathf.Approximately(Time.timeScale, 1f);
            if (scaleOne)
            {
                scaleOneObserved = true;
                return;
            }

            bool withinInitialHitStop = !scaleOneObserved
                && elapsedUnscaledSeconds <= Mathf.Max(0f, initialHitStopRecoveryGraceSeconds);
            if (withinInitialHitStop || scaleOneViolationRecorded)
            {
                return;
            }

            scaleOneViolationRecorded = true;
            string reason = scaleOneObserved
                ? $"timeScale left 1 during the authored aftermath (value={Time.timeScale:R})."
                : $"The lethal hit-stop did not recover to timeScale 1 within {initialHitStopRecoveryGraceSeconds:R}s.";
            RecordError(reason);
        }

        private static bool IsCanonicalBossTerminalClear(StageRunResultSummary summary)
        {
            return summary != null
                && summary.Outcome == StageRouteOutcome.Clear
                && summary.OutcomeFact != null
                && summary.OutcomeFact.OutcomeDisposition == StageOutcomeDisposition.Clear
                && summary.OutcomeFact.ClearReason == StageClearReason.BossTerminal;
        }

        private void RecordError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return;
            }

            LastError = string.IsNullOrEmpty(LastError)
                ? error
                : LastError + " " + error;
        }

        private void RecordQualityWarning(string warning)
        {
            if (string.IsNullOrWhiteSpace(warning))
            {
                return;
            }

            LastQualityWarning = string.IsNullOrEmpty(LastQualityWarning)
                ? warning
                : LastQualityWarning + " " + warning;
        }

        private void InvokeSafely(Action callback, string phase)
        {
            if (callback == null)
            {
                return;
            }

            Delegate[] listeners = callback.GetInvocationList();
            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    ((Action)listeners[i]).Invoke();
                }
                catch (Exception exception)
                {
                    RecordQualityWarning(
                        $"An aftermath {phase} listener failed safely: {exception.Message}");
                    Debug.LogException(exception, this);
                }
            }
        }
    }
}
