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
        private int totalPlayCount;
        private int totalSignalCount;
        private int animatorTriggerRequestCount;
        private int vfxCueRequestCount;
        private ActionCinematicCueProfile.CueKind lastPlayedKind;
        private int lastPlayedTier;
        private string lastPlayedCueId;
        private string lastSignalId;
        private string lastAnimatorTrigger;
        private CombatVfxCueId lastVfxCueId;
        private float frameTimer;
        private float frameDuration;

        public ActionCinematicCueProfile CueProfile => cueProfile;
        public ActionCameraController CameraController => cameraController;
        public Transform CueSpace => cueSpace;
        public bool DrawCinematicBars => drawCinematicBars;
        public bool HasActiveFrameOverlay => frameTimer > 0f;
        public bool HasActiveMovementLock => movementLockActive;
        public bool HasActiveInputLock => inputLockActive;
        public bool IsPlaying => activeRoutine != null;
        public int TotalPlayCount => totalPlayCount;
        public int TotalSignalCount => totalSignalCount;
        public int AnimatorTriggerRequestCount => animatorTriggerRequestCount;
        public int VfxCueRequestCount => vfxCueRequestCount;
        public ActionCinematicCueProfile.CueKind LastPlayedKind => lastPlayedKind;
        public int LastPlayedTier => lastPlayedTier;
        public string LastPlayedCueId => lastPlayedCueId;
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
            frameTimer = 0f;
            frameDuration = 0f;
        }

        private void Update()
        {
            if (frameTimer <= 0f)
            {
                return;
            }

            float deltaTime = useUnscaledClock ? Time.unscaledDeltaTime : Time.deltaTime;
            frameTimer = Mathf.Max(0f, frameTimer - Mathf.Max(0f, deltaTime));
        }

        private void OnGUI()
        {
            if (!drawCinematicBars || frameTimer <= 0f)
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
            if (cueProfile == null
                || cameraController == null
                || !cueProfile.TryGetSequence(kind, out ActionCinematicCueProfile.CueSequence sequence))
            {
                return false;
            }

            if (activeRoutine != null)
            {
                if (sequence.priority < activePriority || (!activeCanBeInterrupted && sequence.priority <= activePriority))
                {
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
            frameTimer = frameDuration;
            activeRoutine = StartCoroutine(PlaySequence(sequence, lastPlayedTier, planarDirection));
            return true;
        }

        private IEnumerator PlaySequence(
            ActionCinematicCueProfile.CueSequence sequence,
            int tier,
            Vector3 planarDirection)
        {
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
                ActionCinematicCueProfile.CameraShot shot = sequence.shots[i];
                if (shot.enabled)
                {
                    RequestShot(shot, tier, planarDirection);
                }

                float waitSeconds = Mathf.Max(0.01f, shot.durationSeconds + Mathf.Max(0f, shot.pauseAfterSeconds));
                float elapsed = 0f;
                while (elapsed < waitSeconds)
                {
                    float deltaTime = useUnscaledClock ? Time.unscaledDeltaTime : Time.deltaTime;
                    elapsed += deltaTime;
                    sequenceElapsed += deltaTime;
                    timeScaleTimer = TickTimeScaleTimer(timeScaleTimer, deltaTime);
                    movementLockTimer = TickMovementLockTimer(movementLockTimer, deltaTime);
                    inputLockTimer = TickInputLockTimer(inputLockTimer, deltaTime);
                    DispatchDueSignals(sequence, signalPlayed, sequenceElapsed, tier, planarDirection);
                    yield return null;
                }
            }

            RestoreCinematicState();
            activeRoutine = null;
            activePriority = 0;
            activeCanBeInterrupted = true;
        }

        private void RequestShot(ActionCinematicCueProfile.CameraShot shot, int tier, Vector3 planarDirection)
        {
            Transform space = cueSpace != null ? cueSpace : transform;
            Vector3 offset = space.TransformDirection(shot.localOffset);
            Vector3 direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                offset += direction.normalized * shot.planarDirectionOffset;
            }

            float tierWeight = Mathf.Clamp01((Mathf.Max(1, tier) - 1) / 2f);
            float scale = Mathf.Lerp(1f, Mathf.Max(0f, shot.tierScale), tierWeight);
            cameraController.RequestCue(
                offset * scale,
                shot.durationSeconds,
                shot.fieldOfViewDelta * scale,
                shot.cameraDistanceDelta * scale,
                shot.focusHeightDelta * scale);
        }

        private Vector3 ResolveDefaultDirection()
        {
            Transform space = cueSpace != null ? cueSpace : transform;
            Vector3 forward = Vector3.ProjectOnPlane(space.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private void ApplyTimeScale(float timeScale)
        {
            if (!hasStoredTimeScale)
            {
                storedTimeScale = Time.timeScale;
                hasStoredTimeScale = true;
            }

            Time.timeScale = Mathf.Clamp(timeScale, 0.05f, 1f);
        }

        private float TickTimeScaleTimer(float timer, float deltaTime)
        {
            if (!hasStoredTimeScale || timer <= 0f)
            {
                return timer;
            }

            timer = Mathf.Max(0f, timer - Mathf.Max(0f, deltaTime));
            if (timer <= 0f)
            {
                RestoreTimeScale();
            }

            return timer;
        }

        private void RestoreTimeScale()
        {
            if (!hasStoredTimeScale)
            {
                return;
            }

            Time.timeScale = storedTimeScale;
            hasStoredTimeScale = false;
        }

        private float ResolveFrameWeight()
        {
            if (frameDuration <= 0f)
            {
                return 0f;
            }

            float normalized = Mathf.Clamp01(frameTimer / frameDuration);
            float fadeIn = Mathf.Clamp01((1f - normalized) / 0.18f);
            float fadeOut = Mathf.Clamp01(normalized / 0.24f);
            float weight = Mathf.Min(fadeIn, fadeOut);
            return weight * weight * (3f - 2f * weight);
        }

        private static float EstimateSequenceSeconds(ActionCinematicCueProfile.CueSequence sequence)
        {
            if (sequence.shots == null || sequence.shots.Length == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < sequence.shots.Length; i++)
            {
                ActionCinematicCueProfile.CameraShot shot = sequence.shots[i];
                total += Mathf.Max(0.01f, shot.durationSeconds + Mathf.Max(0f, shot.pauseAfterSeconds));
            }

            return total;
        }
    }
}
