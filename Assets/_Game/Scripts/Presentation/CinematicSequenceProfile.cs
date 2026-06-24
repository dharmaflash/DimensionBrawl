using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Cinematics/Cinematic Sequence Profile", fileName = "DB_CinematicSequence")]
    public sealed class CinematicSequenceProfile : ScriptableObject
    {
        public enum SequenceCategory
        {
            IntroAwakening,
            GameplayHandoff,
            QteAssist,
            UltimateCutIn,
            DangerCue,
            CombatTutorialOverlay,
            BossIntro,
            PhaseTransition,
            BreakMoment,
            DialogueReactionBeat,
            ResultBridge,
            SummonEntry,
            StoryTransition,
            StageClear,
            FailureBridge,
            SummonFollowupHit,
            SummonEmpower,
            SummonRecall
        }

        public enum ShotPurpose
        {
            NewInformation,
            CharacterAction,
            ThreatDirection,
            MechanicConnection,
            GameplayHandoff,
            EmotionChange,
            Transition
        }

        public enum CameraBlendKind
        {
            Cut,
            Ease,
            PushIn,
            PullBack,
            Reframe,
            GameplayMatch
        }

        public enum ActorRole
        {
            Inori,
            Player,
            Assist,
            Summon,
            Enemy,
            Boss,
            Environment
        }

        public enum ActorCueKind
        {
            BodyState,
            BodyTrigger,
            FaceState,
            FaceTrigger,
            WeaponAttach,
            WeaponVisibility,
            LookAt,
            ClipReferenceOnly
        }

        public enum TutorialCueKind
        {
            None,
            MaskTarget,
            ClickPrompt,
            TimedGuide,
            QtePrompt,
            WarningPrompt,
            SkillPrompt,
            UltimatePrompt
        }

        public enum GameplayReturnMode
        {
            None,
            ActionCameraController,
            MatchGameplayBackView,
            CombatHud,
            ResultUi
        }

        [Serializable]
        public struct CameraCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField] private ShotPurpose purpose;
            [SerializeField] private CameraBlendKind blendKind;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;
            [SerializeField] private Vector3 localOffset;
            [SerializeField] private float planarDirectionOffset;
            [SerializeField] private float fieldOfViewDelta;
            [SerializeField] private float cameraDistanceDelta;
            [SerializeField] private float focusHeightDelta;
            [SerializeField, Min(0f)] private float impulseScale;
            [SerializeField] private bool driveCameraPose;
            [SerializeField] private Vector3 cameraLocalPosition;
            [SerializeField] private Vector3 lookAtLocalPosition;
            [SerializeField, Min(1f)] private float fieldOfView;

            public CameraCue(
                string cueId,
                ShotPurpose purpose,
                CameraBlendKind blendKind,
                float startSeconds,
                float durationSeconds,
                Vector3 localOffset,
                float planarDirectionOffset,
                float fieldOfViewDelta,
                float cameraDistanceDelta,
                float focusHeightDelta,
                float impulseScale = 1f)
            {
                enabled = true;
                this.cueId = cueId;
                this.purpose = purpose;
                this.blendKind = blendKind;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
                this.localOffset = localOffset;
                this.planarDirectionOffset = planarDirectionOffset;
                this.fieldOfViewDelta = fieldOfViewDelta;
                this.cameraDistanceDelta = cameraDistanceDelta;
                this.focusHeightDelta = focusHeightDelta;
                this.impulseScale = Mathf.Max(0f, impulseScale);
                driveCameraPose = false;
                cameraLocalPosition = Vector3.zero;
                lookAtLocalPosition = Vector3.forward;
                fieldOfView = 0f;
            }

            public CameraCue(
                string cueId,
                ShotPurpose purpose,
                CameraBlendKind blendKind,
                float startSeconds,
                float durationSeconds,
                Vector3 localOffset,
                float planarDirectionOffset,
                float fieldOfViewDelta,
                float cameraDistanceDelta,
                float focusHeightDelta,
                Vector3 cameraLocalPosition,
                Vector3 lookAtLocalPosition,
                float fieldOfView,
                float impulseScale = 1f)
                : this(
                    cueId,
                    purpose,
                    blendKind,
                    startSeconds,
                    durationSeconds,
                    localOffset,
                    planarDirectionOffset,
                    fieldOfViewDelta,
                    cameraDistanceDelta,
                    focusHeightDelta,
                    impulseScale)
            {
                driveCameraPose = true;
                this.cameraLocalPosition = cameraLocalPosition;
                this.lookAtLocalPosition = lookAtLocalPosition;
                this.fieldOfView = Mathf.Clamp(fieldOfView, 1f, 179f);
            }

            public bool Enabled => enabled;
            public string CueId => cueId;
            public ShotPurpose Purpose => purpose;
            public CameraBlendKind BlendKind => blendKind;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public Vector3 LocalOffset => localOffset;
            public float PlanarDirectionOffset => planarDirectionOffset;
            public float FieldOfViewDelta => fieldOfViewDelta;
            public float CameraDistanceDelta => cameraDistanceDelta;
            public float FocusHeightDelta => focusHeightDelta;
            public float ImpulseScale => impulseScale > 0f ? impulseScale : 1f;
            public bool DriveCameraPose => driveCameraPose;
            public Vector3 CameraLocalPosition => cameraLocalPosition;
            public Vector3 LookAtLocalPosition => lookAtLocalPosition;
            public float FieldOfView => fieldOfView > 0f ? Mathf.Clamp(fieldOfView, 1f, 179f) : 0f;
        }

        [Serializable]
        public struct ActorCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField] private ActorRole role;
            [SerializeField] private ActorCueKind cueKind;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField] private AnimationClip clip;
            [SerializeField] private AvatarMask avatarMask;
            [SerializeField] private string animatorStateName;
            [SerializeField] private string animatorTriggerName;
            [SerializeField] private string faceStateName;
            [SerializeField] private string socketPath;
            [SerializeField] private bool requireSocket;
            [SerializeField] private RuntimeAnimatorController controllerOverride;
            [SerializeField] private bool objectActive;

            public ActorCue(
                string cueId,
                ActorRole role,
                ActorCueKind cueKind,
                float startSeconds,
                float durationSeconds,
                string animatorStateName,
                string animatorTriggerName = "",
                string faceStateName = "",
                string socketPath = "",
                bool requireSocket = false,
                AnimationClip clip = null,
                AvatarMask avatarMask = null,
                RuntimeAnimatorController controllerOverride = null,
                bool objectActive = true)
            {
                enabled = true;
                this.cueId = cueId;
                this.role = role;
                this.cueKind = cueKind;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0f, durationSeconds);
                this.clip = clip;
                this.avatarMask = avatarMask;
                this.animatorStateName = animatorStateName;
                this.animatorTriggerName = animatorTriggerName;
                this.faceStateName = faceStateName;
                this.socketPath = socketPath;
                this.requireSocket = requireSocket;
                this.controllerOverride = controllerOverride;
                this.objectActive = objectActive;
            }

            public bool Enabled => enabled;
            public string CueId => cueId;
            public ActorRole Role => role;
            public ActorCueKind CueKind => cueKind;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public AnimationClip Clip => clip;
            public AvatarMask AvatarMask => avatarMask;
            public string AnimatorStateName => animatorStateName;
            public string AnimatorTriggerName => animatorTriggerName;
            public string FaceStateName => faceStateName;
            public string SocketPath => socketPath;
            public bool RequireSocket => requireSocket;
            public RuntimeAnimatorController ControllerOverride => controllerOverride;
            public bool ObjectActive => objectActive;
        }

        [Serializable]
        public struct VfxCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField] private bool useCombatVfxCue;
            [SerializeField] private CombatVfxCueId combatVfxCueId;
            [SerializeField] private GameObject prefab;
            [SerializeField] private Vector3 localOffset;
            [SerializeField, Min(0f)] private float intensity;

            public VfxCue(
                string cueId,
                float startSeconds,
                float durationSeconds,
                CombatVfxCueId combatVfxCueId,
                float intensity = 1f)
                : this(cueId, startSeconds, durationSeconds, combatVfxCueId, Vector3.zero, intensity)
            {
            }

            public VfxCue(
                string cueId,
                float startSeconds,
                float durationSeconds,
                CombatVfxCueId combatVfxCueId,
                Vector3 localOffset,
                float intensity = 1f)
            {
                enabled = true;
                this.cueId = cueId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0f, durationSeconds);
                useCombatVfxCue = true;
                this.combatVfxCueId = combatVfxCueId;
                prefab = null;
                this.localOffset = localOffset;
                this.intensity = Mathf.Max(0f, intensity);
            }

            public bool Enabled => enabled;
            public string CueId => cueId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public bool UseCombatVfxCue => useCombatVfxCue;
            public CombatVfxCueId CombatVfxCueId => combatVfxCueId;
            public GameObject Prefab => prefab;
            public Vector3 LocalOffset => localOffset;
            public float Intensity => intensity > 0f ? intensity : 1f;
        }

        [Serializable]
        public struct TutorialCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField] private TutorialCueKind cueKind;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField] private string promptKey;
            [SerializeField] private string guideText;
            [SerializeField] private bool requireLargeReadableText;
            [SerializeField] private Vector2 screenAnchor;

            public TutorialCue(
                string cueId,
                TutorialCueKind cueKind,
                float startSeconds,
                float durationSeconds,
                string promptKey,
                string guideText,
                bool requireLargeReadableText,
                Vector2 screenAnchor)
            {
                enabled = true;
                this.cueId = cueId;
                this.cueKind = cueKind;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0f, durationSeconds);
                this.promptKey = promptKey;
                this.guideText = guideText;
                this.requireLargeReadableText = requireLargeReadableText;
                this.screenAnchor = screenAnchor;
            }

            public bool Enabled => enabled;
            public string CueId => cueId;
            public TutorialCueKind CueKind => cueKind;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public string PromptKey => promptKey;
            public string GuideText => guideText;
            public bool RequireLargeReadableText => requireLargeReadableText;
            public Vector2 ScreenAnchor => screenAnchor;
        }

        [Serializable]
        public struct GameplayHandoffCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private GameplayReturnMode returnMode;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField] private string targetId;
            [SerializeField, Min(0f)] private float inputReleaseDelaySeconds;
            [SerializeField] private bool restoreHud;
            [SerializeField] private bool restoreTimeScale;
            [SerializeField] private bool restoreCamera;

            public GameplayHandoffCue(
                GameplayReturnMode returnMode,
                float startSeconds,
                string targetId,
                float inputReleaseDelaySeconds = 0f,
                bool restoreHud = true,
                bool restoreTimeScale = true,
                bool restoreCamera = true)
            {
                enabled = returnMode != GameplayReturnMode.None;
                this.returnMode = returnMode;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.targetId = targetId;
                this.inputReleaseDelaySeconds = Mathf.Max(0f, inputReleaseDelaySeconds);
                this.restoreHud = restoreHud;
                this.restoreTimeScale = restoreTimeScale;
                this.restoreCamera = restoreCamera;
            }

            public bool Enabled => enabled;
            public GameplayReturnMode ReturnMode => returnMode;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public string TargetId => targetId;
            public float InputReleaseDelaySeconds => Mathf.Max(0f, inputReleaseDelaySeconds);
            public bool RestoreHud => restoreHud;
            public bool RestoreTimeScale => restoreTimeScale;
            public bool RestoreCamera => restoreCamera;
        }

        [Header("Identity")]
        [SerializeField] private string sequenceId = "cinematic_sequence";
        [SerializeField] private string displayName = "Cinematic Sequence";
        [SerializeField] private SequenceCategory category;
        [SerializeField, TextArea(2, 4)] private string reviewerIntent;
        [SerializeField, Min(0.01f)] private float authoredDurationSeconds = 1f;
        [SerializeField, Min(0)] private int priority = 50;

        [Header("Playback")]
        [SerializeField] private bool lockMovement = true;
        [SerializeField] private bool lockInput = true;
        [SerializeField] private bool hideHud;
        [SerializeField] private bool canSkip;
        [SerializeField] private bool useUnscaledClock = true;

        [Header("Cues")]
        [SerializeField] private CameraCue[] cameraCues = Array.Empty<CameraCue>();
        [SerializeField] private ActorCue[] actorCues = Array.Empty<ActorCue>();
        [SerializeField] private VfxCue[] vfxCues = Array.Empty<VfxCue>();
        [SerializeField] private TutorialCue[] tutorialCues = Array.Empty<TutorialCue>();
        [SerializeField] private GameplayHandoffCue gameplayHandoff;

        public string SequenceId => sequenceId;
        public string DisplayName => displayName;
        public SequenceCategory Category => category;
        public string ReviewerIntent => reviewerIntent;
        public float AuthoredDurationSeconds => Mathf.Max(0.01f, authoredDurationSeconds);
        public int Priority => priority;
        public bool LockMovement => lockMovement;
        public bool LockInput => lockInput;
        public bool HideHud => hideHud;
        public bool CanSkip => canSkip;
        public bool UseUnscaledClock => useUnscaledClock;
        public CameraCue[] CameraCues => cameraCues ?? Array.Empty<CameraCue>();
        public ActorCue[] ActorCues => actorCues ?? Array.Empty<ActorCue>();
        public VfxCue[] VfxCues => vfxCues ?? Array.Empty<VfxCue>();
        public TutorialCue[] TutorialCues => tutorialCues ?? Array.Empty<TutorialCue>();
        public GameplayHandoffCue GameplayHandoff => gameplayHandoff;

        public float EstimatedDurationSeconds
        {
            get
            {
                float duration = authoredDurationSeconds;
                CameraCue[] resolvedCameraCues = CameraCues;
                for (int i = 0; i < resolvedCameraCues.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedCameraCues[i].EndSeconds);
                }

                ActorCue[] resolvedActorCues = ActorCues;
                for (int i = 0; i < resolvedActorCues.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedActorCues[i].EndSeconds);
                }

                VfxCue[] resolvedVfxCues = VfxCues;
                for (int i = 0; i < resolvedVfxCues.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedVfxCues[i].EndSeconds);
                }

                TutorialCue[] resolvedTutorialCues = TutorialCues;
                for (int i = 0; i < resolvedTutorialCues.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedTutorialCues[i].EndSeconds);
                }

                if (gameplayHandoff.Enabled)
                {
                    duration = Mathf.Max(duration, gameplayHandoff.StartSeconds + gameplayHandoff.InputReleaseDelaySeconds);
                }

                return Mathf.Max(0.01f, duration);
            }
        }

        public void Configure(
            string newSequenceId,
            string newDisplayName,
            SequenceCategory newCategory,
            string newReviewerIntent,
            float newAuthoredDurationSeconds,
            int newPriority,
            bool newLockMovement,
            bool newLockInput,
            bool newHideHud,
            bool newCanSkip,
            bool newUseUnscaledClock,
            CameraCue[] newCameraCues,
            ActorCue[] newActorCues,
            VfxCue[] newVfxCues,
            TutorialCue[] newTutorialCues,
            GameplayHandoffCue newGameplayHandoff)
        {
            sequenceId = newSequenceId;
            displayName = newDisplayName;
            category = newCategory;
            reviewerIntent = newReviewerIntent;
            authoredDurationSeconds = Mathf.Max(0.01f, newAuthoredDurationSeconds);
            priority = newPriority;
            lockMovement = newLockMovement;
            lockInput = newLockInput;
            hideHud = newHideHud;
            canSkip = newCanSkip;
            useUnscaledClock = newUseUnscaledClock;
            cameraCues = newCameraCues ?? Array.Empty<CameraCue>();
            actorCues = newActorCues ?? Array.Empty<ActorCue>();
            vfxCues = newVfxCues ?? Array.Empty<VfxCue>();
            tutorialCues = newTutorialCues ?? Array.Empty<TutorialCue>();
            gameplayHandoff = newGameplayHandoff;
        }

        public void CollectValidationIssues(List<string> issues)
        {
            if (issues == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sequenceId))
            {
                issues.Add($"{name}: sequence id is empty.");
            }

            if (CameraCues.Length == 0)
            {
                issues.Add($"{name}: no camera cues are authored.");
            }

            if (Category != SequenceCategory.CombatTutorialOverlay && ActorCues.Length == 0)
            {
                issues.Add($"{name}: no actor cues are authored.");
            }

            if (gameplayHandoff.Enabled && string.IsNullOrWhiteSpace(gameplayHandoff.TargetId))
            {
                issues.Add($"{name}: gameplay handoff has no target id.");
            }

            CameraCue[] resolvedCameraCues = CameraCues;
            for (int i = 0; i < resolvedCameraCues.Length; i++)
            {
                CameraCue cue = resolvedCameraCues[i];
                if (!cue.Enabled)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(cue.CueId))
                {
                    issues.Add($"{name}: camera cue {i} has no cue id.");
                }

                if (cue.DurationSeconds <= 0f)
                {
                    issues.Add($"{name}: camera cue {cue.CueId} has no duration.");
                }

                if (cue.DriveCameraPose)
                {
                    if ((cue.CameraLocalPosition - cue.LookAtLocalPosition).sqrMagnitude < 0.0025f)
                    {
                        issues.Add($"{name}: camera cue {cue.CueId} has an unsafe shot pose because camera and look-at are too close.");
                    }

                    if (cue.FieldOfView <= 0f)
                    {
                        issues.Add($"{name}: camera cue {cue.CueId} drives camera pose without an authored field of view.");
                    }
                }
            }

            ActorCue[] resolvedActorCues = ActorCues;
            for (int i = 0; i < resolvedActorCues.Length; i++)
            {
                ActorCue cue = resolvedActorCues[i];
                if (!cue.Enabled)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(cue.CueId))
                {
                    issues.Add($"{name}: actor cue {i} has no cue id.");
                }

                if ((cue.CueKind == ActorCueKind.BodyState || cue.CueKind == ActorCueKind.FaceState)
                    && string.IsNullOrWhiteSpace(cue.AnimatorStateName)
                    && string.IsNullOrWhiteSpace(cue.FaceStateName))
                {
                    issues.Add($"{name}: actor cue {cue.CueId} has no state name.");
                }

                if (cue.CueKind == ActorCueKind.WeaponVisibility
                    && cue.Role == ActorRole.Inori
                    && string.IsNullOrWhiteSpace(cue.SocketPath))
                {
                    issues.Add($"{name}: actor cue {cue.CueId} changes weapon visibility but has no object path.");
                }
            }
        }
    }
}
