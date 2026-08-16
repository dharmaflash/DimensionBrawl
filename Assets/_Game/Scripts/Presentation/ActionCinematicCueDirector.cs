using System;
using System.Collections;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed partial class ActionCinematicCueDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ActionCinematicCueProfile cueProfile;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform cueSpace;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform vfxAnchor;
        [SerializeField] private Animator cueAnimator;

        [Header("Playback Gate")]
        [SerializeField] private bool allowCuePlayback = true;

        [Header("Timing")]
        [SerializeField] private bool useUnscaledClock = true;

        [Header("Screen Framing")]
        [SerializeField] private bool drawCinematicBars = true;
        [SerializeField, Range(0f, 0.22f)] private float maxBarScreenRatio = 0.085f;
        [SerializeField, Range(0f, 1f)] private float maxBarAlpha = 0.62f;

        private Coroutine activeRoutine;
        private int activePriority;
        private bool activeCanBeInterrupted = true;
        private bool hasStoredTimeScale;
        private float storedTimeScale = 1f;
        private bool movementLockActive;
        private bool inputLockActive;
        private bool terminalPlaybackSuppressed;
        private bool terminalCameraStreamSecured;
        private bool lastBossTerminalOwnedStateCleanupSucceeded = true;
        private bool lastBossTerminalStopRequestSucceeded = true;
        private int bossTerminalCancellationCount;
        private bool lastBossTerminalCancellationStoppedActiveCue;
        private int totalPlayCount;
        private int totalSignalCount;
        private int animatorTriggerRequestCount;
        private int vfxCueRequestCount;
        private int bossPressureBreakPlayCount;
        private int summonFollowupHitPlayCount;
        private int summonFollowupHitFrameOverlayCount;
        private int summonRecallPlayCount;
        private ActionCinematicCueProfile.CueKind lastPlayedKind;
        private int lastPlayedTier;
        private string lastPlayedCueId;
        private int lastSummonFollowupHitTier;
        private string lastSummonFollowupHitCueId;
        private string lastSignalId;
        private string lastAnimatorTrigger;
        private CombatVfxCueId lastVfxCueId;
        private float frameEndTime;
        private float frameDuration;

        public ActionCinematicCueProfile CueProfile => cueProfile;
        public ActionCameraController CameraController => cameraController;
        public Transform CueSpace => cueSpace;
        public bool AllowCuePlayback => allowCuePlayback;
        public bool DrawCinematicBars => drawCinematicBars;
        public bool HasActiveFrameOverlay => ResolveFrameSecondsRemaining() > 0f;
        public bool HasActiveMovementLock => movementLockActive;
        public bool HasActiveInputLock => inputLockActive;
        public bool IsPlaying => activeRoutine != null;
        public bool TerminalPlaybackSuppressed => terminalPlaybackSuppressed;
        public bool TerminalCameraStreamSecured => terminalCameraStreamSecured;
        public bool LastBossTerminalOwnedStateCleanupSucceeded =>
            lastBossTerminalOwnedStateCleanupSucceeded;
        public bool LastBossTerminalStopRequestSucceeded =>
            lastBossTerminalStopRequestSucceeded;
        public int BossTerminalCancellationCount => bossTerminalCancellationCount;
        public bool LastBossTerminalCancellationStoppedActiveCue =>
            lastBossTerminalCancellationStoppedActiveCue;
        public bool HasOwnedTimeScaleLease => hasStoredTimeScale;
        public int TotalPlayCount => totalPlayCount;
        public int TotalSignalCount => totalSignalCount;
        public int AnimatorTriggerRequestCount => animatorTriggerRequestCount;
        public int VfxCueRequestCount => vfxCueRequestCount;
        public int BossPressureBreakPlayCount => bossPressureBreakPlayCount;
        public int SummonFollowupHitPlayCount => summonFollowupHitPlayCount;
        public int SummonFollowupHitFrameOverlayCount => summonFollowupHitFrameOverlayCount;
        public int SummonRecallPlayCount => summonRecallPlayCount;
        public ActionCinematicCueProfile.CueKind LastPlayedKind => lastPlayedKind;
        public int LastPlayedTier => lastPlayedTier;
        public string LastPlayedCueId => lastPlayedCueId;
        public int LastSummonFollowupHitTier => lastSummonFollowupHitTier;
        public string LastSummonFollowupHitCueId => lastSummonFollowupHitCueId;
        public string LastSignalId => lastSignalId;
        public string LastAnimatorTrigger => lastAnimatorTrigger;
        public CombatVfxCueId LastVfxCueId => lastVfxCueId;

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }

            ResolveCueSpaceReferences();
        }

        private void OnDisable()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            RestoreCinematicState();
            frameEndTime = 0f;
            frameDuration = 0f;
        }

        private void OnGUI()
        {
            if (!drawCinematicBars || !HasActiveFrameOverlay)
            {
                return;
            }

            float weight = ResolveFrameWeight();
            if (weight <= 0.001f)
            {
                return;
            }

            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            GUI.depth = 900;
            GUI.color = new Color(0f, 0f, 0f, maxBarAlpha * weight);
            float barHeight = Screen.height * maxBarScreenRatio * weight;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, barHeight), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, Screen.height - barHeight, Screen.width, barHeight), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }

        public bool TryPlay(ActionCinematicCueProfile.CueKind kind, int tier)
        {
            return TryPlay(kind, tier, ResolveDefaultDirection());
        }

        public bool TryPlay(ActionCinematicCueProfile.CueKind kind, int tier, Vector3 planarDirection)
        {
            if (terminalPlaybackSuppressed
                || !allowCuePlayback
                || !isActiveAndEnabled
                || cueProfile == null
                || cameraController == null
                || !cueProfile.TryGetSequence(kind, out ActionCinematicCueProfile.CueSequence sequence))
            {
                return false;
            }

            if (activeRoutine != null)
            {
                if (sequence.priority < activePriority || (!activeCanBeInterrupted && sequence.priority <= activePriority))
                {
                    RecordNonInterruptingFollowupHitCueIfNeeded(kind, tier, sequence);
                    return false;
                }

                StopCoroutine(activeRoutine);
                activeRoutine = null;
                RestoreCinematicState();
            }

            activePriority = sequence.priority;
            activeCanBeInterrupted = sequence.canBeInterrupted;
            lastPlayedKind = kind;
            lastPlayedTier = Mathf.Max(1, tier);
            lastPlayedCueId = sequence.cueId;
            totalPlayCount++;
            frameDuration = EstimateSequenceSeconds(sequence);
            frameEndTime = FrameClock + frameDuration;
            RecordPlayedCueKind(kind, lastPlayedTier, lastPlayedCueId, frameDuration > 0f);
            activeRoutine = StartCoroutine(PlaySequence(sequence, lastPlayedTier, planarDirection));
            return true;
        }

        /// <summary>
        /// Gives the canonical boss-terminal presentation exclusive ownership of
        /// the action-camera cue stream. This stops an in-flight multi-shot cue,
        /// suppresses late encounter callbacks, and releases only state owned by
        /// <see cref="PlayerInputLockSource.CinematicCue"/>. It deliberately does
        /// not disable this component or overwrite a time scale changed by another
        /// owner (for example the lethal hit-stop that precedes boss Died).
        /// </summary>
        /// <returns>
        /// True when future camera writes from this director are suppressed. Input,
        /// time-scale, and explicit-stop diagnostics are exposed separately.
        /// </returns>
        public bool CancelForBossTerminalAftermath()
        {
            bool stopRequestSucceeded = true;
            bool stoppedActiveCue = false;

            if (!terminalPlaybackSuppressed)
            {
                terminalPlaybackSuppressed = true;
                bossTerminalCancellationCount++;
            }

            if (activeRoutine != null)
            {
                try
                {
                    StopCoroutine(activeRoutine);
                    activeRoutine = null;
                    stoppedActiveCue = true;
                }
                catch (Exception exception)
                {
                    stopRequestSucceeded = false;
                    Debug.LogException(exception, this);
                }
            }
            lastBossTerminalCancellationStoppedActiveCue |= stoppedActiveCue;

            activePriority = 0;
            activeCanBeInterrupted = true;
            frameEndTime = 0f;
            frameDuration = 0f;

            bool ownedStateCleanupSucceeded = TryReleaseTerminalOwnedState(
                ReleaseMovementLock,
                "movement input lease");
            ownedStateCleanupSucceeded &= TryReleaseTerminalOwnedState(
                ReleaseInputLock,
                "action input leases");
            ownedStateCleanupSucceeded &= TryReleaseTerminalOwnedState(
                RestoreTimeScale,
                "time-scale lease");
            lastBossTerminalStopRequestSucceeded = stopRequestSucceeded;
            lastBossTerminalOwnedStateCleanupSucceeded = ownedStateCleanupSucceeded;

            // PlaySequence observes terminalPlaybackSuppressed before every later
            // shot. Even if Unity rejects the explicit StopCoroutine request, the
            // surviving iterator cannot write another camera cue.
            terminalCameraStreamSecured = terminalPlaybackSuppressed;
            return terminalCameraStreamSecured;
        }

        private bool TryReleaseTerminalOwnedState(Action release, string label)
        {
            try
            {
                release();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{name} could not release its CinematicCue {label} during boss-terminal takeover: {exception}",
                    this);
                return false;
            }
        }

        private void RecordPlayedCueKind(
            ActionCinematicCueProfile.CueKind kind,
            int tier,
            string cueId,
            bool frameOverlayActive)
        {
            switch (kind)
            {
                case ActionCinematicCueProfile.CueKind.BossPressureBreak:
                    bossPressureBreakPlayCount++;
                    break;
                case ActionCinematicCueProfile.CueKind.SummonFollowupHit:
                    summonFollowupHitPlayCount++;
                    lastSummonFollowupHitTier = tier;
                    lastSummonFollowupHitCueId = cueId ?? string.Empty;
                    if (frameOverlayActive)
                    {
                        summonFollowupHitFrameOverlayCount++;
                    }

                    break;
                case ActionCinematicCueProfile.CueKind.SummonRecall:
                    summonRecallPlayCount++;
                    break;
            }
        }

        private IEnumerator PlaySequence(
            ActionCinematicCueProfile.CueSequence sequence,
            int tier,
            Vector3 planarDirection)
        {
            if (terminalPlaybackSuppressed)
            {
                activeRoutine = null;
                yield break;
            }

            float movementLockTimer = Mathf.Max(0f, sequence.movementLockSeconds);
            float inputLockTimer = Mathf.Max(0f, sequence.inputLockSeconds);

            if (movementLockTimer > 0f)
            {
                ApplyMovementLock();
            }

            if (inputLockTimer > 0f)
            {
                ApplyInputLock();
            }

            float timeScaleTimer = Mathf.Max(0f, sequence.timeScaleSeconds);
            if (timeScaleTimer > 0f && sequence.timeScale > 0f && !Mathf.Approximately(sequence.timeScale, 1f))
            {
                ApplyTimeScale(sequence.timeScale);
            }

            bool[] signalPlayed = sequence.SignalCount > 0 ? new bool[sequence.SignalCount] : null;
            float sequenceElapsed = 0f;
            DispatchDueSignals(sequence, signalPlayed, sequenceElapsed, tier, planarDirection);

            for (int i = 0; i < sequence.shots.Length; i++)
            {
                if (terminalPlaybackSuppressed)
                {
                    break;
                }

                ActionCinematicCueProfile.CameraShot shot = sequence.shots[i];
                if (shot.enabled)
                {
                    RequestShot(shot, tier, planarDirection);
                }

                float waitSeconds = Mathf.Max(0.01f, shot.durationSeconds + Mathf.Max(0f, shot.pauseAfterSeconds));
                float elapsed = 0f;
                while (elapsed < waitSeconds)
                {
                    if (terminalPlaybackSuppressed)
                    {
                        break;
                    }

                    float deltaTime = useUnscaledClock ? Time.unscaledDeltaTime : Time.deltaTime;
                    elapsed += deltaTime;
                    sequenceElapsed += deltaTime;
                    timeScaleTimer = TickTimeScaleTimer(timeScaleTimer, deltaTime);
                    movementLockTimer = TickMovementLockTimer(movementLockTimer, deltaTime);
                    inputLockTimer = TickInputLockTimer(inputLockTimer, deltaTime);
                    DispatchDueSignals(sequence, signalPlayed, sequenceElapsed, tier, planarDirection);
                    yield return null;
                }

                if (terminalPlaybackSuppressed)
                {
                    break;
                }
            }

            RestoreCinematicState();
            activeRoutine = null;
            activePriority = 0;
            activeCanBeInterrupted = true;
        }

        private void RecordNonInterruptingFollowupHitCueIfNeeded(
            ActionCinematicCueProfile.CueKind kind,
            int tier,
            ActionCinematicCueProfile.CueSequence sequence)
        {
            if (kind != ActionCinematicCueProfile.CueKind.SummonFollowupHit)
            {
                return;
            }

            float duration = EstimateSequenceSeconds(sequence);
            frameDuration = Mathf.Max(frameDuration, duration);
            frameEndTime = Mathf.Max(frameEndTime, FrameClock + duration);
            totalPlayCount++;
            RecordPlayedCueKind(kind, Mathf.Max(1, tier), sequence.cueId, duration > 0f);
        }

        private float ResolveFrameWeight()
        {
            if (frameDuration <= 0f)
            {
                return 0f;
            }

            float normalized = Mathf.Clamp01(ResolveFrameSecondsRemaining() / frameDuration);
            float fadeIn = Mathf.Clamp01((1f - normalized) / 0.18f);
            float fadeOut = Mathf.Clamp01(normalized / 0.24f);
            float weight = Mathf.Min(fadeIn, fadeOut);
            return weight * weight * (3f - 2f * weight);
        }

        private float ResolveFrameSecondsRemaining()
        {
            return Mathf.Max(0f, frameEndTime - FrameClock);
        }

        private float FrameClock => useUnscaledClock ? Time.unscaledTime : Time.time;
    }
}
