using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
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
            SummonRecall,
            BossSummonPressure
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
        public struct SourceCameraAnimationCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField] private AnimationClip clip;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0f)] private float clipInSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;

            public SourceCameraAnimationCue(
                string cueId,
                AnimationClip clip,
                float startSeconds,
                float clipInSeconds,
                float durationSeconds)
            {
                enabled = clip != null;
                this.cueId = cueId;
                this.clip = clip;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.clipInSeconds = Mathf.Max(0f, clipInSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
            }

            public bool Enabled => enabled && clip != null;
            public string CueId => cueId;
            public AnimationClip Clip => clip;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float ClipInSeconds => Mathf.Max(0f, clipInSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
        }

        [Serializable]
        public struct SourceActorAnimationCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField] private AnimationClip clip;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0f)] private float clipInSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;

            public SourceActorAnimationCue(
                string cueId,
                AnimationClip clip,
                float startSeconds,
                float clipInSeconds,
                float durationSeconds)
            {
                enabled = clip != null;
                this.cueId = cueId;
                this.clip = clip;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.clipInSeconds = Mathf.Max(0f, clipInSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
            }

            public bool Enabled => enabled && clip != null;
            public string CueId => cueId;
            public AnimationClip Clip => clip;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float ClipInSeconds => Mathf.Max(0f, clipInSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
        }

        [Serializable]
        public struct SourceActorGradeCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;
            [SerializeField] private Color startColor;
            [SerializeField] private Color endColor;

            public SourceActorGradeCue(
                string cueId,
                float startSeconds,
                float durationSeconds,
                Color startColor,
                Color endColor)
            {
                enabled = true;
                this.cueId = cueId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
                this.startColor = startColor;
                this.endColor = endColor;
            }

            public bool Enabled => enabled;
            public string CueId => cueId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public Color StartColor => startColor;
            public Color EndColor => endColor;

            public Color Evaluate(float elapsedSeconds)
            {
                if (elapsedSeconds <= StartSeconds)
                {
                    return StartColor;
                }

                if (elapsedSeconds >= EndSeconds)
                {
                    return EndColor;
                }

                float t = Mathf.Clamp01((elapsedSeconds - StartSeconds) / DurationSeconds);
                return Color.Lerp(StartColor, EndColor, t);
            }
        }

        [Serializable]
        public struct ScreenFadeCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private string cueId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;
            [SerializeField] private Color color;
            [SerializeField, Range(0f, 1f)] private float startAlpha;
            [SerializeField, Range(0f, 1f)] private float endAlpha;

            public ScreenFadeCue(
                string cueId,
                float startSeconds,
                float durationSeconds,
                Color color,
                float startAlpha,
                float endAlpha)
            {
                enabled = true;
                this.cueId = cueId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
                this.color = color;
                this.startAlpha = Mathf.Clamp01(startAlpha);
                this.endAlpha = Mathf.Clamp01(endAlpha);
            }

            public bool Enabled => enabled;
            public string CueId => cueId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public Color Color => color;
            public float StartAlpha => Mathf.Clamp01(startAlpha);
            public float EndAlpha => Mathf.Clamp01(endAlpha);

            public float EvaluateAlpha(float elapsedSeconds)
            {
                if (elapsedSeconds <= StartSeconds)
                {
                    return StartAlpha;
                }

                if (elapsedSeconds >= EndSeconds)
                {
                    return EndAlpha;
                }

                float t = Mathf.Clamp01((elapsedSeconds - StartSeconds) / DurationSeconds);
                return Mathf.Lerp(StartAlpha, EndAlpha, t);
            }
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

        [Header("Stage Context")]
        [SerializeField] private bool requiresStageDefinition;
        [SerializeField] private StageDefinitionProfile stageDefinition;
        [SerializeField] private string stageHandoffId;
        [SerializeField] private string stageAnchorId;
        [SerializeField] private string stageRuntimeStateId;
        [TextArea, SerializeField] private string stageContextNote;

        [Header("Cues")]
        [SerializeField] private SourceCameraAnimationCue sourceCameraAnimation;
        [SerializeField] private SourceActorAnimationCue sourceActorAnimation;
        [SerializeField] private SourceCameraAnimationCue[] sourceCameraAnimations =
            Array.Empty<SourceCameraAnimationCue>();
        [SerializeField] private SourceActorAnimationCue[] sourceActorAnimations =
            Array.Empty<SourceActorAnimationCue>();
        [SerializeField] private SourceActorGradeCue[] sourceActorGrades =
            Array.Empty<SourceActorGradeCue>();
        [SerializeField] private ScreenFadeCue[] screenFadeCues =
            Array.Empty<ScreenFadeCue>();
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
        public bool RequiresStageDefinition => requiresStageDefinition;
        public StageDefinitionProfile StageDefinition => stageDefinition;
        public string StageHandoffId => stageHandoffId;
        public string StageAnchorId => stageAnchorId;
        public string StageRuntimeStateId => stageRuntimeStateId;
        public string StageContextNote => stageContextNote;
        public bool HasStageContext =>
            stageDefinition != null
            && !string.IsNullOrWhiteSpace(stageHandoffId)
            && !string.IsNullOrWhiteSpace(stageAnchorId);
        public SourceCameraAnimationCue SourceCameraAnimation => ResolveFirstSourceCameraAnimation();
        public SourceActorAnimationCue SourceActorAnimation => ResolveFirstSourceActorAnimation();
        public SourceCameraAnimationCue[] SourceCameraAnimations => ResolveSourceCameraAnimations();
        public SourceActorAnimationCue[] SourceActorAnimations => ResolveSourceActorAnimations();
        public SourceActorGradeCue[] SourceActorGrades => ResolveSourceActorGrades();
        public ScreenFadeCue[] ScreenFadeCues => ResolveScreenFadeCues();
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
                SourceCameraAnimationCue[] resolvedSourceCameraAnimations = SourceCameraAnimations;
                for (int i = 0; i < resolvedSourceCameraAnimations.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedSourceCameraAnimations[i].EndSeconds);
                }

                SourceActorAnimationCue[] resolvedSourceActorAnimations = SourceActorAnimations;
                for (int i = 0; i < resolvedSourceActorAnimations.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedSourceActorAnimations[i].EndSeconds);
                }

                SourceActorGradeCue[] resolvedSourceActorGrades = SourceActorGrades;
                for (int i = 0; i < resolvedSourceActorGrades.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedSourceActorGrades[i].EndSeconds);
                }

                ScreenFadeCue[] resolvedScreenFadeCues = ScreenFadeCues;
                for (int i = 0; i < resolvedScreenFadeCues.Length; i++)
                {
                    duration = Mathf.Max(duration, resolvedScreenFadeCues[i].EndSeconds);
                }

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

        public void ConfigureSourceCameraAnimation(SourceCameraAnimationCue newSourceCameraAnimation)
        {
            sourceCameraAnimation = newSourceCameraAnimation;
            sourceCameraAnimations = newSourceCameraAnimation.Enabled
                ? new[] { newSourceCameraAnimation }
                : Array.Empty<SourceCameraAnimationCue>();
        }

        public void ConfigureSourceActorAnimation(SourceActorAnimationCue newSourceActorAnimation)
        {
            sourceActorAnimation = newSourceActorAnimation;
            sourceActorAnimations = newSourceActorAnimation.Enabled
                ? new[] { newSourceActorAnimation }
                : Array.Empty<SourceActorAnimationCue>();
        }

        public void ConfigureSourceCameraAnimations(SourceCameraAnimationCue[] newSourceCameraAnimations)
        {
            sourceCameraAnimations = SanitizeSourceCameraAnimations(newSourceCameraAnimations);
            sourceCameraAnimation = sourceCameraAnimations.Length > 0
                ? sourceCameraAnimations[0]
                : default;
        }

        public void ConfigureSourceActorAnimations(SourceActorAnimationCue[] newSourceActorAnimations)
        {
            sourceActorAnimations = SanitizeSourceActorAnimations(newSourceActorAnimations);
            sourceActorAnimation = sourceActorAnimations.Length > 0
                ? sourceActorAnimations[0]
                : default;
        }

        public void ConfigureSourceActorGrades(SourceActorGradeCue[] newSourceActorGrades)
        {
            sourceActorGrades = SanitizeSourceActorGrades(newSourceActorGrades);
        }

        public void ConfigureScreenFades(ScreenFadeCue[] newScreenFadeCues)
        {
            screenFadeCues = SanitizeScreenFadeCues(newScreenFadeCues);
        }

        public void ConfigureStageContext(
            StageDefinitionProfile newStageDefinition,
            string newStageHandoffId,
            string newStageAnchorId,
            string newStageRuntimeStateId,
            string newStageContextNote,
            bool newRequiresStageDefinition = true)
        {
            stageDefinition = newStageDefinition;
            stageHandoffId = newStageHandoffId ?? string.Empty;
            stageAnchorId = newStageAnchorId ?? string.Empty;
            stageRuntimeStateId = newStageRuntimeStateId ?? string.Empty;
            stageContextNote = newStageContextNote ?? string.Empty;
            requiresStageDefinition = newRequiresStageDefinition;
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

            SourceCameraAnimationCue[] resolvedSourceCameraAnimations = SourceCameraAnimations;
            SourceActorAnimationCue[] resolvedSourceActorAnimations = SourceActorAnimations;
            if (CameraCues.Length == 0 && resolvedSourceCameraAnimations.Length == 0)
            {
                issues.Add($"{name}: no camera cues are authored.");
            }

            for (int i = 0; i < resolvedSourceCameraAnimations.Length; i++)
            {
                if (resolvedSourceCameraAnimations[i].DurationSeconds <= 0f)
                {
                    issues.Add($"{name}: source camera animation {i} has no duration.");
                }

                if (string.IsNullOrWhiteSpace(resolvedSourceCameraAnimations[i].CueId))
                {
                    issues.Add($"{name}: source camera animation {i} has no cue id.");
                }
            }

            for (int i = 0; i < resolvedSourceActorAnimations.Length; i++)
            {
                if (resolvedSourceActorAnimations[i].DurationSeconds <= 0f)
                {
                    issues.Add($"{name}: source actor animation {i} has no duration.");
                }

                if (string.IsNullOrWhiteSpace(resolvedSourceActorAnimations[i].CueId))
                {
                    issues.Add($"{name}: source actor animation {i} has no cue id.");
                }
            }

            if (Category != SequenceCategory.CombatTutorialOverlay && ActorCues.Length == 0)
            {
                issues.Add($"{name}: no actor cues are authored.");
            }

            if (gameplayHandoff.Enabled && string.IsNullOrWhiteSpace(gameplayHandoff.TargetId))
            {
                issues.Add($"{name}: gameplay handoff has no target id.");
            }

            if (requiresStageDefinition)
            {
                if (stageDefinition == null)
                {
                    issues.Add($"{name}: requires a stage definition but none is assigned.");
                }

                if (string.IsNullOrWhiteSpace(stageHandoffId))
                {
                    issues.Add($"{name}: requires a stage handoff id but none is assigned.");
                }

                if (string.IsNullOrWhiteSpace(stageAnchorId))
                {
                    issues.Add($"{name}: requires a stage anchor id but none is assigned.");
                }
            }
            else if (stageDefinition != null
                && (string.IsNullOrWhiteSpace(stageHandoffId) || string.IsNullOrWhiteSpace(stageAnchorId)))
            {
                issues.Add($"{name}: has a stage definition but no complete stage handoff/anchor context.");
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

        private SourceCameraAnimationCue ResolveFirstSourceCameraAnimation() =>
            SourceCameraAnimations.Length > 0 ? SourceCameraAnimations[0] : default;

        private SourceActorAnimationCue ResolveFirstSourceActorAnimation() =>
            SourceActorAnimations.Length > 0 ? SourceActorAnimations[0] : default;

        private SourceCameraAnimationCue[] ResolveSourceCameraAnimations()
        {
            if (sourceCameraAnimations != null && sourceCameraAnimations.Length > 0)
            {
                return sourceCameraAnimations;
            }

            return sourceCameraAnimation.Enabled
                ? new[] { sourceCameraAnimation }
                : Array.Empty<SourceCameraAnimationCue>();
        }

        private SourceActorAnimationCue[] ResolveSourceActorAnimations()
        {
            if (sourceActorAnimations != null && sourceActorAnimations.Length > 0)
            {
                return sourceActorAnimations;
            }

            return sourceActorAnimation.Enabled
                ? new[] { sourceActorAnimation }
                : Array.Empty<SourceActorAnimationCue>();
        }

        private SourceActorGradeCue[] ResolveSourceActorGrades()
        {
            return sourceActorGrades ?? Array.Empty<SourceActorGradeCue>();
        }

        private ScreenFadeCue[] ResolveScreenFadeCues()
        {
            return screenFadeCues ?? Array.Empty<ScreenFadeCue>();
        }

        private static SourceCameraAnimationCue[] SanitizeSourceCameraAnimations(SourceCameraAnimationCue[] cues)
        {
            if (cues == null || cues.Length == 0)
            {
                return Array.Empty<SourceCameraAnimationCue>();
            }

            List<SourceCameraAnimationCue> results = new List<SourceCameraAnimationCue>(cues.Length);
            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].Enabled)
                {
                    results.Add(cues[i]);
                }
            }

            return results.ToArray();
        }

        private static SourceActorAnimationCue[] SanitizeSourceActorAnimations(SourceActorAnimationCue[] cues)
        {
            if (cues == null || cues.Length == 0)
            {
                return Array.Empty<SourceActorAnimationCue>();
            }

            List<SourceActorAnimationCue> results = new List<SourceActorAnimationCue>(cues.Length);
            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].Enabled)
                {
                    results.Add(cues[i]);
                }
            }

            return results.ToArray();
        }

        private static SourceActorGradeCue[] SanitizeSourceActorGrades(SourceActorGradeCue[] cues)
        {
            if (cues == null || cues.Length == 0)
            {
                return Array.Empty<SourceActorGradeCue>();
            }

            List<SourceActorGradeCue> results = new List<SourceActorGradeCue>(cues.Length);
            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].Enabled)
                {
                    results.Add(cues[i]);
                }
            }

            return results.ToArray();
        }

        private static ScreenFadeCue[] SanitizeScreenFadeCues(ScreenFadeCue[] cues)
        {
            if (cues == null || cues.Length == 0)
            {
                return Array.Empty<ScreenFadeCue>();
            }

            List<ScreenFadeCue> results = new List<ScreenFadeCue>(cues.Length);
            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].Enabled)
                {
                    results.Add(cues[i]);
                }
            }

            return results.ToArray();
        }
    }
}
